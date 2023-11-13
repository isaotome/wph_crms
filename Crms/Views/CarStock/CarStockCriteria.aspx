<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.CarStock>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両管理検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%CrmsLinqDataContext db = new CrmsLinqDataContext(); %>
<%CodeDao dao = new CodeDao(db); %>

<%using (Html.BeginForm("Criteria", "CarStock", new { id = 0 }, FormMethod.Post)){ %>
<%=Html.ValidationSummary() %>
<%=Html.Hidden("id", "0") %>
<%=Html.Hidden("hdOperateFlag", ViewData["OperateFlag"]) %>
<%=Html.Hidden("hdOperateUser", ViewData["OperateUser"]) %>
<%=Html.Hidden("RequestFlag", ViewData["RequestFlag"]) %>

<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <tr>
        <th style="width:80px">種別 *</th>
        <td colspan="3" style="white-space:nowrap"><%=Html.DropDownList("DataKind", (IEnumerable<SelectListItem>)ViewData["DataKindList"], new { style = "width:100px" })%><%=Html.Hidden("DefaultDataKind", "001") %></td>
    </tr>
    <tr>
        <th style="width:120px">対象年月(YYYY/MM) *</th>
        <td><%=Html.TextBox("TargetMonth", ViewData["TargetMonth"], new { @class = "alphanumeric", maxlength = 7, style = "width:80px", onblur = "if(checkTargetDate(this)){getCloseMonthControlCarStock(this.value)}"})%></td><%//Add 2018/08/28 yano #3922 %>
        <th style="width:80px">新中区分</th>
        <td style="white-space:nowrap"><%=Html.DropDownList("NewUsedType", (IEnumerable<SelectListItem>)ViewData["NewUsedTypeList"], new { style = "width:80px" })%></td>
    </tr>
   <%-- <tr>
        <th style="width:120px">仕入先</th>
        <td colspan ="3" style="width:auto; white-space:nowrap"><%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], new { maxlength = "10" , style = "width:100px", @class="alphanumeric", onblur = "GetNameFromCode('SupplierCode','SupplierName','Supplier')"})%>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog/')" /><span id="SupplierName"><%=CommonUtils.DefaultNbsp(ViewData["SupplierName"]) %></span></td>
    </tr>--%>
    <tr>
        <th>管理番号</th>
        <td style="white-space:nowrap"><%=Html.TextBox("SalesCarNumber", ViewData["SalesCarNumber"], new { maxlength = 50 })%></td>
        <th>車台番号</th>
        <td style="white-space:nowrap"><%=Html.TextBox("Vin", ViewData["Vin"], new { maxlength = 20 })%></td>
    </tr>
     <tr>
        <th></th>
        <td colspan="3" style="white-space:nowrap">
            <%//Mod 2016/11/30 #3659 車両管理　項目追加 ＣＳＶ出力ではなく、Excel出力するように変更する%>
            <%//Add 2015/01/20 arc yano IPO対応(車両在庫) 検索中、ＣＳＶダウンロード中は処理中のアイコンを表示する。 %>
            <%if(ViewData["CloseStatus"] == null || ViewData["CloseStatus"] == ""){ %>
                <input type="button" id="ciriteriaButton" value="検索" disabled="disabled" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '0'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" />
                <input type="button" id="clearaButton" value="クリア" onclick="resetCommonCriteria(new Array('DataKind', 'TargetMonth', 'NewUsedType', 'SalesCarNumber', 'Vin', 'CloseMonthCarStock')); setbuttonAttributeCarStock(''); this.blur()" /><%-- Add 2018/08/28 yano #3922 --%>
                <input type="button" id="carstockOutButton" disabled="disabled" value="車両管理表出力" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '0'; } else { this.blur() }" />
            <%}else{ %>
                <input type="button" id="ciriteriaButton" value="検索" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '0'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" />
                <input type="button" id="clearaButton" value="クリア" onclick="resetCommonCriteria(new Array('DataKind', 'TargetMonth', 'NewUsedType', 'SalesCarNumber', 'Vin', 'CloseMonthCarStock')); setbuttonAttributeCarStock(''); this.blur()" /><%-- Add 2018/08/28 yano #3922 --%>
                <input type="button" id="carstockOutButton" value="車両管理表出力" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '1'; DisplayImage('UpdateMsg', '0'); dispProgressed('PartsStock', 'UpdateMsg'); document.forms[0].submit(); document.getElementById('RequestFlag').value = '0'; } else { this.blur() }" />
            <%} %>
        </td>
    </tr>
</table>
</div>
<%//Add 2015/01/20 arc yano IPO対応(車両在庫) 検索中、ＣＳＶダウンロード中は処理中のアイコンを表示する %>
<div id="UpdateMsg" style="display:none"><img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" /></div>

<%} %>

<% //Add 2015/04/09 arc yano 締処理状況の追加%>
<br />
<div style="height:20px;"><span style="color:blue"><b>※注意：取り込みを行った車両管理データは締め処理後に反映されます</b></span></div>
<input type="button" id="finnanceDataImportButton" value="財務価格一括取込" onclick="openModalAfterRefresh('/CarStock/ImportDialog')" /><%-- Add 2018/06/06 arc yano #3883 --%>
<%if(ViewData["CloseStatus"] == null || ViewData["CloseStatus"] == ""){ %>
<input type="button" id="carStockImportButton" value="車両管理一括取込" disabled="disabled"  onclick="openModalAfterRefresh('/CarStock/ImportDialogForCarStock?TargetMonth=' + document.getElementById('TargetMonth').value)"/><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="closeCarStockButton" value="車両管理締め処理" disabled="disabled"  onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="releaseCarStockButton" value="車両管理締め解除"disabled="disabled" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<%}else if(ViewData["CloseStatus"] != null && ViewData["CloseStatus"].ToString().Equals("002")){ %>
<input type="button" id="carStockImportButton" value="車両管理一括取込" disabled="disabled"  onclick="openModalAfterRefresh('/CarStock/ImportDialogForCarStock?TargetMonth=' + document.getElementById('TargetMonth').value)"/><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="closeCarStockButton" value="車両管理締め処理" disabled="disabled"  onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="releaseCarStockButton" value="車両管理締め解除" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<%}else{ %>
<input type="button" id="carStockImportButton" value="車両管理一括取込" onclick="openModalAfterRefresh('/CarStock/ImportDialogForCarStock?TargetMonth=' + document.getElementById('TargetMonth').value)" /><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="closeCarStockButton" value="車両管理締め処理" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '2'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<input type="button" id="releaseCarStockButton" value="車両管理締め解除" disabled="disabled" onclick="if (chkDate2(document.getElementById('TargetMonth').value)) { document.getElementById('RequestFlag').value = '3'; DisplayImage('UpdateMsg', '0'); displaySearchList() } else { this.blur() }" /><%-- Add 2018/08/28 yano #3922 --%>
<%} %>
<br />
<br />
<table class="input-form">
    <tr>
        <th style="width:100px">締処理状況</th>
        <td style="white-space:nowrap; text-align:center; width:100px"><SPAN id ="CloseMonthCarStock"><%=(Html.Encode(ViewData["CloseStatusName"]))%></SPAN></td><%//締処理状況"%><%-- Mod 2018/08/28 yano #3922 --%>
    </tr>
</table>

<br />
<br />
<% // Mod 2015/01/21 arc yano IPO対応(車両管理) 処理中画面表示のため、ページ番号表示処理で部分ビューを使わずに、専用処理で表示する%>
<% //Html.RenderPartial("PagerControl",Model.PageProperty); %>
    <%if (Model.PageProperty != null)
  {%>
    <span style="font-size:12pt">
<b><%=Model.PageProperty.Count>0 ? Model.PageProperty.PageIndex*Model.PageProperty.PageSize +1 : 0%> - <%=Model.PageProperty.PageIndex*Model.PageProperty.PageSize + Model.PageProperty.Count %> / <%=Model.PageProperty.TotalCount%> </b>&nbsp;
    <%if (Model.PageProperty.PageIndex != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageProperty.PageIndex - 1%>)" onclick ="DisplayImage('UpdateMsg', '0');">&lt&lt前ページ</a>&nbsp;
    <%} %>
    <% for (int i = Model.PageProperty.StartPageIndex; i < Model.PageProperty.TotalPages && i <= Model.PageProperty.PageIndex + 10; i++)
       {
           if (Model.PageProperty.PageIndex != i)
           {%>
                <a href="javascript:displaySearchList(<%=i%>)" onclick ="DisplayImage('UpdateMsg', '0');"><%=(i + 1).ToString()%></a>&nbsp;
           <%}
           else
           {%>
                <%=i + 1%>&nbsp;
           <%} %>
       <%} %>
    <%if (Model.PageProperty.PageIndex != Model.PageProperty.TotalPages - 1 && Model.PageProperty.TotalPages != 0)
      {%>
        <a href="javascript:displaySearchList(<%=Model.PageProperty.PageIndex + 1%>)" onclick ="DisplayImage('UpdateMsg', '0');">&gt&gt次ページ</a>
    <%} %>
    </span>
<%} %>

<br />
<br />

<table class="list" style="width:230%; border-width:2px">
    <tr>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">新中区分</th>
        <% //Mod 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善②　経理課のExcel通りのレイアウトに変更 %>
        <% //Mod 2016/11/30 #3659 車両管理　項目追加 [当月実棚]欄を追加%>
        <% //Mod 2015/04/08 #3164 車両管理対応④ 他勘定受入、リサイクル料の追加、 仕入減少_他ディーラの削除、仕入減少_計の文言を変更(他勘定振替)%>
        <% //Mod 2014/10/01 IPO対応(車両管理 )経理課による指定事項の反映 ブランド別店舗を非表示化%>
        <% //<th rowspan="2" style ="white-space:nowrap">ブランド別店舗</th> %>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">入庫日</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">管理番号</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">メーカー</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">車種名称</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">車台番号</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">仕入・在庫拠点</th> <% //Mod 2018/08/28 yano #3922 %>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">当月実棚</th><% //Mod 2016/11/30 #3659 %>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">仕入区分</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px; border-left-width:2px; border-right-width:2px">仕入先</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">月初在庫</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">当月仕入</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">他勘定受入</th>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">リサイクル料</th>
        <th colspan="12" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px;border-top-width:2px; ">売上</th>
        <th colspan="1" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">仕入減少</th><% //Mod 2018/08/28 yano #3922 %>
        <th colspan="1" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">他勘定振替</th><%//2015/04/13 arc yano 車両管理対応④ 文言変更 %>
        <th rowspan="1" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">△仕入</th><%//Add 2017/08/16 arc nakayama #3782_車両仕入_キャンセル機能追加 %>
        <th rowspan="2" style ="white-space:nowrap; border-left-width:2px; border-right-width:2px">月末在庫</th>
    </tr>
    <tr>
        <th style ="white-space:nowrap; border-left-width:2px">納車日</th>
        <th style ="white-space:nowrap">伝票番号</th>
        <th style ="white-space:nowrap">販売店舗</th>
        <th style ="white-space:nowrap">販売先区分</th>
        <th style ="white-space:nowrap">販売先</th>
        <th style ="white-space:nowrap">車輌本体</th>
        <th style ="white-space:nowrap">値引</th>
        <th style ="white-space:nowrap">付属品</th>
        <th style ="white-space:nowrap">諸費用</th>
        <th style ="white-space:nowrap">売上総合計</th>
        <th style ="white-space:nowrap">売上原価</th>
        <th style ="white-space:nowrap;border-right-width:2px">粗利</th>
        <th style ="white-space:nowrap;border-left-width:2px">自社登録<</th>
       <%// <th style ="white-space:nowrap">他ﾃﾞｨｰﾗｰ</th> %>
        <th style ="white-space:nowrap">デモ代車振替</th>
      <%--  <th style ="white-space:nowrap">デモカー</th>
        <th style ="white-space:nowrap">代車</th>
       <% //Mod 2015/04/08 arc yano 車両管理対応④　レンタカー、業務車、広報車の追加 %>
        <th style ="white-space:nowrap">レンタカー</th>
        <th style ="white-space:nowrap">業務車</th>
        <th style ="white-space:nowrap">広報車</th>--%>
        <%// 2015/04/09 arc yano 車両管理対応④ 仕入ｷｬﾝｾﾙ削除 %>
        <th style ="white-space:nowrap; border-right-width:2px">仕入キャンセル</th>
    </tr>
    <%foreach (var carStock in Model)
      { %>
    <tr>
        <% //Mod 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②　大幅な変更%>
        <%//新中区分 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.NewUsedTypeName)%></td>
        <%//入庫日 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carStock.PurchaseDate))%></td>
        <%//管理番号 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.SalesCarNumber)%></td>
        <%//メーカー名 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.MakerName)%></td> 
        <%//車種名 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarName)%></td>
        <%//車台番号 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.Vin)%></td>
        <%//仕入拠点 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.PurchaseLocationName)%></td>
        <%//当月実棚 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.InventoryLocationName)%></td>
        <%//仕入区分 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarPurchaseTypeName)%></td>
        <%//仕入先 %>
        <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.SupplierName)%></td><% //Mod 2016/11/30 #3659 %>
        <%//月初在庫 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.BeginningInventory))%></td>
        <%//当月仕入 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.MonthPurchase))%></td>
        <%//他勘定受入 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.OtherAccount))%></td>
        <%//リサイクル料 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.RecycleAmount))%></td>
        <%//売上 %> 
        <%//納車日 %> 
        <td style ="white-space:nowrap; border-left-width:2px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carStock.SalesDate))%></td>
        <%//伝票番号 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.SlipNumber)%></td>
        <%//販売店舗 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.SalesDepartmentName)%></td>
        <%//販売先区分 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.SalesType)%></td>
        <%//販売先 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.CustomerName)%></td>
        <%//車両本体価格 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesPrice))%></td>
        <%//値引 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.DiscountAmount))%></td>
        <%//付属品 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.ShopOptionAmount))%></td>
        <%//諸費用 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesCostTotalAmount))%></td>
        <%//売上総合計 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesTotalAmount))%></td>
        <%//売上原価 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesCostAmount))%></td>
        <%//粗利 %>
        <td style="text-align:right; white-space:nowrap; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesProfits))%></td>
        <%//自社登録 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.SelfRegistration))%></td>
        <%//他勘定振替 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.ReductionTotal))%></td>
        <%// 2015/04/09 arc yano 車両管理対応④ 仕入ｷｬﾝｾﾙ削除 %>
        <%// Add 2017/08/16 arc nakayama #3782_車両仕入_キャンセル機能追加 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.CancelPurchase))%></td>
        <%//月末在庫 %>
        <% //<td style="text-align:right; white-space:nowrap; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.CancelCar))</td>%>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.EndInventory))%></td>


        <%//新中区分 %>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.c_NewUsedType != null ? carStock.c_NewUsedType.Name : carStock.NewUsedTypeName)%></td>--%>
        <% //Mod 2014/10/01 IPO対応(車両管理 )経理課による指定事項の反映 ブランド別店舗を非表示化%>
        <% //<td style ="white-space:nowrap"><%=Html.Encode(carStock.BrandStore)</td> %>
        <%//入庫日 %>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carStock.PurchaseDate))%></td>--%>
        <%//管理番号 %>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.SalesCarNumber)%></td>--%>
        <%//メーカー名 %>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarName)%></td>--%> 
        <%//車種名 %>
        <%--<% if(carStock.CarGrade != null){%>--%>
        <%-- <td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarGrade.Car.CarName)%></td>
        <% }else{ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarName)%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<% }%>--%>
        <%--<%//車台番号 %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.Vin)%></td>--%>
        <%//仕入拠点 %>
        <%--<% if (carStock.PurchaseLocation != null){ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.PurchaseLocation.LocationName)%></td>--%>
        <%--<% }else{ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.PurchaseLocationName)%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<% }%>--%>
        <%--<%//在庫拠点 %>--%>
        <%--<% if (carStock.CarStockLocation != null){ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarStockLocation.LocationName)%></td>--%>
        <%--<% }else{ %>--%>
        <%--<td><%=Html.Encode(carStock.CarStockLocationName)%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<% }%>--%>
        <%--<%//当月実棚 %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=carStock.Location != null ? Html.Encode(carStock.Location.LocationName) : carStock.LocationCode%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<%//仕入区分 %>--%>
        <%--<% if (carStock.c_CarPurchaseType != null){ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.c_CarPurchaseType.Name)%></td>--%>
        <%--<% }else{ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.CarPurchaseTypeName)%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<% }%>--%>
        <%--<%//仕入先 %>--%>
        <%--<% if (carStock.Supplier != null){ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.Supplier.SupplierName)%></td>--%>
        <%--<% }else{ %>--%>
        <%--<td style ="white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(carStock.SupplierName)%></td><% //Mod 2016/11/30 #3659 %>--%>
        <%--<% }%>
        <%//月初在庫 %>
        <%//Mod 2014/10/01 IPO対応(車両管理) 経理課からの指摘事項の修正 金額表記漏れの対応。 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.BeginningInventory))%></td>
        <%//当月仕入 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.MonthPurchase))%></td>
        <%//Mod 2015/04/08  他勘定受入、リサイクル料の追加。 %>
        <%//他勘定受入 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.OtherAccount))%></td>
        <%//リサイクル料 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.RecycleAmount))%></td>

        <%//売上 %>
        <%  //Mod 2015/04/08 arc yano 車両管理対応④ 自社登録の金額がnullでない場合、または仕入減少計の値が0でない場合(=仕入減少)売上欄の項目を非表示にする
         if(carStock.SelfRegistration != null ||  carStock.ReductionTotal != null){
        %>
        <td style="text-align:right; white-space:nowrap;border-left-width:2px"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap"></td>
        <td style="text-align:right; white-space:nowrap; border-right-width:2px"></td>
        <% }else{ %>
        <%//納車日 %> 
        <td style ="white-space:nowrap; border-left-width:2px"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}", carStock.SalesDate))%></td>
        <%//伝票番号 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.SlipNumber)%></td>
        <%//販売店舗 %>
        <% if (carStock.Department != null){ %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.Department.DepartmentName)%></td>
        <% }else{ %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.PurchaseDepartmentName)%></td><% //Mod 2016/11/30 #3659 %>
        <% }%>
        <%//販売先区分 %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.SalesType)%></td>
        <%//販売先 %>
        <% if (carStock.Customer != null){ %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.Customer.CustomerName)%></td>
        <% }else{ %>
        <td style ="white-space:nowrap"><%=Html.Encode(carStock.CustomerName)%></td><% //Mod 2016/11/30 #3659 %>
        <% }%>
        <%//車両本体価格 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesPrice))%></td>
        <%//値引 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.DiscountAmount))%></td>
        <%//付属品 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.ShopOptionAmount))%></td>
        <%//諸費用 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesCostTotalAmount))%></td>
        <%//売上総合計 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesTotalAmount))%></td>
        <%//売上原価 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesCostAmount))%></td>
        <%//粗利 %>
        <td style="text-align:right; white-space:nowrap; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.SalesProfits))%></td>
        <%}%>
        
        <%//自社登録 %>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.SelfRegistration))%></td>
        
        <%// 2015/04/09 arc yano 車両管理対応④ 他ディーラ削除 %>
        <% //<td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.OtherDealer))</td>%>
        <%//他勘定振替 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.ReductionTotal))%></td>
        <%//デモカー %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.DemoCar))%></td>
        <%//代車 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.TemporaryCar))%></td> 
        <%//レンタカー %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.RentalCar))%></td>
        <%//業務車 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.BusinessCar))%></td>
        <%//広報車 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.PRCar))%></td>
        <%// 2015/04/09 arc yano 車両管理対応④ 仕入ｷｬﾝｾﾙ削除 %>
        <%// Add 2017/08/16 arc nakayama #3782_車両仕入_キャンセル機能追加 %>
        <td style="text-align:right; white-space:nowrap"><%=Html.Encode(string.Format("{0:N0}", carStock.CancelPurchase))%></td>
        <%//月末在庫 %>
        <% //<td style="text-align:right; white-space:nowrap; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.CancelCar))</td>%>
        <td style="text-align:right; white-space:nowrap; border-left-width:2px; border-right-width:2px"><%=Html.Encode(string.Format("{0:N0}", carStock.EndInventory))%></td>--%>
    </tr>
    <%} %>
</table>
<br />

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        if (document.getElementById('hdOperateFlag').value == '1') {
            alert(document.getElementById('hdOperateUser').value + "さんが操作中です。\r\nしばらくしてから、再度操作を行ってください。")
        }
    }
</script>
</asp:Content>