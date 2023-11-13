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
using Crms.Models;                      //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �����o�[���@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CashJournalController : Controller {

        #region ������
        //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����o�[��";   // ��ʖ�
        private static readonly string PROC_NAME = "�����ݍ��o�^"; // ������
        private static readonly string PROC_NAME_UPD_JSK = "�������яC��"; // ������
        private static readonly string PROC_NAME_SHIME = "����"; // ������
        private static readonly string PROC_NAME_SUITO = "�����o�[�����דo�^"; // ������

        //Add 2018/11/14 yano #3814
        private static readonly string STATUS_CAR_CANCEL = "006";          //�L�����Z��(�ԗ��`�[�X�e�[�^�X)
        private static readonly string STATUS_CAR_ORDERCANCEL = "007";     //�󒍌�L�����Z��(�ԗ��`�[�X�e�[�^�X)
        private static readonly string STATUS_SERVICE_CANCEL = "007";      //�L�����Z��(�T�[�r�X�`�[�X�e�[�^�X)
        private static readonly string JOURNALTYPE_PAYMENT = "001";        //����(�������)


        //Add 2018/08/22 yano #3930
        private static readonly List<string> excludeList = new List<string>() { "003" }; //������^�C�v = �N���W�b�g
        private static readonly List<string> excluedAccountTypetList = new List<string>() { "012", "013" }; //�����^�C�v = �c��, ����

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CashJournalController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion


        #region ������ʕ\��
        /// <summary>
        /// �����o�[��������ʕ\��
        /// </summary>
        /// <returns>�����o�[���������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            Journal journal = new Journal();
            journal.JournalDate = DateTime.Now.Date;
            return Criteria(journal, new FormCollection());
        }

        /// <summary>
        /// �����o�[��������ʕ\��
        /// </summary>
        /// <param name="journal">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�����o�[���������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(Journal journal, FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
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

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            #region ���ߏ���
            if (form["action"].Equals("closeAccount")) {
                //���ߑΏۓ�
                DateTime targetDate = DateTime.Parse(form["selectedDate"] ?? DateTime.Today.ToString());

                //�O�������߂��܂��̏ꍇ�A�G���[
                if (targetDate > DaoConst.FIRST_TARGET_DATE) {

                    CashBalance lastMonthBalance = GetTodayBalanceData(form["OfficeCode"],form["CashAccountCode"], new DateTime(targetDate.Year,targetDate.Month,1).AddDays(-1));
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�

                    if (lastMonthBalance == null || !CommonUtils.DefaultString(lastMonthBalance.CloseFlag).Equals("1"))
                    {
                        ModelState.AddModelError("", "�O�������ߏ������������Ă��Ȃ����ߎw����ł͒��ߏ����ł��܂���");

                        // �������ʃ��X�g�̎擾
                        list = GetSearchResultList(form);

                        // ���̑��o�͍��ڂ̐ݒ�
                        GetCriteriaViewData(journal, form);

                        // �����o�[��������ʂ̕\��
                        return View("CashJournalCriteria", list);
                    }
                }

                using (TransactionScope ts = new TransactionScope()) {                 

                    // �{�����ߏ��(=���͏��)�̎擾
                    CashBalance cashBalance = GetTodayBalanceData(form["OfficeCode"],form["CashAccountCode"],targetDate);

                    // ���߂��Ă��Ȃ��ꍇ
                    if (cashBalance != null && CommonUtils.DefaultString(cashBalance.CloseFlag).Equals("0")) {

                        // ���߂̒��ߏ��擾
                        CashBalance cashBalancePrev = GetLatestClosedData(form["OfficeCode"],form["CashAccountCode"]);
                        decimal totalAmountPrev = (cashBalancePrev == null ? 0m : cashBalancePrev.TotalAmount ?? 0m);

                        // �������׋��z���v�̎擾
                        decimal detailsTotal = GetDetailsTotal(
                            form["OfficeCode"],
                            form["CashAccountCode"],
                            (cashBalancePrev == null ? DaoConst.SQL_DATETIME_MIN : cashBalancePrev.ClosedDate.AddDays(1)),
                            targetDate);

                        // �����ݍ��̏ƍ�
                        if (decimal.Add(totalAmountPrev, detailsTotal).Equals(cashBalance.TotalAmount)) {

                            // ���ߏ���
                            CloseAccount(cashBalance);
                        }
                    }

                    //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                    {
                        try
                        {
                            db.SubmitChanges();
                            // �g�����U�N�V�����̃R�~�b�g
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
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ���O�ɏo��
                                OutputLogger.NLogFatal(cfe, PROC_NAME_SHIME, FORM_NAME, "");
                                // �G���[�y�[�W�ɑJ��
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_SHIME, FORM_NAME, "");
                            return View("Error");
                        }
                    } 
                }
            #endregion

            #region �����o�[�����דo�^����
            }
            else if (form["action"].Equals("registDetail"))
            {

                // �f�[�^�`�F�b�N
                ValidateJournal(journal);
                //journal.CashAccountCode = form["CashAccountCode"];
                //journal.OfficeCode = form["OfficeCode"];

                // �f�[�^�o�^����
                if (ModelState.IsValid)
                {
                    //�����o�[�����דo�^����
                    InsertJournal(journal);
                    //Add 2014/08/12 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                    try
                    {
                        db.SubmitChanges();
                    }
                    catch (SqlException e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogError(e, PROC_NAME_SUITO, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�o�^"));
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_SUITO, FORM_NAME, "");
                            return View("Error");
                        }
                    }

                    form["CondDay"] = journal.JournalDate.Day.ToString();
                    form["CondYearMonth"] = string.Format("{0:yyyy/MM}", journal.JournalDate);
                }

                // ���f���̏�����
                if (ModelState.IsValid)
                {
                    ModelState.Clear();
                    journal = new Journal();
                    journal.JournalDate = DateTime.Now.Date;
                }
            #endregion

            #region �X�ܒ��ߏ�������
            }
            else if (form["action"].Equals("ReleaseAccount"))
            {
                //Add 2016/05/18 arc nakayama #3536_�����o�[���@�X�ܒ��߉��������{�^���̒ǉ� �X�ܒ��߉��������ǉ�
                //���ߑΏۓ�
                DateTime ReleasetargetDate = DateTime.Parse(form["PreviousClosedDate"]);

                using (TransactionScope ts = new TransactionScope())
                {
                    // ���߂̓X�ܒ��ߏ����f�[�^�擾
                    CashBalance PrevcashBalance = GetTodayBalanceData(form["OfficeCode"], form["CashAccountCode"], ReleasetargetDate);

                    //�X�ܒ��߉�������
                    ReleaseAccount(PrevcashBalance);

                    //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                    {
                        try
                        {
                            db.SubmitChanges();
                            // �g�����U�N�V�����̃R�~�b�g
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
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ���O�ɏo��
                                OutputLogger.NLogFatal(cfe, PROC_NAME_SHIME, FORM_NAME, "");
                                // �G���[�y�[�W�ɑJ��
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_SHIME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                }
            }
            #endregion

            // �������ʃ��X�g�̎擾
            list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            GetCriteriaViewData(journal, form);

            if (ModelState.IsValid) {
                ModelState.Clear();
            }

            // �����o�[��������ʂ̕\��
            return View("CashJournalCriteria", list);
        }

        /// <summary>
        /// �����o�[�����דo�^����
        /// </summary>
        /// <param name="journal">�o�[�����f���f�[�^</param>
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

        #region ��ʃf�[�^�擾
        /// <summary>
        /// ������ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="journal">���f���f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void GetCriteriaViewData(Journal journal, FormCollection form) {

            CodeDao dao = new CodeDao(db);
            DateTime selectedDate = DateTime.Parse(form["selectedDate"] != null && !form["selectedDate"].Equals("") ? form["selectedDate"] : DateTime.Today.ToString());

            // �����������̉�ʕ\���f�[�^�擾
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

            // ���ߏ������̉�ʕ\���f�[�^�y�я���������擾
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
                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        if (CommonUtils.DefaultString(cashBalancePrev.CloseFlag).Equals("1") && cashBalancePrev.ClosedDate.Equals(selectedDate))
                        {
                            ViewData["AlreadyClosed"] = "1";
                        }
                    }

                    //���t�ړ����X�g�쐬                                       
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

            // �o�[�����דo�^���̉�ʕ\���f�[�^�擾
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

            //Add 2016/06/17 arc nakayama #3536_�����o�[���@�X�ܒ��߉��������{�^���̒ǉ�
            //�o�������̗L��
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
        /// ���Ə����쌠���`�F�b�N
        /// </summary>
        /// <param name="officeCode">���Ə��R�[�h</param>
        /// <returns>�`�F�b�N����(True:�������� False:�����Ȃ�)</returns>
        private bool CheckOfficeAuth(string officeCode) {

            Office officeCondition = new Office();
            officeCondition.SetAuthCondition((Employee)Session["Employee"]);
            officeCondition.OfficeCode = officeCode;

            return (new OfficeDao(db).GetByKey(officeCondition) != null);
        }

        /// <summary>
        /// �����o�[���������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�����o�[���������ʃ��X�g</returns>
        private List<Journal> GetSearchResultList(FormCollection form) {

            JournalDao journalDao = new JournalDao(db);
            Journal journalCondition = new Journal();
            journalCondition.SlipNumber = form["CondSlipNumber"];

            // �`�[�ԍ����w�肳�ꂽ�瑼�̌��������͖���
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
        /// �����o�[�����̓`�F�b�N
        /// </summary>
        /// <param name="journal">�����o�[���f�[�^</param>
        /// <returns>�����o�[���f�[�^</returns>
        /// <history>
        /// 2019/02/14 yano #3972 �����o�[���@�o���f�[�^�o�^���ɓ`�[�ԍ��݂̂ŕR�Â��邱�Ƃ��ł��Ȃ��B
        /// 2018/04/04 arc yano #3764 �����o�[���ҏW�@�`�[���t�����͎��̕ۑ��̕s�
        /// </history>
        private Journal ValidateJournal(Journal journal) {

            // �K�{�`�F�b�N(���l/���t���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(journal.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "�v�㕔��"));
            }
            if (string.IsNullOrEmpty(journal.OfficeCode)) {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "�������Ə�"));
            }

            if (!ModelState.IsValidField("JournalDate")) {
                ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0003", "�`�[���t"));
                if (ModelState["JournalDate"].Errors.Count > 1) {
                    ModelState["JournalDate"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(journal.AccountCode)) {
                ModelState.AddModelError("AccountCode", MessageUtils.GetMessage("E0001", "�ȖڃR�[�h"));
            }
            if (string.IsNullOrEmpty(journal.JournalType)) {
                ModelState.AddModelError("JournalType", MessageUtils.GetMessage("E0001", "���o���敪"));
            }
            if (!ModelState.IsValidField("Amount")) {
                ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" }));
                if (ModelState["Amount"].Errors.Count > 1) {
                    ModelState["Amount"].Errors.RemoveAt(0);
                }
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("Amount")) {
                if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$")) {
                    ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" }));
                }
            }

            // �l�`�F�b�N
            if (ModelState.IsValidField("JournalDate")) {
                if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0) {
                    ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "�`�[���t", "�{���ȑO" }));
                }
                // 2012.02.23 �����ȊO�͋���
                if (!journal.IsClosed && (journal.AccountType==null || journal.AccountType.Equals("001"))) {
                    CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);
                    DateTime latestClosedDate = (cashBalance == null ? DaoConst.SQL_DATETIME_MIN : cashBalance.ClosedDate);
                    if (journal.JournalDate.CompareTo(latestClosedDate) <= 0) {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "�`�[���t", "�O����ߓ�����" }));
                    }
                }
            }
            // Add 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ�  �����ߎ��̃f�[�^�ҏW�����̗L���ŕ�����i�o�����A����ȊO���j
            //���O�C�����[�U���擾
            Employee loginUser = ((Employee)Session["Employee"]);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //�����f�[�^�ҏW���������邩�Ȃ���


            //Add 2018 2018/04/04 arc yano #3764 �K�{�`�F�b�N�Ɉ���������Ȃ��ꍇ�̂݃`�F�b�N
            if (ModelState.IsValidField("JournalDate"))
            {
                //�����f�[�^�ҏW����������Ζ{���߂̎��̂݃G���[
                if (AppRole.EnableFlag)
                {
                    //�����ߎ��̑��쌠�����������ꍇ�́A�{���ߎ�����NG�ɂ���
                    if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001"))
                    {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "�`�[���t", "�O��̌������ߏ��������s���ꂽ�N������" }));
                    }
                }
                else
                {
                    // �����łȂ���Ή����߈ȏ�ŃG���[
                    // Add 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ� �S�̂ł̏������ʏ������o�����߂�����悤�ɂ���
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001"))
                    {
                        ModelState.AddModelError("JournalDate", MessageUtils.GetMessage("E0013", new string[] { "�`�[���t", "�O��̌������ߏ��������s���ꂽ�N������" }));
                    }
                }
            }

            if (ModelState.IsValidField("Amount")) {
                if ((journal.DelFlag==null || !journal.DelFlag.Equals("1")) && journal.Amount.Equals(0m)) {
                    ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" }));
                }
            }

            //-------------------
            // �`�[�ԍ�
            //-------------------
            if (!string.IsNullOrEmpty(journal.SlipNumber)) {
                if (!new CarSalesOrderDao(db).IsExistSlipNumber(journal.SlipNumber)) {
                    ModelState.AddModelError("SlipNumber", "�w�肳�ꂽ�`�[�ԍ��͑��݂��܂���");
                }
                else if (string.IsNullOrEmpty(journal.CustomerClaimCode) && (!string.IsNullOrWhiteSpace(journal.JournalType) && journal.JournalType.Equals(JOURNALTYPE_PAYMENT))) //Mod 2019/02/14 yano #3972
                {    
                    ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0007", new string[] { "�`�[�ԍ��w��", "������" }));
                }
            }
            // Add 2016/05/16 arc nakayama #3548_������ʂ��u�J�[�h��Ђ���̓����v�������ꍇ�̃o���f�[�V�����ǉ�
            //�J�[�h��Ђ���̓����������ꍇ�̃o���f�[�V����
            if (journal.AccountType == "011")
            {
                //������`�F�b�N ������ʂ��u�N���W�b�g(003)�v�łȂ��ꍇ�G���[
                CustomerClaim CustomerClaimData = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                if (CustomerClaimData != null)
                {
                    if (CustomerClaimData.CustomerClaimType != "003")
                    {
                        ModelState.AddModelError("CustomerClaimCode", "�J�[�h��Ђ���̓������s���ꍇ�A������̐�����ʂ��N���W�b�g�łȂ��Ƃ����܂���B");

                    }
                }

                //�������z = �����z�@�łȂ���΃G���[
                ReceiptPlan ReceiptData = new ReceiptPlanDao(db).GetByStringKey(journal.CreditReceiptPlanId);
                if(ReceiptData != null){

                    if (ReceiptData.Amount != journal.Amount)
                    {
                        ModelState.AddModelError("Amount", "�J�[�h��Ђ���̓������s���ꍇ�́A�������z�Ɠ������z�łȂ��Ə������߂܂���B");
                    }
                }
            }

            //Add 2016/05/16 arc nakayama #3544_������ʂ��J�[�h�E���[���ɂ͕ύX�����Ȃ� ������ʂ��u�J�[�h�v�̓������т��폜���ꂽ�ꍇ�̃o���f�[�V����
            if (journal.DelFlag == "1" && journal.AccountType == "003")
            {
                //�J�[�h��Ђ���̓����\�������
                ReceiptPlan PlanData = new ReceiptPlanDao(db).GetByStringKey(journal.CreditReceiptPlanId);
                if (PlanData != null)
                {
                    Journal CreditJournal = new JournalDao(db).GetByStringKey(PlanData.CreditJournalId);
                    //�J�[�h��Ђ���̓������т����݂�����G���[
                    if (CreditJournal != null)
                    {
                        ModelState.AddModelError("", "�J�[�h��Ђ���̓��������ɍs���Ă��邽�߁A�폜�ł��܂���B�J�[�h��Ђ���̓������т��폜���Ă���ēx���s���ĉ������B");
                    }
                }
            }
            return journal;
        }
        #endregion

        #endregion

        #region �����ݍ����͉��
        /// <summary>
        /// �����ݍ����͉�ʕ\��
        /// </summary>
        /// <param name="id">���Ə��R�[�h</param>
        /// <param name="account">���������R�[�h</param>
        /// <returns>�����ݍ����͉��</returns>
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

            // �o��
            return View("CashJournalEntry", cashBalance);
        }

        /// <summary>
        /// �����ݍ��ǉ��X�V
        /// </summary>
        /// <param name="cashBalance">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ݍ����͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CashBalance cashBalance, FormCollection form) {

            // ���f���f�[�^�Z�o�ݒ荀�ڂ̐ݒ�
            cashBalance.calculate();

            // �f�[�^�`�F�b�N
            ValidateCashBalance(cashBalance);
            if (!ModelState.IsValid) {
                return View("CashJournalEntry", cashBalance);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�o�^����
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

                //Add 2014/08/12 arc amii �G���[���O�Ή� SubmitChanges����{�� & ���O�o�ׂ͂̈�trycatch���ǉ�
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
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�o�^"));
                            return View("CashJournalEntry", cashBalance);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null,(string)null);
        }
        /// <summary>
        /// �����ݍ��e�[�u���ǉ�����
        /// </summary>
        /// <param name="cashBalance">�����ݍ��f�[�^(�o�^���e)</param>
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
        /// �����ݍ��e�[�u���X�V����
        /// </summary>
        /// <param name="targetCashBalance">�����ݍ��f�[�^(�X�V�O���e)</param>
        /// <param name="cashBalance">�����ݍ��f�[�^(�o�^���e)</param>
        private void UpdateCashBalance(CashBalance targetCashBalance, CashBalance cashBalance) {

            UpdateModel(targetCashBalance);
            targetCashBalance.calculate();
            targetCashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            targetCashBalance.LastUpdateDate = DateTime.Now;
        }

        #region Validation
        /// <summary>
        /// �����ݍ����̓`�F�b�N
        /// </summary>
        /// <param name="cashBalance">�����ݍ��f�[�^</param>
        /// <returns>�����o�[���f�[�^</returns>
        private CashBalance ValidateCashBalance(CashBalance cashBalance) {

            // �����`�F�b�N
            if (!ModelState.IsValidField("NumberOf10000")) {
                ModelState.AddModelError("NumberOf10000", MessageUtils.GetMessage("E0004", new string[] { "10,000�~�D����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf5000")) {
                ModelState.AddModelError("NumberOf5000", MessageUtils.GetMessage("E0004", new string[] { "5,000�~�D����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf2000")) {
                ModelState.AddModelError("NumberOf2000", MessageUtils.GetMessage("E0004", new string[] { "2,000�~�D����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf1000")) {
                ModelState.AddModelError("NumberOf1000", MessageUtils.GetMessage("E0004", new string[] { "1,000�~�D����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf500")) {
                ModelState.AddModelError("NumberOf500", MessageUtils.GetMessage("E0004", new string[] { "500�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf100")) {
                ModelState.AddModelError("NumberOf100", MessageUtils.GetMessage("E0004", new string[] { "100�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf50")) {
                ModelState.AddModelError("NumberOf50", MessageUtils.GetMessage("E0004", new string[] { "50�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf10")) {
                ModelState.AddModelError("NumberOf10", MessageUtils.GetMessage("E0004", new string[] { "10�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf5")) {
                ModelState.AddModelError("NumberOf5", MessageUtils.GetMessage("E0004", new string[] { "5�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NumberOf1")) {
                ModelState.AddModelError("NumberOf1", MessageUtils.GetMessage("E0004", new string[] { "1�~�ʖ���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("CheckAmount")) {
                ModelState.AddModelError("CheckAmount", MessageUtils.GetMessage("E0004", new string[] { "���؎蓙���z", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CheckAmount") && cashBalance.CheckAmount != null) {
                if (!Regex.IsMatch(cashBalance.CheckAmount.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CheckAmount", MessageUtils.GetMessage("E0004", new string[] { "���؎蓙���z", "����10���ȓ��̐����̂�" }));
                }
            }

            // �l�`�F�b�N
            if (ModelState.IsValidField("NumberOf10000") && cashBalance.NumberOf10000 != null) {
                if (cashBalance.NumberOf10000 < 0) {
                    ModelState.AddModelError("NumberOf10000", MessageUtils.GetMessage("E0004", new string[] { "10,000�~�D����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf5000") && cashBalance.NumberOf5000 != null) {
                if (cashBalance.NumberOf5000 < 0) {
                    ModelState.AddModelError("NumberOf5000", MessageUtils.GetMessage("E0004", new string[] { "5,000�~�D����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf2000") && cashBalance.NumberOf2000 != null) {
                if (cashBalance.NumberOf2000 < 0) {
                    ModelState.AddModelError("NumberOf2000", MessageUtils.GetMessage("E0004", new string[] { "2,000�~�D����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf1000") && cashBalance.NumberOf1000 != null) {
                if (cashBalance.NumberOf1000 < 0) {
                    ModelState.AddModelError("NumberOf1000", MessageUtils.GetMessage("E0004", new string[] { "1,000�~�D����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf500") && cashBalance.NumberOf500 != null) {
                if (cashBalance.NumberOf500 < 0) {
                    ModelState.AddModelError("NumberOf500", MessageUtils.GetMessage("E0004", new string[] { "500�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf100") && cashBalance.NumberOf100 != null) {
                if (cashBalance.NumberOf100 < 0) {
                    ModelState.AddModelError("NumberOf100", MessageUtils.GetMessage("E0004", new string[] { "100�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf50") && cashBalance.NumberOf50 != null) {
                if (cashBalance.NumberOf50 < 0) {
                    ModelState.AddModelError("NumberOf50", MessageUtils.GetMessage("E0004", new string[] { "50�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf10") && cashBalance.NumberOf10 != null) {
                if (cashBalance.NumberOf10 < 0) {
                    ModelState.AddModelError("NumberOf10", MessageUtils.GetMessage("E0004", new string[] { "10�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf5") && cashBalance.NumberOf5 != null) {
                if (cashBalance.NumberOf5 < 0) {
                    ModelState.AddModelError("NumberOf5", MessageUtils.GetMessage("E0004", new string[] { "5�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NumberOf1") && cashBalance.NumberOf1 != null) {
                if (cashBalance.NumberOf1 < 0) {
                    ModelState.AddModelError("NumberOf1", MessageUtils.GetMessage("E0004", new string[] { "1�~�ʖ���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("TotalAmount")) {
                if (decimal.Compare(cashBalance.TotalAmount ?? 0m, -9999999999m) < 0 || decimal.Compare(cashBalance.TotalAmount ?? 0m, 9999999999m) > 0) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0018", "���v���z"));
                }
            }

            return cashBalance;
        }
        #endregion

        #endregion

        #region �����o�[���ҏW�E�X�V
        /// <summary>
        /// �����o�[���̕ҏW��ʕ\��
        /// </summary>
        /// <param name="id">JournalId</param>
        /// <returns></returns>
        public ActionResult Edit(string id) {
            Journal journal = new JournalDao(db).GetByKey(new Guid(id));
            //CodeDao dao = new CodeDao(db);
            //ViewData["JournalTypeList"] = CodeUtils.GetSelectListByModel(dao.GetJournalTypeAll(false), journal.JournalType, true);
            SetEditViewData(journal);

            // ����J���Ƃ������̔��f
            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);
            // �`�[���t�����������O����������ߏ����ς݈���
            if (cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate) {
                journal.IsClosed = true;
            }
            return View("CashJournalEdit", journal);
        }

        /// <summary>
        /// �����o�[���f�[�^�̍X�V
        /// </summary>
        /// <param name="journal">Journal���f���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/22 yano #3930 �������у��X�g�@�}�C�i�X�̓����\�肪�ł��Ă����ԂŎ��э폜�����ꍇ�̎c���s��
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(Journal journal,FormCollection form) {
            journal = ValidateJournal(journal);

            //���������ۑ��̏ꍇ
            if (form["actionType"].Equals("Slip")) {
                //�`�[�ԍ��K�{
                if (string.IsNullOrEmpty(journal.SlipNumber)) {
                    ModelState.AddModelError("SlipNumber", MessageUtils.GetMessage("E0007", new string[] { "���������ۑ�", "�`�[�ԍ�" }));
                }

                //Mod 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��
                //�����̓����\��̂ݎ擾���Ă���悤�ɕύX
                List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(journal.SlipNumber, journal.CustomerClaimCode);
                if (planList == null || planList.Count == 0) {
                    ModelState.AddModelError("SlipNumber", "�w�肳�ꂽ�`�[�ԍ��ɊY����������\�肪����܂���");
                }
                if (planList != null && planList.Sum(x => x.ReceivableBalance) < journal.Amount) {
                    ModelState.AddModelError("Amount", "�����z�������\������������ߏ����ł��܂���");
                }
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (CommonUtils.DefaultString(journal.JournalType).Equals("002"))
                {
                    ModelState.AddModelError("JournalType", "���������ۑ��͓������������p�ł��܂���");
                }
            }

            if (!ModelState.IsValid) {
                SetEditViewData(journal);
                return View("CashJournalEdit", journal);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                Journal target = new JournalDao(db).GetByKey(journal.JournalId);

                //Add 2016/05/17 arc nakayama #3551_�`�[�ԍ���ύX�������̏���
                //�ύX�O�̓`�[�ԍ��Ɛ�������擾�ޔ�
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


                //Mod 2018/08/22 yano #3930     //�폜�E�X�V�����Ɋւ�炸�A�X�V���ꂽ���т����ɓ����\����č쐬����
                //��x�������т̏C�����e�𔽉f���Ă���
                db.SubmitChanges();
                
                //---------------------------
                //�����\��č쐬
                //---------------------------
                if (!string.IsNullOrEmpty(target.SlipNumber) && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                {
                    // �ԗ��`�[
                    CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                    if (carSalesHeader != null)
                    {
                        CreateCarReceiptPlan(carSalesHeader, journal);
                    }

                    // �T�[�r�X�`�[
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

                ////Add 2016/05/20 #3538_����p�ȊO�������ۑ����s����悤�ɂ���
                ////Mod 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l�� �폜���s�����ꍇ�́A�������Ɏc�������Ƃɖ߂� 
                //if (!string.IsNullOrEmpty(target.SlipNumber) && target.DelFlag.Equals("1") && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //{
                //    //------------
                //    //�폜����
                //    //------------
                //    List<ReceiptPlan> BackPlanDataList = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(target.SlipNumber, target.CustomerClaimCode);

                //    //���z�̑������ɕԋ����s��
                //    RePayment(BackPlanDataList, target.Amount);
                //}
                //else
                //{
                //    //Mod 2018/01/11 arc yano #3717
                //    if (!string.IsNullOrEmpty(target.SlipNumber) && target.ReceiptPlanFlag != null && target.ReceiptPlanFlag.Equals("1"))
                //    //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                //    {
                //        // �ԗ��`�[
                //        CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                //        if (carSalesHeader != null)
                //        {
                //            CreateCarReceiptPlan(carSalesHeader, journal);
                //        }

                //        // �T�[�r�X�`�[
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
                        // Add 2014/08/12 arc amii �G���[���O�Ή� �X�V���s���̃G���[���� + ���O�o�͏�����ǉ�
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetEditViewData(journal);
                            return View("CashJournalEdit", journal);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
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
        /// �����o�[���ҏW��ʃf�[�^�擾
        /// </summary>
        /// <history>
        /// 2018/10/30 yano #3943 ����������̓`�[�ԍ��̐�����ύX�Ή�
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
            //Mod 2015/10/27 arc nakayama #3286_�������яC����ʂŐ����悪�\������Ȃ� 
            //ViewData["CustomerClaimList"] = CodeUtils.GetSelectList(dao.GetCustomerClaimListByjournal(journal.JournalId), journal.CustomerClaimCode, true);

            ViewData["CustomerClaimList"] = CodeUtils.GetSelectList(dao.GetCustomerClaimListByjournalReceiptPlan(journal), journal.CustomerClaimCode, true);    //Mod 2018/12/28 yano #3970


            //Add 2016/05/19 arc nakayama #3544_������ʂ��J�[�h�E���[���ɂ͕ύX�����Ȃ� �o���Ҍ����̗L�����擾

            bool Accounting = CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode, "Accounting");

            c_AccountType AccountTypeRet = new CodeDao(db).GetAccountTypeByKey(journal.AccountType, false);

            ViewData["AccountingFilter"] = false;
            if (AccountTypeRet != null)
            {
                //�o���������Ȃ��ƕҏW�ł��Ȃ�������ʁA���A�o���������Ȃ��@���A�J�[�h�܂��̓��[���łȂ��ꍇ�ҏW�s�̃t�B���^�[�������� 
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

        #region ���ߏ���
        /// <summary>
        /// �{�����ߏ��(=���͏��)�擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <returns>�{�����ߏ��(=���͏��)</returns>
        private CashBalance GetTodayBalanceData(string officeCode, string cashAccountCode, DateTime targetDate) {

            CashBalance cashBalanceCondition = new CashBalance();
            cashBalanceCondition.SetAuthCondition((Employee)Session["Employee"]);
            cashBalanceCondition.OfficeCode = officeCode;
            cashBalanceCondition.CashAccountCode = cashAccountCode;
            cashBalanceCondition.ClosedDate = targetDate;

            return new CashBalanceDao(db).GetByKey(cashBalanceCondition);
        }

        /// <summary>
        /// ���߂̒��ߏ��擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <returns>���߂̒��ߏ��</returns>
        private CashBalance GetLatestClosedData(string officeCode,string cashAccountCode) {

            CashBalance cashBalanceCondition = new CashBalance();
            cashBalanceCondition.SetAuthCondition((Employee)Session["Employee"]);
            cashBalanceCondition.OfficeCode = officeCode;
            cashBalanceCondition.CashAccountCode = cashAccountCode;

            return new CashBalanceDao(db).GetLatestClosedData(cashBalanceCondition);
        }

        /// <summary>
        /// ���������o�[�����׋��z���v�擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="termFrom">�����</param>
        /// <param name="termTo">������</param>
        /// <returns>�����o�[�����׋��z���v</returns>
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
        /// ���ߏ���
        /// </summary>
        /// <param name="cashBalance">�����ݍ����f���f�[�^</param>
        private void CloseAccount(CashBalance cashBalance) {

            cashBalance.CloseFlag = "1";
            cashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.LastUpdateDate = DateTime.Now;
        }

        /// <summary>
        /// ���߉�������
        /// </summary>
        /// <param name="cashBalance">�����ݍ����f���f�[�^</param>
        /// <history>Add 2016/05/18 arc nakayama #3536_�����o�[���@�X�ܒ��߉��������{�^���̒ǉ� �X�ܒ��߉��������ǉ�</history>
        private void ReleaseAccount(CashBalance cashBalance)
        {

            cashBalance.CloseFlag = "0";
            cashBalance.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            cashBalance.LastUpdateDate = DateTime.Now;
        }
        #endregion

        #region �����\����č쐬

        #region �ԗ��`�[�ɂ����������\��č쐬
        /// <summary>
        ///  �ԗ��`�[�ɂ����������\��č쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�w�b�_</param>
        /// <param name="journal">��������</param>
        /// <history>
        /// 2018/11/14 yano #3814 �󒍌�L�����Z����̓������яC���ɂ������\��쐬�s��΍�
        /// 2018/08/22 yano #3930 �������у��X�g�@�}�C�i�X�̓����\�肪�ł��Ă����ԂŎ��э폜�����ꍇ�̎c���s��
        /// 2017/09/25 arc yano  #3800 ����A�c�̓����\��̍č쐬�����̒ǉ�
        /// 2017/09/25 arc yano  #3798 �}�C�i�X�̓����\�̍č쐬���s����l�ɏC��
        /// </history>
        private void CreateCarReceiptPlan(CarSalesHeader header,Journal journal) {

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            //Mod 2018/11/14 yano #3814
            //�`�[�X�e�[�^�X=�L�����Z���A�܂��͎󒍌�L�����Z���̏ꍇ
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
        ///  �ʏ�X�e�[�^�X�̎ԗ��`�[�̓����\��č쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�w�b�_</param>
        /// <param name="journal">��������</param>
        /// <history>
        /// 2018/11/14 yano #3814 �󒍌�L�����Z����̓������яC���ɂ������\��쐬�s��΍�@�V�K�쐬
        /// </history>
        private void CreateNotCancelCarReceiptPlan(CarSalesHeader header, Journal journal, Account carAccount)
        {
            //�����̓����\����폜
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var d in delList)
            {
                //�c�A����ȊO�̓����\����폜
                if (!d.ReceiptType.Equals("012") && !d.ReceiptType.Equals("013"))       //Mod 2017/09/25 arc yano  #3800
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }

            //**�����̓����\���(��)�쐬����**
            //������A�����\����̏��ŕ��ёւ�
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //�����揇
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //�����\�����
                    (0);
            });

            //������ꗗ�쐬�p
            List<string> customerClaimList = new List<string>();


            //Mod 2017/09/25 arc yano  #3798
            // �������ъz
            decimal PlusjournalAmount = 0m;
            decimal MinusjournalAmount = 0m;
            for (int i = 0; i < payList.Count; i++)
            {
                //�����惊�X�g�ɒǉ�
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //�����悪�ς����������ς݋��z���i�āj�擾����
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, true);
                    MinusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, false);
                }

                // ���|�c
                decimal balanceAmount = 0m;

                //�����z���v���X�̏ꍇ
                if (payList[i].Amount >= 0)
                {

                    if (payList[i].Amount >= PlusjournalAmount)
                    {
                        // �\��z >= ���ъz
                        balanceAmount = ((payList[i].Amount ?? 0m) - PlusjournalAmount);
                        PlusjournalAmount = 0m;
                    }
                    else
                    {
                        // �\��z < ���ъz
                        balanceAmount = 0m;
                        PlusjournalAmount = PlusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = PlusjournalAmount * (-1);
                        }
                    }
                }
                else //�����z���}�C�i�X�̏ꍇ
                {
                    if (payList[i].Amount >= MinusjournalAmount)
                    {
                        // �\��z >= ���ъz
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = MinusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = MinusjournalAmount * (-1);
                        }

                    }
                    else
                    {
                        // �\��z < ���ъz
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
                plan.ReceiptType = "001"; // ����
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

            //���[���̓����\��
            decimal remainAmount = 0m; //�����ς݋��z��������

            //�v�������I������Ă���Ƃ���������
            if (!string.IsNullOrEmpty(header.PaymentPlanType))
            {
                //���[���R�[�h���擾����
                object loanCode = header.GetType().GetProperty("LoanCode" + header.PaymentPlanType).GetValue(header, null);

                //���[���R�[�h�͕K�{
                if (loanCode != null && !loanCode.Equals(""))
                {
                    Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());

                    //�}�X�^�ɑ��݂��邱��
                    if (loan != null)
                    {

                        //�����惊�X�g�ɒǉ�
                        customerClaimList.Add(loan.CustomerClaimCode);

                        //�����ς݋��z���擾
                        remainAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, loan.CustomerClaimCode);

                        //���[������
                        decimal planAmount = decimal.Parse(CommonUtils.GetModelProperty(header, "LoanPrincipal" + header.PaymentPlanType).ToString());

                        //�����c
                        decimal receivableBalance = planAmount - remainAmount;
                        if (receivableBalance < 0m)
                        {
                            //�����c���}�C�i�X�̏ꍇ�A�ߓ����ʒm
                            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                            task.CarOverReceive(header, loan.CustomerClaimCode, Math.Abs(receivableBalance));
                            //�c�̓[���Őݒ�
                            receivableBalance = 0m;
                        }

                        ReceiptPlan plan = new ReceiptPlan();
                        plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                        plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.DelFlag = "0";
                        plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�o���ɓ����\��
                        plan.LastUpdateDate = DateTime.Now;
                        plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.ReceiptPlanId = Guid.NewGuid();
                        plan.ReceiptType = "004"; //���[��
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
            //�����ς݂̐����悪����̓`�[����Ȃ��Ȃ��Ă�����̂�ʒm
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList, excluedAccountTypetList);
            foreach (Journal a in journalList)
            {
                if (customerClaimList.IndexOf(a.CustomerClaimCode) < 0)
                {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // �}�C�i�X�œ����\��쐬
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
                    plan.ReceiptType = "001"; // ����
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

            //����A�c�̓����\��̍č쐬
            CreateTradeReceiptPlan(header);      //Add 2017/09/25 arc yano #3800
        }

        /// <summary>
        ///  �L�����Z���A�󒍌�L�����Z���ԗ��`�[�̓����\��č쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�w�b�_</param>
        /// <param name="journal">��������</param>
        /// <history>
        /// 2018/11/14 yano #3814 �󒍌�L�����Z����̓������яC���ɂ������\��쐬�s��΍�
        /// </history>
        private void CreateCancelCarReceiptPlan(CarSalesHeader header, Journal journal)
        {
            //�����\����폜
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var p in planList)
            {
                p.DelFlag = "1";
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //�c�Ɖ���̓������т͍폜
            List<Journal> DelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("012") || x.AccountType.Equals("013"))).ToList();
            foreach (var d in DelJournal)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            db.SubmitChanges();

            //�����ς݂�����΃A���[�g
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            foreach (var j in journalList)
            {

                // �ߓ����̃A���[�g���쐬
                task.CarOverReceive(header, j.CustomerClaimCode, j.Amount);

                // ���ѕ��̓����\����쐬
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
                plusPlan.ReceivableBalance = 0;             //�c�� = 0(�Œ�)
                plusPlan.CompleteFlag = "1";                //���������t���O = 0(�Œ�)
                plusPlan.CreateDate = DateTime.Now;
                plusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.LastUpdateDate = DateTime.Now;
                plusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plusPlan.DelFlag = "0";
                plusPlan.JournalDate = DateTime.Today;
                plusPlan.DepositFlag = "0";
                db.ReceiptPlan.InsertOnSubmit(plusPlan);


                // �ߓ������̃}�C�i�X�����\����쐬
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
        /// ����ԂɊւ�������\��̍쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�w�b�_</param>
        /// <param name="journal">��������</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s����
        /// 2017/09/25 arc yano #3800 ����A�c�̓����\��̍쐬�����̒ǉ��@�V�K�쐬�i�ԗ��`�[�̏�����藬�p�j
        /// </history>
        private void CreateTradeReceiptPlan(CarSalesHeader header)
        {
            //����A�c�̊����̓����\��̍폜
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
            
            foreach (var d in delList)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
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
                    decimal JournalAmount = 0; //����̓����z
                    decimal JournalDebtAmount = 0; //�c�̓����z

                    //Mod 2017/11/14 arc yano #3811
                    //����̓����z�擾
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //�c�̓����z�擾
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    //����̓����\��쐬
                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013";                                  //����
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //���v�Z
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

                    //�c���������ꍇ�c���̓����\����}�C�i�X�ō쐬����
                    if (!string.IsNullOrEmpty(TradeInRemainDebt))
                    {
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�c�͌o���ɐU��ւ�
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012";                                                                 //�c��
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //�v�Z
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

        #region �T�[�r�X�`�[�ɂ����������\��č쐬

        /// <summary>
        /// �T�[�r�X�`�[�ɂ����������\��č쐬
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
        /// <param name="journal">��������</param>
        /// <param name="journal">�ύX�O�`�[�ԍ�</param>
        /// <param name="journal">�ύX�O������R�[�h</param>
        /// <param name="journal">�ύX�O���z</param>
        /// <history>
        /// 2018/11/14 yano #3814 �󒍌�L�����Z����̓������яC���ɂ������\��쐬�s��΍�
        /// 2018/08/22 yano #3930 �������у��X�g�@�}�C�i�X�̓����\�肪�ł��Ă����ԂŎ��э폜�����ꍇ�̎c���s��
        /// 2017/11/28 arc yano #3828 �������у��X�g�|�T�[�r�X�`�[�ŉߓ������ɓ����\��̎c�����}�C�i�X�ƂȂ�Ȃ�
        /// </history>
        private void CreateServiceReceiptPlan(ServiceSalesHeader header, Journal journal, string PrevSlipNumber, string PrevCustomerClaimCode, decimal PrevAmount, bool cashJournalFlag)
        {
            //Mod 2018/08/22 yano #3930
            IServiceSalesOrderService service = new ServiceSalesOrderService(db);

            //Mod 2018/11/14 yano #3814
            if (header.ServiceOrderStatus.Equals(STATUS_SERVICE_CANCEL))    //�`�[�X�e�[�^�X=�L�����Z��
            {
                CreateCancelServiceReceiptPlan(header);
            }
            else
            {
                //�����\��č쐬
                service.CreateReceiptPlan(header, header.ServiceSalesLine, header.ServiceSalesPayment.ToList());
            }

            /*
            //�Ȗڂ��擾
            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");
            if (serviceAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            //Add 2016/05/17 arc nakayama #3551_�`�[�ԍ���ύX�������̏���
            //Mod 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��
            //�ύX�O�ƕύX��œ`�[�ԍ��ɈႢ����������ύX�O�̎c�������Ƃɖ߂�
            //�ύX�O�̓`�[�ԍ��������͂̏ꍇ�͂Ȃɂ����Ȃ�
            if ((!string.IsNullOrEmpty(PrevSlipNumber) && journal.SlipNumber != PrevSlipNumber) || (journal.CustomerClaimCode != PrevCustomerClaimCode))
            {
                //�U��ւ����̓����\��擾�@�����o�[������X�V���ꂽ�ꍇ�͌����̓����\��̂ݎ擾
                List<ReceiptPlan> ReTargetPlan = new List<ReceiptPlan>();

                if (cashJournalFlag)
                {
                    ReTargetPlan = new ReceiptPlanDao(db).GetCashListByCustomerClaimDesc(PrevSlipNumber, PrevCustomerClaimCode);
                }
                else
                {
                    ReTargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaimDesc(PrevSlipNumber, PrevCustomerClaimCode);
                }
                //�U��ւ����̓����\��̎c�������Ƃɖ߂�
                RePayment(ReTargetPlan, PrevAmount);

                //�U��ւ���̓����\��擾�@�����o�[������X�V���ꂽ�ꍇ�͌����̓����\��̂ݎ擾
                List<ReceiptPlan> TargetPlan = new List<ReceiptPlan>();

                if (cashJournalFlag)
                {
                    TargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(journal.SlipNumber, journal.CustomerClaimCode);
                }
                else
                {
                    TargetPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(journal.SlipNumber, journal.CustomerClaimCode);
                }

                //�U��ւ���̎c���X�V
                Payment(TargetPlan, journal.Amount);
            }


            db.SubmitChanges();

            //�����̓����\����폜
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, header.DepartmentCode);
            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }


            //**�����̓����\���(��)�쐬����**
            //������A�����\����̏��ŕ��ёւ�
            List<ServiceSalesPayment> payList = header.ServiceSalesPayment.ToList();
            payList.Sort(delegate(ServiceSalesPayment x, ServiceSalesPayment y)
            {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //�����揇
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //�����\�����
                    (0);
            });

            //Add 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��
            //����ɋ��z�̏��Ȃ����ɕ��ёւ���
            payList = payList.OrderBy(a => a.CustomerClaimCode).ThenBy(b => b.Amount).ToList<ServiceSalesPayment>();


            //������ꗗ�쐬�p
            List<string> customerClaimList = new List<string>();

            //�o�^����Ă���s�������J��Ԃ�
            decimal remainAmount = 0m; //�����ς݋��z
            for (int i = 0; i < payList.Count; i++)
            {
                //Mod 2017/11/28 arc yano #3828 �v�Z���@���T�[�r�X�`�[�ɍ��킹��

                //�����惊�X�g�ɒǉ�
                customerClaimList.Add(payList[i].CustomerClaimCode);

                // ����or�����悪�ς�����Ƃ��A���������ς݋��z���i�āj�擾����
                if (i == 0 || (i > 0 && !payList[i - 1].CustomerClaimCode.Equals(payList[i].CustomerClaimCode)))
                {
                    // �N���W�b�g�E���[���ȊO�̓��������ς݋��z
                    remainAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, payList[i].CustomerClaimCode);
                }

                // ���|�c
                decimal balanceAmount = 0m;

                if (payList[i].Amount >= remainAmount)
                {
                    // �\��z >= ���ъz
                    balanceAmount = ((payList[i].Amount ?? 0m) - remainAmount);
                    remainAmount = 0m;
                }
                else
                {
                    // �\��z < ���ъz
                    balanceAmount = 0m;
                    remainAmount = remainAmount - (payList[i].Amount ?? 0m);

                    // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
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
        /// �X�e�[�^�X���L�����Z���̃T�[�r�X�`�[�̓����\��č쐬
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
        /// <history>
        /// 2018/11/14 yano #3814 �󒍌�L�����Z����̓������яC���ɂ������\��쐬�s��΍�
        /// </history>
        private void CreateCancelServiceReceiptPlan(ServiceSalesHeader header)
        {
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);

            //�����̓����\����폜
            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            foreach (var p in planList)
            {
                p.DelFlag = "1";
            }

            //Mod 2016/05/13 arc yano #3528
            //�����ς݂�����Ύ��ѕ��̃}�C�i�X�̓����\����쐬����
            if (journalList != null && journalList.Count() > 0)
            {
                //�}�C�i�X�̓����\��̍č쐬
                foreach (var j in journalList)
                {
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.DepartmentCode = j.DepartmentCode;
                    plan.OccurredDepartmentCode = j.DepartmentCode;
                    plan.CustomerClaimCode = j.CustomerClaimCode;
                    plan.SlipNumber = j.SlipNumber;
                    plan.ReceiptType = "001";                                                           //�u�����v�Œ�
                    plan.ReceiptPlanDate = j.JournalDate;
                    plan.AccountCode = j.AccountCode;
                    plan.Amount = 0;                                                                    //�u0�v�Œ�
                    plan.ReceivableBalance = j.Amount * (-1);
                    plan.CompleteFlag = "0";                                                            //�u0�v�Œ�
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.CreateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.LastUpdateDate = DateTime.Now;
                    plan.DelFlag = "0";                                                                 //�u0�v�Œ�
                    plan.Summary = j.Summary;
                    plan.JournalDate = null;
                    plan.DepositFlag = null;                                                            //����p�t���O = null
                    plan.PaymentKindCode = j.PaymentKindCode;
                    plan.CommissionRate = null;                                                         //�萔���� = null
                    plan.CommissionAmount = null;                                                       //�萔�� = null
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }
        }

        #endregion
        #endregion

        #region �������у��X�g
        public ActionResult JournalCriteria() {
            return JournalCriteria(new FormCollection());
        }

        #region �������у��X�g��������
        /// <summary>
        /// �������у��X�g��������
        /// </summary>
        /// <param name="form">�t�H�[�����͒l</param>
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

            //Mod 2016/12/01 arc nakayama #3675_�������у��X�g�@���������ƌ������ʂɉ��悪�\������Ă��܂��B
            List<Journal> list = GetSearchResultList(form).Where(x => !x.AccountType.Equals("013")).AsQueryable().ToList();

            // �����������̉�ʕ\���f�[�^�擾
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
            //Mod 2016/12/01 arc nakayama #3675_�������у��X�g�@���������ƌ������ʂɉ��悪�\������Ă��܂��B
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

        #region �������яC����ʕ\��
        /// <summary>
        /// �������яC����ʕ\��
        /// </summary>
        /// <param name="id">��������ID</param>
        /// <returns></returns>
        public ActionResult JournalEdit(string id) {
            Journal journal = new JournalDao(db).GetByKey(new Guid(id));
            // ����J���Ƃ������̔��f
            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);

            // �`�[���t�����������O����������ߏ����ς݈����i�����j
            if (journal.AccountType != null && journal.AccountType.Equals("001") && cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate) {
                journal.IsClosed = true;
            }
 
            // Mod 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ�  �����ߎ��̃f�[�^�ҏW�����̗L���ŕ�����i�o�����A����ȊO���j
            //���O�C�����[�U���擾
            Employee loginUser = ((Employee)Session["Employee"]);
            //loginUser.SecurityRoleCode = new EmployeeDao(db).GetByKey(loginUser.EmployeeCode).SecurityRoleCode;
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //�����f�[�^�ҏW���������邩�Ȃ���

            //�����f�[�^�ҏW����������Ζ{���߂̎��̂݃G���[
            if (AppRole.EnableFlag){
                //�����ߎ��̑��쌠�����������ꍇ�́A�{���ߎ�����NG�ɂ���
                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001")){
                    journal.IsClosed = true;
                }
            }else{
                // �����łȂ���Ή����߈ȏ�ŃG���[
                // Add 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ� �S�̂ł̏������ʏ������o�����߂�����悤�ɂ���
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(journal.DepartmentCode, journal.JournalDate, "001")){
                    journal.IsClosed = true;
                }
            }
            SetEditViewData(journal);
            return View("JournalEdit", journal);
        }
        #endregion

        /// <summary>
        /// �������яC������
        /// </summary>
        /// <param name="journal">�������у��f���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/22 yano #3930 �������у��X�g�@�}�C�i�X�̓����\�肪�ł��Ă����ԂŎ��э폜�����ꍇ�̎c���s��
        /// 2016/05/17 arc nakayama #3551_�`�[�ԍ���ύX�������̏���
        /// 2014/08/12 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
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

                //Del 2018/08/22 yano �Â��R�[�h�̍폜
                
                //Add 2016/05/17 arc nakayama #3551
                //�ύX�O�̓`�[�ԍ��Ɛ�������擾�ޔ�
                string PrevSlipNumber = target.SlipNumber;
                string PrevCustomerClaimCode = target.CustomerClaimCode;
                decimal PrevAmount = target.Amount;

                UpdateModel(target);

                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;


                //��x�������т̏C�����e�𔽉f���Ă���
                db.SubmitChanges();                     //Add 2018/08/22 yano #3930

                //Add 2016/05/20 arc nakayama #3551 �������т��폜�����Ƃ��ɓ`�[�ԍ������͂���Ă�����R�Â������\��̎c�������Ƃɖ߂�
                if (target.DelFlag.Equals("1"))
                {
                    //------------
                    //�폜����
                    //------------

                    //�`�[�ԍ����o�^����Ă���ꍇ�˓����\��̎c�������Ƃɖ߂�
                    //�J�[�h��Ђ���̓����̏ꍇ�˃J�[�h��Ђ���̓����\��̎c�������Ƃɖ߂��i�P�̓����\��ɑ΂��ăJ�[�h�ŕ���������ł��邽�ߕʏ����j
                    if (!string.IsNullOrEmpty(target.SlipNumber))
                    {
                        if (target.AccountType.Equals("011"))
                        {
                            //�J�[�h��Ђ���̓����̏ꍇ �J�[�h��Ђ���̓����\�������
                            ReceiptPlan CreditPlanData = new ReceiptPlanDao(db).GetByStringKey(target.CreditReceiptPlanId);
                            if (CreditPlanData != null)
                            {
                                //�����z���c���ɉ��Z�i���ɖ߂��j
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
                            //Mod 2018/08/22 yano #3930 �����\����č쐬����
                            // �ԗ��`�[
                            CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                            if (carSalesHeader != null)
                            {
                                CreateCarReceiptPlan(carSalesHeader, target);
                            }
                            // �T�[�r�X�`�[
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

                            ////������ʂ��u�J�[�h��Ђ���̓����v�łȂ���΁A�`�[�ԍ��Ɛ�����Ńq�b�g���������\��̎c�������Ƃɖ߂�
                            ////�J�[�h�������ꍇ�́A�J�[�h��Ђ���̓����\����폜����
                            //List<ReceiptPlan> BackPlanData = new ReceiptPlanDao(db).GetListByCustomerClaimDesc(target.SlipNumber, target.CustomerClaimCode);
                            ////Mod 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l�� �����\��z���������ɕԋ�������
                            //RePayment(BackPlanData, target.Amount);


                            //�J�[�h��Ђ���̓����\��폜
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
                    //�ۑ�����
                    //------------
                    //if (!string.IsNullOrEmpty(journal.SlipNumber) && journal.ReceiptPlanFlag != null && journal.ReceiptPlanFlag.Equals("1"))
                    if (!string.IsNullOrEmpty(target.SlipNumber))
                    {
                        //Add arc nakayama #3551
                        //�`�[�ԍ��������Ă����ꍇ�́A�����ώ��тɕҏW����͂��Ȃ̂ŁAReceiptPlanFlag = 0 �� 1�ɕύX����B
                        if (string.IsNullOrWhiteSpace(journal.ReceiptPlanFlag) || journal.ReceiptPlanFlag.Equals("0"))
                        {
                            target.ReceiptPlanFlag = "1";
                        }

                        // �ԗ��`�[
                        CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);
                        if (carSalesHeader != null)
                        {
                            CreateCarReceiptPlan(carSalesHeader, target);
                        }
                        // �T�[�r�X�`�[
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

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
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
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetEditViewData(target);
                            return View("JournalEdit", target);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_UPD_JSK, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
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

        #region  �o�������̃`�F�b�N
        //Add 2016/05/16 arc nakayama #3544_������ʂ��J�[�h�E���[���ɂ͕ύX�����Ȃ� 
        /// <summary>
        /// �����̃`�F�b�N
        /// </summary>
        /// <param name="EmployeeCode">�Ј��R�[�h</param>
        /// <param name="ApplicationCode">�A�v���P�[�V������(������)</param>
        /// <returns>�w�肵�������������Ă����:True  ����ȊO:False</returns>
        public bool CheckApplicationRole(string EmployeeCode, string ApplicationCode)
        {
            //���O�C�����[�U���擾
            Employee loginUser = new EmployeeDao(db).GetByKey(EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, ApplicationCode); //���������邩�Ȃ���

            // �o�������������true�����łȂ����false
            if (AppRole.EnableFlag)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region �����\��z�̏��Ȃ����ɓ������s��
        /// <summary>
        /// �����\��z�̏��Ȃ����ɓ������s���i�����\�胊�X�g, �����z�j
        /// </summary>
        /// <param name="ReceiptPlanList">�����\�胊�X�g</param>
        /// <param name="JournalAmount">�����z</param>
        /// <returns></returns>
        /// <history>Add 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��</history>
        private void Payment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount; //�����z
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

        #region �����\��z�̑������ɕԋ�����������
        /// <summary>
        /// �����\��z�̑������ɕԋ�����������i�����\�胊�X�g, �����z�j
        /// </summary>
        /// <param name="ReceiptPlanList">�����\�胊�X�g</param>
        /// <param name="JournalAmount">�����z</param>
        /// <returns></returns>
        /// <history>Add 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��</history>
        private void RePayment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount;
            decimal BackBlance = 0m; // �ԋ��ł�����z�@�i�����\��z - �����c�� = �ԋ��ł�����z�j

            foreach (var plan in ReceiptPlanList)
            {
                BackBlance = 0m; //������

                if ((plan.ReceivableBalance ?? 0m) + remainAmount <= plan.Amount)
                {
                    plan.ReceivableBalance = (plan.ReceivableBalance ?? 0m) + remainAmount;
                    remainAmount = 0m;
                }
                else
                {
                    //�ԋ��ł�����z�����߂�
                    BackBlance = (plan.Amount ?? 0m) - (plan.ReceivableBalance ?? 0m);

                    //�ԋ��ł���
                    if (BackBlance != 0m)
                    {
                        plan.ReceivableBalance += BackBlance;
                        remainAmount = remainAmount - BackBlance;
                    }
                }

                //���������t���O�X�V
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
