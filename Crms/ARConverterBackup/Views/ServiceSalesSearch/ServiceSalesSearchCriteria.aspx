<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarStock>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス売上検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Criteria", "ServiceSalesSearch", FormMethod.Post))
      { %>
    <%=Html.Hidden("id","0")%>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
    <%=Html.Hidden("SearchList", ViewData["SearchList"]) %>
    <a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
    <br/>
    
<div id="search-form">
    <br/>
    <%=Html.ValidationSummary() %>
    <table class="input-form">
        <tr>
            <th style="width:150px">出力 *</th>
            <td style="width:300px">
                <%=Html.DropDownList("ExportName", (IEnumerable<SelectListItem>)ViewData["ExportList"], new { onchange = "document.getElementById('RequestFlag').value = '0';ExportSelected2(document.forms[0].ExportName.value,1);formSubmit();" })%>
            </td>
        </tr>
        <tbody id="SalesDate">
            <tr>
                <th>納車日(YYYY/MM) *</th>
                <td> <%=Html.DropDownList("SalesJournalYearFrom", (IEnumerable<SelectListItem>)ViewData["SalesJournalYearFromList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("SalesJournalMonthFrom", (IEnumerable<SelectListItem>)ViewData["SalesJournalMonthFromList"])%>&nbsp;月 ～ <%=Html.DropDownList("SalesJournalYearTo", (IEnumerable<SelectListItem>)ViewData["SalesJournalYearToList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("SalesJournalMonthTo", (IEnumerable<SelectListItem>)ViewData["SalesJournalMonthToList"])%>&nbsp;月</td>
            </tr>
        </tbody>
        <tbody id="Division">
            <tr>
                <th>拠点</th>
                <td>
                    <%=Html.DropDownList("DivisionType", (IEnumerable<SelectListItem>)ViewData["DivisionTypeList"]/*, new { onchange = "document.getElementById('OfficeCode').value = '';GetOfficeNameFromCode('OfficeCode','OfficeName','DivisionType')" }*/)%>
                </td>
            </tr>
        </tbody>
        <tbody id="Csv">
            <tr>
                <th></th>
                <td>
                    <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit();" />
                    <input type="button" value="クリア" onclick="clearCondition2();" />
                    <input type="button" value="CSV" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('ServiceSalesSearch', 'UpdateMsg'); document.forms[0].submit();" />
                </td>
            </tr>
        </tbody>
        <tbody id="Journal">
            <tr>
                <th></th>
                <td>
                    <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit();" />
                    <input type="button" value="クリア" onclick="clearCondition2();" />
                    <input type="button" value="CSV" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('ServiceSalesSearch', 'UpdateMsg'); document.forms[0].submit();" />
                </td>
            </tr>
        </tbody>
    </table>
</div>
    <%} %>
        <div id="UpdateMsg" style="display:none">
        <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
        </div>
        <br />
     <%
         CrmsDao.PaginatedProperty page = null;
         CrmsDao.PaginatedList<V_ServiceSalesDownload> listServiceSales = null;
         CrmsDao.PaginatedList<V_OfficeSalesDownload> listOffice = null;

         if (ViewData["SearchList"] != null)
         {
             switch (CommonUtils.DefaultString(ViewData["ExportName"]))
             {
                 case "001": // サービス売上
                     listServiceSales = (CrmsDao.PaginatedList<V_ServiceSalesDownload>)ViewData["SearchList"];
                     page = listServiceSales.PageProperty;
                     break;
                     
                 case "002": // サービス売上(社内)
                     listOffice = (CrmsDao.PaginatedList<V_OfficeSalesDownload>)ViewData["SearchList"];
                     page = listOffice.PageProperty;
                     break;
             }
         }
         else
         {
             page = null;
         }
    %>
    <%Html.RenderPartial("PagerControl", page); %>
        
    <br />
    <br />
    <table class="list">
        <% // サービス売上 %>
        <tbody id="ServiceSales">
            <tr>
                <th rowspan="2" style ="white-space:nowrap"></th>
                <th rowspan="2" colspan="2" style ="white-space:nowrap">部門</th>
                <th rowspan="2" style ="white-space:nowrap">区分</th>
                <th rowspan="2" style ="white-space:nowrap">税率</th>
                <th colspan="3" style ="white-space:nowrap">工賃売上</th>
                <th colspan="3" style ="white-space:nowrap">部品売上</th>
                <th colspan="3" style ="white-space:nowrap">売上合計</th>
                <th rowspan="2" style ="white-space:nowrap">工賃原価</th>
                <th rowspan="2" style ="white-space:nowrap">部品原価</th>
                <th rowspan="2" style ="white-space:nowrap">原価合計</th>
            </tr>
            <tr>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
            </tr>
            <% if (listServiceSales != null)
                {%>
                <%int i = 0; %>
                <% foreach (var line in listServiceSales)
                    {
                        string namePrefix = string.Format("line[{0}].", i);
                        string idPrefix = string.Format("line[{0}]_", i);%>
                    <tr>
                        <!--Add 2015/09/11 arc nakayama #3165_サービス集計表対応 詳細からリンクでサービス集計表を表示-->
                        <td><a href="javascript:void(0);" onclick="OpenServiceSalesReport('<%="SalesJournalYearFrom"%>','<%="SalesJournalYearTo" %>', '<%="SalesJournalMonthFrom" %>', '<%="SalesJournalMonthTo" %>', '<%="line["+i+"]_WorkType" %>', '<%="line["+i+"]_DepartmentCode" %>')">詳細</a></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentCode)%><%=Html.Hidden(namePrefix + "DepartmentCode", line.DepartmentCode, new { id = idPrefix + "DepartmentCode"}) %></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.WorkType)%><%=Html.Hidden(namePrefix + "WorkType", line.WorkType, new { id = idPrefix + "WorkType"}) %></td>
                        <% if (line.rate != null) { %>
                        <td style ="text-align:right;white-space:nowrap"><%=Html.Encode(line.rate)%> %</td>
                        <% } else { %>
                        <td style ="text-align:right;white-space:nowrap"></td>
                        <% } %>
                        
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceTaxAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsTaxAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalTaxAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceCost)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsCost)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.Total)) %></td>
                    </tr>
                <% i++;
                } %>
            <% } %>
        </tbody>

        <% // サービス売上(社内営業) %>
        <tbody id="ServiceSalesOffice">
            <tr>
                <th rowspan="2" colspan="2" style ="white-space:nowrap">部門</th>
                <th rowspan="2" colspan="2" style ="white-space:nowrap">取引先</th>
                <th rowspan="2" style ="white-space:nowrap">新中区分</th>
                <th colspan="3" style ="white-space:nowrap">工賃売上</th>
                <th colspan="3" style ="white-space:nowrap">部品売上</th>
                <th colspan="3" style ="white-space:nowrap">売上合計</th>
                <th rowspan="2" style ="white-space:nowrap">工賃原価</th>
                <th rowspan="2" style ="white-space:nowrap">部品原価</th>
                <th rowspan="2" style ="white-space:nowrap">原価合計</th>
            </tr>
            <tr>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
                <th style ="white-space:nowrap">税抜</th>
                <th style ="white-space:nowrap">消費税</th>
                <th style ="white-space:nowrap">税込</th>
            </tr>
            <% if (listOffice != null)
                {%>
                <% foreach (var line in listOffice)
                    { %>
                    <tr>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerClaimCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerClaimName)%></td>
                        <% if (CommonUtils.DefaultString(line.AccountClassCode).Equals("000")) { %>
                            <td style ="white-space:nowrap"><b><%=Html.Encode("合計")%></b></td>
                        <% } else if (CommonUtils.DefaultString(line.AccountClassCode).Equals("001")) { %>
                            <td style ="white-space:nowrap"><%=Html.Encode("新車")%></td>
                        <% } else if (CommonUtils.DefaultString(line.AccountClassCode).Equals("002")) { %>
                            <td style ="white-space:nowrap"><%=Html.Encode("中古")%></td>
                        <% } else if (CommonUtils.DefaultString(line.AccountClassCode).Equals("999")) { %>
                            <td style ="white-space:nowrap"><%=Html.Encode("ほか")%></td>
                        <% } else { %>
                            <td style ="white-space:nowrap"><%=Html.Encode("不明")%></td>
                        <% } %>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceTaxAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceTotalAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsTaxAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsTotalAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalTaxAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TotalTotalAmount)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ServiceCost)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.PartsCost)) %></td>
                        <td style="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.Total)) %></td>
                    </tr>
                <% } %>
            <% } %>
        </tbody>


    </table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        if (document.getElementById('RequestFlag').value == null || document.getElementById('RequestFlag').value == "") {
            ExportSelected2("001", 1);
        } else {
            ExportSelected2(document.forms[0].ExportName.value, 0);
        }

    }
</script>
</asp:Content>