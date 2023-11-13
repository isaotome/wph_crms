<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarGrade>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	グレード検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "CarGrade", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<br />
<table class="input-form">
    <tr>
        <th>型式指定</th>
        <td colspan="3"><%=Html.TextBox("ModelSpecificateNumber", ViewData["ModelSpecificateNumber"], new { @class = "alphanumeric", maxlength = 10, style = "width:200px" }) %></td>
    </tr>
    <tr>
        <th>類別区分</th>
        <td colspan="3"><%=Html.TextBox("ClassificationTypeNumber", ViewData["ClassificationTypeNumber"], new { @class = "alphanumeric", maxlength = 10, style = "width:200px" }) %></td>
    </tr>
    <tr>
        <th>ブランド・車種・型式</th>
        <td><%=Html.DropDownList("CarBrandCode", (IEnumerable<SelectListItem>)ViewData["CarBrandList"], new { size = 10, multiple = "multiple", style="width:150px", onchange="GetCarList()" }) %></td>
        <td><%=Html.DropDownList("CarCode", (IEnumerable<SelectListItem>)ViewData["CarList"], new { size = 10, style = "width:150px", multiple = "multiple", onchange = "GetModelNameList()" }) %></td>
        <td><%=Html.DropDownList("ModelName", (IEnumerable<SelectListItem>)ViewData["ModelNameList"], new { size = 10, style = "width:150px", multiple = "multiple", onchange = "displaySearchList()" }) %></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="2"><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
<%} %>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>ブランド</th>
        <th>車種</th>
        <th>グレードコード</th>
        <th>グレード名</th>
        <th>年式</th><%//2021/08/02 yano #4097 %>
        <th>型式</th>
        <th>総排気量(cc)</th>
    </tr>
    <%foreach (var carGrade in Model) { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=carGrade.CarGradeCode %>','CarGradeName')">選択</a></td>
        <td><%if (carGrade.Car != null) {%><%=Html.Encode(carGrade.Car.CarBrandCode)%><%} %>&nbsp;<%if (carGrade.Car != null && carGrade.Car.Brand != null) {%><%=Html.Encode(carGrade.Car.Brand.CarBrandName)%><%} %></td>
        <td><%=Html.Encode(carGrade.CarCode)%>&nbsp;<%if (carGrade.Car != null) {%><%=Html.Encode(carGrade.Car.CarName)%><%} %></td>
        <td><%=Html.Encode(carGrade.CarGradeCode)%></td>
        <td><span id="<%="CarGradeName_" + carGrade.CarGradeCode%>"><%=Html.Encode(carGrade.CarGradeName)%></span></td>
        <td><%=Html.Encode(carGrade.ModelYear)%></td><%//2021/08/02 yano #4097 %>
        <td><%=Html.Encode(carGrade.ModelName)%></td>
        <td style="text-align:right"><%=Html.Encode(carGrade.Displacement)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
