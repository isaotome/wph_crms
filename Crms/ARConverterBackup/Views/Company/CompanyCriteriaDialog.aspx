<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Company>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	会社検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Company", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
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
        <th>代表者名</th>
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
        <th>会社コード</th>
        <th>会社名</th>
        <th>代表者名</th>
    </tr>
    <%foreach (var company in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=company.CompanyCode %>','CompanyName')">選択</a></td>
        <td><%=Html.Encode(company.CompanyCode)%></td>
        <td><span id="<%="CompanyName_" + company.CompanyCode%>"><%=Html.Encode(company.CompanyName)%></span></td>
        <td><%if (company.Employee != null) {%><%=Html.Encode(company.Employee.EmployeeName)%><%}%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
