<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PartsCriteria>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Parts", new { id = 0 }, FormMethod.Post)){ %>
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
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", maxlength = 25 })%>
            <br />※部品番号のみの検索は時間がかかります。他の検索項目と併せてご利用ください。</td>
    </tr>
    <tr>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 50, maxlength = 50 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Parts/Entry')"/>
<% // Mod 2018/05/22 arc yano #3887 Excel取込(部品価格改定) 文言変更 %>
<% // Add 2014/09/16 arc amii 部品価格一括更新対応 ボタン追加 %>
<input type="button" value="部品価格一括取込" onclick="openModalAfterRefresh('/Parts/ImportDialog')"/>
<br />
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
        <th>ステータス</th>
    </tr>
    <%foreach (var parts in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/Parts/Entry/' + '<%=parts.PartsNumber%>')">詳細</a></td>
        <td><%=Html.Encode(parts.MakerCode)%>&nbsp;<%=Html.Encode(parts.MakerName)%></td>
        <%--<td><%=Html.Encode(parts.MakerCode)%>&nbsp;<%if (parts.Maker != null) {%><%=Html.Encode(parts.Maker.MakerName)%><%} %></td>--%>
        <td><%=Html.Encode(parts.PartsNumber)%></td>
        <td><%=Html.Encode(parts.PartsNameJp)%></td>
        <td><%=Html.Encode(parts.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
