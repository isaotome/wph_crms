<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	販売報告書入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>
<%Html.RenderPartial("MenuControl", Model); %>
<br />
<%=Html.ValidationSummary() %>
<%using (Html.BeginForm("SalesReport", "CarSalesOrder", FormMethod.Post)) { %>
<div style="width:1050px">
    <table class="input-form">
        <tr>
            <th style="width:100px">管理番号</th>
            <td style="width:100px"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.SalesCarNumber : "") %></td>
            <th style="width:100px">車種・グレード</th>
            <td style="width:150px"><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null && Model.SalesCar.CarGrade!=null && Model.SalesCar.CarGrade.Car!=null ? Model.SalesCar.CarGrade.Car.CarName + "　" + Model.SalesCar.CarGrade.CarGradeName : "") %></td>
            <th style="width:80px">仕入先</th>
            <td style="width:150px"><%=CommonUtils.DefaultNbsp(Model.CarPurchase!=null && Model.CarPurchase.Supplier!=null ? Model.CarPurchase.Supplier.SupplierName : "") %></td>
            <th style="width:80px">仕入日</th>
            <td style="width:100px"><%=CommonUtils.DefaultNbsp(Model.CarPurchase!=null ? string.Format("{0:yyyy/MM/dd}",Model.CarPurchase.PurchaseDate) : "")%></td>
            <th style="width:100px">仕入部門</th>
            <td style="width:100px"><%=CommonUtils.DefaultNbsp(Model.CarPurchase!=null && Model.CarPurchase.Department!=null ?Model.CarPurchase.Department.DepartmentName : "")%></td>
        </tr>
        <tr>
            <th>伝票番号</th>
            <td><%=CommonUtils.DefaultNbsp(Model.SlipNumber) %></td>
            <th>車台番号</th>
            <td><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.Vin : "") %></td>
            <th>系統色</th>
            <td><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null && Model.SalesCar.c_ColorCategory!=null ? Model.SalesCar.c_ColorCategory.Name : "") %></td>
            <th>年式</th>
            <td><%=CommonUtils.DefaultNbsp(Model.SalesCar!=null ? Model.SalesCar.ManufacturingYear : "") %></td>
            <th>入庫日</th>
            <td></td>
        </tr>
        <tr>            
            <th>販売部門</th>
            <td><%=CommonUtils.DefaultNbsp(Model.Department!=null ? Model.Department.DepartmentName : "")%></td>
            <th>販売担当</th>
            <td><%=CommonUtils.DefaultNbsp(Model.Employee!=null ? Model.Employee.EmployeeName : "")%></td>
            <th>顧客</th>
            <td><%=CommonUtils.DefaultNbsp(Model.Customer!=null ? Model.Customer.CustomerName : "")%></td>
            <th>受注日</th>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model.SalesOrderDate)) %></td>
            <th>納車日</th>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",Model.SalesDate)) %></td>
        </tr>
    </table>
    <br />
    <br />
    <div style="float:left">
        <%--合計欄--%>
        <%Html.RenderPartial("TotalControl", Model); %>
    </div>
    <div style="float:left;margin-left:10px">
        <%-- 販売報告 --%>
        <table class="input-form">
            <tr>
                <th style="width:15px"><img src="/Content/Images/plus.gif" alt="行追加" style="cursor:pointer" onclick="document.forms[0].action='/CarSalesOrder/AddReportLine';document.forms[0].DelLine.value='-1';formSubmit();" /></th>
                <th style="width:150px">イベントコード</th>
                <th style="width:250px">費目名</th>
                <th style="width:100px">金額</th>
            </tr>
        </table>
        <div style="width:563px;height:270px;overflow-y:scroll">
            <table class="input-form">
                <% // <!--//2014/05/29 vs2012対応 arc yano 各コントロールにid追加 %>
                <%for(int i=0;i<Model.CarSalesReport.Count;i++){
                      string nameprefix = string.Format("report[{0}]." , i);
                      string idprefix = string.Format("report[{0}]_" , i);
                %>
                <%=Html.Hidden( nameprefix + "SlipNumber" , Model.SlipNumber, new { id = idprefix + "SlipNumber" })%>
                <%=Html.Hidden( nameprefix + "RevisionNumber", Model.RevisionNumber, new { id = idprefix + "RevisionNumber"})%>
                    <tr>
                         <% // <!--//2014/07/14 chrome対応 arc yano パラメータをnameからidに変更 %>
                        <td style="width:15px"><img src="/Content/Images/minus.gif" alt="行削除" style="cursor:pointer" onclick="document.forms[0].action='/CarSalesOrder/AddReportLine';document.forms[0].DelLine.value='<%=i %>';formSubmit();" /> <% //2014/08/04 arc yano id追加漏れ対応 %><%=Html.Hidden(string.Format("report[{0}].LineNumber", i), i + 1, new { id = idprefix + "LineNumber" })%></td>
                        <td style="width:150px"><%=Html.TextBox( nameprefix + "CampaignCode", Model.CarSalesReport[i].CampaignCode,new { id = idprefix + "CampaignCode", @class="alphanumeric",size="10",maxlength="20",onchange="GetNameFromCode('report["+i+"]_CampaignCode','report["+i+"]_ReportName','Campaign')"})%>&nbsp;<img alt="イベント検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('report[<%=i %>]_CampaignCode','report[<%=i %>]_ReportName','/Campaign/CriteriaDialog')" /></td>
                        <td style="width:250px"><%=Html.TextBox( nameprefix + "ReportName", Model.CarSalesReport[i].ReportName, new { id = idprefix + "ReportName", size = "20" })%></td>
                        <td style="width:100px"><%=Html.TextBox( nameprefix + "Amount", string.Format("{0:N0}", Model.CarSalesReport[i].Amount), new { id = idprefix + "Amount", size = "10", @class = "money" })%></td>
                    </tr>
                <%} %>
            </table>
        </div>
     </div>
</div>
   
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("DelLine","") %>
<%=Html.Hidden("DelPayLine","") %>
<%=Html.Hidden("PrintReport","") %>
<%=Html.Hidden("CreateEmployeeCode",Model.CreateEmployeeCode) %>
<%=Html.Hidden("CreateDate",Model.CreateDate) %>
<%=Html.Hidden("reportName",ViewData["reportName"]) %>
<%=Html.Hidden("reportParam", Model.SlipNumber + "," + Model.RevisionNumber)%>
<%=Html.Hidden("update",ViewData["update"]) %>
<%=Html.Hidden("SalesOrderStatus",Model.SalesOrderStatus) %>
<%=Html.Hidden("SalesCarNumber",Model.SalesCarNumber) %>
<%=Html.Hidden("SlipNumber",Model.SlipNumber) %>
<%=Html.Hidden("RevisionNumber",Model.RevisionNumber) %>
<%=Html.Hidden("DepartmentCode",Model.DepartmentCode) %>
<%=Html.Hidden("EmployeeCode",Model.EmployeeCode) %>
<%=Html.Hidden("CustomerCode",Model.CustomerCode) %>
<%=Html.Hidden("SalesDate",Model.SalesDate) %>
<%=Html.Hidden("SalesOrderDate",Model.SalesOrderDate) %>
<% // Add 2014/07/23 arc amii 既存バグ対応 販売報告登録ボタンで、Rateとローンプランが設定されていなかった為エラーになっていたのを修正 %>
<%=Html.Hidden("Rate",Model.Rate) %>
<%=Html.Hidden("PaymentPlanType", Model.PaymentPlanType) %>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
