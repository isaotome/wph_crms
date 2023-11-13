<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Car>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車種マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Car", FormMethod.Post))
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
        <th style="width:100px">車種コード *</th>
           <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CarCode" name="CarCode" value="<%=Model.CarCode%>" readonly="readonly", size="30" /><%}
              else
              { %><%=Html.TextBox("CarCode", Model.CarCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "IsExistCode('CarCode','Car')" })%><%}%>
        </td>
      </tr>
      <tr>
        <th>車種名 *</th>
        <td><%=Html.TextBox("CarName", Model.CarName, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th rowspan="2">ブランド *</th>
        <td><%=Html.TextBox("CarBrandCode", Model.CarBrandCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "GetNameFromCode('CarBrandCode','CarBrandName','Brand')" })%>
            <img alt="ブランド検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarBrandCode', 'CarBrandName', '/Brand/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="CarBrandName"><%=Html.Encode(ViewData["CarBrandName"])%></span></td>       
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
