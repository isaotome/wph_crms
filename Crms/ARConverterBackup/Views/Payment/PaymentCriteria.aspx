<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PaymentPlan>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
支払検索</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","Payment",FormMethod.Post)){%>
<%=Html.Hidden("id","0") %>
<%=Html.Hidden("action","") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:150px" rowspan="2">支払先</th>
        <td><%=Html.TextBox("CondSupplierPaymentCode", ViewData["CondSupplierPaymentCode"], new { @class = "alphanumeric", size = "15", maxlength = "10", onblur = "GetNameFromCode('CondSupplierPaymentCode','CondSupplierPaymentName','SupplierPayment')" })%>&nbsp;<img alt="支払先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondSupplierPaymentCode','CondSupplierPaymentName','/SupplierPayment/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="CondSupplierPaymentName"><%=CommonUtils.DefaultNbsp(ViewData["CondSupplierPaymentName"]) %></span></td>
    </tr>
    <tr>
        <th style="width:150px">支払先種別</th>
        <td><%=Html.DropDownList("CondSupplierPaymentType",(IEnumerable<SelectListItem>)ViewData["CondSupplierPaymentTypeList"]) %></td>
    </tr>
    <tr>
        <th style="width:150px">支払予定日</th>
        <td><%=Html.TextBox("PaymentPlanDateFrom",ViewData["PaymentPlanDateFrom"],new {@class="alphanumeric",size="10",maxlength="10"}) %>～<%=Html.TextBox("PaymentPlanDateTo",ViewData["PaymentPlanDateTo"],new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
    </tr>
    <tr>
        <th></th>
        <td>
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('CondSupplierPaymentCode','CondSupplierPaymentName','CondSupplierPaymentType','PaymentPlanDateFrom','PaymentPlanDateTo'))" />
        </td>
    </tr>
</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<%=Html.ValidationSummary()%>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th>支払日 *</th>
        <th>支払種別 *</th>
        <th>支払額 *</th>
        <th>支払予定日</th>
        <th>発生部門</th>
        <th>伝票番号</th>
        <th>支払先</th>
        <th>科目名</th>
        <th>買掛金残高</th>
    </tr>
    <%for(int i =0;i< Model.Count;i++) {
          string namePrefix = string.Format("line[{0}].", i);
          string idPrefix = string.Format("line[{0}]_", i);
          Journal journal = ((List<Journal>)ViewData["JournalList"])[i];
    %>
    <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールのidを追加 %>
    <%=Html.Hidden(namePrefix + "PaymentPlanId", Model[i].PaymentPlanId.ToString(), new { id = idPrefix + "PaymentPlanId"})%>
    <tr>
        <td><%=Html.TextBox(namePrefix + "JournalDate", string.Format("{0:yyyy/MM/dd}", journal.JournalDate), new { id = idPrefix + "JournalDate", @class = "alphanumeric", size = "10", maxlength = "10" })%></td>
        <td><%=Html.DropDownList(namePrefix + "AccountType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["AccountTypeList"])[i], new { id = idPrefix + "AccountType"})%></td>
        <td>
<%=Html.TextBox(namePrefix + "Amount", journal.Amount, new { id = idPrefix + "Amount", @class = "numeric", size = 10, maxlength = 10 })%>&nbsp;<input type="button" value="全額" style="width:35px" onclick="document.getElementById('<%=idPrefix %>Amount').value = '<%=Model[i].PaymentableBalance ?? 0 %>'" /></td>
        <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model[i].PaymentPlanDate)) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model[i].OccurredDepartment!=null ? Model[i].OccurredDepartment.DepartmentName : "")%></td>
        <td><%=CommonUtils.DefaultNbsp(Model[i].SlipNumber) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model[i].SupplierPayment.SupplierPaymentName) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model[i].Account!=null ? Model[i].Account.AccountName : "") %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",Model[i].PaymentableBalance ?? 0)) %></td>
   </tr>
   <%} %>
    <tr>
        <th>&nbsp;</th>
        <th>&nbsp;</th>
        <th>&nbsp;</th>
        <th>&nbsp;</th>
        <th>&nbsp;</th>
        <th></th>
        <th></th>
        <th><div style="text-align:right;font-weight:bold">残高合計</div></th>
        <th><div style="text-align:right;font-weight:bold"><%=Html.Encode(string.Format("{0:N0}", ViewData["TotalBalance"]))%></div></th>
   </tr>
</table>
<br />
<br />
<%if (Model.Count > 0) { %>
<input type="button" value="支払消込登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />
<%} else { %>
<input type="button" value="支払消込登録" disabled="disabled" />
<%} %>
<%} %>
</asp:Content>
