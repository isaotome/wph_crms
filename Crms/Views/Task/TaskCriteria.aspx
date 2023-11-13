<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Task>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	TaskCriteria
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm()){ %>
<%=Html.Hidden("id","0") %>
<table class="input-form">
    <tr>
        <th>タスク種別</th>
        <td><%=Html.DropDownList("TaskConfigId",(IEnumerable<SelectListItem>)ViewData["TaskConfigList"]) %></td>
        <td><%=Html.CheckBox("TaskStatus",ViewData["TaskStatus"]) %>完了済みも表示</td>
        <td><input type="button" value="検索" onclick="formSubmit()" /></td>
    </tr>
</table>
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="width:150px">タスク作成日時</th>
        <th>タスク作成部門</th>
        <th>タスク作成者</th>
        <th>タイトル</th>
        <th>関連伝票</th>
        <th>顧客名</th>
        <th>タスク完了日時</th>
    </tr>
    <%foreach (var task in Model)
      { %>
    <tr>
        <td><%if(task.TaskCompleteDate==null){ %><a href="javascript:void(0);" onclick="openModalAfterRefresh2('/Task/Entry/<%=task.TaskId %>','','','no','no')">詳細</a><%} %></td>
        <td style="width:150px"><%=task.CreateDate%></td>
        <td><%=task.CreateEmployee!=null && task.CreateEmployee.Department1!=null ? task.CreateEmployee.Department1.DepartmentName : "" %></td>
        <td><%=task.CreateEmployee!=null ? task.CreateEmployee.EmployeeName : ""%></td>
        <td><%=task.TaskConfig!=null ? task.TaskConfig.TaskName : ""%></td>
        <td><%=task.SlipNumber %></td>
        <td><%=Html.Encode(task.CustomerName) %></td>
        <td><%=task.TaskCompleteDate %></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
