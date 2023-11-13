<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %> 
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Car Retail Management System</title>
    <link rel="Stylesheet" href="/Content/style.css" />
    <script type="text/javascript" src="/Scripts/Common.js<%=ViewData["QueryString"]%>"></script><%//Add 2018/04/09 arc yano #3757%>
</head>
<body onload="document.forms[0].userid.focus()">
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <div id="login-message">
                ログインしてください</div>
            <br />
    <% // Add 2014/07/09 arc amii chrome対応 divのstyle属性で画面が中央寄せにならないのを修正 %>
    <div style="margin:0 auto;width:10em;padding-right:10em;">
            <%using (Html.BeginForm("Index", "Home", FormMethod.Post, new { onsubmit = "return checkInProcess();" })) { %>
            <table class="input-form">
                <tr>
                    <th>ユーザーID</th>
                    <td><%=Html.TextBox("userid", ViewData["userid"], new { size = "20", maxlength = "50" })%></td>
                </tr>
                <tr>
                    <th>パスワード</th>
                    <td><%=Html.Password("password", "", new { size = "26" })%></td>
                </tr>
                <tr>
                    <th></th>
                    <td><input type="submit" value="ログイン" /></td>
                </tr>
            </table>
            <%} %>
            <br />
            <%=Html.ValidationSummary()%>
    </div>
    <br />
    <br />
    <% //Mod 2015/10/08 arc nakayama #3271_変更予告画面の追加 %>
    <div class="login-form">
        <div class="login-form-title">
        <table class="input-form">
            <tr>
                <th style="width:500px">変更予告</th>
            </tr>
        </table>
        </div>
        <div class="login-form-title">
        <table class="input-form">
            <tr>
                <th style="width:500px">変更履歴</th>
            </tr>
        </table>
        </div>
    </div>
    <% //Add 2014/09/04 arc amii 変更履歴追加対応 #3081 変更履歴 %>
    <div class="login-form">
         <div class="login-form-1"> 
            <table class="input-form">
                <tr>
                    <% string strChangePlan = CommonUtils.DefaultString(ViewData["ChangePlan"]); %>
                    <td><%=strChangePlan%></td>
                </tr>
            </table>
        </div>
        <div class="login-form-1">
            <table class="input-form">
                <tr>
                    <% string strHistory = CommonUtils.DefaultString(ViewData["history"]); %>
                    <% //Mdd 2015/01/30 arc iijima　更新履歴サイズ変更 %>
                    <td><%=strHistory%></td>
                </tr>
            </table>
        </div>
    </div>
</body>
</html>
