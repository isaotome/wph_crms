using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{

    //Mod 2014/07/30 arc yano �����̖������[�U�̏ꍇ�A�ԗ��`�[�̉���ԓo�^�󎆑オ�擾�ł��Ȃ��Ȃ邽��
    //                        ���[�U�F�؂��N���X�ł͂Ȃ��A���\�b�h���ɐݒ肷��B
    [ExceptionFilter]
    //[AuthFilter]        
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ConfigurationSettingController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�A�v���P�[�V�����ݒ�";     // ��ʖ�
        private static readonly string PROC_NAME = "�A�v���P�[�V�����ݒ�X�V"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ConfigurationSettingController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ���������͉��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]    //Add 2014/07/30 arc yano
        public ActionResult Criteria() {
            List<ConfigurationSetting> list = new ConfigurationSettingDao(db).GetListAll();
            return View("ConfigurationSettingCriteria", list);
        }

        [AuthFilter]    //Add 2014/07/30 arc yano
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(List<ConfigurationSetting> data) {
            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
            using (TransactionScope ts = new TransactionScope()) {
                foreach (var a in data) {
                    ConfigurationSetting target = dao.GetByKey(a.Code);
                    target.Value = a.Value;
                }

                // Mod 2014/08/05 arc amii �G���[���O�Ή� ChangeConflictException��ǉ����A�wthrow e�x���G���[�o�͏����ɕύX����
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
            return Criteria();
        }

        /// <summary>
        /// �A�v���P�[�V�����ݒ�l���擾����
        /// </summary>
        /// <param name="key">�ݒ�L�[</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetMasterDetail() {
            if (Request.IsAjaxRequest()) {
                ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
                List<ConfigurationSetting> settings = dao.GetListAll();
                Dictionary<string, string> ret = new Dictionary<string, string>();
                foreach (var a in settings) {
                    ret.Add(a.Code, a.Value);
                }
                if (ret.Count > 0) {
                    return Json(ret);
                }
            }
            return new EmptyResult();
        }
    }
}
