<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Transfer>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両移動入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%if (ViewData["ClosedMonth"] != null && !ViewData["ClosedMonth"].Equals("1")) { %>
       <%} else { %>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%} %>
   </tr>
</table>
<br />
<%using (Html.BeginForm("Entry","CarTransfer",FormMethod.Post)){ %>
<%=Html.ValidationSummary() %>
<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="4">伝票情報</th>
    </tr>
	<tr>
		<th style="width:100px">
		    出庫日 *
		</th>
		<td style="width:200px">
		    <%if(ViewData["ClosedMonth"]!=null && ViewData["ClosedMonth"].Equals("1")){ %>
		        <%=Html.TextBox("DepartureDate", string.Format("{0:yyyy/MM/dd}", Model.DepartureDate), new { @class = "alphanumeric readonly", size = 10, @readonly = "readonly" }) %>
		    <%}else{ %>
		        <%=Html.TextBox("DepartureDate", string.Format("{0:yyyy/MM/dd}", Model.DepartureDate), new { @class = "alphanumeric", size = "10", maxlength = "10" }) %>
		    <%} %>
		</td>
		<th style="width:100px">
		    入庫予定日 *
		</th>
		<td style="width:200px">
		    <%if(ViewData["ClosedMonth"]!=null && ViewData["ClosedMonth"].Equals("1")){ %>
		        <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric readonly", size = "10", @readonly = "readonly" })%>
            <%}else{ %>
		        <%=Html.TextBox("ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", Model.ArrivalPlanDate), new { @class = "alphanumeric", size = "10", maxlength = "10" })%>
            <%} %>
        </td>
	</tr>
	<tr>
		<th style="width:100px" rowspan="2">
		    出庫担当者 *
		</th>
		<td style="width:200px">
		    <%if(ViewData["ClosedMonth"]!=null && ViewData["ClosedMonth"].Equals("1")){ %>
                <%=Html.TextBox("DepartureEmployeeNumber", Model.DepartureEmployee != null ? Model.DepartureEmployee.EmployeeNumber : "", new { @class = "readonly alphanumeric", style = "width:50px", maxlength = "20", @readonly = "readonly" })%>
                <%=Html.TextBox("DepartureEmployeeCode", Model.DepartureEmployeeCode, new { @class = "readonly alphanumeric", style = "width:80px", maxlength = "50", @readonly = "readonly" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureEmployeeCode", "DepartureEmployeeName", "'/Employee/CriteriaDialog'", "1" }); %>
		    <%}else{ %>
                <%=Html.TextBox("DepartureEmployeeNumber", Model.DepartureEmployee != null ? Model.DepartureEmployee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('DepartureEmployeeNumber',new Array('DepartureEmployeeCode','DepartureEmployeeName'),'Employee')" })%>
                <%=Html.TextBox("DepartureEmployeeCode", Model.DepartureEmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('DepartureEmployeeCode',new Array('DepartureEmployeeNumber','DepartureEmployeeName'),'Employee')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartureEmployeeCode", "DepartureEmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
		    <%} %>
		<th style="width:100px" rowspan="2">
		    入庫ロケーション *
		</th>
		<td style="width:200px">
            <%// Mod 2020/08/29 yano #4057 %><%//移動済の場合は入庫ロケーションは編集不可 %>
            <%if (Model.ArrivalDate != null && !string.IsNullOrWhiteSpace(Model.ArrivalEmployeeCode))
              { %>

		    <%--<%if(ViewData["ClosedMonth"]!=null && ViewData["ClosedMonth"].Equals("1")){ %>--%>
		        <%=Html.TextBox("ArrivalLocationCode", Model.ArrivalLocationCode, new { @class = "alphanumeric readonly", size = "10", @readonly = "readonly" }) %>
		    <%}else{ %>
		        <%=Html.TextBox("ArrivalLocationCode",Model.ArrivalLocationCode,new {@class="alphanumeric",size="10",maxlength="12",onchange="GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')"}) %>&nbsp;<img alt="入庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ArrivalLocationCode','ArrivalLocationName','/Location/CriteriaDialogForCarUsage')" /><%//Mod 2017/07/04 arc yano #3736 車両移動入力 サービス部門のロケーションが選択できない %>
		    <%} %>
		</td>
	</tr>
	<tr>
	    <td>
	        <%=Html.TextBox("DepartureEmployeeName", Model.DepartureEmployee!=null ? Model.DepartureEmployee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
	    <td><span id="ArrivalLocationName"><%=CommonUtils.DefaultNbsp(Model.ArrivalLocation!=null ? Model.ArrivalLocation.LocationName : "")%></span></td>
	</tr>
	<tr>
	    <th>移動種別 *</th>
	    <td>
	        <%if(ViewData["ClosedMonth"]!=null && ViewData["ClosedMonth"].Equals("1")){ %>
                <%=Html.Encode(Model.c_TransferType!=null ? Model.c_TransferType.Name : "")%>
            <%}else{ %>
                <%=Html.DropDownList("TransferType",(IEnumerable<SelectListItem>)ViewData["TransferTypeList"]) %>
            <%} %>
        </td>
	    <th></th>
	    <td></td>
	</tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="4">車両情報</th>
    </tr>
    <tr>
        <th style="width:100px">管理番号 *</th>
        <td style="width:200px">
            <%//if (ViewData["update"] != null && ViewData["update"].Equals("1")) { %>
            <!--2017/07/14 arc nakayama #3778_車両移動データの編集＆削除機能追加 入庫確定前は入庫確定画面以外でも編集可能-->
            <%if (Model.ArrivalDate != null){ %>
                <%=CommonUtils.DefaultNbsp(Model.SalesCarNumber)%><%=Html.Hidden("SalesCarNumber", Model.SalesCarNumber)%>
            <%} else { %>
               <%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { size = "20", onblur = "GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar')" })%>&nbsp;<img alt="管理番号" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="var callback = function() { setTimeout(function (){ GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar'); document.forms[0].SalesCarNumber.focus();}, 100);}; openSearchDialog('SalesCarNumber','Vin','/SalesCar/CriteriaDialog', null, null, null, null, callback); setTimeout(function (){ GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar'); document.forms[0].SalesCarNumber.focus();}, 100);" /><%//Mod 2022/01/10 yano #4121 %>
               <%--<%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { size = "20", onblur = "GetMasterDetailFromCode('SalesCarNumber',new Array('Vin','MakerName','CarBrandName','CarName','CarGradeName','LocationName','LocationCode'),'SalesCar')" })%>&nbsp;<img alt="管理番号" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SalesCarNumber','Vin','/SalesCar/CriteriaDialog');document.forms[0].SalesCarNumber.focus();" />--%>
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
        <%//Mod 2020/06/19 yano #4054 %>
        <%if(Model.ArrivalDate == null){%>
            <th>現在地</th>
            <td><span id="LocationCode"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.LocationCode : "") %></span>&nbsp;<span id="LocationName"><%=CommonUtils.DefaultNbsp((Model.SalesCar!=null && Model.SalesCar.Location!=null) ? Model.SalesCar.Location.LocationName : "") %></span></td>
        <%}else{ %>
            <th>出庫ロケーション</th>
            <td><span id="LocationCode"><%=CommonUtils.DefaultNbsp(Model.DepartureLocationCode) %></span>&nbsp;<span id="LocationName"><%=CommonUtils.DefaultNbsp((Model.DepartureLocation != null) ? Model.DepartureLocation.LocationName : "") %></span></td>
        <%} %>
    </tr>
</table>
<%=Html.Hidden("close",ViewData["close"]) %>
<% //Add 2014/08/18 arc amii 車両移動複数回バグ対応 #3045 更新 or 追加の判断に必要なupdateがこの画面だけ記述が無かった為、追加 %>
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("TransferNumber",Model.TransferNumber) %>
<%} %>
</asp:Content>
