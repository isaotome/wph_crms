<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Department>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部門マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Department", FormMethod.Post))
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
        <th style="width:100px">部門コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="DepartmentCode" name="DepartmentCode" value="<%=Model.DepartmentCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "IsExistCode('DepartmentCode','Department')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>部門名 *</th>
        <td><%=Html.TextBox("DepartmentName", Model.DepartmentName, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>正式名称</th>
        <td><%=Html.TextBox("FullName", Model.FullName, new { size = 50, maxlength = 50 }) %></td>
      </tr>
      <tr>
        <th>略称</th>
        <td><%=Html.TextBox("DepartmentShortName", Model.DepartmentShortName, new { size = 20, maxlength = 50 }) %></td>
      </tr>
      <tr><%//Add 2018/01/30 arc yano #3853 部門マスタ　屋号編集機能の追加 %>
        <th>屋号</th>
        <td><%=Html.TextBox("StoreName", Model.StoreName, new { size = 20, maxlength = 50 }) %></td>
      </tr>
      <tr>
        <th rowspan="2">エリア *</th>
        <td><%=Html.TextBox("AreaCode", Model.AreaCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('AreaCode','AreaName','Area')" })%>
            <img alt="エリア検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('AreaCode', 'AreaName', '/Area/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="AreaName"><%=Html.Encode(ViewData["AreaName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">事業所 *</th>
        <td><%=Html.TextBox("OfficeCode", Model.OfficeCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('OfficeCode','OfficeName','Office')" })%>
            <img alt="事業所検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('OfficeCode', 'OfficeName', '/Office/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="OfficeName"><%=Html.Encode(ViewData["OfficeName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">部門長 *</th>
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
        <td><%=Html.TextBox("Prefecture", Model.Prefecture, new { maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>市区町村</th>
        <td><%=Html.TextBox("City", Model.City, new { maxlength = 50 })%></td>
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
        <th>銀行名</th>
        <td><%=Html.TextBox("BankName", Model.BankName, new { size = 30, maxlength = 15 })%></td>
      </tr>
      <tr>
        <th>支店名</th>
        <td><%=Html.TextBox("BranchName", Model.BranchName, new { size = 30, maxlength = 15 })%></td>
      </tr>
      <tr>
        <th>預金種目</th>
        <td><%=Html.DropDownList("DepositKind", (IEnumerable<SelectListItem>)ViewData["DepositKindList"])%></td>
      </tr>
      <tr>
        <th>口座番号</th>
        <td><%=Html.TextBox("AccountNumber", Model.AccountNumber, new { @class = "alphanumeric", maxlength = 7 })%></td>
      </tr>
      <tr>
        <th>口座名義人</th>
        <td><%=Html.TextBox("AccountHolder", Model.AccountHolder, new { size = 30, maxlength = 30 })%></td>
      </tr>
      <tr>
        <th>振込先印字</th>
        <td><%=Html.CheckBox("PrintFlag",!string.IsNullOrEmpty(Model.PrintFlag) && Model.PrintFlag.Equals("1")) %>注文書・納品請求書に振込先を印字</td>
      </tr>
      <tr>
        <th>業務区分</th>
        <td><%=Html.DropDownList("BusinessType",(IEnumerable<SelectListItem>)ViewData["BusinessTypeList"]) %></td>
      </tr>
       <%//Add 2018/07/27 yano.hiroki #3923 %>
       <tr>
        <th rowspan="2">既定の仕入先</th>
        <td><%=Html.TextBox("DefaultSupplierCode", Model.DefaultSupplierCode, new { @class = "alphanumeric", maxlength = 10, onblur = "GetNameFromCode('DefaultSupplierCode','DefaultSupplierName','Supplier')" })%>
            <img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DefaultSupplierCode', 'DefaultSupplierName', '/Supplier/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DefaultSupplierName"><%=Html.Encode(ViewData["DefaultSupplierName"])%></span></td>       
      </tr>
      <%//Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 車両棚卸対象フラグ、部品棚卸対象フラグの設定の追加 %>
      <tr>
        <th>車両棚卸対象</th>
        <td><%=Html.CheckBox("CarInventoryFlag", !string.IsNullOrWhiteSpace(Model.CarInventoryFlag) && Model.CarInventoryFlag.Equals("1"))%> </td>
      </tr>
       <tr>
        <th>部品棚卸対象</th>
        <td><%=Html.CheckBox("PartsInventoryFlag", !string.IsNullOrWhiteSpace(Model.PartsInventoryFlag) && Model.PartsInventoryFlag.Equals("1"))%> </td>
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
