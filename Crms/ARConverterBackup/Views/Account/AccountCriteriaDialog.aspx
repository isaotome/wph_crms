<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Account>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	科目検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Account", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("CreditFlag",ViewData["CreditFlag"]) %>
<%=Html.Hidden("DebitFlag",ViewData["DebitFlag"]) %>
<%} %>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="width:60px">科目コード</th>
        <th>科目名</th>
        <th>補助科目名</th>
        <th>適用例</th>
    </tr>
    <%foreach (var account in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=account.AccountCode %>','AccountName');return false;">選択</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(account.AccountCode)%></td>
        <td style="white-space:nowrap"><span id="<%="AccountName_" + account.AccountCode%>"><%=Html.Encode(account.AccountName)%></span></td>
        <td><%=Html.Encode(account.SubAccountName)%></td>
        <td><%=Html.Encode(account.Notes)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
