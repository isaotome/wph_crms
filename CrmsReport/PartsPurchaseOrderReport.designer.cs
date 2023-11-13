namespace CrmsReport
{
    /// <summary>
    /// NewActiveReport26_3 の概要の説明です。
    /// </summary>
    partial class PartsPurchaseOrderReport
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PartsPurchaseOrderReport));
            this.pageHeader = new GrapeCity.ActiveReports.SectionReportModel.PageHeader();
            this.reportInfo1 = new GrapeCity.ActiveReports.SectionReportModel.ReportInfo();
            this.detail = new GrapeCity.ActiveReports.SectionReportModel.Detail();
            this.line26 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.txtLineNumber1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.txtQuantity1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.txtc_WorkType_Name1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.textBox1 = new GrapeCity.ActiveReports.SectionReportModel.TextBox();
            this.pageFooter = new GrapeCity.ActiveReports.SectionReportModel.PageFooter();
            this.shape3 = new GrapeCity.ActiveReports.SectionReportModel.Shape();
            this.label12 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.line6 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line7 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line8 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line9 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.line10 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.groupHeader2 = new GrapeCity.ActiveReports.SectionReportModel.GroupHeader();
            this.crossSectionBox1 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionBox();
            this.line25 = new GrapeCity.ActiveReports.SectionReportModel.Line();
            this.crossSectionLine1 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.crossSectionLine3 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.crossSectionLine5 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.crossSectionLine6 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.label21 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label22 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label27 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label28 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label1 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label3 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label14 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label2 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label4 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label5 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.crossSectionLine2 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.crossSectionLine4 = new GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine();
            this.label6 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.label7 = new GrapeCity.ActiveReports.SectionReportModel.Label();
            this.groupFooter2 = new GrapeCity.ActiveReports.SectionReportModel.GroupFooter();
            ((System.ComponentModel.ISupportInitialize)(this.reportInfo1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLineNumber1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtc_WorkType_Name1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label27)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label28)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label14)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.label7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // pageHeader
            // 
            this.pageHeader.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.reportInfo1});
            this.pageHeader.Height = 0.2607448F;
            this.pageHeader.Name = "pageHeader";
            this.pageHeader.Format += new System.EventHandler(this.pageHeader_Format);
            // 
            // reportInfo1
            // 
            this.reportInfo1.FormatString = "{RunDateTime:yyyy年M月d日}";
            this.reportInfo1.Height = 0.2661417F;
            this.reportInfo1.Left = 7.326378F;
            this.reportInfo1.Name = "reportInfo1";
            this.reportInfo1.Style = "font-size: 18pt; text-align: right";
            this.reportInfo1.Top = 0F;
            this.reportInfo1.Width = 1.864568F;
            // 
            // detail
            // 
            this.detail.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.line26,
            this.txtLineNumber1,
            this.txtQuantity1,
            this.txtc_WorkType_Name1,
            this.textBox1});
            this.detail.Height = 0.4F;
            this.detail.Name = "detail";
            this.detail.RepeatToFill = true;
            // 
            // line26
            // 
            this.line26.Height = 0F;
            this.line26.Left = 0F;
            this.line26.LineWeight = 2F;
            this.line26.Name = "line26";
            this.line26.Top = 0.3937008F;
            this.line26.Width = 9.212599F;
            this.line26.X1 = 9.212599F;
            this.line26.X2 = 0F;
            this.line26.Y1 = 0.3937008F;
            this.line26.Y2 = 0.3937008F;
            // 
            // txtLineNumber1
            // 
            this.txtLineNumber1.CanGrow = false;
            this.txtLineNumber1.DataField = "LineNumber";
            this.txtLineNumber1.Height = 0.2F;
            this.txtLineNumber1.Left = 0F;
            this.txtLineNumber1.Name = "txtLineNumber1";
            this.txtLineNumber1.Style = "text-align: center";
            this.txtLineNumber1.Text = "txtLineNumber1";
            this.txtLineNumber1.Top = 0.1145669F;
            this.txtLineNumber1.Width = 0.3031496F;
            // 
            // txtQuantity1
            // 
            this.txtQuantity1.CanGrow = false;
            this.txtQuantity1.DataField = "Quantity";
            this.txtQuantity1.Height = 0.2F;
            this.txtQuantity1.Left = 5.483F;
            this.txtQuantity1.Name = "txtQuantity1";
            this.txtQuantity1.Style = "text-align: right";
            this.txtQuantity1.Text = "txtQuantity1";
            this.txtQuantity1.Top = 0.115F;
            this.txtQuantity1.Width = 0.427F;
            // 
            // txtc_WorkType_Name1
            // 
            this.txtc_WorkType_Name1.CanGrow = false;
            this.txtc_WorkType_Name1.DataField = "LineContents";
            this.txtc_WorkType_Name1.Height = 0.2F;
            this.txtc_WorkType_Name1.Left = 1.958F;
            this.txtc_WorkType_Name1.Name = "txtc_WorkType_Name1";
            this.txtc_WorkType_Name1.Text = "LineContents";
            this.txtc_WorkType_Name1.Top = 0.115F;
            this.txtc_WorkType_Name1.Width = 1.417F;
            // 
            // textBox1
            // 
            this.textBox1.CanGrow = false;
            this.textBox1.DataField = "PartsNumber";
            this.textBox1.Height = 0.2F;
            this.textBox1.Left = 3.625F;
            this.textBox1.Name = "textBox1";
            this.textBox1.Text = "PartsNumber";
            this.textBox1.Top = 0.115F;
            this.textBox1.Width = 1.667F;
            // 
            // pageFooter
            // 
            this.pageFooter.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.shape3,
            this.label12,
            this.line6,
            this.line7,
            this.line8,
            this.line9,
            this.line10});
            this.pageFooter.Height = 1.608613F;
            this.pageFooter.Name = "pageFooter";
            // 
            // shape3
            // 
            this.shape3.Height = 1.456693F;
            this.shape3.Left = 0F;
            this.shape3.Name = "shape3";
            this.shape3.RoundingRadius = new GrapeCity.ActiveReports.Controls.CornersRadius(9.999999F);
            this.shape3.Top = 0.09803151F;
            this.shape3.Width = 9.232284F;
            // 
            // label12
            // 
            this.label12.Height = 0.2F;
            this.label12.HyperLink = null;
            this.label12.Left = 0.08818895F;
            this.label12.Name = "label12";
            this.label12.Style = "font-size: 11.25pt; font-weight: normal; text-align: left";
            this.label12.Text = "車両摘要　その他";
            this.label12.Top = 0.158268F;
            this.label12.Width = 1.287795F;
            // 
            // line6
            // 
            this.line6.Height = 0F;
            this.line6.Left = 0.1574803F;
            this.line6.LineStyle = GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash;
            this.line6.LineWeight = 2F;
            this.line6.Name = "line6";
            this.line6.Top = 0.4755906F;
            this.line6.Width = 8.897638F;
            this.line6.X1 = 9.055119F;
            this.line6.X2 = 0.1574803F;
            this.line6.Y1 = 0.4755906F;
            this.line6.Y2 = 0.4755906F;
            // 
            // line7
            // 
            this.line7.Height = 0F;
            this.line7.Left = 0.1574803F;
            this.line7.LineStyle = GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash;
            this.line7.LineWeight = 2F;
            this.line7.Name = "line7";
            this.line7.Top = 0.7257874F;
            this.line7.Width = 8.897638F;
            this.line7.X1 = 9.055119F;
            this.line7.X2 = 0.1574803F;
            this.line7.Y1 = 0.7257874F;
            this.line7.Y2 = 0.7257874F;
            // 
            // line8
            // 
            this.line8.Height = 0F;
            this.line8.Left = 0.1574803F;
            this.line8.LineStyle = GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash;
            this.line8.LineWeight = 2F;
            this.line8.Name = "line8";
            this.line8.Top = 0.9759842F;
            this.line8.Width = 8.897638F;
            this.line8.X1 = 9.055119F;
            this.line8.X2 = 0.1574803F;
            this.line8.Y1 = 0.9759842F;
            this.line8.Y2 = 0.9759842F;
            // 
            // line9
            // 
            this.line9.Height = 0F;
            this.line9.Left = 0.1574803F;
            this.line9.LineStyle = GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash;
            this.line9.LineWeight = 2F;
            this.line9.Name = "line9";
            this.line9.Top = 1.226181F;
            this.line9.Width = 8.897638F;
            this.line9.X1 = 9.055119F;
            this.line9.X2 = 0.1574803F;
            this.line9.Y1 = 1.226181F;
            this.line9.Y2 = 1.226181F;
            // 
            // line10
            // 
            this.line10.Height = 0F;
            this.line10.Left = 0.1574803F;
            this.line10.LineStyle = GrapeCity.ActiveReports.SectionReportModel.LineStyle.Dash;
            this.line10.LineWeight = 2F;
            this.line10.Name = "line10";
            this.line10.Top = 1.476378F;
            this.line10.Width = 8.897638F;
            this.line10.X1 = 9.055119F;
            this.line10.X2 = 0.1574803F;
            this.line10.Y1 = 1.476378F;
            this.line10.Y2 = 1.476378F;
            // 
            // groupHeader2
            // 
            this.groupHeader2.Controls.AddRange(new GrapeCity.ActiveReports.SectionReportModel.ARControl[] {
            this.crossSectionBox1,
            this.line25,
            this.crossSectionLine1,
            this.crossSectionLine3,
            this.crossSectionLine5,
            this.crossSectionLine6,
            this.label21,
            this.label22,
            this.label27,
            this.label28,
            this.label1,
            this.label3,
            this.label14,
            this.label2,
            this.label4,
            this.label5,
            this.crossSectionLine2,
            this.crossSectionLine4,
            this.label6,
            this.label7});
            this.groupHeader2.DataField = "RevisionNumber";
            this.groupHeader2.Height = 1.225F;
            this.groupHeader2.Name = "groupHeader2";
            this.groupHeader2.RepeatStyle = GrapeCity.ActiveReports.SectionReportModel.RepeatStyle.OnPage;
            // 
            // crossSectionBox1
            // 
            this.crossSectionBox1.Bottom = 0F;
            this.crossSectionBox1.Left = 0F;
            this.crossSectionBox1.LineWeight = 2F;
            this.crossSectionBox1.Name = "crossSectionBox1";
            this.crossSectionBox1.Right = 9.212599F;
            this.crossSectionBox1.Top = 0.8330001F;
            // 
            // line25
            // 
            this.line25.Height = 0F;
            this.line25.Left = 1.490116E-08F;
            this.line25.LineWeight = 2F;
            this.line25.Name = "line25";
            this.line25.Top = 1.222764F;
            this.line25.Width = 9.212599F;
            this.line25.X1 = 9.212599F;
            this.line25.X2 = 1.490116E-08F;
            this.line25.Y1 = 1.222764F;
            this.line25.Y2 = 1.222764F;
            // 
            // crossSectionLine1
            // 
            this.crossSectionLine1.Bottom = 0F;
            this.crossSectionLine1.Left = 0.2952756F;
            this.crossSectionLine1.LineWeight = 2F;
            this.crossSectionLine1.Name = "crossSectionLine1";
            this.crossSectionLine1.Top = 0.8330001F;
            // 
            // crossSectionLine3
            // 
            this.crossSectionLine3.Bottom = 0F;
            this.crossSectionLine3.Left = 3.543F;
            this.crossSectionLine3.LineWeight = 2F;
            this.crossSectionLine3.Name = "crossSectionLine3";
            this.crossSectionLine3.Top = 0.8330001F;
            // 
            // crossSectionLine5
            // 
            this.crossSectionLine5.Bottom = 0F;
            this.crossSectionLine5.Left = 6.034F;
            this.crossSectionLine5.LineWeight = 2F;
            this.crossSectionLine5.Name = "crossSectionLine5";
            this.crossSectionLine5.Top = 0.8330001F;
            // 
            // crossSectionLine6
            // 
            this.crossSectionLine6.Bottom = 0F;
            this.crossSectionLine6.Left = 5.368F;
            this.crossSectionLine6.LineWeight = 2F;
            this.crossSectionLine6.Name = "crossSectionLine6";
            this.crossSectionLine6.Top = 0.8330001F;
            // 
            // label21
            // 
            this.label21.Height = 0.1968505F;
            this.label21.HyperLink = null;
            this.label21.Left = 1.958F;
            this.label21.Name = "label21";
            this.label21.Style = "text-align: center";
            this.label21.Text = "品名";
            this.label21.Top = 0.9480001F;
            this.label21.Width = 1.416929F;
            // 
            // label22
            // 
            this.label22.Height = 0.1968505F;
            this.label22.HyperLink = null;
            this.label22.Left = 4.149F;
            this.label22.Name = "label22";
            this.label22.Style = "text-align: center";
            this.label22.Text = "コード№";
            this.label22.Top = 0.9480001F;
            this.label22.Width = 0.7838584F;
            // 
            // label27
            // 
            this.label27.Height = 0.1968505F;
            this.label27.HyperLink = null;
            this.label27.Left = 5.431F;
            this.label27.Name = "label27";
            this.label27.Style = "text-align: center";
            this.label27.Text = "個数";
            this.label27.Top = 0.9480001F;
            this.label27.Width = 0.5511813F;
            // 
            // label28
            // 
            this.label28.Height = 0.1968505F;
            this.label28.HyperLink = null;
            this.label28.Left = 7.951F;
            this.label28.Name = "label28";
            this.label28.Style = "text-align: center";
            this.label28.Text = "備考";
            this.label28.Top = 0.9480001F;
            this.label28.Width = 0.6692915F;
            // 
            // label1
            // 
            this.label1.Height = 0.3456693F;
            this.label1.HyperLink = null;
            this.label1.Left = 1.743F;
            this.label1.Name = "label1";
            this.label1.Style = "font-size: 21.75pt; text-align: center; text-decoration: underline";
            this.label1.Text = "部品用品発注書 兼 部品マスタ登録依頼書";
            this.label1.Top = 0F;
            this.label1.Width = 5.583F;
            // 
            // label3
            // 
            this.label3.Height = 0.3456693F;
            this.label3.HyperLink = null;
            this.label3.Left = 2.48F;
            this.label3.Name = "label3";
            this.label3.Style = "font-size: 15.75pt; font-weight: bold; text-align: center; text-decoration: none";
            this.label3.Text = "下記部品の登録・注文をお願い致します";
            this.label3.Top = 0.408F;
            this.label3.Width = 3.901496F;
            // 
            // label14
            // 
            this.label14.Height = 0.2208661F;
            this.label14.HyperLink = null;
            this.label14.Left = 6.83F;
            this.label14.Name = "label14";
            this.label14.Style = "text-align: left";
            this.label14.Text = "担当：";
            this.label14.Top = 0.5330001F;
            this.label14.Width = 0.6692915F;
            // 
            // label2
            // 
            this.label2.DataField = "EmployeeName";
            this.label2.Height = 0.2208661F;
            this.label2.HyperLink = null;
            this.label2.Left = 7.56F;
            this.label2.Name = "label2";
            this.label2.Style = "text-align: left";
            this.label2.Text = "";
            this.label2.Top = 0.5330001F;
            this.label2.Width = 1.495F;
            // 
            // label4
            // 
            this.label4.Height = 0.2208661F;
            this.label4.HyperLink = null;
            this.label4.Left = 0.08800001F;
            this.label4.Name = "label4";
            this.label4.Style = "text-align: left";
            this.label4.Text = "伝票番号：";
            this.label4.Top = 0.5330001F;
            this.label4.Width = 0.6692915F;
            // 
            // label5
            // 
            this.label5.DataField = "=SlipNumber + \"_\" + RevisionNumber";
            this.label5.Height = 0.220866F;
            this.label5.HyperLink = null;
            this.label5.Left = 0.8180006F;
            this.label5.Name = "label5";
            this.label5.Style = "text-align: left";
            this.label5.Text = "";
            this.label5.Top = 0.5330001F;
            this.label5.Width = 1.495F;
            // 
            // crossSectionLine2
            // 
            this.crossSectionLine2.Bottom = 0F;
            this.crossSectionLine2.Left = 7.326F;
            this.crossSectionLine2.LineWeight = 2F;
            this.crossSectionLine2.Name = "crossSectionLine2";
            this.crossSectionLine2.Top = 0.8330001F;
            // 
            // crossSectionLine4
            // 
            this.crossSectionLine4.Bottom = 0F;
            this.crossSectionLine4.Left = 1.858F;
            this.crossSectionLine4.LineWeight = 2F;
            this.crossSectionLine4.Name = "crossSectionLine4";
            this.crossSectionLine4.Top = 0.8330001F;
            // 
            // label6
            // 
            this.label6.Height = 0.1968505F;
            this.label6.HyperLink = null;
            this.label6.Left = 6.381001F;
            this.label6.Name = "label6";
            this.label6.Style = "text-align: center";
            this.label6.Text = "仕入先";
            this.label6.Top = 0.9480001F;
            this.label6.Width = 0.6692915F;
            // 
            // label7
            // 
            this.label7.Height = 0.1968505F;
            this.label7.HyperLink = null;
            this.label7.Left = 0.378F;
            this.label7.Name = "label7";
            this.label7.Style = "text-align: center";
            this.label7.Text = "メーカー";
            this.label7.Top = 0.9480001F;
            this.label7.Width = 1.416929F;
            // 
            // groupFooter2
            // 
            this.groupFooter2.Height = 0F;
            this.groupFooter2.Name = "groupFooter2";
            // 
            // PartsPurchaseOrderReport
            // 
            this.MasterReport = false;
            this.PageSettings.DefaultPaperSize = false;
            this.PageSettings.Margins.Bottom = 0.7874016F;
            this.PageSettings.Margins.Left = 1.181102F;
            this.PageSettings.Margins.Right = 1.181102F;
            this.PageSettings.Margins.Top = 0.7874016F;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;
            this.PageSettings.PaperHeight = 11.69291F;
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.PaperWidth = 8.267716F;
            this.PrintWidth = 9.232284F;
            this.Sections.Add(this.pageHeader);
            this.Sections.Add(this.groupHeader2);
            this.Sections.Add(this.detail);
            this.Sections.Add(this.groupFooter2);
            this.Sections.Add(this.pageFooter);
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-style: normal; text-decoration: none; font-weight: normal; font-size: 10pt; " +
                        "color: Black; font-family: MS UI Gothic; ddo-char-set: 128", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 16pt; font-weight: bold", "Heading1", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 14pt; font-weight: bold", "Heading2", "Normal"));
            this.StyleSheet.Add(new DDCssLib.StyleSheetRule("font-size: 13pt; font-weight: bold", "Heading3", "Normal"));
            this.ReportStart += new System.EventHandler(this.PartsPurchaseOrderReport_ReportStart);
            ((System.ComponentModel.ISupportInitialize)(this.reportInfo1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLineNumber1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuantity1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtc_WorkType_Name1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label27)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label28)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.label7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion




































        private GrapeCity.ActiveReports.SectionReportModel.PageHeader pageHeader;
        private GrapeCity.ActiveReports.SectionReportModel.Detail detail;
        private GrapeCity.ActiveReports.SectionReportModel.PageFooter pageFooter;
        private GrapeCity.ActiveReports.SectionReportModel.ReportInfo reportInfo1;
        private GrapeCity.ActiveReports.SectionReportModel.Line line26;
        private GrapeCity.ActiveReports.SectionReportModel.Shape shape3;
        private GrapeCity.ActiveReports.SectionReportModel.Label label12;
        private GrapeCity.ActiveReports.SectionReportModel.Line line6;
        private GrapeCity.ActiveReports.SectionReportModel.Line line7;
        private GrapeCity.ActiveReports.SectionReportModel.Line line8;
        private GrapeCity.ActiveReports.SectionReportModel.Line line9;
        private GrapeCity.ActiveReports.SectionReportModel.Line line10;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtLineNumber1;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtQuantity1;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox txtc_WorkType_Name1;
        private GrapeCity.ActiveReports.SectionReportModel.TextBox textBox1;
        private GrapeCity.ActiveReports.SectionReportModel.GroupHeader groupHeader2;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionBox crossSectionBox1;
        private GrapeCity.ActiveReports.SectionReportModel.Line line25;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine1;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine3;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine5;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine6;
        private GrapeCity.ActiveReports.SectionReportModel.Label label21;
        private GrapeCity.ActiveReports.SectionReportModel.Label label22;
        private GrapeCity.ActiveReports.SectionReportModel.Label label27;
        private GrapeCity.ActiveReports.SectionReportModel.Label label28;
        private GrapeCity.ActiveReports.SectionReportModel.Label label1;
        private GrapeCity.ActiveReports.SectionReportModel.Label label3;
        private GrapeCity.ActiveReports.SectionReportModel.Label label14;
        private GrapeCity.ActiveReports.SectionReportModel.Label label2;
        private GrapeCity.ActiveReports.SectionReportModel.Label label4;
        private GrapeCity.ActiveReports.SectionReportModel.Label label5;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine2;
        private GrapeCity.ActiveReports.SectionReportModel.CrossSectionLine crossSectionLine4;
        private GrapeCity.ActiveReports.SectionReportModel.Label label6;
        private GrapeCity.ActiveReports.SectionReportModel.Label label7;
        private GrapeCity.ActiveReports.SectionReportModel.GroupFooter groupFooter2;
    }
}
