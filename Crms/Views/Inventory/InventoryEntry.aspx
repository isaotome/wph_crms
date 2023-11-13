<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.V_CarInventoryInProcess>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両棚卸入力入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "Inventory", FormMethod.Post))
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
<div id="input-form" style ="width:1000px">
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
        <th style="width:100px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'CarBrandName';formSubmit();return false;" style="text-decoration:underline;">ブランド</a></th>
        <th style="width:100px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'CarName';formSubmit();return false;" style="text-decoration:underline;">車種</a></th>
        <th style="width:250px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'CarGradeName';formSubmit();return false;" style="text-decoration:underline;">グレード</a></th>
        <th style="width:150px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'Vin';formSubmit();return false;" style="text-decoration:underline;">車台番号</a></th>
        <th style="width:50px"><a href="javascript:void();" onclick="document.forms[0].action.value = 'sort';document.forms[0].sortKey.value = 'CarStatusName';formSubmit();return false;" style="text-decoration:underline;">ステータス</a></th>
        <th style="width:50px">実地数</th>
    </tr>
</table>
<% //Mod 2014/07/23 arc yano chrome対応 レイアウトの調整のため、width値を変更する。 1025px→1017px %>
<div style="overflow-y:scroll;width:1017px;height:541px">
<table class="input-form">
<%for (int i = 0; i < Model.Count; i++) {
      string namePrefix = string.Format("line[{0}].", i);
      string idPrefix = string.Format("line[{0}]_", i);
      V_CarInventoryInProcess v_CarInventoryInProcess = Model[i]; %>
    <tr>
        <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにid追加 %>
        <%=Html.Hidden(namePrefix + "InventoryId", v_CarInventoryInProcess.InventoryId, new { id = idPrefix + "InventoryId"})%>
        <%=Html.Hidden(namePrefix + "SalesCarNumber", v_CarInventoryInProcess.SalesCarNumber, new { id = idPrefix + "SalesCarNumber"})%>
        <%=Html.Hidden(namePrefix + "LocationCode", v_CarInventoryInProcess.LocationCode, new { id = idPrefix + "LocationCode"})%>
        <td style="width:150px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.LocationName)%><%=Html.Hidden(namePrefix + "LocationName", v_CarInventoryInProcess.LocationName, new { id = idPrefix + "LocationName"})%></td>
        <td style="width:100px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.MakerName)%><%=Html.Hidden(namePrefix + "MakerName", v_CarInventoryInProcess.MakerName, new { id = idPrefix + "MakerName"})%></td>
        <td style="width:100px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.CarBrandName)%><%=Html.Hidden(namePrefix + "CarBrandName", v_CarInventoryInProcess.CarBrandName, new { id = idPrefix + "CarBrandName"})%></td>
        <td style="width:100px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.CarName)%><%=Html.Hidden(namePrefix + "CarName", v_CarInventoryInProcess.CarName, new { id = idPrefix + "CarName"})%></td>
        <td style="width:250px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.CarGradeName)%><%=Html.Hidden(namePrefix + "CarGradeName", v_CarInventoryInProcess.CarGradeName, new { id = idPrefix + "CarGradeName"})%></td>
        <td style="width:150px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.Vin)%><%=Html.Hidden(namePrefix + "Vin", v_CarInventoryInProcess.Vin, new { id = idPrefix + "Vin"})%></td>
        <td style="width:50px;white-space:normal;"><%=Html.Encode(v_CarInventoryInProcess.CarStatusName)%><%=Html.Hidden(namePrefix + "CarStatusName", v_CarInventoryInProcess.CarStatusName, new { id = idPrefix + "CarStatusName"})%></td>
        <td style="width:50px;white-space:normal;"><%=Html.TextBox(namePrefix + "Quantity", v_CarInventoryInProcess.Quantity, new { id = idPrefix + "Quantity", @class = "numeric", size = 2, maxlength = 1 })%></td>
    </tr>
<%} %>
</table>
</div>
</div>
<%} %>
<br />
</asp:Content>
