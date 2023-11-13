<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ReceiptPlan>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金消込
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "ShopDeposit", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("KeePDispFlag", ViewData["KeePDispFlag"]) %>
<%=Html.Hidden("DefaultDispFlag", ViewData["DefaultDispFlag"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>表示単位</th>
        <td><%=Html.RadioButton("DispFlag", "0", ViewData["DispFlag"] != null && ViewData["DispFlag"].ToString().Equals("0"), new { onclick = "CheckDispFlag(0);"})%>明細<%=Html.RadioButton("DispFlag", "1", ViewData["DispFlag"] != null && ViewData["DispFlag"].ToString().Equals("1"), new { onclick = "CheckDispFlag(1);"})%>伝票・請求先の合計</td>
    </tr>
    <tr>
        <th>入金予定日</th>
        <%if (ViewData["DispFlag"].Equals("0"))
          {%>
            <td><%=Html.TextBox("ReceiptPlanDateFrom", ViewData["ReceiptPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("ReceiptPlanDateTo", ViewData["ReceiptPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <% }else{%>
            <td><%=Html.TextBox("ReceiptPlanDateFrom", ViewData["ReceiptPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, @disabled = "disabled" })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("ReceiptPlanDateTo", ViewData["ReceiptPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, @disabled = "disabled" })%></td>
        <%} %>
    </tr>
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("CondSlipNumber", ViewData["CondSlipNumber"], new { @class = "alphanumeric", size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th rowspan="2">請求先</th>
        <td><%=Html.TextBox("CondCustomerClaimCode", ViewData["CondCustomerClaimCode"], new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('CondCustomerClaimCode','CondCustomerClaimName','CustomerClaim')" })%>
            <img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CondCustomerClaimCode', 'CondCustomerClaimName', '/CustomerClaim/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="CondCustomerClaimName"><%=Html.Encode(ViewData["CondCustomerClaimName"])%></span></td>       
    </tr>
    <tr>
        <th>営業・サービス</th>
        <td><%=Html.DropDownList("AccountUsageType",(IEnumerable<SelectListItem>)ViewData["AccountUsageTypeList"]) %></td>
    </tr>
    <tr>
        <th></th>
        <td>
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('ReceiptPlanDateFrom','ReceiptPlanDateTo','CondSlipNumber','CondCustomerClaimCode','CondCustomerClaimName','AccountUsageType','DispFlag'))" />
        </td>
    </tr>
</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケーターを表示するよう修正--%>
<div id ="UpdateMsg" style ="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<%=Html.ValidationSummary()%>
<br />
<input type="button" value="請求書発行" onclick="printInvoice(<%=Model.Count %>)" />&nbsp;
<input type="button" value="入金消込登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />&nbsp;
<input type="button" value="ワランティ一括取込" onclick="openModalAfterRefresh('/ShopDeposit/ImportDialog')"/><%//Add  2018/05/28 arc yano #3886%>
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
        <th rowspan="2" style="width:30px"><%=Html.CheckBox("AllCheck",false,new {onclick="allChecked(this, Depositcheck)"}) %><%//=Html.CheckBox("AllCheck",false,new {onclick="checkAll("+Model.Count+")"}) %></th>
        <th rowspan="2" style="width:30px"></th>
        <th style="width:100px">入金日 *</th>
        <th>入金事業所</th>
        <th style="width:80px">納車日</th>
        <th>伝票番号</th>
        <th>備考</th>
        <th style="width:80px">入金予定日</th>
        <th style="width:150px">入金額</th>
    </tr>
    <tr>
        <th>入金種別 *</th>
        <th>口座 *</th>
        <th>納車予定日</th>
        <th>請求先</th>
        <th>摘要</th>
        <th>入金予定額</th>
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
          backColor = "#ccccff";
      }
      //Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）
      if (string.IsNullOrEmpty(receiptPlan.strReceiptPlanId))
      {
          receiptPlan.strReceiptPlanId = receiptPlan.ReceiptPlanId.ToString();
      }
    %>
    <% // <!--//2014/05/30 vs2012対応 arc yano 各コントロールにid追加 %>
    <%=Html.Hidden(namePrefix + "ReceiptPlanId", receiptPlan.ReceiptPlanId.ToString(), new { id = idPrefix + "ReceiptPlanId"})%>
    <%=Html.Hidden(namePrefix + "OfficeCode", receiptPlan.ReceiptDepartment != null ? receiptPlan.ReceiptDepartment.OfficeCode : "", new { id = idPrefix + "OfficeCode"})%>
    <%=Html.Hidden(namePrefix + "SlipNumber", receiptPlan.SlipNumber, new { id = idPrefix + "SlipNumber"})%>
    <%=Html.Hidden(namePrefix + "CustomerClaimCode", receiptPlan.CustomerClaim.CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode"})%>
    <tr>
        <!--Mod 2016/01/07 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう-->
        <td rowspan="2" style="background-color:<%=backColor%>">
            <%=Html.CheckBox(namePrefix + "Changetarget", receiptPlan.Changetarget, new { id = idPrefix + "Changetarget", onclick = "Depositcheck("+ i +")"})%>
            <%=Html.Hidden(namePrefix + "targetFlag", receiptPlan.targetFlag, new { id = idPrefix + "targetFlag"})%>
        </td>
        <% // Mod 2014/07/09 arc amii chrome対応 個別画面でボーダー位置調整の為、画面サイズを指定するように修正 %>
        <td rowspan="2" style="background-color:<%=backColor%>;white-space:nowrap"><a href="javascript:void(0)" onclick="openModalAfterRefresh2('/ShopDeposit/Detail?strReceiptPlanId=<%=receiptPlan.ReceiptPlanId%>&SlipNumber=<%=receiptPlan.SlipNumber%>&CustomerClaimCode=<%=receiptPlan.CustomerClaim.CustomerClaimCode %>&KeePDispFlag=<%=ViewData["KeePDispFlag"]%>')">個別</a></td>
        <td style="background-color:<%=backColor%>"><%=Html.TextBox(namePrefix + "JournalDate", string.Format("{0:yyyy/MM/dd}", journal.JournalDate), new { id = idPrefix + "JournalDate", @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.ReceiptDepartment.Office!=null ? receiptPlan.ReceiptDepartment.Office.OfficeName : "") %></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.CarSalesHeader.SalesDate) : receiptPlan.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",receiptPlan.ServiceSalesHeader.SalesDate) : "" )%></td>
        <td style="background-color:<%=backColor%>"><%if (receiptPlan.Account != null && receiptPlan.Account.UsageType != null && receiptPlan.Account.UsageType.Equals("CR")) { %><a href="javascript:void(0)" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=Html.Encode(receiptPlan.SlipNumber)%>');return false;"><%=Html.Encode(receiptPlan.SlipNumber)%></a><%}else{ %><a href="javascript:void(0)" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=Html.Encode(receiptPlan.SlipNumber) %>');return false;"><%=Html.Encode(receiptPlan.SlipNumber) %></a><%} %></td>        
        <td style="background-color:<%=backColor%>"><%=CommonUtils.DefaultNbsp(receiptPlan.Summary) %></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.ReceiptPlanDate != null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.ReceiptPlanDate) : "-")%></td>
        <% // Mod 2014/07/18 arc yano chrome対応 white-space:nowrap指定追加 %>
        <td style="text-align:right;white-space:nowrap";background-color:<%=backColor%>"><input type="button" value="全額" style="width:35px" onclick="document.getElementById('<%=idPrefix %>Amount').value = '<%=receiptPlan.ReceivableBalance %>'" />&nbsp;
<%=Html.TextBox(namePrefix + "Amount", journal.Amount, new { id = idPrefix + "Amount" ,@class = "numeric", size = 10, maxlength = 10 })%>
</td>
    </tr>
    <tr>
        <td style="background-color:<%=backColor%>"><%=Html.DropDownList(namePrefix + "AccountType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["AccountTypeList"])[i], new { id = idPrefix + "AccountType"})%></td>
        <td style="background-color:<%=backColor%>"><%=Html.DropDownList(namePrefix + "CashAccountCode", ((List<IEnumerable<SelectListItem>>)ViewData["CashAccountList"])[i], new { id = idPrefix + "CashAccountCode"})%></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(receiptPlan.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", receiptPlan.CarSalesHeader.SalesPlanDate) : receiptPlan.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",receiptPlan.ServiceSalesHeader.SalesPlanDate) : "" )%></td>
        <td style="background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(customerClaimName)%></td>
        <td style="background-color:<%=backColor%>"><%=Html.TextBox(namePrefix + "Summary",journal.Summary,new { id = idPrefix + "Summary", maxlength=50}) %></td>
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", receiptPlan.Amount)) %></td>        
        <td style="text-align:right;background-color:<%=backColor%>;white-space:nowrap"><%if(receiptPlan.ReceivableBalance<0){ %><span style="color:Red;font-weight:bold"><%} %><%=Html.Encode(string.Format("{0:N0}", receiptPlan.ReceivableBalance))%><%if(receiptPlan.ReceivableBalance<0){ %></span><%} %></td>
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
        <th><div style="text-align:right;font-weight:bold;white-space:nowrap">残高合計</div></th>
        <th><div style="text-align:right;font-weight:bold;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", ViewData["TotalBalance"]))%></div></th>
    </tr>
</table>
<br />
<input type="button" value="入金消込登録" onclick="document.forms[0].action.value='regist';displaySearchList()" />
<br />
<%} %>
<br />
</asp:Content>
