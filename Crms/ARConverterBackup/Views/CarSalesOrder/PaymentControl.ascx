<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<table class="input-form-line">
    <tr>
        <th colspan="6" class="input-form-title">
            支払情報
        </th>
    </tr>
    <tr>
        <th style="width: 20px; height:20px; text-align: center; padding:0px">
            <%if (Model.PaymentEnabled) { %><img alt="行追加" src="/Content/Images/plus.gif" style="cursor: pointer"
                onclick="$('#DelPayLine').val('-1');document.forms[0].action = '/CarSalesOrder/Payment';formSubmit();" /><%} %>
        </th>
        <th style="width: 120px; padding:0px">
            請求先コード
        </th>
        <th style="width: 240px; padding:0px">
            請求先名
        </th>
        <th style="width: 80px; padding:0px">
            入金予定日
        </th>
        <th style="width: 70px; padding:0px">
            金額
        </th>
        <th style="width: 85px; padding:0px">
            備考
        </th>
    </tr>
</table>
<div style="overflow-y: scroll; width: 640px; height: 150px">
    <table class="input-form-slim">
        <%for (int i = 0; i < Model.CarSalesPayment.Count; i++) {
              string namePrefix = string.Format("pay[{0}].", i);
              //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
              string idPrefix = string.Format("pay[{0}]_", i);
        %>
        <tr>
            <td style="width: 20px; text-align: center">
                <%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber"})%>
                <%if(Model.PaymentEnabled){ %>
                <img alt="行削除" src="/Content/Images/minus.gif" style="cursor: pointer" onclick="$('#DelPayLine').val('<%=i %>');document.forms[0].action = '/CarSalesOrder/Payment';formSubmit();" />
                <%} %>
            </td>
            <td style="width: 120px">
                <%if(Model.PaymentEnabled){ %>
                <% //Mod 2014/07/15 arc yano 引数をnameからidに変更 %>
                <%=Html.TextBox(namePrefix + "CustomerClaimCode", Model.CarSalesPayment[i].CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "alphanumeric", style="width:90px", maxlength = "10",onchange="GetNameFromCode('pay["+i+"]_CustomerClaimCode','pay["+i+"]_CustomerClaimName','CustomerClaim')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "0" }); %>
                <%}else{ %>
                <%=Html.TextBox(namePrefix + "CustomerClaimCode", Model.CarSalesPayment[i].CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "readonly alphanumeric", style="width:90px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "1" }); %>
                <%} %>
            </td>
            <td style="width: 240px">
                <%=Html.TextBox(namePrefix + "CustomerClaimName", ViewData["CustomerClaimName[" + i + "]"], new { id = idPrefix + "CustomerClaimName", @class = "readonly", style = "width:234px", @redonly = "readonly" })%>
            </td>
            <td style="width: 80px">
                <%if(Model.PaymentEnabled){ %>
                <%//Add 2017/01/31 arc nakayama  #3652_車両伝票入力で入金予定日で誤入力するとシステムエラーが発生しそれまで入力したデータが消失する 日付チェック%>
                <%//Add 2017/02/01 arc nakayama  #3247_車両伝票入力フォームの改善対応 入金予定日にカレンダーが表示されるように変更%>
                <%=Html.TextBox(namePrefix +  "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", Model.CarSalesPayment[i].PaymentPlanDate), new { id = idPrefix +  "PaymentPlanDate", @class = "alphanumeric", style="width:74px", maxlength = "10" ,onchange ="return chkDate3(document.getElementById('" + idPrefix + "PaymentPlanDate').value, '" + namePrefix + "PaymentPlanDate')", onclick = "$(this).datepicker({format: 'yyyy/mm/dd',autoclose: true}); $(this).datepicker('show');"})%>
                <%}else{ %>
                <%=Html.TextBox(namePrefix + "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", Model.CarSalesPayment[i].PaymentPlanDate), new { id = idPrefix + "PaymentPlanDate", @class = "readonly alphanumeric", style = "width:74px", @readonly = "readonly" })%>
                <%} %>
            </td>
            <td style="width: 70px">
                <%if (Model.PaymentEnabled) { %>
                <%=Html.TextBox(namePrefix + "Amount",  string.Format("{0:N0}", Model.CarSalesPayment[i].Amount), new { id = idPrefix + "Amount", @class = "money", style = "width:64px", maxlength = "10", onkeyup = "calcTotalOptionAmount(); calcTotalAmount()", onpaste = "setTimeout('calcTotalAmount()',100)", onblur = "CheckAmount('pay["+i+"]_Amount');calcTotalOptionAmount();calcTotalAmount()" })%><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
                <%} else { %>
                <%=Html.TextBox(namePrefix + "Amount",  string.Format("{0:N0}", Model.CarSalesPayment[i].Amount), new { id = idPrefix + "Amount", @class = "readonly money", style = "width:64px", @readonly = "readonly" })%>
                <%} %>
            </td>
            <td style="width: 85px">
                <%if(Model.PaymentEnabled){ %>
                <%=Html.TextBox(namePrefix + "Memo", Model.CarSalesPayment[i].Memo, new { id = idPrefix + "Memo", style = "width:79px", maxlength = "100" })%>
                <%}else{ %>
                <%=Html.TextBox(namePrefix + "Memo", Model.CarSalesPayment[i].Memo, new { id = idPrefix + "Memo", @class = "readonly", style = "width:79px", @readonly = "readonly" })%>
                <%} %>
            </td>
        </tr>
        <%  } %>
        <%// Add 2014/05/16 arc yano vs2012対応 隠しフィールド(支払行数)追加。%>
        <%=Html.Hidden("PayLineCount", Model.CarSalesPayment.Count)%>
    </table>
</div>
<br />
<br />
  <!-- Mod 2014/07/11 arc amii chrome対応 項目が右寄せになるよう修正 -->
  <div style="margin:0 auto; padding-left:305px;">
    <table class="input-form-slim">
        <tr>
            <th style="width: 200px">
                下取車価格合計
            </th>
            <td style="width: 100px">
                <%=Html.TextBox("TradeInTotalAmount",  string.Format("{0:N0}", Model.TradeInTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                下取車消費税合計
            </th>
            <td>
                <%=Html.TextBox("TradeInTaxTotalAmount", string.Format("{0:N0}", Model.TradeInTaxTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                下取車未払自動車税種別割合計 <%-- Mod 2019/09/04 yano #4011 --%>
            </th>
            <td>
                <%=Html.TextBox("TradeInUnexpiredCarTaxTotalAmount", string.Format("{0:N0}", Model.TradeInUnexpiredCarTaxTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                下取車リサイクル合計
            </th>
            <td>
                <%=Html.TextBox("TradeInRecycleTotalAmount", string.Format("{0:N0}", Model.TradeInRecycleTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" }) %>
            </td>
        </tr>
        <tr>
            <th>
                下取車残債合計
            </th>
            <td>
                <%=Html.TextBox("TradeInRemainDebtTotalAmount", string.Format("{0:N0}", Model.TradeInRemainDebtTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                下取充当金合計
            </th>
            <td>
                <%=Html.TextBox("TradeInAppropriationTotalAmount", string.Format("{0:N0}", Model.TradeInAppropriationTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                下取後支払総額
            </th>
            <td>
                <%=Html.TextBox("PaymentTotalAmount", string.Format("{0:N0}", Model.PaymentTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                現金(申込金を含む)
            </th>
            <td>
                <%=Html.TextBox("PaymentCashTotalAmount", string.Format("{0:N0}", Model.PaymentCashTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                支払残額(＝ローン元金)
            </th>
            <td>
                <%=Html.TextBox("LoanPrincipalAmount", string.Format("{0:N0}", Model.LoanPrincipalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                ローン選択プラン
            </th>
            <td>
                <%=Html.TextBox("SelectedLoanPlan",Model.PaymentPlanType,new { @class = "readonly numeric", @readonly = "readonly", style = "width:100px" }) %>
            </td>
        </tr>
        <tr>
            <th>
                ローン手数料
            </th>
            <td>
                <%=Html.TextBox("LoanFeeAmount", string.Format("{0:N0}", Model.LoanFeeAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th>
                ローン合計
            </th>
            <td>
                <%=Html.TextBox("LoanTotalAmount", string.Format("{0:N0}", Model.LoanTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px" })%>
            </td>
        </tr>
        <tr>
            <th style="background-color:#CCCC99;font-weight:bold">
                総支払金合計
            </th>
            <td style="background-color:#CCCC99">
                <%=Html.TextBox("PaymentGrandTotalAmount", string.Format("{0:N0}", Model.PaymentCashTotalAmount + Model.TradeInAppropriationTotalAmount + Model.LoanTotalAmount), new { @class = "readonly money", @readonly = "readonly", style = "width:100px;font-weight:bold;background-color:#CCCC99" })%>
            </td>
        </tr>
    </table>
</div>
