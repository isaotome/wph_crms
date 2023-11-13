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
                <img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存
            </td>
        </tr>
    </table>
    <br />
    <%using (Html.BeginForm("Entry", "PartsTransfer", FormMethod.Post)) { %>
    <%=Html.ValidationSummary() %>
    <%=Html.Hidden("close",ViewData["close"]) %>
    <br />
    <table class="input-form-slim">
        <tr>
            <th class="input-form-title" colspan="4">
                伝票情報
            </th>
        </tr>
        <tr>
            <th style="width: 100px">
                出庫日 *
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("DepartureDate", string.Format("{0:yyyy/MM/dd}", Model.DepartureDate), new { @class = "alphanumeric", size = "8", maxlength = "10" })%>
            </td>
            <th style="width: 100px">
                入庫予定日 *
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric", size = "8", maxlength = "10" })%>
            </td>
        </tr>
        <tr>
            <th rowspan="2">
                出庫ロケーション *
            </th>
            <td>
                <%// 2016/06/20 arc yano #3582 部品移動入力　出庫・入庫ロケーションの絞込み %>
                <%//Add 2017/02/03 arc nakayama #3594_部品移動入力　出庫・入庫ロケーションの絞込み② %>
                <%=Html.TextBox("DepartureLocationCode", Model.DepartureLocationCode, new { @class = "alphanumeric", size = "10", maxlength = "12", onblur = "GetNameFromCode('DepartureLocationCode','DepartureLocationName','Location', 'false', CheckLocation, new Array('DepartureLocationCode', 'DepartureLocationName'));GetLocationPartsStock(document.getElementById('PartsNumber').value,document.getElementById('DepartureLocationCode').value)" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureLocationCode", "DepartureLocationName", "'/Location/CriteriaDialog?BusinessType=002,009&LocationType=001&ConditionsHold=1'", "0" }); %><%// 2016/06/20 arc yano #3582 クエリ文字列にロケーション種別を追加%>
            </td>
            <th rowspan="2">
                入庫ロケーション *
            </th>
            <td>
                <%//Add 2017/02/03 arc nakayama #3594_部品移動入力　出庫・入庫ロケーションの絞込み② %>
                <%=Html.TextBox("ArrivalLocationCode", Model.ArrivalLocationCode, new { @class = "alphanumeric", size = "10", maxlength = "12", onblur = "GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location', 'false', CheckLocation, new Array('ArrivalLocationCode', 'ArrivalLocationName'))" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalLocationCode", "ArrivalLocationName", "'/Location/CriteriaDialog?BusinessType=002,009&LocationType=001&ConditionsHold=1'", "0" }); %><%// 2016/06/20 arc yano #3582 クエリ文字列にロケーション種別を追加%>
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
                出庫担当者 *
            </th>
            <td>
                <%=Html.TextBox("DepartureEmployeeNumber", Model.DepartureEmployee != null ? Model.DepartureEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('DepartureEmployeeNumber',new Array('DepartureEmployeeCode','DepartureEmployeeName'),'Employee')" })%><%=Html.TextBox("DepartureEmployeeCode",Model.DepartureEmployeeCode,new {@class="alphanumeric",style="width:80px", maxlength="50",onblur="GetMasterDetailFromCode('DepartureEmployeeCode',new Array('DepartureEmployeeNumber','DepartureEmployeeName'),'Employee')"})%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureEmployeeCode", "DepartureEmployeeName", "'/Employee/CriteriaDialog'", "0" });%>
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
                移動種別 *
            </th>
            <td>
                <%=Html.DropDownList("TransferType",(IEnumerable<SelectListItem>)ViewData["TransferTypeList"]) %>
            </td>
            <th>
            </th>
            <td>
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
                <%=Html.TextBox("PartsNumber", Model.PartsNumber, new { @class = "alphanumeric", size = "20", maxlength = "25", onblur = "GetLocationPartsStock(document.getElementById('PartsNumber').value,document.getElementById('DepartureLocationCode').value)" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "PartsNumber", "PartsNameJp", "'/Parts/CriteriaDialog'", "0" }); %>
            </td>
            <th style="width: 100px">
                在庫数
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("StockQuantity", ViewData["DepartureStockQuantity"], new { @class = "numeric readonly", style = "width:200px", @readonly = "readonly" }) %>
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
                出庫数量 *
            </th>
            <td>
                <%=Html.TextBox("Quantity", Model.Quantity, new { @class = "numeric", style = "width:200px", maxlength = "11" }) %>
            </td>
        </tr>
    </table>
    <%} %>
</asp:Content>
