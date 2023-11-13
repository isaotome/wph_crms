<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Warehouse>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    倉庫検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% //2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成 %>
<%using (Html.BeginForm("CriteriaDialog", "Warehouse", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>

<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>管理部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="倉庫管理部門コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog/')" /></td>
    </tr>
    <tr>
        <th>管理部門名</th>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { size = 40, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th style="width:100px">倉庫コード</th>
        <td><%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new { @class = "alphanumeric", maxlength = 6 })%></td>
    </tr>
    <tr>
        <th>倉庫名</th>
        <td><%=Html.TextBox("WarehouseName", ViewData["WarehouseName"], new { size = 20, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>管理部門</th>
        <th>倉庫コード</th>
        <th>倉庫名</th>
    </tr>
    <%foreach (var warehouse in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=warehouse.WarehouseCode %>','WarehouseName')">選択</a></td>
        <td><%=Html.Encode(warehouse.DepartmentCode)%>&nbsp;<%if (warehouse.Department != null) {%><%=Html.Encode(warehouse.Department.DepartmentName)%><%} %></td>
        <td><%=Html.Encode(warehouse.WarehouseCode)%></td>
        <td><span id='<%="WarehouseName_" + warehouse.WarehouseCode%>'><%=Html.Encode(warehouse.WarehouseName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
