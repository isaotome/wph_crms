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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �u�����h�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BrandController : Controller
    {

        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�u�����h�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�u�����h�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public BrandController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �u�����h������ʕ\��
        /// </summary>
        /// <returns>�u�����h�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �u�����h������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�u�����h�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            
            // �������ʃ��X�g�̎擾
            PaginatedList<Brand> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �u�����h������ʂ̕\��
            return View("BrandCriteria", list);
        }

        /// <summary>
        /// �u�����h�����_�C�A���O�\��
        /// </summary>
        /// <returns>�u�����h�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �u�����h�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�u�����h������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Brand> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];

            // �u�����h������ʂ̕\��
            return View("BrandCriteriaDialog", list);
        }

        /// <summary>
        /// �u�����h�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�u�����h�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�u�����h�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Brand brand;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                brand = new Brand();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                brand = new BrandDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(brand);

            // �o��
            return View("BrandEntry", brand);
        }

        /// <summary>
        /// �u�����h�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="brand">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�u�����h�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Brand brand, FormCollection form)
        {

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateBrand(brand);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(brand);
                return View("BrandEntry", brand);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Brand targetBrand = new BrandDao(db).GetByKey(brand.CarBrandCode, true);
                UpdateModel(targetBrand);
                EditBrandForUpdate(targetBrand);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                brand = EditBrandForInsert(brand);

                // �f�[�^�ǉ�
                db.Brand.InsertOnSubmit(brand);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o�͏����ǉ�
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
                    //Add 2014/08/04 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0010", new string[] { "�u�����h�R�[�h", "�ۑ�" }));
                        GetEntryViewData(brand);
                        return View("BrandEntry", brand);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii �G���[���O�Ή� �wtheow e�x����G���[�y�[�W�J�ڂɕύX
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/04 arc amii �G���[���O�Ή� ChangeConflictException�ȊO�̎��̃G���[�����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(brand.CarBrandCode);
        }

        /// <summary>
        /// �u�����h�R�[�h����u�����h�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�u�����h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Brand brand = new BrandDao(db).GetByKey(code);
                if (brand != null)
                {
                    codeData.Code = brand.CarBrandCode;
                    codeData.Name = brand.CarBrandName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="brand">���f���f�[�^</param>
        private void GetEntryViewData(Brand brand)
        {
            // ���[�J�[���̎擾
            if (!string.IsNullOrEmpty(brand.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(brand.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            // ��Ж��̎擾
            if (!string.IsNullOrEmpty(brand.CompanyCode)) {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(brand.CompanyCode);
                if (company != null) {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }
        }

        /// <summary>
        /// �u�����h�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�u�����h�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Brand> GetSearchResultList(FormCollection form)
        {
            BrandDao brandDao = new BrandDao(db);
            Brand brandCondition = new Brand();
            brandCondition.CarBrandCode = form["CarBrandCode"];
            brandCondition.CarBrandName = form["CarBrandName"];
            brandCondition.Maker = new Maker();
            brandCondition.Maker.MakerCode = form["MakerCode"];
            brandCondition.Maker.MakerName = form["MakerName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                brandCondition.DelFlag = form["DelFlag"];
            }
            return brandDao.GetListByCondition(brandCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="brand">�u�����h�f�[�^</param>
        /// <returns>�u�����h�f�[�^</returns>
        private Brand ValidateBrand(Brand brand)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(brand.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "�u�����h�R�[�h"));
            }
            if (string.IsNullOrEmpty(brand.CarBrandName))
            {
                ModelState.AddModelError("CarBrandName", MessageUtils.GetMessage("E0001", "�u�����h��"));
            }
            if (string.IsNullOrEmpty(brand.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "���[�J�["));
            }
            if (string.IsNullOrEmpty(brand.CompanyCode)) {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "���"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarBrandCode") && !CommonUtils.IsAlphaNumeric(brand.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0012", "�u�����h�R�[�h"));
            }
            if (!ModelState.IsValidField("LaborRate")) {
                ModelState.AddModelError("LaborRate", MessageUtils.GetMessage("E0004", new string[] { "���o���[�g", "(0�ȏ�̐����̂�)" }));
            }
            return brand;
        }

        /// <summary>
        /// �u�����h�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="brand">�u�����h�f�[�^(�o�^���e)</param>
        /// <returns>�u�����h�}�X�^���f���N���X</returns>
        private Brand EditBrandForInsert(Brand brand)
        {
            brand.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            brand.CreateDate = DateTime.Now;
            brand.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            brand.LastUpdateDate = DateTime.Now;
            brand.DelFlag = "0";
            return brand;
        }

        /// <summary>
        /// �u�����h�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="brand">�u�����h�f�[�^(�o�^���e)</param>
        /// <returns>�u�����h�}�X�^���f���N���X</returns>
        private Brand EditBrandForUpdate(Brand brand)
        {
            brand.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            brand.LastUpdateDate = DateTime.Now;
            return brand;
        }

    }
}
