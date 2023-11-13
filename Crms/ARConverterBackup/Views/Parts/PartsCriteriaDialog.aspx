<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PartsCriteria>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Parts", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>

<% //2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 純正区分の隠し項目追加 %>
<%=Html.Hidden("GenuineType", ViewData["GenuineType"]) %>
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
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", maxlength = 25 })%></td>
    </tr>
    <tr>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 50, maxlength = 50 })%></td>
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
        <th>部品番号</th>
        <th>部品名</th>
    </tr>
    <%foreach (var parts in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=parts.PartsNumber %>','PartsNameJp')">選択</a></td>
        <td><%=Html.Encode(parts.MakerCode)%>&nbsp;<%=Html.Encode(parts.MakerName)%></td><%-- Mod 2021/02/22 yano #4083--%>
        <%--<td><%=Html.Encode(parts.MakerCode)%>&nbsp;<%if (parts.Maker != null) {%><%=Html.Encode(parts.Maker.MakerName)%><%} %></td>--%>
        <td><%=Html.Encode(parts.PartsNumber)%></td>
        <td><span id="<%="PartsNameJp_" + parts.PartsNumber%>"><%=Html.Encode(parts.PartsNameJp)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
