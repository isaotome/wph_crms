namespace CrmsReport
{
    /// <summary>
    /// subRep6 の概要の説明です。
    /// </summary>
    partial class subRep_CarSalesOrder
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(subRep_CarSalesOrder));
            this.detail = new GrapeCity.ActiveReports.SectionReportModel.Detail();
            this.txtCarOptionName = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.txtAmount1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.pageHeader = new GrapeCity.ActiveReports.SectionReportModel.PageHeader();
            this.pageFooter = new GrapeCity.ActiveReports.SectionReportModel.PageFooter();
            ((System.ComponentModel.ISupportInitialize)(this.txtCarOptionName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAmount1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // detail
            // 
            this.detail.CanGrow = false;
            this.detail.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.txtCarOptionName,
            this.txtAmount1});
            this.detail.Height = 0.188F;
            this.detail.Name = "detail";
            this.detail.RepeatToFill = true;
            // 
            // txtCarOptionName
            // 
            this.txtCarOptionName.CanGrow = false;
            this.txtCarOptionName.DataField = "CarOptionName";
            this.txtCarOptionName.Height = 0.16F;
            this.txtCarOptionName.Left = 0.01023622F;
            this.txtCarOptionName.MultiLine = false;
            this.txtCarOptionName.Name = "txtCarOptionName";
            this.txtCarOptionName.Style = "font-family: ＭＳ 明朝; font-size: 9.75pt; white-space: nowrap; ddo-char-set: 128";
            this.txtCarOptionName.Text = "txtCarOptionName";
            this.txtCarOptionName.Top = 0.02165354F;
            this.txtCarOptionName.Width = 2.349F;
            // 
            // txtAmount1
            // 
            this.txtAmount1.DataField = "= Amount + TaxAmount";
            this.txtAmount1.Height = 0.16F;
            this.txtAmount1.Left = 2.435F;
            this.txtAmount1.Name = "txtAmount1";
            this.txtAmount1.OutputFormat = resources.GetString("txtAmount1.OutputFormat");
            this.txtAmount1.Style = "font-family: ＭＳ 明朝; font-size: 9.75pt; text-align: right; ddo-char-set: 128";
            this.txtAmount1.Text = "txtAmount1";
            this.txtAmount1.Top = 0.022F;
            this.txtAmount1.Width = 0.802F;
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
            // subRep_CarSalesOrder
            // 
            this.MasterReport = false;
            this.PageSettings.Margins.Bottom = 0F;
            this.PageSettings.Margins.Left = 0F;
            this.PageSettings.Margins.Right = 0F;
            this.PageSettings.Margins.Top = 0F;
            this.PageSettings.PaperHeight = 11F;
            this.PageSettings.PaperWidth = 8.5F;
            this.PrintWidth = 3.287403F;
            this.Sections.Add(this.pageHeader);
            this.Sections.Add(this.detail);
            this.Sections.Add(this.pageFooter);
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-style: normal; text-decoration: none; font-weight: normal; font-size: 10pt; " +
            "color: Black; font-family: MS UI Gothic; ddo-char-set: 128", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 16pt; font-weight: bold", "Heading1", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 14pt; font-weight: bold", "Heading2", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 13pt; font-weight: bold", "Heading3", "Normal"));
            this.ReportStart += new System.EventHandler(this.subRep6_ReportStart);
            ((System.ComponentModel.ISupportInitialize)(this.txtCarOptionName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtAmount1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion





        private GrapeCity.ActiveReports.SectionReportModel.Detail detail;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtCarOptionName;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtAmount1;
        private GrapeCity.ActiveReports.SectionReportModel.PageHeader pageHeader;
        private GrapeCity.ActiveReports.SectionReportModel.PageFooter pageFooter;
    }
}
