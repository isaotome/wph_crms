<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SetMenu>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    セットメニュー検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm("CriteriaDialog","SetMenu",new { id = 0 } ,FormMethod.Post)){ %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>セットメニューコード</th>
        <td><%=Html.TextBox("SetMenuCode",ViewData["SetMenuCode"],new {@class="alphanumeric",size="15",maxlength="11"}) %></td>
    </tr>
    <tr>
        <th>セットメニュー名</th>
        <% // Mod 2014/07/15 arc amii 既存バグ対応 DB項目のlengthと一致していなかった為、入力可能文字数を100→50に修正 %>
        <td><%=Html.TextBox("SetMenuName",ViewData["SetMenuName"],new {size="20",maxlength="50"}) %></td>
    </tr>
    <tr>
        <th>会社コード</th>
        <td><%=Html.TextBox("CompanyCode",ViewData["CompanyCode"],new {@class="alphanumeric",size="15",maxlength="3"}) %></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()" /></td>
    </tr>
</table>
</div>
<%} %>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<table class="list">
    <tr>
        <th></th>
        <th style="width:150px">セットメニューコード</th>
        <th>セットメニュー名</th>
        <% // Mod 2014/07/15 arc amii chrome対応 会社コードの折り返しがIEと異なるのを修正 %>
        <th style="width:30px">会社コード</th>
        <th>会社名</th>
    </tr>
    <%foreach(SetMenu a in Model){ %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=a.SetMenuCode %>','SetMenuName')">選択</a></td>
        <td><%=CommonUtils.DefaultNbsp(a.SetMenuCode) %></td>
        <td><span id="SetMenuName_<%=a.SetMenuCode%>"><%=CommonUtils.DefaultNbsp(a.SetMenuName) %></span></td>
        <td><%=CommonUtils.DefaultNbsp(a.CompanyCode) %></td>
        <td><%=CommonUtils.DefaultNbsp(a.Company!=null ? a.Company.CompanyName : "") %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
