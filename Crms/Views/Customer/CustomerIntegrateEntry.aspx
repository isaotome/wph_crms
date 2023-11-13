<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Customer>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	顧客マスタ統合入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("IntegrateEntry", "Customer", FormMethod.Post)){ %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.ValidationSummary() %>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("claimUpdate",ViewData["claimUpdate"]) %>
<%=Html.Hidden("supplierUpdate",ViewData["supplierUpdate"])%>
<%=Html.Hidden("paymentUpdate",ViewData["paymentUpdate"]) %>
<%=Html.Hidden("customerDMUpdate",ViewData["customerDMUpdate"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<table>
    <tr>
        <td>
            <%if(Model.BasicHasErrors){ %>
                <input type="button" value="▼基本情報" onclick="changeDisplayCustomer('Basic');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%}else{ %>
                <input type="button" value="▼基本情報" onclick="changeDisplayCustomer('Basic');" />
            <%} %>
            <%if(Model.CustomerHasErrors){ %>
                <input type="button" value="▼営業情報" onclick="changeDisplayCustomer('Customer');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%}else{ %>
                <input type="button" value="▼営業情報" onclick="changeDisplayCustomer('Customer');" />
            <%} %>
            <%if (Model.CustomerClaimHasErrors) { %>
                <input type="button" value="▼請求先情報" onclick="changeDisplayCustomer('CustomerClaim');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%} else { %>
                <input type="button" value="▼請求先情報" onclick="changeDisplayCustomer('CustomerClaim');" />
            <%} %>
            <%if(Model.SupplierHasErrors){ %>
                <input type="button" value="▼仕入先情報" onclick="changeDisplayCustomer('Supplier');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%}else{ %>
                <input type="button" value="▼仕入先情報" onclick="changeDisplayCustomer('Supplier');" />
            <%} %>
            <%if (Model.SupplierPaymentHasErrors) { %>
                <input type="button" value="▼支払先情報" onclick="changeDisplayCustomer('SupplierPayment');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%} else { %>
                <input type="button" value="▼支払先情報" onclick="changeDisplayCustomer('SupplierPayment');" />
            <%} %>
            <input type="button" value="▼担当者推移" onclick="changeDisplayCustomer('UpdateLog')" />
            <%if (Model.CustomerDMHasErrors) { %>
                <input type="button" value="▼DM発送先" onclick="changeDisplayCustomer('CustomerDM');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%} else { %>
                <input type="button" value="▼DM発送先" onclick="changeDisplayCustomer('CustomerDM');" />
            <%} %>
            </td>
    </tr>
</table>
<br />
<br />
<div id="Basic" style="<%=!(bool)ViewData["CustomerBasicDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("CustomerBasicEntry", Model); %>
</div>
<div id="Customer"style="<%=!(bool)ViewData["CustomerSalesDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("CustomerSalesEntry", Model); %>
</div>
<div id="CustomerClaim" style="<%=!(bool)ViewData["CustomerClaimDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("CustomerClaimEntry", Model.CustomerClaim); %>
</div>
<div id="Supplier"style="<%=!(bool)ViewData["SupplierDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("SupplierEntry", Model.Supplier); %>
</div>
<div id="SupplierPayment" style="<%=!(bool)ViewData["SupplierPaymentDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("SupplierPaymentEntry", Model.SupplierPayment); %>
</div>
<div id="CustomerDM" style="<%=!(bool)ViewData["CustomerDMDisplay"] ? "display:none" : "display:block"%>">
    <%Html.RenderPartial("CustomerDMEntry", Model.CustomerDM); %>
</div>
<div id="UpdateLog" style="display:none">
    <%Html.RenderPartial("UpdateLogDisplay", Model.CustomerUpdateLog); %>
</div>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
