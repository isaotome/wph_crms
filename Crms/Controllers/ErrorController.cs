using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Crms.Controllers
{
    [OutputCache(Duration=0,VaryByParam="null")]
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Index(string id)
        {
            ViewData["page"] = id;
            return View("ErrorPage");
        }
        public ActionResult Generic()
        {
            return View("GenericErrorPage");
        }
        public ActionResult SessionTimeout(string id)
        {
            ViewData["closeFlag"] = id;
            return View("SessionTimeout");
        }
        public ActionResult AuthenticationError() {
            return View("AuthenticationError");
        }
    }
}
