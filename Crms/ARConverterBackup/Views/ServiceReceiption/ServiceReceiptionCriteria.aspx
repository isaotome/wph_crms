<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_ServiceReceiptTarget>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス受付検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "ServiceReceiption", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">名前(カナ)</th>
        <td><%=Html.TextBox("CustomerNameKana", ViewData["CustomerNameKana"], new { maxlength = 40 })%></td>
        <th style="width:100px">名前(漢字)</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>陸運局コード</th>
        <td><%=Html.TextBox("MorterViecleOfficialCode", ViewData["MorterViecleOfficialCode"], new { maxlength = 5 })%></td>
        <th>車両登録番号(種別)</th>
        <td><%=Html.TextBox("RegistrationNumberType", ViewData["RegistrationNumberType"], new { maxlength = 3 })%></td>
    </tr>
    <tr>
        <th>車両登録番号(かな)</th>
        <td><%=Html.TextBox("RegistrationNumberKana", ViewData["RegistrationNumberKana"], new { maxlength = 1 })%></td>
        <th>車両登録番号(プレート)</th>
        <td><%=Html.TextBox("RegistrationNumberPlate", ViewData["RegistrationNumberPlate"], new { maxlength = 4 })%></td>
    </tr>
    <tr>
        <th>電話番号(下4桁)</th>
        <td><%=Html.TextBox("TelNumber", ViewData["TelNumber"], new { @class = "alphanumeric", maxlength = 4 })%></td>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { maxlength = 20 })%></td>
    </tr>
    <tr>
        <th></th>
        <td></td>
        <th>型式</th>
        <td><%=Html.TextBox("ModelName", ViewData["ModelName"], new { @class = "alphanumeric", maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>初回来店日</th>
        <td colspan="3"><%=Html.TextBox("FirstReceiptionDateFrom", ViewData["FirstReceiptionDateFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("FirstReceiptionDateTo", ViewData["FirstReceiptionDateTo"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>前回来店日</th>
        <td colspan="3"><%=Html.TextBox("LastReceiptionDateFrom", ViewData["LastReceiptionDateFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("LastReceiptionDateTo", ViewData["LastReceiptionDateTo"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('CustomerName','CustomerNameKana','TelNumber','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','Vin','ModelName','FirstReceiptionDateFrom','FirstReceiptionDateTo','LastReceiptionDateFrom','LastReceiptionDateTo'))" />
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
    <input type="button" value="新規顧客登録" onclick="openModalAfterRefresh('/Customer/IntegrateEntry')" />
    <input type="button" value="新規車両登録" onclick="openModalAfterRefresh('/SalesCar/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="width:30px"></th>
        <th style="white-space:nowrap">名前</th>
        <th style="white-space:nowrap">メーカー</th>
        <th style="white-space:nowrap">車種</th>
        <th style="white-space:nowrap">グレード</th>
        <th style="white-space:nowrap">登録番号</th>
        <th style="white-space:nowrap">車台番号</th>
        <th style="white-space:nowrap">住所</th>
        <th style="white-space:nowrap">電話番号</th>
        <th style="white-space:nowrap">フロント担当者</th>
    </tr>
    <%foreach (var target in Model)
      { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/ServiceReceiption/Entry/' + '<%=target.CustomerCode %>' + ',' + encodeURIComponent('<%=target.SalesCarNumber %>'));return false;">受付</a></td>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/ServiceReceiption/History/?customerCode=<%=target.CustomerCode %>&salesCarNumber=<%=target.SalesCarNumber %>');return false;">履歴</a></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.CustomerName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.MakerName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.CarName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.CarGradeName)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.RegistrationNumberPlate)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.Vin)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.CustomerAddress)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.TelNumber)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(target.EmployeeName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
