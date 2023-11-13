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

namespace Crms.Controllers
{
    /// <summary>
    /// �N�[�|���}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CouponController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CouponController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �N�[�|��������ʕ\��
        /// </summary>
        /// <returns>�N�[�|���������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �N�[�|��������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�N�[�|���������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Coupon> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CouponCode"] = form["CouponCode"];
            ViewData["CouponName"] = form["CouponName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �N�[�|��������ʂ̕\��
            return View("CouponCriteria", list);
        }

        /// <summary>
        /// �N�[�|�������_�C�A���O�\��
        /// </summary>
        /// <returns>�N�[�|�������_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �N�[�|�������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�N�[�|��������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["CouponCode"] = Request["CouponCode"];
            form["CouponName"] = Request["CouponName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Coupon> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CouponCode"] = form["CouponCode"];
            ViewData["CouponName"] = form["CouponName"];

            // �N�[�|��������ʂ̕\��
            return View("CouponCriteriaDialog", list);
        }

        /// <summary>
        /// �N�[�|���}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�N�[�|���R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�N�[�|���}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Coupon coupon;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                coupon = new Coupon();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                coupon = new CouponDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(coupon);

            // �o��
            return View("CouponEntry", coupon);
        }

        /// <summary>
        /// �N�[�|���}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="coupon">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�N�[�|���}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Coupon coupon, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCoupon(coupon);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(coupon);
                return View("CouponEntry", coupon);
            }

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                Coupon targetCoupon = new CouponDao(db).GetByKey(coupon.CouponCode);
                UpdateModel(targetCoupon);
                EditCouponForUpdate(targetCoupon);
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
                            throw e;
                        }
                    }
                }
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                coupon = EditCouponForInsert(coupon);

                // �f�[�^�ǉ�
                db.Coupon.InsertOnSubmit(coupon);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0010", new string[] { "�N�[�|���R�[�h", "�ۑ�" }));
                        GetEntryViewData(coupon);
                        return View("CouponEntry", coupon);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �N�[�|���R�[�h����N�[�|�������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�N�[�|���R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Coupon coupon = new CouponDao(db).GetByKey(code);
                if (coupon != null)
                {
                    codeData.Code = coupon.CouponCode;
                    codeData.Name = coupon.CouponName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="coupon">���f���f�[�^</param>
        private void GetEntryViewData(Coupon coupon)
        {
            // �u�����h���̎擾
            if (!string.IsNullOrEmpty(coupon.CarBrandCode))
            {
                BrandDao brandDao = new BrandDao(db);
                Brand brand = brandDao.GetByKey(coupon.CarBrandCode);
                if (brand != null)
                {
                    ViewData["CarBrandName"] = brand.CarBrandName;
                }
            }
        }

        /// <summary>
        /// �N�[�|���}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�N�[�|���}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Coupon> GetSearchResultList(FormCollection form)
        {
            CouponDao couponDao = new CouponDao(db);
            Coupon couponCondition = new Coupon();
            couponCondition.CouponCode = form["CouponCode"];
            couponCondition.CouponName = form["CouponName"];
            couponCondition.Brand = new Brand();
            couponCondition.Brand.CarBrandCode = form["CarBrandCode"];
            couponCondition.Brand.CarBrandName = form["CarBrandName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                couponCondition.DelFlag = form["DelFlag"];
            }
            return couponDao.GetListByCondition(couponCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="coupon">�N�[�|���f�[�^</param>
        /// <returns>�N�[�|���f�[�^</returns>
        private Coupon ValidateCoupon(Coupon coupon)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(coupon.CouponCode))
            {
                ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0001", "�N�[�|���R�[�h"));
            }
            if (string.IsNullOrEmpty(coupon.CouponName))
            {
                ModelState.AddModelError("CouponName", MessageUtils.GetMessage("E0001", "�N�[�|����"));
            }
            if (string.IsNullOrEmpty(coupon.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "�u�����h"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CouponCode") && !CommonUtils.IsAlphaNumeric(coupon.CouponCode))
            {
                ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0012", "�N�[�|���R�[�h"));
            }
            if (ModelState.IsValidField("SalesPrice") && coupon.SalesPrice != null)
            {
                if (!Regex.IsMatch(coupon.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Cost") && coupon.Cost != null)
            {
                if (!Regex.IsMatch(coupon.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }

            return coupon;
        }

        /// <summary>
        /// �N�[�|���}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="coupon">�N�[�|���f�[�^(�o�^���e)</param>
        /// <returns>�N�[�|���}�X�^���f���N���X</returns>
        private Coupon EditCouponForInsert(Coupon coupon)
        {
            coupon.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            coupon.CreateDate = DateTime.Now;
            coupon.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            coupon.LastUpdateDate = DateTime.Now;
            coupon.DelFlag = "0";
            return coupon;
        }

        /// <summary>
        /// �N�[�|���}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="coupon">�N�[�|���f�[�^(�o�^���e)</param>
        /// <returns>�N�[�|���}�X�^���f���N���X</returns>
        private Coupon EditCouponForUpdate(Coupon coupon)
        {
            coupon.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            coupon.LastUpdateDate = DateTime.Now;
            return coupon;
        }

    }
}
