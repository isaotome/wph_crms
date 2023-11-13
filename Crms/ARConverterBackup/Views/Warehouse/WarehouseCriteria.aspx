<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Warehouse>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
<%//Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成%>
	倉庫マスタ検索
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">


<%using (Html.BeginForm("Criteria", "Warehouse", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">倉庫管理部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')"  })%>&nbsp;<img alt="倉庫管理部門コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog/')" /></td>
    </tr>
    <tr>
        <th>倉庫管理部門名</th>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Warehouse/Entry')"/>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>倉庫管理部門</th>
        <th>倉庫コード</th>
        <th>倉庫名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var warehouse in Model)
      { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:openModalAfterRefresh('/Warehouse/Entry/' + '<%=warehouse.WarehouseCode%>')">詳細</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(warehouse.DepartmentCode)%>&nbsp;<%if (warehouse.Department != null) {%><%=Html.Encode(warehouse.Department.DepartmentName)%><%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(warehouse.WarehouseCode)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(warehouse.WarehouseName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(warehouse.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
