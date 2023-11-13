<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetStockCarListResult>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
車両在庫検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%//-------------------------------------------------------------------- 
  // 機能　：車両在庫検索
  // 作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
  //
  //-------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "CarStockList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("NowTargetDateY", ViewData["NowTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("NowTargetDateM", ViewData["NowTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//処理の種別(各ボタンクリック時の処理の種類)%>
<%=Html.Hidden("DefaultTargetRange", "0") %>
<%=Html.Hidden("DefaultStockZeroVisibility", "0") %>
<%=Html.Hidden("InventoryMonth", ViewData["InventoryMonth"]) %>

<br/>
<br/>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<%//Mod 2020/1/20 yano #4033 %>
<span id ="ValidationMessage">
<%=Html.ValidationSummary() %>
</span>
<table class="input-form">

    <tr>
        <th>対象年月指定 </th>
        <td style="width:180px; white-space:nowrap">
            <%=Html.RadioButton("TargetRange", "0", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("0"), new { onclick = "CheckTargetRange(0); SetDisabledAll(new Array('ListDownload'), 'StockZeroVisibility', true ); resetCommonCriteria( new Array('StockZeroVisibility', 'ValidationMessage'))"})%>なし<%//Mod 2020/1/20 yano #4033%>
            <%=Html.RadioButton("TargetRange", "1", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("1"), new { onclick = "CheckTargetRange(1); SetDisabledAll(new Array('ListDownload'), 'StockZeroVisibility', false )"})%>あり <%//Mod 2020/1/20 yano #4033%>
        </td>
        <th style="width: 100px">棚卸対象年月</th>
        <td style="width: 180px; white-space: nowrap" colspan="3">
            <%if (ViewData["TargetRange"] != null && (ViewData["TargetRange"].ToString()).Equals("0"))
              { %>
            <%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"], new { disabled = true  })%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"],new { disabled = true  })%>&nbsp;月&nbsp;
            <% }else{%>
            <%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;
            <%} %>
        </td>
    </tr>
    <tr>
        <th>部門コード (*)</th>
        <td style="width:180px; white-space:nowrap">
            <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, style = "width:40px", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department'); var con = true; if(this.value != ''){con = false;}   SetDisabledAll(new Array('ExcelDownload'), null, con)"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('DepartmentCode').readOnly == false) {openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CarInventoryFlag=1')}" />
        </td>
         <th>部門名 </th>
        <td style="width:180px; white-space:nowrap">
            <span id="DepartmentName" style ="width:160px"><%=Html.Encode(ViewData["DepartmentName"]) %></span>
        </td>
    </tr>
 
    <tr>
        <th>ロケーションコード</th>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location')" })%>&nbsp;<img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog?BusinessType=001,009&DepartmentCode=' + document.getElementById('DepartmentCode').value)" /></td>        
        <th>ロケーション名</th>
        <td style="width:180px; white-space:nowrap">
            <span id="LocationName" style ="width:160px"><%=Html.Encode(ViewData["LocationName"]) %></span>
        </td>
        
    </tr>
    <tr>
        <th>管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 15 , maxlength = 25　})%></td>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength = 50 })%></td>  
    </tr>
    <tr>
        <th>新中区分</th>
        <td><%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
        <th>在庫区分</th>
        <td><%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["CarStatusList"])%></td>
    </tr>
    <tr>
        <th>ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 15 , maxlength = 25　})%></td>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { size = 23, maxlength = 50 })%></td>  
    </tr>
    <tr>
        <th>グレード</th>
        <td><%=Html.TextBox("CarGradeName", ViewData["CarGradeName"], new { size = 15 , maxlength = 25　})%></td>
        <th>車両登録番号</th>
        <td><%=Html.TextBox("RegistrationNumber", ViewData["RegistrationNumber"], new { size = 15 , maxlength = 25　})%></td>
    </tr>
    <tr>
        <th>在庫有無</th>
        <%if(ViewData["TargetRange"].ToString().Equals("0")){ %>
            <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.RadioButton("StockZeroVisibility", "0", ViewData["StockZeroVisibility"].ToString().Equals("0"), new { @disabled = "disabled"})%>全て<%=Html.RadioButton("StockZeroVisibility", "1", ViewData["StockZeroVisibility"].ToString().Equals("1"), new { @disabled = "disabled"})%>有<%=Html.RadioButton("StockZeroVisibility", "2", ViewData["StockZeroVisibility"].ToString().Equals("2"), new { @disabled = "disabled"})%>無</td>
        <%}else{ %>
            <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.RadioButton("StockZeroVisibility", "0", ViewData["StockZeroVisibility"].ToString().Equals("0"))%>全て<%=Html.RadioButton("StockZeroVisibility", "1", ViewData["StockZeroVisibility"].ToString().Equals("1"))%>有<%=Html.RadioButton("StockZeroVisibility", "2", ViewData["StockZeroVisibility"].ToString().Equals("2"))%>無</td>
        <%} %>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <% if (ViewData["DepartmentName"] != null && !string.IsNullOrWhiteSpace(ViewData["DepartmentName"].ToString())){%>
            <input type="button" value="棚卸表出力" id="ExcelDownload" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/>
        <%}else{ %>
            <input type="button" value="棚卸表出力" id="ExcelDownload" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';" disabled ="diabled"/>
        <%} %>
        <%//Add 2020/1/20 yano #4033 %>
         <% if (ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("1"))
            {%>
            <input type="button" value="棚卸リスト出力" id="ListDownload" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/>
        <%}else{ %>
            <input type="button" value="棚卸リスト出力" id="ListDownload" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';" disabled ="diabled"/>
        <%} %>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetRange', 'TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'LocationCode', 'LocationName', 'SalesCarNumber', 'Vin', 'NewUsedType', 'CarStatus', 'CarBrandName', 'CarName', 'CarGradeName', 'RegistrationNumber', 'StockZeroVisibility', 'ValidationMessage')); CheckTargetRange(0); SetDisabledAll(new Array('ExcelDownload', 'ListDownload'), 'StockZeroVisibility', true)"/><%//Mod 2020/1/20 yano #4033%>
        </td>
    </tr>
</table>
<br />
<br />

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
</div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<table class="list" style ="width:100%">
    <tr>
        <th style ="white-space:nowrap">ロケーション名</th>
        <th style ="white-space:nowrap">管理番号</th>
        <th style ="white-space:nowrap">車台番号</th>
        <th style ="white-space:nowrap">新中区分</th>
        <th style ="white-space:nowrap">在庫区分</th>
        <th style ="white-space:nowrap">ブランド名</th>
        <th style ="white-space:nowrap">車種名</th>
        <th style ="white-space:nowrap">グレード</th>
<% if( ViewData["InventoryMonth"] == null){ %>
        <th style ="white-space:nowrap">販売原価</th>
<%} %>
        <th style ="white-space:nowrap">車両登録番号</th>
<% if( ViewData["InventoryMonth"] != null){ %>
        <th style ="white-space:nowrap">理論値</th>
        <th style ="white-space:nowrap">実棚</th>
<%} %>
        <th style ="white-space:nowrap">備考</th>
     </tr>

    <%foreach(var a in Model){%>
    <tr>
        <td style ="white-space:nowrap"><%=Html.Encode(a.LocationName) %></td><%//ロケーション名%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.SalesCarNumber) %></td><%//管理番号%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.Vin) %></td><%//車台番号%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.NewUsedTypeName) %></td><%//新中区分%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CarStatusName) %></td><%//在庫区分%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CarBrandName) %></td><%//ブランド名%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CarName) %></td><%//車種名%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CarGradeName) %></td><%//グレード名%>
        <% if( ViewData["InventoryMonth"] == null){ %>
            <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", a.SalesPrice)) %></td><%//販売価格%>
        <%} %>
        <td style ="white-space:nowrap;"><%=Html.Encode(a.RegistrationNumber) %></td><%//車両登録番号%>
        <% if( ViewData["InventoryMonth"] != null){ %>
            <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F0}", a.Quantity)) %></td><%//理論値%>
            <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F0}", a.PhysicalQuantity)) %></td><%//実棚%>
        <%} %>
        <td style ="white-space:nowrap"><%=Html.Encode(a.Summary) %></td><%//備考%>
    </tr>
    <%
    } %>
</table>
<%}%>
</asp:Content>
