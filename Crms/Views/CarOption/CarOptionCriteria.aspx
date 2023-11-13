﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetCarOptionMaster_Result>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	オプションマスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarOption", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">メーカーコード</th>
        <td><%=Html.TextBox("MakerCode", ViewData["MakerCode"], new { @class = "alphanumeric", maxlength = 5 })%></td>
    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName", ViewData["MakerName"], new { size = 50, maxlength = 50 })%></td>
    </tr>
    <tr>
        <th>オプションコード</th>
        <td><%=Html.TextBox("CarOptionCode", ViewData["CarOptionCode"], new { @class = "alphanumeric", maxlength = 25 })%></td>
    </tr>
    <tr>
        <th>オプション名</th>
        <td><%=Html.TextBox("CarOptionName", ViewData["CarOptionName"], new { size = 50, maxlength = 100 })%></td>
    </tr>
    <!--Add 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定-->
    <tr>
        <th>グレードコード</th>
        <td><%=Html.TextBox("CarGradeCode", ViewData["CarGradeCode"], new { @class = "alphanumeric", size = 30, maxlength = 30, onblur = "GetNameFromCode('CarGradeCode','CarGradeName','CarGrade')" })%>&nbsp;<img alt="車種検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CarGradeCode', 'CarGradeName', '/CarGrade/CriteriaDialog')" /><span id="CarGradeName" style ="width:160px"><%=Html.Encode(ViewData["CarGradeName"]) %></span></td>
    </tr>
    <tr>
        <th>必須設定</th>
        <td><%=Html.RadioButton("RequiredFlag", "9", ViewData["RequiredFlag"])%>全て<%=Html.RadioButton("RequiredFlag", "1", ViewData["RequiredFlag"])%>必須<%=Html.RadioButton("RequiredFlag", "0", ViewData["RequiredFlag"])%>任意</td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.RadioButton("DelFlag", "9", ViewData["DelFlag"])%>全て<%=Html.RadioButton("DelFlag", "0", ViewData["DelFlag"])%>有効<%=Html.RadioButton("DelFlag", "1", ViewData["DelFlag"])%>無効</td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<%} %>

<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarOption/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>メーカー</th>
        <th>オプションコード</th>
        <th>オプション名</th>
        <th>グレード名</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var carOption in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/CarOption/Entry/' + '<%=carOption.CarOptionCode%>')">詳細</a></td>
        <td><%=Html.Encode(carOption.MakerCode)%>&nbsp;<%if (carOption.MakerName != null){%><%=Html.Encode(carOption.MakerName)%><%} %></td>
        <td><%=Html.Encode(carOption.CarOptionCode)%></td>
        <td><%=Html.Encode(carOption.CarOptionName)%></td>
        <td><%=Html.Encode(carOption.CarGradeName)%></td>
        <td><%=Html.Encode(carOption.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
