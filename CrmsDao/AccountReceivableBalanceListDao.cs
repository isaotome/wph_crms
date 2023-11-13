using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class AccountReceivableBalanceList
    {
        //伝票タイプ
        public string SlipType { get; set; }

        //伝票番号
        public string SlipNumber { get; set; }

        //納車日
        public string SalesDate { get; set; }

        //拠点コード
        public string DepartmentCode { get; set; }

        //拠点名
        public string DepartmentName { get; set; }

        //顧客コード
        public string Customercode { get; set; }

        //顧客名
        public string CustomerName { get; set; }

        //請求種別コード
        public string CCTypeCode { get; set; }

        //請求種別名
        public string CCTypeName { get; set; }

        //請求先コード
        public string CustomerClaimCode { get; set; }

        //請求先名
        public string CustomerClaimName { get; set; }

        //金額
        public decimal? Amount { get; set; }

        //後金
        public decimal? AtoAmount { get; set; }

        //前金
        public decimal? MaeAmount { get; set; }

        //残高
        public decimal? BalanceAmount { get; set; }
    }

    public class AccountReceivableBalanceListDao
    {
        private CrmsLinqDataContext db;
        public AccountReceivableBalanceListDao(CrmsLinqDataContext context)
        {
            db = context;
        }
        public List<AccountReceivableBalanceList> GetAccountReceivableBalanceListDao(DateTime? TargetDate, DateTime fromDate, DateTime toDate, String DepartMentCode)
        {
            int lastday = DateTime.DaysInMonth(TargetDate.Value.Year, TargetDate.Value.Month);
            DateTime LastTimeDate = new DateTime(TargetDate.Value.Year, TargetDate.Value.Month, lastday, 23, 59, 59);
            var slip_union =    (
                                from CSH in db.CarSalesHeader
                                where CSH.DelFlag.Equals("0")
                                select new {
                                    SlipNumber = CSH.SlipNumber,
                                    SalesDate = CSH.SalesDate,
                                    SalesOrderDate = CSH.SalesOrderDate,
                                    DepartmentCode = CSH.DepartmentCode,
                                    CustomerCode = CSH.CustomerCode
                                }
                            ).Concat
                            (
                                from SSH in db.ServiceSalesHeader
                                where SSH.DelFlag.Equals("0")
                                select new {
                                    SlipNumber =SSH.SlipNumber,
                                    SalesDate = SSH.SalesDate,
                                    SalesOrderDate = SSH.SalesOrderDate,
                                    DepartmentCode = SSH.DepartmentCode,
                                    CustomerCode = SSH.CustomerCode
                                }
                            );
            var query = from RD in db.ReceivableDetail
                        join SL in slip_union on RD.SlipNumber equals SL.SlipNumber
                        join C in db.Customer on SL.CustomerCode equals C.CustomerCode
                        join CC in db.CustomerClaim on RD.CustomerClaimCode equals CC.CustomerClaimCode
                        join c_CCT in db.c_CustomerClaimType on CC.CustomerClaimType equals c_CCT.Code
                        join D in db.Department on RD.DepartmentCode equals D.DepartmentCode
                        where (RD.InventoryMonth >= TargetDate && RD.InventoryMonth <= LastTimeDate) &&
                              (RD.SalesDate >= fromDate && RD.SalesDate <= toDate) &&
                              RD.DepartmentCode.Contains(DepartMentCode)
                        orderby RD.DepartmentCode, RD.SalesDate, RD.SlipType, RD.SlipNumber
                        select new
                        {
                            SlipType = RD.SlipType,
                            SlipNumber = RD.SlipNumber,
                            SalesDate = RD.SalesDate,
                            DepartmentCode = RD.DepartmentCode,
                            DepartmentName = D.DepartmentName,
                            CustomerCode = C.CustomerCode,
                            CustomerName = C.CustomerName,
                            CCTypeCode = c_CCT.Code,
                            CCTypeName = c_CCT.Name,
                            CustomerClaimCode = CC.CustomerClaimCode,
                            CustomerClaimName = CC.CustomerClaimName,
                            Amount = RD.Amount,
                            AtoAmount = RD.AtoAmount,
                            MaeAmount = RD.MaeAmount,
                            BalanceAmount = RD.BalanceAmount
                        };
            List<AccountReceivableBalanceList> list = new List<AccountReceivableBalanceList>();
            foreach (var r in query)
            {
                AccountReceivableBalanceList report = new AccountReceivableBalanceList();
                report.SlipType = GetSlipType(r.SlipType);
                report.SlipNumber = r.SlipNumber;
                report.SalesDate = (r.SalesDate == null ? String.Empty : r.SalesDate.Value.ToShortDateString());
                report.DepartmentCode = r.DepartmentCode;
                report.DepartmentName = r.DepartmentName;
                report.Customercode = r.CustomerCode;
                report.CustomerName = r.CustomerName;
                report.CCTypeCode = r.CCTypeCode;
                report.CCTypeName = r.CCTypeName;
                report.CustomerClaimCode = r.CustomerClaimCode;
                report.CustomerClaimName = r.CustomerClaimName;
                report.Amount = r.Amount;
                report.AtoAmount = r.AtoAmount;
                report.MaeAmount = r.MaeAmount;
                report.BalanceAmount = r.BalanceAmount;
                list.Add(report);
            }

            return list;
        }

        private string GetSlipType(char? SlipType)
        {
            string result;
            if (SlipType == '0') result = "車";
            else if (SlipType == '1') result = "サ";
            else result = "";
            return result;
        }
    }
}
