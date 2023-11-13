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
    /// クーポンマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CouponController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CouponController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// クーポン検索画面表示
        /// </summary>
        /// <returns>クーポン検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// クーポン検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>クーポン検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Coupon> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CouponCode"] = form["CouponCode"];
            ViewData["CouponName"] = form["CouponName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // クーポン検索画面の表示
            return View("CouponCriteria", list);
        }

        /// <summary>
        /// クーポン検索ダイアログ表示
        /// </summary>
        /// <returns>クーポン検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// クーポン検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>クーポン検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["CouponCode"] = Request["CouponCode"];
            form["CouponName"] = Request["CouponName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Coupon> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CouponCode"] = form["CouponCode"];
            ViewData["CouponName"] = form["CouponName"];

            // クーポン検索画面の表示
            return View("CouponCriteriaDialog", list);
        }

        /// <summary>
        /// クーポンマスタ入力画面表示
        /// </summary>
        /// <param name="id">クーポンコード(更新時のみ設定)</param>
        /// <returns>クーポンマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Coupon coupon;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                coupon = new Coupon();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                coupon = new CouponDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(coupon);

            // 出口
            return View("CouponEntry", coupon);
        }

        /// <summary>
        /// クーポンマスタ追加更新
        /// </summary>
        /// <param name="coupon">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>クーポンマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Coupon coupon, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCoupon(coupon);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(coupon);
                return View("CouponEntry", coupon);
            }

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
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
            // データ追加処理
            else
            {
                // データ編集
                coupon = EditCouponForInsert(coupon);

                // データ追加
                db.Coupon.InsertOnSubmit(coupon);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0010", new string[] { "クーポンコード", "保存" }));
                        GetEntryViewData(coupon);
                        return View("CouponEntry", coupon);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// クーポンコードからクーポン名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">クーポンコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
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
        /// 画面表示データの取得
        /// </summary>
        /// <param name="coupon">モデルデータ</param>
        private void GetEntryViewData(Coupon coupon)
        {
            // ブランド名の取得
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
        /// クーポンマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>クーポンマスタ検索結果リスト</returns>
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
        /// 入力チェック
        /// </summary>
        /// <param name="coupon">クーポンデータ</param>
        /// <returns>クーポンデータ</returns>
        private Coupon ValidateCoupon(Coupon coupon)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(coupon.CouponCode))
            {
                ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0001", "クーポンコード"));
            }
            if (string.IsNullOrEmpty(coupon.CouponName))
            {
                ModelState.AddModelError("CouponName", MessageUtils.GetMessage("E0001", "クーポン名"));
            }
            if (string.IsNullOrEmpty(coupon.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "ブランド"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "上代", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "下代", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CouponCode") && !CommonUtils.IsAlphaNumeric(coupon.CouponCode))
            {
                ModelState.AddModelError("CouponCode", MessageUtils.GetMessage("E0012", "クーポンコード"));
            }
            if (ModelState.IsValidField("SalesPrice") && coupon.SalesPrice != null)
            {
                if (!Regex.IsMatch(coupon.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "上代", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Cost") && coupon.Cost != null)
            {
                if (!Regex.IsMatch(coupon.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "下代", "正の整数のみ" }));
                }
            }

            return coupon;
        }

        /// <summary>
        /// クーポンマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="coupon">クーポンデータ(登録内容)</param>
        /// <returns>クーポンマスタモデルクラス</returns>
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
        /// クーポンマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="coupon">クーポンデータ(登録内容)</param>
        /// <returns>クーポンマスタモデルクラス</returns>
        private Coupon EditCouponForUpdate(Coupon coupon)
        {
            coupon.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            coupon.LastUpdateDate = DateTime.Now;
            return coupon;
        }

    }
}
