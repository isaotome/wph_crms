<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao"  %>
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">Ô¼î{îñ</th>
      </tr>
      <tr>
        <th>ÇÔ</th>
        <td><%if (ViewData["update"].Equals("1")) { %><%=CommonUtils.DefaultNbsp(ViewData["SalesCarNumber"], 20)%><%} else { %><%=Html.TextBox("SalesCar.SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric", size = "15", maxlength = "20",onblur="IsExistCode('SalesCarNumber','SalesCar')" })%><%} %></td>
        <th>O[h *</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onchange = "GetMasterDetailFromCode('SalesCar_CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','SalesCar_MakerName','SalesCar_Capacity','SalesCar_MaximumLoadingWeight','SalesCar_CarWeight','SalesCar_TotalCarWeight','SalesCar_Length','SalesCar_Width','SalesCar_Height','SalesCar_FFAxileWeight','SalesCar_FRAxileWeight','SalesCar_RFAxileWeight','SalesCar_RRAxileWeight','SalesCar_ModelName','SalesCar_EngineType','SalesCar_Displacement','SalesCar_Fuel','SalesCar_ModelSpecificateNumber','SalesCar_ClassificationTypeNumber'),'CarGrade');" })%>
            <img alt="O[hõ" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_CarGradeCode', '', '/CarGrade/CriteriaDialog');GetMasterDetailFromCode('SalesCar_CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','SalesCar_MakerName','SalesCar_Capacity','SalesCar_MaximumLoadingWeight','SalesCar_CarWeight','SalesCar_TotalCarWeight','SalesCar_Length','SalesCar_Width','SalesCar_Height','SalesCar_FFAxileWeight','SalesCar_FRAxileWeight','SalesCar_RFAxileWeight','SalesCar_RRAxileWeight','SalesCar_ModelName','SalesCar_EngineType','SalesCar_Displacement','SalesCar_Fuel','SalesCar_ModelSpecificateNumber','SalesCar_ClassificationTypeNumber'),'CarGrade');" />
            &nbsp;&nbsp;<span id="CarBrandName"><%=Html.Encode(ViewData["CarBrandName"])%></span>&nbsp;<span id="CarName"><%=Html.Encode(ViewData["CarName"])%></span>&nbsp;<span id="CarGradeName"><%=Html.Encode(ViewData["CarGradeName"])%></span>
        </td>
        <th>Væª *</th>
        <td><%=Html.DropDownList("SalesCar.NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
        <th>Ì¿i</th>
        <td><%=Html.TextBox("SalesCar.SalesPrice", Model.SalesPrice, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>nF</th>
        <td><%=Html.DropDownList("SalesCar.ColorType", (IEnumerable<SelectListItem>)ViewData["ColorTypeList"])%></td>
        <th>OF</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.ExteriorColorCode", Model.ExteriorColorCode, new { @class = "alphanumeric", size = 10, maxlength = 8, onblur = "GetNameFromCode('SalesCar_ExteriorColorCode','SalesCar_ExteriorColorName','CarColor')" })%>
            <img alt="Ô¼J[õ" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_ExteriorColorCode', 'SalesCar_ExteriorColorName', '/CarColor/CriteriaDialog')" />
            &nbsp;&nbsp;<%=Html.TextBox("SalesCar.ExteriorColorName", Model.ExteriorColorName, new { size = 25, maxlength = 50 })%>
        </td>
        <th>FÖ</th>
        <td><%=Html.DropDownList("SalesCar.ChangeColor", (IEnumerable<SelectListItem>)ViewData["ChangeColorList"])%></td>
        <th>àF</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.InteriorColorCode", Model.InteriorColorCode, new { @class = "alphanumeric", size = 10, maxlength = 8, onblur = "GetNameFromCode('SalesCar_InteriorColorCode','SalesCar_InteriorColorName','CarColor')" })%>
            <img alt="Ô¼J[õ" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_InteriorColorCode', 'SalesCar_InteriorColorName', '/CarColor/CriteriaDialog')" />
            &nbsp;&nbsp;<%=Html.TextBox("SalesCar.InteriorColorName", Model.InteriorColorName, new { size = 25, maxlength = 50 })%>
        </td>
      </tr>
      <tr>
        <th>ÝÉXe[^X</th>
        <td><%=CommonUtils.DefaultNbsp(ViewData["CarStatusName"], 20)%></td>
        <th>ÝÉP[V</th>
        <td><%=CommonUtils.DefaultNbsp(ViewData["LocationName"], 20)%></td>
        <th>N®</th>
        <td><%=Html.TextBox("SalesCar.ManufacturingYear", Model.ManufacturingYear, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>nh</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"])%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th colspan="12" class="input-form-title">ÔØîñ</th>
      </tr>
      <tr>
        <th>¤^Ç</th>
        <td><%=Html.TextBox("SalesCar.MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { size = 10, maxlength = 5 })%></td>
        <th>o^Ô(íÊ)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberType", Model.RegistrationNumberType, new { @class = "alphanumeric", size = 10, maxlength = 3 })%></td>
        <th>o^Ô(©È)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberKana", Model.RegistrationNumberKana, new { size = 10, maxlength = 1 })%></td>
        <th>o^Ô(v[g)</th>
        <td><%=Html.TextBox("SalesCar.RegistrationNumberPlate", Model.RegistrationNumberPlate, new { @class = "alphanumeric", size = 10, maxlength = 4 })%></td>
        <th>o^ú</th>
        <td><%=Html.TextBox("SalesCar.RegistrationDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>xo^(YYYY/MM)</th>
        <td><%=Html.TextBox("SalesCar.FirstRegistrationYear", Model.FirstRegistrationYear, new { size = 10, maxlength = 7 })%></td>
      </tr>
      <tr>
        <th>©®ÔíÊ</th>
        <td><%=Html.DropDownList("SalesCar.CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"])%></td>
        <th>pr</th>
        <td><%=Html.DropDownList("SalesCar.Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"])%></td>
        <th>©æª</th>
        <td><%=Html.DropDownList("SalesCar.UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"])%></td>
        <th>`ó</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"])%></td>
      </tr>
      <tr>
        <th>Ô¼</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.MakerName", Model.MakerName, new { maxlength = 50 })%></td>
        <th>èõ</th>
        <td><%=Html.TextBox("SalesCar.Capacity", Model.Capacity, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>ÅåÏÚÊ</th>
        <td><%=Html.TextBox("SalesCar.MaximumLoadingWeight", Model.MaximumLoadingWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>Ô¼dÊ</th>
        <td><%=Html.TextBox("SalesCar.CarWeight", Model.CarWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>Ô¼dÊ</th>
        <td><%=Html.TextBox("SalesCar.TotalCarWeight", Model.TotalCarWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>ÔäÔ *</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.Vin", Model.Vin, new { maxlength = 20 })%></td>
        <th>·³</th>
        <td><%=Html.TextBox("SalesCar.Length", Model.Length, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th></th>
        <td><%=Html.TextBox("SalesCar.Width", Model.Width, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>³</th>
        <td><%=Html.TextBox("SalesCar.Height", Model.Height, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>OO²d</th>
        <td><%=Html.TextBox("SalesCar.FFAxileWeight", Model.FFAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>Oã²d</th>
        <td><%=Html.TextBox("SalesCar.FRAxileWeight", Model.FRAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>ãO²d</th>
        <td><%=Html.TextBox("SalesCar.RFAxileWeight", Model.RFAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>ãã²d</th>
        <td colspan="5"><%=Html.TextBox("SalesCar.RRAxileWeight", Model.RRAxileWeight, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
      </tr>
      <tr>
        <th>^®</th>
        <td><%=Html.TextBox("SalesCar.ModelName", Model.ModelName, new { size = 10, maxlength = 20 })%></td>
        <th>´®@^®</th>
        <td><%=Html.TextBox("SalesCar.EngineType", Model.EngineType, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>rCÊ</th>
        <td><%=Html.TextBox("SalesCar.Displacement", Model.Displacement, new { @class = "numeric", size = 10, maxlength = 9 })%></td>
        <th>R¿íÞ</th>
        <td><%=Html.TextBox("SalesCar.Fuel", Model.Fuel, new { size = 10, maxlength = 10 })%></td>
        <th>^®wèÔ</th>
        <td><%=Html.TextBox("SalesCar.ModelSpecificateNumber", Model.ModelSpecificateNumber, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>ÞÊæªÔ</th>
        <td><%=Html.TextBox("SalesCar.ClassificationTypeNumber", Model.ClassificationTypeNumber, new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>LÒ¼</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PossesorName"], 20)%></td>
        <th>LÒZ</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PossesorAddress"], 20)%></td>
      </tr>
      <tr>
        <th>gpÒ¼</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["UserName"], 20)%></td>
        <th>gpÒZ</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["UserAddress"], 20)%></td>
      </tr>
      <tr>
        <th>{n</th>
        <td colspan="5"><%=CommonUtils.DefaultNbsp(ViewData["PrincipalPlace"], 20)%></td>
        <th><%=Html.DropDownList("SalesCar.ExpireType", (IEnumerable<SelectListItem>)ViewData["ExpireTypeList"])%>úÀ</th>
        <td><%=Html.TextBox("SalesCar.ExpireDate", string.Format("{0:yyyy/MM/dd}", Model.ExpireDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>s£</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.Mileage", Model.Mileage, new { @class = "numeric", size = 10, maxlength = 13 })%>&nbsp;<%=Html.DropDownList("SalesCar.MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"])%></td>
      </tr>
      <tr>
        <th rowspan="2">õl</th>
        <td rowspan="2" colspan="5"><%=Html.TextArea("SalesCar.Memo", Model.Memo, 3, 60, new { wrap = "physical", onblur = "checkTextLength('SalesCar_Memo', 255, 'õl')" })%></td>
        <th rowspan="2">Þõl</th>
        <td rowspan="2" colspan="3"><%=Html.TextArea("SalesCar.DocumentRemarks", Model.DocumentRemarks, 3, 35, new { wrap = "physical", onblur = "checkTextLength('SalesCar_DocumentRemarks', 100, 'Þõl')" })%></td>
        <th>Þ</th>
        <td><%=Html.DropDownList("SalesCar.DocumentComplete", (IEnumerable<SelectListItem>)ViewData["DocumentCompleteList"])%></td>
      </tr>
      <tr>
        <th>­sú</th>
        <td><%=Html.TextBox("SalesCar.IssueDate", string.Format("{0:yyyy/MM/dd}", Model.IssueDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
      </tr>
    </table>
    <br />
    <table class="input-form" style="width:1235px">
      <tr>
        <th class="input-form-title" colspan="12">Ô¼Ú×îñyÑ@èïp</th>
      </tr>
      <tr>
        <th>[Ôú</th>
        <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", ViewData["SalesDate"]), 20)%></td>
        <th>_ú</th>
        <td><%=Html.TextBox("SalesCar.InspectionDate", string.Format("{0:yyyy/MM/dd}", Model.InspectionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>ñ_ú</th>
        <td><%=Html.TextBox("SalesCar.NextInspectionDate", string.Format("{0:yyyy/MM/dd}", Model.NextInspectionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>VIN(kÄp)</th>
        <td><%=Html.TextBox("SalesCar.UsVin", Model.UsVin, new { @class = "alphanumeric", size = 10, maxlength = 20 })%></td>
        <th>[J[ÛØ</th>
        <td><%=Html.DropDownList("SalesCar.MakerWarranty", (IEnumerable<SelectListItem>)ViewData["MakerWarrantyList"])%></td>
        <th>L^ë</th>
        <td><%=Html.DropDownList("SalesCar.RecordingNote", (IEnumerable<SelectListItem>)ViewData["RecordingNoteList"])%></td>
      </tr>
      <tr>
        <th>¶Yú</th>
        <td><%=Html.TextBox("SalesCar.ProductionDate", string.Format("{0:yyyy/MM/dd}", Model.ProductionDate), new { @class = "alphanumeric", size = 10, maxlength = 10 })%></td>
        <th>Cð</th>
        <td><%=Html.DropDownList("SalesCar.ReparationRecord", (IEnumerable<SelectListItem>)ViewData["ReparationRecordList"])%></td>
        <th>¨qlwèIC</th>
        <td colspan="3"><%=CommonUtils.DefaultNbsp(ViewData["OilName"])%></td>
        <th>^C</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.Tire", Model.Tire, new { @class = "alphanumeric", size = 10, maxlength = 25, onblur = "GetNameFromCode('SalesCar_Tire','TireName','Parts')" })%>
            <img alt="iõ" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCar_Tire', 'TireName', '/Parts/CriteriaDialog')" />
            &nbsp;&nbsp;<span id="TireName"><%=Html.Encode(ViewData["TireName"])%></span>
        </td>
      </tr>
      <tr>
        <th>L[R[h</th>
        <td><%=Html.TextBox("SalesCar.KeyCode", Model.KeyCode, new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th>I[fBIR[h</th>
        <td><%=Html.TextBox("SalesCar.AudioCode", Model.AudioCode, new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th>Aü</th>
        <td><%=Html.DropDownList("SalesCar.Import", (IEnumerable<SelectListItem>)ViewData["ImportList"])%></td>
        <th>ÛØ</th>
        <td><%=Html.DropDownList("SalesCar.Guarantee", (IEnumerable<SelectListItem>)ViewData["GuaranteeList"])%></td>
        <th>æà</th>
        <td><%=Html.DropDownList("SalesCar.Instructions", (IEnumerable<SelectListItem>)ViewData["InstructionsList"])%></td>
        <th>N[|</th>
        <td><%=Html.DropDownList("SalesCar.CouponPresence", (IEnumerable<SelectListItem>)ViewData["CouponPresenceList"])%></td>
      </tr>
      <tr>
        <th>TCN</th>
        <td><%=Html.DropDownList("SalesCar.Recycle", (IEnumerable<SelectListItem>)ViewData["RecycleList"])%></td>
        <th>õl1</th>
        <td><%=Html.TextBox("SalesCar.Memo1", Model.Memo1, new { size = 10, maxlength = 100 })%></td>
        <th>õl2</th>
        <td><%=Html.TextBox("SalesCar.Memo2", Model.Memo2, new { size = 10, maxlength = 100 })%></td>
        <th>õl3</th>
        <td><%=Html.TextBox("SalesCar.Memo3", Model.Memo3, new { size = 10, maxlength = 100 })%></td>
        <th>õl4</th>
        <td><%=Html.TextBox("SalesCar.Memo4", Model.Memo4, new { size = 10, maxlength = 100 })%></td>
        <th>õl5</th>
        <td><%=Html.TextBox("SalesCar.Memo5", Model.Memo5, new { size = 10, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>TCN</th>
        <td><%=Html.DropDownList("SalesCar.RecycleTicket", (IEnumerable<SelectListItem>)ViewData["RecycleTicketList"])%></td>
        <th>õl6</th>
        <td><%=Html.TextBox("SalesCar.Memo6", Model.Memo6, new { size = 10, maxlength = 100 })%></td>
        <th>õl7</th>
        <td><%=Html.TextBox("SalesCar.Memo7", Model.Memo7, new { size = 10, maxlength = 100 })%></td>
        <th>õl8</th>
        <td><%=Html.TextBox("SalesCar.Memo8", Model.Memo8, new { size = 10, maxlength = 100 })%></td>
        <th>õl9</th>
        <td><%=Html.TextBox("SalesCar.Memo9", Model.Memo9, new { size = 10, maxlength = 100 })%></td>
        <th>õl10</th>
        <td><%=Html.TextBox("SalesCar.Memo10", Model.Memo10, new { size = 10, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>FèÃÔNo</th>
        <td><%=Html.TextBox("SalesCar.ApprovedCarNumber", Model.ApprovedCarNumber, new { @class = "alphanumeric", size = 15, maxlength = 50 }) %></td>
        <th>FèÃÔÛØúÔ</th>
        <td colspan="9"><%=Html.TextBox("SalesCar.ApprovedCarWarrantyDateFrom", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateFrom), new { @class = "alphanumeric", size = 10, maxlength = 10 }) %>`<%=Html.TextBox("SalesCar.ApprovedCarWarrantyDateTo", string.Format("{0:yyyy/MM/dd}",Model.ApprovedCarWarrantyDateTo), new { @class = "alphanumeric", size = 10, maxlength = 10 }) %></td>
      </tr>
      <tr>
        <th>Cg</th>
        <td><%=Html.DropDownList("SalesCar.Light", (IEnumerable<SelectListItem>)ViewData["LightList"])%></td>
        <th>AW</th>
        <td><%=Html.DropDownList("SalesCar.Aw", (IEnumerable<SelectListItem>)ViewData["AwList"])%></td>
        <th>GA</th>
        <td><%=Html.DropDownList("SalesCar.Aero", (IEnumerable<SelectListItem>)ViewData["AeroList"])%></td>
        <th>SR</th>
        <td><%=Html.DropDownList("SalesCar.Sr", (IEnumerable<SelectListItem>)ViewData["SrList"])%></td>
        <th>CD</th>
        <td><%=Html.DropDownList("SalesCar.Cd", (IEnumerable<SelectListItem>)ViewData["CdList"])%></td>
        <th>MD</th>
        <td><%=Html.DropDownList("SalesCar.Md", (IEnumerable<SelectListItem>)ViewData["MdList"])%></td>
      </tr>
      <tr>
        <th>ir</th>
        <td colspan="3">»¢F<%=Html.DropDownList("SalesCar.NaviType", (IEnumerable<SelectListItem>)ViewData["NaviTypeList"])%>
            }ÌF<%=Html.DropDownList("SalesCar.NaviEquipment", (IEnumerable<SelectListItem>)ViewData["NaviEquipmentList"])%>
            ÊuF<%=Html.DropDownList("SalesCar.NaviDashboard", (IEnumerable<SelectListItem>)ViewData["NaviDashboardList"])%>
        </td>
        <th>V[g(F)</th>
        <td><%=Html.TextBox("SalesCar.SeatColor", Model.SeatColor, new { size = 10, maxlength = 10 })%></td>
        <th>V[g</th>
        <td colspan="5"><%=Html.DropDownList("SalesCar.SeatType", (IEnumerable<SelectListItem>)ViewData["SeatTypeList"])%></td>
      </tr>
      <tr>
        <th>\æª</th>
        <td><%=Html.DropDownList("SalesCar.DeclarationType", (IEnumerable<SelectListItem>)ViewData["DeclarationTypeList"])%></td>
        <th>æ¾´ö</th>
        <td><%=Html.DropDownList("SalesCar.AcquisitionReason", (IEnumerable<SelectListItem>)ViewData["AcquisitionReasonList"])%></td>
        <th>ÛÅæª(©®ÔÅ)</th>
        <td><%=Html.DropDownList("SalesCar.TaxationTypeCarTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeCarTaxList"])%></td>
        <th>ÛÅæª(©®Ôæ¾Å)</th>
        <td><%=Html.DropDownList("SalesCar.TaxationTypeAcquisitionTax", (IEnumerable<SelectListItem>)ViewData["TaxationTypeAcquisitionTaxList"])%></td>
        <th>Áo^</th>
        <td colspan="3"><%=Html.DropDownList("EraseRegist",(IEnumerable<SelectListItem>)ViewData["EraseRegistList"]) %></td>
      </tr>
      <tr>
        <th>©®ÔÅ</th>
        <td><%=Html.TextBox("SalesCar.CarTax", Model.CarTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>©®ÔdÊÅ</th>
        <td><%=Html.TextBox("SalesCar.CarWeightTax", Model.CarWeightTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>©®Ôæ¾Å</th>
        <td><%=Html.TextBox("SalesCar.AcquisitionTax", Model.AcquisitionTax, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>©ÓÛ¯¿</th>
        <td><%=Html.TextBox("SalesCar.CarLiabilityInsurance", Model.CarLiabilityInsurance, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
        <th>TCNaõà</th>
        <td colspan="3"><%=Html.TextBox("SalesCar.RecycleDeposit", Model.RecycleDeposit, new { @class = "numeric", size = 10, maxlength = 10 })%></td>
      </tr>
    </table>
