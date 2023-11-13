using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加		
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MenuControlController : Controller
    {
        private CrmsLinqDataContext db;
        public MenuControlController()
        {
            db = new CrmsLinqDataContext();
        }

        public ActionResult Criteria()
        {
            MenuControlDao dao = new MenuControlDao(db);

            List<MenuControl> list = dao.GetListAll();

            return View("MenuControlCriteria",list);
        }

        public ActionResult Entry(string id)
        {

            MenuControl menu;
            if (string.IsNullOrEmpty(id))
            {
                menu = new MenuControl();
            }
            else
            {
                menu = new MenuControlDao(db).GetByKey(id);
            }
            return View("MenuControlEntry",menu);
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(MenuControl menu)
        {
            //menu.CreateDate = DateTime.Now;
            //menu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //db.MenuControl.InsertOnSubmit(menu);
            
            //List<ApplicationRole> list = new List<ApplicationRole>();
            //foreach (var a in new SecurityRoleDao(db).GetListAll())
            //{
            //    ApplicationRole role = new ApplicationRole();
            //    role.SecurityRoleCode = a.SecurityRoleCode;
            //    role.ApplicationCode = menu.MenuControlCode;
            //    role.EnableFlag = false;
            //    role.CreateDate = DateTime.Now;
            //    role.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //    role.LastUpdateDate = DateTime.Now;
            //    role.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //    role.DelFlag = "0";
            //    list.Add(role);
            //}

            //db.ApplicationRole.InsertAllOnSubmit(list);
            //db.SubmitChanges();
            return View("MenuControlEntry",menu);
        }
    }
}
