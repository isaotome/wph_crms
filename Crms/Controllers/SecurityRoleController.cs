using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.Linq;                 //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.SqlClient;            //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// セキュリティロール
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SecurityRoleController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "セキュリティロール";         // 画面名
        private static readonly string PROC_NAME = "セキュリティロール追加更新"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SecurityRoleController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// セキュリティロール入力画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            SecurityRole role = new SecurityRoleDao(db).GetByKey(id);
            if (role == null) {
                role = new SecurityRole();
                role.DelFlag = "0";
            } else {
                ViewData["update"] = "1";
            }
            SetDataComponent(role);
            return View("SecurityRoleEntry",role);
        }

        /// <summary>
        /// セキュリティロール追加更新
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SecurityRole role,FormCollection form)
        {
            if (string.IsNullOrEmpty(role.SecurityRoleCode)) {
                ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0001", "セキュリティロールコード"));
            }
            if (string.IsNullOrEmpty(role.SecurityRoleName)) {
                ModelState.AddModelError("SecurityRoleName", MessageUtils.GetMessage("E0001", "セキュリティロール名"));
            }
            if (!ModelState.IsValid) {
                SetDataComponent(role);
                return View("SecurityRoleEntry", role);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                if (string.IsNullOrEmpty(form["update"])) {
                    role.CreateDate = DateTime.Now;
                    role.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    role.LastUpdateDate = DateTime.Now;
                    role.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    db.SecurityRole.InsertOnSubmit(role);

                    //アプリケーションロール追加
                    SetApplicationRole(role);
                    //タスクロール追加
                    SetTaskRole(role);
                } else {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(role.DelFlag).Equals("1"))
                    {
                        //アプリケーションロール削除
                        DeleteApplicationRole(role);
                        //タスクロール削除
                        DeleteTaskRole(role);

                        //セキュリティロール削除
                        SecurityRole delRole = new SecurityRoleDao(db).GetByKey(role.SecurityRoleCode);
                        db.SecurityRole.DeleteOnSubmit(delRole);
                    } else {
                        //更新
                        SecurityRole target = new SecurityRoleDao(db).GetByKey(role.SecurityRoleCode);
                        UpdateModel(target);
                    }
                }

                // Add 2014/08/06 arc amii エラーログ対応 新たなcatch句(ChangeConflictException, SqlException)とループ文を追加
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
                        // 更新時、クライアントの読み取り以降にDB値が更新された時、ローカルの値をDB値で上書きする
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        // リトライ回数を超えた場合、エラーとする
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
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // 一意制約エラーの場合、メッセージを設定し、返す
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0010", new string[] { "セキュリティロールコード", "保存" }));
                            SetDataComponent(role);
                            return View("SecurityRoleEntry", role);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // Mod 2014/08/06 arc amii エラーログ対応 「throw e」からエラーログ出力し、エラー画面に遷移する処理に変更
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                } 
            }
            SetDataComponent(role);
            ViewData["close"] = "1";
            return View("SecurityRoleEntry",role);
        }

        /// <summary>
        /// データ付き画面コンポーネントの作成
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        private void SetDataComponent(SecurityRole role) {
            CodeDao dao = new CodeDao(db);
            ViewData["SecurityLevelList"] = CodeUtils.GetSelectListByModel(dao.GetSecurityLevelAll(false), "", false);
        }
        
        /// <summary>
        /// セキュリティロールコードが存在するかチェックする（Ajax用）
        /// </summary>
        /// <param name="code">セキュリティコード</param>
        /// <returns></returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SecurityRole role = new SecurityRoleDao(db).GetByKey(code);
                if (role != null) {
                    codeData.Code = role.SecurityRoleCode;
                    codeData.Name = role.SecurityRoleName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// アプリケーションロールを一括追加する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        private void SetApplicationRole(SecurityRole role) {
            foreach (var a in new ApplicationDao(db).GetListAll()) {
                ApplicationRole app = new ApplicationRole();
                app.SecurityRoleCode = role.SecurityRoleCode;
                app.ApplicationCode = a.ApplicationCode;
                app.EnableFlag = false;
                app.CreateDate = DateTime.Now;
                app.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                app.LastUpdateDate = DateTime.Now;
                app.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                app.DelFlag = "0";
                db.ApplicationRole.InsertOnSubmit(app);
            }
        }

        /// <summary>
        /// タスクロールを一括追加する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        private void SetTaskRole(SecurityRole role) {
            foreach (var t in new TaskConfigDao(db).GetAllList(true)) {
                TaskRole task = new TaskRole();
                task.EnableFlag = false;
                task.SecurityRoleCode = role.SecurityRoleCode;
                task.TaskConfigId = t.TaskConfigId;
                db.TaskRole.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// アプリケーションロールを一括削除する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        private void DeleteApplicationRole(SecurityRole role) {
            List<ApplicationRole> delList = new ApplicationRoleDao(db).GetListBySecurityRole(role);
            db.ApplicationRole.DeleteAllOnSubmit<ApplicationRole>(delList);
        }

        /// <summary>
        /// タスクロールを一括削除する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        private void DeleteTaskRole(SecurityRole role) {
            List<TaskRole> delList = new TaskRoleDao(db).GetListBySecurityRole(role);
            db.TaskRole.DeleteAllOnSubmit<TaskRole>(delList);
        }
    }
}
