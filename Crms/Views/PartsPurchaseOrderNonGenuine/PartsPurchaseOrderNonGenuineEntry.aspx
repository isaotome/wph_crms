<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master"
    Inherits="System.Web.Mvc.ViewPage<List<CrmsDao.PartsPurchaseOrder>>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    部品発注入力(社外品)
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%
    /*
 * -------------------------------------------------------------------------------------------
 * 部品発注入力画面(社外品用)
 * 更新履歴：
 *   2017/11/07 arc yano #3808 部品発注入力　１０行追加ボタンの追加
 *   2017/02/14 arc yano #3641 金額欄のカンマ表示対応
 *                             ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
 *                             ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規作成
 * -------------------------------------------------------------------------------------------
 */
%>
<%
    string mode = "1";   //編集フラグ
    bool editFlag = true;
    //コントローラの動作指定(編集可)
    object atrTxtDate = new { @class = "alphanumeric", size = "10", maxlength = "10", onchange = "chkDate3(this.value, this.id);ChangePlanDate(" + Model.Count() + ", '0');" };
    object atrTxtDep    = new { @class = "alphanumeric", size = "10" , onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" };
    object atrTxtEmpNum = new { @class = "alphanumeric", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" };
    object atrTxtEmpCod = new { @class = "alphanumeric", style="width:80px", maxlength="50",onblur="GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')"};
    
    
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
        mode = "3";

        if (ViewData["PurchaseOrderStatus"].Equals("002"))
        {
            mode = "2";
        }

        //コントローラの動作指定(編集不可)
        atrTxtDate   = new { @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10", onchange = "chkDate3(this.value, this.id)" };
        atrTxtDep    = new { @class = "readonly", @readonly = "readonly", size = "10" , onblur = "GetNameFromCode('DepartmentCode','DepartmentName','Department')" };
        atrTxtEmpNum = new { @class = "readonly", @readonly = "readonly", style = "width:50px", maxlength = "20", onblur = "GetMasterDetailFromCode('EmployeeNumber',new Array('EmployeeCode','EmployeeName'),'Employee')" };
        atrTxtEmpCod = new { @class = "readonly", @readonly = "readonly", style = "width:80px", maxlength = "50", onblur = "GetMasterDetailFromCode('EmployeeCode',new Array('EmployeeNumber','EmployeeName'),'Employee')" };
    }
%>
    <%Html.RenderPartial("PurchaseOrderMenuControl", mode);%>
    <%using (Html.BeginForm("Entry", "PartsPurchaseOrderNonGenuine", FormMethod.Post)) { %>
    <br />
    <%=Html.ValidationSummary() %>
    <br />
    <%=Html.Hidden("close",ViewData["close"]) %>
    <%=Html.Hidden("OrderFlag", ViewData["OrderFlag"]) %>
    <!--Mod 2016/05/02 arc nakayama #3513_サービス伝票入力から発注取消を行うと、伝票上の発注数が更新されない 発注取消を行ったタイミングでサービス伝票の画面をリフレッシュする-->
    <%=Html.Hidden("CancelFlag", ViewData["CancelFlag"]) %>
    <%=Html.Hidden("addLine", 0) %><%//Add 2017/11/07 arc yano #3806 %>
    <table class="input-form-slim" style="width: 700px">
        <tr>
            <th colspan ="4">基本情報</th>
        </tr>
        <tr>
            <th style="width: 100px">
                受注伝票番号
            </th>
            <td style="width: 200px">
                <%=Html.TextBox("ServiceSlipNumber", ViewData["ServiceSlipNumber"], new { @class = "readonly alphanumeric", @readonly = "readonly" }) %>
            </td>
              <th>
                発注日 (*)
            </th>
            <td>
                <%=Html.TextBox("PurchaseOrderDate", ViewData["PurchaseOrderDate"], atrTxtDate)%>
            </td>
        </tr>
        <tr>
            <th>
                発注ステータス
            </th>
            <td>
                <%=Html.TextBox("PurchaseOrderStatusName", ViewData["PurchaseOrderStatusName"], new { @class = "readonly", @readonly = "readonly" }) %><%=Html.Hidden("PurchaseOrderStatus", ViewData["PurchaseOrderStatus"])%>
            </td>
            <th colspan ="2"></th>
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
    </table>
    <br/>
    <br/>

    <%//Add 2017/11/07 arc yano #3806 %>
    <%if(editFlag == true){ %>
        <button type="button" style="width:60px;height:20px; vertical-align:middle;" onclick="document.forms[0].addLine.value = '10'; document.forms[0].action = '/PartsPurchaseOrderNonGenuine/AddOrder';formSubmit();">10行追加</button>
        <br/>
        <br/>
    <%} %>
    <table class="input-form-slim" style="width: 1000px">
        <tr>
            <th colspan="16">
                発注リスト
            </th>
        </tr>
        <tr>
            <th style="width:15px"><%//行追加 %>
                <%if(editFlag == true){ %>
                <img alt="追加" style="cursor: pointer" src="/Content/Images/plus.gif" onclick="document.forms[0].addLine.value = '1'; document.forms[0].action='/PartsPurchaseOrderNonGenuine/AddOrder';formSubmit()" /><%//Mod 2017/11/07 arc yano #3806 %>
                <%} %>
            </th>
            <th style="white-space:nowrap;">発注伝票番号</th>
            <th style="white-space:nowrap;" colspan="2">仕入先 *</th>
            <th style="white-space:nowrap;" colspan="2">支払先 *</th>
            <th style="white-space:nowrap;">Webオーダー番号</th>
            <th style="white-space:nowrap;">入荷予定日 (*)</th>
            <th style="white-space:nowrap;">支払予定日 (*)</th>
            <th style="white-space:nowrap;" colspan="2">部品 *</th>
            <th style="white-space:nowrap;">数量 *</th>
            <th style="white-space:nowrap;">原価</th>
            <th style="white-space:nowrap;">定価</th>
            <th style="white-space:nowrap;">金額</th>
            <th style="white-space:nowrap;">備考</th>
        </tr>

        <%
            if(Model != null && Model.Count() > 0){
        %>
        <%for (int i = 0; i < Model.Count(); i++ )
          { 
        %>
        <%    
              PartsPurchaseOrder rec = Model[i];
              string idPrefix = string.Format("line[{0}]_", i);
              string namePrefix = string.Format("line[{0}].", i);
              //GetPartsMasterの引数を文字列化
              string arg1 = "'" + idPrefix + "PartsNumber', " + "'" + idPrefix + "PartsNameJp', " + "'" + idPrefix + "Quantity', " + "'" + idPrefix + "Cost', " + "'" + idPrefix + "Price', " + "'" + idPrefix + "Amount'";
              //calcPartsAmountの引数を文字列化
              string arg2 = "'" + idPrefix + "Quantity', " + "'" + idPrefix + "Cost', " + "'" + idPrefix + "Amount'";
              //GetSupplierNameの引数を文字列化 
              string arg3 = "'" + idPrefix + "SupplierCode', " + "'" + idPrefix + "SupplierName', " + "'" + idPrefix + "SupplierPaymentCode'," + "'" + idPrefix + "SupplierPaymentName'";

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
                <img alt="削除" style="cursor: pointer" src="/Content/Images/minus.gif" onclick="document.forms[0].action='/PartsPurchaseOrderNonGenuine/DelOrder/<%=i %>';formSubmit()" />
                <%=Html.Hidden(namePrefix + "PurchasePartsNumber", rec.PurchaseOrderNumber + "&" + rec.PartsNumber, new { id = idPrefix + "PurchasePartsNumber" })%>
                <%=Html.Hidden(namePrefix + "DelFlag",rec.DelFlag, new { id = idPrefix + "DelFlag" }) %>
            </td>
            <td style="white-space:nowrap"><%//発注伝票番号 %>
                <%=Html.TextBox(namePrefix + "PurchaseOrderNumber", rec.PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber", @class = "readonly alphanumeric", @readonly = "readonly", style = "width:100px" }) %>
            </td>
            <td style="white-space:nowrap"><%//仕入先コード %>
                <%=Html.TextBox(namePrefix + "SupplierCode", rec.SupplierCode, new { id = idPrefix + "SupplierCode", @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetSupplierName(" + arg3 + ")" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierCode", idPrefix + "SupplierName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
                <td style="white-space:nowrap"><%//仕入先名 %>
                <%=Html.TextBox(namePrefix + "SupplierName", rec.SupplierName != null ? rec.SupplierName : (rec.Supplier != null ? rec.Supplier.SupplierName : ""), new { id = idPrefix + "SupplierName", @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
            <td><%//支払先コード %>
                <%=Html.TextBox(namePrefix + "SupplierPaymentCode", rec.SupplierPaymentCode, new { id = idPrefix + "SupplierPaymentCode", @class = "alphanumeric", size = "10", maxlength = "10", onblur = "GetNameFromCode('" + idPrefix + "SupplierPaymentCode','" + idPrefix + "SupplierPaymentName', 'SupplierPayment')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierPaymentCode", idPrefix + "SupplierPaymentName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td style="height: 20px"><%//支払先名 %>
                <%=Html.TextBox(namePrefix + "SupplierPaymentName", rec.SupplierPaymentName != null ? rec.SupplierPaymentName : (rec.SupplierPayment != null ? rec.SupplierPayment.SupplierPaymentName : ""), new { id = idPrefix + "SupplierPaymentName", @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
            <td><%//Webオーダー番号 %>
                <%=Html.TextBox(namePrefix + "WebOrderNumber", rec.WebOrderNumber, new {  id = idPrefix + "WebOrderNumber", @class = "alphanumeric", size = "20", maxlength = "50"})%>
            </td>
                <td><%//入荷予定日 %>
                <%=Html.TextBox(namePrefix + "ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", rec.ArrivalPlanDate), new { id = idPrefix + "ArrivalPlanDate", @class =  "alphanumeric", size = "10",maxlength="10", onchange = "chkDate3(this.value, this.id)"})%>
                </td>
                <td><%//支払予定日 %>
                <%=Html.TextBox(namePrefix + "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", rec.PaymentPlanDate), new { id = idPrefix + "PaymentPlanDate", @class = "alphanumeric", size="10",maxlength="10" , onchange = "chkDate3(this.value, this.id)"}) %>
            </td>
            <td><%//部品番号 %>
                <%=Html.TextBox(namePrefix + "PartsNumber", rec.PartsNumber, new { id = idPrefix + "PartsNumber", @class = "alphanumeric", size = "25", onblur = "GetPartsMaster("  + arg1 + ")" })%>
                <%=Html.Hidden(namePrefix + "hdPartsNumber", rec.PartsNumber)%><%//隠し項目 %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "PartsNameJp", "'/Parts/CriteriaDialog?GenuineType=002'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td><%//部品名 %>
                <%=Html.TextBox(namePrefix + "PartsNameJp", rec.PartsNameJp != null ? rec.PartsNameJp : (rec.Parts != null ? rec.Parts.PartsNameJp : ""), new { id = idPrefix + "PartsNameJp", @readonly = "readonly", @class = "readonly", style = "width:200px" })%>
            </td>
            <td><%//数量 %>
                <%=Html.TextBox(namePrefix + "Quantity", rec.Quantity, new { id = idPrefix + "Quantity", @class = "numeric", size = "5", maxlength = "11", onblur = "calcPartsAmount(" + arg2 + ")" })%>
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
        <%}else{ %><%//編集不可の場合 %>
            <td style="width:15px; text-align:center"><%//行削除 %>
                <%=Html.Hidden(namePrefix + "PurchasePartsNumber", rec.PurchaseOrderNumber + "&" + rec.PartsNumber, new { id = idPrefix + "PurchasePartsNumber" })%>
                <%=Html.Hidden(namePrefix + "DelFlag",rec.DelFlag, new { id = idPrefix + "DelFlag" }) %>
            </td>
                <td style="white-space:nowrap"><%//発注番号 %>
                <%=Html.TextBox(namePrefix + "PurchaseOrderNumber", rec.PurchaseOrderNumber, new { id = idPrefix + "PurchaseOrderNumber", @class = "readonly alphanumeric", @readonly = "readonly", style = "width:100px" }) %>
            </td><%//仕入先コード %>
                <td style="white-space:nowrap">
                <%=Html.TextBox(namePrefix + "SupplierCode", rec.SupplierCode, new { id = idPrefix + "SupplierCode", @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10", onblur = "GetSupplierName(" + arg3 + ")" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierCode", idPrefix + "SupplierName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td><%//仕入先名 %>
                <td style="white-space:nowrap">
                <%=Html.TextBox(namePrefix + "SupplierName", rec.SupplierName != null ? rec.SupplierName : (rec.Supplier != null ? rec.Supplier.SupplierName : ""), new { id = idPrefix + "SupplierName", @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
                <td><%//支払先コード %>
                <%=Html.TextBox(namePrefix + "SupplierPaymentCode", rec.SupplierPaymentCode, new { id = idPrefix + "SupplierPaymentCode", @class = "readonly", @readonly = "readonly", size = "10", maxlength = "10", onblur = "GetNameFromCode('" + idPrefix + "SupplierPaymentCode','" + idPrefix + "SupplierPaymentName', 'SupplierPayment')" })%>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "SupplierPaymentCode", idPrefix + "SupplierPaymentName", "'/Supplier/CriteriaDialog'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td style="height: 20px"><%//支払先名 %>
                <%=Html.TextBox(namePrefix + "SupplierPaymentName", rec.SupplierPaymentName != null ? rec.SupplierPaymentName : (rec.SupplierPayment != null ? rec.SupplierPayment.SupplierPaymentName : "") , new { id = idPrefix + "SupplierPaymentName", @class = "readonly", style = "width:200px", @readonly = "readonly"})%>
            </td>
            <td><%//Webオーダー番号 %>
                    <%=Html.TextBox(namePrefix + "WebOrderNumber", rec.WebOrderNumber, new {  id = idPrefix + "WebOrderNumber", @class = "readonly", @readonly = "readonly", size = "20", maxlength = "50"})%>
                </td>
                <td><%//入荷予定日 %>
                <%=Html.TextBox(namePrefix + "ArrivalPlanDate", string.Format("{0:yyyy/MM/dd}", rec.ArrivalPlanDate), new { id = idPrefix + "ArrivalPlanDate", @class = "readonly", @readonly = "readonly", size = "10",maxlength="10", onchange = "chkDate3(this.value, this.id)"})%>
                </td>
                <td><%//支払予定日 %>
                <%=Html.TextBox(namePrefix + "PaymentPlanDate", string.Format("{0:yyyy/MM/dd}", rec.PaymentPlanDate), new { id = idPrefix + "PaymentPlanDate", @class = "readonly", @readonly = "readonly", size="10",maxlength="10", onchange = "chkDate3(this.value, this.id)" }) %>
            </td>
            <td><%//部品番号 %>
                <%=Html.TextBox(namePrefix + "PartsNumber", rec.PartsNumber, new { id = idPrefix + "PartsNumber", @class = "readonly", @readonly = "readonly", size = "25", onblur = "GetPartsMaster("  + arg1 + ")" })%>
                <%=Html.Hidden(namePrefix + "hdPartsNumber", rec.PartsNumber)%><%//隠し項目 %>
                <%Html.RenderPartial("SearchButtonControl", new string[] { idPrefix + "PartsNumber", idPrefix + "PartsNameJp", "'/Parts/CriteriaDialog?GenuineType=002'", "0", null, null, editFlag.ToString() });%>
            </td>
            <td><%//部品名 %>
                <%=Html.TextBox(namePrefix + "PartsNameJp", rec.PartsNameJp != null ? rec.PartsNameJp : (rec.Parts != null ? rec.Parts.PartsNameJp : "") , new { id = idPrefix + "PartsNameJp", @readonly = "readonly", @class = "readonly", style = "width:200px" })%>
            </td>
            <td><%//数量 %>
                <%=Html.TextBox(namePrefix + "Quantity", rec.Quantity, new { id = idPrefix + "Quantity", @class = "readonly numeric", @readonly = "readonly", size = "3", maxlength = "11", onblur = "calcPartsAmount(" + arg2 + ")" })%>
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
    <%} %>
    </table><%//Mod Mod 2017/11/07 arc yano #3806 タグの位置変更%>
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
             //Add 2016/04/20 arc yano #3496 サービス伝票入力　行移動時の発注数の更新の不具合
             if (parent.window.opener.document.forms[0].id == 'SerivceSalesOrderEntry') {
                 parent.window.opener.document.forms[0].ActionType.value = 'Reflesh';
                 //parent.window.opener.document.forms[0].action = 'Reflesh';
                 parent.window.opener.document.forms[0].submit();
             }
         }

    </script>
</asp:Content>
