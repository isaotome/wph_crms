<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Task>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	タスク詳細
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
    <tr>
        <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <%if (CommonUtils.DefaultString(Model.TaskConfig.TaskType).Equals("001") || CommonUtils.DefaultString(Model.TaskConfig.TaskType).Equals("003"))
          { %>
        <td onclick="formSubmit()"><img src="/Content/Images/build.png" alt="確認" class="command_btn" />&nbsp;確認</td>
        <%} %>
    </tr>
</table>
<br />
<%using(Html.BeginForm("Entry","Task",FormMethod.Post)){ %>
<%=Html.ValidationSummary() %>
<br />
<table class="input-form">
    <tr>
        <th style="width:200px">タスク名</th>
        <td style="width:300px"><%=Model.TaskConfig.TaskName %></td>
    </tr>
    <tr>
        <th>担当者</th>
        <td><%=Model.Employee!=null ? Model.Employee.EmployeeName : ""%></td>
    </tr>
    <tr>
        <th>部門</th>
        <td><%=Model.Department!=null ? Model.Department.DepartmentName : ""%></td>
    </tr>
    <tr>
        <th>作成日時</th>
        <td><%=Model.CreateDate %></td>
    </tr>
    <tr>
        <th>完了条件</th>
        <td><%=Model.TaskConfig.CompleteCondition %></td>
    </tr>
    <tr>
        <th>詳細</th>
        <td><%=Html.TextArea("Description", Model.Description, new { rows = 10, cols = 30 ,@readonly="readonly"})%></td>
    </tr>
    <tr>
        <th>タスク処理内容</th>
        <td><%=!string.IsNullOrEmpty(Model.TaskName) ? Model.TaskName : "" %><br /><br />
        <%switch (Model.TaskConfig.TaskType) {
              case "002"://伝票表示      
        %>
        <%if (!string.IsNullOrEmpty(Model.NavigateUrl)) { %><a href="javascript:void(0);" onclick="openModalDialog('<%=Model.NavigateUrl %>');return false;">伝票を表示して処理してください</a><%} %>
        <% break;
              case "003"://処理内容を入力%>
        <br />対応内容を記載して確認ボタンを押下してください<br /><br />
        <%=Html.TextArea("ActionResult", Model.ActionResult, new { cols = "30", rows = "5" })%>
        <% break;
              case "001"://確認のみ%>
        確認ボタンを押下してください
        <% break;
          }%></td>
    </tr>
</table>
<%=Html.Hidden("TaskId",Model.TaskId) %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("TaskType",Model.TaskConfig.TaskType) %>
<%} %>
</asp:Content>
