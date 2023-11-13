<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceReceiptionHistory>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	顧客受付履歴
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<br />
<%
    Customer customer = Model.Customer; 
    SalesCar salesCar = Model.SalesCar;
    string carName = "";
    try { carName = salesCar.CarGrade.Car.Brand.CarBrandName + " " + salesCar.CarGrade.Car.CarName + " " + salesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
%>
<table class="input-form">
    <tr>
        <th colspan="4" class="input-form-title">顧客情報</th>
    </tr>
    <tr>
        <th style="width:100px">顧客コード</th>
        <td style="width:200px"><%=Html.Encode(customer.CustomerCode) %></td>
        <th style="width:100px">顧客ランク</th>
        <td style="width:200px"><%=Html.Encode(customer.c_CustomerRank!=null ? customer.c_CustomerRank.Name : "") %></td>
    </tr>
    <tr>
        <th>顧客名</th>
        <td><%=Html.Encode(customer.CustomerName) %></td>
        <th>顧客名(カナ)</th>
        <td><%=Html.Encode(customer.CustomerNameKana)%></td>
    </tr>
    <tr>
        <th>住所</th>
        <td colspan="3"><%=Html.Encode(customer.Prefecture)%>&nbsp;<%=Html.Encode(customer.City)%>&nbsp;<%=Html.Encode(customer.Address1)%>&nbsp;<%=Html.Encode(customer.Address2)%></td>
    </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="4">車両情報</th>
    </tr>
    <tr>
        <th style="width:100px">車種</th>
        <td colspan="3"><%=Html.Encode(carName)%></td>
    </tr>
    <tr>
        <th style="width:100px">型式</th>
        <td style="width:200px"><%=Html.Encode(salesCar!=null ? salesCar.ModelName : "") %></td>
        <th style="width:100px">年式</th>
        <td style="width:200px"><%=Html.Encode(salesCar!=null ? salesCar.ManufacturingYear : "") %></td>
    </tr>
</table>
<br />
<table style="border:solid 1px black ;border-collapse:collapse">
    <tr>
        <th style="width:100px;background-color:Yellow;padding:5px"><div style="font-size:14pt">次回点検日</div></th>
        <td style="width:500px;background-color:Yellow;text-align:center;padding:5px"><div style="font-size:14pt;font-weight:bold"><%=Html.Encode(salesCar!=null ? string.Format("{0:yyyy年MM月dd日}",salesCar.NextInspectionDate) : "") %></div></td>
    </tr>
    <tr>
        <th style="width:100px;background-color:Yellow;padding:5px"><div style="font-size:14pt">次回車検日</div></th>
        <td style="width:500px;background-color:Yellow;text-align:center;padding:5px"><div style="font-size:14pt;font-weight:bold"><%=Html.Encode(salesCar!=null ? string.Format("{0:yyyy年MM月dd日}",salesCar.ExpireDate) : "") %></div></td>
    </tr>
</table>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.ServiceSalesHeader!=null ? Model.ServiceSalesHeader.PageProperty : null); %>
<br />

<table class="list">
    <tr>
        <th>伝票番号</th>
        <th>ステータス</th>
        <th>部門</th>
        <th>入庫日</th>
        <th>納車日</th>
        <th>顧客名</th>
        <th>車種名</th>
        <th>登録番号</th>
        <th>走行距離</th>
        <th>車台番号</th>
        <th>主作業</th>
        <th>電話番号</th>
        <th>住所</th>
    </tr>
    <%foreach (var a in Model.ServiceSalesHeader) { %>
    <tr>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=a.SlipNumber%>');return false;"><%=a.SlipNumber %></a></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.c_ServiceOrderStatus != null ? a.c_ServiceOrderStatus.Name : "")%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(a.Department != null ? a.Department.DepartmentName : "") %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",a.ArrivalPlanDate) %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",a.SalesDate) %></td>
        <td><%=CommonUtils.DefaultNbsp(a.Customer == null ? "" : a.Customer.CustomerName)%></td>
        <td><%=CommonUtils.DefaultNbsp(a.CarName)%></td>
        <td style="white-space:nowrap"><%=a.MorterViecleOfficialCode + " " + a.RegistrationNumberType + " " + a.RegistrationNumberKana + " " + a.RegistrationNumberPlate %></td>
        <td><%=a.Mileage != null ? (a.Mileage + (a.c_MileageUnit != null ? "(" + a.c_MileageUnit.Name + ")" : "")) : ""%></td>
        <td style="white-space:nowrap"><%=a.Vin %></td>
        <td><%foreach(var d in a.ServiceSalesLine){
                  if(!string.IsNullOrEmpty(d.ServiceType) && d.ServiceType.Equals("001") && !string.IsNullOrEmpty(d.ServiceWorkCode)){
                      %><%=CommonUtils.DefaultNbsp(d.ServiceWork!=null ? d.ServiceWork.Name : "")%><br /><%
                  }
              }
            %>
        </td>
        <td><%=a.Customer!=null ? a.Customer.TelNumber : "" %></td>
        <td><%=a.Customer!=null ? Html.Encode(a.Customer.Prefecture + a.Customer.City) : "" %></td>
    </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
