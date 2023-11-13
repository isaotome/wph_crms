<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品在庫実棚一括更新
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("ImportDialog", "PartsStock", FormMethod.Post, new {enctype = "multipart/form-data"})) { %>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<div id="import-form">
<br />
<table class="input-form">
    <%=Html.ValidationSummary()%>
    <tr>
        <th style="width:100px">ファイルパス</th>
        <td><input type="file" name="importFile" size="50" />&nbsp<input type="button" value="更新" onclick="document.getElementById('Message').value = '更新開始'; DisplayImage('UpdateMsg', '0'); formSubmit();"/></td>
    </tr>
    <tr>
        <th>
        </th>
        <td><%=Html.TextBox("Message", ViewData["Message"], new { size = 50, maxlength = 50 ,@readonly="readonly"})%></td>
    </tr>
</table>
</div>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />

</div>
    <%} %>
<br />
<br />
</asp:Content>
