using Crms.Models;                      //Add 2014/08/06 arc amii エラーログ対応 ログ出力の為に追加
using CrmsDao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;

namespace Crms.Controllers
{
  /// <summary>
  /// 車両伝票
  /// </summary>
  [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarSalesOrderController : InheritedController {
        
        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarSalesOrderController()
        {
            this.db = new CrmsLinqDataContext();
        }

        //Add 2014/08/06 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両伝票入力";     // 画面名
        private static readonly string PROC_NAME_REPORT = "販売報告データ登録"; // 処理名
        private static readonly string PROC_NAME_CONFIRM = "承認"; // 処理名
        private static readonly string PROC_NAME_ORDER = "受注"; // 処理名
        private static readonly string PROC_NAME_SAVE = "伝票保存（受注以外）"; // 処理名
        private static readonly string PROC_NAME_AKA = "赤伝"; // 処理名
        private static readonly string PROC_NAME_AKA_KURO = "赤黒"; // 処理名
        private static readonly int DEFAULT_GYO_COUNT = 5;  //オプションのデフォルト行数 

        //Add 2016/04/05 arc yano #3441 請求先タイプの定義を追加
        private static readonly List<string> excludeList = new List<string>() { "003" }; //請求先タイプ = クレジット
        private static readonly List<string> excluedAccountTypetList = new List<string>() { "012", "013" }; //口座タイプ = 残債, 下取
        private static readonly List<string> excluedAccountTypetList2 = new List<string>() { "004", "012", "013" }; //口座タイプ = 残債, 下取

        private static readonly string ACCOUNTTYPE_LOAN = "004";   //口座タイプ=ローン    //Add 2022/08/30 yano #4150

        //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
        private static readonly string LAST_EDIT_CARSALSEORDER = "001";           // 車両伝票入力画面で更新した時の値


        private static readonly string PTN_CANCEL_ALL= "1";                       //全て解除           //Add 2018/08/07 yano #3911
        private static readonly string PTN_CANCEL_REGISTRATION = "2";             //登録・引当解除     //Add 2018/08/07 yano #3911
        private static readonly string PTN_CANCEL_RESERVATION = "99";             //引当解除           //Add 2018/08/07 yano #3911

        private static readonly string CANCEL_FROM_CANCEL = "2";                  //受注後キャンセルによる引当解除   //Add 2018/08/07 yano #3911
        private static readonly string CANCEL_FROM_AKADEN = "3";                  //赤伝処理による引当解除           //Add 2018/08/07 yano #3911

        private static readonly string SALESTYPE_BUSINESSSALES = "003";           //業務販売        //Add 2020/11/17 yano #4059
        private static readonly string SALESTYPE_AUTOAUCTION = "004";             //AA              //Add 2020/11/17 yano #4059

        #endregion



        #region 検索画面
        /// <summary>
        /// 車両伝票検索画面表示
        /// </summary>
        /// <returns>車両伝票検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両伝票検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両伝票検索画面</returns>
        /// <history>
        /// 2020/01/14 yano #3982 【車両伝票一覧】使用者で伝票を検索できるようにして欲しい
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            //form["EmployeeCode"] = (form["EmployeeCode"] == null ? ((Employee)Session["Employee"]).EmployeeCode : form["EmployeeCode"]);
            form["SalesOrderStatus"] = Request["status"] == null ? form["SalesOrderStatus"] : Request["status"];
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;

            PaginatedList<CarSalesHeader>list = GetSearchResultList(form);
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
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["Vin"] = form["Vin"];

            //Add 2020/01/14 yano #3982
            ViewData["UserCode"] = form["UserCode"];
            ViewData["UserName"] = form["UserName"];

            //表示項目のセット

            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                ViewData["EmployeeName"] = (new EmployeeDao(db)).GetByKey(form["EmployeeCode"], true).EmployeeName;
            }
            //Mod 2015/07/06 arc nakayama DelFlag対応の漏れ（GetByKeyの第２引数をFalse⇒Trueに変更）
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                ViewData["DepartmentName"] = (new DepartmentDao(db)).GetByKey(form["DepartmentCode"], true).DepartmentName;
            }
            CodeDao dao = new CodeDao(db);
            ViewData["SalesOrderStatusList"] = CodeUtils.GetSelectListByModel(dao.GetSalesOrderStatusAll(false),form["SalesOrderStatus"], true);

            return View("CarSalesOrderCriteria",list);
        }

        /// <summary>
        /// 車両伝票検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両伝票検索結果リスト</returns>
        /// <history>
        /// 2022/08/30 yano #4079【車両伝票入力】ブランド名で検索を行えない
        /// 2020/01/14 yano #3982 【車両伝票一覧】使用者で伝票を検索できるようにして欲しい
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能 他のユーザが編集している場合は入力項目を非活性とする
        /// </history>
        private PaginatedList<CarSalesHeader> GetSearchResultList(FormCollection form) {
            CarSalesOrderDao carSalesOrderDao = new CarSalesOrderDao(db);
            CarSalesHeader salesHeaderCondition = new CarSalesHeader();
            salesHeaderCondition.Employee = new Employee();
            salesHeaderCondition.Employee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.Customer = new Customer();
            salesHeaderCondition.Customer.CustomerCode = form["CustomerCode"];
            salesHeaderCondition.Customer.CustomerName = form["CustomerName"];
            salesHeaderCondition.Customer.TelNumber = form["TelNumber"];
            salesHeaderCondition.SlipNumber = form["SlipNumber"];
            salesHeaderCondition.CampaignCode1 = form["CampaignCode1"];
            salesHeaderCondition.CampaignCode2 = form["CampaignCode2"];
            salesHeaderCondition.SalesOrderStatus = form["SalesOrderStatus"];
            salesHeaderCondition.QuoteDateFrom = CommonUtils.StrToDateTime(form["QuoteDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.QuoteDateTo = CommonUtils.StrToDateTime(form["QuoteDateTo"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.DepartmentCode = form["DepartmentCode"];
            salesHeaderCondition.Vin = form["Vin"];
            salesHeaderCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"], DaoConst.SQL_DATETIME_MIN);

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                salesHeaderCondition.DelFlag = form["DelFlag"];
            }
            salesHeaderCondition.SetAuthCondition((Employee)Session["Employee"]);
            salesHeaderCondition.IsAkaKuro = form["AkaKuro"] != null && form["AkaKuro"].Equals("1");

            salesHeaderCondition.User = new Customer();
            salesHeaderCondition.User.CustomerCode = form["UserCode"];                   //Add 2020/01/14 yano #3982
            salesHeaderCondition.User.CustomerName = form["UserName"];          //Add 2020/01/14 yano #3982

            salesHeaderCondition.CarBrandName = form["CarBrandName"];           //Mod 2022/08/30 yano #4079  

            return carSalesOrderDao.GetListByCondition(salesHeaderCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region 画面コントロールの有効・無効
        /// <summary>
        /// 伝票ステータスから表示するViewを返す
        /// </summary>
        /// <param name="header">CarSalesHeaderオブジェクト</param>
        /// <returns>表示するビュー</returns
        /// <history>
        /// 2021/08/06 #4088【車両伝票入力】ローン入金チェック漏れ対応
        /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// 2017/01/21 arc yano #3657  見積書の個人情報の表示／非表示のチェックボックスの表示・非表示を設定
        /// </history>
        private ActionResult GetViewResult(CarSalesHeader header) {
            
            //コントロールの初期化
            header.BasicEnabled = false;
            header.CarEnabled = false;
            header.OptionEnabled = false;
            header.CostEnabled = false;
            header.CustomerEnabled = false;
            header.TotalEnabled = false;
            header.OwnerEnabled = false;
            header.RequestEnabled = false;
            header.RegistEnabled = false;
            header.SalesDateEnabled = false;
            header.SalesPlanDateEnabled = false;　 //ADD 2014/02/20 ookubo
            header.RateEnabled = false;　          //ADD 2014/02/20 ookubo
            header.PaymentEnabled = false;
            header.UsedEnabled = false;
            header.InsuranceEnabled = false;
            header.LoanEnabled = false;
            header.CarRegEnabled = false;         //ADD 2016/07/08 nishimura
            header.PInfoChekEnabled = false;     //ADD 2017/01/21 arc yano #3657
            header.ReasonEnabled = false; //Add 2017/11/10 arc yano #3787
        
            header.CarVinEnabled = false;       //Add 2018/08/07 yano #3911
            header.RegistButtonVisible = false; //Add 2018/08/07 yano #3911

            header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029


            //Add 2018/08/07 yano #3911
            bool reservedFlag = false;          //引当済フラグ
            bool registeredFlag = false;        //登録済フラグ

          
            //車両が登録済の場合は車台番号を変更できないようにする
            CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //引当済の場合は引当済フラグをON
            if (rec != null && !string.IsNullOrWhiteSpace(rec.ReservationStatus) && rec.ReservationStatus.Equals("1"))
            {
                reservedFlag = true;
            }
            //登録済の場合は登録済フラグをON
            if (rec != null && !string.IsNullOrWhiteSpace(rec.RegistrationStatus) && rec.RegistrationStatus.Equals("1"))
            {
                registeredFlag = true;
            }

            
            if (!string.IsNullOrEmpty(header.DelFlag) && header.DelFlag.Equals("1")) {
                header.ShowMenuIndex = 2;
                header.PInfoChekEnabled = true;    //ADD 2017/01/21 arc yano #3657
            
            }
            else if (!string.IsNullOrEmpty(header.LockEmployeeName)) //Mod 2017/11/10 arc yano #3787
            {
                header.ShowMenuIndex = 11;
                ViewData["ProcessSessionError"] = "この伝票は現在" + header.LockEmployeeName + "さんが使用しているため読み取り専用で表示しています";
            }
            else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("1"))
            {
                // 赤伝
                header.ShowMenuIndex = 8;
                header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787
            }
            else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("2"))
            {
                // 赤黒
                header.ShowMenuIndex = 9;
                header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787
            }
            else
            {
                switch (header.SalesOrderStatus)
                {
                    case "001": //見積

                        //登録情報以外全てEnable
                        header.BasicEnabled = true;
                        header.CarEnabled = true;
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.CustomerEnabled = true;
                        header.TotalEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　         //ADD 2014/02/20 ookubo
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;

                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.RequestEnabled = true;
                        header.ShowMenuIndex = 1;
                        header.SalesOrderDateEnabled = true;
                        header.CarRegEnabled = true;        //ADD 2016/07/08 nishimura

                        header.PInfoChekEnabled = true;    //ADD 2017/01/21 arc yano #3657

                        //Add 2020/01/06 yano #4029
                        //販売区分が「業販」「AA」「依廃」以外
                        if (string.IsNullOrWhiteSpace(header.SalesType) ||
                           (
                            !header.SalesType.Equals("003") &&
                            !header.SalesType.Equals("004") &&
                            !header.SalesType.Equals("008")
                            )
                        )
                        {
                            header.CostAreaEnabled = true;
                        }
                        

                        //Add 2018/08/07 yano #3911
                        if (!reservedFlag && !registeredFlag)
                        {
                            header.CarVinEnabled = true;
                        }


                        break;
                    case "002": //受注
                        //基本、車両、顧客Disable
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.TotalEnabled = true;
                        header.OwnerEnabled = true;
                        header.RegistEnabled = true;
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;


                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　         //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 4;
                        header.SalesOrderDateEnabled = true;
                        header.CarRegEnabled = true;         //ADD 2016/07/08 nishimura

                        //Add 2020/01/06 yano #4029
                        //販売区分が「業販」「AA」「依廃」以外
                        //販売区分が「業販」「AA」「依廃」以外
                        if (string.IsNullOrWhiteSpace(header.SalesType) ||
                           (
                            !header.SalesType.Equals("003") &&
                            !header.SalesType.Equals("004") &&
                            !header.SalesType.Equals("008")
                            )
                        )
                        {
                            header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                        }
                        //Add 2018/08/07 yano #3911
                        if (registeredFlag)
                        {
                            header.RegistButtonVisible = true;

                            //車両登録済でvalidationエラーメッセージを表示しない場合は以下のメッセージを表示
                            if (ModelState.IsValid)
                            {
                                ViewData["MessageCarRegisted"] = "既に車両登録済のため「登録済へ進める」ボタンをクリックして登録済にして下さい";
                            }
                        }

                        break;
                    case "003": //登録済み
                        //基本、車両、顧客、登録情報、所有者情報はDisable
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.TotalEnabled = true;
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;


                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　         //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 5;
                        header.SalesOrderDateEnabled = true;

                        header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029
                        break;
                    case "004": //納車確認書印刷済み
                        //納車日以外Disable
                        header.SalesDateEnabled = true;
                        header.ShowMenuIndex = 6;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;　         //ADD 2014/02/20 ookubo
                        header.SalesOrderDateEnabled = true;

                        header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029

                        // Add 2015/03/18 arc nakayama 伝票修正対応　過去に修正を行った事のある伝票だった場合、理由欄を表示する
                        if (CheckModifiedReason(header.SlipNumber))
                        {
                            header.ModifiedReasonEnabled = true;
                            GetModifiedHistory(header); //修正履歴取得
                        }
                        else
                        {
                            header.ModifiedReasonEnabled = false;
                        }

                        //修正中かどうかのチェック
                        if (CheckModification(header.SlipNumber, header.RevisionNumber))
                        {
                            header.ModificationControl = true; //修正中

                            header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787

                            //伝票修正許可権限があれば入力エリアを開放する
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode))
                            {
                                header.ModificationControlCancel = true; //[修正キャンセル]ボタン 表示
                                header.ModificationControlCommit = true; // [修正完了]ボタン 表示
                                header.BasicEnabled = true;
                                header.CarEnabled = true;
                                header.OptionEnabled = true;
                                header.CostEnabled = true;
                                header.CustomerEnabled = true;
                                header.TotalEnabled = true;
                                header.OwnerEnabled = true;
                                header.RequestEnabled = true;
                                header.RegistEnabled = true;
                                header.SalesDateEnabled = true;
                                header.SalesPlanDateEnabled = true;
                                header.RateEnabled = true;
                                header.PaymentEnabled = true;
                                header.UsedEnabled = true;
                                header.InsuranceEnabled = true;
                                header.LoanEnabled = true;
                                header.CarRegEnabled = true;
                                header.PInfoChekEnabled = true;

                                //Add 2020/01/06 yano #4029
                                //販売区分が「業販」「AA」「依廃」以外
                                //販売区分が「業販」「AA」「依廃」以外
                                if (string.IsNullOrWhiteSpace(header.SalesType) ||
                                   (
                                    !header.SalesType.Equals("003") &&
                                    !header.SalesType.Equals("004") &&
                                    !header.SalesType.Equals("008")
                                    )
                                )
                                {
                                    header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                                }
                            }
                        }
                        else
                        {
                            header.ModificationControl = false; //修正中でない

                            // ログインユーザーが伝票修正許可権限をもっている　かつ　赤伝または過去に赤黒処理を行った元伝票でなかった場合は伝票修正ボタン表示
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode) && !header.SlipNumber.Contains("-1") && AkakuroCheck(header.SlipNumber))
                            {
                                header.ModificationEnabled = true;  //[伝票修正]ボタン表示
                            }
                            else
                            {
                                header.ModificationEnabled = false; //[伝票修正]ボタン非表示
                            }

                        }

                        break;
                    case "005": //納車済み
                        //全てDisable
                        header.ShowMenuIndex = 10;

                        bool IsClose = new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, header.SalesDate, "001");

                        // Add 2015/03/18 arc nakayama 伝票修正対応　過去に修正を行った事のある伝票だった場合、理由欄を表示する
                        if (CheckModifiedReason(header.SlipNumber))
                        {
                            header.ModifiedReasonEnabled = true;
                            GetModifiedHistory(header); //修正履歴取得
                        }
                        else
                        {
                            header.ModifiedReasonEnabled = false;
                        }

                        //修正中かどうかのチェック
                        if (CheckModification(header.SlipNumber, header.RevisionNumber))
                        {
                            header.ModificationControl = true; //修正中 

                            header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787

                            //修正中でも経理の締め処理状況が仮締めか本締めなら変更は不可にして[修正キャンセル]ボタンと[閉じる]ボタンのみの表示になる。理由欄も表示されない
                            if (IsClose)
                            {
                                //伝票修正許可権限があれば入力エリアを開放する
                                if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode))
                                {
                                    header.ModificationControlCommit = true; // [修正完了]ボタン 表示
                                    header.ModificationControlCancel = true; //[修正キャンセル]ボタン 表示
                                    header.BasicEnabled = true;
                                    header.CarEnabled = true;
                                    header.OptionEnabled = true;
                                    header.CostEnabled = true;
                                    header.CustomerEnabled = true;
                                    header.TotalEnabled = true;
                                    header.OwnerEnabled = true;
                                    header.RequestEnabled = true;
                                    header.RegistEnabled = true;
                                    header.SalesDateEnabled = true;
                                    header.SalesPlanDateEnabled = true;
                                    header.RateEnabled = true;
                                    header.PaymentEnabled = true;
                                    header.UsedEnabled = true;
                                    header.InsuranceEnabled = true;
                                    header.LoanEnabled = true;
                                    header.CarRegEnabled = true;
                                    header.PInfoChekEnabled = true;
                                    
                                    //Add 2020/01/06 yano #4029
                                    //販売区分が「業販」「AA」「依廃」以外
                                    //販売区分が「業販」「AA」「依廃」以外
                                    if (string.IsNullOrWhiteSpace(header.SalesType) ||
                                       (
                                        !header.SalesType.Equals("003") &&
                                        !header.SalesType.Equals("004") &&
                                        !header.SalesType.Equals("008")
                                        )
                                    )
                                    {
                                        header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                                    }
                                }
                            }
                            else
                            {
                                header.ModificationControlCommit = false;        // [修正完了]ボタン 非表示
                            }
                        }
                        else
                        {
                            header.ModificationControl = false; //修正中でない

                            // ログインユーザーが伝票修正許可権限をもっている　かつ　赤伝または過去に赤黒処理を行った元伝票でなかった場合は伝票修正ボタン表示
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode) && !header.SlipNumber.Contains("-1") && AkakuroCheck(header.SlipNumber))
                            {
                                header.ModificationEnabled = true;  //[伝票修正]ボタン表示
                            }
                            else
                            {
                                header.ModificationEnabled = false; //[伝票修正]ボタン非表示
                            }
                        }

                        //販売報告入力画面を表示
                        //return View("CarSalesReportEntry", header);
                        break;
                    case "006": //ｷｬﾝｾﾙ
                        //全てDisable 
                        header.ShowMenuIndex = 8;
                        break;
                }

                //管理者権限のみ消費税率使用可
                Employee emp = HttpContext.Session["Employee"] as Employee;
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
                {
                    header.RateEnabled = false;
                }

                //Add 2021/08/06 #4088
                //ローン入金済かどうかをチェックする。
                if (!string.IsNullOrWhiteSpace(header.SlipNumber))
                {
                    List<Journal> jlist = new JournalDao(db).GetListBySlipNumber(header.SlipNumber).Where(x => x.AccountType.Equals("004")).ToList();
                    //ローン入金済の場合は変更させない。
                    if (jlist.Count > 0)
                    {
                        header.LoanEnabled = false;

                        header.LoanCompleted = true;
                    }
                }
            }
            return View("CarSalesOrderEntry", header);
        }

        #endregion

        #region 入力画面 
        /// <summary>
        /// 車両伝票入力画面表示
        /// </summary>
        /// <param name="SlipNo">伝票番号</param>
        /// <param name="RevNo">改訂番号</param>
        /// <returns>車両伝票入力画面</returns>
        /// <history>
        /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// 2017/01/21 arc yano #3657 請求先の個人情報の表示／非表示チェックボックスのデフォルトを設定
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string SlipNo, int? RevNo, string customerCode, string employeeCode, string Mode)
        {
            Employee employee;
            
            if (string.IsNullOrEmpty(employeeCode)) {
                //ログイン担当者
                employee = ((Employee)Session["Employee"]);
            } else {
                //担当者コード付きで呼び出されたとき
                employee = new EmployeeDao(db).GetByKey(employeeCode);
            }

            //顧客コード付きで呼び出されたとき
            Customer customer = new Customer();
            if (!string.IsNullOrEmpty(customerCode)) {
                customer = new CustomerDao(db).GetByKey(customerCode);
            }

            CarSalesHeader header;

            if (string.IsNullOrEmpty(SlipNo))
            {
                //新規作成
                header = new CarSalesHeader();

                if (!string.IsNullOrEmpty(Request["salesCarNumber"])) {
                    header.SalesCarNumber = Request["salesCarNumber"];

                    // Mod 2015/05/18 arc nakayama ルックアップ見直し対応　無効データも表示はさせる(salesCarNumber)
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(Request["salesCarNumber"], true);
                    if (salesCar != null) {
                        header.NewUsedType = salesCar.NewUsedType; 
                        header.MakerName = salesCar.MakerName;
                        header.CarGradeCode = salesCar.CarGradeCode;
                        try { header.CarBrandName = salesCar.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                        try { header.CarName = salesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
                        try { header.CarGradeName = salesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
                        header.ExteriorColorCode = salesCar.ExteriorColorCode;
                        header.ExteriorColorName = salesCar.ExteriorColorName;
                        header.InteriorColorCode = salesCar.InteriorColorCode;
                        header.InteriorColorName = salesCar.InteriorColorName;
                        header.ModelName = salesCar.ModelName;
                        header.Mileage = salesCar.Mileage;
                        header.MileageUnit = salesCar.MileageUnit;
                        header.Vin = salesCar.Vin;
                        try { header.SalesPrice = salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice; } catch (NullReferenceException) { }

                        //MOD 2014/02/20 ookubo
                        header.SalesTax = 0;   //Decimal.Floor((salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m) * ((Decimal)header.Rate / 100)); } catch (NullReferenceException) { }
                        //try { header.SalesTax = Decimal.Floor((salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m) * 0.05m); } catch (NullReferenceException) { }
                        try { header.InspectionRegistCost = salesCar.CarGrade.InspectionRegistCost; } catch (NullReferenceException) { }
                        try { header.RecycleDeposit = salesCar.RecycleDeposit ?? salesCar.CarGrade.RecycleDeposit; } catch (NullReferenceException) { }

                        //Mod 2019/09/04 yano #4011
                        Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                        header.EPDiscountTaxId = retValue.Item1;
                        header.AcquisitionTax = retValue.Item2;
                        //header.AcquisitionTax = CommonUtils.GetAcquisitionTax(salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                       
                        if (salesCar.NewUsedType != null && salesCar.NewUsedType.Equals("U")) {
                            try { header.CarLiabilityInsurance = salesCar.CarLiabilityInsurance ?? new CarLiabilityInsuranceDao(db).GetByUsedDefault().Amount; } catch (NullReferenceException) { }
                            header.CarWeightTax = salesCar.CarWeightTax;
                        } else if (salesCar.NewUsedType != null && salesCar.NewUsedType.Equals("N")) {
                            try { header.CarLiabilityInsurance = salesCar.CarLiabilityInsurance ?? new CarLiabilityInsuranceDao(db).GetByNewDefault().Amount; } catch (NullReferenceException) { }
                            CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarWeight ?? (salesCar.CarGrade != null ? salesCar.CarGrade.CarWeight : 0) ?? 0);
                            header.CarWeightTax = salesCar.CarWeightTax ?? weightTax.Amount;
                        }        
                    }
                }

                //初期値設定
                header.RevisionNumber = 0; //改訂番号のリセット
                header.Employee = employee; //ログイン担当者
                header.Department = employee.Department1;
                header.DepartmentCode = employee.Department1.DepartmentCode;
                header.Customer = customer;
                header.CustomerCode = customer.CustomerCode;
                header.SalesOrderStatus = "001";
                header.HotStatus = "A";
                header.QuoteDate = DateTime.Today;
                header.QuoteExpireDate = DateTime.Today.AddDays(6);
                header.SalesOrderDate = DateTime.Today;
                //ookubo
                //消費税IDが未設定であれば、当日日付で消費税ID取得
                if (header.ConsumptionTaxId == null) {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
                }

                // オプションを5段作成
                header.CarSalesLine = new EntitySet<CarSalesLine>();
                for (int i = 1; i <= DEFAULT_GYO_COUNT; i++)
                {
                    header.CarSalesLine.Add(new CarSalesLine());
                }

                //2020/01/06 yano #4029 Mod
                //諸費用固定値セット
                //ConfigurationSetting numberPlateCost = new ConfigurationSettingDao(db).GetByKey("NumberPlateCost");
                //if (numberPlateCost != null) {
                //    header.NumberPlateCost = decimal.Parse(numberPlateCost.Value);
                //}

                ConfigurationSetting stampCost = new ConfigurationSettingDao(db).GetByKey("TradeInFiscalStampCost");
                if(stampCost!=null){
                    header.TradeInFiscalStampCost = decimal.Parse(stampCost.Value);
                }

                // Add 2014/07/29 arc amii 課題対応 収入印紙代に初期値設定
                ConfigurationSetting revStampCost = new ConfigurationSettingDao(db).GetByKey("RevenueStampCost");
                if (revStampCost != null)
                {
                    header.RevenueStampCost = decimal.Parse(revStampCost.Value);
                }

                //Mod 2020/01/06 yano #4029
                //if (header.Department != null) {
                //    Office office = header.Department.Office;
                //    if (office.CostArea != null) {
                //        header.InspectionRegistFee = office.CostArea.InspectionRegistFee;
                //        //header.TradeInFee = office.CostArea.TradeInFee;
                //        header.PreparationFee = office.CostArea.PreparationFee;
                //        header.RecycleControlFee = office.CostArea.RecycleControlFee;
                //        header.RequestNumberFee = office.CostArea.RequestNumberFee;
                //        header.TradeInAppraisalFee = office.CostArea.AppraisalFee;
                //        header.ParkingSpaceFee = office.CostArea.ParkingSpaceCost;
                //        header.RequestNumberCost = office.CostArea.RequestNumberCost;
                        
                //    }
                //}

                //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
                header.LastEditScreen = "000";
            }
            else
            {
                //編集の場合は伝票を呼び出す
                if (RevNo == null) {
                    header = new CarSalesOrderDao(db).GetBySlipNumber(SlipNo);
                } else {
                    header = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo ?? 1);
                }
                header.CarPurchaseOrder = new CarPurchaseOrderDao(db).GetBySlipNumber(SlipNo);
                if (header.SalesCar != null) {
                    header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
                }
                ViewData["update"] = "1";
                ViewData["Mode"] = Mode;


                //Add 2017/11/10 arc yano #3787
                // 編集権限がある場合のみロック制御対象
                Employee loginUser = (Employee)Session["Employee"];
                string departmentCode = header.DepartmentCode;
                string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
                // 自部門・兼務部門１～３・セキュリティレベルALLのどれかに該当したらロックする
                if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004"))
                {
                    // 何もしない
                }
                else
                {
                    // 自分以外がロックしている場合はエラー表示する
                    string lockEmployeeName = GetProcessLockUser(header);
                    if (!string.IsNullOrEmpty(lockEmployeeName))
                    {
                        header.LockEmployeeName = lockEmployeeName;
                    }
                    else
                    {
                        // 伝票ロック
                        ProcessLock(header);
                    }
                }

            }
            
            //Add 2017/01/21 arc yano 
            if (header.SalesOrderStatus.Equals("001"))    //伝票ステータス=「見積」
            {
                header.DispPersonalInfo = false;
            }
            else ////伝票ステータス≠「見積」
            {
                header.DispPersonalInfo = true;
            }

            //表示項目のセット
            SetDataComponent(ref header);

            //ステータスによって適切なVIEWを表示
            return GetViewResult(header);
        }

        #endregion

        #region 販売報告書
        /// <summary>
        /// 販売報告書入力画面を表示
        /// </summary>
        /// <param name="SlipNo">伝票番号</param>
        /// <param name="RevNo">改訂番号</param>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Report(string SlipNo, int? RevNo) {
            CarSalesHeader header = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo);
            header.CarPurchaseOrder = new CarPurchaseOrderDao(db).GetBySlipNumber(SlipNo);
            if (header.SalesCar != null) {
                header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
            }
            if (header.CarSalesReport != null) {
                ViewData["update"] = "1";
            }
            SetDataComponent(ref header);
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }
        /// <summary>
        /// 販売報告明細行追加・削除
        /// </summary>
        /// <param name="header">伝票ヘッダ情報</param>
        /// <param name="report">販売報告明細</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddReportLine(CarSalesHeader header, EntitySet<CarSalesReport> report, FormCollection form) {
            header.CarSalesReport = report;
            if (!string.IsNullOrEmpty(header.SalesCarNumber)) {
                header.SalesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
            }

            string delLine = form["DelLine"];
            ModelState.Clear();

            //DelLineが0以上だったら指定行削除
            if (Int32.Parse(delLine) >= 0) {
                header.CarSalesReport.RemoveAt(Int32.Parse(delLine));
            } else {
                header.CarSalesReport.Add(new CarSalesReport());
            }
            ViewData["update"] = form["update"];
            //表示項目を再セット
            SetDataComponent(ref header);
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }

        /// <summary>
        /// 販売報告データの登録
        /// </summary>
        /// <param name="header">伝票ヘッダ情報</param>
        /// <param name="line">オプション明細情報</param>
        /// <param name="report">販売報告明細情報</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SalesReport(CarSalesHeader header, EntitySet<CarSalesReport> report, FormCollection form) {
            using (TransactionScope ts = new TransactionScope()) {

                // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                //更新時はDelete&Insert
                if (form["update"] != null && form["update"].Equals("1")) {
                    List<CarSalesReport> delList = new CarSalesReportDao(db).GetBySlipNumber(header.SlipNumber);
                    db.CarSalesReport.DeleteAllOnSubmit(delList);
                }
                try {
                    // Add 2014/07/23 arc amii 登録するデータが１件も無い時、システムエラーにならないよう修正
                    // 登録するデータがある場合のみ、INSERTを行う
                    if (report != null) {
                        db.CarSalesReport.InsertAllOnSubmit(report);
                    }

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/06 arc amii エラーログ対応『throw e』からエラーログを出力する処理に変更
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/06 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(se, PROC_NAME_REPORT, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                        //ステータスを戻す
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_REPORT, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }
                }
                catch (Exception e) {
                    //Add 2014/08/06 arc amii エラーログ対応『throw e』からエラーログを出力する処理に変更
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_REPORT, FORM_NAME, header.SlipNumber);
                    // エラーページに遷移
                    return View("Error");
                }
            }
            header.CarSalesReport = report;
            SetDataComponent(ref header);
            ViewData["close"] = "1";
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }
        #endregion

        #region 車両伝票コピー機能
        /// <summary>
        /// 伝票をコピーして入力画面表示
        /// 用途の違いからキャンセル・受注後キャンセルとそれ以外でコピーする項目を変える。
        /// キャンセル・受注後キャンセル伝票のコピー…ほぼ全ての項目をコピーする
        /// 上記以外…販売車両情報、オプション情報、税金等のみコピー
        /// </summary>
        /// <param name="SlipNo">伝票番号</param>
        /// <param name="RevNo">改訂番号</param>
        /// <returns></returns>
        /// <history>
        /// 2022/05/20 yano #4069【車両伝票入力】車台番号を入力した時の仕様変更
        /// 2017/11/08 arc yano #3553 車両伝票のコピー機能追加 新規作成
        /// </history>
        public ActionResult Copy(string SlipNo, int RevNo)
        {
            CarSalesHeader header = new CarSalesHeader();

            Employee employee = (Employee)Session["Employee"];

            CarSalesHeader original = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo);

            //(受注後)キャンセルの伝票をコピーする場合
            if (original.SalesOrderStatus.Equals("006") || original.SalesOrderStatus.Equals("007"))
            {
                header = MakeCopyDataFromCancel(header, original);
            }
            else //(受注後)キャンセル以外の伝票をコピーする場合
            {
                header = MakeCopyDataFromExceptCancel(header, original);
            }

            SetDataComponent(ref header);

            header.FromCopy = true;   //Add 2022/05/20 yano #4069

            return GetViewResult(header);
        }

        /// <summary>
        /// (受注後)キャンセル伝票からコピー処理
        /// ほぼ全ての項目をコピーする
        /// </summary>
        /// <param name="header">伝票</param>
        /// <param name="orgheader">コピー元伝票</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/08 arc yano #3553 車両伝票のコピー機能追加 新規作成
        /// </history>
        public CarSalesHeader MakeCopyDataFromCancel(CarSalesHeader header, CarSalesHeader original)
        {
            Employee employee = (Employee)Session["Employee"];

            //ヘッダに設定
            header = original;

            //明細情報の初期化
            foreach (var l in header.CarSalesLine)
            {
                //伝票番号、リビジョンを初期化する
                l.SlipNumber = "";                  //伝票番号
                l.RevisionNumber = 0;
            }

            //支払情報の初期化
            foreach (var p in header.CarSalesPayment)
            {
                //伝票番号、リビジョンを初期化する
                p.SlipNumber = "";                  //伝票番号
                p.RevisionNumber = 0;
            }

            //----------------------------------------------
            //情報の初期化(コピー値を使用しない項目の設定)
            //----------------------------------------------
            header = SetSlipData(header);

            //管理番号の設定
            SalesCar car = new SalesCarDao(db).GetDataByVin(original.Vin);

            //対象車両の在庫ステータス＝「在庫」以外の場合は管理番号は設定しない
            if (car != null && !string.IsNullOrWhiteSpace(car.CarStatus) && !car.CarStatus.Equals("001"))
            {
                //管理番号は空文字にする
                header.SalesCarNumber = "";
            }

            return header;
        }

        /// <summary>
        /// (受注後)キャンセル以外の伝票をコピーして入力画面表示
        /// 販売車両情報、オプション情報、税金等の項目のみコピー
        /// </summary>
        /// <param name="header">伝票</param>
        /// <param name="orgheader">コピー元伝票</param>
        /// <returns></returns>
        /// <history>
        /// 2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加
        /// 2021/06/09 yano #4091 【車両伝票】オプション行の区分追加(メンテナス・延長保証)
        /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業  外出ししたオプション合計を修正
        /// 2017/11/08 arc yano #3553 車両伝票のコピー機能追加 新規作成
        /// </history>
        public CarSalesHeader MakeCopyDataFromExceptCancel(CarSalesHeader header, CarSalesHeader original)
        {
            Employee employee = (Employee)Session["Employee"];

            CarGrade carGrade = new CarGradeDao(db).GetByKey(original.CarGradeCode);

            //----------------------------------------------
            //基本情報の設定
            //----------------------------------------------
            header = SetSlipData(header);
            
            header.Employee = employee;                                                 //担当者      
            header.Department = employee.Department1;                                   //部門
            header.DepartmentCode = employee.Department1.DepartmentCode;                //部門コード
            header.HotStatus = "A";                                                     //HOT管理

            //------------------------------
            //販売車両情報の設定
            //------------------------------
            header.NewUsedType = original.NewUsedType;                                  //新中区分

            header.SalesType = "001";                                                   //販売区分

            header.CarGradeCode = original.CarGradeCode;                                //グレードコード

            header.CarGrade = carGrade;

            try { header.MakerName = carGrade.Car.Brand.Maker.MakerName; }              //メーカー名
            catch (NullReferenceException) { }
            try { header.CarBrandName = carGrade.Car.Brand.CarBrandName; }              //ブランド名
            catch (NullReferenceException) { }
            try { header.CarName = carGrade.Car.CarName; }                              //車種名
            catch (NullReferenceException) { }
            try { header.CarGradeName = carGrade.CarGradeName; }                        //グレード名
            catch (NullReferenceException) { }
            try { header.ModelName = carGrade.ModelName; }                              //型式名
            catch (NullReferenceException) { }
            try { header.SalesPrice = (carGrade.SalesPrice ?? 0m); }                    //車両本体価格
            catch (NullReferenceException) { header.SalesPrice = 0m; }

            //車両本体価格(消費税)
            header.SalesTax = CommonUtils.CalculateConsumptionTax(header.SalesPrice, header.Rate);
                    
            //値引額
            header.DiscountAmount = 0;
            header.DiscountTax = 0;

            //------------------------------
            //オプションの設定
            //------------------------------
            //明細情報を取得する
            header.CarSalesLine = original.CarSalesLine;

            foreach (var l in header.CarSalesLine)
            {
                //伝票番号、リビジョンを初期化する
                l.SlipNumber = "";                          //伝票番号
                l.RevisionNumber = 0;                       //改訂番号

                //オプション価格(消費税)
                l.TaxAmount = CommonUtils.CalculateConsumptionTax(l.Amount, header.Rate);
            }

            //-----------------------------
            //税金等
            //-----------------------------
            //登録希望日
            header.RequestRegistDate = null;

            //自動車税環境性能割
            header.AcquisitionTax = original.AcquisitionTax;
            
            //環境性能割税率
            header.EPDiscountTaxId = original.EPDiscountTaxId;      //Add 2019/09/04 yano #4011

            /*
            if (carGrade != null)
            {
                header.AcquisitionTax = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, header.NewUsedType, "");
            }
            */

            //自賠責保険料 
            header.CarLiabilityInsurance = original.CarLiabilityInsurance;
            /*
            if (header.NewUsedType != null && header.NewUsedType.Equals("U"))
            {
                try { header.CarLiabilityInsurance =  new CarLiabilityInsuranceDao(db).GetByUsedDefault().Amount; }
                catch (NullReferenceException) { }
            }
            else if (header.NewUsedType != null && header.NewUsedType.Equals("N"))
            {
                try { header.CarLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByNewDefault().Amount; }
                catch (NullReferenceException) { }
                CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, (carGrade != null ? carGrade.CarWeight : 0) ?? 0);
                header.CarWeightTax = weightTax.Amount;
            }
            */

            //自動車税(種別割）
            header.CarTax = original.CarTax;

            //自動車重量税
            header.CarWeightTax = original.CarWeightTax;
            /*
            if (carGrade != null)
            {
                CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, (carGrade != null ? carGrade.CarWeight : 0) ?? 0);
                header.CarWeightTax = weightTax.Amount;
            }
            */

            //-----------------------------
            //その他非課税
            //-----------------------------
            //車庫証明証紙代
            //header.ParkingSpaceCost = original.ParkingSpaceCost;      //Mod 2020/01/06 yano #4029
                                                                        
            //検査登録印紙代 
            header.InspectionRegistCost = original.InspectionRegistCost;

            //header.InspectionRegistCost = carGrade != null && carGrade.InspectionRegistCost != null ? carGrade.InspectionRegistCost : original.InspectionRegistCost;      

            //ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(一般)
            //header.NumberPlateCost = original.NumberPlateCost;       //Mod 2020/01/06 yano #4029

            /*
            ConfigurationSetting numberPlateCost = new ConfigurationSettingDao(db).GetByKey("NumberPlateCost");
            if (numberPlateCost != null)
            {
                header.NumberPlateCost = decimal.Parse(numberPlateCost.Value);                                                                                          
            }
            */

            //下取車登録印紙代(下取車は０台なのでnull)
            header.TradeInCost = null;                                                                                                                                  

            //リサイクル預託金
            header.RecycleDeposit = original.RecycleDeposit;
            //header.RecycleDeposit = carGrade != null && carGrade.RecycleDeposit != null ? carGrade.RecycleDeposit : original.RecycleDeposit;

            //ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望) ※販売諸費用欄で設定 
            //収入印紙代
            header.RevenueStampCost = original.RevenueStampCost;

            /*
            ConfigurationSetting revStampCost = new ConfigurationSettingDao(db).GetByKey("RevenueStampCost");
            if (revStampCost != null)
            {
                header.RevenueStampCost = decimal.Parse(revStampCost.Value);
            }
            */

            //下取自動車税(種別割)預り金(下取車は０台なのでnull)
            header.TradeInCarTaxDeposit = null;

            //その他(項目名)
            header.TaxFreeFieldName = "";

            //その他(金額)
            header.TaxFreeFieldValue = null;
            
            //任意保険
            header.VoluntaryInsuranceAmount = null;   //Add 2023/01/11 yano #4158 

            //-----------------------------
            //販売諸費用
            //-----------------------------
            header.InspectionRegistFee = original.InspectionRegistFee;                              //検査登録手続代行費用(税抜)
            header.RequestNumberFee = original.RequestNumberFee;                                    //希望ナンバー申請手数料（税抜）
            header.PreparationFee = original.PreparationFee;                                        //納車準備費用

            header.TradeInMaintenanceFee = original.TradeInMaintenanceFee;                         //中古車点検・整備費用（税抜）
            header.FarRegistFee = original.FarRegistFee;                                            //遠方登録代行費用（税抜）
            header.TradeInFee = null;                                                               //下取車諸手続費用（税抜）

            header.RecycleControlFee = original.RecycleControlFee;                                  //リサイクル資金管理料（税抜）
            header.TradeInAppraisalFee = null;                                                      //下取車査定費用（税抜）

            header.ParkingSpaceFee = original.ParkingSpaceFee;                                      //車庫証明手続代行費用（税抜）

            header.InheritedInsuranceFee = original.InheritedInsuranceFee;                          //中古車継承整備費用（税抜）
            
            header.TaxationFieldName = "";                                                          //その他(項目名)

            header.TaxationFieldValue = null;                                                       //その他(税抜金額)

            //header.RequestNumberCost = original.RequestNumberCost;                                  //ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望) //Mod 2020/01/06 yano #4029

            /*
            if (header.Department != null)
            {
                Office office = header.Department.Office;
                if (office.CostArea != null)
                {
                    header.InspectionRegistFee = office.CostArea.InspectionRegistFee;               //検査登録手続代行費用(税抜)
                    header.RequestNumberFee = office.CostArea.RequestNumberFee;                     //希望ナンバー申請手数料（税抜）
                    header.PreparationFee = office.CostArea.PreparationFee;                         //納車準備費用

                    header.TradeInMaintenanceFee = null;                                            //中古車点検・整備費用（税抜）
                    header.FarRegistFee = null;                                                     //遠方登録代行費用（税抜）
                    header.TradeInFee = null;                                                       //下取車諸手続費用（税抜）

                    header.RecycleControlFee = office.CostArea.RecycleControlFee;                   //リサイクル資金管理料（税抜）
                    header.TradeInAppraisalFee = office.CostArea.AppraisalFee;                      //下取車査定費用（税抜）

                    header.ParkingSpaceFee = office.CostArea.ParkingSpaceCost;                      //車庫証明手続代行費用（税抜）

                    try { header.InheritedInsuranceFee = carGrade.Under24; }                        //中古車継承整備費用（税抜）
                    catch (NullReferenceException) { }

                    header.TaxationFieldName = original.TaxationFieldName;                          //その他(項目名)

                    header.TaxationFieldValue = original.TaxationFieldValue;                        //その他(税抜金額)

                    header.RequestNumberCost = office.CostArea.RequestNumberCost;                   //ﾅﾝﾊﾞｰﾌﾟﾚｰﾄ代(希望)
                }
            }
            */

            return header;
        }
                    
        /// <summary>
        /// 共通項目の設定
        /// </summary>
        /// <param name="header">伝票</param>
        /// <param name="orgheader">コピー元伝票</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/08 arc yano #3553 車両伝票のコピー機能追加 新規作成
        /// </history>
        public CarSalesHeader SetSlipData(CarSalesHeader header)
        {
            //----------------------------------------------
            //情報の初期化(コピー値を使用しない項目の設定)
            //----------------------------------------------
            header.SlipNumber = "";                                         //伝票番号
            header.RevisionNumber = 0;                                      //改訂番号

            header.QuoteDate = DateTime.Today;                              //見積日
            header.QuoteExpireDate = DateTime.Today.AddDays(6);             //見積有効期限
            header.SalesOrderDate = DateTime.Today;                         //受注日

            header.SalesPlanDate = null;                                    //納車予定日
            header.SalesDate = null;                                        //納車日

            header.SalesOrderStatus = "001";                                //伝票ステータス=見積

            //消費税率
            //消費税IDが未設定であれば、当日日付で消費税ID取得
            if (header.ConsumptionTaxId == null)
            {
                header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }

            return header;
        }

        #endregion

        #region 承認画面
        /// <summary>
        /// 車両伝票の承認画面を表示
        /// </summary>
        /// <param name="SlipNo">伝票番号</param>
        /// <returns>車両伝票承認画面</returns>
        [AuthFilter]
        public ActionResult Confirm(string SlipNo) {
            CarSalesHeader header = (new CarSalesOrderDao(db).GetBySlipNumber(SlipNo));

            //入力項目は全てDisable
            header.BasicEnabled = false;
            header.CarEnabled = false;
            header.OptionEnabled = false;
            header.CostEnabled = false;
            header.CustomerEnabled = false;
            header.TotalEnabled = false;
            header.OwnerEnabled = false;
            header.RegistEnabled = false;
            header.SalesDateEnabled = false;
            header.RateEnabled = false;         //ADD 2014/02/20 ookubo
            header.PaymentEnabled = false;
            header.UsedEnabled = false;
            header.InsuranceEnabled = false;
            header.LoanEnabled = false;
            header.ShowMenuIndex = 3;
            SetDataComponent(ref header);
            return View("CarSalesOrderEntry",header);
        }

        /// <summary>
        /// 承認ボタンを押した時の処理
        /// </summary>
        /// <param name="SlipNo">伝票番号</param>
        /// <returns>車両伝票承認画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(form["SlipNumber"]);
            header.ApprovalFlag = "1";


            using (TransactionScope ts = new TransactionScope()) {
                List<Task> list = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_PURCHASE_APPROVAL, form["SlipNumber"]);
                foreach (var a in list) {
                    a.TaskCompleteDate = DateTime.Now;
                    a.LastUpdateDate = DateTime.Now;
                    a.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                // Add 2014/08/06 arc amii エラーログ対応 catch句にChangeConflictExceptionを追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(cfe, PROC_NAME_CONFIRM, FORM_NAME, header.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_CONFIRM, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                
                
            }
            ViewData["close"] = "1";
            SetDataComponent(ref header);
            return View("CarSalesOrderEntry", header);
        }
        #endregion

        #region オプション追加削除
        /// <summary>
        /// オプション行追加・行削除ボタンを押下したときの処理
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="line">オプション明細</param>
        /// <param name="pay">支払明細</param>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2020/11/19 yano #4060 【車両伝票入力】オプション行追加・削除時の環境性能割計算の不具合
        /// 2019/10/22 yano #4024 【車両伝票入力】オプション行追加・削除時にエラー発生した時の不具合対応
        /// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算
        /// 2016/01/14 arc yano #3354 車両伝票のオプション行が25行有る時の削除の不具合
        ///                     行数チェックは行追加の場合のみ行うように修正
        /// </history>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Option(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {
            
            //Add 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する。
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }
            
            //Add 2014/05/16 yano vs2012移行化対応 支払行のバインドに失敗時は、POSTデータから値を取得する。
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }            
            
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;

            string delLine = form["DelLine"];
            ModelState.Clear();
            
            //2016/01/14 arc yano #3354

            //DelLineが0以上だったら指定行削除
            if (Int32.Parse(delLine) >= 0) {
                header.CarSalesLine.RemoveAt(Int32.Parse(delLine));
                header = CalcAmount(header);
            } else {
                
                if (line != null && line.Count > 24)
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0014", new string[] { "オプション", "25" }));
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }

                header.CarSalesLine.Add(new CarSalesLine());
            }
            
			//行追加/削除時に5行未満になる場合、５行になるように行を追加する  
            if (header.CarSalesLine.Count() < DEFAULT_GYO_COUNT)
            {
                int AddCount = DEFAULT_GYO_COUNT - header.CarSalesLine.Count();
                for (int i = 0; i < AddCount; i++)
                {
                    header.CarSalesLine.Add(new CarSalesLine());
                }
            }

            //表示項目を再セット
            SetDataComponent(ref header);

            //Add 2019/10/17 yano #4022
            string firstregist = "";

            SalesCar sc = header.SalesCar;
            CarGrade cg = header.CarGrade;

            if (sc == null)
            {
                //sc = new SalesCarDao(db).GetByVin(header.Vin).Where(x => !x.CarStatus.Equals("006")).FirstOrDefault();
                sc = new SalesCarDao(db).GetByVin(header.Vin).Where(x => x.CarStatus == null || !x.CarStatus.Equals("006")).FirstOrDefault();   //Mod 2019/10/22 yano #4024
            }

            if (cg == null)
            {
                cg = new CarGradeDao(db).GetByKey(header.CarGradeCode);
            }

            if (sc != null)
            {
                firstregist = string.Format("{0:yyyy/MM}", sc.FirstRegistrationDate);
            }

            //Mod 2020/11/19 yano #4060
            //環境性能割税率が空欄でない場合のみ計算を行う
            if (string.IsNullOrWhiteSpace(header.EPDiscountTaxId) || !header.EPDiscountTaxId.Equals("999"))
            {
                header.AcquisitionTax = CommonUtils.GetAcquisitionTax((cg != null ? (cg.SalesPrice ?? 0m) : 0m), header.MakerOptionAmount ?? 0m, (cg != null ? cg.VehicleType : ""), header.NewUsedType, firstregist, header.EPDiscountTaxId, header.RequestRegistDate).Item2;
            }
            
            //出口
            return GetViewResult(header);
        }
        #endregion

        #region 支払方法追加削除
        /// <summary>
        /// 支払方法の行追加・行削除ボタンが押下されたときの処理
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="line">オプション明細</param>
        /// <param name="pay">支払明細</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Payment(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する。
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }


            //Add 2014/05/16 yano vs2012移行化対応 支払行のバインドに失敗時は、POSTデータから値を取得する。
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;


            string delPayLine = form["DelPayLine"];
            ModelState.Clear();

            //DelPayLineが0以上だったら指定行削除
            if (Int32.Parse(delPayLine) >= 0) {
                header.CarSalesPayment.RemoveAt(Int32.Parse(delPayLine));
                header = CalcAmount(header);
            } else {
                CarSalesPayment addLine = new CarSalesPayment();
                if (!string.IsNullOrEmpty(header.CustomerCode)) {
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    if (customer != null && customer.CustomerClaim!=null) {
                        addLine.CustomerClaimCode = customer.CustomerClaim.CustomerClaimCode;
                    }
                }
                header.CarSalesPayment.Add(addLine);
            }

            //表示項目を再セット
            SetDataComponent(ref header);

            //支払方法入力画面を初期表示にする
            ViewData["displayContents"] = "invoice";

            //出口
            return GetViewResult(header);

        }
        #endregion

        #region 受注
        /// <summary>
        /// 受注処理
        /// </summary>
        /// <param name="header">伝票ヘッダ</param>
        /// <param name="line">オプション明細</param>
        /// <param name="pay">支払明細</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <hisotry>
        /// 2023/09/05 #4162 インボイス対応 インボイス用の消費税計算処理を追加
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について　車両登録済伝票の場合は、受注処理のタイミングでステータスを登録済にする。
        /// 2017/04/24 arc yano #3755 金額欄の入力時のカーソル位置の不具合 ModelState.Clear()の追加
        /// 2016/04/08 arc yano #3482 ＜売掛金改善対応＞車両伝票入力 入金予定作成前に既存の入金予定が存在する場合は、削除する ※経理用レコードは除く
        /// </hisotry>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Order(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する。
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012移行化対応 支払行のバインドに失敗時は、POSTデータから値を取得する。
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }  

            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
            //画面の値をクリアする
            ModelState.Clear(); //2017/04/24 arc yano #3755

            //共通Validationチェック
            ////Add 2016/12/08 arc nakayama #3674_支払金額と現金販売合計の差分チェックを受注以降に変更する　引数追加
            ValidateAllStatus(header, form, true);
            if (!ModelState.IsValid) {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //受注処理専用入力チェック
            ValidateCarSalesOrder(header);
            if (!ModelState.IsValid) {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //ADD 2014/02/20 ookubo
            for (int i = 0; i < header.CarSalesLine.Count; i++)
            {
                header.CarSalesLine[i].ConsumptionTaxId = header.ConsumptionTaxId;
                header.CarSalesLine[i].Rate = header.Rate;
            }

            using (TransactionScope ts = new TransactionScope()) {
                
                //新しい伝票データを作成
                CreateCarSalesOrder(header);

                //下取車の査定データをINSERTする
                for (int i = 1; i <= 3; i++)
                {
                    CreateAppraisalData(header, i, true);
                }

                //Mod 2018/08/07 yano #3911
                //既存の車両発注依頼データを取得する
                CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

                //既存データが存在しない場合
                if (rec == null)
                {
                    //車両発注依頼データをINSERTする
                    CreatePurchaseOrderData(header);
                }

                //Mod 2016/04/12 arc yano #3482
                //既存の入金予定を削除
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
                foreach (var d in delList)
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                //Add 2016/09/06 arc nakayama #3630_【製造】車両売掛金対応
                //下取車に関する入金予定を作成する
                CreateTradeReceiptPlan(header);

                //入金予定データをINSERTする
                CreatePaymentPlan(header);

                header.Department = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                header.Employee = new EmployeeDao(db).GetByKey(header.EmployeeCode);

                if (!ModelState.IsValid) {
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }
 
                //データインサート
                header.SalesOrderStatus = "002";
                
                //header.SalesOrderDate = DateTime.Today;
                db.CarSalesHeader.InsertOnSubmit(header);

                //Add 2023/09/05 #4162
                InsertInvoiceConsumptionTax(header);

                try {
                    db.SubmitChanges();
                    //コミット
                    ts.Complete();

                } catch (SqlException se) {
                    //Add 2014/08/06 arc amii エラーログ対応SQL文をセッションに登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        //Add 2014/08/06 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(se, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        //ステータスを戻す
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    } else {
                        //Add 2014/08/06 arc amii エラーログ対応SQL文をセッションに登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);
                    // エラーページに遷移
                    return View("Error");
                }
            }

            //全て成功したら受注速報を送信する
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.SendSalesOrderFlash(header);

            //おまじない
            ModelState.Clear();

            //表示項目の再セット
            SetDataComponent(ref header);
            //ViewData["close"] = "1";

            List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();


            //入金実績-請求先一覧作成用
            List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

            bool Mflag = false;
            foreach (string JClaim in JournalClaimList)
            {
                var Ret = from a in payList
                          where a.CustomerClaimCode.Equals(JClaim)
                          select a;

                if (Ret.Count() == 0)
                {
                    Mflag = true;
                    break;
                }
            }

            if (Mflag)
            {
                ModelState.AddModelError("", "処理は完了しましたが、支払情報欄に設定されていない請求先から入金実績があります。");
            }

            return GetViewResult(header);
        }
        #endregion

        #region 伝票保存（受注以外）
        /// <summary>
        /// 車両伝票の保存
        /// </summary>
        /// <param name="header">伝票ヘッダ情報</param>
        /// <param name="line">オプション情報</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2023/09/05 #4162 インボイス対応
        /// 2020/01/09 yano #4030【車両伝票入力】赤黒処理で納車処理を行うと車両マスタの納車日が更新されてしまう。
        /// 2018/12/21 yano #3965 WE版新システム構築 納車確認書は表示しない、ステータス遷移のみ
        /// 2018/11/07 yano #3939 車両伝票入力　納車前（納車確認書出力後）の注文書出力時に改訂番号を進めない
        /// 2018/08/14 yano #3910 車両伝票　デモカーの納車済伝票修正時に車両マスタのロケーションが消える
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について
        /// 2018/06/22 arc yano #3898 車両マスタ　AA販売で納車後キャンセルとなった場合の在庫ステータスについて
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない
        /// 2018/02/20 arc yano #3858 サービス伝票　納車後の保存処理で、納車日を空欄で保存できてしまう
        /// 2017/12/22 arc yano #3793 入金予定再作成時の不具合
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// 2017/10/10 arc yano #3802 入金実績表示　入金実績のある車両伝票を受注後キャンセルした場合の表示について
        /// 2016/09/27 arc nakayama #3630_【製造】車両売掛金対応
        /// 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行
        /// 2017/01/24 arc nakayama #3690_車輌購入申込金を入金出納帳へ登録した後、車輌伝票の支払方法に入金金額が未登録でも納車済にできてしまう。
        /// 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
        /// 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
        /// 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
        /// 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarSalesHeader header,EntitySet<CarSalesLine> line,EntitySet<CarSalesPayment> pay ,FormCollection form)
        {
            // Mod 2014/11/14 arc yano #3129
            // Add 2014/08/06 arc amii
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

   
            //Add 2014/05/16 yano vs2012移行化対応
      if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012移行化対応
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;

            //Add 2017/01/24 arc nakayama #3690
            //画面の値をクリアする
            ModelState.Clear();

            //Add 2017/05/24 arc nakayama #3761
            if (header.ActionType == "ModificationStart" || header.ActionType == "ModificationCancel")
            {
                //伝票を修正中・修正中を解除する
                if (header.ActionType == "ModificationStart")
                {
                    //納車済、または、納車前でも納車日が入力されていたら締めのチェックを行う
                    if (header.SalesOrderStatus.Equals("005") || (header.SalesOrderStatus.Equals("004") && header.SalesDate != null))
                    {
                        //本締めだった場合はエラー、修正中にさせない
                        if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, header.SalesDate, "001") && header.ActionType == "ModificationStart")
                        {
                            ModelState.AddModelError("SalesDate", "本締めが実行されたため、伝票修正は行えません。");

                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                    }

                    //Add 2017/11/10 arc yano #3787
                    //他のユーザに伝票がロックされているかをチェックする。
                    ValidateProcessLock(header, form);
                    if (!ModelState.IsValid)
                    {
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }

                    ModificationStart(header);
                }
                //修正キャンセルが押された場合は修正情報を削除する（修正をキャンセルする）
                if (header.ActionType == "ModificationCancel")
                {
                    //Add 2017/11/10 arc yano #3787
                    //他のユーザに伝票がロックされているかをチェックする。
                    ValidateProcessLock(header, form);
                    if (!ModelState.IsValid)
                    {
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }

                    ModificationCancel(header);

                    CarSalesHeader Prevheader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    if (Prevheader.CarSalesLine == null)
                    {
                        Prevheader.CarSalesLine = getCarSalesLinebyReq();
                    }

                    if (Prevheader.CarSalesPayment== null)
                    {
                        Prevheader.CarSalesPayment = getCarSalesPaymentbyReq();
                    }

                    SetDataComponent(ref Prevheader);
                    return GetViewResult(Prevheader);
                }

                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //Add 2017/11/10 arc yano #3787
            if (form["ForceUnLock"] != null && form["ForceUnLock"].Equals("1"))
            {
                //DBから最新の車両伝票を取得する
                header = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                ProcessLockUpdate(header);

                //赤伝・赤黒だったときの考慮
                if (!string.IsNullOrWhiteSpace(form["Mode"]))
                {
                    ViewData["Mode"] = form["Mode"];
                }

                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            //Add 2017/11/10 arc yano #3787
            //伝票ロックチェック
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                header.SalesOrderStatus = form["PreviousStatus"];   //Add 2018/05/30 arc yano #3889
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //共通Validationチェック
            ValidateAllStatus(header, form);

            //Mod 2014/08/14 arc amii エラーログ対応
            //受注以降は受注と同じValidationチェックを行う
            if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
            {
                ValidateCarSalesOrder(header);
            }

            for (int i = 0; i < header.CarSalesLine.Count; i++)
            {
                header.CarSalesLine[i].ConsumptionTaxId = header.ConsumptionTaxId;
                header.CarSalesLine[i].Rate = header.Rate;
            }

            //Validationエラーが存在する場合
            if (!ModelState.IsValid)
            {
                //Mod 2014/08/14 arc amii エラーログ対応
                //納車処理の時はデータを再取得する
                if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
                {
                    header = new CarSalesOrderDao(db).GetByKey(header.SlipNumber,header.RevisionNumber);
                    //納車日を再セット
                    header.SalesDate = CommonUtils.StrToDateTime(form["SalesDate"]);

                    //Add 2018/02/20 arc yano #3858 伝票修正の理由を設定
                    if (!string.IsNullOrWhiteSpace(form["Reason"]))
                    {
                        header.Reason = form["Reason"];
                    }
                }
                //ステータスを戻す
                header.SalesOrderStatus = form["PreviousStatus"];
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

          
            //トランザクション処理開始
            using (TransactionScope ts = new TransactionScope()) {

                //Mod 2018/11/07 yano #3939
                CarSalesHeader target = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
               
                //DB登録済の最新伝票のステータスが「納車前」「納車済」の場合でかつ伝票修正完了でない場合
                if ((target != null && (target.SalesOrderStatus.Equals("004") || target.SalesOrderStatus.Equals("005"))) && 
                    (string.IsNullOrWhiteSpace(header.ActionType) || !header.ActionType.Equals("ModificationEnd")) )
                {
                    ModelState.Clear();
                    
                    //UpdateModel(target.CarSalesLine); 
                    //UpdateModel(target.CarSalesPayment);

                    target.ProcessSessionControl = new ProcessSessionControlDao(db).GetByKey(header.ProcessSessionId);

                    //target = CopyCarSalesHeader(header, target);

                    UpdateModel(target);

                    target.LastUpdateEmployeeCode = header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    target.LastUpdateDate = header.LastUpdateDate = DateTime.Now;

                    foreach (var l in target.CarSalesLine)
                    {
                        l.LastUpdateDate = DateTime.Now;
                        l.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                    foreach (var p in target.CarSalesPayment)
                    {
                        p.LastUpdateDate = DateTime.Now;
                        p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                }
                else
                {
                    //新しい伝票データを作成
                    CreateCarSalesOrder(header);

                    //Mod 2018/11/07 yano #3939 部品移動
                    //データインサート
                    db.CarSalesHeader.InsertOnSubmit(header);
                }


                //キャンセル処理の時はキャンセル日付をセット
                if (!string.IsNullOrEmpty(form["Cancel"]) && form["Cancel"].Equals("1")) {

                    //Mod 2018/06/22 arc yano #3898 
                    //-----------------------------
                    //車両引当解除
                    //-----------------------------
                    //車両引当解除権限の有無を取得
                    bool cancelAuth = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "ReservationCancel").EnableFlag;    //Add 2018/08/07 yano #3911

                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);

                    string customerType = customer != null ? customer.CustomerType : "";

                    //Mod 2018/08/07 yano #3911  
                    //受注後キャンセルの場合販売区分がAA販売、業販、依廃の場合のみ引当を解除
                    if (
                          header.SalesType.Equals("003") ||     //販売区分=「業販」
                          header.SalesType.Equals("009") ||     //販売区分=「店間移動」
                         (header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) ||  //販売区分=「AA」かつ顧客区分=「AA」
                         (header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))     //販売区分=「依廃」かつ顧客区分=「廃棄」
                        )
                    {
                        //全て解除
                        ReleaseProvision(header, PTN_CANCEL_ALL, CANCEL_FROM_CANCEL);
                    }
                    else
                    {
                        //車両の引当状態をチェック
                        // 引当済みの車両は引当解除
                        CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
                        
                        //登録済の場合
                        if (order != null && !string.IsNullOrWhiteSpace(order.RegistrationStatus) && order.RegistrationStatus.Equals("1"))
                        {
                            //登録・引当解除
                            ReleaseProvision(header, PTN_CANCEL_REGISTRATION, CANCEL_FROM_CANCEL);
                        }
                        else if (order != null && !string.IsNullOrWhiteSpace(order.ReservationStatus) && order.ReservationStatus.Equals("1"))
                        {
                            //引当のみ解除
                            ReleaseProvision(header, PTN_CANCEL_RESERVATION, CANCEL_FROM_CANCEL);
                        }
                    }

                    //Add 2018/06/22 arc yano #3898
                    //-----------------------------
                    //実績削除
                    //-----------------------------
                    DeleteAAJournal(header);

                    // 引当済みの車両は引当解除
                    //CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
                    //if (order != null && order.ReservationStatus != null && order.ReservationStatus.Equals("1")) {
                    //    order.ReservationStatus = "0";
                    //    SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                       
                    //    if (salesCar != null && salesCar.CarStatus.Equals("003")) {
                    //        salesCar.CarStatus = "001";
                    //    }
                    //    order.SalesCarNumber = string.Empty;
                    //}
                    

                    //入金予定を削除
                    List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
                    foreach (var p in planList) {
                        p.DelFlag = "1";
                        p.LastUpdateDate = DateTime.Now;
                        p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                    
                    //Add 2016/09/27 arc nakayama #3630
                    //残債と下取の入金実績は削除
                    List<Journal> DelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("012") || x.AccountType.Equals("013"))).ToList();
                    foreach (var d in DelJournal)
                    {
                        d.DelFlag = "1";
                        d.LastUpdateDate = DateTime.Now;
                        d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }

                    db.SubmitChanges();

                    //入金済みがあればアラート
                    List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    foreach (var j in journalList) {

                        // 過入金のアラートを作成
                        task.CarOverReceive(header, j.CustomerClaimCode, j.Amount);

                        //Add 2017/10/10 arc yano #3802
                        // 実績分の入金予定を作成
                        ReceiptPlan plusPlan = new ReceiptPlan();
                        plusPlan.ReceiptPlanId = Guid.NewGuid();
                        plusPlan.DepartmentCode = j.DepartmentCode;
                        plusPlan.OccurredDepartmentCode = j.DepartmentCode;
                        plusPlan.CustomerClaimCode = j.CustomerClaimCode;
                        plusPlan.SlipNumber = j.SlipNumber;
                        plusPlan.ReceiptType = j.AccountType;
                        plusPlan.ReceiptPlanDate = DateTime.Today;
                        plusPlan.AccountCode = j.AccountCode;
                        plusPlan.Amount = j.Amount;
                        plusPlan.ReceivableBalance = 0;             //残高 = 0(固定)
                        plusPlan.CompleteFlag = "1";                //入金完了フラグ = 0(固定)
                        plusPlan.CreateDate = DateTime.Now;
                        plusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plusPlan.LastUpdateDate = DateTime.Now;
                        plusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plusPlan.DelFlag = "0";
                        plusPlan.JournalDate = DateTime.Today;
                        plusPlan.DepositFlag = "0";
                        db.ReceiptPlan.InsertOnSubmit(plusPlan);


                        // 過入金分のマイナス入金予定を作成
                        ReceiptPlan minusPlan = new ReceiptPlan();
                        minusPlan.ReceiptPlanId = Guid.NewGuid();
                        minusPlan.DepartmentCode = j.DepartmentCode;
                        minusPlan.OccurredDepartmentCode = j.DepartmentCode;
                        minusPlan.CustomerClaimCode = j.CustomerClaimCode;
                        minusPlan.SlipNumber = j.SlipNumber;
                        minusPlan.ReceiptType = j.AccountType;
                        minusPlan.ReceiptPlanDate = DateTime.Today;
                        minusPlan.AccountCode = j.AccountCode;
                        minusPlan.Amount = -1 * j.Amount;
                        minusPlan.ReceivableBalance = -1 * j.Amount;
                        minusPlan.CompleteFlag = "0";
                        minusPlan.CreateDate = DateTime.Now;
                        minusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        minusPlan.LastUpdateDate = DateTime.Now;
                        minusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        minusPlan.DelFlag = "0";
                        minusPlan.JournalDate = DateTime.Today;
                        minusPlan.DepositFlag = "0";
                        db.ReceiptPlan.InsertOnSubmit(minusPlan);
                    }

                    // キャンセル日を更新
                    header.CancelDate = DateTime.Today;
                    header.LastEditScreen = "000";

                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    // 受注以降は「007:受注後キャンセル」に更新
                    if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
                    {
                        header.SalesOrderStatus = "006";
                    } else {
                        header.SalesOrderStatus = "007";

                        // キャンセルメールを送信
                        ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("CancelAlertMailAddress");
                        if (config != null) {
                            SendMail mail = new SendMail();
                            Department department = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                            Employee employee = new EmployeeDao(db).GetByKey(header.EmployeeCode);
                            c_NewUsedType newUsedType = new CodeDao(db).GetNewUsedTypeByKey(header.NewUsedType);

                            string customerName = "";
                            // Add 2014/08/06 arc amii エラーログ対応 catch句にExceptionを追加
                            try
                            {
                                customerName = new CustomerDao(db).GetByKey(header.CustomerCode).CustomerName;
                            }
                            catch (NullReferenceException ne)
                            {
                                // エラーになっても処理続行させる。エラーのみ出力
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogError(ne, PROC_NAME_SAVE+"の顧客名取得", FORM_NAME, header.SlipNumber);
                            }
                            catch (Exception e)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogFatal(e, PROC_NAME_SAVE + "の顧客名取得", FORM_NAME, header.SlipNumber);
                                // エラーページに遷移
                                return View("Error");
                            }
                            string title = "【SYSTEM Information】" + department.DepartmentName + "受注速報";
                            string msg = "■キャンセル\r\n";
                            msg += "受注日 : " + string.Format("{0:yyyy/MM/dd}", header.SalesOrderDate) + "\r\n";
                            msg += "伝票番号 : " + header.SlipNumber + "\r\n";
                            msg += "顧客名 : " + customerName + "\r\n";
                            msg += "担当者 : " + department.DepartmentName + ":" + employee.EmployeeName + "\r\n";
                            msg += "車種　 : " + header.MakerName + header.CarName + header.CarGradeName + "\r\n";
                            msg += "色　　 : " + header.ExteriorColorName + "/" + header.InteriorColorName + "\r\n";
                            msg += "車台No : " + header.Vin + "\r\n";
                            msg += "新中区 : " + newUsedType.Name;
                            mail.Send(title, config.Value, msg);
                        }

                        //Add 2016/09/05 arc nakayama #3630_【製造】車両売掛金対応
                        //受注後キャンセルが行われたとき、下取車の査定データと仕入データが両方存在した場合、何もしない、査定データのみの場合、削除
                        for (int i = 1; i <= 3; i++)
                        {

                            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                            {
                                CarAppraisal CarAppraisalData = new CarAppraisalDao(db).GetBySlipNumberVin(header.SlipNumber, vin.ToString());
                                if (CarAppraisalData != null)
                                {
                                    CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(header.SlipNumber, vin.ToString());

                                    if (CarPurchaseData == null)
                                    {
                                        CarAppraisalData.DelFlag = "1";
                                    }
                                    else
                                    {
                                        //なにもしない。車両の査定と仕入データは店舗の方に任せる
                                    }
                                }
                            }
                        }
                        //下取車とその残債に関する入金予定データも削除する
                        List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, header.DepartmentCode).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
                        foreach (var d in delList)
                        {
                            //残債と下取の入金予定
                            d.DelFlag = "1";
                            d.LastUpdateDate = DateTime.Now;
                            d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        }

                    }

                    // キャンセルタスクを作成する
                    task.CarCancel(header);


                } else {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    //受注以降の場合は入金予定を再作成
                    if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001")
                        //&& !CommonUtils.DefaultString(header.SlipNumber).Contains("-2")       //Mod 2017/12/22 arc yano #3793
                        )
                    {
                        //Add 2016/09/06 arc nakayama #3630_【製造】車両売掛金対応
                        //下取車に関する入金予定を作成する
                        CreateTradeReceiptPlan(header);

                        CreatePaymentPlan(header);

                        // 査定も再作成(2011.02.18追加)
                        // 既存のものを削除してから
                        CarAppraisalDao appraisalDao = new CarAppraisalDao(db);
                        for (int i = 1; i <= 3; i++)
                        {
                            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                            //bool editFlag = false;
                            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                            {

                                // 仕入予定作成済みの査定がなければ作成し直し
                                List<CarAppraisal> appraisalList = appraisalDao.GetListBySlipNumberVin(header.SlipNumber, vin.ToString(), "1");
                                if (appraisalList == null || appraisalList.Count() == 0)
                                {

                                    // 削除してから
                                    DeleteAppraisalData(header, i);

                                    // 作り直す
                                    CreateAppraisalData(header, i, false);

                                    db.SubmitChanges();


                                }
                                //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
                                //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
                            }
                        }
                    }
                }
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                //納車時でかつ黒伝票出ない場合は在庫ステータス更新(納車済み)
                if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
                {
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                    salesCar.CarStatus = "006";    //納車済み

                    //Mod 2018/08/14 yano #3910 //利用用途が空欄(≠固定資産)の場合
                    if (string.IsNullOrWhiteSpace(salesCar.CarUsage))
                    {
                        salesCar.Location = null;  //ロケーション更新
                    }

                    //Mod 2019/01/09 yano #4030 赤黒伝票の納車時には車両マスタの納車日は更新しない
                    if (!CommonUtils.DefaultString(header.SlipNumber).Contains("-2"))
                    {
                        salesCar.SalesDate = header.SalesDate; // 納車日更新
                    }

                    salesCar.LastUpdateDate = DateTime.Now;
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                    // 顧客種別の更新
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    if (string.IsNullOrEmpty(customer.CustomerKind) || !customer.CustomerKind.Equals("002")) {
                        customer.CustomerKind = "002";
                    }
                    //Del 2017/12/22 arc yano #3793
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    //if (CommonUtils.DefaultString(header.SlipNumber).Contains("-2")) {
                    //    ResetReceiptPlan(header);
                    //}
                }

                if (header.ActionType.Equals("ModificationEnd"))
                {
                    //修正履歴追加
                    CreateModifiedHistory(header);
                }

                //Mod 2018/11/07 yano #3939
                //データインサート
                //db.CarSalesHeader.InsertOnSubmit(header);

                //Add 2023/09/05 #4162
                InsertInvoiceConsumptionTax(header);
              
                try {
                    db.SubmitChanges();
                    //コミット
                    ts.Complete();
                } catch (SqlException se) {
                    //Add 2014/08/06 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                       //Add 2014/08/06 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    } else {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                    // エラーページに遷移
                    return View("Error");
                }
            }

            //おまじない
            ModelState.Clear();

            //Add2018/12/21 yano #3965
            if (Session["ConnectDB"] != null && Session["ConnectDB"].Equals("WE_DB"))   //WE版の場合
            {
                if (header.SalesOrderStatus.Equals("004") && !string.IsNullOrWhiteSpace(header.ActionType) && header.ActionType.Equals("CarDeliveryReport"))
                {
                    ModelState.AddModelError("", "本システムでは納車確認書を出力しません。納車確認書は指定のものをご使用下さい。");
                    header.ActionType = "";
                }
            }

            
            //表示項目の再セット
            SetDataComponent(ref header);

            //帳票印刷
            if(!string.IsNullOrEmpty(form["PrintReport"])){
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            }else{
                if (form["requestType"].Equals("requestService")) {
                    ViewData["url"] = "/ServiceRequest/Entry?SlipNo=" + header.SlipNumber;
                }
                //ViewData["close"] = "1";
            }

            //Add 2016/12/08 arc nakayama #3674_支払金額と現金販売合計の差分チェックを受注以降に変更する
            //Mod 2016/12/13 arc nakayama #3678_車両伝票　支払金合計と現金販売合計のバリデーションで、支払金合計にローン手数料が含まれている

            if (string.IsNullOrEmpty(header.PaymentPlanType))
            {
                if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && header.SalesOrderStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "保存は完了しましたが、総支払金合計と現金販売合計が一致していません。");
                    ModelState.AddModelError("", "受注以降は総支払金合計と現金販売合計と一致するようにして下さい。");
                }
            }
            else
            {
                if (header.GrandTotalAmount != header.LoanPrincipalAmount + header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && header.SalesOrderStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "保存は完了しましたが、総支払金合計と現金販売合計が一致していません。");
                    ModelState.AddModelError("", "受注以降は総支払金合計と現金販売合計と一致するようにして下さい。");
                }
            }

            //受注の時にチェックする
            if (header.SalesOrderStatus.Equals("002"))
            {
                List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();

                //入金実績-請求先一覧作成用
                List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

                bool Mflag = false;
                foreach (string JClaim in JournalClaimList)
                {
                    var Ret = from a in payList
                              where a.CustomerClaimCode.Equals(JClaim)
                              select a;

                    if (Ret.Count() == 0)
                    {
                        Mflag = true;
                        break;
                    }
                }

                if (Mflag)
                {
                    ModelState.AddModelError("", "処理は完了しましたが、支払情報欄に設定されていない請求先から入金実績があります。");
                }
            }

            //出口
            return GetViewResult(header);
        }
        #endregion

        #region 赤黒対象検索
        /// <summary>
        /// 赤黒検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyCriteria() {
            criteriaInit = true;
            return ModifyCriteria(new FormCollection());
        }
        /// <summary>
        /// 赤黒検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ModifyCriteria(FormCollection form) {
            form["DelFlag"] = "0";
            form["SalesOrderStatus"] = "005";
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
            form["AkaKuro"] = "1";
            PaginatedList<CarSalesHeader> list;
            if (criteriaInit) {
                list = new PaginatedList<CarSalesHeader>();
            } else {
                list = GetSearchResultList(form);
            }
            foreach (var item in list) {
                // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(item.DepartmentCode, item.SalesDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    // 納車日が棚卸締め未処理だったら赤黒対象外
                    item.IsClosed = true;
                }
                if (new CarSalesOrderDao(db).GetBySlipNumber(item.SlipNumber + "-1") != null) {
                    // 赤伝処理、赤黒処理していなかったら対象
                    item.IsCreated = true;
                }
            }
            return View("CarSalesOrderModifyCriteria", list);
        }
        #endregion

        #region 赤伝

        /// <summary>
        /// 赤伝処理
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/22 arc yano  #3898 車両マスタ　AA販売で納車後キャンセルとなった場合の在庫ステータスについて
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akaden(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する。
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012移行化対応 支払行のバインドに失敗時は、POSTデータから値を取得する。
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
            

            // 赤伝処理・赤黒処理時は理由が必須
            ViewData["Mode"] = form["Mode"];


            //Add 2017/11/10 arc yano #3787
            //他のユーザに伝票がロックされているかをチェックする。
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            if (string.IsNullOrEmpty(header.Reason)) {
                if (form["Mode"].Equals("1")) {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "赤伝処理", "理由" }));
                } else {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "赤黒処理", "理由" }));
                }
            }
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "理由は1024文字以内で入力して下さい");
            }
            CarSalesHeader target = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            if (target != null) {
                ModelState.AddModelError("SlipNumber", "すでに処理されています");
            }

            
            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (ModelState.IsValid) {


                CarSalesHeader history2 = new CarSalesHeader();
                double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

                // 赤伝処理

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
                {

                    //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
                    List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
                    foreach (var d in delList)
                    {
                        //残債と下取の入金予定
                        d.DelFlag = "1";
                        d.LastUpdateDate = DateTime.Now;
                        d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }

                    //Add 2018/06/22 arc yano #3898
                    //------------------------------
                    //AA相殺の入金実績の削除
                    //------------------------------
                    DeleteAAJournal(header);


                    //Add 2018/06/22 arc yano  #3898
                    //----------------------------
                    //車両引当解除処理
                    //----------------------------
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);

                    string customerType = customer != null ? customer.CustomerType : "";

                    //Mod 2018/08/07 yano #3911
                    //受注後キャンセルの場合販売区分がAA販売、業販、依廃の場合のみ引当を解除
                    if (
                          header.SalesType.Equals("003") ||     //販売区分=「業販」
                          header.SalesType.Equals("009") ||     //販売区分=「店間移動」
                         (header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) ||  //販売区分=「AA」かつ顧客区分=「AA」
                         (header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))     //販売区分=「依廃」かつ顧客区分=「廃棄」
                        )
                    {
                        ReleaseProvision(header, PTN_CANCEL_ALL, CANCEL_FROM_AKADEN);  //全て解除
                    }
                    else
                    {
                        //引当解除処理
                        ReleaseProvision(header, PTN_CANCEL_REGISTRATION, CANCEL_FROM_AKADEN); //登録・引当のみ解除(発注は解除しない)

                    }

                    db.SubmitChanges();

                    CarSalesHeader history = CreateAkaden(header, false);

                    db.SubmitChanges();

                    //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
                    //赤伝と元伝票の差分をとって返金の入金予定を作成する
                    db.CreateBackAmountAkaden(header.SlipNumber, ((Employee)Session["Employee"]).EmployeeCode);
                    CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "車両", header.Reason);

                    // Add 2014/08/06 arc amii エラーログ対応 try catch文とエラーログ処理を追加
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        history2 = history;
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログ出力
                            OutputLogger.NLogError(se, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            header.SalesOrderStatus = form["PreviousStatus"];
                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }

                    ViewData["CompleteMessage"] = "伝票番号「" + header.SlipNumber + "（元伝票）」の赤伝「" + history.SlipNumber + "（新番号）」処理が正常に完了しました。";
                    ViewData["Mode"] = null;
                    ModelState.Clear();
                }
                //表示項目の再セット
                SetDataComponent(ref history2);
                return GetViewResult(history2);
            }

            //表示項目の再セット
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
        /// 2018/11/09 yano #3945 車両伝票　赤黒処理した伝票を見積に戻し、受注に進めても「登録済へ進める」ボタンが表示されない
        /// 2018/11/09 yano #3938 車両伝票入力　赤黒処理時の下取車仕入データの注文書番号の更新漏れ対応
        /// 2018/08/14 yano #3910 車両伝票　デモカーの納車済伝票修正時に車両マスタのロケーションが消える
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akakuro(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            //Add 2014/05/16 yano vs2012移行化対応 オプション行のバインドに失敗時は、POSTデータから値を取得する。
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //postデータより取得したオプション行のデータをセット
            }
            else  //lineがnullでない場合
            {
                header.CarSalesLine = line;
            }


            //Add 2014/05/16 yano vs2012移行化対応 支払行のバインドに失敗時は、POSTデータから値を取得する。
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //lineがnullでない場合
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
 
            // 赤伝処理・赤黒処理時は理由が必須
            ViewData["Mode"] = form["Mode"];

            //Add 2017/11/10 arc yano #3787
            //他のユーザに伝票がロックされているかをチェックする。
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            if (string.IsNullOrEmpty(header.Reason)) {
                if (form["Mode"].Equals("1")) {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "赤伝処理", "理由" }));
                } else {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "赤黒処理", "理由" }));
                }
            } 
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "理由は1024文字以内で入力して下さい");
            }
            CarSalesHeader target1 = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            CarSalesHeader target2 = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-2");
            if (target1 != null || target2 != null) {
                ModelState.AddModelError("SlipNumber", "すでに処理されています");
            }

            if (!ModelState.IsValid) {

                ProcessUnLock(header);  //Add 2017/11/10 arc yano #3787 

                //表示項目の再セット
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            // 赤黒伝処理
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {

                // 赤伝処理
                CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "車両", header.Reason);

                CarSalesHeader history = CreateAkaden(header, true);

                //Add 2018/11/09 yano #3945
                string originalSlipNumber = header.SlipNumber;

                CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(originalSlipNumber);

                //車両発注依頼データが存在した場合は伝票番号を黒伝票に振替
                if (rec != null)
                {
                    rec.SlipNumber = originalSlipNumber + "-2";
                    rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    rec.LastUpdateDate = DateTime.Now;
                }

                // 黒処理
                header.SlipNumber = header.SlipNumber + "-2";
                header.RevisionNumber = 1;
                header.SalesOrderStatus = "003";
                header.CreateDate = DateTime.Now;
                header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.LastUpdateDate = DateTime.Now;
                header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.DelFlag = "0";
                header.LastEditScreen = "000";

                foreach (var item in header.CarSalesLine)
                {
                    item.CreateDate = DateTime.Now;
                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.DelFlag = "0";
                    //MOD 赤伝消費税率不具合対応 2014/11/12 ookubo
                    item.ConsumptionTaxId = header.ConsumptionTaxId;
                    item.Rate = header.Rate;
                }
                foreach (var item in header.CarSalesPayment)
                {
                    item.CreateDate = DateTime.Now;
                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.DelFlag = "0";
                }

                SalesCar salesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                if (salesCar != null)
                {
                    //Mod 2018/08/14 yano #3910 在庫ステータスは「在庫」ではなく「登録済」
                    //salesCar.CarStatus = "001";
                    salesCar.CarStatus = "003";     //引当済
                    salesCar.LastUpdateDate = DateTime.Now;
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                    //Mod 2018/08/14 yano #3910
                    //車両マスタのロケーションコードが空欄の場合、部門→倉庫→ロケーションの最初を設定
                    if (string.IsNullOrWhiteSpace(salesCar.LocationCode))
                    {
                        Department department = new DepartmentDao(db).GetByKey(header.DepartmentCode);

                        if (department != null)
                        {
                            DepartmentWarehouse ret = CommonUtils.GetWarehouseFromDepartment(db, department.DepartmentCode);

                            if(ret != null)
                            {
                                Location loc = new LocationDao(db).GetListByLocationType("001", ret.WarehouseCode, "").FirstOrDefault();

                                salesCar.LocationCode = loc != null ? loc.LocationCode : "";

                                ////Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                                //try { salesCar.LocationCode = department.Location.Where(x => CommonUtils.DefaultString(x.LocationType).Equals("001")).FirstOrDefault().LocationCode; }
                                //catch (NullReferenceException) { }
                            }
                            
                        }
                    }
                }

                //Add 2016/08/29 arc nakayama #3595_【大項目】車両売掛金機能改善
                TransferJournal(header.SlipNumber);

                //Add 2016/08/29 arc nakayama #3595_【大項目】車両売掛金機能改善
                TransfeTraderJournal(header);

                //残債と下取の入金予定作成
                CreateTradeReceiptPlan(header);

                // 入金予定作成
                CreatePaymentPlan(header);


                //Add 2018/11/09 yano #3938
                List<CarPurchase> purchaseList = new CarPurchaseDao(db).GetListBySlipNumberVin(originalSlipNumber, "");

                if (purchaseList != null && purchaseList.Count > 0)
                {
                    foreach (var a in purchaseList)
                    {
                        a.SlipNumber = header.SlipNumber;
                        a.LastUpdateEmployeeCode =  ((Employee)Session["Employee"]).EmployeeCode;
                        a.LastUpdateDate = DateTime.Now;
                    }
                }

                //Add 2016/08/29 arc nakayama #3595_【大項目】車両売掛金機能改善
                //カード会社からの入金予定を黒に振り替える
                CreateKuroCreditPlan(header.SlipNumber);

                db.CarSalesHeader.InsertOnSubmit(header);

                // Add 2014/08/06 arc amii エラーログ対応 try catch文とエラーログ処理を追加
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログ出力
                        OutputLogger.NLogError(se, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                    // エラーページに遷移
                    return View("Error");
                }

                ViewData["CompleteMessage"] = "伝票番号「" + form["SlipNumber"] + "（元伝票）」の赤黒「" + header.SlipNumber + "（納車済み伝票のコピー）」処理が正常に完了しました。";
            }

            ProcessUnLock(header);  //Add 2017/11/10 arc yano #3787 

            //表示項目の再セット
            SetDataComponent(ref header);

            ModelState.Clear();
            
            ViewData["Mode"] = null;

            return GetViewResult(header);
        }

    /// <summary>
    /// マイナスの伝票を作成する
    /// </summary>
    /// <param name="header">元伝票</param>
    /// <returns>新伝票</returns>
    /// <history>
    /// 2023/09/28 yano #4183 インボイス対応(経理対応)
    /// 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
    /// 2023/08/15 yano #4176 販売諸費用の修正
    /// 2022/06/23 yano #4140【車両伝票入力】注文書の登録名義人が表示されない不具合の対応
    /// 2021/06/09 yano #4091 【車両伝票】オプション行の区分追加(メンテナス・延長保証)
    /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
    /// 2017/12/27 arc yano #3820 車両伝票－カード会社からの入金予定、入金実績のある車両伝票の赤伝処理不具合
    /// </history>
    private CarSalesHeader CreateAkaden(CarSalesHeader header, bool Akakuro) {
            // 赤伝処理
            CarSalesHeader history = new CarSalesHeader();
            history.SlipNumber = header.SlipNumber + "-1";
            history.RevisionNumber = 1;
            history.QuoteDate = header.QuoteDate;
            history.QuoteExpireDate = header.QuoteExpireDate;
            history.SalesOrderDate = header.SalesOrderDate;
            history.SalesOrderStatus = "005"; // ステータスは納車済み
            history.ApprovalFlag = header.ApprovalFlag;
            history.SalesDate = DateTime.Today; // 納車日＝システム日付
            history.CustomerCode = header.CustomerCode;
            history.DepartmentCode = header.DepartmentCode;
            history.EmployeeCode = header.EmployeeCode;
            history.CampaignCode1 = header.CampaignCode1;
            history.CampaignCode2 = header.CampaignCode2;
            history.NewUsedType = header.NewUsedType;
            history.SalesType = header.SalesType;
            history.MakerName = header.MakerName;
            history.CarBrandName = header.CarBrandName;
            history.CarName = header.CarName;
            history.CarGradeName = header.CarGradeName;
            history.CarGradeCode = header.CarGradeCode;
            history.ManufacturingYear = header.ManufacturingYear;
            history.ExteriorColorCode = header.ExteriorColorCode;
            history.ExteriorColorName = header.ExteriorColorName;
            history.Vin = header.Vin;
            history.UsVin = header.UsVin;
            history.ModelName = header.ModelName;
            history.Mileage = header.Mileage;
            history.MileageUnit = header.MileageUnit;
            history.RequestPlateNumber = header.RequestPlateNumber;
            history.RegistPlanDate = header.RegistPlanDate;
            history.HotStatus = header.HotStatus;
            history.SalesCarNumber = header.SalesCarNumber;
            history.RequestRegistDate = header.RequestRegistDate;
            history.SalesPlanDate = header.SalesPlanDate;
            history.RegistrationType = header.RegistrationType;
            history.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            history.OwnershipReservation = header.OwnershipReservation;
            history.CarLiabilityInsuranceType = header.CarLiabilityInsuranceType;
            history.SealSubmitDate = header.SealSubmitDate;
            history.ProxySubmitDate = header.ProxySubmitDate;
            history.ParkingSpaceSubmitDate = header.ParkingSpaceSubmitDate;
            history.CarLiabilityInsuranceSubmitDate = header.CarLiabilityInsuranceSubmitDate;
            history.OwnershipReservationSubmitDate = header.OwnershipReservationSubmitDate;
            history.Memo = header.Memo;
            history.SalesPrice = header.SalesPrice * (-1);
            history.DiscountAmount = header.DiscountAmount * (-1);
            history.TaxationAmount = header.TaxationAmount * (-1);
            history.TaxAmount = header.TaxAmount * (-1);
            history.ShopOptionAmount = header.ShopOptionAmount * (-1);
            history.ShopOptionTaxAmount = header.ShopOptionTaxAmount * (-1);
            history.MakerOptionAmount = header.MakerOptionAmount * (-1);
            history.MakerOptionTaxAmount = header.MakerOptionTaxAmount * (-1);
            history.OutSourceAmount = header.OutSourceAmount * (-1);
            history.OutSourceTaxAmount = header.OutSourceTaxAmount * (-1);
            history.SubTotalAmount = header.SubTotalAmount * (-1);
            history.CarTax = header.CarTax * (-1);
            history.CarLiabilityInsurance = header.CarLiabilityInsurance * (-1);
            history.CarWeightTax = header.CarWeightTax * (-1);
            history.AcquisitionTax = header.AcquisitionTax * (-1);
            history.InspectionRegistCost = header.InspectionRegistCost * (-1);
            history.ParkingSpaceCost = header.ParkingSpaceCost * (-1);
            history.TradeInCost = header.TradeInCost * (-1);
            history.RecycleDeposit = header.RecycleDeposit * (-1);
            history.RecycleDepositTradeIn = header.RecycleDepositTradeIn * (-1);
            history.NumberPlateCost = header.NumberPlateCost * (-1);
            history.RequestNumberCost = header.RequestNumberCost * (-1);
            history.TradeInFiscalStampCost = header.TradeInFiscalStampCost * (-1);
            // Add 2014/07/25 課題対応 #3018 収入印紙代と下取自動車税預り金追加
            history.RevenueStampCost = header.RevenueStampCost * (-1);
            history.TradeInCarTaxDeposit = header.TradeInCarTaxDeposit * (-1);
            history.TaxFreeFieldName = header.TaxFreeFieldName;
            history.TaxFreeFieldValue = header.TaxFreeFieldValue * (-1);
            history.TaxFreeTotalAmount = header.TaxFreeTotalAmount * (-1);
            history.InspectionRegistFee = header.InspectionRegistFee * (-1);
            history.ParkingSpaceFee = header.ParkingSpaceFee * (-1);
            history.TradeInFee = header.TradeInFee * (-1);
            history.PreparationFee = header.PreparationFee * (-1);
            history.RecycleControlFee = header.RecycleControlFee * (-1);
            history.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn * (-1);
            history.RequestNumberFee = header.RequestNumberFee * (-1);
            history.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount * (-1);
            history.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount * (-1);
            history.TradeInAppraisalFee = header.TradeInAppraisalFee * (-1);
            history.FarRegistFee = header.FarRegistFee * (-1);
            history.OutJurisdictionRegistFee = header.OutJurisdictionRegistFee * (-1);    //Add 2023/08/15 yano #4176
            history.TradeInMaintenanceFee = header.TradeInMaintenanceFee * (-1);
            history.InheritedInsuranceFee = header.InheritedInsuranceFee * (-1);
            history.TaxationFieldName = header.TaxationFieldName;
            history.TaxationFieldValue = header.TaxationFieldValue * (-1);
            history.SalesCostTotalAmount = header.SalesCostTotalAmount * (-1);
            history.SalesCostTotalTaxAmount = header.SalesCostTotalTaxAmount * (-1);
            history.OtherCostTotalAmount = header.OtherCostTotalAmount * (-1);
            history.CostTotalAmount = header.CostTotalAmount * (-1);
            history.TotalTaxAmount = header.TotalTaxAmount * (-1);
            history.GrandTotalAmount = header.GrandTotalAmount * (-1);
            history.PossesorCode = header.PossesorCode;
            history.UserCode = header.UserCode;
            history.PrincipalPlace = header.PrincipalPlace;
            history.VoluntaryInsuranceType = header.VoluntaryInsuranceType;
            history.VoluntaryInsuranceCompanyName = header.VoluntaryInsuranceCompanyName;
            history.VoluntaryInsuranceAmount = header.VoluntaryInsuranceAmount * (-1);
            history.VoluntaryInsuranceTermFrom = header.VoluntaryInsuranceTermFrom;
            history.VoluntaryInsuranceTermTo = header.VoluntaryInsuranceTermTo;
            history.PaymentPlanType = header.PaymentPlanType;
            history.TradeInAmount1 = header.TradeInAmount1 * (-1);
            history.TradeInTax1 = header.TradeInTax1 * (-1);
            history.TradeInUnexpiredCarTax1 = header.TradeInUnexpiredCarTax1 * (-1);
            history.TradeInRemainDebt1 = header.TradeInRemainDebt1 * (-1);
            history.TradeInAppropriation1 = header.TradeInAppropriation1 * (-1);
            history.TradeInRecycleAmount1 = header.TradeInRecycleAmount1 * (-1);
            history.TradeInMakerName1 = header.TradeInMakerName1;
            history.TradeInCarName1 = header.TradeInCarName1;
            history.TradeInClassificationTypeNumber1 = header.TradeInClassificationTypeNumber1;
            history.TradeInModelSpecificateNumber1 = header.TradeInModelSpecificateNumber1;
            history.TradeInManufacturingYear1 = header.TradeInManufacturingYear1;
            history.TradeInInspectionExpiredDate1 = header.TradeInInspectionExpiredDate1;
            history.TradeInMileage1 = header.TradeInMileage1;
            history.TradeInMileageUnit1 = header.TradeInMileageUnit1;
            history.TradeInVin1 = header.TradeInVin1;
            history.TradeInRegistrationNumber1 = header.TradeInRegistrationNumber1;
            history.TradeInUnexpiredLiabilityInsurance1 = header.TradeInUnexpiredLiabilityInsurance1 * (-1);
            history.TradeInAmount2 = header.TradeInAmount2 * (-1);
            history.TradeInTax2 = header.TradeInTax2 * (-1);
            history.TradeInUnexpiredCarTax2 = header.TradeInUnexpiredCarTax2 * (-1);
            history.TradeInRemainDebt2 = header.TradeInRemainDebt2 * (-1);
            history.TradeInAppropriation2 = header.TradeInAppropriation2 * (-1);
            history.TradeInRecycleAmount2 = header.TradeInRecycleAmount2 * (-1);
            history.TradeInMakerName2 = header.TradeInMakerName2;
            history.TradeInCarName2 = header.TradeInCarName2;
            history.TradeInClassificationTypeNumber2 = header.TradeInClassificationTypeNumber2;
            history.TradeInModelSpecificateNumber2 = header.TradeInModelSpecificateNumber2;
            history.TradeInManufacturingYear2 = header.TradeInManufacturingYear2;
            history.TradeInInspectionExpiredDate2 = header.TradeInInspectionExpiredDate2;
            history.TradeInMileage2 = header.TradeInMileage2;
            history.TradeInMileageUnit2 = header.TradeInMileageUnit2;
            history.TradeInVin2 = header.TradeInVin2;
            history.TradeInRegistrationNumber2 = header.TradeInRegistrationNumber2;
            history.TradeInUnexpiredLiabilityInsurance2 = header.TradeInUnexpiredLiabilityInsurance2 * (-1);
            history.TradeInAmount3 = header.TradeInAmount3 * (-1);
            history.TradeInTax3 = header.TradeInTax3 * (-1);
            history.TradeInUnexpiredCarTax3 = header.TradeInUnexpiredCarTax3 * (-1);
            history.TradeInRemainDebt3 = header.TradeInRemainDebt3 * (-1);
            history.TradeInAppropriation3 = header.TradeInAppropriation3 * (-1);
            history.TradeInRecycleAmount3 = header.TradeInRecycleAmount3 * (-1);
            history.TradeInMakerName3 = header.TradeInMakerName3;
            history.TradeInCarName3 = header.TradeInCarName3;
            history.TradeInClassificationTypeNumber3 = header.TradeInClassificationTypeNumber3;
            history.TradeInModelSpecificateNumber3 = header.TradeInModelSpecificateNumber3;
            history.TradeInManufacturingYear3 = header.TradeInManufacturingYear3;
            history.TradeInInspectionExpiredDate3 = header.TradeInInspectionExpiredDate3;
            history.TradeInMileage3 = header.TradeInMileage3;
            history.TradeInMileageUnit3 = header.TradeInMileageUnit3;
            history.TradeInVin3 = header.TradeInVin3;
            history.TradeInRegistrationNumber3 = header.TradeInRegistrationNumber3;
            history.TradeInUnexpiredLiabilityInsurance3 = header.TradeInUnexpiredLiabilityInsurance3 * (-1);
            history.TradeInTotalAmount = header.TradeInTotalAmount * (-1);
            history.TradeInTaxTotalAmount = header.TradeInTaxTotalAmount * (-1);
            history.TradeInUnexpiredCarTaxTotalAmount = header.TradeInUnexpiredCarTaxTotalAmount * (-1);
            history.TradeInRemainDebtTotalAmount = header.TradeInRemainDebtTotalAmount * (-1);
            history.TradeInAppropriationTotalAmount = header.TradeInAppropriationTotalAmount * (-1);
            history.PaymentTotalAmount = header.PaymentTotalAmount * (-1);
            history.PaymentCashTotalAmount = header.PaymentCashTotalAmount * (-1);
            history.LoanPrincipalAmount = header.LoanPrincipalAmount * (-1);
            history.LoanFeeAmount = header.LoanFeeAmount * (-1);
            history.LoanTotalAmount = header.LoanTotalAmount * (-1);
            history.LoanCodeA = header.LoanCodeA;
            history.PaymentFrequencyA = header.PaymentFrequencyA;
            //Add 20170/02/02 arc nakayama #3489_車両伝票の自動車注文申込書のローンの２回目以降の回数の表記
            history.PaymentSecondFrequencyA = header.PaymentSecondFrequencyA;
            history.PaymentTermFromA = header.PaymentTermFromA;
            history.PaymentTermToA = header.PaymentTermToA;
            history.BonusMonthA1 = header.BonusMonthA1;
            history.BonusMonthA2 = header.BonusMonthA2;
            history.FirstAmountA = header.FirstAmountA * (-1);
            history.SecondAmountA = header.SecondAmountA * (-1);
            history.BonusAmountA = header.BonusAmountA * (-1);
            history.CashAmountA = header.CashAmountA * (-1);
            history.LoanPrincipalA = header.LoanPrincipalA * (-1);
            history.LoanFeeA = header.LoanFeeA * (-1);
            history.LoanTotalAmountA = header.LoanTotalAmountA * (-1);
            history.AuthorizationNumberA = header.AuthorizationNumberA;
            history.FirstDirectDebitDateA = header.FirstDirectDebitDateA;
            history.SecondDirectDebitDateA = header.SecondDirectDebitDateA;
            history.LoanCodeB = header.LoanCodeB;
            history.PaymentFrequencyB = header.PaymentFrequencyB;
            //Add 20170/02/02 arc nakayama #3489_車両伝票の自動車注文申込書のローンの２回目以降の回数の表記
            history.PaymentSecondFrequencyB = header.PaymentSecondFrequencyB;
            history.PaymentTermFromB = header.PaymentTermFromB;
            history.PaymentTermToB = header.PaymentTermToB;
            history.BonusMonthB1 = header.BonusMonthB1;
            history.BonusMonthB2 = header.BonusMonthB2;
            history.FirstAmountB = header.FirstAmountB * (-1);
            history.SecondAmountB = header.SecondAmountB * (-1);
            history.BonusAmountB = header.BonusAmountB * (-1);
            history.CashAmountB = header.CashAmountB * (-1);
            history.LoanPrincipalB = header.LoanPrincipalB * (-1);
            history.LoanFeeB = header.LoanFeeB * (-1);
            history.LoanTotalAmountB = header.LoanTotalAmountB * (-1);
            history.AuthorizationNumberB = header.AuthorizationNumberB;
            history.FirstDirectDebitDateB = header.FirstDirectDebitDateB;
            history.SecondDirectDebitDateB = header.SecondDirectDebitDateB;
            history.LoanCodeC = header.LoanCodeC;
            history.PaymentFrequencyC = header.PaymentFrequencyC;
            //Add 20170/02/02 arc nakayama #3489_車両伝票の自動車注文申込書のローンの２回目以降の回数の表記
            history.PaymentSecondFrequencyC = header.PaymentSecondFrequencyC;       //Mod 2018/11/07 yano #3939対応時に見つけたバグの修正
            history.PaymentTermFromC = header.PaymentTermFromC;
            history.PaymentTermToC = header.PaymentTermToC;
            history.BonusMonthC1 = header.BonusMonthC1;
            history.BonusMonthC2 = header.BonusMonthC2;
            history.FirstAmountC = header.FirstAmountC;
            history.SecondAmountC = header.SecondAmountC * (-1);
            history.BonusAmountC = header.BonusAmountC * (-1);
            history.CashAmountC = header.CashAmountC * (-1);
            history.LoanPrincipalC = header.LoanPrincipalC * (-1);
            history.LoanFeeC = header.LoanFeeC * (-1);
            history.LoanTotalAmountC = header.LoanTotalAmountC * (-1);
            history.AuthorizationNumberC = header.AuthorizationNumberC;
            history.FirstDirectDebitDateC = header.FirstDirectDebitDateC;
            history.SecondDirectDebitDateC = header.SecondDirectDebitDateC;
            history.CancelDate = header.CancelDate;
            history.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            history.CreateDate = DateTime.Now;
            history.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            history.LastUpdateDate = DateTime.Now;
            history.DelFlag = "0";
            history.InspectionRegistFeeTax = header.InspectionRegistFeeTax * (-1);
            history.ParkingSpaceFeeTax = header.ParkingSpaceFeeTax * (-1);
            history.TradeInFeeTax = header.TradeInFeeTax * (-1);
            history.PreparationFeeTax = header.PreparationFeeTax * (-1);
            history.RecycleControlFeeTax = header.RecycleControlFeeTax * (-1);
            history.RecycleControlFeeTradeInTax = header.RecycleControlFeeTradeInTax * (-1);
            history.RequestNumberFeeTax = header.RequestNumberFeeTax * (-1);
            history.CarTaxUnexpiredAmountTax = header.CarTaxUnexpiredAmountTax * (-1);
            history.CarLiabilityInsuranceUnexpiredAmountTax = header.CarLiabilityInsuranceUnexpiredAmountTax * (-1);
            history.TradeInAppraisalFeeTax = header.TradeInAppraisalFeeTax * (-1);
            history.FarRegistFeeTax = header.FarRegistFeeTax * (-1);
            history.OutJurisdictionRegistFeeTax = header.OutJurisdictionRegistFeeTax * (-1);  //Add 2023/08/15 yano #4176
            history.TradeInMaintenanceFeeTax = header.TradeInMaintenanceFeeTax * (-1);
            history.InheritedInsuranceFeeTax = header.InheritedInsuranceFeeTax * (-1);
            history.TaxationFieldValueTax = header.TaxationFieldValueTax * (-1);
            history.TradeInEraseRegist1 = header.TradeInEraseRegist1;
            history.TradeInEraseRegist2 = header.TradeInEraseRegist2;
            history.TradeInEraseRegist3 = header.TradeInEraseRegist3;
            history.RemainAmountA = header.RemainAmountA * (-1);
            history.RemainAmountB = header.RemainAmountB * (-1);
            history.RemainAmountC = header.RemainAmountC * (-1);
            history.RemainFinalMonthA = header.RemainFinalMonthA;
            history.RemainFinalMonthB = header.RemainFinalMonthB;
            history.RemainFinalMonthC = header.RemainFinalMonthC;
            history.LoanRateA = header.LoanRateA;
            history.LoanRateB = header.LoanRateB;
            history.LoanRateC = header.LoanRateC;
            history.SalesTax = header.SalesTax * (-1);
            history.DiscountTax = header.DiscountTax * (-1);
            history.TradeInPrice1 = header.TradeInPrice1 * (-1);
            history.TradeInPrice2 = header.TradeInPrice2 * (-1);
            history.TradeInPrice3 = header.TradeInPrice3 * (-1);
            history.TradeInRecycleTotalAmount = header.TradeInRecycleTotalAmount * (-1);
            history.Reason = header.Reason;
            //ADD 2014/02/20 ookubo
            history.ConsumptionTaxId = header.ConsumptionTaxId;
            history.Rate = header.Rate;
            history.LastEditScreen = "000";

            history.EPDiscountTaxId = header.EPDiscountTaxId;   //Add 2019/09/04 yano #4011

            //Add 2021/06/09 yano #4091
            history.CostAreaCode = header.CostAreaCode;
            history.MaintenancePackageAmount = header.MaintenancePackageAmount * (-1);
            history.MaintenancePackageTaxAmount = header.MaintenancePackageTaxAmount * (-1);
            history.ExtendedWarrantyAmount = header.ExtendedWarrantyAmount * (-1);
            history.ExtendedWarrantyTaxAmount = header.ExtendedWarrantyTaxAmount * (-1);

            //Add 2022/06/23 yano #4140
            history.TradeInHolderName1 = header.TradeInHolderName1;
            history.TradeInHolderName2 = header.TradeInHolderName2;
            history.TradeInHolderName3 = header.TradeInHolderName3;

            //Add 2023/09/18 yano #4181
            history.SurchargeAmount = header.SurchargeAmount * (-1);
            history.SurchargeTaxAmount = header.SurchargeTaxAmount * (-1);

            history.SuspendTaxRecv = header.SuspendTaxRecv * (-1);    //Add 2023/09/28 yano #4183

      db.CarSalesHeader.InsertOnSubmit(history);

            // オプション
            foreach (var item in header.CarSalesLine) {
                CarSalesLine history_line = new CarSalesLine();
                history_line.SlipNumber = history.SlipNumber;
                history_line.RevisionNumber = 1;
                history_line.LineNumber = item.LineNumber;
                history_line.CarOptionCode = item.CarOptionCode;
                history_line.CarOptionName = item.CarOptionName;
                history_line.OptionType = item.OptionType;
                history_line.Amount = item.Amount * (-1);
                history_line.CreateDate = DateTime.Now;
                history_line.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_line.LastUpdateDate = DateTime.Now;
                history_line.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_line.DelFlag = "0";
                history_line.TaxAmount = item.TaxAmount * (-1);
                //ADD 2014/02/20 ookubo
                //MOD 赤伝消費税率不具合対応 2014/11/12 ookubo
                history_line.ConsumptionTaxId = history.ConsumptionTaxId;
                history_line.Rate = history.Rate;

                db.CarSalesLine.InsertOnSubmit(history_line);
            }

            // 支払方法
            foreach (var payment in header.CarSalesPayment) {
                CarSalesPayment history_pay = new CarSalesPayment();
                history_pay.SlipNumber = history.SlipNumber;
                history_pay.RevisionNumber = 1;
                history_pay.LineNumber = payment.LineNumber;
                history_pay.CustomerClaimCode = payment.CustomerClaimCode;
                history_pay.PaymentPlanDate = payment.PaymentPlanDate;
                history_pay.Amount = payment.Amount * (-1);
                history_pay.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_pay.CreateDate = DateTime.Now;
                history_pay.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_pay.LastUpdateDate = DateTime.Now;
                history_pay.DelFlag = "0";
                history_pay.Memo = payment.Memo;
                db.CarSalesPayment.InsertOnSubmit(history_pay);
            }

            // 関連する入金予定データ

            List<ReceiptPlan> planList = new List<ReceiptPlan>();

            if (Akakuro)
            {
                //赤黒処理の場合は全ての入金予定
                planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            }
            else
            {
                //赤の場合は下取りと残債とカード会社からの入金予定を除いた入金予定
                planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => (!x.ReceiptType.Equals("011") && !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013"))).ToList(); //Mod 2017/12/27 arc yano #3820
            }
            
            foreach (var item in planList) {
                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptPlanId = Guid.NewGuid();
                plan.DepartmentCode = item.DepartmentCode;
                plan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                plan.CustomerClaimCode = item.CustomerClaimCode;
                plan.SlipNumber = history.SlipNumber;
                plan.ReceiptType = item.ReceiptType;
                plan.ReceiptPlanDate = item.ReceiptPlanDate;
                plan.AccountCode = item.AccountCode;
                plan.Amount = item.Amount * (-1);
                plan.ReceivableBalance = item.Amount * (-1);
                plan.CompleteFlag = "1"; //入金管理画面に表示しなため
                plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.CreateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.LastUpdateDate = DateTime.Now;
                plan.DelFlag = item.DelFlag;
                plan.Summary = "伝票番号" + header.SlipNumber + "の赤伝処理分";
                plan.JournalDate = item.JournalDate;
                plan.DepositFlag = item.DepositFlag;
                plan.PaymentKindCode = item.PaymentKindCode;
                plan.CommissionRate = item.CommissionRate;
                plan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(plan);
            }

            //元伝票の入金予定は入金済み（CompleteFlag = "1"）にする　これ以上元伝票に対して入金できないようにするため
            List<ReceiptPlan> headerPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var item in headerPlanList)
            {
                item.CompleteFlag = "1";
                item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                item.LastUpdateDate = DateTime.Now;
            }

            //赤黒処理の赤伝の場合は下取りと残債の入金予定も入金済みにする
            if (Akakuro)
            {
                List<ReceiptPlan> TradeplanList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => (x.ReceiptType.Equals("012") || x.ReceiptType.Equals("013"))).ToList();
                foreach (var item in TradeplanList)
                {
                    item.CompleteFlag = "1";
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                }
            }

            //Add 2016/05/23 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
            //入金種別が「カード会社からの入金」になっている入金予定もマイナス分の入金予定を作成する。
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var item in CreditPlanList)
            {
                ReceiptPlan Creditplan = new ReceiptPlan();
                Creditplan.ReceiptPlanId = Guid.NewGuid();
                Creditplan.DepartmentCode = item.DepartmentCode;
                Creditplan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                Creditplan.CustomerClaimCode = item.CustomerClaimCode;
                Creditplan.SlipNumber = history.SlipNumber;
                Creditplan.ReceiptType = item.ReceiptType;
                Creditplan.ReceiptPlanDate = item.ReceiptPlanDate;
                Creditplan.AccountCode = item.AccountCode;
                Creditplan.Amount = item.Amount * (-1);
                Creditplan.ReceivableBalance = item.Amount * (-1);
                Creditplan.CompleteFlag = "1";//入金消込の対象外にするため
                Creditplan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                Creditplan.CreateDate = DateTime.Now;
                Creditplan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                Creditplan.LastUpdateDate = DateTime.Now;
                Creditplan.DelFlag = item.DelFlag;
                Creditplan.Summary = "伝票番号" + header.SlipNumber + "の赤伝処理分";
                Creditplan.JournalDate = item.JournalDate;
                Creditplan.DepositFlag = item.DepositFlag;
                Creditplan.PaymentKindCode = item.PaymentKindCode;
                Creditplan.CommissionRate = item.CommissionRate;
                Creditplan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(Creditplan);
            }
            //元伝票の入金予定は入金済み（CompleteFlag = "1"）にする　これ以上元伝票に対して入金できないようにするため
            List<ReceiptPlan> headerCreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var CreditItem in headerCreditPlanList)
            {
                CreditItem.CompleteFlag = "1";
                CreditItem.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                CreditItem.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            return history;
        }
        /// <summary>
        /// 黒伝票時の入金予定データ作成処理
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// Mod 2016/04/05 arc yano #3441 カード入金消込時マイナスの入金予定ができてしまう 入金実績取得時は請求先種別 =　「クレジット」は除く
        /// </history>
        private void ResetReceiptPlan(CarSalesHeader header) {

            #region 事前処理

            //科目を取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var d in delList) {
                d.DelFlag = "1";
            }
            #endregion

            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();

            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y) {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });

            decimal akaAmount = 0m;
            for (int i = 0; i < payList.Count; i++) {

                // 請求先リストに追加
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                // 初回or請求先が変わったとき、赤伝の入金予定金額を取得する
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    akaAmount = new ReceiptPlanDao(db).GetAmountByCustomerClaim(CommonUtils.DefaultString(header.SlipNumber).Replace("-2", "-1"), payList[i].CustomerClaimCode);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                if (payList[i].Amount >= Math.Abs(akaAmount)) {
                    // 予定額 >= 実績額
                    balanceAmount = ((payList[i].Amount ?? 0m) + akaAmount);
                    akaAmount = 0m;
                } else {
                    // 予定額 < 実績額
                    balanceAmount = 0m;
                    akaAmount = akaAmount + (payList[i].Amount ?? 0m);

                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                    if (i == payList.Count() - 1 || 
                        (i < payList.Count() - 1 && 
                        !CommonUtils.DefaultString((payList[i].CustomerClaimCode)).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode)))) {
                        balanceAmount = akaAmount;
                    }
                }

                CreateReceiptPlan(payList[i], balanceAmount, carAccount.AccountCode);
            }

            //入金済みの請求先が今回の伝票からなくなっているものを通知
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList);  //Mod 2016/04/05 arc yano #3441
            foreach (Journal a in journalList) {
                if (!string.IsNullOrEmpty(a.CustomerClaimCode) && customerClaimList.IndexOf(a.CustomerClaimCode) < 0) {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    //task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // マイナスで入金予定作成
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.ReceiptType = "001"; // 現金
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }

            //Dell 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善 赤黒実行時に入金予定は削除しない
        }
        #endregion

        #region 新規オブジェクト作成
        /// <summary>
        /// 新しい伝票データを作成
        /// </summary>
        /// <param name="header">伝票データ</param>
        private void CreateCarSalesOrder(CarSalesHeader header) {

            //新規の時は伝票番号を採番する
            if (header.RevisionNumber == 0) {
                header.SlipNumber = (new SerialNumberDao(db)).GetNewSlipNumber();
                header.ApprovalFlag = "0";
                header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.CreateDate = DateTime.Now;
            }

            //古いリビジョンは削除
            DateTime updateDate = DateTime.Now;
            string updateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            List<CarSalesHeader> delList = (new CarSalesOrderDao(db)).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber);
            foreach (var d in delList) {
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (!CommonUtils.DefaultString(d.DelFlag).Equals("1"))
                {
                    d.LastUpdateDate = updateDate;
                    d.LastUpdateEmployeeCode = updateEmployeeCode;
                    d.DelFlag = "1";
                }
                foreach (var l in d.CarSalesLine) {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (!CommonUtils.DefaultString(l.DelFlag).Equals("1")) {
                        l.LastUpdateDate = updateDate;
                        l.LastUpdateEmployeeCode = updateEmployeeCode;
                        l.DelFlag = "1";
                    }
                }
                foreach (var p in d.CarSalesPayment) {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (!CommonUtils.DefaultString(p.DelFlag).Equals("1")) {
                        p.LastUpdateDate = updateDate;
                        p.LastUpdateEmployeeCode = updateEmployeeCode;
                        p.DelFlag = "1";
                    }
                }
            }
            header.RevisionNumber++;
            header.CreateDate = DateTime.Now;
            header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;
            header.DelFlag = "0";

            foreach (var l in header.CarSalesLine) {
                l.CreateDate = DateTime.Now;
                l.CreateEmployeeCode = header.CreateEmployeeCode;
                l.LastUpdateDate = DateTime.Now;
                l.LastUpdateEmployeeCode = updateEmployeeCode;
                l.DelFlag = "0";
            }
            foreach (var p in header.CarSalesPayment) {
                p.CreateDate = DateTime.Now;
                p.CreateEmployeeCode = header.CreateEmployeeCode;
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = updateEmployeeCode;
                p.DelFlag = "0";
            }
        }
        #endregion

        #region 発注依頼データを作成
        /// <summary>
        /// 発注依頼データを作成する
        /// </summary>
        /// <param name="header">車両伝票</param>
        private void CreatePurchaseOrderData(CarSalesHeader header)
        {
            CarPurchaseOrder purchaseOrder = new CarPurchaseOrder();
            purchaseOrder.CarPurchaseOrderNumber = new SerialNumberDao(db).GetNewCarPurchaseOrderNumber();
            purchaseOrder.SlipNumber = header.SlipNumber;
            purchaseOrder.CarSalesHeader = header;
            purchaseOrder.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));;
            purchaseOrder.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchaseOrder.LastUpdateDate = DateTime.Now;
            purchaseOrder.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchaseOrder.DelFlag = "0";
            db.CarPurchaseOrder.InsertOnSubmit(purchaseOrder);

            header.CarPurchaseOrder = purchaseOrder;

            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            //発注承認依頼タスクを追加する
            task.CarPurchaseApproval(header);

            //引当依頼タスクを追加する
            task.CarPurchaseOrderRequest(header);
        }
        #endregion

        #region 査定データを作成
        /// <summary>
        /// 下取車３台分の査定データを作成する
        /// </summary>
        /// <param name="header">車両伝票</param>
        private void CreateAppraisalData(CarSalesHeader header, int i, bool OrderFlag)
        {
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
            {
                CarAppraisal appraisal = new CarAppraisal();
                appraisal.CarAppraisalId = Guid.NewGuid();
                appraisal.SlipNumber = header.SlipNumber;
                object makerName = CommonUtils.GetModelProperty(header, "TradeInMakerName" + i);
                appraisal.MakerName = makerName != null ? makerName.ToString() : "";
                object tradeInMileage = CommonUtils.GetModelProperty(header, "TradeInMileage" + i);
                appraisal.Mileage = tradeInMileage != null ? decimal.Parse(tradeInMileage.ToString()) : 0m;
                object tradeInMileageUnit = CommonUtils.GetModelProperty(header, "TradeInMileageUnit" + i);
                appraisal.MileageUnit = tradeInMileageUnit != null ? tradeInMileageUnit.ToString() : "";
                object tradeInInspectionExpiredDate = CommonUtils.GetModelProperty(header, "TradeInInspectionExpiredDate" + i);
                appraisal.InspectionExpireDate = tradeInInspectionExpiredDate != null ? CommonUtils.StrToDateTime(tradeInInspectionExpiredDate.ToString()) : null;
                appraisal.Vin = vin.ToString();
                appraisal.DelFlag = "0";
                appraisal.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                appraisal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                appraisal.LastUpdateDate = DateTime.Now;
                appraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                appraisal.PurchaseCreated = "0";
                object classification = CommonUtils.GetModelProperty(header, "TradeInClassificationTypeNumber" + i);
                appraisal.ClassificationTypeNumber = classification != null ? classification.ToString() : "";
                object modelSpecificate = CommonUtils.GetModelProperty(header, "TradeInModelSpecificateNumber" + i);
                appraisal.ModelSpecificateNumber = modelSpecificate != null ? modelSpecificate.ToString() : "";
                appraisal.DepartmentCode = header.DepartmentCode;
                object eraseRegist = CommonUtils.GetModelProperty(header, "TradeInEraseRegist" + i);
                appraisal.EraseRegist = eraseRegist != null ? eraseRegist.ToString() : "";
                //#3036 査定データに消費税IDと税率を設定追加  2014/06/09 arc.ookubo
                appraisal.ConsumptionTaxId = header.ConsumptionTaxId;
                appraisal.Rate = (int)header.Rate;

                //金額の連携
                object tradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                appraisal.AppraisalPrice = tradeInAmount != null ? decimal.Parse(tradeInAmount.ToString()) : 0m;

                object tradeInUnexpiredCarTax = CommonUtils.GetModelProperty(header, "TradeInUnexpiredCarTax" + i);
                appraisal.CarTaxUnexpiredAmount = tradeInUnexpiredCarTax != null ? decimal.Parse(tradeInUnexpiredCarTax.ToString()) : 0m;

                object tradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                appraisal.RemainDebt = tradeInRemainDebt != null ? decimal.Parse(tradeInRemainDebt.ToString()) : 0m;

                object tradeInRecycleAmount = CommonUtils.GetModelProperty(header, "TradeInRecycleAmount" + i);
                appraisal.RecycleDeposit = tradeInRecycleAmount != null ? decimal.Parse(tradeInRecycleAmount.ToString()) : 0m;

                //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
                //受注時は初回作成なので更新メッセージは出さない
                if (OrderFlag)
                {
                    appraisal.LastEditScreen = "000";
                }
                else
                {
                    appraisal.LastEditScreen = LAST_EDIT_CARSALSEORDER;
                    header.LastEditScreen = "000";
                }

                //査定データINSERT
                db.CarAppraisal.InsertOnSubmit(appraisal);

                //査定依頼作成
                task.CarAppraisal(appraisal);

            }
        }
        #endregion

        #region 既存の査定データを削除
        /// <summary>
        /// 既存の査定データを削除する
        /// </summary>
        /// <param name="header"></param>
        private void DeleteAppraisalData(CarSalesHeader header, int i) {
            CarAppraisalDao dao = new CarAppraisalDao(db);
            List<CarAppraisal> appraisalList;
            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
            if (vin != null && !string.IsNullOrEmpty(vin.ToString())) {
                appraisalList = dao.GetListBySlipNumberVin(header.SlipNumber, vin.ToString(), "0");
                foreach (var item in appraisalList) {
                    item.DelFlag = "1";
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }
        }
        #endregion

        #region 入金予定データを作成
        /// <summary>
        /// 入金予定データを作成する
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/12/22 arc yano #3793 入金予定再作成時の不具合
        /// 2017/12/22 arc yano #3703 車両伝票　部門を変更すると請求金額が倍になる問題
        /// 2016/04/05 arc yano #3441 カード入金消込時マイナスの入金予定ができてしまう 入金実績取得時は請求先種別 =　「クレジット」は除く
        /// </history>
        private void CreatePaymentPlan(CarSalesHeader header) {

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            //既存の入金予定を削除
            //Mod 2017/12/22 arc yano #3703
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            //List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber,header.DepartmentCode);
            foreach (var d in delList) {
                //Add 2016/09/05 arc nakayama #3630_【製造】車両売掛金対応
                //残債と下取以外の入金予定
                if (!d.ReceiptType.Equals("012") && !d.ReceiptType.Equals("013"))
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }

            //ローンの既存入金予定を削除
            //Mod 2017/12/22 arc yano #3793
            //2017/01/05 arc yano test
            List<ReceiptPlan> delLoanList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value);
            //List<ReceiptPlan> delLoanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "004");
            foreach (var d in delLoanList) {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //**現金の入金予定を(再)作成する**
            //請求先、入金予定日の順で並び替え
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y) {
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                return
                    !CommonUtils.DefaultString(x.CustomerClaimCode).Equals(CommonUtils.DefaultString(y.CustomerClaimCode)) ? CommonUtils.DefaultString(x.CustomerClaimCode).CompareTo(CommonUtils.DefaultString(y.CustomerClaimCode)) : //請求先順
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //入金予定日順
                    (0);
            });
            
            //請求先一覧作成用
            List<string> customerClaimList = new List<string>();

            // 入金実績額
            //Add 2017/06/16 arc nakayama #3772_【車両伝票】支払情報にマイナスの金額を入れた時の考慮
            decimal PlusjournalAmount = 0m;
            decimal MinusjournalAmount = 0m;
            for (int i = 0; i < payList.Count; i++) {

                //請求先リストに追加
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                //請求先が変わったら入金済み金額を（再）取得する
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    //Add 2017/06/16 arc nakayama #3772_【車両伝票】支払情報にマイナスの金額を入れた時の考慮
                    PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, true);
                    MinusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, false);
                }

                // 売掛残
                decimal balanceAmount = 0m;

                //Add 2017/06/16 arc nakayama #3772_【車両伝票】支払情報にマイナスの金額を入れた時の考慮
                if (payList[i].Amount >= 0)
                {

                    if (payList[i].Amount >= PlusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - PlusjournalAmount);
                        PlusjournalAmount = 0m;
                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = 0m;
                        PlusjournalAmount = PlusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = PlusjournalAmount * (-1);
                        }
                    }
                }
                else
                {
                    if (payList[i].Amount >= MinusjournalAmount)
                    {
                        // 予定額 >= 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = MinusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        // 次の請求先が異なる場合、ここでマイナスの売掛金を作成しておく
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = MinusjournalAmount * (-1);
                        }
                        
                    }
                    else
                    {
                        // 予定額 < 実績額
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = 0m;
                    }
                }

                CreateReceiptPlan(payList[i], balanceAmount, carAccount.AccountCode);
            }


            //ローンの入金予定
            decimal journalAmount = 0m; //入金済み金額

            //プランが選択されているときだけ処理
            if(!string.IsNullOrEmpty(header.PaymentPlanType)){
                //ローンコードを取得する
                object loanCode = header.GetType().GetProperty("LoanCode" + header.PaymentPlanType).GetValue(header, null);

                //ローンコードは必須
                if (loanCode != null && !loanCode.Equals("")) {
                    Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());

                    //マスタに存在すること
                    if (loan != null) {

                        //請求先リストに追加
                        customerClaimList.Add(loan.CustomerClaimCode);

                        //入金済み金額を取得
                        journalAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, loan.CustomerClaimCode);
                        
                        //ローン元金
                        decimal loanAmount = decimal.Parse(CommonUtils.GetModelProperty(header, "LoanPrincipal" + header.PaymentPlanType).ToString());
                        
                        //入金残
                        decimal receivableBalance = loanAmount - journalAmount;

                        ReceiptPlan plan = new ReceiptPlan();
                        plan.CreateDate = DateTime.Now;
                        plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.DelFlag = "0";
                        plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //経理に入金予定
                        plan.LastUpdateDate = DateTime.Now;
                        plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.ReceiptType = "004"; //ローン
                        plan.SlipNumber = header.SlipNumber;
                        plan.OccurredDepartmentCode = header.DepartmentCode;
                        plan.CustomerClaimCode = loan.CustomerClaimCode;
                        plan.ReceiptPlanDate = CommonUtils.GetPaymentPlanDate(header.SalesOrderDate ?? DateTime.Today, loan.CustomerClaim);
                        plan.AccountCode = carAccount.AccountCode;

                        plan.ReceiptPlanId = Guid.NewGuid();
                        plan.Amount = loanAmount;
                        plan.ReceivableBalance = receivableBalance;
                        if (receivableBalance == 0m) {
                            plan.CompleteFlag = "1";
                        }else{
                            plan.CompleteFlag = "0";
                        }
                        db.ReceiptPlan.InsertOnSubmit(plan);
                    }
                }
            }

            //Mod 2016/04/05 arc yano #3441
            //入金済みの請求先が今回の伝票からなくなっているものを通知
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList, excluedAccountTypetList);
            foreach (Journal a in journalList) {
                if (customerClaimList.IndexOf(a.CustomerClaimCode) < 0) {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    task.CarOverReceive(header,a.CustomerClaimCode,a.Amount);
                    
                    // マイナスで入金予定作成
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.ReceiptType = "001"; // 現金
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                  }
            }
        }
        private void CreateReceiptPlan(CarSalesPayment payment, decimal planAmount, string accountCode) {
            ReceiptPlan plan = new ReceiptPlan();
            plan.ReceiptPlanId = Guid.NewGuid();
            plan.CreateDate = DateTime.Now;
            plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.SlipNumber = payment.CarSalesHeader.SlipNumber;
            plan.LastUpdateDate = DateTime.Now;
            plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.DelFlag = "0";
            plan.DepartmentCode = payment.CarSalesHeader.DepartmentCode;
            plan.ReceiptPlanDate = payment.PaymentPlanDate;
            plan.ReceiptType = "001";
            plan.Amount = payment.Amount;
            plan.CustomerClaimCode = payment.CustomerClaimCode;
            plan.OccurredDepartmentCode = payment.CarSalesHeader.DepartmentCode;
            plan.AccountCode = accountCode;
            plan.ReceivableBalance = planAmount;
            plan.Summary = payment.Memo;
            if (planAmount.Equals(0m)) {
                plan.CompleteFlag = "1";
            } else {
                plan.CompleteFlag = "0";
            }
            plan.JournalDate = payment.CarSalesHeader.SalesOrderDate ?? DateTime.Today;
            db.ReceiptPlan.InsertOnSubmit(plan);
        }
        #endregion


        #region 下取車の入金予定を再作成する。残債があれば残債の入金予定も作成する
        /// <summary>
        /// 下取車の入金予定を再作成する
        /// </summary>
        /// <histtory>
        /// 2017/11/14 arc yano  #3811 車両伝票－下取車の入金予定残高更新不整合 入金予定に車台番号を保持する列を追加
        /// </histtory>
        private void CreateTradeReceiptPlan(CarSalesHeader header)
        {
            //既存の入金予定を削除
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
            foreach (var d in delList)
            {
                //Add 2016/09/05 arc nakayama #3630_【製造】車両売掛金対応
                //残債と下取以外の入金予定
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                    {
                        TradeInAmount = varTradeInAmount.ToString();
                    }

                    var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                    {
                        TradeInRemainDebt = varTradeInRemainDebt.ToString();
                    }

                    //string TradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i).ToString();
                    //string TradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i).ToString();
                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //下取の入金額
                    decimal JournalDebtAmount = 0; //残債の入金額

                    //Mod 2017/11/14 arc yano #3811
                    //下取の入金額取得
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //残債の入金額取得
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013"; //下取
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //☆計算
                    if (TradePlan.ReceivableBalance == 0m)
                    {
                        TradePlan.CompleteFlag = "1";
                    }
                    else
                    {
                        TradePlan.CompleteFlag = "0";
                    }
                    TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.CreateDate = DateTime.Now;
                    TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.LastUpdateDate = DateTime.Now;
                    TradePlan.DelFlag = "0";
                    TradePlan.Summary = "";
                    TradePlan.JournalDate = null;
                    TradePlan.DepositFlag = "0";
                    TradePlan.PaymentKindCode = "";
                    TradePlan.CommissionRate = null;
                    TradePlan.CommissionAmount = null;
                    TradePlan.CreditJournalId = "";
                    TradePlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                    db.ReceiptPlan.InsertOnSubmit(TradePlan);

                    //残債があった場合残債分の入金予定をマイナスで作成する
                    if(!string.IsNullOrEmpty(TradeInRemainDebt)){
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //残債は経理に振り替え
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012"; //残債
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //計算
                        if (RemainDebtPlan.ReceivableBalance == 0m)
                        {
                            RemainDebtPlan.CompleteFlag = "1";
                        }
                        else
                        {
                            RemainDebtPlan.CompleteFlag = "0";
                        }
                        RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.CreateDate = DateTime.Now;
                        RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.LastUpdateDate = DateTime.Now;
                        RemainDebtPlan.DelFlag = "0";
                        RemainDebtPlan.Summary = "";
                        RemainDebtPlan.JournalDate = null;
                        RemainDebtPlan.DepositFlag = "0";
                        RemainDebtPlan.PaymentKindCode = "";
                        RemainDebtPlan.CommissionRate = null;
                        RemainDebtPlan.CommissionAmount = null;
                        RemainDebtPlan.CreditJournalId = "";
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                    //db.SubmitChanges();
                }   
            }
        }

        #endregion

        #region Validation
        /// <summary>
        /// 受注処理時の入力チェック
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        private void ValidateCarSalesOrder(CarSalesHeader header)
        {
            CommonValidate("NewUsedType", "新中区分", header, true);
            CommonValidate("CarGradeCode", "グレードコード", header, true);
            CommonValidate("CustomerCode", "顧客コード", header, true);
            string[] str = new string[] { "A", "B", "C" };

            //Add 2014/05/16 arc yano vs2012対応 header.PaymentPlanTypeのチェックを追加
            if ((header.PaymentPlanType != null) && (header.PaymentPlanType != "")) //header.PaymentPlanTypeがnullや「ローンなし」でない場合
            {
                for (int i = 0; i < 3; i++)
                {
                    //if (header.PaymentPlanType.Equals(str[i])) {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    if (CommonUtils.DefaultString(header.PaymentPlanType).Equals(str[i]))
                    {
                        CommonValidate("LoanCode" + str[i], "ローンコード" + str[i], header, true);
                        object loanCode = header.GetType().GetProperty("LoanCode" + str[i]).GetValue(header, null);
                        if (loanCode != null && !string.IsNullOrEmpty(loanCode.ToString()))
                        {
                            Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());
                            if (loan == null)
                            {
                                ModelState.AddModelError("LoanCode" + str[i], MessageUtils.GetMessage("E0016", "ローン" + str[i]));
                            }
                            else
                            {
                                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                                if (loan.CustomerClaim == null ||
                                    CommonUtils.DefaultString(loan.CustomerClaim.CustomerClaimType).Equals("001"))
                                {
                                    ModelState.AddModelError("LoanCode" + str[i], "指定されたローンの請求先が正しく設定されていません");
                                }
                                if (loan.CustomerClaim.CustomerClaimable.Count == 0)
                                {
                                    ModelState.AddModelError("LoanCode" + str[i], "指定されたローンの請求先に決済条件が設定されていません");
                                }
                            }
                        }
                    }
                }
            }
                    
            // 下取車
            if (header.TradeInAmount1 != null && string.IsNullOrEmpty(header.TradeInVin1)) {
                ModelState.AddModelError("TradeInVin1", MessageUtils.GetMessage("E0001", "下取車(1台目)の車台番号" ));
            }
            if (header.TradeInAmount2 != null && string.IsNullOrEmpty(header.TradeInVin2)) {
                ModelState.AddModelError("TradeInVin2", MessageUtils.GetMessage("E0001", "下取車(2台目)の車台番号"));
            }
            if (header.TradeInAmount3 != null && string.IsNullOrEmpty(header.TradeInVin3)) {
                ModelState.AddModelError("TradeInVin3", MessageUtils.GetMessage("E0001", "下取車(3台目)の車台番号"));
            }

            if (header.CarSalesPayment.Count == 0 && string.IsNullOrEmpty(header.PaymentPlanType)) {
                ModelState.AddModelError("LoanPrincipalAmount", "支払方法が入力されていません");
            }
            if (header.LoanPrincipalAmount > header.LoanTotalAmount) {
                ModelState.AddModelError("LoanPrincipalAmount","支払方法が正しく入力されていません");
            }
            //2017/04/27 arc nakayama #3744_[製造] 売掛金自動調整バッチが起因となる不具合改修
            if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
            {
                ModelState.AddModelError("CustomerCode", "受注以降は顧客コードに上様を使用することはできません。");
            }

            for (int i = 0; i < header.CarSalesPayment.Count; i++)
            {
                if (!string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode) && header.CarSalesPayment[i].CustomerClaimCode.Equals("000001"))
                {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "受注以降は請求先に上様を使用することはできません。");
                }
            }
        }

        /// <summary>
        /// 全ステータス共通の入力チェック
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        /// <history>
        /// 2023/08/15 yano #4176 販売諸費用の修正
        /// 2020/11/17 yano #4059 業販伝票のステータス自動更新（納車前）の不具合について
        /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2018/08/14 yano #3910 車両伝票　デモカーの納車済伝票修正時に車両マスタのロケーションが消える
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について
        /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない
        /// 2018/02/20 arc yano #3858 サービス伝票　納車後の保存処理で、納車日を空欄で保存できてしまう
        /// 2018/01/17 arc yano #3813 車両伝票にてローン元金が0円のまま変わらない現象②
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// </history>
        private void ValidateAllStatus(CarSalesHeader header, FormCollection form, bool OrderFlag = false)
        {
            //基本情報
            CommonValidate("DepartmentCode", "部門コード", header, true);
            CommonValidate("EmployeeCode", "担当者コード", header, true);
            //CommonValidate("CustomerCode", "顧客コード", header, true);
            CommonValidate("QuoteDate", "見積日", header, true);
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //見積保存のとき有効期限必須
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
            {
                CommonValidate("QuoteExpireDate", "見積有効期限", header, true);

                //見積有効期限は見積日以降しか認めない
                if (ModelState.IsValidField("QuoteExpireDate") && header.QuoteDate != null && header.QuoteExpireDate != null) {
                    if (header.QuoteDate != null && DateTime.Compare(header.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, header.QuoteExpireDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                        ModelState.AddModelError("QuoteExpireDate", MessageUtils.GetMessage("E0013", new string[] { "見積有効期限", "見積日以降" }));
                    }
                }
            }
            //Mod 2018/02/20 arc yano #3858 納車日のチェックは伝票修正時も行う
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //納車処理の場合納車日必須、在庫が同一部門内にないといけない
            //Add 2017/05/25 伝票修正対応 伝票修正時はロケーションのチェックを行わない（既に納車済なのでロケーションがNULLになっているため）
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
            //if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005") && !header.ActionType.Equals("ModificationEnd"))
            {
                //Mod 2018/08/14 yano #3910
                //伝票修正時または、デモ登録等はロケーションのチェックを行わない
                //if (!header.ActionType.Equals("ModificationEnd"))
                if (!header.ActionType.Equals("ModificationEnd") &&
                    !(
                        !string.IsNullOrWhiteSpace(header.SalesType) &&
                        (
                            header.SalesType.Equals("006") ||                           //デモ登録
                            header.SalesType.Equals("010") ||                           //レンタカー登録
                            header.SalesType.Equals("011") ||                           //代車登録
                            header.SalesType.Equals("012") ||                           //広報車登録
                            header.SalesType.Equals("013") ||                           //業務車登録
                            header.SalesType.Equals(SALESTYPE_BUSINESSSALES) ||         //業販            //Add 2020/11/17 yano #4059
                            header.SalesType.Equals(SALESTYPE_AUTOAUCTION)              //AA              //Add 2020/11/17 yano #4059
                        )
                    )
                )
                {
                    if (header.SalesCarNumber == null)
                    {
                        ModelState.AddModelError("", "車両が引当られていないため納車できません");
                    }
                    else
                    {
                        SalesCar car = new SalesCarDao(db).GetByKey(header.SalesCarNumber);

                        //Mod 2018/04/10 arc yano #3879
                        if (car == null || car.Location == null)
                        {
                            ModelState.AddModelError("", "車両在庫が部門内のロケーションに存在しないため納車できません");
                        }
                        else
                        {
                            DepartmentWarehouse dw = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                            //使用倉庫が見当たらないか、部門の使用倉庫と車両の在庫のある倉庫が異なる場合
                            if (dw == null || !car.Location.WarehouseCode.Equals(dw.WarehouseCode))
                            {
                                ModelState.AddModelError("", "車両在庫が部門内のロケーションに存在しないため納車できません");
                            }
                        }
                        /*
                        if (car == null || car.Location == null || car.Location.Department == null || !car.Location.DepartmentCode.Equals(header.DepartmentCode))
                        {
                            ModelState.AddModelError("", "車両在庫が部門内のロケーションに存在しないため納車できません");
                        }
                        */
                    }
                }
                CommonValidate("SalesDate", "納車日", header, true);
                //納車日が棚卸締め処理された月であればエラー
                if (header.SalesDate != null) {
                    // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode, header.SalesDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("SalesDate", "月次締め処理が終了しているため指定された納車日での納車はできません");
                    }
                }
            }
            if (!ModelState.IsValidField("SalesOrderDate")) {
                if (ModelState["SalesOrderDate"].Errors.Count() > 0) {
                    ModelState["SalesOrderDate"].Errors.RemoveAt(0);
                }
                ModelState.AddModelError("SalesOrderDate", MessageUtils.GetMessage("E0005", "受注日"));
            }
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //受注以降のステータスでは受注日必須
            if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001")
                && !CommonUtils.DefaultString(header.SalesOrderStatus).Equals("006") 
                && header.SalesOrderDate == null)
            {
                ModelState.AddModelError("SalesOrderDate", MessageUtils.GetMessage("E0009", new string[] { "受注以降", "受注日" }));
            }

            //受注日が締め処理済みの月の場合NG
            //if (header.SalesOrderDate != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode,header.SalesOrderDate,"001")) {
            //    ModelState.AddModelError("SalesOrderDate", "指定された受注日は棚卸締め処理が終了しています");
            //}

            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //新車の場合、車両のグレードが販売対象期間でなければいけない
            if (CommonUtils.DefaultString(header.NewUsedType).Equals("N"))
            {
                CarGrade grade = new CarGradeDao(db).GetByKey(header.CarGradeCode);
                if (grade != null) {
                    if (DateTime.Compare(DateTime.Today, grade.SalesStartDate ?? DaoConst.SQL_DATETIME_MIN) < 0 || DateTime.Compare(DateTime.Today, grade.SalesEndDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                        ModelState.AddModelError("CarGradeCode", "指定されたグレードは販売期間外です");
                    }
                }
            }
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            //ADD 2014/02/20 ookubo
            //納車前ステータスまで納車予定日必須
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("002") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("003") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("004"))
            {
                CommonValidate("SalesPlanDate", "納車予定日", header, true);
            }
            
            //販売車両
            CommonValidate("Mileage", "走行距離", header, false);

            //非課税・不課税項目
            CommonValidate("CarTax", "自動車税種別割", header, false);    //Mod 2019/09/04 yano #4011
            CommonValidate("CarLiabilityInsurance", "自賠責保険料", header, false);
            CommonValidate("CarWeightTax", "自動車重量税", header, false);
            CommonValidate("AcquisitionTax", "自動車税環境性能割", header, false);//Mod 2019/09/04 yano #4011
            CommonValidate("InspectionRegistCost", "検査登録費用", header, false);
            CommonValidate("ParkingSpaceCost", "車庫証明費用", header, false);
            CommonValidate("TradeInCost", "下取車費用", header, false);
            CommonValidate("RecycleDeposit", "リサイクル預託金", header, false);
            CommonValidate("RecycleDepositTradeIn", "リサイクル預託金下取", header, false);
            CommonValidate("NumberPlateCost", "ナンバープレート代", header, false);
            // Add 2014/07/25 arc amii 課題対応 # 3018 収入印紙代と下取自動車税預り金を追加
            CommonValidate("RevenueStampCost", "収入印紙代", header, false);
            CommonValidate("TradeInCarTaxDeposit", "下取自動車税種別割預り金", header, false);  //Mod 2019/09/04 yano #4011

            // Add 2014/07/29 arc amii 課題対応 #3018
            // 非課税自由項目名がnull or 空白 or 空白文字の場合
            if (string.IsNullOrWhiteSpace(header.TaxFreeFieldName))
            {
                
                // 非課税自由項目値が入力されていた場合
                if (header.TaxFreeFieldValue != null)
                {
                    ModelState.AddModelError("TaxFreeFieldName", "非課税自由項目名が未入力です");
                }

            }
            CommonValidate("TaxFreeFieldValue", "非課税自由項目値", header, false);

            //課税項目
            CommonValidate("InspectionRegistFee", "検査登録手続", header, false);
            CommonValidate("ParkingSpaceFee", "車庫証明手続", header, false);
            CommonValidate("TradeInFee", "下取車諸費用", header, false);
            CommonValidate("PreparationFee", "納車準備費用", header, false);
            CommonValidate("CarTaxUnexpiredAmount", "自動車税種別割未経過相当額", header, false);  //Mod 2019/09/04 yano #4011
            CommonValidate("CarLiabilityInsuranceUnexpiredAmount", "自賠責未経過相当額", header, false);
            CommonValidate("RecycleControlFee", "リサイクル資金管理料", header, false);
            CommonValidate("RecycleControlFeeTradeIn", "リサイクル資金管理料下取", header, false);
            CommonValidate("RequestNumberFee", "希望番号費用", header, false);

            // Add 2014/07/29 arc amii 課題対応 #3018
            // 課税自由項目名がnull or 空白 or 空白文字の場合
            if (string.IsNullOrWhiteSpace(header.TaxationFieldName))
            {
                // 課税自由項目値が入力されていた場合
                // Mod 2023/08/15 yano #4176
                if(form["TaxFreeFieldValueWithTax"] != null)
                //if (header.TaxationFieldValue != null)
                {
                    ModelState.AddModelError("TaxationFieldName", "課税自由項目名が未入力です");
                }

            }

            //Add 2020/01/06 yano #4029
            //販売区分が業販、AA、依廃以外の場合は、登録先都道府県の必須チェックを行う
            if (!string.IsNullOrWhiteSpace(header.SalesType) &&
               !header.SalesType.Equals("003") &&
               !header.SalesType.Equals("004") &&
               !header.SalesType.Equals("008")
            )
            {
                CommonValidate("CostAreaCode", "販売区分が「業販」「AA」「依廃」以外の場合、都道府県コード", header, true);
            }

            CommonValidate("TaxationFieldValue", "課税自由項目値", header, false);

            //販売金額
            CommonValidate("SalesPrice", "車両本体価格", header, true);
            CommonValidate("DiscountAmount", "値引金額", header, false);

            //登録情報
            CommonValidate("RequestRegistDate", "登録希望日", header, false);
            //20140219  納車日、納車予定日を基本情報へ記述移動 ookubo
            //CommonValidate("SalesPlanDate", "納車予定日", header, false);
            ////CommonValidate("SalesDate", "納車日", header, false);
            CommonValidate("SealSubmitDate", "印鑑証明", header, false);
            CommonValidate("ProxySubmitDate", "委任状", header, false);
            CommonValidate("ParkingSpaceSubmitDate", "車庫証明", header, false);
            CommonValidate("CarLiabilityInsuranceSubmitDate", "自賠責", header, false);
            CommonValidate("OwnershipReservationSubmitDate", "所有権留保書類", header, false);

            //任意保険
            CommonValidate("VoluntaryInsuranceAmount", "保険金額(年額)", header, false);
            CommonValidate("VoluntaryInsuranceTermFrom", "保険契約開始日", header, false);
            CommonValidate("VoluntaryInsuranceTermTo", "保険契約満了日", header, false);

            string[] loan = new string[] { "", "A", "B", "C" };
            for (int i = 1; i <= 3; i++) {
                //下取車
                CommonValidate("TradeInAmount" + i, "下取車価格(" + i + "台目)", header, false);
                CommonValidate("TradeInTax" + i, "下取車消費税(" + i + "台目)", header, false);
                CommonValidate("TradeInUnexpiredCarTax" + i, "下取車未払自動車税種別割(" + i + "台目)", header, false);  //Mod 2019/09/04 yano #4011
                CommonValidate("TradeInRemainDebt" + i, "下取車残債(" + i + "台目)", header, false);
                CommonValidate("TradeInRecycleAmount" + i, "下取車リサイクル(" + i + "台目)", header, false);
                CommonValidate("TradeInInspectionExpiredDate" + i, "下取車車検満了日(" + i + "台目)", header, false);
                CommonValidate("TradeInMileage" + i, "下取車走行距離(" + i + "台目)", header, false);

                //ローン
                CommonValidate("PaymentFrequency" + loan[i], "ローン" + loan[i] + "支払回数", header, false);
                CommonValidate("PaymentTermFrom" + loan[i], "ローン" + loan[i] + "支払開始日" + loan[i], header, false);
                CommonValidate("PaymentTermTo" + loan[i], "ローン" + loan[i] + "支払終了日" + loan[i], header, false);
                CommonValidate("BonusMonth" + loan[i] + "1", "ローン" + loan[i] + "ボーナス月１", header, false);
                CommonValidate("BonusMonth" + loan[i] + "2", "ローン" + loan[i] + "ボーナス月２", header, false);
                CommonValidate("FirstAmount" + loan[i], "ローン" + loan[i] + "初回金額", header, false);
                CommonValidate("FirstDirectDebitDate" + loan[i], "ローン" + loan[i] + "初回引落日", header, false);
                CommonValidate("SecondDirectDebitDate" + loan[i], "ローン" + loan[i] + "2回目以降引落日", header, false);
                CommonValidate("RemainAmount" + loan[i], "ローン" + loan[i] + "残価金額", header, false);
                CommonValidate("RemainFinalMonth" + loan[i], "ローン" + loan[i] + "残価最終月", header, false);
                //Add 20170/02/02 arc nakayama #3489_車両伝票の自動車注文申込書のローンの２回目以降の回数の表記
                CommonValidate("PaymentSecondFrequency" + loan[i], "ローン" + loan[i] + "2回目以降の支払回数", header, false);

                string fieldName = "LoanRate" + loan[i];
                object value = CommonUtils.GetModelProperty(header, "LoanRate" + loan[i]);
                if (!ModelState.IsValidField(fieldName) || (value != null &&
                                                (!Regex.IsMatch(value.ToString(), @"^\d{1,3}\.\d{1,3}$") && !Regex.IsMatch(value.ToString(), @"^\d{1,3}$")))) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { "ローン金利" + loan[i], "正の整数3桁以内かつ小数3桁以内" }));
                }
            }

            //支払方法
            for (int i = 0; i < header.CarSalesPayment.Count; i++) {
                if (string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode)) {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), MessageUtils.GetMessage("E0001", "請求先コード"));
                }
                if (new CustomerClaimDao(db).GetByKey(header.CarSalesPayment[i].CustomerClaimCode) == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), MessageUtils.GetMessage("E0016", "請求先コード"));
                }
                if (header.CarSalesPayment[i].PaymentPlanDate == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].PaymentPlanDate", i), MessageUtils.GetMessage("E0001", "入金予定日"));
                }
                if (!ModelState.IsValidField(string.Format("pay[{0}].PaymantPlanDate", i))) {
                    ModelState.AddModelError(string.Format("pay[{0}].PaymentPlanDate", i), MessageUtils.GetMessage("E0005", "入金予定日"));
                }
                if (header.CarSalesPayment[i].Amount == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].Amount", i), MessageUtils.GetMessage("E0001", "入金金額"));
                }
                if (Regex.IsMatch(header.CarSalesPayment[i].Amount.ToString(), @"\.")) {
                    ModelState.AddModelError(string.Format("pay[{0}].Amount", i), MessageUtils.GetMessage("E0004", new string[] { "入金金額", "正の整数のみ" }));
                }
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
            if (!string.IsNullOrEmpty(header.EmployeeCode))
            {
                Employee EmployeeData = new EmployeeDao(db).GetByKey(header.EmployeeCode);
                if (EmployeeData == null)
                {
                    ModelState.AddModelError("EmployeeCode", "入力されている社員コードはマスタに存在していません。");
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

            //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善　現金販売合計と支払金額が不一致だった場合、処理を先に進めないようにする
            //Mod 2016/12/08 arc nakayama #3674_支払金額と現金販売合計の差分チェックを受注以降に変更する
            //Mod 2016/12/13 arc nakayama #3678_車両伝票　支払金合計と現金販売合計のバリデーションで、支払金合計にローン手数料が含まれている
            //if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount)
            if (string.IsNullOrEmpty(header.PaymentPlanType))
            {
                if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && ((OrderFlag && header.SalesOrderStatus.Equals("001")) || (header.SalesOrderStatus != "001")))
                {
                    ModelState.AddModelError("", "現金販売合計と支払金額が一致していません。");
                    ModelState.AddModelError("", "下取充当金合計 + 現金(申込金を含む) が現金販売合計と一致するようにしてください。");
                }
            }
            else
            {
                if (header.GrandTotalAmount != header.LoanPrincipalAmount + header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && ((OrderFlag && header.SalesOrderStatus.Equals("001")) || (header.SalesOrderStatus != "001")))
                {
                    ModelState.AddModelError("", "現金販売合計と支払金額が一致していません。");
                    ModelState.AddModelError("", "下取充当金合計 + 現金(申込金を含む) + 支払残額(＝ローン元金) が現金販売合計と一致するようにしてください。");
                }
            }

            //Add 2018/01/17 arc yano #3813
            //ローンプランが選択されているにも関わらず支払残高(ローン元金)が0の場合はメッセージを表示する
            if (!string.IsNullOrWhiteSpace(header.PaymentPlanType))
            {
                if (header.LoanPrincipalAmount == 0)
                {
                    ModelState.AddModelError("LoanPrincipalAmount", "支払残高(ローン元金)が無い場合、ローンプランは設定できません。");
                }
            }

            //Add 2017/01/23 arc nakayama #3690_車輌購入申込金を入金出納帳へ登録した後、車輌伝票の支払方法に入金金額が未登録でも納車済にできてしまう。
            //登録済の時にチェックする
            if (form["PreviousStatus"].Equals("003"))
            {
                List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();


                //入金実績-請求先一覧作成用
                List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

                bool Mflag = false;
                foreach (string JClaim in JournalClaimList)
                {
                    var Ret = from a in payList
                              where a.CustomerClaimCode.Equals(JClaim)
                              select a;

                    if(Ret.Count() == 0){
                        Mflag = true;
                        break;
                    }
                }

                if (Mflag)
                {
                    ModelState.AddModelError("", "入金済の入金実績が支払情報に設定されていません。");
                    ModelState.AddModelError("", "支払情報に入金実績と同じ請求先に対する支払情報を設定して下さい。");
                }
            }

            //Add 2017/04/27 arc nakayama #3744_[製造] 売掛金自動調整バッチが起因となる不具合改修
            if (!form["PreviousStatus"].Equals("001"))
            {
                if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
                {
                    ModelState.AddModelError("CustomerCode", "受注以降は顧客コードに上様を使用することはできません。");
                }

                for (int i = 0; i < header.CarSalesPayment.Count; i++)
                {
                    if (!string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode) && header.CarSalesPayment[i].CustomerClaimCode.Equals("000001"))
                    {
                        ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "受注以降は請求先に上様を使用することはできません。");
                    }
                }
            }

            //修正完了時のバリデーションチェック
            if (header.ActionType.Equals("ModificationEnd"))
            {
                if (string.IsNullOrEmpty(header.Reason))
                {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "修正処理", "理由" }));
                }
                if (!string.IsNullOrEmpty(header.Reason) && header.Reason.Length > 1024)
                {
                    ModelState.AddModelError("Reason", "理由は1024文字以内で入力して下さい");
                }
            }

            //Add 2017/11/10 arc yano #3787
            ValidateProcessLock(header, form);

            //Add 2018/08/07 yano #3911
            //キャンセル時のvalidationチェック
            if (!string.IsNullOrEmpty(form["Cancel"]) && form["Cancel"].Equals("1"))
            {
                //顧客情報取得
                Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);
                string customerType = customer != null ? customer.CustomerType : "";

                if (
                      !header.SalesType.Equals("003") &&     //販売区分=「業販」以外
                      !header.SalesType.Equals("009") &&     //販売区分=「店間移動」以外
                      !(header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) &&    //販売区分=「AA」かつ顧客区分=「AA」以外
                      !(header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))         //販売区分=「依廃」かつ顧客区分=「廃棄」以外
                    )
                {
                    //車両の登録・引当状態をチェック
                    CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //登録状態の場合
                    if (rec != null && !string.IsNullOrWhiteSpace(rec.RegistrationStatus) && rec.RegistrationStatus.Equals("1"))
                    {
                        bool cancelAuth = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "ReservationCancel").EnableFlag;

                        //登録解除権限が無い場合
                        if (!cancelAuth)
                        {
                            ModelState.AddModelError("", "権限が無いため、車両登録済の伝票をキャンセルできません。システム課に依頼してください");
                        }
                    }
                }
            }
        }

         /// <summary>
        /// 伝票ロックチェック
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        /// <param name="form">画面データ</param>
        /// <history>
        /// 2018/07/31 yano.hiroki #3918　最終更新日をチェックする処理を追加
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能
        /// </history>
        private void ValidateProcessLock(CarSalesHeader header, FormCollection form)
        {
            // 編集権限がある場合のみロック制御対象
            Employee loginUser = (Employee)Session["Employee"];
            string departmentCode = header.DepartmentCode;
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            // 自部門・兼務部門１～３・セキュリティレベルALLのどれかに該当したらロックする
            if (!departmentCode.Equals(loginUser.DepartmentCode)
            && !departmentCode.Equals(loginUser.DepartmentCode1)
            && !departmentCode.Equals(loginUser.DepartmentCode2)
            && !departmentCode.Equals(loginUser.DepartmentCode3)
            && !securityLevel.Equals("004"))
            {
                // 何もしない
            }
            else
            {
                // 自分以外がロックしている場合はエラー表示する
                string lockEmployeeName = GetProcessLockUser(header);
                if (!string.IsNullOrEmpty(lockEmployeeName))
                {
                    header.LockEmployeeName = lockEmployeeName;
                    ModelState.AddModelError("", "");
                }
                else
                {
                    //リビジョンチェック
                    //dbから最新の伝票を取得する
                    CarSalesHeader dbHeader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //Mod 2018/07/31 yano.hiroki #3918 最終更新日をチェックするように修正
                    if (dbHeader != null)
                    {
                        //それぞれの時刻のミリ秒を切り捨て
                        DateTime dbtime = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", (dbHeader.LastUpdateDate ?? DateTime.Now)));
                        DateTime formtime = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", (header.LastUpdateDate ?? DateTime.Now)));

                        //リビジョン違いの場合
                        if (dbHeader.RevisionNumber > header.RevisionNumber)
                        {
                            ModelState.AddModelError("RevisionNumber", "保存を行おうとしている伝票は最新ではありません。最新の伝票を開き直した上で編集を行って下さい");
                        }
                        else if (dbtime > formtime)
                        {
                            ModelState.AddModelError("", "保存を行おうとしている伝票は最新ではありません。最新の伝票を開き直した上で編集を行って下さい");
                        }
                        else
                        {
                            // 伝票ロック
                            ProcessLock(header);
                        }
                    }
                    
                }
            }
        }

        #endregion

        #region 画面コンポーネントの設定
        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="salesCar">モデルデータ</param>
        /// <history>
        ///  2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// </history>
        private void SetDataComponent(ref CarSalesHeader carSalesHeader)
        {
            ViewData["displayContents"] = "main";

            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), carSalesHeader.NewUsedType, false);
            ViewData["SalesTypeList"] = CodeUtils.GetSelectListByModel<c_SalesType>(dao.GetSalesTypeAll(false), carSalesHeader.SalesType, false);
            ViewData["MileageUnit"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.MileageUnit, false);
            ViewData["TradeInMileageUnit1"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit1, false);
            ViewData["TradeInMileageUnit2"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit2, false);
            ViewData["TradeInMileageUnit3"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit3, false);
            ViewData["RegistrationTypeList"] = CodeUtils.GetSelectListByModel<c_RegistrationType>(dao.GetRegistrationTypeAll(false), carSalesHeader.RegistrationType, false);
            ViewData["CarLiabilityInsuranceTypeList"] = CodeUtils.GetSelectListByModel<c_CarLiabilityInsuranceType>(dao.GetCarLiabilityInsuranceTypeAll(false), carSalesHeader.CarLiabilityInsuranceType, false);
            ViewData["OwnershipReservationList"] = CodeUtils.GetSelectListByModel<c_OwnershipReservation>(dao.GetOwnershipReservationAll(false), carSalesHeader.OwnershipReservation, false);
            ViewData["VoluntaryInsuranceTypeList"] = CodeUtils.GetSelectListByModel<c_VoluntaryInsuranceType>(dao.GetVoluntaryInsuranceTypeAll(false), carSalesHeader.VoluntaryInsuranceType, false);

            ViewData["TradeInEraseRegistList1"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist1, true);
            ViewData["TradeInEraseRegistList2"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist2, true);
            ViewData["TradeInEraseRegistList3"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist3, true);

            //ADD 2014/02/21 ookubo
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carSalesHeader.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carSalesHeader.ConsumptionTaxId;
            ViewData["Rate"] = carSalesHeader.Rate;
            ViewData["ConsumptionTaxIdOld"] = carSalesHeader.ConsumptionTaxId;
            ViewData["SalesDateOld"] = carSalesHeader.SalesDate;
            ViewData["SalesPlanDateOld"] = carSalesHeader.SalesPlanDate;
            //ADD end

            //Add 2019/09/04 yano #4011
            ViewData["EPDiscountTaxList"] = CodeUtils.GetSelectListByModel(dao.GetEPDiscountTaxList(false, DateTime.Now), carSalesHeader.EPDiscountTaxId, false);

            // Mod 2015/05/18 arc nakayama ルックアップ見直し対応　無効データも表示はさせる(Employee/Department/Customer)
            carSalesHeader.Department = new DepartmentDao(db).GetByKey(carSalesHeader.DepartmentCode, true);
            if (carSalesHeader.Department != null)
            {
                ViewData["DepartmentName"] = carSalesHeader.Department.DepartmentName;
            }
            carSalesHeader.Employee = new EmployeeDao(db).GetByKey(carSalesHeader.EmployeeCode, true);
            if (carSalesHeader.Employee != null)
            {
                ViewData["EmployeeName"] = carSalesHeader.Employee.EmployeeName;
            }
            carSalesHeader.ExteriorColor = new CarColorDao(db).GetByKey(carSalesHeader.ExteriorColorCode);
            if (carSalesHeader.ExteriorColor != null)
            {
                ViewData["ExteriorColorName"] = carSalesHeader.ExteriorColor.CarColorName;
            }
            carSalesHeader.InteriorColor = new CarColorDao(db).GetByKey(carSalesHeader.InteriorColorCode);
            if (carSalesHeader.InteriorColor != null)
            {
                ViewData["InteriorColorName"] = carSalesHeader.InteriorColor.CarColorName;
            }
            carSalesHeader.Customer = new CustomerDao(db).GetByKey(carSalesHeader.CustomerCode, true);
            if (carSalesHeader.Customer != null)
            {
                ViewData["CustomerName"] = carSalesHeader.Customer.CustomerName; //carSalesHeader.Customer.FamilyName + "&nbsp;" + carSalesHeader.Customer.FirstName;
                ViewData["CustomerAddress"] = carSalesHeader.Customer.Prefecture + carSalesHeader.Customer.City + carSalesHeader.Customer.Address1 + carSalesHeader.Customer.Address2;
                // Add 2014/09/26 arc amii 登録時住所再確認チェック対応 #3098 住所再確認フラグを取得する
                ViewData["AddressReconfirm"] = carSalesHeader.Customer.AddressReconfirm;

                //Add 2014/10/29 arc amii 住所再確認メッセージ対応 #3119 住所再確認フラグが立っていた場合、メッセージを設定する
                if (carSalesHeader.Customer.AddressReconfirm == true)
                {
                    ViewData["ReconfirmMessage"] = "住所を再確認してください";
                }
                else
                {
                    ViewData["ReconfirmMessage"] = "";
                }
            }
            else
            {
                ViewData["AddressReconfirm"] = false;
                ViewData["ReconfirmMessage"] = "";
            }
            carSalesHeader.Possesor = new CustomerDao(db).GetByKey(carSalesHeader.PossesorCode);
            if (carSalesHeader.Possesor != null)
            {
                ViewData["PossesorName"] = carSalesHeader.Possesor.CustomerName;
                ViewData["PossesorAddress"] = carSalesHeader.Possesor.Prefecture + carSalesHeader.Possesor.City + carSalesHeader.Possesor.Address1 + carSalesHeader.Possesor.Address2;
            }
            carSalesHeader.User = new CustomerDao(db).GetByKey(carSalesHeader.UserCode);
            if (carSalesHeader.User != null)
            {
                ViewData["UserName"] = carSalesHeader.User.CustomerName;
                ViewData["UserAddress"] = carSalesHeader.User.Prefecture + carSalesHeader.User.City + carSalesHeader.User.Address1 + carSalesHeader.User.Address2;
            }

            carSalesHeader.LoanA = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeA);
            if (carSalesHeader.LoanA != null)
            {
                ViewData["LoanNameA"] = carSalesHeader.LoanA.LoanName;
            }
            carSalesHeader.LoanB = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeB);
            if (carSalesHeader.LoanB != null)
            {
                ViewData["LoanNameB"] = carSalesHeader.LoanB.LoanName;
            }
            carSalesHeader.LoanC = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeC);
            if (carSalesHeader.LoanC != null)
            {
                ViewData["LoanNameC"] = carSalesHeader.LoanC.LoanName;
            }

            carSalesHeader.c_SalesOrderStatus = dao.GetSalesOrderStatusByKey(carSalesHeader.SalesOrderStatus);

            for(int i=0;i<carSalesHeader.CarSalesLine.Count;i++)
            {
                ViewData["OptionTypeList[" + i + "]"] = CodeUtils.GetSelectListByModel(dao.GetOptionTypeAll(false), carSalesHeader.CarSalesLine[i].OptionType, false);
            }

            decimal TradeInAppropriationTotal = (carSalesHeader.TradeInAppropriation1 ?? 0) + (carSalesHeader.TradeInAppropriation2 ?? 0) + (carSalesHeader.TradeInAppropriation3 ?? 0);
            decimal PaymentCashAmount = new decimal(0);
            for (int i = 0; i < carSalesHeader.CarSalesPayment.Count; i++)
            {
                carSalesHeader.CarSalesPayment[i].CustomerClaim = new CustomerClaimDao(db).GetByKey(carSalesHeader.CarSalesPayment[i].CustomerClaimCode);
                if(carSalesHeader.CarSalesPayment[i].CustomerClaim!=null){
                    ViewData["CustomerClaimName[" + i + "]"] = carSalesHeader.CarSalesPayment[i].CustomerClaim.CustomerClaimName;
                    PaymentCashAmount += carSalesHeader.CarSalesPayment[i].Amount ?? 0;
                }
            }

            ViewData["TradeInAppropriationTotal"] = TradeInAppropriationTotal;
            ViewData["PaymentTotalAmount"] = carSalesHeader.GrandTotalAmount - TradeInAppropriationTotal;
            ViewData["PaymentCashAmount"] = PaymentCashAmount;
            ViewData["PaymentRemainAmount"] = carSalesHeader.GrandTotalAmount - TradeInAppropriationTotal - PaymentCashAmount;

            carSalesHeader = CalcAmount(carSalesHeader);

            carSalesHeader.BasicHasErrors = BasicHasErrors();
            carSalesHeader.UsedHasErrors = UsedHasErrors();
            carSalesHeader.InvoiceHasErrors = InvoiceHasErrors();
            carSalesHeader.LoanHasErrors = LoanHasErrors();
            carSalesHeader.VoluntaryInsuranceHasErrors = VoluntaryInsuranceHasErrors();

            //下取車が、仕入済なら編集不可にする
            for (int i = 1; i <= 3; i++)
            {
                ViewData["TradeInVinLock" + i] = "0";
                object vin = CommonUtils.GetModelProperty(carSalesHeader, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrWhiteSpace(vin.ToString()))
                {
                    CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(carSalesHeader.SlipNumber, vin.ToString());
                    if (CarPurchaseData != null)
                    {
                        //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
                        if (CarPurchaseData.PurchaseStatus == "002" && !new InventoryScheduleDao(db).IsClosedInventoryMonth(CarPurchaseData.DepartmentCode, CarPurchaseData.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                        {

                            ViewData["TradeInVinLock" + i] = "1";
                        }
                    }
                }
            }

            //Add 2017/06/29 arc nakayama #3761_サブシステムの伝票戻しの移行
            //動作属性をクリアする
            carSalesHeader.ActionType = "";

            //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            //最後に金額の変動があった画面が車両仕入画面でなければメッセージ表示
            /*if (!carSalesHeader.LastEditScreen.Equals(LAST_EDIT_CARSALSEORDER))
            {
                switch (carSalesHeader.LastEditScreen)
                {
                    case "002":
                        carSalesHeader.LastEditMessage = "査定画面から下取車充当金、自税充当、リサイクルの各金額が変更されました。";
                        break;
                    case "003":
                        carSalesHeader.LastEditMessage = "車両仕入画面から下取車充当金、自税充当、リサイクルの各金額が変更されました。";
                        break;
                    default:
                        carSalesHeader.LastEditMessage = "";
                        break;
                }

            }
            else
            {
                carSalesHeader.LastEditMessage = "";
            }*/


            //Add 2017/11/21 arc yano 
            //イベント１
            carSalesHeader.CampaignName1 = new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode1) != null ? new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode1).CampaignName : "";

            //イベント２
            carSalesHeader.CampaignName2 = new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode2) != null ? new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode2).CampaignName : "";

            //Add 2020/01/06 yano #4029
            carSalesHeader.CostArea = new CostAreaDao(db).GetByKey(carSalesHeader.CostAreaCode);
        }
    #endregion

    #region 計算ロジック
    /// <summary>
    /// 合計金額を計算する
    /// </summary>
    /// <param name="header">車両伝票データ</param>
    /// <returns>合計金額を上書きしたデータ</returns>
    /// <history>
    /// 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
    /// 2023/09/05 yano #4162 インボイス対応
    /// 2023/08/15 yano #4176 販売諸費用の修正
    /// 2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加
    /// 2022/08/30 yano #4150【車両伝票入力】入金消込済のローン元金が更新される不具合の対応 消込判断の変更
    /// 2021/06/09 yano #4091 【車両伝票】オプション行の区分追加(メンテナス・延長保証)
    /// 2014/07/25 arc amii 課題対応 #3018 非課税合計に収入印紙代と下取自動車税預かり金を追加
    /// </history>
    private CarSalesHeader CalcAmount(CarSalesHeader header) {

            #region オプション合計計算
            decimal shopOptionTotal = new decimal(0); //販売店オプション合計
            decimal makerOptionTotal = new decimal(0); //メーカーオプション合計
            decimal shopOptionTaxTotal = new decimal(0); //販売店オプション消費税合計
            decimal makerOptionTaxTotal = new decimal(0); //メーカーオプション消費税合計

            //Add 2021/06/09 #4091 yano
            decimal maintenancePackageAmount = 0;
            decimal maintenancePackageTaxAmount = 0;
            decimal extendedWarrantyAmount = 0;
            decimal extendedWarrantyTaxAmount = 0;

            //Add 2023/09/18 yano #4181
            decimal surchargeAmount = 0;
            decimal surchargeTaxAmount = 0;

            foreach (var a in header.CarSalesLine) {
                switch(a.OptionType){
                    
                    //Mod 2023/09/18 yano #4181
                   //case "001": //販売店
                   //     shopOptionTotal += a.Amount ?? 0;
                   //     shopOptionTaxTotal += a.TaxAmount ?? 0;
                   //     break;

                    case "002": //メーカー
                        makerOptionTotal += a.Amount ?? 0;
                        makerOptionTaxTotal += a.TaxAmount ?? 0;
                        break;

                    //Add 2021/06/09 yano #4091
                    case "004": //メンテナンス
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      maintenancePackageAmount += a.Amount ?? 0;
                      maintenancePackageTaxAmount += a.TaxAmount ?? 0;
                      break;

                    case "005": //延長保証
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      extendedWarrantyAmount += a.Amount ?? 0;
                      extendedWarrantyTaxAmount += a.TaxAmount ?? 0;
                      break;

                     //Add 2023/09/18 yano #4181
                     case "006": //サーチャージ
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      surchargeAmount += a.Amount ?? 0;
                      surchargeTaxAmount += a.TaxAmount ?? 0;
                      break;

                     //Add 2023/09/18 yano #4181
                     default: //販売店(001)、暫定追加分
                        shopOptionTotal += a.Amount ?? 0;
                        shopOptionTaxTotal += a.TaxAmount ?? 0;
                        break;
                }
            }
            //オプション合計を更新
            //消費税率処理追加　2014/02/20 ookubo
            header.ShopOptionAmount = shopOptionTotal;
            header.MakerOptionAmount = makerOptionTotal;
            //header.ShopOptionAmount = CommonUtils.CalculateConsumptionTax(shopOptionTotal, header.Rate);
            //header.MakerOptionAmount = CommonUtils.CalculateConsumptionTax(makerOptionTotal, header.Rate);
            //header.ShopOptionAmount = shopOptionTotal;
            //header.MakerOptionAmount = makerOptionTotal;

            //オプション消費税を更新
            //消費税率処理追加　2014/02/20 ookubo
            header.ShopOptionTaxAmount = shopOptionTaxTotal;
            header.MakerOptionTaxAmount = makerOptionTaxTotal;
            //header.ShopOptionTaxAmount = CommonUtils.CalculateConsumptionTax(shopOptionTaxTotal, header.Rate);
            //header.MakerOptionTaxAmount = CommonUtils.CalculateConsumptionTax(makerOptionTaxTotal, header.Rate);
            //header.ShopOptionTaxAmount = shopOptionTaxTotal;
            //header.MakerOptionTaxAmount = makerOptionTaxTotal;

            #endregion

            #region 税金等合計
            header.TaxFreeTotalAmount = (header.CarTax ?? 0) + (header.CarLiabilityInsurance ?? 0) + (header.AcquisitionTax ?? 0) + (header.CarWeightTax ?? 0);
            #endregion

            #region その他非課税合計
            // Mod 2014/07/25 arc amii
            header.OtherCostTotalAmount = (header.InspectionRegistCost ?? 0) + (header.ParkingSpaceCost ?? 0)
                + (header.TradeInCost ?? 0) + (header.RecycleDeposit ?? 0) //+ (header.RecycleDepositTradeIn ?? 0)
                + (header.NumberPlateCost ?? 0) + (header.RequestNumberCost ?? 0) //+ (header.TradeInFiscalStampCost ?? 0)
                + (header.RevenueStampCost ?? 0)
                + (header.TradeInCarTaxDeposit ?? 0)
                + (header.TaxFreeFieldValue ?? 0)
                + (header.VoluntaryInsuranceAmount ?? 0);   // Add 2023/01/11 yano #4158
            #endregion

            #region 販売諸費用
            //Mod 2023/08/15 yano #4176 comment out
            //header.InspectionRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.InspectionRegistFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.PreparationFeeTax = CommonUtils.CalculateConsumptionTax(header.PreparationFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.FarRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.FarRegistFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.RecycleControlFeeTax = CommonUtils.CalculateConsumptionTax(header.RecycleControlFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.ParkingSpaceFeeTax = CommonUtils.CalculateConsumptionTax(header.ParkingSpaceFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.RequestNumberFeeTax = CommonUtils.CalculateConsumptionTax(header.RequestNumberFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.TradeInMaintenanceFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInMaintenanceFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.TradeInFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.TradeInAppraisalFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInAppraisalFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.InheritedInsuranceFeeTax = CommonUtils.CalculateConsumptionTax(header.InheritedInsuranceFee, header.Rate);//消費税率処理追加　2014/02/20 ookubo
            //header.TaxationFieldValueTax = CommonUtils.CalculateConsumptionTax(header.TaxationFieldValue, header.Rate);//消費税率処理追加　2014/02/20 ookubo

            //header.InspectionRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.InspectionRegistFee);
            //header.PreparationFeeTax = CommonUtils.CalculateConsumptionTax(header.PreparationFee);
            //header.FarRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.FarRegistFee);
            //header.RecycleControlFeeTax = CommonUtils.CalculateConsumptionTax(header.RecycleControlFee);
            //header.ParkingSpaceFeeTax = CommonUtils.CalculateConsumptionTax(header.ParkingSpaceFee);
            //header.RequestNumberFeeTax = CommonUtils.CalculateConsumptionTax(header.RequestNumberFee);
            //header.TradeInMaintenanceFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInMaintenanceFee);
            //header.TradeInFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInFee);
            //header.TradeInAppraisalFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInAppraisalFee);
            //header.InheritedInsuranceFeeTax = CommonUtils.CalculateConsumptionTax(header.InheritedInsuranceFee);
            //header.TaxationFieldValueTax = CommonUtils.CalculateConsumptionTax(header.TaxationFieldValue);

            header.SalesCostTotalAmount = (header.InspectionRegistFee ?? 0) + (header.PreparationFee ?? 0) + (header.FarRegistFee ?? 0)
                                        + (header.RecycleControlFee ?? 0) + (header.ParkingSpaceFee ?? 0) + (header.RequestNumberFee ?? 0)
                                        + (header.TradeInMaintenanceFee ?? 0) + (header.TradeInFee ?? 0)
                                        + (header.OutJurisdictionRegistFee ?? 0) + (header.InheritedInsuranceFee ?? 0) + (header.TaxationFieldValue ?? 0)
                                        + (header.CarTaxUnexpiredAmount ?? 0) + (header.CarLiabilityInsuranceUnexpiredAmount ?? 0);                   //Add 2023/09/05  #4162
                                        //+ (header.TradeInAppraisalFee ?? 0) + (header.InheritedInsuranceFee ?? 0) + (header.TaxationFieldValue ?? 0);

            //販売諸費用消費税
            header.SalesCostTotalTaxAmount = (header.InspectionRegistFeeTax ?? 0) + +(header.PreparationFeeTax ?? 0) + (header.FarRegistFeeTax ?? 0)
                                           + (header.RecycleControlFeeTax ?? 0) + (header.ParkingSpaceFeeTax ?? 0) + (header.RequestNumberFeeTax ?? 0)
                                           + (header.TradeInMaintenanceFeeTax ?? 0) + (header.TradeInFeeTax ?? 0)
                                           + (header.OutJurisdictionRegistFeeTax ?? 0) + (header.InheritedInsuranceFeeTax ?? 0) + (header.TaxationFieldValueTax ?? 0)
                                           + (header.CarTaxUnexpiredAmountTax ?? 0) + (header.CarLiabilityInsuranceUnexpiredAmountTax ?? 0);                   //Add 2023/09/05  #4162
                                           //+ (header.TradeInAppraisalFeeTax ?? 0) + (header.InheritedInsuranceFeeTax ?? 0) + (header.TaxationFieldValueTax ?? 0);
            #endregion

            //諸費用合計
            header.CostTotalAmount = (header.TaxFreeTotalAmount ?? 0) + (header.OtherCostTotalAmount ?? 0) + (header.SalesCostTotalAmount ?? 0);

            //車両本体価格
            header.SalesPrice = header.SalesPrice ?? 0;
            header.SalesTax = header.SalesTax ?? 0;
            
            //値引額
            header.DiscountAmount = header.DiscountAmount ?? 0;
            header.DiscountTax = header.DiscountTax ?? 0;

            //課税対象額
            header.TaxationAmount = header.SalesPrice - header.DiscountAmount + header.MakerOptionAmount;

            //車両販売価格合計
            header.SubTotalAmount = shopOptionTotal + makerOptionTotal + header.SalesPrice - header.DiscountAmount;

            //消費税合計を更新
            header.TotalTaxAmount = (header.SalesTax ?? 0) - (header.DiscountTax ?? 0) + (header.ShopOptionTaxAmount ?? 0) + (header.MakerOptionTaxAmount ?? 0)
                    + (header.OutSourceTaxAmount ?? 0) + (header.SalesCostTotalTaxAmount ?? 0);

            //請求合計
            header.GrandTotalAmount = (header.SubTotalAmount ?? 0) + (header.CostTotalAmount ?? 0) + (header.TotalTaxAmount ?? 0);

            //支払方法変更時用
            decimal paymentAmount = new decimal(0);
            foreach (var a in header.CarSalesPayment) {
                paymentAmount += a.Amount ?? 0;
            }
            header.PaymentCashTotalAmount = paymentAmount;
            
            //ローンが選択されている場合、ローン会社からすでに入金済みかどうかをチェックする
            Loan LoanData = new Loan();

            if(!string.IsNullOrEmpty(header.LoanCodeA)){

                LoanData = new LoanDao(db).GetByKey(header.LoanCodeA, false);
            }
            else if (!string.IsNullOrEmpty(header.LoanCodeB))
            {
                LoanData = new LoanDao(db).GetByKey(header.LoanCodeB, false);

            }
            else if (!string.IsNullOrEmpty(header.LoanCodeC))
            {
                LoanData = new LoanDao(db).GetByKey(header.LoanCodeB, false);
            }
            else
            {
                //ローン選択時、ローンコードは必須なのでここに到達することはない
                LoanData = null;
            }

            if (LoanData != null)
            {
                //該当伝票のローンの入金予定を取得する
                ReceiptPlan LoanDataPlan = new ReceiptPlanDao(db).GetByCustomerClaim(header.SlipNumber, LoanData.CustomerClaimCode);
                if (LoanDataPlan != null)
                {
                    if (!string.IsNullOrEmpty(LoanDataPlan.CompleteFlag) && !LoanDataPlan.CompleteFlag.Equals("1"))
                    {
                        header.LoanPrincipalAmount = header.PaymentTotalAmount - paymentAmount;
                    }
                }
                else
                {
                    //何もしない（画面上の値を使用）
                }
            }
            else
            {
                header.LoanPrincipalAmount = header.PaymentTotalAmount - paymentAmount;
            }

            header.LoanPrincipalA = header.LoanPrincipalAmount;
            header.LoanPrincipalB = header.LoanPrincipalAmount;
            header.LoanPrincipalC = header.LoanPrincipalAmount;
            header.LoanTotalAmountA = (header.LoanFeeA ?? 0) + (header.LoanPrincipalA ?? 0);
            header.LoanTotalAmountB = (header.LoanFeeB ?? 0) + (header.LoanPrincipalB ?? 0);
            header.LoanTotalAmountC = (header.LoanFeeC ?? 0) + (header.LoanPrincipalC ?? 0);
            return header;
        }
        #endregion

        #region 黒伝票の入金予定作成前に入金実績を黒伝票に振り返る
        /// <summary>
        /// 黒伝票の入金予定作成前に入金実績を黒伝票に振り返る
        /// </summary>
        /// <param name="SlipNumber">黒伝票番号</param>
        /// <history>
        /// Add 2016/09/29 arc nakayama #3595_【大項目】車両売掛金機能改善
        /// </history>
        private void TransferJournal(string SlipNumber)
        {
            //元伝票の伝票番号で検索するためreplaceする
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //元伝票の入金実績を全て黒伝票に振り替える
            List<Journal> OriginalJournalList = new JournalDao(db).GetListBySlipNumber(OriginalSlipNumber);

            foreach (var OriginJournal in OriginalJournalList)
            {
                OriginJournal.SlipNumber = SlipNumber; //黒伝票番号
                OriginJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                OriginJournal.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            //元伝票の入金実績が全て黒に振り替わったため、もとの入金予定の残高を全て入金前に戻す
            List<ReceiptPlan> OriginalPlanList = new ReceiptPlanDao(db).GetBySlipNumber(OriginalSlipNumber);

            foreach (var OriginalPlan in OriginalPlanList)
            {
                OriginalPlan.ReceivableBalance = OriginalPlan.Amount;
                OriginalPlan.CompleteFlag = "1"; //元伝票に対して入金させないため完了フラグを立てる
                OriginalPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                OriginalPlan.LastUpdateDate = DateTime.Now;
            }
        }

        #endregion


        #region 黒伝票作成前に下取車と残債の入金実績を黒伝票に振り替える
        /// <summary>
        /// 黒伝票の入金予定作成前に入金実績を黒伝票に振り返る
        /// </summary>
        /// <param name="SlipNumber">黒伝票番号</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 車両伝票－下取車の入金予定残高更新不整合
        /// 2016/09/29 arc nakayama #3595_【大項目】車両売掛金機能改善 新規作成
        /// </history>
        private void TransfeTraderJournal(CarSalesHeader header)
        {
            //元伝票の伝票番号で検索するためreplaceする
            string OriginalSlipNumber = header.SlipNumber.Replace("-2", "");

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var RetTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (RetTradeInAmount != null)
                    {
                        TradeInAmount = RetTradeInAmount.ToString();
                    }

                    var RetTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (RetTradeInRemainDebt != null)
                    {
                        TradeInRemainDebt = RetTradeInRemainDebt.ToString();
                    }

                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);

                    //Mod 2017/11/14 arc yano #3811 
                    //下取の入金実績取得
                    Journal OriginalJournalData = new JournalDao(db).GetTradeJournal(OriginalSlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal OriginalJournalData = new JournalDao(db).GetListByCustomerAndSlip(OriginalSlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (OriginalJournalData != null)
                    {
                        OriginalJournalData.SlipNumber = header.SlipNumber;
                        OriginalJournalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        OriginalJournalData.LastUpdateDate = DateTime.Now;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //残債の入金額取得
                    Journal OriginalJournalData2 = new JournalDao(db).GetTradeJournal(OriginalSlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal OriginalJournalData2 = new JournalDao(db).GetListByCustomerAndSlip(OriginalSlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (OriginalJournalData2 != null)
                    {
                        OriginalJournalData2.SlipNumber = header.SlipNumber;
                        OriginalJournalData2.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        OriginalJournalData2.LastUpdateDate = DateTime.Now;
                    }
                }
            }
        }

        #endregion

        #region カード会社からの入金予定を黒伝票分で作成する
        /// <summary>
        /// カード会社からの入金予定を黒伝票分で作成する
        /// </summary>
        /// <param name="SlipNumber">黒伝票番号</param>
        /// <history>
        /// Add 2016/05/25 arc nakayama #3418_赤黒伝票発行時の黒伝票の入金予定（ReceiptPlan）の残高の計算方法
        /// </history>
        private void CreateKuroCreditPlan(string SlipNumber)
        {
            //元伝票の伝票番号で検索するためreplaceする
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //カード会社からの入金予定取得
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(OriginalSlipNumber, "011");

            foreach (var CreditPlan in CreditPlanList)
            {
                decimal? ReceivableBalance = CreditPlan.ReceivableBalance;
                //カード会社からの入金予定に対して入金実績があった場合は残高を更新する
                Journal JournalData = new JournalDao(db).GetByPlanIDAccountType(CreditPlan.ReceiptPlanId.ToString().ToUpper(), "011");
                if (JournalData != null)
                {
                    ReceivableBalance -= JournalData.Amount;
                }

                ReceiptPlan KuroCreditPlan = new ReceiptPlan();
                KuroCreditPlan.ReceiptPlanId = Guid.NewGuid();
                KuroCreditPlan.DepartmentCode = CreditPlan.DepartmentCode;
                KuroCreditPlan.OccurredDepartmentCode = CreditPlan.OccurredDepartmentCode;
                KuroCreditPlan.CustomerClaimCode = CreditPlan.CustomerClaimCode;
                KuroCreditPlan.SlipNumber = SlipNumber;
                KuroCreditPlan.ReceiptType = CreditPlan.ReceiptType;
                KuroCreditPlan.ReceiptPlanDate = CreditPlan.ReceiptPlanDate;
                KuroCreditPlan.AccountCode = CreditPlan.AccountCode;
                KuroCreditPlan.Amount = CreditPlan.Amount;
                KuroCreditPlan.ReceivableBalance = ReceivableBalance;
                if (ReceivableBalance.Equals(0m))
                {
                    KuroCreditPlan.CompleteFlag = "1";
                }
                else
                {
                    KuroCreditPlan.CompleteFlag = "0";
                }
                KuroCreditPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                KuroCreditPlan.CreateDate = DateTime.Now;
                KuroCreditPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                KuroCreditPlan.LastUpdateDate = DateTime.Now;
                KuroCreditPlan.DelFlag = "0";
                KuroCreditPlan.Summary = CreditPlan.Summary;
                KuroCreditPlan.JournalDate = CreditPlan.JournalDate;
                KuroCreditPlan.DepositFlag = CreditPlan.DepositFlag;
                KuroCreditPlan.PaymentKindCode = CreditPlan.PaymentKindCode;
                KuroCreditPlan.CommissionRate = CreditPlan.CommissionRate;
                KuroCreditPlan.CommissionAmount = CreditPlan.CommissionAmount;
                KuroCreditPlan.CreditJournalId = CreditPlan.CreditJournalId;
                db.ReceiptPlan.InsertOnSubmit(KuroCreditPlan);

                //カード、カード会社からの入金、の実績があった場合入金予定IDを新しい入金予定IDに更新する
                List<Journal> CardJournalList = new JournalDao(db).GetByReceiptPlanID(CreditPlan.ReceiptPlanId.ToString());
                if (CardJournalList != null)
                {
                    foreach (var CardJournal in CardJournalList)
                    {
                        CardJournal.CreditReceiptPlanId = KuroCreditPlan.ReceiptPlanId.ToString().ToUpper();
                        CardJournal.LastUpdateDate = DateTime.Now;
                        CardJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                }

                //カード会社からの入金実績も振り替えるため、元伝票の「カード会社からの入金」に対する実績との紐付けを削除
                CreditPlan.CreditJournalId = "";
            }



        }
        #endregion

        //Add 2014/05/20 arc yano  vs2008→2012対応　オプション行のバインド失敗時に、直接POSTデータよりオプション行データを取得する。
        #region Requestよりオプション行のデータ取得(オプション)
        private EntitySet<CarSalesLine> getCarSalesLinebyReq(){
            EntitySet<CarSalesLine> etmpline = new EntitySet<CarSalesLine>();

            int lineCount = 0;                              //明細行数
            int i;                                          //カウンタ

            //現在のオプション行の行数を取得
            if (!string.IsNullOrEmpty(Request["LineCount"]))
            {
                lineCount = Int32.Parse(Request["LineCount"]);
            }
            else
            {
                lineCount = 0;      //0行とする。
            }

            //------------------------------------------------
            //POSTデータよりオプション行データを取得する。
            //------------------------------------------------
            for (i = 0; i<lineCount; i++)
            {

                CarSalesLine tmpline = new CarSalesLine();
                string lineprefix = string.Format("line[{0}].", i);

                if (!string.IsNullOrEmpty(Request["Slipnumber"]))                                        //伝票番号
                {
                    tmpline.SlipNumber = Request["Slipnumber"];
                }

                if (!string.IsNullOrEmpty(Request["RevisionNumber"]))                                   //改訂番号
                {
                    tmpline.RevisionNumber = Int32.Parse(Request["RevisionNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "LineNumber"]))                          //行番号
                {
                    tmpline.LineNumber = Int32.Parse(Request[lineprefix + "LineNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CarOptionCode"]))                       //車両オプションコード
                {
                    tmpline.CarOptionCode = Request[lineprefix + "CarOptionCode"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CarOptionName"]))                       //車両オプション名
                {
                    tmpline.CarOptionName = Request[lineprefix + "CarOptionName"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "OptionType"]))                          //オプション種別
                {
                    tmpline.OptionType = Request[lineprefix + "OptionType"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Amount"]))                              //販売単価
                {
                    tmpline.Amount = Decimal.Parse(Request[lineprefix + "Amount"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "TaxAmount"]))                           //消費税
                {
                    tmpline.TaxAmount = Decimal.Parse(Request[lineprefix + "TaxAmount"]);
                }

                if (!string.IsNullOrEmpty(Request["Rate"]))                                             //消費税率
                {
                    tmpline.Rate = Int32.Parse(Request["Rate"]);
                }

                etmpline.Add(tmpline);    //コレクションに行追加
            }

            return etmpline;
        }
        #endregion

 
        //Add 2014/05/20 arc yano  vs2008→2012対応　オプション行のバインド失敗時に、直接POSTデータより支払方法の行データを取得する。
        #region Requestより支払情報の行データ取得
        private EntitySet<CarSalesPayment> getCarSalesPaymentbyReq()
        {
            EntitySet<CarSalesPayment> etmpline = new EntitySet<CarSalesPayment>();
            

            int lineCount = 0;                              //明細行数
            int i;                                          //カウンタ

            //支払情報の現在の行数を取得
            if (!string.IsNullOrEmpty(Request["PayLineCount"]))
            {
                lineCount = Int32.Parse(Request["PayLineCount"]);
            }
            else
            {
                lineCount = 0;      //0行とする。
            }
            //------------------------------------------------
            //POSTデータより支払情報行データを取得する。
            //------------------------------------------------
            for (i = 0; i < lineCount; i++)
            {
                CarSalesPayment tmpline = new CarSalesPayment();
                string lineprefix = string.Format("pay[{0}].", i);

                if (!string.IsNullOrEmpty(Request["Slipnumber"]))                                        //伝票番号
                {
                    tmpline.SlipNumber = Request["Slipnumber"];
                }

                if (!string.IsNullOrEmpty(Request["RevisionNumber"]))                                   //改訂番号
                {
                    tmpline.RevisionNumber = Int32.Parse(Request["RevisionNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "LineNumber"]))                          //行番号
                {
                    tmpline.LineNumber = Int32.Parse(Request[lineprefix + "LineNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CustomerClaimCode"]))                   //請求先コード
                {
                    tmpline.CustomerClaimCode = Request[lineprefix + "CustomerClaimCode"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "PaymentPlanDate"]))                    //入金予定日
                {
                    tmpline.PaymentPlanDate = DateTime.Parse(Request[lineprefix + "PaymentPlanDate"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Amount"]))                              //入金予定金額
                {
                    tmpline.Amount = Decimal.Parse(Request[lineprefix + "Amount"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Memo"]))                                //メモ
                {
                    tmpline.Memo = Request[lineprefix + "Memo"];
                }

                etmpline.Add(tmpline);    //コレクションに行追加
            }

            return etmpline;
        }
        #endregion

        #region タブごとのエラー仕分
        private bool BasicHasErrors() {
            var query = from a in ModelState
                        where (!a.Key.StartsWith("TradeIn")
                        && !a.Key.StartsWith("pay[")
                        && !a.Key.Equals("LoanPrincipalAmount")
                        && !a.Key.EndsWith("A")
                        && !a.Key.EndsWith("B")
                        && !a.Key.EndsWith("C"))
                        && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }

        private bool UsedHasErrors() {
            var query = from a in ModelState
                        where a.Key.Contains("TradeIn") && a.Value.Errors.Count()>0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        private bool InvoiceHasErrors() {
            var query = from a in ModelState
                        where (a.Key.Contains("pay[") || a.Key.Equals("LoanPrincipalAmount"))
                        && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        private bool LoanHasErrors() {
            var query = from a in ModelState
                        where (a.Key.EndsWith("A") && a.Value.Errors.Count() > 0)
                        || (a.Key.EndsWith("B") && a.Value.Errors.Count() > 0)
                        || (a.Key.EndsWith("C") && a.Value.Errors.Count() > 0)
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;

        }
        private bool VoluntaryInsuranceHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("VoluntaryInsurance") && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        #endregion

        #region  伝票を修正中にする（レコード作成）
        /// <summary>
        /// 伝票を修正中にする（レコード作成）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public void ModificationStart(CarSalesHeader header)
        {
            ModificationControl Modification = new ModificationControl();
            Modification.SlipNumber = header.SlipNumber;
            Modification.RevisionNumber = header.RevisionNumber;
            Modification.SlipType = "0";
            Modification.SalesDate = header.SalesDate;
            Modification.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            Modification.CreateDate = DateTime.Now;
            Modification.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            Modification.LastUpdateDate = DateTime.Now;
            Modification.DelFlag = "0";
            db.ModificationControl.InsertOnSubmit(Modification);
            db.SubmitChanges();

        }
        #endregion

        #region  伝票の修正をキャンセルする（修正を行えないようにする）
        /// <summary>
        /// 伝票の修正をキャンセルする（修正を行えないようにする）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public void ModificationCancel(CarSalesHeader header)
        {
            List<ModificationControl> ModifiRet = new CarSalesOrderDao(db).GetModificationStatusAll(header.SlipNumber);
            if (ModifiRet != null)
            {
                foreach (var ModRet in ModifiRet)
                {
                    db.ModificationControl.DeleteOnSubmit(ModRet);

                }
                db.SubmitChanges();
            }
        }
        #endregion

        #region 過去に修正処理を行った伝票でないかチェックする（修正履歴あり:True  修正履歴なし:False）
        /// <summary>
        /// 過去に修正処理を行った伝票でないかチェックする（修正履歴あり:True  修正履歴なし:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴あり:True  修正履歴なし:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public bool CheckModifiedReason(string SlipNumber)
        {
            ModifiedReason ModifiedRec = new CarSalesOrderDao(db).GetLatestModifiedReason(SlipNumber);

            if (ModifiedRec != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 修正履歴を取得する（該当伝票の全履歴）
        /// <summary>
        /// 修正履歴を取得する（該当伝票の全履歴）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正履歴（修正時間・修正者・修正理由）</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public void GetModifiedHistory(CarSalesHeader header)
        {
            List<ModifiedReason> ModifiedRec = new CarSalesOrderDao(db).GetModifiedReason(header.SlipNumber);
            header.ModifiedReasonList = new List<CarSalesModifiedReason>();

            if (ModifiedRec != null)
            {
                foreach (var Mod in ModifiedRec)
                {
                    CarSalesModifiedReason ModData = new CarSalesModifiedReason();
                    ModData.ModifiedTime = Mod.CreateDate;
                    ModData.ModifiedEmployeeName = new EmployeeDao(db).GetByKey(Mod.CreateEmployeeCode).EmployeeName;
                    ModData.ModifiedReason = Mod.Reason;
                    header.ModifiedReasonList.Add(ModData);
                }
            }
        }
        #endregion

        #region  修正中かどうかのチェック（修正中:True  それ以外:False）
        /// <summary>
        /// 修正中かどうかのチェック（修正中:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>修正中:True  それ以外:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history> 
        public bool CheckModification(string SlipNumber, int RevisionNumber)
        {
            ModificationControl ModifiRet = new CarSalesOrderDao(db).GetModificationStatus(SlipNumber, RevisionNumber);

            if (ModifiRet != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region  権限のチェック
        /// <summary>
        /// 権限のチェック
        /// </summary>
        /// <param name="code"></param>
        /// <returns>伝票修正許可権限をもっていれば:True  それ以外:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public bool CheckApplicationRole(string EmployeeCode)
        {
            //ログインユーザ情報取得
            Employee loginUser = new EmployeeDao(db).GetByKey(EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "SlipModification"); //伝票修正許可権限があるかないか

            // 伝票修正許可権限があればtrueそうでなければfalse
            if (AppRole.EnableFlag)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region  過去に赤黒処理を行った元伝票でないかチェックする（赤黒経歴なし:True  それ以外:False）
        /// <summary>
        /// 過去に赤黒処理を行った元伝票でないかチェックする（赤黒経歴なし:True  それ以外:False）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>赤黒経歴なし:True  赤黒経歴あり:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history> 
        public bool AkakuroCheck(string SlipNumber)
        {
            AkakuroReason AkaKuroRec = new ServiceSalesOrderDao(db).GetAkakuroReason(SlipNumber);

            if (AkaKuroRec != null)
            {
                return false;
            }
            return true;

        }
        #endregion

        #region  修正履歴を作成する（レコード作成）
        /// <summary>
        /// 修正履歴を作成する（レコード作成）
        /// </summary>
        /// <param name="code"></param>
        /// <returns>なし</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public void CreateModifiedHistory(CarSalesHeader header)
        {
            ModifiedReason ModifiedHistory = new ModifiedReason();
            ModifiedHistory.SlipNumber = header.SlipNumber;
            ModifiedHistory.RevisionNumber = header.RevisionNumber;
            ModifiedHistory.SlipType = "0";
            ModifiedHistory.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            ModifiedHistory.CreateDate = DateTime.Now;
            ModifiedHistory.Reason = header.Reason;
            ModifiedHistory.DelFlag = "0";
            db.ModifiedReason.InsertOnSubmit(ModifiedHistory);
            db.SubmitChanges();
        }
        #endregion

        #region 伝票ロック
        /// <summary>
        /// 画面を閉じる際に呼び出される
        /// （伝票ロックを解除する）
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能　新規作成
        /// </history>
        public ActionResult UnLock(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form)
        {
            if (header.RevisionNumber > 0)
            {
                ProcessUnLock(header);
            }

            ViewData["close"] = "1";

            //SetDataComponent(ref header);
 
            //return GetViewResult(header);
            return new EmptyResult();
        }

        /// <summary>
        /// 伝票をロックする
        /// 条件１：既存の伝票であること
        /// 条件２：最新のリビジョンであること（DelFlag='0'）
        /// 条件３：既に自分がロックしていないこと
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能　新規作成
        /// </history>
        private void ProcessLock(CarSalesHeader header)
        {
            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.DelFlag.Equals("0"))
            {
                string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                target.ProcessSessionControl = new ProcessSessionControl();
                target.ProcessSessionControl.ProcessSessionId = Guid.NewGuid();
                target.ProcessSessionControl.TableName = "CarSalesHeader";
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                target.ProcessSessionControl.CreateDate = DateTime.Now;

                db.SubmitChanges();
            }
        }

        /// <summary>
        /// 伝票ロック解除
        /// 条件１：ロックしているのが自分であること
        /// 条件２：もしくは、強制解除であること
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能　新規作成
        /// </history>
        private void ProcessUnLock(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if ((target.ProcessSessionControl != null && target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode)))
            {
                ProcessSessionControl control = new ProcessSessionControlDao(db).GetByKey(target.ProcessSessionId);
                db.ProcessSessionControl.DeleteOnSubmit(control);
                target.ProcessSessionControl = null;

                db.SubmitChanges();
            }
        }
        /// <summary>
        /// 伝票がロックされているか
        /// 条件１：ProcessSessionId!=null
        /// 条件２：ProcessSessionControl!=null
        /// 条件３：自分以外がロックしている
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能　新規作成
        /// </history>
        private string GetProcessLockUser(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.ProcessSessionId != null &&
                target.ProcessSessionControl != null &&
                !target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode))
            {
                return target.ProcessSessionControl.Employee.EmployeeName;
            }

            return null;
        }

        /// <summary>
        /// ロックを自分のものにする
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 車両伝票で古い伝票で上書き防止する機能　新規作成
        /// </history>
        private void ProcessLockUpdate(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target.ProcessSessionControl != null)
            {
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                db.SubmitChanges();
            }
        }
        #endregion

        #region 引当解除処理
        /// <summary>
        /// 引当解除処理
        /// </summary>
        /// <param name="header">車両伝票ヘッダ</param>
        /// <param name="cancelPattern">解除方法</param>
        /// <param name="cause">解除要因</param>
        /// <history>
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について　解除パターンの引数を追加
        /// 2018/06/22 arc yano  #3898 車両マスタ　AA販売で納車後キャンセルとなった場合の在庫ステータスについて　新規作成
        /// </history>
        private void ReleaseProvision(CarSalesHeader header, string cancelPattern, string cause)
        {
            //-------------------------
            //車両伝票
            //-------------------------
            header.ApprovalFlag = "0";
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;

            //Mod 2018/08/07 yano #3911
            //--------------------------
            //車両発注依頼引当
            //--------------------------
            // 引当済みの車両は引当解除
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
            if (order != null && order.ReservationStatus != null && order.ReservationStatus.Equals("1"))
            {
                switch (cancelPattern)
                {
                    case "1":   //全て解除
                        order.ReservationStatus = "0";
                        order.RegistrationStatus = "0";
                        order.PurchaseOrderStatus = "0";
                        order.PurchasePlanStatus = "0";
                        break;

                    case "2":   //登録、引当を解除
                        order.ReservationStatus = "0";
                        order.RegistrationStatus = "0";
                        break;

                    default:   //引当のみ解除
                        order.ReservationStatus = "0";
                        break;                
                }

                //--------------------------
                //車両マスタ
                //--------------------------
                SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);

                //車両マスタの在庫ステータスが「引当済」「仕掛中」「納車準備」「納車済み」の場合は「在庫」に戻す
                if (salesCar != null && (salesCar.CarStatus.Equals("003") || salesCar.CarStatus.Equals("004") || salesCar.CarStatus.Equals("005") || salesCar.CarStatus.Equals("006")))
                {
                    //在庫ステータス＝「在庫」
                    salesCar.CarStatus = "001";                                                         //在庫                                   

                    //ロケーションが空欄の場合
                    if (string.IsNullOrWhiteSpace(salesCar.LocationCode))
                    {
                        Location loc = new LocationDao(db).GetByKey(header.DepartmentCode);

                        if (loc != null)
                        {
                            salesCar.LocationCode = loc.LocationCode;
                        }
                        else
                        {
                            //車両仕入データを取得して、入庫ロケーションを設定する
                            CarPurchase carpurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);

                            if (carpurchase != null)
                            {
                                salesCar.LocationCode = carpurchase.PurchaseLocationCode;
                            }
                            else
                            {
                                salesCar.LocationCode = "";
                            }
                        }
                    }
                    
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    salesCar.LastUpdateDate = DateTime.Now;
                }

                order.SalesCarNumber = string.Empty;
                order.Vin = string.Empty;
                order.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.LastUpdateDate = DateTime.Now;
            }

            //Add 2018/08/07 yano #3911
            //AA販売等の引当解除以外の場合はメールを送信
            if(!cancelPattern.Equals(PTN_CANCEL_ALL))
            {
                CommonUtils.SendCancelReservationMail(db, header, cancelPattern, cause);
            }
            
        }

        #endregion

        #region AA相殺の入金実績の削除
        /// <summary>
        /// AA相殺の入金実績の削除
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/06/22 arc yano  #3898 車両マスタ　AA販売で納車後キャンセルとなった場合の在庫ステータスについて　新規作成
        /// </history>
        private void DeleteAAJournal(CarSalesHeader header)
        {
            CustomerClaim cc = new CustomerClaimDao(db).GetByKey(header.CustomerCode);

            if (cc != null && cc.CustomerClaimType.Equals("201"))
            {
                List<Journal> AADelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("006"))).ToList();

                foreach (var aa in AADelJournal)
                {
                    aa.DelFlag = "1";
                    aa.LastUpdateDate = DateTime.Now;
                    aa.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                db.SubmitChanges();
            }
        }

        #endregion

        #region 対象伝票のローン会社からの入金が入金済みかどうかを返す(Ajax専用）
        /// <summary>
        /// 対象伝票のローン会社からの入金が入金済みかどうかを返す(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>入金済：True 未入金：False</returns>
        /// <history>
        /// 2022/08/30 yano #4150【車両伝票入力】入金消込済のローン元金が更新される不具合の対応 ローン入金消込の判断の変更
        /// 2016/09/08 arc nakayama #3595_【大項目】車両売掛金機能改善 ローン会社から入金が既にされていたらローン元金に振り替えないようにする
        /// </history>
        public ActionResult LoanCompleteCheck(string SlipNumber, string LoanCode)
        {
            if (Request.IsAjaxRequest())
            {

                bool ret = false;       //未入金

                if (!string.IsNullOrEmpty(LoanCode))
                {
                    //ローンコードは、数字＋アルファベット１文字　の構成になっているため末尾のアルファベットを削除する

                    //string MasterLoanCode = LoanCode.Remove(leng, 1); //末尾のアルファベット削除
                    Loan LoanDataPlan = new LoanDao(db).GetByKey(LoanCode, false);

                    //該当伝票の入金予定を取得する
                    ReceiptPlan LoanData = new ReceiptPlanDao(db).GetByCustomerClaim(SlipNumber, LoanDataPlan.CustomerClaimCode);

                    if (LoanData != null)
                    {
                        if (LoanData.CompleteFlag.Equals("1"))
                        {
                            ret = true; //入金完了フラグが立っていたらTrue
                        }
                    }
                    //Mod 2022/08/30 yano #4150
                    else
                    {
                        //該当伝票の入金予定が存在しない場合、入金実績を確認
                        List<Journal> LoanList = new JournalDao(db).GetListByCustomerAndSlip(SlipNumber, LoanDataPlan.CustomerClaimCode).Where(x => x.AccountType.Equals(ACCOUNTTYPE_LOAN)).ToList();
                        
                        if(LoanList != null && LoanList.Count > 0) 
                        {
                            ret = true; //実績ありならTrue
                        }
                    }
                }
                return Json(ret);
            }
            return new EmptyResult();
        }
        #endregion

        #region
         /// <summary>
        /// 伝票コピー
        /// </summary>
        /// <param name="sourceHeader">コピー元</param>
        /// <param name="destHeader">コピー先</param>
        /// <returns>コピー先</returns>
        /// <history>
        /// 2018/11/07 yano #3939 車両伝票納車前（納車確認書出力後）の注文書出力時に改訂番号更新不可とする
        /// </history>
        private CarSalesHeader CopyCarSalesHeader(CarSalesHeader sourceHeader, CarSalesHeader destHeader) 
        {
			destHeader.SlipNumber = sourceHeader.SlipNumber;
            destHeader.RevisionNumber = sourceHeader.RevisionNumber;
			destHeader.QuoteDate = sourceHeader.QuoteDate;
			destHeader.QuoteExpireDate = sourceHeader.QuoteExpireDate;
			destHeader.SalesOrderDate = sourceHeader.SalesOrderDate;
			destHeader.SalesOrderStatus = sourceHeader.SalesOrderStatus;
			destHeader.ApprovalFlag = sourceHeader.ApprovalFlag;
			destHeader.SalesDate = DateTime.Today;
			destHeader.CustomerCode = sourceHeader.CustomerCode;
			destHeader.DepartmentCode = sourceHeader.DepartmentCode;
			destHeader.EmployeeCode = sourceHeader.EmployeeCode;
			destHeader.CampaignCode1 = sourceHeader.CampaignCode1;
			destHeader.CampaignCode2 = sourceHeader.CampaignCode2;
			destHeader.NewUsedType = sourceHeader.NewUsedType;
			destHeader.SalesType = sourceHeader.SalesType;
			destHeader.MakerName = sourceHeader.MakerName;
			destHeader.CarBrandName = sourceHeader.CarBrandName;
			destHeader.CarName = sourceHeader.CarName;
			destHeader.CarGradeName = sourceHeader.CarGradeName;
			destHeader.CarGradeCode = sourceHeader.CarGradeCode;
			destHeader.ManufacturingYear = sourceHeader.ManufacturingYear;
			destHeader.ExteriorColorCode = sourceHeader.ExteriorColorCode;
			destHeader.ExteriorColorName = sourceHeader.ExteriorColorName;
            destHeader.InteriorColorCode = sourceHeader.InteriorColorCode;
            destHeader.InteriorColorName = sourceHeader.InteriorColorName;
			destHeader.Vin = sourceHeader.Vin;
			destHeader.UsVin = sourceHeader.UsVin;
			destHeader.ModelName = sourceHeader.ModelName;
			destHeader.Mileage = sourceHeader.Mileage;
			destHeader.MileageUnit = sourceHeader.MileageUnit;
			destHeader.RequestPlateNumber = sourceHeader.RequestPlateNumber;
			destHeader.RegistPlanDate = sourceHeader.RegistPlanDate;
			destHeader.HotStatus = sourceHeader.HotStatus;
			destHeader.SalesCarNumber = sourceHeader.SalesCarNumber;
			destHeader.RequestRegistDate = sourceHeader.RequestRegistDate;
			destHeader.SalesPlanDate = sourceHeader.SalesPlanDate;
			destHeader.RegistrationType = sourceHeader.RegistrationType;
			destHeader.MorterViecleOfficialCode = sourceHeader.MorterViecleOfficialCode;
			destHeader.OwnershipReservation = sourceHeader.OwnershipReservation;
			destHeader.CarLiabilityInsuranceType = sourceHeader.CarLiabilityInsuranceType;
			destHeader.SealSubmitDate = sourceHeader.SealSubmitDate;
			destHeader.ProxySubmitDate = sourceHeader.ProxySubmitDate;
			destHeader.ParkingSpaceSubmitDate = sourceHeader.ParkingSpaceSubmitDate;
			destHeader.CarLiabilityInsuranceSubmitDate = sourceHeader.CarLiabilityInsuranceSubmitDate;
			destHeader.OwnershipReservationSubmitDate = sourceHeader.OwnershipReservationSubmitDate;
			destHeader.Memo = sourceHeader.Memo;
			destHeader.SalesPrice = sourceHeader.SalesPrice;
			destHeader.DiscountAmount = sourceHeader.DiscountAmount;
			destHeader.TaxationAmount = sourceHeader.TaxationAmount;
			destHeader.TaxAmount = sourceHeader.TaxAmount;
			destHeader.ShopOptionAmount = sourceHeader.ShopOptionAmount;
			destHeader.ShopOptionTaxAmount = sourceHeader.ShopOptionTaxAmount;
			destHeader.MakerOptionAmount = sourceHeader.MakerOptionAmount;
			destHeader.MakerOptionTaxAmount = sourceHeader.MakerOptionTaxAmount;
			destHeader.OutSourceAmount = sourceHeader.OutSourceAmount;
			destHeader.OutSourceTaxAmount = sourceHeader.OutSourceTaxAmount;
			destHeader.SubTotalAmount = sourceHeader.SubTotalAmount;
			destHeader.CarTax = sourceHeader.CarTax;
			destHeader.CarLiabilityInsurance = sourceHeader.CarLiabilityInsurance;
			destHeader.CarWeightTax = sourceHeader.CarWeightTax;
			destHeader.AcquisitionTax = sourceHeader.AcquisitionTax;
			destHeader.InspectionRegistCost = sourceHeader.InspectionRegistCost;
			destHeader.ParkingSpaceCost = sourceHeader.ParkingSpaceCost;
			destHeader.TradeInCost = sourceHeader.TradeInCost;
			destHeader.RecycleDeposit = sourceHeader.RecycleDeposit;
			destHeader.RecycleDepositTradeIn = sourceHeader.RecycleDepositTradeIn;
			destHeader.NumberPlateCost = sourceHeader.NumberPlateCost;
			destHeader.RequestNumberCost = sourceHeader.RequestNumberCost;
			destHeader.TradeInFiscalStampCost = sourceHeader.TradeInFiscalStampCost;
			destHeader.TaxFreeFieldName = sourceHeader.TaxFreeFieldName;
			destHeader.TaxFreeFieldValue = sourceHeader.TaxFreeFieldValue;
			destHeader.TaxFreeTotalAmount = sourceHeader.TaxFreeTotalAmount;
			destHeader.InspectionRegistFee = sourceHeader.InspectionRegistFee;
			destHeader.ParkingSpaceFee = sourceHeader.ParkingSpaceFee;
			destHeader.TradeInFee = sourceHeader.TradeInFee;
			destHeader.PreparationFee = sourceHeader.PreparationFee;
			destHeader.RecycleControlFee = sourceHeader.RecycleControlFee;
			destHeader.RecycleControlFeeTradeIn = sourceHeader.RecycleControlFeeTradeIn;
			destHeader.RequestNumberFee = sourceHeader.RequestNumberFee;
			destHeader.CarTaxUnexpiredAmount = sourceHeader.CarTaxUnexpiredAmount;
			destHeader.CarLiabilityInsuranceUnexpiredAmount = sourceHeader.CarLiabilityInsuranceUnexpiredAmount;
			destHeader.TradeInAppraisalFee = sourceHeader.TradeInAppraisalFee;
			destHeader.FarRegistFee = sourceHeader.FarRegistFee;
			destHeader.TradeInMaintenanceFee = sourceHeader.TradeInMaintenanceFee;
			destHeader.InheritedInsuranceFee = sourceHeader.InheritedInsuranceFee;
			destHeader.TaxationFieldName = sourceHeader.TaxationFieldName;
			destHeader.TaxationFieldValue = sourceHeader.TaxationFieldValue;
			destHeader.SalesCostTotalAmount = sourceHeader.SalesCostTotalAmount;
			destHeader.SalesCostTotalTaxAmount = sourceHeader.SalesCostTotalTaxAmount;
			destHeader.OtherCostTotalAmount = sourceHeader.OtherCostTotalAmount;
			destHeader.CostTotalAmount = sourceHeader.CostTotalAmount;
			destHeader.TotalTaxAmount = sourceHeader.TotalTaxAmount;
			destHeader.GrandTotalAmount = sourceHeader.GrandTotalAmount;
			destHeader.PossesorCode = sourceHeader.PossesorCode;
			destHeader.UserCode = sourceHeader.UserCode;
			destHeader.PrincipalPlace = sourceHeader.PrincipalPlace;
			destHeader.VoluntaryInsuranceType = sourceHeader.VoluntaryInsuranceType;
			destHeader.VoluntaryInsuranceCompanyName = sourceHeader.VoluntaryInsuranceCompanyName;
			destHeader.VoluntaryInsuranceAmount = sourceHeader.VoluntaryInsuranceAmount;
			destHeader.VoluntaryInsuranceTermFrom = sourceHeader.VoluntaryInsuranceTermFrom;
			destHeader.VoluntaryInsuranceTermTo = sourceHeader.VoluntaryInsuranceTermTo;
			destHeader.PaymentPlanType = sourceHeader.PaymentPlanType;
			destHeader.TradeInAmount1 = sourceHeader.TradeInAmount1;
			destHeader.TradeInTax1 = sourceHeader.TradeInTax1;
			destHeader.TradeInUnexpiredCarTax1 = sourceHeader.TradeInUnexpiredCarTax1;
			destHeader.TradeInRemainDebt1 = sourceHeader.TradeInRemainDebt1;
			destHeader.TradeInAppropriation1 = sourceHeader.TradeInAppropriation1;
			destHeader.TradeInRecycleAmount1 = sourceHeader.TradeInRecycleAmount1;
			destHeader.TradeInMakerName1 = sourceHeader.TradeInMakerName1;
			destHeader.TradeInCarName1 = sourceHeader.TradeInCarName1;
			destHeader.TradeInClassificationTypeNumber1 = sourceHeader.TradeInClassificationTypeNumber1;
			destHeader.TradeInModelSpecificateNumber1 = sourceHeader.TradeInModelSpecificateNumber1;
			destHeader.TradeInManufacturingYear1 = sourceHeader.TradeInManufacturingYear1;
			destHeader.TradeInInspectionExpiredDate1 = sourceHeader.TradeInInspectionExpiredDate1;
			destHeader.TradeInMileage1 = sourceHeader.TradeInMileage1;
			destHeader.TradeInMileageUnit1 = sourceHeader.TradeInMileageUnit1;
			destHeader.TradeInVin1 = sourceHeader.TradeInVin1;
			destHeader.TradeInRegistrationNumber1 = sourceHeader.TradeInRegistrationNumber1;
			destHeader.TradeInUnexpiredLiabilityInsurance1 = sourceHeader.TradeInUnexpiredLiabilityInsurance1;
			destHeader.TradeInAmount2 = sourceHeader.TradeInAmount2;
			destHeader.TradeInTax2 = sourceHeader.TradeInTax2;
			destHeader.TradeInUnexpiredCarTax2 = sourceHeader.TradeInUnexpiredCarTax2;
			destHeader.TradeInRemainDebt2 = sourceHeader.TradeInRemainDebt2;
			destHeader.TradeInAppropriation2 = sourceHeader.TradeInAppropriation2;
			destHeader.TradeInRecycleAmount2 = sourceHeader.TradeInRecycleAmount2;
			destHeader.TradeInMakerName2 = sourceHeader.TradeInMakerName2;
			destHeader.TradeInCarName2 = sourceHeader.TradeInCarName2;
			destHeader.TradeInClassificationTypeNumber2 = sourceHeader.TradeInClassificationTypeNumber2;
			destHeader.TradeInModelSpecificateNumber2 = sourceHeader.TradeInModelSpecificateNumber2;
			destHeader.TradeInManufacturingYear2 = sourceHeader.TradeInManufacturingYear2;
			destHeader.TradeInInspectionExpiredDate2 = sourceHeader.TradeInInspectionExpiredDate2;
			destHeader.TradeInMileage2 = sourceHeader.TradeInMileage2;
			destHeader.TradeInMileageUnit2 = sourceHeader.TradeInMileageUnit2;
			destHeader.TradeInVin2 = sourceHeader.TradeInVin2;
			destHeader.TradeInRegistrationNumber2 = sourceHeader.TradeInRegistrationNumber2;
			destHeader.TradeInUnexpiredLiabilityInsurance2 = sourceHeader.TradeInUnexpiredLiabilityInsurance2;
			destHeader.TradeInAmount3 = sourceHeader.TradeInAmount3;
			destHeader.TradeInTax3 = sourceHeader.TradeInTax3;
			destHeader.TradeInUnexpiredCarTax3 = sourceHeader.TradeInUnexpiredCarTax3;
			destHeader.TradeInRemainDebt3 = sourceHeader.TradeInRemainDebt3;
			destHeader.TradeInAppropriation3 = sourceHeader.TradeInAppropriation3;
			destHeader.TradeInRecycleAmount3 = sourceHeader.TradeInRecycleAmount3;
			destHeader.TradeInMakerName3 = sourceHeader.TradeInMakerName3;
			destHeader.TradeInCarName3 = sourceHeader.TradeInCarName3;
			destHeader.TradeInClassificationTypeNumber3 = sourceHeader.TradeInClassificationTypeNumber3;
			destHeader.TradeInModelSpecificateNumber3 = sourceHeader.TradeInModelSpecificateNumber3;
			destHeader.TradeInManufacturingYear3 = sourceHeader.TradeInManufacturingYear3;
			destHeader.TradeInInspectionExpiredDate3 = sourceHeader.TradeInInspectionExpiredDate3;
			destHeader.TradeInMileage3 = sourceHeader.TradeInMileage3;
			destHeader.TradeInMileageUnit3 = sourceHeader.TradeInMileageUnit3;
			destHeader.TradeInVin3 = sourceHeader.TradeInVin3;
			destHeader.TradeInRegistrationNumber3 = sourceHeader.TradeInRegistrationNumber3;
			destHeader.TradeInUnexpiredLiabilityInsurance3 = sourceHeader.TradeInUnexpiredLiabilityInsurance3;
			destHeader.TradeInTotalAmount = sourceHeader.TradeInTotalAmount;
			destHeader.TradeInTaxTotalAmount = sourceHeader.TradeInTaxTotalAmount;
			destHeader.TradeInUnexpiredCarTaxTotalAmount = sourceHeader.TradeInUnexpiredCarTaxTotalAmount;
			destHeader.TradeInRemainDebtTotalAmount = sourceHeader.TradeInRemainDebtTotalAmount;
			destHeader.TradeInAppropriationTotalAmount = sourceHeader.TradeInAppropriationTotalAmount;
			destHeader.PaymentTotalAmount = sourceHeader.PaymentTotalAmount;
			destHeader.PaymentCashTotalAmount = sourceHeader.PaymentCashTotalAmount;
			destHeader.LoanPrincipalAmount = sourceHeader.LoanPrincipalAmount;
			destHeader.LoanFeeAmount = sourceHeader.LoanFeeAmount;
			destHeader.LoanTotalAmount = sourceHeader.LoanTotalAmount;
			destHeader.LoanCodeA = sourceHeader.LoanCodeA;
			destHeader.PaymentFrequencyA = sourceHeader.PaymentFrequencyA;
			destHeader.PaymentTermFromA = sourceHeader.PaymentTermFromA;
			destHeader.PaymentTermToA = sourceHeader.PaymentTermToA;
			destHeader.BonusMonthA1 = sourceHeader.BonusMonthA1;
			destHeader.BonusMonthA2 = sourceHeader.BonusMonthA2;
			destHeader.FirstAmountA = sourceHeader.FirstAmountA;
			destHeader.SecondAmountA = sourceHeader.SecondAmountA;
			destHeader.BonusAmountA = sourceHeader.BonusAmountA;
			destHeader.CashAmountA = sourceHeader.CashAmountA;
			destHeader.LoanPrincipalA = sourceHeader.LoanPrincipalA;
			destHeader.LoanFeeA = sourceHeader.LoanFeeA;
			destHeader.LoanTotalAmountA = sourceHeader.LoanTotalAmountA;
			destHeader.AuthorizationNumberA = sourceHeader.AuthorizationNumberA;
			destHeader.FirstDirectDebitDateA = sourceHeader.FirstDirectDebitDateA;
			destHeader.SecondDirectDebitDateA = sourceHeader.SecondDirectDebitDateA;
			destHeader.LoanCodeB = sourceHeader.LoanCodeB;
			destHeader.PaymentFrequencyB = sourceHeader.PaymentFrequencyB;
			destHeader.PaymentTermFromB = sourceHeader.PaymentTermFromB;
			destHeader.PaymentTermToB = sourceHeader.PaymentTermToB;
			destHeader.BonusMonthB1 = sourceHeader.BonusMonthB1;
			destHeader.BonusMonthB2 = sourceHeader.BonusMonthB2;
			destHeader.FirstAmountB = sourceHeader.FirstAmountB;
			destHeader.SecondAmountB = sourceHeader.SecondAmountB;
			destHeader.BonusAmountB = sourceHeader.BonusAmountB;
			destHeader.CashAmountB = sourceHeader.CashAmountB;
			destHeader.LoanPrincipalB = sourceHeader.LoanPrincipalB;
			destHeader.LoanFeeB = sourceHeader.LoanFeeB;
			destHeader.LoanTotalAmountB = sourceHeader.LoanTotalAmountB;
			destHeader.AuthorizationNumberB = sourceHeader.AuthorizationNumberB;
			destHeader.FirstDirectDebitDateB = sourceHeader.FirstDirectDebitDateB;
			destHeader.SecondDirectDebitDateB = sourceHeader.SecondDirectDebitDateB;
			destHeader.LoanCodeC = sourceHeader.LoanCodeC;
			destHeader.PaymentFrequencyC = sourceHeader.PaymentFrequencyC;
			destHeader.PaymentTermFromC = sourceHeader.PaymentTermFromC;
			destHeader.PaymentTermToC = sourceHeader.PaymentTermToC;
			destHeader.BonusMonthC1 = sourceHeader.BonusMonthC1;
			destHeader.BonusMonthC2 = sourceHeader.BonusMonthC2;
			destHeader.FirstAmountC = sourceHeader.FirstAmountC;
			destHeader.SecondAmountC = sourceHeader.SecondAmountC;
			destHeader.BonusAmountC = sourceHeader.BonusAmountC;
			destHeader.CashAmountC = sourceHeader.CashAmountC;
			destHeader.LoanPrincipalC = sourceHeader.LoanPrincipalC;
			destHeader.LoanFeeC = sourceHeader.LoanFeeC;
			destHeader.LoanTotalAmountC = sourceHeader.LoanTotalAmountC;
			destHeader.AuthorizationNumberC = sourceHeader.AuthorizationNumberC;
			destHeader.FirstDirectDebitDateC = sourceHeader.FirstDirectDebitDateC;
			destHeader.SecondDirectDebitDateC = sourceHeader.SecondDirectDebitDateC;
			destHeader.CancelDate = sourceHeader.CancelDate;
			destHeader.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
			destHeader.CreateDate = DateTime.Now;
			destHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
			destHeader.LastUpdateDate = DateTime.Now;
			destHeader.DelFlag = "0";
			destHeader.InspectionRegistFeeTax = sourceHeader.InspectionRegistFeeTax;
			destHeader.ParkingSpaceFeeTax = sourceHeader.ParkingSpaceFeeTax;
			destHeader.TradeInFeeTax = sourceHeader.TradeInFeeTax;
			destHeader.PreparationFeeTax = sourceHeader.PreparationFeeTax;
			destHeader.RecycleControlFeeTax = sourceHeader.RecycleControlFeeTax;
			destHeader.RecycleControlFeeTradeInTax = sourceHeader.RecycleControlFeeTradeInTax;
			destHeader.RequestNumberFeeTax = sourceHeader.RequestNumberFeeTax;
			destHeader.CarTaxUnexpiredAmountTax = sourceHeader.CarTaxUnexpiredAmountTax;
			destHeader.CarLiabilityInsuranceUnexpiredAmountTax = sourceHeader.CarLiabilityInsuranceUnexpiredAmountTax;
			destHeader.TradeInAppraisalFeeTax = sourceHeader.TradeInAppraisalFeeTax;
			destHeader.FarRegistFeeTax = sourceHeader.FarRegistFeeTax;
			destHeader.TradeInMaintenanceFeeTax = sourceHeader.TradeInMaintenanceFeeTax;
			destHeader.InheritedInsuranceFeeTax = sourceHeader.InheritedInsuranceFeeTax;
			destHeader.TaxationFieldValueTax = sourceHeader.TaxationFieldValueTax;
			destHeader.TradeInEraseRegist1 = sourceHeader.TradeInEraseRegist1;
			destHeader.TradeInEraseRegist2 = sourceHeader.TradeInEraseRegist2;
			destHeader.TradeInEraseRegist3 = sourceHeader.TradeInEraseRegist3;
			destHeader.RemainAmountA = sourceHeader.RemainAmountA;
			destHeader.RemainAmountB = sourceHeader.RemainAmountB;
			destHeader.RemainAmountC = sourceHeader.RemainAmountC;
			destHeader.RemainFinalMonthA = sourceHeader.RemainFinalMonthA;
			destHeader.RemainFinalMonthB = sourceHeader.RemainFinalMonthB;
			destHeader.RemainFinalMonthC = sourceHeader.RemainFinalMonthC;
			destHeader.LoanRateA = sourceHeader.LoanRateA;
			destHeader.LoanRateB = sourceHeader.LoanRateB;
			destHeader.LoanRateC = sourceHeader.LoanRateC;
			destHeader.SalesTax = sourceHeader.SalesTax;
			destHeader.DiscountTax = sourceHeader.DiscountTax;
			destHeader.TradeInPrice1 = sourceHeader.TradeInPrice1;
			destHeader.TradeInPrice2 = sourceHeader.TradeInPrice2;
			destHeader.TradeInPrice3 = sourceHeader.TradeInPrice3;
			destHeader.TradeInRecycleTotalAmount = sourceHeader.TradeInRecycleTotalAmount;
            destHeader.ConsumptionTaxId = sourceHeader.ConsumptionTaxId;
            destHeader.Rate = sourceHeader.Rate;
            destHeader.RevenueStampCost = sourceHeader.RevenueStampCost;
            destHeader.TradeInCarTaxDeposit = sourceHeader.TradeInCarTaxDeposit;
            destHeader.LastEditScreen = sourceHeader.LastEditScreen;
            destHeader.PaymentSecondFrequencyA = sourceHeader.PaymentSecondFrequencyA;
            destHeader.PaymentSecondFrequencyB = sourceHeader.PaymentSecondFrequencyB;
            destHeader.PaymentSecondFrequencyC = sourceHeader.PaymentSecondFrequencyC;
            destHeader.ProcessSessionControl = new ProcessSessionControlDao(db).GetByKey(sourceHeader.ProcessSessionId);;
            destHeader.ProcessSessionId = sourceHeader.ProcessSessionId;
            //destHeader.ProcessSessionId = sourceHeader.ProcessSessionId;
			
            return destHeader;
        }
    #endregion

      #region 適格請求書用の消費税計算、登録処理
        /// <summary>
        /// 適格請求書(インボイス)用の消費税・差額の登録
        /// </summary>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <param name="lines">サービス伝票明細</param>
        /// <param name="paymentList">サービス伝票支払</param>
        /// <returns>支払予定</returns>
        /// <history>
        /// 2023/09/28 yano #4183 インボイス対応(経理対応)
        /// 2023/09/05 yano #4162 インボイス対応 新規作成
        /// </history>
        private void InsertInvoiceConsumptionTax(CarSalesHeader header)
        {
            //登録リスト
            List<InvoiceConsumptionTax> registList = new List<InvoiceConsumptionTax>();

            //-----------------------
            //古いリストの削除
            //-----------------------
            List<InvoiceConsumptionTax> delList = new List<InvoiceConsumptionTax>();

            delList = new InvoiceConsumptionTaxDao(db).GetBySlipNumber(header.SlipNumber);

            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }

            //課税合計金額算出【現金販売合計(税込) - 税金等　- その他非課税】
            decimal amountwithTax = (header.GrandTotalAmount ?? 0m) - (header.TaxFreeTotalAmount ?? 0m) - (header.OtherCostTotalAmount ?? 0m);
           
            decimal amount = 0m;
            decimal taxmount= 0m;

            int taxrate = header.Rate ?? 0;

            //税込金額から税抜金額を計算
            amount = CommonUtils.CalcAmountWithoutTax(amountwithTax, taxrate, 0);

            //消費税計算
            taxmount = amountwithTax - amount;

            InvoiceConsumptionTax  el = new InvoiceConsumptionTax();
                
            el.InvoiceConsumptionTaxId = Guid.NewGuid();                                            //ユニークID
            el.SlipNumber = header.SlipNumber;                                                      //伝票番号
            el.CustomerClaimCode = header.CustomerCode;                                             //請求先コード
            el.Rate = header.Rate ?? 0;                                                             //消費税率
            el.InvoiceConsumptionTaxAmount = taxmount;                                              //インボイス消費税
            el.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;                  //作成者
            el.CreateDate = DateTime.Now;                                                           //作成日
            el.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;              //最終更新者
            el.LastUpdateDate = DateTime.Now;                                                       //最終更新日
            el.DelFlag = "0";                                                                       //削除フラグ

            registList.Add(el);

            db.InvoiceConsumptionTax.InsertAllOnSubmit(registList);

            header.SuspendTaxRecv = taxmount - (header.TotalTaxAmount ?? 0m);                       //消費税差額(インボイス消費税－内部保持消費税) //Add 2023/09/28 yano #4183

        }
    #endregion

  }
}
