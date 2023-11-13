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
    /// ����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepartmentController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public DepartmentController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// ���匟����ʕ\��
        /// </summary>
        /// <returns>���匟�����</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���匟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            //Del 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A closeMonthFlag�̔p�~
            //Add 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʔ��ʗp�̃t���O�ǉ�
            //int closeMonthFlag = 0;
            
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            //Mod 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʔ��ʗp�̃t���O�ǉ�
            PaginatedList<Department> list = GetSearchResultList(form);
            
            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���匟����ʂ̕\��
            return View("DepartmentCriteria", list);
        }

        /// <summary>
        /// ���匟���_�C�A���O�\��
        /// </summary>
        /// <returns>���匟���_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���匟���_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟����ʃ_�C�A���O</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �ԗ��I���t���O�A���i�I���t���O�ɂ��i�荞�ݏ����̒ǉ�
        /// 2015/06/11 arc yano ���̑� CriteriaDialog,CriteriaDialog2�̓���
        /// 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A ����e�[�u����CloseMonthFlag�̌����������A�N�G�������Ŏw��ł���悤�ɂ���
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            //Dell 2015/05/27 yano
            //Add 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʔ��ʗp�̃t���O�ǉ�
            //int closeMonthFlag = 0;
            
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            //Add 2017/05/10 arc yano #3762
            form["CarInventoryFlag"] = Request["CarInventoryFlag"];
            form["PartsInventoryFlag"] = Request["PartsInventoryFlag"];
            

            //Mod 2015/05/27
            form["CloseMonthFlag"] = Request["CloseMonthFlag"];

            //Add 2015/06/10
            form["SearchIsNot"] = Request["SearchIsNot"];

            // �������ʃ��X�g�̎擾
            //Mod 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʔ��ʗp�̃t���O�ǉ�
            PaginatedList<Department> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            //Add 2017/05/10 arc yano #3762
            ViewData["CarInventoryFlag"] = form["CarInventoryFlag"];
            ViewData["PartsInventoryFlag"] = form["PartsInventoryFlag"];

            // ���匟����ʂ̕\��
            return View("DepartmentCriteriaDialog", list);
        }

        //Mod 2015/06/11 arc yano ���匟���_�C�A���O�����̂��߁ACriteriaDialog2�̓R�����g�A�E�g
        /*
        //Add 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʗp�̌��������ǉ�
        /// <summary>
        /// ���匟���_�C�A���O�\��
        /// </summary>
        /// <returns>���匟���_�C�A���O</returns>
        public ActionResult CriteriaDialog2()
        {
            return CriteriaDialog2(new FormCollection());
        }

        //Mod 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A ����e�[�u����CloseMonthFlag�̌����������A�N�G�������Ŏw��ł���悤�ɂ���
        //Add 2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʗp�̌��������ǉ�
        /// <summary>
        /// ���匟���_�C�A���O�\���Q(�����ߏ����󋵉�ʐ�p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            //Del 2015/05/27
            //int closeMonthFlag = 1;
            
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            //Mod 2015/05/27
            form["CloseMonthFlag"] = "1";

            // �������ʃ��X�g�̎擾
            PaginatedList<Department> list = GetSearchResultList(form);

            ViewData["FormAction"] = form["FormAction"] = "/Department/CriteriaDialog2";

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // ���匟����ʂ̕\��
            return View("DepartmentCriteriaDialog", list);
        }

        //Add 2014/10/16 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �ԗ��d�����X�g��p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���匟���_�C�A���O�\��
        /// </summary>
        /// <returns>���匟���_�C�A���O</returns>
        public ActionResult CriteriaDialog3()
        {
            return CriteriaDialog3(new FormCollection());
        }
        */

        //Add 2014/10/16 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �ԗ��d�����X�g��p�̌����_�C�A���O�ǉ�
        /// <summary>
        /// ���匟���_�C�A���O�\���Q(�����ߏ����󋵉�ʐ�p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog3(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            // �������ʃ��X�g�̎擾
            PaginatedList<Department> list = GetSearchResultCarPurchaseList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // ���匟����ʂ̕\��
            return View("DepartmentCriteriaDialog2", list);
        }


        //Add 2014/12/08 arc nakayama ���i�݌Ɍ����Ή�
        /// <summary>
        /// ���匟���_�C�A���O�\���Q(�����ߏ����󋵉�ʐ�p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟����ʃ_�C�A���O</returns>
        //[AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog4(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            // �������ʃ��X�g�̎擾
            PaginatedList<Department> list = GetSearchResultListForParts(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // ���匟����ʂ̕\��
            return View("DepartmentCriteriaDialog", list);
        }


        //Add 2014/15/18 arc nakayama ���b�N�A�b�v�������Ή�
        /// <summary>
        /// ���匟���_�C�A���O�\��
        /// </summary>
        /// <returns>���匟���_�C�A���O</returns>
        public ActionResult CriteriaDialog5()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            return CriteriaDialog5(form);
        }
        
        //Add 2014/15/18 arc nakayama ���b�N�A�b�v�������Ή�
        /// <summary>
        /// ���匟���_�C�A���O�\���i����������DelFlag����j
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���匟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog5(FormCollection form)
        {
            if (criteriaInit)
            {
                form["DelFlag"] = "0";
                ViewData["DelFlag"] = form["DelFlag"];
                // �������ʃ��X�g�̎擾
                PaginatedList<Department> list = GetSearchResultListDelflag(form);
                // ���匟����ʂ̕\��
                return View("DepartmentCriteriaDialog3", list);
            }
            else
            {
                // ���������̐ݒ�
                // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
                //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
                form["CompanyCode"] = Request["CompanyCode"];
                form["CompanyName"] = Request["CompanyName"];
                form["OfficeCode"] = Request["OfficeCode"];
                form["OfficeName"] = Request["OfficeName"];
                form["DepartmentCode"] = Request["DepartmentCode"];
                form["DepartmentName"] = Request["DepartmentName"];
                form["EmployeeName"] = Request["EmployeeName"];
                form["DelFlag"] = Request["DelFlag"];
                form["BusinessType"] = Request["BusinessType"];

                // �������ʃ��X�g�̎擾
                PaginatedList<Department> list = GetSearchResultListDelflag(form);

                // ���̑��o�͍��ڂ̐ݒ�
                ViewData["CompanyCode"] = form["CompanyCode"];
                ViewData["CompanyName"] = form["CompanyName"];
                ViewData["OfficeCode"] = form["OfficeCode"];
                ViewData["OfficeName"] = form["OfficeName"];
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                ViewData["DepartmentName"] = form["DepartmentName"];
                ViewData["EmployeeName"] = form["EmployeeName"];
                ViewData["BusinessType"] = form["BusinessType"];
                ViewData["DelFlag"] = form["DelFlag"];

                // ���匟����ʂ̕\��
                return View("DepartmentCriteriaDialog3", list);
            }
        }




        /// <summary>
        /// ����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Department department;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                department = new Department();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                department = new DepartmentDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(department);

            // �o��
            return View("DepartmentEntry", department);
        }

        /// <summary>
        /// ����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="department">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Department department, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateDepartment(department);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(department);
                return View("DepartmentEntry", department);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                Department targetDepartment = new DepartmentDao(db).GetByKey(department.DepartmentCode, true);
                UpdateModel(targetDepartment);
                EditDepartmentForUpdate(targetDepartment);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                department = EditDepartmentForInsert(department);

                // �f�[�^�ǉ�
                db.Department.InsertOnSubmit(department);                
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

                        ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0010", new string[] { "����R�[�h", "�ۑ�" }));
                        GetEntryViewData(department);
                        return View("DepartmentEntry", department);
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

            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(department.DepartmentCode);
        }

        
        /// <summary>
        /// ����R�[�h���畔�喼���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2015/06/19 arc yano ���̑� includeDeleted�̃f�t�H���g�l��true��false�ɕύX
        /// 2015/05/27 arc yano IPO�Ή�(���i�I��) closeMonthFlag��ǉ�
        /// </history>
        public ActionResult GetMaster(string code, bool includeDeleted = false, string closeMonthFlag = "")
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted, closeMonthFlag);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;

                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ����R�[�h���畔������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetail(string code, bool includeDeleted = false)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted);

                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ����R�[�h���畔�喼���擾����(�ԗ��̂�) (Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster2(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByCarDepartment(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ����R�[�h������擾����(�ԗ��I���Ώە���̂�) (Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �V�K�쐬
        /// </history>
        public ActionResult GetMasterForCarInventory(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByCarInventory(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ����R�[�h����擾����(���i�I���Ώە���̂�) (Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �V�K�쐬
        /// </history>
        public ActionResult GetMasterForPartsInventory(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByPartsInventory(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ����R�[�h����f�t�H���g�d������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2018/07/27 yano.hiroki #3923 ����}�X�^�Ƀf�t�H���g�d�����ݒ�@�V�K�쐬
        /// </history>
        public ActionResult GetSupplierFromDepCode(string code, bool includeDeleted = false, string closeMonthFlag = "")
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted, closeMonthFlag);

                if (department != null)
                {
                    Supplier supp = new SupplierDao(db).GetByKey(department.DefaultSupplierCode);

                    if (supp != null)
                    {
                        codeData.Code = supp.SupplierCode;
                        codeData.Name = supp.SupplierName;
                    }
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="department">���f���f�[�^</param>
        /// <history>
        ///  2018/07/27 yano.hiroki #3923 ����}�X�^�Ƀf�t�H���g�d�����ݒ�
        /// </history>
        private void GetEntryViewData(Department department)
        {
            // �G���A���̎擾
            if (!string.IsNullOrEmpty(department.AreaCode))
            {
                AreaDao areaDao = new AreaDao(db);
                Area area = areaDao.GetByKey(department.AreaCode);
                if (area != null)
                {
                    ViewData["AreaName"] = area.AreaName;
                }
            }

            // ���Ə����̎擾
            if (!string.IsNullOrEmpty(department.OfficeCode))
            {
                OfficeDao officeDao = new OfficeDao(db);
                Office office = officeDao.GetByKey(department.OfficeCode);
                if (office != null)
                {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }

            // ���咷���̎擾
            if (!string.IsNullOrEmpty(department.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(department.EmployeeCode);
                if (employee != null)
                {
                    department.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // Add 2018/07/27 yano.hiroki #3923
            // ����̎d���於�̎擾
            if (!string.IsNullOrEmpty(department.DefaultSupplierCode))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(department.DefaultSupplierCode);
                if (supplier != null)
                {
                    ViewData["DefaultSupplierName"] = supplier.SupplierName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), department.DepositKind, true);
            ViewData["BusinessTypeList"] = CodeUtils.GetSelectListByModel(dao.GetBusinessTypeAll(false), department.BusinessType, true);

        }

       
        /// ����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>����}�X�^�������ʃ��X�g</returns>
        /// <history>
        ///  2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �V�K�쐬
        ///  2015/06/11 arc yano ���̑� CriteriaDialog,CriteriaDialog2�̓���
        ///  2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A �N�G���������CloseMonthFlag��ݒ�ł���悤�ɏC��
        ///  2014/09/08 arc yano IPO�Ή����̂Q �����ߏ����󋵉�ʔ��ʗp�̃t���O�ǉ�
        /// <summary>
        /// </history>
        private PaginatedList<Department> GetSearchResultList(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];

            //Mod 2015/05/27 arc yano
            departmentCondition.CloseMonthFlag = form["CloseMonthFlag"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            //Add 2017/05/10 arc yano #3762
            departmentCondition.PartsInventoryFlag = form["PartsInventoryFlag"];
            departmentCondition.CarInventoryFlag = form["CarInventoryFlag"];


            //Add 2015/06/11 arc yano
            //�������@�w��
            bool searchIsNot = bool.Parse(string.IsNullOrWhiteSpace(form["SearchIsNot"]) ? "false" : form["SearchIsNot"]);


            //Mod 2015/06/11 arc yano GetListByCondition, GetListByCondition2�̓���
            /*
            //Mod 2015/05/27 arc yano
            //Add 2014/09/08 arc yano �����ߏ����󋵉�ʂ���_�C�A���O���J�����ꍇ�́A����������ʂɂ���B
            if (string.IsNullOrWhiteSpace(departmentCondition.CloseMonthFlag) || departmentCondition.CloseMonthFlag.Equals("0"))
            {
            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
            else
            {
                return departmentDao.GetListByCondition2(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
            */

            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE, searchIsNot);
            
        }

        /// <summary>
        /// ����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>����}�X�^�������ʃ��X�g</returns>
        /// <history>
        /// 2014/10/16 arc amii �T�u�V�X�e���d��@�\�ڍs�Ή� �ԗ��d�����X�g��p�̌����_�C�A���O�ǉ�
        /// </history>
        private PaginatedList<Department> GetSearchResultCarPurchaseList(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByCondition3(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        
        //Add 2014/12/08 arc nakayama ���i�݌Ɍ����Ή�
        /// <summary>
        /// ����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Department> GetSearchResultListForParts(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByConditionForParts(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }

        //Add 2014/15/18 arc nakayama ���b�N�A�b�v�������Ή�
        /// <summary>
        /// ����}�X�^�������ʃ��X�g�擾�i����������DelFlag����j
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Department> GetSearchResultListDelflag(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }


        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="department">����f�[�^</param>
        /// <returns>����f�[�^</returns>
        private Department ValidateDepartment(Department department)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(department.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����R�[�h"));
            }
            if (string.IsNullOrEmpty(department.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", MessageUtils.GetMessage("E0001", "���喼"));
            }
            if (string.IsNullOrEmpty(department.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0001", "�G���A"));
            }
            if (string.IsNullOrEmpty(department.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "���Ə�"));
            }
            if (string.IsNullOrEmpty(department.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "���咷"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("DepartmentCode") && !CommonUtils.IsAlphaNumeric(department.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0012", "����R�[�h"));
            }

            return department;
        }

        /// <summary>
        /// ����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="department">����f�[�^(�o�^���e)</param>
        /// <returns>����}�X�^���f���N���X</returns>
        private Department EditDepartmentForInsert(Department department)
        {
            department.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            department.CreateDate = DateTime.Now;
            //department.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //department.LastUpdateDate = DateTime.Now;
            department.DelFlag = "0";
            return EditDepartmentForUpdate(department);
        }

        /// <summary>
        /// ����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="department">����f�[�^(�o�^���e)</param>
        /// <returns>����}�X�^���f���N���X</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �ԗ��I���t���O�A���i�I���t���O�̕ҏW�@�\�̒ǉ�
        /// </history>
        private Department EditDepartmentForUpdate(Department department)
        {
            department.PrintFlag = department.PrintFlag.Contains("true") ? "1" : "0";
            department.CarInventoryFlag = department.CarInventoryFlag.Contains("true") ? "1" : "0";     //Add  2017/05/10 arc yano #3762
            department.PartsInventoryFlag = department.PartsInventoryFlag.Contains("true") ? "1" : "0"; //Add  2017/05/10 arc yano #3762
            department.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            department.LastUpdateDate = DateTime.Now;
            return department;
        }

    }
}
