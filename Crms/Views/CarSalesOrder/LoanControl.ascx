<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
// -----------------------------------------------------------------------------------------------------

// Mod 2021/08/06 yano #4088 ローン入金のチェック漏れ
// Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
//                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
// ----------------------------------------------------------------------------------------------------- 
%>

<%//Add 2021/08/06 yano #4088%>
<%if (Model.LoanCompleted)
  {%>
    <span style="color:blue"><b>ローン入金済のため、プランの変更や編集を行うことができません。変更したい場合は、経理課にローン入金の<br>削除を依頼してください。</b></span>
<%} %>
<table class="input-form" style="width: 400px">
    <tr>
        <th class="input-form-title">
            <% // Mod 2014/07/25 arc amii バグ対応 Model.PaymentPlanTypeがnullの場合、ローン情報がシステムエラーになるのを修正 %>
            <%if (Model.LoanEnabled || string.IsNullOrEmpty(Model.PaymentPlanType)) { %>
            <%=Html.RadioButton("PaymentPlanType", "", string.IsNullOrEmpty(Model.PaymentPlanType), new { onclick = "changeDisplayLoan('NoLoan');calcTotalAmount()" })%>
            <%} else { %>
            <%=Html.RadioButton("PaymentPlanType", "", string.IsNullOrEmpty(Model.PaymentPlanType), new { @disabled = "disabled" })%>
            <%} %>
            ローンなし
            <%=CommonUtils.DefaultNbsp(null,5) %>
            <% if (Model.LoanEnabled || "A".Equals(Model.PaymentPlanType)) { %>
            <%=Html.RadioButton("PaymentPlanType", "A", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("A"), new { onclick = "changeDisplayLoan('LoanA');calcTotalAmount()" })%>
            <%} else { %>
            <%=Html.RadioButton("PaymentPlanType", "A", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("A"), new { @disabled = "disabled" })%>
            <%} %>
            プランA
            <%=CommonUtils.DefaultNbsp(null,5) %>
            <%if (Model.LoanEnabled || "B".Equals(Model.PaymentPlanType)) { %>
            <%=Html.RadioButton("PaymentPlanType", "B", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("B"), new { onclick = "changeDisplayLoan('LoanB');calcTotalAmount()" })%>
            <%} else { %>
            <%=Html.RadioButton("PaymentPlanType", "B", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("B"), new { @disabled = "disabled" })%>
            <%} %>
            プランB
            <%=CommonUtils.DefaultNbsp(null,5) %>
            <%if (Model.LoanEnabled || "C".Equals(Model.PaymentPlanType)) { %>
            <%=Html.RadioButton("PaymentPlanType", "C", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("C"), new { onclick = "changeDisplayLoan('LoanC');calcTotalAmount()" })%>
            <%} else { %>
            <%=Html.RadioButton("PaymentPlanType", "C", Model.PaymentPlanType != null && Model.PaymentPlanType.Equals("C"), new { @disabled = "disabled" })%>
            <%} %>
            プランC
        </th>
    </tr>
</table>
<br />
<%  string str;
    for (int n = 1; n <= 3; n++) {
        switch (n) {
            case 1: str = "A";
                break;
            case 2: str = "B";
                break;
            case 3: str = "C";
                break;
            default: str = "A";
                break;
        }%>
<%if (!string.IsNullOrEmpty(Model.PaymentPlanType) && Model.PaymentPlanType.Equals(str)) { %>
    <div id="Loan<%=str %>">
<%} else { %>
    <div id="Loan<%=str %>" style="display: none">
<%}%>
        <table class="input-form-slim" style="width: 400px">
            <tr>
                <th style="width: 100px">
                    ローンコード
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("LoanCode" + str, CommonUtils.GetModelProperty(Model, "LoanCode" + str), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "LoanCode" + str, "LoanName" + str, "'/Loan/CriteriaDialog'", "0" }); %>
                    <%} else { %>
                    <%=Html.TextBox("LoanCode" + str, CommonUtils.GetModelProperty(Model, "LoanCode" + str), new { @class = "readonly alphanumeric", style = "width:100px", maxlength = "10", @readonly = "readonly" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "LoanCode" + str, "LoanName" + str, "'/Loan/CriteriaDialog'", "1" }); %>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ローン名
                </th>
                <td>
                    <%=Html.TextBox("LoanName" + str, ViewData["LoanName" + str], new { @class = "readonly", style = "width:300px", @readonly = "readonly" })%>
                </td>
            </tr>
            <tr>
                <th>
                    初回金額
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("FirstAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "FirstAmount" + str)), new { @class = "money", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("FirstAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "FirstAmount" + str)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ２回以降金額
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("SecondAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "SecondAmount" + str)), new { @class = "money", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("SecondAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "SecondAmount" + str)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ボーナス月金額
                </th>
                <td style="text-align: left">
                    <!--Mod 2015/11/19 arc nakayama #3305_車両伝票での支払方法ローン情報の金額がおかしい-->
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("BonusAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "BonusAmount" + str)), new { @class = "money", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("BonusAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "BonusAmount" + str)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    初回引落日
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("FirstDirectDebitDate" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "FirstDirectDebitDate" + str)), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("FirstDirectDebitDate" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "FirstDirectDebitDate" + str)), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ２回目以降(毎月)
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("SecondDirectDebitDate" + str, CommonUtils.GetModelProperty(Model, "SecondDirectDebitDate" + str), new { @class = "numeric", style = "width:100px", maxlength = "2" })%>
                    <%} else { %>
                    <%=Html.TextBox("SecondDirectDebitDate" + str, CommonUtils.GetModelProperty(Model, "SecondDirectDebitDate" + str), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ボーナス月１
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("BonusMonth" + str + "1", CommonUtils.GetModelProperty(Model, "BonusMonth" + str + "1"), new { @class = "numeric", style = "width:100px", maxlength = "2" })%>
                    <%} else { %>
                    <%=Html.TextBox("BonusMonth" + str + "1", CommonUtils.GetModelProperty(Model, "BonusMonth" + str + "1"), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ボーナス月２
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("BonusMonth" + str + "2", CommonUtils.GetModelProperty(Model, "BonusMonth" + str + "2"), new { @class = "numeric", style = "width:100px", maxlength = "2" })%>
                    <%} else { %>
                    <%=Html.TextBox("BonusMonth" + str + "2", CommonUtils.GetModelProperty(Model, "BonusMonth" + str + "2"), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    支払回数(月数)
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("PaymentFrequency" + str, CommonUtils.GetModelProperty(Model, "PaymentFrequency" + str), new { @class = "numeric", style = "width:100px", maxlength = "3" })%>
                    <%} else { %>
                    <%=Html.TextBox("PaymentFrequency" + str, CommonUtils.GetModelProperty(Model, "PaymentFrequency" + str), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <%//Add 20170/02/02 arc nakayama #3489_車両伝票の自動車注文申込書のローンの２回目以降の回数の表記 %>
            <tr>
                <th>
                    2回目以降の支払回数(月数)
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("PaymentSecondFrequency" + str, CommonUtils.GetModelProperty(Model, "PaymentSecondFrequency" + str), new { @class = "numeric", style = "width:100px", maxlength = "3" })%>
                    <%} else { %>
                    <%=Html.TextBox("PaymentSecondFrequency" + str, CommonUtils.GetModelProperty(Model, "PaymentSecondFrequency" + str), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    支払期間開始
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("PaymentTermFrom" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "PaymentTermFrom" + str)), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("PaymentTermFrom" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "PaymentTermFrom" + str)), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    支払期間終了
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("PaymentTermTo" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "PaymentTermTo" + str)), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
                    <%} else { %>
                    <%=Html.TextBox("PaymentTermTo" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "PaymentTermTo" + str)), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    承認No.
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("AuthorizationNumber" + str, CommonUtils.GetModelProperty(Model, "AuthorizationNumber" + str), new { @class = "alphanumeric", style = "width:300px", maxlength = "20" })%>
                    <%} else { %>
                    <%=Html.TextBox("AuthorizationNumber" + str, CommonUtils.GetModelProperty(Model, "AuthorizationNumber" + str), new { @class = "readonly alphanumeric", style = "width:300px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    残価金額
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("RemainAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "RemainAmount" + str)), new { @class = "money", style = "width:100px", onkeyup = "calcTotalAmount()" })%>
                    <%} else { %>
                    <%=Html.TextBox("RemainAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "RemainAmount" + str)), new { @class = "readonly money", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    残価最終月
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("RemainFinalMonth" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "RemainFinalMonth" + str)), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%><%///Mod 2021/08/23 yano #4104 属性をnumeric→alphanumericへ変更 %>
                    <%} else { %>
                    <%=Html.TextBox("RemainFinalMonth" + str, string.Format("{0:yyyy/MM/dd}", CommonUtils.GetModelProperty(Model, "RemainFinalMonth" + str)), new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%><%///Mod 2021/08/23 yano #4104 属性をnumeric→alphanumericへ変更 %>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ローン元金
                </th>
                <!--Mod 2016/09/08 arc nakayama #3630_【製造】車両売掛金対応 ローン元金を変更できるようにする-->
                <td style="text-align: left">
                    <%=Html.TextBox("LoanPrincipal" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "LoanPrincipal" + str)), new { @class = "readonly money", @readonly = "readonly", style = "width:100px"})%>
                </td>
            </tr>
            <tr>
                <th>
                    ローン金利
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("LoanRate" + str, CommonUtils.GetModelProperty(Model, "LoanRate" + str), new { @class = "numeric", style = "width:100px" })%>
                    <%} else { %>
                    <%=Html.TextBox("LoanRate" + str, CommonUtils.GetModelProperty(Model, "LoanRate" + str), new { @class = "readonly numeric", style = "width:100px", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ローン手数料
                </th>
                <td style="text-align: left">
                    <%if (Model.LoanEnabled) { %>
                    <%=Html.TextBox("LoanFee" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "LoanFee" + str)), new { @class = "money", style = "width:100px", maxlength = "10", onkeyup = "calcTotalAmount()" })%>
                    <%} else { %>
                    <%=Html.TextBox("LoanFee" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model, "LoanFee" + str)), new { @class = "readonly money", style = "width:100px", maxlength = "10", @readonly = "readonly" })%>
                    <%} %>
                </td>
            </tr>
            <tr>
                <th>
                    ローン合計
                </th>
                <td style="text-align: left">
                    <%=Html.TextBox("LoanTotalAmount" + str, string.Format("{0:N0}", CommonUtils.GetModelProperty(Model,"LoanTotalAmount" + str)), new { @class = "readonly money",  style = "width:100px", @readonly = "readonly" })%>
                </td>
            </tr>
        </table>
    </div>

    <%} %>
