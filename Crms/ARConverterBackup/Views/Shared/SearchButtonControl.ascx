<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string[]>" %>
<%@ Import Namespace="CrmsDao" %> 

<%
    /*-------------------------------------------------------------------------------------------------------------------------
 * ルックアップボタンの部分ビュー
 * 更新履歴：
 *   2021/03/26 sola owashi Chrome対応 IEでも動作するように変更
 *   2021/02/16 sola owashi Chrome対応 検索ボタン押下時に呼び出すウィンドウの
 *              モードレス化に伴い onclick 属性の処理をコールバック関数化
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 
 *   　　　　　　　　　　　　 引数追加(ボタンクリック可／不可フラグ)
 *   2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える 
 *                         openSearchDialogの戻り値を取得する変数を追加 
 *   2014/08/18 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
 *   2014/07/10 高速化対応 chrome対応 子画面からフォーカスが戻った場合、時間を置いてフォーカス移動を行う。
 *   2014/06/06 高速化対応 id追加
 * -----------------------------------------------------------------------------------------------------------------------*/
%>

<% //Mod 2014/08/18 arc amii %>
<%     
    bool editFlag = true;
    if (Model.Count() > 6 && Model[6] != null && bool.Parse(Model[6]) == false)
    {
        editFlag = false;
    } 
%>
<%if (CommonUtils.DefaultString(Model[3]).Equals("0"))
  {   
%>
    <%if (Model.Count() > 5 && Model[5] != null) { %>
    <%if (editFlag == true) { %>
    
<button type="button" id="<%=Model[5] %>" style="width:21px;height:21px;" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);if(document.getElementById('<%=Model[0] %>'))setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>'))document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>} else  {var callback = function(dRet){if(document.getElementById('<%=Model[0] %>')) setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>')) document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>}; openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>, undefined, undefined, undefined, undefined, callback);}">
<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
    <%}else{ %>
<button type="button" id="Button1" style="width:21px;height:21px;" disabled ="disabled" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);if(document.getElementById('<%=Model[0] %>'))setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>'))document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>} else  {var callback = function(dRet){if(document.getElementById('<%=Model[0] %>')) setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>')) document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>}; openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>, undefined, undefined, undefined, undefined, callback);}">
<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
    <%} %>
    <%}else{ %>
<%if (editFlag == true) { %>
 <button type="button" style="width:21px;height:21px;" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);if(document.getElementById('<%=Model[0] %>'))setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>'))document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>} else  {var callback = function(dRet){if(document.getElementById('<%=Model[0] %>')) setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>')) document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>}; openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>, undefined, undefined, undefined, undefined, callback);}">

<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
<%}else{ %>
 <button type="button" style="width:21px;height:21px;" disabled ="disabled" onclick="var userAgent = window.navigator.userAgent.toLowerCase(); if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);if(document.getElementById('<%=Model[0] %>'))setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>'))document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>} else  {var callback = function(dRet){if(document.getElementById('<%=Model[0] %>')) setTimeout(function(){document.getElementById('<%=Model[0] %>').focus();if(document.getElementById('<%=Model[1] %>')) document.getElementById('<%=Model[1] %>').focus()}, 100);<%=Model.Count()>4 && Model[4]!=null ? Model[4] : "" %>}; openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>, undefined, undefined, undefined, undefined, callback);}">

<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
<%} %>
    <%} %>
       
<%}else{ %>
<%if (editFlag == true) { %>
<button type="button" style="width:21px;height:21px;" onclick="var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);" disabled="disabled">
<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
<%}else{ %>
<button type="button" style="width:21px;height:21px;" disabled ="disabled" onclick="var dRet = openSearchDialog('<%=Model[0] %>','<%=Model[1] %>',<%=Model[2] %>);" disabled="disabled">
<img alt="検索" style="width:12px;height:12px" src="/Content/Images/Search.jpg" />
</button>
<%} %>
<%} %>
