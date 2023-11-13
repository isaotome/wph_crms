<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarTax>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	自動車税種別割検索ダイアログ  <%-- Mod 2019/09/04 yano #4011 --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarTax", FormMethod.Post)) { %>
    <table class="list">
        <tr>
            <th></th>
            <th>表示名</th>
            <th>金額(円)</th>
            <th>登録月</th><% //Add 2019/10/29 yano #4024 %>
        </tr>
        <%foreach (var carTax in Model) { %>
        <tr>
            <td><a href="javascript:selectedCriteriaDialog('<%=carTax.Amount %>','CarTaxName')">選択</a></td>
            <td><%=CommonUtils.DefaultNbsp(carTax.CarTaxName)%></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", carTax.Amount))%></td>
            <td><%=CommonUtils.DefaultNbsp(carTax.RegistMonth) %></td><% //Add 2019/10/29 yano #4024 %>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
