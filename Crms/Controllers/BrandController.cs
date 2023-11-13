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
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// ブランドマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BrandController : Controller
    {

        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "ブランドマスタ";     // 画面名
        private static readonly string PROC_NAME = "ブランドマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BrandController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ブランド検索画面表示
        /// </summary>
        /// <returns>ブランド検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ブランド検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ブランド検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            
            // 検索結果リストの取得
            PaginatedList<Brand> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ブランド検索画面の表示
            return View("BrandCriteria", list);
        }

        /// <summary>
        /// ブランド検索ダイアログ表示
        /// </summary>
        /// <returns>ブランド検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ブランド検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ブランド検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Brand> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];

            // ブランド検索画面の表示
            return View("BrandCriteriaDialog", list);
        }

        /// <summary>
        /// ブランドマスタ入力画面表示
        /// </summary>
        /// <param name="id">ブランドコード(更新時のみ設定)</param>
        /// <returns>ブランドマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Brand brand;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                brand = new Brand();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                brand = new BrandDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(brand);

            // 出口
            return View("BrandEntry", brand);
        }

        /// <summary>
        /// ブランドマスタ追加更新
        /// </summary>
        /// <param name="brand">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>ブランドマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Brand brand, FormCollection form)
        {

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateBrand(brand);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(brand);
                return View("BrandEntry", brand);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Brand targetBrand = new BrandDao(db).GetByKey(brand.CarBrandCode, true);
                UpdateModel(targetBrand);
                EditBrandForUpdate(targetBrand);
            }
            // データ追加処理
            else
            {
                // データ編集
                brand = EditBrandForInsert(brand);

                // データ追加
                db.Brand.InsertOnSubmit(brand);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力処理追加
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
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (SqlException e)
                {
                    //Add 2014/08/04 arc amii エラーログ対応 エラーログ出力処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0010", new string[] { "ブランドコード", "保存" }));
                        GetEntryViewData(brand);
                        return View("BrandEntry", brand);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/04 arc amii エラーログ対応 ChangeConflictException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(brand.CarBrandCode);
        }

        /// <summary>
        /// ブランドコードからブランド名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">ブランドコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
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
        /// 画面表示データの取得
        /// </summary>
        /// <param name="brand">モデルデータ</param>
        private void GetEntryViewData(Brand brand)
        {
            // メーカー名の取得
            if (!string.IsNullOrEmpty(brand.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(brand.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            // 会社名の取得
            if (!string.IsNullOrEmpty(brand.CompanyCode)) {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(brand.CompanyCode);
                if (company != null) {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }
        }

        /// <summary>
        /// ブランドマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ブランドマスタ検索結果リスト</returns>
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
        /// 入力チェック
        /// </summary>
        /// <param name="brand">ブランドデータ</param>
        /// <returns>ブランドデータ</returns>
        private Brand ValidateBrand(Brand brand)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(brand.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "ブランドコード"));
            }
            if (string.IsNullOrEmpty(brand.CarBrandName))
            {
                ModelState.AddModelError("CarBrandName", MessageUtils.GetMessage("E0001", "ブランド名"));
            }
            if (string.IsNullOrEmpty(brand.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "メーカー"));
            }
            if (string.IsNullOrEmpty(brand.CompanyCode)) {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "会社"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CarBrandCode") && !CommonUtils.IsAlphaNumeric(brand.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0012", "ブランドコード"));
            }
            if (!ModelState.IsValidField("LaborRate")) {
                ModelState.AddModelError("LaborRate", MessageUtils.GetMessage("E0004", new string[] { "レバレート", "(0以上の整数のみ)" }));
            }
            return brand;
        }

        /// <summary>
        /// ブランドマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="brand">ブランドデータ(登録内容)</param>
        /// <returns>ブランドマスタモデルクラス</returns>
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
        /// ブランドマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="brand">ブランドデータ(登録内容)</param>
        /// <returns>ブランドマスタモデルクラス</returns>
        private Brand EditBrandForUpdate(Brand brand)
        {
            brand.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            brand.LastUpdateDate = DateTime.Now;
            return brand;
        }

    }
}
