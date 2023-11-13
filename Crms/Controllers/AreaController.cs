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
    /// �G���A�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class AreaController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�G���A�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�G���A�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public AreaController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �G���A������ʕ\��
        /// </summary>
        /// <returns>�G���A�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �G���A������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�G���A�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Area> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["AreaCode"] = form["AreaCode"];
            ViewData["AreaName"] = form["AreaName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �G���A������ʂ̕\��
            return View("AreaCriteria", list);
        }

        /// <summary>
        /// �G���A�����_�C�A���O�\��
        /// </summary>
        /// <returns>�G���A�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �G���A�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�G���A������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["AreaCode"] = Request["AreaCode"];
            form["AreaName"] = Request["AreaName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Area> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["AreaCode"] = form["AreaCode"];
            ViewData["AreaName"] = form["AreaName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // �G���A������ʂ̕\��
            return View("AreaCriteriaDialog", list);
        }

        /// <summary>
        /// �G���A�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�G���A�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�G���A�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Area area;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                area = new Area();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                area = new AreaDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(area);

            // �o��
            return View("AreaEntry", area);
        }

        /// <summary>
        /// �G���A�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="area">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�G���A�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Area area, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateArea(area);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(area);
                return View("AreaEntry", area);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                Area targetArea = new AreaDao(db).GetByKey(area.AreaCode, true);
                UpdateModel(targetArea);
                EditAreaForUpdate(targetArea);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                area = EditAreaForInsert(area);

                // �f�[�^�ǉ�
                db.Area.InsertOnSubmit(area);                
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

                        ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0010", new string[] { "�G���A�R�[�h", "�ۑ�" }));
                        GetEntryViewData(area);
                        return View("AreaEntry", area);
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
            return Entry(area.AreaCode);
        }

        /// <summary>
        /// �G���A�R�[�h����G���A�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�G���A�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Area area = new AreaDao(db).GetByKey(code);
                if (area != null)
                {
                    codeData.Code = area.AreaCode;
                    codeData.Name = area.AreaName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="area">���f���f�[�^</param>
        private void GetEntryViewData(Area area)
        {
            // �G���A�����̎擾
            if (!string.IsNullOrEmpty(area.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(area.EmployeeCode);
                if (employee != null)
                {
                    area.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }
        }

        /// <summary>
        /// �G���A�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�G���A�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Area> GetSearchResultList(FormCollection form)
        {
            AreaDao areaDao = new AreaDao(db);
            Area areaCondition = new Area();
            areaCondition.AreaCode = form["AreaCode"];
            areaCondition.AreaName = form["AreaName"];
            areaCondition.Employee = new Employee();
            areaCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                areaCondition.DelFlag = form["DelFlag"];
            }
            return areaDao.GetListByCondition(areaCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="area">�G���A�f�[�^</param>
        /// <returns>�G���A�f�[�^</returns>
        private Area ValidateArea(Area area)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(area.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0001", "�G���A�R�[�h"));
            }
            if (string.IsNullOrEmpty(area.AreaName))
            {
                ModelState.AddModelError("AreaName", MessageUtils.GetMessage("E0001", "�G���A��"));
            }
            if (string.IsNullOrEmpty(area.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�G���A��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("AreaCode") && !CommonUtils.IsAlphaNumeric(area.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0012", "�G���A�R�[�h"));
            }

            return area;
        }

        /// <summary>
        /// �G���A�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="area">�G���A�f�[�^(�o�^���e)</param>
        /// <returns>�G���A�}�X�^���f���N���X</returns>
        private Area EditAreaForInsert(Area area)
        {
            area.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.CreateDate = DateTime.Now;
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.LastUpdateDate = DateTime.Now;
            area.DelFlag = "0";
            return area;
        }

        /// <summary>
        /// �G���A�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="area">�G���A�f�[�^(�o�^���e)</param>
        /// <returns>�G���A�}�X�^���f���N���X</returns>
        private Area EditAreaForUpdate(Area area)
        {
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.LastUpdateDate = DateTime.Now;
            return area;
        }

    }
}
