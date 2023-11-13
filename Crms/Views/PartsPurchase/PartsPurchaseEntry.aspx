<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master"
    Inherits="System.Web.Mvc.ViewPage<CrmsDao.PartsPurchase_PurchaseList>" %>
<%@ Import Namespace="CrmsDao" %> 

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品入荷入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<% 
/*---------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： 部品入荷処理ダイアログ                                                                                           */
/* 作成日： 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)                                 */
/* 更新日：                                                                                                                  */
/*          2018/11/12 yano #3949 部品入荷入力　入荷済データの編集                                                           */
/*          2018/06/01 arc yano #3894 部品入荷入力 JLR用デフォルト仕入先対応                                                 */
/*          2017/11/07 arc yano #3807 部品入荷入力　１０行ボタンの追加                                                       */
/*          2017/11/06 arc yano #3808 部品入荷入力 Webオーダー番号の追加                                                     */
/*          2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能 取消・キャンセルボタン追加                       */
/*                              入荷キャンセルの場合は編集を行えるように修正                                                 */
/*          2017/02/14 arc yano #3641 金額欄のカンマ表示対応                                                                 */
/*                                        ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更                         */
/*                                        ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする                        */
/*          2016/06/20 arc yano #3585 部品入荷一覧　入荷済入荷データの詳細画面の表示                                         */
/*          2016/03/15 arc yano #3470 入荷確定ボタンの文言の変更(入荷確定→確定)                                             */
/*          2016/02/01 arc yano #3421部品入荷入力画面　必須／任意項目の識別 必須項目に*を追加                                */
/*          2015/11/20 arc nakayama 仕様変更に伴い再作成　                                                                   */
/*---------------------------------------------------------------------------------------------------------------------------*/        
%>
    <table class="command">
        <tr>
            <td onclick="formClose();ParentformSubmit();">
                <img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる
            </td>
            <%if(ViewData["PurchaseStatus"].ToString().Equals("001")){ //入荷ステータス = 「未入荷」 //016/06/20 arc yano #3585%>
                <td onclick="document.getElementById('RequestFlag').value = '99'; CheckRemainingQuantityForCommit(<%=Model.line.Count %>)">
                    <img src="/Content/Images/build.png" alt="確定" class="command_btn" />&nbsp;確定  <%//2016/03/15 arc yano #3470 %>
                </td>
            <%}else{%><%//Add 2017/08/02 arc yano #3783 %>
                <td onclick=" document.getElementById('RequestFlag').value = '3'; formSubmit(); ParentformSubmit()"><img src="/Content/Images/build.png" alt="保存" class="command_btn" />&nbsp;保存</td><%//Add 2018/11/12 yano #3949 %>
                <%if (ViewData["PurchaseType"] != null && !ViewData["PurchaseType"].Equals("002"))
                  {%>
                    <%if(ViewData["InventoryStatus"] != null &&  ViewData["InventoryStatus"].Equals("002")){%>
                        <td onclick="if(confirm('入荷キャンセルを実行しても良いですか？') == true){ document.getElementById('RequestFlag').value = '2'; formSubmit(); ParentformSubmit()}">
                        <img src="/Content/Images/cancel.png" alt="入荷キャンセル" class="command_btn" />&nbsp;入荷キャンセル 
                    </td>
                    <%}else{ %>
                        <td onclick="if(confirm('入荷キャンセルを実行しても良いですか？') == true){ document.getElementById('RequestFlag').value = '1'; formSubmit(); ParentformSubmit()}">
                            <img src="/Content/Images/cancel.png" alt="入荷キャンセル" class="command_btn" />&nbsp;入荷キャンセル
                        </td>
                    <%} %>
                <%} %>
                
            <%} %>
        </tr>
    </table>
    <br />
    <%using (Html.BeginForm("Entry", "PartsPurchase", FormMethod.Post))
      { %>
    <%=Html.ValidationSummary() %>
    <%=Html.Hidden("close",ViewData["close"]) %>
    <%=Html.Hidden("PurchaseStatus",ViewData["PurchaseStatus"]) %><%//2016/06/20 arc yano #3585 %>
    <%=Html.Hidden("RequestFlag",ViewData["RequestFlag"]) %><%//Add 2017/08/02 arc yano #3783 %>
    <%=Html.Hidden("addLine", 0) %><%//Add 2017/11/07 arc yano #3807 %>
  
  <%=Html.Hidden("GenuineSupplierCode", ViewData["GenuineSupplierCode"]) %><%//Add 2018/01/15 arc yano #3833 %>
  <%=Html.Hidden("GenuineSupplierName", ViewData["GenuineSupplierName"]) %><%//Add 2018/01/15 arc yano #3833 %>
  <%=Html.Hidden("JLRGenuineSupplierCode", ViewData["JLRGenuineSupplierCode"]) %><%//Add 2018/06/01 arc yano #3894 %>
  <%=Html.Hidden("JLRGenuineSupplierName", ViewData["JLRGenuineSupplierName"]) %><%//Add 2018/06/01 arc yano #3894 %>
    <br />
    
    <table class="input-form">
        <% if (ViewData["PurchaseStatus"].ToString().Equals("001")){    //入荷ステータス = 「未入荷」%>
            <tr>
                <th rowspan="2">部門 *</th><%//2016/02/01 arc yano #3421 %>
                <!--Mod 2016/04/21 arc nakayama #3493_部品入荷入力　仕掛ロケーションを持たない部門の選択時の不具合-->
                <td><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "alphanumeric", size = "10", maxlength = "3",onchange = "GetNameFromCodeForPartsInventory('DepartmentCode','DepartmentName','Department', 'false');" })%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('DepartmentCode','DepartmentName','/Department/CriteriaDialog?CloseMonthFlag=2')" /></td>
                <th  style="width:100px" rowspan="2">担当者 *</th><%//2016/02/01 arc yano #3421 %>
                <td style="width:200px"><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "alphanumeric", maxlength = "50",onblur="GetNameFromCode('EmployeeCode','EmployeeName','Employee')" })%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('EmployeeCode','EmployeeName','/Employee/CriteriaDialog')" /></td>
            </tr>
            <tr>
                <td><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
                <td><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
            </tr>
            <tr>
                <th>入荷伝票区分 *</th><%//2016/02/01 arc yano #3421 %>
                <td><%=Html.DropDownList("PurchaseType",(IEnumerable<SelectListItem>)ViewData["PurchaseTypeList"]) %></td>
                <th>入荷日 *</th><%//2016/02/01 arc yano #3421 %>
                <td><%=Html.TextBox("PurchaseDate",ViewData["PurchaseDate"],new {@class="alphanumeric",size="10",maxlength="10",onchange ="return chkDate3(document.getElementById('PurchaseDate').value, 'PurchaseDate')"})%></td>
            </tr>
        <%}else{ //入荷ステータス = 「入荷済」%>
            <%// Mod 2018/11/12 yano #3949  %>  
             <tr>
                <th style="white-space:nowrap">部門 *</th><%//2016/02/01 arc yano #3421 %>
                <%//Mod 2016/04/21 arc nakayama #3493_部品入荷入力　仕掛ロケーションを持たない部門の選択時の不具合 %>
                <td style="white-space:nowrap"><%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], new { @class = "readonly", @readonly = "readonly", size = "10", maxlength = "3"})%>&nbsp;<img alt="部門検索" style="cursor:pointer" src="/Content/Images/search.jpg" /><span id="DepartmentName"><%=ViewData["DepartmentName"] %></span></td>
             </tr>
             <tr>
                <th style="white-space:nowrap">担当者 *</th>
                <td style="white-space:nowrap"><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], new { @class = "readonly", @readonly = "readonly", maxlength = "50"})%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" /><span id="EmployeeName"><%=ViewData["EmployeeName"] %></span></td>
             </tr>
             <tr>
                 <th style="white-space:nowrap">入荷伝票区分 *</th><%//2016/02/01 arc yano #3421 %>
                 <td style="white-space:nowrap"><%=Html.DropDownList("PurchaseType", (IEnumerable<SelectListItem>)ViewData["PurchaseTypeList"], new { @class = "readonly", @disabled = "disabled" })%><%=Html.Hidden("hdPurchaseType",ViewData["PurchaseType"]) %></td>
             </tr>
             <tr>
                 <th style="white-space:nowrap">入荷日 *</th><%//2016/02/01 arc yano #3421 %>
                 <td style="white-space:nowrap"><%=Html.TextBox("PurchaseDate",ViewData["PurchaseDate"],new {@class="readonly", @readonly = "readonly", size="10",maxlength="10"})%></td>
             </tr>

             <%//棚卸ステータスが完了の場合は入荷キャンセル情報を表示 %>
             <%if(ViewData["InventoryStatus"] != null &&  ViewData["InventoryStatus"].Equals("002")){ %>    
             <tr>
                 <th style="white-space:nowrap">キャンセル担当者 *</th>
                 <td style="white-space:nowrap"><%=Html.TextBox("CancelEmployeeCode", ViewData["CancelEmployeeCode"], new { @class = "alphanumeric", maxlength = "50",onblur="GetNameFromCode('CancelEmployeeCode','CancelEmployeeName','Employee')" })%>&nbsp;<img alt="担当者コード" style="cursor:pointer" src="/Content/Images/search.jpg" onclick="openSearchDialog('CancelEmployeeCode','CancelEmployeeName','/Employee/CriteriaDialog')" /><span id="CancelEmployeeName"><%=ViewData["CancelEmployeeName"] %></span></td>
             </tr>
             <tr>
                 <th style="white-space:nowrap">キャンセル日 *</th><%//2016/02/01 arc yano #3421 %>
                 <td style="white-space:nowrap"><%=Html.TextBox("CancelPurchaseDate",ViewData["CancelPurchaseDate"],new {@class="alphanumeric",size="10",maxlength="10",onchange ="return chkDate3(document.getElementById('CancelPurchaseDate').value, 'CancelPurchaseDate')"})%></td>
             </tr>

            <%} %>
        <%} %>
    </table>
    <br />
    ※発注した部品と異なる部品（バージョンアップ部品等）を入荷する場合は、発注部品番号の左にチェックを入れて、発注した部品番号を入力して下さい。<br />
    <br />
    <div style="height: 200px">
    <% if (ViewData["PurchaseStatus"].ToString().Equals("001")){    //入荷ステータス = 「未入荷」%><% //Add 2017/11/07 arc yano #3807 %>
        <button type="button" style="width:60px;height:20px; vertical-align:middle;" onclick="document.forms[0].addLine.value = '10'; document.forms[0].action = '/PartsPurchase/AddLine';formSubmit();">10行追加</button>
        <br />
        <br />
    <%} %>
    <table class="input-form-line">
        <tr>
            <th colspan="20" class="input-form-title " style="text-align:left"><%//Mod 2017/11/21 arc yano colspan 19 -> 20 %>
                更新対象
            </th>
        </tr>
        <tr>
            <th style="width: 20px; height:20px; text-align: center; padding:0px">
            <% if (ViewData["PurchaseStatus"].ToString().Equals("001")){    //入荷ステータス = 「未入荷」%>
                <img alt="行追加" src="/Content/Images/plus.gif" style="cursor: pointer"onclick="document.forms[0].addLine.value = '1'; document.forms[0].action = '/PartsPurchase/AddLine';formSubmit();" /><% //Mod 2017/11/07 arc yano #3807 %>
            <%} %>
            </th>
            <th style="white-space:nowrap">
                インボイス番号
            </th>
            <th style="white-space:nowrap">
                発注伝票番号
            </th>
            <th style="white-space:nowrap">
                入荷部品番号 *
            </th><%//2016/02/01 arc yano #3421 %>
            <th style="white-space:nowrap">
                入荷部品名
            </th>
            <th style="white-space:nowrap" colspan="2"> 
                発注部品番号 (*)
            </th><%//2016/02/01 arc yano #3421 %>
            <th style="white-space:nowrap">
                発注部品名
            </th>
            <th style="white-space:nowrap" colspan="2">
                ロケーション *
            </th><%//2016/02/01 arc yano #3421 %>
            <th style="white-space:nowrap">
                発注残
            </th>
            <th style="white-space:nowrap">
                入荷数
            </th>
            <th style="white-space:nowrap">
                入荷単価 *
            </th><%//2016/02/01 arc yano #3421 %>
            <th style="white-space:nowrap">
                入荷金額
            </th>
            <th style="white-space:nowrap">
                メーカーオーダー番号
            </th>
            <th style="white-space:nowrap"><%//Add 2017/11/06 arc yano #3808 %>
                Webオーダー番号
            </th>
            <th style="white-space:nowrap">
                納品書番号
            </th>
            <th style="white-space:nowrap" colspan="2">
                仕入先 *
            </th><%//2016/02/01 arc yano #3421 %>
            <th style="white-space:nowrap">
                備考
            </th>
        </tr>
            <%if (Model.line != null) {%>  
                <%for (int i = 0; i < Model.line.Count; i++)
                  {
                      string namePrefix = string.Format("Purchase[{0}].", i);
                      string idPrefix = string.Format("Purchase[{0}]_", i);
                %>
                  <tr>
                    <% if (ViewData["PurchaseStatus"].ToString().Equals("001")){    //入荷ステータス = 「未入荷」%>
                        <td style="width: 22px; text-align: center">
                            <%=Html.Hidden(namePrefix + "LineNumber", i + 1, new { id = idPrefix + "LineNumber"})%>
                            <%=Html.Hidden(namePrefix + "PurchaseNumber", Model.line[i].PurchaseNumber, new { id = idPrefix + "PurchaseNumber"})%><!--入荷伝票番号-->
                            <%=Html.Hidden(namePrefix + "SlipNumber", Model.line[i].SlipNumber, new { id = idPrefix + "SlipNumber"})%><!--受注伝票番号-->
                            <img alt="行削除" src="/Content/Images/minus.gif" style="cursor: pointer" onclick="document.forms[0].action = '/PartsPurchase/DelLine/<%=i %>';formSubmit()" />
                        </td>
                        <!--インボイス番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "InvoiceNo", Model.line[i].InvoiceNo, new { id = idPrefix + "InvoiceNo", @class = "alphanumeric", style="width:120px", maxlength = "50"})%>
                        </td>
                        <!--発注番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PurchaseOrderNumber", Model.line[i].PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber", @class = "alphanumeric", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--入荷部品番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PartsNumber", Model.line[i].PartsNumber, new { id = idPrefix + "PartsNumber", @class = "alphanumeric", style="width:120px", maxlength = "25",onblur="GetMasterDetailFromCode('Purchase["+i+"]_PartsNumber',['Purchase["+i+"]_PartsNameJp', 'Purchase["+i+"]_SupplierCode', 'Purchase["+i+"]_SupplierName'],'Parts', null, null, null, null, setGenuineSupplier)" })%><%//Mod 2018/06/01 arc yano #3894 %>
                            <%//<%=Html.TextBox(namePrefix + "PartsNumber", Model.line[i].PartsNumber, new { id = idPrefix + "PartsNumber", @class = "alphanumeric", style="width:120px", maxlength = "25",onblur="GetNameFromCode('Purchase["+i+"]_PartsNumber','Purchase["+i+"]_PartsNameJp','Parts', 'false', setGenuineSupplier, '" + idPrefix + "')" })%><%//Mod 2018/01/15 arc yano #3833 %>
                            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "PartsNameJp", "'/Parts/CriteriaDialog'", "0" }); %>
                        </td>
                        <!--入荷部品名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PartsNameJp", Model.line[i].PartsNameJp, new { id = idPrefix + "PartsNameJp", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--代替部品フラグ-->
                        <td style="text-align:center">
                            <%=Html.CheckBox(namePrefix + "ChangeParts", Model.line[i].ChangeParts, new { id = idPrefix + "ChangeParts", onclick = "ChangePartsdisabled("+ i +")" })%>
                            <%=Html.Hidden(namePrefix + "ChangePartsFlag", Model.line[i].ChangePartsFlag, new { id = idPrefix + "ChangePartsFlag"})%>
                        </td>
                        <!--発注部品番号-->
                        <%if (Model.line[i].ChangeParts != null && Model.line[i].ChangeParts)
                          { %>
                            <td style="white-space:nowrap">
                                <%=Html.TextBox(namePrefix + "ChangePartsNumber", Model.line[i].ChangePartsNumber, new { id = idPrefix + "ChangePartsNumber", @class = "alphanumeric", style="width:120px", maxlength = "50",onchange="GetNameFromCode('Purchase["+i+"]_ChangePartsNumber','Purchase["+i+"]_ChangePartsNameJp','Parts')" })%>
                                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ChangePartsNumber", idPrefix + "ChangePartsNameJp", "'/Parts/CriteriaDialog'", "0" }); %>
                            </td>
                        <%}else{ %>
                            <td style="white-space:nowrap">
                                <%=Html.TextBox(namePrefix + "ChangePartsNumber", Model.line[i].ChangePartsNumber, new { id = idPrefix + "ChangePartsNumber", @class = "alphanumeric", @disabled = "disabled", style="width:120px", maxlength = "50",onchange="GetNameFromCode('Purchase["+i+"]_ChangePartsNumber','Purchase["+i+"]_ChangePartsNameJp','Parts')" })%>
                                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "ChangePartsNumber", idPrefix + "ChangePartsNameJp", "'/Parts/CriteriaDialog'", "0" }); %>
                            </td>
                        <%} %>
                        <!--発注部品名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "ChangePartsNameJp", Model.line[i].ChangePartsNameJp, new { id = idPrefix + "ChangePartsNameJp", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--ロケーション-->
                        <%//Mod 2016/05/12 arc yano #3529 部品入荷入力　マスタ未登録のロケーションへの入荷 GetNameFromCodeの引数を false → 'false'に修正%>
                        <%//Mod 2016/04/26 arc yano #3510 部品入荷入力　入荷ロケーションの絞込み GetNameFromCodeの引数追加、ロケーション検索ダイアログのクエリ文字列(LocationType=001)を追加%>
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "LocationCode", Model.line[i].LocationCode, new { id = idPrefix + "LocationCode", @class = "alphanumeric", style="width:86px", maxlength = "12",onchange="GetNameFromCode('Purchase["+i+"]_LocationCode','Purchase["+i+"]_LocationName','Location', 'false', CheckLocation, new Array('Purchase["+i+"]_LocationCode', 'Purchase["+i+"]_LocationName'))" })%>
                            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "LocationCode", idPrefix + "LocationName", "'/Location/CriteriaDialog?LocationType=001'", "0" }); %>
                        </td>
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "LocationName", Model.line[i].LocationName, new { id = idPrefix + "LocationName", @class = "readonly", style = "width:120px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--発注残-->
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "RemainingQuantity", string.Format("{0:F2}", Model.line[i].RemainingQuantity), new { id = idPrefix + "RemainingQuantity", @class = "readonly alphanumeric", @readonly = "readonly" , style = "width:80px;text-align:right", maxlength = "10"})%>
                        </td>
                        <!--入荷数-->
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "PurchaseQuantity", string.Format("{0:F2}", Model.line[i].PurchaseQuantity), new { id = idPrefix + "PurchaseQuantity", @class = "numeric", style = "width:80px;text-align:right", onblur = "CalcPurchaseAmount("+ i +")", maxlength = "10"})%><%//Mod 2022/06/09 yano #4137 %>
                            <%--<%=Html.TextBox(namePrefix + "PurchaseQuantity", string.Format("{0:F2}", Model.line[i].PurchaseQuantity), new { id = idPrefix + "PurchaseQuantity", @class = "alphanumeric", style = "width:80px;text-align:right", onblur = "CalcPurchaseAmount("+ i +")", maxlength = "10"})%>--%>
                        </td>
                        <!--入荷単価-->                
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", Model.line[i].Price), new { id = idPrefix + "PurchasePrice", @class = "money", style = "width:80px;text-align:right", onblur = "CalcPurchaseAmount("+ i +")", maxlength = "10"})%>
                        </td>
                        <!--入荷金額-->
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", Model.line[i].Amount), new { id = idPrefix + "PurchaseAmount", @class = "readonly money", style = "width:80px;text-align:right", @readonly = "readonly" , maxlength = "10"})%>
                        </td>
                        <!--メーカーオーダー番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "MakerOrderNumber", Model.line[i].MakerOrderNumber, new { id = idPrefix + "MakerOrderNumber", @class = "alphanumeric", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--Webオーダー番号--><%//Add 2017/11/06 arc yano #3808 %>
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "WebOrderNumber", Model.line[i].WebOrderNumber, new { id = idPrefix + "WebOrderNumber", @class = "alphanumeric", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--納品書番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "ReceiptNumber", Model.line[i].ReceiptNumber, new { id = idPrefix + "", @class = "alphanumeric", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--仕入先-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "SupplierCode", Model.line[i].SupplierCode, new { id = idPrefix + "SupplierCode", @class = "alphanumeric", style="width:100px", maxlength = "10",onchange="GetNameFromCode('Purchase["+i+"]_SupplierCode','Purchase["+i+"]_SupplierName','Supplier')" })%>
                            <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierCode", idPrefix + "SupplierName", "'/Supplier/CriteriaDialog'", "0" }); %>
                        </td>
                        <!--仕入先名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "SupplierName", Model.line[i].SupplierName, new { id = idPrefix + "SupplierName", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "80"})%>
                        </td>
                        <!--備考-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "Memo", Model.line[i].Memo, new { id = idPrefix + "Memo", style="width:120px", maxlength = "100" })%>
                        </td>
                    <%  }else{ //入荷ステータス = 「入荷済」%>
                        <td style="width: 22px; text-align: center">
                             <%//Add 2017/08/02 arc yano #3783 %>
                             <%=Html.Hidden(namePrefix + "PurchaseNumber", Model.line[i].PurchaseNumber, new { id = idPrefix + "PurchaseNumber"})%><!--入荷伝票番号-->
                             <%=Html.Hidden(namePrefix + "SlipNumber", Model.line[i].SlipNumber, new { id = idPrefix + "SlipNumber"})%><!--受注伝票番号-->
                        </td>
                        <!--インボイス番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "InvoiceNo", Model.line[i].InvoiceNo, new { id = idPrefix + "InvoiceNo", @class = "alphanumeric", style="width:120px", maxlength = "50"})%><%//Mod 2018/11/12 yano #3949%>
                        </td>
                        <!--発注番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PurchaseOrderNumber", Model.line[i].PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber", @class = "readonly", @readonly = "readonly", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--入荷部品番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PartsNumber", Model.line[i].PartsNumber, new { id = idPrefix + "PartsNumber", @class = "readonly", @readonly = "readonly", style="width:120px", maxlength = "25" })%><img alt="入荷部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" />
                        </td>
                        <!--入荷部品名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "PartsNameJp", Model.line[i].PartsNameJp, new { id = idPrefix + "PartsNameJp", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--代替部品フラグ-->
                        <td style="text-align:center">
                            <%=Html.CheckBox(namePrefix + "ChangeParts", Model.line[i].ChangeParts, new { id = idPrefix + "ChangeParts", @class = "readonly", @disabled = "disabled"})%>
                        </td>
                        <!--発注部品番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "ChangePartsNumber", Model.line[i].ChangePartsNumber, new { id = idPrefix + "ChangePartsNumber", @class = "readonly", style="width:120px", maxlength = "50",onchange="GetNameFromCode('Purchase["+i+"]_ChangePartsNumber','Purchase["+i+"]_ChangePartsNameJp','Parts')" })%><img alt="発注部品検索" style="cursor:pointer" src="/Content/Images/search.jpg" />
                        </td>
                     
                        <!--発注部品名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "ChangePartsNameJp", Model.line[i].ChangePartsNameJp, new { id = idPrefix + "ChangePartsNameJp", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--ロケーション-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "LocationCode", Model.line[i].LocationCode, new { id = idPrefix + "LocationCode", @class = "readonly", @readonly = "readonly", style="width:86px", maxlength = "12"})%><img alt="ロケーション検索" style="cursor:pointer" src="/Content/Images/search.jpg" />
                        </td>
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "LocationName", Model.line[i].LocationName, new { id = idPrefix + "LocationName", @class = "readonly", style = "width:120px", @readonly = "readonly" , maxlength = "50"})%>
                        </td>
                        <!--発注残-->
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "RemainingQuantity", string.Format("{0:F2}", Model.line[i].RemainingQuantity), new { id = idPrefix + "RemainingQuantity", @class = "readonly alphanumeric", @readonly = "readonly" , style = "width:80px;text-align:right", maxlength = "10"})%>
                        </td>
                        <!--入荷数-->
                        <td style="white-space:nowrap;">
                        <%if(ViewData["InventoryStatus"] != null &&  ViewData["InventoryStatus"].Equals("002")){//Add 2017/08/02 arc yano #3783%>
                            <%=Html.TextBox(namePrefix + "PurchaseQuantity", string.Format("{0:F2}", Model.line[i].PurchaseQuantity), new { id = idPrefix + "PurchaseQuantity", style = "width:80px;text-align:right" , maxlength = "10"})%>
                        <%}else{ %>
                            <%=Html.TextBox(namePrefix + "PurchaseQuantity", string.Format("{0:F2}", Model.line[i].PurchaseQuantity), new { id = idPrefix + "PurchaseQuantity", @class = "readonly", @readonly = "readonly", style = "width:80px;text-align:right" , maxlength = "10"})%>
                        <%} %>
                        </td>
                        <!--入荷単価-->                
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", Model.line[i].Price), new { id = idPrefix + "PurchasePrice",@class = "readonly money", @readonly = "readonly", style = "width:80px;text-align:right", maxlength = "10"})%>
                        </td>
                        <!--入荷金額-->
                        <td style="white-space:nowrap;">
                            <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}",Model.line[i].Amount), new { id = idPrefix + "PurchaseAmount", @class = "readonly money", style = "width:80px;text-align:right", @readonly = "readonly" , maxlength = "10"})%>
                        </td>
                        <!--メーカーオーダー番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "MakerOrderNumber", Model.line[i].MakerOrderNumber, new { id = idPrefix + "MakerOrderNumber", @class = "alphanumeric", style="width:120px", maxlength = "50" })%><%//Mod 2018/11/12 yano #3949%>
                        </td>
                        <!--Webオーダー番号--><%//Add 2017/11/06 arc yano #3808 %>
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "WebOrderNumber", Model.line[i].WebOrderNumber, new { id = idPrefix + "WebOrderNumber", @class = "readonly", @readonly = "readonly", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--納品書番号-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "ReceiptNumber", Model.line[i].ReceiptNumber, new { id = idPrefix + "", @class = "readonly", @readonly = "readonly", style="width:120px", maxlength = "50" })%>
                        </td>
                        <!--仕入先-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "SupplierCode", Model.line[i].SupplierCode, new { id = idPrefix + "SupplierCode",  @class = "readonly", @readonly = "readonly",style="width:100px", maxlength = "10" })%><img alt="仕入先検索" style="cursor:pointer" src="/Content/Images/search.jpg" />
                        </td>
                        <!--仕入先名-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "SupplierName", Model.line[i].SupplierName, new { id = idPrefix + "SupplierName", @class = "readonly", style = "width:330px", @readonly = "readonly" , maxlength = "80"})%>
                        </td>
                        <!--備考-->
                        <td style="white-space:nowrap">
                            <%=Html.TextBox(namePrefix + "Memo", Model.line[i].Memo, new { id = idPrefix + "Memo", style="width:120px", maxlength = "100"})%><%//Mod 2018/11/12 yano #3949%>
                        </td>
                    <%  } %>
                 </tr>
                <%  } %>
                <%=Html.Hidden("PurchaseLineCount", Model.line.Count)%>
        <%} %>
        </table>
    </div>
    <%} %>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    $(document).ready(function (e) {
        if (document.getElementById('PurchaseStatus').value != '002') {  //Mod 2016/06/20 arc yano #3585
        CheckRemainingQuantity(<%=Model.line.Count%>);
        }
    });
</script>
</asp:Content>
