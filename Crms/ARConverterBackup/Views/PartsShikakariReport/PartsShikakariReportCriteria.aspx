<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PartsWipStockAmount>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    仕掛在庫表
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%using (Html.BeginForm("Criteria", "PartsShikakariReport", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//日付のデフォルト値をここで定義 %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
    <%=Html.ValidationSummary() %>
    <table class="input-form">
    <tr>
        <th style="width:100px">対象年月 *</th>
        <td><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月&nbsp;</td>      
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('TargetDateY', 'TargetDateM'))"/>
        </td>
    </tr>
</table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.plist.PageProperty); %>
<br />
<br />
<%--Mod 2023/06/09 yano #4167 --%>
<%if (Session["ConnectDB"] != null && Session["ConnectDB"].Equals("WPH_DB")){%>
<span><b/>CJ藤沢湘南、FA藤沢湘南は同一の倉庫を使用しているため、部品の金額は２つの部門の合算した金額を両部門に表示しています。<br/>また部品の合計値は藤沢湘南の金額が２回加算されています。</span><%//ADD 2017/03/27 arc yano #3735 仕掛品在庫表（暫定）修正対応>%>
<%} %>
<br />
<table class="list" style="width:50%">
    <tr>        
        <th style ="white-space:nowrap">部門コード</th>
        <th style ="white-space:nowrap">部門名</th>
        <th style ="white-space:nowrap">部品</th>
        <th style ="white-space:nowrap">外注費</th>
        <th style ="white-space:nowrap">合計</th>
    </tr>
    <%foreach (var Summary in Model.plist)
      { %>
    <tr>
        <td style ="white-space:nowrap"><%=Html.Encode(Summary.DepartmentCode)%></td><%//伝票番号%>
        <td style ="white-space:nowrap"><%=Html.Encode(Summary.DepartmentName)%></td><%//ラインナンバー%>
        <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Summary.PartsTotalAmount))%></td><%//単価>%>
        <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Summary.TotalCost))%></td><%//金額>%>
        <td style ="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Summary.GrandTotalAmount))%></td><%//外注原価>%>
    </tr>
    <%} %>
    <%//合計を表示%>
    <tr>
        <th></th>
        <th>合計</th>
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalAmount))%></td>  <%//部品の合計%>
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumTotalCost))%></td>  <%//金額の合計%>
        <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",Model.SumGrandTotalAmount))%></td>  <%//外注費の合計%>
    </tr>
</table>

        <%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
