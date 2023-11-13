<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CarAcquisitionTax>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    自動車税環境性能割検索<%-- Mod 2019/09/04 yano #4011 --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "CarAcquisitionTax", FormMethod.Post)) { %>
    <br />
    <input type="button" value="新規作成" onclick="openModalAfterRefresh2('/CarAcquisitionTax/Entry')" />
    <br />
    <br />
    <table class="list">
        <tr>
            <th>経過年数(年)</th>
            <th>残価率</th>
        </tr>
        <%foreach (var tax in Model) { %>
        <tr>
            <td><a href="javascript:void(0);" onclick="openModalAfterRefresh2('/CarAcquisitionTax/Entry/<%=tax.CarAcquisitionTaxId%>');return false;"><%=CommonUtils.DefaultNbsp(tax.ElapsedYears)%></a></td>
            <td><%=CommonUtils.DefaultNbsp(tax.RemainRate)%></td>
        </tr>
        <%} %>
    </table>
<%} %>
</asp:Content>
