<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Import Namespace="CrmsDao" %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SessionTimeout</title>
    <link rel="Stylesheet" href="/Content/style.css" />
</head>
<body>
<div style="margin-top:50%">
<center>
    <table style="border:solid 1px Red;background-color:#ffeeee;width:400px">
        <tr>
            <td>
                セッションタイムアウトまたはログインしていないため、アクセスできません。<br />
                再度ログインしなおして下さい。
                <br />
                <br />
                <%
                    //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(ViewData["closeFlag"]).Equals("1"))
                  { %>
                <a href="javascript:window.close()">画面を閉じる</a>
                <%}
                  else
                  { %>
                <a href="/Home/Index" target="_top"">ログイン画面を表示</a>
                <%} %>
                <br />
            </td>
        </tr>
    </table>
</center>
</div>
</body>
</html>
