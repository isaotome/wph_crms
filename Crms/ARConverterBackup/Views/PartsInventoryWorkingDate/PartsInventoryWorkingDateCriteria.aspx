<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.PartsInventoryWorkingDate>>"%>  
    
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    棚卸基準日登録
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<%//2017/05/10 arc yano #3762 車両棚卸機能追加 車両と部品で共通で使用するため、部品の文言を削除%>

    <%using (Html.BeginForm("Criteria", "PartsInventoryWorkingDate", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("EntryButton", ViewData["EntryButton"]) %><%//登録ボタンの活性/非活性%>
<div id="search-form">
<br />
    <%=Html.ValidationSummary() %>
    <table class="input-form">
    <tr>
        <%//Mod 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 最新確定月の次の年月を表示 %>
        <th>対象年月</th>
        <td><%=Html.TextBox("TargetInventoryMonth", ViewData["TargetInventoryMonth"],"{0:yyyy/MM}", new { @class = "readonly", @readonly = "readonly", size = 10, maxlength = 10 })%></td>
        <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更-->
        <th style="width:100px">棚卸基準日 *</th>
        <td><%=Html.TextBox("InputInventoryWorkingDate", ViewData["InputInventoryWorkingDate"], new { @class = "alphanumeric", size = 10, maxlength = 10 ,onchange ="return chkDate3(document.getElementById('InputInventoryWorkingDate').value, 'InputInventoryWorkingDate')"})%></td>
    </tr>
    <tr>
        <th></th>
        <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更-->
        <td colspan="3">※通常は、棚卸基準日は月末の最終日となります。<br />
                        　 指定した日の24：00時点の在庫を数えます。
        </td>
    </tr>
    </table>
</div>
<br />
<br />
<%//Mod 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 最新確定月の次の月の棚卸状況が未実施なら[登録]ボタンを活性にする %>
<%if(ViewData["EntryButton"] == "0"){ %>
    <input type="button" value="登録" disabled="disabled" />
<%}else{ %>
    <input type="button" value="登録" onclick="ConfirmationInventoryMonth()"/>
    <%} %>
<br />
<br />
<br />
<table class="input-form">

    <%//結果を表示する%>
    <%//Mod 2015/04/22 arc nakayama 部品棚卸作業日登録画面修正 対象年月も表示する%>
    <%foreach (var PartsInventoryWorkingDate in Model)
    { %>
    <tr>
      <th style="width: 100px">対象年月</th>
      <td><%=Html.TextBox("InventoryMonth", PartsInventoryWorkingDate.InventoryMonth,"{0:yyyy/MM}", new { @class = "readonly", @readonly = "readonly", size = 10, maxlength = 10 })%></td>
    </tr>
    <tr>
      <!--Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更-->
      <th style="width: 100px">棚卸基準日</th>
        <td><%=Html.TextBox("InventoryWorkingDate", PartsInventoryWorkingDate.InventoryWorkingDate,  "{0:d}",new {  @class = "readonly",　@readonly = "readonly", @style="text-align:left" ,  size = 10})%></td>
    </tr>
    <%} %>
</table>
        <%} %>
</asp:Content>

