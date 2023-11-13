<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarTax>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自動車税種別割検索   <%-- Mod 2019/09/04 yano #4011 --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarTax", FormMethod.Post)) { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh('/CarTax/Entry')" />
    <br />
    <br />
    <table class="list">
        <tr>
            <th>表示名</th>
            <th>金額(円)</th>
            <th>登録月</th>
            <th>総排気量FROM (cc)</th>
            <th>総排気量TO (cc)</th>
            <th>適用日FROM</th><% //Add 2019/10/21 yano #4023%>
            <th>適用日TO</th><% //Add 2019/10/21 yano #4023%>
        </tr>
        <%foreach (var carTax in Model) { %>
        <tr>
            <td><a href="javascript:void(0);" onclick="openModalAfterRefresh('/CarTax/Entry/<%=carTax.CarTaxId%>');return false;"><%=CommonUtils.DefaultNbsp(carTax.CarTaxName)%></a></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}", carTax.Amount))%></td>
            <td><%=CommonUtils.DefaultNbsp(carTax.RegistMonth) %></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N}",carTax.FromDisplacement)) %></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:N}",carTax.ToDisplacement)) %></td>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",carTax.FromAvailableDate)) %></td><% //Add 2019/10/21 yano #4023%>
            <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",carTax.ToAvailableDate)) %></td><% //Add 2019/10/21 yano #4023%>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
