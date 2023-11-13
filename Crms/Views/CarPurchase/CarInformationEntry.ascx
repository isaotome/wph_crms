<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao"  %>
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">車両基本情報</th>
      </tr>
      <tr>
        <th>管理番号</th>
        <td><%if (ViewData["update"].Equals("1")) { %><%=CommonUtils.DefaultNbsp(ViewData["SalesCarNumber"], 20)%><%} else { %><%=Html.TextBox("SalesCar.SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric", size = "15", maxlength = "20",onblur="IsExistCode('SalesCarNumber','SalesCar')" })%><%} %></td>
        <th>グレード *</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onchange = "GetMasterDetailFromCode('SalesCar_CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','SalesCar_MakerName','SalesCar_Capacity','SalesCar_MaximumLoadingWeight','SalesCar_CarWeight','SalesCar_TotalCarWeight','SalesCar_Length','SalesCar_Width','SalesCar_Height','SalesCar_FFAxileWeight','SalesCar_FRAxileWeight','SalesCar_RFAxileWeight','SalesCar_RRAxileWeight','SalesCar_ModelName','SalesCar_EngineType','SalesCar_Displacement','SalesCar_Fuel','SalesCar_ModelSpecificateNumber','SalesCar_ClassificationTypeNumber'),'CarGrade');" })%>
            <img alt="グレード検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_CarGradeCode', '', '/CarGrade/CriteriaDialog');GetMasterDetailFromCode('SalesCar_CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','SalesCar_MakerName','SalesCar_Capacity','SalesCar_MaximumLoadingWeight','SalesCar_CarWeight','SalesCar_TotalCarWeight','SalesCar_Length','SalesCar_Width','SalesCar_Height','SalesCar_FFAxileWeight','SalesCar_FRAxileWeight','SalesCar_RFAxileWeight','SalesCar_RRAxileWeight','SalesCar_ModelName','SalesCar_EngineType','SalesCar_Displacement','SalesCar_Fuel','SalesCar_ModelSpecificateNumber','SalesCar_ClassificationTypeNumber'),'CarGrade');" />
            &nbsp;&nbsp;<span id="CarBrandName"><%=Html.Encode(ViewData["CarBrandName"])%></span>&nbsp;<span id="CarName"><%=Html.Encode(ViewData["CarName"])%></span>&nbsp;<span id="CarGradeName"><%=Html.Encode(ViewData["CarGradeName"])%></span>
        </td>
        <th>新中区分 *</th>
        <td><%=Html.DropDownList("SalesCar.NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
        <th>販売価格</th>
        <td><%=Html.TextBox("SalesCar.SalesPrice", Model.SalesPrice, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>系統色</th>
        <td><%=Html.DropDownList("SalesCar.ColorType", (IEnumerable<SelectListItem>)ViewData["ColorTypeList"])%></td>
        <th>外装色</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.ExteriorColorCode", Model.ExteriorColorCode, new { @class = "alphanumeric", size = 10, maxlength = 8, onblur = "GetNameFromCode('SalesCar_ExteriorColorCode','SalesCar_ExteriorColorName','CarColor')" })%>
            <img alt="車両カラー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_ExteriorColorCode', 'SalesCar_ExteriorColorName', '/CarColor/CriteriaDialog')" />
            &nbsp;&nbsp;<%=Html.TextBox("SalesCar.ExteriorColorName", Model.ExteriorColorName, new { size = 25, maxlength = 50 })%>
        </td>
        <th>色替</th>
        <td><%=Html.DropDownList("SalesCar.ChangeColor", (IEnumerable<SelectListItem>)ViewData["ChangeColorList"])%></td>
        <th>内装色</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.InteriorColorCode", Model.InteriorColorCode, new { @class = "alphanumeric", size = 10, maxlength = 8, onblur = "GetNameFromCode('SalesCar_InteriorColorCode','SalesCar_InteriorColorName','CarColor')" })%>
            <img alt="車両カラー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_InteriorColorCode', 'SalesCar_InteriorColorName', '/CarColor/CriteriaDialog')" />
            &nbsp;&nbsp;<%=Html.TextBox("SalesCar.InteriorColorName", Model.InteriorColorName, new { size = 25, maxlength = 50 })%>
        </td>
      </tr>
      <tr>
        <th>在庫ステータス</th>
        <td><%=CommonUtils.DefaultNbsp(ViewData["CarStatusName"], 20)%></td>
        <th>在庫ロケーション</th>
        <td><%=CommonUtils.DefaultNbsp(ViewData["LocationName"], 20)%></td>
        <th>年式</th>
        <td><%=Html.TextBox("SalesCar.ManufacturingYear", Model.ManufacturingYear, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>ハンドル</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"])%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th colspan="12" class="input-form-title">車検証情報</th>
      </tr>
      <tr>
        <th>陸運局</th>
        <td><%=Html.TextBox("SalesCar.MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { size = 10, maxlength = 5 })%></td>
        <th>登録番号(種別)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberType", Model.RegistrationNumberType, new { @class = "alphanumeric", size = 10, maxlength = 3 })%></td>
        <th>登録番号(かな)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberKana", Model.RegistrationNumberKana, new { size = 10, maxlength = 1 })%></td>
        <th>登録番号(プレート)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberPlate", Model.RegistrationNumberPlate, new { @class = "alphanumeric", size = 10, maxlength = 4 })%></td>
        <th>登録日</th>
        <td><%=Html.TextBox("SalesCar.RegistrationDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>初度登録(YYYY/MM)</th>
        <td><%=Html.TextBox("SalesCar.FirstRegistrationYear", Model.FirstRegistrationYear, new { size = 10, maxlength = 7 })%></td>
      </tr>
      <tr>
        <th>自動車種別</th>
        <td><%=Html.DropDownList("SalesCar.CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"])%></td>
        <th>用途</th>
        <td><%=Html.DropDownList("SalesCar.Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"])%></td>
        <th>事自区分</th>
        <td><%=Html.DropDownList("SalesCar.UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"])%></td>
        <th>形状</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"])%></td>
      </tr>
      <tr>
        <th>車名</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.MakerName", Model.MakerName, new { maxlength = 50 })%></td>
        <th>定員</th>
        <td><%=Html.TextBox("SalesCar.Capacity", Model.Capacity, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>最大積載量</th>
        <td><%=Html.TextBox("SalesCar.MaximumLoadingWeight", Model.MaximumLoadingWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>車両重量</th>
        <td><%=Html.TextBox("SalesCar.CarWeight", Model.CarWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>車両総重量</th>
        <td><%=Html.TextBox("SalesCar.TotalCarWeight", Model.TotalCarWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>車台番号 *</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.Vin", Model.Vin, new { maxlength = 20 })%></td>
        <th>長さ</th>
        <td><%=Html.TextBox("SalesCar.Length", Model.Length, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>幅</th>
        <td><%=Html.TextBox("SalesCar.Width", Model.Width, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>高さ</th>
        <td><%=Html.TextBox("SalesCar.Height", Model.Height, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>前前軸重</th>
        <td><%=Html.TextBox("SalesCar.FFAxileWeight", Model.FFAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>前後軸重</th>
        <td><%=Html.TextBox("SalesCar.FRAxileWeight", Model.FRAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>後前軸重</th>
        <td><%=Html.TextBox("SalesCar.RFAxileWeight", Model.RFAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>後後軸重</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.RRAxileWeight", Model.RRAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>型式</th>
        <td><%=Html.TextBox("SalesCar.ModelName", Model.ModelName, new { size = 10, maxlength = 20 })%></td>
        <th>原動機型式</th>
        <td><%=Html.TextBox("SalesCar.EngineType", Model.EngineType, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>排気量</th>
        <td><%=Html.TextBox("SalesCar.Displacement", Model.Displacement, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>燃料種類</th>
        <td><%=Html.TextBox("SalesCar.Fuel", Model.Fuel, new { size = 10, maxlength = 10 })%></td>
        <th>型式指定番号</th>
        <td><%=Html.TextBox("SalesCar.ModelSpecificateNumber", Model.ModelSpecificateNumber, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>類別区分番号</th>
        <td><%=Html.TextBox("SalesCar.ClassificationTypeNumber", Model.ClassificationTypeNumber, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>所有者氏名</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PossesorName"], 20)%></td>
        <th>所有者住所</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PossesorAddress"], 20)%></td>
      </tr>
      <tr>
        <th>使用者氏名</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["UserName"], 20)%></td>
        <th>使用者住所</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["UserAddress"], 20)%></td>
      </tr>
      <tr>
        <th>本拠地</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PrincipalPlace"], 20)%></td>
        <th><%=Html.DropDownList("SalesCar.ExpireType", (IEnumerable<SelectListItem>)ViewData["ExpireTypeList"])%>期限</th>
        <td><%=Html.TextBox("SalesCar.ExpireDate", string.Format("{0:yyyy/MM/dd}", Model.ExpireDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>走行距離</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.Mileage", Model.Mileage, new { @class = "numeric", size = 10, maxlength = 13 })%>&nbsp;<%=Html.DropDownList("SalesCar.MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"])%></td>
      </tr>
      <tr>
        <th rowspan="2">備考</th>
        <td rowspan="2" colspan="5"><%=Html.TextArea("SalesCar.Memo", Model.Memo, 3, 60, new { wrap = "physical", onblur = "checkTextLength('SalesCar_Memo', 255, '備考')" })%></td>
        <th rowspan="2">書類備考</th>
        <td rowspan="2" colspan="3"><%=Html.TextArea("SalesCar.DocumentRemarks", Model.DocumentRemarks, 3, 35, new { wrap = "physical", onblur = "checkTextLength('SalesCar_DocumentRemarks', 100, '書類備考')" })%></td>
        <th>書類</th>
        <td><%=Html.DropDownList("SalesCar.DocumentComplete", (IEnumerable<SelectListItem>)ViewData["DocumentCompleteList"])%></td>
      </tr>
      <tr>
        <th>発行日</th>
        <td><%=Html.TextBox("SalesCar.IssueDate", string.Format("{0:yyyy/MM/dd}", Model.IssueDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">車両詳細情報及び法定費用</th>
      </tr>
      <tr>
        <th>納車日</th>
        <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", ViewData["SalesDate"]), 20)%></td>
        <th>点検日</th>
        <td><%=Html.TextBox("SalesCar.InspectionDate", string.Format("{0:yyyy/MM/dd}", Model.InspectionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>次回点検日</th>
        <td><%=Html.TextBox("SalesCar.NextInspectionDate", string.Format("{0:yyyy/MM/dd}", Model.NextInspectionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>VIN(北米用)</th>
        <td><%=Html.TextBox("SalesCar.UsVin", Model.UsVin, new { @class = "alphanumeric", size = 10, maxlength = 20 })%></td>
        <th>メーカー保証</th>
        <td><%=Html.DropDownList("SalesCar.MakerWarranty", (IEnumerable<SelectListItem>)ViewData["MakerWarrantyList"])%></td>
        <th>記録簿</th>
        <td><%=Html.DropDownList("SalesCar.RecordingNote", (IEnumerable<SelectListItem>)ViewData["RecordingNoteList"])%></td>
      </tr>
      <tr>
        <th>生産日</th>
        <td><%=Html.TextBox("SalesCar.ProductionDate", string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>修復歴</th>
        <td><%=Html.DropDownList("SalesCar.ReparationRecord", (IEnumerable<SelectListItem>)ViewData["ReparationRecordList"])%></td>
        <th>お客様指定オイル</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(ViewData["OilName"])%></td>
        <th>タイヤ</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.Tire", Model.Tire, new { @class = "alphanumeric", size = 10, maxlength = 25, onblur = "GetNameFromCode('SalesCar_Tire','TireName','Parts')" })%>
            <img alt="部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_Tire', 'TireName', '/Parts/CriteriaDialog')" />
            &nbsp;&nbsp;<span id="TireName"><%=Html.Encode(ViewData["TireName"])%></span>
        </td>
      </tr>
      <tr>
        <th>キーコード</th>
        <td><%=Html.TextBox("SalesCar.KeyCode", Model.KeyCode, new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th>オーディオコード</th>
        <td><%=Html.TextBox("SalesCar.AudioCode", Model.AudioCode, new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th>輸入</th>
        <td><%=Html.DropDownList("SalesCar.Import", (IEnumerable<SelectListItem>)ViewData["ImportList"])%></td>
        <th>保証書</th>
        <td><%=Html.DropDownList("SalesCar.Guarantee", (IEnumerable<SelectListItem>)ViewData["GuaranteeList"])%></td>
        <th>取説</th>
        <td><%=Html.DropDownList("SalesCar.Instructions", (IEnumerable<SelectListItem>)ViewData["InstructionsList"])%></td>
        <th>クーポン</th>
        <td><%=Html.DropDownList("SalesCar.CouponPresence", (IEnumerable<SelectListItem>)ViewData["CouponPresenceList"])%></td>
      </tr>
      <tr>
        <th>リサイクル</th>
        <td><%=Html.DropDownList("SalesCar.Recycle", (IEnumerable<SelectListItem>)ViewData["RecycleList"])%></td>
        <th>備考1</th>
        <td><%=Html.TextBox("SalesCar.Memo1", Model.Memo1, new { size = 10, maxlength = 100 })%></td>
        <th>備考2</th>
        <td><%=Html.TextBox("SalesCar.Memo2", Model.Memo2, new { size = 10, maxlength = 100 })%></td>
        <th>備考3</th>
        <td><%=Html.TextBox("SalesCar.Memo3", Model.Memo3, new { size = 10, maxlength = 100 })%></td>
        <th>備考4</th>
        <td><%=Html.TextBox("SalesCar.Memo4", Model.Memo4, new { size = 10, maxlength = 100 })%></td>
        <th>備考5</th>
        <td><%=Html.TextBox("SalesCar.Memo5", Model.Memo5, new { size = 10, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>リサイクル券</th>
        <td><%=Html.DropDownList("SalesCar.RecycleTicket", (IEnumerable<SelectListItem>)ViewData["RecycleTicketList"])%></td>
        <th>備考6</th>
        <td><%=Html.TextBox("SalesCar.Memo6", Model.Memo6, new { size = 10, maxlength = 100 })%></td>
        <th>備考7</th>
        <td><%=Html.TextBox("SalesCar.Memo7", Model.Memo7, new { size = 10, maxlength = 100 })%></td>
        <th>備考8</th>
        <td><%=Html.TextBox("SalesCar.Memo8", Model.Memo8, new { size = 10, maxlength = 100 })%></td>
        <th>備考9</th>
        <td><%=Html.TextBox("SalesCar.Memo9", Model.Memo9, new { size = 10, maxlength = 100 })%></td>
        <th>備考10</th>
        <td><%=Html.TextBox("SalesCar.Memo10", Model.Memo10, new { size = 10, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>認定中古車No</th>
        <td><%=Html.TextBox("SalesCar.ApprovedCarNumber", Model.ApprovedCarNumber, new { @class = "alphanumeric", size = 15, maxlength = 50 }) %></td>
        <th>認定中古車保証期間</th>
        <td colspan="9"><%=Html.TextBox("SalesCar.ApprovedCarWarrantyDateFrom", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateFrom), new { @class = "alphanumeric", size = 10, maxlength = 10 }) %>～<%=Html.TextBox("SalesCar.ApprovedCarWarrantyDateTo", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateTo), new { @class = "alphanumeric", size = 10, maxlength = 10 }) %></td>
      </tr>
      <tr>
        <th>ライト</th>
        <td><%=Html.DropDownList("SalesCar.Light", (IEnumerable<SelectListItem>)ViewData["LightList"])%></td>
        <th>AW</th>
        <td><%=Html.DropDownList("SalesCar.Aw", (IEnumerable<SelectListItem>)ViewData["AwList"])%></td>
        <th>エアロ</th>
        <td><%=Html.DropDownList("SalesCar.Aero", (IEnumerable<SelectListItem>)ViewData["AeroList"])%></td>
        <th>SR</th>
        <td><%=Html.DropDownList("SalesCar.Sr", (IEnumerable<SelectListItem>)ViewData["SrList"])%></td>
        <th>CD</th>
        <td><%=Html.DropDownList("SalesCar.Cd", (IEnumerable<SelectListItem>)ViewData["CdList"])%></td>
        <th>MD</th>
        <td><%=Html.DropDownList("SalesCar.Md", (IEnumerable<SelectListItem>)ViewData["MdList"])%></td>
      </tr>
      <tr>
        <th>ナビ</th>
        <td colspan="3">製造：<%=Html.DropDownList("SalesCar.NaviType", (IEnumerable<SelectListItem>)ViewData["NaviTypeList"])%>
            媒体：<%=Html.DropDownList("SalesCar.NaviEquipment", (IEnumerable<SelectListItem>)ViewData["NaviEquipmentList"])%>
            位置：<%=Html.DropDownList("SalesCar.NaviDashboard", (IEnumerable<SelectListItem>)ViewData["NaviDashboardList"])%>
        </td>
        <th>シート(色)</th>
        <td><%=Html.TextBox("SalesCar.SeatColor", Model.SeatColor, new { size = 10, maxlength = 10 })%></td>
        <th>シート</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.SeatType", (IEnumerable<SelectListItem>)ViewData["SeatTypeList"])%></td>
      </tr>
      <tr>
        <th>申告区分</th>
        <td><%=Html.DropDownList("SalesCar.DeclarationType", (IEnumerable<SelectListItem>)ViewData["DeclarationTypeList"])%></td>
        <th>取得原因</th>
        <td><%=Html.DropDownList("SalesCar.AcquisitionReason", (IEnumerable<SelectListItem>)ViewData["AcquisitionReasonList"])%></td>
        <th>課税区分(自動車税)</th>
        <td><%=Html.DropDownList("SalesCar.TaxationTypeCarTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeCarTaxList"])%></td>
        <th>課税区分(自動車取得税)</th>
        <td><%=Html.DropDownList("SalesCar.TaxationTypeAcquisitionTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeAcquisitionTaxList"])%></td>
        <th>抹消登録</th>
        <td colspan="3"><%=Html.DropDownList("EraseRegist",(IEnumerable<SelectListItem>)ViewData["EraseRegistList"]) %></td>
      </tr>
      <tr>
        <th>自動車税</th>
        <td><%=Html.TextBox("SalesCar.CarTax", Model.CarTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>自動車重量税</th>
        <td><%=Html.TextBox("SalesCar.CarWeightTax", Model.CarWeightTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>自動車取得税</th>
        <td><%=Html.TextBox("SalesCar.AcquisitionTax", Model.AcquisitionTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>自賠責保険料</th>
        <td><%=Html.TextBox("SalesCar.CarLiabilityInsurance", Model.CarLiabilityInsurance, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>リサイクル預託金</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.RecycleDeposit", Model.RecycleDeposit, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
      </tr>
    </table>
