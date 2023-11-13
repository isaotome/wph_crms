using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;

namespace Crms.Controllers {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AkakuroAuthFilterAttribute : FilterAttribute, IAuthorizationFilter {
         /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AkakuroAuthFilterAttribute()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #region IAuthorizationFilter メンバ

        public void OnAuthorization(AuthorizationContext filterContext) {
            Employee emp = filterContext.RequestContext.HttpContext.Session["Employee"] as Employee;
            if (emp == null) return; //RedirectLoginPage(filterContext);

            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            if (!CommonUtils.DefaultString(emp.DepartmentCode).Equals("033") 
                && !CommonUtils.DefaultString(emp.DepartmentCode).Equals("042"))
            {
                RedirectErrorPage(filterContext);
            }
        }
        /// <summary>
        /// エラーページにリダイレクトする
        /// </summary>
        /// <param name="filterContext"></param>
        private void RedirectErrorPage(AuthorizationContext filterContext) {
            filterContext.HttpContext.Response.Redirect("/Error/Index/" + filterContext.RouteData.Values["controller"]);
        }

        #endregion
    }
}
