<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarPurchaseOrder>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両発注処理
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    bool purchaseOrderStatus = false;
    bool reservationStatus = false;
    if (Model.PurchaseOrderStatus != null && Model.PurchaseOrderStatus.Equals("1")) {
        purchaseOrderStatus = true;
    }
    if (Model.ReservationStatus != null && Model.ReservationStatus.Equals("1")) {
        reservationStatus = true;
    }
        
%>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%if(!reservationStatus && !purchaseOrderStatus){ %>
       <td onclick="document.forms[0].actionType.value='Order';formSubmit()"><img src="/Content/Images/build.png" alt="発注処理" class="command_btn" />&nbsp;発注処理</td>
       <%} %>
   </tr>
</table>
<br />
<%using (Html.BeginForm("Entry2", "CarPurchaseOrder", FormMethod.Post)) { %>
<%=Html.Hidden("close", ViewData["close"])%>
<%=Html.Hidden("actionType","")%>
<%=Html.Hidden("CarPurchaseOrderNumber", Model.CarPurchaseOrderNumber) %>
<%=Html.ValidationSummary()%>
    <table class="input-form" style="width:98%">
      <tr>
        <th style="width:100px;height:20px">伝票番号</th>
        <td>
            <%=Html.Encode(Model.SlipNumber) %>
            <%=Html.Hidden("SlipNumber", Model.SlipNumber) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">受注日</th>
        <td>
            <%=Html.Encode(string.Format("{0:yyyy/MM/dd}",Model.CarSalesHeader!=null ? Model.CarSalesHeader.SalesOrderDate : null)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">部門</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Department!=null ? Model.CarSalesHeader.Department.DepartmentName : "")%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">営業担当者</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Employee!=null ? Model.CarSalesHeader.Employee.EmployeeName : "") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">顧客名</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.Customer!=null ? Model.CarSalesHeader.Customer.CustomerName : "") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">入金額</th>
        <td>
            <%=Html.Encode(string.Format("{0:N0}",Model.ReceiptAmount)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">諸費用</th>
        <td>
            <%=Html.Encode(string.Format("{0:N0}",Model.CarSalesHeader!=null ? Model.CarSalesHeader.CostTotalAmount : 0m)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">受注額</th>
        <td>
            <%=Html.Encode(string.Format("{0:N0}",Model.CarSalesHeader!=null ? Model.CarSalesHeader.GrandTotalAmount : 0m)) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">承認</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.ApprovalFlag!=null && Model.CarSalesHeader.ApprovalFlag.Equals("1") ? "済" : "未") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">発注日</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("PurchaseOrderDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseOrderDate), new { @class = "alphanumeric readonly", maxlength = 10, size = "10", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("PurchaseOrderDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseOrderDate), new { @class = "alphanumeric", maxlength = 10, size = "10" })%>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">仕入担当者</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "readonly alphanumeric", style = "width:50px", maxlength = "20", @readonly = "readonly" })%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "readonly alphanumeric", style = "width:80px", maxlength = "50", @readonly = "readonly" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "1" }); %>
            <%}else{ %>
                <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">仕入担当者名</th>
        <td>
            <%=Html.TextBox("EmployeeName", Model.Employee!=null ? Model.Employee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
      </tr>
      <tr>
        <th style="height:20px">整理番号</th>
        <td>
            <%=Html.TextBox("ArrangementNumber", Model.ArrangementNumber, new { @class = "alphanumeric", size = "15", maxlength = "20" }) %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">仕入先</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric readonly", size = "10", maxlength = "10", @readonly = "readonly" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierCode", "SupplierName", "'/Supplier/CriteriaDialog'", "1" }); %>
            <%}else{ %>
                <%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('SupplierCode','SupplierName','Supplier')" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierCode", "SupplierName", "'/Supplier/CriteriaDialog'", "0" }); %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">仕入先名</th>
        <td>
            <span id="SupplierName"><%=Html.Encode(ViewData["SupplierName"]) %></span>
        </td>
      </tr>
      <tr>
        <th style="height:20px">入庫予定日</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("ArrivalPlanDate", Model.ArrivalPlanDate, new { @class = "alphanumeric readonly", size = "10", maxlength = "10", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("ArrivalPlanDate", Model.ArrivalPlanDate, new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">入庫ロケーション</th>
        <td>
            <%if (purchaseOrderStatus) { %>
                <%=Html.TextBox("ArrivalLocationCode", Model.ArrivalLocationCode, new { @class = "alphanumeric readonly", size = "10", maxlength = "10", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalLocationCode", "ArrivalLocationName", "'/Location/CriteriaDialog?BusinessType=001,009'", "1" }); %>
            <%} else { %>
                <%=Html.TextBox("ArrivalLocationCode", Model.ArrivalLocationCode, new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalLocationCode", "ArrivalLocationName", "'/Location/CriteriaDialog?BusinessType=001,009'", "0" }); %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">入庫ロケーション名</th>
        <td>
            <span id="ArrivalLocationName"><%=Html.Encode(ViewData["ArrivalLocationName"]) %></span>
        </td>
      </tr>
      <tr>
        <th style="height:20px">支払先</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("SupplierPaymentCode", Model.SupplierCode, new { @class = "alphanumeric readonly", size = "10", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierPaymentCode", "SupplierPaymentName", "'/SupplierPayment/CriteriaDialog'", "1" }); %>
            <%}else{ %>
                <%=Html.TextBox("SupplierPaymentCode", Model.SupplierCode, new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('SupplierPaymentCode','SupplierPaymentName','SupplierPayment')" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierPaymentCode", "SupplierPaymentName", "'/SupplierPayment/CriteriaDialog'", "0" }); %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">支払先名</th>
        <td>
            <span id="SupplierPaymentName"><%=Html.Encode(ViewData["SupplierPaymentName"]) %></span>
        </td>
      </tr>
      <tr>
        <th style="height:20px">支払期限</th>
        <td>
            <%if(purchaseOrderStatus){ %>
                <%=Html.TextBox("PayDueDate", string.Format("{0:yyyy/MM/dd}", Model.PayDueDate), new { @class = "alphanumeric readonly", maxlength = "10", size = "10", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("PayDueDate", string.Format("{0:yyyy/MM/dd}", Model.PayDueDate), new { @class = "alphanumeric", maxlength = "10", size = "10" })%>
            <%} %>
        </td>
      </tr>
      <tr>
     	<th style="height:20px">車両本体価格</th>
     	<td>
     	    <%=Html.TextBox("VehiclePrice", Model.VehiclePrice, new { @class = "numeric", size = "8", maxlength = "10" }) %>
     	</td>
      </tr>
      <tr>
       	<th style="height:20px">ディスカウント金額</th>
       	<td>
            <%=Html.TextBox("DiscountAmount", Model.DiscountAmount, new { @class = "numeric", size = "8", maxlength = "10" }) %></td>
       	</td>
      </tr>
      <tr>
      	<th style="height:20px">メタリック価格</th>
      	<td>
            <%=Html.TextBox("MetallicPrice", Model.MetallicPrice, new { @class = "numeric", size = "8", maxlength = "10" }) %></td>
      	</td>
      </tr>
      <tr>
      	<th style="height:20px">オプション価格</th>
      	<td>
            <%=Html.TextBox("OptionPrice", Model.OptionPrice, new { @class = "numeric", size = "8", maxlength = "10" }) %></td>      	
      	</td>
      </tr>
      <tr>
      	<th style="height:20px">仕入価格</th>
      	<td>
            <%=Html.TextBox("Amount", Model.Amount, new { @class = "numeric", size = "8", maxlength = "10" }) %></td>
      	</td>
      </tr>
      <tr>
        <th style="height:20px">管理番号</th>
        <td>
            <%if(reservationStatus){ %>
                <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric readonly", size = "15", maxlength = "20", @readonly = "readonly" }) %>
            <%}else{ %>
                <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric", size = "15", maxlength = "20" }) %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">メーカー</th>
        <td>
            <%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.MakerName : "") %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">車種</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.CarName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">モデルコード</th>
        <td><%=Html.Encode(Model.CarSalesHeader!=null && Model.CarSalesHeader.CarGrade!=null ? Model.CarSalesHeader.CarGrade.ModelCode : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">型式</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.ModelName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">グレード</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.CarGradeName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">外装色</th>
        <td><%=Html.Encode(Model.CarSalesHeader != null ? Model.CarSalesHeader.ExteriorColorName : "") %></td>
      </tr>
      <tr>
        <th style="height:20px">内装色</th>
        <td><%=Html.Encode(Model.CarSalesHeader!= null ? Model.CarSalesHeader.InteriorColorName : "") %></td>
      </tr>
   </table>
<%} %>
</asp:Content>

