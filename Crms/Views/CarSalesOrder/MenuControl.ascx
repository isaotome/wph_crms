<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.CarSalesHeader>" %>
    <div id="command_button">
    <table class="command">
     <%
        /*--------------------------------------------------------------------------------------------------------------
         * Mod 2021/08/03 yano #4094【車両伝票入力】ステータス=登録済の伝票に保存ボタンを追加
         * Mod 2019/10/04 yano #4017 【車両伝票入力】伝票保存時の環境性能割の再計算の停止
         * Mod 2018/08/29 yano #3909 車両伝票　作業依頼書ボタンの表示条件の変更
         * Mod 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について 登録済へ進めるボタンを追加
         * Mod 2017/11/10 arc yano 3787 車両伝票で古い伝票で上書き防止する機能　新規作成 
         * Mod 2014/06/20 arc yano 税率変更バグ修正 
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
         --------------------------------------------------------------------------------------------------------*/
     %>
        
    <%
        // Add 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 警告メッセージの設定
        string message = "住所要確認となっているので、登録された住所をお客様にご確認お願いします";
        
        switch (Model.ShowMenuIndex) {
          case 1: 
    %>
        <tr>
            <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
            <% // Edit 2014/06/20 arc yano 税率変更バグ対応 %>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <% // <td onclick="calcTotalAmount();formSubmit()"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td> %>
            <td onclick="chkSubmit('CarSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarQuote';formSubmit();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].action = '/CarSalesOrder/Order';formSubmit();"><img src="/Content/Images/build.png" alt="受注処理" class="command_btn" />&nbsp;受注処理</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <%if (Model.RevisionNumber > 0) { %>
            <td onclick="calcTotalOptionAmount(); calcTotalAmount();document.forms[0].Cancel.value='1';formServiceCarCancel();document.forms[0].Cancel.value='0'"><img src="/Content/Images/cancel.png" alt="伝票削除" class="command_btn" />&nbsp;伝票削除</td><%--Mod 2019/10/04 yano #4017--%><%//Mod 2017/11/03 arc yano #3773 既存バグ】伝票削除ボタン押下後、キャンセルを押した後に保存すると伝票削除されてしまう %><%--Mod 2019/09/04 yano #4011--%>
            <%} %>
        </tr>
    <%  break;
          case 2:
    %>
        <tr>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='CarQuote';printReport();"><img src="/Content/Images/pdf.png" alt="見積書" class="command_btn" />&nbsp;見積書</td>
        </tr>
    <%  break;
          case 3:
    %>
        <tr>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].action='/CarSalesOrder/Confirm';formSubmit();"><img src="/Content/Images/build.png" alt="承認" class="command_btn" />&nbsp;承認</td>
        </tr>
    <%  break;
          case 4: 
    %>
        <tr>
            <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
            <% // Edit 2014/06/19 arc yano 税率変更バグ対応 %>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <%//<td onclick="calcTotalAmount();formSubmit();"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td> %>
            <td onclick="chkSubmit('CarSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarSalesOrder';formSubmit();"><img src="/Content/Images/pdf.png" alt="注文書" class="command_btn" />&nbsp;注文書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].requestType.value='requestService';formSubmit()"><img src="/Content/Images/build.png" alt="作業依頼" class="command_btn" />&nbsp;作業依頼</td>
<!--            <td onclick="document.forms[0].PrintReport.value='PurchaseOrderRequest';formSubmit();"><img src="/Content/Images/pdf.png" alt="発注依頼書" class="command_btn" />&nbsp;発注依頼書</td> -->
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarRegistRequest';formSubmit();"><img src="/Content/Images/pdf.png" alt="登録依頼書" class="command_btn" />&nbsp;登録依頼書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <%//Add 2018/08/07 yano #3911 登録済へ進めるボタンを表示%>
            <%if(Model.RegistButtonVisible){ %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].SalesOrderStatus.value='003';formSubmit();"><img src="/Content/Images/build.png" alt="登録済へ進める" class="command_btn" />&nbsp;登録済へ進める</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <%} %>
            <td onclick="calcTotalOptionAmount(); calcTotalAmount();document.forms[0].Cancel.value='1';formServiceCarSalesCancel();document.forms[0].Cancel.value='0'"><img src="/Content/Images/cancel.png" alt="受注後ｷｬﾝｾﾙ" class="command_btn" />&nbsp;受注後ｷｬﾝｾﾙ</td><%--Mod 2019/10/04 yano #4017--%><%//Mod 2017/11/03 arc yano #3773 既存バグ】伝票削除ボタン押下後、キャンセルを押した後に保存すると伝票削除されてしまう %><%--Mod 2019/09/04 yano #4011--%>

        </tr>
    <%  break;
          case 5:
    %>
        <tr>
            <%//Mod 2021/08/03 yano #4094%>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
            <td onclick="chkSubmit('CarSalesOrder');"><img src="/Content/Images/apply.png" alt="保存" class="command_btn" />&nbsp;保存</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarSalesOrder';formSubmit();"><img src="/Content/Images/pdf.png" alt="注文書" class="command_btn" />&nbsp;注文書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].requestType.value='requestService';formSubmit()"><img src="/Content/Images/build.png" alt="作業依頼" class="command_btn" />&nbsp;作業依頼</td><%//Add 2018/08/29 yano #3909%>
            
            <%--Mod 2018/12/21 yano #3965 WE版新システム構築 納車確認書は表示しない、ステータス遷移のみ--%>
            <%--<%if (Session["ConnectDB"] != null && Session["ConnectDB"].Equals("WPH_DB")){%>--%>
            <%if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")){%><%--Mod 2020/03/27 yano #4047--%>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarDeliveryReport';document.forms[0].SalesOrderStatus.value='004';formSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <%}else{ %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].ActionType.value='CarDeliveryReport';document.forms[0].SalesOrderStatus.value='004';formSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
            <%} %>
        </tr>
    <% break;
          case 6:
    %>
        <%if(Model.ModificationControl) {%>
        <%//▼修正中%>
            <tr>
                <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
                <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
                <%if(Model.ModificationEnabled){ %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='ModificationStart';formSubmit();"><img src="/Content/Images/build.png" alt="伝票修正" class="command_btn" />&nbsp;伝票修正</td>
                <%} %>
                <%//月次締め処理状況が仮締め以上なら表示させない %>
                <%if(Model.ModificationControlCommit) {%>
                    <td onclick="document.forms[0].ActionType.value='ModificationEnd';formSubmit();"><img src="/Content/Images/apply.png" alt="修正完了" class="command_btn" />&nbsp;修正完了</td>
                <%} %>    
                <%if(Model.ModificationControlCancel){ %>    
                    <td onclick="document.forms[0].ActionType.value='ModificationCancel';formSubmit();"><img src="/Content/Images/cancel.png" alt="修正キャンセル" class="command_btn" />&nbsp;修正キャンセル</td>
                <%} %>
            </tr>
        <%}else{ %>
            <%//▼修正中でない%>
            <tr>
                <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalAmount();document.forms[0].PrintReport.value='CarSalesOrder';formSubmit();"><img src="/Content/Images/pdf.png" alt="注文書" class="command_btn" />&nbsp;注文書</td>
                <%--Mod 2018/12/21 yano #3965 WE版新システム構築 納車確認書は表示しない、ステータス遷移のみ--%>
                 <%if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")){%>
                    <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].PrintReport.value='CarDeliveryReport';formSubmit();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
                <%} %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} calcTotalOptionAmount(); calcTotalAmount();document.forms[0].SalesOrderStatus.value='005';formSubmit()"><img src="/Content/Images/build.png" alt="納車処理" class="command_btn" />&nbsp;納車処理</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
                <%if(Model.ModificationEnabled){ %>
                    <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='ModificationStart';formSubmit();"><img src="/Content/Images/build.png" alt="伝票修正" class="command_btn" />&nbsp;伝票修正</td>
                <%} %>
            </tr>
        <%} %>
    <% break;
          case 7:
    %>
        <tr>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="formClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="calcTotalOptionAmount(); calcTotalAmount();formSubmit()"><img src="/Content/Images/apply.png" alt="販売報告登録" class="command_btn" />&nbsp;販売報告登録</td><%--Mod 2019/10/04 yano #4017--%><%--Mod 2019/09/04 yano #4011--%>
        </tr>
    <% break;
          case 8:
    %>
        <tr>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
            <td onclick="if(confirm('赤伝処理をしても宜しいですか？')){if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].action = '/CarSalesOrder/Akaden';formSubmit();}"><img src="/Content/Images/build.png" alt="赤伝処理" class="command_btn" />&nbsp;赤伝処理</td>
        </tr>
    <% break;
          case 9:
    %>
        <tr>
            <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
            <td onclick="if(confirm('赤黒処理をしても宜しいですか？')){if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');}  document.forms[0].action = '/CarSalesOrder/Akakuro';formSubmit();}"><img src="/Content/Images/build.png" alt="赤黒処理" class="command_btn" />&nbsp;赤黒処理</td>
        </tr>         
    <% //Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行
       break;
          case 10: //納車済
       %>
        <%if(Model.ModificationControl) {%>
        <%//▼修正中%>
               <tr>
                <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>

                <%if(Model.ModificationEnabled){ %>
                    <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='ModificationStart';formSubmit();"><img src="/Content/Images/build.png" alt="伝票修正" class="command_btn" />&nbsp;伝票修正</td>
                <%} %>

                <%//月次締め処理状況が仮締め以上なら表示させない %>

                <%if(Model.ModificationControlCommit) {%>
                    <td onclick="document.forms[0].ActionType.value='ModificationEnd';formSubmit();"><img src="/Content/Images/apply.png" alt="修正完了" class="command_btn" />&nbsp;修正完了</td>
                <%} %>
                 <%if(Model.ModificationControlCancel){ %>    
                    <td onclick="document.forms[0].ActionType.value='ModificationCancel';formSubmit();"><img src="/Content/Images/cancel.png" alt="修正キャンセル" class="command_btn" />&nbsp;修正キャンセル</td>
                <%} %>
            </tr>
        <%}else{ %>
        <%//▼修正中でない%>
               <tr>
                <td onclick="carSaelsFormClose()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td><%//Mod 2017/11/10 arc yano 3787 %>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='CarSalesOrder';printReport();"><img src="/Content/Images/pdf.png" alt="注文書" class="command_btn" />&nbsp;注文書</td>
                
                <%--Mod 2018/12/21 yano #3965 WE版新システム構築 納車確認書は表示しない、ステータス遷移のみ--%>
                <%if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")){%>
                    <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='CarDeliveryReport';printReport();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
                <%} %>
                 <%if(Model.ModificationEnabled){ %>
                    <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].ActionType.value='ModificationStart';formSubmit();"><img src="/Content/Images/build.png" alt="伝票修正" class="command_btn" />&nbsp;伝票修正</td>
                <%} %>
            </tr>

        <%} %>
       
    <% break;
          case 11: //伝票ロック中        //Add  2017/11/10 arc yano 3787
    %>
               <tr>
                <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
               </tr>
    <%
       break;
          default:
     %>
        <tr>
            <% // Edit 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる %>
            <td onclick="window.close()"><img src="/Content/Images/exit.png" alt="閉じる" class="command_btn" />&nbsp;閉じる</td>
            <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='CarSalesOrder';printReport();"><img src="/Content/Images/pdf.png" alt="注文書" class="command_btn" />&nbsp;注文書</td>
             
            <%--Mod 2018/12/21 yano #3965 WE版新システム構築 納車確認書は表示しない、ステータス遷移のみ--%>
            <%--<%if (Session["ConnectDB"] != null && Session["ConnectDB"].Equals("WPH_DB")){%>--%>
            <%if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")){%><%--Mod 2020/03/27 yano #4047--%>
                <td onclick="if(document.getElementById('AddressReconfirm').value == 'True'){alert('<%=message %>');} document.forms[0].reportName.value='CarDeliveryReport';printReport();"><img src="/Content/Images/pdf.png" alt="納車確認書" class="command_btn" />&nbsp;納車確認書</td>
            <%} %>
        </tr>
    <% break;
      } %>
      </table>
    </div>