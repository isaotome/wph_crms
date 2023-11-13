<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PartsLocation>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品ロケーションマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "PartsLocation", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />

<%
    //Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件に倉庫を追加
    //                                        ①データアクセス方法の変更(関連付けでのメンバのアクセスの廃止)
    //                                        ②検索条件に「倉庫コード」「倉庫名」追加
    //                                        ③クリアボタン追加
%>
<table class="input-form">
    <tr>
        <th style="width:100px">部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 25, maxlength = 25, onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')" })%>
            <img alt="部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('PartsNumber', 'PartsNameJp', '/Parts/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <th>部品名</th>
        <td style="height:20px"><span id="PartsNameJp"><%=Html.Encode(ViewData["PartsNameJp"])%></span></td>       
    </tr>
    <tr>
        <th>部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <th>部門名</th>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>       
    </tr>
    <tr><%// Add 2016/08/13 arc yano #3596 %>
       <th style="width:100px">倉庫コード</th>
        <td><%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new { @class = "alphanumeric", maxlength = 6, onchange = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')" })%>&nbsp;<img alt="倉庫コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('WarehouseCode','WarehouseName','/Warehouse/CriteriaDialog/?WarehouseName='+encodeURIComponent(document.getElementById('WarehouseName').value))" /></td>
    </tr>
    <tr>
        <th>倉庫名</th>
        <td style="height:20px"><span id="WarehouseName"><%=Html.Encode(ViewData["WarehouseName"])%></span></td>      
    </tr>
    <tr>
        <th>ロケーションコード</th>
        <td>
            <%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", maxlength = 12, onblur = "GetNameFromCode('LocationCode','LocationName','Location')" })%>
            <img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog?BusinessType=002,009')" />
        </td>
    </tr>
    <tr>
        <th>ロケーション名</th>
        <td style="height:20px"><span id="LocationName"><%=Html.Encode(ViewData["LocationName"])%></span></td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td>
            <input type="button" value="検索" onclick="displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('PartsNumber', 'PartsNameJp', 'DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'LocationCode', 'LocationName', 'DelFlag'))" />
        </td>
    </tr>
</table>
</div>
<%} %>

<br />
<% //Mod 2017/01/19 arc yano #3694  部品ロケーション　新規作成表示時のシステムエラー 倉庫コードで検索した場合でも新規作成ボタンが表示されるように変更する%>
<%if ((!string.IsNullOrEmpty((string)ViewData["PartsNumber"])) || (!string.IsNullOrEmpty((string)ViewData["DepartmentCode"])) || (!string.IsNullOrEmpty((string)ViewData["WarehouseCode"])))
  {%>
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/PartsLocation/Entry?Status=0&PartsNumber=<%=Html.Encode(ViewData["PartsNumber"])%>&DepartmentCode=<%=Html.Encode(ViewData["DepartmentCode"])%>&WarehouseCode=<%=Html.Encode(ViewData["WarehouseCode"])%>')"/>
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>部品コード</th>
        <th>部品名</th>
        <th>部門コード</th>
        <th>部門名</th>
        <th>倉庫コード</th>
        <th>倉庫名</th>
        <th>ロケーションコード</th>
        <th>ロケーション名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var partsLocation in Model)
      { %>
    <tr>
        <%//Mod 2016/10/24 arc yano #3649 構文エラーの修正%>
        <%//Mod 2016/08/13 arc yano #3596 部門から倉庫へ変更%>
        <td><a href="javascript:openModalAfterRefresh('/PartsLocation/Entry?Status=1&PartsNumber=<%=partsLocation.PartsNumber%>&WarehouseCode=<%=partsLocation.WarehouseCode%>')">詳細</a></td>
        <td><%=Html.Encode(partsLocation.PartsNumber)%></td>    <%//部品番号 %>
        <td><%=Html.Encode(partsLocation.PartsNameJp)%></td>    <%//部品名 %>
        <td><%=Html.Encode(partsLocation.DepartmentCode)%></td> <%//部門コード %>
        <td><%=Html.Encode(partsLocation.DepartmentName)%></td> <%//部門名 %>
        <td><%=Html.Encode(partsLocation.WarehouseCode)%></td>  <%//倉庫コード %>
        <td><%=Html.Encode(partsLocation.WarehouseName)%></td>  <%//倉庫名 %>
        <td><%=Html.Encode(partsLocation.LocationCode)%></td>   <%//ロケーションコード %>
        <td><%=Html.Encode(partsLocation.LocationName)%></td>   <%//ロケーション名 %>
        <td><%=partsLocation.DelFlagName%></td>                 <%//ステータス %>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
