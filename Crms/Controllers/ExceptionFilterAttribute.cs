using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using CrmsDao;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ExceptionFilterAttribute : FilterAttribute, IExceptionFilter {

        #region IExceptionFilter メンバー
        public void OnException(ExceptionContext filterContext) {

            if (filterContext == null)
            {
                throw new ArgumentNullException("No ExceptionContext");
            }
            var db = new CrmsLinqDataContext();

            // ルート・パラメータを取得
            var route = filterContext.RouteData;
            
            /*
             Add 2014/10/14 arc amii エラーメッセージ(IE8)対応 #3091 「HTTP ヘッダーが～」のメッセージはエラーログに出力しない条件文を追加。
                                         IE8が使用されなくなった時、この条件文は不要となる。
             */
            // エラーログに出さないメッセージを設定
            string excludeMsg = "HTTP ヘッダーが送信された後にサーバーでコンテンツ タイプを設定できません。";

            // エラーログに出さないメッセージと一致しない場合のみ、エラーログを出力する
            if (excludeMsg.Equals(CommonUtils.DefaultString(filterContext.Exception.Message)) == false)
            {
                try
                {
                    ErrorLog err = new ErrorLog();
                    err.Uri = filterContext.HttpContext.Request.RawUrl;
                    err.Controller = route.Values["controller"].ToString();
                    err.Action = route.Values["action"].ToString();
                    err.Stack = filterContext.Exception.StackTrace;
                    err.CreateDate = DateTime.Now;
                    err.CreateEmployeeCode = ((Employee)filterContext.HttpContext.Session["Employee"]).EmployeeCode;
                    err.Message = filterContext.Exception.Message;
                    //err.KeyData = filterContext.HttpContext.Session["KeyData"] != null ? filterContext.HttpContext.Session["KeyData"].ToString() : "";
                    err.KeyData = filterContext.HttpContext.Request.Params.ToString();
                    db.ErrorLog.InsertOnSubmit(err);
                    db.SubmitChanges();
                }
                catch { }

                // Add 2014/08/26 arc yano IPO対応 エラー発生時はグローバル変数(車両管理用)フラグをリセットする。
                MvcApplication.OperateFlag = 0;
                MvcApplication.OperateUser = "";

                // Add 2014/09/03 arc yano IPO対応 エラー発生時はグローバル変数(月次締め処理状況画面用)フラグをリセットする。
                MvcApplication.CMOperateFlag = 0;
                MvcApplication.CMOperateUser = "";

                // Add 2014/08/04 arc amii エラーログ対応 Daoでエラーになった場合、ここでログ出力するよう処理を追加
                filterContext.HttpContext.Session["ExecSQL"] = OutputLogData.sqlText;
                if (CommonUtils.DefaultString(OutputLogData.exLog).Equals(""))
                {
                    // Insert or Update時以外で例外が発生した場合
                    OutputLogger.NLogFatalAttribute(filterContext.Exception, OutputLogData.procName, filterContext.HttpContext.Request.RawUrl);
                }
                else
                {

                    // Insert or Update時で例外が発生した場合
                    OutputLogger.NLogFatal(OutputLogData.exLog, OutputLogData.procName, "", "");
                }
            }

            //Add 2015/01/16 arc nakayama システムエラーメッセージに詳細を乗せる
            ViewDataDictionary msgView = new ViewDataDictionary();
            msgView["ExceptionMessage"] = filterContext.Exception.Message;

            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult()

            {
                //ViewData["ExceptionMessage"] = filterContext.Exception.Message;
                ViewName = "Error",
                ViewData = msgView,
            };
        }
        #endregion
    }
}