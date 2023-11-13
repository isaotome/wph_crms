<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両伝票ステータス修正
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
//---------------------------------------------------------------------------------
// 機能  ：車両伝票戻し検索画面
// 作成者：arc nakayama
// 作成日：2017/05/09
// 更新日：
//    2020/08/03 yano #4048 【車両伝票ステータス修正】伝票を戻す機能の非表示化
//---------------------------------------------------------------------------------
%>

<%using (Html.BeginForm("Criteria", "CarSlipStatusChange", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("indexid", ViewData["indexid"])%>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
<%=Html.Hidden("SalesOrderStatus", ViewData["SalesOrderStatus"]) %>
<%=Html.Hidden("DepartmentCode", ViewData["DepartmentCode"]) %>
<%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
<%=Html.Hidden("StatusChangeCode", ViewData["StatusChangeCode"]) %>
<%=Html.Hidden("DeliveredSlipStatusChange", ViewData["DeliveredSlipStatusChange"]) %><%///Add 2020/08/03 yano #4048 %>

<table class="input-form">
    <tr>
        <th>伝票番号</th>
        <td><%=Html.TextBox("SearchSlipNumber", ViewData["SearchSlipNumber"], new { @class = "alphanumeric", size = "10", maxlength = "10" })%></td>
    </tr>
    <tr>
        <th></th>
        <td>
            <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; document.forms[0].submit();" />
        </td>
    </tr>
    <tr>
        <th></th>
        <td>
            <a><b>１．「伝票を戻す」をクリックで、売上伝票ステータスを元に戻します。</b></a><br />
            <a><b>２．「表示を消す」をクリックすると、進行中のリストから外れます。</b></a><br />
            <a><b>３．「履歴」ボタンを押すと過去に修正した履歴が表示されます。</b></a><br />
            <a><b>４．「進行中」ボタンを押すと元の画面に戻ります。</b></a><br />
        </td>
    </tr>
</table>

<br />
<table class="input-form">

    <!--納車日-->
    <tr>
      <th style="width: 100px">納車日</th>
      <td><%=Html.TextBox("SalesDate", ViewData["SalesDate"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
    </tr>
    <!--伝票番号-->
    <tr>
      <th style="width: 100px">伝票番号</th>
      <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
          <%=Html.Hidden("TargetSlipNumber", ViewData["TargetSlipNumber"]) %>
    </tr>
    <!--顧客名-->
    <tr>
      <th style="width: 100px">顧客名</th>
      <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
    </tr>
    <!--伝票ステータス-->
    <tr>
      <th style="width: 100px">伝票ステータス</th>
      <td><%=Html.TextBox("SalesOrderStatusName", ViewData["SalesOrderStatusName"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
    </tr>
    <!--店舗名-->
    <tr>
      <th style="width: 100px">部門名</th>
      <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
    </tr>
    <!--営業担当-->
    <tr>
      <th style="width: 100px">営業担当</th>
      <td><%=Html.TextBox("Employeename", ViewData["Employeename"], new { @class = "readonly", @readonly = "readonly", size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
      <th style="width: 100px">修正依頼者</th>
      <td><%=Html.TextBox("RequestUserName", ViewData["RequestUserName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th></th>
        <td>
            <%=Html.Label(ViewData["Message"].ToString())%>
        </td>
    </tr>
    <tr>
        <th></th>
        <td>
            <input id="StatusBack002" type="button" value="修正する" onclick="document.getElementById('RequestFlag').value = '2';DisplayImage('UpdateMsg', '0'); document.forms[0].submit();" />
            <input id="StatusBack001" type="button" style="width:130px" value="見積作成状態に戻す" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); document.forms[0].submit();" />
        </td>
    </tr>
</table>
<br />
<div id ="UpdateMsg" style ="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<table>
    <tr>
        <td>
            <input type="button" value="進行中" onclick="document.getElementById('indexid').value = 0; changeDisplayCarSlipChange('0');" />
            <input type="button" value="履歴" onclick="document.getElementById('indexid').value = 1; changeDisplayCarSlipChange('1');" />
        </td>
    </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<% // --------進行中-------%>
<div id="0" style="<%=!(bool)ViewData["0_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret0_List", "CarSlipStatusChange", new { ChangeStatus = "1" });%>
</div>
<% // --------履歴-------%>
<div id="1" style="<%=!(bool)ViewData["1_ListDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderAction("Ret1_List", "CarSlipStatusChange", new { ChangeStatus = "2" });%>
</div>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        StatusChangeButtonControll();
    }
</script>
</asp:Content>