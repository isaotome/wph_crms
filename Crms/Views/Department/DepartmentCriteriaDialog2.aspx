<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Department>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部門検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog3", "Department", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>
<% //2014/09/08 arc yano IPO対応その２　月締め処理状況画面／それ以外の画面により、ポスト先を変更する。%>
<%=Html.Hidden("FormAction", ViewData["FormAction"]) %>
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
        <th>部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>部門名</th>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>部門長名</th>
        <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th></th>
        <% //2014/09/08 arc yano IPO対応その２　月締め処理状況画面／それ以外の画面により、ポスト先を変更する。%>
        <td><input type="button" value="検索" onclick="if (document.getElementById('FormAction').value != '') { document.forms[0].action = document.getElementById('FormAction').value } displaySearchList()"/></td>
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
        <th>事業所</th>
        <th>部門コード</th>
        <th>部門名</th>
        <th>部門長</th>
        <th>エリア</th>
    </tr>
    <%foreach (var department in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=department.DepartmentCode %>','DepartmentName')">選択</a></td>
        <td><%if (department.Office != null) {%><%=Html.Encode(department.Office.CompanyCode)%><%} %>&nbsp;<%if ((department.Office != null) && (department.Office.Company != null)) {%><%=Html.Encode(department.Office.Company.CompanyName)%><%} %></td>
        <td><%=department.OfficeCode%>&nbsp;<%if (department.Office != null) {%><%=Html.Encode(department.Office.OfficeName)%><%} %></td>
        <td><%=Html.Encode(department.DepartmentCode)%></td>
        <td><span id="<%="DepartmentName_" + department.DepartmentCode%>"><%=Html.Encode(department.DepartmentName)%></span></td>
        <td><%if (department.Employee != null) {%><%=Html.Encode(department.Employee.EmployeeName)%><%} %></td>
        <td><%if (department.Area != null) {%><%=Html.Encode(department.Area.AreaName)%><%} %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
