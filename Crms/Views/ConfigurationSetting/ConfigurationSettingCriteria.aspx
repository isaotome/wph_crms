<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.ConfigurationSetting>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    アプリケーション設定
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("Entry","ConfigurationSetting",FormMethod.Post)){ %>
    <table class="input-form">
        <tr>
            <th>設定名</th>
            <th>値</th>
        </tr>
        <%for(int i=0;i< Model.Count;i++) { %>
        <tr>
            <td><%=Html.Encode(Model[i].Description) %><%=Html.Hidden(string.Format("data[{0}].Code", i), Model[i].Code, new { id = string.Format("data[{0}]_Code", i)})%></td>
            <td><%=Html.TextBox(string.Format("data[{0}].Value",i),Model[i].Value,new {id = string.Format("data[{0}]_Value",i), @class="alphanumeric",size="25",maxlength="50"}) %></td>
        </tr>
        <%} %>
    </table>
    <br />
    <input type="button" value="設定を保存する" onclick="formSubmit()" />
<%} %>
</asp:Content>
