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
using System.Text.RegularExpressions;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// 社員マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class EmployeeController : Controller {

        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "社員マスタ";     // 画面名
        private static readonly string PROC_NAME = "社員マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EmployeeController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// 社員検索画面表示
        /// </summary>
        /// <returns>社員検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 社員検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>社員検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Employee> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["EmployeeNumber"] = form["EmployeeNumber"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
            ViewData["SecurityRoleName"] = form["SecurityRoleName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 社員検索画面の表示
            return View("EmployeeCriteria", list);
        }

        /// <summary>
        /// 社員検索ダイアログ表示
        /// </summary>
        /// <returns>社員検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 社員検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>社員検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["EmployeeCode"] = Request["EmployeeCode"];
            form["EmployeeNumber"] = Request["EmployeeNumber"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["SecurityRoleCode"] = Request["SecurityRoleCode"];
            form["SecurityRoleName"] = Request["SecurityRoleName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Employee> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["EmployeeNumber"] = form["EmployeeNumber"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
            ViewData["SecurityRoleName"] = form["SecurityRoleName"];

            // 社員検索画面の表示
            return View("EmployeeCriteriaDialog", list);
        }

        /// <summary>
        /// 社員検索ダイアログ表示
        /// </summary>
        /// <returns>社員検索ダイアログ</returns>
        public ActionResult CriteriaDialog2()
        {
            criteriaInit = true;
            return CriteriaDialog2(new FormCollection());
        }

        /// <summary>
        /// 社員検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>社員検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            if (criteriaInit)
            {
                form["DelFlag"] = "0";
                ViewData["DelFlag"] = form["DelFlag"];
                // 検索結果リストの取得
                PaginatedList<Employee> list = GetSearchResultList(form);
                // 部門検索画面の表示
                return View("EmployeeCriteriaDialog2", list);
            }
            else
            {

                // 検索条件の設定
                // (クエリストリングを検索条件に使用する為、Requestを使用。
                //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
                form["EmployeeCode"] = Request["EmployeeCode"];
                form["EmployeeNumber"] = Request["EmployeeNumber"];
                form["EmployeeName"] = Request["EmployeeName"];
                form["DepartmentCode"] = Request["DepartmentCode"];
                form["DepartmentName"] = Request["DepartmentName"];
                form["SecurityRoleCode"] = Request["SecurityRoleCode"];
                form["SecurityRoleName"] = Request["SecurityRoleName"];
                form["DelFlag"] = Request["DelFlag"];

                // 検索結果リストの取得
                PaginatedList<Employee> list = GetSearchResultList(form);

                // その他出力項目の設定
                ViewData["EmployeeCode"] = form["EmployeeCode"];
                ViewData["EmployeeNumber"] = form["EmployeeNumber"];
                ViewData["EmployeeName"] = form["EmployeeName"];
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                ViewData["DepartmentName"] = form["DepartmentName"];
                ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
                ViewData["SecurityRoleName"] = form["SecurityRoleName"];
                ViewData["DelFlag"] = form["DelFlag"];

                // 社員検索画面の表示
                return View("EmployeeCriteriaDialog2", list);
            }
        }

        /// <summary>
        /// 社員マスタ入力画面表示
        /// </summary>
        /// <param name="id">社員コード(更新時のみ設定)</param>
        /// <returns>社員マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            Employee employee;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                employee = new Employee();
            }
                // 更新の場合
            else {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                employee = new EmployeeDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(employee);

            // 出口
            return View("EmployeeEntry", employee);
        }

        /// <summary>
        /// 社員マスタ追加更新
        /// </summary>
        /// <param name="area">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>社員マスタ入力画面</returns>
        /// <history>
        /// 2019/05/24 yano #3994 【社員マスタ】社員マスタ登録・更新時の履歴機能の追加
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Employee employee, FormCollection form) {

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateEmployee(employee);
            if (!ModelState.IsValid) {
                GetEntryViewData(employee);
                return View("EmployeeEntry", employee);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1")) {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                Employee targetEmployee = new EmployeeDao(db).GetByKey(employee.EmployeeCode, true);

                InsertEmployeeHisttory(targetEmployee);     //Mod 2019/05/24 yano #3994

                UpdateModel(targetEmployee);
                EditEmployeeForUpdate(targetEmployee);
                
            }
                // データ追加処理
            else {
                // データ編集
                employee = EditEmployeeForInsert(employee);

                // データ追加
                db.Employee.InsertOnSubmit(employee);                
            }

            // Add 2014/08/05 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力(更新と追加で入っていたSubmitChangesを統合)
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
                } catch (SqlException se) {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0010", new string[] { "社員コード", "保存" }));
                        GetEntryViewData(employee);
                        return View("EmployeeEntry", employee);
                    } else {
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
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(employee.EmployeeCode);
        }

        /// <summary>
        /// 社員コードから社員名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">社員コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code, bool includeDeleted = false)
        {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Employee employee = new EmployeeDao(db).GetByKey(code, includeDeleted);
                if (employee != null) {
                    codeData.Code = employee.EmployeeCode;
                    codeData.Name = employee.EmployeeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        public ActionResult GetMasterDetail(string code) {
            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> data = new Dictionary<string, string>();
                Employee employee;
                employee = new EmployeeDao(db).GetByEmployeeNumber(code);
                //社員番号でヒットしなかったら社員コードで検索し直し
                if (employee == null) {
                    employee = new EmployeeDao(db).GetByKey(code);
                }

                if (employee != null) {
                    data.Add("EmployeeCode", employee.EmployeeCode);
                    data.Add("EmployeeNumber", employee.EmployeeNumber);
                    data.Add("EmployeeName", employee.EmployeeName);
                    data.Add("ReceiptionEmployeeCode", employee.EmployeeCode);
                    data.Add("ReceiptionEmployeeNumber", employee.EmployeeNumber);
                    data.Add("ReceiptionEmployeeName", employee.EmployeeName);
                    data.Add("FrontEmployeeCode", employee.EmployeeCode);
                    data.Add("FrontEmployeeNumber", employee.EmployeeNumber);
                    data.Add("FrontEmployeeName", employee.EmployeeName);
                    data.Add("DepartureEmployeeCode", employee.EmployeeCode);
                    data.Add("DepartureEmployeeNumber", employee.EmployeeNumber);
                    data.Add("DepartureEmployeeName", employee.EmployeeName);
                    data.Add("ArrivalEmployeeCode", employee.EmployeeCode);
                    data.Add("ArrivalEmployeeNumber", employee.EmployeeNumber);
                    data.Add("ArrivalEmployeeName", employee.EmployeeName);
                }
                return Json(data);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="employee">モデルデータ</param>
        private void GetEntryViewData(Employee employee) {

            // 部門名の取得
            if (!string.IsNullOrEmpty(employee.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode1)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode1);
                if (department != null) {
                    ViewData["DepartmentName1"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode2)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode2);
                if (department != null) {
                    ViewData["DepartmentName2"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode3)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode3);
                if (department != null) {
                    ViewData["DepartmentName3"] = department.DepartmentName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["SecurityRoleList"] = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetListAll(), "SecurityRoleCode", "SecurityRoleName", employee.SecurityRoleCode, true);
            ViewData["EmployeeTypeList"] = CodeUtils.GetSelectListByModel(dao.GetEmployeeTypeAll(false), employee.EmployeeType, true);
        }

        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="employee">モデルデータ</param>
        private void SetDataComponent(Employee employee) {

            CodeDao dao = new CodeDao(db);

            // セキュリティロールセレクトリスト
            ViewData["SecurityRoleList"] = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetListAll(), "SecurityRoleCode", "SecurityRoleName", employee.SecurityRoleCode, true);

            // 雇用種別セレクトリスト
            ViewData["EmployeeTypeList"] = CodeUtils.GetSelectListByModel(dao.GetEmployeeTypeAll(false), employee.EmployeeType, true);
        }

        /// <summary>
        /// 社員マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>社員マスタ検索結果リスト</returns>
        private PaginatedList<Employee> GetSearchResultList(FormCollection form) {

            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employeeCondition = new Employee();
            employeeCondition.EmployeeCode = form["EmployeeCode"];
            employeeCondition.EmployeeNumber = form["EmployeeNumber"];
            employeeCondition.EmployeeName = form["EmployeeName"];
            employeeCondition.Department1 = new Department();
            employeeCondition.Department1.DepartmentCode = form["DepartmentCode"];
            employeeCondition.Department1.DepartmentName = form["DepartmentName"];
            employeeCondition.SecurityRole = new SecurityRole();
            employeeCondition.SecurityRole.SecurityRoleCode = form["SecurityRoleCode"];
            employeeCondition.SecurityRole.SecurityRoleName = form["SecurityRoleName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                employeeCondition.DelFlag = form["DelFlag"];
            }
            return employeeDao.GetListByCondition(employeeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="employee">社員データ</param>
        /// <returns>社員データ</returns>
        private Employee ValidateEmployee(Employee employee) {

            // 必須チェック
            if (string.IsNullOrEmpty(employee.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "社員コード"));
            }
            if (string.IsNullOrEmpty(employee.EmployeeName)) {
                ModelState.AddModelError("EmployeeName", MessageUtils.GetMessage("E0001", "氏名"));
            }
            if (string.IsNullOrEmpty(employee.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            if (string.IsNullOrEmpty(employee.SecurityRoleCode)) {
                ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0001", "セキュリティロール"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Birthday")) {
                ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0005", "生年月日"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("EmployeeCode")) {
                if (!Regex.IsMatch(CommonUtils.DefaultString(employee.EmployeeCode), @"^[0-9A-Za-z\.]*$")) {
                    ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0015", "社員コード"));
                }
            }

            return employee;
        }

        /// <summary>
        /// 社員マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="employee">社員データ(登録内容)</param>
        /// <returns>社員マスタモデルクラス</returns>
        private Employee EditEmployeeForInsert(Employee employee) {

            employee.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.CreateDate = DateTime.Now;
            employee.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.LastUpdateDate = DateTime.Now;
            employee.DelFlag = "0";
            return employee;
        }

        /// <summary>
        /// 社員マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="employee">社員データ(登録内容)</param>
        /// <returns>社員マスタモデルクラス</returns>
        private Employee EditEmployeeForUpdate(Employee employee) {

            employee.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.LastUpdateDate = DateTime.Now;
            return employee;
        }

        /// <summary>
        /// 社員マスタ更新履歴登録処理
        /// </summary>
        /// <param name="employee">更新前社員データ</param>
        /// <returns></returns>
        /// <history>
        /// 2019/05/24 yano #3994 【社員マスタ】社員マスタ登録・更新時の履歴機能の追加
        /// </history>
        private void InsertEmployeeHisttory(Employee employee)
        {
            EmployeeHistory history = new EmployeeHistory();

            //履歴テーブルの最新番号を取得
            int revNumber = (new EmployeeHistoryDao(db).GetLatestHistory(employee.EmployeeCode) != null ? (new EmployeeHistoryDao(db).GetLatestHistory(employee.EmployeeCode).RevisionNumber + 1) : 1);

            //履歴テーブルでデータ作成
            history.EmployeeCode = employee.EmployeeCode;
            history.RevisionNumber = revNumber;
            history.EmployeeNumber = employee.EmployeeNumber;
            history.EmployeeName = employee.EmployeeName;
            history.EmployeeNameKana = employee.EmployeeNameKana;
            history.DepartmentCode = employee.DepartmentCode;
            history.SecurityRoleCode = employee.SecurityRoleCode;
            history.MobileNumber = employee.MobileNumber;
            history.MobileMailAddress = employee.MobileMailAddress;
            history.MailAddress = employee.MailAddress;
            history.EmployeeType = employee.EmployeeType;
            history.Birthday = employee.Birthday;
            history.LastLoginDateTime = employee.LastLoginDateTime;
            history.CreateEmployeeCode = employee.CreateEmployeeCode;
            history.CreateDate = employee.CreateDate;
            history.LastUpdateEmployeeCode = employee.LastUpdateEmployeeCode;
            history.LastUpdateDate = employee.LastUpdateDate;
            history.DelFlag = employee.DelFlag;
            history.DepartmentCode1 = employee.DepartmentCode1;
            history.DepartmentCode2 = employee.DepartmentCode2;
            history.DepartmentCode3 = employee.DepartmentCode3;

            db.EmployeeHistory.InsertOnSubmit(history);
        }

    }
}
