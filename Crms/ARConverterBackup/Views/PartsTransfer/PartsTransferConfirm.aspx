<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master"
    Inherits="System.Web.Mvc.ViewPage<CrmsDao.Transfer>" %>

<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品移動入力
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <table class="command">
        <tr>
            <td onclick="formClose()">
                <img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる
            </td>
            <td onclick="formSubmit()">
                <img src="/Content/Images/build.png" alt="入庫確定" class="command_btn" />&nbsp;入庫確定
            </td>
        </tr>
    </table>
    <br />
    <%using (Html.BeginForm("Confirm", "PartsTransfer", FormMethod.Post)) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.Hidden("close",ViewData["close"]) %>
    <%=Html.Hidden("TransferNumber",Model.TransferNumber) %>
    <br />
    <table class="input-form-slim">
        <tr>
            <th class="input-form-title" colspan="4">
                伝票情報
            </th>
        </tr>
        <tr>
            <th style="width: 100px">
                出庫日
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("DepartureDate", string.Format("{0:yyyy/MM/dd}", Model.DepartureDate), new { @class = "readonly alphanumeric", size = "8", maxlength = "10", @readonly = "readonly" })%>
            </td>
            <th style="width: 100px">
                入庫予定日
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "readonly alphanumeric", size = "8", maxlength = "10", @readonly = "readonly" })%>
            </td>
        </tr>
        <tr>
            <th rowspan="2">
                出庫ロケーション
            </th>
            <td>
                <%=Html.TextBox("DepartureLocationCode", Model.DepartureLocationCode, new { @class = "readonly alphanumeric", size = "10", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureLocationCode", "DepartureLocationName", "'/Location/CriteriaDialog?BusinessType=002,009'", "1" }); %>
            </td>
            <th rowspan="2">
                入庫ロケーション *
            </th>
            <td>
                <%=Html.TextBox("ArrivalLocationCode", Model.ArrivalLocationCode, new { @class = "alphanumeric", size = "10", maxlength = "12", onblur = "GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalLocationCode", "ArrivalLocationName", "'/Location/CriteriaDialog?BusinessType=002,009'", "0" }); %>
            </td>
        </tr>
        <tr>
            <td style="height: 20px">
                <%=Html.TextBox("DepartureLocationName", Model.DepartureLocation != null ? Model.DepartureLocation.LocationName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
            </td>
            <td style="height: 20px">
                <%=Html.TextBox("ArrivalLocationName", Model.ArrivalLocation != null ? Model.ArrivalLocation.LocationName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
            </td>
        </tr>
        <tr>
            <th rowspan="2">
                出庫担当者
            </th>
            <td>
                <%=Html.TextBox("DepartureEmployeeNumber", Model.DepartureEmployee != null ? Model.DepartureEmployee.EmployeeNumber : "", new { @class = "readonly alphanumeric", style = "width:50px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureEmployeeCode", "DepartureEmployeeName", "'/Employee/CriteriaDialog'", "1" });%>
            </td>
            <th rowspan="2">
                入庫担当者
            </th>
            <td>
                <%=Html.TextBox("ArrivalEmployeeNumber", Model.ArrivalEmployee != null ? Model.ArrivalEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('ArrivalEmployeeNumber',new Array('ArrivalEmployeeCode','ArrivalEmployeeName'),'Employee')" })%><%=Html.TextBox("ArrivalEmployeeCode", Model.ArrivalEmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('ArrivalEmployeeCode',new Array('ArrivalEmployeeNumber','ArrivalEmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalEmployeeCode", "ArrivalEmployeeName", "'/Employee/CriteriaDialog'", "0" });%>
            </td>
        </tr>
        <tr>
            <td style="height: 20px">
                <%=Html.TextBox("DepartureEmployeeName", Model.DepartureEmployee != null ? Model.DepartureEmployee.EmployeeName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
            </td>
            <td style="height: 20px">
                <%=Html.TextBox("ArrivalEmployeeName", Model.ArrivalEmployee != null ? Model.ArrivalEmployee.EmployeeName : "", new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%>
            </td>
        </tr>
        <tr>
            <th>
                移動種別
            </th>
            <td>
                <%=Html.DropDownList("TransferType", (IEnumerable<SelectListItem>)ViewData["TransferTypeList"], new { @disabled = "disabled" })%>
                <%=Html.Hidden("TransferType",Model.TransferType) %>
            </td>
            <th>
                入庫日 *
            </th>
            <td>
                <%=Html.TextBox("ArrivalDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalDate), new { @class = "alphanumeric", size = "8" })%>
            </td>
        </tr>
    </table>
    <br />
    <table class="input-form-slim">
        <tr>
            <th class="input-form-title" colspan="4">
                部品情報
            </th>
        </tr>
        <tr>
            <th style="width: 100px">
                部品番号 *
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("PartsNumber", Model.PartsNumber, new { @class = "readonly alphanumeric", size = "20", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "PartsNumber", "PartsNameJp", "'/Parts/CriteriaDialog'", "1" }); %>
            </td>
            <th style="width: 100px">
                在庫数
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("StockQuantity", ViewData["ArrivalStockQuantity"], new { @class = "numeric readonly", style = "width:200px", @readonly = "readonly" }) %>
            </td>
        </tr>
        <tr>
            <th>
                部品名
            </th>
            <td>
                <%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { @class = "readonly", style = "width:200px", @readonly = "readonly" }) %>
            </td>
            <th>
                入庫数量
            </th>
            <td>
                <%=Html.TextBox("Quantity", Model.Quantity, new { @class = "readonly numeric", style = "width:200px", @readonly = "readonly" }) %>
            </td>
        </tr>
    </table>
    <%} %>
</asp:Content>
