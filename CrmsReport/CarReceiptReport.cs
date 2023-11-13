using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CrmsDao;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Controls;
using GrapeCity.ActiveReports.SectionReportModel;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.Document;

namespace CrmsReport {
    /// <summary>
    /// CarReceiptReport の概要の説明です。
    /// </summary>
    public partial class CarReceiptReport : GrapeCity.ActiveReports.SectionReport
    {

        public CarReceiptReport()
        {
            InitializeComponent();
        }

        private void CarReceiptReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }

        private void pageHeader_Format(object sender, EventArgs e)
        {

        }

        private void detail_Format(object sender, EventArgs e)
        {

            CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();

            //現金
            // Mod 2015/11/02 arc nakayama #3297_サービス伝票入力画面の入金実績設定値の不具合
            List<Journal> receiptList = new JournalDao(db).GetJournalCalcListBySlipNumber(SlipNumber);
            //Add 2016/12/16 arc nakayama #3680_車両登録依頼書に「下取」と「残債」の入金予定・実績が表示されている
            var query = receiptList.Where(x => x.CustomerClaim != null && x.CustomerClaim.CustomerClaimType != null && !x.CustomerClaim.CustomerClaimType.Equals("004") && x.JournalType.Equals("001") && (!x.AccountType.Equals("012") && !x.AccountType.Equals("013"))).OrderBy(y => y.JournalDate).ToList();

            List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(SlipNumber);
            //Add 2016/12/16 arc nakayama #3680_車両登録依頼書に「下取」と「残債」の入金予定・実績が表示されている
            var planQuery = planList.Where(x => !x.ReceiptType.Equals("004") && (!x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")) && x.CompleteFlag.Equals("0")).OrderBy(y => y.ReceiptPlanDate).ToList();
            foreach (var a in planQuery)
            {
                query.Add(new Journal() { JournalDate = a.ReceiptPlanDate ?? DateTime.Today, Amount = a.ReceivableBalance ?? 0m, Summary = "入金予定" });
            }

            //レポートにバインド
            int count = query.Count();
            if (count > 0)
            {
                txtReceiptDate1.Value = query[0].JournalDate;
                txtReceiptAmount1.Value = query[0].Amount;
                txtReceiptSummary1.Value = query[0].Summary;
                txtAccountType1.Value = query[0].c_AccountType != null ? query[0].c_AccountType.Name : "";
                if (count > 1)
                {
                    txtReceiptDate2.Value = query[1].JournalDate;
                    txtReceiptAmount2.Value = query[1].Amount;
                    txtReceiptSummary2.Value = query[1].Summary;
                    txtAccountType2.Value = query[1].c_AccountType != null ? query[1].c_AccountType.Name : "";
                    if (count > 2)
                    {
                        txtReceiptDate3.Value = query[2].JournalDate;
                        txtReceiptAmount3.Value = query[2].Amount;
                        txtReceiptSummary3.Value = query[2].Summary;
                        txtAccountType3.Value = query[2].c_AccountType != null ? query[2].c_AccountType.Name : "";
                        if (count > 3)
                        {
                            txtReceiptDate4.Value = query[3].JournalDate;
                            txtReceiptAmount4.Value = query[3].Amount;
                            txtReceiptSummary4.Value = query[3].Summary;
                            txtAccountType4.Value = query[3].c_AccountType != null ? query[3].c_AccountType.Name : "";
                        }
                    }
                }
            }


        }
    }
}
