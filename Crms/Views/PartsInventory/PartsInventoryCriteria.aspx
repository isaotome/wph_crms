<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.PartsInventory>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
部品在庫検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%using (Html.BeginForm("Criteria", "PartsInventory", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id",  ViewData["id"]) %>
<%=Html.Hidden("InventoryStatus", ViewData["InventoryStatus"]) %>       <%//全部門の棚卸ステータス%>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//処理の種別(各ボタンクリック時の処理の種類)%>
<%=Html.Hidden("EditFlag", "false") %><%//変更中フラグ%>
<%// Add 2015/05/21 arc yano IPO対応(部品棚卸) 確定ボタンクリック可／不可状態追加 %>
<%=Html.Hidden("InventoryEndButtonEnable", ViewData["InventoryEndButtonEnable"].ToString().ToLower()) %><%//確定ボタンクリック可／不可状態)%>
<%// Mod 2015/06/16 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日→棚卸開始日時に変更 %>
<%=Html.Hidden("PartsInventoryWorkingDate", ViewData["PartsInventoryWorkingDate"]) %>

<%=Html.Hidden("InventoryStatusPartsBalance", ViewData["InventoryStatusPartsBalance"]) %><%//Add 2017/07/26 arc yano #3781)%>
<%=Html.Hidden("DataEditButtonVisible", ViewData["DataEditButtonVisible"].ToString().ToLower()) %><%//Add 2017/07/26 arc yano #3781)%>

<br/>
<br/>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<div id ="ValidBlock">
<%=Html.ValidationSummary() %>
</div>
<table class="input-form">
    <tr>
        <th style="width: 100px">棚卸対象年月</th>
        <td style="width: 130px; white-space: nowrap">
            <span id ="sInventoryMonth"><%=Html.Encode(ViewData["InventoryMonth"]) %></span>
            <%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>
        </td>
        <%// Mod 2015/06/16 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日→棚卸開始日時に変更 %>
        <th style="width: 100px">棚卸開始日時</th>
        <td style="width: 230px; white-space: nowrap">
            <span id ="sPartsInventoryStartDate"><%=Html.Encode(ViewData["PartsInventoryStartDate"]) %></span>
            <%=Html.Hidden("PartsInventoryStartDate", ViewData["PartsInventoryStartDate"]) %>
        </td>
    </tr>
    <tr>
        <th>部門コード </th>
        <td style="width:130px; white-space:nowrap">   
            <%//Mod 2015/05/27 arc yano IPO対応(部品棚卸)障害対応、仕様変更② 棚卸対象の部門に絞り込み、部門検索ダイアログに表示する。また、ルックアップで部門を入力した場合でも、棚卸状況により、各ボタンの制御を行うように修正する %>
            <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new {  maxlength = 3, size = 4, onchange = "GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department', 'true', 'WarehouseCode', 'WarehouseName');"})%>&nbsp;
            <%//Mod 2021/05/20 yano #4045 %>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1){ if(document.getElementById('DepartmentCode').readOnly == false){ if(openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?PartsInventoryFlag=1')){ CheckInventoryStatus(document.getElementById('DepartmentCode').value, document.getElementById('InventoryMonth').value, document.getElementById('PartsInventoryWorkingDate').value); GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department', 'true', 'WarehouseCode', 'WarehouseName');}}}else{ var callback = function (vRet){ CheckInventoryStatus(document.getElementById('DepartmentCode').value, document.getElementById('InventoryMonth').value, document.getElementById('PartsInventoryWorkingDate').value); GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department', 'true', 'WarehouseCode', 'WarehouseName');}; if(document.getElementById('DepartmentCode').readOnly == false){ openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?PartsInventoryFlag=1', undefined, undefined, undefined, undefined, callback)};}" /><%// Mod #3702 部品在庫棚卸 部門のルックアップ入力時の倉庫コードの漏れ%>
            <%--<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('DepartmentCode').readOnly == false) {if(openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?PartsInventoryFlag=1')){CheckInventoryStatus(document.getElementById('DepartmentCode').value, document.getElementById('InventoryMonth').value, document.getElementById('PartsInventoryWorkingDate').value);GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department', 'true', 'WarehouseCode', 'WarehouseName');}}" /><%// Mod #3702 部品在庫棚卸 部門のルックアップ入力時の倉庫コードの漏れ%>--%>
        </td>
         <th>部門名 </th>
        <td style="width:230px; white-space:nowrap">
            <span id ="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span>
        </td>
    </tr>
    <%// Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 倉庫追加 %>
    <tr>
        <th>倉庫コード </th>
        <td style="width:130px; white-space:nowrap">
            <span id ="WarehouseCode" class="WarehouseCode"><%=Html.Encode(ViewData["WarehouseCode"]) %></span><%//Mod 2021/05/20 yano #4045 class追加%>
            <%=Html.Hidden("WarehouseCode", ViewData["WarehouseCode"], new { id = "HdWarehouseCode", @class = "WarehouseCode"})%><%//Mod 2021/05/20 yano #4045 class追加%>
        </td>
        <th>倉庫名 </th>
        <td style="width:230px; white-space:nowrap">
            <span id ="WarehouseName"><%=Html.Encode(ViewData["WarehouseName"]) %></span>
        </td>
    </tr>
    <tr>
        <th style="width: 100px"></th>
        <td colspan ="3">
            <%// Mod 2017/12/18 arc yano #3838 部品在庫棚卸　棚卸表(Excel)取込後の画面への反映漏れ エクセル取込画面を閉じた後は再検索を行うように修正%> 
            <%// Add 2017/07/27 arc yano #3781 部品在庫棚卸　確定データ編集ボタンを追加 %>
            <%// Mod 2016/07/14 arc yano #3618 部品在庫棚卸　棚卸開始ボタン押下判定の不具合　棚卸作業日の翌日に棚卸開始ボタンを押下できるように修正 %>
            <%// Mod 2015/05/27 arc yano IPO対応(部品棚卸)障害対応、仕様変更②  棚卸作業日が設定されていない場合、または棚卸作業日が未来日の設定の場合は棚卸開始ボタンをクリックできないように修正する %> 
            <% if ((ViewData["PartsInventoryWorkingDate"] != null && !string.IsNullOrWhiteSpace(ViewData["PartsInventoryWorkingDate"].ToString()) && (DateTime.Today.Date >= DateTime.Parse(ViewData["PartsInventoryWorkingDate"].ToString()).AddHours(1))) && ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("000"))
            { %>
                <input id="InventoryStartButton" type="button" value="棚卸開始" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
            <%}else{%>
                <input id="InventoryStartButton" type="button" value="棚卸開始" disabled="disabled" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>                
            <%} %>

            <%//検索した部門の棚卸ステータスが実施中(001)だった場合ボタンを活性、そうでなければ非活性 %>
            <% if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("001"))
            {%>
                <input id="ExcelOutputButton" type="button" value="EXCEL出力" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'"/>
                <input id="ExcelInputButton" type="button" value="EXCEL取込" onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/PartsInventory/ImportDialog', '', '', 'no', 'yes')"/><%//Mod 2017/12/18 arc yano #3838 %>
                <input id="InventorySaveButton" type="button" value="棚卸作業結果一時保存" style="width:130px" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
                <%// Add 2015/05/21 arc yano IPO対応(部品棚卸) 確定ボタンクリック表示／非表示状態追加 %>    
                <% if (ViewData["InventoryEndButtonEnable"] != null && (bool)(ViewData["InventoryEndButtonEnable"]) == true)
                { %>
                    <input id="InventoryEndButton" type="button" value="棚卸確定" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '4'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
                <%} %>
                <% else{%>
                    <input id="InventoryEndButton" type="button" value="棚卸確定" disabled="disabled" style="visibility:hidden" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '4'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
                <%} %>

                <%// Mod 2017/07/27 arc yano #3781 %>
                <% if (ViewData["DataEditButtonVisible"] != null && (bool)(ViewData["DataEditButtonVisible"]) == true)
                { %>
                    <input id="InventoryDataEdit" type="button" value="新規作成" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh('/PartsInventory/DataEdit?CreateFlag=1&InventoryMonth=' + document.getElementById('InventoryMonth').value, '', '', 'no', 'no')""/>
               <%} %>

            <%}else{%>
                <% if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("002"))
                {%>
                    <input id="ExcelOutputButton" type="button" value="EXCEL出力" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'"/>
                <%}else{%>
                    <input id="ExcelOutputButton" type="button" value="EXCEL出力" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'"/>
                <%} %>
                    <input id="ExcelInputButton" type="button" value="EXCEL取込" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/PartsInventory/ImportDialog', '', '', 'no', 'yes')"/><%//Mod 2017/12/18 arc yano #3838 %>
                    <input id="InventorySaveButton" type="button" value="棚卸作業結果一時保存" disabled="disabled" style="width:130px" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
                    <%// Add 2015/05/21 arc yano IPO対応(部品棚卸) 確定ボタンクリック表示／非表示状態追加 %>     
                    <% if (ViewData["InventoryEndButtonEnable"] != null && (bool)(ViewData["InventoryEndButtonEnable"]) == true)
                    { %>
                        <input id="InventoryEndButton" type="button" value="棚卸確定" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '4'; DisplayImage('UpdateMsg', '0'); formSubmit()"/><%//Mod 2015/05/26 arc yano IPO対応(部品棚卸) ボタン名変更%> 
                     <%} %>
                    <% else{%>
                        <input id="InventoryEndButton" type="button" value="棚卸確定" disabled="disabled" style="visibility:hidden" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '4'; DisplayImage('UpdateMsg', '0'); formSubmit()"/>
                    <%} %>

                 <%// Mod 2017/07/27 arc yano #3781 %>
                 <% if (ViewData["DataEditButtonVisible"] != null && (bool)(ViewData["DataEditButtonVisible"]) == true)
                    {
                        if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("002") && !ViewData["InventoryStatusPartsBalance"].Equals("002"))
                        {
                 %>
                            <input id="InventoryDataEdit" type="button" value="新規作成" onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/PartsInventory/DataEdit?CreateFlag=1&InventoryMonth=' + document.getElementById('InventoryMonth').value + '&WarehouseCode=' + document.getElementById('WarehouseCode').innerText, '', '', 'no', 'no')""/><%//Mod 2017/12/18 arc yano #3838 %>
                       <%}else{ %>
                            <input id="InventoryDataEdit" type="button" value="新規作成" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); openModalAfterRefresh2('/PartsInventory/DataEdit?CreateFlag=1&InventoryMonth=' + document.getElementById('InventoryMonth').value + '&WarehouseCode=' + document.getElementById('WarehouseCode').innerText, '', '', 'no', 'no')""/><%//Mod 2017/12/18 arc yano #3838 %>
                        <%} %>
                   <%} %>
            <%} %>
        </td>
    </tr>
    <%// Mod 2015/06/16 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 実棚数を入力するための、補足メッセージ欄を追加 %>
    <tr>
        <th style="width: 100px"></th>
        <td style="white-space:nowrap" colspan ="3"> 
            ※実棚数には棚卸開始時点の在庫数を入力してください。
        </td>
    </tr>
</table>
<br />
<br />
<table class="input-form">
   <!--棚卸が確定または実施中なら検索エリアを使えるようにする -->
   <% if (ViewData["InventoryStatus"] != null && (ViewData["InventoryStatus"].Equals("002") || ViewData["InventoryStatus"].Equals("001")))
   {%>
    <tr>
        <th>ロケーションコード</th>
        <%//2015/06/03 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ 曖昧検索対応 %>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location','true')" })%>&nbsp;<img alt="ロケーション検索" id="LocationSearch" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog?BusinessType=002,009&WarehouseCode=' + document.getElementById('WarehouseCode').innerText)" /></td><%//Add 2017/07/27 arc yano #3781 %>        
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 35, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 25 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" id="PartsSearch" src="/Content/Images/Search.jpg" onclick="openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')" /></td>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 35, maxlength = 50 })%></td>  
    </tr>
    <tr>
        <th>棚差有無</th>
        <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.CheckBox("DiffQuantity", (ViewData["DiffQuantity"] != null) && (ViewData["DiffQuantity"].ToString().Contains("true")))%>棚差があるレコードのみ表示</td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <% //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 実棚、コメントを編集後に検索ボタンをクリックした場合は、メッセージを表示する%>
            <input id="SearchButton" type="button" value="検索" onclick="ClearValidateMessage('ValidBlock'); if (document.getElementById('EditFlag').value == 'true') { if (confirm('変更中の項目があります。\r\n保存を行わずに検索を実行すると、変更が破棄されます。\r\n処理を続行してもよろしいでしょうか')) { document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList() } } else { document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList() } "/>
            <input id="ClearButton" type="button" value="クリア" onclick = "ClearValidateMessage('ValidBlock'); resetCommonCriteria(new Array('LocationCode', 'LocationName', 'PartsNumber', 'PartsNameJp', 'DiffQuantity'));"/>
        </td>
    </tr>
    <%}else{ %>
    <tr>
            <th>ロケーションコード</th>
            <%//2015/06/03 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ 曖昧検索対応 %>
            <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "readonly", @readOnly = "readOnly", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location','true')" })%>&nbsp;<img alt="ロケーション検索" id="LocationSearch" style="cursor:pointer" src="/Content/Images/Search.jpg"/></td>        
            <th>ロケーション名</th>
            <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { @class = "readonly", @readOnly = "readOnly", size = 23, maxlength = 50 })%></td>
        </tr>
        <tr>
            <th>部品番号</th>
            <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "readonly", @readOnly = "readOnly", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" id="PartsSearch" style="cursor:pointer" src="/Content/Images/Search.jpg" /></td>
            <th>部品名</th>
            <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { @class = "readonly", @readOnly = "readOnly", size = 23, maxlength = 50 })%></td>  
        </tr>
        <tr>
            <th>棚差有無</th>
            <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.CheckBox("DiffQuantity", (ViewData["DiffQuantity"] != null) && (ViewData["DiffQuantity"].ToString().Contains("true")), new { @disabled = "disabled", @class = "disabled"})%>棚差があるレコードのみ表示</td>
    </tr>
        <tr>
            <th></th>
            <td colspan="3">
            <% //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 実棚、コメントを編集後に検索ボタンをクリックした場合は、メッセージを表示する%>
            <input id="SearchButton" type="button" value="検索" disabled="disabled" onclick="ClearValidateMessage('ValidBlock'); if (document.getElementById('EditFlag').value == 'true') { if (confirm('変更中の項目があります。\r\n保存を行わずに検索を実行すると、変更が破棄されます。\r\n処理を続行してもよろしいでしょうか')) { document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList() } } else { document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList() }"/>
            <input id="ClearButton" type="button" value="クリア" disabled="disabled" onclick = "resetCommonCriteria(new Array('LocationCode', 'LocationName', 'PartsNumber', 'PartsNameJp', 'DiffQuantity'));"/>
            </td>
        </tr>

    <%} %>
</table>

<%//Add 2014/12/15 arc yano IPO対応(部品検索) 処理中表示対応 %>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
</div>
<br />
<br />
<div id="pageBlock"><%//Add 2017/07/26 arc yano #3781 %>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
</div>
<br />
<br />


<%//Mod 2015/07/15 arc yano IPO対応(部品検索) 障害対応、仕様変更⑦ 理論在庫単価、理論在庫金額を追加 %>
<div id ="tableBlock"><%//Add 2017/07/26 arc yano #3781 %>
<table class="list" style ="width:100%">
    <tr>
        <%// Add 2017/07/27 arc yano #3781 %>
        <% if (ViewData["DataEditButtonVisible"] != null && (bool)(ViewData["DataEditButtonVisible"]) == true){
               if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("002") && !ViewData["InventoryStatusPartsBalance"].Equals("002"))
               {
        %>
                <th style ="white-space:nowrap"></th>
              <%} %>
        <%} %>
        <th style ="white-space:nowrap">ロケーションコード</th>
        <th style ="white-space:nowrap">ロケーション名</th>
        <th style ="white-space:nowrap">部品番号</th>
        <th style ="white-space:nowrap">部品名</th>
        <th style ="white-space:nowrap">理論数</th>
        <th style ="white-space:nowrap">単価</th>
        <th style ="white-space:nowrap">金額</th>
        <th style="white-space:nowrap">実棚数</th>
        <th style="white-space:nowrap">コメント</th>  
    </tr>
    <%int i = 0; %>
    <%foreach(var a in Model)
      {
          string namePrefix = string.Format("line[{0}].", i);
          string idPrefix = string.Format("line[{0}]_", i);
    %>
    <tr>
        <%// Add 2017/07/27 arc yano #3781 %>
        <% if (ViewData["DataEditButtonVisible"] != null && (bool)(ViewData["DataEditButtonVisible"]) == true){ 
               if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("002") && !ViewData["InventoryStatusPartsBalance"].Equals("002")){
        %>
                <td style ="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh2('/PartsInventory/DataEdit?LocationCode=' + document.getElementById('<%=idPrefix + "LocationCode"%>').value + '&PartsNumber=' + document.getElementById('<%=idPrefix + "PartsNumber"%>').value + '&InventoryMonth=' + document.getElementById('InventoryMonth').value + '&WarehouseCode=' + document.getElementById('WarehouseCode').innerText, '', '', 'no', 'yes')">編集</a></td><%//Mod 2017/12/18 arc yano #3838 %>
            <%} %>
        <%} %>
        <td><%=Html.Encode(a.LocationCode) %><%=Html.Hidden(namePrefix + "LocationCode", a.LocationCode, new { id = idPrefix + "LocationCode"}) %></td><%//ロケーションコード%>
        <td><%=Html.Encode(a.LocationName) %><%=Html.Hidden(namePrefix + "LocationName", a.LocationName, new { id = idPrefix + "LocationName"}) %></td><%//ロケーション名称%>
        <td><%=Html.Encode(a.PartsNumber) %><%=Html.Hidden(namePrefix + "PartsNumber", a.PartsNumber, new { id = idPrefix + "PartsNumber"}) %></td><%//部品番号%>
        <td><%=Html.Encode(a.PartsNameJp) %><%=Html.Hidden(namePrefix + "PartsNameJp", a.PartsNameJp, new { id = idPrefix + "PartsNameJp"}) %></td><%//部品名%>
        <%if (a.Quantity < 0 && (ViewData["InventoryStatus"] != null && !ViewData["InventoryStatus"].Equals("002"))) 
        { %>
            <td style="text-align:right"><font color="#ff0000"><%=Html.Encode(string.Format("{0:F1}", a.Quantity))%><%=Html.Hidden(namePrefix + "Quantity", a.Quantity, new { id = idPrefix + "Quantity"}) %></font></td><%//理論数%>
        <%}else{%>
            <td style="text-align:right"><%=Html.Encode(string.Format("{0:F1}",a.Quantity)) %><%=Html.Hidden(namePrefix + "Quantity", a.Quantity, new { id = idPrefix + "Quantity"}) %></td><%//理論数%>
        <%}%>
        <%//Mod 2015/07/15 arc yano %>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.PostCost)) %><%=Html.Hidden(namePrefix + "PostCost", a.PostCost, new { id = idPrefix + "PostCost"}) %></td><%//単価%>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.CalcAmount)) %><%=Html.Hidden(namePrefix + "CalcAmount", a.CalcAmount, new { id = idPrefix + "CalcAmount"}) %></td><%//金額%>

        <% if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("001"))
           {%>
                <%if(a.Quantity != a.PhysicalQuantity){%>
                <td style="text-align:right"><%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F1}",a.PhysicalQuantity), new { style="color:#ff0000",  @class = "numeric", id = idPrefix + "PhysicalQuantity", size = 9, maxlength = 9, onchange =" var ret = ChkPhysicalQuantity('"+ idPrefix +"PhysicalQuantity', '" + idPrefix + "SavedPhsycalQuantity'); if(ret){UpdateEditFlag('EditFlag', true)}"})%><%=Html.Hidden(namePrefix + "SavedPhsycalQuantity", string.Format("{0:F1}", a.PhysicalQuantity), new {id = idPrefix + "SavedPhsycalQuantity"})%></td><%//実棚%>
                <%}else{%>
                <td style="text-align:right" ><%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F1}",a.PhysicalQuantity), new { @class = "numeric", id = idPrefix + "PhysicalQuantity", size = 9, maxlength = 9, onchange ="var ret = ChkPhysicalQuantity('"+ idPrefix +"PhysicalQuantity', '" + idPrefix + "SavedPhsycalQuantity'); if(ret){UpdateEditFlag('EditFlag', true)}"})%><%=Html.Hidden(namePrefix + "SavedPhsycalQuantity", string.Format("{0:F1}", a.PhysicalQuantity), new {id = idPrefix + "SavedPhsycalQuantity"})%></td><%//実棚%>
            <%} %>
                <td><%=Html.TextBox(namePrefix + "Comment", a.Comment, new { id = idPrefix + "Comment", size = 15, maxlength = 120, onchange = "UpdateEditFlag('EditFlag', true)"})%></td><%//コメント%>
        <%  } else {%>
             <td style="text-align:right"><%=Html.TextBox(namePrefix + "PhysicalQuantity", string.Format("{0:F1}",a.PhysicalQuantity), new { @class = "readonly", @readonly = "readonly", id = idPrefix + "PhysicalQuantity", size = 9, maxlength = 9, style = "text-align:right"})%></td><%//実棚%>
                <td><%=Html.TextBox(namePrefix + "Comment", a.Comment, new { id = idPrefix + "Comment", @class = "readonly",　@readonly = "readonly", size = 15})%></td><%//コメント%>                
        <%  } %>
    </tr>
    <%
          i++;
    } %>
</table>
</div>
<%}%>
</asp:Content>
