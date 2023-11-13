<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.MenuControl>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	MenuControlEntry
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm())
  { %>
  <div id="input-form">
    <table class="input-form">
        <tr>
            <th>メニューコード</th>
            <td><%=Html.TextBox("MenuControlCode",Model.MenuControlCode)%></td>
        </tr>
        <tr>
            <th>メニュー名</th>
            <td><%=Html.TextBox("MenuName",Model.MenuName)%></td>
        </tr>
        <tr>
            <th>メニューグループコード</th>
            <td><%=Html.TextBox("MenuGroupCode",Model.MenuGroupCode)%></td>
        </tr>
        <tr>
            <th>コントローラ名</th>
            <td><%=Html.TextBox("ControllerName",Model.ControllerName)%></td>
        </tr>
        <tr>
            <th>表示順</th>
            <td><%=Html.TextBox("DisplayOrder",Model.DisplayOrder)%></td>
        </tr>
        <tr>
            <th>アイコン</th>
            <td><%=Html.TextBox("ImageUrl",Model.ImageUrl)%></td>
        </tr>
        <tr>
            <th>削除フラグ</th>
            <td><%=Html.TextBox("DelFlag",Model.DelFlag) %></td>
        </tr>
    </table>
    <input type="submit" value="保存" />
    </div>
<%} %>
</asp:Content>
