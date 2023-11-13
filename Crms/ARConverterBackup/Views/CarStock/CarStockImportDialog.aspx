<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    財務価格一括取込
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%-- ----------------------------------------------------------------
   機能：財務価格取込
　 作成日：2018/06/06 arc yano #3883 タマ表改善 新規作成
------------------------------------------------------------------ --%>

    <%using (Html.BeginForm("ImportDialog", "CarStock", FormMethod.Post, new { enctype = "multipart/form-data" }))
      { %>
    <%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:読み込み / 2:取り込み )%>
<table class="command">
   <tr>
       <td onclick="window.close();"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<div id="import-form">
<br />
<div id ="validSummary">
    <%=Html.ValidationSummary()%>
</div>
<br />
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<table class="input-form">
    <tr>
        <th style="white-space:nowrap;">ファイルパス</th>
        <td><input type="file" name="importFile" size="70" /></td>
    </tr>
    <tr>
        <td colspan="2" style="text-align:right"><input type="button" value="取込実行" onclick = "if (confirm('取込を実行します。よろしいですか')) { document.getElementById('validSummary').style.display = 'none'; document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit(); }"/>&nbsp;<input type="button" value="キャンセル" onclick="    document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '2'; formSubmit()" /></td>
    </tr>
    
</table>
</div>
<br />
    <%} %>
</asp:Content>