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
    /// ローンマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class LoanController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "ローンマスタ";     // 画面名
        private static readonly string PROC_NAME = "ローンマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LoanController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ローン検索画面表示
        /// </summary>
        /// <returns>ローン検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ローン検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ローン検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Loan> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["LoanCode"] = form["LoanCode"];
            ViewData["LoanName"] = form["LoanName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ローン検索画面の表示
            return View("LoanCriteria", list);
        }

        /// <summary>
        /// ローン検索ダイアログ表示
        /// </summary>
        /// <returns>ローン検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ローン検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ローン検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CustomerClaimCode"] = Request["CustomerClaimCode"];
            form["CustomerClaimName"] = Request["CustomerClaimName"];
            form["LoanCode"] = Request["LoanCode"];
            form["LoanName"] = Request["LoanName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Loan> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["LoanCode"] = form["LoanCode"];
            ViewData["LoanName"] = form["LoanName"];

            // ローン検索画面の表示
            return View("LoanCriteriaDialog", list);
        }

        /// <summary>
        /// ローンマスタ入力画面表示
        /// </summary>
        /// <param name="id">ローンコード(更新時のみ設定)</param>
        /// <returns>ローンマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Loan loan;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                loan = new Loan();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                loan = new LoanDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(loan);

            // 出口
            return View("LoanEntry", loan);
        }

        /// <summary>
        /// ローンマスタ追加更新
        /// </summary>
        /// <param name="loan">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>ローンマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Loan loan, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateLoan(loan);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(loan);
                return View("LoanEntry", loan);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Loan targetLoan = new LoanDao(db).GetByKey(loan.LoanCode, true);
                UpdateModel(targetLoan);
                EditLoanForUpdate(targetLoan);
            }
            // データ追加処理
            else
            {
                // データ編集
                loan = EditLoanForInsert(loan);
                // データ追加
                db.Loan.InsertOnSubmit(loan);
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
                catch (SqlException se)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0010", new string[] { "ローンコード", "保存" }));
                        GetEntryViewData(loan);
                        return View("LoanEntry", loan);
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
            return Entry(loan.LoanCode);
        }

        /// <summary>
        /// ローンコードからローン名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">ローンコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Loan loan = new LoanDao(db).GetByKey(code);
                if (loan != null)
                {
                    codeData.Code = loan.LoanCode;
                    codeData.Name = loan.LoanName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="loan">モデルデータ</param>
        private void GetEntryViewData(Loan loan)
        {
            // 請求先名の取得
            if (!string.IsNullOrEmpty(loan.CustomerClaimCode))
            {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(loan.CustomerClaimCode);
                if (customerClaim != null)
                {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }
        }

        /// <summary>
        /// ローンマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ローンマスタ検索結果リスト</returns>
        private PaginatedList<Loan> GetSearchResultList(FormCollection form)
        {
            LoanDao loanDao = new LoanDao(db);
            Loan loanCondition = new Loan();
            loanCondition.LoanCode = form["LoanCode"];
            loanCondition.LoanName = form["LoanName"];
            loanCondition.CustomerClaim = new CustomerClaim();
            loanCondition.CustomerClaim.CustomerClaimCode = form["CustomerClaimCode"];
            loanCondition.CustomerClaim.CustomerClaimName = form["CustomerClaimName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                loanCondition.DelFlag = form["DelFlag"];
            }
            return loanDao.GetListByCondition(loanCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="loan">ローンデータ</param>
        /// <returns>ローンデータ</returns>
        private Loan ValidateLoan(Loan loan)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(loan.LoanCode))
            {
                ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0001", "ローンコード"));
            }
            if (string.IsNullOrEmpty(loan.LoanName))
            {
                ModelState.AddModelError("LoanName", MessageUtils.GetMessage("E0001", "ローン名"));
            }
            if (string.IsNullOrEmpty(loan.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0001", "請求先"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("LoanCode") && !CommonUtils.IsAlphaNumeric(loan.LoanCode))
            {
                ModelState.AddModelError("LoanCode", MessageUtils.GetMessage("E0012", "ローンコード"));
            }

            return loan;
        }

        /// <summary>
        /// ローンマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="loan">ローンデータ(登録内容)</param>
        /// <returns>ローンマスタモデルクラス</returns>
        private Loan EditLoanForInsert(Loan loan)
        {
            loan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.CreateDate = DateTime.Now;
            loan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.LastUpdateDate = DateTime.Now;
            loan.DelFlag = "0";
            return loan;
        }

        /// <summary>
        /// ローンマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="loan">ローンデータ(登録内容)</param>
        /// <returns>ローンマスタモデルクラス</returns>
        private Loan EditLoanForUpdate(Loan loan)
        {
            loan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            loan.LastUpdateDate = DateTime.Now;
            return loan;
        }

    }
}
