<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetAntiqueLedgerListResult>>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    古物台帳
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
    //--------------------------------------------------------------------------------
    //　機能　：古物台帳検索画面
    //　作成日：2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
    //
    //
    //-------------------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "AntiqueLedger", new { id = 0 }, FormMethod.Post))
{ 
%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %><%//日付のデフォルト値(当月の年が入る) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %><%//日付のデフォルト値(当月の月が入る) %>
<%=Html.Hidden("DefaultSearched", ViewData["DefaultSearched"]) %><%//検索項目のデフォルト値(仕入日を選択) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
   <table class="input-form">
     <tr>
        <th style="width:80px">年月</th>
        <td colspan="3"><%=Html.RadioButton("Searched", "0", ViewData["Searched"] != null && ViewData["Searched"].ToString().Equals("0"))%>仕入日<%=Html.RadioButton("Searched", "1", ViewData["Searched"] != null && ViewData["Searched"].ToString().Equals("1"))%>納車日&nbsp;<%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月</td>     
    </tr>
     <tr>
        <th style="width:80px"></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '99'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input id ="ListDownload" type="button" value="画面リスト出力" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('AntiqueLedger', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '99';"/>
        <input id ="AntiqueLedgerDownload" type="button" value="古物台帳出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('AntiqueLedger', 'UpdateMsg'); displaySearchList(); document.getElementById('RequestFlag').value = '99';"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateY', 'TargetDateM', 'Searched'))"/><% //2016/07/12 arc yano #3599 %>
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
        <th colspan ="9" style="white-space:nowrap; text-align:left">取引した古物</th>
        <th colspan ="4" style="white-space:nowrap; text-align:left">確認</th>
        <th colspan ="6" style="white-space:nowrap; text-align:left">仕入元</th>
        <th colspan ="7" style="white-space:nowrap; text-align:left">販売先</th>
    </tr>
    <tr> 
        <th style="white-space:nowrap; text-align:left">仕入日</th>
        <th style="white-space:nowrap; text-align:left">仕入区分</th>
        <th style="white-space:nowrap; text-align:left">メーカー名</th>
        <th style="white-space:nowrap; text-align:left">車種名</th>
        <th style="white-space:nowrap; text-align:left">型式</th>
        <th style="white-space:nowrap; text-align:left">年式</th>
        <th style="white-space:nowrap; text-align:left">外装色</th>
        <th style="white-space:nowrap; text-align:left">走行距離</th>
        <th style="white-space:nowrap; text-align:left">仕入金額</th>
        <th style="white-space:nowrap; text-align:left">管理番号</th>
        <th style="white-space:nowrap; text-align:left">車台番号</th>
        <th style="white-space:nowrap; text-align:left">仕入担当</th>
        <th style="white-space:nowrap; text-align:left">登録番号</th>
        <th style="white-space:nowrap; text-align:left">仕入先名</th>
        <th style="white-space:nowrap; text-align:left">郵便番号</th>
        <th style="white-space:nowrap; text-align:left">都道府県</th>
        <th style="white-space:nowrap; text-align:left">市区町村</th>
        <th style="white-space:nowrap; text-align:left">住所1</th>
        <th style="white-space:nowrap; text-align:left">住所2</th>
        <th style="white-space:nowrap; text-align:left">販売区分</th>
        <th style="white-space:nowrap; text-align:left">販売日</th>
        <th style="white-space:nowrap; text-align:left">販売先</th>
        <th style="white-space:nowrap; text-align:left">都道府県</th>
        <th style="white-space:nowrap; text-align:left">市区町村</th>
        <th style="white-space:nowrap; text-align:left">住所1</th>
        <th style="white-space:nowrap; text-align:left">住所2</th>
    </tr>

    <%//検索結果を表示する。%>
    <% foreach (var item in Model){ %>
        
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.PurchaseDate))%></td>                                                     <%//仕入日%>
            <td style="white-space:nowrap"><%=Html.Encode(item.PurchaseStatus)%></td>                                                                                   <%//仕入区分%>
            <td style="white-space:nowrap"><%=Html.Encode(item.MakerName)%></td>                                                                                        <%//メーカー名%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.CarName)%></td>                                                                                          <%//車種名%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.ModelName)%></td>                                                                                        <%//型式%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.ManufacturingYear)%></td>                                                                                <%//年式%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.ExteriorColorName)%></td>                                                                                <%//外装色%> 
            <td style="white-space:nowrap;  text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.Mileage))%></td>                                               <%//走行距離%> 
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.Amount))%></td>                                                 <%//仕入金額%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber)%></td>                                                                                   <%//管理番号%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.Vin)%></td>                                                                                              <%//車台番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.EmployeeName)%></td>                                                                                     <%//仕入担当%>
            <td style="white-space:nowrap"><%=Html.Encode(item.Registration)%></td>                                                                                     <%//登録番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SupplierName)%></td>                                                                                     <%//仕入先名%>
            <td style="white-space:nowrap"><%=Html.Encode(item.S_PostCode)%></td>                                                                                       <%//郵便番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.S_Prefecture)%></td>                                                                                     <%//都道府県%>
            <td style="white-space:nowrap"><%=Html.Encode(item.S_City)%></td>                                                                                           <%//市区町村%>
            <td style="white-space:nowrap"><%=Html.Encode(item.S_Address1)%></td>                                                                                       <%//住所１%>
            <td style="white-space:nowrap"><%=Html.Encode(item.S_Address2)%></td>                                                                                       <%//住所２%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesTypeName)%></td>                                                                                    <%//販売区分%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesDate))%></td>                                                        <%//販売日%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.CustomerName)%></td>                                                                                     <%//販売先%>
            <td style="white-space:nowrap"><%=Html.Encode(item.C_Prefecture)%></td>                                                                                     <%//都道府県%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.C_City)%></td>                                                                                           <%//市区町村%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.C_Address1)%></td>                                                                                       <%//住所１%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.C_Address2)%></td>                                                                                       <%//住所２%>
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
