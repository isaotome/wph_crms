<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.DepreciationRate>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    償却率検索
</asp:Content>
<%-----------------------------------------------------
機能：
    償却率マスタ検索画面
作成日：
    2018/06/06 arc yano #3883 タマ表改善 新規作成
------------------------------------------------------%>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "DepreciationRate", FormMethod.Post))
  { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/DepreciationRate/Entry')" />
    <input type="button" value="償却率一括取込" onclick="openModalAfterRefresh('/DepreciationRate/ImportDialog')" />
    <br />
    <br />
    <%Html.RenderPartial("PagerControl", Model.PageProperty); %>
    <br />
    <table class="list" style ="width:80%">
        <tr>
            <th style ="white-space:nowrap">耐用年数</th>
            <th style ="white-space:nowrap">償却率</th>
            <th style ="white-space:nowrap">改訂償却率</th>
            <th style ="white-space:nowrap">保障率</th>
        </tr>
        <%foreach (DepreciationRate rec in Model)
          { %>
        <tr>
            <td style ="white-space:nowrap"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/DepreciationRate/Entry?UsefulLives=<%=rec.UsefulLives%>');return false;"><%=CommonUtils.DefaultNbsp(rec.UsefulLives)%></a></td>
            <td style ="white-space:nowrap"><%=CommonUtils.DefaultNbsp(rec.Rate)%></td>
            <td style ="white-space:nowrap"><%=CommonUtils.DefaultNbsp(rec.RevisedRate)%></td>
            <td style ="white-space:nowrap"><%=CommonUtils.DefaultNbsp(rec.SecurityRatio)%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
