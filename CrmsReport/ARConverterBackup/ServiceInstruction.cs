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
    /// 作業指示書 の概要の説明です。
    /// </summary>
    public partial class ServiceInstruction : GrapeCity.ActiveReports.SectionReport
    {

        public ServiceInstruction()
        {
            //
            // ActiveReport デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

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
        /// 作業指示書データ設定
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// </history>
        /// <param name="salesCar">モデルデータ</param>
        private void ServiceInstruction_FetchData(object sender, FetchEventArgs eArgs)
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

            decimal? rate = null;
            try { rate = decimal.Parse(Fields["Rate"].Value.ToString()); }
            catch (InvalidCastException) { }

            decimal? subscriptionFee = null;
            try { subscriptionFee = (decimal)Fields["SubscriptionFee"].Value; }
            catch (InvalidCastException) { }

            decimal? taxableFreeFieldValue = null;
            try { taxableFreeFieldValue = (decimal)Fields["TaxableFreeFieldValue"].Value; }
            catch (InvalidCastException) { }
            //--------------------------------------------------------------------------

            if (carWeightTax == null || carWeightTax == 0)
            {
                this.lblCarWeightTax.Visible = false;
                this.txtCarWeightTax.Visible = false;
            }
            if (carLiabilityInsurance == null || carLiabilityInsurance == 0)
            {
                this.lblCarLiabilityInsurance.Visible = false;
                this.txtCarLiabilityInsurance.Visible = false;
            }
            if (fiscalStampCost == null || fiscalStampCost == 0)
            {
                this.lblFiscalStampCost.Visible = false;
                this.txtFiscalStampCost.Visible = false;
            }
            if (costTotalAmount == null || costTotalAmount == 0)
            {
                this.lblCostTotalAmount.Visible = false;
                this.txtCostTotalAmount.Visible = false;
            }
            if (depositTotalAmount == null || depositTotalAmount == 0)
            {
                this.lblDepositTotalAmount.Visible = false;
                this.txtDepositTotalAmount.Visible = false;
            }
            if (mileage == null)
            {
                this.txtMileageUnit.Visible = false;
            }
            if (quantity != null && ((int)quantity) == quantity.Value)
            {
                txtQuantity1.OutputFormat = "#,##0";
            }
            else
            {
                txtQuantity1.OutputFormat = "#,##0.000";
            }
            if (carTax == null || carTax == 0)
            {
                this.lblCarTax.Visible = false;
                this.txtCarTaxAmount.Visible = false;
            }
            if (numberPlateCost == null || numberPlateCost == 0)
            {
                this.lblNumberPlateCost.Visible = false;
                this.txtNumberPlateCost.Visible = false;
            }
            if (taxFreeFieldValue == null || taxFreeFieldValue == 0)
            {
                this.lblTaxFreeFieldName.Visible = false;
                this.txtTaxFreeFieldValue.Visible = false;
            }

            //Add 2020/02/17 yano #4025-------------------------------------------------
            if (optionalInsurance == null || optionalInsurance == 0)
            {
                this.lblOptionalInsurance.Visible = false;
                this.txtOptionalInsurance.Visible = false;
            }

            if (taxableCostTotalAmount == null || taxableCostTotalAmount == 0)
            {
                this.lblTaxableCostTotalAmount.Visible = false;
                this.txtTaxableCostTotalAmount.Visible = false;
                this.lblTaxableCostTaxAmount.Visible = false;
                this.txtTaxableCostTaxAmount.Visible = false;
                this.lblRate.Visible = false;
            }
            else
            {
                this.lblTaxableCostTotalAmount.Text = "合計";

                decimal r = Math.Round((decimal)(rate / (100 + rate)), 17);

                decimal? taxableCostTaxAmount = 0m;

                if ((taxableCostTotalAmount ?? 0m) > 0)
                {
                    taxableCostTaxAmount = Math.Floor((taxableCostTotalAmount ?? 0m) * r);
                }
                else
                {
                    taxableCostTaxAmount = Math.Ceiling((taxableCostTotalAmount ?? 0m) * r);
                }

                this.txtTaxableCostTaxAmount.Value = taxableCostTaxAmount;
            }

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

            if (taxableFreeFieldValue == null || taxableFreeFieldValue == 0)
            {
                this.lblTaxableFreeFieldName.Visible = false;
                this.txtTaxableFreeFieldValue.Visible = false;
            }

            //--------------------------------------------------------------------------
        }

        private void ServiceInstruction_ReportStart(object sender, EventArgs e)
        {
            // Setup Virtual Printer and Paper Size(A4/Portrait).
            this.Document.Printer.PrinterName = "";
            this.PageSettings.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.PageSettings.Orientation = GrapeCity.ActiveReports.Document.Section.PageOrientation.Portrait;
        }
    }
}
