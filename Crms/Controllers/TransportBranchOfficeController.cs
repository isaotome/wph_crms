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
    /// 運輸支局マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TransportBranchOfficeController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransportBranchOfficeController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 運輸支局検索画面表示
        /// </summary>
        /// <returns>運輸支局検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 運輸支局検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>運輸支局検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<TransportBranchOffice> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["TransportBranchOfficeCode"] = form["TransportBranchOfficeCode"];
            ViewData["TransportBranchOfficeName"] = form["TransportBranchOfficeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 運輸支局検索画面の表示
            return View("TransportBranchOfficeCriteria", list);
        }

        /// <summary>
        /// 運輸支局検索ダイアログ表示
        /// </summary>
        /// <returns>運輸支局検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 運輸支局検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>運輸支局検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["TransportBranchOfficeCode"] = Request["TransportBranchOfficeCode"];
            form["TransportBranchOfficeName"] = Request["TransportBranchOfficeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<TransportBranchOffice> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["TransportBranchOfficeCode"] = form["TransportBranchOfficeCode"];
            ViewData["TransportBranchOfficeName"] = form["TransportBranchOfficeName"];

            // 運輸支局検索画面の表示
            return View("TransportBranchOfficeCriteriaDialog", list);
        }

        /// <summary>
        /// 運輸支局マスタ入力画面表示
        /// </summary>
        /// <param name="id">運輸支局コード(更新時のみ設定)</param>
        /// <returns>運輸支局マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            TransportBranchOffice transportBranchOffice;

            // 表示データ設定(追加の場合)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                transportBranchOffice = new TransportBranchOffice();
            }
            // 表示データ設定(更新の場合)
            else
            {
                ViewData["update"] = "1";
                transportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(id);
            }

            // 出口
            return View("TransportBranchOfficeEntry", transportBranchOffice);
        }

        /// <summary>
        /// 運輸支局マスタ追加更新
        /// </summary>
        /// <param name="transportBranchOffice">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>運輸支局マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(TransportBranchOffice transportBranchOffice, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateTransportBranchOffice(transportBranchOffice);
            if (!ModelState.IsValid)
            {
                return View("TransportBranchOfficeEntry", transportBranchOffice);
            }

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                TransportBranchOffice targetTransportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(transportBranchOffice.TransportBranchOfficeCode);
                UpdateModel(targetTransportBranchOffice);
                EditTransportBranchOfficeForUpdate(targetTransportBranchOffice);
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
                transportBranchOffice = EditTransportBranchOfficeForInsert(transportBranchOffice);

                // データ追加
                db.TransportBranchOffice.InsertOnSubmit(transportBranchOffice);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0010", new string[] { "運輸支局コード", "保存" }));
                        return View("TransportBranchOfficeEntry", transportBranchOffice);
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
        /// 運輸支局コードから運輸支局名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">運輸支局コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                TransportBranchOffice transportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(code);
                if (transportBranchOffice != null)
                {
                    codeData.Code = transportBranchOffice.TransportBranchOfficeCode;
                    codeData.Name = transportBranchOffice.TransportBranchOfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 運輸支局マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>運輸支局マスタ検索結果リスト</returns>
        private PaginatedList<TransportBranchOffice> GetSearchResultList(FormCollection form)
        {
            TransportBranchOfficeDao transportBranchOfficeDao = new TransportBranchOfficeDao(db);
            TransportBranchOffice transportBranchOfficeCondition = new TransportBranchOffice();
            transportBranchOfficeCondition.TransportBranchOfficeCode = form["TransportBranchOfficeCode"];
            transportBranchOfficeCondition.TransportBranchOfficeName = form["TransportBranchOfficeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                transportBranchOfficeCondition.DelFlag = form["DelFlag"];
            }
            return transportBranchOfficeDao.GetListByCondition(transportBranchOfficeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="transportBranchOffice">運輸支局データ</param>
        /// <returns>運輸支局データ</returns>
        private TransportBranchOffice ValidateTransportBranchOffice(TransportBranchOffice transportBranchOffice)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(transportBranchOffice.TransportBranchOfficeCode))
            {
                ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0001", "運輸支局コード"));
            }
            if (string.IsNullOrEmpty(transportBranchOffice.TransportBranchOfficeName))
            {
                ModelState.AddModelError("TransportBranchOfficeName", MessageUtils.GetMessage("E0001", "運輸支局名"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("NormalPaintPrice"))
            {
                ModelState.AddModelError("NormalPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　ノーマル(ペイント)", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("NormalFluorescencePrice"))
            {
                ModelState.AddModelError("NormalFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　ノーマル(字光)", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("OrderPaintPrice"))
            {
                ModelState.AddModelError("OrderPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　希望(ペイント)", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("OrderFluorescencePrice"))
            {
                ModelState.AddModelError("OrderFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　希望(字光)", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("TransportBranchOfficeCode") && !CommonUtils.IsAlphaNumeric(transportBranchOffice.TransportBranchOfficeCode))
            {
                ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0012", "運輸支局コード"));
            }
            if (ModelState.IsValidField("NormalPaintPrice") && transportBranchOffice.NormalPaintPrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.NormalPaintPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("NormalPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　ノーマル(ペイント)", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("NormalFluorescencePrice") && transportBranchOffice.NormalFluorescencePrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.NormalFluorescencePrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("NormalFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　ノーマル(字光)", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("OrderPaintPrice") && transportBranchOffice.OrderPaintPrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.OrderPaintPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("OrderPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　希望(ペイント)", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("OrderFluorescencePrice") && transportBranchOffice.OrderFluorescencePrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.OrderFluorescencePrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("OrderFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "ナンバープレート代　希望(字光)", "正の整数のみ" }));
                }
            }
            
            return transportBranchOffice;
        }

        /// <summary>
        /// 運輸支局マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="transportBranchOffice">運輸支局データ(登録内容)</param>
        /// <returns>運輸支局マスタモデルクラス</returns>
        private TransportBranchOffice EditTransportBranchOfficeForInsert(TransportBranchOffice transportBranchOffice)
        {
            transportBranchOffice.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.CreateDate = DateTime.Now;
            transportBranchOffice.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.LastUpdateDate = DateTime.Now;
            transportBranchOffice.DelFlag = "0";
            return transportBranchOffice;
        }

        /// <summary>
        /// 運輸支局マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="transportBranchOffice">運輸支局データ(登録内容)</param>
        /// <returns>運輸支局マスタモデルクラス</returns>
        private TransportBranchOffice EditTransportBranchOfficeForUpdate(TransportBranchOffice transportBranchOffice)
        {
            transportBranchOffice.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.LastUpdateDate = DateTime.Now;
            return transportBranchOffice;
        }

    }
}
