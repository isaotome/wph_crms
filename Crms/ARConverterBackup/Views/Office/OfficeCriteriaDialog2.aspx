<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Office>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	事業所検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog2", "Office", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DivisionType",ViewData["DivisionType"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">会社コード</th>
        <td><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @class = "alphanumeric", maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>会社名</th>
        <td><%=Html.TextBox("CompanyName", ViewData["CompanyName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>事業所コード</th>
        <td><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>事業所名</th>
        <td><%=Html.TextBox("OfficeName", ViewData["OfficeName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>事業所長名</th>
        <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>会社</th>
        <th>事業所コード</th>
        <th>事業所名</th>
        <th>事業所長名</th>
    </tr>
    <%foreach (var office in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=office.OfficeCode %>','OfficeName')">選択</a></td>
        <td><%=Html.Encode(office.CompanyCode)%>&nbsp;<%if (office.Company != null) {%><%=Html.Encode(office.Company.CompanyName)%><%} %></td>
        <td><%=Html.Encode(office.OfficeCode)%></td>
        <td><span id="<%="OfficeName_" + office.OfficeCode%>"><%=Html.Encode(office.OfficeName)%></span></td>
        <td><%if (office.Employee != null) {%><%=Html.Encode(office.Employee.EmployeeName)%><%} %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
