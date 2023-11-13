<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.TaskConfig>>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	TaskConfigCriteria
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%// Mod 2015/02/03 arc iijima サイズ変更　height = 800 → 685 %>
<input type="button" value="編集" onclick="openModalAfterRefresh('/TaskConfig/Entry',1000,685)" />
<br />
<br />
<%using(Html.BeginForm()){ %>
<table class="list">
    <tr>
        <th>タスクID</th>
        <th>タスク名</th>
        <th>トリガー</th>
        <th>完了条件</th>
        <th>有効/無効</th>
    </tr>
    <%foreach(var t in Model){ %>
    <tr>
        <td><%=t.TaskConfigId %></td>
        <td><%=t.TaskName %></td>
        <td><%=t.TaskTrigger %></td>
        <td><%=t.CompleteCondition %></td>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%=CommonUtils.DefaultString(t.DelFlag).Equals("1") ? "無効" : "有効" %></td>
    </tr>
    <%} %>
</table>
<%} %>
</asp:Content>
