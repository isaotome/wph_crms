<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>

<table class="input-form-slim">
    <tr>
        <th colspan="4" class="input-form-title">
            任意保険
        </th>
    </tr>
    <tr>
        <th style="width: 100px">
            保険会社名
        </th>
        <td colspan="3">
            <%if (Model.InsuranceEnabled) { %>
            <%=Html.TextBox("VoluntaryInsuranceCompanyName", Model.VoluntaryInsuranceCompanyName, new { maxlength = "50",style="width:300px" })%>
            <%}else{ %>
            <%=Html.TextBox("VoluntaryInsuranceCompanyName", Model.VoluntaryInsuranceCompanyName, new { @class = "readonly", @readonly = "readonly", style = "width:300px" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th style="width: 100px">
            ステータス
        </th>
        <td style="width: 100px">
            <%if(Model.InsuranceEnabled){ %>
            <%=Html.DropDownList("VoluntaryInsuranceType", (IEnumerable<SelectListItem>)ViewData["VoluntaryInsuranceTypeList"], new { style = "width:100px;height:20px", onchange = "calcTotalAmount()" })%>
            <%}else{ %>
            <%=Html.DropDownList("VoluntaryInsuranceType", (IEnumerable<SelectListItem>)ViewData["VoluntaryInsuranceTypeList"], new { style = "width:100px;height:20px", @disabled = "disabled" })%>
            <%=Html.Hidden("VoluntaryInsuranceType", Model.VoluntaryInsuranceType) %>
            <%} %>
        </td>
        <th style="width: 100px">
            保険金額（年額）
        </th>
        <td style="width: 100px">
            <%if (Model.InsuranceEnabled) { %>
            <%=Html.TextBox("VoluntaryInsuranceAmount", string.Format("{0:N0}", Model.VoluntaryInsuranceAmount), new { @class = "money", style = "width:94px", maxlength = "10", onkeyup = "calcTotalAmount()" })%>
            <%} else { %>
            <%=Html.TextBox("VoluntaryInsuranceAmount", string.Format("{0:N0}", Model.VoluntaryInsuranceAmount), new { @class = "readonly money", style = "width:94px", maxlength = "10", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
    <tr>
        <th>
            契約開始日
        </th>
        <td>
            <%if (Model.InsuranceEnabled) { %>
            <%=Html.TextBox("VoluntaryInsuranceTermFrom", string.Format("{0:yyyy/MM/dd}", Model.VoluntaryInsuranceTermFrom), new { @class = "alphanumeric", style = "width:94px", maxlength = "10", title = "和暦入力例：H23.12.23" }) %>
            <%} else { %>
            <%=Html.TextBox("VoluntaryInsuranceTermFrom", string.Format("{0:yyyy/MM/dd}", Model.VoluntaryInsuranceTermFrom), new { @class = "readonly alphanumeric", style = "width:94px", @readonly = "readonly" })%>
            <%} %>
        </td>
        <th>
            契約満了日
        </th>
        <td>
            <%if (Model.InsuranceEnabled) { %>
            <%=Html.TextBox("VoluntaryInsuranceTermTo", string.Format("{0:yyyy/MM/dd}", Model.VoluntaryInsuranceTermTo), new { @class = "alphanumeric", style = "width:94px", maxlength = "10", title = "和暦入力例：H23.12.23" })%>
            <%} else { %>
            <%=Html.TextBox("VoluntaryInsuranceTermTo", string.Format("{0:yyyy/MM/dd}", Model.VoluntaryInsuranceTermTo), new { @class = "readonly alphanumeric", style = "width:94px", @readonly = "readonly" })%>
            <%} %>
        </td>
    </tr>
</table>
