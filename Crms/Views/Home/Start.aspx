<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Start</title>
    <link rel="Stylesheet" href="/Content/style.css" />
</head>
<body onload="window.open('/Home/Index','_self')">
    <div class="login">
        <br />
        <br />
        <br />
        <div style="text-align:center;font-size:12pt;">
        アプリケーション起動中.....<br />
	<br />
	自動的にログイン画面が表示されない場合は、下記のリンクをクリックしてください。
        <br />
        <br />
        
        <%=Html.ActionLink("CRMSアプリケーションの起動","Index","Home") %>
        </div>
    </div>
</body>
</html>
