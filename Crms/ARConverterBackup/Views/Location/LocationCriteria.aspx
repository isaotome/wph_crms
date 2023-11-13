<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Location>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ロケーションマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Location", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%
    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件に倉庫を追加
    //                                        ①データアクセス方法の変更(関連付けでのメンバのアクセスの廃止)
    //                                        ②検索条件に「倉庫コード」「倉庫名」追加
    //                                        ③クリアボタンの追加
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
        <%//Mod 2021/08/03 yano #4098 無効なロケーションマスタも検索できるように修正 %>
        <th>ロケーションコード</th>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location', null, null, null, null, null, null, true)" })%>&nbsp;<img alt="ロケーションコード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog/?LocationName='+encodeURIComponent(document.getElementById('LocationName').value))" /></td>
    </tr>
    <tr>
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td>
            <input type="button" value="検索" onclick="displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'LocationCode', 'LocationName', 'DelFlag'))" />
        </td>
    </tr>
</table>
</div>
<%} %>

<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Location/Entry')"/>
<br />
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
        <th>ステータス</th>
    </tr>
    <%foreach (var location in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/Location/Entry/' + '<%=location.LocationCode%>')">詳細</a></td>
        <td><%=Html.Encode(location.DepartmentCode)%>&nbsp;<%=Html.Encode(location.DepartmentName)%></td>
        <td><%=Html.Encode(location.WarehouseCode)%>&nbsp;<%=Html.Encode(location.WarehouseName)%></td>
        <td><%=Html.Encode(location.LocationCode)%></td>
        <td><%=Html.Encode(location.LocationName)%></td>
        <td><%=Html.Encode(location.LocationTypeName) %></td>
        <td><%=Html.Encode(location.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
