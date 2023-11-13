<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %> 
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head id="Head1" runat="server">
    <title>Car Retail Management System</title>
    <link rel="Stylesheet" href="/Content/style.css" />
    <script type="text/javascript" src="/Scripts/Common.js<%=ViewData["QueryString"]%>"></script>
</head>
<%--<%if(ViewData["PasswordChanged"].Equals(true)){ %>--%>
    <body onload="<% if(ViewData["ChangedPassword"] != null && ViewData["ChangedPassword"] .Equals(true)){%><%="alert('登録しました'); document.forms[0].action = 'ChangeTransfer';  document.forms[0].submit();"%><%}else{%><%="document.forms[0].oldpwd.focus()"%><%}%>">
<%--<%}else{ %>
    <body onload="document.forms[0].oldpwd.focus()">
<%} %>--%>


            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
            <br />
         <%--   <div id="login-message">
                パスワード変更</div>
          --%>
    <!--<div style="margin:0 auto;width:30em;padding-right:0em;">-->
        <div style="margin-right: auto; margin-left:auto; width:100%;padding-left:30%;">
        <span id="login-message" style="padding-left:5em;">パスワード変更</span>
      <br />
      <%=Html.ValidationSummary()%>
      <%if (ViewData["ExpireMsg"] != null && !string.IsNullOrWhiteSpace(ViewData["ExpireMsg"].ToString())){ %>
      <span style="color:blue;"><b><%=Html.Encode(ViewData["ExpireMsg"]) %></b></span>
      <%} %>
      <br />      
            <%using (Html.BeginForm("ChangePassword", "Home", FormMethod.Post, new { onsubmit = "return checkInProcess();" })) { %>
             <%=Html.Hidden("RequestFlag", "0")%>
            <table class="input-form">
                <tr>
                    <th>現在のパスワード</th>
                    <td><%=Html.Password("oldpwd", "", new { size = "26" })%></td>
                </tr>
                <tr>
                    <th>新しいパスワード</th>
                    <td><%=Html.Password("newpwd", "", new { size = "26" })%></td>
                </tr>
                <tr>
                    <th>新しいパスワードの確認</th>
                    <td><%=Html.Password("confirmpwd", "", new { size = "26" })%></td>
                </tr>
                <tr>
                    <th></th>
                    <td><input type="button" onclick="if (confirm('登録してもよろしいですか')) { document.forms[0].submit(); }" value="登録" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);width:90px;height:18px;background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid #ffb6b6;"/>&nbsp;<input type="button" onclick="    if (confirm('キャンセルしてもよろしいですか')) { /*document.getElementById('RequestFlag').value = '1';*/ document.forms[0].method = 'get'; document.forms[0].action = '/Home/Index'; document.forms[0].submit(); }" value="キャンセル" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);width:90px;height:18px;background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid #ffb6b6;" /></td><%//Mod 2018/08/31 yano #3934 %>
                </tr>
            </table>
            <%} %>
            <br />
    </div>
    <br />
    <br /> 
</body>
</html>
