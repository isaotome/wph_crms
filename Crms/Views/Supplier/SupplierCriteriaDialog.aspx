<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Supplier>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	仕入先検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Supplier", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">仕入先コード</th>
        <td><%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>仕入先名</th>
        <td><%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>仕入先区分</th>
        <td><%=Html.DropDownList("OutsourceFlag",(IEnumerable<SelectListItem>)ViewData["OutSourceFlagList"]) %></td>
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
        <th>仕入先コード</th>
        <th>仕入先名</th>
        <th>仕入先区分</th>
    </tr>
    <%foreach (var supplier in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=supplier.SupplierCode %>','SupplierName')">選択</a></td>
        <td><%=Html.Encode(supplier.SupplierCode)%></td>
        <td><span id="<%="SupplierName_" + supplier.SupplierCode%>"><%=Html.Encode(supplier.SupplierName)%></span></td>
        <td><%=supplier.OutsourceFlagName %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
