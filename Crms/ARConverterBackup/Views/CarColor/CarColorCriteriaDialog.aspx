<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarColor>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両カラー検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "CarColor", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("CarGradeCode",ViewData["CarGradeCode"]) %>
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
        <th>系統色</th>
        <td><%=Html.DropDownList("ColorCategory", (IEnumerable<SelectListItem>)ViewData["ColorCategoryList"])%></td>
    </tr>
    <tr>
        <th>車両カラーコード</th>
        <td><%=Html.TextBox("CarColorCode", ViewData["CarColorCode"], new { @class = "alphanumeric", maxlength = 8 })%></td>
    </tr>
    <tr>
        <th>車両カラー名</th>
        <td><%=Html.TextBox("CarColorName", ViewData["CarColorName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>メーカーカラーコード</th>
        <td><%=Html.TextBox("MakerColorCode", ViewData["MakerColorCode"], new {size=50, maxlength = 50}) %></td>
    </tr>
    <tr>
        <th>外装色・内装色</th>
        <td><%=Html.CheckBox("InteriorColorFlag",ViewData["InteriorColorFlag"]) %>内装色　<%=Html.CheckBox("ExteriorColorFlag",ViewData["ExteriorColorFlag"]) %>外装色</td>
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
        <th>系統色</th>
        <th>車両カラーコード</th>
        <th>車両カラー名</th>
        <th>メーカーカラーコード</th>
    </tr>
    <%foreach (var carColor in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=carColor.CarColorCode %>','CarColorName')">選択</a></td>
        <td><%=Html.Encode(carColor.MakerCode)%>&nbsp;<%if (carColor.Maker != null) {%><%=Html.Encode(carColor.Maker.MakerName)%><%} %></td>
        <td><%=Html.Encode(carColor.ColorCategory)%>&nbsp;<%if (carColor.c_ColorCategory != null) {%><%=Html.Encode(carColor.c_ColorCategory.Name)%><%} %></td>
        <td><%=Html.Encode(carColor.CarColorCode)%></td>
        <td><span id="<%="CarColorName_" + carColor.CarColorCode%>"><%=Html.Encode(carColor.CarColorName)%></span></td>
        <td><%=Html.Encode(carColor.MakerColorCode) %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
