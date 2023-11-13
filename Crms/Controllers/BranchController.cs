using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

namespace Crms.Controllers
{

    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BranchController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BranchController() {
            db = new CrmsLinqDataContext();
        }
        /// <summary>
        /// 検索ダイアログ表示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id){
            FormCollection form = new FormCollection();
            form["BankCode"] = id;
            return CriteriaDialog(form);
        }
        /// <summary>
        /// 検索ダイアログ
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["BranchCode"] = form["BranchCode"];
            ViewData["BranchName"] = form["BranchName"];

            Branch condition = new Branch();
            condition.Bank = new Bank();
            condition.Bank.BankCode = form["BankCode"];
            condition.Bank.BankName = form["BankName"];
            condition.BranchCode = form["BranchCode"];
            condition.BranchName = form["BranchName"];
            PaginatedList<Branch> list = new BranchDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return View("BranchCriteriaDialog", list);
        }

        /// <summary>
        /// 支店コードから支店名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">ブランドコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string bankCode,string branchCode) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Branch branch = new BranchDao(db).GetByKey(branchCode, bankCode);
                if (branch != null) {
                    codeData.Code = branch.BranchCode;
                    codeData.Name = branch.BranchName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
    }
}
