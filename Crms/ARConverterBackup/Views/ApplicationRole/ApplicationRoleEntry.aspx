<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.Application>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	アプリケーションロール設定
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%CodeDao dao = new CodeDao(db); %>
<%using (Html.BeginForm()) { %>
<%=Html.Hidden("close", ViewData["close"])%>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<table class="input-form">
    <%  /* ADD 2014/10/28 arc ishii　保存ボタン対応 保存メッセージ出力のため*/%>
    <%=Html.ValidationSummary()%>
    <tr>
        <th>ロール</th>
        <th>閲覧レベル</th>
        <%foreach(var menu in Model){ %>

        <%  /* Mod 2014/07/07 arc amii chrome対応 spanタグ追加し、style属性にchrome & IEで縦書き表示になるようにした。
                                                  Chrome縦書き：-webkit-writing-mode
                                                  IE縦書き：writing-mode */
        %>
        <th style="text-align:center;" valign="top">
            <span style="-webkit-writing-mode:vertical-rl;writing-mode:tb-rl;direction:ltr"><%=menu.ApplicationName %></span>
        </th>
        <%} %>
    </tr>
    <%for (int j = 0; j < Model[0].ApplicationRole.Count;j++ ) {%>
    <tr>
        <th><%=Model[0].ApplicationRole[j].SecurityRole.SecurityRoleName%></th>
        <td><%=Html.DropDownList(string.Format("sec[{0}].{1}", Model[0].ApplicationRole[j].SecurityRoleCode, "SecurityLevelCode"), CodeUtils.GetSelectListByModel<c_SecurityLevel>(dao.GetSecurityLevelAll(false), Model[0].ApplicationRole[j].SecurityRole.SecurityLevelCode, false))%></td>
        <%for (int i = 0; i < Model.Count; i++) { %>
        <td><%=Html.CheckBox(string.Format("role[{0}].ApplicationRole[{1}].{2}", i,j,"EnableFlag"), Model[i].ApplicationRole[j].EnableFlag)%></td>
        <%} %>
    </tr>
    <%}%>
</table>
<%} %>
</asp:Content>
