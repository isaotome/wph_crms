//********************************
//　子画面サイズ定義
//********************************
function WindowSize(url) {
    url = url.split("?")[0];
    screenID = url.split("/")[1] + url.split("/")[2];
    switch (screenID) {
        case "CampaignEntry":
            //2014/07/03 #3051 画面サイズ変更 arc.ookubo
            //this.width = 935;
            //this.height = 550;
            this.width = 940;
            this.height = 580;
            break;
        case "CarAppraisalEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 720 → 685
            this.width = 1225;
            this.height = 685;
            break;
        case "CarGradeEntry":
            // Mod 2014/07/14 arc amii chrome対応 画面表示時、『中古車点検・整備費用及び保証継承整備費用」欄がIEと同じに表示されないのを修正
            // Mod 2015/02/03 arc iijima サイズ変更　height = 1000 → 685
            this.width = 850;
            this.height = 685;
            break;
        case "CarGradeCopy":
            this.width = 830;
            this.height = 1000;
            break;
        case "CarPurchaseEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 900 → 685
            this.width = 1200;
            this.height = 685;
            break;
        case "CarReceiptionEntry":
            this.width = 760;
            this.height = 895;
            break;
        case "CashJournalEntry":
            this.width = 445;
            this.height = 450;
            break;
        case "CustomerEntry":
            this.width = 730;
            this.height = 845;
            break;
        case "CustomerIntegrateEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 840 → 685
            this.width = 800;
            this.height = 685;
            break;
        case "DepartmentEntry":
            this.width = 730;
            this.height = 525;
            break;
        case "EmployeeEntry":
            this.width = 730;
            this.height = 650;
            break;
        case "InventoryEntry":
            this.width = 1050;
            this.height = 720;
            break;
        case "OfficeEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 710 → 685
            this.width = 730;
            this.height = 685;
            break;
        case "PartsEntry":
            // Mod 2014/11/06 arc nakayama 部品項目追加対応 
            this.width = 730;
            this.height = 648;
            break;
        case "PartsInventoryEntry":
            this.width = 935;
            this.height = 720;
            break;
        case "SalesCarEntry":
            // Mod 2014/07/11 arc amii chrome対応 画面表示時、横スクロールバーが出ないよう画面の幅を設定
            // Mod 2015/02/03 arc iijima サイズ変更　height = 768 → 685
            this.width = 1210;
            this.height = 685;
            break;
        case "OwnershipChangeEntry":
            this.width = 1200;
            this.height = 768;
            break;
        case "ServiceCostEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 800 → 685
            this.width = 1025;
            this.height = 685;
            break;
        case "ServiceReceiptionEntry":
            this.width = 800;
            this.height = 720;
            break;
        case "ServiceReceiptionHistory":
            this.width = 930;
            this.height = 780;
            break;
        case "AccountCriteriaDialog":
            this.width = 900;
            this.height = 600;
            break;
        case "ApplicationRoleEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 700 → 685
            this.width = 1000;
            this.height = 685;
            break;
        case "AreaCriteriaDialog":
            this.width = 800;
            this.height = 380;
            break;
        case "BrandCriteriaDialog":
            this.width = 800;
            this.height = 410;
            break;
        case "CarCriteriaDialog":
            this.width = 800;
            this.height = 410;
            break;
        case "CarClassCriteriaDialog":
            this.width = 800;
            this.height = 355;
            break;
        case "CarColorCriteriaDialog":
            this.width = 800;
            this.height = 440;
            break;
        case "CarGradeCriteriaDialog":
            this.width = 800;
            this.height = 515;
            break;
        case "CarOptionCriteriaDialog":
            this.width = 800;
            this.height = 435;
            break;
        case "CarPurchaseOrderListEntry":
            this.width = 1100;
            this.height = 950;
            break;
        case "CarPurchaseOrderEntry2":
            this.width = 500;
            this.height = 900;
            break;
        case "CarPurchaseOrderReservationEntry":
            this.width = 500;
            this.height = 600;
            break;
        case "CarPurchaseOrderRegistrationEntry":
            this.width = 500;
            this.height = 900;
            break;
        case "CarSalesOrderEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 980 → 685
            this.width = 1130;
            this.height = 685;
            break;
        case "CarSalesOrderCopy":   //Add 2017/11/08 arc yano #3553
            
            this.width = 1130;
            this.height = 685;
            break;
        case "CarSalesOrderConfirm":
            this.width = 1100;
            this.height = 950;
            break;
        case "CarTransferEntry":
            this.width = 650;
            this.height = 350;
            break;
        case "CustomerCriteriaDialog":
            this.width = 900;
            this.height = 670;
            break;
        case "CustomerClaimCriteriaDialog":
            this.width = 800;
            this.height = 355;
            break;
        case "CompanyCriteriaDialog":
            this.width = 800;
            this.height = 380;
            break;
        case "DepartmentCriteriaDialog":
            this.width = 800;
            this.height = 490;
            break;
        case "EmployeeCriteriaDialog":
            this.width = 800;
            this.height = 490;
            break;
        case "LocationCriteriaDialog":
            this.width = 800;
            this.height = 410;
            break;
        case "MakerCriteriaDialog":
            this.width = 800;
            this.height = 355;
            break;
        case "OfficeCriteriaDialog":
            this.width = 800;
            this.height = 435;
            break;
        case "PartsCriteriaDialog":
            this.width = 800;
            this.height = 610;
            break;
        case "PartsStockCriteriaDialog":
            this.width = 1000;
            this.height = 710;
            break;
        case "PartsTransferEntry":
            this.width = 650;
            this.height = 350;
            break;
        case "PartsTransferConfirm":
            this.width = 650;
            this.height = 350;
            break;
        case "PartsPurchaseOrderEntry":
            this.width = 730;
            this.height = 450;
            break;
        case "PartsPurchaseEntry":
            this.width = 1050;
            this.height = 685;
            break; 
        case "PaymentKindCriteriaDialog":
            this.width = 800;
            this.height = 355;
            break;
        case "SalesCarCriteriaDialog":
            this.width = 1070;
            this.height = 680;
            break;
        case "SecurityRoleEntry":
            this.width = 400;
            this.height = 400;
            break;
        case "ServiceSalesOrderEntry":
            // Mod 2015/02/03 arc iijima サイズ変更　height = 900 → 685
            this.width = 1050;
            this.height = 685;
            break;
        case "ServiceSalesOrderCopy":
            this.width = 1050;
            this.height = 900;
            break;
        case "ServiceSalesOrderCriteriaDialog":
            this.width = 900;
            this.height = 600;
            break;
        case "ServiceRequestEntry":
            // Mod 2014/07/11 arc amii chrome対応 aspxファイルに記述していたのを移動
            this.width = 940;
            this.height = 870;
            break;
        case "ShopDepositDetail":
            // Mod 2014/07/11 arc amii chrome対応 aspxファイルに記述していたのを移動
            this.width = 835;
            this.height = 600;
            break;
        case "TaskEntry":
            this.width = 500;
            this.height = 500;
            break;
        case "PartsImportDialog":
            //  Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
            this.width = 730;
            this.height = 200;
            break;
        case "BankEntry":
            //  Add 2014/11/06 arc ishii スクロールバー表示のため 新規追加
            this.width = 770;
            this.height = 520;
            break;
       case "CarUsageSettingEntry":
            //  Add 2014/10/30 arc amii 車両ステータス追加対応
            this.width = 1280;
            this.height = 960;
		break;
        case "RevenueResultDetail":
            //  Add 2015/02/04 arc yano #3153 入金実績リスト追加対応
            this.width = 1280;
            this.height = 780;
            break;
        case "CarUsageSettingHistory":
            //  Add 2015/02/16 arc yano 車両用途変更(デモカー一発) 操作履歴画面追加
            this.width = 800;
            this.height = 500;
            break;
        case "ServiceSalesSearchServiceSalesReportCriteriaDialog":
            // Add 2015/09/28 arc nakayama #3165_サービス集計表対応 サービス集計表のサイズ追加
            this.width = 1050;
            this.height = 685;
            break;
        case "PartsPurchaseOrderNonGenuineEntry":
            //2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 社外品用の部品発注画面追加
            this.width = 900;
            this.height = 300;
            break;
		case "PartsPurchaseImportDialog":
            // Add 2015/12/15 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
            this.width = 820;
            this.height = 700;
            break;
        case "CarPurchaseImportDialog":
            //Add 2016/12/07 arc nakayama #3663_【製造】車両仕入　Excel取込対応
            this.width = 1050;
            this.height = 685;
            break;
        case "ServiceSalesHistoryLineCriteria":    //Add 2017/03/13 arc yano #3725
            
            this.width = 900;
            this.height = 500;
			break;
        case "CarInventoryImportDialog":
            // Add 2017/12/18 arc yano #3838
            this.width = 1000;
            this.height = 700;
            break;
        case "InspectGuidListImportDialog":  //Add 2018/05/25 arc yano #3888
            this.width = 830;
            this.height = 200;
            break;
        case "ShopDepositImportDialog":     // Add 2018/05/28 arc yano #3886
            this.width = 730;
            this.height = 200;
            break;
        case "DepreciationRateEntry":            // Add 2018/06/06 arc yano #3883
            this.width = 730;
            this.height = 200;
            break;
        case "DepreciationRateImportDialog":   // Add 2018/06/06 arc yano #3883
            this.width = 730;
            this.height = 200;
            break;
        case "CarStockImportDialog":   // Add 2018/06/06 arc yano #3883
            this.width = 730;
            this.height = 200;
            break;
        case "CarStockImportDialogForCarStock":   // Add 2018/08/28 arc yano #3922
            this.width = 730;
            this.height = 200;
            break;
        default:
            this.width = 730;
            this.height = 500;
    }
}

//********************************
//　子画面戻り値
//********************************
function ReturnValue(ret, url) {
    this.ret = ret;
    this.url = url;
}