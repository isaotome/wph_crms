<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>
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
            <%=Html.TextBox(Model.Prefix + "SalesCarNumber", Model.SalesCarNumber, new { @class = "readonly", style = "width:100px", @readonly = "readonly" }) %>
        </td>
        <th style="width: 100px">
            グレード
            <span class="hint">*</span>
        </th>
        <td colspan="5">
            <%=Html.TextBox(Model.Prefix + "CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric readonly", style = "width:50px", maxlength = 30, @readonly = "readonly" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "CarGradeCode", Model.Prefix + "CarGradeName", "'/CarGrade/CriteriaDialog'", "1" }); %>
            <% //Mod 2014/11/18 arc yano 車両ステータス追加対応 利用用途追加によるレイアウト調整。　車両グレード名のwidth 208px→178px%>
            <%=Html.TextBox(Model.Prefix + "CarBrandName", Model.CarBrandName, new { @class = "readonly", style = "width:80px", @readonly = "readonly" })%><%=Html.TextBox(Model.Prefix + "CarName", Model.CarName, new { @class = "readonly", style = "width:80px", @readonly = "readonly" })%><%=Html.TextBox(Model.Prefix + "CarGradeName", Model.CarGradeName, new { @class = "readonly", style = "width:178px", @readonly = "readonly" })%>
        </td>
        <th style="width: 100px">
            新中区分
            <span class="hint">*</span>
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "NewUsedType", Model.NewUsedType) %>
        </td>
        <th style="width: 100px">
            販売価格
        </th>
        <td style="width: 100px">
            <%=Html.TextBox(Model.Prefix + "SalesPrice", Model.SalesPrice, new { @class = "numeric readonly", style = "width:100px", maxlength = 10, @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            系統色
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix + "ColorType", (IEnumerable<SelectListItem>)ViewData["ColorTypeList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "ColorType", Model.ColorType) %>
        </td>
        <th>
            外装色
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "ExteriorColorCode", Model.ExteriorColorCode, new { @class = "alphanumeric readonly", style = "width:50px", maxlength = 8, @readonly = "readonly" })%>
<% // <!--//2014/05/09 vs2012対応 arc yano javaScriptエラー対応 …CarGradeCode.valueの後ろの'を削除する%>
<% //            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "ExteriorColorCode", Model.Prefix + "ExteriorColorName", "'/CarColor/CriteriaDialog?CarGradeCode='+document.forms[0].CarGradeCode.value'", "1" }); %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "ExteriorColorCode", Model.Prefix + "ExteriorColorName", "'/CarColor/CriteriaDialog?CarGradeCode='+document.forms[0].CarGradeCode.value", "1" }); %>
            <%=Html.TextBox(Model.Prefix + "ExteriorColorName", Model.ExteriorColorName, new { style = "width:187px", maxlength = 50,@class = "readonly", @readonly = "readonly" }) %>
        </td>
        <th style="width: 100px">
            色替
        </th>
        <td style="width: 85px">
            <%=Html.DropDownList(Model.Prefix + "ChangeColor", (IEnumerable<SelectListItem>)ViewData["ChangeColorList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "ChangeColor",Model.ChangeColor) %>
        </td>
        <th>
            内装色
        </th>
        <td colspan="3">
            <%=Html.TextBox(Model.Prefix + "InteriorColorCode", Model.InteriorColorCode, new { @class = "alphanumeric readonly", style = "width:81px", maxlength = 8, @readonly = "readonly" }) %>
<% // <!--//2014/05/09 vs2012対応 arc yano javaScriptエラー対応 …CarGradeCode.valueの後ろの'を削除する%>
<% //            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "InteriorColorCode", Model.Prefix + "InteriorColorName", "'/CarColor/CriteriaDialog?CarGradeCode='+document.forms[0].CarGradeCode.value'", "1" }); %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { Model.Prefix + "InteriorColorCode", Model.Prefix + "InteriorColorName", "'/CarColor/CriteriaDialog?CarGradeCode='+document.forms[0].CarGradeCode.value", "1" }); %>
            <%=Html.TextBox(Model.Prefix + "InteriorColorName", Model.InteriorColorName, new { style = "width:187px", maxlength = 50, @class = "readonly", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            在庫ステータス
        </th>
        <td style="width: 105px">
           &nbsp;<%=Html.Encode(Model.c_CarStatus != null ? Model.c_CarStatus.Name: "")%>
           <%=Html.Hidden(Model.Prefix + "CarStatus", Model.CarStatus)%>
        </td>        
         <% //Mod 2014/10/16 arc yano 車両ステータス追加対応 利用用途のリストボックスを在庫ステータスの隣に追加し、その後の項目をずらす%>
        <th>
            利用用途
        </th>
        <td style="width: 105px">
            <% //Add 2014/10/30 arc amii 車両ステータス追加対応
               CrmsLinqDataContext db = new CrmsLinqDataContext();
               CodeDao dao = new CodeDao(db);
               // c_CodeNameの指定した区分のデータを取得
               c_CodeName codeName = dao.GetCodeNameByKey("004", CommonUtils.DefaultString(Model.CarUsage), false);  
            if(codeName != null){
            %>
            <%=Html.Encode(codeName.Name) %>
            <%}else{ %>
            &nbsp;<%=Html.Encode("") %>
            <%} %>
             <%=Html.Hidden(Model.Prefix + "CarUsage", Model.CarUsage)%>
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
            <%=Html.TextBox(Model.Prefix+"ManufacturingYear", Model.ManufacturingYear, new { @class = "alphanumeric readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" }) %>
        </td>
        <th>
            ハンドル
        </th>
        <td colspan ="3">
            <%=Html.DropDownList(Model.Prefix+"Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"], new { style = "width:70px", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "Steering", Model.Steering) %>
        </td>
         <% //Add 2014/08/15 arc amii DMフラグ機能拡張対応 #3069 車検案内項目と備考欄を追加  %>
    </tr>
    <%//Mod  2014/11/10 arc yano 車両ステータス追加対応  車検案内、車検案内発想備考欄を次の行へ移動%>
    <tr>
        <th>
            車検案内
        </th>
        <td>
            <%=Html.DropDownList(Model.Prefix+"InspectGuidFlag", (IEnumerable<SelectListItem>)ViewData["InspectGuidFlagList"], new { style = "width:100%", @disabled = "disabled" })%>
            <%=Html.Hidden(Model.Prefix + "InspectGuidFlag", Model.InspectGuidFlag) %>
        </td>
        <th>
            車検案内発送備考欄
        </th>
        <td colspan ="9">
            <% //2014/11/11 arc yano 車両ステータス変更対応 車両情報を表示するのみの場合は、テキストボックスは入力できないようにする。 %>
            <%=Html.TextBox(Model.Prefix+"InspectGuidMemo", Model.InspectGuidMemo, new { style = "width:100px", @class = "readonly", @readonly = "readonly"  })%>
        </td>
    </tr>
</table>
