using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_OfficeSalesDownloadDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_OfficeSalesDownloadDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        #region 検索
        /// <summary>
        /// サービス売上(社内営業)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_OfficeSalesDownload> GetListServiceSalesOffice(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_OfficeSalesDownload> query =
                            from a in db.V_OfficeSalesDownload
                            where
                                 (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                              && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                              && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                            orderby a.DepartmentCode, a.CustomerClaimCode, a.AccountClassCode
                            group a by new
                            {
                                a.DepartmentCode,
                                a.DepartmentName,
                                a.CustomerClaimCode,
                                a.CustomerClaimName,
                                a.AccountClassCode,
                                a.AccountClassName
                            } into g
                            select new V_OfficeSalesDownload
                            {
                                DepartmentCode = g.Key.DepartmentCode,
                                DepartmentName = g.Key.DepartmentName,
                                CustomerClaimCode = g.Key.CustomerClaimCode,
                                CustomerClaimName = g.Key.CustomerClaimName,
                                AccountClassCode = g.Key.AccountClassCode,
                                AccountClassName = g.Key.AccountClassName,
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

            return new PaginatedList<V_OfficeSalesDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }
        #endregion

        #region CSVデータ取得
        /// <summary>
        /// サービス売上(社内営業)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_OfficeSalesDownload> GetListServiceSalesOfficeCsv(JournalExPortsCondition condition)
        {
            IQueryable<V_OfficeSalesDownload> query =
                            from a in db.V_OfficeSalesDownload
                            where
                                 (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                              && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                              && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                            orderby a.DepartmentCode, a.CustomerClaimCode, a.AccountClassCode
                            group a by new
                            {
                                a.DepartmentCode,
                                a.DepartmentName,
                                a.CustomerClaimCode,
                                a.CustomerClaimName,
                                a.AccountClassCode,
                                a.AccountClassName
                            } into g
                            select new V_OfficeSalesDownload
                            {
                                DepartmentCode = g.Key.DepartmentCode,
                                DepartmentName = g.Key.DepartmentName,
                                CustomerClaimCode = g.Key.CustomerClaimCode,
                                CustomerClaimName = g.Key.CustomerClaimName,
                                AccountClassCode = g.Key.AccountClassCode,
                                AccountClassName = g.Key.AccountClassName,
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

            return query.ToList<V_OfficeSalesDownload>();
        }
        #endregion
    }
}
