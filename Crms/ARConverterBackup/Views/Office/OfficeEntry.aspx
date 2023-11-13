<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Office>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	事業所マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Office", FormMethod.Post))
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
        <th style="width:100px">事業所コード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="OfficeCode" name="OfficeCode" value="<%=Model.OfficeCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("OfficeCode", Model.OfficeCode, new { @class = "alphanumeric", maxlength = 3, onblur = "IsExistCode('OfficeCode','Office')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>事業所名 *</th>
        <td><%=Html.TextBox("OfficeName", Model.OfficeName, new { size = 50, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>正式名称</th>
        <td><%=Html.TextBox("FullName",Model.FullName,new {size=50,maxlength=50}) %></td>
      </tr>
      <tr>
        <th rowspan="2">会社 *</th>
        <td><%=Html.TextBox("CompanyCode", Model.CompanyCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('CompanyCode','CompanyName','Company')" })%>
            <img alt="会社検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CompanyCode', 'CompanyName', '/Company/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CompanyName"><%=Html.Encode(ViewData["CompanyName"])%></span></td>       
      </tr>
      <tr>
        <th rowspan="2">事業所長 *</th>
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
     <%--Mod 2020/02/21 yano <tr>
        <th rowspan="2">諸費用設定エリア</th>
        <% //2014/07/24 arc yano chrome対応 諸費用設定エリアーコードを手入力した場合でも、名称が更新されるように修正。 %>
        <td><%=Html.TextBox("CostAreaCode",Model.CostAreaCode,new {@class="alphanumeric",size=10,maxlength=3, onchange = "GetNameFromCode('CostAreaCode','CostAreaName','CostArea')"}) %>&nbsp;<img alt="諸費用設定エリア検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CostAreaCode','CostAreaName','/CostArea/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td><span id="CostAreaName"><%=CommonUtils.DefaultNbsp(Model.CostArea!=null ? Model.CostArea.CostAreaName : "") %></span></td>
      </tr>--%>
      <tr>
        <th rowspan="2">デフォルト仕入先</th>
        <td>
            <%=Html.TextBox("SupplierCode", Model.SupplierCode, new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "GetNameFromCode('SupplierCode','SupplierName','Supplier')" })%>
            &nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <%//MOD 2014/10/29 ishii 保存ボタン対応 デフォルト仕入先名表示のため%>
        <%--//<td><span id="SupplierName"><%=CommonUtils.DefaultNbsp(ViewData["SupplierName"]) %></span></td> --%>
        <td><span id="SupplierName"><%=CommonUtils.DefaultNbsp(Model.Supplier!=null ? Model.Supplier.SupplierName : "") %></span></td>
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
