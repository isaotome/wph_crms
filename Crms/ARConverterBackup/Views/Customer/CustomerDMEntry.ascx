<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CustomerDM>" %>
<%@ Import Namespace="CrmsDao" %> 
<%=Html.Hidden("dm.DelFlag", Model.DelFlag) %>
<table class="input-form" style="width:700px">
    <tr>
        <th colspan="4" class="input-form-title">DM発送先</th>
    </tr>
    <tr>
        <th style="width: 150px">
        </th>
        <td colspan="3"><%=Html.CheckBox("CustomerDMEnabled", Model.DelFlag!=null && Model.DelFlag.Equals("0"), new { onclick = "document.forms[0].action = '/Customer/DMEnabled';formSubmit()" })%>DM発送先を別名で登録する</td>
    </tr>
    <tr>
        <th style="width:150px">顧客コード</th>
        <td colspan="3">
            <%=Html.Encode(Model.CustomerCode) %><%=Html.Hidden("dm.CustomerCode",Model.CustomerCode) %>
        </td>
    </tr>
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if (CommonUtils.DefaultString(Model.DelFlag).Equals("0"))
      { %>
    <tr>
        <th>前株・後株</th>
        <td colspan="3"><%=Html.DropDownList("dm.CorporationType",(IEnumerable<SelectListItem>)ViewData["dm.CorporationTypeList"])%></td>
    </tr>
    <tr>
        <th>顧客名1(姓) *</th>
        <td colspan="3"><%=Html.TextBox("dm.FirstName", Model.FirstName, new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>顧客名2(名)</th>
        <td colspan="3"><%=Html.TextBox("dm.LastName", Model.LastName, new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>顧客名1(姓カナ) *</th>
        <td colspan="3"><%=Html.TextBox("dm.FirstNameKana", Model.FirstNameKana, new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>顧客名2(名カナ)</th>
        <td colspan="3"><%=Html.TextBox("dm.LastNameKana", Model.LastNameKana, new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>郵便番号</th>
        <td colspan="3"><%=Html.TextBox("dm.PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%> <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode('dm_')" /></td><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
    </tr>
    <tr>
        <th>都道府県</th>
        <td colspan="3"><%=Html.TextBox("dm.Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>市区町村</th>
        <td colspan="3"><%=Html.TextBox("dm.City", Model.City, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>番地</th>
        <td colspan="3"><%=Html.TextBox("dm.Address1", Model.Address1, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>建物名・部屋番号</th>
        <td colspan="3"><%=Html.TextBox("dm.Address2", Model.Address2, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>電話番号</th>
        <td><%=Html.TextBox("dm.TelNumber", Model.TelNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
        <th style="width:150px">FAX番号</th>
        <td><%=Html.TextBox("dm.FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>備考</th>
         <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
        <td colspan="3"><%=Html.TextArea("dm.Memo", Model.Memo, 3, 50, new { wrap = "virtual", onblur = "checkTextLength('dm_Memo', 200, '備考')" })%></td><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
    </tr>
    <%}else{ %>
    <tr>
        <th>前株・後株</th>
        <td colspan="3">
            <%=Html.DropDownList("dm.CorporationType", (IEnumerable<SelectListItem>)ViewData["CorporationTypeList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("dm.CorporationType", Model.CorporationType) %>
        </td>
    </tr>
    <tr>
        <th>顧客名1(姓) *</th>
        <td colspan="3">
            <%=Html.TextBox("dm.FirstName", Model.FirstName, new { size = 50, maxlength = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.FirstName", Model.FirstName) %>
        </td>
    </tr>
    <tr>
        <th>顧客名2(名)</th>
        <td colspan="3">
            <%=Html.TextBox("dm.LastName", Model.LastName, new { size = 50, maxlength = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.LastName", Model.LastName) %>
        </td>
    </tr>
    <tr>
        <th>顧客名1(姓カナ) *</th>
        <td colspan="3">
            <%=Html.TextBox("dm.FirstNameKana", Model.FirstNameKana, new { size = 50, maxlength = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.FirstNameKana", Model.FirstNameKana) %>
        </td>
    </tr>
    <tr>
        <th>顧客名2(名カナ)</th>
        <td colspan="3">
            <%=Html.TextBox("dm.LastNameKana", Model.LastNameKana, new { size = 50, maxlength = 40, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.LastNameKana", Model.LastNameKana) %>
        </td>
    </tr>
    <tr>
        <th>郵便番号</th>
        <td colspan="3">
            <%=Html.TextBox("dm.PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8, @disabled = "disabled" })%> 
            <%=Html.Hidden("dm.PostCode", Model.PostCode) %>
            <input type="button" id="Button1" name="SearchPostCode" value="郵便番号検索" disabled="disabled" />
        </td>
    </tr>
    <tr>
        <th>都道府県</th>
        <td colspan="3">
            <%=Html.TextBox("dm.Prefecture", Model.Prefecture, new { size = 50, maxlength = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.Prefecture", Model.Prefecture) %>
        </td>
    </tr>
    <tr>
        <th>市区町村</th>
        <td colspan="3">
            <%=Html.TextBox("dm.City", Model.City, new { size = 50, maxlength = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.City", Model.City) %>
        </td>
    </tr>
    <tr>
        <th>番地</th>
        <td colspan="3">
            <%=Html.TextBox("dm.Address1", Model.Address1, new { size = 50, maxlength = 100, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.Address1", Model.Address1) %>
        </td>
    </tr>
    <tr>
        <th>建物名・部屋番号</th>
        <td colspan="3">
            <%=Html.TextBox("dm.Address2", Model.Address2, new { size = 50, maxlength = 100, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.Address2", Model.Address2) %>
        </td>
    </tr>
    <tr>
        <th>電話番号</th>
        <td>
            <%=Html.TextBox("dm.TelNumber", Model.TelNumber, new { @class = "alphanumeric", maxlength = 15, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.TelNumber", Model.TelNumber) %>
        </td>
        <th style="width:150px">FAX番号</th>
        <td>
            <%=Html.TextBox("dm.FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15, @disabled = "disabled" })%>
            <%=Html.Hidden("dm.FaxNumber", Model.FaxNumber) %>
        </td>
    </tr>
    <tr>
        <th>備考</th>
        <td colspan="3">
            <%=Html.TextArea("dm.Memo", Model.Memo, 3, 50, new { wrap = "physical", @disabled = "disabled" })%>
            <%=Html.Hidden("dm.Memo", Model.Memo) %>
        </td>
    </tr>
    <%} %>
</table>                            

