<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarAppraisal>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両査定入力</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "CarAppraisal", FormMethod.Post))
  { %>
    <%
      /*-----------------------------------------------------------------------------------
       *  Mod 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応） 
       *  Mod #4072 原動機型式入力エリアの拡張
       *  Mod 2020/11/18 yano #4071 グレードコード変更時にダイアログ表示
       *  Mod 2014/06/20 arc yano 税率変更バグ修正 
       * 「保存」ボタン押下時は、税率変更[setTaxIdByDate()]が処理中かどうかをチェックし、
       *  処理中の場合は処理完了までsubmitしない。
       *
       *  注意：この対応は以下の操作
       *  　　　操作：
       *  　　　　(1)[仕入予定日]の日付を手入力で変更。
       *  　　　　(2)フォーカスを移動しないで、「保存」ボタンを押下
       *        を行った場合に、  
       *
       *        　① 仕入れ予定日のonchangeによる処理(setTaxIdByDate())
       *          ② 保存ボタンのonclickによる処理(formsubmit())
       *　　　　が同時に動作しないようにするためのものであるが、
       *　　　　同じ操作を行っても、①のみ動作する場合がある。
       *　　　　その場合は、再度「保存」ボタンをクリックする必要がある。
       ------------------------------------------------------------------------------------*/  
     %>
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <%if (!CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002")) { %>
            <% // Edit 2014/06/20 arc yano 税率変更バグ対応 %>
            <td onclick="document.forms[0].action.value = 'save';chkSubmit('CarAppraisal');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <%if (!CommonUtils.DefaultString(ViewData["PurchaseCreated"]).Equals("1")) { %>
                <td onclick="document.forms[0].action.value = 'savePurchase';formSubmit();"><img src="/Content/Images/build.png" alt="仕入予定作成" class="command_btn" />&nbsp;仕入予定作成</td>
            <%} %>
        <%}%>
        <td onclick="document.forms[0].PrintReport.value='CarAppraisal';document.forms[0].reportParam.value='<%= Model.CarAppraisalId %>';formSubmit();"><img src="/Content/Images/pdf.png" alt="査定票" class="command_btn" />&nbsp;査定票</td>
        <%if(CommonUtils.DefaultString(ViewData["PurchaseCreated"]).Equals("1")){ %>
            <td onclick="document.forms[0].reportName.value='CarArrival';document.forms[0].reportParam.value='<%= Model.CarPurchase!=null && Model.CarPurchase.Count()>0 ? Model.CarPurchase[0].CarPurchaseId.ToString() : ""%>';printReport();"><img src="/Content/Images/pdf.png" alt="入庫連絡票" class="command_btn" />&nbsp;入庫連絡票</td>
            <td onclick="document.forms[0].PrintReport.value='CarPurchaseAgreement';document.getElementById('reportParam').value='<%= Model.CarAppraisalId%>';formSubmit();"><img src="/Content/Images/pdf.png" alt="車輌買取契約書" class="command_btn" />&nbsp;車輌買取契約書</td>
        <%} %>
    </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("CarAppraisalId", Model.CarAppraisalId)%>
<%=Html.Hidden("SlipNumber", Model.SlipNumber)%>
<%=Html.Hidden("reportName", ViewData["reportName"])%>
<%=Html.Hidden("reportParam", ViewData["reportParam"])%>
<%=Html.Hidden("PrintReport", "")%>
<%=Html.Hidden("LastEditScreen", Model.LastEditScreen)%>
<%=Html.Hidden("LastEditMessage", Model.LastEditMessage)%>
<%=Html.Hidden("OldCarGradeCode", Model.CarGradeCode)%><%//Add 2020/11/18 yano #4071  %>

<div id="input-form">
<%=Html.ValidationSummary()%>
    <%if (ViewData["ErrorSalesCar"] != null) { %>
        <%Html.RenderPartial("VinErrorControl", ViewData["ErrorSalesCar"]); %>
        <%=Html.CheckBox("RegetVin", Model.RegetVin)%>この車台番号で管理番号を再取得する
        <br />
        <br />
    <%} %>
    <%//Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう %>
    <%//Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 %>
<table border="0">
<tr>
    <td>
        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">受注情報</th>
            </tr>
            <tr>
                <th style="width: 100px">
                    伝票番号
                </th>
                <td style="width: 80px">
                    <%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric readonly", style = "width:80px" }) %>
                </td>
                <th style="width: 100px">
                    受注日
                </th>
                <td style="width: 80px">
                    <%=Html.TextBox("SalesOrderDate", ViewData["SalesOrderDate"], new { @class = "alphanumeric readonly", style = "width:80px" })%>
                </td>
            </tr>
            <tr>
                <th>
                    担当部門
                </th>
                <td colspan="3">
                    <%=Html.TextBox("OrderDepartmentName", ViewData["OrderDepartmentName"], new { @class = "readonly", style = "width:274px" })%>
                </td>
            </tr>
            <tr>
                <th>
                    担当者
                </th>
                <td colspan="3">
                    <%=Html.TextBox("OrderEmployeeName", ViewData["OrderEmployeeName"], new { @class = "readonly", style = "width:274px" })%>
                </td>
            </tr>
        </table>
    </td>
    <td style="width:15px"></td>
    <td style="vertical-align:top">
        <table class="input-form-slim">
            <tr>
                <th colspan="4" class="input-form-title">伝票情報</th>
            </tr>
            <tr>
                <th style="width: 100px;white-space:nowrap">
                    査定日
                </th>
                <td style="width: 80px">
                    <%=Html.TextBox("AppraisalDate", string.Format("{0:yyyy/MM/dd}", Model.AppraisalDate), new { @class = "alphanumeric", maxlength = 11, style = "width:80px" }) %>
                </td>
                <th style="width: 100px;white-space:nowrap">
                    買取契約日
                </th>
                <td style="width: 80px">
                    <%=Html.TextBox("PurchaseAgreementDate", string.Format("{0:yyyy/MM/dd}", Model.PurchaseAgreementDate), new { @class = "alphanumeric", maxlength = 11, style = "width:80px" })%>
                </td>
             </tr>
             <tr>
    <!-- ADD 消費税率 2014/02/20 ookubo -->
                <%=Html.Hidden("Rate",Model.Rate) %>
                <%=Html.Hidden("ConsumptionTaxId", ViewData["ConsumptionTaxId"])%>
                <%=Html.Hidden("ConsumptionTaxIdOld", ViewData["ConsumptionTaxIdOld"])%>
   <% //<!-- Add 2014/06/20 arc yano 税率変更バグ対応 --> %>
                <%=Html.Hidden("DateOld1", ViewData["PurchasePlanDateOld"])%>
                <%=Html.Hidden("DateOld2", ViewData["PurchasePlanDateOld"])%>
                <%=Html.Hidden("canSave", 1)%>
                <th style="width: 100px; white-space:nowrap">
                    仕入予定日 *
                </th>
                <td style="width: 80px">
    <!-- MOD 消費税対応 2014/02/20 ookubo -->
                    <%=Html.TextBox("PurchasePlanDate", string.Format("{0:yyyy/MM/dd}", Model.PurchasePlanDate), new { @class = "alphanumeric", maxlength = 11, style = "width:80px", onchange = "setTaxIdByDate(this, this,'CarAppraisal');" })%>
                </td>
    <!-- ADD 消費税率 2014/02/20 ookubo -->
                <th style="width: 100px;white-space:nowrap">
                    消費税率
                </th>
                <td style="width: 80px">
                    <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "readonly alphanumeric", @disabled = "disabled", style = "width:87px", Value = Model.ConsumptionTaxId })%>
                </td>
            </tr>
                <tr>
                    <th class="style1">
                        仕入ステータス
                    </th>
                    <td class="style2">
                        <%=Html.TextBox("PurchaseStatusName", CommonUtils.DefaultString(ViewData["PurchaseStatusName"], "未仕入"), new { @class = "readonly", style = "width:80px" })%>
                    </td>
                    <th class="style1">
                        仕入日
                    </th>
                    <td class="style2">
                        <%=Html.TextBox("PurchaseDate", ViewData["PurchaseDate"], new { @class = "readonly", style = "width:80px" })%>
                    </td>
                </tr>
            </table>
    </td>
    <td style="width:15px"></td>
    <td style="vertical-align:top">
       <table class="input-form-slim">
            <tr>
                <th colspan="2" class="input-form-title">車両情報</th>
            </tr>
            <tr>
                <th style="width: 100px">
                    グレード
                </th>
                <td style="width: 180px">
                    <%//Mod 2020/11/18 yano #4071 %>
                    <%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", maxlength = 30, style="width:150px", onchange = "GetGradeMasterDetailFromCode('CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','MakerName'), new Array('Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelName','EngineType','Displacement','Fuel','ModelSpecificateNumber','ClassificationTypeNumber','Door','TransMission'), 'OldCarGradeCode');" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog'", "0", "GetGradeMasterDetailFromCode('CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','MakerName'), new Array('Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelName','EngineType','Displacement','Fuel','ModelSpecificateNumber','ClassificationTypeNumber','Door','TransMission'), 'OldCarGradeCode')" }); %>
                    <%--<%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", maxlength = 30, style="width:150px", onchange = "GetMasterDetailFromCode('CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','MakerName','Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelName','EngineType','Displacement','Fuel','ModelSpecificateNumber','ClassificationTypeNumber','Door','TransMission'),'CarGrade');" })%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CarGradeCode", "CarGradeName", "'/CarGrade/CriteriaDialog'", "0","GetMasterDetailFromCode('CarGradeCode',new Array('CarBrandName','CarName','CarGradeName','MakerName','Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelName','EngineType','Displacement','Fuel','ModelSpecificateNumber','ClassificationTypeNumber','Door','TransMission'),'CarGrade')" }); %>--%>
                </td>
            </tr>
        </table>
    </td>
</tr>
</table>
<br />
<table class="input-form-slim">
    <tr>
        <th colspan="12" class="input-form-title">車検証情報</th>
    </tr>
    <tr>
        <th style="width:100px">
            陸運局
        </th>
        <td style="width:80px">
            <%=Html.TextBox("MorterViecleOfficialCode", Model.MorterViecleOfficialCode, new { style="width:80px", maxlength = 5 })%>
        </td>
        <th style="width:100px">
            登録番号(種別)
        </th>
        <td style="width:80px">
            <%=Html.TextBox("RegistrationNumberType", Model.RegistrationNumberType, new { @class = "alphanumeric", style="width:80px", maxlength = 3 })%>
        </td>
        <th style="width:100px">
            登録番号(かな)
        </th>
        <td style="width:80px">
            <%=Html.TextBox("RegistrationNumberKana", Model.RegistrationNumberKana, new { style = "width:80px", maxlength = 1 })%>
        </td>
        <th style="width:100px;white-space:nowrap">
            登録番号(ﾌﾟﾚｰﾄ)
        </th>
        <td style="width:80px">
            <%=Html.TextBox("RegistrationNumberPlate", Model.RegistrationNumberPlate, new { @class = "alphanumeric", style = "width:80px", maxlength = 4 })%>
        </td>
        <th style="width:100px">
            登録日
        </th>
        <td style="width:80px">
            <%=Html.TextBox("RegistrationDate", string.Format("{0:yyyy/MM/dd}", Model.RegistrationDate), new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
        <th style="width:100px">
            初年度登録
        </th>
        <td style="width:80px">
            <%=Html.TextBox("FirstRegistrationYear", Model.FirstRegistrationYear, new { style = "width:80px", maxlength = 9 })%>
        </td>
    </tr>
    <tr>
        <th>
            自動車種別
        </th>
        <td>
            <%=Html.DropDownList("CarClassification", (IEnumerable<SelectListItem>)ViewData["CarClassificationList"], new { style = "width:100%" })%>
        </td>
        <th>
            用途
        </th>
        <td>
            <%=Html.DropDownList("Usage", (IEnumerable<SelectListItem>)ViewData["UsageList"], new { style = "width:100%" })%>
        </td>
        <th>
            事自区分
        </th>
        <td>
            <%=Html.DropDownList("UsageType", (IEnumerable<SelectListItem>)ViewData["UsageTypeList"], new { style = "width:100%" })%>
        </td>
        <th>
            形状
        </th>
        <td colspan="5">
            <%=Html.DropDownList("Figure", (IEnumerable<SelectListItem>)ViewData["FigureList"], new { style = "width:80px" })%>
        </td>
    </tr>
    <tr>
        <th>
            車名
        </th>
        <td colspan="3">
            <%=Html.TextBox("MakerName", Model.MakerName, new {style="width:274px", maxlength = 50 })%>
        </td>
        <th>
            定員
        </th>
        <td>
            <%=Html.TextBox("Capacity", Model.Capacity, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            最大積載量
        </th>
        <td>
            <%=Html.TextBox("MaximumLoadingWeight", Model.MaximumLoadingWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            車両重量
        </th>
        <td>
            <%=Html.TextBox("CarWeight", Model.CarWeight, new { @class = "numeric", style="width:80px", maxlength = 9 })%>
        </td>
        <th>
            車両総重量
        </th>
        <td>
            <%=Html.TextBox("TotalCarWeight", Model.TotalCarWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
    </tr>
    <tr>
        <th>
            車台番号 *
        </th>
        <td colspan="3">
            <%//Mod 2016/08/23 arc yano #3621 車両査定入力　車台番号のマスタチェックの廃止 GetSalesCarInfoに引数を追加%>
            <!-- MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面　車台番号から自動入力 -->
            <!-- %=Html.TextBox("Vin", Model.Vin, new { style="width:274px",maxlength = 20 })% -->
            <%=Html.TextBox("Vin", Model.Vin, new { style="width:274px",maxlength = 20, onchange = "GetSalesCarInfo('Vin','SalesCarNumber',new Array('CarGradeCode','MakerName','CarBrandName','CarGradeName','CarName','ExteriorColorName','InteriorColorName','Mileage','MileageUnit','ModelName','SalesCarNumber','Vin','RecycleDeposit','EngineType','FirstRegistrationYear','CarClassification','Usage','UsageType','Figure','Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelSpecificateNumber','ClassificationTypeNumber','Displacement','Fuel','OwnerCode','PossesorName','PossesorAddress','UserCode','UserName','UserAddress','PrincipalPlace','Memo','DocumentRemarks','DocumentComplete','IssueDate','Guarantee','Instructions','Steering','Import','Light','Aw','Aero','Sr','Cd','Md','NaviType','NaviEquipment','NaviDashboard','SeatColor','SeatType','Recycle','RecycleTicket', 'ModelYear','Door','TransMission','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerName','RegistrationDate','ChangeColor'), null, null, null, 'CarAppraisal', false );" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "SalesCarNumber", "Vin", "'/SalesCar/CriteriaDialog'", "0", "GetMasterDetailFromCode('SalesCarNumber',new Array('CarGradeCode','MakerName','CarBrandName','CarGradeName','CarName','ExteriorColorName','InteriorColorName','Mileage','MileageUnit','ModelName','SalesCarNumber','Vin','RecycleDeposit','EngineType','FirstRegistrationYear','CarClassification','Usage','UsageType','Figure','Capacity','MaximumLoadingWeight','CarWeight','TotalCarWeight','Length','Width','Height','FFAxileWeight','FRAxileWeight','RFAxileWeight','RRAxileWeight','ModelSpecificateNumber','ClassificationTypeNumber','Displacement','Fuel','OwnerCode','PossesorName','PossesorAddress','UserCode','UserName','UserAddress','PrincipalPlace','Memo','DocumentRemarks','DocumentComplete','IssueDate','Guarantee','Instructions','Steering','Import','Light','Aw','Aero','Sr','Cd','Md','NaviType','NaviEquipment','NaviDashboard','SeatColor','SeatType','Recycle','RecycleTicket', 'ModelYear','Door','TransMission','InspectionExpireDate','UsVin','MorterViecleOfficialCode','RegistrationNumberType','RegistrationNumberKana','RegistrationNumberPlate','CustomerName','RegistrationDate','ChangeColor'),'CarAppraisal');" }); %>
            <!-- 非表示の管理番号 -->
            <%=Html.TextBox("SalesCarNumber", Model.Vin, new { style="width:0px;visibility:hidden", maxlength = 20 })%>
        </td>
        <td colspan="2">
        </td>
        <th>
            長さ
        </th>
        <td>
            <%=Html.TextBox("Length", Model.Length, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            幅
        </th>
        <td>
            <%=Html.TextBox("Width", Model.Width, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            高さ
        </th>
        <td>
            <%=Html.TextBox("Height", Model.Height, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
    </tr>
    <tr>
        <th>
            前前軸重
        </th>
        <td>
            <%=Html.TextBox("FFAxileWeight", Model.FFAxileWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            前後軸重
        </th>
        <td>
            <%=Html.TextBox("FRAxileWeight", Model.FRAxileWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            後前軸重
        </th>
        <td>
            <%=Html.TextBox("RFAxileWeight", Model.RFAxileWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            後後軸重
        </th>
        <td>
            <%=Html.TextBox("RRAxileWeight", Model.RRAxileWeight, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            型式指定番号
        </th>
        <td>
            <%=Html.TextBox("ModelSpecificateNumber", Model.ModelSpecificateNumber, new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            類別区分番号
        </th>
        <td>
            <%=Html.TextBox("ClassificationTypeNumber", Model.ClassificationTypeNumber, new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
    </tr>
    <tr>
        <th>
            型式
        </th>
        <td>
            <%=Html.TextBox("ModelName", Model.ModelName, new { style = "width:80px", maxlength = 20 })%>
        </td>
        <th>
            原動機型式
        </th>
        <td>
            <%=Html.TextBox("EngineType", Model.EngineType, new { @class = "alphanumeric", style = "width:80px", maxlength = 25 })%><%//Mod 2020/11/27 yano #4072 15 -> 25%><%//Mod  2019/5/23 yano #3992%>
        </td>
        <th>
            排気量
        </th>
        <td>
            <%=Html.TextBox("Displacement", Model.Displacement, new { @class = "numeric", style = "width:80px", maxlength = 9 })%>
        </td>
        <th>
            燃料種類
        </th>
        <td colspan="5">
            <%=Html.DropDownList("Fuel", (IEnumerable<SelectListItem>)ViewData["FuelList"])%>
        </td>
    </tr>
    <tr>
        <th>
            所有者
        </th>
        <td colspan="5">
            <%=Html.TextBox("OwnerCode",Model.OwnerCode, new { @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "GetMasterDetailFromCode('OwnerCode',new Array('PossesorName','PossesorAddress'),'Customer')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "OwnerCode", "PossesorName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('OwnerCode',new Array('PossesorName','PossesorAddress'),'Customer')" }); %>
            <%=Html.TextBox("PossesorName", Model.PossesorName, new { style="width:357px", maxlength = 40 })%>
        </td>
        <th>
            所有者住所
        </th>
        <td colspan="5">
            <%=Html.TextBox("PossesorAddress", Model.PossesorAddress, new { style="width:468px", maxlength = 300 })%>
        </td>
    </tr>
    <tr>
        <th>
            使用者
        </th>
        <td colspan="5">
            <%=Html.TextBox("UserCode", Model.UserCode, new { @class = "alphanumeric", style = "width:80px", maxlength = 10, onchange = "GetMasterDetailFromCode('UserCode',new Array('UserName','UserAddress'),'Customer')" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "UserCode", "UserName", "'/Customer/CriteriaDialog'", "0", "GetMasterDetailFromCode('UserCode',new Array('UserName','UserAddress'),'Customer')" }); %>
            <%=Html.TextBox("UserName", Model.UserName, new { style="width:357px", maxlength = 40 })%>
        </td>
        <th>
            使用者住所
        </th>
        <td colspan="5">
            <%=Html.TextBox("UserAddress", Model.UserAddress, new { style="width:468px", maxlength = 300 })%>
        </td>
    </tr>
    <tr>
        <th>
            本拠地
        </th>
        <td colspan="5">
            <%=Html.TextBox("PrincipalPlace", Model.PrincipalPlace, new { style="width:468px", maxlength = 300 })%>
        </td>
        <th>
            有効期限
        </th>
        <td>
            <%=Html.TextBox("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", Model.InspectionExpireDate), new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
        <th>
            走行距離
        </th>
        <td colspan="3">
            <%=Html.TextBox("Mileage", Model.Mileage, new { @class = "numeric", style = "width:80px", maxlength = 13 })%>
            <%=Html.DropDownList("MileageUnit", (IEnumerable<SelectListItem>)ViewData["MileageUnitList"], new { style = "width:50px;height:20px" })%>
        </td>
    </tr>
    <tr>
        <th rowspan="2">
            備考
        </th>
        <td rowspan="2" colspan="5">
            <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
            <%=Html.TextArea("Memo", Model.Memo, 3, 56, new { style = "width:472px;height:40px", wrap = "virtual", onblur = "checkTextLength('Memo', 255, '備考')" })%>
        </td>
        <th rowspan="2">
            書類備考
        </th>
        <td rowspan="2" colspan="3">
            <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
            <%=Html.TextArea("DocumentRemarks", Model.DocumentRemarks, 3, 32, new { style="width:278px;height:40px", wrap = "virtual", onblur = "checkTextLength('DocumentRemarks', 100, '書類備考')" })%>
        </td>
        <th>
            書類
        </th>
        <td>
            <%=Html.DropDownList("DocumentComplete", (IEnumerable<SelectListItem>)ViewData["DocumentCompleteList"], new { style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            発行日
        </th>
        <td>
            <%=Html.TextBox("IssueDate", string.Format("{0:yyyy/MM/dd}", Model.IssueDate), new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
    </tr>
</table>
<br />
<table class="input-form-slim">
    <tr>
        <th class="input-form-title" colspan="12">査定情報</th>
    </tr>
    <tr>
        <th style="width:100px">
            モデル年
        </th>
        <td style="width:80px">
            <% //Mod 2021/08/02 yano #4097%　入力最大桁数を4→10に変更>%>
            <% // Mod 2014/09/08 arc amii 年式入力対応 #3076 入力最大桁数を20→4に変更 %>
            <%=Html.TextBox("ModelYear", Model.ModelYear, new { @class = "alphanumeric", style = "width:80px", maxlength = 10 })%>
        </td>
        <th style="width:100px">
            顧客名
        </th>
        <td style="width:80px">
            <%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { @class = "readonly", @readonly = "readonly", style = "width:80px" }) %>
        </td>
        <th style="width:100px">
            部門 *
        </th>
        <td colspan="3">
            <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", style="width:80px", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" }); %>
            <%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class="readonly", style = "width:163px",@readonly = "readonly" }) %>
        </td>
        <th style="width:100px">
            担当者 *
        </th>
        <td colspan="3">
                <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
                <%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
    </tr>
    <tr>
        <th>
            ブランド名
        </th>
        <td>
            <%=Html.TextBox("CarBrandName", Model.CarBrandName, new { style="width:80px", maxlength = 50 })%>
        </td>
        <th>
            車種名
        </th>
        <td>
            <%=Html.TextBox("CarName", Model.CarName, new { style = "width:80px", maxlength = 20 })%>
        </td>
        <th>
            グレード名
        </th>
        <td>
            <%=Html.TextBox("CarGradeName", Model.CarGradeName, new { style = "width:80px", maxlength = 50 })%>
        </td>
        <th style="width:100px">
            ドア
        </th>
        <td>
            <%=Html.TextBox("Door", Model.Door, new { @class = "alphanumeric", style = "width:80px", maxlength = 3 })%>
        </td>
        <th style="width:100px">
            MT
        </th>
        <td>
            <%=Html.DropDownList("TransMission", (IEnumerable<SelectListItem>)ViewData["TransMissionList"], new { style = "width:100%" })%>
        </td>
        <td colspan="2">
        </td>
    </tr>
    <tr>
        <th>
            外装色名
        </th>
        <td colspan="3">
            <%=Html.TextBox("ExteriorColorName", Model.ExteriorColorName, new { style="width:274px", maxlength = 50 })%>
        </td>
        <th>
            色替
        </th>
        <td>
            <%=Html.DropDownList("ChangeColor", (IEnumerable<SelectListItem>)ViewData["ChangeColorList"], new { style = "width:100%" })%>
        </td>
        <th>
            元色名
        </th>
        <td>
            <%=Html.TextBox("OriginalColorName", Model.OriginalColorName, new { style="width:80px", maxlength = 50 })%>
        </td>
        <th style="width:100px">
            内装色名
        </th>
        <td colspan="3">
            <%=Html.TextBox("InteriorColorName", Model.InteriorColorName, new { style = "width:274px", maxlength = 50 })%>
        </td>
    </tr>
    <tr>
        <th>
            保証書
        </th>
        <td>
            <%=Html.DropDownList("Guarantee", (IEnumerable<SelectListItem>)ViewData["GuaranteeList"], new { style = "width:100%" })%>
        </td>
        <th>
            取説
        </th>
        <td>
            <%=Html.DropDownList("Instructions", (IEnumerable<SelectListItem>)ViewData["InstructionsList"], new { style = "width:100%" })%>
        </td>
        <th>
            ハンドル
        </th>
        <td>
            <%=Html.DropDownList("Steering", (IEnumerable<SelectListItem>)ViewData["SteeringList"], new { style = "width:100%" })%>
        </td>
        <th>
            輸入
        </th>
        <td>
            <%=Html.DropDownList("Import", (IEnumerable<SelectListItem>)ViewData["ImportList"], new { style = "width:100%" })%>
        </td>
        <td colspan="4">
        </td>
    </tr>
    <tr>
        <th>
            ライト
        </th>
        <td>
            <%=Html.DropDownList("Light", (IEnumerable<SelectListItem>)ViewData["LightList"], new { style = "width:100%" })%>
        </td>
        <th>
            AW
        </th>
        <td>
            <%=Html.DropDownList("Aw", (IEnumerable<SelectListItem>)ViewData["AwList"], new { style = "width:100%" })%>
        </td>
        <th>
            エアロ
        </th>
        <td>
            <%=Html.DropDownList("Aero", (IEnumerable<SelectListItem>)ViewData["AeroList"], new { style = "width:100%" })%>
        </td>
        <th>
            SR
        </th>
        <td>
            <%=Html.DropDownList("Sr", (IEnumerable<SelectListItem>)ViewData["SrList"], new { style = "width:100%" })%>
        </td>
        <th>
            CD
        </th>
        <td>
            <%=Html.DropDownList("Cd", (IEnumerable<SelectListItem>)ViewData["CdList"], new { style = "width:100%" })%>
        </td>
        <th style="width:100px">
            MD
        </th>
        <td>
            <%=Html.DropDownList("Md", (IEnumerable<SelectListItem>)ViewData["MdList"], new { style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            ナビ
        </th>
        <td colspan="7">
            &nbsp;製造：<%=Html.DropDownList("NaviType", (IEnumerable<SelectListItem>)ViewData["NaviTypeList"])%>
            &nbsp;媒体：<%=Html.DropDownList("NaviEquipment", (IEnumerable<SelectListItem>)ViewData["NaviEquipmentList"])%>
            &nbsp;位置：<%=Html.DropDownList("NaviDashboard", (IEnumerable<SelectListItem>)ViewData["NaviDashboardList"])%>
        </td>
        <th>
            シート(色)
        </th>
        <td>
            <%=Html.TextBox("SeatColor", Model.SeatColor, new { style="width:80px", maxlength = 10 })%>
        </td>
        <th>
            シート
        </th>
        <td>
            <%=Html.DropDownList("SeatType", (IEnumerable<SelectListItem>)ViewData["SeatTypeList"], new { style = "width:100%" })%>
        </td>
    </tr>
    <tr>
        <th>
            リサイクル
        </th>
        <td>
            <%=Html.DropDownList("Recycle", (IEnumerable<SelectListItem>)ViewData["RecycleList"], new { style = "width:100%" })%>
        </td>
        <th>
            リサイクル券
        </th>
        <td>
            <%=Html.DropDownList("RecycleTicket", (IEnumerable<SelectListItem>)ViewData["RecycleTicketList"], new { style = "width:100%" })%>
        </td>
        <th>
            リサイクル券番号
        </th>
        <td>
            <%=Html.TextBox("RecycleNumber", Model.RecycleNumber, new { @class = "alphanumeric", style="width:80px", maxlength = 12 })%>
        </td>
        <th>
            リサイクル預託金
        </th>
        <%if (ViewData["PurchasedFlag"] != null && ViewData["PurchasedFlag"].Equals("1")) {%>
            <td><%=Html.TextBox("RecycleDeposit", Model.RecycleDeposit, new { @class = "numeric readonly", style="width:80px", maxlength = 10 , @readonly = "readonly"})%></td>
        <%}else{ %>
            <td><%=Html.TextBox("RecycleDeposit", Model.RecycleDeposit, new { @class = "numeric", style="width:80px", maxlength = 10 })%></td>
        <%} %>
        <th>
            残債
        </th>
        <%if (ViewData["PurchasedFlag"] != null && ViewData["PurchasedFlag"].Equals("1")) {%>
            <td><%=Html.TextBox("RemainDebt", Model.RemainDebt, new { @class = "numeric readonly", style="width:80px", maxlength = 10, @readonly = " readonly"})%></td>
        <%}else{ %>
            <td><%=Html.TextBox("RemainDebt", Model.RemainDebt, new { @class = "numeric", style="width:80px", maxlength = 10 })%></td>
        <%} %>
        <th>
            未払自動車税種別割     <%--Mod 2019/09/04 yano #4011--%>
        </th>
        <%if (ViewData["PurchasedFlag"] != null && ViewData["PurchasedFlag"].Equals("1")) {%>
            <td><%=Html.TextBox("CarTaxUnexpiredAmount", Model.CarTaxUnexpiredAmount, new { @class = "numeric readonly", style="width:80px", maxlength = 10, @readonly = "readonly"})%></td>
        <%}else{ %>
            <td><%=Html.TextBox("CarTaxUnexpiredAmount", Model.CarTaxUnexpiredAmount, new { @class = "numeric", style="width:80px", maxlength = 10 })%></td>
        <%} %>
    </tr>
    <tr>
        <th rowspan="2">
            セールスポイント<br />
            /特記事項
        </th>
        <td rowspan="2" colspan="7">
            <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
            <%=Html.TextArea("Remarks", Model.Remarks, 3, 80, new { style="height:40px", wrap = "virtual", onblur = "checkTextLength('Remarks', 500, 'セールスポイント/特記事項')" })%>
        </td>
        <th>
            残債支払先
        </th>
        <td colspan="3">
            <%=Html.TextBox("RemainDebtPayee", Model.RemainDebtPayee, new { maxlength = 40 })%>
        </td>
    </tr>
    <tr>
        <th>
            抹消登録
        </th>
        <td>
            <%=Html.DropDownList("EraseRegist", (IEnumerable<SelectListItem>)ViewData["EraseRegistList"], new { style = "width:100%" })%>
        </td>
        <td colspan="2">
        </td>
    </tr>
    <tr>
        <th>
            外装評価
        </th>
        <td>
            <%=Html.TextBox("ExteriorEvaluation", Model.ExteriorEvaluation, new { @class = "alphanumeric", style = "width:80px", maxlength = 3 })%>
        </td>
        <th>
            内装評価
        </th>
        <td>
            <%=Html.TextBox("InteriorEvaluation", Model.InteriorEvaluation, new { @class = "alphanumeric", style="width:80px", maxlength = 3 })%>
        </td>
        <th>
            修復歴
        </th>
        <td>
            <%=Html.DropDownList("ReparationRecord", (IEnumerable<SelectListItem>)ViewData["ReparationRecordList"], new { style = "width:100%" })%>
        </td>
        <td colspan="2">
        </td>
        <th>
            シリアル
        </th>
        <td colspan="3">
            <%=Html.TextBox("UsVin", Model.UsVin, new { @class = "alphanumeric", maxlength = 20,style="width:274px" })%>
        </td>
    </tr>
    <tr>
        <th>
            査定価格
        </th>
        <%if (ViewData["PurchasedFlag"] != null && ViewData["PurchasedFlag"].Equals("1")) {%>
            <td><%=Html.TextBox("AppraisalPrice", Model.AppraisalPrice, new { @class = "numeric, readonly", style = "width:80px", maxlength = 10, @readonly = "readonly" })%></td>            
        <%}else{ %>
            <td><%=Html.TextBox("AppraisalPrice", Model.AppraisalPrice, new { @class = "numeric", style = "width:80px", maxlength = 10 })%></td>
        <%} %>
        <th>
            評価点
        </th>
        <td colspan="9">
            <%=Html.TextBox("Evaluation", Model.Evaluation, new { @class = "alphanumeric", style = "width:80px", maxlength = 3 })%>
        </td>
    </tr>
</table>
<!--2014/02/20 ADD DropDownListの初期値（selectedvalue）が取得出来なかった為 ookubo -->
<script type="text/javascript">
    document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxId").value;
</script>
</div>
<%} %>
<br />
</asp:Content>
<asp:Content ID="Content3" runat="server" contentplaceholderid="HeaderContent">

    <style type="text/css">
        .style1
        {
            width: 100px;
            height: 18px;
        }
        .style2
        {
            width: 80px;
            height: 18px;
        }
    </style>

</asp:Content>