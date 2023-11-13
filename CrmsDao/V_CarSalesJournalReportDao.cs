using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_CarSalesJournalReportDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarSalesJournalReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 中古車売上(AA除く)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarSalesJournalReport> GetListUsedCarJournalNAA(JournalExPortsCondition condition, string exportJCode)
        {
            // データの取得
            IOrderedQueryable<V_CarSalesJournalReport> v_UsedCarSalesList =
                    from a in db.V_CarSalesJournalReport
                    where
                    (a.NewUsedType.Equals("U"))
                    && (a.JournalCode.Equals(exportJCode))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.SalesDate
                    select a;

            return v_UsedCarSalesList.ToList<V_CarSalesJournalReport>();
        }

        /// <summary>
        /// 新車売上の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarSalesJournalReport> GetListByNewCarJournal(JournalExPortsCondition condition, string exportJCode)
        {

            // データの取得
            IOrderedQueryable<V_CarSalesJournalReport> v_NewCarSalesList =
                    from a in db.V_CarSalesJournalReport
                    where
                    (a.NewUsedType.Equals("N"))
                    && (a.JournalCode.Equals(exportJCode))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.SalesDate
                    select a;

            return v_NewCarSalesList.ToList<V_CarSalesJournalReport>();
        }
    }
}
