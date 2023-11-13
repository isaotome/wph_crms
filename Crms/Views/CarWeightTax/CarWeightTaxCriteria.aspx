<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarWeightTax>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自動車重量税検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarWeightTax", FormMethod.Post)) { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarWeightTax/Entry')" />
    <br />
    <br />
    <table class="list">
        <tr>
            <th style="width:30px"></th>
            <th style="width:50px">車検年数</th>
            <th>重量(kg)</th>
            <th>金額(円)</th>
        </tr>
        <%foreach (var carWeightTax in Model) { %>
        <tr>
            <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarWeightTax/Entry/<%=carWeightTax.CarWeightTaxId%>');return false;">編集</a></td>
            <td><%=CommonUtils.DefaultNbsp(carWeightTax.InspectionYear) %></td>
            <td><%=CommonUtils.DefaultNbsp(carWeightTax.WeightFrom) %> ～ <%=CommonUtils.DefaultNbsp(carWeightTax.WeightTo) %></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", carWeightTax.Amount))%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
