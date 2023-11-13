using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;

namespace Crms.Controllers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AuthFilterAttribute : FilterAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AuthFilterAttribute()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #region IAuthorizationFilter メンバ

        void  IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext)
        {
            //2016/07/01 arc yano #3597 サービス伝票一覧　システムエラー(暫定対応) 認証メソッドが実行される度に新しいdb接続インスタンスを作成
            db = new CrmsLinqDataContext();

            //セッション変数からEmployeeオブジェクトを取得
            Employee emp = filterContext.RequestContext.HttpContext.Session["Employee"] as Employee;
            if (emp == null) return; //RedirectLoginPage(filterContext);

            //チェックするページ
            string controllerName = filterContext.RouteData.Values["controller"].ToString();

            //ユーザーの権限を取得
            ApplicationRole role = new ApplicationRoleDao(db).GetByKey(emp.SecurityRoleCode,controllerName);
            if (role.EnableFlag == false)
            {
                RedirectErrorPage(filterContext);
            }
            filterContext.RequestContext.HttpContext.Session["Employee"] = emp;

            // Add 2014/08/01 arc amii エラーログ対応 ログファイルにユーザ名と部門名を出力する為、sessionに持たせる処理追加
            string departName = "";

            if (emp.Department1 != null)
            {
                departName += emp.Department1.DepartmentName;
            }

            if (emp.AdditionalDepartment1 != null)
            {
                departName += "(" + emp.AdditionalDepartment1.DepartmentName + ")";
            }

            if (emp.AdditionalDepartment2 != null)
            {
                departName += "(" + emp.AdditionalDepartment2.DepartmentName + ")";
            }

            if (emp.AdditionalDepartment3 != null)
            {
                departName += "(" + emp.AdditionalDepartment3.DepartmentName + ")";
            }

            filterContext.RequestContext.HttpContext.Session["DepartmentName"] = departName;
            filterContext.RequestContext.HttpContext.Session["EmployeeName"] = emp.EmployeeName;
            filterContext.RequestContext.HttpContext.Session["ExecSQL"] = "";
        }

        /// <summary>
        /// エラーページにリダイレクトする
        /// </summary>
        /// <param name="filterContext"></param>
        private void RedirectErrorPage(AuthorizationContext filterContext)
        {
            filterContext.HttpContext.Response.Redirect("/Error/Index/" + filterContext.RouteData.Values["controller"]);
        }
        private void RedirectLoginPage(AuthorizationContext filterContext)
        {
            string actionName = filterContext.RouteData.Values["action"].ToString();

            if (actionName.Contains("Dialog") || actionName.Contains("Entry"))
            {
                filterContext.HttpContext.Response.Redirect("/Error/SessionTimeout/1");
            }
            else
            {
                filterContext.HttpContext.Response.Redirect("/Error/SessionTimeout/0");
            }
        }
        #endregion
    }
}
