<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.JournalTransferList>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	入金実績振替
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria","JournalTransfer",FormMethod.Post)){%>
<%=Html.Hidden("RequestFlag", "99") %>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { @class = "alphanumeric", maxlength = "50" })%></td>
        <td colspan="3">
            <input type="button" value="検索" onclick="if (document.getElementById('SlipNumber').value != '') { DisplayImage('UpdateMsg', '0'); displaySearchList() } else { alert('伝票番号を入力して下さい')}" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SlipNumber'))" />
        </td>
    </tr>
</table>
</div>
<br />

<div id ="UpdateMsg" style ="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<br />
<%=Html.ValidationSummary() %>
<%if(!Model.InitFlag) {%>
<%if (Model.list != null && Model.list.Count > 0){ %>
<table class="list">
    <tr>
        <th colspan="6">現在の情報</th>
        <th colspan="2">修正後の情報</th>
    </tr>
    <tr>
        <th style ="white-space:nowrap">伝票番号</th>
        <th style ="white-space:nowrap">入金日</th>
        <th style ="white-space:nowrap">顧客名</th>
        <th style ="white-space:nowrap">伝票ステータス</th>
        <th style ="white-space:nowrap">入金種別</th>
        <th style ="white-space:nowrap">入金額</th>
        <th style ="white-space:nowrap">Check</th>
        <th style ="white-space:nowrap">伝票番号</th>
    </tr>
    <% int cnt = 0; %>
    <%foreach (var a in Model.list) {
          string idPrefix = "line[" + cnt + "]_";
          string namePrefix = "line[" + cnt + "].";
     %>
    <tr>
        <%//伝票番号 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.SlipNumber) %><%=Html.Hidden(namePrefix + "SlipNumber", a.SlipNumber, new { id = idPrefix + "SlipNumber"})%></td>
        <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", a.JournalDate)) %><%=Html.Hidden(namePrefix + "JournalDate", a.JournalDate, new { id = idPrefix + "JournalDate"}) %></td><%//入金日 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.CustomerName) %><%=Html.Hidden(namePrefix + "CustomerName", a.CustomerName, new { id = idPrefix + "CustomerName"}) %></td><%//顧客名 %>
        <td style="white-space:nowrap"><%=Html.Encode(a.SalesStatusName) %><%=Html.Hidden(namePrefix + "SalesStatusName", a.SalesStatusName, new { id = idPrefix + "SalesStatusName"}) %></td><%//伝票ステータス %>
        <td style="white-space:nowrap"><%=Html.Encode(a.AccountTypeName) %><%=Html.Hidden(namePrefix + "AccountTypeName", a.AccountTypeName, new { id = idPrefix + "AccountTypeName"}) %></td><%//入金種別名 %>
        <td style="white-space:nowrap;text-align:right"><%=Html.Encode(string.Format("{0:N0}", a.Amount)) %><%=Html.Hidden(namePrefix + "Amount", a.Amount, new { id = idPrefix + "Amount"}) %></td><%//入金額 %>
        <td style="white-space:nowrap"><%=Html.CheckBox(namePrefix + "TransferCheck", a.TransferCheck, new { id = idPrefix + "TransferCheck", @class = "TransferCheck"}) %></td><%//check %>
        <td style="white-space:nowrap"><%=Html.TextBox(namePrefix + "TransferStlipNumber", a.TransferStlipNumber, new {  id = idPrefix + "TransferStlipNumber", onchange = "if(this.value == document.getElementById('" + idPrefix + "SlipNumber').value){ alert('振替元の伝票番号とは異なる伝票番号を入力して下さい'); this.value = ''; this.blur(); this.focus(); }else{isExistsSlip(this, 'JournalTransfer') }" })%></td><%//振替先伝票番号 %>
        <%=Html.Hidden(namePrefix + "JournalId", a.JournalId, new { id = idPrefix + "JournalId"}) %><%//入金実績ID %>
        <%=Html.Hidden(namePrefix + "DepartmentCode", a.DepartmentCode, new { id = idPrefix + "DepartmentCode"}) %><%//部門コード %>
    </tr>
    <%
        cnt++;
      } %>
    <tr>
      <td colspan="5" style="color:#0000ff">▲修正したい行にチェックを入れて振替えたい伝票番号を入力後に「修正を反映」ボタンをクリック</td>
      <td colspan="3">
            <input type="button" value="修正を反映" onclick="if (checkExecTransfer('TransferCheck')) { document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit(); }" />
      </td>  
    </tr>
</table>

<%}else{ %>
    <div style ="font-size:14px">入金実績がありません</div>
<%} %>

<br />
<%} %>
<% // --------実績振替履歴-------%>
<hr />
<div id="JournalTransferList" style ="width:75%">
<%Html.RenderAction("JournalChangeList", "JournalTransfer");%>
</div>

    <%} %>
</asp:Content>
