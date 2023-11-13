<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetServiceSalesSlipResult>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス売上伝票発行
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","ServiceSalesSlip",FormMethod.Post)){%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %>
<%=Html.Hidden("ExcelDownloadSlipNumber", "") %>
<%=Html.Hidden("ExcelDownloadRevisionNumber", "") %>
<%=Html.Hidden("ExcelDownloadServiceWorkCode", "") %>
<%=Html.Hidden("RequestFlag", "99") %>

<%=Html.ValidationSummary() %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", size ="14", maxlength = "50" })%></td>
        <th style="width:100px">入庫日</th>
        <td><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;&nbsp;</td>
    </tr>
    <tr>
        <th style="width:100px">部門</th>
        <td><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"],new {@class="alphanumeric",maxlength="3",size="5" ,onblur="GetNameFromCodeDelflagNoCheck('DepartmentCode','DepartmentName','Department')"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CloseMonthFlag=2')" /><span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"])%></span></td>
        <th>伝票ステータス</th>
        <td><%=Html.DropDownList("ServiceOrderStatus",(IEnumerable<SelectListItem>)ViewData["ServiceOrderStatusList"]) %></td>
    </tr>
    <tr>
        <th>主作業</th>
        <td colspan="3"><%=Html.TextBox("ServiceWorkCode",ViewData["ServiceWorkCode"],new {@class="alphanumeric",size="10",maxlength="5",onchange="GetNameFromCode('ServiceWorkCode','ServiceWorkName','ServiceWork')"}) %>&nbsp;<img alt="主作業コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceWorkCode','ServiceWorkName','/ServiceWork/CriteriaDialog')" /><span id="ServiceWorkName"><%=CommonUtils.DefaultNbsp(ViewData["ServiceWorkName"])%></span></td>
    </tr>
    <tr>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", size = "10", maxlength = "10", onchange = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('CustomerName').value))" /></td>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = "30", maxlength = "80" })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SlipNumber', 'TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'ServiceOrderStatus', 'ServiceWorkCode', 'CustomerCode', 'CustomerName'))" />
        </td>
    </tr>
</table>
</div>
<br />
<div id ="UpdateMsg" style ="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%} %>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th>伝票番号</th>
        <th>伝票ステータス</th>
        <th>納車日</th>
        <th>入庫日</th>
        <th>顧客名</th>
        <th>主作業</th>
        <th>請求先</th>
        <th>部門名</th>
    </tr>
    <%  int i = 0;
        foreach (var a in Model) {
            
            string idPrefix = string.Format("line[{0}]_", i); //Add 2014/05/29 arc yano        
    %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesSlip', 'UpdateMsg');document.getElementById('ExcelDownloadSlipNumber').value='<%=a.SlipNumber%>'; document.getElementById('ExcelDownloadRevisionNumber').value='<%=a.RevisionNumber%>'; document.getElementById('ExcelDownloadServiceWorkCode').value='<%=a.ServiceWorkCode%>' ; document.getElementById('RequestFlag').value = '1'; formSubmit();document.getElementById('RequestFlag').value = '99'"><%=a.SlipNumber %></a></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.ServiceOrderStatusName)%></td>                         <%//伝票ステータス %>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}", a.SalesDate) %></td>                 <%//伝票番号 %>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}", a.ArrivalPlanDate) %></td>           <%//入庫日 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.CustomerName)%></td>                                   <%//顧客名 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.ServiceWorkName)%></td>                                <%//主作業 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.CustomerClaimName)%></td>                              <%//請求先名 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.DepartmentName)%></td>                                 <%//部門名 %>
    </tr>
    <%
       i++;
    } %>
</table>
</asp:Content>
