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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// 現金出納帳機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CashJournalController : Controller {

        #region 初期化
        //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "現金出納帳";   // 画面名
        private static readonly string PROC_NAME = "現金在高登録"; // 処理名
        private static readonly string PROC_NAME_UPD_JSK = "入金実績修正"; // 処理名
        private static readonly string PROC_NAME_SHIME = "締め"; // 処理名
        private static readonly string PROC_NAME_SUITO = "現金出納帳明細登録"; // 処理名

        //Add 2018/11/14 yano #3814
        private static readonly string STATUS_CAR_CANCEL = "006";          //キャンセル(車両伝票ステータス)
        private static readonly string STATUS_CAR_ORDERCANCEL = "007";     //受注後キャンセル(車両伝票ステータス)
        private static readonly string STATUS_SERVICE_CANCEL = "007";      //キャンセル(サービス伝票ステータス)
        private static readonly string JOURNALTYPE_PAYMENT = "001";        //入金(入金種別)


        //Add 2018/08/22 yano #3930
        private static readonly List<string> excludeList = new List<string>() { "003" }; //請求先タイプ = クレジット
        private static readonly List<string> excluedAccountTypetList = new List<string>() { "012", "013" }; //口座タイプ = 残債, 下取

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CashJournalController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion


        #region 検索画面表示
        /// <summary>
        /// 現金出納帳検索画面表示
        /// </summary>
        /// <returns>現金出納帳検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            Journal journal = new Journal();
            journal.JournalDate = DateTime.Now.Date;
            return Criteria(journal, new FormCollection());
        }

        /// <summary>
        /// 現金出納帳検索画面表示
        /// </summary>
        /// <param name="journal">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>現金出納帳検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(Journal journal, FormCollection form) {

            // デフォルト値の設定
            try { form["OfficeCode"] = (string.IsNullOrEmpty(form["OfficeCode"]) ? ((Employee)Session["Employee"]).Department1.OfficeCode : form["OfficeCode"]); } catch (NullReferenceException) { }
            //form["CondDepartmentCode"] = (form["CondDepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["CondDepartmentCode"]);
            form["CondYearMonth"] = (form["CondYearMonth"] == null ? string.Format("{0:yyyy/MM}", DateTime.Now) : form["CondYearMonth"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);
            form["CashAccountCode"] = form["CashAccountCode"] == null ? "001" : form["CashAccountCode"];
            form["AccountType"] = "001";
            form["CondDay"] = (form["CondDay"] == null ? DateTime.Now.Day.ToString() : form["CondDay"]);
            ViewData["DefaultCondYearMonth"] = string.Format("{0:yyyy/MM}", DateTime.Now);
            ViewData["DefaultCondDay"] = DateTime.Now.Day.ToString();

            List<Journal> list;

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            #region 締め処理
            if (form["action"].Equals("closeAccount")) {
                //締め対象日
                DateTime targetDate = DateTime.Parse(form["selectedDate"] ?? DateTime.Today.ToString());

                //前月末締めがまだの場合、エラー
                if (targetDate > DaoConst.FIRST_TARGET_DATE) {

                    CashBalance lastMonthBalance = GetTodayBalanceData(form["OfficeCode"],form["CashAccountCode"], new DateTime(targetDate.Year,targetDate.Month,1).AddDays(-1));
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加

                    if (lastMonthBalance == null || !CommonUtils.DefaultString(lastMonthBalance.CloseFlag).Equals("1"))
                    {
                        ModelState.AddModelError("", "前月末締め処理が完了していないため指定日では締め処理できません");

                        // 検索結果リストの取得
                        list = GetSearchResultList(form);

                        // その他出力項目の設定
                        GetCriteriaViewData(journal, form);

                        // 現金出納帳検索画面の表示
                        return View("CashJournalCriteria", list);
                    }
                }

                using (TransactionScope ts = new TransactionScope()) {                 

                    // 本日締め情報(=入力情報)の取得
                    CashBalance cashBalance = GetTodayBalanceData(form["OfficeCode"],form["CashAccountCode"],targetDate);

                    // 締められていない場合
                    if (cashBalance != null && CommonUtils.DefaultString(cashBalance.CloseFlag).Equals("0")) {

                        // 直近の締め情報取得
                        CashBalance cashBalancePrev = GetLatestClosedData(form["OfficeCode"],form["CashAccountCode"]);
                        decimal totalAmountPrev = (cashBalancePrev == null ? 0m : cashBalancePrev.TotalAmount ?? 0m);

                        // 今期明細金額合計の取得
                        decimal detailsTotal = GetDetailsTotal(
                            form["OfficeCode"],
                            form["CashAccountCode"],
                            (cashBalancePrev == null ? DaoConst.SQL_DATETIME_MIN : cashBalancePrev.ClosedDate.AddDays(1)),
                            targetDate);

                        // 現金在高の照合
                        if (decimal.Add(totalAmountPrev, detailsTotal).Equals(cashBalance.TotalAmount)) {

                            // 締め処理
                            CloseAccount(cashBalance);
                        }
                    }

                    //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                    {
                        try
                        {
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
                            if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogFatal(cfe, PROC_NAME_SHIME, FORM_NAME, "");
                                // エラーページに遷移
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_SHIME, FORM_NAME, "");
                            return View("Error");
                        }
                    } 
                }
            #endregion

            #region 現金出納帳明細登録処理
            }
            else if (form["action"].Equals("registDetail"))
            {

                // データチェック
                ValidateJournal(journal);
                //journal.CashAccountCode = form["CashAccountCode"];
                //journal.OfficeCode = form["OfficeCode"];

                // データ登録処理
                if (ModelState.IsValid)
                {
                    //現金出納帳明細登録処理
                    InsertJournal(journal);
                    //Add 2014/08/12 arc amii エラーログ対応 エラーログ出力処理追加
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogError(e, PROC_NAME_SUITO, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "登録"));
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_SUITO, FORM_NAME, "");
                            return View("Error");
                        }
                    }

                    form["CondDay"] = journal.JournalDate.Day.ToString();
                    form["CondYearMonth"] = string.Format("{0:yyyy/MM}", journal.JournalDate);
                }

                // モデルの初期化
                if (ModelState.IsValid)
                {
                    ModelState.Clear();
                    journal = new Journal();
                    journal.JournalDate = DateTime.Now.Date;
                }
            #endregion

            #region 店舗締め処理解除
            }
            else if (form["action"].Equals("ReleaseAccount"))
            {
                //Add 2016/05/18 arc nakayama #3536_現金出納帳　店舗締め解除処理ボタンの追加 店舗締め解除処理追加
                //締め対象日
                DateTime ReleasetargetDate = DateTime.Parse(form["PreviousClosedDate"]);

                using (TransactionScope ts = new TransactionScope())
                {
                    // 直近の店舗締め処理データ取得
                    CashBalance PrevcashBalance = GetTodayBalanceData(form["OfficeCode"], form["CashAccountCode"], ReleasetargetDate);

                    //店舗締め解除処理
                    ReleaseAccount(PrevcashBalance);

                    //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                    {
                        try
                        {
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
                            if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogFatal(cfe, PROC_NAME_SHIME, FORM_NAME, "");
                                // エラーページに遷移
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_SHIME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                }
            }
            #endregion

            // 検索結果リストの取得
            list = GetSearchResultList(form);

            // その他出力項目の設定
            GetCriteriaViewData(journal, form);

            if (ModelState.IsValid) {
                ModelState.Clear();
            }

            // 現金出納帳検索画面の表示
            return View("CashJournalCriteria", list);
        }

        /// <summary>
        /// 現金出納帳明細登録処理
        /// </summary>
        /// <param name="journal">出納帳モデルデータ</param>
        private void InsertJournal(Journal journal) {

            journal.JournalId = Guid.NewGuid();
            journal.AccountType = "001";
            journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.CreateDate = DateTime.Now;
            journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.LastUpdateDate = DateTime.Now;
            journal.DelFlag = "0";
            journal.TransferFlag = "0";
            db.Journal.InsertOnSubmit(journal);
            
        }

        #region 画面データ取得
        /// <summary>
        /// 検索画面表示データの取得
        /// </summary>
        /// <param name="journal">モデルデータ</param>
        /// <param name="form">フォームデータ</param>
        private void GetCriteriaViewData(Journal journal, FormCollection form) {

            CodeDao dao = new CodeDao(db);
            DateTime selectedDate = DateTime.Parse(form["selectedDate"] != null && !form["selectedDate"].Equals("") ? form["selectedDate"] : DateTime.Today.ToString());

            // 検索条件部の画面表示データ取得
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            ViewData["CondDepartmentCode"] = form["CondDepartmentCode"];
            if (!string.IsNullOrEmpty(form["CondDepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["CondDepartmentCode"]);
                if (department != null) {
                    ViewData["CondDepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["CondJournalTypeList"] = CodeUtils.GetSelectListByModel(dao.GetJournalTypeAll(false), form["CondJournalType"], true);
            ViewData["CondYearMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetYearMonthsList(), form["CondYearMonth"], true);
            List<CodeData> dayList = new List<CodeData>();
            for(int i=1;i<=31;i++){
                dayList.Add(new CodeData() { Code = i.ToString(), Name = i.ToString() });
            }
            ViewData["CondDay"] = form["CondDay"];
            ViewData["CondDayList"] = CodeUtils.GetSelectList(dayList, form["CondDay"], true);
            ViewData["CondSlipNumber"] = form["CondSlipNumber"];
            ViewData["CondAccountCode"] = form["CondAccountCode"];
            if (!string.IsNullOrEmpty(form["CondAccountCode"])) {
                Account account = new AccountDao(db).GetByKey(form["CondAccountCode"]);
                if (account != null) {
                    ViewData["CondAccountName"] = account.AccountName;
                }
            }
            ViewData["CondSummary"] = form["CondSummary"];

            // 締め処理部の画面表示データ及び処理制御情報取得
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                ViewData["UpdateAuth"] = (CheckOfficeAuth(form["OfficeCode"]) ? "1" : "0");
                if (ViewData["UpdateAuth"].Equals("1")) {
                    ViewData["LastMonth"] = selectedDate.AddMonths(-1).Month;
                    CashBalance cashBalanceLastMonth = new CashBalanceDao(db).GetLastMonthClosedData(form["OfficeCode"], form["CashAccountCode"], selectedDate);
                    if (cashBalanceLastMonth != null) {
                        ViewData["LastMonthBalance"] = cashBalanceLastMonth.TotalAmount;
                    }

                    DateTime targetDate;
                    CashBalance cashBalancePrev = GetLatestClosedData(form["OfficeCode"], form["CashAccountCode"]);
                    if (cashBalancePrev == null) {
                        ViewData["PreviousBalance"] = 0m;
                        targetDate = new DateTime(2010, 6, 30);
                    } else {
                        targetDate = cashBalancePrev.ClosedDate;
                        ViewData["PreviousClosedDate"] = cashBalancePrev.ClosedDate;
                        ViewData["PreviousBalance"] = cashBalancePrev.TotalAmount;
                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        if (CommonUtils.DefaultString(cashBalancePrev.CloseFlag).Equals("1") && cashBalancePrev.ClosedDate.Equals(selectedDate))
                        {
                            ViewData["AlreadyClosed"] = "1";
                        }
                    }

                    //日付移動リスト作成                                       
                    List<CodeData> dateList = new List<CodeData>();
                    while (targetDate < DateTime.Today) {
                        CodeData data = new CodeData();
                        data.Code = string.Format("{0:yyyy/MM/dd}", targetDate);
                        data.Name = string.Format("{0:yyyy/MM/dd}", targetDate);
                        dateList.Add(data);
                        targetDate = targetDate.AddDays(1);
                    }
                    dateList.Add(new CodeData() { Code = string.Format("{0:yyyy/MM/dd}", DateTime.Today), Name = string.Format("{0:yyyy/MM/dd}", DateTime.Today) });
                    ViewData["MovableDateList"] = CodeUtils.GetSelectList(dateList, string.Format("{0:yyyy/MM/dd}", selectedDate), false);

                    List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
                    List<CodeData> accountDataList = new List<CodeData>();
                    foreach (var a in accountList) {
                        CodeData data = new CodeData();
                        data.Code = a.CashAccountCode;
                        data.Name = a.CashAccountName;
                        accountDataList.Add(data);
                    }
                    ViewData["CashBalanceAccountList"] = CodeUtils.GetSelectList(accountDataList, form["CashAccountCode"], false);

                    if (!CommonUtils.DefaultString(ViewData["AlreadyClosed"]).Equals("1")) {
                        ViewData["DetailsTotal"] = GetDetailsTotal(
                            form["OfficeCode"],
                            form["CashAccountCode"],
                            (cashBalancePrev == null ? DaoConst.SQL_DATETIME_MIN : cashBalancePrev.ClosedDate.AddDays(1)),
                            selectedDate);
                        ViewData["LogicalBalance"] = decimal.Add((ViewData["PreviousBalance"] == null ? 0m : (decimal)ViewData["PreviousBalance"]), (decimal)ViewData["DetailsTotal"]);
                        CashBalance cashBalance = GetTodayBalanceData(form["OfficeCode"], form["CashAccountCode"], selectedDate);
                        if (cashBalance != null) {
                            ViewData["InputBalance"] = cashBalance.TotalAmount;
                        }
                    }
                }
            }

            // 出納帳明細登録部の画面表示データ取得
            //ModelState.Remove("DepartmentCode");
            ViewData["DepartmentCode"] = string.IsNullOrEmpty(journal.DepartmentCode) ? ((Employee)Session["Employee"]).DepartmentCode : journal.DepartmentCode;
            Department dep = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"].ToString());
            ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : "";

            ViewData["JournalDate"] = journal.JournalDate;
            ViewData["AccountCode"] = journal.AccountCode;
            if (!string.IsNullOrEmpty(journal.AccountCode)) {
                Account account = new AccountDao(db).GetByKey(journal.AccountCode);
                if (account != null) {
                    ViewData["AccountName"] = account.AccountName;
                }
            }
            ViewData["JournalTypeList"] = CodeUtils.GetSelectListByModel(dao.GetJournalTypeAll(false), journal.JournalType, true);
            ViewData["Amount"] = journal.Amount;
            ViewData["Summary"] = journal.Summary;
            ViewData["SlipNumber"] = journal.SlipNumber;
            ViewData["CustomerClaimList"] = CodeUtils.GetSelectList(dao.GetCustomerClaimList(journal.SlipNumber), journal.CustomerClaimCode, true);

            //Add 2016/06/17 arc nakayama #3536_現金出納帳　店舗締め解除処理ボタンの追加
            //経理権限の有無
            Employee loginUser = (Employee)Session["Employee"];
            ViewData["Accounting"] = CheckApplicationRole(loginUser.EmployeeCode, "Accounting");
            
            CashBalance cashBalancePrevData = GetLatestClosedData(form["OfficeCode"], form["CashAccountCode"]);

            ViewData["CloseStatus"] = false;
            if (cashBalancePrevData != null && cashBalancePrevData.ClosedDate != null)
            {
                string TargetDate = cashBalancePrevData.ClosedDate.ToString();
                ViewData["CloseStatus"] = new CloseMonthControlDao(db).IsCloseEndInventoryMonth(TargetDate);
            }
        }

        /// <summary>
        /// 事業所操作権限チェック
        /// </summary>
        /// <param name="officeCode">事業所コード</param>
        /// <returns>チェック結果(True:権限あり False:権限なし)</returns>
        private bool CheckOfficeAuth(string officeCode) {

            Office officeCondition = new Office();
            officeCondition.SetAuthCondition((Employee)Session["Employee"]);
            officeCondition.OfficeCode = officeCode;

            return (new OfficeDao(db).GetByKey(officeCondition) != null);
        }

        /// <summary>
        /// 現金出納帳検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>現金出納帳検索結果リスト</returns>
        private List<Journal> GetSearchResultList(FormCollection form) {

            JournalDao journalDao = new JournalDao(db);
            Journal journalCondition = new Journal();
            journalCondition.SlipNumber = form["CondSlipNumber"];

            // 伝票番号が指定されたら他の検索条件は無視
            if (string.IsNullOrEmpty(journalCondition.SlipNumber)) {
                journalCondition.JournalType = form["CondJournalType"];
                if (!string.IsNullOrEmpty(form["CondYearMonth"])) {
                    string[] yyyyMMArr = CommonUtils.DefaultString(form["CondYearMonth"]).Split(new string[] { "/" }, StringSplitOptions.None);
                    if (!string.IsNullOrEmpty(form["CondDay"])) {
                        journalCondition.CondJournalDate = CommonUtils.StrToDateTime(form["CondYearMonth"] + "/" + string.Format("{0:00}", form["CondDay"]), DateTime.Today) ?? DateTime.Today;
                    } else {
                        string dayFrom = "01";
                        string dayTo = "10";
                        if (!string.IsNullOrEmpty(form["CondDayFrom"])) {
                            dayFrom = form["CondDayFrom"];
                        }
                        if (!string.IsNullOrEmpty(form["CondDayTo"])) {
                            dayTo = form["CondDayTo"].Equals("31") ? string.Format("{0:00}", DateTime.DaysInMonth(int.Parse(yyyyMMArr[0]), int.Parse(yyyyMMArr[1]))) : form["CondDayTo"];
                        }
                        ViewData["CondDayFrom"] = dayFrom;
                        ViewData["CondDayTo"] = dayTo;
                        journalCondition.JournalDateFrom = CommonUtils.StrToDateTime(form["CondYearMonth"] + "/" + dayFrom, DaoConst.SQL_DATETIME_MAX);
                        journalCondition.JournalDateTo = CommonUtils.StrToDateTime(form["CondYearMonth"] + "/" + dayTo, DaoConst.SQL_DATETIME_MAX);
                    }
                }
                journalCondition.OfficeCode = form["OfficeCode"];
                journalCondition.DepartmentCode = form["CondDepartmentCode"];
                journalCondition.CashAccountCode = form["CashAccountCode"];
                journalCondition.AccountCode = form["CondAccountCode"];
                journalCondition.Summary = form["CondSummary"];
            }
            journalCondition.AccountType = form["AccountType"];
            return journalDao.GetListByCondition(journalCondition);
        }

        #endregion

        #region Validation
        /// <summary>
        /// 現金出納帳入力チェック
        /// </summary>
        /// <param name="journal">現金出納帳データ</param>
        /// <returns>現金出納帳データ</returns>
        /// <history>
        /// 2019/02/14 yano #3972 現金出納帳　出金データ登録時に伝票番号のみで紐づけることができない。
        /// 2018/04/04 arc yano #3764 現金出納帳編集　伝票日付未入力時の保存の不具合
        /// </history>
        private Journal ValidateJournal(Journal journal) {

            // 必須チェック(数値/日付項目は属性チェックを兼ねる)
            if (string.IsNullOrEmpty(journal.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "計上部門"));
            }
            if (string.IsNullOrEmpty(journal.OfficeCode)) {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "入金事業所"));
            }

            if (!ModelState.IsValidField("JournalDate")) {
                ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0003", "伝票日付"));
                if (ModelState["JournalDate"].Errors.Count > 1) {
                    ModelState["JournalDate"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(journal.AccountCode)) {
                ModelState.AddModelError("AccountCode", MessageUtils.GetMessage("E0001", "科目コード"));
            }
            if (string.IsNullOrEmpty(journal.JournalType)) {
                ModelState.AddModelError("JournalType", MessageUtils.GetMessage("E0001", "入出金区分"));
            }
            if (!ModelState.IsValidField("Amount")) {
                ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" }));
                if (ModelState["Amount"].Errors.Count > 1) {
                    ModelState["Amount"].Errors.RemoveAt(0);
                }
            }

            // フォーマットチェック
            if (ModelState.IsValidField("Amount")) {
                if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$")) {
                    ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" }));
                }
            }

            // 値チェック
            if (ModelState.IsValidField("JournalDate")) {
                if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0) {
                    ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "伝票日付", "本日以前" }));
                }
                // 2012.02.23 現金以外は許可
                if (!journal.IsClosed && (journal.AccountType==null || journal.AccountType.Equals("001"))) {
                    CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);
                    DateTime latestClosedDate = (cashBalance == null ? DaoConst.SQL_DATETIME_MIN : cashBalance.ClosedDate);
                    if (journal.JournalDate.CompareTo(latestClosedDate) <= 0) {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "伝票日付", "前回締め日より後" }));
                    }
                }
            }
            // Add 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる  仮締め時のデータ編集権限の有無で分ける（経理か、それ以外か）
            //ログインユーザ情報取得
            Employee loginUser = ((Employee)Session["Employee"]);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //仮締データ編集権限があるかないか


            //Add 2018 2018/04/04 arc yano #3764 必須チェックに引っかからない場合のみチェック
            if (ModelState.IsValidField("JournalDate"))
            {
                //仮締データ編集権限があれば本締めの時のみエラー
                if (AppRole.EnableFlag)
                {
                    //仮締め時の操作権限があった場合は、本締め時だけNGにする
                    if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001"))
                    {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "伝票日付", "前回の月次締め処理が実行された年月より後" }));
                    }
                }
                else
                {
                    // そうでなければ仮締め以上でエラー
                    // Add 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる 全体での消込も個別消込も経理締めを見るようにする
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001"))
                    {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "伝票日付", "前回の月次締め処理が実行された年月より後" }));
                    }
                }
            }

            if (ModelState.IsValidField("Amount")) {
                if ((journal.DelFlag==null || !journal.DelFlag.Equals("1")) && journal.Amount.Equals(0m)) {
                    ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", "0以外の10桁以内の整数のみ" }));
                }
            }

            //-------------------
            // 伝票番号
            //-------------------
            if (!string.IsNullOrEmpty(journal.SlipNumber)) {
                if (!new CarSalesOrderDao(db).IsExistSlipNumber(journal.SlipNumber)) {
                    ModelState.AddModelError("SlipNumber", "指定された伝票番号は存在しません");
                }
                else if (string.IsNullOrEmpty(journal.CustomerClaimCode) && (!string.IsNullOrWhiteSpace(journal.JournalType) && journal.JournalType.Equals(JOURNALTYPE_PAYMENT))) //Mod 2019/02/14 yano #3972
                {    
                    ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0007", new string[] { "伝票番号指定", "請求先" }));
                }
            }
            // Add 2016/05/16 arc nakayama #3548_入金種別が「カード会社からの入金」だった場合のバリデーション追加
            //カード会社からの入金だった場合のバリデーション
            if (journal.AccountType == "011")
            {
                //請求先チェック 請求種別が「クレジット(003)」でない場合エラー
                CustomerClaim CustomerClaimData = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                if (CustomerClaimData != null)
                {
                    if (CustomerClaimData.CustomerClaimType != "003")
                    {
                        ModelState.AddModelError("CustomerClaimCode", "カード会社からの入金を行う場合、請求先の請求種別がクレジットでないといけません。");

                    }
                }

                //請求金額 = 入金額　でなければエラー
                ReceiptPlan ReceiptData = new ReceiptPlanDao(db).GetByStringKey(journal.CreditReceiptPlanId);
                if(ReceiptData != null){

                    if (ReceiptData.Amount != journal.Amount)
                    {
                        ModelState.AddModelError("Amount", "カード会社からの入金を行う場合は、請求金額と同じ金額でないと消し込めません。");
                    }
                }
            }

            //Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」の入金実績が削除された場合のバリデーション
            if (journal.DelFlag == "1" && journal.AccountType == "003")
            {
                //カード会社からの入金予定を検索
                ReceiptPlan PlanData = new ReceiptPlanDao(db).GetByStringKey(journal.CreditReceiptPlanId);
                if (PlanData != null)
                {
                    Journal CreditJournal = new JournalDao(db).GetByStringKey(PlanData.CreditJournalId);
                    //カード会社からの入金実績が存在したらエラー
                    if (CreditJournal != null)
                    {
                        ModelState.AddModelError("", "カード会社からの入金が既に行われているため、削除できません。カード会社からの入金実績を削除してから再度実行して下さい。");
                    }
                }
            }
            return journal;
        }
        #endregion

        #endregion

        #region 現金在高入力画面
        /// <summary>
        /// 現金在高入力画面表示
        /// </summary>
        /// <param name="id">事業所コード</param>
        /// <param name="account">現金口座コード</param>
        /// <returns>現金在高入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id,string account) {
            DateTime targetDate = DateTime.Parse( Request["targetDate"] ?? DateTime.Today.ToString());

            CashBalance cashBalance = new CashBalanceDao(db).GetByKey(id, account, targetDate);

            if (cashBalance == null) {
                cashBalance = new CashBalance();
                cashBalance.OfficeCode = id;
                cashBalance.CashAccountCode = account;
                cashBalance.ClosedDate = targetDate;
            }
            if (CommonUtils.DefaultString(cashBalance.CloseFlag).Equals("1")) {
                ModelState.AddModelError("", MessageUtils.GetMessage("E0017"));
                ViewData["AlreadyClosed"] = "1";
            }
            cashBalance.calculate();

            // 出口
            return View("CashJournalEntry", cashBalance);
        }

        /// <summary>
        /// 現金在高追加更新
        /// </summary>
        /// <param name="cashBalance">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>現金在高入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CashBalance cashBalance, FormCollection form) {

            // モデルデータ算出設定項目の設定
            cashBalance.calculate();

            // データチェック
            ValidateCashBalance(cashBalance);
            if (!ModelState.IsValid) {
                return View("CashJournalEntry", cashBalance);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ登録処理
            using (TransactionScope ts = new TransactionScope()) {
                CashBalance targetCashBalance = new CashBalanceDao(db).GetByKey(cashBalance.OfficeCode, cashBalance.CashAccountCode, cashBalance.ClosedDate);
                if (targetCashBalance == null) {
                    InsertCashBalance(cashBalance);
                } else {
                    if (CommonUtils.DefaultString(targetCashBalance.CloseFlag).Equals("1")) {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0017"));
                        ViewData["AlreadyClosed"] = "1";
                    } else {
                        UpdateCashBalance(targetCashBalance, cashBalance);
                    }
                }

                //Add 2014/08/12 arc amii エラーログ対応 SubmitChangesを一本化 & ログ出力の為のtrycatch文追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
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
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
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
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "登録"));
                            return View("CashJournalEntry", cashBalance);
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
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null,(string)null);
        }
        /// <summary>
        /// 現金在高テーブル追加処理
        /// </summary>
        /// <param name="cashBalance">現金在高データ(登録内容)</param>
        private void InsertCashBalance(CashBalance cashBalance) {

            cashBalance.CloseFlag = "0";
            cashBalance.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.CreateDate = DateTime.Now;
            cashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.LastUpdateDate = DateTime.Now;
            cashBalance.DelFlag = "0";
            db.CashBalance.InsertOnSubmit(cashBalance);
            
        }

        /// <summary>
        /// 現金在高テーブル更新処理
        /// </summary>
        /// <param name="targetCashBalance">現金在高データ(更新前内容)</param>
        /// <param name="cashBalance">現金在高データ(登録内容)</param>
        private void UpdateCashBalance(CashBalance targetCashBalance, CashBalance cashBalance) {

            UpdateModel(targetCashBalance);
            targetCashBalance.calculate();
            targetCashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            targetCashBalance.LastUpdateDate = DateTime.Now;
        }

        #region Validation
        /// <summary>
        /// 現金在高入力チェック
        /// </summary>
        /// <param name="cashBalance">現金在高データ</param>
        /// <returns>現金出納帳データ</returns>
        private CashBalance ValidateCashBalance(CashBalance cashBalance) {

            // 属性チェック
            if (!ModelState.IsValidField("NumberOf10000")) {
                ModelState.AddModelError("NumberOf10000", MessageUtils.GetMessage("E0004", new string[] { "10,000円札枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf5000")) {
                ModelState.AddModelError("NumberOf5000", MessageUtils.GetMessage("E0004", new string[] { "5,000円札枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf2000")) {
                ModelState.AddModelError("NumberOf2000", MessageUtils.GetMessage("E0004", new string[] { "2,000円札枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf1000")) {
                ModelState.AddModelError("NumberOf1000", MessageUtils.GetMessage("E0004", new string[] { "1,000円札枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf500")) {
                ModelState.AddModelError("NumberOf500", MessageUtils.GetMessage("E0004", new string[] { "500円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf100")) {
                ModelState.AddModelError("NumberOf100", MessageUtils.GetMessage("E0004", new string[] { "100円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf50")) {
                ModelState.AddModelError("NumberOf50", MessageUtils.GetMessage("E0004", new string[] { "50円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf10")) {
                ModelState.AddModelError("NumberOf10", MessageUtils.GetMessage("E0004", new string[] { "10円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf5")) {
                ModelState.AddModelError("NumberOf5", MessageUtils.GetMessage("E0004", new string[] { "5円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NumberOf1")) {
                ModelState.AddModelError("NumberOf1", MessageUtils.GetMessage("E0004", new string[] { "1円玉枚数", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("CheckAmount")) {
                ModelState.AddModelError("CheckAmount", MessageUtils.GetMessage("E0004", new string[] { "小切手等金額", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CheckAmount") && cashBalance.CheckAmount != null) {
                if (!Regex.IsMatch(cashBalance.CheckAmount.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CheckAmount", MessageUtils.GetMessage("E0004", new string[] { "小切手等金額", "正の10桁以内の整数のみ" }));
                }
            }

            // 値チェック
            if (ModelState.IsValidField("NumberOf10000") && cashBalance.NumberOf10000 != null) {
                if (cashBalance.NumberOf10000 < 0) {
                    ModelState.AddModelError("NumberOf10000", MessageUtils.GetMessage("E0004", new string[] { "10,000円札枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf5000") && cashBalance.NumberOf5000 != null) {
                if (cashBalance.NumberOf5000 < 0) {
                    ModelState.AddModelError("NumberOf5000", MessageUtils.GetMessage("E0004", new string[] { "5,000円札枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf2000") && cashBalance.NumberOf2000 != null) {
                if (cashBalance.NumberOf2000 < 0) {
                    ModelState.AddModelError("NumberOf2000", MessageUtils.GetMessage("E0004", new string[] { "2,000円札枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf1000") && cashBalance.NumberOf1000 != null) {
                if (cashBalance.NumberOf1000 < 0) {
                    ModelState.AddModelError("NumberOf1000", MessageUtils.GetMessage("E0004", new string[] { "1,000円札枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf500") && cashBalance.NumberOf500 != null) {
                if (cashBalance.NumberOf500 < 0) {
                    ModelState.AddModelError("NumberOf500", MessageUtils.GetMessage("E0004", new string[] { "500円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf100") && cashBalance.NumberOf100 != null) {
                if (cashBalance.NumberOf100 < 0) {
                    ModelState.AddModelError("NumberOf100", MessageUtils.GetMessage("E0004", new string[] { "100円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf50") && cashBalance.NumberOf50 != null) {
                if (cashBalance.NumberOf50 < 0) {
                    ModelState.AddModelError("NumberOf50", MessageUtils.GetMessage("E0004", new string[] { "50円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf10") && cashBalance.NumberOf10 != null) {
                if (cashBalance.NumberOf10 < 0) {
                    ModelState.AddModelError("NumberOf10", MessageUtils.GetMessage("E0004", new string[] { "10円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf5") && cashBalance.NumberOf5 != null) {
                if (cashBalance.NumberOf5 < 0) {
                    ModelState.AddModelError("NumberOf5", MessageUtils.GetMessage("E0004", new string[] { "5円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NumberOf1") && cashBalance.NumberOf1 != null) {
                if (cashBalance.NumberOf1 < 0) {
                    ModelState.AddModelError("NumberOf1", MessageUtils.GetMessage("E0004", new string[] { "1円玉枚数", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("TotalAmount")) {
                if (decimal.Compare(cashBalance.TotalAmount ?? 0m, -9999999999m) < 0 || decimal.Compare(cashBalance.TotalAmount ?? 0m, 9999999999m) > 0) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0018", "合計金額"));
                }
            }

            return cashBalance;
        }
        #endregion

        #endregion

        #region 現金出納帳編集・更新
        /// <summary>
        /// 現金出納帳の編集画面表示
        /// </summary>
        /// <param name="id">JournalId</param>
        /// <returns></returns>
        public ActionResult Edit(string id) {
            Journal journal = new JournalDao(db).GetByKey(new Guid(id));
            //CodeDao dao = new CodeDao(db);
            //ViewData["JournalTypeList"] = CodeUtils.GetSelectListByModel(dao.GetJournalTypeAll(false), journal.JournalType, true);
            SetEditViewData(journal);

            // 初回開くときだけの判断
            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);
            // 伝票日付が締日よりも前だったら締め処理済み扱い
            if (cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate) {
                journal.IsClosed = true;
            }
            return View("CashJournalEdit", journal);
        }

        /// <summary>
        /// 現金出納帳データの更新
        /// </summary>
        /// <param name="journal">Journalモデルデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(Journal journal,FormCollection form) {
            journal = ValidateJournal(journal);

            //入金消込保存の場合
            if (form["actionType"].Equals("Slip")) {
                //伝票番号必須
                if (string.IsNullOrEmpty(journal.SlipNumber)) {
                    ModelState.AddModelError("SlipNumber", MessageUtils.GetMessage("E0007", new string[] { "入金消込保存", "伝票番号" }));
                }

                //Mod 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
                //現金の入金予定のみ取得してくるように変更
                List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(journal.SlipNumber, journal.CustomerClaimCode);
                if (planList == null || planList.Count == 0) {
                    ModelState.AddModelError("SlipNumber", "指定された伝票番号に該当する入金予定がありません");
                }
                if (planList != null && planList.Sum(x => x.ReceivableBalance) < journal.Amount) {
                    ModelState.AddModelError("Amount", "入金額が入金予定よりも多いため消込できません");
                }
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (CommonUtils.DefaultString(journal.JournalType).Equals("002"))
                {
                    ModelState.AddModelError("JournalType", "入金消込保存は入金時しか利用できません");
                }
            }

            if (!ModelState.IsValid) {
                SetEditViewData(journal);
                return View("CashJournalEdit", journal);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                Journal target = new JournalDao(db).GetByKey(journal.JournalId);

                //Add 2016/05/17 arc nakayama #3551_伝票番号を変更した時の処理
                //変更前の伝票番号と請求先を取得退避
                string PrevSlipNumber = target.SlipNumber;
                string PrevCustomerClaimCode = target.CustomerClaimCode;
                decimal PrevAmount = target.Amount;

                UpdateModel<Journal>(target);
                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                if (form["actionType"].Equals("Slip")) {
                    List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetCashListByCustomerClaim(journal.SlipNumber, journal.CustomerClaimCode);

                    Payment(planList, journal.Amount);

                    target.ReceiptPlanFlag = "1";
                    target.CustomerClaimCode = planList[0].CustomerClaimCode;
                }


                //Mod 2018/08/22 yano #3930     //削除・更新処理に関わらず、更新された実績を元に入金予定を再作成する
                //一度入金実績の修正内容を反映しておく
                db.SubmitChanges();
                
                //---------------------------
                //入金予定再作成
                //---------------------------
                if (!string.IsNullOrEmpty(target.SlipNumber) && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                {
                    // 車両伝票
                    CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                    if (carSalesHeader != null)
                    {
                        CreateCarReceiptPlan(carSalesHeader, journal);
                    }

                    // サービス伝票
                    ServiceSalesHeader serviceSalesheader = new ServiceSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                    if (serviceSalesheader != null)
                    {
                        CreateServiceReceiptPlan(serviceSalesheader, journal, PrevSlipNumber, PrevCustomerClaimCode, PrevAmount, true);
                    }
                    if (!ModelState.IsValid)
                    {
                        SetEditViewData(journal);
                        return View("CashJournal", journal);
                    }
                }

                ////Add 2016/05/20 #3538_諸費用以外も消込保存が行えるようにする
                ////Mod 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮 削除を行った場合は、多い順に残高をもとに戻す 
                //if (!string.IsNullOrEmpty(target.SlipNumber) && target.DelFlag.Equals("1") && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //{
                //    //------------
                //    //削除処理
                //    //------------
                //    List<ReceiptPlan> BackPlanDataList = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(target.SlipNumber, target.CustomerClaimCode);

                //    //金額の多い順に返金を行う
                //    RePayment(BackPlanDataList, target.Amount);
                //}
                //else
                //{
                //    //Mod 2018/01/11 arc yano #3717
                //    if (!string.IsNullOrEmpty(target.SlipNumber) && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //    //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                //    {
                //        // 車両伝票
                //        CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                //        if (carSalesHeader != null)
                //        {
                //            CreateCarReceiptPlan(carSalesHeader, journal);
                //        }

                //        // サービス伝票
                //        ServiceSalesHeader serviceSalesheader = new ServiceSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                //        if (serviceSalesheader != null)
                //        {
                //            CreateServiceReceiptPlan(serviceSalesheader, journal, PrevSlipNumber, PrevCustomerClaimCode, PrevAmount, true);
                //        }
                //        if (!ModelState.IsValid)
                //        {
                //            SetEditViewData(journal);
                //            return View("CashJournal", journal);
                //        }
                //    }
                //}

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
                        // Add 2014/08/12 arc amii エラーログ対応 更新失敗時のエラー処理 + ログ出力処理を追加
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, target.SlipNumber);
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
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetEditViewData(journal);
                            return View("CashJournalEdit", journal);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, target.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                        return View("Error");
                    }
                }
                
            }
            SetEditViewData(journal);
            ViewData["close"] = "1";
            return View("CashJournalEdit", journal);
        }
        /// <summary>
        /// 現金出納帳編集画面データ取得
        /// </summary>
        /// <history>
        /// 2018/10/30 yano #3943 入金消込後の伝票番号の請求先変更対応
        /// </history>
        /// <param name="journal"></param>
        private void SetEditViewData(Journal journal) {
            CodeDao dao = new CodeDao(db);
            ViewData["JournalTypeList"] = CodeUtils.GetSelectListByModel(dao.GetJournalTypeAll(false), journal.JournalType, true);
            List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(journal.OfficeCode);
            List<CodeData> accountDataList = new List<CodeData>();
            foreach (var a in accountList) {
                CodeData data = new CodeData();
                data.Code = a.CashAccountCode;
                data.Name = a.CashAccountName;
                accountDataList.Add(data);
            }
            ViewData["CashAccountCodeList"] = CodeUtils.GetSelectList(accountDataList, journal.CashAccountCode, false);

 
            Employee loginUser = (Employee)Session["Employee"];
            ViewData["AccountTypeList"] = CodeUtils.GetSelectListByModel(dao.GetAccountTypeAll(false), journal.AccountType, false);
            journal.Department = new DepartmentDao(db).GetByKey(journal.DepartmentCode);
            journal.Office = new OfficeDao(db).GetByKey(journal.OfficeCode);
            journal.Account = new AccountDao(db).GetByKey(journal.AccountCode);

            
            //Mod 2018/10/30 yano #3943
            //Mod 2015/10/27 arc nakayama #3286_入金実績修正画面で請求先が表示されない 
            //ViewData["CustomerClaimList"] = CodeUtils.GetSelectList(dao.GetCustomerClaimListByjournal(journal.JournalId), journal.CustomerClaimCode, true);

            ViewData["CustomerClaimList"] = CodeUtils.GetSelectList(dao.GetCustomerClaimListByjournalReceiptPlan(journal), journal.CustomerClaimCode, true);    //Mod 2018/12/28 yano #3970


            //Add 2016/05/19 arc nakayama #3544_入金種別をカード・ローンには変更させない 経理者権限の有無を取得

            bool Accounting = CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode, "Accounting");

            c_AccountType AccountTypeRet = new CodeDao(db).GetAccountTypeByKey(journal.AccountType, false);

            ViewData["AccountingFilter"] = false;
            if (AccountTypeRet != null)
            {
                //経理権限がないと編集できない入金種別、かつ、経理権限がない　かつ、カードまたはローンでない場合編集不可のフィルターをかける 
                if (AccountTypeRet.CommonSelectFlag == "0" && !Accounting && !(journal.AccountType.Equals("003") || journal.AccountType.Equals("004")))
                {
                    ViewData["AccountingFilter"] = true;
                }
            }
        }
        public ActionResult GetMasterWithClaim(string slipNumber) {
            if (Request.IsAjaxRequest()) {
                CodeDataList dataList = new CodeDataList();
                if (new CarSalesOrderDao(db).IsExistSlipNumber(slipNumber)) {
                    dataList.Code = slipNumber;
                    dataList.DataList = new CodeDao(db).GetCustomerClaimList(slipNumber);
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, dataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
            return new EmptyResult();
        }
        #endregion

        #region 締め処理
        /// <summary>
        /// 本日締め情報(=入力情報)取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>本日締め情報(=入力情報)</returns>
        private CashBalance GetTodayBalanceData(string officeCode, string cashAccountCode, DateTime targetDate) {

            CashBalance cashBalanceCondition = new CashBalance();
            cashBalanceCondition.SetAuthCondition((Employee)Session["Employee"]);
            cashBalanceCondition.OfficeCode = officeCode;
            cashBalanceCondition.CashAccountCode = cashAccountCode;
            cashBalanceCondition.ClosedDate = targetDate;

            return new CashBalanceDao(db).GetByKey(cashBalanceCondition);
        }

        /// <summary>
        /// 直近の締め情報取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>直近の締め情報</returns>
        private CashBalance GetLatestClosedData(string officeCode,string cashAccountCode) {

            CashBalance cashBalanceCondition = new CashBalance();
            cashBalanceCondition.SetAuthCondition((Employee)Session["Employee"]);
            cashBalanceCondition.OfficeCode = officeCode;
            cashBalanceCondition.CashAccountCode = cashAccountCode;

            return new CashBalanceDao(db).GetLatestClosedData(cashBalanceCondition);
        }

        /// <summary>
        /// 今期現金出納帳明細金額合計取得
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="termFrom">期首日</param>
        /// <param name="termTo">期末日</param>
        /// <returns>現金出納帳明細金額合計</returns>
        private decimal GetDetailsTotal(string officeCode, string cashAccountCode, DateTime termFrom, DateTime termTo) {

            Journal journalCondition = new Journal();
            journalCondition.SetAuthCondition((Employee)Session["Employee"]);
            journalCondition.OfficeCode = officeCode;
            journalCondition.CashAccountCode = cashAccountCode;

            journalCondition.JournalDateFrom = termFrom;
            journalCondition.JournalDateTo = termTo;
            journalCondition.AccountType = "001";

            return new JournalDao(db).GetDetailsTotal(journalCondition);
        }
        /// <summary>
        /// 締め処理
        /// </summary>
        /// <param name="cashBalance">現金在高モデルデータ</param>
        private void CloseAccount(CashBalance cashBalance) {

            cashBalance.CloseFlag = "1";
            cashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.LastUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// 締め解除処理
        /// </summary>
        /// <param name="cashBalance">現金在高モデルデータ</param>
        /// <history>Add 2016/05/18 arc nakayama #3536_現金出納帳　店舗締め解除処理ボタンの追加 店舗締め解除処理追加</history>
        private void ReleaseAccount(CashBalance cashBalance)
        {

            cashBalance.CloseFlag = "0";
            cashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.LastUpdateDate = DateTime.Now;
        }
        #endregion

        #region 入金予定を再作成

        #region 車両伝票にかかわる入金予定再作成
        /// <summary>
        ///  車両伝票にかかわる入金予定再作成
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <param name="journal">入金実績</param>
        /// <history>
        /// 2018/11/14 yano #3814 受注後キャンセル後の入金実績修正による入金予定作成不具合対策
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正
        /// 2017/09/25 arc yano  #3800 下取、残債の入金予定の再作成処理の追加
        /// 2017/09/25 arc yano  #3798 マイナスの入金予の再作成も行える様に修正
        /// </history>
        private void CreateCarReceiptPlan(CarSalesHeader header,Journal journal) {

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            //Mod 2018/11/14 yano #3814
            //伝票ステータス=キャンセル、または受注後キャンセルの場合
            if (header.SalesOrderStatus.Equals(STATUS_CAR_CANCEL) || header.SalesOrderStatus.Equals(STATUS_CAR_ORDERCANCEL))
            {
                CreateCancelCarReceiptPlan(header, journal);
            }
            else
            {
                CreateNotCancelCarReceiptPlan(header, journal, carAccount);
            }            
        }

         /// <summary>
        ///  通常ステータスの車両伝票の入金予定再作成
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <param name="journal">入金実績</param>
        /// <history>
        /// 2018/11/14 yano #3814 受注後キャンセル後の入金実績修正による入金予定作成不具合対策　新規作成
        /// </history>
        private void CreateNotCancelCarReceiptPlan(CarSalesHeader header, Journal journal, Account carAccount)
        {
            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var d in delList)
            {
                //残債、下取以外の入金予定を削除
                if (!d.ReceiptType.Equals("012") && !d.ReceiptType.Equals("013"))       //Mod 2017/09/25 arc yano  #3800
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }

            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });

            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();


            //Mod 2017/09/25 arc yano  #3798
            // 入金実績額
            decimal PlusjournalAmount = 0m;
            decimal MinusjournalAmount = 0m;
            for (int i = 0; i < payList.Count; i++)
            {
                //請求先リストに追加
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //請求先が変わったら入金済み金額を（再）取得する
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, true);
                    MinusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, false);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                //請求額がプラスの場合
                if (payList[i].Amount >= 0)
                {

                    if (payList[i].Amount >= PlusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - PlusjournalAmount);
                        PlusjournalAmount = 0m;
                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = 0m;
                        PlusjournalAmount = PlusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = PlusjournalAmount * (-1);
                        }
                    }
                }
                else //請求額がマイナスの場合
                {
                    if (payList[i].Amount >= MinusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = MinusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = MinusjournalAmount * (-1);
                        }

                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = 0m;
                    }
                }

                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptPlanId = Guid.NewGuid();
                plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.DelFlag = "0";
                plan.LastUpdateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.ReceiptType = "001"; // 現金
                plan.SlipNumber = header.SlipNumber;
                plan.OccurredDepartmentCode = header.DepartmentCode;
                plan.AccountCode = carAccount.AccountCode;
                plan.CustomerClaimCode = payList[i].CustomerClaimCode;
                plan.DepartmentCode = header.DepartmentCode;
                plan.ReceiptPlanDate = payList[i].PaymentPlanDate;
                plan.Amount = payList[i].Amount;
                plan.ReceivableBalance = balanceAmount;
                plan.Summary = payList[i].Memo;
                if (balanceAmount.Equals(0m))
                {
                    plan.CompleteFlag = "1";
                }
                else
                {
                    plan.CompleteFlag = "0";
                }
                plan.JournalDate = header.SalesOrderDate ?? DateTime.Today;
                db.ReceiptPlan.InsertOnSubmit(plan);
            }

            //ローンの入金予定
            decimal remainAmount = 0m; //入金済み金額を初期化

            //プランが選択されているときだけ処理
            if (!string.IsNullOrEmpty(header.PaymentPlanType))
            {
                //ローンコードを取得する
                object loanCode = header.GetType().GetProperty("LoanCode" + header.PaymentPlanType).GetValue(header, null);

                //ローンコードは必須
                if (loanCode != null && !loanCode.Equals(""))
                {
                    Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());

                    //マスタに存在すること
                    if (loan != null)
                    {

                        //請求先リストに追加
                        customerClaimList.Add(loan.CustomerClaimCode);

                        //入金済み金額を取得
                        remainAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, loan.CustomerClaimCode);

                        //ローン元金
                        decimal planAmount = decimal.Parse(CommonUtils.GetModelProperty(header, "LoanPrincipal" + header.PaymentPlanType).ToString());

                        //入金残
                        decimal receivableBalance = planAmount - remainAmount;
                        if (receivableBalance < 0m)
                        {
                            //入金残がマイナスの場合、過入金通知
                            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                            task.CarOverReceive(header, loan.CustomerClaimCode, Math.Abs(receivableBalance));
                            //残はゼロで設定
                            receivableBalance = 0m;
                        }

                        ReceiptPlan plan = new ReceiptPlan();
                        plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                        plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.DelFlag = "0";
                        plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //経理に入金予定
                        plan.LastUpdateDate = DateTime.Now;
                        plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.ReceiptPlanId = Guid.NewGuid();
                        plan.ReceiptType = "004"; //ローン
                        plan.SlipNumber = header.SlipNumber;
                        plan.OccurredDepartmentCode = header.DepartmentCode;
                        plan.CustomerClaimCode = loan.CustomerClaimCode;
                        plan.ReceiptPlanDate = CommonUtils.GetPaymentPlanDate(header.SalesOrderDate ?? DateTime.Today, loan.CustomerClaim);
                        plan.Amount = planAmount;
                        plan.ReceivableBalance = receivableBalance;
                        if (receivableBalance.Equals(0m))   //Mod 2018/08/22 yano  #3930
                        {
                            plan.CompleteFlag = "1";
                        }
                        else
                        {
                            plan.CompleteFlag = "0";
                        }
                        plan.AccountCode = carAccount.AccountCode;
                        db.ReceiptPlan.InsertOnSubmit(plan);
                    }
                }
            }

            //Add 2018/08/22 yano #3930
            //入金済みの請求先が今回の伝票からなくなっているものを通知
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList, excluedAccountTypetList);
            foreach (Journal a in journalList)
            {
                if (customerClaimList.IndexOf(a.CustomerClaimCode) < 0)
                {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // マイナスで入金予定作成
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.ReceiptType = "001"; // 現金
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }

            //下取、残債の入金予定の再作成
            CreateTradeReceiptPlan(header);      //Add 2017/09/25 arc yano #3800
        }

        /// <summary>
        ///  キャンセル、受注後キャンセル車両伝票の入金予定再作成
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <param name="journal">入金実績</param>
        /// <history>
        /// 2018/11/14 yano #3814 受注後キャンセル後の入金実績修正による入金予定作成不具合対策
        /// </history>
        private void CreateCancelCarReceiptPlan(CarSalesHeader header, Journal journal)
        {
            //入金予定を削除
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var p in planList)
            {
                p.DelFlag = "1";
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //残債と下取の入金実績は削除
            List<Journal> DelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("012") || x.AccountType.Equals("013"))).ToList();
            foreach (var d in DelJournal)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            db.SubmitChanges();

            //入金済みがあればアラート
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            foreach (var j in journalList)
            {

                // 過入金のアラートを作成
                task.CarOverReceive(header, j.CustomerClaimCode, j.Amount);

                // 実績分の入金予定を作成
                ReceiptPlan plusPlan = new ReceiptPlan();
                plusPlan.ReceiptPlanId = Guid.NewGuid();
                plusPlan.DepartmentCode = j.DepartmentCode;
                plusPlan.OccurredDepartmentCode = j.DepartmentCode;
                plusPlan.CustomerClaimCode = j.CustomerClaimCode;
                plusPlan.SlipNumber = j.SlipNumber;
                plusPlan.ReceiptType = j.AccountType;
                plusPlan.ReceiptPlanDate = DateTime.Today;
                plusPlan.AccountCode = j.AccountCode;
                plusPlan.Amount = j.Amount;
                plusPlan.ReceivableBalance = 0;             //残高 = 0(固定)
                plusPlan.CompleteFlag = "1";                //入金完了フラグ = 0(固定)
                plusPlan.CreateDate = DateTime.Now;
                plusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.LastUpdateDate = DateTime.Now;
                plusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.DelFlag = "0";
                plusPlan.JournalDate = DateTime.Today;
                plusPlan.DepositFlag = "0";
                db.ReceiptPlan.InsertOnSubmit(plusPlan);


                // 過入金分のマイナス入金予定を作成
                ReceiptPlan minusPlan = new ReceiptPlan();
                minusPlan.ReceiptPlanId = Guid.NewGuid();
                minusPlan.DepartmentCode = j.DepartmentCode;
                minusPlan.OccurredDepartmentCode = j.DepartmentCode;
                minusPlan.CustomerClaimCode = j.CustomerClaimCode;
                minusPlan.SlipNumber = j.SlipNumber;
                minusPlan.ReceiptType = j.AccountType;
                minusPlan.ReceiptPlanDate = DateTime.Today;
                minusPlan.AccountCode = j.AccountCode;
                minusPlan.Amount = -1 * j.Amount;
                minusPlan.ReceivableBalance = -1 * j.Amount;
                minusPlan.CompleteFlag = "0";
                minusPlan.CreateDate = DateTime.Now;
                minusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                minusPlan.LastUpdateDate = DateTime.Now;
                minusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                minusPlan.DelFlag = "0";
                minusPlan.JournalDate = DateTime.Today;
                minusPlan.DepositFlag = "0";
                db.ReceiptPlan.InsertOnSubmit(minusPlan);
            }
        }

        /// <summary>
        /// 下取車に関する入金予定の作成
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <param name="journal">入金実績</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 車両伝票－下取車の入金予定残高更新不整合
        /// 2017/09/25 arc yano #3800 下取、残債の入金予定の作成処理の追加　新規作成（車両伝票の処理より流用）
        /// </history>
        private void CreateTradeReceiptPlan(CarSalesHeader header)
        {
            //下取、残債の既存の入金予定の削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
            
            foreach (var d in delList)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                    {
                        TradeInAmount = varTradeInAmount.ToString();
                    }

                    var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                    {
                        TradeInRemainDebt = varTradeInRemainDebt.ToString();
                    }

                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //下取の入金額
                    decimal JournalDebtAmount = 0; //残債の入金額

                    //Mod 2017/11/14 arc yano #3811
                    //下取の入金額取得
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //残債の入金額取得
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    //下取の入金予定作成
                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013";                                  //下取
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //☆計算
                    if (TradePlan.ReceivableBalance == 0m)
                    {
                        TradePlan.CompleteFlag = "1";
                    }
                    else
                    {
                        TradePlan.CompleteFlag = "0";
                    }
                    TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.CreateDate = DateTime.Now;
                    TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.LastUpdateDate = DateTime.Now;
                    TradePlan.DelFlag = "0";
                    TradePlan.Summary = "";
                    TradePlan.JournalDate = null;
                    TradePlan.DepositFlag = "0";
                    TradePlan.PaymentKindCode = "";
                    TradePlan.CommissionRate = null;
                    TradePlan.CommissionAmount = null;
                    TradePlan.CreditJournalId = "";
                    TradePlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                    db.ReceiptPlan.InsertOnSubmit(TradePlan);

                    //残債があった場合残債分の入金予定をマイナスで作成する
                    if (!string.IsNullOrEmpty(TradeInRemainDebt))
                    {
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //残債は経理に振り替え
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012";                                                                 //残債
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //計算
                        if (RemainDebtPlan.ReceivableBalance == 0m)
                        {
                            RemainDebtPlan.CompleteFlag = "1";
                        }
                        else
                        {
                            RemainDebtPlan.CompleteFlag = "0";
                        }
                        RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.CreateDate = DateTime.Now;
                        RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.LastUpdateDate = DateTime.Now;
                        RemainDebtPlan.DelFlag = "0";
                        RemainDebtPlan.Summary = "";
                        RemainDebtPlan.JournalDate = null;
                        RemainDebtPlan.DepositFlag = "0";
                        RemainDebtPlan.PaymentKindCode = "";
                        RemainDebtPlan.CommissionRate = null;
                        RemainDebtPlan.CommissionAmount = null;
                        RemainDebtPlan.CreditJournalId = "";
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                }
            }
        }

        #endregion

        #region サービス伝票にかかわる入金予定再作成

        /// <summary>
        /// サービス伝票にかかわる入金予定再作成
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="journal">入金実績</param>
        /// <param name="journal">変更前伝票番号</param>
        /// <param name="journal">変更前請求先コード</param>
        /// <param name="journal">変更前金額</param>
        /// <history>
        /// 2018/11/14 yano #3814 受注後キャンセル後の入金実績修正による入金予定作成不具合対策
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正
        /// 2017/11/28 arc yano #3828 入金実績リスト－サービス伝票で過入金時に入金予定の残高がマイナスとならない
        /// </history>
        private void CreateServiceReceiptPlan(ServiceSalesHeader header, Journal journal, string PrevSlipNumber, string PrevCustomerClaimCode, decimal PrevAmount, bool cashJournalFlag)
        {
            //Mod 2018/08/22 yano #3930
            IServiceSalesOrderService service = new ServiceSalesOrderService(db);

            //Mod 2018/11/14 yano #3814
            if (header.ServiceOrderStatus.Equals(STATUS_SERVICE_CANCEL))    //伝票ステータス=キャンセル
            {
                CreateCancelServiceReceiptPlan(header);
            }
            else
            {
                //入金予定再作成
                service.CreateReceiptPlan(header, header.ServiceSalesLine, header.ServiceSalesPayment.ToList());
            }

            /*
            //科目を取得
            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");
            if (serviceAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            //Add 2016/05/17 arc nakayama #3551_伝票番号を変更した時の処理
            //Mod 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
            //変更前と変更後で伝票番号に違いがあったら変更前の残高をもとに戻す
            //変更前の伝票番号が未入力の場合はなにもしない
            if ((!string.IsNullOrEmpty(PrevSlipNumber) && journal.SlipNumber != PrevSlipNumber) || (journal.CustomerClaimCode != PrevCustomerClaimCode))
            {
                //振り替え元の入金予定取得　現金出納帳から更新された場合は現金の入金予定のみ取得
                List<ReceiptPlan> ReTargetPlan = new List<ReceiptPlan>();

                if (cashJournalFlag)
                {
                    ReTargetPlan = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(PrevSlipNumber, PrevCustomerClaimCode);
                }
                else
                {
                    ReTargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaimDesc(PrevSlipNumber, PrevCustomerClaimCode);
                }
                //振り替え元の入金予定の残高をもとに戻す
                RePayment(ReTargetPlan, PrevAmount);

                //振り替え先の入金予定取得　現金出納帳から更新された場合は現金の入金予定のみ取得
                List<ReceiptPlan> TargetPlan = new List<ReceiptPlan>();

                if (cashJournalFlag)
                {
                    TargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(journal.SlipNumber, journal.CustomerClaimCode);
                }
                else
                {
                    TargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(journal.SlipNumber, journal.CustomerClaimCode);
                }

                //振り替え先の残高更新
                Payment(TargetPlan, journal.Amount);
            }


            db.SubmitChanges();

            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, header.DepartmentCode);
            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }


            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            List<ServiceSalesPayment> payList = header.ServiceSalesPayment.ToList();
            payList.Sort(delegate(ServiceSalesPayment x, ServiceSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });

            //Add 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮
            //さらに金額の少ない順に並び替える
            payList = payList.OrderBy(a => a.CustomerClaimCode).ThenBy(b => b.Amount).ToList<ServiceSalesPayment>();


            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();

            //登録されている行数だけ繰り返す
            decimal remainAmount = 0m; //入金済み金額
            for (int i = 0; i < payList.Count; i++)
            {
                //Mod 2017/11/28 arc yano #3828 計算方法をサービス伝票に合わせる

                //請求先リストに追加
                customerClaimList.Add(payList[i].CustomerClaimCode);

                // 初回or請求先が変わったとき、入金消込済み金額を（再）取得する
                if (i == 0 || (i > 0 && !payList[i - 1].CustomerClaimCode.Equals(payList[i].CustomerClaimCode)))
                {
                    // クレジット・ローン以外の入金消込済み金額
                    remainAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, payList[i].CustomerClaimCode);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                if (payList[i].Amount >= remainAmount)
                {
                    // 予定額 >= 実績額
                    balanceAmount = ((payList[i].Amount ?? 0m) - remainAmount);
                    remainAmount = 0m;
                }
                else
                {
                    // 予定額 < 実績額
                    balanceAmount = 0m;
                    remainAmount = remainAmount - (payList[i].Amount ?? 0m);

                    // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                    if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !payList[i].CustomerClaimCode.Equals(payList[i + 1].CustomerClaimCode)))
                    {
                        balanceAmount = remainAmount * (-1);
                    }
                }

                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptPlanId = Guid.NewGuid();
                plan.CreateDate = DateTime.Now;
                plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.SlipNumber = header.SlipNumber;
                plan.LastUpdateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.DelFlag = "0";

                plan.DepartmentCode = header.DepartmentCode;
                plan.ReceiptPlanDate = payList[i].PaymentPlanDate;
                plan.ReceiptType = "001";

                plan.Amount = payList[i].Amount;
                plan.CustomerClaimCode = payList[i].CustomerClaimCode;
                plan.OccurredDepartmentCode = header.DepartmentCode;
                plan.AccountCode = serviceAccount.AccountCode;
                plan.ReceivableBalance = balanceAmount;
                plan.Summary = payList[i].Memo;
                if (balanceAmount.Equals(0m))
                {
                    plan.CompleteFlag = "1";
                }
                else
                {
                    plan.CompleteFlag = "0";
                }
                plan.JournalDate = header.SalesOrderDate ?? DateTime.Today;
                if (payList[i].DepositFlag != null && payList[i].DepositFlag.Equals("1"))
                {
                    plan.DepositFlag = "1";
                }
                db.ReceiptPlan.InsertOnSubmit(plan);
            }
            */
        }
       
        /// <summary>
        /// ステータスがキャンセルのサービス伝票の入金予定再作成
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <history>
        /// 2018/11/14 yano #3814 受注後キャンセル後の入金実績修正による入金予定作成不具合対策
        /// </history>
        private void CreateCancelServiceReceiptPlan(ServiceSalesHeader header)
        {
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);

            //既存の入金予定を削除
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var p in planList)
            {
                p.DelFlag = "1";
            }

            //Mod 2016/05/13 arc yano #3528
            //入金済みがあれば実績分のマイナスの入金予定を作成する
            if (journalList != null && journalList.Count() > 0)
            {
                //マイナスの入金予定の再作成
                foreach (var j in journalList)
                {
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.DepartmentCode = j.DepartmentCode;
                    plan.OccurredDepartmentCode = j.DepartmentCode;
                    plan.CustomerClaimCode = j.CustomerClaimCode;
                    plan.SlipNumber = j.SlipNumber;
                    plan.ReceiptType = "001";                                                           //「現金」固定
                    plan.ReceiptPlanDate = j.JournalDate;
                    plan.AccountCode = j.AccountCode;
                    plan.Amount = 0;                                                                    //「0」固定
                    plan.ReceivableBalance = j.Amount * (-1);
                    plan.CompleteFlag = "0";                                                            //「0」固定
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.CreateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.LastUpdateDate = DateTime.Now;
                    plan.DelFlag = "0";                                                                 //「0」固定
                    plan.Summary = j.Summary;
                    plan.JournalDate = null;
                    plan.DepositFlag = null;                                                            //諸費用フラグ = null
                    plan.PaymentKindCode = j.PaymentKindCode;
                    plan.CommissionRate = null;                                                         //手数料率 = null
                    plan.CommissionAmount = null;                                                       //手数料 = null
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }
        }

        #endregion
        #endregion

        #region 入金実績リスト
        public ActionResult JournalCriteria() {
            return JournalCriteria(new FormCollection());
        }

        #region 入金実績リスト検索処理
        /// <summary>
        /// 入金実績リスト検索処理
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult JournalCriteria(FormCollection form) {

            form["CondYearMonth"] = (form["CondYearMonth"] == null ? string.Format("{0:yyyy/MM}", DateTime.Now) : form["CondYearMonth"]);
            form["CondDay"] = (form["CondDay"] == null ? DateTime.Now.Day.ToString() : form["CondDay"]);
            try { form["OfficeCode"] = (string.IsNullOrEmpty(form["OfficeCode"]) ? ((Employee)Session["Employee"]).Department1.OfficeCode : form["OfficeCode"]); } catch (NullReferenceException) { }
            ViewData["DefaultCondYearMonth"] = string.Format("{0:yyyy/MM}", DateTime.Now);
            ViewData["DefaultCondDay"] = DateTime.Now.Day.ToString();
            ViewData["DefaultOfficeCode"] = ((Employee)Session["Employee"]).Department1.OfficeCode;
            ViewData["DefaultOfficeName"] = new OfficeDao(db).GetByKey(ViewData["DefaultOfficeCode"].ToString()).OfficeName;

            //Mod 2016/12/01 arc nakayama #3675_入金実績リスト　検索条件と検索結果に下取が表示されてしまう。
            List<Journal> list = GetSearchResultList(form).Where(x => !x.AccountType.Equals("013")).AsQueryable().ToList();

            // 検索条件部の画面表示データ取得
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }

            ViewData["CondDepartmentCode"] = form["CondDepartmentCode"];
            if (!string.IsNullOrEmpty(form["CondDepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["CondDepartmentCode"]);
                if (department != null) {
                    ViewData["CondDepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["CondJournalTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetJournalTypeAll(false), form["CondJournalType"], true);
            ViewData["CondYearMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetYearMonthsList(), form["CondYearMonth"], true);
            ViewData["CondSlipNumber"] = form["CondSlipNumber"];
            ViewData["CondDay"] = form["CondDay"];
            ViewData["CondAccountCode"] = form["CondAccountCode"];
            if (!string.IsNullOrEmpty(form["CondAccountCode"])) {
                Account account = new AccountDao(db).GetByKey(form["CondAccountCode"]);
                if (account != null) {
                    ViewData["CondAccountName"] = account.AccountName;
                }
            }
            ViewData["CondSummary"] = form["CondSummary"];

            List<CodeData> dayList = new List<CodeData>();
            for (int i = 1; i <= 31; i++) {
                dayList.Add(new CodeData() { Code = i.ToString(), Name = i.ToString() });
            }

            ViewData["CondDayList"] = CodeUtils.GetSelectList(dayList, form["CondDay"], true);
            CodeDao dao = new CodeDao(db);
            //Mod 2016/12/01 arc nakayama #3675_入金実績リスト　検索条件と検索結果に下取が表示されてしまう。
            ViewData["AccountType"] = CodeUtils.GetSelectListByModel(dao.GetAccountTypeAll(true), form["AccountType"], true).Where(x => !x.Value.Equals("013"));
            List<CashAccount> cashAccountList = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
            List<CodeData> accountDataList = new List<CodeData>();
            foreach (var a in cashAccountList) {
                CodeData data = new CodeData();
                data.Code = a.CashAccountCode;
                data.Name = a.CashAccountName;
                accountDataList.Add(data);
            }
            ViewData["CashAccountList"] = CodeUtils.GetSelectListByModel(accountDataList, form["CashAccountCode"], true);

            return View("JournalCriteria",list);
        }
        #endregion

        #region 入金実績修正画面表示
        /// <summary>
        /// 入金実績修正画面表示
        /// </summary>
        /// <param name="id">入金実績ID</param>
        /// <returns></returns>
        public ActionResult JournalEdit(string id) {
            Journal journal = new JournalDao(db).GetByKey(new Guid(id));
            // 初回開くときだけの判断
            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);

            // 伝票日付が締日よりも前だったら締め処理済み扱い（現金）
            if (journal.AccountType != null && journal.AccountType.Equals("001") && cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate) {
                journal.IsClosed = true;
            }
 
            // Mod 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる  仮締め時のデータ編集権限の有無で分ける（経理か、それ以外か）
            //ログインユーザ情報取得
            Employee loginUser = ((Employee)Session["Employee"]);
            //loginUser.SecurityRoleCode = new EmployeeDao(db).GetByKey(loginUser.EmployeeCode).SecurityRoleCode;
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //仮締データ編集権限があるかないか

            //仮締データ編集権限があれば本締めの時のみエラー
            if (AppRole.EnableFlag){
                //仮締め時の操作権限があった場合は、本締め時だけNGにする
                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001")){
                    journal.IsClosed = true;
                }
            }else{
                // そうでなければ仮締め以上でエラー
                // Add 2014/04/17 arc nakayama #3096 締め処理後でも店舗入金消込が個別だと可能となる 全体での消込も個別消込も経理締めを見るようにする
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001")){
                    journal.IsClosed = true;
                }
            }
            SetEditViewData(journal);
            return View("JournalEdit", journal);
        }
        #endregion

        /// <summary>
        /// 入金実績修正処理
        /// </summary>
        /// <param name="journal">入金実績モデルデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/22 yano #3930 入金実績リスト　マイナスの入金予定ができている状態で実績削除した場合の残高不正
        /// 2016/05/17 arc nakayama #3551_伝票番号を変更した時の処理
        /// 2014/08/12 arc amii エラーログ対応 登録用にDataContextを設定する
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult JournalEdit(Journal journal) {
            journal = ValidateJournal(journal);
            if (!ModelState.IsValid) {
                SetEditViewData(journal);
                return View("JournalEdit", journal);
            }

            ModelState.Clear();

            // Add 2014/08/12 arc amii 
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope())    //Add arc nakayama #355
            {
                Journal target = new JournalDao(db).GetByKey(journal.JournalId);

                //Del 2018/08/22 yano 古いコードの削除
                
                //Add 2016/05/17 arc nakayama #3551
                //変更前の伝票番号と請求先を取得退避
                string PrevSlipNumber = target.SlipNumber;
                string PrevCustomerClaimCode = target.CustomerClaimCode;
                decimal PrevAmount = target.Amount;

                UpdateModel(target);

                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;


                //一度入金実績の修正内容を反映しておく
                db.SubmitChanges();                     //Add 2018/08/22 yano #3930

                //Add 2016/05/20 arc nakayama #3551 入金実績を削除したときに伝票番号が入力されていたら紐づく入金予定の残高をもとに戻す
                if (target.DelFlag.Equals("1"))
                {
                    //------------
                    //削除処理
                    //------------

                    //伝票番号が登録されている場合⇒入金予定の残高をもとに戻す
                    //カード会社からの入金の場合⇒カード会社からの入金予定の残高をもとに戻す（１つの入金予定に対してカードで複数回入金できるため別処理）
                    if (!string.IsNullOrEmpty(target.SlipNumber))
                    {
                        if (target.AccountType.Equals("011"))
                        {
                            //カード会社からの入金の場合 カード会社からの入金予定を検索
                            ReceiptPlan CreditPlanData = new ReceiptPlanDao(db).GetByStringKey(target.CreditReceiptPlanId);
                            if (CreditPlanData != null)
                            {
                                //入金額を残高に加算（元に戻す）
                                CreditPlanData.ReceivableBalance += target.Amount;
                                if (CreditPlanData.ReceivableBalance.Equals(0m))
                                {
                                    CreditPlanData.CompleteFlag = "1";
                                }
                                else
                                {
                                    CreditPlanData.CompleteFlag = "0";
                                }

                                CreditPlanData.LastUpdateDate = DateTime.Now;
                                CreditPlanData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            }
                        }
                        else
                        {
                            //Mod 2018/08/22 yano #3930 入金予定を再作成する
                            // 車両伝票
                            CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                            if (carSalesHeader != null)
                            {
                                CreateCarReceiptPlan(carSalesHeader, target);
                            }
                            // サービス伝票
                            ServiceSalesHeader serviceSalesheader = new ServiceSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                            if (serviceSalesheader != null)
                            {
                                CreateServiceReceiptPlan(serviceSalesheader, target, PrevSlipNumber, PrevCustomerClaimCode, PrevAmount, false);
                            }
                            if (!ModelState.IsValid)
                            {
                                SetEditViewData(target);
                                return View("JournalEdit", target);
                            }

                            ////入金種別が「カード会社からの入金」でなければ、伝票番号と請求先でヒットした入金予定の残高をもとに戻す
                            ////カードだった場合は、カード会社からの入金予定も削除する
                            //List<ReceiptPlan> BackPlanData = new ReceiptPlanDao(db).GetListByCustomerClaimDesc(target.SlipNumber, target.CustomerClaimCode);
                            ////Mod 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮 入金予定額が多い順に返金をする
                            //RePayment(BackPlanData, target.Amount);


                            //カード会社からの入金予定削除
                            if (target.AccountType.Equals("003"))
                            {
                                ReceiptPlan DelPlan = new ReceiptPlanDao(db).GetByStringKey(target.CreditReceiptPlanId);
                                if (DelPlan != null)
                                {
                                    DelPlan.DelFlag = "1";
                                    DelPlan.LastUpdateDate = DateTime.Now;
                                    DelPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //------------
                    //保存処理
                    //------------
                    //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                    if (!string.IsNullOrEmpty(target.SlipNumber))
                    {
                        //Add arc nakayama #3551
                        //伝票番号が入っていた場合は、消込済実績に編集するはずなので、ReceiptPlanFlag = 0 → 1に変更する。
                        if (string.IsNullOrWhiteSpace(journal.ReceiptPlanFlag) || journal.ReceiptPlanFlag.Equals("0"))
                        {
                            target.ReceiptPlanFlag = "1";
                        }

                        // 車両伝票
                        CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                        if (carSalesHeader != null)
                        {
                            CreateCarReceiptPlan(carSalesHeader, target);
                        }
                        // サービス伝票
                        ServiceSalesHeader serviceSalesheader = new ServiceSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                        if (serviceSalesheader != null)
                        {
                            CreateServiceReceiptPlan(serviceSalesheader, target, PrevSlipNumber, PrevCustomerClaimCode, PrevAmount, false);
                        }
                        if (!ModelState.IsValid)
                        {
                            SetEditViewData(target);
                            return View("JournalEdit", target);
                        }
                    }
                }

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
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
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
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
                            OutputLogger.NLogError(se, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetEditViewData(target);
                            return View("JournalEdit", target);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                        return View("Error");
                    }
                }

                journal = target;    //Add arc nakayama #3551
            }
            ViewData["close"] = "1";
            SetEditViewData(journal);
            return View("JournalEdit", journal);
        }

        #region  経理権限のチェック
        //Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 
        /// <summary>
        /// 権限のチェック
        /// </summary>
        /// <param name="EmployeeCode">社員コード</param>
        /// <param name="ApplicationCode">アプリケーション名(権限名)</param>
        /// <returns>指定した権限をもっていれば:True  それ以外:False</returns>
        public bool CheckApplicationRole(string EmployeeCode, string ApplicationCode)
        {
            //ログインユーザ情報取得
            Employee loginUser = new EmployeeDao(db).GetByKey(EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, ApplicationCode); //権限があるかないか

            // 経理権限があればtrueそうでなければfalse
            if (AppRole.EnableFlag)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 入金予定額の少ない順に入金を行う
        /// <summary>
        /// 入金予定額の少ない順に入金を行う（入金予定リスト, 入金額）
        /// </summary>
        /// <param name="ReceiptPlanList">入金予定リスト</param>
        /// <param name="JournalAmount">入金額</param>
        /// <returns></returns>
        /// <history>Add 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮</history>
        private void Payment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount; //入金額
            foreach (var plan in ReceiptPlanList)
            {
                if ((plan.ReceivableBalance ?? 0m) - remainAmount > 0)
                {
                    plan.ReceivableBalance = (plan.ReceivableBalance ?? 0m) - remainAmount;
                    remainAmount = 0m;
                }
                else
                {
                    remainAmount = remainAmount - (plan.ReceivableBalance ?? 0m);
                    plan.ReceivableBalance = 0;
                    plan.CompleteFlag = "1";
                }

                plan.LastUpdateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                if (remainAmount.Equals(0m))
                {
                    break;
                }
            }
            
        }
        #endregion

        #region 入金予定額の多い順に返金処理をする
        /// <summary>
        /// 入金予定額の多い順に返金処理をする（入金予定リスト, 入金額）
        /// </summary>
        /// <param name="ReceiptPlanList">入金予定リスト</param>
        /// <param name="JournalAmount">入金額</param>
        /// <returns></returns>
        /// <history>Add 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮</history>
        private void RePayment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount;
            decimal BackBlance = 0m; // 返金できる金額　（入金予定額 - 入金残高 = 返金できる金額）

            foreach (var plan in ReceiptPlanList)
            {
                BackBlance = 0m; //初期化

                if ((plan.ReceivableBalance ?? 0m) + remainAmount <= plan.Amount)
                {
                    plan.ReceivableBalance = (plan.ReceivableBalance ?? 0m) + remainAmount;
                    remainAmount = 0m;
                }
                else
                {
                    //返金できる金額を求める
                    BackBlance = (plan.Amount ?? 0m) - (plan.ReceivableBalance ?? 0m);

                    //返金できる
                    if (BackBlance != 0m)
                    {
                        plan.ReceivableBalance += BackBlance;
                        remainAmount = remainAmount - BackBlance;
                    }
                }

                //入金完了フラグ更新
                if (plan.ReceivableBalance == 0m)
                {
                    plan.CompleteFlag = "1";
                }
                else
                {
                    plan.CompleteFlag = "0";
                }
                plan.LastUpdateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                if (remainAmount <= 0m)
                {
                    break;
                }
            }
        }
        #endregion

        #endregion
    }
}
