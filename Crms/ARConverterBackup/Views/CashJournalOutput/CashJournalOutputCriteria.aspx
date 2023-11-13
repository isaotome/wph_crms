<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.T_CashJournalOutput>>"%>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    現金出納帳出力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Criteria", "CashJournalOutput", new { id = 0 }, FormMethod.Post))
  { %>
    <%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("CashAccountName", ViewData["CashAccountName"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:CSV出力 / それ以外:クリア)%>

<br/>
<br/>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<% //Mod 2015/03/20 arc yano 現金出納帳出力対応 エクセル出力ボタンクリック時は、ValidationErrorMessageを消す。※エクセル出力時は画面が更新されないため%>
<div id="validatediv">
<%=Html.ValidationSummary() %>
</div>
<% //Mod 2015/02/20 arc yano 現金出納帳出力対応 レイアウト変更(事業所名はテキストボックスではなく、ラベルにする)%>
<table class="input-form">
    <tr>
        <th style="width:80px">対象年月 *</th>
        <td colspan ="3"><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;</td>      
    </tr>
    <tr>
        <th style="width:80px">事務所コード</th>
         <%//Add 2015/03/19 arc yano 現金出納帳出力(エクセル)対応 有効な事務所コードが入力されている場合はエクセル出力ボタンを活性にする%>
        <td colspan ="3"><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @clas = "alphanumeric", size = 1, maxlength = 3, onblur = "GetCashAccountCode(new Array('OfficeCode', 'OfficeName', 'CashAccountCode'), null, 'excelOut')" })%> <%Html.RenderPartial("SearchButtonControl", new string[] { "OfficeCode", "OfficeName", "'/Office/CriteriaDialog'", "0", "GetCashAccountCode(new Array('OfficeCode', 'OfficeName', 'CashAccountCode'))" }); %> <span id ="OfficeName" ><%=Html.Encode(ViewData["OfficeName"])%></span></td><%//Mod 2019/02/14 yano #3972%>
    </tr>
    <tr>
        <th>現金口座名</th>
        <td colspan ="3"><%=Html.DropDownList("CashAccountCode", (IEnumerable<SelectListItem>)ViewData["CashAccountList"], new { Style = "width:190px;" })%></td>   
    </tr>

    
    <tr>
        <th></th>
        <td colspan ="3">
            <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <%//Add 2015/03/19 arc yano 現金出納帳出力(エクセル)対応 クリアボタン押下時はボタンを非活性にする%>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'OfficeCode', 'OfficeName', 'CashAccountCode')); SetDisabled('excelOut', true, null)"/>
            <input type="button" value="CSV出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CashJournalOutput', 'UpdateMsg'); document.forms[0].submit();"/>
            <%//Add 2015/03/19 arc yano 現金出納帳出力(エクセル)対応 エクセル出力ボタン非活性化フラグ((True:活性 / False:非活性))%>
            <% if (ViewData["ExcelButtonEnabled"] != null && ViewData["ExcelButtonEnabled"].Equals(true)) {%>
            <input type="button" name="excelOut" value="Excel出力" onclick="document.getElementById('validatediv').style.display = 'none'; document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); dispProgressed('CashJournalOutput', 'UpdateMsg'); document.forms[0].submit();"/>
            <%}else{ %>
            <input type="button" name="excelOut" value="Excel出力" disabled ="disabled" onclick="document.getElementById('validatediv').style.display = 'none'; document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); dispProgressed('CashJournalOutput', 'UpdateMsg'); document.forms[0].submit();"/>
            <%} %>
        </td>
    </tr>
</table>
<br />
<br />
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
</div>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="input-form" >
    <tr>
        <th style ="white-space:nowrap">事務所コード</th>
        <th style ="white-space:nowrap">現金口座コード</th>
        <th style ="white-space:nowrap">事務所名</th>
        <th style ="white-space:nowrap">現金口座名</th>
        <th style ="text-align:right;white-space:nowrap">前月残高</th>
        <th style ="text-align:right;white-space:nowrap">当月入金</th>
        <th style ="text-align:right;white-space:nowrap">当月出金</th>
        <th style ="text-align:right;white-space:nowrap">当月残高</th>
    </tr>
    <%foreach (var CashJournalData in Model)
    {%>
    <tr>
        <td><%=Html.Encode(CashJournalData.OfficeCode)%></td>                                  <%//事務所コード%>
        <td><%=Html.Encode(CashJournalData.CashAccountCode)%></td>                             <%//現金口座コード%>
        <td><%=Html.Encode(CashJournalData.OfficeName)%></td>                                  <%//事務所名%>
        <td><%=Html.Encode(CashJournalData.CashAccountName)%></td>                             <%//現金口座名%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:#,0}", CashJournalData.LastMonthBalance))%></td>  <%//前月残高%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:#,0}", CashJournalData.ThisMonthJournal))%></td>  <%//当月入金%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:#,0}", CashJournalData.ThisMonthPayment))%></td>  <%//当月出金%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:#,0}", CashJournalData.ThisMonthBalance))%></td>  <%//当月残高%>
    </tr>
       <% } %>
     <% } %>
</table>

</asp:Content>
