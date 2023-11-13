<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SupplierPayment>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支払先マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "SupplierPayment", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">支払先コード</th>
        <td><%=Html.TextBox("SupplierPaymentCode", ViewData["SupplierPaymentCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>支払先名</th>
        <td><%=Html.TextBox("SupplierPaymentName", ViewData["SupplierPaymentName"], new { size = 40, maxlength = 20 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/SupplierPayment/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>支払先コード</th>
        <th>支払先名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var supplierPayment in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/SupplierPayment/Entry/' + '<%=supplierPayment.SupplierPaymentCode%>')">詳細</a></td>
        <td><%=Html.Encode(supplierPayment.SupplierPaymentCode)%></td>
        <td><%=Html.Encode(supplierPayment.SupplierPaymentName)%></td>
        <td><%=Html.Encode(supplierPayment.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
