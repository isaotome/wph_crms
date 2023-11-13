<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CashBalance>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	現金在高入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Entry", "CashJournal", FormMethod.Post))
  { %>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("OfficeCode", Model.OfficeCode)%>
<%=Html.Hidden("CashAccountCode",Model.CashAccountCode) %>
<%=Html.Hidden("ClosedDate", Model.ClosedDate)%>
<div id="input-form">
<br />
<h2><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", Model.ClosedDate))%>&nbsp;の現金締め処理</h2>
<br />
<%=Html.ValidationSummary()%>
<br />
<table class="input-form">
    <tr>
        <th style="text-align:right;width:80px;font-size:12pt">金種</th>
        <th style="text-align:right;font-size:12pt">枚数</th>
        <th style="text-align:right;font-size:12pt">金額</th>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">10,000</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf10000", Model.NumberOf10000, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf10000"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf10000))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">5,000</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf5000", Model.NumberOf5000, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf5000"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf5000))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">2,000</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf2000", Model.NumberOf2000, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf2000"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf2000))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">1,000</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf1000", Model.NumberOf1000, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf1000"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf1000))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">500</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf500", Model.NumberOf500, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf500"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf500))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">100</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf100", Model.NumberOf100, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf100"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf100))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">50</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf50", Model.NumberOf50, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf50"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf50))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">10</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf10", Model.NumberOf10, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf10"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf10))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">5</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf5", Model.NumberOf5, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf5"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf5))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">1</th>
        <td style="text-align:right"><%=Html.TextBox("NumberOf1", Model.NumberOf1, new { @class = "numeric", maxlength = 9, onkeyup = "calcTotalCachBalance()" })%></td>
        <td style="text-align:right;font-size:12pt"><span id="AmountOf1"><%=Html.Encode(string.Format("{0:N0}", Model.AmountOf1))%></span></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">小切手等</th>
        <td></td>
        <td style="text-align:right"><%=Html.TextBox("CheckAmount", Model.CheckAmount, new { @class = "numeric", maxlength = 10, onkeyup = "calcTotalCachBalance()" })%></td>
    </tr>
    <tr>
        <th style="text-align:right;font-size:12pt">合計</th>
        <td></td>
        <td style="text-align:right;font-size:12pt"><span id="TotalAmount"><%=Html.Encode(string.Format("{0:N0}", Model.TotalAmount))%></span></td>
    </tr>
    <tr>
        <th></th>
        <td style="text-align:center" colspan="2">
        <%if (CommonUtils.DefaultString(ViewData["AlreadyClosed"]).Equals("1")) { %>
            <input type="button" value="登録" onclick="formSubmit()" disabled="disabled" />
        <%} else { %>
            <input type="button" value="登録" onclick="formSubmit()" />
        <%} %>
        </td>
    </tr>
</table>
</div>
<%} %>
<br />
</asp:Content>
