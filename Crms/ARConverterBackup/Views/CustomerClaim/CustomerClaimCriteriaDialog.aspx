<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CustomerClaim>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	請求先検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "CustomerClaim", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%//Add 2016/04/13 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み %>
<%=Html.Hidden("SWCustomerClaimClass", ViewData["SWCustomerClaimClass"])%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">請求先コード</th>
        <td><%=Html.TextBox("CustomerClaimCode", ViewData["CustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>請求先名</th>
        <td><%=Html.TextBox("CustomerClaimName", ViewData["CustomerClaimName"], new { size = 50, maxlength = 80 })%></td>
    </tr>
    <%// #3111 請求先検索ダイアログに請求種別を追加 ADD 2014/10/20 arc ishii %>
    <tr>
        <th>請求先区分</th>
        <td><%=Html.DropDownList("CustomerClaimType",(IEnumerable<SelectListItem>)ViewData["CustomerClaimTypeList"]) %></td>
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
        <th>請求先コード</th>
        <th>請求先名</th>
    </tr>
    <%foreach (var customerClaim in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=customerClaim.CustomerClaimCode %>','CustomerClaimName')">選択</a></td>
        <td><%=Html.Encode(customerClaim.CustomerClaimCode)%></td>
        <td><span id="<%="CustomerClaimName_" + customerClaim.CustomerClaimCode%>"><%=Html.Encode(customerClaim.CustomerClaimName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
