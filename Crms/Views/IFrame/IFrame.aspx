<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%=ViewData["title"] %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<iframe id="Iframe" src ="<%=ViewData["url"] %>" style ="width:<%= ViewData["width"]%>px; height:<%= ViewData["height"]%>px" frameborder ="0">
</iframe>

</asp:Content>