<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PartsPurchaseOrder>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品発注入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<%using(Html.BeginForm("Confirm","PartsPurchaseOrder",FormMethod.Post)){ %>
<br />
<%=Html.ValidationSummary() %>
<br />
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("OrderFlag","") %>
<%=Html.Hidden("DelFlag",Model.DelFlag) %>
<table class="input-form" style="width:700px">
    <tr>
        <th style="width:100px;height:20px">発注番号</th>
        <td style="width:200px;height:20px"><%=Model.PurchaseOrderNumber %><%=Html.Hidden("PurchaseOrderNumber",Model.PurchaseOrderNumber) %></td>
        <th style="width:100px;height:20px">受注伝票番号</th>
        <td style="width:200px;height:20px"><%=Model.ServiceSlipNumber %><%=Html.Hidden("ServiceSlipNumber",Model.ServiceSlipNumber) %></td>
    </tr>
    <tr>
        <th style="height:20px">発注日</th>
        <td style="height:20px"><%=string.Format("{0:yyyy/MM/dd}",Model.PurchaseOrderDate)%></td>
        <th style="height:20px">ステータス</th>
        <td style="height:20px"><%=Model.c_PurchaseOrderStatus!=null ? Model.c_PurchaseOrderStatus.Name : "未発注"%><%=Html.Hidden("PurchaseOrderStatus",Model.PurchaseOrderStatus) %></td>
    </tr>
    <tr>
        <th rowspan="2">部門</th>
        <td style="height:20px"><%=Model.DepartmentCode%></td>
        <th rowspan="2">担当者</th>
        <td style="height:20px"><%=Model.EmployeeCode%></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
        <td style="height:20px"><span id="EmployeeName"><%=Html.Encode(ViewData["EmployeeName"]) %></span></td>
    </tr>
    <tr>
        <th rowspan="2">仕入先</th>
        <td style="height:20px"><%=Model.SupplierCode%></td>
        <th rowspan="2">支払先</th>
        <td style="height:20px"><%=Model.SupplierPaymentCode%></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="SupplierName"><%=Html.Encode(ViewData["SupplierName"]) %></span></td>
        <td style="height:20px"><span id="SupplierPaymentName"><%=Html.Encode(ViewData["SupplierPaymentName"]) %></span></td>
    </tr>
    <tr>
        <th>WEBオーダー番号</th>
        <td><%=Html.TextBox("WebOrderNumber", Model.WebOrderNumber, new { @class = "alphanumeric", size = "20",maxlength="50" })%></td>
        <th>メーカーオーダー番号</th>
        <td><%=Html.TextBox("MakerOrderNumber", Model.MakerOrderNumber, new { @class = "alphanumeric", size = "20",maxlength="50" })%></td>
    </tr>
    <tr>
        <th style="height:20px">部品番号</th>
        <td style="height:20px"><%=Model.PartsNumber%></td>
        <th style="height:20px">オーダー区分</th>
        <td style="height:20px"><%=Model.c_OrderType!=null ? Model.c_OrderType.Name : ""%></td>
    </tr>
    <tr>
        <th style="height:20px">部品名</th>
        <td style="height:20px"><span id="PartsNameJp"><%=Html.Encode(ViewData["PartsNameJp"]) %></span></td>
        <th style="height:20px">数量</th>
        <td style="height:20px"><%=Model.Quantity%></td>
    </tr>
    <tr>
        <th style="height:20px">入荷予定日</th>
        <td style="height:20px"><%=string.Format("{0:yyyy/MM/dd}",Model.ArrivalPlanDate)%></td>
        <th style="height:20px">原価</th>
        <td style="height:20px"><%=string.Format("{0:N0}",Model.Cost)%></td>
    </tr>
    <tr>
        <th>インボイスNO</th>
        <td><%=Html.TextBox("InvoiceNo", Model.InvoiceNo, new { @class = "alphanumeric", size = "10",maxlength="50" })%></td>
        <th>定価</th>
        <td><%=string.Format("{0:N0}",Model.Price)%></td>
    </tr>
    <tr>
        <th style="height:20px">支払予定日</th>
        <td style="height:20px"><%=string.Format("{0:yyyy/MM/dd}",Model.PaymentPlanDate) %></td>
        <th style="height:20px">金額</th>
        <td style="height:20px"><%=string.Format("{0:N0}",Model.Amount)%></td>
    </tr>
</table>
<%} %>
</asp:Content>