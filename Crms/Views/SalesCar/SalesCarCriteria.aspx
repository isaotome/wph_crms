<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SalesCar>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "SalesCar", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("DefaultDelFlag", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th style="width:100px">ブランド名</th>
        <td><%=Html.TextBox("CarBrandName", ViewData["CarBrandName"], new { size = 10, maxlength = 50 })%></td>
        <th style="width:100px">車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { size = 10, maxlength = 20 })%></td>
        <th style="width:100px">グレード名</th>
        <td><%=Html.TextBox("CarGradeName", ViewData["CarGradeName"], new { size = 10, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>使用者名</th>
        <td><%=Html.TextBox("UserName", ViewData["UserName"], new { size = 10, maxlength = 80 })%></td>
        <th>使用者名(カナ)</th>
        <td><%=Html.TextBox("UserNameKana", ViewData["UserNameKana"], new { size = 10, maxlength = 80 })%></td>
        <th>新中区分</th>
        <td><%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
        <th>系統色</th>
        <td><%=Html.DropDownList("ColorType", (IEnumerable<SelectListItem>)ViewData["ColorTypeList"])%></td>
    </tr>
    <tr>
        <th>外装色コード</th>
        <td><%=Html.TextBox("ExteriorColorCode", ViewData["ExteriorColorCode"], new { @calss = "alphanumeric", size = 10, maxlength = 8 })%></td>
        <th>外装色名</th>
        <td><%=Html.TextBox("ExteriorColorName", ViewData["ExteriorColorName"], new { size = 10, maxlength = 50 })%></td>
        <th>内装色コード</th>
        <td><%=Html.TextBox("InteriorColorCode", ViewData["InteriorColorCode"], new { @calss = "alphanumeric", size = 10, maxlength = 8 })%></td>
        <th>内装色名</th>
        <td><%=Html.TextBox("InteriorColorName", ViewData["InteriorColorName"], new { size = 10, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 10, maxlength = 20 })%></td>
        <th>在庫ステータス</th>
        <td><%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["CarStatusList"])%></td>
        <% //Mod 2014/10/16 arc yano 車両ステータス追加対応 利用用途のリストボックスを在庫ステータスの隣に追加し、その後の項目をずらす%>
        <th>利用用途</th>
        <td><%=Html.DropDownList("CarUsage", (IEnumerable<SelectListItem>)ViewData["CarUsageList"])%></td>
        <th>ロケーション名</th>
        <td><%=Html.TextBox("LocationName", ViewData["LocationName"], new { size = 10, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>所有者名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = 10, maxlength = 40 })%></td>
        <th>陸運局コード</th>
        <td><%=Html.TextBox("MorterViecleOfficialCode", ViewData["MorterViecleOfficialCode"], new { size = 10, maxlength = 5 })%></td>
        <th>車両登録番号(種別)</th>
        <td><%=Html.TextBox("RegistrationNumberType", ViewData["RegistrationNumberType"], new { @calss = "alphanumeric", size = 10, maxlength = 3 })%></td>
        <th>車両登録番号(かな)</th>
        <td><%=Html.TextBox("RegistrationNumberKana", ViewData["RegistrationNumberKana"], new { size = 10, maxlength = 1 })%></td>
    </tr>
    <tr>
        <th>車両登録番号(プレート)</th>
        <td><%=Html.TextBox("RegistrationNumberPlate", ViewData["RegistrationNumberPlate"], new { @calss = "alphanumeric", size = 10, maxlength = 4 })%></td>
        <th>年式</th>
        <td><%=Html.TextBox("ManufacturingYear", ViewData["ManufacturingYear"], new { @calss = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>ハンドル</th>
        <td><%=Html.DropDownList("Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"])%></td>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td colspan="7">
            <input type="button" value="検索" onclick="displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SalesCarNumber','CarBrandName','CarName','CarGradeName','UserName','UserNameKana','NewUsedType','ColorType','ExteriorColorCode','ExteriorColorName','InteriorColorCode','InteriorColorName','Vin','CarStatus','LocationName','CustomerName','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','ManufacturingYear','Steering','DelFlag'))" />
        </td>
    </tr>
</table>
</div>
<br />
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/SalesCar/Entry?Master=1')"/>
<br />
<br />
     <% //DEL 2014/08/15 arc amii #3071 在庫ステータス変更対応 検索画面から入力画面へ項目を移動 %>
<!-- <%if (Model.Count > 0)
  {%>
<table class="input-form">
    <tr>
        <th style="width:140px">在庫ステータス変更</th>
        <td><%=Html.DropDownList("ChangeCarStatus", (IEnumerable<SelectListItem>)ViewData["ChangeCarStatusList"])%>&nbsp;<input type="button" value="変更" onclick="changeCarStatus()"/></td>
    </tr>
</table>
<%} %> -->
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:20px" rowspan="2"></th>
        <th style="width:30px" rowspan="2"></th>
        <th>管理番号</th>
        <th>ブランド</th>
        <th>車種</th>
        <th>グレード</th>
        <th>新中区分</th>
        <th>系統色</th>
        <th>外装色</th>
        <th>年式</th>
        <th>走行距離</th>
    </tr>
    <tr>
        <th>車台番号</th>
        <th>在庫ステータス</th>
        <th>在庫ロケーション</th>
        <th>使用者名</th>
        <th>陸運局</th>
        <th>登録番号</th>
        <th>内装色</th>
        <th>ハンドル</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var salesCar in Model)
      { %>
    <tr>
        <td rowspan="2"><%=Html.CheckBox("chkTarget", new { value = salesCar.SalesCarNumber })%></td>
        <td rowspan="2"><a href="javascript:openModalAfterRefresh('/SalesCar/Entry/' + '<%=salesCar.SalesCarNumber%>?Master=1')">詳細</a></td>
        <td><%=Html.Encode(salesCar.SalesCarNumber)%></td>
        <td><%try { %><%=Html.Encode(salesCar.CarGrade.Car.Brand.CarBrandName)%><%} catch (NullReferenceException) { } %></td>
        <td><%try { %><%=Html.Encode(salesCar.CarGrade.Car.CarName)%><%} catch (NullReferenceException) { } %></td>
        <td><%if (salesCar.CarGrade != null) {%><%=Html.Encode(salesCar.CarGrade.CarGradeName)%><%} %></td>
        <td><%if (salesCar.c_NewUsedType != null) {%><%=Html.Encode(salesCar.c_NewUsedType.Name)%><%} %></td>
        <td><%if (salesCar.c_ColorCategory != null) {%><%=Html.Encode(salesCar.c_ColorCategory.Name)%><%} %></td>
        <td><%=Html.Encode(salesCar.ExteriorColorName)%></td>
        <td><%=Html.Encode(salesCar.ManufacturingYear)%></td>
        <td><%=Html.Encode(salesCar.Mileage)%><%if (salesCar.Mileage != null && salesCar.c_MileageUnit != null) {%><%=Html.Encode(salesCar.c_MileageUnit.Name)%><%} %></td>
    </tr>
    <tr>
        <td style="white-space:nowrap"><%=Html.Encode(salesCar.Vin)%></td>
        <td><%if (salesCar.c_CarStatus != null) {%><%=Html.Encode(salesCar.c_CarStatus.Name)%> <%} %></td>
        <td><%if (salesCar.Location != null) {%><%=Html.Encode(salesCar.Location.LocationName)%> <%} %></td>
        <td><%if (salesCar.User != null) {%><%=Html.Encode(salesCar.User.CustomerName)%><%} %></td>
        <td><%=Html.Encode(salesCar.MorterViecleOfficialCode)%></td>
        <td style="white-space:nowrap"><%=Html.Encode(salesCar.RegistrationNumberType)%>&nbsp;<%=Html.Encode(salesCar.RegistrationNumberKana)%>&nbsp;<%=Html.Encode(salesCar.RegistrationNumberPlate)%></td>
        <td><%=Html.Encode(salesCar.InteriorColorName)%></td>
        <td><%if (salesCar.c_Steering != null) {%><%=Html.Encode(salesCar.c_Steering.Name)%><%} %></td>
        <td><%=Html.Encode(salesCar.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
<%} %>
</asp:Content>
