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

namespace CrmsReport
{
    /// <summary>
    /// 作業指示書 の概要の説明です。
    /// </summary>
    public partial class OutSourceRequestReport : GrapeCity.ActiveReports.SectionReport
    {

        public OutSourceRequestReport()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }


        private decimal? tcamount = 0m;  //Add 2022/05/25 yano #4136
         

        /// <summary>
        /// 明細行の成形
        /// </summary>
        /// <history>
        /// 2022/05/25 yano #4136 【サービス伝票】外注依頼書表示
        /// </history>
        /// <param name="sender">オブジェクト</param>
        private void detail_Format(object sender, EventArgs e)
        {
            //技術料の加算
            try  { tcamount += (decimal)Fields["Cost"].Value; }
            catch (InvalidCastException) { }
            catch (NullReferenceException) { }     
        }

        /// <summary>
        ///データ設定
        /// </summary>
        /// <history>
        /// 2022/05/25 yano #4136 【サービス伝票】外注依頼書表示
        /// </history>
        /// <param name="sender">オブジェクト</param>
        private void OutSourceRequestReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            //消費税
            decimal? taxAmount = 0m;
            decimal? rate = null;

            try { rate = decimal.Parse(Fields["Rate"].Value.ToString()); }
            catch (InvalidCastException) { }
            catch (NullReferenceException) { }

            rate = rate / 100m;

            if (tcamount > 0) {
                taxAmount =  Math.Floor((tcamount ?? 0m) * (rate ?? 0m));
            } else {
                taxAmount =  Math.Ceiling((tcamount ?? 0m) * (rate ?? 0m));
            }

            //技術料合計
            this.txtEngineerTotalAmount.Value = tcamount;

            //消費税
            this.txtTotalTaxAmount.Value = taxAmount;
                 
            //総合計
            this.txtGrandTotalAmount.Value = tcamount + taxAmount;
        }

        private void OutSourceRequestReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }
    }
}
