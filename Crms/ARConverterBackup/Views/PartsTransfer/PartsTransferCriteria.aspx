<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.Transfer>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品移動
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using(Html.BeginForm()){ %>
<%=Html.Hidden("id","0") %>
<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%//Mod 2016/06/20 arc yano #3584 部品入荷一覧 検索条件(受注伝票番号)の追加%>
 <table class="input-form">
    <tr>
        <th style="width:100px">出庫日</th>
        <td style="width:300px"><%=Html.TextBox("DepartureDateFrom", ViewData["DepartureDateFrom"], new { size = "10", @class = "alphanumeric" })%> ～ <%=Html.TextBox("DepartureDateTo", ViewData["DepartureDateTo"], new { size = "10", @class = "alphanumeric" })%></td>
        <th style="width:100px">入庫予定日</th>
        <td style="width:300px"><%=Html.TextBox("ArrivalDateFrom", ViewData["ArrivalDateFrom"], new { size = "10", @class = "alphanumeric" })%> ～ <%=Html.TextBox("ArrivalDateTo", ViewData["ArrivalDateTo"], new { size = "10", @class = "alphanumeric" })%></td>
    </tr>

    <tr>
        <th rowspan="2">出庫ロケーション</th>
        <td><%=Html.TextBox("DepartureLocationCode", ViewData["DepartureLocationCode"], new { @class="alphanumeric", maxlength="12", onblur = "GetNameFromCode('DepartureLocationCode','DepartureLocationName','Location')" })%>&nbsp;<img alt="出庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartureLocationCode','DepartureLocationName','/Location/CriteriaDialog?BusinessType=002,009')" /></td>
        <th rowspan="2">入庫ロケーション</th>
        <td><%=Html.TextBox("ArrivalLocationCode", ViewData["ArrivalLocationCode"], new { @class="alphanumeric", maxlength="12", onblur = "GetNameFromCode('ArrivalLocationCode','ArrivalLocationName','Location')" })%>&nbsp;<img alt="入庫ロケーション" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('ArrivalLocationCode','ArrivalLocationName','/Location/CriteriaDialog?BusinessType=002,009')" /></td>
    </tr>
    <tr>
        <td style="height:20px"><span id="DepartureLocationName"><%=Html.Encode(ViewData["DepartureLocationName"]) %></span></td>
        <td style="height:20px"><span id="ArrivalLocationName"><%=Html.Encode(ViewData["ArrivalLocationName"]) %></span></td>
    </tr>
    <tr>
        <th>受注伝票番号</th><%//Add 2016/06/20 arc yano #3584%>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
        <th rowspan="2">部門</th>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = 5, maxlength = 3, onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
    </tr>
     <tr>
        <th>部品番号</th><%//Mod 2016/06/20 arc yano #3584 一行下へ移動%>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = "15", maxlength = "25" })%></td>
        <td><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
    </tr>
    <tr>
        <th>移動種別</th><%//Mod 2016/06/20 arc yano #3584 一行下へ移動%>
        <td colspan="3"><%=Html.DropDownList("TransferType",(IEnumerable<SelectListItem>)ViewData["TransferTypeList"]) %></td>
    </tr>
    <tr>
        <th>入庫ステータス</th>
        <td colspan="3"><%=Html.CheckBox("TransferConfirm",ViewData["TransferConfirm"]) %>確定&nbsp;<%=Html.CheckBox("TransferUnConfirm",ViewData["TransferUnConfirm"]) %>未確定</td>
    </tr>
     <tr>
      <th>&nbsp;</th>
      <td colspan="3">
        <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
        <%--<input type="button" value="検索" onclick="displaySearchList()" />--%>
        <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()" />
        <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('DepartureDateFrom','DepartureDateTo','ArrivalDateFrom','ArrivalDateTo','DepartureLocationCode','DepartureLocationName','ArrivalLocationCode','ArrivalLocationName','PartsNumber','DepartmentCode','DepartmentName','TransferType','TransferConfirm','TransferUnConfirm'))" />
      </td>
    </tr>
</table>
</div>
<%} %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<input type="button" value="新規作成" onclick="openModalAfterRefresh2('/PartsTransfer/Entry')" />&nbsp;
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <tr>
        <%//Mod 2016/06/20 arc yano #3584 文言変更 受注番号→受注伝票番号 ヘッダ項目のnorap指定%>
        <th style="width:30px"></th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">受注伝票番号</th>
        <th style="white-space:nowrap">種別</th>
        <th style="white-space:nowrap">出庫日</th>
        <th style="white-space:nowrap">出庫ロケーションコード</th><%//Add 2018/04/25 arc yano  #3714%>
        <th style="white-space:nowrap">出庫ロケーション名</th>
        <th style="white-space:nowrap">入庫予定日</th>
        <th style="white-space:nowrap">入庫日</th>
        <th style="white-space:nowrap">入庫ロケーションコード</th><%//Add 2018/04/25 arc yano  #3714%>
        <th style="white-space:nowrap">入庫ロケーション名</th>
        <th style="white-space:nowrap">メーカー</th>
        <th style="white-space:nowrap">品番</th>
        <th style="white-space:nowrap">品名</th>
        <th style="white-space:nowrap">数量</th>
    </tr>
    <%foreach (var a in Model) { %>
    <tr>
        <%// Mod 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない %>
        <%// Mod 2015/10/01 arc yano 入庫先のNULLチェックを追加 %>
        <%// Mod 2015/01/07 arc yano #3148 ログインユーザの兼務部門１～３のロケーションが入庫先に選択されている場合は、入庫処理を行えるように修正 %>
        <%// Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
        <td style="white-space:nowrap">
        <%if (a.ArrivalDate == null && a.ArrivalLocation != null)
          {
              bool ret = false;

              CrmsLinqDataContext db = CrmsDataContext.GetDataContext();

              List<Department> aDept = CommonUtils.GetDepartmentFromWarehouse(db, a.ArrivalLocation.WarehouseCode);

              foreach (var d in aDept)
              {
                  if (CommonUtils.DefaultString(d.DepartmentCode).Equals(((Employee)Session["Employee"]).DepartmentCode) ||
                      CommonUtils.DefaultString(d.DepartmentCode).Equals(((Employee)Session["Employee"]).DepartmentCode1) ||
                      CommonUtils.DefaultString(d.DepartmentCode).Equals(((Employee)Session["Employee"]).DepartmentCode2) ||
                      CommonUtils.DefaultString(d.DepartmentCode).Equals(((Employee)Session["Employee"]).DepartmentCode3)
                  )
                  {
                      ret = true;
                      break;
                  }
              }
              if (ret)
              { %>
                <a href="javascript:void(0);" onclick="openModalAfterRefresh2('/PartsTransfer/Confirm/<%=a.TransferNumber%>')">入庫</a>
        <%    }
          } %></td>
        <td style="white-space:nowrap"><%=a.TransferNumber %></td>
        <td style="white-space:nowrap"><%=a.SlipNumber %></td>
        <td style="white-space:nowrap"><%=a.c_TransferType!=null ? a.c_TransferType.Name : "" %></td>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}",a.DepartureDate )%></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.IsNullOrWhiteSpace(a.DepartureLocationCode) ? "" : a.DepartureLocationCode)%></td><%//Add 2018/04/25 arc yano  #3714%>
        <td style="white-space:nowrap"><%=Html.Encode(a.DepartureLocation!=null ? a.DepartureLocation.LocationName : "") %></td>
        <td style="white-space:nowrap"><%=string.Format("{0:yyyy/MM/dd}",a.ArrivalPlanDate) %></td>
        <td style="white-space:nowrap"><%if (a.ArrivalDate != null) { %>
                <%=string.Format("{0:yyyy/MM/dd}", a.ArrivalDate)%>
            <%} else {%>
                <div style="color:red">入庫未確定</div>
            <%} %></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.IsNullOrWhiteSpace(a.ArrivalLocationCode) ? "" : a.ArrivalLocationCode)%></td><%//Add 2018/04/25 arc yano #3714%>
        <td style="white-space:nowrap"><%=Html.Encode(a.ArrivalLocation!=null ? a.ArrivalLocation.LocationName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.Parts!=null && a.Parts.Maker!=null ? a.Parts.Maker.MakerName : "") %></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.PartsNumber) %></td>
        <td style="white-space:nowrap"><%=Html.Encode(a.Parts!=null ? a.Parts.PartsNameJp : "")%></td>
        <td style="white-space:nowrap"><%=a.Quantity %></td>
    </tr>
    <%} %>
</table>
</asp:Content>
