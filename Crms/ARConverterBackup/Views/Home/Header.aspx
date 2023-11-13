<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %>
<%
    Employee emp = (Employee)Session["Employee"];
    if (emp == null) {
%>
    <script type="text/javascript">
        alert('一定時間が過ぎたのでセッションタイムアウトが発生しました。');
        top.location.href = "/Home/Index";
    </script>
<%        
    }
    Session["Employee"] = emp;

    //Add 2023/06/09 yano #4167
    string bgfrom = ""; //背景色の設定値(from)
    string bgto = "";   //背景色の設定値(to)

    if (Session["ConnectDB"] != null)
    {

         if(Session["ConnectDB"].Equals("WPH_DB"))
         {//WPH_DB
            bgfrom = "#edf4fc";
            bgto = "#b0d3ff";
         }
         else if(Session["ConnectDB"].Equals("WE_DB"))
         {//WE_DB
            bgfrom = "#edf4fc";
            bgto = "#b0d3ff";
         }
         else if(Session["ConnectDB"].Equals("WEN_DB"))
         {//WEN_DB
            bgfrom = "#f5f5dc";
            bgto = "#dddda5";
         }
         else
         {
            //何もしない
         }
    }

    ViewData["bgFrom"] = bgfrom;
    ViewData["bgTo"] = bgto;

%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Header</title>
    <style type="text/css">
        body{
 	        width:100%;
	        font-family:'MS UI Gothic';
	        font-size:9pt;
	        background-color:#edf4fc;
       }
     <%--  #message {
	        width:100%;
	        filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#edf4fc, EndColorStr=#b0d3ff);
            background: -webkit-gradient(linear, center top, center bottom, from(#edf4fc), to(#b0d3ff)); <% //  Add 2014/07/15 arc amii chrome対応 chromeで表示したとき、背景色がついていないのを修正 %>
	        margin:0px 0px;
	        border-top:1px solid #6693cf;
	        padding:10px 0px;
	        font-size:9pt;
	        text-align:right;
	        color:#15428b;
        }--%>

        #information {
	        width:100%;
	        filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#edf4fc, EndColorStr=#b0d3ff);
            background: -webkit-gradient(linear, center top, center bottom, from(<%=ViewData["bgFrom"]%>), to(<%=ViewData["bgTo"]%>)); <%//Mod 2023/06/09 yano #4167%><% //  Add 2014/07/15 arc amii chrome対応 chromeで表示したとき、背景色がついていないのを修正 %>
            <%--background: -webkit-gradient(linear, center top, center bottom, from(#edf4fc), to(#b0d3ff));--%>
	        margin:0px 0px;
	        border-top:1px solid #6693cf;
	        /*padding:10px 0px;*/
	        font-size:9pt;
	        color:#15428b;
            height:32px;    /*Add 2023/06/09 yano #4167  */
        }
        #revision { <% // Add 2018/06/22 arc yano #3864 %>
            float: left;
	        text-align:left;
        }
        #message { <% // Add 2018/06/22 arc yano #3864 %>
	        text-align:right;
        }


    <% /* Add 2014/07/24 arc yano chrome対応*/ %>
       div#mask{
	       z-index: 10;
	        overflow: visible;
	        position: absolute;
	        top: 0px;
	        left: 0px;
	        width: 100%;
	        height: 100%;
	        background-color: black;
	        filter: alpha(opacity = 50);
	        -moz-opacity: 0.5;
	        opacity: 0.5;
	        display: none;
	        margin: 0px;
	        padding: 0px;
        }

       

    </style>    
    <script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
    <script type="text/javascript" src="/Scripts/Common.js<%=ViewData["QueryString"]%>"></script><%//Add 2018/04/09 arc yano #3757%>
    <script type="text/javascript">
        window.onload = function() {
            GetTaskCount(); //初期表示
            timerID = setInterval("GetTaskCount()", 1000 * 30); //一定間隔ごとにチェック
        }
    </script>
</head>
<body>
    <div id="information"><p id="revision"><b>Revision:<%=SubWCRev.RevisionNumber%></b></p><p id="message"><b><%=emp.Department1!=null ? emp.Department1.DepartmentName : "" %><%=emp.AdditionalDepartment1!=null ? "(" + emp.AdditionalDepartment1.DepartmentName + ")" : "" %><%=emp.AdditionalDepartment2!=null ? "(" + emp.AdditionalDepartment2.DepartmentName + ")" : "" %><%=emp.AdditionalDepartment3!=null ? "(" + emp.AdditionalDepartment3.DepartmentName + ")" : "" %>　<%=emp.EmployeeName%>さん</b>　<span style="font-weight:bold;color:Red">残タスク：<span id="TaskCount"></span>件</span>　ログイン日時:<%=emp.LastLoginDateTime %>　<a href="/Home/Index" target="_top">ログアウト</a>&nbsp;<a href="/Home/ChangePassword?userid=<%=emp.EmployeeCode%>" target="_top">パスワード変更</a></p></div><%--//2018/06/21 arc yano #3867--%>
<% //Add 2014/07/24 arc yano chrome対応 モーダルダイアログを開いている間は入力操作を受け付けないように修正。%>
<div id="mask"></div>
</body>
</html>
