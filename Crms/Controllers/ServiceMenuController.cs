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
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �T�[�r�X���j���[�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceMenuController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�T�[�r�X���j���[�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�T�[�r�X���j���[�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceMenuController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �T�[�r�X���j���[������ʕ\��
        /// </summary>
        /// <returns>�T�[�r�X���j���[�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �T�[�r�X���j���[������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X���j���[�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<ServiceMenu> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["ServiceMenuCode"] = form["ServiceMenuCode"];
            ViewData["ServiceMenuName"] = form["ServiceMenuName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �T�[�r�X���j���[������ʂ̕\��
            return View("ServiceMenuCriteria", list);
        }

        /// <summary>
        /// �T�[�r�X���j���[�����_�C�A���O�\��
        /// </summary>
        /// <returns>�T�[�r�X���j���[�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �T�[�r�X���j���[�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X���j���[������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["ServiceMenuCode"] = Request["ServiceMenuCode"];
            form["ServiceMenuName"] = Request["ServiceMenuName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<ServiceMenu> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["ServiceMenuCode"] = form["ServiceMenuCode"];
            ViewData["ServiceMenuName"] = form["ServiceMenuName"];

            // �T�[�r�X���j���[������ʂ̕\��
            return View("ServiceMenuCriteriaDialog", list);
        }

        /// <summary>
        /// �T�[�r�X���j���[�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�T�[�r�X���j���[�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�T�[�r�X���j���[�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            ServiceMenu serviceMenu;

            // �\���f�[�^�ݒ�(�ǉ��̏ꍇ)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                serviceMenu = new ServiceMenu();
            }
            // �\���f�[�^�ݒ�(�X�V�̏ꍇ)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                serviceMenu = new ServiceMenuDao(db).GetByKey(id, true);
            }

            // �o��
            return View("ServiceMenuEntry", serviceMenu);
        }

        /// <summary>
        /// �T�[�r�X���j���[�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="serviceMenu">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�T�[�r�X���j���[�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceMenu serviceMenu, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateServiceMenu(serviceMenu);
            if (!ModelState.IsValid)
            {
                return View("ServiceMenuEntry", serviceMenu);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope())
            {
                // �f�[�^�X�V����
                if (form["update"].Equals("1"))
                {
                    // �f�[�^�ҏW�E�X�V
                    //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����
                    ServiceMenu targetServiceMenu = new ServiceMenuDao(db).GetByKey(serviceMenu.ServiceMenuCode, true);
                    UpdateModel(targetServiceMenu);
                    EditServiceMenuForUpdate(targetServiceMenu);
                }
                // �f�[�^�ǉ�����
                else
                {
                    // �f�[�^�ҏW
                    serviceMenu = EditServiceMenuForInsert(serviceMenu);
                    // �f�[�^�ǉ�
                    db.ServiceMenu.InsertOnSubmit(serviceMenu);
                }

                // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        if (form["update"].Equals("1"))
                        {
                            db.ChangeServiceCostMulti(serviceMenu.ServiceMenuCode, null, serviceMenu.DelFlag, serviceMenu.LastUpdateEmployeeCode);
                        }
                        else
                        {
                            db.AddServiceCostMulti(serviceMenu.ServiceMenuCode, null, serviceMenu.CreateEmployeeCode);
                        }
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
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0010", new string[] { "�T�[�r�X���j���[�R�[�h", "�ۑ�" }));
                            return View("ServiceMenuEntry", serviceMenu);
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
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(serviceMenu.ServiceMenuCode);
        }

        /// <summary>
        /// �T�[�r�X���j���[�R�[�h����T�[�r�X���j���[�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�T�[�r�X���j���[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                ServiceMenu serviceMenu = new ServiceMenuDao(db).GetByKey(code);
                if (serviceMenu != null)
                {
                    codeData.Code = serviceMenu.ServiceMenuCode;
                    codeData.Name = serviceMenu.ServiceMenuName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �T�[�r�X���j���[�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X���j���[�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<ServiceMenu> GetSearchResultList(FormCollection form)
        {
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            ServiceMenu serviceMenuCondition = new ServiceMenu();
            serviceMenuCondition.ServiceMenuCode = form["ServiceMenuCode"];
            serviceMenuCondition.ServiceMenuName = form["ServiceMenuName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                serviceMenuCondition.DelFlag = form["DelFlag"];
            }
            return serviceMenuDao.GetListByCondition(serviceMenuCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="serviceMenu">�T�[�r�X���j���[�f�[�^</param>
        /// <returns>�T�[�r�X���j���[�f�[�^</returns>
        private ServiceMenu ValidateServiceMenu(ServiceMenu serviceMenu)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(serviceMenu.ServiceMenuCode))
            {
                ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0001", "�T�[�r�X���j���[�R�[�h"));
            }
            if (string.IsNullOrEmpty(serviceMenu.ServiceMenuName))
            {
                ModelState.AddModelError("ServiceMenuName", MessageUtils.GetMessage("E0001", "�T�[�r�X���j���[��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("ServiceMenuCode") && !CommonUtils.IsAlphaNumeric(serviceMenu.ServiceMenuCode))
            {
                ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0012", "�T�[�r�X���j���[�R�[�h"));
            }

            return serviceMenu;
        }

        /// <summary>
        /// �T�[�r�X���j���[�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="serviceMenu">�T�[�r�X���j���[�f�[�^(�o�^���e)</param>
        /// <returns>�T�[�r�X���j���[�}�X�^���f���N���X</returns>
        private ServiceMenu EditServiceMenuForInsert(ServiceMenu serviceMenu)
        {
            serviceMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.CreateDate = DateTime.Now;
            serviceMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.LastUpdateDate = DateTime.Now;
            serviceMenu.DelFlag = "0";
            return serviceMenu;
        }

        /// <summary>
        /// �T�[�r�X���j���[�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="serviceMenu">�T�[�r�X���j���[�f�[�^(�o�^���e)</param>
        /// <returns>�T�[�r�X���j���[�}�X�^���f���N���X</returns>
        private ServiceMenu EditServiceMenuForUpdate(ServiceMenu serviceMenu)
        {
            serviceMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.LastUpdateDate = DateTime.Now;
            return serviceMenu;
        }

    }
}
