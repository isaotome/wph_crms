using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;                      //Add 2014/08/06 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TaskConfigController : Controller
    {
        //Add 2014/08/06 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�^�X�N�ݒ�";     // ��ʖ�
        private static readonly string PROC_NAME = "�^�X�N�ݒ�X�V"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TaskConfigController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �^�X�N�ݒ胊�X�g����������
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria(){
            return View("TaskConfigCriteria",new TaskConfigDao(db).GetAllList(true));
        }

        /// <summary>
        /// �^�X�N�ݒ���͉��
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry() {
            List<TaskConfig> list = new TaskConfigDao(db).GetAllList(true);
            if(list!=null && list.Count>0){
                ViewData["RoleCount"] = list[0].TaskRole.Count;
            }

            //foreach(var ret in list){
                //ret.SecurityLevelList = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetLevelListAll(), ret.SecurityLevelCode , false);
            return View("TaskConfigEntry",list);
        }

        /// <summary>
        /// �^�X�N�ݒ�X�V����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form) {
            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            List<TaskConfig> taskList = new TaskConfigDao(db).GetAllList(true);
            for (int i = 0; i < taskList.Count; i++) {
                taskList[i].PopUp = form[string.Format("task[{0}].{1}",i,"PopUp")];
                taskList[i].SecurityLevelCode = form[string.Format("task[{0}].{1}",i,"SecurityLevelCode")];
                taskList[i].DelFlag = form[string.Format("task[{0}].{1}", i, "DelFlag")].Contains("true") ? "0" : "1";
                for (int j = 0; j < taskList[i].TaskRole.Count; j++) {
                    taskList[i].TaskRole[j].EnableFlag = form[string.Format("task[{0}].role[{1}].{2}", i, j, "EnableFlag")].Contains("true") ? true : false;
                }
            }
            //Add 2014/08/05 arc amii �G���[���O�Ή� SubmitChanges��try����ǉ�
            try
            {
                db.SubmitChanges();
            }
            catch (Exception e)
            {
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ���O�ɏo��
                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                // �G���[�y�[�W�ɑJ��
                return View("Error");
            }
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            return View("TaskConfigEntry", taskList);
        }
    }
}
