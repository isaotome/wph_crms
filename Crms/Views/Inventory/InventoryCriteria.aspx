<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_CarInventorySummary>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両棚卸検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "Inventory", new { id = 0 }, FormMethod.Post, new { name = "dummy" })) { %>
<%=Html.Hidden("id", "0")%>
<%=Html.Hidden("actionType", "")%>
<%=Html.Hidden("fixDepartment", "")%>
<%=Html.Hidden("fixMonth", "")%>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <%string securityLevelCode = "";
      try { securityLevelCode = CommonUtils.DefaultString(((Employee)Session["Employee"]).SecurityRole.SecurityLevelCode); } catch (NullReferenceException) { } %>
    <tr>
        <th style="width:100px" rowspan="2">会社</th>
        <td><%switch (securityLevelCode) {
                  case "002":%><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @readonly = true, onfocus = "document.forms[0].DepartmentCode.focus();" })%>
                           <%break; %>
            <%case "003":%><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @readonly = true, onfocus = "document.forms[0].OfficeCode.focus();" })%>
                           <%break; %>
            <%case "004":%><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('CompanyCode','CompanyName','Company')" })%>
                           <img alt="会社検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CompanyCode', 'CompanyName', '/Company/CriteriaDialog')" />
                           <%break; %>
            <%default:%><%=Html.TextBox("CompanyCode", ViewData["CompanyCode"], new { @readonly = true, onfocus = "document.forms[0].InventoryMonth.focus();" })%>
                           <%break; %>
            <%} %>
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="CompanyName"><%=Html.Encode(ViewData["CompanyName"])%></span></td>       
    </tr>
    <tr>
        <th style="width:100px" rowspan="2">事業所</th>
        <td><%switch (securityLevelCode) {
                  case "002":%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @readonly = true, onfocus = "document.forms[0].DepartmentCode.focus();" })%>
                           <%break; %>
            <%case "003":
              case "004":%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('OfficeCode','OfficeName','Office')" })%>
                           <img alt="事業所検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('OfficeCode', 'OfficeName', '/Office/CriteriaDialog')" />
                           <%break; %>
            <%default:%><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @readonly = true, onfocus = "document.forms[0].InventoryMonth.focus();" })%>
                           <%break; %>
            <%} %>
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="OfficeName"><%=Html.Encode(ViewData["OfficeName"])%></span></td>       
    </tr>
    <tr>
        <th style="width:100px" rowspan="2">部門 *</th>
        <td><%switch (securityLevelCode) {
                  case "002":
                  case "003":
                  case "004":%><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>
                           <img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode', 'DepartmentName', '/Department/CriteriaDialog')" />
                           <%break; %>
            <%default:%><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @readonly = true, onfocus = "document.forms[0].InventoryMonth.focus();" })%>
                           <%break; %>
            <%} %>
        </td>
    </tr>
    <tr>
        <td style="height:20px"><span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"])%></span></td>       
    </tr>
    <tr>
        <th>棚卸月</th>
        <td><%=Html.DropDownList("InventoryMonth", (IEnumerable<SelectListItem>)ViewData["InventoryMonthList"])%></td>
    </tr>
    <tr>
        <th></th>
        <td><input type="button" value="検索" onclick="displaySearchList()"/></td>
    </tr>
</table>
</div>
<br />
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:50px">締め処理</th>
        <th style="width:50px">原票</th>
        <th style="width:50px">差異表</th>
        <th style="width:50px">実棚</th>
        <th>部門</th>
        <th style="width:100px">ステータス</th>
        <th style="width:50px">差異数</th>
        <th style="width:100px">最終更新日</th>
    </tr>
    <%foreach (var v_CarInventorySummary in Model) { %>
    <tr>
        <%bool canComplete = ((!CommonUtils.DefaultString(v_CarInventorySummary.InventoryStatus).Equals("003")) && (v_CarInventorySummary.DifferentialQuantity == 0));
          bool canInput = ((!CommonUtils.DefaultString(v_CarInventorySummary.InventoryStatus).Equals("003")) && (v_CarInventorySummary.DifferentialQuantity != 0));
          string fixDisable = (canComplete ? "" : "disabled=\"disabled\"");%>
        <td><input type="button" value="実行" onclick="document.forms[0].actionType.value='fixInventory';document.forms[0].fixDepartment.value='<%=v_CarInventorySummary.DepartmentCode %>';document.forms[0].fixMonth.value='<%=string.Format("{0:yyyy/MM}", v_CarInventorySummary.InventoryMonth) %>';formSubmit();" <%=fixDisable %> style="width:50px" /></td>
        <td><a href="javascript:void();" onclick="document.forms[0].reportName.value='CarInventorySrc';document.forms[0].reportParam.value='<%=v_CarInventorySummary.DepartmentCode %>,<%=string.Format("{0:yyyyMM}", v_CarInventorySummary.InventoryMonth) %>';printReport();return false;">印刷</a></td>
        <td><a href="javascript:void();" onclick="document.forms[0].reportName.value='CarInventoryDiff';document.forms[0].reportParam.value='<%=v_CarInventorySummary.DepartmentCode %>,<%=string.Format("{0:yyyyMM}", v_CarInventorySummary.InventoryMonth) %>';printReport();return false;">印刷</a></td>
        <td><%if (canInput) { %><a href="javascript:void();" onclick="openModalAfterRefresh2('/Inventory/Entry/<%=v_CarInventorySummary.DepartmentCode %>,<%=string.Format("{0:yyyyMM}", v_CarInventorySummary.InventoryMonth) %>');return false;">入力</a><%} else { %>入力<%} %></td>
        <td><%=Html.Encode(v_CarInventorySummary.DepartmentName)%></td>
        <td><%=Html.Encode(v_CarInventorySummary.InventoryStatusName)%></td>
        <td style="text-align:right"><%=Html.Encode(string.Format("{0:N0}", v_CarInventorySummary.DifferentialQuantity))%></td>
        <td><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", v_CarInventorySummary.LastUpdateDate))%></td>
    </tr>
    <%} %>
</table>
<br />
<%=Html.Hidden("reportName", "")%>
<%=Html.Hidden("reportParam", "")%>
<%} %>
</asp:Content>
