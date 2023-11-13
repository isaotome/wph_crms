<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceWork>" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	主作業マスタ入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
// -----------------------------------------------------------------------------------------------------
// Mod 022/01/24 yano #4122【主作業マスタ入力】入力項目（新車・中古車選択）を追加
// Mod 2022/01/24 yano #4124 権限による保存ボタンの表示、非表示の切替
// Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
//                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
// ----------------------------------------------------------------------------------------------------- 
%>
    <%using (Html.BeginForm("Entry", "ServiceWork", FormMethod.Post))
      {
    %>
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
       <%//Mod 2022/01/24 yano #4124 %>
       <% if (ViewData["ButtonVisible"] != null && (bool)ViewData["ButtonVisible"].Equals(true)){%>
            <td onclick="formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn"/>&nbsp;保存</td>
       <%} %>
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
        <th style="width:120px">主作業コード(小分類) *</th>
          <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td><%if (CommonUtils.DefaultString(ViewData["update"]).Equals("1"))
              { %><input type="text" id="ServiceWorkCode" name="ServiceWorkCode" value="<%=Model.ServiceWorkCode%>" readonly="readonly" /><%}
              else
              { %><%=Html.TextBox("ServiceWorkCode", Model.ServiceWorkCode, new { @class = "alphanumeric", maxlength = 5, onblur = "IsExistCode('ServiceWorkCode','ServiceWork')" })%><%} %>
        </td>
      </tr>
      <tr>
        <th>主作業名(小分類) *</th>
        <td><%=Html.TextBox("Name", Model.Name, new { size = 40, maxlength = 20 })%></td>
      </tr>
      <tr>
        <th>サービス料金</th>
        <td><%=Html.TextBox("Price", string.Format("{0:N0}", Model.Price), new { @class = "money", maxlength = 10 })%></td>
      </tr>
      <tr>
        <th>大分類</th>
        <td><%=Html.DropDownList("Classification1", (IEnumerable<SelectListItem>)ViewData["Classification1List"])%></td>
      </tr>
      <tr>
        <th>中分類</th>
        <td><%=Html.DropDownList("Classification2", (IEnumerable<SelectListItem>)ViewData["Classification2List"])%></td>
      </tr>
      <tr><%//Mod 2016/04/15 arc yano #3480 %>
        <th>請求先分類</th>
        <td><%=Html.DropDownList("CustomerClaimClass", (IEnumerable<SelectListItem>)ViewData["CustomerClaimClassList"])%></td>
      </tr>
       <tr><%//Add2016/04/15 arc yano #3480 %>
        <th>新中区分(サービス売上集計用)</th>
        <td><%=Html.DropDownList("AccountClassCode", (IEnumerable<SelectListItem>)ViewData["AccountClassList"])%></td>
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
