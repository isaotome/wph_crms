<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceWork>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	主作業マスタ検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "ServiceWork", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">大分類</th>
        <td><%=Html.DropDownList("Classification1", (IEnumerable<SelectListItem>)ViewData["Classification1List"])%></td>
    </tr>
    <tr>
        <th>中分類</th>
        <td><%=Html.DropDownList("Classification2", (IEnumerable<SelectListItem>)ViewData["Classification2List"])%></td>
    </tr>
    <tr>
        <th>小分類コード</th>
        <td><%=Html.TextBox("ServiceWorkCode", ViewData["ServiceWorkCode"], new { @class = "alphanumeric", maxlength = 5 })%></td>
    </tr>
    <tr>
        <th>小分類名</th>
        <td><%=Html.TextBox("Name", ViewData["Name"], new { size = 40, maxlength = 20 })%></td>
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
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/ServiceWork/Entry')"/>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:30px"></th>
        <th>大分類</th>
        <th>中分類</th>
        <th>小分類コード</th>
        <th>小分類名</th>
        <th>サービス料金</th>
        <th>ステータス</th>
    </tr>
    <%foreach (var serviceWork in Model)
      { %>
    <tr>
        <td><a href="javascript:openModalAfterRefresh('/ServiceWork/Entry/' + '<%=serviceWork.ServiceWorkCode%>')">詳細</a></td>
        <td><%=Html.Encode(serviceWork.Classification1)%>&nbsp;<%if (serviceWork.c_ServiceWorkClass1 != null) {%><%=Html.Encode(serviceWork.c_ServiceWorkClass1.Name)%><%} %></td>
        <td><%=Html.Encode(serviceWork.Classification2)%>&nbsp;<%if (serviceWork.c_ServiceWorkClass2 != null) {%><%=Html.Encode(serviceWork.c_ServiceWorkClass2.Name)%><%} %></td>
        <td><%=Html.Encode(serviceWork.ServiceWorkCode)%></td>
        <td><%=Html.Encode(serviceWork.Name)%></td>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", serviceWork.Price))%></td>
        <td><%=Html.Encode(serviceWork.DelFlagName)%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
