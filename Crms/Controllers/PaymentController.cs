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
using Crms.Models;                      //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PaymentController : Controller
    {
        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�x���Ǘ� ";               // ��ʖ�
        private static readonly string PROC_NAME = "�x���������דo�^";        // ������ 

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PaymentController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �x���\�茟����ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            return Criteria(new EntitySet<Journal>() ,new FormCollection());
        }

        /// <summary>
        /// �x���\�茟������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x���\�茟�����ʕ\��</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(EntitySet<Journal> line, FormCollection form) {


            // �f�t�H���g�l�̐ݒ�
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // �����������דo�^����
            if (form["action"].Equals("regist") && line != null) {

                // �f�[�^�`�F�b�N
                ValidateJournal(line, form);

                // �f�[�^�o�^����
                if (ModelState.IsValid) {

                    for (int i = 0; i < line.Count; i++) {

                        string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                        if ((!CommonUtils.DefaultString(form[prefix + "JournalDate"]).Equals(string.Format("{0:yyyy/MM/dd}", DateTime.Now.Date)))
                            || (!string.IsNullOrEmpty(form[prefix + "AccountType"]))
                            || (!CommonUtils.DefaultString(form[prefix + "Amount"]).Equals("0"))) {

                            Journal journal = line[i];

                            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                            db = new CrmsLinqDataContext();
                            db.Log = new OutputWriter();

                            using (TransactionScope ts = new TransactionScope()) {

                                PaymentPlan targetPaymentPlan = new PaymentPlanDao(db).GetByKey(new Guid(form[prefix + "PaymentPlanId"]));

                                // �o�[���o�^
                                InsertJournal(targetPaymentPlan, journal);

                                // �x������
                                UpdateReceiptPlan(targetPaymentPlan, journal);

                                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
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
                                            // �Z�b�V������SQL����o�^
                                            Session["ExecSQL"] = OutputLogData.sqlText;
                                            // ���O�ɏo��
                                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
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
                                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
                                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�x�������o�^"));
                                        }
                                        else
                                        {
                                            // ���O�ɏo��
                                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetPaymentPlan.SlipNumber);
                                            // �G���[�y�[�W�ɑJ��
                                            return View("Error");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        // �Z�b�V������SQL����o�^
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
        /// �x���������̓`�F�b�N
        /// </summary>
        /// <param name="journal">�x�������f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x�������f�[�^</returns>
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

                    // �K�{�`�F�b�N(���l/���t���ڂ͑����`�F�b�N�����˂�)
                    if (!ModelState.IsValidField(prefix + "JournalDate")) {
                        ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "�`�[���t")));
                        alreadyOutputMsgE0003 = true;
                        if (ModelState[prefix + "JournalDate"].Errors.Count > 1) {
                            ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                        }
                    }
                    if (string.IsNullOrEmpty(journal.AccountType)) {
                        ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "�������")));
                        alreadyOutputMsgE0001 = true;
                    }
                    if (!ModelState.IsValidField(prefix + "Amount")) {
                        ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                        alreadyOutputMsgE0002 = true;
                        if (ModelState[prefix + "Amount"].Errors.Count > 1) {
                            ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                        }
                    }

                    // �t�H�[�}�b�g�`�F�b�N
                    if (ModelState.IsValidField(prefix + "Amount")) {
                        if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$")) {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }

                    // �l�`�F�b�N
                    if (ModelState.IsValidField(prefix + "JournalDate")) {
                        if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0) {
                            ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0013a ? "" : MessageUtils.GetMessage("E0013", new string[] { "�`�[���t", "�{���ȑO" })));
                            alreadyOutputMsgE0013a = true;
                        }
                    }
                    if (ModelState.IsValidField(prefix + "Amount")) {
                        if (journal.Amount.Equals(0m)) {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// �c�����v�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x���c�����v���z</returns>
        private decimal GetTotalBalance(FormCollection form) {
            return new PaymentPlanDao(db).GetTotalBalance(GetSearchCondition(form));
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g
        /// </summary>
        /// <param name="list">�x���\��f�[�^���X�g</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x���\��f�[�^���X�g</returns>
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
        /// �x���\�茟�����ʂ��擾����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x���\�茟�����ʃf�[�^���X�g</returns>
        private PaginatedList<PaymentPlan> GetSearchResult(FormCollection form) {
            PaginatedList<PaymentPlan> list = new PaymentPlanDao(db).GetListByCondition(GetSearchCondition(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return list;
        }

        /// <summary>
        /// �����������Z�b�g����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x���\�茟�������f�[�^</returns>
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
        /// �x���������דo�^����
        /// </summary>
        /// <param name="targetReceiptPlan">�����Ώێx���\�胂�f���f�[�^</param>
        /// <param name="journal">�o�[�����f���f�[�^</param>
        private void InsertJournal(PaymentPlan targetPaymentPlan, Journal journal) {

            // JournalDate,AccountType,Amount�̓t���[�����[�N�ɂĕҏW�ς�
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
        /// �x���\��e�[�u���X�V����
        /// </summary>
        /// <param name="targetReceiptPlan">�����Ώێx���\�胂�f���f�[�^</param>
        /// <param name="journal">�o�[�����f���f�[�^</param>
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
