<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Warehouse>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
<%//Add 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成%>
    倉庫マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Warehouse", FormMethod.Post))
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
    <table class="input-form" style="width:400px">
      <tr>
        <th style="width:100px">倉庫コード *</th>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="WarehouseCode" name="WarehouseCode" value="<%=Model.WarehouseCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("WarehouseCode", Model.WarehouseCode, new { @class = "alphanumeric", maxlength = 6, onblur = "IsExistCode('WarehouseCode','Warehouse')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>倉庫名 *</th>
        <td><%=Html.TextBox("WarehouseName", Model.WarehouseName, new { size = 40, maxlength = 40 })%></td>
      </tr>
      <tr>
        <th rowspan="2">倉庫管理部門 *</th>
        <td><%=Html.TextBox("DepartmentCode", Model.DepartmentCode, new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
            <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>       
      </tr>
      <tr>
        <th>ステータス</th>
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
