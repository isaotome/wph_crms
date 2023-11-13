<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.SalesCar>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両利用用途検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% //Add 2014/10/30 arc amii 車両ステータス追加対応 %>
<%CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%using (Html.BeginForm("Criteria", "CarUsageSetting", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.Hidden("id", "0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">管理番号</th>
        <td><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { @class = "alphanumeric", size = 10, maxlength = 50 })%></td>
        <th style="width:100px">車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"], new { size = 10, maxlength = 20 , style= "width:150px"})%></td>
    </tr>
    <tr>
        <th>在庫ステータス</th>
        <td><%=Html.DropDownList("CarStatus", (IEnumerable<SelectListItem>)ViewData["CarStatusList"])%></td>
        <th>利用用途</th>
        <td><%=Html.DropDownList("CarUsage", (IEnumerable<SelectListItem>)ViewData["CarUsageList"])%></td>
    </tr>
<!--
    <tr>
        <th>所有者コード</th>
        <td><%=Html.TextBox("OwnerCode", ViewData["OwnerCode"], new { size = 10, maxlength = 40, @class = "alphanumeric", onblur = "GetNameFromCode('OwnerCode','PossesorName','Customer')" })%>&nbsp;<img alt="所有者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('OwnerCode','PossesorName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('PossesorName').value))" /></td>
        <th>所有者名</th>
        <td><%=Html.TextBox("PossesorName", ViewData["PossesorName"], new { size = 10, maxlength = 40 })%></td>
    </tr>
    <tr>
        <th>使用者コード</th>
        <td><%=Html.TextBox("UserCode", ViewData["UserCode"], new { size = 10, maxlength = 40,  @class = "alphanumeric", onblur = "GetNameFromCode('UserCode','UserName','Customer')"})%>&nbsp;<img alt="使用者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('UserCode','UserName','/Customer/CriteriaDialog/?CustomerName='+encodeURIComponent(document.getElementById('UserName').value))" /></td>
        <th>使用者名</th>
        <td><%=Html.TextBox("UserName", ViewData["UserName"], new { size = 10, maxlength = 80 })%></td>
    </tr>
-->
    <tr>
        <th>所有者名</th>
        <td><%=Html.TextBox("PossesorName", ViewData["PossesorName"], new { size = 10, maxlength = 80 })%></td>
        <th>使用者名</th>
        <td><%=Html.TextBox("UserName", ViewData["UserName"], new { size = 10, maxlength = 80 })%></td>
    </tr>
     <tr>
        <th></th>
        <td colspan="3" style="white-space:nowrap">
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SalesCarNumber', 'Vin', 'CarStatus', 'CarUsage', 'UserName', 'PossesorName'));" />
            <input type="button" value="変更履歴" onclick="openModalAfterRefresh('/CarUsageSetting/History/'); return false;" /><%// 2015/02/18 arc yano 車両用途変更(変更履歴画面追加)%>
        </td>
    </tr>
</table>
</div>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<% 
  //Add 2014/10/30 arc amii 車両ステータス追加対応
  CodeDao dao = new CodeDao(db);
  // c_CodeNameの指定した区分のデータを取得
  List<c_CodeName> list = dao.GetCodeName("004", false);
     %>
<table class="list" style ="width:1485px">
    <tr>
        <th style="width:90px">在庫ステータス</th>
        <th style="width:80px">管理番号</th>
        <th style="width:150px">車台番号</th>
        <th style="width:75px">利用用途</th>
        <th style="width:170px">在庫ロケーション</th>
        <th style="width:160px">車種名</th>
        <th style="width:380px">所有者</th>
        <th style="width:380px">使用者</th>
    </tr>
    <%foreach (var salesCar in Model)
      { %>

        <% 
           //Add 2014/10/30 arc amii 車両ステータス追加対応
           // c_CodeNameから 利用用途名を取得する
           IEnumerable<c_CodeName> codeName = list.Where(n => n.Code.Equals(CommonUtils.DefaultString(salesCar.CarUsage)));
           string carUsageName = "";

           foreach (var data in codeName)
           {
               carUsageName = data.Name;
           }
        %>
    <tr>
        <td><%if (!string.IsNullOrEmpty(salesCar.CarStatus)){%><%=Html.Encode(salesCar.c_CarStatus.Name)%><%} %></td>
        <% //Mod 2014/10/30 arc amii 車両ステータス追加対応 %>
        <td style ="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarUsageSetting/Entry/' + '<%=salesCar.SalesCarNumber.ToString() %>');return false;" /><%=CommonUtils.DefaultString(salesCar.SalesCarNumber)%></td>
        <%--<td><a><%=Html.Encode(salesCar.SalesCarNumber)%></a></td>--%>
        <td style ="white-space:nowrap"><%=Html.Encode(salesCar.Vin)%></td>
        <% //Mod 2014/10/30 arc amii 車両ステータス追加対応 %>
        <td><%if (!string.IsNullOrEmpty(salesCar.CarUsage)){%><%=Html.Encode(carUsageName)%><%} %></td>
        <%--<td><%if (salesCar.CarUsage != null){%><%=Html.Encode()%><%} %></td>--%>
        <td><%if (salesCar.Location != null) {%><%=Html.Encode(salesCar.Location.LocationName)%> <%} %></td>
        <td><%try { %><%=Html.Encode(salesCar.CarGrade.Car.CarName)%><%} catch (NullReferenceException) { } %></td>
        <td><%=Html.Encode(salesCar.PossesorName)%></td>
        <td><%=Html.Encode(salesCar.UserName)%></td>
    </tr>
    <%} %>
</table>
<br />
<%} %>
</asp:Content>
