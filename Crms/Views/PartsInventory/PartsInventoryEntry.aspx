<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.V_PartsInventoryInProcess>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品棚卸入力入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "PartsInventory", FormMethod.Post))
  { %>
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <td onclick="document.forms[0].action.value = 'save';formSubmit();"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
    </tr>
</table>
<br />
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("sortKey", "") %>
<%=Html.Hidden("DepartmentCode", ViewData["DepartmentCode"])%>
<%=Html.Hidden("InventoryMonth", string.Format("{0:yyyy/MM}", ViewData["InventoryMonth"])) %>
<!-- <div id="input-form"> -->
<% //Mod 2014/07/23 arc yano chrome対応 レイアウトの調整のため、widthを指定する。 %>
<div id="input-form" style="width:893px;">
<%=Html.ValidationSummary()%>
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">会社</th>
        <td colspan="3"><%=Html.Encode(ViewData["CompanyName"])%></td>
    </tr>
    <tr>
        <th>事業所</th>
        <td style="width:200px"><%=Html.Encode(ViewData["OfficeName"])%></td>
        <th style="width:100px">部門</th>
        <td style="width:200px"><%=Html.Encode(ViewData["DepartmentName"])%></td>
    </tr>
    <tr>
        <th>棚卸月</th>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM}", ViewData["InventoryMonth"]))%></td>
        <th>最終更新日時</th>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd HH:mm:ss}", ViewData["LastUpdateDate"]))%></td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th style="width:150px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'LocationName';formSubmit();return false;" style="text-decoration:underline;">ロケーション</a></th>
        <th style="width:100px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'MakerName';formSubmit();return false;" style="text-decoration:underline;">メーカー</a></th>
        <th style="width:150px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'PartsNumber';formSubmit();return false;" style="text-decoration:underline;">品番</a></th>
        <th style="width:250px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'PartsNameJp';formSubmit();return false;" style="text-decoration:underline;">部品名</a></th>
        <th style="width:100px">理論値</th>
        <th style="width:100px">実地数</th>
    </tr>
</table>
<div style="overflow-y:scroll;width:910px;height:541px">
<table class="input-form">
<%for (int i = 0; i < Model.Count; i++) {
      string namePrefix = string.Format("line[{0}].", i);
      string idPrefix = string.Format("line[{0}]_", i);
      V_PartsInventoryInProcess v_PartsInventoryInProcess = Model[i]; %>
    <tr>
        <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにid追加 %>
        <%=Html.Hidden(namePrefix + "InventoryId", v_PartsInventoryInProcess.InventoryId, new { id = idPrefix + "InventoryId"})%>
        <%=Html.Hidden(namePrefix + "LocationCode", v_PartsInventoryInProcess.LocationCode, new { id = idPrefix + "LocationCode"})%>
        <% //Mod 2014/07/23 arc yano chrome対応 レイアウトの調整のため、white-space:nomalを追加。 %>
        <td style="width:150px;white-space:normal;"><%=Html.Encode(v_PartsInventoryInProcess.LocationName)%><%=Html.Hidden(namePrefix + "LocationName", v_PartsInventoryInProcess.LocationName, new { id = idPrefix + "LocationName"})%></td>
        <td style="width:100px;white-space:normal;"><%=Html.Encode(v_PartsInventoryInProcess.MakerName)%><%=Html.Hidden(namePrefix + "MakerName", v_PartsInventoryInProcess.MakerName, new { id = idPrefix + "MakerName"})%></td>
        <td style="width:150px;white-space:normal;"><%=Html.Encode(v_PartsInventoryInProcess.PartsNumber)%><%=Html.Hidden(namePrefix + "PartsNumber", v_PartsInventoryInProcess.PartsNumber, new { id = idPrefix + "PartsNumber"})%></td>
        <td style="width:250px;white-space:normal;"><%=Html.Encode(v_PartsInventoryInProcess.PartsNameJp)%><%=Html.Hidden(namePrefix + "PartsNameJp", v_PartsInventoryInProcess.PartsNameJp, new { id = idPrefix + "PartsNameJp"})%></td>
        <td style="width:100px;text-align:right;white-space:normal;"><%=Html.Encode(v_PartsInventoryInProcess.LogicalQuantity)%><%=Html.Hidden(namePrefix + "LogicalQuantity", v_PartsInventoryInProcess.LogicalQuantity, new { id = idPrefix + "LogicalQuantity"})%></td>
        <td style="width:100px;white-space:normal;"><%=Html.TextBox(namePrefix + "Quantity", v_PartsInventoryInProcess.Quantity, new { id = idPrefix + "Quantity", @class = "numeric", size = 10, maxlength = 11 })%></td>
    </tr>
<%} %>
</table>
</div>
</div>
<%} %>
<br />
</asp:Content>
