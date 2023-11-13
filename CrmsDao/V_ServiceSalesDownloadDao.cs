using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_ServiceSalesDownloadDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ServiceSalesDownloadDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        #region 検索
        /// <summary>
        /// サービス売上の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_ServiceSalesDownload> GetListServiceSales(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {

            IQueryable<V_ServiceSalesDownload> query =
                        from a in db.V_ServiceSalesDownload
                        where
                            (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                        && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                        group a by new
                        {

                            a.ServiceId,
                            a.WorkType,
                            a.DepartmentCode,
                            a.DepartmentName,
                            a.rate
                        } into g
                        orderby g.Key.DepartmentCode
                        select new V_ServiceSalesDownload
                        {
                            DepartmentCode = g.Key.DepartmentCode,
                            DepartmentName = g.Key.DepartmentName,
                            ServiceId = g.Key.ServiceId,
                            WorkType = g.Key.WorkType,
                            rate = g.Key.rate,
                            ServiceAmount = g.Sum(p => p.ServiceAmount),
                            ServiceTaxAmount = g.Sum(p => p.ServiceTaxAmount),
                            ServiceTotalAmount = g.Sum(p => p.ServiceTotalAmount),
                            PartsAmount = g.Sum(p => p.PartsAmount),
                            PartsTaxAmount = g.Sum(p => p.PartsTaxAmount),
                            PartsTotalAmount = g.Sum(p => p.PartsTotalAmount),
                            TotalAmount = g.Sum(p => p.TotalAmount),
                            TotalTaxAmount = g.Sum(p => p.TotalTaxAmount),
                            TotalTotalAmount = g.Sum(p => p.TotalTotalAmount),
                            ServiceCost = g.Sum(p => p.ServiceCost),
                            PartsCost = g.Sum(p => p.PartsCost),
                            Total = g.Sum(p => p.Total)
                        };


            return new PaginatedList<V_ServiceSalesDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }
        #endregion

        #region CSV取得
        /// <summary>
        /// サービス売上のCSVデータ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<V_ServiceSalesDownload> GetListServiceSalesCsv(JournalExPortsCondition condition)
        {

            IQueryable<V_ServiceSalesDownload> query =
                        from a in db.V_ServiceSalesDownload
                        where
                            (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                        && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                        group a by new
                        {

                            a.ServiceId,
                            a.WorkType,
                            a.DepartmentCode,
                            a.DepartmentName,
                            a.rate
                        } into g
                        orderby g.Key.DepartmentCode
                        select new V_ServiceSalesDownload
                        {
                            DepartmentCode = g.Key.DepartmentCode,
                            DepartmentName = g.Key.DepartmentName,
                            ServiceId = g.Key.ServiceId,
                            WorkType = g.Key.WorkType,
                            rate = g.Key.rate,
                            ServiceAmount = g.Sum(p => p.ServiceAmount),
                            ServiceTaxAmount = g.Sum(p => p.ServiceTaxAmount),
                            ServiceTotalAmount = g.Sum(p => p.ServiceTotalAmount),
                            PartsAmount = g.Sum(p => p.PartsAmount),
                            PartsTaxAmount = g.Sum(p => p.PartsTaxAmount),
                            PartsTotalAmount = g.Sum(p => p.PartsTotalAmount),
                            TotalAmount = g.Sum(p => p.TotalAmount),
                            TotalTaxAmount = g.Sum(p => p.TotalTaxAmount),
                            TotalTotalAmount = g.Sum(p => p.TotalTotalAmount),
                            ServiceCost = g.Sum(p => p.ServiceCost),
                            PartsCost = g.Sum(p => p.PartsCost),
                            Total = g.Sum(p => p.Total)
                        };


            return query.ToList<V_ServiceSalesDownload>();
        }
        #endregion
    }
}
