<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.MenuGroup>>" %>
<%@ Import Namespace="CrmsDao" %>
<% CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<% string securityRoleCode = ((Employee)Session["Employee"]).SecurityRoleCode; %>
<% string departmentCode = ((Employee)Session["Employee"]).DepartmentCode; %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=7" />
    <title>Car Retail Management System</title>
    <link rel="stylesheet" href="/Content/menuStyle.css" type="text/css" />
    <script type="text/javascript" src="/Scripts/DefConst.js<%=ViewData["QueryString"]%>"></script><%//Add 2018/04/09 arc yano #3757%>
    <script type="text/javascript" src="/Scripts/Common.js<%=ViewData["QueryString"]%>"></script><%//Add 2018/04/09 arc yano #3757%>
</head>
<body>
<div id="accordion">
    <dl class="accordion" id="slider">
    <%foreach(var group in Model){ %>
    <%
        List<MenuControl> menuList = new MenuControlDao(db).GetAvailableListByGroupCode(group.MenuGroupCode,securityRoleCode);
        if(menuList.Count>0){
             //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            // 赤黒処理は一時的に本部機能のみとする
            if (!CommonUtils.DefaultString(departmentCode).Equals("033") && !CommonUtils.DefaultString(departmentCode).Equals("042"))
            {
                menuList = menuList.Where(x => !CommonUtils.DefaultString(x.MenuControlCode).Equals("214") && !CommonUtils.DefaultString(x.MenuControlCode).Equals("314")).ToList<MenuControl>();
            }        
    %>
            <dt><%=group.MenuGroupName%></dt>
            <dd><% foreach (var menu in menuList){%>
                <% // Mod 2014/07/03 arc amii chrome対応 画像のサイズをwidth=12 height=12に調整 %>
                <span><img src="/Content/Images/<%=menu.ImageUrl%>" alt="" width="12" height="12" />&nbsp;
                    <%if(!string.IsNullOrEmpty(menu.ShowModal) && menu.ShowModal.Equals("1")){ %>
                        <a href="javascript:void(0)" onclick="openModalDialog('<%=menu.NavigateUrl %>')"><%=menu.MenuName %></a>
                    <%}else{ %>
                        <a href="<%=menu.NavigateUrl%>" target="MainFrame"><%=menu.MenuName%></a>
                    <%} %>
                </span>
                <%} %>
            </dd>
        <%} %>
    <%} %>
    </dl>
</div>
<script type="text/javascript">

    var slider1 = new accordion.slider("slider1");
    slider1.init("slider");

</script>
<% //Add 2014/07/24 arc yano chrome対応 モーダルダイアログを開いている間は入力操作を受け付けないように修正。%>
<div id="mask"></div>
</body>
</html>