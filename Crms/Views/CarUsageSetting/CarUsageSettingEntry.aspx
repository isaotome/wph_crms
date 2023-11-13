<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.SalesCar>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両ステータス入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarUsageSetting", FormMethod.Post)){ %>
   
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td> 
        <%if (!CommonUtils.DefaultString(ViewData["SaveButtonHidden"]).Equals("001")){ %>
            <td onclick="document.forms[0].action.value = 'save';chkSubmit('CarUsageSetting');"><img src="/Content/Images/apply.png" alt="登録" class="command_btn" />&nbsp;登録</td>
        <%} %>
    </tr>
</table>
<br />
<%=Html.Hidden("reportName", ViewData["reportName"])%>
<%=Html.Hidden("reportParam", ViewData["reportParam"])%>
<%=Html.Hidden("PrintReport", "")%>
<%=Html.Hidden("canSave", 1)%>
<% Model.Prefix = "salesCar."; %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <table class="input-form-slim">
    <tr>
        <th class="input-form-title" colspan="4">
            登録情報
        </th>
    </tr>
    <tr>
        <th style="width: 100px">
            登録種別 *
        </th>
        <td style="width: 80px" colspan ="3">
            <% //Mod 2014/10/30 arc amii 車両ステータス追加対応 %>
            <%if (!CommonUtils.DefaultString(ViewData["SaveButtonHidden"]).Equals("001")){ %>
                <%=Html.DropDownList("CarUsageType", (IEnumerable<SelectListItem>)ViewData["ChangeCarUsageList"])%>
            <%} else { %>
            <%=Html.DropDownList("CarUsageType", (IEnumerable<SelectListItem>)ViewData["ChangeCarUsageList"], new { @disabled = "disabled" })%>
            <%} %>
            
            <%--<%=Html.DropDownList(Model.Prefix + "CarUsageType", (IEnumerable<SelectListItem>)ViewData["CarUsageTypeList"], new { style = "width:100%"})%>--%>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            振替日 *
        </th>
        <td style="width: 80px" colspan ="3">
            <%=Html.TextBox("ChangeDate", ViewData["ChangeDate"], new { @class = "alphanumeric", size = 10, maxlength = 10 })%>
        </td>
    </tr>
    <tr>
        <%//Mod 2015/09/08 arc yano #3249 車両用途変更でロケーション入れないと入庫ロケーションがNULLになる　入庫ロケーションを必須項目に変更 %>
        <th style="width: 100px">
            入庫ロケーション *
        </th>
         <td colspan="3">
            <%=Html.TextBox("PurchaseLocationCode", ViewData["PurchaseLocationCode"], new { style = "width:50px", maxlength = 8, onblur = "GetNameFromCodeForCarUsage('PurchaseLocationCode','PurchaseLocationName','Location')"}) %>
            <%Html.RenderPartial("SearchButtonControl",new string[]{"PurchaseLocationCode", "PurchaseLocationName", "'/Location/CriteriaDialogForCarUsage'","0"});%>
            <%=Html.TextBox("PurchaseLocationName", ViewData["PurchaseLocationName"], new { @class = "readonly", @readonly = "readonly", style = "width:130px" })%>
        </td>
    </tr>
    </table>
    <br />
    <%Html.RenderPartial("CarBasicInformationDisplay", Model); %>
    <br />
    <%Html.RenderPartial("CarInspectionDisplay", Model); %>
    <br />
    <%Html.RenderPartial("CarDetailInformationDisplay", Model); %>
</div>
<%} %>
<br />
</asp:Content>
