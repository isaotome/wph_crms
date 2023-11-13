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
    /// CarPurchaseAgreementReport_ura の概要の説明です。
    /// </summary>
    public partial class CarPurchaseAgreementReport_ura : GrapeCity.ActiveReports.SectionReport
    {

        public CarPurchaseAgreementReport_ura()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private void CarPurchaseAgreementReport_ura_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }
    }
}
