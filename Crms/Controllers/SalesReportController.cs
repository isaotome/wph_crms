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
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SalesReportController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �ꗗ��ʕ\��
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

            //�{���\�Ȏ��Ə��ꗗ���擾����
            foreach (var office in officeList) {

                //���傲�ƂɏW�v���ʂ��Z�b�g����
                foreach (var department in office.Department) {
                    /* �ԗ� */
                    //����
                    department.CarQuoteList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "001");
                    //��
                    department.CarSalesOrderList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "002");
                    //�[��
                    department.CarSalesList = new SalesSummaryDao(db).GetCarSummaryList(department.DepartmentCode, targetDate, "005");

                    /* �T�[�r�X */
                    //����
                    department.ServiceQuoteList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "001");
                    //��
                    department.ServiceSalesOrderList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "002");
                    //�[��
                    department.ServiceSalesList = new SalesSummaryDao(db).GetSummarySummaryList(department.DepartmentCode, targetDate, "005");
                    
                }
            }
            ViewData["SlipDate"] = targetDate;
            return View("SalesReportCriteria",officeList);
        }
    }
}
