using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CrmsDao;
using System.Linq;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Controls;
using GrapeCity.ActiveReports.SectionReportModel;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.Document;

namespace CrmsReport
{
    /// <summary>
    /// 車両納車確認書（会社控え）の概要の説明です。
    /// </summary>
    public partial class CarDeliveryReport_hikae : GrapeCity.ActiveReports.SectionReport
    {

        public CarDeliveryReport_hikae()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private void detail_Format(object sender, EventArgs e)
        {


        }

        private void CarDeliveryReport_hikae_FetchData(object sender, FetchEventArgs eArgs)
        {
            List<CarReceiptData> list = new List<CarReceiptData>();

            CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();
            CarSalesHeader header = new CarSalesOrderDao(db).GetByKey(SlipNumber, int.Parse(RevisionNumber));

            //ローンがある場合
            if (!string.IsNullOrEmpty(header.PaymentPlanType))
            {
                list.Add(new CarReceiptData
                {
                    ReceiptDate = header.SalesOrderDate,
                    ReceiptAmount = header.LoanPrincipalAmount,
                    ReceiptType = "ローン"
                });
            }

            ////現金
            //List<Journal> receiptList = new JournalDao(db).GetListBySlipNumber(SlipNumber).Where(x => x.JournalType.Equals("001")).ToList();
            //foreach (var a in receiptList) {
            //    if (a.CustomerClaim!=null && a.CustomerClaim.CustomerClaimType != null && !a.CustomerClaim.CustomerClaimType.Equals("004")) {
            //        list.Add(new CarReceiptData {
            //            ReceiptDate = a.JournalDate,
            //            ReceiptAmount = a.Amount,
            //            ReceiptType = "現金"
            //        });
            //    }
            //}

            //現金
            // Mod 2015/11/02 arc nakayama #3297_サービス伝票入力画面の入金実績設定値の不具合
            //Add 2016/12/16 arc nakayama #3681_車両伝票　納車確認書に「下取」と「残債」の入金予定・実績が表示されている
            List<Journal> receiptList = new JournalDao(db).GetJournalCalcListBySlipNumber(SlipNumber);
            var query = from a in receiptList
                        where a.CustomerClaim != null && a.CustomerClaim.CustomerClaimType != null && !a.CustomerClaim.CustomerClaimType.Equals("004") && (!a.AccountType.Equals("012") && !a.AccountType.Equals("013"))
                        group a by new { JournalDate = a.JournalDate, AccountType = a.AccountType.Equals("005") ? "振込" : a.c_AccountType.Name } into grp
                        select new
                        {
                            JournalDate = grp.Key.JournalDate,
                            AccountType = grp.Key.AccountType,
                            Amount = grp.Sum(x => x.Amount)
                        };

            foreach (var a in query)
            {
                //if (a.CustomerClaim.CustomerClaimType != null && !a.CustomerClaim.CustomerClaimType.Equals("004")) {
                list.Add(new CarReceiptData
                {
                    ReceiptDate = a.JournalDate,
                    ReceiptType = a.AccountType,
                    ReceiptAmount = a.Amount
                });
                //}
            }

            //入金日で並び替え
            list = list.OrderBy(x => x.ReceiptDate).ToList();

            //レポートにバインド
            int count = list.Count;
            if (count > 0)
            {
                txtReceiptDate1.Value = list[0].ReceiptDate;
                txtReceiptAmount1.Value = list[0].ReceiptAmount;
                txtReceiptType1.Value = list[0].ReceiptType;
                if (count > 1)
                {
                    txtReceiptDate2.Value = list[1].ReceiptDate;
                    txtReceiptAmount2.Value = list[1].ReceiptAmount;
                    txtReceiptType2.Value = list[1].ReceiptType;
                    if (count > 2)
                    {
                        txtReceiptDate3.Value = list[2].ReceiptDate;
                        txtReceiptAmount3.Value = list[2].ReceiptAmount;
                        txtReceiptType3.Value = list[2].ReceiptType;
                        if (count > 3)
                        {
                            txtReceiptDate4.Value = list[3].ReceiptDate;
                            txtReceiptAmount4.Value = list[3].ReceiptAmount;
                            txtReceiptType4.Value = list[3].ReceiptType;
                            if (count > 4)
                            {
                                txtReceiptDate5.Value = list[4].ReceiptDate;
                                txtReceiptAmount5.Value = list[4].ReceiptAmount;
                                txtReceiptType5.Value = list[4].ReceiptType;
                                if (count > 5)
                                {
                                    txtReceiptDate6.Value = list[5].ReceiptDate;
                                    txtReceiptAmount6.Value = list[5].ReceiptAmount;
                                    txtReceiptType6.Value = list[5].ReceiptType;
                                    if (count > 6)
                                    {
                                        txtReceiptDate7.Value = list[6].ReceiptDate;
                                        txtReceiptAmount7.Value = list[6].ReceiptAmount;
                                        txtReceiptType7.Value = list[6].ReceiptType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            decimal? tradeInAppropriation = 0;
            try { tradeInAppropriation = (decimal)Fields["TradeInAppropriation"].Value; }
            catch (InvalidCastException) { }
            decimal? grandTotalAmount = 0;
            try { grandTotalAmount = (decimal)Fields["GrandTotalAmount"].Value; }
            catch (InvalidCastException) { }
            this.txtTodaysReceiptAmount.Value = grandTotalAmount - (list.Sum(x => x.ReceiptAmount) ?? 0m) + (tradeInAppropriation ?? 0);
            string customerName = "";
            string customerType = "";
            try { customerType = Fields["CustomerType"].Value.ToString(); }
            catch { }

            if (customerType.Equals("002"))
            {
                customerName = Fields["FirstName"].Value + "\r\n" + Fields["LastName"].Value;
            }
            else
            {
                customerName = Fields["CustomerName"].Value != null ? Fields["CustomerName"].Value.ToString() : "";
            }
            this.txtCustomerName.Text = customerName;
        }

        private void CarDeliveryReport_hikae_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Landscape).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;
        }
  
    }
}
