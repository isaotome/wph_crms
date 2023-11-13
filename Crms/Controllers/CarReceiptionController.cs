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
    /// 店舗受付機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarReceiptionController : Controller {

        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "店舗受付入力"; // 画面名
        private static readonly string PROC_NAME = "店舗受付登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarReceiptionController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 店舗受付検索画面表示
        /// </summary>
        /// <returns>店舗受付検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 店舗受付検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>店舗受付検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // 検索結果リストの取得
            PaginatedList<V_ServiceReceiptTarget> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            //ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["Vin"] = form["Vin"];
            //ViewData["CarName"] = form["CarName"];
            ViewData["ModelName"] = form["ModelName"];
            ViewData["FirstReceiptionDateFrom"] = form["FirstReceiptionDateFrom"];
            ViewData["FirstReceiptionDateTo"] = form["FirstReceiptionDateTo"];
            ViewData["LastReceiptionDateFrom"] = form["LastReceiptionDateFrom"];
            ViewData["LastReceiptionDateTo"] = form["LastReceiptionDateTo"];

            // 店舗受付検索画面の表示
            return View("CarReceiptionCriteria", list);
        }

        /// <summary>
        /// 来店履歴画面表示
        /// </summary>
        /// <param name="id">顧客コード</param>
        /// <returns>来店履歴画面</returns>
        public ActionResult History(string id) {

            // 検索結果リストの取得
            List<CustomerReceiption> list = new CustomerReceiptionDao(db).GetHistoryByCustomer(id);

            // その他出力項目の設定
            ViewData["CustomerCode"] = id;
            if (!string.IsNullOrEmpty(id)) {
                Customer customer = new CustomerDao(db).GetByKey(id);
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

            // 来店履歴画面の表示
            return View("CarReceiptionHistory", list);
        }

        /// <summary>
        /// 店舗受付入力画面表示
        /// </summary>
        /// <param name="id">顧客コード</param>
        /// <returns>店舗受付入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            // モデルデータの生成
            CustomerReceiption customerReceiption = new CustomerReceiption();
            customerReceiption.CustomerCode = id;
            customerReceiption.ReceiptionDate = DateTime.Now.Date;
            customerReceiption.QuestionnarieEntryDate = DateTime.Now.Date;
            customerReceiption.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            customerReceiption.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            // その他表示データの取得
            GetEntryViewData(customerReceiption);

            // 出口
            return View("CarReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// 店舗受付登録
        /// </summary>
        /// <param name="customerReceiption">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>店舗受付マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerReceiption customerReceiption, FormCollection form) {

            // データチェック
            ValidateCarReceiption(customerReceiption);
            if (!ModelState.IsValid) {
                GetEntryViewData(customerReceiption);
                return View("CarReceiptionEntry", customerReceiption);
            }

            // Add 2014/08/07 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // 各種データ登録処理
            using (TransactionScope ts = new TransactionScope()) {

                // 店舗受付データ登録処理
                customerReceiption = EditCustomerReceiptionForInsert(customerReceiption);
                db.CustomerReceiption.InsertOnSubmit(customerReceiption);

                // 顧客マスタ来店日更新処理
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    EditCustomerForUpdate(customer);
                }

                
                // DBアクセスの実行
                try
                {
                    db.SubmitChanges();
                    // コミット
                    ts.Complete();
                }
                catch (SqlException e)
                {
                    // Add 2014/08/07 arc amii エラーログ対応 Exception時にログ出力処理追加
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveQuote") ? "見積作成" : "保存")));
                        GetEntryViewData(customerReceiption);
                        return View("CarReceiptionEntry", customerReceiption);
                    }
                    else
                    {
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // Add 2014/08/07 arc amii エラーログ対応 上記以外のException時にログ出力する処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/23 arc ishii 保存ボタン対応
            // 出口
            if (form["action"].Equals("saveQuote")) {
                ModelState.AddModelError("", MessageUtils.GetMessage("I0003", "見積作成"));
                ViewData["url"] = "/CarSalesOrder/Entry/?customerCode=" + customerReceiption.CustomerCode + "&employeeCode=" + customerReceiption.EmployeeCode;
            }
            
            ViewData["close"] = "1";
                        
            return Entry((string)null);
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
                    customerReceiption.Customer = customer;
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
                    customerReceiption.Department = department;
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // 担当者名の取得
            if (!string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(customerReceiption.EmployeeCode);
                if (employee != null) {
                    customerReceiption.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // イベント名1,2の取得
            CampaignDao campaignDao = new CampaignDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode1)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode1);
                if (campaign != null) {
                    customerReceiption.Campaign1 = campaign;
                    ViewData["CampaignName1"] = campaign.CampaignName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode2)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode2);
                if (campaign != null) {
                    customerReceiption.Campaign2 = campaign;
                    ViewData["CampaignName2"] = campaign.CampaignName;
                }
            }

            // 興味のある商品1～4の取得
            CarDao carDao = new CarDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar1)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar1);
                if (car != null) {
                    customerReceiption.Car1 = car;
                    ViewData["InterestedCarName1"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar2)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar2);
                if (car != null) {
                    customerReceiption.Car2 = car;
                    ViewData["InterestedCarName2"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar3)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar3);
                if (car != null) {
                    customerReceiption.Car3 = car;
                    ViewData["InterestedCarName3"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar4)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar4);
                if (car != null) {
                    customerReceiption.Car4 = car;
                    ViewData["InterestedCarName4"] = car.CarName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["ReceiptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionTypeAll(false), customerReceiption.ReceiptionType, true);
            ViewData["PurposeList"] = CodeUtils.GetSelectListByModel(dao.GetPurposeAll(false), customerReceiption.Purpose, true);
            ViewData["VisitOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetVisitOpportunityAll(false), customerReceiption.VisitOpportunity, true);
            ViewData["KnowOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetKnowOpportunityAll(false), customerReceiption.KnowOpportunity, true);
            ViewData["AttractivePointList"] = CodeUtils.GetSelectListByModel(dao.GetAttractivePointAll(false), customerReceiption.AttractivePoint, true);
            ViewData["DemandList"] = CodeUtils.GetSelectListByModel(dao.GetDemandAll(false), customerReceiption.Demand, true);
            ViewData["QuestionnarieList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), customerReceiption.Questionnarie, true);
        }

        /// <summary>
        /// 顧客マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>顧客マスタ検索結果リスト</returns>
        private PaginatedList<V_ServiceReceiptTarget> GetSearchResultList(FormCollection form) {

            V_ServiceReceiptTargetDao v_ServiceReceiptTargetDao = new V_ServiceReceiptTargetDao(db);
            V_ServiceReceiptTarget v_ServiceReceiptTargetCondition = new V_ServiceReceiptTarget();
            v_ServiceReceiptTargetCondition.CustomerNameKana = form["CustomerNameKana"];
            v_ServiceReceiptTargetCondition.CustomerName = form["CustomerName"];
            v_ServiceReceiptTargetCondition.TelNumber = form["TelNumber"];
            v_ServiceReceiptTargetCondition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];
            v_ServiceReceiptTargetCondition.RegistrationNumberType = form["RegistrationNumberType"];
            v_ServiceReceiptTargetCondition.RegistrationNumberKana = form["RegistrationNumberKana"];
            v_ServiceReceiptTargetCondition.RegistrationNumberPlate = form["RegistrationNumberPlate"];
            v_ServiceReceiptTargetCondition.Vin = form["Vin"];
            v_ServiceReceiptTargetCondition.ModelName = form["ModelName"];
            v_ServiceReceiptTargetCondition.FirstReceiptionDateFrom = CommonUtils.StrToDateTime(form["FirstReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.FirstReceiptionDateTo = CommonUtils.StrToDateTime(form["FirstReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_ServiceReceiptTargetCondition.LastReceiptionDateFrom = CommonUtils.StrToDateTime(form["LastReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.LastReceiptionDateTo = CommonUtils.StrToDateTime(form["LastReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            return v_ServiceReceiptTargetDao.GetListByCondition(v_ServiceReceiptTargetCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="customerReceiption">店舗受付データ</param>
        /// <returns>店舗受付データ</returns>
        private CustomerReceiption ValidateCarReceiption(CustomerReceiption customerReceiption) {

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

            // 属性チェック
            if (!ModelState.IsValidField("QuestionnarieEntryDate")) {
                ModelState.AddModelError("QuestionnarieEntryDate", MessageUtils.GetMessage("E0005", "アンケート記入日"));
            }

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
    }
}
