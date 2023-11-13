<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarClass>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両クラス検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "CarClass", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">車両クラスコード</th>
        <td><%=Html.TextBox("CarClassCode", ViewData["CarClassCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>車両クラス名</th>
        <td><%=Html.TextBox("CarClassName", ViewData["CarClassName"], new { size = 40, maxlength = 20 })%></td>
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
        <th>車両クラスコード</th>
        <th>車両クラス名</th>
    </tr>
    <%foreach (var carClass in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=carClass.CarClassCode %>','CarClassName')">選択</a></td>
        <td><%=Html.Encode(carClass.CarClassCode)%></td>
        <td><span id="<%="CarClassName_" + carClass.CarClassCode%>"><%=Html.Encode(carClass.CarClassName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
