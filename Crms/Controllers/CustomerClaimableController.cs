using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���Ϗ����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerClaimableController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���Ϗ����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���Ϗ����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CustomerClaimableController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���Ϗ���������ʕ\��
        /// </summary>
        /// <returns>���Ϗ����������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���Ϗ���������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ϗ����������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �폜����
            if (form["action"].Equals("remove"))
            {
                CustomerClaimable targetCustomerClaimable = new CustomerClaimableDao(db).GetByKey(form["key1"], form["key2"]);
                db.CustomerClaimable.DeleteOnSubmit(targetCustomerClaimable);
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
                            // Mod 2014/08/04 arc amii �G���[���O�Ή� ���O�t�@�C���ɃG���[���o�͂���悤�C��
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/04 arc amii �G���[���O�Ή� ChangeConflictException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            // �������ʃ��X�g�̎擾
            PaginatedList<CustomerClaimable> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            if (!string.IsNullOrEmpty(form["CustomerClaimCode"]))
            {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(form["CustomerClaimCode"]);
                if (customerClaim != null)
                {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }

            // ���Ϗ���������ʂ̕\��
            return View("CustomerClaimableCriteria", list);
        }

        /// <summary>
        /// ���Ϗ����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���Ϗ����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���Ϗ����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CustomerClaimable customerClaimable;

            // �\���f�[�^�ݒ�
            customerClaimable = new CustomerClaimable();
            customerClaimable.CustomerClaimCode = id;
            GetEntryViewData(customerClaimable);

            // �o��
            return View("CustomerClaimableEntry", customerClaimable);
        }

        /// <summary>
        /// ���Ϗ����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="customerClaimable">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���Ϗ����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerClaimable customerClaimable, FormCollection form)
        {
            // �f�[�^�`�F�b�N
            ValidateCustomerClaimable(customerClaimable);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(customerClaimable);
                return View("CustomerClaimableEntry", customerClaimable);
            }

            // �f�[�^�ҏW
            customerClaimable = EditCustomerClaimableForInsert(customerClaimable);

            // Add 2014/08/01 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�ǉ�
            db.CustomerClaimable.InsertOnSubmit(customerClaimable);
            try
            {
                db.SubmitChanges();
            }
            catch (SqlException e)
            {
                // Mod 2014/08/01 arc amii �G���[���O�Ή� ���O�t�@�C���ɃG���[���o�͂���悤�C��
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;

                if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                {
                    // ���O�ɏo��
                    OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                    ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0010", new string[] { "�x�����", "�ۑ�" }));
                    GetEntryViewData(customerClaimable);
                    return View("CustomerClaimableEntry", customerClaimable);
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
                //Add 2014/08/01 arc amii �G���[���O�Ή� SqlException�ȊO�̎��̃G���[�����ǉ�
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ���O�ɏo��
                OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                // �G���[�y�[�W�ɑJ��
                return View("Error");
            }

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �O���[�h�R�[�h�C�I�v�V�����R�[�h����f�[�^���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code1">�O���[�h�R�[�h</param>
        /// <param name="code2">�I�v�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code1, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CustomerClaimable customerClaimable = new CustomerClaimableDao(db).GetByKey(code1, code2);
                if (customerClaimable != null)
                {
                    codeData.Code = customerClaimable.CustomerClaimCode;
                    codeData.Code2 = customerClaimable.PaymentKindCode;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="customerClaimable">���f���f�[�^</param>
        private void GetEntryViewData(CustomerClaimable customerClaimable)
        {
            // �x����ʖ��̎擾
            if (!string.IsNullOrEmpty(customerClaimable.PaymentKindCode))
            {
                PaymentKindDao paymentKindDao = new PaymentKindDao(db);
                PaymentKind paymentKind = paymentKindDao.GetByKey(customerClaimable.PaymentKindCode);
                if (paymentKind != null)
                {
                    ViewData["PaymentKindName"] = paymentKind.PaymentKindName;
                }
            }
        }

        /// <summary>
        /// ���Ϗ����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ϗ����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<CustomerClaimable> GetSearchResultList(FormCollection form)
        {
            CustomerClaimableDao customerClaimableDao = new CustomerClaimableDao(db);
            CustomerClaimable customerClaimableCondition = new CustomerClaimable();
            customerClaimableCondition.CustomerClaimCode = form["CustomerClaimCode"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                customerClaimableCondition.DelFlag = form["DelFlag"];
            }
            return customerClaimableDao.GetListByCondition(customerClaimableCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="customerClaimable">���Ϗ����f�[�^</param>
        /// <returns>���Ϗ����f�[�^</returns>
        private CustomerClaimable ValidateCustomerClaimable(CustomerClaimable customerClaimable)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(customerClaimable.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0001", "�x�����"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("PaymentKindCode") && !CommonUtils.IsAlphaNumeric(customerClaimable.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0012", "�x�����"));
            }

            return customerClaimable;
        }

        /// <summary>
        /// ���Ϗ����}�X�^�ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="customerClaimable">���Ϗ����f�[�^(�o�^���e)</param>
        /// <returns>���Ϗ����}�X�^���f���N���X</returns>
        private CustomerClaimable EditCustomerClaimableForInsert(CustomerClaimable customerClaimable)
        {
            customerClaimable.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaimable.CreateDate = DateTime.Now;
            customerClaimable.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaimable.LastUpdateDate = DateTime.Now;
            customerClaimable.DelFlag = "0";
            return customerClaimable;
        }

    }
}
