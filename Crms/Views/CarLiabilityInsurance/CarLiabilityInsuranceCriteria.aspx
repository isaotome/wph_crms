<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarLiabilityInsurance>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自賠責保険料検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarLiabilityInsurance", FormMethod.Post)) { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarLiabilityInsurance/Entry')" />
    <br />
    <br />
    <table class="list">
        <tr>
            <th>表示名</th>
            <th>金額(円)</th>
        </tr>
        <%foreach (var insurance in Model) { %>
        <tr>
            <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarLiabilityInsurance/Entry/<%=insurance.CarLiabilityInsuranceId%>');return false;"><%=CommonUtils.DefaultNbsp(insurance.CarLiabilityInsuranceName)%></a></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", insurance.Amount))%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
