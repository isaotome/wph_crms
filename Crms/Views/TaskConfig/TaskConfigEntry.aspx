<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.TaskConfig>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	タスク設定
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% CrmsLinqDataContext db = new CrmsLinqDataContext();%>
<%using (Html.BeginForm()) { %>
<%=Html.Hidden("close", ViewData["close"])%>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<div style="font-weight:bold">※タスクの担当範囲は各セキュリティロールで設定された閲覧範囲に準じます</div>
<br />
<table class="input-form">
    <%  /* ADD 2014/10/28 arc ishii　保存ボタン対応 保存メッセージ出力のため*/%>
    <%=Html.ValidationSummary()%>
    <tr>
        <th>タスクID</th>
        <th>タスク名</th>
        <th>有効</th>
        <th>POPUP</th>
        <%if (Model.Count > 0) { %>
        <%foreach (var role in Model[0].TaskRole) {%>

        <%  /* Mod 2014/07/07 arc amii chrome対応 spanタグ追加し、style属性にchrome & IEで縦書き表示になるようにした。
                                                  Chrome縦書き：-webkit-writing-mode
                                                  IE縦書き：writing-mode
             */
        %>
        <th style="text-align:center;" valign="top">
            <span style="-webkit-writing-mode: vertical-rl; writing-mode: tb-rl; direction: ltr"><%=role.SecurityRole.SecurityRoleName %></span>
        </th>
        <%}
          } %>
    </tr>
<%for(int i = 0;i<Model.Count;i++) {
      CodeDao dao = new CodeDao(db);
      IEnumerable<SelectListItem> list = CodeUtils.GetSelectListByModel<c_SecurityLevel>(dao.GetSecurityLevelAll(false),Model[i].SecurityLevelCode,false);
      IEnumerable<SelectListItem> popup = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), Model[i].PopUp, false);
      //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
      string namePrefix = string.Format("task[{0}].", i);
      string idPrefix = string.Format("task[{0}]_", i);
      
%>
    <tr>
        <td><%=Model[i].TaskConfigId %></td>
        <td><%=Model[i].TaskName %></td>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%=Html.CheckBox(namePrefix + "DelFlag", CommonUtils.DefaultString(Model[i].DelFlag).Equals("0"), new { id = idPrefix + "DelFlag" })%></td>
        <td><%=Html.DropDownList(namePrefix + "PopUp", popup, new { id = idPrefix + "PopUp"})%></td>
        <%for(int j=0;j<Model[i].TaskRole.Count;j++) { %>
        <td style="text-align:center;width:100px"><%=Html.CheckBox(namePrefix + string.Format("role[{1}].{2}", i, j, "EnableFlag"), Model[i].TaskRole[j].EnableFlag, new { id = idPrefix + string.Format("role[{1}]_{2}",i,j,"EnableFlag")})%></td>
        <%} %>
    </tr>
<%} %>
</table>
<%} %>
</asp:Content>
