<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<table class="input-form-slim">
    <tr>
        <th colspan="6" class="input-form-title">
            基本情報
        </th>
    </tr>
    <tr>
        <th style="width: 80px">
            伝票番号
        </th>
        <td style="width: 100px">
            <%=Html.TextBox("SlipNumber", Model.SlipNumber, new { @class = "readonly", @readonly = "readonly", style = "width:100px" })%>
        </td>
        <th style="width: 80px">
            改訂番号
        </th>
        <td style="width: 100px">
            <%if(Model.RevisionNumber==0){ %>
                <%=Html.TextBox("RevisionNumber1", "", new { @class = "readonly", @readonly = "readonly", style = "width:100px" })%>
                <%=Html.Hidden("RevisionNumber",0) %>
            <%}else{ %>
                <%=Html.TextBox("RevisionNumber", Model.RevisionNumber, new { @class = "readonly", @readonly = "readonly", style = "width:100px" })%>
            <%} %>
        </td>
        <th style="width: 80px">
            入庫日
        </th>
        <td style="width: 100px">
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric readonly",@readonly="readonly", style = "width:100px", maxlength = "10" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            見積日
        </th>
        <td>
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("QuoteDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("QuoteDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteDate), new { @class = "alphanumeric readonly", @readonly="readonly", style = "width:100px", maxlength = "10" })%>
            <%} %>
        </td>
        <th>
            見積有効期限
        </th>
        <td>
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("QuoteExpireDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteExpireDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("QuoteExpireDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteExpireDate), new { @class = "alphanumeric readonly", @readonly="readonly", style = "width:100px", maxlength = "10" })%>
            <%} %>
        </td>
        <th>
            受注日
        </th>
        <td>
            <%=Html.TextBox("SalesOrderDate", string.Format("{0:yyyy/MM/dd}", Model.SalesOrderDate), new { @class = "readonly", @readonly = "readonly", style = "width:100px" })%>
        </td>
    </tr>
   <!--2014/02/20 ADD 納車予定日、納車日、消費税率を追加 ookubo -->
    <tr>
        <%=Html.Hidden("Rate",Model.Rate) %>
        <%=Html.Hidden("ConsumptionTaxId", ViewData["ConsumptionTaxId"])%>
        <%=Html.Hidden("ConsumptionTaxIdOld", ViewData["ConsumptionTaxIdOld"])%>
        <%=Html.Hidden("DateOld1", ViewData["SalesDateOld"])%>
        <%=Html.Hidden("DateOld2", ViewData["SalesPlanDateOld"])%>
           <!-- Add 2014/06/19 arc yano 税率変更バグ対応 -->
        <%=Html.Hidden("canSave", 1)%>

        <th>
            納車予定日 *
        </th>
        <td>
            <%if (Model.SlipEnabled){ %>
            <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10", onchange = "setTaxIdByDate(document.getElementById('SalesDate'), this, 'ServiceSalesOrder');" })%>
            <%} else { %>
            <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            納車日
        </th>
        <td>
            <%if(Model.SalesDateEnabled){ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10", onchange = "setTaxIdByDate(this,document.getElementById('SalesPlanDate'),'ServiceSalesOrder');" })%>
            <%}else{ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate), new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            消費税率 *
        </th>
        <td>
            <%if (Model.RateEnabled) { %>
            <% //<!--　Edit 2014/06/23 arc yano 消費税変更バグ対応 引数追加-->%>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "alphanumeric", style = "width:105px", Value = Model.ConsumptionTaxId, onchange = "setTaxRateById(this, 'ServiceSalesOrder', null);" })%>
                       
            <%}else{ %>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "readonly alphanumeric", style = "width:105px", @disabled = "disabled", Value = Model.ConsumptionTaxId })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            部門
        </th>
        <td colspan="3">
            <!--Mod 2016/12/26 arc nakayama #3687_サービス伝票　赤黒処理後の黒伝票は部門を編集不可にする-->
            <%if(Model.SlipEnabled){ %>
                <%if (!string.IsNullOrEmpty(Model.SlipNumber) && Model.SlipNumber.Contains("-2"))
                  {%>
                    <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class="alphanumeric readonly", @readonly="readonly", style="width:50px" ,maxlength="3"})%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "1" });%>
                <%}else{ %>
                    <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class="alphanumeric",style="width:50px" ,maxlength="3" ,onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')"})%>
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" });%>
                <%} %>
            <%}else{ %>
                <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class="alphanumeric readonly", @readonly="readonly", style="width:50px" ,maxlength="3"})%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "1" });%>
            <%} %>
            <%=Html.TextBox("DepartmentName", Model.Department != null ? Model.Department.DepartmentName : "", new { @class = "readonly", @readonly = "readonly", style = "width:213px" })%>
        </td>
        <th>
            車両伝票番号
        </th>
        <td>
            <%=Html.TextBox("CarSlipNumber",string.Format("{0:yyyy/MM/dd}",Model.CarSlipNumber), new { @class = "readonly", @readonly = "readonly", style = "width:100px" })  %>
        </td>
    </tr>
    <tr>
        <%//Mod 2019/03/07 yano #3976 保留のため、文言をもとに戻す(営業→受付) %>
        <%//Mod 2019/02/13 yano #3976 文言の変更(受付→営業)  %>
        <th>
            受付担当者
        </th>
        <td colspan="3">
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("ReceiptionEmployeeNumber", Model.ReceiptionEmployee != null ? Model.ReceiptionEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('ReceiptionEmployeeNumber',new Array('ReceiptionEmployeeCode','ReceiptionEmployeeName'),'Employee')" })%><%=Html.TextBox("ReceiptionEmployeeCode",Model.ReceiptionEmployeeCode,new {@class="alphanumeric",style="width:80px", maxlength="50",onblur="GetMasterDetailFromCode('ReceiptionEmployeeCode',new Array('ReceiptionEmployeeNumber','ReceiptionEmployeeName'),'Employee')"})%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ReceiptionEmployeeCode", "ReceiptionEmployeeName", "'/Employee/CriteriaDialog'", "0" });%>
            <%}else{ %>
                <%=Html.TextBox("ReceiptionEmployeeNumber", Model.ReceiptionEmployee != null ? Model.ReceiptionEmployee.EmployeeNumber : "", new { @class = "alphanumeric readonly", @readonly="readonly", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('ReceiptionEmployeeNumber',new Array('ReceiptionEmployeeCode','ReceiptionEmployeeName'),'Employee')" })%><%=Html.TextBox("ReceiptionEmployeeCode", Model.ReceiptionEmployeeCode, new { @class = "alphanumeric readonly", @readonly = "readonly", style = "width:80px", maxlength = "50" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "ReceiptionEmployeeCode", "ReceiptionEmployeeName", "'/Employee/CriteriaDialog'", "1" });%>
            <%} %>
            <%=Html.TextBox("ReceiptionEmployeeName", Model.ReceiptionEmployee != null ? Model.ReceiptionEmployee.EmployeeName : "", new { @class = "readonly", style = "width:127px", @readonly = "readonly" })%>
        </td>
        <th>
            車両受注日
        </th>
        <td>
            <%=Html.TextBox("CarSalesOrderDate",string.Format("{0:yyyy/MM/dd}",Model.CarSalesOrderDate), new { @class = "readonly", @readonly = "readonly", style = "width:100px" })  %>
        </td>
    </tr>
    <tr>
        <th>
            フロント担当者
        </th>
        <td colspan="3">
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("FrontEmployeeNumber", Model.FrontEmployee != null ? Model.FrontEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('FrontEmployeeNumber',new Array('FrontEmployeeCode','FrontEmployeeName'),'Employee')" })%><%=Html.TextBox("FrontEmployeeCode", Model.FrontEmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('FrontEmployeeCode',new Array('FrontEmployeeNumber','FrontEmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "FrontEmployeeCode", "FrontEmployeeName", "'/Employee/CriteriaDialog'", "0" });%>
            <%}else{ %>
                <%=Html.TextBox("FrontEmployeeNumber", Model.FrontEmployee != null ? Model.FrontEmployee.EmployeeNumber : "", new { @class = "alphanumeric readonly", style = "width:50px", maxlength = "20", @readonly="readonly"})%><%=Html.TextBox("FrontEmployeeCode", Model.FrontEmployeeCode, new { @class = "alphanumeric readonly", @readonly="readonly", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('FrontEmployeeCode',new Array('FrontEmployeeNumber','FrontEmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "FrontEmployeeCode", "FrontEmployeeName", "'/Employee/CriteriaDialog'", "1" });%>
            <%} %>
            <%=Html.TextBox("FrontEmployeeName", Model.FrontEmployee != null ? Model.FrontEmployee.EmployeeName : "", new { @class = "readonly", style = "width:127px", @readonly = "readonly" })%>
        </td>
        <% //<!--　ADD 2014/11/25 arc ookubo #3135 車両預り中-->%>
        <th>
            車両預り中
        </th>
        <td>
            <%if (Model.KeepsCarFlagEnabled){ %>
                <%=Html.CheckBox("KeepsCarFlag",Model.KeepsCarFlag!=null && Model.KeepsCarFlag == true ? true : false) %>
            <%}else{ %>
                <%=Html.CheckBox("KeepsCarFlag",Model.KeepsCarFlag!=null && Model.KeepsCarFlag == true ? true : false, new { disabled="disabled" })%>
            <%} %>
        </td>
<!--  DEL 2014/02/20 ookubo
        <th>
            納車日
        </th>
        <td>
            <%if(Model.SalesDateEnabled){ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate), new { @class = "alphanumeric", style = "width:100px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate), new { @class = "readonly", style = "width:100px", @readonly = "readonly" })%>
            <%} %>
        </td>
    DEL 2014/02/20 ookubo -->
    </tr>
    <tr>
        <th>
            イベント1
        </th>
        <td colspan="5">
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "alphanumeric", style = "width:100px", maxlength = "50", onblur = "GetNameFromCode('CampaignCode1','CampaignName1','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode1", "CampaignName1", "'/Campaign/CriteriaDialog'", "0" });%>
            <%}else{ %>
                <%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "alphanumeric readonly",@readonly="readonly", style = "width:100px", maxlength = "50", onblur = "GetNameFromCode('CampaignCode1','CampaignName1','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode1", "CampaignName1", "'/Campaign/CriteriaDialog'", "1" });%>
            <%} %>
            <%=Html.TextBox("CampaignName1",Model.Campaign1!=null ? Model.Campaign1.CampaignName : "", new { @class = "readonly", @readonly = "readonly", style = "width:357px" })  %>
        </td>
    </tr>
    <tr>
        <th>
            イベント2
        </th>
        <td colspan="5">
            <%if(Model.SlipEnabled){ %>
                <%=Html.TextBox("CampaignCode2", Model.CampaignCode2, new { @class = "alphanumeric", style = "width:100px", maxlength = "50", onblur = "GetNameFromCode('CampaignCode2','CampaignName2','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode2", "CampaignName2", "'/Campaign/CriteriaDialog'", "0" });%>
            <%}else{ %>
                <%=Html.TextBox("CampaignCode2", Model.CampaignCode2, new { @class = "alphanumeric readonly", @readonly="readonly", style = "width:100px", maxlength = "50", onblur = "GetNameFromCode('CampaignCode2','CampaignName2','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode2", "CampaignName2", "'/Campaign/CriteriaDialog'", "1" });%>
            <%} %>
            <%=Html.TextBox("CampaignName2",Model.Campaign2!=null ? Model.Campaign2.CampaignName : "", new { @class = "readonly", @readonly = "readonly", style = "width:357px" })  %>
        </td>
    </tr>
</table>
<!--2014/02/20 ADD DropDownListの初期値（selectedvalue）が取得出来なかった為 ookubo -->
<script type="text/javascript">
    document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxId").value;
</script>
