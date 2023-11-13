<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CustomerDataResult>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    顧客データリスト検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CustomerDataList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:CSV出力 / それ以外:クリア)%>
<%=Html.Hidden("DefaultDmFlag", ViewData["DefaultDmFlag"]) %><%//DM可否リスト (可 / 不可)%>
<%=Html.Hidden("DefaultPrivateFlag", ViewData["DefaultPrivateFlag"]) %><%//自社取扱いフラグ%>
<%=Html.Hidden("DefaultCustomerAddressReconfirm", ViewData["DefaultCustomerAddressReconfirm"]) %><%//住所再確認 (要 / 不要)%>
<%=Html.Hidden("DefaultCustomerKind", ViewData["DefaultCustomerKind"]) %><%//顧客種別 (未納客/既納客/サービス顧客)%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
<table class="input-form">
    <%//Mod 2015/01/07 arc nakayama 顧客DM指摘事項⑩　備考のコメントアウト%>
    <%//Add 2015/01/08 arc nakayama 顧客DM指摘事項⑨　検索項目の追加（車種名・メーカー名・納車日(営業/サービス)・担当者(営業/サービス)）%>
    <tr>
        <th>営業DM可否</th>
        <td colspan="3"><%=Html.DropDownList("DmFlag", (IEnumerable<SelectListItem>)ViewData["DmFlagList"])%></td>
        <th>顧客種別</th>
        <td colspan="3"><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%></td>
        <%//<th>備考＜営業＞</th> %>
        <%//<td><%=Html.TextBox("DmMemo", ViewData["DmMemo"], new { size = 50, maxlength = 50 })</td> %>
    </tr>
    <tr>
        <%//Mod 2015/02/16 arc nakayama 顧客DM仕様変更　部門名は表示のみにする %>
        <th title="顧客マスタに設定されている営業担当部門コードです">営業担当部門コード</th>
        <td colspan="3"><%=Html.TextBox("SalesDepartmentCode2", ViewData["SalesDepartmentCode2"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('SalesDepartmentCode2','SalesDepartmentName2','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesDepartmentCode2','SalesDepartmentName2','/Department/CriteriaDialog')" /><span id="SalesDepartmentName2" style ="width:160px"><%=Html.Encode(ViewData["SalesDepartmentName2"]) %></span></td>        
        <th>車両伝票納車日</th>
        <td colspan="3"><%=Html.TextBox("SalesDateFrom",ViewData["SalesDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('SalesDateFrom').value, 'SalesDateFrom')" }) %>～<%=Html.TextBox("SalesDateTo", ViewData["SalesDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 ,onchange ="return chkDate3(document.getElementById('SalesDateTo').value, 'SalesDateTo')"}) %></td>
    </tr>
    <tr>
        <th title="顧客マスタに設定されているサービス担当部門コードです">サービス担当部門コード</th>
        <td colspan="3"><%=Html.TextBox("ServiceDepartmentCode2", ViewData["ServiceDepartmentCode2"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('ServiceDepartmentCode2','ServiceDepartmentName2','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceDepartmentCode2','ServiceDepartmentName2','/Department/CriteriaDialog')" /><span id="ServiceDepartmentName2" style ="width:160px"><%=Html.Encode(ViewData["ServiceDepartmentName2"]) %></span></td>        
        <th>サービス伝票入庫日</th>
        <td><%=Html.TextBox("ArrivalPlanDateFrom",ViewData["ArrivalPlanDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('ArrivalPlanDateFrom').value, 'ArrivalPlanDateFrom')" }) %>～<%=Html.TextBox("ArrivalPlanDateTo",ViewData["ArrivalPlanDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('ArrivalPlanDateTo').value, 'ArrivalPlanDateTo')" }) %></td>
    </tr>
    <tr>
        <%//Mod 2016/02/02 ARC Mikami #3277_営業担当者Delflag検索対応 %>
        <th title="顧客マスタに設定されている営業担当者の社員コードです">営業部門担当者コード</th>
        <td colspan="3"><%=Html.TextBox("CarEmployeeCode", ViewData["CarEmployeeCode"], new { onblur = "GetNameFromCodeDelflagNoCheck('CarEmployeeCode','CarEmployeeName','Employee')"})%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarEmployeeCode','CarEmployeeName','/Employee/CriteriaDialog2')" /><span id="CarEmployeeName" style ="width:160px"><%=Html.Encode(ViewData["CarEmployeeName"]) %></span></td>
        <th>住所再確認</th>
        <td colspan="3"><%=Html.DropDownList("CustomerAddressReconfirm", (IEnumerable<SelectListItem>)ViewData["CustomerAddressReconfirmList"])%></td>
    </tr>
    <tr>
        <th>自社取扱いのみ</th>
        <td style="width:90px;"><%=Html.CheckBox("PrivateFlag", ViewData["PrivateFlag"] != null && ViewData["PrivateFlag"].ToString().Equals("true"), new { onclick="CheckPrivateFlag()" })%></td>           
        <th>メーカー名</th>
        <td><%=Html.DropDownList("MakerCode", (IEnumerable<SelectListItem>)ViewData["MakerList"] , new { Style= "width:190px;", onchange="GetCarMasterList()" }) %></td>
        <th>車種名</th>
        <td style="width:300px;"><%=Html.DropDownList("CarCode", (IEnumerable<SelectListItem>)ViewData["CarList"], new { Style= "width:202px;" })%></td>
    </tr>
    <tr>
        <th>初年度登録(YYYY/MM)</th>
        <td colspan="3"><%=Html.TextBox("FirstRegistrationFrom", ViewData["FirstRegistrationFrom"], "{0:yyyy/MM}" ,new { @class = "alphanumeric", size = 7, maxlength = 7, onchange = "if(document.getElementById('FirstRegistrationFrom').value.length > 7 ){this.value = this.value.substr(0, this.value.length - 3)};return chkDateYYMM(document.getElementById('FirstRegistrationFrom').value, 'FirstRegistrationFrom')" })%>～<%=Html.TextBox("FirstRegistrationTo", ViewData["FirstRegistrationTo"], "{0:yyyy/MM}", new { @class = "alphanumeric", size = 7, maxlength = 7, onchange = "if(document.getElementById('FirstRegistrationTo').value.length > 7 ){this.value = this.value.substr(0, this.value.length - 3)};return chkDateYYMM(document.getElementById('FirstRegistrationTo').value, 'FirstRegistrationTo')"})%></td>
        <th>登録年月日</th>
        <td><%=Html.TextBox("RegistrationDateFrom",ViewData["RegistrationDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('RegistrationDateFrom').value, 'RegistrationDateFrom')" })%>～<%=Html.TextBox("RegistrationDateTo",ViewData["RegistrationDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('RegistrationDateTo').value, 'RegistrationDateTo')" }) %></td>
    </tr>
    <%//Add 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加%>
    <tr>
        <th>住所(都道府県市区町村)</th>
        <td colspan="3"><%=Html.TextBox("CustomerAddress", ViewData["CustomerAddress"], new { size = 30, maxlength = 100})%></td>
        <th colspan ="2"></th>
    </tr>
    <tr>
        <th></th>
        <td colspan="5">※車検DMの発送先を検索する場合は、車検点検リストを利用してください。</td>     
    </tr>

    <tr>
        <th></th>
        <td colspan ="6">
            <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DmFlag', 'SalesDateFrom', 'SalesDateTo', 'ArrivalPlanDateFrom', 'ArrivalPlanDateTo', 'SalesDepartmentCode2', 'ServiceDepartmentCode2', 'SalesDepartmentName2', 'ServiceDepartmentName2', 'CarEmployeeName', 'CarEmployeeCode', 'MakerCode', 'CarCode', 'PrivateFlag', 'FirstRegistrationFrom', 'FirstRegistrationTo', 'RegistrationDateFrom', 'RegistrationDateTo', 'CustomerAddressReconfirm', 'CustomerKind')); GetMakerMasterList('1'); GetPrivateCarList('1');"/>
            <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CustomerDataList', 'UpdateMsg'); document.forms[0].submit();"/>
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
        <th style="white-space:nowrap;"colspan="2">DM可否</th>
        <th style="white-space:nowrap;"colspan="7">顧客情報</th>
        <th style="white-space:nowrap;"colspan="7">顧客住所</th>
        <th style="white-space:nowrap;"colspan="5">DM発送先住所</th>
        <th style="white-space:nowrap;"colspan="10">車両情報</th>
        <th style="white-space:nowrap;"colspan="3">車両伝票</th>
        <th style="white-space:nowrap;"colspan="3">サービス伝票</th>
    </tr>
    <tr>
        <th style="white-space:nowrap">営業DM可否</th>          <%//DM可否%>
        <th style="white-space:nowrap">営業DM発送備考</th>  <%//DM可否%>
        <th style="white-space:nowrap">顧客コード</th>    <%//顧客情報%>
        <th style="white-space:nowrap">顧客種別</th>      <%//顧客情報%>
        <th style="white-space:nowrap">顧客名</th>      <%//顧客情報%>
        <th style="white-space:nowrap">営業担当部門名</th><%//顧客情報%>
        <th style="white-space:nowrap">営業担当者</th><%//顧客情報%>
        <th style="white-space:nowrap">サービス担当部門名</th><%//顧客情報%>
        <th style="white-space:nowrap">サービス担当者</th><%//顧客情報%>
        <th style="white-space:nowrap">郵便番号</th>      <%//顧客住所%>
        <th style="white-space:nowrap">都道府県</th>      <%//顧客住所%>
        <th style="white-space:nowrap">住所</th>          <%//顧客住所%>
        <th style="white-space:nowrap">電話番号</th>      <%//顧客住所%>
        <th style="white-space:nowrap">携帯電話番号</th>  <%//顧客住所%>
        <th style="white-space:nowrap">メールアドレス</th><%//顧客住所%><%//Add 2015/12/16 arc ookubo #3318_メールアドレスを追加%>
        <th style="white-space:nowrap">住所再確認</th>    <%//顧客住所%>
        <th style="white-space:nowrap">顧客名</th>        <%//DM発送先住所%><%//Add 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名%>
        <th style="white-space:nowrap">郵便番号</th>      <%//DM発送先住所%>
        <th style="white-space:nowrap">都道府県</th>      <%//DM発送先住所%>
        <th style="white-space:nowrap">住所</th>          <%//DM発送先住所%>
        <th style="white-space:nowrap">電話番号</th>      <%//DM発送先住所%>
        <th style="white-space:nowrap">管理番号</th>      <%//車両情報%>
        <th style="white-space:nowrap">車両登録番号</th><%//車両情報%>
        <th style="white-space:nowrap">車台番号</th>      <%//車両情報%>
        <th style="white-space:nowrap">メーカー</th>      <%//車両情報%>
        <th style="white-space:nowrap">車種名</th>    　<%//車両情報%>
        <th style="white-space:nowrap">初年度登録</th>    <%//車両情報%>
        <th style="white-space:nowrap">登録年月日</th>    <%//車両情報%>
        <th style="white-space:nowrap">次回点検日</th>    <%//車両情報%>
        <th style="white-space:nowrap">車検満了日</th>    <%//車両情報%>
        <th style="white-space:nowrap">車両重量(kg)</th>    <%//車両情報%><% //Add 2015/05/20 arc nakayama #3199_顧客データリスト出力項目追加%>
        <th style="white-space:nowrap">部門名</th>          <%//営業担当%>
        <th style="white-space:nowrap">担当者</th>        <%//営業担当%>
        <th style="white-space:nowrap">納車年月日</th>    <%//営業担当%>
        <th style="white-space:nowrap">部門名</th>          <%//サービス担当%>
        <th style="white-space:nowrap">担当者</th>        <%//サービス担当%>
        <th style="white-space:nowrap">入庫日</th>    <%//サービス担当%>
    </tr>
    <%foreach (var CustomerData in Model)
      {%>
    <tr>
        <td style="text-align:center"><%=Html.Encode(CustomerData.DmFlag.Equals("001") ? "可" : CustomerData.DmFlag.Equals("002") ? "不可" : CustomerData.DmFlag.Equals("009") ? "他" : "" )%></td><%//営業(DM可否)%>
        <td><%=Html.Encode(CustomerData.DmMemo)%></td><%//備考(DM可否)%>
        <td><%=Html.Encode(CustomerData.CustomerCode)%></td><%//顧客コード(顧客情報)%>
        <td style="text-align:center"><%=Html.Encode(CustomerData.CustomerKind.Equals("001") ? "未納客" : CustomerData.CustomerKind.Equals("002") ? "既納客" : CustomerData.CustomerKind.Equals("003") ? "サービス顧客" : "" )%></td><%//顧客種別(顧客情報)%>
        <td><%=Html.Encode(CustomerData.CustomerName)%></td><%//顧客名称(顧客情報)%>
        <td><%=Html.Encode(CustomerData.DepartmentName2)%></td><%//営業担当部門名(顧客情報)%>
        <td><%=Html.Encode(CustomerData.CarEmployeeName)%></td><%//営業担当者(顧客情報)%>
        <td><%=Html.Encode(CustomerData.ServiceDepartmentName2)%></td><%//サービス担当部門名(顧客情報)%>
        <td><%=Html.Encode(CustomerData.ServiceEmployeeName2)%></td><%//サービス担当者名(顧客情報)%>
        <td><%=Html.Encode(CustomerData.CustomerPostCode)%></td><%//郵便番号(顧客住所)%>
        <td><%=Html.Encode(CustomerData.CustomerPrefecture)%></td><%//都道府県(顧客住所)%>
        <td><%=Html.Encode(CustomerData.CustomerAddress)%></td><%//住所(顧客住所)%>
        <td><%=Html.Encode(CustomerData.CustomerTelNumber)%></td><%//電話番号(顧客住所)%>
        <td><%=Html.Encode(CustomerData.MobileNumber)%></td><%//携帯電話番号%>
        <td><%=Html.Encode(CustomerData.MailAddress)%></td><%//メールアドレス%><%//Add 2015/12/16 arc ookubo #3318_メールアドレスを追加%>
        <td style="text-align:center"><%=Html.Encode(CustomerData.CustomerAddressReconfirmName)%></td><%//住所再確認(顧客住所)%>
        <td><%=Html.Encode(CustomerData.DmCustomerName)%></td><%//顧客名称(DM発送先住所)%><%//Add 2015/07/01 arc nakayama #3220_顧客データリスト出力項目追加 DM発送先の顧客名%>
        <td><%=Html.Encode(CustomerData.DmPostCode)%></td><%//郵便番号(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmPrefecture)%></td><%//都道府県(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmCustomerDmAddress)%></td><%//住所(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmTelNumber)%></td><%//電話番号(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.SalesCarNumber)%></td><%//管理番号(車両情報)%>
        <td><%=Html.Encode(CustomerData.MorterViecleOfficialCode)%></td><%//陸運事務局名称(車両情報)%>
        <td><%=Html.Encode(CustomerData.Vin)%></td><%//車台番号(車両情報)%>
        <td><%=Html.Encode(CustomerData.MakerName)%></td><%//メーカー(車両情報)%>
        <td><%=Html.Encode(CustomerData.CarName)%></td><%//車種名称(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM}",CustomerData.FirstRegistrationDate))%></td><%//初年度登録(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.RegistrationDate))%></td><%//登録年月日(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.NextInspectionDate))%></td><%//次回点検日(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.ExpireDate))%></td><%//車検満了日(車両情報)%>
        <td style="text-align:right"><%=Html.Encode(CustomerData.CarWeight)%></td><%//車両重量(車両情報)%>
        <td><%=Html.Encode(CustomerData.SalesDepartmentName)%></td><%//部門名(営業担当)%>
        <td><%=Html.Encode(CustomerData.SalesEmployeeName)%></td><%//担当者名(営業担当)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.SalesDate))%></td><%//納車年月日(営業担当)%>
        <td><%=Html.Encode(CustomerData.ServiceDepartmentName)%></td><%//部門名(サービス担当)%>
        <td><%=Html.Encode(CustomerData.ServiceEmployeeName)%></td><%//担当者名(サービス担当)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.ArrivalPlanDate))%></td><%//納車日(サービス担当)%>
    </tr>
    <%}%>
</table>

</asp:Content>