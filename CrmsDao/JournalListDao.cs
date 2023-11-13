using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class JournalList
    {
        // 入金日
        public string JournalDate { get; set; }

        // 拠点コード
        public string DepartmentCode { get; set; }

        // 拠点名
        public string DepartmentName { get; set; }

        // 顧客コード
        public string CustomerCode { get; set; }

        // 顧客名
        public string CustomerName { get; set; }

        // 請求先コード
        public string CustomerClaimCode { get; set; }

        // 請求先名称
        public string CustomerClaimName { get; set; }

        // 請求区分
        public string BillingName { get; set; }

        // 伝票番号
        public string SlipNumber { get; set; }

        // 伝票ステイタス
        public string OrderStatusName { get; set; }

        // 作業完了日
        public string SalesOrderDate { get; set; }

        // 納車日
        public string SalesDate { get; set; }

        // 金額
        public decimal Amount { get; set; }

        // 口座種別
        public string AccountKind { get; set; }

        // 科目コード
        public string AccountCode { get; set; }

        // 科目名
        public string AccountName { get; set; }

        // 事業所コード
        public string OfficeCode { get; set; }

        // 事業所名
        public string OfficeName { get; set; }

        // 事業所名
        public string Summary { get; set; }
    }
    public class JournalListDao
    {
        private CrmsLinqDataContext db;
        public JournalListDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 入金実績取得
        /// </summary>
        /// <param name="fromDate">期間FROM</param>
        /// <param name="toDate">期間TO</param>
        /// <returns></returns>
        public List<JournalList> GetJournalList(DateTime fromDate, DateTime toDate, String DepartmentCode)
        {
            var query = from J in db.Journal
                        
                        join cA in db.c_AccountType on J.AccountType equals cA.Code
                        join CC in db.CustomerClaim on J.CustomerClaimCode equals CC.CustomerClaimCode into GCC
                        from LCC in GCC.DefaultIfEmpty()
                        join SH in db.ServiceSalesHeader on new { a = J.SlipNumber, b = "0" } equals new { a = SH.SlipNumber, b = SH.DelFlag } into GSH
                        from LSH in GSH.DefaultIfEmpty()
                        join CM in db.Customer on LSH.CustomerCode equals CM.CustomerCode into GCM
                        from LCM in GCM.DefaultIfEmpty()                        
                        join D in db.Department on J.DepartmentCode equals D.DepartmentCode
                        join cS in db.c_ServiceOrderStatus on LSH.ServiceOrderStatus equals cS.Code into GCS
                        from LCS in GCS.DefaultIfEmpty()                        
                        join A in db.Account on J.AccountCode equals A.AccountCode
                        join cCC in db.c_CustomerClaimType on LCC.CustomerClaimType equals cCC.Code
                        join O in db.Office on J.OfficeCode equals O.OfficeCode into GO
                        from LO in GO.DefaultIfEmpty()
                        where (J.JournalDate >= fromDate && J.JournalDate <= toDate)
                            && J.DelFlag.Equals("0")
                            && J.JournalType.Equals("001")
                            && J.SlipNumber != ""
                            && J.DepartmentCode.Contains(DepartmentCode)
                        orderby J.JournalDate, J.DepartmentCode, J.SlipNumber
                        select new
                        {
                            JournalDate = J.JournalDate.ToShortDateString(),
                            DepartmentCode = J.DepartmentCode,
                            DepartmentName = D.DepartmentName,
                            CustomerCode = (LSH.CustomerCode == null ? String.Empty : LSH.CustomerCode),
                            CustomerName = (LCM.CustomerName == null ? String.Empty : LCM.CustomerName),
                            CustomerClaimCode = J.CustomerClaimCode,
                            CustomerClaimName = (LCC.CustomerClaimName == null ? String.Empty : LCC.CustomerClaimName),
                            BillingName = cCC.Name,
                            SlipNumber = J.SlipNumber,
                            OrderStatusName = (LCS.Name == null ? String.Empty : LCS.Name),
                            //SalesOrderDate = (LSH.SalesOrderDate == null ? DaoConst.SQL_DATETIME_MIN : LSH.SalesOrderDate),
                            SalesOrderDate = LSH.SalesOrderDate,
                            //SalesDate = (LSH.SalesDate == null ? String.Empty : LSH.SalesDate.ToString()),
                            SalesDate = LSH.SalesDate,
                            Amount = J.Amount,
                            AccountKind = cA.Name,
                            AccountCode = J.AccountCode,
                            AccountName = A.AccountName,
                            OfficeCode = (LO.OfficeCode == null ? String.Empty : LO.OfficeCode),
                            OfficeName = (LO.OfficeName == null ? String.Empty : LO.OfficeName),
                            Summary = J.Summary
                        };
                                
            List<JournalList> list = new List<JournalList>();
            foreach (var r in query)
            {
                JournalList report = new JournalList();
                report.JournalDate = r.JournalDate;
                report.DepartmentCode = r.DepartmentCode;
                report.DepartmentName = r.DepartmentName;
                report.CustomerCode = r.CustomerCode;
                report.CustomerName = r.CustomerName;
                report.CustomerClaimCode = r.CustomerClaimCode;
                report.CustomerClaimName = r.CustomerClaimName;
                report.BillingName = r.BillingName;
                report.SlipNumber = r.SlipNumber;
                report.OrderStatusName = r.OrderStatusName;
                report.SalesOrderDate = (r.SalesOrderDate == null ? String.Empty : r.SalesOrderDate.Value.ToShortDateString());
                report.SalesDate = (r.SalesDate == null ? String.Empty : r.SalesDate.Value.ToShortDateString());
                report.Amount = r.Amount;
                report.AccountKind = r.AccountKind;
                report.AccountCode = r.AccountCode;
                report.AccountName = r.AccountName;
                report.OfficeCode = r.OfficeCode;
                report.OfficeName = r.OfficeName;
                report.Summary = r.Summary;
                list.Add(report);
            } 
            return list;
        }

    }

}
