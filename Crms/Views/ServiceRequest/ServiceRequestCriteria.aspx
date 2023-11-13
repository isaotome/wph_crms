<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceRequest>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	作業依頼検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "ServiceRequest", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
 <table class="input-form">
    <tr>
        <th rowspan="2">担当部門</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = "10", maxlength = "3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%> <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th rowspan="2">担当営業</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", size = "15", maxlength = "50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')" })%> <img alt="担当者検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
        <td style="height:20px"><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
    </tr>
    <tr>
        <th>顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"], new { size = "15", maxlength = "40" })%></td>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = "15", maxlength = "20" })%></td>
    </tr>
    <tr>
        <th>入庫予定日</th>
        <td colspan="3"><%=Html.TextBox("ArrivalPlanDateFrom", ViewData["ArrivalPlanDateFrom"], new { @class = "alphanumeric", size = "10",maxlength="10", onchange ="return chkDate3(document.getElementById('ArrivalPlanDateFrom').value, 'ArrivalPlanDateFrom')" })%> ～ <%=Html.TextBox("ArrivalPlanDateTo", ViewData["ArrivalPlanDateTo"], new { @class = "alphanumeric", size = "10", maxlength = "10", onchange ="return chkDate3(document.getElementById('ArrivalPlanDateTo').value, 'ArrivalPlanDateTo')" })%></td>
    </tr>
    <tr>
        <th>希望納期</th>
        <td colspan="3"><%=Html.TextBox("DeliveryRequirementFrom", ViewData["DeliveryRequirementFrom"], new { @class = "alphanumeric", size = "10", maxlength = "10", onchange ="return chkDate3(document.getElementById('DeliveryRequirementFrom').value, 'DeliveryRequirementFrom')" })%> ～ <%=Html.TextBox("DeliveryRequirementTo", ViewData["DeliveryRequirementTo"], new { @class = "alphanumeric", size = "10", maxlength = "10", onchange ="return chkDate3(document.getElementById('DeliveryRequirementTo').value, 'DeliveryRequirementTo')"  })%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="4">
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0');formSubmit()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DepartmentCode', 'DepartmentName', 'EmployeeCode', 'EmployeeName', 'CustomerName', 'Vin', 'ArrivalPlanDateFrom', 'ArrivalPlanDateTo', 'DeliveryRequirementFrom', 'DeliveryRequirementTo'))"/>
        </td>
    </tr>
</table>
</div>
<br />
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<%} %>
<br />
<table class="list">
    <tr>
        <% // Mod 2014/07/11 arc amii chrome対応 見積作成が折り返さないよう項目の幅を修正 %>
        <th style="width:30px"></th>
        <th>担当拠点</th>
        <th>担当営業</th>
        <th>伝票番号</th>
        <th>顧客名</th>
        <th>車種</th>
        <th>車台番号</th>
        <th>入庫予定日</th>
        <th>希望納期</th>
        <th>備考</th>

    </tr>
    <%foreach (var s in Model){%>
    <%
          string departmentName = "";
          string employeeName = "";
          string customerName = "";
          string carName = "";
          string vin = "";
          DateTime? arrivalPlanDate = null;

          try { departmentName = s.CarSalesHeader.Department.DepartmentName; } catch (NullReferenceException) { }
          try { employeeName = s.CarSalesHeader.Employee.EmployeeName; } catch (NullReferenceException) { }
          try { customerName = s.CarSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }
          try { carName = s.CarSalesHeader.CarName; } catch (NullReferenceException) { }
          try { vin = s.CarSalesHeader.Vin; } catch (NullReferenceException) { }
          try { arrivalPlanDate = s.CarPurchaseOrder.ArrivalPlanDate; } catch (NullReferenceException) { }
     %>
    <tr>
        <td><%if(s.CarSalesHeader!=null){ %><a href="javascript:void(0);" onclick="openModalAfterRefresh('/ServiceRequest/Result?SlipNo=<%=s.OriginalSlipNumber %>')">詳細</a><%} %></td>
        <td><%=departmentName %></td>
        <td><%=employeeName %></td>
        <td><%=s.OriginalSlipNumber %></td>
        <td><%=customerName %></td>
        <td><%=carName %></td>
        <td><%=vin %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",arrivalPlanDate) %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",s.DeliveryRequirement) %></td>
        <td><%=s.Memo %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
