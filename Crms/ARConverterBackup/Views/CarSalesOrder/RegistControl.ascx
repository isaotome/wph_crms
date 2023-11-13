<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<table class="input-form-slim">
    <tr>
        <th colspan="4" class="input-form-title">登録情報</th>
    </tr>
    
    <%//Add 2020/01/06 yano #4029%>
    <tr>
    
     <th style="width: 100px">
            都道府県コード(*)
     </th>
     <td colspan="3" style="text-align:left">
            <%if (Model.CostAreaEnabled)
              { %>
                <%=Html.TextBox("CostAreaCode", Model.CostAreaCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "10", onchange ="GetMasterDetailFromCode('CostAreaCode',new Array('CostAreaName','ParkingSpaceFeeWithTax', 'InspectionRegistFeeWithTax', 'TradeInFeeWithTax', 'PreparationFeeWithTax', 'RequestNumberFeeWithTax', 'RecycleControlFeeWithTax' , 'RequestNumberCost', 'ParkingSpaceCost', 'NumberPlateCost','FarRegistFeeWithTax', 'OutJurisdictionRegistFeeWithTax'),'CostArea', null, null, null, calcTotalAmount); calcTotalAmount()" })%><%-- Mod 2023/10/05 yano #4184 --%><%--Mod 2023/08/15 yano #4176--%><%//Mod 2020/01/06 yano #4029%>
                <%--<%=Html.TextBox("CostAreaCode", Model.CostAreaCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "10", onchange ="GetMasterDetailFromCode('CostAreaCode',new Array('CostAreaName','ParkingSpaceFeeWithTax', 'InspectionRegistFeeWithTax', 'TradeInFeeWithTax', 'PreparationFeeWithTax', 'RequestNumberFeeWithTax', 'RecycleControlFeeWithTax' , 'RequestNumberCost', 'ParkingSpaceCost', 'NumberPlateCost','FarRegistFeeWithTax', 'TradeInMaintenanceFeeWithTax', 'InheritedInsuranceFeeWithTax', 'OutJurisdictionRegistFeeWithTax'),'CostArea', null, null, null, calcTotalAmount); calcTotalAmount()" })%>--%>
                <%//Mod 2021/05/07 yano #4045 Chrome対応%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CostAreaCode", "CostAreaName", "'/CostArea/CriteriaDialog'", "0", "triggerEvent(document.getElementById('CostAreaCode'), 'change');" }); %>
                <%--<%Html.RenderPartial("SearchButtonControl", new string[] { "CostAreaCode", "CostAreaName", "'/CostArea/CriteriaDialog'", "0", "document.getElementById('CostAreaCode').fireEvent('onchange');" }); %>--%>
                <span id="CostAreaName"><%=Model.CostArea != null ? Model.CostArea.CostAreaName : ""%></span>
            <%}else{ %>
                <%=Html.TextBox("CostAreaCode", Model.CostAreaCode, new { @class = "readonly alphanumeric", style = "width:100px", @readonly="readonly", onchange ="GetMasterDetailFromCode('CostAreaCode',new Array('CostAreaName','ParkingSpaceFeeWithTax', 'InspectionRegistFeeWithTax', 'TradeInFeeWithTax', 'PreparationFeeWithTax', 'RequestNumberFeeWithTax', 'RecycleControlFeeWithTax' , 'RequestNumberCost', 'ParkingSpaceCost', 'NumberPlateCost','FarRegistFeeWithTax', 'OutJurisdictionRegistFeeWithTax'),'CostArea', null, null, null, calcTotalAmount); calcTotalAmount()"  })%><%-- Mod 2023/10/05 yano #4184 --%><%--Mod 2023/08/15 yano #4176--%>
                <%--<%=Html.TextBox("CostAreaCode", Model.CostAreaCode, new { @class = "readonly alphanumeric", style = "width:100px", @readonly="readonly", onchange ="GetMasterDetailFromCode('CostAreaCode',new Array('CostAreaName','ParkingSpaceFeeWithTax', 'InspectionRegistFeeWithTax', 'TradeInFeeWithTax', 'PreparationFeeWithTax', 'RequestNumberFeeWithTax', 'RecycleControlFeeWithTax' , 'RequestNumberCost', 'ParkingSpaceCost', 'NumberPlateCost','FarRegistFeeWithTax', 'TradeInMaintenanceFeeWithTax', 'InheritedInsuranceFeeWithTax', 'OutJurisdictionRegistFeeWithTax'),'CostArea', null, null, null, calcTotalAmount); calcTotalAmount()"  })%>--%>
                <% //Mod 2021/11/11 yano #4112%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CostAreaCode", "CostAreaName", "'/CostArea/CriteriaDialog'", "1", "triggerEvent(document.getElementById('CostAreaCode'), 'change');" }); %>
                <%--<%Html.RenderPartial("SearchButtonControl", new string[] { "CostAreaCode", "CostAreaName", "'/CostArea/CriteriaDialog'", "1", "document.getElementById('CostAreaCode').fireEvent('onchange');" }); %>--%>
                <span id="CostAreaName"><%=Model.CostArea != null ? Model.CostArea.CostAreaName : ""%></span>
            <%} %>
     </td>
   </tr>
    <tr>
        <th style="width: 100px">
            所有者コード
        </th>
        <td colspan="3" style="text-align:left">
            <%if(Model.OwnerEnabled){ %>
                <%=Html.TextBox("PossesorCode", Model.PossesorCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "10", onblur = "GetMasterDetailFromCode('PossesorCode',new Array('PossesorName','PossesorAddress'),'Customer')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "PossesorCode", "PossesorName", "'/Customer/CriteriaDialog'", "0" }); %>
            <%}else{ %>
                <%=Html.TextBox("PossesorCode", Model.PossesorCode, new { @class = "readonly alphanumeric", style = "width:100px", @readonly="readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "PossesorCode", "PossesorName", "'/Customer/CriteriaDialog'", "1" }); %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>所有者氏名</th>
        <td colspan="3"><%=Html.TextBox("PossesorName", ViewData["PossesorName"], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%></td>
    </tr>
    <tr>
        <th>所有者住所</th>
        <td colspan="3"><%=Html.TextBox("PossesorAddress", ViewData["PossesorAddress"], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%></td>
    </tr>
    <tr>
        <th>使用者コード</th>
        <td colspan="3" style="text-align:left">
            <%if (Model.OwnerEnabled) { %>
                <%=Html.TextBox("UserCode", Model.UserCode, new { @class = "alphanumeric", style="width:100px", maxlength="10", onblur = "GetMasterDetailFromCode('UserCode',new Array('UserName','UserAddress','PrincipalPlace'),'Customer')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "UserCode", "UserName", "'/Customer/CriteriaDialog'", "0" }); %>
            <%}else{ %>
                <%=Html.TextBox("UserCode", Model.UserCode, new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "UserCode", "UserName", "'/Customer/CriteriaDialog'", "1" }); %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>使用者氏名</th>
        <td colspan="3"><%=Html.TextBox("UserName", ViewData["UserName"], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%></td>
    </tr>
    <tr>
        <th>使用者住所</th>
        <td colspan="3"><%=Html.TextBox("UserAddress", ViewData["UserAddress"], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%></td>
    </tr>
    <tr>
        <th>使用者本拠地</th>
        <td colspan="3">
            <%if(Model.OwnerEnabled){ %>
                <%=Html.TextBox("PrincipalPlace", Model.PrincipalPlace, new { style = "width:300px", maxlength = "200" })%>
            <%}else{ %>
                <%=Html.TextBox("PrincipalPlace", Model.PrincipalPlace, new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            管理番号
        </th>
        <td colspan="3">
            <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "readonly", @readonly = "readonly", style = "width:300px" })%>
        </td>
    </tr>

    <tr>
        <th>
            登録種別
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.DropDownList("RegistrationType", (IEnumerable<SelectListItem>)ViewData["RegistrationTypeList"], new { style = "width:105px;height:20px" })%>
            <%} else { %>
            <%=Html.DropDownList("RegistrationType", (IEnumerable<SelectListItem>)ViewData["RegistrationTypeList"], new { style = "width:105px;height:20px", @disabled = "disabled" })%>
            <%=Html.Hidden("RegistrationType", Model.RegistrationType) %>
            <%} %>
        </td>
        <th>
            登録支局
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            希望番号
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("RequestPlateNumber", Model.RequestPlateNumber, new { @class = "alphanumeric", style = "width:100px", maxlength = "4" })%>
            <%} else { %>
            <%=Html.TextBox("RequestPlateNumber", Model.RequestPlateNumber, new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            所有権留保
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.DropDownList("OwnershipReservation", (IEnumerable<SelectListItem>)ViewData["OwnershipReservationList"], new { style = "width:105px;height:20px" })%>
            <%} else { %>
            <%=Html.DropDownList("OwnershipReservation", (IEnumerable<SelectListItem>)ViewData["OwnershipReservationList"], new { style = "width:105px;height:20px", @disabled = "disabled" })%>
            <%=Html.Hidden("OwnershipReservation", Model.OwnershipReservation) %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            自賠責加入
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.DropDownList("CarLiabilityInsuranceType", (IEnumerable<SelectListItem>)ViewData["CarLiabilityInsuranceTypeList"], new { style = "width:105px;height:20px" })%>
            <%} else { %>
            <%=Html.DropDownList("CarLiabilityInsuranceType", (IEnumerable<SelectListItem>)ViewData["CarLiabilityInsuranceTypeList"], new { style = "width:105px;height:20px", @disabled = "disabled" })%>
            <%=Html.Hidden("CarLiabilityInsuranceType", Model.CarLiabilityInsuranceType) %>
            <%} %>
        </td>
        <th>
            印鑑証明
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("SealSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.SealSubmitDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("SealSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.SealSubmitDate), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            委任状
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("ProxySubmitDate", string.Format("{0:yyyy/MM/dd}", Model.ProxySubmitDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("ProxySubmitDate", string.Format("{0:yyyy/MM/dd}", Model.ProxySubmitDate), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            車庫証明
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("ParkingSpaceSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.ParkingSpaceSubmitDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("ParkingSpaceSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.ParkingSpaceSubmitDate), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            自賠責
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("CarLiabilityInsuranceSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.CarLiabilityInsuranceSubmitDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("CarLiabilityInsuranceSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.CarLiabilityInsuranceSubmitDate), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            所有権留保書類
        </th>
        <td>
            <%if (Model.RegistEnabled) { %>
            <%=Html.TextBox("OwnershipReservationSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.OwnershipReservationSubmitDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%} else { %>
            <%=Html.TextBox("OwnershipReservationSubmitDate", string.Format("{0:yyyy/MM/dd}", Model.OwnershipReservationSubmitDate), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>備考</th>
        <td colspan="3">
            <%if(Model.RegistEnabled){ %>
            <%=Html.TextBox("Memo", Model.Memo, new { style="width:300px",maxlength="200" })%>
            <%}else{ %>
            <%=Html.TextBox("Memo", Model.Memo, new { @class = "readonly", style = "width:300px", maxlength = "200", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>  
</table>