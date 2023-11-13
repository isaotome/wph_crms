<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PartsPurchaseOrder>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品発注一括編集
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("ListEntry", "PartsPurchaseOrder", FormMethod.Post)) { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%if(Model.PurchaseOrderStatus!=null && Model.PurchaseOrderStatus.Equals("001")){ %>
       <td onclick="document.forms[0].OrderFlag.value='1';formSubmit();"><img src="/Content/Images/build.png" alt="発注処理" class="command_btn"/>&nbsp;発注処理</td>
       <td onclick="document.forms[0].DelFlag.value='1';formSubmit();"><img src="/Content/Images/cancel.png" alt="発注取消" class="command_btn" />&nbsp;発注取消</td>
       <%} %>
   </tr>
</table>
<br />
<br />
<%=Html.ValidationSummary() %>
<br />
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("OrderFlag","") %>
<%=Html.Hidden("DelFlag",Model.DelFlag) %>
<%=Html.Hidden("PurchaseOrderStatus", Model.PurchaseOrderStatus) %>
<table class="input-form">
    <tr>
        <th style="width:100px;height:20px">発注日 *</th>
        <td style="width:200px;height:20px"><%=Html.TextBox("PurchaseOrderDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseOrderDate), new { @class = "alphanumeric", size = "10", maxlength = "10" }) %></td>
        <th style="width:100px;height:20px">オーダー区分 *</th>
        <td style="width:200px;height:20px"><%=Html.DropDownList("OrderType",(IEnumerable<SelectListItem>)ViewData["OrderTypeList"]) %></td>
    </tr>
    <tr>
        <th rowspan="2">部門 *</th>
        <td><%=Html.TextBox("DepartmentCode",Model.DepartmentCode,new {@class="alphanumeric",size="10",maxlength="3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')"}) %>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th rowspan="2">担当者 *</th>
        <td><%=Html.TextBox("EmployeeCode",Model.EmployeeCode,new {@class="alphanumeric",size="10",maxlength="50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')"}) %>&nbsp;<img alt="担当者検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"]) %></span></td>
        <td><span id="EmployeeName"><%=CommonUtils.DefaultNbsp(ViewData["EmployeeName"]) %></span></td>
    </tr>
    <tr>
        <th rowspan="2">仕入先 *</th>
        <td><%=Html.TextBox("SupplierCode",Model.SupplierCode,new {@class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('SupplierCode','SupplierName','Supplier')"}) %>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" /></td>
        <th rowspan="2">支払先 *</th>
        <td><%=Html.TextBox("SupplierPaymentCode",Model.SupplierPaymentCode,new {@class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('SupplierPaymentCode','SupplierPaymentName','SupplierPayment')"}) %>&nbsp;<img alt="支払先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('SupplierPaymentCode','SupplierPaymentName','/SupplierPayment/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="SupplierName"><%=CommonUtils.DefaultNbsp(ViewData["SupplierName"]) %></span></td>
        <td><span id="SupplierPaymentName"><%=CommonUtils.DefaultNbsp(ViewData["SupplierPaymentName"]) %></span></td>
    </tr>
    <tr>
        <th>入荷予定日 *</th>
        <td><%=Html.TextBox("ArrivalPlanDate",string.Format("{0:yyyy/MM/dd}",Model.ArrivalPlanDate), new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
        <th>支払予定日 *</th>
        <td><%=Html.TextBox("PaymentPlanDate",string.Format("{0:yyyy/MM/dd}",Model.PaymentPlanDate), new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
    </tr>
    <tr>
        <th>WEBオーダー番号</th>
        <td><%=Html.TextBox("WebOrderNumber", Model.WebOrderNumber, new { @class = "alphanumeric", size = "20",maxlength="50" })%></td>
        <th>メーカーオーダー番号</th>
        <td><%=Html.TextBox("MakerOrderNumber", Model.MakerOrderNumber, new { @class = "alphanumeric", size = "20",maxlength="50" })%></td>
    </tr>
    <tr>
        <th>インボイス番号</th>
        <td><%=Html.TextBox("InvoiceNo", Model.InvoiceNo, new { @class = "alphanumeric", size = "20",maxlength="50" })%></td>
        <th>備考</th>
        <td><%=Html.TextBox("Memo",Model.Memo,new {size=20,maxlength=100}) %></td>
    </tr>
</table>

<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="5">更新対象</th>
    </tr>
    <tr>
        <th nowrap style="width:80px">発注番号</th>
        <th nowrap style="width:200px">部品番号</th>
        <th nowrap style="width:300px">部品名</th>
        <th nowrap style="width:70px">数量</th>
    </tr>
</table>
<div style="overflow-y:scroll;width:695px;height:200px">
<table class="input-form">
    <%if (ViewData["UpdateTargetList"] != null) {
          List<PartsPurchaseOrder> targetList = (List<PartsPurchaseOrder>)ViewData["UpdateTargetList"];
          for (int i = 0; i < targetList.Count; i++) {
              string namePrefix = string.Format("list[{0}].", i);
              PartsPurchaseOrder item = targetList[i];
              //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
              string idPrefix = string.Format("list[{0}]_", i);
    %>
    <tr>
        <% // Mod 2014/07/18 arc amii chrome対応 フル桁表示するとレイアウトが崩れるのを修正 %>
        <td  style="width:80px;white-space:normal"><%=CommonUtils.DefaultNbsp(item.PurchaseOrderNumber)%></td>
        <td  style="width:200px;white-space:normal"><%=CommonUtils.DefaultNbsp(item.PartsNumber)%></td>
        <td  style="width:300px;white-space:normal"><%=CommonUtils.DefaultNbsp(item.Parts != null ? (string.IsNullOrEmpty(item.Parts.PartsNameJp) ? item.Parts.PartsNameEn : item.Parts.PartsNameJp) : "")%></td>
        <td  style="width:70px;text-align:right;white-space:normal"><%=CommonUtils.DefaultNbsp(item.Quantity)%>
    </tr>
    <%=Html.Hidden(namePrefix + "PurchaseOrderNumber", item.PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber"})%>
    <% }
      }%>
</table>

</div>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>

