<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.AccountsReceivable>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%//Mod 2015/08/11 arc yano  #3233 売掛金帳票対応 受け取るModelの方の変更(V_ReceivableManagement→AccountsReceivable)%>
    売掛金管理
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "ReceivableManagement", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%//=Html.Hidden("DefaultSlipType", ViewData["DefaultSlipType"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DefaultZerovisible", ViewData["DefaultZerovisible"]) %><%//ゼロ表示のデフォルト値) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:CSV出力 / それ以外:クリア)%>
<%=Html.Hidden("DefaultSummaryPattern", ViewData["DefaultSummaryPattern"]) %><%//集計方法のデフォルト値) %><%// Add 2020/05/22 yano #4032 %>
<%=Html.Hidden("DefaultClassification", ViewData["DefaultClassification"]) %><%//集計方法のデフォルト値) %><%// Add 2020/05/22 yano #4032 %>
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
        <th>区分</th>
        <td><%=Html.DropDownList("Classification", (IEnumerable<SelectListItem>)ViewData["ClassificationList"])%></td>
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
        <%//Mod 2020/05/22 yano #4032 %>
        <th>集計条件</th>
        <td><%=Html.RadioButton("SummaryPattern", "0", ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("0"))%>請求先毎<%=Html.RadioButton("SummaryPattern", "1", ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("1"))%>部門毎<%=Html.RadioButton("SummaryPattern", "9", ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("9"))%>なし</td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'SlipNumber', 'SalesDateFrom', 'SalesDateTo', 'CustomerCode', 'CustomerName', 'SummaryPattern', 'Classification'))"/><%//Mod 2020/05/22 yano #4032%>
        <!--Mod 2015/06/18 arc nakayama  売掛金対応① Excel対応　　ボタン名変更「CSV出力」⇒「Excel出力」-->
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('ReceivableManagement', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '1'"/><%//Mod 2020/05/22 yano #4032%>
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
<%//--------------------------------------------------------//%>
<%//                      ヘッダ行 　　　　　　　　　　　　 //%>
<%//--------------------------------------------------------//%>
<%//Mod 2020/05/22 yano #4032%>
<%if( ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("0")){ %>
     <%//請求先毎 %>
     <tr>
        <th style="white-space:nowrap;"rowspan="2">請求先コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先名</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分名</th>
        <th style="white-space:nowrap;"rowspan="2">伝票番号</th>
        <th style="white-space:nowrap;"rowspan="2">納車日</th>
        <th style="white-space:nowrap;"rowspan="2">部門コード</th>
        <th style="white-space:nowrap;"rowspan="2">部門名</th>
        <th style="white-space:nowrap;"rowspan="2">前月繰越(A)</th>
        <th style="white-space:nowrap;"colspan="2">当月発生(B)</th>
        <th style="white-space:nowrap;"rowspan="2">合計(A+B)</th>
        <th style="white-space:nowrap;"colspan="2">当月入金額</th>
        <th style="white-space:nowrap;"rowspan="2">残高</th>
    </tr>
    <tr>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
    </tr>
<%} else if( ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("1")){ %>
    <%//部門毎 %> 
    <tr>
        <th style="white-space:nowrap;"rowspan="2">部門コード</th>
        <th style="white-space:nowrap;"rowspan="2">部門名</th>
        <th style="white-space:nowrap;"rowspan="2">請求先コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先名</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分名</th>
        <th style="white-space:nowrap;"rowspan="2">伝票番号</th>
        <th style="white-space:nowrap;"rowspan="2">納車日</th>
        <th style="white-space:nowrap;"rowspan="2">前月繰越(A)</th>
        <th style="white-space:nowrap;"colspan="2">当月発生(B)</th>
        <th style="white-space:nowrap;"rowspan="2">合計(A+B)</th>
        <th style="white-space:nowrap;"colspan="2">当月入金額</th>
        <th style="white-space:nowrap;"rowspan="2">残高</th>
    </tr>
    <tr>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
    </tr> 
<%}else{ %>
     <%//なし %>
     <tr>
        <th style="white-space:nowrap;"rowspan="2">伝票番号</th>
        <th style="white-space:nowrap;"rowspan="2">納車日</th>
        <th style="white-space:nowrap;"rowspan="2">部門コード</th>
        <th style="white-space:nowrap;"rowspan="2">部門名</th>
        <th style="white-space:nowrap;"rowspan="2">顧客コード</th>
        <th style="white-space:nowrap;"rowspan="2">顧客名</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先区分名</th>
        <th style="white-space:nowrap;"rowspan="2">請求先コード</th>
        <th style="white-space:nowrap;"rowspan="2">請求先名</th>
        <th style="white-space:nowrap;"rowspan="2">前月繰越(A)</th>
        <th style="white-space:nowrap;"colspan="2">当月発生(B)</th>
        <th style="white-space:nowrap;"rowspan="2">合計(A+B)</th>
        <th style="white-space:nowrap;"colspan="2">当月入金額</th>
        <th style="white-space:nowrap;"rowspan="2">残高</th>
    </tr>
    <tr>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
        <th style="white-space:nowrap">諸費用以外</th>
        <th style="white-space:nowrap">諸費用</th>
    </tr>
<%} %>

    <%//--------------------------------------------------------//%>
    <%//                      データ行 　　　　　　　　　　　　 //%>
    <%//--------------------------------------------------------//%>
    <%//Mod 2015/08/11 arc yano  #3233 売掛金帳票対応 受け取るModelの方の変更(V_ReceivableManagement→AccountsReceivable)%>
    <!--Mod 2015/06/23 arc nakayama 伝票種別の名称をView上でマスタから取得するように変更-->
    <%
        //変数の初期化
        string prevCustomerClaimCode = "";
        string prevDepartmentCode = "";
        string style = "";
        string style2 = "";   
     %>

    <%if( ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("0")){ %>
    <%//請求先毎 %>
        <%foreach (var ReceivableMgm in Model) 
          {
              if (ReceivableMgm.SlipNumber.Contains("合計"))
              {
                  style = " style=\"font-weight:bold\"";
                  style2 = " style=\"text-align:right; font-weight:bold\"";
              }
              else
              {
                  style = "";
                  style2 = " style=\"text-align:right\"";
              }
              %>
            <tr>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.CustomerClaimCode) && ReceivableMgm.CustomerClaimCode.Equals(prevCustomerClaimCode) ? "" : ReceivableMgm.CustomerClaimCode)%></td><%//請求先コード%>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.CustomerClaimCode) && ReceivableMgm.CustomerClaimCode.Equals(prevCustomerClaimCode) ? "" : ReceivableMgm.CustomerClaimName)%></td><%//請求先名%>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.CustomerClaimCode) && ReceivableMgm.CustomerClaimCode.Equals(prevCustomerClaimCode) ? "" : ReceivableMgm.CustomerClaimType)%></td><%//請求先区分コード%>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.CustomerClaimCode) && ReceivableMgm.CustomerClaimCode.Equals(prevCustomerClaimCode) ? "" : ReceivableMgm.CustomerClaimTypeName)%></td><%//請求先区分%>  
                <td<%=style%>><%=Html.Encode(ReceivableMgm.SlipNumber)%></td><%//伝票番号%>
                <td<%=style%>><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", ReceivableMgm.SalesDate))%></td><%//納車日 ※当月前受金が0でない場合は納車日はNULL%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.DepartmentCode)%></td><%//部門コード%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.DepartmentName)%></td><%//部門名%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.CarriedBalance))%></td><%//前月繰越(A)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.PresentMonth))%></td><%//諸費用以外(当月発生(B))%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Expendes))%></td><%//諸費用(当月発生(B))%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.TotalAmount))%></td><%//合計(A+B)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Payment))%></td><%//当月入金額(諸費用以外)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.ChargesPayment))%></td><%//当月入金額(諸費用)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.BalanceAmount))%></td><%//残高%>
            </tr>
            <% prevCustomerClaimCode = ReceivableMgm.CustomerClaimCode; %>
        <%}%>
    <%} else if( ViewData["SummaryPattern"] != null && ViewData["SummaryPattern"].ToString().Equals("1")){ %>
    <%//部門毎 %>
        <%foreach (var ReceivableMgm in Model)
          {
              if (ReceivableMgm.CustomerClaimCode.Contains("合計"))
              {
                  style = " style=\"font-weight:bold\"";
                  style2 = " style=\"text-align:right; font-weight:bold\"";
              }
              else
              {
                  style = "";
                  style2 = " style=\"text-align:right\"";
              }
         %>
            <tr>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.DepartmentCode) && ReceivableMgm.DepartmentCode.Equals(prevDepartmentCode) ? "" : ReceivableMgm.DepartmentCode)%></td><%//部門コード%>
                <td<%=style%>><%=Html.Encode( !string.IsNullOrWhiteSpace(ReceivableMgm.DepartmentCode) && ReceivableMgm.DepartmentCode.Equals(prevDepartmentCode) ? "" : ReceivableMgm.DepartmentName)%></td><%//部門名%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.CustomerClaimCode)%></td><%//請求先コード%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.CustomerClaimName)%></td><%//請求先名%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.CustomerClaimType)%></td><%//請求先区分コード%>
                <td<%=style%>><%=Html.Encode(ReceivableMgm.CustomerClaimTypeName)%></td><%//請求先区分%>  
                <td<%=style%>><%=Html.Encode(ReceivableMgm.SlipNumber)%></td><%//伝票番号%>
                <td<%=style%>><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", ReceivableMgm.SalesDate))%></td><%//納車日 ※当月前受金が0でない場合は納車日はNULL%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.CarriedBalance))%></td><%//前月繰越(A)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.PresentMonth))%></td><%//諸費用以外(当月発生(B))%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Expendes))%></td><%//諸費用(当月発生(B))%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.TotalAmount))%></td><%//合計(A+B)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Payment))%></td><%//当月入金額(諸費用以外)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.ChargesPayment))%></td><%//当月入金額(諸費用)%>
                <td<%=style2%>><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.BalanceAmount))%></td><%//残高%>
            </tr>
            <% prevDepartmentCode = ReceivableMgm.DepartmentCode; %>
        <%}%>
    <%}else{ %>
    <%//なし %>
        <%foreach (var ReceivableMgm in Model)
          {%>
                <tr>
                    <td><%=Html.Encode(ReceivableMgm.SlipNumber)%></td><%//伝票番号%>
                    <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", ReceivableMgm.SalesDate))%></td><%//納車日 ※当月前受金が0でない場合は納車日はNULL%>
                    <td><%=Html.Encode(ReceivableMgm.DepartmentCode)%></td><%//部門コード%>
                    <td><%=Html.Encode(ReceivableMgm.DepartmentName)%></td><%//部門名%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerCode)%></td><%//顧客コード%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerName)%></td><%//顧客名%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerClaimType)%></td><%//請求先区分コード%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerClaimTypeName)%></td><%//請求先区分%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerClaimCode)%></td><%//請求先コード%>
                    <td><%=Html.Encode(ReceivableMgm.CustomerClaimName)%></td><%//請求先名%>    
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.CarriedBalance))%></td><%//前月繰越(A)%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.PresentMonth))%></td><%//諸費用以外(当月発生(B))%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Expendes))%></td><%//諸費用(当月発生(B))%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.TotalAmount))%></td><%//合計(A+B)%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.Payment))%></td><%//当月入金額(諸費用以外)%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.ChargesPayment))%></td><%//当月入金額(諸費用)%>
                    <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",ReceivableMgm.BalanceAmount))%></td><%//残高%>
                </tr>
        <%}%> 
    <%} %>
</table>


</asp:Content>
