<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarWeightTax>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	自動車重量税検索ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarWeightTax", FormMethod.Post)) { %>
    <table class="list">
        <tr>
            <th></th>
            <th>車検年数</th>
            <th>重量(kg)</th>
            <th>金額(円)</th>
        </tr>
        <%foreach (var carWeightTax in Model) { %>
        <tr>
            <td><a href="javascript:selectedCriteriaDialog('<%=carWeightTax.Amount %>','')">選択</a></td>
            <td><%=CommonUtils.DefaultNbsp(carWeightTax.InspectionYear) %></td>
            <td><%=CommonUtils.DefaultNbsp(carWeightTax.WeightFrom) %> ～ <%=CommonUtils.DefaultNbsp(carWeightTax.WeightTo) %></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", carWeightTax.Amount))%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
