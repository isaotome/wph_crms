<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.MonthlyStatus>>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	MonthlyCriteria
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("Criteria","Monthly",FormMethod.Post)){ %>
<%// Add 2014/09/01 arc yano IPO対応 その２ validationチェック用のメッセージ、隠し項目追加%>
<%=Html.ValidationSummary() %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<%=Html.Hidden("hdCMOperateFlag", ViewData["CMOperateFlag"]) %>
<%=Html.Hidden("hdCMOperateUser", ViewData["CMOperateUser"]) %>
<table class="input-form">

<%// Mod 2017/05/10 arc yano #3762 車両棚卸機能追加 車両在庫棚卸状況追加のため、レイアウト変更 %>
<%// Mod 2015/06/11 arc yano 部門検索ダイアログの引数を追加(closeMonthFlag, SearchIsNot) %>
<%// Mod 2014/09/01 arc yano IPO対応 その２　月締め処理追加のため、レイアウト変更 %>
    <tr>
        <th style="width:100px">締め処理 *</th>
        <td style="width:auto; white-space:nowrap"><%=Html.DropDownList("CloseType", (IEnumerable<SelectListItem>)ViewData["CloseTypeList"])%></td>
    </tr>
    <tr>
        <th style="width:100px">処理範囲 *</th>
        <td style="width:350px; white-space:nowrap"><%=Html.RadioButton("TargetRange", "0", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("0"), new { id = "test", onclick = "changeEnable('DepartmentCode', 'DepartmentName', true)"})%>全体<%=Html.RadioButton("TargetRange", "1", ViewData["TargetRange"] != null && ViewData["TargetRange"].ToString().Equals("1"), new { onclick = "changeEnable('DepartmentCode', 'DepartmentName', false)"})%>部門単位&nbsp<% if ((ViewData["TargetRange"].ToString()).Equals("0")){ %><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new {  @class = "readonly", @ReadOnly = "ReadOnly",maxlength = 3, style = "width:40px", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%><% }else{%><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new {  maxlength = 3, style = "width:40px", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%><%} %>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="if(document.getElementById('DepartmentCode').readOnly == false){openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CloseMonthFlag=0&SearchIsNot=true')}" /><span id="DepartmentName" style ="width:160px"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th style="width:100px">対象月 *</th>
        <td style="width:auto; white-space:nowrap"><%=Html.DropDownList("TargetYear", (IEnumerable<SelectListItem>)ViewData["TargetYearList"], new { onchange = "document.getElementById('RequestFlag').value = '0'; displaySearchList()"})%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetMonth", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"], new { onchange = "document.getElementById('RequestFlag').value = '0'; displaySearchList()"})%>&nbsp;月</td>
    </tr>
</table>
<%} %>
<br />
<%// Add 2014/09/01 arc yano IPO対応 再表示ボタン削除 %>
<%//<input type="button" value="再表示" onclick="document.getElementById('RequestFlag').value = '0'; displaySearchList()" />%>
<input type="button" id ="execClose" value="実行" onclick="DisplayImage('UpdateMsg', '0'); formExecClose();" /><%//Add 2016/11/30 arc yano #3659 %>
<br />
<br />

<%//Add 2016/11/30 arc yano #3659 車両管理項目追加 月次締め実行中を示すインジケータを追加%>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>

<table class="input-form">
    <tr>
        <th colspan="2" style="text-align:center">部門</th>
        <th colspan="2" style="text-align:center">現金締め処理状況</th>
        <th colspan="2" style="text-align:center">車両在庫棚卸処理状況</th><%// Add 2017/05/10 arc yano #3762 %>
        <th colspan="2" style="text-align:center">部品在庫棚卸処理状況</th>
        <th colspan="2" style="text-align:center">月次締め処理状況</th><%// Add 2014/09/01 arc yano IPO対応 文言変更 %>
    </tr>
    <tr>
        <% //Mod 2014/09/25 arc yano #3095 現金出納締 表示月の変更 ヘッダ行の文言の変更　締め日時→締め処理実行日 %>
        <th>コード</th>
        <th>名前</th>
        <th>ステータス</th>
        <th>締め処理実行日</th>
        <th>ステータス</th><%// Add 2017/05/10 arc yano #3762 %>
        <th>締め処理実行日</th><%// Add 2017/05/10 arc yano #3762 %>
        <th>ステータス</th>
        <th>締め処理実行日</th>
        <th>ステータス</th>
        <th>締め処理実行日</th>
    </tr>
<%foreach (var item in Model) {%>
    <% //Mod 2017/12/07 arc yano #3878 月次締め処理状況　特定の部門が表示されない #3762で変更した条件を元に戻す%>
    <% //Mod 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 CarInventoryFlag = '1' or PartsInventoryFlag = '1' の部門を表示するように変更 %>
    <% //Mod 2014/12/16 arc yano IPO対応(部品在庫)CloseMonthFlag = '1' or '2'の部門を表示する。%>
    <% //Mod 2014/09/05 arc yano IPO対応その２(月次締め処理) 画面非表示フラグが立っていない部門を表示する。 %>
    <% if (!string.IsNullOrEmpty(item.Department.CloseMonthFlag) && (item.Department.CloseMonthFlag.Equals("1") || (item.Department.CloseMonthFlag.Equals("2"))))
       { %>
    <tr>
        <%//部門 %>
        <td style="white-space:nowrap"><%=Html.Encode(item.Department!=null ? item.Department.DepartmentCode : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.Department!=null ? item.Department.DepartmentName : "") %></td>
        <%//現金締め処理状況 %>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td style="width:50px;text-align:center"><%=Html.Encode(item.CashBalance!=null ? (CommonUtils.DefaultString(item.CashBalance.CloseFlag).Equals("1") ? "済" : "未") : "") %></td>
        <% //Mod 2014/09/25 arc yano #3095 現金出納締データ取得方法の変更 締め日→締め処理実行日 %>
        <td style="width:120px"><%=Html.Encode(item.CashBalance!=null ? string.Format("{0:yyyy/MM/dd HH:mm:ss}",item.CashBalance.LastUpdateDate) : "") %></td>

        <%// Add 2017/05/10 arc yano #3762 %>
        <%//車両在庫棚卸処理状況 %>
        <td style="width:50px;text-align:center"><%=Html.Encode(item.InventoryCarSchedule!=null ? !item.InventoryCarSchedule.InventoryStatus.Equals("003") ? !item.InventoryCarSchedule.InventoryStatus.Equals("002") ? "実施中" : "仮確定" : "確定" : "") %></td>
        <td style="width:120px"><%=Html.Encode(item.InventoryCarSchedule!=null && item.InventoryCarSchedule.InventoryStatus.Equals("003") ? string.Format("{0:yyyy/MM/dd HH:mm:ss}",item.InventoryCarSchedule.EndDate) : "" ) %></td>

        <%//部品在庫棚卸処理状況 %>
        <% //Add 2014/11/05 arc nakayama 部品棚卸対応%>
        <% //ステータスが003(本締め)の場合、「確定」を表示する%>
        <td style="width:50px;text-align:center"><%=Html.Encode(item.InventoryPartsSchedule!=null ? !item.InventoryPartsSchedule.InventoryStatus.Equals("002") ? "実施中" : "確定" : "") %></td>
        <td style="width:120px"><%=Html.Encode(item.InventoryPartsSchedule!=null && item.InventoryPartsSchedule.InventoryStatus.Equals("002") ? string.Format("{0:yyyy/MM/dd HH:mm:ss}",item.InventoryPartsSchedule.EndDate) : "" ) %></td>

        <%//月次締め処理状況 %>
        <% //Mod 2014/09/02 arc yano IPO対応 ステータスが001の場合、ステータス欄を非表示とする。%>
        <td style="width:50px;text-align:center"><%=Html.Encode(item.InventorySchedule!=null && item.InventorySchedule.c_InventoryStatus!=null ? item.InventorySchedule.c_InventoryStatus.Code.Equals("001") ? "" : item.InventorySchedule.c_InventoryStatus.Name : "-") %></td>
        <td style="width:120px"><%=Html.Encode(item.InventorySchedule!=null && !item.InventorySchedule.InventoryStatus.Equals("001") ? string.Format("{0:yyyy/MM/dd HH:mm:ss}",item.InventorySchedule.EndDate) : "" ) %></td>
    </tr>
    <%} %>
<%} %>
</table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        if (document.getElementById('hdCMOperateFlag').value == '1') {
            alert(document.getElementById('hdCMOperateUser').value + "さんが操作中です。\r\nしばらくしてから、再度操作を行ってください。");
        }
    }
</script>
</asp:Content>
