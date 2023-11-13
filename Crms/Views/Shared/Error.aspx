<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>システムエラー</title>
    <link rel="Stylesheet" href="/Content/style.css" />
</head>
<body>
<div style="margin-top:200px">
<center>
    <table style="border:solid 1px Red;background-color:#ffeeee;width:800px">
        <tr>
            <td style="font-size:medium ;padding:20px 20px 20px 20px;vertical-align:top;text-align:left">
                システムエラーが発生しました。<br />
                恐れ入りますがシステム管理者までお問い合わせください<br />
                <br />
                <br />
                <%:ViewData["ExceptionMessage"]%><br />
            </td>
        </tr>
    </table>
</center>
</div>
</body>
</html>
