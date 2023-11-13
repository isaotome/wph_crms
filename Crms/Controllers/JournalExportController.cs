using CrmsDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Crms.Models;
using System.IO;                //Add 2014/11/19 arc yano 自動仕分け　仕訳ファイル保存対応
using System.Configuration;     //Add 2014/11/19 arc yano 自動仕分け　仕訳ファイル保存対応
using System.Text.RegularExpressions;   // Add 2015/01/19 arc nakayama 勘定奉行データファイル名変更対応

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class JournalExportController : Controller
    {
        #region 初期化
        
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public JournalExportController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 定数
        private const string USED_CAR_EX_AA = "001"; // 中古車売上(オートオークション除く)
        private const string NEW_CAR_SALES = "002";  // 新車売上
        private const string USED_CAR_AA = "003";    // 中古車売上(オートオークション)
        private const string RECEIPT_TRANS = "006";  // 振込入金
        private const string PETTY_CASH = "007";     // 小口現金
        private const string RECEIPT_LOAN = "009";   // ローン入金

        private static readonly string FORM_NAME = "勘定奉行データ出力";     // 画面名
        private static readonly string PROC_NAME_SEARCH = "勘定奉行データ検索";     // 処理名(検索)
        private static readonly string PROC_NAME_CSV = "勘定奉行データCSV出力";     // 処理名(CSV出力)
        private static readonly string PROC_NAME_TEXT = "勘定奉行データ仕訳出力";   // 処理名(TEXT出力)
        //車両納車リスト

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
        /// 車両管理検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両管理検索画面</returns>
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

                case "3": // 仕訳ボタン
                    targetName = GetTargetTextName(form);
                    return JournalReport(form, targetName);

                default:  // 初期表示
                    ViewData["SearchList"] = null;
                    // 検索項目の設定
                    // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定  日付の初期値設定
                    form["SalesJournalYearFrom"] = DateTime.Today.Year.ToString().Substring(1, 3);
                    form["SalesJournalYearTo"] = DateTime.Today.Year.ToString().Substring(1, 3);
                    form["SalesJournalMonthFrom"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
                    form["SalesJournalMonthTo"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
                    form["JournalSalesYear"] = DateTime.Today.Year.ToString().Substring(1, 3);
                    form["JournalSalesMonth"] = DateTime.Today.Month.ToString().PadLeft(3, '0');

                    SetComponent(form);

                    // 検索画面の表示
                    return View("JournalExportCriteria");
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

            if(exportName.Equals(USED_CAR_EX_AA) || exportName.Equals(NEW_CAR_SALES) || exportName.Equals(USED_CAR_AA)) {
                // 中古車売上(AA除く) 新車売上 中古車売上(AA)の場合
                targetName = "CarSalesCsv";

            } else if (exportName.Equals(RECEIPT_TRANS) || exportName.Equals(PETTY_CASH) || exportName.Equals(RECEIPT_LOAN)) {
                // 振込入金 小口現金 ローン入金の場合
                targetName = "PaymentCsv";
            }

            return targetName;
        }

        /// <summary>
        /// ExportList.xmlのID取得(TEXT)
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private string GetTargetTextName(FormCollection form)
        {
            string targetName = "";
            string exportName = CommonUtils.DefaultString(form["ExportName"]);

            if (exportName.Equals(USED_CAR_EX_AA) || exportName.Equals(NEW_CAR_SALES) || exportName.Equals(USED_CAR_AA)) {
                // 中古車売上(AA除く) 新車売上 中古車売上(AA)の場合
                targetName = "CarSalesText";

            } else if (exportName.Equals(RECEIPT_TRANS) || exportName.Equals(PETTY_CASH) || exportName.Equals(RECEIPT_LOAN)) {
                // 振込入金 小口現金 ローン入金の場合
                targetName = "JournalText";
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

            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定　ドロップダウンリストになるめ、必須チェックを外す

            switch (CommonUtils.DefaultString(form["ExportName"]))
            {
                case USED_CAR_EX_AA: // 中古車売上(AA除く)
                    ViewData["SearchList"] = SearchUsedCarSalesNotAA(form);
                    break;

                case NEW_CAR_SALES: // 新車売上
                    ViewData["SearchList"] = SearchNewCar(form);
                    break;

                case USED_CAR_AA: // 中古車売上(AA)
                    ViewData["SearchList"] = SearchUsedCarSalesAA(form);
                    break;

                case RECEIPT_TRANS: // 振込入金
                    ViewData["SearchList"] = SearchTransfer(form);
                    break;

                case PETTY_CASH: // 小口現金
                    ViewData["SearchList"] = SearchPettyCash(form);
                    break;

                case RECEIPT_LOAN: // ローン入金
                    ViewData["SearchList"] = SearchListLoan(form);
                    break;

                default:
                    ViewData["SearchList"] = null;
                    break;
            }

            // 検索項目の設定
            SetComponent(form);

            // 検索画面の表示
            return View("JournalExportCriteria");
        }

         #endregion

        #region CSVボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form, string targetName)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            List<V_CarSalesDownload> carSalesList = null;
            List<V_ReceiptTransDownload> receiptTransList = null;
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
                case USED_CAR_EX_AA: // 中古車売上(AA除く)
                    carSalesList = SearchUsedCarCsvNotAA(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(carSalesList, data, true));
                    break;

                case NEW_CAR_SALES: // 新車売上
                    carSalesList = SearchNewCarCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(carSalesList, data, true));
                    break;

                case USED_CAR_AA: // 中古車売上(AA)
                    carSalesList = SearchUsedCarCsvAA(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(carSalesList, data, true));
                    break;

                case RECEIPT_TRANS: // 振込入金
                    receiptTransList = SearchTransferCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(receiptTransList, data, true));
                    break;

                case PETTY_CASH: // 小口現金
                    receiptTransList = SearchPettyCashCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(receiptTransList, data, true));
                    break;

                case RECEIPT_LOAN: // ローン入金
                    receiptTransList = SearchListLoanCsv(form);
                    contentResult = CsvDownload(fileName, writer.GetBuffer2(receiptTransList, data, true));
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

        // Mod 2014/11/19 arc yano サブシステム仕訳機能移行対応その２ 仕訳ファイル保存対応 リターン値をcontentsからviewに変更
        #region 仕訳ボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult JournalReport(FormCollection form, string targetName)
        {            
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_TEXT);

            ModelState.Clear();     //モデルのクリア

            List<V_CarSalesJournalReport> carSalesList = null;
            List<V_UsedCarAutoAuctionJournalReport> usedCarAAList = null;
            List<V_ReceiptTransferJournalReport> receiptTransList = null;
            List<V_PettyCashJournalReport> pettyCashList = null;
            List<V_ReceiptLoanJournalReport> loanList = null;

            SeparatedValueWriter writer = new SeparatedValueWriter(FieldSeparator.CSV);
            IEnumerable<XElement> data = GetFieldList(targetName);
            //ViewResult viewResult = new ViewResult();
            int ret = 0;

            //出力フィールド定義データがなければ中止
            if (data == null)
            {
                ModelState.AddModelError("", "出力フィールド定義データがありません。");
                // 検索項目の設定
                SetComponent(form);
                return View("JournalExportCriteria");
            }

            //Add 2015/01/22 arc nakayama #3150_勘定奉行データファイル名変更対応(検索項目をファイル名にする)
            string fileName = MakeFileName(form, true); //ファイル名作成

            switch (CommonUtils.DefaultString(form["ExportName"]))
            {
                case USED_CAR_EX_AA: // 中古車売上(AA除く)
                    carSalesList = SearchUsedCarJournalNotAA(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(carSalesList, data, false));
                    break;

                case NEW_CAR_SALES: // 新車売上
                    carSalesList = SearchNewCarJournal(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(carSalesList, data, false));
                    break;

                case USED_CAR_AA: // 中古車売上(AA)
                    usedCarAAList = SearchUsedCarJournalAA(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(usedCarAAList, data, false));
                    break;

                case RECEIPT_TRANS: // 振込入金
                    receiptTransList = SearchTransferJournal(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(receiptTransList, data, false));
                    break;

                case PETTY_CASH: // 小口現金
                    pettyCashList = SearchPettyCashJournal(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(pettyCashList, data, false));
                    break;

                case RECEIPT_LOAN: // ローン入金
                    loanList = SearchListLoanJournal(form);
                    ret = TextDownload(fileName, writer.GetBuffer2(loanList, data, false));
                    break;

                default:
                    break;
            }

            // 検索項目の設定
            SetComponent(form);
            return View("JournalExportCriteria");
        }

        // Mod 2014/11/19 arc yano サブシステム仕訳機能移行対応その２ 仕訳ファイル保存対応
        // Mod 2015/01/22 arc nakayama #3150_勘定奉行データファイル名変更対応(検索項目をファイル名にする)
        /// <summary>
        /// Textを作成し所定の場所にファイルを保存する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefix">仕訳データ種別</param>
        /// <param name="buffer">出力内容</param>
        private int TextDownload(string fileName, string buffer)
        {
            int ret = 0;
            
            /*
            // ファイル名が日本語でも文字化けしない様にする為、UrlEncodeを使用
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(fileName));
            Response.ContentType = "text/plain";
            System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("Shift_JIS");
            //return Content(buffer, "text/plain", encoding);
            */

            //web.configより、保存フォルダのパスを取得
            string path = ConfigurationManager.AppSettings["JournalReportFolderPath"];

            //パスが設定されていない場合は、終了
            if (string.IsNullOrWhiteSpace(path) == true)
            {
                ModelState.AddModelError("", "仕訳データの保存先が設定されていないため、保存を行いません。");
                ret = -1;
                return ret;
            }

            //--------------------------------------
            //ファイル保存処理
            //--------------------------------------
            // ファイルに保存する
            //MOD 2014/12/25 arc ishii 仕訳データ保存ディレクトリ変更　\wwwroot配下は使用しない
            //string directoryName = Server.MapPath(path);
            string directoryName = path;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            fileName = directoryName + fileName;

            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, System.Text.Encoding.GetEncoding("shift_jis"));

            try
            {
                sw.Write(buffer);
                ModelState.AddModelError("", "仕訳データを保存しました");
            }
            finally
            {
                //閉じる
                sw.Close();
            }

            return ret;
        }
        #endregion

        #region 入力チェック
        /// <summary>
        /// 検索時の入力チェック
        /// </summary>
        /// <param name="form"></param>
        private void ValidateSearch(FormCollection form)
        {
            ModelState.Clear();
            string exportName = CommonUtils.DefaultString(form["ExportName"]);

            if (exportName.Equals(USED_CAR_EX_AA) || exportName.Equals(NEW_CAR_SALES) || exportName.Equals(USED_CAR_AA))
            {
                /*
                 * 中古車売上(AA除く) 新車売上 中古車売上(AA) サービス売上 サービス売上(社内)
                 */
                // 納車日の必須チェック
                if (string.IsNullOrEmpty(form["SalesJournalDateFrom"]))
                {
                    ModelState.AddModelError("SalesJournalDateFrom", MessageUtils.GetMessage("E0001", "納車日"));
                    return;
                }

                /*
                 日付正当性チェック 日付変換できなかった場合、エラーとする
                 */
                // 納車日From
                if (convertDateTime(CommonUtils.DefaultString(form["SalesJournalDateFrom"]), false) == null)
                {
                    ModelState.AddModelError("SalesJournalDateFrom", MessageUtils.GetMessage("E0019", "納車日"));
                }

                // 納車日To(入力されていた場合のみチェック)
                if (string.IsNullOrEmpty(form["SalesJournalDateTo"]) == false)
                {
                    if (convertDateTime(CommonUtils.DefaultString(form["SalesJournalDateTo"]), false) == null)
                    {
                        ModelState.AddModelError("SalesJournalDateTo", MessageUtils.GetMessage("E0019", "納車日"));
                    }
                }
                
            }
            else if (exportName.Equals(RECEIPT_TRANS) || exportName.Equals(PETTY_CASH) || exportName.Equals(RECEIPT_LOAN))
            {
                /*
                 * 振込入金 小口現金 ローン入金
                 */
                // 伝票日付の必須チェック
                if (string.IsNullOrEmpty(form["JournalSalesDate"]))
                {
                    ModelState.AddModelError("JournalSalesDate", MessageUtils.GetMessage("E0001", "伝票日付"));
                    return;
                }

                /*
                 日付正当性チェック 日付変換できなかった場合、エラーとする
                 */
                // 伝票日付
                if (convertDateTime(CommonUtils.DefaultString(form["JournalSalesDate"]), false) == null)
                {
                    ModelState.AddModelError("JournalSalesDate", MessageUtils.GetMessage("E0019", "伝票日付"));
                }
            }            
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
            ViewData["JournalSalesYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["JournalSalesYear"], false);
            ViewData["JournalSalesMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["JournalSalesMonth"], false);
            ViewData["SalesJournalYearFrom"] = form["SalesJournalYearFrom"];
            ViewData["SalesJournalYearTo"] = form["SalesJournalYearTo"];
            ViewData["SalesJournalMonthFrom"] = form["SalesJournalMonthFrom"];
            ViewData["SalesJournalMonthTo"] = form["SalesJournalMonthTo"];
            ViewData["JournalSalesYear"] = form["JournalSalesYear"];
            ViewData["JournalSalesMonth"] = form["JournalSalesMonth"];

            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["CarPurchaseDateFrom"] = form["CarPurchaseDateFrom"];
            ViewData["CarPurchaseDateTo"] = form["CarPurchaseDateTo"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["ExportName"] = form["ExportName"];
            ViewData["ExportList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("001", false), form["ExportName"], false);
            
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("003", false), form["PurchaseStatus"], false);

            // 振込入金 小口現金 ローン入金の場合、拠点選択リストに空白を表示させない
            if (CommonUtils.DefaultString(form["ExportName"]).Equals(RECEIPT_TRANS)
                || CommonUtils.DefaultString(form["ExportName"]).Equals(PETTY_CASH)
                || CommonUtils.DefaultString(form["ExportName"]).Equals(RECEIPT_LOAN))
            {
                ViewData["DivisionTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("002", false), form["DivisionType"], false);
            }
            else
            {
                ViewData["DivisionTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("002", false), form["DivisionType"], true);
            }

            // 事業所名取得
            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                ViewData["OfficeName"] = (new OfficeDao(db)).GetByKey(form["OfficeCode"]).OfficeName;
            }

            // 部門名取得
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ViewData["DepartmentName"] = (new DepartmentDao(db)).GetByKey(form["DepartmentCode"]).DepartmentName;
            }

        }
        #endregion

        #region 中古車売上(AA除く)
        /// <summary>
        /// 中古車売上(AA除く)の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_CarSalesDownload> SearchUsedCarSalesNotAA(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);
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

            return V_CarSalesDao.GetListByUserdCarNAA(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// CSVデータ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_CarSalesDownload> SearchUsedCarCsvNotAA(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);
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

            return V_CarSalesDao.GetListUserdCarCsvNAA(condition);

        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2019/01/12 yano #3965 WE版新システム対応 動作確認時に発見した不具合の修正
        /// </history>
        private List<V_CarSalesJournalReport> SearchUsedCarJournalNotAA(FormCollection form)
        {
            V_CarSalesJournalReportDao journalDao = new V_CarSalesJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();
            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            // Mod 2019/01/12 yano #3965
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            //DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strFromDate = string.Format("{0:yyyy/MM/dd}", FromDate); //yyyy/MM/ddで日付を取得
            strFromDate = strFromDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = convertDateTime(strFromDate, false);
            condition.SalesDateTo = convertDateTime(strFromDate, true);
            condition.DivisionType = form["DivisionType"];

            return journalDao.GetListUsedCarJournalNAA(condition, form["ExportName"]);

        }

        #endregion

        #region 新車売上
        /// <summary>
        /// 新車売上の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_CarSalesDownload> SearchNewCar(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);

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

            return V_CarSalesDao.GetListByNewCar(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 新車売上のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_CarSalesDownload> SearchNewCarCsv(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);
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

            return V_CarSalesDao.GetListNewCarCsv(condition);
        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2019/01/12 yano #3965 WE版新システム対応 動作確認時に発見した不具合の修正
        /// </history>
        private List<V_CarSalesJournalReport> SearchNewCarJournal(FormCollection form)
        {
            V_CarSalesJournalReportDao journalDao = new V_CarSalesJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            // Mod 2019/01/12 yano #3965
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            //DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strFromDate = string.Format("{0:yyyy/MM/dd}", FromDate); //yyyy/MM/ddで日付を取得
            strFromDate = strFromDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = convertDateTime(strFromDate, false);
            condition.SalesDateTo = convertDateTime(strFromDate, true);
            condition.DivisionType = form["DivisionType"];

            return journalDao.GetListByNewCarJournal(condition, form["ExportName"]);

        }
        #endregion

        #region 中古車売上(AAのみ)
        /// <summary>
        /// 中古車売上(AA)の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_CarSalesDownload> SearchUsedCarSalesAA(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);

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

            return V_CarSalesDao.GetListByUserdCarAutoAuction(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 中古車売上(AA)のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_CarSalesDownload> SearchUsedCarCsvAA(FormCollection form)
        {
            V_CarSalesDownloadDao V_CarSalesDao = new V_CarSalesDownloadDao(db);

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

            return V_CarSalesDao.GetListUserdCarAutoAuctionCsv(condition);
        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        ///     2019/01/12 yano #3965 WE版新システム対応 動作確認時に発見した不具合の修正
        /// </history>

        private List<V_UsedCarAutoAuctionJournalReport> SearchUsedCarJournalAA(FormCollection form)
        {
            V_UsedCarAutoAuctionJournalReportDao journalDao = new V_UsedCarAutoAuctionJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            // Mod 2019/01/12 yano #3965
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearFrom"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthFrom"]).Name), 1);
            //DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["SalesJournalYearTo"]).Name), int.Parse(dao.GetMonth(true, form["SalesJournalMonthTo"]).Name), 1);
            string strFromDate = string.Format("{0:yyyy/MM/dd}", FromDate); //yyyy/MM/ddで日付を取得
            strFromDate = strFromDate.Substring(0, 7);　//yyyy/MMまでを抽出
            condition.SalesDateFrom = convertDateTime(strFromDate, false);
            condition.SalesDateTo = convertDateTime(strFromDate, true);

            return journalDao.GetListUsedCarJournalAA(condition, form["ExportName"]);

        }
        #endregion

        #region 振込入金
        /// <summary>
        /// 振込入金の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_ReceiptTransDownload> SearchTransfer(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListTransfer(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 振込入金のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ReceiptTransDownload> SearchTransferCsv(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListTransferCsv(condition);
        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ReceiptTransferJournalReport> SearchTransferJournal(FormCollection form)
        {
            V_ReceiptTransferJournalReportDao journalDao = new V_ReceiptTransferJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();
            List<V_ReceiptTransferJournalReport> list = new List<V_ReceiptTransferJournalReport>();
            string departmentCode = "";
            string journalDate = "";

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = FromDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.DivisionType = form["DivisionType"];

            list = journalDao.GetListReceiptTransJournal(condition, form["ExportName"]);

            // 「*」を表示するかの判断と設定
            for (int cnt = 0; cnt < list.Count;cnt++)
            {
                // 入金日または部門コードが前レコードと違う場合、「*」を設定
                if (departmentCode.Equals(list[cnt].DrDepartmentCode) == false || journalDate.Equals(list[cnt].JournalDate2) == false)
                {
                    list[cnt].Mark = "*";
                }

                departmentCode = list[cnt].DrDepartmentCode;
                journalDate = list[cnt].JournalDate2;
            }

            return list;

            //return journalDao.GetListReceiptTransJournal(condition, form["ExportName"]);

        }
        #endregion

        #region 小口現金
        /// <summary>
        /// 振込入金の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_ReceiptTransDownload> SearchPettyCash(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListPettyCash(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 振込入金のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ReceiptTransDownload> SearchPettyCashCsv(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();


            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListPettyCashCsv(condition);
        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_PettyCashJournalReport> SearchPettyCashJournal(FormCollection form)
        {
            V_PettyCashJournalReportDao journalDao = new V_PettyCashJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();
            List<V_PettyCashJournalReport> list = new List<V_PettyCashJournalReport>();
            string departmentCode = "";
            string journalDate = "";
            Decimal DrCalcAmount = Decimal.Zero;
            Decimal CrCalcAmount = Decimal.Zero;
            Decimal rate = Decimal.Zero;

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = FromDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.DivisionType = form["DivisionType"];
            // データ取得
            list = journalDao.GetListPettyCashJournal(condition, form["ExportName"]);

            // 「*」を表示するかの判断と設定
            for (int cnt = 0; cnt < list.Count; cnt++)
            {
                /*
                 * 借方税額と貸方税額を計算・設定する
                 */
                // 借方金額
                DrCalcAmount = list[cnt].DrCalcAmount;
                // 貸方税額
                CrCalcAmount = list[cnt].CrCalcAmount;

                // 消費税を取得する
                if (list[cnt].CarSalesRate != null)
                {
                    // 車両伝票の消費税を取得
                    rate = (decimal)list[cnt].CarSalesRate;
                }
                else if (list[cnt].ServiceSalesRate != null)
                {
                    // サービス伝票の消費税を取得
                    rate = (decimal)list[cnt].ServiceSalesRate;
                }
                else
                {
                    // 入金日基準の消費税を取得
                    rate = (decimal)(list[cnt].JournalRate == null ? 0m : (decimal)list[cnt].JournalRate);
                }
                
                // 消費税額を求める 計算式： (税込金額 * 税率) / (税率+100)
                list[cnt].DrTaxAmount = Decimal.Truncate(Decimal.Divide(Decimal.Multiply(DrCalcAmount, rate), (rate + 100m)));
                list[cnt].CrTaxAmount = Decimal.Truncate(Decimal.Divide(Decimal.Multiply(CrCalcAmount, rate), (rate + 100m)));
                

                // 入金日または部門コードが前レコードと違う場合、「*」を設定
                if (departmentCode.Equals(list[cnt].DrDepartmentCode) == false || journalDate.Equals(list[cnt].JournalDate2) == false)
                {
                    list[cnt].Mark = "*";
                }

                departmentCode = list[cnt].DrDepartmentCode;
                journalDate = list[cnt].JournalDate2;
            }

            return list;
        }
        #endregion

        #region ローン入金
        /// <summary>
        /// ローン入金の画面表示レコード検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<V_ReceiptTransDownload> SearchListLoan(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListLoan(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ローン入金のCSVデータ検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ReceiptTransDownload> SearchListLoanCsv(FormCollection form)
        {

            V_ReceiptTransDownloadDao V_ResTransDao = new V_ReceiptTransDownloadDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            ToDate = ToDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;
            condition.OfficeCode = form["OfficeCode"];
            condition.DivisionType = form["DivisionType"];

            return V_ResTransDao.GetListLoanCsv(condition);
        }

        /// <summary>
        /// 仕訳データ取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private List<V_ReceiptLoanJournalReport> SearchListLoanJournal(FormCollection form)
        {
            V_ReceiptLoanJournalReportDao journalDao = new V_ReceiptLoanJournalReportDao(db);
            JournalExPortsCondition condition = new JournalExPortsCondition();

            CodeDao dao = new CodeDao(db);
            // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
            // 出力されるデータはFromで指定した月のみであるため、1日～末日はFromの値を設定
            DateTime FromDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
            DateTime ToDate = FromDate.AddMonths(1).AddDays(-1);
            condition.JournalDateFrom = FromDate;
            condition.JournalDateTo = ToDate;

            return journalDao.GetListLoanJournal(condition, form["ExportName"]);

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
            prefix = (new CodeDao(db).GetCodeName2("001", form["ExportName"])).ShortName; // DivisionType から名称取得  

            // 日付は初期値が入っているため、出力ごとに分ける
            // 中古車売上(AA除く)/新車売上/中古車売上(AA)/サービス売上/サービス売上(社内営業) の場合は納車日をファイル名にする
            if (form["ExportName"] == USED_CAR_EX_AA || form["ExportName"] == NEW_CAR_SALES || form["ExportName"] == USED_CAR_AA)
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

            // 振込入金/小口現金/ローン入金の場合は伝票日付をファイル名にする
            if (form["ExportName"] == RECEIPT_TRANS || form["ExportName"] == PETTY_CASH || form["ExportName"] == RECEIPT_LOAN)
            {
                    CodeDao dao = new CodeDao(db);
                    DateTime TargetDate = new DateTime(int.Parse(dao.GetYear(true, form["JournalSalesYear"]).Name), int.Parse(dao.GetMonth(true, form["JournalSalesMonth"]).Name), 1);
                    string strTargetDate = string.Format("{0:yyyy/MM/dd}", TargetDate); //yyyy/MM/ddで日付を取得
                    //ファイル名に/(スラッシュ)は使用できないので取る
                    string JournalSalesDate = strTargetDate.Substring(0, 7);　//yyyy/MMまでを抽出
                    prefix = prefix + "_" + JournalSalesDate;
                
            }


            // 拠点 
            if (!string.IsNullOrEmpty(form["DivisionType"]))
            {
                string DivisionName = (new CodeDao(db).GetCodeName2("002", form["DivisionType"])).Name; // DivisionType から名称取得
                prefix = prefix + "_" + DivisionName;
            }


            // 事業所
            // 振込入金/小口現金/ローン入金の場合は事業所をファイル名にする
            if (form["ExportName"] == RECEIPT_TRANS || form["ExportName"] == PETTY_CASH || form["ExportName"] == RECEIPT_LOAN)
            {
                if (!string.IsNullOrEmpty(form["OfficeCode"]))
                {
                    string OfficeName = (new OfficeDao(db)).GetByKey(form["OfficeCode"]).OfficeName;
                    prefix = prefix + "_" + OfficeName;
                }
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
    }
}
