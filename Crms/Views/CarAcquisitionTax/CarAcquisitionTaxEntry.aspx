<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarAcquisitionTax>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自動車税環境性能割入力<%-- Mod 2019/09/04 yano #4011 --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "CarAcquisitionTax", FormMethod.Post)) { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%// Mod 2022/01/27 yano #4125 %>
       <%if (ViewData["ButtonVisible"] != null && (bool)ViewData["ButtonVisible"].Equals(true))
         { %>
            <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%} %>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("CarAcquisitionTaxId", Model.CarAcquisitionTaxId)%>
<%=Html.ValidationSummary()%>
<br />
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:100px">経過年数(年) *</th>
        <td><%=Html.TextBox("ElapsedYears", Model.ElapsedYears, new {@class="numeric", size = "3", maxlength = "100" })%></td>
    </tr>
    <tr>
        <th>残価率 *</th>
        <td><%=Html.TextBox("RemainRate", Model.RemainRate, new { @class = "numeric", size = "10", maxlength = "11" })%></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              {%>
                <%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効
            <%} else {%>
                <%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効
            <%} %>
        </td>
  </tr>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
