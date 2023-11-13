<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master"
    Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.PartsPurchaseOrder>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品発注入力(純正品)
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    /*
 * ----------------------------------------------------------------------------------------------
 * 部品発注入力画面
 * 更新履歴：
 *   2018/07/27 yano.hiroki #3923 部門マスタにデフォルト仕入先を設定
 *   2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
 *   2017/11/07 arc yano #3806 部品発注入力　１０行追加ボタンの追加 
 *   2017/02/14 arc yano #3641 金額欄のカンマ表示対応
 *                             ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
 *                             ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) レイアウトの変更
 * ----------------------------------------------------------------------------------------------
 */
%>

<%
    string mode = "1";   //メニューボタンモード
    bool editFlag = true;

    //コントローラの動作指定(編集可)
    object atrTxtDate = new { @class = "alphanumeric", size = "10", maxlength = "10", onchange = "chkDate3(this.value, this.id)" };
    object atrPurchaseOrderDate = new { @class = "alphanumeric", size = "10", maxlength = "10", onchange = "chkDate3(this.value, this.id);ChangePlanDate(" + Model.Count() + ", '1');" };
    object atrTxtDep = new { @class = "alphanumeric", size = "10", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department', 'false', setSupplierPayment, new Array('DepartmentCode', 'SupplierCode', 'SupplierName', 'SupplierPaymentCode', 'SupplierPaymentName', 'SupplierPayment'))" };      //Mod  2018/07/27 yano.hiroki #3923
    //object atrTxtDep = new { @class = "alphanumeric", size = "10", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department', 'false', getSuppierCodeFromPartsPurchase, new Array('DepartmentCode', 'SupplierCode', 'SupplierName', 'PartsPurchaseOrder', 'SupplierPaymentCode', 'SupplierPaymentName', 'SupplierPayment'))" };      //Mod  2018/06/01 arc yano #3894
    object atrTxtEmpNum = new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" };
    object atrTxtEmpCod = new { @class = "alphanumeric", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" };
    object atrTxtSupCode = new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetSupplierName('SupplierCode', 'SupplierName', 'SupplierPaymentCode', 'SupplierPaymentName')" };
    object atrTxtSupPayCod = new { @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('SupplierPaymentCode','SupplierPaymentName','SupplierPayment')" };
    object atrTxtWebOrder = new { @class = "alphanumeric", size = "20", maxlength = "50" };
    
    //発注済の場合は全項目編集不可
    if (ViewData["PurchaseOrderStatus"] != null && 
         (
            ViewData["PurchaseOrderStatus"].Equals("002") ||
            ViewData["PurchaseOrderStatus"].Equals("003") ||
            ViewData["PurchaseOrderStatus"].Equals("004")
         )
       )
    {
        editFlag = false;
        
        mode = "3";        //デフォルト3

        if (ViewData["PurchaseOrderStatus"].Equals("002"))
        {
            mode = "2";        
        }
            
        //コントローラの動作指定(編集不可)
        atrTxtDate = new { @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10" };
        atrTxtDep = new { @class = "readonly", @readonly = "readonly", size = "10", onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department', 'false', getSuppierCodeFromPartsPurchase, new Array('DepartmentCode', 'SupplierCode', 'SupplierName', 'PartsPurchaseOrder', 'SupplierPaymentCode', 'SupplierPaymentName', 'SupplierPayment'))" };      //Mod  2018/06/01 arc yano #3894
        atrTxtEmpNum = new { @class = "readonly", @readonly = "readonly", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" };
        atrTxtEmpCod = new { @class = "readonly", @readonly = "readonly", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" };
        atrTxtSupCode = new { @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10", onblur = "GetSupplierName('SupplierCode', 'SupplierName', 'SupplierPaymentCode', 'SupplierPaymentName')" };
        atrTxtSupPayCod = new { @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10", onblur = "GetNameFromCode('SupplierPaymentCode','SupplierPaymentName','SupplierPayment')" };
        atrTxtWebOrder = new { @class = "readonly", @readonly = "readonly", size = "20", maxlength = "50" };

    }
%>
    <%Html.RenderPartial("PurchaseOrderMenuControl", mode); %> <%//Mod 2015/11/09 arc yano #3291 %>
    <%using (Html.BeginForm("Entry", "PartsPurchaseOrder", FormMethod.Post)) { %>
    <br />
    <%=Html.ValidationSummary() %>
    <br />
    <%=Html.Hidden("close",ViewData["close"]) %>
    <%=Html.Hidden("OrderFlag", ViewData["OrderFlag"]) %>
    <!--Mod 2016/05/02 arc nakayama #3513_サービス伝票入力から発注取消を行うと、伝票上の発注数が更新されない 発注取消を行ったタイミングでサービス伝票の画面をリフレッシュする-->
    <%=Html.Hidden("CancelFlag", ViewData["CancelFlag"]) %>
    <%//Add 2016/01/26 arc yano #3399 部品発注画面の[オーダー区分]の非活性化(#3397_【大項目】部品仕入機能改善 課題管理表対応) オーダー区分編集フラグ追加 %>
    <%=Html.Hidden("OrderTypeEdit", ViewData["OrderTypeEdit"]) %>
    <%=Html.Hidden("addLine", 0) %><%//Add 2017/11/07 arc yano #3806 %>
    <table class="input-form-slim" style="width: 700px">
        <tr>
            <th colspan ="4">基本情報</th><%//Add 2015/11/09 arc yano #3291%>
        </tr>
        <tr>
            <th style="width: 100px">
                発注伝票番号
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("PurchaseOrderNumber", ViewData["PurchaseOrderNumber"], new { @class = "readonly alphanumeric", @readonly = "readonly" }) %>
            </td>
            <th style="width: 100px">
                受注伝票番号
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("ServiceSlipNumber", ViewData["ServiceSlipNumber"], new { @class = "readonly alphanumeric", @readonly = "readonly" }) %>
            </td>
        </tr>
        <tr>
            <th>
                発注日 (*)
            </th>
            <td>
                <%=Html.TextBox("PurchaseOrderDate", ViewData["PurchaseOrderDate"], atrPurchaseOrderDate)%>
            </td>
            <th>
                発注ステータス<%//Mod 2015/11/09 arc yano #3291%>
            </th>
            <td>
                <%=Html.TextBox("PurchaseOrderStatusName", ViewData["PurchaseOrderStatusName"], new { @class = "readonly", @readonly = "readonly" }) %><%=Html.Hidden("PurchaseOrderStatus", ViewData["PurchaseOrderStatus"])%>
            </td>
        </tr>
        <tr>
            <th rowspan="2">
                部門 *
            </th>
            <td>
                <%=Html.TextBox("DepartmentCode", ViewData["DepartmentCode"], atrTxtDep)%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "DepartmentCode", "DepartmentName", "'/Department/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
            <th rowspan="2">
                担当者 *
            </th>
            <td>
                <%=Html.TextBox("EmployeeNumber", ViewData["EmployeeNumber"], atrTxtEmpNum)%><%=Html.TextBox("EmployeeCode", ViewData["EmployeeCode"], atrTxtEmpCod)%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "EmployeeCode", "EmployeeName", "'/Employee/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
        </tr>
        <tr>
            <td><%=Html.TextBox("DepartmentName", ViewData["DepartmentName"], new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%></td>
            <td><%=Html.TextBox("EmployeeName", ViewData["EmployeeName"], new { @class = "readonly", style = "width:200px", @readonly = "readonly" })%></td>
        </tr>
        <tr>
            <th rowspan="2">
                仕入先 (*)
            </th>
            <td>
                <%=Html.TextBox("SupplierCode", ViewData["SupplierCode"], atrTxtSupCode)%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierCode", "SupplierName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
            <th rowspan="2">
                支払先 (*)
            </th>
            <td>
                <%=Html.TextBox("SupplierPaymentCode", ViewData["SupplierPaymentCode"], atrTxtSupPayCod)%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { "SupplierPaymentCode", "SupplierPaymentName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
        </tr>
        <tr>
            <td style="height: 20px">
                <%=Html.TextBox("SupplierName", ViewData["SupplierName"], new { @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
            <td style="height: 20px">
                <%=Html.TextBox("SupplierPaymentName", ViewData["SupplierPaymentName"], new { @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
        </tr>
        <tr>
            <th>
                WEBオーダー番号
            </th>
            <td>
                <%=Html.TextBox("WebOrderNumber", ViewData["WebOrderNumber"], atrTxtWebOrder)%>
            </td>
             <th>
                オーダー区分
            </th>
            <td>
                <%//Add 2016/01/26 arc yano #3399 部品発注画面の[オーダー区分]の非活性化(#3397_【大項目】部品仕入機能改善 課題管理表対応) オーダー区分編集フラグ追加 %>
                <%if (editFlag == true && ViewData["OrderTypeEdit"].ToString().Equals("1"))
                  {%>
                <%=Html.DropDownList("OrderType", (IEnumerable<SelectListItem>)ViewData["OrderTypeList"])%><%//Mod 2015/11/09 arc yano #3291%>
                <%} %>
                <%else{ %>
                <%=Html.DropDownList("OrderType", (IEnumerable<SelectListItem>)ViewData["OrderTypeList"], new { @disabled = "disabled" })%><%//Mod 2015/11/09 arc yano #3291%>
                <%} %>
                 <%=Html.DropDownList("HdOrderType",(IEnumerable<SelectListItem>)ViewData["OrderTypeList"],  new { style = "visibility:hidden"})%>
            </td>
        </tr>
        <tr>
            <th>
                入荷予定日 (*)
            </th>
            <td>
                <%=Html.TextBox("ArrivalPlanDate", ViewData["ArrivalPlanDate"], atrTxtDate)%>
            </td>
            <th>
                支払予定日 (*)
            </th>
            <td>
                <%=Html.TextBox("PaymentPlanDate", ViewData["PaymentPlanDate"], atrTxtDate) %>
            </td>
        </tr>      
    </table>
    <br/>
    <br/>
     
    <%//Add 2017/11/07 arc yano #3806 %>
    <%if(editFlag == true){ %>
        <button type="button" style="width:60px;height:20px; vertical-align:middle;" onclick="document.forms[0].addLine.value = '10'; document.forms[0].action = '/PartsPurchaseOrder/AddOrder';formSubmit();">10行追加</button>
        <br/>
        <br/>
    <%} %>

    <table class="input-form-slim" style="width: 1000px">
        <tr>
            <th colspan="8">
                発注リスト
            </th>
        </tr>
        <tr>
            <th style="width:15px"><%//行追加 %>
                <%if(editFlag == true){ %>
                <img alt="追加" style="cursor: pointer" src="/Content/Images/plus.gif" onclick="document.forms[0].addLine.value = '1'; document.forms[0].action='/PartsPurchaseOrder/AddOrder';formSubmit()" /><%//Mod 2017/11/07 arc yano #3806 %>
                <%} %>
            </th>
            <th style="white-space:nowrap;" colspan="2">部品 *</th>
            <th style="white-space:nowrap;">数量</th>
            <th style="white-space:nowrap;">原価</th>
            <th style="white-space:nowrap;">定価</th>
            <th style="white-space:nowrap;">金額</th>
            <th style="white-space:nowrap;">備考</th>
        </tr>
        <%if(Model != null && Model.Count() > 0) {%>
        <%for (int i = 0; i < Model.Count(); i++ )
          { 
        %>
        <%    
              PartsPurchaseOrder rec = Model[i];
              string idPrefix = string.Format("line[{0}]_", i);
              string namePrefix = string.Format("line[{0}].", i);

              //GetPartsMasterの引数を文字列化
              string arg1 = "'" + idPrefix + "PartsNumber', " + "'" + idPrefix + "PartsNameJp', " + "'" + idPrefix + "Quantity', " + "'" + idPrefix + "Cost', " +  "'" + idPrefix + "Price', "+ "'" + idPrefix + "Amount'";
              //calcPartsAmountの引数を文字列化
              string arg2 = "'" + idPrefix + "Quantity', " + "'" + idPrefix + "Cost', " + "'" + idPrefix + "Amount'";

              string display = "";
              
              //削除フラグONの場合
              if (!string.IsNullOrEmpty(rec.DelFlag) && rec.DelFlag.Equals("1"))
              {
                  display = "none";
              }      
        %>
        <tr style = "display:<%=display %>">
            <%if(editFlag == true){ //編集可の場合%>
            
            <td style="width:15px; text-align:center"><%//行削除 %>
                <img alt="削除" style="cursor: pointer" src="/Content/Images/minus.gif" onclick="document.forms[0].action='/PartsPurchaseOrder/DelOrder/<%=i %>';formSubmit()" />
                <%=Html.Hidden(namePrefix + "PurchasePartsNumber", rec.PurchaseOrderNumber + "&" + rec.PartsNumber, new { id = idPrefix + "PurchasePartsNumber" })%>
                <%=Html.Hidden(namePrefix + "DelFlag",rec.DelFlag, new { id = idPrefix + "DelFlag" }) %>
            </td>
            <td><%//部品番号 %>
                <%=Html.TextBox(namePrefix + "PartsNumber", rec.PartsNumber, new { id = idPrefix + "PartsNumber", @class = "alphanumeric", size = "25", onblur = "GetPartsMaster("  + arg1 + ")" })%>
                <%=Html.Hidden(namePrefix + "hdPartsNumber", rec.PartsNumber)%><%//隠し項目 %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "PartsNameJp", "'/Parts/CriteriaDialog?GenuineType=001'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td><%//部品名 %>
                <%=Html.TextBox(namePrefix + "PartsNameJp", rec.PartsNameJp != null ? rec.PartsNameJp : (rec.Parts != null ? rec.Parts.PartsNameJp : ""), new { id = idPrefix + "PartsNameJp", @readonly = "readonly", @class = "readonly", style = "width:200px" })%>
            </td>
            <td><%//数量 %>
                <%=Html.TextBox(namePrefix + "Quantity", rec.Quantity, new { id = idPrefix + "Quantity", @class = "numeric", size = "5", maxlength = "11",  onblur = "calcPartsAmount(" + arg2 + ")" })%>
            </td>
            <td><%//原価 %>
                <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", rec.Cost), new { id = idPrefix + "Cost", @class = "money", size = "10",  onblur = "calcPartsAmount(" + arg2 + ")"})%>
            </td>
            <td><%//定価 %>
                <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", rec.Price), new { id = idPrefix + "Price", @class = "readonly money", size = "10", @readonly = "readonly" })%>
            </td> 
            <td><%//金額 %>
                <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", rec.Amount), new { id = idPrefix + "Amount", @class = "readonly money", size = "10", @readonly = "readonly" })%>
            </td>
            <td><%//備考 %>
                <%=Html.TextBox(namePrefix + "Memo", rec.Memo, new { id = idPrefix + "Memo", size = 50, maxlength = 100 })%>
            </td>
            <%}else{ //編集不可の場合%>
            <td style="width:15px; text-align:center"><%//行削除 %>
                <%=Html.Hidden(namePrefix + "PurchasePartsNumber", rec.PurchaseOrderNumber + "&" + rec.PartsNumber, new { id = idPrefix + "PurchasePartsNumber" })%>
                <%=Html.Hidden(namePrefix + "DelFlag",rec.DelFlag, new { id = idPrefix + "DelFlag" }) %>
            </td>
            <td><%//部品番号 %>
                <%=Html.TextBox(namePrefix + "PartsNumber", rec.PartsNumber, new { id = idPrefix + "PartsNumber", @class = "readonly", @readonly = "readonly", size = "25", onchange = "GetPartsMaster("  + arg1 + ")" })%>
                <%=Html.Hidden(namePrefix + "hdPartsNumber", rec.PartsNumber)%><%//隠し項目 %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "PartsNameJp", "'/Parts/CriteriaDialog?GenuineType=001'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td><%//部品名 %>
                <%=Html.TextBox(namePrefix + "PartsNameJp", rec.PartsNameJp != null ? rec.PartsNameJp : (rec.Parts != null ? rec.Parts.PartsNameJp : ""), new { id = idPrefix + "PartsNameJp", @readonly = "readonly", @class = "readonly", style = "width:200px" })%>
            </td>
            <td><%//数量 %>
                <%=Html.TextBox(namePrefix + "Quantity", rec.Quantity, new { id = idPrefix + "Quantity", @class = "readonly numeric", @readonly = "readonly numeric", size = "3", maxlength = "11",  onblur = "calcPartsAmount(" + arg2 + ")" })%>
            </td>
            <td><%//原価 %>
                <%=Html.TextBox(namePrefix + "Cost", string.Format("{0:N0}", rec.Cost), new { id = idPrefix + "Cost", @class = "readonly money", @readonly = "readonly", size = "10",  onblur = "calcPartsAmount(" + arg2 + ")"})%>
            </td>
            <td><%//定価 %>
                <%=Html.TextBox(namePrefix + "Price", string.Format("{0:N0}", rec.Price), new { id = idPrefix + "Price", @class = "readonly money", size = "10", @readonly = "readonly" })%>
            </td> 
            <td><%//金額 %>
                <%=Html.TextBox(namePrefix + "Amount", string.Format("{0:N0}", rec.Amount), new { id = idPrefix + "Amount", @class = "readonly money", size = "10", @readonly = "readonly" })%>
            </td>
            <td><%//備考 %>
                <%=Html.TextBox(namePrefix + "Memo", rec.Memo, new { id = idPrefix + "Memo",  @class = "readonly", @readonly = "readonly", size = 50, maxlength = 100 })%>
            </td>
            <%} %>
        </tr>
        <%} %>
    </table>
    <%} %>
    <%} %>
   <script type="text/javascript">
       if (document.forms[0].OrderFlag.value == '1') { //発注処理直後後の場合
           document.forms[0].OrderFlag.value = '0';      //フラグOFF
           document.forms[0].action = 'Download';      //アクションにExcel出力を設定
           document.forms[0].submit();
           document.forms[0].action = 'Entry';

           //Add 2016/04/20 arc yano #3496 サービス伝票入力　行移動時の発注数の更新の不具合
           if (parent.window.opener.document.forms[0].id == 'SerivceSalesOrderEntry') {
               parent.window.opener.document.forms[0].ActionType.value = 'Reflesh';
               //parent.window.opener.document.forms[0].action = 'Reflesh';
               parent.window.opener.document.forms[0].submit();
           }

       //Mod 2016/05/02 arc nakayama #3513_サービス伝票入力から発注取消を行うと、伝票上の発注数が更新されない 発注取消を行ったタイミングでサービス伝票の画面をリフレッシュする
       } else if (document.forms[0].CancelFlag.value == '1') {//発注取消処理直後後の場合
           if (parent.window.opener.document.forms[0].id == 'SerivceSalesOrderEntry') {
               parent.window.opener.document.forms[0].ActionType.value = 'Reflesh';
               //parent.window.opener.document.forms[0].action = 'Reflesh';
               parent.window.opener.document.forms[0].submit();
           }
       }
    </script>
</asp:Content>
