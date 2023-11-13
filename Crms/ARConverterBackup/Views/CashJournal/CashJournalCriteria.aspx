<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.Journal>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	現金出納帳検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CashJournal", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("DefaultCondYearMonth", ViewData["DefaultCondYearMonth"]) %>
<%=Html.Hidden("DefaultCondDay", ViewData["DefaultCondDay"]) %>
<%=Html.Hidden("PreviousClosedDate", ViewData["PreviousClosedDate"]) %>
<%=Html.Hidden("Accounting", ViewData["Accounting"]) %>
<%=Html.ValidationSummary()%>
<%string securityLevelCode = "";
  try { securityLevelCode = ((Employee)Session["Employee"]).SecurityRole.SecurityLevelCode; } catch (NullReferenceException) { }%>
<fieldset style="margin-left:3px;padding:10px;width:700px">
<legend>現金締め処理情報</legend>
<br />
<table class="input-form">
    <tr>
        <th>日付</th>
        <!--Mod 2015/05/17 arc nakayama #3552_変更ボタン非表示-->
        <td><%=Html.DropDownList("selectedDate", (IEnumerable<SelectListItem>)ViewData["MovableDateList"], new { onchange = "displaySearchList()" })%></td>
        <th>口座</th>
        <!--Mod 2015/05/17 arc nakayama #3552_変更ボタン非表示-->
        <td><%=Html.DropDownList("CashAccountCode",(IEnumerable<SelectListItem>)ViewData["CashBalanceAccountList"], new { onchange = "displaySearchList()" }) %></td>
        <th>入金事業所 *</th>
        <td style="width:250px">
            <!--Mod 2015/05/17 arc nakayama #3552_変更ボタン非表示-->
            <%switch (securityLevelCode) {
              case "002":%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @readonly = "readonly", size = 1, style = "readonly", onchange = "displaySearchList()"})%>
                           <%break; %>
            <%case "003":
              case "004":%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", size = 1, maxlength = 3, onblur = "GetNameFromCode('OfficeCode','OfficeName','Office','false',displaySearchList)"}) %>
                           <img alt="事業所検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('OfficeCode', 'OfficeName', '/Office/CriteriaDialog')" />
                           <%break; %>
            <%default:%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @readonly = "readonly", size=1, onfocus = "document.forms[0].CondJournalType.focus();"})%>
                           <%break; %>
            <%} %><span id="OfficeName"><%=Html.Encode(ViewData["OfficeName"]) %></span></td>
        <!--Mod 2015/05/17 arc nakayama #3552_変更ボタン非表示-->
        <!--<td><input type="button" value="変更" onclick="displaySearchList()" /></td>-->
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">前月繰越残高</th>
        <th style="width:100px">前回締め日</th>
        <th style="width:100px">入出金額合計</th>
        <th style="width:125px">現金在高(理論値)</th>
        <th colspan="2">現金在高(入力値)</th>
    </tr>
    <tr>
        <td><%if (ViewData["LastMonthBalance"] != null) { %><div style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", ViewData["LastMonthBalance"]))%></div><%}else{ %><div style="text-align:center;color:Red"><%=ViewData["LastMonth"] %>月末未処理</div><%} %></td>
        <td style="text-align:center"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", ViewData["PreviousClosedDate"]))%></td>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", ViewData["DetailsTotal"]))%></td>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", ViewData["LogicalBalance"]))%></td>
        <%if (ViewData["InputBalance"] == null) { %>
            <td style="width:125px;text-align:center">未入力</td>
        <%} else { %>
            <td style="width:125px;text-align:right"><%=Html.Encode(string.Format("{0:N0}", ViewData["InputBalance"]))%></td>
        <%} %>
        <td>
        <%if (!CommonUtils.DefaultString(ViewData["UpdateAuth"]).Equals("1") || CommonUtils.DefaultString(ViewData["AlreadyClosed"]).Equals("1")) { %>
            <input type="button" value="入力" onclick="openModalAfterRefresh2('/CashJournal/Entry/<%=ViewData["OfficeCode"]%>')" disabled="disabled" />
        <%} else { %>
            <input type="button" value="入力" onclick="openModalAfterRefresh2('/CashJournal/Entry/<%=ViewData["OfficeCode"]%>?targetDate='+document.getElementById('selectedDate').value + '&account='+document.getElementById('CashAccountCode').value)" />
        <%} %>
        </td>
    </tr>
    <tr>
        <td style="text-align:center" colspan="6">
        <%if ((!CommonUtils.DefaultString(ViewData["UpdateAuth"]).Equals("1"))
              || (CommonUtils.DefaultString(ViewData["AlreadyClosed"]).Equals("1"))
              || (!CommonUtils.DefaultString(ViewData["LogicalBalance"]).Equals(CommonUtils.DefaultString(ViewData["InputBalance"])))) { %>
            <input type="button" value="店舗締め処理" onclick="document.forms[0].action.value='closeAccount';displaySearchList();" disabled="disabled" />
        <%} else { %>
            <input type="button" value="店舗締め処理" onclick="document.forms[0].action.value='closeAccount';displaySearchList();" />
        <%} %>

        <%if (ViewData["Accounting"].Equals(true))
          {
              string OfficeName = ViewData["OfficeName"].ToString();
              string PreviousClosedDate = string.Format("{0:yyyy/MM/dd}", ViewData["PreviousClosedDate"]);
              %>
            
            <%if ((ViewData["CloseStatus"]).Equals(true))
              { %>
                <input type="button" value="店舗締め解除処理" onclick="CheckReleaseAccount('<%=OfficeName%>', '<%=PreviousClosedDate%>');" />
            <%} else { %>
                <input type="button" value="店舗締め解除処理" disabled="disabled" />
            <%} %>
        <% }%>
        </td>
    </tr>
</table>
</fieldset>
<fieldset style="margin-left:3px;padding:10px;width:750px">
<legend>現金出納帳入力</legend>
<br />
<table class="input-form">
    <tr>
        <th>伝票日付 *</th>
        <th colspan="3">計上部門 *</th>
    </tr>
    <tr>
        <td><%=Html.TextBox("JournalDate", string.Format("{0:yyyy/MM/dd}", ViewData["JournalDate"]), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new {@class="alphanumeric", size = 1,onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?OfficeCode='+document.getElementById('OfficeCode').value)" /></td>
        <td style="width:200px;white-space:nowrap" colspan="2"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>
    </tr>
    <tr>
        <th>入出金区分 *</th>
        <th colspan="2">科目 *</th>
        <th>摘要</th>
    </tr>
    <tr>
        <!--Add 2015/05/17 arc nakayama #3552_入出金区分を待避する隠し項目の追加-->
        <td><%=Html.DropDownList("JournalType", (IEnumerable<SelectListItem>)ViewData["JournalTypeList"])%><%=Html.Hidden("JournalType", (IEnumerable<SelectListItem>)ViewData["JournalTypeList"], new { id = "HdJournalType"})%></td>
        <td><%=Html.TextBox("AccountCode", ViewData["AccountCode"], new { @class = "alphanumeric", size = 10, maxlength = 50, onblur = "GetNameFromCode('AccountCode','AccountName','Account')" })%>
            <img alt="科目検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('AccountCode', 'AccountName', '/Account/CriteriaDialog?journalType='+document.forms[0].JournalType.value)" />
        </td>
        <td style="width:150px;white-space:nowrap"><span id="AccountName"><%=Html.Encode(ViewData["AccountName"])%></span></td>
        <td><%=Html.TextBox("Summary", ViewData["Summary"], new { size=20, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>伝票番号</th>
        <th>請求先</th>
        <th>請求残高</th>
        <th>金額 *</th>
    </tr>
    <tr>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", size = 10, maxlength = 30, onchange = "GetCustomerClaimList()" })%></td>
        <td><%=Html.DropDownList("CustomerClaimCode", (IEnumerable<SelectListItem>)ViewData["CustomerClaimList"], new { onchange = "GetCustomerReceiptBalance()" })%></td>
        <td style="width:150px;text-align:right"><span id="ReceiptBalance"></span></td>
        <td><%=Html.TextBox("Amount", ViewData["Amount"], new { @class = "numeric", size = 11, maxlength = 11 })%></td>
    </tr>
</table>
<br />
<%if (CommonUtils.DefaultString(ViewData["UpdateAuth"]).Equals("1")) { %>
    <input type="button" value="登録" onclick="document.forms[0].action.value='registDetail';displaySearchList();" />
<%} else { %>
    <input type="button" value="登録" onclick="document.forms[0].action.value='registDetail';displaySearchList();" disabled="disabled" />
<%} %>
</fieldset>
<br />
<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:none">
<br />
<table class="input-form">
    <tr>
        <th style="width:300px">計上部門</th>
        <th>入出金区分</th>
        <th>伝票番号</th>
        <th>年月度</th>
        <th>日</th>
    </tr>
    <tr>
        <td><%=Html.TextBox("CondDepartmentCode", ViewData["CondDepartmentCode"], new { @class = "alphanumeric",size=1, maxlength = 3, onblur = "GetNameFromCode('CondDepartmentCode','CondDepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondDepartmentCode', 'CondDepartmentName', '/Department/CriteriaDialog')" />
        <span id="CondDepartmentName"><%=Html.Encode(ViewData["CondDepartmentName"])%></span></td>
        <td><%=Html.DropDownList("CondJournalType", (IEnumerable<SelectListItem>)ViewData["CondJournalTypeList"])%></td>
        <td><%=Html.TextBox("CondSlipNumber",ViewData["CondSlipNumber"],new {@class="alphanumeric",size=10,maxlength=50}) %></td>
        <td><%=Html.DropDownList("CondYearMonth", (IEnumerable<SelectListItem>)ViewData["CondYearMonthList"])%></td>
        <td><%=Html.DropDownList("CondDay", (IEnumerable<SelectListItem>)ViewData["CondDayList"]) %></td>
    </tr>
    <tr>
        <th>勘定科目</th>
        <th colspan="2">摘要</th>
        <th colspan="2"></th>
    </tr>
    <tr>
        <td><%=Html.TextBox("CondAccountCode", ViewData["CondAccountCode"], new { @class = "alphanumeric", size = 10, maxlength = 50, onblur = "GetNameFromCode('CondAccountCode','CondAccountName','Account')" }) %>&nbsp;<img alt="勘定科目検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondAccountCode','CondAccountName','/Account/CriteriaDialog')" />
        <span id="CondAccountName"><%=Html.Encode(ViewData["CondAccountName"]) %></span></td>
        <td colspan="2"><%=Html.TextBox("CondSummary", ViewData["CondSummary"], new { size = "30", maxlength = "50" }) %></td>
        <td colspan="2">
            <input type="button" value="リスト絞込" onclick="displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('CondDepartmentCode','CondDepartmentName','CondJournalType','CondSlipNumber','CondYearMonth','CondDay','CondAccountCode','CondAccountName','CondSummary'))" />
        </td>
    </tr>
</table>
</div>
<br />
<br />
<%if(ViewData["CondDayFrom"]!=null && ViewData["COndDayFrom"].Equals("01")){ %>
    <input type="button" value="1日-10日" disabled="disabled" />&nbsp;
<%}else{ %>
    <input type="button" value="1日-10日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='01';document.getElementById('CondDayTo').value='10';displaySearchList()" />&nbsp;
<%} %>
<%if(ViewData["CondDayFrom"]!=null && ViewData["CondDayFrom"].Equals("11")){ %>
    <input type="button" value="11日-20日" disabled="disabled" />&nbsp;
<%}else{ %>
    <input type="button" value="11日-20日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='11';document.getElementById('CondDayTo').value='20';displaySearchList()" />&nbsp;
<%} %>
<%if(ViewData["CondDayFrom"]!=null && ViewData["CondDayFrom"].Equals("21")){ %>
    <input type="button" value="21日-末日" disabled="disabled" />
<%}else{ %>
    <input type="button" value="21日-末日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='21';document.getElementById('CondDayTo').value='31';displaySearchList()" />
<%} %>
<%=Html.Hidden("CondDayFrom",ViewData["CondDayFrom"]) %>
<%=Html.Hidden("CondDayTo",ViewData["CondDayTo"]) %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="white-space:nowrap">伝票日付</th>
        <th style="white-space:nowrap">計上部門</th>
        <th style="white-space:nowrap">口座</th>
        <th style="white-space:nowrap">科目</th>
        <th style="text-align:right;white-space:nowrap">入金額</th>
        <th style="text-align:right;white-space:nowrap">出金額</th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">請求先</th>
        <th style="white-space:nowrap">消込</th>
        <th style="white-space:nowrap">摘要</th>
    </tr>
    <%foreach (var journal in Model)
      { 
        string journalUsageType="";
        try { journalUsageType = journal.Account.UsageType; } catch (NullReferenceException) { }
    %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh2('/CashJournal/Edit/<%=journal.JournalId %>')">編集</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", journal.JournalDate))%></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.Department!=null ? journal.Department.DepartmentName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.CashAccount!=null ? journal.CashAccount.CashAccountName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.AccountCode)%>&nbsp;<%if (journal.Account != null) {%><%=Html.Encode(journal.Account.AccountName)%><%} %></td>
        <td style="text-align:right;white-space:nowrap"><%if (CommonUtils.DefaultString(journal.JournalType).Equals("001")) {%><%=Html.Encode(string.Format("{0:N0}", journal.Amount))%><%} %></td>
        <td style="text-align:right;white-space:nowrap"><%if (CommonUtils.DefaultString(journal.JournalType).Equals("002")) {%><%=Html.Encode(string.Format("{0:N0}", journal.Amount))%><%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.SlipNumber)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.CustomerClaim!=null ? journal.CustomerClaim.CustomerClaimName : "") %></td>
        <td style="white-space:nowrap"><%=journal.ReceiptPlanFlag!=null && journal.ReceiptPlanFlag.Equals("1") ? "済" : "" %></td>
        <td style="white-space:nowrap"><%=Html.Encode(journal.Summary)%></td>
    </tr>
    <%} %>
    <tr>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
        <th style="text-align:right">合計</th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th style="text-align:right;font-weight:bold;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",Model.Where(x=>CommonUtils.DefaultString(x.JournalType).Equals("001")).Sum(x=>x.Amount))) %></th>
        <th style="text-align:right;font-weight:bold;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",Model.Where(x=>CommonUtils.DefaultString(x.JournalType).Equals("002")).Sum(x=>x.Amount))) %></th>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
    </tr>
</table>
<br />
<%} %>
</asp:Content>
