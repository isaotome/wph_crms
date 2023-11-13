<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarPurchaseOrder>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両発注入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%using(Html.BeginForm("Entry","CarPurchaseOrder",FormMethod.Post)){ %>
<%=Html.ValidationSummary() %>
<br />
<%=Html.Hidden("close",ViewData["close"]) %>
<%
    CarGrade carGrade;
    string makerName = "";
    string gradeName = "";
    string brandName ="";
    string modelCode ="";
    string carName = "";
    string modelYear = "";
    try{
        carGrade = (CarGrade)ViewData["CarGrade"];
        makerName = carGrade.Car.Brand.Maker.MakerName;
        gradeName = carGrade.CarGradeName;
        brandName = carGrade.Car.Brand.CarBrandName;
        modelCode = carGrade.ModelCode;
        carName = carGrade.Car.CarName;
        modelYear = carGrade.ModelYear;
    }catch(NullReferenceException){
    }
%>
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="4">車両情報</th>
    </tr>
      <tr>
        <th style="width:120px">グレードコード *</th>
        <% // 2014/07/22  chrome対応 子画面からフォーカスが戻った場合、時間を置いてフォーカス移動を行う。%>
        <td style="width:200px"><%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = "15", maxlength = "20",onblur="GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarBrandName','CarGradeName','CarName','ModelCode','ModelYear'),'CarGrade')" })%>&nbsp;<img alt="グレード検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="var callback = function() { if(document.getElementById('CarGradeCode'))setTimeout(function(){document.getElementById('CarGradeCode').focus();if(document.getElementById('ExteriorColorCode'))document.getElementById('ExteriorColorCode').focus()}, 100);}; openSearchDialog('CarGradeCode','CarGradeName','/CarGrade/CriteriaDialog', null, null, null, null, callback);if(document.getElementById('CarGradeCode'))setTimeout(function(){document.getElementById('CarGradeCode').focus();if(document.getElementById('ExteriorColorCode'))document.getElementById('ExteriorColorCode').focus()}, 100);" /></td><%//Mod 2022/01/10 yano #4121 %>
        <%--<td style="width:200px"><%=Html.TextBox("CarGradeCode", Model.CarGradeCode, new { @class = "alphanumeric", size = "15", maxlength = "20",onblur="GetMasterDetailFromCode('CarGradeCode',new Array('MakerName','CarBrandName','CarGradeName','CarName','ModelCode','ModelYear'),'CarGrade')" })%>&nbsp;<img alt="グレード検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('CarGradeCode','CarGradeName','/CarGrade/CriteriaDialog');if(document.getElementById('CarGradeCode'))setTimeout(function(){document.getElementById('CarGradeCode').focus();if(document.getElementById('ExteriorColorCode'))document.getElementById('ExteriorColorCode').focus()}, 100);" /></td>--%>
        <th style="width:120px">メーカー名</th>
        <td style="width:200px"><span id="MakerName"><%=CommonUtils.DefaultNbsp(makerName) %></span></td>
      </tr>
      <tr>
        <th>グレード名</th>
        <td><span id="CarGradeName"><%=CommonUtils.DefaultNbsp(gradeName) %></span></td>
        <th>ブランド名</th>
        <td><span id="CarBrandName"><%=CommonUtils.DefaultNbsp(brandName) %></span></td>
      </tr>
      <tr>
        <th>モデルコード</th>
        <td><span id="ModelCode"><%=CommonUtils.DefaultNbsp(modelCode) %></span></td>
        <th>車種名</th>
        <td><span id="CarName"><%=CommonUtils.DefaultNbsp(carName) %></span></td>
      </tr>
      <tr>
        <th rowspan="2">外装色</th>
        <td><%=Html.TextBox("ExteriorColorCode",Model.ExteriorColorCode,new {@class="alphanumeric",size="10",maxlength="8",onchange="GetNameFromCode('ExteriorColorCode','ExteriorColorName','CarColor')"}) %>&nbsp;<img alt="外装色検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('ExteriorColorCode','ExteriorColorName','/CarColor/CriteriaDialog?ExteriorColorFlag=true')" /></td>
        <th rowspan="2">内装色</th>
        <td><%=Html.TextBox("InteriorColorCode",Model.InteriorColorCode,new {@class="alphanumeric",size="10",maxlength="8",onchange="GetNameFromCode('InteriorColorCode','InteriorColorName','CarColor')"}) %>&nbsp;<img alt="内装色検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('InteriorColorCode','InteriorColorName','/CarColor/CriteriaDialog?InteriorColorFlag=true')" /></td>
      </tr>
      <tr>
        <td><span id="ExteriorColorName"><%=CommonUtils.DefaultNbsp(ViewData["ExteriorColorName"]) %></span></td>
        <td><span id="InteriorColorName"><%=CommonUtils.DefaultNbsp(ViewData["InteriorColorName"]) %></span></td>
      </tr>
      <tr>
        <th>モデル年</th>
        <td><span id="ModelYear"><%=CommonUtils.DefaultNbsp(modelYear) %></span></td>
        <th>管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", Model.SalesCarNumber, new { @class = "alphanumeric", size = 10, maxlength = 20, onblur = "IsExistCode('SalesCarNumber','SalesCar')" })%></td>
      </tr>
    </table>
    <br />
    <table class="input-form">
      <tr>
            <th colspan="4" class="input-form-title">発注情報</th>
      </tr>
      <tr>
        <th style="width:120px" rowspan="2">仕入先 *</th>
        <td style="width:200px"><%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric", size = "15", maxlength = "10", onblur = "GetNameFromCode('SupplierCode','SupplierName','Supplier')" })%>&nbsp;<img alt="仕入先検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" /></td>
        <th style="width:120px" rowspan="2">部門 *</th>
        <td style="width:200px"><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", size = "10", maxlength = "3", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td><span id="SupplierName"><%=CommonUtils.DefaultNbsp(Model.Supplier!=null ? Model.Supplier.SupplierName : "") %></span></td>
       <td><span id="DepartmentName"><%=CommonUtils.DefaultNbsp(ViewData["DepartmentName"]) %></span></td>
      </tr>
      <tr>
        <th rowspan="2">支払先 *</th>
        <td><%=Html.TextBox("SupplierPaymentCode", Model.SupplierPaymentCode, new { @class = "alphanumeric", size = "15", maxlength = "10", onblur = "GetNameFromCode('SupplierPaymentCode','SupplierPaymentName','SupplierPayment')" })%>&nbsp;<img alt="支払先検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('SupplierPaymentCode','SupplierPaymentName','/SupplierPayment/CriteriaDialog')" /></td>
        <th rowspan="2">担当者 *</th>
        <td>
            <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
        </td>
      </tr>
      <tr>
        <td><span id="SupplierPaymentName"><%=CommonUtils.DefaultNbsp(Model.SupplierPayment!=null ? Model.SupplierPayment.SupplierPaymentName : "") %></span></td>
        <td>            
            <%=Html.TextBox("EmployeeName", Model.Employee!=null ? Model.Employee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>
      </tr>
      <tr>
        <th>発注日 *</th>
        <td><%=Html.TextBox("PurchaseOrderDate",string.Format("{0:yyyy/MM/dd}",Model.PurchaseOrderDate),new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
         <th>仕入価格 *</th>
      	<td><%=Html.TextBox("Amount", Model.Amount, new { @class = "numeric", size = "10", maxlength = "10" })%></td>
     
      </tr>
      <tr>
        <th>メーカー発注番号</th>
        <td><%=Html.TextBox("MakerOrderNumber", Model.MakerOrderNumber, new { @class = "alphanumeric", @size = "20", maxlength = "20" })%></td>
        <th>支払期限 *</th>
        <td><%=Html.TextBox("PayDueDate", string.Format("{0:yyyy/MM/dd}", Model.PayDueDate), new { @class = "alphanumeric", @size = "10", maxlength = "10" })%></td>
      </tr>
    </table>
<%} %>
</asp:Content>
