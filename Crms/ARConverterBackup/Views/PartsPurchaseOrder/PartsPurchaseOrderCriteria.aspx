<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PartsPurchaseOrder>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品発注検索
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    /*-------------------------------------------------------------------------------------------
     * 機能：発注一覧画面
     * 
     * 更新履歴：
     *            2015/02/02 arc yano  #3423 部品発注一覧画面　社外品の発注区分の非表示化
     *            2015/12/09 arc yano  #3290 部品仕入機能改善(部品発注一覧) 画面レイアウトの変更
     * 
     * 
     * 
     *------------------------------------------------------------------------------------------*/
   %>
<%using (Html.BeginForm("Criteria","PartsPurchaseOrder", new { id = 0 },FormMethod.Post)){ %>
<%=Html.Hidden("id","0") %>
<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<%=Html.Hidden("DefaultPurchaseOrderStatus", ViewData["DefaultPurchaseOrderStatus"]) %>
<%=Html.Hidden("DefaultGenuineType", ViewData["DefaultGenuineType"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:120px">発注伝票番号</th>
        <td><%=Html.TextBox("PurchaseOrderNumber", ViewData["PurchaseOrderNumber"], new { @class = "alphanumeric", size = "10",maxlength="8" })%></td>
        <th style="width:120px">受注伝票番号</th>
        <td><%=Html.TextBox("ServiceSlipNumber", ViewData["ServiceSlipNumber"], new { @class = "alphanumeric", size = "10",maxlength="10" })%></td>
    </tr>
    <tr>
        <th>発注日</th>
        <td><%=Html.TextBox("PurchaseOrderDateFrom",ViewData["PurchaseOrderDateFrom"],new {@class="alphanumeric", size="10", maxlength="10", onchange = "chkDate3(this.value, this.id)"}) %>～<%=Html.TextBox("PurchaseOrderDateTo",ViewData["PurchaseOrderDateTo"],new {@class="alphanumeric",size="10", maxlength="10", onchange = "chkDate3(this.value, this.id)"}) %></td>
        <th>発注ステータス</th>
        <td><%=Html.DropDownList("PurchaseOrderStatus",(IEnumerable<SelectListItem>)ViewData["PurchaseOrderStatusList"]) %></td>
    </tr> 
    <tr>
        <th style="width:120px" rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"], new {@class="alphanumeric",size="10",maxlength="3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')"}) %>&nbsp;<img alt="部門検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th rowspan="2">担当者</th>
        <td><%=Html.TextBox("EmployeeCode",ViewData["EmployeeCode"],new {@class="alphanumeric",size="15",maxlength="50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')"}) %>&nbsp;<img alt="担当者検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
        <td><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
    </tr>

     <tr>
         <th rowspan="2">仕入先</th>
            <td>
                <%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('SupplierCode', 'SupplierName', 'Supplier')" })%> &nbsp;<img alt="仕入先検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog') "/>
            </td>
        <th>WEBオーダー番号</th>
        <td><%=Html.TextBox("WebOrderNumber",ViewData["WebOrderNumber"],new { @class="alphanumeric",size="15",maxlength="50"}) %></td>
    </tr>
    <tr>
        <td><%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { size = "25", maxlength = "80" })%></td>
        <th>純正区分</th>
        <td><%=Html.DropDownList("GenuineType",(IEnumerable<SelectListItem>)ViewData["GenuineTypeList"]) %></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('PurchaseOrderNumber', 'ServiceSlipNumber', 'PurchaseOrderDateFrom', 'PurchaseOrderDateTo', 'PurchaseOrderStatus', 'DepartmentCode', 'DepartmentName', 'EmployeeCode', 'EmployeeName', 'SupplierCode', 'SupplierName', 'WebOrderNumber', 'GenuineType'))" />
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<input type="button" value="新規作成(純正品)" style="width:200px" onclick="openModalAfterRefresh('/PartsPurchaseOrder/Entry')" />
<input type="button" value="新規作成(社外品)" style="width:200px" onclick="openModalAfterRefresh('/PartsPurchaseOrderNonGenuine/Entry')" />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="white-space:nowrap">発注伝票番号</th>
        <th style="white-space:nowrap">受注伝票番号</th>
        <th style="white-space:nowrap">受注日</th>
        <th style="white-space:nowrap">顧客名</th>
        <th style="white-space:nowrap">部品番号</th>
        <th style="white-space:nowrap">部品名</th>
        <th style="white-space:nowrap">純正区分</th>
        <th style="white-space:nowrap">数量</th>
        <th style="white-space:nowrap">仕入先</th>
        <th style="white-space:nowrap">発注区分</th>
        <th style="white-space:nowrap">発注ステータス</th>
        <th style="white-space:nowrap">発注日</th>
        <th style="white-space:nowrap">サービスフロント</th>
        <th style="white-space:nowrap">WEBオーダー番号</th>                                                                                                                                                                                                                                                                                                                           
        <th style="white-space:nowrap">部門</th>
        <th style="white-space:nowrap">担当者</th>
    </tr>
    <%            
        for(int i=0;i<Model.Count;i++){
            string orderUrl = "";       //発注入力画面のurl
            
            string purchaseOrderNumber = Model[i].PurchaseOrderNumber;
            string serviceSlipNumber = Model[i].ServiceSlipNumber;
            string purchaseOrderStatus = Model[i].c_PurchaseOrderStatus != null ? Model[i].c_PurchaseOrderStatus.Name : "";
            string supplierCode = Model[i].Supplier != null ? Model[i].Supplier.SupplierName : "";
            string purchaseOrderDate = string.Format("{0:yyyy/MM/dd}", Model[i].PurchaseOrderDate);
            string webOrderNumber = Model[i].WebOrderNumber;
            string departmentName = Model[i].Department != null ? Model[i].Department.DepartmentName : "";
            string employeeName = Model[i].Employee != null ? Model[i].Employee.EmployeeName : "";
            string partsNumber = Model[i].PartsNumber;
            string partsName = Model[i].Parts != null ? (string.IsNullOrEmpty(Model[i].Parts.PartsNameJp) ? Model[i].Parts.PartsNameEn : Model[i].Parts.PartsNameJp) : "";
            decimal? quantity = Model[i].Quantity;

            DateTime? salesOrderDate = Model[i].ServiceSalesHeader != null ? Model[i].ServiceSalesHeader.SalesOrderDate : null;
            string frontEmployeeName = "";
            try { frontEmployeeName = Model[i].ServiceSalesHeader.FrontEmployee.EmployeeName; } catch (NullReferenceException) { }
            string customerName = "";
            try { customerName = Model[i].ServiceSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }

            string genuineType = "";
            string genuineTypeName = "";
            try { genuineType = Model[i].Parts.GenuineType; }catch (NullReferenceException) { }
            try { genuineTypeName = Model[i].Parts.c_GenuineType.Name; }catch (NullReferenceException) { }


            //Mod 2015/02/02 arc yano  #3423
            string orderType = "";
            
            //発注区分の設定
            //発注画面のurlの作成(純正区分により開く画面を変更する)
            if (!string.IsNullOrWhiteSpace(genuineType) && genuineType.Equals("002"))     //純正区分 =「社外品」
            {
                orderUrl = "/PartsPurchaseOrderNonGenuine/Entry?PurchaseOrderNumber=";
                
            }
            else
            {
                orderUrl = "/PartsPurchaseOrder/Entry?PurchaseOrderNumber=";
                orderType = Model[i].c_OrderType != null ? Model[i].c_OrderType.Name : "";
            }
            
            orderUrl += purchaseOrderNumber;
    %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('<%=orderUrl %>')" ><%=CommonUtils.DefaultNbsp(purchaseOrderNumber) %></a></td>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry/?SlipNo=<%=CommonUtils.DefaultNbsp(serviceSlipNumber) %>')"><%=serviceSlipNumber %></a></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",salesOrderDate)) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(customerName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(partsNumber) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(partsName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(genuineTypeName) %></td>
        <td style="white-space:nowrap; text-align:right"><%=CommonUtils.DefaultNbsp(quantity) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(supplierCode) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(orderType) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(purchaseOrderStatus) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(purchaseOrderDate)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(frontEmployeeName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(webOrderNumber) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(departmentName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(employeeName) %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
