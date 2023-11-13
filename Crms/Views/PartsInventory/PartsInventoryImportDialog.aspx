<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="CrmsDao" %> 
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品在庫実棚一括更新
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("ImportDialog", "PartsInventory", FormMethod.Post, new {enctype = "multipart/form-data"})) { %>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<div id="import-form">
<br />
<%//Mod 2015/04/27 arc yano IPO対応(部品棚卸) 更新ボタンクリック時にエラーメッセージを非表示にする %>
<div id ="validSummary">
    <%=Html.ValidationSummary()%>
</div>
<table class="input-form">
    <tr>
        <th style="width:100px">ファイルパス</th>
        <td><input type="file" name="importFile" size="50" />&nbsp<input type="button" value="更新" onclick = "document.getElementById('validSummary').style.display = 'none'; stopwatchClear(); stopwatchStart(); dispProgresseddispTimer('PartsInventory', 'UpdateMsg'); formSubmit();"/></td>

    </tr>
    <tr>
        <% //Mod 2015/04/27 arc yano IPO対応(部品棚卸) メッセージボックスを経過時間のみ表示するように変更 %>
        <th>
            経過時間
        </th>
        <%=Html.Hidden("ElapsedTime", ViewData["ElapsedTime"])%>
        <td>
            <div id="stopwatch" style ="font-weight:bold">
                <span id="stopwatchHour"><%=Html.Encode(ViewData["ElapsedHours"]) %></span>
                <span>:</span>
                <span id="stopwatchMinute"><%=Html.Encode(ViewData["ElapsedMinutes"]) %></span>
                <span>:</span>
                <span id="stopwatchSecond"><%=Html.Encode(ViewData["ElapsedSeconds"]) %></span>
            </div>
        </td>

    </tr>
</table>
</div>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />

</div>
    <%} %>
<br />
<br />
</asp:Content>
