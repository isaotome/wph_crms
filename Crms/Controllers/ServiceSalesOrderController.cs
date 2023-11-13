using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Crms.Models;

using OfficeOpenXml;            //Add 2017/10/19 arc yano #3803
using System.Configuration;     //Add 2017/10/19 arc yano #3803

namespace Crms.Controllers
{
    /// <summary>
    /// サービス伝票
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceSalesOrderController : InheritedController {

	//Mod 2014/07/12 arc yano chrome対応 定数クラスの宣言位置をServiceSalesOrderController内へ移動
    //Add 2014/07/02 arc yano 定数定義

        /// <summary>
        ///定数
        /// </summary>     
       static class Constants
        {
            //主作業コード
            public const string svWkWaranty = "10501";      //ワランティ
            public const string svWkRecall = "10502";       //リコール

            //請求先コード
            public const string cClaimFCJWaranty = "A000100174";      //フィアット　クライスラー　ジャパンＦＣＪワランティ（月末締めＦＩＡＴ／ＡＬＦＡワランティ専用）
        }

        #region 初期化
        //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "サービス伝票";     // 画面名
        private static readonly string PROC_NAME_AKA = "赤伝"; // 処理名
        private static readonly string PROC_NAME_SAVE = "サービス伝票保存"; // 処理名
        private static readonly string PROC_NAME_AKA_KURO = "赤黒"; // 処理名

        private static readonly string PROC_NAME_PURCHASEORDER_DOWNLOAD = "発注書出力";  // 処理名      //Add 2017/10/19 arc yano #3803


        //Add 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        private static readonly string TYPE_GENUINE = "001";                //純正品
        private static readonly string TYPE_NONGENUINE = "002";            //社外品



        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 初回表示フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// 在庫処理サービス
        /// </summary>
        private IStockService stockService;

        /// <summary>
        /// サービス伝票処理サービス
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceSalesOrderController() {
            db = new CrmsLinqDataContext();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db); 
        }

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }
        #endregion

        #region コントロールの有効無効
        /// <summary>
        /// 伝票ステータスから表示するVIEWを返す
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/03/22 yano #4078【サービス伝票入力】納車確認書で出力する帳票の種類を動的に絞る
        /// 2017/01/21 arc yano #3657  見積書の個人情報の表示／非表示のチェックボックスの表示・非表示を設定 
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 締め処理状況チェック処理の引数の変更(SlipData.DepartmentCode → warehouseCode)
        /// </history>
        private ActionResult GetViewResult(ServiceSalesHeader header) {
            Employee loginUser = (Employee)Session["Employee"];

            string departmentCode = header.DepartmentCode;
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";

            header.SlipEnabled = false;
            header.CustomerEnabled = false;
            header.CarEnabled = false;
            header.LineEnabled = false;
            header.CostEnabled = false;
            header.PaymentEnabled = false;
            header.SalesDateEnabled = false;
            header.SalesPlanDateEnabled = false;　 //ADD 2014/02/20 ookubo
            header.RateEnabled = false;　          //ADD 2014/02/20 ookubo
            header.PInfoChekEnabled = false;       // Add 2017/01/21 arc yano #3657
            header.ClaimReportChecked = false   ;       //Add 2021/03/22 yano #4078

            if (ViewData["Mode"] != null && ViewData["Mode"].Equals("1"))
            {
                // 赤伝
                header.ShowMenuIndex = 10;
            } else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("2")) {
                // 赤黒
                header.SlipEnabled = true;
                header.CarEnabled = true;
                header.CustomerEnabled = true;
                header.LineEnabled = true;
                header.CostEnabled = true;
                header.PaymentEnabled = true;
                header.SalesDateEnabled = true;
                header.RateEnabled = true;              //ADD 2014/02/20 ookubo
                header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                header.ShowMenuIndex = 11;
            
                // 2012.03.21 変更
                // ログイン担当者の所属部門または兼務部門でなければ編集不可
                // ログイン担当者のセキュリティレベルが004：ALLなら編集可
            } else if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004")) {
                header.ShowMenuIndex = 8;
            } else if (header.DelFlag != null && header.DelFlag.Equals("1")) { 
                header.ShowMenuIndex = 7;
                header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
            } else if (!string.IsNullOrEmpty(header.LockEmployeeName)){
                header.SlipEnabled = false;
                header.CarEnabled = false;
                header.CustomerEnabled = false;
                header.LineEnabled = false;
                header.CostEnabled = false;
                header.PaymentEnabled = false;
                header.ShowMenuIndex = 8;
                ViewData["ProcessSessionError"] = "この伝票は現在" + header.LockEmployeeName + "さんが使用しているため読み取り専用で表示しています";
            } else {
                switch (header.ServiceOrderStatus) {
                    case "001": //見積
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;　 //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 1;
                        header.KeepsCarFlagEnabled = true;　  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "002": //受注
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;　 //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 2;
                        header.KeepsCarFlagEnabled = true;　  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "003": //作業中
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;　 //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 3;
                        header.KeepsCarFlagEnabled = true;　  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "004": //作業完了
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;　 //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 4;
                        header.KeepsCarFlagEnabled = true;　  //ADD 2014/11/25 #3135 ookubo
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        break;
                    case "005": //納車前
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = true;
                        header.SalesPlanDateEnabled = true;　 //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 5;
                        header.KeepsCarFlagEnabled = true;　  //ADD 2014/11/25 #3135 ookubo
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        break;
                    case "006": //納車
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        //変更前の伝票情報取得
                        ServiceSalesHeader SlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                        // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                        // Mod 2015/04/01 arc nakayama 納車日チェック対応 経理締が仮締め以上でも納車日が変更できないようにする。
                        // Mod 2015/04/15 arc yano　サービス系は締判定、部品棚卸判定両方行う。仮締中変更可能なユーザの場合、仮締めの場合でも、変更可能とする
                        int ret = 0;

                        //Mod 2016/08/13 arc yano #3596
                        //部門コードから使用倉庫を割出す
                        DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                        ret = CheckTempClosedEdit(dWarehouse, SlipData.SalesDate);

                        //ret = CheckTempClosedEdit(SlipData.DepartmentCode, SlipData.SalesDate);

                        //Mod 2015/05/07 arc yano 仮締中編集権限追加 仮締中変更可能なユーザの場合、仮締めの場合でも、変更可能とする
                        if (ret != 0) //月次締=仮締め以上
                        {
                            header.SalesDateEnabled = false;
                        }
                        else
                        {
                            header.SalesDateEnabled = true;
                        }

                        header.ShowMenuIndex = 6;
                        header.KeepsCarFlagEnabled = false;　 //ADD 2014/11/25 #3135 ookubo

                        // Add 2015/03/18 arc nakayama 伝票修正対応　過去に修正を行った事のある伝票だった場合、理由欄を表示する
                        if(service.CheckModifiedReason(header.SlipNumber)){
                            header.ModifiedReasonEnabled = true; 
                            service.GetModifiedHistory(header); //修正履歴取得
                        }else{
                            header.ModifiedReasonEnabled = false;
                        }

                        // Add 2015/03/17 arc nakayama 伝票修正対応　ログインユーザーが支店長・システム管理者　かつ　赤伝または過去に赤黒処理を行った元伝票でなかった場合は伝票修正ボタン表示
                        // ログインユーザーが伝票修正許可権限をもっている　かつ　赤伝または過去に赤黒処理を行った元伝票でなかった場合は伝票修正ボタン表示
                        if (service.CheckApplicationRole(loginUser.EmployeeCode) && !header.SlipNumber.Contains("-1") && service.AkakuroCheck(header.SlipNumber)){
                            header.ModificationEnabled = true;  //[伝票修正]ボタン表示
                        }else{
                            header.ModificationEnabled = false; //[伝票修正]ボタン非表示
                        }

                        // Add 2015/03/17 arc nakayama 修正中だった場合は修正完了ボタンと修正キャンセルボタンの表示を切り替える
                        if (service.CheckModification(header.SlipNumber, header.RevisionNumber)){
                            header.ModificationControl = true; //修正中
                            header.ModificationControlCancel = true; //[修正キャンセル]ボタン 表示

                            //修正中でも経理の締め処理状況が仮締めか本締めなら変更は不可にして[修正キャンセル]ボタンと[閉じる]ボタンのみの表示になる。理由欄も表示されない
                            // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                            //if (new InventoryScheduleDao(db).IsClosedInventoryMonth(SlipData.DepartmentCode, SlipData.SalesDate, "001"))
                            if (ret != 2)
                            {
                                header.SlipEnabled = true;
                                header.CarEnabled = true;
                                header.CustomerEnabled = true;
                                header.CostEnabled = true;
                                header.PaymentEnabled = true;
                                header.SalesDateEnabled = true;
                                
                                header.ModificationControlCommit = true;        // [修正完了]ボタン 表示
                                header.LineEnabled = true;                      //　明細行は変更可能
                                
                                //修正中でも部品の棚卸状況が確定なら明細の変更は不可にする
                                if (ret != 1)
                                {
                                    header.LineEnabled = true;  //明細行編集可
                                }
                                else
                                {
                                    header.LineEnabled = false; //明細行編集不可
                                }
                            }
                            else
                            {
                                header.SlipEnabled = false;
                                header.CarEnabled = false;
                                header.CustomerEnabled = false;
                                header.LineEnabled = false;
                                header.CostEnabled = false;
                                header.PaymentEnabled = false;
                                header.SalesDateEnabled = false;
                                header.ModificationControlCommit = false;// [修正完了]ボタン 非表示
                            }
                        }

                        break;
                    case "007": //キャンセル
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 8;
                        break;
                    case "009": //作業履歴
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 8;
                        break;
                    case "010": //作業中止
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 9;
                        break;
                }

                //管理者権限のみ消費税率使用可
                Employee emp = HttpContext.Session["Employee"] as Employee;
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
                {
                    header.RateEnabled = false;
                }

            }
            if (ViewData["EntryMode"] != null){
                if (ViewData["EntryMode"].Equals("FullScreen")) {
                    return View("ServiceSalesLineEntry", header);
                } else {
                    return View("ServiceSalesOrderEntry", header);
                }
            }

            return View("ServiceSalesOrderEntry", header);
        }
        #endregion

        #region 検索処理
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/14 yano #3912 サービス伝票検索　検索条件に入力したイベントの名称が検索実行後、消える
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            //form["EmployeeCode"] = (form["EmployeeCode"] == null ? ((Employee)Session["Employee"]).EmployeeCode : form["EmployeeCode"]);
            form["ServiceOrderStatus"] = Request["status"] == null ? form["ServiceOrderStatus"] : Request["status"];
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;

            PaginatedList<ServiceSalesHeader> list = GetSearchResultList(form);
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["QuoteDateFrom"] = form["QuoteDateFrom"];
            ViewData["QuoteDateTo"] = form["QuoteDateTo"];
            ViewData["SalesOrderDateFrom"] = form["SalesOrderDateFrom"];
            ViewData["SalesOrderDateTo"] = form["SalesOrderDateTo"];
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["PlateNumber"] = form["PlateNumber"];
            ViewData["Vin"] = form["Vin"];
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];

            //表示項目のセット
            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                ViewData["EmployeeName"] = (new EmployeeDao(db)).GetByKey(form["EmployeeCode"], true).EmployeeName;
            }
            if (!string.IsNullOrEmpty(form["ServiceWorkCode"])) {
                ViewData["ServiceWorkName"] = (new ServiceWorkDao(db)).GetByKey(form["ServiceWorkCode"]).Name;
            }
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                ViewData["DepartmentName"] = (new DepartmentDao(db)).GetByKey(form["DepartmentCode"], true).DepartmentName;
            }
            //Add 2018/08/14 yano #3912　//イベント名の設定
            if (!string.IsNullOrEmpty(form["CampaignCode"]))
            {
                ViewData["CampaignName"] = (new CampaignDao(db)).GetByKey(form["CampaignCode"]).CampaignName;
            }

            CodeDao dao = new CodeDao(db);
            ViewData["ServiceOrderStatusList"] = CodeUtils.GetSelectListByModel<c_ServiceOrderStatus>(dao.GetServiceOrderStatusAll(false), form["ServiceOrderStatus"], true);

            return View("ServiceSalesOrderCriteria",list);
        }

        /// <summary>
        /// サービス伝票検索ダイアログ
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(FormCollection form) {
            if (string.IsNullOrEmpty(form["CustomerCode"])) {
                form["CustomerCode"] = Request["customerCode"];
            }
            if (string.IsNullOrEmpty(form["VinFull"])) {
                form["VinFull"] = Request["vin"];
            }
            form["DelFlag"] = "0";
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["VinFull"] = form["VinFull"];
            ViewData["DelFlag"] = form["DelFlag"];
            PaginatedList<ServiceSalesHeader> list = GetSearchResultList(form);
            return View("ServiceSalesOrderCriteriaDialog", list);
        }
        /// <summary>
        /// サービス伝票検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービス伝票検索結果リスト</returns>
        /// <history>
        /// 2018/08/14 yano #3912 サービス伝票検索　検索条件に入力したイベントの名称が検索実行後、消える
        /// </history>
        private PaginatedList<ServiceSalesHeader> GetSearchResultList(FormCollection form) {
            ServiceSalesOrderDao serviceSalesOrderDao = new ServiceSalesOrderDao(db);
            ServiceSalesHeader salesHeaderCondition = new ServiceSalesHeader();
            salesHeaderCondition.FrontEmployee = new Employee();
            salesHeaderCondition.FrontEmployee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.ReceiptionEmployee = new Employee();
            salesHeaderCondition.ReceiptionEmployee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.Customer = new Customer();
            salesHeaderCondition.Customer.CustomerCode = form["CustomerCode"];
            salesHeaderCondition.Customer.CustomerName = form["CustomerName"];
            salesHeaderCondition.Customer.CustomerNameKana = form["CustomerNameKana"];
            salesHeaderCondition.Customer.TelNumber = form["TelNumber"];
            salesHeaderCondition.SlipNumber = form["SlipNumber"];
            salesHeaderCondition.CampaignCode1 = form["CampaignCode"];  //Mod 2018/08/14 yano #3912
            salesHeaderCondition.CampaignCode2 = form["CampaignCode"];  //Mod 2018/08/14 yano #3912
            salesHeaderCondition.ServiceOrderStatus = form["ServiceOrderStatus"];
            salesHeaderCondition.Vin = form["Vin"];
            salesHeaderCondition.VinFull = form["VinFull"];
            salesHeaderCondition.RegistrationNumberPlate = form["PlateNumber"];
            salesHeaderCondition.CarBrandName = form["CarBrandName"];
            salesHeaderCondition.QuoteDateFrom = CommonUtils.StrToDateTime(form["QuoteDateFrom"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.QuoteDateTo = CommonUtils.StrToDateTime(form["QuoteDateTo"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.ServiceWorkCode = form["ServiceWorkCode"];
            salesHeaderCondition.DepartmentCode = form["DepartmentCode"];
            salesHeaderCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"]);
            salesHeaderCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"]);
            salesHeaderCondition.WithoutAkaden = form["WithoutAkaden"] != null && form["WithoutAkaden"].Equals("0");
            salesHeaderCondition.CustomerClaimCode = form["CustomerClaimCode"];
            salesHeaderCondition.CustomerClaimName = form["CustomerClaimName"];

            if (form["DelFlag"] != null && (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))) {
                salesHeaderCondition.DelFlag = form["DelFlag"];
            }
            //salesHeaderCondition.SetAuthCondition((Employee)Session["Employee"]);
            return serviceSalesOrderDao.GetListByCondition(salesHeaderCondition, (Employee)Session["Employee"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region 入力画面表示
        /// <summary>
        /// サービス伝票入力画面表示
        /// </summary>
        /// <param name="SlipNo">サービス伝票番号</param>
        /// <param name="RevNo">改訂番号</param>
        /// <param name="OrgSlipNo">車両伝票番号</param>
        /// <history>
        /// 2017/01/21 arc yano #3657 見積書の個人情報の表示・非表示を切り替えるチェックボックスを追加
        /// 2015/10/28 arc yano #3289 部品仕入機能改善(サービス伝票入力)
        /// </history>
        /// <returns></returns>
        public ActionResult Entry(string SlipNo, int? RevNo, string OrgSlipNo, string Mode) {

            //サービス伝票が指定されている場合はデータを取得する
            ServiceSalesHeader header;
            if (string.IsNullOrEmpty(SlipNo)) {
                header = new ServiceSalesHeader();
                Employee employee = (Employee)Session["Employee"];
                header.QuoteDate = DateTime.Today;
                header.QuoteExpireDate = DateTime.Today.AddDays(6);
                header.Department = employee.Department1;
                header.DepartmentCode = employee.DepartmentCode;
                header.FrontEmployeeCode = employee.EmployeeCode;
                header.ReceiptionEmployeeCode = employee.EmployeeCode;
                header.ServiceOrderStatus = "001";
                header.RevisionNumber = 0;
                header.LaborRate = 0;
                header.ArrivalPlanDate = DateTime.Today;

                header.ServiceSalesLine = new EntitySet<ServiceSalesLine>();
                header.ServiceSalesLine.Add(new ServiceSalesLine { ServiceType = "001" });

                //消費税IDが未設定であれば、当日日付で消費税ID取得
                if (header.ConsumptionTaxId == null)
                {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
                }


                for (int i = 0; i < 4; i++) {
                    header.ServiceSalesLine.Add(new ServiceSalesLine { ServiceType = "004" });
                }

            } else {
                //リビジョンを指定していない場合最新伝票を取得
                if (RevNo == null) {
                    header = new ServiceSalesOrderDao(db).GetBySlipNumber(SlipNo);
                } else {
                    header = new ServiceSalesOrderDao(db).GetByKey(SlipNo, RevNo ?? 1);
                }

                // 編集権限がある場合のみロック制御対象
                Employee loginUser = (Employee)Session["Employee"];
                string departmentCode = header.DepartmentCode;
                string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
                // 自部門・兼務部門１～３・セキュリティレベルALLのどれかに該当したらロックする
                if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004")) { 
                    // 何もしない
                } else {
                    // 自分以外がロックしている場合はエラー表示する
                    string lockEmployeeName = service.GetProcessLockUser(header);
                    if (!string.IsNullOrEmpty(lockEmployeeName)) {
                        header.LockEmployeeName = lockEmployeeName;
                    } else {
                        // 伝票ロック
                        service.ProcessLock(header);
                    }
                }

                //Add 2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能  次回点検日と車検有効期限は車両マスタから取得する
                //管理番号があり、納車済・キャンセル・作業履歴・作業中止でない、または、納車済でも修正中だった場合は次回点検日と車検有効期限は車両マスタから取得する
                if ((!string.IsNullOrEmpty(header.SalesCarNumber) && header.ServiceOrderStatus != "006" && header.ServiceOrderStatus != "007" && header.ServiceOrderStatus != "009" && header.ServiceOrderStatus != "010") || (header.ServiceOrderStatus == "006" && service.CheckModification(header.SlipNumber, header.RevisionNumber)))
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                    if (SalesCarData != null)
                    {
                        header.NextInspectionDate = SalesCarData.NextInspectionDate;
                        header.InspectionExpireDate = SalesCarData.ExpireDate;
                    }
                }


                //Del 2015/10/28 arc yano #3289 SetDataComponentへ移動
                /*
                PartsStockDao dao = new PartsStockDao(db);
                foreach (var a in header.ServiceSalesLine) {
                    if (!string.IsNullOrWhiteSpace(a.PartsNumber))
                    {
                        a.PartsStock = dao.GetStockQuantity(a.PartsNumber, header.DepartmentCode);
                    }   
                }
                 */
            }

            //車両伝票が指定されている場合は車両伝票の情報を引き継ぐ
            if (!string.IsNullOrEmpty(OrgSlipNo)) {
                CarSalesHeader car = new CarSalesOrderDao(db).GetBySlipNumber(OrgSlipNo);
                if (car != null) {
                    header.CarSlipNumber = OrgSlipNo;
                    header.CarEmployeeCode = car.EmployeeCode;
                    header.CarSalesOrderDate = car.SalesOrderDate;
                    header.CustomerCode = car.CustomerCode;
                    header.CarGradeCode = car.CarGradeCode;
                    header.CarGradeName = car.CarGradeName;
                    header.CarName = car.CarName;
                    header.CarBrandName = car.CarBrandName;
                    header.ModelName = car.ModelName;
                    header.Mileage = car.Mileage;
                    header.MileageUnit = car.MileageUnit;
                    header.ManufacturingYear = car.ManufacturingYear;
                    header.Vin = car.Vin;
                    header.SalesCarNumber = car.SalesCarNumber;
                    header.LaborRate = 0;
                    //作業依頼から引き継ぎ
                    ServiceRequest request = new ServiceRequestDao(db).GetBySlipNumber(OrgSlipNo);
                    header.RequestContent = request.Memo;
                    foreach (var a in request.ServiceRequestLine) {
                        ServiceSalesLine line = new ServiceSalesLine();
                        Parts parts = new PartsDao(db).GetByKey(a.CarOptionCode);
                        if (parts != null) {
                            line.PartsNumber = parts.PartsNumber;
                        }
                        line.ServiceType = "003";
                        line.ServiceTypeName = "部品";
                        line.LineContents = a.CarOptionName;
                        line.Amount = a.Amount;
                        line.LineNumber = a.LineNumber;
                        line.RequestComment = a.RequestComment;
                        header.ServiceSalesLine.Add(line);
                    }
                }
            }

            

            //サービス受付からの引き継ぎ
            if (!string.IsNullOrEmpty(Request["customerCode"])) {
                header.CustomerCode = Request["customerCode"];

            }
            if (!string.IsNullOrEmpty(Request["employeeCode"])) {
                header.ReceiptionEmployeeCode = Request["employeeCode"];
            }
            if (!string.IsNullOrEmpty(Request["salesCarNumber"])) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(Request["salesCarNumber"]);
                header.Vin = salesCar.Vin;
                header.RegistrationNumberType = salesCar.RegistrationNumberType;
                header.RegistrationNumberPlate = salesCar.RegistrationNumberPlate;
                header.RegistrationNumberKana = salesCar.RegistrationNumberKana;
                header.NextInspectionDate = salesCar.NextInspectionDate;
                header.MorterViecleOfficialCode = salesCar.MorterViecleOfficialCode;
                header.ModelName = salesCar.ModelName;
                header.MileageUnit = salesCar.MileageUnit;
                header.Mileage = salesCar.Mileage;
                header.ManufacturingYear = salesCar.ManufacturingYear;
                header.FirstRegistration = salesCar.FirstRegistrationYear;
                header.EngineType = salesCar.EngineType;
                header.CarName = salesCar.CarGrade.Car.CarName;
                header.CarGradeName = salesCar.CarGrade.CarGradeName;
                header.CarGradeCode = salesCar.CarGradeCode;
                header.CarBrandName = salesCar.CarGrade.Car.Brand.CarBrandName;
                header.SalesCarNumber = salesCar.SalesCarNumber;
                if (salesCar.ExpireDate != null && salesCar.ExpireType != null && salesCar.ExpireType.Equals("001")) {
                    header.InspectionExpireDate = salesCar.ExpireDate;
                }
                try { header.LaborRate = salesCar.CarGrade.Car.Brand.LaborRate; } catch (NullReferenceException) { }
            }
            if (!string.IsNullOrEmpty(Request["arrivalPlanDate"])) {
                header.ArrivalPlanDate = DateTime.Parse(HttpUtility.HtmlDecode(Request["arrivalPlanDate"]));
            }
            if (!string.IsNullOrEmpty(Request["requestDetail"])) {
                header.RequestContent = Request["requestDetail"];
            } 

            //Add 2017/01/21 arc yano 
            if (header.ServiceOrderStatus.Equals("001"))    //伝票ステータス=「見積」
            {
                header.DispPersonalInfo = false;
            }
            else ////伝票ステータス≠「見積」
            {
                header.DispPersonalInfo = true;            
            }


            //データ付き画面コンポーネントの読み込み
            SetDataComponent(ref header);

            //値引額を税込表示に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //合計計算
            //2014/03/19.ookubo「[CRMs - バグ #3006] 【サ】納車済みで数字が変わる」対応のためコメントアウト（処理廃止→クライアントjs処理にまかす）
            //service.CalcLineAmount(header);

            ViewData["Mode"] = Mode;

            //入力画面を表示
            return GetViewResult(header);
        }

        /// <summary>
        /// 伝票をコピーして入力画面表示（明細だけ複製）
        /// </summary>
        /// <param name="SlipNo"></param>
        /// <param name="RevNo"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// 2015/10/28 arc yano #3289 部品仕入機能改善(サービス伝票入力)
        /// </history>
        public ActionResult Copy(string SlipNo, int RevNo) {
            ServiceSalesHeader header = new ServiceSalesHeader();
            Employee employee = (Employee)Session["Employee"];
            header = new ServiceSalesHeader();
            header.QuoteDate = DateTime.Today;
            header.QuoteExpireDate = DateTime.Today.AddDays(6);
            header.Department = employee.Department1;
            header.DepartmentCode = employee.DepartmentCode;
            header.FrontEmployeeCode = employee.EmployeeCode;
            header.ReceiptionEmployeeCode = employee.EmployeeCode;
            header.ServiceOrderStatus = "001";
            header.RevisionNumber = 0;
            header.LaborRate = 0;
            header.ArrivalPlanDate = DateTime.Today;

            //消費税IDが未設定であれば、当日日付で消費税ID取得
            if (header.ConsumptionTaxId == null)
            {
                header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }


            ServiceSalesHeader original = new ServiceSalesOrderDao(db).GetByKey(SlipNo, RevNo);
            if (original.ServiceSalesLine.Count() > 0) {
                foreach (var item in original.ServiceSalesLine) {
                    // 請求先をクリア
                    item.CustomerClaimCode = "";
                    // 引当済数、発注数をクリア
                    item.ProvisionQuantity = 0; // Add 2015/10/28 arc yano
                    item.OrderQuantity = 0; // Add 2015/10/28 arc yano

                    //Add 2018/05/22 arc yano #3887
                    //原価(単価)を再取得

                    if(!string.IsNullOrWhiteSpace(item.PartsNumber))
                    {
                        PartsMovingAverageCost condition = new PartsMovingAverageCost();

                        condition.PartsNumber = item.PartsNumber;
                        condition.CompanyCode = "001";

                        item.UnitCost = (new PartsMovingAverageCostDao(db).GetByKey(condition) != null ? new PartsMovingAverageCostDao(db).GetByKey(condition).Price : (new PartsDao(db).GetByKey(item.PartsNumber) != null ? new PartsDao(db).GetByKey(item.PartsNumber).Cost : item.UnitCost));
                        item.Cost = Math.Round((item.UnitCost ?? 0) * (item.Quantity ?? 0), 0, MidpointRounding.AwayFromZero);
                    }
                }

                // 明細を引き継ぐ
                header.ServiceSalesLine = original.ServiceSalesLine;
            }

            //データ付き画面コンポーネントの読み込み
            SetDataComponent(ref header);

            //値引額を税込表示に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //合計計算
            //2014/03/19.ookubo「[CRMs - バグ #3006] 【サ】納車済みで数字が変わる」対応のためコメントアウト（処理廃止→クライアントjs処理にまかす）
            //service.CalcLineAmount(header);

            return GetViewResult(header);
        }
        #endregion

        #region セットメニュー追加
        /// <summary>
        /// 主作業が選択されたときセットメニューがあれば追加する
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <param name="line">サービス明細データ</param>
        /// <param name="pay">支払方法データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        /// <history>
        /// arc yano #3596 【大項目】部門棚統合対応 在庫の管理を部門単位から倉庫単位に変更
        /// 2016/04/14 arc yano #3480
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddSetMenu(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {
            int currentLineNumber = int.Parse(form["CurrentLineNumber"] ?? "0");
            ViewData["EntryMode"] = form["EntryMode"];

            //Add 2014/06/17 arc yano lineをlineNumber順にソートする。
            ModelState.Clear();
            //明細行をlineNumber順に並び替え
            Sortline(ref line);

            //セットメニューコードを取得
            string setMenuCode = line[currentLineNumber].SetMenuCode;
            string classification1 = line[currentLineNumber].Classification1;

            //セットメニューの行を削除
            line.RemoveAt(currentLineNumber);

            if (!string.IsNullOrEmpty(setMenuCode)) {
                //セットメニューに紐付いている作業項目を取得
                List<SetMenuList> list = new SetMenuListDao(db).GetListByCondition(new SetMenuList() { SetMenuCode = setMenuCode });

                for (int i = 0; i < list.Count; i++) {
                    ServiceSalesLine addLine = new ServiceSalesLine();
                    addLine.ServiceType = list[i].ServiceType;
                    addLine.WorkType = list[i].WorkType;

                    switch (list[i].ServiceType){
                        case "001":
                            //主作業
                            addLine.ServiceWorkCode = list[i].ServiceWorkCode;
                            addLine.LineContents = list[i].ServiceWork.Name;
                            addLine.Classification1 = classification1; 
                            //Add #3480 主作業の場合は、請求先分類を設定する
                            if(!string.IsNullOrWhiteSpace(list[i].ServiceWorkCode))
                            {
                                addLine.ServiceWork = new ServiceWorkDao(db).GetByKey(list[i].ServiceWorkCode.Trim());
                                addLine.SWCustomerClaimClass = addLine.ServiceWork.CustomerClaimClass;
                            }
                            
                            break;
                        case "002":
                            //サービスメニュー
                            addLine.ServiceMenuCode = list[i].ServiceMenuCode;
                            addLine.LineContents = list[i].ServiceMenu.ServiceMenuName;
                            addLine.Classification1 = classification1;
                            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                            //グレードが指定されていたら工数を取得
                            if (!string.IsNullOrEmpty(header.CarGradeCode) && CommonUtils.DefaultString(list[i].AutoSetAmount).Equals("001"))
                            {
                                header.CarGrade = new CarGradeDao(db).GetByKey(header.CarGradeCode);
                                ServiceCost cost = new ServiceCostDao(db).GetByKey(list[i].ServiceMenuCode, header.CarGrade.CarClassCode);
                                addLine.ManPower = cost != null ? cost.Cost : 0;
                                int laborRate = 0;
                                if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                     laborRate = int.Parse(form["LaborRate"] ?? "0");
                                }
                                addLine.LaborRate = laborRate;
                                addLine.TechnicalFeeAmount = addLine.ManPower * addLine.LaborRate;
                            }
                            break;
                        case "003":
                            //部品
                            addLine.PartsNumber = list[i].PartsNumber;
                            addLine.LineContents = list[i].Parts != null ? (list[i].Parts.PartsNameJp ?? list[i].Parts.PartsNameEn) : "";
                            addLine.Classification1 = classification1;
                            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                            if (CommonUtils.DefaultString(list[i].AutoSetAmount).Equals("001"))
                            {
                                //在庫と単価をセット
                                addLine.Price = list[i].Parts.Price;
                                addLine.Quantity = 1;
                                addLine.Amount = addLine.Price * addLine.Quantity;

                                //Mod 2016/08/13 arc yano #3596
                                //部門コードから使用倉庫を割出す
                                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                                addLine.PartsStock = new PartsStockDao(db).GetStockQuantity(list[i].PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));
                                //addLine.PartsStock = new PartsStockDao(db).GetStockQuantity(list[i].PartsNumber, header.DepartmentCode);
                            }
                            //ADD 2016/05/06 arc nakayama #3514_サービス伝票　判断が「SP」在庫判断が「在庫」の組み合わせを入力できてしまうケースがある
                            //判断が「SP」だった場合は在庫判断も「SP」にする
                            if (addLine.WorkType == "015")
                            {
                                addLine.StockStatus = "997";//SP
                            }

                            break;
                        case "004":
                            //コメント
                            addLine.LineContents = list[i].Comment;
                            addLine.Classification1 = classification1;
                            break;
                    }
                    //addLine.c_ServiceType = new CodeDao(db).GetServiceTypeByKey("002");

                    line.Insert(currentLineNumber + i, addLine);
                }
            }
            //Add 2016/04/22 arc nakayama #3495_サービス伝票入力　セットメニュー選択時の順序の不具合 LineNumberとdisplayOrderを振りなおしてソートする
            for (int i = 0; i < line.Count; i++)
            {
                line[i].LineNumber = i;
                line[i].DisplayOrder = i;
            }
            Sortline(ref line);

            ModelState.Clear();

            //支払方法を紐付け
            header.ServiceSalesPayment = pay;
           
            //明細と紐付け
            header.ServiceSalesLine = line;

            //合計を計算する
            //2014/03/19.ookubo「[CRMs - バグ #3006] 【サ】納車済みで数字が変わる」対応のためコメントアウト（処理廃止→クライアントjs処理にまかす）
            //service.CalcLineAmount(header);

            //画面コンポーネントのセット
            SetDataComponent(ref header);

            //入力画面を表示
            return GetViewResult(header);
        }
        #endregion

        #region 
        /// <summary>
        /// 全画面モード切替
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <param name="line">サービス明細データ</param>
        /// <param name="pay">支払方法データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeEntryMode(
            ServiceSalesHeader header,
            EntitySet<ServiceSalesLine> line,
            EntitySet<ServiceSalesPayment> pay,
            FormCollection form) {

            //Add 2014/06/17 arc yano lineをlineNumber順にソートする。
            ModelState.Clear();
            //明細行をlineNumber順に並び替え
            Sortline(ref line);

            //Ad 2017/11/18 #3803
            SetOutputTragetFlag(ref line);


            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            ViewData["EntryMode"] = form["EntryMode"];
            //Add 2014/12/24 arc yano #3143 合計値の再計算(SetDataComponent内で実行)の前に値引額を一旦、税抜表示に戻し、再計算後に、再度税込表示に変換する。
            //値引額を税込表示から、税抜表示に変換
            service.SetDiscountAmountWithoutTax(line);
            
            //画面コンポーネントのセット
            SetDataComponent(ref header);

            //値引額を税込表示に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            // 標準モード
            return GetViewResult(header);
        }

        #endregion

        #region 作業部品明細行追加、削除
        /*//Del 2014/06/12 arc yano 明細行の追加、削除はクライアントでおこなうため、削除。
        public ActionResult AddServiceLine(ServiceSalesHeader header,EntitySet<ServiceSalesLine> line,EntitySet<ServiceSalesPayment> pay,FormCollection form){
           
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;
            string classification1 = "";
            string customerClaimCode = "";
            if (line!=null && line.Count > 0) {
                classification1 = line[line.Count - 1].Classification1;
                customerClaimCode = line[line.Count - 1].CustomerClaimCode;
            }
            ViewData["lineScroll"] = form["lineScroll"];
            ViewData["EntryMode"] = form["EntryMode"];

            if (!string.IsNullOrEmpty(form["EditType"])) {
                ModelState.Clear();
                switch (form["EditType"]) {
                    case "add":
                        int addSize = int.Parse(form["AddSize"] ?? "1");
                        for (int i = 0; i < addSize; i++) {
                            ServiceSalesLine addLine = new ServiceSalesLine();
                            addLine.ServiceType = form["ServiceType"];
                            addLine.Classification1 = classification1;
                            addLine.CustomerClaimCode = customerClaimCode;
                            c_ServiceType serviceType = new CodeDao(db).GetServiceTypeByKey(form["ServiceType"]);
                            if (serviceType != null) {
                                addLine.ServiceTypeName = serviceType.ShortName;
                            }
                            if (addLine.ServiceType.Equals("002")) {
                                int laborRate = 0;
                                if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                    int.TryParse(form["LaborRate"], out laborRate);
                                }
                                addLine.LaborRate = laborRate;
                                if (line != null) {
                                    var query =
                                        (from a in line
                                         where a.ServiceType.Equals("002")
                                         orderby a.LineNumber descending
                                         select a).FirstOrDefault();
                                    addLine.EmployeeCode = query != null ? query.EmployeeCode : "";
                                }
                            }

                            header.ServiceSalesLine.Add(addLine);
                        }
                        break;
                    case "delete":
                        header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                        break;
                    case "insert":
                        ServiceSalesLine insertLine = new ServiceSalesLine();
                        insertLine.ServiceType = form["ServiceType"];
                        insertLine.Classification1 = classification1;
                        c_ServiceType insertServiceType = new CodeDao(db).GetServiceTypeByKey(form["ServiceType"]);
                        if (insertServiceType != null) {
                            insertLine.ServiceTypeName = insertServiceType.ShortName;
                        }
                        if (insertLine.ServiceType.Equals("002")) {
                            int laborRate = 0;
                            if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                int.TryParse(form["LaborRate"], out laborRate);
                            }
                            insertLine.LaborRate = laborRate;
                            var query =
                                (from a in line
                                 where a.ServiceType.Equals("002")
                                 && a.LineNumber <= int.Parse(form["EditLine"])
                                 orderby a.LineNumber descending
                                 select a).FirstOrDefault();
                            insertLine.EmployeeCode = query!=null ? query.EmployeeCode : "";

                        }
                        header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]), insertLine);
                        break;
                    case "copy":
                        ServiceSalesLine copyLine = (ServiceSalesLine)header.ServiceSalesLine[int.Parse(form["EditLine"])].Clone();
                        header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) + 1, copyLine);
                        
                        break;
                    case "up":
                        if (int.Parse(form["EditLine"]) > 0) {
                            ServiceSalesLine upLine = header.ServiceSalesLine[int.Parse(form["EditLine"])];
                            header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                            header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) - 1, upLine);
                        }
                        break;
                    case "down":
                        if (int.Parse(form["EditLine"]) < header.ServiceSalesLine.Count-1) {
                            ServiceSalesLine upLine = header.ServiceSalesLine[int.Parse(form["EditLine"])];
                            header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                            header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) + 1, upLine);
                        }
                        break;
                    default:
                        break;
                }
            }  
            
            //合計計算
            //2014/03/19.ookubo「[CRMs - バグ #3006] 【サ】納車済みで数字が変わる」対応のためコメントアウト（処理廃止→クライアントjs処理にまかす）
            //service.CalcLineAmount(header);
            
            //画面コンポーネントのセット
            SetDataComponent(ref header);

            //入力画面の表示
            return GetViewResult(header);
                        
        }
         */
        #endregion
        
        #region 支払方法行追加、削除
        public ActionResult AddPaymentLine(ServiceSalesHeader header,EntitySet<ServiceSalesLine> line,EntitySet<ServiceSalesPayment> pay,FormCollection form){
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            foreach (var p in header.ServiceSalesPayment) {
                p.DepositFlag = p.DepositFlag.Contains("true") ? "1" : "0";
            }

            if (!string.IsNullOrEmpty(form["DelPayLine"])) {
                ModelState.Clear();
                //DelPayLineが0以上だったら指定行削除
                if (Int32.Parse(form["DelPayLine"]) >= 0) {
                    header.ServiceSalesPayment.RemoveAt(Int32.Parse(form["DelPayLine"]));
                    header.PaymentTotalAmount = header.ServiceSalesPayment.Sum(x => x.Amount);
                } else {
                    ServiceSalesPayment addPayment = new ServiceSalesPayment();
                    if (!string.IsNullOrEmpty(header.CustomerCode)) {
                        header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    }
                    if (header.Customer != null && header.Customer.CustomerClaim != null) {
                        addPayment.CustomerClaimCode = header.Customer.CustomerClaim.CustomerClaimCode;
                    }
                    addPayment.DepositFlag = "0";
                    header.ServiceSalesPayment.Add(addPayment);
                }
            }

            //合計計算
            //2014/03/19.ookubo「[CRMs - バグ #3006] 【サ】納車済みで数字が変わる」対応のためコメントアウト（処理廃止→クライアントjs処理にまかす）
            //service.CalcLineAmount(header);

            //表示項目を再セット
            SetDataComponent(ref header);

            //支払方法入力画面を初期表示にする
            ViewData["displayContents"] = "invoice";

            //出口
            return GetViewResult(header);
        }
        #endregion

        #region 請求先一括設定
        /// <summary>
        /// 請求先を一括登録する
        /// </summary>
        /// <param name="header">ヘッダ</param>
        /// <param name="line">明細</param>
        /// <param name="pay">支払い方法</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 設定する請求先が主作業の請求先分類と一致したもののみ設定する
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SetCustomerClaim(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            //Add 2014/06/17 arc yano lineをlineNumber順にソートする。
            ModelState.Clear();
            //明細行をlineNumber順に並び替え
            Sortline(ref line);
            
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;
            ModelState.Clear();
            ViewData["EntryMode"] = form["EntryMode"];
            if (!string.IsNullOrEmpty(header.CustomerCode)) {
                header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode);
            }
            if (header.Customer != null && header.Customer.CustomerClaim!=null) {
                for (int i = 0; i < header.ServiceSalesLine.Count; i++) {
                    ServiceWork rec = new ServiceWorkDao(db).GetByKey(header.ServiceSalesLine[i].ServiceWorkCode);

                    if (string.IsNullOrEmpty(header.ServiceSalesLine[i].CustomerClaimCode) && (string.IsNullOrWhiteSpace(header.ServiceSalesLine[i].ServiceWorkCode) || string.IsNullOrWhiteSpace(rec.CustomerClaimClass) || header.Customer.CustomerClaim.c_CustomerClaimType == null || string.IsNullOrWhiteSpace(header.Customer.CustomerClaim.c_CustomerClaimType.CustomerClaimClass) || rec.CustomerClaimClass.Equals(header.Customer.CustomerClaim.c_CustomerClaimType.CustomerClaimClass)))   //2016/04/14 arc yano #3480
                    {
                        header.ServiceSalesLine[i].CustomerClaimCode = header.Customer.CustomerClaim.CustomerClaimCode;
                    }
                }    
            }

            //Add 2014/12/24 arc yano #3143 合計値の再計算(SetDataComponent内で実行)の前に値引額を一旦、税抜表示に戻し、再計算後に、再度税込表示に変換する。
            //値引額を税込表示から、税抜表示に変換
            service.SetDiscountAmountWithoutTax(line);

            SetDataComponent(ref header);

            //値引額を税込表示に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            return GetViewResult(header);
        }
        #endregion

        #region 伝票保存処理

        /// <summary>
        /// サービス伝票保存処理
        /// </summary>
        /// <param name="header">サービス伝票</param>
        /// <param name="line">サービス伝票明細</param>
        /// <param name="pay">サービス伝票支払</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2023/08/22 yano #4177【サービス伝票入力】請求先別明細の表示不正の対応
        /// 2018/05/28 arc yano #3889 サービス伝票入力 サービス伝票発注部品が引当されない
        /// 2018/05/24 arc yano #3896 サービス伝票入力　納車後の納車確認書が印刷できない
        /// 2018/02/23 arc yano #3849  ショートパーツ、社内調達品の赤伝・赤黒処理時のチェック 伝票修正時のvalidationチェックに引数(line)を追加
        /// 2018/02/20 arc yano #3858 サービス伝票　納車後の保存処理で、納車日を空欄で保存できてしまう
        /// 2017/11/03 arc yano #3732 サービス伝票 赤黒伝票作成で「入力されているメカ担当者はマスタに存在していません 」表示 引数追加
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
        /// 2016/04/21 arc yano #3496 サービス伝票入力　行移動時の発注数の更新の不具合
        /// 2016/04/20 arc yano #3492 サービス伝票入力　伝票保存時のエラー 
        /// 2016/01/26 arc yano #3453 サービス伝票　在庫管理対象外の部品はvalidationを行わない
        /// 2016/02/22 arc yano #3434 サービス伝票  消耗品(SP)の対応
        /// 2016/02/17 arc yano #3435 サービス伝票　原価０の部品の対応
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数、発注数の更新を行う
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            List<PartsPurchaseOrder> orderList = new List<PartsPurchaseOrder>(); //Add 2015/10/28 arc yano #3289

            //Add 2016/04/20 arc yano #3496
            if (header.ActionType == "Reflesh")
            {
                ModelState.Clear();

                header = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                //画面コンポーネントをセット
                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            //Add 2015/03/17 arc nakayama 伝票修正ボタン　または　修正キャンセル　が押された場合は別処理（修正中にする　または　修正を中止する）
            if (header.ActionType == "ModificationStart" || header.ActionType == "ModificationCancel")
            {   
                //Add 2017/04/23 arc yano
                ModelState.Clear();

                //伝票修正ボタンが押される前の情報取得
                ServiceSalesHeader SlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                //本締めだった場合はエラー、修正中にさせない
                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, SlipData.SalesDate, "001") && header.ActionType == "ModificationStart")
                {
                    ModelState.AddModelError("SalesDate", "本締めが実行されたため、伝票修正は行えません。");
                    //明細行をlineNumber順に並び替え
                    Sortline(ref line);

                    service.ProcessLockUpdate(header);
                    header.ServiceSalesLine = line;
                    header.ServiceSalesPayment = pay;
                    //Add 2015/04/02 arc nakayama バグ修正　走行距離の単位が表示されないタイミングがある
                    header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                    //値引額を税込表示から、税抜表示に変換
                    service.SetDiscountAmountWithoutTax(line);
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }
                service.SalesOrder(header, line, ref orderList);    //Mod 2015/10/28 arc yano #3289
                ModelState.Clear();
                //明細行をlineNumber順に並び替え
                Sortline(ref line);

                service.ProcessLockUpdate(header);
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                //Add 2015/04/02 arc nakayama バグ修正　走行距離の単位が表示されないタイミングがある
                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                //Add 2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能 修正中にするときは最新の日付を取得する
                //伝票修正ボタンが押され、管理番号がある場合は車両マスタから次回点検日と車検有効期限を取得する
                if (header.ActionType == "ModificationStart")
                {
                    if (!string.IsNullOrEmpty(header.SalesCarNumber))
                    {
                        SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                        if (SalesCarData != null)
                        {
                            header.NextInspectionDate = SalesCarData.NextInspectionDate;
                            header.InspectionExpireDate = SalesCarData.ExpireDate;
                        }
                    }
                }
                else
                {
                  //修正キャンセルが押された場合は、スナップショットの日付に戻す
                    ServiceSalesHeader PrevSlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //Mod 2018/02/23 ardc yano #3741
                    header = PrevSlipData;
                    //header.NextInspectionDate = PrevSlipData.NextInspectionDate;
                    //header.InspectionExpireDate = PrevSlipData.InspectionExpireDate;
                }


                //値引額を税込表示から、税抜表示に変換
                service.SetDiscountAmountWithoutTax(line);
                SetDataComponent(ref header);
                return GetViewResult(header);
            }



            //Add 2014/06/10 arc yano 高速化対応
            ModelState.Clear();
            //明細行をlineNumber順に並び替え
            Sortline(ref line);

            //Add 2017/10/19 arc yano #3803
            SetOutputTragetFlag(ref line);

            if (form["ForceUnLock"] != null && form["ForceUnLock"].Equals("1"))
                
            {
                service.ProcessLockUpdate(header);

                header = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);       //DBから引き直し

                /*
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                */
                ViewData["ForceUnLock"] = "1";

                //Add 2014/12/24 arc yano #3143 合計値の再計算(SetDataComponent内で実行)の前に値引額を一旦、税抜表示に戻し、再計算後に、再度税込表示に変換する。
                //値引額を税込表示から、税抜表示に変換
                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //値引額を税込表示に変換
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
        
            }
            //Add 2014/06/17 arc yano 高速化対応 削除対象行のデータ削除
            //Mod 2015/10/15 arc nakayama #3264_サービス伝票のコピーで再利用した伝票で有効なメカ担当者修正後「入力されているメカ担当者はマスタに存在していません 」表示 バリデーションチェックの前に移動
            Delline(ref line);

            //Mod 2018/05/25 arc yano #3896 納車済以降の保存・帳票の出力の場合はチェックしない
            //Mod 2018/02/20 arc yano #3858
            //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
            //Add 2015/10/23 arc nakayama #3254_作業履歴伝票を活用できるようにしたい 見積に戻すボタンが押されたときもチェックを行わない
            //ステータスが納車済みでない、かつ、納車確認書ボタン/保存ボタンが押されていない時、または、伝票削除を押されていない時だけvalidationチェックを行う
            if (
                 !(
                    (header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update")) || 
                     header.ActionType.Equals("Cancel") || 
                     header.ActionType.Equals("Restore")
                  )
               )
            {
                //Add 2017/10/19 arc yano #3803
                //発注書出力の場合は専用の処理を行う
                if (header.Output.Equals(true))
                {
                    ValidateOutput(form, header, line);
                }

                //共通Validationチェック
                ValidateAllStatus(header, line, form);  //Mod 2017/11/03 arc yano #3732
            }

            //Add 2018/05/28 arc yano #3889
            //キャンセルの場合はエラーチェック
            if (header.ActionType.Equals("Cancel"))
            {
                //------------------------------------------------------
                //別ユーザにより伝票が更新されていた場合はエラーとする
                //------------------------------------------------------
                //DBから最新の伝票を取得
                ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                if (dbHeader != null && dbHeader.RevisionNumber > header.RevisionNumber)
                {
                    ModelState.AddModelError("RevisionNumber", "削除しようとしている伝票は最新ではありません。最新の伝票を開き直した上で削除を行って下さい");
                }
            }

            //Mod  2018/05/28 arc yano #3889
            if (!ModelState.IsValid)
            {
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                //Add 2015/04/02 arc nakayama バグ修正　走行距離の単位が表示されないタイミングがある
                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                //Add 2014/12/24 arc yano #3143 合計値の再計算(SetDataComponent内で実行)の前に値引額を一旦、税抜表示に戻し、再計算後に、再度税込表示に変換する。
                //値引額を税込表示から、税抜表示に変換
                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //値引額を税込表示に変換
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }

            // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {
                //値引を税抜に変換
                service.SetDiscountAmountWithoutTax(line);

                //見積保存時とそれ以外で処理を分岐
                switch (header.ActionType)
                {
                    case "Quote":
                        // 見積保存
                        service.Quote(header, line);
                        break;

                    case "History":
                        // 作業履歴
                        service.History(header, line);
                        break;

                    case "Restore":
                        // 見積に戻す
                        service.Quote(header, line);
                        //Add 2016/04/08 arc nakayama #3197サービス伝票の車検有効期限＆次回点検日を車両マスタへコピーする際の確認メッセージ機能 見積に戻した時も最新の次回点検日と車検有効期限に切り替える
                        if (!string.IsNullOrEmpty(header.SalesCarNumber))
                        {
                            SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                            if (SalesCarData != null)
                            {
                                header.NextInspectionDate = SalesCarData.NextInspectionDate;
                                header.InspectionExpireDate = SalesCarData.ExpireDate;
                            }
                        }

                        break;
                    case "Stop":
                        // 作業中止
                        service.Stop(header, line);
                        break;
                    case "Cancel":
                        // 伝票キャンセル
                        service.Cancel(header, line);
                        break;

                    //Add 2015/03/18 arc nakayama 伝票修正対応　修正完了処理
                    case "ModificationEnd":　//修正完了（修正確定）

                        ValidateSalesOrder(header, line);
                        ValidateForModification(header, line); //修正時のvalidationチェック       //Mod 2018/02/22 arc yano #3741

                        //ロケーションに不備があったらエラー
                        if (!ModelState.IsValid)
                        {
                            //値引を税込に変換
                            service.SetDiscountAmountWithTax(line);

                            header.ServiceSalesLine = line;
                            header.ServiceSalesPayment = pay;

                            SetDataComponent(ref header);

                            return GetViewResult(header);
                        }

                        service.SalesOrder(header, line, ref orderList);    //Mod 2015/10/28 arc yano #3289
                        break;

                    default:
                        //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
                        //ステータスが納車済み、かつ、納車確認書ボタン/保存ボタンが押された時は引き継ぎメモだけを更新する、また、納車日が編集可能であれば更新する
                        if (header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update"))
                        {
                            //納車日と引き継ぎメモの文字数のバリデーションチェック
                            ValidateForMemoUpdate(header);
                            if (!ModelState.IsValid)
                            {
                                //値引を税込に変換
                                service.SetDiscountAmountWithTax(line);

                                header.ServiceSalesLine = line;
                                header.ServiceSalesPayment = pay;
                                SetDataComponent(ref header);
                                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                                return GetViewResult(header);
                            }

                            //引き継ぎメモ更新
                            service.UpDateMemoServiceSalesHeader(header);

                            //Mod 2023/08/22 #4177
                            //service.SetDiscountAmountWithTax(line);
                            
                            header.ServiceSalesLine = line;
                            header.ServiceSalesPayment = pay;
                        }
                        else
                        {
                            // 受注以降の共通処理
                            ValidateSalesOrder(header, line);
                            
                            //ロケーションに不備があったらエラー
                            if (!ModelState.IsValid)
                            {
                                //値引を税込に変換
                                service.SetDiscountAmountWithTax(line);

                                header.ServiceSalesLine = line;
                                header.ServiceSalesPayment = pay;

                                SetDataComponent(ref header);

                                return GetViewResult(header);
                            }

                            /*  2016/04/21 dell #3496
                            //Mod 2015/10/28 arc yano #3289
                            ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                            List<ServiceSalesLine> list  = new List<ServiceSalesLine>();

                            if (dbheader != null)
                            {
                                list = dbheader.ServiceSalesLine.ToList();
                            }

                            for (int cnt = 0; cnt < line.Count(); cnt++)
                            {
                                //Mod 2016/04/20 arc yano #3492 FirstOrDefault()を追加
                                if (list.Count > 0 && list.Where(x => x.LineNumber.Equals(line[cnt].LineNumber)).FirstOrDefault() != null)
                                {
                                    line[cnt].OrderQuantity = list.Where(x => x.LineNumber.Equals(line[cnt].LineNumber)).FirstOrDefault().OrderQuantity;
                                }
                            }
                            */

                            service.SalesOrder(header, line, ref orderList); //Mod 2015/10/28 arc yano #3289

                            //

                        }
                        break;
                }

                //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
                //ステータスが納車済みでない、かつ、納車確認書ボタン/保存ボタンが押されていない時だけvalidationチェックを行う
                if (!(header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update")))
                {
                    //ヘッダINSERT
                    db.ServiceSalesHeader.InsertOnSubmit(header);
                }
                //作業依頼のタスクを削除
                if (!string.IsNullOrEmpty(header.CarSlipNumber))
                {
                    List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.SERVICE_REQUEST, header.CarSlipNumber);
                    foreach (var a in taskList)
                    {
                        a.TaskCompleteDate = DateTime.Now;
                    }
                }

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0023", "キー重複のデータは"));
                        //Add 2014/08/08 arc amii エラーログ対応 ステータス・伝票番号・改訂番号を戻す処理追加
                        header.ServiceOrderStatus = form["PreviousStatus"];
                        header.SlipNumber = form["PrvSlipNumber"];
                        header.RevisionNumber = Int32.Parse(form["PrvRevisionNumber"]);
                        header.ServiceSalesLine = line;
                        header.ServiceSalesPayment = pay;
                        SetDataComponent(ref header);

                        //Mod 2014/12/24 arc yano #3143 値引を税込の変換処理を再計算後に行うように移動する
                        //値引を税込に変換
                        service.SetDiscountAmountWithTax(line);

                        return GetViewResult(header);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    //Add 2014/08/08 arc amii エラーログ対応 上記以外の例外の場合、エラーログを出力する処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                    // エラーページに遷移
                    return View("Error");
                }
            }

            ModelState.Clear();

            //画面コンポーネントをセット
            SetDataComponent(ref header);

            //Mod 2014/12/24 arc yano #3143 値引を税込の変換処理を再計算後に行うように移動する
            //値引を税込に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //帳票印刷
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            }

            /*
            //Add 2017/10/19 arc yano #3803
            if (header.ActionType.Equals("Output"))
            {
                header.Output = true;
            }
            else
            {
                header.Output = false;
            }
            */

            //Add 2015/10/28 arc yano #3289
            if (orderList != null)
            {
                header = makeOrderUrl(header, orderList);
            } 
            
            return GetViewResult(header);
        }
        #endregion

        #region Excel出力機能
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">発注データ明細</param> 
        /// <returns>Excelデータ</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力
        /// </history>
        //[AcceptVerbs(HttpVerbs.Post)]
        public  ActionResult Download(FormCollection form, ServiceSalesHeader header, EntitySet<ServiceSalesLine> line)
        {
            //-------------------------------
            //変数宣言
            //-------------------------------
            string fileName = "";               //ファイル名
            string filePathName = "";           //パス＋ファイル名
            string tfilePathName = "";          //テンプレートファイル名(パス＋ファイル名)

            //-------------------------------
            //初期処理
            //-------------------------------  
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_PURCHASEORDER_DOWNLOAD);

            ModelState.Clear();

            //DBから取得
            ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //-------------------------------
            //ファイル名・テンプレートファイルの設定
            //-------------------------------
            SetFileName(ref fileName, ref filePathName, ref tfilePathName, form);
   
            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");

                header.ServiceSalesLine = line;

                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //値引額を税込表示に変換
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, dbHeader, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                header.ServiceSalesLine = line;

                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //値引額を税込表示に変換
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }


            //発注済フラグの更新
            SetOrderedFlag(dbHeader, line, form);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        protected byte[] MakeExcelData(FormCollection form, ServiceSalesHeader header, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine hconfigLine = null;            //設定値(ヘッダ、フッタ)
            ConfigLine lconfigLine = null;            //設定値(明細)

            byte[] excelData = null;                  //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                  //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定値の取得
            //----------------------------
            int columnLine = 2;             //列の位置

            //設定ファイル読込
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //----------------------------------
            //ヘッダ、フッタの設定値の取得
            //----------------------------------
            if (config != null)
            {
                hconfigLine = dExport.GetConfigLine(config, columnLine);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }

                return excelData;
            }

            columnLine++;

            //---------------------------
            //明細設定値の取得
            //---------------------------
            lconfigLine = dExport.GetConfigLine(config, columnLine);

            //----------------------------
            // 設定するデータの取得
            //----------------------------
            List<PurchaseOrderExcelHeader> orderList = SetOrderList(header, header.ServiceSalesLine, form["StockCode"]);

            /*
            //取得したデータが0件の場合は処理終了
            if (orderList.Count == 0)
            {
                ModelState.AddModelError("", "発注書に表示するデータがありません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }

                return excelData;
            }
            */

            //オリジナルシートの取得
            ExcelWorksheet originSheet = excelFile.Workbook.Worksheets[hconfigLine.SheetName];

            int sheetCnt = 4;                                       //開始シートの位置

            string sheetName = "";                                  //操作対象のシート名

            string orgSheetName = hconfigLine.SheetName;           //テンプレシート


            //------------------------------
            //データ設定
            //------------------------------
            foreach (var order in orderList)
            {
                List<PurchaseOrderExcelHeader> headerList = new List<PurchaseOrderExcelHeader>();

                //シートを追加する
                //シート名を発注区分付のものに変更
                //sheetName = orgSheetName + "_" + order.sheetNameIdx.Replace("/", "");

                sheetName = order.sheetNameIdx.Replace("/", "");

                excelFile.Workbook.Worksheets.Add(sheetName, originSheet);

                //-------------------------
                //ヘッダー、フッターの設定
                //-------------------------
                hconfigLine.SheetName = sheetName;

                headerList.Add(order);

                //データ設定
                ret = dExport.SetData<PurchaseOrderExcelHeader, PurchaseOrderExcelHeader>(ref excelFile, headerList, hconfigLine);

                //-------------------------
                //明細の設定
                //-------------------------
                //設定値のシート名を新しいシート名に変更
                lconfigLine.SheetName = sheetName;

                //データ設定
                ret = dExport.SetData<PurchaseOrderExcelLine, PurchaseOrderExcelLine>(ref excelFile, order.line, lconfigLine);

                sheetCnt++;
            }

            //テンプレシートの削除
            excelFile.Workbook.Worksheets.Delete(originSheet);

            excelData = excelFile.GetAsByteArray();

            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

        /// <summary>
        /// Excelのヘッダー、フッター部分の設定
        /// </summary>
        /// <param name="header">ヘッダ情報</param>
        /// <returns name ="eHeader"></returns>
        /// <history>
        /// 2017/12/14 arc yano #3834 部品発注書の部品名の参照先の変更 部品名(PartsNameJp)
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        private List<PurchaseOrderExcelHeader> SetOrderList(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, string stockStatusCode)
        {
            //ヘッダ情報
            List<PurchaseOrderExcelHeader> eHeaderList = new List<PurchaseOrderExcelHeader>();
            //明細情報
            List<PurchaseOrderExcelLine> eline = new List<PurchaseOrderExcelLine>();

            //発注書に出力する明細を抽出
            eline = line.Where
                (
                            
                    x => 
                        x.OutputTargetFlag != null &&
                        x.OutputTargetFlag.Equals("1") && 
                        !string.IsNullOrWhiteSpace(x.StockStatus) && 
                        !string.IsNullOrWhiteSpace(x.PartsNumber)       
                ).Select
                (
                    x =>
                    
                        new PurchaseOrderExcelLine
                    
                        {

                            //Mod 2017/12/14 arc yano #3834 
                            
                            OrderPartsNumber = (x.Parts != null ? !string.IsNullOrWhiteSpace(x.Parts.MakerPartsNumber) ? x.Parts.MakerPartsNumber : x.PartsNumber : x.PartsNumber)      //部品番号                                                                            //発注部品番号
                            ,
                            OrderPartsNameJp = (x.Parts != null ? !string.IsNullOrWhiteSpace(x.Parts.PartsNameJp) ? x.Parts.PartsNameJp : x.LineContents : x.LineContents)    //発注部品名 //Mod 2017/12/14 arc yano #3834
                            ,
                            Quantity = (x.Quantity ?? 0)                                                                                                                                //数量
                            ,
                            Price = (x.Parts != null ? x.Parts.Price != null ? x.Parts.Price : x.Price : x.Price)                                                                       //定価
                            ,
                            Cost = null
                            ,
                            Memo = null
                            ,
                            StockStatus = x.StockStatus                                                                                                                                 //判断
                            ,
                        PartsNumber = x.PartsNumber
                        }
                ).ToList();

            //抽出した明細をさらに部品番号、判断毎に抽出
            eline = eline.GroupBy(x => new { x.OrderPartsNumber, x.OrderPartsNameJp, x.StockStatus, x.Price, x.PartsNumber }).Select(group =>
                    new PurchaseOrderExcelLine
                    {
                        OrderPartsNumber = group.Key.OrderPartsNumber                           //発注部品番号
                        ,
                        OrderPartsNameJp = group.Key.OrderPartsNameJp                           //発注部品名
                        ,
                        Quantity = group.Sum( x => x.Quantity)                                  //数量（合計値）
                        ,
                        Price = group.Key.Price                                                 //定価
                        ,
                        Cost = null                                                             //原価
                        ,
                        Memo = null                                                             //備考
                        ,
                        StockStatus = group.Key.StockStatus                                     //判断
                        ,
                        PartsNumber = group.Key.PartsNumber                                     //部品番号
                    }
                    ).ToList();

            //判断区分
            List<string> stockStatusCodeList = new List<string>();

            //明細で判断毎にグルーピングしてリストを取得(部品在庫出庫伝票の場合は判断＝在庫のみ)
            stockStatusCodeList = line.Where
                                    (
                                        x => 
                                            x.OutputTargetFlag != null &&
                                            x.OutputTargetFlag.Equals("1") && 

                                            !string.IsNullOrWhiteSpace(x.StockStatus) &&
                                            (
                                              ( //判断 = 「S/O等の発注」
                                                string.IsNullOrWhiteSpace(stockStatusCode) &&
                                                x.c_StockStatus.StatusType.Equals("001")
                                              ) ||
                                              x.StockStatus.Equals(stockStatusCode)     //判断 = 「在庫」
                                            )

                                    ).GroupBy
                                    (
                                        x => 
                                            x.StockStatus
                                    ).Select
                                    (
                                        x =>
                                            x.Key
                                    ).ToList();

            //判断
            List<c_StockStatus> stockStatusList = new List<c_StockStatus>();

            foreach (var code in stockStatusCodeList)
            {
                 
                c_StockStatus rec = new CodeDao(db).GetStockStatus(false, code, "");

                if (rec != null)
                {
                    stockStatusList.Add(rec);
                }
            }

            //判断毎にデータを作成
            foreach (var stockStatus in stockStatusList)
            {
                //発注データの設定
                eHeaderList = SetOrderData(stockStatus, header, line, eHeaderList, eline);
            }

            return eHeaderList;
        }

        /// <summary>
        /// ファイル名、テンプレートファイル名の設定
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="filePathName">パス名＋ファイル名</param>
        /// <param name="tfilePathName">テンプレートファイル名</param>
        /// <return></return>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        private int SetFileName(ref string fileName , ref string filePathName, ref string tfilePathName, FormCollection form   )
        {
            string filePath = "";

            //ワークフォルダ取得
            filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            //部品発注書出力の場合
            if (string.IsNullOrWhiteSpace(form["StockCode"]))
            {
                //ファイル名(部品発注書_yyyyMMddhhmiss(ダウンロード時刻))
                fileName = "部品発注書" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

                filePathName = filePath + fileName;

                //テンプレートファイルパス取得
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrderFromService"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrderFromService"];

            }
            else //部品在庫出庫伝票の場合
            {
                //ファイル名(PartsPurchaseOrder_xxx(発注番号)_yyyyMMddhhmiss(ダウンロード時刻))
                fileName = "部品在庫出庫伝票" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

                filePathName = filePath + fileName;

                //テンプレートファイルパス取得
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsMaterialDocumentForService"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsMaterialDocumentForService"];
            }
            
            return 0;
        }

        /// <summary>
        /// Excelのヘッダー、フッター部分の設定
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="line">サービス伝票明細</param>
        /// <param name="eHeaderList">発注書ヘッダ</param>
        /// <param name="elin">発注書明細</param>
        /// <returns name ="eHeaderList"></returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        private List<PurchaseOrderExcelHeader> SetOrderData(c_StockStatus stockStatus, ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, List<PurchaseOrderExcelHeader> eHeaderList, List<PurchaseOrderExcelLine> eline)
        { 

            int offset = 0;     //データの取得位置
            int index = 1;      //シート名のindex

            string searchSlipNumber = header.SlipNumber.Substring(0, 8);        //黒伝票の場合は「－」以降を除去（※赤伝の場合は本処理には来ない）

            //ループ処理
            while (true)
            {
                PurchaseOrderExcelHeader eHeader = new PurchaseOrderExcelHeader();

                //部門名
                if (!string.IsNullOrWhiteSpace(header.DepartmentCode))
                {
                    eHeader.DepartmentName = header.Department.DepartmentName;
                }

                //フロント担当者
                if (!string.IsNullOrWhiteSpace(header.FrontEmployeeCode))
                {
                    eHeader.FrontEmployeeName = header.FrontEmployee.EmployeeName;
                }

                //メカ、または外注
                string employeeCode = null;
                string supplierCode = null;

                //明細の中で、メカニック担当者コードが入力されていて、行番号が一番若いものを取得する。
                employeeCode = line.OrderBy(x => x.LineNumber).Where(x => !string.IsNullOrWhiteSpace(x.EmployeeCode)).Select(x => x.EmployeeCode).FirstOrDefault();

                //取得できた場合
                if (!string.IsNullOrWhiteSpace(employeeCode))
                {
                    eHeader.EmployeeSupplierName = new EmployeeDao(db).GetByKey(employeeCode).EmployeeName;
                }
                else //取得できなかった場合
                {
                    //明細の中で、外注者コードが入力されていて、行番号が一番若いものを取得する
                    supplierCode = line.OrderBy(x => x.LineNumber).Where(x => !string.IsNullOrWhiteSpace(x.SupplierCode)).Select(x => x.SupplierCode).FirstOrDefault();

                    eHeader.EmployeeSupplierName = new SupplierDao(db).GetByKey(supplierCode) != null ? new SupplierDao(db).GetByKey(supplierCode).SupplierName : "";
                }

                //発注日
                eHeader.PurchaseOrderDate = DateTime.Now;

                //伝票番号
                eHeader.SlipNumber = header.SlipNumber;

                //顧客名
                eHeader.CustomerName = header.Customer.CustomerName;

                //車種名
                eHeader.CarName = header.CarName;

                //型式
                eHeader.ModelName = header.ModelName;

                //初年度登録
                eHeader.FirstRegistration = header.FirstRegistration;

                //車台番号
                eHeader.Vin = header.Vin;

                //オーダー区分(名称)
                eHeader.OrderTypeName = stockStatus.Name;

                //メーカーオーダー番号
                eHeader.MakerOrderNumber = "";     //現時点では未対応（ユーザに手入力してもらう）

                //インボイス番号
                eHeader.InvoiceNumber = "";        //現時点では未対応（ユーザに手入力してもらう）

                //入庫日
                eHeader.ArrivalPlanDate = header.ArrivalPlanDate;

                //引き継ぎメモ
                eHeader.Memo = header.Memo;

                //発注伝票番号
                eHeader.PurchaseOrderNumber = "";  //現時点では未対応（ユーザに手入力してもらう）

                //明細(明細は10件ずつ取得して、10件超えた場合は次のデータに設定
                eHeader.line = eline.Where(x => !string.IsNullOrWhiteSpace(x.StockStatus) && x.StockStatus.Equals(stockStatus.Code)).Skip(offset).Take(10).ToList();

                //備考欄の設定
                foreach (var l in eHeader.line)
                {
                    //発注済数
                    decimal ? orderedQuantity = null;

                    orderedQuantity = new ServiceSalesOrderDao(db).GetOutputQuantity(searchSlipNumber, l.PartsNumber, l.StockStatus);

                    //過去の発注が無い場合
                    if (orderedQuantity == null)
                    {
                        l.Memo = "新規追加";
                    }
                    else //過去の発注数があり
                    {
                         //今回発注する数量と過去の数量が変っていた場合
                        if (l.Quantity != orderedQuantity)
                        {
                            l.Memo = "数量が " + orderedQuantity + " ⇒ " + l.Quantity + " に変更されました";
                        }
                    }
                }

                //０件の場合は処理を抜ける
                if (eHeader.line.Count == 0)
                {
                    break;
                }
                
                //オフセット値の更新
                offset += 10;

                //シート名はオーダー種別＋index
                eHeader.sheetNameIdx = eHeader.OrderTypeName + "_" + index;

                index++;

                eHeaderList.Add(eHeader);
            }
            
            return eHeaderList;
        }

        /// <summary>
        /// 発注済フラグの設定
        /// </summary>
        /// <param name="dbHeader">ヘッダ(dbから取得)</param>
        /// <param name="line">明細</param>
        /// <param name="form">フォーム値</param>
        /// <return></return>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        private void SetOrderedFlag(ServiceSalesHeader dbHeader, EntitySet<ServiceSalesLine> line, FormCollection form)
        {
            //発注済フラグを設定
            dbHeader.ServiceSalesLine.Where
               (
                   x =>
                       x.OutputTargetFlag != null &&
                       x.OutputTargetFlag.Equals("1") &&
                       (
                       //判断 = 「S/O,E/O等の発注」
                           string.IsNullOrWhiteSpace(form["StockCode"]) &&
                           (
                               x.c_StockStatus.StatusType.Equals("001")
                           )
                           ||
                           x.StockStatus.Equals(form["StockCode"])     //判断 = 「在庫」
                        )
                ).ToList().ForEach(x => x.OutputFlag = "1");

            int index = 0;

            //明細行の登録し直し
            EntitySet<ServiceSalesLine> eline = new EntitySet<ServiceSalesLine>();

            //再度登録
            foreach (var l in dbHeader.ServiceSalesLine)
            {
                ServiceSalesLine newline = new ServiceSalesLine();

                newline.SlipNumber = l.SlipNumber;                      //伝票番号
                newline.RevisionNumber = l.RevisionNumber;              //改訂番号
                newline.LineNumber = l.LineNumber;                      //行番号
                newline.ServiceType = l.ServiceType;                    //種別
                newline.SetMenuCode = l.SetMenuCode;                    //セットメニューコード
                newline.ServiceWorkCode = l.ServiceWorkCode;            //主作業コード
                newline.ServiceMenuCode = l.ServiceMenuCode;            //サービスメニューコード
                newline.PartsNumber = l.PartsNumber;                    //部品番号
                newline.LineContents = l.LineContents;                  //部品名
                newline.RequestComment = l.RequestComment;             
                newline.WorkType = l.WorkType;                          //作業区分
                newline.LaborRate = l.LaborRate;                        //レバレー度
                newline.ManPower = l.ManPower;                          //
                newline.TechnicalFeeAmount = l.TechnicalFeeAmount;      //技術料
                newline.Quantity = l.Quantity;                          //数量
                newline.Price = l.Price;                                //単価
                newline.Amount = l.Amount;                              //金額
                newline.Cost = l.Cost;                                  //原価
                newline.EmployeeCode = l.EmployeeCode;
                newline.SupplierCode = l.SupplierCode;
                newline.CustomerClaimCode = l.CustomerClaimCode;
                newline.StockStatus = l.StockStatus;
                newline.CreateEmployeeCode = l.CreateEmployeeCode;
                newline.CreateDate = l.CreateDate;
                newline.LastUpdateEmployeeCode = l.LastUpdateEmployeeCode;
                newline.LastUpdateDate = l.LastUpdateDate;
                newline.DelFlag = l.DelFlag;
                newline.Classification1 = l.Classification1;
                newline.TaxAmount = l.TaxAmount;
                newline.UnitCost = l.UnitCost;
                newline.LineType = l.LineType;
                newline.ConsumptionTaxId = l.ConsumptionTaxId;
                newline.Rate = l.Rate;
                newline.ProvisionQuantity = l.ProvisionQuantity;
                newline.OrderQuantity = l.OrderQuantity;
                newline.DisplayOrder = l.DisplayOrder;
                newline.OutputTargetFlag = l.OutputTargetFlag;
                newline.OutputFlag = l.OutputFlag;

                eline.Add(newline);

                index++;
            }

            db.ServiceSalesLine.DeleteAllOnSubmit(dbHeader.ServiceSalesLine);

            db.ServiceSalesLine.InsertAllOnSubmit(eline);

            //DB登録
            db.SubmitChanges();
        }



        #endregion

        #region Validationチェック
        /// <summary>
        /// 共通Validationチェック
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// 2019/02/06 yano #3959 サービス伝票入力　請求先誤り
        /// 2018/11/09 yano #3953 サービス伝票入力　納車時のマスタチェックでショートパーツ、社内調達部品はマスタチェックを行わない
        /// 2018/08/29 yano #3932 サービス伝票_請求先未入力で納車処理を行える
        /// 2018/08/29 yano #3925 サービス伝票　明細にマスタ未登録の部品が存在したまま納車できる
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない
        /// 2018/05/17 arc yano #3884 サービス伝票　赤黒処理時に元伝票の納車日の締めチェックを行っている
        /// 2018/02/20 arc yano #3858 サービス伝票　納車後の保存処理で、納車日を空欄で保存できてしまう
        /// 2017/11/03 arc yano #3732 赤黒伝票作成で「入力されているメカ担当者はマスタに存在していません 」表示 引数(form)追加　赤・赤黒の場合はマスタチェックしない
        /// 2017/11/03 arc yano #3774 サービス伝票の黒伝票で、明細の一行目を主作業以外にすると落ちる 黒伝票の時もチェックするように処理を変更する
        /// 2017/07/03 arc yano #3776 サービス伝票　同一倉庫を使用している部門変更時の引当の不具合
        /// 2017/02/08 arc yano #3645 入荷確定時のエラー対応 LineContentsの長さのチェック値を25→50に変更(DBのサイズに合わせる)
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 部門が変更された場合は、引当済数は０としてvalidationチェックを行う
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2016/01/26 arc yano #3406 サービス伝票　引当時のvalidaionエラーメッセージ出力の抑制 引当チェックの引数追加
        /// </history>
        private void ValidateAllStatus(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, FormCollection form) {

            if (header.RevisionNumber != 0) {
                ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
                string employeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                if (target.ProcessSessionControl != null && !target.ProcessSessionControl.EmployeeCode.Equals(employeeCode)) {
                    ModelState.AddModelError("", "この伝票は" + target.ProcessSessionControl.Employee.EmployeeName + "さんによって編集されたため保存出来ません。「閉じる」ボタンで伝票を閉じて下さい。");
                    return;
                }
            }

            //vs2012対応 2014/04/17 arc.ookubo
            //if (!header.SlipNumber.Contains("-")) {
            if ((header.SlipNumber == null) || (!header.SlipNumber.Contains("-1"))) //Mod 2017/11/03 arc yano #3774
            {

                //vs2012対応 2014/04/17 arc.ookubo
                if (header.CustomerCode == null)
                {
                    header.CustomerCode = string.Empty;
                }
                //必須チェック
                CommonValidate("DepartmentCode", "部門", header, true);
                CommonValidate("FrontEmployeeCode", "フロント担当者", header, true);
                CommonValidate("ReceiptionEmployeeCode", "受付担当者", header, true);
                CommonValidate("QuoteDate", "見積日", header, true);
                CommonValidate("InspectionExpireDate", "車検有効期限", header, false);
                CommonValidate("CarTax", "自動車税種別割", header, false);    //Mod 2019/09/04 yano #4011
                CommonValidate("CarLiabilityInsurance", "自賠責保険料", header, false);
                CommonValidate("CarWeightTax", "自動車重量税", header, false);
                CommonValidate("NumberPlateCost", "ナンバー代", header, false);
                CommonValidate("FiscalStampCost", "印紙代", header, false);

                CommonValidate("TaxFreeFieldValue", "その他", header, false);  //Mod 2020/02/17 yano #4025

                //Add 2020/02/17 yano #4025
                CommonValidate("OptionalInsurance", "任意保険", header, false);
                CommonValidate("SubscriptionFee", "サービス加入料", header, false);
                CommonValidate("TaxableFreeFieldValue", "その他(課税)", header, false);
                
                
                // Add 2015/05/07 arc nakayama #3083_サービスで伝票検索出来ない 保存時にも顧客の必須チェックを行う
                CommonValidate("CustomerCode", "顧客", header, true);
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("001"))
                {
                    CommonValidate("QuoteExpireDate", "見積有効期限", header, true);

                    //見積有効期限は見積日以降しか認めない
                    if (ModelState.IsValidField("QuoteExpireDate") && header.QuoteDate != null && header.QuoteExpireDate != null) {
                        if (header.QuoteDate != null && DateTime.Compare(header.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, header.QuoteExpireDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                            ModelState.AddModelError("QuoteExpireDate", MessageUtils.GetMessage("E0013", new string[] { "見積有効期限", "見積日以降" }));
                        }
                    }
                }
                CommonValidate("ArrivalPlanDate", "入庫日", header, false);

                //初度登録
                if (!string.IsNullOrEmpty(header.FirstRegistration)) {
                    if (!Regex.IsMatch(header.FirstRegistration, "([0-9]{4})/([0-9]{2})")
                        && !Regex.IsMatch(header.FirstRegistration, "([0-9]{4}/[0-9]{1})")) {
                        ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "初度登録"));
                    }
                    DateTime result;
                    try {
                        DateTime.TryParse(header.FirstRegistration + "/01", out result);
                        if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
                            ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "初度登録"));
                            if (ModelState["FirstRegistration"].Errors.Count() > 1) {
                                ModelState["FirstRegistration"].Errors.RemoveAt(0);
                            }
                        }
                    } catch {
                        ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "初度登録"));
                    }

                }
                //Edit 2014/06/16 arc yano 高速化対応 先頭に削除行の可能性があるため、明細1行目はline[0]ではなくline[x]
                int count = 0;
                //明細1行目が主作業必須
                if (lines == null || lines.Count == 0)  //明細行数が、無い場合
                {
                    ModelState.AddModelError(string.Format("line[{0}].{1}", "0", "LineContents"), "明細の1行目は主作業である必要があります");
                }
                else
                {
                    for (count = 0; count < lines.Count; count++)
                    {
                        if (string.IsNullOrEmpty(lines[count].CliDelFlag) || ((lines[count].CliDelFlag != null) && !(lines[count].CliDelFlag.Equals("1"))))
                        {
                            break;
                        }
                    }
                    if (count >= lines.Count || (lines[count].ServiceType != null && !(lines[count].ServiceType.Equals("001"))))
                    {
                        ModelState.AddModelError(string.Format("line[{0}].{1}", "0", "LineContents"), "明細の1行目は主作業である必要があります");
                    }
                }

                //int chkflg = 0;     //Add 2014/06/27 arc yano サービス伝票チェック新システム対応
                int posServiceWork = 0;

                bool displayFlag = false;   //表示済フラグ     //Mod 2016/01/26 arc yano

                //decimal totalAmount = 0;            //金額合計
                //decimal totalTechFee = 0;           //技術料合計
                //decimal totalSmCost = 0;            //原価(合計)合計/サービスメニュー
                //decimal totalPtCost = 0;            //原価(合計)合計/部品

                //明細行のチェック
                ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);      //Add 2017/01/31 arc yano #3566

                if (lines != null && lines.Count > 0)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        ServiceSalesLine line = lines[i];
                        //Add 2014/06/16 arc yano 入力検証はCliDelFlagが1以外(非表示レコード)に対して行う。
                        if (string.IsNullOrEmpty(line.CliDelFlag) || ((line.CliDelFlag != null) && !(line.CliDelFlag.Equals("1"))))
                        {

                            //種別が「主作業」の場合
                            if (!string.IsNullOrEmpty(line.ServiceType) && line.ServiceType.Equals("001"))
                            {
                                if (string.IsNullOrEmpty(line.ServiceWorkCode))     //主作業コードが未入力
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ServiceWorkCode"), MessageUtils.GetMessage("E0001", "主作業コード"));
                                    //chkflg = 0;
                                }
                                else
                                {
                                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、nullチェック追加
                                    if (line.ServiceWorkCode != null && line.ServiceWorkCode.Substring(0, 1).Equals("2"))    //主作業コードが「社内請求」
                                    {
                                        //-----------------------
                                        //社内請求チェック
                                        //-----------------------
                                        string CustomerClaimType = null;
                                        if (!string.IsNullOrEmpty(line.CustomerClaimCode))
                                        {
                                            CustomerClaimType = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode).CustomerClaimType; //請求先のタイプを取得
                                        }

                                        if (!string.IsNullOrEmpty(CustomerClaimType) && !CustomerClaimType.Equals("005")) //請求先タイプが社内以外
                                        {
                                            if (!ModelState.ContainsKey(string.Format("line[{0}].{1}", i, "ServiceWorkCode")))   //主作業コードに対して検証エラー未発生の場合。
                                            {
                                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ServiceWorkCode"), "");
                                            }

                                            ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), MessageUtils.GetMessage("E0022", new string[] { "主作業コードが社内請求", "請求先に請求先タイプが社内のものを設定する必要があります。" }));
                                        }

                                        //chkflg = 1;             //社内粗利チェック実行  //Del 2014/07/17 arc yano サービス伝票チェック新システム対応 社内粗利チェックはクライアントで行う。
                                        posServiceWork = i;     //主作業レコードの位置を待避
                                    }
                                    else
                                    {
                                        //chkflg = 0;             //チェックなし
                                    }
                                }
                            }

                            if (line.Quantity != null)
                            {
                                if ((Regex.IsMatch(line.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(line.Quantity.ToString(), @"^\d{1,7}$"))))
                                {
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "Quantity"), MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" }));
                                    if (ModelState[string.Format("line[{0}].{1}", i, "Quantity")].Errors.Count > 1)
                                    {
                                        ModelState[string.Format("line[{0}].{1}", i, "Quantity")].Errors.RemoveAt(0);
                                    }
                                }
                            }
                            if (line.ManPower != null)
                            {
                                if ((Regex.IsMatch(line.ManPower.ToString(), @"^\d{1,3}\.\d{1,2}$") || (Regex.IsMatch(line.ManPower.ToString(), @"^\d{1,5}$"))))
                                {
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ManPower"), MessageUtils.GetMessage("E0002", new string[] { "工数", "正の整数5桁以内かつ小数2桁以内" }));
                                    if (ModelState[string.Format("line[{0}].{1}", i, "ManPower")].Errors.Count > 1)
                                    {
                                        ModelState[string.Format("line[{0}].{1}", i, "ManPower")].Errors.RemoveAt(0);
                                    }
                                }
                            }

                            //Mod 2017/02/08 arc yano #3645
                            if (!string.IsNullOrEmpty(line.LineContents) && line.LineContents.Length > 50)
                            {
                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "LineContents"), "表示名は50文字以内で入力してください");
                            }

                            //Mod 2015/10/28 arc yano #3289 サービス伝票 部品仕入機能の改善(サービス伝票入力)
                            if (
                                header.ServiceOrderStatus.Equals("002") ||
                                header.ServiceOrderStatus.Equals("003") ||
                                header.ServiceOrderStatus.Equals("004") ||
                                header.ServiceOrderStatus.Equals("005")
                                )
                            {

                                //Add 2017/01/31 arc yano #3566
                                decimal? provisionQuantity;

                                //画面入力値の部門とDBに登録されている部門が異なる場合(=部門が変更された場合)
                                if (dbheader != null && !dbheader.DepartmentCode.Equals(header.DepartmentCode))
                                {
                                    //Mod 2107/07/03 arc yano #3776
                                    //変更前部門コードから使用倉庫を割出す
                                    DepartmentWarehouse prdWarehouse = CommonUtils.GetWarehouseFromDepartment(db, dbheader.DepartmentCode);

                                    //変更後部門コードから使用倉庫を割出す
                                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                                    //変更前の使用倉庫と変更後の使用倉庫が異なる場合
                                    if (dWarehouse != null && !prdWarehouse.WarehouseCode.Equals(dWarehouse.WarehouseCode))
                                    {
                                        //在庫数の再取得
                                        line.PartsStock = new PartsStockDao(db).GetStockQuantity(line.PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));

                                        //引当済数はリセット
                                        provisionQuantity = 0m;
                                    }
                                    else //使用倉庫が同一の場合
                                    {
                                        //引当済数はリセットしない
                                        provisionQuantity = line.ProvisionQuantity;
                                    }
                                }
                                else
                                {
                                    provisionQuantity = line.ProvisionQuantity;
                                }

                                ValidateHikiate(line, i, ref displayFlag, provisionQuantity);
                            }

                            //Add 2018/08/29 yano #3925, #3932
                            //納車処理、または伝票修正完了の場合
                            if (
                                   (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") && CommonUtils.DefaultString(header.ActionType).Equals("Sales")) ||
                                   (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd"))
                               )
                            {
                                //Mod 2018/11/09 yano #3953
                                //-------------------
                                //マスタチェック
                                //-------------------
                                //種別が部品の場合で在庫判断が「社内調達」「ショートパーツ」でない場合
                                if (line.ServiceType.Equals("003") && (!string.IsNullOrWhiteSpace(line.StockStatus) && !line.StockStatus.Equals("997") && !line.StockStatus.Equals("998")))
                                {
                                    //部品マスタ
                                    Parts parts = new PartsDao(db).GetByKey(line.PartsNumber);

                                    //マスタに無い場合
                                    if (parts == null)
                                    {
                                        ModelState.AddModelError(string.Format("line[{0}].{1}", i, "PartsNumber"), "部品マスタに未登録の部品のため、納車処理を行えません。部品マスタに登録後に再度納車処理を行って下さい");
                                    }
                                }
                                //請求先マスタ
                                //主作業の場合のみチェックを行う
                                if (line.ServiceType.Equals("001"))
                                {
                                    //請求先コードが未入力の場合
                                    if (string.IsNullOrWhiteSpace(line.CustomerClaimCode))
                                    {
                                        ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "請求先が未入力のため、納車処理を行えません。請求先を入力後に再度納車処理を行って下さい");
                                    }
                                    else //マスタチェック
                                    {
                                        CustomerClaim customerclaim = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode);

                                        if (customerclaim == null)
                                        {
                                            ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "請求先マスタに未登録の請求先のため、納車処理を行えません。請求先マスタに登録後に再度納車処理を行って下さい");
                                        }
                                        //Add 2019/02/06 yano #3959
                                        else
                                        {
                                            string makercode = (new DepartmentDao(db).GetByKey(header.DepartmentCode) != null ? new DepartmentDao(db).GetByKey(header.DepartmentCode).MainMakerCode : "");

                                            ServiceWorkCustomerClaim swCustomerClaim = new ServiceWorkCustomerClaimDao(db).GetByKey(line.ServiceWorkCode, makercode);

                                            ////主作業に対して請求先が設定されており、かつ明細の請求先が主作業に設定された請求先と異なる場合
                                            if (swCustomerClaim != null && !line.CustomerClaimCode.Equals(swCustomerClaim.CustomerClaimCode))
                                            {
                                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "主作業が 【" + line.LineContents + "】 の場合は請求先に 『" + swCustomerClaim.CustomerClaim.CustomerClaimName + "(コード=" + swCustomerClaim.CustomerClaimCode + ")』 を設定してください");

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Mod 2018/05/17 arc yano #3884
            //Mod 2018/02/20 arc yano #3858
            //Mod 2015/05/07 arc yano 仮締中編集権限追加 仮締中変更可能なユーザの場合、仮締めの場合でも、納車日の変更を行える
            //                        その他、見直し(伝票修正完了時の入力チェックの削除(専用のチェック処理[ValidateForModification]で行う))
            //Mod 2015/04/02 arc nakayama 納車日チェック対応　保存時も納車日のチェックを行う
            //Mod 2015/03/25 arc nakayama 伝票修正対応　修正完了時も同じチェックをかけるように変更
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //納車処理時または納車済での保存処理は納車日必須
            if (
                (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") && CommonUtils.DefaultString(header.ActionType).Equals("Sales"))  ||
                (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && (CommonUtils.DefaultString(header.ActionType).Equals("Update") || CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd")))
                )
            //if ((CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") &&
            //    CommonUtils.DefaultString(header.ActionType).Equals("Sales")) || (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("Update")))
                //CommonUtils.DefaultString(header.ActionType).Equals("Sales") || CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd") || (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("Update")))
            {
                int ret = 0;

                CommonValidate("SalesDate", "納車日", header, true);
                if (header.SalesDate != null) {
                    
                    // Del 2016/08/13 arc yano 古いコードは削除
                    // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                    // Mod 2015/02/03 arc nakayama 伝票修正対応　修正処理の場合は修正前と修正後で変化があった時だけチェックを行う

                    //Mod 2016/08/13 arc yano #3596
                    //部門コードから使用倉庫を割出す
                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode); ;

                    //伝票保存時
                    if (CommonUtils.DefaultString(header.ActionType).Equals("Update"))
                    {
                        //　編集前と差分があったらチェック
                        if (header.SalesDate != new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber).SalesDate)
                        {
                            ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                        }
                    }
                    else //納車処理時
                    {
                        ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                    }
                    
                    switch (ret)
                    {
                        //部品棚卸済による編集不可
                        case 1:
                            ModelState.AddModelError("SalesDate", "部品棚卸処理が実行されているため、指定された納車日では納車できません");
                            break;
                           

                        //月次締済による編集不可
                        case 2:
                            ModelState.AddModelError("SalesDate", "月次締処理が実行されているため、指定された納車日では納車できません");
                            break;
                            

                        default:
                            //何もしない
                            break;
                    }
                
                }
            }

            //-----------------------------------
            //マスタチェック
            //-----------------------------------
            //Mod 2017/11/03 arc yano #3732
            //赤伝・赤黒処理以外、または赤伝、黒伝の保存以外
            if ((string.IsNullOrWhiteSpace(form["Mode"]) || (!form["Mode"].Equals("1") && !form["Mode"].Equals("2"))) && !string.IsNullOrWhiteSpace(header.SlipNumber) && !header.SlipNumber.Contains("-") )
            {
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                //ADD 2014/02/20 ookubo
                //納車前ステータスまで納車予定日必須
                if (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("001") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("002") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("003") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("004"))
                {
                    CommonValidate("SalesPlanDate", "納車予定日", header, true);
                }

                // Add 2015/05/18 arc nakayama ルックアップ見直し対応　無効データも入力可能になるため、マスタチェックを行う
                //車両
                if (!string.IsNullOrEmpty(header.SalesCarNumber))
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                    if (SalesCarData == null)
                    {
                        ModelState.AddModelError("SalesCarNumber", "入力されている管理番号はマスタに存在していません。");
                    }
                }

                //顧客
                Customer CustomerData = new CustomerDao(db).GetByKey(header.CustomerCode);
                if (CustomerData == null)
                {
                    ModelState.AddModelError("CustomerCode", "入力されている顧客コードはマスタに存在していません。");
                }

                //社員
                if (!string.IsNullOrEmpty(header.ReceiptionEmployeeCode))
                {
                    Employee EmployeeData = new EmployeeDao(db).GetByKey(header.ReceiptionEmployeeCode);
                    if (EmployeeData == null)
                    {
                        ModelState.AddModelError("ReceiptionEmployeeCode", "入力されている社員コードはマスタに存在していません。");
                    }
                }

                //部門
                if (!string.IsNullOrEmpty(header.DepartmentCode))
                {
                    Department DepartmentData = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                    if (DepartmentData == null)
                    {
                        ModelState.AddModelError("DepartmentCode", "入力されている部門コードはマスタに存在していません。");
                    }
                }
                //メカ担当者
                if (lines != null && lines.Count > 0)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        ServiceSalesLine line = lines[i];
                        if (!string.IsNullOrEmpty(line.EmployeeCode))
                        {
                            Employee EmployeeData = new EmployeeDao(db).GetByKey(line.EmployeeCode);
                            if (EmployeeData == null)
                            {
                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "EmployeeCode"), "入力されているメカ担当者はマスタに存在していません");
                            }
                        }
                    }
                }
            }
            //Add 2015/05/22 arc nakayama #3208_サービス伝票の引継メモの文字数制限を超えるとシステムエラーになる 引き継ぎメモの文字数チェック追加（200文字）
            if (header.Memo != null && header.Memo.Length > 400)
            {
                ModelState.AddModelError("Memo", "引き継ぎメモは400文字以内で入力して下さい（改行は2文字分とみなされます）");
            }

            //Add 2017/04/27 arc nakayanma #3744_[製造] 売掛金自動調整バッチが起因となる不具合改修
            if ((header.ActionType.Equals("SalesOrder") && !string.IsNullOrWhiteSpace(header.Vin)) || (!header.ServiceOrderStatus.Equals("001") && !string.IsNullOrWhiteSpace(header.Vin)))
            {
                if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
                {
                    ModelState.AddModelError("CustomerCode", "受注以降は顧客コードに上様を使用することはできません。");
                }
            }

            //Add 2018/05/30 arc yano #3889
            //------------------------------------------------------
            //別ユーザにより伝票が更新されていた場合はエラーとする
            //------------------------------------------------------
            //DBから最新の伝票を取得
            ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            if (dbHeader != null && dbHeader.RevisionNumber > header.RevisionNumber)
            {
                ModelState.AddModelError("RevisionNumber", "保存を行おうとしている伝票は最新ではありません。最新の伝票を開き直した上で編集を行って下さい");
            }

        }

        /// <summary>
        /// 部品発注書出力時のvalidationチェック
        /// </summary>
        /// <param name="form">フォームの値</param>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        private void ValidateOutput(FormCollection form, ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) 
        {
            
            int cnt = lines.Where
                    (
                        x => 
                            x.OutputTarget.Contains("true") && 
                            (
                                //判断 = 「S/O,E/O等の発注」
                                string.IsNullOrWhiteSpace(form["StockCode"]) &&
                                (
                                    x.c_StockStatus != null && 
                                    x.c_StockStatus.StatusType.Equals("001") || 
                                    new CodeDao(db).GetStockStatus(false, x.StockStatus, "").StatusType.Equals("001")
                                )
                                ||
                                x.StockStatus.Equals(form["StockCode"])     //判断 = 「在庫」
                             )
                     ).Count();


            //取得したデータが0件の場合は処理終了
            if (cnt == 0)
            {
                ModelState.AddModelError("", "明細に表示するデータがありません");
            }

            return;
        }

        /// <summary>
        /// 仮締中編集権限有／無によるデータ編集可／不可判定処理
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">対象日付</param>
        /// <return>チェック結果(0:編集可 1:編集不可(月次締=仮締未満、部品棚卸=完了　※仮締中編集権限なしユーザがこの状態になる) 2:編集不可(月次締済))</return>
        /// <history>
        /// 2021/11/09 yano #4111【サービス伝票入力】仮締中の納車済伝票の納車日編集チェック処理の不具合
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/05/07 arc yano 仮締中編集権限追加  新規追加 仮締中変更可能なユーザの場合、仮締めの場合でも、変更可能とする
        /// </history>
        private int CheckTempClosedEdit(DepartmentWarehouse dWarehouse, DateTime? targetDate)
        {
            string securityCode = ((Employee)Session["Employee"]).SecurityRoleCode;     //セキュリティコード
            bool editFlag = false;                                                      //仮締中編集権限

            int ret = 0;                                                                //編集状況

            //部門コード
            string departmentCode = dWarehouse != null ? dWarehouse.DepartmentCode : "";
            //倉庫コード
            string warehouseCode = dWarehouse != null ? dWarehouse.WarehouseCode : "";

            //仮締中編集権限を取得
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(securityCode, "EditTempClosedData");

            if (rec != null)
            {
                editFlag = rec.EnableFlag;
            }

            //候補となる部門全てでチェックを行い、一つでも該当するものがあればそれを採用する
            //編集権限ありの場合
            if (editFlag == true)
            {
                //月次締のみチェック   ※部門単位でチェック
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, targetDate, "001", securityCode))
                {
                    ret = 2;
                }
            }
            else
            {
                //部品棚卸チェック 　※倉庫単位でチェック
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(warehouseCode, targetDate, "002"))
                {
                    ret = 1;
                }
                else //Mod 2021/11/09 yano #4111
                {
                    //部品棚卸チェックで問題ない場合、月次締チェック   ※部門単位でチェック
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, targetDate, "001", securityCode))
                    {
                        ret = 2;
                    }
                }
            }
         
            return ret;
        }
        /// <summary>
        /// 受注処理の時のValidationチェック
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <param name="lines">サービス伝票データ(明細行)</param>
        /// <history>
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 部門が変更されている場合は引当済数は０としてvalidationチェックを行う
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ロケーション取得の引数を部門コードから倉庫コードに変更
        /// 2016/04/11 arc yano #3487 納車処理時の未引当validationチェック 引当処理前にvalidationを行うように修正
        /// 2016/01/26 arc yano #3406_サービス伝票 引当時のvalidationエラーメッセージ出力の抑制 (#3397_【大項目】部品仕入機能改善 課題管理表対応) 引数(validation発生フラグ)追加
        /// 2016/01/25 arc yano #3407_サービス伝票 引当時のvalidationチェックの不具合対応 (#3397_【大項目】部品仕入機能改善 課題管理表対応) 引数(行数)追加
        /// </history>
        private void ValidateSalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) {
            CommonValidate("CustomerCode", "顧客", header, true);

            if (header.GrandTotalAmount == null) {
                ModelState.AddModelError("GrandTotalAmount", "金額が正しく設定できていません");
            }
            //if (!header.GrandTotalAmount.Equals(header.PaymentTotalAmount)) {
            //    ModelState.AddModelError("PaymentTotalAmount", "請求合計＝支払合計になっていません");
            //}
            //科目を取得
            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");
            if (serviceAccount == null) {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
            }
            if (header.ServiceSalesPayment != null) {
                List<ServiceSalesPayment> pay = header.ServiceSalesPayment.ToList();
                for(int i=0;i<header.ServiceSalesPayment.Count;i++) {
                    CustomerClaim claim = new CustomerClaimDao(db).GetByKey(pay[i].CustomerClaimCode);
                    if (string.IsNullOrEmpty(claim.CustomerClaimType)) {
                        ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "指定された請求先の請求先種別が設定されていません");
                    }
                    //クレジット会社の場合、決済条件が必須
                    if (claim != null && claim.CustomerClaimType != null && claim.CustomerClaimType.Equals("003")) {
                        if (claim.CustomerClaimable == null || claim.CustomerClaimable.Count == 0) {
                            ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "指定された請求先に決済条件が設定されていません");
                        }
                    }
                }
            }
            /* //2016/02/10 引当ロケーション廃止のため、validation処理をコメントアウト
                Location hikiateLocation = stockService.GetHikiateLocation(header.DepartmentCode);
                if (hikiateLocation == null) {
                    ModelState.AddModelError("", "部門内に引当ロケーションが1つも設定されていません");
                }
            */

            //仕掛ロケーションを取得する
            //伝票の部門コードから、使用倉庫を割り出す
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
           
            string warehouseCode = "";
            if (dWarehouse != null)
            {
                warehouseCode = dWarehouse.WarehouseCode;
            }

            //Location shikakariLocation = stockService.GetShikakariLocation(header.DepartmentCode);
            Location shikakariLocation = stockService.GetShikakariLocation(warehouseCode);
            if (shikakariLocation == null) {
                ModelState.AddModelError("", "該当部門が使用している倉庫内に仕掛ロケーションが1つも設定されていません");
            }

            int index = 0;
            bool displayFlag = false;   //表示済フラグ     //Mod 2016/01/26 arc yano

            foreach (var l in lines)
            {
                //判断 = 「在庫の場合」
                if( !string.IsNullOrWhiteSpace(l.StockStatus) && l.StockStatus.Equals("999"))       //Mod 2016/01/25 arc yano
                {
                    decimal? provisionQuantity = l.ProvisionQuantity;

                    ValidateHikiate(l, index, ref displayFlag, provisionQuantity);
                }
                index++;
            }

            //Mod 2016/04/11 #3487 arc yano
            if (header.ActionType.Equals("Sales"))
            {
                ValidateForSales(header, lines);
            }
        }

        /// <summary>
        /// 納車処理時のValidationチェック
        /// </summary>
        /// <param name="header"></param>
        /// <param name="lines"></param>
        /// <history>
        /// Add 2016/11/11 arc yano #3656 納車処理時のvalidationチェック処理を外だし
        /// </history>
        private void ValidateForSales(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            int cnt = 0;
            bool msgflg = false;

            foreach (var line in lines)
            {
                    //明細行の中でその部品が在庫管理対象で、原価０部品(社内調達品)や消耗品でない場合
                if (new PartsDao(db).IsInventoryParts(line.PartsNumber) && !line.StockStatus.Equals("998") && !line.StockStatus.Equals("997"))
                        {
                    //未引当行数をチェック
                    if ((line.Quantity - line.ProvisionQuantity) > 0)
                            {
                        //ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "未引当部品があるため、納車できません");
                        msgflg = true;
                        break;
                        }
                    }
                    cnt++;
                }

            if (msgflg == true)
            {
                //明細行の引当済数をDBの値で更新する
                service.ResetProvisionQuantity(header, lines);
                ModelState.AddModelError("", "未引当部品があるため、納車できませんでした。データを読み直しましたので、引当済数を確認後、再度納車処理を行って下さい");
            }
        }

        /// <summary>
        /// 赤伝処理時のValidationチェック
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/02/22 arc yano #3849 ショートパーツ、社内調達品の赤伝・赤黒処理時のチェック処理除外
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ロケーション取得の引数を部門コードから倉庫コードに変更
        /// </history>
        private void ValidateForAkaden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) {
            if (string.IsNullOrEmpty(header.Reason)) {
                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "赤伝処理", "理由" }));
            }
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "理由は1024文字以内で入力して下さい");
            }

            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            if (target != null) {
                ModelState.AddModelError("SlipNumber", "すでに処理されています");
            }

            //Mod 2018/02/22 arc yano #3849 ショートパーツ、社内調達については対象外
            //Mod 2014/06/16 arc yano 高速化対応 CliDelFlagが"1"以外のものをチェックする。
            var partsList = from a in lines
                            where a.ServiceType.Equals("003")
                            && (a.DelFlag == null || !a.DelFlag.Equals("1"))
                            && (string.IsNullOrEmpty(a.CliDelFlag) || !a.CliDelFlag.Equals("1"))
                            && !string.IsNullOrEmpty(a.PartsNumber)
                            && !a.PartsNumber.Contains("DISCNT")
                            && (!string.IsNullOrWhiteSpace(a.StockStatus) && !a.StockStatus.Equals("997") && !a.StockStatus.Equals("998"))
                            group a by a.PartsNumber into parts
                            select new {
                                PartsNumber = parts.Key,
                                Quantity = parts.Sum(x => x.Quantity)
                            };

            //PartsLocation location = new PartsLocationDao(db).GetByKey(item.PartsNumber, header.DepartmentCode);

            //Mod 2016/08/13 arc yano #3596
            //Location location = stockService.GetDefaultLocation(item.PartsNumber, header.DepartmentCode);
            //部門コードから使用している倉庫を割出す
            DepartmentWarehouse dWarehous = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            string warehouseCode = "";
            if (dWarehous != null)
            {
                warehouseCode = dWarehous.WarehouseCode;
            }

            foreach (var item in partsList) {
                
                Location location = stockService.GetDefaultLocation(item.PartsNumber, warehouseCode);
                if (location == null) {
                    ModelState.AddModelError("", "部品" + item.PartsNumber + "のロケーションが定義されておりません。ロケーションを再定義後に、赤伝（赤黒）処理を再度実施お願いいたします。");
                }
            }
        }

        #endregion
        
        //Add 2015/03/23 arc nakayama 伝票修正対応　伝票修正時のvalidationチェック追加
        #region 伝票修正時のvalidationチェック
        /// <summary>
        /// 伝票修正時のvalidationチェック
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/02/23 arc yano #3849  ショートパーツ、社内調達品の赤伝・赤黒処理時のチェック 類似 伝票修正時に数量が変っていて、かつ対象のロケーションが無い場合はエラーとする 引数追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 締め処理状況のチェック処理の引数を変更(header.DepartmentCode → warehouseCode)
        /// </history>
        private void ValidateForModification(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line)
        {
            if (string.IsNullOrEmpty(header.Reason))
            {
                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "修正処理", "理由" }));
            }
            if (header.Reason.Length > 1024)
            {
                ModelState.AddModelError("Reason", "理由は1024文字以内で入力して下さい");
            }

            // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
            //納車年月の締め処理状況が仮締め以上だった場合エラー
            /*
            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode, header.SalesDate, "001"))
            {
                ModelState.AddModelError("SalesDate", "月次締め処理が実行されているため、指定された納車日では納車できません");
            }
            */

            //Mod 2016/08/13 arc yano #3596
            //部門コードから使用倉庫を割出す
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

            int ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);

            switch (ret)
            {
                //部品棚卸済による編集不可
                case 1:
                    ModelState.AddModelError("SalesDate", "部品棚卸処理が実行されているため、指定された納車日では納車できません");
                    break;

                //月次締済による編集不可
                case 2:
                    ModelState.AddModelError("SalesDate", "月次締処理が実行されているため、指定された納車日では納車できません");
                    break;

                default:
                    //何もしない
                    break;
            }

            //Add 2018/02/23 arc yano #3849
            //-------------------------------
            //ロケーションのチェック
            //-------------------------------
            DepartmentWarehouse dWarehous = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            string warehouseCode = "";
            if (dWarehous != null)
            {
                warehouseCode = dWarehous.WarehouseCode;
            }

            //在庫管理対象の部品の明細行だけ取得
            var partsLinesEntity = (line.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997")))).Select(x => new { PartsNumber = x.PartsNumber, Quantity = (x.Quantity ?? 0), ProvisionQuantity = (x.ProvisionQuantity ?? 0) });

            List<PartsStock> stockList = new List<PartsStock>();

            foreach (var l in partsLinesEntity)
            {
                //数量を変更した場合(元々の引当済数と入力値の数量が異なっている場合)
                if (l.Quantity > l.ProvisionQuantity) //在庫を引き落とす場合
                {
                    //移動元は在庫の多いロケーションから順番に使う
                    stockList = new PartsStockDao(db).GetListByWarehouse(warehouseCode, l.PartsNumber, true).OrderBy(x => x.DelFlag).OrderByDescending(x => x.Quantity).ToList();

                    if (stockList.Count == 0)
                    {
                        ModelState.AddModelError("", "部品" + l.PartsNumber + "の在庫情報が無いため、数量を変更できません");
                    }
                }
                else if (l.Quantity < l.ProvisionQuantity) //在庫を戻す場合
                {
                    Location location = stockService.GetDefaultLocation(l.PartsNumber, warehouseCode);
                    if (location == null)
                    {
                        ModelState.AddModelError("", "部品" + l.PartsNumber + "のロケーションが定義されておりません。ロケーションを再定義後に、伝票修正処理を再度実施お願いいたします。");
                    }
                }
            }

        }
        #endregion

        #region 引当処理のValidationチェック
        /// <summary>
        /// 引当処理のValidationチェック
        /// </summary>
        /// <param name="line">サービス伝票データ(明細)</param>
        /// <param name="index">要素数</param>
        /// <param name="displayFlag">表示フラグ</param>
        /// <history>
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 部門が変更された場合は、引当済数は０としてvalidationチェックを行う
        /// 2016/01/26 arc yano #3453 サービス伝票　在庫管理対象外の部品はvalidationを行わない
        /// 2016/01/26 arc yano #3435 サービス伝票　原価０の部品の対応 左記対応時に発覚した不具合の対応
        /// 2016/01/26 arc yano #3406_サービス伝票　引当時のvalidaionエラーメッセージ出力の抑制 (#3397_【大項目】部品仕入機能改善 課題管理表対応) 引当チェック時の引数追加
        /// </history>
        private void ValidateHikiate(ServiceSalesLine line, int index, ref bool displayFlag, decimal? provisionQuantity)
        {
            string strName = string.Format("line[{0}].StockStatus", index);

            //decimal? requireQuantity = (line.Quantity ?? 0) - (line.ProvisionQuantity ?? 0);                    //必要数量  //Add 2016/01/26 arc yano
            decimal? requireQuantity = (line.Quantity ?? 0) - (provisionQuantity ?? 0);                           //必要数量  //Mod 2017/01/31 arc yano #3566 

            bool ret = new PartsDao(db).IsInventoryParts(line.PartsNumber);                                     //Mod 2016/01/26 arc yano #3453

            //判断＝「在庫」の場合で、在庫数 < 数量の場合、エラーメッセージを表示
            if (line.StockStatus.Equals("999") && (requireQuantity > line.PartsStock) && (ret))        
            {
                if (displayFlag == false)
                {
                    ModelState.AddModelError(strName, "在庫がありません。判断は発注を選択してください");       //Mod 2016/01/26 arc yano
                    displayFlag = true;
                }
                else
                {
                    ModelState.AddModelError(strName, "");       //Mod 2016/01/26 arc yano
                }
            }
        }
        #endregion

        #region　引き継ぎメモだけを更新するとき(納車済みの時)のvalidationチェック
        //Add 2015/08/05 arc nakayama #3221_無効となっている部門や車両等が設定されている納車確認書が印刷出来ない
        /// <summary>
        /// 引き継ぎメモだけを更新するとき(納車済みの時)のvalidationチェック
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/05/24 arc yano #3896 サービス伝票入力　納車後の納車確認書が印刷できない
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void ValidateForMemoUpdate(ServiceSalesHeader header)
        {
            int ret = 0;

            CommonValidate("SalesDate", "納車日", header, true);   //Add 2018/05/24 arc yano #3896

            //　編集前と差分があったらチェック
            if (header.SalesDate != new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber).SalesDate)
            {
                //Mod 2016/08/13 arc yano #3596
                //部門コードから使用倉庫を割出す
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
  
                ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                
                //ret = CheckTempClosedEdit(header.DepartmentCode, header.SalesDate);
            }
            switch (ret)
            {
                //部品棚卸済による編集不可
                case 1:
                    ModelState.AddModelError("SalesDate", "部品棚卸処理が実行されているため、指定された納車日では納車できません");
                    break;

                //月次締済による編集不可
                case 2:
                    ModelState.AddModelError("SalesDate", "月次締処理が実行されているため、指定された納車日では納車できません");
                    break;

                default:
                    //何もしない
                    break;
            }

            //引き継ぎメモの文字数
            if (header.Memo != null && header.Memo.Length > 400)
            {
                ModelState.AddModelError("Memo", "引き継ぎメモは400文字以内で入力して下さい（改行は2文字分とみなされます）");
            }
        }
        #endregion
        #region 画面表示用処理
        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <history>
        /// 
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// 2018/02/23 arc yano #3471 サービス伝票　区分の絞込みの対応 区分は種別による絞り込みを行う
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 マスタ未登録により発注できない部品を表示する
        /// 2017/02/03 arc yano #3426 サービス伝票・伝票修正時　発注画面表示の不具合
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2016/05/09 arc yano #3480 サービス伝票の請求先を主作業の内容により切り分ける 請求先分類を設定する処理の追加
        /// 2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応 区分は種別による絞り込みを行う →2016/04/19 不具合発生のため、削除
        /// 2016/01/28 arc yano #3401 サービス伝票　代替品で分納時の明細行のソート処理の追加　
        /// 2015/10/28 arc yano #3289 部品仕入機能改善(サービス伝票入力)
        /// </history>
        /// <param name="salesCar">モデルデータ</param>
        private void SetDataComponent(ref ServiceSalesHeader header) {
            ViewData["displayContents"] = "main";
            // Mod 2015/05/18 arc nakayama ルックアップ見直し対応　無効データも表示はさせる(FrontEmployee/ReceiptionEmployee/Department/Customer)
            header.FrontEmployee = new EmployeeDao(db).GetByKey(header.FrontEmployeeCode, true);
            header.ReceiptionEmployee = new EmployeeDao(db).GetByKey(header.ReceiptionEmployeeCode, true);
            header.Department = new DepartmentDao(db).GetByKey(header.DepartmentCode, true);
            header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);
            header.CustomerClaim = new CustomerClaimDao(db).GetByKey(header.CustomerClaimCode, true);
            header.c_ServiceOrderStatus = new CodeDao(db).GetServiceOrderStatusByKey(header.ServiceOrderStatus);

            //Add 2016/01/28 arc yano #3401 明細行を画面表示順に並び替え
            EntitySet<ServiceSalesLine> wkEntity = new EntitySet<ServiceSalesLine>();
            wkEntity.AddRange(header.ServiceSalesLine.OrderBy(x => x.DisplayOrder).ThenBy(x => x.LineNumber));
            header.ServiceSalesLine = wkEntity;


            CodeDao dao = new CodeDao(db);

            PartsStockDao pDao = new PartsStockDao(db);

            //ADD 2014/02/21 ookubo
            if (header.ConsumptionTaxId == null || header.ConsumptionTaxId == "")
            {
                if (header.SalesDate != null)
                {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(header.SalesDate);
                }
                else
                {
                    if (header.SalesPlanDate != null)
                    {
                        header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(header.SalesPlanDate);
                    }
                    else
                    {
                        header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    }
                }
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), header.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = header.ConsumptionTaxId;
            ViewData["Rate"] = header.Rate;
            ViewData["ConsumptionTaxIdOld"] = header.ConsumptionTaxId;
            ViewData["SalesDateOld"] = header.SalesDate;
            ViewData["SalesPlanDateOld"] = header.SalesPlanDate;
            //ADD end
            
            ViewData["KeepsCarFlag"] = header.KeepsCarFlag;   //ADD 2014/11/25 #3135 ookubo

            //Add 2020/02/17 yano #4025
            if ((header.TaxableCostTotalAmount ?? 0m) != 0m)
            {
                //諸費用（課税）の消費税算出
                header.TaxableCostSubTotalTaxAmount = Math.Truncate((header.TaxableCostTotalAmount ?? 0m) * (decimal)(header.Rate / (100 + header.Rate)));
                //諸費用（課税）の税抜
                header.TaxableCostSubTotalAmount = (header.TaxableCostTotalAmount ?? 0m) - header.TaxableCostSubTotalTaxAmount;
            }
            else
            {
                //諸費用（課税）の消費税算出
                header.TaxableCostSubTotalTaxAmount = 0m;
                //諸費用（課税）の税抜
                header.TaxableCostSubTotalAmount = 0m;
            }
            
            List<IEnumerable<SelectListItem>> serviceTypeList = new List<IEnumerable<SelectListItem>>();
            List<c_ServiceType> serviceTypeListSrc = dao.GetServiceTypeAll(true);
            List<IEnumerable<SelectListItem>> workTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> stockStatusList = new List<IEnumerable<SelectListItem>>();
            //Mod 2015/10/28 2015/10/28 arc yano #3289 サービス伝票 純正区分により、在庫判断リストの中身を変える。この段階は初期化のみ行う
            List<c_StockStatus> stockStatusListSrc = new List<c_StockStatus>();
            //List<c_StockStatus> stockStatusListSrc = dao.GetStockStatusAll(false);
            List<IEnumerable<SelectListItem>> lineTypeList = new List<IEnumerable<SelectListItem>>();
            List<CodeData> lineTypeListSrc = CodeUtils.GetLineTypeList();
            foreach (var line in header.ServiceSalesLine) {
                List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false);

                //Mod 2018/02/23 arc yano #3741
                //種別による絞込
                //種別 = サービスメニュー
                if (!string.IsNullOrWhiteSpace(line.ServiceType) && line.ServiceType.Equals("002"))
                {
                    workTypeListSrc = workTypeListSrc.Where(x => !string.IsNullOrWhiteSpace(x.ServiceMenuUse) && x.ServiceMenuUse.Equals("1")).ToList();

                }
                else if (!string.IsNullOrWhiteSpace(line.ServiceType) && line.ServiceType.Equals("003"))
                {
                    workTypeListSrc = workTypeListSrc.Where(x => !string.IsNullOrWhiteSpace(x.PartsUse) && x.PartsUse.Equals("1")).ToList();
                }

                //List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false, line.ServiceType);   //Mod 2016/03/17 arc yano   #3471 　→2016/04/19 不具合のため、削除
                serviceTypeList.Add(CodeUtils.GetSelectListByModel(serviceTypeListSrc, line.ServiceType, false));
                line.CustomerClaim = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode);
                // Mod 2015/05/21 arc nakayama #3186_サービス伝票明細のメカニック欄が表示されない　表示するときはDelFlagを考慮しない
                line.Employee = new EmployeeDao(db).GetByKey(line.EmployeeCode, true);
                workTypeList.Add(CodeUtils.GetSelectListByModel(workTypeListSrc, line.WorkType, true));
                //Add 2015/10/28 2015/10/28 arc yano #3289 サービス伝票 純正区分により、在庫判断リストの中身を変える

                if (!string.IsNullOrWhiteSpace(line.PartsNumber))
                {
                    Parts parts = new PartsDao(db).GetByKey(line.PartsNumber);

                    if (parts != null)
                    {
                        //Mod 2016/02/05 arc nakayama #3427_サービス伝票入力画面　部品番号未入力時の判断表示 初期値は社外品
                        //部品マスタに純正区分が設定されていない場合は、デフォルト「社外品」で設定しておく
                        stockStatusListSrc = dao.GetStockStatusAll(false);         //Mod 2017/10/19 arc yano #3803

                        //Add 2017/02/03 arc yano #3426
                        if (service.CheckModification(header.SlipNumber, header.RevisionNumber))   //納車済の場合
                        {
                            stockStatusListSrc = stockStatusListSrc.Where(x => !x.StatusType.Equals("001")).ToList();
                        }

                    }

                    //Mod 2016/08/13 arc yano #3596
                    //部門コードから使用倉庫を割出す
                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                    line.PartsStock = pDao.GetStockQuantity(line.PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));   //Add 2015/10/28 arc yano #3289

                }
                else //Add 2015/10/28 2015/10/28 arc yano #3289
                {
                    //Mod 2016/02/05 arc nakayama #3427_サービス伝票入力画面　部品番号未入力時の判断表示 初期値は社外品
                    stockStatusListSrc = dao.GetStockStatusAll(false); //Mod 2017/10/19 arc yano #3803

                    //Add 2017/02/03 arc yano #3426
                    if (service.CheckModification(header.SlipNumber, header.RevisionNumber))   //納車済の場合
                    {
                        stockStatusListSrc = stockStatusListSrc.Where(x => !x.StatusType.Equals("001")).ToList();
                    }
                }
                stockStatusList.Add(CodeUtils.GetSelectListByModel(stockStatusListSrc, line.StockStatus, false));
                line.ServiceTypeName = dao.GetServiceTypeByKey(line.ServiceType).Name;
                line.Supplier = new SupplierDao(db).GetByKey(line.SupplierCode);
                lineTypeList.Add(CodeUtils.GetSelectListByModel(lineTypeListSrc, line.LineType, false));

                //Add 2016/05/09 arc yano #3480
                //請求先区分(主作業)の設定
                if (line.ServiceWork != null)
                {
                    line.SWCustomerClaimClass = line.ServiceWork.CustomerClaimClass;
                }
                //請求先区分(請求先)の設定
                if (line.CustomerClaim != null && line.CustomerClaim.c_CustomerClaimType != null)
                {
                    line.CCCustomerClaimClass = line.CustomerClaim.c_CustomerClaimType.CustomerClaimClass;
                }

                //Add 2017/10/19 arc yano #3803
                
                //伝票ステータス=受注～納車確認書印刷済み
                if (!string.IsNullOrWhiteSpace(header.ServiceOrderStatus) && (header.ServiceOrderStatus.Equals("002") || header.ServiceOrderStatus.Equals("003") || header.ServiceOrderStatus.Equals("004") || header.ServiceOrderStatus.Equals("005")))
                {
                    //判断にS/Oなどの発注が選ばれている
                    if (line.c_StockStatus != null && (line.c_StockStatus.StatusType.Equals("001") || line.c_StockStatus.StatusType.Equals("010")))
                    {
                        //部品番号が部品マスタに登録されていない場合
                        if (line.Parts == null && !string.IsNullOrWhiteSpace(line.DelFlag) && !line.DelFlag.Equals("1"))
                        {
                            ViewData["UnregisteredPartsList"] += ("明細" + line.LineNumber + "行目の部品【部品番号=" + line.PartsNumber + "】はマスタ未登録のため発注画面には表示されません<br/>");   //Add 2017/02/03 arc yano #3426
                        }
                    }
                }

            }
            for (int i = 0; i < header.ServiceSalesPayment.Count; i++) {
                header.ServiceSalesPayment[i].CustomerClaim = new CustomerClaimDao(db).GetByKey(header.ServiceSalesPayment[i].CustomerClaimCode);
            }
            ViewData["ServiceTypeLineList"] = serviceTypeList;
            ViewData["MileageUnit"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), header.MileageUnit, false);
            ViewData["ServiceTypeList"] = CodeUtils.GetSelectListByModel<c_ServiceType>(dao.GetServiceTypeAll(true), header.ServiceType, false);
            ViewData["WorkTypeList"] = workTypeList;
            ViewData["StockStatus"] = stockStatusList;
            ViewData["LineTypeList"] = lineTypeList;

            ViewData["JournalTotalAmount"] = 0m;
            if (!string.IsNullOrEmpty(header.SlipNumber)) {
                // Mod 2015/11/02 arc nakayama #3297_サービス伝票入力画面の入金実績設定値の不具合
                List<Journal> journalList = new JournalDao(db).GetJournalCalcListBySlipNumber(header.SlipNumber);
                ViewData["JournalTotalAmount"] = journalList != null ? journalList.Sum(x => x.Amount) : 0m;
            }
            header.BasicHasErrors = BasicHasErrors();
            header.InvoiceHasErrors = InvoiceHasErrors();
            header.TaxHasErrors = TaxHasErrors();

            //Mod 2020/02/17 yano #4025
            // 諸費用の合計を預かり金として作成
            decimal depositTotal = (header.CarTax ?? 0m) + (header.CarWeightTax ?? 0m) + (header.CarLiabilityInsurance ?? 0m) +
                               (header.NumberPlateCost ?? 0m) + (header.FiscalStampCost ?? 0m) + (header.TaxFreeFieldValue ?? 0m) +
                               (header.OptionalInsurance ?? 0m) + (header.SubscriptionFee ?? 0m) + (header.TaxableFreeFieldValue ?? 0m);
      //decimal depositTotal = (header.CarTax ?? 0m) + (header.CarWeightTax ?? 0m) + (header.CarLiabilityInsurance ?? 0m) +
      //                    (header.NumberPlateCost ?? 0m) + (header.FiscalStampCost ?? 0m) + (header.TaxFreeFieldValue ?? 0m);


      //明細を請求先ごとに合計金額を集計
      var query = from a in header.ServiceSalesLine
                    //  Edit 2014/06/12 arc yano 高速化対応 CliDelFlagが1の行は集計しない
                    //where (a.ServiceType.Equals("002") || a.ServiceType.Equals("003"))
                  where ((a.ServiceType.Equals("002") || a.ServiceType.Equals("003")) && (string.IsNullOrEmpty(a.CliDelFlag) || (!(string.IsNullOrEmpty(a.CliDelFlag)) && !(a.CliDelFlag.Equals("1")))))
                  group a by new { CustomerClaimCode = a.CustomerClaimCode, ServiceWorkCode = a.ServiceWorkCode } into customerClaim
                  select new
                  {
                    customerClaim.Key,
                    TechnicalFeeAmount = customerClaim.Sum(y => service.IsDiscountRecord(y.ServiceMenuCode) ? (-1) * y.TechnicalFeeAmount : (y.TechnicalFeeAmount ?? 0m)),
                    PartsAmount = customerClaim.Sum(x => service.IsDiscountRecord(x.PartsNumber) ? (-1) * x.Amount : (x.Amount ?? 0m)),
                    TaxAmount = customerClaim.Sum(x => (service.IsDiscountRecord(x.PartsNumber) || service.IsDiscountRecord(x.ServiceMenuCode)) ? (-1) * x.TaxAmount : (x.TaxAmount ?? 0m)),
                    TechnicalCost = customerClaim.Sum(x => x.ServiceType.Equals("002") ? x.Cost : 0m),
                    PartsCost = customerClaim.Sum(x => x.ServiceType.Equals("003") ? x.Cost : 0m)
                  };

      header.ServiceClaimable = new List<ServiceClaimable>();
            // 顧客・請求先がセットされている場合のみ
            if (!string.IsNullOrEmpty(header.CustomerCode) && header.ServiceSalesLine.Where(x => x.CustomerClaimCode != null).Count() > 0) {
                bool ExistCustomerCode = false;
                //Add 2023/05/01 openwave #xxxx
                string CustomerClaimCode = header.CustomerCode;
                if (!string.IsNullOrEmpty(header.CustomerClaimCode)) {
                    CustomerClaimCode = header.CustomerClaimCode;
                }
                foreach (var item in query) {
                    if (item.Key.CustomerClaimCode != null) {
                        ServiceClaimable serviceClaimable = new ServiceClaimable();
                        serviceClaimable.CustomerClaimCode = item.Key.CustomerClaimCode;
                        serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(item.Key.CustomerClaimCode);
                        serviceClaimable.ServiceWorkCode = item.Key.ServiceWorkCode;
                        serviceClaimable.ServiceWork = new ServiceWorkDao(db).GetByKey(item.Key.ServiceWorkCode);
                        serviceClaimable.TechnicalFeeAmount = item.TechnicalFeeAmount ?? 0m;
                        serviceClaimable.PartsAmount = item.PartsAmount ?? 0m;
                        serviceClaimable.Amount = (item.TechnicalFeeAmount ?? 0m) + (item.PartsAmount ?? 0m);
                        serviceClaimable.AmountWithTax = serviceClaimable.Amount + (item.TaxAmount ?? 0m);
                        serviceClaimable.TechnicalCost = item.TechnicalCost ?? 0m;
                        serviceClaimable.PartsCost = item.PartsCost ?? 0m;
                        serviceClaimable.Cost = (item.TechnicalCost ?? 0m) + (item.PartsCost ?? 0m);
                        serviceClaimable.TechnicalMargin = serviceClaimable.TechnicalFeeAmount - serviceClaimable.TechnicalCost;
                        serviceClaimable.PartsMargin = serviceClaimable.PartsAmount - serviceClaimable.PartsCost;
                        serviceClaimable.Margin = serviceClaimable.Amount - serviceClaimable.Cost;
                        try {
                            serviceClaimable.MarginRate = Math.Round(Decimal.Divide(serviceClaimable.Margin, serviceClaimable.Amount) * 100, 1);
                        } catch {
                            serviceClaimable.MarginRate = 0;
                        }
                        //Mod 2023/05/01 openwave #xxxx
                        //if (item.Key.CustomerClaimCode.Equals(header.CustomerCode)) {
                        if (item.Key.CustomerClaimCode.Equals(CustomerClaimCode)) {
                            serviceClaimable.DepositAmount = depositTotal;
                            ExistCustomerCode = true;
                        }

                        header.ServiceClaimable.Add(serviceClaimable);
                    }
                }

                if (!ExistCustomerCode && depositTotal > 0) {
                    ServiceClaimable serviceClaimable = new ServiceClaimable();
                    //Mod 2023/05/01 openwave #xxxx
                    //serviceClaimable.CustomerClaimCode = header.CustomerCode;
                    //serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(header.CustomerCode);
                    serviceClaimable.CustomerClaimCode = CustomerClaimCode;
                    serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(CustomerClaimCode);
                    serviceClaimable.ServiceWorkCode = "";
                    serviceClaimable.ServiceWork = null;
                    serviceClaimable.TechnicalFeeAmount = 0;
                    serviceClaimable.PartsAmount = 0;
                    serviceClaimable.Amount = 0;
                    serviceClaimable.AmountWithTax = 0;
                    serviceClaimable.TechnicalCost = 0;
                    serviceClaimable.PartsCost = 0;
                    serviceClaimable.Cost = 0;
                    serviceClaimable.TechnicalMargin = 0;
                    serviceClaimable.PartsMargin = 0;
                    serviceClaimable.Margin = 0;
                    serviceClaimable.MarginRate = 0;
                    serviceClaimable.DepositAmount = depositTotal;
                    header.ServiceClaimable.Add(serviceClaimable);
                }
            }

            //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 顧客情報欄に表示する住所再確認メッセージを設定する
            ViewData["ReconfirmMessage"] = "";

            if (header.Customer != null && header.Customer.AddressReconfirm == true)
            {
                // 住所再確認フラグが立っていた場合、メッセージを設定する
                ViewData["ReconfirmMessage"] = "住所を再確認してください";
            }

            //Add 2017/10/19 arc yano #3803
            //validationエラーの場合は、発注書、出庫伝票は出力しない
            if (!ModelState.IsValid)
            {
                header.Output = false;
            }
        }
        #endregion
        
        #region タブごとのエラー仕訳
        /// <summary>
        /// タブごとのエラー仕訳
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// </history>
        private bool BasicHasErrors() {
            var query = from a in ModelState
                        where (!a.Key.StartsWith("pay[")
                        && !a.Key.Equals("")
                        && !a.Key.Equals("PaymentTotalAmount")
                        && !a.Key.Equals("CarTax")
                        && !a.Key.Equals("CarLiabilityInsurance")
                        && !a.Key.Equals("CarWeightTax")
                        && !a.Key.Equals("NumberPlateCost")
                        && !a.Key.Equals("FiscalStampCost")
                        && !a.Key.Equals("TaxFreeFieldName")
                        && !a.Key.Equals("TaxFreeFieldValue")
                        && !a.Key.Equals("OptionalInsurance")           //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("SubscriptionFee")             //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("TaxableFreeFieldValue")       //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("TaxableFreeFieldName"))       //Add 2020/02/17 yano #4025

                        && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool InvoiceHasErrors() {
            var query = from a in ModelState
                        where (a.Key.StartsWith("pay[") || a.Key.Equals("PaymentTotalAmount"))
                        && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// タブごとのエラー仕訳
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// </history>
        private bool TaxHasErrors() {
            if (ModelState["CarTax"] != null && ModelState["CarTax"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["CarLiabilityInsurance"] != null && ModelState["CarLiabilityInsurance"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["CarWeightTax"] != null && ModelState["CarWeightTax"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["NumberPlaceCost"] != null && ModelState["NumberPlateCost"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["FiscalStampCost"] != null && ModelState["FiscalStampCost"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["TaxFreeFieldName"] != null && ModelState["TaxFreeFieldName"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["TaxFreeFieldValue"] != null && ModelState["TaxFreeFieldValue"].Errors.Count() > 0) {
                return true;
            }
            //Add 2020/02/17 yano #4025
            if (ModelState["OptionalInsurance"] != null && ModelState["OptionalInsurance"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["SubscriptionFee"] != null && ModelState["SubscriptionFee"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["TaxableFreeFieldValue"] != null && ModelState["TaxableFreeFieldValue"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["TaxableFreeFieldName"] != null && ModelState["TaxableFreeFieldName"].Errors.Count() > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 赤黒処理
        /// <summary>
        /// 赤黒検索画面表示
        /// </summary>
        /// <returns></returns>
        [AkakuroAuthFilter]
        public ActionResult ModifyCriteria() {
            criteriaInit = true;
            return ModifyCriteria(new FormCollection());
        }
        /// <summary>
        /// 赤黒検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ModifyCriteria(FormCollection form) {
            form["DelFlag"] = "0";
            form["ServiceOrderStatus"] = "006";
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["DepartmentName"] = "";
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            PaginatedList<ServiceSalesHeader> list;

            //Mod 2015/05/07 arc yano 仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
            int ret = 0;

            if (criteriaInit) {
                list = new PaginatedList<ServiceSalesHeader>();
            } else {
                list = GetSearchResultListModeAkaden(form);
            }
            foreach (var item in list) {
                /*
                // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(item.DepartmentCode, item.SalesDate, "002")){
                    // 納車日が棚卸締め未処理だったら赤黒対象外
                    item.IsClosed = true;
                }
                */

                //Mod 2016/08/13 arc yano #3596
                //Mod 2015/05/07 arc yano 仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                //部門コードから使用倉庫を割出す
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, item.DepartmentCode);
                ret = CheckTempClosedEdit(dWarehouse, item.SalesDate);
                //ret = CheckTempClosedEdit(item.DepartmentCode, item.SalesDate);
                
                if (ret != 0)
                {
                    item.IsClosed = true;
                }

                if (new ServiceSalesOrderDao(db).GetBySlipNumber(item.SlipNumber + "-1") != null) {
                    // 赤伝処理、赤黒処理していなかったら対象
                    item.IsCreated = true;
                }
            }
            return View("ServiceSalesOrderModifyCriteria", list);
        }
        /// <summary>
        /// 赤黒検索処理
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        private PaginatedList<ServiceSalesHeader> GetSearchResultListModeAkaden(FormCollection form) {
            form["WithoutAkaden"] = "0";
            return GetSearchResultList(form);
        }
        /// <summary>
        /// 赤伝処理
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akaden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            //Add 2014/06/17 arc yano 高速化対応
            //削除対象行のデータ削除
            Delline(ref line);

            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            // 赤伝処理は理由が必須
            ViewData["Mode"] = form["Mode"];
            ValidateForAkaden(header, line);
            if (ModelState.IsValid)
            {

                // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();
                stockService = new StockService(db);
                service = new ServiceSalesOrderService(db);

                
                service.ProcessUnLock(header);

                ServiceSalesHeader history2 = new ServiceSalesHeader();

                //Mod 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法 途中でSubmitChangesをかけるためトランザクションスコープ追加
                //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
                double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
                {
                    // 赤伝処理
                    ServiceSalesHeader history = service.CreateAkaden(header);
                    CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "サービス", header.Reason);

                    //Mod 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
                    //入金があった場合、入金実績分の返金の入金予定作成
                    service.CreateBackAmountAkaden(header.SlipNumber);

                    //Add 2014/08/08 arc amii エラーログ対応 ログ出力を行う為、try catch文を追加
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        history2 = history;
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //// ログに出力
                            OutputLogger.NLogError(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);

                            string message = "帳票番号=" + header.SlipNumber + "のデータは、";
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0023", message));

                            //ステータスを戻す
                            header.ServiceOrderStatus = form["PreviousStatus"];
                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }

                    ViewData["CompleteMessage"] = "伝票番号「" + header.SlipNumber + "（元伝票）」の赤伝「" + history.SlipNumber + "（新番号）」処理が正常に完了しました。";
                    ViewData["Mode"] = null;
                    ModelState.Clear();
                }
                // 表示項目の再セット
                SetDataComponent(ref history2);
                return GetViewResult(history2);
            }
            // 表示項目の再セット
            //Add 2014/06/10 arc yano 高速化対応 
            Sortline(ref line);
            header.ServiceSalesLine = line;

            SetDataComponent(ref header);
            return GetViewResult(header);
        }

        /// <summary>
        /// 赤黒処理
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/03 arc yano #3732 サービス伝票 赤黒伝票作成で「入力されているメカ担当者はマスタに存在していません 」表示 引数追加
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更
        /// </history>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akakuro(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            // 赤黒処理は理由が必須
            ViewData["Mode"] = form["Mode"];

            //Add 2014/06/17 arc yano lineをlineNumber順にソートする。
            ModelState.Clear();
            //明細行をlineNumber順に並び替え
            Sortline(ref line);

            //Add 2014/06/17 arc yano 高速化対応
            //Mod 2015/10/15 arc nakayama #3264_サービス伝票のコピーで再利用した伝票で有効なメカ担当者修正後「入力されているメカ担当者はマスタに存在していません 」表示 バリデーションチェックの前に移動
            //削除対象行の削除
            Delline(ref line);

            ValidateForAkaden(header, line);
            ValidateAllStatus(header, line, form);      //Mod 2017/11/03 arc yano #3732

            if (!ModelState.IsValid) {
                
                service.ProcessUnLock(header);

                //値引を税込に変換
                service.SetDiscountAmountWithTax(line);

                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;

                // 表示項目の再セット
                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            

            // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);


            //Mod 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法 途中でSubmitChangesをかけるためトランザクションスコープ追加
            //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {

                // 赤黒処理
                CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "サービス", header.Reason);

                ServiceSalesHeader original = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);

                // 赤伝票作成
                ServiceSalesHeader history = service.CreateAkaden(original);

                // 黒伝票作成
                header = service.CreateKuroden(header, line);       //2015/10/28 arc yano #3289 戻り値を受け取る
                //db.ServiceSalesHeader.InsertOnSubmit(header);

                //Add 2014/08/08 arc amii エラーログ対応 ログ出力を行う為、try catch文を追加
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //// ログに出力
                        OutputLogger.NLogError(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);

                        string message = "帳票番号=" + header.SlipNumber + "のデータは、";
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0023", message));

                        //ステータスを戻す
                        header.ServiceOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                    return View("Error");
                }

                ViewData["CompleteMessage"] = "伝票番号「" + original.SlipNumber + "（元伝票）」の赤黒「" + header.SlipNumber + "（納車済み伝票のコピー）」処理が正常に完了しました。";
                ViewData["Mode"] = null;
                ModelState.Clear();
            }

            service.ProcessUnLock(header);

            //Add 2014/06/17 arc yano 高速化対応 
            header.ServiceSalesLine.Insert(0, new ServiceSalesLine { ServiceType = "001", CliDelFlag = "1" });

            //値引を税込に変換
            service.SetDiscountAmountWithTax(line);

            // 表示項目の再セット
            SetDataComponent(ref header);

            return GetViewResult(header);

        }

        #endregion

        #region 伝票ロック制御
        /// <summary>
        /// 画面を閉じる際に呼び出される
        /// （伝票ロックを解除する）
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public ActionResult UnLock(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {
            //header.ServiceSalesLine = line;
            //header.ServiceSalesPayment = pay;
            if (header.RevisionNumber > 0) {
                service.ProcessUnLock(header);
            }
            ViewData["close"] = "1";

            //Add 2014/12/24 arc yano #3143 合計値の再計算(SetDataComponent内で実行)の前に値引額を一旦、税抜表示に戻し、再計算後に、再度税込表示に変換する。
            //値引額を税込表示から、税抜表示に変換
            service.SetDiscountAmountWithoutTax(line);

            SetDataComponent(ref header);

            //値引額を税込表示に変換
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            return GetViewResult(header);
        }
        

        #endregion

        //Add 2014/06/17 ardc yano 高速化対応
        #region 不要行の削除
        /// <summary>
        /// 削除対象行(クライアントで削除した行)の削除を行う。
        /// </summary>
        /// <param name="line">モデルデータ</param>
        private void Delline(ref EntitySet<ServiceSalesLine> line)
        {
            //----------------------
            //不要な行の削除
            //----------------------
            int i;
            for (i = line.Count - 1; i >= 0; i--)
            {
                //削除対象行の場合
                if (!string.IsNullOrEmpty(line[i].CliDelFlag) && (line[i].CliDelFlag.Equals("1")))
                {
                    line.RemoveAt(i);
                }
            }
        }
        #endregion

        #region 明細行ソート
        /// <summary>
        /// 明細行のデータセットをlineNumber順に並び替える。
        /// </summary>
        /// <param name="line">モデルデータ</param>
        /// <param name="resline">リストア用データ</param>
        /// <history>
        /// Mod 2016/01/28 arc yano #3401_サービス伝票　代替品で分納時の明細行のソート処理の追加 ソート番号の設定処理の追加
        /// Add 2014/06/17 arc yano 高速化対応 
        /// </history>
        private void Sortline(ref EntitySet<ServiceSalesLine> line)
        {
            int cnt = 1;    //Add 2016/01/28 arc yano #340   
            
            //明細行データをLineNumberでソートする。
            ServiceSalesLine[] restoreline = (from a in line
                                orderby a.LineNumber 
                                select a).ToArray();

            line.Clear();   //明細行データクリア

            foreach (ServiceSalesLine a in restoreline)
            {
                a.DisplayOrder = cnt++; //Add 2016/01/28 arc yano #340    
                line.Add(a);
            }
        }
        #endregion

        #region 発注フラグの変換
        /// <summary>
        /// 明細行のデータセットをlineNumber順に並び替える。
        /// </summary>
        /// <param name="line">モデルデータ</param>
        /// <param name="resline">リストア用データ</param>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成 
        /// </history>
        private void SetOutputTragetFlag (ref EntitySet<ServiceSalesLine> line)
        {
            foreach (var l in line)
            {
                //チェックが入っている場合
                if (l.OutputTarget.ToLower().Contains("true"))
                {
                    l.OutputTargetFlag = "1";
                }
                else
                {
                    l.OutputTargetFlag = "0";
                }
            }
        }
        #endregion

        //Del 2015/07/31 arc yano #3231 サービス伝票のFAワランティチェックを外す
        //Add 2014/07/03 ardc yano サービス伝票チェック新システム
        #region ワランティチェック
        /// <summary>
        /// 明細行のワランティチェックを行う。
        /// </summary>
        /// <param name="totalTechFee">技術料合計</param>
        /// <param name="totalAmount">金額合計</param>
        /// <param name="totalSmCost">原価(合計)/サービスメニュー</param>
        /// <param name="totalPtCost">原価(合計)/部品</param>
        /// <param name="posServiceWork">エラー対象レコード</param>
        /// </summary>
        private void ValidateWaranty(ref decimal totalTechFee, ref decimal totalAmount, ref decimal totalSmCost, ref decimal totalPtCost, int posServiceWork)
        {
            /*
            int adderr = 0;     //エラーメッセージ追加フラグ

            //------------------------------------------
            //サービスメニューでのチェック
            //------------------------------------------
            if (totalTechFee != totalSmCost)       //技術料≠原価(合計)/サービスメニュー
            {
                adderr = 1;
            }
            //------------------------------------------
            //部品でのチェック
            //------------------------------------------
            if (totalAmount != totalPtCost)       //部品≠原価(合計)/部品
            {
                adderr = 1;
            }


            if (adderr == 1)
            {
                if (!ModelState.ContainsKey(string.Format("line[{0}].{1}", posServiceWork, "ServiceWorkCode")))   //主作業コードに対して検証エラー未発生の場合。
                {
                    ModelState.AddModelError(string.Format("line[{0}].{1}", posServiceWork, "ServiceWorkCode"), "");
                }

                ModelState.AddModelError("", MessageUtils.GetMessage("E0022", new string[] { "主作業がワランティまたはリコール", "技術料の合計と原価(合計)の合計、または金額の合計と原価(合計)の合計を一致させる必要があります" }));
            }
                
            //リセット
            totalTechFee = 0;
            totalSmCost = 0;
            totalAmount = 0;
            totalPtCost = 0;
            */
        }
        #endregion

        //Add 2015/10/28 arc yano #3289 サービス伝票 発注情報の作成方法の変更
        #region 発注画面url作成
        /// <summary>
        /// 明細行のワランティチェックを行う。
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ情報</param>
        /// <param name="orderList">発注情報</param>
        /// </summary>
        private ServiceSalesHeader makeOrderUrl(ServiceSalesHeader header, List<PartsPurchaseOrder> orderList)
        { 
            header.orderUrlList = new List<string>();

            string strUrl = "";

            //--------------------------------
            //純正品のURL作成
            //--------------------------------
            //オーダー区分リストの取得
            var oTypeList = orderList.GroupBy(x => x.OrderType).Select(x => new { key = x.Key });

            int cnt = 0;

            foreach (var oType in oTypeList)
            {
                cnt = 0;

                strUrl = "/PartsPurchaseOrder/Entry";       //リクエスト先の設定

                //オーダー区分で発注情報を絞込み
                var oList = orderList.Where(x => (x.OrderType.Equals(oType.key) && (x.GenuineType ?? "").Equals(TYPE_GENUINE))).GroupBy(x => new { x.PartsNumber, x.ServiceSlipNumber, x.DepartmentCode, x.OrderType }).Select
                    (x => new
                    {
                          PartsNumber = x.Key.PartsNumber
                        , ServiceSlipNumber = x.Key.ServiceSlipNumber
                        , DepartmentCode = x.Key.DepartmentCode
                        , OrderType = x.Key.OrderType
                        , Quantity = x.Sum(y => y.Quantity)
                    }).OrderBy(x => x.PartsNumber).ToList();

                if (oList.Count > 0)
                {
                    strUrl += "?ServiceSlipNumber=" + oList[cnt].ServiceSlipNumber + "&DepartmentCode=" + oList[cnt].DepartmentCode + "&OrderType=" + oList[cnt].OrderType;
                    foreach (var order in oList)
                    {
                        strUrl += "&partsList[" + cnt + "].PartsNumber=" + order.PartsNumber + "&partsList[" + cnt + "].Quantity=" + order.Quantity;
                        cnt++;
                    }

                    header.orderUrlList.Add(strUrl);
                }
            }

            //--------------------------------
            //社外品のURL作成
            //--------------------------------
            //オーダー区分リストの取得
            var oTypeListNonGenuine = orderList.GroupBy(x => x.OrderType).Select(x => new { key = x.Key });

            foreach (var oType in oTypeListNonGenuine)
            {
                cnt = 0;

                strUrl = "/PartsPurchaseOrderNonGenuine/Entry";       //リクエスト先の設定

                //オーダー区分で発注情報を絞込み
                var oListNonGenuine = orderList.Where(x => (x.OrderType.Equals(oType.key) && (x.GenuineType ?? "").Equals(TYPE_NONGENUINE))).GroupBy(x => new { x.PartsNumber, x.ServiceSlipNumber, x.DepartmentCode, x.OrderType }).Select
                    (x => new
                    {
                        PartsNumber = x.Key.PartsNumber
                        ,
                        ServiceSlipNumber = x.Key.ServiceSlipNumber
                        ,
                        DepartmentCode = x.Key.DepartmentCode
                        ,
                        OrderType = x.Key.OrderType
                        ,
                        Quantity = x.Sum(y => y.Quantity)
                    }).OrderBy(x => x.PartsNumber).ToList();


                if (oListNonGenuine.Count > 0)
                {
                    strUrl += "?ServiceSlipNumber=" + oListNonGenuine[cnt].ServiceSlipNumber + "&DepartmentCode=" + oListNonGenuine[cnt].DepartmentCode + "&OrderType=" + oListNonGenuine[cnt].OrderType;
                    foreach (var order in oListNonGenuine)
                    {
                        strUrl += "&partsList[" + cnt + "].PartsNumber=" + order.PartsNumber + "&partsList[" + cnt + "].Quantity=" + order.Quantity;
                        cnt++;
                    }

                    header.orderUrlList.Add(strUrl);
                }
            }
            
            return header;
        }
        #endregion

        #region Ajax専用
        
        /// <summary>
        /// 判断一覧を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>判断一覧</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書出力
        /// 2017/02/03 arc yano #3426 サービス伝票・伝票修正時　発注画面表示の不具合
        /// 2015/10/28 arc yano #3289 サービス伝票 Ajaxにて在庫判断の一覧を取得する
        /// </history>
        public ActionResult GetMasterList(string code, string slipnumber, int revisionnumber)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();

                var stcokList = new CodeDao(db).GetStockStatusAll(false); //Mod 2017/10/19 arc yano #3803

                //Mod 2017/10/19 arc yano #3803
                //Add 2017/02/03 arc yano #3426
                if (service.CheckModification(slipnumber, revisionnumber))   //伝票修正中の場合
                {
                    stcokList = stcokList.Where(x => !x.StatusType.Equals("001") && !x.StatusType.Equals("010")).ToList();
                }

                foreach (var rec in stcokList)
                {
                    CodeData codeData = new CodeData();

                    codeData.Code = rec.Code;
                    codeData.Name = rec.Name;
                    codeDataList.DataList.Add(codeData);
                }

                return Json(codeDataList);
            }
            return new EmptyResult();
        }

        // 
        /// <summary>
        /// 対象の明細行の部品が発注済かどうかを返す(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>発注数</returns>
        /// <history>
        /// Add 2016/02/12 arc yano #3429 サービス伝票　判断の活性／非活性の制御の追加
        /// </history>
        public ActionResult IsOrdered(string code, int lineNumber)
        {
            if (Request.IsAjaxRequest())
            {

                bool ret = false;       //未発注

                //該当の明細行を取得
                ServiceSalesLine line = new ServiceSalesOrderDao(db).GetBySlipNumber(code).ServiceSalesLine.Where(x => x.LineNumber.Equals(lineNumber)).FirstOrDefault();

                //注文数が1つでもあった場合は発注済
                if (line != null && line.OrderQuantity > 0)     //Mod 2017/04/24 arc yano 
                {
                    ret = true;        //発注済
                }

                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 区分一覧を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">種別</param>
        /// <returns>区分一覧</returns>
        /// <history>
        /// 2018/02/22 arc yano #3471 サービス伝票　区分の絞込みの対応　再更新
        /// 2016/03/17 arc yano #3471 サービス伝票　区分の絞込みの対応
        /// </history>
        public ActionResult GetWorkTypeList(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();

                var workTypeList = new CodeDao(db).GetWorkTypeAll(false);

                //種別による絞込

                //種別 = サービスメニュー
                if (!string.IsNullOrWhiteSpace(code) && code.Equals("002"))
                {
                    workTypeList = workTypeList.Where(x => !string.IsNullOrWhiteSpace(x.ServiceMenuUse) && x.ServiceMenuUse.Equals("1")).ToList();

                }
                else if (!string.IsNullOrWhiteSpace(code) && code.Equals("003"))
                {
                    workTypeList = workTypeList.Where(x => !string.IsNullOrWhiteSpace(x.PartsUse) && x.PartsUse.Equals("1")).ToList();
                }

                //空白の項目を追加
                CodeData DefcodeData = new CodeData();
                DefcodeData.Code = "";
                DefcodeData.Name = "";
                codeDataList.DataList.Add(DefcodeData);

                foreach (var rec in workTypeList)
                {
                    CodeData codeData = new CodeData();

                    codeData.Code = rec.Code;
                    codeData.Name = rec.Name;
                    codeDataList.DataList.Add(codeData);
                }

                return Json(codeDataList);
            }
            return new EmptyResult();
        }

        //Add 2016/05/31 arc nakayama #3568_【サービス伝票】見積から受注にするとタイムアウトで落ちる
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>処理完了</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();

                ret.Add("ProcessedTime", "処理完了");

                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 主作業から請求先を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">主作業コード</param>
        /// <returns>請求先情報</returns>
        /// <history>
        /// 2019/02/06 yano #3959 サービス伝票入力　請求先誤り　新規作成
        /// </history>
        public ActionResult GetCustomerClaimByServiceWork(string code, string departmentCode)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();

                string makerCode = (new DepartmentDao(db).GetByKey(departmentCode) != null ? new DepartmentDao(db).GetByKey(departmentCode).MainMakerCode : "");

                if (!string.IsNullOrWhiteSpace(makerCode))
                {
                    ServiceWorkCustomerClaim rec = new ServiceWorkCustomerClaimDao(db).GetByKey(code, makerCode);

                    if (rec != null)
                    {
                        codeData.Code = rec.CustomerClaimCode;
                        codeData.Name = (rec.CustomerClaim != null ? rec.CustomerClaim.CustomerClaimName : "");
                    }
                }

                return Json(codeData);
            }
            return new EmptyResult();
        }

        #endregion
    }
}