<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.Transfer>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両移動履歴
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="list">
    <tr>
        <th>移動伝票番号</th>
        <th>出庫日</th>
        <th>出庫ロケーション</th>
        <th></th>
        <th>入庫ロケーション</th>
        <th>入庫予定日</th>
        <th>入庫日</th>
    </tr>
<%foreach (var a in Model) { %>
    <tr>
        <td><%=CommonUtils.DefaultNbsp(a.TransferNumber) %></td>
        <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",a.DepartureDate.Equals(new DateTime()) ? "" : a.DepartureDate.ToString())) %></td>
        <td><%=CommonUtils.DefaultNbsp(a.DepartureLocation!=null ? a.DepartureLocation.LocationName : "") %></td>
        <td>→</td>
        <td><%=CommonUtils.DefaultNbsp(a.ArrivalLocation!=null ? a.ArrivalLocation.LocationName : "") %></td>
        <td><%=CommonUtils.DefaultNbsp(string.Format("{0:yyyy/MM/dd}",a.ArrivalPlanDate.Equals(new DateTime()) ? "" : a.ArrivalPlanDate.ToString())) %></td>
        <td><%=a.ArrivalDate!=null ? string.Format("{0:yyyy/MM/dd}",a.ArrivalDate.Equals(new DateTime()) ? null : a.ArrivalDate) : "入庫未確定" %></td>
    </tr>
<%} %>
</table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
</asp:Content>
