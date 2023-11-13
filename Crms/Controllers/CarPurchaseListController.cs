using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

namespace Crms.Controllers {
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarPurchaseListController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarPurchaseListController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �ԗ��d���ꗗ
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            form["PurchasePlanDateFrom"] = string.Format("{0:yyyy/MM/01}", DateTime.Today);
            form["PurchasePlanDateTo"] = string.Format("{0:yyyy/MM/dd}", new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1));
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            return Criteria(form);
        }

        /// <summary>
        /// �ԗ��d������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["DefaultPurchasePlanDateFrom"] = string.Format("{0:yyyy/MM/01}", DateTime.Today);
            ViewData["DefaultPurchasePlanDateTo"] = string.Format("{0:yyyy/MM/dd}", new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1));
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode).DepartmentName;

            ViewData["PurchasePlanDateFrom"] = form["PurchasePlanDateFrom"];
            ViewData["PurchasePlanDateTo"] = form["PurchasePlanDateTo"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department!=null ? department.DepartmentName : "";
            }
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetPurchaseStatusAll(false), form["PurchaseStatus"], true);
            ViewData["Vin"] = form["Vin"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];

            V_CarPurchaseList condition = new V_CarPurchaseList();
            condition.PurchasePlanDateFrom = CommonUtils.StrToDateTime(form["PurchasePlanDateFrom"]);
            condition.PurchasePlanDateTo = CommonUtils.StrToDateTime(form["PurchasePlanDateTo"]);
            condition.PurchaseStatus = form["PurchaseStatus"];
            condition.Vin = form["Vin"];
            condition.MakerName = form["MakerName"];
            condition.CarName = form["CarName"];
            condition.DepartmentCode = form["DepartmentCode"];

            CarPurchaseDao dao = new CarPurchaseDao(db);
            PaginatedList<V_CarPurchaseList> list = dao.GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return View("CarPurchaseList", list);
        }
    }
}
