<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SupplierPayment>" %>
<%@ Import Namespace="CrmsDao" %>
<table class="input-form" style="width: 700px">
    <tr>
        <th colspan="4" class="input-form-title">
            支払先情報
        </th>
    </tr>
    <tr>
        <th style="width: 150px">
        </th>
        <td colspan="3">
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <%=Html.CheckBox("SupplierPaymentEnabled", CommonUtils.DefaultString(Model.DelFlag).Equals("0"), new { onclick = "document.forms[0].action='/Customer/SupplierPaymentEnabled';formSubmit()" })%>支払先として登録する   
        </td>
    </tr>
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if (CommonUtils.DefaultString(Model.DelFlag).Equals("0"))
      { %>
    <tr>
        <th>
            支払先名 *
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.SupplierPaymentName", Model.SupplierPaymentName, new { size = 40, maxlength = 20 })%>
        </td>
    </tr>
    <tr>
        <th>
            支払先種別 *
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.SupplierPaymentType", (IEnumerable<SelectListItem>)ViewData["SupplierPaymentTypeList"])%>
        </td>
    </tr>
    <tr>
        <th>
            支払区分 *
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.PaymentType", (IEnumerable<SelectListItem>)ViewData["PaymentType2List"], new { onchange = "changePaymentType2();" })%>
            <%if (Model.PaymentType!=null && Model.PaymentType.Equals("003")) {%>
            <%=Html.TextBox("pay.PaymentDayCount", Model.PaymentDayCount, new { @class = "numeric", maxlength = 3, size = 5, onfocus = "focusPaymentDayCount2()" })%>
            <%} else {%>
            <%=Html.TextBox("pay.PaymentDayCount", Model.PaymentDayCount, new { @class = "numeric", maxlength = 3, size = 5, @readonly = true, style = "background-color:#e0e0e0", onfocus = "focusPaymentDayCount2()" })%>
            <%} %>
            日後(5～240)
        </td>
    </tr>
    <tr>
        <th>
            支払日
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.PaymentDay", Model.PaymentDay, new { @class = "numeric", maxlength = 6, size = 5 })%>※月末払いの場合は0を入力して下さい
        </td>
    </tr>
    <tr>
        <th>
            猶予日数1 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod1", Model.PaymentPeriod1, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th style="width: 150px">
            発生金利1 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate1", Model.PaymentRate1, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数2 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod2", Model.PaymentPeriod2, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th>
            発生金利2 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate2", Model.PaymentRate2, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数3 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod3", Model.PaymentPeriod3, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th>
            発生金利3 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate3", Model.PaymentRate3, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数4 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod4", Model.PaymentPeriod4, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th>
            発生金利4 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate4", Model.PaymentRate4, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数5 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod5", Model.PaymentPeriod5, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th>
            発生金利5 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate5", Model.PaymentRate5, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数6 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod6", Model.PaymentPeriod6, new { @class = "numeric", maxlength = 3, size = 5 })%>
        </td>
        <th>
            発生金利6 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate6", Model.PaymentRate6, new { @class = "numeric", maxlength = 6, size = 5 })%>
        </td>
    </tr>
    <tr>
        <th>
            銀行
        </th>
        <td colspan="3">
            <% //Mod 2014/07/15 arc yano chrome対応 スクリプトのパラメータをname→idに修正 %>
            <%=Html.TextBox("pay.BankCode", Model.BankCode, new { id ="pay_BankCode" ,@class = "alphanumeric", size = 5, onblur = "GetNameFromCode('pay_BankCode','pay_BankName','Bank')" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "pay_BankCode", "pay_BankName", "'/Bank/CriteriaDialog'", "0" }); %>
            <span id="pay_BankName"><%=Html.Encode(ViewData["BankName"]) %></span>
        </td>
    </tr>
    <tr>
        <th>
            支店
        </th>
        <td colspan="3">
            <% //Mod 2014/07/15 arc yano chrome対応 スクリプトのパラメータをname→idに修正 %>
            <%=Html.TextBox("pay.BranchCode", Model.BranchCode, new { id ="pay_BranchCode" ,@class = "alphanumeric", size = 5, onblur = "GetBranchNameFromBankCode()" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "pay_BranchCode", "pay_BranchName", "'/Branch/CriteriaDialog/' + document.getElementById('pay_BankCode').value", "0" }); %>
            <span id="pay_BranchName"><%=Html.Encode(ViewData["BranchName"]) %></span>
        </td>
    </tr>
    <tr>
        <th>
            口座種別            
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.DepositKind",(IEnumerable<SelectListItem>)ViewData["DepositKindList"]) %>
        </td>
    </tr>
    <tr>
        <th>
            口座番号
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.AccountNumber", Model.AccountNumber, new { @class = "alphanumeric", maxlength = 7 }) %>
        </td>
    </tr>
    <tr>
        <th>
            口座名義（かな）
        </th>
        <td>
            <%=Html.TextBox("pay.AccountHolderKana", Model.AccountHolderKana, new { maxlength = 100 }) %>
        </td>
    </tr>
    <tr>
        <th>
            口座名義
        </th>
        <td>
            <%=Html.TextBox("pay.AccountHolder", Model.AccountHolder, new { maxlength = 50 }) %>
        </td>
    </tr>
    <%}else{ %>
    <tr>
        <th>
            支払先名 *
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.SupplierPaymentName", Model.SupplierPaymentName, new { size = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.SupplierPaymentName", Model.SupplierPaymentName)%>
        </td>
    </tr>
    <tr>
        <th>
            支払先種別 *
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.SupplierPaymentType", (IEnumerable<SelectListItem>)ViewData["SupplierPaymentTypeList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("pay.SupplierPaymentType", Model.SupplierPaymentType)%>
        </td>
    </tr>
    <tr>
        <th>
            支払区分 *
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.PaymentType", (IEnumerable<SelectListItem>)ViewData["PaymentType2List"], new { @disabled = "disabled" })%>
            <%=Html.TextBox("pay.PaymentDayCount", Model.PaymentDayCount, new { size = 5, @disabled = "disabled" })%>
            日後(5～240)
            <%=Html.Hidden("pay.PaymentType",Model.PaymentType) %>
            <%=Html.Hidden("pay.PaymentDayCount", Model.PaymentDayCount)%>
        </td>
    </tr>
    <tr>
        <th>
            支払日
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.PaymentDay", Model.PaymentDay, new { size = 5, @disabled = "disabled" })%>※月末払いの場合は0を入力して下さい
            <%=Html.Hidden("pay.PaymentDay", Model.PaymentDay)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数1 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod1", Model.PaymentPeriod1, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod1", Model.PaymentPeriod1)%>
        </td>
        <th style="width: 150px">
            発生金利1 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate1", Model.PaymentRate1, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate1",Model.PaymentRate1)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数2 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod2", Model.PaymentPeriod2, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod2", Model.PaymentPeriod2)%>
        </td>
        <th>
            発生金利2 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate2", Model.PaymentRate2, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate2", Model.PaymentRate2)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数3 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod3", Model.PaymentPeriod3, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod3", Model.PaymentPeriod3)%>
        </td>
        <th>
            発生金利3 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate3", Model.PaymentRate3, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate3", Model.PaymentRate3)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数4 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod4", Model.PaymentPeriod4, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod4", Model.PaymentPeriod4)%>
        </td>
        <th>
            発生金利4 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate4", Model.PaymentRate4, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate4", Model.PaymentRate4)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数5 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod5", Model.PaymentPeriod5, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod5", Model.PaymentPeriod5)%>
        </td>
        <th>
            発生金利5 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate5", Model.PaymentRate5, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate5", Model.PaymentRate5)%>
        </td>
    </tr>
    <tr>
        <th>
            猶予日数6 (日以上)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentPeriod6", Model.PaymentPeriod6, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentPeriod6", Model.PaymentPeriod6)%>
        </td>
        <th>
            発生金利6 (%)
        </th>
        <td>
            <%=Html.TextBox("pay.PaymentRate6", Model.PaymentRate6, new { size = 5, @disabled = "disabled" })%>
            <%=Html.Hidden("pay.PaymentRate6", Model.PaymentRate6)%>
        </td>
    </tr>
    <tr>
        <th>
            銀行コード
        </th>
        <td colspan="3">
            <% // Mod 2014/07/15 arc yano chrome対応 スクリプトのパラメータをnameからidに変更%>
            <%=Html.TextBox("pay.BankCode", Model.BankCode, new { id = "pay_BankCode" , @class = "alphanumeric", size = 5, @disabled="disabled" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "pay_BankCode", "pay_BankName", "'/Bank/CriteriaDialog'", "1" }); %>
            <span id="pay_BankName"><%=Html.Encode(ViewData["BankName"]) %></span>
        </td>
    </tr>
    <tr>
        <th>
            支店コード
        </th>
        <td colspan="3">
             <% // Mod 2014/07/15 arc yano chrome対応 スクリプトのパラメータをnameからidに変更%>
            <%=Html.TextBox("pay.BranchCode", Model.BranchCode, new {id = "pay_BankName" , @class = "alphanumeric", size = 5, @disabled = "disabled" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "pay_BranchCode", "pay_BranchName", "'/Branch/CriteriaDialog/' + document.getElementById('pay_BankCode').value", "1" }); %>
            <span id="pay_BranchName"><%=Html.Encode(ViewData["BranchName"]) %></span>
        </td>
    </tr>
    <tr>
        <th>
            口座種別            
        </th>
        <td colspan="3">
            <%=Html.DropDownList("pay.DepositKind", (IEnumerable<SelectListItem>)ViewData["DepositKindList"], new { @disabled = "disabled" }) %>
            <%=Html.Hidden("pay.DepositKind", Model.DepositKind) %>
        </td>
    </tr>
    <tr>
        <th>
            口座番号
        </th>
        <td colspan="3">
            <%=Html.TextBox("pay.AccountNumber", Model.AccountNumber, new { @class = "alphanumeric readonly", maxlength = 7,@readonly = "readonly" }) %>
        </td>
    </tr>
    <tr>
        <th>
            口座名義（かな）
        </th>
        <td>
            <%=Html.TextBox("pay.AccountHolderKana", Model.AccountHolderKana, new {@class="alphanumeric", maxlength = 100 ,@readonly = "readonly"}) %>
        </td>
    </tr>
    <tr>
        <th>
            口座名義
        </th>
        <td>
            <%=Html.TextBox("pay.AccountHolder", Model.AccountHolder, new {@class="alphanumeric", maxlength = 50,@readonly="readonly" }) %>
        </td>
    </tr>
     <%} %>
</table>
<%=Html.Hidden("pay.SupplierPaymentCode", Model.SupplierPaymentCode)%>
<%=Html.Hidden("pay.DelFlag", Model.DelFlag)%>