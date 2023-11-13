<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/MainPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.PaginatedList<CrmsDao.GetPartsPurchase_Result>>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	部品入荷検索
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%using (Html.BeginForm("Criteria", "PartsPurchase", new { id = 0 }, FormMethod.Post)) { %>
<%=Html.Hidden("id","0") %>
<%=Html.Hidden("DefaultDepartmentCode", ViewData["DefaultDepartmentCode"]) %>
<%=Html.Hidden("DefaultDepartmentName", ViewData["DefaultDepartmentName"]) %>
<%=Html.Hidden("DefaultPurchaseStatus", ViewData["DefaultPurchaseStatus"]) %>
<%=Html.Hidden("DefaultPurchaseType", ViewData["DefaultPurchaseType"]) %>
<a href="javascript:void(0);" onclick="changeSearchDisplay();return false;">▼検索フォーム表示/非表示▼</a>
<div id="search-form">
<br />
<table class="input-form">
    <!--Add 2015/11/10 arc nakayama #7468_部品仕入れ機能の改善 検索条件変更（追加・変更・削除）-->
    <tr>
        <th>入荷伝票番号</th>
        <td><%=Html.TextBox("PurchaseNumberFrom", ViewData["PurchaseNumberFrom"], new { @class = "alphanumeric", size = "10", maxlength = "50" })%>～<%=Html.TextBox("PurchaseNumberTo", ViewData["PurchaseNumberTo"], new { @class = "alphanumeric", size = "10", maxlength = "50" })%></td>
        <% // Add 2014/09/04 arc amii 検索条件対応 #3041 受注伝票番号の検索条件項目を追加 %>
        <th>発注伝票番号</th>
        <td><%=Html.TextBox("PurchaseOrderNumberFrom",ViewData["PurchaseOrderNumberFrom"],new {@class="alphanumeric",size="10",maxlength="50"}) %>～<%=Html.TextBox("PurchaseOrderNumberTo",ViewData["PurchaseOrderNumberTo"],new {@class="alphanumeric",size="10",maxlength="50"}) %></td>
    </tr>
    <tr>
        <th>発注日</th>
        <td><%=Html.TextBox("PurchaseOrderDateFrom",ViewData["PurchaseOrderDateFrom"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchaseOrderDateFrom').value, 'PurchaseOrderDateFrom')"}) %>～<%=Html.TextBox("PurchaseOrderDateTo",ViewData["PurchaseOrderDateTo"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchaseOrderDateTo').value, 'PurchaseOrderDateTo')"}) %></td>
        <th>受注伝票番号</th>
        <td><%=Html.TextBox("SlipNumberFrom", ViewData["SlipNumberFrom"], new { @class = "alphanumeric", size = "10", maxlength = "50" })%>～<%=Html.TextBox("SlipNumberTo", ViewData["SlipNumberTo"], new { @class = "alphanumeric", size = "10", maxlength = "50" })%></td>
    </tr>
    <tr>
        <th>入荷伝票区分</th>
        <td><%=Html.DropDownList("PurchaseType",(IEnumerable<SelectListItem>)ViewData["PurchaseTypeList"]) %></td>
        <th>発注区分</th>
        <td><%=Html.DropDownList("OrderType",(IEnumerable<SelectListItem>)ViewData["OrderTypeList"]) %></td>
    </tr>
    <tr>
        <th rowspan="2">顧客</th>
        <td><%=Html.TextBox("CustomerCode", ViewData["CustomerCode"], new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('CustomerCode','CustomerName','Customer')" })%>&nbsp;<img alt="顧客コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CustomerCode','CustomerName','/Customer/CriteriaDialog')" /></td>
        <th rowspan="2">部品</th>
        <td><%=Html.TextBox("PartsNumber", ViewData["PartsNumber"], new { @class = "alphanumeric", size = 15 , maxlength = 25 , onblur = "GetNameFromCode('PartsNumber','PartsNameJp','Parts')"})%>&nbsp;<img alt="部品検索" style="cursor:pointer" src="/Content/Images/Search.jpg" onclick="openSearchDialog('PartsNumber','PartsNameJp','/Parts/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="CustomerName"><%=Html.Encode(ViewData["CustomerName"]) %></span></td>
        <td><span id="PartsNameJp"><%=Html.Encode(ViewData["PartsNameJp"]) %></span></td>
    </tr>
    <tr>
        <th>入荷予定日</th>
        <td><%=Html.TextBox("PurchasePlanDateFrom",ViewData["PurchasePlanDateFrom"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchasePlanDateFrom').value, 'PurchasePlanDateFrom')"}) %>～<%=Html.TextBox("PurchasePlanDateTo",ViewData["PurchasePlanDateTo"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchasePlanDateTo').value, 'PurchasePlanDateTo')"}) %></td>
        <th>入荷日</th>
        <td><%=Html.TextBox("PurchaseDateFrom",ViewData["PurchaseDateFrom"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchaseDateFrom').value, 'PurchaseDateFrom')"}) %>～<%=Html.TextBox("PurchaseDateTo",ViewData["PurchaseDateTo"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('PurchaseDateTo').value, 'PurchaseDateTo')"}) %></td>
    </tr>
    <tr>
        <th rowspan="2">部門 * </th><%//Mod 2016/08/08 arc yano #3624 入力必須項目に変更%>
        <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = "10", maxlength = "3",onblur="GetNameFromCode('DepartmentCode','DepartmentName','Department')" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog')" /></td>
        <th rowspan="2">サービスフロント</th>
        <td><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", maxlength = "50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')" })%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
    </tr>
    <tr>
        <td><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
        <td><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
    </tr>
    <tr>
        <th style="width:100px" rowspan="2">仕入先</th>
        <td style="width:200px"><%=Html.TextBox("SupplierCode",ViewData["SupplierCode"],new {@class="alphanumeric",size="10",maxlength="10",onblur="GetNameFromCode('SupplierCode','SupplierName','Supplier')"}) %>&nbsp;<img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('SupplierCode','SupplierName','/Supplier/CriteriaDialog')" /></td>
        <th>WEBオーダー番号</th>
        <td><%=Html.TextBox("WebOrderNumber",ViewData["WebOrderNumber"],new {@class="alphanumeric",size="15",maxlength="50"}) %></td>        
    </tr>
    <tr>
        <td style="height:20px"><span id="SupplierName"><%=ViewData["SupplierName"] %></span></td>
        <th>メーカーオーダー番号</th>
        <td><%=Html.TextBox("MakerOrderNumber",ViewData["MakerOrderNumber"],new {@class="alphanumeric",size="15",maxlength="50"}) %></td>
    </tr>
    <tr>
        <th>インボイス番号</th>
        <td><%=Html.TextBox("InvoiceNo",ViewData["InvoiceNo"],new {@class="alphanumeric",size="15",maxlength="50"}) %></td>
        <th>入荷ステータス</th>
        <td><%=Html.DropDownList("PurchaseStatus", (IEnumerable<SelectListItem>)ViewData["PurchaseStatusList"], new { onchange = "ChangePurchaseStatus()"})%></td>
    </tr>
    <%// 2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加 %>
    <tr>
        <th>LinkEntry取込日</th>
        <td><%=Html.TextBox("LinkEntryCaptureDateFrom",ViewData["LinkEntryCaptureDateFrom"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('LinkEntryCaptureDateFrom').value, 'LinkEntryCaptureDateFrom')"}) %>～<%=Html.TextBox("LinkEntryCaptureDateTo",ViewData["LinkEntryCaptureDateTo"],new {@class="alphanumeric",size="10",maxlength="10", onchange ="return chkDate3(document.getElementById('LinkEntryCaptureDateTo').value, 'LinkEntryCaptureDateTo')"}) %></td>
        <th colspan="3"></th>
    </tr>
    <tr>
        <th></th>
        <td colspan="3">
            <%//Mod 2016/08/08 arc yano #3624　検索条件に部門コードが入力されていない場合は、アラートを表示し、検索を行わない%>
            <%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
            <input type="button" value="検索" onclick="if(document.getElementById('DepartmentCode').value == ''){alert('部門コードは入力必須項目です。入力後、再度検索を実行して下さい'); return false; } DisplayImage('UpdateMsg','0');formSubmit()" />
             <% // Mod 2014/09/04 arc amii 検索条件対応 #3041 クリアする項目に受注伝票番号項目を追加 %>
            <input type="button" value="クリア" onclick="resetCommonCriteria(new Array('PurchaseNumberFrom', 'PurchaseNumberTo', 'SlipNumberFrom', 'SlipNumberTo', 'PurchaseOrderNumberFrom', 'PurchaseOrderNumberTo', 'PurchaseOrderDateFrom', 'PurchaseOrderDateTo', 'PurchasePlanDateFrom', 'PurchasePlanDateTo', 'PurchaseType', 'OrderType', 'DepartmentCode', 'CustomerCode', 'CustomerName', 'PartsNumber', 'PartsNameJp', 'DepartmentName', 'PurchaseDateFrom', 'PurchaseDateTo', 'EmployeeCode', 'EmployeeName', 'SupplierCode', 'MakerOrderNumber', 'WebOrderNumber', 'PurchaseStatus', 'InvoiceNo','LinkEntryCaptureDateFrom', 'LinkEntryCaptureDateTo')); CheckPurchaseStatus();" /><%// 2018/03/26 arc yano #3863 %>
        </td>
    </tr>
</table>
</div>
<%//} %> <%//Mod 2016/03/15 arc yano #3472 一覧の下へ移動 %>
<br />
<%--// Mod 2015/06/30 arc ishii 検索時インジケータを表示するよう修正--%>
<div id ="UpdateMsg" style="display:none">
    <img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<input type="button" value="新規作成" onclick="openPartsPurchaseDialog(0, '', '', '', 1, '001')" /><%//Mod 2016/06/20 arc yano #3585 引数追加 %>
<!--<input type="button" value="チェックした項目の編集" style="width:150px" onclick="openPartsPurchaseDialog(<%=Model.Count %>, '', '', '', 3)"/>-->
    <input type="button" value="チェックした項目の編集" style="width:150px" onclick="openPartsPurchaseDialog(<%=Model.Count %>, '', '', '', 3,'<%=ViewData["PurchaseStatus"]%>', document.forms[0])"/><%//Mod 2016/06/20 arc yano #3585 引数追加 %>
<input type="button" value="Excel取込(LinkEntry)" style="width:150px" onclick="openModalAfterRefresh('/PartsPurchase/ImportDialog', '', '', 'no', 'no')" />
<br />
<br />
<%Html.RenderPartial("PagerControl",Model.PageProperty); %>
<br />
<br />
<table class="list">
    <%if(ViewData["PurchaseStatus"].ToString().Equals("001")) {%>        
    <tr>
        <!--Mod 2016/03/14 arc yano #3469 部品仕入一覧　[全てにチェック]の追加　 [全てにチェック]のチェックボックスを追加-->
        <th><input type="checkbox" id="allCheck" value="0" onclick="allChecked(this)"/></th><!--チェックボックス-->
        <th style="white-space:nowrap">入荷伝票番号</th>
        <th style="white-space:nowrap">発注伝票番号</th>
        <th style="white-space:nowrap">発注日</th>
        <th style="white-space:nowrap">受注伝票番号</th>
        <th style="white-space:nowrap">顧客名</th>
        <th style="white-space:nowrap">部品番号</th>
        <th style="white-space:nowrap">部品名</th>
        <th style="white-space:nowrap">発注残</th>
        <th style="white-space:nowrap">入荷予定数</th>
        <th style="white-space:nowrap">発注区分</th>
        <th style="white-space:nowrap">入荷予定日</th>
        <th style="white-space:nowrap">WEBオーダー番号</th>
        <th style="white-space:nowrap">メーカーオーダー番号</th>
        <th style="white-space:nowrap">インボイス番号</th>
        <th style="white-space:nowrap">部門名</th>
        <th style="white-space:nowrap">サービスフロント</th>
        <th style="white-space:nowrap">仕入先名</th>
        <th style="white-space:nowrap">LinkEntry取込日</th><%//Add 2018/03/26 arc yano #3863 %>
    </tr>
        <%for(int i = 0; i < Model.Count; i++)
          {
              string PurchaseNumber = Model[i].PurchaseNumber;
              string PurchaseOrderNumber = Model[i].PurchaseOrderNumber;
              string PurchaseOrderDate = string.Format("{0:yyyy/MM/dd}", Model[i].PurchaseOrderDate);
              string SlipNumber = Model[i].SlipNumber;
              string CustomerName = Model[i].CustomerName;
              string PartsNumber = Model[i].PartsNumber;
              string PartsNameJp = Model[i].PartsNameJp;
              string RemainingQuantity = string.Format("{0:F2}", Model[i].RemainingQuantity);
              string PurchasePlanQuantity = string.Format("{0:F2}", Model[i].PurchasePlanQuantity);
              string OrderTypeName = Model[i].OrderTypeName;
              string PurchasePlanDate = string.Format("{0:yyyy/MM/dd}", Model[i].PurchasePlanDate);
              string WebOrderNumber = Model[i].WebOrderNumber;
              string MakerOrderNumber = Model[i].MakerOrderNumber;
              string InvoiceNo = Model[i].InvoiceNo;
              string DepartmentName = Model[i].DepartmentName;
              string EmployeeName = Model[i].EmployeeName;
              string SupplierName = Model[i].SupplierName;

              string LinkEntryCaptureDate = string.Format("{0:yyyy/MM/dd}", Model[i].LinkEntryCaptureDate);  //Add 2018/03/26 arc yano #3863
              
              //Mod 2016/03/15 arc yano #3472 部品入荷一覧　チェックした項目の編集 チェックした項目の情報の成形をサーバ側で行うように対応
              string id   = string.Format("KeyList[{0}]_", i);
              string name = string.Format("KeyList[{0}].", i);
              
             %>
            <tr>
                <!--Mod 2016/03/14 arc yano #3469 部品仕入一覧　[全てにチェック]の追加　チェックボックスにname属性を追加 -->
                <!--Add 2015/11/10 arc nakayama #7468_部品仕入れ機能の改善 チェックボックス追加-->
                <td style="text-align:center"><%=Html.CheckBox(string.Format("check[{0}]", i), new { id = string.Format("check[{0}]", i) })%><%--<%=Html.Hidden(string.Format("Key[{0}]", i), PurchaseNumber != null && PurchaseNumber != "" ? PurchaseNumber : PurchaseOrderNumber, new { id = string.Format("Key[{0}]", i) })%>--%></td>

                <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openPartsPurchaseDialog(0, '<%=PurchaseNumber%>', '<%=PurchaseOrderNumber%>', '<%=PartsNumber%>', 2, '<%=ViewData["PurchaseStatus"]%>');"><%=PurchaseNumber%></a><%=Html.Hidden(string.Format("PurchaseNumber[{0}]", i), PurchaseNumber != null && PurchaseNumber != "" ? PurchaseNumber : "", new { id = string.Format("PurchaseNumber[{0}]", i) })%><%=Html.Hidden(name + "PurchaseNumber", PurchaseNumber != null && PurchaseNumber != "" ? PurchaseNumber : "", new { id = id + "PurchaseNumber"})%><%//Mod 2016/03/15 arc yano #3472 %></td><!--入荷伝票番号-->
                <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openPartsPurchaseDialog(0, '', '<%=PurchaseOrderNumber%>', '', 4);"><%=PurchaseOrderNumber%></a><%=Html.Hidden(string.Format("PurchaseOrderNumber[{0}]", i), PurchaseOrderNumber != null && PurchaseOrderNumber != "" ? PurchaseOrderNumber : "", new { id = string.Format("PurchaseOrderNumber[{0}]", i) })%><%=Html.Hidden(name + "PurchaseOrderNumber", PurchaseOrderNumber != null && PurchaseOrderNumber != "" ? PurchaseOrderNumber : "", new { id = id + "PurchaseOrderNumber"})%><%//Mod 2016/03/15 arc yano #3472 %></td><!--発注伝票番号--><%//Mod 2021/05/25 yano #4045 Chrome対応 idの誤り訂正(PurchaseNumber→PurchaseOrderNumber %>
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(PurchaseOrderDate)%></td>                          <!--発注日-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(SlipNumber) %></td>                                <!--受注伝票番号-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(CustomerName)%></td>                               <!--顧客名-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(PartsNumber) %><%=Html.Hidden(string.Format("PartsNumber[{0}]", i), PartsNumber != null && PartsNumber != "" ? PartsNumber : "", new { id = string.Format("PartsNumber[{0}]", i) })%><%=Html.Hidden(name + "PartsNumber", PartsNumber != null && PartsNumber != "" ? PartsNumber : "", new { id = id + "PartsNumber"})%><%//Mod 2016/03/15 arc yano #3472 %></td><!--部品番号-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(PartsNameJp) %></td>                               <!--部品名-->
                <td style="white-space:nowrap; text-align:right"><%=CommonUtils.DefaultNbsp(RemainingQuantity)%></td>        <!--発注残-->
                <td style="white-space:nowrap; text-align:right"><%=CommonUtils.DefaultNbsp(PurchasePlanQuantity)%></td>     <!--入荷予定数-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(OrderTypeName)%></td>                              <!--発注区分-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(PurchasePlanDate)%></td>                           <!--入荷予定日-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(WebOrderNumber) %></td>                            <!--WEBオーダー番号-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(MakerOrderNumber)%></td>                           <!--メーカーオーダー番号-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(InvoiceNo)%></td>                                  <!--インボイス番号-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(DepartmentName)%></td>                             <!--部門-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(EmployeeName)%></td>                               <!--サービスフロント-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(SupplierName)%></td>                               <!--仕入先-->
                <td style="white-space:nowrap"><%=CommonUtils.DefaultNbsp(LinkEntryCaptureDate)%></td>                       <!--取込日(LinkEntry)--><%//Add 2018/03/26 arc yano #3863 %>
            </tr>
        <%} %>
    <%}else{ %>
        <tr>
            <th style="white-space:nowrap">入荷伝票番号</th>                <!--入荷伝票番号-->
            <th style="white-space:nowrap">発注伝票番号</th>                <!--発注伝票番号-->
            <th style="white-space:nowrap">発注日</th>                      <!--発注日-->
            <th style="white-space:nowrap">受注伝票番号</th>                <!--受注伝票番号-->
            <th style="white-space:nowrap">入荷伝票区分</th>                <!--仕入伝票区分-->
            <th style="white-space:nowrap">顧客名</th>                      <!--顧客名-->
            <th style="white-space:nowrap">部品番号</th>                    <!--部品番号-->
            <th style="white-space:nowrap">部品名</th>                      <!--部品名-->
            <th style="white-space:nowrap">発注数</th>                      <!--発注数-->
            <th style="white-space:nowrap">入荷数</th>                      <!--仕入数-->
            <th style="white-space:nowrap">発注区分</th>                    <!--発注区分-->
            <th style="white-space:nowrap">入荷予定日</th>                  <!--入荷予定日-->
            <th style="white-space:nowrap">入荷日</th>                      <!--入荷日-->
            <th style="white-space:nowrap">WEBオーダー番号</th>             <!--WEBオーダー番号-->
            <th style="white-space:nowrap">メーカーオーダー番号</th>        <!--メーカーオーダー番号-->
            <th style="white-space:nowrap">インボイス番号</th>              <!--納品書番号(orインボイス番号)-->
            <th style="white-space:nowrap">部門名</th>                      <!--部門-->
            <th style="white-space:nowrap">サービスフロント</th>            <!--サービスフロント-->
            <th style="white-space:nowrap">仕入先名</th>                    <!--仕入先-->
            <th style="white-space:nowrap">LinkEntry取込日</th>             <%//Add 2018/03/26 arc yano #3863 %>
        </tr>
            <%// Mod 2016/06/20 arc yano #3585 部品入荷一覧　入荷済入荷データの詳細画面の表示
          int i = 0;
                foreach(var a in Model){

                  string PurchaseNumber = Model[i].PurchaseNumber;
                  string PurchaseOrderNumber = Model[i].PurchaseOrderNumber;
                  string PartsNumber = Model[i].PartsNumber;

                  string id = string.Format("KeyList[{0}]_", i);
                  string name = string.Format("KeyList[{0}].", i);                 
             %>
            <tr>
                <td style="white-space:nowrap"><a href="javascript:void(0);" onclick="openPartsPurchaseDialog(0, '<%=PurchaseNumber%>', '<%=PurchaseOrderNumber%>', '<%=PartsNumber%>', 2, '<%=ViewData["PurchaseStatus"]%>');"><%=PurchaseNumber%></a><%=Html.Hidden(string.Format("PurchaseNumber[{0}]", i), PurchaseNumber != null && PurchaseNumber != "" ? PurchaseNumber : "", new { id = string.Format("PurchaseNumber[{0}]", i) })%><%=Html.Hidden(name + "PurchaseNumber", PurchaseNumber != null && PurchaseNumber != "" ? PurchaseNumber : "", new { id = id + "PurchaseNumber"})%></td>
                <td style="white-space:nowrap"><%=Html.Encode(a.PurchaseOrderNumber)%><%=Html.Hidden(string.Format("PurchaseOrderNumber[{0}]", i), PurchaseOrderNumber != null && PurchaseOrderNumber != "" ? PurchaseOrderNumber : "", new { id = string.Format("PurchaseOrderNumber[{0}]", i) })%><%=Html.Hidden(name + "PurchaseOrderNumber", PurchaseOrderNumber != null && PurchaseOrderNumber != "" ? PurchaseOrderNumber : "", new { id = id + "PurchaseOrderNumber"})%></td><!--発注伝票番号--><%//Mod 2021/05/25 yano #4045 Chrome対応 idの誤り訂正(PurchaseNumber→PurchaseOrderNumber %>
                <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",a.PurchaseOrderDate))%></td><!--発注日-->
                <td style="white-space:nowrap"><%=Html.Encode(a.SlipNumber)%></td>                                       <!--受注伝票番号-->
                <td style="white-space:nowrap"><%=Html.Encode(a.PurchaseTypeName)%></td>                                 <!--仕入伝票区分-->
                <td style="white-space:nowrap"><%=Html.Encode(a.CustomerName)%></td>                                     <!--顧客名-->
                <td style="white-space:nowrap"><%=Html.Encode(a.PartsNumber)%><%=Html.Hidden(string.Format("PartsNumber[{0}]", i), PartsNumber != null && PartsNumber != "" ? PartsNumber : "", new { id = string.Format("PartsNumber[{0}]", i) })%><%=Html.Hidden(name + "PartsNumber", PartsNumber != null && PartsNumber != "" ? PartsNumber : "", new { id = id + "PartsNumber"})%></td>                                      <!--部品番号-->
                <td style="white-space:nowrap"><%=Html.Encode(a.PartsNameJp)%></td>                                      <!--部品名-->
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F2}",a.PurchaseOrderQuantity))%></td><!--発注数-->
                <td style="white-space:nowrap; text-align:right"><%=Html.Encode(string.Format("{0:F2}",a.PurchaseQuantity))%></td><!--入荷数-->
                <td style="white-space:nowrap"><%=Html.Encode(a.OrderTypeName)%></td>                                    <!--発注区分-->
                <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",a.PurchasePlanDate))%></td> <!--入荷予定日-->
                <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",a.PurchaseDate))%></td>     <!--入荷日-->
                <td style="white-space:nowrap"><%=Html.Encode(a.WebOrderNumber)%></td>                                   <!--WEBオーダー番号-->
                <td style="white-space:nowrap"><%=Html.Encode(a.MakerOrderNumber)%></td>                                 <!--メーカーオーダー番号-->
                <td style="white-space:nowrap"><%=Html.Encode(a.InvoiceNo)%></td>                                        <!--インボイス番号-->
                <td style="white-space:nowrap"><%=Html.Encode(a.DepartmentName)%></td>                                   <!--部門-->
                <td style="white-space:nowrap"><%=Html.Encode(a.EmployeeName)%></td>                                     <!--サービスフロント-->
                <td style="white-space:nowrap"><%=Html.Encode(a.SupplierName)%></td>                                     <!--仕入先-->
                <td style="white-space:nowrap"><%=Html.Encode(string.Format("{0:yyyy/MM/dd}",a.LinkEntryCaptureDate))%></td><!--取込日--><%//Add 2018/03/26 arc yano #3863 %>
            </tr>
            <%
                    i++;
            }%>
        <%} %>
</table>
<%} %>  <%//Mod 2016/03/15 arc yano #3472 部品入荷一覧　チェックした項目の編集 一覧の内容をformに含める %>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    window.onload = function (e) {
        ChangePurchaseStatus();
    }
</script>
</asp:Content>
