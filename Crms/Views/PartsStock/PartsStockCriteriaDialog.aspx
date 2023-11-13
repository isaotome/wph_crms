<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetPartsStockForDialogResult>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品在庫検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "PartsStock", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<br />
<%=Html.ValidationSummary() %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">メーカーコード</th>
        <td><%=Html.TextBox("MakerCode", ViewData["MakerCode"], new { @class = "alphanumeric", maxlength = 5})%></td>
    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>ブランドコード</th>
        <td><%=Html.TextBox("CarBrandCode",ViewData["CarBrandCode"],new {@class="alphanumeric",size="10",maxlength="30"}) %></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName",ViewData["CarBrandName"],new {size="50",maxlength="50"}) %></td>
    </tr>
    <tr>
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", maxlength = 25 })%></td>
    </tr>
    <tr>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>部門コード * </th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { size = 10, maxlength = 3, @class = "alphanumeric", onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" />&nbsp;<span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
    </tr>
    <tr>
        <th>仕入先コード</th>
        <td><%=Html.TextBox("SupplierCode",ViewData["SupplierCode"],new {@class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('SupplierCode','SupplierName','Supplier')"}) %>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" />&nbsp;<span id="SupplierName"><%=ViewData["SupplierName"] %></span></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="white-space:nowrap">メーカー</th>
        <th style="white-space:nowrap">部品番号</th>
        <th style="white-space:nowrap">部品名</th>
        <th style="white-space:nowrap">数量</th>
        <% // Add 2015/10/07 arc nakayama #3266_部品在庫検索ダイアログに仕入先の検索項目を追加 %>
        <th>仕入先名</th>
    </tr>
    <%foreach (var a in Model)
      { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:selectedCriteriaDialog('<%=a.PartsNumber %>','PartsNameJp','<%=a.PartsNameJp%>')">選択</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.MakerName) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.PartsNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.PartsNameJp) %></td>
        <td style="white-space:nowrap;text-align:right"><%=a.Quantity%></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.SupplierName) %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
