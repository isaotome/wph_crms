<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Car>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車種検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Car", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">ブランドコード</th>
        <td><%=Html.TextBox("CarBrandCode", ViewData["CarBrandCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>車種コード</th>
        <td><%=Html.TextBox("CarCode", ViewData["CarCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { size = 40, maxlength = 20 })%></td>
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
        <th>ブランド</th>
        <th>車種コード</th>
        <th>車種名</th>
    </tr>
    <%foreach (var car in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=car.CarCode %>','CarName');return false;">選択</a></td>
        <td><%=Html.Encode(car.CarBrandCode)%>&nbsp;<%if (car.Brand != null) {%><%=Html.Encode(car.Brand.CarBrandName)%><%}%></td>
        <td><%=Html.Encode(car.CarCode)%></td>
        <td><span id="<%="CarName_" + car.CarCode%>"><%=Html.Encode(car.CarName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
