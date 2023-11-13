<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CommonPartsStock>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
部品在庫検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%using (Html.BeginForm("Criteria", "PartsStock", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("NowTargetDateY", ViewData["NowTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("NowTargetDateM", ViewData["NowTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DepartmentIvnentoryStatus", ViewData["DepartmentIvnentoryStatus"]) %><%//部門毎の棚卸ステータス%>
<%=Html.Hidden("AllIvnentoryStatus", ViewData["AllIvnentoryStatus"]) %><%//全部門の棚卸ステータス%>
<%=Html.Hidden("ToDate", ViewData["ToDate"]) %><%//当日の日付%>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//処理の種別(各ボタンクリック時の処理の種類)%>
<%=Html.Hidden("DefaultTargetRange", ViewData["DefaultTargetRange"]) %>
<%=Html.Hidden("DefaultStockZeroVisibility", ViewData["DefaultStockZeroVisibility"]) %>     <%//Add 2015/03/11 arc yano #3160 %>

<br/>
<br/>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<%=Html.ValidationSummary() %>
<table class="input-form">
    <% //Mod 2015/03/10 arc yano #3160 部品在庫検索画面レイアウト変更(対象年月指定欄追加)%>
    <% // Del 2015/04/23 arc nakayama 部品在庫検索画面見直し　棚卸に関する検索項目削除(旧ソースは古いリビジョンを参照して下さい) %>
    <tr>
        <th>対象年月指定 </th>
        <td style="width:180px; white-space:nowrap">
            <%=Html.RadioButton("TargetRange", "0", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("0"), new { id = "TargetRange",   onclick = "CheckTargetRange(0)"})%>なし
            <%=Html.RadioButton("TargetRange", "1", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("1"), new { id = "TargetRange2" , onclick = "CheckTargetRange(1)"})%>あり
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
        <th>部門 </th>
        <td style="width:180px; white-space:nowrap">
            <%//Mod 2015/03/02 arc yano 部門検索ダイアログの絞り込みをやめる%>
            <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new {  maxlength = 3, style = "width:40px", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('DepartmentCode').readOnly == false) {openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')}" />
        </td>
         <th>部門名 </th>
        <td style="width:180px; white-space:nowrap">
            <span id="DepartmentName" style ="width:160px"><%=Html.Encode(ViewData["DepartmentName"]) %></span>
        </td>
    </tr>
 
    <tr>
        <% //Mod 2015/03/11 arc yano #3160 部門コードによる絞り込み追加%>
        <th>ロケーションコード</th>
        <td><%=Html.TextBox("LocationCode", ViewData["LocationCode"], new { @class = "alphanumeric", size = 12 , maxlength = 12, onchange = "GetNameFromCode('LocationCode','LocationName','Location')" })%>&nbsp;<img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('LocationCode','LocationName','/Location/CriteriaDialog?BusinessType=002,009&DepartmentCode=' + document.getElementById('DepartmentCode').value)" /></td>        
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 23, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>部品番号</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')" /></td>
        <th>部品名</th>
        <td><%=Html.TextBox("PartsNameJp", ViewData["PartsNameJp"], new { size = 23, maxlength = 50 })%></td>  
    </tr>
    <tr>
        <th>在庫ゼロ表示</th>
        <td colspan="3" style="width:350px; white-space:nowrap"><%=Html.RadioButton("StockZeroVisibility", "0", ViewData["StockZeroVisibility"])%>しない<%=Html.RadioButton("StockZeroVisibility", "1", ViewData["StockZeroVisibility"])%>する</td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '5'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '5';"/><% //Mod 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加%>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetRange', 'TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'LocationCode', 'LocationName', 'PartsNumber', 'PartsNameJp', 'StockZeroVisibility')); CheckTargetRange(0);"/>
        <%//Add 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正）データ編集ボタン追加%>
        <%if(bool.Parse(ViewData["EditButtonVisible"].ToString()).Equals(true)){ %>
            <%if (ViewData["PartsStockSearch"] != null && bool.Parse(ViewData["PartsStockSearch"].ToString()).Equals(true)){ %>
                <input id="EditButton" type="button" value="新規作成" onclick="openModalAfterRefresh('/PartsStock/DataEdit?CreateFlag=1', '', '', 'no', 'no')" />
            <%}else{ %>
                <input id="EditButton" type="button" value="新規作成" disabled ="disabled" onclick="openModalAfterRefresh('/PartsStock/DataEdit?CreateFlag=1', '', '', 'no', 'no')" />
            <%} %>
        <%} %>
        </td>
    </tr>
</table>
<br />
<br />
<%// Del 2015/04/23 arc nakayama 部品在庫検索画面見直し　棚卸機能カット %>
<%//Add 2014/12/15 arc yano IPO対応(部品検索) 処理中表示対応 %>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
</div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<%//Mod 2016/06/24 arc yano #3591 部品在庫検索　過去月検索時の一覧の数量の表示%>
<%//Mod 2016/02/01 arc yano #3409 フリー在庫表示対応%>
<%//Mod 2015/03/11 arc yano #3160(部門コードの任意入力にする) 現在庫を検索する場合は棚卸情報は表示しない %>
<%//Mod 2015/02/12 arc yano IPO対応(部品在庫検索) 棚卸情報、在庫情報のタイトル行の追加 %>
<%// Del 2015/04/23 arc nakayama 部品在庫検索画面見直し　棚卸に関する表示項目削除(旧ソースは古いリビジョンを参照して下さい) %>
<table class="list" style ="width:100%">
    <tr>
        <%if(bool.Parse(ViewData["EditButtonVisible"].ToString()).Equals(true)){ %>
            <%if (ViewData["PartsStockSearch"] != null && bool.Parse(ViewData["PartsStockSearch"].ToString()).Equals(true)){ %>
                <th></th>
            <%} %>
        <%} %>
        <th style ="white-space:nowrap">部門名</th>
        <th style ="white-space:nowrap">ロケーションコード</th>
        <th style ="white-space:nowrap">ロケーション名</th>
        <th style ="white-space:nowrap">部品番号</th>
        <th style ="white-space:nowrap">部品名</th>
        <th style ="white-space:nowrap">数量</th>
        <th style ="white-space:nowrap">(引当済数)</th>
        <th style ="white-space:nowrap">(フリー在庫数)</th>
        <th style ="white-space:nowrap">標準原価</th>
        <th style ="white-space:nowrap">移動平均単価</th>
        <th style ="white-space:nowrap">金額</th>
     </tr>
    <%int i = 0; %>
    <%
        foreach(var a in Model){
            string namePrefix = string.Format("line[{0}].", i);
            string idPrefix = string.Format("line[{0}]_", i);  
    %>
    <tr>
        <%if(bool.Parse(ViewData["EditButtonVisible"].ToString()).Equals(true)){ %>
            <%if (ViewData["PartsStockSearch"] != null && bool.Parse(ViewData["PartsStockSearch"].ToString()).Equals(true)){ %>
                <td style ="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/PartsStock/DataEdit?LocationCode=' + document.getElementById('<%=idPrefix + "LocationCode"%>').value + '&PartsNumber=' + document.getElementById('<%=idPrefix + "PartsNumber"%>').value + '&DepartmentCode=' + document.getElementById('<%=idPrefix + "DepartmentCode"%>').value, '', '', 'no', 'no')">編集</a></td>
            <%} %>
        <%} %>
        <td><%=Html.Encode(a.DepartmentName) %></td><%//部門名%>
        <td><%=Html.Encode(a.LocationCode) %> <%=Html.Hidden(namePrefix + "LocationCode" , a.LocationCode, new {id = idPrefix + "LocationCode"})%></td><%//ロケーションコード%>
        <td><%=Html.Encode(a.LocationName) %></td><%//ロケーション名称%>
        <td><%=Html.Encode(a.PartsNumber) %><%=Html.Hidden(namePrefix + "PartsNumber" , a.PartsNumber, new {id = idPrefix + "PartsNumber"})%></td><%//部品番号%>
        <td><%=Html.Encode(a.PartsNameJp) %></td><%//部品名%>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode(string.Format("{0:F1}",(ViewData["TargetRange"].ToString().Equals("1") ? (a.PhysicalQuantity ?? 0) : (a.Quantity ?? 0) ))) %></td><%//数量%><%//Mod 2016/06/24 arc yano #3591 対象月の指定がある場合は実棚数を指定が無い場合は理論数を表示%>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode((!string.IsNullOrWhiteSpace(a.LocationType) && a.LocationType.Equals("003")) ? "-" : string.Format("{0:F1}",(a.ProvisionQuantity ?? 0))) %></td><%//引当済数%><% //Mod 2016/02/01 arc yano #3409 %>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode((!string.IsNullOrWhiteSpace(a.LocationType) && a.LocationType.Equals("003")) ? "-" : string.Format("{0:F1}",(ViewData["TargetRange"].ToString().Equals("1") ? (a.PhysicalQuantity ?? 0) : (a.Quantity ?? 0)) - (a.ProvisionQuantity ?? 0))) %></td><%//フリー在庫数%><% //Mod 2016/02/01 arc yano #3409 %><%//Mod 2016/06/24 arc yano #3591 対象月の指定がある場合は実棚数を指定が無い場合は理論数を表示%>
        <%//Mod 2015/02/12 arc yano IPO対応(部品在庫検索) 標準原価は棚卸ステータスに関わらず必ず表示する。 %>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode(a.StandardPrice != null ? string.Format("{0:N0}", a.StandardPrice) : "")%></td><%//標準原価%>
        <%//2018/05/14 arc yano #3880  %>
        <%//Mod 2015/03/20 arc iijma TargetRangeにて判定、金額は全表示。 %>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",a.MoveAverageUnitPrice))%></td>
        <%//<td style="text-align:right;white-space:nowrap"><%=Html.Encode(ViewData["TargetRange"].Equals("1") ? string.Format("{0:N0}",a.MoveAverageUnitPrice) : "")</td>%><%//移動平均単価%>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}",a.Price))%></td><%//金額%>
        <%=Html.Hidden(namePrefix + "DepartmentCode" , a.DepartmentCode, new {id = idPrefix + "DepartmentCode"})%>     <%//Add 2018/06/01 arc yano #3900 %>
    </tr>
    <%
            i++;
    } %>
</table>
<%}%>
</asp:Content>
