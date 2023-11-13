<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.SupplierPayment>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支払先マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "SupplierPayment", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:150px">支払先コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td colspan="3"><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="SupplierPaymentCode" name="SupplierPaymentCode" value="<%=Model.SupplierPaymentCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("SupplierPaymentCode", Model.SupplierPaymentCode, new { @class = "alphanumeric", maxlength = 10, onblur = "IsExistCode('SupplierPaymentCode','SupplierPayment')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>支払先名 *</th>
        <td colspan="3"><%=Html.TextBox("SupplierPaymentName", Model.SupplierPaymentName, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>支払先種別 *</th>
        <td colspan="3"><%=Html.DropDownList("SupplierPaymentType", (IEnumerable<SelectListItem>)ViewData["SupplierPaymentTypeList"])%></td>
      </tr>
      <tr>
        <th>支払区分 *</th>
        <td colspan="3"><%=Html.DropDownList("PaymentType", (IEnumerable<SelectListItem>)ViewData["PaymentType2List"], new { onchange = "changePaymentType();" })%>
            <%if (CommonUtils.DefaultString(Model.PaymentType).Equals("003"))
              {%>
              <%=Html.TextBox("PaymentDayCount", Model.PaymentDayCount, new { @class = "numeric", maxlength = 3, size=5, onfocus = "focusPaymentDayCount()" })%>
            <%}
              else
              {%>
              <%=Html.TextBox("PaymentDayCount", Model.PaymentDayCount, new { @class = "numeric", maxlength = 3, size=5, @readonly = true, style = "background-color:#e0e0e0", onfocus = "focusPaymentDayCount()" })%>
            <%} %>
            日後(5～240)
        </td>
      </tr>
      <tr>
        <th>支払日</th>
        <td colspan="3"><%=Html.TextBox("PaymentDay", Model.PaymentDay, new { @class = "numeric", maxlength = 6, size=5 })%>※月末払いの場合は0を入力して下さい</td>
      </tr>
      <tr>
        <th>猶予日数1 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod1", Model.PaymentPeriod1, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th style="width:150px">発生金利1 (%)</th>
        <td><%=Html.TextBox("PaymentRate1", Model.PaymentRate1, new { @class = "numeric", maxlength = 6, size = 5 })%></td>
      </tr>
      <tr>
        <th>猶予日数2 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod2", Model.PaymentPeriod2, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th>発生金利2 (%)</th>
        <td><%=Html.TextBox("PaymentRate2",Model.PaymentRate2, new { @class = "numeric", maxlength = 6, size = 5 }) %></td>
      </tr>
      <tr>
        <th>猶予日数3 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod3", Model.PaymentPeriod3, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th>発生金利3 (%)</th>
        <td><%=Html.TextBox("PaymentRate3", Model.PaymentRate3, new { @class = "numeric", maxlength = 6, size = 5 })%></td>
      </tr>
      <tr>
        <th>猶予日数4 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod4", Model.PaymentPeriod4, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th>発生金利4 (%)</th>
        <td><%=Html.TextBox("PaymentRate4", Model.PaymentRate4, new { @class = "numeric", maxlength = 6, size = 5 })%></td>
      </tr>
      <tr>
        <th>猶予日数5 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod5", Model.PaymentPeriod5, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th>発生金利5 (%)</th>
        <td><%=Html.TextBox("PaymentRate5",Model.PaymentRate5, new { @class = "numeric", maxlength = 6, size = 5 }) %></td>
      </tr>
      <tr>
        <th>猶予日数6 (日以上)</th>
        <td><%=Html.TextBox("PaymentPeriod6", Model.PaymentPeriod6, new { @class = "numeric", maxlength = 3, size = 5 })%></td>
        <th>発生金利6 (%)</th>
        <td><%=Html.TextBox("PaymentRate6", Model.PaymentRate6, new { @class = "numeric", maxlength = 6, size = 5 })%></td>
      </tr>
      <tr>
        <th>ステータス</th>
           <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td colspan="3"><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td colspan="3"><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
