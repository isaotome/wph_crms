using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �x����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SupplierPaymentController : Controller
    {
        //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�x����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�x����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SupplierPaymentController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �x���挟����ʕ\��
        /// </summary>
        /// <returns>�x���挟�����</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �x���挟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x���挟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<SupplierPayment> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            ViewData["SupplierPaymentName"] = form["SupplierPaymentName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �x���挟����ʂ̕\��
            return View("SupplierPaymentCriteria", list);
        }

        /// <summary>
        /// �x���挟���_�C�A���O�\��
        /// </summary>
        /// <returns>�x���挟���_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �x���挟���_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x���挟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["SupplierPaymentCode"] = Request["SupplierPaymentCode"];
            form["SupplierPaymentName"] = Request["SupplierPaymentName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<SupplierPayment> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            ViewData["SupplierPaymentName"] = form["SupplierPaymentName"];

            // �x���挟����ʂ̕\��
            return View("SupplierPaymentCriteriaDialog", list);
        }

        /// <summary>
        /// �x����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�x����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�x����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            SupplierPayment supplierPayment;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                supplierPayment = new SupplierPayment();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                supplierPayment = new SupplierPaymentDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(supplierPayment);

            // �o��
            return View("SupplierPaymentEntry", supplierPayment);
        }

        /// <summary>
        /// �x����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="supplierPayment">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SupplierPayment supplierPayment, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateSupplierPayment(supplierPayment,form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(supplierPayment);
                return View("SupplierPaymentEntry", supplierPayment);
            }

            // Add 2014/08/12 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                SupplierPayment targetSupplierPayment = new SupplierPaymentDao(db).GetByKey(supplierPayment.SupplierPaymentCode);
                UpdateModel(targetSupplierPayment);
                EditSupplierPaymentForUpdate(targetSupplierPayment);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                supplierPayment = EditSupplierPaymentForInsert(supplierPayment);

                // �f�[�^�ǉ�
                db.SupplierPayment.InsertOnSubmit(supplierPayment);
            }

            //Add 2014/08/12 arc amii �G���[���O�Ή� SubmitChanges��������{�� & Exception���ɃG���[���O�o�͏����ǉ�
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException e)
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
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (SqlException e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0010", new string[] { "�x����R�[�h", "�ۑ�" }));
                        GetEntryViewData(supplierPayment);
                        return View("SupplierPaymentEntry", supplierPayment);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �x����R�[�h����x���於���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�x����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                SupplierPayment supplierPayment = new SupplierPaymentDao(db).GetByKey(code);
                if (supplierPayment != null)
                {
                    codeData.Code = supplierPayment.SupplierPaymentCode;
                    codeData.Name = supplierPayment.SupplierPaymentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="supplierPayment">���f���f�[�^</param>
        private void GetEntryViewData(SupplierPayment supplierPayment)
        {
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["SupplierPaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSupplierPaymentTypeAll(false), supplierPayment.SupplierPaymentType, true);
            ViewData["PaymentType2List"] = CodeUtils.GetSelectListByModel(dao.GetPaymentType2All(false), supplierPayment.PaymentType, true);
        }

        /// <summary>
        /// �x����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<SupplierPayment> GetSearchResultList(FormCollection form)
        {
            SupplierPaymentDao supplierPaymentDao = new SupplierPaymentDao(db);
            SupplierPayment supplierPaymentCondition = new SupplierPayment();
            supplierPaymentCondition.SupplierPaymentCode = form["SupplierPaymentCode"];
            supplierPaymentCondition.SupplierPaymentName = form["SupplierPaymentName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                supplierPaymentCondition.DelFlag = form["DelFlag"];
            }
            return supplierPaymentDao.GetListByCondition(supplierPaymentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="supplierPayment">�x����f�[�^</param>
        /// <returns>�x����f�[�^</returns>
        private SupplierPayment ValidateSupplierPayment(SupplierPayment supplierPayment,FormCollection form)
        {
            // �K�{�`�F�b�N(���l�K�{���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentCode))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0001", "�x����R�[�h"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentName))
            {
                ModelState.AddModelError("SupplierPaymentName", MessageUtils.GetMessage("E0001", "�x���於"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentType))
            {
                ModelState.AddModelError("SupplierPaymentType", MessageUtils.GetMessage("E0001", "�x������"));
            }
            if (string.IsNullOrEmpty(supplierPayment.PaymentType))
            {
                ModelState.AddModelError("PaymentType", MessageUtils.GetMessage("E0001", "�x���敪"));
            }
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            if ((CommonUtils.DefaultString(supplierPayment.PaymentType).Equals("003")) && (supplierPayment.PaymentDayCount == null))
            {
                ModelState.AddModelError("PaymentDayCount", MessageUtils.GetMessage("E0008", new string[] { "�x���敪��n����", "����", "5�`240" }));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("PaymentDay"))
            {
                ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "�x����", "0�`31" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod1")) {
                ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0004", new string[] { "�P�\����1", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod2")) {
                ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0004", new string[] { "�P�\����2", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod3")) {
                ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0004", new string[] { "�P�\����3", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod4")) {
                ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0004", new string[] { "�P�\����4", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod5")) {
                ModelState.AddModelError("PaymentPeriod5", MessageUtils.GetMessage("E0004", new string[] { "�P�\����5", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod6")) {
                ModelState.AddModelError("PaymentPeriod6", MessageUtils.GetMessage("E0004", new string[] { "�P�\����6", "0�ȊO�̐��̐���" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("SupplierPaymentCode") && !CommonUtils.IsAlphaNumeric(supplierPayment.SupplierPaymentCode))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0012", "�x����R�[�h"));
            }

            if (string.IsNullOrEmpty(form["PaymentRate1"]) || (Regex.IsMatch(form["PaymentRate1"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate1"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate1", MessageUtils.GetMessage("E0004", new string[] { "��������1", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate2"]) || (Regex.IsMatch(form["PaymentRate2"], @"^\d{1,3}\.\d{1,3}$"))
                            || (Regex.IsMatch(form["PaymentRate2"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate2", MessageUtils.GetMessage("E0004", new string[] { "��������2", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate3"]) || (Regex.IsMatch(form["PaymentRate3"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate3"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate3", MessageUtils.GetMessage("E0004", new string[] { "��������3", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate4"]) || (Regex.IsMatch(form["PaymentRate4"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate4"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate4", MessageUtils.GetMessage("E0004", new string[] { "��������4", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if ((string.IsNullOrEmpty(form["PaymentRate5"]) || Regex.IsMatch(form["PaymentRate5"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate5"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate5", MessageUtils.GetMessage("E0004", new string[] { "��������5", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate6"]) || (Regex.IsMatch(form["PaymentRate6"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate6"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate6", MessageUtils.GetMessage("E0004", new string[] { "��������6", "���̐���3���ȓ�������3���ȓ�" }));
            }


            //�P�\���������͂���Ă���ꍇ�A�����͓��͕K�{
            if (!string.IsNullOrEmpty(form["PaymentPeriod1"]) && string.IsNullOrEmpty(form["PaymentRate1"])) {
                ModelState.AddModelError("PaymentRate1", MessageUtils.GetMessage("E0001", new string[] { "��������1" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod2"]) && string.IsNullOrEmpty(form["PaymentRate2"])) {
                ModelState.AddModelError("PaymentRate2", MessageUtils.GetMessage("E0001", new string[] { "��������2" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod3"]) && string.IsNullOrEmpty(form["PaymentRate3"])) {
                ModelState.AddModelError("PaymentRate3", MessageUtils.GetMessage("E0001", new string[] { "��������3" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod4"]) && string.IsNullOrEmpty(form["PaymentRate4"])) {
                ModelState.AddModelError("PaymentRate4", MessageUtils.GetMessage("E0001", new string[] { "��������4" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod5"]) && string.IsNullOrEmpty(form["PaymentRate5"])) {
                ModelState.AddModelError("PaymentRate5", MessageUtils.GetMessage("E0001", new string[] { "��������5" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod6"]) && string.IsNullOrEmpty(form["PaymentRate6"])) {
                ModelState.AddModelError("PaymentRate6", MessageUtils.GetMessage("E0001", new string[] { "��������6" }));
            }


            // �l�`�F�b�N
            if (ModelState.IsValidField("PaymentDayCount"))
            {
                if (supplierPayment.PaymentDayCount < 5 || supplierPayment.PaymentDayCount > 240)
                {
                    ModelState.AddModelError("PaymentDayCount", MessageUtils.GetMessage("E0004", new string[] { "����", "5�`240" }));
                }
            }
            if (ModelState.IsValidField("PaymentDay"))
            {
                if (supplierPayment.PaymentDay < 0 || supplierPayment.PaymentDay > 31)
                {
                    ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "�x����", "0�`31" }));
                }
            }

            //�O������̃`�F�b�N
            if (!string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (supplierPayment.PaymentPeriod1 >= supplierPayment.PaymentPeriod2) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", "�P�\����2�͗P�\����1���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (supplierPayment.PaymentPeriod2 >= supplierPayment.PaymentPeriod3) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", "�P�\����3�͗P�\����2���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (supplierPayment.PaymentPeriod3 >= supplierPayment.PaymentPeriod4) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", "�P�\����4�͗P�\����3���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod5"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "�P�\����4" }));
                    }
                }
                if (supplierPayment.PaymentPeriod4 >= supplierPayment.PaymentPeriod5) {
                    if (ModelState["PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod5", "�P�\����5�͗P�\����4���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod6"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "�P�\����4" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod5"])) {
                    if (ModelState["PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod5", MessageUtils.GetMessage("E0001", new string[] { "�P�\����5" }));
                    }
                }
                if (supplierPayment.PaymentPeriod5 >= supplierPayment.PaymentPeriod6) {
                    if (ModelState["PaymentPeriod6"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod6", "�P�\����6�͗P�\����5���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }
            return supplierPayment;
        }

        /// <summary>
        /// �x����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="supplierPayment">�x����f�[�^(�o�^���e)</param>
        /// <returns>�x����}�X�^���f���N���X</returns>
        private SupplierPayment EditSupplierPaymentForInsert(SupplierPayment supplierPayment)
        {
            supplierPayment.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.CreateDate = DateTime.Now;
            supplierPayment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.LastUpdateDate = DateTime.Now;
            supplierPayment.DelFlag = "0";
            return supplierPayment;
        }

        /// <summary>
        /// �x����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="supplierPayment">�x����f�[�^(�o�^���e)</param>
        /// <returns>�x����}�X�^���f���N���X</returns>
        private SupplierPayment EditSupplierPaymentForUpdate(SupplierPayment supplierPayment)
        {
            supplierPayment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.LastUpdateDate = DateTime.Now;
            return supplierPayment;
        }

    }
}
