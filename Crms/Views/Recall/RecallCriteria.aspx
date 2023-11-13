<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	リコール検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form" style="display:none">
<br />
    <table class="input-form">
    <tr>
        <th>リコール番号</th>
        <td><%=Html.TextBox("RecallCode", ViewData["RecallCode"])%></td>
    </tr>
    <tr>
        <th>対象部位部品</th>
        <td><%=Html.TextBox("Description", ViewData["Description"])%></td>
    </tr>
    <tr>
        <th>届出日</th>
        <td><%=Html.TextBox("ReportDate", ViewData["ReportDate"])%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="submit" value="検索" /></td>
    </tr>
</table>
</div>
<br />
<br />
<input type="button" value="インポート" />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th style="width:100px">リコール番号</th>
        <th>対象部位部品</th>
        <th style="width:150px">届出日</th>
        <th>対象車両数</th>
        <th>実施済</th>
        <th>未実施</th>
    </tr>
    <tr>
        <td><%=Html.ActionLink("詳細","Entry","Recall",null,new {target="_blank"})%></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
    </tr>
</table>
<br />


</asp:Content>
