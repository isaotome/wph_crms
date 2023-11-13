<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CustomerClaim>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	請求先マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CustomerClaim", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">請求先コード</th>
        <td><%=Html.TextBox("CustomerClaimCode", ViewData["CustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>請求先名</th>
        <td><%=Html.TextBox("CustomerClaimName", ViewData["CustomerClaimName"], new { size = 50, maxlength = 80 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CustomerClaim/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>請求先コード</th>
        <th>請求先名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var customerClaim in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/CustomerClaim/Entry/' + '<%=customerClaim.CustomerClaimCode%>')">詳細</a></td>
        <td><%=Html.Encode(customerClaim.CustomerClaimCode)%></td>
        <td><%=Html.Encode(customerClaim.CustomerClaimName)%></td>
        <td><%=Html.Encode(customerClaim.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
