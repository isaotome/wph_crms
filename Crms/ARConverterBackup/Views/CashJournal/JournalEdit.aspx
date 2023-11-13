﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Journal>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金実績修正
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("JournalEdit", "CashJournal", FormMethod.Post)) { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%if ((!Model.IsClosed) && (ViewData["AccountingFilter"].Equals(false)))
         { %>
       <td onclick="document.forms[0].DelFlag.value='1';formCancel()"><img src="/Content/Images/cancel.png" alt="削除" class="command_btn" />&nbsp;削除</td>
       <%} %>
       <!--Mod 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」「ローン」の場合は保存ボタンを表示せず、削除ボタンのみにする-->
       <%if (ViewData["AccountingFilter"].Equals(false) && (Model.AccountType != null && !Model.AccountType.Equals("003") && !Model.AccountType.Equals("004")))
         { %>
        <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%} %>
   </tr>
</table>
<br />
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("JournalId",Model.JournalId) %>
<%=Html.Hidden("ReceiptPlanFlag",Model.ReceiptPlanFlag) %>
<%=Html.Hidden("OfficeCode",Model.OfficeCode) %>
<%=Html.Hidden("actionType","") %>
<%=Html.Hidden("IsClosed",Model.IsClosed) %>
<%=Html.Hidden("DelFlag",Model.DelFlag) %>
<!--Mod 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない-->
<%=Html.Hidden("AccountingFilter", ViewData["AccountingFilter"]) %>
<%=Html.Hidden("CreditReceiptPlanId",Model.CreditReceiptPlanId) %>
<%=Html.ValidationSummary()%>
<table class="input-form" style="width:100%">
<!--Mod 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」または「ローン」だった場合は、編集不可にする-->
    <tr>
        <th>
            口座
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.DropDownList("CashAccountCode", (IEnumerable<SelectListItem>)ViewData["CashAccountCodeList"], new { @disabled = "disabled" })%>
                <%=Html.Hidden("CashAccountCode", Model.CashAccountCode) %>
            <%}else{ %>
                <%=Html.DropDownList("CashAccountCode", (IEnumerable<SelectListItem>)ViewData["CashAccountCodeList"])%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px" rowspan="2">
            計上部門 *
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric readonly", size = 10, maxlength = "3", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'","1" }); %>
            <%}else{ %>
                <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", size = 10, maxlength = "3", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'","0" }); %>
            <%} %>
        </td>
    </tr>
    <tr>
        <td>
            <span id="DepartmentName">
                <%=CommonUtils.DefaultNbsp(Model.Department != null ? Model.Department.DepartmentName : "")%></span>
        </td>
    </tr>
    <tr>
        <th>
            伝票日付 *
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.TextBox("JournalDate", string.Format("{0:yyyy/MM/dd}", Model.JournalDate), new { @class = "alphanumeric readonly", size = "10", maxlength = "10", @readonly = "readonly" })%>
            <%} else { %>
                <%=Html.TextBox("JournalDate", string.Format("{0:yyyy/MM/dd}", Model.JournalDate), new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th rowspan="2">
            科目 *
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.TextBox("AccountCode", Model.AccountCode, new { @class = "alphanumeric readonly", size = 10, maxlength = 50, @readonly = "readonly" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "AccountCode", "AccountName", "'/Account/CriteriaDialog'", "1" }); %>
            <%}else{ %>
                <%=Html.TextBox("AccountCode", Model.AccountCode, new { @class = "alphanumeric", size = 10, maxlength = 50, onblur = "GetNameFromCode('AccountCode','AccountName','Account')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "AccountCode", "AccountName", "'/Account/CriteriaDialog'", "0" }); %>
            <%} %>
        </td>
    </tr>
    <tr>
        <td>
            <span id="AccountName">
                <%=CommonUtils.DefaultNbsp(Model.Account != null ? Model.Account.AccountName : "")%></span>
        </td>
    </tr>
    <tr>
        <th>
            入出金区分 *
        </th>
        <td>
            <!--Mod 2016/07/08 arc nakayama #3613_【サービス売掛金】　現金出納帳で伝票を番号を入力すると入出金区分を「入金」に変更する処理を解除する   伝票番号の有無ではなく入出金区分が「入金」だった場合　編集不可にする-->
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true) || Model.JournalType.Equals("001"))
              { %>
                <%=Html.DropDownList("JournalType", (IEnumerable<SelectListItem>)ViewData["JournalTypeList"], new { @disabled = "disabled" }) %>
                <%=Html.Hidden("JournalType", Model.JournalType, new { id = "HdJournalType"})%>
            <%}else{ %>
                <%=Html.DropDownList("JournalType", (IEnumerable<SelectListItem>)ViewData["JournalTypeList"])%>
                <%=Html.Hidden("JournalType", Model.JournalType, new { id = "HdJournalType"})%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            入金種別
        </th>
        <td>
            <!--Mod 201605/19 arc nakayama #3544_入金種別をカード・ローンには変更させない-->
            <%//if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004"))
              //{ %>
                <%=Html.DropDownList("AccountType", (IEnumerable<SelectListItem>)ViewData["AccountTypeList"], new { @disabled = "disabled" }) %>
                <%=Html.Hidden("AccountType", Model.AccountType) %>
            <%//}else{ %>
                <%//=Html.DropDownList("AccountType", (IEnumerable<SelectListItem>)ViewData["AccountTypeList"]) %>
            <%//} %>
        </td>
    </tr>
    <tr>
        <th>
            金額 *
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.TextBox("Amount", Model.Amount, new { @class = "numeric readonly", style = "width:50px", maxlength = 11, @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("Amount", Model.Amount, new { @class = "numeric", style = "width:50px", maxlength = 11 })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            受注伝票番号
        </th>
        <td><%//Mod 2018/01/23 arc yano  #3836 入金振替機能移行に伴い、現金出納帳では入金消込保存を行った場合は伝票番号の変更をできなくする%>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true) || (Model.ReceiptPlanFlag != null && Model.ReceiptPlanFlag.Equals("1")))         
              { %>
                <%=Html.TextBox("SlipNumber", Model.SlipNumber, new { @class = "alphanumeric readonly", style = "width:100px", @readonly = "readonly" }) %>
            <%}else{ %>
                <%=Html.TextBox("SlipNumber", Model.SlipNumber, new { @class = "alphanumeric", style = "width:100px", maxlength = 50, onchange = "GetCustomerClaimList()" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            請求先
        </th>
        <td>
            <%if (Model.IsClosed || Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              { %>
                <%=Html.DropDownList("CustomerClaimCode", (IEnumerable<SelectListItem>)ViewData["CustomerClaimList"], new { @disabled = "disabled" })%>
                <%=Html.Hidden("CustomerClaimCode", Model.CustomerClaimCode) %>
            <%}else{ %>
                <%=Html.DropDownList("CustomerClaimCode", (IEnumerable<SelectListItem>)ViewData["CustomerClaimList"]) %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            摘要
        </th>
        <td>
            <%if (Model.AccountType.Equals("003") || Model.AccountType.Equals("004") || ViewData["AccountingFilter"].Equals(true))
              {%>
                <%=Html.TextBox("Summary", Model.Summary, new { size=100, maxlength=50, @class="readonly", @readonly = "readonly" }) %>
            <%}else{ %>
                <%=Html.TextBox("Summary",Model.Summary, new { size=100, maxlength=50 })%>
            <%} %>
        </td>
    </tr>
</table>

<%} %>


</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>