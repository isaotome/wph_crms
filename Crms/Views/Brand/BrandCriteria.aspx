<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Brand>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ブランドマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Brand", new { id = 0 }, FormMethod.Post)){ %>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Brand/Entry')"/>
<br />
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
        <th>レバレート</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var brand in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/Brand/Entry/' + '<%=brand.CarBrandCode%>');return false;">詳細</a></td>
        <td><%=Html.Encode(brand.MakerCode)%>&nbsp;<%if (brand.Maker != null) {%><%=Html.Encode(brand.Maker.MakerName)%><%} %></td>
        <td><%=Html.Encode(brand.CarBrandCode)%></td>
        <td><%=Html.Encode(brand.CarBrandName)%></td>
        <td><%=Html.Encode(string.Format("{0:N0}",brand.LaborRate)) %></td>
        <td><%=Html.Encode(brand.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
