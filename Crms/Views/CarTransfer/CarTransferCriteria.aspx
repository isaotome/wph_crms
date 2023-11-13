<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Transfer>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	車両移動検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","CarTransfer",FormMethod.Post)) { %>
<%=Html.Hidden("id","0") %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
 <table class="input-form">
    <tr>
        <th style="width:100px">出庫日</th>
        <td style="width:300px"><%=Html.TextBox("DepartureDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["DepartureDateFrom"]),new {@class="alphanumeric",size="10",maxlength="10"}) %> ～ <%=Html.TextBox("DepartureDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["DepartureDateTo"]),new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
        <th style="width:100px">入庫予定日</th>
        <td style="width:300px"><%=Html.TextBox("ArrivalDateFrom",string.Format("{0:yyyy/MM/dd}",ViewData["ArrivalDateFrom"]),new {@class="alphanumeric",size="10",maxlength="10"}) %> ～ <%=Html.TextBox("ArrivalDateTo",string.Format("{0:yyyy/MM/dd}",ViewData["ArrivalDateTo"]),new {@class="alphanumeric",size="10",maxlength="10"}) %></td>
    </tr>

    <tr>
        <th rowspan="2">出庫ロケーション</th>
        <td><%=Html.TextBox("DepartureLocationCode", ViewData["DepartureLocationCode"], new { @class="alphanumeric", maxlength="12", onblur = "GetNameFromCode('DepartureLocationCode','DepartureLocationName','Location')" })%>&nbsp;<img alt="出庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartureLocationCode','DepartureLocationName','/Location/CriteriaDialog?BusinessType=001,009')" /></td>
        <th rowspan="2">入庫ロケーション</th>
        <td><%=Html.TextBox("ArrivalLocationCode", ViewData["ArrivalLocationCode"], new { @class = "alphanumeric", maxlength = "12", onblur = "GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')" })%>&nbsp;<img alt="入庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ArrivalLocationCode','ArrivalLocationName','/Location/CriteriaDialog?BusinessType=001,009')" /></td>
    </tr>
    <tr>
        <td><span id="DepartureLocationName"><%=CommonUtils.DefaultNbsp(ViewData["DepartureLocationName"],1) %></span></td>
        <td><span id="ArrivalLocationName"><%=CommonUtils.DefaultNbsp(ViewData["ArrivalLocationName"],1) %></span></td>
    </tr>
    <tr>
        <th>車台番号</th>
        <td><%=Html.TextBox("Vin",ViewData["Vin"],new {@class="alphanumeric",size="15",maxlength="20"}) %></td>
        <th>入庫ステータス</th>
        <td style="width:200px"><%=Html.CheckBox("TransferConfirm",ViewData["TransferConfirm"]) %>確定&nbsp;<%=Html.CheckBox("TransferUnConfirm",ViewData["TransferUnConfirm"]) %>未確定</td>
    </tr>
     <tr>
      <th>移動種別</th>
      <td colspan="3"><%=Html.DropDownList("TransferType",(IEnumerable<SelectListItem>)ViewData["TransferTypeList"]) %></td>
    </tr>
    <tr>
      <th style="width:100px">&nbsp;</th>
      <td colspan="3">
        <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
        <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
        <input type="button" value="検索" onclick="DisplayImage('UpdateMsg','0');displaySearchList()" />
        <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DepartureDateFrom','DepartureDateTo','ArrivalDateFrom','ArrivalDateTo','DepartureLocationCode','DepartureLocationName','ArrivalLocationCode','ArrivalLocationName','Vin','TransferConfirm','TransferUnConfirm','TransferType'))" />
      </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30"/>
</div>
    <br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarTransfer/Entry')" />&nbsp;<input type="button" value="回送依頼書" onclick="alert('工事中です')" />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <th style="width:25px;white-space:nowrap">&nbsp;</th>
        <th style="white-space:nowrap">移動伝票番号</th>
        <th style="width:80px;white-space:nowrap">出庫日</th>
        <th style="width:100px;white-space:nowrap">出庫ロケーション</th>
        <th style="width:80px;white-space:nowrap">入庫予定日</th>
        <th style="width:80px;white-space:nowrap">入庫日</th>
        <th style="width:100px;white-space:nowrap">入庫ロケーション</th>
        <th style="width:80px;white-space:nowrap">ブランド</th>
        <th style="width:80px;white-space:nowrap">車種</th>
        <th>グレード</th>
		<th style="width:100px">車台番号</th>
    </tr>
    <%foreach (var a in Model) { %>
    <%
        string brandName="", carName="", gradeName="", vin="", arrivalLocationName="", departureLocationName="";
          try { brandName = a.SalesCar.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
          try { carName = a.SalesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
          try { gradeName = a.SalesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
          try { vin = a.SalesCar.Vin; } catch (NullReferenceException) { }
          try { departureLocationName = a.DepartureLocation.LocationName; } catch (NullReferenceException) { }
          try { arrivalLocationName = a.ArrivalLocation.LocationName; } catch (NullReferenceException) { }
    %>
    <tr>
        <td style="white-space:nowrap"><input type="checkbox" /></td>
        <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarTransfer/Entry/<%=a.TransferNumber %>')" ><%=CommonUtils.DefaultNbsp(a.TransferNumber) %></a></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",a.DepartureDate)) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(departureLocationName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",a.ArrivalPlanDate)) %></td>
        <td style="white-space:nowrap"><%if(a.ArrivalDate==null){ %><input type="button" value="入庫処理" style="width:65px" onclick="openModalAfterRefresh('/CarTransfer/Confirm/<%=a.TransferNumber %>')" /><%}else{ %><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",a.ArrivalDate)) %><%} %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(arrivalLocationName)%></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(brandName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(carName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(gradeName) %></td>
        <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(vin) %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
