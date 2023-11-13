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
    /// 仕入先マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SupplierController : Controller
    {

        private static readonly string FORM_NAME = "仕入先マスタ";     // 画面名
        private static readonly string PROC_NAME = "仕入先マスタ登録"; // 処理名


        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SupplierController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 仕入先検索画面表示
        /// </summary>
        /// <returns>仕入先検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 仕入先検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>仕入先検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Supplier> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["SupplierCode"] = form["SupplierCode"];
            ViewData["SupplierName"] = form["SupplierName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 仕入先検索画面の表示
            return View("SupplierCriteria", list);
        }

        /// <summary>
        /// 仕入先検索ダイアログ表示
        /// </summary>
        /// <returns>仕入先検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 仕入先検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>仕入先検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["SupplierCode"] = Request["SupplierCode"];
            form["SupplierName"] = Request["SupplierName"];
            form["OutsourceFlag"] = Request["OutsourceFlag"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, form["OutsourceFlag"] , true);

            // 検索結果リストの取得
            PaginatedList<Supplier> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["SupplierCode"] = form["SupplierCode"];
            ViewData["SupplierName"] = form["SupplierName"];

            // 仕入先検索画面の表示
            return View("SupplierCriteriaDialog", list);
        }

        /// <summary>
        /// 仕入先マスタ入力画面表示
        /// </summary>
        /// <param name="id">仕入先コード(更新時のみ設定)</param>
        /// <returns>仕入先マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Supplier supplier;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                supplier = new Supplier();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                supplier = new SupplierDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(supplier);

            // 出口
            return View("SupplierEntry", supplier);
        }

        /// <summary>
        /// 仕入先マスタ追加更新
        /// </summary>
        /// <param name="supplier">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>仕入先マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Supplier supplier, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateSupplier(supplier);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(supplier);
                return View("SupplierEntry", supplier);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Supplier targetSupplier = new SupplierDao(db).GetByKey(supplier.SupplierCode, true);
                UpdateModel(targetSupplier);
                EditSupplierForUpdate(targetSupplier);
            }
            // データ追加処理
            else
            {
                // データ編集
                supplier = EditSupplierForInsert(supplier);
                // データ追加
                db.Supplier.InsertOnSubmit(supplier);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {   
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0010", new string[] { "仕入先コード", "保存" }));
                        GetEntryViewData(supplier);
                        return View("SupplierEntry", supplier);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
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
            return Entry(supplier.SupplierCode);
        }

        /// <summary>
        /// 仕入先コードから仕入先名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">仕入先コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Supplier supplier = new SupplierDao(db).GetByKey(code);
                if (supplier != null)
                {
                    codeData.Code = supplier.SupplierCode;
                    codeData.Name = supplier.SupplierName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="supplier">モデルデータ</param>
        private void GetEntryViewData(Supplier supplier)
        {
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, supplier.OutsourceFlag, true);
        }

        /// <summary>
        /// 仕入先マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>仕入先マスタ検索結果リスト</returns>
        private PaginatedList<Supplier> GetSearchResultList(FormCollection form)
        {
            SupplierDao supplierDao = new SupplierDao(db);
            Supplier supplierCondition = new Supplier();
            supplierCondition.SupplierCode = form["SupplierCode"];
            supplierCondition.SupplierName = form["SupplierName"];
            supplierCondition.OutsourceFlag = form["OutsourceFlag"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                supplierCondition.DelFlag = form["DelFlag"];
            }
            return supplierDao.GetListByCondition(supplierCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="supplier">仕入先データ</param>
        /// <returns>仕入先データ</returns>
        private Supplier ValidateSupplier(Supplier supplier)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(supplier.SupplierCode))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "仕入先コード"));
            }
            if (string.IsNullOrEmpty(supplier.SupplierName))
            {
                ModelState.AddModelError("SupplierName", MessageUtils.GetMessage("E0001", "仕入先名"));
            }
            if (string.IsNullOrEmpty(supplier.OutsourceFlag))
            {
                ModelState.AddModelError("OutsourceFlag", MessageUtils.GetMessage("E0001", "外注フラグ"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("SupplierCode") && !CommonUtils.IsAlphaNumeric(supplier.SupplierCode))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0012", "仕入先コード"));
            }

            return supplier;
        }

        /// <summary>
        /// 仕入先マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="supplier">仕入先データ(登録内容)</param>
        /// <returns>仕入先マスタモデルクラス</returns>
        private Supplier EditSupplierForInsert(Supplier supplier)
        {
            supplier.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.CreateDate = DateTime.Now;
            supplier.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.LastUpdateDate = DateTime.Now;
            supplier.DelFlag = "0";
            return supplier;
        }

        /// <summary>
        /// 仕入先マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="supplier">仕入先データ(登録内容)</param>
        /// <returns>仕入先マスタモデルクラス</returns>
        private Supplier EditSupplierForUpdate(Supplier supplier)
        {
            supplier.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.LastUpdateDate = DateTime.Now;
            return supplier;
        }

    }
}
