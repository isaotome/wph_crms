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
using System.Globalization;

namespace CrmsReport {
    /// <summary>
    /// CarArrivalReport の概要の説明です。
    /// </summary>
    public partial class CarArrivalReport : GrapeCity.ActiveReports.SectionReport
    {

        public CarArrivalReport()
        {
            InitializeComponent();
        }

        private void CarArrivalReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            if (Fields["CarPurchaseType"] != null)
            {
                switch (Fields["CarPurchaseType"].Value.ToString())
                {
                    case "001":
                        this.shapeShitadori.Visible = true;
                        break;
                    case "002":
                        this.shapeKaitori.Visible = true;
                        break;
                    case "003":
                        this.shapeAAShiire.Visible = true;
                        break;
                    case "004":
                        this.shapeGyouhanShiire.Visible = true;
                        break;
                    case "005":
                        this.shapeIhai.Visible = true;
                        break;
                }
            }

            if (Fields["TradeInRemainDebt"] != null && decimal.Parse(Fields["TradeInRemainDebt"].Value.ToString()) > 0)
            {
                this.shapeAri.Visible = true;
            }
            else
            {
                this.shapeNasi.Visible = true;
            }

            //Add 2015/09/01 arc nakayama #3214_入庫連絡票への項目追加 西暦を和暦に変換する
            //Add 2015/09/16 arc nakayama 入庫連絡票が出力されないバグ修正
            if (!string.IsNullOrEmpty(Fields["ExpireDate"].Value.ToString()))
            {
                System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("ja-JP");
                System.Globalization.JapaneseCalendar jp = new System.Globalization.JapaneseCalendar();

                DateTime dt = (DateTime)Fields["ExpireDate"].Value;

                ci.DateTimeFormat.Calendar = jp;

                // 「書式」「カルチャの書式情報」を使用し、文字列に変換します。
                this.ExpireDate.Value = "　" + dt.ToString("ggyy年M月d日", ci);
            }
            else
            {
                this.ExpireDate.Value = "";
            }
        }

        private void CarArrivalReport_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Landscape).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;
        }
    }
}
