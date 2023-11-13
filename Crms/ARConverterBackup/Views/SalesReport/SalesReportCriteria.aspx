<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<Office>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	売上速報(日報)
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<h3><%=string.Format("{0:yyyy/MM/dd}",ViewData["SlipDate"]) %>の集計結果</h3>
<br />
<a href="/SalesReport/Criteria/<%=Html.Encode(string.Format("{0:yyyyMMdd}",DateTime.Parse(ViewData["SlipDate"].ToString()).AddDays(-1)))%>">前日</a>　
<a href="/SalesReport/Criteria/<%=Html.Encode(string.Format("{0:yyyyMMdd}",DateTime.Today))%>">今日</a>　
<a href="/SalesReport/Criteria/<%=Html.Encode(string.Format("{0:yyyyMMdd}",DateTime.Parse(ViewData["SlipDate"].ToString()).AddDays(1)))%>">翌日</a>
<br />
<br />
<table class="list2">
    <tr>
        <th rowspan="2" style="text-align:center">事業所</th>
        <th colspan="6" style="text-align:center">車両</th>
        <th colspan="6" style="text-align:center">サービス</th>
    </tr>
    <tr>
        <th colspan="2" style="text-align:center">見積</th>
        <th colspan="2" style="text-align:center">受注</th>
        <th colspan="2" style="text-align:center">納車</th>
        <th colspan="2" style="text-align:center">見積</th>
        <th colspan="2" style="text-align:center">受注</th>
        <th colspan="2" style="text-align:center">納車</th>
    </tr>
    <%foreach(var a in Model){ %>
    <tr>
        <td><img id="office_<%=a.OfficeCode %>" src="/Content/Images/plus.gif" style="cursor:pointer" onclick="openFolder('depart_<%=a.OfficeCode %>', 'office_<%=a.OfficeCode %>')" alt="" />&nbsp;<%=CommonUtils.DefaultNbsp(a.OfficeName) %></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.CarQuoteList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",a.Department.Sum(x=>x.CarQuoteList.Sum(y=>y.GrandTotalAmount))),10)%></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.CarSalesOrderList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",a.Department.Sum(x=>x.CarSalesOrderList.Sum(y=>y.GrandTotalAmount))),10)%></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.CarSalesList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",a.Department.Sum(x=>x.CarSalesOrderList.Sum(y=>y.GrandTotalAmount))),10)%></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.ServiceQuoteList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", a.Department.Sum(x=>x.ServiceQuoteList.Sum(y=>y.GrandTotalAmount))),10)%></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.ServiceSalesOrderList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", a.Department.Sum(x=>x.ServiceSalesOrderList.Sum(y=>y.GrandTotalAmount))),10)%></td>
        <td style="text-align:right;width:20px"><%=CommonUtils.DefaultNbsp(a.Department.Sum(x=>x.ServiceSalesList.Count()),3) %></td>
        <td style="text-align:right;width:60px"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", a.Department.Sum(x=>x.ServiceSalesList.Sum(y=>y.GrandTotalAmount))),10)%></td>
    </tr>
    <tbody id="depart_<%=a.OfficeCode %>" style="display:none">
    <%foreach(var b in a.Department){ %>
    <tr>
        <td><div style="margin-left:20px"><%=b.DepartmentName %></div></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.CarQuoteList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.CarQuoteList.Sum(x=>x.GrandTotalAmount)!=0){ %><a id="carQuote_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('carQuoteDetail_<%=b.DepartmentCode %>','carQuote_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.CarQuoteList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.CarSalesOrderList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.CarSalesOrderList.Sum(x=>x.GrandTotalAmount)!=0){ %><a id="carSalesOrder_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('carSalesOrderDetail_<%=b.DepartmentCode %>','carSalesOrder_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.CarSalesOrderList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.CarSalesList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.CarSalesList.Sum(x=>x.GrandTotalAmount)!=0){ %><a id="carSales_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('carSalesDetail_<%=b.DepartmentCode %>','carSalesDetail_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.CarSalesList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.ServiceQuoteList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.ServiceQuoteList.Sum(x=>x.GrandTotalAmount)!=0){%><a id="serviceQuote_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('serviceQuoteDetail_<%=b.DepartmentCode %>','serviceQuoteDetail_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.ServiceQuoteList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.ServiceSalesOrderList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.ServiceSalesOrderList.Sum(x=>x.GrandTotalAmount)!=0){ %><a id="serviceSalesOrder_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('serviceSalesOrderDetail_<%=b.DepartmentCode %>','serviceSalesOrderDetail_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.ServiceSalesOrderList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
        <td style="text-align:right"><%=CommonUtils.DefaultNbsp(b.ServiceSalesList.Count(),3) %></td>
        <td style="text-align:right"><%if(b.ServiceSalesList.Sum(x=>x.GrandTotalAmount)!=0){ %><a id="serviceSales_<%=b.DepartmentCode %>" href="javascript:void(0);" onclick="openFolder('serviceSalesDetail_<%=b.DepartmentCode %>','serviceSalesDetail_<%=b.DepartmentCode %>');return false;"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",b.ServiceSalesList.Sum(x=>x.GrandTotalAmount)),10) %></a><%}else{ %>0<%} %></td>
    </tr>
    <tr id="carQuoteDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">車両見積明細</th>
                </tr>
                <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.CarQuoteList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <tr id="carSalesOrderDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">車両受注明細</th>
                </tr>
               <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.CarSalesOrderList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <tr id="carSalesDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">車両納車明細</th>
                </tr>
               <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.CarSalesList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/CarSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <tr id="serviceQuoteDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">サービス見積明細</th>
                </tr>
                <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.ServiceQuoteList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <tr id="serviceSalesOrderDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">サービス受注明細</th>
                </tr>
                <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.ServiceSalesOrderList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <tr id="serviceSalesDetail_<%=b.DepartmentCode %>" style="display:none">
        <td colspan="13">
            <div style="margin:10px 10px 10px 10px">
            <table class="list">
                <tr>
                    <th colspan="5" class="input-form-title">サービス納車明細</th>
                </tr>
                <tr>
                    <th style="width:150px">伝票番号</th>
                    <th>顧客名</th>
                    <th>車種</th>
                    <th>グレード</th>
                    <th style="text-align:right">金額</th>
                </tr>
            <%foreach(var c in b.ServiceSalesList){ %>
                <tr>
                    <td><a href="javascript:void(0);" onclick="openModalDialog('/ServiceSalesOrder/Entry?SlipNo=<%=c.SlipNumber %>')"><%=CommonUtils.DefaultNbsp(c.SlipNumber) %></a></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CustomerName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarName) %></td>
                    <td><%=CommonUtils.DefaultNbsp(c.CarGradeName) %></td>
                    <td style="text-align:right"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",c.GrandTotalAmount)) %></td>
                </tr>
            <%} %>
            </table>
            </div>
        </td>
    </tr>
    <%} %>
    </tbody>
    <%} %>
</table>
</asp:Content>
