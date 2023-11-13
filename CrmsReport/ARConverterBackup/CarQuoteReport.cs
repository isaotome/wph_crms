using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Controls;
using GrapeCity.ActiveReports.SectionReportModel;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.Document;

namespace CrmsReport
{
    /// <summary>
    /// 車両見積書
    /// </summary>
    public partial class CarQuoteReport : GrapeCity.ActiveReports.SectionReport
    {

        public CarQuoteReport()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();

        }

        private void NewActiveReport2_ReportStart(object sender, EventArgs e)
        {
            // 帳票サイズ設定。A4/Landscape
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;
        }

        private void detail_Format(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CrmsDao.Properties.Settings.CRMSConnectionString"].ToString();
            string optionSql = "select * from CarSalesLine where SlipNumber='" + this.SlipNumber + "' and RevisionNumber=" + this.RevisionNumber;
            string tradeInSql = "select * from V_CarTradeIn where TradeInMakerName1 is not null and TradeInCarName1 is not null and TradeInCarName1<>'' and SlipNumber='" + this.SlipNumber + "' and RevisionNumber=" + this.RevisionNumber + " order by Id";
            string loanSql = "select * from V_CarLoan where LoanCodeA is not null and LoanCodeA<>'' and SlipNumber='" + this.SlipNumber + "' and RevisionNumber=" + this.RevisionNumber + " order by Id";
            subRep rpt = new subRep();
            rpt.DataSource = new GrapeCity.ActiveReports.Data.SqlDBDataSource(connectionString, optionSql, 30);
            this.ctlSubReport.Report = rpt;
            subRep_shitadori rpt2 = new subRep_shitadori();
            rpt2.DataSource = new GrapeCity.ActiveReports.Data.SqlDBDataSource(connectionString, tradeInSql, 30);
            this.ctlSubReport2.Report = rpt2;
        }

        /// <summary>
        /// 車両見積データfetch処理
        /// </summary>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// </history>
        private void CarQuoteReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            decimal? discountAmount = 0m;
            try { discountAmount = (decimal)Fields["DiscountAmount"].Value; }
            catch (InvalidCastException) { }
            if (discountAmount == null || discountAmount == 0)
            {
                this.lblDiscountAmount.Visible = false;
                this.txtDiscountAmount.Visible = false;
            }

            DateTime judgedate = new DateTime(2019, 10, 1);

            DateTime RequestRegistDate = new DateTime(2999, 1, 1);

            if(!string.IsNullOrWhiteSpace(Fields["RequestRegistDate"].Value.ToString()))
            {
                DateTime.TryParse(Fields["RequestRegistDate"].Value.ToString(), out RequestRegistDate);
            }

            //登録希望日が10/1以降の場合は文言変更
            if (RequestRegistDate >= judgedate)
            {
                this.a03_label12.Text = "自動車税環境性能割";
                this.a03_label7.Text = "自動車税種別割";
                this.a06_label6.Text = "未払自動車税種別割△";
                this.label32.Text = "下取自動車税種別割預り金";
            }

            //Add 2021/08/02 yano #4097
            //新中区分で年式の参照元データを変更
            string newusedtype = "";
            try { newusedtype = Fields["NewUsedTypeCode"].Value.ToString(); }
            catch (InvalidCastException) { }

            //新車の場合…グレードマスタの年式
            if (newusedtype.Equals("N"))
            {
                try { this.txtModelYear1.Text = Fields["ModelYear"].Value.ToString(); }
                catch (InvalidCastException) { }
            }
            else//中古車の場合…車両マスタの年式
            {
                try { this.txtModelYear1.Text = Fields["ManufacturingYear"].Value.ToString(); }
                catch (InvalidCastException) { }
            }
        }
    }
}
