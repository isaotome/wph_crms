<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarSalesHeader>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両伝票検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("ModifyCriteria", "CarSalesOrder", new { id = "0" }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0")%>
<table class="input-form">
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"])%></td>
    </tr>
    <tr>
        <th rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>
    </tr>
    <tr>
        <th>納車日</th>
        <td><%=Html.TextBox("SalesDateFrom", ViewData["SalesDateFrom"], new { @class = "alphanumeric", style = "width:80px" })%> ～ <%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", style = "width:80px" })%></td>
    </tr>
    <tr>
        <th rowspan="2">顧客</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", onchange = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;
        <img alt="顧客検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { style = "width:250px" })%></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"])%></td>
    </tr>
    <tr>
        <th></th>
        <td>
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" /></td>--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" /></td>
    </tr>
</table>
<br />
<%} %>
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:90px;height:30px"></th>
        <th style="height:30px">伝票番号</th>
        <th style="height:30px">納車日</th>
        <th style="height:30px">部門</th>
        <th style="height:30px">担当者</th>
        <th style="height:30px">顧客名</th>
        <th style="height:30px">車台番号</th>
        <th style="height:30px">ブランド</th>
        <th style="height:30px">車種</th>
    </tr>
    <%foreach(var item in Model){ %>
    <tr>
        <!--Mod 2016/01/20 arc nakayama #3304_赤黒伝票作成のタイミングについて 　　締めのチェックを外す-->
        <td style="height:30px;text-align:center;white-space:nowrap">
            <%if(item.IsCreated){ %>
            処理済み
            <%}else{ %>
            <button type="button" style="width:27px;height:26px;" onclick="openModalDialog('/CarSalesOrder/Entry?Mode=1&SlipNo=<%=item.SlipNumber %>')"><img alt="赤" src="/Content/images/aka.jpg" /></button>            
            &nbsp;<button type="button" style="width:45px;height:26px;" onclick="openModalDialog('/CarSalesOrder/Entry?Mode=2&SlipNo=<%=item.SlipNumber %>')"><img alt="赤黒" src="/Content/images/akakuro.jpg" /></button>
            <%} %>
        </td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.SlipNumber) %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", item.SalesDate)) %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.Department!=null ? item.Department.DepartmentName : "") %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.Employee != null ? item.Employee.EmployeeName : "") %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.Customer != null ? item.Customer.CustomerName : "") %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.Vin) %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.CarBrandName) %></td>
        <td style="white-space:nowrap;height:30px"><%=Html.Encode(item.CarName) %></td>
    </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
