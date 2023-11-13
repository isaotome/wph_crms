using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Controls;
using GrapeCity.ActiveReports.SectionReportModel;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.Document;

namespace CrmsReport {
    /// <summary>
    /// ServiceReceiptionReport の概要の説明です。
    /// </summary>
    public partial class ServiceReceiptionReport : GrapeCity.ActiveReports.SectionReport
    {

        public ServiceReceiptionReport()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        /// <summary>
        /// 問診表データ設定
        /// </summary>
        /// <param name="sender">モデルデータ</param>
        /// <param name="sender">モデルデータ</param>
        /// <history>
        /// 2020/06/08 yano #4053【サービス受付】車両整備・修理受付問診表に顧客のメールアドレス追加 新規作成
        /// </history>
        private void ServiceReceiptionReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            string customerMailAddress = "";
            try { customerMailAddress = (string)Fields["CustomerMailAddress"].Value; }
            catch (InvalidCastException) { }

            string customerMobileMailAddress = "";
            try { customerMobileMailAddress = (string)Fields["CustomerMobileMailAddress"].Value; }
            catch (InvalidCastException) { }

            //連絡先メールアドレスに値を設定
            this.txtCustomerMailAddress.Value = string.IsNullOrWhiteSpace(customerMailAddress) ? customerMobileMailAddress : customerMailAddress;

        }

        private void ServiceReceiptionReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;

        }

    }
}
