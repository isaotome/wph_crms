using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// カード入金予定一覧クラス
    /// </summary>
    /// <history>
    /// 2018/01/15 #3830 カード入金予定一覧の検索結果の出力形式追記列依頼 事業所コード、事業所名の追加
    /// </history>
    public class CardReceiptPlanList
    {
        // 入金日
        public string JournalDate { get; set; }

        // 部門コード
        public string OccurredDepartmentCode { get; set; }

        // 部門名
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
        public string CCTypeName { get; set; }

        // 伝票番号
        public string SlipNumber { get; set; }

        // 伝票ステータス
        public string OrderStatusName { get; set; }

        // 受注日
        public string SalesOrderDate { get; set; }

        // 納車日
        public string SalesDate { get; set; }

        // 金額
        public decimal? Amount { get; set; }

        // 講座種別名
        public string ReceiptTypeName { get; set; }

        // 科目コード
        public string AccountCode { get; set; }

        // 科目名
        public string AccountName { get; set; }

        // 事業所コード
        public string OfficeCode { get; set; }      //Add 2018/01/15 #3830

        // 事業所名
        public string OfficeName { get; set; }      //Add 2018/01/15 #3830

        // 摘要
        public string Summary { get; set; }

    }
    public class CardReceiptPlanListDao
    {
        private CrmsLinqDataContext db;
        public CardReceiptPlanListDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// カード入金予定一覧取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/15 #3830 カード入金予定一覧の検索結果の出力形式追記列依頼
        /// </history>
        public List<CardReceiptPlanList> GetCardReceiptPlanList(DateTime fromDate, DateTime toDate, String DepartMentCode)
        {
            var query = from RP in db.ReceiptPlan
                        // 2016.07.08 売掛変更
                        //join cRT in db.c_ReceiptType on RP.ReceiptType equals cRT.Code
                        join CC in db.CustomerClaim on RP.CustomerClaimCode equals CC.CustomerClaimCode into GCC
                        from LCC in GCC.DefaultIfEmpty()
                        join SH in db.ServiceSalesHeader on new { a = RP.SlipNumber, b = "0" } equals new { a = SH.SlipNumber, b = SH.DelFlag } into GSH
                        from LSH in GSH.DefaultIfEmpty()
                        join C in db.Customer on LSH.CustomerCode equals C.CustomerCode into GC
                        from LC in GC.DefaultIfEmpty()
                        join D in db.Department on RP.OccurredDepartmentCode equals D.DepartmentCode
                        join cCC in db.c_ServiceOrderStatus on LSH.ServiceOrderStatus equals cCC.Code into GcCC
                        from LcCC in GcCC.DefaultIfEmpty()
                        join A in db.Account on RP.AccountCode equals A.AccountCode
                        join cCCT in db.c_CustomerClaimType on LCC.CustomerClaimType equals cCCT.Code
                        // 2016.07.08 売掛変更
                        //join J in db.Journal on  equals J.Amount into GJ 
                        //join J in db.Journal on new { a = RP.SlipNumber, b = RP.ReceiptType, c = RP.AccountCode, d = (RP.Amount ?? 0),  e = "0" }
                        //                 equals new { a = J.SlipNumber, b = J.AccountType, c = J.AccountCode, d = J.Amount, e = J.DelFlag } //into GJ
                        //from LJ in GJ.DefaultIfEmpty()
                        join J in db.Journal on RP.ReceiptPlanId.ToString() equals J.CreditReceiptPlanId
                        join cRT in db.c_ReceiptType on J.AccountType equals cRT.Code
                        join OC in db.Office on D.OfficeCode equals OC.OfficeCode       //Add 2018/01/15 arc yano #3830
                        where RP.DelFlag.Equals("0") &&
                              J.AccountType.Equals("003") &&
                              (J.JournalDate >= fromDate && J.JournalDate <= toDate) &&
                              RP.OccurredDepartmentCode.Contains(DepartMentCode)
                        orderby J.JournalDate, RP.DepartmentCode, RP.SlipNumber
                        select new
                        {
                            JournalDate = J.JournalDate,
                            //JournalDate = DateTime.Now,
                            OccurredDepartmentCode = RP.OccurredDepartmentCode,
                            DepartmentName = D.DepartmentName,
                            CustomerCode = (LSH.CustomerCode == null ? string.Empty : LSH.CustomerCode),
                            CustomerName = (LC.CustomerName == null ? string.Empty : LC.CustomerName),
                            CustomerClaimCode = RP.CustomerClaimCode,
                            CustomerClaimName = (LCC.CustomerClaimName == null ? string.Empty : LCC.CustomerClaimName),
                            CCTypeName = cCCT.Name,
                            SlipNumber = (LSH.SlipNumber == null ? string.Empty : LSH.SlipNumber),
                            OrderStatusName = (LcCC.Name == null ? string.Empty : LcCC.Name),
                            SalesOrderDate = LSH.SalesOrderDate,
                            SalesDate = LSH.SalesDate,
                            Amount = RP.Amount,
                            ReceiptTypeName = cRT.Name,
                            AccountCode = RP.AccountCode,
                            AccountName = A.AccountName,
                            OfficeCode = D.OfficeCode,      //Add 2018/01/15 arc yano #3830
                            OfficeName = OC.OfficeName,     //Add 2018/01/15 arc yano #3830
                            Summary = RP.Summary
                        };
            List<CardReceiptPlanList> list = new List<CardReceiptPlanList>();

            foreach (var r in query)
            {
                CardReceiptPlanList report = new CardReceiptPlanList();
                report.JournalDate =r.JournalDate.ToShortDateString();
                report.OccurredDepartmentCode = r.OccurredDepartmentCode;
                report.DepartmentName = r.DepartmentName;
                report.CustomerCode = r.CustomerCode;
                report.CustomerName = r.CustomerName;
                report.CustomerClaimCode = r.CustomerClaimCode;
                report.CustomerClaimName = r.CustomerClaimName;
                report.CCTypeName = r.CCTypeName;
                report.SlipNumber = r.SlipNumber;
                report.OrderStatusName = r.OrderStatusName;
                report.SalesOrderDate = (r.SalesOrderDate == null ? string.Empty : r.SalesOrderDate.Value.ToShortDateString());
                report.SalesDate = (r.SalesDate == null ? string.Empty : r.SalesDate.Value.ToShortDateString());
                report.Amount = r.Amount;
                report.ReceiptTypeName = r.ReceiptTypeName;
                report.AccountCode = r.AccountCode;
                report.AccountName = r.AccountName;
                report.OfficeCode = r.OfficeCode;       //Add 2018/01/15 arc yano #3830
                report.OfficeName = r.OfficeName;       //Add 2018/01/15 arc yano #3830
                report.Summary = r.Summary;
                list.Add(report);
            }

            return list;
        }
    }
}
