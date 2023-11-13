<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarStatusCheck>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両追跡<%--Mod 2018/11/19 yano #3950_パーツステータス管理_画面タイトル誤り修正 類似処理--%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
    //--------------------------------------------------------------------------------
    //　機能　：車両追跡検索画面
    //　作成日：2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
    //
    //
    //-------------------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "CarStatusCheck", FormMethod.Post))
{ 
%>

<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
   <table class="input-form">
     <tr>
        <th style="width:80px">管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 20, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 20, maxlength = 20 })%></td>
    </tr>

    <tr>
        <th style="width:80px"></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('SalesCarNumber', 'Vin'))"/>
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />

<%
//-------------------------
//車両基本情報  
//-------------------------
%>
<%if (Model.GetCarBasicInfoResult.Count > 0)
  { //車両基本情報がある場合
%> 
    <span>車両基本情報</span>
    <table class="list">
        <tr>
            <th style="white-space:nowrap; text-align:left">管理番号</th>
            <th style="white-space:nowrap; text-align:left">車台番号</th>
            <th style="white-space:nowrap; text-align:left">新中区分</th>
            <th style="white-space:nowrap; text-align:left">ロケーション</th>
            <th style="white-space:nowrap; text-align:left">状態</th>
            <th style="white-space:nowrap; text-align:left">メーカー</th>
            <th style="white-space:nowrap; text-align:left">ブランド</th>
            <th style="white-space:nowrap; text-align:left">車種</th>
            <th style="white-space:nowrap; text-align:left">所有者</th>
            <th style="white-space:nowrap; text-align:left">使用者</th>
            <th style="white-space:nowrap; text-align:left">有効／無効</th>
        </tr>
  
    <%//検索結果を表示する。%>
    <% foreach (var item in Model.GetCarBasicInfoResult){ %>
        
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber)%></td>                                                                                                                               <%//管理番号%>
            <td style="white-space:nowrap"><a href="javascript:void(0);" onclick ="document.getElementById('Vin').value ='<%=Html.Encode(item.Vin)%>'; DisplayImage('UpdateMsg', '0'); displaySearchList()"><%=Html.Encode(item.Vin)%></></td><%//車台番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.NewUsedName)%></td>                                                                                                                                  <%//新中区分%>
            <td style="white-space:nowrap"><%=Html.Encode(item.LocationName)%></td>                                                                                                                                 <%//ロケーション名%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CarStatusName)%></td>                                                                                                                                <%//状態名%>
            <td style="white-space:nowrap"><%=Html.Encode(item.MakerName)%></td>                                                                                                                                    <%//メーカー%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CarBrandName)%></td>                                                                                                                                 <%//ブランド%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CarName)%></td>                                                                                                                                      <%//車種%>
            <td style="white-space:nowrap"><%=Html.Encode(item.PossesorName)%></td>                                                                                                                                 <%//所有者%>
            <td style="white-space:nowrap"><%=Html.Encode(item.UserName)%></td>                                                                                                                                     <%//使用者%>
            <td style="white-space:nowrap"><%=Html.Encode(item.DelName)%></td>                                                                                                                                      <%//有効／無効%>
        </tr>
    <%} %>
    </table>
<%} %>

<%
//-------------------------
//遷移 
//-------------------------
%>
<%if (Model.GetCarStatusTransitionResult.Count > 0)
  { //車両遷移情報がある場合
%> 
    <br />
    <br />
    <span>遷移</span>
    <table class="list">
        <tr>
            <th colspan="2" style="white-space:nowrap; text-align:left">区分</th>
            <th style="white-space:nowrap; text-align:left">伝票日付</th>
            <th style="white-space:nowrap; text-align:left">店舗</th>
            <th style="white-space:nowrap; text-align:left">取引先</th>
            <th style="white-space:nowrap; text-align:left">ステータス</th>
            <th style="white-space:nowrap; text-align:left">担当者</th>
            <th style="white-space:nowrap; text-align:left">管理番号</th>
            <th style="white-space:nowrap; text-align:left">伝票番号</th>
            <th style="white-space:nowrap; text-align:left">車両本体価格<br />(税抜)</th>
            <th style="white-space:nowrap; text-align:left">仕入／販売合計<br />(税抜)</th>
        </tr>
  
    <%//検索結果を表示する。%>
    <% foreach (var item in Model.GetCarStatusTransitionResult)
       { 
    %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipTypeName)%></td>                                                                        <%//区分%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipType)%></td>                                                                            <%//区分名%>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SlipDate))%></td>                                            <%//伝票日付%>
            <td style="white-space:nowrap"><%=Html.Encode(item.LocationName)%></td>                                                                        <%//店舗%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CustomerName)%></td>                                                                        <%//取引先%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipStatus)%></td>                                                                          <%//ステータス%>
            <td style="white-space:nowrap"><%=Html.Encode(item.EmployeeName)%></td>                                                                        <%//担当者%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber)%></td>                                                                      <%//管理番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipNumber)%></td>                                                                          <%//伝票番号%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.VehiclePrice))%></td>                              <%//車両本体価格(税抜)%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.Amount))%></td>                                    <%//仕入合計／販売合計(税抜)%>
        </tr>
    <%} %>
    </table>
<%} %>
    
<%
//-------------------------
//車両販売伝票 
//-------------------------
%>
<%if (Model.GetCarSalesSlipResult.Count > 0)
  { //車両販売伝票がある場合
%> 
    <br />
    <br />
    <span>車両販売伝票</span>
    <table class="list">
        <tr>
            <th style="white-space:nowrap; text-align:left">伝票番号</th>
            <th style="white-space:nowrap; text-align:left">管理番号</th>
            <th style="white-space:nowrap; text-align:left">伝票ステータス</th>
            <th style="white-space:nowrap; text-align:left">販売店舗</th>
            <th style="white-space:nowrap; text-align:left">営業担当</th>
            <th style="white-space:nowrap; text-align:left">取引先</th>
        </tr>
  
    <%//検索結果を表示する。%>
    <% foreach (var item in Model.GetCarSalesSlipResult)
       { 
    %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.SlipNumber)%></td>                                                               <%//伝票番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber)%></td>                                                           <%//管理番号%>
            <td style="white-space:nowrap"><%=Html.Encode(item.SalesOrderStatusName)%></td>                                                     <%//伝票ステータス%>
            <td style="white-space:nowrap"><%=Html.Encode(item.DepartmentName)%></td>                                                           <%//販売店舗%>
            <td style="white-space:nowrap"><%=Html.Encode(item.EmployeeName)%></td>                                                             <%//営業担当%>
            <td style="white-space:nowrap"><%=Html.Encode(item.CustomerName)%></td>                                                             <%//取引先%>
        </tr>
    <%} %>
    </table>
<%} %> 

<%} %>

</asp:Content>
