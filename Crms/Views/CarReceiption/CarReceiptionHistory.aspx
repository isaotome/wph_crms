<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.CustomerReceiption>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	顧客受付履歴
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<table class="command">
   <tr>
       <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn"/>&nbsp;閉じる</td>
   </tr>
</table>
<br />
<table class="input-form">
    <tr>
        <th colspan="4" class="input-form-title">顧客情報</th>
    </tr>
    <tr>
        <th style="width:100px">顧客コード</th>
        <td style="width:200px"><%=Html.Encode(ViewData["CustomerCode"]) %></td>
        <th style="width:100px">顧客ランク</th>
        <td style="width:200px"><%=Html.Encode(ViewData["CustomerRankName"]) %></td>
    </tr>
    <tr>
        <th>顧客名</th>
        <td><%=Html.Encode(ViewData["CustomerName"]) %></td>
        <th>顧客名(カナ)</th>
        <td><%=Html.Encode(ViewData["CustomerNameKana"])%></td>
    </tr>
    <tr>
        <th>都道府県</th>
        <td><%=Html.Encode(ViewData["Prefecture"]) %></td>
        <th>市区町村</th>
        <td><%=Html.Encode(ViewData["City"]) %></td>
    </tr>
    <tr>
        <th>住所1</th>
        <td colspan="3"><%=Html.Encode(ViewData["Address1"]) %></td>
    </tr>
    <tr>
        <th>住所2</th>
        <td colspan="3"><%=Html.Encode(ViewData["Address2"])%></td>
    </tr>
</table>
<br />
<br />
<table class="input-form">
    <tr>
        <th class="input-form-title" colspan="3">受付履歴</th>
    </tr>
    <tr>
        <th style="width:100px">受付日</th>
        <th style="width:200px">来店種別</th>
        <th style="width:340px">来店目的</th>
    </tr>
</table>
<div style="overflow-y:scroll;width:680px;height:200px">
<table class="input-form">
<%foreach (var customerReceiption in Model)
  { %>
    <tr>
        <td style="width:100px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", customerReceiption.ReceiptionDate))%></td>
        <td style="width:200px"><%if (customerReceiption.c_ReceiptionType != null) { %><%=Html.Encode(customerReceiption.c_ReceiptionType.Name)%><%} %></td>
        <td style="width:340px"><%if (customerReceiption.c_Purpose != null) { %><%=Html.Encode(customerReceiption.c_Purpose.Name)%><%} %></td>
   </tr>
<%} %>
</table>
</div>
<br />
</asp:Content>
