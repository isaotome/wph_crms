<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2018/10/25 yano #3947　車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
   // Mod 2017/04/23 arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<table class="input-form-slim">

 <%  //Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更
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
            <%=Html.CheckBox(Model.Prefix + "ConfirmDriverLicense", Model.ConfirmDriverLicense!=null && Model.ConfirmDriverLicense.Equals(true), new { @class = "readonly", onclick = "return false;"}) %>免許証
        </td>
        <td colspan="2" style="background-color:#b0d3ff;">
            <%=Html.CheckBox(Model.Prefix + "ConfirmCertificationSeal", Model.ConfirmCertificationSeal!=null && Model.ConfirmCertificationSeal.Equals(true),  new { @class = "readonly", onclick = "return false;"}) %>印鑑証明
        </td>
        <td colspan="4" style="background-color:#b0d3ff;">
            その他&nbsp;<%=Html.TextBox(Model.Prefix + "ConfirmOther", Model.ConfirmOther,  new { @class = "readonly", @readonly = "readonly"  ,style = "width:300px", maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            納車日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "SalesDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["SalesGengouList"], new { @disabled = "disabled" })%><%=Html.Hidden(Model.Prefix + "SalesDateWareki.Gengou", Model.SalesDateWareki!=null ? Model.SalesDateWareki.Gengou : null) %>
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Year", Model.SalesDateWareki != null ? Model.SalesDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Month", Model.SalesDateWareki != null ? Model.SalesDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
            <%=Html.TextBox(Model.Prefix + "SalesDateWareki.Day", Model.SalesDateWareki != null ? Model.SalesDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
        </td>
        <td></td>
        <th style="width: 100px">
            点検日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "InspectionDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["InspectionGengouList"], new { @disabled = "disabled" })%><%=Html.Hidden(Model.Prefix + "InspectionDateWareki.Gengou", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Gengou : null) %>
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Year", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Month", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
            <%=Html.TextBox(Model.Prefix + "InspectionDateWareki.Day", Model.InspectionDateWareki != null ? Model.InspectionDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
        </td>
        <td></td>
        <th style="width: 100px">
            次回点検日
        </th>
        <td colspan="2">
            <%=Html.DropDownList(Model.Prefix + "NextInspectionDateWareki.Gengou", (IEnumerable<SelectListItem>)ViewData["NextInspectionGengouList"], new { @disabled = "diabled" })%><%=Html.Hidden(Model.Prefix + "NextInspectionDateWareki.Gengou", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Gengou : null) %>
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Year", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Year : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>年
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Month", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Month : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>月
            <%=Html.TextBox(Model.Prefix + "NextInspectionDateWareki.Day", Model.NextInspectionDateWareki != null ? Model.NextInspectionDateWareki.Day : null, new { @class = "numeric readonly", style = "width:15px", maxlength = 2, @readonly = "readonly" })%>日
        </td>
        <td></td>
    </tr>
    <tr>
        <th>
            生産日
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "ProductionDate", string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" }) %>
        </td>
        <th>
            修復歴
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "ReparationRecord", (IEnumerable<SelectListItem>)ViewData["ReparationRecordList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "ReparationRecord", Model.ReparationRecord) %>
        </td>
        <th style="width: 100px">
            シリアル
        </th>
        <td style="width: 80px">
            <%=Html.TextBox(Model.Prefix+ "UsVin", Model.UsVin, new { @class = "alphanumeric readonly", style="width:80px", maxlength = 20, @readonly = "readonly" })%>
        </td>
        <th style="width: 100px">
            メーカー保証
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "MakerWarranty", (IEnumerable<SelectListItem>)ViewData["MakerWarrantyList"], new { style = "width:100%", @disabled = "disabled" }) %>
            <%=Html.Hidden(Model.Prefix + "MakerWarranty", Model.MakerWarranty) %>
        </td>
        <th style="width: 100px">
            記録簿
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "RecordingNote", (IEnumerable<SelectListItem>)ViewData["RecordingNoteList"], new { style = "width:100%", @disabled = "disabled" }) %>
            <%=Html.Hidden(Model.Prefix + "RecordingNote", Model.RecordingNote) %>
        </td>
    </tr>
    <tr>
        <th>
            お客様指定オイル
        </th>
        <td colspan="5">
            <%=CommonUtils.DefaultNbsp(ViewData["OilName"])%>
        </td>
        <th>
            お客様指定タイヤ
        </th>
        <td colspan="5">
            <%=Html.TextBox(Model.Prefix + "Tire", Model.Tire, new { @class = "alphanumeric readonly", size = 10, maxlength = 25, @readonly = "readonly"})%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "Tire", Model.Prefix + "TireName", "'/Parts/CriteriaDialog'", "1" }); %>
            <%=Html.TextBox(Model.Prefix + "TireName", ViewData["TireName"], new { @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            キーコード
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "KeyCode", Model.KeyCode, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 50, @readonly = "readonly" })%>
        </td>
        <th>
            オーディオコード
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "AudioCode", Model.AudioCode, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 50, @readonly = "readonly" })%>
        </td>
        <th>
            輸入
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Import", (IEnumerable<SelectListItem>)ViewData["ImportList"], new { style = "width:100%",@disabled="disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Import", Model.Import) %>
        </td>
        <th>
            保証
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Guarantee", (IEnumerable<SelectListItem>)ViewData["GuaranteeList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Guarantee", Model.Guarantee) %>
        </td>
        <th>
            取説
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Instructions", (IEnumerable<SelectListItem>)ViewData["InstructionsList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Instructions", Model.Instructions) %>
        </td>
        <th>
            クーポン
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "CouponPresence", (IEnumerable<SelectListItem>)ViewData["CouponPresenceList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "CouponPresence", Model.CouponPresence) %>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Recycle", (IEnumerable<SelectListItem>)ViewData["RecycleList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Recycle", Model.Recycle) %>
        </td>
        <th>
            備考1
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo1", Model.Memo1, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考2
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo2", Model.Memo2, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考3
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo3", Model.Memo3, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考4
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo4", Model.Memo4, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考5
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo5", Model.Memo5, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル券
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "RecycleTicket", (IEnumerable<SelectListItem>)ViewData["RecycleTicketList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "RecycleTicket", Model.RecycleTicket) %>
        </td>
        <th>
            備考6
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo6", Model.Memo6, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考7
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo7", Model.Memo7, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考8
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo8", Model.Memo8, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考9
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo9", Model.Memo9, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            備考10
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "Memo10", Model.Memo10, new { style = "width:80px", maxlength = 100, @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            認定中古車No
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "ApprovedCarNumber", Model.ApprovedCarNumber, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 50, @readonly = "readonly" })%>
        </td>
        <th colspan="2">
            認定中古車保証期間
        </th>
        <td colspan="2">
            <%=Html.TextBox(Model.Prefix + "ApprovedCarWarrantyDateFrom", string.Format("{0:yyyy/MM/dd}", Model.ApprovedCarWarrantyDateFrom), new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" })%>～<%=Html.TextBox("ApprovedCarWarrantyDateTo", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateTo), new { @class = "alphanumeric readonly", style="width:80px", maxlength = 10, @readonly = "readonly" }) %>
        </td>
        <th>
            シート(色)
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "SeatColor", Model.SeatColor, new { style = "width:80px", maxlength = 10, @class = "readonly", @readonly = "readonly" })%>
        </td>
        <th>
            シート
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "SeatType", (IEnumerable<SelectListItem>)ViewData["SeatTypeList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "SeatType", Model.SeatType) %>
        </td>
        <th>
            ライト
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Light", (IEnumerable<SelectListItem>)ViewData["LightList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Light", Model.Light) %>
        </td>
    </tr>
    <tr>
        <th>
            AW
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Aw", (IEnumerable<SelectListItem>)ViewData["AwList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Aw", Model.Aw) %>
        </td>
        <th>
            エアロ
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Aero", (IEnumerable<SelectListItem>)ViewData["AeroList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Aero", Model.Aero) %>
        </td>
        <th>
            SR
        </th>
        <td colspan="3">
            <%=Html.DropDownList(Model.Prefix + "Sr", (IEnumerable<SelectListItem>)ViewData["SrList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Sr", Model.Sr) %>
        </td>
        <th>
            CD
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Cd", (IEnumerable<SelectListItem>)ViewData["CdList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Cd", Model.Cd) %>
        </td>
        <th>
            MD
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Md", (IEnumerable<SelectListItem>)ViewData["MdList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Md", Model.Md) %>
        </td>
    </tr>
    <tr>
        <th>
            ナビ
        </th>
        <td colspan="4">
            &nbsp;
            製造：<%=Html.DropDownList(Model.Prefix + "NaviType", (IEnumerable<SelectListItem>)ViewData["NaviTypeList"], new { @disabled = "disabled" })%>
            媒体：<%=Html.DropDownList(Model.Prefix + "NaviEquipment", (IEnumerable<SelectListItem>)ViewData["NaviEquipmentList"], new { @disabled = "disabled" })%>
            位置：<%=Html.DropDownList(Model.Prefix + "NaviDashboard", (IEnumerable<SelectListItem>)ViewData["NaviDashboardList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "NaviType",Model.NaviType) %>
            <%=Html.Hidden(Model.Prefix + "NaviEquipment",Model.NaviEquipment) %>
            <%=Html.Hidden(Model.Prefix + "NaviDashboard",Model.NaviDashboard) %>
        </td>
        <td colspan="7">
        </td>
    </tr>
    <tr>
        <th>
            申告区分
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "DeclarationType", (IEnumerable<SelectListItem>)ViewData["DeclarationTypeList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "DeclarationType", Model.DeclarationType) %>
        </td>
        <th>
            取得原因
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "AcquisitionReason", (IEnumerable<SelectListItem>)ViewData["AcquisitionReasonList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "AcquisitionReason", Model.AcquisitionReason)%>
        </td>
        <th>
            課税区分(種別割)<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "TaxationTypeCarTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeCarTaxList"], new { style = "width:100%", @disabled ="disabled" })%>
        </td>
        <th>
            課税区分(環境性能割)<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "TaxationTypeAcquisitionTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeAcquisitionTaxList"], new { style = "width:100%", @disabled ="disabled" })%>
            <%=Html.Hidden(Model.Prefix + "TaxationTypeAcquisitionTax", Model.TaxationTypeAcquisitionTax, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
        <th>
            抹消登録
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "EraseRegist", (IEnumerable<SelectListItem>)ViewData["EraseRegistList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "EraseRegist",Model.EraseRegist) %>
        </td>
        <th>
            ファイナンス
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "Finance", (IEnumerable<SelectListItem>)ViewData["FinanceList"], new { style = "width:100%", @disabled = "disabled" }) %>
            <%=Html.Hidden(Model.Prefix + "Finance", Model.Finance) %>
       </td>
    </tr>
    <tr>
        <th>
            自動車税種別割<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarTax", string.Format("{0:N0}", Model.CarTax), new { @class = "money readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" })%>
            <%=Html.Hidden(Model.Prefix + "CarTax", Model.CarTax, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
        <th>
            重量税
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarWeightTax", string.Format("{0:N0}", Model.CarWeightTax), new { @class = "money readonly", style = "width:80px", maxlength = 10, @readonly = "readonly"  })%>
            <%=Html.Hidden(Model.Prefix + "CarWeightTax",Model.CarWeightTax, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
        <th>
            自動車税環境性能割<%-- Mod 2019/09/04 yano #4011 --%>
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "AcquisitionTax", string.Format("{0:N0}", Model.AcquisitionTax), new { @class = "money readonly", style = "width:80px", maxlength = 10, @readonly = "readonly"  })%>
            <%=Html.Hidden(Model.Prefix + "AcquisitionTax",Model.AcquisitionTax, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
        <th>
            自賠責保険料
        </th>
        <td>
            <%=Html.TextBox(Model.Prefix + "CarLiabilityInsurance", string.Format("{0:N0}", Model.CarLiabilityInsurance), new { @class = "money readonly", style = "width:80px", maxlength = 10 , @readonly = "readonly" })%>
            <%=Html.Hidden(Model.Prefix + "CarLiabilityInsurance",Model.CarLiabilityInsurance, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
        <th>
            リサイクル預託金
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "RecycleDeposit", string.Format("{0:N0}", Model.RecycleDeposit), new { @class = "money readonly", style = "width:80px", maxlength = 10, @readonly = "readonly"  })%>
            <%=Html.Hidden(Model.Prefix + "RecycleDeposit",Model.RecycleDeposit, new { @class = "money" })%><%//2017/04/23 arc yano #3755 %>
        </td>
    </tr>
    <tr>
        <th>
            走行距離
        </th>
        <td colspan="3">
                <%=Html.TextBox(Model.Prefix+"Mileage", Model.Mileage, new { maxlength = "10", style = "width:64px", @class = "numeric readonly", @readonly = "readonly" })%><%=Html.DropDownList(Model.Prefix+"MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { style = "height:20px", @disabled = "disabled" })%>
                <%=Html.Hidden(Model.Prefix + "MileageUnit", Model.MileageUnit) %>
        </td>
        <th rowspan="2">
            書類備考
        </th>
        <td rowspan="2" colspan="7">
            <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
            <%=Html.TextArea(Model.Prefix + "DocumentRemarks", Model.DocumentRemarks, 3, 32, new { style = "width:678px;height:40px", @class = "readonly", @readonly = "readonly" , wrap = "virtual", onblur = "checkTextLength('"+idPrefix+"DocumentRemarks', 100, '書類備考')" })%>
<%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
        </td>
    </tr>
    <tr>
        <th>
            書類
        </th>
        <td colspan="3">
            <%=Html.DropDownList(Model.Prefix + "DocumentComplete", (IEnumerable<SelectListItem>)ViewData["DocumentCompleteList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "DocumentComplete", Model.DocumentComplete) %>
        </td>
    </tr>
 </table>
