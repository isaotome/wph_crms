<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_LocationListForCarUsage>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ロケーション検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialogForCarUsage", "Location", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>ロケーションコード</th>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", maxlength = 12 })%></td>
    </tr>
    <tr>
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 50, maxlength = 50 })%></td>
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
        <th>ロケーションコード</th>
        <th>ロケーション名</th>
    </tr>
    <%foreach (var location in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=location.LocationCode %>','LocationName')">選択</a></td>
        <td><%=Html.Encode(location.LocationCode)%></td>
        <td><span id='<%="LocationName_" + location.LocationCode%>'><%=Html.Encode(location.LocationName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
