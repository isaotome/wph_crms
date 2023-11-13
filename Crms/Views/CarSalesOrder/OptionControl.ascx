<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<%--オプション（ヘッダ）--%>
<table class="input-form-line">
    <tr>
        <th colspan="7" class="input-form-title">オプション</th>
    </tr>
    <tr>
        <th style="width:20px;height:21px;padding:0px;white-space:nowrap"><%if (Model.OptionEnabled) { %><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="$('#DelLine').val('-1');document.forms[0].action='/CarSalesOrder/Option';formSubmit();" /><%} %></th>
        <th style="width:110px;padding:0px;white-space:nowrap">区分</th><%//Mod 2021/06/09 yano #4091 80px->110px %>
        <th style="width:140px;padding:0px;white-space:nowrap">品番</th><%//Mod 2021/06/09 yano #4091 150px->140px %>
        <th style="width:170px;padding:0px;white-space:nowrap">品名</th><%//Mod 2021/06/09 yano #4091 190px->170px %>
        <th style="width:60px;padding:0px;white-space:nowrap">税抜</th>
        <th style="width:60px;padding:0px;white-space:nowrap">消費税</th>
        <th style="width:60px;padding:0px;white-space:nowrap">税込</th>
    </tr>
</table>
<%--オプション（明細）--%>
<div style="overflow-y: scroll;width:645px;height: 90px">
    <table class="input-form-line">
        <%
          for (int i = 0; i < Model.CarSalesLine.Count;i++ ){
              //Add 2014/05/22 arc yano 各コントロール(テキストボックス、ドロップダウンリスト等)のidを別に設定する。
              string idPrefix = string.Format("line[{0}]_", i);
              string namePrefix = string.Format("line[{0}].", i);
        %>
                <tr>
                    <td style="width:20px;white-space:nowrap">
                        <% // Edit 2014/05/28 arc yano 各コントロールに、idを追加する。%>
                        <%=Html.Hidden(namePrefix + "LineNumber", i + 1, new {id=idPrefix + "LineNumber"})%>
                        <%if(Model.OptionEnabled){ %>
                            <img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="$('#DelLine').val('<%=i %>');document.forms[0].action='/CarSalesOrder/Option';formSubmit();" />
                        <%} %>
                    </td>
                    <td style="width:110px;white-space:nowrap"><%//Mod 2021/06/09 yano #4091 80px->110px %>
                        <%if(Model.OptionEnabled){ %>
                            <%=Html.DropDownList(namePrefix + "OptionType", (IEnumerable<SelectListItem>)ViewData["OptionTypeList[" + i + "]"], new { id=idPrefix + "OptionType", style="width:110px",onchange = "calcTotalOptionAmount(); GetAcquisitionTax(document.getElementById('EPDiscountTaxList').value, document.getElementById('RequestRegistDate').value); calcTotalAmount()" })%><%//Mod 2019/10/17 yano #4022 %><%//Mod 2021/06/09 yano #4091 80px->100px %>
                           <%--<%=Html.DropDownList(namePrefix + "OptionType", (IEnumerable<SelectListItem>)ViewData["OptionTypeList[" + i + "]"], new { id=idPrefix + "OptionType", style="width:80px",onchange = "calcTotalOptionAmount(); GetAcquisitionTax(document.getElementById('EPDiscountTaxList').value, document.getElementById('SalesPrice').value); calcTotalAmount()" })%><%//Mod 2019/09/04 yano #4011%>--%>
                        <%}else{ %>
                        　 <%=Html.DropDownList(namePrefix + "OptionType", (IEnumerable<SelectListItem>)ViewData["OptionTypeList[" + i + "]"], new { id=idPrefix + "OptionType", style="width:110px",@disabled="disabled" })%><%//Mod 2021/06/09 yano #4091 80px->100px %>
                           <%=Html.Hidden(namePrefix + "OptionType", Model.CarSalesLine[i].OptionType, new {id=idPrefix + "OptionType"})%>
                        <%} %>
                    </td>
                    <td style="width:140px;white-space:nowrap"><%//Mod 2021/06/09 yano #4091 150px->140px %>
                        <%if(Model.OptionEnabled){ %>
                            <%=Html.TextBox(namePrefix + "CarOptionCode", Model.CarSalesLine[i].CarOptionCode, new { id=idPrefix + "CarOptionCode", @class = "alphanumeric", style = "width:110px", maxlength = "25" })%><%//Mod 2021/06/09 yano #4091 120px->110px %>
                            <% //Mod 2014/07/14 arc SearchButtonControl に渡すパラメータをname→idに修正。 %>
                            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CarOptionCode", idPrefix + "CarOptionName", "'/CarOption/CriteriaDialog?CarGradeCode='+encodeURI(document.getElementById('CarGradeCode').value)", "0", "GetOptionMasterFromCode(" + i + ")" }); %>
                        <%}else{ %> 
                           <%=Html.TextBox(namePrefix + "CarOptionCode", Model.CarSalesLine[i].CarOptionCode, new { id=idPrefix + "CarOptionCode", @class = "readonly alphanumeric", style = "width:110px", @readonly="readonly" })%><%//Mod 2021/06/09 yano #4091 120px->110px %>
                        <%} %>
                    </td>
                    <td style="width:170px;white-space:nowrap"><%//Mod 2021/06/09 yano #4091 190px->170px %>
                        <%if(Model.OptionEnabled){ %>
                            <% // Edit 2014/05/22 arc yano idを追加、nameの設定の際にnamePrefixを使用するように変更する。%>
                            <%=Html.TextBox(namePrefix + "CarOptionName", Model.CarSalesLine[i].CarOptionName, new { id=idPrefix + "CarOptionName", style="width:160px",maxlength="100" })%><%//Mod 2021/06/09 yano #4091 180px->170px %>
                        <%}else{ %>
                            <%=Html.TextBox(namePrefix + "CarOptionName", Model.CarSalesLine[i].CarOptionName, new { id=idPrefix + "CarOptionName", @class="readonly", style="width:160px",@readonly="readonly" })%><%//Mod 2021/06/09 yano #4091 180px->170px %>
                        <%} %>
                    </td>
                    <td style="width:60px;white-space:nowrap">
                        <%=Html.TextBox(namePrefix + "Amount",string.Format("{0:N0}", Model.CarSalesLine[i].Amount), new { id=idPrefix + "Amount", @class="readonly money", style="width:50px",@readonly="readonly"})%>
                    </td>
                    <td style="width:60px;white-space:nowrap">
                        <%if(Model.OptionEnabled){ %>
                            <%=Html.TextBox(namePrefix + "TaxAmount", string.Format("{0:N0}", Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "TaxAmount", @class = "money", style = "width:50px", maxlength = 10, onkeyup = "calcOptionAmount(false,"+i+")", onchange = "calcOptionAmount(false,"+i+"); GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());" })%><%//Mod 2022/06/08 yano #4137%><%//Mod 2019/10/17 yano #4022 %>
                            <%--<%=Html.TextBox(namePrefix + "TaxAmount", string.Format("{0:N0}", Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "TaxAmount", @class = "money", style = "width:50px", maxlength = 10, onkeyup = "calcOptionAmount(false,"+i+")", onchange = "GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());" })%><%//Mod 2019/10/17 yano #4022 %>--%>
                        <%}else{ %>
                            <%=Html.TextBox(namePrefix + "TaxAmount", string.Format("{0:N0}", Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "TaxAmount", @class = "readonly money", style = "width:50px", @readonly="readonly" })%>
                        <%} %>
                    </td>
                    <td style="width:60px;white-space:nowrap">
                        <%if(Model.OptionEnabled){ %>
                            <%=Html.TextBox(namePrefix + "AmountWithTax", string.Format("{0:N0}", Model.CarSalesLine[i].Amount + Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "AmountWithTax", @class = "money", style = "width:50px", maxlength = 10, onkeyup = "calcOptionAmount(true,"+i+")", onchange = "calcOptionAmount(true,"+i+"); GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());" })%><%//Mod 2022/06/08 yano #4137%><%//Mod 2019/10/17 yano #4022 %>
                            <%--<%=Html.TextBox(namePrefix + "AmountWithTax", string.Format("{0:N0}", Model.CarSalesLine[i].Amount + Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "AmountWithTax", @class = "money", style = "width:50px", maxlength = 10, onkeyup = "calcOptionAmount(true,"+i+")", onchange = "GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());" })%><%//Mod 2019/10/17 yano #4022 %>--%>
                        <%}else{ %>
                            <%=Html.TextBox(namePrefix + "AmountWithTax", string.Format("{0:N0}", Model.CarSalesLine[i].Amount + Model.CarSalesLine[i].TaxAmount), new { id=idPrefix + "AmountWithTax", @class = "readonly money", style = "width:50px", @readonly="readonly" })%>
                        <%} %>
                    </td>
                </tr>
        <%} %>
        <%--ookubo.add--%>
        <%=Html.Hidden("LineCount", Model.CarSalesLine.Count)%>
    </table>
</div>
<%--オプション（合計）--%>
<table class="input-form-line">
    <tr>
        <th style="width:20px;padding:0px;white-space:nowrap"></th>
        <th style="width:110px;padding:0px;white-space:nowrap"></th><%//Mod 2021/06/09 yano #4091 80px->110px %>
        <th style="width:140px;padding:0px;white-space:nowrap"></th><%//Mod 2021/06/09 yano #4091 150px->140px %>
        <th style="width:170px;padding:0px;white-space:nowrap">合計</th><%//Mod 2021/06/09 yano #4091 190px->180px %>
        <th style="width:60px;padding:0px;white-space:nowrap"><%=Html.TextBox("OptionTotalAmount", string.Format("{0:N0}", Model.CarSalesLine.Sum(x => x.Amount)), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%></th>
        <th style="width:60px;padding:0px;white-space:nowrap"><%=Html.TextBox("OptionTotalTaxAmount", string.Format("{0:N0}", Model.CarSalesLine.Sum(x => x.TaxAmount)), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%></th>
        <th style="width:60px;padding:0px;white-space:nowrap"><%=Html.TextBox("OptionTotalAmountWithTax", string.Format("{0:N0}", Model.CarSalesLine.Sum(x => x.Amount + x.TaxAmount)), new { @class = "readonly money", style = "width:50px", @readonly = "readonly" })%></th>
    </tr>
</table>                        
