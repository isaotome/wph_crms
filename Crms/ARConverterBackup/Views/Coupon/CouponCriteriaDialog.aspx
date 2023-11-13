<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Coupon>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	クーポン検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Coupon", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">ブランドコード</th>
        <td><%=Html.TextBox("CarBrandCode", ViewData["CarBrandCode"], new { @class = "alphanumeric", size = 30, maxlength = 30 })%></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>クーポンコード</th>
        <td><%=Html.TextBox("CouponCode", ViewData["CouponCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>クーポン名</th>
        <td><%=Html.TextBox("CouponName", ViewData["CouponName"], new { size = 50, maxlength = 50 })%></td>
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
        <th>ブランド</th>
        <th>クーポンコード</th>
        <th>クーポン名</th>
    </tr>
    <%foreach (var coupon in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=coupon.CouponCode %>','CouponName')">選択</a></td>
        <td><%=Html.Encode(coupon.CarBrandCode)%>&nbsp;<%if (coupon.Brand != null) {%><%=Html.Encode(coupon.Brand.CarBrandName)%><%} %></td>
        <td><%=Html.Encode(coupon.CouponCode)%></td>
        <td><span id="<%="CouponName_" + coupon.CouponCode%>"><%=Html.Encode(coupon.CouponName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
