<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarPurchase>>" %>
<%@ Import Namespace="CrmsDao"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両入荷検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarPurchase", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.ValidationSummary() %><%//Add 2017/04/13 arc yano %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("RequestFlag", "1") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { maxlength = 50 })%></td>
        <th>部門</th>
        <td colspan="3"><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
            <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span>
        </td>       
    </tr>
    <tr>
        <th>仕入先</th>
        <td colspan="3"><%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('SupplierCode','SupplierName','Supplier')" })%>
            <img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SupplierCode', 'SupplierName', '/Supplier/CriteriaDialog')" />
            <span id="SupplierName"><%=Html.Encode(ViewData["SupplierName"])%></span>       
        </td>
        <th>仕入ステータス</th>
        <td><%=Html.DropDownList("PurchaseStatus", (IEnumerable<SelectListItem>)ViewData["PurchaseStatusList"])%></td>
    </tr>
    <tr>
        <th>発注日</th>
        <td><%=Html.TextBox("PurchaseOrderDateFrom", ViewData["PurchaseOrderDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("PurchaseOrderDateTo", ViewData["PurchaseOrderDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>仕入日</th>
        <td><%=Html.TextBox("SlipDateFrom", ViewData["SlipDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("SlipDateTo", ViewData["SlipDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>入庫日</th>
        <td><%=Html.TextBox("PurchaseDateFrom", ViewData["PurchaseDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("PurchaseDateTo", ViewData["PurchaseDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { maxlength = 50 })%></td>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { maxlength = 50 })%></td>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>グレード名</th>
        <td><%=Html.TextBox("CarGradeName", ViewData["CarGradeName"], new { maxlength = 50 })%></td>
        <th>車台番号</th>
        <td colspan="3"><%=Html.TextBox("Vin", ViewData["Vin"], new { maxlength = 20 })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="7">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケーターを表示するよう修正---%>
            <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
            <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1';DisplayImage('UpdateMsg','0');displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SalesCarNumber','DepartmentCode','DepartmentName','SupplierCode','SupplierName','PurchaseStatus','PurchaseOrderDateFrom','PurchaseOrderDateTo','SlipDateFrom','SlipDateTo','PurchaseDateFrom','PurchaseDateTo','MakerName','CarBrandName','CarName','CarGradeName','Vin'))" />
            <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CarPurchase', 'UpdateMsg'); displaySearchList();document.getElementById('RequestFlag').value = '1'"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケーターを表示するよう修正--%>
<div id ="UpdateMsg" style ="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarPurchase/Entry')" />
    <!--Add 2016/11/30 arc nakayama #3663_【製造】車両仕入　Excel取込対応-->
    <input type="button" value="一括取込" onclick="openModalAfterRefresh('/CarPurchase/ImportDialog', '', '', 'no', 'no')" />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th rowspan="2"style ="white-space:nowrap"></th>
        <th rowspan="2"style ="white-space:nowrap">入庫区分</th><%//Add 2018/06/06 arc yano #3883 タマ表改善 %>
        <th rowspan="2"style ="white-space:nowrap">管理番号</th>
        <th rowspan="2"style ="white-space:nowrap">部門</th>
        <th rowspan="2"style ="white-space:nowrap">新中区分</th>
        <th rowspan="2"style ="white-space:nowrap">仕入先</th>
        <th rowspan="2"style ="white-space:nowrap">仕入担当</th>
        <th rowspan="2"style ="white-space:nowrap">仕入ロケーション</th>
        <th rowspan="2"style ="white-space:nowrap">発注日</th>
        <th rowspan="2"style ="white-space:nowrap">仕入日</th>
        <th rowspan="2"style ="white-space:nowrap">入庫日</th>
        <th rowspan="2"style ="white-space:nowrap">メーカー</th>
        <th rowspan="2"style ="white-space:nowrap">ブランド</th>
        <th rowspan="2"style ="white-space:nowrap">車種</th>
        <th rowspan="2"style ="white-space:nowrap">グレード</th>
        <th rowspan="2"style ="white-space:nowrap">車台番号</th>
        <th colspan="3"style ="white-space:nowrap">車両本体価格</th>
        <th colspan="3"style ="white-space:nowrap">落札料</th>
        <th rowspan="2"style ="white-space:nowrap">リサイクル</th>
        <th colspan="3"style ="white-space:nowrap">自税充当</th>
        <th colspan="3"style ="white-space:nowrap">仕入金額</th>
        <th rowspan="2"style ="white-space:nowrap">最終更新者</th>
    </tr>
    <tr>
        <th style ="white-space:nowrap">税抜</th>
        <th style ="white-space:nowrap">消費税</th>
        <th style ="white-space:nowrap">税込</th>
        <th style ="white-space:nowrap">税抜</th>
        <th style ="white-space:nowrap">消費税</th>
        <th style ="white-space:nowrap">税込</th>
        <th style ="white-space:nowrap">税抜</th>
        <th style ="white-space:nowrap">消費税</th>
        <th style ="white-space:nowrap">税込</th>
        <th style ="white-space:nowrap">税抜</th>
        <th style ="white-space:nowrap">消費税</th>
        <th style ="white-space:nowrap">税込</th>
    </tr>
    <%foreach (var carPurchase in Model)
      { %>
    <tr>
        <!--コピーボタン-->
        <td style="white-space:nowrap"><input type="button" value="コピー" style="width:50px" onclick="openModalAfterRefresh('/CarPurchase/Entry/' + '<%=carPurchase.CarPurchaseId.ToString() %>    ' + ',1');" /></td>
        <!--入庫区分--><%//Add 2018/06/06 arc yano #3883 タマ表改善 %>
        <td style="white-space:nowrap"><%=Html.Encode(carPurchase.c_CarPurchaseType != null ? carPurchase.c_CarPurchaseType.Name : "")%></td>
        <!--管理番号-->
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarPurchase/Entry/' + '<%=carPurchase.CarPurchaseId.ToString() %>' + ',0');return false;"><%=CommonUtils.DefaultNbsp(carPurchase.SalesCar.SalesCarNumber, 20)%></a></td>
        <!--部門-->
        <td style="white-space:nowrap"><%if (carPurchase.Department != null) { %><%=Html.Encode(carPurchase.Department.DepartmentName)%><%} %></td>
        <!--新中区分-->
        <td style="white-space:nowrap"><%if (carPurchase.SalesCar.c_NewUsedType != null) { %><%=Html.Encode(carPurchase.SalesCar.c_NewUsedType.Name)%><%} %></td>
        <!--仕入先-->
        <td style="white-space:nowrap"><%if (carPurchase.Supplier != null) { %><%=Html.Encode(carPurchase.Supplier.SupplierName)%><%} %></td>
        <!--仕入担当-->
        <td style="white-space:nowrap"><%if (carPurchase.Employee != null) { %><%=Html.Encode(carPurchase.Employee.EmployeeName)%><%} %></td>
        <!--仕入ロケーション-->
        <td style="white-space:nowrap"><%if (carPurchase.Location != null) { %><%=Html.Encode(carPurchase.Location.LocationName)%><%} %></td>
        <!--発注日-->
        <td style="white-space:nowrap"><%try { %><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carPurchase.CarPurchaseOrder.PurchaseOrderDate))%><%} catch (NullReferenceException) { } %></td>
        <!--仕入日-->
        <td style="white-space:nowrap"><%try { %><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carPurchase.SlipDate))%><%} catch (NullReferenceException) { } %></td>
        <!--入庫日-->
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carPurchase.PurchaseDate))%></td>
        <!--メーカー-->
        <td style="white-space:nowrap"><%try { %><%=Html.Encode(carPurchase.SalesCar.CarGrade.Car.Brand.Maker.MakerName)%><%} catch (NullReferenceException) { } %></td>
        <!--ブランド-->
        <td style="white-space:nowrap"><%try { %><%=Html.Encode(carPurchase.SalesCar.CarGrade.Car.Brand.CarBrandName)%><%} catch (NullReferenceException) { } %></td>
        <!--車種-->
        <td style="white-space:nowrap"><%try { %><%=Html.Encode(carPurchase.SalesCar.CarGrade.Car.CarName)%><%} catch (NullReferenceException) { } %></td>
        <!--グレード-->
        <td style="white-space:nowrap"><%if (carPurchase.SalesCar.CarGrade != null) { %><%=Html.Encode(carPurchase.SalesCar.CarGrade.CarGradeName)%><%} %></td>
        <!--車台番号-->
        <td style="white-space:nowrap"><%=Html.Encode(carPurchase.SalesCar.Vin)%></td>
        <!--車両本体価格(税抜)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.VehiclePrice))%></td>
        <!--車両本体価格(消費税)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.VehicleTax))%></td>
        <!--車両本体価格(税込)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.VehicleAmount))%></td>
        <!--落札料(税抜)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.AuctionFeePrice))%></td>
        <!--落札料(消費税)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.AuctionFeeTax))%></td>
        <!--落札料(税込)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.AuctionFeeAmount))%></td>
        <!--リサイクル-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.RecycleAmount))%></td>
        <!--自税充当(税抜)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.CarTaxAppropriatePrice))%></td>
        <!--自税充当(消費税)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.CarTaxAppropriateTax))%></td>
        <!--自税充当(税込)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.CarTaxAppropriateAmount))%></td>
        <!--仕入金額(税抜)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.Amount))%></td>
        <!--仕入金額(消費税)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.TaxAmount))%></td>
        <!--仕入金額(税込)-->
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}",carPurchase.TotalAmount))%></td>
        <!--最終更新者-->
        <td style="white-space:nowrap;text-align:right"><%if (carPurchase.Employee1 != null && (!string.IsNullOrEmpty(carPurchase.Employee1.EmployeeName))){%><%=Html.Encode(carPurchase.Employee1.EmployeeName)%><%}else{%><%=Html.Encode("システム更新")%><%}%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
