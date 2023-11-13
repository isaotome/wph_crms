using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CrmsDao;
using Crms.Models;
using System.Xml.Linq;

//Add 2015/03/18 arc yano 現金出納帳出力(エクセル)対応
using OfficeOpenXml;
using System.Configuration;
using System.IO;

namespace Crms.Controllers
{
    //Mod 2015/02/23 arc yano 現金出納帳出力 クラス名の変更(T_CashJournalOutput → T_CashJournalOutput)
    /// <summary>
    /// 現金出納帳出力コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CashJournalOutputController : Controller
    {
         #region 定数
        private static readonly string FORM_NAME = "現金出納帳出力";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "現金出納帳出力検索"; // 処理名（ログ出力用）　
        private static readonly string PROC_NAME_CSV = "現金出納帳出力CSV出力";       // 処理名(CSV出力)
        private static readonly string FILE_NAME = "CashJournalOutputText";  //CSV出力時のファイル名
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
        public CashJournalOutputController()
        {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion


        /// <summary>
        /// 現金出納帳出力検索画面表示
        /// </summary>
        /// <returns>現金出納帳出力検索画面</returns>
        //[AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);    //初期値に当日の年をセットする
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');  //初期値に当日の月をセットする
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            return Criteria(form);
        }

        /// <summary>
        /// 現金出納帳検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>現金出納帳検索画面</returns>
        //[AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<T_CashJournalOutput> list = new PaginatedList<T_CashJournalOutput>();

            ActionResult ret;


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
                        ret =  SearchStart(form, list);
                        break;

                    case "2": // CSVボタン
                       ret =  Download(form);
                       break;

                    case "3": // CSVボタン
                       ret =  ExcelDownload(form);
                       break;

                    default:  // 初期表示(クリアボタン)
                        // 検索項目の設定
                        SetComponent(form);

                        // 検索画面の表示
                        ret =  View("CashJournalOutputCriteria", list);
                        break;
                }

                return ret;

            }
            // 検索項目の設定
            SetComponent(form);
            return View("CashJournalOutputCriteria", list);
        }


        #region 検索ボタン押下
        /// <summary>
        /// 検索ボタン押下処理
        /// </summary>
        /// <param name="form"></param>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SearchStart(FormCollection form, PaginatedList<T_CashJournalOutput> list)
        {
            //画面設定
            SetComponent(form);

            //現金出納帳検索
            list = GetSearchResultList(form);

            return View("CashJournalOutputCriteria", list);

        }
        #endregion



        #region　現金出納帳検索
        private PaginatedList<T_CashJournalOutput> GetSearchResultList(FormCollection form)
         {
             T_CashJournalOutput T_CashJournalOutputCondition = new T_CashJournalOutput();
            CodeDao dao = new CodeDao(db);

            //検索年月取得
            //Mod 2015/02/23 arc yano 現金出納帳 抽出条件バグ修正
            DateTime targetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる    
            //DateTime dt2 = targetDate.AddDays(-1);
            //検索項目セット
            //T_CashJournalOutputCondition.Lastdate = dt2;                                //対象年月
            T_CashJournalOutputCondition.Lastdate = targetDate;                           
            T_CashJournalOutputCondition.OfficeCode = form["OfficeCode"];                 //事務所コード
            T_CashJournalOutputCondition.CashAccountCode = form["CashAccountCode"];       //現金口座コード
            T_CashJournalOutputCondition.OfficeName = form["OfficeName"];
            //事務所名
            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null)
                {
                    T_CashJournalOutputCondition.OfficeName = office.OfficeName;
                }
            }
            //現金口座名
            if (!string.IsNullOrEmpty(form["CashAccountCode"]))
            {
                CashAccount CashAccount = new CashAccountDao(db).GetByKey(form["OfficeCode"], form["CashAccountCode"]);
                if (CashAccount != null)
                {
                    T_CashJournalOutputCondition.CashAccountName = CashAccount.CashAccountName;
                }
            }


            return new T_CashJournalOutputDao(db).GetJournalOutputByCondition(T_CashJournalOutputCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        #endregion



        #region CSVボタン押下
       [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            List<T_CashJournalOutput> CashJournalOutputList = null;
            SeparatedValueWriter writer = new SeparatedValueWriter(FieldSeparator.CSV);
            IEnumerable<XElement> data = GetFieldList(FILE_NAME);
            ContentResult contentResult = new ContentResult();

            DocumentExportCondition T_CashJournalOutputCondition = new DocumentExportCondition();
            CodeDao dao = new CodeDao(db);

            //検索年月取得
            //Mod 2015/02/23 arc yano 現金出納帳 抽出条件バグ修正
            //検索年月取得
            DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる    
           // DateTime dt2 = TargetDate.AddDays(-1);
            //検索項目セット
            T_CashJournalOutputCondition.TargetDate = TargetDate;                         //対象年月
            //T_CashJournalOutputCondition.Lastdate = dt2;                                  //前月末日
            T_CashJournalOutputCondition.Lastdate = TargetDate;                           //前月末日→対象年月に変更
            T_CashJournalOutputCondition.OfficeCode = form["OfficeCode"];                 //事務所コード
            T_CashJournalOutputCondition.CashAccountCode = form["CashAccountCode"];       //現金口座コード
            T_CashJournalOutputCondition.OfficeName = form["OfficeName"];
            //事務所名
            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null)
                {
                    T_CashJournalOutputCondition.OfficeName = office.OfficeName;
                }
            }
            //現金口座名
            if (!string.IsNullOrEmpty(form["CashAccountCode"]))
            {
                CashAccount CashAccount = new CashAccountDao(db).GetByKey(form["OfficeCode"], form["CashAccountCode"]);
                if (CashAccount != null)
                {
                    T_CashJournalOutputCondition.CashAccountName = CashAccount.CashAccountName;
                }
            }

            //出力フィールド定義データがなければ中止
            if (data == null)
            {
                return contentResult;
            }


            CashJournalOutputList = new T_CashJournalOutputDao(db).GetCashJournalOutputByConditionForCSV(T_CashJournalOutputCondition);
            contentResult = CsvDownload("現金出納帳出力", writer.GetBuffer(CashJournalOutputList, data, T_CashJournalOutputCondition));

            return contentResult;
        }
        #endregion

       //Mod 2015/03/18 arc yano 
        #region Excel出力ボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExcelDownload(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            ContentResult contentResult = new ContentResult();
            CodeDao dao = new CodeDao(db);

            PaginatedList<T_CashJournalOutput> list = new PaginatedList<T_CashJournalOutput>();

           
            //ファイル名(ReceiptNoteBook_xxx(部門コード)_yyyyMM(対象年月)_yyyyMMddhhmiss(ダウンロード時刻))
            string fileName = "ReceiptNoteBook" + "_" + form["OfficeCode"] + "_" + dao.GetYear(true, form["TargetDateY"]).Name + dao.GetMonth(true, form["TargetDateM"]).Name
                + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryForCashJournal"]) ? "" : ConfigurationManager.AppSettings["TemporaryForCashJournal"];

            string filePathName = filePath + fileName;

            //テンプレートファイル取得
            string tFileName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCashJournal"]) ? "" : ConfigurationManager.AppSettings["TemplateForCashJournal"];

            //テンプレートファイルのパスが設定されていない場合
            if (tFileName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form);
                return View("CashJournalOutputCriteria", list);
            }
            
            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tFileName);
                                   
            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("CashJournalOutputCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// エクセルデータ作成
        /// </summary>
        /// <param name="documentName">帳票名</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2018/04/02 arc yano #3876 現金出納帳出力　六口座対応
        /// </history>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tFileName)
        {
            //データ取得
            CashBalanceDao cbDao = new CashBalanceDao(db);
            JournalDao joDao = new JournalDao(db);
            CodeDao dao = new CodeDao(db);

            ConfigLine configLine;      //設定値

            bool tFileExists = true;    //テンプレートファイルあり／なし(実際にあるかどうか)

            byte[] excelData = null;

            string officeName = "";                         //事務所名
            bool ret;

            List<string> nameList = new List<string>();

            //対象年月(年/月)の取り出し
            int targetDateY = int.Parse(dao.GetYear(true, form["TargetDateY"]).Name);
            int targetDateM = int.Parse(dao.GetMonth(true, form["TargetDateM"]).Name);

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tFileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                //excelData = excelFile.GetAsByteArray();
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

            //----------------------------------
            //共通シートの設定
            //----------------------------------
            CommonSheet common = new CommonSheet();

            //事務所コード
            common.OfficeCode = form["OfficeCode"];

            //事務所名
            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null)
                {
                    officeName = office.OfficeName;
                }
            }

            //現金口座コード
            List<CashAccount> calist = new List<CashAccount>();

            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                calist = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
            }

            //現金口座のデータが無い場合は終了
            if (calist.Count <= 0)
            {
                ModelState.AddModelError("", "登録されている現金口座のデータがありません(事業所コード = " + form["OfficeCode"] + ")");
                excelData = excelFile.GetAsByteArray();
                dExport.DeleteFileStream(fileName);
                return excelData;
            }

            //名称
            /*
            foreach (var a in calist)
            {
                common.CashAccountName.Add(officeName + " " + a.CashAccountName);                       //会社名 + (スペース) + 現金口座コード
            }
            */
            switch (calist.Count)
            {
                case 2:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    common.CashAccountName2 = (officeName + " " + calist[1].CashAccountName);
                    break;
                case 3:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    common.CashAccountName2 = (officeName + " " + calist[1].CashAccountName);
                    common.CashAccountName3 = (officeName + " " + calist[2].CashAccountName);
                    break;
                case 4:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    common.CashAccountName2 = (officeName + " " + calist[1].CashAccountName);
                    common.CashAccountName3 = (officeName + " " + calist[2].CashAccountName);
                    common.CashAccountName4 = (officeName + " " + calist[3].CashAccountName);
                    break;
                case 5: //Add 2018/04/02 arc yano #3876
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    common.CashAccountName2 = (officeName + " " + calist[1].CashAccountName);
                    common.CashAccountName3 = (officeName + " " + calist[2].CashAccountName);
                    common.CashAccountName4 = (officeName + " " + calist[3].CashAccountName);
                    common.CashAccountName5 = (officeName + " " + calist[4].CashAccountName);
                    break;
                case 6:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    common.CashAccountName2 = (officeName + " " + calist[1].CashAccountName);
                    common.CashAccountName3 = (officeName + " " + calist[2].CashAccountName);
                    common.CashAccountName4 = (officeName + " " + calist[3].CashAccountName);
                    common.CashAccountName5 = (officeName + " " + calist[4].CashAccountName);
                    common.CashAccountName6 = (officeName + " " + calist[5].CashAccountName);
                    break;
                default:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName); 
                    break;
            
            }
            
            //年
            common.TargetDateY = targetDateY;
            //月
            common.TargetDateM = targetDateM;

            //繰越金額
            List<decimal?> templist = new List<decimal?>();
            foreach (var a in calist)
            {
                var preAccount = cbDao.GetPreAccount(targetDateY, targetDateM, form["OfficeCode"], a.CashAccountCode);

                if (preAccount != null)
                {
                    templist.Add(preAccount.TotalAmount);
                }
                else
                {
                    templist.Add(null);
                }
            }

            switch (calist.Count)
            {
                case 2:
                    common.PreAccount1 = templist[0];
                    common.PreAccount2 = templist[1];
                    break;
                case 3:
                    common.PreAccount1 = templist[0];
                    common.PreAccount2 = templist[1];
                    common.PreAccount3 = templist[2];
                    break;
                case 4:
                    common.PreAccount1 = templist[0];
                    common.PreAccount2 = templist[1];
                    common.PreAccount3 = templist[2];
                    common.PreAccount4 = templist[3];
                    break;
                case 5: //Add 2018/04/02 arc yano #3876
                    common.PreAccount1 = templist[0];
                    common.PreAccount2 = templist[1];
                    common.PreAccount3 = templist[2];
                    common.PreAccount4 = templist[3];
                    common.PreAccount5 = templist[4];
                    break;
                case 6:
                    common.PreAccount1 = templist[0];
                    common.PreAccount2 = templist[1];
                    common.PreAccount3 = templist[2];
                    common.PreAccount4 = templist[3];
                    common.PreAccount5 = templist[4];
                    common.PreAccount6 = templist[5];
                    break;
                default:
                    common.CashAccountName1 = (officeName + " " + calist[0].CashAccountName);
                    break;
            }

            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(common)
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

            List<CommonSheet> clist = new List<CommonSheet>();
            clist.Add(common);

            //データ設定
            ret = dExport.SetData<CommonSheet, CommonSheet>(ref excelFile, clist, configLine);

            //データ設定した結果がfalseなら
            if (false == ret)
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートの設定値が正しくないため、ファイルを作成できません");
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

            //-----------------------------
            //出納・現金シート設定
            //-----------------------------
            int cnt = 3;
            foreach (var a in calist)
            {
                //現金出納帳データ取得
                List<GetJournalCashResult> journalList = joDao.GetGetJournalCash(targetDateY, targetDateM, form["OfficeCode"], a.CashAccountCode);

                //設定価取得
                configLine = dExport.GetConfigLine(config, cnt);

                ret = dExport.SetData<GetJournalCashResult, GetJournalCashResult>(ref excelFile, journalList, configLine);

                cnt++;

                //金種データ取得
                List<T_CashBalanceSheet> cBalanceList = cbDao.GetCashBalance(targetDateY, targetDateM, form["OfficeCode"], a.CashAccountCode);

                configLine = dExport.GetConfigLine(config, cnt);

                ret = dExport.SetData<T_CashBalanceSheet, T_CashBalanceSheet>(ref excelFile, cBalanceList, configLine);

                cnt++;
            }

            //最後に再計算を実行する
            excelFile.Workbook.Calculate();

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
        private ContentResult CsvDownload(string prefix, string buffer)
        {
            string dateTime = string.Format("{0:yyyyMMdd}", DateTime.Now);
            string fileName = dateTime + "_" + prefix + ".csv";
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
            DateTime targetDate = DateTime.Parse(dao.GetYear(false, form["TargetDateY"]).Name + "/" + dao.GetMonth(false, form["TargetDateM"]).Name + "/01");

            //検索条件の再セット
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(false), form["TargetDateY"], false);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(false), form["TargetDateM"], false);
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["CashAccountCode"] = form["CashAccountCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            
            //Add 2015/03/19 arc yano 現金出納帳出力(Excel)
            if (!string.IsNullOrWhiteSpace(form["OfficeCode"]))
            {
                ViewData["ExcelButtonEnabled"] = true;
            }
            else
            {
                ViewData["ExcelButtonEnabled"] = false;
            }

            if (!string.IsNullOrEmpty(form["OfficeCode"]))
            {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null)
                {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            if (!string.IsNullOrEmpty(form["CashAccountCode"]))
            {
                CashAccount CashAccount = new CashAccountDao(db).GetByKey(form["OfficeCode"], form["CashAccountCode"]);
                if (CashAccount != null)
                {
                    ViewData["CashAccountName"] = CashAccount.CashAccountName;
                }
            }
            ViewData["CashAccountName"] = form["CashAccountName"];

            //現金口座名ドロップダウンリストセット
            List<CashAccount> cashAccountList = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
            List<CodeData> accountDataList = new List<CodeData>();
            
            foreach (var a in cashAccountList)
            {
                CodeData data = new CodeData();
                data.Code = a.CashAccountCode;
                data.Name = a.CashAccountName;
                accountDataList.Add(data);
            }
            ViewData["CashAccountList"] = CodeUtils.GetSelectListByModel(accountDataList, form["CashAccountCode"], true);

            return;

        }

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
