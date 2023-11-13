<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Area>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	エリア検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Area", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">エリアコード</th>
        <td><%=Html.TextBox("AreaCode", ViewData["AreaCode"], new { @class = "alphanumeric", maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>エリア名</th>
        <td><%=Html.TextBox("AreaName", ViewData["AreaName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>エリア長名</th>
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
        <th>エリアコード</th>
        <th>エリア名</th>
        <th>エリア長名</th>
    </tr>
    <%foreach (var area in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=area.AreaCode %>','AreaName');return false;">選択</a></td>
        <td><%=Html.Encode(area.AreaCode)%></td>
        <td><span id="<%="AreaName_" + area.AreaCode%>"><%=Html.Encode(area.AreaName)%></span></td>
        <td><%if (area.Employee != null) {%><%=Html.Encode(area.Employee.EmployeeName)%><%} %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
