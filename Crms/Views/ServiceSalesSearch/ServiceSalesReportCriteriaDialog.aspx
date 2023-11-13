<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceSalesAmount>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    サービス集計表
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("ServiceSalesReportCriteriaDialog", "ServiceSalesSearch", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("FormAction", ViewData["FormAction"]) %>
<%=Html.Hidden("DefaultSalesDateFrom", ViewData["DefaultSalesDateFrom"]) %>
<%=Html.Hidden("DefaultSalesDateTo", ViewData["DefaultSalesDateTo"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:CSV出力)%>
<script type="text/javascript">
    (function ($, window, undefined) {
        $(function () {
            $('.list tr:even').addClass('even');
        });
    }(jQuery, window));
</script>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">納車年月日</th>
        <td colspan="3"><%=Html.TextBox("SalesDateFrom",ViewData["SalesDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('SalesDateFrom').value, 'SalesDateFrom')" }) %>～<%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 ,onchange ="return chkDate3(document.getElementById('SalesDateTo').value, 'SalesDateTo')"}) %></td>
    </tr>
    <tr>
        <th>区分</th>
        <td><%=Html.DropDownList("WorkType", (IEnumerable<SelectListItem>)ViewData["WorkTypeList"])%></td>
        <th>部門コード</th>
        <td style="width:200px;"><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department','false')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CloseMonthFlag=2')"/><span id="DepartmentName" style ="width:160px"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">伝票ステータスは全て納車済みになっています</td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SalesDateFrom', 'SalesDateTo', 'WorkType', 'DepartmentCode', 'DepartmentName'));"/>
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CustomerDataList', 'UpdateMsg'); displaySearchList(); document.forms[0].submit(); inProcess = 0"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl",Model.plist.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="white-space:nowrap">納車日</th>
        <th style="white-space:nowrap">見積日</th>
        <th style="white-space:nowrap">入庫日</th>
        <th style="white-space:nowrap">作業終了日</th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">主作業コード</th>
        <th style="white-space:nowrap">区分</th>
        <th style="white-space:nowrap">主作業名</th>
        <th style="white-space:nowrap">部門コード</th>
        <th style="white-space:nowrap">部門名</th>
        <th style="white-space:nowrap">工賃売上</th>
        <th style="white-space:nowrap">部品売上</th>
        <th style="white-space:nowrap">消費税</th>
        <th style="white-space:nowrap">売上合計</th>
        <th style="white-space:nowrap">工賃原価</th>
        <th style="white-space:nowrap">部品原価</th>
        <th style="white-space:nowrap">原価合計</th>
        <th style="white-space:nowrap">工賃粗利</th>
        <th style="white-space:nowrap">部品粗利</th>
        <th style="white-space:nowrap">粗利合計</th>
        <th style="white-space:nowrap">フロント担当者</th>
        <th style="white-space:nowrap">メカニック担当者</th>
        <th style="white-space:nowrap">請求先名</th>

    </tr>
    <%foreach (var line in Model.plist){ %>
    <tr>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.SalesDate))%></td><!--納車日-->
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.QuoteDate))%></td><!--見積日-->
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.ArrivalPlanDate))%></td><!--入庫日-->
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd hh:mm:ss}",line.WorkingEndDate))%></td><!--作業終了日-->
        <td style="white-space:nowrap"><%=Html.Encode(line.SlipNumber)%></td><!--伝票番号-->
        <td style="white-space:nowrap"><%=Html.Encode(line.ServiceWorkCode)%></td><!--主作業コード-->
        <td style="white-space:nowrap"><%=Html.Encode(line.WorkTypeName)%></td><!--区分-->
        <td style="white-space:nowrap"><%=Html.Encode(line.ServiceWorkName)%></td><!--主作業名-->
        <td style="white-space:nowrap"><%=Html.Encode(line.DepartmentCode)%></td><!--部門コード-->
        <td style="white-space:nowrap"><%=Html.Encode(line.DepartmentName)%></td><!--部門名-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.ServiceAmount))%></td><!--工賃売上-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.PartsAmount))%></td><!--部品売上-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.TaxAmount))%></td><!--消費税-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.TotalAmount))%></td><!--売上合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.ServiceCost))%></td><!--工賃原価-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.PartsCost))%></td><!--部品原価-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.TotalCost))%></td><!--原価合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.ServiceProfits))%></td><!--工賃粗利-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.PartsProfits))%></td><!--部品粗利-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",line.TotalProfits))%></td><!--粗利合計-->
        <td style="white-space:nowrap"><%=Html.Encode(line.FrontEmployeeName)%></td><!--フロント担当者-->
        <td style="white-space:nowrap"><%=line.MechanicEmployeeName%></td><!--メカニック担当者-->
        <td style="white-space:nowrap"><%=Html.Encode(line.CustomerClaimName)%></td><!--請求先名-->
    </tr>
    <%} %>
    <%//合計を表示%>
    <tr>
        <th colspan="9"></th>
        <th>合計</th>
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalServiceAmount))%></td> <!--工賃売上の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalPartsAmount))%></td>   <!--部品売上の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalTaxAmount))%></td>     <!--消費税の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumGrandTotalAmount))%></td>   <!--売上の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalServiceCost))%></td>   <!--工賃原価の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalPartsCost))%></td>     <!--部品原価の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumGrandTotalCost))%></td>     <!--原価の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalServiceProfits))%></td><!--工賃粗利の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalPartsProfits))%></td>  <!--部品粗利の総合計-->
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumGrandTotalProfits))%></td>  <!--粗利の総合計-->
        <th colspan="3"></th>
    </tr>
</table>
<br />
</asp:Content>

