<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarColor>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両カラーマスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarColor", FormMethod.Post))
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
        <th style="width:100px">車両カラーコード *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="CarColorCode" name="CarColorCode" value="<%=Model.CarColorCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("CarColorCode", Model.CarColorCode, new { @class = "alphanumeric", maxlength = 8, onblur = "IsExistCode('CarColorCode','CarColor')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>車両カラー名 *</th>
        <td><%=Html.TextBox("CarColorName", Model.CarColorName, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>メーカーカラーコード *</th>
        <td><%=Html.TextBox("MakerColorCode", Model.MakerColorCode, new {@class="alphanumeric",size="50",maxlength="50"}) %></td>
      </tr>
      <tr>
        <th rowspan="2">メーカー *</th>
        <td><%=Html.TextBox("MakerCode", Model.MakerCode, new { @class = "alphanumeric", maxlength = 5, onblur = "GetNameFromCode('MakerCode','MakerName','Maker')" })%>
            <img alt="メーカー検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('MakerCode', 'MakerName', '/Maker/CriteriaDialog')" /></td>
      </tr>
      <tr>
        <td style="height:20px"><span id="MakerName"><%=Html.Encode(ViewData["MakerName"])%></span></td>       
      </tr>
      <tr>
        <th>系統色</th>
        <td><%=Html.DropDownList("ColorCategory", (IEnumerable<SelectListItem>)ViewData["ColorCategoryList"])%></td>
      </tr>
      <tr>
        <th>外装色・内装色</th>
        <td>
            <%=Html.CheckBox("InteriorColorFlag",Model.InteriorColorFlag!=null && Model.InteriorColorFlag.Equals("1") ? true : false) %>内装色 
            <%=Html.CheckBox("ExteriorColorFlag",Model.ExteriorColorFlag!=null && Model.ExteriorColorFlag.Equals("1") ? true : false) %>外装色
        </td>
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
