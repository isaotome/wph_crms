using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PaymentController : Controller
    {
        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "支払管理 ";               // 画面名
        private static readonly string PROC_NAME = "支払消込明細登録";        // 処理名 

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PaymentController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 支払予定検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            return Criteria(new EntitySet<Journal>() ,new FormCollection());
        }

        /// <summary>
        /// 支払予定検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払予定検索結果表示</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(EntitySet<Journal> line, FormCollection form) {


            // デフォルト値の設定
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // 入金消込明細登録処理
            if (form["action"].Equals("regist") && line != null) {

                // データチェック
                ValidateJournal(line, form);

                // データ登録処理
                if (ModelState.IsValid) {

                    for (int i = 0; i < line.Count; i++) {

                        string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                        if ((!CommonUtils.DefaultString(form[prefix + "JournalDate"]).Equals(string.Format("{0:yyyy/MM/dd}", DateTime.Now.Date)))
                            || (!string.IsNullOrEmpty(form[prefix + "AccountType"]))
                            || (!CommonUtils.DefaultString(form[prefix + "Amount"]).Equals("0"))) {

                            Journal journal = line[i];

                            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
                            db = new CrmsLinqDataContext();
                            db.Log = new OutputWriter();

                            using (TransactionScope ts = new TransactionScope()) {

                                PaymentPlan targetPaymentPlan = new PaymentPlanDao(db).GetByKey(new Guid(form[prefix + "PaymentPlanId"]));

                                // 出納帳登録
                                InsertJournal(targetPaymentPlan, journal);

                                // 支払消込
                                UpdateReceiptPlan(targetPaymentPlan, journal);

                                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                                for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                                {
                                    try
                                    {
                                        db.SubmitChanges();
                                        ts.Complete();
                                        break;
                                    }
                                    catch (ChangeConflictException cfe)
                                    {
                                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                                        {
                                            occ.Resolve(RefreshMode.KeepCurrentValues);
                                        }
                                        if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                                        {
                                            // セッションにSQL文を登録
                                            Session["ExecSQL"] = OutputLogData.sqlText;
                                            // ログに出力
                                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
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
                                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
                                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "支払消込登録"));
                                        }
                                        else
                                        {
                                            // ログに出力
                                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
                                            // エラーページに遷移
                                            return View("Error");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        // セッションにSQL文を登録
                                        Session["ExecSQL"] = OutputLogData.sqlText;
                                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
                                        return View("Error");
                                    }
                                }
                            }
                        }
                    }
                    ModelState.Clear();
                }
            } else {
                ModelState.Clear();
            }
            PaginatedList<PaymentPlan> list = GetSearchResult(form);
            return View("PaymentCriteria",SetDataComponent(list,form));
        }

        /// <summary>
        /// 支払消込入力チェック
        /// </summary>
        /// <param name="journal">支払消込データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払消込データ</returns>
        private EntitySet<Journal> ValidateJournal(EntitySet<Journal> line, FormCollection form) {

            bool alreadyOutputMsgE0001 = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0003 = false;
            bool alreadyOutputMsgE0013a = false;
            for (int i = 0; i < line.Count; i++) {

                Journal journal = line[i];
                string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                if ((!CommonUtils.DefaultString(form[prefix + "JournalDate"]).Equals(string.Format("{0:yyyy/MM/dd}", DateTime.Now.Date)))
                    || (!string.IsNullOrEmpty(form[prefix + "AccountType"]))
                    || (!CommonUtils.DefaultString(form[prefix + "Amount"]).Equals("0"))) {

                    // 必須チェック(数値/日付項目は属性チェックを兼ねる)
                    if (!ModelState.IsValidField(prefix + "JournalDate")) {
                        ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "伝票日付")));
                        alreadyOutputMsgE0003 = true;
                        if (ModelState[prefix + "JournalDate"].Errors.Count > 1) {
                            ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                        }
                    }
                    if (string.IsNullOrEmpty(journal.AccountType)) {
                        ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "入金種別")));
                        alreadyOutputMsgE0001 = true;
                    }
                    if (!ModelState.IsValidField(prefix + "Amount")) {
                        ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                        alreadyOutputMsgE0002 = true;
                        if (ModelState[prefix + "Amount"].Errors.Count > 1) {
                            ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                        }
                    }

                    // フォーマットチェック
                    if (ModelState.IsValidField(prefix + "Amount")) {
                        if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$")) {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }

                    // 値チェック
                    if (ModelState.IsValidField(prefix + "JournalDate")) {
                        if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0) {
                            ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0013a ? "" : MessageUtils.GetMessage("E0013", new string[] { "伝票日付", "本日以前" })));
                            alreadyOutputMsgE0013a = true;
                        }
                    }
                    if (ModelState.IsValidField(prefix + "Amount")) {
                        if (journal.Amount.Equals(0m)) {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// 残高合計取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払残高合計金額</returns>
        private decimal GetTotalBalance(FormCollection form) {
            return new PaymentPlanDao(db).GetTotalBalance(GetSearchCondition(form));
        }

        /// <summary>
        /// データ付き画面コンポーネント
        /// </summary>
        /// <param name="list">支払予定データリスト</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払予定データリスト</returns>
        private List<PaymentPlan> SetDataComponent(List<PaymentPlan> list,FormCollection form) {
            ViewData["CondSupplierPaymentCode"] = form["CondSupplierPaymentCode"];
            string supplierPaymentName = "";
            if (!string.IsNullOrEmpty(form["CondSupplierPaymentCode"])) {
                SupplierPayment supplierPayment = new SupplierPaymentDao(db).GetByKey(form["CondSupplierPaymentCode"]);
                if (supplierPayment != null) {
                    supplierPaymentName = supplierPayment.SupplierPaymentName;
                }
            }
            ViewData["CondSupplierPaymentName"] = supplierPaymentName;
            ViewData["PaymentPlanDateFrom"] = form["PaymentPlanDateFrom"];
            ViewData["PaymentPlanDateTo"] = form["PaymentPlanDateTo"];

            List<IEnumerable<SelectListItem>> accountTypeList = new List<IEnumerable<SelectListItem>>();
            CodeDao dao = new CodeDao(db);
            List<c_AccountType> accountTypeListSrc = dao.GetAccountTypeAll(false);
            List<Journal> journalList = new List<Journal>();
            for (int i = 0; i < list.Count; i++) {
                Journal journal = new Journal();
                journal.JournalDate = DateTime.Now.Date;
                journalList.Add(journal);
                accountTypeList.Add(CodeUtils.GetSelectListByModel<c_AccountType>(accountTypeListSrc, "", true));
            }
            ViewData["AccountTypeList"] = accountTypeList;
            ViewData["JournalList"] = journalList;
            ViewData["CondSupplierPaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSupplierPaymentTypeAll(false), form["CondSupplierPaymentType"], true);
            ViewData["TotalBalance"] = GetTotalBalance(form);
            return list;
        }

        /// <summary>
        /// 支払予定検索結果を取得する
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払予定検索結果データリスト</returns>
        private PaginatedList<PaymentPlan> GetSearchResult(FormCollection form) {
            PaginatedList<PaymentPlan> list = new PaymentPlanDao(db).GetListByCondition(GetSearchCondition(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return list;
        }

        /// <summary>
        /// 検索条件をセットする
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払予定検索条件データ</returns>
        private PaymentPlan GetSearchCondition(FormCollection form) {
            PaymentPlan condition = new PaymentPlan();
            condition.SupplierPayment = new SupplierPayment();
            condition.SupplierPayment.SupplierPaymentCode = form["CondSupplierPaymentCode"];
            condition.SupplierPayment.SupplierPaymentType = form["CondSupplierPaymentType"];
            condition.PaymentPlanDateFrom = CommonUtils.StrToDateTime(form["PaymentPlanDateFrom"], DaoConst.SQL_DATETIME_MIN);
            condition.PaymentPlanDateTo = CommonUtils.StrToDateTime(form["PaymentPlanDateTo"], DaoConst.SQL_DATETIME_MAX);
            condition.SetAuthCondition(((Employee)Session["Employee"]));
            return condition;
        }
        /// <summary>
        /// 支払消込明細登録処理
        /// </summary>
        /// <param name="targetReceiptPlan">消込対象支払予定モデルデータ</param>
        /// <param name="journal">出納帳モデルデータ</param>
        private void InsertJournal(PaymentPlan targetPaymentPlan, Journal journal) {

            // JournalDate,AccountType,Amountはフレームワークにて編集済み
            journal.JournalId = Guid.NewGuid();
            journal.JournalType = "002";
            journal.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            journal.CustomerClaimCode = targetPaymentPlan.SupplierPaymentCode;
            journal.SlipNumber = targetPaymentPlan.SlipNumber;
            journal.AccountCode = targetPaymentPlan.AccountCode;
            journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.CreateDate = DateTime.Now;
            journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.LastUpdateDate = DateTime.Now;
            journal.DelFlag = "0";
            db.Journal.InsertOnSubmit(journal);
        }

        /// <summary>
        /// 支払予定テーブル更新処理
        /// </summary>
        /// <param name="targetReceiptPlan">消込対象支払予定モデルデータ</param>
        /// <param name="journal">出納帳モデルデータ</param>
        private void UpdateReceiptPlan(PaymentPlan targetPaymentPlan,Journal journal) {

            targetPaymentPlan.PaymentableBalance = decimal.Subtract(targetPaymentPlan.PaymentableBalance ?? 0m, journal.Amount);
            if ((targetPaymentPlan.PaymentableBalance ?? 0m).Equals(0m)) {
                targetPaymentPlan.CompleteFlag = "1";
            }
            targetPaymentPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            targetPaymentPlan.LastUpdateDate = DateTime.Now;
        }
    }
}
