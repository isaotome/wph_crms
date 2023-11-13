<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetCreditJournal_Result>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    クレジット入金確認
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CreditJournalCheck", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:EXCEL出力 / それ以外:検索)%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary()%>
<table class="input-form">
    <tr>
        <th>決済日 *</th>
        <td><%=Html.TextBox("JournalDateFrom",ViewData["JournalDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('JournalDateFrom').value, 'JournalDateFrom')" }) %>～<%=Html.TextBox("JournalDateTo", ViewData["JournalDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 ,onchange ="return chkDate3(document.getElementById('JournalDateTo').value, 'JournalDateTo')"}) %></td>
        <th>納車日</th>
        <td><%=Html.TextBox("SalesDateFrom",ViewData["SalesDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('SalesDateFrom').value, 'SalesDateFrom')" }) %>～<%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 ,onchange ="return chkDate3(document.getElementById('SalesDateTo').value, 'SalesDateTo')"}) %></td>
    </tr>
    <tr>
        <th>伝票タイプ</th>
        <td><%=Html.DropDownList("SlipType", (IEnumerable<SelectListItem>)ViewData["SlipTypeList"])%></td>
        <th>伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
    </tr>
    <tr>
        <th>部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th style="width:100px">部門名</th>
        <td > <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", maxlength = 10, onchange = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" /></td><%//Mod 2021/05/29 yano #4045 Chrome対応%>
        <th style="width:100px">顧客名</th>
        <td > <span id="CustomerName"><%=Html.Encode(ViewData["CustomerName"]) %></span></td>
    </tr>
    <tr>
        <th>請求先コード</th>
        <td><%=Html.TextBox("CustomerClaimCode", ViewData["CustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10, onchange = "GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim')" })%>&nbsp;<img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerClaimCode','CustomerClaimName','/CustomerClaim/CriteriaDialog?CustomerClaimType=003')" /></td>
        <th style="width:100px">請求先名</th>
        <td > <span id="CustomerClaimName"><%=Html.Encode(ViewData["CustomerClaimName"]) %></span></td>
    </tr>
    <tr>
        <th>入金状況</th>
        <td><%=Html.DropDownList("CompleteFlag", (IEnumerable<SelectListItem>)ViewData["CompleteFlagList"])%></td>
        <th colspan="2"></th>
    </tr>
    <tr>
        <th></th>
        <td colspan ="6">
            <input type="button" value="検索" onclick="if (CreditRequiredCheck()) { document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { }"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('JournalDateFrom', 'JournalDateTo', 'SalesDateFrom', 'SalesDateTo', 'SlipType', 'SlipNumber', 'DepartmentCode', 'DepartmentName', 'CustomerCode', 'CustomerName', 'CustomerClaimCode', 'CustomerClaimName', 'CompleteFlag'))"/>
            <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CreditJournalCheck', 'UpdateMsg'); document.forms[0].submit();"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<br />
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="input-form">
    <tr>
        <th style="white-space:nowrap">伝票タイプ</th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">伝票ステータス</th>
        <th style="white-space:nowrap">部門コード</th>
        <th style="white-space:nowrap">部門名</th>
        <th style="white-space:nowrap">受注日</th>
        <th style="white-space:nowrap">納車日</th>
        <th style="white-space:nowrap">顧客コード</th>
        <th style="white-space:nowrap">顧客名</th>
        <th style="white-space:nowrap">決済日</th>
        <th style="white-space:nowrap">請求先コード</th>
        <th style="white-space:nowrap">請求先名</th>
        <th style="white-space:nowrap">決済金額</th>
        <th style="white-space:nowrap">入金状況</th>
        <th style="white-space:nowrap">科目コード</th><!--Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加-->
        <th style="white-space:nowrap">科目名</th><!--Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加-->
        <th style="white-space:nowrap">摘要</th>
    </tr>
    <%foreach (var a in Model)
      {%>
    <tr>
        <td><%=Html.Encode(a.SlipTypeName)%></td>                                           <%//伝票タイプ%>
        <td><%=Html.Encode(a.SlipNumber)%></td>                                             <%//伝票番号%>
        <td><%=Html.Encode(a.StatusName)%></td>                                             <%//伝票ステータス%>
        <td><%=Html.Encode(a.OccurredDepartmentCode)%></td>                                 <%//部門コード%>
        <td><%=Html.Encode(a.DepartmentName)%></td>                                         <%//部門名%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", a.SalesOrderDate))%></td>        <%//受注日%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", a.SalesDate))%></td>             <%//納車日%>
        <td><%=Html.Encode(a.CustomerCode)%></td>                                           <%//顧客コード%>
        <td><%=Html.Encode(a.CustomerName)%></td>                                           <%//顧客名%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", a.JournalDate))%></td>           <%//決済日%>
        <td><%=Html.Encode(a.CustomerClaimCode)%></td>                                      <%//請求先コード%>
        <td><%=Html.Encode(a.CustomerClaimName)%></td>                                      <%//請求先名%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.Amount))%></td><%//決済金額%>
        <td><%=Html.Encode(a.CompleteFlagName)%></td>                                       <%//入金状況%>
        <td><%=Html.Encode(a.AccountCode)%></td>                                            <%//科目コード%><!--Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加-->
        <td><%=Html.Encode(a.AccountName)%></td>                                            <%//科目名%><!--Mod 2017/03/23 arc nakayama #3737_クレジット入金確認　検索結果項目追加-->
        <td><%=Html.Encode(a.Summary)%></td>                                                <%//摘要%>
    </tr>
    <%}%>
</table>

</asp:Content>

