using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;                      //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.SqlClient;            //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.Linq;                 //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TaskController : Controller
    {
        //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "タスク";   // 画面名
        private static readonly string PROC_NAME = "タスク実行"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TaskController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <returns>タスク検索画面</returns>
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// タスク検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            List<SelectListItem> conf = new List<SelectListItem>();
            List<TaskConfig> taskConfigList = new TaskConfigDao(db).GetAllList(false);
            conf.Add(new SelectListItem{Text="",Value=""});
            foreach (var a in taskConfigList) {
                conf.Add(new SelectListItem { Text = a.TaskName, Value = a.TaskConfigId, Selected = a.TaskConfigId.Equals(form["TaskConfigId"]) });
            }
            ViewData["TaskConfigList"] = conf;

            Task taskCondition = new Task();
            if (!string.IsNullOrEmpty(form["TaskStatus"]) && form["TaskStatus"].Contains("true")) {
                taskCondition.TaskStatus = true;
            } else {
                taskCondition.TaskStatus = false;
            }
            ViewData["TaskStatus"] = taskCondition.TaskStatus;
            taskCondition.TaskConfigId = form["TaskConfigId"];
            List<Task> list = ((new TaskDao(db)).GetListByEmployeeCode(taskCondition,((Employee)Session["Employee"]).EmployeeCode, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            foreach (var item in list) {
                if (!string.IsNullOrEmpty(item.SlipNumber)) {
                    CarSalesHeader car = new CarSalesOrderDao(db).GetBySlipNumber(item.SlipNumber);
                    if (car != null) {
                        try {
                            item.CustomerName = car.Customer.CustomerName;
                        } catch { }
                    } else {
                        ServiceSalesHeader service = new ServiceSalesOrderDao(db).GetBySlipNumber(item.SlipNumber);
                        try {
                            item.CustomerName = service.Customer.CustomerName;
                        } catch { }
                    }
                }
            }
            return View("TaskCriteria", list);
        }

        /// <summary>
        /// タスク実行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            Task t = ((new TaskDao(db)).GetByKey(new Guid(id)));
            return View("TaskEntry",t);
        }

        /// <summary>
        /// タスク実行時の処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>タスク表示画面</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Task task,FormCollection form) {
            if (form["TaskType"].Equals("003")) {
                if (string.IsNullOrEmpty(form["ActionResult"])) {
                    ModelState.AddModelError("", "処理内容を入力して下さい");
                    
                    return View("TaskEntry",new TaskDao(db).GetByKey(new Guid(form["TaskId"])));
                }
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            Task t = new TaskDao(db).GetByKey(new Guid(form["TaskId"]));
            t.ActionResult = form["ActionResult"];
            t.TaskCompleteDate = DateTime.Now;
            t.LastUpdateDate = DateTime.Now;
            t.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為のtrycatch文追加
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException cfe)
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
                        OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
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
            
            ViewData["close"] = "1";
            return View("TaskEntry", t);
        }
        
        /// <summary>
        /// タスク追加ポップアップ
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id) {
            Task task;
            if (string.IsNullOrEmpty(id)) {
                task = new Task();
            } else {
                task = new TaskDao(db).GetByKey(new Guid(id));
            }
            return View("TaskDialog",task);
        }

    }
}
