using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BankController : InheritedController
    {

        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "銀行マスタ";     // 画面名
        private static readonly string PROC_NAME = "銀行マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BankController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            form["DelFlag"] = "0";
            return Criteria(form);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["DelFlag"] = form["DelFlag"];
            PaginatedList<Bank> list = GetSearchResult(form);
            return View("BankCriteria", list);
        }

        /// <summary>
        /// 検索ダイアログ表示
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 検索ダイアログ処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["id"] = form["id"];
            PaginatedList<Bank> list = GetSearchResult(form);
            return View("BankCriteriaDialog", list);
        }

        /// <summary>
        /// 検索結果を取得する
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<Bank> GetSearchResult(FormCollection form) {
            Bank condition = new Bank();
            condition.BankCode = form["BankCode"];
            condition.BankName = form["BankName"];
            condition.DelFlag = form["DelFlag"];
            return new BankDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 銀行マスタ追加・更新画面表示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Entry(string id) {
            Bank model;
            if (string.IsNullOrEmpty(id)) {
                model = new Bank();
                ViewData["update"] = "0";
            } else {
                //ADD 2014/10/29 ishii 保存ボタン対応
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                db = new CrmsLinqDataContext();
                model = new BankDao(db).GetByKey(id, true);
                ViewData["update"] = "1";
            }
            return View("BankEntry", model);
        }

        /// <summary>
        /// 銀行マスタ追加・更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Bank model,EntitySet<Branch> branches, FormCollection form) {
            // ADD 2014/06/09 arc uchida チェック処理の実行
            ValidateBank(model);
            if (!ModelState.IsValid) {
                //ADD 2014/10/30 ishii 保存ボタン対応
                model.Branch = branches;
                SetDataComponent(form);
                return View("BankEntry", model);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                // データ更新処理
                if (form["update"].Equals("1")) {
                    //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                    Bank target = new BankDao(db).GetByKey(model.BankCode, true);

                    //ADD arc uchida vs2012対応
                    if (branches != null)
                    {
                        foreach (var branch1 in branches)
                        {
                            int flag = 0;
                            foreach (var branch2 in branches)
                            {
                                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                                if (CommonUtils.DefaultString(branch1.BranchCode).Equals(CommonUtils.DefaultString(branch2.BranchCode)))
                                {
                                    flag += 1;
                                }
                            }
                            if (flag >= 2)
                            {
                                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0010", new string[] { branch1.BranchCode, "変更" }));
                                break;
                            }
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        // 支店がなくなっているものは論理削除する
                        // 元のデータベース
                        foreach (var branch in target.Branch)
                        {

                            //ADD arc uchida vs2012対応
                            if (branches == null)
                            {
                                branch.DelFlag = "1";
                            }
                            else
                            {
                                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                                // 画面の入力リストに存在するか
                                Branch updateBranch = (from a in branches
                                                       where CommonUtils.DefaultString(a.BranchCode).Equals(CommonUtils.DefaultString(branch.BranchCode))
                                                       select a).FirstOrDefault();

                                // 存在しなかったら削除されることになる
                                if (updateBranch == null)
                                {
                                    branch.DelFlag = "1";

                                    // 存在するけど、元のデータが論理削除されていたら復活させる
                                }
                                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                                else if (CommonUtils.DefaultString(branch.DelFlag).Equals("1"))
                                {
                                    // ADD arc uchida vs2012対応
                                    if (branch.BranchCode != null && branch.BranchCode != "")
                                    {
                                        branch.BranchName = updateBranch.BranchName;
                                        branch.DelFlag = "0";
                                    }
                                }
                            }
                            EditForBranchUpdate(branch);

                        }
                        //ADD arc uchida vs2012対応
                        if (branches != null)
                        {
                            foreach (var branch in branches)
                            {
                                // ADD arc uchida vs2012対応
                                if (branch.BranchCode != "" && branch.BranchCode != null)
                                {
                                    Branch branchTarget = new BranchDao(db).GetByKey(branch.BranchCode, branch.BankCode);
                                    if (branchTarget != null)
                                    {
                                        branchTarget.BranchName = branch.BranchName;
                                        EditForBranchUpdate(branchTarget);
                                    }
                                    else
                                    {
                                        EditForBranchCreate(branch);
                                        db.Branch.InsertOnSubmit(branch);
                                    }
                                }
                            }
                        }
                        UpdateModel(target);
                    }
                    // ADD arc uchida vs2012対応
                    else
                    {
                        model.Branch = branches;
                        SetDataComponent(form);
                        return View("BankEntry", model);
                    }
                } else {
                    model.CreateDate = DateTime.Now;
                    model.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    model.LastUpdateDate = DateTime.Now;
                    model.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    model.DelFlag = "0";
                    db.Bank.InsertOnSubmit(model);

                    if (branches != null) {
                        foreach (var branch in branches) {
                           db.Branch.InsertOnSubmit(branch);
                        }
                    }
                }
                // Mod 2014/08/04 arc amii エラーログ対応 ログ出力する為、try catch文を追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
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
                    catch (SqlException se)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("BankCode", MessageUtils.GetMessage("E0011", "保存"));
                            SetDataComponent(form);
                            return View("BankEntry", model);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");

                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                
            }
           
            //MOD 2014/10/29 ishii 保存ボタン対応
            ModelState.Clear();
            SetDataComponent(form);
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            //return View("BankEntry", model);
            return Entry(model.BankCode);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidateBank(Bank model) {
            CommonValidate("BankCode", "銀行コード", model, true);
        }

        /// <summary>
        /// 画面コンポーネント
        /// </summary>
        /// <param name="form"></param>
        private void SetDataComponent(FormCollection form) {
            ViewData["update"] = form["update"];
        }

        /// <summary>
        /// CreateDate更新
        /// </summary>
        /// <param name="branch"></param>
        private void EditForBranchCreate(Branch branch) {
            branch.CreateDate = DateTime.Now;
            branch.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            branch.DelFlag = "0";
            EditForBranchUpdate(branch);
        }
        /// <summary>
        /// LastUpdate更新
        /// </summary>
        /// <param name="branch"></param>
        private void EditForBranchUpdate(Branch branch) {
            branch.LastUpdateDate = DateTime.Now;
            branch.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        }
         
        /// <summary>
        /// 支店リストを1行追加する
        /// </summary>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult AddBranch(Bank model,EntitySet<Branch> branches, FormCollection form) {
            
            if (branches == null) {
                branches = new EntitySet<Branch>();
            }
            Branch branch = new Branch() { BankCode = model.BankCode, DelFlag = "0" };
            branches.Add(branch);

            ModelState.Clear();

            model.Branch = branches;
            SetDataComponent(form);
            return View("BankEntry", model);
        }

        /// <summary>
        /// 支店リストを1行削除する
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult DelBranch(int id,Bank model, EntitySet<Branch> branches, FormCollection form) {
            branches.RemoveAt(id);
            ModelState.Clear();
            model.Branch = branches;
            SetDataComponent(form); 
            return View("BankEntry", model);
        }

        /// <summary>
        /// 銀行コードから銀行名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">ブランドコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Bank bank = new BankDao(db).GetByKey(code);
                if (bank != null) {
                    codeData.Code = bank.BankCode;
                    codeData.Name = bank.BankName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
    }
}
