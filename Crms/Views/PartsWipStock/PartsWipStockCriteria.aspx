<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.InventoryParts_Shikakari>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品仕掛在庫検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Criteria", "PartsWipStock", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %>

<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<%=Html.Hidden("DefaultServiceType", ViewData["DefaultServiceType"]) %>

<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//Add 2016/07/14 arc yano #3600 仕掛在庫検索　Excel出力機能追加%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
    <%=Html.ValidationSummary() %>
    <table class="input-form">
    <tr>
        <%//Mod 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧ ビューテーブル廃止 にともないデータの型を変更 %>
        <th style="width:100px">対象年月 *</th>
        <td><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"], new { onchange="CheckDateForDropDown()"})%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"], new { onchange="CheckDateForDropDown()"})%>&nbsp;月&nbsp;</td>      
        <th>部門コード *</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department','false')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?PartsInventoryFlag=1')"/><span id="DepartmentName" style ="width:160px"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>        
    </tr>
    <tr>
        <!--Add 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応 検索項目に明細種別追加-->
        <th style="width:100px">明細種別</th>
        <td><%=Html.DropDownList("ServiceType", (IEnumerable<SelectListItem>)ViewData["ServiceTypeList"])%></td>
        <th colspan="2"></th>
    </tr>
    <tr>
        <th>入庫日</th>
        <%//Mod 2015/01/09 arc yano 部品棚卸対応、検索条件の入庫日を範囲指定に変更 %>
        <td><%=Html.TextBox("ArrivalPlanDateFrom", ViewData["ArrivalPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>　～　<%=Html.TextBox("ArrivalPlanDateTo", ViewData["ArrivalPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
　      <th>伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { size = 8, maxlength = 8 })%></td>
    </tr>
    <tr>
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", maxlength = 25 })%></td>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 50, maxlength = 50 })%></td>
  
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength = 20 })%></td>
        <th>顧客名(漢字／カナ)</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <%  // Add 2016/07/14 arc yano 仕掛在庫検索　Excel出力機能追加 %>
        <%  // Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正 %>
        <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/>
        <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'ArrivalPlanDateFrom', 'ArrivalPlanDateTo', 'SlipNumber', 'PartsNumber', 'PartsNameJp', 'CustomerName', 'Vin'))"/>
        </td>
    </tr>

</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id ="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>        
        <th style ="white-space:nowrap">入庫日</th>
        <th colspan="2" style ="white-space:nowrap">伝票番号</th>
        <th style ="white-space:nowrap">伝票ステータス</th>
        <th style ="white-space:nowrap">主作業</th>
        <th style ="white-space:nowrap">フロント担当者</th>
        <th style ="white-space:nowrap">メカニック担当者</th>
        <th style ="white-space:nowrap">顧客名</th>
        <th style ="white-space:nowrap">車種</th>
        <th style ="white-space:nowrap">車台番号</th>
        <th style ="white-space:nowrap">明細種別</th>
        <th style ="white-space:nowrap">状況</th>
        <th style ="white-space:nowrap">発注日</th>
        <th style ="white-space:nowrap">入荷予定</th>
        <th style ="white-space:nowrap">入荷日</th>
        <th style ="white-space:nowrap">部品番号</th>
        <th style ="white-space:nowrap">部品名</th>
        <th style ="white-space:nowrap">単価</th>
        <th style ="white-space:nowrap">数量</th>
        <th style ="white-space:nowrap">金額</th>
        <th style ="white-space:nowrap">外注先</th>
        <th style ="white-space:nowrap">作業内容</th>
        <th style ="white-space:nowrap">外注原価</th>
    </tr>
    <%foreach (var line in Model)
      { %>
    <tr>
        <%//Mod 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧ ビューテーブル廃止 にともないデータの型を変更 %>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.ArrivalPlanDate))%></td><%//入庫日%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.SlipNumber)%></td><%//伝票番号%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.LineNumber)%></td><%//ラインナンバー%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.ServiceOrderStatusName)%></td><%//伝票ステータス%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.ServiceWorksName)%></td><%//主作業%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.FrontEmployeeName)%></td><%//フロント担当者%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.MekaEmployeeName)%></td><%//メカニック担当者%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerName)%></td><%//顧客名%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.CarName)%></td><%//車種%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.Vin)%></td><%//車台番号>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.ServiceTypeName)%></td><%//明細種別>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.StockTypeName)%></td><%//状況>%>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.PurchaseOrderDate))%></td><%//発注日>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.PurchaseDate)%></td><%//入荷日>%>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",line.PartsArravalPlanDate))%></td><%//入荷予定>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.PartsNumber)%></td><%//部品番号>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.LineContents1)%></td><%//部品名>%>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",line.Price))%></td><%//単価>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.Quantity)%></td><%//数量>%>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",line.Amount))%></td><%//金額>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.SupplierName)%></td><%//外注先>%>
        <td style ="white-space:nowrap"><%=Html.Encode(line.LineContents2)%></td><%//作業内容>%>
        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",line.Cost))%></td><%//外注原価>%>
    </tr>
    <%} %>
</table>

        <%} %>
</asp:Content>

