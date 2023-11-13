<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.SecurityRole>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	セキュリティロール入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div id="input-form">
<%using(Html.BeginForm("Entry","SecurityRole",FormMethod.Post)){ %>
<%=Html.Hidden("close", ViewData["close"])%>
<%=Html.Hidden("DelFlag",Model.DelFlag) %>
<%=Html.Hidden("update",ViewData["update"]) %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%if(ViewData["update"]!=null && ViewData["update"].Equals("1")){ %>
       <td onclick="document.forms[0].DelFlag.value='1';formSubmit()"><img src="/Content/Images/cancel.png" alt="削除" class="command_btn" />&nbsp;削除</td>
       <%} %>
   </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<br />
<div>閲覧範囲はタスクの設定・通知の範囲にも適用されます</div>
<br />
<table class="input-form">
<tr>
    <th>セキュリティロールコード</th>
    <td><%if(ViewData["update"] != null && ViewData["update"].Equals("1")){ %>
            <%=Html.TextBox("SecurityRoleCode",Model.SecurityRoleCode,new {@readonly="readonly"}) %>
        <%}else{ %>
            <%=Html.TextBox("SecurityRoleCode",Model.SecurityRoleCode, new { maxlength="50",onblur = "IsExistCode('SecurityRoleCode','SecurityRole','IsExistSecurityRole')" })%>
        <%} %>
    </td>
        
</tr>
<tr>
    <th>セキュリティロール名</th>
    <% // 2014/07/16 arc amii 既存バグ対応 最大文字数の制限がない為、システムエラーになっていたのを制限を入れることでエラーにならないよう修正 %>
    <td><%=Html.TextBox("SecurityRoleName",Model.SecurityRoleName, new { maxlength="50"}) %></td>
</tr>
<tr>
    <th>閲覧範囲</th>
    <td><%=Html.DropDownList("SecurityLevelCode",(IEnumerable<SelectListItem>)ViewData["SecurityLevelList"]) %></td>
</tr>
</table>
<%} %>
</div>
</asp:Content>
