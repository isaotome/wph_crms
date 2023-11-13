<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Task>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	タスク通知
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div id="input-form">
    <br />
    <div style="font-weight:bold;color:Red;font-size:12pt">新しいタスクが追加されました</div>
    <br />
    <%=Html.Encode(Model.TaskConfig !=null ? Model.TaskConfig.TaskName : "" )%>
    <br />
    <input type="button" value="確認する" onclick="returnValue='1';window.close();" />　<input type="button" value="確認しない" onclick="window.close();" />
</div>

</asp:Content>
