<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>

<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<% // Add 2014/07/29 arc amii 課題対応 #3018 非課税自由項目名と課税自由項目名にプレースホルダを表示するJavascript追加 %>
<script type="text/javascript">
    $(function () {
        $(window).load(function () {
            $('input[type=text],input[type=password],textarea').each(function () {
                var thisTitle = $(this).attr('title');
                if (!(thisTitle === '')) {
                    $(this).wrapAll('<span style="text-align:left;display:inline-block;position:relative;"></span>');
                    $(this).parent('span').append('<span class="placeholder">' + thisTitle + '</span>');
                    $(this).removeAttr('title');
                    $('.placeholder').css({
                        top: '4px',
                        left: '5px',
                        fontSize: '100%',
                        lineHeight: '120%',
                        textAlign: 'left',
                        color: '#999',
                        overflow: 'hidden',
                        position: 'absolute',
                        zIndex: '99'
                    }).click(function () {
                        $(this).prev().focus();
                    });

                    $(this).focus(function () {
                        $(this).next('span').css({ display: 'none' });
                    });

                    $(this).blur(function () {
                        var thisVal = $(this).val();
                        if (thisVal === '') {
                            $(this).next('span').css({ display: 'inline-block' });
                        } else {
                            $(this).next('span').css({ display: 'none' });
                        }
                    });

                    var thisVal = $(this).val();

                    if (thisVal === '') {
                        $(this).next('span').css({ display: 'inline-block' });
                    } else {
                        $(this).next('span').css({ display: 'none' });
                    }
                }
            });
        });
    });

</script>


            <%--税金・保険料・預かり法定費用--%>
            <table class="input-form-slim" onkeyup="calcTotalAmount()">
                <tr>
                    <th colspan="6" class="input-form-title">税金等</th>
                </tr>
                <tr>
                    <th style="width:90px">
                        登録希望日
                    </th>
                    <td style="width:77px">
                        <% //2016.07.08 業務依頼 if (Model.CostEnabled) { %>
                        <% if (Model.CarRegEnabled) { %>
                            <%//Mod 2019/10/17 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り %>
                            <%=Html.TextBox("RequestRegistDate", string.Format("{0:yyyy/MM/dd}", Model.RequestRegistDate), new { @class = "alphanumeric", style = "width:71px", maxlength = "10", onchange = "GetCarTax(); GetAcquisitionTax($('#EPDiscountTaxList').val(), $(this).val());" })%>
                            <%--<%=Html.TextBox("RequestRegistDate", string.Format("{0:yyyy/MM/dd}", Model.RequestRegistDate), new { @class = "alphanumeric", style = "width:71px", maxlength = "10", onchange = "GetCarTax()" })%>--%>
                        <%}else{ %>
                            <%=Html.TextBox("RequestRegistDate", string.Format("{0:yyyy/MM/dd}", Model.RequestRegistDate), new { @class = "readonly alphanumeric", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <%-- Mod 2019/09/04 yano #4011 --%>
                    <th style="width:120px">
                        自動車税環境性能割
                    </th>
                    <td style="width:137px">
                        <% //2016.07.08 業務依頼 if (Model.CostEnabled) { %>
                        <% if (Model.CarRegEnabled) { %>
                            <%=Html.TextBox("AcquisitionTax", string.Format("{0:N0}", Model.AcquisitionTax), new { @class = "money", maxlength = "10", style = "width:55px" })%>
                            <%=Html.DropDownList("EPDiscountTaxId", (IEnumerable<SelectListItem>)ViewData["EPDiscountTaxList"], new { @class = "alphanumeric", id = "EPDiscountTaxList", style = "width:70px", onchange = "GetAcquisitionTax(this.value, $('#RequestRegistDate').val())" })%><%//Mod 2019/10/17 yano #4022 %>
                            <%--<%=Html.DropDownList("EPDiscountTaxId", (IEnumerable<SelectListItem>)ViewData["EPDiscountTaxList"], new { @class = "alphanumeric", id = "EPDiscountTaxList", style = "width:70px", onchange = "GetAcquisitionTax(this.value, $('#SalesPrice').val())" })%>--%>
                            <%--<input type="button" value="計算" style="width:30px" onclick="GetAcquisitionTax()" />--%>
                        <%}else{ %>
                            <%=Html.TextBox("AcquisitionTax", string.Format("{0:N0}", Model.AcquisitionTax), new { @class = "readonly money", style = "width:40px", @readonly = "readonly" })%>
                            <%=Html.DropDownList("EPDiscountTaxId", (IEnumerable<SelectListItem>)ViewData["EPDiscountTaxList"], new { @class = "readonly alphanumeric", id = "EPDiscountTaxList", style = "width:70px", onchange = "GetAcquisitionTax(this.value, $('#RequestRegistDate').val())", @disabled = "disabled"  })%><%//Mod 2019/10/17 yano #4022 %>
                            <%--<%=Html.DropDownList("EPDiscountTaxId", (IEnumerable<SelectListItem>)ViewData["EPDiscountTaxList"], new { @class = "readonly alphanumeric", id = "EPDiscountTaxList", style = "width:70px", onchange = "GetAcquisitionTax(this.value, $('#SalesPrice').val())", @disabled = "disabled"  })%>--%>
                            <%=Html.Hidden("EPDiscountTaxId", Model.EPDiscountTaxId)%>
                        <%--<input type="button" value="計算" style="width:30px" disabled="disabled" />--%>
                        <%} %>
                    </td>
                    <th style="width:90px">
                        自賠責保険料
                    </th>
                    <td style="width:77px">
                        <% //2016.07.08 業務依頼 if (Model.CostEnabled) { %>
                        <% if (Model.CarRegEnabled) { %>
                            <%=Html.TextBox("CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { @class = "money", maxlength = "10", style = "width:50px" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarLiabilityInsurance", "", "'/CarLiabilityInsurance/CriteriaDialog'", "0", "calcTotalAmount()" }); %>
                        <%}else{ %>
                            <%=Html.TextBox("CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarLiabilityInsurance", "", "'/CarLiabilityInsurance/CriteriaDialog'", "1", "calcTotalAmount()" }); %>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th>
                        自動車税種別割 <%-- Mod 2019/09/04 yano #4011 --%>
                    </th>
                    <td>
                        <% //2016.07.08 業務依頼 if (Model.CostEnabled) { %>
                        <% if (Model.CarRegEnabled) { %>
                            <%=Html.TextBox("CarTax", string.Format("{0:N0}", Model.CarTax), new { @class = "money", style = "width:50px", maxlength = "10" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarTax", "", "'/CarTax/CriteriaDialog'", "0", "calcTotalAmount()" }); %>
                        <%}else{ %>
                            <%=Html.TextBox("CarTax", string.Format("{0:N0}", Model.CarTax), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarTax", "", "'/CarTax/CriteriaDialog'", "1", "calcTotalAmount()" }); %>
                        <%} %>
                    </td>
                    <th>
                        自動車重量税
                    </th>
                    <td>
                        <% //2016.07.08 業務依頼 if (Model.CostEnabled) { %>
                        <% if (Model.CarRegEnabled) { %>
                            <%=Html.TextBox("CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { @class = "money", maxlength = "10", style = "width:50px" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarWeightTax", "", "'/CarWeightTax/CriteriaDialog'", "0", "calcTotalAmount()" }); %>
                        <%}else{ %>
                            <%=Html.TextBox("CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%><%Html.RenderPartial("SearchButtonControl", new string[] { "CarWeightTax", "", "'/CarWeightTax/CriteriaDialog'", "1", "calcTotalAmount()" }); %>
                        <%} %>
                    </td>
                    <%//Del 2023/01/11 yano #4158 %>
                    <%//Mod 2018/11/15 yano #3936 任意保険料の項目を削除 %>
                    <th colspan="2"></th>
                </tr>
            </table>
            <br />
            <table class="input-form-slim" onkeyup="calcTotalAmount()">
                <tr>
                    <th colspan="6" class="input-form-title">その他非課税</th>
                </tr>
                <tr>
                    <th style="width:120px">
                        車庫証明証紙代
                    </th>
                    <td style="width:77px">
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("ParkingSpaceCost", string.Format("{0:N0}", Model.ParkingSpaceCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("ParkingSpaceCost", string.Format("{0:N0}", Model.ParkingSpaceCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <th style="width:120px">
                        検査登録印紙代
                    </th>
                    <td style="width:77px">
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("InspectionRegistCost", string.Format("{0:N0}", Model.InspectionRegistCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("InspectionRegistCost", string.Format("{0:N0}", Model.InspectionRegistCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <th style="width:120px">
                        ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(一般)
                    </th>
                    <td style="width:77px">
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("NumberPlateCost", string.Format("{0:N0}", Model.NumberPlateCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("NumberPlateCost", string.Format("{0:N0}", Model.NumberPlateCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
               </tr>
                <tr>
                    <th>
                        下取車登録印紙代
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TradeInCost", string.Format("{0:N0}", Model.TradeInCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("TradeInCost", string.Format("{0:N0}", Model.TradeInCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <th>
                        ﾘｻｲｸﾙ預託金
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RecycleDeposit", string.Format("{0:N0}", Model.RecycleDeposit), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("RecycleDeposit", string.Format("{0:N0}", Model.RecycleDeposit), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <th>
                        ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望)
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RequestNumberCost", string.Format("{0:N0}", Model.RequestNumberCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("RequestNumberCost", string.Format("{0:N0}", Model.RequestNumberCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                     <% // Add 2014/07/25 arc amii 課題対応 #3018 画面に収入印紙代と下取自動車税預り金項目を追加 %>
                    <th>
                        収入印紙代
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RevenueStampCost", string.Format("{0:N0}", Model.RevenueStampCost), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("RevenueStampCost", string.Format("{0:N0}", Model.RevenueStampCost), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                    <th>
                        下取自動車税種別割預り金   <%-- Mod 2019/09/04 yano #4011 --%>
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TradeInCarTaxDeposit", string.Format("{0:N0}", Model.TradeInCarTaxDeposit), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("TradeInCarTaxDeposit", string.Format("{0:N0}", Model.TradeInCarTaxDeposit), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>

                    <%//Mod 2023/01/11 yano #4158%>
                    <th style="padding:0px">
                      任意保険料
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("VoluntaryInsuranceAmount", string.Format("{0:N0}", Model.VoluntaryInsuranceAmount), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("VoluntaryInsuranceAmount", string.Format("{0:N0}", Model.VoluntaryInsuranceAmount), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>
                </tr>
                
                <%//Add 2023/01/11 yano #4158%>
                 <tr>
                    <th style="padding:0px">
                        <%=Html.Hidden("TaxFreeFieldId", "0") %>
                        <%if(Model.CostEnabled){ %>

                            <% // Mod 2014/07/29 arc amii 課題対応 プレースホルダ用のtitle属性追加(上のJavascriptにて使用 %>
                            <%=Html.TextBox("TaxFreeFieldName", Model.TaxFreeFieldName, new { size = "10", maxlength = "50" , title="その他"})%>
                        <%}else{ %>
                            <%=Html.TextBox("TaxFreeFieldName", Model.TaxFreeFieldName, new { @class = "readonly", size = "10", maxlength = "50", @readonly = "readonly" })%>
                        <%} %>
                    </th>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TaxFreeFieldValue", string.Format("{0:N0}", Model.TaxFreeFieldValue), new { @class = "money", maxlength = "10", style = "width:71px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("TaxFreeFieldValue", string.Format("{0:N0}", Model.TaxFreeFieldValue), new { @class = "readonly money", maxlength = "10", style = "width:71px", @readonly = "readonly" })%>
                        <%} %>
                    </td>

                    <th colspan="4"></th>
                </tr>
            </table>
            <br />
            <%--販売諸費用--%>
            <%--Mod 2023/10/05 yano #4184 レイアウト変更 %>
            <%--Mod 2023/08/15 yano #4176 入力を税抜→税込に変更、レイアウト変更--%>
            <table class="input-form-slim" onkeyup="calcTotalAmount()">
                <tr>
                    <th colspan="8" class="input-form-title">販売諸費用</th>
                </tr>
                <tr>
                    <th style="width: 130px"></th>
                    <th style="width: 50px">税抜</th>
                    <th style="width: 50px">消費税</th>
                    <th style="width: 50px">税込</th>
                    <th style="width: 130px"></th>
                    <th style="width: 50px">税抜</th>
                    <th style="width: 50px">消費税</th>
                    <th style="width: 50px">税込</th>
                </tr>
                <tr>
                    <th>検査登録手続
                        代行費用</th>
                    <td>
                        <%=Html.TextBox("InspectionRegistFee", string.Format("{0:N0}", Model.InspectionRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("InspectionRegistFeeTax", string.Format("{0:N0}", Model.InspectionRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("InspectionRegistFeeWithTax", string.Format("{0:N0}", (Model.InspectionRegistFee ?? 0) + (Model.InspectionRegistFeeTax ?? 0)), new {  @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("InspectionRegistFeeWithTax", string.Format("{0:N0}", (Model.InspectionRegistFee ?? 0) + (Model.InspectionRegistFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                         <%} %>
                    </td>
                    <th>希望ナンバー申請手数料</th>
                    <td>
                        <%=Html.TextBox("RequestNumberFee", string.Format("{0:N0}", Model.RequestNumberFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("RequestNumberFeeTax", string.Format("{0:N0}", Model.RequestNumberFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RequestNumberFeeWithTax", string.Format("{0:N0}", (Model.RequestNumberFee ?? 0) + (Model.RequestNumberFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("RequestNumberFeeWithTax", string.Format("{0:N0}", (Model.RequestNumberFee ?? 0) + (Model.RequestNumberFeeTax ?? 0)), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                 </tr>
                 <tr>
                    <th>納車費用</th>
                    <td>
                        <%=Html.TextBox("PreparationFee", string.Format("{0:N0}", Model.PreparationFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("PreparationFeeTax", string.Format("{0:N0}", Model.PreparationFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>    
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("PreparationFeeWithTax", string.Format("{0:N0}", (Model.PreparationFee ?? 0) + (Model.PreparationFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("PreparationFeeWithTax", string.Format("{0:N0}", (Model.PreparationFee ?? 0) + (Model.PreparationFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>  
                    </td>
                    <th>下取車所有権解除手続費用</th>
                    <td>
                        <%=Html.TextBox("TradeInFee", string.Format("{0:N0}", Model.TradeInFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("TradeInFeeTax", string.Format("{0:N0}", Model.TradeInFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TradeInFeeWithTax", string.Format("{0:N0}", (Model.TradeInFee ?? 0) + (Model.TradeInFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("TradeInFeeWithTax", string.Format("{0:N0}", (Model.TradeInFee ?? 0) + (Model.TradeInFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                 </tr>
                 <tr>
                    <th>管轄外登録手続費用</th>
                    <td>
                        <%=Html.TextBox("OutJurisdictionRegistFee", string.Format("{0:N0}", Model.OutJurisdictionRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("OutJurisdictionRegistFeeTax", string.Format("{0:N0}", Model.OutJurisdictionRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("OutJurisdictionRegistFeeWithTax", string.Format("{0:N0}", (Model.OutJurisdictionRegistFee ?? 0) + (Model.OutJurisdictionRegistFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("OutJurisdictionRegistFeeWithTax", string.Format("{0:N0}", (Model.OutJurisdictionRegistFee ?? 0) + (Model.OutJurisdictionRegistFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>

                    <th>県外登録手続代行費用</th>
                    <td>
                        <%=Html.TextBox("FarRegistFee", string.Format("{0:N0}", Model.FarRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("FarRegistFeeTax", string.Format("{0:N0}", Model.FarRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("FarRegistFeeWithTax", string.Format("{0:N0}", (Model.FarRegistFee ?? 0) + (Model.FarRegistFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("FarRegistFeeWithTax", string.Format("{0:N0}", (Model.FarRegistFee ?? 0) + (Model.FarRegistFeeTax ?? 0)), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th>車庫証明手続代行費用</th>
                    <td>
                        <%=Html.TextBox("ParkingSpaceFee", string.Format("{0:N0}", Model.ParkingSpaceFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("ParkingSpaceFeeTax", string.Format("{0:N0}", Model.ParkingSpaceFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", (Model.ParkingSpaceFee ?? 0) + (Model.ParkingSpaceFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", (Model.ParkingSpaceFee ?? 0) + (Model.ParkingSpaceFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                    <th>リサイクル資金管理料</th>
                    <td>
                        <%=Html.TextBox("RecycleControlFee", string.Format("{0:N0}", Model.RecycleControlFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("RecycleControlFeeTax", string.Format("{0:N0}", Model.RecycleControlFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", (Model.RecycleControlFee ?? 0) + (Model.RecycleControlFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", (Model.RecycleControlFee ?? 0) + (Model.RecycleControlFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                </tr>
                <%//Add 2023/09/05 #4162%>
                   <tr>
                    <th>自動車税種別割未経過相当額</th>
                    <td>
                        <%=Html.TextBox("CarTaxUnexpiredAmount", string.Format("{0:N0}", Model.CarTaxUnexpiredAmount), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("CarTaxUnexpiredAmountTax", string.Format("{0:N0}", Model.CarTaxUnexpiredAmountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("CarTaxUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarTaxUnexpiredAmount ?? 0) + (Model.CarTaxUnexpiredAmountTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("CarTaxUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarTaxUnexpiredAmount ?? 0) + (Model.CarTaxUnexpiredAmountTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                        <%} %>
                    </td>
                    <th>自賠責未経過相当額</th>
                    <td>
                        <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmount", string.Format("{0:N0}", Model.CarLiabilityInsuranceUnexpiredAmount), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountTax", string.Format("{0:N0}", Model.CarLiabilityInsuranceUnexpiredAmountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarLiabilityInsuranceUnexpiredAmount ?? 0) + (Model.CarLiabilityInsuranceUnexpiredAmountTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarLiabilityInsuranceUnexpiredAmount ?? 0) + (Model.CarLiabilityInsuranceUnexpiredAmountTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th style="padding:0px">
                        <%if(Model.CostEnabled){ %>
                            <% // Mod 2014/07/29 arc amii 課題対応 プレースホルダ用のtitle属性追加(上のJavascriptにて使用 %>
                            <%=Html.TextBox("TaxationFieldName", Model.TaxationFieldName, new { size = "10", maxlength = "50" , title="その他" })%>
                        <%}else{ %>
                            <%=Html.TextBox("TaxationFieldName", Model.TaxationFieldName, new { @class = "readonly", size = "10", maxlength = "50", @readonly = "readonly" })%>
                        <%} %>
                    </th>
                    <td>
                        <%=Html.TextBox("TaxationFieldValue", string.Format("{0:N0}", Model.TaxationFieldValue), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("TaxationFieldValueTax", string.Format("{0:N0}", Model.TaxationFieldValueTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>    
                    </td>
                    <td>
                        <%if (Model.CostEnabled) { %>
                            <%=Html.TextBox("TaxationFieldValueWithTax", string.Format("{0:N0}", (Model.TaxationFieldValue ?? 0) + (Model.TaxationFieldValueTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} else { %>
                            <%=Html.TextBox("TaxationFieldValueWithTax", string.Format("{0:N0}", (Model.TaxationFieldValue ?? 0) + (Model.TaxationFieldValueTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%> 
                        <%} %>
                    </td>
                    <th></th>
                    <td></td>
                    <td></td>
                    <td></td>
                </tr>
           </table>
           <%--<table class="input-form-slim" onkeyup="calcTotalAmount()">
                <tr>
                    <th colspan="8" class="input-form-title">販売諸費用</th>
                </tr>
                <tr>
                    <th style="width: 130px"></th>
                    <th style="width: 50px">税抜</th>
                    <th style="width: 50px">消費税</th>
                    <th style="width: 50px">税込</th>
                    <th style="width: 130px"></th>
                    <th style="width: 50px">税抜</th>
                    <th style="width: 50px">消費税</th>
                    <th style="width: 50px">税込</th>
                </tr>
                <tr>
                    <th>検査登録手続
                        代行費用</th>
                    <td>
                        <%=Html.TextBox("InspectionRegistFee", string.Format("{0:N0}", Model.InspectionRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("InspectionRegistFeeTax", string.Format("{0:N0}", Model.InspectionRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("InspectionRegistFeeWithTax", string.Format("{0:N0}", (Model.InspectionRegistFee ?? 0) + (Model.InspectionRegistFeeTax ?? 0)), new {  @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("InspectionRegistFeeWithTax", string.Format("{0:N0}", (Model.InspectionRegistFee ?? 0) + (Model.InspectionRegistFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                         <%} %>
                    </td>
                    <th>希望ナンバー申請手数料</th>
                    <td>
                        <%=Html.TextBox("RequestNumberFee", string.Format("{0:N0}", Model.RequestNumberFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("RequestNumberFeeTax", string.Format("{0:N0}", Model.RequestNumberFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RequestNumberFeeWithTax", string.Format("{0:N0}", (Model.RequestNumberFee ?? 0) + (Model.RequestNumberFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("RequestNumberFeeWithTax", string.Format("{0:N0}", (Model.RequestNumberFee ?? 0) + (Model.RequestNumberFeeTax ?? 0)), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                 </tr>
                 <tr>
                    <th>納車費用</th>
                    <td>
                        <%=Html.TextBox("PreparationFee", string.Format("{0:N0}", Model.PreparationFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("PreparationFeeTax", string.Format("{0:N0}", Model.PreparationFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>    
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("PreparationFeeWithTax", string.Format("{0:N0}", (Model.PreparationFee ?? 0) + (Model.PreparationFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("PreparationFeeWithTax", string.Format("{0:N0}", (Model.PreparationFee ?? 0) + (Model.PreparationFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>  
                    </td>
                    <th>中古車点検・整備費用</th>
                    <td>
                        <%=Html.TextBox("TradeInMaintenanceFee", string.Format("{0:N0}", Model.TradeInMaintenanceFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("TradeInMaintenanceFeeTax", string.Format("{0:N0}", Model.TradeInMaintenanceFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TradeInMaintenanceFeeWithTax", string.Format("{0:N0}", (Model.TradeInMaintenanceFee ?? 0) + (Model.TradeInMaintenanceFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("TradeInMaintenanceFeeWithTax", string.Format("{0:N0}", (Model.TradeInMaintenanceFee ?? 0) + (Model.TradeInMaintenanceFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                 </tr>
                 <tr>
                    <th>管轄外登録手続費用</th>
                    <td>
                        <%=Html.TextBox("OutJurisdictionRegistFee", string.Format("{0:N0}", Model.OutJurisdictionRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("OutJurisdictionRegistFeeTax", string.Format("{0:N0}", Model.OutJurisdictionRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("OutJurisdictionRegistFeeWithTax", string.Format("{0:N0}", (Model.OutJurisdictionRegistFee ?? 0) + (Model.OutJurisdictionRegistFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("OutJurisdictionRegistFeeWithTax", string.Format("{0:N0}", (Model.OutJurisdictionRegistFee ?? 0) + (Model.OutJurisdictionRegistFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                    <th>下取車所有権解除手続費用</th>
                    <td>
                        <%=Html.TextBox("TradeInFee", string.Format("{0:N0}", Model.TradeInFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("TradeInFeeTax", string.Format("{0:N0}", Model.TradeInFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("TradeInFeeWithTax", string.Format("{0:N0}", (Model.TradeInFee ?? 0) + (Model.TradeInFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("TradeInFeeWithTax", string.Format("{0:N0}", (Model.TradeInFee ?? 0) + (Model.TradeInFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th>県外登録手続代行費用</th>
                    <td>
                        <%=Html.TextBox("FarRegistFee", string.Format("{0:N0}", Model.FarRegistFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("FarRegistFeeTax", string.Format("{0:N0}", Model.FarRegistFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("FarRegistFeeWithTax", string.Format("{0:N0}", (Model.FarRegistFee ?? 0) + (Model.FarRegistFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("FarRegistFeeWithTax", string.Format("{0:N0}", (Model.FarRegistFee ?? 0) + (Model.FarRegistFeeTax ?? 0)), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                    <th>リサイクル資金管理料</th>
                    <td>
                        <%=Html.TextBox("RecycleControlFee", string.Format("{0:N0}", Model.RecycleControlFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("RecycleControlFeeTax", string.Format("{0:N0}", Model.RecycleControlFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", (Model.RecycleControlFee ?? 0) + (Model.RecycleControlFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("RecycleControlFeeWithTax", string.Format("{0:N0}", (Model.RecycleControlFee ?? 0) + (Model.RecycleControlFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th>車庫証明手続代行費用</th>
                    <td>
                        <%=Html.TextBox("ParkingSpaceFee", string.Format("{0:N0}", Model.ParkingSpaceFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("ParkingSpaceFeeTax", string.Format("{0:N0}", Model.ParkingSpaceFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", (Model.ParkingSpaceFee ?? 0) + (Model.ParkingSpaceFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("ParkingSpaceFeeWithTax", string.Format("{0:N0}", (Model.ParkingSpaceFee ?? 0) + (Model.ParkingSpaceFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                    <th>中古車継承整備費用</th>
                    <td>
                        <%=Html.TextBox("InheritedInsuranceFee", string.Format("{0:N0}", Model.InheritedInsuranceFee), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("InheritedInsuranceFeeTax", string.Format("{0:N0}", Model.InheritedInsuranceFeeTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("InheritedInsuranceFeeWithTax", string.Format("{0:N0}", (Model.InheritedInsuranceFee ?? 0) + (Model.InheritedInsuranceFeeTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%}else{ %>
                            <%=Html.TextBox("InheritedInsuranceFeeWithTax", string.Format("{0:N0}", (Model.InheritedInsuranceFee ?? 0) + (Model.InheritedInsuranceFeeTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} %>
                    </td>
                </tr>
                <%//Add 2023/09/05 #4162%>
                   <tr>
                    <th>自動車税種別割未経過相当額</th>
                    <td>
                        <%=Html.TextBox("CarTaxUnexpiredAmount", string.Format("{0:N0}", Model.CarTaxUnexpiredAmount), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("CarTaxUnexpiredAmountTax", string.Format("{0:N0}", Model.CarTaxUnexpiredAmountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("CarTaxUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarTaxUnexpiredAmount ?? 0) + (Model.CarTaxUnexpiredAmountTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("CarTaxUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarTaxUnexpiredAmount ?? 0) + (Model.CarTaxUnexpiredAmountTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                        <%} %>
                    </td>
                    <th>自賠責未経過相当額</th>
                    <td>
                        <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmount", string.Format("{0:N0}", Model.CarLiabilityInsuranceUnexpiredAmount), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountTax", string.Format("{0:N0}", Model.CarLiabilityInsuranceUnexpiredAmountTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                    </td>
                    <td>
                        <%if(Model.CostEnabled){ %>
                            <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarLiabilityInsuranceUnexpiredAmount ?? 0) + (Model.CarLiabilityInsuranceUnexpiredAmountTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%>
                        <%}else{ %>
                            <%=Html.TextBox("CarLiabilityInsuranceUnexpiredAmountWithTax", string.Format("{0:N0}", (Model.CarLiabilityInsuranceUnexpiredAmount ?? 0) + (Model.CarLiabilityInsuranceUnexpiredAmountTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>
                        <%} %>
                    </td>
                </tr>
                <tr>
                    <th style="padding:0px">
                        <%if(Model.CostEnabled){ %>
                            <% // Mod 2014/07/29 arc amii 課題対応 プレースホルダ用のtitle属性追加(上のJavascriptにて使用 %>
                            <%=Html.TextBox("TaxationFieldName", Model.TaxationFieldName, new { size = "10", maxlength = "50" , title="その他" })%>
                        <%}else{ %>
                            <%=Html.TextBox("TaxationFieldName", Model.TaxationFieldName, new { @class = "readonly", size = "10", maxlength = "50", @readonly = "readonly" })%>
                        <%} %>
                    </th>
                    <td>
                        <%=Html.TextBox("TaxationFieldValue", string.Format("{0:N0}", Model.TaxationFieldValue), new { @class = "readonly money", maxlength = "10", style = "width:50px", @readonly = "readonly" })%>
                    </td>
                    <td>
                        <%=Html.TextBox("TaxationFieldValueTax", string.Format("{0:N0}", Model.TaxationFieldValueTax), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%>    
                    </td>
                    <td>
                        <%if (Model.CostEnabled) { %>
                            <%=Html.TextBox("TaxationFieldValueWithTax", string.Format("{0:N0}", (Model.TaxationFieldValue ?? 0) + (Model.TaxationFieldValueTax ?? 0)), new { @class = "money", maxlength = "10", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%>
                        <%} else { %>
                            <%=Html.TextBox("TaxationFieldValueWithTax", string.Format("{0:N0}", (Model.TaxationFieldValue ?? 0) + (Model.TaxationFieldValueTax ?? 0)), new { @class = "readonly money", @readonly = "readonly", style = "width:50px" })%><%//Mod 2023/09/01 yano #4176%> 
                        <%} %>
                    </td>
                    <th></th>
                    <td></td>
                    <td></td>
                    <td></td>
                </tr>
           </table>--%>
           <%//Del 2023/10/5 yano #4184%>