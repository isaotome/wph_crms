<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CustomerDataResult>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車検点検リスト
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "InspectGuidList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:CSV出力 / それ以外:クリア)%>
<%=Html.Hidden("DefaultDmFlag", ViewData["DefaultDmFlag"]) %><%//DM可否リスト (可 / 不可)%>
<%=Html.Hidden("DefaultPrivateFlag", ViewData["DefaultPrivateFlag"]) %><%//自社取扱いフラグ%>
<%=Html.Hidden("DefaultCustomerAddressReconfirm", ViewData["DefaultCustomerAddressReconfirm"]) %><%//住所再確認 (要 / 不要)%>
<%=Html.Hidden("DefaultCustomerKind", ViewData["DefaultCustomerKind"]) %><%//顧客種別 (未納客/既納客/サービス顧客)%>
<%=Html.Hidden("DefaultTargetDateFlag", ViewData["DefaultTargetDateFlag"]) %><%//対象日付（次回点検日か車検満了日か)%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
<table class="input-form">
     <!--Add 2015/08/12 arc nakayama #3241 顧客データリスト（車検案内）画面修正  レイアウト変更⇒次回点検日と車検満了日を上部に移動-->
     <tr>
        <th></th>
        <td colspan="3"><%=Html.RadioButton("TargetDateFlag", "0", ViewData["TargetDateFlag"] != null && ViewData["TargetDateFlag"].ToString().Equals("0"), new { onclick = "resetCommonCriteria(new Array('TargetDateFrom', 'TargetDateTo'))"})%>次回点検日<%=Html.RadioButton("TargetDateFlag", "1", ViewData["TargetDateFlag"] != null && ViewData["TargetDateFlag"].ToString().Equals("1"), new { onclick = "resetCommonCriteria(new Array('TargetDateFrom', 'TargetDateTo'))"})%>車検満了日&nbsp;<%=Html.TextBox("TargetDateFrom",ViewData["TargetDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('TargetDateFrom').value, 'TargetDateFrom')" }) %>～<%=Html.TextBox("TargetDateTo", ViewData["TargetDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('TargetDateTo').value, 'TargetDateTo')" }) %></td>     
        <th colspan="5"></th>
    </tr>
    <tr>
        <th>車検案内DM可否</th>
        <td colspan="3"><%=Html.DropDownList("InspectGuidFlag", (IEnumerable<SelectListItem>)ViewData["InspectGuidList"])%></td>
        <th>顧客種別</th>
        <td colspan="3"><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%></td>
    </tr>
    <tr>
        <th title="顧客マスタに設定されている営業担当部門コードです">営業担当部門コード</th>
        <td colspan="3"><%=Html.TextBox("SalesDepartmentCode2", ViewData["SalesDepartmentCode2"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('SalesDepartmentCode2','SalesDepartmentName2','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesDepartmentCode2','SalesDepartmentName2','/Department/CriteriaDialog')" /><span id="SalesDepartmentName2" style ="width:160px"><%=Html.Encode(ViewData["SalesDepartmentName2"]) %></span></td>        
           <th>住所再確認</th>
        <td colspan="3"><%=Html.DropDownList("CustomerAddressReconfirm", (IEnumerable<SelectListItem>)ViewData["CustomerAddressReconfirmList"])%></td>
    </tr>
    <tr>
        <th title="顧客マスタに設定されているサービス担当部門コードです">サービス担当部門コード</th>
        <td colspan="3"><%=Html.TextBox("ServiceDepartmentCode2", ViewData["ServiceDepartmentCode2"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('ServiceDepartmentCode2','ServiceDepartmentName2','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceDepartmentCode2','ServiceDepartmentName2','/Department/CriteriaDialog')" /><span id="ServiceDepartmentName2" style ="width:160px"><%=Html.Encode(ViewData["ServiceDepartmentName2"]) %></span></td>        
        <th title="顧客マスタに設定されている営業担当者の社員コードです">営業部門担当者コード</th>
        <td colspan="3"><%=Html.TextBox("CarEmployeeCode", ViewData["CarEmployeeCode"], new { onblur = "GetNameFromCode('CarEmployeeCode','CarEmployeeName','Employee')"})%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarEmployeeCode','CarEmployeeName','/Employee/CriteriaDialog')" /><span id="CarEmployeeName" style ="width:160px"><%=Html.Encode(ViewData["CarEmployeeName"]) %></span></td>
    </tr>
    <tr>
        <th>自社取扱いのみ</th>
        <td style="width:90px;"><%=Html.CheckBox("PrivateFlag", ViewData["PrivateFlag"] != null && ViewData["PrivateFlag"].ToString().Equals("true"), new { onclick="CheckPrivateFlag()" })%></td>           
        <th>メーカー名</th>
        <td><%=Html.DropDownList("MakerCode", (IEnumerable<SelectListItem>)ViewData["MakerList"] , new { Style= "width:190px;", onchange="GetCarMasterList()" }) %></td>
        <th>車種名</th>
        <td colspan ="3" style="width:300px;"><%=Html.DropDownList("CarCode", (IEnumerable<SelectListItem>)ViewData["CarList"], new { Style= "width:202px;" })%></td>
    </tr>
    <tr>
        <th>初年度登録(YYYY/MM)</th>
        <td colspan="3"><%=Html.TextBox("FirstRegistrationFrom", ViewData["FirstRegistrationFrom"], "{0:yyyy/MM}" ,new { @class = "alphanumeric", size = 7, maxlength = 7, onchange = "if(document.getElementById('FirstRegistrationFrom').value.length > 7 ){this.value = this.value.substr(0, this.value.length - 3)};return chkDateYYMM(document.getElementById('FirstRegistrationFrom').value, 'FirstRegistrationFrom')" })%>～<%=Html.TextBox("FirstRegistrationTo", ViewData["FirstRegistrationTo"], "{0:yyyy/MM}", new { @class = "alphanumeric", size = 7, maxlength = 7, onchange = "if(document.getElementById('FirstRegistrationTo').value.length > 7 ){this.value = this.value.substr(0, this.value.length - 3)};return chkDateYYMM(document.getElementById('FirstRegistrationTo').value, 'FirstRegistrationTo')"})%></td>
        <th>登録年月日</th>
        <td colspan="3"><%=Html.TextBox("RegistrationDateFrom",ViewData["RegistrationDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange ="return chkDate3(document.getElementById('RegistrationDateFrom').value, 'RegistrationDateFrom')" })%>～<%=Html.TextBox("RegistrationDateTo",ViewData["RegistrationDateTo"], new { @class = "alphanumeric", size = 10, maxlength = 10 , onchange ="return chkDate3(document.getElementById('RegistrationDateTo').value, 'RegistrationDateTo')" }) %></td>
    </tr>
     <%//Add 2018/04/25 arc yano #3842 顧客データリスト（営業案内）　住所による絞込の追加%>
    <tr>
        <th>住所(都道府県市区町村)</th>
        <td colspan="3"><%=Html.TextBox("CustomerAddress", ViewData["CustomerAddress"], new { size = 30, maxlength = 100})%></td>
        <th colspan = "4"></th>
    </tr>

    <tr>
        <th></th>
        <td colspan ="8">
            <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('InspectGuidFlag', 'SalesDepartmentCode2', 'ServiceDepartmentCode2', 'SalesDepartmentName2', 'ServiceDepartmentName2', 'CarEmployeeName', 'CarEmployeeCode', 'MakerCode', 'CarCode', 'PrivateFlag', 'FirstRegistrationFrom', 'FirstRegistrationTo', 'RegistrationDateFrom', 'RegistrationDateTo', 'TargetDateFlag', 'TargetDateFrom', 'TargetDateTo', 'CustomerAddressReconfirm', 'CustomerKind')); GetMakerMasterList('1'); GetPrivateCarList('1');"/>
            <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CustomerDataList', 'UpdateMsg'); document.forms[0].submit();"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<input type="button" value="車検満了日一括更新" style="width:120px" onclick="openModalAfterRefresh('/InspectGuidList/ImportDialog/1')"/><%//Add 2018/05/25 arc yano #3888 Excel取込(車検点検更新) %>
<input type="button" value="次回点検日一括更新" style="width:120px" onclick="openModalAfterRefresh('/InspectGuidList/ImportDialog/2')"/><%//Add 2018/05/25 arc yano #3888 Excel取込(車検点検更新) %>
<br />
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty);%>
<br />
<br />
<%// Add 2016/03/08 arc nakayama #3452_車検点検リスト項目追加(#3451_【大項目】車検活動リスト作成)%>
<table class="list">
     <tr>
        <th style="white-space:nowrap;"colspan="4">DM可否</th>
        <th style="white-space:nowrap;"colspan="7">顧客情報</th>
        <th style="white-space:nowrap;"colspan="7">顧客住所</th>
        <th style="white-space:nowrap;"colspan="5">DM発送先住所</th>
        <th style="white-space:nowrap;"colspan="12">車両情報</th>
        <th style="white-space:nowrap;"colspan="2">車両伝票</th>
    　　<th style="white-space:nowrap;"colspan="2">サービス伝票</th>
    </tr>
    <tr>
        <th style="white-space:nowrap">営業案内DM可否</th>    <%//DM可否%>
        <th style="white-space:nowrap">営業案内発送備考</th>  <%//DM可否%>
        <th style="white-space:nowrap">車検案内DM可否</th>    <%//DM可否%>
        <th style="white-space:nowrap">車検案内発送備考</th>  <%//DM可否%>
        <th style="white-space:nowrap">顧客コード</th>        <%//顧客情報%>
        <th style="white-space:nowrap">顧客種別</th>          <%//顧客情報%>
        <th style="white-space:nowrap">顧客名</th>            <%//顧客情報%>
        <th style="white-space:nowrap">営業担当部門名</th>    <%//顧客情報%>
        <th style="white-space:nowrap">営業担当者</th>        <%//顧客情報%>
        <th style="white-space:nowrap">サービス担当部門名</th><%//顧客情報%>
        <th style="white-space:nowrap">サービス担当者</th>    <%//顧客情報%>
        <th style="white-space:nowrap">郵便番号</th>          <%//顧客住所%>
        <th style="white-space:nowrap">都道府県</th>          <%//顧客住所%>
        <th style="white-space:nowrap">住所</th>              <%//顧客住所%>
        <th style="white-space:nowrap">電話番号</th>          <%//顧客住所%>
        <th style="white-space:nowrap">携帯電話番号</th>      <%//顧客住所%>
        <th style="white-space:nowrap">メールアドレス</th>    <%//顧客住所%><%//Add 2015/12/16 arc ookubo #3318_メールアドレスを追加%>
        <th style="white-space:nowrap">住所再確認</th>        <%//顧客住所%>
        <th style="white-space:nowrap">顧客名</th>            <%//DM発送先住所%>
        <th style="white-space:nowrap">郵便番号</th>          <%//DM発送先住所%>
        <th style="white-space:nowrap">都道府県</th>          <%//DM発送先住所%>
        <th style="white-space:nowrap">住所</th>              <%//DM発送先住所%>
        <th style="white-space:nowrap">電話番号</th>          <%//DM発送先住所%>
        <th style="white-space:nowrap">管理番号</th>          <%//車両情報%>
        <th style="white-space:nowrap">車両登録番号</th>      <%//車両情報%>
        <th style="white-space:nowrap">車台番号</th>          <%//車両情報%>
        <th style="white-space:nowrap">メーカー</th>          <%//車両情報%>
        <th style="white-space:nowrap">車種名</th>    　      <%//車両情報%>
        <th style="white-space:nowrap">グレード名</th>    　  <%//車両情報%>
        <th style="white-space:nowrap">在庫ロケーション</th>  <%//車両情報%>
        <th style="white-space:nowrap">初年度登録</th>        <%//車両情報%>
        <th style="white-space:nowrap">登録年月日</th>        <%//車両情報%>
        <th style="white-space:nowrap">次回点検日</th>        <%//車両情報%>
        <th style="white-space:nowrap">車検満了日</th>        <%//車両情報%>
        <th style="white-space:nowrap">車両重量(kg)</th>      <%//車両情報%>
        <th style="white-space:nowrap">伝票ステータス</th>    <%//車両伝票%>
        <th style="white-space:nowrap">納車年月日</th>        <%//車両伝票%>
        <th style="white-space:nowrap">最終入庫拠点</th>      <%//サービス伝票%>
        <th style="white-space:nowrap">最終入庫年月日</th>    <%//サービス伝票%>

    </tr>
    <%foreach (var CustomerData in Model)
      {%>
    <tr>
        <td style="text-align:center"><%=Html.Encode(CustomerData.DmFlag.Equals("002") ? "不可" : CustomerData.DmFlag.Equals("001") ? "可" : "" )%></td><%//車検案内(DM可否)%>
        <td><%=Html.Encode(CustomerData.DmMemo)%></td><%//備考(DM可否)%>
        <td style="text-align:center"><%=Html.Encode(CustomerData.InspectGuidFlag.Equals("002") ? "不可" : CustomerData.InspectGuidFlag.Equals("001") ? "可" : "" )%></td><%//車検案内(DM可否)%>
        <td><%=Html.Encode(CustomerData.InspectGuidMemo)%></td><%//備考(DM可否)%>
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
        <td><%=Html.Encode(CustomerData.DmCustomerName)%></td><%//顧客名称(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmPostCode)%></td><%//郵便番号(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmPrefecture)%></td><%//都道府県(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmCustomerDmAddress)%></td><%//住所(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.DmTelNumber)%></td><%//電話番号(DM発送先住所)%>
        <td><%=Html.Encode(CustomerData.SalesCarNumber)%></td><%//管理番号(車両情報)%>
        <td><%=Html.Encode(CustomerData.MorterViecleOfficialCode)%></td><%//陸運事務局名称(車両情報)%>
        <td><%=Html.Encode(CustomerData.Vin)%></td><%//車台番号(車両情報)%>
        <td><%=Html.Encode(CustomerData.MakerName)%></td><%//メーカー(車両情報)%>
        <td><%=Html.Encode(CustomerData.CarName)%></td><%//車種名称(車両情報)%>
        <td><%=Html.Encode(CustomerData.CarGradeName)%></td><%//グレード名(車両情報)%>
        <td><%=Html.Encode(CustomerData.LocationName)%></td><%//在庫ロケーション(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM}",CustomerData.FirstRegistrationDate))%></td><%//初年度登録(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.RegistrationDate))%></td><%//登録年月日(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.NextInspectionDate))%></td><%//次回点検日(車両情報)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.ExpireDate))%></td><%//車検満了日(車両情報)%>
        <td style="text-align:right"><%=Html.Encode(CustomerData.CarWeight)%></td><%//車両重量(車両情報)%>
        <td><%=Html.Encode(CustomerData.SalesOrderStatusName)%></td><%//車両伝票ステータス(車両伝票)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.SalesDate))%></td><%//納車年月日(車両伝票)%>
        <td><%=Html.Encode(CustomerData.ServiceDepartmentName3)%></td><%//最終入庫ロケーション(サービス伝票)%>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",CustomerData.ArrivalPlanDate))%></td><%//最終入庫日(サービス伝票)%>
    </tr>
    <%}%>
</table>
<br />
</asp:Content>
