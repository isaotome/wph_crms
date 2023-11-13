using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���ƃ}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceWorkController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���ƃ}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���ƃ}�X�^�o�^"; // ������

        //Add 2016/04/14 arc yano #3480 �T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� ���Ɓ̐�����R�[�h�i����
        private static readonly string CATEGORYCODE_CUSTOMERCLAIMCLASS = "016";         // �����敪��

        //�T�[�r�X�W�v�\�E�V�ԁE���Îԋ敪
        private static readonly string CATEGORYCODE_ACCOUNTCLASSCODE = "028";           //Add 2022/01/24 yano #4122

        private static readonly string APPLICATIONCODE_SERVICEMASTEREDIT = "ServiceMasterEdit";        //Add 2022/01/24 yano #4124

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceWorkController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���ƌ�����ʕ\��
        /// </summary>
        /// <returns>���ƌ������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���ƌ�����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���ƌ������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<ServiceWork> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), form["Classification1"], true);
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), form["Classification2"], true);
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["Name"] = form["Name"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���ƌ�����ʂ̕\��
            return View("ServiceWorkCriteria", list);
        }

        /// <summary>
        /// ���ƌ����_�C�A���O�\��
        /// </summary>
        /// <returns>���ƌ����_�C�A���O</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@����\�����ɑ啪�ނ��w�肳�ꂢ��ꍇ��form�ɐݒ肷��
        /// </history>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���ƌ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���ƌ�����ʃ_�C�A���O</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� �����悪���͂���Ă���ꍇ�́A������^�C�v�Ŏ��Ƃ��i�荞��
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["Classification1"] = Request["Classification1"];
            form["Classification2"] = Request["Classification2"];
            form["ServiceWorkCode"] = Request["ServiceWorkCode"];
            form["Name"] = Request["Name"];

            form["CCCustomerClaimClass"] = Request["CCCustomerClaimClass"];   //Add  2016/04/14 arc yano #3480

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<ServiceWork> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), form["Classification1"], true);

            //Mod 2016/04/14 arc yano #3480
            ViewData["CCCustomerClaimClass"] = form["CCCustomerClaimClass"];
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), form["Classification2"], true);
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["Name"] = form["Name"];

            // ���ƌ�����ʂ̕\��
            return View("ServiceWorkCriteriaDialog", list);
        }

        /// <summary>
        /// ���ƃ}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���ƃR�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���ƃ}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            ServiceWork serviceWork;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                serviceWork = new ServiceWork();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                serviceWork = new ServiceWorkDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(serviceWork);

            // �o��
            return View("ServiceWorkEntry", serviceWork);
        }

        /// <summary>
        /// ���ƃ}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="serviceWork">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���ƃ}�X�^���͉��</returns>
        /// <history>
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceWork serviceWork, FormCollection form)
        {
            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }
            

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateServiceWork(serviceWork);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(serviceWork);
                return View("ServiceWorkEntry", serviceWork);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                ServiceWork targetServiceWork = new ServiceWorkDao(db).GetByKey(serviceWork.ServiceWorkCode, true);
                UpdateModel(targetServiceWork);
                EditServiceWorkForUpdate(targetServiceWork);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                serviceWork = EditServiceWorkForInsert(serviceWork);

                // �f�[�^�ǉ�
                db.ServiceWork.InsertOnSubmit(serviceWork);
                
            }
            // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
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

                        ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0010", new string[] { "���ƃR�[�h(������)", "�ۑ�" }));
                        GetEntryViewData(serviceWork);
                        return View("ServiceWorkEntry", serviceWork);
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

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(serviceWork.ServiceWorkCode);
        }

        /// <summary>
        /// ���ƃR�[�h������Ɩ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���ƃR�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                ServiceWork serviceWork = new ServiceWorkDao(db).GetByKey(code);
                if (serviceWork != null)
                {
                    codeData.Code = serviceWork.ServiceWorkCode;
                    codeData.Name = serviceWork.Name;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// ���ƃf�[�^�̎擾
        /// </summary>
        /// <param name="code">���ƃR�[�h</param>
        /// <history>
        /// Mod 2016/04/15 arc yano #3480 �擾������Ƃ̏��Ɂu�����敪�ށv��ǉ�
        /// </history>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                ServiceWork work = new ServiceWorkDao(db).GetByKey(code);
                if (work != null) {
                    ret.Add("ServiceWorkCode", work.ServiceWorkCode);
                    ret.Add("ServiceWorkName", work.Name);
                    ret.Add("Classification1", work.Classification1);
                    ret.Add("ClassificationName1", work.c_ServiceWorkClass1.Name);
                    ret.Add("CustomerClaimClass", (work.CustomerClaimClass ?? ""));   //Add 2016/04/15 arc yano #3480
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="serviceWork">���f���f�[�^</param>
        /// <histroy>
        /// 2022/01/24 yano #4124�y���ƃ}�X�^���́z�����ɂ��ۑ��@�\�̐����̎���
        /// 2022/01/24 yano #4122�y���ƃ}�X�^���́z�V�ԁE���ÎԂ�I���ł��鍀�ڂ̒ǉ�
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� ���Ɓ̐�����R�[�h�i����
        /// </histroy>
        private void GetEntryViewData(ServiceWork serviceWork)
        {
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), serviceWork.Classification1, true);
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), serviceWork.Classification2, true);

            ViewData["CustomerClaimClassList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORYCODE_CUSTOMERCLAIMCLASS,false), serviceWork.CustomerClaimClass, true);  //Add 2016/04/14 arc yano #3480

            ViewData["AccountClassList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORYCODE_ACCOUNTCLASSCODE,false), serviceWork.AccountClassCode, false);  //Add 2022/01/24 yano #4122

            //Add #4124 yano
            //�T�[�r�X�}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_SERVICEMASTEREDIT).EnableFlag;

        }

        /// <summary>
        /// ���ƃ}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���ƃ}�X�^�������ʃ��X�g</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� ���Ɓ̐�����R�[�h�i����
        /// </history>
        private PaginatedList<ServiceWork> GetSearchResultList(FormCollection form)
        {
            ServiceWorkDao serviceWorkDao = new ServiceWorkDao(db);
            ServiceWork serviceWorkCondition = new ServiceWork();
            serviceWorkCondition.ServiceWorkCode = form["ServiceWorkCode"];
            serviceWorkCondition.Name = form["Name"];
            serviceWorkCondition.Classification1 = form["Classification1"];
            serviceWorkCondition.Classification2 = form["Classification2"];
            serviceWorkCondition.CustomerClaimClass = form["CCCustomerClaimClass"];     //Add 2016/04/14 arc yano #3480

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                serviceWorkCondition.DelFlag = form["DelFlag"];
            }
            return serviceWorkDao.GetListByCondition(serviceWorkCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="serviceWork">���ƃf�[�^</param>
        /// <returns>���ƃf�[�^</returns>
        private ServiceWork ValidateServiceWork(ServiceWork serviceWork)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(serviceWork.ServiceWorkCode))
            {
                ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0001", "���ƃR�[�h(������)"));
            }
            if (string.IsNullOrEmpty(serviceWork.Name))
            {
                ModelState.AddModelError("Name", MessageUtils.GetMessage("E0001", "���Ɩ�(������)"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Price"))
            {
                ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { "�T�[�r�X����", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("ServiceWorkCode") && !CommonUtils.IsAlphaNumeric(serviceWork.ServiceWorkCode))
            {
                ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0012", "���ƃR�[�h(������)"));
            }
            if (ModelState.IsValidField("Price") && serviceWork.Price != null)
            {
                if (!Regex.IsMatch(serviceWork.Price.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { "�T�[�r�X����", "���̐����̂�" }));
                }
            }

            return serviceWork;
        }

        /// <summary>
        /// ���ƃ}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="serviceWork">���ƃf�[�^(�o�^���e)</param>
        /// <returns>���ƃ}�X�^���f���N���X</returns>
        private ServiceWork EditServiceWorkForInsert(ServiceWork serviceWork)
        {
            serviceWork.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.CreateDate = DateTime.Now;
            serviceWork.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.LastUpdateDate = DateTime.Now;
            serviceWork.DelFlag = "0";
            return serviceWork;
        }

        /// <summary>
        /// ���ƃ}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="serviceWork">���ƃf�[�^(�o�^���e)</param>
        /// <returns>���ƃ}�X�^���f���N���X</returns>
        private ServiceWork EditServiceWorkForUpdate(ServiceWork serviceWork)
        {
            serviceWork.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.LastUpdateDate = DateTime.Now;
            return serviceWork;
        }

    }
}
