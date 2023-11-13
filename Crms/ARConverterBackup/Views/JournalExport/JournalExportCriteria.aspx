<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarStock>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	奉行データ出力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using(Html.BeginForm("Criteria","JournalExport",FormMethod.Post)){ %>
    <%=Html.Hidden("id","0")%>
    <%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>
    <%=Html.Hidden("SearchList", ViewData["SearchList"]) %>
    <%--<%=Html.Hidden("ExportId", ViewData["ExportName"]) %>--%>
    <a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
    <br/>
    
<div id="search-form">
    <br/>
    <%=Html.ValidationSummary() %>
    <table class="input-form">
        <tr>
            <th style="width:150px">出力 *</th>
            <td style="width:300px">
                <%=Html.DropDownList("ExportName", (IEnumerable<SelectListItem>)ViewData["ExportList"], new { onchange = "document.getElementById('RequestFlag').value = '0';ExportSelected(document.forms[0].ExportName.value,1);formSubmit();" })%>
            </td>
        </tr>
        <tbody id="SalesDate">
            <tr>
                <th>納車日(YYYY/MM) *</th>
                <td> <%=Html.DropDownList("SalesJournalYearFrom", (IEnumerable<SelectListItem>)ViewData["SalesJournalYearFromList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("SalesJournalMonthFrom", (IEnumerable<SelectListItem>)ViewData["SalesJournalMonthFromList"])%>&nbsp;月 ～ <%=Html.DropDownList("SalesJournalYearTo", (IEnumerable<SelectListItem>)ViewData["SalesJournalYearToList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("SalesJournalMonthTo", (IEnumerable<SelectListItem>)ViewData["SalesJournalMonthToList"])%>&nbsp;月</td>
            </tr>
        </tbody>
        <tbody id="JournalDate">
            <tr>
                <th>伝票日付(YYYY/MM) *</th>
                <td><%=Html.DropDownList("JournalSalesYear", (IEnumerable<SelectListItem>)ViewData["JournalSalesYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("JournalSalesMonth", (IEnumerable<SelectListItem>)ViewData["JournalSalesMonthList"])%>&nbsp;月</td>
            </tr>
        </tbody>
        <tbody id="Division">
            <tr>
                <th>拠点</th>
                <td>
                    <%=Html.DropDownList("DivisionType", (IEnumerable<SelectListItem>)ViewData["DivisionTypeList"], new { onchange = "document.getElementById('OfficeCode').value = '';GetOfficeNameFromCode('OfficeCode','OfficeName','DivisionType')" })%>
                </td>
            </tr>
        </tbody>
        <tbody id="Office">
            <tr>
                <th>事業所</th>
                <td><%=Html.TextBox("OfficeCode", ViewData["OfficeCode"], new { @class = "alphanumeric", size = 10, maxlength = 3, onblur = "GetOfficeNameFromCode('OfficeCode','OfficeName','DivisionType')" })%>
                    &nbsp;<img alt="" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('OfficeCode','OfficeName','/Office/CriteriaDialog2/?DivisionType='+document.getElementById('DivisionType').value)" />
                    <span id="OfficeName"><%=ViewData["OfficeName"] %></span>
                </td>
            </tr>
        </tbody>
        <tbody id="PurchaseDate">
            <tr>
                <th>入庫日(YYYY/MM) *</th>
                <td> <%=Html.TextBox("CarPurchaseDateFrom", ViewData["CarPurchaseDateFrom"], new { @class = "alphanumeric", maxlength = 7, style = "width:80px"})%>～<%=Html.TextBox("CarPurchaseDateTo", ViewData["CarPurchaseDateTo"], new { @class = "alphanumeric", maxlength = 7, style = "width:80px"})%></td>
            </tr>
        </tbody>
        <tbody id="Department">
            <tr>
                <th>部門 (車両のみ)</th>
                <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCpde"], new { @class = "alphanumeric", size = 10, maxlength = 3, onblur = "GetDepartmentNameFromCode('DepartmentCode','DepartmentName')" })%>
                    &nbsp;<img alt="" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog3')" />
                    <span id="DepartmentName"><%=ViewData["DepartmentName"] %></span>
                </td>
            </tr>
        </tbody>
        <tbody id="Purchase">
            <tr>
                <th>仕入ステータス *</th>
                <td><%=Html.DropDownList("PurchaseStatus",(IEnumerable<SelectListItem>)ViewData["PurchaseStatusList"]) %></td>
            </tr>
        </tbody>
        <tbody id="Csv">
            <tr>
                <th></th>
                <td>
                    <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit();" />
                    <input type="button" value="クリア" onclick="clearCondition();" />
                    <%--<input type="button" value="クリア" onclick="resetCommonCriteria(new Array('SalesJournalDateFrom', 'SalesJournalDateTo', 'JournalSalesDate', 'OfficeCode', 'OfficeName', 'DivisionType', 'CarPurchaseDateFrom', 'CarPurchaseDateTo', 'DepartmentCode', 'DepartmentName', '', 'PurchaseStatus'))" />--%>
                    <input type="button" value="ＣＳＶ" onclick="document.getElementById('RequestFlag').value = '2'; document.forms[0].submit();" /><%//Mod 2021/10/25 yano #4100%>
                </td>
            </tr>
        </tbody>
        <tbody id="Journal">
            <tr>
                <th></th>
                <td>
                    <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); formSubmit();" />
                    <input type="button" value="クリア" onclick="clearCondition();" />
                    <input type="button" value="ＣＳＶ" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('JournalExport', 'UpdateMsg'); document.forms[0].submit();" /><%//Mod 2021/10/25 yano #4100%>
                    <input type="button" value="仕訳" onclick="document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); dispProgressed('JournalExport', 'UpdateMsg'); document.forms[0].submit();" />
                </td>
            </tr>
        </tbody>
    </table>
</div>
    <%} %>
    <br />
    <div id="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
    </div>
    <br />
     <%
         CrmsDao.PaginatedProperty page = null;
         CrmsDao.PaginatedList<V_CarSalesDownload> listCarSales = null;
         CrmsDao.PaginatedList<V_ReceiptTransDownload> listTrans = null;

         if (ViewData["SearchList"] != null)
         {
             switch (CommonUtils.DefaultString(ViewData["ExportName"]))
             {
                 case "002": // 新車売上
                     listCarSales = (CrmsDao.PaginatedList<V_CarSalesDownload>)ViewData["SearchList"];
                     page = listCarSales.PageProperty;
                     break;
                 case "003": // 中古車売上(AA)
                     listCarSales = (CrmsDao.PaginatedList<V_CarSalesDownload>)ViewData["SearchList"];
                     page = listCarSales.PageProperty;
                     break;
                 case "006": // 振込入金
                     listTrans = (CrmsDao.PaginatedList<V_ReceiptTransDownload>)ViewData["SearchList"];
                     page = listTrans.PageProperty;
                     break;
                 case "007": // 小口現金
                     listTrans = (CrmsDao.PaginatedList<V_ReceiptTransDownload>)ViewData["SearchList"];
                     page = listTrans.PageProperty;
                     break;
                 case "009": // ローン入金
                     listTrans = (CrmsDao.PaginatedList<V_ReceiptTransDownload>)ViewData["SearchList"];
                     page = listTrans.PageProperty;
                     break;
                 default: // 中古車売上(AA除く)
                     listCarSales = (CrmsDao.PaginatedList<V_CarSalesDownload>)ViewData["SearchList"];
                     page = listCarSales.PageProperty;
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
        <% // 中古車売上(AA除く) 新車売上 中古車売上(AA) %>
        <tbody id="CarSales">
            <tr>
                <th style ="white-space:nowrap">納車日</th>
                <th style ="white-space:nowrap">新中区分</th>
                <th style ="white-space:nowrap">伝票番号</th>
                <th style ="white-space:nowrap">管理番号</th>
                <th style ="white-space:nowrap">車台番号</th>
                <th style ="white-space:nowrap">顧客名</th>
                <th colspan="2" style ="white-space:nowrap">店舗</th>
                <th style ="white-space:nowrap">営業担当</th>
                <th style ="white-space:nowrap">車体本体価格</th>
                <th style ="white-space:nowrap">販売店オプション</th>
                <th style ="white-space:nowrap">メーカーオプション</th>
                <th style ="white-space:nowrap">AA諸費用</th>
                <th style ="white-space:nowrap">諸費用課税</th>
                <th style ="white-space:nowrap">値引</th>
                <th style ="white-space:nowrap">非課税諸費用</th>
                <th style ="white-space:nowrap">税金</th>
                <th style ="white-space:nowrap">自賠責</th>
                <th style ="white-space:nowrap">リサイクル</th>
                <th style ="white-space:nowrap">販売合計</th>
                <th style ="white-space:nowrap">下取本体</th>
                <th style ="white-space:nowrap">未払自動車税種別割</th><%-- Mod 2019/09/04 yano #4011 --%>
                <th style ="white-space:nowrap">残債</th>
                <th style ="white-space:nowrap">充当合計</th>
                <th style ="white-space:nowrap">車種</th>
                <th style ="white-space:nowrap">ブランド</th>
            </tr>
           
            <% if (listCarSales != null)
                {%>
                <% foreach (var line in listCarSales)
                    { %>
                    <tr>
                        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", line.SalesDate))%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.Name)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.SlipNumber)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.SalesCarNumber)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.Vin)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.EmployeeName)%></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.SalesPrice)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.ShopOptionAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.MakerOptionAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.AAAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.SalesCostTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.DiscountAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.OtherCostTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TaxFreeTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.CarLiabilityInsurance)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.RecycleDeposit)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.GrandTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TradeInTotalAmountExcTax)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TradeInUnexpiredCarTaxTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TradeInRemainDebtTotalAmount)) %></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.TradeInAppropriationTotalAmount)) %></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CarName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CarBrandName)%></td>
                    </tr>
                <% } %>
            <% } %>
            
        </tbody>

        <% // 振込入金 小口現金 ローン入金 %>
        <tbody id="ReceiptTransfer">
            <tr>
                <th style ="white-space:nowrap">入金日</th>
                <th style ="white-space:nowrap">区分</th>
                <th colspan="2" style ="white-space:nowrap">事業所</th>
                <th colspan="2" style ="white-space:nowrap">計上部門</th>
                <th style ="white-space:nowrap">伝票番号</th>
                <th colspan="2" style ="white-space:nowrap">請求先</th>
                <th colspan="2" style ="white-space:nowrap">科目</th>
                <th style ="white-space:nowrap">金額</th>
                <th style ="white-space:nowrap">適用</th>

            </tr>
            <!-- listTrans -->
            <% if (listTrans != null) {%>
                <% foreach (var line in listTrans) { %>
                    <tr>
                        <td style ="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", line.JournalDate))%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.Name)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.OfficeCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.OfficeName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.DepartmentName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.SlipNumber)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerClaimCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.CustomerClaimName)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.AccountCode)%></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.AccountName)%></td>
                        <td style ="text-align:right;white-space:nowrap"><%=CommonUtils.DefaultNbsp(string.Format("{0:N0}",line.Amount)) %></td>
                        <td style ="white-space:nowrap"><%=Html.Encode(line.Summary)%></td>
                    </tr>
                <% } %>
            <% } %>
        </tbody>
    </table>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        //document.getElementById('RequestFlag').value = '0';
        if (document.getElementById('RequestFlag').value == null || document.getElementById('RequestFlag').value == "") {
            ExportSelected("002", 1);
        } else {
            ExportSelected(document.forms[0].ExportName.value, 0);
        }
        
    }
</script>
</asp:Content>