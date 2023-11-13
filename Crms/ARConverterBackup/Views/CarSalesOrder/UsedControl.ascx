<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<%//Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう %>
<span style="color:blue"><b>※注意：下取車がある場合、車両仕入の仕入価格(仕入データが存在する場合のみ)及び、査定データの査定金額に下取車充当金を反映します。</b></span>
<%for (int m = 1; m <= 3; m++) { %>
<table class="input-form-slim">
    <tr>
        <th colspan="6" class="input-form-title">
            下取車(<%=m %>台目)
        </th>
        <%string TradeInVinLock = "TradeInVinLock" + m;%>
    </tr>
    <tr>
        <th style="width: 100px">
            下取車充当金
        </th>
        <td style="width: 100px">
            <!--Mod 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう-->
            <%if (Model.UsedEnabled && !string.IsNullOrWhiteSpace(ViewData[TradeInVinLock].ToString()) && ViewData[TradeInVinLock].ToString().Equals("0"))
              { %>
                <%=Html.TextBox("TradeInAmount" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInAmount" + m)), new { @class = "money", style = "width:100px", maxlength = "10", onchange = "setTradeInFee(" + m + ")", onkeyup = "calcTotalAmount()" }) %>
            <%}else{ %>
                <%=Html.TextBox("TradeInAmount" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInAmount" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            車台番号
        </th>
        <td colspan="3">
            <% // Mod 2014/07/23 arc amii chrome対応 style指定に「box-sizing: border-box;」を追加し、欄からはみ出るのを修正 %>
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInVin"+m, CommonUtils.GetModelProperty(Model,"TradeInVin" + m), new { style="width:100%;box-sizing: border-box;", maxlength = "20",onkeyup="setTradeInCost()" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInVin" + m, CommonUtils.GetModelProperty(Model, "TradeInVin" + m), new { @class = "readonly", style = "width:100%;box-sizing: border-box;", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>        
        <th style="width: 100px">
            自税充当
        </th>
        <td style="width: 100px">
            <!--Mod 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう-->
            <%if (Model.UsedEnabled && !string.IsNullOrWhiteSpace(ViewData[TradeInVinLock].ToString()) && ViewData[TradeInVinLock].ToString().Equals("0"))
              { %>
                <%=Html.TextBox("TradeInUnexpiredCarTax"+m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model,"TradeInUnexpiredCarTax" + m)), new { @class = "money", style="width:100px", maxlength = "10", onkeyup = "calcTotalAmount()", onchange = "calcTotalAmount()" })%><%//Mod 2022/06/08 yano #4137 %>
                <%--<%=Html.TextBox("TradeInUnexpiredCarTax"+m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model,"TradeInUnexpiredCarTax" + m)), new { @class = "money", style="width:100px", maxlength = "10", onkeyup = "calcTotalAmount()" })%>--%>
            <%}else{ %>
                <%=Html.TextBox("TradeInUnexpiredCarTax" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInUnexpiredCarTax" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            登録番号
        </th>
        <td colspan="3">
            <% // Mod 2014/07/23 arc amii chrome対応 style指定に「box-sizing: border-box;」を追加し、欄からはみ出るのを修正 %>
            <%if (Model.UsedEnabled) { %>
                <%=Html.TextBox("TradeInRegistrationNumber" + m, CommonUtils.GetModelProperty(Model, "TradeInRegistrationNumber" + m), new { style = "width:100%;box-sizing: border-box;", maxlength = "20" })%>
            <%} else { %>
                <%=Html.TextBox("TradeInRegistrationNumber" + m, CommonUtils.GetModelProperty(Model, "TradeInRegistrationNumber" + m), new { @class = "readonly", style = "width:100%;box-sizing: border-box;", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル
        </th>
        <td>
            <!--Mod 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう-->
            <%if (Model.UsedEnabled && !string.IsNullOrWhiteSpace(ViewData[TradeInVinLock].ToString()) && ViewData[TradeInVinLock].ToString().Equals("0"))
              { %>
                <%=Html.TextBox("TradeInRecycleAmount" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRecycleAmount" + m)), new { @class = "money", style = "width:100px", maxlength = "10", onkeyup = "calcTotalAmount()", onchange = "calcTotalAmount()" }) %><%//Mod 2022/06/08 yano #4137 %>
                <%--<%=Html.TextBox("TradeInRecycleAmount" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRecycleAmount" + m)), new { @class = "money", style = "width:100px", maxlength = "10", onkeyup = "calcTotalAmount()" }) %>--%>
            <%}else{ %>
                <%=Html.TextBox("TradeInRecycleAmount" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRecycleAmount" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            メーカー
        </th>
        <td>
            <%if (Model.UsedEnabled) { %>
                <%=Html.TextBox("TradeInMakerName" + m, CommonUtils.GetModelProperty(Model, "TradeInMakerName" + m), new { style = "width:100px", maxlength = "50" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInMakerName" + m, CommonUtils.GetModelProperty(Model, "TradeInMakerName" + m), new { @class = "readonly", style = "width:100px", maxlength = "50", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            年式
        </th>
        <td>
            <%if (Model.UsedEnabled) { %>
                <%=Html.TextBox("TradeInManufacturingYear" + m, CommonUtils.GetModelProperty(Model, "TradeInManufacturingYear" + m), new { @class = "numeric", style = "width:100px", maxlength = "50" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInManufacturingYear" + m, CommonUtils.GetModelProperty(Model, "TradeInManufacturingYear" + m), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            下取車価格(税抜)
        </th>
        <td>
            <%=Html.TextBox("TradeInPrice" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInPrice" + m)), new { @class = "readonly money", style = "width:100px", maxlength = "10", @readonly = "readonly" })%>
        </td>
        <th>
            車名
        </th>
        <td>
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInCarName"+m, CommonUtils.GetModelProperty(Model,"TradeInCarName" + m), new { style="width:100px", maxlength = "50" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInCarName" + m, CommonUtils.GetModelProperty(Model, "TradeInCarName" + m), new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            車検満了日
        </th>
        <td>
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInInspectionExpiredDate"+m, string.Format("{0:yyyy/MM/dd}",CommonUtils.GetModelProperty(Model,"TradeInInspectionExpiredDate" + m)), new { @class = "alphanumeric", style="width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInInspectionExpiredDate"+m, string.Format("{0:yyyy/MM/dd}",CommonUtils.GetModelProperty(Model,"TradeInInspectionExpiredDate" + m)), new { @class = "readonly alphanumeric", style="width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            消費税
        </th>
        <td style="width: 100px">
            <%=Html.TextBox("TradeInTax" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInTax" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
        </td>
        <th>
            型式指定
        </th>
        <td>
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInModelSpecificateNumber"+m, CommonUtils.GetModelProperty(Model,"TradeInModelSpecificateNumber" + m), new { style="width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInModelSpecificateNumber"+m, CommonUtils.GetModelProperty(Model,"TradeInModelSpecificateNumber" + m), new { @class = "readonly", style="width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            類別区分
        </th>
        <td>
            <%if (Model.UsedEnabled) { %>
                <%=Html.TextBox("TradeInClassificationTypeNumber" + m, CommonUtils.GetModelProperty(Model, "TradeInClassificationTypeNumber" + m), new { style = "width:100px", maxlength = "50" })%>
            <%} else { %>
                <%=Html.TextBox("TradeInClassificationTypeNumber" + m, CommonUtils.GetModelProperty(Model, "TradeInClassificationTypeNumber" + m), new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            下取車残債△
        </th>
        <td>
            <!--Mod 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう-->
            <%if (Model.UsedEnabled && !string.IsNullOrWhiteSpace(ViewData[TradeInVinLock].ToString()) && ViewData[TradeInVinLock].ToString().Equals("0"))
              {%>
                <%=Html.TextBox("TradeInRemainDebt" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRemainDebt" + m)), new { @class = "money", style="width:100px", maxlength = "10", onkeyup = "calcTotalAmount()", onchange = "calcTotalAmount()" })%><%//Mod 2022/06/08 yano #4137 %>
                <%--<%=Html.TextBox("TradeInRemainDebt" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRemainDebt" + m)), new { @class = "money", style="width:100px", maxlength = "10", onkeyup = "calcTotalAmount()" })%>--%>
            <%} else { %>
                <%=Html.TextBox("TradeInRemainDebt" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInRemainDebt" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            走行距離
        </th>
        <td style="text-align:left">
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInMileage"+m, CommonUtils.GetModelProperty(Model,"TradeInMileage" + m), new { @class = "numeric", style="width:80px", maxlength = "11" })%><%=Html.DropDownList("TradeInMileageUnit" + m, (IEnumerable<SelectListItem>)ViewData["TradeInMileageUnitList" + m], new { style = "height:20px" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInMileage"+m, CommonUtils.GetModelProperty(Model,"TradeInMileage" + m), new { @class = "readonly numeric", style="width:100px", @readonly = "readonly" })%><%=Html.DropDownList("TradeInMileageUnit" + m, (IEnumerable<SelectListItem>)ViewData["TradeInMileageUnitList" + m], new { @disabled = "disabled", style = "height:20px" })%>
                <%=Html.Hidden("TradeInMileageUnit" + m, CommonUtils.GetModelProperty(Model, "TradeInMileageUnit" + m)) %>
            <%} %>
        </td>
        <%//Add 2022/06/23 yano #4140%>
         <th>
            登録名義人
        </th>
        <td>
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInHolderName" + m, CommonUtils.GetModelProperty(Model, "TradeInHolderName" + m), new { style="width:100px", maxlength="80" })%>
            <%}else{ %>
                <%=Html.TextBox("TradeInHolderName" + m, CommonUtils.GetModelProperty(Model, "TradeInHolderName" + m), new { style="width:100px", maxlength="80" , @class = "readonly", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            下取充当金(総額)
        </th>
        <td>
            <%=Html.TextBox("TradeInAppropriation" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInAppropriation" + m)), new { style = "width:100px", @class = "readonly money", @readonly = "readonly" })%>
        </td>
        <th>
            自賠責未経過分
        </th>
        <td style="text-align:left">
            <%if(Model.UsedEnabled){ %>
                <%=Html.TextBox("TradeInUnexpiredLiabilityInsurance"+m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model,"TradeInUnexpiredLiabilityInsurance"+m)),new {@class="money",style="width:100px",maxlength="10"}) %>
            <%}else{ %>
                <%=Html.TextBox("TradeInUnexpiredLiabilityInsurance" + m, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "TradeInUnexpiredLiabilityInsurance" + m)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            抹消登録
        </th>
        <td>
            <%if (Model.UsedEnabled) { %>
                <%=Html.DropDownList("TradeInEraseRegist" + m, (IEnumerable<SelectListItem>)ViewData["TradeInEraseRegistList" + m], new { style = "width:105px;height:20px", onchange = "setTradeInCost()" })%>
            <%}else{ %>
                <%=Html.DropDownList("TradeInEraseRegist" + m, (IEnumerable<SelectListItem>)ViewData["TradeInEraseRegistList" + m], new { style = "width:105px;height:20px", @disabled = "disabled" })%>
                <%=Html.Hidden("TradeInEraseRegist" + m, CommonUtils.GetModelProperty(Model, "TradeInEraseRegist" + m)) %>
            <%} %>
        </td>
    </tr>
</table>
<br />
<%} %>
