<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Supplier>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	仕入先マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Supplier", new { id = 0 }, FormMethod.Post)){ %>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/Supplier/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>仕入先コード</th>
        <th>仕入先名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var supplier in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/Supplier/Entry/' + '<%=supplier.SupplierCode%>')">詳細</a></td>
        <td><%=Html.Encode(supplier.SupplierCode)%></td>
        <td><%=Html.Encode(supplier.SupplierName)%></td>
        <td><%=Html.Encode(supplier.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
