<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.TransportBranchOffice>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	運輸支局マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "TransportBranchOffice", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:none">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">運輸支局コード</th>
        <td><%=Html.TextBox("TransportBranchOfficeCode", ViewData["TransportBranchOfficeCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>運輸支局名</th>
        <td><%=Html.TextBox("TransportBranchOfficeName", ViewData["TransportBranchOfficeName"], new { size = 50, maxlength = 50 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/TransportBranchOffice/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>運輸支局コード</th>
        <th>運輸支局名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var transportBranchOffice in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/TransportBranchOffice/Entry/' + '<%=transportBranchOffice.TransportBranchOfficeCode%>')">詳細</a></td>
        <td><%=Html.Encode(transportBranchOffice.TransportBranchOfficeCode)%></td>
        <td><%=Html.Encode(transportBranchOffice.TransportBranchOfficeName)%></td>
        <td><%=Html.Encode(transportBranchOffice.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
