<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2018/10/25 yano #3947　車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<table class="input-form-slim">
    <%  //Add 2014/07/22 arc yano chrome対応 id追加
        string idPrefix = "";

        if (Model.Prefix != null)
        {
            idPrefix = Model.Prefix.Replace(".", "_");
        }  
    %>

    <tr>
        <th class="input-form-title" colspan="12">
            車両詳細情報及び法定費用
        </th>
    </tr>
    <%-- Add 2018/10/25 yano #3947 --%>
    <tr>
         <th style="width: 400px" colspan="4">
           古物取引時相手の確認方法&nbsp;&nbsp;(*)
        </th>
        <td colspan="2" style="background-color:#b0d3ff;">
            <%=Html.CheckBox(Model.Prefix + "ConfirmDriverLicense", Model.ConfirmDriverLicense!=null && Model.ConfirmDriverLicense.Equals(true), new { id = idPrefix + "ConfirmDriverLicense"}) %>免許証
        </td>
        <td colspan="2" style="background-color:#b0d3ff;">
            <%=Html.CheckBox(Model.Prefix + "ConfirmCertificationSeal", Model.ConfirmCertificationSeal!=null && Model.ConfirmCertificationSeal.Equals(true), new { id = idPrefix + "ConfirmCertificationSeal"}) %>印鑑証明
        </td>
        <td colspan="4" style="background-color:#b0d3ff;">
            その他&nbsp;<%=Html.TextBox(Model.Prefix + "ConfirmOther", Model.ConfirmOther, new { id = idPrefix + "ConfirmOther", style = "width:300px", maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            納車日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "SalesDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["SalesGengouList"], new { id = idPrefix + "SalesDateWareki.Gengou", @disabled = "disabled" })%><%=Html.Hidden(Model.Prefix + "SalesDateWareki.Gengou", Model.SalesDateWareki!=null ? Model.SalesDateWareki.Gengou : null, new {id = idPrefix + "SalesDateWareki.Gengou",}) %>
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Year", Model.SalesDateWareki != null ? Model.SalesDateWareki.Year : null, new { id = idPrefix + "SalesDateWareki.Year", @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Month", Model.SalesDateWareki != null ? Model.SalesDateWareki.Month : null, new { id = idPrefix + "SalesDateWareki.Month" , @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Day", Model.SalesDateWareki != null ? Model.SalesDateWareki.Day : null, new { id = idPrefix + "SalesDateWareki.Day" ,@class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
        </td>
        <td></td>
        <th style="width: 100px">
            点検日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "InspectionDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["InspectionGengouList"], new { id = idPrefix + "InspectionDateWareki.Gengou" })%>
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Year", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Year : null, new { id = idPrefix + "InspectionDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Month", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Month : null, new { id = idPrefix + "InspectionDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Day", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Day : null, new { id = idPrefix + "InspectionDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
        </td>
        <td></td>
        <th style="width: 100px">
            次回点検日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "NextInspectionDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["NextInspectionGengouList"], new { id = idPrefix + "NextInspectionDateWareki.Gengou" })%>
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Year", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Year : null, new { id = idPrefix + "NextInspectionDateWareki.Year", @class = "numeric", style = "width:15px", maxlength = 2 })%>年
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Month", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Month : null, new { id = idPrefix + "NextInspectionDateWareki.Month", @class = "numeric", style = "width:15px", maxlength = 2 })%>月
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Day", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Day : null, new { id = idPrefix + "NextInspectionDateWareki.Day", @class = "numeric", style = "width:15px", maxlength = 2 })%>日
        </td>
        <td></td>
    </tr>
    <tr>
        <th>
            生産日
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "ProductionDate", string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), new { id = idPrefix + "ProductionDate", @class = "alphanumeric", style="width:80px", maxlength = 10 })%>
        </td>
        <th>
            修復歴
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "ReparationRecord", (IEnumerable<SelectListItem>)ViewData["ReparationRecordList"], new { id = idPrefix + "ReparationRecord", style = "width:100%" })%>
        </td>
         <th style="width: 100px">
            シリアル
        </th>
        <td style="width: 80px">
            <%=Html.TextBox(Model.Prefix+ "UsVin", Model.UsVin, new { id = idPrefix + "UsVin", @class = "alphanumeric", style="width:80px", maxlength = 20 })%>
        </td>
        <th style="width: 100px">
            メーカー保証
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "MakerWarranty", (IEnumerable<SelectListItem>)ViewData["MakerWarrantyList"], new { id = idPrefix + "MakerWarranty", style = "width:100%" })%>
        </td>
        <th style="width: 100px">
            記録簿
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "RecordingNote", (IEnumerable<SelectListItem>)ViewData["RecordingNoteList"], new { id = idPrefix + "RecordingNote", style = "width:100%" })%>
        </td>
        <th></th>
        <td></td>
    </tr>
    <tr>
         <th>
            お客様指定オイル
        </th>
        <td colspan="5">
            <%=Html.TextBox(Model.Prefix + "Oil", Model.Oil, new { id = idPrefix + "Oil", @class = "alphanumeric", size = 10, maxlength = 25, onblur = "GetNameFromCode('" + idPrefix + "Oil','" + idPrefix + "OilName','Parts')" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "Oil", idPrefix + "OilName", "'/Parts/CriteriaDialog'", "0" }); %>
            <%=Html.TextBox(Model.Prefix + "OilName", ViewData["OilName"], new { id = idPrefix + "OilName", @class = "readonly", @readonly = "readonly", style="width:320px" }) %>
        </td>
        <th>
            お客様指定タイヤ
        </th>
        <td colspan="5">
            <%=Html.TextBox(Model.Prefix + "Tire", Model.Tire, new { id = idPrefix + "Tire", @class = "alphanumeric", size = 10, maxlength = 25, onblur = "GetNameFromCode('" + idPrefix + "Tire','" + idPrefix + "TireName','Parts')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "Tire", idPrefix + "TireName", "'/Parts/CriteriaDialog'", "0" }); %>
            <%=Html.TextBox(Model.Prefix + "TireName", ViewData["TireName"], new { id = idPrefix + "TireName", @class = "readonly", @readonly = "readonly", style="width:320px" }) %>
        </td>
    </tr>
    <tr>
        <th>
            キーコード
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "KeyCode", Model.KeyCode, new { id = idPrefix + "KeyCode", @class = "alphanumeric", style = "width:80px", maxlength = 50 })%>
        </td>
        <th>
            オーディオコード
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "AudioCode", Model.AudioCode, new { id = idPrefix + "AudioCode", @class = "alphanumeric", style = "width:80px", maxlength = 50 })%>
        </td>
        <th>
            輸入
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Import", (IEnumerable<SelectListItem>)ViewData["ImportList"], new { id = idPrefix + "Import", style = "width:100%" })%>
        </td>
        <th>
            保証書
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Guarantee", (IEnumerable<SelectListItem>)ViewData["GuaranteeList"], new { id = idPrefix + "Guarantee", style = "width:100%" })%>
        </td>
        <th>
            取説
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Instructions", (IEnumerable<SelectListItem>)ViewData["InstructionsList"], new { id = idPrefix + "Instructions", style = "width:100%" })%>
        </td>
        <th>
            クーポン
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "CouponPresence", (IEnumerable<SelectListItem>)ViewData["CouponPresenceList"], new { id = idPrefix + "CouponPresence", style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Recycle", (IEnumerable<SelectListItem>)ViewData["RecycleList"], new { id = idPrefix + "Recycle", style = "width:100%" })%>
        </td>
        <th>
            備考1
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo1", Model.Memo1, new { id = idPrefix + "Memo1", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考2
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo2", Model.Memo2, new { id = idPrefix + "Memo2", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考3
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo3", Model.Memo3, new { id = idPrefix + "Memo3", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考4
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo4", Model.Memo4, new { id = idPrefix + "Memo4", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考5
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo5", Model.Memo5, new { id = idPrefix + "Memo5", style = "width:80px", maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル券
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "RecycleTicket", (IEnumerable<SelectListItem>)ViewData["RecycleTicketList"], new { id = idPrefix + "RecycleTicket", style = "width:100%" })%>
        </td>
        <th>
            備考6
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo6", Model.Memo6, new { id = idPrefix + "Memo6", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考7
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo7", Model.Memo7, new { id = idPrefix + "Memo7", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考8
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo8", Model.Memo8, new { id = idPrefix + "Memo8", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考9
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo9", Model.Memo9, new { id = idPrefix + "Memo9", style = "width:80px", maxlength = 100 })%>
        </td>
        <th>
            備考10
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo10", Model.Memo10, new { id = idPrefix + "Memo10", style = "width:80px", maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th>
            認定中古車No
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "ApprovedCarNumber", Model.ApprovedCarNumber, new { id = idPrefix + "ApprovedCarNumber", @class = "alphanumeric", style = "width:80px", maxlength = 50 })%>
        </td>
        <th colspan="2">
            認定中古車保証期間
        </th>
        <td colspan="2">
            <%=Html.TextBox(Model.Prefix + "ApprovedCarWarrantyDateFrom", string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateFrom), new { id = idPrefix + "ApprovedCarWarrantyDateFrom", @class = "alphanumeric", style="width:80px", maxlength = 10 })%>～<%=Html.TextBox("ApprovedCarWarrantyDateTo", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateTo), new { @class = "alphanumeric", style="width:80px", maxlength = 10 }) %>
        </td>
        <th>
            シート(色)
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "SeatColor", Model.SeatColor, new { id = idPrefix + "SeatColor", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            シート
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "SeatType", (IEnumerable<SelectListItem>)ViewData["SeatTypeList"], new { id = idPrefix + "SeatType", style = "width:100%" })%>
        </td>
        <th>
            ライト
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Light", (IEnumerable<SelectListItem>)ViewData["LightList"], new { id = idPrefix + "Light", style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            AW
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Aw", (IEnumerable<SelectListItem>)ViewData["AwList"], new { id = idPrefix + "Aw", style = "width:100%" })%>
        </td>
        <th>
            エアロ
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Aero", (IEnumerable<SelectListItem>)ViewData["AeroList"], new {id = idPrefix + "Aero",  style = "width:100%" })%>
        </td>
        <th>
            SR
        </th>
        <td colspan="3">
            <%=Html.DropDownList(Model.Prefix + "Sr", (IEnumerable<SelectListItem>)ViewData["SrList"], new { id = idPrefix + "Sr", style = "width:100%" })%>
        </td>
        <th>
            CD
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Cd", (IEnumerable<SelectListItem>)ViewData["CdList"], new { id = idPrefix + "Cd", style = "width:100%" })%>
        </td>
        <th>
            MD
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Md", (IEnumerable<SelectListItem>)ViewData["MdList"], new {  id = idPrefix + "Md", style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            ナビ
        </th>
        <td colspan="4">
            &nbsp;
            製造：<%=Html.DropDownList(Model.Prefix + "NaviType", (IEnumerable<SelectListItem>)ViewData["NaviTypeList"], new { id = idPrefix + "NaviType"})%>
            媒体：<%=Html.DropDownList(Model.Prefix + "NaviEquipment", (IEnumerable<SelectListItem>)ViewData["NaviEquipmentList"], new { id = idPrefix + "NaviEquipment"})%>
            位置：<%=Html.DropDownList(Model.Prefix + "NaviDashboard", (IEnumerable<SelectListItem>)ViewData["NaviDashboardList"], new { id = idPrefix + "NaviDashboard"})%>
        </td>
        <td colspan="7">
        </td>
    </tr>
    <tr>
        <th>
            申告区分
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "DeclarationType", (IEnumerable<SelectListItem>)ViewData["DeclarationTypeList"], new { id = idPrefix + "DeclarationType", style = "width:100%" })%>
        </td>
        <th>
            取得原因
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "AcquisitionReason", (IEnumerable<SelectListItem>)ViewData["AcquisitionReasonList"], new { id = idPrefix + "AcquisitionReason", style = "width:100%" })%>
        </td>
        <th>
            課税区分(種別割)<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "TaxationTypeCarTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeCarTaxList"], new { id = idPrefix + "TaxationTypeCarTax", style = "width:100%" })%>
        </td>
        <th>
            課税区分(環境性能割)<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "TaxationTypeAcquisitionTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeAcquisitionTaxList"], new { id = idPrefix + "TaxationTypeAcquisitionTax", style = "width:100%" })%>
        </td>
        <th>
            抹消登録
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "EraseRegist", (IEnumerable<SelectListItem>)ViewData["EraseRegistList"], new {  id = idPrefix + "EraseRegist", style = "width:100%" })%>
        </td>
        <th>
            ファイナンス
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Finance", (IEnumerable<SelectListItem>)ViewData["FinanceList"], new { id = idPrefix + "Finance", style = "width:100%" }) %>
        </td>
    </tr>
    <tr>
        <th>
            自動車税種別割<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarTax", string.Format("{0:N0}", Model.CarTax), new { id = idPrefix + "CarTax", @class = "money", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            重量税
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { id = idPrefix + "CarWeightTax", @class = "money", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            自動車税環境性能割<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "AcquisitionTax", string.Format("{0:N0}", Model.AcquisitionTax), new { id = idPrefix + "AcquisitionTax", @class = "money", style = "width:80px", maxlength = 10  })%>
        </td>
        <th>
            自賠責保険料
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { id = idPrefix + "CarLiabilityInsurance", @class = "money", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            リサイクル預託金
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "RecycleDeposit", string.Format("{0:N0}", Model.RecycleDeposit), new { id = idPrefix + "RecycleDeposit", @class = "money", style = "width:80px", maxlength = 10})%>
        </td>
    </tr>
    <tr>
        <th>
            走行距離
        </th>
        <td colspan="3">
                <%=Html.TextBox(Model.Prefix+"Mileage", Model.Mileage, new { id = idPrefix + "Mileage", maxlength = "10", style = "width:64px", @class = "numeric" })%><%=Html.DropDownList(Model.Prefix+"MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { id = idPrefix + "MileageUnit", style = "height:20px" })%>
        </td>
        <th rowspan="2">
            書類備考
        </th>
        <td rowspan="2" colspan="7">
			 <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
            <%=Html.TextArea(Model.Prefix + "DocumentRemarks", Model.DocumentRemarks, 3, 32, new { id = idPrefix + "DocumentRemarks", style = "width:678px;height:40px", wrap = "virtual", onblur = "checkTextLength('"+idPrefix+"DocumentRemarks', 100, '書類備考')" })%>
        </td>
    </tr>
    <tr>
        <th>
            書類
        </th>
        <td colspan="3">
            <%=Html.DropDownList(Model.Prefix + "DocumentComplete", (IEnumerable<SelectListItem>)ViewData["DocumentCompleteList"], new { id = idPrefix + "DocumentComplete", style = "width:100%" })%>
        </td>
    </tr>
</table>
