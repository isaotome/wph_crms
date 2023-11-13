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
// Add 2015/06/01 arc nakyama #3202_顧客データリスト画面EXCEL出力対応
using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
namespace Crms.Controllers
{
    /// <summary>
    /// 顧客データリスト検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerDataListController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "顧客データリスト（営業案内）";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "顧客データリスト（営業案内）検索"; // 処理名（ログ出力用）　
        private static readonly string PROC_NAME_CSV = "顧客データリスト（営業案内）Excel出力";       // 処理名(CSV出力)
        private static readonly string DM_NASHI = "";     //DMの可否（指定なし）
        //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
        private static readonly string AddressReconfirm_NASHI = ""; //住所再確認（指定なし）
        private static readonly string CustomerKind_NASHI = "";     //顧客種別（指定なし）
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
        public CustomerDataListController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        /// <summary>
        /// 顧客データリスト検索画面表示
        /// </summary>
        /// <returns>顧客データリスト検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
            form["DmFlag"] = DM_NASHI;                //デフォルト値（スペース）
            form["DefaultDmFlag"] = form["DmFlag"];
            form["RequestFlag"] = "1";  //リクエストフラグ　デフォルト値 = 1 
            form["PrivateFlag"] = "true";  //自社取扱いフラグ  デフォルト値 = 0
            form["DefaultPrivateFlag"] = form["PrivateFlag"]; //自社取扱いフラグ
            form["DefaultCustomerAddressReconfirm"] = AddressReconfirm_NASHI;//デフォルト値（スペース）住所再確認
            form["DefaultCustomerAddressReconfirm"] = form["CustomerAddressReconfirm"];//デフォルト値（スペース）住所再確認
            form["CustomerKind"] = CustomerKind_NASHI; //デフォルト値（スペース）顧客種別
            form["DefaultCustomerKind"] = form["CustomerKind"];
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
                        return View("CustomerDataListCriteria", list);
                }
                
            }
            // 検索項目の設定
            SetDropDownList(form);
            SetComponent(form);
            return View("CustomerDataListCriteria", list);
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

            return View("CustomerDataListCriteria", list);

        }
        #endregion

        #region　顧客データリスト検索
        /// <summary>
        /// 検索ボタン押下処理
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加
        /// </history>
        private PaginatedList<CustomerDataResult> GetSearchResultList(FormCollection form)
        {
            //検索項目セット
            CustomerDataSearch CustomerDataSearchCondition = new CustomerDataSearch();
            CodeDao dao = new CodeDao(db);
            CustomerDataSearchCondition.DmFlag = form["DmFlag"];	                            //営業(DM可否)
            //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
            //CustomerDataSearchCondition.DmMemo = form["DmMemo"];	                            //備考(営業)
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
            CustomerDataSearchCondition.DepartmentCode2 = form["SalesDepartmentCode2"];	    //営業担当部門コード
            CustomerDataSearchCondition.CarEmployeeCode = form["CarEmployeeCode"];          //営業担当者コード
            CustomerDataSearchCondition.ServiceDepartmentCode2 = form["ServiceDepartmentCode2"];	//サービス担当部門コード
            if (!string.IsNullOrEmpty(form["FirstRegistrationFrom"]))
            {
                CustomerDataSearchCondition.FirstRegistrationFrom = CommonUtils.StrToDateTime(form["FirstRegistrationFrom"] + "/01", DaoConst.SQL_DATETIME_MAX);	//初年度登録From
            }
            if (!string.IsNullOrEmpty(form["FirstRegistrationTo"]))
            {
                CustomerDataSearchCondition.FirstRegistrationTo = CommonUtils.StrToDateTime(form["FirstRegistrationTo"] + "/01", DaoConst.SQL_DATETIME_MAX);	    //初年度登録To
            }
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.FirstRegistrationFrom != null) && CustomerDataSearchCondition.FirstRegistrationTo == null)
            {
                CustomerDataSearchCondition.FirstRegistrationTo = CustomerDataSearchCondition.FirstRegistrationFrom;
            }
            //Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）
            CustomerDataSearchCondition.SalesEmployeeName = form["SalesEmployeeName"];      //担当者名(営業)
            //CustomerDataSearchCondition.ServiceEmployeeName = form["ServiceEmployeeName"];  //担当者名(サービス)
            if (!string.IsNullOrEmpty(form["MakerCode"]))
            {
                CustomerDataSearchCondition.MakerName = new V_CarMasterDao(db).GetByMakerkey(form["MakerCode"]).MakerName;  //メーカ名
            }
            if (!string.IsNullOrEmpty(form["CarCode"]))
            {
                CustomerDataSearchCondition.CarName = new V_CarMasterDao(db).GetByCarkey(form["CarCode"]).CarName; //車種名
            } 
            CustomerDataSearchCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"], DaoConst.SQL_DATETIME_MAX);                    //納車年月日(営業)From
            CustomerDataSearchCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"], DaoConst.SQL_DATETIME_MAX);                        //納車年月日(営業)To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.SalesDateFrom != null) && CustomerDataSearchCondition.SalesDateTo == null)
            {
                CustomerDataSearchCondition.SalesDateTo = CustomerDataSearchCondition.SalesDateFrom;
            }
            //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
            CustomerDataSearchCondition.ArrivalPlanDateFrom = CommonUtils.StrToDateTime(form["ArrivalPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);      //納車年月日(サービス)From
            CustomerDataSearchCondition.ArrivalPlanDateTo = CommonUtils.StrToDateTime(form["ArrivalPlanDateTo"], DaoConst.SQL_DATETIME_MAX);          //納車年月日(サービス)To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.ArrivalPlanDateFrom != null) && CustomerDataSearchCondition.ArrivalPlanDateTo == null)
            {
                CustomerDataSearchCondition.ArrivalPlanDateTo = CustomerDataSearchCondition.ArrivalPlanDateFrom;
            }            
            CustomerDataSearchCondition.RegistrationDateFrom = CommonUtils.StrToDateTime(form["RegistrationDateFrom"], DaoConst.SQL_DATETIME_MAX);	    //登録年月日From
            CustomerDataSearchCondition.RegistrationDateTo = CommonUtils.StrToDateTime(form["RegistrationDateTo"], DaoConst.SQL_DATETIME_MAX);	        //登録年月日To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.RegistrationDateFrom != null) && CustomerDataSearchCondition.RegistrationDateTo == null)
            {
                CustomerDataSearchCondition.RegistrationDateTo = CustomerDataSearchCondition.RegistrationDateFrom;
            }
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除

            //Add 2015/04/14 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
            CustomerDataSearchCondition.CustomerKind = form["CustomerKind"];

            //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
            switch (form["CustomerAddressReconfirm"])
            {
                case "001":
                    CustomerDataSearchCondition.CustomerAddressReconfirm = true;
                    break;
                case "002":
                    CustomerDataSearchCondition.CustomerAddressReconfirm = false;
                    break;
                default:
                    CustomerDataSearchCondition.CustomerAddressReconfirm = null;
                    break;
            }


            //住所
            CustomerDataSearchCondition.CustomerAddress = form["CustomerAddress"];  //Add 2018/04/25 arc yano #3842

            return new CustomerDao(db).GetCustomerDataListByCondition(CustomerDataSearchCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region Excelボタン押下
        /// <summary>
        /// Excelボタン押下
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2015/06/05 arc nakayama #3202_顧客データリスト画面EXCEL出力対応 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CSV);

            // Add 2015/05/20 #3202_顧客データリスト画面EXCEL出力対応　Excel出力の処理を追加
            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名(yyyyMMdd(ダウンロード年月日)_顧客データリスト)
            string fileName = DownLoadTime + "_顧客データリスト（営業案内）" + ".xlsx";

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
        /// <history>
        /// 2015/06/05 arc nakayama #3202_顧客データリスト画面EXCEL出力対応 新規作成
        /// </history>
        private byte[] MakeExcelData(FormCollection form, string fileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            string sheetName = "CustomerDataList";    //シート名
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
            IEnumerable<XElement> data = GetFieldList("CustomerDataText");

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
            List<CustomerDataExcelResult> list = new CustomerDao(db).GetCustomerDataListByConditionForCSV(condition);

            //データ設定
            ret = dExport.SetData<CustomerDataExcelResult, CustomerDataExcelResult>(ref excelFile, list, configLine);
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
            DocumentExportCondition CustomerDataSearchCondition = new DocumentExportCondition();
            CustomerDataSearchCondition.DmFlag = form["DmFlag"];	                            //営業(DM可否)
            //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
            //CustomerDataSearchCondition.DmMemo = form["DmMemo"];	                            //備考(営業)
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
            CustomerDataSearchCondition.DepartmentCode2 = form["SalesDepartmentCode2"];	    //営業担当部門コード
            CustomerDataSearchCondition.CarEmployeeCode = form["CarEmployeeCode"];          //営業担当者コード
            CustomerDataSearchCondition.ServiceDepartmentCode2 = form["ServiceDepartmentCode2"];	//サービス担当部門コード
            if (!string.IsNullOrEmpty(form["FirstRegistrationFrom"]))
            {
                CustomerDataSearchCondition.DtFirstRegistrationFrom = CommonUtils.StrToDateTime(form["FirstRegistrationFrom"] + "/01", DaoConst.SQL_DATETIME_MAX);	//初年度登録From
            }
            if (!string.IsNullOrEmpty(form["FirstRegistrationTo"]))
            {
                CustomerDataSearchCondition.DtFirstRegistrationTo = CommonUtils.StrToDateTime(form["FirstRegistrationTo"] + "/01", DaoConst.SQL_DATETIME_MAX);	    //初年度登録To
            }
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.DtFirstRegistrationFrom != null) && CustomerDataSearchCondition.DtFirstRegistrationTo == null)
            {
                CustomerDataSearchCondition.DtFirstRegistrationTo = CustomerDataSearchCondition.DtFirstRegistrationFrom;
            }
            //Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）
            CustomerDataSearchCondition.SalesEmployeeName = form["SalesEmployeeName"];      //担当者名(営業)
            //CustomerDataSearchCondition.ServiceEmployeeName = form["ServiceEmployeeName"];  //担当者名(サービス)
            if (!string.IsNullOrEmpty(form["MakerCode"]))
            {
                CustomerDataSearchCondition.MakerName = new V_CarMasterDao(db).GetByMakerkey(form["MakerCode"]).MakerName;  //メーカ名
            }
            if (!string.IsNullOrEmpty(form["CarCode"]))
            {
                CustomerDataSearchCondition.CarName = new V_CarMasterDao(db).GetByCarkey(form["CarCode"]).CarName; //車種名
            }
            CustomerDataSearchCondition.SalesDateFromForDm = CommonUtils.StrToDateTime(form["SalesDateFrom"], DaoConst.SQL_DATETIME_MAX);                    //納車年月日(営業)From
            CustomerDataSearchCondition.SalesDateToForDm = CommonUtils.StrToDateTime(form["SalesDateTo"], DaoConst.SQL_DATETIME_MAX);                        //納車年月日(営業)To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.SalesDateFromForDm != null) && CustomerDataSearchCondition.SalesDateToForDm == null)
            {
                CustomerDataSearchCondition.SalesDateToForDm = CustomerDataSearchCondition.SalesDateFromForDm;
            }
            //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
            CustomerDataSearchCondition.ArrivalPlanDateFrom = CommonUtils.StrToDateTime(form["ArrivalPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);      //納車年月日(サービス)From
            CustomerDataSearchCondition.ArrivalPlanDateTo = CommonUtils.StrToDateTime(form["ArrivalPlanDateTo"], DaoConst.SQL_DATETIME_MAX);          //納車年月日(サービス)To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.ArrivalPlanDateFrom != null) && CustomerDataSearchCondition.ArrivalPlanDateTo == null)
            {
                CustomerDataSearchCondition.ArrivalPlanDateTo = CustomerDataSearchCondition.ArrivalPlanDateFrom;
            }
            CustomerDataSearchCondition.RegistrationDateFrom = CommonUtils.StrToDateTime(form["RegistrationDateFrom"], DaoConst.SQL_DATETIME_MAX);	    //登録年月日From
            CustomerDataSearchCondition.RegistrationDateTo = CommonUtils.StrToDateTime(form["RegistrationDateTo"], DaoConst.SQL_DATETIME_MAX);	        //登録年月日To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((CustomerDataSearchCondition.RegistrationDateFrom != null) && CustomerDataSearchCondition.RegistrationDateTo == null)
            {
                CustomerDataSearchCondition.RegistrationDateTo = CustomerDataSearchCondition.RegistrationDateFrom;
            }
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除

            //Add 2015/04/14 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
            CustomerDataSearchCondition.CustomerKind = form["CustomerKind"];

            //Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
            if (!string.IsNullOrEmpty(form["CustomerAddressReconfirm"]))
            {
                //表示用の名称取得（要/不要）
                //CategoryCode：013(出力 (住所再確認))　Code：001/002(要/不要)
                CustomerDataSearchCondition.SearchAddressReconfirmName = dao.GetCodeNameByKey("013", form["CustomerAddressReconfirm"], false).Name;

                //検索用の値に詰め替える
                switch (form["CustomerAddressReconfirm"])
                {
                    case "001":
                        CustomerDataSearchCondition.CustomerAddressReconfirm = true;
                        break;
                    case "002":
                        CustomerDataSearchCondition.CustomerAddressReconfirm = false;
                        break;
                    default:
                        CustomerDataSearchCondition.CustomerAddressReconfirm = null;
                        break;
                }

            }else{
                CustomerDataSearchCondition.CustomerAddressReconfirm = null;
                CustomerDataSearchCondition.SearchAddressReconfirmName = "";
            }

            //Add 2015/04/14 arc nakayama 顧客DM追加項目　顧客種別を検索条件に入れる
            //Mod 2015/06/17 arc nakayama 名称をマスタから取得するように変更
            if (!string.IsNullOrEmpty(form["CustomerKind"])){
                CustomerDataSearchCondition.CustomerKindName = (new CodeDao(db).GetCustomerKindName(form["CustomerKind"], false)).Name;
            }else{
                CustomerDataSearchCondition.CustomerKindName = "";
            }

            //検索項目出力用の処理
            if (!string.IsNullOrEmpty(form["DmFlag"])){
                CustomerDataSearchCondition.DmFlagName = (new CodeDao(db).GetAllowanceName(form["DmFlag"], false)).Name;
            }else{
                CustomerDataSearchCondition.DmFlagName = "";
            }

            if (string.IsNullOrEmpty(form["SalesDepartmentCode2"]))
            {      //部門名
                CustomerDataSearchCondition.DepartmentName2 = "";    //部門コードが未入力なら空文字
            }
            else
            {
                CustomerDataSearchCondition.DepartmentName2 = new DepartmentDao(db).GetByKey(form["SalesDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }
            if (string.IsNullOrEmpty(form["CarEmployeeCode"]))
            {      //社員コード
                CustomerDataSearchCondition.CarEmployeeName = "";    //社員コードが未入力なら空文字
            }
            else
            {
                //Mod 2016/02/03 ARC Mikami #3277_営業担当者Delflag検索対応
                CustomerDataSearchCondition.CarEmployeeName = new EmployeeDao(db).GetByKey(form["CarEmployeeCode"].ToString(),true).EmployeeName;  //入力済みなら社員テーブルから検索
            }
            if (string.IsNullOrEmpty(form["ServiceDepartmentCode2"]))
            {      //部門名
                CustomerDataSearchCondition.ServiceDepartmentName2 = "";    //部門コードが未入力なら空文字
            }
            else
            {
                CustomerDataSearchCondition.ServiceDepartmentName2 = new DepartmentDao(db).GetByKey(form["ServiceDepartmentCode2"].ToString()).DepartmentName;  //入力済みなら部門テーブルから検索
            }

            //住所
            CustomerDataSearchCondition.CustomerAddress = form["CustomerAddress"];  //Add 2018/04/25 arc yano #3842


            //検索実行

            return CustomerDataSearchCondition;
        }
        #endregion

        //Add 2015/06/05 arc nakayama #3202_顧客データリスト画面EXCEL出力対応
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

            if (!string.IsNullOrEmpty(condition.DmFlag))
            {
                conditionText += string.Format("営業DM可否={0}　", condition.DmFlagName);
            }
            if (!string.IsNullOrEmpty(condition.DepartmentCode2))
            {
                conditionText += string.Format("営業担当部門コード={0}:{1}　", condition.DepartmentCode2, condition.DepartmentName2);
            }
            if (!string.IsNullOrEmpty(condition.ServiceDepartmentCode2))
            {
                conditionText += string.Format("サービス担当部門コード={0}:{1}　", condition.ServiceDepartmentCode2, condition.ServiceDepartmentName2);
            }
            if (condition.SalesDateFromForDm != null || condition.SalesDateToForDm != null)
            {
                conditionText += string.Format("車両伝票納車日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.SalesDateFromForDm, condition.SalesDateToForDm);
            }
            //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
            if (condition.ArrivalPlanDateFrom != null || condition.ArrivalPlanDateTo != null)
            {
                conditionText += string.Format("サービス伝票入庫日={0:yyyy/MM/dd}～{1:yyyy/MM/dd}　", condition.ArrivalPlanDateFrom, condition.ArrivalPlanDateTo);
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
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除

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

        //Add 2015/06/05 arc nakayama #3202_顧客データリスト画面EXCEL出力対応
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
            //Mod 2015/01/08 arc nakayama 顧客DM指摘事項⑥・⑧　DM可否・車検案内に「スペース」を設ける(検索のみ)
            //ViewData["DmFlagList"] = CodeUtils.GetSelectList(CodeUtils.DmFlagList(), form["DmFlag"], false);
            ViewData["DmFlagList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), form["DmFlag"], true);
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
            ViewData["DefaultDmFlag"] = form["DmFlag"];
            ViewData["DmFlag"] = form["DmFlag"];
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
            //Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト
            //ViewData["DmMemo"] = form["DmMemo"];
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
                //Mod 2016/02/02 ARC Mikami #3277_営業担当者Delflag検索対応
                ViewData["CarEmployeeName"] = new EmployeeDao(db).GetByKey(form["CarEmployeeCode"].ToString(),true).EmployeeName;  //入力済みなら社員テーブルから検索
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
            // Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　住所再確認を検索条件に入れる
            ViewData["CustomerAddressReconfirmList"] = CodeUtils.GetSelectList(CodeUtils.CustomerAddressReconfirmList(), form["CustomerAddressReconfirmList"], false);
            ViewData["CustomerAddressReconfirm"] = form["CustomerAddressReconfirm"]; //住所再確認
            //ViewData["CustomerKindList"] = CodeUtils.GetSelectList(CodeUtils.CustomerKindList(), form["CustomerKind"], false); //顧客種別
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerKind"] = form["CustomerKind"];//顧客種別
            ViewData["FirstRegistrationFrom"] = form["FirstRegistrationFrom"];
            ViewData["FirstRegistrationTo"] = form["FirstRegistrationTo"];
            //Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            //Mod 2015/07/17 arc nakayama サービス伝票納車日を入庫日に変更（ServiceSalesDate ⇒ ArrivalPlanDate）
            ViewData["ArrivalPlanDateFrom"] = form["ArrivalPlanDateFrom"];
            ViewData["ArrivalPlanDateTo"] = form["ArrivalPlanDateTo"];
            ViewData["RegistrationDateFrom"] = form["RegistrationDateFrom"];
            ViewData["RegistrationDateTo"] = form["RegistrationDateTo"];
            //Del 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合 車検案内に関する条件/項目を削除
            ViewData["RequestFlag"] = "1";
            ViewData["DefaultDmFlag"] = form["DefaultDmFlag"];                      //DM可否(営業)
            ViewData["DefaultPrivateFlag"] = form["DefaultPrivateFlag"];            //自社取扱いフラグ
            ViewData["DefaultCustomerAddressReconfirm"] = form["DefaultCustomerAddressReconfirm"]; //住所再確認
            ViewData["DefaultCustomerKind"] = form["DefaultCustomerKind"]; //顧客種別

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

        //Add 2014/12/22 arc nakayama IPO対応(顧客DM検索) 処理中対応
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