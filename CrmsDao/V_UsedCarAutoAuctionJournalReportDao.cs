using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_UsedCarAutoAuctionJournalReportDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_UsedCarAutoAuctionJournalReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 中古車売上(AA)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_UsedCarAutoAuctionJournalReport> GetListUsedCarJournalAA(JournalExPortsCondition condition, string exportJCode)
        {
            // データの取得
            IOrderedQueryable<V_UsedCarAutoAuctionJournalReport> v_UsedCarAAList =
                    from a in db.V_UsedCarAutoAuctionJournalReport
                    where
                    (a.JournalCode.Equals(exportJCode))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    orderby a.DepartmentCode, a.SalesDate
                    select a;

            return v_UsedCarAAList.ToList<V_UsedCarAutoAuctionJournalReport>();
        }
    }
}
