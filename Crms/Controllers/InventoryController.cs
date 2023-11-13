using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// 車両棚卸機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class InventoryController : Controller {

        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両棚卸";               // 画面名
        private static readonly string PROC_NAME_FIX = "締め";               // 処理名
        private static readonly string PROC_NAME_REC = "車両棚卸実地数登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InventoryController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 車両棚卸検索画面表示
        /// </summary>
        /// <returns>車両棚卸検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両棚卸検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件等)</param>
        /// <returns>車両棚卸検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            try { form["CompanyCode"] = (form["CompanyCode"] == null ? ((Employee)Session["Employee"]).Department1.Office.CompanyCode : form["CompanyCode"]); } catch (NullReferenceException) { }
            try { form["OfficeCode"] = (form["OfficeCode"] == null ? ((Employee)Session["Employee"]).Department1.OfficeCode : form["OfficeCode"]); } catch (NullReferenceException) { }
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            form["InventoryMonth"] = (form["InventoryMonth"] == null ? string.Format("{0:yyyy/MM}", DateTime.Now.AddMonths(-1)) : form["InventoryMonth"]);
            form["actionType"] = (form["actionType"] == null ? "" : form["actionType"]);

            // 締め処理
            if (form["actionType"].Equals("fixInventory")) {

                // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                using (TransactionScope ts = new TransactionScope()) {

                    string departmentCode = CommonUtils.DefaultString(form["fixDepartment"]);
                    DateTime inventoryMonth = DateTime.Parse(form["fixMonth"] + "/01 00:00:00.000");

                    // 棚卸スケジュールテーブルの取得
                    InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);

                    //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                    try
                    {
                        // 確定されていない場合
                        if (inventorySchedule != null && !inventorySchedule.InventoryStatus.Equals("003"))
                        {
                            // 差分チェック
                            V_CarInventorySummary v_CarInventorySummary = GetCarInventorySummaryForFix(departmentCode, inventoryMonth);
                            if (v_CarInventorySummary.DifferentialQuantity == 0)
                            {
                                // 棚卸実績テーブルへの追加と削除・棚卸スケジュールテーブルのステータス更新のストアドプロシージャ実行
                                db.FixInventory(departmentCode, inventoryMonth, "001", ((Employee)Session["Employee"]).EmployeeCode);
                            }
                        }
                        // トランザクションのコミット
                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_FIX, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            // 検索結果リストの取得
            PaginatedList<V_CarInventorySummary> list = GetSearchResultList(form);

            // その他出力項目の設定
            GetCriteriaViewData(form);

            // 車両棚卸検索画面の表示
            return View("InventoryCriteria", list);
        }

        /// <summary>
        /// 車両棚卸入力画面表示
        /// </summary>
        /// <param name="id">部門コード,棚卸月</param>
        /// <returns>車両棚卸入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            string[] idArr = CommonUtils.DefaultString(id).Split(new string[] { "," }, StringSplitOptions.None);
            string departmentCode = idArr[0];
            DateTime inventoryMonth = DateTime.Parse(idArr[1].Substring(0, 4) + "/" + idArr[1].Substring(4, 2) + "/01 00:00:00.000");

            // 入力対象リストの取得
            V_CarInventoryInProcess condition = new V_CarInventoryInProcess();
            condition.DepartmentCode = departmentCode;
            condition.InventoryMonth = inventoryMonth;
            condition.DefferenceSelect = true;
            List<V_CarInventoryInProcess> list = new V_CarInventoryInProcessDao(db).GetListByCondition(condition);

            // その他出力項目の設定
            GetEntryViewData(departmentCode, inventoryMonth);

            // 出口
            return View("InventoryEntry", list);
        }

        /// <summary>
        /// 車両棚卸実地数登録
        /// </summary>
        /// <param name="line">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両棚卸入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(List<V_CarInventoryInProcess> line, FormCollection form) {
           
            form["action"] = (form["action"] == null ? "" : form["action"]);
            string departmentCode = form["DepartmentCode"];
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");

            // 画面操作による処理制御
            switch (form["action"]) {

                // 明細行ソート
                case "sort":

                    // 明細行ソート
                    line = SortLine(line, form["sortKey"]);

                    // その他出力項目の設定
                    GetEntryViewData(departmentCode, inventoryMonth);

                    // 出口
                    ModelState.Clear();
                    return View("InventoryEntry", line);

                // 実地数登録
                default:

                    // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
                    db = new CrmsLinqDataContext();
                    db.Log = new OutputWriter();

                    using (TransactionScope ts = new TransactionScope()) {
                        // 棚卸スケジュールテーブルの取得
                        InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);

                        // 確定されていない場合
                        if (inventorySchedule != null && !inventorySchedule.InventoryStatus.Equals("003"))
                        {
                            // データチェック
                            ValidateInventory(line, form);
                            if (!ModelState.IsValid)
                            {
                                GetEntryViewData(departmentCode, inventoryMonth);
                                return View("InventoryEntry", line);
                            }

                            // データ登録処理
                            RegistInventory(line, form, inventorySchedule);
                        }
                        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                        {
                            try
                            {
                                // データ操作の実行・コミット
                                db.SubmitChanges();
                                // トランザクションのコミット
                                ts.Complete();
                                break;
                            }
                            catch (ChangeConflictException cfe)
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
                                    OutputLogger.NLogFatal(cfe, PROC_NAME_REC, FORM_NAME, "");
                                    // エラーページに遷移
                                    return View("Error");
                                }
                            }
                            catch (SqlException e)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                                {
                                    OutputLogger.NLogError(e, PROC_NAME_REC, FORM_NAME, "");

                                    ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                                    // その他出力項目の設定
                                    GetEntryViewData(departmentCode, inventoryMonth);
                                    return View("InventoryEntry", line);
                                }
                                else
                                {
                                    // ログに出力
                                    OutputLogger.NLogFatal(e, PROC_NAME_REC, FORM_NAME, "");
                                    return View("Error");
                                }
                            }
                            catch (Exception e)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                OutputLogger.NLogFatal(e, PROC_NAME_REC, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                    }
                    // その他出力項目の設定
                    GetEntryViewData(departmentCode, inventoryMonth);
                    // 出口
                    ModelState.Clear();
                    ViewData["close"] = "1";
                    return Entry(departmentCode + "," + string.Format("{0:yyyyMM}", inventoryMonth));
            }
        }

        /// <summary>
        /// 棚卸スケジュール情報取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸スケジュール情報</returns>
        private InventorySchedule GetInventorySchedule(string departmentCode, DateTime inventoryMonth) {

            InventorySchedule inventoryScheduleCondition = new InventorySchedule();
            inventoryScheduleCondition.SetAuthCondition((Employee)Session["Employee"]);
            inventoryScheduleCondition.Department = new Department();
            inventoryScheduleCondition.Department.DepartmentCode = departmentCode;
            inventoryScheduleCondition.InventoryMonth = inventoryMonth;
            inventoryScheduleCondition.InventoryType = "001";

            return new InventoryScheduleDao(db).GetByKey(inventoryScheduleCondition);
        }

        /// <summary>
        /// 確定対象車両棚卸サマリデータ取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>確定対象車両棚卸サマリデータ</returns>
        private V_CarInventorySummary GetCarInventorySummaryForFix(string departmentCode, DateTime inventoryMonth) {

            V_CarInventorySummaryDao v_CarInventorySummaryDao = new V_CarInventorySummaryDao(db);
            V_CarInventorySummary v_CarInventorySummaryCondition = new V_CarInventorySummary();
            v_CarInventorySummaryCondition.DepartmentCode = departmentCode;
            v_CarInventorySummaryCondition.InventoryMonth = inventoryMonth;

            return v_CarInventorySummaryDao.GetListByCondition(v_CarInventorySummaryCondition, 0, DaoConst.PAGE_SIZE)[0];
        }

        /// <summary>
        /// 車両棚卸検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両棚卸検索結果リスト</returns>
        private PaginatedList<V_CarInventorySummary> GetSearchResultList(FormCollection form) {

            V_CarInventorySummaryDao v_CarInventorySummaryDao = new V_CarInventorySummaryDao(db);
            V_CarInventorySummary v_CarInventorySummaryCondition = new V_CarInventorySummary();
            v_CarInventorySummaryCondition.SetAuthCondition((Employee)Session["Employee"]);
            v_CarInventorySummaryCondition.CompanyCode = form["CompanyCode"];
            v_CarInventorySummaryCondition.OfficeCode = form["OfficeCode"];
            v_CarInventorySummaryCondition.DepartmentCode = form["DepartmentCode"];
            v_CarInventorySummaryCondition.InventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");

            return v_CarInventorySummaryDao.GetListByCondition(v_CarInventorySummaryCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 検索画面表示データの取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void GetCriteriaViewData(FormCollection form) {

            // 検索条件部の画面表示データ取得
            ViewData["CompanyCode"] = form["CompanyCode"];
            if (!string.IsNullOrEmpty(form["CompanyCode"])) {
                Company company = new CompanyDao(db).GetByKey(form["CompanyCode"]);
                if (company != null) {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["InventoryMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetInventoryMonthsList(), form["InventoryMonth"], false);
        }

        /// <summary>
        /// 入力画面表示データの取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        private void GetEntryViewData(string departmentCode, DateTime inventoryMonth) {

            // 継続保持する情報の設定
            ViewData["DepartmentCode"] = departmentCode;
            ViewData["InventoryMonth"] = inventoryMonth;

            // 棚卸スケジュール関連情報の取得
            InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);
            if (inventorySchedule != null) {
                try { ViewData["CompanyName"] = inventorySchedule.Department.Office.Company.CompanyName; } catch (NullReferenceException) { }
                try { ViewData["OfficeName"] = inventorySchedule.Department.Office.OfficeName; } catch (NullReferenceException) { }
                try { ViewData["DepartmentName"] = inventorySchedule.Department.DepartmentName; } catch (NullReferenceException) { }
                ViewData["LastUpdateDate"] = inventorySchedule.LastUpdateDate;
            }
        }

        /// <summary>
        /// 実地数入力明細ソート
        /// </summary>
        /// <param name="line">実地数入力明細</param>
        /// <param name="sortKey">ソートキー</param>
        /// <returns>ソート済み実地数入力明細</returns>
        private List<V_CarInventoryInProcess> SortLine(List<V_CarInventoryInProcess> line, string sortKey) {

            line.Sort(delegate(V_CarInventoryInProcess x, V_CarInventoryInProcess y) {
                string xVal = CommonUtils.DefaultString(x.GetType().GetProperty(sortKey).GetValue(x, null));
                string yVal = CommonUtils.DefaultString(y.GetType().GetProperty(sortKey).GetValue(y, null));
                string xVin = CommonUtils.DefaultString(x.Vin);
                string yVin = CommonUtils.DefaultString(y.Vin);
                return (xVal.Equals(yVal) ? xVin.CompareTo(yVin) : xVal.CompareTo(yVal));
            });

            return line;
        }

        /// <summary>
        /// 車両棚卸入力チェック
        /// </summary>
        /// <param name="line">車両棚卸実地入力データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両棚卸データ</returns>
        private List<V_CarInventoryInProcess> ValidateInventory(List<V_CarInventoryInProcess> line, FormCollection form) {

            bool alreadyOutputMsgE0004 = false;
            for (int i = 0; i < line.Count; i++) {

                V_CarInventoryInProcess inventory = line[i];
                string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                // 属性チェック
                if (!ModelState.IsValidField(prefix + "Quantity")) {
                    ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0004", new string[] { "実地数", "0または1のみ" })));
                    alreadyOutputMsgE0004 = true;
                }

                // 値チェック
                if (ModelState.IsValidField(prefix + "Quantity") && inventory.Quantity != null) {
                    if ((inventory.Quantity == 0) || (inventory.Quantity == 1)) {
                    } else {
                        ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0004", new string[] { "実地数", "0または1のみ" })));
                        alreadyOutputMsgE0004 = true;
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// 車両棚卸明細登録処理
        /// </summary>
        /// <param name="line">車両棚卸実地入力データ</param>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventorySchedule">棚卸スケジュールデータ</param>
        private void RegistInventory(List<V_CarInventoryInProcess> line, FormCollection form, InventorySchedule inventorySchedule) {

            // 実地数明細の登録
            for (int i = 0; i < line.Count; i++) {

                V_CarInventoryInProcess inventory = line[i];

                // 棚卸テーブル追加
                if (string.IsNullOrEmpty(CommonUtils.DefaultString(inventory.InventoryId))) {

                    db.Inventory.InsertOnSubmit(EditInventoryForInsert(inventory, form));

                // 棚卸テーブル更新
                } else {

                    EditInventoryForUpdate(new InventoryDao(db).GetByKey(inventory.InventoryId ?? new Guid()), inventory);
                }
            }

            // 棚卸スケジュールテーブル更新
            EditInventoryScheduleForUpdate(inventorySchedule);
        }

        /// <summary>
        /// 車両棚卸追加データ編集
        /// </summary>
        /// <param name="input">車両棚卸実地入力データ</param>
        /// <param name="form">フォームデータ</param>
        /// <param name="cashBalance">車両棚卸データ(登録内容)</param>
        private Inventory EditInventoryForInsert(V_CarInventoryInProcess input, FormCollection form) {

            Inventory inventory = new Inventory();
            inventory.InventoryId = Guid.NewGuid();
            inventory.DepartmentCode = form["DepartmentCode"];
            inventory.InventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");
            inventory.LocationCode = input.LocationCode;
            inventory.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.InventoryType = "001";
            inventory.SalesCarNumber = input.SalesCarNumber;
            inventory.Quantity = input.Quantity;
            inventory.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.CreateDate = DateTime.Now;
            inventory.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.LastUpdateDate = DateTime.Now;
            inventory.DelFlag = "0";

            return inventory;
        }

        /// <summary>
        /// 車両棚卸更新データ編集
        /// </summary>
        /// <param name="oldData">更新前データ</param>
        /// <param name="newData">車両棚卸実地入力データ</param>
        /// <param name="cashBalance">車両棚卸データ(登録内容)</param>
        private Inventory EditInventoryForUpdate(Inventory oldData, V_CarInventoryInProcess newData) {

            oldData.Quantity = newData.Quantity;
            oldData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            oldData.LastUpdateDate = DateTime.Now;

            return oldData;
        }

        /// <summary>
        /// 車両棚卸スケジュール更新データ編集
        /// </summary>
        /// <param name="inventorySchedule">車両棚卸スケジュール更新対象データ</param>
        /// <param name="cashBalance">車両棚卸スケジュールデータ(登録内容)</param>
        private InventorySchedule EditInventoryScheduleForUpdate(InventorySchedule inventorySchedule) {

            inventorySchedule.InventoryStatus = "002";
            if (inventorySchedule.StartDate == null) {
                inventorySchedule.StartDate = DateTime.Now.Date;
            }
            inventorySchedule.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventorySchedule.LastUpdateDate = DateTime.Now;

            return inventorySchedule;
        }
    }
}
