<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceRequest>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	作業依頼詳細
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <td onclick="openModalDialog('/Report/Print?reportName=ServiceRequest&reportParam=<%=Model.OriginalSlipNumber %>');"><img src="/Content/Images/pdf.png" alt="車両作業依頼書" class="command_btn" />&nbsp;車両作業依頼書</td>
    </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<%using (Html.BeginForm("Result", "ServiceRequest", FormMethod.Post))
  { %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%
    CarSalesHeader header = Model.CarSalesHeader;
    CarPurchaseOrder order = Model.CarPurchaseOrder;
    string newUsedType = "";
    string salesType = "";
    string carGradeCode = "";
    string makerName = "";
    string carBrandName = "";
    string carName = "";
    string carGradeName = "";
    string exteriorColorName = "";
    string interiorColorName = "";
    string vin = "";
    string modelName = "";
    string mileage = "";
    string customerCode = "";
    string customerName = "";
    string customerNameKana = "";
    string customerAddress = "";
    decimal totalAmount = 0m;
    try { newUsedType = header.c_NewUsedType.Name; } catch (NullReferenceException) { }
    try { salesType = header.c_SalesType.Name; } catch (NullReferenceException) { }
    try { carGradeCode = header.CarGradeCode; } catch (NullReferenceException) { }
    try { makerName = header.MakerName; } catch (NullReferenceException) { }
    try { carBrandName = header.CarBrandName; } catch (NullReferenceException) { }
    try { carName = header.CarName; } catch (NullReferenceException) { }
    try { carGradeName = header.CarGradeName; } catch (NullReferenceException) { }
    try { exteriorColorName = header.ExteriorColorName; } catch (NullReferenceException) { }
    try { interiorColorName = header.InteriorColorName; } catch (NullReferenceException) { }
    try { vin = header.Vin; } catch (NullReferenceException) { }
    try { modelName = header.ModelName; } catch (NullReferenceException) { }
    try { mileage = string.Format("{0:0.00}", header.Mileage) + " " + header.c_MileageUnit.Name; } catch (NullReferenceException) { }
    try { customerCode = header.CustomerCode; } catch (NullReferenceException) { }
    try { customerName = header.Customer.CustomerName; } catch (NullReferenceException) { }
    try { customerNameKana = header.Customer.CustomerNameKana; } catch (NullReferenceException) { }
    try { customerAddress = header.Customer.Prefecture + header.Customer.City + header.Customer.Address1 + header.Customer.Address2; } catch (NullReferenceException) { }
    try { totalAmount = (header.ShopOptionAmount ?? 0m) + (header.MakerOptionAmount ?? 0m) + (header.OutSourceAmount ?? 0m); } catch (NullReferenceException) { }   
%>
<div>
<br />
    <table class="input-form">
        <tr>
            <th class="input-form-title" colspan="2">依頼情報</th>
        </tr>
        <tr>
            <th rowspan="2">依頼先部門</th>
            <td><%=CommonUtils.DefaultNbsp(Model.DepartmentCode)%></td>
        </tr>
        <tr>
            <td style="height:20px"><%=CommonUtils.DefaultNbsp(Model.Department!=null ? Model.Department.DepartmentName : "") %></td>
        </tr>
        <tr>
            <th style="width:100px;height:20px">備考</th>
            <td style="width:230px;height:20px"><%=CommonUtils.DefaultNbsp(Model.Memo)%></td>
        </tr>
    </table>
</div>
<br />
<table class="input-form">
    <tr>
        <th colspan="6" class="input-form-title">販売車両情報</th>
    </tr>
    <tr>
        <th style="width:80px;height:20px">新中区分</th>
        <td style="width:200px;height:20px"><%=CommonUtils.DefaultNbsp(newUsedType)%></td>
        <th style="width:80px;height:20px">販売区分</th>
        <td style="width:200px;height:20px"><%=CommonUtils.DefaultNbsp(salesType)%></td>
        <th style="width:80px;height:20px">グレードコード</th>
        <td style="width:200px;height:20px"><%=CommonUtils.DefaultNbsp(carGradeCode) %></td>
    </tr>
    <tr>
        <th style="height:20px">メーカー名</th>
        <td><%=CommonUtils.DefaultNbsp(makerName) %></td>
        <th style="height:20px">ブランド名</th>
        <td><%=CommonUtils.DefaultNbsp(carBrandName) %></td>
        <th style="height:20px">車種名</th>
        <td><%=CommonUtils.DefaultNbsp(carName) %></td>
    </tr>
    <tr>
        <th style="height:20px">グレード名</th>
        <td><%=CommonUtils.DefaultNbsp(carGradeName) %></td>
        <th style="height:20px">外装色</th>
        <td><%=CommonUtils.DefaultNbsp(exteriorColorName)%></td>
        <th style="height:20px">内装色</th>
        <td><%=CommonUtils.DefaultNbsp(interiorColorName)%></td>
    </tr>
    <tr>
        <th style="height:20px">車台番号</th>
        <td><%=CommonUtils.DefaultNbsp(vin) %></td>
        <th style="height:20px">型式</th>
        <td><%=CommonUtils.DefaultNbsp(modelName) %></td>
        <th style="height:20px">走行距離</th>
        <td><%=CommonUtils.DefaultNbsp(mileage) %></td>
    </tr>
</table>
<div>
    <br />
    <table class="input-form">
        <tr>
            <th colspan="4" class="input-form-title">顧客情報</th>
        </tr>
        <tr>
            <th style="width:100px;height:20px">顧客コード</th>
            <td colspan="3" style="height:20px"><%=CommonUtils.DefaultNbsp(customerCode) %></td>
        </tr>
        <tr>
            <th style="width:100px;height:20px">顧客名</th>
            <td style="width:150px;height:20px"><%=CommonUtils.DefaultNbsp(customerName)%></td>
            <th style="width:100px;height:20px">顧客名(カナ)</th>
            <td style="width:150px;height:20px"><%=CommonUtils.DefaultNbsp(customerNameKana)%></td>
        </tr>
        <tr>
            <th style="height:20px">住所</th>
            <td style="height:20px" colspan="3"><%=CommonUtils.DefaultNbsp(customerAddress) %></td>
        </tr>

    </table>
    <br />
</div>
<div>
<table class="input-form">
    <tr>
        <th colspan="6" class="input-form-title">発注車両情報</th>
    </tr>
    <tr>
        <th style="width:80px;height:20px">現在値</th>
        <td style="width:150px;height:20px"></td>
        <th style="width:80px;height:20px">出荷予定日</th>
        <td style="width:150px;height:20px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",order!=null ? order.MakerShipmentDate : null)) %></td>
        <th style="width:80px;height:20px">到着予定日</th>
        <td style="width:150px;height:20px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",order!=null ? order.ArrivalPlanDate : null)) %></td>
    </tr>
    <tr>
        <th style="height:20px">登録予定日</th>
        <td style="height:20px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",order!=null ?order.RegistrationPlanDate : null)) %></td>
        <th style="height:20px">納車予定日</th>
        <td style="height:20px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",header!=null ? header.SalesPlanDate : null)) %></td>
        <th style="height:20px">区分</th>
        <td><%=Model.c_OwnershipChange.Name %></td>
    </tr>
    <tr>
        <th style="height:20px">12ヶ月点検</th>
        <td style="height:20px"><%=CommonUtils.DefaultNbsp(Model.c_AnnualInspection!=null ? Model.c_AnnualInspection.Name : "") %></td>
        <th style="height:20px">保証継承</th>
        <td style="height:20px"><%=CommonUtils.DefaultNbsp(Model.c_InsuranceInheritance!=null ? Model.c_InsuranceInheritance.Name : "") %></td>
        <th style="height:20px">希望納期</th>
        <td style="height:20px"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model.DeliveryRequirement)) %></td>
    </tr>
    </table>
<br />
<table class="input-form">
    <tr>
        <th colspan="6" class="input-form-title">オプション</th>
    </tr>
    <tr>
        <th style="width:100px"><div style="text-align:center">区分</div></th>
        <th style="width:170px"><div style="text-align:center">品番</div></th>
        <th style="width:250px"><div style="text-align:center">品名</div></th>
        <th style="width:63px"><div style="text-align:center">金額</div></th>
        <th style="width:200px"><div style="text-align:center">コメント</div></th>
        <th style="width:50px"><div style="text-align:center">有償</div></th>
    </tr>
    </table>
    <div style="overflow-y:scroll;width:893px;height:300px">
    <table class="input-form">
    <%foreach(ServiceRequestLine line in Model.ServiceRequestLine){ %>
        <tr>
            <td style="width:100px;height:20px"><%=CommonUtils.DefaultNbsp(line.c_OptionType.Name) %></td>
            <td style="width:170px;height:20px"><%=CommonUtils.DefaultNbsp(line.CarOptionCode) %></td>
            <td style="width:250px;height:20px"><%=CommonUtils.DefaultNbsp(line.CarOptionName) %></td>
            <td style="width:63px;height:20px;text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.Amount)) %></td>
            <td style="width:200px;height:20px"><%=CommonUtils.DefaultNbsp(line.RequestComment) %></td>
            <td style="width:50px;height:20px"><%=CommonUtils.DefaultNbsp(line.ClaimType==true ? "有償" : "") %></td>
        </tr>
    <%} %>
    </table>
    </div>
    <table class="input-form">
        <tr>
            <th style="width:535px">合計</th>
            <th style="width:63px;text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",totalAmount)) %></th>
            <th style="width:200px"></th>
            <th style="width:50px"></th>
        </tr>               
     </table>
</div>
<%} %>

</asp:Content>
