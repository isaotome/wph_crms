using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 決済条件マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerClaimableController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "決済条件マスタ";     // 画面名
        private static readonly string PROC_NAME = "決済条件マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CustomerClaimableController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 決済条件検索画面表示
        /// </summary>
        /// <returns>決済条件検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 決済条件検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>決済条件検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // 削除処理
            if (form["action"].Equals("remove"))
            {
                CustomerClaimable targetCustomerClaimable = new CustomerClaimableDao(db).GetByKey(form["key1"], form["key2"]);
                db.CustomerClaimable.DeleteOnSubmit(targetCustomerClaimable);
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
                            // Mod 2014/08/04 arc amii エラーログ対応 ログファイルにエラーを出力するよう修正
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
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
            }

            // 検索結果リストの取得
            PaginatedList<CustomerClaimable> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            if (!string.IsNullOrEmpty(form["CustomerClaimCode"]))
            {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(form["CustomerClaimCode"]);
                if (customerClaim != null)
                {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }

            // 決済条件検索画面の表示
            return View("CustomerClaimableCriteria", list);
        }

        /// <summary>
        /// 決済条件マスタ入力画面表示
        /// </summary>
        /// <param name="id">決済条件コード(更新時のみ設定)</param>
        /// <returns>決済条件マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CustomerClaimable customerClaimable;

            // 表示データ設定
            customerClaimable = new CustomerClaimable();
            customerClaimable.CustomerClaimCode = id;
            GetEntryViewData(customerClaimable);

            // 出口
            return View("CustomerClaimableEntry", customerClaimable);
        }

        /// <summary>
        /// 決済条件マスタ追加更新
        /// </summary>
        /// <param name="customerClaimable">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>決済条件マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerClaimable customerClaimable, FormCollection form)
        {
            // データチェック
            ValidateCustomerClaimable(customerClaimable);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(customerClaimable);
                return View("CustomerClaimableEntry", customerClaimable);
            }

            // データ編集
            customerClaimable = EditCustomerClaimableForInsert(customerClaimable);

            // Add 2014/08/01 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ追加
            db.CustomerClaimable.InsertOnSubmit(customerClaimable);
            try
            {
                db.SubmitChanges();
            }
            catch (SqlException e)
            {
                // Mod 2014/08/01 arc amii エラーログ対応 ログファイルにエラーを出力するよう修正
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;

                if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                {
                    // ログに出力
                    OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                    ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0010", new string[] { "支払種別", "保存" }));
                    GetEntryViewData(customerClaimable);
                    return View("CustomerClaimableEntry", customerClaimable);
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
                //Add 2014/08/01 arc amii エラーログ対応 SqlException以外の時のエラー処理追加
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ログに出力
                OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                // エラーページに遷移
                return View("Error");
            }

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// グレードコード，オプションコードからデータを取得する(Ajax専用）
        /// </summary>
        /// <param name="code1">グレードコード</param>
        /// <param name="code2">オプションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code1, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CustomerClaimable customerClaimable = new CustomerClaimableDao(db).GetByKey(code1, code2);
                if (customerClaimable != null)
                {
                    codeData.Code = customerClaimable.CustomerClaimCode;
                    codeData.Code2 = customerClaimable.PaymentKindCode;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="customerClaimable">モデルデータ</param>
        private void GetEntryViewData(CustomerClaimable customerClaimable)
        {
            // 支払種別名の取得
            if (!string.IsNullOrEmpty(customerClaimable.PaymentKindCode))
            {
                PaymentKindDao paymentKindDao = new PaymentKindDao(db);
                PaymentKind paymentKind = paymentKindDao.GetByKey(customerClaimable.PaymentKindCode);
                if (paymentKind != null)
                {
                    ViewData["PaymentKindName"] = paymentKind.PaymentKindName;
                }
            }
        }

        /// <summary>
        /// 決済条件マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>決済条件マスタ検索結果リスト</returns>
        private PaginatedList<CustomerClaimable> GetSearchResultList(FormCollection form)
        {
            CustomerClaimableDao customerClaimableDao = new CustomerClaimableDao(db);
            CustomerClaimable customerClaimableCondition = new CustomerClaimable();
            customerClaimableCondition.CustomerClaimCode = form["CustomerClaimCode"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                customerClaimableCondition.DelFlag = form["DelFlag"];
            }
            return customerClaimableDao.GetListByCondition(customerClaimableCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="customerClaimable">決済条件データ</param>
        /// <returns>決済条件データ</returns>
        private CustomerClaimable ValidateCustomerClaimable(CustomerClaimable customerClaimable)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(customerClaimable.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0001", "支払種別"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("PaymentKindCode") && !CommonUtils.IsAlphaNumeric(customerClaimable.PaymentKindCode))
            {
                ModelState.AddModelError("PaymentKindCode", MessageUtils.GetMessage("E0012", "支払種別"));
            }

            return customerClaimable;
        }

        /// <summary>
        /// 決済条件マスタ追加データ編集
        /// </summary>
        /// <param name="customerClaimable">決済条件データ(登録内容)</param>
        /// <returns>決済条件マスタモデルクラス</returns>
        private CustomerClaimable EditCustomerClaimableForInsert(CustomerClaimable customerClaimable)
        {
            customerClaimable.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaimable.CreateDate = DateTime.Now;
            customerClaimable.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaimable.LastUpdateDate = DateTime.Now;
            customerClaimable.DelFlag = "0";
            return customerClaimable;
        }

    }
}
