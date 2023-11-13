using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// サービス受付機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceReceiptionController : Controller {

        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "サービス受付";     // 画面名
        private static readonly string PROC_NAME = "サービス受付登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 車両マスタ追加処理制御情報
        /// </summary>
        private bool addSalesCar = false;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceReceiptionController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// サービス受付検索画面表示
        /// </summary>
        /// <returns>サービス受付検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// サービス受付検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービス受付検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            PaginatedList<V_ServiceReceiptTarget> list;
            if (criteriaInit) {
                list = new PaginatedList<V_ServiceReceiptTarget>();
            } else {
                // 検索結果リストの取得
                list = GetSearchResultList(form);
            }
            // その他出力項目の設定
            ViewData["CustomerNameKana"]         = form["CustomerNameKana"];
            ViewData["CustomerName"]             = form["CustomerName"];
            ViewData["TelNumber"]                = form["TelNumber"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"]   = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"]   = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"]  = form["RegistrationNumberPlate"];
            ViewData["Vin"]                      = form["Vin"];
            ViewData["ModelName"]                = form["ModelName"];
            ViewData["FirstReceiptionDateFrom"]  = form["FirstReceiptionDateFrom"];
            ViewData["FirstReceiptionDateTo"]    = form["FirstReceiptionDateTo"];
            ViewData["LastReceiptionDateFrom"]   = form["LastReceiptionDateFrom"];
            ViewData["LastReceiptionDateTo"]     = form["LastReceiptionDateTo"];

            // サービス受付検索画面の表示
            return View("ServiceReceiptionCriteria", list);
        }

        /// <summary>
        /// サービス受付入力画面表示
        /// </summary>
        /// <param name="id">サービス受付対象情報(顧客コード＋","＋管理番号)</param>
        /// <returns>サービス受付入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            // モデルデータの生成
            string[] idArr = CommonUtils.DefaultString(id, ",").Split(new string[] { "," }, StringSplitOptions.None);
            CustomerReceiption customerReceiption = new CustomerReceiption();
            customerReceiption.CustomerCode = idArr[0];
            customerReceiption.SalesCarNumber = idArr[1];
            customerReceiption.ReceiptionDate = DateTime.Now.Date;
            customerReceiption.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            customerReceiption.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.ArrivalPlanDate = DateTime.Now.Date;

            // その他表示データの取得
            GetEntryViewData(customerReceiption);

            // 出口
            return View("ServiceReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// サービス受付登録
        /// </summary>
        /// <param name="customerReceiption">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>サービス受付マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerReceiption customerReceiption, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/07 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            // 処理制御情報の取得
            if (!string.IsNullOrEmpty(customerReceiption.CustomerCode) && !string.IsNullOrEmpty(customerReceiption.Vin)) {
                List<SalesCar> salesCarList = new SalesCarDao(db).GetListByReceiption(customerReceiption.CustomerCode, customerReceiption.Vin);
                if (salesCarList.Count == 0) {
                    customerReceiption.SalesCarNumber = string.Empty; //管理番号のクリア
                    addSalesCar = true;
                }
            }

            // データチェック
            ValidateServiceReceiption(customerReceiption);
            if (!ModelState.IsValid) {
                GetEntryViewData(customerReceiption);
                return View("ServiceReceiptionEntry", customerReceiption);
            }

            // 各種データ登録処理
            using (TransactionScope ts = new TransactionScope()) {

                // 車両マスタ登録処理
                if (addSalesCar) {
                    ValidateForInsert(customerReceiption);
                    if (!ModelState.IsValid) {
                        GetEntryViewData(customerReceiption);
                        return View("ServiceReceiptionEntry", customerReceiption);
                    }
                    customerReceiption.SalesCar = EditSalesCarForInsert(customerReceiption);
                    db.SalesCar.InsertOnSubmit(customerReceiption.SalesCar);
                }

                // サービス受付データ登録処理
                customerReceiption = EditCustomerReceiptionForInsert(customerReceiption);
                db.CustomerReceiption.InsertOnSubmit(customerReceiption);

                // 顧客マスタ来店日更新処理
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    EditCustomerForUpdate(customer);
                }

                //車両マスタ走行距離更新処理
                if (!addSalesCar && !string.IsNullOrEmpty(customerReceiption.SalesCarNumber)) {
                    SalesCar originalSalesCar = new SalesCarDao(db).GetByKey(customerReceiption.SalesCarNumber);
                    if (originalSalesCar != null) {
                        if (
                            ((string.IsNullOrEmpty(originalSalesCar.Vin) && string.IsNullOrEmpty(customerReceiption.Vin)) || originalSalesCar.Vin.Equals(customerReceiption.Vin))
                            && ((string.IsNullOrEmpty(originalSalesCar.MorterViecleOfficialCode) && string.IsNullOrEmpty(customerReceiption.MorterViecleOfficialCode)) || originalSalesCar.MorterViecleOfficialCode.Equals(customerReceiption.MorterViecleOfficialCode))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberKana) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberKana)) || originalSalesCar.RegistrationNumberKana.Equals(customerReceiption.RegistrationNumberKana))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberPlate) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberPlate)) || originalSalesCar.RegistrationNumberPlate.Equals(customerReceiption.RegistrationNumberPlate))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberType) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberType)) || originalSalesCar.RegistrationNumberType.Equals(customerReceiption.RegistrationNumberType))
                            && ((string.IsNullOrEmpty(originalSalesCar.CarGradeCode) && string.IsNullOrEmpty(customerReceiption.CarGradeCode)) || originalSalesCar.CarGradeCode.Equals(customerReceiption.CarGradeCode))
                            ) {
                            //車台番号、陸運局コード、登録番号（種別、かな、プレート）、グレードが変更されていない場合
                            //車両マスタの走行距離を受付時の入力値で更新
                            originalSalesCar.Mileage = customerReceiption.Mileage;
                        }
                    }
                }
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // DBアクセスの実行
                        db.SubmitChanges();
                        // コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        // Add 2014/08/07 arc amii エラーログ対応 ChangeConflictException発生時の処理を追加
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // Mod 2014/08/07 arc amii エラーログ対応 SqlException発生時、ログ出力を行う処理を追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveQuote") ? "見積作成" : "保存")));
                            GetEntryViewData(customerReceiption);
                            return View("ServiceReceiptionEntry", customerReceiption);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/04 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                

                
            }
            if (form["action"].Equals("saveQuote")) {
                ViewData["url"] = "/ServiceSalesOrder/Entry/?customerCode="+customerReceiption.CustomerCode+"&employeeCode="+customerReceiption.EmployeeCode+"&salesCarNumber="+customerReceiption.SalesCarNumber+"&requestDetail="+HttpUtility.UrlEncode(customerReceiption.RequestDetail)+"&arrivalPlanDate="+HttpUtility.UrlEncode(string.Format("{0:yyyy/MM/dd}",customerReceiption.ArrivalPlanDate));
            }

            // 画面コンポーネントのセット
            GetEntryViewData(customerReceiption);
            
            // 帳票印刷
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            } else {
                ViewData["close"] = "1";
            }
            return View("ServiceReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="customerReceiption">モデルデータ</param>
        private void GetEntryViewData(CustomerReceiption customerReceiption) {

            // 顧客情報の取得
            if (!string.IsNullOrEmpty(customerReceiption.CustomerCode)) {
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    try { ViewData["CustomerRankName"] = customer.c_CustomerRank.Name; } catch (NullReferenceException) { }
                    ViewData["CustomerName"] = customer.CustomerName;
                    ViewData["CustomerNameKana"] = customer.CustomerNameKana;
                    ViewData["Prefecture"] = customer.Prefecture;
                    ViewData["City"] = customer.City;
                    ViewData["Address1"] = customer.Address1;
                    ViewData["Address2"] = customer.Address2;
                }
            }

            // 部門名の取得
            if (!string.IsNullOrEmpty(customerReceiption.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(customerReceiption.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // 担当者名の取得
            if (!string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(customerReceiption.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    ViewData["EmployeeNumber"] = employee.EmployeeNumber;
                }
            }

            // イベント名1,2の取得
            CampaignDao campaignDao = new CampaignDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode1)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode1);
                if (campaign != null) {
                    ViewData["CampaignName1"] = campaign.CampaignName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode2)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode2);
                if (campaign != null) {
                    ViewData["CampaignName2"] = campaign.CampaignName;
                }
            }

            // 車両情報の取得
            if (!string.IsNullOrEmpty(customerReceiption.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(customerReceiption.SalesCarNumber);
                if (salesCar != null) {
                    customerReceiption.SalesCarNumber = salesCar.SalesCarNumber;
                    customerReceiption.CarGradeCode = salesCar.CarGradeCode;
                    customerReceiption.Mileage = salesCar.Mileage;
                    customerReceiption.MileageUnit = salesCar.MileageUnit;
                    customerReceiption.RegistrationNumberKana = salesCar.RegistrationNumberKana;
                    customerReceiption.RegistrationNumberPlate = salesCar.RegistrationNumberPlate;
                    customerReceiption.RegistrationNumberType = salesCar.RegistrationNumberType;
                    customerReceiption.Vin = salesCar.Vin;
                    customerReceiption.MorterViecleOfficialCode = salesCar.MorterViecleOfficialCode;
                    customerReceiption.RegistrationDate = salesCar.RegistrationDate;
                    customerReceiption.FirstRegistrationYear = salesCar.FirstRegistrationYear;

                    try { ViewData["MakerName"] = salesCar.CarGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
                    try { ViewData["CarName"] = salesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { ViewData["CarGradeName"] = salesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }

                }
                
            } else {
                // グレード情報の取得
                if (!string.IsNullOrEmpty(customerReceiption.CarGradeCode)) {
                    CarGrade carGrade = new CarGradeDao(db).GetByKey(customerReceiption.CarGradeCode);
                    if (carGrade != null) {
                        try { ViewData["MakerName"] = carGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
                        try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                        ViewData["CarGradeName"] = carGrade.CarGradeName;
                    }
                }
            }

            

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["ReceiptionStateList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionStateAll(false), customerReceiption.ReceiptionState, true);
            ViewData["ReceiptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionTypeAll(false), customerReceiption.ReceiptionType, true);
            ViewData["VisitOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetVisitOpportunityAll(false), customerReceiption.VisitOpportunity, true);
            ViewData["RequestContentList"] = CodeUtils.GetSelectListByModel(dao.GetRequestContentAll(false), customerReceiption.RequestContent, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), customerReceiption.MileageUnit, false);
        }

        /// <summary>
        /// サービス受付対象データ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービス受付対象データ検索結果リスト</returns>
        private PaginatedList<V_ServiceReceiptTarget> GetSearchResultList(FormCollection form) {

            V_ServiceReceiptTargetDao v_ServiceReceiptTargetDao       = new V_ServiceReceiptTargetDao(db);
            V_ServiceReceiptTarget v_ServiceReceiptTargetCondition    = new V_ServiceReceiptTarget();
            v_ServiceReceiptTargetCondition.CustomerNameKana          = form["CustomerNameKana"];
            v_ServiceReceiptTargetCondition.CustomerName              = form["CustomerName"];
            v_ServiceReceiptTargetCondition.TelNumber                 = form["TelNumber"];
            v_ServiceReceiptTargetCondition.MorterViecleOfficialCode  = form["MorterViecleOfficialCode"];
            v_ServiceReceiptTargetCondition.RegistrationNumberType    = form["RegistrationNumberType"];
            v_ServiceReceiptTargetCondition.RegistrationNumberKana    = form["RegistrationNumberKana"];
            v_ServiceReceiptTargetCondition.RegistrationNumberPlate   = form["RegistrationNumberPlate"];
            v_ServiceReceiptTargetCondition.Vin                       = form["Vin"];
            v_ServiceReceiptTargetCondition.ModelName                 = form["ModelName"];
            v_ServiceReceiptTargetCondition.FirstReceiptionDateFrom   = CommonUtils.StrToDateTime(form["FirstReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.FirstReceiptionDateTo     = CommonUtils.StrToDateTime(form["FirstReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_ServiceReceiptTargetCondition.LastReceiptionDateFrom    = CommonUtils.StrToDateTime(form["LastReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.LastReceiptionDateTo      = CommonUtils.StrToDateTime(form["LastReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            return v_ServiceReceiptTargetDao.GetListByCondition(v_ServiceReceiptTargetCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Mod 2015/09/10 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動対応(類似対応)  ErrorSalesCarの型をSalesCar→List<SalesCar>に変更
        /// <summary>
        /// 新規登録時のマスタ存在チェック
        /// </summary>
        /// <param name="salesCar"></param>
        private void ValidateForInsert(CustomerReceiption customerReceiption) { 
            List<SalesCar> list = new SalesCarDao(db).GetByVin(customerReceiption.Vin);
            if (list != null && list.Count > 0) {
                ModelState.AddModelError("Vin", "車台番号:" + customerReceiption.Vin + "は既に登録されています");
                ViewData["ErrorSalesCar"] = list;
            }
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="customerReceiption">サービス受付データ</param>
        /// <returns>サービス受付データ</returns>
        /// <history>
        /// 2018/04/26 arc yano #3816 車両査定入力　管理番号にN/Aが入ってしまう
        /// </history>
        private CustomerReceiption ValidateServiceReceiption(CustomerReceiption customerReceiption) {

            // 必須チェック(日付項目は属性チェックも兼ねる)
            if (string.IsNullOrEmpty(customerReceiption.CustomerCode)) {
                ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0001", "顧客コード"));
            }
            if (customerReceiption.ReceiptionDate == null) {
                ModelState.AddModelError("ReceiptionDate", MessageUtils.GetMessage("E0003", "受付日"));
            }
            if (string.IsNullOrEmpty(customerReceiption.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            if (string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            if (addSalesCar)    //Mod 2018/04/26 arc yano #3816
            {
                if (string.IsNullOrEmpty(customerReceiption.CarGradeCode))
                {
                    ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "顧客コードと車台番号の新しい組み合わせ登録", "グレードコード" }));
                }
                else //グレードコードが入力されていた場合は、マスタチェックを行う
                {
                    CarGrade rec = new CarGradeDao(db).GetByKey(customerReceiption.CarGradeCode);

                    if (rec == null)
                    {
                        ModelState.AddModelError("CarGradeCode", "車両グレードマスタに登録されていません。マスタ登録を行ってから再度実行して下さい");
                    }
                }
            }
            /*
            if (addSalesCar && string.IsNullOrEmpty(customerReceiption.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "顧客コードと車台番号の新しい組み合わせ登録", "グレードコード" }));
            }
            */
            // 属性チェック
            if (!ModelState.IsValidField("ArrivalPlanDate")) {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0005", "入庫日"));
            }
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
            }
            if (!ModelState.IsValidField("RegistrationDate")) {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "登録日"));
            }
            if (!string.IsNullOrEmpty(customerReceiption.FirstRegistrationYear)) {
                if (!Regex.IsMatch(customerReceiption.FirstRegistrationYear, "([0-9]{4})/([0-9]{2})")
                    && !Regex.IsMatch(customerReceiption.FirstRegistrationYear, "([0-9]{4}/[0-9]{1})")) {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録年月"));
                }
                DateTime result;
                try {
                    DateTime.TryParse(customerReceiption.FirstRegistrationYear + "/01", out result);
                    if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
                        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録年月"));
                        if (ModelState["FirstRegistrationYear"].Errors.Count() > 1) {
                            ModelState["FirstRegistrationYear"].Errors.RemoveAt(0);
                        }
                    }
                } catch {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録年月"));
                }

            }
            // フォーマットチェック
            if (ModelState.IsValidField("Mileage") && customerReceiption.Mileage != null) {
                if ((Regex.IsMatch(customerReceiption.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(customerReceiption.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
                }
            }

            // 値チェック
            /*if (ModelState.IsValidField("ArrivalPlanDate") && (customerReceiption.ArrivalPlanDate != null)) {
                if (DateTime.Compare(customerReceiption.ArrivalPlanDate ?? DateTime.MaxValue , DateTime.Now.Date) < 0) {
                    ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0013", new string[] { "入庫日", "本日以降" }));
                }
            }*/

            return customerReceiption;
        }

        /// <summary>
        /// 顧客受付テーブル追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="customerReceiption">顧客受付テーブルデータ(登録内容)</param>
        /// <returns>顧客受付テーブルモデルクラス</returns>
        private CustomerReceiption EditCustomerReceiptionForInsert(CustomerReceiption customerReceiption) {

            customerReceiption.CarReceiptionId = Guid.NewGuid();
            customerReceiption.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.CreateDate = DateTime.Now;
            customerReceiption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.LastUpdateDate = DateTime.Now;
            customerReceiption.DelFlag = "0";
            return customerReceiption;
        }

        /// <summary>
        /// 車両マスタ追加データ編集
        /// </summary>
        /// <param name="customerReceiption">顧客受付テーブルデータ</param>
        /// <returns>車両マスタモデルクラス</returns>
        private SalesCar EditSalesCarForInsert(CustomerReceiption customerReceiption) {

            SalesCar salesCar = new SalesCar();
            salesCar.CarGradeCode = customerReceiption.CarGradeCode;
            salesCar.NewUsedType = "U"; // 新中区分:中古車
            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            salesCar.Mileage = customerReceiption.Mileage;
            salesCar.MileageUnit = customerReceiption.MileageUnit;
            //salesCar.OwnerCode = customerReceiption.CustomerCode;
            salesCar.UserCode = customerReceiption.CustomerCode;
            salesCar.Vin = customerReceiption.Vin;
            salesCar.MorterViecleOfficialCode = customerReceiption.MorterViecleOfficialCode;
            salesCar.RegistrationNumberType = customerReceiption.RegistrationNumberType;
            salesCar.RegistrationNumberKana = customerReceiption.RegistrationNumberKana;
            salesCar.RegistrationNumberPlate = customerReceiption.RegistrationNumberPlate;
            salesCar.RegistrationDate = customerReceiption.RegistrationDate;
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            return salesCar;
        }

        /// <summary>
        /// 顧客マスタ更新データ編集(来店日の更新)
        /// </summary>
        /// <param name="customerReceiption">顧客マスタデータ</param>
        /// <returns>顧客マスタモデルクラス</returns>
        private Customer EditCustomerForUpdate(Customer customer) {

            if (customer.FirstReceiptionDate == null) {
                customer.FirstReceiptionDate = DateTime.Now.Date;
            }
            customer.LastReceiptionDate = DateTime.Now.Date;
            customer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.LastUpdateDate = DateTime.Now;
            return customer;
        }

        public ActionResult History(string customerCode, string salesCarNumber,FormCollection form) {

            ServiceReceiptionHistory history = new ServiceReceiptionHistory();
            history.Customer = new CustomerDao(db).GetByKey(customerCode);
            history.SalesCar = new SalesCarDao(db).GetByKey(salesCarNumber);

            ServiceSalesHeader condition = new ServiceSalesHeader();
            condition.Customer = new Customer();
            condition.Customer.CustomerCode = customerCode;
            condition.SalesCarNumber = salesCarNumber;
            condition.DelFlag = "0";
            history.ServiceSalesHeader = new ServiceSalesOrderDao(db).GetListByCondition(condition, (Employee)Session["Employee"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            //history.V_ServiceReceiptionHistory = new V_ServiceReceiptionHistoryDao(db).GetListByCondition(customerCode, salesCarNumber);
            return View("ServiceReceiptionHistory", history);
        }
    }
}
