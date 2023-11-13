using CrmsDao;
using GrapeCity.ActiveReports.SectionReportModel;
using System;

namespace CrmsReport
{
  /// <summary>
  /// NewActiveReport7_3 の概要の説明です。
  /// </summary>
  public partial class ServiceClaimDetailReport : GrapeCity.ActiveReports.SectionReport
    {
        public ServiceClaimDetailReport()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        private CrmsDao.CrmsLinqDataContext db = new CrmsLinqDataContext();   //Add 2023/09/05 #4162
        
        private int RowNumber;
        private void detail_Format(object sender, EventArgs e)
        {
            RowNumber++;
            if (RowNumber < 22)
            {
                // 件数が24件に満たない場合、改ページは行いません。
                this.detail.NewPage = NewPage.None;
                this.detail.RepeatToFill = true;
            }
            else
            {
                // 24件出力した後、改ページを行い、カウンタをリセットします。
                this.detail.NewPage = NewPage.After;
                this.detail.RepeatToFill = false;
                RowNumber = 0;
            }
        }

        /// <summary>
        /// 明細請求書データ設定
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// </history>
        /// <param name="sender"></param>
        /// <param name="eArgs"></param>
        private void ServiceClaimDetailReport_FetchData(object sender, FetchEventArgs eArgs)
        {
            decimal? carWeightTax = null;
            try { carWeightTax = (decimal)Fields["CarWeightTax"].Value; }
            catch (InvalidCastException) { }
            decimal? carLiabilityInsurance = null;
            try { carLiabilityInsurance = (decimal)Fields["CarLiabilityInsurance"].Value; }
            catch (InvalidCastException) { }
            decimal? fiscalStampCost = null;
            try { fiscalStampCost = (decimal)Fields["FiscalStampCost"].Value; }
            catch (InvalidCastException) { }
            decimal? costTotalAmount = null;
            try { costTotalAmount = (decimal)Fields["CostTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? depositTotalAmount = null;
            try { depositTotalAmount = (decimal)Fields["DepositTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? quantity = null;
            try { quantity = (decimal)Fields["Quantity"].Value; }
            catch (InvalidCastException) { }
            decimal? mileage = null;
            try { mileage = (decimal)Fields["Mileage"].Value; }
            catch (InvalidCastException) { }
            decimal? carTax = null;
            try { carTax = (decimal)Fields["CarTax"].Value; }
            catch (InvalidCastException) { }
            decimal? numberPlateCost = null;
            try { numberPlateCost = (decimal)Fields["NumberPlateCost"].Value; }
            catch (InvalidCastException) { }
            decimal? taxFreeFieldValue = null;
            try { taxFreeFieldValue = (decimal)Fields["TaxFreeFieldValue"].Value; }
            catch (InvalidCastException) { }

            //Add 2015/05/29 arc nakayama #3210 サブシステムの「サービス」⇒「ワランティ納品書」のメニューの移植
            decimal? EngineerTotalAmount = null;
            try { EngineerTotalAmount = (decimal)Fields["EngineerTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? PartsTotalAmount = null;
            try { PartsTotalAmount = (decimal)Fields["PartsTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? TaxTotalAmount = null;
            try { TaxTotalAmount = (decimal)Fields["TaxTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? CostTotalAmount = null;
            try { CostTotalAmount = (decimal)Fields["CostTotalAmount"].Value; }
            catch (InvalidCastException) { }
            decimal? DepositTotalAmount = null;
            try { DepositTotalAmount = (decimal)Fields["DepositTotalAmount"].Value; }
            catch (InvalidCastException) { }
            //Mod 2023/09/05  #4162
            //int? Rate = null;
            //try { Rate = (int)Fields["Rate"].Value; }
            //catch (InvalidCastException) { }
            //Add 2015/07/15 arc nakayama 請求明細書の自由欄の項目に値を入れると表示がおかしくなるバグ修正
            string TaxFreeFieldName = "";
            try { TaxFreeFieldName = (string)Fields["TaxFreeFieldName"].Value; }
            catch (InvalidCastException) { }
            int? TaxFreeFieldValue = null;
            try { TaxFreeFieldValue = (int)Fields["TaxFreeFieldValue"].Value; }
            catch (InvalidCastException) { }

            //Add 2020/02/17 yano #4025-------------------------------------------------
            decimal? optionalInsurance = null;
            try { optionalInsurance = (decimal)Fields["OptionalInsurance"].Value; }
            catch (InvalidCastException) { }

            decimal? taxableCostTotalAmount = null;
            try { taxableCostTotalAmount = (decimal)Fields["TaxableCostTotalAmount"].Value; }
            catch (InvalidCastException) { }

            string subscriptionFeeMemo = null;
            try { subscriptionFeeMemo = (string)Fields["SubscriptionFeeMemo"].Value; }
            catch (InvalidCastException) { }

            int ? rate = null;
            try { rate = int.Parse(Fields["Rate"].Value.ToString()); }    //Mod 2023/09/05  #4162
            catch (InvalidCastException) { }

            decimal? subscriptionFee = null;
            try { subscriptionFee = (decimal)Fields["SubscriptionFee"].Value; }
            catch (InvalidCastException) { }

            decimal? taxableFreeFieldValue = null;
            try { taxableFreeFieldValue = (decimal)Fields["TaxableFreeFieldValue"].Value; }
            catch (InvalidCastException) { }
            //--------------------------------------------------------------------------
            
            //Add 2020/03/27 yano #4046
            //-------------------------------------------------------------------------
            int? warrantyFlag = null;
            try { warrantyFlag = (int)Fields["WarrantyFlag"].Value; }
            catch (InvalidCastException) { }
            int? reporttype = null;
            try { reporttype = (int)Fields["REPORT_TYPE"].Value; }
            catch (InvalidCastException) { }
            string taxableFreeFieldName = "";
            try { taxableFreeFieldName = (string)Fields["TaxableFreeFieldName"].Value; }
            catch (InvalidCastException) { }
            //--------------------------------------------------------------------------

            //Mod 2023/09/05 yano  #4162
            //走行距離
            if (mileage == null)
            {
              this.txtMileageUnit.Visible = false;
            }
            //数量
            if (quantity != null && ((int)quantity) == quantity.Value)
            {
              txtQuantity1.OutputFormat = "#,##0";
            }
            else
            {
              txtQuantity1.OutputFormat = "#,##0.000";
            }
            //預かり金
            if (depositTotalAmount == null || depositTotalAmount == 0)
            {
              this.lblDepositTotalAmount.Visible = false;
              this.txtDepositTotalAmount.Visible = false;
            }
            //----------------------------------
            // 諸費用（非課税）
            //----------------------------------
            //自動税
            if (carTax == null || carTax == 0)
            {
              this.lblCarTax.Visible = false;
              this.txtCarTaxAmount.Visible = false;
            }
            //自動車重量税
            if (carWeightTax == null || carWeightTax == 0)
            {
              this.lblCarWeightTax.Visible = false;
              this.txtCarWeightTax.Visible = false;
            }
            //自賠責保険料
            if (carLiabilityInsurance == null || carLiabilityInsurance == 0)
            {
              this.lblCarLiabilityInsurance.Visible = false;
              this.txtCarLiabilityInsurance.Visible = false;
            }
            //ナンバー代
            if (numberPlateCost == null || numberPlateCost == 0)
            {
              this.lblNumberPlateCost.Visible = false;
              this.txtNumberPlateCost.Visible = false;
            }
            //各種印紙代
            if (fiscalStampCost == null || fiscalStampCost == 0)
            {
              this.lblFiscalStampCost.Visible = false;
              this.txtFiscalStampCost.Visible = false;
            }
            //Add 2020/02/17 yano #4025-------------------------------------------------
            //任意保険
            if (optionalInsurance == null || optionalInsurance == 0)
            {
              this.lblOptionalInsurance.Visible = false;
              this.txtOptionalInsurance.Visible = false;
            }
            //その他（非課税）
            if (taxFreeFieldValue == null || taxFreeFieldValue == 0)
            {
              this.lblTaxFreeFieldName.Visible = false;
              this.txtTaxFreeFieldValue.Visible = false;
            }
            //諸費用(非課税)合計
            //if (costTotalAmount == null || costTotalAmount == 0)
            //{
            //    this.lblCostTotalAmount.Visible = false;
            //    this.txtCostTotalAmount.Visible = false;
            //}
            //----------------------------------
            // 諸費用（課税）
            //----------------------------------
            //Mod 2023/09/05 yano 
            //サービス加入料
            if (subscriptionFee == null || subscriptionFee == 0)
            {
                this.lblSubscriptionFee.Visible = false;
                this.txtSubscriptionFee.Visible = false;

                if (taxableFreeFieldValue != null && taxableFreeFieldValue != 0)
                {
                    System.Drawing.PointF p = new System.Drawing.PointF();
                    p.X = this.lblSubscriptionFee.Location.X;
                    p.Y = this.lblSubscriptionFee.Location.Y;

                    this.lblTaxableFreeFieldName.Location = p;

                    System.Drawing.PointF q = new System.Drawing.PointF();
                    q.X = this.txtSubscriptionFee.Location.X;
                    q.Y = this.txtSubscriptionFee.Location.Y;

                    this.txtTaxableFreeFieldValue.Location = q;
                }
            }
            //その他（課税）
            if (taxableFreeFieldValue == null || taxableFreeFieldValue == 0)
            {
                this.lblTaxableFreeFieldName.Visible = false;
                this.txtTaxableFreeFieldValue.Visible = false;
            }
            else
            {
                if (reporttype == 1 && warrantyFlag == 1)
                {
                    this.lblTaxableFreeFieldName.Value = null;
                }
                else
                {
                    this.lblTaxableFreeFieldName.Value = taxableFreeFieldName;
                }
            }
            
            //Add 2023/09/05  #4162
            //------------------------
            //インボイス消費税取得
            //------------------------
            string slipNumber = "";
            try { slipNumber = Fields["SlipNumber"].Value.ToString(); }
            catch (InvalidCastException) { }

            //一般消費税取得
            string customerclaimCode = "";
            try { customerclaimCode = Fields["CustomerClaimCode"].Value.ToString(); }
            catch (InvalidCastException) { }

            decimal ? invoicetaxamount = 0m;

            if(!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerclaimCode)){
              invoicetaxamount = new InvoiceConsumptionTaxDao(db).GetByKey(slipNumber, customerclaimCode, rate ?? 0) != null ? new InvoiceConsumptionTaxDao(db).GetByKey(slipNumber, customerclaimCode, rate ?? 0).InvoiceConsumptionTaxAmount : 0;
            }

            this.txtTotalTaxAmount.Value = invoicetaxamount;
   
            //Mod 2023/09/05  #4162
            ////諸費用合計（課税）
            //if (taxableCostTotalAmount == null || taxableCostTotalAmount == 0)
            //{
            //    if (taxableCostTotalAmount == null) taxableCostTotalAmount = 0;
            
            //    //this.lblTaxableCostTotalAmount.Visible = false;
            //    //this.txtTaxableCostTotalAmount.Visible = false;
                
            //    //this.lblTaxableCostTaxAmount.Visible = false;
            //    //this.txtTaxableCostTaxAmount.Visible = false;
            //    //this.lblRate.Visible = false;
            //}
            //else
            //{
            //    //this.lblTaxableCostTotalAmount.Text = "合計";

            //    decimal r = Math.Round((decimal)(rate / (100 + rate)), 17);

            //    decimal? taxableCostTaxAmount = 0m;

            //    if ((taxableCostTotalAmount ?? 0m) > 0)
            //    {
            //        taxableCostTaxAmount = Math.Floor((taxableCostTotalAmount ?? 0m) * r);
            //    }
            //    else
            //    {
            //        taxableCostTaxAmount = Math.Ceiling((taxableCostTotalAmount ?? 0m) * r);
            //    }

            //    //Add 2020/03/27 yano #4046
            //    if (reporttype == 1 && warrantyFlag == 1)
            //    {
            //        taxableCostTaxAmount = null;
            //    }

            //    //Mod 2023/09/05 yano  #4162
            //    //this.txtTaxableCostTaxAmount.Value = taxableCostTaxAmount;
            //}

            //--------------------------------------------------------------------------
        }

        private void ServiceClaimDetailReport_ReportStart(object sender, EventArgs e)
        {
            //Add 2023/04/06 yano #4162
            var query = new ConfigurationSettingDao(db).GetByKey("InvoiceProviderNumber");

            this.txtInvoiceProviderNumber.Text = query != null ? ( "登録番号：" + query.Value) : "";

            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
       }
    }
}
