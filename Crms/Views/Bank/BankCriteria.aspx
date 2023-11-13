<%@ Page Title="銀行マスタ検索" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Bank>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	銀行マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Bank", new { id = 0 }, FormMethod.Post)){ %>
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
            <%=Html.TextBox("BankCode", ViewData["BankCode"], new { @class = "alphanumeric", size = 30, maxlength = 4 })%>
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
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()" /></td>
    </tr>
</table>
</div>
<%} %>
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh2('/Bank/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:80px">銀行コード</th>
        <th>銀行名</th>
        <th>ステータス</th>
    </tr>
    <%foreach(var item in Model){ %>
    <tr>
        <td><a href="javascript:void(0)" onclick="openModalAfterRefresh2('/Bank/Entry/'+'<%=Html.Encode(item.BankCode) %>')"><%=Html.Encode(item.BankCode) %></a></td>
        <td><%=Html.Encode(item.BankName) %></td>
        <td><%=Html.Encode(item.DelFlagName) %></td>
    </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
