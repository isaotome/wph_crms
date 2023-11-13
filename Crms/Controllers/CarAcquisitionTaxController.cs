using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;                 //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarAcquisitionTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����ԐŊ����\���}�X�^";     // ��ʖ�      //Mod 2019/09/04 yano #4011
        private static readonly string PROC_NAME = "�����ԐŊ����\���}�X�^�o�^"; // ������      //Mod 2019/09/04 yano #4011

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarAcquisitionTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �����ԐŊ����\��������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �����ԐŊ����\����������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԐŊ����\����������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarAcquisitionTax> list = new CarAcquisitionTaxDao(db).GetListAll();
            return View("CarAcquisitionTaxCriteria", list);
        }

        /// <summary>
        /// �����ԐŊ����\�����͉�ʕ\��
        /// </summary>
        /// <param name="id">�����ԐŊ����\��ID</param>
        /// <returns>�����ԐŊ����\�����͉��</returns>
        /// <history>
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarAcquisitionTax carTax = new CarAcquisitionTax();
            if (!string.IsNullOrEmpty(id)) {
                carTax = new CarAcquisitionTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }
            GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
            return View("CarAcquisitionTaxEntry", carTax);
        }

        /// <summary>
        /// �����ԐŊ����\���o�^����
        /// </summary>
        /// <param name="carTax">�����ԐŊ����\���f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�����ԐŊ����\�����͉��</returns>
        /// <history>
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarAcquisitionTax carTax, FormCollection form) {
            ViewData["update"] = form["update"];

            // Add 2014/08/01 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                ValidateCarTax(carTax);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                    return View("CarAcquisitionTaxEntry", carTax);
                }
                CarAcquisitionTax target = new CarAcquisitionTaxDao(db).GetByKey(carTax.CarAcquisitionTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                CarAcquisitionTax target = new CarAcquisitionTaxDao(db).GetByValue(carTax.ElapsedYears);
                if (target != null) {
                    ModelState.AddModelError("ElapsedYears", "�w�肳�ꂽ�o�ߔN���͊��ɓo�^����Ă��܂�");
                    if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                        ModelState["ElapsedYears"].Errors.RemoveAt(0);
                    }
                }
                ValidateCarTax(carTax);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                    return View("CarAcquisitionTaxEntry", carTax);
                }
                EditForInsert(carTax);
                db.CarAcquisitionTax.InsertOnSubmit(carTax);
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

                        GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                        
                        return View("CarAcquisitionTaxEntry", carTax);
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
            
            GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
            return View("CarAcquisitionTaxEntry", carTax);
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="carTax">�����Ԋ����\���f�[�^</param>
        private void ValidateCarTax(CarAcquisitionTax carTax) {
            //CommonValidate("ElapsedYears", "�o�ߔN��", carTax, true);
            //CommonValidate("RemainRate","�c����",carTax,true);

            if (!ModelState.IsValidField("ElapsedYears")) {
                ModelState.AddModelError("ElapsedYears", "�o�ߔN����0�ȏ�100����������1���ȓ��i���͕K�{�j�ł�");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("ElapsedYears") &&
                (Regex.IsMatch(carTax.ElapsedYears.ToString(), @"^\d{1,2}\.\d{1,1}$")
                        || (Regex.IsMatch(carTax.ElapsedYears.ToString(), @"^\d{1,1}$")))) {
            } else {
                ModelState.AddModelError("ElapsedYears", "�o�ߔN����0�ȏ�100����������1���ȓ��i���͕K�{�j�ł�");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }
            if (carTax.ElapsedYears < 0 || carTax.ElapsedYears >= 100) {
                ModelState.AddModelError("ElapsedYears", "�o�ߔN����0�ȏ�100����������1���ȓ��i���͕K�{�j�ł�");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }

            if (!ModelState.IsValidField("RemainRate")) {
                ModelState.AddModelError("RemainRate", "�c������0�ȏ�10����������3���ȓ��i���͕K�{�j�ł�");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("RemainRate") &&
                (Regex.IsMatch(carTax.RemainRate.ToString(), @"^\d{1,2}\.\d{1,3}$")
                        || (Regex.IsMatch(carTax.RemainRate.ToString(), @"^\d{1,3}$")))) {
            } else {
                ModelState.AddModelError("RemainRate", "�c������0�ȏ�10����������3���ȓ��i���͕K�{�j�ł�");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }


            if (carTax.RemainRate < 0 || carTax.RemainRate >= 10) {
                ModelState.AddModelError("RemainRate", "�c������0�ȏ�10����������3���ȓ��i���͕K�{�j�ł�");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }


        }

        /// <summary>
        /// �����ԐŊ����\���}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԐŊ����\���f�[�^</param>
        /// <returns>�����ԐŊ����\���f�[�^</returns>
        private CarAcquisitionTax EditForUpdate(CarAcquisitionTax carTax) {
            carTax.LastUpdateDate = DateTime.Now;
            carTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carTax;
        }

        /// <summary>
        /// �����ԐŊ����\���}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carTax">�����ԐŊ����\���f�[�^</param>
        /// <returns>�����ԐŊ����\���f�[�^</returns>
        private CarAcquisitionTax EditForInsert(CarAcquisitionTax carTax) {
            carTax.CarAcquisitionTaxId = Guid.NewGuid();
            carTax.CreateDate = DateTime.Now;
            carTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carTax.DelFlag = "0";
            return EditForUpdate(carTax);
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carAcquisitionTax">���f���f�[�^</param>
        /// <histroy
        /// 2022/01/27 yano #4125�y�����ԐŊ֘A�}�X�^�z�����ɂ��ۑ��@�\�̐����̎���
        /// </histroy>
        private void GetEntryViewData(CarAcquisitionTax carAcquisitionTax)
        {     
            //�ԗ��}�X�^�ҏW�����̂��郆�[�U�̂ݕۑ��{�^����\������B
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }
    }
}
