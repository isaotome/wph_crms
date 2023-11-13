<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CostArea>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	諸費用設定エリア検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CostArea", FormMethod.Post)) { %>
    <br />
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    <br />
    <table class="list">
        <tr>
            <th></th>
            <th>諸費用設定エリアコード</th>
            <th>諸費用設定エリア名</th>
        </tr>
        <%foreach (CostArea area in Model) { %>
        <tr>
            <td><a href="javascript:selectedCriteriaDialog('<%=area.CostAreaCode %>','CostAreaName')">選択</a></td>
            <td><%=CommonUtils.DefaultNbsp(area.CostAreaCode)%></td>
            <td><span id="<%="CostAreaName_" + area.CostAreaCode%>"><%=CommonUtils.DefaultNbsp(area.CostAreaName)%></span></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
