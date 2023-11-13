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
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 支払種別マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PaymentKindController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "支払種別マスタ";     // 画面名
        private static readonly string PROC_NAME = "支払種別マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PaymentKindController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 支払種別検索画面表示
        /// </summary>
        /// <returns>支払種別検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 支払種別検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払種別検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<PaymentKind> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["PaymentKindCode"] = form["PaymentKindCode"];
            ViewData["PaymentKindName"] = form["PaymentKindName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 支払種別検索画面の表示
            return View("PaymentKindCriteria", list);
        }

        /// <summary>
        /// 支払種別検索ダイアログ表示
        /// </summary>
        /// <returns>支払種別検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 支払種別検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払種別検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["PaymentKindCode"] = Request["PaymentKindCode"];
            form["PaymentKindName"] = Request["PaymentKindName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<PaymentKind> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["PaymentKindCode"] = form["PaymentKindCode"];
            ViewData["PaymentKindName"] = form["PaymentKindName"];

            // 支払種別検索画面の表示
            return View("PaymentKindCriteriaDialog", list);
        }

        /// <summary>
        /// 支払種別マスタ入力画面表示
        /// </summary>
        /// <param name="id">支払種別コード(更新時のみ設定)</param>
        /// <returns>支払種別マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            PaymentKind paymentKind;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                paymentKind = new PaymentKind();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                paymentKind = new PaymentKindDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(paymentKind);

            // 出口
            return View("PaymentKindEntry", paymentKind);
        }

        /// <summary>
        /// 支払種別マスタ追加更新
        /// </summary>
        /// <param name="paymentKind">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>支払種別マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PaymentKind paymentKind, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidatePaymentKind(paymentKind);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(paymentKind);
                return View("PaymentKindEntry", paymentKind);
            }

            // Add 2014/08/01 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                PaymentKind targetPaymentKind = new PaymentKindDao(db).GetByKey(paymentKind.PaymentKindCode, true);
                UpdateModel(targetPaymentKind);
                EditPaymentKindForUpdate(targetPaymentKind); 
            }
            // データ追加処理
            else
            {
                // データ編集
                paymentKind = EditPaymentKindForInsert(paymentKind);
                // データ追加
                db.PaymentKind.InsertOnSubmit(paymentKind);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力処理追加
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
                catch (SqlException e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0010", new string[] { "支払種別コード", "保存" }));
                        GetEntryViewData(paymentKind);
                        return View("PaymentKindEntry", paymentKind);
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
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(paymentKind.PaymentKindCode);
        }

        /// <summary>
        /// 支払種別コードから支払種別名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">支払種別コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                PaymentKind paymentKind = new PaymentKindDao(db).GetByKey(code);
                if (paymentKind != null)
                {
                    codeData.Code = paymentKind.PaymentKindCode;
                    codeData.Name = paymentKind.PaymentKindName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="paymentKind">モデルデータ</param>
        private void GetEntryViewData(PaymentKind paymentKind)
        {
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["PaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetPaymentTypeAll(false), paymentKind.PaymentType, true);
        }

        /// <summary>
        /// 支払種別マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>支払種別マスタ検索結果リスト</returns>
        private PaginatedList<PaymentKind> GetSearchResultList(FormCollection form)
        {
            PaymentKindDao paymentKindDao = new PaymentKindDao(db);
            PaymentKind paymentKindCondition = new PaymentKind();
            paymentKindCondition.PaymentKindCode = form["PaymentKindCode"];
            paymentKindCondition.PaymentKindName = form["PaymentKindName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                paymentKindCondition.DelFlag = form["DelFlag"];
            }
            return paymentKindDao.GetListByCondition(paymentKindCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="paymentKind">支払種別データ</param>
        /// <returns>支払種別データ</returns>
        private PaymentKind ValidatePaymentKind(PaymentKind paymentKind)
        {
            // 必須チェック(数値必須項目は属性チェックも兼ねる)
            if (string.IsNullOrEmpty(paymentKind.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0001", "支払種別コード"));
            }
            if (string.IsNullOrEmpty(paymentKind.PaymentKindName))
            {
                ModelState.AddModelError("PaymentKindName", MessageUtils.GetMessage("E0001", "支払種別名"));
            }
            if (!ModelState.IsValidField("CommissionRate"))
            {
                ModelState.AddModelError("CommissionRate", MessageUtils.GetMessage("E0002", new string[] { "手数料率", "正の整数3桁以内かつ小数5桁以内" }));
                if (ModelState["CommissionRate"].Errors.Count > 1)
                {
                    ModelState["CommissionRate"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("ClaimDay"))
            {
                ModelState.AddModelError("ClaimDay", MessageUtils.GetMessage("E0002", new string[] { "締め日", "0～31" }));
                if (ModelState["ClaimDay"].Errors.Count > 1)
                {
                    ModelState["ClaimDay"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(paymentKind.PaymentType))
            {
                ModelState.AddModelError("PaymentType", MessageUtils.GetMessage("E0001", "支払区分"));
            }
            if (!ModelState.IsValidField("PaymentDay"))
            {
                ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0002", new string[] { "支払日", "0～31" }));
                if (ModelState["PaymentDay"].Errors.Count > 1)
                {
                    ModelState["PaymentDay"].Errors.RemoveAt(0);
                }
            }

            // フォーマットチェック
            if (ModelState.IsValidField("PaymentKindCode") && !CommonUtils.IsAlphaNumeric(paymentKind.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0012", "支払種別コード"));
            }
            if (ModelState.IsValidField("CommissionRate"))
            {
                if ((Regex.IsMatch(paymentKind.CommissionRate.ToString(), @"^\d{1,3}\.\d{1,5}$"))
                    || (Regex.IsMatch(paymentKind.CommissionRate.ToString(), @"^\d{1,3}$")))
                {
                }
                else
                {
                    ModelState.AddModelError("CommissionRate", MessageUtils.GetMessage("E0002", new string[] { "手数料率", "正の整数3桁以内かつ小数5桁以内" }));
                }
            }

            // 値チェック
            if (ModelState.IsValidField("ClaimDay"))
            {
                if (paymentKind.ClaimDay < 0 || paymentKind.ClaimDay > 31)
                {
                    ModelState.AddModelError("ClaimDay", MessageUtils.GetMessage("E0002", new string[] { "締め日", "0～31" }));
                }
            }
            if (ModelState.IsValidField("PaymentDay"))
            {
                if (paymentKind.PaymentDay < 0 || paymentKind.PaymentDay > 31)
                {
                    ModelState.AddModelError("PaymentDay", MessageUtils.GetMessage("E0002", new string[] { "支払日", "0～31" }));
                }
            }

            return paymentKind;
        }

        /// <summary>
        /// 支払種別マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="paymentKind">支払種別データ(登録内容)</param>
        /// <returns>支払種別マスタモデルクラス</returns>
        private PaymentKind EditPaymentKindForInsert(PaymentKind paymentKind)
        {
            paymentKind.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.CreateDate = DateTime.Now;
            paymentKind.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.LastUpdateDate = DateTime.Now;
            paymentKind.DelFlag = "0";
            return paymentKind;
        }

        /// <summary>
        /// 支払種別マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="paymentKind">支払種別データ(登録内容)</param>
        /// <returns>支払種別マスタモデルクラス</returns>
        private PaymentKind EditPaymentKindForUpdate(PaymentKind paymentKind)
        {
            paymentKind.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            paymentKind.LastUpdateDate = DateTime.Now;
            return paymentKind;
        }

    }
}
