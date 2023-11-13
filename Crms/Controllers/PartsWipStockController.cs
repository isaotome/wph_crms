using System;
using System.Collections;
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

//Add 2016/07/14 arc yano #3600 仕掛在庫検索　Excel出力機能追加
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
    public class PartsWipStockController : Controller
    {
        
        private static readonly string FORM_NAME = "部品仕掛在庫";                          // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "部品仕掛在庫検索";               // 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";               // 処理名(Excel出力)   //Add 2016/07/14 arc yano #3600 仕掛在庫検索　Excel出力機能追加

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsWipStockController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 部品検索画面表示
        /// </summary>
        /// <returns>部品検索画面</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 部品棚卸フラグによる絞込に変更
        /// </history>
        [AuthFilter]
        public ActionResult Criteria()
        {
            //Add 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応 検索項目に明細種別追加
            criteriaInit = true;
            FormCollection form = new FormCollection();
            //return Criteria(form);
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1,3);    //初期値に当日の年をセットする
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');  //初期値に当日の月をセットする
            form["ServiceType"] = "";
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];
            form["DefaultServiceType"] = form["ServiceType"];
            //部門(ログインユーザの部門を設定する)
            //Mod 2015/06/15 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 部品棚卸対象外の部門のログインユーザの場合は部門コードは空欄とする
            //Department department = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode, includeDeleted: false, closeMonthFlag: "2");
            Department department = new DepartmentDao(db).GetByPartsInventory(((Employee)Session["Employee"]).DepartmentCode);      //Mod 2017/05/10 arc yano #3762
            if (department != null)
            {
                form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
                form["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            }
            else
            {
                form["DepartmentCode"] = "";
                form["DefaultDepartmentCode"] = "";
            }

            return Criteria(form);
        }

        /// <summary>
        /// 部品仕掛在庫検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品仕掛在庫検索画面</returns>
        /// <history>
        /// Mod 2016/07/14 arc yano  #3600 仕掛在庫検索　Excel出力機能追加 検索処理、
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ActionResult ret = new EmptyResult();

            // 検索結果リストの取得
            PaginatedList<InventoryParts_Shikakari> list = new PaginatedList<InventoryParts_Shikakari>();
            
            if (criteriaInit)
            {
                //画面設定
                SetDataComponent(form);
                ret = View("PartsWipStockCriteria", list);
            }
            else
            {
                //入力チェック
                ValidateSearch(form);
                if (!ModelState.IsValid)
                {
                    //画面データ再設定
                    SetDataComponent(form);
                    return View("PartsWipStockCriteria", list);
                }

                //
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

                        ret = View("PartsWipStockCriteria", list);
                        break;
                }
            }
        
            return ret;
        }

        //Mod 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧ 仕掛在庫データの取得方法をビューテーブルからストアドに変更
        //Add 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応 検索項目に明細種別追加
        //Mod 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
        /// <summary>
        /// 部品仕掛在庫検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品仕掛在庫検索結果リスト（現在のデータ）</returns>
        private PaginatedList<InventoryParts_Shikakari> GetSearchResultList(FormCollection form)
        {
            PartsWipStockDao PartsWipStockDao = new PartsWipStockDao(db);
            InventoryParts_Shikakari condition = new InventoryParts_Shikakari();
            
            CodeDao dao = new CodeDao(db);
            
            condition.InventoryMonth = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる                      
            condition.DepartmentCode = form["DepartmentCode"];                                                                    //部門コード
            condition.ArrivalPlanDateFrom = CommonUtils.StrToDateTime(form["ArrivalPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);    //入庫日(From)
            condition.ArrivalPlanDateTo = CommonUtils.StrToDateTime(form["ArrivalPlanDateTo"], DaoConst.SQL_DATETIME_MAX);        //入庫日(To)
            condition.PartsNumber = form["PartsNumber"];                                                                          //部品ナンバー
            condition.LineContents1 = form["PartsNameJp"];                                                                          //部品名(日本語名)
            condition.SlipNumber = form["SlipNumber"];                                                                            //伝票番号
            condition.CustomerName = form["CustomerName"];                                                                        //顧客名
            condition.Vin = form["Vin"];                                                                                          //車台番号
            condition.ServiceType = form["ServiceType"];                                                                          //明細種別 

            return PartsWipStockDao.GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Add 2016/10/28 arc nakayama #3653_仕掛在庫のExcel出力で行うと100件しか表示されない
        /// <summary>
        /// 部品仕掛在庫検索結果リスト取得(Excel用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品仕掛在庫検索結果リスト（現在のデータ）</returns>
        private List<InventoryParts_Shikakari> GetSearchResultListForExcel(FormCollection form)
        {
            PartsWipStockDao PartsWipStockDao = new PartsWipStockDao(db);
            InventoryParts_Shikakari condition = new InventoryParts_Shikakari();

            CodeDao dao = new CodeDao(db);

            condition.InventoryMonth = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる                      
            condition.DepartmentCode = form["DepartmentCode"];                                                                    //部門コード
            condition.ArrivalPlanDateFrom = CommonUtils.StrToDateTime(form["ArrivalPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);    //入庫日(From)
            condition.ArrivalPlanDateTo = CommonUtils.StrToDateTime(form["ArrivalPlanDateTo"], DaoConst.SQL_DATETIME_MAX);        //入庫日(To)
            condition.PartsNumber = form["PartsNumber"];                                                                          //部品ナンバー
            condition.LineContents1 = form["PartsNameJp"];                                                                          //部品名(日本語名)
            condition.SlipNumber = form["SlipNumber"];                                                                            //伝票番号
            condition.CustomerName = form["CustomerName"];                                                                        //顧客名
            condition.Vin = form["Vin"];                                                                                          //車台番号
            condition.ServiceType = form["ServiceType"];                                                                          //明細種別 

            return PartsWipStockDao.GetListByConditionForExcel(condition);
        }


        //Del 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧　当月／過去の判定はストアドで行うように対応するため、削除

        #region 入力チェック
        /// <summary>
        /// 検索時の入力チェック
        /// </summary>
        /// <param name="form"></param>
        private void ValidateSearch(FormCollection form)
        {
            ModelState.Clear();

            //対象年
            if (string.IsNullOrEmpty(form["TargetDateY"]))
            {
                ModelState.AddModelError("TargetDateY", MessageUtils.GetMessage("E0001", "対象年"));
                return;
            }

            //対象月
            if (string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateM", MessageUtils.GetMessage("E0001", "対象月"));
                return;
            }

            //部門コード
            //対象月
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門コード"));
                return;
            }

        }
        #endregion


         #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetDataComponent(FormCollection form)
        {

            CodeDao dao = new CodeDao(db);
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);

            ViewData["ServiceTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("015", false), form["ServiceType"], true);
            //Add 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応 検索項目に明細種別追加
            ViewData["ServiceType"] = form["ServiceType"];
            ViewData["DefaultServiceType"] = form["DefaultServiceType"];

            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];          //対象日付(yyyy)
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];          //対象日付(MM)
            //Mod 2015/06/15 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④
            ViewData["DefaultDepartmentCode"] = form["DefaultDepartmentCode"];
            if (string.IsNullOrEmpty(form["DefaultDepartmentCode"]))
            {      //部門名
                ViewData["DefaultDepartmentName"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
                ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(form["DefaultDepartmentCode"].ToString()).DepartmentName;  //入力済みなら部門表から検索
            }

            ViewData["TargetDateY"] = form["TargetDateY"];          //対象日付(yyyy)
            ViewData["TargetDateM"] = form["TargetDateM"];          //対象日付(MM)
            ViewData["DepartmentCode"] = form["DepartmentCode"];    //部門コード
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {      //部門名
                ViewData["DepartmentName"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //入力済みなら部門表から検索
            }
            //Mod 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
            ViewData["ArrivalPlanDateFrom"] = form["ArrivalPlanDateFrom"];  //入庫日(From)
            ViewData["ArrivalPlanDateTo"] = form["ArrivalPlanDateTo"];      //入庫日(To)
            ViewData["SlipNumber"] = form["SlipNumber"];                    //伝票番号
            ViewData["PartsNumber"] = form["PartsNumber"];                  //部品番号
            ViewData["PartsNameJp"] = form["PartsNameJp"];                  //部品名(英)
            ViewData["CustomerName"] = form["CustomerName"];                //顧客名
            ViewData["Vin"] = form["Vin"];                                  //車台番号
            
        }
        #endregion

        //Add 2016/07/14 arc yano #3600 仕掛在庫検索　Excel出力機能追加
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
            InventoryParts_Shikakari list = new InventoryParts_Shikakari();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "Inventory_Shikakari" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForInventory_Shikakari"]) ? "" : ConfigurationManager.AppSettings["TemplateForInventory_Shikakari"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);
                return View("PartsWipStockCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("PartsWipStockCriteria", list);
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

            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(form);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //検索結果取得
            //Mod 2016/10/28 arc nakayama #3653_仕掛在庫のExcel出力で行うと100件しか表示されない
            List<InventoryParts_Shikakari> list = GetSearchResultListForExcel(form);

            List<ExcelInventory_Shikakari> elist = new List<ExcelInventory_Shikakari>();

            elist = MakeExcelList(list, form);

            //データ設定
            ret = dExport.SetData<ExcelInventory_Shikakari, ExcelInventory_Shikakari>(ref excelFile, elist, configLine);

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

        //Mod 2016/10/28 arc nakayama #3653_仕掛在庫のExcel出力で行うと100件しか表示されない
        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        private List<ExcelInventory_Shikakari> MakeExcelList(List<InventoryParts_Shikakari> list, FormCollection form)
        {
            List<ExcelInventory_Shikakari> elist = new List<ExcelInventory_Shikakari>();

            //対象年月の指定がある場合は実棚を数量に

            elist = list.Select(x =>
                    new ExcelInventory_Shikakari()
                    {
                        //入庫日
                        ArrivalPlanDate = string.Format("{0:yyyy/MM/dd}", x.ArrivalPlanDate),
                        //伝票番号
                        SlipNumber = x.SlipNumber,
                        //明細行番号
                        LineNumber = string.Format("{0:N0}", x.LineNumber),
                        //伝票ステータス名
                        ServiceOrderStatusName = x.ServiceOrderStatusName,
                        //主作業名
                        ServiceWorksName = x.ServiceWorksName,
                        //フロント担当者名
                        FrontEmployeeName = x.FrontEmployeeName,
                        //メカニック担当者
                        MekaEmployeeName = x.MekaEmployeeName,
                        //顧客名
                        CustomerName = x.CustomerName,
                        //車種名
                        CarName = x.CarName,
                        //車台番号
                        Vin = x.Vin,
                        //明細種別名
                        ServiceTypeName = x.ServiceTypeName,
                        //状況
                        StockTypeName = x.StockTypeName,
                        //発注日
                        PurchaseOrderDate = string.Format("{0:yyyy/MM/dd}", x.PurchaseOrderDate),
                        //入荷日
                        PurchaseDate = string.Format("{0:yyyy/MM/dd}", x.PurchaseDate),
                        //入荷予定日
                        PartsArravalPlanDate = string.Format("{0:yyyy/MM/dd}", x.PartsArravalPlanDate),
                        //部品番号
                        PartsNumber = x.PartsNumber,
                        //部品名
                        LineContents1 = x.LineContents1,
                        //単価
                        Price = string.Format("{0:N0}", x.Price),
                        //数量
                        Quantity = string.Format("{0:F2}", x.Quantity),
                        //金額
                        Amount = string.Format("{0:N0}", x.Amount),
                        //外注先名
                        SupplierName = x.SupplierName,
                        //作業内容
                        LineContents2 = x.LineContents2,
                        //外注原価
                        Cost = string.Format("{0:N0}", x.Cost)
                    }
            ).ToList();

            return elist;

        }

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns name = "dt" >検索条件</returns>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            CodeDao dao = new CodeDao(db);

            //---------------------
            //　列定義
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();
            
            //対象年月
            if (!string.IsNullOrWhiteSpace(form["TargetDateY"]) && !string.IsNullOrWhiteSpace(form["TargetDateM"]))
            {
                conditionText += "対象年月=" + dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name;
            }

            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "　部門=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }

            //明細種別
            if (!string.IsNullOrWhiteSpace(form["ServiceType"]))
            {
                conditionText += "　明細種別=" + dao.GetCodeNameByKey("015", form["ServiceType"], false);
            }

            //入庫日
            if (!string.IsNullOrWhiteSpace(form["ArrivalPlanDateFrom"]) || !string.IsNullOrWhiteSpace(form["ArrivalPlanDateTo"]))
            {
                conditionText += "　入庫日=" + form["ArrivalPlanDateFrom"] + "～" + form["ArrivalPlanDateTo"];
            }
            //伝票番号
            if (!string.IsNullOrWhiteSpace(form["SlipNumber"]))
            {
                conditionText += "　伝票番号=" + form["SlipNumber"];
            }
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
            //車台番号
            if (!string.IsNullOrEmpty(form["Vin"]))
            {
                conditionText += "　車台番号=" + form["Vin"];
            }
            //顧客名
            if (!string.IsNullOrEmpty(form["CustomerName"]))
            {
                conditionText += "　顧客名=" + form["CustomerName"];
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