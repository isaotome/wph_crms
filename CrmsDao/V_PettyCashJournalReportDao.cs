using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_PettyCashJournalReportDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_PettyCashJournalReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 小口現金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_PettyCashJournalReport> GetListPettyCashJournal(JournalExPortsCondition condition, string exportJCode)
        {
            // データの取得
            IOrderedQueryable<V_PettyCashJournalReport> v_PettyCashList =
                    from a in db.V_PettyCashJournalReport
                    where
                    (a.JournalCode.Equals(exportJCode))
                    && (condition.JournalDateFrom == null || DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                    && (condition.JournalDateTo == null || DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                    orderby a.OfficeCode, a.DrDepartmentCode,a.JournalDate
                    select a;

            return v_PettyCashList.ToList<V_PettyCashJournalReport>();
        }
    }
}
