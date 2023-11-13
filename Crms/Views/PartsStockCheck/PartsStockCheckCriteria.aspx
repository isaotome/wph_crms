<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.TotalPartsStockCheck>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品在庫確認
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Criteria", "PartsStockCheck", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<% //2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更%>
<%//Mod 2015/03/20 arc yano 部品在庫確認画面のレイアウトを全面的に変更%>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DefaultDispUnit", ViewData["DefaultDispUnit"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//Add 2016/07/12 arc yano #3599 処理の種別(各ボタンクリック時の処理の種類)%>
<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
    <table class="input-form">
    <tr>
        <th style="width:100px">対象年月 *</th>
        <td style="width:auto; white-space:nowrap"><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月</td>
        <th style="width:100px">在庫棚卸ステータス</th>
        <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 判定条件変更：Nullなら未実施　⇒　nullまたは空文字なら未実施-->
        <td style="white-space:nowrap; text-align:center; width:100px"><%=(Html.Encode(ViewData["InventoryStatusParts"] == null || ViewData["InventoryStatusParts"] == "" ? "未実施" : ViewData["InventoryStatusParts"] ))%></td><%//在庫棚卸ステータス"%>
        <th style="width:100px">算出日</th>
        <td style="white-space:nowrap; text-align:left; width:100px"><%=Html.Encode(Model.plist.Count < 1 ? "" : string.Format("{0:yyyy/MM/dd}", Model.plist[0].CalculatedDate))%></td><%//算出日"%>
    </tr>
    <tr>
        <th>表示単位 </th>
        <td style="white-space:nowrap" colspan ="5">
            <% //2016/07/12 arc yano #3599 %>
            <%=Html.RadioButton("DispUnit", "0", ViewData["DispUnit"] != null && ViewData["DispUnit"].ToString().Equals("0"), new { onclick = "SetDisabled('PartsNumber', true, ''); SetDisabled('PartsNameJp', true, ''); SetDisabled('WarehouseCode', false, null); SetDisabled('ExcelDownload', true, null); "} ) %>倉庫毎
            <%=Html.RadioButton("DispUnit", "1", ViewData["DispUnit"] != null && ViewData["DispUnit"].ToString().Equals("1"), new { onclick = "SetDisabled('PartsNumber', false, null); SetDisabled('PartsNameJp', false, null); SetDisabled('WarehouseCode', true, ''); document.getElementById('WarehouseName').innerText = ''; SetDisabled('ExcelDownload', true, null)"})%>部品毎
            <%=Html.RadioButton("DispUnit", "2", ViewData["DispUnit"] != null && ViewData["DispUnit"].ToString().Equals("2"), new { onclick = "SetDisabled('PartsNumber', false, null); SetDisabled('PartsNameJp', false, null); SetDisabled('WarehouseCode', false, null); SetDisabled('ExcelDownload', false, null)"})%>倉庫＋部品毎
        </td>
    </tr>
    <tr>
        <th>倉庫 </th>
        <td style="white-space:nowrap" colspan ="2">
            <%//Mod 2015/06/12 arc yano IPO対応(部品棚卸)障害対応、仕様変更② ルックアップの一覧に表示する部門を部品棚卸対象部門のみ表示する %>
            <% if (ViewData["DispUnit"].ToString().Equals("0") || ViewData["DispUnit"].ToString().Equals("2")) {%>
            <%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new {  maxlength = 6, style = "width:80px", onblur = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')"})%>&nbsp;<img alt="倉庫検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('WarehouseCode').disabled != true) {openSearchDialog('WarehouseCode','WarehouseName','/Warehouse/CriteriaDialog')}" />
            
            <%}else{ %>
                <%=Html.TextBox("WarehouseCode", ViewData["WarehouseCode"], new {  disabled = true , maxlength = 6, style = "width:80px", onblur = "GetNameFromCode('WarehouseCode','WarehouseName','Warehouse')"})%>&nbsp;<img alt="倉庫検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('WarehouseCode').disabled != true) {openSearchDialog('WarehouseCode','WarehouseName','/Warehouse/CriteriaDialog')}" />
            <%} %>
             <span id="WarehouseName" style ="width:160px"><%=Html.Encode(ViewData["WarehouseName"]) %></span>
        </td>
        <th>部品番号</th>
        <% if (ViewData["DispUnit"].ToString().Equals("1") || ViewData["DispUnit"].ToString().Equals("2")){ %> <%//表示単位が「部品毎」または「倉庫＋部品毎」 %>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="if(document.getElementById('PartsNumber').disabled != true){openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')}" /></td>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 23, maxlength = 50 })%></td>  
        <%}else{ %>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { disabled = true, @class = "alphanumeric", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="if(document.getElementById('PartsNumber').disabled != true){openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')}" /></td>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { disabled = true, size = 23, maxlength = 50 })%></td>  
        <%} %>
    </tr>
    <tr>
         <th></th>
        <td colspan="6">
        <% //2016/07/12 arc yano #3599 部品在庫確認　Excel出力機能追加%>
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        
        <%if (ViewData["DispUnit"].ToString().Equals("2")){ //表示単位=「倉庫＋部品毎」の場合%> 
            <input id ="ExcelDownload" type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/>
        <%}else{ %>
            <input id ="ExcelDownload" type="button" disabled ="disabled" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/>
        <%} %>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'WarehouseCode', 'WarehouseName', 'PartsNumber', 'PartsNameJp', 'DispUnit')); SetDisabled('PartsNumber', true, ''); SetDisabled('PartsNameJp', true, ''); SetDisabled('WarehouseCode', false, null); SetDisabled('ExcelDownload', true, null)"/><% //2016/07/12 arc yano #3599 %>
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.plist.PageProperty); %>
<br />
<br />

<% //2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更%>
<% //2015/07/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ レイアウト変更(引当在庫列、単価、数量を追加) %>
<% //2015/06/03 arc yano IPO対応(部品棚卸) 障害対応、仕様変更③ レイアウト変更(理論在庫列を追加、列の並び替え(単価差額→理論在庫→棚差→月末在庫の順にする)) %>
<table class="list">
    <tr> 
    <% if (ViewData["DispUnit"].ToString().Equals("0")){ %> <%//表示単位が【倉庫毎】%>
        <th style="white-space:nowrap">倉庫コード</th>
        <th style="white-space:nowrap">倉庫名</th>
        <th style="white-space:nowrap">月初在庫</th>
        <th style="white-space:nowrap">当月仕入</th>
        <th style="white-space:nowrap">当月移動受入</th>
        <th style="white-space:nowrap">当月納車</th>
        <th style="white-space:nowrap">当月移動払出</th>
        <th style="white-space:nowrap">単価差額</th>
        <th style="white-space:nowrap">理論在庫</th><%//Add 2015/06/03 arc yano %>
        <th style="white-space:nowrap">棚差</th>
        <th style="white-space:nowrap">月末在庫</th>
        <th style="white-space:nowrap">(引当在庫)</th><%//Add 2015/07/17 arc yano %>
        <th style="white-space:nowrap">(仕掛在庫)</th>
    <%} %>
    <% else{ //表示単位が【倉庫毎】または【倉庫＋部品毎】%>
        <% if (ViewData["DispUnit"].ToString().Equals("2")){ %> <%//表示単位が【部門＋部品毎】の場合は部門も表示%>
            <th style="white-space:nowrap" rowspan="2">倉庫コード</th>
            <th style="white-space:nowrap" rowspan="2">倉庫名</th>
        <%} %>

        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" rowspan="2">部品番号</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" rowspan="2">部品名</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="3">月初在庫</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">当月仕入</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">当月移動受入</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">当月納車</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">当月移動払出</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" rowspan="2">単価差額</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">理論在庫</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="3">棚差</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="3">月末在庫</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">(引当在庫)</th>
        <th style="white-space:nowrap; border-left-width:2px; border-right-width:2px" colspan="2">(仕掛在庫)</th>
    <%} %>
    </tr>

    <% if (ViewData["DispUnit"].ToString().Equals("1") || ViewData["DispUnit"].ToString().Equals("2")){ //-------------------表示単位が【部品毎】または【部門＋部品毎】--------------------%>
        <tr>
           <%//月初在庫%>
           <th style="white-space:nowrap; border-left-width:2px">単価</th>                            <%//単価 %>
           <th style="white-space:nowrap">数量</th>                                                   <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//当月仕入%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//当月移動受入%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
    　     
           <%//当月納車%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//当月移動払出%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//単価差額はなし%>

           <%//理論在庫%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//棚差%>
           <th style="white-space:nowrap; border-left-width:2px">単価</th>                            <%//単価 %>
           <th style="white-space:nowrap">数量</th>                                                   <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//月末在庫%>
           <th style="white-space:nowrap; border-left-width:2px">単価</th>                            <%//単価 %>
           <th style="white-space:nowrap">数量</th>                                                   <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//引当在庫%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
           
           <%//仕掛在庫%>
           <th style="white-space:nowrap; border-left-width:2px">数量</th>                            <%//数量 %>
           <th style="white-space:nowrap; border-right-width:2px">金額</th>                           <%//金額 %>
        </tr>
    <% } %>

    <%//検索結果を表示する。部品毎または、倉庫＋部品毎の場合は単価、数量、金額を縦で表示%>
    <% foreach (var item in Model.plist){ %>
    <%
    %>       
    <% if (ViewData["DispUnit"].ToString().Equals("0")){ //-------------------表示単位が【倉庫毎】--------------------%>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.WarehouseCode)%></td>                                                                                        <%//倉庫コード%>
            <td style="white-space:nowrap"><%=Html.Encode(item.WarehouseName)%></td>                                                                                        <%//倉庫名称%> 
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.PreAmount))%></td>                                                  <%//月初在庫%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.PurchaseAmount))%></td>                                             <%//当月仕入%>    
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.TransferArrivalAmount))%></td>                                      <%//当月移動受入金額%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.ShipAmount))%></td>                                                 <%//当月納車%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.TransferDepartureAmount))%></td>                                    <%//当月移動払出金額%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.UnitPriceDifference))%></td>                                        <%//単価差額%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.CalculatedAmount))%></td>                                           <%//理論在庫%><%//Add 2015/06/03 arc yno %>
            <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④  受け払いステータスが確定の時だけ棚差と月末在庫を表示する-->
            <%if (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")){ %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.DifferenceAmount))%></td>                                       <%//棚差%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.PostAmount))%></td>                                             <%//月末在庫%>
            <%}else{%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                                  <%//棚差%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                                  <%//月末在庫%>
            <%} %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.ReservationAmount))%></td>                                          <%//引当在庫%><%//Add 2015/07/17 arc yano %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.InProcessAmount))%></td>                                            <%//仕掛在庫%>
        </tr>        
    <%} %>    
    <%else{ //--------表示単位が【部品毎】または【倉庫＋部品毎】------------------%>
        <tr>
            <% if (ViewData["DispUnit"].ToString().Equals("2")){ //表示単位が【部門＋部品毎】の場合は部門も表示する%>
                <td style="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(item.WarehouseCode)%></td>                                     <%//倉庫コード%>
                <td style="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(item.WarehouseName)%></td>                                     <%//倉庫名称%>
            <%} %>

            <td style="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(item.PartsNumber)%></td>                                           <%//倉庫コード%>
            <td style="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(item.PartsNameJp == null ? "" : item.PartsNameJp)%></td>           <%//倉庫名称%>

            <%//月初在庫%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PreCost))%></td>                             <%//単価 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.PreQuantity))%></td>                                                <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PreAmount))%></td>                          <%//金額 %>

            <%//当月仕入%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PurchaseQuantity))%></td>                                           <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PurchaseAmount))%></td>                                             <%//金額 %>

            <%//当月移動受入%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.TransferArrivalQuantity))%></td>                                    <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.TransferArrivalAmount))%></td>                                      <%//金額 %>

            <%//当月納車%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.ShipQuantity))%></td>                                               <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.ShipAmount))%></td>                                                 <%//金額 %>

            <%//当月移動払出%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.TransferDepartureQuantity))%></td>                                  <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.TransferDepartureAmount))%></td>                                    <%//金額 %>

            <%//単価差額%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.UnitPriceDifference))%></td>                                        <%//金額 %>

            <%//理論在庫%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.CalculatedQuantity))%></td>                                         <%//数量 %>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.CalculatedAmount))%></td>                                           <%//金額 %>

            <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④  受け払いステータスが確定の時だけ棚差と月末在庫を表示する-->
            <%if (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002")){ %>
                <%//棚差%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PostCost))%></td>                                               <%//単価 %>                                           
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.DifferenceQuantity))%></td>                                                             <%//数量 %>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.DifferenceAmount))%></td>                                       <%//金額 %>
                
                <%//月末在庫%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PostCost))%></td>                                               <%//単価 %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.PostQuantity))%></td>                                                                   <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.PostAmount))%></td>                                             <%//金額%>
            <%}else{ //棚卸が終了していない場合は、棚差、月末在庫は表示しない%>
                <%//棚差%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode("")%></td>                                                                                  <%//単価%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                                                          <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode("")%></td>                                                                                  <%//金額%>
    
                <%//月末在庫%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode("")%></td>                                                                                  <%//単価%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                                                          <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode("")%></td>                                                                                  <%//金額%>
            <%} %>

            <%//引当在庫%><%//Add 2015/07/17 arc yano %>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.ReservationQuantity))%></td>                                        <%//数量%>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.ReservationAmount))%></td>                                          <%//金額%>
            
            <%//仕掛在庫%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.InProcessQuantity))%></td>                                          <%//数量%>                                          
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",item.InProcessAmount))%></td>                                            <%//金額%>                                              
        </tr>
    <%} %>
    <%} %>
    <%
        /*------------------------------------*/
        /*             合計金額　             */
        /*------------------------------------*/
    %>       
    <% if (ViewData["DispUnit"].ToString().Equals("0")){ //-------------------表示単位が【倉庫毎】-----------------------%>
        <tr>
            <th style="border-right-style:none" colspan ="1"></th>
            <th style="border-left-style:none">合計金額</th>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPreAmount))%></td>                    <%//月初在庫の合計%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPurchaseAmount))%></td>               <%//当月仕入の合計%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferArrivalAmount))%></td>    <%//当月移動受入金額の合計%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalShipAmount))%></td>                   <%//当月納車の合計%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferDepartureAmount))%></td>  <%//当月移動払出金額の合計%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalUnitPriceDifference))%></td>          <%//単価差額の合計%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalCalculatedAmount))%></td>             <%//理論在庫の合計%><%//Add 2015/06/03 arc yno %>
            <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④  受け払いステータスが確定の時だけ棚差と月末在庫を表示する-->
            <%if (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002"))
                { %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalDifferenceAmount))%></td>         <%//棚差の合計%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPostAmount))%></td>               <%//月末在庫の合計%>
            <%}else{%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                          <%//棚差%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                          <%//月末在庫%>
            <%} %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalReservationAmount))%></td>            <%//引当在庫の合計%><%//Add 2015/07/17 arc yano %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalInProcessAmount))%></td>              <%//仕掛在庫の合計%>
        </tr>    
    <%} %>
    <%else
       { //--------表示単位が【部品毎】または【倉庫＋部品毎】------------------%>
         <tr>
            <% if (ViewData["DispUnit"].ToString().Equals("2")){ %> <%//表示単位が「倉庫＋部品毎」 %>
                <th style="border-right-style:none" colspan ="3"></th>
            <%} %>
            <%else{ %>
                <th style="border-right-style:none" colspan ="1"></th>
            <%} %>

             <th style="white-space:nowrap; border-left-style:none">合計金額</th>
            
                <%//月初在庫の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px">-</td>                                                                                     <%//単価 %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPreQuantity))%></td>                      <%//数量%> 
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPreAmount))%></td>                        <%//金額%>

             　 <%//当月仕入の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPurchaseQuantity))%></td>                 <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPurchaseAmount))%></td>                   <%//金額%>

                <%//当月移動受入金額の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferArrivalQuantity))%></td>          <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferArrivalAmount))%></td>            <%//金額%>

                <%//当月納車の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalShipQuantity))%></td>                     <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalShipAmount))%></td>                       <%//金額%>

                <%//当月移動払出金額の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferDepartureQuantity))%></td>        <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalTransferDepartureAmount))%></td>          <%//金額%>

                <%//単価差額の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalUnitPriceDifference))%></td>            　<%//金額%>
                
                <%//理論在庫の合計%><%//Add 2015/06/03 arc yno %>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalCalculatedQuantity))%></td>               <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalCalculatedAmount))%></td>                 <%//金額%>
                
                             
            <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④  受け払いステータスが確定の時だけ棚差と月末在庫を表示する-->
            <%if (ViewData["inventoryStatusPartsBalance"] != null && ViewData["inventoryStatusPartsBalance"].Equals("002"))
              { %>
                <%//棚差の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px">-</td>                                                                                     <%//単価 %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalDifferenceQuantity))%></td>               <%//数量 %>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalDifferenceAmount))%></td>                 <%//金額 %>  
                
                <%//月末在庫の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px">-</td>                                                                                     <%//単価 %>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPostQuantity))%></td>                     <%//数量 %>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalPostAmount))%></td>                       <%//金額 %>
                            
            <%}else{%>
                <%//棚差の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode("")%></td>                                                                  <%//単価%>
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                  <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode("")%></td>                                                                  <%//金額%>
                <%//月末在庫の合計%>
                <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode("")%></td>                                                                  <%//単価%> 
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode("")%></td>                                                                  <%//数量%>
                <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode("")%></td>                                                                  <%//金額%>
            <%} %>

            <%//引当在庫の合計%><%//Add 2015/07/17 arc yano %>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalReservationQuantity))%></td>                  <%//数量%>
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalReservationAmount))%></td>                    <%//金額%>
             
            <%//仕掛在庫の合計%>
            <td style="white-space:nowrap; text-align:right; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalInProcessQuantity))%></td>                    <%//数量%>                    
            <td style="white-space:nowrap; text-align:right; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}",Model.TotalInProcessAmount))%></td>                      <%//金額%>
        </tr>    
        <%} %>
</table>
        <%} %>

</asp:Content>
