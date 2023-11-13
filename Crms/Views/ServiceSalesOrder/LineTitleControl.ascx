<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CrmsDao.ServiceSalesHeader>" %>
<% 
   // -----------------------------------------------------------------------------------------------------
   // Mod 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
   
   // Mod 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
   //                               ①金額欄のテキストボックスのクラス名をnumeric→moneyに変更
   //                               ②金額欄の初期値をカンマ表示(=string.Format("{0:N0}")とする
   // ----------------------------------------------------------------------------------------------------- 
%>

    <table class="input-form-line">
        <tr>
            <th nowrap style="width:93px">
                <%if(Model.LineEnabled){ %>
                    <%=Html.DropDownList("ServiceType", (IEnumerable<SelectListItem>)ViewData["ServiceTypeList"])%>
                <%}else{ %>
                    <%=Html.DropDownList("ServiceType", (IEnumerable<SelectListItem>)ViewData["ServiceTypeList"],new {@disabled="disabled"})%>
                <%} %>
            </th>
            <th>
                <% //Edit 2014/06/16 arc yano 高速化対応 
                   //①「５行追加」ボタンクリック時に、クライアント側で処理するように変更
                   //② LineEnabledを参照して、ボタン表示／非表示を切り替える
                %>
                <%if(Model.LineEnabled){ %>
                    <% //Edit 2014/07/04 arc yano chrome対応 type=button追加%>
                        <button type="button" style="width:60px;height:20px" onclick="$('#EditType').val('add');$('#AddSize').val(5);editList();">5行追加</button>
                <%} %>
            </th>
        </tr>
    </table>
    <table class="input-form-line">
        <tr>
            <th nowrap style="width:100px;height:27px">
                <%if(Model.LineEnabled){ %>
                <% //2014/06/09 arc yano 高速化対応 「行追加」ボタンクリック時に、クライアント側で処理するように変更%>
                <% //Edit 2014/07/04 arc yano chrome対応 type=button追加%>
                   <button type="button" style="width:60px;height:20px" onclick="$('#EditType').val('add');$('#AddSize').val(1);editList();">行追加</button>

                <%} %>
            </th>
            <th nowrap style="width:100px">種別</th><%//Mod 2015/10/28 arc yano #3289 93px→100px%>
            <th nowrap style="width:130px">コード</th>
            <th nowrap style="width:200px">作業内容・部品</th>
            <th nowrap style="width:60px">区分</th>
            <th nowrap style="width:70px">工数(h)</th>
            <th nowrap style="width:50px">ﾚﾊﾞﾚｰﾄ<br />
                <%if(Model.LineEnabled){ %>
                    <%=Html.TextBox("LaborRate", string.Format("{0:N0}", Model.LaborRate), new { @class = "money", style="width:35px", maxlength = "10" })%>
                <%}else{ %>
                    <!--Add 2015/08/13 arc nakayama #3240_サービス伝票のレバレート、走行距離の単位、支払合計の表示不具合 ステータス納車済みで保存し再描画される時に、データがバインドされないためデータソースと同じ名前の隠し項目を追加-->
                    <%=Html.TextBox("disLaborRate", string.Format("{0:N0}", Model.LaborRate), new { @class = "money", style = "width:35px", maxlength = "10", @disabled="disabled"})%>
                    <%=Html.Hidden("LaborRate", Model.LaborRate)%>
                <%} %></th>
            <th nowrap style="width:70px">技術料</th>
            <th nowrap style="width:60px">在庫</th><% //Add 2015/10/28 arc yano #3289 幅変更(40px → 60px) %>
            <th nowrap style="width:90px">判断</th><%// 2016/02/19 arc yano #3435 width 60px → 90px%>
            <th nowrap style="width:100px">発注書・出庫表</th><%//Add 2017/10/19 arc yano #3803%>
            <th nowrap style="width:60px">数量</th>
            <th nowrap style="width:60px">引当済数</th><% //Add 2015/10/28 arc yano #3289 サービス伝票 引当済数、発注数の追加 %>
            <th nowrap style="width:60px">発注数</th><% //Add 2015/10/28 arc yano #3289 サービス伝票 引当済数、発注数の追加 %>
            <th nowrap style="width:70px">単価</th>
            <th nowrap style="width:70px">金額</th>
            <th nowrap style="width:70px">原価(単価)</th>
            <th nowrap style="width:70px">原価(合計)</th>
            <th nowrap style="width:340px">メカニック/外注先/請求先<%if(Model.LineEnabled){ %> [ <a href="javascript:void(0);"  onclick="document.forms[0].action='/ServiceSalesOrder/SetCustomerClaim';formList();formSubmit();return false;">一括設定</a> ]<%} %></th><%//Edit 2014/06/16 arc yano 高速化対応 %>
            <th nowrap style="width:100px">連絡事項</th>
        </tr>
    </table>
    
