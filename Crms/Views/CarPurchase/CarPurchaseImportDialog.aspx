<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarPurchaseExcelImportList>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Excel一括取込
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("ImportDialog", "CarPurchase", FormMethod.Post, new { enctype = "multipart/form-data" }))
      { %>
    <%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:読み込み / 2:取り込み )%>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<div id="import-form">
<br />
<div id ="validSummary">
    <%=Html.ValidationSummary()%>
</div>
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">ファイルパス</th>
        <td><input type="file" name="ImportFile" size="50" />&nbsp<input type="button" value="読み取り" onclick = "document.getElementById('validSummary').style.display = 'none'; document.getElementById('RequestFlag').value = '1'; dispProgresseddispTimer('PartsPurchase', 'UpdateMsg'); formSubmit();"/>
        <input type="button" value="キャンセル" onclick="document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '3'; formSubmit()" />
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <%if(ViewData["ErrFlag"].Equals("0")) {%>
            <input type="button" value="取込実行" onclick="document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '2'; formSubmit()" />
        <%}else{ %>
            <input type="button" value="取込実行" disabled="disabled" />
        <%} %>
        </td>
    </tr>
</table>
</div>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>

<br />
    <table class="list">
        <tr>
            <th></th>
            <th style="white-space:nowrap;text-align:left">
                管理番号("A"で自動)
            </th>
            <th style="white-space:nowrap;text-align:left">
                車台番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                VIN(シリアル)
            </th>
            <th style="white-space:nowrap;text-align:left">
                メーカー名
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両グレードコード
            </th>
            <th style="white-space:nowrap;text-align:left">
                新中区分("N"or"U")
            </th>
            <th style="white-space:nowrap;text-align:left">
                系統色
            </th>
            <th style="white-space:nowrap;text-align:left">
                外装色コード
            </th>
            <th style="white-space:nowrap;text-align:left">
                外装色名
            </th>
            <th style="white-space:nowrap;text-align:left">
                内装色コード
            </th>
            <th style="white-space:nowrap;text-align:left">
                内装色名
            </th>
            <th style="white-space:nowrap;text-align:left">
                年式
            </th>
            <th style="white-space:nowrap;text-align:left">
                ハンドル
            </th>
            <th style="white-space:nowrap;text-align:left">
                販売価格(税抜）
            </th>
            <th style="white-space:nowrap;text-align:left">
                型式
            </th>
            <th style="white-space:nowrap;text-align:left">
                原動機型式
            </th>
            <th style="white-space:nowrap;text-align:left">
                排気量
            </th>
            <th style="white-space:nowrap;text-align:left">
                型式指定番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                類別区分番号
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考１
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考２
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考３
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考４
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考５
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考６
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考７
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考８
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考９
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考１０
            </th>
            <th style="white-space:nowrap;text-align:left">
                自動車税種別割       <%-- //Mod 2019/09/04 yano #4011 --%>
            </th>
            <th style="white-space:nowrap;text-align:left">
                自動車重量税
            </th>
            <th style="white-space:nowrap;text-align:left">
                自賠責保険料
            </th>
            <th style="white-space:nowrap;text-align:left">
                自動車税環境性能割   <%-- //Mod 2019/09/04 yano #4011 --%>
            </th>
            <th style="white-space:nowrap;text-align:left">
                リサイクル預託金
            </th>
            <th style="white-space:nowrap;text-align:left">
                認定中古車No
            </th>
            <th style="white-space:nowrap;text-align:left">
                認定中古車保証期間FROM
            </th>
            <th style="white-space:nowrap;text-align:left">
                認定中古車保証期間TO
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入日
            </th>
            <th style="white-space:nowrap;text-align:left">
                入庫予定日
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入先コード
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入ロケーションコード
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両本体価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両本体消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                車両本体税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                オプション価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                オプション消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                オプション税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                ディスカウント価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                ディスカウント消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                ディスカウント税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                ファーム価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                ファーム消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                ファーム税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                メタリック価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                メタリック消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                メタリック税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                加装価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                加修価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                その他価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                その他消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                その他税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                自税充当
            </th>
            <th style="white-space:nowrap;text-align:left">
                リサイクル
            </th>
            <th style="white-space:nowrap;text-align:left">
                オークション落札料
            </th>
            <th style="white-space:nowrap;text-align:left">
                オークション落札料消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                オークション落札料税込
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                消費税
            </th>
            <th style="white-space:nowrap;text-align:left">
                仕入税込価格
            </th>
            <th style="white-space:nowrap;text-align:left">
                備考
            </th>
        </tr>


        <%int i = 0;%>
        <%foreach (var a in Model)
          {
              string namePrefix = string.Format("ImportLine[{0}].", i);
              string idPrefix = string.Format("ImportLine[{0}]_", i);
        %>
        <tr>
            <!--行番号-->
            <td style="white-space:nowrap;text-align:left;">
                <%=Html.TextBox(namePrefix + "LineNumber", (i + 1).ToString(), new { id = idPrefix + "LineNumber", style = "width:20px", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--管理番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SalesCarNumber", a.SalesCarNumber, new { id = idPrefix + "SalesCarNumber", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車台番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Vin", a.Vin, new { id = idPrefix + "Vin", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--VIN(シリアル)-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "UsVin", a.UsVin, new { id = idPrefix + "UsVin", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--メーカー名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MakerName", a.MakerName, new { id = idPrefix + "MakerName", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車両グレードコード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarGradeCode", a.CarGradeCode, new { id = idPrefix + "CarGradeCode", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--新中区分-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "NewUsedType", a.NewUsedType, new { id = idPrefix + "NewUsedType", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--系統色-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ColorTypeName", a.ColorType, new { id = idPrefix + "ColorTypeName", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--外装色コード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ExteriorColorCode", a.ExteriorColorCode, new { id = idPrefix + "ExteriorColorCode", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--外装色名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ExteriorColorName", a.ExteriorColorName, new { id = idPrefix + "ExteriorColorName", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--内装色コード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "InteriorColorCode", a.InteriorColorCode, new { id = idPrefix + "InteriorColorCode", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--内装色名-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "InteriorColorName", a.InteriorColorName, new { id = idPrefix + "InteriorColorName", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--年式-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ManufacturingYear", a.ManufacturingYear, new { id = idPrefix + "ManufacturingYear", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ハンドル-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Steering", a.Steering, new { id = idPrefix + "Steering", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--販売価格(税抜）-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SalesPrice", a.SalesPrice, new { id = idPrefix + "SalesPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--型式-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ModelName", a.ModelName, new { id = idPrefix + "ModelName", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--原動機型式-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "EngineType", a.EngineType, new { id = idPrefix + "EngineType", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--排気量-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Displacement", a.Displacement, new { id = idPrefix + "Displacement", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--型式指定番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ModelSpecificateNumber", a.ModelSpecificateNumber, new { id = idPrefix + "ModelSpecificateNumber", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--類別区分番号-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ClassificationTypeNumber", a.ClassificationTypeNumber, new { id = idPrefix + "ClassificationTypeNumber", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考１-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo1", a.Memo1, new { id = idPrefix + "Memo1", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考２-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo2", a.Memo2, new { id = idPrefix + "Memo2", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考３-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo3", a.Memo3, new { id = idPrefix + "Memo3", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考４-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo4", a.Memo4, new { id = idPrefix + "Memo4", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考５-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo5", a.Memo5, new { id = idPrefix + "Memo5", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考６-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo6", a.Memo6, new { id = idPrefix + "Memo6", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考７-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo7", a.Memo7, new { id = idPrefix + "Memo7", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考８-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo8", a.Memo8, new { id = idPrefix + "Memo8", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考９-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo9", a.Memo9, new { id = idPrefix + "Memo9", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考１０-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo10", a.Memo10, new { id = idPrefix + "Memo10", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--自動車税種別割-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarTax", a.CarTax, new { id = idPrefix + "CarTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--自動車重量税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarWeightTax", a.CarWeightTax, new { id = idPrefix + "CarWeightTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--自賠責保険料-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarLiabilityInsurance", a.CarLiabilityInsurance, new { id = idPrefix + "CarLiabilityInsurance", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--自動車税環境性能割-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "AcquisitionTax", a.AcquisitionTax, new { id = idPrefix + "AcquisitionTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--リサイクル預託金-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "RecycleDeposit", a.RecycleDeposit, new { id = idPrefix + "RecycleDeposit", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--認定中古車No-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ApprovedCarNumber", a.ApprovedCarNumber, new { id = idPrefix + "ApprovedCarNumber", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--認定中古車保証期間FROM-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ApprovedCarWarrantyDateFrom", a.ApprovedCarWarrantyDateFrom, new { id = idPrefix + "ApprovedCarWarrantyDateFrom", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--認定中古車保証期間TO-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "ApprovedCarWarrantyDateTo", a.ApprovedCarWarrantyDateTo, new { id = idPrefix + "ApprovedCarWarrantyDateTo", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--仕入日-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SlipDate", a.SlipDate, new { id = idPrefix + "SlipDate", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--入庫予定日-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PurchaseDate", a.PurchaseDate, new { id = idPrefix + "PurchaseDate", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--仕入先コード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "SupplierCode", a.SupplierCode, new { id = idPrefix + "SupplierCode", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--仕入ロケーションコード-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "PurchaseLocationCode", a.PurchaseLocationCode, new { id = idPrefix + "PurchaseLocationCode", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車両本体価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "VehiclePrice", a.VehiclePrice, new { id = idPrefix + "VehiclePrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車両本体消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "VehicleTax", a.VehicleTax, new { id = idPrefix + "VehicleTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--車両本体税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "VehicleAmount", a.VehicleAmount, new { id = idPrefix + "VehicleAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オプション価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OptionPrice", a.OptionPrice, new { id = idPrefix + "OptionPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オプション消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OptionTax", a.OptionTax, new { id = idPrefix + "OptionTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オプション税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OptionAmount", a.OptionAmount, new { id = idPrefix + "OptionAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ディスカウント価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "DiscountPrice", a.DiscountPrice, new { id = idPrefix + "DiscountPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ディスカウント消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "DiscountTax", a.DiscountTax, new { id = idPrefix + "DiscountTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ディスカウント税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "DiscountAmount", a.DiscountAmount, new { id = idPrefix + "DiscountAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ファーム価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "FirmPrice", a.FirmPrice, new { id = idPrefix + "FirmPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ファーム消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "FirmTax", a.FirmTax, new { id = idPrefix + "FirmTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--ファーム税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "FirmAmount", a.FirmAmount, new { id = idPrefix + "FirmAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--メタリック価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MetallicPrice", a.MetallicPrice, new { id = idPrefix + "MetallicPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--メタリック消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MetallicTax", a.MetallicTax, new { id = idPrefix + "MetallicTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--メタリック税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "MetallicAmount", a.MetallicAmount, new { id = idPrefix + "MetallicAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--加装価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "EquipmentPrice", a.EquipmentPrice, new { id = idPrefix + "EquipmentPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--加修価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "RepairPrice", a.RepairPrice, new { id = idPrefix + "RepairPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--その他価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OthersPrice", a.OthersPrice, new { id = idPrefix + "OthersPrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--その他消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OthersTax", a.OthersTax, new { id = idPrefix + "OthersTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--その他税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "OthersAmount", a.OthersAmount, new { id = idPrefix + "OthersAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--自税充当-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "CarTaxAppropriatePrice", a.CarTaxAppropriatePrice, new { id = idPrefix + "CarTaxAppropriatePrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--リサイクル-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "RecyclePrice", a.RecyclePrice, new { id = idPrefix + "RecyclePrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オークション落札料-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "AuctionFeePrice", a.AuctionFeePrice, new { id = idPrefix + "AuctionFeePrice", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オークション落札料消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "AuctionFeeTax", a.AuctionFeeTax, new { id = idPrefix + "AuctionFeeTax", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--オークション落札料税込-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "AuctionFeeAmount", a.AuctionFeeAmount, new { id = idPrefix + "AuctionFeeAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--仕入価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Amount", a.Amount, new { id = idPrefix + "Amount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--消費税-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "TaxAmount", a.TaxAmount, new { id = idPrefix + "TaxAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--仕入税込価格-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "TotalAmount", a.TotalAmount, new { id = idPrefix + "TotalAmount", @class = "readonly", @readonly = "readonly"})%>
            </td>
            <!--備考-->
            <td style="white-space:nowrap;">
                <%=Html.TextBox(namePrefix + "Memo", a.Memo, new { id = idPrefix + "Memo", @class = "readonly", @readonly = "readonly"})%>
            </td>

        </tr>
        <%i++;
          } %>
    </table>
    <div id="Div1" style="display:none">
    <img id="Img1" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
    </div>

<br />
    <%} %>
</asp:Content>