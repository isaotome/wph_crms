using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
//Add 2014/08/04 arc yano IPO対応(在庫管理機能) 
using System.Data.SqlClient;            
using Crms.Models;                 
using System.Data.Linq;
using System.Transactions;
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

//Add 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;

// Del 2015/04/23 arc nakayama 部品在庫検索画面見直し　棚卸に関する機能削除(旧ソースは古いリビジョンを参照して下さい ※棚卸機能は別画面に移行)
namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsStockController : Controller
    {
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
        public PartsStockController() {
            db = new CrmsLinqDataContext();
            //タイムアウト値の設定
            db.CommandTimeout = 300;
        }

        //Add 2014/12/04 arc yano IPO対応(在庫検索)
        private static readonly string TYP_PARTS = "002";                           // 部品(棚卸種別)
        private static readonly string FORM_NAME = "部品在庫";                      // 画面名
        private static readonly string PROC_NAME_SEARCH = "部品在庫検索"; 			// 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "Excel出力";       // 処理名(Excel出力)   //Add 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加

        //Add 2017/07/18 arc yano #3779
        private static readonly string PROC_NAME_DATAEDIT = "データ編集"; 			// 処理名（ログ出力用）
        //private static readonly string PROC_NAME_ADDLINE  = "データ行追加";         // 処理名（ログ出力用）
        private static readonly string PROC_NAME_DELLINE  = "データ行削除";         // 処理名（ログ出力用）

     
        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 処理を全体的に見直し
        /// </history>
        public ActionResult Criteria() {
            
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<CommonPartsStock> line = new EntitySet<CommonPartsStock>();

            //初期値の設定
            form["StockZeroVisibility"] = "0";                                      //0(在庫数量=0を表示しない)
            form["DefaultStockZeroVisibility"] = form["StockZeroVisibility"];       //0(在庫数量=0を表示しない)
            form["RequestFlag"] = "5";                                              //5(リクエストの種類は「検索」)
            form["TargetRange"] = "0";                                              //0(対象年月指定なし)
            form["DefaultTargetRange"] = form["TargetRange"];                       //0(対象年月指定なし)

            ViewData["PartsStockSearch"] = true;                                    //在庫テーブル検索

            return Criteria(form, line);
        }

        /// <summary>
        /// 部品在庫画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 処理を全体的に見直し
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, EntitySet<CommonPartsStock> line)
        {

            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            PaginatedList<CommonPartsStock> list = new PaginatedList<CommonPartsStock>();

            //初回表示の場合
            if (criteriaInit)
            {
                ret = View("PartsStockCriteria", list);
            }
            //検索 or Excel出力
            else 
            {
                //入力値のチェック
                ValidateForSearch(form);

                if (!ModelState.IsValid)
                {
                    //画面設定
                    SetComponent(form);
                    // 検索画面の表示
                    return View("PartsStockCriteria", list);
                }

                switch (form["RequestFlag"])
                {
                    case "1": //Excel出力

                        ret = Download(form);
                        
                        break;

                    default: //検索処理

                        list = new PaginatedList<CommonPartsStock>(SearchList(form).AsQueryable<CommonPartsStock>(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                        ret = View("PartsStockCriteria", list);
                        break;
                }
            }

            //画面設定
            SetComponent(form);

            //コントロールの有効無効
            GetViewResult(form);

            return ret ;
        }

        //Del 2016/08/13 arc yano #3596 不要のため、削除

        /// <summary>
        /// 部品在庫検索画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/24 arc yano #3799 部品在庫検索　現在／過去を識別するフラグの設定を追加
        /// 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 処理を全体的に見直し
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private List<CommonPartsStock> SearchList(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            List<CommonPartsStock> list = new List<CommonPartsStock>();

            //全体の棚卸ステータス取得
            CodeDao dao = new CodeDao(db);

            //棚卸月の取得
            DateTime targetDate = DateTime.Parse((dao.GetYear(true, form["TargetDateY"]) == null ? string.Format("{0:yyyy}", DateTime.Now) : dao.GetYear(true, form["TargetDateY"]).Name) + "/" + (dao.GetMonth(true, form["TargetDateM"]) == null ? string.Format("{0:MM}", DateTime.Now) : dao.GetMonth(true, form["TargetDateM"]).Name) + "/01");  //年と月をつなげる    

            //-----------------------------------------------------------------------
            // Mod 2015/03/10 arc yano #3160(部門コードを任意項目に変更) 
            //     参照するテーブルを以下の条件で変更するように修正
            //     対象年月の入力ありの場合⇒InventoryStock
            //     対象年月の入力なしの場合⇒PartsStock
            //-----------------------------------------------------------------------
            if ((string.IsNullOrWhiteSpace(form["TargetDateY"])) || (string.IsNullOrWhiteSpace(form["TargetDateM"]))) //対象年月のいずれかが未選択状態の場合
            {
                list = GetSearchResultListNow(form, targetDate);
                ViewData["PartsStockSearch"] = true;                //Add 2017/07/24 arc yano #3799
            }
            else
            {
                //検索処理
                list = GetSearchResultListPast(form, targetDate);
                ViewData["PartsStockSearch"] = false;                //Add 2017/07/24 arc yano #3799
            }

            //画面設定
            SetComponent(form);

            return list;
        }
       
        #region 部品在庫検索
        /// <summary>
        /// 部品在庫検索(現在庫)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫０対象、在庫０対象外で呼ぶメソッドを一本化
        /// 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 処理を全体的に見直し
        /// </history>
        //部品在庫検索(現在庫)
        private List<CommonPartsStock> GetSearchResultListNow(FormCollection form, DateTime targetDate)
        {
            //検索項目セット
            CodeDao dao = new CodeDao(db);
            PartsStock condition = new PartsStock();
            condition.Location = new Location();
            condition.Parts = new Parts();
            //condition.Location.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.Location.LocationCode = form["LocationCode"];
            condition.Location.LocationName = form["LocationName"];
            condition.Parts.PartsNumber = form["PartsNumber"];
            condition.Parts.PartsNameJp = form["PartsNameJp"];

            //Mod 2016/08/13 arc yano #3596
            //在庫０表示かどうかの状態を設定
            condition.StockZeroVisibility = form["StockZeroVisibility"];


            //現在庫の検索
            return new PartsStockDao(db).GetListAllByCondition(condition, targetDate);
            
            /*
            if (form["StockZeroVisibility"] == "1")
            {
                //在庫ゼロ対象
                return new PartsStockDao(db).GetListByDepartmentAll(condition, targetDate);
            }
            else
            {
                //在庫ゼロ対象外
                return new PartsStockDao(db).GetListByDepartmentAllNotQuantityZero(condition, targetDate);
            }
            */
        }
        #endregion

        #region  部品在庫検索(指定月在庫)
        /// <summary>
        /// 部品在庫検索(指定月在庫)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫０対象、在庫０対象外で呼ぶメソッドを一本化
        /// 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 処理を全体的に見直し
        /// </history>

        private List<CommonPartsStock> GetSearchResultListPast(FormCollection form, DateTime targetDate)
        {
            CodeDao dao = new CodeDao(db);

            //検索項目セット
            CommonPartsStockSearch CommonPartsStockSearchCondition = new CommonPartsStockSearch();
            CommonPartsStockSearchCondition.TargetDate = targetDate;
            CommonPartsStockSearchCondition.DepartmentCode = form["DepartmentCode"];
            CommonPartsStockSearchCondition.LocationCode = form["LocationCode"];
            CommonPartsStockSearchCondition.LocationName = form["LocationName"];
            CommonPartsStockSearchCondition.PartsNumber = form["PartsNumber"];
            CommonPartsStockSearchCondition.PartsNameJp = form["PartsNameJp"];

            
            //Mod 2016/08/13 arc yano #3596
            //在庫０表示かどうかの状態を設定
            CommonPartsStockSearchCondition.StockZeroVisibility = form["StockZeroVisibility"];


            //指定月在庫の検索
            return new InventoryStockDao(db).GetListAllByCondition(CommonPartsStockSearchCondition);

            /*
            if (form["StockZeroVisibility"] == "1")
            {
                return new InventoryStockDao(db).GetListByDepartmentAll(CommonPartsStockSearchCondition);
            }
            else
            {
                return new InventoryStockDao(db).GetListByDepartmentAllNotQuantityZero(CommonPartsStockSearchCondition);
            }
            */
        }
        #endregion

        #region 個別処理用Validationcheck

        /// <summary>
        /// 検索時のValidationチェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2015/03/11 arc yano #3160
        /// </history>
        private void ValidateForSearch(FormCollection form)
        {
            //対象年月(年=未選択、月=選択)
            if (string.IsNullOrEmpty(form["TargetDateY"]) && !string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateY", "対象年月(年)を選択してください");
            }
            //対象年月(年=選択、月=未選択)
            if (!string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ModelState.AddModelError("TargetDateM", "対象年月(月)を選択してください");
            }
            //対象年月指定ありかつ、対象年月(年=選択、月=未選択)
            if ((form["TargetRange"] == "1") && (string.IsNullOrEmpty(form["TargetDateY"]) && string.IsNullOrEmpty(form["TargetDateM"])))
            {
                ModelState.AddModelError("TargetDateY", "対象年月指定ありの場合は、対象年月を選択してください");
                ModelState.AddModelError("TargetDateM", "");
            }

            return;
        }

        /// <summary>
        /// 部品在庫編集時のvalidationチェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2022/08/30 yano #4101【部品在庫編集】在庫編集画面の調整時のメッセージ
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） 新規作成
        /// </history>
        private void ValidateForDataEdit(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            int cnt = 0;
            bool msg = false;

            //編集行が０行の場合はエラーし、それ以降のvalidationエラー
            if (line == null || line.Count == 0)
            {
                ModelState.AddModelError("", "明細が0行のため、保存できません");

                //即リターン
                return;
            }

            foreach (var a in line)
            {
                ///------------------------
                // 必須チェック
                //------------------------           
                //ロケーションコード
                if (string.IsNullOrWhiteSpace(a.LocationCode))
                {
                    ModelState.AddModelError("line[" + cnt + "].LocationCode", MessageUtils.GetMessage("E0001", cnt + 1 + "行目のロケーションコード"));
                }
                //部品番号
                if (string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("line[" + cnt + "].PartsNumber", MessageUtils.GetMessage("E0001", cnt + 1 + "行目の部品番号"));
                }

                //Add 2022/08/30 yano #4101
                //---------------------------------------------------
                // 編集中に在庫数、引当済数が更新された場合はエラー
                //---------------------------------------------------
              
                  PartsStock stock = new PartsStockDao(db).GetByKey(a.PartsNumber, a.LocationCode);
          
                  decimal quantity = stock != null ? (stock.Quantity ?? 0m) : 0m;
                  decimal provisionquantity = stock != null ? (stock.ProvisionQuantity ?? 0m) : 0m;

                  string quantityindex  = "line[" + cnt + "].SavedQuantity";
                  string provisionindex = "line[" + cnt + "].SavedProvisionQuantity";
                    
                  //修正前に数量
                  decimal prequantity;
                  decimal.TryParse(form[quantityindex], out prequantity);

                  //修正前に引当済数
                  decimal preprovision;
                  decimal.TryParse(form[provisionindex], out preprovision);
          
                  //数量に変更があったか確認
                  if (quantity != prequantity)
                  {
                      if (ModelState.IsValid) 
                      { 
                          ModelState.Clear();
                      }
                      a.Quantity = quantity;
                      ModelState.AddModelError("line[" + cnt + "].Quantity",  cnt + 1 + "行目の在庫数が別のユーザにより'" + prequantity + "'→'" + quantity + "'に更新されました。再度修正してください");
                  }
                    //引当済数に変更があったか確認
                  if (provisionquantity != preprovision)
                  {
                      if (ModelState.IsValid) 
                      { 
                          ModelState.Clear();
                      }

                      a.ProvisionQuantity = provisionquantity;
                      ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", cnt + 1 + "行目の引当済数が別のユーザにより'" + preprovision + "'→'" + provisionquantity + "'に変更されました。再度修正してください");
                  }
                 
                //----------------------------------------------
                // 引当済数が数量を上回っていないかのチェック
                //----------------------------------------------
                if (a.Quantity - a.ProvisionQuantity < 0)
                {
                    ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "数量より多い引当済数を設定できません");
                }
                
                //--------------------------------------------------------
                //　引当済数がサービス伝票の引当済数を下回る場合はエラー
                //--------------------------------------------------------
                if (ModelState.IsValid)
                {
                    //部門倉庫マスタから倉庫コードの割り出し
                    DepartmentWarehouse condition = new DepartmentWarehouse();

                    condition.WarehouseCode = new LocationDao(db).GetByKey(a.LocationCode).WarehouseCode;

                    List<DepartmentWarehouse> dwList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                    //サービス伝票の総引当済数を算出
                    decimal svProvisionQuantity = 0m;
                    foreach (var b in dwList)
                    {
                        svProvisionQuantity += new ServiceSalesOrderDao(db).GetPartsProvisionQuantityByPartsNumber(b.DepartmentCode, a.PartsNumber);
                    }

                    //倉庫内に存在する在庫の引当済数を算出
                    decimal stcProvisionQuantity = 0m;

                    stcProvisionQuantity = new PartsStockDao(db).GetListByWarehouse(condition.WarehouseCode, a.PartsNumber, false, true).Where(x => !x.LocationCode.Equals(a.LocationCode)).AsQueryable().Sum(x => x.ProvisionQuantity ?? 0);

                    stcProvisionQuantity += (a.ProvisionQuantity ?? 0);

                    if (stcProvisionQuantity < svProvisionQuantity)
                    {
                        ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "サービス伝票で引当されている数量の合計より少ない引当済数を設定できません");
                    }
                }

                //--------------------------------------------------------
                //　既に存在する在庫情報を新規作成した場合はエラー
                //--------------------------------------------------------
                if (a.NewRecFlag.Equals(true))
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(a.PartsNumber, a.LocationCode, false);

                    if (rec != null)
                    {
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "既に同一の部品番号、ロケーションの在庫情報が存在しているため、新規作成できません。");
                    }
                }

                //ロケーションコード・部品番号がnullではない場合
                if (!string.IsNullOrWhiteSpace(a.LocationCode) && !string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    //----------------------------------------------------------------
                    // 同一ロケーション・部品番号のレコードが編集画面内に複数行存在
                    //----------------------------------------------------------------
                    int count = line.Where(x => x.LocationCode.Equals(a.LocationCode) && x.PartsNumber.Equals(a.PartsNumber)).Count();

                    if (count > 1)
                    {
                        if (msg == false)
                        {
                            ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                            ModelState.AddModelError("line[" + cnt + "].LocationCode", "ロケーションコード=" + line[cnt].LocationCode + ": 部品番号=" + line[cnt].PartsNumber + "が編集画面上に複数行存在します");
                            msg = true;
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                            ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                        }

                    }                
                }
                cnt++;
            }

            return;
        }

        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            
            //Mod 2015/02/22 arc iijima 全部門部品在庫検索対応
            //検索条件の再セット

            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(false), form["TargetDateY"], true);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(false), form["TargetDateM"], true);
            form["NowTargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);            //部門別検索用当日年
            form["NowTargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');           //部門別検索用当日月
            ViewData["NowTargetDateY"] = form["NowTargetDateY"];
            ViewData["NowTargetDateM"] = form["NowTargetDateM"];
            ViewData["TargetDateY"] = form["TargetDateY"];
            ViewData["TargetDateM"] = form["TargetDateM"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            //処理範囲
            ViewData["TargetRange"] = form["TargetRange"];
            ViewData["DefaultTargetRange"] = form["DefaultTargetRange"];

            if (string.IsNullOrEmpty(form["DepartmentCode"])){              //部門名
                ViewData["DepartmentName"] = "";                            //部門コードが未入力なら空文字
            }
            else{
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];    //対象日付(yyyy)
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];    //対象日付(MM)
            
            //Mod 2015/02/12 arc yano 部品在庫検索 フォームの在庫0表示の値がNULLの場合は0をデフォルトで設定する。
            ViewData["StockZeroVisibility"] = (form["StockZeroVisibility"] != null ? form["StockZeroVisibility"] : "0");
            
            //Mod 2015/03/11 arc yano #3160
            ViewData["DefaultStockZeroVisibility"] = form["DefaultStockZeroVisibility"];
            ViewData["RequestFlag"] = form["RequestFlag"];

            //部品棚卸作業日取得
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDateDao(db).GetAllVal();
            if (WorkingDate != null)
            {
                ViewData["PartsInventoryWorkingDate"] = WorkingDate.InventoryWorkingDate;
            }

            //Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
            //棚卸ステータスの設定
            //Mod 2015/02/22 arc iijima 日付null対応
            if (string.IsNullOrEmpty(form["TargetDateY"]) || string.IsNullOrEmpty(form["TargetDateM"]))
            {
                ViewData["DepartmentInventoryStatus"] = ""; //日付が入力されていないため、ステータスは空欄
                return;
            }
            else
            {
                DateTime targetDate = DateTime.Parse(dao.GetYear(false, form["TargetDateY"]).Name + "/" + dao.GetMonth(false, form["TargetDateM"]).Name + "/01");
                InventoryScheduleParts ivs = new InventorySchedulePartsDao(db).GetByKey(form["DepartmentCode"], targetDate, TYP_PARTS);

                if (ivs != null)
                {
                    ViewData["DepartmentInventoryStatus"] = ivs.InventoryStatus;
                }
            }
            return;
        }
        #endregion

        //Add 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加
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

            PaginatedList<CommonPartsStock> list = new PaginatedList<CommonPartsStock>();
            

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //ファイル名
            string fileName = "PartsStock" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsStock"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsStock"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form);
                return View("PartsStockCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("PartsStockCriteria", list);
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

            //検索結果の取得
            List<CommonPartsStock> list = SearchList(form);

            List<ExcelCommonPartsStock> elist = null;

            //取得した検索結果をExcel出力用に成形
            elist = MakeExcelList(list, form);

            //データ設定
            ret = dExport.SetData<ExcelCommonPartsStock, ExcelCommonPartsStock>(ref excelFile, elist, configLine);

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
        private List<ExcelCommonPartsStock> MakeExcelList(List<CommonPartsStock> list, FormCollection form)
        {
            List<ExcelCommonPartsStock> elist = new List<ExcelCommonPartsStock>();

            //対象年月の指定がある場合は実棚を数量に
            if (!string.IsNullOrWhiteSpace(form["TargetDateY"]) && !string.IsNullOrWhiteSpace(form["TargetDateM"]))
            {
                elist = list.Select(x =>
                        new ExcelCommonPartsStock()
                        {
                            DepartmentName = x.DepartmentName
                            ,
                            LocationCode = x.LocationCode
                            ,
                            LocationName = x.LocationName
                            ,
                            PartsNumber = x.PartsNumber
                            ,
                            PartsNameJp = x.PartsNameJp
                            ,
                            Quantity = string.Format("{0:F1}", (x.PhysicalQuantity ?? 0))
                            ,
                            ProvisionQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.ProvisionQuantity ?? 0))
                            ,
                            FreeQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.PhysicalQuantity ?? 0) - (x.ProvisionQuantity ?? 0))
                            ,
                            StandardPrice = x.StandardPrice != null ? string.Format("{0:N0}", x.StandardPrice) : ""
                            ,
                            MoveAverageUnitPrice = x.MoveAverageUnitPrice != null ? string.Format("{0:N0}", x.MoveAverageUnitPrice) : ""
                            ,
                            Price = x.Price != null ? string.Format("{0:N0}", x.Price) : ""
                        }
                ).ToList();
            }
            else
            {
                elist = list.Select(x =>
                        new ExcelCommonPartsStock()
                        {
                            DepartmentName = x.DepartmentName
                            ,
                            LocationCode = x.LocationCode
                            ,
                            LocationName = x.LocationName
                            ,
                            PartsNumber = x.PartsNumber
                            ,
                            PartsNameJp = x.PartsNameJp
                            ,
                            Quantity = string.Format("{0:F1}", (x.Quantity ?? 0))
                            ,
                            ProvisionQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.ProvisionQuantity ?? 0))
                            ,
                            FreeQuantity = !x.LocationType.Equals("001") ? "-" : string.Format("{0:F1}", (x.Quantity ?? 0) - (x.ProvisionQuantity ?? 0))
                            ,
                            StandardPrice = x.StandardPrice != null ? string.Format("{0:N0}", x.StandardPrice) : ""
                            ,
                            MoveAverageUnitPrice = x.MoveAverageUnitPrice != null ? string.Format("{0:N0}", x.MoveAverageUnitPrice) : ""
                            ,
                            Price = x.Price != null ? string.Format("{0:N0}", x.Price) : ""
                        }
                ).ToList();
            }

            return elist;

        }

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(FormCollection form)
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
            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "　部門=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }
            //ロケーションコード
            if (!string.IsNullOrEmpty(form["LocationCode"]))
            {
                conditionText += "　ロケーションコード=" + form["LocationCode"];
            }
            //ロケーション名
            if (!string.IsNullOrEmpty(form["LocationName"]))
            {
                conditionText += "　ロケーション名=" + form["LocationName"];
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
            //在庫ゼロ表示
            if (form["StockZeroVisibility"].Equals("0"))
            {
                conditionText += "　在庫ゼロ表示=しない";
            }
            else
            {
                conditionText += "　在庫ゼロ表示=する";
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

       
        /// <summary>
        /// データ編集画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3900 部品在庫検索　編集画面を表示すると同一の在庫情報が２行表示される
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正）新規作成
        /// </history>
        public ActionResult DataEdit()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<CommonPartsStock> line = new EntitySet<CommonPartsStock>();

            //初期値の設定
            form["RequestFlag"] = "99";                                              // 99(リクエストの種類は「検索」)

            form["CreateFlag"] = Request["CreateFlag"];                              //新規作成フラグ

            form["LocationCode"] = Request["LocationCode"];                          //ロケーションコード
            form["PartsNumber"]  = Request["PartsNumber"];                           //部品番号
            form["DepartmentCode"] = Request["DepartmentCode"];                      //部門コード    //Add 2018/06/01 arc yano #3900

            return DataEdit(form, line);
        }

        /// <summary>
        /// データ編集画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3900 部品在庫検索　編集画面を表示すると同一の在庫情報が２行表示される
        /// 2017/07/18 arc yano #3779  部品在庫（部品在庫の修正）新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DataEdit(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            List<CommonPartsStock> list = new List<CommonPartsStock>();

            //新規作成の場合
            if (!string.IsNullOrWhiteSpace(form["CreateFlag"]) && form["CreateFlag"].Equals("1"))
            {
                ret = AddLine(form, line);
            }
            //更新の場合
            else
            {
                switch (form["RequestFlag"])
                {
                    case "10" : //保存

                        //入力値のチェック
                        ValidateForDataEdit(form, line);

                        if (!ModelState.IsValid)
                        {
                            //画面設定
                            SetComponent(form);
                            //コントロールの有効無効
                            GetViewResult(form);

                            if (line != null && line.Count > 0)
                            {
                                list = line.ToList();
                            }
                            
                            return View("PartsStockDataEdit", list);
                        }

                        //保存処理
                        ret = DataSave(form, line);

                        break;
                    
                    case "11": //行追加

                        ret = AddLine(form, line);

                        break;

                    case "12": //行削除

                        ret = DelLine(form, line);

                        break;

                    default: //検索処理

                        list = SearchList(form).Where(x => x.LocationCode.Equals(form["LocationCode"]) && x.PartsNumber.Equals(form["PartsNumber"]) && x.DepartmentCode.Equals(form["DepartmentCode"])).ToList<CommonPartsStock>();     //Add 2018/06/01 arc yano #3900
                        ret = View("PartsStockDataEdit", list);
                        break;
                }
            }

            //画面設定
            SetComponent(form);
                
            //コントロールの有効無効
            GetViewResult(form);

            return ret;
        }
        
        /// <summary>
        /// 部品在庫検索ダイアログ初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog(){
            criteriaInit = true;
            Employee emp = (Employee)Session["Employee"];
            FormCollection form = new FormCollection();
            //if(emp.SecurityRole.SecurityLevelCode.Equals("001")){
                form["DepartmentCode"] = emp.DepartmentCode;
            //}
            return CriteriaDialog(form);
        }
        /// <summary>
        /// 部品在庫検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            
            // クエリストリングから検索条件を取得
            form["CarBrandName"] = Request["CarBrandName"];

            PaginatedList<GetPartsStockForDialogResult> list = new PaginatedList<GetPartsStockForDialogResult>();
            Employee emp = (Employee)Session["Employee"];
            //form["DepartmentCode"] = emp.DepartmentCode;

            // 検索条件の設定
            // Add 2015/10/07 arc nakayama #3266_部品在庫検索ダイアログに仕入先の検索項目を追加
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                ViewData["SupplierName"] = new SupplierDao(db).GetByKey(form["SupplierCode"], false).SupplierName;
            }
            form["DelFlag"] = "0";

            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

            // マスタに存在しない部門はエラー
            if (!criteriaInit && dep == null) {
                ModelState.AddModelError("", MessageUtils.GetMessage("E0016", "部門コード"));
                return View("PartsStockCriteriaDialog", list);
            } else {
                ViewData["DepartmentName"] = dep.DepartmentName;
            }

            // 指定された部門の参照権限がない場合はエラー
            switch (emp.SecurityRole.SecurityLevelCode) {
                case "001": //部門内
                    if (!emp.DepartmentCode.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode1.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode2.Equals(dep.DepartmentCode) &&
                        !emp.DepartmentCode3.Equals(dep.DepartmentCode)) {
                        ModelState.AddModelError("", "指定された部門の在庫を参照する権限がありません");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "002": //事業部内
                    if (!emp.Department1.OfficeCode.Equals(dep.OfficeCode) &&
                        !(emp.AdditionalDepartment1!=null && emp.AdditionalDepartment1.OfficeCode.Equals(dep.OfficeCode)) &&
                        !(emp.AdditionalDepartment2!=null && emp.AdditionalDepartment2.OfficeCode.Equals(dep.OfficeCode)) &&
                        !(emp.AdditionalDepartment3!=null && emp.AdditionalDepartment3.OfficeCode.Equals(dep.OfficeCode)))
                    {
                        ModelState.AddModelError("", "指定された部門の在庫を参照する権限がありません");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "003": //会社内
                    if (!emp.Department1.Office.CompanyCode.Equals(dep.Office.CompanyCode) &&
                        !(emp.AdditionalDepartment1!=null && emp.AdditionalDepartment1.Office!=null && !emp.AdditionalDepartment1.Office.CompanyCode.Equals(dep.Office.CompanyCode)) &&
                        !(emp.AdditionalDepartment2!=null && emp.AdditionalDepartment2.Office!=null && !emp.AdditionalDepartment2.Office.CompanyCode.Equals(dep.Office.CompanyCode)) &&
                        !(emp.AdditionalDepartment3!=null && emp.AdditionalDepartment3.Office!=null && !emp.AdditionalDepartment3.Office.CompanyCode.Equals(dep.Office.CompanyCode))
                        ) {
                        ModelState.AddModelError("", "指定された部門の在庫を参照する権限がありません");
                        return View("PartsStockCriteriaDialog", list);
                    }
                    break;
                case "004": //ALL
                    break;

            }

            // 検索結果リストの取得
            if (criteriaInit) {
                ViewData["InstallableFlag"] = false;
                return View("PartsStockCriteriaDialog", list);
            } else {
                if (string.IsNullOrEmpty(form["DepartmentCode"])) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", "部門コード"));
                    return View("PartsStockCriteriaDialog", list);
                }
                list = GetPartsSearchResultList(form);
            }

            // 部品在庫検索画面の表示
            return View("PartsStockCriteriaDialog", list);
        }

        /// <summary>
        /// 在庫データ保存処理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano 部品在庫（部品在庫の修正）#3779 新規作成
        /// </history>
        private ActionResult DataSave(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCarテーブルのレコード更新
                //------------------------------------------------------
                List<PartsStock> psList = new List<PartsStock>();

                for (int k = 0; k < line.Count; k++)
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(line[k].PartsNumber, line[k].LocationCode, true);

                    if (rec != null)
                    {
                        rec.Quantity = line[k].Quantity;
                        rec.ProvisionQuantity = line[k].ProvisionQuantity;
                        rec.DelFlag = "0";                                      //削除フラグは常に有効にしておく

                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
                    }
                    else
                    {
                        PartsStock partsstock = new PartsStock();
                        partsstock.PartsNumber = line[k].PartsNumber;                                    //部品番号
                        partsstock.LocationCode = line[k].LocationCode;                                  //ロケーションコード
                        partsstock.Quantity = line[k].Quantity;                                          //数量
                        partsstock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //作成者
                        partsstock.CreateDate = DateTime.Now;                                                   //作成日
                        partsstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;       //最終更新者
                        partsstock.LastUpdateDate = DateTime.Now;                                               //最終更新日
                        partsstock.DelFlag = "0";                                                               //削除フラグ
                        partsstock.ProvisionQuantity = line[k].ProvisionQuantity;                               //引当済数

                        psList.Add(partsstock);
                    }

                    //保存後は新規作成フラグを落とす
                    if (line[k].NewRecFlag.Equals(true))
                    {
                        line[k].NewRecFlag = false;
                    }
                }

                db.PartsStock.InsertAllOnSubmit(psList);

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_DATAEDIT, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            return View("PartsStockDataEdit", line.ToList());
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_DATAEDIT, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            ModelState.AddModelError("", "保存しました");

            return View("PartsStockDataEdit", line.ToList());
        }

        /// <summary>
        /// 部品リストを検索する
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <returns>部品データリスト</returns>
        private PaginatedList<GetPartsStockForDialogResult> GetPartsSearchResultList(FormCollection form) {
            
            PartsStockSearchCondition condition = new PartsStockSearchCondition();
            condition.MakerCode = form["MakerCode"];
            condition.MakerName = form["MakerName"];
            condition.PartsNumber = form["PartsNumber"];
            condition.PartsNameJp = form["PartsNameJp"];
            condition.CarBrandName = form["CarBrandName"];
            condition.CarBrandCode = form["CarBrandCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.SupplierCode = form["SupplierCode"];

            return new PartsDao(db).GetListByConditionForDialog(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 部品番号から部品在庫詳細情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部品番号</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 単価の取得先をdbo.PartsAverageCsot→dbo.PartsMovingAverageCostに変更
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/10/28 arc yano #3289 サービス伝票 純正区分を取得する処理の追加
        /// </history>
        public ActionResult GetMasterDetail(string code, string departmentCode) {
            if (Request.IsAjaxRequest()) {

                //Add 2016/08/13 arc yano #3596
                //部門コードから使用倉庫を割出す
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

                decimal quantity = new PartsStockDao(db).GetStockQuantity(code, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));

                Dictionary<string, string> retParts = new Dictionary<string, string>();

                //Mod 2018/05/14 arc yano #3880
                PartsMovingAverageCost condition = new PartsMovingAverageCost();
                condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;
                condition.PartsNumber = code;
                PartsMovingAverageCost averageCost = new PartsMovingAverageCostDao(db).GetByKey(condition);
                
                /*
                    PartsAverageCost condition = new PartsAverageCost();
                    condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;
                    condition.PartsNumber = code;
                    PartsAverageCost averageCost = new PartsAverageCostDao(db).GetByKey(condition);
                */
                retParts.Add("Quantity", quantity.ToString());
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null) {
                    retParts.Add("PartsNumber", code);
                    retParts.Add("Price", parts.Price.ToString());
                    retParts.Add("PartsName", string.IsNullOrEmpty(parts.PartsNameJp) ? parts.PartsNameEn : parts.PartsNameJp);
                    retParts.Add("GenuineType", parts.GenuineType ?? "");   //Add 2015/10/28 arc yano #3289
                    if (averageCost != null) {
                        retParts.Add("Cost", averageCost.Price.Value.ToString());
                    } else {
                        //Mod 2016/07/14 arc kashiwada #3619
                        //retParts.Add("Cost", "0");
                        // 新規登録部品もしくは移動平均単価が無い部品の場合、部品マスタの原価を表示する
                        if (parts.Cost != null)
                        {
                            retParts.Add("Cost", parts.Cost.Value.ToString());
                        }
                        else
                        {
                            retParts.Add("Cost", "0");
                        }

                    }
                }
                return Json(retParts);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// 部品番号とロケーションコードから部品在庫詳細情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部品番号</param>
        /// <param name="location">ロケーションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>2016/06/20 arc yano #3582</history>
        public ActionResult GetMaster(string partsNumber,string location) {
            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retParts = new Dictionary<string, string>();
                PartsStock stock = new PartsStockDao(db).GetByKey(partsNumber, location);
                
                retParts.Add("Quantity", stock!=null ? ((stock.Quantity ?? 0) - (stock.ProvisionQuantity ?? 0)).ToString() : "0");  //Mod 2016/06/20 arc yano #3582
                Parts parts = new PartsDao(db).GetByKey(partsNumber);
                if (parts != null) {
                    retParts.Add("PartsNumber", parts.PartsNumber);
                    retParts.Add("PartsNameJp", parts.PartsNameJp);
                    retParts.Add("PartsNameEn", parts.PartsNameEn);
                    //retParts.Add("Cost", parts.Cost.ToString());
                }
                return Json(retParts);
            }
            return new EmptyResult();
        }

         #region コントロールの有効無効
        /// <summary>
        /// コントロールの有効無効状態を返す。
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正）データ編集
        /// </history>
        private void GetViewResult(FormCollection form)
        {
            Employee loginUser = (Employee)Session["Employee"];
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            //セキュリティレベルが4でないユーザは、ボタンを押下不可にする。
            if (!securityLevel.Equals("004"))
            {
                ViewData["ButtonEnabled"] = false;
            }
            else
            {
                ViewData["ButtonEnabled"] = true;
            }

            //Add 2017/07/18 arc yano #3779
            //データ編集ボタン表示・非表示処理　権限のあるユーザのみ（現状はシステム課ユーザ）表示
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "StockDataEdit");

            if (rec != null && rec.EnableFlag.Equals(true))
            {
                ViewData["EditButtonVisible"] = true;
            }
            else
            {
                ViewData["EditButtonVisible"] = false;
            }

            return;
        }
        #endregion

        #region 部品在庫行追加・削除
        /// <summary>
        /// 部品在庫行追加
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="line">部品在庫明細</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） 新規作成
        /// </history>
        private ActionResult AddLine(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            if (line == null)
            {
                line = new EntitySet<CommonPartsStock>();
            }

            CommonPartsStock rec = new CommonPartsStock();

            //数量、引当済数の初期化
            rec.Quantity = 0;
            rec.ProvisionQuantity = 0;

            rec.NewRecFlag = true;

            line.Add(rec);

            form["EditFlag"] = "true";

            SetComponent(form);

            return View("PartsStockDataEdit", line.ToList());
        }

        /// <summary>
        /// 部品在庫データ行削除
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">明細データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） 新規作成
        /// </history>
        private ActionResult DelLine(FormCollection form, EntitySet<CommonPartsStock> line)
        {
            ModelState.Clear();

            int targetId = int.Parse(form["DelLine"]);

            //---------------------------------------------------
            //サービス伝票で引当済の場合は削除できないようにする
            //---------------------------------------------------

            //部門倉庫マスタから倉庫コードの割り出し
            DepartmentWarehouse condition = new DepartmentWarehouse();

            condition.WarehouseCode = new LocationDao(db).GetByKey(line[targetId].LocationCode).WarehouseCode;

            List<DepartmentWarehouse> dwList = new DepartmentWarehouseDao(db).GetByCondition(condition);

            //サービス伝票の総引当済数を算出
            decimal svProvisionQuantity = 0m;
            foreach (var b in dwList)
            {
                svProvisionQuantity += new ServiceSalesOrderDao(db).GetPartsProvisionQuantityByPartsNumber(b.DepartmentCode, line[targetId].PartsNumber);
            }

            //倉庫内に存在する在庫の引当済数を算出
            decimal stcProvisionQuantity = 0m;

            stcProvisionQuantity = new PartsStockDao(db).GetListByWarehouse(condition.WarehouseCode, line[targetId].PartsNumber, false, true).Where(x => !x.LocationCode.Equals(line[targetId].LocationCode)).AsQueryable().Sum(x => x.ProvisionQuantity ?? 0);

            if (stcProvisionQuantity < svProvisionQuantity)
            {
                ModelState.AddModelError("", "サービス伝票で引当されている数量の合計より少なくなるため、削除できません");

                SetComponent(form);

                return View("PartsStockDataEdit", line.ToList());
            }

            using (TransactionScope ts = new TransactionScope())
            {
                if (line[targetId].NewRecFlag.Equals(false))
                {
                    PartsStock rec = new PartsStockDao(db).GetByKey(line[targetId].PartsNumber, line[targetId].LocationCode);

                    if (rec != null)
                    {
                        rec.DelFlag = "1";          //削除フラグを立てる
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
                    }
                }
                try
                {
                    //エンティティの削除処理
                    line.RemoveAt(targetId);

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_DELLINE, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_DELLINE, FORM_NAME, "");
                    ts.Dispose();
                }
            }
         
            SetComponent(form);

            return View("PartsStockDataEdit", line.ToList());
        }

        #endregion


        //Add 2014/12/15 arc yano IPO対応(部品検索) 処理中対応
       /// <summary>
       /// 処理中かどうかを取得する。(Ajax専用）
       /// </summary>
       /// <param name="processType">処理種別</param>
       /// <returns>アイドリング状態</returns>
       public ActionResult GetProcessed(string processType)
       {
           if (Request.IsAjaxRequest())
           {
               Dictionary<string, string> retParts = new Dictionary<string, string>();
                
               retParts.Add("ProcessedTime", "アイドリング中…");
                
               return Json(retParts);
           }
           return new EmptyResult();
       }
    }
}
