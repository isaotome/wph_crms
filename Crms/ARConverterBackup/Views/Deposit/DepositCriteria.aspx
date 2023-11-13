<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ReceiptPlan>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金消込
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Deposit", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("KeePDispFlag", ViewData["KeePDispFlag"]) %>
<%=Html.Hidden("DefaultDispFlag", ViewData["DefaultDispFlag"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<!--Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加　入金予定日を決済事業所の上に移動-->
<table class="input-form">
    <tr>
        <th>表示単位</th>
        <td><%=Html.RadioButton("DispFlag", "0", ViewData["DispFlag"] != null && ViewData["DispFlag"].ToString().Equals("0"), new { onclick = "CheckDispFlag(0);"})%>明細<%=Html.RadioButton("DispFlag", "1", ViewData["DispFlag"] != null && ViewData["DispFlag"].ToString().Equals("1"), new { onclick = "CheckDispFlag(1);"})%>伝票・請求先の合計</td>
    </tr>
    <tr>
        <th>入金予定日</th>
        <%if (ViewData["DispFlag"].Equals("0")){%>
            <td><%=Html.TextBox("ReceiptPlanDateFrom", ViewData["ReceiptPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("ReceiptPlanDateTo", ViewData["ReceiptPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <%}else{ %>
            <td><%=Html.TextBox("ReceiptPlanDateFrom", ViewData["ReceiptPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, @disabled = "disabled" })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("ReceiptPlanDateTo", ViewData["ReceiptPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, @disabled = "disabled" })%></td>
        <%} %>
    </tr>
    <tr>
        <th rowspan="2">決済事業所</th>
        <td><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", size = 10, maxlength = 3, onblur = "GetNameFromCode('OfficeCode','OfficeName','Office')" })%>&nbsp;<img alt="" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('OfficeCode','OfficeName','/Office/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="OfficeName"><%=ViewData["OfficeName"] %></span></td>
    </tr>
    <tr>
        <th>納車日</th>
        <td><%=Html.TextBox("SalesDateFrom", ViewData["SalesDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 }) %>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 }) %></td>
    </tr>
    <tr>
        <th>決済日</th>
        <td><%=Html.TextBox("JournalDateFrom", ViewData["JournalDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 }) %>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("JournalDateTo", ViewData["JournalDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 }) %></td>
    </tr>
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("CondSlipNumber", ViewData["CondSlipNumber"], new { @class = "alphanumeric", size = 50, maxlength = 50 })%></td>
    </tr>
    <tr><%//Add 2016/05/18 arc yano #3558 社内請求分の表示絞り込み%>
        <th></th>
        <td><%=Html.RadioButton("CustomerClaimFilter", "001", ViewData["CustomerClaimFilter"] != null && ViewData["CustomerClaimFilter"].ToString().Equals("001"), new { onclick = "GetCustomerClaimTypeListByCustomerClaimFilter(this.value, 'CustomerClaimType')"})%>クレジット・ローンのみ<%=Html.RadioButton("CustomerClaimFilter", "002", ViewData["CustomerClaimFilter"] != null && ViewData["CustomerClaimFilter"].ToString().Equals("002"), new { onclick = "GetCustomerClaimTypeListByCustomerClaimFilter(this.value, 'CustomerClaimType')"})%>社内のみ<%=Html.RadioButton("CustomerClaimFilter", "", ViewData["CustomerClaimFilter"] == null || ViewData["CustomerClaimFilter"].ToString().Equals(""), new { onclick = "GetCustomerClaimTypeListByCustomerClaimFilter(this.value, 'CustomerClaimType')"})%>全て
            <%=Html.Hidden("DefaultCustomerClaimFilter", "001") %><%//デフォルト値 %>
        </td>
    </tr>
    <tr>
        <th>請求先区分</th>
        <td><%=Html.DropDownList("CustomerClaimType",(IEnumerable<SelectListItem>)ViewData["CustomerClaimTypeList"]) %></td>
    </tr>
    <tr>
        <th rowspan="2">請求先</th>
        <td><%=Html.TextBox("CondCustomerClaimCode", ViewData["CondCustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10, onblur = "GetCustomerClaimWithClaimable('CondCustomerClaimCode','CondCustomerClaimName','PaymentKindCode')" })%>
            <img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() { GetCustomerClaimWithClaimable('CondCustomerClaimCode','CondCustomerClaimName','PaymentKindCode'); document.getElementById('PaymentKindCode').focus()};  openSearchDialog('CondCustomerClaimCode', 'CondCustomerClaimName', '/CustomerClaim/CriteriaDialog', null, null, null, null, callback); GetCustomerClaimWithClaimable('CondCustomerClaimCode','CondCustomerClaimName','PaymentKindCode'); document.getElementById('PaymentKindCode').focus()" /><%//Mod 2022/01/10 yano #4121%>
            <%--<img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondCustomerClaimCode', 'CondCustomerClaimName', '/CustomerClaim/CriteriaDialog');document.getElementById('PaymentKindCode').focus()" />--%>
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="CondCustomerClaimName"><%=Html.Encode(ViewData["CondCustomerClaimName"])%></span></td>       
    </tr>
    <tr>
        <th>決済種別</th>
        <td>
            <%=Html.DropDownList("PaymentKindCode",(IEnumerable<SelectListItem>)ViewData["PaymentKindList"]) %>
        </td>
    </tr>
    <tr>
        <th>営業・サービス</th>
        <td><%=Html.DropDownList("AccountUsageType",(IEnumerable<SelectListItem>)ViewData["AccountUsageTypeList"]) %></td>
    </tr>
    <tr>
        <th>入金種別</th>
        <td><%=Html.DropDownList("ReceiptType", (IEnumerable<SelectListItem>)ViewData["ReceiptTypeList"]) %></td>
    </tr>
    <tr>
        <th></th>
        <td><%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <%//Mod 2016/05/18 arc yano #3558 社内請求分の表示絞り込み ラジオボタンの初期化処理を追加%>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('OfficeCode','OfficeName','SalesDateFrom','SalesDateTo','JournalDateFrom','JournalDateTo','ReceiptPlanDateFrom','ReceiptPlanDateTo','CondSlipNumber','CustomerClaimType','CondCustomerClaimCode','CondCustomerClaimName','PaymentKindCode','AccountUsageType','ReceiptType', 'CustomerClaimFilter','DispFlag')); GetCustomerClaimTypeListByCustomerClaimFilter(document.getElementById('DefaultCustomerClaimFilter').value, 'CustomerClaimType') " />
        </td>
    </tr>
</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30"/>
</div>

<%=Html.ValidationSummary()%>
<br />
<div style="float:left">
<input type="button" value="請求書発行" onclick="printInvoice(<%=Model.Count %>)" />&nbsp;&nbsp;
<input type="button" value="入金消込登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />
</div>
<div style="float:right;margin-right:2%">
<table style="border:solid 1px black ;border-collapse:collapse">
    <tr>
        <th style="background-color:Yellow;padding:5px"><div style="font-size:11pt;text-align:right">請求残高合計</div></th>
        <td style="background-color:Yellow;text-align:center;padding:5px"><div style="font-size:14pt;font-weight:bold"><span id="TotalBalance"><%=string.Format("{0:N0}",ViewData["TotalBalance"]) %></span></div></td>
    </tr>
    <!--Mod 2016/07/22 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）-->
    <tr>
        <th style="background-color:Yellow;padding:5px"><div style="font-size:11pt;text-align:right">入金後の残高合計</div></th>
        <td style="background-color:Yellow;text-align:center;padding:5px"><div style="font-size:14pt;font-weight:bold"><span id="CalcTotalBalance"><%=string.Format("{0:N0}",ViewData["CalcTotalBalance"]) %></span></div></td>
    </tr>
</table>
</div>
<br />
<br />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<%=Html.Hidden("CheckFlag","0") %>
<%=Html.Hidden("JournalCount",Model.Count()) %>
<br />
<br />
<table class="list">
    <tr>
        <%//Mod 2016/04/19 arc yano #3490 店舗入金消込 入金管理画面で[全選択]のチェックボックスが動作しない %>
        <th rowspan="2" style="width:30px"><%=Html.CheckBox("AllCheck",false,new {onclick="allChecked(this, Depositcheck);calcDepositAmount("+ Model.Count +");"}) %><%//=Html.CheckBox("AllCheck",false,new {onclick="checkAll("+Model.Count+")"}) %></th>
        <!--<th rowspan="2" style="width:30px"></th>-->
        <th style="width:100px">入金日 *</th>
        <th>入金事業所*</th><%//Mod 2019/02/14 yano #3978%>
        <th style="width:80px">納車日</th>
        <th>伝票番号</th>
        <th>備考</th>
        <th style="width:80px">入金予定日</th>
        <th>手数料抜き</th>
        <th style="width:150px">入金額</th>
    </tr>
    <tr>
        <th>入金種別 *</th>
        <th>口座 *</th>
        <th>納車予定日</th>
        <th>請求先</th>
        <th>摘要</th>
        <th>入金予定額</th>
        <th>手数料</th>
        <th>請求残高</th>
    </tr>
<%for (int i = 0; i < Model.Count; i++) {
      string namePrefix = string.Format("line[{0}].", i);
      string idPrefix = string.Format("line[{0}]_", i);
      ReceiptPlan receiptPlan = Model[i];
      Journal journal = ((List<Journal>)ViewData["JournalList"])[i];
      string customerClaimName = "";
      try {
          customerClaimName = receiptPlan.CustomerClaim.CustomerClaimName;
      } catch (NullReferenceException) {
      }
      string accountName = "";
      try { accountName = receiptPlan.Account.AccountName; } catch (NullReferenceException) { }
      
      string backColor = "#ffffff";
      if (i % 2 == 1) {
          backColor = "#f5f5f5";
      }

      //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）
      if (string.IsNullOrEmpty(receiptPlan.strReceiptPlanId))
      {
          receiptPlan.strReceiptPlanId = receiptPlan.ReceiptPlanId.ToString();
      }
          
    %>
    <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにid追加 %>
    <%//=Html.Hidden(namePrefix + "ReceiptPlanId", receiptPlan.ReceiptPlanId.ToString(), new { id = idPrefix + "ReceiptPlanId" })%>
    <%=Html.Hidden(namePrefix + "ReceiptPlanId", receiptPlan.strReceiptPlanId, new { id = idPrefix + "ReceiptPlanId" })%>
    <%--<%=Html.Hidden(namePrefix + "OfficeCode", receiptPlan.ReceiptDepartment != null ? receiptPlan.ReceiptDepartment.OfficeCode : "", new { id = idPrefix + "OfficeCode"})%>--%><%//Mod 2019/02/14 yano #3978 一覧の中で移動 %>
    <%=Html.Hidden(namePrefix + "SlipNumber", receiptPlan.SlipNumber, new { id = idPrefix + "SlipNumber"})%>
    <%=Html.Hidden(namePrefix + "CustomerClaimCode", receiptPlan.CustomerClaim.CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode"})%>
    <tr>
        <%// 2014/09/22 arc yano chrome対応 name,idを個別で設定する。%>
        <!--Mod 2016/01/07 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう-->
        <td rowspan="2" style="background-color:<%=backColor%>">
            <%=Html.CheckBox(namePrefix + "Changetarget", receiptPlan.Changetarget, new { id = idPrefix + "Changetarget", onclick = "Depositcheck("+ i +");calcDepositAmount("+ Model.Count +");"})%>
            <%=Html.Hidden(namePrefix + "targetFlag", receiptPlan.targetFlag, new { id = idPrefix + "targetFlag"})%>
        </td>
        <!--<td rowspan="2" style="background-color:<%=backColor%>;white-space:nowrap"><a href="javascript:void(0)" onclick="openModalAfterRefresh2('/ShopDeposit/Detail/<%=receiptPlan.strReceiptPlanId %>')">個別</a></td>-->
        <td style="background-color:<%=backColor%>"><%=Html.TextBox(namePrefix + "JournalDate", string.Format("{0:yyyy/MM/dd}", journal.JournalDate), new { id = idPrefix + "JournalDate", @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        
        <%//Mod 2019/02/14 yano #3978%>
        <td style="background-color:<%=backColor%>;white-space:nowrap">
            <%=Html.TextBox(namePrefix + "OfficeCode", receiptPlan.ReceiptDepartment != null ? receiptPlan.ReceiptDepartment.OfficeCode : "", new { id = idPrefix + "OfficeCode", @class = "alphanumeric", size = 1, maxlength = 3, onblur = "GetNameFromCode('" + idPrefix + "OfficeCode', '" + idPrefix + "OfficeName','Office', 'false', GetCashAccountCode, new Array('" + idPrefix + "OfficeCode', '" + idPrefix + "OfficeName', '" + idPrefix + "CashAccountCode', false))"}) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "OfficeCode", idPrefix + "OfficeName", "'/Office/CriteriaDialog'", "0","GetCashAccountCode( new Array('" + idPrefix + "OfficeCode', '" + idPrefix + "OfficeName', '" + idPrefix + "CashAccountCode', false))" });%>
            <span id="<%=idPrefix%>OfficeName"><%=Html.Encode(receiptPlan.ReceiptDepartment.Office!=null ? receiptPlan.ReceiptDepartment.Office.OfficeName : "") %></span>
        </td>
        <%--<td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.ReceiptDepartment.Office!=null ? receiptPlan.ReceiptDepartment.Office.OfficeName : "") %></td>--%>
        
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.CarSalesHeader.SalesDate) : receiptPlan.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",receiptPlan.ServiceSalesHeader.SalesDate) : "" )%></td>
        <td style="background-color:<%=backColor%>"><%if (receiptPlan.Account != null && receiptPlan.Account.UsageType != null && receiptPlan.Account.UsageType.Equals("CR")) { %><a href="javascript:void(0)" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=Html.Encode(receiptPlan.SlipNumber)%>');return false;"><%=Html.Encode(receiptPlan.SlipNumber)%></a><%}else{ %><a href="javascript:void(0)" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=Html.Encode(receiptPlan.SlipNumber) %>');return false;"><%=Html.Encode(receiptPlan.SlipNumber) %></a><%} %></td>        
        <td style="background-color:<%=backColor%>"><%=CommonUtils.DefaultNbsp(receiptPlan.Summary) %></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.ReceiptPlanDate != null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.ReceiptPlanDate) : "-")%></td>
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",(receiptPlan.Amount ?? 0m) - (receiptPlan.CommissionAmount ?? 0m)))%></td>
        <td style="text-align:right;background-color:<%=backColor%>"><input type="button" value="全額" style="width:35px" onclick="document.getElementById('<%=idPrefix %>Amount').value = '<%=receiptPlan.ReceivableBalance %>';calcDepositAmount(<%=Model.Count%>);" />&nbsp;

<% // <%=Html.TextBox(namePrefix + "Amount", journal.Amount, new { @class = "numeric", size = 10, maxlength = 10 })%>
<%=Html.TextBox(namePrefix + "Amount", journal.Amount, new { id = idPrefix + "Amount", @class = "numeric", size = 10, maxlength = 10, onblur = "calcDepositAmount("+ Model.Count +");"})%>
</td>

    </tr>
    <tr>
        <td style="background-color:<%=backColor%>"><%=Html.DropDownList(namePrefix + "AccountType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["AccountTypeList"])[i], new { id = idPrefix + "AccountType"})%></td>
        <td style="background-color:<%=backColor%>"><%=Html.DropDownList(namePrefix + "CashAccountCode", ((List<IEnumerable<SelectListItem>>)ViewData["CashAccountList"])[i], new { id = idPrefix + "CashAccountCode"})%></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.CarSalesHeader.SalesPlanDate) : receiptPlan.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",receiptPlan.ServiceSalesHeader.SalesPlanDate) : "" )%></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(customerClaimName)%></td>
        <td style="background-color:<%=backColor%>"><%=Html.TextBox(namePrefix + "Summary",journal.Summary, new { id = idPrefix + "Summary", maxlength=50}) %></td>
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", receiptPlan.Amount)) %></td>     
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", (receiptPlan.CommissionAmount ?? 0m)))%></td>   
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", receiptPlan.ReceivableBalance))%></td>
    </tr>
<%} %>
    <tr>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
        <th></th>
        <th><div style="text-align:right;font-weight:bold;white-space:nowrap">入金額合計</div></th>
        <th><div style="text-align:right;font-weight:bold;white-space:nowrap"><span id="TotalAmount"><%=string.Format("{0:N0}",ViewData["TotalAmount"]) %></span></div></th>
    </tr>
</table>
<br />
<input type="button" value="入金消込登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />
<br />
<%} %>
<br />
</asp:Content>
