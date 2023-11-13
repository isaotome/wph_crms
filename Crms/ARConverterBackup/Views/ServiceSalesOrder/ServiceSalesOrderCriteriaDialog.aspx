<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceSalesHeader>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス伝票履歴
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog","ServiceSalesOrder",FormMethod.Post)){%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("CustomerCode",ViewData["CustomerCode"]) %>
<%=Html.Hidden("VinFull",ViewData["VinFull"]) %>
<%=Html.Hidden("DelFlag",ViewData["DelFlag"]) %>
<%} %>
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
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
    <%foreach (var a in Model) { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=a.SlipNumber%>');return false;"><%=a.SlipNumber %></a></td>
        <td><%=CommonUtils.DefaultNbsp(a.c_ServiceOrderStatus != null ? a.c_ServiceOrderStatus.Name : "")%></td>
        <td><%=CommonUtils.DefaultNbsp(a.Department != null ? a.Department.DepartmentName : "") %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",a.ArrivalPlanDate) %></td>
        <td><%=string.Format("{0:yyyy/MM/dd}",a.SalesDate) %></td>
        <td><%=CommonUtils.DefaultNbsp(a.Customer == null ? "" : a.Customer.CustomerName)%></td>
        <td><%=CommonUtils.DefaultNbsp(a.CarName)%></td>
        <td><%=a.MorterViecleOfficialCode + " " + a.RegistrationNumberType + " " + a.RegistrationNumberKana + " " + a.RegistrationNumberPlate %></td>
        <td><%=a.Mileage != null ? (a.Mileage + (a.c_MileageUnit != null ? "(" + a.c_MileageUnit.Name + ")" : "")) : ""%></td>
        <td><%=a.Vin %></td>
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
