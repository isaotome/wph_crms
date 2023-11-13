<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetJournalDataResult>>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    入金実績情報
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "JournalData", new { id = "0" }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//処理の種別(各ボタンクリック時の処理の種類)%>

<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">　
<br />
<div id ="ValidBlock">
<%=Html.ValidationSummary() %>
</div>
<table class="input-form">
    <tr>
        <th style ="width:70px">入金日</th>
        <td ><%=Html.TextBox("TargetDateFrom", ViewData["TargetDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "return chkDate3(this.value, 'TargetDateFrom')" })%>～<%=Html.TextBox("TargetDateFromTo", ViewData["TargetDateFromTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "return chkDate3(this.value, 'TargetDateFromTo')" })%></td>
    </tr>
     <tr>
        <th></th>
        <td>
        <input type="button" value="検索" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '999'; DisplayImage('UpdateMsg', '0'); dispProgressed('JournalData', 'UpdateMsg'); displaySearchList()"/>
        <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('TargetDateFrom', 'TargetDateFromTo'));"/>
        <input type="button"  id="ExcelOutputButton" value="EXCEL出力" onclick="ClearValidateMessage('ValidBlock'); document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('JournalData', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '999'"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>

<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table style="width:180%" class="list">
    <tr>
        <th style ="white-space:nowrap">入金日</th>
        <th style ="white-space:nowrap">部門コード</th>
        <th style ="white-space:nowrap">部門名</th>
        <th style ="white-space:nowrap">請求先コード</th>
        <th style ="white-space:nowrap">請求先名称</th>
        <th style ="white-space:nowrap">伝票番号</th>
        <th style ="white-space:nowrap">伝票ステータス</th>
        <th style ="white-space:nowrap">納車日</th>
        <th style ="white-space:nowrap">顧客コード</th>
        <th style ="white-space:nowrap">顧客名</th>
        <th style ="white-space:nowrap">金額</th>
        <th style ="white-space:nowrap">口座種別</th>
        <th style ="white-space:nowrap">摘要</th>
        <th style ="white-space:nowrap">科目コード</th>
        <th style ="white-space:nowrap">科目名</th>
    </tr>
    <%foreach(var a in Model){%>
    <tr>
        <td style ="white-space:nowrap"><%=Html.Encode(a.JournalDate != null ? string.Format("{0:yyy/MM/dd}", a.JournalDate) : "") %></td>                          <%//入金日%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.DepartmentCode) %></td>                                                                                    <%//部門コード%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.DepartmentName) %></td>                                                                                    <%//部門名%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CustomerClaimCode) %></td>                                                                                 <%//請求先コード%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CustomerClaimName) %></td>                                                                                 <%//請求先名称%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.SlipNumber) %></td>                                                                                        <%//伝票番号%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.OrderStatus) %></td>                                                                                       <%//伝票ステータス%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.SalesDate != null ? string.Format("{0:yyy/MM/dd}", a.SalesDate) : "") %></td>                              <%//納車日%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CustomerCode) %></td>                                                                                      <%//顧客コード%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.CustomerName) %></td>                                                                                      <%//顧客名%>
        <td style="text-align:right;white-space:nowrap"><%=Html.Encode(a.Amount != null ? string.Format("{0:N0}", a.Amount) : "")%></td><%//金額%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.AccountType) %></td>                                                                                       <%//口座種別%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.Summary) %></td>                                                                                           <%//摘要%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.AccountCode) %></td>                                                                                       <%//科目コード%>
        <td style ="white-space:nowrap"><%=Html.Encode(a.AccountName) %></td>                                                                                       <%//科目名%>
    </tr>
    <%
    } %>
</table>
    <br />
</asp:Content>

<asp:Content ID="Content3" runat="server" contentplaceholderid="HeaderContent">
   
</asp:Content>


