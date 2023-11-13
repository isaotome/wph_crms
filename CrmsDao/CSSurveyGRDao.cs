using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class CSSurveyGR
    {
        //拠点名
        public string DepartmentName { get; set; }

        //顧客名
        public string CustomerName { get; set; }

        //車台番号
        public string Vin { get; set; }

        //伝票番号
        public string SlipNumber { get; set; }

        //伝票ステイタス
        public string OrderStatusName { get; set; }

        //作業完了日
        public String WorkingEndDate { get; set; }

        //納車日
        public String SalesDate { get; set; }

        //主作業
        public string MainWorkName { get; set; }

        //フロント担当者
        public string FrontName { get; set; }

        //担当営業
        public string SalesName { get; set; }
    }
    public class CSSurveyGRDao
    {
        private CrmsLinqDataContext db;
        public CSSurveyGRDao(CrmsLinqDataContext context)
        {
            db = context;
        }


        /// <summary>
        /// CSサーベイデータ取得
        /// </summary>
        /// <param name="fromDate">期間FROM</param>
        /// <param name="toDate">期間TO</param>
        /// <returns></returns>
        public List<CSSurveyGR> GetCSSurveyGRtList(DateTime fromDate, DateTime toDate, String DepartMentCode)
        {
            var query = from sh in db.ServiceSalesHeader
                        join sl in
                        (
                            from sline in db.ServiceSalesLine
                            where new[] { "10315", "10204", "10301", "10202", "10501" }.Contains(sline.ServiceWorkCode)
                            group sline by new
                            {
                                sline.SlipNumber,
                                sline.RevisionNumber,
                                sline.ServiceWorkCode
                            } into gsline
                            select new
                            {
                                SlipNumber = gsline.Key.SlipNumber,
                                RevisionNumber = gsline.Key.RevisionNumber,
                                ServiceWorkCode = gsline.Key.ServiceWorkCode
                            }
                        ) on new { sh.SlipNumber, sh.RevisionNumber } equals new { sl.SlipNumber, sl.RevisionNumber }

                        join d in db.Department on new { a = sh.DepartmentCode, b = "0" } equals new { a = d.DepartmentCode, b = d.DelFlag }
                        join c in db.Customer on new { a = sh.CustomerCode, b = "0" } equals new { a = c.CustomerCode, b = c.DelFlag }
                        join cs in db.c_ServiceOrderStatus on new { a = sh.ServiceOrderStatus, b = "0" } equals new { a = cs.Code, b = cs.DelFlag }
                        join sw in db.ServiceWork on new { a = sl.ServiceWorkCode, b = "0" } equals new { a = sw.ServiceWorkCode, b = sw.DelFlag }
                        join e in db.Employee on new { a = sh.FrontEmployeeCode, b = "0" } equals new { a = e.EmployeeCode, b = e.DelFlag } into ge
                        from fe in ge.DefaultIfEmpty()
                        join e2 in db.Employee on new { a = c.CarEmployeeCode, b = "0" } equals new { a = e2.EmployeeCode, b = e2.DelFlag } into ge2
                        from se in ge2.DefaultIfEmpty()
                        where sh.DelFlag.Contains("0") && (sh.WorkingEndDate >= fromDate && sh.WorkingEndDate <= toDate) && sh.DepartmentCode.Contains(DepartMentCode)
                        orderby sh.DepartmentCode, sh.SlipNumber, sl.ServiceWorkCode, sh.WorkingEndDate
                        select new
                        {
                            DepartmentName = d.DepartmentName,
                            CustomerName = c.CustomerName,
                            Vin = sh.Vin,
                            SlipNumber = sh.SlipNumber,
                            OrderStatusName = cs.Name,
                            WorkingEndDate = sh.WorkingEndDate,
                            SalesDate = sh.SalesDate,
                            MainWorkName = sw.Name,
                            FrontName = (fe.EmployeeName == null ? string.Empty : fe.EmployeeName),
                            SalesName = (se.EmployeeName == null ? string.Empty : se.EmployeeName)
                        };
            List<CSSurveyGR> list = new List<CSSurveyGR>();
            foreach (var r in query)
            {
                CSSurveyGR report = new CSSurveyGR();
                report.DepartmentName = r.DepartmentName;
                report.CustomerName = r.CustomerName;
                report.Vin = r.Vin;
                report.SlipNumber = r.SlipNumber;
                report.OrderStatusName = r.OrderStatusName;
                report.WorkingEndDate = (r.WorkingEndDate == null ? String.Empty : r.WorkingEndDate.Value.ToShortDateString());
                report.SalesDate = (r.SalesDate == null ? String.Empty : r.SalesDate.Value.ToShortDateString());
                report.MainWorkName = r.MainWorkName;
                report.FrontName = r.FrontName;
                report.SalesName = r.SalesName;
                list.Add(report);
            }
            return list;
        }

    }

}
