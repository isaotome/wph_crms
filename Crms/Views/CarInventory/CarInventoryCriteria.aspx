<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarInventory>>" %>

<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <%
    // ------------------------------------------------------------------
    // 画面名：車両在庫棚卸
    // 作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    // 更新日：
    // ------------------------------------------------------------------
    %>
車両在庫棚卸
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
    <%using (Html.BeginForm("Criteria", "CarInventory", new { id = 0 }, FormMethod.Post))
      { %>
    <%=Html.Hidden("id",  ViewData["id"]) %>
    <%=Html.Hidden("InventoryStatus", ViewData["InventoryStatus"]) %>                                                   <%//全部門の棚卸ステータス%>
    <%=Html.Hidden("RequestFlag", "99") %>                                                           <%//処理の種別(各ボタンクリック時の処理の種類)%>
    <%=Html.Hidden("EditFlag", ViewData["EditFlag"]) %>                                                                 <%//変更中フラグ%>
    <%=Html.Hidden("InventoryTempDecidedVisible", ViewData["InventoryTempDecidedVisible"].ToString().ToLower()) %>      <%//仮確定ボタンクリック可／不可状態)%>
    <%=Html.Hidden("InventoryDecidedVisible", ViewData["InventoryDecidedVisible"].ToString().ToLower()) %>              <%//確定ボタンクリック可／不可状態)%>
    <%=Html.Hidden("InventoryDecidedClickable", ViewData["InventoryDecidedClickable"].ToString().ToLower()) %>          <%//確定ボタンクリック可／不可状態)%>
    <%=Html.Hidden("InventoryWorkingDate", ViewData["InventoryWorkingDate"]) %>                                         <%//棚卸作業日 %>

    <br />
    <br />
    <a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
    <div id="search-form" style="display: block">
        <br />
        <div id="ValidBlock">
            <%=Html.ValidationSummary() %>
        </div>
        <table class="input-form">
            <tr>
                <th style="width: 100px">棚卸対象年月</th>
                <td style="width: 130px; white-space: nowrap">
                    <span id="sInventoryMonth"><%=Html.Encode(ViewData["InventoryMonth"]) %></span>
                    <%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>
                </td>
                <th style="width: 100px">棚卸開始日時</th>
                <td style="width: 230px; white-space: nowrap">
                    <span id="sInventoryStartDate"><%=Html.Encode(ViewData["InventoryStartDate"]) %></span>
                    <%=Html.Hidden("InventoryStartDate", ViewData["InventoryStartDate"]) %>
                </td>
            </tr>
            <tr>
                <th>部門コード </th>
                <td style="width: 130px; white-space: nowrap">
                    <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, size = 4, onchange = "GetNameFromCodeForCarInventory('Department', SetDisableCarInventory, '999');"})%>&nbsp;
                <%// Mod 2021/05/20 yano #4045 %>
                <img alt="部門検索" style="cursor: pointer" src="/Content/Images/search.jpg" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) { if(document.getElementById('DepartmentCode').readOnly == false) { if(openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog?CarInventoryFlag=1')){ GetNameFromCodeForCarInventory('Department', SetDisableCarInventory, '999'); }}} else { var callback = function (vRet){ GetNameFromCodeForCarInventory('Department', SetDisableCarInventory, '999'); }; openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog?CarInventoryFlag=1', undefined, undefined, undefined, undefined, callback);}" />
                <%--<img alt="部門検索" style="cursor: pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('DepartmentCode').readOnly == false) {if(openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CarInventoryFlag=1')){GetNameFromCodeForCarInventory('Department', SetDisableCarInventory, '999');}}" />--%>
                </td>
                <th>部門名 </th>
                <td style="width: 230px; white-space: nowrap">
                    <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span>
                </td>
            </tr>
            <tr>
                <th>倉庫コード </th>
                <td style="width: 130px; white-space: nowrap">
                    <span id="WarehouseCode" class ="WarehouseCode"><%=Html.Encode(ViewData["WarehouseCode"]) %></span><%// Mod 2021/05/20 yano #4045  class名を追加%>
                    <%=Html.Hidden("WarehouseCode", ViewData["WarehouseCode"], new { id = "HdWarehouseCode", @class = "WarehouseCode"})%><%// Mod 2021/05/20 yano #4045  class名を追加%>
                </td>
                <th>倉庫名 </th>
                <td style="width: 230px; white-space: nowrap">
                    <span id="WarehouseName"><%=Html.Encode(ViewData["WarehouseName"]) %></span>
                </td>
            </tr>
            <tr>
                <th style="width: 100px"></th>
                <td colspan="3">
                    <%//棚卸開始ボタン %>
                    <input id="InventoryStartButton" type="button" value="棚卸開始" <%=ViewData["StartStatus"]%> onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
                    <%//棚卸表出力ボタン %>
                    <input id="ExcelOutputButton" type="button" value="棚卸表出力" <%=ViewData["OutputStatus"]%> onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CarInventory', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'" />
                    <%//棚卸表取込ボタン %>
                    <input id="ExcelInputButton" type="button" value="棚卸表取込" <%=ViewData["InputStatus"]%> onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/CarInventory/ImportDialog?InventoryMonth=<%=ViewData["InventoryMonth"]%>    &DepartmentCode=' + document.getElementById('DepartmentCode').value + '&WarehouseCode=' + document.getElementById('WarehouseCode').innerText, '', '', 'yes', 'yes')" /><%//Mod 2017/12/18 arc yano #3838 %>
                    <%//棚卸作業結果一時保存ボタン %>
                    <input id="InventorySaveButton" type="button" value="棚卸作業結果一時保存" style="width: 130px" <%=ViewData["TempStatus"]%> onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
                    <%//棚卸確定ボタン %>
                    <input id="InventoryTempDecidedButton" type="button" value="棚卸確定" <%=ViewData["TempDecidedStatus"]%> <%=ViewData["TempDecidedVisible"]%> onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '4'; DisplayImage('UpdateMsg', '0'); formSubmit()" />
                     <%//棚卸確定取消ボタン %>
                    <input id="InventoryCancelButton" type="button" value="棚卸確定取消" <%=ViewData["CancelStatus"]%> <%=ViewData["CancelVisible"]%> onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '6'; DisplayImage('UpdateMsg', '0'); formSubmit()" /><%//Add 2018/08/01 yano #3926 %>
                    <%//棚卸本確定ボタン %>
                    <input id="InventoryDecidedButton" type="button" value="棚卸本確定" <%=ViewData["DecidedStatus"]%> <%=ViewData["DecidedVisible"]%> onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/CarInventory/DecidedDialog?InventoryMonth=<%=ViewData["InventoryMonth"]%>', '', '', 'no', 'yes')" /><%//Mod 2017/12/18 arc yano #3838 %>
                </td>
            </tr>
        </table>
        <br />
        <br />
        <table class="input-form">
            <!--棚卸ステータスが「未実施」でない、または本確定を行えるユーザの場合は検索を行えるようにする -->
            <% if ((!ViewData["InventoryStatus"].Equals("999") && !ViewData["InventoryStatus"].Equals("000")) || ViewData["InventoryDecidedVisible"].ToString().ToLower().Equals("true"))
               {%>
            <tr>
                <th>ロケーションコード</th>
                <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location')" })%>&nbsp;
            <img alt="ロケーション検索" id="LocationSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="if(document.getElementById('LocationCode').readOnly == false){ openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog?BusinessType=002,009&WarehouseCode=' + document.getElementById('HdWarehouseCode').value) }" />
                </td>
                <th>ロケーション名</th>
                <td><span id="LocationName" style ="width:160px"><%=Html.Encode(ViewData["LocationName"]) %></span></td>
            </tr>
            <tr>
                <th>管理番号</th>
                <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
                <th>車台番号</th>
                <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength = 20 })%></td>
            </tr>
            <tr>
                <th>新中区分</th>
                <td><%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
                <th>在庫区分</th>
                <td><%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["CarStatusList"])%></td>
            </tr>
             <tr><%//Add 2017/09/07 arc yano #3784%>
                <th>棚差有無</th>
                <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.CheckBox("InventoryDiff", (ViewData["InventoryDiff"] != null) && (ViewData["InventoryDiff"].ToString().Contains("true")))%>棚差があるレコードのみ表示</td>
            </tr>
            <tr>
                <th></th>
                <td colspan="3">
                    <input id="SearchButton" type="button" value="検索" onclick="ClearValidateMessage('ValidBlock'); if (document.getElementById('EditFlag').value.toLowerCase() == 'true') { if (confirm('変更中の項目があります。\r\n保存を行わずに検索を実行すると、変更が破棄されます。\r\n処理を続行してもよろしいでしょうか')) { document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList() } } else { document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList() } " />
                    <input id="ExcelOut" type="button" value="画面リスト出力" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); dispProgressed('CarInventory', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'" />
                    <input id="ClearButton" type="button" value="クリア" onclick="ClearValidateMessage('ValidBlock'); resetCommonCriteria(new Array('LocationCode', 'LocationName', 'SalesCarNumber', 'Vin', 'NewUsedType', 'CarStatus', 'InventoryDiff'));" />
                </td>
            </tr>

            <%}
               else
               { %>
            <tr>
                <th>ロケーションコード</th>
                <td>
                    <%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "readonly alphanumeric", @readonly = "readonly", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location')" })%>&nbsp;
            <img alt="ロケーション検索" id="LocationSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="if(document.getElementById('LocationCode').readOnly == false){ openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog?BusinessType=002,009&WarehouseCode=' + document.getElementById('HdWarehouseCode').value)}" />
                </td>
                <th>ロケーション名</th>
                <td><span id="LocationName" style ="width:160px"><%=Html.Encode(ViewData["LocationName"]) %></span></td>
            </tr>
            <tr>
                <th>管理番号</th>
                <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "readonly alphanumeric", @readonly = "readonly", size = 10, maxlength = 50 })%></td>
                <th>車台番号</th>
                <td><%=Html.TextBox("Vin", ViewData["Vin"], new { @class = "readonly alphanumeric", @readonly = "readonly", size = 20, maxlength = 20 })%></td>
            </tr>
            <tr>
                <th>新中区分</th>
                <td><%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { @disabled = "disabled"})%></td>
                <th>在庫区分</th>
                <td><%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["CarStatusList"], new { @disabled = "disabled"})%></td>
            </tr>
            <tr><%//Add 2017/09/07 arc yano #3784%>
               <th>棚差有無</th>
                <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.CheckBox("InventoryDiff", (ViewData["InventoryDiff"] != null) && (ViewData["InventoryDiff"].ToString().Contains("true")), new { @disabled = "disabled", @class = "disabled"})%>棚差があるレコードのみ表示</td>
            </tr>
            <tr>
                <th></th>
                <td colspan="3">
                    <input id="SearchButton" type="button" value="検索" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); if (document.getElementById('EditFlag').value.toLowerCase() == 'true') { if (confirm('変更中の項目があります。\r\n保存を行わずに検索を実行すると、変更が破棄されます。\r\n処理を続行してもよろしいでしょうか')) { document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList() } } else { document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList() }" />
                    <input id="ExcelOut" type="button" value="画面リスト出力" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); dispProgressed('CarInventory', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'" />
                    <input id="ClearButton" type="button" value="クリア" disabled="disabled" onclick="resetCommonCriteria(new Array('LocationCode', 'LocationName', 'SalesCarNumber', 'Vin', 'NewUsedType', 'CarStatus', 'InventoryDiff'));" />
                </td>
            </tr>
            <%} %>
        </table>

        <div id="UpdateMsg" style="display: none">
            <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display: block" width="30" height="30" />
        </div>
    </div>
    <br />
    <br />
    <div id="pageBlock">
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    </div>
    <br />
    <br />

    <div id ="tableBlock">
    <table class="list" style="width: 100%">
        <tr>
            <th style="width: 20px; height: 20px; text-align: center;">
                <% if (ViewData["InventoryStatus"].ToString().Equals("001"))
                   {    //棚卸ステータス ≠ 「未実施」「本確定」%>
                <img alt="行追加" src="/Content/Images/plus.gif" style="cursor: pointer" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '11'; DisplayImage('UpdateMsg', '0'); formSubmit();" />
                <% } %>
            </th>
            <th style="white-space: nowrap">ロケーションコード</th>
            <th style="white-space: nowrap">ロケーション名</th>
            <th style="white-space: nowrap">管理番号</th>
            <th style="white-space: nowrap">車台番号</th>
            <th style="white-space: nowrap">新中区分</th>
            <th style="white-space: nowrap">在庫区分</th>
            <th style="white-space: nowrap">ブランド名</th>
            <th style="white-space: nowrap">車種名</th>
            <th style="white-space: nowrap">系統色</th>
            <th style="white-space: nowrap">車両カラーコード</th>
            <th style="white-space: nowrap">車両カラー名</th>
            <th style="white-space: nowrap">車両登録番号</th>
            <th style="white-space: nowrap">実棚</th>
            <th style="white-space: nowrap">誤差理由</th>
            <th style="white-space: nowrap">備考</th>
        </tr>

        <%int i = 0; %>
        <%foreach (var a in Model)
          {
              string namePrefix = string.Format("line[{0}].", i);
              string idPrefix = string.Format("line[{0}]_", i);
        %>
        <tr> 
        <% if (ViewData["InventoryStatus"].ToString().Equals("001")){ %>
            <td style="width: 22px; text-align: center">
                <img alt="行削除" src="/Content/Images/minus.gif" style="cursor: pointer" onclick="ClearValidateMessage('ValidBlock'); if (document.getElementById('EditFlag').value.toLowerCase() == 'true') { if (confirm('変更中の項目があります。\r\n保存を行わずに削除を実行すると、変更が破棄されます。\r\n処理を続行してもよろしいでしょうか')) { document.getElementById('RequestFlag').value = '12'; document.getElementById('DelLine').value  = '<%=i %>'; DisplayImage('UpdateMsg', '0'); formSubmit() } }else{ document.getElementById('RequestFlag').value = '12'; document.getElementById('DelLine').value  = '<%=i %>'; DisplayImage('UpdateMsg', '0'); formSubmit() }" />
            </td>
            <%//ロケーションコード%>
            <td style="white-space: nowrap"><%=Html.TextBox(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode", @class="alphanumeric", size = "5", onchange = "GetNameFromCode('" + idPrefix + "LocationCode', '" + idPrefix + "LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '" + idPrefix + "LocationName' ); UpdateEditFlag('EditFlag', true)"}) %></td>
            <%//ロケーション名称%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>LocationName"><%=Html.Encode(a.LocationName) %></span><%=Html.Hidden(namePrefix + "LocationName", a.LocationName, new { id = idPrefix + "LocationName_hd"}) %></td>
            <%//管理番号%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>SalesCarNumber"><%=Html.Encode(a.SalesCarNumber) %></span><%=Html.Hidden(namePrefix + "SalesCarNumber", a.SalesCarNumber, new { id = idPrefix + "SalesCarNumber_hd"}) %></td>
            <td style="white-space: nowrap"><%//車台番号 %>
                <% if (a.NewRecFlag)
                   {%>
                <%=Html.TextBox(namePrefix + "Vin", a.Vin, new { style = "width:130px", maxlength = "20", id = idPrefix + "Vin", onchange = "GetSalesCarInfo('" + idPrefix + "Vin','" + idPrefix + "SalesCarNumber', new Array('"+ idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "ColorType','" + idPrefix + "ExteriorColorCode','" + idPrefix + "ExteriorColorName','" + idPrefix + "SalesCarNumber','" + idPrefix + "RegistrationNumber'), null, null, null,null, null, setCarInventoryHidden);" }) %>
                <%//Mod 2021/04/27 yano #4045 %>
                <img alt="車両検索" id="SalesCarSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {var dRet = openSearchDialog('<%=idPrefix%>SalesCarNumber','<%=idPrefix%>Vin','/SalesCar/CriteriaDialog'); GetMasterDetailFromCode('<%=idPrefix%>SalesCarNumber',new Array('<%=idPrefix%>CarBrandName','<%=idPrefix%>CarName','<%=idPrefix%>ColorType','<%=idPrefix%>SalesCarNumber','<%=idPrefix%>ExteriorColorCode','<%=idPrefix%>ExteriorColorName','<%=idPrefix%>RegistrationNumber'),'SalesCar', null, null, null, setCarInventoryHidden);} else  {var callback = function(dRet){GetMasterDetailFromCode('<%=idPrefix%>SalesCarNumber',new Array('<%=idPrefix%>CarBrandName','<%=idPrefix%>CarName','<%=idPrefix%>ColorType','<%=idPrefix%>SalesCarNumber','<%=idPrefix%>ExteriorColorCode','<%=idPrefix%>ExteriorColorName','<%=idPrefix%>RegistrationNumber'),'SalesCar', null, null, null, setCarInventoryHidden);}; openSearchDialog('<%=idPrefix%>SalesCarNumber','<%=idPrefix%>Vin','/SalesCar/CriteriaDialog', undefined, undefined, undefined, undefined, callback);};" />
                <%--<img alt="車両検索" id="SalesCarSearch" style="cursor: pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=idPrefix%>SalesCarNumber','<%=idPrefix%>Vin','/SalesCar/CriteriaDialog'); GetMasterDetailFromCode('<%=idPrefix%>SalesCarNumber',new Array('<%=idPrefix%>CarBrandName','<%=idPrefix%>CarName','<%=idPrefix%>ColorType','<%=idPrefix%>SalesCarNumber','<%=idPrefix%>ExteriorColorCode','<%=idPrefix%>ExteriorColorName','<%=idPrefix%>RegistrationNumber'),'SalesCar', null, null, null, setCarInventoryHidden);" />--%>
                <%}
                   else
                   { %>
                <%=Html.Encode(a.Vin) %><%=Html.Hidden(namePrefix + "Vin", a.Vin, new { id = idPrefix + "Vin"}) %>
                <%} %>
            </td>

            <%//Mod 2020/08/29 yano #4049 システム管理者または新規追加レコードのみ新中区分を変更できるように修正 %>
            <%//Mod 2017/12/15 arc yano #3839 %>
            <td style="white-space: nowrap">
            <% if(!string.IsNullOrWhiteSpace(a.CarStatus) && (a.CarStatus.Equals("999") || a.CarStatus.Equals("006"))){ %>
                <%if(ViewData["InventoryDecidedVisible"].ToString().ToLower().Equals("true") || a.NewRecFlag == true){ %>
                    <%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" , onchange = "UpdateEditFlag('EditFlag', true)"})%>
                <%}else{ %>
                    <%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" , onchange = "UpdateEditFlag('EditFlag', true)",  @disabled="disabled" })%>
                <%} %>            
            <%}else{ %>
                --<%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" , onchange = "UpdateEditFlag('EditFlag', true)" , style = "display:none"})%>
            <%} %>
            <%=Html.Hidden(namePrefix + "hdNewUsedType",  a.hdNewUsedType, new { id = idPrefix + "hdNewUsedType" })%><%//Add 2021/02/22 yano #4081 %>
            </td>
            <%//新中区分%>
            <td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "CarStatus", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CarStatusListLine"])[i], new { id = idPrefix + "CarStatus" , onchange = "UpdateEditFlag('EditFlag', true); changeDisplayNewUedType('" + idPrefix + "CarStatus' , '" + idPrefix + "NewUsedType', '" +  idPrefix + "hdNewUsedType')" })%></td><%//Mod 2021/02/22 yano #4081%>
            <%--<td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "CarStatus", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CarStatusListLine"])[i], new { id = idPrefix + "CarStatus" , onchange = "UpdateEditFlag('EditFlag', true); changeDisplayNewUedType('" + idPrefix + "CarStatus' , '" + idPrefix + "NewUsedType')" })%></td>--%>
            <%//在庫区分%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarBrandName"><%=Html.Encode(a.CarBrandName) %></span><%=Html.Hidden(namePrefix + "CarBrandName", a.CarBrandName, new { id = idPrefix + "CarBrandName_hd"}) %></td>
            <%//ブランド名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarName"><%=Html.Encode(a.CarName) %></span><%=Html.Hidden(namePrefix + "CarName", a.CarName, new { id = idPrefix + "CarName_hd"}) %></td>
            <%//車種名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ColorType"><%=Html.Encode(a.ColorType) %></span><%=Html.Hidden(namePrefix + "ColorType", a.ColorType, new { id = idPrefix + "ColorType_hd"}) %></td>
            <%//系統色%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorCode"><%=Html.Encode(a.ExteriorColorCode) %></span><%=Html.Hidden(namePrefix + "ExteriorColorCode", a.ExteriorColorCode, new { id = idPrefix + "ExteriorColorCode_hd"}) %></td>
            <%//車両カラーコード%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorName"><%=Html.Encode(a.ExteriorColorName) %></span><%=Html.Hidden(namePrefix + "ExteriorColorName", a.ExteriorColorName, new { id = idPrefix + "ExteriorColorName_hd"}) %></td>
            <%//車両カラー名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>RegistrationNumber"><%=Html.Encode(a.RegistrationNumber) %></span><%=Html.Hidden(namePrefix + "RegistrationNumber", a.RegistrationNumber, new { id = idPrefix + "RegistrationNumber_hd"}) %></td>
            <%//車両登録番号%>
            <td style="text-align: right"><%//実棚%>
                <%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F0}",a.PhysicalQuantity), new { @class = "numeric", id = idPrefix + "PhysicalQuantity", size = 1, maxlength = 1, onchange ="var ret = ChkPhysicalQuantityForCarInventory('"+ idPrefix +"PhysicalQuantity', '" + idPrefix + "SavedPhsycalQuantity'); if(ret){UpdateEditFlag('EditFlag', true)}"})%>
                <%=Html.Hidden(namePrefix + "SavedPhsycalQuantity", string.Format("{0:F0}", a.PhysicalQuantity), new {id = idPrefix + "SavedPhsycalQuantity"})%>
            </td>
            <td style="white-space: nowrap""><%//誤差理由%>
                <%=Html.DropDownList(namePrefix + "Comment", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CommentList"])[i], new { id = idPrefix + "Comment" , onchange = "UpdateEditFlag('EditFlag', true)" })%>
            </td>
            <td style="white-space: nowrap""><%//備考%>
                <%=Html.TextBox(namePrefix + "Summary", a.Summary, new { id = idPrefix + "Summary", size = 15, maxlength = 255, onchange = "UpdateEditFlag('EditFlag', true)"})%>
            </td>
       <%}else{ %>
             <td style="width: 22px; text-align: center">
            </td>
            <%//ロケーションコード%>
            <td style="white-space: nowrap"><%=Html.TextBox(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode", @class="readonly alphanumeric", @readonly="readonly", size = "5", onchange = "GetNameFromCode('" + idPrefix + "LocationCode', '" + idPrefix + "LocationName', 'Location', null, null, null, null, setCarInventoryHidden, '" + idPrefix + "LocationName' ); UpdateEditFlag('EditFlag', true)"}) %></td>
            <%//ロケーション名称%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>LocationName"><%=Html.Encode(a.LocationName) %></span><%=Html.Hidden(namePrefix + "LocationName", a.LocationName, new { id = idPrefix + "LocationName_hd"}) %></td>
            <%//管理番号%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>SalesCarNumber"><%=Html.Encode(a.SalesCarNumber) %></span><%=Html.Hidden(namePrefix + "SalesCarNumber", a.SalesCarNumber, new { id = idPrefix + "SalesCarNumber_hd"}) %></td>
            <td style="white-space: nowrap"><%//車台番号 %>
             <%=Html.Encode(a.Vin) %><%=Html.Hidden(namePrefix + "Vin", a.Vin, new { id = idPrefix + "Vin"}) %>
            </td>
            <%//新中区分%>
            <%//Mod 2017/12/15 arc yano #3839 %>
            <td style="white-space: nowrap">
            <% if(!string.IsNullOrWhiteSpace(a.CarStatus) && (a.CarStatus.Equals("999") || a.CarStatus.Equals("006"))){ %>
                <%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" , @disabled="disabled", onchange = "UpdateEditFlag('EditFlag', true)"})%>
            <%}else{ %>
                --<%=Html.DropDownList(namePrefix + "NewUsedType", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["NewUsedTypeListLine"])[i], new { id = idPrefix + "NewUsedType" ,@disabled="disabled", onchange = "UpdateEditFlag('EditFlag', true)" , style = "display:none"})%>
            <%} %>
            <%=Html.Hidden(namePrefix + "hdNewUsedType", a.hdNewUsedType, new { id = idPrefix + "hdNewUsedType" })%><%//Add 2021/02/22 yano #4081 %>

            </td>
            <%//在庫区分%>
            <td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "CarStatus", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CarStatusListLine"])[i], new { id = idPrefix + "CarStatus" , onchange = "UpdateEditFlag('EditFlag', true); changeDisplayNewUedType('" + idPrefix + "CarStatus' , '" + idPrefix + "NewUsedType', '" +  idPrefix + "hdNewUsedType')" })%></td><%//Mod 2021/02/22 yano #4081%>
            <%--<td style="white-space: nowrap"><%=Html.DropDownList(namePrefix + "CarStatus", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CarStatusListLine"])[i], new { id = idPrefix + "CarStatus" , @disabled="disabled", onchange = "UpdateEditFlag('EditFlag', true); changeDisplayNewUedType('" + idPrefix + "CarStatus' , '" + idPrefix + "NewUsedType')" })%></td>--%>
            <%//ブランド名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarBrandName""><%=Html.Encode(a.CarBrandName) %></span><%=Html.Hidden(namePrefix + "CarBrandName", a.CarBrandName, new { id = idPrefix + "CarBrandName_hd"}) %></td>
            <%//車種名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>CarName"><%=Html.Encode(a.CarName) %></span><%=Html.Hidden(namePrefix + "CarName", a.CarName, new { id = idPrefix + "CarName_hd"}) %></td>
            <%//系統色%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ColorType"><%=Html.Encode(a.ColorType) %></span><%=Html.Hidden(namePrefix + "ColorType", a.ColorType, new { id = idPrefix + "ColorType_hd"}) %></td>
            <%//車両カラーコード%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorCode"><%=Html.Encode(a.ExteriorColorCode) %></span><%=Html.Hidden(namePrefix + "ExteriorColorCode", a.ExteriorColorCode, new { id = idPrefix + "ExteriorColorCode_hd"}) %></td>
            <%//車両カラー名%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>ExteriorColorName"><%=Html.Encode(a.ExteriorColorName) %></span><%=Html.Hidden(namePrefix + "ExteriorColorName", a.ExteriorColorName, new { id = idPrefix + "ExteriorColorName_hd"}) %></td>
            <%//車両登録番号%>
            <td style="white-space: nowrap"><span id="<%=idPrefix%>RegistrationNumber"><%=Html.Encode(a.RegistrationNumber) %></span><%=Html.Hidden(namePrefix + "RegistrationNumber", a.RegistrationNumber, new { id = idPrefix + "RegistrationNumber_hd"}) %></td> 
            <td style="text-align: right"><%//実棚%>
                <%=Html.Encode(string.Format("{0:F0}", a.PhysicalQuantity)) %><%=Html.Hidden(namePrefix + "PhysicalQuantity", a.PhysicalQuantity, new { id = idPrefix + "PhysicalQuantity"}) %>
            </td>
            <td style="white-space: nowrap""><%//誤差理由%>
                <%=Html.DropDownList(namePrefix + "Comment", (IEnumerable<SelectListItem>)((List<IEnumerable<SelectListItem>>)ViewData["CommentList"])[i], new { id = idPrefix + "Comment" , @disabled="disabled", onchange = "UpdateEditFlag('EditFlag', true)" })%>
            </td>
            <td style="white-space: nowrap""><%//備考%>
                <%=Html.Encode(a.Summary) %><%=Html.Hidden(namePrefix + "Summary", a.Summary, new { id = idPrefix + "Summary"}) %>
            </td>
       <%} %>
            <%=Html.Hidden(namePrefix + "NewRecFlag", a.NewRecFlag, new { id = idPrefix + "NewRecFlag"}) %><%//新規追加フラグ %>
            <%=Html.Hidden(namePrefix + "InventoryId", a.InventoryId, new { id = idPrefix + "InventoryId"}) %><%//棚卸ID %>
        </tr>
        <%
               i++;
          } %>

            <%//削除行 %>
            <%=Html.Hidden("DelLine", 0 ) %>
    </table>
    </div>
    <%}%>
</asp:Content>
