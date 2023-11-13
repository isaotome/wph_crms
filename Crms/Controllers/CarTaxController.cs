using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Data.Linq;                 //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �����ԐŎ�ʊ��}�X�^�N���X
    /// </summary>
    /// <history>
    /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����ԐŎ�ʊ��}�X�^";     // ��ʖ�        //Mod 2019/09/04 yano #4011
        private static readonly string PROC_NAME = "�����ԐŎ�ʊ��}�X�^�o�^"; // ������        //Mod 2019/09/04 yano #4011

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �����ԐŎ�ʊ�������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �����ԐŎ�ʊ���������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԐŌ�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarTax> list = new CarTaxDao(db).GetListAll();
            return View("CarTaxCriteria", list);
        }

        /// <summary>
        /// �����ԐŎ�ʊ������_�C�A���O�\��
        /// </summary>
        /// <returns>�����ԐŎ�ʊ������_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            List<CarTax> list = new CarTaxDao(db).GetListAll();
            return View("CarTaxCriteriaDialog", list);
        }

        /// <summary>
        /// �����ԐŎ�ʊ����͉�ʕ\��
        /// </summary>
        /// <param name="id">�����ԐŎ�ʊ�ID</param>
        /// <returns>�����ԐŎ�ʊ����͉��</returns>
        /// <history>
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarTax carTax = new CarTax();
            if (!string.IsNullOrEmpty(id)) {
                carTax = new CarTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125

            return View("CarTaxEntry", carTax);
        }

        /// <summary>
        /// �����ԐŎ�ʊ��o�^����
        /// </summary>
        /// <param name="carTax">�����ԐŎ�ʊ��f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԐŎ�ʊ����͉��</returns>
         /// <history>
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarTax carTax, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarTax(carTax);
            if(!ModelState.IsValid){
                GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
                return View("CarTaxEntry",carTax);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CarTax target = new CarTaxDao(db).GetByKey(carTax.CarTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(carTax);
                db.CarTax.InsertOnSubmit(carTax);
            }

            // Mod 2014/08/04 arc amii �G���[���O�Ή� ��Controller��catch������킹��悤�C��
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException cfe)
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
                        OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    //Add 2014/08/04 arc amii �G���[���O�Ή��wthrow e�x����G���[���O���o�͂��鏈���ɕύX
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/04 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                        GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
                        return View("CarTaxEntry", carTax);
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
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            ViewData["update"] = "1";
            GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
            return View("CarTaxEntry",carTax);
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="carTax">�����ԐŎ�ʊ��f�[�^</param>
        /// <history>
        /// 2019/10/21 yano #4023 �y�ԗ��`�[���́z���ÎԂ̎����Ԏ�ʊ��̌v�Z�̌��
        /// </history>
        private void ValidateCarTax(CarTax carTax){
            //ADD 2014/06/09 arc uchida �t�H�[�}�b�g�`�F�b�N�̒ǉ�
            if (!ModelState.IsValidField("RegistMonth"))
            {
                ModelState.AddModelError("RegistMonth", MessageUtils.GetMessage("E0004", new string[] { "�o�^��", "���̐����̂�" }));
            }
            else {
                CommonValidate("RegistMonth", "�o�^��", carTax, true);
            }
            
            CommonValidate("CarTaxName", "�\����", carTax, true);
            CommonValidate("Amount","���z(�~)",carTax,true);

            //Add 2019/10/21 yano #4023
            CommonValidate("FromAvailableDate", "�K�p��FROM", carTax, true);
            CommonValidate("ToAvailableDate", "�K�p��TO", carTax, true);

            if (ModelState["FromAvailableDate"].Errors.Count > 1)
            {
                ModelState["FromAvailableDate"].Errors.RemoveAt(0);
            }
            if(ModelState["ToAvailableDate"].Errors.Count > 1)
            {
                ModelState["ToAvailableDate"].Errors.RemoveAt(0);
            }
               

            //CommonValidate("RegistMonth", "�o�^��", carTax, true);
        }

        /// <summary>
        /// �����ԐŎ�ʊ��}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԐŎ�ʊ��f�[�^</param>
        /// <returns>�����ԐŎ�ʊ��f�[�^</returns>
        private CarTax EditForUpdate(CarTax carTax) {
            carTax.LastUpdateDate = DateTime.Now;
            carTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carTax;
        }

        /// <summary>
        /// �����ԐŎ�ʊ��}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԐŎ�ʊ��f�[�^</param>
        /// <returns>�����ԐŎ�ʊ��f�[�^</returns>
        private CarTax EditForInsert(CarTax carTax) {
            carTax.CarTaxId = Guid.NewGuid();
            carTax.CreateDate = DateTime.Now;
            carTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carTax.DelFlag = "0";
            return EditForUpdate(carTax);
        }
        
        /// <summary>
        /// �O���[�h�R�[�h�Ɠo�^��]�����玩���ԐŎ�ʊ����z���擾����(Ajax��p�j
        /// </summary>
        /// <param name="carGradeCode">�O���[�h�R�[�h</param>
        /// <param name="requestRegistDate">�o�^��]��</param>
        /// <param name="vin">�ԑ�ԍ�</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2019/10/29 yano #4024 �y�ԗ��`�[���́z�I�v�V�����s�ǉ��E�폜���ɃG���[�����������̕s��Ή�
        /// 2019/10/17 yano #4023 �y�ԗ��`�[���́z���ÎԂ̎����Ԏ�ʊ��̌v�Z�̌��
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C��� 
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetCarTax(string carGradeCode, string requestRegistDate, string vin ) {
            if (Request.IsAjaxRequest()) {

                //Mod 2019/10/17 yano #4023
                DateTime firstrequestRegistDate = DateTime.Today;
                SalesCar sc = new SalesCar();

                //�ԑ�ԍ��Ŏԗ��}�X�^���������āA���N�x�o�^��ݒ肷��
                if (!string.IsNullOrWhiteSpace(vin))
                {
                    sc = new SalesCarDao(db).GetByVin(vin).FirstOrDefault();

                    firstrequestRegistDate = (sc != null ? (sc.FirstRegistrationDate ?? DateTime.Today) : DateTime.Today);
                }
               

                CarGrade carGrade = new CarGradeDao(db).GetByKey(carGradeCode);

                try {
                    int registMonth = DateTime.Parse(requestRegistDate).Month;
                    if (carGradeCode != null) {
                        //Add 2019/09/04 yano #4011 //�d�C�̏ꍇ�͔r�C��1.0�ȉ��Ɠ���
                        if (!string.IsNullOrWhiteSpace(carGrade.VehicleType) && carGrade.VehicleType.Equals("002"))
                        {
                            carGrade.Displacement = 1m;
                        }
                        //Mod 2019/10/29 yano #4024
                        //Mod 2019/10/17 yano #4023
                        CarTax carTax = new CarTaxDao(db).GetByDisplacement(sc.Displacement != null ? (sc.Displacement ?? 0m) : (carGrade.Displacement ?? 0m), registMonth, firstrequestRegistDate);
                        //CarTax carTax = new CarTaxDao(db).GetByDisplacement(carGrade.Displacement ?? 0m, registMonth, firstrequestRegistDate);
                        
                        return Json(carTax.Amount);
                    }
                } catch {
                }
            }
            return new EmptyResult();
        }

         /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="serviceWork">���f���f�[�^</param>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        private void GetEntryViewData(CarTax carTax)
        {     
            //�ԗ��}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }
    }
}
