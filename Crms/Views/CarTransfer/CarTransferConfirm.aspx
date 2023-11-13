<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Transfer>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両移動入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="document.forms[0].ActionType.value='Complete';formSubmit()"><img src="/Content/Images/build.png" alt="入庫確定" class="command_btn"/>&nbsp;入庫確定</td>
       <!--2017/07/14 arc nakayama #3778_車両移動データの編集＆削除機能追加-->
       <td onclick="document.forms[0].ActionType.value='Delete';formCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
   </tr>
</table>
<br />
<%using (Html.BeginForm("Confirm","CarTransfer",FormMethod.Post)){ %>
<%=Html.Hidden("ActionType","") %>
<%=Html.ValidationSummary() %>
<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="4">伝票情報</th>
    </tr>
	<tr>
		<th style="width:100px">出庫日</th>
		<td style="width:200px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model.DepartureDate))%><%=Html.Hidden("DepartureDate",Model.DepartureDate) %></td>
		<th style="width:100px">入庫予定日</th>
		<td style="width:200px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model.ArrivalPlanDate))%><%=Html.Hidden("ArrivalPlanDate",Model.ArrivalPlanDate) %></td>
	</tr>
	<tr>
		<th style="width:100px" rowspan="2">出庫担当者</th>
		<td style="width:200px"><%=CommonUtils.DefaultNbsp(Model.DepartureEmployeeCode)%><%=Html.Hidden("DepartureEmployeeCode",Model.DepartureEmployeeCode) %></td>
		<th style="width:100px" rowspan="2">入庫ロケーション *</th>
		<td style="width:200px"><%=Html.TextBox("ArrivalLocationCode",Model.ArrivalLocationCode,new {@class="alphanumeric",size="10",maxlength="12",onchange="GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')"}) %>&nbsp;<img alt="入庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ArrivalLocationCode','ArrivalLocationName','/Location/CriteriaDialog?BusinessType=001,009')" /></td>
	</tr>
	<tr>
	    <td><span id="DepartureEmployeeName"><%=CommonUtils.DefaultNbsp(Model.DepartureEmployee!=null ? Model.DepartureEmployee.EmployeeName : "") %></span></td>
	    <td><span id="ArrivalLocationName"><%=CommonUtils.DefaultNbsp(Model.ArrivalLocation!=null ? Model.ArrivalLocation.LocationName : "")%></span></td>
	</tr>
	<tr>
	    <th>移動種別 *</th>
        <td><%=Html.DropDownList("TransferType",(IEnumerable<SelectListItem>)ViewData["TransferTypeList"]) %></td>
	    <th rowspan="2">入庫担当者</th>
	    <td>
            <%=Html.TextBox("ArrivalEmployeeNumber", Model.ArrivalEmployee != null ? Model.ArrivalEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('ArrivalEmployeeNumber',new Array('ArrivalEmployeeCode','ArrivalEmployeeName'),'Employee')" })%>
            <%=Html.TextBox("ArrivalEmployeeCode", Model.ArrivalEmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('ArrivalEmployeeCode',new Array('ArrivalEmployeeNumber','ArrivalEmployeeName'),'Employee')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "ArrivalEmployeeCode", "ArrivalEmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
	    </td>
	</tr>
	<tr>
	    <th>入庫日 *</th>
	    <td><%=Html.TextBox("ArrivalDate",string.Format("{0:yyyy/MM/dd}",Model.ArrivalPlanDate),new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
        <td><%=Html.TextBox("ArrivalEmployeeName", Model.ArrivalEmployee!=null ? Model.ArrivalEmployee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%></td>
	</tr>
</table>
<br />

<table class="input-form">
<!--2017/07/14 arc nakayama #3778_車両移動データの編集＆削除機能追加 入庫確定前も編集可能-->
<%if(Model.ArrivalDate != null) {%>

    <tr>
        <th class="input-form-title" colspan="4">車両情報</th>
    </tr>
    <tr>
        <th style="width:100px">管理番号 *</th>
        <td style="width:200px">
            <%if (ViewData["update"] != null && ViewData["update"].Equals("1")) { %>
                <%=CommonUtils.DefaultNbsp(Model.SalesCarNumber)%><%=Html.Hidden("SalesCarNumber", Model.SalesCarNumber)%>
            <%} else { %>
                <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { size = "20", onblur = "GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar')" })%>&nbsp;<img alt="管理番号" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function(){ GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar')}; openSearchDialog('SalesCarNumber','Vin','/SalesCar/CriteriaDialog', null, null, null, null, callback); GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar');" /><%//Mod 2022/02/02 yano #4128%>
            <%} %>
        </td>
        <th style="width:100px">車台番号</th>
        <td style="width:200px"><span id="Vin"><%=CommonUtils.DefaultNbsp(Model.SalesCar !=null ? Model.SalesCar.Vin : "") %></span></td>
    </tr>
    <tr>
        <th>メーカー</th>
        <td><span id="MakerName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.Brand.Maker.MakerName : "") %></span></td>
        <th>ブランド</th>
        <td><span id="CarBrandName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.Brand.CarBrandName : "") %></span></td>
    </tr>
    <tr>
        <th>車種</th>
        <td><span id="CarName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.CarName : "") %></span></td>
        <th>グレード</th>
        <td><span id="CarGradeName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.CarGradeName : "") %></span></td>
    </tr>
    <tr>
        <th>現在地</th>
        <td><span id="LocationCode"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.LocationCode : "") %></span>&nbsp;<span id="LocationName"><%=CommonUtils.DefaultNbsp((Model.SalesCar!=null && Model.SalesCar.Location!=null) ? Model.SalesCar.Location.LocationName : "") %></span></td>
    </tr>

<%}else{ %>

    <tr>
        <th class="input-form-title" colspan="4">車両情報</th>
    </tr>
    <tr>
        <th style="width:100px">車台番号</th>
        <td style="width:200px"><%=CommonUtils.DefaultNbsp(Model.SalesCar != null ? Model.SalesCar.Vin : "") %></td>
        <th style="width:100px">管理番号</th>
        <td style="width:200px"><%=CommonUtils.DefaultNbsp(Model.SalesCar !=null ? Model.SalesCar.SalesCarNumber : "")%><%=Html.Hidden("SalesCarNumber",Model.SalesCar!=null ? Model.SalesCar.SalesCarNumber : "") %></td>
    </tr>
    <tr>
        <th>メーカー</th>
        <td><span id="MakerName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.Brand.Maker.MakerName : "") %></span></td>
        <th>ブランド</th>
        <td><span id="CarBrandName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.Brand.CarBrandName : "") %></span></td>
    </tr>
    <tr>
        <th>車種</th>
        <td><span id="CarName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.Car.CarName : "") %></span></td>
        <th>グレード</th>
        <td><span id="CarGradeName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.CarGrade.CarGradeName : "") %></span></td>
    </tr>
    <tr>
        <th>現在地</th>
        <td><span id="LocationCode"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.LocationCode : "") %></span>&nbsp;<span id="LocationName"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.Location.LocationName : "") %></span></td>
    </tr>    

<%} %>
</table>
<%=Html.Hidden("TransferNumber",Model.TransferNumber) %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%} %>
</asp:Content>
