using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using System.Transactions;
using Crms.Models;
using System.Xml.Linq;
using OfficeOpenXml;
using System.Configuration;
using System.Data;
using Microsoft.VisualBasic;    //Add 2018/05/25 arc yano #3888
namespace Crms.Controllers
{
    /// <summary>
    /// 顧客データリスト(車検案内)機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class InspectGuidListController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "顧客データリスト(車検案内)";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "顧客データリスト(車検案内)検索"; // 処理名（ログ出力用）　
        private static readonly string PROC_NAME_CSV = "顧客データリスト(車検案内)Excel出力";       // 処理名(CSV出力)
        private static readonly string InsGuid_NASHI = "";//車検案内（指定なし）
        private static readonly string AddressReconfirm_NASHI = ""; //住所再確認（指定なし）
        private static readonly string CustomerKind_NASHI = "";     //顧客種別（指定なし）
        private static readonly string TARGET_DATE_FLAG = "0";      //次回点検日か車検満了日かを分けるフラグ（0:次回点検日　1:車検満了日）
        private static readonly string PROC_NAME_EXCELUPLOAD = "車検点検日更新一括取込";       // 処理名(Excel取込)     //Add 2018/05/25 arc yano #3888
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
        public InspectGuidListController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        /// <summary>
        /// 顧客データリスト(車検案内)検索画面表示
        /// </summary>
        /// <returns>顧客データリスト(車検案内)検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            form["InspectGuidFlag"] = InsGuid_NASHI;  //デフォルト値（スペース）
            form["DefaultInspectGuidFlag"] = form["InspectGuidFlag"];
            form["RequestFlag"] = "1";  //リクエストフラグ　デフォルト値 = 1 
            form["PrivateFlag"] = "true";  //自社取扱いフラグ  デフォルト値 = 0
            form["DefaultPrivateFlag"] = form["PrivateFlag"]; //自社取扱いフラグ
            form["DefaultCustomerAddressReconfirm"] = AddressReconfirm_NASHI;//デフォルト値（スペース）住所再確認
            form["DefaultCustomerAddressReconfirm"] = form["CustomerAddressReconfirm"];//デフォルト値（スペース）住所再確認
            form["CustomerKind"] = CustomerKind_NASHI; //デフォルト値（スペース）顧客種別
            form["DefaultCustomerKind"] = form["CustomerKind"];
            form["TargetDateFlag"] = TARGET_DATE_FLAG; //次回点検日か車検満了日かを分けるフラグ デフォルト値は次回点検日
            form["DefaultTargetDateFlag"] = form["TargetDateFlag"];
            return Criteria(form);
        }

        /// <summary>
        /// 顧客データリスト検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>顧客データリスト検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            PaginatedList<CustomerDataResult> list = new PaginatedList<CustomerDataResult>();
   
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
                        SetDropDownList(form);
                        SetComponent(form);

                        // 検索画面の表示
                        return View("InspectGuidListCriteria", list);
                }
                
            }
            // 検索項目の設定
            SetDropDownList(form);
            SetComponent(form);
            return View("InspectGuidListCriteria", list);
        }


        #region 検索ボタン押下
        /// <summary>
        /// 検索ボタン押下処理
        /// </summary>
        /// <param name="form"></param>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SearchStart(FormCollection form, PaginatedList<CustomerDataResult> list)
        {
            //画面設定
            SetDropDownList(form);
            SetComponent(form);

            //顧客データ検索
            list = GetSearchResultList(form);

            return View("InspectGuidListCriteria", list);

        }
        #endregion

        #region　顧客データリスト検索
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        private PaginatedList<CustomerDataResult> GetSearchResultList(FormCollection form)
        {
            //検索項目セット
            CustomerDataSearch InspectGuidDataSearchCondition = new CustomerDataSearch();
            CodeDao dao = new CodeDao(db);
            InspectGuidDataSearchCondition.InspectGuidFlag = form["InspectGuidFlag"];	            //車検案内(DM可否)
            InspectGuidDataSearchCondition.DepartmentCode2 = form["SalesDepartmentCode2"];	    //営業担当部門コード
            InspectGuidDataSearchCondition.CarEmployeeCode = form["CarEmployeeCode"];          //営業担当者コード
            InspectGuidDataSearchCondition.ServiceDepartmentCode2 = form["ServiceDepartmentCode2"];	//サービス担当部門コード
            if (!string.IsNullOrEmpty(form["FirstRegistrationFrom"]))
            {
                InspectGuidDataSearchCondition.FirstRegistrationFrom = CommonUtils.StrToDateTime(form["FirstRegistrationFrom"] + "/01", DaoConst.SQL_DATETIME_MAX);	//初年度登録From
            }
            if (!string.IsNullOrEmpty(form["FirstRegistrationTo"]))
            {
                InspectGuidDataSearchCondition.FirstRegistrationTo = CommonUtils.StrToDateTime(form["FirstRegistrationTo"] + "/01", DaoConst.SQL_DATETIME_MAX);	    //初年度登録To
            }
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((InspectGuidDataSearchCondition.FirstRegistrationFrom != null) && InspectGuidDataSearchCondition.FirstRegistrationTo == null)
            {
                InspectGuidDataSearchCondition.FirstRegistrationTo = InspectGuidDataSearchCondition.FirstRegistrationFrom;
            }
            InspectGuidDataSearchCondition.SalesEmployeeName = form["SalesEmployeeName"];      //担当者名(営業)
            if (!string.IsNullOrEmpty(form["MakerCode"]))
            {
                InspectGuidDataSearchCondition.MakerName = new V_CarMasterDao(db).GetByMakerkey(form["MakerCode"]).MakerName;  //メーカ名
            }
            if (!string.IsNullOrEmpty(form["CarCode"]))
            {
                InspectGuidDataSearchCondition.CarName = new V_CarMasterDao(db).GetByCarkey(form["CarCode"]).CarName; //車種名
            } 
            InspectGuidDataSearchCondition.RegistrationDateFrom = CommonUtils.StrToDateTime(form["RegistrationDateFrom"], DaoConst.SQL_DATETIME_MAX);	    //登録年月日From
            InspectGuidDataSearchCondition.RegistrationDateTo = CommonUtils.StrToDateTime(form["RegistrationDateTo"], DaoConst.SQL_DATETIME_MAX);	        //登録年月日To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((InspectGuidDataSearchCondition.RegistrationDateFrom != null) && InspectGuidDataSearchCondition.RegistrationDateTo == null)
            {
                InspectGuidDataSearchCondition.RegistrationDateTo = InspectGuidDataSearchCondition.RegistrationDateFrom;
            }

            //次回点検日が選択されていた場合、入力された日付を次回点検日として検索を行う。そうでなければ車検満了日で検索
            if (form["TargetDateFlag"].ToString().Equals("0"))
            {
                InspectGuidDataSearchCondition.NextInspectionDateFrom = CommonUtils.StrToDateTime(form["TargetDateFrom"], DaoConst.SQL_DATETIME_MAX);	//次回点検日From
                InspectGuidDataSearchCondition.NextInspectionDateTo = CommonUtils.StrToDateTime(form["TargetDateTo"], DaoConst.SQL_DATETIME_MAX);	    //次回点検日To
                //開始日だけ入力されていた場合は終了日に同じ値をセットする
                if ((InspectGuidDataSearchCondition.NextInspectionDateFrom != null) && InspectGuidDataSearchCondition.NextInspectionDateTo == null)
                {
                    InspectGuidDataSearchCondition.NextInspectionDateTo = InspectGuidDataSearchCondition.NextInspectionDateFrom;
                }
            }
            else
            {
                InspectGuidDataSearchCondition.ExpireDateFrom = CommonUtils.StrToDateTime(form["TargetDateFrom"], DaoConst.SQL_DATETIME_MAX);	                //車検満了日From
                InspectGuidDataSearchCondition.ExpireDateTo = CommonUtils.StrToDateTime(form["TargetDateTo"], DaoConst.SQL_DATETIME_MAX);	                    //車検満了日To
                //開始日だけ入力されていた場合は終了日に同じ値をセットする
                if ((InspectGuidDataSearchCondition.ExpireDateFrom != null) && InspectGuidDataSearchCondition.ExpireDateTo == null)
                {
                    InspectGuidDataSearchCondition.ExpireDateTo = InspectGuidDataSearchCondition.ExpireDateFrom;
                }
            }

            //顧客種別
            InspectGuidDataSearchCondition.CustomerKind = form["CustomerKind"];

            //住所再確認
            switch (form["CustomerAddressReconfirm"])
            {
                case "001":
                    InspectGuidDataSearchCondition.CustomerAddressReconfirm = true;
                    break;
                case "002":
                    InspectGuidDataSearchCondition.CustomerAddressReconfirm = false;
                    break;
                default:
                    InspectGuidDataSearchCondition.CustomerAddressReconfirm = null;
                    break;
            }

            //住所
            InspectGuidDataSearchCondition.CustomerAddress = form["CustomerAddress"];  //Add 2018/04/25 arc yano #3842


            return new CustomerDao(db).GetInspectGuidListByCondition(InspectGuidDataSearchCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
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
            //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト(車検案内))
            string fileName = DownLoadTime + "_車検点検リスト";

            //Add 2015/08/12 arc nakayama #3241 顧客データリスト（車検案内）画面修正 次回点検日と車検満了日の選択でファイル名を変更する　日付が未入力の場合は「車検点検リスト」
            if (!string.IsNullOrEmpty(form["TargetDateFrom"]) || !string.IsNullOrEmpty(form["TargetDateTo"]))
            {

                if (form["TargetDateFlag"].ToString().Equals("0"))
                {
                    fileName = fileName + "_点検";
                }
                else
                {
                    fileName = fileName + "_車検";
                }
            }

            fileName = fileName + ".xlsx";

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
            string sheetName = "車検点検";    //シート名
            int dateType = 0;                       //データタイプ(帳票形式)
            string setPos = "A1";                   //設定位置
            bool ret = false;

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルなし)
            ExcelPackage excelFile = dExport.MakeExcel(fileName);

            //Add 2015/08/12 arc nakayama #3241 顧客データリスト（車検案内）画面修正 次回点検日と車検満了日の選択でシート名を変更する　日付が未入力の場合は「車検点検リスト」
            if (!string.IsNullOrEmpty(form["TargetDateFrom"]) || !string.IsNullOrEmpty(form["TargetDateTo"]))
            {

                if (form["TargetDateFlag"].ToString().Equals("0"))
                {
                    sheetName = "点検";
                }
                else
                {
                    sheetName = "車検";
                }
            }


            //ワークシート追加
            var worksheet = excelFile.Workbook.Worksheets.Add(sheetName);

            //----------------------------
            // 検索条件出力
            //----------------------------
            //設定値
            configLine = dExport.GetDefaultConfigLine(0, sheetName, dateType, setPos);

            //検索条件取得
            DocumentExportCondition condition = SetCondition(form);

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
            IEnumerable<XElement> data = GetFieldList("InspectGuidText");

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

            //棚卸情報の取得
            List<InspectGuidDataExcelResult> list = new CustomerDao(db).GetInspectGuidListByConditionForExcel(condition);

            //データ設定
            ret = dExport.SetData<InspectGuidDataExcelResult, InspectGuidDataExcelResult>(ref excelFile, list, configLine);
            //戻り値チェック


            excelData = excelFile.GetAsByteArray();

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
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// 2015/06/05 arc nakayama #3202_顧客データリスト画面EXCEL出力対応 新規作成
        /// </history>
        private DocumentExportCondition SetCondition(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            DocumentExportCondition InspectGuidDataSearchCondition = new DocumentExportCondition();
            InspectGuidDataSearchCondition.InspectGuidFlag = form["InspectGuidFlag"];	        //車検案内(DM可否)
            InspectGuidDataSearchCondition.DepartmentCode2 = form["SalesDepartmentCode2"];	    //営業担当部門コード
            InspectGuidDataSearchCondition.CarEmployeeCode = form["CarEmployeeCode"];          //営業担当者コード
            InspectGuidDataSearchCondition.ServiceDepartmentCode2 = form["ServiceDepartmentCode2"];	//サービス担当部門コード
            if (!string.IsNullOrEmpty(form["FirstRegistrationFrom"]))
            {
                InspectGuidDataSearchCondition.DtFirstRegistrationFrom = CommonUtils.StrToDateTime(form["FirstRegistrationFrom"] + "/01", DaoConst.SQL_DATETIME_MAX);	//初年度登録From
            }
            if (!string.IsNullOrEmpty(form["FirstRegistrationTo"]))
            {
                InspectGuidDataSearchCondition.DtFirstRegistrationTo = CommonUtils.StrToDateTime(form["FirstRegistrationTo"] + "/01", DaoConst.SQL_DATETIME_MAX);	    //初年度登録To
            }
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((InspectGuidDataSearchCondition.DtFirstRegistrationFrom != null) && InspectGuidDataSearchCondition.DtFirstRegistrationTo == null)
            {
                InspectGuidDataSearchCondition.DtFirstRegistrationTo = InspectGuidDataSearchCondition.DtFirstRegistrationFrom;
            }
            InspectGuidDataSearchCondition.SalesEmployeeName = form["SalesEmployeeName"];      //担当者名(営業)
            if (!string.IsNullOrEmpty(form["MakerCode"]))
            {
                InspectGuidDataSearchCondition.MakerName = new V_CarMasterDao(db).GetByMakerkey(form["MakerCode"]).MakerName;  //メーカ名
            }
            if (!string.IsNullOrEmpty(form["CarCode"]))
            {
                InspectGuidDataSearchCondition.CarName = new V_CarMasterDao(db).GetByCarkey(form["CarCode"]).CarName; //車種名
            }
            InspectGuidDataSearchCondition.RegistrationDateFrom = CommonUtils.StrToDateTime(form["RegistrationDateFrom"], DaoConst.SQL_DATETIME_MAX);	    //登録年月日From
            InspectGuidDataSearchCondition.RegistrationDateTo = CommonUtils.StrToDateTime(form["RegistrationDateTo"], DaoConst.SQL_DATETIME_MAX);	        //登録年月日To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((InspectGuidDataSearchCondition.RegistrationDateFrom != null) && InspectGuidDataSearchCondition.RegistrationDateTo == null)
            {
                InspectGuidDataSearchCondition.RegistrationDateTo = InspectGuidDataSearchCondition.RegistrationDateFrom;
            }
            //次回点検日が選択されていた場合、入力された日付を次回点検日として検索を行う。そうでなければ車検満了日で検索
            if (form["TargetDateFlag"].ToString().Equals("0"))
            {
                InspectGuidDataSearchCondition.NextInspectionDateFrom = CommonUtils.StrToDateTime(form["TargetDateFrom"], DaoConst.SQL_DATETIME_MAX);	//次回点検日From
                InspectGuidDataSearchCondition.NextInspectionDateTo = CommonUtils.StrToDateTime(form["TargetDateTo"], DaoConst.SQL_DATETIME_MAX);	    //次回点検日To
                //開始日だけ入力されていた場合は終了日に同じ値をセットする
                if ((InspectGuidDataSearchCondition.NextInspectionDateFrom != null) && InspectGuidDataSearchCondition.NextInspectionDateTo == null)
                {
                    InspectGuidDataSearchCondition.NextInspectionDateTo = InspectGuidDataSearchCondition.NextInspectionDateFrom;
                }
            }
            else
            {
                InspectGuidDataSearchCondition.ExpireDateFromForDm = CommonUtils.StrToDateTime(form["TargetDateFrom"], DaoConst.SQL_DATETIME_MAX);	                //車検満了日From
                InspectGuidDataSearchCondition.ExpireDateToForDm = CommonUtils.StrToDateTime(form["TargetDateTo"], DaoConst.SQL_DATETIME_MAX);	                    //車検満了日To
                //開始日だけ入力されていた場合は終了日に同じ値をセットする
                if ((InspectGuidDataSearchCondition.ExpireDateFromForDm != null) && InspectGuidDataSearchCondition.ExpireDateToForDm == null)
                {
                    InspectGuidDataSearchCondition.ExpireDateToForDm = InspectGuidDataSearchCondition.ExpireDateFromForDm;
                }
            }
            //顧客種別
            InspectGuidDataSearchCondition.CustomerKind = form["CustomerKind"];

            //住所再確認
            if (!string.IsNullOrEmpty(form["CustomerAddressReconfirm"]))
            {
                //表示用の名称取得（要/不要）
                //CategoryCode：013(出力 (住所再確認))　Code：001/002(要/不要)
                InspectGuidDataSearchCondition.SearchAddressReconfirmName = dao.GetCodeNameByKey("013", form["CustomerAddressReconfirm"], false).Name;

                //検索用の値に詰め替える
                switch (form["CustomerAddressReconfirm"])
                {
                    case "001":
                        InspectGuidDataSearchCondition.CustomerAddressReconfirm = true;
                        break;
                    case "002":
                        InspectGuidDataSearchCondition.CustomerAddressReconfirm = false;
                        break;
                    default:
                        InspectGuidDataSearchCondition.CustomerAddressReconfirm = null;
                        break;
                }

            }else{
                InspectGuidDataSearchCondition.CustomerAddressReconfirm = null;
                InspectGuidDataSearchCondition.SearchAddressReconfirmName = "";
            }

            if (!string.IsNullOrEmpty(form["CustomerKind"])){
                InspectGuidDataSearchCondition.CustomerKindName = (new CodeDao(db).GetCustomerKindName(form["CustomerKind"], false)).Name;
            }else{
                InspectGuidDataSearchCondition.CustomerKindName = "";
            }

            //検索項目出力用の処理
            if (!string.IsNullOrEmpty(form["InspectGuidFlag"])){
                InspectGuidDataSearchCondition.InspectGuidFlagName = (new CodeDao(db).GetAllowanceName(form["InspectGuidFlag"], false)).Name;
            }else{
                InspectGuidDataSearchCondition.InspectGuidFlagName = "";
            }

            if (string.IsNullOrEmpty(form["SalesDepartmentCode2"]))
            {      //部門名
                InspectGuidDataSearchCondition.DepartmentName2 = "";    //部門コードが未入力なら空文字
            }
            else
            {
                InspectGuidDataSearchCondition.DepartmentName2 = new DepartmentDao(db).GetByKey(form["SalesDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            if (string.IsNullOrEmpty(form["CarEmployeeCode"]))
            {      //社員コード
                InspectGuidDataSearchCondition.CarEmployeeName = "";    //社員コードが未入力なら空文字
            }
            else
            {
                InspectGuidDataSearchCondition.CarEmployeeName = new EmployeeDao(db).GetByKey(form["CarEmployeeCode"].ToString()).EmployeeName;  //入力済みなら社員テーブルから検索
            }
            if (string.IsNullOrEmpty(form["ServiceDepartmentCode2"]))
            {      //部門名
                InspectGuidDataSearchCondition.ServiceDepartmentName2 = "";    //部門コードが未入力なら空文字
            }
            else
            {
                InspectGuidDataSearchCondition.ServiceDepartmentName2 = new DepartmentDao(db).GetByKey(form["ServiceDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }

            //住所
            InspectGuidDataSearchCondition.CustomerAddress = form["CustomerAddress"];  //Add 2018/04/25 arc yano #3842

            return InspectGuidDataSearchCondition;
        }
        #endregion

        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        private DataTable MakeConditionRow(DocumentExportCondition condition)
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

            if (!string.IsNullOrEmpty(condition.InspectGuidFlag))
            {
                conditionText += string.Format("車検案内DM可否={0}　", condition.InspectGuidFlagName);
            }
            if (!string.IsNullOrEmpty(condition.DepartmentCode2))
            {
                conditionText += string.Format("営業担当部門コード={0}:{1}　", condition.DepartmentCode2, condition.DepartmentName2);
            }
            if (!string.IsNullOrEmpty(condition.ServiceDepartmentCode2))
            {
                conditionText += string.Format("サービス担当部門コード={0}:{1}　", condition.ServiceDepartmentCode2, condition.ServiceDepartmentName2);
            }
            if (!string.IsNullOrEmpty(condition.CarEmployeeCode))
            {
                conditionText += string.Format("営業担当者={0}:{1}　", condition.CarEmployeeCode, condition.CarEmployeeName);
            }
            if (!string.IsNullOrEmpty(condition.CustomerKind))
            {
                conditionText += string.Format("顧客種別={0}　", condition.CustomerKindName);
            }
            if (condition.CustomerAddressReconfirm != null)
            {
                conditionText += string.Format("住所再確認={0}　", condition.SearchAddressReconfirmName);
            }
            if (!string.IsNullOrEmpty(condition.MakerName))
            {
                conditionText += string.Format("メーカー名={0}　", condition.MakerName);
            }
            if (!string.IsNullOrEmpty(condition.CarName))
            {
                conditionText += string.Format("車種名={0}　", condition.CarName);
            }
            if (condition.DtFirstRegistrationFrom != null || condition.DtFirstRegistrationTo != null)
            {
                conditionText += string.Format("初年度登録={0:yyyy/MM}～{1:yyyy/MM}　", condition.DtFirstRegistrationFrom, condition.DtFirstRegistrationTo);
            }
            if (condition.RegistrationDateFrom != null || condition.RegistrationDateTo != null)
            {
                conditionText += string.Format("登録年月日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.RegistrationDateFrom, condition.RegistrationDateTo);
            }
            if (condition.NextInspectionDateFrom != null || condition.NextInspectionDateTo != null)
            {
                conditionText += string.Format("次回点検日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.NextInspectionDateFrom, condition.NextInspectionDateTo);
            }
            if (condition.ExpireDateFromForDm != null || condition.ExpireDateToForDm != null)
            {
                conditionText += string.Format("車検満了日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.ExpireDateFromForDm, condition.ExpireDateToForDm);
            }

            //Add 2018/04/25 arc yano #3842
            if (condition.CustomerAddress != null || condition.CustomerAddress != null)
            {
                conditionText += string.Format("住所(都道府県市区町村)={0}　", condition.CustomerAddress);
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
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //検索条件の再セット
            ViewData["InspectGuidList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), form["InspectGuidFlag"], true);
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["CarCode"] = form["CarCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];
            if (!string.IsNullOrEmpty(form["MakerCode"]))
            {
                ViewData["MakerName"] = new V_CarMasterDao(db).GetByMakerkey(form["MakerCode"]).MakerName;
            }
            if (!string.IsNullOrEmpty(form["CarCode"]))
            {
                ViewData["CarName"] = new V_CarMasterDao(db).GetByCarkey(form["CarCode"]).CarName;
            }
            if ((form["PrivateFlag"]).Equals("false")){
                form["PrivateFlag"] = "false";
            }else{
                form["PrivateFlag"] = "true";
            }
            ViewData["PrivateFlag"] = form["PrivateFlag"];
            ViewData["InspectGuidFlag"] = form["InspectGuidFlag"];
            ViewData["SalesDepartmentCode2"] = form["SalesDepartmentCode2"];　//営業担当部門コード
            if (string.IsNullOrEmpty(form["SalesDepartmentCode2"]))
            {      //部門名
                ViewData["SalesDepartmentName2"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
                ViewData["SalesDepartmentName2"] = new DepartmentDao(db).GetByKey(form["SalesDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            ViewData["CarEmployeeCode"] = form["CarEmployeeCode"]; //営業担当者コード
            if (string.IsNullOrEmpty(form["CarEmployeeCode"]))
            {      //社員コード
                ViewData["CarEmployeeName"] = "";    //社員コードが未入力なら空文字
            }
            else
            {
                ViewData["CarEmployeeName"] = new EmployeeDao(db).GetByKey(form["CarEmployeeCode"].ToString()).EmployeeName;  //入力済みなら社員テーブルから検索
            }

            ViewData["ServiceDepartmentCode2"] = form["ServiceDepartmentCode2"];    //サービス担当部門モード

            if (string.IsNullOrEmpty(form["ServiceDepartmentCode2"]))
            {      //部門名
                ViewData["ServiceDepartmentName2"] = "";    //部門コードが未入力なら空文字
            }
            else
            {
                ViewData["ServiceDepartmentName2"] = new DepartmentDao(db).GetByKey(form["ServiceDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            ViewData["CustomerAddressReconfirmList"] = CodeUtils.GetSelectList(CodeUtils.CustomerAddressReconfirmList(), form["CustomerAddressReconfirmList"], false);
            ViewData["CustomerAddressReconfirm"] = form["CustomerAddressReconfirm"]; //住所再確認
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerKind"] = form["CustomerKind"];//顧客種別
            ViewData["FirstRegistrationFrom"] = form["FirstRegistrationFrom"];
            ViewData["FirstRegistrationTo"] = form["FirstRegistrationTo"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["RegistrationDateFrom"] = form["RegistrationDateFrom"];
            ViewData["RegistrationDateTo"] = form["RegistrationDateTo"];
            ViewData["TargetDateFrom"] = form["TargetDateFrom"];
            ViewData["TargetDateTo"] = form["TargetDateTo"];
            ViewData["RequestFlag"] = "1";
            ViewData["DefaultInspectGuidFlag"] = form["DefaultInspectGuidFlag"];    //DM可否(車検案内)
            ViewData["DefaultPrivateFlag"] = form["DefaultPrivateFlag"];            //自社取扱いフラグ
            ViewData["DefaultCustomerAddressReconfirm"] = form["DefaultCustomerAddressReconfirm"]; //住所再確認
            ViewData["DefaultCustomerKind"] = form["DefaultCustomerKind"]; //顧客種別
            ViewData["TargetDateFlag"] = form["TargetDateFlag"];//対象日付（次回点検日か車検満了日か）
            ViewData["DefaultTargetDateFlag"] = form["DefaultTargetDateFlag"]; //対象日付（次回点検日か車検満了日か）

            ViewData["CustomerAddress"] = form["CustomerAddress"]; //顧客住所       //Add 2018/04/25 arc yano #3842
            
            return;
        }

        private void SetDropDownList(FormCollection form)
        {            
            //メーカー名一覧
            List<CodeData> data = new List<CodeData>();
            List<V_CarMaster> MakerList = new List<V_CarMaster>();
            if (string.IsNullOrEmpty(form["PrivateFlag"]) || form["PrivateFlag"].Equals("false")){
                MakerList = new V_CarMasterDao(db).GetPrivateListBykey(null, null);
            }else{
                form["PrivateFlag"] = "1";
                MakerList = new V_CarMasterDao(db).GetPrivateListBykey(null, form["PrivateFlag"]);
            }

            foreach (var item in MakerList)
            {
                data.Add(new CodeData { Code = item.MakerCode, Name = item.MakerName });
            }
            ViewData["MakerList"] = CodeUtils.GetSelectListByModel(data, form["MakerCode"], true);


            //車種名一覧
            //メーカーが選択させれていた場合はそのメーカーの車種を表示　そうでなければ　車種をすべて表示
            List<CodeData> carData = new List<CodeData>();
            List<V_CarMaster> CarMasterList = new List<V_CarMaster>();
            if (!string.IsNullOrEmpty(form["MakerCode"])){
                CarMasterList = new V_CarMasterDao(db).GetCarListBykey(form["MakerCode"]);

            }else{
                //自社取扱いが[無]の場合は全件検索
                if (form["PrivateFlag"].Equals("false")){
                    CarMasterList = new V_CarMasterDao(db).GetCarListBykey(null, null);
                }else{
                    form["PrivateFlag"] = "1";
                    CarMasterList = new V_CarMasterDao(db).GetCarListBykey(null, form["PrivateFlag"]);
                }
            }

            foreach (var car in CarMasterList)
            {
                carData.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
            }

            ViewData["CarList"] = CodeUtils.GetSelectListByModel(carData, form["CarCode"], true);
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

        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog(string id)
        {
            List<InspectionDate> ImportList = new List<InspectionDate>();
            FormCollection form = new FormCollection();
            
            ViewData["ErrFlag"] = "1";
            ViewData["id"] = id;

            return View("InspectGuidListImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<InspectionDate> ImportList = new List<InspectionDate>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("InspectGuidListImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("InspectGuidListImportDialog");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("InspectGuidListImportDialog");
                    }

                    //DB登録
                    DBExecute(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("InspectGuidListImportDialog");

                //--------------
                //キャンセル
                //--------------
                case "2":
                    ImportList = new List<InspectionDate>();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("InspectGuidListImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("InspectGuidListImportDialog");
            }
        }
        #endregion

        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        ///  2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        private List<InspectionDate> ReadExcelData(HttpPostedFileBase importFile, List<InspectionDate> ImportList)
        {
            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets[1];

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
                    return ImportList;
                }
                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　       //行の開始位置
                int EndRow = ws.Dimension.End.Row;             //行の終了位置
                int StartCol = ws.Dimension.Start.Column;      //列の開始位置
                int EndCol = ws.Dimension.End.Column;          //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[ws.Dimension.End.Column];

                for (datarow = StartRow + 2; datarow < EndRow + 1; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        Result[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region Excel読み取り前のチェック
        /// <summary>
        /// Excel読み取り前のチェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        private void ValidateExcelFile(HttpPostedFileBase filePath, FormCollection form)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            return;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        public void ValidateImportList(List<InspectionDate> ImportList, FormCollection form)
        {
            //SalesCar condition = new SalesCar();

            DateTime ret;
            DateTime? previousDate = null;
            DateTime? followDate = null;

            string targetNamePre = (form["id"].Equals("1") ? "車検満期日" : "次回点検日");
            string targetNameFollow = (form["id"].Equals("1") ? "車検満期日（修正）" : "次回点検日（修正）");

            //車両リストを取得する
            //List<SalesCar> carList = new SalesCarDao(db).GetListByCondition(condition);
            
            for (int i = 0; i < ImportList.Count; i++)
            {
                //----------------
                //管理番号
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].SalesCarNumber))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の管理番号が入力されていません。管理番号"));
                }
                else //マスタチェック
                {
                    SalesCar rec = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber);
                    //SalesCar rec = carList.Where(x => Strings.StrConv(x.SalesCarNumber, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].SalesCarNumber)).FirstOrDefault();

                    //車両マスタから検索して見つからなかった場合はエラー
                    if (rec == null)
                    {
                        ModelState.AddModelError("", i + 1 + "行目の管理番号：" + ImportList[i].SalesCarNumber + " はマスタに登録されていません。");
                    }
                    else
                    {
                        previousDate = (form["id"].Equals("1") ? rec.ExpireDate : rec.NextInspectionDate);
                    }
                }
                //---------------
                //変更後の日付
                //---------------
                if (string.IsNullOrEmpty(ImportList[i].FollowDate))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0003", i + 1 + "行目(管理番号：" + ImportList[i].SalesCarNumber + ")の" + targetNameFollow + "が入力されていません。" + targetNameFollow));
                }
                else if (!DateTime.TryParse(ImportList[i].FollowDate, out ret)) //フォーマットチェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0003", i + 1 + "行目(管理番号：" + ImportList[i].SalesCarNumber + ")の" + targetNameFollow + "が日付の形式ではありません。" + targetNameFollow));
                }
                else //変更後の日付が変更前日付と同じまたは過去の日付の場合はエラー
                {
                    //変更後日付の変換
                    if (DateTime.TryParse(ImportList[i].FollowDate, out ret))
                    {
                        followDate = ret;
                    }
                    
                    //変更前日付の取得
                    //if(!string.IsNullOrWhiteSpace(ImportList[i].SalesCarNumber))
                    //{
                    //    previousDate = (form["id"].Equals("1") ? carList.Where(x => Strings.StrConv(x.SalesCarNumber, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].SalesCarNumber)).Select(x => x.ExpireDate).FirstOrDefault() : carList.Where(x => Strings.StrConv(x.SalesCarNumber, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].SalesCarNumber)).Select(x => x.NextInspectionDate).FirstOrDefault()); 
                    //}

                    //変更前日付と変更後日付を比較
                    if (previousDate != null && previousDate >= followDate)
                    {
                        ModelState.AddModelError("", i + 1 + "行目(管理番号：" + ImportList[i].SalesCarNumber + ")の" + targetNameFollow + "の日付は" + targetNamePre + "よりも未来の日付を設定してください");
                    }
                }
            }
        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        ///  2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        public List<InspectionDate> SetProperty(ref string[] Result, ref List<InspectionDate> ImportList)
        {
            InspectionDate SetLine = new InspectionDate();

            // 車両管理番号
            SetLine.SalesCarNumber = Result[0];
            // 車台番号
            SetLine.Vin = Result[1];
            // 変更前日付
            SetLine.PreviousDate = Result[2];
            // 変更後日付
            SetLine.FollowDate = Result[3];

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region ダイアログのデータ付きコンポーネント設定
        /// <summary>
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["id"] = form["id"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(車検点検日一括取込画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/05/25 arc yano #3888 Excel取込(車検点検更新)
        /// </history>
        private void DBExecute(List<InspectionDate> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {

                //車両マスタの取得
                //SalesCar condition = new SalesCar();
                //List<SalesCar> carList = new SalesCarDao(db).GetListByCondition(condition);

                //車検点検日の更新
                foreach (var LineData in ImportList)
                {
                    //対象の部品マスタを取得
                    SalesCar target = new SalesCarDao(db).GetByKey(LineData.SalesCarNumber);
                    //SalesCar target = carList.Where(x => x.SalesCarNumber.Equals(LineData.SalesCarNumber)).FirstOrDefault();

                    //変更後日付の更新
                    if (form["id"].Equals("1"))
                    {
                        target.ExpireDate = DateTime.Parse(LineData.FollowDate);
                    }
                    else
                    {
                        target.NextInspectionDate = DateTime.Parse(LineData.FollowDate);
                    }
                }
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

    }
}
