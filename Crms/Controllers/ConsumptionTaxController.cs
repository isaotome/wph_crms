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

namespace Crms.Controllers
{
    /// <summary>
    /// 消費税率マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ConsumptionTaxController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConsumptionTaxController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        ///// <summary>
        ///// 消費税率検索画面表示
        ///// </summary>
        ///// <returns>消費税率検索画面</returns>
        //[AuthFilter]
        //public ActionResult Criteria()
        //{
        //    return Criteria(new FormCollection());
        //}

        ///// <summary>
        ///// 消費税率検索画面表示
        ///// </summary>
        ///// <param name="form">フォームデータ(検索条件)</param>
        ///// <returns>消費税率検索画面</returns>
        //[AuthFilter]
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Criteria(FormCollection form)
        //{
        //    // デフォルト値の設定
        //    form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

        //     検索結果リストの取得
        //    PaginatedList<ConsumptionTax> list = GetSearchResultList(form);

        //     その他出力項目の設定
        //    ViewData["ConsumptionTaxId"] = form["ConsumptionTaxId"];
        //    ViewData["Rate"] = form["Rate"];
        //    ViewData["EmployeeName"] = form["EmployeeName"];
        //    ViewData["DelFlag"] = form["DelFlag"];

        //     消費税率検索画面の表示
        //    return View("ConsumptionTaxCriteria", list);
        //}

        ///// <summary>
        ///// 消費税率検索ダイアログ表示
        ///// </summary>
        ///// <returns>消費税率検索ダイアログ</returns>
        //public ActionResult CriteriaDialog()
        //{
        //    return CriteriaDialog(new FormCollection());
        //}

        ///// <summary>
        ///// 消費税率検索ダイアログ表示
        ///// </summary>
        ///// <param name="form">フォームデータ(検索条件)</param>
        ///// <returns>消費税率検索画面ダイアログ</returns>
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult CriteriaDialog(FormCollection form)
        //{
        //    // 検索条件の設定
        //    // (クエリストリングを検索条件に使用する為、Requestを使用。
        //    //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
        //    form["ConsumptionTaxId"] = Request["ConsumptionTaxId"];
        //    form["Rate"] = Request["Rate"];
        //    form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

        //    // 検索結果リストの取得
        //    PaginatedList<ConsumptionTax> list = GetSearchResultList(form);

        //    // その他出力項目の設定
        //    ViewData["ConsumptionTaxId"] = form["ConsumptionTaxId"];
        //    ViewData["Rate"] = form["Rate"];
        //    ViewData["EmployeeName"] = form["EmployeeName"];

        //    // 消費税率検索画面の表示
        //    return View("ConsumptionTaxCriteriaDialog", list);
        //}

        ///// <summary>
        ///// 消費税率マスタ入力画面表示
        ///// </summary>
        ///// <param name="id">消費税率コード(更新時のみ設定)</param>
        ///// <returns>消費税率マスタ入力画面</returns>
        //[AuthFilter]
        //public ActionResult Entry(string id)
        //{
        //    ConsumptionTax ConsumptionTax;

        //    // 追加の場合
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        ViewData["update"] = "0";
        //        ConsumptionTax = new ConsumptionTax();
        //    }
        //    // 更新の場合
        //    else
        //    {
        //        ViewData["update"] = "1";
        //        ConsumptionTax = new ConsumptionTaxDao(db).GetByKey(id);
        //    }

        //    // その他表示データの取得
        //    GetEntryViewData(ConsumptionTax);

        //    // 出口
        //    return View("ConsumptionTaxEntry", ConsumptionTax);
        //}
        
        ///// <summary>
        ///// 画面表示データの取得
        ///// </summary>
        ///// <param name="ConsumptionTax">モデルデータ</param>
        //private void GetEntryViewData(ConsumptionTax ConsumptionTax)
        //{
        //    // 消費税率データ作成者の取得
        //    if (!string.IsNullOrEmpty(ConsumptionTax.CreateEmployeeCode))
        //    {
        //        EmployeeDao employeeDao = new EmployeeDao(db);
        //        Employee employee = employeeDao.GetByKey(ConsumptionTax.CreateEmployeeCode);
        //        if (employee != null)
        //        {
        //            //ConsumptionTax.CreateEmployeeCode = CreateEmployeeCode;
        //            //ViewData["EmployeeName"] = employee.EmployeeName;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 消費税率マスタ追加更新
        ///// </summary>
        ///// <param name="ConsumptionTax">モデルデータ(登録内容)</param>
        ///// <param name="form">フォームデータ</param>
        ///// <returns>消費税率マスタ入力画面</returns>
        //[AuthFilter]
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Entry(ConsumptionTax ConsumptionTax, FormCollection form)
        //{
        //    // 継続保持する出力情報の設定
        //    ViewData["update"] = form["update"];

        //    // データチェック
        //    ValidateConsumptionTax(ConsumptionTax);
        //    if (!ModelState.IsValid)
        //    {
        //        GetEntryViewData(ConsumptionTax);
        //        return View("ConsumptionTaxEntry", ConsumptionTax);
        //    }

        //    // データ更新処理
        //    if (form["update"].Equals("1"))
        //    {
        //        // データ編集・更新
        //        ConsumptionTax targetConsumptionTax = new ConsumptionTaxDao(db).GetByKey(ConsumptionTax.ConsumptionTaxId);
        //        UpdateModel(targetConsumptionTax);
        //        //EditConsumptionTaxForUpdate(targetConsumptionTax);
        //        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
        //        {
        //            try
        //            {
        //                db.SubmitChanges();
        //                break;
        //            }
        //            catch (ChangeConflictException e)
        //            {
        //                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
        //                {
        //                    occ.Resolve(RefreshMode.KeepCurrentValues);
        //                }
        //                if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
        //                {
        //                    throw e;
        //                }
        //            }
        //        }
        //    }
        //    // データ追加処理
        //    else
        //    {
        //        // データ編集
        //        ConsumptionTax = EditConsumptionTaxForInsert(ConsumptionTax);

        //        // データ追加
        //        db.ConsumptionTax.InsertOnSubmit(ConsumptionTax);
        //        try
        //        {
        //            db.SubmitChanges();
        //        }
        //        catch (SqlException e)
        //        {
        //            if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
        //            {
        //                ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0010", new string[] { "消費税率コード", "保存" }));
        //                GetEntryViewData(ConsumptionTax);
        //                return View("ConsumptionTaxEntry", ConsumptionTax);
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        }
        //    }

        //    // 出口
        //    ViewData["close"] = "1";
        //    return Entry((string)null);
        //}


        ///// <summary>
        ///// 消費税率マスタ検索結果リスト取得
        ///// </summary>
        ///// <param name="form">フォームデータ(検索条件)</param>
        ///// <returns>消費税率マスタ検索結果リスト</returns>
        //private PaginatedList<ConsumptionTax> GetSearchResultList(FormCollection form)
        //{
        //    ConsumptionTaxDao ConsumptionTaxDao = new ConsumptionTaxDao(db);
        //    ConsumptionTax ConsumptionTaxCondition = new ConsumptionTax();
        //    ConsumptionTaxCondition.ConsumptionTaxId = form["ConsumptionTaxId"];
        //    ConsumptionTaxCondition.RateName = form["RateName"];
        //    ConsumptionTaxCondition.Rate = form["Rate"];
        //    ConsumptionTaxCondition.FromAvailableDate = form["FromAvailableDate"];
        //    ConsumptionTaxCondition.ToAvailableDate = form["ToAvailableDate"];
        //    if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
        //    {
        //        ConsumptionTaxCondition.DelFlag = form["DelFlag"];
        //    }
        //    return ConsumptionTaxDao.GetListByCondition(ConsumptionTaxCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        //}

        ///// <summary>
        ///// 入力チェック
        ///// </summary>
        ///// <param name="ConsumptionTax">消費税率データ</param>
        ///// <returns>消費税率データ</returns>
        //private ConsumptionTax ValidateConsumptionTax(ConsumptionTax ConsumptionTax)
        //{
        //    // 必須チェック
        //    if (string.IsNullOrEmpty(ConsumptionTax.ConsumptionTaxId))
        //    {
        //        ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0001", "消費税率コード"));
        //    }
        //    if (string.IsNullOrEmpty(ConsumptionTax.RateName))
        //    {
        //        ModelState.AddModelError("RateName", MessageUtils.GetMessage("E0001", "消費税率名"));
        //    }
        //    //if (string.IsNullOrEmpty.(ConsumptionTax.Rate))
        //    //{
        //    //    ModelState.AddModelError("Rate", MessageUtils.GetMessage("E0001", "消費税率"));
        //    //}

        //    // フォーマットチェック
        //    if (ModelState.IsValidField("ConsumptionTaxId") && !CommonUtils.IsAlphaNumeric(ConsumptionTax.ConsumptionTaxId))
        //    {
        //        ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0012", "消費税率コード"));
        //    }

        //    return ConsumptionTax;
        //}

        ///// <summary>
        ///// 消費税率マスタ追加データ編集(フレームワーク外の補完編集)
        ///// </summary>
        ///// <param name="ConsumptionTax">消費税率データ(登録内容)</param>
        ///// <returns>消費税率マスタモデルクラス</returns>
        //private ConsumptionTax EditConsumptionTaxForInsert(ConsumptionTax ConsumptionTax)
        //{
        //    ConsumptionTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        //    ConsumptionTax.CreateDate = DateTime.Now;
        //    ConsumptionTax.DelFlag = "0";
        //    return ConsumptionTax;
        //}

        /// <summary>
        /// 消費税率IDから消費税率を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">消費税率コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        [AcceptVerbs(HttpVerbs.Post)]           //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetRateById (string ConsumptionTaxId)
        {
            System.Nullable<int> Rate = null;
            
            if (Request.IsAjaxRequest())
            {
                //Edit 2014/06/20 arc yano 税率変更バグ対応 ConsumptionTaxIdがnullの対処
                if (ConsumptionTaxId != null)
                {
                    Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(ConsumptionTaxId));
                }
                
                Dictionary<string, System.Nullable<int>> ret = new Dictionary<string, System.Nullable<int>>();
                if (Rate != null)
                {
                    ret.Add("Rate", Rate);
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetIdByDate(string strDt)
        {
            string ConsumptionTaxId = null;
           
            if (Request.IsAjaxRequest())
            {
                
                //Add 2014/06/20 arc yano 税率変更バグ対応 strDtがnullの対処
                if (strDt != null)
                {
                    ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Parse(strDt));
                }
                
                Dictionary<string, string> ret = new Dictionary<string, string>();
                
                if (ConsumptionTaxId != null)
                {
                    ret.Add("ConsumptionTaxId", ConsumptionTaxId);
                }

                return Json(ret);
            }
            return new EmptyResult();
        }

    }
}
