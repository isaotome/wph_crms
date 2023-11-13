<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Car>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車種マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Car", new { id = 0 }, FormMethod.Post))
  { %>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Car/Entry')"/>
<br />
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
        <th>ステータス</th>
    </tr>
    <%foreach (var car in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/Car/Entry/' + '<%=car.CarCode%>');return false;">詳細</a></td>
        <td><%=Html.Encode(car.CarBrandCode)%>&nbsp;<%if (car.Brand != null) {%><%=Html.Encode(car.Brand.CarBrandName)%><%}%></td>
        <td><%=Html.Encode(car.CarCode)%></td>
        <td><%=Html.Encode(car.CarName)%></td>
        <td><%=Html.Encode(car.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
