using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using System.Transactions;
using System.Text.RegularExpressions;
using Crms.Models;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
namespace Crms.Controllers
{
    /// <summary>
    /// クレジット入金確認検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CreditJournalCheckController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "クレジット入金確認";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "クレジット入金確認 検索"; // 処理名（ログ出力用）　
        private static readonly string PROC_NAME_EXCEL = "クレジット入金確認 Excel出力";       // 処理名(CSV出力)
        #endregion

        #region 初期化
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
        public CreditJournalCheckController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        /// <summary>
        /// クレジット入金確認画面表示
        /// </summary>
        /// <returns>クレジット入金確認画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            form["RequestFlag"] = "1";  //リクエストフラグ　デフォルト値 = 1
            return Criteria(form);
        }

        /// <summary>
        /// クレジット入金確認画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>クレジット入金確認画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<GetCreditJournal_Result> list = new PaginatedList<GetCreditJournal_Result>();

            // 検索結果リストの取得
            if (criteriaInit)
            {
                //何もしない(初回表示)
            }
            else
            {
                switch (CommonUtils.DefaultString(form["RequestFlag"]))
                {

                    case "1": // 検索ボタン

                        //検索処理
                        return SearchStart(form, list);

                    case "2": // Excelボタン
                        return Download(form);


                    default:  // 初期表示(クリアボタン)
                        // 検索項目の設定
                        SetComponent(form);

                        // 検索画面の表示
                        return View("CreditJournalCheckCriteria", list);
                }

            }
            // 検索項目の設定
            SetComponent(form);
            return View("CreditJournalCheckCriteria", list);
        }

        #region 検索ボタン押下
        /// <summary>
        /// 検索ボタン押下処理
        /// </summary>
        /// <param name="form"></param>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SearchStart(FormCollection form, PaginatedList<GetCreditJournal_Result> list)
        {
            //検索条件SET
            GetCreditJournalSearchCondition Condition = new GetCreditJournalSearchCondition();
            Condition = GetSearchCondition(form);

            //検索
            list = GetSearchResultList(form, Condition);

            //画面コンポーネントSET
            SetComponent(form);

            return View("CreditJournalCheckCriteria", list);

        }
        #endregion

        #region　クレジット入金状況検索
        private PaginatedList<GetCreditJournal_Result> GetSearchResultList(FormCollection form, GetCreditJournalSearchCondition Condition)
        {           
            return new JournalDao(db).GetCreditJournal(Condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region Excelボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCEL);

            List<CarPurchase> list = new List<CarPurchase>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名(yyyyMMdd(ダウンロード年月日)_クレジット入金確認)
            string fileName = DownLoadTime + "_クレジット入金確認" + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイル取得
            string tfilePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CreditJournalCheck"]) ? "" : ConfigurationManager.AppSettings["CreditJournalCheck"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePath.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form);
                return View("CarPurchaseCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePath);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        //Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加
        #region エクセルデータ作成(テンプレートファイルあり)
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
            GetCreditJournalSearchCondition Condition = new GetCreditJournalSearchCondition();
            Condition = GetSearchCondition(form);
            DataTable dtCondtion = MakeConditionRow(Condition);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //検索結果取得
            List<GetCreditJournal_ExcelResult> list = new JournalDao(db).GetCreditJournalForExcel(Condition);

            //データ設定
            ret = dExport.SetData<GetCreditJournal_ExcelResult, GetCreditJournal_ExcelResult>(ref excelFile, list, configLine);

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
        #endregion

        //Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加
        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(GetCreditJournalSearchCondition condition)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            //---------------------
            //　列定義
            //---------------------
            //１つの列を設定  
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();


            if (!string.IsNullOrEmpty(condition.JournalDateFrom) || !string.IsNullOrEmpty(condition.JournalDateTo))
            {
                conditionText += string.Format("決済日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.JournalDateFrom, condition.JournalDateTo);
            }
            if (!string.IsNullOrEmpty(condition.SalesDateFrom) || !string.IsNullOrEmpty(condition.SalesDateTo))
            {
                conditionText += string.Format("納車日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.SalesDateFrom, condition.SalesDateTo);
            }
            if (!string.IsNullOrEmpty(condition.SlipType))
            {
                string Name = new CodeDao(db).GetCodeNameByKey("014", condition.SlipType, false).Name;
                conditionText += string.Format("伝票タイプ={0}　", Name);
            }
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                string DepartmentName = new DepartmentDao(db).GetByKey(condition.DepartmentCode, false).DepartmentName;

                conditionText += string.Format("部門コード={0}:{1}　", condition.DepartmentCode, DepartmentName);
            }
            if (!string.IsNullOrEmpty(condition.CustomerCode))
            {
                string CustomerName = new CustomerDao(db).GetByKey(condition.CustomerCode, false).CustomerName;

                conditionText += string.Format("顧客コード(決済者）={0}:{1}　", condition.CustomerCode, CustomerName);
            }
            if (!string.IsNullOrEmpty(condition.CustomerClaimCode))
            {
                string CustomerClaimName = new CustomerClaimDao(db).GetByKey(condition.CustomerClaimCode, false).CustomerClaimName;

                conditionText += string.Format("請求先コード = {0}:{1}　", condition.CustomerClaimCode, CustomerClaimName);
            }
            if (!string.IsNullOrEmpty(condition.CompleteFlag))
            {
                string Name = new CodeDao(db).GetCodeNameByKey("017", condition.CompleteFlag, false).Name;
                conditionText += string.Format("入金状況={0}　", Name);
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region 検索条件SET
        private GetCreditJournalSearchCondition GetSearchCondition(FormCollection form)
        {
            GetCreditJournalSearchCondition Condition = new GetCreditJournalSearchCondition();

            Condition.JournalDateFrom = form["JournalDateFrom"];
            Condition.JournalDateTo = form["JournalDateTo"];
            Condition.SalesDateFrom = form["SalesDateFrom"];
            Condition.SalesDateTo = form["SalesDateTo"];
            Condition.SlipType = form["SlipType"];
            Condition.SlipNumber = form["SlipNumber"];
            Condition.DepartmentCode = form["DepartmentCode"];
            Condition.CustomerCode = form["CustomerCode"];
            Condition.CustomerClaimCode = form["CustomerClaimCode"];
            Condition.CompleteFlag = form["CompleteFlag"];

            return Condition;
        }
        #endregion

        #region 画面コンポーネント
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["JournalDateFrom"] = form["JournalDateFrom"];
            ViewData["JournalDateTo"] = form["JournalDateTo"];
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["SlipType"] = form["SlipType"];
            ViewData["SlipTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("014", false), form["SlipType"], true);
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {      
                ViewData["DepartmentName"] = "";    //未入力なら空文字
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //入力済みならマスタから検索
            }

            ViewData["CustomerCode"] = form["CustomerCode"];
            if (string.IsNullOrEmpty(form["CustomerCode"]))
            {
                ViewData["CustomerName"] = "";    //未入力なら空文字
            }
            else
            {
                ViewData["CustomerName"] = new CustomerDao(db).GetByKey(form["CustomerCode"].ToString()).CustomerName;  //入力済みならマスタから検索
            }

            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            if (string.IsNullOrEmpty(form["CustomerClaimCode"]))
            {
                ViewData["CustomerClaimName"] = "";    //未入力なら空文字
            }
            else
            {
                ViewData["CustomerClaimName"] = new CustomerClaimDao(db).GetByKey(form["CustomerClaimCode"].ToString()).CustomerClaimName;  //入力済みならマスタから検索
            }

            ViewData["CompleteFlag"] = form["CompleteFlag"];
            ViewData["CompleteFlagList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("017", false), form["CompleteFlag"], true); ;


        }
        #endregion

        #region 検索中アニメーション
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>処理完了</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retParts = new Dictionary<string, string>();

                retParts.Add("ProcessedTime", "処理完了");

                return Json(retParts);
            }
            return new EmptyResult();
        }

        #endregion
    }
}