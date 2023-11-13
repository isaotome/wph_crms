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
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ��Ѓ}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0,VaryByParam="none")]
    public class CompanyController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "��Ѓ}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "��Ѓ}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CompanyController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ��Ќ�����ʕ\��
        /// </summary>
        /// <returns>��Ќ������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ��Ќ�����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>��Ќ������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Company> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ��Ќ�����ʂ̕\��
            return View("CompanyCriteria", list);
        }

        /// <summary>
        /// ��Ќ����_�C�A���O�\��
        /// </summary>
        /// <returns>��Ќ����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ��Ќ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>��Ќ�����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Company> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // ��Ќ�����ʂ̕\��
            return View("CompanyCriteriaDialog", list);
        }

        /// <summary>
        /// ��Ѓ}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">��ЃR�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>��Ѓ}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Company company;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                company = new Company();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                company = new CompanyDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(company);

            // �o��
            return View("CompanyEntry", company);
        }

        /// <summary>
        /// ��Ѓ}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="area">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>��Ѓ}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Company company, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCompany(company,form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(company);
                return View("CompanyEntry", company);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                Company targetCompany = new CompanyDao(db).GetByKey(company.CompanyCode, true);
                UpdateModel(targetCompany);
                EditCompanyForUpdate(targetCompany);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                company = EditCompanyForInsert(company);

                // �f�[�^�ǉ�
                db.Company.InsertOnSubmit(company);                
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��(�X�V�ƒǉ��œ����Ă���SubmitChanges�𓝍�)
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

                        ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0010", new string[] { "��ЃR�[�h", "�ۑ�" }));
                        GetEntryViewData(company);
                        return View("CompanyEntry", company);
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
                    // ��L�ȊO�̗�O�̏ꍇ�A�G���[���O�o�͂��A�G���[��ʂɑJ�ڂ���
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(company.CompanyCode);
        }

        /// <summary>
        /// ��ЃR�[�h�����Ж����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">��ЃR�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Company company = new CompanyDao(db).GetByKey(code);
                if (company != null)
                {
                    codeData.Code = company.CompanyCode;
                    codeData.Name = company.CompanyName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="company">���f���f�[�^</param>
        private void GetEntryViewData(Company company)
        {
            // ��\�Җ��̎擾
            if (!string.IsNullOrEmpty(company.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(company.EmployeeCode);
                if (employee != null)
                {
                    company.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }
        }

        /// <summary>
        /// ��Ѓ}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>��Ѓ}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Company> GetSearchResultList(FormCollection form)
        {
            CompanyDao companyDao = new CompanyDao(db);
            Company companyCondition = new Company();
            companyCondition.CompanyCode = form["CompanyCode"];
            companyCondition.CompanyName = form["CompanyName"];
            companyCondition.Employee = new Employee();
            companyCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                companyCondition.DelFlag = form["DelFlag"];
            }
            return companyDao.GetListByCondition(companyCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="company">��Ѓf�[�^</param>
        /// <returns>��Ѓf�[�^</returns>
        private Company ValidateCompany(Company company,FormCollection form)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(company.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "��ЃR�[�h"));
            }
            if (string.IsNullOrEmpty(company.CompanyName))
            {
                ModelState.AddModelError("CompanyName", MessageUtils.GetMessage("E0001", "��Ж�"));
            }
            if (string.IsNullOrEmpty(company.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "��\��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CompanyCode") && !CommonUtils.IsAlphaNumeric(company.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0012", "��ЃR�[�h"));
            }

            // �l�`�F�b�N
            if (!form["update"].Equals("1") && ModelState.IsValidField("CompanyCode") && !new SerialNumberDao(db).CanUserCompanyCode(company.CompanyCode)) {
                ModelState.AddModelError("CompanyCode", "�w�肳�ꂽ��ЃR�[�h�͊����̎ԗ��}�X�^�̃R�[�h�̌n�Əd�����邽�ߎg�p�ł��܂���");
            }
            return company;
        }

        /// <summary>
        /// ��Ѓ}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="company">��Ѓf�[�^(�o�^���e)</param>
        /// <returns>��Ѓ}�X�^���f���N���X</returns>
        private Company EditCompanyForInsert(Company company)
        {
            company.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.CreateDate = DateTime.Now;
            company.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.LastUpdateDate = DateTime.Now;
            company.DelFlag = "0";
            return company;
        }

        /// <summary>
        /// ��Ѓ}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="company">��Ѓf�[�^(�o�^���e)</param>
        /// <returns>��Ѓ}�X�^���f���N���X</returns>
        private Company EditCompanyForUpdate(Company company)
        {
            company.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.LastUpdateDate = DateTime.Now;
            return company;
        }

    }
}
