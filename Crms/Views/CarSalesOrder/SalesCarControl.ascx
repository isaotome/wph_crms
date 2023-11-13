<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao"%>
<%--販売車両情報--%>
<table class="input-form-slim">
    <tr>
        <th colspan="6" class="input-form-title">販売車両情報</th>
    </tr>
    <tr>
        <th style="width: 80px">
            新中区分
        </th>
        <td style="width: 120px">
            <%if(Model.CarEnabled){ %>
                <%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { style = "width:120px;height:20px" , onchange = "GetRequiredOptionByCarGradeCode('CarGradeCode', " + Model.CarSalesLine.Count +")"})%>
            <%}else{ %>
                <%if(!string.IsNullOrEmpty(Model.SalesOrderStatus) && Model.SalesOrderStatus.Equals("002")) {%>
                    <%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { style = "width:120px;height:20px" , onchange = "NewUsedCalcCheck('CarGradeCode', " + Model.CarSalesLine.Count +")"})%>
                <%}else{%>
                    <%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { @class = "readonly", style = "width:120px;height:20px", @disabled = "disabled" })%>
                    <%=Html.Hidden("NewUsedType",Model.NewUsedType) %>
                <%} %>
            <%} %>
        </td>
        <th style="width: 80px">
            販売区分
        </th>
        <td style="width: 120px">
            <%if(Model.CarEnabled){ %>
                <%=Html.DropDownList("SalesType", (IEnumerable<SelectListItem>)ViewData["SalesTypeList"], new { style = "width:120px;height:20px", onchange = "if(this.value == '003' || this.value == '004' || this.value == '008' ){ setReadOnly('CostAreaCode', true, 'readonly alphanumeric'); var element = document.getElementById('CostAreaCode'); element.value = ''; triggerEvent(element, 'change');}else{ setReadOnly('CostAreaCode', false, 'alphanumeric')} if(this.value == '004' || this.value == '008'){ ClearCostAmount();} calcTotalOptionAmount(); GetAcquisitionTax(document.getElementById('EPDiscountTaxList').value, document.getElementById('RequestRegistDate').value);calcTotalAmount()" })%><%//Mod 2021/11/11 yano #4112 %>
                <%--<%=Html.DropDownList("SalesType", (IEnumerable<SelectListItem>)ViewData["SalesTypeList"], new { style = "width:120px;height:20px", onchange = "if(this.value == '003' || this.value == '004' || this.value == '008' ){ setReadOnly('CostAreaCode', true, 'readonly alphanumeric'); var element = document.getElementById('CostAreaCode'); element.value = ''; element.fireEvent('onchange'); }else{ setReadOnly('CostAreaCode', false, 'alphanumeric')} if(this.value == '004' || this.value == '008'){ ClearCostAmount();} calcTotalOptionAmount(); GetAcquisitionTax(document.getElementById('EPDiscountTaxList').value, document.getElementById('RequestRegistDate').value);calcTotalAmount()" })%><%//Mod 2020/01/06 yano #4029 %>--%>
            <%}else{ %>
                <%=Html.DropDownList("SalesType", (IEnumerable<SelectListItem>)ViewData["SalesTypeList"], new { @class = "readonly", style = "width:120px;height:20px", @disabled = "disabled" })%>
                <%=Html.Hidden("SalesType",Model.SalesType) %>
            <%} %>
        </td>
        <th style="width: 80px">
            グレードコード
        </th>
        <td style="width: 120px">
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", style = "width:93px", maxlength = "30", onchange = "GradeChangeCalcCheck('CarGradeCode', " + Model.CarSalesLine.Count +")"})%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog?CarBrandName='+encodeURI(document.getElementById('CarBrandName').value)", "0", "$('#ExteriorColorCode').focus(); GradeChangeCalcCheck('CarGradeCode', " + Model.CarSalesLine.Count + ");" }); %>
            <%} else { %>
                <%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "readonly alphanumeric", style = "width:93px", maxlength = "30", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog?CarBrandName='+encodeURI(document.getElementById('CarBrandName').value)", "1", "$('#ExteriorColorCode').focus()" }); %> 
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            メーカー名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("MakerName", Model.MakerName, new { maxlength = "50", style = "width:115px" })%>
            <%} else { %>
                <%=Html.TextBox("MakerName", Model.MakerName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            ブランド名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("CarBrandName", Model.CarBrandName, new { maxlength = "50", style = "width:115px" })%>
            <%} else { %>
                <%=Html.TextBox("CarBrandName", Model.CarBrandName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            車種名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("CarName", Model.CarName, new { maxlength = "50", style = "width:115px" })%>
            <%} else { %>
                <%=Html.TextBox("CarName", Model.CarName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            グレード名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("CarGradeName", Model.CarGradeName, new { maxlength = "50", style = "width:115px" })%>
            <%} else { %>
                <%=Html.TextBox("CarGradeName", Model.CarGradeName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            外装色コード
        </th>
        <td>
            <%if(Model.CarEnabled){ %>
                <%=Html.TextBox("ExteriorColorCode", Model.ExteriorColorCode, new { maxlength = "8", style = "width:94px", @class = "alphanumeric", onblur = "GetCarColorMasterFromCode('CarGradeCode','ExteriorColorCode','ExteriorColorName','CarColor')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ExteriorColorCode", "ExteriorColorName", "'/CarColor/CriteriaDialog?ExteriorColorFlag=true&CarGradeCode='+document.forms[0].CarGradeCode.value", "0" }); %>
            <%}else{ %>
                <%=Html.TextBox("ExteriorColorCode", Model.ExteriorColorCode, new { @class = "readonly alphanumeric", style = "width:94px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ExteriorColorCode", "ExteriorColorName", "'/CarColor/CriteriaDialog'", "1" }); %>
            <%} %>
        </td>
        <th>
            内装色コード
        </th>
        <td>
            <%if(Model.CarEnabled){ %>
                <%=Html.TextBox("InteriorColorCode", Model.InteriorColorCode, new { maxlength = "8", style = "width:94px", @class = "alphanumeric", onblur = "GetCarColorMasterFromCode('CarGradeCode','InteriorColorCode','InteriorColorName','CarColor')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "InteriorColorCode", "InteriorColorName", "'/CarColor/CriteriaDialog?InteriorColorFlag=true&CarGradeCode='+document.forms[0].CarGradeCode.value", "0" }); %>
            <%}else{ %>
                <%=Html.TextBox("InteriorColorCode", Model.InteriorColorCode, new { maxlength = "8", style="width:94px", @class = "readonly alphanumeric", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "InteriorColorCode", "InteriorColorName", "'/CarColor/CriteriaDialog'", "1" }); %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            型式
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("ModelName", Model.ModelName, new { maxlength = "20", style = "width:115px", @class = "alphanumeric" })%>
            <%} else { %>
                <%=Html.TextBox("ModelName", Model.ModelName, new { style = "width:115px", @class = "readonly alphanumeric", @readonly = "readonly" })%>
            <%}%>
        </td>
        <th>
            外装色名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("ExteriorColorName", Model.ExteriorColorName, new { style = "width:115px", maxlength = "50" })%>
            <%} else { %>
                <%=Html.TextBox("ExteriorColorName", Model.ExteriorColorName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            内装色名
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("InteriorColorName", Model.InteriorColorName, new { style = "width:115px", maxlength = "50" })%>
            <%} else { %>
                <%=Html.TextBox("InteriorColorName", Model.InteriorColorName, new { @class = "readonly", style = "width:115px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            走行距離
        </th>
        <td>
            <%if (Model.CarEnabled) { %>
                <%=Html.TextBox("Mileage", Model.Mileage, new { maxlength = "10", style = "width:64px", @class = "numeric" })%><%=Html.DropDownList("MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { style = "height:20px" })%>
            <%} else { %>
                <%=Html.TextBox("Mileage", Model.Mileage, new { style = "width:64px", @class = "readonly numeric", @readonly = "readonly" })%><%=Html.DropDownList("MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { style = "height:20px", @disabled = "disabled" })%><%=Html.Hidden("MileageUnit", Model.MileageUnit) %>
           <%} %>
        </td>
        <th>
            車台番号
        </th>
        <td colspan="3" style="text-align:left">
            <%if (Model.CarEnabled && Model.CarVinEnabled) { //Mod 2018/08/07 yano #3911%>
            　　<%-- Mod 2019/09/04 yano #4011 --%>
                <%//Mod 2018/07/30 arc yano #3919 AA、依廃の場合は非課税項目諸費用項目は設定しない %>
                <%// Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える 車台番号を手入力した場合、それに紐づく車両情報を取得するように修正%>
                <%=Html.TextBox("Vin", Model.Vin, new { maxlength = "20", style = "width:180px", onchange = "decisionSetCost(1," + Model.CarSalesLine.Count +");" })%>
                <%--<%=Html.TextBox("Vin", Model.Vin, new { maxlength = "20", style = "width:180px", onchange = "decisionSetCost(1," + Model.CarSalesLine.Count +"); /*setTimeout( function(){ GetRequiredOptionByVin('Vin'," + Model.CarSalesLine.Count +")}, 500)*/" })%><%--Del 2019/09/04 yano ref--%>
                <%-- Mod 2019/09/04 yano #4011 --%>
                <%--Del 2019/09/04 yano ref--%>
                <%//Mod 2018/07/30 arc yano #3919 AA、依廃の場合は諸費用は０ %>
                <%// Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える ダイアログで項目を選択した場合にのみ、各情報を取得するように修正%>
                <%// Mod 2015/07/29 arc nakayama #3217_デモカーが販売できてしまう問題の改善 車両伝票の車台番号のルックアップから本関数が呼ばれたかどうかを判定する引数追加 %> 
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "if(dRet){ decisionSetCost(2," + Model.CarSalesLine.Count + ")} " }); %>
                <%--<%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "if(dRet){ decisionSetCost(2," + Model.CarSalesLine.Count + "); /*setTimeout( function(){calcSalesPrice(1); GetRequiredOptionByVin('Vin'," + Model.CarSalesLine.Count + ")}, 500)*/} " });%> --%>
            <%} else { %>
                <%// Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える ダイアログで項目を選択した場合にのみ、各情報を取得するように修正%>
                <%// Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える 車台番号を手入力した場合、それに紐づく車両情報を取得するように修正%>
                <%=Html.TextBox("Vin", Model.Vin, new { @class = "readonly", maxlength = "20", style = "width:180px", @readonly = "readonly", onchange = "if(dRet){ decisionSetCost(1); }"})%><%//Mod 2018/07/30 arc yano #3919 AA、依廃の場合は諸費用は０ %><%--Del 2019/09/04 yano ref--%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "1" }); %>
            <%} %>
        </td>
    </tr>
</table>