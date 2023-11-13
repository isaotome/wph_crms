using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class SalesSummaryDao {
        private CrmsLinqDataContext db;
        public SalesSummaryDao(CrmsLinqDataContext context) {
            db = context;
        }
        /// <summary>
        /// 車両日計を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="slipDate">伝票日付</param>
        /// <returns></returns>
        public List<V_CarSummary> GetCarSummaryList(string departmentCode, DateTime slipDate,string salesOrderStatus) {
            var query =
                from a in db.V_CarSummary
                where a.DepartmentCode.Equals(departmentCode)
                && a.SlipDate.Equals(slipDate)
                && a.SalesOrderStatus.Equals(salesOrderStatus)
                select a;
            return query.ToList();
        }

        /// <summary>
        /// サービス日計を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="slipDate">伝票日付</param>
        /// <returns></returns>
        public List<V_ServiceSummary> GetSummarySummaryList(string departmentCode, DateTime slipDate, string serviceOrderStatus) {
            var query =
                from a in db.V_ServiceSummary
                where a.DepartmentCode.Equals(departmentCode)
                && a.SlipDate.Equals(slipDate)
                && a.ServiceOrderStatus.Equals(serviceOrderStatus)
                select a;
            return query.ToList();
        }
    }


}
