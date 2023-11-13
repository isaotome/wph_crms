<%@ Page Title="支店検索ダイアログ" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Branch>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	支店検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Branch", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>
            銀行コード
        </th>
        <td>
            <%=Html.TextBox("BankCode", ViewData["BankCode"], new { @class = "alphanumeric", size = 3, maxlength = 4 })%>
        </td>
    </tr>
    <tr>
        <th>
            銀行名
        </th>
        <td>
            <%=Html.TextBox("BankName", ViewData["BankName"], new { maxlength = 30 }) %>
        </td>
    </tr>
    <tr>
        <th>
            支店コード
        </th>
        <td>
            <%=Html.TextBox("BranchCode", ViewData["BranchCode"], new { @class = "alphanumeric", size = 3, maxlength = 3 }) %>
        </td>
    </tr>
    <tr>
        <th>支店名</th>
        <td>
            <%=Html.TextBox("BranchName", ViewData["BranchName"], new { size = 30, maxlength = 50 }) %>
        </td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()" /></td>
    </tr>
</table>
</div>
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th></th>
        <th>銀行コード</th>
        <th>銀行名</th>
        <th>支店コード</th>
        <th>支店名</th>
    </tr>
    <%foreach(var item in Model){ %>
        <tr>
            <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=item.BranchCode %>','BranchName');return false;">選択</a></td>
            <td><%=Html.Encode(item.BankCode) %></td>
            <td><%=Html.Encode(item.Bank != null ? item.Bank.BankName : "") %></td>
            <td><%=Html.Encode(item.BranchCode) %></td>
            <td><span id="<%="BranchName_" + item.BranchCode%>"><%=Html.Encode(item.BranchName)%></span></td>
        </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
