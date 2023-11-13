<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Loan>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ローン検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Loan", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
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
        <td><%=Html.TextBox("CustomerClaimName", ViewData["CustomerClaimName"], new { size = 40, maxlength = 80 })%></td>
    </tr>
    <tr>
        <th>ローンコード</th>
        <td><%=Html.TextBox("LoanCode", ViewData["LoanCode"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>ローン名</th>
        <td><%=Html.TextBox("LoanName", ViewData["LoanName"], new { size = 40, maxlength = 20 })%></td>
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
        <th>請求先</th>
        <th>ローンコード</th>
        <th>ローン名</th>
    </tr>
    <%foreach (var loan in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=loan.LoanCode %>','LoanName')">選択</a></td>
        <td><%=Html.Encode(loan.CustomerClaimCode)%>&nbsp;<%if (loan.CustomerClaim != null) {%><%=Html.Encode(loan.CustomerClaim.CustomerClaimName)%><%} %></td>
        <td><%=Html.Encode(loan.LoanCode)%></td>
        <td><span id="<%="LoanName_" + loan.LoanCode%>"><%=Html.Encode(loan.LoanName)%></span></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
