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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �x����ʃ}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PaymentKindController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�x����ʃ}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�x����ʃ}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PaymentKindController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �x����ʌ�����ʕ\��
        /// </summary>
        /// <returns>�x����ʌ������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �x����ʌ�����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x����ʌ������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<PaymentKind> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["PaymentKindCode"] = form["PaymentKindCode"];
            ViewData["PaymentKindName"] = form["PaymentKindName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �x����ʌ�����ʂ̕\��
            return View("PaymentKindCriteria", list);
        }

        /// <summary>
        /// �x����ʌ����_�C�A���O�\��
        /// </summary>
        /// <returns>�x����ʌ����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �x����ʌ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x����ʌ�����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["PaymentKindCode"] = Request["PaymentKindCode"];
            form["PaymentKindName"] = Request["PaymentKindName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<PaymentKind> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["PaymentKindCode"] = form["PaymentKindCode"];
            ViewData["PaymentKindName"] = form["PaymentKindName"];

            // �x����ʌ�����ʂ̕\��
            return View("PaymentKindCriteriaDialog", list);
        }

        /// <summary>
        /// �x����ʃ}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�x����ʃR�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�x����ʃ}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            PaymentKind paymentKind;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                paymentKind = new PaymentKind();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                paymentKind = new PaymentKindDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(paymentKind);

            // �o��
            return View("PaymentKindEntry", paymentKind);
        }

        /// <summary>
        /// �x����ʃ}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="paymentKind">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�x����ʃ}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PaymentKind paymentKind, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidatePaymentKind(paymentKind);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(paymentKind);
                return View("PaymentKindEntry", paymentKind);
            }

            // Add 2014/08/01 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                PaymentKind targetPaymentKind = new PaymentKindDao(db).GetByKey(paymentKind.PaymentKindCode, true);
                UpdateModel(targetPaymentKind);
                EditPaymentKindForUpdate(targetPaymentKind); 
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                paymentKind = EditPaymentKindForInsert(paymentKind);
                // �f�[�^�ǉ�
                db.PaymentKind.InsertOnSubmit(paymentKind);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o�͏����ǉ�
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
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
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

                        ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0010", new string[] { "�x����ʃR�[�h", "�ۑ�" }));
                        GetEntryViewData(paymentKind);
                        return View("PaymentKindEntry", paymentKind);
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
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(paymentKind.PaymentKindCode);
        }

        /// <summary>
        /// �x����ʃR�[�h����x����ʖ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�x����ʃR�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                PaymentKind paymentKind = new PaymentKindDao(db).GetByKey(code);
                if (paymentKind != null)
                {
                    codeData.Code = paymentKind.PaymentKindCode;
                    codeData.Name = paymentKind.PaymentKindName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="paymentKind">���f���f�[�^</param>
        private void GetEntryViewData(PaymentKind paymentKind)
        {
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["PaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetPaymentTypeAll(false), paymentKind.PaymentType, true);
        }

        /// <summary>
        /// �x����ʃ}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�x����ʃ}�X�^�������ʃ��X�g</returns>
        private PaginatedList<PaymentKind> GetSearchResultList(FormCollection form)
        {
            PaymentKindDao paymentKindDao = new PaymentKindDao(db);
            PaymentKind paymentKindCondition = new PaymentKind();
            paymentKindCondition.PaymentKindCode = form["PaymentKindCode"];
            paymentKindCondition.PaymentKindName = form["PaymentKindName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                paymentKindCondition.DelFlag = form["DelFlag"];
            }
            return paymentKindDao.GetListByCondition(paymentKindCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="paymentKind">�x����ʃf�[�^</param>
        /// <returns>�x����ʃf�[�^</returns>
        private PaymentKind ValidatePaymentKind(PaymentKind paymentKind)
        {
            // �K�{�`�F�b�N(���l�K�{���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(paymentKind.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0001", "�x����ʃR�[�h"));
            }
            if (string.IsNullOrEmpty(paymentKind.PaymentKindName))
            {
                ModelState.AddModelError("PaymentKindName", MessageUtils.GetMessage("E0001", "�x����ʖ�"));
            }
            if (!ModelState.IsValidField("CommissionRate"))
            {
                ModelState.AddModelError("CommissionRate", MessageUtils.GetMessage("E0002", new string[] { "�萔����", "���̐���3���ȓ�������5���ȓ�" }));
                if (ModelState["CommissionRate"].Errors.Count > 1)
                {
                    ModelState["CommissionRate"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("ClaimDay"))
            {
                ModelState.AddModelError("ClaimDay", MessageUtils.GetMessage("E0002", new string[] { "���ߓ�", "0�`31" }));
                if (ModelState["ClaimDay"].Errors.Count > 1)
                {
                    ModelState["ClaimDay"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(paymentKind.PaymentType))
            {
                ModelState.AddModelError("PaymentType", MessageUtils.GetMessage("E0001", "�x���敪"));
            }
            if (!ModelState.IsValidField("PaymentDay"))
            {
                ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0002", new string[] { "�x����", "0�`31" }));
                if (ModelState["PaymentDay"].Errors.Count > 1)
                {
                    ModelState["PaymentDay"].Errors.RemoveAt(0);
                }
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("PaymentKindCode") && !CommonUtils.IsAlphaNumeric(paymentKind.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0012", "�x����ʃR�[�h"));
            }
            if (ModelState.IsValidField("CommissionRate"))
            {
                if ((Regex.IsMatch(paymentKind.CommissionRate.ToString(), @"^\d{1,3}\.\d{1,5}$"))
                    || (Regex.IsMatch(paymentKind.CommissionRate.ToString(), @"^\d{1,3}$")))
                {
                }
                else
                {
                    ModelState.AddModelError("CommissionRate", MessageUtils.GetMessage("E0002", new string[] { "�萔����", "���̐���3���ȓ�������5���ȓ�" }));
                }
            }

            // �l�`�F�b�N
            if (ModelState.IsValidField("ClaimDay"))
            {
                if (paymentKind.ClaimDay < 0 || paymentKind.ClaimDay > 31)
                {
                    ModelState.AddModelError("ClaimDay", MessageUtils.GetMessage("E0002", new string[] { "���ߓ�", "0�`31" }));
                }
            }
            if (ModelState.IsValidField("PaymentDay"))
            {
                if (paymentKind.PaymentDay < 0 || paymentKind.PaymentDay > 31)
                {
                    ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0002", new string[] { "�x����", "0�`31" }));
                }
            }

            return paymentKind;
        }

        /// <summary>
        /// �x����ʃ}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="paymentKind">�x����ʃf�[�^(�o�^���e)</param>
        /// <returns>�x����ʃ}�X�^���f���N���X</returns>
        private PaymentKind EditPaymentKindForInsert(PaymentKind paymentKind)
        {
            paymentKind.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.CreateDate = DateTime.Now;
            paymentKind.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.LastUpdateDate = DateTime.Now;
            paymentKind.DelFlag = "0";
            return paymentKind;
        }

        /// <summary>
        /// �x����ʃ}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="paymentKind">�x����ʃf�[�^(�o�^���e)</param>
        /// <returns>�x����ʃ}�X�^���f���N���X</returns>
        private PaymentKind EditPaymentKindForUpdate(PaymentKind paymentKind)
        {
            paymentKind.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.LastUpdateDate = DateTime.Now;
            return paymentKind;
        }

    }
}
