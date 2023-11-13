using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.Linq;                 //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.SqlClient;            //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �Z�L�����e�B���[��
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SecurityRoleController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�Z�L�����e�B���[��";         // ��ʖ�
        private static readonly string PROC_NAME = "�Z�L�����e�B���[���ǉ��X�V"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SecurityRoleController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �Z�L�����e�B���[�����͉�ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            SecurityRole role = new SecurityRoleDao(db).GetByKey(id);
            if (role == null) {
                role = new SecurityRole();
                role.DelFlag = "0";
            } else {
                ViewData["update"] = "1";
            }
            SetDataComponent(role);
            return View("SecurityRoleEntry",role);
        }

        /// <summary>
        /// �Z�L�����e�B���[���ǉ��X�V
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SecurityRole role,FormCollection form)
        {
            if (string.IsNullOrEmpty(role.SecurityRoleCode)) {
                ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0001", "�Z�L�����e�B���[���R�[�h"));
            }
            if (string.IsNullOrEmpty(role.SecurityRoleName)) {
                ModelState.AddModelError("SecurityRoleName", MessageUtils.GetMessage("E0001", "�Z�L�����e�B���[����"));
            }
            if (!ModelState.IsValid) {
                SetDataComponent(role);
                return View("SecurityRoleEntry", role);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                if (string.IsNullOrEmpty(form["update"])) {
                    role.CreateDate = DateTime.Now;
                    role.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    role.LastUpdateDate = DateTime.Now;
                    role.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    db.SecurityRole.InsertOnSubmit(role);

                    //�A�v���P�[�V�������[���ǉ�
                    SetApplicationRole(role);
                    //�^�X�N���[���ǉ�
                    SetTaskRole(role);
                } else {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(role.DelFlag).Equals("1"))
                    {
                        //�A�v���P�[�V�������[���폜
                        DeleteApplicationRole(role);
                        //�^�X�N���[���폜
                        DeleteTaskRole(role);

                        //�Z�L�����e�B���[���폜
                        SecurityRole delRole = new SecurityRoleDao(db).GetByKey(role.SecurityRoleCode);
                        db.SecurityRole.DeleteOnSubmit(delRole);
                    } else {
                        //�X�V
                        SecurityRole target = new SecurityRoleDao(db).GetByKey(role.SecurityRoleCode);
                        UpdateModel(target);
                    }
                }

                // Add 2014/08/06 arc amii �G���[���O�Ή� �V����catch��(ChangeConflictException, SqlException)�ƃ��[�v����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException ce)
                    {
                        // �X�V���A�N���C�A���g�̓ǂݎ��ȍ~��DB�l���X�V���ꂽ���A���[�J���̒l��DB�l�ŏ㏑������
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        // ���g���C�񐔂𒴂����ꍇ�A�G���[�Ƃ���
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
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ��Ӑ���G���[�̏ꍇ�A���b�Z�[�W��ݒ肵�A�Ԃ�
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0010", new string[] { "�Z�L�����e�B���[���R�[�h", "�ۑ�" }));
                            SetDataComponent(role);
                            return View("SecurityRoleEntry", role);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // Mod 2014/08/06 arc amii �G���[���O�Ή� �uthrow e�v����G���[���O�o�͂��A�G���[��ʂɑJ�ڂ��鏈���ɕύX
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                } 
            }
            SetDataComponent(role);
            ViewData["close"] = "1";
            return View("SecurityRoleEntry",role);
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̍쐬
        /// </summary>
        /// <param name="role">�Z�L�����e�B���[��</param>
        private void SetDataComponent(SecurityRole role) {
            CodeDao dao = new CodeDao(db);
            ViewData["SecurityLevelList"] = CodeUtils.GetSelectListByModel(dao.GetSecurityLevelAll(false), "", false);
        }
        
        /// <summary>
        /// �Z�L�����e�B���[���R�[�h�����݂��邩�`�F�b�N����iAjax�p�j
        /// </summary>
        /// <param name="code">�Z�L�����e�B�R�[�h</param>
        /// <returns></returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SecurityRole role = new SecurityRoleDao(db).GetByKey(code);
                if (role != null) {
                    codeData.Code = role.SecurityRoleCode;
                    codeData.Name = role.SecurityRoleName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// �A�v���P�[�V�������[�����ꊇ�ǉ�����
        /// </summary>
        /// <param name="role">�Z�L�����e�B���[��</param>
        private void SetApplicationRole(SecurityRole role) {
            foreach (var a in new ApplicationDao(db).GetListAll()) {
                ApplicationRole app = new ApplicationRole();
                app.SecurityRoleCode = role.SecurityRoleCode;
                app.ApplicationCode = a.ApplicationCode;
                app.EnableFlag = false;
                app.CreateDate = DateTime.Now;
                app.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                app.LastUpdateDate = DateTime.Now;
                app.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                app.DelFlag = "0";
                db.ApplicationRole.InsertOnSubmit(app);
            }
        }

        /// <summary>
        /// �^�X�N���[�����ꊇ�ǉ�����
        /// </summary>
        /// <param name="role">�Z�L�����e�B���[��</param>
        private void SetTaskRole(SecurityRole role) {
            foreach (var t in new TaskConfigDao(db).GetAllList(true)) {
                TaskRole task = new TaskRole();
                task.EnableFlag = false;
                task.SecurityRoleCode = role.SecurityRoleCode;
                task.TaskConfigId = t.TaskConfigId;
                db.TaskRole.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �A�v���P�[�V�������[�����ꊇ�폜����
        /// </summary>
        /// <param name="role">�Z�L�����e�B���[��</param>
        private void DeleteApplicationRole(SecurityRole role) {
            List<ApplicationRole> delList = new ApplicationRoleDao(db).GetListBySecurityRole(role);
            db.ApplicationRole.DeleteAllOnSubmit<ApplicationRole>(delList);
        }

        /// <summary>
        /// �^�X�N���[�����ꊇ�폜����
        /// </summary>
        /// <param name="role">�Z�L�����e�B���[��</param>
        private void DeleteTaskRole(SecurityRole role) {
            List<TaskRole> delList = new TaskRoleDao(db).GetListBySecurityRole(role);
            db.TaskRole.DeleteAllOnSubmit<TaskRole>(delList);
        }
    }
}
