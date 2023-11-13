<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.ServiceWork>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	主作業検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("CriteriaDialog", "ServiceWork", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<% //2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業大分類の絞込み情報を追加 %>
<%=Html.Hidden("CCCustomerClaimClass", ViewData["CCCustomerClaimClass"]) %>
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
        <th>大分類</th>
        <th>中分類</th>
        <th>小分類コード</th>
        <th>小分類名</th>
        <th>サービス料金</th>
    </tr>
    <%foreach (var serviceWork in Model)
      { %>
    <tr>
        <td><a href="javascript:selectedCriteriaDialog('<%=serviceWork.ServiceWorkCode %>','Name')">選択</a></td>
        <td><%=Html.Encode(serviceWork.Classification1)%>&nbsp;<%if (serviceWork.c_ServiceWorkClass1 != null) {%><%=Html.Encode(serviceWork.c_ServiceWorkClass1.Name)%><%} %></td>
        <td><%=Html.Encode(serviceWork.Classification2)%>&nbsp;<%if (serviceWork.c_ServiceWorkClass2 != null) {%><%=Html.Encode(serviceWork.c_ServiceWorkClass2.Name)%><%} %></td>
        <td><%=Html.Encode(serviceWork.ServiceWorkCode)%></td>
        <td><span id="<%="Name_" + serviceWork.ServiceWorkCode%>"><%=Html.Encode(serviceWork.Name)%></span></td>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", serviceWork.Price))%></td>
    </tr>
    <%} %>
</table>
<br />
</asp:Content>
