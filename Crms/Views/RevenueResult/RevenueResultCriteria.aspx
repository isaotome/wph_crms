<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.V_RevenueResult>>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    入金実績表示 <%//Mod 2018/08/22 yano #3930%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "RevenueResult", new { id = "0" }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>

<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">　
<br />
<%=Html.ValidationSummary() %>
<table class="input-form">
    <tr>
        <%--//Mod 2015/02/17 arc iijima 部門コード入力チェック取り外し--%> 
        <th >部門コード</th>
        <td ><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th style="width:100px"> 部門名</th> 
        <td > <span id="DepartmentName"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <%// del 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　検索対象日付のラジオボタン削除 %>
    <%// del 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　年月のみで検索するチェックボックス削除 %>
    <tr>
        <th >伝票番号</th>
        <td ><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"], new { maxlength = 50 })%></td>
        <th>受注日</th>
        <td ><%=Html.TextBox("TargetDateFrom", ViewData["TargetDateFrom"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "return chkDate3(document.getElementById('TargetDateFrom').value, 'TargetDateFrom')" })%>～<%=Html.TextBox("TargetDateFromTo", ViewData["TargetDateFromTo"], new { @class = "alphanumeric", size = 10, maxlength = 10, onchange = "return chkDate3(document.getElementById('TargetDateFromTo').value, 'TargetDateFromTo')" })%></td>
    </tr>
    <%// del 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　伝票ステータス削除 %>
    <tr>
        <th>顧客コード</th>
        <td><%=Html.TextBox("CustomerCode",ViewData["CustomerCode"], new { onblur = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" /></td>
        <th>顧客名</th>
        <td><span id="CustomerName" ><%=Html.Encode(ViewData["CustomerName"])%></span></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="4">
            <input type="button" value="検索" onclick="DisplayImage('UpdateMsg', '0'); displaySearchList();" />
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SlipNumber', 'DepartmentCode', 'DepartmentName', 'TargetDateFrom', 'TargetDateFromTo', 'CustomerCode', 'CustomerName'))" />
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
<table class="list">
    <tr>
        <th colspan="2" class="auto-style2" >伝票番号</th>
        <th class="auto-style2">受注日</th>
        <th class="auto-style2">入金日</th>
        <th class="auto-style2">伝票ステータス</th>
        <th class="auto-style2">顧客コード</th>
        <th class="auto-style2">顧客名</th>
        <th class="auto-style2">車種名</th>
        <th class="auto-style2">サービス内容</th>
        <th class="auto-style2">請求金額</th>
        <th class="auto-style2">入金額</th>
        <th class="auto-style2">残高</th>
    </tr>
    <%for(int i = 0 ; i<Model.Count;i++){
          var item = Model[i];
          string backColor = "#ffffff";
          if (i % 2 == 1) {
              backColor = "#f5f5f5";
          }
    %>
    <tr>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.ST)%></td>
        <td style="white-space:nowrap;background-color:<%=backColor%>"><a href="javascript:void(0);" onclick="openModalAfterRefresh('/RevenueResult/Detail?SlipNo=<%=item.SlipNumber%>&CustomerCode=<%=item.CustomerCode%>&ST=<%=item.ST%>')"><%=Html.Encode(item.SlipNumber)%></a></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.SalesOrderDate))%></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",item.JournalDate))%></td>
        <%// Mod 2015/04/15 arc nakayama サービス伝票ステータスだけ表示されないバグ修正%>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(!string.IsNullOrEmpty(item.SalesStatusName) ? item.SalesStatusName : !string.IsNullOrEmpty(item.ServiceStatusName) ? item.ServiceStatusName: "" )%></td>
        <td style="background-color:<%=backColor%>">
        <a href="javascript:void(0);" onclick="openModalAfterRefresh('/RevenueResult/Detail?CustomerCode=<%=item.CustomerCode%>' )"><%=item.CustomerCode%></a>
        </td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.CustomerName)%></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.Desc1)%>&nbsp;<%=Html.Encode(item.Desc2 != "" && item.Desc1 != "" ? "/" + item.Desc2 : item.Desc2 )%></td>
        <td style="background-color:<%=backColor%>"><%=Html.Encode(item.Desc3)%></td>
        <td style="text-align:right;background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:N0}", (item.ReceiptAmount ?? 0m)))%></td>
        <td style="text-align:right;background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:N0}", (item.Amount ?? 0m)))%></td>
        <td style="text-align:right;background-color:<%=backColor%>"><%=Html.Encode(string.Format("{0:N0}",(item.ReceivableBalance ?? 0)))%></td>
    </tr>
    <%} %>
    </table>
    <br />
</asp:Content>

<asp:Content ID="Content3" runat="server" contentplaceholderid="HeaderContent">
   
</asp:Content>


