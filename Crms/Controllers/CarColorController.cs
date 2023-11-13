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
using Crms.Models;                      //Add 2014/08/01 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �ԗ��J���[�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarColorController : Controller
    {
        //Add 2014/08/01 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��J���[�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ��J���[�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarColorController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �ԗ��J���[������ʕ\��
        /// </summary>
        /// <returns>�ԗ��J���[�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ��J���[������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��J���[�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<CarColor> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorCategory"], true);
            ViewData["CarColorCode"] = form["CarColorCode"];
            ViewData["CarColorName"] = form["CarColorName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["MakerColorCOde"] = form["MakerColorCode"];

            // �ԗ��J���[������ʂ̕\��
            return View("CarColorCriteria", list);
        }

        /// <summary>
        /// �ԗ��J���[�����_�C�A���O�\��
        /// </summary>
        /// <returns>�ԗ��J���[�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �ԗ��J���[�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��J���[������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["ColorCategory"] = Request["ColorCategory"];
            form["CarColorCode"] = Request["CarColorCode"];
            form["CarColorName"] = Request["CarColorName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["CarGradeCode"] = Request["CarGradeCode"];
            form["InteriorColorFlag"] = Request["InteriorColorFlag"];
            form["ExteriorColorFlag"] = Request["ExteriorColorFlag"];
            // �������ʃ��X�g�̎擾
            PaginatedList<CarColor> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorCategory"], true);
            ViewData["CarColorCode"] = form["CarColorCode"];
            ViewData["CarColorName"] = form["CarColorName"];
            ViewData["MakerColorCode"] = form["MakerColorCode"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            ViewData["InteriorColorFlag"] = form["InteriorColorFlag"]!=null && form["InteriorColorFlag"].Contains("true");
            ViewData["ExteriorColorFlag"] = form["ExteriorColorFlag"]!=null && form["ExteriorColorFlag"].Contains("true");
            // �ԗ��J���[������ʂ̕\��
            return View("CarColorCriteriaDialog", list);
        }

        /// <summary>
        /// �ԗ��J���[�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ��J���[�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�ԗ��J���[�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarColor carColor;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carColor = new CarColor();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                carColor = new CarColorDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(carColor);

            // �o��
            return View("CarColorEntry", carColor);
        }

        /// <summary>
        /// �ԗ��J���[�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="carColor">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��J���[�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarColor carColor, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCarColor(carColor);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(carColor);
                return View("CarColorEntry", carColor);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                CarColor targetCarColor = new CarColorDao(db).GetByKey(carColor.CarColorCode, true);
                UpdateModel(targetCarColor);
                EditCarColorForUpdate(targetCarColor, form);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                carColor = EditCarColorForInsert(carColor, form);
                // �f�[�^�ǉ�
                db.CarColor.InsertOnSubmit(carColor);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
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
                catch (SqlException e)
                {
                    //Add 2014/08/04 arc amii �G���[���O�Ή� �Z�b�V������SQL����o�^���鏈���ǉ�
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/04 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0010", new string[] { "�ԗ��J���[�R�[�h", "�ۑ�" }));
                        GetEntryViewData(carColor);
                        return View("CarColorEntry", carColor);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii �G���[���O�Ή� �wtheow e�x����G���[�y�[�W�J�ڂɕύX
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
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
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carColor.CarColorCode);
        }

        /// <summary>
        /// �ԗ��J���[�R�[�h����ԗ��J���[�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ԗ��J���[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarColor carColor = new CarColorDao(db).GetByKey(code);
                if (carColor != null)
                {
                    codeData.Code = carColor.CarColorCode;
                    codeData.Name = carColor.CarColorName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        public ActionResult GetMaster2(string carGradeCode, string carColorCode) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CarAvailableColor carColor = new CarAvailableColorDao(db).GetByKey(carGradeCode, carColorCode);
                if (carColor != null) {
                    codeData.Code = carColor.CarGradeCode;
                    codeData.Code2 = carColor.CarColorCode;
                    codeData.Name = carColor.CarColor.CarColorName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carColor">���f���f�[�^</param>
        private void GetEntryViewData(CarColor carColor)
        {
            // ���[�J�[���̎擾
            if (!string.IsNullOrEmpty(carColor.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(carColor.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), carColor.ColorCategory, true);
        }

        /// <summary>
        /// �ԗ��J���[�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��J���[�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<CarColor> GetSearchResultList(FormCollection form)
        {
            CarColorDao carColorDao = new CarColorDao(db);
            CarColor carColorCondition = new CarColor();
            carColorCondition.CarColorCode = form["CarColorCode"];
            carColorCondition.CarColorName = form["CarColorName"];
            carColorCondition.Maker = new Maker();
            carColorCondition.Maker.MakerCode = form["MakerCode"];
            carColorCondition.Maker.MakerName = form["MakerName"];
            carColorCondition.ColorCategory = form["ColorCategory"];
            carColorCondition.MakerColorCode = form["MakerColorCode"];
            carColorCondition.CarGradeCode = form["CarGradeCode"];
            carColorCondition.InteriorColorFlag = form["InteriorColorFlag"]!=null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColorCondition.ExteriorColorFlag = form["ExteriorColorFlag"]!=null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carColorCondition.DelFlag = form["DelFlag"];
            }
            return carColorDao.GetListByCondition(carColorCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carColor">�ԗ��J���[�f�[�^</param>
        /// <returns>�ԗ��J���[�f�[�^</returns>
        private CarColor ValidateCarColor(CarColor carColor)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(carColor.CarColorCode))
            {
                ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0001", "�ԗ��J���[�R�[�h"));
            }
            if (string.IsNullOrEmpty(carColor.CarColorName))
            {
                ModelState.AddModelError("CarColorName", MessageUtils.GetMessage("E0001", "�ԗ��J���[��"));
            }
            if (string.IsNullOrEmpty(carColor.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "���[�J�["));
            }
            if (string.IsNullOrEmpty(carColor.MakerColorCode)) {
                ModelState.AddModelError("MakerColorCode", MessageUtils.GetMessage("E0001", "���[�J�[�J���[�R�[�h"));
            }
            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarColorCode") && !CommonUtils.IsAlphaNumeric(carColor.CarColorCode))
            {
                ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0012", "�ԗ��J���[�R�[�h"));
            }

            return carColor;
        }

        /// <summary>
        /// �ԗ��J���[�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carColor">�ԗ��J���[�f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��J���[�}�X�^���f���N���X</returns>
        private CarColor EditCarColorForInsert(CarColor carColor, FormCollection form)
        {
            carColor.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.CreateDate = DateTime.Now;
            carColor.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.LastUpdateDate = DateTime.Now;
            carColor.DelFlag = "0";
            //2015.10.14 nishimura.akira�@�����F�A�O���F�̓o�^�Ή�
            carColor.InteriorColorFlag = form["InteriorColorFlag"] != null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColor.ExteriorColorFlag = form["ExteriorColorFlag"] != null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";
            return carColor;
        }

        /// <summary>
        /// �ԗ��J���[�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carColor">�ԗ��J���[�f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��J���[�}�X�^���f���N���X</returns>
        private CarColor EditCarColorForUpdate(CarColor carColor, FormCollection form)
        {
            carColor.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.LastUpdateDate = DateTime.Now;
            //2015.10.14 nishimura.akira�@�����F�A�O���F�̓o�^�Ή�
            carColor.InteriorColorFlag = form["InteriorColorFlag"] != null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColor.ExteriorColorFlag = form["ExteriorColorFlag"] != null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";
            return carColor;
        }

    }
}
