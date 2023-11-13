using CrmsDao;
using System;
using System.Configuration;
namespace CrmsReport
{
    /// <summary>
    /// NewActiveReport4 の概要の説明です。
    /// </summary>
    public partial class CarSalesOrderReport : GrapeCity.ActiveReports.SectionReport
    {
        public CarSalesOrderReport()
        {
            InitializeComponent();
        }

        //datacontext
        private CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();   //Add 2023/09/05 #4162

        private void detail_Format(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CrmsDao.Properties.Settings.CRMSConnectionString"].ToString();
            string optionSql = "select * from CarSalesLine where SlipNumber='" + this.SlipNumber + "' and RevisionNumber=" + this.RevisionNumber;

            subRep_CarSalesOrder rpt = new subRep_CarSalesOrder();
            rpt.DataSource = new GrapeCity.ActiveReports.Data.SqlDBDataSource(connectionString, optionSql, 30);
            this.ctlSubReport.Report = rpt;

            if (!this.VisibleCustomer)
            {
                this.txtCustomerAddress.Visible = false;
                this.txtCustomerName.Visible = false;
                this.txtCustomerNameKana.Visible = false;
                this.txtCustomerPostCode.Visible = false;
                this.txtCustomerAddress.Visible = false;
                this.txtCustomerFaxNumber.Visible = false;
                this.txtCustomerMailAddress.Visible = false;
                this.txtCustomerMobileNumber.Visible = false;
                this.txtCustomerTelNumber.Visible = false;
                this.txtBirthDay.Visible = false;
                //add 2015/03/13 arc iijima 勤務先名項目追加
                this.txtWorkingCompanyName.Visible = false;
                this.txtWorkingCompanyAddress.Visible = false;
                this.txtWorkingCompanyTelNumber.Visible = false;
                this.txtPositionName.Visible = false;
            }
            else
            {
                string sex = "";
                try { sex = Fields["Sex"].Value.ToString(); }
                catch (NullReferenceException) { }
                switch (sex)
                {
                    case "001":
                        circleMan.Visible = true;
                        break;
                    case "002":
                        circleWoman.Visible = true;
                        break;
                    default:
                        break;
                }
            }

            string customerCode = "";
            try { customerCode = Fields["CustomerCode"].Value.ToString(); }
            catch { }
            string userCode = "";
            try { userCode = Fields["UserCode"].Value.ToString(); }
            catch { }

            if (!string.IsNullOrEmpty(customerCode) && !string.IsNullOrEmpty(userCode))
            {
                if (customerCode.Equals(userCode))
                {
                    // 顧客=使用者
                    this.circleOnaji.Visible = true;
                    this.txtMeigiName.Visible = false;
                    this.txtMeigiPostCode.Visible = false;
                    this.txtMeigiAddress.Visible = false;
                }
                else
                {
                    // 顧客≠使用者
                    this.circleKotonaru.Visible = true;
                }
            }
            else
            {
                this.txtMeigiName.Visible = false;
                this.txtMeigiPostCode.Visible = false;
                this.txtMeigiAddress.Visible = false;
            }

        }

        /// <summary>
        /// 車両注文書データfetch処理
        /// </summary>
        /// <history>
        /// 2023/10/31 yano #4186 車輛買取契約書の消費税額計算の誤り対応
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 新規作成
        /// </history>
        private void CarSalesOrderReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            decimal? discountAmount = (decimal)Fields["DiscountAmount"].Value;
            if (discountAmount == null || discountAmount == 0)
            {
                this.lblDiscountAmount.Visible = false;
                this.txtDiscountAmount.Visible = false;

            }
            this.txtReportName.Text = this.ReportName;
            this.txtFurikomiTesuryo.Visible = this.FurikomiTesuryoIsVisible;

            string printFlag = Fields["PrintFlag"].Value.ToString();
            if (!printFlag.Equals("1"))
            {
                this.txtAccountInformation.Visible = false;
                this.txtAccountName.Visible = false;
            }
            string customerName = "";
            string customerType = "";
            try { customerType = Fields["CustomerType"].Value.ToString(); }
            catch { }

            if (customerType.Equals("002"))
            {
                customerName = Fields["FirstName"].Value + "\r\n" + Fields["LastName"].Value;
            }
            else
            {
                customerName = Fields["CustomerName"].Value != null ? Fields["CustomerName"].Value.ToString() : "";
            }
            this.txtCustomerName.Text = customerName;

            //Add 2019/09/04 yano #4011
            DateTime judgedate = new DateTime(2019, 10, 1);

            DateTime RequestRegistDate = new DateTime(2999, 1, 1);

            if (!string.IsNullOrWhiteSpace(Fields["RequestRegistDate"].Value.ToString()))
            {
                DateTime.TryParse(Fields["RequestRegistDate"].Value.ToString(), out RequestRegistDate);
            }

            //登録希望日が10/1以降の場合は文言変更
            if (RequestRegistDate >= judgedate)
            {
                this.textBox21.Text = "自動車税種別割（";
                this.textBox27.Text = "自動車税環境性能割";
                this.textBox106.Text = "未払自動車税種別割△";
                this.textBox145.Text = "下取自動車税種別割預り金";
            }

            //Add 2021/08/02 yano #4097
            //新中区分で年式の参照元データを変更
            string newusedtype = "";
            try { newusedtype = Fields["NewUsedTypeCode"].Value.ToString(); }
            catch (InvalidCastException) { }

            //新車の場合…グレードマスタの年式
            if (newusedtype.Equals("N"))
            {
                try { this.txtManufacturingYear.Text = Fields["ModelYear"].Value.ToString(); }
                catch (InvalidCastException) { }
            }
            else//中古車の場合…車両マスタの年式
            {
                try { this.txtManufacturingYear.Text = Fields["ManufacturingYear"].Value.ToString(); }
                catch (InvalidCastException) { }
            }


            //Add 2023/09/05  #4162
            //--------------------------
            //適格請求書関連
            //--------------------------
            //------------------------
            //販売車両関連
            //------------------------
            int? rate = 0;
            try { rate = int.Parse(Fields["Rate"].Value.ToString()); }
            catch (InvalidCastException) { }

            string slipNumber = "";
            try { slipNumber = Fields["SlipNumber"].Value.ToString(); }
            catch (InvalidCastException) { }

            //一般消費税取得
            string customerclaimCode = "";
            try { customerclaimCode = Fields["CustomerCode"].Value.ToString(); }
            catch (InvalidCastException) { }

            decimal? invoicetaxamount = 0m;

            if (!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerclaimCode))
            {
                invoicetaxamount = new InvoiceConsumptionTaxDao(db).GetByKey(slipNumber, customerclaimCode, rate ?? 0) != null ? new InvoiceConsumptionTaxDao(db).GetByKey(slipNumber, customerclaimCode, rate ?? 0).InvoiceConsumptionTaxAmount : 0;
            }

            this.txtTotalTaxAmount.Value = invoicetaxamount;


            //-------------------------------
            // 下取車関連
            //---------------------------------
            decimal? tradeinamount = 0m;
            try { tradeinamount = (decimal)Fields["TradeInAmount"].Value; }
            catch (InvalidCastException) { }

            decimal? tradeinunexpiredcartax = 0m;
            try { tradeinunexpiredcartax = (decimal)Fields["TradeInUnexpiredCarTax"].Value; }
            catch (InvalidCastException) { }

            decimal? tradeinrecycleamount = 0m;
            try { tradeinrecycleamount = (decimal)Fields["TradeInRecycleAmount"].Value; }
            catch (InvalidCastException) { }

            //消費税率
            this.txtRate.Value = rate;

            //Mod 2023/10/31 yano #4186
            decimal d1 = (decimal)(1.0m * rate);

            decimal d2 = (decimal)(100m + rate);

            decimal r = d1 / d2;

            r = Math.Round(r, 17, MidpointRounding.AwayFromZero);

            //decimal r = Math.Round((decimal)(1.0 * rate / (100 + rate)), 17);

            decimal? totaltaxamount = 0m;

            decimal? totalamount = (tradeinamount ?? 0m) - (tradeinunexpiredcartax ?? 0m) - (tradeinrecycleamount ?? 0m);

            if ((totalamount ?? 0m) > 0)
            {
                totaltaxamount = Math.Floor((totalamount ?? 0m) * r);
            }
            else
            {
                totaltaxamount = Math.Ceiling((totalamount ?? 0m) * r);
            }

            //課税合計
            this.txtTradeInTaxrableTotalAmount.Value = String.Format("{0:#,0}", totalamount);

            //非課税合計
            this.txtTradeInNonTaxableTotalAmount.Value = String.Format("{0:#,0}", (tradeinunexpiredcartax ?? 0m) + (tradeinrecycleamount ?? 0m));

            //消費税
            this.txtTradeInTotalTaxAmount.Value = String.Format("{0:#,0}", totaltaxamount);

        }

        private void CarSalesOrderReport_ReportStart(object sender, EventArgs e)
        {
            //Add 2023/04/06 yano #4162
            CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();

            var query = new ConfigurationSettingDao(db).GetByKey("InvoiceProviderNumber");

            this.txtInvoiceProviderNumber.Text = query != null ? (query.Value) : "";


            // Setup Virtual Printer and Paper Size(A3/Landscape).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A3;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Landscape;

        }
    }
}
