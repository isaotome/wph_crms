<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.MenuControl>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	MenuControlCriteria
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm()){ %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>メニュー表示名</th>
        <td><%=Html.TextBox("MenuName") %></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()" /></td>
    </tr>
</table>
</div>
<%} %>
<br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/MenuControl/Entry',500,500,false,false)" />
<br />
<table class="list">
    <tr>
        <th></th>
        <th>メニューコード</th>
        <th>メニュー表示名</th>
        <th>メニューグループ名</th>
    </tr>
    <%foreach(var m in Model){ %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/MenuControl/Entry/<%=m.MenuControlCode%>',500,500,false,false)">詳細</a></td>
        <td><%=m.MenuControlCode %></td>
        <td><%=m.MenuName %></td>
        <td><%=m.MenuGroup.MenuGroupName %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
