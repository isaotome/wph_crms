<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス伝票明細入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<br />
<table class="input-form">
    <tr>
        <th style="width:80px">伝票番号</th>
        <th style="width:200px">顧客名</th>
        <th style="width:300px">車種名</th>
    </tr>
    <tr>
        <td style="white-space:nowrap"><%=Html.Encode(Model.SlipNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(Model.CustomerCode) %>&nbsp;<%=Html.Encode(Model.Customer!=null ? Model.Customer.CustomerName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(Model.CarBrandName) %>&nbsp;<%=Html.Encode(Model.CarName) %>&nbsp;<%=Html.Encode(Model.CarName) %>&nbsp;<%=Html.Encode(Model.CarGradeName) %></td>
    </tr>
</table>
<%using(Html.BeginForm("ChangeEntryMode","ServiceSalesOrder",FormMethod.Post)){ %>
<br />
<%//Edit 2014/06/12 arc yano 高速化対応 サーバ送信時には、name属性のidを振り直す %>
<input type="button" value="通常モードに戻る" onclick="formList(); document.getElementById('EntryMode').value = 'Normal';formSubmit();" />
<br />
<br />


<% //2017/04/24 arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合 %>

<!-- Hidden -->
<%=Html.Hidden("Rate",Model.Rate) %><%//Edit 2014/06/12 arc yano 高速化対応 %>
<%=Html.Hidden("SlipNumber",Model.SlipNumber) %>
<%=Html.Hidden("RevisionNumber",Model.RevisionNumber) %>
<%=Html.Hidden("CarSlipNUmber",Model.CarSlipNumber) %>
<%=Html.Hidden("CarSalesOrderDate",Model.CarSalesOrderDate) %>
<%=Html.Hidden("QuoteDate",Model.QuoteDate) %>
<%=Html.Hidden("QuoteExpireDate",Model.QuoteExpireDate) %>
<%=Html.Hidden("ServiceOrderStatus",Model.ServiceOrderStatus) %>
<%=Html.Hidden("ArrivalPlanDate",Model.ArrivalPlanDate) %>
<%=Html.Hidden("ApprovalFlag",Model.ApprovalFlag) %>
<%=Html.Hidden("CampaignCode1",Model.CampaignCode1) %>
<%=Html.Hidden("CampaignCode2",Model.CampaignCode2) %>
<%=Html.Hidden("WorkingStartDate",Model.WorkingStartDate) %>
<%=Html.Hidden("WorkingEndDate",Model.WorkingEndDate) %>
<%=Html.Hidden("SalesDate",Model.SalesDate) %>
<%=Html.Hidden("CustomerCode",Model.CustomerCode) %>
<%=Html.Hidden("DepartmentCode",Model.DepartmentCode) %>
<%=Html.Hidden("CarEmployeeCode",Model.CarEmployeeCode) %>
<%=Html.Hidden("FrontEmployeeCode",Model.FrontEmployeeCode) %>
<%=Html.Hidden("ReceiptionEmployeeCode",Model.ReceiptionEmployeeCode) %>
<%=Html.Hidden("CarGradeCode",Model.CarGradeCode) %>
<%=Html.Hidden("CarBrandName",Model.CarBrandName) %>
<%=Html.Hidden("CarName",Model.CarName) %>
<%=Html.Hidden("CarGradeName",Model.CarGradeName) %>
<%=Html.Hidden("EngineType",Model.EngineType) %>
<%=Html.Hidden("ManufacturingYear",Model.ManufacturingYear) %>
<%=Html.Hidden("Vin",Model.Vin) %>
<%=Html.Hidden("ModelName",Model.ModelName) %>
<%=Html.Hidden("Mileage",Model.Mileage) %>
<%=Html.Hidden("MileageUnit",Model.MileageUnit) %>
<%=Html.Hidden("SalesPlanDate",Model.SalesPlanDate) %>
<%=Html.Hidden("FirstRegistration",Model.FirstRegistration) %>
<%=Html.Hidden("NextInspectionDate",Model.NextInspectionDate) %>
<%=Html.Hidden("MorterViecleOfficialCode",Model.MorterViecleOfficialCode) %>
<%=Html.Hidden("RegistrationNumberType",Model.RegistrationNumberType) %>
<%=Html.Hidden("RegistrationNumberKana",Model.RegistrationNumberKana) %>
<%=Html.Hidden("RegistrationNumberPlate",Model.RegistrationNumberPlate) %>
<%=Html.Hidden("MakerShipmentDate",Model.MakerShipmentDate) %>
<%=Html.Hidden("RegistrationPlanDate",Model.RegistrationPlanDate) %>
<%=Html.Hidden("RequestContent",Model.RequestContent) %>
<%=Html.Hidden("CarTax", Model.CarTax, new { @class = "money"})%>
<%=Html.Hidden("CarLiabilityInsurance",Model.CarLiabilityInsurance, new { @class = "money"}) %>
<%=Html.Hidden("CarWeightTax",Model.CarWeightTax, new { @class = "money"}) %>
<%=Html.Hidden("FiscalStampCost",Model.FiscalStampCost, new { @class = "money"}) %>
<%=Html.Hidden("InspectionRegistCost",Model.InspectionRegistCost, new { @class = "money"}) %>
<%=Html.Hidden("ParkingSpaceCost",Model.ParkingSpaceCost, new { @class = "money"}) %>
<%=Html.Hidden("TradeInCost",Model.TradeInCost, new { @class = "money"}) %>
<%=Html.Hidden("ReplacementFee",Model.ReplacementFee, new { @class = "money"}) %>
<%=Html.Hidden("InspectionRegistFee",Model.InspectionRegistFee, new { @class = "money"}) %>
<%=Html.Hidden("ParkingSpaceFee",Model.ParkingSpaceFee, new { @class = "money"}) %>
<%=Html.Hidden("TradeInFee",Model.TradeInFee, new { @class = "money"}) %>
<%=Html.Hidden("PreparationFee",Model.PreparationFee, new { @class = "money"}) %>
<%=Html.Hidden("RecycleControlFee",Model.RecycleControlFee, new { @class = "money"}) %>
<%=Html.Hidden("RecycleControlFeeTradeIn",Model.RecycleControlFeeTradeIn, new { @class = "money"}) %>
<%=Html.Hidden("RequestNumberFee",Model.RequestNumberFee, new { @class = "money"}) %>
<%=Html.Hidden("CarTaxUnexpiredAmount",Model.CarTaxUnexpiredAmount, new { @class = "money"}) %>
<%=Html.Hidden("CarLiabilityInsuranceUnexpiredAmount",Model.CarLiabilityInsuranceUnexpiredAmount, new { @class = "money"}) %>
<%=Html.Hidden("Memo",Model.Memo) %>
<%=Html.Hidden("EngineerTotalAmount",Model.EngineerTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("PartsTotalAmount",Model.PartsTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("SubTotalAmount",Model.SubTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("TotalTaxAmount",Model.TotalTaxAmount, new { @class = "money"}) %>
<%=Html.Hidden("ServiceTotalAmount",Model.ServiceTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("CostTotalAmount",Model.CostTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("GrandTotalAmount",Model.GrandTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("PaymentTotalAmount",Model.PaymentTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %>
<%=Html.Hidden("InspectionExpireDate",Model.InspectionExpireDate) %>
<%=Html.Hidden("NumberPlateCost",Model.NumberPlateCost, new { @class = "money"}) %>
<%=Html.Hidden("TaxFreeFieldName",Model.TaxFreeFieldName) %>
<%=Html.Hidden("TaxFreeFieldValue",Model.TaxFreeFieldValue, new { @class = "money"}) %>
<%=Html.Hidden("UsVin",Model.UsVin) %>
<%for (int i=0; i < Model.ServiceSalesPayment.Count();i++ ) { %>
    <% string payPrefix = string.Format("pay[{0}].", i); %>
    <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにid追加 %>
    <% string payidPrefix = string.Format("pay[{0}]_", i); %>
    <%=Html.Hidden(payPrefix + "LineNumber", Model.ServiceSalesPayment[i].LineNumber, new { id = payidPrefix + "LineNumber"})%>
    <%=Html.Hidden(payPrefix + "CustomerClaimCode", Model.ServiceSalesPayment[i].CustomerClaimCode, new { id = payidPrefix + "CustomerClaimCode"})%>
    <%=Html.Hidden(payPrefix + "PaymentPlanDate", Model.ServiceSalesPayment[i].PaymentPlanDate, new { id = payidPrefix + "PaymentPlanDate"})%>
    <%=Html.Hidden(payPrefix + "Amount", Model.ServiceSalesPayment[i].Amount, new { id = payidPrefix + "Amount", @class = "money"})%>
    <%=Html.Hidden(payPrefix + "Memo", Model.ServiceSalesPayment[i].Memo, new { id = payidPrefix + "Memo"})%>
    <%=Html.Hidden(payPrefix + "DepositFlag", Model.ServiceSalesPayment[i].DepositFlag, new { id = payidPrefix + "DepositFlag"})%>
<%} %>
<%=Html.Hidden("PaymentCount",Model.ServiceSalesPayment.Count) %>
<%=Html.Hidden("EntryMode",ViewData["EntryMode"]) %>
<%=Html.Hidden("CurrentLineNumber","") %>
<%=Html.Hidden("LineCount",Model.ServiceSalesLine.Count) %>
<%=Html.Hidden("EditType","") %>
<%=Html.Hidden("EditLine","") %>
<%=Html.Hidden("AddSize","") %>
<%=Html.Hidden("lineScroll", ViewData["lineScroll"])%>
<%=Html.Hidden("KeepsCarFlag",Model.KeepsCarFlag) %>
&nbsp;
<%--<%=Html.Hidden("LaborRate",Model.LaborRate, new { @class = "money"}) %><%//Del 2019/08/26 yano #4005 %>--%>
<%=Html.Hidden("MileageUnitName", Model.c_MileageUnit) %>

<%//Add 2020/02/17 yano #4025-------------------------------------------------------------------------- %>
<%=Html.Hidden("OptionalInsurance",Model.OptionalInsurance, new { @class = "money"}) %>
<%=Html.Hidden("SubscriptionFee",Model.SubscriptionFee, new { @class = "money"}) %>
<%=Html.Hidden("TaxableCostTotalAmount",Model.TaxableCostTotalAmount, new { @class = "money"}) %>
<%=Html.Hidden("CarTaxMemo",Model.CarTaxMemo) %>
<%=Html.Hidden("CarLiabilityInsuranceMemo",Model.CarLiabilityInsuranceMemo) %>
<%=Html.Hidden("CarWeightTaxMemo",Model.CarWeightTaxMemo) %>
<%=Html.Hidden("NumberPlateCostMemo",Model.NumberPlateCostMemo) %>
<%=Html.Hidden("FiscalStampCostMemo",Model.FiscalStampCostMemo) %>
<%=Html.Hidden("OptionalInsuranceMemo",Model.OptionalInsuranceMemo) %>
<%=Html.Hidden("SubscriptionFeeMemo",Model.SubscriptionFeeMemo) %>
<%=Html.Hidden("TaxableFreeFieldValue",Model.TaxableFreeFieldValue) %>
<%=Html.Hidden("TaxableFreeFieldName",Model.TaxableFreeFieldName) %>
<%//Add 2023/04/25 openwave #xxxx %>
<%=Html.Hidden("CustomerClaimCode",Model.CustomerClaimCode) %>
<%//---------------------------------------------------------------------------------------------------- %>

<%//Add 2021/02/22 yano #4074--------------------------------------------------------------------------- %>
<%=Html.Hidden("SalesOrderDate",Model.SalesOrderDate) %>
<%=Html.Hidden("CampaignName1",Model.CampaignCode1) %>
<%=Html.Hidden("CampaignName2",Model.CampaignCode2) %>
<%//---------------------------------------------------------------------------------------------------- %>

<!-- ここまで -->
<div id="head" style="width:1949px;height:67px;overflow:hidden"><%//Add 2021/10/25 yano #4100%>
<%Html.RenderPartial("LineTitleControl", Model); %>
</div>
<div id="line" style="width:1966px;overflow:auto"><%//Mod 2021/10/25 yano #4100%><%//Mod 2021/11/22 yano #4116 height削除%>
<%Html.RenderPartial("LineControl", Model); %>
</div>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
