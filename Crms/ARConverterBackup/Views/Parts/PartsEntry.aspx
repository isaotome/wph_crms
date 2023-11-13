<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.Parts>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "Parts", FormMethod.Post))
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
        <th style="width:100px">部品番号 *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="PartsNumber" name="PartsNumber" value="<%=Model.PartsNumber%>" readonly="readonly" size="25" /><%}
              else
              { %><%=Html.TextBox("PartsNumber", Model.PartsNumber, new { @class = "alphanumeric", size = 25, maxlength = 25, onblur = "IsExistCode('PartsNumber','Parts')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>部品名(日本語) *</th>
        <td><%=Html.TextBox("PartsNameJp", Model.PartsNameJp, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>部品名(英語)</th>
        <td><%=Html.TextBox("PartsNameEn", Model.PartsNameEn, new { @class = "alphanumeric", size = 50, maxlength = 50 })%></td>
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
        <th>定価</th>
        <td><%=Html.TextBox("Price", Model.Price, new { @class = "numeric", maxlength = 10, onchange = "document.getElementById('SalesPrice').value=this.value" })%></td>
      </tr>
      <tr>
        <th>S/O原価</th>
        <td><%=Html.TextBox("SoPrice", Model.SoPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>E/O原価</th>
        <td><%=Html.TextBox("EoPrice", Model.EoPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>M/P原価</th>
        <td><%=Html.TextBox("MpPrice", Model.MpPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <% // Mod 2016/01/21 arc yano #3403_部品マスタ入力　純正区分、メーカー部品番号の必須項目化 (#3397_【大項目】部品仕入機能改善 課題管理表対応) 純正区分を必須項目に %>
        <th>純正区分 *</th>
        <td><%=Html.DropDownList("GenuineType", (IEnumerable<SelectListItem>)ViewData["GenuineTypeList"])%></td>
      </tr>
      <tr>
        <th>備考(旧部品番号など)</th>
        <td><%=Html.TextBox("Memo", Model.Memo, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr><% //Add 2016/06/03 arc yano #3570 部品マスタ編集画面　在庫棚卸対象・非対象設定の項目追加%>
        <th>在庫棚卸対象 *</th>
        <td><%=Html.CheckBox("NonInventoryFlag", (string.IsNullOrWhiteSpace(Model.NonInventoryFlag) || !Model.NonInventoryFlag.Equals("1"))) %></td>
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
       <tr>
        <% // Mod 2016/01/21 arc yano #3403_部品マスタ入力　純正区分、メーカー部品番号の必須項目化 (#3397_【大項目】部品仕入機能改善 課題管理表対応) メーカー部品番号を必須項目に %>
        <th>メーカー部品番号 *</th>
        <td><%=Html.TextBox("MakerPartsNumber", Model.MakerPartsNumber, new { @class = "alphanumeric", size = 25, maxlength = 25 })%></td>
      </tr>
      <tr>
        <th>メーカー部品名(日本語)</th>
        <td><%=Html.TextBox("MakerPartsNameJp", Model.MakerPartsNameJp, new { size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>メーカー部品名(英語)</th>
        <td><%=Html.TextBox("MakerPartsNameEn", Model.MakerPartsNameEn, new { @class = "alphanumeric", size = 50, maxlength = 50 })%></td>
      </tr>
      <tr>
        <th>販売価格</th>
        <td><%=Html.TextBox("SalesPrice", Model.SalesPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
          <%//Mod 2015/03/20 arc iijima 名称変更　原価→標準原価 %>
        <th>標準原価</th>
        <td><%=Html.TextBox("Cost", Model.Cost, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>クレーム申請部品代</th>
        <td><%=Html.TextBox("ClaimPrice", Model.ClaimPrice, new { @class = "numeric", maxlength = 10 })%></td>
      </tr>
      <% // Add 2014/11/06 arc nakayama 部品項目追加対応 %>
      <tr>
        <th>移動平均単価</th>
        <td><%=Html.TextBox("MoveAverageUnitPrice", ViewData["MoveAverageUnitPrice"], new {  @class = "readonly",　@readonly = "readonly", @style="text-align: right" , maxlength = 10 })%></td>
      </tr>
     <tr>
        <th>単位</th>
        <td><%=Html.TextBox("FixationQuantit", "１", new { @class = "readonly",　@readonly = "readonly",  @style="vertical-align:top;text-align: center", size = 2, maxlength = 10 })%> <%=Html.DropDownList("UnitCD1", (IEnumerable<SelectListItem>)ViewData["UnitCD1List"] )%> = <%=Html.TextBox("QuantityPerUnit1", Model.QuantityPerUnit1, new { @class = "numeric",size = 10, maxlength = 10 })%> <%=Html.TextBox("FixationUnit", "個", new { @class = "readonly",　@readonly = "readonly", @style="vertical-align:top;text-align: center", size = 2, maxlength = 10 })%></td> 
      </tr>
      
   </table>
</div>
<%} %>
<br />
</asp:Content>
