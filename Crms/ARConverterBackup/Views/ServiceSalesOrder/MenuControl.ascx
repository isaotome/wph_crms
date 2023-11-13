<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
    <table class="command2">
    <%  /*------------------------------------------------------------------------------------------------
         *  Mod 2021/08/03 yano #4093【サービス伝票入力】作業終了、納車前ステータスでの「保存」ボタン追加
         *  Mod 2018/03/20 arc yano #3872 サービス伝票入力　作業指示書の表示条件の変更 作業指示書ボタンを受注～納車確認書印刷済まで表示できるように修正
         *  Mod 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
         *  Edit 2014/06/20 arc yano 税率変更バグ修正 
         * 「保存」ボタン押下時は、税率変更[setTaxIdByDate()]が処理中かどうかをチェックし、
         *  処理中の場合は処理完了までsubmitしない。
         *
         *  注意：この対応は以下の操作
         *  　　　操作：
         *  　　　　(1)[納車予定日]の日付を手入力で変更。
         *  　　　　(2)フォーカスを移動しないで、「保存」ボタンを押下
         *        を行った場合に、  
         *
         *        　① 納車予定日のonchangeによる処理(setTaxIdByDate())
         *          ② 保存ボタンのonclickによる処理(formsubmit())
         *　　　　  が同時に動作しないようにするためのものであるが、
         *　　　　  同じ操作を行っても、①のみ動作する場合がある。
         *　　　　  その場合は、再度「保存」ボタンをクリックする必要がある。
         ------------------------------------------------------------------------------------------------*/
     %>
    <%
        // Add 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 警告メッセージの設定
        string message = "住所要確認となっているので、登録された住所をお客様にご確認お願いします";
        
        switch (Model.ShowMenuIndex) {
          case 1:
    %>
        <tr>
            <%//Mod 2017/12/14 arc yano #3843 部品発注書、出庫伝票ボタンの表示、それに伴いレイアウトの変更%>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <td  style="width:70px" onclick="ServiceFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <% // Edit 2014/06/20 arc yano 税率変更バグ修正 POST可／不可をチェック%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td style="width:70px" onclick="document.forms[0].ActionType.value='Quote';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td style="width:80px" onclick ="calcTotalServiceAmount();document.forms[0].ActionType.value='Quote';document.forms[0].PrintReport.value='ServiceQuote';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Quote';document.forms[0].PrintReport.value='ServiceInstruction';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');dispProgressed('ServiceSalesOrder', 'UpdateMsg');;serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="作業指示書" class="command_btn" />&nbsp;作業指示書</td>
                        <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Quote';document.forms[0].Output.value=true;document.getElementById('StockCode').value='999'; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品在庫出庫伝票" class="command_btn" />&nbsp;出庫伝票</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Quote';document.forms[0].Output.value=true;document.getElementById('StockCode').value=''; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品発注書" class="command_btn" />&nbsp;部品発注書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='SalesOrder';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/build.png" alt="受注処理" class="command_btn" />&nbsp;受注処理</td>
            <%if (Model.RevisionNumber > 0) { %>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="calcTotalServiceAmount();formList();document.forms[0].ActionType.value='Cancel';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
            <td onclick="if(confirm('ステータスを「作業履歴」に変更しても良いですか？')){if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='History';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit()}"><img src="/Content/Images/build.png" alt="作業履歴へ" class="command_btn" />&nbsp;作業履歴へ</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='Stop';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit()"><img src="/Content/Images/cancel.png" alt="作業中止" class="command_btn" />&nbsp;作業中止</td>
            <%} %>
        </tr>
    <%
        break;
          case 2:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <% // Edit 2014/06/20 arc yano 税率変更バグ修正 POST可／不可をチェック%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='ServiceQuote';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='ServiceInstruction';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="作業指示書" class="command_btn" />&nbsp;作業指示書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='999'; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品在庫出庫伝票" class="command_btn" />&nbsp;出庫伝票</td><%//Mod 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value=''; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品発注書" class="command_btn" />&nbsp;部品発注書</td><%//Add 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='OutSourceRequest';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="外注依頼書" class="command_btn" />&nbsp;外注依頼書</td><%//Mod 2022/07/05 yano #4142 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='StartWorking';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit()"><img src="/Content/Images/build.png" alt="作業開始" class="command_btn" />&nbsp;作業開始</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
        </tr>
    <% 
        break;
          case 3:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <% // Edit 2014/06/20 arc yano 税率変更バグ修正 POST可／不可をチェック%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='ServiceQuote';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='ServiceInstruction';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="作業指示書" class="command_btn" />&nbsp;作業指示書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='999'; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品在庫出庫伝票" class="command_btn"/>&nbsp;出庫伝票</td><%//Mod 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品発注書" class="command_btn" />&nbsp;部品発注書</td><%//Mod 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='OutSourceRequest';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="外注依頼書" class="command_btn" />&nbsp;外注依頼書</td><%//Mod 2022/07/05 yano #4142 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='EndWorking';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/build.png" alt="作業終了" class="command_btn" />&nbsp;作業終了</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
        </tr>
    <%
        break;
          case 4:
               %>
        <tr>
            <%// Mod 2021/08/03 yano #4093 %>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='999'; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品在庫出庫伝票" class="command_btn" />&nbsp;出庫伝票</td><%//Add 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品発注書" class="command_btn" />&nbsp;部品発注書</td><%//Add 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='OutSourceRequest';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="外注依頼書" class="command_btn" />&nbsp;外注依頼書</td><%//Mod 2022/07/05 yano #4142 %><%//Add 2018/03/20 arc yano #3872%>
            <td onclick="calcTotalServiceAmount();document.forms[0].PrintReport.value='ServiceSales';document.forms[0].ActionType.value='SalesConfirm';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
        </tr>
    <%
        break;
          case 5:
    %>
        <tr>
            <%// Mod 2021/08/03 yano #4093 %>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value='999'; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品在庫出庫伝票" class="command_btn" />&nbsp;出庫伝票</td><%//Add 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].Output.value=true;document.getElementById('StockCode').value=''; DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/excel.png" alt="部品発注書" class="command_btn" />&nbsp;部品発注書</td><%//Add 2017/10/19 arc yano #3803 %>
            <td onclick="calcTotalServiceAmount();document.forms[0].ActionType.value='Update';document.forms[0].PrintReport.value='OutSourceRequest';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="外注依頼書" class="command_btn" />&nbsp;外注依頼書</td><%//Mod 2022/07/05 yano #4142 %><%//Add 2018/03/20 arc yano #3872%>
            <td onclick="calcTotalServiceAmount();document.forms[0].PrintReport.value='ServiceSales';document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
            <td onclick="document.forms[0].ActionType.value='Sales';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceSalesSubmit();"><img src="/Content/Images/build.png" alt="納車処理" class="command_btn" />&nbsp;納車処理</td>
            <td onclick="document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
        </tr>
    <%
        break;
          case 6:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <% // Mod  2015/03/17 arc nakayama 伝票修正対応　伝票が修正中だった場合は　閉じる・保存・修正キャンセルにする %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <%if (Model.ModificationControl){ %>
            
                <% // ▼修正中　%>
                <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
                <%if(Model.ModificationControlCommit) {%><%//月次締め処理状況が仮締め以上なら表示させない %>
                <td onclick="document.forms[0].ActionType.value='ModificationEnd';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="修正完了" class="command_btn" />&nbsp;修正完了</td>
                <%} %>    
                <td onclick="document.forms[0].ActionType.value='ModificationCancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();;"><img src="/Content/Images/cancel.png" alt="修正キャンセル" class="command_btn" />&nbsp;修正キャンセル</td>
            
            <%}else{ %>

                <% // ▼修正中でない　%>
                <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
                <td onclick="calcTotalServiceAmount();document.forms[0].PrintReport.value='ServiceSales';document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
                <% // Edit 2014/06/20 arc yano 税率変更バグ修正 POST可／不可をチェック%>
                <td onclick="document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');chkSubmit('ServiceSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
                <% if (Model.ModificationEnabled){ %><%//Add arc nakayama 伝票修正対応　支店長・システム管理者だった場合は伝票修正ボタン表示 %>
                       <td onclick="document.forms[0].ActionType.value='ModificationStart';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();"><img src="/Content/Images/build.png" alt="伝票修正" class="command_btn" />&nbsp;伝票修正</td>
                <% }%>

            <%} %>
        </tr>
    <%
        break;
           case 7:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='ServiceQuote';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');printReport();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td>            
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='ServiceInstruction';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');printReport();"><img src="/Content/Images/pdf.png" alt="作業指示書" class="command_btn" />&nbsp;作業指示書</td>
        </tr>
    <%
        break;    
          case 8:
    %>
        <tr>
            <% // Add 2015/10/19 arc nakayama #3254_作業履歴伝票を活用できるようにしたい %>
            <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>

            <%//作業履歴なら「見積に戻す」ボタン / それ以外、何も表示しない%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <%if(Model.ServiceOrderStatus.Equals("009")){ %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='Restore';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();"><img src="/Content/Images/build.png" alt="見積に戻す" class="command_btn" />&nbsp;見積に戻す</td>
            <%} %>
        </tr>
    <%
        break;  
          case 9:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='Restore';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();"><img src="/Content/Images/build.png" alt="見積に戻す" class="command_btn" />&nbsp;見積に戻す</td>
        </tr>
    <%  break;
          case 10:
    %>
        <tr>
            <% // Edit 2014/06/17 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(confirm('赤伝処理をしても宜しいですか？')){if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');}  document.forms[0].action = '/ServiceSalesOrder/Akaden';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();}"><img src="/Content/Images/build.png" alt="赤伝処理" class="command_btn" />&nbsp;赤伝処理</td>
        </tr>
    <% break;
          case 11:
    %>
        <tr>
            <% // Edit 2014/06/13 arc yano 高速化対応 閉じるボタン以外をクリック時はsubmit前に明細行データの整形を行う。%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(confirm('赤黒処理をしても宜しいですか？')){if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');}  document.forms[0].action = '/ServiceSalesOrder/Akakuro';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formSubmit();}"><img src="/Content/Images/build.png" alt="赤伝処理" class="command_btn" />&nbsp;赤黒処理</td>
        </tr>         
    <% break; 
        case 12:
    %>
        <tr>
            <% // Add 2015/03/17 arc nakayama 伝票修正　伝票修正ボタン表示%>
            <!--Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる グルグルアニメーション追加-->
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="calcTotalServiceAmount();document.forms[0].PrintReport.value='ServiceSales';document.forms[0].ActionType.value='Update';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceFormSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
            <td onclick="document.forms[0].ActionType.value='Sales';DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');serviceSalesSubmit();"><img src="/Content/Images/build.png" alt="納車処理" class="command_btn" />&nbsp;納車処理</td>
            <td onclick="document.forms[0].ActionType.value='Cancel';formList();DisplayImage('UpdateMsg', '0');dispProgressed('ServiceSalesOrder', 'UpdateMsg');formServiceCarCancel();"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td>
        </tr>
    <%
       break;
       
        } %>
    </table>
