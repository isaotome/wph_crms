using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;                 //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CostAreaController : InheritedController
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "����p�ݒ�G���A";     // ��ʖ�
        private static readonly string PROC_NAME = "����p�ݒ�G���A�o�^"; // ������

        private static readonly string APPLICATIONCODE_MASTEREDIT = "MasterEdit";         //Add 2022/01/27 yano #4126

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CostAreaController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ����p�ݒ�G���A������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ����p�ݒ�G���A��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>����p�ݒ�G���A��������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            CostArea condition = new CostArea();
            PaginatedList<CostArea> list = new CostAreaDao(db).GetListByCondition(condition,int.Parse(form["id"] ?? "0"),DaoConst.PAGE_SIZE);
            return View("CostAreaCriteria", list);
        }
        /// <summary>
        /// ����p�ݒ�G���A�����_�C�A���O�\��
        /// </summary>
        /// <returns>����p�ݒ�G���A�����_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ����p�ݒ�G���A�����_�C�A���O��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>����p�ݒ�G���A��������</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            CostArea condition = new CostArea();
            PaginatedList<CostArea> list = new CostAreaDao(db).GetListByCondition(condition,int.Parse(form["id"] ?? "0"),DaoConst.PAGE_SIZE);
            return View("CostAreaCriteriaDialog", list);
        }
        /// <summary>
        /// ����p�ݒ�G���A���͉�ʕ\��
        /// </summary>
        /// <param name="id">����p�ݒ�G���A�R�[�h</param>
        /// <returns>����p�ݒ�G���A���͉��</returns>
        /// <histroy>
        /// 2022/01/27 yano #4126�y����p�ݒ�G���A�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CostArea area = new CostArea();
            if (!string.IsNullOrEmpty(id)) {
                area = new CostAreaDao(db).GetByKey(id);
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(area);			//Add 2022/01/27 yano #4126
            return View("CostAreaEntry", area);
        }

        /// <summary>
        /// ����p�ݒ�G���A�o�^����
        /// </summary>
        /// <param name="area">����p�ݒ�G���A�f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>����p�ݒ�G���A���͉��</returns>
        /// <history>
        /// 2022/01/27 yano #4126�y����p�ݒ�G���A�z�����ɂ��ۑ��@�\�̐����̎���
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CostArea area, FormCollection form) {

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }
            
            ViewData["update"] = form["update"];
            ValidateCostArea(area);
            if(!ModelState.IsValid){
                GetEntryViewData(area);			//Add 2022/01/27 yano #4126
                return View("CostAreaEntry",area);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CostArea target = new CostAreaDao(db).GetByKey(area.CostAreaCode);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(area);
                db.CostArea.InsertOnSubmit(area);
            }

            //Mod 2014/08/05 arc amii �G���[���O�Ή� SubmitChanges��try catch�������C��
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    // DB�A�N�Z�X�̎��s
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
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0010", new string[] { "����p�ݒ�G���A�R�[�h", "�ۑ�" }));
                        GetEntryViewData(area);			//Add 2022/01/27 yano #4126
                        return View("CostAreaEntry", area);
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

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            ViewData["update"] = "1";
            GetEntryViewData(area);			//Add 2022/01/27 yano #4126
            return View("CostAreaEntry", area);
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <history>
        /// 2023/10/05 yano #4184�y�ԗ��`�[���́z�̔�����p��[���Îԓ_���E������p][���ÎԌp��������p]�̍폜
        /// </history>
        /// <param name="area">����p�ݒ�G���A�f�[�^</param>
        private void ValidateCostArea(CostArea area) {

            //Mod 2023/10/05 yano #4184
            CommonValidate("CostAreaCode", "����p�ݒ�G���A�R�[�h", area, true);
            CommonValidate("CostAreaName", "����p�ݒ�G���A��", area, true);
            CommonValidate("RequestNumberCost", "�i���o�[�v���[�g��(��])", area, false);
            CommonValidate("ParkingSpaceCost", "�Ԍɏؖ��؎���", area, false);
            CommonValidate("InspectionRegistFeeWithTax", "�����o�^�葱��s��p", area, false);
            CommonValidate("TradeInFeeWithTax", "����ԏ��L�������葱��p", area, false);
            CommonValidate("PreparationFeeWithTax", "�[�Ԕ�p", area, false);
            CommonValidate("RequestNumberFeeWithTax", "��]�i���o�[�\���萔��", area, false);
            CommonValidate("OutJurisdictionRegistFeeWithTax", "�Ǌ��O�o�^�葱��p", area, false);
            CommonValidate("FarRegistFeeWithTax", "���O�o�^�葱��s��p", area, false);
            CommonValidate("ParkingSpaceFeeWithTax", "�Ԍɏؖ��葱��s��p", area, false);
            CommonValidate("RecycleControlFeeWithTax", "���T�C�N�������Ǘ���", area, false);

            //CommonValidate("CostAreaCode", "����p�ݒ�G���A�R�[�h", area, true);
            //CommonValidate("CostAreaName", "����p�ݒ�G���A��", area, true);
            //CommonValidate("RequestNumberCost", "�i���o�[�v���[�g��(��])", area, false);
            //CommonValidate("ParkingSpaceCost", "�Ԍɏؖ��؎���", area, false);
            //CommonValidate("InspectionRegistFee", "�����E�o�^�葱��s��p", area, false);
            //CommonValidate("TradeInFee", "����ԏ��葱��p", area, false);
            //CommonValidate("PreparationFee", "�[�ԏ�����p", area, false);
            //CommonValidate("AppraisalFee", "����ԍ����p", area, false);
            ////CommonValidate("RecycleControlFee", "���T�C�N�������Ǘ���", area, false);
            //CommonValidate("RequestNumberFee", "��]�i���o�[�\���萔��", area, false);
        }

        /// <summary>
        /// ����p�ݒ�G���A�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">����p�ݒ�G���A�f�[�^</param>
        /// <returns>����p�ݒ�G���A�f�[�^</returns>
        private CostArea EditForUpdate(CostArea area) {
            area.LastUpdateDate = DateTime.Now;
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return area;
        }

        /// <summary>
        /// ����p�ݒ�G���A�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">����p�ݒ�G���A�f�[�^</param>
        /// <returns>����p�ݒ�G���A�f�[�^</returns>
        private CostArea EditForInsert(CostArea area) {
            area.CreateDate = DateTime.Now;
            area.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.DelFlag = "0";
            return EditForUpdate(area);
        }

        /// <summary>
        /// ����p�ݒ�G���A�R�[�h���珔��p�ݒ�G���A�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����p�ݒ�G���A�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CostArea area = new CostAreaDao(db).GetByKey(code);
                if (area != null) {
                    codeData.Code = area.CostAreaCode;
                    codeData.Name = area.CostAreaName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// ����p�ݒ�G���A�R�[�h���珔��p�ݒ���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����p�ݒ�G���A�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2023/10/05 yano #4184 �y�ԗ��`�[���́z�̔�����p��[���Îԓ_���E������p][���ÎԌp��������p]�̍폜
        /// 2023/08/15 yano #4176 �̔�����p�̏C��
        /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ� ���ڂ̒ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CostArea area = new CostAreaDao(db).GetByKey(code);

                Dictionary<string, string> ret = new Dictionary<string, string>();

                if (area != null)
                {
                    //Mod 2023/10/05 yano #4184
                    //Mod 2023/08/15 yano #4176
                    ret.Add("CostAreaCode", area.CostAreaCode);
                    ret.Add("CostAreaName", area.CostAreaName);
                    ret.Add("RequestNumberCost", (area.RequestNumberCost ?? 0).ToString());
                    ret.Add("ParkingSpaceFeeWithTax", (area.ParkingSpaceFeeWithTax ?? 0).ToString());
                    ret.Add("InspectionRegistFeeWithTax", (area.InspectionRegistFeeWithTax ?? 0).ToString());
                    ret.Add("TradeInFeeWithTax", (area.TradeInFeeWithTax ?? 0).ToString());
                    ret.Add("PreparationFeeWithTax", (area.PreparationFeeWithTax ?? 0).ToString());
                    ret.Add("RequestNumberFeeWithTax", (area.RequestNumberFeeWithTax ?? 0).ToString());
                    ret.Add("ParkingSpaceCost", (area.ParkingSpaceCost ?? 0).ToString());
                    ret.Add("NumberPlateCost", (area.NumberPlateCost ?? 0).ToString());
                    ret.Add("RecycleControlFeeWithTax", (area.RecycleControlFeeWithTax ?? 0).ToString());
                    ret.Add("FarRegistFeeWithTax", (area.FarRegistFeeWithTax ?? 0).ToString());
                    ret.Add("OutJurisdictionRegistFeeWithTax", (area.OutJurisdictionRegistFeeWithTax ?? 0).ToString());

                    //ret.Add("CostAreaCode", area.CostAreaCode);
                    //ret.Add("CostAreaName", area.CostAreaName);
                    //ret.Add("RequestNumberCost", (area.RequestNumberCost ?? 0).ToString());
                    //ret.Add("ParkingSpaceFeeWithTax", (area.ParkingSpaceFeeWithTax ?? 0).ToString());
                    //ret.Add("InspectionRegistFeeWithTax", (area.InspectionRegistFeeWithTax ?? 0).ToString());
                    //ret.Add("TradeInFeeWithTax", (area.TradeInFeeWithTax ?? 0).ToString());
                    //ret.Add("PreparationFeeWithTax", (area.PreparationFeeWithTax ?? 0).ToString());
                    //ret.Add("RequestNumberFeeWithTax", (area.RequestNumberFeeWithTax ?? 0).ToString());
                    //ret.Add("ParkingSpaceCost", (area.ParkingSpaceCost ?? 0).ToString());
                    //ret.Add("NumberPlateCost", (area.NumberPlateCost ?? 0).ToString());
                    //ret.Add("RecycleControlFeeWithTax", (area.RecycleControlFeeWithTax ?? 0).ToString());
                    //ret.Add("FarRegistFeeWithTax", (area.FarRegistFeeWithTax ?? 0).ToString());
                    //ret.Add("TradeInMaintenanceFeeWithTax", (area.TradeInMaintenanceFeeWithTax ?? 0).ToString());
                    //ret.Add("InheritedInsuranceFeeWithTax", (area.InheritedInsuranceFeeWithTax ?? 0).ToString());
                    //ret.Add("OutJurisdictionRegistFeeWithTax", (area.OutJurisdictionRegistFeeWithTax ?? 0).ToString());

                  
                }

                return Json(ret);
            }

            return new EmptyResult();
        }
            
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="serviceWork">���f���f�[�^</param>
        /// <histroy
        /// 2022/01/27 yano #4126�y����p�ݒ�G���A�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        private void GetEntryViewData(CostArea area)
        {     
            //�ԗ��}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_MASTEREDIT).EnableFlag;
        }
    }
}
