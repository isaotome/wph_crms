<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Location>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ロケーション検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Location", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%
    //Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成
    //                                                       ①検索条件、結果一覧に「倉庫」を追加
    //                                                       ②検索条件の部門コードにルックアップを追加
    //                                                       ③データのアクセス方法の変更(関連付けでのアクセスの廃止)
%>
<table class="input-form">
     <tr>
        <th style="width:100px">部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, size = 3, onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog/?DepartmentName='+encodeURIComponent(document.getElementById('DepartmentName').value))" /></td>
    </tr>
    <tr>
        <th>部門名</th>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { maxlength = 20 })%></td>
    </tr>
     <tr>
       <th style="width:100px">倉庫コード</th>
        <td><%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new { @class = "alphanumeric", maxlength = 6, onchange = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')" })%>&nbsp;<img alt="倉庫コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('WarehouseCode','WarehouseName','/Warehouse/CriteriaDialog/?WarehouseName='+encodeURIComponent(document.getElementById('WarehouseName').value))" /></td>
    </tr>
    <tr>
        <th>倉庫名</th>
        <td><%=Html.TextBox("WarehouseName", ViewData["WarehouseName"], new { size = 20, maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>ロケーションコード</th>
        <%//Mod 2016/04/26 arc yano #3502 部品入荷入力　入荷ロケーションの絞込み ロケーション種別を保持する隠し項目を追加 %>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", maxlength = 12 })%><%=Html.Hidden("LocationType",  ViewData["LocationType"]) %></td>
    </tr>
    <tr>
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<%=Html.Hidden("BusinessType", ViewData["BusinessType"])%>
<%=Html.Hidden("ConditionsHold", ViewData["ConditionsHold"])%>
<%=Html.Hidden("HoldBusinessType", ViewData["HoldBusinessType"])%>
<%} %>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>部門</th>
        <th>倉庫</th>
        <th>ロケーションコード</th>
        <th>ロケーション名</th>
        <th>ロケーション種別</th>
    </tr>
    <%foreach (var location in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=location.LocationCode %>','LocationName')">選択</a></td>
        <td><%=Html.Encode(location.DepartmentCode)%>&nbsp;<%=Html.Encode(location.DepartmentName)%></td>
        <td><%=Html.Encode(location.WarehouseCode)%>&nbsp;<%=Html.Encode(location.WarehouseName)%></td>
        <td><%=Html.Encode(location.LocationCode)%></td>
        <td><span id="<%="LocationName_" + location.LocationCode%>"><%=Html.Encode(location.LocationName)%></span></td>
        <td><%=Html.Encode(location.LocationTypeName) %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
