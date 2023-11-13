<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.Journal>>" %>
<%@ Import Namespace="CrmsDao"  %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金実績リスト
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%string securityLevelCode = "";
  try { securityLevelCode = ((Employee)Session["Employee"]).SecurityRole.SecurityLevelCode; } catch (NullReferenceException) { }%>

<%using (Html.BeginForm("JournalCriteria", "CashJournal", FormMethod.Post)) { %>
<%=Html.Hidden("DefaultCondYearMonth", ViewData["DefaultCondYearMonth"]) %>
<%=Html.Hidden("DefaultCondDay", ViewData["DefaultCondDay"]) %>
<%=Html.Hidden("DefaultOfficeCode", ViewData["DefaultOfficeCode"]) %>
<%=Html.Hidden("DefaultOfficeName", ViewData["DefaultOfficeName"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:300px">事業所</th>
        <th colspan="2">口座</th>
        <th>年月度/日</th>
    </tr>
    <tr>
        <td>
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <%if (CommonUtils.DefaultString(securityLevelCode).Equals("003") || CommonUtils.DefaultString(securityLevelCode).Equals("004"))
              { %>
                <%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @clas = "alphanumeric", size = 1, maxlength = 3, onblur = "GetCashAccountCode(new Array('OfficeCode', 'OfficeName', 'CashAccountCode'))" })%><%//Mod 2019/02/14 yano #3972%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "OfficeCode", "OfficeName", "'/Office/CriteriaDialog'", "0", "GetCashAccountCode(new Array('OfficeCode', 'OfficeName', 'CashAccountCode'))" }); %><%//Mod 2019/02/14 yano #3972%>
            <%}else{ %>
                <%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric readonly", size = 1, maxlength = 3, @readonly = "readonly" }) %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "OfficeCode", "OfficeName", "'/Office/CriteriaDialog'", "1" }); %>
            <%} %>
            <span id="OfficeName">
                <%=Html.Encode(ViewData["OfficeName"]) %></span>
        </td>
        <td colspan="2">
            <%=Html.DropDownList("CashAccountCode",(IEnumerable<SelectListItem>)ViewData["CashAccountList"]) %>
        </td>
        <td>
            <%=Html.DropDownList("CondYearMonth", (IEnumerable<SelectListItem>)ViewData["CondYearMonthList"])%> / 
            <%=Html.DropDownList("CondDay", (IEnumerable<SelectListItem>)ViewData["CondDayList"])%>
       </td>
    </tr>
    <tr>
        <th>計上部門</th>
        <th>入金種別</th>
        <th>伝票番号</th>
        <th></th>
    </tr>
    <tr>
        <td>
            <%=Html.TextBox("CondDepartmentCode", ViewData["CondDepartmentCode"], new { @class = "alphanumeric", size = 1, maxlength = 3, onblur = "GetNameFromCode('CondDepartmentCode','CondDepartmentName','Department')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "CondDepartmentCode", "CondDepartmentName", "'/Department/CriteriaDialog?OfficeCode='+document.getElementById('OfficeCode').value", "0" }); %>
            <span id="CondDepartmentName">
                <%=Html.Encode(ViewData["CondDepartmentName"])%></span>
        </td>
        <td>
            <%=Html.DropDownList("AccountType", (IEnumerable<SelectListItem>)ViewData["AccountTypeList"]) %>
        </td>
        <td>
            <%=Html.TextBox("CondSlipNumber", ViewData["CondSlipNumber"], new { @class = "alphanumeric", size = 10, maxlength = 50 })%>
        </td>
         <td>
        </td>
   
    </tr>
    <tr>
        <th>勘定科目</th>
        <th colspan="2">摘要</th>
        <th></th>
    </tr>
    <tr>
        <td><%=Html.TextBox("CondAccountCode", ViewData["CondAccountCode"], new { @class = "alphanumeric", size = 10, maxlength = 50, onblur = "GetNameFromCode('CondAccountCode','CondAccountName','Account')" }) %>&nbsp;<img alt="勘定科目検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondAccountCode','CondAccountName','/Account/CriteriaDialog')" />
        <span id="CondAccountName"><%=Html.Encode(ViewData["CondAccountName"]) %></span></td>
        <td colspan="2"><%=Html.TextBox("CondSummary", ViewData["CondSummary"], new { size = "30", maxlength = "50" }) %></td>
        <td>
            <input type="button" value="リスト絞込" onclick="displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('OfficeCode','OfficeName','CashAccountCode','CondYearMonth','CondDay','CondDepartmentCode','CondDepartmentName','AccountType','CondSlipNumber','CondAccountCode','CondAccountName','CondSummary'))" />
        </td>
    </tr>
</table>
</div>
<br />
<br />
<%if (ViewData["CondDayFrom"] != null && ViewData["COndDayFrom"].Equals("01")) { %>
    <input type="button" value="1日-10日" disabled="disabled" />&nbsp;
<%} else { %>
    <input type="button" value="1日-10日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='01';document.getElementById('CondDayTo').value='10';displaySearchList()" />&nbsp;
<%} %>
<%if (ViewData["CondDayFrom"] != null && ViewData["CondDayFrom"].Equals("11")) { %>
    <input type="button" value="11日-20日" disabled="disabled" />&nbsp;
<%} else { %>
    <input type="button" value="11日-20日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='11';document.getElementById('CondDayTo').value='20';displaySearchList()" />&nbsp;
<%} %>
<%if (ViewData["CondDayFrom"] != null && ViewData["CondDayFrom"].Equals("21")) { %>
    <input type="button" value="21日-末日" disabled="disabled" />
<%} else { %>
    <input type="button" value="21日-末日" onclick="document.getElementById('CondDay').selectedIndex=0;document.getElementById('CondDayFrom').value='21';document.getElementById('CondDayTo').value='31';displaySearchList()" />
<%} %>
<%=Html.Hidden("CondDayFrom", ViewData["CondDayFrom"])%>
<%=Html.Hidden("CondDayTo", ViewData["CondDayTo"])%><br />
<br />
<table class="list">
    <tr>
        <th style="white-space:nowrap;width:30px"></th>
        <th style="white-space:nowrap">伝票日付</th>
        <th style="white-space:nowrap">計上部門</th>
        <th style="white-space:nowrap">口座</th>
        <th style="white-space:nowrap">入金種別</th>
        <th style="white-space:nowrap">入金額</th>
        <th style="white-space:nowrap">科目</th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">請求先</th>
        <th style="white-space:nowrap">摘要</th>
    </tr>
    <%foreach (var item in Model) { %>
    <tr>
        <!--Mod 2016/05/16 arc nakayama #3460_入金種別が「カード」「ローン」でも編集のリンクを表示させる  表示の条件を削除-->
        <td style="white-space:nowrap">
            <%//if (item.AccountType != null && !item.AccountType.Equals("003") && !item.AccountType.Equals("004")) { %>
         <a href="javascript:void(0)" onclick="openModalAfterRefresh2('/CashJournal/JournalEdit/<%=item.JournalId %>')">編集</a>
            <%//} %>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(string.Format("{0:yyyy/MM/dd}", item.JournalDate))%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.Department != null ? item.Department.DepartmentName : "")%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.CashAccount != null ? item.CashAccount.CashAccountName : "")%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.c_AccountType != null ? item.c_AccountType.Name : "")%>
        </td>
        <td style="white-space: nowrap; text-align: right">
            <%=Html.Encode(string.Format("{0:N0}", item.Amount))%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.Account != null ? item.Account.AccountName : "")%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.SlipNumber)%>
        </td>
        <td style="white-space: nowrap">
            <%=Html.Encode(item.CustomerClaim != null ? item.CustomerClaim.CustomerClaimName : "")%>
        </td>
        <td>
            <%=Html.Encode(item.Summary)%>
        </td>
    </tr>
    <%} %>
</table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
