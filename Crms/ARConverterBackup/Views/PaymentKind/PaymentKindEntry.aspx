<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaymentKind>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支払種別マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "PaymentKind", FormMethod.Post))
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
        <th style="width:100px">支払種別コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="PaymentKindCode" name="PaymentKindCode" value="<%=Model.PaymentKindCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("PaymentKindCode", Model.PaymentKindCode, new { @class = "alphanumeric", maxlength = 10, onblur = "IsExistCode('PaymentKindCode','PaymentKind')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>支払種別名 *</th>
        <td><%=Html.TextBox("PaymentKindName", Model.PaymentKindName, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>手数料率 *</th>
        <td><%=Html.TextBox("CommissionRate", Model.CommissionRate, new { @class = "numeric", maxlength = 9 })%>※小数点以下第5位まで入力可</td>
      </tr>
      <tr>
        <th>締め日 *</th>
        <td><%=Html.TextBox("ClaimDay", Model.ClaimDay, new { @class = "numeric", maxlength = 2 })%>※月末締めの場合は0を入力して下さい</td>
      </tr>
      <tr>
        <th>支払区分 *</th>
        <td><%=Html.DropDownList("PaymentType", (IEnumerable<SelectListItem>)ViewData["PaymentTypeList"])%></td>
      </tr>
      <tr>
        <th>支払日 *</th>
        <td><%=Html.TextBox("PaymentDay", Model.PaymentDay, new { @class = "numeric", maxlength = 2 })%>※月末払いの場合は0を入力して下さい</td>
      </tr>
      <tr>
        <th>ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
