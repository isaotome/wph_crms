<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage2.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceSalesHistoryRet>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    翼）整備履歴詳細
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

<table class="command">
        <tr>
            <td onclick="window.close()">
                <img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる
            </td>
        </tr>
</table>
<%using (Html.BeginForm("LineCriteria", "ServiceSalesHistory", new { id = 0 }, FormMethod.Post))
{ 
%>

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
    <% foreach (var item in Model.headerList){ %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_DepartmentName) ? item.H_DepartmentName.Trim() : "")%></td>                                                                              <%//店舗名%>
            <td style="white-space:nowrap"><%=Html.Encode(DateTime.TryParseExact(item.H_SalesInputDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out salesDate) == false ? "" : string.Format("{0:yyyy/MM/dd}", salesDate))%></td>                                                                                                             <%//納車日%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_SlipNumber) ? item.H_SlipNumber.Trim() : "")%></td>                                                                                      <%//伝票番号%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_ServiceWorkName) ? item.H_ServiceWorkName.Trim() : "")%></td>       <%//主作業名 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_CustomerName1) ? item.H_CustomerName1.Trim() : "")%></td>                                                                                <%//顧客名 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_Vin) ? item.H_Vin.Trim() : "")%></td>                                                                                                    <%//車台番号 %>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.H_RegistNumber) ? item.H_RegistNumber.Trim() : "")%></td>                                                                                  <%//登録番号 %>
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
<%//明細部 %>
<br />
<br />

<table class="list">
    <tr> 
        <th style="white-space:nowrap; text-align:center">区分1</th>
        <th style="white-space:nowrap; text-align:center">区分2</th>
        <th style="white-space:nowrap; text-align:center">内容</th>
        <th style="white-space:nowrap; text-align:center">メカ担当</th>
        <th style="white-space:nowrap; text-align:center">数量</th>
        <th style="white-space:nowrap; text-align:center">技術料</th>
        <th style="white-space:nowrap; text-align:center">部品代</th>
        <th style="white-space:nowrap; text-align:center">諸費用</th>
    </tr>

    <%//検索結果を表示する。%>
    <% 
       decimal ltechnicalFeeAmount = 0m;
       decimal lpartsAmount = 0m;
       decimal lvariousAmount = 0m;
       decimal lquantity = 0m;
     %>
    <% foreach (var item in Model.lineList)
       { %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.L_LineNumber) ? item.L_LineNumber.Trim() : "")%></td>                                                                                                                                                 <%//区分１%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.L_ContentsType) ? item.L_ContentsType.Trim() : "")%></td>                                                                                                                                             <%//区分２%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.L_ContentsName) ? item.L_ContentsName.Trim() : "")%></td>                                                                                                                                             <%//内容%>
            <td style="white-space:nowrap"><%=Html.Encode(!string.IsNullOrWhiteSpace(item.L_MechanicEmployeeCode) ? item.L_MechanicEmployeeCode.Trim() : "")%>&nbsp;<%=Html.Encode(!string.IsNullOrWhiteSpace(item.L_MechanicEmployeeName) ? item.L_MechanicEmployeeName.Trim() : "")%></td>    <%//メカ担当（コード） %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F2}", Decimal.TryParse(item.L_Quantity, out lquantity) == false ? 0m : lquantity))%></td>                                                                                                         <%//数量%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.L_TechnicalFeeAmount, out ltechnicalFeeAmount) == false ? 0m : ltechnicalFeeAmount))%></td>                                                                           <%//技術料 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.L_PartsAmount, out lpartsAmount) == false ? 0m : lpartsAmount))%></td>                                                                                                <%//部品代 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}", Decimal.TryParse(item.L_VariousAmount, out lvariousAmount) == false ? 0m : lvariousAmount))%></td>                                                                                          <%//諸費用 %>
        </tr>
    <%} %> 
</table>
<%} %>

</asp:Content>
