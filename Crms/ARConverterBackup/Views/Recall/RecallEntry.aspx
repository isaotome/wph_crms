<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	リコール編集
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<div style="text-align:center;">
<table border="0" cellpadding="5" cellspacing="5" style="text-align:center;">
    <tr>
        <td style="width:60px"><img alt="閉じる" style="cursor:pointer" src="/Content/Images/exit.png" onclick="formClose()" /></td>
        <td style="width:60px"><img alt="保存" style="cursor:pointer" src="/Content/Images/build.png" onclick="document.frmCompanyEntry.submit();" /></td>
    </tr>
    <tr>
        <td style="width:60px">閉じる</td>
        <td style="width:60px">保存</td>
    </tr>
</table>
</div>

<div id="input-form">
    <table class="input-form" style="width:700px">
      <tr>
        <th style="width:100px">リコール番号</th>
        <td style="width:200px"><%=Html.TextBox("RecallCode") %></td>
      </tr>
      <tr>
        <th style="width:100px">対象部位部品</th>
        <td style="width:200px"><%=Html.TextBox("Description") %></td>
      </tr>
      <tr>
        <th style="width:100px">届出日</th>
        <td style="width:200px"><%=Html.TextBox("ReportDate") %></td>
      </tr>
    </table>
</div>
</asp:Content>
