<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceMenu>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービスメニュー検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "ServiceMenu", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">サービスメニューコード</th>
        <td><%=Html.TextBox("ServiceMenuCode", ViewData["ServiceMenuCode"], new { @class = "alphanumeric", maxlength = 8 })%></td>
    </tr>
    <tr>
        <th>サービスメニュー名</th>
        <td><%=Html.TextBox("ServiceMenuName", ViewData["ServiceMenuName"], new { size = 40, maxlength = 20 })%></td>
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
        <th>サービスメニューコード</th>
        <th>サービスメニュー名</th>
    </tr>
    <%foreach (var serviceMenu in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=serviceMenu.ServiceMenuCode %>','ServiceMenuName')">選択</a></td>
        <td><%=Html.Encode(serviceMenu.ServiceMenuCode)%></td>
        <td><span id="<%="ServiceMenuName_" + serviceMenu.ServiceMenuCode%>"><%=Html.Encode(serviceMenu.ServiceMenuName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
