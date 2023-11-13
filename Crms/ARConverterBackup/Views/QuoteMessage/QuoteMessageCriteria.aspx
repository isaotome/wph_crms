<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.QuoteMessage>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	見積メッセージマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "QuoteMessage", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:none">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">会社コード</th>
        <td><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @class = "alphanumeric", maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>会社名</th>
        <td><%=Html.TextBox("CompanyName", ViewData["CompanyName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>見積種別</th>
        <td><%=Html.DropDownList("QuoteType", (IEnumerable<SelectListItem>)ViewData["QuoteTypeList"])%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/QuoteMessage/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>会社</th>
        <th>見積種別</th>
        <th>見積メッセージ</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var quoteMessage in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/QuoteMessage/Entry/' + '<%=quoteMessage.CompanyCode%>' + ',' + '<%=quoteMessage.QuoteType%>')">詳細</a></td>
        <td><%=Html.Encode(quoteMessage.CompanyCode)%>&nbsp;<%if (quoteMessage.Company != null) {%><%=Html.Encode(quoteMessage.Company.CompanyName)%><%} %></td>
        <td><%if (quoteMessage.c_QuoteType != null) {%><%=Html.Encode(quoteMessage.c_QuoteType.Name)%><%} %></td>
        <td><%=Html.Encode(quoteMessage.Description)%></td>
        <td><%=Html.Encode(quoteMessage.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
