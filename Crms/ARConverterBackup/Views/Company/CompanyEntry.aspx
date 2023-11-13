<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Company>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	会社マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Company", FormMethod.Post))
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
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <br />
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:100px">会社コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CompanyCode" name="CompanyCode" value="<%=Model.CompanyCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("CompanyCode", Model.CompanyCode, new { @class = "alphanumeric", maxlength = 3, onblur = "IsExistCode('CompanyCode','Company')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>会社名 *</th>
        <td><%=Html.TextBox("CompanyName", Model.CompanyName, new { maxlength = 20 })%></td>
      </tr>
      <tr>
        <th rowspan="2">代表者 *</th>
        <td>    
            <%=Html.TextBox("EmployeeNumber", Model.Employee != null ? Model.Employee.EmployeeNumber : "", new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" })%>
            <%=Html.TextBox("EmployeeCode", Model.EmployeeCode, new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" })%>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0" }); %>
        </td>
      </tr>
      <tr>
        <td style="height:20px">            
            <%=Html.TextBox("EmployeeName", Model.Employee!=null ? Model.Employee.EmployeeName : "", new { @class = "readonly", style = "width:150px", @readonly = "readonly" })%>
        </td>       
      </tr>
      <tr>
        <th>郵便番号</th>
        <td><%=Html.TextBox("PostCode", Model.PostCode, new { @class = "alphanumeric", maxlength = 8 })%> <input type="button" id="SearchPostCode" name="SearchPostCode" value="郵便番号検索" onclick="getAddressFromPostCode()" /></td>
      </tr>
      <tr>
        <th>都道府県</th>
        <td><%=Html.TextBox("Prefecture", Model.Prefecture, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>市区町村</th>
        <td><%=Html.TextBox("City", Model.City, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>住所1</th>
        <td><%=Html.TextBox("Address1", Model.Address1, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>住所2</th>
        <td><%=Html.TextBox("Address2", Model.Address2, new { size = 50, maxlength = 100 })%></td>
      </tr>
      <tr>
        <th>電話番号1</th>
        <td><%=Html.TextBox("TelNumber1", Model.TelNumber1, new { @class = "alphanumeric", maxlength = 15 })%></td>
      </tr>
      <tr>
        <th>電話番号2</th>
        <td><%=Html.TextBox("TelNumber2", Model.TelNumber2, new { @class = "alphanumeric", maxlength = 15 })%></td>
      </tr>
      <tr>
        <th>FAX番号</th>
        <td><%=Html.TextBox("FaxNumber", Model.FaxNumber, new { @class = "alphanumeric", maxlength = 15 })%></td>
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
