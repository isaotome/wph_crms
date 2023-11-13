<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
        <%
        if (ViewData["id"].Equals("1"))
        {%>
            車検満了日一括更新
        <%}else{%>
            次回点検日一括更新
        <%}%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
/*--------------------------------------------------------------*/
/* 車検満了日・次回点検日一括更新                               */
/* 2018/05/25 arc yano #3888 Excel取込(車検点検更新)            */
/*--------------------------------------------------------------*/        
%>
    <%using (Html.BeginForm("ImportDialog", "InspectGuidList", FormMethod.Post, new { enctype = "multipart/form-data" }))
      {%>
    <%=Html.Hidden("ErrFlag", ViewData["ErrFlag"]) %>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:読み込み / 2:取り込み )%>
    <%=Html.Hidden("id", ViewData["id"]) %>
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
        <td><input type="file" name="importFile" size="70" />&nbsp;<input type="button" value="取込実行" onclick = "if (confirm('取込を実行します。よろしいですか')) { document.getElementById('validSummary').style.display = 'none'; document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit(); }"/>&nbsp;<input type="button" value="キャンセル" onclick="    document.forms[0].enctype.value = 'application/x-www-form-urlencoded'; document.getElementById('RequestFlag').value = '2'; formSubmit()" /></td>
    </tr>
</table>
</div>
<br />
    <%} %>
</asp:Content>