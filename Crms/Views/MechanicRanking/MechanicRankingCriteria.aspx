<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetMechanicRankingResult>>"%>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    メカニック売上ランキング
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    //--------------------------------------------------------------------------------
    //　機能　：メカニックランキング検索画面
    //　作成日：2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング) 新規作成
    //
    //
    //-------------------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "MechanicRanking", new { id = 0 }, FormMethod.Post ))
  { 
%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultTargetDateY", ViewData["DefaultTargetDateY"]) %>
<%=Html.Hidden("DefaultTargetDateM", ViewData["DefaultTargetDateM"]) %>

<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />

   <table class="input-form">
     <tr>
        <th style="width:80px">年月</th>
        <td colspan="3"><%=Html.DropDownList("TargetDateY", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("TargetDateM", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月</td>     
    </tr>
     <tr>
        <th style="width:80px"></th>
        <td colspan="3">
            <input type="button" value="検索" onclick=" DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateY', 'TargetDateM'))" />
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />

<%
      decimal? maxPix = 0m;
      maxPix = (((Model.Select(x => x.TechnicalFeeAmount).Max() / 7000m) + 5) < 350m ? 350m : ((Model.Select(x => x.TechnicalFeeAmount).Max() / 7000m) + 5));
      
 %>

<span><%=Html.Encode(ViewData["TextTargetDateY"])%>年<%=Html.Encode(ViewData["TextTargetDateM"])%>月度</span>
<table class="list" style ="table-layout: fixed; width:750px" >
    <tr> 
        <th style="white-space:nowrap; text-align:left; width:50px">順位</th>
        <th style="white-space:nowrap; text-align:left; width:150px">所属</th>
        <th style="white-space:nowrap; text-align:left; width:120px">氏名</th>
        <th style="white-space:nowrap; text-align:left; width:90px">工賃売上</th>
        <th style="white-space:nowrap; text-align:left; width:<%=maxPix%>px">グラフ</th>
    </tr>

    <%//検索結果を表示する。%>
   
    <% foreach (var item in Model)
       { %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(item.Ranking)%></td>                                                                                                         <%//順位%> 
            <td style="white-space:nowrap"><%=Html.Encode(item.DepartmentName)%></td>                                                                                                  <%//所属%>
            <td style="white-space:nowrap"><%=Html.Encode(item.EmployeeName)%></td>                                                                                                    <%//氏名%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",item.TechnicalFeeAmount))%></td>                                                    <%//仕入金額%> 
            <td style="white-space:nowrap"><img src="/Content/Images/blue.gif" style="height:20px; width:<%= item.TechnicalFeeAmount > 0 ? (item.TechnicalFeeAmount/7000) : 0%>px"></td>
            
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
