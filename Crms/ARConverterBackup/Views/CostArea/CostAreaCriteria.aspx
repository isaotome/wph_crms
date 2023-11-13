<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CostArea>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    諸費用設定エリア検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CostArea", FormMethod.Post)) { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CostArea/Entry')" />
    <br />
    <br />
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    <br />
    <table class="list">
        <tr>
            <th>諸費用設定エリアコード</th>
            <th>諸費用設定エリア名</th>
        </tr>
        <%foreach (CostArea area in Model) { %>
        <tr>
            <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CostArea/Entry/<%=area.CostAreaCode%>');return false;"><%=CommonUtils.DefaultNbsp(area.CostAreaCode)%></a></td>
            <td><%=CommonUtils.DefaultNbsp(area.CostAreaName)%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
