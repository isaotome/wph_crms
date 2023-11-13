<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetSupplierPaymentListResult>>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    外注先　支払一覧
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
    //----------------------------------------------------------------------------------
    //　機能　：外注先支払一覧画面
    //　作成日：2017/03/23 arc yano #3729 サブシステム機能移行(外注先支払一覧) 新規作成
    //
    //---------------------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "SupplierPaymentList", new { id = 0 }, FormMethod.Post))
{ 
%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultTarget", ViewData["DefaultTarget"]) %><%//検索項目のデフォルト値(納車日を選択) %>
<%=Html.Hidden("DefaultTargetDateFrom", ViewData["DefaultTargetDateFrom"]) %>
<%=Html.Hidden("DefaultTargetDateTo", ViewData["DefaultTargetDateTo"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
   <table class="input-form">
     <tr>
        <th style="width:80px">年月</th>
        <td colspan="3"><%=Html.RadioButton("Target", "0", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("0"))%>納車日<%=Html.RadioButton("Target", "1", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("1"))%>受注日&nbsp;<%=Html.TextBox("TargetDateFrom", ViewData["TargetDateFrom"], new { @class="alphanumeric",size = "8",maxlength="10", onchange = "chkDate3(this.value, 'TargetDateFrom')" })%> ～ <%=Html.TextBox("TargetDateTo", ViewData["TargetDateTo"], new { @class="alphanumeric",size = "8",maxlength="10", onchange = "chkDate3(this.value, 'TargetDateTo')" })%></td>     
    </tr>
     <tr>
        <th style="width:80px">部門</th>
        <td><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"],new {@class="alphanumeric",maxlength="3", size = "3" , onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /><span id ="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>     
        <th style ="width:80px">主作業</th>
        <td><%=Html.DropDownList("ServiceWork",(IEnumerable<SelectListItem>)ViewData["ServiceWorkList"]) %></td>
     </tr>
    <tr>
        <th style ="width:80px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
        <th style ="width:80px">車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength = 20 })%></td>
    </tr>
     <tr>
        <th style ="width:80px">顧客コード</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", maxlength = 10, onblur="GetNameFromCode('CustomerCode', 'CustomerName', 'Customer')" })%>&nbsp;<img alt="顧客検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" /></td>
        <th style ="width:80px">顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { maxlength = 80 })%></td>
    </tr>
    <tr>
        <th style ="width:80px">外注先コード</th>
        <td><%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], new { @class = "alphanumeric", maxlength = 10, onblur="GetNameFromCode('SupplierCode', 'SupplierName', 'Supplier')" })%>&nbsp;<img alt="外注先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" /></td>
        <th style ="width:80px">外注先名</th>
        <td><%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { maxlength = 50 })%></td>
    </tr>
     <tr>
        <th style="width:80px"></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input id ="ListDownload" type="button" value="画面リスト出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('SupplierPaymentList', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '99';"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('Target', 'TargetDateFrom', 'TargetDateTo', 'DepartmentCode', 'DepartmentName', 'ServiceWork', 'SlipNumber', 'Vin', 'CustomerCode', 'CustomerName', 'SupplierCode', 'SupplierName'))"/>
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<table class="list">
    <tr> 
        <th style="white-space:nowrap; text-align:left">外注先コード</th>
        <th style="white-space:nowrap; text-align:left">外注先名</th>
        <th style="white-space:nowrap; text-align:left">伝票番号</th>
        <th style="white-space:nowrap; text-align:left">主作業名</th>
        <th style="white-space:nowrap; text-align:left">作業内容</th>
        <th style="white-space:nowrap; text-align:left">売上</th>
        <th style="white-space:nowrap; text-align:left">原価</th>
        <th style="white-space:nowrap; text-align:left">請求先</th>
        <th style="white-space:nowrap; text-align:left">顧客名</th>
        <th style="white-space:nowrap; text-align:left">ステータス</th>
        <th style="white-space:nowrap; text-align:left">部門コード</th>
        <th style="white-space:nowrap; text-align:left">部門名</th>
        <th style="white-space:nowrap; text-align:left">納車日</th>
        <th style="white-space:nowrap; text-align:left">受注日</th>
        <th style="white-space:nowrap; text-align:left">車台番号</th>
    </tr>

    <%//検索結果を表示する。%>
    <% foreach (var item in Model){ %>
        
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.SupplierCode)%></td>                                                                                     <%//外注先コード %>
            <td style="white-space:nowrap"><%=Html.Encode(item.SupplierName)%></td>                                                                                     <%//外注先名 %>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipNumber)%></td>                                                                                       <%//伝票番号 %>
            <td style="white-space:nowrap"><%=Html.Encode(item.ServiceWorkName)%></td>                                                                                  <%//主作業名 %>
            <td style="white-space:nowrap"><%=Html.Encode(item.LineContents)%></td>                                                                                     <%//作業内容 %>
            <td style="white-space:nowrap;  text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.TechnicalFeeAmount))%></td>                                    <%//売上%> 
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.Cost))%></td>                                                   <%//原価%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.CustomerClaimName)%></td>                                                                                <%//請求先名%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CustomerName)%></td>                                                                                     <%//顧客名%>
            <td style="white-space:nowrap"><%=Html.Encode(item.ServiceOrderStatusName)%></td>                                                                           <%//ステータス%>
            <td style="white-space:nowrap"><%=Html.Encode(item.DepartmentCode)%></td>                                                                                   <%//部門コード %>
            <td style="white-space:nowrap"><%=Html.Encode(item.DepartmentName)%></td>                                                                                   <%//部門名%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesDate))%></td>                                                        <%//納車日%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesOrderDate))%></td>                                                   <%//受注日%>
            <td style="white-space:nowrap"><%=Html.Encode(item.Vin)%></td>                                                                                              <%//車台番号%>
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
