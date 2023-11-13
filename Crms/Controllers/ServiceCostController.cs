using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �T�[�r�X�H���\�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceCostController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�T�[�r�X�H���\�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�T�[�r�X�H���\�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceCostController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �T�[�r�X�H���\������ʕ\��
        /// </summary>
        /// <returns>�T�[�r�X�H���\�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �T�[�r�X�H���\������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X�H���\�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �T�[�r�X���j���[���X�g�̎擾
            List<ServiceMenu> list = new ServiceMenuDao(db).GetAll(false);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["DelFlag"] = form["DelFlag"];

            // �T�[�r�X�H���\������ʂ̕\��
            return View("ServiceCostCriteria", list);
        }

        /// <summary>
        /// �T�[�r�X�H���\�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�T�[�r�X�H���\�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�T�[�r�X�H���\�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            // �\���f�[�^�ݒ�
            GetEntryDisplayData(null, null);

            // �o��
            return View("ServiceCostEntry");
        }

        /// <summary>
        /// �T�[�r�X�H���\�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="serviceCost">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�T�[�r�X�H���\�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form)
        {
            // �f�[�^�`�F�b�N
            List<string> errorItemList = ValidateServiceCost(form);
            if (errorItemList.Count > 0)
            {
                GetEntryDisplayData(form, errorItemList);
                return View("ServiceCostEntry");
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�o�^����
            ServiceCostDao serviceCostDao = new ServiceCostDao(db);
            
            foreach (string key in form.AllKeys)
            {
                string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                if (keyArr[0].Equals("Cost"))
                {
                    ServiceCost serviceCost = serviceCostDao.GetByKey(keyArr[1], keyArr[2]);

                    // �f�[�^�ǉ�
                    if (serviceCost == null)
                    {
                        db.AddServiceCost(keyArr[1], keyArr[2], 0m, ((Employee)Session["Employee"]).EmployeeCode);
                    }
                    // �f�[�^�X�V
                    else
                    {
                        EditServiceCostForUpdate(serviceCost, decimal.Parse(form[key]));
                        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                        {
                            try
                            {
                                db.SubmitChanges();
                                break;
                            }
                            catch (ChangeConflictException e)
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
                                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
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
                }
            }
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �o�^��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="errorItemList">�G���[���ڃ��X�g</param>
        private void GetEntryDisplayData(FormCollection form, List<string> errorItemList)
        {
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            CarClassDao carClassDao = new CarClassDao(db);

            ViewData["ServiceMenuList"] = serviceMenuDao.GetAll(false);
            ViewData["CarClassList"] = carClassDao.GetAll(false);
            if (form == null)
            {
                ServiceCostDao serviceCostDao = new ServiceCostDao(db);
                ViewData["ServiceCostDic"] = serviceCostDao.GetAll(true);
            }
            else
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (string key in form.AllKeys)
                {
                    string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                    if (keyArr[0].Equals("Cost"))
                    {
                        dic.Add(keyArr[1] + "_" + keyArr[2], form[key]);
                    }
                }
                ViewData["ServiceCostDic"] = dic;
            }
            if (errorItemList == null)
            {
                ViewData["ErrorItemList"] = new List<string>();
            }
            else
            {
                ViewData["ErrorItemList"] = errorItemList;
            }
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="serviceCost">�T�[�r�X�H���\�f�[�^</param>
        /// <returns>�G���[���ڃ��X�g</returns>
        private List<string> ValidateServiceCost(FormCollection form)
        {
            List<string> ret = new List<string>();
            foreach (string key in form.AllKeys)
            {
                string msg = MessageUtils.GetMessage("E0002", new string[] { "�H��", "���̐���3���ȓ�������2���ȓ�" });
                string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                if (keyArr[0].Equals("Cost"))
                {
                    // �K�{�`�F�b�N
                    if (string.IsNullOrEmpty(form[key]))
                    {
                        if (ret.Count == 0)
                        {
                            ModelState.AddModelError("", msg);
                        }
                        ret.Add(key);
                        continue;
                    }

                    // �t�H�[�}�b�g�`�F�b�N
                    if ((Regex.IsMatch(form[key], @"^\d{1,3}\.\d{1,2}$"))
                        || (Regex.IsMatch(form[key], @"^\d{1,3}$")))
                    {
                    }
                    else
                    {
                        if (ret.Count == 0)
                        {
                            ModelState.AddModelError("", msg);
                        }
                        ret.Add(key);
                        continue;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �T�[�r�X�H���\�}�X�^�ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="serviceMenuCode">�T�[�r�X���j���[�R�[�h</param>
        /// <param name="carClassCode">�ԗ��N���X�R�[�h</param>
        /// <param name="cost">�H��</param>
        /// <returns>�T�[�r�X�H���\�}�X�^���f���N���X</returns>
        private ServiceCost EditServiceCostForInsert(string serviceMenuCode, string carClassCode, decimal cost)
        {
            ServiceCost serviceCost = new ServiceCost();
            serviceCost.ServiceMenuCode = serviceMenuCode;
            serviceCost.CarClassCode = carClassCode;
            serviceCost.Cost = cost;
            serviceCost.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.CreateDate = DateTime.Now;
            serviceCost.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.LastUpdateDate = DateTime.Now;
            serviceCost.DelFlag = "0";
            return serviceCost;
        }

        /// <summary>
        /// �T�[�r�X�H���\�}�X�^�X�V�f�[�^�ҏW
        /// </summary>
        /// <param name="serviceCost">�T�[�r�X�H���\�f�[�^(�X�V�O���e)</param>
        /// <param name="cost">�H��</param>
        /// <returns>�T�[�r�X�H���\�}�X�^���f���N���X</returns>
        private ServiceCost EditServiceCostForUpdate(ServiceCost serviceCost, decimal cost)
        {
            serviceCost.Cost = cost;
            serviceCost.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.LastUpdateDate = DateTime.Now;
            return serviceCost;
        }

        /// <summary>
        /// �O���[�h�R�[�h�A�T�[�r�X���j���[�R�[�h����H�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="gradeCode">�O���[�h�R�[�h</param>
        /// <param name="menuCode">�T�[�r�X���j���[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetail(string gradeCode,string menuCode) {

            if (Request.IsAjaxRequest()) {
                string serviceMenuCode = "";
                string serviceMenuName = "";
                string costRate = "";
                Dictionary<string, string> retCost = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(menuCode)) {
                    ServiceMenu menu = new ServiceMenuDao(db).GetByKey(menuCode);
                    if (menu != null) {
                        serviceMenuCode = menu.ServiceMenuCode;
                        serviceMenuName = menu.ServiceMenuName;
                    }
                    CarGrade grade = new CarGradeDao(db).GetByKey(gradeCode);
                    if (grade != null) {
                        string carClass = grade.CarClassCode;

                        ServiceCost cost = new ServiceCostDao(db).GetByKey(menuCode, carClass);
                        if (cost != null) {
                            costRate = cost.Cost.ToString();
                        }
                    }
                }
                retCost.Add("ServiceMenuName", serviceMenuName);
                retCost.Add("ServiceMenuCode", serviceMenuCode);
                retCost.Add("Cost", costRate);
                return Json(retCost);
            }
            return new EmptyResult();
        }
    }
}
