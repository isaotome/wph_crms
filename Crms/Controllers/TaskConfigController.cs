using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;                      //Add 2014/08/06 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TaskConfigController : Controller
    {
        //Add 2014/08/06 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "タスク設定";     // 画面名
        private static readonly string PROC_NAME = "タスク設定更新"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TaskConfigController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// タスク設定リストを検索する
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria(){
            return View("TaskConfigCriteria",new TaskConfigDao(db).GetAllList(true));
        }

        /// <summary>
        /// タスク設定入力画面
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry() {
            List<TaskConfig> list = new TaskConfigDao(db).GetAllList(true);
            if(list!=null && list.Count>0){
                ViewData["RoleCount"] = list[0].TaskRole.Count;
            }

            //foreach(var ret in list){
                //ret.SecurityLevelList = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetLevelListAll(), ret.SecurityLevelCode , false);
            return View("TaskConfigEntry",list);
        }

        /// <summary>
        /// タスク設定更新処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form) {
            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            List<TaskConfig> taskList = new TaskConfigDao(db).GetAllList(true);
            for (int i = 0; i < taskList.Count; i++) {
                taskList[i].PopUp = form[string.Format("task[{0}].{1}",i,"PopUp")];
                taskList[i].SecurityLevelCode = form[string.Format("task[{0}].{1}",i,"SecurityLevelCode")];
                taskList[i].DelFlag = form[string.Format("task[{0}].{1}", i, "DelFlag")].Contains("true") ? "0" : "1";
                for (int j = 0; j < taskList[i].TaskRole.Count; j++) {
                    taskList[i].TaskRole[j].EnableFlag = form[string.Format("task[{0}].role[{1}].{2}", i, j, "EnableFlag")].Contains("true") ? true : false;
                }
            }
            //Add 2014/08/05 arc amii エラーログ対応 SubmitChangesにtry文を追加
            try
            {
                db.SubmitChanges();
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
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            return View("TaskConfigEntry", taskList);
        }
    }
}
