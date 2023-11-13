<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // ----------------------------------------------------------------------------------------------------------------------
   // Mod 2020/01/06 yano #4025 【サービス諸経費】費目毎に仕訳できるように機能追加　レイアウトを大幅に変更
   // Mod 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 文言、レイアウト変更
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // -----------------------------------------------------------------------------------------------------------------------
%>
<table class="input-form-slim" style="float:left; margin-right:5px;" onkeyup ="calcTotalServiceAmount()"><%//Mod 2022/06/08 yano #4137 %>
<%--<table class="input-form-slim" style="float:left; margin-right:5px;">--%>
    <%//Mod Mod 2020/01/06 yano #4025 %>
    <tr>
        <th colspan="2" class="input-form-title">
            税金・保険料・預かり法定費用（非課税）
        </th>
        <th class="input-form-title">
            備考
        </th>
    </tr>
    <tr>
        <th style="width:100px"><%// Mod 2019/09/04 yano #4011 80px->90px %>
            自動車税種別割 
        </th>
        <td style="width:100px">
            <%= "" %>
            <%if(Model.CostEnabled){ %>
                <%=Html.TextBox("CarTax", string.Format("{0:N0}", Model.CarTax), new { @class = "money", style="width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%= "" %>
            <%}else{ %>
                <%=Html.TextBox("CarTax", string.Format("{0:N0}", Model.CarTax), new { @class = "money readonly", @readonly = "readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
        <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("CarTaxMemo", Model.CarTaxMemo, new { style="width:150px" })%>
            <%} else { %>
                <%=Html.TextBox("CarTaxMemo", Model.CarTaxMemo, new { @class = "readonly", style="width:150px"})%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            自賠責保険料
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if(Model.CostEnabled){ %>
                <%=Html.TextBox("CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { @class = "money", style="width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%}else{ %>
                <%=Html.TextBox("CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { @class = "money readonly", @readonly="readonly", style="width:94px", maxlength = "10" })%>
            <%} %>
        </td>
         <td style="width:150px"> 
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("CarLiabilityInsuranceMemo", Model.CarLiabilityInsuranceMemo, new { style="width:150px"})%>
            <%} else { %>
                <%=Html.TextBox("CarLiabilityInsuranceMemo", Model.CarLiabilityInsuranceMemo, new { @class = "readonly", style="width:150px"})%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            自動車重量税
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if(Model.CostEnabled){ %>
                <%=Html.TextBox("CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%}else{ %>
                <%=Html.TextBox("CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { @class = "money readonly", @readonly="readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
         <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("CarWeightTaxMemo", Model.CarWeightTaxMemo, new { style="width:150px"})%>
            <%} else { %>
                <%=Html.TextBox("CarWeightTaxMemo", Model.CarWeightTaxMemo, new { @class = "readonly", style="width:150px"})%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px"><%// Mod 2019/09/04 yano #4011 80px->90px %>
            ナンバー代
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("NumberPlateCost", string.Format("{0:N0}", Model.NumberPlateCost), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%} else { %>
                <%=Html.TextBox("NumberPlateCost", string.Format("{0:N0}", Model.NumberPlateCost), new { @class = "money readonly", @readonly = "readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
         <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("NumberPlateCostMemo", Model.NumberPlateCostMemo, new { style="width:150px" })%>
            <%} else { %>
                <%=Html.TextBox("NumberPlateCostMemo", Model.NumberPlateCostMemo, new { @class = "readonly", style="width:150px"})%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            各種印紙代
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("FiscalStampCost", string.Format("{0:N0}", Model.FiscalStampCost), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%} else { %>
                <%=Html.TextBox("FiscalStampCost", string.Format("{0:N0}", Model.FiscalStampCost), new { @class = "money readonly", @readonly="readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
         <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("FiscalStampCostMemo", Model.FiscalStampCostMemo, new { style="width:150px" })%>
            <%} else { %>
                <%=Html.TextBox("FiscalStampCostMemo", Model.FiscalStampCostMemo, new { @class = "readonly", style="width:150px" })%>
            <%} %>
        </td>
    </tr>
     <tr>
        <th style="width: 100px">
            任意保険
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("OptionalInsurance", string.Format("{0:N0}", Model.OptionalInsurance), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%} else { %>
                <%=Html.TextBox("OptionalInsurance", string.Format("{0:N0}", Model.OptionalInsurance), new { @class = "money readonly", @readonly="readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
        <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("OptionalInsuranceMemo", Model.OptionalInsuranceMemo, new { style="width:150px" })%>
            <%} else { %>
                <%=Html.TextBox("OptionalInsuranceMemo", Model.OptionalInsuranceMemo, new { @class = "readonly", style="width:150px" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 80px">
           その他
        </th>
        <td style="width: 100px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("TaxFreeFieldValue", string.Format("{0:N0}", Model.TaxFreeFieldValue), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
            <%} else { %>
                <%=Html.TextBox("TaxFreeFieldValue", string.Format("{0:N0}", Model.TaxFreeFieldValue), new { @class = "money readonly", @readonly = "readonly", style = "width:94px", maxlength = "10" })%>
            <%} %>
        </td>
        <td style="width:150px">
            <%= "" %>
            <%if (Model.CostEnabled) { %>
                <%=Html.TextBox("TaxFreeFieldName", Model.TaxFreeFieldName, new { style = "width:150px" })%>
            <%} else { %>
                <%=Html.TextBox("TaxFreeFieldName", Model.TaxFreeFieldName, new { @class = "readonly", style = "width:150px"})%>
            <%} %>
        </td>
    </tr>
</table>
<%//Add 2020/01/06 yano #4025 %>
<table class="input-form-slim" onkeyup ="calcTotalServiceAmount()"><%//Mod 2022/06/08 yano #4137 %>
<%--<table class="input-form-slim">--%>
    <tr>
        <th colspan="2" class="input-form-title">
            その他諸費用（課税）
        </th>
        <th class="input-form-title">
            備考
        </th>
    </tr>
    <tr>
        <th style="width:130px">サービス加入料（税込）</th>
        <td style="width: 100px">
        <%= "" %>
        <%if (Model.CostEnabled) { %>
            <%=Html.TextBox("SubscriptionFee", string.Format("{0:N0}", Model.SubscriptionFee), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
        <%} else { %>
            <%=Html.TextBox("SubscriptionFee", string.Format("{0:N0}", Model.SubscriptionFee), new { @class = "money readonly", @readonly="readonly", style = "width:94px", maxlength = "10" })%>
        <%} %>
        </td>
        <td style="width: 150px">
        <%= "" %>
        <%if (Model.CostEnabled) { %>
            <%=Html.TextBox("SubscriptionFeeMemo", Model.SubscriptionFeeMemo, new { style="width:150px" })%>
        <%} else { %>
            <%=Html.TextBox("SubscriptionFeeMemo", Model.SubscriptionFeeMemo, new { @class = "readonly", style="width:150px" })%>
        <%} %>
        </td>
    </tr>
     <tr>
        <th style="width:130px">その他（税込）</th>
        <td style="width: 100px">
        <%= "" %>
        <%if (Model.CostEnabled) { %>
            <%=Html.TextBox("TaxableFreeFieldValue", string.Format("{0:N0}", Model.TaxableFreeFieldValue), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalServiceAmount()" })%>
        <%} else { %>
            <%=Html.TextBox("TaxableFreeFieldValue", string.Format("{0:N0}", Model.TaxableFreeFieldValue), new { @class = "money readonly", @readonly="readonly", style = "width:94px", maxlength = "10" })%>
        <%} %>
        </td>
        <td style="width: 150px">
        <%= "" %>
        <%if (Model.CostEnabled) { %>
            <%=Html.TextBox("TaxableFreeFieldName", Model.TaxableFreeFieldName, new { style="width:150px" })%>
        <%} else { %>
            <%=Html.TextBox("TaxableFreeFieldName", Model.TaxableFreeFieldName, new { @class = "readonly", style="width:150px" })%>
        <%} %>
        </td>
    </tr>
</table>
<%//Add 2023/04/25 openwave #4141 %>
<br />
<table class="input-form-slim">
    <tr>
        <th style="width:130px">請求先コード</th>
        <td style="width:340px">
        <%= "" %>
        <%if (Model.CostEnabled) { %>
            <%=Html.TextBox("CustomerClaimCode", !string.IsNullOrEmpty(Model.CustomerClaimCode) ? Model.CustomerClaimCode : "", new { @class = "alphanumeric", style="width:92px", maxlength="10", onblur = "GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim', 'false', null, null, null, null, null, null)" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerClaimCode", "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "0" });%>
        <%= "" %>
        <%} else { %>
            <%=Html.TextBox("CustomerClaimCode", !string.IsNullOrEmpty(Model.CustomerClaimCode) ? Model.CustomerClaimCode : "", new { @class = "alphanumeric readonly", @readonly="readonly", style="width:92px", maxlength="10" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerClaimCode", "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "1" });%>
        <%} %>
        <%= "" %>

       <%//Mod 2023/08/03 yano #4141 %>
       <%=Html.TextBox("CustomerClaimName", Model.CustomerClaim != null ? Model.CustomerClaim.CustomerClaimName : "", new { @class = "readonly", @readonly = "readonly", style = "width:295px"})%>
       <%-- <%if (Model.CustomerClaimCode != null) { %>
            <%=Html.TextBox("CustomerClaimName", Model.CustomerClaim.CustomerClaimName, new { @class = "readonly", @readonly = "readonly", style = "width:295px"})%>
        <%= "" %>
        <%} else { %>
            <%=Html.TextBox("CustomerClaimName", "", new { @class = "readonly", @readonly = "readonly", style = "width:295px"})%>
        <%} %>--%>
        </td>
    </tr>
</table>
