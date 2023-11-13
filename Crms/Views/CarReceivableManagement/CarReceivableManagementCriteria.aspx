<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.AccountsReceivableCar>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両売掛金管理
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarReceivableManagement", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DefaultZerovisible", ViewData["DefaultZerovisible"]) %><%//ゼロ表示のデフォルト値) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:Excel出力 / それ以外:クリア)%>
<br/>
<br/>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<%=Html.ValidationSummary() %>
<table class="input-form">
    <tr>
        <th style="width:100px">対象年月 *</th>
        <td><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"], new { onchange="CheckDateForDropDown()" })%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"], new { onchange="CheckDateForDropDown()" })%>&nbsp;月&nbsp;</td>      
        <th>納車日</th>
        <td><%=Html.TextBox("SalesDateFrom",ViewData["SalesDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10,  onchange ="return chkDate3(document.getElementById('SalesDateFrom').value, 'SalesDateFrom')" }) %>～<%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('SalesDateTo').value, 'SalesDateTo')"}) %></td>     
    </tr>
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
        <th colspan="2"></th>
    </tr>
    <tr>
        <th>部門コード</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", maxlength = 3,  onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>        
        <th>部門名</th>
        <td><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" /></td>
        <th>顧客名</th>
        <td><span id="CustomerName"><%=Html.Encode(ViewData["CustomerName"]) %></span></td>
    </tr>
    <tr>
        <th>ゼロ表示</th>
        <td><%=Html.RadioButton("Zerovisible", "1", ViewData["Zerovisible"] != null && ViewData["Zerovisible"].ToString().Equals("1"))%>する<%=Html.RadioButton("Zerovisible", "0", ViewData["Zerovisible"] != null && ViewData["Zerovisible"].ToString().Equals("0"))%>しない</td>
        <th colspan="2"></th>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'SlipNumber', 'SalesDateFrom', 'SalesDateTo', 'CustomerCode', 'CustomerName', 'Zerovisible'))"/>
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('ReceivableManagement', 'UpdateMsg'); document.forms[0].submit();"/>
        </td>
    </tr>
</table>
<br />
<br />
<%} %>
</div>
<br />
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<table class="input-form">
    <tr>
        <th style="width:100px">締処理状況</th>
        <td style="white-space:nowrap; text-align:center; width:100px"><%=(Html.Encode((ViewData["CloseStatus"] != null && ViewData["CloseStatus"] == "") ? "未実施" : ViewData["CloseStatus"] ))%></td><%//締処理状況"%>
    </tr>
</table>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="input-form">
     <tr>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">納車日</th>
        <th style="white-space:nowrap">部門コード</th>
        <th style="white-space:nowrap">部門名</th>
        <th style="white-space:nowrap">顧客コード</th>
        <th style="white-space:nowrap">顧客名</th>
        <th style="white-space:nowrap">前月繰越(A)</th>
        <th style="white-space:nowrap">当月発生(B)</th>
        <th style="white-space:nowrap">当月入金額(C)</th>
        <th style="white-space:nowrap">残高(A+B-C)</th>
    </tr>
    <%foreach (var ReceivableMgm in Model)
      {%>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=Html.Encode(ReceivableMgm.SlipNumber)%>')"><%=Html.Encode(ReceivableMgm.SlipNumber)%></a></td>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", ReceivableMgm.SalesDate))%></td><%//納車日 ※当月前受金が0でない場合は納車日はNULL%>
        <td><%=Html.Encode(ReceivableMgm.DepartmentCode)%></td><%//部門コード%>
        <td><%=Html.Encode(ReceivableMgm.DepartmentName)%></td><%//部門名%>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/RevenueResult/Detail?SlipNo=<%=ReceivableMgm.SlipNumber%>&CustomerCode=<%=ReceivableMgm.CustomerCode%>&ST=<%="車両"%>')"><%=Html.Encode(ReceivableMgm.CustomerCode)%></a></td>
        <td><%=Html.Encode(ReceivableMgm.CustomerName)%></td><%//顧客名%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.CarriedBalance))%></td><%//前月繰越(A)%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.PresentMonth))%></td><%//当月発生金額%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.PaymentAmount))%></td><%//当月入金額%>
        <!--まだ請求等が残っている状態で計算結果が０になっているので赤文字で残高を表示-->
        <%if (ReceivableMgm.BalanceAmount == 0 && ((ReceivableMgm.CustomerBalance != null && ReceivableMgm.CustomerBalance != 0) || (ReceivableMgm.TradeBalance != null && ReceivableMgm.TradeBalance != 0) || (ReceivableMgm.RemainDebtBalance != null && ReceivableMgm.RemainDebtBalance != 0)))
          {%>
            <td style="text-align:right;color:red"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.BalanceAmount))%></td><%//残高%>
        <%}else{ %><!--そうでなければ、顧客への請求・下取車の仕入・下取車の残債が０で残高が０になっているのでデフォルトの黒文字で残高を表示-->
            <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.BalanceAmount))%></td><%//残高%>
        <%} %>
    </tr>
    <%}%>
</table>


</asp:Content>