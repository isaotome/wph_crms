<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarGrade>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	グレードマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarGrade", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">ブランドコード</th>
        <td><%=Html.TextBox("CarBrandCode", ViewData["CarBrandCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>車両クラスコード</th>
        <td><%=Html.TextBox("CarClassCode", ViewData["CarClassCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>車両クラス名</th>
        <td><%=Html.TextBox("CarClassName", ViewData["CarClassName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>車種コード</th>
        <td><%=Html.TextBox("CarCode", ViewData["CarCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { size = 40, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>グレードコード</th>
        <td><%=Html.TextBox("CarGradeCode", ViewData["CarGradeCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>グレード名</th>
        <td><%=Html.TextBox("CarGradeName", ViewData["CarGradeName"], new { size = 50, maxlength = 50 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarGrade/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>ブランド</th>
        <th>車両クラス</th>
        <th>車種</th>
        <th>グレードコード</th>
        <th>グレード名</th>
        <th>ステータス</th>
        <th></th>
    </tr>
    <%foreach (var carGrade in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/CarGrade/Entry/' + '<%=carGrade.CarGradeCode%>')">詳細</a></td>
        <td><%if (carGrade.Car != null) {%><%=Html.Encode(carGrade.Car.CarBrandCode)%><%} %>&nbsp;<%if (carGrade.Car != null && carGrade.Car.Brand != null) {%><%=Html.Encode(carGrade.Car.Brand.CarBrandName)%><%} %></td>
        <td><%=Html.Encode(carGrade.CarClassCode)%>&nbsp;<%if (carGrade.CarClass != null) {%><%=Html.Encode(carGrade.CarClass.CarClassName)%><%} %></td>
        <td><%=Html.Encode(carGrade.CarCode)%>&nbsp;<%if (carGrade.Car != null) {%><%=Html.Encode(carGrade.Car.CarName)%><%} %></td>
        <td><%=Html.Encode(carGrade.CarGradeCode)%></td>
        <td><%=Html.Encode(carGrade.CarGradeName)%></td>
        <td><%=Html.Encode(carGrade.DelFlagName)%></td>
        <td><input type="button" value="コピー" onclick="openModalAfterRefresh('/CarGrade/Copy/?code=<%=carGrade.CarGradeCode %>')" style="width:50px" /></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
