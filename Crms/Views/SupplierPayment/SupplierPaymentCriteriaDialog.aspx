<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SupplierPayment>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支払先検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "SupplierPayment", new { id = 0 }, FormMethod.Post)){ %>
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
        <th>支払先コード</th>
        <th>支払先名</th>
    </tr>
    <%foreach (var supplierPayment in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=supplierPayment.SupplierPaymentCode %>','SupplierPaymentName')">選択</a></td>
        <td><%=Html.Encode(supplierPayment.SupplierPaymentCode)%></td>
        <td><span id="<%="SupplierPaymentName_" + supplierPayment.SupplierPaymentCode%>"><%=Html.Encode(supplierPayment.SupplierPaymentName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
