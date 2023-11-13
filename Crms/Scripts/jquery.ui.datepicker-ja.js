jQuery(function($){
	$.datepicker.regional['ja'] = {
		closeText: '閉じる',
		prevText: '&#x3c;前',
		nextText: '次&#x3e;',
		currentText: '今日',
		monthNames: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		monthNamesShort: ['1月','2月','3月','4月','5月','6月',
		'7月','8月','9月','10月','11月','12月'],
		dayNames: ['日曜日','月曜日','火曜日','水曜日','木曜日','金曜日','土曜日'],
		dayNamesShort: ['日','月','火','水','木','金','土'],
		dayNamesMin: ['日','月','火','水','木','金','土'],
		weekHeader: '週',
		dateFormat: 'yy/mm/dd',
		firstDay: 0,
		isRTL: false,
		showMonthAfterYear: true,
		yearSuffix: '年',
		showAnim: '',
		numberOfMonths : 2 };
	$.datepicker.setDefaults($.datepicker.regional['ja']);
});

//カレンダー表示設定
$(function() {
    var targetId = new Array(
        'ArrivalDateFrom',
        'ArrivalDateTo',
        'ArrivalPlanDate',
        //'Birthday',
        'CampaignStartDateFrom',
        'CampaignStartDateTo',
        'CampaignEndDateFrom',
        'CampaignEndDateTo',
        'CampaignStartDate',
        'CampaignEndDate',
        'CarLiabilityInsuranceSubmitDate',
        'CreateDateFrom',
        'CreateDateTo',
        'DepartureDate',
        'DepartureDateFrom',
        'DepartureDateTo',
        'DocumentPurchaseRequestDateFrom',
        'DocumentPurchaseRequestDateTo',
        'DocumentPurchaseDate',
        'DocumentPurchaseDateFrom',
        'DocumentPurchaseDateTo',
        'DocumentReceiptDate',
        'DocumentReceiptDateFrom',
        'DocumentReceiptDateTo',
        'ExpireDateFrom',
        'ExpireDateTo',
        'FirstReceiptionDate',
        'FirstReceiptionDateFrom',
        'FirstReceiptionDateTo',
        'InspectionDate',
        //'InspectionExpireDate',
        'InvoiceDateFrom',
        'InvoiceDateTo',
        'IssueDate',
        'JournalDate',
        'LastReceiptionDate',
        'LastReceiptionDateFrom',
        'LastReceiptionDateTo',
        'LastUpdateDate',
        //'NextInspectionDate',
        'NextInspectionDateFrom',
        'NextInspectionDateTo',
        'OwnershipReservationSubmitDate',
        'ParkingSpaceSubmitDate',
        'PayDueDate',
        'PaymentPlanDate',
        'ProductionDate',
        'ProxySubmitDate',
        'PublishStartDate',
        'PublishEndDate',
        'PurchaseDate',
        'PurchaseDateFrom',
        'PurchaseDateTo',
        'PurchasePlanDateFrom',
        'PurchasePlanDateTo',
        'PurchaseOrderDate',
        'PurchaseOrderDateFrom',
        'PurchaseOrderDateTo',
        'QuestionnarieEntryDate',
        'QuoteDate',
        'QuoteExpireDate',
        'QuoteDateFrom',
        'QuoteDateTo',
        'ReceiptionDate',
        'ReceiptPlanDateFrom',
        'ReceiptPlanDateTo',
        'RegistrationDate',
        'RegistrationDateFrom',
        'RegistrationDateTo',
        'RegistrationPlanDateFrom',
        'RegistrationPlanDateTo',
        'RequestRegistDate',
        'SalesDate',
        'SalesDateFrom',
        'SalesDateTo',
        'SalesStartDate',
        'SalesEndDate',
        'SalesOrderDateFrom',
        'SalesOrderDateTo',
        'SalesPlanDate',
        'SalesCar_RegistrationDate',
        'SalesCar_ExpireDate',
        'SalesCar_IssueDate',
        'SalesCar_InspectionDate',
        'SalesCar_NextInspectionDate',
        'SalesCar_ProductionDate',
        'SalesCar_SalesDate',
        'SealSubmitDate',
        'SlipDate',
        'TermFrom',
        'TermTo',
        'ProcessDate',   //Add 2014/08/15 arc yano IPO対応　車両管理画面の作成年月日を追加。
        'CarSalesDateFrom', //Add 2014/08/15 arc yano #3080(顧客検索機能の新設) カレンダー表示項目追加
        'CarSalesDateTo',   
        'ServiceSalesDateFrom',
        'InputInventoryWorkingDate',    //Add 2014/11/25 arc nakayama 部品棚卸作業日登録対応
        'ServiceSalesDateTo',
		'ChangeDate',     //Add 2014/11/10 arc yano 車両ステータス変更対応 振替日をカレンダー表示項目に追加
        'NextInspectionDate',    //Add 2014/12/17 arc nakayama 顧客DM対応　次回点検日
        'ExpireDate',             //Add 2014/12/17 arc nakayama 顧客DM対応　車検満了日
        'ArrivalPlanDateFrom',      //Add 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
        'ArrivalPlanDateTo',      //Add 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
        'TargetDateFrom',  // Add 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　受注日（対象日）をFrom～Toにする
        'TargetDateFromTo', // Add 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　受注日（対象日）をFrom～Toにする
        'TargetDateTo' ,// Add 2015/08/03 arc nakayama #3229_顧客データ抽出の不具合
        'JournalDateFrom', //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
        'JournalDateTo',    //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
        'DeliveryRequirementFrom', //Add 2017/02/21 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
        'DeliveryRequirementTo',//Add 2017/02/21 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
        'DeliveryRequirement',//Add 2017/03/07 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
        'SlipDateFrom',
        'SlipDateTo',
        'CancelDate', //ADD 2017/08/10 arc nakayama #3782_車両仕入_キャンセル機能追加
        'LinkEntryCaptureDateFrom',                             //Add 2018/03/26 arc yano #3863
        'LinkEntryCaptureDateTo',                                //Add 2018/03/26 arc yano #3863
        'CancelPurchaseDate',                                    //Add 2018/12/10 yano #3949
        'FromAvailableDate',                                    //Add 2019/10/21 yano #4023
        'ToAvailableDate'                                       //Add 2019/10/21 yano #4023
        


        //'VoluntaryInsuranceTermFrom',
        //'VoluntaryInsuranceTermTo'
        //new Array('pay', 'PaymentPlanDate', 'PaymentCount'),
        //new Array('line', 'JournalDate', 'JournalCount')
        );
    for (var i = 0; i < targetId.length; i++) {
        /*if (IsArray(targetId[i])) {
            var targetCount = document.getElementById(targetId[i][2]);
            if (targetCount) {
                for (var j = 0; j < targetCount.value; j++) {
                    if (document.getElementById(targetId[i][0] + '[' + j + '].' + targetId[i][1]) && !document.getElementById(targetId[i][0] + '[' + j + '].' + targetId[i][1]).readOnly) {
                        $('#' + targetId[i][0] + '[' + j + '].' + targetId[i][1]).datepicker();
                        $('#line[0].JournalDate').datepicker();
                    }
                }
            }
        } else {
        */
            if (document.getElementById(targetId[i]) && !document.getElementById(targetId[i]).readOnly) {
                $('#' + targetId[i]).datepicker();
            }
        //}
    }
});

function IsArray(array)
{
  return !(
    !array || 
    (!array.length || array.length == 0) || 
    typeof array !== 'object' || 
    !array.constructor || 
    array.nodeType || 
    array.item 
  );
}