<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarPurchaseOrder>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両発注依頼検索
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","CarPurchaseOrder",FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:block">
<br />
<table class="input-form">
    <tr>
        <th>ステータス</th>
        <td colspan="3"><%=Html.CheckBox("ApprovalFlag",ViewData["ApprovalFlag"]) %>承認済 <%=Html.CheckBox("PurchaseOrderStatus",ViewData["PurchaseOrderStatus"]) %>発注済 <%=Html.CheckBox("ReservationStatus",ViewData["ReservationStatus"]) %>引当済 <%=Html.CheckBox("StopFlag",ViewData["StopFlag"])%>預り <%=Html.CheckBox("RegistrationStatus",ViewData["RegistrationStatus"]) %>登録済 <%=Html.CheckBox("NoReservation", ViewData["NoReservation"])%>未引当 <%=Html.CheckBox("NoRegistration",ViewData["NoRegistration"]) %>未登録 <%=Html.CheckBox("CancelFlag",ViewData["CancelFlag"]) %>キャンセルも表示</td>
    </tr>
    <tr>
        <th rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = "10",maxlength="3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th rowspan="2">営業担当者</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", size = "15",maxlength="50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')" })%>&nbsp;<img alt="担当者検索" src="/Content/Images/search.jpg" style="cursor:pointer" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
        <td style="height:20px"><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
    </tr>
    <tr>
        <th style="width:100px">管理番号</th>
        <td><%=Html.TextBox("SalesCarNumberFrom", ViewData["SalesCarNumberFrom"], new { @class = "alphanumeric", size = "15" })%>～<%=Html.TextBox("SalesCarNumberTo", ViewData["SalesCarNumberTo"], new { @class = "alphanumeric", size = "15" })%></td>
        <th style="width:100px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumberFrom", ViewData["SlipNumberFrom"], new { @class = "alphanumeric", size = "15" })%>～<%=Html.TextBox("SlipNumberTo", ViewData["SlipNumberTo"], new { @class = "alphanumeric", size = "15" })%></td>
    </tr>
    <tr>
        <th>受注日</th>
        <td><%=Html.TextBox("SalesOrderDateFrom", ViewData["SalesOrderDateFrom"], new { @class = "alphanumeric", size = "10" })%>～<%=Html.TextBox("SalesOrderDateTo", ViewData["SalesOrderDateTo"], new { @class = "alphanumeric", size = "10" })%></td>
        <th>メーカーオーダー番号</th>
        <td><%=Html.TextBox("MakerOrderNumberFrom", ViewData["MakerOrderNumberFrom"], new { @class = "alphanumeric", size = "15" })%>～<%=Html.TextBox("MakerOrderNumberTo", ViewData["MakerOrderNumberTo"], new { @class = "alphanumeric", size = "15" })%></td>
    </tr>
    <tr>
        <th>月内</th>
        <td><%=Html.DropDownList("RegistMonth",(IEnumerable<SelectListItem>)ViewData["RegistMonthList"]) %></td>
    
        <th>登録予定日</th>
        <td><%=Html.TextBox("RegistrationPlanDateFrom", ViewData["RegistrationPlanDateFrom"], new { @class = "alphanumeric", size = "10", maxlength = "10"})%>～
        <%=Html.TextBox("RegistrationPlanDateTo", ViewData["RegistrationPlanDateTo"], new { @class = "alphanumeric", size = "10", maxlength = "10"})%></td>

    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { size = "15" })%></td>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName", ViewData["CarName"], new { size = "15" })%></td>
    </tr>
    <tr>
        <th>モデルコード</th>
        <td><%=Html.TextBox("ModelCode", ViewData["ModelCode"], new { @class = "alphanumeric", size = "15" })%></td>
        <th>型式</th>
        <td><%=Html.TextBox("ModelName", ViewData["ModelName"], new { @class = "alphanumeric", size = "15" })%></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = "20", maxlength = 20 }) %></td>
        <th></th>
        <td></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()"/>--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()"/>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('ApprovalFlag','PurchaseOrderStatus','ReservationStatus','StopFlag','RegistrationStatus','NoReservation','NoRegistration','CancelFlag','DepartmentCode','DepartmentName','EmployeeCode','EmployeeName','SalesCarNumberFrom','SalesCarNumberTo','SlipNumberFrom','SlipNumberTo','SalesOrderDateFrom','SalesOrderDateTo','MakerOrderNumberFrom','MakerOrderNumberTo','RegistMonth','RegistrationPlanDateFrom','RegistrationPlanDateTo','MakerName','CarName','ModelCode','ModelName','Vin'))" />
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新日" style="display:block" width="30" height="30" />
</div>
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarPurchaseOrder/Entry')"/> 
<input type="button" value="チェックした項目を編集" style="width:200px" onclick="openCarPurchaseOrderDialog(<%=Model.Count %>)" />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<%=Html.Hidden("CheckFlag","0") %>
<br />
<br />
<table class="list">
  <tr>
    <th style="width:30px"><%=Html.CheckBox("CheckAll", false, new { onclick = "checkAll("+Model.Count+")" })%></th>
    <th style="width:30px"></th>
    <th style="width:30px"></th>
    <th style="width:30px"></th>
    <th style="text-align:center">承認</th>
    <th style="text-align:center">発注</th>
    <th style="text-align:center">引当</th>
    <th style="text-align:center">預り</th>
    <th style="text-align:center">仕入</th>
    <th style="text-align:center">登録</th>
    <th style="white-space:nowrap">発注依頼番号</th>
    <th style="white-space:nowrap">管理番号</th>
    <th style="white-space:nowrap">オーダー番号</th>
    <th style="white-space:nowrap">伝票番号</th>
    <th style="white-space:nowrap">受注日</th>
    <th style="white-space:nowrap">部門</th>
    <th style="white-space:nowrap">顧客名</th>
    <th style="white-space:nowrap">メーカー</th>
    <th style="white-space:nowrap">モデルコード</th>
    <th style="white-space:nowrap">型式</th>
    <th style="white-space:nowrap">車種</th>
    <th style="white-space:nowrap">グレード</th>
  </tr>

  <%for (int i = 0; i < Model.Count;i++ ) { %>
  <%
      string departmentName = "";
      string customerName = "";
      string makerName = "";
      string modelCode = "";
      string modelName = "";
      string gradeName = "";
      string carName = "";
      if (Model[i].CarSalesHeader != null) {
          try { departmentName = Model[i].CarSalesHeader.Department.DepartmentName; } catch (NullReferenceException) { }
          try { customerName = Model[i].CarSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }
          try { makerName = Model[i].CarSalesHeader.MakerName; } catch (NullReferenceException) { }
          try { modelCode = Model[i].CarSalesHeader.CarGrade.ModelCode; } catch (NullReferenceException) { }
          try { modelName = Model[i].CarSalesHeader.CarGrade.ModelName; } catch (NullReferenceException) { }
          try { carName = Model[i].CarSalesHeader.CarGrade.Car.CarName; } catch (NullReferenceException) { }
          try { gradeName = Model[i].CarSalesHeader.CarGradeName; } catch (NullReferenceException) { }
      } else {
          try { makerName = Model[i].SalesCar.CarGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
          try { modelCode = Model[i].SalesCar.CarGrade.ModelCode; } catch (NullReferenceException) { }
          try { modelName = Model[i].SalesCar.CarGrade.ModelName; } catch (NullReferenceException) { }
          try { carName = Model[i].SalesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
          try { gradeName = Model[i].SalesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
      }
                
   %>  
   <tr>
        <% // Mod 2014/07/11 arc amii chrome対応 チェックボックスにIDを追加 %>
        <td><%=Html.CheckBox(string.Format("check[{0}]", i), new { id = string.Format("check[{0}]", i) })%><%=Html.Hidden(string.Format("CarPurchaseOrderId[{0}]",i),Model[i].CarPurchaseOrderNumber, new { id = string.Format("CarPurchaseOrderId[{0}]",i)}) %></td>
        <td style="white-space:nowrap"><a href="javascript:void(0)" onclick="openModalDialog('/CarPurchaseOrder/Entry2/<%=Model[i].CarPurchaseOrderNumber %>')">発注</a></td>
        <td style="white-space:nowrap"><a href="javascript:void(0)" onclick="openModalDialog('/CarPurchaseOrder/ReservationEntry/<%=Model[i].CarPurchaseOrderNumber %>')">引当</a></td>
        <!--Mod 2016/02/03 arc nakayama #3422_車両発注依頼引当画面の引当処理が出来ない不具合 引当済でないと登録を押せないようにする-->
        <%if (!string.IsNullOrEmpty(Model[i].ReservationStatus) && Model[i].ReservationStatus.Equals("1")){ %>
            <td style="white-space:nowrap"><a href="javascript:void(0)" onclick="openModalDialog('/CarPurchaseOrder/RegistrationEntry/<%=Model[i].CarPurchaseOrderNumber %>')">登録</a></td>
        <%}else{%>
            <td style="white-space:nowrap">登録</td>
        <%} %>
        <td style="text-align:center"><%=Model[i].CarSalesHeader!=null && Model[i].CarSalesHeader.ApprovalFlag!=null && Model[i].CarSalesHeader.ApprovalFlag.Equals("1") ? "済" : "未" %></td>
        <td style="text-align:center"><%=Model[i].PurchaseOrderStatus!=null && Model[i].PurchaseOrderStatus.Equals("1") ? "済" : "未" %></td>
        <td style="text-align:center"><%=Model[i].ReservationStatus!=null && Model[i].ReservationStatus.Equals("1") ? "済" : "未" %></td>
        <td style="text-align:center"><%=Model[i].StopFlag!=null && Model[i].StopFlag.Equals("1") ? "○" : "-" %></td>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td style="text-align:center"><%=Model[i].CarPurchase!=null && Model[i].CarPurchase.Count>0 && CommonUtils.DefaultString(Model[i].CarPurchase[0].PurchaseStatus).Equals("002") ? "済" : Model[i].PurchasePlanStatus!=null && Model[i].PurchasePlanStatus.Equals("1") ? "予定" : "未" %></td>
        <td style="text-align:center"><%=Model[i].RegistrationStatus!=null && Model[i].RegistrationStatus.Equals("1") ? "済" : "未" %></td>
        <td style="white-space:nowrap"><%=Model[i].CarPurchaseOrderNumber %></td>
        <td style="white-space:nowrap"><%=Model[i].SalesCar!=null ? Model[i].SalesCar.SalesCarNumber : ""%></td>
        <td style="white-space:nowrap"><%=Model[i].MakerOrderNumber%></td>
        <td style="white-space:nowrap"><%=Model[i].SlipNumber%></td>
        <td style="white-space:nowrap"><%=Model[i].CarSalesHeader!=null ? CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}", Model[i].CarSalesHeader.SalesOrderDate)) : ""%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(departmentName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(customerName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(makerName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(modelCode)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(modelName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(carName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(gradeName)%></td>
  </tr>
  <%} %>
</table>
<br />
</asp:Content>
