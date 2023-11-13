<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Campaign>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	イベント検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "Campaign", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">イベントコード</th>
        <td><%=Html.TextBox("CampaignCode", ViewData["CampaignCode"], new { @class = "alphanumeric", maxlength = 20 })%></td>
    </tr>
    <tr>
        <th>イベント名</th>
        <td><%=Html.TextBox("CampaignName", ViewData["CampaignName"], new { size = 50, maxlength = 100 })%></td>
    </tr>
    <tr>
        <th>担当者名</th>
        <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { size = 50, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>開始日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("CampaignStartDateFrom", ViewData["CampaignStartDateFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("CampaignStartDateTo", ViewData["CampaignStartDateTo"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th>終了日(YYYY/MM/DD)</th>
        <td><%=Html.TextBox("CampaignEndDateFrom", ViewData["CampaignEndDateFrom"], new { @class = "alphanumeric", maxlength = 10 })%>&nbsp;&nbsp;～&nbsp;&nbsp;<%=Html.TextBox("CampaignEndDateTo", ViewData["CampaignEndDateTo"], new { @class = "alphanumeric", maxlength = 10 })%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
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
        <th style="width:30px"></th>
        <th>イベント名</th>
        <th>開始日</th>
        <th>終了日</th>
    </tr>
    <%foreach (var campaign in Model)
      { %>
    <tr>
        <td><a href="javascript:void(0);" onclick="selectedCriteriaDialog('<%=campaign.CampaignCode%>', 'CampaignName');return false;">詳細</a></td>
        <td><span id="<%="CampaignName_" + campaign.CampaignCode%>"><%=Html.Encode(campaign.CampaignName)%></span></td>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", campaign.CampaignStartDate))%></td>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", campaign.CampaignEndDate))%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
