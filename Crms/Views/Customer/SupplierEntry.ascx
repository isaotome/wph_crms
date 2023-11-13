<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.Supplier>" %>
<%@ Import Namespace="CrmsDao" %> 
<table class="input-form" style="width: 700px">
    <tr>
        <th colspan="4" class="input-form-title">
            仕入先情報
        </th>
    </tr>
    <tr>
        <th style="width: 150px">
        </th>
        <td>
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
            <%=Html.CheckBox("SupplierEnabled", CommonUtils.DefaultString(Model.DelFlag).Equals("0"), new { onclick = "document.forms[0].action='/Customer/SupplierEnabled';formSubmit()" })%>仕入先として登録する    
        </td>
    </tr>
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if(CommonUtils.DefaultString(Model.DelFlag).Equals("0")){ %>
    <tr>
        <th>
            仕入先名 *
        </th>
        <td>
            <%=Html.TextBox("sup.SupplierName", Model.SupplierName, new { size = 50, maxlength = 50 })%>
        </td>
    </tr>
    <tr>
        <th>
            外注フラグ *
        </th>
        <td>
            <%=Html.DropDownList("sup.OutsourceFlag", (IEnumerable<SelectListItem>)ViewData["OutsourceFlagList"])%>
        </td>
    </tr>
    <tr>
        <th>
            郵便番号
        </th>
        <td>
            <%=Html.TextBox("sup.PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%>
            <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode('sup_')" /><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
        </td>
    </tr>
    <tr>
        <th>
            都道府県
        </th>
        <td>
            <%=Html.TextBox("sup.Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%>
        </td>
    </tr>
    <tr>
        <th>
            市区町村
        </th>
        <td>
            <%=Html.TextBox("sup.City", Model.City, new { size = 50, maxlength = 50 })%>
        </td>
    </tr>
    <tr>
        <th>
            住所1
        </th>
        <td>
            <%=Html.TextBox("sup.Address1", Model.Address1, new { size = 50, maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th>
            住所2
        </th>
        <td>
            <%=Html.TextBox("sup.Address2", Model.Address2, new { size = 50, maxlength = 100 })%>
        </td>
    </tr>
    <tr>
        <th>
            電話番号1
        </th>
        <td>
            <%=Html.TextBox("sup.TelNumber1", Model.TelNumber1, new { @class = "alphanumeric", maxlength = 15 })%>
        </td>
    </tr>
    <tr>
        <th>
            電話番号2
        </th>
        <td>
            <%=Html.TextBox("sup.TelNumber2", Model.TelNumber2, new { @class = "alphanumeric", maxlength = 15 })%>
        </td>
    </tr>
    <tr>
        <th>
            FAX番号
        </th>
        <td>
            <%=Html.TextBox("sup.FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%>
        </td>
    </tr>
    <tr>
        <th>
            仕入先担当者名
        </th>
        <td>
            <%=Html.TextBox("sup.ContactName", Model.ContactName, new { size = 40, maxlength = 20 })%>
        </td>
    </tr>
    <%//Add 2023/09/05 yano #4162 %>
    <tr>
        <th>
            適格請求書発行事業者登録番号<%--Mod 2023/09/28 yano #4183 文言修正(適適格→適格)--%>
        </th>
        <td>
            <%=Html.TextBox("sup.IssueCompanyNumber", Model.IssueCompanyNumber, new { size = 40, maxlength = 50 })%>
        </td>
    </tr>
    <%}else{ %>
    <tr>
        <th>
            仕入先名 *
        </th>
        <td>
            <%=Html.TextBox("sup.SupplierName", Model.SupplierName, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.SupplierName", Model.SupplierName)%>
        </td>
    </tr>
    <tr>
        <th>
            外注フラグ *
        </th>
        <td>
            <%=Html.DropDownList("sup.OutsourceFlag", (IEnumerable<SelectListItem>)ViewData["OutsourceFlagList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("sup.OutsourceFlag", Model.OutsourceFlag)%>
        </td>
    </tr>
    <tr>
        <th>
            郵便番号
        </th>
        <td>
            <%=Html.TextBox("sup.PostCode", Model.PostCode, new { @disabled = "disabled" })%>
            <input type="button" id="Button1" name="SearchPostCode" value="郵便番号検索" disabled="disabled" />
            <%=Html.Hidden("sup.PostCode", Model.PostCode)%>
        </td>
    </tr>
    <tr>
        <th>
            都道府県
        </th>
        <td>
            <%=Html.TextBox("sup.Prefecture", Model.Prefecture, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.Prefecture", Model.Prefecture)%>
        </td>
    </tr>
    <tr>
        <th>
            市区町村
        </th>
        <td>
            <%=Html.TextBox("sup.City", Model.City, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.City", Model.City)%>
        </td>
    </tr>
    <tr>
        <th>
            住所1
        </th>
        <td>
            <%=Html.TextBox("sup.Address1", Model.Address1, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.Address1", Model.Address1)%>
        </td>
    </tr>
    <tr>
        <th>
            住所2
        </th>
        <td>
            <%=Html.TextBox("sup.Address2", Model.Address2, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.Address2", Model.Address2)%>
        </td>
    </tr>
    <tr>
        <th>
            電話番号1
        </th>
        <td>
            <%=Html.TextBox("sup.TelNumber1", Model.TelNumber1, new { @disabled = "disabled" })%>
            <%=Html.Hidden("sup.TelNumber1", Model.TelNumber1)%>
        </td>
    </tr>
    <tr>
        <th>
            電話番号2
        </th>
        <td>
            <%=Html.TextBox("sup.TelNumber2", Model.TelNumber2, new { @disabled = "disabled" })%>
            <%=Html.Hidden("sup.TelNumber2", Model.TelNumber2)%>
        </td>
    </tr>
    <tr>
        <th>
            FAX番号
        </th>
        <td>
            <%=Html.TextBox("sup.FaxNumber", Model.FaxNumber, new { @disabled = "disabled" })%>
            <%=Html.Hidden("sup.FaxNumber", Model.FaxNumber)%>
        </td>
    </tr>
    <tr>
        <th>
            仕入先担当者名
        </th>
        <td>
            <%=Html.TextBox("sup.ContactName", Model.ContactName, new { size = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.ContactName", Model.ContactName)%>
        </td>
    </tr>
    <%//Add 2023/09/05 yano #4162 %>
     <tr>
        <th>
            インボイス登録番号
        </th>
        <td>
            <%=Html.TextBox("sup.IssueCompanyNumber", Model.IssueCompanyNumber, new { size = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("sup.IssueCompanyNumber", Model.IssueCompanyNumber)%>
        </td>
    </tr>    
    
    <%} %>
</table>
<%=Html.Hidden("sup.SupplierCode", Model.SupplierCode)%>
<%=Html.Hidden("sup.DelFlag", Model.DelFlag)%>