<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CustomerReceiption>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス受付入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "ServiceReceiption", FormMethod.Post))
  { %>
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <td onclick="document.forms[0].action.value = 'saveQuote';formSubmit()"><img src="/Content/Images/build.png" alt="見積作成" class="command_btn" />&nbsp;見積作成</td>
        <td onclick="document.forms[0].PrintReport.value ='ServiceReceiption';formSubmit()"><img src="/Content/Images/pdf.png" alt="承り書" class="command_btn" />承り書</td>
    </tr>
</table>
<br />
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %>
<%=Html.Hidden("url",ViewData["url"]) %>
<%=Html.Hidden("reportName",ViewData["reportName"]) %>
<%=Html.Hidden("reportParam", Model.CarReceiptionId)%>
<%=Html.Hidden("PrintReport","") %>
<%=Html.Hidden("CarReceiptionId",Model.CarReceiptionId) %>
<div id="input-form">
<%=Html.ValidationSummary()%>
<br />
<%if(ViewData["ErrorSalesCar"]!=null){ %>
    <%Html.RenderPartial("VinErrorControl", ViewData["ErrorSalesCar"]); %>
<%} %>
<table class="input-form">
    <tr>
        <th colspan="4" class="input-form-title">顧客情報</th>
    </tr>
    <tr>
        <th style="width:150px">顧客コード *</th>
        <td style="width:220px">
            <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric", maxlength = 10, style="width:100px", onblur = "GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" })%> 
            <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerCode", "CustomerName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" });%>
            <%
                //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 
                //①マスタボタンをクリック時に顧客コードが入っていない場合は、ポップアップメッセージを表示する
                //②id追加
                //③詳細情報を取得中に非活性にする項目を追加 
            %>
            <button type="button" id="MasterForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/Customer/IntegrateEntry/'+$('#CustomerCode').val()) : openModalDialog('/Customer/IntegrateEntry/'); return false;;GetMasterDetailFromCode('CustomerCode',new Array('CustomerRankName','CustomerName','CustomerNameKana','Prefecture','City','Address1','Address2'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" >マスタ</button>
        </td>
        <th style="width:150px">顧客ランク</th>
        <td style="width:220px"><%=Html.TextBox("CustomerRankName", ViewData["CustomerRankName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th style="height:20px">顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
        <th>顧客名(カナ)</th>
        <td><%=Html.TextBox("CustomerNameKana", ViewData["CustomerNameKana"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th style="height:20px">都道府県</th>
        <td><%=Html.TextBox("Prefecture", ViewData["Prefecture"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
        <th>市区町村</th>
        <td><%=Html.TextBox("City", ViewData["City"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th style="height:20px">住所１</th>
        <td colspan="3"><%=Html.TextBox("Address1", ViewData["Address1"], new { @class = "readonly", @readonly = "readonly", style = "width:550px" })%></td>
    </tr>
    <tr>
        <th style="height:20px">住所２</th>
        <td colspan="3"><%=Html.TextBox("Address2", ViewData["Address2"], new { @class = "readonly", @readonly = "readonly", style = "width:550px" })%></td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th colspan="4" class="input-form-title">受付内容</th>
    </tr>
    <tr>
        <th style="width:150px">受付日 *</th>
        <td style="width:220px">
            <%=Html.TextBox("ReceiptionDate", string.Format("{0:yyyy/MM/dd}", Model.ReceiptionDate), new { @class = "alphanumeric", maxlength = 10, size=8 })%>
            <% //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) %>
            <button type="button" id="HistoryForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/ServiceSalesOrder/CriteriaDialog?customerCode='+$('#CustomerCode').val()) : false;">履歴</button>
        </td>
        <th style="width:150px" rowspan="2">部門 *</th>
        <td style="width:220px">
            <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
        </td>
    </tr>
    <tr>
        <th>受付状況</th>
        <td><%=Html.DropDownList("ReceiptionState", (IEnumerable<SelectListItem>)ViewData["ReceiptionStateList"])%></td>
        <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th>来店種別</th>
        <td><%=Html.DropDownList("ReceiptionType", (IEnumerable<SelectListItem>)ViewData["ReceiptionTypeList"])%></td>
        <th rowspan="2">担当者 *</th>
        <td>
            <%=Html.TextBox("EmployeeNumber", ViewData["EmployeeNumber"], new { @class = "alphanumeric", style = "width:50px", maxlength = 10, onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:120px", maxlength = 50, onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
            <img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() {GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee'); document.getElementById('EmployeeName').focus()}; openSearchDialog('EmployeeCode', 'EmployeeName', '/Employee/CriteriaDialog/?DepartmentCode=<%=((Employee)Session["Employee"]).DepartmentCode%>', null, null, null, null, callback);document.getElementById('EmployeeName').focus()" /><%//Mod 2022/02/02 yano #4128%><%//Mod 2022/01/10 yano #4121%>
            <%--<img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode', 'EmployeeName', '/Employee/CriteriaDialog/?DepartmentCode=<%=((Employee)Session["Employee"]).DepartmentCode%>');document.getElementById('EmployeeName').focus()" />--%>
        </td>
    </tr>
    <tr>
        <th>来店きっかけ</th>
        <td><%=Html.DropDownList("VisitOpportunity", (IEnumerable<SelectListItem>)ViewData["VisitOpportunityList"])%></td>
        <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th>依頼事項</th>
        <td><%=Html.DropDownList("RequestContent", (IEnumerable<SelectListItem>)ViewData["RequestContentList"])%></td>
        <th>入庫日</th>
        <td><%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>依頼内容</th>
        <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
        <td colspan="3"><%=Html.TextArea("RequestDetail", Model.RequestDetail, 3, 70, new { wrap = "virtual", onblur = "checkTextLength('RequestDetail', 200, '依頼内容')" })%></td>
    </tr>
    <tr>
        <th>イベント１</th>
        <td colspan="3"><%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "alphanumeric", maxlength = 20, onblur = "GetNameFromCode('CampaignCode1','CampaignName1','Campaign')" })%>
            <img alt="イベント検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CampaignCode1', 'CampaignName1', '/Campaign/CriteriaDialog')" />
            <%=Html.TextBox("CampaignName1", ViewData["CampaignName1"], new { @class = "readonly", @readonly = "readonly", style = "width:415px" })%>
        </td>
    </tr>
    <tr>
        <th>イベント２</th>
        <td colspan="3"><%=Html.TextBox("CampaignCode2", Model.CampaignCode2, new { @class = "alphanumeric", maxlength = 20, onblur = "GetNameFromCode('CampaignCode2','CampaignName2','Campaign')" })%>
            <img alt="イベント検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CampaignCode2', 'CampaignName2', '/Campaign/CriteriaDialog')" />
            <%=Html.TextBox("CampaignName2", ViewData["CampaignName2"], new { @class = "readonly", @readonly = "readonly", style = "width:415px" })%>
        </td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th colspan="4" class="input-form-title">車両情報</th>
    </tr>
    <tr>
        <th style="width:150px">車台番号</th>
        <td style="width:220px">
            <%// Mod 2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える GetSalesCarInfoの引数変更につき、呼び出し元も変更する%>
            <%//Mod 2015/09/10 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 車台番号をルックアップ、または手入力した際に車両情報を設定するように対応 %>
            <%=Html.TextBox("Vin", Model.Vin, new { maxlength = 20, style="width:130px", onchange = "GetSalesCarInfo('Vin','SalesCarNumber', new Array('Mileage', 'MileageUnit', 'MorterViecleOfficialCode', 'CarGradeCode', 'RegistrationNumberType', 'MakerName', 'RegistrationNumberKana', 'CarName', 'RegistrationNumberPlate', 'CarGradeName', 'FirstRegistration', 'RegistrationDate'), null, null, new Array('MasterForSalesCar'));"})%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "GetMasterDetailFromCode('SalesCarNumber',new Array('Mileage', 'MileageUnit', 'MorterViecleOfficialCode', 'CarGradeCode', 'RegistrationNumberType', 'MakerName', 'RegistrationNumberKana', 'CarName', 'RegistrationNumberPlate', 'CarGradeName', 'FirstRegistration', 'RegistrationDate'),'SalesCar', null, new Array('MasterForSalesCar'));" }); %>
            <%//Mod 2015/09/11 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 
              //①マスタボタンをクリック時に車台番号が入っていない場合は、車両マスタの新規登録画面を表示する
              //②id追加
            %>
            <button type="button" id="MasterForSalesCar" style="width:60px;height:20px" onclick="$('#SalesCarNumber').val()!='' ? openModalDialog('/SalesCar/Entry/'+$('#SalesCarNumber').val()) : openModalDialog('/SalesCar/Entry/'); return false">マスタ</button>
        </td>
        <th style="width:150px">走行距離</th>
        <td style="width:220px"><%=Html.TextBox("Mileage", Model.Mileage, new { @class = "numeric", maxlength = 13 })%>&nbsp;<%=Html.DropDownList("MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"])%></td>
    </tr>
    <tr>
        <th>陸運局コード</th>
        <td><%=Html.TextBox("MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { maxlength = 5 })%></td>
        <th>グレードコード</th>
        <td><%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", maxlength = 30, onblur = "GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarName','CarGradeName'),'CarGrade')" })%>
            <img alt="グレード検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() { GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarName','CarGradeName'),'CarGrade')}; openSearchDialog('CarGradeCode','','/CarGrade/CriteriaDialog', null, null, null, null, callback); GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarName','CarGradeName'),'CarGrade');" /><%//Mod 2022/01/10 yano #4121%>
            <%--<img alt="グレード検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarGradeCode','','/CarGrade/CriteriaDialog');GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarName','CarGradeName'),'CarGrade');" />--%>
        </td>
    </tr>
    <tr>
        <th>登録番号(種別)</th>
        <td><%=Html.TextBox("RegistrationNumberType", Model.RegistrationNumberType, new { @class = "alphanumeric", maxlength = 3 })%></td>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th>登録番号(かな)</th>
        <td><%=Html.TextBox("RegistrationNumberKana", Model.RegistrationNumberKana, new { maxlength = 1 })%></td>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <th>登録番号(プレート)</th>
        <td><%=Html.TextBox("RegistrationNumberPlate", Model.RegistrationNumberPlate, new { @class = "alphanumeric", maxlength = 4 })%></td>
        <th>グレード名</th>
        <td><%=Html.TextBox("CarGradeName", ViewData["CarGradeName"], new { @class = "readonly", @readonly = "readonly", style = "width:210px" })%></td>
    </tr>
    <tr>
        <%// Mod 2015/09/10 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 初年度登録のidを変更(FirstRegistrationYear → FirstRegistration ※SalesCarのAjaxのデータ名に合わせる) %>
        <th>初度登録年月</th>
        <td><%=Html.TextBox("FirstRegistrationYear", Model.FirstRegistrationYear, new { id = "FirstRegistration", @class = "alphanumeric", maxlength = 7, size = 10 }) %></td>
        <th>登録年月日</th>
        <td><%=Html.TextBox("RegistrationDate",string.Format("{0:yyyy/MM/dd}",Model.RegistrationDate),new {@class="alphanumeric",maxlength="10",size="10"}) %></td>
    </tr>
</table>
</div>
<%} %>
<br />
</asp:Content>
