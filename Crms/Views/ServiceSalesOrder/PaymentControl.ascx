<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<table class="input-form-slim">
    <tr>
        <th colspan="7" class="input-form-title">支払情報<%if(Model.PaymentEnabled){ %> [ <a href="javascript:void(0);" onclick="document.forms[0].action='/ServiceSalesOrder/SetPaymentLine';formSubmit();return false;" >明細から取込</a> ]<%} %></th>
    </tr>
    <tr>
        <th style="width: 15px;text-align:center"><%if (Model.PaymentEnabled) { %><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="document.forms[0].action='/ServiceSalesOrder/AddPaymentLine';$('#DelPayLine').val('-1');formSubmit();" /><%} %></th>
        <th style="width: 120px">請求先コード</th>
        <th style="width: 260px">請求先名</th>
        <th style="width: 80px">入金予定日</th>
        <th style="width: 83px">金額</th>
        <th style="width: 200px">備考</th>
        <th style="width: 50px">預かり金</th>
    </tr>
</table>
<div style="overflow-y: scroll; width: 875px; height: 150px">
    <table class="input-form-slim">
        <%for (int i = 0; i < Model.ServiceSalesPayment.Count; i++) {
              ServiceSalesPayment pay = Model.ServiceSalesPayment[i];
              string namePrefix = string.Format("pay[{0}].", i);
              //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
              string idPrefix = string.Format("pay[{0}]_", i);
              
              %>
            <%if (Model.PaymentEnabled) { %>
            <tr>
                <td style="width: 21px;text-align:center"><%=Html.Hidden( namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber" })%><img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="document.forms[0].action='/ServiceSalesOrder/AddPaymentLine';$('#DelPayLine').val('<%=i %>');formSubmit();" /></td>
                <% //Mod 2014/07/15 arc yano chrome対応 メソッドのパラメータをname→idに修正 %>
                <td style="width: 126px"><%=Html.TextBox(namePrefix +"CustomerClaimCode",pay.CustomerClaimCode, new { id = idPrefix +"CustomerClaimCode", @class = "alphanumeric", style="width:95px",maxlength="10",onchange="GetNameFromCode('pay["+i+"]_CustomerClaimCode','pay["+i+"]_CustomerClaimName','CustomerClaim')" })%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "0" });%></td>
                <td style="width: 266px"><%=Html.TextBox(namePrefix + "CustomerClaimName", pay.CustomerClaim != null ? pay.CustomerClaim.CustomerClaimName : "", new { id = idPrefix + "CustomerClaimName", @class = "readonly", @readonly = "readonly", style = "width:260px" })%></td>
                <td style="width: 87px"><%=Html.TextBox(namePrefix + "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", pay.PaymentPlanDate), new { id = idPrefix + "PaymentPlanDate", @class = "alphanumeric", style="width:80px",maxlength="10" })%></td>
                <td style="width: 89px"><%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", pay.Amount), new { id = idPrefix + "Amount", @class = "money", style="width:82px", onkeyup = "calcPaymentAmount()" })%></td>
                <td style="width: 206px"><%=Html.TextBox(namePrefix + "Memo",pay.Memo, new { id = idPrefix + "Memo", style="width:200px",maxlength="100"})%></td>
                <!--Mod 2015/08/13 arc nakayama #3240_サービス伝票のレバレート、走行距離の単位、支払合計の表示不具合 checkboxとtextboxで名称が重複していたのでcheckbox側の名称変更）-->
                <td style="width: 57px;text-align:center"><%=Html.CheckBox(namePrefix + "checkDepositFlag", pay.DepositFlag != null && pay.DepositFlag.Equals("1"), new { id = idPrefix + "DepositFlag" })%></td>
            </tr>
            <%} else { %>
            <tr>
                <% //Mod 2014/07/15 arc yano chrome対応 メソッドのパラメータをname→idに修正 %>
                <td style="width: 21px;text-align:center"><%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber" })%></td>
                <td style="width: 126px"><%=Html.TextBox(namePrefix +"CustomerClaimCode",pay.CustomerClaimCode, new { id = idPrefix +"CustomerClaimCode", @class = "alphanumeric readonly", @readonly="readonly", style="width:95px",maxlength="10"})%><%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CustomerClaimCode", idPrefix + "CustomerClaimName", "'/CustomerClaim/CriteriaDialog'", "1" });%></td>
                <td style="width: 266px"><%=Html.TextBox(namePrefix + "CustomerClaimName", pay.CustomerClaim != null ? pay.CustomerClaim.CustomerClaimName : "", new { id = idPrefix + "CustomerClaimName", @class = "readonly", @readonly = "readonly", style = "width:260px" })%></td>
                <td style="width: 87px"><%=Html.TextBox(namePrefix + "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", pay.PaymentPlanDate), new { id = idPrefix + "PaymentPlanDate", @class = "alphanumeric readonly",@readonly="readonly", style="width:80px",maxlength="10" })%></td>
                <td style="width: 89px"><%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", pay.Amount), new { id = idPrefix + "Amount", @class = "money readonly", style = "width:82px", @readonly = "readonly" })%></td>
                <td style="width: 206px"><%=Html.TextBox(namePrefix + "Memo",pay.Memo, new { id = idPrefix + "Memo", @class="readonly",@readonly="readonly", style="width:200px",maxlength="100"})%></td>
                <!--Mod 2015/08/13 arc nakayama #3240_サービス伝票のレバレート、走行距離の単位、支払合計の表示不具合 checkboxとtextboxで名称が重複していたのでcheckbox側の名称変更）-->
                <td style="width: 57px;text-align:center"><%=Html.CheckBox(namePrefix + "checkDepositFlag", pay.DepositFlag != null && pay.DepositFlag.Equals("1"), new { id = idPrefix + "DepositFlag", @disabled = "disabled" })%><%=Html.Hidden(namePrefix + "DepositFlag", pay.DepositFlag, new { id = idPrefix + "DepositFlag" })%></td>
            </tr>
            <%} %>
        <%} %>
    </table>
</div>
<%=Html.Hidden("PaymentCount",Model.ServiceSalesPayment.Count) %>
<%=Html.Hidden("DelPayLine","") %>

