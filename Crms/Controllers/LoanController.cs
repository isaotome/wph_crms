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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���[���}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class LoanController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���[���}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���[���}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public LoanController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���[��������ʕ\��
        /// </summary>
        /// <returns>���[���������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���[��������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[���������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Loan> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["LoanCode"] = form["LoanCode"];
            ViewData["LoanName"] = form["LoanName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���[��������ʂ̕\��
            return View("LoanCriteria", list);
        }

        /// <summary>
        /// ���[�������_�C�A���O�\��
        /// </summary>
        /// <returns>���[�������_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���[�������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[��������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CustomerClaimCode"] = Request["CustomerClaimCode"];
            form["CustomerClaimName"] = Request["CustomerClaimName"];
            form["LoanCode"] = Request["LoanCode"];
            form["LoanName"] = Request["LoanName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Loan> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["LoanCode"] = form["LoanCode"];
            ViewData["LoanName"] = form["LoanName"];

            // ���[��������ʂ̕\��
            return View("LoanCriteriaDialog", list);
        }

        /// <summary>
        /// ���[���}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���[���R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���[���}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Loan loan;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                loan = new Loan();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                loan = new LoanDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(loan);

            // �o��
            return View("LoanEntry", loan);
        }

        /// <summary>
        /// ���[���}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="loan">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���[���}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Loan loan, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateLoan(loan);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(loan);
                return View("LoanEntry", loan);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Loan targetLoan = new LoanDao(db).GetByKey(loan.LoanCode, true);
                UpdateModel(targetLoan);
                EditLoanForUpdate(targetLoan);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                loan = EditLoanForInsert(loan);
                // �f�[�^�ǉ�
                db.Loan.InsertOnSubmit(loan);
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
                catch (SqlException se)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0010", new string[] { "���[���R�[�h", "�ۑ�" }));
                        GetEntryViewData(loan);
                        return View("LoanEntry", loan);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
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
            return Entry(loan.LoanCode);
        }

        /// <summary>
        /// ���[���R�[�h���烍�[�������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���[���R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Loan loan = new LoanDao(db).GetByKey(code);
                if (loan != null)
                {
                    codeData.Code = loan.LoanCode;
                    codeData.Name = loan.LoanName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="loan">���f���f�[�^</param>
        private void GetEntryViewData(Loan loan)
        {
            // �����於�̎擾
            if (!string.IsNullOrEmpty(loan.CustomerClaimCode))
            {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(loan.CustomerClaimCode);
                if (customerClaim != null)
                {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }
        }

        /// <summary>
        /// ���[���}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[���}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Loan> GetSearchResultList(FormCollection form)
        {
            LoanDao loanDao = new LoanDao(db);
            Loan loanCondition = new Loan();
            loanCondition.LoanCode = form["LoanCode"];
            loanCondition.LoanName = form["LoanName"];
            loanCondition.CustomerClaim = new CustomerClaim();
            loanCondition.CustomerClaim.CustomerClaimCode = form["CustomerClaimCode"];
            loanCondition.CustomerClaim.CustomerClaimName = form["CustomerClaimName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                loanCondition.DelFlag = form["DelFlag"];
            }
            return loanDao.GetListByCondition(loanCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="loan">���[���f�[�^</param>
        /// <returns>���[���f�[�^</returns>
        private Loan ValidateLoan(Loan loan)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(loan.LoanCode))
            {
                ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0001", "���[���R�[�h"));
            }
            if (string.IsNullOrEmpty(loan.LoanName))
            {
                ModelState.AddModelError("LoanName", MessageUtils.GetMessage("E0001", "���[����"));
            }
            if (string.IsNullOrEmpty(loan.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0001", "������"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("LoanCode") && !CommonUtils.IsAlphaNumeric(loan.LoanCode))
            {
                ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0012", "���[���R�[�h"));
            }

            return loan;
        }

        /// <summary>
        /// ���[���}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="loan">���[���f�[�^(�o�^���e)</param>
        /// <returns>���[���}�X�^���f���N���X</returns>
        private Loan EditLoanForInsert(Loan loan)
        {
            loan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.CreateDate = DateTime.Now;
            loan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.LastUpdateDate = DateTime.Now;
            loan.DelFlag = "0";
            return loan;
        }

        /// <summary>
        /// ���[���}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="loan">���[���f�[�^(�o�^���e)</param>
        /// <returns>���[���}�X�^���f���N���X</returns>
        private Loan EditLoanForUpdate(Loan loan)
        {
            loan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.LastUpdateDate = DateTime.Now;
            return loan;
        }

    }
}
