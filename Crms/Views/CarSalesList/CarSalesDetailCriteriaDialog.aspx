<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetCarSalesHeaderListResult>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">

<%//Mod 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ） %>
<%//Mod 2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）%>
<%//Mod 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加 列追加%>
    納車リスト（明細）
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("CarSalesDetailCriteriaDialog", "CarSalesList", new { id = 0 }, FormMethod.Post))
  { %>
<%=Html.Hidden("id", "0") %>
<%//=Html.Hidden("SelectYearCode", ViewData["SelectYearCode"]) %>
<%//=Html.Hidden("SelectMonthCode", ViewData["SelectMonthCode"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %><%//押下ボタン判定(1:検索 / 2:Excel出力)%>
<script type="text/javascript">
    (function ($, window, undefined) {
        $(function () {
            $('.list tr:even').addClass('even');
        });
    }(jQuery, window));
</script>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th>納車日(YYYY/MM) *</th>
        <td> <%=Html.DropDownList("SelectYearCode", (IEnumerable<SelectListItem>)ViewData["TargetYearList"])%>&nbsp;年&nbsp;<%=Html.DropDownList("SelectMonthCode", (IEnumerable<SelectListItem>)ViewData["TargetMonthList"])%>&nbsp;月</td>
        <th>部門コード</th>
        <td style="width:200px;"><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", style = "width:30px", onchange = "GetNameFromCode('DepartmentCode','DepartmentName','Department','false')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')"/><span id="DepartmentName" style ="width:160px"><%=Html.Encode(ViewData["DepartmentName"]) %></span></td>
    </tr>
    <tr>
        <th>新中区分</th>
        <td> <%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"])%></td>
        <%//Mod 2020/01/16 yano #4027%>
        <th>区分</th>
        <td><%=Html.DropDownList("AAType", (IEnumerable<SelectListItem>)ViewData["AATypeList"])%></td>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
        <input type="button" value="検索" onclick="document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); displaySearchList()"/>
        <input type="button" value="Excel出力" onclick="document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); dispProgressed('CustomerDataList', 'UpdateMsg'); document.forms[0].submit(); inProcess = 0"/>
        </td>
    </tr>
</table>
</div>
<%} %>
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />

<table class="list">
    <tr>
        <th style="white-space:nowrap">納車日</th>
        <th style="white-space:nowrap">新中区分</th>
        <th style="white-space:nowrap">伝票番号</th>
        <th style="white-space:nowrap">管理番号</th>
        <th style="white-space:nowrap">車台番号</th>
        <th style="white-space:nowrap">顧客名</th>
        <th style="white-space:nowrap">部門コード</th>
        <th style="white-space:nowrap">部門名</th>
        <th style="white-space:nowrap">営業担当</th>
        <th style="white-space:nowrap">車両本体価格</th>
        <th style="white-space:nowrap">販売店オプション</th>
        <th style="white-space:nowrap">メンテナンスパッケージ</th> <%//Add 2023/01/16 yano #4138%>
        <th style="white-space:nowrap">延長保証</th>               <%//Add 2023/01/16 yano #4138%>
        <th style="white-space:nowrap">特別サーチャージ</th>       <%//Add 2023/09/18 yano #4181%>
        <th style="white-space:nowrap">メーカーオプション</th>
        <th style="white-space:nowrap">ＡＡ諸費用</th><%//Add 2017/10/14 arc yano #3790 %>
        <th style="white-space:nowrap">諸費用課税</th>
        <th style="white-space:nowrap">値引</th>
        <th style="white-space:nowrap">非課税諸費用</th>
        <th style="white-space:nowrap">税金</th>
        <th style="white-space:nowrap">自賠責</th>
        <th style="white-space:nowrap">リサイクル</th>
        <th style="white-space:nowrap">販売合計</th>
        <th style="white-space:nowrap">下取本体</th>
        <th style="white-space:nowrap">下取車車台番号(1)</th><%//Add 2017/10/14 arc yano #3790 %>
        <th style="white-space:nowrap">下取車車台番号(2)</th><%//Add 2017/10/14 arc yano #3790 %>
        <th style="white-space:nowrap">下取車車台番号(3)</th><%//Add 2017/10/14 arc yano #3790 %>
        <th style="white-space:nowrap">未払自動車税種別割</th><%-- Mod 2019/09/04 yano #4011 --%>
        <th style="white-space:nowrap">残債</th>
        <th style="white-space:nowrap">充当合計</th>
        <th style="white-space:nowrap">車種</th>
        <th style="white-space:nowrap">ブランド</th>
    </tr>
    <%foreach(var a in Model){ %>
        <tr>
            <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",a.SalesDate))%></td><!--納車日-->
            <td style="white-space:nowrap"><%=Html.Encode(a.NewUsedTypeName)%></td><!--新中-->
            <td style="white-space:nowrap"><%=Html.Encode(a.SlipNumber)%></td><!--伝票番号-->
            <td style="white-space:nowrap"><%=Html.Encode(a.SalesCarNumber)%></td><!--管理番号-->
            <td style="white-space:nowrap"><%=Html.Encode(a.Vin)%></td><!--車台番号-->
            <td style="white-space:nowrap"><%=Html.Encode(a.CustomerName)%></td><!--顧客名-->
            <td style="white-space:nowrap"><%=Html.Encode(a.DepartmentCode)%></td><!--部門コード-->
            <td style="white-space:nowrap"><%=Html.Encode(a.DepartmentName)%></td><!--部門名-->
            <td style="white-space:nowrap"><%=Html.Encode(a.Employeename)%></td><!--営業担当-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.SalesPrice))%></td><!--車両本体価格-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.ShopOptionAmountWithTax))%></td><!--販売店オプション-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.MaintenancePackageAmount))%></td><!--メンテナンスパッケージ--> <%//Add 2023/01/16 yano #4138%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.ExtendedWarrantyAmount))%></td><!--延長保証-->                 <%//Add 2023/01/16 yano #4138%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.SurchargeAmount))%></td><!--特別サーチャージ-->                <%//Add 2023/09/18 yano #4181%>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.MakerOptionAmount))%></td><!--メーカーオプション-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.AAAmount))%></td><!--AA諸費用--><%//Add 2017/10/14 arc yano #3790 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.SalesCostTotalAmount))%></td><!--諸費用課税-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.DiscountAmount))%></td><!--値引-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.OtherCostTotalAmount))%></td><!--非課税諸費用-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TaxFreeTotalAmount))%></td><!--税金-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.CarLiabilityInsurance))%></td><!--自賠責-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.RecycleDeposit))%></td><!--リサイクル-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.GrandTotalAmount))%></td><!--販売合計-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInTotalAmountNotTax))%></td><!--下取本体-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInVin1))%></td><!--下取車車台番号(1)--><%//Add 2017/10/14 arc yano #3790 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInVin2))%></td><!--下取車車台番号(2)--><%//Add 2017/10/14 arc yano #3790 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInVin3))%></td><!--下取車車台番号(3)--><%//Add 2017/10/14 arc yano #3790 %>
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInUnexpiredCarTaxTotalAmount))%></td><!--未払自動車税種別割-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInRemainDebtTotalAmount))%></td><!--残債-->
            <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:N0}",a.TradeInAppropriationTotalAmount))%></td><!--充当合計-->
            <td style="white-space:nowrap"><%=Html.Encode(a.CarName)%></td><!--車種-->
            <td style="white-space:nowrap"><%=Html.Encode(a.CarBrandName)%></td><!--ブランド-->
        </tr>
    <%} %>
</table>
<br />
</asp:Content>
