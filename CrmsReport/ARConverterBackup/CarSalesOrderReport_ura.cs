using System;
using CrmsDao;                                    //Add 2023/04/06 yano #4162
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
    /// NewActiveReport6_ura の概要の説明です。
    /// </summary>
    public partial class CarSalesOrderReport_ura : GrapeCity.ActiveReports.SectionReport
    {

        public CarSalesOrderReport_ura()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private void CarSalesOrderReport_ura_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A3/Landscape).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A3;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;

        }

        /// <summary>
        /// 車両注文書データfetch処理
        /// </summary>
        /// <history>
        /// 2023/06/09 yano #4168 社名変更に伴う変更（社名をマスタから取得するように修正する。 
        /// 2020/03/18 yano #4044 処理不要のため、コメントアウト
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 新規作成
        /// </history>
        private void CarSalesOrderReport_ura_Fetch(object sender, EventArgs e)
        {
            CrmsLinqDataContext db = new CrmsLinqDataContext();

            //Mod 2023/06/09 yano #4168   
            Company rec = new CompanyDao(db).GetByKey("001");

            //会社名
            string companyname = rec != null ? rec.CompanyName : "";

            //label5の文言の中の会社名を会社マスタから取得した会社名に変更する
            this.label5.Text = "販売会社が" + companyname + "の場合、株式会社ウイルプラスホールディングスの子会社の範囲において、下記の通り個人情報を共同利用します。\r\n";
            this.label5.Text += "1.共同利用する目的：①の利用目的\r\n";
            this.label5.Text += "2.共同利用する個人情報：表記記載事項、車両登録情報、取扱情報及び修理・整備入庫履歴\r\n";
            this.label5.Text += "3.共同利用する個人情報の管理については、" + companyname + "がその責任を有します。";


            //Del 2023/06/09 yano コメントアウト部分を削除
            //Mod #4044
            ////Add 2019/09/04 yano #4011
           
        }
    }
}
