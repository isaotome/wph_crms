<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetServiceSalesHistoryHeaderResult>>"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    翼）整備履歴
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
    //--------------------------------------------------------------------------------
    //　機能　：整備履歴検索画面
    //　作成日：2017/03/13 arc yano #3725 サブシステム移行(整備履歴) 新規作成
    //
    //
    //-------------------------------------------------------------------------------- 
%>

<%using (Html.BeginForm("Criteria", "ServiceSalesHistory", new { id = 0 }, FormMethod.Post))
{ 
%>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("DefaultDivType", "018") %><%//拠点タイプのデフォルト値 %>
<%=Html.Hidden("DefaultDepartmentName", "0") %><%//拠点のデフォルト値 %>

<br />
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<%=Html.ValidationSummary() %>
   <table class="input-form">
     <tr>
        <th style="width:80px">拠点</th>
        <td colspan="3"><%=Html.RadioButton("DivType", "018", ViewData["DivType"] != null && ViewData["DivType"].ToString().Equals("018"), new { onclick = "GetCodeMasterListAll('ServiceSalesHistory', 'Department', this.value)" })%>CJ<%=Html.RadioButton("DivType", "019", ViewData["DivType"] != null && ViewData["DivType"].ToString().Equals("019"), new { onclick = "GetCodeMasterListAll('ServiceSalesHistory', 'Department', this.value)" })%>FA&nbsp;<%=Html.DropDownList("Department", (IEnumerable<SelectListItem>)ViewData["DepartmentNameList"])%></td>     
    </tr>
     <tr>
        <th style="width:80px">伝票番号</th>
        <td><%=Html.TextBox("SlipNumber", ViewData["SlipNumber"])%></td>
         <th style="width:80px">車台番号</th>
        <td><%=Html.TextBox("Vin", ViewData["Vin"])%></td>
    </tr>
    <tr>
        <th style="width:80px">登録番号</th>
        <td><%=Html.TextBox("RegistNumber", ViewData["RegistNumber"])%></td>
         <th colspan ="2"></th>
    </tr>
    <tr>
        <th style="width:80px">顧客名</th>
        <td><%=Html.TextBox("CustomerName", ViewData["CustomerName"])%></td>
         <th style="width:80px">顧客名（かな）</th>
        <td><%=Html.TextBox("CustomerNameKana", ViewData["CustomerNameKana"])%></td>
    </tr>
     <tr>
        <th style="width:80px"></th>
        <td colspan="3">
            <input type="button" value="検索" onclick=" DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
            <input type="button" value="クリア" onclick = "resetCommonCriteria(new Array('DivType', 'Department', 'SlipNumber', 'Vin', 'RegistNumber', 'CustomerName', 'CustomerNameKana')); GetCodeMasterListAll('ServiceSalesHistory', 'Department', document.getElementById('DefaultDivType').value)" />
        </td>
    </tr>
    </table>
</div>

<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>
<br />
<br />
<%Html.RenderPartial("PagerControl", Model.PageProperty); %>
<br />
<br />

<table class="list">
    <tr> 
        <th style="white-space:nowrap; text-align:left">店舗名</th>
        <th style="white-space:nowrap; text-align:left">納車日</th>
        <th style="white-space:nowrap; text-align:left">伝票番号</th>
        <th style="white-space:nowrap; text-align:left">主作業</th>
        <th style="white-space:nowrap; text-align:left">顧客名</th>
        <th style="white-space:nowrap; text-align:left">車台番号</th>
        <th style="white-space:nowrap; text-align:left">登録番号</th>
        <th style="white-space:nowrap; text-align:left">走行距離</th>
        <th style="white-space:nowrap; text-align:left">フロント担当</th>
        <th style="white-space:nowrap; text-align:left">メカ担当</th>
        <th style="white-space:nowrap; text-align:left">工賃</th>
        <th style="white-space:nowrap; text-align:left">部品代</th>
        <th style="white-space:nowrap; text-align:left">諸費用</th>
        <th style="white-space:nowrap; text-align:left">合計</th>
    </tr>

    <%//検索結果を表示する。%>
    <% 
       decimal technicalFeeAmount = 0m;
       decimal partsAmount = 0m;
       decimal variousAmount = 0m;
       decimal runningDate = 0m;
       DateTime salesDate;
     %>
    <% foreach (var item in Model){ %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_DepartmentName) ? item.H_DepartmentName.Trim() : "")%></td>                                                                              <%//店舗名%>
            <td style="white-space:nowrap"><%=Html.Encode(DateTime.TryParseExact(item.H_SalesInputDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out salesDate) == false ? "" : string.Format("{0:yyyy/MM/dd}", salesDate))%></td>                                                                                                             <%//納車日%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_SlipNumber) ? item.H_SlipNumber.Trim() : "")%></td>                                                                                      <%//伝票番号%>
            <td style="white-space:nowrap"><a href="javascript:openModalAfterRefresh('/ServiceSalesHistory/LineCriteria?SlipNumber=<%=item.H_SlipNumber%>&DivType=<%=ViewData["DivType"]%>')"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_ServiceWorkName) ? item.H_ServiceWorkName.Trim() : "")%></a></td>       <%//主作業名 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_CustomerName1) ? item.H_CustomerName1.Trim() : "")%></td>                                                                                <%//顧客名 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_Vin) ? item.H_Vin.Trim() : "")%></td>                                                                                                    <%//車台番号 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_RegistName) ? item.H_RegistName.Trim() : "")%>&nbsp;<%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_RegistType) ? item.H_RegistType.Trim() : "")%>&nbsp;<%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_RegistNumberKana) ? item.H_RegistNumberKana.Trim() : "")%>&nbsp;<%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_RegistNumber) ? item.H_RegistNumber.Trim() : "")%></td>                                                                                  <%//登録番号 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F0}", Decimal.TryParse(item.H_RunningData, out runningDate) == false ? 0m : runningDate))%></td>                                                                                                      <%//走行距離 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_FrontEmployeeName) ? item.H_FrontEmployeeName.Trim() : "")%></td>                                                                        <%//フロント担当者 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_MechanicEmployeeName) ? item.H_MechanicEmployeeName.Trim() : "")%></td>                                                                  <%//メカニック担当者 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.H_TechnicalFeeAmount, out technicalFeeAmount) == false ? 0m : technicalFeeAmount))%></td>                                                                                               <%//技術料 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.H_PartsAmount, out partsAmount) == false ? 0m : partsAmount))%></td>                                                                                                      <%//部品代 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.H_VariousAmount, out variousAmount) == false ? 0m : variousAmount))%></td>                                                                                                    <%//諸費用 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", (Decimal.TryParse(item.H_TechnicalFeeAmount, out technicalFeeAmount) == false ? 0m : technicalFeeAmount) + (Decimal.TryParse(item.H_PartsAmount, out partsAmount) == false ? 0m : partsAmount) + (Decimal.TryParse(item.H_VariousAmount, out variousAmount) == false ? 0m : variousAmount)))%></td><%//合計 %>
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
