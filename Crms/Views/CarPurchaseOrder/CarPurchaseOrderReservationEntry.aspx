<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarPurchaseOrder>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両発注処理
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%if (Model.ReservationStatus == null || Model.ReservationStatus.Equals("0")) { %>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <td onclick="document.forms[0].actionType.value='Reservation';formSubmit()"><img src="/Content/Images/build.png" alt="引当処理" class="command_btn" />&nbsp;引当処理</td>
       <%} %>
   </tr>
</table>
<br />
<%using (Html.BeginForm("ReservationEntry", "CarPurchaseOrder", FormMethod.Post)) { %>
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
        <th style="height:20px">管理番号</th>
        <td>
            <%if (Model.ReservationStatus != null && Model.ReservationStatus.Equals("1")) { %>
                <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric readonly", size = "15", maxlength = "20", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "1" }); %>
            <%} else { %>
                <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric", size = "15", maxlength = "20", onblur = "GetNameFromCode('SalesCarNumber','Vin','SalesCar')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0" }); %>
            <%} %>
        </td>
      </tr>
      <tr>
        <th style="height:20px">車台番号</th>
        <td>
            <span id="Vin"><%=Html.Encode(ViewData["Vin"]) %></span>
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

