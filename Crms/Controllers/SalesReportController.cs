using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SalesReportController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SalesReportController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 一覧画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria(string id) {
            DateTime targetDate;
            if (string.IsNullOrEmpty(id)) {
                targetDate = DateTime.Today;
            } else {
                targetDate = new DateTime(int.Parse(id.Substring(0, 4)), int.Parse(id.Substring(4, 2)), int.Parse(id.Substring(6, 2)));
            }
            Employee employee = (Employee)Session["Employee"];
            CrmsAuth condition = new CrmsAuth();
            condition.SetAuthCondition(employee);

            List<Office> officeList = new OfficeDao(db).GetListByAuthCondition(condition);

            //閲覧可能な事業所一覧を取得する
            foreach (var office in officeList) {

                //部門ごとに集計結果をセットする
                foreach (var department in office.Department) {
                    /* 車両 */
                    //見積
                    department.CarQuoteList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "001");
                    //受注
                    department.CarSalesOrderList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "002");
                    //納車
                    department.CarSalesList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "005");

                    /* サービス */
                    //見積
                    department.ServiceQuoteList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "001");
                    //受注
                    department.ServiceSalesOrderList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "002");
                    //納車
                    department.ServiceSalesList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "005");
                    
                }
            }
            ViewData["SlipDate"] = targetDate;
            return View("SalesReportCriteria",officeList);
        }
    }
}
