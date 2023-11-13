<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Office>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	事業所マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Office", new { id = 0 }, FormMethod.Post)){ %>
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
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Office/Entry')"/>
<br />
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
        <th>ステータス</th>
    </tr>
    <%foreach (var office in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/Office/Entry/' + '<%=office.OfficeCode%>')">詳細</a></td>
        <td><%=Html.Encode(office.CompanyCode)%>&nbsp;<%if (office.Company != null) {%><%=Html.Encode(office.Company.CompanyName)%><%} %></td>
        <td><%=Html.Encode(office.OfficeCode)%></td>
        <td><%=Html.Encode(office.OfficeName)%></td>
        <td><%if (office.Employee != null) {%><%=Html.Encode(office.Employee.EmployeeName)%><%} %></td>
        <td><%=Html.Encode(office.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
