using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_ReceiptTransferJournalReportDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ReceiptTransferJournalReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 振込入金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_ReceiptTransferJournalReport> GetListReceiptTransJournal(JournalExPortsCondition condition, string exportJCode)
        {
            // データの取得
            IQueryable<V_ReceiptTransferJournalReport> v_receiptTransList =
                    from a in db.V_ReceiptTransferJournalReport
                    where
                    (a.JournalCode.Equals(exportJCode))
                    && (condition.JournalDateFrom == null || DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                    && (condition.JournalDateTo == null || DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                    orderby a.OfficeCode,a.DepartmentCode,a.JournalDate
                    select a;

            return v_receiptTransList.ToList<V_ReceiptTransferJournalReport>();
        }
    }
}
