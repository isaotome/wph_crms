using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_ReceiptTransDownloadDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ReceiptTransDownloadDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        #region 検索
        
        /// <summary>
        /// 振込入金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_ReceiptTransDownload> GetListTransfer(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("002")
                        && !a.CustomerClaimType.Equals("003")
                        && !a.CustomerClaimType.Equals("004")
                        && !a.CustomerClaimType.Equals("005")
                        && !a.CustomerClaimType.Equals("006")
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return new PaginatedList<V_ReceiptTransDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// 小口現金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_ReceiptTransDownload> GetListPettyCash(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("001")
                        && a.Amount != 0
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return new PaginatedList<V_ReceiptTransDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// ローン入金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_ReceiptTransDownload> GetListLoan(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("004")
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return new PaginatedList<V_ReceiptTransDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }

        #endregion

        #region CSVデータ取得
        /// <summary>
        /// 振込入金のCSVデータ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_ReceiptTransDownload> GetListTransferCsv(JournalExPortsCondition condition)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("002")
                        && !a.CustomerClaimType.Equals("003")
                        && !a.CustomerClaimType.Equals("004")
                        && !a.CustomerClaimType.Equals("005")
                        && !a.CustomerClaimType.Equals("006")
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return query.ToList<V_ReceiptTransDownload>();
        }

        /// <summary>
        /// 小口現金のCSVデータ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_ReceiptTransDownload> GetListPettyCashCsv(JournalExPortsCondition condition)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("001")
                        && a.Amount != 0
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return query.ToList<V_ReceiptTransDownload>();
        }

        /// <summary>
        /// ローン入金の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_ReceiptTransDownload> GetListLoanCsv(JournalExPortsCondition condition)
        {
            IQueryable<V_ReceiptTransDownload> query =
                        from a in db.V_ReceiptTransDownload
                        where
                            a.AccountType.Equals("004")
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateFrom) >= 0)
                        && (DateTime.Compare(a.JournalDate, condition.JournalDateTo) <= 0)
                        && (string.IsNullOrEmpty(condition.OfficeCode) || a.OfficeCode.Equals(condition.OfficeCode))
                        && (string.IsNullOrEmpty(condition.DivisionType) || a.OfficeId.Equals(condition.DivisionType))
                        orderby a.OfficeCode, a.DepartmentCode, a.JournalDate
                        select a;

            return query.ToList<V_ReceiptTransDownload>();
        }

        #endregion
    }
}
