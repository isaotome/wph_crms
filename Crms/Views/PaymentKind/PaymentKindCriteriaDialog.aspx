<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PaymentKind>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支払種別検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "PaymentKind", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">支払種別コード</th>
        <td><%=Html.TextBox("PaymentKindCode", ViewData["PaymentKindCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>支払種別名</th>
        <td><%=Html.TextBox("PaymentKindName", ViewData["PaymentKindName"], new { size = 50, maxlength = 50 })%></td>
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
        <th>支払種別コード</th>
        <th>支払種別名</th>
        <th>手数料率</th>
        <th>締め日</th>
        <th>支払区分</th>
        <th>支払日</th>
    </tr>
    <%foreach (var paymentKind in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=paymentKind.PaymentKindCode %>','PaymentKindName')">選択</a></td>
        <td><%=Html.Encode(paymentKind.PaymentKindCode)%></td>
        <td><span id="<%="PaymentKindName_" + paymentKind.PaymentKindCode%>"><%=Html.Encode(paymentKind.PaymentKindName)%></span></td>
        <td><%=Html.Encode(paymentKind.CommissionRate)%>%</td>
        <td><%=Html.Encode(paymentKind.ClaimDayName)%></td>
        <td><%if (paymentKind.c_PaymentType != null) {%><%=Html.Encode(paymentKind.c_PaymentType.Name)%><%} %></td>
        <td><%=Html.Encode(paymentKind.PaymentDayName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
