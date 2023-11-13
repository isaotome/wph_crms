<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Car Retail Management System</title>
    <link rel="Stylesheet" href="/Content/style.css" />
</head>
<frameset rows="33,*" border="0" frameborder="no">
    <frame src="/Home/Header" style="border:none;border-right:1px solid #6693cf;" name="HeaderFrame" marginwidth="0" marginheight="0" scrolling="no"><%--Add 2018/06/22 arc yano #3864--%>
    <frameset cols="203,*" border="0">
        <frame src="/Home/Menu" style="border:1px solid #6693cf;border-top:none;border-right:none;" name="MenuFrame" marginwidth="0" marginheight="0">
        <frame src="/Task/Criteria" class="frameborder" name="MainFrame" marginwidth="0" marginheight="0">
    </frameset>
</frameset>
<body></body>
</html>