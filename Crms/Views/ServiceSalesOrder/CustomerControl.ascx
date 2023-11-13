<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ServiceSalesHeader>" %>
<%@ Import Namespace="CrmsDao" %>
    <table class="input-form-slim">
        <tr>
            <th colspan="2" class="input-form-title">顧客情報</th>
        </tr>
        <tr>
            <th style="width:100px;" valign="top">顧客コード
            <%//Add 2022/05/03 yano #4133%>
            <span id="CustomerInfo" style="margin-left:2em; visibility:<%=Model.Customer != null && !string.IsNullOrWhiteSpace(Model.Customer.Memo) ? "visible" : "hidden" %>;"><img src="/Content/Images/info.png" width="15px" height="15px"/></span>
            </th>
            <td style="width:300px">
                <%if (Model.CustomerEnabled) { %>
                    <% //Mod 2022/05/03 yano #4133 %>
                    <% //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動 詳細情報取得中にロックする項目を引数として追加 %>
                    <% //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 住所再確認メッセージをArrayに追加 %>
                    <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric", style="width:150px", maxlength="10", onblur = "GetMasterDetailFromCode('CustomerCode',new Array('CustomerName','CustomerNameKana','CustomerAddress','CustomerTelNumber','AddressReconfirm','ReconfirmMessage','CustomerMemo'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'), null, dispCustomerInfo, null, dispCustomerInfo)" })%> 
                       <%--<%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric", style="width:150px", maxlength="10", onblur = "GetMasterDetailFromCode('CustomerCode',new Array('CustomerName','CustomerNameKana','CustomerAddress','CustomerTelNumber','AddressReconfirm','ReconfirmMessage'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" })%>--%> 
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerCode", "CustomerName", "'/Customer/CriteriaDialog'", "0" });%>
                <%} else { %>
                    <%=Html.TextBox("CustomerCode", Model.CustomerCode, new { @class = "alphanumeric readonly", style = "width:150px", maxlength = "10", @readonly = "readonly" })%> 
                    <%Html.RenderPartial("SearchButtonControl", new string[] { "CustomerCode", "CustomerName", "'/Customer/CriteriaDialog'", "1" });%>
                <%} %>
                <%
                    //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動
                    //①マスタボタンをクリック時に顧客コードが入っていない場合は、顧客マスタの新規登録画面を表示する
                    //②idを追加
                %>
                <button type="button" id="MasterForCustomer" style="width:50px;height:20px" onclick="var callback = function(){GetMasterDetailFromCode('CustomerCode',new Array('CustomerName','CustomerNameKana','CustomerAddress','AddressReconfirm','ReconfirmMessage'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))}; $('#CustomerCode').val()!='' ? openModalDialogNotMask('/Customer/IntegrateEntry/'+$('#CustomerCode').val(), null, null, null, null, callback) : openModalDialog('/Customer/IntegrateEntry/'); return false ;" >マスタ</button><%//Mod 2022/07/28 yano #4146%>
                <%--<button type="button" id="MasterForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/Customer/IntegrateEntry/'+$('#CustomerCode').val()) : openModalDialog('/Customer/IntegrateEntry/'); return false ;GetMasterDetailFromCode('CustomerCode',new Array('CustomerName','CustomerNameKana','CustomerAddress','AddressReconfirm','ReconfirmMessage'),'Customer', null, new Array('MasterForCustomer', 'HistoryForCustomer'))" >マスタ</button>--%>
                <%
                    //Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動 
                    //①履歴ボタンをクリック時に顧客コードが入っていない場合は、メッセージダイアログを表示する 
                    //②idを追加
                %>
                <button type="button" id="HistoryForCustomer" style="width:50px;height:20px" onclick="$('#CustomerCode').val()!='' ? openModalDialog('/ServiceSalesOrder/CriteriaDialog?customerCode='+$('#CustomerCode').val()) : alert('顧客コードを入力して下さい'); return false;">履歴</button>
            </td>
                
        </tr>
        <tr>
            <th>顧客名</th>
            <td><%=Html.TextBox("CustomerName", Model.Customer != null ? Model.Customer.CustomerName : "", new { @class = "readonly", @readonly = "readonly", style = "width:295px"})%></td>
        </tr>
        <tr>
            <th>顧客名(カナ)</th>
            <td><%=Html.TextBox("CustomerNameKana", Model.Customer != null ? Model.Customer.CustomerNameKana : "", new { @class = "readonly", @readonly = "readonly", style = "width:295px" })%></td>
        </tr>
        <tr>
            <th>住所</th>
            <td style="width:300px"><%=Html.TextBox("CustomerAddress",Model.Customer!=null ? Model.Customer.Prefecture + Model.Customer.City + Model.Customer.Address1 + Model.Customer.Address2 : "" , new { @class = "readonly", @readonly = "readonly", style = "width:295px" })%></td>
        </tr>
        <tr>
            <th>電話番号</th>
            <td style="width:300px"><%=Html.TextBox("CustomerTelNumber", Model.Customer != null ? Model.Customer.TelNumber : "", new { @class = "readonly", @readonly = "readonly", style = "width:295px" })%></td>
        </tr>
        <tr>
            <th>依頼事項</th>
            <td style="width:300px">
                <%if(Model.CustomerEnabled){ %>
                    <%=Html.TextArea("RequestContent", Model.RequestContent, 4, 35, new { maxlength = "200", style = "height:20px;width:299px; resize:none; overflow-x:hidden; overflow-y:auto;"})%>
                <%}else{ %>
                    <%=Html.TextArea("RequestContent", Model.RequestContent, 4, 35, new {@readonly="readonly",@class="readonly",style="height:20px;width:299px; resize:none; overflow-x:hidden; overflow-y:auto;", maxlength = "200" })%>
                <%} %>
            </td>
        </tr>
    </table>
<%=Html.Hidden("AddressReconfirm",Model.Customer!=null ? Model.Customer.AddressReconfirm : false) %>
<%=Html.Hidden("CustomerMemo",Model.Customer!=null ? Model.Customer.Memo : "") %>