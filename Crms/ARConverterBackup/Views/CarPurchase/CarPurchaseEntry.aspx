<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarPurchase>" %>
<%@ Import Namespace="CrmsDao" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    <% //Mod 2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加 %>
<%if (CommonUtils.DefaultString(ViewData["copy"]).Equals("1"))
  { %>
    車両仕入入力(コピー登録)
  <%} else { %>
    車両仕入入力
  <%} %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "CarPurchase", FormMethod.Post))
      { %>
    <%
          /*-------------------------------------------------------------------------------------------
               *   Mod 2018/02/21 arc yano #3866 車両仕入入力　仕入キャンセルボタンの画像差し替え
             　*   Mod 2017/08/21 arc yano #3791 車両仕入 仕入削除機能の追加
               *   Mod 2017/03/06 arc yano #3640 車両仕入の計算がおかしい 仕入計上時に金額のチェックを行う
               *   Mod 2014/06/20 arc yano 税率変更バグ修正 
               * 「保存」ボタン押下時は、税率変更[setTaxIdByDate()]が処理中かどうかをチェックし、
               *  処理中の場合は処理完了までsubmitしない。
               *
               *  注意：この対応は以下の操作
               *  　　　操作：
               *  　　　　(1)[入庫日]の日付を手入力で変更。
               *  　　　　(2)フォーカスを移動しないで、「保存」ボタンを押下
               *        を行った場合に、  
               *
               *        　① 入庫日のonchangeによる処理(setTaxIdByDate())
               *          ② 保存ボタンのonclickによる処理(formsubmit())
               *　　　　  が同時に動作しないようにするためのものであるが、
               *　　　　  同じ操作を行っても、①のみ動作する場合がある。
               *　　　　  その場合は、再度「保存」ボタンをクリックする必要がある。
               -------------------------------------------------------------------------------------------*/      
    
     %>
<table class="command">
    <tr>
        <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
        <% //Edit 2014/06/20 arc yano 税率変更バグ修正 %>
        <%// Add 2015/09/17 arc nakayama #3260 仕入入力で入庫連絡ボタン押すと編集可能になる %>
        <%if ((ViewData["ClosedMonth"] != null && ViewData["ClosedMonth"].Equals("1")) || CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("003"))
          { %>
            <td onclick="openModalDialog('/Report/Print?reportName=CarArrival&reportParam=<%=Model.CarPurchaseId %>');"><img src="/Content/Images/pdf.png" alt="入庫連絡票" class="command_btn" />&nbsp;入庫連絡票</td>
        <%}else{%>
            <td onclick="document.forms[0].action.value = 'save';chkSubmit('CarPurchase');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="document.forms[0].PrintReport.value='CarArrival';document.getElementById('reportParam').value='<%=Model.CarPurchaseId %>';chkSubmit('CarPurchase');"><img src="/Content/Images/pdf.png" alt="入庫連絡票" class="command_btn" />&nbsp;入庫連絡票</td>
        <%} %>
        <%if (CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("001")) { %>
            <td onclick="document.forms[0].action.value = 'saveStock';chkSubmit('CarPurchase');"><img src="/Content/Images/build.png" alt="仕入計上" class="command_btn" />&nbsp;仕入計上</td>
            <td onclick="document.forms[0].action.value = 'DeleteStock';chkSubmit('CarPurchase');"><img src="/Content/Images/cancel.png" alt="仕入削除" class="command_btn" />&nbsp;仕入削除</td><%//Add 2017/08/21 arc yano #3791 %>
        <%}else if(CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002")){%>
            <%if(ViewData["CancelFlag"] != null && ViewData["CancelFlag"].ToString().Equals("1")){ %>
                <!--ボタンを出さない-->
            <%}else{%>
                <td onclick="document.forms[0].action.value = 'CancelStock';chkSubmit('CarPurchase');"><img src="/Content/Images/cancel.png" alt="仕入キャンセル" class="command_btn" />&nbsp;仕入キャンセル</td><%//Mod 2018/02/21 arc yano #3866 画像差し替え %>
            <%} %>
        <%}else{ %>
            <% if (ViewData["PurchaseStatus"] == null || string.IsNullOrWhiteSpace(ViewData["PurchaseStatus"].ToString())) //Mod 2017/11/15 arc yano #3826
               { %>
                <td onclick="document.forms[0].action.value = 'saveStock';chkSubmit('CarPurchase');"><img src="/Content/Images/build.png" alt="仕入計上" class="command_btn" />&nbsp;仕入計上</td>
            <%} %>
        <%} %>
        <%if (Model.CarAppraisalId != null )
          { %>       
             <%if (ViewData["ClosedMonth"] != null && ViewData["ClosedMonth"].ToString().Equals("1") || CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("003")) //2017/11/15 arc yano #3826
              {%>
                <td onclick="openModalDialog('/Report/Print?reportName=CarPurchaseAgreement&reportParam=<%=Model.CarAppraisalId %>');"><img src="/Content/Images/pdf.png" alt="車輌買取契約書" class="command_btn" />&nbsp;車輌買取契約書</td>
            <%}else{ %>
                <td onclick="document.forms[0].PrintReport.value='CarPurchaseAgreement';document.getElementById('reportParam').value='<%=Model.CarAppraisalId %>';chkSubmit('CarPurchase');"><img src="/Content/Images/pdf.png" alt="車輌買取契約書" class="command_btn" />&nbsp;車輌買取契約書</td>

            <%} %>

        <%} %>
    </tr>
</table>
<br />
<%=Html.Hidden("update", ViewData["update"])%>
<%=Html.Hidden("close", ViewData["close"]) %>
<%=Html.Hidden("action", "") %>
<%=Html.Hidden("copy", ViewData["copy"])%>
<%=Html.Hidden("CarPurchaseId", Model.CarPurchaseId)%>
<%=Html.Hidden("CarAppraisalId",Model.CarAppraisalId) %>
<%=Html.Hidden("reportName", ViewData["reportName"])%>
<%=Html.Hidden("reportParam", ViewData["reportParam"])%>
<%=Html.Hidden("PrintReport", "")%>
<%=Html.Hidden("LastEditScreen", Model.LastEditScreen)%>
<%=Html.Hidden("LastEditMessage", Model.LastEditMessage)%>
<%=Html.Hidden("CancelFlag", ViewData["CancelFlag"]) %>
<%=Html.Hidden("Vin", Model.Vin) %><%//2018/07/31 yano.hiroki #3919 %>
<% Model.SalesCar.Prefix = "salesCar."; %>
<div id="input-form">
    <%=Html.ValidationSummary()%>
    <%if(ViewData["ErrorSalesCar"]!=null){ %>
        <%Html.RenderPartial("VinErrorControl", ViewData["ErrorSalesCar"]); %>
        <%=Html.CheckBox("RegetVin",Model.RegetVin) %>この車台番号で管理番号を再取得する
    <br />
    <br />
    <%} %>
    <%//Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう %>
    <%//Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 %>
	 <%//Add 2017/01/16 #3640 初期表示時に仕入金額と各項目の合計が一致しない場合はエラーメッセージを表示する %>
    <%if (!string.IsNullOrWhiteSpace(Model.CalcResultMessage) && (ViewData["ClosedMonth"] == null || !ViewData["ClosedMonth"].Equals("1"))){ %>
    <br />
    <span style="color:red"><b><%=Html.Encode(Model.CalcResultMessage)%></b></span>
    <%} %>
    <br />
    <!--Mod 20170808_#3782_車両仕入_キャンセル機能追加/01_ALTER_TABLE_CarPurchase.sql-->
    <%if ((ViewData["ClosedMonth"] != null && ViewData["ClosedMonth"].Equals("1")) || (ViewData["CancelFlag"] != null && ViewData["CancelFlag"].ToString().Equals("1")))
      { %>
        <%Html.RenderPartial("CarPurchaseAmountDisplay", Model); %>
    <%}else{ %>
        <%Html.RenderPartial("CarPurchaseAmountEntry", Model); %>
    <%} %>
    <br />
<%if ((CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002")) || (ViewData["CancelFlag"] != null && ViewData["CancelFlag"].ToString().Equals("1")))
  { %>
    <%Html.RenderPartial("CarBasicInformationDisplay", Model.SalesCar); %>
    <br />
    <%Html.RenderPartial("CarInspectionDisplay", Model.SalesCar); %>
    <br />
    <%Html.RenderPartial("CarDetailInformationDisplay", Model.SalesCar); %>
<%} else { %>
    <%Html.RenderPartial("CarBasicInformationEntry", Model.SalesCar); %>
    <br />
    <%Html.RenderPartial("CarInspectionEntry", Model.SalesCar); %>
    <br />
    <%Html.RenderPartial("CarDetailInformationEntry", Model.SalesCar); %>
<%} %>
</div>
<%} %>
<br />
</asp:Content>
