<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Maker>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	メーカー検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Maker", new { id = 0 }, FormMethod.Post)){ %>
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
        <th>メーカーコード</th>
        <th>メーカー名</th>
        <th>略名</th>
    </tr>
    <%foreach (var maker in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=maker.MakerCode%>','MakerName')">選択</a></td>
        <td><%=Html.Encode(maker.MakerCode)%></td>
        <td><span id="<%="MakerName_" + maker.MakerCode%>"><%=Html.Encode(maker.MakerName)%></span></td>
        <td><%=Html.Encode(maker.ShortName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
