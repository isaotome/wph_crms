using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;             //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.Linq;        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// セットメニューマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SetMenuController : InheritedController
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "セットメニューマスタ";     // 画面名
        private static readonly string PROC_NAME = "セットメニューマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SetMenuController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// セットメニューマスタ検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// セットメニューマスタ検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>セットメニューリスト画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            ViewData["SetMenuName"] = form["SetMenuName"];
            ViewData["CompanyCode"] = form["CompanyCode"];
            PaginatedList<SetMenu> list = GetSearchResult(form);
            return View("SetMenuCriteria", list);
        }

        /// <summary>
        /// セットメニューマスタ検索ダイアログ表示
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// セットメニューマスタ検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>セットメニューリスト画面</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            ViewData["SetMenuName"] = form["SetMenuName"];
            ViewData["CompanyCode"] = form["CompanyCode"];
            PaginatedList<SetMenu> list = GetSearchResult(form);
            return View("SetMenuCriteriaDialog", list);
        }
        /// <summary>
        /// セットメニュー入力画面表示
        /// </summary>
        /// <param name="id">セットメニューコード</param>
        /// <returns>セットメニュー入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {
            SetMenu setMenu = new SetMenuDao(db).GetByKey(id);
            if (setMenu == null) {
                setMenu = new SetMenu();
            } else {
                ViewData["update"] = "1";
            }

            return View("SetMenuEntry", setMenu);
        }

        /// <summary>
        /// セットメニュー登録・更新処理
        /// </summary>
        /// <param name="setMenu">セットメニューデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>セットメニュー入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SetMenu setMenu,FormCollection form) {
            ValidateSetMenu(setMenu);
            if (!ModelState.IsValid) {
            	//ADD　2014/10/30　ishii　保存ボタン対応　
                ViewData["update"] = form["update"];
                SetDataComponent(form);
                return View("SetMenuEntry", setMenu);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                if (!string.IsNullOrEmpty(form["update"]) && form["update"].Equals("1")) {
                    SetMenu target = new SetMenuDao(db).GetByKey(setMenu.SetMenuCode);
                    UpdateModel<SetMenu>(target);
                } else {
                    setMenu.CreateDate = DateTime.Now;
                    setMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.LastUpdateDate = DateTime.Now;
                    setMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.DelFlag = "0";
                    db.SetMenu.InsertOnSubmit(setMenu);
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
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
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetDataComponent(form);
                            return View("SetMenuEntry", setMenu);
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
                        // 上記以外の例外の場合、エラーログ出力し、エラー画面に遷移する
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
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            //return View("SetMenuEntry", setMenu);
            return Entry(setMenu.SetMenuCode);
        }
        /// <summary>
        /// セットメニュー検索結果取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>セットメニューリスト</returns>
        private PaginatedList<SetMenu> GetSearchResult(FormCollection form) {
            SetMenu condition = new SetMenu();
            condition.SetMenuCode = form["SetMenuCode"];
            condition.SetMenuName = form["SetMenuName"];
            condition.CompanyCode = form["CompanyCode"];

            return new SetMenuDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// セットメニューコードからセットメニュー名を取得する(Ajax用)
        /// </summary>
        /// <param name="code">セットメニューコード</param>
        /// <returns>取得結果(取得できない場合でもNULLではない)</returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SetMenu setMenu = new SetMenuDao(db).GetByKey(code);
                if (setMenu != null) {
                    codeData.Code = setMenu.SetMenuCode;
                    codeData.Name = setMenu.SetMenuName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="setMenu">セットメニューデータ</param>
        private void ValidateSetMenu(SetMenu setMenu) {
            CommonValidate("SetMenuCode", "セットメニューコード", setMenu, true);
            CommonValidate("SetMenuName", "セットメニュー名", setMenu, true);
            CommonValidate("CompanyCode", "会社コード", setMenu, true);
        }

        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetDataComponent(FormCollection form) {
            
        }
    }
}
