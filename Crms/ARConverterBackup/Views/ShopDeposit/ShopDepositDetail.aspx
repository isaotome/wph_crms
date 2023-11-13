<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ReceiptPlan>" %>
<%@ Import Namespace="CrmsDao" %>
<%@ Import Namespace="System.Data.Linq" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ShopDepositDetail
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("Detail","ShopDeposit",FormMethod.Post)){ %>
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <td onclick="formSubmit()"><img src="/Content/Images/build.png" alt="消込処理" class="command_btn" />&nbsp;消込処理</td>
    </tr>
</table>
<br />
<%=Html.Hidden("ReceiptPlanId",Model.ReceiptPlanId) %>
<%=Html.Hidden("AccountCode",Model.AccountCode) %>
<%=Html.Hidden("ReceivableBalance",Model.ReceivableBalance) %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.ValidationSummary() %>
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">入金予定日</th>
        <th style="width:100px">納車予定日</th>
        <th style="width:300px">請求先</th>
        <th style="width:100px">入金予定額</th>
    </tr>
    <tr>
        <!--Mod 2016/07/22 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加　車両伝票の納車予定日しか出ないようになっていたため、サービス伝票の納車予定日も出すように修正-->
        <td><%=CommonUtils.DefaultNbsp(Model.ReceiptPlanDate != null ? string.Format("{0:yyyy/MM/dd}",Model.ReceiptPlanDate) : "-") %><%=Html.Hidden("ReceiptPlanDate",Model.ReceiptPlanDate) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", Model.CarSalesHeader.SalesPlanDate) : Model.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",Model.ServiceSalesHeader.SalesPlanDate) : "" ) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model.CustomerClaimCode) %>&nbsp;<%=CommonUtils.DefaultNbsp(Model.CustomerClaim!=null ? Model.CustomerClaim.CustomerClaimName : "") %><%=Html.Hidden("CustomerClaimCode",Model.CustomerClaimCode) %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",Model.Amount)) %><%=Html.Hidden("Amount",Model.Amount) %></td>
    </tr>
    <tr>
        <th>伝票番号</th>
        <th>納車日</th>
        <th>備考</th>
        <th>請求残高</th>
    </tr>
    <tr>
        <!--Mod 2016/07/22 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加　車両伝票の納車日しか出ないようになっていたため、サービス伝票の納車日も出すように修正-->
        <td><%=CommonUtils.DefaultNbsp(Model.SlipNumber)%><%=Html.Hidden("SlipNumber",Model.SlipNumber) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model.CarSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}", Model.CarSalesHeader.SalesDate) : Model.ServiceSalesHeader!=null ? string.Format("{0:yyyy/MM/dd}",Model.ServiceSalesHeader.SalesDate) : "" ) %></td>
        <td><%=CommonUtils.DefaultNbsp(Model.Summary) %><%=Html.Hidden("Summary",Model.Summary) %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",Model.ReceivableBalance)) %><%=Html.Hidden("ReceivableBalance",Model.ReceivableBalance) %></td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th colspan="2">計上部門 *</th>
        <th colspan="2">入金事業所 *</th>    
    </tr>
    <tr>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = "1", maxlength = "3", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="計上部門" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <td style="width:250px"><span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"]) %></span></td>
        <td><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", size = "1", maxlength = "3", onblur = "GetNameFromCode('OfficeCode','OfficeName','Office')" })%>&nbsp;<img alt="入金事業所" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('OfficeCode','OfficeName','/Office/CriteriaDialog')" /></td>
        <td style="width:250px"><span id="OfficeName"><%=CommonUtils.DefaultNbsp(ViewData["OfficeName"]) %></span></td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th rowspan="2" style="width:15px"><img alt="行追加" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="document.forms[0].action='/ShopDeposit/AddLine';formSubmit();" /></th>
        <% //  Mod 2014/07/09 arc amii chrome対応 データ行とのボーダー位置を合わせる為、widthを変更 %>
        <th style="width:150px">入金日 *</th>
        <th style="width:255px">科目 *</th>
        <th colspan="2">請求先 *</th>
    </tr>
    <tr>
        <th>入金種別 *</th>
        <th>口座 * / 入金額 *</th>
        <% // Mod 2014/07/09 arc amii chrome対応 データ行とのボーダー位置を合わせる為、widthを変更 %>
        <th style="width:160px">決済種別</th>
        <% // Mod 2014/07/09 arc amii chrome対応 データ行とのボーダー位置を合わせる為、widthを変更 %>
        <th style="width:160px">摘要</th>
    </tr>
</table>
    <% // Mod 2014/07/09 arc amii chrome対応 データ行とのボーダー位置を合わせる為、widthをピクセル指定からパーセント指定に変更 %>
<div style="overflow-y:scroll;width:100%;height:250px">
<table class="input-form">
<%
    EntitySet<Journal> list = (EntitySet<Journal>)ViewData["JournalList"];
    for(int i=0;i<list.Count;i++){ 
    //2014/05/30 vs2012対応 arc yano 各コントロールにid追加
        string namePrefix = string.Format("line[{0}].", i);
        string idPrefix = string.Format("line[{0}]_", i);

%>
    <tr>
        <td rowspan="2" nowrap="nowrap" style="width:15px"><img alt="行削除" src="/Content/Images/minus.gif" style="cursor:pointer" onclick="document.forms[0].action='/ShopDeposit/DelLine/<%=i %>';formSubmit();" /></td>
        <% // Mod 2014/07/09 arc amii chrome対応 見出し行とのボーダー位置を合わせる為、widthを変更 %>
        <td nowrap="nowrap" style="width:150px"><%=Html.TextBox(namePrefix + "JournalDate", string.Format("{0:yyyy/MM/dd}", list[i].JournalDate), new { id = idPrefix + "JournalDate", @class = "alphanumeric", size = "10", maxlength = "10" })%></td>
        <% //Mod 2014/07/15 arc yano chrome対応 スクリプトのパラメータをname→idに変更 %>
        <td nowrap="nowrap" style="width:255px"><%=Html.TextBox(namePrefix + "AccountCode", list[i].AccountCode, new { id = idPrefix + "AccountCode", @class = "alphanumeric", size = "5", maxlength = "10", onchange = "GetNameFromCode('" + string.Format("line[{0}]_{1}", i, "AccountCode") + "','" + string.Format("line[{0}]_{1}", i, "AccountName" + "','Account')") })%>&nbsp;<img alt="科目検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=string.Format("line[{0}]_{1}",i,"AccountCode")%>','<%=string.Format("line[{0}]_{1}",i,"AccountName")%>','/Account/CriteriaDialog?journalType=001')" />&nbsp;<span id='<%=string.Format("line[{0}]_{1}",i,"AccountName")%>'><%=CommonUtils.DefaultNbsp(list[i].Account!=null ? list[i].Account.AccountName : "") %></span></td>
        <td nowrap="nowrap" colspan="2"><%=Html.TextBox(namePrefix + "CustomerClaimCode", list[i].CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetCustomerClaimWithClaimable('line[" + i + "]_CustomerClaimCode','line["+i+"]_CustomerClaimName','line["+i+"]_PaymentKindCode')" }) %>&nbsp;<img alt="請求先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="var callback = function(){GetCustomerClaimWithClaimable('line[<%=i%>]_CustomerClaimCode','line[<%=i%>]_CustomerClaimName','line[<%=i%>]_PaymentKindCode'); document.getElementById('line[<%=i %>]_PaymentKindCode').focus(); }; openSearchDialog('<%=string.Format("line[{0}]_{1}",i,"CustomerClaimCode")%>','<%=string.Format("line[{0}]_{1}",i,"CustomerClaimName") %>','/CustomerClaim/CriteriaDialog', null, null, null, null, callback); document.getElementById('line[<%=i %>]_PaymentKindCode').focus();" />&nbsp;<span id='<%=string.Format("line[{0}]_{1}",i,"CustomerClaimName") %>'><%=CommonUtils.DefaultNbsp(list[i].CustomerClaim!=null ? list[i].CustomerClaim.CustomerClaimName : "") %></span></td><%//Mod 2022/02/02 yano #4128%><%//Mod 2022/01/10 yano #4121%>
        <%--<td nowrap="nowrap" colspan="2"><%=Html.TextBox(namePrefix + "CustomerClaimCode", list[i].CustomerClaimCode, new { id = idPrefix + "CustomerClaimCode", @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetCustomerClaimWithClaimable('line[" + i + "]_CustomerClaimCode','line["+i+"]_CustomerClaimName','line["+i+"]_PaymentKindCode')" }) %>&nbsp;<img alt="請求先検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=string.Format("line[{0}]_{1}",i,"CustomerClaimCode")%>','<%=string.Format("line[{0}]_{1}",i,"CustomerClaimName") %>','/CustomerClaim/CriteriaDialog');document.getElementById('line[<%=i %>]_PaymentKindCode').focus()" />&nbsp;<span id='<%=string.Format("line[{0}]_{1}",i,"CustomerClaimName") %>'><%=CommonUtils.DefaultNbsp(list[i].CustomerClaim!=null ? list[i].CustomerClaim.CustomerClaimName : "") %></span></td>--%>
    </tr>
    <tr>
        <td><%=Html.DropDownList(namePrefix + "AccountType", ((List<IEnumerable<SelectListItem>>)ViewData["AccountTypeList"])[i], new{ id = idPrefix + "AccountType"}) %></td>
        <td><%=Html.DropDownList(namePrefix + "CashAccountCode", ((List<IEnumerable<SelectListItem>>)ViewData["CashAccountCodeList"])[i], new { id = idPrefix + "CashAccountCode"}) %>&nbsp;<%=Html.TextBox(namePrefix + "Amount", list[i].Amount, new { id = idPrefix + "Amount", @class = "numeric", size = "5", maxlength = "10",onchange="calcDepositDetail()" })%></td>          
        <% // Mod 2014/07/09 arc amii chrome対応 見出し行とのボーダー位置を合わせる為、widthを変更 %>
        <td style="white-space:nowrap;width:160px"><%=Html.DropDownList(namePrefix + "PaymentKindCode", ((List<IEnumerable<SelectListItem>>)ViewData["PaymentKindCodeList"])[i], new { id = idPrefix + "PaymentKindCode", style = "width:160px" })%></td>
        <% // Mod 2014/07/09 arc amii chrome対応 見出し行とのボーダー位置を合わせる為、widthを変更 %>
        <td style="white-space:nowrap;width:160px"><%=Html.TextBox(namePrefix + "Summary", list[i].Summary, new { id = idPrefix + "Summary", style = "width:150px", maxlength = "50" }) %></td>
    </tr>
   <%} %>
</table>
</div>
<table class="input-form">
    <tr>
        <th nowrap="nowrap" style="width:15px"></th>
        <th nowrap="nowrap" style="width:150px"></th>
        <th nowrap="nowrap" style="width:255px;text-align:right"><div style="font-weight:bold">合計&nbsp;&nbsp;<span id="TotalAmount"><%=string.Format("{0:N0}",list.Sum(x=>x.Amount)) %></span></div></th>
         <!-- Mod 2014/07/09 arc amii chrome対応 見出し行とのボーダー位置を合わせる為、widthを変更 -->
        <th nowrap="nowrap" style="width:327px"></th>
    </tr>
    <tr>
        <th></th>
        <th></th>
        <th style="text-align:right"><div style="font-weight:bold;color:Red">差額&nbsp;&nbsp;<span id="TotalBalance"><%=string.Format("{0:N0}",Model.ReceivableBalance - list.Sum(x=>x.Amount)) %></span></div></th>
        <th></th>
    </tr>
</table>
<%=Html.Hidden("LineCount",list.Count) %>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
