<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Maker>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	メーカーマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Maker", new { id = 0 }, FormMethod.Post)){ %>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Maker/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>メーカーコード</th>
        <th>メーカー名</th>
        <th>略名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var maker in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/Maker/Entry/' + '<%=maker.MakerCode%>')">詳細</a></td>
        <td><%=Html.Encode(maker.MakerCode)%></td>
        <td><%=Html.Encode(maker.MakerName)%></td>
        <td><%=Html.Encode(maker.ShortName)%></td>
        <td><%=Html.Encode(maker.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
