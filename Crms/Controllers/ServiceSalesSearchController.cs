using CrmsDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Crms.Models;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
//Add 2015/09/15 arc nakayama #3165_サービス集計表対応
using OfficeOpenXml;
using System.Data;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceSalesSearchController : Controller
    {
        #region 初期化

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceSalesSearchController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 定数
        private const string SERVICE_SALES = "001";  // サービス売上
        private const string OFFICE_SALES = "002";   // サービス売上(社内営業)

        private static readonly string FORM_NAME = "サービス売上検索";     // 画面名
        private static readonly string PROC_NAME_SEARCH = "サービス売上検索";     // 処理名(検索)
        private static readonly string PROC_NAME_CSV = "サービス売上CSV出力";     // 処理名(CSV出力)

        #endregion

        #region 画面表示

        /// <summary>
        /// 画面表示
        /// </summary>
        /// <returns>部品検索画面</returns>
        //[AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// サービス売上検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービス売上検索画面</returns>
        //[AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            string targetName = "";

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {

                case "1": // 検索ボタン
                    return Search(form);

                case "2": // CSVボタン
                    targetName = GetTargetCsvName(form);
                    return Download(form, targetName);

                default:  // 初期表示
                    ViewData["SearchList"] = null;
                    // 検索項目の設定
                    SetComponent(form);

                    // 検索画面の表示
                    return View("ServiceSalesSearchCriteria");
            }
        }
        /// <summary>
        /// ExportList.xmlのID取得(CSV)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private string GetTargetCsvName(FormCollection form)
        {
            string targetName = "";
            string exportName = CommonUtils.DefaultString(form["ExportName"]);

            if (exportName.Equals(SERVICE_SALES))
            {
                // サービス売上の場合
                targetName = "ServiceSalesCsv";

            }
            else if (exportName.Equals(OFFICE_SALES))
            {
                // サービス売上(社内)の場合
                targetName = "OfficeSalesCsv";
            }

            return targetName;
        }
        #endregion

        #region 検索ボタン押下
        /// <summary>
        /// 検索ボタン押下処理
        /// </summary>
        /// <param name="form"></param>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Search(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            switch (CommonUtils.DefaultString(form["ExportName"]))
            {
                case SERVICE_SALES: // サービス売上
                    ViewData["SearchList"] = SearchServiceSales(form);
                    break;

                case OFFICE_SALES: // サービス売上(社内)
                    ViewData["SearchList"] = SearchServiceSalesOffice(form);
                    break;

                default:
                    ViewData["SearchList"] = null;
                    break;
            }

            // 検索項目の設定
            SetComponent(form);

            // 検索画面の表示
            return View("ServiceSalesSearchCriteria");
        }

        #endregion

        #region CSVボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form, string targetName)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            List<V_ServiceSalesDownload> serviceSalesList = null;
            List<V_OfficeSalesDownload> officeSalesList = null;
            SeparatedValueWriter writer = new SeparatedValueWriter(FieldSeparator.CSV);
            IEnumerable<XElement> data = GetFieldList(targetName);
            ContentResult contentResult = new ContentResult();

            //出力フィールド定義データがなければ中止
            if (data == null)
            {
                return contentResult;
            }

            //Add 2015/01/16 arc nakayama #3150_勘定奉行データファイル名変更対応(検索項目をファイル名にする)
            string fileName = MakeFileName(form); //ファイル名作成

            switch (CommonUtils.DefaultString(form["ExportName"]))
            {
                case SERVICE_SALES: // サービス売上
                    serviceSalesList = SearchServiceSalesCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(serviceSalesList, data, true));
                    break;

                case OFFICE_SALES: // サービス売上(社内営業)
                    officeSalesList = SearchServiceSalesOfficeCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(officeSalesList, data, true));
                    break;

                default:
                    break;
            }

            return contentResult;
        }

        /// <summary>
        /// フィールド定義データから出力フィールドリストを取得する
        /// </summary>
        /// <param name="documentName">帳票名</param>
        /// <returns></returns>
        private IEnumerable<XElement> GetFieldList(string documentName)
        {
            XDocument xml = XDocument.Load(Server.MapPath("/Models/ExportFieldList.xml"));
            var query = (from x in xml.Descendants("Title")
                         where x.Attribute("ID").Value.Equals(documentName)
                         select x).FirstOrDefault();
            if (query == null)
            {
                return null;
            }
            else
            {
                var list = from a in query.Descendants("Name") select a;
                return list;
            }
        }

        /// <summary>
        /// CSVを作成しダウンロードダイアログを表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Excel出力対象データリスト</param>
        /// <param name="fieldList">出力フィールドリスト</param>
        private ContentResult CsvDownload(string fileName, string buffer)
        {
            // ファイル名が日本語でも文字化けしない様にする為、UrlEncodeを使用
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(fileName));
            Response.ContentType = "text/csv";
            System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("Shift_JIS");

            return Content(buffer, "text/csv", encoding);
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

            ViewData["RequestFlag"] = form["RequestFlag"];
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            ViewData["SalesJournalYearFromList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["SalesJournalYearFrom"], false);
            ViewData["SalesJournalYearToList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["SalesJournalYearTo"], false);
            ViewData["SalesJournalMonthFromList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["SalesJournalMonthFrom"], false);
            ViewData["SalesJournalMonthToList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["SalesJournalMonthTo"], false);
            ViewData["SalesJournalYearFrom"] = form["SalesJournalYearFrom"];
            ViewData["SalesJournalYearTo"] = form["SalesJournalYearTo"];
            ViewData["SalesJournalMonthFrom"] = form["SalesJournalMonthFrom"];
            ViewData["SalesJournalMonthTo"] = form["SalesJournalMonthTo"];
            ViewData["ExportName"] = form["ExportName"];
            ViewData["ExportList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("012", false), form["ExportName"], false);
            ViewData["DivisionTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("002", false), form["DivisionType"], true);

        }
        #endregion

        #region サービス売上
        /// <summary>
        /// サービス売上の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_ServiceSalesDownload> SearchServiceSales(FormCollection form)
        {

            V_ServiceSalesDownloadDao V_ServiceSalesDao = new V_ServiceSalesDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime? FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strToDate = string.Format("{0:yyyy/MM/dd}", ToDate); //yyyy/MM/ddで日付を取得
            strToDate = strToDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = FromDate;
            condition.SalesDateTo = convertDateTime(strToDate, true);　//末日を取得する

            condition.DivisionType = form["DivisionType"];

            return V_ServiceSalesDao.GetListServiceSales(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// サービス売上のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ServiceSalesDownload> SearchServiceSalesCsv(FormCollection form)
        {

            V_ServiceSalesDownloadDao V_ServiceSalesDao = new V_ServiceSalesDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strToDate = string.Format("{0:yyyy/MM/dd}", ToDate); //yyyy/MM/ddで日付を取得
            strToDate = strToDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = FromDate;
            condition.SalesDateTo = convertDateTime(strToDate, true);　//末日を取得する

            condition.DivisionType = form["DivisionType"];

            return V_ServiceSalesDao.GetListServiceSalesCsv(condition);
        }
        #endregion

        #region サービス売上(社内営業)
        /// <summary>
        /// サービス売上(社内営業)の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_OfficeSalesDownload> SearchServiceSalesOffice(FormCollection form)
        {

            V_OfficeSalesDownloadDao V_SalesOfficeDao = new V_OfficeSalesDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime? FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strToDate = string.Format("{0:yyyy/MM/dd}", ToDate); //yyyy/MM/ddで日付を取得
            strToDate = strToDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = FromDate;
            condition.SalesDateTo = convertDateTime(strToDate, true);　//末日を取得する

            condition.DivisionType = form["DivisionType"];

            return V_SalesOfficeDao.GetListServiceSalesOffice(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// サービス売上(社内営業)のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_OfficeSalesDownload> SearchServiceSalesOfficeCsv(FormCollection form)
        {

            V_OfficeSalesDownloadDao V_SalesOfficeDao = new V_OfficeSalesDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime? FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strToDate = string.Format("{0:yyyy/MM/dd}", ToDate); //yyyy/MM/ddで日付を取得
            strToDate = strToDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = FromDate;
            condition.SalesDateTo = convertDateTime(strToDate, true);　//末日を取得する

            condition.DivisionType = form["DivisionType"];

            return V_SalesOfficeDao.GetListServiceSalesOfficeCsv(condition);
        }
        #endregion

        #region 日付変換
        /// <summary>
        /// 日付変換(NULL許可)
        /// </summary>
        /// <param name="convDate">変換対象日付</param>
        /// <param name="lastFlag">末日フラグ</param>
        /// <returns>変換後日付</returns>
        private DateTime? convertDateTime(string convDate, bool lastFlag)
        {
            DateTime date = DateTime.Now;

            if (DateTime.TryParse(convDate + "/01", out date))
            {
                if (lastFlag == true)
                {
                    //末日
                    date = date.AddMonths(1).AddDays(-1);
                }
            }
            else
            {
                return null;
            }

            return date;
        }

        /// <summary>
        /// 日付変換(NULL禁止)
        /// </summary>
        /// <param name="convDate">変換対象日付</param>
        /// <param name="lastFlag">末日フラグ</param>
        /// <returns>変換後日付</returns>
        private DateTime convertDateTimeNotNULL(string convDate, bool lastFlag)
        {
            DateTime date = DateTime.Now;

            if (DateTime.TryParse(convDate + "/01", out date))
            {
                if (lastFlag == true)
                {
                    //末日
                    date = date.AddMonths(1).AddDays(-1);
                }
            }
            return date;
        }
        #endregion

        // Add 2015/01/19 arc nakayama 勘定奉行データファイル名変更対応
        #region CSVのファイル名作成
        /// <summary>
        /// CSVのファイル名作成
        /// </summary>
        /// <param name="convDate">フォームデータ</param>
        /// <returns>ファイル名(****.csv)</returns>
        private string MakeFileName(FormCollection form, bool TxtFlag = false)
        {
            string prefix = "";     //検索項目
            string fileName = "";   //ファイル名
            string dateTime = string.Format("{0:yyyyMMdd}", DateTime.Now);  //当日の日付


            // 出力
            prefix = (new CodeDao(db).GetCodeName2("012", form["ExportName"])).ShortName; // DivisionType から名称取得  

            // 日付は初期値が入っているため、出力ごとに分ける
            // サービス売上/サービス売上(社内営業) の場合は納車日をファイル名にする
            if (form["ExportName"] == SERVICE_SALES || form["ExportName"] == OFFICE_SALES)
            {
                CodeDao dao = new CodeDao(db);
                // 納車日From
                DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
                string strFromDate = string.Format("{0:yyyy/MM/dd}", FromDate); //yyyy/MM/ddで日付を取得
                string SalesJournalDateFrom = strFromDate.Substring(0, 7);　//yyyy/MMまでを抽出
                prefix = prefix + "_" + SalesJournalDateFrom;

                // 納車日To
                DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
                string strToDate = string.Format("{0:yyyy/MM/dd}", ToDate); //yyyy/MM/ddで日付を取得
                string SalesJournalDateTo = strToDate.Substring(0, 7);　//yyyy/MMまでを抽出
                prefix = prefix + "～" + SalesJournalDateTo;
            }

            // 拠点 
            if (!string.IsNullOrEmpty(form["DivisionType"]))
            {
                string DivisionName = (new CodeDao(db).GetCodeName2("002", form["DivisionType"])).Name; // DivisionType から名称取得
                prefix = prefix + "_" + DivisionName;
            }

            if (TxtFlag == true)
            {
                fileName = dateTime + "_" + prefix + ".txt"; // ファイル名を連結させる(テキストファイル(仕訳出力))
            }
            else
            {
                fileName = dateTime + "_" + prefix + ".csv"; // ファイル名を連結させる(CSVファイル(CSV出力))
            }
            //Add 2015/01/22 arc nakayama　禁則文字がファイル名に含まれていないか最終チェックを行う
            fileName = DeleteSlash(fileName);
            return fileName;
        }
        #endregion

        #region 日付のスラッシュを削除してYYYYMだった場合はYYYYMMに変換する
        // Add 2015/01/19 arc nakayama 勘定奉行データファイル名変更対応
        /// <summary>
        /// 日付のスラッシュを削除してYYYYMだった場合はYYYYMMに変換する
        /// </summary>
        /// <param name="postcode">日付（YYYY/MMまたはYYYYM）</param>
        /// <returns></returns>

        public static string DeleteSlash(string prefix, bool DateFlag = false)
        {
            // Mod 2015/01/22 arc nakayama ファイル名に禁則文字(\ / : * ? " < > |)が含まれていたら削除する
            prefix = prefix.Replace("/", "");
            prefix = prefix.Replace("\\", "");
            prefix = prefix.Replace(":", "");
            prefix = prefix.Replace("*", "");
            prefix = prefix.Replace("?", "");
            prefix = prefix.Replace("\"", "");
            prefix = prefix.Replace("<", "");
            prefix = prefix.Replace(">", "");
            prefix = prefix.Replace("|", "");

            // Mod 2015/01/22 arc nakayama 日付データの場合のみ形式チェックを行う
            if (DateFlag == true)
            {
                // 6桁(YYYYMM)でなければ５桁目に0(ゼロ)を挿入
                if (!Regex.IsMatch(prefix.ToString(), @"\d{6}"))
                {
                    prefix = prefix.Insert(4, "0");
                }
            }

            return prefix;
        }

        #endregion

        #region 処理中かどうかを取得する。(Ajax専用）
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

        #region サービス集計表
        /// <summary>
        /// サービス集計表検索ダイアログ
        /// </summary>
        /// <returns>サービス集計表検索ダイアログ</returns>
        public ActionResult ServiceSalesReportCriteriaDialog()
        {
            FormCollection form = new FormCollection();

            if (Request["SalesDateFrom"] != null)
            {
                form["SalesDateFrom"] = string.Format("{0:yyyy/MM/dd}", Request["SalesDateFrom"].ToString());
            }
            if (Request["SalesDateTo"] != null)
            {
                form["SalesDateTo"] = string.Format("{0:yyyy/MM/dd}", Request["SalesDateTo"]);
            }
            if (Request["DepartmentCode"] != null)
            {
                form["DepartmentCode"] = Request["DepartmentCode"];
            }
            if (Request["WorkType"] != null)
            {
                form["WorkType"] = Request["WorkType"];
            }
            if (Request["RequestFlag"] != null)
            {
                form["RequestFlag"] = Request["RequestFlag"];
            }

            return ServiceSalesReportCriteriaDialog(form);
        }

        //2015/09/08 arc nakayama サービス集計表対応　エントリー画面追加 
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ServiceSalesReportCriteriaDialog(FormCollection form)
        {
            ServiceSalesAmount list = new ServiceSalesAmount();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                case "1": // 検索ボタン

                    list = SalesReportSearch(form);
                    SetDataCriteriaComponent(form);
                    return View("ServiceSalesReportCriteriaDialog", list);

                case "2": // Excelボタン
                    return ExcelDownload(form);

                default:  // 初期表示
                    // 検索項目の設定
                    SetDataCriteriaComponent(form);
                    // 検索画面の表示
                    return View("ServiceSalesReportCriteriaDialog", list);
            }

        }

        #region 検索処理
        public ServiceSalesAmount SalesReportSearch(FormCollection form)
        {
            ServiceSalesReportDao Dao = new ServiceSalesReportDao(db);
            ServiceSalesAmount Ret = new ServiceSalesAmount();

            ServiceSalesSearchCondition condition = Setcondition(form);

            Ret = Dao.GetSummaryByCondition(condition);
            Ret.plist = new PaginatedList<GetServiceSalesReportResult>(Ret.list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return Ret;

        }
        #endregion

        #region 検索条件セット
        public ServiceSalesSearchCondition Setcondition(FormCollection form)
        {
            ServiceSalesSearchCondition condition = new ServiceSalesSearchCondition();
            condition.SalesDateFrom = form["SalesDateFrom"];
            condition.SalesDateTo = form["SalesDateTo"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.WorkTypeCode = form["WorkType"];

            return condition;
        }
        #endregion

        #region 画面コンポーネントセット
        public void SetDataCriteriaComponent(FormCollection form)
        {
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {      
                ViewData["DepartmentName"] = "";
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;
            }
            ViewData["WorkType"] = Request["WorkType"];
            ViewData["WorkTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("016", false), form["WorkType"], true);

            return;
        }
        #endregion

        #region Excelボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExcelDownload(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名:サービス集計表_yyyyMMdd.xlsx
            string fileName = DownLoadTime + "_サービス集計表" + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        #region エクセルデータ作成(テンプレートファイルなし)
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルなし)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            string sheetName = "サービス集計表";    //シート名
            int dateType = 0;                       //データタイプ(帳票形式)
            string setPos = "A1";                   //設定位置
            bool ret = false;

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルなし)
            ExcelPackage excelFile = dExport.MakeExcel(fileName);

            //ワークシート追加
            var worksheet = excelFile.Workbook.Worksheets.Add(sheetName);

            //----------------------------
            // 検索条件出力
            //----------------------------
            //設定値
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //検索条件取得
            ServiceSalesSearchCondition condition = Setcondition(form);

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(condition);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // ヘッダ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A2";

            //設定値
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //ヘッダ行の取得
            IEnumerable<XElement> data = GetFieldList("ServiceSalesReportText");

            //取得したヘッダ行リストをデータテーブルに設定
            DataTable dtHeader = MakeHeaderRow(data);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtHeader, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A3";

            //設定値を取得
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //サービス集計表情報を取得
            List<ServiceSalesReportExcelResult> list = new ServiceSalesReportDao(db).GetSummaryByConditionForExcel(condition);

            //データ設定
            ret = dExport.SetData<ServiceSalesReportExcelResult, ServiceSalesReportExcelResult>(ref excelFile, list, configLine);
            //戻り値チェック


            excelData = excelFile.GetAsByteArray();

            return excelData;
        }
        #endregion

        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(ServiceSalesSearchCondition condition)
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

            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if (!string.IsNullOrEmpty(condition.SalesDateFrom) && string.IsNullOrEmpty(condition.SalesDateTo))
            {
                condition.SalesDateTo = condition.SalesDateFrom;
            }

            if (!string.IsNullOrEmpty(condition.SalesDateFrom) || !string.IsNullOrEmpty(condition.SalesDateTo))
            {
                conditionText += string.Format("納車日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.SalesDateFrom, condition.SalesDateTo);
            }
            if (!string.IsNullOrEmpty(condition.WorkTypeCode))
            {
                //categorycord = 016 （社内/社外）
                conditionText += string.Format("区分={0}　", new CodeDao(db).GetCodeName2("016", condition.WorkTypeCode).Name);
            }
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                conditionText += string.Format("部門コード={0}:{1}　", condition.DepartmentCode, new DepartmentDao(db).GetByKey(condition.DepartmentCode).DepartmentName);
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region ヘッダ行の作成(Excel出力用)
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

        #endregion
    }
}