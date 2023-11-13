<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.TransportBranchOffice>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	運輸支局マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "TransportBranchOffice", FormMethod.Post))
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
        <th style="width:200px">運輸支局コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="TransportBranchOfficeCode" name="TransportBranchOfficeCode" value="<%=Model.TransportBranchOfficeCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("TransportBranchOfficeCode", Model.TransportBranchOfficeCode, new { @class = "alphanumeric", maxlength = 10, onblur = "IsExistCode('TransportBranchOfficeCode','TransportBranchOffice')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>運輸支局名 *</th>
        <td><%=Html.TextBox("TransportBranchOfficeName", Model.TransportBranchOfficeName, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>ナンバープレート代 ノーマル(ペイント)</th>
        <td><%=Html.TextBox("NormalPaintPrice", Model.NormalPaintPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>ナンバープレート代 ノーマル(字光)</th>
        <td><%=Html.TextBox("NormalFluorescencePrice", Model.NormalFluorescencePrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>ナンバープレート代 希望(ペイント)</th>
        <td><%=Html.TextBox("OrderPaintPrice", Model.OrderPaintPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>ナンバープレート代 希望(字光)</th>
        <td><%=Html.TextBox("OrderFluorescencePrice", Model.OrderFluorescencePrice, new { @class = "numeric", maxlength = 10 })%></td>
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
