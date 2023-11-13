<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.SecurityRole>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	アプリケーションロール
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <input type="button" value="セキュリティロールを追加" style="width:200px" onclick="openModalAfterRefresh('/SecurityRole/Entry','','','no','no')" />　
    <input type="button" value="アプリケーションロールを編集" style="width:200px" onclick="openModalAfterRefresh('/ApplicationRole/Entry');" />
    <br />
    <br />
<%using (Html.BeginForm("Criteria","ApplicationRole",FormMethod.Post)){ %>
    <br />
    <table class="list">
    <tr>
        <th>セキュリティロールコード</th>
        <th>セキュリティロール名</th>
        <th>セキュリティレベル</th>
    </tr>
    <%foreach (var r in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0)" onclick="openModalAfterRefresh('/SecurityRole/Entry/<%=r.SecurityRoleCode%>','','','no','no')"><%=r.SecurityRoleCode %></a></td>
        <td><%=r.SecurityRoleName%></td>
        <td><%=r.c_SecurityLevel.Name%></td>
    </tr>
    <%} %>
<%} %>
    </table>
</asp:Content>
