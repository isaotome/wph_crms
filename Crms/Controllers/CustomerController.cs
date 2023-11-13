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
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// 顧客マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerController : Controller {

        //Add 2014/08/01 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "顧客マスタ";     // 画面名
        private static readonly string PROC_NAME = "顧客マスタ登録"; // 処理名

        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CustomerController() {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 顧客検索
        /// <summary>
        /// 顧客検索画面表示
        /// </summary>
        /// <returns>顧客検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 顧客検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>顧客検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Customer> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), form["CustomerRank"], true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), form["CustomerType"], true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), form["Sex"], true);
            ViewData["BirthdayFrom"] = form["BirthdayFrom"];
            ViewData["BirthdayTo"] = form["BirthdayTo"];
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), form["Occupation"], true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), form["CarOwner"], true);
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];

            // 顧客検索画面の表示
            return View("CustomerCriteria", list);
        }

        /// <summary>
        /// 顧客検索ダイアログ表示
        /// </summary>
        /// <returns>顧客検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 顧客検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>顧客検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CustomerCode"] = Request["CustomerCode"];
            form["CustomerRank"] = Request["CustomerRank"];
            form["CustomerKind"] = Request["CustomerKind"];
            form["CustomerName"] = Request["CustomerName"];
            form["CustomerType"] = Request["CustomerType"];
            form["Sex"] = Request["Sex"];
            form["BirthdayFrom"] = Request["BirthdayFrom"];
            form["BirthdayTo"] = Request["BirthdayTo"];
            form["Occupation"] = Request["Occupation"];
            form["CarOwner"] = Request["CarOwner"];
            form["TelNumber"] = Request["TelNumber"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Customer> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), form["CustomerRank"], true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), form["CustomerType"], true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), form["Sex"], true);
            ViewData["BirthdayFrom"] = form["BirthdayFrom"];
            ViewData["BirthdayTo"] = form["BirthdayTo"];
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), form["Occupation"], true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), form["CarOwner"], true);
            ViewData["TelNumber"] = form["TelNumber"];

            // 顧客検索画面の表示
            return View("CustomerCriteriaDialog", list);
        }
        #endregion

        #region 入力画面
        /// <summary>
        /// 顧客マスタ入力画面表示
        /// </summary>
        /// <param name="id">顧客コード(更新時のみ設定)</param>
        /// <returns>顧客マスタ入力画面</returns>
        /// <remarks>2014/08/04 未使用</remarks>
        [AuthFilter]
        public ActionResult Entry(string id) {

            Customer customer;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                customer = new Customer();
                customer.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                customer.CustomerType = "001";
            }
                // 更新の場合
            else {
                ViewData["update"] = "1";
                customer = new CustomerDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(customer);

            //請求先宛名の追加
            ViewData["NewCustomerClaimName"] = customer.CustomerClaim != null ? customer.CustomerClaim.CustomerClaimName : "";

            // 出口
            return View("CustomerEntry", customer);
        }

        /// <summary>
        /// 顧客マスタ追加更新
        /// </summary>
        /// <param name="customer">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>顧客マスタ入力画面</returns>
        /// <remarks>2014/08/04 未使用</remarks>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Customer customer, FormCollection form) {

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];
            ViewData["NewCustomerClaimName"] = form["NewCustomerClaimName"];
            ViewData["UpdateCustomerClaim"] = !string.IsNullOrEmpty(form["UpdateCustomerClaim"]) && form["UpdateCustomerClaim"].Contains("true") ? true : false;
            
            // データチェック
            ValidateCustomer(customer, form);
            if (!ModelState.IsValid) {
                GetEntryViewData(customer);
                return View("CustomerEntry", customer);
            }

            // Add 2014/08/04 arc amii エラーログ対応 ログファイルにSQLを出力する処理追加
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            

            // データ更新処理
            if (form["update"].Equals("1")) {
                // データ編集・更新
                Customer targetCustomer = new CustomerDao(db).GetByKey(customer.CustomerCode);
                UpdateModel(targetCustomer);
                EditCustomerForUpdate(targetCustomer);

                //請求先にもコピーするチェック
                if (!string.IsNullOrEmpty(form["UpdateCustomerClaim"]) && form["UpdateCustomerClaim"].Contains("true")) {
                    if (targetCustomer.CustomerClaim != null) {
                        targetCustomer.CustomerClaim.CustomerClaimName = form["CustomerClaimName"];
                        targetCustomer.CustomerClaim.PostCode = customer.PostCode;
                        targetCustomer.CustomerClaim.Prefecture = customer.Prefecture;
                        targetCustomer.CustomerClaim.City = customer.City;
                        targetCustomer.CustomerClaim.Address1 = customer.Address1;
                        targetCustomer.CustomerClaim.Address2 = customer.Address2;
                        targetCustomer.CustomerClaim.TelNumber1 = customer.TelNumber;
                        targetCustomer.CustomerClaim.FaxNumber = customer.FaxNumber;
                    }
                }
            }
                // データ追加処理
            else 
            {
                // データ編集
                customer = EditCustomerForInsert(customer);
                
                //請求先の追加
                CustomerClaim customerClaim = CreateCustomerClaim(customer);
                if (customerClaim != null) {
                    db.CustomerClaim.InsertOnSubmit(customerClaim);
                    if (string.IsNullOrEmpty(customer.CustomerClaimCode)) {
                        customer.CustomerClaimCode = customerClaim.CustomerClaimCode;
                    }
                }
                // データ追加
                db.Customer.InsertOnSubmit(customer);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0011", "保存"));
                        GetEntryViewData(customer);
                        return View("CustomerEntry", customer);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // 上記以外の例外の場合、エラーログ出力し、エラー画面に遷移する
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            
            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        #endregion

        #region Ajax
        /// <summary>
        /// 顧客コードから顧客名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">顧客コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Customer customer = new CustomerDao(db).GetByKey(code);
                if (customer != null) {
                    codeData.Code = customer.CustomerCode;
                    codeData.Name = customer.CustomerName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 顧客コードから顧客詳細情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">顧客コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retCustomer = new Dictionary<string, string>();
                Customer customer = new CustomerDao(db).GetByKey(code);
                if (customer != null) {
                    retCustomer.Add("CustomerCode", customer.CustomerCode);
                    retCustomer.Add("CustomerName", customer.CustomerName);
                    retCustomer.Add("CustomerAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("CustomerNameKana", customer.CustomerNameKana);
                    retCustomer.Add("Prefecture", customer.Prefecture);
                    retCustomer.Add("City", customer.City);
                    retCustomer.Add("Address1", customer.Address1);
                    retCustomer.Add("Address2", customer.Address2);
                    retCustomer.Add("OwnerCode", customer.CustomerCode);
                    retCustomer.Add("PossesorCode", customer.CustomerCode);
                    retCustomer.Add("PossesorName", customer.CustomerName);
                    retCustomer.Add("PossesorAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("UserCode", customer.CustomerCode);
                    retCustomer.Add("UserName", customer.CustomerName);
                    retCustomer.Add("UserAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("PrincipalPlace", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("CustomerRankName", (customer.c_CustomerRank == null ? null : customer.c_CustomerRank.Name));
                    retCustomer.Add("CustomerTelNumber", customer.TelNumber);
                    // Add 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 サービス or 車両伝票で警告表示に使用する住所再確認フラグの取得を追加
                    retCustomer.Add("AddressReconfirm", CommonUtils.DefaultString(customer.AddressReconfirm));

                    retCustomer.Add("CustomerMemo", customer.Memo); //Add 2022/02/09 yano 

                    if (customer.AddressReconfirm == true)
                    {
                        //ViewData["ReconfirmMessage"] = "住所を再確認してください";
                        retCustomer.Add("ReconfirmMessage", "住所を再確認してください");
                    }
                    else
                    {
                        ViewData["ReconfirmMessage"] = "";
                        retCustomer.Add("ReconfirmMessage", "");
                    }

                }
                return Json(retCustomer);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 顧客コードから顧客詳細情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">顧客コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetailForCustomerIntegrate(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retCustomer = new Dictionary<string, string>();
                GetCustomerIntegrateDataResult customer = new CustomerDao(db).GetCustomerIntegrateData(code);
                if (customer != null)
                {
                    //retCustomer.Add("0", customer.CustomerCode);
                    retCustomer.Add("0", customer.CustomerName);
                    retCustomer.Add("1", customer.TelNumber);
                    retCustomer.Add("2", customer.MobileNumber);
                    retCustomer.Add("3", customer.PostCode);
                    retCustomer.Add("4", customer.Prefecture);
                    retCustomer.Add("5", customer.City);
                    retCustomer.Add("6", customer.Address1);
                    retCustomer.Add("7", customer.Address2);
                    if (customer.CarCnt != null)
                    {
                        retCustomer.Add("8", customer.CarCnt.ToString());
                    }
                    else
                    {
                        retCustomer.Add("8", "");
                    }
                    if (customer.ServiceCnt != null)
                    {
                        retCustomer.Add("9", customer.ServiceCnt.ToString());
                    }
                    else
                    {
                        retCustomer.Add("9", "");
                    }
                }
                return Json(retCustomer);
            }
            return new EmptyResult();
        }

        #endregion

        #region 画面コンポーネント
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="customer">モデルデータ</param>
        /// <history>
        /// 2019/01/18 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// </history>
        private void GetEntryViewData(Customer customer) {

            // 請求先名の取得
            if (!string.IsNullOrEmpty(customer.CustomerClaimCode)) {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(customer.CustomerClaimCode);
                if (customerClaim != null) {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }

            // 部門名の取得
            DepartmentDao departmentDao = new DepartmentDao(db);
            if (!string.IsNullOrEmpty(customer.DepartmentCode)) {
                
                Department department = departmentDao.GetByKey(customer.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(customer.ServiceDepartmentCode)) {
                Department serviceDepartment = departmentDao.GetByKey(customer.ServiceDepartmentCode);
                if (serviceDepartment != null) {
                    ViewData["ServiceDepartmentName"] = serviceDepartment.DepartmentName;
                }
            }

            // 営業担当者，サービス担当者名の取得
            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employee;
            if (!string.IsNullOrEmpty(customer.CarEmployeeCode)) {
                employee = employeeDao.GetByKey(customer.CarEmployeeCode);
                if (employee != null) {
                    ViewData["CarEmployeeName"] = employee.EmployeeName;
                }
            }
            if (!string.IsNullOrEmpty(customer.ServiceEmployeeCode)) {
                employee = employeeDao.GetByKey(customer.ServiceEmployeeCode);
                if (employee != null) {
                    ViewData["ServiceEmployeeName"] = employee.EmployeeName;
                }
            }

            //セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), customer.CustomerRank, true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), customer.CustomerKind, true);
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), customer.CustomerType, true);
            ViewData["PaymentKindList"] = CodeUtils.GetSelectListByModel(dao.GetPaymentKindAll(false), customer.PaymentKind, true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), customer.Sex, true);
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), customer.Occupation, true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), customer.CarOwner, true);
            //Mod 2015/01/08 arc nakayama 顧客DM指摘事項⑥・⑧　DM可否を可・不可のどちらかにする(他・スペースは削除)
            ViewData["DmFlagList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), customer.DmFlag, false);
            ViewData["CorporationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCorporationTypeAll(false), customer.CorporationType, true);

            ViewData["dm.CorporationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCorporationTypeAll(false), (customer.CustomerDM != null ? customer.CustomerDM.CorporationType : ""), true);   //Mod 2019/01/18 yano #3965

        }

        #endregion


        /// <summary>
        /// 顧客マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>顧客マスタ検索結果リスト</returns>
        private PaginatedList<Customer> GetSearchResultList(FormCollection form) {

            CustomerDao customerDao = new CustomerDao(db);
            Customer customerCondition = new Customer();
            customerCondition.CustomerCode = form["CustomerCode"];
            customerCondition.CustomerRank = form["CustomerRank"];
            customerCondition.CustomerKind = form["CustomerKind"];
            customerCondition.CustomerName = form["CustomerName"];
            customerCondition.CustomerNameKana = form["CustomerNameKana"];
            customerCondition.CustomerType = form["CustomerType"];
            customerCondition.Sex = form["Sex"];
            customerCondition.BirthdayFrom = CommonUtils.StrToDateTime(form["BirthdayFrom"], DaoConst.SQL_DATETIME_MAX);
            customerCondition.BirthdayTo = CommonUtils.StrToDateTime(form["BirthdayTo"], DaoConst.SQL_DATETIME_MIN);
            customerCondition.Occupation = form["Occupation"];
            customerCondition.CarOwner = form["CarOwner"];
            customerCondition.TelNumber = form["TelNumber"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                customerCondition.DelFlag = form["DelFlag"];
            }
            customerCondition.CustomerNameKana = form["CustomerNameKana"];
            return customerDao.GetListByCondition(customerCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        #region Validation
        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>顧客データ</returns>
        private Customer ValidateCustomer(Customer customer, FormCollection form) {

            // 必須チェック
            if (string.IsNullOrEmpty(customer.FirstName)) {
                ModelState.AddModelError("FirstName", MessageUtils.GetMessage("E0001", "顧客名1(姓)"));
            }
            if (string.IsNullOrEmpty(customer.FirstNameKana)) {
                ModelState.AddModelError("FirstNameKana", MessageUtils.GetMessage("E0001", "顧客名1(姓カナ)"));
            }
            if (string.IsNullOrEmpty(customer.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門" ));
            }
            if (string.IsNullOrEmpty(customer.ServiceDepartmentCode)) {
                ModelState.AddModelError("ServiceDepartmentCode", MessageUtils.GetMessage("E0001", "サービス担当部門"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Birthday")) {
                ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0005", "生年月日"));
            }
            if (!ModelState.IsValidField("FirstReceiptionDate")) {
                ModelState.AddModelError("FirstReceiptionDate", MessageUtils.GetMessage("E0005", "初回来店日"));
            }
            if (!ModelState.IsValidField("LastReceiptionDate")) {
                ModelState.AddModelError("LastReceiptionDate", MessageUtils.GetMessage("E0005", "前回来店日"));
            }

            // 形式チェック
            // Add 2015/01/09 arc nakayama 顧客DM指摘事項④ 登録時の郵便番号はハイフンなしの７桁か、ハイフンありの８桁にする。また、７桁でハイフンが入っていなければハイフンを入れる
            if (!string.IsNullOrEmpty(customer.PostCode)){
                if ((!Regex.IsMatch(customer.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customer.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("PostCode", MessageUtils.GetMessage("E0026", "郵便番号"));
                }
            }

            // ADD 2016/02/08 ARC Mikami #3428_顧客マスタ_生年月日バリデーションチェック
            if (customer.Birthday.HasValue) {
                if (customer.Birthday >= DateTime.Parse("1753/01/01") && customer.Birthday <= DateTime.Parse("9999/12/31")) {
                } else {
                    ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0030", "生年月日"));
                }
            }
            
            return customer;
        }

        /// <summary>
        /// 請求先の入力チェック
        /// </summary>
        /// <param name="customerClaim"></param>
        private void ValidateCustomerClaim(CustomerClaim customerClaim,EntitySet<CustomerClaimable> customerClaimable) {
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimName)) {
                ModelState.AddModelError("claim.CustomerClaimName", MessageUtils.GetMessage("E0001", "請求先名"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimType)) {
                ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0001", "請求種別"));
            }
            //クレジットまたはローンなら決済種別必須
            if(customerClaim.CustomerClaimType!=null && (customerClaim.CustomerClaimType.Equals("003") || customerClaim.CustomerClaimType.Equals("004"))){
                if (customerClaimable == null || customerClaimable.Count() == 0) {
                    ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0007", new string[] { "クレジットまたはローン", "決済種別" }));

                } else {
                    var targetClaimable =
                        from a in customerClaimable
                        where !string.IsNullOrEmpty(a.PaymentKindCode)
                        select a;

                    if (targetClaimable == null || targetClaimable.Count() == 0) {
                        ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0007", new string[] { "クレジットまたはローン", "決済種別" }));
                    }
                }
            }
            if (customerClaimable != null) {
                var query =
                    from a in customerClaimable
                    group a by a.PaymentKindCode into kind
                    select kind;
                var duplication = query.Where(x => x.Count() > 1);
                if (duplication == null || duplication.Count() > 0) {
                    ModelState.AddModelError("", "同一支払種別が複数登録されています");
                }
            }
            // 形式チェック
            // Add 2015/02/02 arc nakayama 顧客DM指摘事項④ 登録時の郵便番号はハイフンなしで登録する
            if (!string.IsNullOrEmpty(customerClaim.PostCode))
            {
                if ((!Regex.IsMatch(customerClaim.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customerClaim.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("claim.PostCode", MessageUtils.GetMessage("E0026", "郵便番号"));
                }
            }

        }

        /// <summary>
        /// 仕入先の入力チェック
        /// </summary>
        /// <param name="supplier">仕入先</param>
        private void ValidateSupplier(Supplier supplier) {
            if (string.IsNullOrEmpty(supplier.SupplierName)) {
                ModelState.AddModelError("sup.SupplierName", MessageUtils.GetMessage("E0001", "仕入先名"));
            }
            if (string.IsNullOrEmpty(supplier.OutsourceFlag)) {
                ModelState.AddModelError("sup.OutsourceFlag", MessageUtils.GetMessage("E0001", "外注フラグ"));
            }
            // 形式チェック
            // Add 2015/02/02 arc nakayama 顧客DM指摘事項④ 登録時の郵便番号はハイフンなしで登録する
            if (!string.IsNullOrEmpty(supplier.PostCode))
            {
                if ((!Regex.IsMatch(supplier.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(supplier.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("sup.PostCode", MessageUtils.GetMessage("E0026", "郵便番号"));
                }
            }
        }

        /// <summary>
        /// 支払先の入力チェック
        /// </summary>
        /// <param name="payment"></param>
        private void ValidateSupplierPayment(SupplierPayment supplierPayment,FormCollection form) {

            // 必須チェック(数値必須項目は属性チェックも兼ねる)
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentName)) {
                ModelState.AddModelError("pay.SupplierPaymentName", MessageUtils.GetMessage("E0001", "支払先名"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentType)) {
                ModelState.AddModelError("pay.SupplierPaymentType", MessageUtils.GetMessage("E0001", "支払先種別"));
            }
            if (string.IsNullOrEmpty(supplierPayment.PaymentType)) {
                ModelState.AddModelError("pay.PaymentType", MessageUtils.GetMessage("E0001", "支払区分"));
            }
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            if ((CommonUtils.DefaultString(supplierPayment.PaymentType).Equals("003")) && (supplierPayment.PaymentDayCount == null))
            {
                ModelState.AddModelError("pay.PaymentDayCount", MessageUtils.GetMessage("E0008", new string[] { "支払区分がn日後", "日数", "5～240" }));
            }

            // 属性チェック
            if (!ModelState.IsValidField("pay.PaymentDay")) {
                ModelState.AddModelError("pay.PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "支払日", "0～31" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod1")) {
                ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0004", new string[] { "猶予日数1", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod2")) {
                ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0004", new string[] { "猶予日数2", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod3")) {
                ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0004", new string[] { "猶予日数3", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod4")) {
                ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0004", new string[] { "猶予日数4", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod5")) {
                ModelState.AddModelError("pay.PaymentPeriod5", MessageUtils.GetMessage("E0004", new string[] { "猶予日数5", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod6")) {
                ModelState.AddModelError("pay.PaymentPeriod6", MessageUtils.GetMessage("E0004", new string[] { "猶予日数6", "0以外の正の整数" }));
            }

            // フォーマットチェック

            if (string.IsNullOrEmpty(form["pay.PaymentRate1"]) || (Regex.IsMatch(form["pay.PaymentRate1"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate1"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate1", MessageUtils.GetMessage("E0004", new string[] { "発生金利1", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate2"]) || (Regex.IsMatch(form["pay.PaymentRate2"], @"^\d{1,3}\.\d{1,3}$"))
                            || (Regex.IsMatch(form["pay.PaymentRate2"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate2", MessageUtils.GetMessage("E0004", new string[] { "発生金利2", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate3"]) || (Regex.IsMatch(form["pay.PaymentRate3"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate3"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate3", MessageUtils.GetMessage("E0004", new string[] { "発生金利3", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate4"]) || (Regex.IsMatch(form["pay.PaymentRate4"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate4"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate4", MessageUtils.GetMessage("E0004", new string[] { "発生金利4", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if ((string.IsNullOrEmpty(form["pay.PaymentRate5"]) || Regex.IsMatch(form["pay.PaymentRate5"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate5"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate5", MessageUtils.GetMessage("E0004", new string[] { "発生金利5", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate6"]) || (Regex.IsMatch(form["pay.PaymentRate6"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate6"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate6", MessageUtils.GetMessage("E0004", new string[] { "発生金利6", "正の整数3桁以内かつ小数3桁以内" }));
            }


            //猶予日数が入力されている場合、金利は入力必須
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod1"]) && string.IsNullOrEmpty(form["pay.PaymentRate1"])) {
                ModelState.AddModelError("pay.PaymentRate1", MessageUtils.GetMessage("E0001", new string[] { "発生金利1" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod2"]) && string.IsNullOrEmpty(form["pay.PaymentRate2"])) {
                ModelState.AddModelError("pay.PaymentRate2", MessageUtils.GetMessage("E0001", new string[] { "発生金利2" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod3"]) && string.IsNullOrEmpty(form["pay.PaymentRate3"])) {
                ModelState.AddModelError("pay.PaymentRate3", MessageUtils.GetMessage("E0001", new string[] { "発生金利3" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod4"]) && string.IsNullOrEmpty(form["pay.PaymentRate4"])) {
                ModelState.AddModelError("pay.PaymentRate4", MessageUtils.GetMessage("E0001", new string[] { "発生金利4" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod5"]) && string.IsNullOrEmpty(form["pay.PaymentRate5"])) {
                ModelState.AddModelError("pay.PaymentRate5", MessageUtils.GetMessage("E0001", new string[] { "発生金利5" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod6"]) && string.IsNullOrEmpty(form["pay.PaymentRate6"])) {
                ModelState.AddModelError("pay.PaymentRate6", MessageUtils.GetMessage("E0001", new string[] { "発生金利6" }));
            }


            // 値チェック
            if (ModelState.IsValidField("pay.PaymentDayCount")) {
                if (supplierPayment.PaymentDayCount < 5 || supplierPayment.PaymentDayCount > 240) {
                    ModelState.AddModelError("pay.PaymentDayCount", MessageUtils.GetMessage("E0004", new string[] { "日数", "5～240" }));
                }
            }
            if (ModelState.IsValidField("pay.PaymentDay")) {
                if (supplierPayment.PaymentDay < 0 || supplierPayment.PaymentDay > 31) {
                    ModelState.AddModelError("pay.PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "支払日", "0～31" }));
                }
            }

            //前提条件のチェック
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (supplierPayment.PaymentPeriod1 >= supplierPayment.PaymentPeriod2) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", "猶予日数2は猶予日数1より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (supplierPayment.PaymentPeriod2 >= supplierPayment.PaymentPeriod3) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", "猶予日数3は猶予日数2より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (supplierPayment.PaymentPeriod3 >= supplierPayment.PaymentPeriod4) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", "猶予日数4は猶予日数3より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod5"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "猶予日数4" }));
                    }
                }
                if (supplierPayment.PaymentPeriod4 >= supplierPayment.PaymentPeriod5) {
                    if (ModelState["pay.PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod5", "猶予日数5は猶予日数4より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod6"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "猶予日数4" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod5"])) {
                    if (ModelState["pay.PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod5", MessageUtils.GetMessage("E0001", new string[] { "猶予日数5" }));
                    }
                }
                if (supplierPayment.PaymentPeriod5 >= supplierPayment.PaymentPeriod6) {
                    if (ModelState["pay.PaymentPeriod6"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod6", "猶予日数6は猶予日数5より大きな数字である必要があります");
                    }
                }
            }
        }
        private void ValidateCustomerDM(CustomerDM customerDM) {
            // 必須チェック
            if (string.IsNullOrEmpty(customerDM.FirstName)) {
                ModelState.AddModelError("dm.FirstName", MessageUtils.GetMessage("E0001", "顧客名1(姓)"));
            }
            if (string.IsNullOrEmpty(customerDM.FirstNameKana)) {
                ModelState.AddModelError("dm.FirstNameKana", MessageUtils.GetMessage("E0001", "顧客名1(姓カナ)"));
            }
            // 形式チェック
            // Add 2015/01/09 arc nakayama 顧客DM指摘事項④ 登録時の郵便番号はハイフンなしで登録する
            if (!string.IsNullOrEmpty(customerDM.PostCode))
            {
                if ((!Regex.IsMatch(customerDM.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customerDM.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("dm.PostCode", MessageUtils.GetMessage("E0026", "郵便番号"));
                }
            }
        }
        #endregion


        /// <summary>
        /// 顧客マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="customer">顧客データ(登録内容)</param>
        /// <returns>顧客マスタモデルクラス</returns>
        private Customer EditCustomerForInsert(Customer customer) {

            customer.CustomerCode = new SerialNumberDao(db).GetNewCustomerCode(customer.DepartmentCode);
            customer.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.CreateDate = DateTime.Now;

            customer.DelFlag = "0";
            return EditCustomerForUpdate(customer);
        }

        /// <summary>
        /// 顧客マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="customer">顧客データ(登録内容)</param>
        /// <returns>顧客マスタモデルクラス</returns>
        private Customer EditCustomerForUpdate(Customer customer) {

            customer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.LastUpdateDate = DateTime.Now;

            //----20100513追加
            //Add 2017/02/08 arc nakayama #3627_顧客マスタ統合入力_入力文字の文字化け 入力値を退避
            string PrevFirstName = customer.FirstName;
            string PrevLastName = customer.LastName;
 
            customer.FirstName = Strings.StrConv(customer.FirstName.Trim(),VbStrConv.Wide,0);
            customer.LastName = Strings.StrConv(customer.LastName.Trim(),VbStrConv.Wide,0);

            //Add 2017/02/08 arc nakayama #3627_顧客マスタ統合入力_入力文字の文字化け 変換に失敗すると「？」になるため、「？」が含まれていたら入力値でDBに登録する
            if (customer.FirstName.Contains("？") || customer.FirstName.Contains("?"))
            {
                customer.FirstName = PrevFirstName;
            }
            if (customer.LastName.Contains("？") || customer.LastName.Contains("？"))
            {
                customer.LastName = PrevLastName;
            }

            customer.FirstNameKana = Strings.StrConv(customer.FirstNameKana.Trim(),VbStrConv.Wide,0);
            customer.LastNameKana = Strings.StrConv(customer.LastNameKana.Trim(),VbStrConv.Wide,0);
            customer.CustomerName = customer.FirstName + " " + customer.LastName;
            customer.CustomerNameKana = customer.FirstNameKana + " " + customer.LastNameKana;
            //----

            return customer;
        }

        /// <summary>
        /// 新規追加する顧客と同じ請求先を追加する
        /// （既に存在するコードだったらNULLを返す）
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <returns>請求先データ</returns>
        private CustomerClaim CreateCustomerClaim(Customer customer) {

            CustomerClaimDao dao = new CustomerClaimDao(db);
            CustomerClaim customerClaim = dao.GetByKey(customer.CustomerCode);
            if (customerClaim == null) {
                customerClaim = new CustomerClaim();
                customerClaim.CustomerClaimCode = customer.CustomerCode;
                customerClaim.CustomerClaimName = customer.CustomerName;
                customerClaim.CustomerClaimType = !string.IsNullOrEmpty(customer.CustomerType) && (customer.CustomerType.Equals("001") || customer.CustomerType.Equals("002")) ? customer.CustomerType : "001";
                customerClaim.CreateDate = DateTime.Now;
                customerClaim.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                customerClaim.LastUpdateDate = DateTime.Now;
                customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                customerClaim.DelFlag = "0";
            }
            return customerClaim;

        }

        #region 統合画面
        /// <summary>
        /// 顧客統合入力画面の表示
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public ActionResult IntegrateEntry(string id) {

            Customer customer;
            if (!string.IsNullOrEmpty(id)) {
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                customer = new CustomerDao(db).GetByKey(id, true);
                ViewData["update"] = "1";
            } else {
                customer = new Customer();
                ViewData["update"] = "0";
            }

            if (customer.CustomerClaim == null) {
                customer.CustomerClaim = new CustomerClaim() { CustomerClaimCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["claimUpdate"] = "0";
            } else {
                ViewData["claimUpdate"] = "1";
            }
            if (customer.Supplier == null) {
                customer.Supplier = new Supplier() { SupplierCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["supplierUpdate"] = "0";
            } else {
                ViewData["supplierUpdate"] = "1";
            }
            if (customer.SupplierPayment == null) {
                customer.SupplierPayment = new SupplierPayment() { SupplierPaymentCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["paymentUpdate"] = "0";
            } else {
                ViewData["paymentUpdate"] = "1";
            }
            if (customer.CustomerDM == null) {
                customer.CustomerDM = new CustomerDM() { CustomerCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["customerDMUpdate"] = "0";
            } else {
                ViewData["customerDMUpdate"] = "1";
            }
            if (customer.CustomerUpdateLog == null) {
                customer.CustomerUpdateLog = new EntitySet<CustomerUpdateLog>();
            }
            GetEntryViewData(customer);
            SetCustomerClaim(customer);
            ViewData["CustomerBasicDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// 統合画面での登録処理
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="claim">請求先データ</param>
        /// <param name="claimable">決済条件</param>
        /// <param name="sup">仕入先データ</param>
        /// <param name="pay">支払先データ</param>
        /// <param name="updateLog">担当者推移データ</param>
        /// <param name="form">フォーム入力値データ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/03 arc yano #3804 顧客統合入力　サービス担当部門を変更しても、担当者推移に反映されない
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IntegrateEntry(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {

            //顧客データのValidationチェック
            ValidateCustomer(customer, form);

            if (form["CustomerClaimEnabled"].Contains("true")) {
                ValidateCustomerClaim(claim,claimable);
            }
            if (form["SupplierEnabled"].Contains("true")) {
                ValidateSupplier(sup);
            }
            if (form["SupplierPaymentEnabled"].Contains("true")) {
                ValidateSupplierPayment(pay, form);
            }
            if (form["CustomerDMEnabled"].Contains("true")) {
                ValidateCustomerDM(dm);
            }
            if (!ModelState.IsValid) {
                customer.CustomerClaim = claim;
                customer.CustomerClaim.CustomerClaimable = claimable;
                customer.Supplier = sup;
                customer.SupplierPayment = pay;
                customer.CustomerUpdateLog = updateLog;
                customer.CustomerDM = dm;

                GetEntryViewData(customer);
                SetCustomerClaim(customer, form);
                ViewData["CustomerBasicDisplay"] = true;
                return View("CustomerIntegrateEntry", customer);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            Employee employee = (Employee)Session["Employee"];
            using (TransactionScope ts = new TransactionScope()) {


                //顧客の更新
                if (form["update"].Equals("1")) {

                    //請求先の更新
                    if (form["claimUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                        CustomerClaim targetClaim = new CustomerClaimDao(db).GetByKey(claim.CustomerClaimCode, true);
                        UpdateModel(targetClaim,"claim");
                        targetClaim.LastUpdateDate = DateTime.Now;
                        targetClaim.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/02/02 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        targetClaim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                        if (form["CustomerClaimEnabled"].Contains("true")) {
                            targetClaim.DelFlag = "0";
                        } else {
                            targetClaim.DelFlag = "1";
                        }
                    } else {
                        //新規作成
                        if (form["CustomerClaimEnabled"].Contains("true")) {
                            claim.CustomerClaimCode = customer.CustomerCode;
                            claim.CreateDate = DateTime.Now;
                            claim.CreateEmployeeCode = employee.EmployeeCode;
                            claim.LastUpdateDate = DateTime.Now;
                            claim.LastUpdateEmployeeCode = employee.EmployeeCode;
                            claim.DelFlag = "0";
                            claim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                            db.CustomerClaim.InsertOnSubmit(claim);
                        } else {
                            //作成しない
                            customer.CustomerClaim = null;
                        }
                    }

                    //仕入先の更新
                    if (form["supplierUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                        Supplier targetSupplier = new SupplierDao(db).GetByKey(sup.SupplierCode, true);
                        UpdateModel(targetSupplier,"sup");
                        targetSupplier.LastUpdateDate = DateTime.Now;
                        targetSupplier.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/02/02 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        targetSupplier.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                        if (form["SupplierEnabled"].Contains("true")) {
                            targetSupplier.DelFlag = "0";
                        } else {
                            targetSupplier.DelFlag = "1";
                        }

                    } else {
                        //新規作成
                        if (form["SupplierEnabled"].Contains("true")) {
                            sup.SupplierCode = customer.CustomerCode;
                            sup.CreateDate = DateTime.Now;
                            sup.CreateEmployeeCode = employee.EmployeeCode;
                            sup.LastUpdateDate = DateTime.Now;
                            sup.LastUpdateEmployeeCode = employee.EmployeeCode;
                            sup.DelFlag = "0";
                            //Add 2015/02/02 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                            sup.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                            db.Supplier.InsertOnSubmit(sup);
                        } else {
                            //作成しない
                            customer.Supplier = null;
                        }
                    }

                    //支払先の更新
                    if (form["paymentUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                        SupplierPayment targetPayment = new SupplierPaymentDao(db).GetByKey(pay.SupplierPaymentCode, true);
                        UpdateModel(targetPayment,"pay");
                        targetPayment.LastUpdateDate = DateTime.Now;
                        targetPayment.LastUpdateEmployeeCode = employee.EmployeeCode;
                        if (form["SupplierPaymentEnabled"].Contains("true")) {
                            targetPayment.DelFlag = "0";
                        } else {
                            targetPayment.DelFlag = "1";
                        }
                    } else {
                        //新規作成
                        if (form["SupplierPaymentEnabled"].Contains("true")) {
                            pay.SupplierPaymentCode = customer.CustomerCode;
                            pay.CreateDate = DateTime.Now;
                            pay.CreateEmployeeCode = employee.EmployeeCode;
                            pay.LastUpdateDate = DateTime.Now;
                            pay.LastUpdateEmployeeCode = employee.EmployeeCode;
                            pay.DelFlag = "0";
                            db.SupplierPayment.InsertOnSubmit(pay);
                        } else {
                            //作成しない
                            customer.SupplierPayment = null;
                        }
                    }

                    //DM発送先の更新
                    if (form["customerDMUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                        CustomerDM targetDM = new CustomerDMDao(db).GetByKey(customer.CustomerCode, true);
                        UpdateModel(targetDM, "dm");
                        targetDM.LastUpdateDate = DateTime.Now;
                        targetDM.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/01/29 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        targetDM.PostCode = CommonUtils.InsertHyphenInPostCode(dm.PostCode);
                        if (form["CustomerDMEnabled"].Contains("true")) {
                            targetDM.DelFlag = "0";
                        } else {
                            targetDM.DelFlag = "1";
                        }
                    } else {
                        if (form["CustomerDMEnabled"].Contains("true")) {
                            dm.CustomerCode = customer.CustomerCode;
                            dm.CreateDate = DateTime.Now;
                            dm.CreateEmployeeCode = employee.EmployeeCode;
                            dm.LastUpdateDate = DateTime.Now;
                            dm.LastUpdateEmployeeCode = employee.EmployeeCode;
                            dm.DelFlag = "0";
                            db.CustomerDM.InsertOnSubmit(dm);
                        }else{
                            customer.CustomerDM = null;
                        }
                    }
                    //決済条件の更新
                    //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                    CustomerClaim customerClaimableTarget = new CustomerClaimDao(db).GetByKey(claim.CustomerClaimCode, true);
                    //なくなっているものは削除する
                    if (customerClaimableTarget != null) {
                        foreach (var original in customerClaimableTarget.CustomerClaimable) {
                            IEnumerable<CustomerClaimable> query = null;
                            if (claimable != null) {
                                query =
                                    from a in claimable
                                    where a.PaymentKindCode.Equals(original.PaymentKindCode)
                                    select a;
                            }
                            if (claimable == null || query == null || query.Count() == 0) {
                                db.CustomerClaimable.DeleteOnSubmit(original);
                            }
                        }
                    }
                    if (claimable != null) {
                        foreach (var item in claimable) {
                            if (!string.IsNullOrEmpty(item.PaymentKindCode)) {
                                //ないものは追加する
                                CustomerClaimable target = new CustomerClaimableDao(db).GetByKey(claim.CustomerClaimCode, item.PaymentKindCode);
                                if (target == null) {
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = employee.EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = employee.EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CustomerClaimable.InsertOnSubmit(item);
                                }
                            }
                        }
                    }

                    //顧客データの更新
                    //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                    Customer targetCustomer = new CustomerDao(db).GetByKey(customer.CustomerCode, true);
                    string departmentFrom = targetCustomer.DepartmentCode;
                    string serviceDepartmentFrom = targetCustomer.ServiceDepartmentCode;        //Add 2017/11/03 arc yano #3804

                    string carEmployeeFrom = targetCustomer.CarEmployeeCode;
                    string serviceEmployeeFrom = targetCustomer.ServiceEmployeeCode;
                    UpdateModel(targetCustomer);
                    EditCustomerForUpdate(targetCustomer);
                    //Add　2015/01/29 arc nakayama 顧客DM指摘事項修正　登録または更新前に郵便番号にハイフンが入ってなければハイフンを入れる
                    targetCustomer.PostCode = CommonUtils.InsertHyphenInPostCode(customer.PostCode);

                    //Mod 2017/11/03 arc yano #3804
                    //担当者推移の作成(営業担当部門)
                    if (!CommonUtils.DefaultString(departmentFrom).Equals(CommonUtils.DefaultString(customer.DepartmentCode))) {
                        Department fromDepartment = new DepartmentDao(db).GetByKey(departmentFrom);
                        Department toDepartment = new DepartmentDao(db).GetByKey(customer.DepartmentCode);
                        CreateUpdateLog(
                            customer,
                            fromDepartment!=null ? fromDepartment.DepartmentName : "",
                            toDepartment!=null ? toDepartment.DepartmentName : "",
                            "営業担当部門"        
                            );
                    }

                    //担当者推移の作成(サービス担当部門)
                    if (!CommonUtils.DefaultString(serviceDepartmentFrom).Equals(CommonUtils.DefaultString(customer.ServiceDepartmentCode)))
                    {
                        Department fromDepartment = new DepartmentDao(db).GetByKey(serviceDepartmentFrom);
                        Department toDepartment = new DepartmentDao(db).GetByKey(customer.ServiceDepartmentCode);
                        CreateUpdateLog(
                            customer,
                            fromDepartment != null ? fromDepartment.DepartmentName : "",
                            toDepartment != null ? toDepartment.DepartmentName : "",
                            "サービス担当部門"
                            );
                    }

                    if (!CommonUtils.DefaultString(carEmployeeFrom).Equals(CommonUtils.DefaultString(customer.CarEmployeeCode))) {
                        Employee fromEmployee = new EmployeeDao(db).GetByKey(carEmployeeFrom);
                        Employee toEmployee = new EmployeeDao(db).GetByKey(customer.CarEmployeeCode);
                        CreateUpdateLog(
                            customer,
                            fromEmployee!=null ? fromEmployee.EmployeeName : "",
                            toEmployee!=null ? toEmployee.EmployeeName : "",
                            "営業担当"
                            );
                    }
                    if (!CommonUtils.DefaultString(serviceEmployeeFrom).Equals(CommonUtils.DefaultString(customer.ServiceEmployeeCode))) {
                        Employee fromEmployee = new EmployeeDao(db).GetByKey(serviceEmployeeFrom);
                        Employee toEmployee = new EmployeeDao(db).GetByKey(customer.ServiceEmployeeCode);
                        CreateUpdateLog(
                            customer, 
                            fromEmployee!=null ? fromEmployee.EmployeeName : "", 
                            toEmployee!=null ? toEmployee.EmployeeName : "",
                            "サービス担当"
                            );
                    }

                } else {
                    //顧客コードの採番
                    EditCustomerForInsert(customer);

                    //支払先
                    if (form["SupplierPaymentEnabled"].Contains("true")) {
                        pay.SupplierPaymentCode = customer.CustomerCode;
                        pay.CreateDate = DateTime.Now;
                        pay.CreateEmployeeCode = employee.EmployeeCode;
                        pay.LastUpdateDate = DateTime.Now;
                        pay.LastUpdateEmployeeCode = employee.EmployeeCode;
                        pay.DelFlag = "0";
                        customer.SupplierPayment = pay;
                    } else {
                        customer.SupplierPayment = null;
                    }

                    //仕入先
                    if (form["SupplierEnabled"].Contains("true")) {
                        sup.SupplierCode = customer.CustomerCode;
                        sup.CreateDate = DateTime.Now;
                        sup.CreateEmployeeCode = employee.EmployeeCode;
                        sup.LastUpdateDate = DateTime.Now;
                        sup.LastUpdateEmployeeCode = employee.EmployeeCode;
                        sup.DelFlag = "0";
                        //Add 2015/01/29 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        sup.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                        customer.Supplier = sup;
                    } else {
                        customer.Supplier = null;
                    }

                    //請求先
                    if (form["CustomerClaimEnabled"].Contains("true")) {
                        claim.CustomerClaimCode = customer.CustomerCode;
                        claim.CreateDate = DateTime.Now;
                        claim.CreateEmployeeCode = employee.EmployeeCode;
                        claim.LastUpdateDate = DateTime.Now;
                        claim.LastUpdateEmployeeCode = employee.EmployeeCode;
                        claim.DelFlag = "0";
                        //Add 2015/01/29 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        claim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                        customer.CustomerClaim = claim;
                        if (claimable != null) {
                            foreach (var item in claimable) {
                                // ADD 2014/05/21 arc uchida vs2012対応
                                if (item.PaymentKindCode != null)
                                {
                                    item.CustomerClaimCode = customer.CustomerCode;
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = employee.EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = employee.EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CustomerClaimable.InsertOnSubmit(item);
                                }
                            }
                        }
                    } else {
                        customer.CustomerClaim = null;
                    }

                    //DM発送先の更新
                    if (form["CustomerDMEnabled"].Contains("true")) {
                        dm.CustomerCode = customer.CustomerCode;
                        dm.CreateDate = DateTime.Now;
                        dm.CreateEmployeeCode = employee.EmployeeCode;
                        dm.LastUpdateDate = DateTime.Now;
                        dm.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/01/29 arc nakayama 顧客DM対応　ハイフンがなければ入れる
                        dm.PostCode = CommonUtils.InsertHyphenInPostCode(dm.PostCode);
                        dm.DelFlag = "0";
                        db.CustomerDM.InsertOnSubmit(dm);
                    } else {
                        customer.CustomerDM = null;
                    }

                    //Add　2015/01/29 arc nakayama 顧客DM指摘事項修正　登録または更新前に郵便番号にハイフンが入ってなければハイフンを入れる
                    customer.PostCode = CommonUtils.InsertHyphenInPostCode(customer.PostCode);
                    db.Customer.InsertOnSubmit(customer);
                }

                // Mod 2014/08/04 arc amii エラーログ対応 catch句を追加。『throw e』をログファイルにエラーを出力する処理に変更
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException ce)
                    {
                        // 更新時、クライアントの読み取り以降にDB値が更新された時、ローカルの値をDB値で上書きする
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        // リトライ回数を超えた場合、エラーとする
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0011", "保存"));
                            customer.CustomerClaim = claim;
                            customer.Supplier = sup;
                            customer.SupplierPayment = pay;
                            GetEntryViewData(customer);
                            SetCustomerClaim(customer, form);
                            ViewData["CustomerBasicDisplay"] = true;
                            return View("CustomerIntegrateEntry", customer);
                        }
                        else
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["close"] = "1";
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// 担当者推移を作成する
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="fromValue">変更前</param>
        /// <param name="toValue">変更後</param>
        /// <param name="updateColumn">更新対象項目名</param>
        private void CreateUpdateLog(Customer customer,string fromValue,string toValue,string updateColumn) {
            Employee employee = (Employee)Session["Employee"];

            CustomerUpdateLog log = new CustomerUpdateLog();
            log.CustomerUpdateLogId = Guid.NewGuid();
            log.UpdateDate = DateTime.Now;
            log.CustomerCode = customer.CustomerCode;
            log.UpdateColumn = updateColumn;
            log.UpdateEmployeeCode = employee.EmployeeCode;
            log.UpdateValueFrom = string.IsNullOrEmpty(fromValue) ? "" : fromValue;
            log.UpdateValueTo = string.IsNullOrEmpty(toValue) ? "" : toValue;
            db.CustomerUpdateLog.InsertOnSubmit(log);
        }

        #region 統合画面コンポーネント
        /// <summary>
        /// 統合画面コンポーネントのセット(updateフラグ付き)
        /// </summary>
        /// <param name="customer"></param>
        private void SetCustomerClaim(Customer customer,FormCollection form) {

            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];



            SetCustomerClaim(customer);
        }

        /// <summary>
        /// 統合画面コンポーネントのセット
        /// </summary>
        /// <param name="customer"></param>
        /// <history>
        /// 2019/01/18 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// </history>
        private void SetCustomerClaim(Customer customer){


            if (customer.CustomerClaim != null && customer.CustomerClaim.CustomerClaimable != null && customer.CustomerClaim.CustomerClaimable != null) {
                foreach (var claimable in customer.CustomerClaim.CustomerClaimable) {
                    claimable.PaymentKind = new PaymentKindDao(db).GetByKey(claimable.PaymentKindCode);
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customer.CustomerClaim.CustomerClaimType, true);
            ViewData["PaymentKindTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), customer.CustomerClaim.PaymentKindType, true);
            ViewData["RoundTypeList"] = CodeUtils.GetSelectListByModel(dao.GetRoundTypeAll(false), customer.CustomerClaim.RoundType, true);

            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, customer.Supplier.OutsourceFlag, true);
            ViewData["SupplierPaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSupplierPaymentTypeAll(false), customer.SupplierPayment.SupplierPaymentType, true);
            ViewData["PaymentType2List"] = CodeUtils.GetSelectListByModel(dao.GetPaymentType2All(false), customer.SupplierPayment.PaymentType, true);

            if (customer.SupplierPayment != null) {
                try { ViewData["BankName"] = new BankDao(db).GetByKey(customer.SupplierPayment.BankCode).BankName; } catch { }
                try { ViewData["BranchName"] = new BranchDao(db).GetByKey(customer.SupplierPayment.BranchCode, customer.SupplierPayment.BankCode).BranchName; } catch { }
                try { ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), customer.SupplierPayment.DepositKind, true); } catch { }

            }
            ViewData["CustomerBasicDisplay"] = false;
            ViewData["CustomerSalesDisplay"] = false;
            ViewData["CustomerClaimDisplay"] = false;
            ViewData["SupplierDisplay"] = false;
            ViewData["SupplierPaymentDisplay"] = false;
            ViewData["CustomerDMDisplay"] = false;

            //Add 20115/03/06 arc iijima 基本情報にDM発送先の登録有無の追加

            //Mod 2019/01/18 yano #3965
            if (customer.CustomerDM != null && (string.IsNullOrWhiteSpace(customer.CustomerDM.DelFlag) || customer.CustomerDM.DelFlag.Equals("0")))
            {
                ViewData["DMEnabledmessage"] = "DM発送先別途登録あり";
            }
            else{
                ViewData["DMEnabledmessage"] = "  ";
            }


            customer.BasicHasErrors = BasicHasErrors();
            customer.CustomerHasErrors = CustomerHasErrors();
            customer.CustomerClaimHasErrors = CustomerClaimHasErrors();
            customer.SupplierHasErrors = SupplierHasErrors();
            customer.SupplierPaymentHasErrors = SupplierPaymentHasErrors();
            customer.CustomerDMHasErrors = CustomerDMHasErrors();

        }
        #endregion

        #region 請求先・仕入先・支払先の有効／無効処理
        /// <summary>
        /// 請求先を有効にする
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="claim">請求先データ</param>
        /// <param name="claimable">決済条件データ</param>
        /// <param name="sup">仕入先データ</param>
        /// <param name="pay">支払先データ</param>
        /// <param name="updateLog">担当者推移データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        public ActionResult ClaimEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.CustomerDM = dm; 

            if (form["CustomerClaimEnabled"].Contains("true")) {
                customer.CustomerClaim.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.CustomerClaim.CustomerClaimName)) {
                    customer.CustomerClaim.CustomerClaimName = customer.FirstName + customer.LastName;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.PostCode)) {
                    customer.CustomerClaim.PostCode = customer.PostCode;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Prefecture)) {
                    customer.CustomerClaim.Prefecture = customer.Prefecture;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.City)) {
                    customer.CustomerClaim.City = customer.City;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Address1)) {
                    customer.CustomerClaim.Address1 = customer.Address1;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Address2)) {
                    customer.CustomerClaim.Address2 = customer.Address2;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.TelNumber1)) {
                    customer.CustomerClaim.TelNumber1 = customer.TelNumber;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.FaxNumber)) {
                    customer.CustomerClaim.FaxNumber = customer.FaxNumber;
                //}
            } else {
                customer.CustomerClaim.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["CustomerClaimDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// 仕入先を有効にする
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="claim"></param>
        /// <param name="claimable"></param>
        /// <param name="sup"></param>
        /// <param name="pay"></param>
        /// <param name="updateLog"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult SupplierEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["SupplierEnabled"].Contains("true")) {
                customer.Supplier.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.Supplier.SupplierName)) {
                    customer.Supplier.SupplierName = customer.FirstName + customer.LastName;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.PostCode)) {
                    customer.Supplier.PostCode = customer.PostCode;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Prefecture)) {
                    customer.Supplier.Prefecture = customer.Prefecture;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.City)) {
                    customer.Supplier.City = customer.City;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Address1)) {
                    customer.Supplier.Address1 = customer.Address1;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Address2)) {
                    customer.Supplier.Address2 = customer.Address2;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.TelNumber1)) {
                    customer.Supplier.TelNumber1 = customer.TelNumber;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.FaxNumber)) {
                    customer.Supplier.FaxNumber = customer.FaxNumber;
                //}
            } else {
                customer.Supplier.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["SupplierDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// 支払先を有効にする
        /// </summary>
        /// <param name="customer">顧客データ</param>
        /// <param name="claim">請求先データ</param>
        /// <param name="claimable">決済条件データ</param>
        /// <param name="sup">仕入先データ</param>
        /// <param name="pay">支払先データ</param>
        /// <param name="updateLog">担当者推移データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        public ActionResult SupplierPaymentEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["SupplierPaymentEnabled"].Contains("true")) {
                customer.SupplierPayment.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.SupplierPayment.SupplierPaymentName)) {
                    customer.SupplierPayment.SupplierPaymentName = customer.FirstName + customer.LastName;
                //}
            } else {
                customer.SupplierPayment.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["SupplierPaymentDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        public ActionResult DMEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["CustomerDMEnabled"].Contains("true")) {
                customer.CustomerDM.DelFlag = "0";
                if (string.IsNullOrEmpty(customer.CustomerDM.CorporationType)) {
                    customer.CustomerDM.CorporationType = customer.CorporationType;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FirstName)) {
                    customer.CustomerDM.FirstName = customer.FirstName;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FirstNameKana)) {
                    customer.CustomerDM.FirstNameKana = customer.FirstNameKana;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.LastName)) {
                    customer.CustomerDM.LastName = customer.LastName;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.LastNameKana)) {
                    customer.CustomerDM.LastNameKana = customer.LastNameKana;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.PostCode)) {
                    customer.CustomerDM.PostCode = customer.PostCode;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Prefecture)) {
                    customer.CustomerDM.Prefecture = customer.Prefecture;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.City)) {
                    customer.CustomerDM.City = customer.City;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Address1)) {
                    customer.CustomerDM.Address1 = customer.Address1;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Address2)) {
                    customer.CustomerDM.Address2 = customer.Address2;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.TelNumber)) {
                    customer.CustomerDM.TelNumber = customer.TelNumber;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FaxNumber)) {
                    customer.CustomerDM.FaxNumber = customer.FaxNumber;
                }
            } else {
                customer.CustomerDM.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer, form);
            ViewData["CustomerDMDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }
        #endregion

        #region 決済条件の行追加・行削除
        public ActionResult AddClaimable(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog,FormCollection form) {
             
            if (claimable == null) {
                claimable = new EntitySet<CustomerClaimable>();
            }
            CustomerClaimable newClaimable = new CustomerClaimable() { CustomerClaimCode = claim.CustomerClaimCode, DelFlag = "0" };
            claimable.Add(newClaimable);

            ModelState.Clear();

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;
            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["CustomerClaimDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }

        public ActionResult DelClaimable(int id, Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {

            claimable.RemoveAt(id);

            ModelState.Clear();

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;
            GetEntryViewData(customer);
            SetCustomerClaim(customer, form);
            ViewData["CustomerClaimDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }
        #endregion

        private bool BasicHasErrors() {
            if (
                ModelStateIsInvalid("CustomerCode") ||
                ModelStateIsInvalid("CustomerType") ||
                ModelStateIsInvalid("CorporationType") ||
                ModelStateIsInvalid("FirstName") ||
                ModelStateIsInvalid("LastName") ||
                ModelStateIsInvalid("FirstNameKana") ||
                ModelStateIsInvalid("LastNameKana") ||
                ModelStateIsInvalid("PostCode") ||
                ModelStateIsInvalid("Prefecture") ||
                ModelStateIsInvalid("City") ||
                ModelStateIsInvalid("Address1") ||
                ModelStateIsInvalid("Address2") ||
                ModelStateIsInvalid("TelNumber") ||
                ModelStateIsInvalid("FaxNumber") ||
                ModelStateIsInvalid("FirstReceiptionDate") ||
                ModelStateIsInvalid("LastReceiptionDate") ||
                ModelStateIsInvalid("Memo") ||
                ModelStateIsInvalid("DelFlag")) {
                return true;
            }
            return false;

        }
        private bool CustomerHasErrors() {
            if(
                ModelStateIsInvalid("CustomerRank") ||
                ModelStateIsInvalid("CustomerKind") ||
                ModelStateIsInvalid("PaymentKind") ||
                ModelStateIsInvalid("Sex") ||
                ModelStateIsInvalid("Birthday") ||
                ModelStateIsInvalid("Occupation") ||
                ModelStateIsInvalid("CarOwner") ||
                ModelStateIsInvalid("MailAddress") ||
                ModelStateIsInvalid("MobileNumber") ||
                ModelStateIsInvalid("MobileAddress") ||
                ModelStateIsInvalid("DmFlag") ||
                ModelStateIsInvalid("DmMemo") ||
                ModelStateIsInvalid("DepartmentCode") ||
                ModelStateIsInvalid("CarEmployeeCode") ||
                ModelStateIsInvalid("ServiceDepartmentCode") ||
                ModelStateIsInvalid("ServiceEmployeeCode") ||
                ModelStateIsInvalid("WorkingCompanyName") ||
                ModelStateIsInvalid("WorkingCompanyAddress") || 
                ModelStateIsInvalid("WorkingCompanyTelNumber") ||
                ModelStateIsInvalid("PositionName") ||
                ModelStateIsInvalid("CustomerEmployeeName") ||
                ModelStateIsInvalid("AccountEmployeeName"))
            {
                return true;
            }
            return false;
        }

        private bool CustomerClaimHasErrors() {
            var query = from a in ModelState
                        where (a.Key.StartsWith("claim.") && a.Value.Errors.Count() > 0)
                        || (a.Key.StartsWith("claimable[") && a.Value.Errors.Count() > 0)
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool SupplierHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("sup.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool SupplierPaymentHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("pay.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool CustomerDMHasErrors(){
            var query = from a in ModelState
                        where a.Key.StartsWith("dm.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool ModelStateIsInvalid(string keyName){
            return ModelState[keyName]!=null && ModelState[keyName].Errors.Count()>0;
        }
        #endregion
    }

}
