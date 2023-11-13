<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetPartsStatusResult>>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    パーツステータス管理<%//Mod 2018/11/09 yano #3950 %>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
    //-----------------------------------------------------------------------------
    //　機能　：パーツステータス検索画面
    //　作成日：2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
    //  更新日：
    //          パーツステータス管理　画面タイトル誤り
    //          2017/11/06 arc yano #3809 パーツステータス管理　引当済数の追加
    //          
    //-----------------------------------------------------------------------------
%>

<%using (Html.BeginForm("Criteria", "PartsStatus", new { id = 0 }, FormMethod.Post))
{ 
%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DefaultTarget", ViewData["DefaultTarget"]) %><%//検索項目のデフォルト値(仕入日を選択) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<%=Html.Hidden("DefaultServiceOrderStatus", ViewData["DefaultServiceOrderStatus"]) %><%//伝票ステータスのデフォルト値 %>
<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
   <table class="input-form">
    <tr>
        <th style="width:80px" rowspan ="2">年月</th>
        <td colspan="3"><%=Html.RadioButton("Target", "0", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("0"))%>指定なし<%=Html.RadioButton("Target", "1", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("1"))%>入庫日<%=Html.RadioButton("Target", "2", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("2"))%>受注日<%=Html.RadioButton("Target", "3", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("3"))%>作業開始日<%=Html.RadioButton("Target", "4", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("4"))%>作業終了日<%=Html.RadioButton("Target", "5", ViewData["Target"] != null && ViewData["Target"].ToString().Equals("5"))%>納車日&nbsp;&nbsp;</td>     
    </tr>
    <tr>
        <td colspan="3"><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;&nbsp;</td>     
    </tr>
    <tr>
        <th style="width:80px">部門</th>
        <td colspan="3"><%=Html.TextBox("DepartmentCode",ViewData["DepartmentCode"],new {@class="alphanumeric", size = 15 , maxlength="3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>     
    </tr>
    <tr>
        <th style="width:80px">伝票ステータス</th>
        <td colspan="3"><%=Html.DropDownList("ServiceOrderStatus", (IEnumerable<SelectListItem>)ViewData["ServiceOrderStatusList"])%></td>
    </tr>
    <tr>
        <th>部品番号</th>
        <td colspan="3"><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')" /><span id="PartsNameJp"><%=Html.Encode(ViewData["PartsNameJp"]) %></span></td>
    </tr>
    <tr>
        <th style="width:80px"></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input id ="ListDownload" type="button" value="画面リスト出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStatus', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '99';"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('Target', 'TargetDateY', 'TargetDateM', 'DepartmentCode', 'DepartmentName', 'ServiceOrderStatus', 'PartsNumber', 'PartsNameJp'))"/>
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<table class="list">
    <tr> 
        <th style="white-space:nowrap; text-align:left">伝票番号</th>
        <th style="white-space:nowrap; text-align:left">伝票ステータス</th>
        <th style="white-space:nowrap; text-align:left">部品番号</th>
        <th style="white-space:nowrap; text-align:left">部品名</th>
        <th style="white-space:nowrap; text-align:left">数量</th>
        <th style="white-space:nowrap; text-align:left">引当済数</th><%//Add 2017/11/06 arc yano #3809 %>
        <th style="white-space:nowrap; text-align:left">入庫日</th>
        <th style="white-space:nowrap; text-align:left">受注日</th>
        <th style="white-space:nowrap; text-align:left">作業開始日</th>
        <th style="white-space:nowrap; text-align:left">作業終了日</th>
        <th style="white-space:nowrap; text-align:left">納車日</th>
    </tr>

    <%//検索結果を表示する。%>
    <% foreach (var item in Model){ %>
        
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipNumber)%></td>                                                                                       <%//伝票番号 %>
            <td style="white-space:nowrap"><%=Html.Encode(item.ServiceOrderStatusName)%></td>                                                                           <%//伝票ステータス %>
            <td style="white-space:nowrap"><%=Html.Encode(item.PartsNumber)%></td>                                                                                      <%//部品番号 %>
            <td style="white-space:nowrap"><%=Html.Encode(item.LineContents)%></td>                                                                                     <%//部品名 %>
            <td style="white-space:nowrap;  text-align:right"><%=Html.Encode(string.Format("{0:F2}",item.Quantity))%></td>                                              <%//数量%>
            <td style="white-space:nowrap;  text-align:right"><%=Html.Encode(string.Format("{0:F2}",item.ProvisionQuantity))%></td>                                     <%//引当済数%><%//Add 2017/11/06 arc yano #3809 %>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.ArrivalPlanDate))%></td>                                                  <%//入庫日%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesOrderDate))%></td>                                                   <%//受注日%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.WorkingStartDate))%></td>                                                 <%//作業開始日%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.WorkingEndDate))%></td>                                                   <%//作業終了日%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesDate))%></td>                                                        <%//納車日%>   
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
