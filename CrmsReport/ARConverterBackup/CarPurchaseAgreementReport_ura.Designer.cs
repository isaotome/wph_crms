namespace CrmsReport {
    /// <summary>
    /// CarPurchaseAgreementReport_ura の概要の説明です。
    /// </summary>
    partial class CarPurchaseAgreementReport_ura
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CarPurchaseAgreementReport_ura));
      this.detail = new GrapeCity.ActiveReports.SectionReportModel.Detail();
      this.label1 = new GrapeCity.ActiveReports.SectionReportModel.Label();
      this.label2 = new GrapeCity.ActiveReports.SectionReportModel.Label();
      ((System.ComponentModel.ISupportInitialize)(this.label1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.label2)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
      // 
      // detail
      // 
      this.detail.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.label1,
            this.label2});
      this.detail.Height = 11.05906F;
      this.detail.Name = "detail";
      // 
      // label1
      // 
      this.label1.Height = 10.662F;
      this.label1.HyperLink = null;
      this.label1.Left = 0F;
      this.label1.Name = "label1";
      this.label1.Style = "font-family: ＭＳ Ｐ明朝; font-size: 8pt";
      this.label1.Text = resources.GetString("label1.Text");
      this.label1.Top = 0.39F;
      this.label1.Width = 6.98819F;
      // 
      // label2
      // 
      this.label2.Height = 0.2F;
      this.label2.HyperLink = null;
      this.label2.Left = 0F;
      this.label2.Name = "label2";
      this.label2.Style = "text-align: center";
      this.label2.Text = "車輌買取契約書約款";
      this.label2.Top = 0.08346457F;
      this.label2.Width = 6.98819F;
      // 
      // CarPurchaseAgreementReport_ura
      // 
      this.MasterReport = false;
      this.PageSettings.Margins.Bottom = 0.1968504F;
      this.PageSettings.Margins.Left = 0.5905512F;
      this.PageSettings.Margins.Right = 0.5905512F;
      this.PageSettings.Margins.Top = 0.3937008F;
      this.PageSettings.PaperHeight = 11F;
      this.PageSettings.PaperWidth = 8.5F;
      this.PrintWidth = 6.98819F;
      this.Sections.Add(this.detail);
      this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-style: normal; text-decoration: none; font-weight: normal; font-size: 10pt; " +
            "color: Black; font-family: MS UI Gothic; ddo-char-set: 128", "Normal"));
      this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 16pt; font-weight: bold", "Heading1", "Normal"));
      this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 14pt; font-weight: bold", "Heading2", "Normal"));
      this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 13pt; font-weight: bold", "Heading3", "Normal"));
      this.ReportStart += new System.EventHandler(this.CarPurchaseAgreementReport_ura_ReportStart);
      ((System.ComponentModel.ISupportInitialize)(this.label1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.label2)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion



        private GrapeCity.ActiveReports.SectionReportModel.Detail detail;
        private GrapeCity.ActiveReports.SectionReportModel.Label label1;
        private GrapeCity.ActiveReports.SectionReportModel.Label label2;
    }
}
