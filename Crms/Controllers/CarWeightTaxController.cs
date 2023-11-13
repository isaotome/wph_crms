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
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarWeightTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����ԏd�ʐŃ}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�����ԏd�ʐŃ}�X�^�o�^"; // ������

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarWeightTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �����ԏd�ʐŌ�����ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �����ԏd�ʐŌ�������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԏd�ʐŌ�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarWeightTax> list = new CarWeightTaxDao(db).GetListAll();
            return View("CarWeightTaxCriteria", list);
        }

        /// <summary>
        /// �����ԏd�ʐŌ����_�C�A���O�\��
        /// </summary>
        /// <returns>�����ԏd�ʐŌ����_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            List<CarWeightTax> list = new CarWeightTaxDao(db).GetListAll();
            return View("CarWeightTaxCriteriaDialog", list);
        }

        /// <summary>
        /// �����ԏd�ʐœ��͉�ʕ\��
        /// </summary>
        /// <param name="id">�����Ԑ�ID</param>
        /// <returns>�����Ԑœ��͉��</returns>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarWeightTax carWeightTax = new CarWeightTax();
            if (!string.IsNullOrEmpty(id)) {
                carWeightTax = new CarWeightTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
            return View("CarWeightTaxEntry", carWeightTax);
        }

        /// <summary>
        /// �����ԏd�ʐœo�^����
        /// </summary>
        /// <param name="carTax">�����ԏd�ʐŃf�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԏd�ʐœ��͉��</returns>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarWeightTax carWeightTax, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarWeightTax(carWeightTax);
            if(!ModelState.IsValid){
                GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                return View("CarWeightTaxEntry",carWeightTax);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CarWeightTax target = new CarWeightTaxDao(db).GetByKey(carWeightTax.CarWeightTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{

                CarWeightTax tax = new CarWeightTaxDao(db).GetByWeight(carWeightTax.InspectionYear, carWeightTax.WeightFrom);
                if (tax != null) {
                    ModelState.AddModelError("WeightFrom", "�o�^�ς݂̐ݒ�Əd�����Ă��܂�");
                }

                tax = new CarWeightTaxDao(db).GetByWeight(carWeightTax.InspectionYear, carWeightTax.WeightTo);
                if (tax != null) {
                    ModelState.AddModelError("WeightTo", "�o�^�ς݂̐ݒ�Əd�����Ă��܂�");
                }
                if (!ModelState.IsValid) {
                    GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                    return View("CarWeightTaxEntry", carWeightTax);
                }
                EditForInsert(carWeightTax);
                db.CarWeightTax.InsertOnSubmit(carWeightTax);
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
                    // Add 2014/08/04 arc amii �G���[���O�Ή� ChangeConflictException�����ǉ�
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
                        GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                        return View("CarWeightTaxEntry", carWeightTax);
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
            GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
            return View("CarWeightTaxEntry",carWeightTax);
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="carTax">�����ԏd�ʐŃf�[�^</param>
        private void ValidateCarWeightTax(CarWeightTax carWeightTax){
            //CommonValidate("CarWeightTaxName", "�\����", carWeightTax, true);
            CommonValidate("Amount","���z(�~)",carWeightTax,true);
            CommonValidate("InspectionYear", "�Ԍ��N��", carWeightTax, true);
            CommonValidate("WeightFrom", "�d��(kg)", carWeightTax, true);
            CommonValidate("WeightTo", "�d��(kg)", carWeightTax, true);
            if (carWeightTax.InspectionYear <= 0) {
                ModelState.AddModelError("InspectionYear", MessageUtils.GetMessage("E0004", new string[] { "�Ԍ��N��", "1�ȏ�̐��̐���" }));
            }
            if (carWeightTax.WeightFrom >= carWeightTax.WeightTo) {
                ModelState.AddModelError("WeightFrom", "�d��(kg)�͈̔͂��s���ł�");
            }
            if (carWeightTax.WeightFrom < 0) {
                ModelState.AddModelError("WeightFrom", MessageUtils.GetMessage("E0004", new string[] { "�d��(kg)", "���̐���" }));
            }
            if (carWeightTax.WeightTo < 0) {
                ModelState.AddModelError("WeightTo", MessageUtils.GetMessage("E0004", new string[] { "�d��(kg)", "���̐���" }));
            }
        }

        /// <summary>
        /// �����ԏd�ʐŃ}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԏd�ʐŃf�[�^</param>
        /// <returns>�����ԏd�ʐŃf�[�^</returns>
        private CarWeightTax EditForUpdate(CarWeightTax carWeightTax) {
            carWeightTax.LastUpdateDate = DateTime.Now;
            carWeightTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carWeightTax;
        }

        /// <summary>
        /// �����ԏd�ʐŃ}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԏd�ʐŃf�[�^</param>
        /// <returns>�����ԏd�ʐŃf�[�^</returns>
        private CarWeightTax EditForInsert(CarWeightTax carWeightTax) {
            carWeightTax.CarWeightTaxId = Guid.NewGuid();
            carWeightTax.CreateDate = DateTime.Now;
            carWeightTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carWeightTax.DelFlag = "0";
            return EditForUpdate(carWeightTax);
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carWeightTax">���f���f�[�^</param>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        private void GetEntryViewData(CarWeightTax carWeightTax)
        {     
            //�ԗ��}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }

    }
}
