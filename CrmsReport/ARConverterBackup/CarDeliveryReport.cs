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

namespace CrmsReport
{
    /// <summary>
    /// 車両納車確認書（お客様控え） の概要の説明です。
    /// </summary>
    public partial class CarDeliveryReport : GrapeCity.ActiveReports.SectionReport
    {

        public CarDeliveryReport()
        {
            InitializeComponent();
        }

        private void detail_Format(object sender, EventArgs e)
        {
        }


        /// <summary>
        /// データ取得処理
        /// </summary>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2021/01/04 yano #4073 【車両伝票入力】納車確認書のレイアウト変更 下取残債が０の場合は、下取残債を非表示化
        /// </history>
        private void CarDeliveryReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            List<CarReceiptData> list = new List<CarReceiptData>();

            CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();
            CarSalesHeader header = new CarSalesOrderDao(db).GetByKey(SlipNumber, int.Parse(RevisionNumber));

            //ローンがある場合
            //if (!string.IsNullOrEmpty(header.PaymentPlanType)) {
            //    list.Add(new CarReceiptData {
            //        ReceiptDate = header.SalesOrderDate,
            //        ReceiptAmount = header.LoanPrincipalAmount,
            //        ReceiptType = "ローン"
            //    });
            //}

            //現金（2011/12/01 ローンも含むよう修正）
            // Mod 2015/11/02 arc nakayama #3297_サービス伝票入力画面の入金実績設定値の不具合
            List<Journal> receiptList = new JournalDao(db).GetJournalCalcListBySlipNumber(SlipNumber); // 伝票番号指定した入金伝票を抽出

            // 入金日・金種（現金ごとにサマリ
            //Add 2016/12/16 arc nakayama #3681_車両伝票　納車確認書に「下取」と「残債」の入金予定・実績が表示されている
            var query = from a in receiptList
                        // 2011/12/01 ローン会社も含む
                        //where a.CustomerClaim != null && a.CustomerClaim.CustomerClaimType != null && !a.CustomerClaim.CustomerClaimType.Equals("004")
                        where (!a.AccountType.Equals("012") && !a.AccountType.Equals("013"))
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

            //Add 2021/01/04 yano #4073
            //下取残債
            decimal? tradeInRemainDebt = 0;
            try { tradeInRemainDebt = (decimal)Fields["TradeInRemainDebt"].Value; }
            catch (InvalidCastException) { }

            //下取車残債が0の場合、非表示にする
            if (tradeInRemainDebt == 0)
            {
                this.lblTradeInRemainDebt.Visible = false;
                this.txtTradeInRemainDebt.Visible = false;
                this.txtTradeInRemainDebtYen.Visible = false;
            }

            //下取金額（残債含まず）
            decimal? tradeInAmount = 0;
            try { tradeInAmount = (decimal)Fields["TradeInAmount"].Value; }
            catch (InvalidCastException) { }

            this.txtTradeInRemainDebt.Value = tradeInRemainDebt;

            //お支払合計
            decimal? totalPaymentAmount = 0m;
            this.txtTotalPaymentAmount.Value = totalPaymentAmount = grandTotalAmount + tradeInRemainDebt;

            //お支払残額（支払うべき金額の合計（残債含む）－入金額の合計－下取車両金額（残債含まず）
            this.txtTodaysReceiptAmount.Value = totalPaymentAmount - (list.Sum(x => x.ReceiptAmount) ?? 0m) - tradeInAmount;
            //this.txtTodaysReceiptAmount.Value = grandTotalAmount - (list.Sum(x => x.ReceiptAmount) ?? 0m) - (tradeInAppropriation ?? 0);

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

            this.txtHikaeTitle.Text = "（" + this.HikaeTitle + "）";

             //Add 2021/08/02 yano #4097
            //新中区分で年式の参照元データを変更
            string newusedtype = "";
            try { newusedtype = Fields["NewUsedTypeCode"].Value.ToString(); }
            catch (InvalidCastException) { }

             //新車の場合…グレードマスタの年式
            if (newusedtype.Equals("N"))
            {
                try { this.txtManufacturingYear1.Text = (Fields["ModelYear"].Value.ToString() ?? "") + " " + (Fields["CarBrandName"].Value.ToString() ?? ""); }
                catch (InvalidCastException) { }
            }
            else//中古車の場合…車両マスタの年式
            {
                try { this.txtManufacturingYear1.Text = (Fields["ManufacturingYear"].Value.ToString() ?? "") + " " + (Fields["CarBrandName"].Value.ToString() ?? ""); }
                catch (InvalidCastException) { }
            }

        }

        private void CarDeliveryReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Landscape).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;
        }
    }
}
