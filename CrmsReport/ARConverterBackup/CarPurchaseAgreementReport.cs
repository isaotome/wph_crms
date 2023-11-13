using GrapeCity.ActiveReports.SectionReportModel;
using System;
using CrmsDao;

namespace CrmsReport
{
  /// <summary>
  /// CarPurchaseAgreementReport の概要の説明です。
  /// </summary>
  public partial class CarPurchaseAgreementReport : GrapeCity.ActiveReports.SectionReport
    {
        private string path;                    //Add 2018/12/21 yano #3965

        public CarPurchaseAgreementReport(string filepath)
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();

            path = (!string.IsNullOrWhiteSpace(filepath) ? filepath : "");               //Add 2018/12/21 yano #3965 
        }


          /// <summary>
        /// データフェッチ処理
        /// </summary>
        /// <history>
        /// 2023/10/31 yano #4186 車輛買取契約書の消費税額計算の誤り対応
        /// 2023/09/05 yano #4162 インボイス対応
        /// </history>
        private void CarPurchaseAgreementReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            object depositKind = Fields["DepositKind"].Value;
            if (depositKind != null)
            {
                switch (depositKind.ToString())
                {
                    case "1":
                        shapeFutsu.Visible = true;
                        break;
                    case "2":
                        shapeToza.Visible = true;
                        break;
                }
            }

            object cd = Fields["Cd"].Value;
            if (cd != null)
            {
                switch (cd.ToString())
                {
                    case "001":
                        shapeAudioJun.Visible = true;
                        break;
                    case "002":
                        shapeAudioGai.Visible = true;
                        break;
                }
            }

            object aw = Fields["Aw"].Value;
            if (aw != null)
            {
                switch (aw.ToString())
                {
                    case "001":
                        shapeAwJun.Visible = true;
                        break;
                    case "002":
                        shapeAwGai.Visible = true;
                        break;
                }
            }

            object navi = Fields["NaviType"].Value;
            if (navi != null)
            {
                switch (navi.ToString())
                {
                    case "001":
                        shapeNaviJun.Visible = true;
                        break;
                    case "002":
                        shapeNaviGai.Visible = true;
                        break;
                }
            }

            
            //Add 2023/09/05
            //-------------------------------
            // 適格請求書発行用処理
            //---------------------------------
            decimal ? appraisalprice = 0m;
             try { appraisalprice = (decimal)Fields["AppraisalPrice"].Value; }
             catch(InvalidCastException){ }
             
             decimal ? recycleDeposit = 0m;
             try { recycleDeposit = (decimal)Fields["RecycleDeposit"].Value; }
             catch(InvalidCastException){ }

             int ? rate = 0;
             try { rate = int.Parse(Fields["Rate"].Value.ToString());}
             catch(InvalidCastException){ }

             //消費税率
             this.txtRate.Value = rate;

             //Mod 2023/10/31 yano #4186
             decimal d1 = (decimal)(1.0m * rate);

             decimal d2 = (decimal)(100m + rate);

             decimal r = d1 / d2;

             r = Math.Round(r, 17, MidpointRounding.AwayFromZero);

             //decimal r = Math.Round((decimal)(1.0 * rate /(100 + rate)), 28);

             decimal? totaltaxamount = 0m;

             decimal ? totalamount = (appraisalprice ?? 0m) - (recycleDeposit ?? 0m);

             if ((totalamount ?? 0m) > 0)
             {
                totaltaxamount = Math.Floor((totalamount ?? 0m) * r);
             }
             else
             {
                totaltaxamount = Math.Ceiling((totalamount ?? 0m) * r);
             }

             //課税合計
             this.txtTotalAmount.Value = String.Format("{0:#,0}", totalamount);

             //非課税合計
             this.txtNonTaxableTotalAmount.Value =  String.Format("{0:#,0}", recycleDeposit);

             //消費税
             this.txtTotalTaxAmount.Value = String.Format("{0:#,0}", totaltaxamount);
        }

        private void CarPurchaseAgreementReport_DataInitialize(object sender, EventArgs e)
        {
            this.txtHikaeName.Text = this.HikaeName;
        }

        /// <summary>
        /// CarPurchaseAgreementReport レポート開始
        /// </summary>
        /// <history>
        /// 2023/09/05 #4162 インボイス対応
        /// </history>
        private void CarPurchaseAgreementReport_ReportStart(object sender, EventArgs e)
        {

            //Add 2023/04/06 yano #4162
            //--------------------------------------
            // 適格請求書発行事業者登録番号の取得
            //-------------------------------------
            CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();

            var query = new ConfigurationSettingDao(db).GetByKey("InvoiceProviderNumber");

            this.txtInvoiceProviderNumber.Text = query != null ? ( query.Value) : "";


            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;

            //Add 2018/12/21 yano #3965
            if (!string.IsNullOrWhiteSpace(path))
            {
                //Mod 2023/06/09 yano #4167 サイズ調整
                this.picture1.Width *= 1.1F;
                this.picture1.Height *= 1.1F;
                this.picture1.SizeMode = SizeModes.Zoom;

                System.IO.FileStream fs = new System.IO.FileStream(
                path,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
                this.picture1.Image = System.Drawing.Image.FromStream(fs);
                fs.Close();
            }

        }
    }
}
