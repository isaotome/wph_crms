<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarPurchaseOrder>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	CarPurchaseOrderListEntry
</asp:Content>
<asp:Content ID="Header" ContentPlaceHolderID="HeaderContent" runat="server">
    <script type="text/javascript">
            //背景色設定
            $(function() {
                $('.input-form tr:even').addClass('even');
            });
            
            //ソートの実行
            function SortRecord(sortKey) {
                document.getElementById('action').value = 'sort';
                if (document.getElementById('sortkey').value == sortKey && document.getElementById('desc').value != '1') {
                    document.getElementById('desc').value = '1';
                } else {
                    document.getElementById('desc').value = '0';
                }
                document.getElementById('sortkey').value = sortKey;
                formSubmit();
                return false;
            }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%using(Html.BeginForm()){ %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("action","") %>
<%=Html.Hidden("sortkey",ViewData["sortKey"]) %>
<%=Html.Hidden("desc",ViewData["desc"]) %>
<div style="overflow:auto;height:680px;width:1080px">
<%=Html.ValidationSummary() %>
    <table class="input-form">
      <tr>
        <th><a href="javascript:void(0)" onclick="SortRecord('SlipNumber')" style="text-decoration:underline">伝票番号</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.SalesOrderDate')" style="text-decoration:underline">受注日</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.ApprovalFlag')" style="text-decoration:underline">承認</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('PurchaseOrderStatus')" style="text-decoration:underline">発注</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('ReservationStatus')" style="text-decoration:underline">引当</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('PurchasePlanStatus')" style="text-decoration:underline">仕入予定</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('RegistrationStatus')" style="text-decoration:underline">登録</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('ReceiptAmount')" style="text-decoration:underline">入金額</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.CostTotalAmount')" style="text-decoration:underline">諸費用</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.GrandTotalAmount')" style="text-decoration:underline">受注額</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.DepartmentCode')" style="text-decoration:underline">部門</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.EmployeeCode')" style="text-decoration:underline">営業担当者</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.CustomerCode')" style="text-decoration:underline">顧客名</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.Customer.CustomerType')" style="text-decoration:underline">顧客区分</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.MakerName')" style="text-decoration:underline">メーカー</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.CarName')" style="text-decoration:underline">車種</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.CarGrade.ModelCode')" style="text-decoration:underline">モデルコード</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.ModelName')" style="text-decoration:underline">型式</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.CarGradeName')" style="text-decoration:underline">グレード</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.ExteriorColorName')" style="text-decoration:underline">外装色</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.InteriorColorName')" style="text-decoration:underline">内装色</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('CarSalesHeader.SalesCarNumber')" style="text-decoration:underline">管理番号</a></th>
      	<th>車台番号</th>
      	<th>移動履歴</th>
        <th><a href="javascript:void(0)" onclick="SortRecord('PurchaseOrderDate')" style="text-decoration:underline">発注日</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('MakerOrderNumber')" style="text-decoration:underline">オーダー番号</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('EmployeeCode')" style="text-decoration:underline">仕入担当者</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('ArrangementNumber')" style="text-decoration:underline">整理番号</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('SupplierCode')" style="text-decoration:underline">仕入先</a></th>
        <th>仕入先名</th>
        <th><a href="javascript:void(0)" onclick="SortRecord('SupplierPaymentCode')" style="text-decoration:underline">支払先</a></th>
        <th>支払先名</th>
        <th><a href="javascript:void(0)" onclick="SortRecord('TradeInPurchaseFlag')" style="text-decoration:underline">下車仕入</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('MakerShipmentDate')" style="text-decoration:underline">メーカー出荷日</a></th>
        <th><a href="javascript:void(0)" onclick="SortRecord('MakerShipmentPlanDate')" style="text-decoration:underline">メーカー出荷予定日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('InspectionDate')" style="text-decoration:underline">検査取得日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('ReInspectionDate')" style="text-decoration:underline">再予備検取得日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('InspectionInformation')" style="text-decoration:underline">検査情報</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('IncentiveOfficeCode')" style="text-decoration:underline">インセン対象店</a></th>
      	<th>インセン対象店名</th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('ArrivalLocationCode')" style="text-decoration:underline">入庫ロケーション</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('ArrivalLocationName')" style="text-decoration:underline">入庫ロケーション名</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('ArrivalPlanDate')" style="text-decoration:underline">仕入予定日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistrationArea1')" style="text-decoration:underline">登録エリア１</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistrationArea2')" style="text-decoration:underline">登録エリア２</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistMonth')" style="text-decoration:underline">月内</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistrationPlanMonth')" style="text-decoration:underline">登録予定月</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistrationPlanDate')" style="text-decoration:underline">登録予定日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('RegistrationDate')" style="text-decoration:underline">登録日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('DocumentPurchaseRequestDate')" style="text-decoration:underline">書類購入希望日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('DocumentPurchase.DocumentPurchaseDate')" style="text-decoration:underline">書類購入日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('DocumentReceiptPlanDate')" style="text-decoration:underline">書類到着予定日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('DocumentReceiptDate')" style="text-decoration:underline">書類到着日</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentExpireDate')" style="text-decoration:underline">金利発生</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('Firm')" style="text-decoration:underline">ファーム</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('FirmMargin')" style="text-decoration:underline">ファームマージン</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PayDueDate')" style="text-decoration:underline">支払期限</a></th>
     	<th><a href="javascript:void(0)" onclick="SortRecord('VehiclePrice')" style="text-decoration:underline">車両本体価格</a></th>
       	<th><a href="javascript:void(0)" onclick="SortRecord('DiscountAmount')" style="text-decoration:underline">ディスカウント金額</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('MetallicPrice')" style="text-decoration:underline">メタリック価格</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('OptionPrice')" style="text-decoration:underline">オプション価格</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('Amount')" style="text-decoration:underline">仕入価格</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('StopFlag')" style="text-decoration:underline">預り</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod1')" style="text-decoration:underline">金利日数1</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod2')" style="text-decoration:underline">金利日数2</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod3')" style="text-decoration:underline">金利日数3</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod4')" style="text-decoration:underline">金利日数4</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod5')" style="text-decoration:underline">金利日数5</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentPeriod6')" style="text-decoration:underline">金利日数6</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount1')" style="text-decoration:underline">金利金額1</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount2')" style="text-decoration:underline">金利金額2</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount3')" style="text-decoration:underline">金利金額3</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount4')" style="text-decoration:underline">金利金額4</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount5')" style="text-decoration:underline">金利金額5</a></th>
      	<th><a href="javascript:void(0)" onclick="SortRecord('PaymentAmount6')" style="text-decoration:underline">金利金額6</a></th>
     </tr>
     <%for (int i = 0; i < Model.Count; i++) { %>
     <%
         CarPurchaseOrder order = Model[i];

         bool approvalFlag = false;
         DateTime? salesOrderDate = null;
         string departmentName = "";
         string employeeName = "";
         string customerName = "";
         string customerKind = "";
         string brandName = "";
         string carName = "";
         string modelCode = "";
         string modelName = "";
         string gradeName = "";
         string exteriorColorName = "";
         string interiorColorName = "";
         string salesCarNumber = "";
         decimal? costTotalAmount = null;
         decimal? grandTotalAmount = null;
         string slipNumber = "";
         int? revisionNumber = null;
         if (order.CarSalesHeader != null) {
             try { approvalFlag = order.CarSalesHeader.ApprovalFlag!=null && order.CarSalesHeader.ApprovalFlag.Equals("1"); } catch (NullReferenceException) { }
             try { salesOrderDate = order.CarSalesHeader.SalesOrderDate; } catch (NullReferenceException) { }
             try { departmentName = order.CarSalesHeader.Department.DepartmentName; } catch (NullReferenceException) { }
             try { employeeName = order.CarSalesHeader.Employee.EmployeeName; } catch (NullReferenceException) { }
             try { customerName = order.CarSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }
             try { customerKind = order.CarSalesHeader.Customer.c_CustomerKind.Name; } catch (NullReferenceException) { }
             try { brandName = order.CarSalesHeader.CarBrandName; } catch (NullReferenceException) { }
             try { carName = order.CarSalesHeader.CarName; } catch (NullReferenceException) { }
             try { modelCode = order.CarSalesHeader.CarGrade.ModelCode; } catch (NullReferenceException) { }
             try { modelName = order.CarSalesHeader.ModelName; } catch (NullReferenceException) { }
             try { gradeName = order.CarSalesHeader.CarGrade.CarGradeName; } catch (NullReferenceException) { }
             try { exteriorColorName = order.CarSalesHeader.ExteriorColorName; } catch (NullReferenceException) { }
             try { interiorColorName = order.CarSalesHeader.InteriorColorName; } catch (NullReferenceException) { }
             try { costTotalAmount = order.CarSalesHeader.CostTotalAmount; } catch (NullReferenceException) { }
             try { grandTotalAmount = order.CarSalesHeader.GrandTotalAmount; } catch (NullReferenceException) { }
             try { slipNumber = order.CarSalesHeader.SlipNumber;}catch(NullReferenceException){}
             try { revisionNumber = order.CarSalesHeader.RevisionNumber;}catch(NullReferenceException){}
         } else {
             try { brandName = order.SalesCar.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
             try { carName = order.SalesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
             try { modelCode = order.SalesCar.CarGrade.ModelCode; } catch (NullReferenceException) { }
             try { modelName = order.SalesCar.ModelName; } catch (NullReferenceException) { }
             try { gradeName = order.SalesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
             try { exteriorColorName = order.SalesCar.ExteriorColorName; } catch (NullReferenceException) { }
             try { interiorColorName = order.SalesCar.InteriorColorName; } catch (NullReferenceException) { }
         }
         try { salesCarNumber = order.SalesCar.SalesCarNumber; } catch (NullReferenceException) { }
         bool purchaseOrderStatus = order.PurchaseOrderStatus != null && order.PurchaseOrderStatus.Equals("1");
         bool reservationStatus = order.ReservationStatus != null && order.ReservationStatus.Equals("1");
         bool registrationStatus = order.RegistrationStatus != null && order.RegistrationStatus.Equals("1");
         bool purchasePlanStatus = order.PurchasePlanStatus != null && order.PurchasePlanStatus.Equals("1");
         bool purchaseDataExist = order.CarPurchase != null && order.CarPurchase.Count>0 && !string.IsNullOrEmpty(order.SalesCarNumber);
           
         string namePrefix = string.Format("data[{0}].", i);
         //2014/05/29 vs2012対応 arc yano 各コントロールにid追加
         string idPrefix = string.Format("data[{0}]_", i);
     %>
     <tr>
     <%=Html.Hidden(namePrefix + "CarPurchaseOrderNumber", order.CarPurchaseOrderNumber, new { id = idPrefix + "CarPurchaseOrderNumber" })%>
     <%=Html.Hidden(namePrefix + "SlipNumber", order.SlipNumber, new { id = idPrefix + "SlipNumber"})%>
     <%if(purchaseOrderStatus){ %><%=Html.Hidden(namePrefix+"PurchaseOrderStatus",order.PurchaseOrderStatus, new { id = idPrefix + "PurchaseOrderStatus"}) %><%} %>
     <%if(reservationStatus){ %><%=Html.Hidden(namePrefix + "ReservationStatus", order.ReservationStatus, new { id = idPrefix + "ReservationStatus"})%><%} %>
     <%if(purchasePlanStatus){ %><%=Html.Hidden(namePrefix+"PurchasePlanStatus",order.PurchasePlanStatus, new { id = idPrefix + "PurchasePlanStatus"}) %><%} %>
     <%if(registrationStatus){ %><%=Html.Hidden(namePrefix+"RegistrationStatus",order.RegistrationStatus, new { id = idPrefix + "RegistrationStatus"}) %><%} %>
<!--伝票番号--> <td><a href="javascript:void(0);" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=slipNumber %>&RevNo=<%=revisionNumber %>');return false;"><%=order.SlipNumber %></a></td>
<!--受注日-->   <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",salesOrderDate)) %></td>
<!--承認-->     <td style="text-align:center"><%=!string.IsNullOrEmpty(slipNumber) ? (approvalFlag ? "済" : "未") : ""%></td>
<!--発注-->     <td style="text-align:center"><%if(purchaseOrderStatus){%>済<%}else if(reservationStatus){%>-<%}else{%><%=Html.CheckBox(namePrefix + "PurchaseOrderStatus", new { id = idPrefix + "PurchaseOrderStatus", onclick = "checkPurchaseOrder("+i+")" })%><%} %></td>

<% // 2014/05/09 Edit arc yano vs2012対応 コンパイルエラーがでているため、書式を変更する。%>
<% // <!--引当-->     <td style="text-align:center"><%=!string.IsNullOrEmpty(slipNumber) ? (reservationStatus ? "済" : Html.CheckBox(namePrefix + "ReservationStatus", new { onclick = "checkPurchaseOrder(" + i + ")" })) : ""</td>%>
<% // <!--仕入予定--> <td style="text-align:center"><%=purchaseOrderStatus ? (purchasePlanStatus ? "済" : Html.CheckBox(namePrefix + "PurchasePlanStatus")) : "-"</td> %>
<% // <!--登録-->     <td style="text-align:center"><%=reservationStatus ? (registrationStatus ? "済" : Html.CheckBox(namePrefix + "RegistrationStatus")) : "-"</td> %>
<% // 2014/05/09 %>

<!--引当-->     <td style="text-align:center"><%if(!string.IsNullOrEmpty(slipNumber)){ if(reservationStatus){%>済<%}else{%><%=Html.CheckBox(namePrefix + "ReservationStatus", new { id = idPrefix + "ReservationStatus", onclick = "checkPurchaseOrder(" + i + ")" })%><%} }else{%><%}%></td>
<!--仕入予定--> <td style="text-align:center"><%if(purchaseOrderStatus){ if(purchasePlanStatus){%>済<%}else{%><%=Html.CheckBox(namePrefix + "PurchasePlanStatus", new { id = idPrefix + "PurchasePlanStatus" })%><%} }else{%>-<%}%></td>
<!--登録-->     <td style="text-align:center"><%if(reservationStatus){  if(registrationStatus){%>済<%}else{%><%=Html.CheckBox(namePrefix + "RegistrationStatus", new { id = idPrefix + "RegistrationStatus" })%><%} }else{%>-<%}%></td>
<!--入金額-->   <td><div style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.ReceiptAmount)) %></div></td>
<!--諸費用-->   <td><div style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",costTotalAmount))%></div></td>
<!--受注額-->   <td><div style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",grandTotalAmount)) %></div></td>
<!--部門-->     <td><%=CommonUtils.DefaultNbsp(departmentName) %></td>
<!--営業担当--> <td><%=CommonUtils.DefaultNbsp(employeeName) %></td>
<!--顧客名-->   <td><%=CommonUtils.DefaultNbsp(customerName)%></td>
<!--顧客区分--> <td><%=CommonUtils.DefaultNbsp(customerKind) %></td>
<!--ブランド--> <td><%=CommonUtils.DefaultNbsp(brandName)%></td>
<!--車種-->     <td><%=CommonUtils.DefaultNbsp(carName) %></td>
<!--M/CODE-->   <td><%=CommonUtils.DefaultNbsp(modelCode)%></td>
<!--型式-->     <td><%=CommonUtils.DefaultNbsp(modelName) %></td>
<!--グレード--> <td><%=CommonUtils.DefaultNbsp(gradeName) %></td>
<!--外装色-->   <td><%=CommonUtils.DefaultNbsp(exteriorColorName) %></td>
<!--内装色-->   <td><%=CommonUtils.DefaultNbsp(interiorColorName) %></td>
<!--管理番号--> <td><%if(reservationStatus){ %>
                        <%=Html.TextBox(namePrefix+"SalesCarNumber",salesCarNumber,new {id = idPrefix+"SalesCarNumber",size="15",@readonly="readonly"})%>
                    <%}else{ %>
                        <% //Mod 2014/07/15 arc yano chrome対応 openSearchDialogのパラメータをname→idに %>
                        <%=Html.TextBox(namePrefix + "SalesCarNumber", salesCarNumber, new { id = idPrefix + "SalesCarNumber", @class = "alphanumeric", size = "15", maxlength = "20", onblur = "GetSalesCar(" + i + ")" })%>&nbsp;<img alt="車両検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('data[<%=i %>]_SalesCarNumber','data[<%=i %>]_Vin','/SalesCar/CriteriaDialog');" />
                    <%} %>
                </td>
                 <% //Mod 2014/07/22 arc yano chrome対応 スクリプトのパラメータをname→idに修正%>
<!--車台番号--> <td><span id="<%=idPrefix%>Vin"><%=CommonUtils.DefaultNbsp(order.SalesCar!=null ? order.SalesCar.Vin : "",20) %></span></td>
<!--移動履歴--> <td style="text-align:center"><a href="javascript:void(0)" style="text-decoration:underline" onclick="if(document.getElementById('<%=idPrefix %>SalesCarNumber').value!=''){openModalDialog('/CarTransfer/CriteriaDialog/'+document.getElementById('<%=idPrefix %>SalesCarNumber').value);}else{alert('管理番号が指定されていません');}">参照</a></td>
<!--発注日-->   <td><%if(purchaseOrderStatus){ %>
                        <%=Html.TextBox(namePrefix + "PurchaseOrderDate", string.Format("{0:yyyy/MM/dd}", order.PurchaseOrderDate), new { id = idPrefix + "PurchaseOrderDate", @class = "alphanumeric", size = "10",maxlength="10",@readonly="readonly" })%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "PurchaseOrderDate", string.Format("{0:yyyy/MM/dd}", order.PurchaseOrderDate), new { id = idPrefix + "PurchaseOrderDate", @class = "alphanumeric", size = "10",maxlength="10" })%>
                    <%} %>
                </td>
<!--OrderNo-->  <td><%=Html.TextBox(namePrefix + "MakerOrderNumber",order.MakerOrderNumber,new { id = idPrefix + "MakerOrderNumber", @class="alphanumeric",size="15",maxlength="20"}) %></td>
<!--仕入担当-->  <td><%if(purchaseOrderStatus){ %>
                        <%=Html.TextBox(namePrefix + "EmployeeNumber", order.Employee != null ? order.Employee.EmployeeNumber : "", new { id = idPrefix + "EmployeeNumber", @class = "alphanumeric readonly", style = "width:50px", maxlength = "20", @readonly = "readonly" })%>
                        <%=Html.TextBox(namePrefix + "EmployeeCode", order.EmployeeCode,new { id = idPrefix + "EmployeeCode", size="15",maxlength="50",@readonly="readonly"}) %>
                     <%}else{ %>
                        <%=Html.TextBox(namePrefix + "EmployeeNumber", order.Employee != null ? order.Employee.EmployeeNumber : "", new { id = idPrefix + "EmployeeNumber", @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('"+idPrefix+"EmployeeNumber',new Array('"+ idPrefix+"EmployeeCode','"+idPrefix+"EmployeeName'),'Employee')" })%>
                        <%=Html.TextBox(namePrefix + "EmployeeCode", order.EmployeeCode, new { id = idPrefix + "EmployeeCode", @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('" + idPrefix + "EmployeeCode',new Array('" + idPrefix + "EmployeeNumber','" + idPrefix + "EmployeeName'),'Employee')" })%>
                        <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "EmployeeCode", idPrefix + "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
                        <%=Html.TextBox(namePrefix + "EmployeeName", order.Employee!=null ? order.Employee.EmployeeName : "", new { id = idPrefix + "EmployeeName", @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
                     <%} %>
                 </td>
<!--整理番号--> <td><%=Html.TextBox(namePrefix + "ArrangementNumber",order.ArrangementNumber,new { id = idPrefix + "ArrangementNumber", @class="alphanumeric",size="15",maxlength="20"}) %></td>
<!--仕入先-->   <td><%if(purchaseOrderStatus){ %>
                        <%=Html.TextBox(namePrefix + "SupplierCode",order.SupplierCode,new { id = idPrefix + "SupplierCode", @class="alphanumeric",size="10",maxlength="10",@readonly="readonly"})%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "SupplierCode",order.SupplierCode,new { id = idPrefix + "SupplierCode", @class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('data["+i+"]_SupplierCode','data["+i+"]_SupplierName','Supplier')"}) %>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('data[<%=i %>]_SupplierCode','data[<%=i %>]_SupplierName','/Supplier/CriteriaDialog')" />
                    <%} %>
                </td>
<!--仕入先名--> <td><span id="<%=idPrefix%>SupplierName"><%=order.Supplier!=null ? order.Supplier.SupplierName : "" %></span></td>
<!--支払先-->   <td><%if(purchaseOrderStatus){ %>
                        <%=Html.TextBox(namePrefix + "SupplierPaymentCode", order.SupplierPaymentCode, new { id = idPrefix + "SupplierPaymentCode", @class = "alphanumeric", size = "10",@readonlye="readonly" })%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "SupplierPaymentCode", order.SupplierPaymentCode, new { id = idPrefix + "SupplierPaymentCode", @class = "alphanumeric", size = "10",maxlength="10",onblur="GetNameFromCode('data["+i+"]_SupplierPaymentCode','data["+i+"]_SupplierPaymentName','SupplierPayment')" })%>&nbsp;<img alt="支払先" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('data[<%=i %>]_SupplierPaymentCode','data[<%=i %>]_SupplierPaymentName','/SupplierPayment/CriteriaDialog')" />
                    <%} %>
                </td>
<!--支払先名--> <td><span id="<%=idPrefix%>SupplierPaymentName"><%=order.SupplierPayment!=null ? order.SupplierPayment.SupplierPaymentName : "" %></span></td>
<!--下車仕入--> <td><%=!string.IsNullOrEmpty(slipNumber) ? (CommonUtils.DefaultNbsp(order.TradeInPurchaseFlag ? "済" : "未")) : "" %></td>
<!--出荷日-->   <td><%=Html.TextBox(namePrefix + "MakerShipmentDate", string.Format("{0:yyyy/MM/dd}",order.MakerShipmentDate), new { id = idPrefix + "MakerShipmentDate",  @class = "alphanumeric", size = "10", maxlength = "10" })%></td>
<!--出荷予定日--><td><%=Html.TextBox(namePrefix + "MakerShipmentPlanDate", string.Format("{0:yyyy/MM/dd}",order.MakerShipmentPlanDate),new { id = idPrefix + "MakerShipmentPlanDate", @class="alphanumeric",size="10",maxlength="10"}) %></td>
<!--検査取得--> <td><%=Html.TextBox(namePrefix + "InspectionDate", string.Format("{0:yyyy/MM/dd}", order.InspectionDate), new { id = idPrefix + "InspectionDate", @class = "alphanumeric", size = "10", maxlength = "10" })%></td>
<!--再予備検取得日--><td><%=Html.TextBox(namePrefix + "ReInspectionDate",string.Format("{0:yyyy/MM/dd}",order.ReInspectionDate),new { id = idPrefix + "ReInspectionDate", @class="alphanumeric",size="10",maxlength="10"}) %></td>
<!--検査情報--> <td><%=Html.TextBox(namePrefix + "InspectionInformation",order.InspectionInformation,new { id = idPrefix + "InspectionInformation", size="20",maxlength="50"}) %></td>
<!--インセン--> <td><%=Html.TextBox(namePrefix + "IncentiveOfficeCode",order.IncentiveOfficeCode,new { id = idPrefix + "IncentiveOfficeCode", @class = "alphanumeric", size = "10", maxlength = "3",onchange="GetNameFromCode('data["+i+"]_IncentiveOfficeCode','data["+i+"]_IncentiveOfficeName','Office')" }) %>&nbsp;<img alt="事業所検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('data[<%=i%>]_IncentiveOfficeCode','data[<%=i %>]_IncentiveOfficeName','/Office/CriteriaDialog')" /></td>
<!--インセン名--><td><span id="<%=idPrefix%>IncentiveOfficeName"><%=CommonUtils.DefaultNbsp(order.Office!=null ? order.Office.OfficeName : "") %></span></td>
<!--入庫先-->   <td><%if(purchasePlanStatus){ %>
                        <%=Html.TextBox(namePrefix + "ArrivalLocationCode", order.ArrivalLocationCode, new { id = idPrefix + "ArrivalLocationCode", @class = "alphanumeric", size = "10", maxlength = "12",@readonly="readonly"})%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "ArrivalLocationCode", order.ArrivalLocationCode, new { id = idPrefix + "ArrivalLocationCode", @class = "alphanumeric", size = "10", maxlength = "12",onblur="GetNameFromCode('data["+i+"]_ArrivalLocationCode','data["+i+"]_ArrivalLocationName','Location')" })%>&nbsp;<img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('data[<%=i %>]_ArrivalLocationCode','data[<%=i %>]_ArrivalLocationName','/Location/CriteriaDialog?BusinessType=001,009')" />
                    <%} %>
                </td>
<!--入庫先名--> <td><span id="<%=idPrefix%>ArrivalLocationName"><%=order.Location!=null ? order.Location.LocationName : "" %></span></td>
<!--入庫予定--> <td><%if(purchasePlanStatus){ %>
                        <%=Html.TextBox(namePrefix + "ArrivalPlanDate",string.Format("{0:yyyy/MM/dd}",order.ArrivalPlanDate), new { id = idPrefix + "ArrivalPlanDate", @class = "alphanumeric", size = "10",maxlength="10",@readonly="readonly" }) %>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "ArrivalPlanDate",string.Format("{0:yyyy/MM/dd}",order.ArrivalPlanDate), new { id = idPrefix + "ArrivalPlanDate", @class = "alphanumeric", size = "10",maxlength="10" }) %>
                    <%} %>
                </td>
<!--登録Area1--><td><%=CommonUtils.DefaultNbsp(order.CarSalesHeader!=null && order.CarSalesHeader.User!=null ? order.CarSalesHeader.User.Prefecture : "")%></td>
<!--登録Area2--><td><%=CommonUtils.DefaultNbsp(order.CarSalesHeader!=null && order.CarSalesHeader.User!=null ? order.CarSalesHeader.User.City : "")%></td>
<!--月内-->    <td><%=Html.DropDownList(namePrefix + "RegistMonth",(IEnumerable<SelectListItem>)order.RegistMonthList, new { id = idPrefix + "RegistMonth" } )%></td>
<!--登録予定月--><td><%if(registrationStatus){ %>
                        <%=Html.TextBox(namePrefix + "RegistrationPlanMonth",order.RegistrationPlanMonth,new { id = idPrefix + "RegistrationPlanMonth", size="15",@readonly="readonly"}) %>
                     <%}else{ %>
                        <%=Html.TextBox(namePrefix + "RegistrationPlanMonth",order.RegistrationPlanMonth,new { id = idPrefix + "RegistrationPlanMonth", size="15",maxlength="10"}) %>
                     <%} %>
                 </td>
<!--登録予定日--><td><%if(registrationStatus){ %>
                        <%=Html.TextBox(namePrefix + "RegistrationPlanDate",string.Format("{0:yyyy/MM/dd}",order.RegistrationPlanDate), new { id = idPrefix + "RegistrationPlanDate", @class = "alphanumeric", size = "10",maxlength="10",@readonly="readonly" }) %>
                     <%}else{ %>
                        <%=Html.TextBox(namePrefix + "RegistrationPlanDate",string.Format("{0:yyyy/MM/dd}",order.RegistrationPlanDate), new { id = idPrefix + "RegistrationPlanDate", @class = "alphanumeric", size = "10",maxlength="10" }) %>
                     <%} %>
                </td>
<!--登録日-->   <td><%if (registrationStatus) { %>
                        <%=Html.TextBox(namePrefix + "RegistrationDate", string.Format("{0:yyyy/MM/dd}", order.RegistrationDate), new { id = idPrefix + "RegistrationDate", @class = "alphanumeric", size = "10", maxlength = "10", @readonly = "readonly" })%>
                    <%} else { %>
                        <%=Html.TextBox(namePrefix + "RegistrationDate", string.Format("{0:yyyy/MM/dd}", order.RegistrationDate), new { id = idPrefix + "RegistrationDate", @class = "alphanumeric", size = "10", maxlength = "10" })%>
                    <%} %>
                </td>
<!--書類購入希望日--><td><%=Html.TextBox(namePrefix + "DocumentPurchaseRequestDate",string.Format("{0:yyyy/MM/dd}",order.DocumentPurchaseRequestDate) , new { id = idPrefix + "DocumentPurchaseRequestDate", @class = "alphanumeric", size = "10",maxlength="10" })%></td>
<!--書類購入日--><td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",order.DocumentPurchaseDate)) %></td>
<!--書類予定--> <td><%=Html.TextBox(namePrefix + "DocumentReceiptPlanDate",string.Format("{0:yyyy/MM/dd}",order.DocumentReceiptDate), new { id = idPrefix + "DocumentReceiptPlanDate", @class = "alphanumeric", size = "10",maxlength="10" }) %></td>
<!--書類到着--> <td><%=Html.TextBox(namePrefix + "DocumentReceiptDate",string.Format("{0:yyyy/MM/dd}",order.DocumentReceiptDate), new { id = idPrefix + "DocumentReceiptDate", @class = "alphanumeric", size = "10",maxlength="10" }) %></td>
<!--金利発生--> <td style="text-align:right"><%=order.PaymentExpiredate==null ? "未出荷" : order.PaymentExpiredate < 0 ? "<font color=red>"+order.PaymentExpiredate.ToString()+"</font>" : order.PaymentExpiredate.ToString() %></td>
<!--ファーム--> <td><%=Html.DropDownList(namePrefix + "Firm",(IEnumerable<SelectListItem>)order.FirmList, new { id = idPrefix + "Firm"}) %></td>
<!--ファームマージン--><td><%=Html.TextBox(namePrefix + "FirmMargin",order.FirmMargin,new { id = idPrefix + "FirmMargin", @class="numeric",size="10",maxlength="10"}) %></td>
<!--支払期限--> <td><%if (purchaseOrderStatus) { %>
                        <%=Html.TextBox(namePrefix + "PayDueDate", string.Format("{0:yyyy/MM/dd}", order.PayDueDate), new { id = idPrefix + "PayDueDate", @class = "alphanumeric", size = "10", maxlength = "10" ,@readonly="readonly"})%>
                    <%}else{ %>
                        <%=Html.TextBox(namePrefix + "PayDueDate", string.Format("{0:yyyy/MM/dd}", order.PayDueDate), new { id = idPrefix + "PayDueDate", @class = "alphanumeric", size = "10", maxlength = "10" })%>
                    <%} %>
                </td>
<!--車両価格--> <td><%=Html.TextBox(namePrefix + "VehiclePrice", order.VehiclePrice, new { id = idPrefix + "VehiclePrice", @class = "numeric", size = "8", maxlength = "10" })%></td>
<!--値引額-->   <td><%=Html.TextBox(namePrefix + "DiscountAmount",  order.DiscountAmount, new { id = idPrefix + "DiscountAmount", @class = "numeric", size = "8", maxlength = "10" })%></td>
<!--メタリック価格--><td><%=Html.TextBox(namePrefix + "MetallicPrice", order.MetallicPrice, new { id = idPrefix + "MetallicPrice", @class = "numeric", size = "8", maxlength = "10" })%></td>
<!--オプション価格--><td><%=Html.TextBox(namePrefix + "OptionPrice", order.OptionPrice, new { id = idPrefix + "OptionPrice", @class = "numeric", size = "8", maxlength = "10" })%></td>
<!--原価-->     <td><%=Html.TextBox(namePrefix + "Amount", order.Amount, new { id = idPrefix + "Amount", @class = "numeric", size = "8", maxlength = "10" })%></td>
<!--預り-->     <td><%=Html.CheckBox(namePrefix + "StopFlag", order.StopFlag!=null && order.StopFlag.Equals("001"), new { id = idPrefix + "StopFlag"}) %></td>
<!--金利日数1--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod1) %></td>
<!--金利日数2--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod2) %></td>
<!--金利日数3--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod3) %></td>
<!--金利日数4--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod4) %></td>
<!--金利日数5--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod5) %></td>
<!--金利日数6--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(order.PaymentPeriod6) %></td>
<!--金利金額1--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount1 ?? 0)) %></td>
<!--金利金額2--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount2 ?? 0)) %></td>
<!--金利金額3--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount3 ?? 0)) %></td>
<!--金利金額4--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount4 ?? 0)) %></td>
<!--金利金額5--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount5 ?? 0)) %></td>
<!--金利金額6--><td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",order.PaymentAmount6 ?? 0)) %></td>

      </tr>
      <%} %>
</table>
</div>
<%} %>

</asp:Content>
