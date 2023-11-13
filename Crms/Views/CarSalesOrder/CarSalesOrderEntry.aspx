<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.CarSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    車両伝票入力
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%Html.RenderPartial("MenuControl", Model); %>
<br />
<%if (Model.RegistButtonVisible){ //Add 2018/08/07 yano #3911%>
    <span style="color:blue"><b><%=Html.Encode(ViewData["MessageCarRegisted"]) %></b></span>
<%} %>
<%=Html.ValidationSummary() %>
<%//Add 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能  %>
<%if(ViewData["ProcessSessionError"]!=null){ %>
<span style="color:Red;font-weight:bold"><%=ViewData["ProcessSessionError"] %></span>
<br />
<br />
<input type="button" value="強制的にこの伝票のロックを解除する" style="width:300px" onclick="ExceptCommaAll(); document.forms[0].ForceUnLock.value = '1'; document.forms[0].submit();" />
<br />
<br />
<%} %>
<%using (Html.BeginForm("Entry", "CarSalesOrder", FormMethod.Post, new { name = "dummy" })) {%>
<%=Html.Hidden("LastUpdateDate",Model.LastUpdateDate) %><%//Add 2018/07/31 yano.hiroki #3918%>
<div style="width:1080px">
    <span style="color:Blue;font-weight:bold"><%=ViewData["CompleteMessage"] %></span>
    <%if (ViewData["Mode"] != null && (ViewData["Mode"].Equals("1") || ViewData["Mode"].Equals("2")) || (Model.ModificationControl && Model.ModificationControlCommit))
      { %>
    <%=Html.Hidden("Mode",ViewData["Mode"]) %> 

    <br />
    <br />
    <table class="input-form-slim">
        <tr>
            <th style="width:80px">
                理由 *
            </th>
            <td>
                 <%if(Model.ReasonEnabled){ //Mod 2017/11/10 arc yano #3787%>
                    <%=Html.TextArea("Reason", Model.Reason, new { style = "width:550px;height:50px" })%>
                <% }else{%>
                    <%=Html.TextArea("Reason", Model.Reason, new { style = "width:550px;height:50px" ,@readonly = "readonly", @class = "readonly",})%>
                <%} %>
            </td>
        </tr>
    </table>

    <br />
    <%} %>
    <%  //Add 2017/01/21 arc yano #3657 見積書に個人情報を記載するかどうかのチェックボックスを追加%>
    <%if(Model.PInfoChekEnabled != null && Model.PInfoChekEnabled.Equals(true)){%>
       <%=Html.CheckBox(string.Format("DispPersonalInfo"), Model.DispPersonalInfo) %>見積書に請求先の個人情報を記載する
    <%} %>
    
    <table>
        <tr>
            <td>
                <%if(Model.BasicHasErrors){ %>
                    <input type="button" value="▼基本情報" onclick="changeDisplayContent('basic');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
                <%}else{ %>
                    <input type="button" value="▼基本情報" onclick="changeDisplayContent('basic');" />
                <%} %>
                <%if (Model.UsedHasErrors) { %>
                    <input type="button" value="▼下取車" onclick="changeDisplayContent('used');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
                <%} else { %>
                    <input type="button" value="▼下取車" onclick="changeDisplayContent('used');" />
                <%} %>
                <%if(Model.InvoiceHasErrors){ %>
                    <input type="button" value="▼支払方法" onclick="changeDisplayContent('invoice');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
                <%}else{ %>
                    <input type="button" value="▼支払方法" onclick="changeDisplayContent('invoice');" />
                <%} %>
                <%if(Model.LoanHasErrors){ %>
                    <input type="button" value="▼ローン" onclick="changeDisplayContent('loan');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
                <%}else{ %>
                    <input type="button" value="▼ローン" onclick="changeDisplayContent('loan');" />
                <%} %>
                <%//Mod 2018/11/15 yano #3936 任意保険料の項目を削除 %>
              <%--  <%if(Model.VoluntaryInsuranceHasErrors){ %>
                    <input type="button" value="▼任意保険" onclick="changeDisplayContent('VoluntaryInsurance');" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
                <%}else{ %>
                    <input type="button" value="▼任意保険" onclick="changeDisplayContent('VoluntaryInsurance');" />
                <%} %>--%>
            </td>
        </tr>
    </table>
    <%//Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう %>
    <%//Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 %>
    <%//if(!string.IsNullOrWhiteSpace(Model.LastEditMessage)){ %>
    <!--<br />-->
    <!--<span style="color:red"><b><%//=Html.Encode(Model.LastEditMessage)%></b></span>-->
    <%//} %>
    <br />

    <div style="width:1080px;"><%//Add 2021/10/25 yano #4100%>
    <div style="float: left; width:650px;"><%//Mod 2021/10/25 yano #4100 width追加%>
        <%-- 基本情報 --%>
        <%Html.RenderPartial("SlipControl", Model); %>
        <br />
        <div id="basic" 
        <%if (ViewData["displayContents"] != null && ViewData["displayContents"].Equals("invoice")) { %>
            style="float: left; text-align: left;display:none;">
        <%} else { %>
            style="float: left; text-align: left;">
        <%} %>
            <%-- 販売車両情報 --%>
            <%Html.RenderPartial("SalesCarControl", Model); %>
            <br />
            <%-- オプション --%>
            <%Html.RenderPartial("OptionControl", Model); %>
            <br />
            <%-- 諸費用 --%>
            <%Html.RenderPartial("CostControl", Model); %>
        </div> 
            
        <%--下取車--%>   
        <div id="used" style="display: none; float: left;width:650px">
            <%Html.RenderPartial("UsedControl", Model); %>
        </div>
        
        <%--ローン情報--%>
        <div id="loan" style="float: left; display:none; width:600px">
            <%Html.RenderPartial("LoanControl", Model); %>
        </div>
        
        <%--支払方法--%>
        <div id="invoice"
        <%if (ViewData["displayContents"] != null && ViewData["displayContents"].Equals("invoice")) { %>
             style="float: left">
        <%} else { %>
            style="float: left;display:none">
        <%} %>
            <%Html.RenderPartial("PaymentControl", Model); %>
        </div>
        
        <%--任意保険--%>
        <div id="VoluntaryInsurance" style="float:left;display:none; width:600px">
            <%Html.RenderPartial("InsuranceControl", Model); %>
        </div>
    </div>

    <div style="float: right;margin-left:10px; width:420px;"><%//Modd 2021/10/25 yano #4100%>
        <% // Mod 2014/07/11 arc amii chrome対応 項目が右寄せになるよう修正 %>
        <div style="margin:0 auto;padding-left:0px;">
            <% ////Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 住所再確認の表示を追加 %>
            <table style="width:auto; border-collapse:collapse; border-spacing:0px; empty-cells:show;">
                <tr>
                    <td style="width:170px;height:15px"><span id="ReconfirmMessage" style="color:Red;font-weight:bold;"><%=Html.Encode(ViewData["ReconfirmMessage"])%></span></td>
                    <td>
                    <%// Add 2017/05/25 arc nakayama 伝票修正対応  表示する伝票が修正中だった場合メッセージを表示する%>
                    <%if(Model.ModificationControl != null && Model.ModificationControl){ %>
                    <span style="color:red"><b>この伝票は現在修正中です。</b></span>
                    <%} %>
                        <table class="input-form">
                            <tr>
                                <td style="width:30px;height:15px"><%=Model.SlipNumber==null ? "通常" : Model.SlipNumber.Contains("-1") ? "<span style=\"color:red\">赤伝</span>" : Model.SlipNumber.Contains("-2") ? "<span style=\"color:red\">赤</span>黒" : "通常" %></td>
                                <td style="width:200px;height:15px"><%=Model.c_SalesOrderStatus != null ? Model.c_SalesOrderStatus.Name : ""%></td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
        <%--顧客情報--%>
        <%Html.RenderPartial("CustomerControl", Model); %>
        <br />
        <%--合計欄--%>
        <%Html.RenderPartial("TotalControl", Model); %>
        <br />
        <%--登録情報--%>
        <%Html.RenderPartial("RegistControl", Model); %>    
    </div>
    </div>
</div>
<%// Add 2017/05/25 arc nakayama 伝票修正対応 表示する伝票に編集履歴があれば表示する%>
<%if (Model.ModifiedReasonEnabled == null || Model.ModifiedReasonEnabled){ %>
<table class="input-form-slim">
        <br />
        <tr><th class="input-form-title" colspan="3">修正履歴</th></tr>
        <tr><th style = "width:130px">日付</th><th style = "width:100px">修正者</th><th style = "width:750px">理由</th></tr>
    </table>
    <div style="float:left;text-align:left; width:1020px; height: 100px; overflow:scroll">
        <%Html.RenderPartial("ModifiedReasonControl", Model); %>
    </div>
    <br />
<%} %>
<%--Hiddenパラメータ--%>
<%=Html.Hidden("close", ViewData["close"])%>
<%=Html.Hidden("DelLine", "")%>
<%=Html.Hidden("DelPayLine", "")%>
<%=Html.Hidden("PrintReport", "")%>
<%=Html.Hidden("reportName", ViewData["reportName"])%>
<%=Html.Hidden("reportParam", Model.SlipNumber + "," + Model.RevisionNumber)%>
<%=Html.Hidden("Cancel", "")%>
<%=Html.Hidden("ApprovalFlag", Model.ApprovalFlag)%>
<%=Html.Hidden("PreviousStatus", Model.SalesOrderStatus)%>
<%=Html.Hidden("SalesOrderStatus", Model.SalesOrderStatus)%>
<%=Html.Hidden("ApprovalFlag", Model.ApprovalFlag)%>
<%=Html.Hidden("CancelDate", Model.CancelDate)%>
<%=Html.Hidden("requestType","" )%>
<%=Html.Hidden("url",ViewData["url"]) %>
    <% //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 新規追加 %>
<%=Html.Hidden("ReconfirmMessage", ViewData["ReconfirmMessage"] )%>
<%=Html.Hidden("LastEditScreen", Model.LastEditScreen) %>
<%=Html.Hidden("LastEditMessage", Model.LastEditMessage) %>
<%=Html.Hidden("ActionType",Model.ActionType) %>

<%=Html.Hidden("ProcessSessionId", Model.ProcessSessionId) %>   <%//Add 2017/11/10 arc yano #3787 %>
<%=Html.Hidden("ForceUnLock", "0") %>                           <%//Add 2017/11/10 arc yano #3787 %>
<%=Html.Hidden("FromCopy", Model.FromCopy) %>                   <%//Add 2022/05/20 yano #4069 %>

<%=Html.Hidden("SuspendTaxRecv", Model.SuspendTaxRecv) %>       <%//Add 2023/09/28 yano #4183 %>


<%} %>

<%//Add 2017/06/23 arc nakayama #3770_【既存バグ】車両伝票にてローン元金が0円のまま変わらない現象 %>
<script type="text/javascript">

    addOnload(function () { calcTotalOptionAmount(); calcTotalAmount(); }); //Mod 2019/09/04 yano #4011

    // onloadイベントを追加する。 
    function addOnload(func) {
        try {
            window.addEventListener("load", func, false);
        } catch (e) {
            // IE用 
            window.attachEvent("onload", func);
        }
    }

    //Add 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
    window.onbeforeunload = function (e) {
        if (((event.clientX > document.body.clientWidth) && (event.clientY < 0)) || event.altKey) {
            document.forms[0].action = '/CarSalesOrder/UnLock';
            document.forms[0].submit();
        }
    }
</script> 


</asp:Content>