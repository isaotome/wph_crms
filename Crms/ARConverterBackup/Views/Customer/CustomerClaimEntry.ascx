<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CustomerClaim>" %>
<%@ Import Namespace="CrmsDao" %> 

<table class="input-form" style="width: 700px">
    <tr>
        <th colspan="2" class="input-form-title">請求先情報</th>
    </tr>
    <tr>
        <th style="width: 150px">
        </th>
        <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%=Html.CheckBox("CustomerClaimEnabled", CommonUtils.DefaultString(Model.DelFlag).Equals("0"), new { onclick = "document.forms[0].action = '/Customer/ClaimEnabled';formSubmit()" })%>請求先として登録する</td>
    </tr>
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
    <%if (CommonUtils.DefaultString(Model.DelFlag).Equals("0")) { %>
    <tr>
        <th>請求先名 *</th>
        <td><%=Html.TextBox("claim.CustomerClaimName", Model.CustomerClaimName, new { size = 50, maxlength = 80 })%></td>
    </tr>
    <tr>
        <th>請求種別 *</th>
        <td><%=Html.DropDownList("claim.CustomerClaimType", (IEnumerable<SelectListItem>)ViewData["CustomerClaimTypeList"])%></td>
    </tr>
    <tr>
        <th>締めの有無</th>
        <td><%=Html.DropDownList("claim.PaymentKindType", (IEnumerable<SelectListItem>)ViewData["PaymentKindTypeList"])%></td>
    </tr>
    <tr>
        <th>小数点以下の端数処理</th>
        <td><%=Html.DropDownList("claim.RoundType", (IEnumerable<SelectListItem>)ViewData["RoundTypeList"])%></td>
    </tr>
    <tr>
        <th>郵便番号</th>
        <td>
            <%=Html.TextBox("claim.PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%>
            <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode('claim_')" /><%//Mod 2021/05/27 yano #4045 Chrome対応 checkTextLengthに引数をid名に変更 %>
        </td>
    </tr>
    <tr>
        <th>都道府県</th>
        <td><%=Html.TextBox("claim.Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>市区町村</th>
        <td><%=Html.TextBox("claim.City", Model.City, new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>住所1</th>
        <td><%=Html.TextBox("claim.Address1", Model.Address1, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>住所2</th>
        <td><%=Html.TextBox("claim.Address2", Model.Address2, new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>電話番号1</th>
        <td><%=Html.TextBox("claim.TelNumber1", Model.TelNumber1, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>電話番号2</th>
        <td><%=Html.TextBox("claim.TelNumber2", Model.TelNumber2, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <tr>
        <th>FAX番号</th>
        <td><%=Html.TextBox("claim.FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
    </tr>
    <%} else {%>
    <tr>
        <th>請求先名 *</th>
        <td>
            <%=Html.TextBox("claim.CustomerClaimName", Model.CustomerClaimName, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("claim.CustomerClaimName", Model.CustomerClaimName)%>
        </td>
    </tr>
    <tr>
        <th>請求種別 *</th>
        <td>
            <%=Html.DropDownList("claim.CustomerClaimType", (IEnumerable<SelectListItem>)ViewData["CustomerClaimTypeList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.CustomerClaimType", Model.CustomerClaimType)%>
        </td>
    </tr>
    <tr>
        <th>締めの有無</th>
        <td>
            <%=Html.DropDownList("claim.PaymentKindType", (IEnumerable<SelectListItem>)ViewData["PaymentKindTypeList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.PaymentKindType", Model.PaymentKindType)%>
        </td>
    </tr>
    <tr>
        <th>小数点以下の端数処理</th>
        <td>
            <%=Html.DropDownList("claim.RoundType", (IEnumerable<SelectListItem>)ViewData["RoundTypeList"], new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.RoundType", Model.RoundType)%>    
        </td>
    </tr>
    <tr>
        <th>郵便番号</th>
        <td>
            <%=Html.TextBox("claim.PostCode", Model.PostCode, new { @disabled = "disabled" })%>
            <input type="button" id="Button1" name="SearchPostCode" value="郵便番号検索" disabled="disabled" />
            <%=Html.Hidden("claim.PostCode", Model.PostCode)%>
        </td>
    </tr>
    <tr>
        <th>都道府県</th>
        <td>
            <%=Html.TextBox("claim.Prefecture", Model.Prefecture, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("claim.Prefecture", Model.Prefecture)%>
        </td>
    </tr>
    <tr>
        <th>市区町村</th>
        <td>
            <%=Html.TextBox("claim.City", Model.City, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("claim.City", Model.City)%>
        </td>
    </tr>
    <tr>
        <th>住所1</th>
        <td>
            <%=Html.TextBox("claim.Address1", Model.Address1, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("claim.Address1", Model.Address1)%>
        </td>
    </tr>
    <tr>
        <th>住所2</th>
        <td>
            <%=Html.TextBox("claim.Address2", Model.Address2, new { size = 50, @disabled = "disabled" })%>
            <%=Html.Hidden("claim.Address2", Model.Address2)%>
        </td>
    </tr>
    <tr>
        <th>電話番号1</th>
        <td>
            <%=Html.TextBox("claim.TelNumber1", Model.TelNumber1, new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.TelNumber1", Model.TelNumber1)%>    
        </td>
    </tr>
    <tr>
        <th>電話番号2</th>
        <td>
            <%=Html.TextBox("claim.TelNumber2", Model.TelNumber2, new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.TelNumber2", Model.TelNumber2)%>
        </td>
    </tr>
    <tr>
        <th>FAX番号</th>
        <td>
            <%=Html.TextBox("claim.FaxNumber", Model.FaxNumber, new { @disabled = "disabled" })%>
            <%=Html.Hidden("claim.FaxNumber", Model.FaxNumber)%>
        </td>
    </tr>
    <%} %>
</table>
<br />
<table class="input-form" style="width: 700px">
    <tr>
        <th class="input-form-title" colspan="3">
            決済条件
        </th>
    </tr>
    <tr>
        <th style="width: 15px">
            <img alt="追加" style="cursor: pointer" src="/Content/Images/plus.gif" onclick="document.forms[0].action='/Customer/AddClaimable';formSubmit()" />
        </th>
        <th style="width: 150px">
            支払種別コード
        </th>
        <th>
            支払種別名
        </th>
    </tr>
</table>
<div style="width: 718px; height: 200px; overflow-y: scroll">
    <table class="input-form" style="width: 700px">
        <%for (int i = 0; i < Model.CustomerClaimable.Count();i++ ) {
              var item = Model.CustomerClaimable[i];
              string prefix = string.Format("claimable[{0}].", i);
              //2014/05/30 vs2012対応 arc yano 各コントロールにid追加
              string idprefix = string.Format("claimable[{0}]_", i);
              //Mod 2014/07/15 arc yano chrome対応 opensearchdialog等のスクリプトに渡すパラメータをnameからidに変更。
        %>
        <tr>
            <td style="width: 15px">
                <img alt="削除" style="cursor: pointer" src="/Content/Images/minus.gif" onclick="document.forms[0].action='/Customer/DelClaimable/<%=i %>';formSubmit()" />
                <%=Html.Hidden(prefix + "CustomerClaimCode", item.CustomerClaimCode, new { id = idprefix + "CustomerClaimCode" })%>
            </td>
            <td style="width: 150px">
                <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
                <%if (CommonUtils.DefaultString(Model.DelFlag).Equals("0"))
                  { %>
                    <%=Html.TextBox(prefix + "PaymentKindCode", item.PaymentKindCode, new { id = idprefix + "PaymentKindCode", @class = "alphanumeric", size = "10", maxlength = "10",onblur="GetNameFromCode('"+idprefix+"PaymentKindCode','"+idprefix+"PaymentKindName','PaymentKind')" })%>&nbsp;<img alt="支払種別" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('<%=idprefix + "PaymentKindCode" %>','<%=idprefix + "PaymentKindName" %>','/PaymentKind/CriteriaDialog')" />
                <%}else{ %>
                    <%=Html.TextBox(prefix + "PaymentKindCode", item.PaymentKindCode,new { id = idprefix + "PaymentKindCode", @disabled="disabled",size="10"}) %>
                    <%=Html.Hidden(prefix + "PaymentKindCode", item.PaymentKindCode, new { id = idprefix + "PaymentKindCode" })%>
                <%} %>
            </td>
            <td>
                <span id="<%=idprefix + "PaymentKindName" %>"><%=Html.Encode(item.PaymentKind != null ? item.PaymentKind.PaymentKindName : "")%></span>
            </td>
        </tr>
        <%} %>
    </table>
</div>
<%=Html.Hidden("claim.CustomerClaimCode", Model.CustomerClaimCode)%>
<%=Html.Hidden("claim.DelFlag", Model.DelFlag)%>