using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Collections;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [OutputCache(Duration=0,VaryByParam="null")]
    [AuthFilter]
    public class ApplicationRoleController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�A�v���P�[�V�������[��";     // ��ʖ�
        private static readonly string PROC_NAME = "�A�v���P�[�V�������[���o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ApplicationRoleController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �A�v���P�[�V�������[��������ʕ\��(�Z�L�����e�B���[�����X�g��\���j
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            SecurityRoleDao dao = new SecurityRoleDao(db);
            List<SecurityRole> list = dao.GetListAll();
            return View("ApplicationRoleCriteria",list);
        }

        /// <summary>
        /// �A�v���P�[�V�������[�����͉�ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry()
        {
            List<Application> header = new ApplicationDao(db).GetListAll();
            
            return View("ApplicationRoleEntry",header);
        }

        /// <summary>
        /// �A�v���P�[�V�������[���X�V
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form)
        {
            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //�Z�L�����e�B���x�����X�V����
            List<SecurityRole> secList = new SecurityRoleDao(db).GetListAll();
            foreach(var s in secList){
                s.SecurityLevelCode = form[string.Format("sec[{0}].SecurityLevelCode", s.SecurityRoleCode)];
            }

            //���j���[���X�g���擾����
            List<Application> menuList = new ApplicationDao(db).GetListAll();
            for (int i = 0; i < menuList.Count; i++) {
                for (int j = 0; j < menuList[i].ApplicationRole.Count; j++) {
                    menuList[i].ApplicationRole[j].EnableFlag = form[string.Format("role[{0}].ApplicationRole[{1}].{2}",i,j,"EnableFlag")].Contains("true") ? true : false;
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
            return View("ApplicationRoleEntry", menuList);
        }
    }
}
