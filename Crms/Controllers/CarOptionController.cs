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
    /// �I�v�V�����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarOptionController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�I�v�V�����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�I�v�V�����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarOptionController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �I�v�V����������ʕ\��
        /// </summary>
        /// <returns>�I�v�V�����������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �I�v�V����������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�I�v�V�����������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["RequiredFlag"] = (form["RequiredFlag"] == null ? "1" : form["RequiredFlag"]);
            form["ActionFlag"] = "0";
            // �������ʃ��X�g�̎擾
            PaginatedList<GetCarOptionMaster_Result> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            //Add 2016/02/22 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarOptionCode"] = form["CarOptionCode"];
            ViewData["CarOptionName"] = form["CarOptionName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["RequiredFlag"] = form["RequiredFlag"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["CarGradeCode"]))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(form["CarGradeCode"]);
                if (CarGradedata != null)
                {
                    ViewData["CarGradeCodeName"] = CarGradedata.CarGradeName;
                }
                else
                {
                    ViewData["CarGradeCodeName"] = "";
                }
            }

            // �I�v�V����������ʂ̕\��
            return View("CarOptionCriteria", list);
        }

        /// <summary>
        /// �I�v�V���������_�C�A���O�\��
        /// </summary>
        /// <returns>�I�v�V���������_�C�A���O</returns>
        public ActionResult CriteriaDialog(string CarGradeCode = "")
        {
            return CriteriaDialog(new FormCollection(), CarGradeCode);
        }

        /// <summary>
        /// �I�v�V���������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�I�v�V����������ʃ_�C�A���O</returns>
        //Mod 2016/02/22 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form, string CarGradeCode = "")
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)

            //�O���[�h�R�[�h���n���ꂽ��Y���̃��[�J�[���ƃ��[�J�[�R�[�h������
            if (!string.IsNullOrEmpty(CarGradeCode))
            {
                CarGrade carGrade = new CarGradeDao(db).GetByKey(CarGradeCode);
                form["MakerCode"] = carGrade.Car.Brand.Maker.MakerCode;
                form["MakerName"] = carGrade.Car.Brand.Maker.MakerName;
                form["CarGradeCode"] = CarGradeCode;

            }
            else
            {
                form["MakerCode"] = Request["MakerCode"];
                form["MakerName"] = Request["MakerName"];
                form["CarGradeCode"] = Request["CarGradeCode"];
            }
            form["CarOptionCode"] = Request["CarOptionCode"];
            form["CarOptionName"] = Request["CarOptionName"];
            form["RequiredFlag"] = Request["RequiredFlag"];
            form["RequiredFlag"] = (form["RequiredFlag"] == null ? "0" : form["RequiredFlag"]);
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["ActionFlag"] = "1";
            // �������ʃ��X�g�̎擾
            PaginatedList<GetCarOptionMaster_Result> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarOptionCode"] = form["CarOptionCode"];
            ViewData["CarOptionName"] = form["CarOptionName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["RequiredFlag"] = form["RequiredFlag"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["CarGradeCode"]))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(form["CarGradeCode"]);
                if (CarGradedata != null)
                {
                    ViewData["CarGradeName"] = CarGradedata.CarGradeName;
                }
                else
                {
                    ViewData["CarGradeName"] = "";
                }
            }

            // �I�v�V����������ʂ̕\��
            return View("CarOptionCriteriaDialog", list);
        }

        /// <summary>
        /// �I�v�V�����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�I�v�V�����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�I�v�V�����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarOption carOption;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carOption = new CarOption();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                carOption = new CarOptionDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(carOption);

            // �o��
            return View("CarOptionEntry", carOption);
        }

        /// <summary>
        /// �I�v�V�����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="carOption">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�I�v�V�����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarOption carOption, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCarOption(carOption);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(carOption);
                return View("CarOptionEntry", carOption);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                CarOption targetCarOption = new CarOptionDao(db).GetByKey(carOption.CarOptionCode);
                //�Ԏ�R�[�h�������͂̏ꍇ�͑S�Ԏ틤�ʃI�v�V�����Ƃ��A�C�ӃI�v�V�����Ƃ��� 
                UpdateModel(targetCarOption);
                if (string.IsNullOrEmpty(carOption.CarGradeCode))
                {
                    targetCarOption.CarGradeCode = "";
                    targetCarOption.RequiredFlag = "0";
                }
                EditCarOptionForUpdate(targetCarOption);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                carOption = EditCarOptionForInsert(carOption);

                // �f�[�^�ǉ�
                db.CarOption.InsertOnSubmit(carOption);
            
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
                    // �X�V���A�N���C�A���g�̓ǂݎ��ȍ~��DB�l���X�V���ꂽ���A���[�J���̒l��DB�l�ŏ㏑������
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
                        ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0010", new string[] { "�I�v�V�����R�[�h", "�ۑ�" }));
                        GetEntryViewData(carOption);
                        return View("CarOptionEntry", carOption);
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
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carOption.CarOptionCode);
        }

        /// <summary>
        /// �I�v�V�����R�[�h����I�v�V���������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�I�v�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarOption carOption = new CarOptionDao(db).GetByKey(code);
                if (carOption != null)
                {
                    codeData.Code = carOption.CarOptionCode;
                    codeData.Name = carOption.CarOptionName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �I�v�V�����R�[�h����I�v�V�����ڍ׏����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�I�v�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retOption = new Dictionary<string, string>();
                CarOption carOption = new CarOptionDao(db).GetByKey(code);
                if (carOption != null)
                {
                    retOption.Add("CarOptionCode", carOption.CarOptionCode);
                    retOption.Add("CarOptionName", carOption.CarOptionName);
                    retOption.Add("SalesPrice", carOption.SalesPrice.ToString());
                }
                return Json(retOption);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carOption">���f���f�[�^</param>
        private void GetEntryViewData(CarOption carOption)
        {
            // ���[�J�[���̎擾
            if (!string.IsNullOrEmpty(carOption.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(carOption.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }
            //Add 2016/02/22 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
            // �Ԏ햼�̎擾
            if (!string.IsNullOrEmpty(carOption.CarGradeCode))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(carOption.CarGradeCode);
                if (CarGradedata != null)
                {
                    carOption.CarGradeName = CarGradedata.CarGradeName;
                }
                else
                {
                    carOption.CarGradeName = "";
                }
            }

            CodeDao dao = new CodeDao(db);
            // �敪�̎擾
            ViewData["OptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOptionTypeAll(false), carOption.OptionType, false);


        }

        /// <summary>
        /// �I�v�V�����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�I�v�V�����}�X�^�������ʃ��X�g</returns>
        //Mod 2016/02/22 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
        private PaginatedList<GetCarOptionMaster_Result> GetSearchResultList(FormCollection form)
        {
            CarOptionDao carOptionDao = new CarOptionDao(db);
            CarOption carOptionCondition = new CarOption();
            carOptionCondition.CarOptionCode = form["CarOptionCode"];
            carOptionCondition.CarOptionName = form["CarOptionName"];
            carOptionCondition.Maker = new Maker();
            carOptionCondition.Maker.MakerCode = form["MakerCode"];
            carOptionCondition.Maker.MakerName = form["MakerName"];
            carOptionCondition.CarGradeCode = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["ActionFlag"]))
            {
                carOptionCondition.ActionFlag = form["ActionFlag"];
            }
            else
            {
                carOptionCondition.ActionFlag = "0";
            }
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carOptionCondition.DelFlag = form["DelFlag"];
            }
            if (form["RequiredFlag"].Equals("0") || form["RequiredFlag"].Equals("1"))
            {
                carOptionCondition.RequiredFlag = form["RequiredFlag"];
            }
            return carOptionDao.GetListByCondition(carOptionCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carOption">�I�v�V�����f�[�^</param>
        /// <returns>�I�v�V�����f�[�^</returns>
        private CarOption ValidateCarOption(CarOption carOption)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(carOption.CarOptionCode))
            {
                ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0001", "�I�v�V�����R�[�h"));
            }
            if (string.IsNullOrEmpty(carOption.CarOptionName))
            {
                ModelState.AddModelError("CarOptionName", MessageUtils.GetMessage("E0001", "�I�v�V������"));
            }
            if (string.IsNullOrEmpty(carOption.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "���[�J�["));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarOptionCode") && !CommonUtils.IsAlphaNumeric(carOption.CarOptionCode))
            {
                ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0012", "�I�v�V�����R�[�h"));
            }
            if (ModelState.IsValidField("Cost") && carOption.Cost != null)
            {
                if (!Regex.IsMatch(carOption.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesPrice") && carOption.SalesPrice != null)
            {
                if (!Regex.IsMatch(carOption.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "���̐����̂�" }));
                }
            }

            return carOption;
        }

        /// <summary>
        /// �I�v�V�����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carOption">�I�v�V�����f�[�^(�o�^���e)</param>
        /// <returns>�I�v�V�����}�X�^���f���N���X</returns>
        private CarOption EditCarOptionForInsert(CarOption carOption)
        {
            carOption.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.CreateDate = DateTime.Now;
            carOption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.LastUpdateDate = DateTime.Now;
            carOption.DelFlag = "0";
            //�Ԏ�R�[�h�������͂̏ꍇ�͑S�Ԏ틤�ʃI�v�V�����Ƃ��A�C�ӃI�v�V�����Ƃ���
            if (string.IsNullOrEmpty(carOption.CarGradeCode))
            {
                carOption.CarGradeCode = "";
                carOption.RequiredFlag = "0";
            }
            return carOption;
        }

        /// <summary>
        /// �I�v�V�����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carOption">�I�v�V�����f�[�^(�o�^���e)</param>
        /// <returns>�I�v�V�����}�X�^���f���N���X</returns>
        private CarOption EditCarOptionForUpdate(CarOption carOption)
        {
            carOption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.LastUpdateDate = DateTime.Now;
            return carOption;
        }

        //Add 2016/02/15 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
        /// <summary>
        /// �O���[�h�R�[�h����K�{�̃I�v�V���������擾����(Ajax��p�j
        /// </summary>
        /// <param name="GradeCode">�O���[�h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetRequiredOptionByCarGradeCode(string GradeCode)
        {
            if (Request.IsAjaxRequest())
            {
                CarGrade carGrade = new CarGradeDao(db).GetByKey(GradeCode);

                if (carGrade != null)
                {
                    //�Y���Ԏ�̕K�{�I�v�V�������擾����
                    List<GetCarOptionSetListResult> OptionSet = new CarOptionDao(db).GetRequiredOptionByCarCode(GradeCode, carGrade.Car.Brand.Maker.MakerCode);
                    return Json(OptionSet);
                }
                else
                {
                    return new EmptyResult();
                }
            }
            return new EmptyResult();
        }

        //Add 2016/02/15 arc nakayama #3415_�ԗ��`�[�쐬���̃I�v�V�����̃f�t�H���g�ݒ�
        /// <summary>
        /// �ԑ�ԍ�����K�{�̃I�v�V���������擾����(Ajax��p�j
        /// </summary>
        /// <param name="VinCode">�ԑ�ԍ�</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetRequiredOptionByVin(string VinCode)
        {
            if (Request.IsAjaxRequest())
            {
                // �ԑ�ԍ����L�[�Ƀ��R�[�h���擾
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(VinCode);

                string GradeCode = "";

                // �f�[�^������ꍇ�A�O���[�h�R�[�h��ݒ肷��
                if (salesCarList != null && salesCarList.Count > 0)
                {
                    GradeCode = salesCarList[0].CarGradeCode;      // �O���[�h�R�[�h
                }
                else
                {
                    return new EmptyResult();
                }

                //�V�Ԃ̏ꍇ�̂݃I�v�V������I������
                if (salesCarList[0].NewUsedType == "N")
                {
                    //�擾�����O���[�h�R�[�h����Ԏ�R�[�h�ƃ��[�J�[�R�[�h���擾
                    CarGrade carGrade = new CarGradeDao(db).GetByKey(GradeCode);

                    if (carGrade != null)
                    {
                        //�Y���Ԏ�̕K�{�I�v�V�������擾����
                        List<GetCarOptionSetListResult> OptionSet = new CarOptionDao(db).GetRequiredOptionByCarCode(GradeCode, carGrade.Car.Brand.Maker.MakerCode);
                        return Json(OptionSet);
                    }
                    else
                    {
                        return new EmptyResult();
                    }
                }
                else
                {
                    return new EmptyResult();
                }
            }
            return new EmptyResult();
        }
    }
}
