<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CustomerClaimable>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	決済条件マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CustomerClaimable", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("key1", "") %>
<%=Html.Hidden("key2", "") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th rowspan="2" style="width:100px">請求先</th>
        <td><%=Html.TextBox("CustomerClaimCode", ViewData["CustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim')" })%>
            <img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerClaimCode', 'CustomerClaimName', '/CustomerClaim/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="CustomerClaimName"><%=Html.Encode(ViewData["CustomerClaimName"])%></span></td>       
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>

<br />
<%if (!string.IsNullOrEmpty((string)ViewData["CustomerClaimCode"]))
  {%>
<input type="button" value="追加" onclick="openModalAfterRefresh('/CustomerClaimable/Entry/' + '<%=ViewData["CustomerClaimCode"]%>')"/>
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>支払種別コード</th>
        <th>支払種別名</th>
    </tr>
    <%foreach (var customerClaimable in Model)
      { %>
    <tr>
        <td><a href="javascript:removeRelation('<%=customerClaimable.CustomerClaimCode%>', '<%=customerClaimable.PaymentKindCode%>')">削除</a></td>
        <td><%=Html.Encode(customerClaimable.PaymentKindCode)%></td>
        <td><%if (customerClaimable.PaymentKind != null) {%><%=Html.Encode(customerClaimable.PaymentKind.PaymentKindName)%><%} %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
