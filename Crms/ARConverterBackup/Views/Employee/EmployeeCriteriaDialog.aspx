<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Employee>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	社員検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Employee", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">社員コード</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>社員番号</th>
        <td><%=Html.TextBox("EmployeeNumber", ViewData["EmployeeNumber"], new { @class = "alphanumeric", maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>社員名</th>
        <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>部門名</th>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>セキュリティロールコード</th>
        <td><%=Html.TextBox("SecurityRoleCode", ViewData["SecurityRoleCode"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>セキュリティロール名</th>
        <td><%=Html.TextBox("SecurityRoleName", ViewData["SecurityRoleName"], new { size = 50, maxlength = 50 })%></td>
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
        <th>社員コード</th>
        <th>社員番号</th>
        <th>社員名</th>
        <th>部門</th>
        <th>セキュリティロール</th>
    </tr>
    <%foreach (var employee in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=employee.EmployeeCode %>','EmployeeName')">選択</a></td>
        <td><%=Html.Encode(employee.EmployeeCode)%></td>
        <td><%=Html.Encode(employee.EmployeeNumber)%></td>
        <td><span id="<%="EmployeeName_" + employee.EmployeeCode.Replace(".", "_")%>"><%=Html.Encode(employee.EmployeeName)%></span></td>
        <td><%=Html.Encode(employee.DepartmentCode)%>&nbsp;<%if (employee.Department1 != null) {%><%=Html.Encode(employee.Department1.DepartmentName)%><%} %></td>
        <td><%=Html.Encode(employee.SecurityRoleCode)%>&nbsp;<%if (employee.SecurityRole != null) {%><%=Html.Encode(employee.SecurityRole.SecurityRoleName)%><%} %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
