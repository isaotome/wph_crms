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
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
namespace Crms.Controllers
{

    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarReceivableManagementController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "車両売掛金管理";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "車両売掛金検索"; // 処理名（ログ出力用）
        private static readonly string PROC_NAME_CSV = "車両売掛金管理Excel出力";       // 処理名(CSV出力)
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
        public CarReceivableManagementController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        /// <summary>
        /// 車両売掛金管理検索画面表示
        /// </summary>
        /// <returns>車両売掛金管理検索画面表示</returns>
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
            form["Zerovisible"] = "1";  //残高ゼロ表示フラグ(0:非表示 1:表示)　デフォルト値 = 1
            form["DefaultZerovisible"] = form["Zerovisible"];
            return Criteria(form);
        }

        /// <summary>
        /// 車両売掛金管理検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両売掛金管理検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<AccountsReceivableCar> list = new PaginatedList<AccountsReceivableCar>();

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
                        return View("CarReceivableManagementCriteria", list);

                    case "2": // Excelボタン
                        SetComponent(form);
                        return Download(form);

                    default:
                        // 検索項目の設定
                        SetComponent(form);

                        // 検索画面の表示
                        return View("CarReceivableManagementCriteria", list);
                }

            }
            // 検索項目の設定
            SetComponent(form);
            return View("CarReceivableManagementCriteria", list);
        }

        #region 車両売掛金検索
        private PaginatedList<AccountsReceivableCar> SearchStart(FormCollection form)
        {
            //検索条件の設定
            AccountsReceivableCar condition = SetCondition(form);

            //締めステータスの取得
            string closeStatus = getCloseStatus(form);

            //検索実行
            return new AccountsReceivableCarDao(db).GetListByCondition(condition, closeStatus, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion


        #region Excelボタン押下
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト)
            string fileName = DownLoadTime + "_売掛管理" + ".xlsx";

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
            string sheetName = "CarReceivableManagement";    //シート名
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
            AccountsReceivableCar condition = SetCondition(form);

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
            IEnumerable<XElement> data = GetFieldList("CarReceivableManagementText");

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
            List<AccountsReceivableCarExcelResult> list = new AccountsReceivableCarDao(db).GetExcelListByCondition(condition, closeStatus);

            //データ設定
            ret = dExport.SetData<AccountsReceivableCarExcelResult, AccountsReceivableCarExcelResult>(ref excelFile, list, configLine);
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
        private DataTable MakeConditionRow(AccountsReceivableCar condition)
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
            if (condition.SalesDateFrom != null || condition.SalesDateTo != null)
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
                if(condition.Zerovisible.Equals("1")){
                    ZerovisibleName = "する";
                }else{
                    ZerovisibleName = "しない";
                }

                conditionText += string.Format("ゼロ表示={0}", ZerovisibleName);
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


        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetComponent(FormCollection form)
        {

            //検索条件の再セット
            CodeDao dao = new CodeDao(db);
            //対象年
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象月
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);
            ViewData["TargetDateY"] = form["TargetDateY"];          //対象日付(yyyy)
            ViewData["TargetDateM"] = form["TargetDateM"];          //対象日付(MM)
            ViewData["SlipNumber"] = form["SlipNumber"];            //伝票番号
            ViewData["DepartmentCode"] = form["DepartmentCode"];    //部門コード
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {      //部門名
                ViewData["DepartmentName"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
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
            ViewData["Zerovisible"] = form["Zerovisible"];                        //ゼロ表示※デフォルト値
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];          //対象日付(yyyy)※デフォルト値
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];          //対象日付(MM)※デフォルト値
            ViewData["DefaultZerovisible"] = form["DefaultZerovisible"];          //ゼロ表示※デフォルト値

            return;
        }
        #endregion

        #region フィールド定義データから出力フィールドリストを取得する
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
        #endregion

        #region 検索条件設定
        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>検索条件</returns>
        private AccountsReceivableCar SetCondition(FormCollection form)
        {
            AccountsReceivableCar condition = new AccountsReceivableCar();
            CodeDao dao = new CodeDao(db);
            DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01").Date;  //年と月をつなげる。日は1日固定          
            condition.CloseMonth = TargetDate;                              //対象月
            condition.SlipNumber = form["SlipNumber"];                      //伝票番号
            condition.DepartmentCode = form["DepartmentCode"];              //部門コード
            condition.SalesDateFrom = form["SalesDateFrom"];
            condition.SalesDateTo = form["SalesDateTo"];

            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((condition.SalesDateFrom != null) && condition.SalesDateTo == null)
            {
                condition.SalesDateTo = condition.SalesDateFrom;
            }
            condition.CustomerCode = form["CustomerCode"];     //顧客コード

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

            condition.Zerovisible = form["Zerovisible"]; //ゼロ表示

            return condition;
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

            ViewData["CloseStatus"] = ret == null ? "未実施" : ret.Name;

            return CloseMonthData.CloseStatus;
        }
        #endregion

    }
}