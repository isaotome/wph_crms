using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class ServiceDailyReport {
        //日付
        public DateTime ReportDate { get; set; }
        
        //売上高
        public decimal SalesAmount { get; set; }
        
        //粗利額
        public decimal SalesGrossAmount { get; set; }
        
        //入庫台数
        public int ArrivalCount { get; set; }
        
        //出庫台数
        public int SalesCount { get; set; }
        
        //車検入庫台数
        public int InspectionArrivalCount { get; set; }
        
        //車検入庫予約残台数
        public int InspectionRemainReserveCount { get; set; }
        
        //12検入庫台数
        public int AnnualInspectionArrivalCount { get; set; }
        
        //12検入庫予約残台数
        public int AnnualInspectionRemainReserveCount { get; set; }
    }

    public class ServiceDailyReportDao {
        private CrmsLinqDataContext db;
        public ServiceDailyReportDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// サービス日報データリストを取得する
        /// </summary>
        /// <param name="fromDate">期間FROM</param>
        /// <param name="toDate">期間TO</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns></returns>
        public List<ServiceDailyReport> GetServiceDailyReportList(DateTime fromDate,DateTime toDate,string departmentCode) {
            List<ServiceDailyReport> list = new List<ServiceDailyReport>();
            while(fromDate<=toDate){
                ServiceDailyReport report = new ServiceDailyReport();
                report.ReportDate = fromDate;
                decimal salesAmount = GetSalesAmount(fromDate, departmentCode);
                decimal costAmount = GetCostAmount(fromDate, departmentCode);
                report.SalesAmount = salesAmount;
                report.SalesGrossAmount = salesAmount - costAmount;
                report.SalesCount = GetSalesCount(fromDate, departmentCode);
                report.ArrivalCount = GetArrivalCount(fromDate, departmentCode);
                report.InspectionArrivalCount = GetInspectionArrivalCount(fromDate, departmentCode, "10101");
                report.AnnualInspectionArrivalCount = GetInspectionArrivalCount(fromDate, departmentCode, "10202");
                list.Add(report);
                fromDate = fromDate.AddDays(1);
            }
            return list;
        }

        /// <summary>
        /// 指定日の売上高合計を取得する
        /// </summary>
        /// <param name="targetDate">指定日</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>売上高</returns>
        private decimal GetSalesAmount(DateTime targetDate,string departmentCode) {
            var query = from a in db.ServiceSalesHeader
                        where a.SalesDate.Equals(targetDate)
                        && a.DepartmentCode.Equals(departmentCode)
                        && a.DelFlag.Equals("0")
                        && a.ServiceOrderStatus.Equals("006")
                        select a;
            return query.Sum(x => x.GrandTotalAmount) ?? 0m;
        }

        /// <summary>
        /// 指定日の原価合計を取得する
        /// </summary>
        /// <param name="targetDate">指定日</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>原価合計</returns>
        private decimal GetCostAmount(DateTime targetDate, string departmentCode) {
            var query = from a in db.ServiceSalesLine
                        join b in db.ServiceSalesHeader on new { a.SlipNumber, a.RevisionNumber } equals new { b.SlipNumber, b.RevisionNumber } into header
                        from b in header
                        where b.DelFlag.Equals("0")
                        && b.SalesDate.Equals(targetDate)
                        && b.DepartmentCode.Equals(departmentCode)
                        && b.ServiceOrderStatus.Equals("006")
                        select a;
            return query.Sum(x => x.Cost) ?? 0;
        }

        /// <summary>
        /// 指定日の入庫台数を取得する
        /// </summary>
        /// <param name="targetDate">指定日</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>入庫台数</returns>
        private int GetArrivalCount(DateTime targetDate, string departmentCode) {
            var query = from a in db.ServiceSalesHeader
                        where a.DelFlag.Equals("0")
                        && a.DepartmentCode.Equals(departmentCode)
                        && a.ArrivalPlanDate.Equals(targetDate)
                        select a;
            return query.Count();
        }

        /// <summary>
        /// 指定日の出庫台数を取得する
        /// </summary>
        /// <param name="targetDate">指定日</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>出庫台数</returns>
        private int GetSalesCount(DateTime targetDate, string departmentCode) {
            var query = from a in db.ServiceSalesHeader
                        where a.SalesDate.Equals(targetDate)
                                       && a.DepartmentCode.Equals(departmentCode)
                                       && a.DelFlag.Equals("0")
                                       && a.ServiceOrderStatus.Equals("006")
                        select a;
            return query.Count();
        }

        /// <summary>
        /// 指定日の指定作業入庫台数を取得する
        /// </summary>
        /// <param name="targetDate">指定日</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>車検入庫台数</returns>
        private int GetInspectionArrivalCount(DateTime targetDate, string departmentCode,string serviceWorkCode) {
            var query = from a in db.ServiceSalesHeader
                        where a.ArrivalPlanDate.Equals(targetDate)
                        && a.DelFlag.Equals("0")
                        && a.DepartmentCode.Equals(departmentCode)
                        && (
                            from b in db.ServiceSalesLine
                            where b.ServiceWorkCode.Equals(serviceWorkCode)
                            select b.SlipNumber
                            ).Contains(a.SlipNumber)
                        select a;
            return query.Count();
        }
    }
}
