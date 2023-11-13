<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/PopUpPage.Master" Inherits="System.Web.Mvc.ViewPage<CrmsDao.ServiceSalesHeader>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	サービス伝票入力
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%using (Html.BeginForm("Entry", "ServiceSalesOrder", FormMethod.Post, new { id = "SerivceSalesOrderEntry"})) //Mod 2016/04/21 arc yano #3496
      { %>
<%Html.RenderPartial("MenuControl", Model); %>
<!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる-->
<div id="UpdateMsg" style="display:none">
<img id="IndicatorImage" src="/Content/Images/indicator.gif" alt="更新中" style="display:block" width="30" height="30" />
</div>
<br />
<%=Html.ValidationSummary() %>
<%//Add 2017/10/19 arc yano #3803 %>
<span style="color:blue"><b><%=ViewData["UnregisteredPartsList"] %></b></span>

<%if(ViewData["ProcessSessionError"]!=null){ %>
<span style="color:Red;font-weight:bold"><%=ViewData["ProcessSessionError"] %></span>
<br />
<br />
<%  //Edit 2014/06/16 arc yano 高速化対応  %>
<%// Mod 2017/05/01 arc nakayama #3759_サービス伝票_伝票のロック解除処理でカンマ除去漏れ %>
<input type="button" value="強制的にこの伝票のロックを解除する" style="width:300px" onclick="ExceptCommaAll();document.forms[0].ForceUnLock.value = '1'; formList(); document.getElementsByName('KeepsCarFlag')[1].value = document.getElementsByName('KeepsCarFlag')[0].checked; document.forms[0].submit();" />
<br />
<br />
<%} %>
<%if( Model.ModificationControl && Model.ModificationControlCommit == false){ %>
<%//Mod 2015/05/20 arc yano 仮締中データ編集権限追加 文言の修正 %>
<span style="color:red"><b>月次締済みのため、伝票修正は行えません。赤黒処理を行ってください。<br /></b></span>
<%} %>
<span style="color:Blue;font-weight:bold"><%=ViewData["CompleteMessage"] %></span>
<% //Mod 2015/03/17 arc nakayama 修正中の場合も理由を表示する　経理締めが仮締め以上の時は表示しない（修正させないため）%>
<%if (ViewData["Mode"] != null && (ViewData["Mode"].Equals("1") || ViewData["Mode"].Equals("2")) || (Model.ModificationControl && Model.ModificationControlCommit) )
  { %>
<%=Html.Hidden("Mode",ViewData["Mode"]) %>
<table class="input-form-slim">
    <tr>
        <th style="width:80px">
            理由 *
        </th>
        <td>
            <%=Html.TextArea("Reason", Model.Reason, new { style = "width:550px;height:50px" })%>
        </td>
    </tr>
</table>

<br />
<%} %>
<div style="width:1010px">
    <%  //Add 2017/01/21 arc yano #3657 見積書に個人情報を記載するかどうかのチェックボックスを追加%>
    <%if(Model.PInfoChekEnabled != null && Model.PInfoChekEnabled.Equals(true)){%>
        <%=Html.CheckBox(string.Format("DispPersonalInfo"), Model.DispPersonalInfo) %>見積書に請求先の個人情報を記載する
    <%} %>

    <%//2021/03/22 yano #4078 %>
     <%if (Model.ClaimReportChecked)
    {%>
        <%=Html.CheckBox(string.Format("ClaimReportOutPut"), Model.ClaimReportOutPut) %>請求先に関わらず、納品請求書を出力する
    <%} %>
    
    
    <div style="width:1130px;"><%//Add 2021/10/25 yano #4100%>
    
        <div style="float:left;text-align:left;width:590px;"><%//Mod 2021/10/25 yano #4100%>
            <%Html.RenderPartial("SlipControl", Model); %>
            <br />
        </div>
    
        <div style="float:left;margin-left:10px;text-align:left;width:480px"><%//Mod 2021/10/25 yano #4100%>
            <% // Mod 2014/07/18 arc amii chrome対応 項目が右寄せになるよう修正 %>
            <div style="margin:0 auto;padding-left:0px;">
                <% ////Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 住所再確認の表示を追加 %>
                <table style="width:auto; border-collapse:collapse; border-spacing:0px; empty-cells:show;">
                    <tr>
                        <td style="width:165px;height:15px"><span id="ReconfirmMessage" style="color:Red;font-weight:bold;"><%=Html.Encode(ViewData["ReconfirmMessage"])%></span></td>
                        <td>
                        <%// Add 2015/03/23 arc nakayama 伝票修正対応  表示する伝票が修正中だった場合メッセージを表示する%>
                        <%if(Model.ModificationControl != null && Model.ModificationControl){ %>
                        <span style="color:red"><b>この伝票は現在修正中です。</b></span>
                        <%} %>
                            <table class="input-form">
                                <tr>
                                    <td style="width:30px;height:15px"><%=Model.SlipNumber==null ? "通常" : Model.SlipNumber.Contains("-1") ? "<span style=\"color:red\">赤伝</span>" : Model.SlipNumber.Contains("-2") ? "<span style=\"color:red\">赤</span>黒" : "通常" %></td>
                                    <td style="width:200px;height:15px"><%=Model.c_ServiceOrderStatus!=null ? Model.c_ServiceOrderStatus.Name : ""%></td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </div>
            <%Html.RenderPartial("CustomerControl",Model); %>
        </div>
    
   </div>
   <div style="float:left;text-align:left;">
    <%Html.RenderPartial("SalesCarControl", Model); %>
    <br />
        <div style="width:1000px">
            <% // Edit 2014/06/11 arc yano 高速化対応 %>
            <%//<input type="button" value="全画面モード" onclick="document.getElementById('EntryMode').value='FullScreen';document.forms[0].action = '/ServiceSalesOrder/ChangeEntryMode';formSubmit();" /> %>
            <input type="button" value="全画面モード" onclick="formList(); document.getElementById('EntryMode').value = 'FullScreen'; document.forms[0].action = '/ServiceSalesOrder/ChangeEntryMode'; formSubmit(); document.getElementsByName('KeepsCarFlag')[1].value = document.getElementsByName('KeepsCarFlag')[0].checked; document.forms[0].submit();" />
            <%if(Model.BasicHasErrors){ %>
                <input type="button" value="作業内容・部品" onclick="changeDisplayContent('detail')" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />　
            <%}else{ %>
                <input type="button" value="作業内容・部品" onclick="changeDisplayContent('detail')" />　
            <%} %>
            <%if(Model.TaxHasErrors){ %>
                <input type="button" value="諸費用" onclick="changeDisplayContent('tax')" style="filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr=#ffebeb, EndColorStr=#ffc5c5);background-repeat:repeat-x;background-color:#FFEBEB;border:1px solid red;" />
            <%}else{ %>
                <input type="button" value="諸費用" onclick="changeDisplayContent('tax')" />
            <%} %>
            <input type="button" value="請求先別明細（保存後に確認）" onclick="changeDisplayContent('claimable')" style="width:200px" />
            <br />
            <br />
        </div>
        <div style="float:left;text-align:left;<%=((ViewData["displayContents"]!=null) && (ViewData["displayContents"].Equals("invoice"))) ? "display:none;" : ""%>" id="detail">
            <!-- Mod 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 発注書の列追加による幅の変更 1849px → 1949px -->
            <!-- Mod 2016/04/07 arc yano #3486 部品仕入機能改善でのwidth調整失敗による不具合 1855px → 1849px -->
            <!-- Mod 2016/02/19 arc yano #3435 レイアウト変更 width: 1815px → 1855px -->
            <!-- Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　レイアウト変更 width: 1670px → 1825px -->
            <!-- Mod 2015/05/22 arc nakayama #3209 サービス伝票の明細画面の横スクロールを無くす タイトル部(head)とデータ部(line)のwidhthの差が17px(縦スクロールの幅分)開くように調整する-->
            <div id="head" style="width:1949px;height:67px;overflow:hidden">
                <%Html.RenderPartial("LineTitleControl", Model); %>
            </div>
            <!-- Mod 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 発注書の列追加による幅の変更 1866px → 1966px -->
            <!-- Mod 2016/04/07 arc yano #3486 部品仕入機能改善でのwidth調整失敗による不具合 1855px → 1866px -->
            <!-- Mod 2016/02/19 arc yano #3435 レイアウト変更 width: 1815px → 1855px -->
            <!-- Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　レイアウト変更 width: 1688px → 1825px -->
            <div id="line" style="width:1966px;height:243px;overflow:auto">
                <%Html.RenderPartial("LineControl", Model); %>
            </div>
        </div>
        <div id="tax" style="text-align:left;display:none;float:left;width:1000px;height:310px">
            <%Html.RenderPartial("CostControl", Model); %>
        </div>
        <div id="invoice" style="float:left;width:1000px;height:310px;text-align:left;<%=(ViewData["displayContents"]!=null) && (!ViewData["displayContents"].Equals("invoice")) ? "display:none;" :""%>">
            <%Html.RenderPartial("PaymentControl", Model); %>
        <br />
        </div> 
        <div id="claimable" style="float:left;width:1000px;height:310px;text-align:left;display:none">
            <%Html.RenderPartial("ClaimableControl", Model.ServiceClaimable); %>
            <br />
        </div>
        <div style="float:left;width:1000px;text-align:left">
            <%Html.RenderPartial("TotalControl", Model); %>
        </div>
    </div>
    <%// Add 2015/03/18 arc nakayama 伝票修正対応 表示する伝票に編集履歴があれば表示する%>
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
</div>
<%=Html.Hidden("RevisionNumber",Model.RevisionNumber) %>
<%=Html.Hidden("SlipNumber",Model.SlipNumber) %>
<%=Html.Hidden("close",ViewData["close"]) %>
<%=Html.Hidden("OrderFlag","") %>
<%=Html.Hidden("ServiceOrderStatus",Model.ServiceOrderStatus) %>
<%=Html.Hidden("ActionType","") %>
<%=Html.Hidden("reportName",ViewData["reportName"]) %>
<%=Html.Hidden("reportParam", Model.SlipNumber + "," + Model.RevisionNumber)%>
<%=Html.Hidden("PrintReport","") %>
<%=Html.Hidden("WorkingStartDate",Model.WorkingStartDate) %>
<%=Html.Hidden("WorkingEndDate",Model.WorkingEndDate) %>
<%=Html.Hidden("CurrentLineNumber","") %>
<%=Html.Hidden("LineCount",Model.ServiceSalesLine.Count) %>
<%=Html.Hidden("EditType","") %>
<%=Html.Hidden("EditLine","") %>
<%=Html.Hidden("AddSize","") %>
<%=Html.Hidden("lineScroll", ViewData["lineScroll"])%>
<%=Html.Hidden("EntryMode",ViewData["EntryMode"]) %>
<%=Html.Hidden("ProcessSessionId", Model.ProcessSessionId) %>
<%=Html.Hidden("ForceUnLock", "0") %>
<%=Html.Hidden("Output", Model.Output) %><%//Add 2017/10/19 arc yano #3803 %>
<%=Html.Hidden("StockCode", Model.StockCode) %><%//Add 2017/10/19 arc yano #3803 %>

        <% //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 新規追加 %>
<%=Html.Hidden("ReconfirmMessage", ViewData["ReconfirmMessage"] )%>
<%} %>
<script type="text/javascript" language="javascript">
   
    //      
    const instance = tippy('#CustomerInfo', {
        content: document.getElementById('CustomerMemo').value.replace(/\n/g, '<br>'),
        placement: 'bottom',
        arrow: false,
        allowHTML: true,
        maxWidth: '99rem',
        theme: 'light'
    });

   
    tippy('.tippy02', {
        //content: "a",
        placement: 'left',
        animation: 'scale',
        duration: 1000,
        arrow: false
    });



    var head_obj = document.getElementById("head");
    var detail_obj = document.getElementById("line");
    detail_obj.onscroll = scroll_test_detail;
    function scroll_test_detail() {
        head_obj.scrollLeft = detail_obj.scrollLeft;
        head_obj.scrollTop = detail_obj.scrollTop;
    }
    detail_obj.scrollTop = document.getElementById('lineScroll').value;

    
    //Add 2017/10/19 arc yano #3803
    var timer = setInterval(function () {

        if (document.readyState === 'complete')
        {

            if (document.forms[0].Output.value == 'True') {
                document.forms[0].Output.value = 'false';
                document.forms[0].action = '/ServiceSalesOrder/Download';      //アクションにExcel出力を設定
                ExceptCommaAll();
                document.forms[0].submit();
                InsertCommaAll();
                document.forms[0].action = '/ServiceSalesOrder/Entry';

            }
            //タイマーを停止
            clearInterval(timer);
        }
    }
        , 1000
    );


    //Add 2022/05/03 #4133
    function dispCustomerInfo() {

        var memo = document.getElementById('CustomerMemo').value.replace(/\n/g, '<br>');

        if (memo != '') {
            document.getElementById('CustomerInfo').style.visibility = 'visible';
        }
        else{
            document.getElementById('CustomerInfo').style.visibility = 'hidden';
        }

        instance[0].setContent(memo);
    }
    
</script>


</asp:Content>
<asp:Content ContentPlaceHolderID="HeaderContent" runat="server">
<script type="text/javascript">
    <%//Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数の更新を行う %>
    <% if (Model.orderUrlList != null){%>
        var cnt = 0;
        <% foreach (string url in Model.orderUrlList) { %>
            openModalAfterRefresh('<%=url%>', null, null, null, null, cnt * 30, cnt * 30);
            cnt++;
        <% }%>
    <% }%>

    window.onbeforeunload = function (e) {
        if (((event.clientX > document.body.clientWidth) && (event.clientY < 0)) || event.altKey) {
            document.forms[0].action = '/ServiceSalesOrder/UnLock';
            document.forms[0].submit();
        }
    }
</script>
</asp:Content>
