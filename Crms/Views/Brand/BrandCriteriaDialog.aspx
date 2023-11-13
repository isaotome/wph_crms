<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Brand>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ブランド検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Brand", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">メーカーコード</th>
        <td><%=Html.TextBox("MakerCode", ViewData["MakerCode"], new { @class = "alphanumeric", maxlength = 5 })%></td>
    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>ブランドコード</th>
        <td><%=Html.TextBox("CarBrandCode", ViewData["CarBrandCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 50, maxlength = 50 })%></td>
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
        <th>メーカー</th>
        <th>ブランドコード</th>
        <th>ブランド名</th>
    </tr>
    <%foreach (var brand in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=brand.CarBrandCode %>','CarBrandName');return false;">選択</a></td>
        <td><%=Html.Encode(brand.MakerCode)%>&nbsp;<%if (brand.Maker != null) {%><%=Html.Encode(brand.Maker.MakerName)%><%} %></td>
        <td><%=Html.Encode(brand.CarBrandCode)%></td>
        <td><span id="<%="CarBrandName_" + brand.CarBrandCode%>"><%=Html.Encode(brand.CarBrandName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
