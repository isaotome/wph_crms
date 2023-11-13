<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Brand>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	ブランドマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Brand", FormMethod.Post))
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
        <th style="width:100px">ブランドコード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CarBrandCode" name="CarBrandCode" value="<%=Model.CarBrandCode%>" readonly="readonly", size="30" /><%}
              else
              { %><%=Html.TextBox("CarBrandCode", Model.CarBrandCode, new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "IsExistCode('CarBrandCode','Brand')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>ブランド名 *</th>
        <td><%=Html.TextBox("CarBrandName", Model.CarBrandName, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th rowspan="2">メーカー *</th>
        <td><%=Html.TextBox("MakerCode", Model.MakerCode, new { @class = "alphanumeric", maxlength = 5, onblur = "GetNameFromCode('MakerCode','MakerName','Maker')" })%>
            <img alt="メーカー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('MakerCode', 'MakerName', '/Maker/CriteriaDialog')" />
        </td>
      </tr>
      <tr>
        <td style="height:20px"><span id="MakerName"><%=Html.Encode(ViewData["MakerName"])%></span></td>       
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
        <th>レバレート</th>
          <% // Mod 2014/07/14 arc amii 既存バグ対応 DBの桁数と入力可能文字数が一致していなかったので、入力可能文字数を11 → 10 に修正  %>
        <td><%=Html.TextBox("LaborRate", Model.LaborRate, new { @class = "numeric", maxlength = "10", size = "10"})%></td>
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
