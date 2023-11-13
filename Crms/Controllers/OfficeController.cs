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
    /// ���Ə��}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class OfficeController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���Ə��}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���Ə��}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public OfficeController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���Ə�������ʕ\��
        /// </summary>
        /// <returns>���Ə��������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���Ə�������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ə��������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Office> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���Ə�������ʂ̕\��
            return View("OfficeCriteria", list);
        }

        /// <summary>
        /// ���Ə������_�C�A���O�\��
        /// </summary>
        /// <returns>���Ə������_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���Ə������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ə�������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["EmployeeName"] = Request["EmployeeName"];

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Office> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // ���Ə�������ʂ̕\��
            return View("OfficeCriteriaDialog", list);
        }

        //Add 2014/10/27 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �����s�f�[�^�o�͉�ʐ�p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���Ə������_�C�A���O�\�� (�����s�f�[�^�o�͉�ʐ�p)
        /// </summary>
        /// <returns>���Ə������_�C�A���O</returns>
        public ActionResult CriteriaDialog2()
        {
            return CriteriaDialog2(new FormCollection());
        }

        //Add 2014/10/27 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �����s�f�[�^�o�͉�ʐ�p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���Ə������_�C�A���O�\�� (�����s�f�[�^�o�͉�ʐ�p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ə�������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {

            string divisionType = CommonUtils.DefaultString(Request["DivisionType"]);
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DivisionType"] = divisionType;

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Office> list = GetSearchDivisionResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DivisionType"] = form["DivisionType"];

            // ���Ə�������ʂ̕\��
            return View("OfficeCriteriaDialog2", list);
        }

        /// <summary>
        /// ���Ə��}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���Ə��R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���Ə��}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Office office;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                office = new Office();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                office = new OfficeDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(office);

            // �o��
            return View("OfficeEntry", office);
        }

        /// <summary>
        /// ���Ə��}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="area">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���Ə��}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Office office, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateOffice(office);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(office);
                return View("OfficeEntry", office);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                Office targetOffice = new OfficeDao(db).GetByKey(office.OfficeCode, true);
                UpdateModel(targetOffice);
                EditOfficeForUpdate(targetOffice);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                office = EditOfficeForInsert(office);

                // �f�[�^�ǉ�
                db.Office.InsertOnSubmit(office);
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

                        ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0010", new string[] { "���Ə��R�[�h", "�ۑ�" }));
                        GetEntryViewData(office);
                        return View("OfficeEntry", office);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error"); ;
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

            //MOD 2014/10/29 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(office.OfficeCode);
        }

        /// <summary>
        /// ���Ə��R�[�h���玖�Ə������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���Ə��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Office office = new OfficeDao(db).GetByKey(code);
                if (office != null)
                {
                    codeData.Code = office.OfficeCode;
                    codeData.Name = office.OfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        //Add 2014/10/27 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �����s�f�[�^�o�͉�ʐ�p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���Ə��R�[�h���玖�Ə������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���Ə��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster2(string code, string divType)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Office office = new OfficeDao(db).GetByDivisionKey(code, divType);
                if (office != null)
                {
                    codeData.Code = office.OfficeCode;
                    codeData.Name = office.OfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="office">���f���f�[�^</param>
        private void GetEntryViewData(Office office)
        {
            // ��Ж��̎擾
            if (!string.IsNullOrEmpty(office.CompanyCode))
            {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(office.CompanyCode);
                if (company != null)
                {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }

            // ���Ə������̎擾
            if (!string.IsNullOrEmpty(office.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(office.EmployeeCode);
                if (employee != null)
                {
                    office.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), office.DepositKind, true);
        }

        /// <summary>
        /// ���Ə��}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ə��}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Office> GetSearchResultList(FormCollection form)
        {
            OfficeDao officeDao = new OfficeDao(db);
            Office officeCondition = new Office();
            officeCondition.OfficeCode = form["OfficeCode"];
            officeCondition.OfficeName = form["OfficeName"];
            officeCondition.Employee = new Employee();
            officeCondition.Employee.EmployeeName = form["EmployeeName"];
            officeCondition.Company = new Company();
            officeCondition.Company.CompanyCode = form["CompanyCode"];
            officeCondition.Company.CompanyName = form["CompanyName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                officeCondition.DelFlag = form["DelFlag"];
            }
            return officeDao.GetListByCondition(officeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Add 2014/10/27 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �����s�f�[�^�o�͉�ʐ�p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���Ə��}�X�^�������ʃ��X�g�擾(���_�w��)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���Ə��}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Office> GetSearchDivisionResultList(FormCollection form)
        {
            OfficeDao officeDao = new OfficeDao(db);
            Office officeCondition = new Office();
            officeCondition.OfficeCode = form["OfficeCode"];
            officeCondition.OfficeName = form["OfficeName"];
            officeCondition.Employee = new Employee();
            officeCondition.Employee.EmployeeName = form["EmployeeName"];
            officeCondition.Company = new Company();
            officeCondition.Company.CompanyCode = form["CompanyCode"];
            officeCondition.Company.CompanyName = form["CompanyName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                officeCondition.DelFlag = form["DelFlag"];
            }
            return officeDao.GetListDivCondition(officeCondition, form["DivisionType"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="office">���Ə��f�[�^</param>
        /// <returns>���Ə��f�[�^</returns>
        private Office ValidateOffice(Office office)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(office.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "���Ə��R�[�h"));
            }
            if (string.IsNullOrEmpty(office.OfficeName))
            {
                ModelState.AddModelError("OfficeName", MessageUtils.GetMessage("E0001", "���Ə���"));
            }
            if (string.IsNullOrEmpty(office.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "���"));
            }
            if (string.IsNullOrEmpty(office.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "���Ə���"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("OfficeCode") && !CommonUtils.IsAlphaNumeric(office.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0012", "���Ə��R�[�h"));
            }

            return office;
        }

        /// <summary>
        /// ���Ə��}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="office">���Ə��f�[�^(�o�^���e)</param>
        /// <returns>���Ə��}�X�^���f���N���X</returns>
        private Office EditOfficeForInsert(Office office)
        {
            office.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            office.CreateDate = DateTime.Now;
            //office.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //office.LastUpdateDate = DateTime.Now;
            office.DelFlag = "0";
            return EditOfficeForUpdate(office);
        }

        /// <summary>
        /// ���Ə��}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="office">���Ə��f�[�^(�o�^���e)</param>
        /// <returns>���Ə��}�X�^���f���N���X</returns>
        private Office EditOfficeForUpdate(Office office)
        {
            //office.PrintFlagCar = office.PrintFlagCar.Contains("true") ? "1" : "0";
            //office.PrintFlagService = office.PrintFlagService.Contains("true") ? "1" : "0";
            office.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            office.LastUpdateDate = DateTime.Now;
            return office;
        }

    }
}
