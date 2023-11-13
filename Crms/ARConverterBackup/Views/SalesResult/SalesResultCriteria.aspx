<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SalesResult>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	販売実績検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "SalesResult", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />

<%//Mod 2014/10/06 arc yano #3080_顧客検索機能の新設対応その２ 経理課の要望事項の反映 検索項目の追加(車両登録番号【種別、かな、プレート】、型式)%>
<table class="input-form">
    <tr>
        <th style="width:100px">顧客コード</th>
        <td>
            <%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog/')" />
        </td>
        <th>顧客名(漢字／カナ)</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = 50, maxlength = 50})%></td>
    </tr>
    <tr>
        <th>管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 15, maxlength = 50 })%></td>
         <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength =20 })%></td>
    </tr>
    <tr>
        <%//2014/10/29 arc yano #3080_顧客検索機能の新設対応その３ 検索条件「陸運局コード」の追加%> 
        <th>陸運局コード</th>
        <td><%=Html.TextBox("MorterViecleOfficialCode", ViewData["MorterViecleOfficialCode"], new { maxlength = 5 })%></td>
        <th>車両登録番号(種別)</th>
        <td><%=Html.TextBox("RegistrationNumberType", ViewData["RegistrationNumberType"], new { @class = "alphanumeric", size = 15, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>車両登録番号(かな)</th>
        <td><%=Html.TextBox("RegistrationNumberKana", ViewData["RegistrationNumberKana"], new { size = 20, maxlength =20 })%></td> 
        <th>車両登録番号(プレート)</th>
        <td><%=Html.TextBox("RegistrationNumberPlate", ViewData["RegistrationNumberPlate"], new { @class = "alphanumeric", size = 15, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th></th>
        <td></td>
        <th>型式</th>
        <td><%=Html.TextBox("ModelName", ViewData["ModelName"], new { size = 20, maxlength =20 })%></td>
    </tr>
    <tr>
        <th>車両伝票番号</th>
        <td><%=Html.TextBox("CarSlipNumber", ViewData["CarSlipNumber"], new { maxlength = 50 })%></td>
        <th>車両伝票納車日</th>
        <td><%=Html.TextBox("CarSalesDateFrom", ViewData["CarSalesDateFrom"], new { size = "8" })%> ～ <%=Html.TextBox("CarSalesDateTo", ViewData["CarSalesDateTo"], new { size = "8" })%></td>
    </tr>
        <tr>
        <th>サービス伝票番号</th>
        <td><%=Html.TextBox("ServiceSlipNumber", ViewData["ServiceSlipNumber"], new { maxlength = 50 })%></td>
        <th>サービス伝票納車日</th>
        <td><%=Html.TextBox("ServiceSalesDateFrom", ViewData["ServiceSalesDateFrom"], new { size = "8" })%> ～ <%=Html.TextBox("ServiceSalesDateTo", ViewData["ServiceSalesDateTo"], new { size = "8" })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan ="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('CustomerCode', 'CustomerName', 'SalesCarNumber', 'Vin', 'RegistrationNumberType', 'RegistrationNumberKana', 'RegistrationNumberPlate', 'ModelName', 'CarSlipNumber', 'CarSalesDateFrom', 'CarSalesDateTo', 'ServiceSlipNumber', 'ServiceSalesDateFrom', 'ServiceSalesDateTo','MorterViecleOfficialCode'))" />
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id ="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<%
  // Mod 2014/11/05 arc yano #3080_顧客検索機能の新設対応その３ 
  // ①ヘッダ列(顧客情報、車両情報、車両伝票情報、サービス伝票情報)を追加
  // ②一覧の登録番号(種別／かな／プレート)を一つの列に変更
 %>
<%//Mod 2014/10/06 arc yano #3080_顧客検索機能の新設対応その２ 経理課の要望事項の反映 一覧表の項目を追加(車両登録番号【種別、かな、プレート】、型式)%>
<table class="srlist" style="width:1310px">
     <tr>
        <th style="white-space:nowrap;"colspan="2">顧客情報</th>
        <th style="white-space:nowrap;"colspan="4">車両情報</th>
        <th style="white-space:nowrap;"colspan="2">車両伝票情報</th>
        <th style="white-space:nowrap;"colspan="2">サービス伝票情報</th>

    </tr>
    <tr>
        <th style="white-space:nowrap;width:70px">顧客コード</th>
        <th style="white-space:nowrap;width:350px">顧客名</th>
        <th style="white-space:nowrap;width:100px">管理番号</th>
        <th style="white-space:nowrap;width:140px">車台番号</th>
        <th style="white-space:nowrap;width:120px">車両登録番号</th>
        <th style="white-space:nowrap;width:120px">型式</th>
        <th style="white-space:nowrap;width:100px">伝票番号</th>
        <th style="white-space:nowrap;width:105px">納車日</th>
        <th style="white-space:nowrap;width:100px">伝票番号</th>
        <th style="white-space:nowrap;width:105px">納車日</th>
    </tr>
    <%
    string customerCode = null;
    string salesCarNumber = null;
    string vin = null;
    string carSlipNumber = null;
    int customerflg = 0;
    int scarnumberflg = 0;
    int vinflg = 0;
    int carslipflg = 0;
    foreach (var sresults in Model){
    %>
    <tr>
        <%  
            //Mod 2014/11/04 arc yano #3080_顧客検索機能の新設対応その３ 顧客コードがnullの場合は、スペース" "に変換する。)
        
            if (string.IsNullOrEmpty(sresults.CustomerCode))
            {
                sresults.CustomerCode = " ";
            }
            
            //顧客コード
            if (string.IsNullOrEmpty(customerCode) || !customerCode.Equals(sresults.CustomerCode)){
              customerCode = sresults.CustomerCode;
              customerflg = 1;
         %>
         <td style="white-space:nowrap;border-bottom:none"><a href="javascript:openModalAfterRefresh('/Customer/IntegrateEntry/' + '<%=sresults.CustomerCode%>')"><%=Html.Encode(sresults.CustomerCode) %></a></td>
        <%
        }else{
            customerflg = 0;
        %>
        <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <%} %>
        
        <% //顧客名称
         if(customerflg == 1){
        %>
        <td style="white-space:nowrap;border-bottom:none"><%=Html.Encode(sresults.CustomerName)%></td>
        <%}else{ %>
        <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <%} %>
        
        <% //管理番号
        if ((string.IsNullOrEmpty(salesCarNumber) || !salesCarNumber.Equals(sresults.SalesCarNumber)) || customerflg == 1){
            salesCarNumber = sresults.SalesCarNumber;
            scarnumberflg = 1;
        %>
        <td style="white-space:nowrap;border-bottom:none"><a href="javascript:openModalAfterRefresh('/SalesCar/Entry/' + '<%=sresults.SalesCarNumber%>?Master=1')"><%=Html.Encode(sresults.SalesCarNumber)%></a></td>
        <%}else{
            scarnumberflg = 0;
        %>
        <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <%} %>

        <% //Mod 2014/11/04 arc yano #3080_顧客検索機能の新設対応その３ 車台番号の表示条件を変更する。(管理番号が異なっている場合は、必ず表示する)) %>
        <% //車台番号
        if ((string.IsNullOrEmpty(vin) || !vin.Equals(sresults.Vin)) || scarnumberflg == 1) {
            vin = sresults.Vin;
            vinflg = 1;
        %>
        <td style="white-space:nowrap;border-bottom:none"><%=Html.Encode(sresults.Vin)%></td>
        <%}else{ 
             vinflg = 0;
        %>
         <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <%} %>
        <% //Mod 2014/10/29 arc yano #3080_顧客検索機能の新設対応その３ 車両登録番号(陸運局コード、種別、かな、プレート)を一つにまとめる) %>
        <% //車両登録番号(陸運局コード＋種別＋かな＋プレート) %>
        <% if(vinflg == 1) { %>
            <td style="white-space:nowrap;border-bottom:none"><%=Html.Encode(sresults.MorterViecleOfficialCode + " " + sresults.RegistrationNumberType + " " + sresults.RegistrationNumberKana + " " + sresults.RegistrationNumberPlate)%></td>
        <% }else{ %>
            <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <% } %>

        <% //型式 %>
        <% if(vinflg == 1) { %>
            <td style="white-space:nowrap;border-bottom:none"><%=Html.Encode(sresults.ModelName)%></td>
        <% }else{ %>
            <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <% } %>

        <% //車両伝票番号
        if ((string.IsNullOrEmpty(carSlipNumber) || !carSlipNumber.Equals(sresults.CarSlipNumber)) || vinflg == 1) {
             carSlipNumber = sresults.CarSlipNumber;
             carslipflg = 1;
        %>
        <td style="white-space:nowrap;border-bottom:none"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarSalesOrder/Entry?SlipNo=<%=sresults.CarSlipNumber%>&RevNo=<%=sresults.CarRevisionNumber%>')"><%=Html.Encode(sresults.CarSlipNumber)%></a></td>
        <%}else{
            carslipflg = 0;
        %>
        <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
         <%} %>
      
        <%　//車両伝票・受注日
        if (carslipflg == 1){ %>
        <td style="white-space:nowrap;border-bottom:none"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",sresults.CarSalesDate))%></td>
        <%}else{ %>
        <td style="white-space:nowrap; border-top:none; border-bottom:none"></td>
        <%} %>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/ServiceSalesOrder/Entry/?SlipNo=<%=sresults.ServiceSlipNumber%>&RevNo=<%=sresults.ServiceRevisionNumber %>')"><%=Html.Encode(sresults.ServiceSlipNumber)%></a></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",sresults.ServiceSalesDate))%></td>
    </tr>
    <%}%>
</table>
<br />
</asp:Content>