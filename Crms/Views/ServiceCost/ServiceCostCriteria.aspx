<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.ServiceMenu>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス工数表
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <input type="button" value="編集" onclick="openModalDialog('/ServiceCost/Entry')" />
    <br />
    <br />
    <table class="list">
    <tr>
        <th>サービスメニューコード</th>
        <th>サービスメニュー名</th>
    </tr>
    <%foreach (var serviceMenu in Model) { %>
    <tr>
        <td><%=Html.Encode(serviceMenu.ServiceMenuCode)%></td>
        <td><%=Html.Encode(serviceMenu.ServiceMenuName)%></td>
    </tr>
    <%} %>
    </table>
</asp:Content>
