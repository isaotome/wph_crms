//*******************************************************************************************************************
//　共通関数
//
//  更新履歴:
//   2014/07/04 arc yano    chrome 対応　
//                          テーブルの行など、配列型のオブジェクト(例：【サービス伝票入力】の作業内容明細行)
//                          を取得(getElementbyID等)する際は、ID名称を渡すようにパラメータを変更する。
//                          また、上記のような配列型のオブジェクトのname属性、id属性は以下のように設定すること。
//                          name = xxxx[n].xxxx("."で区切る)
//                          id   = xxxx[n]_xxxx("_"で区切る)
//*******************************************************************************************************************
//Add 2014/07/16 arc yano chrome対応 trim関数追加

//---------------------------------
//trim関数
//---------------------------------
String.prototype.trim = function () {
    return this.replace(/^\s+|\s+$/g, "");
}

//------------------------------------------------------------------------------
// 機能：getElmentsByClassName関数が未実装のブラウザの場合は自作で実装
//       
// 作成：2021/07/01 yano #4045 Chrome対応
// 履歴：
//
//------------------------------------------------------------------------------
if (typeof (document.getElementsByClassName) == 'undefined') {
    document.getElementsByClassName = function (t) {
        var elems = new Array();
        if (document.all) {
            var allelem = document.all;
        } else {
            var allelem = document.getElementsByTagName("*");
        }
        for (var i = j = 0, l = allelem.length; i < l; i++) {
            var names = allelem[i].className.split(/\s/);
            for (var k = names.length - 1; k >= 0; k--) {
                if (names[k] === t) {
                    elems[j] = allelem[i];
                    j++;
                    break;
                }
            }
        }
        return elems;
    };
}


//--------------------------------------------------------------------------------------
// 機能：array.indexOf関数の定義
// 作成：2020/08/29 yano #4061【車両マスタ】所有者変更時の車検案内情報の更新
//-------------------------------------------------------------------------------------
// This version tries to optimize by only checking for "in" when looking for undefined and
// skipping the definitely fruitless NaN search. Other parts are merely cosmetic conciseness.
// Whether it is actually faster remains to be seen.
if (!Array.prototype.indexOf)
Array.prototype.indexOf = (function (Object, max, min) {
    "use strict"
    return function indexOf(member, fromIndex) {
        if (this === null || this === undefined)
            throw TypeError("Array.prototype.indexOf called on null or undefined")

        var that = Object(this), Len = that.length >>> 0, i = min(fromIndex | 0, Len)
        if (i < 0) i = max(0, Len + i)
        else if (i >= Len) return -1

        if (member === void 0) {        // undefined
            for (; i !== Len; ++i) if (that[i] === void 0 && i in that) return i
        } else if (member !== member) { // NaN
            return -1 // Since NaN !== NaN, it will never be found. Fast-path it.
        } else                          // all else
            for (; i !== Len; ++i) if (that[i] === member) return i

        return -1 // if the value was not found, then return -1
    }
})(Object, Math.max, Math.min)



//キー押下時 
//window.document.onkeydown = onKeyDown;
//BackSpaceキー押下防止 
function onKeyDown(e) {

    if (navigator.appName == "Microsoft Internet Explorer") {

        //ALT＋←ダメ 
        if (event.keyCode == 0x25 && event.altKey == true) {
            //alert("ALT＋←はダメ！"); 
            return false;
        }
        //テキストボックス、パスワードボックスは許す 
        for (i = 0; i < document.all.tags("INPUT").length; i++) {
            if (document.all.tags("INPUT")(i).name == window.event.srcElement.name &&
                (document.all.tags("INPUT")(i).type == "text" || document.all.tags("INPUT")(i).type == "password") &&
                 document.all.tags("INPUT")(i).readOnly == false) {
                return true;
            }
        }
        //テキストエリアは許す 
        for (i = 0; i < document.all.tags("TEXTAREA").length; i++) {
            if (document.all.tags("TEXTAREA")(i).name == window.event.srcElement.name &&
                document.all.tags("TEXTAREA")(i).readOnly == false) {
                return true;
            }
        }
        //BackSpaceダメ 
        if (event.keyCode == 8) {
            //alert("BackSpaseはダメ！"); 
            return false;
        }
    } else
        if (navigator.appName == "Netscape") {
        if (e.which == 8) {
            return false;
        }
    }
}
//function window.onbeforeunload() {
//    if (((event.clientX > document.body.clientWidth) && (event.clientY < 0)) || event.altKey) {
//        alert('閉じるボタンから終了してね');
//        return false;
//    }
//}

//クローズ確認
function formClose() {
    if (confirm('作業終了前に保存はしましたか？\r\nウィンドウを閉じてよければ「はい」を選択して下さい。')) {
        window.close();
    }
}
function ServiceFormClose() {
    if (confirm('作業終了前に保存はしましたか？\r\nウィンドウを閉じてよければ「はい」を選択して下さい。')) {
        document.forms[0].action = '/ServiceSalesOrder/UnLock';
        document.forms[0].submit();
    }
}
//キャンセル確認
function formCancel() {
    if (confirm('対象伝票を削除します。\r\nよろしければ「はい」を選択して下さい。')) {
        formSubmit();
    }
}

//受注後ｷｬﾝｾﾙ確認
function formSalesCancel() {
    if (confirm('受注後のキャンセル処理を実施します。本当にキャンセル処理してもよろしいですか？')) {
        formSubmit();
    }
}

//月次処理確認
function formCalculate() {
    document.forms[0].actionType.value = "";
    if (confirm('処理を実行すると取り消せません。よろしいですか？')) {
        document.forms[0].actionType.value = 'Calculation';
        formSubmit();
    }
}

// Add 2014/09/29 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる
//キャンセル確認（車両伝票・サービス伝票用）
function formServiceCarCancel() {
    if (confirm('対象伝票を削除します。\r\nよろしければ「はい」を選択して下さい。')) {

        if (document.getElementById('AddressReconfirm').value == 'True') {
            alert('住所要確認となっているので、登録された住所をお客様にご確認お願いします');
        }

        formSubmit();
    }
}

// Add 2014/09/29 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる
//受注後ｷｬﾝｾﾙ確認（車両伝票・サービス伝票用）
function formServiceCarSalesCancel() {
    if (confirm('受注後のキャンセル処理を実施します。本当にキャンセル処理してもよろしいですか？')) {

        if (document.getElementById('AddressReconfirm').value == 'True') {
            alert('住所要確認となっているので、登録された住所をお客様にご確認お願いします');
        }

        formSubmit();
    }
}

//2度押し防止フラグ
var inProcess = "0";

//2度押し防止チェック
function checkInProcess() {
    if (inProcess == "0") {
        inProcess = "1";
        return true;
    } else {
        return false;
    }
}

/*------------------------------------------------------------------------------*/
/* 機能　： フォーム送信			                                            */
/* 作成日： ????/??/??                                                          */
/* 更新日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/*                                                                              */
/*------------------------------------------------------------------------------*/
//2度押し防止サブミット
function formSubmit() {
    if (checkInProcess()) {
        //カンマ除去
        ExceptCommaAll();   //Add 2017/02/14  arc yano #3641
        //$.blockUI({message:'<br />少々お待ち下さい<br /><br /><img src="/Content/Images/indicator.gif" /><br /><br />'});
        document.forms[0].submit();
    }
}
function ParentformSubmit() {
    parent.window.opener.document.forms[0].submit();
}

//HTML用デコード
function decodeHtml(str, flg) {
    if (str == null) {
        return "";
    }
    str = str.replace(/&amp;/g, "&");
    str = str.replace(/&quot;/g, "\"");
    str = str.replace(/&#039;/g, "\'");
    str = str.replace(/&lt;/g, "<");
    str = str.replace(/&gt;/g, ">");
    if (flg) {
        str = str.replace(/<br>/g, "\n");
    }
    return str;
}

//HTML用エスケープ
function escapeHtml(str, flg) {
    if (str == null) {
        return "";
    }
    str = str.replace(/&/g, "&amp;");
    str = str.replace(/"/g, "&quot;");
    str = str.replace(/'/g, "&#039;");
    str = str.replace(/</g, "&lt;");
    str = str.replace(/>/g, "&gt;");
    if (flg) {
        str = str.replace(/\n/g, "<br>");
    }
    return str;
}

//Mod 2021/06/23 yano Chrome対応
//テキストレングスチェック
function checkTextLength(id, len, name) {
    el = document.getElementById(id);
    if (el.value.length > len) {

        var clearstrnum = el.value.length - len;
        alert(name + "は" + len + "字以内で入力して下さい。オーバーした文字はクリアします");

        el.value = el.value.substr(0, len);

        el.focus();
    }
}

//オブジェクトの表示/非表示を切り替える
function checkExpand(ch) {
    var obj = document.all && document.all(ch) || document.getElementById && document.getElementById(ch);
    if (obj && obj.style) obj.style.display =
    "none" == obj.style.display ? "" : "none"
}

function ChDsp(flg, ch) {
    var obj = document.all && document.all(ch) || document.getElementById && document.getElementById(ch);
    if (obj && obj.style) {
        if (flg) {
            obj.style.display = "block";
        } else {
            obj.style.display = "none";
        }
    }
}

// オブジェクトの絶対位置を取得する
function AbsPos(obj) {
    this.left = 0;
    this.top = 0;
    this.rewind = function(obj, start) {
        if (obj == document.body) return;
        this.left += obj.offsetLeft;
        this.top += obj.offsetTop;
        if (!start && (obj.tagName == 'TD' || obj.tagName == 'TH')) {
            this.left += obj.clientLeft;
            this.top += obj.clientTop;
        }
        this.rewind(obj.offsetParent, false);
    }
    this.rewind(obj, true);
}

// ウインドウリサイズ時に明細の幅を合わせる
function resizeDiv() {
    var target = document.getElementById('line');
    var target2 = document.getElementById('head');

    pos = new AbsPos(target);
    target.style.width = document.body.clientWidth - pos.left + 20;
    head.style.width = document.body.clientWidth - pos.left + 2;
}

//ボタンによって表示を切り替える（車両）
function changeDisplayContent(ch) {
    ChDsp(0, 'basic');
    ChDsp(0, 'used');
    ChDsp(0, 'loan');
    ChDsp(0, 'invoice');
    ChDsp(0, 'detail');
    ChDsp(0, 'tax');
    ChDsp(0, 'VoluntaryInsurance');
    ChDsp(0, 'claimable');
    ChDsp(1, ch);
}

function changeDisplayLoan(plan) {
    ChDsp(0, 'NoLoan');
    ChDsp(0, 'LoanA');
    ChDsp(0, 'LoanB');
    ChDsp(0, 'LoanC');
    ChDsp(1, plan);
}

function changeDisplayCustomer(customer) {
    ChDsp(0, 'Basic');
    ChDsp(0, 'Customer');
    ChDsp(0, 'CustomerClaim');
    ChDsp(0, 'Supplier');
    ChDsp(0, 'SupplierPayment');
    ChDsp(0, 'CustomerDM');
    ChDsp(0, 'UpdateLog');
    ChDsp(1, customer);
}
function changeDisplayCarGrade(ch) {
    ChDsp(0, 'Basic');
    ChDsp(0, 'Color');
    ChDsp(1, ch);
}
function changeSearchDisplay() {
    var element = document.getElementById('search-form');
    if (element && element.style.display == 'none') {
        element.style.display = 'block';
    } else {
        element.style.display = 'none';
    }
}

//Add 2015/01/26 arc yano #3153 入金実績リスト追加対応
//Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）サマリ表示追加
function changeDisplayReceipt(name) {
    ChDsp(0, 'Plan');
    ChDsp(0, 'Result');
    ChDsp(0, 'History');
    ChDsp(0, 'ResultCard');
    ChDsp(0, 'SumPlan');
    ChDsp(1, name);

    //Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）入金予定タブ表示時のみラジオボタンを出す
    //ラジオボタンの表示切替（入金予定の時だけ表示）
    if (name == 'Plan' || name == 'SumPlan') {
        document.getElementById('ReceiptRadio').style.visibility = "visible";
    } else {
        document.getElementById('ReceiptRadio').style.visibility = "hidden";
    }
}

/* 子要素の表示・非表示切替 */
function openFolder(childObj, parentObj) {
    var child = "";
    var parent = "";
    var sw = "/Content/Images/minus.gif"; /* フォルダ表示時のアイコン画像 */
    var hd = "/Content/Images/plus.gif"; /* フォルダ非表示時のアイコン画像 */
    child = document.getElementById(childObj).style;
    parent = document.getElementById(parentObj);
    if (child.display == "none") {

        // Mod 2014/07/03 arc amii chrome対応 IEとIE以外で、style.Displayに設定する値を変えるよう修正
        if (navigator.appName == "Microsoft Internet Explorer") {
            child.display = "block";
        } else {
            child.display = "";
        }
        parent.src = sw;
    } else {
        child.display = "none";
        parent.src = hd;
    }
}

/*
//検索結果の表示
function displaySearchList(pageIndex) {
    if (pageIndex == null) {
        document.forms[0].id.value = 0;
    } else {
        document.forms[0].id.value = pageIndex;
    }
    formSubmit();
}
*/
//Mod 2015/02/06 arc yano #3153 入金実績リスト追加対応
//検索結果の表示
function displaySearchList(pageIndex) {
    var index = 0;

    //１画面複数ページング
    if (document.getElementById("indexid")) {
        //何番目のidかを設定
        if (document.getElementById("indexid").value == "") {
            index = 0;
        }
        else {
            index = parseInt(document.getElementById("indexid").value);
        }

        var idarray = document.getElementsByName("id");

        if (pageIndex == null) {
            idarray[index].value = 0;
        } else {
            idarray[index].value = pageIndex;
        }
    }
    else {  //１画面で１ページング

        if (pageIndex == null) {
            document.forms[0].id.value = 0;
        } else {
            document.forms[0].id.value = pageIndex;
        }
    }
  
    formSubmit();
}

//関係の追加
function addRelation() {
    document.forms[0].action.value = "add";
    displaySearchList();
}

//関係の削除
function removeRelation(key1, key2) {
    document.forms[0].action.value = "remove";
    document.forms[0].key1.value = key1;
    document.forms[0].key2.value = key2;
    displaySearchList();
}

//Mod 2014/09/25 arc yano #3094 締め処理の文言変更 本締め処理の文言を「本締め」に統一する。
//Mod 2014/09/18 arc yano 締め処理の種類により、確認ダイアログのメッセージを変更する。
//Mod 2014/09/17 arc yano #3090 締め処理実行確認ダイアログ表示シーケンスエラー対応　確認ダイアログを表示する前に、入力値のチェックを行う。
//Add 2014/09/11 arc yano IPO対応 締め処理実行前に確認ダイアログを表示する
//締め処理確認
function formExecClose() {

    var i;

    //------------------------------
    //入力値のチェック
    //------------------------------
    //締め処理の選択値を取得
    var select = document.getElementById('CloseType');
    var options = select.options;
    var type = options.item(select.selectedIndex).value;


    //処理範囲の選択値を取得
    var radios = document.getElementsByName('TargetRange');

    for (i = 0; i < radios.length; i++) {

        if (radios[i].checked == true) {
            break;
        }
    }
    var targetrange = radios[i].value;

    //処理範囲が部門単位の場合
    if (targetrange == "1") {
        //締め処理タイプが月次締めの場合
        if (type == "003") {
            alert("本締めは、部門単位で行えません");
            return false;
        }
        else {//月次締め処理以外の場合
            
            //部門コードが入力されているかをチェックする。
            if (document.getElementById('DepartmentCode').value == '') {
                alert("部門は必須項目です");
                return false;
            }
        }
    }

    //---------------------------------------
    //確認ダイアログ表示
    //---------------------------------------
    //メッセージ設定
    var msg;
    switch (type) {

        //仮締め解除
        case '001':
            msg = '仮締め解除を実行します。よろしければ「OK」を選択してください';
            break;

        //仮締め処理
        case '002':
            msg = '仮締めを実行します。よろしければ「OK」を選択してください';
            break;

        //本締め処理
        case '003':
            msg = '本締めを実行します。よろしければ「OK」を選択してください';
            break;

        //上記以外(ここにはこない)
        default:
            //処理なし
            break;
    }


    if (confirm(msg)) {
        document.getElementById('RequestFlag').value = '1';
        displaySearchList();
    }
}

//----------------------------------------------------------------------------------------
//機能：検索ダイアログで選択
//作成日：??????
//更新日：2014/07/23 arc yano  chrome対応
//        ①returnValueにセットする変数の型の変更(Object→Array)
//        ②htmlから値を取得する場合に、htmlがnull出ない場合は、trim()処理を追加
//----------------------------------------------------------------------------------------
function selectedCriteriaDialog(code, prefix, value) {

    var args = new Array();     //args[0]…code , args[1]…name
    //var args = new Object();
    args[0] = code;
    //args.code = code;
    if (value) {
        args[1] = value;
    } else {
        if (prefix == "EmployeeName") {
            code = code.replace(/\./g, "_");
        }
        
        if ($('#' + prefix + '_' + code).html() != null) {
        args[1] = $('#' + prefix + '_' + code).html().trim();
        }
        else {
            args[1] = $('#' + prefix + '_' + code).html();
        }
        //args.name = $('#' + prefix + '_' + code).html();
    }

    //window.returnValue = args;
    this.parent.window.returnValue = args;
    this.parent.close();

}

//支払先マスタの支払種別変更時制御
function changePaymentType() {
    var paymentType = document.forms[0].PaymentType;
    var paymentDayCount = document.forms[0].PaymentDayCount;
    if (paymentType.selectedIndex == -1) {
        paymentDayCount.value = "";
        paymentDayCount.readOnly = true;
        paymentDayCount.style.backgroundColor = "silver";
    } else {
        if (paymentType.options[paymentType.selectedIndex].value == "003") {
            paymentDayCount.readOnly = false;
            paymentDayCount.style.backgroundColor = "#ffffff";
        } else {
            paymentDayCount.value = "";
            paymentDayCount.readOnly = true;
            paymentDayCount.style.backgroundColor = "#e0e0e0";
        }
    }
}

//Mod 2014/07/15 arc yano chrome対応 getElementByIdのパラメータをname→idに
function changePaymentType2() {
    var paymentType = document.getElementById('pay_PaymentType');
    var paymentDayCount = document.getElementById('pay_PaymentDayCount');
    if (paymentType.selectedIndex == -1) {
        paymentDayCount.value = "";
        paymentDayCount.readOnly = true;
        paymentDayCount.style.backgroundColor = "silver";
    } else {
        if (paymentType.options[paymentType.selectedIndex].value == "003") {
            paymentDayCount.readOnly = false;
            paymentDayCount.style.backgroundColor = "#ffffff";
        } else {
            paymentDayCount.value = "";
            paymentDayCount.readOnly = true;
            paymentDayCount.style.backgroundColor = "#e0e0e0";
        }
    }
}

//支払先マスタの日数フォーカス取得時制御
function focusPaymentDayCount() {
    var paymentType = document.forms[0].PaymentType;
    var paymentDay = document.forms[0].PaymentDay;
    if (paymentType.selectedIndex == -1) {
        paymentDay.focus();
    } else {
        if (paymentType.options[paymentType.selectedIndex].value == "003") {
        } else {
            paymentDay.focus();
        }
    }
}

//Mod 2014/07/15 arc yano chrome対応 getElementByIdのパラメータをname→idに
//支払先マスタの日数フォーカス取得時制御
function focusPaymentDayCount2() {
    var paymentType = document.getElementById('pay_PaymentType');
    var paymentDay = document.getElementById('pay_PaymentDay');
    if (paymentType.selectedIndex == -1) {
        paymentDay.focus();
    } else {
        if (paymentType.options[paymentType.selectedIndex].value == "003") {
        } else {
            paymentDay.focus();
        }
    }
}

//在庫ステータスの変更
function changeCarStatus() {
    if (document.forms[0].ChangeCarStatus.selectedIndex <= 0) {
        alert("変更ステータスを選択して、変更ボタンを押して下さい");
        return;
    }
    document.forms[0].action.value = "change";
    var chkTarget = document.forms[0].chkTarget;
    var existsCheck = false;
    if (chkTarget.length > 0) {
        for (var i = 0; i < chkTarget.length; i++) {
            if (chkTarget[i].checked) {
                existsCheck = true;
                break;
            }
        }
    } else {
        if (chkTarget.checked) {
            existsCheck = true;
        }
    }
    if (existsCheck) {
        displaySearchList();
    } else {
        alert("変更対象1つ以上の明細にチェックを入れて、変更ボタンを押して下さい");
        return;
    }
}

//セットメニューの明細種別変更
function changeSetMenuDetailsType(i) {
    var doc = document;
    /*  Mod 2014/07/10 arc amii chrome対応 document.getElementByIdの指定がIDではなくnameになっていたのを修正  */
    switch (doc.getElementById("line[" + i + "]_ServiceType").value) {
        case "001":
            doc.getElementById("line[" + i + "]_InputWork").style.display = "inline";
            doc.getElementById("line[" + i + "]_InputParts").style.display = "none";
            doc.getElementById("line[" + i + "]_InputService").style.display = "none";
            doc.getElementById("line[" + i + "]_InputComment").style.display = "none";
            doc.getElementById("line[" + i + "]_InputComment").style.display = "none";
            doc.getElementById("line[" + i + "]_ServiceMenuCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceMenuName").innerText = "";
            doc.getElementById("line[" + i + "]_PartsNumber").value = "";
            doc.getElementById("line[" + i + "]_PartsNameJp").innerText = "";
            doc.getElementById("line[" + i + "]_Comment").value = "";
            doc.getElementById("line[" + i + "]_InputQuantity").style.display = "none";
            doc.getElementById("line[" + i + "]_Quantity").value = "";
            break;
        case "002":
            doc.getElementById("line[" + i + "]_InputWork").style.display = "none";
            doc.getElementById("line[" + i + "]_InputParts").style.display = "none";
            doc.getElementById("line[" + i + "]_InputService").style.display = "inline";
            doc.getElementById("line[" + i + "]_InputComment").style.display = "none";
            doc.getElementById("line[" + i + "]_ServiceWorkCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceWorkName").innerText = "";
            doc.getElementById("line[" + i + "]_PartsNumber").value = "";
            doc.getElementById("line[" + i + "]_PartsNameJp").innerText = "";
            doc.getElementById("line[" + i + "]_Comment").value = "";
            doc.getElementById("line[" + i + "]_InputQuantity").style.display = "none";
            doc.getElementById("line[" + i + "]_Quantity").value = "";
            break;
        case "003":
            doc.getElementById("line[" + i + "]_InputWork").style.display = "none";
            doc.getElementById("line[" + i + "]_InputService").style.display = "none";
            doc.getElementById("line[" + i + "]_InputParts").style.display = "inline";
            doc.getElementById("line[" + i + "]_InputComment").style.display = "none";
            doc.getElementById("line[" + i + "]_ServiceWorkCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceWorkName").innerText = "";
            doc.getElementById("line[" + i + "]_ServiceMenuCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceMenuName").innerText = "";
            doc.getElementById("line[" + i + "]_Comment").value = "";
            doc.getElementById("line[" + i + "]_InputQuantity").style.display = "inline";
            break;
        case "004":
            doc.getElementById("line[" + i + "]_InputWork").style.display = "none";
            doc.getElementById("line[" + i + "]_InputService").style.display = "none";
            doc.getElementById("line[" + i + "]_InputParts").style.display = "none";
            doc.getElementById("line[" + i + "]_InputComment").style.display = "inline";
            doc.getElementById("line[" + i + "]_ServiceWorkCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceWorkName").innerText = "";
            doc.getElementById("line[" + i + "]_ServiceMenuCode").value = "";
            doc.getElementById("line[" + i + "]_ServiceMenuName").innerText = "";
            doc.getElementById("line[" + i + "]_PartsNumber").value = "";
            doc.getElementById("line[" + i + "]_PartsNameJp").innerText = "";
            doc.getElementById("line[" + i + "]_InputQuantity").style.display = "none";
            doc.getElementById("line[" + i + "]_Quantity").value = "";
            break;
    }
}


/*--------------------------------------------------------------------------------------------------------------------*/
/* 機能　： サービス明細の表示切替                                                                                                                           */
/* 作成日： ????                                                                                                                                             */
/* 更新日： 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力                                                   */
/*          2016/04/14 arc yano #3480 サービス伝票　サービス伝票 請求先分類を初期化する処理を追加                     */
/*          2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応 種別選択時に区分リストを再取得する             */
/*          2016/03/17 arc yano #3471 サービス伝票入力　区分の絞込みの対応 主作業の場合、区分を選択不可                                                      */
/*          2015/11/05 arc yano #3289 サービス伝票 引当済数、発注数の追加                                                                                    */
/*          2014/07/03 arc yano chrome対応 getElementbyIdのパラメータをname→idに変更                                                                        */
/*          2014/06/13 arc yano サービスタイプ主作業変更時には文字色は黒にする                                                                               */
/*--------------------------------------------------------------------------------------------------------------------*/
function ChangeServiceType(num) {

    var doc = document;
    //*** クリア ***

    // コード
    doc.getElementById('line[' + num + ']_InputServiceWorkCode').style.display = "none";
    doc.getElementById('line[' + num + ']_InputServiceMenuCode').style.display = "none";
    doc.getElementById('line[' + num + ']_InputPartsNumber').style.display = "none";
    doc.getElementById('line[' + num + ']_InputSetMenuCode').style.display = "none";
    doc.getElementById('line[' + num + ']_ServiceWorkCode').value = "";
    doc.getElementById('line[' + num + ']_ServiceMenuCode').value = "";
    doc.getElementById('line[' + num + ']_PartsNumber').value = "";
    doc.getElementById('line[' + num + ']_LineContents').value = "";
    doc.getElementById('line[' + num + ']_SetMenuCode').value = "";
    doc.getElementById('line[' + num + ']_SWCustomerClaimClass').value = "";

    // 名称
    doc.getElementById('line[' + num + ']_LineContents').style.width = "185px";     //Mod 2014/07/10 chrome対応 190px→185px
    doc.getElementById('line[' + num + ']_LineContents').readOnly = false;
    doc.getElementById('line[' + num + ']_LineContents').style.backgroundColor = "#FFFFFF";
    doc.getElementById('line[' + num + ']_LineContents').style.color = "#000000";

    // 連絡事項
    doc.getElementById('line[' + num + ']_RequestComment').style.display = "none";
    //Add 2014/06/17 arc yano 高速化対応
    doc.getElementById('line[' + num + ']_RequestComment').value = ""

    // 区分
    doc.getElementById('line[' + num + ']_InputWorkType').style.display = "none";
    doc.getElementById('line[' + num + ']_WorkType').selectedIndex = 0;

    // 工数
    doc.getElementById('line[' + num + ']_InputManPower').style.display = "none";
    doc.getElementById('line[' + num + ']_ManPower').value = "";

    // レバレート
    doc.getElementById('line[' + num + ']_InputLaborRate').style.display = "none";
    doc.getElementById('line[' + num + ']_LaborRate').value = "";

    // 技術料
    doc.getElementById('line[' + num + ']_InputTechnicalFeeAmount').style.display = "none";
    doc.getElementById('line[' + num + ']_TechnicalFeeAmount').value = "";

    // 在庫
    doc.getElementById('line[' + num + ']_PartsStock').style.display = "none";

    // 判断
    doc.getElementById('line[' + num + ']_InputStockStatus').style.display = "none";
    doc.getElementById('line[' + num + ']_StockStatus').selectedIndex = 0;

    // 発注書出力
    doc.getElementById('line[' + num + ']_InputOutputTargetFlag').style.display = "none";     //Add 2017/10/19 arc yano #3803
    doc.getElementById('line[' + num + ']_OutputTarget').checked = false;                     //Add 2017/10/19 arc yano #3803


    // 数量
    doc.getElementById('line[' + num + ']_InputQuantity').style.display = "none";
    doc.getElementById('line[' + num + ']_Quantity').value = "";

    // Add 2015/10/28 arc yano #3289
    // 引当済数
    doc.getElementById('line[' + num + ']_InputProvisionQuantity').style.display = "none";
    doc.getElementById('line[' + num + ']_ProvisionQuantity').value = "";

    // 発注数
    doc.getElementById('line[' + num + ']_InputOrderQuantity').style.display = "none";
    doc.getElementById('line[' + num + ']_OrderQuantity').value = "";


    // 単価
    doc.getElementById('line[' + num + ']_InputPrice').style.display = "none";
    doc.getElementById('line[' + num + ']_Price').value = "";

    // 金額
    doc.getElementById('line[' + num + ']_InputAmount').style.display = "none";
    doc.getElementById('line[' + num + ']_Amount').value = "";

    // 原価（単価）
    doc.getElementById('line[' + num + ']_InputUnitCost').style.display = "none";
    doc.getElementById('line[' + num + ']_UnitCost').value = "";

    // 原価（合計）
    doc.getElementById('line[' + num + ']_InputCost').style.display = "none";
    if (doc.getElementById('line[' + num + ']_Cost')) {
        doc.getElementById('line[' + num + ']_Cost').value = "";
    }

    // 消費税
    doc.getElementById('line[' + num + ']_TaxAmount').value = "";

    // 請求先
    doc.getElementById('line[' + num + ']_InputCustomerClaim').style.display = "none";

    // メカニック/外注先
    doc.getElementById('line[' + num + ']_InputSupplier').style.display = "none";
    doc.getElementById('line[' + num + ']_LineType').selectedIndex = 0;
    doc.getElementById('line[' + num + ']_SupplierCode').value = "";
    doc.getElementById('line[' + num + ']_SupplierName').value = "";
    doc.getElementById('line[' + num + ']_EmployeeCode').value = "";
    doc.getElementById('line[' + num + ']_EmployeeNumber').value = "";
    doc.getElementById('line[' + num + ']_EmployeeName').value = "";
    changeOutsourceEmployee(num);


    var serviceType = doc.getElementById('line[' + num + ']_ServiceType').value;
    switch (serviceType) {
        case "001": // 主作業
            doc.getElementById('line[' + num + ']_LineContents').readOnly = true;
            doc.getElementById('line[' + num + ']_LineContents').style.backgroundColor = "#EEEEEE";
            doc.getElementById('line[' + num + ']_LineContents').style.color = "#000000";          //Add 2014/06/13 arc yano
            //doc.getElementById('line[' + num + ']_WorkType').style.color = "#000000";            //Add 2014/06/13 arc yano //2016/03/17 arc yano #3471
            doc.getElementById('line[' + num + ']_InputServiceWorkCode').style.display = "inline";
            //doc.getElementById('line[' + num + ']_InputWorkType').style.display = "inline";       //2016/03/17 arc yano #3471
            doc.getElementById('line[' + num + ']_RequestComment').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputCustomerClaim').style.display = "inline";
            break;
        case "002": // サービスメニュー
            doc.getElementById('line[' + num + ']_LineContents').style.color = "#0000FF";
            doc.getElementById('line[' + num + ']_InputServiceMenuCode').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputWorkType').style.display = "inline";
            doc.getElementById('line[' + num + ']_WorkType').style.color = "#0000FF";
            doc.getElementById('line[' + num + ']_InputManPower').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputLaborRate').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputTechnicalFeeAmount').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputCost').style.display = "inline";
            // Mod 2015/06/15 arc nakayama #3206_サービス伝票の原価(合計)欄入力制御 ｻｰﾋﾞｽﾒﾆｭｰが再選択されたときはリードオンリー解除
            doc.getElementById('line[' + num + ']_Cost').readOnly = false;
            doc.getElementById('line[' + num + ']_Cost').style.backgroundColor = "#FFFFFF";
            doc.getElementById('line[' + num + ']_InputSupplier').style.display = "inline";
            doc.getElementById('line[' + num + ']_LaborRate').value = doc.getElementById('LaborRate').value;
            doc.getElementById('line[' + num + ']_RequestComment').style.display = "inline";                    //Add 2014/06/17 arc yano 高速化対応
            //Add 2016/03/17 arc yano #3471 
            GetWorkTypeList('line[' + num + ']_WorkType', serviceType); //不具合発生のため、一旦コメントアウト
            break;
        case "003": // 部品
            doc.getElementById('line[' + num + ']_LineContents').style.color = "#FF00FF";
            doc.getElementById('line[' + num + ']_InputPartsNumber').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputWorkType').style.display = "inline";
            doc.getElementById('line[' + num + ']_WorkType').style.color = "#FF00FF";
            doc.getElementById('line[' + num + ']_PartsStock').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputStockStatus').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputOutputTargetFlag').style.display = "inline";     //Add 2017/10/19 arc yano #3803
            doc.getElementById('line[' + num + ']_InputQuantity').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputPrice').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputAmount').style.display = "inline";
            doc.getElementById('line[' + num + ']_InputUnitCost').style.display = "inline";
            // Mod 2015/06/15 arc nakayama #3206_サービス伝票の原価(合計)欄入力制御 部品が選択されたときはリードオンリー
            doc.getElementById('line[' + num + ']_InputCost').style.display = "inline";
            doc.getElementById('line[' + num + ']_Cost').readOnly = true;
            doc.getElementById('line[' + num + ']_Cost').style.backgroundColor = "#EEEEEE";
            doc.getElementById('line[' + num + ']_RequestComment').style.display = "inline";    //Add 2014/06/17 arc yano 高速化対応

            doc.getElementById('line[' + num + ']_InputProvisionQuantity').style.display = "inline";    //Add 2015/11/05 arc yano #3289
            doc.getElementById('line[' + num + ']_ProvisionQuantity').value = 0;                        //Add 2015/11/05 arc yano #3289
            doc.getElementById('line[' + num + ']_InputOrderQuantity').style.display = "inline";        //Add 2015/11/05 arc yano #3289
            doc.getElementById('line[' + num + ']_OrderQuantity').value = 0;                            //Add 2015/11/05 arc yano #3289

            //Add 2016/03/17 arc yano #3471 
            GetWorkTypeList('line[' + num + ']_WorkType', serviceType);   //不具合発生のため、一旦コメントアウト

            break;
        case "004": // コメント
            doc.getElementById('line[' + num + ']_LineContents').style.width = "323px";
            doc.getElementById('line[' + num + ']_LineContents').style.color = "#006600";   //Add 2014/06/13 arc yano サービスタイプをコメント変更時には、文字色を深緑色に設定する。
            doc.getElementById('line[' + num + ']_RequestComment').style.display = "inline";    //Add 2014/06/17 arc yano 高速化対応
            break;
        case "005": // セットメニュー
            doc.getElementById('line[' + num + ']_InputSetMenuCode').style.display = "inline";
            break;

    }
    calcTotalServiceAmount();
}


//選択されたラジオボタンの値を取得する
function radioValue(element) {
    var len;
    len = element.length;
    for (i = 0; i < len; i++) {
        if (element[i].checked) return element[i].value;
    }
    return "";
}

// 車両発注依頼引当画面の全チェック機能
function checkAll(num) {
    var element = document.getElementById('CheckFlag');
    if (element) {
        if (element.value == '0') {
            changeCheck(1);
            element.value = '1';
        } else {
            changeCheck(0);
            element.value = '0';
        }
    }
    function changeCheck(flag) {
        for (var i = 0; i < num; i++) {
            if (flag == 1) {
                document.getElementById('check[' + i + ']').checked = true;
            } else {
                document.getElementById('check[' + i + ']').checked = false;
            }
        }
    }
}

//車両発注依頼引当の入力画面表示
function openCarPurchaseOrderDialog(num) {
    var url = "/CarPurchaseOrder/ListEntry/?OrderId=";
    var query = "";
    for (var i = 0; i < num; i++) {
        var flg = document.getElementById('check[' + i + ']').checked;
        if (flg == true) {
            query = query + document.getElementById('CarPurchaseOrderId[' + i + ']').value + ",";
        }
    }
    if (query == "") {
        alert('１つ以上チェックする必要があります');
        return;
    }
    openModalAfterRefresh(url + query.substring(query, query.length - 1));
}

//部品入荷一括入力画面表示
function openPartsPurchaseOrderDialog(num) {
    var url = "/PartsPurchaseOrder/ListEntry/?OrderId=";
    var query = "";
    for (var i = 0; i < num; i++) {
        var flg = document.getElementById('check[' + i + ']').checked;
        if (flg) {
            query = query + document.getElementById('PurchaseOrderNumber[' + i + ']').value + ",";
        }
    }
    if (query == "") {
        alert('１つ以上チェックする必要があります');
        return;
    }
    openModalAfterRefresh(url + query.substring(query, query.length - 1));
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 部品入荷一括入力画面表示                                                         */
/* 作成日：2015/12/01 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善)  */
/* 更新日：                                                                                  */
/*         2016/06/20 arc yano #3585 部品入荷一覧　引数追加(PurchaseStatus)                  */
/*　　　　 2016/03/15 arc yano #3472 部品入荷一覧  チェックした項目の編集 はサーバで行う     */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
//Add 
//部品入荷一括入力画面表示
function openPartsPurchaseDialog(num, PurchaseNumber, PurchaseOrderNumber, PartsNumber, OpenType, PurchaseStatus, Form) {

    var cnt = 0;

    switch (OpenType)
    {
        case 1:
            //新規作成
            var url = "/PartsPurchase/Entry/?keyList[" + cnt + "].PurchaseNumber=" + "" + "&keyList[" + cnt + "].PurchaseOrderNumber=" + "" + "&keyList[" + cnt + "].PartsNumber=" + "";
            openModalAfterRefresh(url);
            break;

        case 2:

            //入荷伝票番号クリックの場合
            var url = "/PartsPurchase/Entry/?keyList[" + cnt + "].PurchaseNumber=" + PurchaseNumber + "&keyList[" + cnt + "].PurchaseOrderNumber=" + PurchaseOrderNumber + "&keyList[" + cnt + "].PartsNumber=" + PartsNumber + "&PurchaseStatus=" + PurchaseStatus;

            openModalAfterRefresh(url);
            break;

        case 3:     //Mod 2016/03/15 arc yano #3472
            /*
            //チェックした項目の編集
            var url = "/PartsPurchase/Entry/?";
            var query = "";
            for (var i = 0; i < num; i++) {
                var flg = document.getElementById('check[' + i + ']').checked;
                if (flg) {
                    query = query + "keyList[" + cnt + "].PurchaseNumber=" + document.getElementById('PurchaseNumber[' + i + ']').value + "&keyList[" + cnt + "].PurchaseOrderNumber=" + document.getElementById('PurchaseOrderNumber[' + i + ']').value + "&keyList[" + cnt + "].PartsNumber=" + document.getElementById('PartsNumber[' + i + ']').value + "&";
                    cnt++;
                }
            }
            
            //ループが終了したら最後の「&」を削除
            query = query.substr(0, (query.length - 1));

            if (query == "") {
                alert('１つ以上チェックする必要があります');
                return;
            }
            */

            var url = '';
            var query = '';

            //チェック処理
            for (var i = 0; i < num; i++) {
                var flg = document.getElementById('check[' + i + ']').checked;
                if (flg) {
                    query = query + "keyList[" + cnt + "].PurchaseNumber=" + document.getElementById('PurchaseNumber[' + i + ']').value + "&keyList[" + cnt + "].PurchaseOrderNumber=" + document.getElementById('PurchaseOrderNumber[' + i + ']').value + "&keyList[" + cnt + "].PartsNumber=" + document.getElementById('PartsNumber[' + i + ']').value + "&";
                    cnt++;
                }
            }
            if (query == '') {
                alert('１つ以上チェックする必要があります');
                return;
            }

            //入荷ステータス追加
            query = query + "PurchaseStatus =" + PurchaseStatus;    //Mod 2016/06/20 arc yano #3585

            //画面呼び出し
            if (typeof (Form) !== 'undefined') {
                Form.target = 'test';
                Form.action = '/PartsPurchase/EditCheckedItemList';

                //サイズ指定
                var windowSize = new WindowSize('/PartsPurchase/Entry/?');
                var width = windowSize.width;
                var height = windowSize.height;

                openModalAfterRefresh('', Form.target, width, height);
                Form.submit();
                Form.target = '_self';
                Form.action = '/PartsPurchase/Criteria';
            }
            break;

        case 4:
            //発注伝票番号クリック
            var url = "/PartsPurchase/Entry/?keyList[" + cnt + "].PurchaseNumber=" + PurchaseNumber + "&keyList[" + cnt + "].PurchaseOrderNumber=" + PurchaseOrderNumber + "&keyList[" + cnt + "].PartsNumber=" + PartsNumber + "&OrderNumberClick=true" + "&PurchaseOrderNumber=" + PurchaseOrderNumber;
            openModalAfterRefresh(url);
            break;

        default:
            break;
    }
}
//車両発注依頼引当検索画面での月内チェック処理
function CheckCurrentMonth() {

    var checkBox = document.getElementById('CurrentMonth');
    var fromElement = document.getElementById('RegistrationPlanDateFrom');
    var toElement = document.getElementById('RegistrationPlanDateTo');

    if (!checkBox.checked) {
        fromElement.value = '';
        toElement.value = '';
        fromElement.readOnly = false;
        toElement.readOnly = false;
        fromElement.style.backgroundColor = "#ffffff";
        toElement.style.backgroundColor = "#ffffff";
        return;
    }

    var now = new Date();
    var year = now.getYear(); // 年
    var month = now.getMonth() + 1; // 月
    var endOfMonthDate = new Date(year, month, 0);
    var startOfMonthDate = new Date(year, month, 1);

    var endOfMonthYear = endOfMonthDate.getFullYear();
    var endOfMonth = endOfMonthDate.getMonth() + 1;
    var endOfMonthDay = endOfMonthDate.getDate();

    var startOfMonthYear = startOfMonthDate.getFullYear();
    var startOfMonth = startOfMonthDate.getMonth();
    var startOfMonthDay = startOfMonthDate.getDate();

    fromElement.value = startOfMonthYear + "/" + startOfMonth + "/" + startOfMonthDay;
    toElement.value = endOfMonthYear + "/" + endOfMonth + "/" + endOfMonthDay;
    fromElement.readOnly = true;
    toElement.readOnly = true;
    fromElement.style.backgroundColor = "#e0e0e0";
    toElement.style.backgroundColor = "#e0e0e0";
}

//店舗入金消込の請求書印刷処理
//-------------------------------------------------------------------------------------
// Mod 2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正
//-------------------------------------------------------------------------------------
function printInvoice(num) {
    var url = "/Deposit/PrintInvoice?PlanId=";
    var query = "";
    for (var i = 0; i < num; i++) {
        //var flg = document.getElementById('check[' + i + ']').checked;
        var flg = document.getElementById('line[' + i + ']_Changetarget').checked;
        if (flg == true) {
            query = query + document.getElementById('line[' + i + ']_ReceiptPlanId').value + ",";
        }
    }
    if (query == "") {
        alert('１つ以上チェックする必要があります');
        return;
    }
    window.open(url + query.substring(query, query.length - 1), 'report', 'width=1024,height=768');

}

//セットメニュー検索ダイアログを表示し、
//選択された時だけセットメニュー追加実行する
//-------------------------------------------------------------------------------------------
// Edit 2014/07/07 arc yano chrome対応 getElementbyIdのパラメータ変更(name値→id値)
//-------------------------------------------------------------------------------------------
function openSetMenu(num) {

    // Mod 2021/03/26 sola owashi Chrome対応
    // IEでも動くように修正
    // 修正前
    // // Mod 2021/02/15 sola owashi Chrome対応
    // // 処理をコールバック関数化
    // // 修正前
    // // ret = openSearchDialog('line[' + num + ']_SetMenuCode', 'line[' + num + ']_LineContents', '/SetMenu/CriteriaDialog');
    // // if (ret) {

    // //     //Del 2014/07/07 arc yano　サービス伝票高速化・不具合対応
    // //     /*
    // //     //Add 2014/06/11 arc yano 高速化対応
    // //     var obj = document.getElementById('line[' + num + ']_SetMenuCode');
    // //     var curtr = obj.parentNode.parentNode.parentNode;
    // //     var pretr = curtr.previousSibling;
    // //     var count = 0;
    // //     while (pretr) {
    // //         if (pretr.nodeName == 'TR') {
    // //             count++;
    // //         }
    // //         pretr = pretr.previousSibling;    
    // //     }
    // //     */
    // //     formList(); //明細行データの調整

    // //     //Add 2014/07/07 arc yano　サービス伝票高速化・不具合対応
    // //     //振り直し後のlineNumberを取得
    // //     var obj = document.getElementById('line[' + num + ']_LineNumber');
    // //     document.forms[0].CurrentLineNumber.value = parseInt(obj.value) - 1;

    // //     //document.forms[0].CurrentLineNumber.value = count;
    // //     //document.forms[0].CurrentLineNumber.value = num;
    // //     document.forms[0].action = '/ServiceSalesOrder/AddSetMenu';
    // //     formSubmit();
    // // }
    // // 修正後：開始---------------
    // let callback = (ret) => {
    //     if (ret) {
    //         formList(); //明細行データの調整

    //         //振り直し後のlineNumberを取得
    //         var obj = document.getElementById('line[' + num + ']_LineNumber');
    //         document.forms[0].CurrentLineNumber.value = parseInt(obj.value) - 1;

    //         document.forms[0].action = '/ServiceSalesOrder/AddSetMenu';
    //         formSubmit();
    //     }        
    // }
    // openSearchDialog(
    //     'line[' + num + ']_SetMenuCode', 'line[' + num + ']_LineContents', '/SetMenu/CriteriaDialog'
    //     , undefined, undefined, undefined, undefined, callback);
    // // 修正後：終了---------------

    // 修正後：開始---------------
    var userAgent = window.navigator.userAgent.toLowerCase();
    if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {
        // IEの場合
        ret = openSearchDialog('line[' + num + ']_SetMenuCode', 'line[' + num + ']_LineContents', '/SetMenu/CriteriaDialog');
        if (ret) {
            formList(); //明細行データの調整
    
            //振り直し後のlineNumberを取得
            var obj = document.getElementById('line[' + num + ']_LineNumber');
            document.forms[0].CurrentLineNumber.value = parseInt(obj.value) - 1;
    
            document.forms[0].action = '/ServiceSalesOrder/AddSetMenu';
            formSubmit();
        }
    } else {
        // IE以外の場合
        var callback = function(ret){
            if (ret) {
                formList(); //明細行データの調整
    
                //振り直し後のlineNumberを取得
                var obj = document.getElementById('line[' + num + ']_LineNumber');
                document.forms[0].CurrentLineNumber.value = parseInt(obj.value) - 1;
    
                document.forms[0].action = '/ServiceSalesOrder/AddSetMenu';
                formSubmit();
            }
        }
        openSearchDialog(
            'line[' + num + ']_SetMenuCode', 'line[' + num + ']_LineContents', '/SetMenu/CriteriaDialog'
            , undefined, undefined, undefined, undefined, callback);
    }
    // 修正後：終了---------------
}
//********************************
//　ウインドウ表示関数
//********************************


//Add 2015/10/28 arc yano #3289 サービス伝票 発注情報の作成方法の変更 ウィンドウの表示位置を指定する
/*-------------------------------------------------------------------------------------------*/
/* 機能　： モードレスウインドウの表示                                                       */
/* 作成日：????/??/?? ???                                                                    */
/* 更新日：2016/03/15 arc yano #3472 部品入荷一覧　チェックした項目の編集                    */
/*                                                 targetが指定していない場合は_blank指定    */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/

function openModalDialog(url, target, width, height, status, scroll, top, left) {
    if (!width || !height) {
        size = new WindowSize(url);
        width = size.width;
        height = size.height;
    }

    var w_left;
    var w_top;

    var targetwindow;

    if (typeof (left) === 'undefined') {   //leftの引数が指定されていない場合
        w_left = (window.screen.width - width) / 2
    }
    else {
        w_left = (window.screen.width - width) / 2 + left;
    }
    if (typeof (top) === 'undefined') {   //topの引数が指定されていない場合
        w_top = 0;
    }
    else {
        w_top = (window.screen.height - height) / 2 + top;
    }

    //Mod 2016/03/15 arc yano #3472 //Mod 2016/04/21 arc yano #3496
    if (typeof (target) === 'undefined' || target == null) {
        targetwindow = '_blank'
    }
    else {
        targetwindow = target;
    }
    
    //var w_left = (window.screen.width - width) / 2;
    //var w_top = (window.screen.height - height) / 2;
    window.open(url, targetwindow, 'width=' + width + ',height=' + height + ',left=' + w_left + ',top=' + w_top + ',menubar=no,toolbar=no,resizable=yes,scrollbars=yes');

    //openModalDialog2(url, width, height, status, scroll);
}


//-------------------------------------------------------------------------------------------
// 機能　： modalwindowの表示（ダイアログ上で保存処理を行う場合）                                                       
// 作成日： 2022/07/28 yano #4146 【サービス伝票】マスタの修正内容がサービス伝票に反映されない
// 更新日：                    
//    
//                                                                                           
//-------------------------------------------------------------------------------------------
//モーダル子画面の表示
function openModalDialogNotMask(url, width, height, status, scroll,callback) {
   
    var childWindow = openModalDialog2(url, width, height, status, scroll);

    var intervlId = setInterval(
        function(){
            if(childWindow.closed){
                clearInterval(intervlId);
                intervlId = null;

                if (callback != undefined) {
                    // 引数でコールバック関数が指定されている場合は呼び出す
                    //callback.bind(null, ret)();
                    callback.bind(null, null)();
                }
            }
        }
        ,300
     );

    //childWindow.addEventListener('load', function (event) {
        
    //    childWindow.addEventListener('unload', function (event) {
    //        var ret = childWindow.returnValue;

    //        if (ret != null) {
    //            document.getElementById(codeItem).value = ret[0];
    //            if (nameItem != '') {
    //                if ((document.getElementById(nameItem).tagName == 'INPUT') || (document.getElementById(nameItem).tagName == 'input')) { //input項目の場合
    //                    document.getElementById(nameItem).value = ret[1];
    //                }
    //                else {  //それ以外
    //                    document.getElementById(nameItem).innerText = decodeHtml(ret[1]);
    //                }

    //            }
    //            if (document.getElementById(codeItem).type != "hidden") {
    //                document.getElementById(codeItem).focus();
    //            }

    //            ret = true;
    //        }

    //        if (callback != undefined) {
    //            // 引数でコールバック関数が指定されている場合は呼び出す
    //            callback.bind(null, ret)();
    //        }
    //    }, false);
    // }, false);
}


//モーダル子画面の表示
function openModalDialog2(url, width, height, status, scroll) {
    var opt = "";
    if (!width || !height) {
        size = new WindowSize(url);
        width = size.width;
        height = size.height;
    }

    // 修正後：開始---------------
    var userAgent = window.navigator.userAgent.toLowerCase();
    if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {
        // IEの場合
        opt += "dialogWidth:" + width + "px;";
        opt += "dialogHeight:" + height + "px;";
        if (!status) {
            opt += "status:no;";
        } else {
            opt += "status:" + status + ";";
        }
        if (!scroll) {
            opt += "scroll:yes;";
        } else {
            opt += "scroll:" + scroll + ";";
        }
        return showModalDialog(url, this, opt);

    } else  {
        // IE以外の場合
        var left = ( screen.availWidth - width ) / 2;
        var top = ( screen.availHeight - height ) / 2;
    
        opt += "left=" + left + ",";
        opt += "top=" + top + ",";
    
        opt += "width=" + width + ",";
        opt += "height=" + height + ",";
        if (!status) {
            opt += "status=no,";
        } else {
            opt += "status=" + status + ",";
        }
        if (!scroll) {
            opt += "scrollbars=yes";
        } else {
            opt += "scrollbars=" + scroll;
        }
        var win = window.open(url, "_blank", opt);
    
        return win;
    }
    // 修正後：終了---------------
}
//Add 2015/10/28 arc yano #3289 サービス伝票 発注情報の作成方法の変更 ウィンドウの表示位置(top, left)を指定する
/*-------------------------------------------------------------------------------------------*/
/* 機能　：モーダル子画面の表示(終了後親画面サブミット)                                      */
/* 作成日：????/??/?? ???                                                                    */
/* 更新日：2016/03/15 arc yano #3472 部品入荷一覧　チェックした項目の編集 引数追加(target)   */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function openModalAfterRefresh(url, target, width, height, status, scroll, top, left) {
    /*if (!width || !height) {
    size = new WindowSize(url);
    width = size.width;
    height = size.height;
    }
    var ret = openModalDialog(url, width, height, status, scroll);
    while ((typeof ret != 'undefined') && (typeof ret.url != 'undefined')) {
    ret = openModalDialog(ret.url);
    }
    formSubmit();
    */
    openModalDialog(url, target, width, height, status, scroll, top, left);
}

//モーダル子画面の表示(終了後親画面サブミット)
// Mod 2014/07/24 arc yano chrome対応 モーダルダイアログを開いている間は、その他のウィンドウの操作を受け付けなくする。
function openModalAfterRefresh2(url, width, height, status, scroll) {
    //$('#mask').css('display', 'block');

    var widthS = 0;
    var heightS = 0;

    widthS = window.screen.width;
    heightS = window.screen.height;


    // スクロールを含む幅と高さを取得する（ブラウザのモードによって取得が違う為、分岐させる）
    if (document.compatMode == "CSS1Compat") {

        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.documentElement.scrollWidth > window.screen.width) {
            widthS = document.documentElement.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.documentElement.scrollHeight > window.screen.height) {
            heightS = document.documentElement.scrollHeight;
        }
    } else {
        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.body.scrollWidth > window.screen.width) {
            widthS = document.body.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.body.scrollHeight > window.screen.height) {
            heightS = document.body.scrollHeight;
        }
    }
    
    // 取得した幅と高さ分、親画面にマスクをかける
    $('#mask').css({
        width: widthS,
        height: heightS,
        display: 'block'
    });
    
    if (parent.HeaderFrame != null) {//ヘッダ画面ありの場合
        $('#mask', parent.HeaderFrame.document).css('display', 'block');
    }

    if (parent.MenuFrame != null) {//メニュー画面ありの場合
        $('#mask', parent.MenuFrame.document).css('display', 'block');
    }

    if (!width || !height) {
        size = new WindowSize(url);
        width = size.width;
        height = size.height;
    }

    // Mod 2021/03/26 sola owashi Chrome対応
    // IEでも動くように修正
    // 修正前
    // // Mod 2021/02/14 sola owashi Chrome対応
    // // 修正前------------
    // /*
    // var ret = openModalDialog2(url, width, height, status, scroll);
    // while ((typeof ret != 'undefined') && (typeof ret.url != 'undefined')) {
    //     ret = openModalDialog2(ret.url);
    // }
    // formSubmit();

    // $('#mask').css('display', 'none');

    // //ヘッダ画面ありの場合
    // if (parent.HeaderFrame != null) {
    //     $('#mask', parent.HeaderFrame.document).css('display', 'none');
    // }
    // if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //     $("#mask", parent.MenuFrame.document).css('display', 'none');
    // }
    // */
    // // 修正後：開始------------
    // function openModalDialog2AndAddListener(isFirstCall, url, width, height, status, scroll){
    //     var childWindow = openModalDialog2(url, width, height, status, scroll);

    //     let loadListener = function(event){
    //         let unloadListener = function(event){
    //             var ret = childWindow.returnValue;

    //             var checkWindowClose = ()=>{
    //                 if(!childWindow.window.closed){
    //                     // 子画面が閉じていない場合（子画面でのsubmit時にエラーが発生した場合）
    //                     childWindow.addEventListener('unload', unloadListener, false);

    //                 } else {
    //                     while ((typeof ret != 'undefined') && (typeof ret.url != 'undefined')) {
    //                         // 再帰呼び出し
    //                         openModalDialog2AndAddListener(false, ret.url);
    //                     }
        
    //                     if(isFirstCall){
    //                         // 最初の呼び出しの場合のみ、画面操作を行なう（再帰呼び出しの場合は行わない）
    //                         formSubmit();
            
    //                         $('#mask').css('display', 'none');
                        
    //                         //ヘッダ画面ありの場合
    //                         if (parent.HeaderFrame != null) {
    //                             $('#mask', parent.HeaderFrame.document).css('display', 'none');
    //                         }
    //                         if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //                             $("#mask", parent.MenuFrame.document).css('display', 'none');
    //                         }
    //                     }
    //                 }
    //             }
    //             setTimeout(checkWindowClose, 100);
    //         }
    //         childWindow.addEventListener('unload', unloadListener, false);
    //     }
    //     childWindow.addEventListener('load', loadListener, false);
    // };

    // openModalDialog2AndAddListener(true, url, width, height, status, scroll);
    // // 修正後：終了------------

    // 修正後：開始---------------
    var userAgent = window.navigator.userAgent.toLowerCase();
    if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {
        // IEの場合
        var ret = openModalDialog2(url, width, height, status, scroll);
        while ((typeof ret != 'undefined') && (typeof ret.url != 'undefined')) {
            ret = openModalDialog2(ret.url);
        }
        formSubmit();
    
        $('#mask').css('display', 'none');
    
        //ヘッダ画面ありの場合
        if (parent.HeaderFrame != null) {
            $('#mask', parent.HeaderFrame.document).css('display', 'none');
        }
        if (parent.MenuFrame != null) {//メニュー画面ありの場合
            $("#mask", parent.MenuFrame.document).css('display', 'none');
        }
    } else  {
        // IE以外の場合
        function openModalDialog2AndAddListener(isFirstCall, url, width, height, status, scroll){
            var childWindow = openModalDialog2(url, width, height, status, scroll);
    
            var loadListener = function(event){
                var unloadListener = function(event){
                    var ret = childWindow.returnValue;
    
                    //var checkWindowClose = ()=>{
                    var checkWindowClose = function(){
                        if(!childWindow.window.closed){
                            // 子画面が閉じていない場合（子画面でのsubmit時にエラーが発生した場合）
                            childWindow.addEventListener('unload', unloadListener, false);
    
                        } else {
                            while ((typeof ret != 'undefined') && (typeof ret.url != 'undefined')) {
                                // 再帰呼び出し
                                openModalDialog2AndAddListener(false, ret.url);
                            }
            
                            if(isFirstCall){
                                // 最初の呼び出しの場合のみ、画面操作を行なう（再帰呼び出しの場合は行わない）
                                formSubmit();
                
                                $('#mask').css('display', 'none');
                            
                                //ヘッダ画面ありの場合
                                if (parent.HeaderFrame != null) {
                                    $('#mask', parent.HeaderFrame.document).css('display', 'none');
                                }
                                if (parent.MenuFrame != null) {//メニュー画面ありの場合
                                    $("#mask", parent.MenuFrame.document).css('display', 'none');
                                }
                            }
                        }
                    };
                    setTimeout(checkWindowClose, 100);
                }
                childWindow.addEventListener('unload', unloadListener, false);
            }
            childWindow.addEventListener('load', loadListener, false);
        };
    
        openModalDialog2AndAddListener(true, url, width, height, status, scroll);
    }
    // 修正後：終了---------------
}

// 検索ダイアログの表示
// Mod 2014/09/03 arc amii 親画面ロック対応 画面のサイズ(スクロール含む)を取得し、そのサイズでmaskをかける。
// Mod 2014/07/24 arc yano chrome対応 モーダルダイアログを開いている間は、その他のウィンドウの操作を受け付けなくする。
// Mod 2014/07/14 arc yano chrome対応 直接検索ダイアログを呼ばずに、親フレームを呼ぶ。
// Mod 2014/07/02 arc yano chrome対応 nameの設定方法をタグの種類により使い分ける。
// Mod 2021/02/15 sola owashi chrome対応 ShowModalDialog廃止対応、関数定義にcallbackパラメータを追加、戻り値廃止
//function openSearchDialog(codeItem, nameItem, url, width, height, status, scroll) {
function openSearchDialog(codeItem, nameItem, url, width, height, status, scroll, callback) {

    // Mod 2021/02/15 sola owashi Chrome対応
    // この関数の呼び出し時に子画面からの戻り値を返せなくなったため戻り値を廃止（コールバック関数に処理を移行）
    //var ret = false;

    var widthS = 0;
    var heightS = 0;

    widthS = window.screen.width;
    heightS = window.screen.height;


    // スクロールを含む幅と高さを取得する（ブラウザのモードによって取得が違う為、分岐させる）
    if (document.compatMode == "CSS1Compat") {

        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.documentElement.scrollWidth > window.screen.width) {
            widthS = document.documentElement.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.documentElement.scrollHeight > window.screen.height) {
            heightS = document.documentElement.scrollHeight;
        }
    } else {
        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.body.scrollWidth > window.screen.width) {
            widthS = document.body.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.body.scrollHeight > window.screen.height) {
            heightS = document.body.scrollHeight;
        }
    }
    
    // 設定したサイズ分、親画面にマスクをかける
    $('#mask').css({
        width: widthS,
        height: heightS,
        display:'block'
    });

    if (parent.HeaderFrame != null) {//ヘッダ画面ありの場合
        $('#mask', parent.HeaderFrame.document).css('display', 'block');
    }

    if (parent.MenuFrame != null) {//メニュー画面ありの場合
        $('#mask', parent.MenuFrame.document).css('display', 'block');
    }

    if (!width || !height) {
        size = new WindowSize(url);
        width = size.width;
        height = size.height;
    }


    //urlの調整
    url = url.replace('?', '&');
    //インラインフレームの幅の調整
    var modwidth = width - 16;
    url = "/IFrame/IFrame?url=" + url + "&width=" + modwidth + "&height=" + height;

    // Mod 2021/03/26 sola owashi Chrome対応
    // IEでも動くように修正
    // 修正前
    // // Mod 2021/02/15 sola owashi Chrome対応
    // // 修正前------------
    // /*
    // var ret = openModalDialog2(url, width, height, status, scroll);
    // if (ret != null) {
    //     document.getElementById(codeItem).value = ret[0];
    //     if (nameItem != '') {
    //         if ((document.getElementById(nameItem).tagName == 'INPUT') || (document.getElementById(nameItem).tagName == 'input')){ //input項目の場合
    //             document.getElementById(nameItem).value = ret[1];
    //         }
    //         else {  //それ以外
    //             document.getElementById(nameItem).innerText = decodeHtml(ret[1]);
    //         }

    //     }
    //     if (document.getElementById(codeItem).type != "hidden") {
    //         document.getElementById(codeItem).focus();
    //     }
    //     //return true;
    //     ret = true;
    // }

    // $('#mask').css('display', 'none');

    // //ヘッダ画面ありの場合
    // if (parent.HeaderFrame != null) {
    //     $('#mask', parent.HeaderFrame.document).css('display', 'none');
    // }
    // if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //     $("#mask", parent.MenuFrame.document).css('display', 'none');
    // }

    // //return false;

    // return ret;
    // */
    // // 修正後：開始------------
    // var childWindow = openModalDialog2(url, width, height, status, scroll);

    // childWindow.addEventListener('load',function(event){
    //     childWindow.addEventListener('unload', event => {
    //         var ret = childWindow.returnValue;

    //         if (ret != null) {
    //             document.getElementById(codeItem).value = ret[0];
    //             if (nameItem != '') {
    //                 if ((document.getElementById(nameItem).tagName == 'INPUT') || (document.getElementById(nameItem).tagName == 'input')){ //input項目の場合
    //                     document.getElementById(nameItem).value = ret[1];
    //                 }
    //                 else {  //それ以外
    //                     document.getElementById(nameItem).innerText = decodeHtml(ret[1]);
    //                 }
        
    //             }
    //             if (document.getElementById(codeItem).type != "hidden") {
    //                 document.getElementById(codeItem).focus();
    //             }
                
    //             ret = true;
    //         }
    
    //         $('#mask').css('display', 'none');
    
    //         //ヘッダ画面ありの場合
    //         if (parent.HeaderFrame != null) {
    //             $('#mask', parent.HeaderFrame.document).css('display', 'none');
    //         }
    //         if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //             $("#mask", parent.MenuFrame.document).css('display', 'none');
    //         }
            
    //         if(callback != undefined){
    //             // 引数でコールバック関数が指定されている場合は呼び出す
    //             callback.bind(null, ret)();
    //         }
    //     }, false);
    // }, false);
    // // 修正後：終了------------

    // 修正後：開始---------------
    var userAgent = window.navigator.userAgent.toLowerCase();
    if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {
        // IEの場合
        var ret = openModalDialog2(url, width, height, status, scroll);
        if (ret != null) {
            document.getElementById(codeItem).value = ret[0];
            if (nameItem != '') {
                if ((document.getElementById(nameItem).tagName == 'INPUT') || (document.getElementById(nameItem).tagName == 'input')){ //input項目の場合
                    document.getElementById(nameItem).value = ret[1];
                }
                else {  //それ以外
                    document.getElementById(nameItem).innerText = decodeHtml(ret[1]);
                }
    
            }
            if (document.getElementById(codeItem).type != "hidden") {
                document.getElementById(codeItem).focus();
            }
            //return true;
            ret = true;
        }
    
        $('#mask').css('display', 'none');
    
        //ヘッダ画面ありの場合
        if (parent.HeaderFrame != null) {
            $('#mask', parent.HeaderFrame.document).css('display', 'none');
        }
        if (parent.MenuFrame != null) {//メニュー画面ありの場合
            $("#mask", parent.MenuFrame.document).css('display', 'none');
        }
    
        //return false;
    
        return ret;
    } else {

        // IE以外の場合
        var childWindow = openModalDialog2(url, width, height, status, scroll);

        childWindow.addEventListener('load', function(event){
            //childWindow.addEventListener('unload', event => {
            childWindow.addEventListener('unload', function(event){
                var ret = childWindow.returnValue;
    
                if (ret != null) {
                    document.getElementById(codeItem).value = ret[0];
                    if (nameItem != '') {
                        if ((document.getElementById(nameItem).tagName == 'INPUT') || (document.getElementById(nameItem).tagName == 'input')){ //input項目の場合
                            document.getElementById(nameItem).value = ret[1];
                        }
                        else {  //それ以外
                            document.getElementById(nameItem).innerText = decodeHtml(ret[1]);
                        }
            
                    }
                    if (document.getElementById(codeItem).type != "hidden") {
                        document.getElementById(codeItem).focus();
                    }
                    
                    ret = true;
                }
        
                $('#mask').css('display', 'none');
        
                //ヘッダ画面ありの場合
                if (parent.HeaderFrame != null) {
                    $('#mask', parent.HeaderFrame.document).css('display', 'none');
                }
                if (parent.MenuFrame != null) {//メニュー画面ありの場合
                    $("#mask", parent.MenuFrame.document).css('display', 'none');
                }
   
                if(callback != undefined){
                    // 引数でコールバック関数が指定されている場合は呼び出す

                    //Mod 2022/08/03 yano #4069
                    var intervalId = setInterval(
                        function () {
                            if (childWindow.closed)
                            {
                                clearInterval(intervalId);
                                intervalId = null;
                                callback.bind(null, ret)();
                            }
                        }, 100);

                    //setTimeout(function () { callback.bind(null, ret)(); }, 300);

                    //callback.bind(null, ret)();
                }
            }, false);
        }, false);
    }
    // 修正後：終了---------------
}

// クローズチェック＆帳票印刷(子画面用)
function closeCheck() {
    if ((document.forms[0]) && (document.forms[0].close)) {
        if ((document.forms[0].url) && document.forms[0].url.value != '') {
            var url = document.forms[0].url.value;
            openModalDialog(url);
        }
        printReport();
        if (document.forms[0].close.value == '1') {
            window.close();
        }
    }
}

//------------------------------------------------------------------------------------------------
// 機能　： 帳票を印刷します                                         
// 作成日： ????/??/??
// 更新日：
//          2021/03/22 yano #4078【サービス伝票入力】納車確認書で出力する帳票の種類を動的に絞る
//          2017/01/21 arc yano #3657 見積書の個人情報を表示／非表示の引数を設定                                                         
//------------------------------------------------------------------------------------------------
function printReport() {
    if ((document.forms[0].reportName) && (document.forms[0].reportParam) &&
            (document.forms[0].reportName.value != '')) {
        var width = 800;
        var height = 600;
        var w_left = (window.screen.width - width) / 2;
        var w_top = (window.screen.height - height) / 2;


        //Mod 2017/01/21 arc yano #3657 
        var strurl = '/Report/Print?reportName=' + document.forms[0].reportName.value + '&reportParam=' + document.forms[0].reportParam.value;

        //個人情報表示のチェックボックスが設定されていた場合
        if (document.getElementById('DispPersonalInfo') != null) {
            strurl += '&dispPersonalInfo=' + document.getElementById('DispPersonalInfo').checked;
        }

        //Add 2021/03/22 yano #4078
        //納品請求書出力のチェックボックスが存在する場合
        if (document.getElementById('ClaimReportOutPut') != null && document.getElementById('ClaimReportOutPut') != 'undefined') {
            strurl += '&claimReportOutPut=' + document.getElementById('ClaimReportOutPut').checked;
        }


        window.open(strurl, 'report', 'width=' + width + ',height=' + height + ',top=0,left=' + w_left + ',resizable=yes');
    }
}
//********************************
//　Ajax系
//********************************



//アコーディオンメニュー処理用関数
var accordion = function() {
    var tm = sp = 5;
    function slider(n) { this.nm = n; this.arr = [] }
    slider.prototype.init = function(t, c, k) {
        var a, h, s, l, i; a = document.getElementById(t); this.sl = k ? k : '';
        h = a.getElementsByTagName('dt'); s = a.getElementsByTagName('dd'); this.l = h.length;
        for (i = 0; i < this.l; i++) { var d = h[i]; this.arr[i] = d; d.onclick = new Function(this.nm + '.pro(this)'); if (c == i) { d.className = this.sl } }
        l = s.length;
        for (i = 0; i < l; i++) { d = s[i]; d.mh = d.offsetHeight; if (c != i) { d.style.height = 0; d.style.display = 'none' } }
    }
    slider.prototype.pro = function(d) {
        for (var i = 0; i < this.l; i++) {
            var h = this.arr[i], s = h.nextSibling; s = s.nodeType != 1 ? s.nextSibling : s; clearInterval(s.tm);
            if (h == d && s.style.display == 'none') { s.style.display = ''; su(s, 1); h.className = this.sl }
            else if (s.style.display == '') { su(s, -1); h.className = '' }
        }
    }
    function su(c, f) { c.tm = setInterval(function() { sl(c, f) }, tm) }
    function sl(c, f) {
        var h = c.offsetHeight, m = c.mh, d = f == 1 ? m - h : h; c.style.height = h + (Math.ceil(d / sp) * f) + 'px';
        c.style.opacity = h / m; c.style.filter = 'alpha(opacity=' + h * 100 / m + ')';
        // Mod 2014/07/03 arc amii chrome対応 判定条件修正 (h == 1 を　 h <= 6)
        if (f == 1 && h >= m) { clearInterval(c.tm) } else if (f != 1 &&h <= 6) { c.style.display = 'none'; clearInterval(c.tm) }
    }
    return { slider: slider }
} ();

// ---------------------------------
//	文字列を通貨型に変更する
// ---------------------------------
function currency(n) {
    var result;
    var str = "" + n;
    var ary;
    if (!(ary = str.match(/^([\+-]|)(\d+)(\.\d+|)$/)))
        return "";
    var int_part = ary[2];
    var len = int_part.length;
    var mod = (len - 1) % 3 + 1;
    result = int_part.substr(0, mod);
    for (var i = mod; i < len; i += 3)
        result += "," + int_part.substr(i, 3);
    result = ary[1] + result + ary[3];
    return result;
}

//管理帳票リストを選択
//検索条件を変更する

/*------------------------------------------------------------------------------*/
/* 機能　： 管理帳票選択時の画面レイアウトの変更処理　                          */
/* 作成日： ????/??/??  ????                                                    */
/* 更新日： 2017/09/04 arc yano #3786 預かり車Excle出力対応 預かり車一覧        */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function DocumentSelected(name, clearFlag) {
    setDisableAll();
    if (clearFlag == '1') {
        clearFormData();
    }

    // Mod 2014/07/03 arc amii chrome対応 IEとIE以外で、style.Displayに設定する値を変えるよう修正
    if (navigator.appName == "Microsoft Internet Explorer") {
        styleDisplay = "block";
    } else {
        styleDisplay = "";
    }
    
    document.getElementById('TemplateUse').value = '0';     //Add 2017/09/04 arc yano #3786
    
    switch (name) {
        case "NewCustomerList":
            document.getElementById('LastUpdate').style.display = styleDisplay;
            document.getElementById('LastUpdateFlag').innerText = '最近';
            break;
        case "ReceiptPlanList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('ReceiptPlanAccount').style.display = styleDisplay;
            break;
        case "ReceiptList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Office').style.display = styleDisplay;
            document.getElementById('CustomerClaimCd').style.display = styleDisplay;
            break;
        case "PaymentPlanList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('PaymentPlanAccount').style.display = styleDisplay;
            break;
        case "PaymentList":
            document.getElementById('Term').style.display = styleDisplay;
            break;
        case "ReceiptionList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Employee').style.display = styleDisplay;
            break;
        case "CarSalesList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('SalesOrder').style.display = styleDisplay;
            document.getElementById('Type').style.display = styleDisplay;
            break;
        case "ServiceSalesList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('ServiceOrder').style.display = styleDisplay;
            break;
        case "CarDM":
            document.getElementById('Rank').style.display = styleDisplay;
            document.getElementById('Type').style.display = styleDisplay;
            document.getElementById('CustomerKind').style.display = styleDisplay;
            document.getElementById('CommonDM').style.display = styleDisplay;
            document.getElementById('CarDM').style.display = styleDisplay;
            break;
        case "ServiceDM":
            document.getElementById('Rank').style.display = styleDisplay;
            document.getElementById('Type').style.display = styleDisplay;
            document.getElementById('CommonDM').style.display = styleDisplay;
            document.getElementById('ServiceDM').style.display = styleDisplay;
            break;
        case "CarStockList":
            document.getElementById('Car').style.display = styleDisplay;
            document.getElementById('NewUsed').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Type').style.display = styleDisplay;
            break;
        case "PartsStockList":
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Location').style.display = styleDisplay;
            break;
        case "CarStopList":
            break;
        case "CarPurchaseOrderList":
            break;
        case "PartsTransferList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "PartsPurchaseList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Office').style.display = styleDisplay;
            document.getElementById('Slip').style.display = styleDisplay;
            document.getElementById('Customer').style.display = styleDisplay;
            document.getElementById('Supplier').style.display = styleDisplay;
            break;
        case "DeadStockPartsList":
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('LastUpdate').style.display = styleDisplay;
            document.getElementById('LastUpdateFlag').innerText = '以前';
            break;
        case "ServiceDailyReport":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "ReceiptList":
            document.getElementById('Office').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('CustomerClaimCd').style.display = styleDisplay;
        case "CSSurveyGR":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "JournalList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "AccountReceivableBalanceList":
            document.getElementById('TargetYearMonth').style.display = styleDisplay;
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "CardReceiptPlanList":
            document.getElementById('Term').style.display = styleDisplay;
            document.getElementById('Department').style.display = styleDisplay;
            break;
        case "PartsAverageCostList":
            document.getElementById('TargetYearMonth').style.display = styleDisplay;
            break;
        case "CarStorageList":      //Add 2017/09/04 arc yano #3786
            document.getElementById('Department').style.display = styleDisplay;
            document.getElementById('TemplateUse').value = '1';
            break;
    }
    function setDisableAll() {
        document.getElementById('Term').style.display = "none";
        document.getElementById('SalesOrder').style.display = "none";
        document.getElementById('ServiceOrder').style.display = "none";
        document.getElementById('Car').style.display = "none";
        document.getElementById('NewUsed').style.display = "none";
        document.getElementById('Rank').style.display = "none";
        document.getElementById('Employee').style.display = "none";
        document.getElementById('Department').style.display = "none";
        document.getElementById('LastUpdate').style.display = "none";
        document.getElementById('Location').style.display = "none";
        document.getElementById('Type').style.display = "none";
        document.getElementById('CustomerKind').style.display = "none";
        document.getElementById('CustomerClaim').style.display = "none";
        document.getElementById('ReceiptPlanAccount').style.display = "none";
        document.getElementById('PaymentPlanAccount').style.display = "none";
        document.getElementById('CommonDM').style.display = "none";
        document.getElementById('CarDM').style.display = "none";
        document.getElementById('ServiceDM').style.display = "none";
        document.getElementById('Office').style.display = "none";
        document.getElementById('Slip').style.display = "none";
        document.getElementById('Customer').style.display = "none";
        document.getElementById('Supplier').style.display = "none";
        document.getElementById('CustomerClaimCd').style.display = "none";
        document.getElementById('TargetYearMonth').style.display = "none"; // add 2016.05.20 nishimura.akira 
  
    }
    function clearFormData() {
        document.getElementById('TermFrom').value = "";
        document.getElementById('TermTo').value = "";
        document.getElementById('SalesOrderStatus').selectedIndex = 0;
        document.getElementById('ServiceOrderStatus').selectedIndex = 0;
        document.getElementById('CarStatus').selectedIndex = 0;
        document.getElementById('NewUsedType').selectedIndex = 0;
        document.getElementById('CustomerRank').selectedIndex = 0;
        document.getElementById('CustomerType').selectedIndex = 0;
        document.getElementById('CustomerClaimType').selectedIndex = 0;
        document.getElementById('ReceiptPlanType').selectedIndex = 0;
        document.getElementById('PaymentPlanType').selectedIndex = 0;
        document.getElementById('EmployeeCode').value = "";
        document.getElementById('EmployeeName').innerText = "";
        document.getElementById('DepartmentCode').value = "";
        document.getElementById('DepartmentName').innerText = "";
        document.getElementById('OfficeCode').value = "";
        document.getElementById('OfficeName').innerText = "";
        document.getElementById('SlipNumber').value = "";
        document.getElementById('CustomerName').value = "";
        document.getElementById('SupplierCode').value = "";
        document.getElementById('SupplierName').innerText = "";
        document.getElementById('LastUpdateDate').value = "";
        document.getElementById('LocationType').selectedIndex = 0;
        document.getElementById('CustomerClaimCode').value = "";
        document.getElementById('CustomerClaimName').innerText = "";
    }
}
function displaySalesCar(obj) {
    var target = document.getElementById(obj);
    if (target.style.display == 'none') {
        target.style.display = 'block';
    } else {
        target.style.display = 'none';
    }
}
//-------------------------------------------------------------------------------------
// Mod 2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正
//-------------------------------------------------------------------------------------
function checkPurchaseOrder(num) {
    var purchaseOrder = document.getElementById('data[' + num + ']_PurchaseOrderStatus');
    var reserve = document.getElementById('data[' + num + ']_ReservationStatus');

    //発注時は引当もチェック
    if (purchaseOrder && purchaseOrder.checked) {
        reserve.checked = true;
    }
}
//function selectChangeType() {
//    var obj = document.getElementById('OwnershipChangeType');
//    if (obj.options[obj.selectedIndex].value == '003') {
//        if (confirm('所有者を「転売」に変更しても宜しいですか？')) {
//            document.getElementById('OwnerCode').value = '9990000001';
//            document.getElementById('PossesorName').value = '';
//            document.getElementById('PossesorAddress').value = '';
//            GetMasterDetailFromCode('OwnerCode', new Array('PossesorName', 'PossesorAddress'), 'Customer');
//        }
//    }
//}
function serviceSalesSubmit() {
    var doc = document;
    var msg = "";

    // 納車日
    var salesDate = doc.getElementById('SalesDate').value;

    // 次回点検日
    var nextInspectionDate = doc.getElementById('NextInspectionDate').value;

    // 車検有効期限
    var inspectionExpireDate = doc.getElementById('InspectionExpireDate').value;
    
    //Mod 2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能　納車日が未入力だった場合は当日で比較する
    //納車日が未入力だった場合、当日日付にする
    if (salesDate == "" || salesDate == undefined) {

        // 当日日付をyyyy/mm/ddで取得
        var ToDateTime = new Date();
        var ToDate = new Date(ToDateTime.getFullYear(), ToDateTime.getMonth(), ToDateTime.getDate());
        var strToDateYear = ToDate.getFullYear();
        var strToDateMonth = ToDate.getMonth() + 1;
        var strToDateDay = ToDate.getDate();
        //月・日の0(ゼロ)埋め
        strToDateMonth = ('0' + strToDateMonth).slice(-2);
        strToDateDay = ('0' + strToDateDay).slice(-2);
        var strToDateDate = strToDateYear + "/" + strToDateMonth + "/" + strToDateDay;

        salesDate = strToDateDate;
    }

    //伝票ステータス取得
    var ServiceOrderStatus = doc.getElementById('ServiceOrderStatus').value;

    if (nextInspectionDate == "") {
        msg += "次回点検日が入力されていません。\r\n";
    }
    if (inspectionExpireDate == "") {
        msg += "車検有効期限が入力されていません。\r\n";
    }
    if (nextInspectionDate != "" && nextInspectionDate <= salesDate) {
        msg += "次回点検日の日付が過去の日付になっています。\r\n";
    }
    if (inspectionExpireDate != "" && inspectionExpireDate <= salesDate) {
        msg += "車検有効期限が日付が過去の日付になっています。\r\n";
    }

    //納車確認書印刷済⇒納車済にする場合と、作業終了⇒納車確認書印刷済にする場合でメッセージを分ける
    if (msg != "") {
        if (ServiceOrderStatus == "005") {
            msg += "納車処理してもよろしいですか？";
        } else {
            msg += "納車確認書を出力してもよろしいですか？";
        }
    }

    if (msg == "") {
        if (ServiceOrderStatus == "005") {
            serviceFormSubmit();
        } else {
            calcTotalServiceAmount();
            doc.forms[0].PrintReport.value = 'ServiceSales';
            doc.forms[0].ActionType.value = 'SalesConfirm';
            serviceFormSubmit();
        }
    } else {
        var Ret = confirm(msg);
        if (Ret) {
            if (ServiceOrderStatus == "005") {
                serviceFormSubmit();
            } else {
                calcTotalServiceAmount();
                doc.forms[0].PrintReport.value = 'ServiceSales';
                doc.forms[0].ActionType.value = 'SalesConfirm';
                serviceFormSubmit();
            }
        }
    }
}
//----------------------------------------------------------------------------------------------------------------
// 機能　　： テキストボックス入力可／不可切替
// 引数  　： なし
// 作成日　： 2014/08/29 arc yano IPO対応その２
// 更新日　： 
//----------------------------------------------------------------------------------------------------------------
function changeEnable(id1, id2, behavor) {
    
    var element1 = document.getElementById(id1);
    var element2 = document.getElementById(id2);

    if (behavor == true) {  //入力不可
        element1.readOnly = true;
        element1.style.backgroundColor = "#eeeeee";

        element1.value = "";
        if (element2.tagName == 'input' || element2.tagName == 'INPUT') {
            element2.value = "";
        }
        else {
            element2.innerHTML = "";
        }
        
                 
    }
    else {  //入力可
        element1.readOnly = false;
        element1.style.backgroundColor = "#ffffff";
        
    }
}
//----------------------------------------------------------------------------------------------------------------
// 機能　　： サービス伝票submit処理
//          　サービス伝票submit前の入力項目チェックを行い、アラートを出力する。      
// 引数  　： なし
// 作成日　： ????/??/??            
// 更新日　：
//            2018/09/20 yano #3932 サービス伝票　請求先未入力で納車処理を行える 納車処理の場合は請求先のチェックは
//                                                サーバーで行う
//            2017/02/14 arc yano  #3641 金額欄のカンマ表示対応
//            2014/08/14 arc yano  #3065 ポップアップメッセージ大量出力対応 メッセージを10件まで表示するように修正
//            2014/07/16 arc yano  サービス伝票チェック新システム対応 社内粗利チェック追加      
//            2014/06/24 arc yano  サービス伝票チェック新システム対応 
//                                 ①外注原価が０の場合にアラートを出力するように修正
//                                 ②部品原価(合計)が０の場合にアラートを出力するように修正
//                                 ③部品原価(単価)が単価より高い場合にアラートを出力するように修正
//-------------------------------------------------------------------------------------------------------------------
function serviceFormSubmit() {

    //全ての金額欄のカンマの除去
    ExceptCommaAll();   //Add 2017/02/14 arc yano  #3641

    //--------------------------------------------------------
    // 作業内容明細行のidprefixを見た目の並びで取得
    //--------------------------------------------------------
    //テーブルオブジェクト取得
    var tbody = document.getElementById("salesline-tbody");
    
    var idArray = new Array();

    for (var i = 0; i < tbody.rows.length; i++) {
        var tr = tbody.rows.item(i);
        
        var result = tr.innerHTML.match(/line\[\d{1,4}\]\_/);

        if (result) {
            idArray[i] = result[0];     //id("line[xx]_")を保存
            //alert("idは" + idArray[i] + "です。");
        }
        else {
            idArray[i] = "";            //空文字
            //alert("idが見つかりませんでした。");
        }
    }


    //明細行数を取得
    //var LineCount = isNaN(parseInt($('#LineCount').val())) ? 0 : parseInt($('#LineCount').val());
    var LineCount = tbody.rows.length;

    //エラーメッセージ格納用
    var msg = "";
    var dispmsgflg = 0;
    var flgAlert = 0;                       //警告出力フラグ
    var chkInOffice = 0;                    //社内粗利チェック実行フラグ
    var chkParts = 0;                       //部品チェック
    var chkService = 0;                     //サービスメニューチェック
    var totalTechFee = 0;                   //技術料の合計
    var totalAmount = 0;                    //金額の合計
    var totalSmCost = 0;                    //原価(合計)の合計　※サービスメニュー
    var totalPtCost = 0;                    //原価(合計)の合計　※部品
    var msgcnt = 0;                         //メッセージカウント

    var costArray = new Array();
    for (var i = 0; i < LineCount; i++) {
        //Add 2014/06/09 arc yano 高速化対応 削除対象行以外の行をチェックする
        var delflag = document.getElementById(idArray[i] +'CliDelFlag');
        if ((delflag != null) && (delflag.value != "1")) {
            var serviceType = document.getElementById(idArray[i] + 'ServiceType');
            var lineContents = document.getElementById(idArray[i] + 'LineContents').value;

            //主作業
            if (serviceType && serviceType.value == '001') {

                //-------------------------------
                //社内粗利チェック実行
                //-------------------------------
                if(chkInOffice == 1){
                    //サービスメニュー
                    chkService = cmpValue(totalTechFee, totalSmCost);
                    totalTechFee = 0;
                    totalSmCost = 0;     

                    //部品
                    chkParts = cmpValue(totalAmount, totalPtCost);
                    totalAmount = 0;
                    totalPtCost = 0;
                }
                //-------------------------------
                //請求先未入力チェック
                //-------------------------------
                //Mod 2018/09/20 yano #3932
                if (document.forms[0].ActionType.value != 'Sales' && document.forms[0].ActionType.value != 'ModificationEnd') {  //納車処理、修正完了以外以外の場合

                    var customerClaimCode = document.getElementById(idArray[i] + 'CustomerClaimCode').value;
                    if (customerClaimCode == null || customerClaimCode == '') {

                        if (msgcnt < 10) { //メッセージ件数が１０件未満の場合   
                            msg += "主作業「" + lineContents + "」に対する請求先が指定されていません\r\n";
                        }
                        msgcnt++;       //メッセージカウントを加算
                    }
                }
                //--------------------------------
                //社内請求チェック
                //--------------------------------
                var svwkCode = document.getElementById(idArray[i] + 'ServiceWorkCode').value;
                if ((svwkCode != null) && (svwkCode != '') && (svwkCode.substr(0, 1) == '2')) {
                    chkInOffice = 1;
                }
                else {
                    chkInOffice = 0;
                }
            }

            //サービスメニュ
            if (serviceType && serviceType.value == '002') {
                var supplierCode = document.getElementById(idArray[i] + 'SupplierCode');
                var supplierName = document.getElementById(idArray[i] + 'SupplierName');
                var serviceCost = document.getElementById(idArray[i] + 'Cost');
                //-------------------------
                //外注原価未入力チェック
                //-------------------------
                //外注原価がnull、空文字、０の場合にアラートを出力する。
                if (supplierCode && supplierName && supplierCode.value != null && supplierCode.value != '' && (serviceCost.value == null || serviceCost.value == '' || serviceCost.value == '0')) {
                    if (msgcnt < 10) { //メッセージ件数が１０件未満の場合   
                        msg += "外注先「" + supplierName.value + "」の原価が入力されていません\r\n";
                    }
                    costArray.push(idArray[i]);
                    msgcnt++;       //メッセージカウントを加算
                }
                //-------------------------
                //社内粗利加算処理
                //-------------------------
                if (chkInOffice == 1) { //社内粗利チェック対象レコードの場合
                    var linetype = document.getElementById(idArray[i] + 'LineType').value;
                    if (linetype == '002') {    //外注
                        //技術料加算
                        var TechnicalFeeAmount = document.getElementById(idArray[i] + 'TechnicalFeeAmount').value;
                        totalTechFee += isNaN(parseInt(TechnicalFeeAmount)) ? 0 : parseInt(TechnicalFeeAmount);

                        //原価(合計)加算
                        var smCost = serviceCost.value;
                        totalSmCost += isNaN(parseInt(smCost)) ? 0 : parseInt(smCost);
                    }
                }
            }

            //部品
            if (serviceType && serviceType.value == '003') {
                var partsCost = document.getElementById(idArray[i] + 'Cost');

                //-----------------------------
                //部品原価未入力チェック
                //-----------------------------
                //部品原価(合計)がnull、空文字、０の場合にアラートを出力する。
                if (partsCost && partsCost.value == null || partsCost.value == '' || partsCost.value == '0') {
                    if (msgcnt < 10) { //メッセージ件数が１０件未満の場合   
                        msg += "部品「" + lineContents + "」の原価が入力されていません\r\n";
                    }
                    costArray.push(idArray[i]);
                    msgcnt++;       //メッセージカウントを加算
                }
                var objcost = document.getElementById(idArray[i] + 'UnitCost');
                var partUnitCost = isNaN(parseInt(objcost.value)) ? 0 : parseInt(objcost.value);

                var objprice = document.getElementById(idArray[i] + 'Price');
                var partPrice = isNaN(parseInt(objprice.value)) ? 0 : parseInt(objprice.value);

                //-----------------------------
                //部品原価、単価相関チェック
                //-----------------------------
                if (partUnitCost > partPrice) { //部品原価(単価)が単価より高い場合
                    dispmsgflg = 1;
                }
                //-------------------------
                //社内粗利加算処理
                //-------------------------
                if (chkInOffice == 1) {
                    var linetype = document.getElementById(idArray[i] + 'LineType').value;
                    
                    //金額加算
                    var Amount = document.getElementById(idArray[i] + 'Amount').value;
                    totalAmount += isNaN(parseInt(Amount)) ? 0 : parseInt(Amount);

                    //原価(合計)加算
                    var ptCost = partsCost.value;
                    totalPtCost += isNaN(parseInt(ptCost)) ? 0 : parseInt(ptCost);
                }
            }
        }
    }
    //------------------------------------------
    //最後にもう一度社内粗利チェック実行
    //------------------------------------------
    if (chkInOffice == 1) {
        //サービスメニュー
        chkService = cmpValue(totalTechFee, totalSmCost);
        //部品
        chkParts = cmpValue(totalAmount, totalPtCost);
    }
    if (chkService) {
        if (msgcnt < 10) { //メッセージ件数が１０件未満の場合 
            msg += "外注先使用の作業の売上と原価が一致しません\r\n";
        }
        msgcnt++;       //メッセージカウントを加算
    }
    if (chkParts) {
        if (msgcnt < 10) { //メッセージ件数が１０件未満の場合 
            msg += "部品の売上と原価が一致しません\r\n";
        }
        msgcnt++;       //メッセージカウントを加算
    }

    if (dispmsgflg == 1) {
        if (msgcnt < 10) { //メッセージ件数が１０件未満の場合 
            msg += "部品原価(単価)が単価を超えています\r\n";
        }
        msgcnt++;       //メッセージカウントを加算
    }

    //本来出力されるメッセージ件数が10件以上の場合
    if (msgcnt > 10) {
        msg += "メッセージ件数が10件以上あるため、10件以降は表示しません。\r\n";
    }

    if (msg != "") {
        msg += "処理を続行しますか？";
        if (!confirm(msg)) {
            //カンマ挿入
            InsertCommaAll();   //Add 2017/02/14 arc yano  #3641
            return;
        }
        for (var m = 0; m < costArray.length; m++) {
            document.getElementById(costArray[m] + 'Cost').value = 0;
        }
    }

    // Add 2014/09/29 arc amii arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる
    if (document.getElementById('AddressReconfirm').value != null && document.getElementById('AddressReconfirm').value == 'True') {
        alert('住所要確認となっているので、登録された住所をお客様にご確認お願いします');
    }

    formList();
    formSubmit();
}


// Add 2014/11/26 arc nakayama 部品棚卸作業日登録対応
function ConfirmationInventoryMonth() {

    var InventoryDate = document.getElementById('InputInventoryWorkingDate').value;
    var msg = "";
        
    msg += InventoryDate + "を登録します\r\n";
    msg += "処理を続行しますか？";
    if (!confirm(msg)) {
        return;
    }

    formSubmit();
}

function changeOutsourceEmployee(num) {
    var employee = document.getElementById('line[' + num + ']_Employee');
    var supplier = document.getElementById('line[' + num + ']_Supplier');
    var val = document.getElementById('line[' + num + ']_LineType');
    if (val && val.value == '001') {
        document.getElementById('line[' + num + ']_SupplierCode').value = '';
        document.getElementById('line[' + num + ']_SupplierName').value = '';
        employee.style.display = "inline";
        supplier.style.display = "none";
    } else {
        document.getElementById('line[' + num + ']_EmployeeNumber').value = '';
        document.getElementById('line[' + num + ']_EmployeeCode').value = '';
        document.getElementById('line[' + num + ']_EmployeeName').value = '';
        employee.style.display = "none";
        supplier.style.display = "inline";
    }
}



// SELECTの子要素（OPTION）を全削除する
function removeNodes(x) {
    if (x.hasChildNodes()) {
        while (x.childNodes.length > 0) {
            x.removeChild(x.firstChild);
        }
    }
}
// SELECTの子要素（OPTION）を作成する
function createNodes(x, data, addBlank) {
    var m = 0;
    //if (addBlank && data.DataList.length > 0) {
    if (addBlank) {
        x.options[m] = new Option("", "");
        m++;
    }
    for (var i = 0; i < data.DataList.length; i++) {
        x.options[m] = new Option(data.DataList[i].Name, data.DataList[i].Code);
        m++;
    }
}

//郵便番号検索
function getAddressFromPostCode(prefix) {

    if (!prefix) {
        prefix = '';
    }
    var prefecture = document.getElementById(prefix + 'Prefecture');
    var city = document.getElementById(prefix + 'City');
    var address = document.getElementById(prefix + 'Address1');
    var postcode = document.getElementById(prefix + 'PostCode');

    try {
        $.getJSON("/Zip/Criteria?zip=" + postcode.value,
            function(json) {
                if (json == null || json.length == 0) {
                    alert("正しい郵便番号を入れてください");
                    postcode.focus();
                } else {
                    prefecture.value = '';
                    city.value = '';
                    address.value = '';

                    prefecture.value = json.Prefecture;
                    city.value = json.City;
                    address.value = json.Town;
                }
            }
        );
    } catch (e) {
        alert(e);
    }
}
//コードからマスタを検索し、名称を取得する
//--------------------------------------------------------------------------------------------------
// Mod 2021/08/03 yano #4098 【ロケーションマスタ】ロケーションを無効に変更した時のチェック処理の追加
// Mod 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
// Mod 2016/04/13 arc yano #3480 サービス伝票 サービス伝票の請求先を主作業の内容により切り分ける　
//                            　　　　　　　　　 Code2フィールドの引数を追加 
// Mod 2015/11/09 arc yano #3291 
//                         名称取得に成功した際にコールするメソッドを引数として追加                         
//
// Mod 2015/06/09 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤
//                         曖昧検索時は、マスタに存在しない場合にメッセージを表示しない
// Mod 2014/07/09 arc yano chrome対応 
//  項目の種類により、値をセットする方法を変える。
// Mod 2014/06/25 arc
//  タイムアウト値の変更(5000→10000)
//  タイムアウト発生時のエラーハンドリング追加
//  戻り値を明示化
//---------------------------------------------------------------------------------------------------
function GetNameFromCode(
    CodeField,
    NameField,
    ControllerName,
    Ambiguous,          //曖昧検索フラグ(省略されていた場合は、曖昧検索は行わない) //Mod 2015/06/09
    func,              //名称取得成功時に行うメソッド
    param,　            //メソッドの引数
    Code2Field,
    func2,
    param2,
    includedeleted     //Add 2021/08/03 yano #4098
) {
    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')){//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        //Add 2016/04/14 arc yano #3480
        if (typeof(Code2Field) != 'undefined' && Code2Field != null && Code2Field != '') {
            var element = document.getElementById(Code2Field);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {

        //Add 2021/08/04 yano
        if (typeof (includedeleted) == 'undefined' || includedeleted == null || includedeleted == '') {
            includedeleted = false;
        }

        //if ($('#' + CodeField).val() != null && $('#'+CodeField).val() == '') {
        //    $('#' + NameField).html('');
        //    return;
        //} else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMaster",
            data: { code: document.getElementById(CodeField).value, includeDeleted: includedeleted },       //Mod 2021/08/03 yano #4098
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {

                    //Mod 2015/06/09 arc yano 
                    if ((typeof Ambiguous === 'undefined') || (Ambiguous == 'false') || (Ambiguous == null)) {  //曖昧検索ではない場合のみメッセージを表示

                        alert("マスタに存在しません");

                        document.getElementById(CodeField).value = '';
                        document.getElementById(CodeField).focus();
                    }

                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                            element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    //Add 2016/04/14 arc yano #3480
                    if (typeof (Code2Field) != 'undefined' && Code2Field != null && Code2Field != '') {
                        var element = document.getElementById(Code2Field);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.value = '';
                        }
                    }
                    //$('#' + NameField).html('');
                    //$('#' + CodeField).val('');
                    //$('#' + CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')){//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                            element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    //Add 2016/04/14 arc yano #3480 Code2Fieldが引数としてある場合は取得した値をセットする
                    if (typeof(Code2Field) != 'undefined' && Code2Field != null && Code2Field != '') {
                        element = document.getElementById(Code2Field);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Code2;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Code2);
                            }
                        } catch (e) {
                            element.value = data.Code2;
                        }
                    }

                    //引数が指定されていた場合は処理を行う
                    if (typeof (func) != 'undefined' && func != null) {
                        var ret = func(param, data);
                    }

                    //引数が指定されていた場合は処理を行う
                    if (typeof (func2) != 'undefined' && func2 != null) {
                        var ret = func2(param2, data.Name);
                    }
  
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}

//コードからマスタを検索し、名称を取得する(部品棚卸専用)
//------------------------------------------------------------------------------------------------------------
//  Mod 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 部品棚卸フラグによる絞込
//  Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
//  Mod 2015/06/05 arc yano IPO(部品棚卸)　障害対応、仕様変更② 引数にボタン制御を行うかどうかのフラグを追加
//  Mod 2015/05/27 arc yano IPO(部品棚卸)　障害対応、仕様変更② 引数にcloseMonthFlagを追加
//  Add 2015/05/15 arc yano IPO(部品棚卸)　取得に失敗した場合は、ボタンを非活性
//------------------------------------------------------------------------------------------------------------
function GetNameFromCodeForPartsInventory(
    CodeField,
    NameField,
    ControllerName,
    ButtonControl,
    WarehouseCode,
    WarehouseName
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }

            //Add 2016/08/13 arc yano #3596
            if (typeof (WarehouseCode) != 'undefined' && typeof (WarehouseName) != 'undefined') {
                //成功した場合は、倉庫情報を取得する
                GetWarehouseFromDepartment('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DepartmentWarehouse', ButtonControl);
            }
        }
        if (ButtonControl == 'true') {
            SetDisable();
        }
        return false;
    } else {
        //if ($('#' + CodeField).val() != null && $('#'+CodeField).val() == '') {
        //    $('#' + NameField).html('');
        //    return;
        //} else {
        $.ajax({
            type: "POST",
            //url: "/" + ControllerName + "/GetMaster",
            url: "/" + ControllerName + "/GetMasterForPartsInventory",      //Mod 2017/05/10 arc yano #3762
            data: { code: document.getElementById(CodeField).value },       //Mod 2017/05/10 arc yano #3762
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    if (ControllerName = 'Department') {
                        alert("入力した部門は部品棚卸対象外か、またはマスタに存在しません");
                    }
                    else {
                    alert("マスタに存在しません");
                    }
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();

                    //Add 2016/08/13 arc yano #3596
                    if (typeof (WarehouseCode) != 'undefined' && typeof (WarehouseName) != 'undefined') {
                        //成功した場合は、倉庫情報を取得する
                        GetWarehouseFromDepartment('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DepartmentWarehouse', ButtonControl);
                    }

                    //$('#' + NameField).html('');
                    //$('#' + CodeField).val('');
                    //$('#' + CodeField).focus();
                    //Mod 2015/06/05 arc yano
                    if (ButtonControl == 'true') {
                        SetDisable();
                    }
                    
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }

                    /* //Mod 2016/08/13 arc yano #3596 GetWarehouseFromDepartment内へ移動
                    //Mod 2015/06/05 arc yano
                    if (ButtonControl == 'true') {

                        //Mod 2015/06/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 部門コード、対象年月、棚卸作業日を変数に格納

                        var departmentCode = document.getElementById('DepartmentCode').value;
                        var inventoryMonth = document.getElementById('InventoryMonth').value;
                        var workingDate = document.getElementById('PartsInventoryWorkingDate').value;

                        var warehouseCode = document.getElementById('HdWarehouseCode').value;   //倉庫コード //Add 2016/08/13 arc yano #3596

                        //CheckInventoryStatus(departmentCode, inventoryMonth, workingDate);    //Mod  2016/08/13 arc yano #3596
                        CheckInventoryStatus(warehouseCode, inventoryMonth, workingDate);

                        //Mod 2015/06/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日ではなく、棚卸開始日時を表示するように変更
                        //                                                               部門コード取得に成功した場合は、棚卸開始日を取得する
                        //GetStartDateForPartsInventory(departmentCode, inventoryMonth, 'PartsInventoryStartDate', 'PartsInventory');
                        GetStartDateForPartsInventory(warehouseCode, inventoryMonth, 'PartsInventoryStartDate', 'PartsInventory');
                    }
                    */
                    //Add 2016/08/13 arc yano #3596
                    if (typeof (WarehouseCode) != 'undefined' && typeof (WarehouseName) != 'undefined') {
                        //成功した場合は、倉庫情報を取得する
                        GetWarehouseFromDepartment('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DepartmentWarehouse', ButtonControl);
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                if (NameField != '') {
                    var element = document.getElementById(NameField);
                    try {
                        if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                            element.value = '';
                        }
                        else {
                            element.innerHTML = '';
                        }
                    } catch (e) {
                        element.innerText = '';
                    }
                }
                document.getElementById(CodeField).value = '';
                document.getElementById(CodeField).focus();

                if (ButtonControl == 'true') {
                    SetDisable();
                }
                return false;
            }
        });
    }
}

//部門コード、対象年月から棚卸開始日時を取得する
//------------------------------------------------------------------------------------------------------------
//  Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数を変更(departmentCode→warehouseCode)
//  Add 2015/06/17 arc yano IPO(部品棚卸)　障害対応、仕様変更⑥ 棚卸開始日時を取得
//------------------------------------------------------------------------------------------------------------
function GetStartDateForPartsInventory(
    warehouseCode,
    inventoryMonth,
    startDateField,
    controllerName
    ) {

    var element = new Array();

    element[0] = document.getElementById(startDateField);
    element[1] = document.getElementById('s' + startDateField);

    //対象年月、部門コードのいずれか一方がnullまたは空文字の場合
    if ((inventoryMonth == null || inventoryMonth == '') || (warehouseCode == null || warehouseCode == '')) {
        
        for (i = 0; i < element.length; i++) {
            try {
                if ((element[i].tagName == 'INPUT') || (element[i].tagName == 'input')) {//input項目の場合
                    element[i].value = '';
                }
                else {
                    element[i].innerHTML = '';
                }
            } catch (e) {
                element[i].innerText = '';
            }
        }

        return false;

    } else {
        $.ajax({
            type: "POST",
            url: "/" + controllerName + "/GetStartDate",
            data: { warehouseCode: warehouseCode, inventoryMonth: inventoryMonth },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Name == null) {
                    if (startDateField != '') {

                        for (i = 0; i < element.length; i++) {
                            try {
                                if ((element[i].tagName == 'INPUT') || (element[i].tagName == 'input')) {//input項目の場合
                                    element[i].value = '';
                                }
                                else {
                                    element[i].innerHTML = '';
                                }
                            } catch (e) {
                                element[i].innerText = '';
                            }
                        }
                    }
                    return true;
                } else {
                    if (startDateField != '') {
                        for (i = 0; i < element.length; i++) {
                            try {
                                if ((element[i].tagName == 'INPUT') || (element[i].tagName == 'input')) {//input項目の場合
                                    element[i].value = data.Name;;
                                }
                                else {
                                    element[i].innerHTML = escapeHtml(data.Name);
                                }
                            } catch (e) {
                                element[i].innerText = data.Name;
                            }
                        }
                    }
                    return true;
                }
            }
        ,
            error: function () {  //通信失敗時
                alert("棚卸開始日時取得に失敗しました。");
                if (startDateField != '') {
                    for (i = 0; i < element.length; i++) {
                        try {
                            if ((element[i].tagName == 'INPUT') || (element[i].tagName == 'input')) {//input項目の場合
                                element[i].value = '';
                            }
                            else {
                                element[i].innerHTML = '';
                            }
                        } catch (e) {
                            element[i].innerText = '';
                        }
                    }
                }
                return false;
            }
        });
    }
}

//コードからマスタを検索し、名称を取得する(車両ステータス入力画面専用)
//------------------------------------------------------------------------------------
//Add 2014/11/12 arc yano 車両ステータス変更対応
//-----------------------------------------------------------------------------------
function GetNameFromCodeForCarUsage(
    CodeField,
    NameField,
    ControllerName
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
        //if ($('#' + CodeField).val() != null && $('#'+CodeField).val() == '') {
        //    $('#' + NameField).html('');
        //    return;
        //} else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMasterForCarUsage",
            data: { code: document.getElementById(CodeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しません");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();

                    //$('#' + NameField).html('');
                    //$('#' + CodeField).val('');
                    //$('#' + CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}

//-------------------------------------------------------------------------------------
// Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数追加
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明示化
//-------------------------------------------------------------------------------------
//コードがマスタに存在するか確認する
function IsExistCode(
    CodeField,
    ControllerName,
    NameField,
    ControllerName2
    ) {
    var code = document.getElementById(CodeField).value;
    //if ($('#' + CodeField).val() != null && $('#' + CodeField).val() == '') {
    if (code != null && code == '') {
        resetEntity();
        return false;
    }
    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/GetMaster",
        //data: { code: $("#" + CodeField).val() },
        data: { code: code },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code != null) {
                alert("既に登録されています");
                resetEntity();
                /*
                    $('#' + CodeField).val('');
                    $('#' + CodeField).focus();
                */
                return true;
            }
            
            //Mod 2016/08/13 arc yano #3596
            //引数が指定されていた場合は処理を行う
            if (typeof (NameField) != 'undefined' && NameField != null) {
                GetNameFromCode(CodeField, NameField, ControllerName2);
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("マスタ取得に失敗しました。");
            return false;
        }
    });

    //Add 2016/08/13 arc yano #3596
    function resetEntity() {
        document.getElementById(CodeField).value = '';
        
        //引数が指定されていた場合は処理を行う

        if (typeof (NameField) != 'undefined' && NameField != null) {
            var target = document.getElementById(NameField);
            var tagName = target.tagName.toLowerCase();

            if (tagName == 'input') {   //テキストボックス
                document.getElementById(NameField).value = '';
            }
            else if (tagName == 'select') { //リストボックス   
                var option = target.getElementsByTagName('option');
                for (k = 0; k < option.length; k++) {
                    if (k == 0) {
                        option[k].selected = true;
                    }
                    else {
                        option[k].selected = false;
                    }
                }
            }
            else {  //その他(ラベル)
                target.innerText = '';
            }
        }

       
    }


}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明示化
//---------------------------------------------------
//コードがマスタに存在するか確認する
function IsExistCode2(
    CodeField1,
    CodeField2,
    ControllerName,
    ClearField
    ) {
    if (($('#' + CodeField1).val() != null && $('#' + CodeField1).val() == '') || ($('#' + CodeField2).val() != null && $('#' + CodeField2).val() == '')) {
        return false;
    }
    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/GetMaster",
        data: { code1: $("#" + CodeField1).val(), code2: $("#" + CodeField2).val() },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code != null) {
                alert("既に登録されています");
                $('#' + ClearField).html('');
                $('#' + CodeField2).val('');
                $('#' + CodeField2).focus();
                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("マスタ取得に失敗しました。");
            return false;
        }
    });
}

//-------------------------------------------------------------------------------------------------------------------
// Mod 2022/05/03 yano #4133 引数追加 resetEntry()に行うメソッド
// Mod 2018/06/01 arc yano #3894 JLRデフォルト仕入先対応
// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//
// Mod 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//          取得したデータをカンマ付に変換する
// Mod 2015/09/18 arc yano #3263 車両伝票入力　車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える
//                                             項目のクリア処理にリストボックスだった場合の処理を追加
// Mod 2015/09/12 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動
//                             　詳細情報取得中に非活性にしたい項目を引数として追加   
// Mod 2015/07/29 arc nakayama #3217_デモカーが販売できてしまう問題の改善 
//                             車両伝票の車台番号のルックアップから本関数が呼ばれたかどうかを判定する引数追加
// Mod 2015/10/05 arc nakayama #3255_サービス伝票画面の車両のマスタボタンで車両データ開閉後、伝票上の車両情報が消える 
//                             サービス伝票のマスタボタンから呼ばれた場合は、マスタに存在していなくてもメッセージを出さず、内容も消さない
// Edit 2014/07/15 arc yano chrome対応
// ①elementの種類により、値を設定する方法を変える
// ②dataがundefine値だった場合を考慮した処理に修正
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明示化
// 2009/12/16追加 t.ryumura
// キー項目からマスタの詳細情報を取得して画面にセットする
// 引数1:codeField コード項目名
// 引数2:nameFieldArray 値を取得する項目の配列
// 引数3:controllerName:Ajaxのサーバー側コントローラー名
//--------------------------------------------------------------------------------------------------------------------
function GetMasterDetailFromCode(
    codeField,                  //マスタ検索時のキー項目
    nameFieldArray,             //取得したマスタ情報をセットする項目　※複数設定可
    controllerName,             //検索先マスタ
    SelectByCarSlip,            //絞込み条件(車両マスタの場合は、在庫ステータス)
    lockObjectArray,            //マスタ取得失敗等に読取専用にする項目 ※複数設定可
    NotMaster,                  //マスタに存在していなくても何もしない(メッセージを出さず、内容も空にしない)
    func,
    func2,                      //Add 2018/06/01 arc yano #3894
    clearfunc                   //Add 2022/05/03 yano #4133
    ) {

    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
        for (i = 0; i < lockObjectArray.length; i++) {
            document.getElementById(lockObjectArray[i]).disabled = true;
        }
    }

    var isTaxRecalc = false;
    if (SelectByCarSlip == null || SelectByCarSlip == '') {
        SelectByCarSlip = "0";
    }
    if (NotMaster == null || NotMaster == '') {
        NotMaster = "0";
    }
    if (document.getElementById(codeField).value != null && document.getElementById(codeField).value == '') {
        resetEntity();
        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
            for (i = 0; i < lockObjectArray.length; i++) {
                document.getElementById(lockObjectArray[i]).disabled = false;
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/" + controllerName + "/GetMasterDetail",
            data: { code: document.getElementById(codeField).value, SelectByCarSlip: SelectByCarSlip },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                var srvCodeName = (codeField.indexOf("_") == -1 ? codeField : codeField.substring(codeField.indexOf("_") + 1));
                srvCodeName = (srvCodeName.indexOf(".") == -1 ? srvCodeName : srvCodeName.substring(srvCodeName.indexOf(".") + 1));
                if (data[srvCodeName] == null) {
                    if (NotMaster != "1") {
                        alert("マスタに存在しません");
                        resetEntity();
                    }
                    document.getElementById(codeField).focus();
                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                        for (i = 0; i < lockObjectArray.length; i++) {
                            document.getElementById(lockObjectArray[i]).disabled = false;
                        }
                    }
                    return false;
                } else {
                    for (var i = 0; i < nameFieldArray.length; i++) {
                        var srvItemName = (nameFieldArray[i].indexOf("_") == -1 ? nameFieldArray[i] : nameFieldArray[i].substring(nameFieldArray[i].indexOf("_") + 1));
                        srvItemName = (srvItemName.indexOf(".") == -1 ? srvItemName : srvItemName.substring(srvItemName.indexOf(".") + 1));
                        //debug for chrome
                        //alert("srvItemName=" + srvItemName + ":data[srvItemName]=" + data[srvItemName]);
                        var element = document.getElementById(nameFieldArray[i]);
                        if (element.tagName == "SELECT") {
                            for (var m = 0; m < element.childNodes.length; m = m + 2) {
                                if (element.childNodes[m].value == data[srvItemName]) {
                                    element.selectedIndex = element.childNodes[m].index;
                                    element.value = data[srvItemName]; //Add 2019/09/17 #4011
                                }
                            }
                        } else {
                            try {
                                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合

                                    //2017/04/23 arc yano #3755
                                    if (element.className == 'money') { //金額欄なら
                                        element.value = data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]);
                                    }
                                    else {
                                        element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
                                    }
                                }
                                else {

                                    //2017/04/23 arc yano #3755
                                    if (element.className == 'money') { //金額欄なら
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]));
                                    }
                                    else {
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : data[srvItemName]);
                                    }
                                }
                            } catch (e) {
                                //2017/04/23 arc yano #3755
                                if (element.className == 'money') { //金額欄なら
                                    element.value = data[srvItemName] == null ? '' : ConversionWithComma(data[srvItemName]);
                                }
                                else {
                                    element.value = data[srvItemName] == null ? '' : data[srvItemName];
                                }
                            }
                        }

                        //ADD 2014/02/20 ookubo.グレードコードのみ車両消費税額の再計算
                        if (controllerName == 'CarGrade' && srvItemName == "SalesTax") {
                            isTaxRecalc = true;
                        }

                        //--------------------------
                        // 設定されている場合
                        //--------------------------
                        if (typeof (func) != 'undefined' && typeof (func) != null && func != null) {
                            func(nameFieldArray[i], data[srvItemName]);
                        }
                    }

                    //Add 2018/06/01 arc yano #3894
                    //--------------------------
                    // 設定されている場合
                    //--------------------------
                    if (typeof (func2) != 'undefined' && typeof (func2) != null && func2 != null) {
                        func2(nameFieldArray, data);
                    }

                    //ADD 2014/02/20 ookubo.グレードコードのみ車両消費税額の再計算
                    if (isTaxRecalc) { calcSalesPrice(1); }
                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                        for (i = 0; i < lockObjectArray.length; i++) {
                            document.getElementById(lockObjectArray[i]).disabled = false;
                        }
                    }

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                    for (i = 0; i < lockObjectArray.length; i++) {
                        document.getElementById(lockObjectArray[i]).disabled = false;
                    }
                }
                return false;
            }

        });
    }
    //Edit 2014/07/18 arc yano chrome対応 項目の種類により値のセット方法を変更する。
    function resetEntity() {
        document.getElementById(codeField).value = '';
        for (var j = 0; j < nameFieldArray.length; j++) {
            try {

                var target = document.getElementById(nameFieldArray[j]);
                var tagName = target.tagName.toLowerCase();

                if (tagName == 'input') {   //テキストボックス
                    document.getElementById(nameFieldArray[j]).value = '';
                }
                else if (tagName == 'select') { //リストボックス   
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                else {  //その他(ラベル)
                    document.getElementById(nameFieldArray[j]).innerText = '';
                }

            } catch (e) {
                document.getElementById(nameFieldArray[j]).innerText = '';
            }
        }

        //Add 2022/05/03 yano #4133
        //--------------------------
        // 設定されている場合
        //--------------------------
        if (typeof (clearfunc) != 'undefined' && typeof (clearfunc) != null && clearfunc != null) {
            clearfunc(nameFieldArray);
        }
    }
}


//--------------------------------------------------------------------------------------------
// 機能：グレードマスタ情報取得(上書きするかダイアログを表示）
//       
// 作成：2020/11/18 yano #4071 【車両マスタ】グレードコード変更時に車両情報を上書きするかの確認機能の追加
// 履歴：
//
//--------------------------------------------------------------------------------------------
function GetGradeMasterDetailFromCode(
    codeField,                          //マスタ検索時のキー項目
    nameFieldArrayRequired,             //取得したマスタ情報をセットする項目（必須項目)
    nameFieldArrayChoices,              //取得したマスタ情報をセットする項目（選択項目）
    oldCodeField                       //古いコードを保持するID
    
    ) {

    //キー項目の値が未入力の場合は空にする。
    if (document.getElementById(codeField).value != null && document.getElementById(codeField).value == '') {
        resetEntity();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/CarGrade/GetMasterDetail",
            data: { code: document.getElementById(codeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                var srvCodeName = (codeField.indexOf("_") == -1 ? codeField : codeField.substring(codeField.indexOf("_") + 1));
                srvCodeName = (srvCodeName.indexOf(".") == -1 ? srvCodeName : srvCodeName.substring(srvCodeName.indexOf(".") + 1));
                if (data[srvCodeName] == null) {
                        alert("マスタに存在しません");
                        resetEntity();
                        document.getElementById(codeField).focus();
                    return false;
                } else {
                    //必須項目から取得する
                    for (var i = 0; i < nameFieldArrayRequired.length; i++) {
                        var srvItemName = (nameFieldArrayRequired[i].indexOf("_") == -1 ? nameFieldArrayRequired[i] : nameFieldArrayRequired[i].substring(nameFieldArrayRequired[i].indexOf("_") + 1));
                        srvItemName = (srvItemName.indexOf(".") == -1 ? srvItemName : srvItemName.substring(srvItemName.indexOf(".") + 1));
                        var element = document.getElementById(nameFieldArrayRequired[i]);
                        if (element.tagName == "SELECT") {
                            for (var m = 0; m < element.childNodes.length; m = m + 2) {
                                if (element.childNodes[m].value == data[srvItemName]) {
                                    element.selectedIndex = element.childNodes[m].index;
                                    element.value = data[srvItemName];
                                }
                            }
                        } else {
                            try {
                                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合

                                    if (element.className == 'money') { //金額欄なら
                                        element.value = data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]);
                                    }
                                    else {
                                        element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
                                    }
                                }
                                else {

                                    if (element.className == 'money') { //金額欄なら
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]));
                                    }
                                    else {
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : data[srvItemName]);
                                    }
                                }
                            } catch (e) {

                                if (element.className == 'money') { //金額欄なら
                                    element.value = data[srvItemName] == null ? '' : ConversionWithComma(data[srvItemName]);
                                }
                                else {
                                    element.value = data[srvItemName] == null ? '' : data[srvItemName];
                                }
                            }
                        }
                    }

                    //選択項目の更新
                    if (document.getElementById(oldCodeField).value == '' || confirm('グレードが変更されました。マスタに登録している情報で上書きしますか。\nキャンセルをクリックした場合は車名、ブランド名、車種名、グレード名のみ\n変更されます')) {
                       
                        for (var i = 0; i < nameFieldArrayChoices.length; i++) {
                            var srvItemName = (nameFieldArrayChoices[i].indexOf("_") == -1 ? nameFieldArrayChoices[i] : nameFieldArrayChoices[i].substring(nameFieldArrayChoices[i].indexOf("_") + 1));
                            srvItemName = (srvItemName.indexOf(".") == -1 ? srvItemName : srvItemName.substring(srvItemName.indexOf(".") + 1));
                            var element = document.getElementById(nameFieldArrayChoices[i]);
                            if (element.tagName == "SELECT") {
                                for (var m = 0; m < element.childNodes.length; m = m + 2) {
                                    if (element.childNodes[m].value == data[srvItemName]) {
                                        element.selectedIndex = element.childNodes[m].index;
                                        element.value = data[srvItemName];
                                    }
                                }
                            } else {
                                try {
                                    if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合

                                        if (element.className == 'money') { //金額欄なら
                                            element.value = data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]);
                                        }
                                        else {
                                            element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
                                        }
                                    }
                                    else {

                                        if (element.className == 'money') { //金額欄なら
                                            element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]));
                                        }
                                        else {
                                            element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : data[srvItemName]);
                                        }
                                    }
                                } catch (e) {

                                    if (element.className == 'money') { //金額欄なら
                                        element.value = data[srvItemName] == null ? '' : ConversionWithComma(data[srvItemName]);
                                    }
                                    else {
                                        element.value = data[srvItemName] == null ? '' : data[srvItemName];
                                    }
                                }
                            }
                        }
                    }

                    document.getElementById(oldCodeField).value = document.getElementById(codeField).value;

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                
                return false;
            }

        });
    }
    
    //リセット処理
    function resetEntity() {

        document.getElementById(oldCodeField).value = '';

        document.getElementById(codeField).value = '';

        var nameFieldArray = nameFieldArrayRequired.concat(nameFieldArrayChoices);

        for (var j = 0; j < nameFieldArray.length; j++) {
            try {

                var target = document.getElementById(nameFieldArray[j]);
                var tagName = target.tagName.toLowerCase();

                if (tagName == 'input') {   //テキストボックス
                    document.getElementById(nameFieldArray[j]).value = '';
                }
                else if (tagName == 'select') { //リストボックス   
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                else {  //その他(ラベル)
                    document.getElementById(nameFieldArray[j]).innerText = '';
                }

            } catch (e) {
                document.getElementById(nameFieldArray[j]).innerText = '';
            }
        }
    }
}




// 2009/12/25追加 t.ryumura
// オプションコードからオプション名、単価を取得して画面にセットする
//-------------------------------------------------------------------------------------------------------------
// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算
// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 、オプション計算、環境性能割計算
// Mod 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//                               取得したデータをカンマ付に変換する
// Mod 2014/07/07 arc yano chrome対応
// ①getElementbyIdに渡すパラメータをname→idに修正
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//-------------------------------------------------------------------------------------------------------------
function GetOptionMasterFromCode(num) {
    if (document.getElementById('line[' + num + ']_CarOptionCode').value != null && document.getElementById('line[' + num + ']_CarOptionCode').value == '') {
        resetEntity();
        //Mod 2019/09/04 yano #4011
        calcTotalOptionAmount();
        //Mod 2019/10/17 yano #4022
        GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
        //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
        calcTotalAmount();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/CarOption/GetMasterDetail",
            data: { code: document.getElementById('line[' + num + ']_CarOptionCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['CarOptionCode'] == null) {
                    alert("マスタに存在しません");
                    resetEntity();
                    document.getElementById('line[' + num + ']_CarOptionCode').focus();
                    return false;
                } else {
                    var salesPrice = isNaN(parseInt(data['SalesPrice'])) ? 0 : parseInt(data['SalesPrice']);
                    var taxAmount = calcTaxAmount(salesPrice);
                    document.getElementById('line[' + num + ']_CarOptionName').value = data['CarOptionName'];
                    document.getElementById('line[' + num + ']_Amount').value = ConversionWithComma(data['SalesPrice']);
                    document.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(taxAmount);
                    document.getElementById('line[' + num + ']_AmountWithTax').value = ConversionWithComma(salesPrice + taxAmount);

                    // Mod 2021/03/26 sola owashi エラーが出るので修正
                    //        calcOptionTotalAmount() は存在しない関数名
                    //       以前の修正ミス（誤記）＋テスト漏れの可能性が高い
                    // 修正前 --------------------
                    // //Mod 2019/09/04 yano #4011
                    // calcOptionTotalAmount();
                    // 修正後：開始---------------
                    calcTotalOptionAmount();
                    // 修正後：終了---------------
                    
                    //Mod 2019/10/17 yano #4022
                    GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
                    //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
                    calcTotalAmount();
                    return true;
                }
            }
             ,
            error: function () {  //通信失敗時
                alert("オプション名、単価の取得に失敗しました。");
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById('line[' + num + ']_CarOptionCode').value = '';
        document.getElementById('line[' + num + ']_CarOptionName').value = '';
        document.getElementById('line[' + num + ']_Amount').value = '';
        document.getElementById('line[' + num + ']_TaxAmount').value = '';
        document.getElementById('line[' + num + ']_AmountWithTax').value = '';
    }
}


//---------------------------------------------------
// Edit 2014/07/07 arc yano chrome対応
// ①getElementbyIdに渡すパラメータをname→idに修正
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
// グレードコードから各種情報を取得して画面にセットする
function GetGradeMasterFromCode(num) {
    if (document.getElementById('line[' + num + ']_CarGradeCode').value != null && document.getElementById('line[' + num + ']_CarGradeCode').value == '') {
        resetEntity();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/CarGrade/GetMasterDetail",
            data: { code: document.getElementById('line[' + num + ']_CarGradeCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['CarGradeCode'] == null) {
                    alert("マスタに存在しません");
                    resetEntity();
                    document.getElementById('line[' + num + ']_CarGradeCode').focus();
                    return false;
                } else {
                    document.getElementById('line[' + num + ']_MakerName').innerText = data['MakerName'];
                    document.getElementById('line[' + num + ']_ModelCode').innerText = data['ModelCode'];
                    document.getElementById('line[' + num + ']_CarName').innerText = data['CarName'];
                    document.getElementById('line[' + num + ']_CarGradeName').innerText = data['CarGradeName'];
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("グレードマスタの取得に失敗しました。");
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById('line[' + num + ']_CarGradeCode').value = '';
        document.getElementById('line[' + num + ']_MakerName').innerText = '';
        document.getElementById('line[' + num + ']_ModelCode').innerText = '';
        document.getElementById('line[' + num + ']_CarName').innerText = '';
        document.getElementById('line[' + num + ']_CarGradeName').innerText = '';
    }
}

//部品番号から在庫数量、単価を取得する
//------------------------------------------------------------------------------------------------------------
// Mod 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 判断の引数に純正区分を使用しないように変更
// Mod 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//                               取得したデータをカンマ付に変換する
// Mod 2015/10/28 arc yano #3289 部品仕入機能改善(サービス伝票入力) 
//                         ①サービス伝票純正区分により、在庫判断リストを取得する処理を追加
//                         ②明細行の引当済数、発注数を更新する処理を追加
//                         ③情報の取得に失敗した場合は、部品名称、原価をクリアする処理を追加
// Mod 2014/07/07 arc yano chrome対応 getElementbyIdのパラメータ変更(name値→id値)
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//-------------------------------------------------------------------------------------------
function GetPartsStockMasterFromCode(num) {

    // Add  2015/10/28 arc yano #3289
    document.getElementById('line[' + num + ']_ProvisionQuantity').value = '0';
    document.getElementById('line[' + num + ']_OrderQuantity').value = '0';


    if (document.getElementById('line[' + num + ']_PartsNumber').value != null && document.getElementById('line[' + num + ']_PartsNumber').value == '') {
        resetEntity();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/PartsStock/GetMasterDetail",
            data: { code: document.getElementById('line[' + num + ']_PartsNumber').value, departmentCode: document.getElementById('DepartmentCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['PartsNumber'] == null) {
                    //alert("マスタに存在しません");
                    resetEntity();
                    calcTotalServiceAmount();
                    //document.getElementById('line[' + num + ']_PartsNumber').focus();
                    return false;
                } else {
                    document.getElementById('line[' + num + ']_PartsStock').value = data['Quantity'];
                    document.getElementById('line[' + num + ']_Price').value = ConversionWithComma(data['Price']);  //Mod 2017/04/23 arc yano #3755
                    document.getElementById('line[' + num + ']_LineContents').value = data['PartsName'];
                    document.getElementById('line[' + num + ']_UnitCost').value = ConversionWithComma(data['Cost']);    //Mod 2017/04/23 arc yano #3755

                    // Mod 2017/10/19 arc yano #3803
                    // Add  2015/10/28 arc yano #3289
                    // Mod 2016/02/09 arc nakayama #3427_サービス伝票入力画面　部品番号未入力時の判断表示 純正区分が設定されていない部品は社外品の判断を表示する
                    //if (data['GenuineType'] == null || data['GenuineType'] == "") {
                    //    data['GenuineType'] = "002"
                    //}
                    GetMasterListAll('ServiceSalesOrder', 'line[' + num + ']_StockStatus');

                    //Add 2014/06/24 arc yano 部品を変更した場合は、原価(合計)を再計算
                    //calcTotalServiceAmount()はcalcLineCost(num)内で行っているため、ここではコメントアウト
                    calcLineCost(num);

                    //calcLineAmount(num);
                    //calcTotalServiceAmount();

                    return true;
                }
            }
             ,
            error: function () {  //通信失敗時
                alert("部品在庫情報の取得に失敗しました。");
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById('line[' + num + ']_PartsStock').value = '0';
        document.getElementById('line[' + num + ']_Price').value = '0';
        document.getElementById('line[' + num + ']_LineContents').value = '';
        document.getElementById('line[' + num + ']_UnitCost').value = '0';
        document.getElementById('line[' + num + ']_Amount').value = '0';
    }
}

//---------------------------------------------------------------------
// Mod 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//                               取得したデータをカンマ付に変換する
// Mod 2014/07/09 arc yano
// ①getElementbyIdに渡すパラメータをname→idに修正
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------------------------
//グレードコード、サービスメニューコードから標準工数を取得する
function GetCostFromServiceMenu(num) {
    if ((document.getElementById('CarGradeCode').value != null && document.getElementById('CarGradeCode').value == '') ||
        (document.getElementById('line[' + num + ']_ServiceMenuCode').value != null && document.getElementById('line[' + num + ']_ServiceMenuCode').value == '')) {
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/ServiceCost/GetMasterDetail",
            data: { gradeCode: document.getElementById('CarGradeCode').value, menuCode: document.getElementById('line[' + num + ']_ServiceMenuCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['ServiceMenuCode'] == null) {
                    document.getElementById('line[' + num + ']_ManPower').value = '0';
                    calcLineAmount(num);
                    return false;
                } else {
                    document.getElementById('line[' + num + ']_LineContents').value = data['ServiceMenuName'];
                    document.getElementById('line[' + num + ']_ManPower').value = data['Cost'];    //Mod 2017/04/23 arc yano #3755
                    calcLineAmount(num);
                    calcTotalServiceAmount();
                    return true;
                }
            }
             ,
            error: function () {  //通信失敗時
                alert("標準工数の取得に失敗しました。");
                return false;
            }

        });
    }
}
//------------------------------------------------------------------------------
// Mod 2019/02/06 yano #3959 サービス伝票入力　請求先誤り
// Mod 2016/04/14 arc yano #3480
// ①請求先分類も取得するように修正
// Mod 2014/07/07 arc yano chrome対応
// ①getElementbyIdに渡すパラメータをname→idに修正
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//------------------------------------------------------------------------------
//主作業を取得する
function GetServiceWork(num) {

    //Mod 2016/04/14 arc yano #3480 請求先分類を取得する
    var customerClaimClass = document.getElementById('line[' + num + ']_CCCustomerClaimClass').value;
    var CustomerClaimCode = document.getElementById('line[' + num + ']_CustomerClaimCode').value;

    if (document.getElementById('line[' + num + ']_ServiceWorkCode').value != null && document.getElementById('line[' + num + ']_ServiceWorkCode').value == '') {
        document.getElementById('line[' + num + ']_LineContents').value = '';
        document.getElementById('line[' + num + ']_Classification1').value = '';
        document.getElementById('line[' + num + ']_SWCustomerClaimClass').value = ''; //Mod 2016/04/14 arc yano #3480
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/ServiceWork/GetMasterDetail",
            data: { code: document.getElementById('line[' + num + ']_ServiceWorkCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['ServiceWorkCode'] == null) {
                    alert('マスタに存在しません');
                    document.getElementById('line[' + num + ']_ServiceWorkCode').value = '';
                    document.getElementById('line[' + num + ']_LineContents').value = '';
                    document.getElementById('line[' + num + ']_Classification1').value = '';
                    document.getElementById('line[' + num + ']_SWCustomerClaimClass').value = ''; //Mod 2016/04/14 arc yano #3480
                    return false;
                } else {
                    
                    //Mod 2016/04/14 arc yano #3480 入力されている請求先のタイプと一致しない主作業を入力した場合はメッセージを表示し、入力値をクリア
                    //Mod 2017/05/02 arc nakayama #3760_サービス伝票　請求先を選択した状態で主作業のルックアップを開くとシステムエラー
                    if (customerClaimClass != '' && data['CustomerClaimClass'] != '' && customerClaimClass != data['CustomerClaimClass'] && CustomerClaimCode != '') {
                        if (customerClaimClass == '2') {
                            alert('社内の請求先が設定されているため、社内の主作業を選択して下さい');
                        }
                        else {
                            alert('社外の請求先が設定されているため、社外の主作業を選択して下さい');
                        }

                        document.getElementById('line[' + num + ']_ServiceWorkCode').value = '';
                        document.getElementById('line[' + num + ']_LineContents').value = '';
                        document.getElementById('line[' + num + ']_Classification1').value = '';
                        document.getElementById('line[' + num + ']_SWCustomerClaimClass').value = ''; //Mod 2016/04/14 arc yano #3480 
                    }
                    else {
                        document.getElementById('line[' + num + ']_LineContents').value = data['ServiceWorkName'];
                        document.getElementById('line[' + num + ']_Classification1').value = data['Classification1'];
                        document.getElementById('line[' + num + ']_SWCustomerClaimClass').value = data['CustomerClaimClass']; //Mod 2016/04/14 arc yano #3480

                        GetCustomerClaimByServiceWork(document.getElementById('line[' + num + ']_ServiceWorkCode').value, document.getElementById('DepartmentCode').value, num);  //Add 2019/02/06 yano #3959

                        //calcTotalServiceAmount(); //Mod 2019/02/06 yano #3959 GetCustomerClaimByServiceWork内に移動
                    }
                    return true;
                }
            }
               ,
            error: function () {  //通信失敗時
                alert("主作業の取得に失敗しました。");
                return false;
            }
        });
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function GetLocationPartsStock(partsNumber, locationCode) {
    if (partsNumber == null || partsNumber == '' || locationCode == null || locationCode == '') {
        //Mod 2015/03/17 arc iijima パーツ名取得
        document.getElementById('StockQuantity').value = '0';
        GetNameFromCode('PartsNumber', 'PartsNameJp', 'Parts')
        // Mod 2014/07/24 arc amii 部品番号が空白のままフォーカス外れた時、部品名称と在庫数をクリアする処理を追加
        //document.getElementById('PartsNameJp').value = '';
        //document.getElementById('StockQuantity').value = '0';
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/PartsStock/GetMaster",
            data: { partsNumber: partsNumber, location: locationCode },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['PartsNumber'] == null) {

                    //Mod 2015/03/17 arc iijima マスタにないパーツはエラー、PartsNumberを空白にする
                    alert("マスタに存在しません");
                    document.getElementById('PartsNumber').value = '';
                    document.getElementById('PartsNameJp').value = '';
                    document.getElementById('StockQuantity').value = '0';
                    //document.getElementById('PartsNumber').focus();
                    return false;
                } else {
                    // Mod 2014/07/24 arc amii chrome対応 innerTextはInputタグの項目には使用できないのでvalueに変更
                    document.getElementById('PartsNameJp').value = data['PartsNameJp'];
                    document.getElementById('StockQuantity').value = data['Quantity'];

                    return true;
                }
            }
             ,
            error: function () {  //通信失敗時
                alert("部品名、在庫数取得に失敗しました。");
                return false;
            }
        });
    }
}

//---------------------------------------------------
// Edit 2014/07/09 arc yano chrome対応
// ①elementの種類により、値を設定する方法を変える
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
//ロケーションコードからマスタを検索し、名称を取得する
function GetLocationNameFromCode(
    CodeField,
    NameField,
    BusinessTypeArray,
    LocationType
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Location/GetMaster",
            data: { code: document.getElementById(CodeField).value, businessType: BusinessTypeArray, locationType: LocationType },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しません");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            element.value = '';
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();

                    //$('#' + NameField).html('');
                    //$('#' + CodeField).val('');
                    //$('#' + CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                            element.innerHTML = escapeHtml(data.Name);
                            }
                            
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
             ,
            error: function () {  //通信失敗時
                alert("ロケーション名の取得に失敗しました。");
                return false;
            }
        });
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
//タスク件数を取得
//新規タスクが追加されたらダイアログを表示
function GetTaskCount() {
    try {
        $.ajax({
            type: "POST",
            url: "/Home/GetTaskCount",
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                document.getElementById('TaskCount').innerText = data['Count'];
                if (data['Flag'] == "1") {
                    if (confirm('新しいタスクが追加されました。\r\n今すぐ確認しますか？')) {
                        top.MainFrame.location.href = "/Task/Criteria";
                    }
                }
                return true;
            }
            ,
            error: function () {  //通信失敗時
                //alert("タスク件数の取得に失敗しました。");    //2014/06/30 定周期で呼ばれるため、コメントアウト
                return false;
            }
        });
    } catch (e) {
        return false;
    }
}

//---------------------------------------------------
// Edit 2014/07/07 arc yano chrome対応
// ①getElementbyIdに渡すパラメータをname→idに修正
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function GetSalesCar(num) {
    if (document.getElementById('data[' + num + ']_SalesCarNumber').value != null && document.getElementById('data[' + num + ']_SalesCarNumber').value == '') {
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/SalesCar/GetMaster",
            data: { code: document.getElementById('data[' + num + ']_SalesCarNumber').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {
                    document.getElementById('data[' + num + ']_Vin').innerText = "新規作成";
                } else {
                    document.getElementById('data[' + num + ']_Vin').innerText = data.Name == null ? "" : data.Name;
                }
                return true;
            }
            ,
            error: function () {  //通信失敗時
                alert("車両情報の取得に失敗しました。");
                return false;
            }
        });
    }
}

//------------------------------------------------------------------------------------------------------------
//機能：
//      手入力した車台番号に紐づく車両情報の取得
//  　  入力された車台番号をキーに有効な車両情報のみ取得するため、本来は一意で取得できるはずだが、
//      現状、車両情報(dbo.SalesCar)が整備されていないため、有効な車両情報が複数取得される場合がある。
//      このため、複数取れた場合はルックアップのウィンドウ表示し、そこからユーザに選択させるように対応。
//更新履歴：
//     2022/01/08 yano #4121 【サービス伝票入力】Chrome・明細行の部品在庫情報取得の不具合対応
//     2017/05/10 arc yano #3762 車両在庫棚卸機能追加
//     2016/08/23 arc yano #3621 車両査定入力　車台番号のマスタチェックの廃止 
//                                             マスタに存在しない場合に、メッセージを表示するかどうかの引数を追加
//     2016/02/16 ARC Mikami #3077 車両査定入力画面用にコントローラを切替え
//     2015/09/24 arc yano #3263 車台番号検索のダイアログで何も選ばず消すとリストボックスの項目が消える
//                         引数に絞込み条件を追加
//     2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 
//                        ①車台番号がクリアされた時は、紐づく車両情報もクリアするように修正
//                        ②サービス伝票入力画面専用処理となっていたので、他画面からも呼べるように共通化
//                      　③車両情報取得中に非活性にする項目を引数として追加
// Add 2014/07/24 arc amii 新規追加 
// 車台番号から管理番号を取得し、車両情報を取得・表示する
//------------------------------------------------------------------------------------------------------------
function GetSalesCarInfo(vinField, salesCarNumField, nameFieldArray, selectByCarSlip, clearFieldeArray, lockObjectArray, controllerName, errmsgFlag, func) {

    if (selectByCarSlip == null || selectByCarSlip == '') {
        selectByCarSlip = "0";
    }

    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
        for (i = 0; i < lockObjectArray.length; i++) {
            document.getElementById(lockObjectArray[i]).disabled = true;
        }
    }
    if (document.getElementById(vinField).value != null && document.getElementById(vinField).value == '') {
        resetEntity();
        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
            for (i = 0; i < lockObjectArray.length; i++) {
                document.getElementById(lockObjectArray[i]).disabled = false;
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/SalesCar/GetSalesCarNumberFromVin",
            data: { vinCode: document.getElementById(vinField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {

                if (parseInt(data['count']) >= 2) {
                    // 同じ車台番号のレコードが2件以上ある場合、メッセージを表示し、検索ダイアログを表示させる
                    alert("同じ車台番号が2件以上存在します。検索ダイアログから指定して下さい。");

                    // Mod 2022/01/08 yano #4121
                    // Mod 2017/05/10 arc yano #3762

                    var callback = function () { if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100); }
                    openSearchDialog('salesCarNumField', 'Vin', '/SalesCar/CriteriaDialog?vin=' + document.getElementById(vinField).value, null, null, null, null, callback);
                    // 100ミリ秒後、フォーカスを車台番号に持っていく
                    // Mod 2017/05/10 arc yano #3762
                    if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100);
 
                } else if (parseInt(data['count']) == 0) {

                    //Mod 2016/08/23 arc yano #3621
                    //エラーメッセージを表示する場合
                    if (typeof (errmsgFlag) == 'undefined' || errmsgFlag == null || errmsgFlag == true) {

                        // レコードが0件の場合、メッセージを表示させる
                        alert("マスタに存在しません");
                        resetEntity();
                        document.getElementById(vinField).value = '';
                        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                            for (i = 0; i < lockObjectArray.length; i++) {
                                document.getElementById(lockObjectArray[i]).disabled = false;
                            }
                        }
                    }
                   
                    return false;

                } else {
                    // 車台番号が1件のみの場合、管理番号項目に値を設定
                    if (data['vin'] == null) {
                        document.getElementById(salesCarNumField).value = '';
                    } else {
                        document.getElementById(salesCarNumField).value = data['salesCarNumber'] == null ? '' : data['salesCarNumber'];
                    }
                }

                // 管理番号から車両情報を取得・表示させる
                // MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面用にコントローラを切替え
                if (controllerName == null) controllerName = 'SalesCar';
                GetMasterDetailFromCode(salesCarNumField, nameFieldArray, controllerName, selectByCarSlip, lockObjectArray, null, func);       //Mod 2017/05/10 arc yano #3762

                return true;

            }
            ,
            error: function () {  //通信失敗時
                alert("車両情報の取得に失敗しました。");
                resetEntity();
                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                    for (i = 0; i < lockObjectArray.length; i++) {
                        document.getElementById(lockObjectArray[i]).disabled = false;
                    }
                }
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById(salesCarNumField).value = '';

        var cleartargetArray;
        if (clearFieldeArray != null) {
            cleartargetArray = clearFieldeArray;
        }
        else {
            cleartargetArray = nameFieldArray;
        }

        for (var j = 0; j < cleartargetArray.length; j++) {
            try {
                
                var target = document.getElementById(cleartargetArray[j]);

                var tagName = target.tagName.toLowerCase();

                //テキストボックス
                if (tagName == 'input') {
                    target.value = '';
                }
                //リストボックス
                else if (tagName == 'select') {
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                //その他(ラベル)
                else {
                    target.innerText = '';
                }

            } catch (e) {
                target.innerText = '';
            }
        }
    }
}


//-------------------------------------------------------------------------------------------
// Edit 2014/07/07 arc yano chrome対応 getElementbyIdのパラメータ変更(name値→id値)
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//-------------------------------------------------------------------------------------------
function GetEngineerFromCode(num, codeType) {
    if ((codeType == 'EmployeeCode' && (document.getElementById('line[' + num + ']_EmployeeCode').value == null || document.getElementById('line[' + num + ']_EmployeeCode').value == ''))
        || (codeType == 'EmployeeNumber' && (document.getElementById('line[' + num + ']_EmployeeNumber').value == null || document.getElementById('line[' + num + ']_EmployeeNumber').value == ''))) {
        document.getElementById('line[' + num + ']_EmployeeCode').value = "";
        document.getElementById('line[' + num + ']_EmployeeName').value = "";
        document.getElementById('line[' + num + ']_EmployeeNumber').value = "";
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Employee/GetMasterDetail",
            data: { code: codeType == 'EmployeeCode' ? document.getElementById('line[' + num + ']_EmployeeCode').value : document.getElementById('line[' + num + ']_EmployeeNumber').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['EmployeeCode'] == null) {
                    alert("マスタに存在しません");
                    document.getElementById('line[' + num + ']_EmployeeCode').value = "";
                    document.getElementById('line[' + num + ']_EmployeeName').value = "";
                    document.getElementById('line[' + num + ']_EmployeeNumber').value = "";
                    return false;
                } else {
                    document.getElementById('line[' + num + ']_EmployeeCode').value = data['EmployeeCode'] == null ? "" : data['EmployeeCode'];
                    document.getElementById('line[' + num + ']_EmployeeName').value = data['EmployeeName'] == null ? "" : data['EmployeeName'];
                    document.getElementById('line[' + num + ']_EmployeeNumber').value = data['EmployeeNumber'] == null ? "" : data['EmployeeNumber'];
                    return true;
                }
            }
               ,
            error: function () {  //通信失敗時
                alert("社員情報の取得に失敗しました。");
                return false;
            }
        });
    }
}
//---------------------------------------------------------------
// 機能：
//　　部品情報の取得
//
// 更新履歴：
//    2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//                              取得したデータをカンマ付に変換する
//　　2017/02/14 arc yano #3641 金額欄のカンマ表示対応
// 　 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力)
//                               各フィールドのidを渡すように修正
//                               GetPartsMaster2と処理を統合
//    2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------------------
function GetPartsMaster(idPartsNumber, idPartsNameJp, idQuantity, idUnitPrice, idPrice, idAmount, msgflg) {
    
    if (document.getElementById(idPartsNumber).value != null && document.getElementById(idPartsNumber).value == '') {
        resetInputValue(idPartsNameJp, idUnitPrice, idPrice, idAmount);

        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Parts/GetMasterDetail",
            data: { code: document.getElementById(idPartsNumber).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data['PartsNumber'] == null) {
                    if (typeof msgflg == 'undefined' || msgflg == true) {   //Mod 2015/11/09 arc yano #3291
                        alert("マスタに存在しません");
                        document.getElementById(idPartsNumber).value = '';
                    }
                    resetInputValue(idPartsNameJp, idUnitPrice, idPrice, idAmount);
                } else {
                    document.getElementById(idPartsNameJp).value = data['PartsNameJp'];
                    document.getElementById(idUnitPrice).value = ConversionWithComma(data['Cost']);     //2017/04/23 arc yano #3755
                    document.getElementById(idPrice).value = ConversionWithComma(data['Price']);        //2017/04/23 arc yano #3755

                    //Add 2017/02/14 arc yano #3641
                    //カンマ挿入
                    //InsertComma(document.getElementById(idPrice));
                    //InsertComma(document.getElementById(idUnitPrice));
                }
                calcPartsAmount(idQuantity, idUnitPrice, idAmount);
            }
            ,
            error: function () {  //通信失敗時
                alert("部品情報の取得に失敗しました。");

                resetInputValue(idPartsNameJp, idUnitPrice, idPrice, idAmount);
                
                return false;
            }
        });
    }
    function resetInputValue(idPartsNameJp,idUnitPrice, idPrice, idAmount) {
        document.getElementById(idPartsNameJp).value = '';
        document.getElementById(idUnitPrice).value = 0;
        document.getElementById(idPrice).value = 0;
        document.getElementById(idAmount).value = 0;
    }

}

/* //Del 2015/11/09 arc yano #3291 GetPartsMasterと処理がほぼ同じのため、統合
//---------------------------------------------------
// Edit 2015/03/17 arc iijima
// パーツマスタに入力したパーツナンバーがない場合アラートメッセージ表示
//---------------------------------------------------
function GetPartsMaster2() {
    if (document.getElementById('PartsNumber').value != null && document.getElementById('PartsNumber').value == '') {
        document.getElementById('PartsNameJp').value = '';
        document.getElementById('Price').value = 0;
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Parts/GetMasterDetail",
            data: { code: document.getElementById('PartsNumber').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data['PartsNumber'] == null) {
                    alert("マスタに存在しません");
                    document.getElementById('PartsNumber').value = '';
                    document.getElementById('PartsNameJp').value = '';
                    document.getElementById('Price').value = 0;
                } else {
                    document.getElementById('PartsNameJp').value = data['PartsNameJp'];
                    document.getElementById('Price').value = data['Cost'];
                }
                calcPartsAmount2();
            }
            ,
            error: function () {  //通信失敗時
                alert("部品情報の取得に失敗しました。");
                return false;
            }
        });
    }
}
*/
//---------------------------------------------------
// Mod 2014/07/23 arc yano
// ①getElementByIdのパラメータをnameからidに変更
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function GetBranchNameFromBankCode() {
    if (document.getElementById('pay_BankCode').value == null || document.getElementById('pay_BankCode').value == '') {
        alert('銀行コードが指定されていません');
        document.getElementById('pay_BankCode').focus();
        return false;
    }
    if (document.getElementById('pay_BranchCode').value != null && document.getElementById('pay_BranchCode').value == '') {
        document.getElementById('pay_BranchName').innerText = '';
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Branch/GetMaster",
            data: { bankCode: document.getElementById('pay_BankCode').value, branchCode: document.getElementById('pay_BranchCode').value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しません");
                    document.getElementById('pay_BranchName').innerText = '';
                    document.getElementById('pay_BranchCode').value = '';

                    return false;
                } else {
                    document.getElementById('pay_BranchName').innerText = data.Name;
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("支店情報の取得に失敗しました。");
                return false;
            }
        });
    }
}
//-------------------------------------------------------------------------------------------------------------------------------
// Mod 2019/02/14 yano #3978 【入金管理】入金事業所、入金口座を編集できない
// パラメータを配列に変更(param[0]…idofficecode, param[1]…idofficename, param[2]…idcashaccountcode, pram[3]…isblank
// Mod 2015/03/25 arc yano　現金出納帳出力の追加
// disableパラメータの追加
// Mod 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//------------------------------------------------------------------------------------------------------------------------------
function GetCashAccountCode(param, data, disable) {


    // Mod 2019/02/14 yano #3978
    var officeCode = document.getElementById(param[0]).value;
    var cashAccountCode = document.getElementById(param[2]);
    var isblank = (param[3] != undefined ? param[3] : true);

    //var officeCode = document.getElementById('OfficeCode').value;
    //var cashAccountCode = document.getElementById('CashAccountCode');

    if (officeCode == null || officeCode == '') {
        officeCode.value = '';
        resetEntity();
        if (disable != undefined) {
            SetDisabled(disable, true, null);
        }
        return false;
    }
    if (officeCode != null && officeCode != '') {
        $.ajax({
            type: "POST",
            url: "/CashAccount/GetMasterDetail",
            data: { code: officeCode },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しません");
                    resetEntity();
                    document.getElementById(param[0]).focus();  //Mod 2019/02/14 yano #3978
                    //document.getElementById('OfficeCode').focus();

                    if (disable != undefined) {
                        SetDisabled(disable, true, null);
                    }

                    return false;
                }

                removeNodes(cashAccountCode);

                createNodes(cashAccountCode, data, isblank);    //Mod 2019/02/14 yano #3978
                document.getElementById(param[1]).innerText = data.Name;    //Mod 2019/02/14 yano #3978
                //document.getElementById('OfficeName').innerText = data.Name;

                if (disable != undefined) {
                    SetDisabled(disable, false, null);
                }
                return true;
            }
            ,
            error: function () {  //通信失敗時
                alert("現金口情報の取得に失敗しました。");
                if (disable != undefined) {
                    SetDisabled(disable, false, null);
                }
                return false;
            }
        });
    }
    function resetEntity() {
        //Mod 2019/02/14 yano #3978
        document.getElementById(param[0]).value = '';
        document.getElementById(param[1]).innerText = '';
        //document.getElementById('OfficeCode').value = '';
        //document.getElementById('OfficeName').innerText = '';
        removeNodes(cashAccountCode);
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function GetCustomerClaimWithClaimable(customerClaimCodeField, customerclaimNameField, paymentKindCodeField) {
    var customerClaimCode = document.getElementById(customerClaimCodeField);
    var customerClaimName = document.getElementById(customerclaimNameField);
    var paymentKindCode = document.getElementById(paymentKindCodeField);

    if (customerClaimCode.value == null || customerClaimCode.value == '') {
        resetEntity();
        return false;
    }

    $.ajax({
        type: "POST",
        url: "/CustomerClaim/GetMasterWithClaimable",
        data: { code: customerClaimCode.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code == null) {
                alert("マスタに存在しません");
                resetEntity();
                customerClaimCode.focus();
                return false;
            }
            removeNodes(paymentKindCode);
            createNodes(paymentKindCode, data, true);
            customerClaimName.innerText = data.Name;
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("決済種別情報の取得に失敗しました。");
            return false;
        }
    });
    function resetEntity() {
        customerClaimCode.value = '';
        customerClaimName.innerText = '';
        removeNodes(paymentKindCode);
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
// ブランドから車種リストを取得し、リストボックスにセットする
function GetCarList() {
    var obj = document.getElementById('CarBrandCode');
    if (obj.selectedIndex < 0) {
        removeNodes(document.getElementById('CarCode'));
        return false;
    }
    var carBrandCode = obj.options[obj.selectedIndex].value;
    $.ajax({
        type: "POST",
        url: "/Car/GetMasterList",
        data: { code: carBrandCode },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code == null) {
                removeNodes(document.getElementById('CarCode'));
                return false;
            }
            removeNodes(document.getElementById('CarCode'));
            createNodes(document.getElementById('CarCode'), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("車種情報の取得に失敗しました。");
            return false;
        }
    });
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
// 車種コードから型式リストを取得してリストボックスにセットする
function GetModelNameList() {
    var obj = document.getElementById('CarCode');
    if (obj.selectedIndex < 0) {
        removeNodes(document.getElementById('ModelName'));
        return false;
    }
    var carCode = obj.options[obj.selectedIndex].value;
    $.ajax({
        type: "POST",
        url: "/CarGrade/GetModelNameList",
        data: { carCode: carCode },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code == null) {
                removeNodes(document.getElementById('ModelName'));
                displaySearchList();
                return false;
            }
            removeNodes(document.getElementById('ModelName'));
            createNodes(document.getElementById('ModelName'), data, false);
            displaySearchList();
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("型式情報の取得に失敗しました。");
            return false;
        }
    });
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
// 車両カラーマスタを取得する
function GetCarColorMasterFromCode(
    codeField1,
    codeField2,
    nameField,
    controllerName) {
    if (document.getElementById(codeField2).value != null && document.getElementById(codeField2).value == '') {
        resetEntity();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/CarColor/GetMaster2",
            data: { carGradeCode: document.getElementById(codeField1).value, carColorCode: document.getElementById(codeField2).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しないか、選択不可能です");
                    resetEntity();
                    document.getElementById(codeField2).focus();
                    return false;
                } else {
                    document.getElementById(codeField2).value = data.Code2;
                    document.getElementById(nameField).value = data.Name;
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("車両カラー情報の取得に失敗しました。");
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById(codeField2).value = '';
        document.getElementById(nameField).value = '';
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
// 現金出納帳で伝票番号を入力したときに請求先一覧をセットする
function GetCustomerClaimList() {
    var slipNumber = document.getElementById('SlipNumber');
    var customerClaim = document.getElementById('CustomerClaimCode');
    var journalType = document.getElementById('JournalType');

    //Del 2016/05/17 arc nakayama #3539
    // 出金時は無視
    //if (journalType && journalType.options[journalType.selectedIndex].value == '002') return true;

    //2016/05/17 arc nakayama #3539 入出金区分　伝票番号が入力されていいる場合は編集不可
    //伝票番号の有無で画面の入出金区分の活性/非活性を切り替える
    if (slipNumber.value == null || slipNumber.value == '') {
        resetEntity();
        return false;
    } else {

        //伝票番号が入っている場合は、入出金区分 = 「入金(001)」かつ非活性に変更
        //Mod 2016/07/08 arc nakayama #3613_【サービス売掛金】　現金出納帳で伝票を番号を入力すると入出金区分を「入金」に変更する処理を解除する
        //journalType.value = '001';          //入出金区分 = 「001」

        var option = journalType.getElementsByTagName('option');
        for (i = 0; i < option.length; i++) {
            if (option[i].value == journalType.value) {
                option[i].selected = true;
            }
            else {
                option[i].selected = false;
            }
        }

        var hdJournalType = document.getElementById('hdJournalType');

        if (hdJournalType != null) {
            hdJournalType.value = journalType.value;
        }

        //入出金区分のドロップダウンを非活性
        //Mod 2016/07/08 arc nakayama #3613_【サービス売掛金】　現金出納帳で伝票を番号を入力すると入出金区分を「入金」に変更する処理を解除する
        //document.getElementById('JournalType').disabled = true;
    }

    $.ajax({
        type: "POST",
        url: "/CashJournal/GetMasterWithClaim",
        data: { slipNumber: slipNumber.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code == null) {
                alert("伝票番号が正しくありません");
                resetEntity();
                slipNumber.focus();
                return false;
            }
            if (data.DataList == null || data.DataList.length == 0) {
                alert("請求先情報が伝票に登録されていません");
                removeNodes(customerClaim);
                return false;
            }
            removeNodes(customerClaim);
            createNodes(customerClaim, data, true);

            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("請求先情報の取得に失敗しました。");
            return false;
        }
    });
    function resetEntity() {
        slipNumber.value = '';
        //Mod 2016/07/08 arc nakayama #3613_【サービス売掛金】　現金出納帳で伝票を番号を入力すると入出金区分を「入金」に変更する処理を解除する
        //document.getElementById("JournalType").disabled = false;
        removeNodes(customerClaim);
    }
}

//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function GetCustomerReceiptBalance() {
    var slipNumber = document.getElementById('SlipNumber');
    var customerClaim = document.getElementById('CustomerClaimCode');
    if (slipNumber.value == null || slipNumber.value == '') {
        resetEntity();
        return false;
    }
    $.ajax({
        type: "POST",
        url: "/Deposit/GetReceiptBalance",
        data: { slipNumber: slipNumber.value, customerClaimCode: customerClaim.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data != null) {
                document.getElementById('ReceiptBalance').innerText = currency(data);
                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("入金予定情報の取得に失敗しました。");
            return false;
        }
    });
    function resetEntity() {
        slipNumber.value = '';
        removeNodes(customerClaim);
    }
}
//-------------------------------------------------------------------------------------------------
//機能：
//  仕入先名の取得と支払先名の設定
//更新履歴：
// 2015/11/09 arc yano #3291 
//                     ①引数を渡すように設定　支払先取得の処理を別メソッドにし、パラメータで渡す
// ????/??/?? ??? 新規作成
//---------------------------------------------------------------------------------------------------
function GetSupplierName(strSupplierCode, strSupplierName, strSupplierPaymentCode, strSupplierPaymentName) {

    //支払先名取得処理のパラメータの作成
    var param = [strSupplierPaymentCode, strSupplierPaymentName, strSupplierCode];

    //仕入先名の取得(支払先設定のメソッドを引数として渡す)
    var ret = GetNameFromCode(strSupplierCode, strSupplierName, 'Supplier', 'false', SetSupplierPayment, param)

    /* Del 2015/11/09 arc yano #3291
    if (ret == true) {
        //支払先名の取得
        if (confirm('支払先にも同じコードをセットして宜しいですか？')) {
            document.getElementById(strSupplierPaymentCode).value = document.getElementById(strSupplierCode).value;
            GetNameFromCode(strSupplierPaymentCode, strSupplierPaymentName, 'SupplierPayment');
        }
    }
    */
}

//---------------------------------------------------------------------------------------------------
//機能：
//  支払先名の設定
//更新履歴：
// 2019/08/30 yano #4003 【部品発注入力】仕入先入力時の支払先デフォルト入力のダイアログ表示の廃止
// 2015/11/09 arc yano #3291 新規作成
//---------------------------------------------------------------------------------------------------
function SetSupplierPayment(param) {
  
    //Mod 2019/08/30 yano #4003
    //if (confirm('支払先にも同じコードをセットして宜しいですか？')) {
        document.getElementById(param[0]).value = document.getElementById(param[2]).value;
        GetNameFromCode(param[0], param[1], 'SupplierPayment');
    //}
}
//---------------------------------------------------
// Add 2014/10/16 arc amii サブシステム移行対応
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
// Add 2014/10/16 arc サブシステム仕訳機能移行対応
//---------------------------------------------------
//部門コードからマスタを検索し、名称を取得する
function GetDepartmentNameFromCode(
    CodeField,
    NameField
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Department/GetMaster2",
            data: { code: document.getElementById(CodeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    alert("入力された部門は選択できません。");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}


//コードからマスタを検索し、名称を取得する (マスタに存在しない場合アラートは返さない、入力されたコードは削除しない)
//------------------------------------------------------------------------------------
//ADD 2015/01/30 arc ishii　入金実績リストサブシステム移行対応
//コードがマスタに存在するか検索　
//-----------------------------------------------------------------------------------
function GetNameFromCodeNoAlert(
    CodeField,
    NameField,
    ControllerName
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
         $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMaster",
            data: { code: document.getElementById(CodeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                
                if (data.Code == null) {
                    //alert("入力された部門は選択できません。");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    //document.getElementById(CodeField).value = '';
                    //document.getElementById(CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}

//---------------------------------------------------
// Add 2014/10/16 arc amii サブシステム移行対応
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
// Add 2014/10/27 arc サブシステム仕訳機能移行対応
//---------------------------------------------------
//事業所コードからマスタを検索し、名称を取得する
function GetOfficeNameFromCode(
    CodeField,
    NameField,
    DivField
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/Office/GetMaster2",
            data: { code: document.getElementById(CodeField).value, divType: document.getElementById(DivField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    alert("入力された事業所は選択できません。");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}


//------------------------------------------------------
// 合計金額を計算するロジックを記載
//------------------------------------------------------

/*@cc_on_d = document; eval('var document=_d')@*/

/*---------------------------------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： 車両伝票合計金額計算                                                                                                                     */
/* 作成日：                                                                                                                                          */
/* 更新日：                                                                                                                                          */
/*          2023/10/05 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除                                       */
/*          2023/09/05 yano #4162 インボイス対応                                                                                                     */
/*          2023/08/15 yano #4176 販売諸費用の修正                                                                                                   */
/*          2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加                                                                           */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  オプション合計の計算処理を外だし                                 */
/*          2018/06/08 arc yano #3901車両伝票入力　販売区分で「AA」「依廃」を選択後に金額欄に金額を入力しても０になる                                */
/*          2018/03/26 arc yano #3756 【車】車両伝票の販売区分「AA」選択時に「販売諸費用」 「非課税」項目の金額を0円へ                               */
/*          2017/04/24 arc yano #3755 金額欄の入力時のカーソル位置の不具合 カンマ除去・挿入処理はテキストの入力値ではなく、内部に保持した数値で行う  */ 
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                                                                 */
/*          2016/09/02 arc nakayama #3630_【製造】車両売掛金対応 下取車の充当金を合計金額から引かないようにする                                      */
/*          2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正                                                               */
/*                                                                                                                                                   */
/*---------------------------------------------------------------------------------------------------------------------------------------------------*/
function calcTotalAmount() {//Mod 2019/09/04 yano #4011

    //Del2019/09/04 yano ref コメントアウトされていた処理の削除
    //Mod 2017/04/24 arc yano #3755

    //Del 2018/06/08 arc yano #3901　
   
    var SalesPrice = ConversionExceptComma($('#SalesPrice').val());
    var SalesTax = ConversionExceptComma($('#SalesTax').val());
    var SalesPriceWithTax = ConversionExceptComma($('#SalesPriceWithTax').val());

    //値引金額
    var DiscountAmount = ConversionExceptComma($('#DiscountAmount').val());
    var DiscountTax = ConversionExceptComma($('#DiscountTax').val());
    var DiscountAmountWithTax = ConversionExceptComma($('#DiscountAmountWithTax').val());


    //本体価格または値引金額が変更されたときだけ自動計算
    //Del2019/09/04 yano ref

    var ShopOptionAmount = ConversionExceptComma($('#ShopOptionAmount').val());
    var MakerOptionAmount = ConversionExceptComma($('#MakerOptionAmount').val());
    var ShopOptionTaxAmount = ConversionExceptComma($('#ShopOptionTaxAmount').val());
    var MakerOptionTaxAmount = ConversionExceptComma($('#MakerOptionTaxAmount').val());


    // 2019/09/04 yano #4011
    //for (var i = 0; i < 25; i++) {
    //    if (document.getElementById('line[' + i + ']_OptionType') == null) continue;

    //    var amount = ConversionExceptComma(document.getElementById('line[' + i + ']_Amount').value);
    //    var tax = ConversionExceptComma(document.getElementById('line[' + i + ']_TaxAmount').value);

    //    //Del 2019/09/12 ref コメントアウトされていた処理を削除 
        
    //    switch (document.getElementById('line[' + i + ']_OptionType').value) {
    //        case '001':
    //            ShopOptionAmount += amount;
    //            ShopOptionTaxAmount += tax;
    //            break;
    //        case '002':
    //            MakerOptionAmount += amount;
    //            MakerOptionTaxAmount += tax;
    //            break;
    //    }
    //}

    ////オプション合計表示
    //$('#OptionTotalAmount').val(ConversionWithComma(ShopOptionAmount + MakerOptionAmount));
    //$('#OptionTotalTaxAmount').val(ConversionWithComma(ShopOptionTaxAmount + MakerOptionTaxAmount));
    //$('#OptionTotalAmountWithTax').val(ConversionWithComma(ShopOptionAmount + MakerOptionAmount + ShopOptionTaxAmount + MakerOptionTaxAmount));

    ////販売店オプション合計
    //$('#ShopOptionAmount').val(ConversionWithComma(ShopOptionAmount));
    //$('#ShopOptionTaxAmount').val(ConversionWithComma(ShopOptionTaxAmount));
    //$('#ShopOptionAmountWithTax').val(ConversionWithComma(ShopOptionAmount + ShopOptionTaxAmount));

    ////メーカーオプション合計
    //$('#MakerOptionAmount').val(ConversionWithComma(MakerOptionAmount));
    //$('#MakerOptionTaxAmount').val(ConversionWithComma(MakerOptionTaxAmount));
    //$('#MakerOptionAmountWithTax').val(ConversionWithComma(MakerOptionAmount + MakerOptionTaxAmount));

    //課税対象額(車両本体価格-値引金額+メーカーオプション合計)
    var TaxationAmount = SalesPrice - DiscountAmount + MakerOptionAmount;
    var TaxationAmountTax = SalesTax - DiscountTax + MakerOptionTaxAmount;
    $('#TaxationAmount').val(ConversionWithComma(TaxationAmount));
    $('#TaxationAmountTax').val(ConversionWithComma(TaxationAmountTax));
    $('#TaxationAmountWithTax').val(ConversionWithComma(TaxationAmount + TaxationAmountTax));

    //車両販売価格合計
    var SubTotalAmount = SalesPrice - DiscountAmount + ShopOptionAmount + MakerOptionAmount;
    var SubTotalTaxAmount = SalesTax - DiscountTax + ShopOptionTaxAmount + MakerOptionTaxAmount;
    var SubTotalAmountWithTax = (SalesPrice + SalesTax) - (DiscountAmount + DiscountTax) + (ShopOptionAmount + ShopOptionTaxAmount) + (MakerOptionAmount + MakerOptionTaxAmount);

    $('#SubTotalAmount').val(ConversionWithComma(SubTotalAmount));
    $('#SubTotalTaxAmount').val(ConversionWithComma(SubTotalTaxAmount));
    $('#SubTotalAmountWithTax').val(ConversionWithComma(SubTotalAmountWithTax));

    //--税金等ここから
    //自動車税種別割
    var CarTax = ConversionExceptComma($('#CarTax').val());

    //自動車重量税
    var CarWeightTax = ConversionExceptComma($('#CarWeightTax').val());

    //自賠責保険
    var CarLiabilityInsurance = ConversionExceptComma($('#CarLiabilityInsurance').val());

    //自動車税環境性能割
    var AcquisitionTax = ConversionExceptComma($('#AcquisitionTax').val());

    //Mod 2023/01/11 yano #4158
    ////任意保険料(新規・自社継続以外の場合のみ加算）
    //var VoluntaryInsuranceAmount = 0;
    //if ($('#VoluntaryInsuranceType').val() == '002' || $('#VoluntaryInsuranceType').val() == '003') {
    //    VoluntaryInsuranceAmount = ConversionExceptComma($('#VoluntaryInsuranceAmount').val());
    //    $('#VoluntaryInsuranceAmount2').val(ConversionWithComma(VoluntaryInsuranceAmount));
    //}

    //税金等合計
    var TaxFreeTotalAmount = CarTax + CarWeightTax + CarLiabilityInsurance + AcquisitionTax;    //Mod 2023/01/11 yano #4158
    //var TaxFreeTotalAmount = CarTax + CarWeightTax + CarLiabilityInsurance + AcquisitionTax + VoluntaryInsuranceAmount;
    var TaxFreeTotalTaxAmount = 0;
    $('#TaxFreeTotalAmount').val(ConversionWithComma(TaxFreeTotalAmount));
    $('#TaxFreeTotalAmountWithTax').val(ConversionWithComma(TaxFreeTotalAmount + TaxFreeTotalTaxAmount));

    //--その他非課税

    // 車庫証明証紙代
    var ParkingSpaceCost = ConversionExceptComma($('#ParkingSpaceCost').val());

    // 検査登録印紙代
    var InspectionRegistCost = ConversionExceptComma($('#InspectionRegistCost').val());

    // ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(一般)
    var NumberPlateCost = ConversionExceptComma($('#NumberPlateCost').val());

    // 下取車登録印紙代
    var TradeInCost = ConversionExceptComma($('#TradeInCost').val());

    // ﾘｻｲｸﾙ預託金
    var RecycleDeposit = ConversionExceptComma($('#RecycleDeposit').val());

    // ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望)
    var RequestNumberCost = ConversionExceptComma($('#RequestNumberCost').val());

    //非課税自由項目
    var TaxFreeFieldValue = ConversionExceptComma($('#TaxFreeFieldValue').val());

     // Add 2014/07/25 arc amii 課題対応 #3018 収入印紙代と下取自動車税預り金項目を追加
    //収入印紙代
    var RevenueStampCost = ConversionExceptComma($('#RevenueStampCost').val());

    //下取自動車税種別割預り金
    var TradeInCarTaxDeposit = ConversionExceptComma($('#TradeInCarTaxDeposit').val());

    //Add 2023/01/11 yano #4158
    //任意保険
    var VoluntaryInsuranceAmount = ConversionExceptComma($('#VoluntaryInsuranceAmount').val());

    //非課税合計金額
    var OtherCostTotalAmount = ParkingSpaceCost + InspectionRegistCost + NumberPlateCost + TradeInCost + RecycleDeposit + RequestNumberCost + TaxFreeFieldValue + RevenueStampCost + TradeInCarTaxDeposit + (VoluntaryInsuranceAmount ?? 0);  //Mod 2023/01/11 yano #4158
    //var OtherCostTotalAmount = ParkingSpaceCost + InspectionRegistCost + NumberPlateCost + TradeInCost + RecycleDeposit + RequestNumberCost + TaxFreeFieldValue + RevenueStampCost + TradeInCarTaxDeposit;
    var OtherCostTotalTaxAmount = 0;
    $('#OtherCostTotalAmount').val(ConversionWithComma(OtherCostTotalAmount));
    $('#OtherCostTotalAmountWithTax').val(ConversionWithComma(OtherCostTotalAmount + OtherCostTotalTaxAmount));


    //-------------------------------------------------------------------------
    // --販売諸費用
    //-------------------------------------------------------------------------
    //Mod 2023/08/15 yano #4176 計算元を税抜→税込に変更

    // 検査登録手続代行費用
    var InspectionRegistFeeWithTax = ConversionExceptComma(($('#InspectionRegistFeeWithTax').val()));
    var InspectionRegistFeeTax = calcTaxAmountFromTotalAmount(InspectionRegistFeeWithTax);
    $('#InspectionRegistFeeTax').val(InspectionRegistFeeTax == 0 ? '' : ConversionWithComma(InspectionRegistFeeTax));
    $('#InspectionRegistFee').val(ConversionWithComma(InspectionRegistFeeWithTax - InspectionRegistFeeTax));

    // 希望ナンバー申請手数料
    var RequestNumberFeeWithTax = ConversionExceptComma($('#RequestNumberFeeWithTax').val());
    var RequestNumberFeeTax = calcTaxAmountFromTotalAmount(RequestNumberFeeWithTax);
    $('#RequestNumberFeeTax').val(RequestNumberFeeTax == 0 ? '' : ConversionWithComma(RequestNumberFeeTax));
    $('#RequestNumberFee').val(ConversionWithComma(RequestNumberFeeWithTax - RequestNumberFeeTax));

    // 納車費用
    var PreparationFeeWithTax = ConversionExceptComma($('#PreparationFeeWithTax').val());
    var PreparationFeeTax = calcTaxAmountFromTotalAmount(PreparationFeeWithTax);
    $('#PreparationFeeTax').val(PreparationFeeTax == 0 ? '' : ConversionWithComma(PreparationFeeTax));
    $('#PreparationFee').val(ConversionWithComma(PreparationFeeWithTax - PreparationFeeTax));

    //Mod 2023/10/5 yano #4184
    //// 中古車点検・整備費用
    //var TradeInMaintenanceFeeWithTax = ConversionExceptComma($('#TradeInMaintenanceFeeWithTax').val());
    //var TradeInMaintenanceFeeTax = calcTaxAmountFromTotalAmount(TradeInMaintenanceFeeWithTax);
    //$('#TradeInMaintenanceFeeTax').val(TradeInMaintenanceFeeTax == 0 ? '' : ConversionWithComma(TradeInMaintenanceFeeTax));
    //$('#TradeInMaintenanceFee').val(ConversionWithComma(TradeInMaintenanceFeeWithTax - TradeInMaintenanceFeeTax));

    // 県外登録手続代行費用
    var FarRegistFeeWithTax = ConversionExceptComma($('#FarRegistFeeWithTax').val());
    var FarRegistFeeTax = calcTaxAmountFromTotalAmount(FarRegistFeeWithTax);
    $('#FarRegistFeeTax').val(FarRegistFeeTax == 0 ? '' : ConversionWithComma(FarRegistFeeTax));
    $('#FarRegistFee').val(ConversionWithComma(FarRegistFeeWithTax - FarRegistFeeTax));

    // 下取車所有権解除手続費用
    var TradeInFeeWithTax = ConversionExceptComma($('#TradeInFeeWithTax').val());
    var TradeInFeeTax = calcTaxAmountFromTotalAmount(TradeInFeeWithTax);
    $('#TradeInFeeTax').val(TradeInFeeTax == 0 ? '' : ConversionWithComma(TradeInFeeTax));
    $('#TradeInFee').val(ConversionWithComma(TradeInFeeWithTax - TradeInFeeTax));

    // リサイクル資金管理料
    var RecycleControlFeeWithTax = ConversionExceptComma($('#RecycleControlFeeWithTax').val());
    var RecycleControlFeeTax = calcTaxAmountFromTotalAmount(RecycleControlFeeWithTax);
    $('#RecycleControlFeeTax').val(RecycleControlFeeTax == 0 ? '' : ConversionWithComma(RecycleControlFeeTax));
    $('#RecycleControlFee').val(ConversionWithComma(RecycleControlFeeWithTax - RecycleControlFeeTax));

    // 管轄外登録手続費用
    var OutJurisdictionRegistFeeWithTax = ConversionExceptComma($('#OutJurisdictionRegistFeeWithTax').val());
    var OutJurisdictionRegistFeeTax = calcTaxAmountFromTotalAmount(OutJurisdictionRegistFeeWithTax);
    $('#OutJurisdictionRegistFeeTax').val(OutJurisdictionRegistFeeTax == 0 ? '' : ConversionWithComma(OutJurisdictionRegistFeeTax));
    $('#OutJurisdictionRegistFee').val(ConversionWithComma(OutJurisdictionRegistFeeWithTax - OutJurisdictionRegistFeeTax));

    // 車庫証明手続代行費用
    var ParkingSpaceFeeWithTax = ConversionExceptComma($('#ParkingSpaceFeeWithTax').val());
    var ParkingSpaceFeeTax = calcTaxAmountFromTotalAmount(ParkingSpaceFeeWithTax);
    $('#ParkingSpaceFeeTax').val(ParkingSpaceFeeTax == 0 ? '' : ConversionWithComma(ParkingSpaceFeeTax));
    $('#ParkingSpaceFee').val(ConversionWithComma(ParkingSpaceFeeWithTax - ParkingSpaceFeeTax));

    //Mod 2023/10/5 yano #4184
    //// 中古車継承整備費用
    //var InheritedInsuranceFeeWithTax = ConversionExceptComma($('#InheritedInsuranceFeeWithTax').val());
    //var InheritedInsuranceFeeTax = calcTaxAmountFromTotalAmount(InheritedInsuranceFeeWithTax);
    //$('#InheritedInsuranceFeeTax').val(InheritedInsuranceFeeTax == 0 ? '' : ConversionWithComma(InheritedInsuranceFeeTax));
    //$('#InheritedInsuranceFee').val(ConversionWithComma(InheritedInsuranceFeeWithTax - InheritedInsuranceFeeTax));

    //課税自由項目
    var TaxationFieldValueWithTax = ConversionExceptComma($('#TaxationFieldValueWithTax').val());
    var TaxationFieldValueTax = calcTaxAmountFromTotalAmount(TaxationFieldValueWithTax);
    $('#TaxationFieldValueTax').val(TaxationFieldValueTax == 0 ? '' : ConversionWithComma(TaxationFieldValueTax));
    $('#TaxationFieldValue').val(ConversionWithComma(TaxationFieldValueWithTax - TaxationFieldValueTax));

    //Add 2023/09/05  #4162
    //自動車税種別割未経過相当額
    var CarTaxUnexpiredAmountWithTax = ConversionExceptComma($('#CarTaxUnexpiredAmountWithTax').val());
    var CarTaxUnexpiredAmountTax = calcTaxAmountFromTotalAmount(CarTaxUnexpiredAmountWithTax);
    $('#CarTaxUnexpiredAmountTax').val(CarTaxUnexpiredAmountTax == 0 ? '' : ConversionWithComma(CarTaxUnexpiredAmountTax));
    $('#CarTaxUnexpiredAmount').val(ConversionWithComma(CarTaxUnexpiredAmountWithTax - CarTaxUnexpiredAmountTax));

    //Add 2023/09/05  #4162
    //自賠責未経過相当額
    var CarLiabilityInsuranceUnexpiredAmountWithTax = ConversionExceptComma($('#CarLiabilityInsuranceUnexpiredAmountWithTax').val());
    var CarLiabilityInsuranceUnexpiredAmountTax = calcTaxAmountFromTotalAmount(CarLiabilityInsuranceUnexpiredAmountWithTax);
    $('#CarLiabilityInsuranceUnexpiredAmountTax').val(CarLiabilityInsuranceUnexpiredAmountTax == 0 ? '' : ConversionWithComma(CarLiabilityInsuranceUnexpiredAmountTax));
    $('#CarLiabilityInsuranceUnexpiredAmount').val(ConversionWithComma(CarLiabilityInsuranceUnexpiredAmountWithTax - CarLiabilityInsuranceUnexpiredAmountTax));

    //販売諸費用(税込)合計額
    //Mdo 2023/10/5 yano #4184
    var SalesCostTotalAmountWithTax = InspectionRegistFeeWithTax + RequestNumberFeeWithTax
        + PreparationFeeWithTax + FarRegistFeeWithTax
        + TradeInFeeWithTax + RecycleControlFeeWithTax
        + OutJurisdictionRegistFeeWithTax + ParkingSpaceFeeWithTax
        + CarTaxUnexpiredAmountWithTax + CarLiabilityInsuranceUnexpiredAmountWithTax //Mod 2023/09/05  #4162
        + TaxationFieldValueWithTax;

    //販売諸費用消費税合計額
    var SalesCostTotalTaxAmount = InspectionRegistFeeTax + RequestNumberFeeTax
        + PreparationFeeTax + FarRegistFeeTax + TradeInFeeTax
        + RecycleControlFeeTax + OutJurisdictionRegistFeeTax
        + ParkingSpaceFeeTax + CarTaxUnexpiredAmountTax
        + CarLiabilityInsuranceUnexpiredAmountTax //Mod 2023/09/05  #4162
        + TaxationFieldValueTax;

    //var SalesCostTotalAmountWithTax = InspectionRegistFeeWithTax + RequestNumberFeeWithTax
    //    + PreparationFeeWithTax + TradeInMaintenanceFeeWithTax
    //    + FarRegistFeeWithTax + TradeInFeeWithTax
    //    + RecycleControlFeeWithTax + OutJurisdictionRegistFeeWithTax
    //    + ParkingSpaceFeeWithTax + InheritedInsuranceFeeWithTax
    //    + CarTaxUnexpiredAmountWithTax + CarLiabilityInsuranceUnexpiredAmountWithTax //Mod 2023/09/05  #4162
    //    + TaxationFieldValueWithTax;

    ////販売諸費用消費税合計額
    //var SalesCostTotalTaxAmount = InspectionRegistFeeTax + RequestNumberFeeTax
    //    + PreparationFeeTax + TradeInMaintenanceFeeTax
    //    + FarRegistFeeTax + TradeInFeeTax
    //    + RecycleControlFeeTax + OutJurisdictionRegistFeeTax
    //    + ParkingSpaceFeeTax + InheritedInsuranceFeeTax
    //    + CarTaxUnexpiredAmountTax + CarLiabilityInsuranceUnexpiredAmountTax //Mod 2023/09/05  #4162
    //    + TaxationFieldValueTax;

    $('#SalesCostTotalAmountWithTax').val(ConversionWithComma(SalesCostTotalAmountWithTax));
    $('#SalesCostTotalTaxAmount').val(ConversionWithComma(SalesCostTotalTaxAmount));
    $('#SalesCostTotalAmount').val(ConversionWithComma(SalesCostTotalAmountWithTax - SalesCostTotalTaxAmount));

    //諸費用計
    var CostTotalAmount = (SalesCostTotalAmountWithTax - SalesCostTotalTaxAmount) + TaxFreeTotalAmount + OtherCostTotalAmount;
    var CostTotalTaxAmount = SalesCostTotalTaxAmount + TaxFreeTotalTaxAmount + OtherCostTotalTaxAmount;
   
    //Del 2023/10/05 yano #4184
   
    $('#CostTotalAmount').val(ConversionWithComma(CostTotalAmount));
    $('#CostTotalTaxAmount').val(ConversionWithComma(CostTotalTaxAmount));
    $('#CostTotalAmountWithTax').val(ConversionWithComma(CostTotalAmount + CostTotalTaxAmount));

    //消費税合計(課税対象額+販売店オプション消費税額+メーカーオプション消費税額+加装・加修消費税額+販売所費用消費税額)
    var TaxTotal = SalesTax - DiscountTax + ShopOptionTaxAmount + MakerOptionTaxAmount + SalesCostTotalTaxAmount;
    $('#TotalTaxAmount').val(ConversionWithComma(TaxTotal));

    var Total = SubTotalAmount + CostTotalAmount + TaxTotal;
    var TotalPrice = SubTotalAmount + CostTotalAmount;

    //現金販売価格
    $('#GrandTotalAmount').val(ConversionWithComma(Total));
    $('#TotalPrice').val(ConversionWithComma(TotalPrice));

    //下取充当額を計算

    // *** 1台目 *** 
    //下取車価格
    var TradeInAmount1 = ConversionExceptComma($('#TradeInAmount1').val());

    //下取車未払自動車税種別割
    var TradeInUnexpiredCarTax1 = ConversionExceptComma($('#TradeInUnexpiredCarTax1').val());

    //下取車リサイクル
    var TradeInRecyleAmount1 = ConversionExceptComma($('#TradeInRecycleAmount1').val());

    //下取車消費税
    var TradeInTax1 = calcTaxAmountFromTotalAmount(TradeInAmount1 - TradeInUnexpiredCarTax1 - TradeInRecyleAmount1);
    $('#TradeInTax1').val(ConversionWithComma(TradeInTax1));

    //下取車価格(税抜)
    var TradeInPrice1 = TradeInAmount1 - TradeInUnexpiredCarTax1 - TradeInRecyleAmount1 - TradeInTax1;
    $('#TradeInPrice1').val(ConversionWithComma(TradeInPrice1));

    //下取車残債
    var TradeInRemainDebt1 = ConversionExceptComma($('#TradeInRemainDebt1').val());

    //下取充当金(総額)
    var TradeInAppropriation1 = TradeInAmount1 - TradeInRemainDebt1;
    $('#TradeInAppropriation1').val(ConversionWithComma(TradeInAppropriation1));

    // *** 2台目 ***

    //下取車価格
    var TradeInAmount2 = ConversionExceptComma($('#TradeInAmount2').val());

    //下取車未払自動車税種別割
    var TradeInUnexpiredCarTax2 = ConversionExceptComma($('#TradeInUnexpiredCarTax2').val());

    //下取車リサイクル
    var TradeInRecyleAmount2 = ConversionExceptComma($('#TradeInRecycleAmount2').val());

    //下取車消費税
    var TradeInTax2 = calcTaxAmountFromTotalAmount(TradeInAmount2 - TradeInUnexpiredCarTax2 - TradeInRecyleAmount2);
    $('#TradeInTax2').val(ConversionWithComma(TradeInTax2));

    //下取車価格(税抜)
    var TradeInPrice2 = TradeInAmount2 - TradeInUnexpiredCarTax2 - TradeInRecyleAmount2 - TradeInTax2;
    $('#TradeInPrice2').val(ConversionWithComma(TradeInPrice2));

    //下取車残債
    var TradeInRemainDebt2 = ConversionExceptComma($('#TradeInRemainDebt2').val());

    //下取充当金(総額)
    var TradeInAppropriation2 = TradeInAmount2 - TradeInRemainDebt2;
    $('#TradeInAppropriation2').val(ConversionWithComma(TradeInAppropriation2));

    // *** 3台目 ***

    //下取車価格
    var TradeInAmount3 = ConversionExceptComma($('#TradeInAmount3').val());

    //下取車未払自動車税種別割
    var TradeInUnexpiredCarTax3 = ConversionExceptComma($('#TradeInUnexpiredCarTax3').val());

    //下取車リサイクル
    var TradeInRecyleAmount3 = ConversionExceptComma($('#TradeInRecycleAmount3').val());

    //下取車消費税
    var TradeInTax3 = calcTaxAmountFromTotalAmount(TradeInAmount3 - TradeInUnexpiredCarTax3 - TradeInRecyleAmount3);
    $('#TradeInTax3').val(ConversionWithComma(TradeInTax3));

    //下取車価格(税抜)
    var TradeInPrice3 = TradeInAmount3 - TradeInUnexpiredCarTax3 - TradeInRecyleAmount3 - TradeInTax3;
    $('#TradeInPrice3').val(ConversionWithComma(TradeInPrice3));

    //下取車残債
    var TradeInRemainDebt3 = ConversionExceptComma($('#TradeInRemainDebt3').val());

    //下取充当金(総額)
    var TradeInAppropriation3 = TradeInAmount3 - TradeInRemainDebt3;
    $('#TradeInAppropriation3').val(ConversionWithComma(TradeInAppropriation3));

    //下取車合計額
    var TradeInTotalAmount = TradeInAmount1 + TradeInAmount2 + TradeInAmount3
    $('#TradeInTotalAmount').val(ConversionWithComma(TradeInTotalAmount));

    //下取車消費税合計
    var TradeInTaxTotalAmount = TradeInTax1 + TradeInTax2 + TradeInTax3;
    $('#TradeInTaxTotalAmount').val(ConversionWithComma(TradeInTaxTotalAmount));

    //下取車未払自動車税種別割合計
    var TradeInUnexpiredCarTaxTotalAmount = TradeInUnexpiredCarTax1 + TradeInUnexpiredCarTax2 + TradeInUnexpiredCarTax3;
    $('#TradeInUnexpiredCarTaxTotalAmount').val(ConversionWithComma(TradeInUnexpiredCarTaxTotalAmount));

    //下取車リサイクル合計
    var TradeInRecycleTotalAmount = TradeInRecyleAmount1 + TradeInRecyleAmount2 + TradeInRecyleAmount3;
    $('#TradeInRecycleTotalAmount').val(ConversionWithComma(TradeInRecycleTotalAmount));

    //下取車残債合計
    var TradeInRemainDebtTotalAmount = TradeInRemainDebt1 + TradeInRemainDebt2 + TradeInRemainDebt3;
    $('#TradeInRemainDebtTotalAmount').val(ConversionWithComma(TradeInRemainDebtTotalAmount));

    //下取充当額合計
    var TradeInAppropriationTotalAmount = TradeInAppropriation1 + TradeInAppropriation2 + TradeInAppropriation3;
    $('#TradeInAppropriationTotalAmount').val(ConversionWithComma(TradeInAppropriationTotalAmount));

    //下取後支払総額
    var PaymentTotalAmount = Total - (TradeInTotalAmount - TradeInRemainDebtTotalAmount);
    $('#PaymentTotalAmount').val(ConversionWithComma(PaymentTotalAmount));

    var PaymentAmount = 0;

    for (var j = 0; j < 19; j++) {
        if (document.getElementById('pay[' + j + ']_Amount') == null) continue;
        if (document.getElementById('pay[' + j + ']_Amount').value != null && document.getElementById('pay[' + j + ']_Amount').value != '') {
            PaymentAmount += ConversionExceptComma(document.getElementById('pay[' + j + ']_Amount').value);
        }
    }

    //現金合計
    $('#PaymentCashTotalAmount').val(ConversionWithComma(PaymentAmount));

    //残価金額
    //var RemainAmountA = isNaN(parseInt($('#RemainAmountA').val())) ? 0 : parseInt($('#RemainAmountA').val());
    //var RemainAmountB = isNaN(parseInt($('#RemainAmountB').val())) ? 0 : parseInt($('#RemainAmountB').val());
    //var RemainAmountC = isNaN(parseInt($('#RemainAmountC').val())) ? 0 : parseInt($('#RemainAmountC').val());

    //ローンに関する金額の計算
    var PrevLoanPrincipalAmount = ConversionExceptComma($('#LoanPrincipalAmount').val());
    var PrevLoanFeeAmount = ConversionExceptComma($('#LoanFeeAmount').val());
    var PrevLoanTotalAmount = ConversionExceptComma($('#LoanTotalAmount').val());

    var LoanPrincipalAmount = PaymentTotalAmount - PaymentAmount;
    var LoanPrincipalA = PaymentTotalAmount - PaymentAmount;  //- RemainAmountA;
    var LoanPrincipalB = PaymentTotalAmount - PaymentAmount;  //- RemainAmountB;
    var LoanPrincipalC = PaymentTotalAmount - PaymentAmount;  //- RemainAmountC;

    //各プランにセット    
    //選択プランからローン手数料、ローン合計を算出
    var PaymentPlanType = radioValue(document.forms[0].PaymentPlanType);
    var LoanFeeAmount = 0;
    var LoanTotalAmount = 0;
    var judgePaymentPlanType = PaymentPlanType;

    //ローンPlanが設定されていたらチェックする
    if (PaymentPlanType != null && PaymentPlanType != '') {

        //ローン会社からの入金が既に完了していた場合、ローンへ振り替えないようにする
        var SlipNumber = document.getElementById('SlipNumber').value;
        var LoanCode = document.getElementById('LoanCode' + PaymentPlanType).value;

        $.ajax({
            type: "POST",
            url: "/CarSalesOrder/LoanCompleteCheck",
            async: false, //同期通信
            data: { SlipNumber: SlipNumber, LoanCode: LoanCode },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data != null && data.length != 0) {
                    if (data) {
                        judgePaymentPlanType = 'D'; //空文字・A・B・C　以外の値をセット
                    } else {
                        judgePaymentPlanType = PaymentPlanType;
                    }

                    switch (judgePaymentPlanType) {
                        case "": //ローンなし
                            LoanFeeAmount = 0;
                            LoanTotalAmount = 0;
                            //LoanPrincipalAmount = 0;
                            break;
                        case "A":
                            $('#LoanPrincipalA').val(ConversionWithComma(LoanPrincipalA));
                            var LoanFeeA = ConversionExceptComma($('#LoanFeeA').val());
                            var LoanTotalAmountA = LoanPrincipalA + LoanFeeA;

                            LoanFeeAmount = LoanFeeA;
                            LoanTotalAmount = LoanTotalAmountA;
                            LoanPrincipalAmount = LoanPrincipalA;
                            $('#LoanTotalAmountA').val(ConversionWithComma(LoanTotalAmountA));
                            break;
                        case "B":
                            $('#LoanPrincipalB').val(ConversionWithComma(LoanPrincipalB));
                            var LoanFeeB = ConversionExceptComma($('#LoanFeeB').val());
                            var LoanTotalAmountB = LoanPrincipalB + LoanFeeB;

                            LoanFeeAmount = LoanFeeB;
                            LoanTotalAmount = LoanTotalAmountB;
                            LoanPrincipalAmount = LoanPrincipalB;
                            $('#LoanTotalAmountB').val(ConversionWithComma(LoanTotalAmountB));
                            break;
                        case "C":
                            $('#LoanPrincipalC').val(ConversionWithComma(LoanPrincipalC));
                            var LoanFeeC = ConversionExceptComma($('#LoanFeeC').val());
                            var LoanTotalAmountC = LoanPrincipalC + LoanFeeC;

                            LoanFeeAmount = LoanFeeC;
                            LoanTotalAmount = LoanTotalAmountC;
                            LoanPrincipalAmount = LoanPrincipalC;
                            $('#LoanTotalAmountC').val(ConversionWithComma(LoanTotalAmountC));
                            break;

                        default:
                            //何もしない(元々の値を入れる)
                            LoanPrincipalAmount = PrevLoanPrincipalAmount;
                            LoanFeeAmount = PrevLoanFeeAmount;
                            LoanTotalAmount = PrevLoanTotalAmount;
                            

                            break;
                    }

                    $('#SelectedLoanPlan').val(PaymentPlanType);
                    $('#LoanPrincipalAmount').val(ConversionWithComma(LoanPrincipalAmount));
                    $('#LoanFeeAmount').val(ConversionWithComma(LoanFeeAmount));
                    $('#LoanTotalAmount').val(ConversionWithComma(LoanTotalAmount));
                    $('#PaymentGrandTotalAmount').val(ConversionWithComma(PaymentAmount + (TradeInAppropriationTotalAmount < 0 ? 0 : TradeInAppropriationTotalAmount) + LoanTotalAmount));
                }
            },
            error: function () {  //通信失敗時
                alert("ローン情報の取得に失敗しました。");
            }
        });
    }
    $('#SelectedLoanPlan').val(PaymentPlanType);
    $('#LoanPrincipalAmount').val(ConversionWithComma(LoanPrincipalAmount));
    $('#LoanFeeAmount').val(ConversionWithComma(LoanFeeAmount));
    $('#LoanTotalAmount').val(ConversionWithComma(LoanTotalAmount));
    $('#PaymentGrandTotalAmount').val(ConversionWithComma(PaymentAmount + (TradeInAppropriationTotalAmount < 0 ? 0 : TradeInAppropriationTotalAmount) + LoanTotalAmount));
    
    //全ての金額欄にカンマを挿入する
    //InsertCommaAll();  //Add 2017/02/14  arc yano #3641


}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： 車両本体価格の計算                                                             */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/* 更新日：                                                                                */
/*  2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算                 */
/*  2019/09/17 yano #4011  消費税、自動車税、自動車取得税変更に伴う改修作業  全面的に改修  */
/*  2017/04/23 arc yano #3755  金額欄の入力時のカーソル位置の不具合                        */
/*                                     取得したデータをカンマ付に変換する                  */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcSalesPrice(calcFlag) {
    var salesPriceWithTax = 0;
    var salesTax = 0;
    var salesPrice = 0;

    //Del 2019/09/12 yano ref
   
    switch (calcFlag) {
        case 1: // 税抜入力
            salesPrice = isNaN(parseInt(ConversionExceptComma(document.getElementById('SalesPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('SalesPrice').value)); 
            salesTax = calcTaxAmount(salesPrice);
            salesPriceWithTax = salesPrice + salesTax;
            document.getElementById('SalesTax').value = ConversionWithComma(salesTax);
            document.getElementById('SalesPriceWithTax').value = ConversionWithComma(salesPriceWithTax);
            break;
        case 2: // 消費税入力
            salesPriceWithTax = isNaN(parseInt(ConversionExceptComma(document.getElementById('SalesPriceWithTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('SalesPriceWithTax').value));
            salesTax = isNaN(parseInt(ConversionExceptComma(document.getElementById('SalesTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('SalesTax').value));
            salesPrice = salesPriceWithTax - salesTax;
            document.getElementById('SalesPrice').value = ConversionWithComma(salesPrice);
            break;
        case 3: // 税込入力
            salesPriceWithTax = isNaN(parseInt(ConversionExceptComma(document.getElementById('SalesPriceWithTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('SalesPriceWithTax').value));
            salesTax = calcTaxAmountFromTotalAmount(salesPriceWithTax);
            salesPrice = salesPriceWithTax - salesTax;
            document.getElementById('SalesTax').value = ConversionWithComma(salesTax);
            document.getElementById('SalesPrice').value = ConversionWithComma(salesPrice);
            break;
        case 3:

            break;
    }

    //Del 2019/09/12 yano ref

    calcTotalAmount();

    //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());   //Add 2019/09/17 yano #4011   //Del 2019/10/17 yano #4022
}

/*------------------------------------------------------------------------------*/
/* 機能　： 車両値引価格の計算                                                  */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合      */
/*          取得したデータをカンマ付に変換する                                  */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function calcDiscountAmount(calcFromTotal) {
    var discountAmountWithTax = 0;
    var discountTax = 0;
    var discountAmount = 0;


    //Mod  arc yano #3755
    if (calcFromTotal) {
        discountAmountWithTax = isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmountWithTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmountWithTax').value));
        discountTax = calcTaxAmountFromTotalAmount(discountAmountWithTax);
        discountAmount = discountAmountWithTax - discountTax;
        document.getElementById('DiscountTax').value = ConversionWithComma(discountTax);
        document.getElementById('DiscountAmount').value = ConversionWithComma(discountAmount);
    } else {
        discountTax = isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value));
        discountAmount = isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value));
        discountAmountWithTax = ConversionWithComma(discountAmount + discountTax);
        document.getElementById('DiscountAmountWithTax').value = ConversionWithComma(discountAmountWithTax);
    }

    //Del 2019/09/12 yano ref

    calcTotalAmount();
}

/*----------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： オプション金額の計算                                                                                              */
/* 作成日：                                                                                                                   */
/* 更新日：                                                                                                                   */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  外出ししたオプション合計再計算処理の追加  */
/*          2017/04/21 arc yano #3755 カンマ対応による不具合                                                                  */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                                          */
/*          2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正                                        */
/*                                                                                                                            */
/*----------------------------------------------------------------------------------------------------------------------------*/
function calcOptionAmount(calcFromTotal, num) {
    var amount = 0;
    var tax = 0;
    var amountWithTax = 0;

    //Mod 2017/04/21 arc yano 暫定版
    var amount = ConversionExceptComma(document.getElementById('line[' + num + ']_Amount').value);
    var tax = ConversionExceptComma(document.getElementById('line[' + num + ']_TaxAmount').value);
    var amountWithTax = ConversionExceptComma(document.getElementById('line[' + num + ']_AmountWithTax').value);

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
  
    if (calcFromTotal) {
        //amountWithTax = isNaN(parseInt(document.getElementById('line[' + num + ']_AmountWithTax').value)) ? 0 : parseInt(document.getElementById('line[' + num + ']_AmountWithTax').value);
        tax = calcTaxAmountFromTotalAmount(amountWithTax);
        amount = amountWithTax - tax;
        document.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(tax);
        document.getElementById('line[' + num + ']_Amount').value = ConversionWithComma(amount);
    } else {
        //tax = isNaN(parseInt(document.getElementById('line[' + num + ']_TaxAmount').value)) ? 0 : parseInt(document.getElementById('line[' + num + ']_TaxAmount').value);
        //amount = isNaN(parseInt(document.getElementById('line[' + num + ']_Amount').value)) ? 0 : parseInt(document.getElementById('line[' + num + ']_Amount').value);
        amountWithTax = amount + tax;
        document.getElementById('line[' + num + ']_AmountWithTax').value = ConversionWithComma(amountWithTax);
    }

    //Del 2019/09/12 yano ref

    //Add 2019/09/04 yano #4011
    //オプション合計計算
    calcTotalOptionAmount();

    calcTotalAmount();
}

// 現金在高合計金額計算
function calcTotalCachBalance() {
    var amountOf10000 = (isNaN(parseInt(document.forms[0].NumberOf10000.value)) ? 0 : parseInt(document.forms[0].NumberOf10000.value) * 10000);
    var amountOf5000 = (isNaN(parseInt(document.forms[0].NumberOf5000.value)) ? 0 : parseInt(document.forms[0].NumberOf5000.value) * 5000);
    var amountOf2000 = (isNaN(parseInt(document.forms[0].NumberOf2000.value)) ? 0 : parseInt(document.forms[0].NumberOf2000.value) * 2000);
    var amountOf1000 = (isNaN(parseInt(document.forms[0].NumberOf1000.value)) ? 0 : parseInt(document.forms[0].NumberOf1000.value) * 1000);
    var amountOf500 = (isNaN(parseInt(document.forms[0].NumberOf500.value)) ? 0 : parseInt(document.forms[0].NumberOf500.value) * 500);
    var amountOf100 = (isNaN(parseInt(document.forms[0].NumberOf100.value)) ? 0 : parseInt(document.forms[0].NumberOf100.value) * 100);
    var amountOf50 = (isNaN(parseInt(document.forms[0].NumberOf50.value)) ? 0 : parseInt(document.forms[0].NumberOf50.value) * 50);
    var amountOf10 = (isNaN(parseInt(document.forms[0].NumberOf10.value)) ? 0 : parseInt(document.forms[0].NumberOf10.value) * 10);
    var amountOf5 = (isNaN(parseInt(document.forms[0].NumberOf5.value)) ? 0 : parseInt(document.forms[0].NumberOf5.value) * 5);
    var amountOf1 = (isNaN(parseInt(document.forms[0].NumberOf1.value)) ? 0 : parseInt(document.forms[0].NumberOf1.value) * 1);
    var checkAmount = (isNaN(parseInt(document.forms[0].CheckAmount.value)) ? 0 : parseInt(document.forms[0].CheckAmount.value));
    var totalAmount = amountOf10000 + amountOf5000 + amountOf2000 + amountOf1000
                    + amountOf500 + amountOf100 + amountOf50 + amountOf10 + amountOf5 + amountOf1 + checkAmount;
    $('#AmountOf10000').html(currency(amountOf10000));
    $('#AmountOf5000').html(currency(amountOf5000));
    $('#AmountOf2000').html(currency(amountOf2000));
    $('#AmountOf1000').html(currency(amountOf1000));
    $('#AmountOf500').html(currency(amountOf500));
    $('#AmountOf100').html(currency(amountOf100));
    $('#AmountOf50').html(currency(amountOf50));
    $('#AmountOf10').html(currency(amountOf10));
    $('#AmountOf5').html(currency(amountOf5));
    $('#AmountOf1').html(currency(amountOf1));
    $('#TotalAmount').html(currency(totalAmount));
}

/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： 車両本体価格の計算                                                                                    */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*                                    取得したデータをカンマ付に変換する                                          */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*          2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正                            */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcVehiecleAmount(calcFlag) {
    var vehiclePrice = 0;
    var vehicleTax = 0;
    var vehicleAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1: // 税抜入力
            vehiclePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehiclePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehiclePrice').value)));
            vehicleTax = calcTaxAmount(vehiclePrice);
            vehicleAmount = vehiclePrice + vehicleTax;
            $('#VehicleTax').val(ConversionWithComma(vehicleTax));
            $('#VehicleAmount').val(ConversionWithComma(vehicleAmount));
            break;
        case 2: // 消費税入力
            vehicleAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value)));
            vehicleTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehicleTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehicleTax').value)));
            vehiclePrice = vehicleAmount - vehicleTax;
            $('#VehiclePrice').val(ConversionWithComma(vehiclePrice));
            break;
        case 3: // 税込入力
            vehicleAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value)));
            vehicleTax = calcTaxAmountFromTotalAmount(vehicleAmount);
            vehiclePrice = vehicleAmount - vehicleTax;
            $('#VehicleTax').val(ConversionWithComma(vehicleTax));
            $('#VehiclePrice').val(ConversionWithComma(vehiclePrice));
            break;
    }

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除

}

/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： オークション落札料の計算                                                                              */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*          取得したデータをカンマ付に変換する                                                                    */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcAuctionFeeAmount(calcFlag) {
    var auctionFeePrice = 0;
    var auctionFeeTax = 0;
    var auctionFeeAmount = 0;

    switch (calcFlag) {
        case 1: // 税抜入力
            auctionFeePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value)));
            auctionFeeTax = calcTaxAmount(auctionFeePrice);
            auctionFeeAmount = auctionFeePrice + auctionFeeTax;
            $('#AuctionFeeTax').val(ConversionWithComma(auctionFeeTax));
            $('#AuctionFeeAmount').val(ConversionWithComma(auctionFeeAmount));
            break;
        case 2: // 消費税入力
            auctionFeeAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value)));
            auctionFeeTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value)));
            auctionFeePrice = auctionFeeAmount - auctionFeeTax;
            $('#AuctionFeePrice').val(ConversionWithComma(auctionFeePrice));
            break;
        case 3: // 税込入力
            auctionFeeAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value)));
            auctionFeeTax = calcTaxAmountFromTotalAmount(auctionFeeAmount);
            auctionFeePrice = auctionFeeAmount - auctionFeeTax;
            $('#AuctionFeeTax').val(ConversionWithComma(auctionFeeTax));
            $('#AuctionFeePrice').val(ConversionWithComma(auctionFeePrice));
            break;
    }
    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： 自税充当の計算                                                                                        */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*          取得したデータをカンマ付に変換する                                                                    */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*         2012.04.14 課税対象となり税込金額から計算するよう変更                                                  */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcCarTaxAppropriatePrice() {


    //Mod 2017/04/23 arc yano #3755
    var carTaxAppropriateAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value)));
    var carTaxAppropriateTax = calcTaxAmountFromTotalAmount(carTaxAppropriateAmount);
    var carTaxAppropriatePrice = carTaxAppropriateAmount - carTaxAppropriateTax;

    $('#CarTaxAppropriateTax').val(ConversionWithComma(carTaxAppropriateTax));
    $('#CarTaxAppropriatePrice').val(ConversionWithComma(carTaxAppropriatePrice));

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： リサイクル価格の計算                                                                                  */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*          取得したデータをカンマ付に変換する                                                                    */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcRecyclePrice() {

    //Mod 2017/04/23 arc yano #3755
    var recyclePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value)));
    var recycleTax = 0;
    var recycleAmount = recyclePrice;
    $('#RecycleTax').val(ConversionWithComma(recycleTax));
    $('#RecycleAmount').val(ConversionWithComma(recyclePrice));

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： 加装価格の計算                                                                                        */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*          取得したデータをカンマ付に変換する                                                                    */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcEquipmentPrice(calcFlag) {
    var equipmentPrice = 0;
    var equipmentTax = 0;
    var equipmentAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1:
            equipmentPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value)));
            equipmentTax = calcTaxAmount(equipmentPrice);
            equipmentAmount = equipmentPrice + equipmentTax;
            $('#EquipmentTax').val(ConversionWithComma(equipmentTax));
            $('#EquipmentAmount').val(ConversionWithComma(equipmentAmount));
            break;
        case 2:
            equipmentAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value)));
            equipmentTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value)));
            equipmentPrice = equipmentAmount - equipmentTax;
            $('#EquipmentPrice').val(ConversionWithComma(equipmentPrice));
            break;
        case 3:
            equipmentAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value)));
            equipmentTax = calcTaxAmountFromTotalAmount(equipmentAmount);
            equipmentPrice = equipmentAmount - equipmentTax;
            $('#EquipmentTax').val(ConversionWithComma(equipmentTax));
            $('#EquipmentPrice').val(ConversionWithComma(equipmentPrice));
            break;
    }
    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}
/*----------------------------------------------------------------------------------------------------------------*/
/* 機能　： 加修価格の計算                                                                                        */
/* 作成日：                                                                                                       */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                        */
/*          取得したデータをカンマ付に変換する                                                                    */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                                              */
/*          2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正                            */
/*                                                                                                                */
/*----------------------------------------------------------------------------------------------------------------*/
function calcRepairPrice(calcFlag) {
    var repairPrice = 0;
    var repairTax = 0;
    var repairAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1:
            repairPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value)));
            repairTax = calcTaxAmount(repairPrice);
            repairAmount = repairPrice + repairTax;
            $('#RepairTax').val(ConversionWithComma(repairTax));
            $('#RepairAmount').val(ConversionWithComma(repairAmount));
            break;
        case 2:
            repairAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value)));
            repairTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairTax').value)));
            repairPrice = repairAmount - repairTax;
            $('#RepairPrice').val(ConversionWithComma(repairPrice));
            break;
        case 3:
            repairAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value)));
            repairTax = calcTaxAmountFromTotalAmount(repairAmount);
            repairPrice = repairAmount - repairTax;
            $('#RepairTax').val(ConversionWithComma(repairTax));
            $('#RepairPrice').val(ConversionWithComma(repairPrice));
            break;
    }
    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： メタリック価格の計算                                                           */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */         
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcMetallicPrice(calcFlag) {
    var metallicPrice = 0;
    var metallicTax = 0;
    var metallicAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1: // 税抜入力
            metallicPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value)));
            metallicTax = calcTaxAmount(metallicPrice);
            metallicAmount = metallicPrice + metallicTax;
            $('#MetallicTax').val(ConversionWithComma(metallicTax));
            $('#MetallicAmount').val(ConversionWithComma(metallicAmount));
            break;
        case 2: // 消費税入力
            metallicAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value)));
            metallicTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value)));
            metallicPrice = metallicAmount - metallicTax;
            $('#MetallicPrice').val(ConversionWithComma(metallicPrice));
            break;
        case 3: // 税込入力
            metallicAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value)));
            metallicTax = calcTaxAmountFromTotalAmount(metallicAmount);
            metallicPrice = metallicAmount - metallicTax;
            $('#MetallicTax').val(ConversionWithComma(metallicTax));
            $('#MetallicPrice').val(ConversionWithComma(metallicPrice));
            break;
    }

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： オプション価格の計算                                                           */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcOptionPrice(calcFlag) {
    var optionPrice = 0;
    var optionTax = 0;
    var optionAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1: // 税抜入力
            optionPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value)));
            optionTax = calcTaxAmount(optionPrice);
            optionAmount = optionPrice + optionTax;
            $('#OptionTax').val(ConversionWithComma(optionTax));
            $('#OptionAmount').val(ConversionWithComma(optionAmount));
            break;
        case 2: // 消費税入力
            optionAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value)));
            optionTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionTax').value)));
            optionPrice = optionAmount - optionTax;
            $('#OptionPrice').val(ConversionWithComma(optionPrice));
            break;
        case 3: // 税込入力
            optionAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value)));
            optionTax = calcTaxAmountFromTotalAmount(optionAmount);
            optionPrice = optionAmount - optionTax;
            $('#OptionTax').val(ConversionWithComma(optionTax));
            $('#OptionPrice').val(ConversionWithComma(optionPrice));
            break;
    }
    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： ファーム価格の計算                                                             */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */         
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcFirmPrice(calcFlag) {
    var firmPrice = 0;
    var firmTax = 0;
    var firmAmount = 0;

    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1: // 税抜入力
            firmPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value)));
            firmTax = calcTaxAmount(firmPrice);
            firmAmount = firmPrice + firmTax;
            $('#FirmTax').val(ConversionWithComma(firmTax));
            $('#FirmAmount').val(ConversionWithComma(firmAmount));
            break;
        case 2: // 消費税入力
            firmAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value)));
            firmTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmTax').value)));
            firmPrice = firmAmount - firmTax;
            $('#FirmPrice').val(ConversionWithComma(firmPrice));
            break;
        case 3: // 税込入力
            firmAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value)));
            firmTax = calcTaxAmountFromTotalAmount(firmAmount);
            firmPrice = firmAmount - firmTax;
            $('#FirmTax').val(ConversionWithComma(firmTax));
            $('#FirmPrice').val(ConversionWithComma(firmPrice));
            break;
    }
    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： ディスカウント価格の計算                                                       */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcDiscountPrice(calcFlag) {
    var discountPrice = 0;
    var discountTax = 0;
    var discountAmount = 0;

    switch (calcFlag) {
        case 1: // 税抜入力
            discountPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value)));
            discountTax = calcTaxAmount(discountPrice);
            discountAmount = discountPrice + discountTax;
            $('#DiscountTax').val(ConversionWithComma(discountTax));
            $('#DiscountAmount').val(ConversionWithComma(discountAmount));
            break;
        case 2: // 消費税入力
            discountAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value)));
            discountTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value)));
            discountPrice = discountAmount - discountTax;
            $('#DiscountPrice').val(ConversionWithComma(discountPrice));
            break;
        case 3: // 税込入力
            discountAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value)));
            discountTax = calcTaxAmountFromTotalAmount(discountAmount);
            discountPrice = discountAmount - discountTax;
            $('#DiscountTax').val(ConversionWithComma(discountTax));
            $('#DiscountPrice').val(ConversionWithComma(discountPrice));
            break;
    }

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： その他価格の計算                                                               */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcOthersPrice(calcFlag) {
    var othersPrice = 0;
    var othersTax = 0;
    var othersAmount = 0;


    //Mod 2017/04/23 arc yano #3755
    switch (calcFlag) {
        case 1:
            othersPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value)));
            othersTax = calcTaxAmount(othersPrice);
            othersAmount = othersPrice + othersTax;

            $('#OthersTax').val(ConversionWithComma(othersTax));      //テスト
            $('#OthersAmount').val(ConversionWithComma(othersAmount));
            break;
        case 2:
            othersAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value)));
            othersTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersTax').value)));
            othersPrice = othersAmount - othersTax;
            $('#OthersPrice').val(ConversionWithComma(othersPrice));
            break;
        case 3:
            othersAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value)));
            othersTax = calcTaxAmountFromTotalAmount(othersAmount);
            othersPrice = othersAmount - othersTax;
            $('#OthersTax').val(ConversionWithComma(othersTax));
            $('#OthersPrice').val(ConversionWithComma(othersPrice));
            break;
    }

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： 仕入価格の計算                                                                 */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                      */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcPurchaseAmount() {

    //Mod 2017/04/23 arc yano #3755
    var purchaseAmount = (isNaN(parseInt(ConversionExceptComma(document.forms[0].TotalAmount.value))) ? 0 : parseInt(ConversionExceptComma(document.forms[0].TotalAmount.value)));
    var purchaseTax = calcTaxAmountFromTotalAmount(purchaseAmount);
    var purchasePrice = purchaseAmount - purchaseTax;
    $('#TotalPrice').val(ConversionWithComma(purchasePrice));
    $('#TaxAmount').val(ConversionWithComma(purchaseTax));

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： 車両本体価格を逆算する                                                         */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                       */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcVehicleAmountFromTotal() {

    //Mod 2017/04/23 arc yano #3755
    var auctionFeePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value)));
    var carTaxAppropriatePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriatePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriatePrice').value)));
    var recyclePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value)));
    var equipmentPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value)));
    var repairPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value)));
    var metallicPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value)));
    var optionPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value)));
    var firmPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value)));
    var discountPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value)));
    var othersPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value)));

    var auctionFeeTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value)));
    var carTaxAppropriateTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateTax').value)));
    var recycleTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecycleTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecycleTax').value)));
    var equipmentTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value)));
    var repairTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairTax').value)));
    var metallicTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value)));
    var optionTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionTax').value)));
    var firmTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmTax').value)));
    var discountTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value)));
    var othersTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersTax').value)));

    var auctionFeeAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value)));
    var carTaxAppropriateAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value)));
    var recycleAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecycleAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecycleAmount').value)));
    var equipmentAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value)));
    var repairAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value)));
    var metallicAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value)));
    var optionAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value)));
    var firmAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value)));
    var discountAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value)));
    var othersAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value)));

    var totalAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('TotalAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('TotalAmount').value)));
    var taxAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('TaxAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('TaxAmount').value)));
    var amount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('Amount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('Amount').value)));

    var vehicleAmount = totalAmount - (auctionFeeAmount + recycleAmount + carTaxAppropriateAmount + equipmentAmount + repairAmount + metallicAmount + optionAmount + firmAmount + discountAmount + othersAmount);
    var vehicleTax = calcTaxAmountFromTotalAmount(vehicleAmount);
    var vehiclePrice = vehicleAmount - vehicleTax;

    $('#VehicleAmount').val(ConversionWithComma(vehicleAmount));
    $('#VehicleTax').val(ConversionWithComma(vehicleTax));
    $('#VehiclePrice').val(ConversionWithComma(vehiclePrice));

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： 仕入合計金額計算                                                               */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                       */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcTotalPurchaseAmount() {

    //Mod 2017/04/23 arc yano #3755
    var vehiclePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehiclePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehiclePrice').value)));
    var auctionFeePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeePrice').value)));
    var carTaxAppropriatePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriatePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriatePrice').value)));
    var recyclePrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecyclePrice').value)));
    var equipmentPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentPrice').value)));
    var repairPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairPrice').value)));
    var metallicPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicPrice').value)));
    var optionPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionPrice').value)));
    var firmPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmPrice').value)));
    var discountPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountPrice').value)));
    var othersPrice = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersPrice').value)));

    var vehicleTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehicleTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehicleTax').value)));
    var auctionFeeTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeTax').value)));
    var carTaxAppropriateTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateTax').value)));
    var recycleTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecycleTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecycleTax').value)));
    var equipmentTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentTax').value)));
    var repairTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairTax').value)));
    var metallicTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicTax').value)));
    var optionTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionTax').value)));
    var firmTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmTax').value)));
    var discountTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountTax').value)));
    var othersTax = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersTax').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersTax').value)));

    var vehicleAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('VehicleAmount').value)));
    var auctionFeeAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('AuctionFeeAmount').value)));
    var carTaxAppropriateAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value)));
    var recycleAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RecycleAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RecycleAmount').value)));
    var equipmentAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('EquipmentAmount').value)));
    var repairAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('RepairAmount').value)));
    var metallicAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MetallicAmount').value)));
    var optionAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OptionAmount').value)));
    var firmAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('FirmAmount').value)));
    var discountAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('DiscountAmount').value)));
    var othersAmount = (isNaN(parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('OthersAmount').value)));


    //仕入価格
    var totalPrice = vehiclePrice + auctionFeePrice + carTaxAppropriatePrice
                    + recyclePrice + equipmentPrice + repairPrice + metallicPrice
                    + optionPrice + firmPrice + discountPrice + othersPrice;
    $('#Amount').val(ConversionWithComma(totalPrice));

    //消費税
    var taxAmount = vehicleTax + auctionFeeTax + carTaxAppropriateTax
                    + recycleTax + equipmentTax + repairTax + metallicTax
                    + optionTax + firmTax + discountTax + othersTax;
    $('#TaxAmount').val(ConversionWithComma(taxAmount));

    //仕入税込価格
    var totalAmount = vehicleAmount + auctionFeeAmount + carTaxAppropriateAmount
                    + recycleAmount + equipmentAmount + repairAmount + metallicAmount
                    + optionAmount + firmAmount + discountAmount + othersAmount;
    $('#TotalAmount').val(ConversionWithComma(totalAmount));

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： サービス伝票の明細金額を計算                                                   */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                       */
/*          2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正     */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcLineAmount(num) {
    var doc = document;

    //Mod 2017/04/23 arc yano #3755
    switch (doc.getElementById('line[' + num + ']_ServiceType').value) {
        case "001": //主作業
            break;
        case "002": //サービスメニュー
          
            var ManPower = isNaN(doc.getElementById('line[' + num + ']_ManPower').value) ? 0 : doc.getElementById('line[' + num + ']_ManPower').value;
            var LaborRate = isNaN(ConversionExceptComma(doc.getElementById('line[' + num + ']_LaborRate').value)) ? 0 : ConversionExceptComma(doc.getElementById('line[' + num + ']_LaborRate').value);
            doc.getElementById('line[' + num + ']_TechnicalFeeAmount').value = ConversionWithComma(ManPower * LaborRate);
            doc.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(calcTaxAmount(ManPower + LaborRate));
            break;

        case "003": //部品
            var Price = isNaN(ConversionExceptComma(doc.getElementById('line[' + num + ']_Price').value)) ? 0 : ConversionExceptComma(doc.getElementById('line[' + num + ']_Price').value);
            var Quantity = isNaN(ConversionExceptComma(doc.getElementById('line[' + num + ']_Quantity').value)) ? 0 : ConversionExceptComma(doc.getElementById('line[' + num + ']_Quantity').value);

            doc.getElementById('line[' + num + ']_Amount').value = ConversionWithComma(Price * Quantity);
            doc.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(calcTaxAmount(Price * Quantity));
            break;
    }

    //Del 2019/09/12 yano ref コメントアウトされている処理の削除
}

/*-----------------------------------------------------------------------------------------*/
/* 機能　： サービス伝票の明細原価(合計)を計算                                                   */
/* 作成日：                                                                                */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                 */
/*          取得したデータをカンマ付に変換する                                             */
/*          2017/02/14 arc yano #3641 金額表示をカンマ表示に統一する                       */
/*                                                                                         */
/*-----------------------------------------------------------------------------------------*/
function calcLineCost(num) {
    var doc = document;

    //Mod 017/04/23 arc yano #3755
    var objUnitCost = doc.getElementById('line[' + num + ']_UnitCost');
    var objQuantity = doc.getElementById('line[' + num + ']_Quantity');

    if (objUnitCost && objQuantity) {

        var unitCost = isNaN(parseInt(ConversionExceptComma(objUnitCost.value))) ? 0 : parseInt(ConversionExceptComma(objUnitCost.value));
        var quantity = isNaN(parseFloat(objQuantity.value)) ? 0 : parseFloat(objQuantity.value);
        doc.getElementById('line[' + num + ']_Cost').value = ConversionWithComma(parseInt(unitCost * quantity));
    }
}


//--------------------------------------------------------------------------------------------
// サービス伝票の合計金額を計算
//
// 機能：
//       サービス伝票の合計金額を計算
//
// 作成日：????/??/?? who
// 更新日：
//       yano #4045 Chrome対応 Chrome動作確認中に発覚した不具合の修正
//       2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
//       2018/04/25 arc yano #3881 サービス伝票入力　課税の主作業→非課税の主作業に変更した際の
//                                 部品値引の消費税額のクリア漏れ
//       2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//                                 取得したデータをカンマ付に変換する
//       2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//       2016/04/14 arc yano #3480 サービス伝票　請求先の主作業大分類の設定処理を追加
//       2014/10/15 arc yano #3109 複数請求先の設定誤り対応
//                           サービス明細の各行の処理順序をid順ではなく、見た目の並び順に変更
//       2014/06/09 arc yano  高速化対応 
//                            明細行の計算は削除対象行以外のみ行う。
//       2014/03/19 ookubo    #3006] 【サ】納車済みで数字が変わる対応 DISCNTにToupper追加
//--------------------------------------------------------------------------------------------
function calcTotalServiceAmount() {
    var doc = document;

    //Mod 2017/04/23 arc yano #3755

    //自動車税種別割
    var CarTaxElement = ConversionExceptComma(doc.getElementById('CarTax').value);
    var CarTax = isNaN(parseInt(CarTaxElement)) ? 0 : parseInt(CarTaxElement);

    //自動車重量税
    var CarWeightTaxElement = ConversionExceptComma(doc.getElementById('CarWeightTax').value);
    var CarWeightTax = isNaN(parseInt(CarWeightTaxElement)) ? 0 : parseInt(CarWeightTaxElement);

    //自賠責保険
    var CarLiabilityInsuranceElement = ConversionExceptComma(doc.getElementById('CarLiabilityInsurance').value);
    var CarLiabilityInsurance = isNaN(parseInt(CarLiabilityInsuranceElement)) ? 0 : parseInt(CarLiabilityInsuranceElement);

    //ナンバー代
    var NumberPlateCostElement = ConversionExceptComma(doc.getElementById('NumberPlateCost').value);
    var NumberPlateCost = isNaN(parseInt(NumberPlateCostElement)) ? 0 : parseInt(NumberPlateCostElement);

    //印紙代
    var FiscalStampCostElement = ConversionExceptComma(doc.getElementById('FiscalStampCost').value);
    var FiscalStampCost = isNaN(parseInt(FiscalStampCostElement)) ? 0 : parseInt(FiscalStampCostElement);

    //自由項目
    var TaxFreeFieldValueElement = ConversionExceptComma(doc.getElementById('TaxFreeFieldValue').value);
    var TaxFreeFieldValue = isNaN(parseInt(TaxFreeFieldValueElement)) ? 0 : parseInt(TaxFreeFieldValueElement);

    //Add 2020/02/17 yano #4025 ------------------------------------------------------------------------------------------
    //任意保険
    var OptionalInsuranceElement = ConversionExceptComma(doc.getElementById('OptionalInsurance').value);
    var OptionalInsurance = isNaN(parseInt(OptionalInsuranceElement)) ? 0 : parseInt(OptionalInsuranceElement);

    //サービス加入料
    var SubscriptionFeeElement = ConversionExceptComma(doc.getElementById('SubscriptionFee').value);
    var SubscriptionFee = isNaN(parseInt(SubscriptionFeeElement)) ? 0 : parseInt(SubscriptionFeeElement);

    //課税自由項目
    var TaxableFreeFieldValueElement = ConversionExceptComma(doc.getElementById('TaxableFreeFieldValue').value);
    var TaxableFreeFieldValue = isNaN(parseInt(TaxableFreeFieldValueElement)) ? 0 : parseInt(TaxableFreeFieldValueElement);

    //諸費用(課税)合計
    var TaxableCostTotalAmount = SubscriptionFee + TaxableFreeFieldValue;
    $('#TaxableCostTotalAmount').val(ConversionWithComma(TaxableCostTotalAmount));

    //諸費用(課税)消費税
    var TaxableCostTaxAmount = calcTaxAmountFromTotalAmount(TaxableCostTotalAmount);
    $('#TaxableCostSubTotalTaxAmount').val(ConversionWithComma(TaxableCostTaxAmount));

    //諸費用(課税)小計(税抜)
    var TaxableCostSubTotalAmount = TaxableCostTotalAmount - TaxableCostTaxAmount;
    $('#TaxableCostSubTotalAmount').val(ConversionWithComma(TaxableCostSubTotalAmount));
    //-------------------------------------------------------------------------------------------------------------------

    //諸費用小計
    var CostSubTotalAmount = CarWeightTax + CarLiabilityInsurance + FiscalStampCost + CarTax + NumberPlateCost + TaxFreeFieldValue + OptionalInsurance; //Mod 2020/02/17 yano #4025
    $('#CostSubTotalAmount').val(ConversionWithComma(CostSubTotalAmount));

    //諸費用合計
    $('#CostTotalAmount').val(ConversionWithComma(CostSubTotalAmount));

    //明細合計
    var LineCountElement = doc.getElementById('LineCount').value;
    var LineCount = isNaN(parseInt(LineCountElement)) ? 0 : parseInt(LineCountElement);
    var EngineerTotalAmount = 0;    //技術料合計
    var EngineerTotalTaxAmount = 0; //技術料消費税合計
    var PartsTotalAmount = 0;       //部品合計
    var PartsTotalTaxAmount = 0;    //部品消費税合計
    var SubTotalAmount = 0;         //整備小計
    var TaxTotalAmount = 0;         //一般消費税
    var ServiceTotalAmount = 0;     //整備料合計
    var GrandTotalAmount = 0;       //請求合計

    var EngineerCostTotal = 0;      //整備原価合計
    var PartsCostTotal = 0;         //部品原価合計
    var CostTotal = 0;              //原価合計
    var GrossSalesAmount = 0;       //粗利

    //var list = [];
    var customerClaimCode = "";
    var customerClaimName = "";
    var classification1 = "";
    var serviceWorkCode = "";
    var swCustomerClaimClass = "";     //Add 2016/04/14 arc yano #3480
    var ccCustomerClaimClass = "";     //Add 2016/04/14 arc yano #3480

    if (LineCount > 0) {

        //--------------------------------------------------
        //line[xx]のidを見た目上の並びで取得する。
        //--------------------------------------------------
        var tbody = document.getElementById('salesline-tbody');
        var child = tbody.firstChild;
        var i = 0;
        var strid = [];     //id一覧
        while (true) {

            if (child.nodeName.toUpperCase() == 'TR') { //行の場合
                //上の行が非表示(=削除対象行)ではない場合
                if (child.style.display != "none") {
                    //フィールド取得
                    var objselect = child.getElementsByTagName('select');
                    var j;
                    for (j = 0; j < objselect.length; j++) {
                        if (objselect[j].name.match(/line\[\d{1,4}\]\.ServiceType/)) {
                            strid[i] = objselect[j].name.substring(0, (objselect[j].name.length - 12));     //line[x].ServiceTypeからline[x]だけを抜き出す。
                            i++;
                            child = child.nextSibling;
                            break;
                        }
                    }
                }
                else {
                    child = child.nextSibling;
                }
            }
            else {
                child = child.nextSibling;
            }

            if (child == null) {    //子ノードがnullの場合、ループ抜け
                break;
            }
        }

        for (var num = 0; num < strid.length; num++) {
            var delflag = doc.getElementById(strid[num] + '_CliDelFlag');
            if ((delflag != null) && (delflag.value != "1")) {
                switch (doc.getElementById(strid[num] + '_ServiceType').value) {
                    case "001": //主作業
                        customerClaimCode = doc.getElementById(strid[num] + '_CustomerClaimCode').value;
                        customerClaimName = doc.getElementById(strid[num] + '_CustomerClaimName').value;
                        classification1 = doc.getElementById(strid[num] + '_Classification1').value;
                        serviceWorkCode = doc.getElementById(strid[num] + '_ServiceWorkCode').value;
                        ccCustomerClaimClass = doc.getElementById(strid[num] + '_CCCustomerClaimClass').value;    //Add 2016/04/14 arc yano #3480
                        swCustomerClaimClass = doc.getElementById(strid[num] + '_SWCustomerClaimClass').value;    //Add 2016/04/14 arc yano #3480

                        break;
                    case "002": //サービスメニュー
                        //Mod 2021/05/24 yano #4045
                        var ManPower = isNaN(doc.getElementById(strid[num] + '_ManPower').value) ? '0' : doc.getElementById(strid[num] + '_ManPower').value;
                        //var ManPower = isNaN(doc.getElementById(strid[num] + '_ManPower').value) ? 0 : doc.getElementById(strid[num] + '_ManPower').value;

                        var LaborRate = isNaN(parseFloat(ConversionExceptComma(doc.getElementById(strid[num] + '_LaborRate').value))) ? 0 : parseFloat(ConversionExceptComma(doc.getElementById(strid[num] + '_LaborRate').value));
                        var dot = ManPower.length - ManPower.indexOf(".", 0) - 1;

                        var TechnicalFeeAmount = Math.floor(Math.round(ManPower * Math.pow(10, dot) * LaborRate) / Math.pow(10, dot));
                        //var TechnicalFeeAmount = Math.floor(ManPower * LaborRate);
                        var ServiceMenuCode = doc.getElementById(strid[num] + '_ServiceMenuCode').value;

                        //原価
                        var EngineerCost = isNaN(parseInt(ConversionExceptComma(doc.getElementById(strid[num] + '_Cost').value))) ? 0 : parseInt(ConversionExceptComma(doc.getElementById(strid[num] + '_Cost').value));
                        EngineerCostTotal += EngineerCost;
                        CostTotal += EngineerCost;

                        doc.getElementById(strid[num] + '_TechnicalFeeAmount').value = ConversionWithComma(TechnicalFeeAmount);
                        doc.getElementById(strid[num] + '_CustomerClaimCode').value = customerClaimCode;
                        doc.getElementById(strid[num] + '_CustomerClaimName').value = customerClaimName;
                        doc.getElementById(strid[num] + '_Classification1').value = classification1;
                        doc.getElementById(strid[num] + '_ServiceWorkCode').value = serviceWorkCode;
                        doc.getElementById(strid[num] + '_CCCustomerClaimClass').value = ccCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        doc.getElementById(strid[num] + '_SWCustomerClaimClass').value = swCustomerClaimClass;    //Add 2016/04/14 arc yano #3480

                        //課税対象
                        if (classification1 != '002') {
                            //値引
                            if (ServiceMenuCode.substr(0, 6).toUpperCase() == 'DISCNT') {
                                //税込金額から消費税額を計算
                                taxAmount = calcTaxAmountFromTotalAmount(TechnicalFeeAmount);
                                //消費税額から税抜金額を計算
                                TechnicalFeeAmount = TechnicalFeeAmount - taxAmount;
                                //消費税額をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = ConversionWithComma(taxAmount);
                                //文字色を赤に
                                doc.getElementById(strid[num] + '_TechnicalFeeAmount').style.color = 'red';
                                //技術料合計から税抜金額をマイナス
                                EngineerTotalAmount -= TechnicalFeeAmount;
                                //技術料消費税合計から消費税額をマイナス(2010/10/12)
                                EngineerTotalTaxAmount -= taxAmount;
                                //消費税合計から消費税額をマイナス
                                TaxTotalAmount -= taxAmount;

                                //通常
                            } else {
                                //税抜金額から消費税額を計算
                                taxAmount = calcTaxAmount(TechnicalFeeAmount);
                                //消費税額をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = ConversionWithComma(taxAmount);
                                //技術料合計に税抜金額をプラス
                                EngineerTotalAmount += TechnicalFeeAmount;
                                //技術料消費税合計に消費税額をプラス(2010/10/12)
                                EngineerTotalTaxAmount += taxAmount;
                                //消費税合計に消費税額をプラス
                                TaxTotalAmount += taxAmount;
                            }
                            //非課税
                        } else {
                            //値引
                            //2014/03/19.ookubo #3006] 【サ】納車済みで数字が変わる対応 DISCNTにToupper追加
                            if (ServiceMenuCode.substr(0, 6).toUpperCase() == 'DISCNT') {
                                //消費税額に0をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = 0;
                                //文字色を赤に
                                doc.getElementById(strid[num] + '_TechnicalFeeAmount').style.color = 'red';
                                //技術料合計から税抜金額をマイナス
                                EngineerTotalAmount -= TechnicalFeeAmount;
                                //通常
                            } else {
                                //消費税額に0をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = 0;
                                //技術料合計に税抜金額をプラス
                                EngineerTotalAmount += TechnicalFeeAmount;

                            }

                        }
                        break;
                    case "003": //部品
                        var Price = isNaN(ConversionExceptComma(doc.getElementById(strid[num] + '_Price').value)) ? 0 : ConversionExceptComma(doc.getElementById(strid[num] + '_Price').value);
                        var Quantity = isNaN(doc.getElementById(strid[num] + '_Quantity').value) ? 0 : doc.getElementById(strid[num] + '_Quantity').value;
                        var PartsAmount = Math.floor(Price * Quantity);
                        var PartsCost = isNaN(parseInt(ConversionExceptComma(doc.getElementById(strid[num] + '_Cost').value))) ? 0 : parseInt(ConversionExceptComma(doc.getElementById(strid[num] + '_Cost').value));
                        PartsCostTotal += PartsCost;
                        CostTotal += PartsCost;

                        doc.getElementById(strid[num] + '_Amount').value = ConversionWithComma(PartsAmount);
                        doc.getElementById(strid[num] + '_CustomerClaimCode').value = customerClaimCode;
                        doc.getElementById(strid[num] + '_CustomerClaimName').value = customerClaimName;
                        doc.getElementById(strid[num] + '_Classification1').value = classification1;
                        doc.getElementById(strid[num] + '_ServiceWorkCode').value = serviceWorkCode;
                        doc.getElementById(strid[num] + '_CCCustomerClaimClass').value = ccCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        doc.getElementById(strid[num] + '_SWCustomerClaimClass').value = swCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        var PartsNumber = doc.getElementById(strid[num] + '_PartsNumber').value;

                        //課税対象
                        if (classification1 != '002') {
                            //値引
                            if (PartsNumber.substr(0, 6).toUpperCase() == 'DISCNT') {
                                //税込金額から消費税額を計算
                                taxAmount = calcTaxAmountFromTotalAmount(PartsAmount);
                                //消費税額から税抜額を計算
                                PartsAmount = PartsAmount - taxAmount;
                                //消費税額をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = ConversionWithComma(taxAmount);
                                //文字を赤色に
                                doc.getElementById(strid[num] + '_Amount').style.color = 'red';
                                //部品合計から税抜値引額をマイナス
                                PartsTotalAmount -= PartsAmount;
                                //部品消費税合計から消費税額をマイナス
                                PartsTotalTaxAmount -= taxAmount;

                                //消費税合計から消費税額をマイナス
                                TaxTotalAmount -= taxAmount;
                            } else {
                                //税抜金額から消費税額を計算
                                taxAmount = calcTaxAmount(PartsAmount);
                                //消費税額をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = ConversionWithComma(taxAmount);
                                //部品合計に税抜額をプラス
                                PartsTotalAmount += PartsAmount;
                                //部品消費税合計に消費税額をプラス
                                PartsTotalTaxAmount += taxAmount;
                                //消費税合計に消費税額をプラス
                                TaxTotalAmount += taxAmount;
                            }
                            //非課税
                        } else {
                            //値引
                            //2014/03/19.ookubo #3006] 【サ】納車済みで数字が変わる対応 DISCNTにToupper追加
                            if (PartsNumber.substr(0, 6).toUpperCase() == 'DISCNT') {
                                //税込金額から消費税額を計算
                                //taxAmount = calcTaxAmountFromTotalAmount(PartsAmount);
                                //消費税額から税抜額を計算
                                //PartsAmount = PartsAmount - taxAmount;

                                //Add 2018/04/25 #3881
                                //消費税額に0をセット
                                doc.getElementById(strid[num] + '_TaxAmount').value = 0;

                                //文字を赤色に
                                doc.getElementById(strid[num] + '_Amount').style.color = 'red';
                                //部品合計から税抜値引額をマイナス
                                PartsTotalAmount -= PartsAmount;
                            } else {
                                //部品合計に税抜額をプラス
                                PartsTotalAmount += PartsAmount;
                                //消費税額は0
                                doc.getElementById(strid[num] + '_TaxAmount').value = 0;
                            }
                        }
                        break;
                    case "004":
                        doc.getElementById(strid[num] + '_CustomerClaimCode').value = customerClaimCode;
                        doc.getElementById(strid[num] + '_CustomerClaimName').value = customerClaimName;
                        doc.getElementById(strid[num] + '_Classification1').value = classification1;
                        doc.getElementById(strid[num] + '_ServiceWorkCode').value = serviceWorkCode;
                        doc.getElementById(strid[num] + '_CCCustomerClaimClass').value = ccCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        doc.getElementById(strid[num] + '_SWCustomerClaimClass').value = swCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        break;
                    case "005":
                        doc.getElementById(strid[num] + '_CustomerClaimCode').value = customerClaimCode;
                        doc.getElementById(strid[num] + '_CustomerClaimName').value = customerClaimName;
                        doc.getElementById(strid[num] + '_Classification1').value = classification1;
                        doc.getElementById(strid[num] + '_ServiceWorkCode').value = serviceWorkCode;
                        doc.getElementById(strid[num] + '_CCCustomerClaimClass').value = ccCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        doc.getElementById(strid[num] + '_SWCustomerClaimClass').value = swCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        break;
                    case "006":
                        doc.getElementById(strid[num] + '_CustomerClaimCode').value = customerClaimCode;
                        doc.getElementById(strid[num] + '_CustomerClaimName').value = customerClaimName;
                        doc.getElementById(strid[num] + '_Classification1').value = classification1;
                        doc.getElementById(strid[num] + '_ServiceWorkCode').value = serviceWorkCode;
                        doc.getElementById(strid[num] + '_CCCustomerClaimClass').value = ccCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        doc.getElementById(strid[num] + '_SWCustomerClaimClass').value = swCustomerClaimClass;    //Add 2016/04/14 arc yano #3480
                        break;
                }
            }
        }
    }
    //技術料小計
    $('#EngineerTotalAmount').val(ConversionWithComma(EngineerTotalAmount));
    $('#EngineerTotalTaxAmount').val(ConversionWithComma(EngineerTotalTaxAmount));
    $('#EngineerTotalAmountWithTax').val(ConversionWithComma(EngineerTotalAmount + EngineerTotalTaxAmount));
    $('#EngineerTotalCost').val(ConversionWithComma(EngineerCostTotal));
    $('#EngineerGrossSalesAmount').val(ConversionWithComma(EngineerTotalAmount - EngineerCostTotal));

    //部品小計
    $('#PartsTotalAmount').val(ConversionWithComma(PartsTotalAmount));
    $('#PartsTotalTaxAmount').val(ConversionWithComma(PartsTotalTaxAmount));
    $('#PartsTotalAmountWithTax').val(ConversionWithComma(PartsTotalAmount + PartsTotalTaxAmount));
    $('#PartsTotalCost').val(ConversionWithComma(PartsCostTotal));
    $('#PartsGrossSalesAmount').val(ConversionWithComma(PartsTotalAmount - PartsCostTotal));

    //整備小計
    SubTotalAmount = PartsTotalAmount + EngineerTotalAmount;
    $('#SubTotalAmount').val(ConversionWithComma(SubTotalAmount));

    //一般消費税
    $('#TotalTaxAmount').val(ConversionWithComma(TaxTotalAmount));

    //整備料合計
    ServiceTotalAmount = SubTotalAmount + TaxTotalAmount;
    $('#ServiceTotalAmount').val(ConversionWithComma(ServiceTotalAmount));

    //請求合計
    GrandTotalAmount = ServiceTotalAmount + CostSubTotalAmount + TaxableCostTotalAmount; //Mod 2020/02/17 yano #4025
    $('#GrandTotalAmount').val(ConversionWithComma(GrandTotalAmount));

    //合計
    //Mod 2020/02/17 yano #4025---------------------------------------------------------------------------------------------------------------------------------------
    $('#TotalAmountWithoutTax').val(ConversionWithComma(EngineerTotalAmount + PartsTotalAmount + CostSubTotalAmount + TaxableCostSubTotalAmount));
    $('#TotalTaxAmount').val(ConversionWithComma(TaxTotalAmount + TaxableCostTaxAmount));
    $('#TotalAmountWithTax').val(ConversionWithComma(EngineerTotalAmount + EngineerTotalTaxAmount + PartsTotalAmount + PartsTotalTaxAmount + CostSubTotalAmount + TaxableCostTotalAmount));
    $('#TotalCost').val(ConversionWithComma(CostTotal));
    $('#TotalGrossSalesAmount').val(ConversionWithComma(SubTotalAmount - CostTotal));

    //$('#TotalAmountWithoutTax').val(ConversionWithComma(EngineerTotalAmount + PartsTotalAmount + CostSubTotalAmount));
    //$('#TotalTaxAmount').val(ConversionWithComma(TaxTotalAmount));
    //$('#TotalAmountWithTax').val(ConversionWithComma(EngineerTotalAmount + EngineerTotalTaxAmount + PartsTotalAmount + PartsTotalTaxAmount + CostSubTotalAmount));
    //$('#TotalCost').val(ConversionWithComma(CostTotal));
    //$('#TotalGrossSalesAmount').val(ConversionWithComma(SubTotalAmount - CostTotal));
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------
    //支払合計
    calcPaymentAmount();
}

//--------------------------------------------------------------------------------------------
// サービス伝票の明細金額を計算
//
// 機能：
//       サービス伝票の合計金額を計算
//
// 作成日：????/??/?? who
// 更新日：
//       2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//       取得したデータをカンマ付に変換する
//       2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//       2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正
//
//--------------------------------------------------------------------------------------------
function calcLineAmount(num) {
    var doc = document;

    //Mod 2017/04/23 arc yano #3755
    switch (doc.getElementById('line[' + num + ']_ServiceType').value) {
        case "001": //主作業
            break;
        case "002": //サービスメニュー
            var ManPower = isNaN(doc.getElementById('line[' + num + ']_ManPower').value) ? 0 : doc.getElementById('line[' + num + ']_ManPower').value;

            var LaborRate = isNaN(ConversionExceptComma(doc.getElementById('line[' + num + ']_LaborRate').value)) ? 0 : ConversionExceptComma(doc.getElementById('line[' + num + ']_LaborRate').value);
            doc.getElementById('line[' + num + ']_TechnicalFeeAmount').value = ConversionWithComma(ManPower * LaborRate);
            doc.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(calcTaxAmount(ManPower * LaborRate));
            break;
        case "003": //部品

            var Price = isNaN(ConversionExceptComma(doc.getElementById('line[' + num + ']_Price').value)) ? 0 : ConversionExceptComma(doc.getElementById('line[' + num + ']_Price').value);
            var Quantity = isNaN(doc.getElementById('line[' + num + ']_Quantity').value) ? 0 : doc.getElementById('line[' + num + ']_Quantity').value;
            doc.getElementById('line[' + num + ']_Amount').value = ConversionWithComma(Price * Quantity);
            doc.getElementById('line[' + num + ']_TaxAmount').value = ConversionWithComma(calcTaxAmount(Price * Quantity));
            break;
    }
}

//--------------------------------------------------------------------------------------------
// 機能：
// サービス伝票の明細原価(合計)を計算
//
// 作成：????/??/?? who
// 更新日：
//       2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//       取得したデータをカンマ付に変換する
//       2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//--------------------------------------------------------------------------------------------
function calcLineCost(num) {
    var doc = document;


    //Mod 2017/04/23 arc yano #3755
    var objUnitCost = doc.getElementById('line[' + num + ']_UnitCost');
    var objQuantity = doc.getElementById('line[' + num + ']_Quantity');

    if (objUnitCost && objQuantity) {
        var unitCost = isNaN(parseInt(ConversionExceptComma(objUnitCost.value))) ? 0 : parseInt(ConversionExceptComma(objUnitCost.value));
        var quantity = isNaN(parseFloat(objQuantity.value)) ? 0 : parseFloat(objQuantity.value);
        doc.getElementById('line[' + num + ']_Cost').value = ConversionWithComma(parseInt(unitCost * quantity));
    }

    //Del 2019/09/12 yano ref コメントアウトされた処理の削除
    calcTotalServiceAmount();
}

//サービス明細集計用
var ServiceLine = function() {
    this.CustomerClaimCode;
    this.TaxationAmount;
    this.TaxFreeAmount;
    this.TaxAmount;
}


//--------------------------------------------------------------------------------------------
// 機能：
//       サービス支払予定合計金額の計算
//
// 作成：????/??/?? who
// 更新日：
//       2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//       取得したデータをカンマ付に変換する
//       2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//--------------------------------------------------------------------------------------------
function calcPaymentAmount() {
    var paymentAmount = 0;
    var doc = document;


    //Mod 2017/04/23 arc yano #3755
    var num = isNaN(parseInt(doc.getElementById('PaymentCount').value)) ? 0 : parseInt(doc.getElementById('PaymentCount').value);
    for (var i = 0; i < num; i++) {

        paymentAmount += isNaN(parseInt(ConversionExceptComma(doc.getElementById('pay[' + i + ']_Amount').value))) ? 0 : parseInt(ConversionExceptComma(doc.getElementById('pay[' + i + ']_Amount').value));

    }
    doc.getElementById('PaymentTotalAmount').value = ConversionWithComma(paymentAmount);

    //Del 2019/09/12 yano ref コメントアウトされた処理の削除
}
//-----------------------------------------------------------------------------
//機能：部品単価と、入力した数量から金額を表示する 
//      2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//      取得したデータをカンマ付に変換する
//      2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//      2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力)
//                     
//-----------------------------------------------------------------------------
function calcPartsAmount(idQuantity, idUnitPrice, idAmount) {
    var doc = document;
   
    var quantity = isNaN(parseInt(doc.getElementById(idQuantity).value)) ? 0 : parseInt(doc.getElementById(idQuantity).value);

    //カンマ除去
    //ExceptComma(doc.getElementById(idUnitPrice));   //Add 2017/02/14 arc yano #3641

    var cost = isNaN(parseInt(ConversionExceptComma(doc.getElementById(idUnitPrice).value))) ? 0 : parseInt(ConversionExceptComma(doc.getElementById(idUnitPrice).value));

    //カンマ挿入
    //InsertComma(doc.getElementById(idUnitPrice));   //Add 2017/02/14 arc yano #3641

    var amount = quantity * cost;
    doc.getElementById(idAmount).value = ConversionWithComma(amount);

    //カンマ挿入
    //InsertComma(doc.getElementById(idAmount));  //Add 2017/02/14 arc yano #3641
}

//Del 2015/11/09 arc yano #3291　似たような関数が既にあるため、統合(こちらは廃止)

/*-------------------------------------------------------------------------------------------------*/
/* 機能　： 自動車税環境性能割を計算して合計計算する                                               */
/* 作成日：                                                                                        */
/* 更新日：                                                                                        */
/*          2020/11/17 yano #4065 【車両伝票入力】環境性能割・マスタの設定値が不正の場合の対応     */
/*          2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算                 */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  全面的に改修   *
/*          2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                         */
/*          取得したデータをカンマ付に変換する                                                     */                                                   
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                              */
/*          2014/06/25 arc                                                                         */
/*             ①タイムアウト値の変更(5000→10000)                                                 */
/*             ②タイムアウト発生時のエラーハンドリング追加                                        */
/*             ③戻り値を明確化                                                                    */
/*                                                                                                 */
/*-------------------------------------------------------------------------------------------------*/
function GetAcquisitionTax(taxid, requestRegistDate) { //Mod 2019/10/17 yano #4022 //Mod 2019/09/04 yano #4011 
    var gradeCode = document.getElementById('CarGradeCode').value;
    var salesCarId = document.getElementById('SalesCarNumber').value;
    var ControllerName = "";
    var code = "";
    var salesType = document.getElementById('SalesType');

    if (salesCarId != null && salesCarId != "") {
        ControllerName = "SalesCar";
        code = salesCarId;
    } else if (gradeCode != null || gradeCode != "") {
        ControllerName = "CarGrade";
        code = gradeCode;
    } else {
        alert('グレードコード、車両が指定されていません');
        return false;
    }
    //Mod 2019/09/04 yano #4011 
    //taxid=999(手計算）の場合は即リターン
    if (taxid != null && taxid == '999') {
        return false;
    }

    //販売区分が「AA」または「依廃」の場合は即リターン
    if (salesType != 'undefined' && (salesType.value == '004' || salesType.value == '008')) {
        return false;
    }

    //登録希望日が設定されていない場合はnull
    if (requestRegistDate == 'undefined') {
        requestRegistDate = null;
    }

    //車両本体価格がパラメータとして設定されていない場合はnull
    //if (salesprice == 'undefined') {
    //    salesprice = null;
    //}

    var optionAmount = isNaN(parseInt(ConversionExceptComma(document.getElementById('MakerOptionAmount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('MakerOptionAmount').value));

    //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/GetAcquisitionTax",
        data: { code: code, optionAmount: optionAmount, taxid: taxid, requestregistdate: requestRegistDate }, //Mod 2019/10/17 yano #4022
        //data: { code: code, optionAmount: optionAmount, taxid: taxid, salesprice: ConversionExceptComma(salesprice) }, //Mod 2019/09/04 yano #4011  
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 15000,
        success: function (data, dataType) {
            if (data != null) {

                //Mod 2020/11/17 yano #4065
                if (data.Item2 >= 0) {

                //document.getElementById('AcquisitionTax').value = ConversionWithComma(data);
                document.getElementById('AcquisitionTax').value = ConversionWithComma(data.Item2);

                //ドロップダウンの設定
                SetSelectedIndex('EPDiscountTaxList', data.Item1);

                //合計値の再計算
                calcCostAmountForCarSalesOrder();
                
                return true;
            }
                else {
                    alert("登録希望日が初度登録年月よりも前の日付となっています。\n確認のうえ修正を行ってください");    //Mod 2019/09/04 yano #4011
                    return false;
                }
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("自動車税環境性能割の取得に失敗しました。");    //Mod 2019/09/04 yano #4011
            return false;
        }
    });

    /*
    //カンマを取り除く
    ExceptComma(document.getElementById('MakerOptionAmount'));  //Add 2017/02/14  arc yano #3641

    var optionAmount = isNaN(parseInt(document.getElementById('MakerOptionAmount').value)) ? 0 : parseInt(document.getElementById('MakerOptionAmount').value);

    //カンマを挿入
    InsertComma(document.getElementById('MakerOptionAmount'));  //Add 2017/02/14  arc yano #3641 

    //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/GetAcquisitionTax",
        data: { code: code, optionAmount: optionAmount },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data != null) {
                document.getElementById('AcquisitionTax').value = data;

                //カンマを挿入
                InsertComma(document.getElementById('AcquisitionTax'));

                setTimeout('calcTotalAmount()', 100);
                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
        alert("取得税の取得に失敗しました。");
        return false;
    }
    });
    */

}

/*---------------------------------------------------------------------------------------------*/
/* 機能　： 自動車税種別割を取得して合計計算する                                               */
/* 作成日：                                                                                    */
/* 更新日：                                                                                    */
/*          2019/10/17 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り             */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 文言変更    */
/*          2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                     */
/*          取得したデータをカンマ付に変換する                                                 */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                          */
/*          Edit 2014/06/25 arc                                                                */
/*          ①タイムアウト値の変更(5000→10000)                                                */
/*          ②タイムアウト発生時のエラーハンドリング追加                                       */
/*          ③戻り値を明確                                                                     */
/*                                                                                             */
/*---------------------------------------------------------------------------------------------*/
function GetCarTax() {
    var carGradeCode = document.getElementById('CarGradeCode').value;
    var requestRegistDate = document.getElementById('RequestRegistDate').value

    var vin = document.getElementById('Vin').value; //Add 2019/10/17 yano #4023

    //Mod 2017/04/23 arc yano #3755
    if (carGradeCode && requestRegistDate) {
        //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
        $.ajax({
            type: "POST",
            url: "/CarTax/GetCarTax",
            data: { carGradeCode: carGradeCode, requestRegistDate: requestRegistDate, vin: vin },    //Mod  2019/10/17 yano #4023 
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data != null) {
                    document.getElementById('CarTax').value = ConversionWithComma(data);
                    setTimeout('calcTotalAmount()', 100);
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("自動車税種別割の取得に失敗しました。");   //Mod 2019/09/04 yano #4011
                return false;
            }
        });
    }
}

/*----------------------------------------------------------------------------------------------*/
/* 機能　： 下取車諸手続費用をセットする                                                        */
/* 作成日：                                                                                     */
/* 更新日：                                                                                     */
/*          2020/02/07 yano #4038 【車両伝票入力】下取車情報変更時の下取諸費用の計算の不具合    */
/*          2018/02/21 arc yano #3850 車両伝票入力　下取車諸手続費用の設定                      */
/*          2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                      */
/*          取得したデータをカンマ付に変換する                                                  */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                           */
/*          Edit 2014/06/26 arc                                                                 */
/*          ①タイムアウト値の変更(5000→10000)                                                 */
/*          ②タイムアウト発生時のエラーハンドリング追加                                        */
/*          ③戻り値を明確                                                                      */
/*                                                                                              */
/*----------------------------------------------------------------------------------------------*/
function setTradeInFee(num) {
    
    //Add 2020/02/07 yano #4038
    var costarecode = document.getElementById('CostAreaCode').value;

    if (costarecode == null || costarecode == '') {
        costarecode = '999';    //未定義で設定
    }

    //Add 2018/02/21 arc yano #3850
    var retflag = false;

    //全ての下取車の下取車充当金をチェックして、全て未入力の場合は、下取車諸費用を0円にする
    for (var i = 1; i <= 3; i++) {

        //下取車充当金をチェック
        if (document.getElementById('TradeInAmount' + i).value != '') {
            retflag = true;
            break;
        }
    }

    //下取車充当金が一つも入っていない場合
    if (retflag == false) {
        document.getElementById('TradeInFee').value = '';

        //再計算処理
        calcTotalAmount();

        return retflag;
    }

    //Mod 2020/02/07 yano #4038 引数をdepartmentCode→costareaCode
    //Mod 2017/04/23 arc yano #3755
    if ($('#TradeInFee').value == null || $('#TradeInFee').value == 0) {
        //var departmentCode = document.getElementById('DepartmentCode').value;
        //if (departmentCode) {
            //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
            $.ajax({
                type: "POST",
                url: "/CostArea/GetMasterDetail",
                //data: { departmentCode: departmentCode },
                data: { code: costarecode },
                contentType: "application/x-www-form-urlencoded",
                dataType: "json",
                timeout: 10000,
                success: function (data, dataType) {
                    if (data != null) {
                        document.getElementById('TradeInFee').value = ConversionWithComma(data["TradeInFee"]);
                        setTimeout('calcTotalAmount()', 100);
                        return true;
                    }
                }
                ,
                error: function () {  //通信失敗時
                    alert("下取車消費税の取得に失敗しました。");
                    return false;
                }
            });
        //}
    }
    if (document.getElementById('TradeInAmount' + num) && document.getElementById('TradeInAmount' + num)) {

        var amount = ConversionExceptComma(document.getElementById('TradeInAmount' + num).value);
        var tax = calcTaxAmountFromTotalAmount(amount);
        document.getElementById('TradeInTax' + num).value = ConversionWithComma(tax);

        return true;
    }
    //Del 2019/09/12 ref
    
}

//Del 2019/09/12 ref

/*------------------------------------------------------------------------------*/
/* 機能　： 下取車登録印紙代をセットする                                        */
/* 作成日：                                                                     */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合      */
/*          取得したデータをカンマ付に変換する                                  */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/*          Edit 2014/06/25 arc                                                 */
/*          ①タイムアウト値の変更(5000→10000)                                 */
/*          ②タイムアウト発生時のエラーハンドリング追加                        */
/*          ③戻り値を明確                                                      */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function setTradeInCost() {
    //下取台数
    var tradeInCount = 0;
    var eraseCount = 0;
    var tradeInCost = 0;

    for (var i = 1; i <= 3; i++) {
        if (document.getElementById('TradeInVin' + i).value != '' && document.getElementById('TradeInVin' + i).value != null) {
            if (document.getElementById('TradeInEraseRegist' + i).value != '') {
                eraseCount++;
            } else {
                tradeInCount++;
            }
        }
    }
    if (tradeInCount > 0 || eraseCount > 0) {
        //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
        $.ajax({
            type: "POST",
            url: "/ConfigurationSetting/GetMasterDetail",
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data != null) {
                    var tradeInCostValue = isNaN(parseInt(data["TradeInCost"])) ? 0 : parseInt(data["TradeInCost"]);
                    tradeInCost = tradeInCount * tradeInCostValue;

                    var tradeInEraseValue = isNaN(parseInt(data["TradeInCostErase"])) ? 0 : parseInt(data["TradeInCostErase"]);
                    tradeInCost += eraseCount * tradeInEraseValue;


                    //Mod 2017/04/23 arc yano #375
                    document.getElementById('TradeInCost').value = ConversionWithComma(tradeInCost);
                    //document.getElementById('TradeInCost').value = tradeInCost;

                    //カンマ挿入
                    //InsertComma(document.getElementById('TradeInCost')); //Add 2017/02/15 arc yano #3641

                    setTimeout('calcTotalAmount()', 100);
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("下取車登録印紙代の取得に失敗しました。");
                return false;
            }
        });
    }
}

// 車両重量と前前軸重から後後軸重を計算してセットする
function calcRRAxileWeight() {
    var carWeight = document.getElementById('CarWeight');
    var ffAxileWeight = document.getElementById('FFAxileWeight');
    var rrAxileWeight = document.getElementById('RRAxileWeight');
    var carWeightValue = isNaN(parseInt(carWeight.value)) ? 0 : parseInt(carWeight.value);
    var ffAxileWeightValue = isNaN(parseInt(ffAxileWeight.value)) ? 0 : parseInt(ffAxileWeight.value);
    if (carWeightValue > 0 && ffAxileWeightValue > 0) {
        rrAxileWeight.value = carWeightValue - ffAxileWeightValue;
    }
    return false;
}

// 車検有効期限から次回点検日を計算してセットする
//---------------------------------------------------
// Edit 2014/06/25 arc
// ①タイムアウト値の変更(5000→10000)
// ②タイムアウト発生時のエラーハンドリング追加
// ③戻り値を明確化
//---------------------------------------------------
function getNextInspectionDate(prefix) {
    var expireDate;
    var expireDateGengou;
    var expireDateYear;
    var expireDateMonth;
    var expireDateDay;
    var registrationNumberType;

    if (document.getElementById('InspectionExpireDate')) {
        expireDate = document.getElementById('InspectionExpireDate').value;
    }
    
    if (document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Gengou')) {
        expireDateGengou = document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Gengou').value;
    }
    if (document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Year')) {
        expireDateYear = document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Year').value;
    }
    if (document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Month')) {
        expireDateMonth = document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Month').value;
    }
    if (document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Day')) {
        expireDateDay = document.getElementById((prefix == null ? '' : prefix) + 'ExpireDateWareki.Day').value;
    }
    if (document.getElementById((prefix == null ? '' : prefix) + 'RegistrationNumberType')) {
        registrationNumberType = document.getElementById((prefix == null ? '' : prefix) + 'RegistrationNumberType').value;
    }
    if (expireDate != null || (expireDateGengou != null && expireDateYear != null && expireDateMonth != null && expireDateDay != null)) {
        //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
        $.ajax({
            type: "POST",
            url: "/SalesCar/GetNextInspectionDate",
            data: { registrationNumberType: registrationNumberType, expireDate: expireDate, gengou: expireDateGengou, year: expireDateYear, month: expireDateMonth, day: expireDateDay },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data != null) {
                    if (expireDate != null) {
                        if (confirm("次回点検日は" + data["Seireki"] + "でよろしいですか？")) {
                            document.getElementById('NextInspectionDate').value = data["Seireki"];
                        }
                    } else {
                        if (confirm("次回点検日は" + data["GengouName"] + data["Year"] + "年" + data["Month"] + "月" + data["Day"] + "日でよろしいですか？")) {
                            var element = document.getElementById((prefix == null ? '' : prefix) + 'NextInspectionDateWareki.Gengou');
                            for (var m = 0; m < element.childNodes.length; m = m + 2) {
                                if (element.childNodes[m].value == data['Gengou']) {
                                    element.selectedIndex = element.childNodes[m].index;
                                }
                            }

                            document.getElementById((prefix == null ? '' : prefix) + 'NextInspectionDateWareki.Year').value = data["Year"];
                            document.getElementById((prefix == null ? '' : prefix) + 'NextInspectionDateWareki.Month').value = data["Month"];
                            document.getElementById((prefix == null ? '' : prefix) + 'NextInspectionDateWareki.Day').value = data["Day"];
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("次回点検日の取得に失敗しました。");
                return false;
            }
        });
    }
}

//個別入金消込画面の合計計算
//-------------------------------------------------------------------------------------
// Mod 2014/07/09 arc yano chrome対応　getElementbyIdのパラメータをname→idに修正
//-------------------------------------------------------------------------------------
function calcDepositDetail() {
    var lineCount = document.getElementById('LineCount').value;
    var totalAmount = 0;
    for (var i = 0; i < lineCount; i++) {
        var amount = isNaN(parseInt(document.getElementById('line[' + i + ']_Amount').value)) ? 0 : parseInt(document.getElementById('line[' + i + ']_Amount').value);
        totalAmount += amount;
    }
    document.getElementById('TotalAmount').innerText = currency(totalAmount);
    var receivableBalance = isNaN(parseInt(document.getElementById('ReceivableBalance').value)) ? 0 : parseInt(document.getElementById('ReceivableBalance').value);
    document.getElementById('TotalBalance').innerText = currency(receivableBalance - totalAmount);
}


//ADD　2014/02/26 ookubo
//税率変更がキャンセルされた場合の処理（モード毎にOld値を差戻し）
function resumeOld(mode){
    switch (mode) {
        case "CarSalesOrder":
            document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("ConsumptionTaxId").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("SalesDate").value = (document.getElementById("DateOld1").value).substring(0, 10);
            document.getElementById("SalesPlanDate").value = (document.getElementById("DateOld2").value).substring(0, 10);
            break;
        case "ServiceSalesOrder":
            document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("ConsumptionTaxId").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("SalesDate").value = (document.getElementById("DateOld1").value).substring(0, 10);
            document.getElementById("SalesPlanDate").value = (document.getElementById("DateOld2").value).substring(0, 10);
            break;
        case "CarAppraisal":    //Edit 2014/06/20 arc yano 税率変更バグ対応
            document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("ConsumptionTaxId").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("PurchasePlanDate").value = (document.getElementById("DateOld2").value).substring(0, 10);

            break;
        case "CarPurchase":
            document.getElementById("ConsumptionTaxList").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("ConsumptionTaxId").value = document.getElementById("ConsumptionTaxIdOld").value;
            document.getElementById("PurchaseDate").value = (document.getElementById("DateOld2").value).substring(0, 10);
            break;
   }
}

//ADD　2014/02/26 ookubo
//税率変更処理続行判定
function confirmChangeConsumptionTaxId(newId, mode) {
    var oldId = document.getElementById("ConsumptionTaxIdOld")
    if (newId.value != oldId.value) {
        switch (mode){
            case "CarSalesOrder":
                if (confirm("納車日変更にともない、消費税率が変更されます。\n金額が変更となりますので、注文書の再発行並びに\n注文書の差し替えが必要です。\n\nオプションの金額は税込金額で入力されています。\n再計算された金額に問題がないかどうかをご確認ください")) {
                    return 1;
                }
                else {
                //キャンセルされた場合、Old値を差戻し
                    resumeOld(mode);
                    return -1;
                }
            case "ServiceSalesOrder":
                if (confirm("納車日変更にともない、消費税率が変更されます。\n金額が変更となりますのでご注意ください。")) {
                    return 1;
                }
                else {
                //キャンセルされた場合、Old値を差戻し
                    resumeOld(mode);
                    return -1;
                }
            default: 
                return 1;
        }
    }
    return 0;
}

//--------------------------------------------------------------------------------------------------------------------------
// 消費税率取得
//
// 機能：
//       IDから消費税率取得
//
// 作成：2014/02/26 arc ookubo
// 履歴：
//       2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業　外出ししたオプション計算処理を追加
//       2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合
//       取得したデータをカンマ付に変換する
//       2017/02/14 arc yano #3641 金額欄のカンマ表示対応
//       2014/07/04 arc yano chrome対応 input項目の値を変更する際は、innerHTMLではなく、
//                           ①getElementbyIdのパラメータの変更(name→id)
//                           ②"Rate"項目の値を変更する際は、innerHTMLではなく、
//                              valueを使う。
//       2014/06/23 arc yano 税率変更バグ修正 
//                           ①タイムアウト値の変更(5000→10000)
//                           ②タイムアウト時の処理を追加(error)
//                           ③submit可／不可フラグの更新を行う。
//                           ④引数追加(入力した日付)
//       2014/05/27 arc yano vs2012対応 リクエストメソッドの変更(GET→POST)
//-------------------------------------------------------------------------------------------------------------------------
function setTaxRateById(target, mode, dtarray) {

    document.getElementById("canSave").value = '0';

    //    alert(target.value);
    if (target.value == "") {
        document.getElementById("canSave").value = '1';
        return false;
    }
    else {
        var ConsumptionTaxId = target.value;
        //消費税率取得
        $.ajax({
            type: "POST",
            url: "/ConsumptionTax/GetRateById",
            data: { ConsumptionTaxId: ConsumptionTaxId },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data != null) {
                    var element = document.getElementById("Rate");
                    try {
                        //element.innerHTML = escapeHtml(data.Rate);
                        element.value = data.Rate;
                    } catch (e) {
                        element.value = data.Rate;
                    }
                    //更新前IDを更新後IDで置換
                    document.getElementById("ConsumptionTaxId").value = ConsumptionTaxId;
                    document.getElementById("ConsumptionTaxIdOld").value = ConsumptionTaxId;

                    //Add 2014/06/23 arc yano 税率変更バグ対応
                    //更新前日付を更新後の日付で置換
                    if (dtarray != null) {
                        document.getElementById("DateOld1").value = dtarray[0];
                        document.getElementById("DateOld2").value = dtarray[1];
                    }
                    //再計算
                    switch (mode) {
                        case "CarSalesOrder":
                            calcSalesPrice(1);
                            calcSalesPrice(2);
                            calcSalesPrice(3);
                            calcDiscountAmount(true)
                            calcDiscountAmount(false);
                            
                            var lineCount = document.getElementById("LineCount").value;
                            for (var i = 0; i < lineCount; i++) {
                                if (document.getElementById('line[' + i + ']_Amount').value != "") {

                                    //Mod 2017/04/23 arc yano #3755
                                    //カンマ除去
                                    //ExceptComma(document.getElementById('line[' + i + ']_Amount'));    //Add 2017/02/14 arc yano #3641

                                    var Amount = isNaN(parseInt(ConversionExceptComma(document.getElementById('line[' + i + ']_Amount').value))) ? 0 : parseInt(ConversionExceptComma(document.getElementById('line[' + i + ']_Amount').value));

                                    //カンマ挿入
                                    //InsertComma(document.getElementById('line[' + i + ']_Amount'));    //Add 2017/02/14 arc yano #3641

                                    var TaxAmount;
                                    //税抜きから再計算
                                    TaxAmount = calcTaxAmount(Amount);
                                    document.getElementById('line[' + i + ']_TaxAmount').value = ConversionWithComma(TaxAmount);
                                    document.getElementById('line[' + i + ']_AmountWithTax').value = ConversionWithComma(Amount + TaxAmount);

                                    //カンマ挿入
                                    //InsertComma(document.getElementById('line[' + i + ']_TaxAmount'));        //Add 2017/02/14 arc yano #3641
                                    //InsertComma(document.getElementById('line[' + i + ']_AmountWithTax'));    //Add 2017/02/14 arc yano #3641
                                }
                            }
                            calcTotalOptionAmount();                            //Add 2019/09/04 yano #4011
                            calcTotalAmount();
                            break;

                        case "ServiceSalesOrder":
                            var lineCount2 = document.getElementById("LineCount").value;
                            var ManPower;
                            var Qty;
                            for (var j = 0; j < lineCount2; j++) {
                                //2014/04/09.ookubo #3021]【サ】サービス伝票の原価欄が　０　になる 
                                //サービスタイプが部品の場合、原価合計計算を行う。 
                                switch (document.getElementById('line[' + j + ']_ServiceType').value) {
                                    case "001": //主作業
                                        break;
                                    case "002": //サービスメニュ
                                        calcTotalServiceAmount();   //2014/07/14 arc yano #3060
                                        break;
                                    case "003": //部品
                                        ManPower = document.getElementById('line[' + j + ']_ManPower');
                                        Qty = document.getElementById('line[' + j + ']_Quantity');
                                        if (ManPower.value != "" || Qty.value != "") {
                                            calcLineCost(j);
                                        }
                                        break;
                                }
                            }
                            break;

                        case "CarPurchase":
                            calcMetallicPrice(1);
                            calcMetallicPrice(2);
                            calcMetallicPrice(3);
                            calcVehiecleAmount(1);
                            calcVehiecleAmount(2);
                            calcVehiecleAmount(3);
                            calcOptionPrice(1);
                            calcOptionPrice(2);
                            calcOptionPrice(3);
                            calcAuctionFeeAmount(1);
                            calcAuctionFeeAmount(2);
                            calcAuctionFeeAmount(3);
                            calcFirmPrice(1);
                            calcFirmPrice(2);
                            calcFirmPrice(3);
                            calcDiscountPrice(1);
                            calcDiscountPrice(2);
                            calcDiscountPrice(3);
                            calcRecyclePrice();
                            calcEquipmentPrice(1);
                            calcEquipmentPrice(2);
                            calcEquipmentPrice(3);
                            calcOthersPrice(1);
                            calcOthersPrice(2);
                            calcOthersPrice(3);
                            calcRepairPrice(1);
                            calcRepairPrice(2);
                            calcRepairPrice(3);
                            calcCarTaxAppropriatePrice();
                            calcTotalPurchaseAmount();
                            break;
                    }
                    document.getElementById("canSave").value = '1';
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("消費税率の取得に失敗しました。");
                //Old値を差戻し
                resumeOld(mode);
                document.getElementById("canSave").value = '-1';
                return false;
            }
        });
    }
}

//--------------------------------------------------------------------------------------------
// 消費税率ID取得
//
// 機能：
//       複数の日付から優先順位判定の上、消費税IDと消費税率取得
// 　　　基準日が一つしかない画面は１，２両方に同じオブジェクトを指定する
// 作成：2014/02/26 arc ookubo
// 履歴：
//       2014/06/20 arc yano 税率変更バグ修正 
//                           ①税率変更ダイアログで「キャンセル」ボタン押下時の処理を追加
//                           ②処理の終わりに明示的に値を返すように修正。
//                           ③タイムアウト値の変更(5000→10000)
//                           ④タイムアウト時の処理を追加(error)
//--------------------------------------------------------------------------------------------
function setTaxIdByDate(dt1, dt2, mode) {
    
    document.getElementById("canSave").value = '0';

    if (!chkDate(dt1)) { 
        document.getElementById("canSave").value = '1';
        return false;
    }
    if (!chkDate(dt2)) { 
        document.getElementById("canSave").value = '1';
        return false;
    }
    //優先順位１の日付をデフォルトで採用
    var strDt = dt1.value;

    if (strDt == "") {
        //優先順位１が空の場合、優先順位２の日付を採用
        strDt = dt2.value;
    }

    //優先順位２も空でない場合のみ処理実行
    if (strDt != "") {
        //Add 2014/05/27 arc yano vs2012対応 リクエストメソッドを変更(GET→POST)
        $.ajax({
            type: "POST",
            url: "/ConsumptionTax/GetIdByDate",
            data: { strDt: strDt },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function(data, dataType) {
                if (data != null) {
                    var id = data.ConsumptionTaxId;
                    if (isNaN(id)) {
                        alert("消費税ID取得に失敗しました。");
                        //Old値を差戻し
                        resumeOld(mode);
                        document.getElementById("canSave").value = '1';
                        return false;
                    }
                    var element = document.getElementById("ConsumptionTaxList");
                    element.value = id;
                    switch (confirmChangeConsumptionTaxId(element, mode)) {
                        case 0:
                            document.getElementById("DateOld1").value = dt1.value;
                            document.getElementById("DateOld2").value = dt2.value;
                            document.getElementById("canSave").value = '1';
                            //alert("保存されました");
                            return true;
                            break;
                        case 1:
                            
                            //IDから税率を取得

                            //Edit 2014/06/23 arc yano ここでは、日付の待避を行わず、税率取得後に行う。
                            //document.getElementById("DateOld1").value = dt1.value;
                            //document.getElementById("DateOld2").value = dt2.value;
                            var dtarray = [];
                            dtarray[0] = dt1.value;
                            dtarray[1] = dt2.value;
                            setTaxRateById(element, mode, dtarray);
                            return true;
                            break;

                        default:
                            document.getElementById("canSave").value = '-1';
                            return false;
                            break;

                    }
                }
            } 
            ,
            error: function(){  //通信失敗時
                alert("消費税IDの取得に失敗しました。");
                //Old値を差戻し
                resumeOld(mode);
                document.getElementById("canSave").value = '-1';
                return false;
            }
        });
    }
    else {
        document.getElementById("canSave").value = '1';
        return true;
    }
}
function chkDate(dt) { 
    var val = dt.value
    if (val == '') {
        return true;
    }
    var y = val.substring(0, 4);
    var m = val.substring(5, 7);
    var d = val.substring(8, 10);
    if (m.substring(1, 2) == '/') {
        m = val.substring(5, 6);
        d = val.substring(7, 9);
    }
    //入力日付チェック
    var di = new Date(y, m - 1, d);
    if (!(di.getFullYear() == y && di.getMonth() == m - 1 && di.getDate() == d)) {
        alert('正しい日付を入力してください。');
        return false;
    }
    return true; 
}
//----------------------------------------------
// 機能：入力値チェック(日付)
// 作成：2014/08/16 IPO対応
// 更新：
//----------------------------------------------
function chkDate2(dt) {

    //未入力?
    if (dt == '') {
        alert('年月をYYYY/MM形式で入力してください。');
        return false;
    }

    //書式チェック
    if (!dt.match(/^\d{4}\/\d{2}$/)) {
        alert('年月をYYYY/MM形式で入力してください。');
        return false;
    }

    var vYear = dt.substr(0, 4) - 0;
    var vMonth = dt.substr(5, 2) - 1; // Javascriptは、0-11で表現
    var vDay = 1                      // 日は１日とする。

    //月日の妥当性チェック
    if (vMonth >= 0 && vMonth <= 11 && vDay >= 1 && vDay <= 31) {   //月日の範囲チェック
        var vDt = new Date(vYear, vMonth, vDay);
        if (isNaN(vDt)) {
            alert('年月をYYYY/MM形式で入力してください。');
            return false;
        }else if(vDt.getFullYear() == vYear && vDt.getMonth() == vMonth && vDt.getDate() == vDay){
            return true;
        }else{
            alert('年月をYYYY/MM形式で入力してください。');
            return false;
        }
    } else {
        alert('年月をYYYY/MM形式で入力してください。');
        return false;
    }
}

//----------------------------------------------
// 機能：入力値チェック(日付)
// 作成：2015/03/02 顧客DM対応  arc nakayama
// 更新：
//----------------------------------------------
function chkDateYYMM(dt, ColName) {

    //未入力?
    if (dt == '') {
        return true;
    }

    //書式チェック
    if (!dt.match(/^\d{4}\/\d{2}$/)) {
        alert('年月をYYYY/MM形式で入力してください。');
        document.getElementById(ColName).value = '';
        return false;
    }

    var vYear = dt.substr(0, 4) - 0;
    var vMonth = dt.substr(5, 2) - 1; // Javascriptは、0-11で表現
    var vDay = 1                      // 日は１日とする。

    //月日の妥当性チェック
    if (vMonth >= 0 && vMonth <= 11 && vDay >= 1 && vDay <= 31) {   //月日の範囲チェック
        var vDt = new Date(vYear, vMonth, vDay);
        if (isNaN(vDt)) {
            alert('年月をYYYY/MM形式で入力してください。');
            document.getElementById(ColName).value = '';
            return false;
        } else if (vDt.getFullYear() == vYear && vDt.getMonth() == vMonth && vDt.getDate() == vDay) {
            return true;
        } else {
            alert('年月をYYYY/MM形式で入力してください。');
            document.getElementById(ColName).value = '';
            return false;
        }
    } else {
        alert('年月をYYYY/MM形式で入力してください。');
        document.getElementById(ColName).value = '';
        return false;
    }
}

//----------------------------------------------
// 機能：入力値チェック(日付)
// 作成：2015/03/02 顧客DM対応 arc nakayama
// 更新：
//----------------------------------------------
function chkDate3(dt, ColName) {

    //未入力?
    if (dt == '') {
        return true;
    }

    //書式チェック
    if (!dt.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
        alert('年月日をYYYY/MM/DD形式で入力してください。');
        document.getElementById(ColName).value = '';
        return false;
    }

    var vYear = dt.substr(0, 4) - 0;
    var vMonth = dt.substr(5, 2) - 1; // Javascriptは、0-11で表現
    var vDay = dt.substring(8,10)     // 日は１日とする。

    //月日の妥当性チェック
    if (vMonth >= 0 && vMonth <= 11 && vDay >= 1 && vDay <= 31) {   //月日の範囲チェック
        var vDt = new Date(vYear, vMonth, vDay);
        if (isNaN(vDt)) {
            alert('年月日をYYYY/MM/DD形式で入力してください。');
            document.getElementById(ColName).value = '';
            return false;
        } else if (vDt.getFullYear() == vYear && vDt.getMonth() == vMonth && vDt.getDate() == vDay) {
            return true;
        } else {
            alert('年月日をYYYY/MM/DD形式で入力してください。');
            document.getElementById(ColName).value = '';
            return false;
        }
    } else {
        alert('年月日をYYYY/MM/DD形式で入力してください。');
        document.getElementById(ColName).value = '';
        return false;
    }
}
//消費税計算してかえす
function calcTaxAmount(amount) {
    //MOD　2014/02/20 ookubo
    //var rate = 0.05; 
    var rate = parseFloat(document.getElementById("Rate").value) / parseFloat(100);

    if (amount > 0) {
        return Math.floor((amount == null ? 0 : amount) * rate);
    } else {
        return Math.ceil((amount == null ? 0 : amount) * rate);
    }
}
function calcTaxAmountFromTotalAmount(totalAmount) {
    //MOD　2014/02/20 ookubo
    //var rate = 5 / 105;
    var x = parseFloat(document.getElementById("Rate").value)
    var y = x + parseFloat(100)
    var rate = (x / y);

    if (totalAmount > 0) {
        return Math.floor((totalAmount == null ? 0 : totalAmount) * rate);
    } else {
        return Math.ceil((totalAmount == null ? 0 : totalAmount) * rate);
    }
}


//------------------------------------------------------------------------
// 機能：検索フォームリセット
// 作成：????
// 更新：
//
//      2014/08/21 arc yano IPO対応 ドロップダウンリストのリセット処理追加 
//
//      2014/07/16 arc yano chrome対応 
//        getElementsByNameで取得できない場合はgetElementByIdで取得する。
//
//    　2014/01/27 arc ishii 非活性のドロップダウンをアクティブにする。
//------------------------------------------------------------------------
// resetEntityArrayにカンマ区切りでオブジェクト名を渡す
//Mod 2014/07/16 arc yano chrome対応 getElementsByNameで取得できない場合は
//                                   getElementByIdで取得する。
function resetCommonCriteria(resetEntityArray) {
    for (var i = 0; i < resetEntityArray.length; i++) {
        var element = document.getElementsByName(resetEntityArray[i]);
        var defaultElement = document.getElementById("Default" + resetEntityArray[i]);
        if (element) {
            if (element.length > 1) {
                for (var m = 0; m < element.length; m++) {
                    if (defaultElement && (element[m].value == defaultElement.value)) {
                        element[m].checked = true;
                    }
                    else {
                        element[m].checked = false;
                    }
                }
            }
            else if (element.length == 1){
            	
            	if (defaultElement) {
                if (element[0].tagName == "SPAN") {
                    element[0].innerText = defaultElement.value;
                    }
                    else if (element[0].tagName == "select") {
                        var option = element[0].getElementsByTagName('option');
                        for (i = 0; i < option.length; i++) {
                            if (option[i].value == defaultElement.value) {
                                option[i].selected = true;
                            }
                            else {
                                option[i].selected = false;
                            }
                        }
                    }
                    else {
                    element[0].value = defaultElement.value;
                        element[0].disabled = false;
                }
            	}
            	else {
                if (element[0].tagName == "SPAN") {
                    element[0].innerText = "";
                }
                    else {
                element[0].value = "";
                    }
                }
        	}
            else {   //nameで取れない場合、idで取得する。
                var idelement = document.getElementById(resetEntityArray[i]);

                if (defaultElement) {
                    if (idelement.tagName == "SPAN") {
                        idelement.innerText = defaultElement.value;
                    } else {
                        idelement.value = defaultElement.value;
                    }
                } else {
                    if (idelement.tagName == "SPAN") {
                        idelement.innerText = "";
                    }
                    else {
                        idelement.value = "";
                    }
                }
            }
        }
    }
}


//-----------------------------------------------------------------------------------------------------------
// 機能：車両伝票・諸費用金額クリア処理
// 作成：????
// 更新：
//      2023/10/5 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除
//-----------------------------------------------------------------------------------------------------------
function ClearCostAmount() {
    document.getElementById('AcquisitionTax').value = 0;
    document.getElementById('CarLiabilityInsurance').value = 0;
    document.getElementById('CarTax').value = 0;
    document.getElementById('CarWeightTax').value = 0;
    document.getElementById('ParkingSpaceCost').value = 0;
    document.getElementById('InspectionRegistCost').value = 0;
    document.getElementById('NumberPlateCost').value = 0;
    document.getElementById('TradeInCost').value = 0;
    document.getElementById('RecycleDeposit').value = 0;
    document.getElementById('RequestNumberCost').value = 0;
    document.getElementById('TaxFreeFieldName').value = "";
    //Mod 2014/08/08 arc amii バグ対応 #3070 自由項目の金額欄に「0」を入れないよう修正
    document.getElementById('TaxFreeFieldValue').value = "";
    //Add 2014/08/08 arc amii バグ対応 #3070 収入印紙代と下取自動車税預り金の金額を「0」にする処理追加
    document.getElementById('RevenueStampCost').value = 0;
    document.getElementById('TradeInCarTaxDeposit').value = 0;
    document.getElementById('InspectionRegistFee').value = 0;
    document.getElementById('InspectionRegistFeeTax').value = 0;
    document.getElementById('InspectionRegistFeeWithTax').value = 0;
    document.getElementById('RequestNumberFee').value = 0;
    document.getElementById('RequestNumberFeeTax').value = 0;
    document.getElementById('RequestNumberFeeWithTax').value = 0;
    document.getElementById('PreparationFee').value = 0;
    document.getElementById('PreparationFeeTax').value = 0;
    document.getElementById('PreparationFeeWithTax').value = 0;
    document.getElementById('FarRegistFee').value = 0;
    document.getElementById('FarRegistFeeTax').value = 0;
    document.getElementById('FarRegistFeeWithTax').value = 0;
    document.getElementById('TradeInFee').value = 0;
    document.getElementById('TradeInFeeTax').value = 0;
    document.getElementById('TradeInFeeWithTax').value = 0;
    document.getElementById('RecycleControlFee').value = 0;
    document.getElementById('RecycleControlFeeTax').value = 0;
    document.getElementById('RecycleControlFeeWithTax').value = 0;
    document.getElementById('TradeInAppraisalFee').value = 0;
    document.getElementById('TradeInAppraisalFeeTax').value = 0;
    document.getElementById('TradeInAppraisalFeeWithTax').value = 0;
    document.getElementById('ParkingSpaceFee').value = 0;
    document.getElementById('ParkingSpaceFeeTax').value = 0;
    document.getElementById('ParkingSpaceFeeWithTax').value = 0;

    document.getElementById('TaxationFieldName').value = "";
    //Mod 2014/08/08 arc amii バグ対応 #3070 自由項目の金額欄に「0」を入れないよう修正
    document.getElementById('TaxationFieldValue').value = "";
    document.getElementById('TaxationFieldValueTax').value = 0;
    document.getElementById('TaxationFieldValueWithTax').value = 0;

    //Mod 2023/10/5 yano #4184
    //中古車整備・点検費用
    //document.getElementById('InheritedInsuranceFee').value = 0;
    //document.getElementById('InheritedInsuranceFeeTax').value = 0;
    //document.getElementById('InheritedInsuranceFeeWithTax').value = 0;

    //中古車継承整備費用
    //document.getElementById('TradeInMaintenanceFee').value = 0;
    //document.getElementById('TradeInMaintenanceFeeTax').value = 0;
    //document.getElementById('TradeInMaintenanceFeeWithTax').value = 0;

    //Add 2023/10/5 yano #4184　前回の追加漏れ対応
    //管轄外登録手続費用
    document.getElementById('OutJurisdictionRegistFee').value = 0;
    document.getElementById('OutJurisdictionRegistFeeTax').value = 0;
    document.getElementById('OutJurisdictionRegistFeeWithTax').value = 0;
    //自動車税種別割未経過相当額
    document.getElementById('CarTaxUnexpiredAmount').value = 0;
    document.getElementById('CarTaxUnexpiredAmountTax').value = 0;
    document.getElementById('CarTaxUnexpiredAmountWithTax').value = 0;
    //自賠責未経過相当額
    document.getElementById('CarLiabilityInsuranceUnexpiredAmount').value = 0;
    document.getElementById('CarLiabilityInsuranceUnexpiredAmountTax').value = 0;
    document.getElementById('CarLiabilityInsuranceUnexpiredAmountWithTax').value = 0;
}

/*---------------------------------------------------------------------------------------------------*/
/* 機能　： サービス伝票・作業部品明細行の編集                       */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日： 2017/01/13  arc yano #3646 サービス伝票の全ての明細行削除後に伝票削除した時の不具合対応  */
/*---------------------------------------------------------------------------------------------------*/
function editList(obj) {

    // tbody要素に指定したIDを取得し、変数「tbody」に代入
    var tbody = document.getElementById("salesline-tbody");

    var classification1 = "";
    var customerClaimCode = "";

    var defaultserviceWorkCode = "";                                                 //Add 2017/01/13  arc yano #3646
    var defaultserviceWorkName = "";                                                 //Add 2017/01/13  arc yano #3646
    var defaultcustomerCode = "";                                               //Add 2017/01/13  arc yano #3646
    var defaultcustomerClaimName = "";                                               //Add 2017/01/13  arc yano #3646

    
    var editType = document.getElementById("EditType").value;                 //編集種別
    var lineCount = parseInt(document.getElementById("LineCount").value);     //行数
    
    var tmpobj = tbody.lastChild;
    var tmpcnt = lineCount;
    var strid1 = "";
    var strid2 = "";
    
    while (tmpcnt > 0) {
        if (tmpobj.nodeName == 'TR') {//行オブジェクト

                //ドロップダウン取得
                var objselect = tmpobj.getElementsByTagName('select');
                var j;
                for (j = 0; j < objselect.length; j++) {
                    if (objselect[j].name.match(/line\[\d{1,4}\]\.ServiceType/)) {
                        var index = objselect[j].selectedIndex;
                        var svtype = objselect[j].options[index].value;
                    }
                }

            if (tmpobj.style.display != 'none') {   //削除行でない場合

                if (svtype == "001") {  //主作業の場合
                    //隠しフィールド取得
                    var objInput = tmpobj.getElementsByTagName('INPUT');
                    var k;
                    for (k = 0; k < objInput.length; k++) {
                        if (objInput[k].type == 'hidden') {
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.Classification1/)) {
                                classification1 = objInput[k].value;
                            }
                        }
                        else if (objInput[k].type == 'text') {
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.CustomerClaimCode/)) {
                                customerClaimCode = objInput[k].value;
                            }
                            //Add 2017/01/13  arc yano #3646
                            //請求先コード
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.CustomerClaimCode/)) {
                                defaultcustomerClaimCode = objInput[k].value;
                            }
                            //請求先名
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.CustomerClaimName/)) {
                                defaultcustomerClaimName = objInput[k].value;
                            }
                            //主作業コードの取得
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.ServiceWorkCode/)) {
                                defaultserviceWorkCode = objInput[k].value;
                            }
                            //主作業名の取得
                            if (objInput[k].name.match(/line\[\d{1,4}\]\.LineContents/)) {
                                defaultserviceWorkName = objInput[k].value;
                            }

                        }
                    }
                    break;
                }
                tmpobj = tmpobj.previousSibling;
                tmpcnt--;
            }
            else {
                //Add 2017/01/13  arc yano #3646
                if (svtype == "001") {  //主作業の場合
                    //主作業フィールド取得
                    var objtarget = tmpobj.getElementsByTagName('INPUT');

                    for (var l = 0; l < objtarget.length; l++) {
                        //Add 2017/01/13  arc yano #3646
                        //請求先コード
                        if (objtarget[l].name.match(/line\[\d{1,4}\]\.CustomerClaimCode/)) {
                            defaultcustomerClaimCode = objtarget[l].value;
                        }
                        //請求先名
                        if (objtarget[l].name.match(/line\[\d{1,4}\]\.CustomerClaimName/)) {
                            defaultcustomerClaimName = objtarget[l].value;
                        }

                        //主作業コードの取得
                        if (objtarget[l].name.match(/line\[\d{1,4}\]\.ServiceWorkCode/)) {
                            defaultserviceWorkCode = objtarget[l].value;
                        }
                        //主作業名の取得
                        if (objtarget[l].name.match(/line\[\d{1,4}\]\.LineContents/)) {
                            defaultserviceWorkName = objtarget[l].value;
                        }
                    }
                }

                tmpobj = tmpobj.previousSibling;
                tmpcnt--;
            }
        }
        else {
            tmpobj = tmpobj.previousSibling;
        }
    }

    if (tmpcnt == 0) {  //表示行が無かった場合
        customerClaimCode = "";
        classification1 = "";
    }

    

    switch (editType) {

        //----------------------------
        // 行追加
        //----------------------------
        case "add":      
            addList(tbody, lineCount, classification1, customerClaimCode);
            break;

        //----------------------------
        // 行削除
        //----------------------------
        case "delete":
            removeList(obj, lineCount, classification1, customerClaimCode, defaultserviceWorkCode, defaultserviceWorkName, defaultcustomerClaimCode, defaultcustomerClaimName);            //Mod 2017/01/13  arc yano #3646
            break;
        //----------------------------
        // 行挿入
        //----------------------------
        case "insert":
            insertList(tbody, obj, lineCount, classification1, customerClaimCode);
            break;
        //----------------------------
        // 行コピー
        //----------------------------
        case "copy":
            copyList(tbody, obj, lineCount);
            break;
        //----------------------------
        // 上へ移動
        //----------------------------
        case "up":
            upList(tbody, obj);
            break;
        //----------------------------
        // 下へ移動
        //----------------------------
        case "down":
            downList(tbody, obj);
            break;
    }

    //合計値計算
    calcTotalServiceAmount();
}
//-------------------------------------------------------------------
// 機能　： 文字列置換                                               
// 作成日： 2014/06/11　arc yano 高速化対応                          
// 更新日： 2014/07/04  arc yano chrome対応                          
//                     ノードにより、innerHTML,outerHTMLを使い分け                      
//-------------------------------------------------------------------
function permuteStr(subject, keyword, changestr) {

    var i;
    var str;

    //subject内のinnerHTMLからkeywordの検索を行い、見つかった場合は、changestrに変換
    for (i = 0; i < subject.childNodes.length ; i++) {

        var child = subject.childNodes[i];

        if (child.childNodes.length != 0) { //子ノードが存在する場合
            str = child.innerHTML;
        }
        else {
            str = child.outerHTML;
        }

        if ((str != null) && (str != 'undefine')) {

            //innerHTML、またはouterHTMLよりkeywordを検索する。
            var result = str.match(keyword);

            if (result) {   //keywordが見つかった場合、changestrに置換
                str = str.replace(keyword, changestr);
                if (child.childNodes.length != 0) {
                child.innerHTML = str;
            }
                else {
                    child.outerHTML = str;
                }
            }
        }
    }
    return subject;
}

//-------------------------------------------------------------------------------------------------
// 機能　： イベントハンドラ設定                                     
// 作成日： 2014/06/11　arc yano 高速化対応                          
// 更新日：
//          2022/06/08 yano #4137 【部品入荷入力】入荷金額計算処理の不具合の対応
//          2022/01/08 yano #4121 【サービス伝票入力】Chrome・明細行の部品在庫情報取得の不具合対応
//          2017/04/26  arc yano #3755  金額欄のイベントハンドラ追加
//          2016/04/14  arc yano #3480  _SerchServiceWorkのopensearchdialog()のパラメータ変更
//          2014/07/11  arc yano chrome opensearchdialog()のパラメータ変更
//-------------------------------------------------------------------------------------------------
function setEventHandler(index) {

    var prefix = "line[" + index + "]";
    var doc = document;

    if (doc.getElementById(prefix + '_DelLine')) {
        doc.getElementById(prefix + '_DelLine').onclick = function () { doc.getElementById('EditType').value = 'delete'; doc.getElementById('EditLine').value = index; doc.getElementById('lineScroll').value = doc.getElementById('line').scrollTop; editList(this); };
    }
    if (doc.getElementById(prefix + '_InsLine')) {
        doc.getElementById(prefix + '_InsLine').onclick = function () { doc.getElementById('EditType').value = 'insert'; doc.getElementById('EditLine').value = index; doc.getElementById('lineScroll').value = doc.getElementById('line').scrollTop; editList(this); };
    }
    if (doc.getElementById(prefix + '_CopyLine')) {
        doc.getElementById(prefix + '_CopyLine').onclick = function () { doc.getElementById('EditType').value = 'copy'; doc.getElementById('EditLine').value = index; doc.getElementById('lineScroll').value = doc.getElementById('line').scrollTop; editList(this); };
    }
    if (doc.getElementById(prefix + '_UpLine')) {
        doc.getElementById(prefix + '_UpLine').onclick = function () { doc.getElementById('EditType').value = 'up'; doc.getElementById('EditLine').value = index; doc.getElementById('lineScroll').value = doc.getElementById('line').scrollTop; editList(this); };
    }
    if (doc.getElementById(prefix + '_DownLine')) {
        doc.getElementById(prefix + '_DownLine').onclick = function () { doc.getElementById('EditType').value = 'down'; doc.getElementById('EditLine').value = index; doc.getElementById('lineScroll').value = doc.getElementById('line').scrollTop; editList(this); };
    }
    if (doc.getElementById(prefix + '_ServiceType')) {
        doc.getElementById(prefix + '_ServiceType').onchange = function () { ChangeServiceType(index) };
    }
    if (doc.getElementById(prefix + '_ServiceWorkCode')) {
        doc.getElementById(prefix + '_ServiceWorkCode').onblur = function () { GetServiceWork(index) };
    }
    if (doc.getElementById(prefix + '_ServiceMenuCode')) {
        doc.getElementById(prefix + '_ServiceMenuCode').onchange = function () { GetCostFromServiceMenu(index) };
    }
    //Mod 2016/04/14 arc yano #3480
    if (doc.getElementById(prefix + '_SerchServiceWork')) {
        //2022/01/08 yano #4121
        doc.getElementById(prefix + '_SerchServiceWork').onclick = function () { var callback = function () { if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); GetServiceWork(index) }; openSearchDialog(prefix + '_ServiceWorkCode', prefix + '_LineContents', '/ServiceWork/CriteriaDialog?CCCustomerClaimClass=' + document.getElementById(prefix + '_CCCustomerClaimClass').value, null, null, null, null, callback); if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); GetServiceWork(index) };
        //doc.getElementById(prefix + '_SerchServiceWork').onclick = function () { openSearchDialog(prefix + '_ServiceWorkCode', prefix + '_LineContents', '/ServiceWork/CriteriaDialog?CCCustomerClaimClass=' + document.getElementById(prefix + '_CCCustomerClaimClass').value); if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); GetServiceWork(index) };
    }
    if (doc.getElementById(prefix + '_SerchCost')) {
        //2022/01/08 yano #4121
        doc.getElementById(prefix + '_SerchCost').onclick = function () { var callback = function () { if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); setTimeout('GetCostFromServiceMenu(' + index + ')', 500) }; openSearchDialog(prefix + '_ServiceMenuCode', prefix + '_LineContents', '/ServiceMenu/CriteriaDialog', null, null, null, null, callback); if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); setTimeout('GetCostFromServiceMenu(' + index + ')', 500) };
        //doc.getElementById(prefix + '_SerchCost').onclick = function () { openSearchDialog(prefix + '_ServiceMenuCode', prefix + '_LineContents', '/ServiceMenu/CriteriaDialog'); if (doc.getElementById(prefix + '_LineContents')) doc.getElementById(prefix + '_LineContents').focus(); setTimeout('GetCostFromServiceMenu(' + index + ')', 500) };
    }
    if (doc.getElementById(prefix + '_PartsNumber')) {
        doc.getElementById(prefix + '_PartsNumber').onchange = function () { GetPartsStockMasterFromCode(index) };
    }
    if (doc.getElementById(prefix + '_SerchPartsStock')) {
        //2022/01/08 yano #4121
        doc.getElementById(prefix + '_SerchPartsStock').onclick = function () { var callback = function () { if (doc.getElementById(prefix + '_LineContents')) document.getElementById(prefix + '_LineContents').focus(); setTimeout('GetPartsStockMasterFromCode(' + index + ')', 500) }; openSearchDialog(prefix + '_PartsNumber', prefix + '_LineContents', '/PartsStock/CriteriaDialog?CarBrandName=' + encodeURI(document.getElementById('CarBrandName').value), null, null, null, null, callback); if (doc.getElementById(prefix + '_LineContents')) document.getElementById(prefix + '_LineContents').focus(); setTimeout('GetPartsStockMasterFromCode(' + index + ')', 500) };
        //doc.getElementById(prefix + '_SerchPartsStock').onclick = function () { openSearchDialog(prefix + '_PartsNumber', prefix + '_LineContents', '/PartsStock/CriteriaDialog?CarBrandName=' + encodeURI(document.getElementById('CarBrandName').value)); if (doc.getElementById(prefix + '_LineContents')) document.getElementById(prefix + '_LineContents').focus(); setTimeout('GetPartsStockMasterFromCode(' + index + ')', 500) };
    }
    if (doc.getElementById(prefix + '_SetMenu')) {
        doc.getElementById(prefix + '_SetMenu').onclick = function () { openSetMenu(index) };
    }
    if (doc.getElementById(prefix + '_Quantity')) {
        doc.getElementById(prefix + '_Quantity').onchange = function () { calcLineCost(index) };
    }
    if (doc.getElementById(prefix + '_UnitCost')) {
        doc.getElementById(prefix + '_UnitCost').onchange = function () { calcLineCost(index) };
    }
    if (doc.getElementById(prefix + '_LineType')) {
        doc.getElementById(prefix + '_LineType').onchange = function () { changeOutsourceEmployee(index) };
    }
    if (doc.getElementById(prefix + '_EmployeeNumber')) {
        doc.getElementById(prefix + '_EmployeeNumber').onblur = function () { GetEngineerFromCode(index, 'EmployeeNumber') };
    }
    if (doc.getElementById(prefix + '_EmployeeCode')) {
        doc.getElementById(prefix + '_EmployeeCode').onblur = function () { GetEngineerFromCode(index, 'EmployeeCode') };
    }


    //Mod 2022/06/08 yano #4137
    //Add 2017/04/26 arc yano #3755
    //if (doc.getElementById(prefix + '_TaxAmount')) {
    //    doc.getElementById(prefix + '_TaxAmount').onblur = function () { InsertComma(doc.getElementById(prefix + '_TaxAmount')) };
    //}
    //if (doc.getElementById(prefix + '_AmountWithoutTax')) {
    //    doc.getElementById(prefix + '_AmountWithoutTax').onblur = function () { InsertComma(doc.getElementById(prefix + '_AmountWithoutTax')) };
    //}
    //if (doc.getElementById(prefix + '_TechnicalFeeAmountWithoutTax')) {
    //    doc.getElementById(prefix + '_TechnicalFeeAmountWithoutTax').onblur = function () { InsertComma(doc.getElementById(prefix + '_TechnicalFeeAmountWithoutTax')) };
    //}
    
    //if (doc.getElementById(prefix + '_LaborRate')) {
    //    doc.getElementById(prefix + '_LaborRate').onblur = function () { InsertComma(doc.getElementById(prefix + '_LaborRate')) };
    //}
    //if (doc.getElementById(prefix + '_TechnicalFeeAmount')) {
    //    doc.getElementById(prefix + '_TechnicalFeeAmount').onblur = function () { InsertComma(doc.getElementById(prefix + '_TechnicalFeeAmount')) };
    //}
    //if (doc.getElementById(prefix + '_Price')) {
    //    doc.getElementById(prefix + '_Price').onblur = function () { InsertComma(doc.getElementById(prefix + '_Price')) };
    //}
    //if (doc.getElementById(prefix + '_Amount')) {
    //    doc.getElementById(prefix + '_Amount').onblur = function () { InsertComma(doc.getElementById(prefix + '_Amount')) };
    //}
    //if (doc.getElementById(prefix + '_UnitCost')) {
    //    doc.getElementById(prefix + '_UnitCost').onblur = function () { InsertComma(doc.getElementById(prefix + '_UnitCost')) };
    //}
    //if (doc.getElementById(prefix + '_Cost')) {
    //    doc.getElementById(prefix + '_Cost').onblur = function () { InsertComma(doc.getElementById(prefix + '_Cost')) };
    //}
}
//-------------------------------------------------------------------
// 機能　： 行追加                                                   
// 作成日： 2014/06/11 arc yano 高速化対応
// 更新日： 
//          2022/06/08 yano #4137 【部品入荷入力】入荷金額計算処理の不具合の対応
//          2016/04/14 arc yano #3480 サービス伝票　請求先取得の引数追加
//          201408/07/04 arc yano chrome対応
//                      TRノードの取得方法を変更
//-------------------------------------------------------------------
function addList(tbody, lncnt, classification, customerclaimcode) {

    var i = 0, j = 0, k = 0;
    var tr = null;

    //追加行数
    var addSize = parseInt(document.getElementById("AddSize").value);

    var srcSvType = document.getElementById("ServiceType");
    var valServceType = srcSvType.options[srcSvType.selectedIndex].value;

    for (i = 0; i < addSize; i++) {

        // tbodyタグのTRノードを複製し、変数「tr」に代入
        var tr = searchTr(tbody);

        //------------------------------
        // 追加行のHTML書換
        //------------------------------
        //行数を加算
        lncnt++;

        var index = lncnt - 1;
        var re = /line\[\d{1,4}\]/g;                //検索キー
        var strprefix = "line[" + index + "]";      //置換文字列

        //文字列置換
        tr = permuteStr(tr, re, strprefix);

        //非表示から表示へ
        tr.style.display = 'table-row';

        tbody.insertBefore(tr, tbody.lastChild);

        // 追加行のイベントハンドラ設定
        setEventHandler(index);

        //--------------------------------
        // 初期値設定
        //--------------------------------

        //削除対象Flagをoffに
        document.getElementById(strprefix + "_CliDelFlag").value = "";

        //種別の設定
        var dstSvType = document.getElementById(strprefix + "_ServiceType");

        for (j = 0; j < dstSvType.options.length; j++) {
            var option = dstSvType.options[j];

            if (option.value == valServceType) {
                option.selected = true;
            }
            else {
                option.selected = false;
            }
        }

        //サービスタイプ変更
        ChangeServiceType(index);

        //分類の設定
        document.getElementById(strprefix + "_Classification1").value = classification;

        //請求先の設定
        document.getElementById(strprefix + "_CustomerClaimCode").value = customerclaimcode;

        //請求先(名称、分類等)の設定
        //if (document.getElementById(strprefix + "_CustomerClaimCode").value != "") {
        //GetNameFromCode(strprefix + '_CustomerClaimCode', strprefix + '_CustomerClaimName', 'CustomerClaim');
        GetNameFromCode(strprefix + '_CustomerClaimCode', strprefix + '_CustomerClaimName', 'CustomerClaim', null, null, null, strprefix + '_CCCustomerClaimClass');
        //}

        //追加行の種別が「サービスメニュー」の場合の初期設定
        if (valServceType == "002") {
            //レバレートの設定
            var laborrate = document.getElementById("LaborRate");

            if (laborrate != null) {
                document.getElementById(strprefix + "_LaborRate").value = laborrate.value;
            }
            else {
                document.getElementById(strprefix + "_LaborRate").value = 0;
            }

            //外注先の設定
            setOuter(index, strprefix);
        }   
    }
    document.getElementById("LineCount").value = lncnt;

    //スクロール位置調整
    var divline = document.getElementById("line");
    divline.scrollTop = divline.scrollHeight;

    //Add 2022/06/08 yano #4137
    setAttribute();

    //test
    //alert(document.getElementById("LineCount").value);
}


/*-------------------------------------------------------------------------------------*/
/* 機能　： 行削除                                                   */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日： 2017/01/13  arc yano #3646 全行削除した場合は主作業の明細行を自動的に追加  */
/*-------------------------------------------------------------------------------------*/
function removeList(obj, lncnt, classification, customerClaimCode, defaultserviceWorkCode, defaultserviceWorkName, defaultcustomerClaimCode, defaultcustomerClaimName) {

    // 削除対象の行を取得
    var editline = document.getElementById("EditLine").value;

    // tbody要素に指定したIDを取得し、変数「tbody」に代入
    var tbody = document.getElementById("salesline-tbody");
    // objの親の親のノードを取得し（つまりtr要素）、変数「tr」に代入
    var tr = obj.parentNode.parentNode.parentNode;

    // trを非表示にする。
    tr.style.display = "none";
   
    // 削除行の削除フラグをONにする。
    document.getElementById("line[" + editline + "]_CliDelFlag").value = "1";
   
    
    //Add 2017/01/13 arc yano #3646
    //全行削除された場合は、主作業の明細行を追加する。
    var addflg = true;

    var nexttr = tbody.firstChild;

    //行検索
    while (true) {

        if (nexttr != null) {//下に行が存在する場合
            if (nexttr.nodeName == "TR") {
                //下の行が非表示(=削除対象行)ではない場合
                if (nexttr.style.display != "none") {
                    addflg = false;
                    break;
                }
                else {  //下の行が非表示(=削除対象行)の場合
                    nexttr = nexttr.nextSibling;  //さらにひとつ下の行をチェックする。
                }
            }
            else {  //その他の場合
                nexttr = nexttr.nextSibling;
            }
        }
        else {  //下に行が存在しない場合
            break;
        }
    }

    //追加フラグがtrueの場合には主作業の明細行を１行追加
    if (addflg == true) {
        
        insertList(tbody, obj, lncnt, classification, customerClaimCode, '001', defaultserviceWorkCode, defaultserviceWorkName, defaultcustomerClaimCode, defaultcustomerClaimName);
    }
   
    //test
    //alert(document.getElementById("LineCount").value);
}

//----------------------------------------------------------------------------------
// 機能　： 行挿入                                                   
// 作成日： 2014/06/11　arc yano 高速化対応  
// 更新日
//       ： 2022/06/08 yano #4137 【部品入荷入力】入荷金額計算処理の不具合の対応
//       ： 2017/01/13  arc yano #3646 種別を引数で渡された時はその種別を設定する
//       ： 2016/04/14  arc yano  #3480 サービス伝票　請求先取得の引数追加
//       ： 2014/10/09  arc yano #3105_請求先の設定誤り対応
//       ： 2014/07/04  arc yano chrome対応
//                      TRの取得方法の変更()
//---------------------------------------------------------------------------------
function insertList(tbody, obj, lncnt, classification, customerClaimCode, serviceType, defaultserviceWorkCode, defaultserviceWorkName, defaultcustomerClaimCode, defaultcustomerClaimName) {

    var valcustomerClaimCode;   //請求先

    var j = 0, k = 0;


    //Mod 2017/01/13  arc yano #3646
    //追加行の種別
    var valServceType;

    if (typeof (serviceType) !== 'undefined' && typeof (serviceType) !== null) {     //引数が設定されている場合
        valServceType = serviceType;
    }
    else {

    var srcSvType = document.getElementById("ServiceType");

        valServceType = srcSvType.options[srcSvType.selectedIndex].value;
    }

    // tbodyタグのTRノードを複製し、変数「tr」に代入
    var tr = searchTr(tbody);

    //請求先
    valcustomerClaimCode = SetCustomerClaimCode(obj, customerClaimCode);

    //------------------------------
    // 挿入行のHTML書換
    //------------------------------
    //行数加算
    lncnt++;

    var index = lncnt - 1;

    var re = /line\[\d{1,4}\]/g;            //検索キー
    var strprefix = "line[" + index + "]";  //置換文字列

    //文字列置換
    tr = permuteStr(tr, re, strprefix);

    //非表示から表示へ
    tr.style.display = 'table-row';

    //「行挿入」ボタンが押された行の前に追加する。
    tbody.insertBefore(tr, obj.parentNode.parentNode.parentNode);

    // 挿入行のイベントハンドラ設定
    setEventHandler(index);

    //--------------------------------
    // 初期値設定
    //--------------------------------

    //削除対象Flagをoffに
    document.getElementById(strprefix + "_CliDelFlag").value = "";

    //請求先の設定
    document.getElementById(strprefix + "_CustomerClaimCode").value = valcustomerClaimCode;

    //請求先名の設定
    //GetNameFromCode(strprefix + '_CustomerClaimCode', strprefix + '_CustomerClaimName', 'CustomerClaim');
    GetNameFromCode(strprefix + '_CustomerClaimCode', strprefix + '_CustomerClaimName', 'CustomerClaim', null, null, null, strprefix + '_CCCustomerClaimClass');   //Mod 2016/04/14

    //サービスタイプの設定
    var dstSvType = document.getElementById(strprefix + "_ServiceType");

    for (j = 0; j < dstSvType.options.length; j++) {
       var option = dstSvType.options[j];

       if (option.value == valServceType) {
           option.selected = true;
       }
       else {
           option.selected = false;
       }
    }

    //サービスタイプ変更
    ChangeServiceType(index);

    //分類の設定    
    document.getElementById(strprefix + "_Classification1").value = classification;

    
    //追加行の種別が「サービスメニュー」の場合の初期設定
    if (valServceType == "002") {
    
        //レバレートの設定
        var laborrate = document.getElementById("LaborRate");

        if (laborrate != null) {
            document.getElementById(strprefix + "_LaborRate").value = laborrate.value;
        }
        else {
            document.getElementById(strprefix + "_LaborRate").value = 0;
        }

        //外注先の設定
        setOuter(index, strprefix);
    }

    document.getElementById("LineCount").value = lncnt;

    //Add 2017/01/13  arc yano #3646
    if (typeof (defaultserviceWorkCode) !== 'undefined' && typeof (defaultserviceWorkCode) !== null) {     //主作業コードが設定されている場合
        document.getElementById(strprefix + "_ServiceWorkCode").value = defaultserviceWorkCode;
    }
    if (typeof (defaultserviceWorkName) !== 'undefined' && typeof (defaultserviceWorkName) !== null) {     //主作業名が設定されている場合
        document.getElementById(strprefix + "_LineContents").value = defaultserviceWorkName;
    }
    if (typeof (defaultcustomerClaimCode) !== 'undefined' && typeof (defaultcustomerClaimCode) !== null) { //請求先コードが設定されている場合
        document.getElementById(strprefix + "_CustomerClaimCode").value = defaultcustomerClaimCode;
    }
    if (typeof (defaultcustomerClaimName) !== 'undefined' && typeof (defaultcustomerClaimName) !== null) {   //請求先名が設定されている場合
        document.getElementById(strprefix + "_CustomerClaimName").value = defaultcustomerClaimName;
    }

    setAttribute(); //Mod 2022/06/08 yano #4137

    //for debug
    //alert(document.getElementById(strprefix + "_CustomerClaimCode").value);
    //alert(document.getElementById(strprefix + "_CustomerClaimName").value);
    //alert(document.getElementById("LineCount").value);
}

/*----------------------------------------------------------------------------------*/
/* 機能　： 行コピー                                                                */
/* 作成日： 2014/06/11　arc yano 高速化対応                                         */
/* 更新日：                                                                         */
/*          2022/06/08 yano #4137 【部品入荷入力】入荷金額計算処理の不具合の対応    */
/*          2015/10/28 #3289 arc yano 部品仕入機能改善(サービス伝票入力)            */
/*----------------------------------------------------------------------------------*/
function copyList(tbody, obj, lncnt) {

    //「行コピー」ボタンが押された行を取得
    var src = obj.parentNode.parentNode.parentNode;

    // コピー行を作成
    var dst = src.cloneNode(true);

    //------------------------------
    // コピー行のHTML書換
    //------------------------------
    //行数加算
    lncnt++;

    var index = lncnt - 1;

    var re = /line\[\d{1,4}\]/g;            //検索キー
    var strprefix = "line[" + index + "]"   //置換文字列

    //文字列置換
    
    dst = permuteStr(dst, re, strprefix);

    // コントロールの選択状態のコピー
    copyState(src, dst);

    //非表示から表示へ
    //tr.style.display = 'table-row';

    //「行コピー」ボタンが押された行の次に追加する。
    tbody.insertBefore(dst, src.nextSibling);

    // 挿入行のイベントハンドラ設定
    setEventHandler(index);

    //ChangeServiceType(index);

    //行数更新
    document.getElementById("LineCount").value = lncnt;

    //test
    //alert(document.getElementById("LineCount").value);

    //Add  2015/10/28 #3289
    //引当済数、発注数は初期化する。
    document.getElementById(strprefix + '_ProvisionQuantity').value = '0';
    document.getElementById(strprefix + '_OrderQuantity').value = '0';

    //Mod 2022/06/08 yano #4137
    setAttribute();
}

/*-------------------------------------------------------------------*/
/* 機能　： コントロール選択状態コピー                               */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function copyState(from, to) {

    var tagname = from.tagName;
    

    if((tagname == 'INPUT') || tagname == 'input'){ //input項目の場合
       
        switch (from.type) {

            case 'radio':       //ラジオボタン
            case 'checkbox':    //チェックボックス   

                to.checked = from.checked; 
                break;


            case 'text':        //テキストボックス
            case 'textarea':    //テキストエリア

                to.value = from.value; 
                break;
        }
    }
    else if (typeof (from.selected) != 'undefined') {
        to.selected = from.selected;    //プルダウン(IE)
    }


    var node, copy;
    for (var i = 0; i < from.childNodes.length; i++) {
        node = from.childNodes.item(i);
        copy = to.childNodes.item(i);

        copyState(node, copy);
    }
}

/*-------------------------------------------------------------------*/
/* 機能　： 上へ移動                                                 */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function upList(tbody, obj) {

    //「上へ移動」ボタンが押された行を取得
    var tr = obj.parentNode.parentNode.parentNode;
    // 一つ上の行を取得
    var pretr = tr.previousSibling;

    while (true) {
        //上に行が存在する場合
        if (pretr != null) {
            if (pretr.nodeName == "TR") {   //行の場合
                //上の行が非表示(=削除対象行)ではない場合
                if (pretr.style.display != "none") {
                    // 「tr」を直前の兄弟ノードの上に挿入
                    tbody.insertBefore(tr, pretr);
                    break;
                }
                else {  //上の行が非表示(=削除対象行)の場合
                    pretr = pretr.previousSibling;  //さらにひとつ上の行をチェックする。
                }
            }
            else {//行以外
                pretr = pretr.previousSibling;  //さらにひとつ上の行をチェックする。
            }
        }
        else {//上に行が存在しない場合
            break;
        }
    }
}
/*-------------------------------------------------------------------*/
/* 機能　： 下へ移動                                                 */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function downList(tbody, obj) {

    //「下へ移動」ボタンが押された行を取得
    var tr = obj.parentNode.parentNode.parentNode;
    // 一つ下の行を取得
    var nexttr = tr.nextSibling;

    while (true) {


        if (nexttr != null) {//下に行が存在する場合
            if (nexttr.nodeName == "TR") {
            //下の行が非表示(=削除対象行)ではない場合
            if (nexttr.style.display != "none") {
                // 「tr」を直前の兄弟ノードの上に挿入
                tbody.insertBefore(tr, nexttr.nextSibling);
                break;
            }
            else {  //下の行が非表示(=削除対象行)の場合
                nexttr = nexttr.nextSibling;  //さらにひとつ下の行をチェックする。
            }
        }
            else{  //その他の場合
                nexttr = nexttr.nextSibling;
            }
        }
        else {  //下に行が存在しない場合
            break;
        }
    }
}

/*-------------------------------------------------------------------*/
/* 機能　： 明細行HTML修正処理                                       */
/* 作成日： 2014/06/16　arc yano 高速化対応                          */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function formList() {

    //------------------------------
    // name属性,LineNumber振り直し
    //------------------------------
    // tbody要素に指定したIDを取得し、変数「tbody」に代入
    var tbody = document.getElementById("salesline-tbody");

    //行数
    var lineCount = parseInt(document.getElementById("LineCount").value);
    var i;
    var lineNumber = 1;
    var prefix = 0;

    for (i = 0; i < tbody.childNodes.length; i++) {
        var child = tbody.childNodes[i];
        if (child.nodeName == 'TR') {    //子ノードが行の場合
            //Edit 2014/07/07 arc yano 高速化・不具合対応 削除行もLineNumberを振りなおす。
            //if (child.style.display != 'none') {    //非表示ではない(=削除対象でない)場合
                //隠しフィールド取得
                var objInput = child.getElementsByTagName('INPUT');
                var j;
                for (j = 0; j < objInput.length; j++) {
                    if (objInput[j].type == 'hidden') {

                        if (objInput[j].name.match(/line\[\d{1,4}\]\.LineNumber/)) {

                            if (child.style.display != 'none') {    //非表示ではない(=削除対象でない)場合
                                objInput[j].value = lineNumber;
                                lineNumber++;
                            }
                            else {
                                objInput[j].value = 999;     //非表示行のLineNumberは999とする。
                            }
                        }
                    }
                }
            //}
           // else{   //非表示行(=削除対象行)の場合。
           //    tbody.removeChild(child);
           // }

            /*
            //name属性値変更
            // tbodyタグ直下のノード（行）を複製し、変数「list」に代入
            var re = /line\[\d{1,4}\]/g;
            var strprefix = "line[" + prefix + "]";

            child = permuteStr(child, re, strprefix);
            prefix++;
            */
        }
    }

    //alert("処理完了");
}

/*-------------------------------------------------------------------*/
/* 機能　： 外注先（サービスメニュー）の設定                         */
/* 作成日： 2014/06/11　arc yano 高速化対応                          */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function setOuter(idx, prefix) {

    //外注先の設定(追加行の直近の「サービスメニュー」行と同じものを設定する。)
    //追加した行を取得する。
    var chkTr = document.getElementById(prefix + "_CliDelFlag").parentNode.parentNode;
    var preTr = chkTr.previousSibling;

    while (preTr) { //追加した行から上方向へ行単位でチェックする。

        if (preTr.nodeName == 'TR') {    //子ノードが行の場合
            if (preTr.style.display != 'none') {    //非表示でない(=削除対象でない)場合
                //隠しフィールド取得
                var objselect = preTr.getElementsByTagName('SELECT');
                var j;
                for (j = 0; j < objselect.length; j++) {
                    if (objselect[j].name.match(/line\[\d{1,4}\]\.ServiceType/)) {
                        var svtype = objselect[j].value;
                    }
                }

                if (svtype == "002") {  //「サービスメニュー」の場合
                    //さらに検索
                    var objInput = preTr.getElementsByTagName('INPUT');
                    for (j = 0; j < objInput.length; j++) {
                        if (objInput[j].type == "text") {
                            if (objInput[j].name.match(/line\[\d{1,4}\]\.EmployeeCode/)) {
                                var employeecode = objInput[j].value;
                            }
                        }
                    }
                    document.getElementById(prefix + "_EmployeeCode").value = employeecode;
                    break;
                }
            }
        }
        preTr = preTr.previousSibling;
    }

    //外注先の社員名、社員コードの設定
    if (document.getElementById(prefix + "_EmployeeCode").value != "") {
        GetEngineerFromCode(idx, 'EmployeeCode');
    }

}

//-----------------------------------------------------------------------------------------------------------
// 機能　　： サブミット可／不可チェック                                       
//          　各画面の保存ボタンクリック時には、税率変更(setTaxIDbyDate())       
//          　が処理中かどうかを監視し、処理中の場合は処理が終わるまで、       
//          　submit処理を行わない。                                  　       
// 引数  　： dispId(呼び出し元の画面)                                         
// 作成日　： 2014/06/20 arc yano 税率変更バグ対応                            
// 更新日　： 
//            2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 オプション計算処理追加
//            2017/03/06 arc yano #3640 車両仕入入力の計算がおかしい問題の対応
//-----------------------------------------------------------------------------------------------------------
function chkSubmit(dispId) {
    
    //alert("canSave:" + document.getElementById("canSave").value + "rate:" + document.getElementById("Rate").value);

    switch (document.getElementById("canSave").value) {

        case '-1':          //キャンセル
            document.getElementById("canSave").value = '1';
            return false;
            break;

        case '0':           //POST保留(再度自分呼び出し)
            setTimeout("chkSubmit('" + dispId + "')", 300);
            break;

        case '1':           //POST可
            if (dispId == 'ServiceSalesOrder') {//サービス伝票
                calcTotalServiceAmount();
                serviceFormSubmit();
            }
            //Add  2017/03/06 arc yano #3640
            else if (dispId == 'CarPurchase') {//車両仕入
                var ret = 0;

                //カンマ除去
                ExceptCommaAll();

                ret = ChkCarPurhcasePrice();        //税抜価格のチェック

                //税抜価格のチェックがOKだった場合は税込価格のチェックを行う
                if(ret == 0){
                    ret = ChkCarPurhcaseAmount();   //税込価格のチェック
                }


                //税抜価格のチェック、または税込価格のチェックでNGだった場合
                if (ret != 0) {
                    alert('(z)の金額が(a)～(k)の合計と一致しません。金額を確認後、再度保存処理を行って下さい。');
                    document.forms[0].action.value = '';
                    document.getElementById('reportParam').value = '';
                    document.forms[0].PrintReport.value = '';

                    //カンマ挿入
                    InsertCommaAll();
                    return false;
                }
                /*
                //税抜価格のチェック、または税込価格のチェックでNGだった場合
                if(ret != 0){
                    if (confirm('(z)の金額が(a)～(k)の合計と一致しません。処理を続行しますか？') != true) {
                        document.forms[0].action.value = '';

                        //カンマ挿入
                        InsertCommaAll();
                        return false;
                    }
                }
                */

                formSubmit();
            }
            else {//サービス伝票以外(車両伝票、車両査定、車両仕入)
                if (dispId == 'CarSalesOrder') {//車両伝票
                    calcTotalOptionAmount();    //Add 2019/09/04 yano #4011

                    //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());       //Add 2019/09/04 yano #4011
                    calcTotalAmount();          //合計値計算
                    // Add 2014/09/29 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグが立っている顧客の場合、警告メッセージを表示させる
                    if (document.getElementById('AddressReconfirm').value == 'True') {
                        alert('住所要確認となっているので、登録された住所をお客様にご確認お願いします');
                    }
                }

                formSubmit();                   //submit処理
            }
            break;

        default:            //状態不明
            alert("状態不明なため、submitしません。");
            break;
    }
    return true;
}

//-------------------------------------------------------------------
// 機能　： TRオブジェクト検索                                       
// 作成日： 2014/07/07　arc yano 高速化対応                          
// 更新日：                                                          
//-------------------------------------------------------------------
function searchTr(tbody) {

    // tbodyタグのTRノードを複製し、変数「tr」に代入
    for (k = 0; k < tbody.childNodes.length; k++) {
        if (tbody.childNodes[k].nodeName == 'TR') {
            tr = tbody.childNodes[k].cloneNode(true);
            break;
        }
    }

    if (k >= tbody.childNodes.length) { //TRが見つからなかった場合
        return null
    }
    else {
        return tr
    }
}

//------------------------------------------------------------------------------
// 機能　　： 数値比較                                       
//          　引数１、引数２を比較し、値が異なっていた場合はtrue,
//          　それ以外はfalseを返す。                           　       
// 引数  　： 数値１、数値２                                         
// 作成日　： 2014/07/16　arc yano 税率変更バグ対応                            
// 更新日　：                                                                  
//---------------------------------------------------------------------------
function cmpValue(num1, num2) {

    if (num1 != num2) {
        return true;
    }
    return false;
}

//------------------------------------------------------------------------------
// 機能　　： コンボボックス値と変更前の値の比較                                       
//          　引数１、引数２を比較し、値が異なっていた場合はメッセージを表示                           　       
// 引数  　： 数値１、数値２                                         
// 作成日　： 2014/08/15　arc amii 在庫ステータス変更対応 #3071
// 更新日　：                                                                  
//---------------------------------------------------------------------------
function CompStatus(beforeData, id) {
    if (beforeData != document.getElementById(id).value) {
        // 初期表示時のステータスと異なっていた場合、メッセージを表示
        alert("在庫ステータスが変更されました");
    }
}

//------------------------------------------------------------------------------
// 機能　　： 更新時に更新インジケータの表示・非表示制御                                                                　       
// 引数  　： 制御するID、表示切替数値                                         
// 作成日　： 2014/09/22　arc amii 部品価格一括更新対応
// 更新日　：                                                                  
//---------------------------------------------------------------------------
function DisplayImage(id, flag) {
    var element = document.getElementById(id);

    if (flag == "0") {
        element.style.display = 'block';
        setTimeout(function () { element.innerHTML = '<img src="/Content/Images/indicator.gif" width="30" height="30"/>'; }, 100);
    } else {
        element.style.display = "none";
    } 
}


/*-------------------------------------------------------------------*/
/* 機能　： 請求先設定                                               */
/* 作成日： 2014/10/09　arc yano #3109_請求先設定誤り対応            */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/

function SetCustomerClaimCode(obj, customerClaimCode) {

    var valcustomerClaimCode = customerClaimCode;

    //行編集(挿入、コピー)ボタンが押された行を取得
    var tr = obj.parentNode.parentNode.parentNode;
    // 一つ上の行を取得
    var pretr = tr.previousSibling;

    while (true) {
        //上に行が存在する場合
        if (pretr != null) {
            if (pretr.nodeName == "TR") {   //行の場合
                //上の行が非表示(=削除対象行)ではない場合
                if (pretr.style.display != "none") {
                    //フィールド取得
                    var objselect = pretr.getElementsByTagName('select');
                    var j;
                    for (j = 0; j < objselect.length; j++) {
                        if (objselect[j].name.match(/line\[\d{1,4}\]\.ServiceType/)) {
                            var svtype = objselect[j].value;
                            break;
                        }
                    }
                    //種別が主作業の場合
                    if (svtype == "001") {
                        //フィールド取得
                        var objtext = pretr.getElementsByTagName('input');
                        var k;
                        for (k = 0; k < objtext.length; k++) {
                            if (objtext[k].name.match(/line\[\d{1,4}\]\.CustomerClaimCode/)) {
                                valcustomerClaimCode = objtext[k].value;
                                break;
                            }
                        }
                        break;
                    }
                    else {//種別が主作業以外の場合は、さらに一つ上の行をチェックする。
                        pretr = pretr.previousSibling;
                    }
                }
                else {  //上の行が非表示(=削除対象行)の場合
                    pretr = pretr.previousSibling;  //さらにひとつ上の行をチェックする。
                }
            }
            else {//行以外
                pretr = pretr.previousSibling;  //さらにひとつ上の行をチェックする。
            }
        }
        else {//上に行が存在しない場合
            valcustomerClaimCode = "";
            break;
        }
    }
    return valcustomerClaimCode;
}


/*-------------------------------------------------------------------*/
/* 機能　： 勘定奉行データ出力検索条件項目判定                       */
/* 作成日： 2014/10/10　arc amii サブシステム移行対応                */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function ExportSelected(name2, clearFlag) {
    setDisabledCondition();
    

    // Mod 2014/07/03 arc amii chrome対応 IEとIE以外で、style.Displayに設定する値を変えるよう修正
    if (navigator.appName == "Microsoft Internet Explorer") {
        styleDisplay = "block";
    } else {
        styleDisplay = "";
    }
    var name = document.getElementById('ExportName').value;
    switch (name) {
        case "001": //中古車売上(AA除く)
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('CarSales').style.display = styleDisplay; // リスト
            break;

        case "002": //新車売上
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('CarSales').style.display = styleDisplay; // リスト
            break;

        case "003": //中古車売上(AA)
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('CarSales').style.display = styleDisplay; // リスト
            break;

        case "006": //振込入金
            document.getElementById('JournalDate').style.display = styleDisplay; //伝票日付
            document.getElementById('Office').style.display = styleDisplay;    //事業所
            document.getElementById('Division').style.display = styleDisplay;    //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('ReceiptTransfer').style.display = styleDisplay; //リスト
            break;

        case "007": //小口現金
            document.getElementById('JournalDate').style.display = styleDisplay; //伝票日付
            document.getElementById('Office').style.display = styleDisplay;    //事業所
            document.getElementById('Division').style.display = styleDisplay;    //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('ReceiptTransfer').style.display = styleDisplay; //リスト
            break;
        
        case "009": //ローン入金
            document.getElementById('JournalDate').style.display = styleDisplay; //伝票日付
            document.getElementById('Office').style.display = styleDisplay;    //事業所
            document.getElementById('Division').style.display = styleDisplay;    //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('ReceiptTransfer').style.display = styleDisplay; //リスト
            break;

        default:
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            document.getElementById('CarSales').style.display = styleDisplay; // リスト
            break;
    }

    if (clearFlag == '1') {
        clearCondition();
    }

    function setDisabledCondition() {
        document.getElementById('SalesDate').style.display = "none";
        document.getElementById('JournalDate').style.display = "none";
        document.getElementById('Office').style.display = "none";
        document.getElementById('Division').style.display = "none";
        document.getElementById('PurchaseDate').style.display = "none";
        document.getElementById('Department').style.display = "none";
        document.getElementById('Purchase').style.display = "none";
        document.getElementById('Csv').style.display = "none";
        document.getElementById('Journal').style.display = "none";
        document.getElementById('CarSales').style.display = "none";
        document.getElementById('ReceiptTransfer').style.display = "none";
    }
}

function clearCondition() {
    // Add 2015/05/14 arc nakayama #3136 勘定奉行データ出力画面の日付指定
    var sysDate = new Date();
    var year = sysDate.getFullYear();
    var month = sysDate.getMonth() + 1;
    var yearMonth = year + '/' + month;

    var yearIndex = year - 2001;
    var monthIndex = month - 1;
    

    document.getElementById('SalesJournalYearFrom').selectedIndex = yearIndex;
    document.getElementById('SalesJournalMonthFrom').selectedIndex = monthIndex;
    document.getElementById('SalesJournalYearTo').selectedIndex = yearIndex;
    document.getElementById('SalesJournalMonthTo').selectedIndex = monthIndex;
    document.getElementById('JournalSalesYear').selectedIndex = yearIndex;
    document.getElementById('JournalSalesMonth').selectedIndex = monthIndex;
    document.getElementById('OfficeCode').value = "";
    document.getElementById('OfficeName').innerText = "";

    //document.getElementById('DivisionType').selectedIndex = -1;
    document.getElementById('CarPurchaseDateFrom').value = yearMonth;
    document.getElementById('CarPurchaseDateTo').value = yearMonth;
    document.getElementById('DepartmentCode').value = "";
    document.getElementById('DepartmentName').innerText = "";
    document.getElementById('PurchaseStatus').selectedIndex = 0;

    if (document.getElementById('ExportName').value == "006"
        || document.getElementById('ExportName').value == "007"
        || document.getElementById('ExportName').value == "009") {

        document.getElementById('DivisionType').selectedIndex = 0;
    } else {
        document.getElementById('DivisionType').selectedIndex = -1;
    }


}
//--------------------------------------------------------------------
//Mod 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加
//                                            二度押し防止フラグを初期化
//Add 2014/12/15 arc yano IPO対応(部品検索)　処理中表示対応
//--------------------------------------------------------------------
function dispProgressed(ControllerName, subjectId) {

    var strurl = "/" + ControllerName + "/GetProcessed";
    var count = 0;
    // 1000ミリ秒おきによ呼び出し
    var id = setInterval(function () {
        $.ajax({
            type: "POST",
            url: strurl,
            data: { processType: null },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 1000,
            success: function (data, dataType) {
                DisplayImage(subjectId, '1');
                clearInterval(id);
                inProcess = "0";        //Add 2016/07/05 arc yano  #3598
                return true;
            }     
            ,
            error: function (XMLHttpRequest, textStatus, errorThrown) {  //通信失敗時
                return false;
            }
        });
    }, 1000);

}

//--------------------------------------------------------------------
//Add 2015/04/27 arc yano IPO対応(部品検索)　処理中に経過時間を表示
//--------------------------------------------------------------------
function dispProgresseddispTimer(ControllerName, subjectId) {

    var strurl = "/" + ControllerName + "/GetProcessed";
    var count = 0;
    // 100ミリ秒おきによ呼び出し
    var id = setInterval(function () {
        $.ajax({
            type: "POST",
            url: strurl,
            data: { processType: null },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 100,
            success: function (data, dataType) {
                stopwatchStop();
                clearInterval(id);
                return true;
            }
            ,
            error: function (XMLHttpRequest, textStatus, errorThrown) {  //通信失敗時
                return false;
            }
        });
    }, 1000);

}

//----------------------------------------------------------------------------------------
//機能：メーカーから車種リストを取得し、リストボックスにセットする(顧客DMリスト専用)
//作成日：2015/01/21 arc nakayama
//更新日：
//----------------------------------------------------------------------------------------
function GetCarMasterList() {

    var obj = document.getElementById('MakerCode');
    
    if (obj.selectedIndex < 0) {
        removeNodes(document.getElementById('CarCode'));
        return false;
    }
    var MakerCode = obj.options[obj.selectedIndex].value;
    var PrivateFlag = document.getElementsByName('PrivateFlag');
    var PrivateFlagVale = 0;
    for (var i = 0 ;  i < PrivateFlag.length; i++) {
        if (PrivateFlag[i].checked) {
            PrivateFlagVale = 1;
        }
    }

    $.ajax({
        type: "POST",
        url: "/Maker/GetCarMasterList",
        data: { code: MakerCode, code2: PrivateFlagVale },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
                if (data.Code == null) {
                    removeNodes(document.getElementById('CarCode'));
                    return false;
                }
                removeNodes(document.getElementById('CarCode'));
                createNodes(document.getElementById('CarCode'), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("車種情報の取得に失敗しました。");
            return false;
        }
    });
}

//----------------------------------------------------------------------------------------
//機能：ラジオボタンの切替でメーカー一覧を変える(顧客DMリスト専用)
//作成日：2015/01/26 arc nakayama
//更新日：
//----------------------------------------------------------------------------------------
function GetMakerMasterList(PrivateFlag) {

    $.ajax({
        type: "POST",
        url: "/Maker/GetMakerMasterList",
        data: { PrivateFlag : PrivateFlag },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                removeNodes(document.getElementById('MakerCode'));
                return false;
            }
            removeNodes(document.getElementById('MakerCode'));
            createNodes(document.getElementById('MakerCode'), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("メーカー情報の取得に失敗しました。");
            return false;
        }
    });
}

//----------------------------------------------------------------------------------------
//機能：ラジオボタンの切替で車種一覧を変える(自社取扱いのみ)(顧客DMリスト専用)
//作成日：2015/01/26 arc nakayama
//更新日：
//----------------------------------------------------------------------------------------
function GetPrivateCarList(PrivateFlag) {

    $.ajax({
        type: "POST",
        url: "/Maker/GetPrivateCarList",
        data: { PrivateFlag: PrivateFlag },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                removeNodes(document.getElementById('CarCode'));
                return false;
            }
            removeNodes(document.getElementById('CarCode'));
            createNodes(document.getElementById('CarCode'), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("車種情報の取得に失敗しました。");
            return false;
        }
    });
}


//----------------------------------------------------------------------------------------
//機能：ラジオボタンの切替で車種名一覧を変える(顧客DMリスト専用)
//作成日：2015/02/27 arc nakayama
//更新日：
//----------------------------------------------------------------------------------------
function GetCarMasterListAll() {

    var obj = null;

    $.ajax({
        type: "POST",
        url: "/Maker/GetCarMasterListAll",
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                removeNodes(document.getElementById('CarCode'));
                return false;
            }
            removeNodes(document.getElementById('CarCode'));
            createNodes(document.getElementById('CarCode'), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("車種情報の取得に失敗しました。");
            return false;
        }
    });
}


//----------------------------------------------------------------------------------------
//機能：検索ダイアログで選択(顧客DMリスト専用)
//作成日：2015/01/23 arc nakayama
//更新日：
//----------------------------------------------------------------------------------------
function selectedCriteriaDialogForCustomerDM(code, value1, value2) {

    var args = new Array();     //args[0]…code , args[1]…name1 , args[2]･･･name2
    //var args = new Object();
    args[0] = code;
    args[1] = value1;
    args[2] = value2;

    //window.returnValue = args;
    this.parent.window.returnValue = args;
    this.parent.close();
}


//----------------------------------------------------------------------------------------
//機  能：検索ダイアログの表示(顧客DMリスト専用)
//作成日：2015/01/23 arc nakayama
//更新日：2021/02/15 sola owashi chrome対応 ShowModalDialog廃止対応、関数定義にcallbackパラメータを追加、戻り値廃止
//----------------------------------------------------------------------------------------
//function openSearchDialogForCustomerDM(codeItem, nameItem1, nameItem2, url, width, height, status, scroll) {
function openSearchDialogForCustomerDM(codeItem, nameItem1, nameItem2, url, width, height, status, scroll, callback) {

    // Mod 2021/03/26 sola owashi Chrome対応
    // IEでも動くように修正
    // 修正前
    // // Mod 2021/02/15 sola owashi Chrome対応
    // // この関数の呼び出し時に子画面からの戻り値を返せなくなったため戻り値を廃止（コールバック関数に処理を移行）
    // //var ret = false;

    // 修正後：開始---------------
    var ret = false;
    // 修正後：終了---------------

    var widthS = 0;
    var heightS = 0;

    widthS = window.screen.width;
    heightS = window.screen.height;


    // スクロールを含む幅と高さを取得する（ブラウザのモードによって取得が違う為、分岐させる）
    if (document.compatMode == "CSS1Compat") {

        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.documentElement.scrollWidth > window.screen.width) {
            widthS = document.documentElement.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.documentElement.scrollHeight > window.screen.height) {
            heightS = document.documentElement.scrollHeight;
        }
    } else {
        // スクロールを含む幅がモニタサイズ以上の場合、ロックする幅をスクロールを含む幅に設定する
        if (document.body.scrollWidth > window.screen.width) {
            widthS = document.body.scrollWidth;
        }

        // スクロールを含む高さがモニタサイズ以上の場合、ロックする高さをスクロールを含む高さに設定する
        if (document.body.scrollHeight > window.screen.height) {
            heightS = document.body.scrollHeight;
        }
    }

    // 設定したサイズ分、親画面にマスクをかける
    $('#mask').css({
        width: widthS,
        height: heightS,
        display: 'block'
    });

    if (parent.HeaderFrame != null) {//ヘッダ画面ありの場合
        $('#mask', parent.HeaderFrame.document).css('display', 'block');
    }

    if (parent.MenuFrame != null) {//メニュー画面ありの場合
        $('#mask', parent.MenuFrame.document).css('display', 'block');
    }

    if (!width || !height) {
        size = new WindowSize(url);
        width = size.width;
        height = size.height;
    }


    //urlの調整
    url = url.replace('?', '&');
    //インラインフレームの幅の調整
    var modwidth = width - 16;
    url = "/IFrame/IFrame?url=" + url + "&width=" + modwidth + "&height=" + height;

    // Mod 2021/03/26 sola owashi Chrome対応
    // IEでも動くように修正
    // 修正前
    // // Mod 2021/02/15 sola owashi Chrome対応
    // // 修正前------------
    // /*
    // var ret = openModalDialog2(url, width, height, status, scroll);
    // if (ret != null) {
    //     document.getElementById(codeItem).value = ret[0];
    //     if (nameItem1 != '') {
    //         if ((document.getElementById(nameItem1).tagName == 'INPUT') || (document.getElementById(nameItem1).tagName == 'input')) { //input項目の場合
    //             document.getElementById(nameItem1).value = ret[1];
    //         }
    //         else {  //それ以外
    //             document.getElementById(nameItem1).innerText = decodeHtml(ret[1]);
    //         }

    //     }

    //     if (nameItem2 != '') {
    //         if ((document.getElementById(nameItem2).tagName == 'INPUT') || (document.getElementById(nameItem2).tagName == 'input')) { //input項目の場合
    //             document.getElementById(nameItem2).value = ret[2];
    //         }
    //         else {  //それ以外
    //             document.getElementById(nameItem2).innerText = decodeHtml(ret[2]);
    //         }

    //     }
    //     if (document.getElementById(codeItem).type != "hidden") {
    //         document.getElementById(codeItem).focus();
    //     }
    //     //return true;
    //     ret = true;
    // }

    // $('#mask').css('display', 'none');

    // //ヘッダ画面ありの場合
    // if (parent.HeaderFrame != null) {
    //     $('#mask', parent.HeaderFrame.document).css('display', 'none');
    // }
    // if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //     $("#mask", parent.MenuFrame.document).css('display', 'none');
    // }

    // //return false;

    // return ret;
    // */
    // // 修正後：開始------------
    // var childWindow = openModalDialog2(url, width, height, status, scroll);

    // childWindow.addEventListener('load',function(event){
    //     childWindow.addEventListener('unload', event => {
    //         var ret = childWindow.returnValue;

    //         if (ret != null) {
    //             document.getElementById(codeItem).value = ret[0];
    //             if (nameItem1 != '') {
    //                 if ((document.getElementById(nameItem1).tagName == 'INPUT') || (document.getElementById(nameItem1).tagName == 'input')) { //input項目の場合
    //                     document.getElementById(nameItem1).value = ret[1];
    //                 }
    //                 else {  //それ以外
    //                     document.getElementById(nameItem1).innerText = decodeHtml(ret[1]);
    //                 }
        
    //             }
        
    //             if (nameItem2 != '') {
    //                 if ((document.getElementById(nameItem2).tagName == 'INPUT') || (document.getElementById(nameItem2).tagName == 'input')) { //input項目の場合
    //                     document.getElementById(nameItem2).value = ret[2];
    //                 }
    //                 else {  //それ以外
    //                     document.getElementById(nameItem2).innerText = decodeHtml(ret[2]);
    //                 }
        
    //             }
    //             if (document.getElementById(codeItem).type != "hidden") {
    //                 document.getElementById(codeItem).focus();
    //             }
                
    //             ret = true;
    //         }
        
    //         $('#mask').css('display', 'none');
        
    //         //ヘッダ画面ありの場合
    //         if (parent.HeaderFrame != null) {
    //             $('#mask', parent.HeaderFrame.document).css('display', 'none');
    //         }
    //         if (parent.MenuFrame != null) {//メニュー画面ありの場合
    //             $("#mask", parent.MenuFrame.document).css('display', 'none');
    //         }

    //         if(callback != undefined){
    //             // 引数でコールバック関数が指定されている場合は呼び出す
    //             callback.bind(null, ret)();
    //         }
    //     }, false);
    // }, false);
    // // 修正後：終了------------
    // 修正後：開始---------------
    var userAgent = window.navigator.userAgent.toLowerCase();
    if(userAgent.indexOf('msie') != -1 || userAgent.indexOf('trident') != -1) {
        // IEの場合
        var ret = openModalDialog2(url, width, height, status, scroll);
        if (ret != null) {
            document.getElementById(codeItem).value = ret[0];
            if (nameItem1 != '') {
                if ((document.getElementById(nameItem1).tagName == 'INPUT') || (document.getElementById(nameItem1).tagName == 'input')) { //input項目の場合
                    document.getElementById(nameItem1).value = ret[1];
                }
                else {  //それ以外
                    document.getElementById(nameItem1).innerText = decodeHtml(ret[1]);
                }
    
            }
    
            if (nameItem2 != '') {
                if ((document.getElementById(nameItem2).tagName == 'INPUT') || (document.getElementById(nameItem2).tagName == 'input')) { //input項目の場合
                    document.getElementById(nameItem2).value = ret[2];
                }
                else {  //それ以外
                    document.getElementById(nameItem2).innerText = decodeHtml(ret[2]);
                }
    
            }
            if (document.getElementById(codeItem).type != "hidden") {
                document.getElementById(codeItem).focus();
            }
            //return true;
            ret = true;
        }
    
        $('#mask').css('display', 'none');
    
        //ヘッダ画面ありの場合
        if (parent.HeaderFrame != null) {
            $('#mask', parent.HeaderFrame.document).css('display', 'none');
        }
        if (parent.MenuFrame != null) {//メニュー画面ありの場合
            $("#mask", parent.MenuFrame.document).css('display', 'none');
        }
    
        //return false;
    
        return ret;
    } else  {
        // IE以外の場合
        var childWindow = openModalDialog2(url, width, height, status, scroll);

        childWindow.addEventListener('load', function(event){
            //childWindow.addEventListener('unload', event => {
                childWindow.addEventListener('unload', function(event){
                var ret = childWindow.returnValue;
    
                if (ret != null) {
                    document.getElementById(codeItem).value = ret[0];
                    if (nameItem1 != '') {
                        if ((document.getElementById(nameItem1).tagName == 'INPUT') || (document.getElementById(nameItem1).tagName == 'input')) { //input項目の場合
                            document.getElementById(nameItem1).value = ret[1];
                        }
                        else {  //それ以外
                            document.getElementById(nameItem1).innerText = decodeHtml(ret[1]);
                        }
            
                    }
            
                    if (nameItem2 != '') {
                        if ((document.getElementById(nameItem2).tagName == 'INPUT') || (document.getElementById(nameItem2).tagName == 'input')) { //input項目の場合
                            document.getElementById(nameItem2).value = ret[2];
                        }
                        else {  //それ以外
                            document.getElementById(nameItem2).innerText = decodeHtml(ret[2]);
                        }
            
                    }
                    if (document.getElementById(codeItem).type != "hidden") {
                        document.getElementById(codeItem).focus();
                    }
                    
                    ret = true;
                }
            
                $('#mask').css('display', 'none');
            
                //ヘッダ画面ありの場合
                if (parent.HeaderFrame != null) {
                    $('#mask', parent.HeaderFrame.document).css('display', 'none');
                }
                if (parent.MenuFrame != null) {//メニュー画面ありの場合
                    $("#mask", parent.MenuFrame.document).css('display', 'none');
                }
    
                if(callback != undefined){
                    // 引数でコールバック関数が指定されている場合は呼び出す
                    callback.bind(null, ret)();
                }
            }, false);
        }, false);
    }
    // 修正後：終了---------------
}
//--------------------------------------------------------------------
//Add 2015/01/28 arc ishii 入金検索リスト ドロップダウンリスト制御
//--------------------------------------------------------------------
function CheckDayEnabled() {

    var checkBox = document.getElementById('DayEnabled');
    var TargetDateYFrom = document.getElementById('TargetDateYFrom');
    var TargetDateYTo = document.getElementById('TargetDateYTo');
    var TargetDateMFrom = document.getElementById('TargetDateMFrom');
    var TargetDateMTo = document.getElementById('TargetDateMTo');
    var TargetDateDFrom = document.getElementById('TargetDateDFrom');
    var TargetDateDTo = document.getElementById('TargetDateDTo');
    

    if (checkBox.checked) {
        var yearFrom = TargetDateYFrom.options[TargetDateYFrom.selectedIndex].text;// 対象年(from)
        var monthFrom = TargetDateMFrom.options[TargetDateMFrom.selectedIndex].text; // 対象月(from)
        var yearTo = TargetDateYTo.options[TargetDateYTo.selectedIndex].text;　//対象年(to)
        var monthTo = TargetDateMTo.options[TargetDateMTo.selectedIndex].text;　//　対象月（to）

        var DateFrom = new Date(yearFrom , monthFrom, 1);
        var DateTo = new Date(yearTo, monthTo, 0);
        //monthFromからmonthToまでの1日と末日取得
        TargetDateDFrom.value = DateFrom.getDate();
        TargetDateDTo.value = DateTo.getDate();

        TargetDateDFrom.disabled = true;
        TargetDateDTo.disabled = true;
        document.getElementById('DayEnabled').checked = true;
        return;
    } else {
        TargetDateDFrom.disabled = false;
        TargetDateDTo.disabled = false;
        document.getElementById('DayEnabled').checked = false;
    }
}

//--------------------------------------------------------------------
//Add 2015/02/19 arc iijima 部品在庫検索 ドロップダウンリスト制御
//--------------------------------------------------------------------
function CheckTargetRange(Enabled) {

    var TargetRange = document.getElementById('TargetRange');
    var TargetDateY = document.getElementById('TargetDateY');
    var TargetDateM = document.getElementById('TargetDateM'); 
    var NowTargetDateY = document.getElementById('NowTargetDateY');
    var NowTargetDateM = document.getElementById('NowTargetDateM');

    if (Enabled == '0') {
 
        TargetDateY.value = "";
        TargetDateM.value = "";

        TargetDateY.disabled = true;
        TargetDateM.disabled = true;
        document.getElementById('TargetRange').value = "0";
        document.getElementById("TargetRange").checked = true;
    } else {

        TargetDateY.value = NowTargetDateY.value;
        TargetDateM.value = NowTargetDateM.value;
        TargetDateY.disabled = false;
        TargetDateM.disabled = false;
        document.getElementById('TargetRange').value = "1";
        //document.getElementById('TargetRange')[1].checked = true;

    }
    return;
}

//--------------------------------------------------------------------------------------
// 機能：車両マスタの「車検案内」デフォルト設定
// 作成：2015/02/09 arc ishii #3147 顧客コードを取得し、車検案内の可否を指定する。
// 更新：2020/08/29 yano #4061【車両マスタ】所有者変更時の車検案内情報の更新
//--------------------------------------------------------------------------------------
function chkInspect(codeField, flagField) {
    var selectCode = document.getElementById(codeField);
    var selectList = document.getElementById(flagField);
    //車検案内不可のコード
    var chkCode = new Array("A000199999", "A004301883");

    //Mod 2020/08/29 yano #4061
    selectList.value = "001";

    if (chkCode.indexOf(selectCode.value) >= 0) {
        selectList.value = "002";
    }

    //for (var i = 0; i < chkCode.length ; i++) {
    //    if (selectCode.value == chkCode[i]) {
    //        selectList.value = "002";
    //    }
    //}
}

//-------------------------------------------------------------------------------------
// 機能：車両マスタの「車検案内」デフォルト設定
// 作成：2015/02/13 arc yano 特定の顧客が選択された場合に、車検案内発送備考欄にデフォルト設定を行う
// 更新：2020/08/29 yano #4061【車両マスタ】所有者変更時の車検案内情報の更新
//-------------------------------------------------------------------------------------
function setInspectGuidMemo(chkField, setField) {
    var chkfld = document.getElementById(chkField);
    var setfld = document.getElementById(setField);

    if (chkfld.value == 'A000199999') {
        setfld.value = '売却・代替';
    }
    else if (chkfld.value == 'A004301883') {
        setfld.value = '抹消';
    }
    else {
        //文字列クリア
        setfld.value = '';  //Add 2020/08/29 yano #4061
    }
}
/*-------------------------------------------------------------------*/
/* 機能　： 車検案内設定                                             */
/*　　　　　自分の選択値により、対象を自動的に選択状態にする。       */
/* 作成日： 2015/02/16　arc yano 車両ステータス追加対応　            */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function SetInspectGuidFlag(obj, sbj) {

    //ドロップダウンの選択値を取得する。
    var val = obj.options[obj.selectedIndex].value;

    switch (val) {

        case "001": //デモカー
        case "002": //レンタカー
        case "003": //業務車両
        case "004": //広報車
        case "005": //代車

            var sbj_options = sbj.getElementsByTagName('option');
            for (i = 0; i < sbj_options.length; i++) {
                if (sbj_options[i].value == "002") {　//車検案内「否」
                    sbj_options[i].selected = true;
                }
            }
            break;

        default:
            var sbj_options = sbj.getElementsByTagName('option');
            for (i = 0; i < sbj_options.length; i++) {
                if (sbj_options[i].value == "001") {　//車検案内「要」
                    sbj_options[i].selected = true;
                }
            }
            break;
    }
}

/*-------------------------------------------------------------------*/
/* 機能　： オブジェクトの活性／非活性                               */
/*　　　　　自分の選択値により、対象を自動的に選択状態にする。       */
/* 作成日： 2015/03/19　arc yano 現金出納帳出力         　           */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function SetDisabled(targetName, disabled, value) {

    var target = document.getElementsByName(targetName);
    
    for (var i = 0; i < target.length; i++) {
        //活性／非活性
        target[i].disabled = disabled;

        //value
        if (value != undefined) {
            target[i].value = value;
        }
    }




}

//----------------------------------------------------------------------------------------
// Add 2015/04/10 arc nakayama 顧客DM指摘事項修正Part2　自社取扱いをチェックボックスにする
// チェックボックスで自社取扱いとそうでないものを切り替える
//----------------------------------------------------------------------------------------
function CheckPrivateFlag() {

    var checkBox = document.getElementById('PrivateFlag');

    if (checkBox.checked) {
        
        resetCommonCriteria(new Array('MakerCode', 'CarCode'));
        GetMakerMasterList('1');
        GetPrivateCarList('1');

//        document.getElementById('PrivateFlag').checked = true;
//       document.getElementById('PrivateFlag').value = "1"
        return;
    } else {
        resetCommonCriteria(new Array('MakerCode', 'CarCode'));
        GetMakerMasterList();
        GetCarMasterListAll();

//        document.getElementById('PrivateFlag').checked = false;
//        document.getElementById('PrivateFlag').value = "0"
    }
}

//----------------------------------------------------------------------------------------
// Add 2015/04/27 arc yano IPO対応(部品棚卸) 経過時間の表示
//----------------------------------------------------------------------------------------
function stopwatchStart() {
    if ($startTime === undefined) {
        var $startDate = new Date();
        $startTime = $startDate.getTime();
    }
    var $nowDate = new Date();
    $stopwatchTime = $nowDate.getTime() - $startTime + $stopwatchTimeAdd;
    $stopwatchMillisecond = $stopwatchTime % 1000;
    $stopwatchSecond = Math.floor($stopwatchTime / 1000) % 60;
    $stopwatchMinute = Math.floor($stopwatchTime / 1000 / 60) % 60;
    $stopwatchHour = Math.floor(Math.floor($stopwatchTime / 1000 / 60) / 60);
    if ($stopwatchMillisecond < 10) {
        $stopwatchMillisecond = '0' + $stopwatchMillisecond;
    }
    if ($stopwatchMillisecond < 100) {
        $stopwatchMillisecond = '0' + $stopwatchMillisecond;
    }
    if ($stopwatchSecond < 10) {
        $stopwatchSecond = '0' + $stopwatchSecond;
    }
    if ($stopwatchMinute < 10) {
        $stopwatchMinute = '0' + $stopwatchMinute;
    }
    if ($stopwatchHour < 10) {
        $stopwatchHour = '0' + $stopwatchHour;
    }
    jQuery('#stopwatchHour').text($stopwatchHour);
    jQuery('#stopwatchMinute').text($stopwatchMinute);
    jQuery('#stopwatchSecond').text($stopwatchSecond);
    jQuery('#stopwatchMillisecond').text($stopwatchMillisecond);
    $stopwatch = setTimeout("stopwatchStart()", 1);
}
function stopwatchStop() {
    clearTimeout($stopwatch);
    $startTime = undefined;
    $stopwatchTimeAdd = $stopwatchTime;
}
function stopwatchClear() {
    $startTime = undefined;
    $stopwatchTimeAdd = 0;
    jQuery('#stopwatchHour').text('00');
    jQuery('#stopwatchMinute').text('00');
    jQuery('#stopwatchSecond').text('00');
    jQuery('#stopwatchMillisecond').text('000');
}

/*----------------------------------------------------------------------------------------------------*/
/* 機能　： オブジェクトの非活性                                                                      */
/*          部品棚卸画面のボタンを全て非活性や、空欄にする                                            */
/* 作成日： 2015/05/14　arc yano IPO対応(部品棚卸)                                                    */
/* 更新日： 2017/07/18 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 新規作成ボタンの追加             */
/*          2015/06/17  arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥                               */
/*          棚卸開始日時の内容をクリアする処理を追加                                                  */
/*       ： 2015/05/27  arc yano IPO対応(部品棚卸) 障害対応、仕様変更②                               */
/*          テキストボックス、チェックボックス等も無効にする                                          */
/*----------------------------------------------------------------------------------------------------*/
function SetDisable() {
    document.getElementById('SearchButton').disabled = true;        //検索ボタン
    document.getElementById('ClearButton').disabled = true;         //クリアボタン
    document.getElementById('ExcelOutputButton').disabled = true;   //Excel出力ボタン
    document.getElementById('ExcelInputButton').disabled = true;    //Excel取込ボタン
    document.getElementById('InventorySaveButton').disabled = true; //棚卸結果一時保存ボタン
    //Mod 2015/05/21 隠し項目の値で確定ボタンクリック表示／非表示の判定を行う
    if (document.getElementById('InventoryEndButtonEnable').value == 'true') {
        document.getElementById('InventoryEndButton').disabled = true;   //棚卸確定ボタン		//Mod 2015/05/26 arc yano IPO対応(部品棚卸) ボタン名変更
    }
    document.getElementById('InventoryStartButton').disabled = true;   //棚卸開始ボタン

    

    //Mod 2015/05/27 arc yano
    document.getElementById('LocationCode').readOnly = true;
    document.getElementById('LocationCode').style.backgroundColor = "#eeeeee";
    document.getElementById('LocationCode').value = "";
    document.getElementById('LocationName').readOnly = true;
    document.getElementById('LocationName').style.backgroundColor = "#eeeeee";
    document.getElementById('LocationName').value = "";
    document.getElementById('PartsNumber').readOnly = true;
    document.getElementById('PartsNumber').style.backgroundColor = "#eeeeee";
    document.getElementById('PartsNumber').value = "";
    document.getElementById('PartsNameJp').readOnly = true;
    document.getElementById('PartsNameJp').style.backgroundColor = "#eeeeee";
    document.getElementById('PartsNameJp').value = "";
    document.getElementById('DiffQuantity').disabled = true;
    document.getElementById('DiffQuantity').checked = false;
    document.getElementById('SearchButton').disabled = true;
    document.getElementById('ClearButton').disabled = true;
    document.getElementById('LocationSearch').onclick = function () { return };
    document.getElementById('PartsSearch').onclick = function () { return };

    //Add 2015/06/17
    //棚卸開始日時
    document.getElementById('PartsInventoryStartDate').value = '';
    document.getElementById('sPartsInventoryStartDate').innerHTML = '';

   

    //Add 2017/07/18  arc yano #3781 新規作成ボタンのクリア
    if (document.getElementById('DataEditButtonVisible').value == 'true') {
        document.getElementById('InventoryDataEdit').disabled = true;  //新規作成ボタン
    }

    //テーブル、ページのクリア
    var tablehtml = '<table class="list" style ="width:100%">';
    tablehtml += '<tr>';
    tablehtml += '<th style ="white-space:nowrap">ロケーションコード</th>';
    tablehtml += '<th style ="white-space:nowrap">ロケーション名</th>';
    tablehtml += '<th style ="white-space:nowrap">部品番号</th>';
    tablehtml += '<th style ="white-space:nowrap">部品名</th>';
    tablehtml += '<th style ="white-space:nowrap">理論数</th>';
    tablehtml += '<th style ="white-space:nowrap">単価</th>';
    tablehtml += '<th style ="white-space:nowrap">金額</th>';
    tablehtml += '<th style="white-space:nowrap">実棚数</th>';
    tablehtml += '<th style="white-space:nowrap">コメント</th>';
    tablehtml += '</tr>';
    tablehtml += '</table>';

    var pagehtml = '<span style="font-size:12pt"><b>0 - 0 / 0 </b>&nbsp;</span>';

    //ページのクリア
    document.getElementById('pageBlock').innerHTML = pagehtml;
    //一覧表のクリア
    document.getElementById('tableBlock').innerHTML = tablehtml;






}

/*---------------------------------------------------------------------------------------------------------*/
/* 機能　： 棚卸時の各オブジェクトの活性／非活性                                                           */
/*          部門の棚卸ステータスで活性／非活性を切り替える                                                 */
/* 作成日： 2015/04/28　arc nakayama IPO対応(部品棚卸)                                                     */
/* 更新日： 2017/07/18  arc yano #3781 部品在庫棚卸（棚卸在庫の修正）新規作成ボタンの活性処理の追加        */
/*          2016/08/13  arc yano #3596 【大項目】部門棚統合対応 引数変更(DepartmentCode→WarehouseCode)    */
/*          2016/07/14  arc yano #3618 部品在庫棚卸　棚卸開始ボタン押下判定の不具合                        */
/*          2015/05/21　arc yano IPO対応(部品棚卸)                                                         */
/*          権限による確定ボタンクリック可／不可判定追加                                                   */
/*---------------------------------------------------------------------------------------------------------*/
function CheckInventoryStatus(WarehouseCode, InventoryMonth, WorkingDate)
{
    //対象年月
    var InventoryMonth = InventoryMonth + "/01"
    //前対象年月
    var vYear = InventoryMonth.substr(0, 4) - 0;
    var vMonth = InventoryMonth.substr(5, 2) - 1;
    var vDay = 01;                      // 日は１日とする。
    var PrevInventoryMonth = vYear + "/" + vMonth + "/" + vDay;
    //本日日付
    var ToDatetime = new Date();
    var tYear = ToDatetime.getYear();
    var tMonth = ToDatetime.getMonth() + 1;             //Mod 2015/06/01 arc yano getMonth()はその月-1となるため、＋１する
    var tDay = 01;                      // 日は１日とする。
    var ToDate = tYear + "/" + tMonth + "/" + tDay;

    //Mod 2015/06/01 arc yano 棚卸作業日を時刻に変換
    var workingDateD = null;

    if (WorkingDate != null && WorkingDate != "") {
        workingDateD = new Date(WorkingDate);
        workingDateD.setDate(workingDateD.getDate() + 1);
    }
    
    //Add 2017/07/18  arc yano #3781
    //ページ情報はリセット
    //テーブル、ページのクリア
    var tablehtml = '<table class="list" style ="width:100%">';
    tablehtml += '<tr>';
    tablehtml += '<th style ="white-space:nowrap">ロケーションコード</th>';
    tablehtml += '<th style ="white-space:nowrap">ロケーション名</th>';
    tablehtml += '<th style ="white-space:nowrap">部品番号</th>';
    tablehtml += '<th style ="white-space:nowrap">部品名</th>';
    tablehtml += '<th style ="white-space:nowrap">理論数</th>';
    tablehtml += '<th style ="white-space:nowrap">単価</th>';
    tablehtml += '<th style ="white-space:nowrap">金額</th>';
    tablehtml += '<th style="white-space:nowrap">実棚数</th>';
    tablehtml += '<th style="white-space:nowrap">コメント</th>';
    tablehtml += '</tr>';
    tablehtml += '</table>';

    var pagehtml = '<span style="font-size:12pt"><b>0 - 0 / 0 </b>&nbsp;</span>';

    //ページのクリア
    document.getElementById('pageBlock').innerHTML = pagehtml;
    //一覧表のクリア
    document.getElementById('tableBlock').innerHTML = tablehtml;


    $.ajax({
        type: "POST",
        url: "/Monthly/PartsInventorySchedule",
        data: { WarehouseCode: WarehouseCode, InventoryMonth: InventoryMonth },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == "001") {
                //実施中
                document.getElementById('LocationCode').readOnly = false;
                document.getElementById('LocationCode').style.backgroundColor = "#ffffff";
                document.getElementById('LocationName').readOnly = false;
                document.getElementById('LocationName').style.backgroundColor = "#ffffff";
                document.getElementById('PartsNumber').readOnly = false;
                document.getElementById('PartsNumber').style.backgroundColor = "#ffffff";
                document.getElementById('PartsNameJp').readOnly = false;
                document.getElementById('PartsNameJp').style.backgroundColor = "#ffffff";
                document.getElementById('DiffQuantity').disabled = false;        //棚差
                document.getElementById('SearchButton').disabled = false;        //検索ボタン
                document.getElementById('ClearButton').disabled = false;         //クリアボタン
                document.getElementById('ExcelOutputButton').disabled = false;   //Excel出力ボタン
                document.getElementById('ExcelInputButton').disabled = false;    //Excel取込ボタン
                document.getElementById('InventorySaveButton').disabled = false; //棚卸結果一時保存ボタン

                //2017/07/18  arc yano #3781
                if (document.getElementById('DataEditButtonVisible').value == 'true') {
                    document.getElementById('InventoryDataEdit').disabled = true;  //新規作成ボタン
                }
                

                //Mod 2015/05/21 隠し項目の値で確定ボタンクリック表示／非表示の判定を行う
                if (document.getElementById('InventoryEndButtonEnable').value == 'true') {
                    document.getElementById('InventoryEndButton').disabled = false;   //棚卸確定ボタン
                }
                else {
                    //document.getElementById('InventoryEndButton').disabled = true;   //棚卸確定ボタン
                }
                document.getElementById('InventoryStartButton').disabled = true;   //棚卸開始ボタン
                //document.getElementById('LocationSearch').onclick = function () { openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog?BusinessType=002,009&DepartmentCode=' + document.getElementById('WarehouseCode').value); };  //Mod 2016/08/13  arc yano #3596
                document.getElementById('LocationSearch').onclick = function () { openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog?BusinessType=002,009&WarehouseCode=' + document.getElementById('WarehouseCode').innerText); };
                document.getElementById('PartsSearch').onclick = function () { openSearchDialog('PartsNumber', 'PartsNameJp', '/Parts/CriteriaDialog'); };


            } else if (data.Code == "002") {
                //確定
                document.getElementById('LocationCode').readOnly = false;
                document.getElementById('LocationCode').style.backgroundColor = "#ffffff";
                document.getElementById('LocationName').readOnly = false;
                document.getElementById('LocationName').style.backgroundColor = "#ffffff";
                document.getElementById('PartsNumber').readOnly = false;
                document.getElementById('PartsNumber').style.backgroundColor = "#ffffff";
                document.getElementById('PartsNameJp').readOnly = false;
                document.getElementById('PartsNameJp').style.backgroundColor = "#ffffff";
                document.getElementById('DiffQuantity').disabled = false;       //棚差
                document.getElementById('SearchButton').disabled = false;       //検索ボタン
                document.getElementById('ClearButton').disabled = false;        //クリアボタン
                document.getElementById('ExcelOutputButton').disabled = false;   //Excel出力ボタン
                document.getElementById('ExcelInputButton').disabled = true;    //Excel取込ボタン
                document.getElementById('InventorySaveButton').disabled = true; //棚卸結果一時保存ボタン
                //Mod 2015/05/21 隠し項目の値で確定ボタンクリック表示／非表示の判定を行う
                if (document.getElementById('InventoryEndButtonEnable').value == 'true') {
                    document.getElementById('InventoryEndButton').disabled = true;   //棚卸確定ボタン	//Mod 2015/05/26 arc yano IPO対応(部品棚卸) ボタン名変更
                }

                document.getElementById('InventoryStartButton').disabled = true;   //棚卸開始ボタン
               // document.getElementById('LocationSearch').onclick = function () { openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog?BusinessType=002,009&DepartmentCode=' + document.getElementById('DepartmentCode').value); };//Mod 2016/08/13  arc yano #3596
                document.getElementById('LocationSearch').onclick = function () { openSearchDialog('LocationCode', 'LocationName', '/Location/CriteriaDialog?WarehouseCode=' + document.getElementById('WarehouseCode').innerText); };
                document.getElementById('PartsSearch').onclick = function () { openSearchDialog('PartsNumber', 'PartsNameJp', '/Parts/CriteriaDialog'); };

                //2017/07/18  arc yano #3781
                if (document.getElementById('DataEditButtonVisible').value == 'true') {
                    if (document.getElementById('InventoryStatusPartsBalance').value != '002') {
                        document.getElementById('InventoryDataEdit').disabled = false;  //新規作成ボタン
                    }
                    else {
                        document.getElementById('InventoryDataEdit').disabled = true;  //新規作成ボタン
                    }
                }

            } else {
                //未実施(レコードなし)
                document.getElementById('LocationCode').readOnly = true;
                document.getElementById('LocationCode').style.backgroundColor = "#eeeeee";
                document.getElementById('LocationCode').value = "";
                document.getElementById('LocationName').readOnly = true;
                document.getElementById('LocationName').style.backgroundColor = "#eeeeee";
                document.getElementById('LocationName').value = "";
                document.getElementById('PartsNumber').readOnly = true;
                document.getElementById('PartsNumber').style.backgroundColor = "#eeeeee";
                document.getElementById('PartsNumber').value = "";
                document.getElementById('PartsNameJp').readOnly = true;
                document.getElementById('PartsNameJp').style.backgroundColor = "#eeeeee";
                document.getElementById('PartsNameJp').value = "";
                document.getElementById('DiffQuantity').disabled = true;
                document.getElementById('DiffQuantity').checked = false;
                document.getElementById('SearchButton').disabled = true;
                document.getElementById('ClearButton').disabled = true;
                document.getElementById('ExcelOutputButton').disabled = true;   //Excel出力ボタン
                document.getElementById('ExcelInputButton').disabled = true;    //Excel取込ボタン
                document.getElementById('InventorySaveButton').disabled = true; //棚卸結果一時保存ボタン
                //Mod 2015/05/21 隠し項目の値で確定ボタンクリック表示／非表示の判定を行う
                if (document.getElementById('InventoryEndButtonEnable').value == 'true') {
                    document.getElementById('InventoryEndButton').disabled = true;   //棚卸確定ボタン		//Mod 2015/05/26 arc yano IPO対応(部品棚卸) ボタン名変更
                }
                document.getElementById('LocationSearch').onclick = function () { return };
                document.getElementById('PartsSearch').onclick = function () { return };

                //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 作業日が登録されていない場合は、棚卸開始ボタンを非活性にする
                //if (WorkingDate != null && WorkingDate != "" && ToDatetime >= workingDateD && DepartmentCode != "" && DepartmentCode != null) { //Mod 2016/08/13  arc yano #3596
                if (WorkingDate != null && WorkingDate != "" && ToDatetime >= workingDateD && WarehouseCode != "" && WarehouseCode != null) {
                    document.getElementById('InventoryStartButton').disabled = false;   //棚卸開始ボタン
                } else {
                    document.getElementById('InventoryStartButton').disabled = true;   //棚卸開始ボタン
                }

                //2017/07/18  arc yano #3781
                if (document.getElementById('DataEditButtonVisible').value == 'true') {
                    document.getElementById('InventoryDataEdit').disabled = true;  //新規作成ボタン
                }
            }
            return true;
        }
        ,error: function () {  //通信失敗時
            alert("棚卸情報の取得に失敗しました。");
            return false;
        }
    });
}

/*-------------------------------------------------------------------*/
/* 機能　： 実棚数入力値チェック                                     */
/*          実棚数の入力値が以下になっているかをチェックする　       */
/*          ①フォーマットチェック(マイナス、空欄もエラー)           */
/* 作成日： 2015/04/28　arc nakayama IPO対応(部品棚卸)  　           */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function ChkPhysicalQuantity(id, preid) {

    //var PhysicalQuantity = document.getElementById(colName).value;
    /*
    if (PhysicalQuantity < 0) {
        alert("実棚数にマイナスの値は入力できません")
        document.getElementById(colName).value = '';
        return false
    }
    */

    //実棚数(編集後)
    var target = document.getElementById(id);

    //実棚数(編集前)
    var pretarget = document.getElementById(preid);

    if (!target.value.match(/^\d{1,7}(\.\d{1,1})?$/)) {
        alert("実棚数には正の数(整数7桁以内、小数1桁以内)を入力してください")
        target.value = pretarget.value;
        target.blur();
        target.focus();
        return false
    }

    return true
    
}
/*-------------------------------------------------------------------*/
/* 機能　： Validationエラーメッセージの非表示                       */
/* 作成日： 2015/04/29　arc yano IPO対応(部品棚卸)  　               */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function ClearValidateMessage(id) {

    var target = document.getElementById(id);

    target.style.display = 'none';
    
    return true;
}

/*-------------------------------------------------------------------*/
/* 機能　： 編集中フラグ更新                                         */
/* 作成日： 2015/04/29　arc yano IPO対応(部品棚卸)  　               */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function UpdateEditFlag(id, update) {

    var target = document.getElementById(id);

    target.value = update;

    return true;
}

/*------------------------------------------------------------------------------*/
/* 機能　： サービス売上検索条件項目判定                       					*/
/* 作成日： 2015/05/15　arc nakayama  #3165勘定奉行データ出力画面について修正 　*/
/* 更新日：                              			                            */
/*------------------------------------------------------------------------------*/
function ExportSelected2(name2, clearFlag) {
    setDisabledCondition2();


    // Mod 2014/07/03 arc amii chrome対応 IEとIE以外で、style.Displayに設定する値を変えるよう修正
    if (navigator.appName == "Microsoft Internet Explorer") {
        styleDisplay = "block";
    } else {
        styleDisplay = "";
    }
    var name = document.getElementById('ExportName').value;
    switch (name) {

        case "001": //サービス売上
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Csv').style.display = styleDisplay;  //ボタン
            document.getElementById('ServiceSales').style.display = styleDisplay; // リスト
            break;

        case "002": //サービス売上(社内営業)
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Csv').style.display = styleDisplay;  //ボタン
            document.getElementById('ServiceSalesOffice').style.display = styleDisplay;　// リスト
            break;

        default:
            document.getElementById('SalesDate').style.display = styleDisplay; //納車日
            document.getElementById('Division').style.display = styleDisplay;  //拠点
            document.getElementById('Journal').style.display = styleDisplay;  //ボタン
            break;
    }

    if (clearFlag == '1') {
        clearCondition2();
    }

    function setDisabledCondition2() {
        document.getElementById('SalesDate').style.display = "none";
        document.getElementById('Division').style.display = "none";
        document.getElementById('Csv').style.display = "none";
        document.getElementById('Journal').style.display = "none";
        document.getElementById('ServiceSales').style.display = "none";
        document.getElementById('ServiceSalesOffice').style.display = "none";
    }
}

function clearCondition2() {
    var sysDate = new Date();
    var year = sysDate.getFullYear();
    var month = sysDate.getMonth() + 1;
    var yearMonth = year + '/' + month;
    var yearIndex = year - 2001;
    var monthIndex = month - 1;

    document.getElementById('SalesJournalYearFrom').selectedIndex = yearIndex;
    document.getElementById('SalesJournalMonthFrom').selectedIndex = monthIndex;
    document.getElementById('SalesJournalYearTo').selectedIndex = yearIndex;
    document.getElementById('SalesJournalMonthTo').selectedIndex = monthIndex;
    //document.getElementById('JournalSalesDate').value = yearMonth;
    //document.getElementById('OfficeCode').value = "";
    //document.getElementById('OfficeName').innerText = "";

    document.getElementById('DivisionType').selectedIndex = -1;
    //document.getElementById('CarPurchaseDateFrom').value = yearMonth;
    //document.getElementById('CarPurchaseDateTo').value = yearMonth;
    //document.getElementById('DepartmentCode').value = "";
    //document.getElementById('DepartmentName').innerText = "";
    //document.getElementById('PurchaseStatus').selectedIndex = 0;

    document.getElementById('DivisionType').selectedIndex = -1;


}
//------------------------------------------------------------------------------------
//コードからマスタを検索し、名称を取得する
// Add 2015/05/18 arc nakayama ルックアップ見直し対応　delflagをチェックしないメソッド追加
//  項目の種類により、値をセットする方法を変える。
//-----------------------------------------------------------------------------------
function GetNameFromCodeDelflagNoCheck(
    CodeField,
    NameField,
    ControllerName
    ) {

    if (document.getElementById(CodeField).value != null && document.getElementById(CodeField).value == '') {
        if (NameField != '') {
            var element = document.getElementById(NameField);
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }
        }
        return false;
    } else {
        //if ($('#' + CodeField).val() != null && $('#'+CodeField).val() == '') {
        //    $('#' + NameField).html('');
        //    return;
        //} else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMaster",
            data: { code: document.getElementById(CodeField).value , includeDeleted: true},
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    alert("マスタに存在しません");
                    if (NameField != '') {
                        var element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }
                    document.getElementById(CodeField).value = '';
                    document.getElementById(CodeField).focus();

                    //$('#' + NameField).html('');
                    //$('#' + CodeField).val('');
                    //$('#' + CodeField).focus();
                    return false;
                } else {
                    if (NameField != '') {
                        element = document.getElementById(NameField);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }
                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
}

/*-------------------------------------------------------------------*/
/* 機能　： 対象年月が当月より未来日かどうかをチェックする           */
/* 作成日： 2015/08/10　arc nakayama  (#3233_売掛金帳票対応)  　     */
/* 更新日：                                                          */
/*                                                                   */
/*-------------------------------------------------------------------*/
function CheckDateForDropDown()
{
    var select = document.getElementById('TargetDateY');
    var options = select.options;
    var TargetDateY = options.item(select.selectedIndex).innerText;

    var select2 = document.getElementById('TargetDateM');
    var options2 = select2.options;
    var TargetDateM = options2.item(select2.selectedIndex).innerText;

    //対象日付（日は1日固定）
    var TargetDate = new Date(TargetDateY,TargetDateM, 1 )

    //当日日付
    var now = new Date();
    var ToDateTime = new Date(now.getFullYear(), now.getMonth() + 1, 1);

    if (TargetDate > ToDateTime) {
        alert("当月より未来の年月は設定できません");
        resetCommonCriteria(new Array('TargetDateY', 'TargetDateM'));
        return true;
    }
    return true;

}
/*-------------------------------------------------------------------------------------------*/
/* 機能　： 数値以外の文字列を含んでいないかチェックする                                     */
/* 作成日： 2015/09/01　arc nakayama  (#3154_車両伝票入力画面で支払情報の金額項目入力エラー) */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                   */
/*          取得したデータをカンマ付に変換する                                               */
/*          2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する                        */
/*          2015/09/24  arc yano       問合せ対応 マイナスの金額の場合はチェックしない       */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function CheckAmount(id) {


    //Mod 2017/04/23 arc yano #3755
    //カンマ除去
    //ExceptComma(document.getElementById(id));       //Add 2017/02/14  arc yano #3641
    //var val = document.getElementById(id).value;

    var val = ConversionExceptComma(document.getElementById(id).value);
    val = "" + val;

    //入力した値が数値に変換できるか？
    if (!isFinite(val)) {
        alert("金額には整数を入れてください。");
        document.getElementById(id).innerText = ''
        document.getElementById(id).focus();
    }
    else {
        //入力した値が小数点を含んでいるか？
        var result = val.match(/\./);

        if (result) {
            alert("金額には整数を入れてください。");
            document.getElementById(id).innerText = ''
            document.getElementById(id).focus();
        }
    }

    //カンマ挿入
    //InsertComma(document.getElementById(id));       //Add 2017/02/14  arc yano #3641
}
/*-------------------------------------------------------------------------------------------*/
/* 機能　： サービス集計表をサービス売上の[詳細]から開く                                     */
/* 作成日： 2015/09/15　arc nakayama  #3165_サービス集計表対応                               */
/* 更新日：                                                                                  */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function OpenServiceSalesReport(SalesJournalYearFrom, SalesJournalYearTo, SalesJournalMonthFrom, SalesJournalMonthTo, WorkType, DepartmentCode) {

    //納車日From（日は1日固定）
    var select = document.getElementById(SalesJournalYearFrom);
    var options = select.options;
    var TargetYearFrom = options.item(select.selectedIndex).innerText;

    var select2 = document.getElementById(SalesJournalMonthFrom);
    var options2 = select2.options;
    var TargetMonthFrom = options2.item(select2.selectedIndex).innerText;


    var SalesDateFrom = new Date(TargetYearFrom, TargetMonthFrom, 1)
    var formatSalesDateFrom = SalesDateFrom.getFullYear() + '/' + (SalesDateFrom.getMonth()) + '/' + SalesDateFrom.getDate();

    //納車日To（末日）
    var select3 = document.getElementById(SalesJournalYearTo);
    var options3 = select.options;
    var TargetYearTo = options3.item(select3.selectedIndex).innerText;

    var select4 = document.getElementById(SalesJournalMonthTo);
    var options4 = select2.options;
    var TargetMonthTo = options4.item(select4.selectedIndex).innerText;

    var SalesDateTo = new Date(TargetYearTo, TargetMonthTo, 0)
    var ToMonth = SalesDateTo.getMonth() + 1
    //SalesDateTo.setDate(0)
    var ToDay = SalesDateTo.getDate()
    var formatSalesDateTo = SalesDateTo.getFullYear() + '/' + ToMonth + '/' + ToDay;

    //区分コード
    var WorkType = document.getElementById(WorkType).value;
    var WorkTypeCode;
    if (WorkType == '社外')
    {
        WorkTypeCode = '1';
    }
    else
    {
        WorkTypeCode = '2';
    }
    //部門コード
    var DepartmentCode = document.getElementById(DepartmentCode).value;

    //RequestFlag = 1 (検索)
    var RequestFlag = '1';


    openModalDialog('/ServiceSalesSearch/ServiceSalesReportCriteriaDialog?SalesDateFrom=' + formatSalesDateFrom + '&SalesDateTo=' + formatSalesDateTo + '&WorkType=' + WorkTypeCode + '&DepartmentCode=' + DepartmentCode + '&RequestFlag=' + RequestFlag + '')
    
}

//----------------------------------------------------------------------------------------
//機能：ドロップダウンリストの中身を取得する
//作成日：2015/10/28 arc yano #3289 サービス伝票
//更新日：2017/10/19 arc yano #3803 サービス伝票 部品発注書出力 引数変更(category⇒無し)
//        2017/02/03 arc yano #3426 サービス伝票・伝票修正時　発注画面表示の不具合
//----------------------------------------------------------------------------------------
function GetMasterListAll(controller, targetId) {

    var obj = null;
    var code = null;

    if (typeof category != 'undefined') {
        code = category;
    }

    //Add 2017/02/03 arc yano #3426
    var objslipnumber = document.getElementById('SlipNumber');
    var slipnumber;
    var objrevisionnumber = document.getElementById('RevisionNumber');
    var revisionnumber;

    if (objslipnumber != null) {
        slipnumber = objslipnumber.value;
    }
    if (objrevisionnumber != null) {
        revisionnumber = objrevisionnumber.value;
    }


    $.ajax({
        type: "POST",
        url: "/" + controller + "/GetMasterList",
        contentType: "application/x-www-form-urlencoded",
        data: { code: code, slipnumber: slipnumber, revisionnumber: revisionnumber },
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.length == 0) {
                removeNodes(document.getElementById(targetId));
                return false;
            }
            removeNodes(document.getElementById(targetId));
            createNodes(document.getElementById(targetId), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("在庫判断リストの取得に失敗しました。");
            return false;
        }
    });
}
/*-------------------------------------------------------------------------------------------*/
/* 機能　： 仕入ステータスで検索条件のアクティブ非アクティブを切り替える                     */
/* 作成日： 2015/11/16 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善) */
/* 更新日：                                                                                  */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function ChangePurchaseStatus() {

    var select = document.getElementById("PurchaseStatus");
    var options = select.options;
    var PurchaseStatus = options.item(select.selectedIndex).value;

    if (PurchaseStatus == "001") {
        document.getElementById("PurchaseType").disabled = true;
        document.getElementById("PurchaseDateFrom").disabled = true;
        document.getElementById("PurchaseDateTo").disabled = true;
    } else {
        document.getElementById("PurchaseType").disabled = false;
        document.getElementById("PurchaseDateFrom").disabled = false;
        document.getElementById("PurchaseDateTo").disabled = false;
    }
}
//--------------------------------------------------------------------
//Add 2015/12/28 arc nakayama 部品入荷一覧 ドロップダウンリスト制御
//--------------------------------------------------------------------
function CheckPurchaseStatus() {

    var select = document.getElementById("PurchaseStatus");
    var options = select.options;
    var PurchaseStatus = options.item(select.selectedIndex).value;

    if (PurchaseStatus == '001') {

        document.getElementById("PurchaseType").value = '001';
        document.getElementById("PurchaseDateFrom").value = "";
        document.getElementById("PurchaseDateTo").value = "";

        document.getElementById("PurchaseType").disabled = true;
        document.getElementById("PurchaseDateFrom").disabled = true;
        document.getElementById("PurchaseDateTo").disabled = true;
    } else {

        document.getElementById("PurchaseType").value = '001';
        document.getElementById("PurchaseDateFrom").value = "";
        document.getElementById("PurchaseDateTo").value = "";

        document.getElementById("PurchaseType").disabled = false;
        document.getElementById("PurchaseDateFrom").disabled = false;
        document.getElementById("PurchaseDateTo").disabled = false;

    }
    return;
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 代替え部品フラグを見て、アクティブ/非アクティブを切り替える                      */
/* 作成日： 2015/11/16 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善) */
/* 更新日：                                                                                  */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function ChangePartsdisabled(num) {

    var checkVal = document.getElementById("Purchase[" + num + "]_ChangeParts").checked;

    if (checkVal) {
        document.getElementById("Purchase[" + num + "]_ChangePartsNumber").disabled = false;
        document.getElementById("Purchase[" + num + "]_ChangePartsFlag").value = "1";
    } else {
        document.getElementById("Purchase[" + num + "]_ChangePartsNumber").disabled = true;
        document.getElementById("Purchase[" + num + "]_ChangePartsFlag").value = "0";
    }
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 単価と数量から金額を求める                                                       */
/* 作成日： 2015/12/08 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善) */
/* 更新日： 2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                   */
/*          取得したデータをカンマ付に変換する                                               */
/*          2017/02/14 arc yano #3641 金額欄のカンマ表示対応                                 */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function CalcPurchaseAmount(Num) {

    //Mod 2017/04/23 arc yano #3755
    //カンマ除去
    //ExceptComma(document.getElementById("Purchase[" + Num + "]_PurchasePrice")); //Add 2017/02/14 arc yano #3641

    var price = ConversionExceptComma(document.getElementById("Purchase[" + Num + "]_PurchasePrice").value);

    //カンマ挿入
    //InsertComma(document.getElementById("Purchase[" + Num + "]_PurchasePrice")); //Add 2017/02/14 arc yano #3641

    var Quantity = document.getElementById("Purchase[" + Num + "]_PurchaseQuantity").value;
    var Amount = 0;

    //金額と数量がNullまたは空文字でない時だけ計算を行う
    if ((price != null && price != "") && (Quantity != null && Quantity != "")) {
            Amount = price * Quantity;
        
    }

    document.getElementById("Purchase[" + Num + "]_PurchaseAmount").value = ConversionWithComma(Amount);

    //カンマ挿入
    //InsertComma(document.getElementById("Purchase[" + Num + "]_PurchaseAmount")); //Add 2017/02/14 arc yano #3641
}
/*---------------------------------------------------------------------------------------------------*/
/* 機能　： 入金消込画面で消込対象をにフラグを立てる		                              			 */
/* 作成日： 2016/01/06 arc nakayama #3303_入金消込登録でチェックが付いていないものも処理されてしまう */
/* 更新日：                                                                                          */
/*                                                                                                   */
/*---------------------------------------------------------------------------------------------------*/
function Depositcheck(num) {

    var checkVal = document.getElementById("line[" + num + "]_Changetarget").checked;

    if (checkVal) {
        document.getElementById("line[" + num + "]_targetFlag").value = "1";
    } else {
        document.getElementById("line[" + num + "]_targetFlag").value = "0";
    }
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 発注日を変更したときに連動する日付を変更する                                     */
/* 作成日： 2015/12/08 arc nakayama #3291_部品発注入力(#3234_【大項目】部品仕入れ機能の改善) */
/* 更新日：                                                                                  */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function ChangePlanDate(num, GenuineFlag) {

    var TargetDate = document.getElementById("PurchaseOrderDate").value;

    //書式チェック
    //一致しなければ何もせずに返す
    if (!TargetDate.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
        return;
    }

    var PurchaseOrderDate = new Date(TargetDate);

    var ArrivalPlanDate = new Date(PurchaseOrderDate.getFullYear(), PurchaseOrderDate.getMonth(), PurchaseOrderDate.getDate()+ 1);
    var strArrivalPlanYear = ArrivalPlanDate.getFullYear();
    var strArrivalPlanMonth = ArrivalPlanDate.getMonth() + 1;
    var strArrivalPlanDay = ArrivalPlanDate.getDate();
    //月・日の0(ゼロ)埋め
    strArrivalPlanMonth = ('0' + strArrivalPlanMonth).slice(-2);
    strArrivalPlanDay = ('0' + strArrivalPlanDay).slice(-2);
    var strArrivalPlanDate = strArrivalPlanYear + "/" + strArrivalPlanMonth + "/" + strArrivalPlanDay;

    var PaymentPlanDate = new Date(PurchaseOrderDate.getFullYear(), PurchaseOrderDate.getMonth() + 2, 0);
    var strPaymentPlanYear = PaymentPlanDate.getFullYear();
    var strPaymentPlanMonth = PaymentPlanDate.getMonth() + 1;
    var strPaymentPlanDay = PaymentPlanDate.getDate();
    //月・日の0(ゼロ)埋め
    strPaymentPlanMonth = ('0' + strPaymentPlanMonth).slice(-2);
    strPaymentPlanDay = ('0' + strPaymentPlanDay).slice(-2);
    var strPaymentPlanDate = strPaymentPlanYear + "/" + strPaymentPlanMonth + "/" + strPaymentPlanDay;

    //社外品の時
    if (GenuineFlag == '0') {
        for (var i = 0; i < num; i++) {

            document.getElementById("line[" + i + "]_ArrivalPlanDate").value = strArrivalPlanDate;
            document.getElementById("line[" + i + "]_PaymentPlanDate").value = strPaymentPlanDate;
        }
    //純正品の時
    } else {
        document.getElementById("ArrivalPlanDate").value = strArrivalPlanDate;
        document.getElementById("PaymentPlanDate").value = strPaymentPlanDate;
    }

}
/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： 入荷入力画面表示時に、発注残が０(ゼロ)のデータがあったら入荷数のエリアを読み取り専用にする    */
/* 作成日： 2016/02/10 arc nakayama #3424_部品入荷検索画面　入荷確定後の再検索                            */
/* 更新日：                                                                                               */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function CheckRemainingQuantity(num) {
    for (var i = 0; i < num; i++) {
        var RemainingQuantity = document.getElementById("Purchase[" + i + "]_RemainingQuantity").value;
        
        if (RemainingQuantity != "" && RemainingQuantity <= 0) {    //Mod 2016/02/25 arc yano 不具合修正
            document.getElementById("Purchase[" + i + "]_PurchaseQuantity").readOnly = true;
            document.getElementById("Purchase[" + i + "]_PurchaseQuantity").style.backgroundColor = "#EEEEEE";
            //入荷数が入っていた場合は0(ゼロ)にする
            if (document.getElementById("Purchase[" + i + "]_PurchaseQuantity").value >= 0) {
                document.getElementById("Purchase[" + i + "]_PurchaseQuantity").value = 0;
            }
        }
    }
}

/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： 入荷確定時に、DBの発注残と画面の発注残を比較して差分が出た時はアラートを出す                  */
/* 作成日： 2016/02/10 arc nakayama #3424_部品入荷検索画面　入荷確定後の再検索                            */
/* 更新日： 2016/03/22 arc yano データの送信方法を変更                                                    */
/* 更新日： 2016/04/25 arc nakayama #3494_部品入荷入力画面　発注情報のない入荷データでのエラー            */
/*--------------------------------------------------------------------------------------------------------*/
function CheckRemainingQuantityForCommit(num) {

    var ClientRemainingQuantity = 0.00; //画面上の発注残
    var Difference = 0;          //発注残の差分
    var msg = "";                //メッセージ
    var query = "";
    var cnt = 0;
    //Mod 2016/03/22 arc yano
    var key = function (purchaseOrderNumber, partsNumber, PurchaseQuantity) {
        this.PurchaseOrderNumber = purchaseOrderNumber;
        this.PartsNumber = partsNumber;
        this.PurchaseQuantity = PurchaseQuantity;
        };

    var keyList = new Array();

    for (var i = 0; i < num; i++) {

        //var PurchaseQuantity = document.getElementById('Purchase[' + i + ']_PurchaseQuantity');
        ////入荷数が入力されていて、0でない時だけ発注残のチェックを行う
        //if (PurchaseQuantity != null && PurchaseQuantity.value != null && PurchaseQuantity.value != '' && PurchaseQuantity.value != 0 ) {
        keyList[i] = new key(document.getElementById('Purchase[' + i + ']_PurchaseOrderNumber').value, document.getElementById('Purchase[' + i + ']_PartsNumber').value, document.getElementById("Purchase[" + i + "]_PurchaseQuantity").value);
        //}
    }

   
    $.ajax({
        type: "POST",
        url: "/PartsPurchaseOrder/GetRemainingQuantity",
        contentType: "application/x-www-form-urlencoded",
        data: { KeyList: keyList },
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.length == 0) {
                return false;
            }

            for (var j = 0; j < data.length; j++) {
                var Gyo = j;
                Gyo += 1;
                ClientRemainingQuantity = document.getElementById("Purchase[" + j + "]_RemainingQuantity").value;

                // Mod 2016/04/21 arc nakayama 
                //if (ClientRemainingQuantity == null || ClientRemainingQuantity == "") {
                //    ClientRemainingQuantity = 0;
                //}
                //if (data[j].RemainingQuantity == null || data[j].RemainingQuantity == "") {
                //    data[j].RemainingQuantity = 0;
                //}
                //画面上の発注残と最新の発注残の差分を計算して、差分が０超過だった場合、処理を続行するか問い合わせる
                //画面の発注残と取得した発注残がNULLまたは空文字だった場合は、計算しない
                if (!(ClientRemainingQuantity == null || ClientRemainingQuantity == "") && !(data[j].RemainingQuantity == null || data[j].RemainingQuantity == "")) {

                    Difference = ClientRemainingQuantity - data[j].RemainingQuantity;
                    if (data[j].RemainingQuantity != null && Difference > 0) {
                        msg += Gyo + "行目の発注残が、入荷入力画面を開いた後に更新されています。" + "\n";
                    }

                }
            }
            //メッセージが何もなければ通常通り入荷処理を行う
            if (msg != "") {

                msg += "入荷処理を続行しますか？" + "\n" + "続行すると過入荷として扱われます。";

                MsgRet = confirm(msg);

                if (MsgRet == true) {
                    //はい
                    formSubmit(); //入荷処理
                    ParentformSubmit(); //再検索
                    return true;
                } else {
                    //いいえ
                    ParentformSubmit(); //再検索
                    window.close(); //閉じる
                    return false;
                }

            } else {
                formSubmit(); //入荷処理
                ParentformSubmit(); //再検索
            }

        },
        /*
        error: function (XMLHttpRequest, textStatus, errorThrown) {  //通信失敗時
            alert('通信に失敗しました。textStatus=' + textStatus + 'errorThrown=' + errorThrown + 'XMLHttpRequest.statusText=' + XMLHttpRequest.statusText);
            return false;
        */
        error: function () {  //通信失敗時
            alert('通信に失敗しました');
            return false;
        }
    });
    
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： サービス伝票明細の部品が発注済かどうかをチェックする                             */
/* 作成日： 2016/02/12 arc yano #3429 サービス伝票　判断の活性／非活性の制御の追加           */
/* 更新日： 2016/02/19 arc yano #3435 サービス伝票　原価０の部品の対応                       */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function IsOrdered(
    idPrefix,
    idSlipNumber,
    idLineNumber,
    idStockStatus,
    idStockStatusNote,
    idProvisionQuaintity,
    ControllerName
    ) {

    //伝票番号
    var slipNumber = document.getElementById(idSlipNumber).value;

    if (slipNumber == null || slipNumber == '') {   //伝票番号が空文字の場合(新規作成時)はチェックしない
        return true;
    }

    //行番号
    var lineNumber = document.getElementById(idPrefix + idLineNumber).value;
    //在庫判断
    var stockStatus = document.getElementById(idPrefix + idStockStatus);
    //在庫判断(控え)
    var stockStatusNote = document.getElementById(idPrefix + idStockStatusNote);
   

    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/IsOrdered",
        data: { code: slipNumber, lineNumber: lineNumber },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data != null && data == true) {
                alert("既に部品の発注が行われているため、判断を変更できません");
                
                var option = stockStatus.getElementsByTagName('option');

                for (i = 0; i < option.length; i++) {

                    if (option[i].value == stockStatusNote.value) {
                        option[i].selected = true;
                    }
                    else {
                        option[i].selected = false;
                    }
                }

                if (stockStatus.disabled == true) {
                    stockStatus.disabled = false;
                }

                return false;
            }
            else {
                //在庫判断(控え)を更新
                stockStatusNote.value = stockStatus.value;

                //社達が選択された場合は、引当済数を0にする
                clearValue(stockStatus, idPrefix + idProvisionQuaintity); //Add 2016/02/19 arc yano #3435

                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("マスタ取得に失敗しました。");
            return false;
        }
    });
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 在庫判断で「社達」が選択された場合、引当済数を０にする                           */
/* 作成日：2016/02/19 arc yano #3435 サービス伝票　原価０の部品の対応                        */
/* 更新日：2016/02/22 arc yano #3434 サービス伝票　消耗品(SP)の対応                          */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function clearValue(obj, targetName) {

    if (obj.options[obj.selectedIndex].value == '998' ||
        obj.options[obj.selectedIndex].value == '997' ) {//判断 = 「社達」or「SP」

        var target = document.getElementById(targetName);
        target.value = 0;
    }
}
/*-------------------------------------------------------------------------------------------*/
/* 機能　： 区分で「SP」を選択した場合、判断で「SP」を自動的に選択させる                     */
/* 作成日： #3434 サービス伝票  消耗品(SP)の対応                                             */
/* 更新日：                                                                                  */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function changeStockStatus(obj, prefix , idStockStatus, idHdStockStatus, idOrderQuantity) {

    var stockStatus = document.getElementById(prefix + idStockStatus);                  //判断
    var hdStockStatus = document.getElementById(prefix + idHdStockStatus);              //判断(隠し要素)
    var orderQuantity = document.getElementById(prefix + idOrderQuantity);      //引当済数

    if (stockStatus != undefined && orderQuantity.value == 0) {  //
        
        if (obj.options[obj.selectedIndex].value == '015') { //区分 = 「SP」
            //判断 = 「SP」を選択させる

            var targetOption = stockStatus.getElementsByTagName('option');

            for (var i = 0; i < targetOption.length; i++) {

                if (targetOption[i].value == '997') {
                    targetOption[i].selected = true;
                }
                else {
                    targetOption[i].selected = false;
                }
            }

            var event = stockStatus.onchange();

            hdStockStatus.value = stockStatus.value;

            stockStatus.disabled = true;
            hdStockStatus.disabled = false;
        }
        else {
            stockStatus.disabled = false;
            hdStockStatus.disabled = true;
        }
    }
}

/*-------------------------------------------------------------------------------------------*/
/* 機能　： 全選択・全解除                                                                   */
/* 作成日： 2016/03/14 #3469 部品仕入一覧　[全てにチェック]の追加                            */
/* 更新日： 2016/04/19 #3490 arc yano 機能修正 引数に実行する関数を追加                                                                                 */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function allChecked(obj, func) {

    if (obj.checked) {//全選択

        $(".list input[type=checkbox]").each(function (index) {
            var setval = $(this).val();
            $(this).val([setval]);

            if (typeof (func) != 'undefined' && func != null) {
                if (index >= 1) {//全選択のチェックボックス以外のチェックボックスの場合
                    func(index - 1);
                }
                
            }
        });
    }
    else {//全解除
        $(".list input[type=checkbox]").each(function (index) {
            var setval = $(this).val();
            $(this).val([]);

            if (typeof (func) != 'undefined' && func != null) {
                if (index >= 1) {//全選択のチェックボックス以外のチェックボックスの場合
                    func(index - 1);
                }
            }
        });
    }
}


/*-------------------------------------------------------------------------------------------*/
/* 機能　： 区分リストを取得する                                                             */
/* 作成日： 2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応          　           */
/*                                                                                           */
/*-------------------------------------------------------------------------------------------*/
function GetWorkTypeList(
    targetId,
    ServiceType
    ) {
    
    $.ajax({
        type: "POST",
        url: "/ServiceSalesOrder/GetWorkTypeList",
        data: { code: ServiceType },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.length == 0) {
                removeNodes(document.getElementById(targetId));
                return false;
            }
            removeNodes(document.getElementById(targetId));
            createNodes(document.getElementById(targetId), data, false);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("区分リストの取得に失敗しました。");
            return false;
        }
    });
}
/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： オプションマスタ入力画面のグレードコードをチェックして、必須設定を切り替える            　    */
/* 作成日： 2016/02/15 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定                      */
/* 更新日：                                                                                               */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function CheckCarCode() {

    var CarCode = document.getElementById("CarGradeCode").value;

    //グレードコードが未入力状態だったら、必須設定を「任意」にして「必須」を選択不可にする
    if (CarCode == "" || CarCode == null || CarCode == undefined) {

        //必須フラグが必須になっていたら「任意」にする
        var RequiredFlag = document.getElementById("RequiredFlag").value;
        if (RequiredFlag == "1") {
            document.getElementsByName("RequiredFlag")[1].checked = true;
        }
        //「必須」を非活性にする
        document.getElementsByName("RequiredFlag")[0].disabled = true;

    } else {

        //新規作成だった場合は必須にチェックを入れる、更新の場合は何もしない
        if (document.getElementById("update").value == "0") {

            document.getElementsByName("RequiredFlag")[0].checked = true;
        }

        //「必須」を活性にする
        document.getElementsByName("RequiredFlag")[0].disabled = false;

    }

}
/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： グレードコード選択時に、該当車種に必須のオプションがあった場合オプションを自動追加する        */
/* 作成日： 2016/02/15 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定                      */
/* 更新日：                                                                                               */
/*          2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算                        */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 オプション計算処理追加 */
/*          2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                */
/*          取得したデータをカンマ付に変換する                                                            */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function GetRequiredOptionByCarGradeCode(CarGradeCode, cunt) {

    var GradeCode = document.getElementById(CarGradeCode).value;
    if (GradeCode != null && GradeCode != "") {

        //画面で選択されている新中区分を取得する
        var select = document.getElementById("NewUsedType");
        var options = select.options;
        var NewUsedType = options.item(select.selectedIndex).value;

        if (NewUsedType == "N") {
            $.ajax({
                type: "POST",
                url: "/CarOption/GetRequiredOptionByCarGradeCode",
                contentType: "application/x-www-form-urlencoded",
                async: false,
                data: { GradeCode: GradeCode },
                dataType: "json",
                timeout: 10000,
                success: function (data, dataType) {
                    if (data.length == 0) {
                        for (var i = 0; i < cunt; i++) {
                            document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
                            document.getElementById("line[" + i + "]_CarOptionCode").value = "";
                            document.getElementById("line[" + i + "]_CarOptionName").value = "";
                            document.getElementById("line[" + i + "]_Amount").value = "";
                            document.getElementById("line[" + i + "]_TaxAmount").value = "";
                            document.getElementById("line[" + i + "]_AmountWithTax").value = "";
                        }

                        //Mod 2019/09/04 yano #4011
                        calcTotalOptionAmount();                                
                        //Mod 2019/10/17 yano #4022
                        GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
                        //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
                        calcTotalAmount();
                        return false;
                    }

                    //現在のオプション内容をクリアする
                    for (var i = 0; i < cunt; i++) {
                        document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
                        document.getElementById("line[" + i + "]_CarOptionCode").value = "";
                        document.getElementById("line[" + i + "]_CarOptionName").value = "";
                        document.getElementById("line[" + i + "]_Amount").value = "";
                        document.getElementById("line[" + i + "]_TaxAmount").value = "";
                        document.getElementById("line[" + i + "]_AmountWithTax").value = "";
                    }

                    //取得した必須内容を入れる
                    var taxAmount = 0;
                    for (var i = 0; i < data.length; i++) {

                        taxAmount = calcTaxAmount(data[i].SalesPrice); //伝票の税率から税金を求める
                        document.getElementById("line[" + i + "]_OptionType").value = data[i].OptionType;
                        document.getElementById("line[" + i + "]_CarOptionCode").value = data[i].CarOptionCode;
                        document.getElementById("line[" + i + "]_CarOptionName").value = data[i].CarOptionName;
                        //Mod 2017/04/23 arc yano #3755
                        document.getElementById("line[" + i + "]_Amount").value = ConversionWithComma(data[i].SalesPrice);
                        document.getElementById("line[" + i + "]_TaxAmount").value = ConversionWithComma(taxAmount);

                        calcOptionAmount(false, i);
                        taxAmount = 0;
                    }

                    //Mod 2019/09/04 yano #4011
                    calcTotalOptionAmount();
                    //Mod 2019/10/17 yano #4022
                    GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
                    //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
                    calcTotalAmount();
                },
                error: function () {  //通信失敗時
                    alert("オプション情報の取得に失敗しました。");
                    return false;
                }
            });
        } else {
            //中古車の場合はクリアする
            for (var i = 0; i < cunt; i++) {
                document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
                document.getElementById("line[" + i + "]_CarOptionCode").value = "";
                document.getElementById("line[" + i + "]_CarOptionName").value = "";
                document.getElementById("line[" + i + "]_Amount").value = "";
                document.getElementById("line[" + i + "]_TaxAmount").value = "";
                document.getElementById("line[" + i + "]_AmountWithTax").value = "";                
            }
            calcTotalOptionAmount();                                //Add 2019/09/04 yano #4011
            //Mod 2019/10/17 yano #4022
            GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
            //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val()); //Add 2019/09/04 yano #4011
            calcTotalAmount();
            return false;
        }
    } else {
        for (var i = 0; i < cunt; i++) {
            document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
            document.getElementById("line[" + i + "]_CarOptionCode").value = "";
            document.getElementById("line[" + i + "]_CarOptionName").value = "";
            document.getElementById("line[" + i + "]_Amount").value = "";
            document.getElementById("line[" + i + "]_TaxAmount").value = "";
            document.getElementById("line[" + i + "]_AmountWithTax").value = "";
        }
        //Mod 2019/09/04 yano #4011
        calcTotalOptionAmount();
        //Mod 2019/10/17 yano #4022
        GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
        //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
        calcTotalAmount();
    }

}
/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： 車台番号選択時に、該当車種に必須のオプションがあった場合オプションを自動追加する              */
/* 作成日： 2016/02/15 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定                      */
/* 更新日：                                                                                               */
/*          2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算                        */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業                        */
/*          2017/04/23 arc yano #3755 金額欄の入力時のカーソル位置の不具合                                */
/*          取得したデータをカンマ付に変換する                                                            */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function GetRequiredOptionByVin(Vin, cunt) {


    var VinCode = document.getElementById(Vin).value;

    if (VinCode != null && VinCode != "") {

        $.ajax({
            type: "POST",
            url: "/CarOption/GetRequiredOptionByVin",
            contentType: "application/x-www-form-urlencoded",
            async: false,
            data: { VinCode: VinCode },
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data == null || data.length == 0) {
                    for (var i = 0; i < cunt; i++) {
                        document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
                        document.getElementById("line[" + i + "]_CarOptionCode").value = "";
                        document.getElementById("line[" + i + "]_CarOptionName").value = "";
                        document.getElementById("line[" + i + "]_Amount").value = "";
                        document.getElementById("line[" + i + "]_TaxAmount").value = "";
                        document.getElementById("line[" + i + "]_AmountWithTax").value = "";
                    }
                    //Mod 2019/09/04 yano #4011
                    calcTotalOptionAmount();
                    //Mod 2019/10/17 yano #4022
                    GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
                    //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
                    calcTotalAmount();
                    return false;
                }

                //現在のオプション内容をクリアする
                for (var i = 0; i < cunt; i++) {
                    document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
                    document.getElementById("line[" + i + "]_CarOptionCode").value = "";
                    document.getElementById("line[" + i + "]_CarOptionName").value = "";
                    document.getElementById("line[" + i + "]_Amount").value = "";
                    document.getElementById("line[" + i + "]_TaxAmount").value = "";
                    document.getElementById("line[" + i + "]_AmountWithTax").value = "";
                }

                //取得した必須内容を入れる
                var taxAmount = 0;
                for (var i = 0; i < data.length; i++) {

                    taxAmount = calcTaxAmount(data[i].SalesPrice); //伝票の税率から税金を求める
                    document.getElementById("line[" + i + "]_OptionType").value = data[i].OptionType;
                    document.getElementById("line[" + i + "]_CarOptionCode").value = data[i].CarOptionCode;
                    document.getElementById("line[" + i + "]_CarOptionName").value = data[i].CarOptionName;

                    //Mod 2017/04/23 arc yano #3755
                    document.getElementById("line[" + i + "]_Amount").value = ConversionWithComma(data[i].SalesPrice);
                    document.getElementById("line[" + i + "]_TaxAmount").value = ConversionWithComma(taxAmount);

                    calcOptionAmount(false, i);
                    taxAmount = 0;
                }

                //Mod 2019/09/04 yano #4011
                calcTotalOptionAmount();
                //Mod 2019/10/17 yano #4022
                GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
                //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
                calcTotalAmount();
            },
            error: function () {  //通信失敗時
                alert("オプション情報の取得に失敗しました。");
                return false;
            }
        });
    } else {
        for (var i = 0; i < cunt; i++) {
            document.getElementById("line[" + i + "]_OptionType").value = "001"; //初期値の「販売店」
            document.getElementById("line[" + i + "]_CarOptionCode").value = "";
            document.getElementById("line[" + i + "]_CarOptionName").value = "";
            document.getElementById("line[" + i + "]_Amount").value = "";
            document.getElementById("line[" + i + "]_TaxAmount").value = "";
            document.getElementById("line[" + i + "]_AmountWithTax").value = "";
        }

        //Mod 2019/09/04 yano #4011
        calcTotalOptionAmount();
        //Mod 2019/10/17 yano #4022
        GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#RequestRegistDate').val());
        //GetAcquisitionTax($('#EPDiscountTaxList').val(), $('#SalesPrice').val());
        calcTotalAmount();
        return true;
    }
}

/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： 取得したロケーションのタイプが通常かどうかをチェックする            　                        */
/* 作成日： 2016/04/26 arc yano #3510 部品入荷入力　入荷ロケーションの絞込み                              */
/* 更新日：                                                                                               */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function CheckLocation(param, data) {

    if (data.Code2 != '001') { //ロケーション種別 ≠「通常」
        alert('在庫ロケーションを指定してください');

        document.getElementById(param[0]).value = '';
        document.getElementById(param[1]).value = '';
    }
   
    return;
}
/*--------------------------------------------------------------------------------------------------------------*/
/* 機能　： 請求先タイプチェック(主作業の大分類と請求先タイプをチェックして、矛盾がある場合は請求先をクリアする)*/
/* 作成日： 2016/04/14 arc yano #3480 サービス伝票請求先タイプの振分け                                          */
/* 更新日：                                                                                                     */
/*                                                                                                              */
/*--------------------------------------------------------------------------------------------------------------*/
function CheckCustomerClaimType(param) {
    
    //請求先の請求先分類
    var sWCustomerClaimClass = document.getElementById(param[0]).value;

    //請求先の主作業の大分類
    var cCCustomerClaimClass = document.getElementById(param[1]).value;

    //主作業コード
    var ServiceWorkCode = document.getElementById(param[4]).value;

    //請求先の主作業コード大分類と、主作業の大分類が異なる場合
    //Mod 2017/05/02 arc nakayama #3760_サービス伝票　請求先を選択した状態で主作業のルックアップを開くとシステムエラー
    if (sWCustomerClaimClass != '' && cCCustomerClaimClass != '' && sWCustomerClaimClass != cCCustomerClaimClass && ServiceWorkCode != '') {
        //何もしない
        if (sWCustomerClaimClass == '1') {//請求先分類 = 「社外」
            alert('社外の主作業が設定されているため、社外の請求先を選択してください');
        }
        else {
            alert('社内の主作業が設定されているため、社内の請求先を選択してください');
        }

        //請求先コードをクリア
        document.getElementById(param[2]).value = '';

        //請求先名称をクリア
        document.getElementById(param[3]).value = '';

        document.getElementById(param[1]).value = '';
    }
    calcTotalServiceAmount();
}

/*--------------------------------------------------------------------------------------------*/
/* 機能　： 請求先分類リストの取得                                                            */
/* 作成日： 2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加                       */
/* 更新日：                                                                                   */
/*                                                                                            */
/*--------------------------------------------------------------------------------------------*/
function GetCustomerClaimTypeListByCustomerClaimFilter(customerClaimFilter, targetId) {
    
    var target = document.getElementById(targetId);

    $.ajax({
        type: "POST",
        url: "/Deposit/CustomerClaimTypeByCustomerClaimFilter",
        data: { customerClaimFilter: customerClaimFilter },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data == null || data.length == 0) {
                alert("請求先種別を取得できませんでした");
                removeNodes(target);
                return false;
            }
            removeNodes(target);
            createNodes(target, data, true);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("請求先種別の取得に失敗しました。");
            return false;
        }
    });
    function resetEntity() {
        removeNodes(target);
    }
}
/*--------------------------------------------------------------------------------------------------------*/
/* 機能　： 店舗締め解除処理の前にダイアログで確認する                                                    */
/* 作成日： 2016/02/15 arc nakayama #3536_現金出納帳　店舗締め解除処理ボタンの追加                        */
/* 更新日：                                                                                               */
/*                                                                                                        */
/*--------------------------------------------------------------------------------------------------------*/
function CheckReleaseAccount(OfficeName, CloseDate) {

    var msg = "";   //メッセージ

    msg += OfficeName + ' ' + CloseDate + 'の店舗締めを解除します。' + "\n" + 'よろしいですか？';

    MsgRet = confirm(msg);

    if (MsgRet == true) {
        document.forms[0].action.value = 'ReleaseAccount';
        displaySearchList();

    } else {
        //いいえ
    }
}
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： 表示単位の切替で入金予定日From～Toの活性/非活性を切り替える                                                                                       */
/* 作成日： 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加                        */
/* 更新日：                                                                                                                                                   */
/*                                                                                                                                                            */
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
function CheckDispFlag(Enabled) {

    if (Enabled == '1') {
        document.getElementById("ReceiptPlanDateFrom").disabled = true;
        document.getElementById("ReceiptPlanDateTo").disabled = true;
    } else {
        document.getElementById("ReceiptPlanDateFrom").disabled = false;
        document.getElementById("ReceiptPlanDateTo").disabled = false;
    }
}
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： 入金管理画面にある入金額の合計と、入金後の残高を更新する                                                                                          */
/* 作成日： 2016/07/22 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）表示単位を変更できる件条件追加                        */
/* 更新日：                                                                                                                                                   */
/*                                                                                                                                                            */
/*------------------------------------------------------------------------------------------------------------------------------------------------------------*/
function calcDepositAmount(num) {

    var totalAmount = 0;

    for (var i = 0; i < num; i++) {
        var flg = document.getElementById('line[' + i + ']_Changetarget').checked;
        if (flg) {
            var amount = isNaN(parseInt(document.getElementById('line[' + i + ']_Amount').value)) ? 0 : parseInt(document.getElementById('line[' + i + ']_Amount').value);
            totalAmount += amount;
        }
    }
    //入金額合計更新
    document.getElementById('TotalAmount').innerText = totalAmount;
    //入金後の残高更新

    var TotalBalance = 0
    TotalBalance = (document.getElementById('TotalBalance').innerText).replace(/,/g, "");

    var receivableBalance = isNaN(parseInt(TotalBalance)) ? 0 : parseInt(TotalBalance);
    document.getElementById('CalcTotalBalance').innerText = currency(receivableBalance - totalAmount);
}

/*----------------------------------------------------------------------------------------------*/
/* 機能　： 入力した部門コードから倉庫の情報を取得する                                          */
/* 作成日： 2016/08/13 arc yano #3596 【大項目】部門棚統合対応部門棚統合対応 倉庫追加           */
/* 更新日： 2021/05/20 yano #4045 Chrome対応 nameではなくclassで要素を取得するように修正        */
/*                                                                                              */
/*----------------------------------------------------------------------------------------------*/
function GetWarehouseFromDepartment(
    InputCodeField,
    InputNameField,
    TargetCodeField,
    TargetNameField,
    ControllerName,
    ButtonControl
    ) {

    if (document.getElementById(InputCodeField).value == null || document.getElementById(InputCodeField).value == '') {
        if (TargetCodeField != '') {
            var elements = document.getElementsByClassName(TargetCodeField); //Mod 2021/05/20 yano #4045
            //var elements = document.getElementsByName(TargetCodeField);

            //倉庫コードの名を持つエレメント全てに値をクリアする
            for (var i = 0; i < elements.length; i++) {
                try {
                    if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                        elements[i].value = '';
                    }
                    else {
                        elements[i].innerHTML = '';
                    }
                } catch (e) {
                    elements[i].value = '';
                }
            }

            var element2 = document.getElementById(TargetNameField);
            try {
                if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {//input項目の場合
                    element2.value = '';
                }
                else {
                    element2.innerHTML = '';
                }
            } catch (e) {
                element2.value = '';
            }
        }

        return false;

    } else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMaster",
            data: { code: document.getElementById(InputCodeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {

                    alert("倉庫が存在しません");

                    //入力したコードをクリア
                    document.getElementById(InputCodeField).value = '';
                    document.getElementById(InputCodeField).focus();

                    //入力した名前をクリア
                    document.getElementById(InputNameField).value = '';
                    document.getElementById(InputNameField).focus();

                    if (TargetCodeField != '') {
                        var elements = document.getElementsByClassName(TargetCodeField); //Mod 2021/05/20 yano #4045
                        //var elements = document.getElementsByName(TargetCodeField);  
                        //倉庫コードの名を持つエレメント全てに値を設定する
                        for (var i = 0; i < elements.length; i++) {
                            try {
                                if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                                    elements[i].value = '';
                                }
                                else {
                                    elements[i].innerHTML = '';
                                }
                            } catch (e) {
                                elements[i].value = '';
                            }
                        }

                        var element2 = document.getElementById(TargetNameField);
                        try {
                            if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {
                                element2.value = '';
                            }
                            else {
                                element2.innerHTML = '';
                            }
                        } catch (e) {
                            element2.innerText = '';
                        }

                    }
                   
                    return false;

                } else {
                    if (TargetCodeField != '') {

                        var elements = document.getElementsByClassName(TargetCodeField); //Mod 2021/05/20 yano #4045
                        //var elements = document.getElementsByName(TargetCodeField);
                        //倉庫コードの名を持つエレメント全てに値を設定する
                        for (var i = 0; i < elements.length; i++) {
                            try {
                                if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                                    elements[i].value = data.Code;
                                }
                                else {
                                    elements[i].innerHTML = escapeHtml(data.Code);
                                }
                            } catch (e) {
                                elements[i].value = data.Code;
                            }
                        }
                        
                        element2 = document.getElementById(TargetNameField);
                        try {
                            if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {//input項目の場合
                                element2.value = data.Name;
                            }
                            else {
                                element2.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element2.value = data.Name;
                        }
                    }

                    if (ButtonControl == 'true') {

                        //棚卸月
                        var inventoryMonth = document.getElementById('InventoryMonth').value;
                        //棚卸作業日
                        var workingDate = document.getElementById('PartsInventoryWorkingDate').value;
                        //倉庫コード
                        var warehouseCode = document.getElementById('HdWarehouseCode').value;

                        //棚卸ステータスチェック
                        CheckInventoryStatus(warehouseCode, inventoryMonth, workingDate);

                        //棚卸開始日取得
                        GetStartDateForPartsInventory(warehouseCode, inventoryMonth, 'PartsInventoryStartDate', 'PartsInventory');
                    }

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
    
}
/*----------------------------------------------------------------*/
/* 機能　： クレジット入金確認画面の必須チェック                  */
/* 作成日： 2016/09/07 arc nakayama #3630_【製造】車両売掛金対応  */
/* 更新日：                                                       */
/*                                                                */
/*----------------------------------------------------------------*/
function CreditRequiredCheck() {

    var JournalDateFrom = document.getElementById("JournalDateFrom").value;
    var JournalDateTo = document.getElementById("JournalDateTo").value;

    if ((JournalDateFrom == null || JournalDateFrom == '') && (JournalDateTo == null || JournalDateTo == '')) {

        alert("決済日は必須項目です");

        return false;
    } else {
        return true;
    }
}

/*---------------------------------------------------------------------------------*/
/* 機能　： 新中区分変更時にダイアログをだす                                       */
/*          は　い：オプションを入れ替え再計算する                                 */
/*          いいえ：新中区分のみを変更                                             */
/* 作成日： 2017/02/06 arc nakayama #3481_車両伝票の新中区分の間違えないようにする */
/* 更新日：                                                                        */
/*                                                                                 */
/*---------------------------------------------------------------------------------*/
function NewUsedCalcCheck(CarGradeCode, cunt) {

    var msg = "";   //メッセージ

    msg += '新中区分が変更されました。' + '\n' + 'オプション情報を再取得して再計算を行ってもよろしいですか？' + '\n' + '「キャンセル」を選択した場合、新中区分のみ変更されます。';

    MsgRet = confirm(msg);

    if (MsgRet == true) {
        GetRequiredOptionByCarGradeCode(CarGradeCode, cunt)
    } else {
        //いいえ 何もしない
    }
}
//Mod 2021/05/07 yano #4045 Chrome対応 別関数に移動したため、コメントアウト
///*------------------------------------------------------------------------------*/
///* 機能　： 金額欄のカンマ挿入/除去                                              */
///* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
///* 更新日：                                                                     */
///*                                                                              */
///*------------------------------------------------------------------------------*/
//function InsertExceptComma() {
  
//    //フォーカスを失ったとき
//    $('.money').blur(function () {
//        //カンマ挿入
//        InsertComma(this);
//    })

//    /* //2017/04/21 arc yano カンマ暫定対応
//    //フォーカスを得たとき
//    $('.money').focus(function () {
//        //カンマ除去
//        ExceptComma(this);
//    })
//    */
//}

/*------------------------------------------------------------------------------*/
/* 機能　： 金額欄のカンマ挿入                                                  */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function InsertComma(obj) {

    //うっかり入力しているかもしれないカンマを消す
    obj.value = obj.value.replace(/,/g, '');

    //整数に変換したのち文字列に戻す
    //この時点で数字とマイナス記号だけが残る
    var num = "" + parseInt(obj.value);

    //変数 num の中身が、桁区切りされる
    while (num != (num = num.replace(/(\d)(?=(\d{3})+$)/g, '$1,')));

    //numに入っている値が数値じゃないときは0とする
    if (isNaN(parseInt(num))) num = '';

    //桁区切りした結果（変数 num）でテキストボックスの中身を書き換える
    obj.value = num;
}

/*------------------------------------------------------------------------------*/
/* 機能　： 金額欄のカンマ除去                                                  */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function ExceptComma(obj) {

    //カンマを消す
    obj.value = obj.value.replace(/,/g, '');
}

/*------------------------------------------------------------------------------*/
/* 機能　： 全ての金額欄のカンマ挿入                                            */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function InsertCommaAll() {

    var elements = $('.money');

    for (i = 0; i < elements.length; i++) {
        InsertComma(elements[i]);
    }
}

/*------------------------------------------------------------------------------*/
/* 機能　： 全ての金額欄のカンマ除去                                            */
/* 作成日： 2017/02/14  arc yano #3641 金額表示をカンマ表示に統一する           */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function ExceptCommaAll() {

    var elements = $('.money');

    for (i = 0; i < elements.length; i++) {
        ExceptComma(elements[i]);
    }
}
/*------------------------------------------------------------------------------*/
/* 機能　： カンマ付数値→カンマ無数値に変換                                    */
/* 作成日： 2017/04/22  arc yano #3755 カンマ対応によるカーソル位置の不具合     */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function ConversionExceptComma(commaval) {

    //カンマを消す
    var val = isNaN(parseInt(commaval.replace(/,/g, ''))) ? 0 : parseInt(commaval.replace(/,/g, ''));

    return val;
}
/*------------------------------------------------------------------------------*/
/* 機能　： カンマ付数値←カンマ無数値に変換                                    */
/* 作成日： 2017/04/22  arc yano #3755 カンマ対応によるカーソル位置の不具合     */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function ConversionWithComma(val) {


    //うっかり入力しているかもしれないカンマを消す
    var commaval = "" + val;
    var commaval = commaval.replace(/,/g, '');

    commaval = "" + parseInt(commaval);

    //変数 commaval の中身が、桁区切りされる
    while (commaval != (commaval = commaval.replace(/(\d)(?=(\d{3})+$)/g, '$1,')));

    //commavalに入っている値が数値じゃないときは0とする
    if (isNaN(parseInt(commaval))) commaval = '';

    return commaval;
    
}



/*---------------------------------------------------*/
/* 機能　： 納車リストの表示を切り替える             */
/* 作成日： 2017/03/09 arc nakayama #3723_納車リスト */
/* 更新日：                                          */
/*                                                   */
/*---------------------------------------------------*/
function changeDisplayCarSales(name) {
    ChDsp(0, '0');
    ChDsp(0, '1');
    ChDsp(0, '2');
    ChDsp(0, '3');
    ChDsp(0, '4');
    ChDsp(1, name);
}
/*----------------------------------------------------------------*/
/* 機能　： 納車リスト(明細)を台数から開く                        */
/* 作成日： 2017/03/09 arc nakayama #3723_納車リスト              */
/* 更新日： 2020/01/16 yano #4027 引数追加                        */
/*                                                                */
/*----------------------------------------------------------------*/
function OpenCarSalesDetail(TargetYear, SelectMonthCode, DepartmentCode, DepartmentName, AAType) {//Mod 2020/01/16 yano #4027

    //指定年
    var select = document.getElementById(TargetYear);
    var options = select.options;
    var SelectYearCode = options.item(select.selectedIndex).value;

    var RequestFlag = '1';

    openModalDialog('/CarSalesList/CarSalesDetailCriteriaDialog?SelectYearCode=' + SelectYearCode + '&SelectMonthCode=' + SelectMonthCode + '&DepartmentCode=' + DepartmentCode + '&DepartmentName=' + DepartmentName + '&RequestFlag=' + RequestFlag + '&AAType=' + AAType + '') //Mod 2020/01/16 yano #4027

}

/*----------------------------------------------------------------*/
/* 機能　： 納車リスト(担当者別)を部門から開く                    */
/* 作成日： 2017/03/09 arc nakayama #3723_納車リスト              */
/* 更新日： 2020/01/16 yano #4027 引数追加                        */
/*                                                                */
/*----------------------------------------------------------------*/
function OpenCarSalesDetailEmployee(TargetYear, DepartmentCode, DepartmentName, AAType) {//Mod 2020/01/16 yano #4027


    //指定年
    var select = document.getElementById(TargetYear);
    var options = select.options;
    var SelectYearCode = options.item(select.selectedIndex).value;
    openModalDialog('/CarSalesList/CarSalesDetailEmployeeCriteriaDialog?SelectYearCode=' + SelectYearCode + '&DepartmentCode=' + DepartmentCode + '&DepartmentName=' + DepartmentName + '&indexid=' + AAType + '')  //Mod 2020/01/16 yano #4027

}

/*-----------------------------------------------------------------------------------------------------------------------*/
/* 機能　： 顧客コードからマスタを検索し、名称、電話番号、携帯番号、住所、車両伝票の数、サービス伝票の数を取得する       */
/* 作成日： 2017/03/09 arc nakayama #3723_納車リスト                                                                     */
/* 更新日：                                                                                                              */
/*                                                                                                                       */
/*-----------------------------------------------------------------------------------------------------------------------*/
function GetNameFromCodeForCustomerIntegrate(
    codeField,     //マスタ検索時のキー項目
    nameFieldArray,//取得したマスタ情報をセットする項目　※複数設定可
    controllerName //検索先マスタ
    ) {

    if (document.getElementById(codeField).value != null && document.getElementById(codeField).value == '') {
        resetEntity();
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/" + controllerName + "/GetMasterDetailForCustomerIntegrate",
            data: { code: document.getElementById(codeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data[0] == undefined) {
                    alert("マスタに存在しません");
                    resetEntity();
                    document.getElementById(codeField).focus();
                    return false;
                }else{
                    for (var i = 0; i < nameFieldArray.length; i++) {
                        var srvItemName = nameFieldArray[i];
                        var element = document.getElementById(nameFieldArray[i]);
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
                            }
                            else
                            {
                                element.innerHTML = escapeHtml(data[i] == undefined ? '' : data[i]);
                            }
                        } catch (e) {
                            element.value = data[i] == null ? '' : data[i];
                        }
                    }
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");

                return false;
            }

        });
    }
    function resetEntity() {
        document.getElementById(codeField).value = '';
        for (var j = 0; j < nameFieldArray.length; j++) {
            try {

                var target = document.getElementById(nameFieldArray[j]);
                var tagName = target.tagName.toLowerCase();

                if (tagName == 'input') {   //テキストボックス
                    document.getElementById(nameFieldArray[j]).value = '';
                }
                else if (tagName == 'select') { //リストボックス   
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                else {  //その他(ラベル)
                    document.getElementById(nameFieldArray[j]).innerText = '';
                }

            } catch (e) {
                document.getElementById(nameFieldArray[j]).innerText = '';
            }
        }
    }
}
/*--------------------------------------------------*/
/* 機能　： 名寄せ処理ボタン押下時のチェック        */
/* 作成日： 2017/03/21 arc nakayama #3723_納車リスト*/
/* 更新日：                                         */
/*                                                  */
/*--------------------------------------------------*/
function CustomerIntegrateCheck(codeField1, codeField2) {

    if ((document.getElementById(codeField1).value == null || document.getElementById(codeField1).value == '') || (document.getElementById(codeField2).value == null || document.getElementById(codeField2).value == '')) {
        alert("残したい顧客コードと消したい顧客コードを両方入力して下さい。");
        return false;
    }

    if (document.getElementById(codeField1).value == document.getElementById(codeField2).value) {
        alert('残したい顧客コードと消したい顧客コードが同じです。' + '\n' + '別の顧客コードを設定して下さい。');
        return false;
    }

    var msg = "";   //メッセージ

    msg += document.getElementById(codeField2).value + 'を削除し、' + document.getElementById(codeField1).value + 'にデータを統合します。' + '\n' + 'また、処理に1～2分かかる場合がります。よろしいですか？';

    MsgRet = confirm(msg);

    if (MsgRet == true) {
        DisplayImage('UpdateMsg', '0');
        formSubmit();
    } else {
        //いいえ 何もしない
    }
}

/*----------------------------------------------------------------------------------------------*/
/* 機能　： 車両仕入税抜金額の計算のチェック                                                    */
/* 作成日： 2017/03/06  arc yano #3640 車両仕入の計算がおかしい                                 */
/* 更新日： 2017/04/27  arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合       */
/*                                                                                              */
/*----------------------------------------------------------------------------------------------*/
function ChkCarPurhcasePrice() {

    var ret = 0;

    //各項目の税抜価格
    var vehiclePrice = ConversionExceptComma(document.getElementById('VehiclePrice').value);
    var auctionFeePrice = ConversionExceptComma(document.getElementById('AuctionFeePrice').value);
    var carTaxAppropriatePrice = ConversionExceptComma(document.getElementById('CarTaxAppropriatePrice').value);
    var recyclePrice = ConversionExceptComma(document.getElementById('RecyclePrice').value);
    var othersPrice = ConversionExceptComma(document.getElementById('OthersPrice').value);
    var metallicPrice = ConversionExceptComma(document.getElementById('MetallicPrice').value);
    var optionPrice = ConversionExceptComma(document.getElementById('OptionPrice').value);
    var firmPrice = ConversionExceptComma(document.getElementById('FirmPrice').value);
    var discountPrice = ConversionExceptComma(document.getElementById('DiscountPrice').value);
    var equipmentPrice = ConversionExceptComma(document.getElementById('EquipmentPrice').value);
    var repairPrice = ConversionExceptComma(document.getElementById('RepairPrice').value);
    

    //仕入税抜価格(入力値)
    var amount = ConversionExceptComma(document.getElementById('Amount').value);

    //仕入税抜価格(各税抜価格の合計値)
    var calcPrice = vehiclePrice + auctionFeePrice + carTaxAppropriatePrice
                   + recyclePrice + othersPrice + metallicPrice + optionPrice
                   + firmPrice + discountPrice + equipmentPrice + repairPrice;


    if(amount != calcPrice){
        ret = 1;
    }

    return ret;
}

/*------------------------------------------------------------------------------------------*/
/* 機能　： 車両仕入税込金額の計算のチェック                                                */
/* 作成日： 2017/03/06  arc yano #3640 車両仕入の計算がおかしい                             */
/* 更新日： 2017/04/27  arc yano #3755 車両伝票入力　金額欄の入力時のカーソル位置の不具合   */
/*                                                                                          */
/*------------------------------------------------------------------------------------------*/
function ChkCarPurhcaseAmount() {

    var ret = 0;

    //各項目の税込価格
    var vehicleAmount = ConversionExceptComma(document.getElementById('VehicleAmount').value);
    var auctionFeeAmount = ConversionExceptComma(document.getElementById('AuctionFeeAmount').value);
    var carTaxAppropriateAmount = ConversionExceptComma(document.getElementById('CarTaxAppropriateAmount').value);
    var recycleAmount = ConversionExceptComma(document.getElementById('RecycleAmount').value);
    var othersAmount = ConversionExceptComma(document.getElementById('OthersAmount').value);
    var metallicAmount = ConversionExceptComma(document.getElementById('MetallicAmount').value);
    var optionAmount = ConversionExceptComma(document.getElementById('OptionAmount').value);
    var firmAmount = ConversionExceptComma(document.getElementById('FirmAmount').value);
    var discountAmount = ConversionExceptComma(document.getElementById('DiscountAmount').value);
    var equipmentAmount = ConversionExceptComma(document.getElementById('EquipmentAmount').value);
    var repairAmount = ConversionExceptComma(document.getElementById('RepairAmount').value);

    //仕入税込価格(入力値)
    var totalAmount = ConversionExceptComma(document.getElementById('TotalAmount').value);

    //仕入税込価格(各税込価格の合計値)
    var calcTotalAmount = vehicleAmount + auctionFeeAmount + carTaxAppropriateAmount
                    + recycleAmount + othersAmount + metallicAmount + optionAmount
                    + firmAmount + discountAmount + equipmentAmount + repairAmount;
    

    if(totalAmount != calcTotalAmount){
        ret = 1;
    }

    return ret;
}

//----------------------------------------------------------------------------------------
//機能：コードリスト(c_CodeName)を取得する
//作成日：2017/03/13 arc yano #3725 サブシステム移行(整備履歴) 新規作成
//更新日：
//----------------------------------------------------------------------------------------
function GetCodeMasterListAll(controller, targetId, category) {

    var categorycode = null;

    if (typeof category != 'undefined') {
        categorycode = category;
    }

    $.ajax({
        type: "POST",
        url: "/" + controller + "/GetCodeMasterList",
        contentType: "application/x-www-form-urlencoded",
        data: { categorycode: categorycode },
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.length == 0) {
                removeNodes(document.getElementById(targetId));
                return false;
            }
            removeNodes(document.getElementById(targetId));
            createNodes(document.getElementById(targetId), data, true);
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("コードリストの取得に失敗しました。");
            return false;
        }
    });
}

/*----------------------------------------------------------------------------*/
/* 機能　： 伝票ステータスによってボタンの活性非活性を切り替える(伝票戻し)    */
/* 作成日： 2017/05/11 arc nakayama #3761_サブシステムの伝票戻しの移行        */
/* 更新日：                                                                   */
/*                                                                            */
/*----------------------------------------------------------------------------*/
function StatusChangeButtonControll() {

    var SalesOrderStatus = document.getElementById('SalesOrderStatus').value;
    var ErrFlag = document.getElementById('ErrFlag').value;

    if ((ErrFlag != '1') && (SalesOrderStatus == '003' || SalesOrderStatus == '004' || SalesOrderStatus == '005')) {

        document.getElementById('StatusBack002').disabled = false;
        document.getElementById('StatusBack001').disabled = true;

    } else if (ErrFlag != '1' && SalesOrderStatus == '002') {
        document.getElementById('StatusBack002').disabled = true;
        document.getElementById('StatusBack001').disabled = false;
    } else {
        document.getElementById('StatusBack002').disabled = true;
        document.getElementById('StatusBack001').disabled = true;
    }

}

/*---------------------------------------------------------------------*/
/* 機能　： 進行中と履歴の表示を切り替える(伝票戻し)                   */
/* 作成日： 2017/05/11 arc nakayama #3761_サブシステムの伝票戻しの移行 */
/* 更新日：                                                            */
/*                                                                     */
/*---------------------------------------------------------------------*/
function changeDisplayCarSlipChange(name) {
    ChDsp(0, '0');
    ChDsp(0, '1');
    ChDsp(1, name);
}


/*---------------------------------------------------------------------------------------------------------------------------------------------*/
/* 機能　： グレードコード変更時にダイアログをだす                                                                                             */
/*          は　い：オプションを入れ替え再計算する                                                                                             */
/*          いいえ：グレードコードのみを変更                                                                                                   */
/* 作成日： 2017/06/01 arc nakayama #3767_車両伝票　見積時、グレードを変更しても金額変更するかしないかをユーザーに選ばせる                     */
/* 更新日：                                                                                                                                    */
/*          2023/10/05 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除                                 */
/*          2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 グレードコード変更時に環境性能割の再計算を行う              */
/*          2018/07/30 yano #3906 販売区分でAA・依廃の場合は諸費用・非課税項目に金額を設定しない                                               */
/*                                                                                                                                             */
/*---------------------------------------------------------------------------------------------------------------------------------------------*/
function GradeChangeCalcCheck(CarGradeCode, cunt) {

    //Add 2018/07/30 yano #3906
    var salesType = document.getElementById('SalesType').value;
    var param;

    if (salesType == '004' || salesType == '008') {//販売区分がAA・依廃の場合

        param = new Array(
              'MakerName'
            , 'CarBrandName'
            , 'CarName'
            , 'CarGradeName'
            , 'SalesPrice'
            , 'SalesTax'
            , 'SalesPriceWithTax'
            , 'ModelName'
            )
    }
    else {

        param = new Array(
              'MakerName'
            , 'CarBrandName'
            , 'CarName'
            , 'CarGradeName'
            , 'SalesPrice'
            , 'SalesTax'
            , 'SalesPriceWithTax'
            , 'InspectionRegistCost'
            //, 'TradeInMaintenanceFee'     //Mod 2023/10/05 yano #4184
            //, 'InheritedInsuranceFee'     //Mod 2023/10/05 yano #4184
            , 'AcquisitionTax'
            , 'RecycleDeposit'
            , 'ModelName'
            , 'CarLiabilityInsurance'
            , 'CarWeightTax'
            , 'EPDiscountTaxList'       //Add 2019/09/04 yano #4011
            )
    }

    //グレードコードが入っているときだけチェックする
    if (CarGradeCode != '' || CarGradeCode != null || CarGradeCode != undefined) { 

        //伝票番号が空文字でない(=一度でも伝票が保存されている)
        if (document.getElementById('SlipNumber').value != '') {  //Mod 2017/11/09 arc yano 
            var msg = "";   //メッセージ
            msg += 'グレードコードが変更されました。' + '\n' + 'オプション情報や金額を再取得して再計算を行ってもよろしいですか？' + '\n' + '「キャンセル」を選択した場合、グレードコードのみ変更されます。';

            MsgRet = confirm(msg);

            if (MsgRet == true) {
                GetMasterDetailFromCode('CarGradeCode', param, 'CarGrade'); //Mod 2018/07/31 yano #3906
                //GetMasterDetailFromCode('CarGradeCode', new Array('MakerName', 'CarBrandName', 'CarName', 'CarGradeName', 'SalesPrice', 'SalesTax', 'SalesPriceWithTax', 'InspectionRegistCost', 'TradeInMaintenanceFee', 'InheritedInsuranceFee', 'AcquisitionTax', 'RecycleDeposit', 'ModelName', 'CarLiabilityInsurance', 'CarWeightTax'), 'CarGrade');

                //setTimeout('calcTotalAmount(true)', 1000);    //Del 2019/09/04 yano #4011

                setTimeout(function () { GetRequiredOptionByCarGradeCode(CarGradeCode, cunt); GetCarTax() }, 500);

                //GetRequiredOptionByCarGradeCode(CarGradeCode, cunt);

            } else {
                //車両情報だけ更新
                GetMasterDetailFromCode('CarGradeCode', new Array('MakerName', 'CarBrandName', 'CarName', 'CarGradeName', 'ModelName'), 'CarGrade');
            }
        }
        else {  //伝票が保存されていない場合は無条件で更新
            GetMasterDetailFromCode('CarGradeCode', param, 'CarGrade'); //Mod 2018/07/31 yano #3906

            //setTimeout('calcTotalAmount(true)', 1000);    //Del 2019/09/04 yano #4011

            setTimeout(function () { GetRequiredOptionByCarGradeCode(CarGradeCode, cunt);; GetCarTax() }, 500);

            //GetRequiredOptionByCarGradeCode(CarGradeCode, cunt);
        }
    }
}

//---------------------------------------------------------------------------------
//機能  ：部門コードから部門名・倉庫コード・倉庫名・棚卸開始日時を取得する
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：
//---------------------------------------------------------------------------------
function GetNameFromCodeForCarInventory(
    ControllerName,
    func,
    param
    ) {

    var tablehtml = '<table class="list" style="width: 100%">';
    tablehtml += '<tr>';
    tablehtml += '<th style="width: 20px; height: 20px; text-align: center;">';
    tablehtml += '</th>';
    tablehtml += '<th style="white-space: nowrap">ロケーションコード</th>';
    tablehtml += '<th style="white-space: nowrap">ロケーション名</th>';
    tablehtml += '<th style="white-space: nowrap">管理番号</th>';
    tablehtml += '<th style="white-space: nowrap">車台番号</th>';
    tablehtml += '<th style="white-space: nowrap">新中区分</th>';
    tablehtml += '<th style="white-space: nowrap">在庫区分</th>';
    tablehtml += '<th style="white-space: nowrap">ブランド名</th>';
    tablehtml += '<th style="white-space: nowrap">車種名</th>';
    tablehtml += '<th style="white-space: nowrap">系統色</th>';
    tablehtml += '<th style="white-space: nowrap">車両カラーコード</th>';
    tablehtml += '<th style="white-space: nowrap">車両カラー名</th>';
    tablehtml += '<th style="white-space: nowrap">車両登録番号</th>';
    tablehtml += '<th style="white-space: nowrap">実棚</th>';
    tablehtml += '<th style="white-space: nowrap">誤差理由</th>';
    tablehtml += '<th style="white-space: nowrap">備考</th>';
    tablehtml += '</tr>';
    tablehtml += '</table>';


    var pagehtml = '<span style="font-size:12pt"><b>0 - 0 / 0 </b>&nbsp;</span>';

    //部門コードが未入力の場合
    if (document.getElementById('DepartmentCode').value != null && document.getElementById('DepartmentCode').value == '') {
        if ('DepartmentName' != '') {

            //部門名のクリア
            var element = document.getElementById('DepartmentName');
            try {
                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                    element.value = '';
                }
                else {
                    element.innerHTML = '';
                }
            } catch (e) {
                element.value = '';
            }

            //倉庫コードのクリア
            document.getElementById('WarehouseCode').innerHTML = '';
            //倉庫名のクリア
            document.getElementById('WarehouseName').innerHTML = '';

            //棚卸開始日時のクリア
            document.getElementById('sInventoryStartDate').innerHTML = '';
            document.getElementById('InventoryStartDate').value = '';

            //ページのクリア
            document.getElementById('pageBlock').innerHTML = pagehtml;
            //一覧表のクリア
            document.getElementById('tableBlock').innerHTML = tablehtml;
        }
        
        //引数が指定されていた場合は処理を行う
        if (typeof (func) != 'undefined' && func != null) {
            var ret = func(param);
        }

        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMasterForCarInventory",
            data: { code: document.getElementById('DepartmentCode').value},
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {
                    if (controllername = 'Department') {
                        alert("入力した部門コードは棚卸対象外部門か、またはマスタに存在しません");
                    }
                    else {
                        alert("マスタに存在しません");
                    }
                    if ('DepartmentName' != '') {
                        var element = document.getElementById('DepartmentName');
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = '';
                            }
                            else {
                                element.innerHTML = '';
                            }
                        } catch (e) {
                            element.innerText = '';
                        }
                    }

                    //倉庫コードのクリア
                    document.getElementById('WarehouseCode').innerHTML = '';
                    //倉庫名のクリア
                    document.getElementById('WarehouseName').innerHTML = '';

                    //棚卸開始日時のクリア
                    document.getElementById('sInventoryStartDate').innerHTML = '';
                    document.getElementById('InventoryStartDate').value = '';

                    document.getElementById('DepartmentCode').value = '';
                    document.getElementById('DepartmentCode').focus();

                    //ページのクリア
                    document.getElementById('pageBlock').innerHTML = pagehtml;
                    //一覧表のクリア
                    document.getElementById('tableBlock').innerHTML = tablehtml;

                    GetWarehouseFromDepartmentCarInventory('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DepartmentWarehouse', SetInventoryStartDate);
                    
                    return false;

                } else {
                    if ('DepartmentName' != '') {
                        element = document.getElementById('DepartmentName');
                        try {
                            if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                                element.value = data.Name;
                            }
                            else {
                                element.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element.value = data.Name;
                        }
                    }

                    //ページのクリア
                    document.getElementById('pageBlock').innerHTML = pagehtml;
                    //一覧表のクリア
                    document.getElementById('tableBlock').innerHTML = tablehtml;

                    GetWarehouseFromDepartmentCarInventory('DepartmentCode', 'DepartmentName', 'WarehouseCode', 'WarehouseName', 'DepartmentWarehouse', SetInventoryStartDate);

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                if ('DepartmentName' != '') {
                    var element = document.getElementById('DepartmentName');
                    try {
                        if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合
                            element.value = '';
                        }
                        else {
                            element.innerHTML = '';
                        }
                    } catch (e) {
                        element.innerText = '';
                    }
                }

                //倉庫コードのクリア
                document.getElementById('WarehouseCode').innerHTML = '';
                //倉庫名のクリア
                document.getElementById('WarehouseName').innerHTML = '';

                //棚卸開始日時のクリア
                document.getElementById('sInventoryStartDate').innerHTML = '';
                document.getElementById('InventoryStartDate').value = '';

                document.getElementById('DepartmentCode').value = '';
                document.getElementById('DepartmentCode').focus();

                //ページのクリア
                document.getElementById('pageBlock').innerHTML = pagehtml;
                //一覧表のクリア
                document.getElementById('tableBlock').innerHTML = tablehtml;

                //引数が指定されていた場合は処理を行う
                if (typeof (func) != 'undefined' && func != null) {
                    var ret = func(param);
                }
                return false;
            }
        });
    }
}

//----------------------------------------------------------------------------------------------------------
//機能  ：オブジェクトの活性/非活性切替
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：2018/04/10 arc yano #3875 車両棚卸　部門コード入力時に棚卸開始ボタンが活性化しない
//        2018/02/02 arc yano #3852 車両棚卸　棚卸基準日当日で「棚卸開始」できてしまう 
//        2017/09/07 arc yano #3784 棚差チェックボックス、画面リスト出力ボタンの活性／非活性処理の追加
//----------------------------------------------------------------------------------------------------------
function SetDisableCarInventory(status) {

    switch (status) {
       
        case '000':     //ステータス=「未実施」

            //棚卸基準日の翌日以降の場合のみ「棚卸開始」ボタンをクリックできる。
            var workingDateTime= new Date(document.getElementById('InventoryWorkingDate').value);
            //現在時刻
            var currentDateTime = new Date();

            //日付のみに直す
            var workingDate = new Date(workingDateTime.getFullYear(), workingDateTime.getMonth(), workingDateTime.getDay());

            var currentDate = new Date(currentDateTime.getFullYear(), currentDateTime.getMonth(), currentDateTime.getDay());
            
            /*
            //棚卸作業日(整数値)
            var wd = workingDate.getFullYear() * 100 + workingDate.getMonth() * 10 + workingDate.getDate();
            //現在日時(整数値)
            var cd = currentDate.getFullYear() * 100 + currentDate.getMonth() * 10 + currentDate.getDate(); 
            */

            //棚卸開始ボタン
            if (currentDate > workingDate) {
                document.getElementById('InventoryStartButton').disabled = false;
            }
            else{
                document.getElementById('InventoryStartButton').disabled = true;
            }

            document.getElementById('ExcelOutputButton').disabled = true;       //Excel出力ボタン
            document.getElementById('ExcelInputButton').disabled = true;        //Excel取込ボタン
            document.getElementById('InventorySaveButton').disabled = true;     //棚卸結果一時保存ボタン

            //棚卸仮確定ボタン
            if (document.getElementById('InventoryTempDecidedVisible').value == 'true') {
                document.getElementById('InventoryTempDecidedButton').disabled = true;
            }

            //Add 2018/08/01 yano #3926
            //棚卸仮確定キャンセルボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryCancelButton').disabled = true;
            }


            //棚卸本確定ボタンを押せるユーザ以外
            if (document.getElementById('InventoryDecidedVisible').value != 'true') {

            document.getElementById('SearchButton').disabled = true;            //検索ボタン
            document.getElementById('ExcelOut').disabled = true;                //画面リスト出力ボタン
            document.getElementById('ClearButton').disabled = true;             //クリアボタン

            //テキストボックスの読取専用
            document.getElementById('LocationCode').readOnly = true;
            document.getElementById('LocationName').readOnly = true;
            document.getElementById('SalesCarNumber').readOnly = true;
            document.getElementById('Vin').readOnly = true;
            document.getElementById('NewUsedType').disabled = true;
            document.getElementById('CarStatus').disabled = true;
            document.getElementById('InventoryDiff').disabled = true;           //棚差有無  //Add 2017/09/07 arc yano #3784

            //背景色、入力値の設定
            document.getElementById('LocationCode').style.backgroundColor = "#eeeeee";
            document.getElementById('LocationName').style.backgroundColor = "#eeeeee";
            document.getElementById('SalesCarNumber').style.backgroundColor = "#eeeeee";
            document.getElementById('Vin').style.backgroundColor = "#eeeeee";

            }

            break;

        case '001':     //ステータス=「実施中」

            document.getElementById('InventoryStartButton').disabled = true;     //棚卸開始ボタン
            document.getElementById('ExcelOutputButton').disabled = false;       //Excel出力ボタン
            document.getElementById('ExcelInputButton').disabled = false;        //Excel取込ボタン
            document.getElementById('InventorySaveButton').disabled = false;     //棚卸結果一時保存ボタン

            //棚卸仮確定ボタン
            if (document.getElementById('InventoryTempDecidedVisible').value == 'true') {
                document.getElementById('InventoryTempDecidedButton').disabled = false;
            }

            //Add 2018/08/01 yano #3926
            //棚卸仮確定キャンセルボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryCancelButton').disabled = true;
            }
           
            //棚卸本確定ボタンを押せるユーザ以外
            if (document.getElementById('InventoryDecidedVisible').value != 'true') {

            document.getElementById('SearchButton').disabled = false;            //検索ボタン
            document.getElementById('ExcelOut').disabled = false;                //画面リスト出力ボタン
            document.getElementById('ClearButton').disabled = false;             //クリアボタン

            //テキストボックスの読取専用
            document.getElementById('LocationCode').readOnly = false;
            document.getElementById('LocationName').readOnly = false;
            document.getElementById('SalesCarNumber').readOnly = false;
            document.getElementById('Vin').readOnly = false;
            document.getElementById('NewUsedType').disabled = false;
            document.getElementById('CarStatus').disabled = false;
            document.getElementById('InventoryDiff').disabled = false;           //棚差有無  //Add 2017/09/07 arc yano #3784

            //背景色、入力値の設定
            document.getElementById('LocationCode').style.backgroundColor = "#ffffff";
            document.getElementById('LocationName').style.backgroundColor = "#ffffff";
            document.getElementById('SalesCarNumber').style.backgroundColor = "#ffffff";
            document.getElementById('Vin').style.backgroundColor = "#ffffff";
            }

            break;

        case '002'://ステータス=「仮確定」
            document.getElementById('InventoryStartButton').disabled = true;     //棚卸開始ボタン
            document.getElementById('ExcelOutputButton').disabled = false;       //Excel出力ボタン
            document.getElementById('ExcelInputButton').disabled = true;        //Excel取込ボタン
            document.getElementById('InventorySaveButton').disabled = true;     //棚卸結果一時保存ボタン

            //棚卸仮確定ボタン
            if (document.getElementById('InventoryTempDecidedVisible').value == 'true') {
                document.getElementById('InventoryTempDecidedButton').disabled = true;
            }

            //Add 2018/08/01 yano #3926
            //棚卸仮確定キャンセルボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryCancelButton').disabled = false;
            }

            //棚卸本確定ボタンを押せるユーザ以外
            if (document.getElementById('InventoryDecidedVisible').value != 'true') {

                document.getElementById('SearchButton').disabled = false;            //検索ボタン
                document.getElementById('ExcelOut').disabled = false;                //画面リスト出力ボタン
                document.getElementById('ClearButton').disabled = false;             //クリアボタン

                //テキストボックスの読取専用
                document.getElementById('LocationCode').readOnly = false;
                document.getElementById('LocationName').readOnly = false;
                document.getElementById('SalesCarNumber').readOnly = false;
                document.getElementById('Vin').readOnly = false;
                document.getElementById('NewUsedType').disabled = false;
                document.getElementById('CarStatus').disabled = false;
                document.getElementById('InventoryDiff').disabled = true;           //棚差有無  //Add 2017/09/07 arc yano #3784

                //背景色、入力値の設定
                document.getElementById('LocationCode').style.backgroundColor = "#ffffff";
                document.getElementById('LocationName').style.backgroundColor = "#ffffff";
                document.getElementById('SalesCarNumber').style.backgroundColor = "#ffffff";
                document.getElementById('Vin').style.backgroundColor = "#ffffff";
            }
            break;

        case '003'://ステータス=「確定」
            document.getElementById('InventoryStartButton').disabled = true;     //棚卸開始ボタン
            document.getElementById('ExcelOutputButton').disabled = false;       //Excel出力ボタン
            document.getElementById('ExcelInputButton').disabled = true;        //Excel取込ボタン
            document.getElementById('InventorySaveButton').disabled = true;     //棚卸結果一時保存ボタン

            //棚卸仮確定ボタン
            if (document.getElementById('InventoryTempDecidedVisible').value == 'true') {
                document.getElementById('InventoryTempDecidedButton').disabled = true;
            }

            //Add 2018/08/01 yano #3926
            //棚卸仮確定キャンセルボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryCancelButton').disabled = true;
            }

            /*
            //棚卸本確定ボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryDecidedButton').disabled = true;
            }
            */

            //棚卸本確定ボタンを押せるユーザ以外
            if (document.getElementById('InventoryDecidedVisible').value != 'true') {

            document.getElementById('SearchButton').disabled = false;            //検索ボタン
                document.getElementById('ExcelOut').disabled = false;                //画面リスト出力ボタン
            document.getElementById('ClearButton').disabled = false;             //クリアボタン

            //テキストボックスの読取専用
            document.getElementById('LocationCode').readOnly = false;
            document.getElementById('LocationName').readOnly = false;
            document.getElementById('SalesCarNumber').readOnly = false;
            document.getElementById('Vin').readOnly = false;
            document.getElementById('NewUsedType').disabled = false;
            document.getElementById('CarStatus').disabled = false;
                document.getElementById('InventoryDiff').disabled = true;           //棚差有無  //Add 2017/09/07 arc yano #3784

            //背景色、入力値の設定
            document.getElementById('LocationCode').style.backgroundColor = "#ffffff";
            document.getElementById('LocationName').style.backgroundColor = "#ffffff";
            document.getElementById('SalesCarNumber').style.backgroundColor = "#ffffff";
            document.getElementById('Vin').style.backgroundColor = "#ffffff";
            }
            break;

        default:     //ステータス=「不明」

            document.getElementById('InventoryStartButton').disabled = true;    //棚卸開始ボタン
            document.getElementById('ExcelOutputButton').disabled = true;       //Excel出力ボタン
            document.getElementById('ExcelInputButton').disabled = true;        //Excel取込ボタン
            document.getElementById('InventorySaveButton').disabled = true;     //棚卸結果一時保存ボタン

            //棚卸仮確定ボタン
            if (document.getElementById('InventoryTempDecidedVisible').value == 'true') {
                document.getElementById('InventoryTempDecidedButton').disabled = true;
            }

            //Add 2018/08/01 yano #3926
            //棚卸仮確定キャンセルボタン
            if (document.getElementById('InventoryDecidedVisible').value == 'true') {
                document.getElementById('InventoryCancelButton').disabled = true;
            }

            //棚卸本確定ボタンを押せるユーザ以外
            if (document.getElementById('InventoryDecidedVisible').value != 'true') {
            document.getElementById('SearchButton').disabled = true;            //検索ボタン
                document.getElementById('ExcelOut').disabled = true;                //画面リスト出力ボタン
            document.getElementById('ClearButton').disabled = true;             //クリアボタン

            //テキストボックスの読取専用
            document.getElementById('LocationCode').readOnly = true;
            document.getElementById('LocationName').readOnly = true;
            document.getElementById('SalesCarNumber').readOnly = true;
            document.getElementById('Vin').readOnly = true;
            document.getElementById('NewUsedType').disabled = true;
            document.getElementById('CarStatus').disabled = true;

            //背景色、入力値の設定
            document.getElementById('LocationCode').style.backgroundColor = "#eeeeee";
            document.getElementById('LocationCode').value = "";
            document.getElementById('LocationName').style.backgroundColor = "#eeeeee";
            document.getElementById('LocationName').innerHTML = "";
            document.getElementById('SalesCarNumber').style.backgroundColor = "#eeeeee";
            document.getElementById('SalesCarNumber').value = "";
            document.getElementById('Vin').style.backgroundColor = "#eeeeee";
            document.getElementById('Vin').value = "";
            }
            break;
    }

    //棚卸本確定ボタン
    if (document.getElementById('InventoryDecidedClickable').value == 'true') {
        document.getElementById('InventoryDecidedButton').disabled = false;
    }
    else {
        document.getElementById('InventoryDecidedButton').disabled = true;
    }

    return;

}
//--------------------------------------------------------------------
//機能  ：入力した部門コードから倉庫の情報を取得する   
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：2021/05/20 yano #4045 elementをnameではなく、classで取得
//--------------------------------------------------------------------
function GetWarehouseFromDepartmentCarInventory(
    InputCodeField,
    InputNameField,
    TargetCodeField,
    TargetNameField,
    ControllerName,
    func
    ) {

    if (document.getElementById(InputCodeField).value == null || document.getElementById(InputCodeField).value == '') {
        if (TargetCodeField != '') {
            //Mod 2021/05/20 yano #4045
            var elements = document.getElementsByClassName(TargetCodeField);
            //var elements = document.getElementsByName(TargetCodeField);

            //倉庫コードの名を持つエレメント全てに値をクリアする
            for (var i = 0; i < elements.length; i++) {
                try {
                    if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                        elements[i].value = '';
                    }
                    else {
                        elements[i].innerHTML = '';
                    }
                } catch (e) {
                    elements[i].value = '';
                }
            }

            var element2 = document.getElementById(TargetNameField);
            try {
                if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {//input項目の場合
                    element2.value = '';
                }
                else {
                    element2.innerHTML = '';
                }
            } catch (e) {
                element2.value = '';
            }
        }

        //引数が指定されていた場合は処理を行う
        if (typeof (func) != 'undefined' && func != null) {
            var ret = func();
        }

        return false;

    } else {
        $.ajax({
            type: "POST",
            url: "/" + ControllerName + "/GetMaster",
            data: { code: document.getElementById(InputCodeField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data.Code == null) {

                    alert("倉庫が存在しません");

                    //入力したコードをクリア
                    document.getElementById(InputCodeField).value = '';
                    document.getElementById(InputCodeField).focus();

                    //入力した名前をクリア
                    document.getElementById(InputNameField).value = '';
                    document.getElementById(InputNameField).focus();

                    if (TargetCodeField != '') {
                        //Mod 2021/05/20 yano #4045
                        var elements = document.getElementsByClassName(TargetCodeField);
                        //var elements = document.getElementsByName(TargetCodeField);
                        //倉庫コードの名を持つエレメント全てに値を設定する
                        for (var i = 0; i < elements.length; i++) {
                            try {
                                if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                                    elements[i].value = '';
                                }
                                else {
                                    elements[i].innerHTML = '';
                                }
                            } catch (e) {
                                elements[i].value = '';
                            }
                        }

                        var element2 = document.getElementById(TargetNameField);
                        try {
                            if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {
                                element2.value = '';
                            }
                            else {
                                element2.innerHTML = '';
                            }
                        } catch (e) {
                            element2.innerText = '';
                        }

                    }
                   
                    return false;

                } else {
                    if (TargetCodeField != '') {
                        //Mod 2021/05/20 yano #4045
                        var elements = document.getElementsByClassName(TargetCodeField);
                        //var elements = document.getElementsByName(TargetCodeField);
                        //倉庫コードの名を持つエレメント全てに値を設定する
                        for (var i = 0; i < elements.length; i++) {
                            try {
                                if ((elements[i].tagName == 'INPUT') || (elements[i].tagName == 'input')) {//input項目の場合
                                    elements[i].value = data.Code;
                                }
                                else {
                                    elements[i].innerHTML = escapeHtml(data.Code);
                                }
                            } catch (e) {
                                elements[i].value = data.Code;
                            }
                        }
                        
                        element2 = document.getElementById(TargetNameField);
                        try {
                            if ((element2.tagName == 'INPUT') || (element2.tagName == 'input')) {//input項目の場合
                                element2.value = data.Name;
                            }
                            else {
                                element2.innerHTML = escapeHtml(data.Name);
                            }
                        } catch (e) {
                            element2.value = data.Name;
                        }
                    }

                    //引数が指定されていた場合は処理を行う
                    if (typeof (func) != 'undefined' && func != null) {
                        var ret = func();
                    }

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                return false;
            }
        });
    }
    
}

//--------------------------------------------------------------------
//機能  ：入力した部門コードから倉庫の情報を取得する   
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：
//--------------------------------------------------------------------
function SetInventoryStartDate() {

    //棚卸月
    var inventoryMonth = document.getElementById('InventoryMonth').value;
   
    //倉庫コード
    var warehouseCode = document.getElementById('HdWarehouseCode').value;

    //棚卸作業日
    var workingDate = document.getElementById('InventoryWorkingDate').value;

    //棚卸ステータスチェック
    CheckCarInventoryStatus(warehouseCode, inventoryMonth, workingDate);

    //棚卸開始日取得
    GetStartDateForPartsInventory(warehouseCode, inventoryMonth, 'InventoryStartDate', 'CarInventory');

}

//--------------------------------------------------------------------
//機能  ：棚卸ステータスチェック 
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：
//--------------------------------------------------------------------
function CheckCarInventoryStatus(warehousecode, inventorymonth, workingdate) {

    //対象年月
    var InventoryMonth = InventoryMonth + "/01";

    //前対象年月
    var vYear = InventoryMonth.substr(0, 4) - 0;
    var vMonth = InventoryMonth.substr(5, 2) - 1;
    var vDay = 01;                      // 日は１日とする。
    var PrevInventoryMonth = vYear + "/" + vMonth + "/" + vDay;

    //本日日付
    var ToDatetime = new Date();
    var tYear = ToDatetime.getYear();
    var tMonth = ToDatetime.getMonth() + 1;
    var tDay = 01;                      // 日は１日とする。
    var ToDate = tYear + "/" + tMonth + "/" + tDay;

    var workingDateD = null;

    if (workingdate != null && workingdate != "") {
        workingDateD = new Date(workingdate);
        workingDateD.setDate(workingDateD.getDate() + 1);
    }

    $.ajax({
        type: "POST",
        url: "/Monthly/CarInventorySchedule",
        data: { WarehouseCode: warehousecode, InventoryMonth: inventorymonth },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {

            //各項目の設定
            SetDisableCarInventory(data.Code);
            return true;
        }
        , error: function () {  //通信失敗時
            alert("棚卸情報の取得に失敗しました。");

            //各項目の設定
            SetDisableCarInventory('999');
            return false;
        }
    });
}

//--------------------------------------------------------------------
//機能  ：隠し項目設定 
//作成日：2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
//更新日：
//--------------------------------------------------------------------
function setCarInventoryHidden(id, data) {

    var hdid = id + '_hd';

    var target = document.getElementById(hdid);

    if (target != null && typeof (target) != 'undefined') {

        if (target.tagName == "SELECT") {
            for (var m = 0; m < target.childNodes.length; m = m + 2) {
                if (target.childNodes[m].value == data) {
                    target.selectedIndex = target.childNodes[m].index;
                }
            }
        } else {
            try {
                if ((target.tagName == 'INPUT') || (target.tagName == 'input')) {//input項目の場合

                    if (target.className == 'money') { //金額欄なら
                        target.value = data == undefined ? '' : ConversionWithComma(data);
                    }
                    else {
                        target.value = data == undefined ? '' : data;
                    }


                }
                else {
                    if (target.className == 'money') { //金額欄なら
                        target.innerHTML = escapeHtml(data == undefined ? '' : ConversionWithComma(data));
                    }
                    else {
                        target.innerHTML = escapeHtml(data == undefined ? '' : data);
                    }

                }
            } catch (e) {
                if (target.className == 'money') { //金額欄なら
                    target.value = data == null ? '' : ConversionWithComma(data);
                }
                else {
                    target.value = data == null ? '' : data;
                }
            }
        }
    }
}

//-------------------------------------------------------------------
// 機能　： 実棚数入力値チェック                                     
//          実棚数の入力値のチェックを行う                 　        
//                                                                   
// 作成日： 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成  　           
// 更新日： 2017/12/18 arc yano #3840 車両在庫棚卸　実棚数の初期値の変更                                                        
//-------------------------------------------------------------------
function ChkPhysicalQuantityForCarInventory(id, preid, permitnull) {

  
    //実棚数(編集後)
    var target = document.getElementById(id);

    //実棚数(編集前)
    var pretarget = document.getElementById(preid);

    //Mod 2017/12/18 arc yano #3840 
    if (typeof (permitnull) != 'undefined' && permitnull != true) {

    if (target.value != '1' && target.value != '0') {
        alert("実棚数には1か0を入力して下さい。")
        target.value = pretarget.value;
        target.blur();
        target.focus();
        return false;
    }
    }
    else {

        if (target.value != '' && target.value != '1' && target.value != '0') {
            alert("実棚数には1か0を入力して下さい。")
            target.value = pretarget.value;
            target.blur();
            target.focus();
            return false;
        }
    }

    

    return true;

}

//-------------------------------------------------------------------
// 機能　： オブジェクトの活性／非活性                               
//　　　　　自分の選択値により、対象を自動的に選択状態にする。       
// 作成日： 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成  
// 更新日：                                                          
//-------------------------------------------------------------------
function SetDisabledAll(elementsid, elementsname, disabled) {


    if (elementsid != null) {

        for (var i = 0; i < elementsid.length; i++) {

            var target = document.getElementById(elementsid[i]);

            try {
                if ((target.type == 'BUTTON') || (target.type == 'button')) {
                    target.disabled = disabled;
                }
                else if ((target.type == 'TEXT') || (target.type == 'text')) {
                    target.readonly = !disabled;
                    target.value = '';
                }
                else if ((target.type == 'radio') || (target.type == 'RADIO')) {
                    target.disabled = disabled;
                }
                else {

                }
            } catch (e) {

            }
        }

    }

    if (elementsname != null) {

        var target = document.getElementsByName(elementsname);

        for (var i = 0; i < target.length; i++) {

            try {
                if ((target[i].type == 'BUTTON') || (target[i].type == 'button')) {
                    target[i].disabled = disabled;
                }
                else if ((target[i].type == 'TEXT') || (target[i].type == 'text')) {
                    target[i].readonly = !disabled;
                    target[i].value = '';
                }
                else if ((target[i].type == 'radio') || (target[i].type == 'RADIO')) {
                    target[i].disabled = disabled;
                }
                else {

                }
            } catch (e) {

            }
        }

    }
}

/*--------------------------------------------------------------------------------*/
/* 機能　： 数量入力値チェック                                                    */
/*          実棚数の入力値が以下になっているかをチェックする　                    */
/*          ①フォーマットチェック(マイナス、空欄もエラー)                        */
/* 作成日： #3779 部品在庫（部品在庫の修正）データ編集  　                        */
/* 更新日：                                                                       */
/*          2022/08/30 yano #4101【部品在庫編集】在庫編集画面の調整時のメッセージ */
/*--------------------------------------------------------------------------------*/
function ChkQuantity(id, preid, format, targetName, formatName) {

    //数量(編集後)
    var target = document.getElementById(id);

    //数量(編集前)
    var pretarget = document.getElementById(preid);

    //指定された正規表現にマッチしない場合は編集前に戻す
    if (!target.value.match(format)) {
        alert(targetName + 'には' + formatName + 'を入力してください');
        target.value = pretarget.value;
        target.blur();
        target.focus();
        return false;
    }

    //pretarget.value = target.value;   //Mod 2022/08/30 yano #4101 コメントアウト

    return true;
}

/*-------------------------------------------------------------------*/
/* 機能　： フリー在庫算出                                           */
/*          数量と引当済数からフリー在庫を算出し設定する　           */
/* 作成日： #3779 部品在庫（部品在庫の修正）データ編集  　           */
/* 更新日：                                                          */
/*-------------------------------------------------------------------*/
function SetFreeStock(quantityId, provisionId, freeId) {

    //数量
    var quantity = parseFloat(document.getElementById(quantityId).value);

    //引当済数
    var provisionquantity = parseFloat(document.getElementById(provisionId).value);

    //フリー在庫数
    document.getElementById(freeId).innerHTML = (quantity - provisionquantity).toFixed(1);
}

/*------------------------------------------------------------------------------*/
/* 機能　： 画面閉じる			                                                */
/* 作成日： 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能    */
/* 更新日：                                                                     */
/*                                                                              */
/*------------------------------------------------------------------------------*/
function carSaelsFormClose() {

    if (confirm('作業終了前に保存はしましたか？\r\nウィンドウを閉じてよければ「はい」を選択して下さい。')) {
        document.forms[0].action = '/CarSalesOrder/UnLock';
        document.forms[0].submit();
        window.close();
    }
}
//-----------------------------------------------------------------------------------------------
//機能  ：テキストボックスに入力した文字列から空白文字(全角・半角・タブスペース)を除去する
//作成日：#3775 サービス伝票　引当されない 部品番号の空白、タブスペースを削除 新規作成
//更新日：
//-----------------------------------------------------------------------------------------------
function removalSpace(target) {
    
    //入力文字列から全角・半角・タブスペースを除去する
    target.value = target.value.replace(/^[\s　]+|[\s　]+$/g, "");

}

//-----------------------------------------------------------------------------------------------
//機能  ：新中区分の表示・非表示を切り替える
//作成日：#3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更
//更新日：2021/02/22 yano #4081 【車両仕入】【車両在庫棚卸】画面から棚卸データ追加時の不具合
//        2020/08/29 yano #4049 【車両在庫棚卸】新中区分を更新できない不具合の対応
//-----------------------------------------------------------------------------------------------
function changeDisplayNewUedType(idcarStatus, idnewuedType, idhdnewuedType) {

    //選択値を取得する
    var selectedVal = document.getElementById(idcarStatus).value;

    if (selectedVal == '999' || selectedVal == '006') { //在庫区分が「在庫」または「自社登録」
        ChDsp(true, idnewuedType);

        var html = document.getElementById(idnewuedType).parentNode.innerHTML.replace('--', '');

        document.getElementById(idnewuedType).parentNode.innerHTML = html;    //ハイフンを消す
    }
    else {

        //Add 2021/02/22 yano #4081
        if (document.getElementById(idhdnewuedType) != 'undefined') {
            document.getElementById(idhdnewuedType).value = document.getElementById(idnewuedType).value;
        }

        ChDsp(false, idnewuedType);
        document.getElementById(idnewuedType).parentNode.innerHTML = document.getElementById(idnewuedType).parentNode.innerHTML.replace('--', '');
        document.getElementById(idnewuedType).parentNode.innerHTML += '--';    //ハイフンを消す
    }

    //Add 2020/08/29 yano #4049
    //棚卸確定ボタンが表示(=システム管理者)以外の場合は新中区分を非活性に変更する
    var objdsdbuttton = document.getElementById('InventoryDecidedVisible');

    if (objdsdbuttton != 'undefined' && objdsdbuttton.value == 'false') {
        document.getElementById(idnewuedType).disabled = true;
    }
}

//-----------------------------------------------------------------------------------------------
//機能  ：純正品の場合、デフォルト仕入先の設定
//作成日：2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
//更新日：2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
//-----------------------------------------------------------------------------------------------
function setGenuineSupplier(nameArray, data) {
   
    //Mod 2018/06/01 arc yano #3894
    //対象の部品が純正品の場合
    if (data["GenuineType"] == '001' && (document.getElementById(nameArray[1]).value == '' || document.getElementById(nameArray[1]).value == null)) {

        //メーカーコードが「JG」または「LR」の場合仕入先はJLR、それ以外はFCJ
        if (data["MakerCode"] == 'JG' || data["MakerCode"] == 'LR') {
            document.getElementById(nameArray[1]).value = document.getElementById('JLRGenuineSupplierCode').value;
            document.getElementById(nameArray[2]).value = document.getElementById('JLRGenuineSupplierName').value;
        }
        else {
            document.getElementById(nameArray[1]).value = document.getElementById('GenuineSupplierCode').value;
            document.getElementById(nameArray[2]).value = document.getElementById('GenuineSupplierName').value;
        }
    } 
}


//-----------------------------------------------------------------------------------------------
//機能  ：入力した伝票番号が存在するかチェック
//作成日：2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
//更新日：
//----------------------------------------------------------------------------------------------
function isExistsSlip(target, ControllerName) {

    if (target.value == null || target.value == '') {
        resetEntity();
        return false;
    }

    $.ajax({
        type: "POST",
        url: "/" + ControllerName + "/IsExistSlip",
        data: { slipNumber: target.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function(data, dataType) {
            if (data.Code == null) {
                alert("入力した伝票番号の伝票は存在しません");
                resetEntity();
                target.focus();
                return false;
            }
            
            return true;
        }
        ,
        error: function () {  //通信失敗時
            alert("伝票情報の取得に失敗しました。");
            return false;
        }
    });
    function resetEntity() {
        target.value = '';
    }
}
//-----------------------------------------------------------------------------------------------
//機能  ：入金振替前のチェック
//作成日：2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
//更新日：
//----------------------------------------------------------------------------------------------
function checkExecTransfer(className) {

    //指定されたクラス名を持つ要素の抜出
    if (typeof (document.getElementsByClassName) == 'undefined') {
        document.getElementsByClassName = function (t) {
            var elems = new Array();
            if (document.all) {
                var allelem = document.all;
            } else {
                var allelem = document.getElementsByTagName("*");
            }
            for (var i = j = 0, l = allelem.length; i < l; i++) {
                var names = allelem[i].className.split(/\s/);
                for (var k = names.length - 1; k >= 0; k--) {
                    if (names[k] === t) {
                        elems[j] = allelem[i];
                        j++;
                        break;
                    }
                }
            }
            return elems;
        };
    }

    var elements = document.getElementsByClassName(className);

    var ret = false;

    for (var i = 0; i < elements.length ; i++) {
        
        if (elements[i].checked == true) {
            ret = true;
        }
    }

    if (!ret) {
        alert('チェックボックスにチェックを入れて下さい');
        return false;
    }
    else {
        return true;
    }

}
//-----------------------------------------------------------------------------------------------
//機能  ：引当済数の解除
//作成日：2018/02/24 arc yano #3831 新規作成
//更新日：
//----------------------------------------------------------------------------------------------
function resetProvisionQuantity(subject, targetid) {

    var target = document.getElementById(targetid);


    if (subject.value != '999' && subject.value != '998' && subject.value != '997' &&  target.value != '0') {
        target.value = 0.00;
    }

}
//-----------------------------------------------------------------------------
//機能  ：入力した部門の入荷データの直近の仕入先を取得する
//作成日：2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
//更新日：
//-----------------------------------------------------------------------------
function getSuppierCodeFromPartsPurchase(param) {//param[0]…departmentCodeId, param[1]…supplierCodeId, param[2]…supplierNameId, param[3]…PartsPurchaseOrder, param[4]…supplierPaymentCodeId, param[5]…supplierPaymentNameId, param[6]…supplierPayment

    target = document.getElementById(param[0]);

    if (target.value == null || target.value == '') {
        return false;
    }

    $.ajax({
        type: "POST",
        url: "/" + param[3] + "/GetSupplierCodeFromPartsPurchase",
        data: { DepartmentCode: target.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                return false;
            }
            else {
                //コードをセット
                document.getElementById(param[1]).value = data.Code;
                //名称をセット
                document.getElementById(param[2]).value = data.Name;

                document.getElementById(param[4]).value = data.Code;

                GetNameFromCode(param[4], param[5], param[6]);

                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("仕入先の取得に失敗しました。");
            return false;
        }
    });
}

//-----------------------------------------------------------------------------
//機能  ：部品発注画面の支払先に仕入先と同じように設定する。
//作成日：2018/07/27 yano.hiroki #3923 部門マスタにデフォルト仕入先を設定
//更新日：
//-----------------------------------------------------------------------------
function setSupplierPayment(param) {//param[0]…departmentCodeId, param[1]…supplierCodeId, param[2]…supplierNameId, param[3]…supplierPaymentCodeId, param[4]…supplierPaymentNameId, param[5]…supplierPayment

    target = document.getElementById(param[0]);

    if (target.value == null || target.value == '') {
        return false;
    }

    $.ajax({
        type: "POST",
        url: "/Department/GetSupplierFromDepCode",
        data: { code: target.value },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                return false;
            }
            else {
                //コードをセット
                document.getElementById(param[1]).value = data.Code;
                //名称をセット
                document.getElementById(param[2]).value = data.Name;

                //コードを支払い先にセット
                document.getElementById(param[3]).value = data.Code;

                GetNameFromCode(param[3], param[4], param[5]);

                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("仕入先の取得に失敗しました。");
            return false;
        }
    });
}

//---------------------------------------------------------------------------------------------------------
//機能  ：車両販売区分がAA・依廃の場合は諸費用・非課税項目の金額を設定しない
//作成日：2018/07/30 yano.hiroki #3906 新規作成
//更新日：
//        2023/10/05 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除
//        2022/05/20 yano  #4069【車両伝票入力】車台番号を入力した時の仕様変更
//        2022/01/13 yano #4123 【サービス伝票入力】未仕入の車両が選択できる不具合の対応
//        2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応 
//        2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  オプション行の引数追加
//---------------------------------------------------------------------------------------------------------
function decisionSetCost(functype, linecount) {
    
    var salestype = document.getElementById('SalesType').value;

    var param;

    //販売区分がAA・依廃の場合は諸費用・非課税項目は金額の設定対象外
    if (salestype == '004' || salestype == '008') {

        param = new Array(
              'NewUsedType'
            , 'CarGradeCode'
            , 'CarGradeName'
            , 'MakerName'
            , 'CarBrandName'
            , 'CarName'
            , 'ExteriorColorCode'
            , 'ExteriorColorName'
            , 'InteriorColorCode'
            , 'InteriorColorName'
            , 'Mileage'
            , 'MileageUnit'
            , 'ModelName'
            , 'SalesPrice'
            , 'SalesCarNumber'
            )
    }
    else {

        param = new Array(
              'NewUsedType'
            , 'CarGradeCode'
            , 'CarGradeName'
            , 'MakerName'
            , 'CarBrandName'
            , 'CarName'
            , 'ExteriorColorCode'
            , 'ExteriorColorName'
            , 'InteriorColorCode'
            , 'InteriorColorName'
            , 'Mileage'
            , 'MileageUnit'
            , 'ModelName'
            , 'SalesPrice'
            , 'SalesCarNumber'
            //, 'TradeInMaintenanceFee'     //Mod 2023/10/05 yano #4184
            //, 'InheritedInsuranceFee'     //Mod 2023/10/05 yano #4184
            , 'AcquisitionTax'
            , 'CarWeightTax'
            , 'CarLiabilityInsurance'
            , 'RecycleDeposit'
            , 'EPDiscountTaxList'       //Add 2019/09/04 yano #4011
            , 'Vin'                     //Add 2020/06/09 yano #4052
            )
    }

    //Add 2022/05/20 yano #4069
    var vin = document.getElementById('Vin').value;
    //車台番号が入力されている場合のみチェックする
    if (vin != '' && vin != null && vin != undefined) {

        //伝票番号が空文字でない(=一度でも伝票が保存されている)
        if (document.getElementById('SlipNumber').value != '' || (document.getElementById('FromCopy') != undefined && document.getElementById('FromCopy').value.toLowerCase() == 'true')) {
            var msg = "";   //メッセージ
            msg += '車台番号が変更されました。' + '\n' + 'オプション情報や金額を再取得して再計算を行ってもよろしいですか？' + '\n' + '「キャンセル」を選択した場合、車台番号・グレードのみ変更されます。';

            MsgRet = confirm(msg);

            if (MsgRet == true) {

                //呼び出すメソッドの判定
                if (functype == 1) {   //CarSalesInfoの場合

                    GetSalesCarInfoForSalesOrder('Vin', 'SalesCarNumber', param, '1', null, null, null, null, linecount);       ///Mod 2022/01/13 yano #4123
                }
                else {

                    GetSalesCarMasterDetailFromCode('SalesCarNumber', param, 'SalesCar', '1', null, null, linecount);   //Mod 2022/01/13 yano #4123
                }
            }
            else {//オプション、金額を計算せずにブランド名のみ更新する

                //呼び出すメソッドの判定
                if (functype == 1) {   //CarSalesInfoの場合

                    GetSalesCarInfoForSalesOrder('Vin', 'SalesCarNumber', new Array('NewUsedType', 'MakerName', 'CarBrandName', 'CarName', 'CarGradeCode', 'CarGradeName', 'ModelName', 'ExteriorColorCode', 'ExteriorColorName', 'InteriorColorCode', 'InteriorColorName', 'SalesCarNumber','Mileage', 'MileageUnit'), '1', null, null, null, null, linecount, '2');
                }
                else {

                    GetSalesCarMasterDetailFromCode('SalesCarNumber', new Array('NewUsedType', 'MakerName', 'CarBrandName', 'CarName', 'CarGradeCode', 'CarGradeName', 'ModelName', 'ExteriorColorCode', 'ExteriorColorName', 'InteriorColorCode', 'InteriorColorName', 'SalesCarNumber', 'Mileage', 'MileageUnit'), 'SalesCar', '1', null, null, linecount, '2');
                }
            }
        }
        else {

            //呼び出すメソッドの判定
            if (functype == 1) {   //CarSalesInfoの場合

                GetSalesCarInfoForSalesOrder('Vin', 'SalesCarNumber', param, '1', null, null, null, null, linecount);       ///Mod 2022/01/13 yano #4123
            }
            else {

                GetSalesCarMasterDetailFromCode('SalesCarNumber', param, 'SalesCar', '1', null, null, linecount);   //Mod 2022/01/13 yano #4123
            }
        }
    }
    else if (vin == '') {

        param.push('SalesTax', 'SalesPriceWithTax');
        GetSalesCarInfoForSalesOrder('Vin', 'SalesCarNumber', param, '1', null, null, null, null, linecount);
        calcTotalAmount();
    }

}

//-----------------------------------------------------------------------------
//機能  ：車両管理の締状況を取得する
//作成日：2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
//更新日：
//-----------------------------------------------------------------------------
function getCloseMonthControlCarStock(strTargetMonth) {

    var subject = document.getElementById('CloseMonthCarStock');

    if (strTargetMonth == null || strTargetMonth == '') {
        subject.innerText = '';
        setbuttonAttributeCarStock('');
        return false;
    }

    var processDate = strTargetMonth.replace('/', '') + '01';
    
    $.ajax({
        type: "POST",
        url: "/CarStock/GetCloseMonthCarStock",
        data: { processDate: processDate },
        contentType: "application/x-www-form-urlencoded",
        dataType: "json",
        timeout: 10000,
        success: function (data, dataType) {
            if (data.Code == null) {
                
                setbuttonAttributeCarStock('');

                return false;
            }
            else {
                //コードをセット
                subject.innerText = data.Name;

                setbuttonAttributeCarStock(data.Code);

                return true;
            }
        }
        ,
        error: function () {  //通信失敗時
            alert("車両管理締め状況の取得に失敗しました。");
            subject.innerText = '';

            setbuttonAttributeCarStock('');
         
            return false;
        }
    });
}


//-----------------------------------------------------------------------------
//機能  ：車両管理画面のボタンの制御
//作成日：2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
//更新日：
//-----------------------------------------------------------------------------
function setbuttonAttributeCarStock(status) {

    switch (status) {
        //未締
        case '001':
        case '999':
            //車両一括取込ボタン
            document.getElementById('carStockImportButton').disabled = false;
            //車両管理締め処理ボタン
            document.getElementById('closeCarStockButton').disabled = false;
            //車両管理締め解除ボタン
            document.getElementById('releaseCarStockButton').disabled = true;
            //検索ボタン
            document.getElementById('ciriteriaButton').disabled = false;
            //車両管理表出力ボタン
            document.getElementById('carstockOutButton').disabled = false;

            break;
        //締済
        case '002':
            //車両一括取込ボタン
            document.getElementById('carStockImportButton').disabled = true;
            //車両管理締め処理ボタン
            document.getElementById('closeCarStockButton').disabled = true;
            //車両管理締め解除ボタン
            document.getElementById('releaseCarStockButton').disabled = false;
            //検索ボタン
            document.getElementById('ciriteriaButton').disabled = false;
            //車両管理表出力ボタン
            document.getElementById('carstockOutButton').disabled = false;
            break;
        //その他
        default:
            //車両一括取込ボタン
            document.getElementById('carStockImportButton').disabled = true;
            //車両管理締め処理ボタン
            document.getElementById('closeCarStockButton').disabled = true;
            //車両管理締め解除ボタン
            document.getElementById('releaseCarStockButton').disabled = true;
            //検索ボタン
            document.getElementById('ciriteriaButton').disabled = true;
            //車両管理表出力ボタン
            document.getElementById('carstockOutButton').disabled = true;
            break;
    }
}

//-----------------------------------------------------------------------------
//機能  ：日付のチェック
//作成日：2018/08/28 yano #3922 車両管理表(タマ表)　機能改善②
//更新日：
//-----------------------------------------------------------------------------
function checkTargetDate(obj) {

    //書式チェック
    if (obj.value != '' && !obj.value.match(/^\d{4}\/\d{2}$/)) {
        alert('年月をYYYY/MM形式で入力してください。');
        obj.value = '';
        setbuttonAttributeCarStock('');
        return false;
    }
    

    return true;
}

//-----------------------------------------------------------------------------
//機能  ：主作業に対するデフォルト請求先の取得
//作成日：2019/02/06 yano #3959 サービス伝票入力　請求先誤り防止
//更新日：
//-----------------------------------------------------------------------------
function GetCustomerClaimByServiceWork(serviceworkCode, departmentCode, num) {

    //主作業コードがnullまたは空文字
    if (serviceworkCode == null || serviceworkCode == '') {
        //何もしない(初期化)もしない
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/ServiceSalesOrder/GetCustomerClaimByServiceWork",
            data: { code: serviceworkCode, departmentCode: departmentCode},
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                if (data == null) {

                    //何もしない

                } else {
                    //請求先が設定されている場合は
                    if (data.Code != null && data.Code != '') {
                        document.getElementById('line[' + num + ']_CustomerClaimCode').value = data.Code;
                        document.getElementById('line[' + num + ']_CustomerClaimName').value = data.Name;
                    }
                }

                calcTotalServiceAmount();
                return true
            }
               ,
            error: function () {  //通信失敗時
                alert("請求先の取得に失敗しました。");
                calcTotalServiceAmount();
                return false;
            }
        });
    }
}

//----------------------------------------------------------------------------------
//機能  ：ドロップダウンを選択状態の設定
//作成日：2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
//更新日：
//---------------------------------------------------------------------------------
function SetSelectedIndex(objname, value) {

    var selectobj = document.getElementById(objname);

    //ドロップダウンが存在する場合
    if (selectobj != 'undefined' && selectobj != null) {

        for (var m = 0; m < selectobj.childNodes.length; m = m + 2) {
            if (selectobj.childNodes[m].value == value) {
                selectobj.selectedIndex = selectobj.childNodes[m].index;
            }
        }

        selectobj.value = value;
    }
}

//----------------------------------------------------------------------------------
//機能  ：車両販売・諸費用に係るところの合計値の再計算
//作成日：
//      2019 / 09 / 04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
//更新日：
//      2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加
//---------------------------------------------------------------------------------
function calcCostAmountForCarSalesOrder() {

    //----------------------
    //税金合計
    //----------------------
    //自動車税種別割
    var CarTax = ConversionExceptComma($('#CarTax').val());
    //自動車重量税
    var CarWeightTax = ConversionExceptComma($('#CarWeightTax').val());
    //自賠責保険
    var CarLiabilityInsurance = ConversionExceptComma($('#CarLiabilityInsurance').val());
    //自動車税環境性能割
    var AcquisitionTax = ConversionExceptComma($('#AcquisitionTax').val());

    //Mod 2023/01/11 yano #4158
    ////任意保険料(新規・自社継続以外の場合のみ加算）
    //var VoluntaryInsuranceAmount = 0;
    //if ($('#VoluntaryInsuranceType').val() == '002' || $('#VoluntaryInsuranceType').val() == '003') {
    //    VoluntaryInsuranceAmount = ConversionExceptComma($('#VoluntaryInsuranceAmount').val());
    //    $('#VoluntaryInsuranceAmount2').val(ConversionWithComma(VoluntaryInsuranceAmount));
    //}

    //合計
    var TaxFreeTotalAmount = CarTax + CarWeightTax + CarLiabilityInsurance + AcquisitionTax;
    //var TaxFreeTotalAmount = CarTax + CarWeightTax + CarLiabilityInsurance + AcquisitionTax + VoluntaryInsuranceAmount;   //Mod 2023/01/11 yano #4158

    //税金の消費税
    var TaxFreeTotalTaxAmount = 0;

    $('#TaxFreeTotalAmount').val(ConversionWithComma(TaxFreeTotalAmount));
    $('#TaxFreeTotalAmountWithTax').val(ConversionWithComma(TaxFreeTotalAmount + TaxFreeTotalTaxAmount));

    //----------------------
    //諸費用計
    //----------------------
    var CostTotalAmount = ConversionExceptComma($('#SalesCostTotalAmount').val()) +
                          TaxFreeTotalAmount +
                          ConversionExceptComma($('#OtherCostTotalAmount').val());


    var CostTotalTaxAmount = ConversionExceptComma($('#SalesCostTotalTaxAmount').val()) +
                             TaxFreeTotalTaxAmount +
                             ConversionExceptComma($('#OtherCostTotalTaxAmount').val());

    $('#CostTotalAmount').val(ConversionWithComma(CostTotalAmount));
    $('#CostTotalTaxAmount').val(ConversionWithComma(CostTotalTaxAmount));
    $('#CostTotalAmountWithTax').val(ConversionWithComma(CostTotalAmount + CostTotalTaxAmount));

    //----------------------
    //現金販売合計
    //----------------------
    var Total = ConversionExceptComma($('#SubTotalAmount').val()) +
                CostTotalAmount +
                ConversionExceptComma($('#TotalTaxAmount').val());

    var TotalPrice = Total - ConversionExceptComma($('#TotalTaxAmount').val());;

    $('#GrandTotalAmount').val(ConversionWithComma(Total));
    $('#TotalPrice').val(ConversionWithComma(TotalPrice));

}

//----------------------------------------------------------------------------------------------------------
//機能  ：車両販売・諸費用に係るところの合計値の再計算
//作成日：2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
//更新日：
//        2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
//        2021/06/09 yano #4091【車両伝票】オプション行の区分追加(メンテナス・延長保証)
//----------------------------------------------------------------------------------------------------------
function calcTotalOptionAmount() {

    var ShopOptionAmount = 0;
    var MakerOptionAmount = 0;
    var ShopOptionTaxAmount = 0;
    var MakerOptionTaxAmount = 0;

    //Add 2021/06/09 #4091 yano
    var MaintenancePackageAmount = 0;
    var MaintenancePackageTaxAmount = 0;
    var ExtendedWarrantyAmount = 0;
    var ExtendedWarrantyTaxAmount = 0;

    //Add 2023/09/18 yano #4181
    var SurchargeAmount = 0;
    var SurchargeTaxAmount = 0;

    //車両オプションの各金額を計算
    for (var i = 0; i < 25; i++) {

        if (document.getElementById('line[' + i + ']_OptionType') == null) continue;

        var amount = ConversionExceptComma(document.getElementById('line[' + i + ']_Amount').value);
        var tax = ConversionExceptComma(document.getElementById('line[' + i + ']_TaxAmount').value);

        //Del 2019/09/12 ref コメントアウトされていた処理を削除 

        switch (document.getElementById('line[' + i + ']_OptionType').value) {

            //Mod 2023/09/18 yano #4181
            //case '001':
            //    ShopOptionAmount += amount;
            //    ShopOptionTaxAmount += tax;
            //    break;

            case '002'://メーカー
                MakerOptionAmount += amount;
                MakerOptionTaxAmount += tax;
                break;

            //Add 2021/06/09 yano #4091
            case '004': //メンテナンス
                ShopOptionAmount += amount;
                ShopOptionTaxAmount += tax;
                MaintenancePackageAmount += amount;
                MaintenancePackageTaxAmount += tax;
                break;

            case '005': //延長保証
                ShopOptionAmount += amount;
                ShopOptionTaxAmount += tax;
                ExtendedWarrantyAmount += amount;
                ExtendedWarrantyTaxAmount += tax;
                break;

            //Add 2023/09/18 yano #4181
            case '006': //サーチャージ
                ShopOptionAmount += amount;
                ShopOptionTaxAmount += tax;
                SurchargeAmount += amount;
                SurchargeTaxAmount += tax;
                break;

            //Add 2023/09/18 yano #4181
            default: //販売店(001)、暫定追加分
                ShopOptionAmount += amount;
                ShopOptionTaxAmount += tax;
                break;
        }
    }

    //オプション合計表示
    $('#OptionTotalAmount').val(ConversionWithComma(ShopOptionAmount + MakerOptionAmount));
    $('#OptionTotalTaxAmount').val(ConversionWithComma(ShopOptionTaxAmount + MakerOptionTaxAmount));
    $('#OptionTotalAmountWithTax').val(ConversionWithComma(ShopOptionAmount + MakerOptionAmount + ShopOptionTaxAmount + MakerOptionTaxAmount));

    //販売店オプション合計
    $('#ShopOptionAmount').val(ConversionWithComma(ShopOptionAmount));
    $('#ShopOptionTaxAmount').val(ConversionWithComma(ShopOptionTaxAmount));
    $('#ShopOptionAmountWithTax').val(ConversionWithComma(ShopOptionAmount + ShopOptionTaxAmount));

    //メーカーオプション合計
    $('#MakerOptionAmount').val(ConversionWithComma(MakerOptionAmount));
    $('#MakerOptionTaxAmount').val(ConversionWithComma(MakerOptionTaxAmount));
    $('#MakerOptionAmountWithTax').val(ConversionWithComma(MakerOptionAmount + MakerOptionTaxAmount));

    //Add 2021/06/09 yano #4091
    //サービス加入料（メンテナンス）合計
    $('#MaintenancePackageAmount').val(ConversionWithComma(MaintenancePackageAmount));
    $('#MaintenancePackageTaxAmount').val(ConversionWithComma(MaintenancePackageTaxAmount));

    //サービス加入料（延長保証）合計
    $('#ExtendedWarrantyAmount').val(ConversionWithComma(ExtendedWarrantyAmount));
    $('#ExtendedWarrantyTaxAmount').val(ConversionWithComma(ExtendedWarrantyTaxAmount));

    //Add 2023/09/18 yano #4181
    //特別サーチャージ合計
    $('#SurchargeAmount').val(ConversionWithComma(SurchargeAmount));
    $('#SurchargeTaxAmount').val(ConversionWithComma(SurchargeTaxAmount));
}


//------------------------------------------------------------------------------------------------------------
//機能：
//      手入力した車台番号に紐づく車両情報の取得
//  　  入力された車台番号をキーに有効な車両情報のみ取得するため、本来は一意で取得できるはずだが、
//      現状、車両情報(dbo.SalesCar)が整備されていないため、有効な車両情報が複数取得される場合がある。
//      このため、複数取れた場合はルックアップのウィンドウ表示し、そこからユーザに選択させるように対応。
//更新履歴：
//      2022/07/06 yano #4145 【サービス伝票】車台番号入力した際に顧客情報が表示されない不具合の対応
//      2022/01/08 yano #4121 【サービス伝票入力】Chrome・明細行の部品在庫情報取得の不具合対応
//      2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
//      2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 車両伝票入力用に新規作成
//------------------------------------------------------------------------------------------------------------
function GetSalesCarInfoForSalesOrder
    (
          vinField
        , salesCarNumField
        , nameFieldArray
        , selectByCarSlip
        , clearFieldeArray
        , lockObjectArray
        , controllerName
        , errmsgFlag
        , linecount
        , slipType      //伝票タイプ(2…サービス伝票、2以外…車両伝票   //Add 2022/01/08 yano #4121
        , func          //処理成功後に処理させたい関数        //Add 2022/07/06 yano #4145
        , clearfunc     //クリア時に処理させたい関数          //Add 2022/07/06 yano #4145
    ) {

    if (selectByCarSlip == null || selectByCarSlip == '') {
        selectByCarSlip = "0";
    }

    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
        for (i = 0; i < lockObjectArray.length; i++) {
            document.getElementById(lockObjectArray[i]).disabled = true;
        }
    }

    if (document.getElementById(vinField).value != null && document.getElementById(vinField).value == '') {
        resetEntity();
        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
            for (i = 0; i < lockObjectArray.length; i++) {
                document.getElementById(lockObjectArray[i]).disabled = false;
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/SalesCar/GetSalesCarNumberFromVin",
            data: { vinCode: document.getElementById(vinField).value },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {

                if (parseInt(data['count']) >= 2) {
                    // 同じ車台番号のレコードが2件以上ある場合、メッセージを表示し、検索ダイアログを表示させる
                    alert("同じ車台番号が2件以上存在します。検索ダイアログから指定して下さい。");

                    //Mod 2022/01/08 yano #4121
                    var callback = function () { if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100); }
                    openSearchDialog('salesCarNumField', 'Vin', '/SalesCar/CriteriaDialog?vin=' + document.getElementById(vinField).value, null, null, null, null, callback);
                    if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100);

                    //openSearchDialog('salesCarNumField', 'Vin', '/SalesCar/CriteriaDialog?vin=' + document.getElementById(vinField).value);
                    //// 100ミリ秒後、フォーカスを車台番号に持っていく
                    //if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100);

                } else if (parseInt(data['count']) == 0) {

                    //エラーメッセージを表示する場合
                    if (typeof (errmsgFlag) == 'undefined' || errmsgFlag == null || errmsgFlag == true) {

                        // レコードが0件の場合、メッセージを表示させる
                        alert("マスタに存在しません");
                        resetEntity();
                        document.getElementById(vinField).value = '';
                        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                            for (i = 0; i < lockObjectArray.length; i++) {
                                document.getElementById(lockObjectArray[i]).disabled = false;
                            }
                        }
                    }

                    return false;

                //Mod 2022/01/13 yano #4123
                //    //Add 2020/06/09 yano #4052------------------------------------------------
                //} else if (data['status'] == '') {

                //    alert("未仕入車両のため、選択できません。");
                //    resetEntity();
                //    document.getElementById(vinField).value = '';
                //    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                //        for (i = 0; i < lockObjectArray.length; i++) {
                //            document.getElementById(lockObjectArray[i]).disabled = false;
                //        }
                //    }

                //    return false;
                    //--------------------------------------------------------------------------
                } else {
                    // 車台番号が1件のみの場合、管理番号項目に値を設定
                    if (data['vin'] == null) {
                        document.getElementById(salesCarNumField).value = '';
                    } else {
                        document.getElementById(salesCarNumField).value = data['salesCarNumber'] == null ? '' : data['salesCarNumber'];
                    }
                }

                // 管理番号から車両情報を取得・表示させる
                if (controllerName == null) controllerName = 'SalesCar';
                GetSalesCarMasterDetailFromCode(salesCarNumField, nameFieldArray, controllerName, selectByCarSlip, lockObjectArray, null, linecount, slipType, func, clearfunc);//Mod 2022/07/06 yano #4145  //Mod 2022/01/13 yano #4123
                //GetSalesCarMasterDetailFromCode(salesCarNumField, nameFieldArray, controllerName, selectByCarSlip, lockObjectArray, null, linecount, slipType);

                return true;
            }
            ,
            error: function () {  //通信失敗時
                alert("車両情報の取得に失敗しました。");
                resetEntity();
                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                    for (i = 0; i < lockObjectArray.length; i++) {
                        document.getElementById(lockObjectArray[i]).disabled = false;
                    }
                }
                return false;
            }
        });
    }
    function resetEntity() {
        document.getElementById(salesCarNumField).value = '';

        var cleartargetArray;
        if (clearFieldeArray != null) {
            cleartargetArray = clearFieldeArray;
        }
        else {
            cleartargetArray = nameFieldArray;
        }

        for (var j = 0; j < cleartargetArray.length; j++) {
            try {

                var target = document.getElementById(cleartargetArray[j]);

                var tagName = target.tagName.toLowerCase();

                //テキストボックス
                if (tagName == 'input') {
                    target.value = '';
                }
                //リストボックス
                else if (tagName == 'select') {
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                //その他(ラベル)
                else {
                    target.innerText = '';
                }

            } catch (e) {
                target.innerText = '';
            }
        }

        //Add 2022/07/06 yano #4145
        //--------------------------
        // 設定されている場合
        //--------------------------
        if (typeof (clearfunc) != 'undefined' && typeof (clearfunc) != null && clearfunc != null) {
            clearfunc(nameFieldArray);
        }
    }
}

////------------------------------------------------------------------------------------------------------------
////機能：
////      手入力した車台番号に紐づく車両情報の取得
////  　  入力された車台番号をキーに有効な車両情報のみ取得するため、本来は一意で取得できるはずだが、
////      現状、車両情報(dbo.SalesCar)が整備されていないため、有効な車両情報が複数取得される場合がある。
////      このため、複数取れた場合はルックアップのウィンドウ表示し、そこからユーザに選択させるように対応。
////更新履歴：
////      2022/01/08 yano #4121 【サービス伝票入力】Chrome・明細行の部品在庫情報取得の不具合対応
////      2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
////      2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 車両伝票入力用に新規作成
////------------------------------------------------------------------------------------------------------------
//function GetSalesCarInfoForCarSalesOrder(vinField, salesCarNumField, nameFieldArray, selectByCarSlip, clearFieldeArray, lockObjectArray, controllerName, errmsgFlag, linecount) {

//    if (selectByCarSlip == null || selectByCarSlip == '') {
//        selectByCarSlip = "0";
//    }

//    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//        for (i = 0; i < lockObjectArray.length; i++) {
//            document.getElementById(lockObjectArray[i]).disabled = true;
//        }
//    }

//    if (document.getElementById(vinField).value != null && document.getElementById(vinField).value == '') {
//        resetEntity();
//        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//            for (i = 0; i < lockObjectArray.length; i++) {
//                document.getElementById(lockObjectArray[i]).disabled = false;
//            }
//        }
//        return false;
//    } else {
//        $.ajax({
//            type: "POST",
//            url: "/SalesCar/GetSalesCarNumberFromVin",
//            data: { vinCode: document.getElementById(vinField).value },
//            contentType: "application/x-www-form-urlencoded",
//            dataType: "json",
//            timeout: 10000,
//            success: function (data, dataType) {

//                if (parseInt(data['count']) >= 2) {
//                    // 同じ車台番号のレコードが2件以上ある場合、メッセージを表示し、検索ダイアログを表示させる
//                    alert("同じ車台番号が2件以上存在します。検索ダイアログから指定して下さい。");

//                    //Mod 2022/01/08 yano #4121
//                    var callback = function () { if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100); }
//                    openSearchDialog('salesCarNumField', 'Vin', '/SalesCar/CriteriaDialog?vin=' + document.getElementById(vinField).value, null, null, null, null, callback);
//                    if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100);
                   
//                    //openSearchDialog('salesCarNumField', 'Vin', '/SalesCar/CriteriaDialog?vin=' + document.getElementById(vinField).value);
//                    //// 100ミリ秒後、フォーカスを車台番号に持っていく
//                    //if (document.getElementById(salesCarNumField)) setTimeout(function () { document.getElementById(salesCarNumField).focus(); if (document.getElementById(vinField)) document.getElementById(vinField).focus() }, 100);

//                } else if (parseInt(data['count']) == 0) {

//                    //エラーメッセージを表示する場合
//                    if (typeof (errmsgFlag) == 'undefined' || errmsgFlag == null || errmsgFlag == true) {

//                        // レコードが0件の場合、メッセージを表示させる
//                        alert("マスタに存在しません");
//                        resetEntity();
//                        document.getElementById(vinField).value = '';
//                        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                            for (i = 0; i < lockObjectArray.length; i++) {
//                                document.getElementById(lockObjectArray[i]).disabled = false;
//                            }
//                        }
//                    }

//                    return false;

//                    //Add 2020/06/09 yano #4052------------------------------------------------
//                } else if (data['status'] == '') {  

//                    alert("未仕入車両のため、選択できません。");
//                    resetEntity();
//                    document.getElementById(vinField).value = '';
//                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                        for (i = 0; i < lockObjectArray.length; i++) {
//                            document.getElementById(lockObjectArray[i]).disabled = false;
//                        }
//                    }

//                    return false;
//                    //--------------------------------------------------------------------------
//                } else {
//                    // 車台番号が1件のみの場合、管理番号項目に値を設定
//                    if (data['vin'] == null) {
//                        document.getElementById(salesCarNumField).value = '';
//                    } else {
//                        document.getElementById(salesCarNumField).value = data['salesCarNumber'] == null ? '' : data['salesCarNumber'];
//                    }
//                }

//                // 管理番号から車両情報を取得・表示させる
//                if (controllerName == null) controllerName = 'SalesCar';
//                GetSalesCarMasterDetailFromCode(salesCarNumField, nameFieldArray, controllerName, selectByCarSlip, lockObjectArray, null, linecount);   //Mod 2022/01/13 yano #4123
//                //GetMasterDetailFromCodeForCarSalesOrder(salesCarNumField, nameFieldArray, controllerName, selectByCarSlip, lockObjectArray, null, linecount);

//                return true;
//            }
//            ,
//            error: function () {  //通信失敗時
//                alert("車両情報の取得に失敗しました。");
//                resetEntity();
//                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                    for (i = 0; i < lockObjectArray.length; i++) {
//                        document.getElementById(lockObjectArray[i]).disabled = false;
//                    }
//                }
//                return false;
//            }
//        });
//    }
//    function resetEntity() {
//        document.getElementById(salesCarNumField).value = '';

//        var cleartargetArray;
//        if (clearFieldeArray != null) {
//            cleartargetArray = clearFieldeArray;
//        }
//        else {
//            cleartargetArray = nameFieldArray;
//        }

//        for (var j = 0; j < cleartargetArray.length; j++) {
//            try {

//                var target = document.getElementById(cleartargetArray[j]);

//                var tagName = target.tagName.toLowerCase();

//                //テキストボックス
//                if (tagName == 'input') {
//                    target.value = '';
//                }
//                    //リストボックス
//                else if (tagName == 'select') {
//                    var option = target.getElementsByTagName('option');
//                    for (k = 0; k < option.length; k++) {
//                        if (k == 0) {
//                            option[k].selected = true;
//                        }
//                        else {
//                            option[k].selected = false;
//                        }
//                    }
//                }
//                    //その他(ラベル)
//                else {
//                    target.innerText = '';
//                }

//            } catch (e) {
//                target.innerText = '';
//            }
//        }
//    }
//}


//------------------------------------------------------------------------------------------------------------
//機能：
//      入力された車台番号を元にして、それに紐づく車両情報を設定
//更新履歴：
//      2022/07/06 yano #4145 【サービス伝票】車台番号入力した際に顧客情報が表示されない不具合の対応
//      2022/01/13 yano #4123 【サービス伝票入力】未仕入の車両が選択できる不具合の対応
//      2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
//      2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 車両伝票入力用に更新
//------------------------------------------------------------------------------------------------------------
function GetSalesCarMasterDetailFromCode(
    codeField,                  //マスタ検索時のキー項目
    nameFieldArray,             //取得したマスタ情報をセットする項目　※複数設定可
    controllerName,             //検索先マスタ
    SelectByCarSlip,            //絞込み条件(車両マスタの場合は、在庫ステータス)
    lockObjectArray,            //マスタ取得失敗等に読取専用にする項目 ※複数設定可
    NotMaster,                  //マスタに存在していなくても何もしない(メッセージを出さず、内容も空にしない)
    linecount,                  //オプション行数
    slipType,                   //伝票の種類(2…サービス伝票、2以外…車両伝票)  //Add 2019/09/04 yano #4011
    func,                       //データ取得後に処理させたい関数               //Add 2022/07/06 yano #4145 
    clearfunc                   //データクリア時に処理させたい関数             //Add 2022/07/06 yano #4145 

) {

    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
        for (i = 0; i < lockObjectArray.length; i++) {
            document.getElementById(lockObjectArray[i]).disabled = true;
        }
    }

    var isTaxRecalc = false;
    if (SelectByCarSlip == null || SelectByCarSlip == '') {
        SelectByCarSlip = "0";
    }
    if (NotMaster == null || NotMaster == '') {
        NotMaster = "0";
    }
    if (document.getElementById(codeField).value != null && document.getElementById(codeField).value == '') {
        resetEntity();
        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
            for (i = 0; i < lockObjectArray.length; i++) {
                document.getElementById(lockObjectArray[i]).disabled = false;
            }
        }
        return false;
    } else {
        $.ajax({
            type: "POST",
            url: "/" + controllerName + "/GetMasterDetail",
            data: { code: document.getElementById(codeField).value, SelectByCarSlip: SelectByCarSlip },
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            timeout: 10000,
            success: function (data, dataType) {
                var srvCodeName = (codeField.indexOf("_") == -1 ? codeField : codeField.substring(codeField.indexOf("_") + 1));
                srvCodeName = (srvCodeName.indexOf(".") == -1 ? srvCodeName : srvCodeName.substring(srvCodeName.indexOf(".") + 1));
                if (data[srvCodeName] == null) {
                    if (NotMaster != "1") {
                        alert("マスタに存在しません");
                        resetEntity();
                    }
                    document.getElementById(codeField).focus();
                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                        for (i = 0; i < lockObjectArray.length; i++) {
                            document.getElementById(lockObjectArray[i]).disabled = false;
                        }
                    }
                    return false;
                    //Add 2020/06/09 yano #4052------------------------------------------------
                } else if ((slipType == '2' && data['CarStatus'] == '999') || (slipType != '2' && (data['CarStatus'] == '999' || data['CarStatus'] == ''))) { //Mod 2022/01/13 yano #4123
                //} else if (data['CarStatus'] == '') {
                    alert("未仕入車両のため、選択できません。");

                    resetEntity();

                    document.getElementById(codeField).focus();
                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                        for (i = 0; i < lockObjectArray.length; i++) {
                            document.getElementById(lockObjectArray[i]).disabled = false;
                        }
                    }
                    return false;
                //--------------------------------------------------------------------------
                } else {
                    for (var i = 0; i < nameFieldArray.length; i++) {
                        var srvItemName = (nameFieldArray[i].indexOf("_") == -1 ? nameFieldArray[i] : nameFieldArray[i].substring(nameFieldArray[i].indexOf("_") + 1));
                        srvItemName = (srvItemName.indexOf(".") == -1 ? srvItemName : srvItemName.substring(srvItemName.indexOf(".") + 1));

                        var element = document.getElementById(nameFieldArray[i]);
                        if (element.tagName == "SELECT") {
                            for (var m = 0; m < element.childNodes.length; m = m + 2) {
                                if (element.childNodes[m].value == data[srvItemName]) {
                                    element.selectedIndex = element.childNodes[m].index;
                                    element.value = data[srvItemName]; //Add 2019/09/17 #4011
                                }
                            }
                        } else {
                            try {
                                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合

                                    if (element.className == 'money') { //金額欄なら
                                        element.value = data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]);
                                    }
                                    else {
                                        element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
                                    }
                                }
                                else {

                                    if (element.className == 'money') { //金額欄なら
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]));
                                    }
                                    else {
                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : data[srvItemName]);
                                    }
                                }
                            } catch (e) {

                                if (element.className == 'money') { //金額欄なら
                                    element.value = data[srvItemName] == null ? '' : ConversionWithComma(data[srvItemName]);
                                }
                                else {
                                    element.value = data[srvItemName] == null ? '' : data[srvItemName];
                                }
                            }
                        }
                    }

                    //Add 2022/01/13 yano #4123
                    if (slipType != '2') {  //伝票の種類がサービス伝票以外の場合
                        //車両本体価格の計算
                        calcSalesPrice(1)
                        GetRequiredOptionByVin('Vin', linecount);

                        GetCarTax();
                    }

                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                        for (i = 0; i < lockObjectArray.length; i++) {
                            document.getElementById(lockObjectArray[i]).disabled = false;
                        }
                    }

                    //Add 2022/07/06 yano #4145
                    //--------------------------
                    // 設定されている場合
                    //--------------------------
                    if (typeof (func) != 'undefined' && typeof (func) != null && func != null) {
                        func(nameFieldArray[i], data[srvItemName]);
                    }

                    return true;
                }
            }
            ,
            error: function () {  //通信失敗時
                alert("マスタ取得に失敗しました。");
                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
                    for (i = 0; i < lockObjectArray.length; i++) {
                        document.getElementById(lockObjectArray[i]).disabled = false;
                    }
                }
                return false;
            }

        });
    }

    function resetEntity() {
        document.getElementById(codeField).value = '';
        for (var j = 0; j < nameFieldArray.length; j++) {
            try {

                var target = document.getElementById(nameFieldArray[j]);
                var tagName = target.tagName.toLowerCase();

                if (tagName == 'input') {   //テキストボックス
                    document.getElementById(nameFieldArray[j]).value = '';
                }
                else if (tagName == 'select') { //リストボックス   
                    var option = target.getElementsByTagName('option');
                    for (k = 0; k < option.length; k++) {
                        if (k == 0) {
                            option[k].selected = true;
                        }
                        else {
                            option[k].selected = false;
                        }
                    }
                }
                else {  //その他(ラベル)
                    document.getElementById(nameFieldArray[j]).innerText = '';
                }

            } catch (e) {
                document.getElementById(nameFieldArray[j]).innerText = '';
            }
        }

        //Add 2022/07/06 yano #4145
        //--------------------------
        // 設定されている場合
        //--------------------------
        if (typeof (clearfunc) != 'undefined' && typeof (clearfunc) != null && clearfunc != null) {
            clearfunc(nameFieldArray);
        }
    }
}


////------------------------------------------------------------------------------------------------------------
////機能：
////      入力された車台番号を元にして、それに紐づく車両情報を設定
////更新履歴：
////      2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
////      2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 車両伝票入力用に更新
////------------------------------------------------------------------------------------------------------------
//function GetMasterDetailFromCodeForCarSalesOrder(
//    codeField,                  //マスタ検索時のキー項目
//    nameFieldArray,             //取得したマスタ情報をセットする項目　※複数設定可
//    controllerName,             //検索先マスタ
//    SelectByCarSlip,            //絞込み条件(車両マスタの場合は、在庫ステータス)
//    lockObjectArray,            //マスタ取得失敗等に読取専用にする項目 ※複数設定可
//    NotMaster,                  //マスタに存在していなくても何もしない(メッセージを出さず、内容も空にしない)
//    linecount                   //オプション行数
//    ) {

//    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//        for (i = 0; i < lockObjectArray.length; i++) {
//            document.getElementById(lockObjectArray[i]).disabled = true;
//        }
//    }

//    var isTaxRecalc = false;
//    if (SelectByCarSlip == null || SelectByCarSlip == '') {
//        SelectByCarSlip = "0";
//    }
//    if (NotMaster == null || NotMaster == '') {
//        NotMaster = "0";
//    }
//    if (document.getElementById(codeField).value != null && document.getElementById(codeField).value == '') {
//        resetEntity();
//        if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//            for (i = 0; i < lockObjectArray.length; i++) {
//                document.getElementById(lockObjectArray[i]).disabled = false;
//            }
//        }
//        return false;
//    } else {
//        $.ajax({
//            type: "POST",
//            url: "/" + controllerName + "/GetMasterDetail",
//            data: { code: document.getElementById(codeField).value, SelectByCarSlip: SelectByCarSlip },
//            contentType: "application/x-www-form-urlencoded",
//            dataType: "json",
//            timeout: 10000,
//            success: function (data, dataType) {
//                var srvCodeName = (codeField.indexOf("_") == -1 ? codeField : codeField.substring(codeField.indexOf("_") + 1));
//                srvCodeName = (srvCodeName.indexOf(".") == -1 ? srvCodeName : srvCodeName.substring(srvCodeName.indexOf(".") + 1));
//                if (data[srvCodeName] == null) {
//                    if (NotMaster != "1") {
//                        alert("マスタに存在しません");
//                        resetEntity();
//                    }
//                    document.getElementById(codeField).focus();
//                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                        for (i = 0; i < lockObjectArray.length; i++) {
//                            document.getElementById(lockObjectArray[i]).disabled = false;
//                        }
//                    }
//                    return false;
//                //Add 2020/06/09 yano #4052------------------------------------------------
//                } else if (data['CarStatus'] == '') {
//                    alert("未仕入車両のため、選択できません。");

//                    resetEntity();

//                    document.getElementById(codeField).focus();
//                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                        for (i = 0; i < lockObjectArray.length; i++) {
//                            document.getElementById(lockObjectArray[i]).disabled = false;
//                        }
//                    }
//                    return false;
//               //--------------------------------------------------------------------------
//                } else {
//                    for (var i = 0; i < nameFieldArray.length; i++) {
//                        var srvItemName = (nameFieldArray[i].indexOf("_") == -1 ? nameFieldArray[i] : nameFieldArray[i].substring(nameFieldArray[i].indexOf("_") + 1));
//                        srvItemName = (srvItemName.indexOf(".") == -1 ? srvItemName : srvItemName.substring(srvItemName.indexOf(".") + 1));
                       
//                        var element = document.getElementById(nameFieldArray[i]);
//                        if (element.tagName == "SELECT") {
//                            for (var m = 0; m < element.childNodes.length; m = m + 2) {
//                                if (element.childNodes[m].value == data[srvItemName]) {
//                                    element.selectedIndex = element.childNodes[m].index;
//                                    element.value = data[srvItemName]; //Add 2019/09/17 #4011
//                                }
//                            }
//                        } else {
//                            try {
//                                if ((element.tagName == 'INPUT') || (element.tagName == 'input')) {//input項目の場合

//                                    if (element.className == 'money') { //金額欄なら
//                                        element.value = data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]);
//                                    }
//                                    else {
//                                        element.value = data[srvItemName] == undefined ? '' : data[srvItemName];
//                                    }
//                                }
//                                else {

//                                    if (element.className == 'money') { //金額欄なら
//                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : ConversionWithComma(data[srvItemName]));
//                                    }
//                                    else {
//                                        element.innerHTML = escapeHtml(data[srvItemName] == undefined ? '' : data[srvItemName]);
//                                    }
//                                }
//                            } catch (e) {

//                                if (element.className == 'money') { //金額欄なら
//                                    element.value = data[srvItemName] == null ? '' : ConversionWithComma(data[srvItemName]);
//                                }
//                                else {
//                                    element.value = data[srvItemName] == null ? '' : data[srvItemName];
//                                }
//                            }
//                        }            
//                    }

//                    //車両本体価格の計算
//                    calcSalesPrice(1)
//                    GetRequiredOptionByVin('Vin', linecount);

//                    GetCarTax();

//                    if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                        for (i = 0; i < lockObjectArray.length; i++) {
//                            document.getElementById(lockObjectArray[i]).disabled = false;
//                        }
//                    }

//                    return true;
//                }
//            }
//            ,
//            error: function () {  //通信失敗時
//                alert("マスタ取得に失敗しました。");
//                if ((lockObjectArray != undefined) && (lockObjectArray != null)) {
//                    for (i = 0; i < lockObjectArray.length; i++) {
//                        document.getElementById(lockObjectArray[i]).disabled = false;
//                    }
//                }
//                return false;
//            }

//        });
//    }
    
//    function resetEntity() {
//        document.getElementById(codeField).value = '';
//        for (var j = 0; j < nameFieldArray.length; j++) {
//            try {

//                var target = document.getElementById(nameFieldArray[j]);
//                var tagName = target.tagName.toLowerCase();

//                if (tagName == 'input') {   //テキストボックス
//                    document.getElementById(nameFieldArray[j]).value = '';
//                }
//                else if (tagName == 'select') { //リストボックス   
//                    var option = target.getElementsByTagName('option');
//                    for (k = 0; k < option.length; k++) {
//                        if (k == 0) {
//                            option[k].selected = true;
//                        }
//                        else {
//                            option[k].selected = false;
//                        }
//                    }
//                }
//                else {  //その他(ラベル)
//                    document.getElementById(nameFieldArray[j]).innerText = '';
//                }

//            } catch (e) {
//                document.getElementById(nameFieldArray[j]).innerText = '';
//            }
//        }
//    }
//}

//--------------------------------------------------------------------------------------------
// 機能：テキストボックスの読み取り専用に設定
//       
// 作成：2020/01/06 yano #4029
// 履歴：
//
//--------------------------------------------------------------------------------------------
function setReadOnly(objid, boolreadonly, classname) {

    var obj = document.getElementById(objid);

    //入力項目の場合
    if (obj.tagName.toUpperCase() == 'INPUT') {
        obj.readOnly = boolreadonly;
        obj.className = classname;
    }
    else {
        alert('テキスト項目ではありません');
        return falase;
    }

    return true;

}


//--------------------------------------------------------------------------------------------
// 機能：イベント追加
//       
// 作成：2021/05/07 yano #4045 Chrome対応
// 履歴：
//
//--------------------------------------------------------------------------------------------
function addEvent(element, event, listener) {

    if (element.attachEvent) {
        //IE
        element.attachEvent('on' + event, listener);
    }
    else if (element.addEventListener) {
        //IE以外
        element.addEventListener(event, listener, false);
    }
    else {
        alert('対応外ブラウザです')
    }
}

//--------------------------------------------------------------------------------------------
// 機能：イベント発火
//       
// 作成：2021/05/07 yano #4045 Chrome対応
// 履歴：
//
//--------------------------------------------------------------------------------------------
function triggerEvent(element, event) {
    if (document.createEvent) {
        // IE以外
        var evt = document.createEvent("HTMLEvents");
        evt.initEvent(event, true, true);
        return element.dispatchEvent(evt);
    } else {
        // IE
        var evt = document.createEventObject();
        return element.fireEvent("on" + event, evt);
    }
}

//--------------------------------------------------------------------------------------------
// 機能：属性設定
//       
// 作成：2021/05/07 yano #4045 Chrome対応
// 履歴：2021/10/19 yano #4105 Chromeでの全角入力制限機能不全項目の対応
//
//--------------------------------------------------------------------------------------------
function setAttribute() {

    //数値項目の入力チェックの追加
    //フォーカスを失ったとき
    $('.numeric').blur(function () {
        //カンマ挿入
        textCheckForNum(this);
    }); 

    //金額項目の入力チェックの追加
    //フォーカスを失ったとき
    $('.money').blur(function () {

        var ret = textCheckForNum(this);

        //数値が入っていた場合
        if (ret) {
            //カンマ挿入
            InsertComma(this);
        }
    });

    //Add 2021/10/19 yano #4105
    $('.alphanumeric').blur(function () {
        //カンマ挿入
        convFulltoHalf(this);
    });
}

//--------------------------------------------------------------------------------------------
// 機能：入力チェック
//       
// 作成：2021/05/07 yano #4045 Chrome対応
// 履歴：2022/06/09 yano #4137【部品入荷入力】入荷金額計算処理の不具合の対応
//
//--------------------------------------------------------------------------------------------
function textCheckForNum($this) {
    var warning = "";
    var str = $this.value;

    var ret = true;

    // 文頭から文末まで全て0-9かチェック
    if (str != null && str != '' && !str.match(/^[0-9- ０-９－, . ，．]+$/)) {

        // そうでなければ入力文字を空白に変換し、警告メッセージを表示
        str = '';
        warning = '半角数字のみ入力してください';

        if ($this.className.indexOf('input-validation-error') < 0) {
            $this.className = 'input-validation-error ' + $this.className;
        }

        //メッセージ表示
        splash(warning);

        ret = false;
    }
    else {

        //numericのテキストボックスの場合はさらに「,，」をチェック
        if ($this.className.indexOf('numeric') >= 0 && str.match(/[,，]/g)) {

            // そうでなければ入力文字を空白に変換し、警告メッセージを表示q
            str = '';

            warning = 'カンマは入力できません';

            if ($this.className.indexOf('input-validation-error') < 0) {
                $this.className = 'input-validation-error ' + $this.className;
            }

            //メッセージ表示
            splash(warning);

            ret = false;
        }
        else {
            //Mod 2022/06/08 yano #4137
            // 全角文字が含まれている場合
            if (str.match(/[０-９－，．]/)) {

            //全角だった場合は半角に変更する。
            str = str.replace(/[０-９－，．]/g, function (s) {
                return String.fromCharCode(s.charCodeAt(0) - 0xFEE0);
            });

                $this.value = str;

                //半角文字に修正後に、再度イベント発火
                triggerEvent($this, 'change');
                triggerEvent($this, 'blur');
            }

            $this.className = $this.className.replace('input-validation-error ', '');
        }
    }

    $this.value = str;

    return ret;
}

//--------------------------------------------------------------------------------------------
// 機能：全角→半角変換
//       
// 作成：2021/10/19 yano #4105【顧客マスタ・他】Chromeでの全角入力制限機能不全項目の対応
// 更新：2021/11/18 yano #4114【顧客マスタ入力】メールアドレス欄への記号入力の不具合対応
//--------------------------------------------------------------------------------------------
function convFulltoHalf($this) {
    var warning = "";
    var str = $this.value;

    var ret = true;

    //文頭から文末まで全て英数字、記号かをチェック
    if (str != null && str != '' && !str.match(/^[0-9-０-９－Ａ-Ｚａ-ｚ a-z A-Z, . ，．／ \/!！#＃$＄%％&＆*＊+＋?？^＾_＿{｛|｜}｝~～@＠]+$/)) {

        // そうでなければ入力文字を空白に変換し、警告メッセージを表示
        str = '';
        warning = '半角英数字、記号のみ入力してください';

        if ($this.className.indexOf('input-validation-error') < 0) {
            $this.className = 'input-validation-error ' + $this.className;
        }

        //メッセージ表示
        splash(warning);

        ret = false;
    }
    else {

        //全角だった場合は半角に変更する。
        str = str.replace(/[０-９－，．Ａ-Ｚａ-ｚ／！＃＄％＆＊＋？＾＿｛｜｝～＠]/g, function (s) {
            return String.fromCharCode(s.charCodeAt(0) - 0xFEE0);
        });

        var classname = $this.className;

        $this.className = $this.className.replace('input-validation-error ', '');
    }

    $this.value = str;

    return ret;
}

//------------------------------------------------------------------------------
// 機能：スプラッシュメッセージ表示
//       
// 作成：2021/05/07 yano #4045 Chrome対応
// 履歴：
//
//------------------------------------------------------------------------------
function splash(msg, custom_set) {

    //Default
    var set = {
        message_class: 'splashmsg default',
        fadein_sec: 0.1,
        wait_sec: 1.0,
        fadeout_sec: 1.5,
        opacity: 0.9,
        trans_in: 'ease-in',
        trans_out: 'ease-out',
        outer_style: 'top: 0px;left: 0px;position: fixed;z-index: 1000;width: 100%;height: 100%;',
        message_style: 'padding:0.5em;font-size:3em;color:white;background-color:gray; position: absolute;top: 50%; left: 50%;transform: translateY(-50%) translateX(-50%);-webkit-transform: translateY(-50%) translateX(-50%);',
        style_id: 'append_splash_msg_style',
        outer_id: 'append_splash_msg',
        message_id: 'append_splash_msg_inner',
        on_splash_vanished: null //callback function
    };

    //Override custom_set
    for (var key in custom_set) {
        if (custom_set.hasOwnProperty(key)) {
            set[key] = custom_set[key];
        }
    }

     ////Style
    //if (!document.getElementById(set.style_id)) {
    //    var style = document.createElement('style');
    //    style.id = set.style_id;
    //    style.innerHTML =
    //        "#" + set.outer_id + " { " + set.outer_style + " } " +
    //        "#" + set.outer_id + " > #" + set.message_id + " {opacity: 0;transition: opacity " + set.fadeout_sec + "s " + set.trans_out + ";-webkit-transition: opacity " + set.fadeout_sec + "s " + set.trans_out + ";} " +
    //        "#" + set.outer_id + ".show > #" + set.message_id + " {opacity: " + set.opacity + ";transition: opacity " + set.fadein_sec + "s " + set.trans_in + ";-webkit-transition: opacity " + set.fadein_sec + "s " + set.trans_in + ";}" +
    //        "#" + set.message_id + " { " + set.message_style + " } ";

    //    document.body.appendChild(style);
    //}

    //Element (Outer, Inner)
    if ((e = document.getElementById(set.outer_id))) { e.parentNode.removeChild(e); if (set.on_splash_vanished) set.on_splash_vanished(); }
    var splash = document.createElement('div');
    splash.id = set.outer_id;
    splash.onclick = function () {
        if ((e = document.getElementById(set.outer_id))) e.parentNode.removeChild(e);
        if (set.on_splash_vanished) set.on_splash_vanished();
    }
    splash.innerHTML = '<div id="' + set.message_id + '" class="' + set.message_class + '">' + msg + '</div>';
    document.body.appendChild(splash);

    //Timer
    setTimeout(function () { if (splash) splash.className = 'show'; }, 0);
    setTimeout(function () { if (splash) splash.className = ''; }, set.wait_sec * 1000);

    //setTimeout(function () { if (splash) splash.classList.add('show'); }, 0);
    //setTimeout(function () { if (splash) splash.classList.remove('show'); }, set.wait_sec * 1000);


    setTimeout(function () { if (splash && splash.parentNode) splash.parentNode.removeChild(splash); if (set.on_splash_vanished) set.on_splash_vanished(); }, (set.fadeout_sec + set.wait_sec) * 1000);
}