<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.DepartmentWarehouse>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
<%//Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成%>
	部門・倉庫組合せマスタ検索
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


<%using (Html.BeginForm("Criteria", "DepartmentWarehouse", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')"  })%>&nbsp;<img alt="部門コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog/')" /></td>
    </tr>
    <tr>
        <th>部門名</th>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { size = 20, maxlength = 20 })%></td>
    </tr>
    <tr>
       <th style="width:100px">倉庫コード</th>
        <td><%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new { @class = "alphanumeric", maxlength = 6, onchange = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')" })%>&nbsp;<img alt="倉庫コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('WarehouseCode','WarehouseName','/Warehouse/CriteriaDialog/')" /></td>
    </tr>
    <tr>
        <th>倉庫名</th>
        <td><%=Html.TextBox("WarehouseName", ViewData["WarehouseName"], new { size = 20, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DelFlag'))" />
        </td>
    </tr>
</table>
</div>
<%} %>

<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/DepartmentWarehouse/Entry')"/>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>部門コード</th>
        <th>部門名</th>
        <th>倉庫コード</th>
        <th>倉庫名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var departmentwarehouse in Model)
      { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:openModalAfterRefresh('/DepartmentWarehouse/Entry/' + '?departmentCode=<%=departmentwarehouse.DepartmentCode%>&warehouseCode=<%=departmentwarehouse.WarehouseCode%>')">詳細</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(departmentwarehouse.DepartmentCode)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(departmentwarehouse.Department != null ? departmentwarehouse.Department.DepartmentName : "")%></td>
        <td style="white-space:nowrap"><%=Html.Encode(departmentwarehouse.WarehouseCode)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(departmentwarehouse.Warehouse != null ? departmentwarehouse.Warehouse.WarehouseName : "")%></td>
        <td style="white-space:nowrap"><%=Html.Encode(departmentwarehouse.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
