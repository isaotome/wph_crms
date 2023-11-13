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
    public class CarLiabilityInsuranceController : InheritedController
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����ӕی����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�����ӕی����}�X�^�o�^"; // ������

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarLiabilityInsuranceController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �����ӕی���������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �����ӕی�����������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ӕی�����������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarLiabilityInsurance> list = new CarLiabilityInsuranceDao(db).GetListAll();
            return View("CarLiabilityInsuranceCriteria", list);
        }

        /// <summary>
        /// �����ӕی��������_�C�A���O�\��
        /// </summary>
        /// <returns>�����ӕی��������_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            List<CarLiabilityInsurance> list = new CarLiabilityInsuranceDao(db).GetListAll();
            return View("CarLiabilityInsuranceCriteriaDialog", list);
        }

        /// <summary>
        /// �����ӕی������͉�ʕ\��
        /// </summary>
        /// <param name="id">�����ӕی���ID</param>
        /// <returns>�����ӕی������͉��</returns>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarLiabilityInsurance insurance = new CarLiabilityInsurance();
            if (!string.IsNullOrEmpty(id)) {
                insurance = new CarLiabilityInsuranceDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
            return View("CarLiabilityInsuranceEntry", insurance);
        }

        /// <summary>
        /// �����ӕی����o�^����
        /// </summary>
        /// <param name="insurance">�����ӕی����f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ӕی������͉��</returns>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarLiabilityInsurance insurance, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarLiabilityInsurance(insurance);
            if(!ModelState.IsValid){
                GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
                return View("CarLiabilityInsuranceEntry",insurance);
            }
            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            if (form["update"].Equals("1")) {
                CarLiabilityInsurance target = new CarLiabilityInsuranceDao(db).GetByKey(insurance.CarLiabilityInsuranceId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(insurance);
                db.CarLiabilityInsurance.InsertOnSubmit(insurance);
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
                        GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
                        return View("CarLiabilityInsuranceEntry", insurance);
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
            //return View("CarLiabilityInsuranceEntry",insurance);
            return Entry(insurance.CarLiabilityInsuranceId.ToString());
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="insurance">�����ӕی����f�[�^</param>
        private void ValidateCarLiabilityInsurance(CarLiabilityInsurance insurance){
            CommonValidate("CarLiabilityInsuranceName", "�\����", insurance, true);
            CommonValidate("Amount","���z(�~)",insurance,true);
            if (!string.IsNullOrEmpty(insurance.NewDefaultFlag) && insurance.NewDefaultFlag.Contains("true")) {
                CarLiabilityInsurance carLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (carLiabilityInsurance != null
                    && !CommonUtils.DefaultString(carLiabilityInsurance.CarLiabilityInsuranceId).Equals(CommonUtils.DefaultString(insurance.CarLiabilityInsuranceId)))
                {
                    ModelState.AddModelError("NewDefaultFlag", "���̋��z�Ńf�t�H���g�ݒ肳��Ă��邽�߂��̃f�[�^�̓f�t�H���g�ݒ�ɂ͂ł��܂���");
                }
            }
            if (!string.IsNullOrEmpty(insurance.UsedDefaultFlag) && insurance.UsedDefaultFlag.Contains("true")) {
                CarLiabilityInsurance carLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (carLiabilityInsurance != null && !CommonUtils.DefaultString(carLiabilityInsurance.CarLiabilityInsuranceId).Equals(CommonUtils.DefaultString(insurance.CarLiabilityInsuranceId)))
                {
                    ModelState.AddModelError("UsedDefaultFlag", "���̋��z�Ńf�t�H���g�ݒ肳��Ă��邽�߂��̃f�[�^�̓f�t�H���g�ݒ�ɂ͂ł��܂���B");
                }
            }
        }

        /// <summary>
        /// �����ӕی����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="insurance">�����ӕی����f�[�^</param>
        /// <returns>�����ӕی����f�[�^</returns>
        private CarLiabilityInsurance EditForUpdate(CarLiabilityInsurance insurance) {
            insurance.LastUpdateDate = DateTime.Now;
            insurance.LastUpdateEmployee = ((Employee)Session["Employee"]).EmployeeCode;
            if (!string.IsNullOrEmpty(insurance.NewDefaultFlag)) {
                if (insurance.NewDefaultFlag.Contains("true")) {
                    insurance.NewDefaultFlag = "1";
                } else {
                    insurance.NewDefaultFlag = "0";
                }
            } else {
                insurance.NewDefaultFlag = "0";
            }

            if (!string.IsNullOrEmpty(insurance.UsedDefaultFlag)) {
                if (insurance.UsedDefaultFlag.Contains("true")) {
                    insurance.UsedDefaultFlag = "1";
                } else {
                    insurance.UsedDefaultFlag = "0";
                }
            } else {
                insurance.UsedDefaultFlag = "0";
            }
            return insurance;
        }

        /// <summary>
        /// �����ӕی����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ӕی����f�[�^</param>
        /// <returns>�����ӕی����f�[�^</returns>
        private CarLiabilityInsurance EditForInsert(CarLiabilityInsurance insurance) {
            insurance.CarLiabilityInsuranceId = Guid.NewGuid();
            insurance.CreateDate = DateTime.Now;
            insurance.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            insurance.DelFlag = "0";
            return EditForUpdate(insurance);
        }
        
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="insurance">���f���f�[�^</param>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        private void GetEntryViewData(CarLiabilityInsurance insurance)
        {     
            //�ԗ��}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }

    }
}
