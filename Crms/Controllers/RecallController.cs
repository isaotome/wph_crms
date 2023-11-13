using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Crms.Controllers
{
    public class RecallController : Controller
    {
        //
        // GET: /Recall/

        public ActionResult Criteria()
        {
            return View("RecallCriteria");
        }

        public ActionResult Entry()
        {
            return View("RecallEntry");
        }

    }
}
