<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarLiabilityInsurance>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	自賠責保険料ダイアログ
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarLiabilityInsurance", FormMethod.Post)) { %>
    <table class="list">
        <tr>
            <th></th>
            <th>表示名</th>
            <th>金額(円)</th>
        </tr>
        <%foreach (var insurance in Model) { %>
        <tr>
            <td><a href="javascript:selectedCriteriaDialog('<%=insurance.Amount %>','CarLiabilityInsuranceName')">選択</a></td>
            <td><%=CommonUtils.DefaultNbsp(insurance.CarLiabilityInsuranceName)%></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", insurance.Amount))%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
