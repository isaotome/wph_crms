using System;
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
//Add 2015/06/18 arc nakayama  売掛金対応① Excel対応
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
namespace Crms.Controllers
{

    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ReceivableManagementController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "売掛金管理";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "売掛金検索"; // 処理名（ログ出力用）
        //Mod 2015/06/18 arc nakayama  売掛金対応① Excel対応
        private static readonly string PROC_NAME_CSV = "売掛金管理Excel出力";       // 処理名(CSV出力)
        //private static readonly string FILE_NAME = "ReceivableManagementText";  //CSV出力時のファイル名
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
        public ReceivableManagementController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        /// <summary>
        /// 売掛金管理検索画面表示
        /// </summary>
        /// <returns>売掛金管理検索画面</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        /// </history>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);    //初期値に当日の年をセットする
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');   //初期値に当日の月をセットする
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];
            form["RequestFlag"] = "1";  //リクエストフラグ　デフォルト値 = 1
            //form["SlipType"] = "2";     //伝票種別　デフォルト値 = 2（空白）
            //form["DefaultSlipType"] = form["SlipType"];
            form["Zerovisible"] = "1";  //残高ゼロ表示フラグ(0:非表示 1:表示)　デフォルト値 = 1
            form["DefaultZerovisible"] = form["Zerovisible"];

            //Add 2020/05/22 yano #4032
            form["SummaryPattern"] = "0";                                           //集計方法(請求先毎)
            form["DefaultSummaryPattern"] = form["SummaryPattern"];                 //集計方法・初期値

            form["Classification"] = "1";                                           //区分(社外)
            form["DefaultClassification"] = form["Classification"];                 //区分・初期値

            return Criteria(form);
        }

        /// <summary>
        /// 売掛金管理検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>売掛金管理検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<AccountsReceivable> list = new PaginatedList<AccountsReceivable>();

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
                        list = SearchStart(form);
                        SetComponent(form);
                        return View("ReceivableManagementCriteria", list);

                    case "2": // Excelボタン
                        SetComponent(form);
                        return Download(form);

                    default:
                        // 検索項目の設定
                        SetComponent(form);

                        // 検索画面の表示
                        return View("ReceivableManagementCriteria", list);
                }
            }
            // 検索項目の設定
            SetComponent(form);
            return View("ReceivableManagementCriteria", list);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>売掛金管理検索画面</returns>
        /// <history>
        /// 2015/08/11 arc yano  #3233 新規作成
        /// </history>
        private PaginatedList<AccountsReceivable> SearchStart(FormCollection form)
        {
            //検索条件の設定
            AccountsReceivable condition = SetCondition(form);
             
            //締めステータスの取得
            string closeStatus = getCloseStatus(form);

            //検索実行
            return new AccountsReceivableDao(db).GetListByCondition(condition, closeStatus, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        #region Excelボタン押下
        /// <summary>
        /// Excelボタン押下
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>売掛金管理検索画面</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        /// 2015/06/18 arc nakayama  売掛金対応① Excel対応 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            PaginatedList<AccountsReceivable> list = new PaginatedList<AccountsReceivable>();   //Add 2020/05/22 yano #4032

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
           
            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string fileName = "";

            string filePathName = "";

            //Add 2020/05/22 yano #4032
            //テンプレートファイル取得
            string tFileName = "";

            byte[] excelData = null;

            switch (form["SummaryPattern"])
            {

                case "0":

                    //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト)
                    fileName = DownLoadTime + "_サービス売掛管理_請求先毎" + ".xlsx";
                    filePathName = filePath + fileName;

                    tFileName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateAccountsReceivableCustomerClaim"]) ? "" : ConfigurationManager.AppSettings["TemplateAccountsReceivableCustomerClaim"];
                    excelData = MakeExcelDataWithTemplate(form, filePathName, tFileName);
                    break;
                
                case "1":

                    //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト)
                    fileName = DownLoadTime + "_サービス売掛管理_部門毎" + ".xlsx";
                    filePathName = filePath + fileName;

                    tFileName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateAccountsReceivableDepartment"]) ? "" : ConfigurationManager.AppSettings["TemplateAccountsReceivableDepartment"];
                    excelData = MakeExcelDataWithTemplate(form, filePathName, tFileName);
                    break;
                
                default:
                    fileName = DownLoadTime + "_サービス売掛管理" + ".xlsx";
                    filePathName = filePath + fileName;
                    excelData = MakeExcelData(form, filePathName);
                    break;
            }
            
            //byte[] excelData = MakeExcelData(form, filePathName);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        
        }
        #endregion


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

        #region エクセルデータ作成
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルなし)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2015/06/18 arc nakayama  売掛金対応① Excel対応
        /// </history>
        private byte[] MakeExcelData(FormCollection form, string fileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                        //設定値
            byte[] excelData = null;                      //エクセルデータ
            string sheetName = "ReceivableManagement";    //シート名
            int dateType = 0;                             //データタイプ(帳票形式)
            string setPos = "A1";                         //設定位置
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
            AccountsReceivable condition = SetCondition(form);

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
            IEnumerable<XElement> data = GetFieldList("ReceivableManagementText");

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

            //Mod 2015/08/11 arc yano  #3233 売掛金帳票対応 締めステータスによる、データ取得方法の変更
            //締めステータスを取得する。
            string closeStatus = getCloseStatus(form);

            //売掛金情報の取得
            List<AccountsReceivable> list = new AccountsReceivableDao(db).GetQueryable(condition, closeStatus).ToList();

            //Excel出力用に変換
            List<ReceivableManagementExcelResult> exlist = ConvertDate(ref list);

            ret = dExport.SetData<ReceivableManagementExcelResult, ReceivableManagementExcelResult>(ref excelFile, exlist, configLine);
            
            
            //戻り値チェック


            excelData = excelFile.GetAsByteArray();

            return excelData;
        }

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規作成
        /// </history>
        private byte[] MakeExcelDataWithTemplate(FormCollection form, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            ConfigLine defconfigLine;               //設定値       //Add 2020/05/22 yano #4032
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)
            int dateType = 0;                       //データタイプ
            string setPos = "A1";                   //設定位置

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");

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
            //出力位置の設定
            setPos = "A1";
            
            //設定値
            defconfigLine = dExport.GetDefaultConfigLine(0, configLine.SheetName, dateType, setPos);

            //検索条件取得
            AccountsReceivable condition = SetCondition(form);

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(condition);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, defconfigLine);

            //----------------------------
            // データ行出力
            //----------------------------
            string closeStatus = getCloseStatus(form);

            IEnumerable<AccountsReceivable> alist = null;

            List<AccountsReceivable> list = null;


            //売掛金情報の取得
            //List<AccountsReceivable> list = new AccountsReceivableDao(db).GetQueryable(condition, closeStatus).ToList();

            alist = new AccountsReceivableDao(db).GetQueryable(condition, closeStatus).AsEnumerable();

            //データ設定
            if (form["SummaryPattern"].Equals("0"))
            {

                IEnumerable<ReceivableManagementExcelResultByCustomerClaim> excellist = ConvertDataCustomerClaim(alist);

                excellist = MoldingSummaryDataCustomerClaim(excellist);

                ret = dExport.SetData<ReceivableManagementExcelResultByCustomerClaim>(ref excelFile, excellist.ToList(), configLine);

                //ret = dExport.SetData<AccountsReceivable, ReceivableManagementExcelResultByCustomerClaim>(ref excelFile, list, configLine);
            }
            else
            {
                IEnumerable<ReceivableManagementExcelResultByDepartment> excellist = ConvertDataDepartment(alist);

                excellist = MoldingSummaryDataDepartment(excellist);

                ret = dExport.SetData<ReceivableManagementExcelResultByDepartment>(ref excelFile, excellist.ToList(), configLine);
            }
            
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


        #region 検索条件設定(Excel出力用)
        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>検索条件</returns>
        /// <history>
        ///  2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        ///  2015/08/11 arc yano  #3233 売掛金帳票対応　データのクラスの変更(dbo.AccountsReceivableに変更)
        ///  2015/06/18 arc nakayama  売掛金対応① Excel対応　新規作成
        /// </history>
        private AccountsReceivable SetCondition(FormCollection form)
        {
            AccountsReceivable condition = new AccountsReceivable();
            CodeDao dao = new CodeDao(db);
            DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01").Date;  //年と月をつなげる。日は1日固定          
            condition.CloseMonth = TargetDate;                              //対象月
            condition.SlipNumber = form["SlipNumber"];                      //伝票番号
            //condition.SlipType = form["SlipType"];                          //伝票種別
            condition.DepartmentCode = form["DepartmentCode"];              //部門コード
            condition.SalesDateFrom = form["SalesDateFrom"];                //納車年月日From
            condition.SalesDateTo =form["SalesDateTo"];                     //納車年月日To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((condition.SalesDateFrom != null) && condition.SalesDateTo == null)
            {
                condition.SalesDateTo = condition.SalesDateFrom;
            }
            condition.CustomerCode = form["CustomerCode"];     //顧客コード

            //Mod 2015/06/23 arc nakayama 検索項目の文字列をマスタから取得するように変更
            //if (string.IsNullOrEmpty(form["SlipType"]))
            //{
            //    condition.SlipTypeName = "";
            //}else{
            //    condition.SlipTypeName = dao.GetCodeNameByKey("014", form["SlipType"], false).Name;
            //}

            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {      //部門名
                condition.DepartmentName = "";    //部門コードが未入力なら空文字
            }
            else
            {
                condition.DepartmentName = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            if (string.IsNullOrEmpty(form["CustomerCode"]))
            {      //顧客名
                condition.CustomerName = "";    //顧客コードが未入力なら空文字
            }
            else
            {
                condition.CustomerName = new CustomerDao(db).GetByKey(form["CustomerCode"].ToString()).CustomerName;  //入力済みなら顧客テーブルから検索
            }

            condition.Zerovisible = form["Zerovisible"];
            condition.Classification = form["Classification"];

            //Add 2020/05/22 yano #4032
            condition.SummaryPattern = int.Parse(form["SummaryPattern"]);       //集計方法

            condition.ClassificationName = "";

            //区分名
            if (!string.IsNullOrWhiteSpace(form["Classification"]))
            {
                condition.ClassificationName = dao.GetCodeNameByKey("016", form["Classification"], false).Name;
            }

            return condition;
        }
        #endregion

       
        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <hisotory>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        /// 2015/08/11 arc yano  #3233 売掛金帳票対応　データのクラスの変更(DocumentExportCondition→AccountsReceivableに変更)
        /// 2015/06/18 arc nakayama  売掛金対応① Excel対応　新規作成
        /// </hisotory>
        private DataTable MakeConditionRow(AccountsReceivable condition)
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

            if (condition.CloseMonth != null)
            {
                conditionText += string.Format("対象年月={0:yyyy/MM/dd}　", condition.CloseMonth);
            }
            if (!string.IsNullOrEmpty(condition.SalesDateFrom) || !string.IsNullOrEmpty(condition.SalesDateTo))
            {
                conditionText += string.Format("納車年月日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.SalesDateFrom, condition.SalesDateTo);
            }
            if (!string.IsNullOrEmpty(condition.SlipNumber))
            {
                conditionText += string.Format("伝票番号={0}　", condition.SlipNumber);
            }
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                conditionText += string.Format("部門={0}:{1}　", condition.DepartmentCode, condition.DepartmentName);
            }
            if (!string.IsNullOrEmpty(condition.CustomerCode))
            {
                conditionText += string.Format("顧客コード={0}　", condition.CustomerCode);
            }
            if (!string.IsNullOrEmpty(condition.Zerovisible))
            {
                string ZerovisibleName = "";
                if (condition.Zerovisible.Equals("1"))
                {
                    ZerovisibleName = "する";
                }
                else
                {
                    ZerovisibleName = "しない";
                }

                conditionText += string.Format("ゼロ表示={0}　", ZerovisibleName);
            }

            //Add  2020/05/22 yano #4032
            //区分名
            if (!string.IsNullOrEmpty(condition.ClassificationName))
            {
                conditionText += string.Format("区分={0}　", condition.ClassificationName);
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion


        //Mod 2015/08/11 arc yano  #3233 売掛金帳票対応　Excel出力用のデータに変換
        #region
        List<ReceivableManagementExcelResult> ConvertDate(ref List<AccountsReceivable> list)
        {
            List<ReceivableManagementExcelResult> ret = new List<ReceivableManagementExcelResult>();

            foreach (var a in list)
            {
                ReceivableManagementExcelResult rec = new ReceivableManagementExcelResult();

                //rec.SlipTypeName = a.SlipTypeName;
                rec.SlipNumber = a.SlipNumber;
                rec.SalesDate = (a.SalesDate != null ? string.Format("{0:yyyy/MM/dd }", a.SalesDate) : "");
                rec.DepartmentCode = a.DepartmentCode;
                rec.DepartmentName = a.DepartmentName;
                rec.CustomerCode = a.CustomerCode;
                rec.CustomerName = a.CustomerName;
                rec.CustomerClaimType = a.CustomerClaimType;
                rec.CustomerClaimTypeName = a.CustomerClaimTypeName;
                rec.CustomerClaimCode = a.CustomerClaimCode;
                rec.CustomerClaimName = a.CustomerClaimName;
                rec.CarriedBalance = (a.CarriedBalance != null ? string.Format("{0:N0}", a.CarriedBalance) : "0");
                rec.PresentMonth = (a.PresentMonth != null ? string.Format("{0:N0}", a.PresentMonth) : "0");
                rec.Expendes = (a.Expendes != null ? string.Format("{0:N0}", a.Expendes) : "0");
                rec.TotalAmount = (a.TotalAmount != null ? string.Format("{0:N0}", a.TotalAmount) : "0");
                rec.Payment = (a.Payment != null ? string.Format("{0:N0}", a.Payment) : "0");
                rec.ChargesPayment = (a.ChargesPayment != null ? string.Format("{0:N0}", a.ChargesPayment) : "0");
                rec.BalanceAmount = (a.BalanceAmount != null ? string.Format("{0:N0}", a.BalanceAmount) : "0");

                ret.Add(rec);
            }

            return ret;
        }
        #endregion

        #region Excel用の処理
        /// <summary>
        ///クラス変換(請求先毎)
        /// </summary>
        /// <param name="alist">売掛金データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private List<ReceivableManagementExcelResultByCustomerClaim> ConvertDataCustomerClaim(IEnumerable<AccountsReceivable> alist)
        {
            List<ReceivableManagementExcelResultByCustomerClaim> excellist = alist.Select(
                                   x => new ReceivableManagementExcelResultByCustomerClaim()
                                   {
                                       CustomerClaimCode = x.CustomerClaimCode
                                       ,
                                       CustomerClaimName = x.CustomerClaimName
                                       ,
                                       CustomerClaimType = x.CustomerClaimTypeName
                                       ,
                                       CustomerClaimTypeName = x.CustomerClaimTypeName
                                       ,
                                       SlipNumber = x.SlipNumber
                                       ,
                                       SalesDate = x.SalesDate
                                       ,
                                       DepartmentCode = x.DepartmentCode
                                       ,
                                       DepartmentName = x.DepartmentName
                                       ,
                                       CarriedBalance = x.CarriedBalance
                                       ,
                                       PresentMonth = x.PresentMonth
                                       ,
                                       Expendes = x.Expendes
                                       ,
                                       TotalAmount = x.TotalAmount
                                       ,
                                       Payment = x.Payment
                                       ,
                                       ChargesPayment = x.ChargesPayment
                                       ,
                                       BalanceAmount = x.BalanceAmount
                                   }
                                   ).ToList();

            return excellist;
        }

        /// <summary>
        ///クラス変換(部門毎)
        /// </summary>
        /// <param name="alist">売掛金データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private List<ReceivableManagementExcelResultByDepartment> ConvertDataDepartment(IEnumerable<AccountsReceivable> alist)
        {
            List<ReceivableManagementExcelResultByDepartment> excellist = alist.Select(
                                   x => new ReceivableManagementExcelResultByDepartment()
                                   {
                                       DepartmentCode = x.DepartmentCode
                                       ,
                                       DepartmentName = x.DepartmentName
                                       ,
                                       CustomerClaimCode = x.CustomerClaimCode
                                       ,
                                       CustomerClaimName = x.CustomerClaimName
                                       ,
                                       CustomerClaimType = x.CustomerClaimType
                                       ,
                                       CustomerClaimTypeName = x.CustomerClaimTypeName
                                       ,
                                       SlipNumber = x.SlipNumber
                                       ,
                                       SalesDate = x.SalesDate
                                       ,
                                       CarriedBalance = x.CarriedBalance
                                       ,
                                       PresentMonth = x.PresentMonth
                                       ,
                                       Expendes = x.Expendes
                                       ,
                                       TotalAmount = x.TotalAmount
                                       ,
                                       Payment = x.Payment
                                       ,
                                       ChargesPayment = x.ChargesPayment
                                       ,
                                       BalanceAmount = x.BalanceAmount
                                   }
                                   ).ToList();

            return excellist;
        }

        /// <summary>
        /// Excel用の調整処理(請求先毎)
        /// </summary>
        /// <param name="alist">売掛金データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private IEnumerable<ReceivableManagementExcelResultByCustomerClaim> MoldingSummaryDataCustomerClaim(IEnumerable<ReceivableManagementExcelResultByCustomerClaim> alist)
        {
            IEnumerable<ReceivableManagementExcelResultByCustomerClaim> retgrp = null;
            List<ReceivableManagementExcelResultByCustomerClaim> ret = null;

            retgrp = alist.GroupBy(h => h.CustomerClaimCode, (k, v) => v.OrderBy(o => o.SlipNumber).FirstOrDefault());

            ret = alist.ToList();

            foreach (var l in retgrp)
            {
                ret.Where(x => l.CustomerClaimCode.Equals(x.CustomerClaimCode) && !x.SlipNumber.Equals(l.SlipNumber)).ToList().ForEach(y => { y.CustomerClaimCode = null; y.CustomerClaimName = null; y.CustomerClaimType = null; y.CustomerClaimTypeName = null; });
            }

            alist = ret.AsEnumerable();

            return alist;
        }

        /// <summary>
        /// Excel用の調整処理(部門毎)
        /// </summary>
        /// <param name="alist">売掛金データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private IEnumerable<ReceivableManagementExcelResultByDepartment> MoldingSummaryDataDepartment(IEnumerable<ReceivableManagementExcelResultByDepartment> alist)
        {
            IEnumerable<ReceivableManagementExcelResultByDepartment> retgrp = null;
            List<ReceivableManagementExcelResultByDepartment> ret = null;

            retgrp = alist.GroupBy(h => h.DepartmentCode, (k, v) => v.OrderBy(q => q.CustomerClaimCode).ThenBy(o => o.SlipNumber).FirstOrDefault());

            ret = alist.ToList();

            foreach (var l in retgrp)
            {
                ret.Where(x => l.DepartmentCode.Equals(x.DepartmentCode) && !(x.CustomerClaimCode.Equals(l.CustomerClaimCode) && x.SlipNumber.Equals(l.SlipNumber))).ToList().ForEach(y => { y.DepartmentCode = null; y.DepartmentName = null; });
            }

            alist = ret.AsEnumerable();

            return alist;
        }
        ///// <summary>
        ///// Excel用の調整処理
        ///// </summary>
        ///// <param name="alist">売掛金データ</param>
        ///// <param name="form">売掛金データ</param>
        ///// <returns></returns>
        ///// <history>
        ///// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        ///// </history>
        //private IEnumerable<AccountsReceivable> MoldingSummaryData(IEnumerable<AccountsReceivable> alist, string summarypattern)
        //{
        //    IEnumerable<AccountsReceivable> retgrp = null;
        //    List<AccountsReceivable> ret = null;


        //    switch (summarypattern)
        //    {
        //        case "0":   //請求先毎

        //            retgrp = alist.GroupBy(h => h.CustomerClaimCode, (k, v) => v.OrderBy(o => o.SlipNumber).FirstOrDefault());

        //            ret = alist.ToList();

        //            foreach (var l in retgrp)
        //            {
        //                ret.Where(x => l.CustomerClaimCode.Equals(x.CustomerClaimCode) && !x.SlipNumber.Equals(l.SlipNumber)).ToList().ForEach(y => { y.CustomerClaimCode = null; y.CustomerClaimName = null; y.CustomerClaimType = null; y.CustomerClaimTypeName = null; });
        //            }

        //            alist = ret.AsEnumerable();

        //            break;

        //        case "1":   //部門毎

        //            retgrp = alist.GroupBy(h => h.DepartmentCode, (k, v) => v.OrderBy(q => q.CustomerClaimCode).ThenBy(o => o.SlipNumber).FirstOrDefault());

        //            ret = alist.ToList();

        //            foreach (var l in retgrp)
        //            {
        //                ret.Where(x => l.DepartmentCode.Equals(x.DepartmentCode) && !(x.CustomerClaimCode.Equals(l.CustomerClaimCode) && x.SlipNumber.Equals(l.SlipNumber))).ToList().ForEach(y => { y.DepartmentCode = null; y.DepartmentName = null; });
        //            }

        //            alist = ret.AsEnumerable();

        //            break;

        //        default:    //集計なし
        //            //なし
        //            break;
        //    }

        //    return alist;
        //}
        #endregion

        //Add 2015/06/18 arc nakayama  売掛金対応① Excel対応
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

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        /// </history>
        private void SetComponent(FormCollection form)
        {

            //検索条件の再セット
            //Mod 2015/02/20 src nakayama 売掛金管理指摘事項修正　検索項目から「部門名」「顧客名」を削除
            CodeDao dao = new CodeDao(db);
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);
            ViewData["TargetDateY"] = form["TargetDateY"];          //対象日付(yyyy)
            ViewData["TargetDateM"] = form["TargetDateM"];          //対象日付(MM)
            //ViewData["SlipType"] = form["SlipType"];                //伝票種別
            ViewData["SlipNumber"] = form["SlipNumber"];            //伝票番号
            ViewData["DepartmentCode"] = form["DepartmentCode"];    //部門コード
            if (string.IsNullOrEmpty(form["DepartmentCode"])){      //部門名
                ViewData["DepartmentName"] = "";    //部門コードが未入力なら空文字
            }else{
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];      //納車日From
            ViewData["SalesDateTo"] = form["SalesDateTo"];          //納車日To
            ViewData["CustomerCode"] = form["CustomerCode"];        //顧客コード
            if (string.IsNullOrEmpty(form["CustomerCode"]))
            {      //部門名
                ViewData["CustomerName"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
                ViewData["CustomerName"] = new CustomerDao(db).GetByKey(form["CustomerCode"].ToString()).CustomerName;  //入力済みなら部門テーブルから検索
            }
            ViewData["RequestFlag"] = "1";
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];          //対象日付(yyyy)※デフォルト値
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];          //対象日付(MM)※デフォルト値
            ViewData["Zerovisible"] = form["Zerovisible"];
            ViewData["DefaultZerovisible"] = form["DefaultZerovisible"];
            ViewData["Classification"] = form["Classification"];
            ViewData["ClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("016", false), form["Classification"], true);
            ViewData["DefaultClassification"] = form["DefaultClassification"];    //Add 2020/05/22 yano #4032
            
            //ViewData["SlipTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("014", false), form["SlipType"], true);
            //ViewData["DefaultSlipType"] = form["DefaultSlipType"];      //伝票種別※デフォルト値

            //Add 2020/05/22 yano #4032
            ViewData["SummaryPattern"] = form["SummaryPattern"];
            ViewData["DefaultSummaryPattern"] = form["DefaultSummaryPattern"];

            return;
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

        //Add 2015/08/11 arc yano arc yano  #3233 売掛金帳票対応
        #region 締めステータス取得
        /// <summary>
        /// 締めステータスの取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private string getCloseStatus(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //formの対象年月をdatetime型に入れる
            DateTime? TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");
            
            //対象年月をyyyymmdd形式にする
            string strTargetDate = string.Format("{0:yyyyMMdd}", TargetDate);
            
      
            //月締め処理状況テーブルから該当のレコードを取得する
            CloseMonthControl CloseMonthData = new CloseMonthControlDao(db).GetByKey(strTargetDate);
            //レコードがnullだった場合、空文字にする
            if (CloseMonthData == null)
            {
                CloseMonthData = new CloseMonthControl();
                CloseMonthData.CloseStatus = "";
            }

            //ビューに設定する
            c_CloseStatus ret = dao.GetCloseStatus(false, CloseMonthData.CloseStatus);

            ViewData["CloseStatus"] = ret == null ? "未実施": ret.Name;
            
            return CloseMonthData.CloseStatus;
        }
        #endregion
    }
}