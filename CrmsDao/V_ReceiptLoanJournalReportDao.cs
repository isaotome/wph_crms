using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_ReceiptLoanJournalReportDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ReceiptLoanJournalReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// ローン入金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_ReceiptLoanJournalReport> GetListLoanJournal(JournalExPortsCondition condition, string exportJCode)
        {
            // データの取得
            IOrderedQueryable<V_ReceiptLoanJournalReport> v_LoanList =
                    from a in db.V_ReceiptLoanJournalReport
                    where
                     (a.JournalCode.Equals(exportJCode))
                    && (condition.JournalDateFrom == null || DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                    && (condition.JournalDateTo == null || DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                    orderby a.JournalDate
                    select a;

            return v_LoanList.ToList<V_ReceiptLoanJournalReport>();
        }
    }
}
