using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Crms
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",                                              // Route name
                "{controller}/{action}/{id}",                           // URL with parameters
                new { controller = "Home", action = "Start", id = "" }  // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
        }

        //---------------------------------------
        //車両管理画面用
        //---------------------------------------
        //Add 2014/08/19 arc yano IPO対応　グローバル変数追加
        public static int OperateFlag = 0;           //操作中フラグ
        public static string OperateUser = "";       //操作者


        //---------------------------------------
        //月次締め処理状況画面用
        //---------------------------------------
        //Add 2014/09/03 arc yano IPO対応
        public static int CMOperateFlag = 0;           //操作中フラグ
        public static string CMOperateUser = "";       //操作者
    }
}