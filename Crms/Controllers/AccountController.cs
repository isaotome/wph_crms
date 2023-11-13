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

namespace Crms.Controllers {

    /// <summary>
    /// 科目マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class AccountController : Controller {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AccountController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 科目検索ダイアログ表示
        /// </summary>
        /// <returns>科目検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 科目検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>科目検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // 検索条件の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            switch (Request["journalType"]) {
                case "001":
                    form["CreditFlag"] = "1";
                    break;
                case "002":
                    form["DebitFlag"] = "1";
                    break;
                default:
                    break;
            }
            // 検索結果リストの取得
            PaginatedList<Account> list = GetSearchResultList(form);

            // 科目検索画面の表示
            return View("AccountCriteriaDialog", list);
        }

        /// <summary>
        /// 科目コードから科目名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">科目コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Account account = new AccountDao(db).GetByKey(code);
                if (account != null) {
                    codeData.Code = account.AccountCode;
                    codeData.Name = account.AccountName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 科目マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>科目マスタ検索結果リスト</returns>
        private PaginatedList<Account> GetSearchResultList(FormCollection form) {

            AccountDao accountDao = new AccountDao(db);
            Account accountCondition = new Account();
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                accountCondition.DelFlag = form["DelFlag"];
            }
            accountCondition.CreditFlag = form["CreditFlag"];
            accountCondition.DebitFlag = form["DebitFlag"];
            return accountDao.GetListByCondition(accountCondition, int.Parse(form["id"] ?? "0"), 20);
        }
    }
}
