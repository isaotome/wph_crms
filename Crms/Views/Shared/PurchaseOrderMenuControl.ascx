<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="CrmsDao" %>
    
<%
/*--------------------------------------------------------------------------------------
 * 部品発注入力のメニューボタン
 * 更新履歴：
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規作成
 * ------------------------------------------------------------------------------------*/
%>
<table class="command">
    <tr>
        <td onclick="formClose()">
            <img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる
        </td>
        <% if (ViewData["CancelFlag"] == null || !ViewData["CancelFlag"].ToString().Equals("1")){ %>
            <%if (Model.Equals("1"))
                { %>
            <td onclick="formSubmit()">
                <img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存
            </td>
            <td onclick="document.forms[0].OrderFlag.value='1';formSubmit();">
                <img src="/Content/Images/build.png" alt="発注処理" class="command_btn" />&nbsp;発注処理
            </td>
            <% } %>
            <%else if (Model.Equals("2"))
                { %>
            <td onclick="document.forms[0].action = 'OrderCancel';formSubmit();">
                <img src="/Content/Images/cancel.png" alt="発注取消" class="command_btn" />&nbsp;発注取消
            </td>
                <td onclick="document.forms[0].action = 'Download'; document.forms[0].submit() ;document.forms[0].action = 'Entry'">
                <img src="/Content/Images/build.png" alt="Excel出力" class="command_btn" />&nbsp;Excel出力
            </td>
             <% } %>
             <%else if (Model.Equals("3"))
                { %>
                <td onclick="document.forms[0].action = 'Download'; document.forms[0].submit() ;document.forms[0].action = 'Entry'">
                <img src="/Content/Images/build.png" alt="Excel出力" class="command_btn" />&nbsp;Excel出力
            </td>
            <%} %>
        <%} %>
    </tr>
</table>