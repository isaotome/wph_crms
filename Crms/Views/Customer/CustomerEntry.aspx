<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Customer>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	顧客マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Customer", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"]) %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("CustomerCode", Model.CustomerCode)%>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th>顧客ランク</th>
        <td><%=Html.DropDownList("CustomerRank", (IEnumerable<SelectListItem>)ViewData["CustomerRankList"])%></td>
        <th style="width:100px">顧客種別</th>
        <td><%=Html.DropDownList("CustomerKind", (IEnumerable<SelectListItem>)ViewData["CustomerKindList"])%></td>
      </tr>
      <tr>
        <th>前株・後株</th>
        <td colspan="3"><%=Html.DropDownList("CorporationType",(IEnumerable<SelectListItem>)ViewData["CorporationTypeList"])%></td>
      </tr>                            
      <tr>
        <th>顧客名1(姓) *</th>
        <td colspan="3"><%=Html.TextBox("FirstName", Model.FirstName, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>顧客名2(名) *</th>
        <td colspan="3"><%=Html.TextBox("LastName", Model.LastName, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>顧客名1(姓カナ)</th>
        <td colspan="3"><%=Html.TextBox("FirstNameKana", Model.FirstNameKana, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>顧客名2(名カナ)</th>
        <td colspan="3"><%=Html.TextBox("LastNameKana", Model.LastNameKana, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>顧客区分</th>
        <td><%=Html.DropDownList("CustomerType", (IEnumerable<SelectListItem>)ViewData["CustomerTypeList"])%></td>
        <th>支払方法</th>
        <td><%=Html.DropDownList("PaymentKind", (IEnumerable<SelectListItem>)ViewData["PaymentKindList"])%></td>
      </tr>
      <tr>
        <th>性別</th>
        <td><%=Html.DropDownList("Sex", (IEnumerable<SelectListItem>)ViewData["SexList"])%></td>
        <th>生年月日</th>
        <td><%=Html.TextBox("Birthday", string.Format("{0:yyyy/MM/dd}", Model.Birthday), new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>職業</th>
        <td><%=Html.DropDownList("Occupation", (IEnumerable<SelectListItem>)ViewData["OccupationList"])%></td>
        <th>車の所有</th>
        <td><%=Html.DropDownList("CarOwner", (IEnumerable<SelectListItem>)ViewData["CarOwnerList"])%></td>
      </tr>
      <tr>
           <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>郵便番号</th>
        <td colspan="3"><%=Html.TextBox("PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%> <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode()" /></td>
      </tr>
      <tr>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>都道府県</th>
        <td colspan="3"><%=Html.TextBox("Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>市区町村</th>
        <td colspan="3"><%=Html.TextBox("City", Model.City, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>住所1</th>
        <td colspan="3"><%=Html.TextBox("Address1", Model.Address1, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>住所2</th>
        <td colspan="3"><%=Html.TextBox("Address2", Model.Address2, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>電話番号</th>
        <td><%=Html.TextBox("TelNumber", Model.TelNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>

          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <th <%=CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "style=\"background-color:#cccc66\"" : ""%>>FAX番号</th>
        <td><%=Html.TextBox("FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
      </tr>
         <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        { %>
      <tr>
        <th style="background-color:#cccc66">請求先宛名</th>
        <td colspan="3"><%=Html.TextBox("CustomerClaimName", ViewData["NewCustomerClaimName"], new { size = 50, maxlength = 80 })%></td>
      </tr>
      <tr>
        <th style="background-color:#cccc66">請求先更新</th>
        <td colspan="3"><%=Html.CheckBox("UpdateCustomerClaim",ViewData["UpdateCustomerClaim"]) %>請求先にも同じ情報をコピーする</td>
      </tr>
      <%} %>
      <tr>
        <th>メールアドレス</th>
        <td colspan="3"><%=Html.TextBox("MailAddress", Model.MailAddress, new { @class = "alphanumeric", size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>携帯電話番号</th>
        <td colspan="3"><%=Html.TextBox("MobileNumber", Model.MobileNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
      </tr>
      <tr>
        <th>携帯メールアドレス</th>
        <td colspan="3"><%=Html.TextBox("MobileMailAddress", Model.MobileMailAddress, new { @class = "alphanumeric", size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>請求先</th>
        <td colspan="3"><%=Html.TextBox("CustomerClaimCode", Model.CustomerClaimCode, new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('CustomerClaimCode','CustomerClaimName','CustomerClaim')" })%>
            <img alt="請求先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerClaimCode', 'CustomerClaimName', '/CustomerClaim/CriteriaDialog')" />
            &nbsp;&nbsp;<span id="CustomerClaimName"><%=Html.Encode(ViewData["CustomerClaimName"])%></span>
        </td>
      </tr>
      <tr>
        <th>DM可否</th>
        <td><%=Html.DropDownList("DmFlag", (IEnumerable<SelectListItem>)ViewData["DmFlagList"])%></td>
        <th>DM発送備考欄</th>
        <td><%=Html.TextBox("DmMemo", Model.DmMemo, new { maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>勤務先名</th>
        <td colspan="3"><%=Html.TextBox("WorkingCompanyName", Model.WorkingCompanyName, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>勤務先住所</th>
        <td colspan="3"><%=Html.TextBox("WorkingCompanyAddress", Model.WorkingCompanyAddress, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>勤務先電話番号</th>
        <td><%=Html.TextBox("WorkingCompanyTelNumber", Model.WorkingCompanyTelNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
        <th>役職名</th>
        <td><%=Html.TextBox("PositionName", Model.PositionName, new { maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>取引先担当者名</th>
        <td><%=Html.TextBox("CustomerEmployeeName", Model.CustomerEmployeeName, new { maxlength = 40 })%></td>
        <th>経理担当者名</th>
        <td><%=Html.TextBox("AccountEmployeeName", Model.AccountEmployeeName, new { maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>部門 <%=(CommonUtils.DefaultString(ViewData["update"]).Equals("1") ? "" : "*") %></th>
        <td colspan="3">
            <%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
            <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span>
        </td>
      </tr>
      <tr>
        <th>営業担当者</th>
        <td colspan="3"><%=Html.TextBox("CarEmployeeCode", Model.CarEmployeeCode, new { @class = "alphanumeric", size = 50, maxlength = 50, onblur = "GetNameFromCode('CarEmployeeCode','CarEmployeeName','Employee')" })%>
            <img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarEmployeeCode', 'CarEmployeeName', '/Employee/CriteriaDialog')" />
            &nbsp;&nbsp;<span id="CarEmployeeName"><%=Html.Encode(ViewData["CarEmployeeName"])%></span>
        </td>
      </tr>
      <tr>
        <th>サービス担当者</th>
        <td colspan="3"><%=Html.TextBox("ServiceEmployeeCode", Model.ServiceEmployeeCode, new { @class = "alphanumeric", size = 50, maxlength = 50, onblur = "GetNameFromCode('ServiceEmployeeCode','ServiceEmployeeName','Employee')" })%>
            <img alt="社員検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ServiceEmployeeCode', 'ServiceEmployeeName', '/Employee/CriteriaDialog')" />
            &nbsp;&nbsp;<span id="ServiceEmployeeName"><%=Html.Encode(ViewData["ServiceEmployeeName"])%></span>
        </td>
      </tr>
      <tr>
        <th>初回来店日</th>
        <td><%=Html.TextBox("FirstReceiptionDate", Model.FirstReceiptionDate, new { @class = "alphanumeric", maxlength = 10 })%></td>
        <th>前回来店日</th>
        <td><%=Html.TextBox("LastReceiptionDate", Model.LastReceiptionDate, new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>備考</th>
          <% // Mod 2014/07/22 arc amii chromeでDB登録する際、改行コードも文字として登録してしまうのを修正 %>
        <td colspan="3"><%=Html.TextArea("Memo", Model.Memo, 3, 50, new { wrap = "virtual", onblur = "checkTextLength('Memo', 200, '備考')" })%></td>
      </tr>
      <tr>
        <th>ステータス</th>
        <td colspan="3">
            <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
          <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
            {%>
            <%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効
          <%}
            else
            {%>
            <%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効
          <%} %>
        </td>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
