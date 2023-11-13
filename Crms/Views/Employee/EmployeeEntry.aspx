<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Employee>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	社員マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Employee", FormMethod.Post))
      { %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
   </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"])%>
<%=Html.Hidden("close", ViewData["close"]) %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:100px">社員コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              {%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "readonly", @readonly = "readonly", size = 50 }) %>
            <%}else{%>
                <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", size = 50, maxlength = 50, onblur = "IsExistCode('EmployeeCode','Employee')" })%>
            <%} %>
        </td>
      </tr>
      <tr>
        <th>社員番号</th>
        <td><%=Html.TextBox("EmployeeNumber", Model.EmployeeNumber, new { @class = "alphanumeric", maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>氏名 *</th>
        <td><%=Html.TextBox("EmployeeName", Model.EmployeeName, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th>氏名(カナ)</th>
        <td><%=Html.TextBox("EmployeeNameKana", Model.EmployeeNameKana, new { size = 50, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th rowspan="2">部門 *</th>
        <td><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%> <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">兼務部門1</th>
        <td><%=Html.TextBox("DepartmentCode1", Model.DepartmentCode1, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode1','DepartmentName1','Department')" })%> <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode1', 'DepartmentName1', '/Department/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DepartmentName1"><%=Html.Encode(ViewData["DepartmentName1"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">兼務部門2</th>
        <td><%=Html.TextBox("DepartmentCode2", Model.DepartmentCode2, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode2','DepartmentName2','Department')" })%> <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode2', 'DepartmentName2', '/Department/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DepartmentName2"><%=Html.Encode(ViewData["DepartmentName2"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">兼務部門3</th>
        <td><%=Html.TextBox("DepartmentCode3", Model.DepartmentCode3, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode3','DepartmentName3','Department')" })%> <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode3', 'DepartmentName3', '/Department/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DepartmentName3"><%=Html.Encode(ViewData["DepartmentName3"])%></span></td>       
      </tr>
      <tr>
        <th>セキュリティロール *</th>
        <td><%=Html.DropDownList("SecurityRoleCode", (IEnumerable<SelectListItem>)ViewData["SecurityRoleList"])%></td>
      </tr>
      <tr>
        <th>携帯電話番号</th>
        <td><%=Html.TextBox("MobileNumber", Model.MobileNumber, new { @class = "alphanumeric", maxlength = 15 }) %></td>
      </tr>
      <tr>  
        <th>携帯メールアドレス</th>
        <td><%=Html.TextBox("MobileMailAddress", Model.MobileMailAddress, new { @class = "alphanumeric", size = 40, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>メールアドレス</th>
        <td><%=Html.TextBox("MailAddress", Model.MailAddress, new { @class = "alphanumeric", size = 40, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>雇用種別</th>
        <td><%=Html.DropDownList("EmployeeType", (IEnumerable<SelectListItem>)ViewData["EmployeeTypeList"])%></td>
      </tr>
      <tr>
        <th>生年月日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("Birthday", string.Format("{0:yyyy/MM/dd}", Model.Birthday), new { @class = "alphanumeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>ステータス</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
      <%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
        {%>
        <td><%=Html.RadioButton("DelFlag", "0")%>有効<%=Html.RadioButton("DelFlag", "1")%>無効</td>
      <%}
        else
        {%>
        <td><%=Html.RadioButton("DelFlag", "0", true)%>有効<%=Html.RadioButton("DelFlag", "1", new { disabled = true })%>無効</td>
      <%} %>
      </tr>
    </table>
</div>
<%} %>
<br />
</asp:Content>
