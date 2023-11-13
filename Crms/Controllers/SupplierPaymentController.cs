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
using Crms.Models;                      //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 支払先マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SupplierPaymentController : Controller
    {
        //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "支払先マスタ";     // 画面名
        private static readonly string PROC_NAME = "支払先マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SupplierPaymentController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 支払先検索画面表示
        /// </summary>
        /// <returns>支払先検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 支払先検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払先検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<SupplierPayment> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            ViewData["SupplierPaymentName"] = form["SupplierPaymentName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 支払先検索画面の表示
            return View("SupplierPaymentCriteria", list);
        }

        /// <summary>
        /// 支払先検索ダイアログ表示
        /// </summary>
        /// <returns>支払先検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 支払先検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払先検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["SupplierPaymentCode"] = Request["SupplierPaymentCode"];
            form["SupplierPaymentName"] = Request["SupplierPaymentName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<SupplierPayment> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            ViewData["SupplierPaymentName"] = form["SupplierPaymentName"];

            // 支払先検索画面の表示
            return View("SupplierPaymentCriteriaDialog", list);
        }

        /// <summary>
        /// 支払先マスタ入力画面表示
        /// </summary>
        /// <param name="id">支払先コード(更新時のみ設定)</param>
        /// <returns>支払先マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            SupplierPayment supplierPayment;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                supplierPayment = new SupplierPayment();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                supplierPayment = new SupplierPaymentDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(supplierPayment);

            // 出口
            return View("SupplierPaymentEntry", supplierPayment);
        }

        /// <summary>
        /// 支払先マスタ追加更新
        /// </summary>
        /// <param name="supplierPayment">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払先マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SupplierPayment supplierPayment, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateSupplierPayment(supplierPayment,form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(supplierPayment);
                return View("SupplierPaymentEntry", supplierPayment);
            }

            // Add 2014/08/12 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                SupplierPayment targetSupplierPayment = new SupplierPaymentDao(db).GetByKey(supplierPayment.SupplierPaymentCode);
                UpdateModel(targetSupplierPayment);
                EditSupplierPaymentForUpdate(targetSupplierPayment);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                supplierPayment = EditSupplierPaymentForInsert(supplierPayment);

                // データ追加
                db.SupplierPayment.InsertOnSubmit(supplierPayment);
            }

            //Add 2014/08/12 arc amii エラーログ対応 SubmitChanges処理を一本化 & Exception時にエラーログ出力処理追加
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
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0010", new string[] { "支払先コード", "保存" }));
                        GetEntryViewData(supplierPayment);
                        return View("SupplierPaymentEntry", supplierPayment);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
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

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// 支払先コードから支払先名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">支払先コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                SupplierPayment supplierPayment = new SupplierPaymentDao(db).GetByKey(code);
                if (supplierPayment != null)
                {
                    codeData.Code = supplierPayment.SupplierPaymentCode;
                    codeData.Name = supplierPayment.SupplierPaymentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="supplierPayment">モデルデータ</param>
        private void GetEntryViewData(SupplierPayment supplierPayment)
        {
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["SupplierPaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSupplierPaymentTypeAll(false), supplierPayment.SupplierPaymentType, true);
            ViewData["PaymentType2List"] = CodeUtils.GetSelectListByModel(dao.GetPaymentType2All(false), supplierPayment.PaymentType, true);
        }

        /// <summary>
        /// 支払先マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払先マスタ検索結果リスト</returns>
        private PaginatedList<SupplierPayment> GetSearchResultList(FormCollection form)
        {
            SupplierPaymentDao supplierPaymentDao = new SupplierPaymentDao(db);
            SupplierPayment supplierPaymentCondition = new SupplierPayment();
            supplierPaymentCondition.SupplierPaymentCode = form["SupplierPaymentCode"];
            supplierPaymentCondition.SupplierPaymentName = form["SupplierPaymentName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                supplierPaymentCondition.DelFlag = form["DelFlag"];
            }
            return supplierPaymentDao.GetListByCondition(supplierPaymentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="supplierPayment">支払先データ</param>
        /// <returns>支払先データ</returns>
        private SupplierPayment ValidateSupplierPayment(SupplierPayment supplierPayment,FormCollection form)
        {
            // 必須チェック(数値必須項目は属性チェックも兼ねる)
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentCode))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0001", "支払先コード"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentName))
            {
                ModelState.AddModelError("SupplierPaymentName", MessageUtils.GetMessage("E0001", "支払先名"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentType))
            {
                ModelState.AddModelError("SupplierPaymentType", MessageUtils.GetMessage("E0001", "支払先種別"));
            }
            if (string.IsNullOrEmpty(supplierPayment.PaymentType))
            {
                ModelState.AddModelError("PaymentType", MessageUtils.GetMessage("E0001", "支払区分"));
            }
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            if ((CommonUtils.DefaultString(supplierPayment.PaymentType).Equals("003")) && (supplierPayment.PaymentDayCount == null))
            {
                ModelState.AddModelError("PaymentDayCount", MessageUtils.GetMessage("E0008", new string[] { "支払区分がn日後", "日数", "5～240" }));
            }

            // 属性チェック
            if (!ModelState.IsValidField("PaymentDay"))
            {
                ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "支払日", "0～31" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod1")) {
                ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0004", new string[] { "猶予日数1", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod2")) {
                ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0004", new string[] { "猶予日数2", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod3")) {
                ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0004", new string[] { "猶予日数3", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod4")) {
                ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0004", new string[] { "猶予日数4", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod5")) {
                ModelState.AddModelError("PaymentPeriod5", MessageUtils.GetMessage("E0004", new string[] { "猶予日数5", "0以外の正の整数" }));
            }
            if (!ModelState.IsValidField("PaymentPeriod6")) {
                ModelState.AddModelError("PaymentPeriod6", MessageUtils.GetMessage("E0004", new string[] { "猶予日数6", "0以外の正の整数" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("SupplierPaymentCode") && !CommonUtils.IsAlphaNumeric(supplierPayment.SupplierPaymentCode))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0012", "支払先コード"));
            }

            if (string.IsNullOrEmpty(form["PaymentRate1"]) || (Regex.IsMatch(form["PaymentRate1"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate1"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate1", MessageUtils.GetMessage("E0004", new string[] { "発生金利1", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate2"]) || (Regex.IsMatch(form["PaymentRate2"], @"^\d{1,3}\.\d{1,3}$"))
                            || (Regex.IsMatch(form["PaymentRate2"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate2", MessageUtils.GetMessage("E0004", new string[] { "発生金利2", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate3"]) || (Regex.IsMatch(form["PaymentRate3"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate3"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate3", MessageUtils.GetMessage("E0004", new string[] { "発生金利3", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate4"]) || (Regex.IsMatch(form["PaymentRate4"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate4"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate4", MessageUtils.GetMessage("E0004", new string[] { "発生金利4", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if ((string.IsNullOrEmpty(form["PaymentRate5"]) || Regex.IsMatch(form["PaymentRate5"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate5"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate5", MessageUtils.GetMessage("E0004", new string[] { "発生金利5", "正の整数3桁以内かつ小数3桁以内" }));
            }
            if (string.IsNullOrEmpty(form["PaymentRate6"]) || (Regex.IsMatch(form["PaymentRate6"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["PaymentRate6"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("PaymentRate6", MessageUtils.GetMessage("E0004", new string[] { "発生金利6", "正の整数3桁以内かつ小数3桁以内" }));
            }


            //猶予日数が入力されている場合、金利は入力必須
            if (!string.IsNullOrEmpty(form["PaymentPeriod1"]) && string.IsNullOrEmpty(form["PaymentRate1"])) {
                ModelState.AddModelError("PaymentRate1", MessageUtils.GetMessage("E0001", new string[] { "発生金利1" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod2"]) && string.IsNullOrEmpty(form["PaymentRate2"])) {
                ModelState.AddModelError("PaymentRate2", MessageUtils.GetMessage("E0001", new string[] { "発生金利2" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod3"]) && string.IsNullOrEmpty(form["PaymentRate3"])) {
                ModelState.AddModelError("PaymentRate3", MessageUtils.GetMessage("E0001", new string[] { "発生金利3" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod4"]) && string.IsNullOrEmpty(form["PaymentRate4"])) {
                ModelState.AddModelError("PaymentRate4", MessageUtils.GetMessage("E0001", new string[] { "発生金利4" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod5"]) && string.IsNullOrEmpty(form["PaymentRate5"])) {
                ModelState.AddModelError("PaymentRate5", MessageUtils.GetMessage("E0001", new string[] { "発生金利5" }));
            }
            if (!string.IsNullOrEmpty(form["PaymentPeriod6"]) && string.IsNullOrEmpty(form["PaymentRate6"])) {
                ModelState.AddModelError("PaymentRate6", MessageUtils.GetMessage("E0001", new string[] { "発生金利6" }));
            }


            // 値チェック
            if (ModelState.IsValidField("PaymentDayCount"))
            {
                if (supplierPayment.PaymentDayCount < 5 || supplierPayment.PaymentDayCount > 240)
                {
                    ModelState.AddModelError("PaymentDayCount", MessageUtils.GetMessage("E0004", new string[] { "日数", "5～240" }));
                }
            }
            if (ModelState.IsValidField("PaymentDay"))
            {
                if (supplierPayment.PaymentDay < 0 || supplierPayment.PaymentDay > 31)
                {
                    ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "支払日", "0～31" }));
                }
            }

            //前提条件のチェック
            if (!string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (supplierPayment.PaymentPeriod1 >= supplierPayment.PaymentPeriod2) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", "猶予日数2は猶予日数1より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (supplierPayment.PaymentPeriod2 >= supplierPayment.PaymentPeriod3) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", "猶予日数3は猶予日数2より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (supplierPayment.PaymentPeriod3 >= supplierPayment.PaymentPeriod4) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", "猶予日数4は猶予日数3より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod5"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "猶予日数4" }));
                    }
                }
                if (supplierPayment.PaymentPeriod4 >= supplierPayment.PaymentPeriod5) {
                    if (ModelState["PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod5", "猶予日数5は猶予日数4より大きな数字である必要があります");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["PaymentPeriod6"])) {
                if (string.IsNullOrEmpty(form["PaymentPeriod1"])) {
                    if (ModelState["PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "猶予日数1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod2"])) {
                    if (ModelState["PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "猶予日数2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod3"])) {
                    if (ModelState["PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "猶予日数3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod4"])) {
                    if (ModelState["PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "猶予日数4" }));
                    }
                }
                if (string.IsNullOrEmpty(form["PaymentPeriod5"])) {
                    if (ModelState["PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod5", MessageUtils.GetMessage("E0001", new string[] { "猶予日数5" }));
                    }
                }
                if (supplierPayment.PaymentPeriod5 >= supplierPayment.PaymentPeriod6) {
                    if (ModelState["PaymentPeriod6"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod6", "猶予日数6は猶予日数5より大きな数字である必要があります");
                    }
                }
            }
            return supplierPayment;
        }

        /// <summary>
        /// 支払先マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="supplierPayment">支払先データ(登録内容)</param>
        /// <returns>支払先マスタモデルクラス</returns>
        private SupplierPayment EditSupplierPaymentForInsert(SupplierPayment supplierPayment)
        {
            supplierPayment.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.CreateDate = DateTime.Now;
            supplierPayment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.LastUpdateDate = DateTime.Now;
            supplierPayment.DelFlag = "0";
            return supplierPayment;
        }

        /// <summary>
        /// 支払先マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="supplierPayment">支払先データ(登録内容)</param>
        /// <returns>支払先マスタモデルクラス</returns>
        private SupplierPayment EditSupplierPaymentForUpdate(SupplierPayment supplierPayment)
        {
            supplierPayment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplierPayment.LastUpdateDate = DateTime.Now;
            return supplierPayment;
        }

    }
}
