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
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �C�x���g�e�[�u���A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CampaignController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�C�x���g�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�C�x���g�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CampaignController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �C�x���g������ʕ\��
        /// </summary>
        /// <returns>�C�x���g�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �C�x���g������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�C�x���g�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Campaign> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["CampaignName"] = form["CampaignName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["CampaignStartDateFrom"] = form["CampaignStartDateFrom"];
            ViewData["CampaignStartDateTo"] = form["CampaignStartDateTo"];
            ViewData["CampaignEndDateFrom"] = form["CampaignEndDateFrom"];
            ViewData["CampaignEndDateTo"] = form["CampaignEndDateTo"];

            // �C�x���g������ʂ̕\��
            return View("CampaignCriteria", list);
        }

        /// <summary>
        /// �C�x���g�����_�C�A���O�\��
        /// </summary>
        /// <returns>�C�x���g�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �C�x���g�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�C�x���g������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CampaignCode"] = Request["CampaignCode"];
            form["CampaignName"] = Request["CampaignName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["CampaignStartDateFrom"] = Request["CampaignStartDateFrom"];
            form["CampaignStartDateTo"] = Request["CampaignStartDateTo"];
            form["CampaignEndDateFrom"] = Request["CampaignEndDateFrom"];
            form["CampaignEndDateTo"] = Request["CampaignEndDateTo"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Campaign> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["CampaignName"] = form["CampaignName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["CampaignStartDateFrom"] = form["CampaignStartDateFrom"];
            ViewData["CampaignStartDateTo"] = form["CampaignStartDateTo"];
            ViewData["CampaignEndDateFrom"] = form["CampaignEndDateFrom"];
            ViewData["CampaignEndDateTo"] = form["CampaignEndDateTo"];

            // �C�x���g������ʂ̕\��
            return View("CampaignCriteriaDialog", list);
        }

        /// <summary>
        /// �C�x���g�e�[�u�����͉�ʕ\��
        /// </summary>
        /// <param name="id">�C�x���g�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�C�x���g�e�[�u�����͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Campaign campaign;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                campaign = new Campaign();
                campaign.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                campaign = new CampaignDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(campaign);

            // �o��
            return View("CampaignEntry", campaign);
        }

        /// <summary>
        /// �C�x���g�e�[�u���ǉ��X�V
        /// </summary>
        /// <param name="campaign">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�C�x���g�e�[�u�����͉��</returns>
        /// <history>
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή� ModelState.Clear()�Ή�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Campaign campaign, EntitySet<CampaignCar> line, FormCollection form)
        {

            // Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // ���f���f�[�^�̕R�t��
            campaign.CampaignCar = line;

            // �s�ǉ��y�эs�폜����
            string delLine = form["DelLine"];
            if (!string.IsNullOrEmpty(delLine))
            {
                // �w��s�폜
                if (Int32.Parse(delLine) >= 0)
                {
                    campaign.CampaignCar.RemoveAt(Int32.Parse(delLine));
                }
                // �s�ǉ�
                else
                {
                    campaign.CampaignCar.Add(new CampaignCar());
                }

                // ���̑��\���f�[�^�̎擾
                GetEntryViewData(campaign);

                // �o��
                ModelState.Clear();
                return View("CampaignEntry", campaign);
            }

            // �f�[�^�`�F�b�N
            ValidateCampaign(campaign);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(campaign);
                return View("CampaignEntry", campaign);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // Mod 2014/08/05 arc amii �G���[���O�Ή� �ǉ��E�X�V�E�폜�̊e���\�b�h����SubmitChanges���ꂩ���ɏW��
            using (TransactionScope ts = new TransactionScope())
            {
                // �f�[�^�X�V����
                if (form["update"].Equals("1"))
                {
                    Campaign targetCampaign = new CampaignDao(db).GetByKey(campaign.CampaignCode);

                    // �C�x���g�e�[�u���̘_���폜
                    if (form["action"].Equals("delete"))
                    {
                        LogicalDeleteCampaign(targetCampaign);
                    }
                    // �C�x���g�e�[�u���y�уC�x���g�Ώێԗ��e�[�u���̃f�[�^�ҏW�E�X�V
                    else
                    {
                        targetCampaign.CampaignCar = campaign.CampaignCar;
                        UpdateCampaign(targetCampaign);
                    }
                }
                // �f�[�^�ǉ�����
                else
                {
                    // �C�x���g�e�[�u���y�уC�x���g�Ώێԗ��e�[�u���̃f�[�^�ǉ�
                    InsertCampaign(campaign);
                }

                // Add 2014/08/05 arc amii �G���[���O�Ή� DB�X�V�����̒ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // DB�A�N�Z�X�̎��s
                        db.SubmitChanges();
                        // �g�����U�N�V�����̃R�~�b�g
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
                            ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0010", new string[] { "�C�x���g�R�[�h", "�ۑ�" }));
                            GetEntryViewData(campaign);
                            return View("CampaignEntry", campaign);
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
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }
            
            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �C�x���g�R�[�h����C�x���g�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�C�x���g�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Campaign campaign = new CampaignDao(db).GetByKey(code);
                if (campaign != null)
                {
                    codeData.Code = campaign.CampaignCode;
                    codeData.Name = campaign.CampaignName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="campaign">���f���f�[�^</param>
        private void GetEntryViewData(Campaign campaign)
        {
            // ���[�����̎擾
            if (!string.IsNullOrEmpty(campaign.LoanCode))
            {
                LoanDao loanDao = new LoanDao(db);
                Loan loan = loanDao.GetByKey(campaign.LoanCode);
                if (loan != null)
                {
                    ViewData["LoanName"] = loan.LoanName;
                }
            }

            // �S���Җ��̎擾
            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employee;
            if (!string.IsNullOrEmpty(campaign.EmployeeCode))
            {
                employee = employeeDao.GetByKey(campaign.EmployeeCode);
                if (employee != null)
                {
                    campaign.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            //�Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["TargetServiceList"] = CodeUtils.GetSelectListByModel(dao.GetTargetServiceAll(false), campaign.TargetService, true);
            ViewData["CampaignTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCampaignTypeAll(false), campaign.CampaignType, true);

            // �O���[�h���X�g�̎擾
            List<CarGrade> carGradeList = new List<CarGrade>();
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade;
                for (int i = 0; i < campaign.CampaignCar.Count; i++)
                {
                    carGrade = carGradeDao.GetByKey(campaign.CampaignCar[i].CarGradeCode);
                    if (carGrade == null)
                    {
                        carGradeList.Add(new CarGrade());
                    }
                    else
                    {
                        carGradeList.Add(carGrade);
                    }
                }
            }
            ViewData["CarGradeList"] = carGradeList;
        }

        /// <summary>
        /// �C�x���g�e�[�u���������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�C�x���g�e�[�u���������ʃ��X�g</returns>
        private PaginatedList<Campaign> GetSearchResultList(FormCollection form)
        {
            CampaignDao campaignDao = new CampaignDao(db);
            Campaign campaignCondition = new Campaign();
            campaignCondition.CampaignCode = form["CampaignCode"];
            campaignCondition.CampaignName = form["CampaignName"];
            campaignCondition.CampaignStartDateFrom = CommonUtils.StrToDateTime(form["CampaignStartDateFrom"], DaoConst.SQL_DATETIME_MAX);
            campaignCondition.CampaignStartDateTo = CommonUtils.StrToDateTime(form["CampaignStartDateTo"], DaoConst.SQL_DATETIME_MIN);
            campaignCondition.CampaignEndDateFrom = CommonUtils.StrToDateTime(form["CampaignEndDateFrom"], DaoConst.SQL_DATETIME_MAX);
            campaignCondition.CampaignEndDateTo = CommonUtils.StrToDateTime(form["CampaignEndDateTo"], DaoConst.SQL_DATETIME_MIN);
            campaignCondition.Employee = new Employee();
            campaignCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                campaignCondition.DelFlag = form["DelFlag"];
            }

            return campaignDao.GetListByCondition(campaignCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^</param>
        /// <returns>�C�x���g�f�[�^</returns>
        private Campaign ValidateCampaign(Campaign campaign)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(campaign.CampaignCode))
            {
                ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0001", "�C�x���g�R�[�h"));
            }
            if (string.IsNullOrEmpty(campaign.CampaignName))
            {
                ModelState.AddModelError("CampaignName", MessageUtils.GetMessage("E0001", "�C�x���g��"));
            }
            if (string.IsNullOrEmpty(campaign.TargetService))
            {
                ModelState.AddModelError("TargetService", MessageUtils.GetMessage("E0001", "�ΏۋƖ�"));
            }
            if (string.IsNullOrEmpty(campaign.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                bool alreadyOutputNotNullMsg = false;
                for (int i = 0; i < campaign.CampaignCar.Count; i++)
                {
                    if (string.IsNullOrEmpty(campaign.CampaignCar[i].CarGradeCode))
                    {
                        if (alreadyOutputNotNullMsg)
                        {
                            ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", "");
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", MessageUtils.GetMessage("E0001", "�O���[�h�R�[�h"));
                            alreadyOutputNotNullMsg = true;
                        }
                    }
                }
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("PublishStartDate"))
            {
                ModelState.AddModelError("PublishStartDate", MessageUtils.GetMessage("E0005", "�f�ڊJ�n��"));
            }
            if (!ModelState.IsValidField("PublishEndDate"))
            {
                ModelState.AddModelError("PublishEndDate", MessageUtils.GetMessage("E0005", "�f�ڏI����"));
            }
            if (!ModelState.IsValidField("CampaignStartDate"))
            {
                ModelState.AddModelError("CampaignStartDate", MessageUtils.GetMessage("E0005", "�C�x���g�J�n��"));
            }
            if (!ModelState.IsValidField("CampaignEndDate"))
            {
                ModelState.AddModelError("CampaignEndDate", MessageUtils.GetMessage("E0005", "�C�x���g�I����"));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "��p", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CampaignCode") && !CommonUtils.IsAlphaNumeric(campaign.CampaignCode))
            {
                ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0012", "�C�x���g�R�[�h"));
            }
            if (ModelState.IsValidField("Cost") && campaign.Cost != null)
            {
                if (!Regex.IsMatch(campaign.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "��p", "���̐����̂�" }));
                }
            }

            // �d���`�F�b�N
            if (ModelState.IsValid && campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                bool alreadyOutputDuplicateMsg = false;
                for (int i = 0; i < campaign.CampaignCar.Count - 1; i++)
                {
                    for (int j = i + 1; j < campaign.CampaignCar.Count; j++)
                    {
                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        if (CommonUtils.DefaultString(campaign.CampaignCar[i].CarGradeCode).Equals(CommonUtils.DefaultString(campaign.CampaignCar[j].CarGradeCode)))
                        {
                            if (alreadyOutputDuplicateMsg)
                            {
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", "");
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(j) + "].CarGradeCode", "");
                            }
                            else
                            {
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(i) + "].CarGradeCode", MessageUtils.GetMessage("E0006", "�O���[�h�R�[�h"));
                                ModelState.AddModelError("line[" + CommonUtils.IntToStr(j) + "].CarGradeCode", "");
                                alreadyOutputDuplicateMsg = true;
                            }
                        }
                    }
                }
            }

            return campaign;
        }

        /// <summary>
        /// �C�x���g�e�[�u���ǉ�
        ///   ���C�x���g�Ώێԗ��e�[�u���������ɒǉ������B
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^</param>
        private void InsertCampaign(Campaign campaign)
        {
            // �f�[�^�ҏW
            campaign = EditCampaignForInsert(campaign);

            // �f�[�^�ǉ�
            db.Campaign.InsertOnSubmit(campaign);
            //db.SubmitChanges();
        }

        /// <summary>
        /// �C�x���g�e�[�u���X�V
        ///   ���C�x���g�Ώێԗ��e�[�u���������ɍ폜�{�ǉ������B
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^</param>
        private void UpdateCampaign(Campaign campaign)
        {
            //using (TransactionScope ts = new TransactionScope())
            //{
            // �C�x���g�Ώێԗ��e�[�u���̍폜
            List<CampaignCar> campaignCarList = new CampaignCarDao(db).GetByCampaign(campaign.CampaignCode);
            foreach (CampaignCar campaignCar in campaignCarList)
            {
                db.CampaignCar.DeleteOnSubmit(campaignCar);
            }

            // �C�x���g�e�[�u���̃f�[�^�ҏW�E�X�V
            // �y�уC�x���g�Ώێԗ��e�[�u���̃f�[�^�ҏW�E�ǉ�
            UpdateModel(campaign);
            EditCampaignForUpdate(campaign);

            //    // DB�A�N�Z�X�̎��s
            //    db.SubmitChanges();

            //    // �g�����U�N�V�����̃R�~�b�g
            //    ts.Complete();
            //}
        }

        /// <summary>
        /// �C�x���g�e�[�u���_���폜
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^</param>
        private void LogicalDeleteCampaign(Campaign campaign)
        {
            // �C�x���g�e�[�u���̘_���폜
            EditCampaignForLogicalDelete(campaign);
            //for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            //{
            //    EditCampaignForLogicalDelete(campaign);
            //    try
            //    {
            //        db.SubmitChanges();
            //        break;
            //    }
           
            //}
        }

        /// <summary>
        /// �C�x���g�e�[�u���ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^(�o�^���e)</param>
        /// <returns>�C�x���g�e�[�u�����f���N���X</returns>
        private Campaign EditCampaignForInsert(Campaign campaign)
        {
            campaign.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.CreateDate = DateTime.Now;
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            campaign.DelFlag = "0";
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                foreach (CampaignCar campaignCar in campaign.CampaignCar)
                {
                    campaignCar.CreateEmployeeCode = campaign.CreateEmployeeCode;
                    campaignCar.CreateDate = campaign.CreateDate;
                    campaignCar.LastUpdateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.LastUpdateDate = campaign.LastUpdateDate;
                    campaignCar.DelFlag = "0";
                }
            }
            return campaign;
        }

        /// <summary>
        /// �C�x���g�e�[�u���X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^(�o�^���e)</param>
        /// <returns>�C�x���g�e�[�u�����f���N���X</returns>
        private Campaign EditCampaignForUpdate(Campaign campaign)
        {
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            if (campaign.CampaignCar != null && campaign.CampaignCar.Count > 0)
            {
                foreach (CampaignCar campaignCar in campaign.CampaignCar)
                {
                    campaignCar.CreateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.CreateDate = campaign.LastUpdateDate;
                    campaignCar.LastUpdateEmployeeCode = campaign.LastUpdateEmployeeCode;
                    campaignCar.LastUpdateDate = campaign.LastUpdateDate;
                    campaignCar.DelFlag = "0";
                }
            }
            return campaign;
        }

        /// <summary>
        /// �C�x���g�e�[�u���_���폜�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="campaign">�C�x���g�f�[�^(�o�^���e)</param>
        /// <returns>�C�x���g�e�[�u�����f���N���X</returns>
        private Campaign EditCampaignForLogicalDelete(Campaign campaign)
        {
            campaign.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            campaign.LastUpdateDate = DateTime.Now;
            campaign.DelFlag = "1";
            return campaign;
        }
    }
}
