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
    /// 車両登録依頼書
    /// </summary>
    public partial class CarRegistRequest : GrapeCity.ActiveReports.SectionReport
    {

        public CarRegistRequest()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private void detail_Format(object sender, EventArgs e)
        {

        }

        private void CarRegistRequest_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            // Add 2015/03/13 arc nakayama 車両登録依頼書の余白を指定するように変更(#3168)
            this.PageSettings.Margins.Top = (float)0.3; //余白上（インチ）
            this.PageSettings.Margins.Bottom = (float)0.0; //余白下（インチ）
            this.PageSettings.Margins.Left = (float)0.0; //余白左（インチ）
            this.PageSettings.Margins.Right = (float)0.0; //余白右（インチ）
            this.PageSettings.Gutter = (float)0.8; //とじしろ（インチ）
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }

        /// <summary>
        /// 車両見積データfetch処理
        /// </summary>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 新規作成
        /// </history>
        private void CarRegistRequest_FetchData(object sender, EventArgs e)
        {
            DateTime judgedate = new DateTime(2019, 10, 1);

            DateTime RequestRegistDate = new DateTime(2999, 1, 1);

            if (!string.IsNullOrWhiteSpace(Fields["RequestRegistDate"].Value.ToString()))
            {
                DateTime.TryParse(Fields["RequestRegistDate"].Value.ToString(), out RequestRegistDate);
            }

            //登録希望日が10/1以降の場合は文言変更
            if (RequestRegistDate >= judgedate)
            {
                this.label31.Text = "自動車税種別割";
                this.label34.Text = "自動車税環境性能割";
            }

             //Add 2021/08/02 yano #4097
            //新中区分で年式の参照元データを変更
            string newusedtype = "";
            try { newusedtype = (Fields["NewUsedType"].Value.ToString() ?? ""); }
            catch (InvalidCastException) { }

             //新車の場合…グレードマスタの年式
            if (newusedtype.Equals("N"))
            {
                try { this.textBox1.Text = (Fields["ModelYear"].Value.ToString() ?? "") + " " + (Fields["CarName"].Value.ToString() ?? ""); }
                catch (InvalidCastException) { }
            }
            else//中古車の場合…車両マスタの年式
            {
                try { this.textBox1.Text = (Fields["ManufacturingYear"].Value.ToString() ?? "") + " " + (Fields["CarName"].Value.ToString() ?? ""); }
                catch (InvalidCastException) { }
            }
        }
    }
}
