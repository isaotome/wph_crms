<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<%--基本情報--%>

<table class="input-form-slim">
    <tr>
        <th colspan="6" class="input-form-title">基本情報</th>
    </tr>
    <tr>
        <th style="width:80px">
            伝票番号
        </th>
        <td style="width:120px">
            <%=Html.TextBox("SlipNumber", Model.SlipNumber, new { @class = "readonly", style = "width:112px", @readonly = "readonly" })%>
        </td>
        <th style="width:80px">
            改訂番号
        </th>
        <td style="width:120px">
            <%if(Model.RevisionNumber>0){ %>
                <%=Html.TextBox("RevisionNumber", Model.RevisionNumber, new { @class = "readonly", style = "width:112px", @readonly = "readonly" })%>
            <%}else{ %>
                <%  //Edit 2014/05/15 yano name変更 %>
                <%//<%=Html.Hidden("RevisionNumbe", Model.RevisionNumber) %>
                <%=Html.Hidden("RevisionNumber", Model.RevisionNumber) %>
            <%} %>
        </td>
        <th style="width:80px"></th>
        <td style="width:120px"></td>
    </tr>
    <tr>
        <th>
            見積日 *
        </th>
        <td>
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("QuoteDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteDate), new { @class = "alphanumeric", style = "width:112px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("QuoteDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteDate), new { @class = "readonly alphanumeric", style = "width:112px", maxlength = "10", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            見積有効期限 *
        </th>
        <td>
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("QuoteExpireDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteExpireDate), new { @class = "alphanumeric", style = "width:112px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("QuoteExpireDate", string.Format("{0:yyyy/MM/dd}", Model.QuoteExpireDate), new { @class = "readonly alphanumeric", style = "width:112px", maxlength = "10", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>受注日</th>
        <td>
            <%if(Model.SalesOrderDateEnabled){ %>
                <%=Html.TextBox("SalesOrderDate", string.Format("{0:yyyy/MM/dd}", Model.SalesOrderDate), new { @class = "alphanumeric", style = "width:112px", maxlength = "10" })%>
            <%}else{ %>
                <%=Html.TextBox("SalesOrderDate", string.Format("{0:yyyy/MM/dd}", Model.SalesOrderDate), new { @class = "readonly", style = "width:112px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <!--2014/02/20 ADD 納車予定日、納車日、消費税率を追加 ookubo -->
    <%=Html.Hidden("Rate",Model.Rate) %>
    <%=Html.Hidden("ConsumptionTaxId", ViewData["ConsumptionTaxId"])%>
    <%=Html.Hidden("ConsumptionTaxIdOld", ViewData["ConsumptionTaxIdOld"])%>
    <%=Html.Hidden("DateOld1", ViewData["SalesDateOld"])%>
    <%=Html.Hidden("DateOld2", ViewData["SalesPlanDateOld"])%>
    <!-- Add 2014/06/19 arc yano 税率変更バグ対応 -->
    <%=Html.Hidden("canSave", 1)%>

    <tr>
        <th>
            納車予定日 *
        </th>
        <td>
            <%if (Model.SalesOrderDateEnabled){ %>
            <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "alphanumeric", style = "width:112px", maxlength = "10", onchange = "setTaxIdByDate(document.getElementById('SalesDate'), this, 'CarSalesOrder');" })%>
            <%} else { %>
            <%=Html.TextBox("SalesPlanDate", string.Format("{0:yyyy/MM/dd}", Model.SalesPlanDate), new { @class = "readonly", style = "width:112px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            納車日
        </th>
        <td>
            <%if (Model.SalesDateEnabled){ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate), new { @class = "alphanumeric", style = "width:112px", maxlength = "10", onchange = "setTaxIdByDate(this, document.getElementById('SalesPlanDate'), 'CarSalesOrder');" })%>
            <%}else{ %>
                <%=Html.TextBox("SalesDate", string.Format("{0:yyyy/MM/dd}", Model.SalesDate),  new { @class = "readonly", style = "width:112px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            消費税率 *
        </th>
        <td>
            <%if (Model.RateEnabled) { %>
            <% //<!--　Edit 2014/06/23 arc yano 消費税変更バグ対応 引数追加-->%>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "alphanumeric", style = "width:118px", onchange = "setTaxRateById(this, 'CarSalesOrder', null);" })%>
            <%}else{ %>
                <%=Html.DropDownList("ConsumptionTaxList", (IEnumerable<SelectListItem>)ViewData["ConsumptionTaxList"], new { @class = "readonly alphanumeric", style = "width:118px", @disabled = "disabled" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>HOT管理</th>
        <td colspan="5" style="text-align:left">
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <%if (Model.BasicEnabled) { %>
                <%=Html.RadioButton("HotStatus", "A", CommonUtils.DefaultString(Model.HotStatus).Equals("A"))%>AHOT
                <%=Html.RadioButton("HotStatus", "B", CommonUtils.DefaultString(Model.HotStatus).Equals("B"))%>BHOT
                <%=Html.RadioButton("HotStatus", "C", CommonUtils.DefaultString(Model.HotStatus).Equals("C"))%>CHOT
                <%=Html.RadioButton("HotStatus", "D", CommonUtils.DefaultString(Model.HotStatus).Equals("D"))%>DHOT
            <%} else { %>
                <%=Html.RadioButton("HotStatus", "A", CommonUtils.DefaultString(Model.HotStatus).Equals("A"), new { @disabled = "disabled" })%>AHOT
                <%=Html.RadioButton("HotStatus", "B", CommonUtils.DefaultString(Model.HotStatus).Equals("B"), new { @disabled = "disabled" })%>BHOT
                <%=Html.RadioButton("HotStatus", "C", CommonUtils.DefaultString(Model.HotStatus).Equals("C"), new { @disabled = "disabled" })%>CHOT
                <%=Html.RadioButton("HotStatus", "D", CommonUtils.DefaultString(Model.HotStatus).Equals("D"), new { @disabled = "disabled" })%>DHOT
                <%=Html.Hidden("HotStatus",Model.HotStatus) %>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>部門 *</th>
        <td colspan="5" style="text-align:left">
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "3", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" }); %>
                <%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "readonly alphanumeric", style = "width:100px",  @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "1" }); %>
                <%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>担当者 *</th>
        <td colspan="5" style="text-align:left">
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:100px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
                <%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:340px", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "readonly alphanumeric", style = "width:50px", @readonly = "readonly" })%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "1" }); %>
                <%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:340px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>イベント1</th>
        <td colspan="5" style="text-align:left">
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "alphanumeric", style = "width:100px", maxlength = "20", onblur = "GetNameFromCode('CampaignCode1','CampaignName1','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode1", "CampaignName1", "'/Campaign/CriteriaDialog'", "0" }); %>
                <%=Html.TextBox("CampaignName1", ViewData["CampaignName1"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("CampaignCode1", Model.CampaignCode1, new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode1", "CampaignName1", "'/Campaign/CriteriaDialog'", "1" }); %>
                <%=Html.TextBox("CampaignName1", ViewData["CampaignName1"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>イベント2</th>
        <td colspan="5" style="text-align:left">
            <%if(Model.BasicEnabled){ %>
                <%=Html.TextBox("CampaignCode2", Model.CampaignCode2, new { @class = "alphanumeric", style = "width:100px", maxlength = "20", onblur = "GetNameFromCode('CampaignCode2','CampaignName2','Campaign')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode2", "CampaignName2", "'/Campaign/CriteriaDialog'", "0" }); %>
                <%=Html.TextBox("CampaignName2", ViewData["CampaignName2"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%}else{ %>
                <%=Html.TextBox("CampaignCode2", Model.CampaignCode2, new { @class = "readonly alphanumeric", style = "width:100px", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "CampaignCode2", "CampaignName2", "'/Campaign/CriteriaDialog'", "1" }); %>
                <%=Html.TextBox("CampaignName2", ViewData["CampaignName2"], new { @class = "readonly", style = "width:400px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
</table>
<!--2014/02/20 ADD DropDownListの初期値（selectedvalue）が取得出来なかった為 ookubo -->
<script type="text/javascript">
    document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxId").value;
</script>

