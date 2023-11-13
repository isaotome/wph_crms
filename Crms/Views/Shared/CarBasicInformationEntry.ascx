<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>
<%=Html.Hidden("OldCarGradeCode", Model.CarGradeCode)%><%//Add 2020/11/18 yano #4071  %>
<table class="input-form-slim">
    <tr>
        <th class="input-form-title" colspan="12">
            車両基本情報
        </th>
    </tr>
    <tr>
        <th style="width: 100px">
            管理番号
        </th>
        <td style="width: 80px">
            <%
                //2014/07/14 arc yano chrome対応 パラメータをname→idに変更
                string idPrefix = "";
                if (!string.IsNullOrEmpty(Model.Prefix))
                {
                    idPrefix = Model.Prefix.Replace(".", "_");
                } 
            %>
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <% //UPDATE 2018/06/11 arc kawaji #3895_車両仕入入力　管理番号の非活性化 %>
            <%--<%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))--%>
              <%--{ %>--%>
            <%=Html.TextBox(Model.Prefix + "SalesCarNumber", Model.SalesCarNumber, new { @class = "readonly", style = "width:100px", @readonly = "readonly" }) %>
            <%--<%} else { %>--%>
            <%// Edit 2014/06/26 ajax-エラーハンドリング対応 IsExistCode()の引数(コントローラ名)の誤りの修正%>
            <%--<%=Html.TextBox(Model.Prefix + "SalesCarNumber", Model.SalesCarNumber, new { id = idPrefix + "SalesCarNumber", @class = "alphanumeric", style = "width:100px", maxlength = "20", onblur = "IsExistCode('" + idPrefix + "SalesCarNumber','SalesCar')" }) %>--%>
            <%--<%} %>--%>
        </td>
        <th style="width: 100px">
            グレード <span class="hint">*</span>
        </th>
        <td colspan="5">
            <%//Mod 2020/11/18 yano #4071%>
            <%=Html.TextBox(Model.Prefix + "CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", style = "width:50px", maxlength = 30, onchange = "GetGradeMasterDetailFromCode('" + idPrefix + "CarGradeCode', new Array('" + idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "CarGradeName','" + idPrefix + "MakerName'), new Array('" + idPrefix + "Capacity','" + idPrefix + "MaximumLoadingWeight','" + idPrefix + "CarWeight','" + idPrefix + "TotalCarWeight','" + idPrefix + "Length','" + idPrefix + "Width','" + idPrefix + "Height','" + idPrefix + "FFAxileWeight','" + idPrefix + "FRAxileWeight','" + idPrefix + "RFAxileWeight','" + idPrefix + "RRAxileWeight','" + idPrefix + "ModelName','" + idPrefix + "EngineType','" + idPrefix + "Displacement','" + idPrefix + "Fuel','" + idPrefix + "ModelSpecificateNumber','" + idPrefix + "ClassificationTypeNumber'), 'OldCarGradeCode');" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CarGradeCode", idPrefix + "CarGradeName", "'/CarGrade/CriteriaDialog'", "0", "GetGradeMasterDetailFromCode('" + idPrefix + "CarGradeCode', new Array('" + idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "CarGradeName','" + idPrefix + "MakerName'), new Array('" + idPrefix + "Capacity','" + idPrefix + "MaximumLoadingWeight','" + idPrefix + "CarWeight','" + idPrefix + "TotalCarWeight','" + idPrefix + "Length','" + idPrefix + "Width','" + idPrefix + "Height','" + idPrefix + "FFAxileWeight','" + idPrefix + "FRAxileWeight','" + idPrefix + "RFAxileWeight','" + idPrefix + "RRAxileWeight','" + idPrefix + "ModelName','" + idPrefix + "EngineType','" + idPrefix + "Displacement','" + idPrefix + "Fuel','" + idPrefix + "ModelSpecificateNumber','" + idPrefix + "ClassificationTypeNumber'), 'OldCarGradeCode');" }); %>
            <%--<%=Html.TextBox(Model.Prefix + "CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", style = "width:50px", maxlength = 30, onchange = "GetMasterDetailFromCode('" + idPrefix + "CarGradeCode',new Array('" + idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "CarGradeName','" + idPrefix + "MakerName','" + idPrefix + "Capacity','" + idPrefix + "MaximumLoadingWeight','" + idPrefix + "CarWeight','" + idPrefix + "TotalCarWeight','" + idPrefix + "Length','" + idPrefix + "Width','" + idPrefix + "Height','" + idPrefix + "FFAxileWeight','" + idPrefix + "FRAxileWeight','" + idPrefix + "RFAxileWeight','" + idPrefix + "RRAxileWeight','" + idPrefix + "ModelName','" + idPrefix + "EngineType','" + idPrefix + "Displacement','" + idPrefix + "Fuel','" + idPrefix + "ModelSpecificateNumber','" + idPrefix + "ClassificationTypeNumber'),'CarGrade');" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "CarGradeCode", idPrefix + "CarGradeName", "'/CarGrade/CriteriaDialog'", "0", "GetMasterDetailFromCode('" + idPrefix + "CarGradeCode',new Array('" + idPrefix + "CarBrandName','" + idPrefix + "CarName','" + idPrefix + "CarGradeName','" + idPrefix + "MakerName','" + idPrefix + "Capacity','" + idPrefix + "MaximumLoadingWeight','" + idPrefix + "CarWeight','" + idPrefix + "TotalCarWeight','" + idPrefix + "Length','" + idPrefix + "Width','" + idPrefix + "Height','" + idPrefix + "FFAxileWeight','" + idPrefix + "FRAxileWeight','" + idPrefix + "RFAxileWeight','" + idPrefix + "RRAxileWeight','" + idPrefix + "ModelName','" + idPrefix + "EngineType','" + idPrefix + "Displacement','" + idPrefix + "Fuel','" + idPrefix + "ModelSpecificateNumber','" + idPrefix + "ClassificationTypeNumber'),'CarGrade')" }); %>--%>
            <% //Mod 2014/11/18 arc yano 車両ステータス追加対応 利用用途追加によるレイアウト調整。　車両グレード名のwidth 208px→178px%>
            <%=Html.TextBox(Model.Prefix + "CarBrandName", Model.CarBrandName, new { @class = "readonly", style = "width:80px", @readonly = "readonly" })%><%=Html.TextBox(Model.Prefix + "CarName", Model.CarName, new { @class = "readonly", style = "width:80px", @readonly = "readonly" })%><%=Html.TextBox(Model.Prefix + "CarGradeName", Model.CarGradeName, new { @class = "readonly", style = "width:178px", @readonly = "readonly" })%>
        </td>
        <th style="width: 100px">
            新中区分 <span class="hint">*</span>
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { style = "width:100%" })%>
        </td>
        <th style="width: 100px">
            販売価格
        </th>
        <td style="width: 100px">
            <%=Html.TextBox(Model.Prefix + "SalesPrice", Model.SalesPrice, new { @class = "numeric", style = "width:100px", maxlength = 10 })%>
        </td>
    </tr>
    <tr>
        <th>
            系統色
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "ColorType", (IEnumerable<SelectListItem>)ViewData["ColorTypeList"], new { style = "width:100%" })%>
        </td>
        <th>
            外装色
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "ExteriorColorCode", Model.ExteriorColorCode, new { @class = "alphanumeric", style = "width:50px", maxlength = 8, onblur = "GetCarColorMasterFromCode('"+ idPrefix + "CarGradeCode','" + idPrefix + "ExteriorColorCode','" + idPrefix + "ExteriorColorName','CarColor')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ExteriorColorCode", idPrefix + "ExteriorColorName", "'/CarColor/CriteriaDialog?ExteriorColorFlag=true&CarGradeCode='+document.getElementById('"+idPrefix+"CarGradeCode').value", "0" }); %>
            <%=Html.TextBox(Model.Prefix + "ExteriorColorName", Model.ExteriorColorName, new { style = "width:187px", maxlength = 50 }) %>
        </td>
        <th style="width: 100px">
            色替
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "ChangeColor", (IEnumerable<SelectListItem>)ViewData["ChangeColorList"], new { style = "width:100%" })%>
        </td>
        <th>
            内装色
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "InteriorColorCode", Model.InteriorColorCode, new { @class = "alphanumeric", style = "width:81px", maxlength = 8, onblur = "GetCarColorMasterFromCode('" + idPrefix + "CarGradeCode','" + idPrefix + "InteriorColorCode','" + idPrefix + "InteriorColorName','CarColor')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "InteriorColorCode", idPrefix + "InteriorColorName", "'/CarColor/CriteriaDialog?InteriorColorFlag=true&CarGradeCode='+document.getElementById('"+idPrefix+"CarGradeCode').value", "0" }); %>
            <!--
            <%=Html.TextBox(Model.Prefix + "InteriorColorName", Model.InteriorColorName, new { style = "width:187px", maxlength = 50 })%>
            -->
            <%=Html.TextBox(Model.Prefix + "InteriorColorName", Model.InteriorColorName, new { style = "width:157px", maxlength = 50 })%>

        </td>
    </tr>
    <tr>
        <th>
            在庫ステータス
        </th>
        <td>
            <% //Add 2014/08/15 arc amii 在庫ステータス変更対応 #3071 検索画面から入力画面へ項目を移動 %>
            <%if (Model.CarStatusEnabled) { %>
                <% string carStatus = Model.c_CarStatus != null ? Model.c_CarStatus.Code : ""; %>
                <%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["ChangeCarStatusList"], new { onchange="CompStatus('" + carStatus +"','CarStatus');"})%>
            <%}else{ %>
                <%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["ChangeCarStatusList"], new { @class = "readonly alphanumeric", @disabled = "disabled"})%>
            <%} %>
        </td>
        <% //Mod 2014/10/16 arc yano 車両ステータス追加対応 利用用途のリストボックスを在庫ステータスの隣に追加し、その後の項目をずらす%>
        <th>
            利用用途
        </th>
        <td>
            <%//Mod 2015/07/29 arc nakayama #3217_デモカーが販売できてしまう問題の改善 車両マスタステータス変更権限がなければリードオンリーにする %>
            <%if(Model.CarUsageEnabled){ %>
                <%=Html.DropDownList("CarUsage", (IEnumerable<SelectListItem>)ViewData["ChangeCarUsageList"], new { onchange = "SetInspectGuidFlag(this, document.getElementById('" + idPrefix + "InspectGuidFlag'));" })%><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
            <%}else{%>
                <%=Html.DropDownList("CarUsage", (IEnumerable<SelectListItem>)ViewData["ChangeCarUsageList"], new { @class = "readonly", @disabled = "disabled" })%>
            <%} %>
        </td>
        <th>
            ロケーション
        </th>
        <td style="width: 80px">
            <%=Html.TextBox(Model.Prefix + "LocationName", ViewData["LocationName"], new { style = "width:80px", @class = "readonly", @readonly = "readonly" }) %>
        </td>
        <th style="width: 100px">
            年式
        </th>
        <td style="width: 80px">
            <% // Mod 2021/08/11 #4097 最大桁数を4→10桁に戻す %>
            <% // Mod 2014/09/08 arc amii 年式入力対応 #3076 入力最大桁数を10→4に変更 %>
            <%=Html.TextBox(Model.Prefix+"ManufacturingYear", Model.ManufacturingYear, new { @class = "alphanumeric", style = "width:80px", maxlength = 10 }) %>
        </td>
        <th>
            ハンドル
        </th>
        <td style="width: 85px" colspan ="3">
            <%=Html.DropDownList(Model.Prefix+"Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"], new { style = "width:100%" })%>
        </td>
        <% //Add 2014/08/15 arc amii DMフラグ機能拡張対応 #3069 車検案内項目と備考欄を追加  %>
<!--
        <th>
            車検案内
        </th>
        <td style="width: 100px"><%//Mod  2014/10/16 arc yano 車両ステータス追加対応  レイアウト調整 85px → 100px%>
            <%=Html.DropDownList(Model.Prefix+"InspectGuidFlag", (IEnumerable<SelectListItem>)ViewData["InspectGuidFlagList"], new { style = "width:100%" })%>
        </td>
        <th>
            車検案内発送備考欄
        </th>
        <td  style="width: 100px">
            <%=Html.TextBox(Model.Prefix+"InspectGuidMemo", Model.InspectGuidMemo, new { style = "width:100px", maxlength = 100 })%>
        </td>
-->
        <%//Mod  2014/11/10 arc yano 車両ステータス追加対応  車検案内、車検案内発想備考欄を次の行へ移動%>
    </tr>
    <tr>
         <th>
            車検案内
        </th>
        <td style="width: 100px"><%//Mod  2014/10/16 arc yano 車両ステータス追加対応  レイアウト調整 85px → 100px%>
            <%=Html.DropDownList(Model.Prefix+"InspectGuidFlag", (IEnumerable<SelectListItem>)ViewData["InspectGuidFlagList"], new { style = "width:100%" })%>
        </td>
        <th>
            車検案内発送備考欄
        </th>
        <td colspan ="9">
            <%=Html.TextBox(Model.Prefix+"InspectGuidMemo", Model.InspectGuidMemo, new { style = "width:100px", maxlength = 100 })%>
        </td>
    </tr>
</table>
