using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;             //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.Linq;        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �Z�b�g���j���[�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SetMenuController : InheritedController
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�Z�b�g���j���[�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�Z�b�g���j���[�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SetMenuController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �Z�b�g���j���[�}�X�^������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �Z�b�g���j���[�}�X�^��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Z�b�g���j���[���X�g���</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            ViewData["SetMenuName"] = form["SetMenuName"];
            ViewData["CompanyCode"] = form["CompanyCode"];
            PaginatedList<SetMenu> list = GetSearchResult(form);
            return View("SetMenuCriteria", list);
        }

        /// <summary>
        /// �Z�b�g���j���[�}�X�^�����_�C�A���O�\��
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �Z�b�g���j���[�}�X�^��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Z�b�g���j���[���X�g���</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            ViewData["SetMenuName"] = form["SetMenuName"];
            ViewData["CompanyCode"] = form["CompanyCode"];
            PaginatedList<SetMenu> list = GetSearchResult(form);
            return View("SetMenuCriteriaDialog", list);
        }
        /// <summary>
        /// �Z�b�g���j���[���͉�ʕ\��
        /// </summary>
        /// <param name="id">�Z�b�g���j���[�R�[�h</param>
        /// <returns>�Z�b�g���j���[���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {
            SetMenu setMenu = new SetMenuDao(db).GetByKey(id);
            if (setMenu == null) {
                setMenu = new SetMenu();
            } else {
                ViewData["update"] = "1";
            }

            return View("SetMenuEntry", setMenu);
        }

        /// <summary>
        /// �Z�b�g���j���[�o�^�E�X�V����
        /// </summary>
        /// <param name="setMenu">�Z�b�g���j���[�f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Z�b�g���j���[���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SetMenu setMenu,FormCollection form) {
            ValidateSetMenu(setMenu);
            if (!ModelState.IsValid) {
            	//ADD�@2014/10/30�@ishii�@�ۑ��{�^���Ή��@
                ViewData["update"] = form["update"];
                SetDataComponent(form);
                return View("SetMenuEntry", setMenu);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                if (!string.IsNullOrEmpty(form["update"]) && form["update"].Equals("1")) {
                    SetMenu target = new SetMenuDao(db).GetByKey(setMenu.SetMenuCode);
                    UpdateModel<SetMenu>(target);
                } else {
                    setMenu.CreateDate = DateTime.Now;
                    setMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.LastUpdateDate = DateTime.Now;
                    setMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.DelFlag = "0";
                    db.SetMenu.InsertOnSubmit(setMenu);
                }

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
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetDataComponent(form);
                            return View("SetMenuEntry", setMenu);
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
                
            }
            //MOD 2014/10/29 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            //return View("SetMenuEntry", setMenu);
            return Entry(setMenu.SetMenuCode);
        }
        /// <summary>
        /// �Z�b�g���j���[�������ʎ擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Z�b�g���j���[���X�g</returns>
        private PaginatedList<SetMenu> GetSearchResult(FormCollection form) {
            SetMenu condition = new SetMenu();
            condition.SetMenuCode = form["SetMenuCode"];
            condition.SetMenuName = form["SetMenuName"];
            condition.CompanyCode = form["CompanyCode"];

            return new SetMenuDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// �Z�b�g���j���[�R�[�h����Z�b�g���j���[�����擾����(Ajax�p)
        /// </summary>
        /// <param name="code">�Z�b�g���j���[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�NULL�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SetMenu setMenu = new SetMenuDao(db).GetByKey(code);
                if (setMenu != null) {
                    codeData.Code = setMenu.SetMenuCode;
                    codeData.Name = setMenu.SetMenuName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="setMenu">�Z�b�g���j���[�f�[�^</param>
        private void ValidateSetMenu(SetMenu setMenu) {
            CommonValidate("SetMenuCode", "�Z�b�g���j���[�R�[�h", setMenu, true);
            CommonValidate("SetMenuName", "�Z�b�g���j���[��", setMenu, true);
            CommonValidate("CompanyCode", "��ЃR�[�h", setMenu, true);
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void SetDataComponent(FormCollection form) {
            
        }
    }
}
