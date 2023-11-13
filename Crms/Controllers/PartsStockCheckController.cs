using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using Crms.Models;

//Add 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
using System.Xml.Linq;


namespace Crms.Controllers
{
    /// <summary>
    /// 部品仕掛在庫検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsStockCheckController : Controller
    {

        private static readonly string FORM_NAME = "部品在庫確認";                        // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "部品在庫確認検索";             // 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";             // 処理名(Excel出力)   //Add 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsStockCheckController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 当月のステータス一覧を表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();

            //初期値の設定

            //対象年月(デフォルトは当月)
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            //表示単位(デフォルトは「倉庫毎」)
            form["DispUnit"] = "0";
            form["DefaultDispUnit"] = form["DispUnit"];

            return Criteria(form);
        }

        /// <summary>
        /// 指定月のステータス一覧を表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            ActionResult ret = new EmptyResult();       //2016/07/12 arc yano #3599

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            TotalPartsStockCheck list = new TotalPartsStockCheck();

            CodeDao dao = new CodeDao(db);

            //入力値チェック
            DateTime targetDate = new DateTime(int.Parse(dao.GetYear(false, form["targetDateY"]).Name), int.Parse(dao.GetMonth(false, form["targetDateM"]).Name), 1);

            DateTime thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            if (targetDate > thisMonth)
            {
                ModelState.AddModelError("targetDateY", "現在より未来の対象年月は選択できません");
                ModelState.AddModelError("targetDateM", "");

                SetDataComponent(form);

                return View("PartsStockCheckCriteria", list);
            }


            switch (form["RequestFlag"])
            {
                case "1": //Excel出力

                    ret = Download(form);

                    break;

                default: //検索処理

                    // 検索結果を取得
                    list = GetSearchResultList(form);
                    //画面設定
                    SetDataComponent(form);

                    ret = View("PartsStockCheckCriteria", list);
                    break;
            }



            // 検索結果を取得
            list = GetSearchResultList(form);

            //画面設定
            SetDataComponent(form);
            
            /*
            CodeDao dao = new CodeDao(db);
            DateTime targetMonth = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name);
            */
            
            // 部品在庫確認画面の表示
            return ret;
        }

        /// <summary>
        /// 受払表検索
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>受払検索結果</returns>
        /// <history>
        /// 2019/08/26 yano #4004 【部品在庫確認】対象年月のパラメータ誤り
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/03/20 arc yano 部品在庫確認データの取得先を変更
        /// </history>
        private TotalPartsStockCheck GetSearchResultList(FormCollection form)
        {
            //データアクセス
            PartsStockCheckDao PartsStockCheckDao = new PartsStockCheckDao(db);

            CodeDao dao = new CodeDao(db);

            DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる       

            string strTargetDate = string.Format("{0:yyyyMMdd}", TargetDate);

            TargetDate = TargetDate.Date; //YYYY/MM/ddのみのデータに変換する(時間を抜く)

            ViewData["TargetDate"] = TargetDate;        //vieDataに保存

            string inventoryStatusParts = "";           //棚卸ステータス
            string inventoryStatusPartsBalance = "";    //受け払いステータス

            InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetByKey(strTargetDate);

            if (rec != null)
            {
                inventoryStatusParts = rec.InventoryStatus;
            }

            //Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④
            if (!string.IsNullOrEmpty(inventoryStatusParts))
            {
                //確定の場合でも計算中の場合（日次バッチ動作前）は結果が表示されないため「確定（計算中）」にする
                if (inventoryStatusParts == "002"){

                    InventoryMonthControlPartsBalance BalanceRec = new InventoryMonthControlPartsBalanceDao(db).GetByKey(strTargetDate);
                    if (BalanceRec != null)
                    {
                        inventoryStatusPartsBalance = BalanceRec.InventoryStatus;
                    }

                    if (!string.IsNullOrEmpty(inventoryStatusPartsBalance) && inventoryStatusPartsBalance == "002"){
                        
                        ViewData["InventoryStatusParts"] = dao.GetCodeNameByKey("011", inventoryStatusPartsBalance, false).Name;
                        ViewData["inventoryStatusPartsBalance"] = inventoryStatusPartsBalance;

                    }else if (inventoryStatusPartsBalance == "001"){

                        //CategoryCode：011(在庫棚卸ステータス)　Code：003( 確定（計算中）)
                        ViewData["InventoryStatusParts"] = dao.GetCodeNameByKey("011", "003", false).Name;
                    }

                }else{
                    ViewData["InventoryStatusParts"] = dao.GetCodeNameByKey("011", inventoryStatusParts, false).Name;
                }
            }

            //検索条件の設定
            //Mod 2019/08/26 yano #4004
            int targetDateY = TargetDate.Year;   //処理対象年
            int targetDateM = TargetDate.Month;  //処理対象月
            
            //int targetDateY = int.Parse(form["TargetDateY"]);   //処理対象年
            //int targetDateM = int.Parse(form["TargetDateM"]);   //処理対象月
            int summaryMode = int.Parse(form["DispUnit"]);      //表示単位
            //string departmentCode = form["DepartmentCode"];   //部門コード
            string partsNumber = form["PartsNumber"];           //部品番号
            string partsNameJp = form["PartsNameJp"];           //部品名称
            string warehouseCode = form["WarehouseCode"];       //倉庫コード     //2016/08/13 arc yano #3596

            //検索結果を取得する
            TotalPartsStockCheck ret = new TotalPartsStockCheck();

            //ret = PartsStockCheckDao.GetPartsBalance(targetDateY, targetDateM, summaryMode, departmentCode, partsNumber, partsNameJp);
            ret = PartsStockCheckDao.GetPartsBalance(targetDateY, targetDateM, summaryMode, warehouseCode, partsNumber, partsNameJp);  //Mod 2016/08/13 arc yano #3596
  
            //ページング対応
            ret.plist = new PaginatedList<PartsBalance>(ret.list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return ret;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void SetDataComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);

            //Mod 2015/03/20 arc yano 部品在庫確認 レイアウト変更
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];
            ViewData["DefaultDispUnit"] = form["DefaultDispUnit"];
            ViewData["DispUnit"] = form["DispUnit"];

            //Mod 2016/08/13 arc yano #3596
            //ViewData["DepartmentCode"] = form["DepartmentCode"];
            //Department dep = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"] == null ? "" : ViewData["DepartmentCode"].ToString());
            //ViewData["DepartmentName"] = dep == null ? "" : dep.DepartmentName;
            
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            Warehouse wh = new WarehouseDao(db).GetByKey(ViewData["WarehouseCode"] == null ? "" : ViewData["WarehouseCode"].ToString());
            
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
        }
        #endregion

        //Add 2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加
        #region Excel出力処理
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private ActionResult Download(FormCollection form)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //検索結果
            TotalPartsStockCheck list = new TotalPartsStockCheck();


            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "PartsBalance" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsBalance"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsBalance"];
          
            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);
                return View("PartsStockCheckCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("PartsStockCheckCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //検索条件行でも参照するため、あらかじめ検索結果を取得しておく
            TotalPartsStockCheck data = GetSearchResultList(form);

            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(form, data);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
         
            //------------------------
            //合計金額の出力
            //------------------------
            configLine.SetPos[0] = "E4";

            List<ExcelPartsBalanceTotal> totallist = new List<ExcelPartsBalanceTotal>();
            
            ExcelPartsBalanceTotal total = new ExcelPartsBalanceTotal();

            //エクセル出力用に変換
            total = MakeExcelList(data, form);

            totallist.Add(total);

            //データ設定
            ret = dExport.SetData<ExcelPartsBalanceTotal, ExcelPartsBalanceTotal>(ref excelFile, totallist, configLine);

            //------------------------
            //明細行
            //------------------------
            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            List<ExcelPartsBalance> elist = new List<ExcelPartsBalance>();

            elist = MakeExcelList(data.list, form);

            //データ設定
            ret = dExport.SetData<ExcelPartsBalance, ExcelPartsBalance>(ref excelFile, elist, configLine);

           
            excelData = excelFile.GetAsByteArray();


            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private List<ExcelPartsBalance> MakeExcelList(List<PartsBalance> list, FormCollection form)
        {
            List<ExcelPartsBalance> elist = new List<ExcelPartsBalance>();

            //対象年月の指定がある場合は実棚を数量に
        
            elist = list.Select(x =>
                    new ExcelPartsBalance()
                    {
                        //DepartmentCode = x.DepartmentCode,                                                                                                                                                    //部門コード
                        //DepartmentName = x.DepartmentName,                                                                                                                                                    //部門名
                        WarehouseCode = x.WarehouseCode,                                                                                                                                                        //倉庫コード
                        WarehouseName = x.WarehouseName,                                                                                                                                                        //倉庫名
                        PartsNumber = x.PartsNumber,                                                                                                                                                            //部品番号
                        PartsNameJp = x.PartsNameJp,                                                                                                                                                            //部品名
                        PreCost = string.Format("{0:N0}", x.PreCost),                                                                                                                                           //前月末単価
                        PreQuantity = string.Format("{0:N0}", x.PreQuantity),                                                                                                                                   //数量(月初在庫)
                        PreAmount = string.Format("{0:N0}", x.PreAmount),                                                                                                                                       //金額(月初在庫)
                        PurchaseQuantity = string.Format("{0:N0}", x.PurchaseQuantity),                                                                                                                         //数量(当月仕入)
                        PurchaseAmount = string.Format("{0:N0}", x.PurchaseAmount),                                                                                                                             //金額(当月仕入)
                        TransferArrivalQuantity = string.Format("{0:N0}", x.TransferArrivalQuantity),                                                                                                           //数量(当月移動受入)
                        TransferArrivalAmount = string.Format("{0:N0}", x.TransferArrivalAmount),                                                                                                               //金額(当月移動受入)
                        ShipQuantity = string.Format("{0:N0}", x.ShipQuantity),                                                                                                                                 //数量(当月納車)
                        ShipAmount = string.Format("{0:N0}", x.ShipAmount),                                                                                                                                     //金額(当月納車)
                        TransferDepartureQuantity = string.Format("{0:N0}", x.TransferDepartureQuantity),                                                                                                       //数量(当月移動払出)
                        TransferDepartureAmount = string.Format("{0:N0}", x.TransferDepartureAmount),                                                                                                           //金額(当月移動払出)
                        UnitPriceDifference = string.Format("{0:N0}", x.UnitPriceDifference),                                                                                                                   //単価差額
                        CalculatedQuantity = string.Format("{0:N0}", x.CalculatedQuantity),                                                                                                                     //数量(理論在庫)
                        CalculatedAmount = string.Format("{0:N0}", x.CalculatedAmount),                                                                                                                         //金額(理論在庫)
                        DifferencetCost = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.PostCost) : null,              //単価(棚差)
                        DifferenceQuantity = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.DifferenceQuantity) : null, //数量(棚差)
                        DifferenceAmount = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.DifferenceAmount) : null,     //金額(棚差)
                        PostCost = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.PostCost) : null,                     //単価(月末在庫)
                        PostQuantity = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.PostQuantity) : null,             //数量(月末在庫)
                        PostAmount = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", x.PostAmount) : null,                 //金額(月末在庫)
                        ReservationQuantity = string.Format("{0:N0}", x.ReservationQuantity),                                                                                                                   //数量(引当在庫)
                        ReservationAmount = string.Format("{0:N0}", x.ReservationAmount),                                                                                                                       //金額(引当在庫)
                        InProcessQuantity = string.Format("{0:N0}", x.InProcessQuantity),                                                                                                                       //数量(仕掛在庫)
                        InProcessAmount = string.Format("{0:N0}", x.InProcessAmount)                                                                                                                            //金額(仕掛在庫)
                    }
            ).ToList();
          
            return elist;
        }

        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        private ExcelPartsBalanceTotal MakeExcelList(TotalPartsStockCheck data, FormCollection form)
        {
            ExcelPartsBalanceTotal total = new ExcelPartsBalanceTotal();
            //月初単価
            total.TotalPreCost = "-";
            //数量(月初在庫)
            total.TotalPreQuantity = string.Format("{0:N0}", data.TotalPreQuantity);
            //金額(月初在庫)
            total.TotalPreAmount = string.Format("{0:N0}", data.TotalPreAmount);
            //数量(当月仕入)
            total.TotalPurchaseQuantity = string.Format("{0:N0}", data.TotalPurchaseQuantity);
            //金額(当月仕入)
            total.TotalPurchaseAmount = string.Format("{0:N0}", data.TotalPurchaseAmount);
            //数量(当月移動受入)
            total.TotalTransferArrivalQuantity = string.Format("{0:N0}", data.TotalTransferArrivalQuantity);
            //金額(当月移動受入)
            total.TotalTransferArrivalAmount = string.Format("{0:N0}", data.TotalTransferArrivalAmount);
            //数量(当月納車)
            total.TotalShipQuantity = string.Format("{0:N0}", data.TotalShipQuantity);
            //金額(当月納車)
            total.TotalShipAmount = string.Format("{0:N0}", data.TotalShipAmount);
            //数量(当月移動払出)
            total.TotalTransferDepartureQuantity = string.Format("{0:N0}", data.TotalTransferDepartureQuantity);
            //金額(当月移動払出)
            total.TotalTransferDepartureAmount = string.Format("{0:N0}", data.TotalTransferDepartureAmount);
            //単価差額
            total.TotalUnitPriceDifference = string.Format("{0:N0}", data.TotalUnitPriceDifference);
            //数量(理論在庫)
            total.TotalCalculatedQuantity = string.Format("{0:N0}", data.TotalCalculatedQuantity);
            //金額(理論在庫)
            total.TotalCalculatedAmount = string.Format("{0:N0}", data.TotalCalculatedAmount);
            //単価(棚差)
            total.TotalDifferenceCost = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? "-" : null;
            //数量(棚差)
            total.TotalDifferenceQuantity = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", data.TotalDifferenceQuantity) : null;
            //金額(棚差)
            total.TotalDifferenceAmount = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", data.TotalDifferenceAmount) : null;
            //単価(月末在庫)
            total.TotalPostCost = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? "-" : null;
            //数量(月末在庫)
            total.TotalPostQuantity = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", data.TotalPostQuantity) : null;
            //金額(月末在庫)
            total.TotalPostAmount = (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")) ? string.Format("{0:N0}", data.TotalPostAmount) : null;
            //数量(引当在庫)
            total.TotalReservationQuantity = string.Format("{0:N0}", data.TotalReservationQuantity);
            //金額(引当在庫)
            total.TotalReservationAmount = string.Format("{0:N0}", data.TotalReservationAmount);
            //数量(仕掛在庫)
            total.TotalInProcessQuantity = string.Format("{0:N0}", data.TotalInProcessQuantity);
            //金額(仕掛在庫)
            total.TotalInProcessAmount = string.Format("{0:N0}", data.TotalInProcessAmount);
       
            return total;
        }
        
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <param name="data">検索結果</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private DataTable MakeConditionRow(FormCollection form, TotalPartsStockCheck data)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            //---------------------
            //　列定義
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();
            //棚卸対象年月
            if (!string.IsNullOrWhiteSpace(form["TargetDateY"]) && !string.IsNullOrWhiteSpace(form["TargetDateM"]))
            {
                CodeDao dao = new CodeDao(db);

                conditionText += "対象年月=" + dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name;
            }

            //在庫棚卸ステータス
            conditionText += "　在庫棚卸ステータス=" + (ViewData["InventoryStatusParts"] == null || string.IsNullOrWhiteSpace(ViewData["InventoryStatusParts"].ToString()) ? "未実施" : ViewData["InventoryStatusParts"]);

            //算出日
            conditionText += "　算出日 =" + string.Format("{0:yyyy/MM/dd}", data.plist[0].CalculatedDate);
       
            /*
            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "　部門=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }
            */
            //部品番号
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                conditionText += "　部品番号=" + form["PartsNumber"];
            }
            //部品名
            if (!string.IsNullOrEmpty(form["PartsNameJp"]))
            {
                conditionText += "　部品名=" + form["PartsNameJp"];
            }
            //倉庫
            if (!string.IsNullOrWhiteSpace(form["WarehouseCode"]))
            {
                Warehouse wh = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);

                conditionText += "　倉庫=" + wh.WarehouseName + "(" + wh.WarehouseCode + ")";
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }

        /// <summary>
        /// ヘッダ行の作成(Excel出力用)
        /// </summary>
        /// <param name="list">列名リスト</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();

            //データテーブルにxmlの値を設定する
            int i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }
        #endregion
    }
}