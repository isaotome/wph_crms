namespace CrmsReport
{
    /// <summary>
    /// NewActiveReport8 の概要の説明です。
    /// </summary>
    partial class subRep
    {


        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        #region ActiveReport デザイナで生成されたコード
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。
        /// このメソッドの内容をコード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(subRep));
            this.detail = new GrapeCity.ActiveReports.SectionReportModel.Detail();
            this.line2 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line3 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.txtLineNumber = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.txtCarOptionName = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.txtAmount1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.line1 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line4 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.pageHeader = new GrapeCity.ActiveReports.SectionReportModel.PageHeader();
            this.pageFooter = new GrapeCity.ActiveReports.SectionReportModel.PageFooter();
            ((System.ComponentModel.ISupportInitialize)(this.txtLineNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCarOptionName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAmount1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // detail
            // 
            this.detail.CanGrow = false;
            this.detail.ColumnSpacing = 0F;
            this.detail.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.line2,
            this.line3,
            this.txtLineNumber,
            this.txtCarOptionName,
            this.txtAmount1,
            this.line1,
            this.line4});
            this.detail.Height = 0.1574803F;
            this.detail.Name = "detail";
            this.detail.RepeatToFill = true;
            // 
            // line2
            // 
            this.line2.Height = 0F;
            this.line2.Left = 0F;
            this.line2.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.line2.LineWeight = 1F;
            this.line2.Name = "line2";
            this.line2.Top = 0F;
            this.line2.Width = 2.677083F;
            this.line2.X1 = 0F;
            this.line2.X2 = 2.677083F;
            this.line2.Y1 = 0F;
            this.line2.Y2 = 0F;
            // 
            // line3
            // 
            this.line3.Height = 0.1653543F;
            this.line3.Left = 0.2165354F;
            this.line3.LineColor = System.Drawing.Color.Green;
            this.line3.LineWeight = 1F;
            this.line3.Name = "line3";
            this.line3.Top = 0F;
            this.line3.Width = 0F;
            this.line3.X1 = 0.2165354F;
            this.line3.X2 = 0.2165354F;
            this.line3.Y1 = 0F;
            this.line3.Y2 = 0.1653543F;
            // 
            // txtLineNumber
            // 
            this.txtLineNumber.DataField = "LineNumber";
            this.txtLineNumber.Height = 0.1181102F;
            this.txtLineNumber.Left = 0F;
            this.txtLineNumber.Name = "txtLineNumber";
            this.txtLineNumber.Style = "font-family: ＭＳ 明朝; font-size: 9pt; text-align: center; ddo-char-set: 1";
            this.txtLineNumber.Text = "txtLineNumber";
            this.txtLineNumber.Top = 0.02165354F;
            this.txtLineNumber.Width = 0.1968504F;
            // 
            // txtCarOptionName
            // 
            this.txtCarOptionName.DataField = "CarOptionName";
            this.txtCarOptionName.Height = 0.1181102F;
            this.txtCarOptionName.Left = 0.2397638F;
            this.txtCarOptionName.Name = "txtCarOptionName";
            this.txtCarOptionName.Style = "font-family: ＭＳ 明朝; font-size: 9pt; ddo-char-set: 1";
            this.txtCarOptionName.Text = "txtCarOptionName";
            this.txtCarOptionName.Top = 0.02165354F;
            this.txtCarOptionName.Width = 1.699213F;
            // 
            // txtAmount1
            // 
            this.txtAmount1.DataField = "= Amount + TaxAmount";
            this.txtAmount1.Height = 0.1181102F;
            this.txtAmount1.Left = 2.051969F;
            this.txtAmount1.Name = "txtAmount1";
            this.txtAmount1.Style = "font-family: ＭＳ 明朝; font-size: 9pt; text-align: right; ddo-char-set: 1";
            this.txtAmount1.Text = "txtAmount1";
            this.txtAmount1.Top = 0.02165354F;
            this.txtAmount1.Width = 0.6141734F;
            // 
            // line1
            // 
            this.line1.Height = 0.03937F;
            this.line1.Left = 2.467717F;
            this.line1.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.line1.LineWeight = 1F;
            this.line1.Name = "line1";
            this.line1.Top = 0.1259843F;
            this.line1.Width = 0F;
            this.line1.X1 = 2.467717F;
            this.line1.X2 = 2.467717F;
            this.line1.Y1 = 0.1259843F;
            this.line1.Y2 = 0.1653543F;
            // 
            // line4
            // 
            this.line4.Height = 0.03937F;
            this.line4.Left = 2.275591F;
            this.line4.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.line4.LineWeight = 1F;
            this.line4.Name = "line4";
            this.line4.Top = 0.1259843F;
            this.line4.Width = 0F;
            this.line4.X1 = 2.275591F;
            this.line4.X2 = 2.275591F;
            this.line4.Y1 = 0.1259843F;
            this.line4.Y2 = 0.1653543F;
            // 
            // pageHeader
            // 
            this.pageHeader.Height = 0F;
            this.pageHeader.Name = "pageHeader";
            // 
            // pageFooter
            // 
            this.pageFooter.Height = 0.003937008F;
            this.pageFooter.Name = "pageFooter";
            // 
            // subRep
            // 
            this.MasterReport = false;
            this.PageSettings.Margins.Bottom = 0F;
            this.PageSettings.Margins.Left = 0F;
            this.PageSettings.Margins.Right = 0F;
            this.PageSettings.Margins.Top = 0F;
            this.PageSettings.PaperHeight = 11F;
            this.PageSettings.PaperWidth = 8.5F;
            this.PrintWidth = 2.692914F;
            this.Sections.Add(this.pageHeader);
            this.Sections.Add(this.detail);
            this.Sections.Add(this.pageFooter);
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-style: normal; text-decoration: none; font-weight: normal; font-size: 10pt; " +
                        "color: Black; font-family: MS UI Gothic; ddo-char-set: 128", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 16pt; font-weight: bold", "Heading1", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 14pt; font-weight: bold", "Heading2", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 13pt; font-weight: bold", "Heading3", "Normal"));
            this.ReportStart += new System.EventHandler(this.subRep_ReportStart);
            ((System.ComponentModel.ISupportInitialize)(this.txtLineNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCarOptionName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAmount1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion










        private GrapeCity.ActiveReports.SectionReportModel.Detail detail;
        private GrapeCity.ActiveReports.SectionReportModel.Line line2;
        private GrapeCity.ActiveReports.SectionReportModel.Line line3;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtLineNumber;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtCarOptionName;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtAmount1;
        private GrapeCity.ActiveReports.SectionReportModel.PageHeader pageHeader;
        private GrapeCity.ActiveReports.SectionReportModel.PageFooter pageFooter;
        private GrapeCity.ActiveReports.SectionReportModel.Line line1;
        private GrapeCity.ActiveReports.SectionReportModel.Line line4;
    }
}
