<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_CarPurchaseList>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	CarPurchaseList
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarPurchaseList", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id","0") %>
<%=Html.Hidden("DefaultPurchasePlanDateFrom", ViewData["DefaultPurchasePlanDateFrom"]) %>
<%=Html.Hidden("DefaultPurchasePlanDateTo", ViewData["DefaultPurchasePlanDateTo"]) %>
<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<table class="input-form">
    <tr>
        <th>入庫予定日</th>
        <td colspan="3"><%=Html.TextBox("PurchasePlanDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["PurchasePlanDateFrom"])) %>～<%=Html.TextBox("PurchasePlanDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["PurchasePlanDateTo"])) %></td>
    </tr>
    <tr>
        <th rowspan="2">部門</th>
        <td colspan="3">
            <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style="width:30px", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" }) %>
            <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0" }); %>
        </td>
    </tr>
    <tr>
        <td colspan="3">
            <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span>
        </td>
    </tr>
    <tr>
        <th>ステータス</th>
        <td><%=Html.DropDownList("PurchaseStatus",(IEnumerable<SelectListItem>)ViewData["PurchaseStatusList"]) %></td>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin",ViewData["Vin"]) %></td>
    </tr>
    <tr>
        <th>メーカー名</th>
        <td><%=Html.TextBox("MakerName",ViewData["MakerName"]) %></td>
        <th>車種名</th>
        <td><%=Html.TextBox("CarName",ViewData["CarName"]) %></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('PurchasePlanDateFrom','PurchasePlanDateTo','DepartmentCode','DepartmentName','PurchaseStatus','Vin','MakerName','CarName'))" />
        </td>
    </tr>
</table>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="white-space:nowrap">入庫予定日</th>
        <th style="white-space:nowrap">入庫日</th>
        <th style="white-space:nowrap">新中区分</th>
        <th style="white-space:nowrap">管理番号</th>
        <th style="white-space:nowrap">車台番号</th>
        <th style="white-space:nowrap">メーカー</th>
        <th style="white-space:nowrap">車種</th>
        <th style="white-space:nowrap">仕入先名</th>
        <th style="white-space:nowrap">仕入日</th>
        <th style="white-space:nowrap">種別</th>
        <th style="white-space:nowrap">ステータス</th>
    </tr>
    <%foreach(var item in Model){ %>
    <tr>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.PurchasePlanDate)) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.PurchaseDate)) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.NewUsedType) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.SalesCarNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.Vin) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.MakerName) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.CarName) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.SupplierName) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SlipDate)) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.RecordType) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(item.PurchaseStatusName) %></td>
    </tr>
    <%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
