using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarPurchaseOrderController : InheritedController {
        #region 初期化
        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両発注依頼引当";             // 画面名
        private static readonly string PROC_NAME = "車両発注依頼登録";             // 処理名
        private static readonly string PROC_NAME_IKKATSU = "発注依頼引当一括登録"; // 処理名
        private static readonly string PROC_NAME_HACCHU = "発注";                  // 処理名
        private static readonly string PROC_NAME_HIKIATE = "個別引当";             // 処理名
        private static readonly string PROC_NAME_TOROKU = "個別登録";              // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarPurchaseOrderController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;
        #endregion

        #region 検索画面
        /// <summary>
        /// 車両発注依頼検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両発注依頼検索処理
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (dep != null) {
                    ViewData["DepartmentName"] = dep.DepartmentName;
                }
            }
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                Employee emp = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                if (emp != null) {
                    ViewData["EmployeeName"] = emp.EmployeeName;
                }
            }
            ViewData["SalesCarNumberFrom"] = form["SalesCarNumberFrom"];
            ViewData["SalesCarNumberTo"] = form["SalesCarNumberTo"];
            ViewData["SlipNumberFrom"] = form["SlipNumberFrom"];
            ViewData["SlipNumberTo"] = form["SlipNumberTo"];
            ViewData["SalesOrderDateFrom"] = form["SalesOrderDateFrom"];
            ViewData["SalesOrderDateTo"] = form["SalesOrderDateTo"];
            ViewData["MakerOrderNumberFrom"] = form["MakerOrderNumberFrom"];
            ViewData["MakerOrderNumberTo"] = form["MakerOrderNumberTo"];
            ViewData["RegistrationPlanDateFrom"] = form["RegistrationPlanDateFrom"];
            ViewData["RegistrationPlanDateTo"] = form["RegistrationPlanDateTo"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["ModelCode"] = form["ModelCode"];
            ViewData["ModelName"] = form["ModelName"];
            ViewData["ApprovalFlag"] = !string.IsNullOrEmpty(form["ApprovalFlag"]) && form["ApprovalFlag"].Contains("true");
            ViewData["PurchaseOrderStatus"] = !string.IsNullOrEmpty(form["PurchaseOrderStatus"]) && form["PurchaseOrderStatus"].Contains("true");
            ViewData["ReservationStatus"] = !string.IsNullOrEmpty(form["ReservationStatus"]) && form["ReservationStatus"].Contains("true");
            ViewData["RegistrationStatus"] = !string.IsNullOrEmpty(form["RegistrationStatus"]) && form["RegistrationStatus"].Contains("true");
            ViewData["StopFlag"] = !string.IsNullOrEmpty(form["StopFlag"]) && form["StopFlag"].Contains("true");
            ViewData["NoRegistration"] = !string.IsNullOrEmpty(form["NoRegistration"]) && form["NoRegistration"].Contains("true");
            ViewData["CancelFlag"] = !string.IsNullOrEmpty(form["CancelFlag"]) && form["CancelFlag"].Contains("true");
            ViewData["NoReservation"] = !string.IsNullOrEmpty(form["NoReservation"]) && form["NoReservation"].Contains("true");
            ViewData["Vin"] = form["Vin"];

            CodeDao dao = new CodeDao(db);
            ViewData["RegistMonth"] = CodeUtils.GetSelectListByModel<c_RegistMonth>(dao.GetRegistMonthAll(false), form["RegistMonth"], true);

            if (!string.IsNullOrEmpty(form["CurrentMonth"]) && form["CurrentMonth"].Contains("true")) {
                ViewData["CurrentMonth"] = "true";
            } else {
                ViewData["CurrentMonth"] = "false";
            }

            PaginatedList<CarPurchaseOrder> list;
            if (criteriaInit) {
                list = new PaginatedList<CarPurchaseOrder>();
            } else {
                 list = GetSearchResultList(form);
            }
            return View("CarPurchaseOrderCriteria",list );
        }
        /// <summary>
        /// フォームの入力値からデータ検索する
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>発注依頼データリスト</returns>
        private PaginatedList<CarPurchaseOrder> GetSearchResultList(FormCollection form) {
            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            CarPurchaseOrder carPurchaseOrderCondition = new CarPurchaseOrder();
            carPurchaseOrderCondition.CarSalesHeader = new CarSalesHeader();
            //承認フラグ
            if (form["ApprovalFlag"].Contains("true")) {
                carPurchaseOrderCondition.CarSalesHeader.ApprovalFlag = "1";
            }
            //発注フラグ
            if (form["PurchaseOrderStatus"].Contains("true")) {
                carPurchaseOrderCondition.PurchaseOrderStatus = "1";
            }
            //引当フラグ
            if (form["ReservationStatus"].Contains("true")) {
                carPurchaseOrderCondition.ReservationStatus = "1";
            }
            //登録フラグ
            if (form["RegistrationStatus"].Contains("true")) {
                carPurchaseOrderCondition.RegistrationStatus = "1";
            }
            //預り
            if (form["StopFlag"].Contains("true")) {
                carPurchaseOrderCondition.StopFlag = "1";
            }
            //未引当
            if (form["NoReservation"].Contains("true")) {
                carPurchaseOrderCondition.NoReservation = "1";
            }
            //未登録
            if (form["NoRegistration"].Contains("true")) {
                carPurchaseOrderCondition.NoRegistration = "1";
            }

            //キャンセルも表示
            if (!form["CancelFlag"].Contains("true")) {
                carPurchaseOrderCondition.CancelFlag = "1";
            }

            //部門
            carPurchaseOrderCondition.DepartmentCode = form["DepartmentCode"];

            //担当者
            carPurchaseOrderCondition.EmployeeCode = form["EmployeeCode"];

            //管理番号
            carPurchaseOrderCondition.SalesCarNumberFrom = form["SalesCarNumberFrom"];
            carPurchaseOrderCondition.SalesCarNumberTo = form["SalesCarNumberTo"];

            //伝票番号
            carPurchaseOrderCondition.SlipNumberFrom = form["SlipNumberFrom"];
            carPurchaseOrderCondition.SlipNumberTo = form["SlipNumberTo"];

            //受注日
            carPurchaseOrderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseOrderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MAX);

            //オーダー番号
            carPurchaseOrderCondition.MakerOrderNumberFrom = form["MakerOrderNumberFrom"];
            carPurchaseOrderCondition.MakerOrderNumberTo = form["MakerOrderNumberTo"];

            //登録予定日
            carPurchaseOrderCondition.RegistrationPlanDateFrom = CommonUtils.StrToDateTime(form["RegistrationPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseOrderCondition.RegistrationPlanDateTo = CommonUtils.StrToDateTime(form["RegistrationPlanDateTo"], DaoConst.SQL_DATETIME_MIN);

            //メーカー名
            carPurchaseOrderCondition.MakerName = form["MakerName"];

            //車種
            carPurchaseOrderCondition.CarName = form["CarName"];

            //モデルコード
            carPurchaseOrderCondition.ModelCode = form["ModelCode"];

            //型式
            carPurchaseOrderCondition.ModelName = form["ModelName"];

            //Vin
            carPurchaseOrderCondition.Vin = form["Vin"];

            //閲覧権限の設定
            Employee employee = (Employee)Session["Employee"];
            carPurchaseOrderCondition.SetAuthCondition(employee);

            PaginatedList<CarPurchaseOrder> list = dao.GetListByCondition(carPurchaseOrderCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            CarSalesOrderDao salesDao = new CarSalesOrderDao(db);
            foreach (var a in list) {
                a.CarSalesHeader = salesDao.GetBySlipNumber(a.SlipNumber);
            }
            return list;
        }
        #endregion

        #region 新規発注依頼
        /// <summary>
        /// 新規発注依頼登録
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry()
        {
            CarPurchaseOrder order = new CarPurchaseOrder();
            order.Employee = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode);
            order.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DepartmentName"] = ((Employee)Session["Employee"]).Department1.DepartmentName;
            order.PurchaseOrderDate = DateTime.Today;

            //テスト用
            //order.CarGradeCode = "100010112001";
            //order.ExteriorColorCode = "10103";
            //order.InteriorColorCode = "10103";
            //order.PurchaseAmount = 2000000m;
            //order.PayDueDate = DateTime.Parse("2010/03/10");
            //order.SupplierCode = "0000000001";
            //order.SupplierPaymentCode = "0000000001";
            SetDataComponent(order, new FormCollection());
            return View("CarPurchaseOrderEntry",order);
        }

        /// <summary>
        /// 車両発注依頼登録処理
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarPurchaseOrder order, FormCollection form) {
            ValidateEntry(order,form);
            if (!ModelState.IsValid) {
                SetDataComponent(order,form);
                return View("CarPurchaseOrderEntry", order);
            }
            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //発注依頼データを新規作成
                order.CarPurchaseOrderNumber = new SerialNumberDao(db).GetNewCarPurchaseOrderNumber();
                
                //発注処理
                CreatePurchaseOrder(order);

                //引当処理も同時に行う
                //order.ReservationStatus = "1";

                //支払予定作成
                CreatePaymentPlan(order);

                //発注依頼INSERT
                order.CreateDate = DateTime.Now;
                order.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.LastUpdateDate = DateTime.Now;
                order.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.DelFlag = "0";
                order.PurchaseOrderStatus = "1";
                
                db.CarPurchaseOrder.InsertOnSubmit(order);

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
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
                        // エラーレベル『ERROR』でログ出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        SetDataComponent(order, form);
                        return View("CarPurchaseOrderEntry", order);
                    }
                    else
                    {
                        // エラーレベル『FATAL』でログ出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    return View("Error");
                }
            }
            SetDataComponent(order,form);
            ViewData["close"] = "1";
            return View("CarPurchaseOrderEntry", order);
        }

        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetDataComponent(CarPurchaseOrder order ,FormCollection form) {

            ViewData["CarGradeCode"] = string.IsNullOrEmpty(order.CarGradeCode ?? form["CarGradeCode"]) ? null : order.CarGradeCode ?? form["CarGradeCode"];
            ViewData["carGrade"] = new CarGradeDao(db).GetByKey(order.CarGradeCode ?? form["CarGradeCode"]);
            Department dep = new DepartmentDao(db).GetByKey(order.DepartmentCode ?? form["DepartmentCode"]);
            ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : "";
            //ADD 2014/10/30  arc ishii 保存ボタン対応　保存後画面データ表示のため
            CarColor InteriorColor = new CarColorDao(db).GetByKey(order.InteriorColorCode ?? form["InteriorColorCode"] );
            ViewData["InteriorColorName"] = InteriorColor != null ? InteriorColor.CarColorName : "";
            CarColor ExteriorColor = new CarColorDao(db).GetByKey(order.ExteriorColorCode ?? form["ExteriorColorCode"]);
            ViewData["ExteriorColorName"] = ExteriorColor != null ? ExteriorColor.CarColorName : "";
            order.Employee = new EmployeeDao(db).GetByKey(order.EmployeeCode ?? form["EmployeeCode"]);
            order.Supplier = new SupplierDao(db).GetByKey(order.SupplierCode ?? form["SupplierCode"]);
            order.SupplierPayment = new SupplierPaymentDao(db).GetByKey(order.SupplierPaymentCode ?? form["SupplierPaymentCode"]);
        }
        /// <summary>
        /// 発注依頼データ新規作成時のValidationチェック
        /// </summary>
        /// <param name="order">発注依頼データ</param>
        /// <param name="form">フォームデータ</param>
        private void ValidateEntry(CarPurchaseOrder order, FormCollection form) {
            CommonValidate("CarGradeCode", "グレード", order, true);
            CommonValidate("PurchaseOrderDate", "発注日", order, true);
            CommonValidate("DepartmentCode", "部門", order, true);
            CommonValidate("EmployeeCode", "担当者", order, true);
            CommonValidate("SupplierCode", "仕入先", order, true);
            CommonValidate("SupplierPaymentCode", "支払先", order, true);
            CommonValidate("PayDueDate", "支払期限", order, true);
            CommonValidate("Amount", "仕入価格", order, true);

            if (!string.IsNullOrEmpty(order.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                if (salesCar != null) {
                    ModelState.AddModelError("SalesCarNumber", "指定された管理番号が既に存在するため発注できません");
                } else {
                    //マスタに存在しなくても、自動採番の範囲に含まれたらエラー
                    if (!new SerialNumberDao(db).CanUseSalesCarNumber(order.SalesCarNumber)) {
                        ModelState.AddModelError("SalesCarNumber", "指定された管理番号は自動採番で使用する範囲に含まれるため使用できません");
                    }
                }
            }

        }

        #endregion

        #region 発注依頼一括画面
        /// <summary>
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="orderList">発注依頼リストデータ</param>
        private void SetDataComponent(List<CarPurchaseOrder> orderList) {
            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            dao.SetCarPurchaseOrderList(orderList);
        }
        
        /// <summary>
        /// 車両発注依頼引当リスト入力画面表示
        /// </summary>
        /// <param name="id">車両発注依頼番号リスト(カンマ区切り)</param>
        /// <returns></returns>
        public ActionResult ListEntry()
        {
            string id = Request["OrderId"];
            string[] keyList = id.Split(',');
            List<CarPurchaseOrder> list = new CarPurchaseOrderDao(db).GetListByKeyList(keyList);
            SetDataComponent(list);
            //2010.08.19ソート順の指定
            var orderdList = from a in list
                             orderby a.CarSalesHeader.SalesOrderDate, a.ReceiptAmount descending
                             select a;
            return View("CarPurchaseOrderListEntry",orderdList.ToList());
        }

        /// <summary>
        /// 発注依頼引当一括登録処理
        /// </summary>
        /// <param name="data">発注依頼データリスト</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ListEntry(List<CarPurchaseOrder> data, FormCollection form) {
            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            CarSalesOrderDao sales = new CarSalesOrderDao(db);
            JournalDao journalDao = new JournalDao(db);
            EmployeeDao employeeDao = new EmployeeDao(db);
            SupplierDao supplierDao = new SupplierDao(db);
            SupplierPaymentDao supplierPaymentDao = new SupplierPaymentDao(db);
            CarPurchaseDao purchaseDao = new CarPurchaseDao(db);
            switch (form["action"]) {
                case "sort":
                    foreach (var a in data) {
                        a.CarSalesHeader = sales.GetBySlipNumber(a.SlipNumber);
                        a.ReceiptAmount = journalDao.GetTotalBySlipNumber(a.SlipNumber);
                        a.Employee = employeeDao.GetByKey(a.EmployeeCode);
                        a.Supplier = supplierDao.GetByKey(a.SupplierCode);
                        a.SupplierPayment = supplierPaymentDao.GetByKey(a.SupplierPaymentCode);
                        if (a.CarSalesHeader == null) a.CarSalesHeader = new CarSalesHeader();
                        if (a.Employee == null) a.Employee = new Employee();
                        if (a.Supplier == null) a.Supplier = new Supplier();
                        if (a.SupplierPayment == null) a.SupplierPayment = new SupplierPayment();
                    }
                    data = SortData(data, form["sortkey"], form["desc"].Equals("1"));

                    ViewData["sortKey"] = form["sortkey"];
                    ViewData["desc"] = form["desc"];
                    ModelState.Clear();
                    SetDataComponent(data);
                    return View("CarPurchaseOrderListEntry", data);
                default:
                    //Validationチェック
                    ValidatePurchaseOrder(data);
                    if (!ModelState.IsValid) {
                        SetDataComponent(data);
                        return View("CarPurchaseOrderListEntry", data);
                    }

                    

                    using (TransactionScope ts = new TransactionScope()) {
                        CodeDao codeDao = new CodeDao(db);

                        foreach (CarPurchaseOrder formData in data) {

                            //更新対象を取得
                            CarPurchaseOrder target = dao.GetByKey(formData.CarPurchaseOrderNumber);

                            //車両伝票と紐づける
                            if (!string.IsNullOrEmpty(formData.SlipNumber)) {
                                //formData.CarSalesHeader = sales.GetBySlipNumber(formData.SlipNumber);
                                target.CarSalesHeader = formData.CarSalesHeader;
                            }

                            //発注依頼データの更新
                            target.ArrangementNumber = formData.ArrangementNumber;
                            target.ArrivalLocationCode = formData.ArrivalLocationCode;
                            target.ArrivalPlanDate = formData.ArrivalPlanDate;
                            target.DocumentReceiptDate = formData.DocumentReceiptDate;
                            target.DocumentReceiptPlanDate = formData.DocumentReceiptPlanDate;
                            target.EmployeeCode = formData.EmployeeCode;
                            target.GracePeriod = formData.GracePeriod;
                            target.IncentiveOfficeCode = formData.IncentiveOfficeCode;
                            target.InspectionDate = formData.InspectionDate;
                            target.InspectionInformation = formData.InspectionInformation;
                            target.LastUpdateDate = DateTime.Now;
                            target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            target.MakerShipmentDate = formData.MakerShipmentDate;
                            target.MakerShipmentPlanDate = formData.MakerShipmentPlanDate;
                            target.PayDueDate = formData.PayDueDate;
                            target.PDIDepartureDate = formData.PDIDepartureDate;
                            target.PurchaseOrderDate = formData.PurchaseOrderDate;
                            target.RegistrationDate = formData.RegistrationDate;
                            target.RegistrationPlanDate = formData.RegistrationPlanDate;
                            target.RegistrationPlanMonth = formData.RegistrationPlanMonth;
                            target.ReserveLocationCode = formData.ReserveLocationCode;
                            target.MakerOrderNumber = formData.MakerOrderNumber;
                            if (!string.IsNullOrEmpty(formData.StopFlag) && formData.StopFlag.Contains("true")) {
                                target.StopFlag = "1";
                            } else {
                                target.StopFlag = "0";
                            }
                            target.SupplierCode = formData.SupplierCode;
                            target.SupplierPaymentCode = formData.SupplierPaymentCode;
                            target.SalesCarNumber = formData.SalesCarNumber;
                            target.Vin = formData.Vin;
                            target.DocumentPurchaseRequestDate = formData.DocumentPurchaseRequestDate;
                            target.RegistMonth = formData.RegistMonth;
                            target.Firm = formData.Firm;
                            target.DiscountAmount = formData.DiscountAmount;
                            target.Amount = formData.Amount;
                            target.FirmMargin = formData.FirmMargin;
                            target.MetallicPrice = formData.MetallicPrice;
                            target.VehiclePrice = formData.VehiclePrice;
                            target.OptionPrice = formData.OptionPrice;

                            //仕入データを抽出
                            CarPurchase purchase = purchaseDao.GetBySalesCarNumber(formData.SalesCarNumber);

                            if (purchase != null) {
                                if (!purchase.FirmPrice.Equals(formData.FirmMargin)) {
                                    purchase.FirmPrice = formData.FirmMargin ?? 0m;
                                }
                                if (!purchase.DiscountPrice.Equals(formData.DiscountAmount)) {
                                    purchase.DiscountPrice = formData.DiscountAmount ?? 0m;
                                }
                                if (!purchase.MetallicPrice.Equals(formData.MetallicPrice)) {
                                    purchase.MetallicPrice = formData.MetallicPrice ?? 0m;
                                }
                                if (!purchase.OptionPrice.Equals(formData.OptionPrice)) {
                                    purchase.OptionPrice = formData.OptionPrice ?? 0m;
                                }
                                if (!purchase.Amount.Equals(formData.Amount)) {
                                    purchase.Amount = formData.Amount ?? 0m;
                                }
                                if (!purchase.VehiclePrice.Equals(formData.VehiclePrice)) {
                                    purchase.VehiclePrice = formData.VehiclePrice ?? 0m;
                                }
                            }


                            //発注処理
                            if (formData.PurchaseOrderStatus != null && formData.PurchaseOrderStatus.Contains("true")) {
                                target.PurchaseOrderStatus = "1";
                                target.CarGradeCode = target.CarSalesHeader.CarGradeCode;

                                //発注処理
                                CreatePurchaseOrder(target);

                                //支払予定作成
                                CreatePaymentPlan(target);
                            }

                            //引当処理
                            if (formData.ReservationStatus != null && formData.ReservationStatus.Contains("true")) {
                                target.ReservationStatus = "1";
                                if (formData.PurchaseOrderStatus.Contains("true")) {
                                    CreateReservation(target, true);
                                } else {
                                    CreateReservation(target, false);
                                }
                            }

                            //仕入予定作成
                            if (formData.PurchasePlanStatus != null && formData.PurchasePlanStatus.Contains("true")) {
                                target.PurchasePlanStatus = "1";
                                CreatePurchasePlan(target);
                            }

                            // 登録処理
                            if (formData.RegistrationStatus != null && formData.RegistrationStatus.Contains("true")) {
                                target.RegistrationStatus = "1";
                                UpdateRegistration(target);
                            }
                        }
                        try {
                            db.SubmitChanges();
                            ts.Complete();
                        } catch (SqlException e) {
                            //Add 2014/08/11 arc amii エラーログ対応 Exception発生時、エラーログ出力するよう修正
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;

                            if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "登録"));

                                //再度Validationチェック（管理番号が重複している可能性）
                                ValidatePurchaseOrder(data);
                                SetDataComponent(data);
                                return View("CarPurchaseOrderListEntry", data);
                            }
                            else
                            {
                                // ログに出力
                                OutputLogger.NLogFatal(e, PROC_NAME_IKKATSU, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_IKKATSU, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    SetDataComponent(data);
                    ViewData["close"] = "1";
                    return View("CarPurchaseOrderListEntry", data);
            }
        }


        #region Validation
        /// <summary>
        /// 一括発注処理時のValidationチェック
        /// </summary>
        /// <param name="order">発注依頼リスト</param>
        private void ValidatePurchaseOrder(List<CarPurchaseOrder> orderList) {
            //管理番号が重複入力されていたらエラー
            var query = orderList.Where(x=>!string.IsNullOrEmpty(x.SalesCarNumber))
                .GroupBy(x => x.SalesCarNumber)
                .Select(g => new { SalesCarNumber = g.Key, Num = g.Count() })
                .Where(n => 1 < n.Num);
            if (query.Count() > 0) {
                ModelState.AddModelError("", "管理番号が重複しているため保存できません");
            }

            for (int i = 0; i < orderList.Count; i++) {
                CarPurchaseOrder order = orderList[i];
                order.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(order.SlipNumber);

                #region 発注処理時
                if (order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) {
                    //発注日は必須項目
                    if (order.PurchaseOrderDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchaseOrderDate", i), MessageUtils.GetMessage("E0009", new string[] { "発注処理", "発注日" }));
                    } else if (!ModelState.IsValidField("PurchaseOrderDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchaseOrderDate", i), MessageUtils.GetMessage("E0005", "発注日"));
                        if (ModelState[string.Format("data[{0}].PurchaseOrderDate", i)].Errors.Count > 1) {
                            ModelState[string.Format("data[{0}].PurchaseOrderDate", i)].Errors.RemoveAt(0);
                        }
                    }
                    //仕入先は必須項目
                    if (string.IsNullOrEmpty(order.SupplierCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierCode", i), MessageUtils.GetMessage("E0007", new string[] { "発注処理", "仕入先コード" }));
                    }

                    //支払先は必須項目
                    if (string.IsNullOrEmpty(order.SupplierPaymentCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierPaymentCode", i), MessageUtils.GetMessage("E0007", new string[] { "発注処理", "支払先コード" }));
                    }

                    //グレードがマスタ上特定出来ない場合はNG
                    if (order.CarSalesHeader.CarGrade == null) {
                        ModelState.AddModelError("", "グレードが特定出来ないため発注できません。");
                    }

                    //支払期限は必須項目
                    if (order.PayDueDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].PayDueDate", i), MessageUtils.GetMessage("E0009", new string[] { "発注処理", "支払期限" }));
                    } else if (!ModelState.IsValidField("PayDueDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].PayDueDate", i), MessageUtils.GetMessage("E0005", "支払期限"));
                    }

                    //仕入価格は必須項目
                    if (order.Amount == null) {
                        ModelState.AddModelError(string.Format("data[{0}].Amount", i), MessageUtils.GetMessage("E0007", new string[] { "発注処理", "車両仕入価格" }));
                    }

                    //発注時に管理番号は入力されていたらマスタチェック
                    //マスタに存在したらエラー
                    if (!string.IsNullOrEmpty(order.SalesCarNumber)) {
                        SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                        if (salesCar != null) {
                            ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "指定された管理番号が既に存在するため発注できません");
                        } else {
                        //マスタに存在しなくても、自動採番の範囲に含まれたらエラー
                            if (!new SerialNumberDao(db).CanUseSalesCarNumber(order.SalesCarNumber)) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "指定された管理番号は自動採番で使用する範囲に含まれるため使用できません");
                            }
                        }
                    }
                }
                #endregion

                #region 引当処理時
                if (order.ReservationStatus!=null && order.ReservationStatus.Contains("true")) {
                    //発注を同時にしない場合
                    if(order.PurchaseOrderStatus==null || !order.PurchaseOrderStatus.Contains("true")){
                        if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                            //管理番号必須
                            ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), MessageUtils.GetMessage("E0001", "管理番号"));
                        } else {
                            //管理番号はマスタに存在し、在庫である必要がある
                            SalesCar car = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                            if (car == null) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "指定された車両はマスタに存在しません");
                            } else if (string.IsNullOrEmpty(car.LocationCode) || (car.CarStatus == null || !car.CarStatus.Equals("001"))) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "指定された車両は在庫がないか商談中です");
                            }
                        }
                    }
                }
                #endregion

                #region 仕入予定作成時
                if (order.PurchasePlanStatus!=null && order.PurchasePlanStatus.Contains("true")) {
                    if ((order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) || (order.ReservationStatus!=null && order.ReservationStatus.Contains("true"))) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchasePlanStatus", i), "発注や引当と同時に仕入予定は作成できません");
                    }
                    if (string.IsNullOrEmpty(order.ArrivalLocationCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalLocationCode", i), MessageUtils.GetMessage("E0001", "入庫ロケーション"));
                    }
                    if (order.ArrivalPlanDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalPlanDate", i), MessageUtils.GetMessage("E0001", "入庫予定日"));
                    }
                    if (!ModelState.IsValidField("ArrivalPlanDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalPlanDate", i), MessageUtils.GetMessage("E0002", "入庫予定日"));
                    }
                    if (string.IsNullOrEmpty(order.SupplierCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierCode", i), MessageUtils.GetMessage("E0001", "仕入先コード"));
                    }

                    if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                        ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), MessageUtils.GetMessage("E0001", "引当処理"));
                    }
                }
                #endregion

                #region 車両登録時
                if (order.RegistrationStatus!=null && order.RegistrationStatus.Contains("true")) {
                    if((order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) || (order.ReservationStatus!=null && order.ReservationStatus.Contains("true"))){
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationStatus",i),"発注や引当と同時に車両登録はできません");
                    }
                    if (order.RegistrationDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationDate", i), MessageUtils.GetMessage("E0001", "登録日"));
                    }
                    if (!ModelState.IsValidField("RegistrationDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationDate", i), MessageUtils.GetMessage("E0003", "登録日"));
                    }

                }
                #endregion

            }
        }
        #endregion

        #region Sort
        /// <summary>
        /// 発注依頼リストのソート
        /// </summary>
        /// <param name="data">車両発注依頼リストデータ</param>
        /// <param name="sortKey">ソートキー</param>
        /// <returns></returns>
        private List<CarPurchaseOrder> SortData(List<CarPurchaseOrder> data, string sortKey, bool desc) {

            string[] keyArray = sortKey.Split('.');

            //LinqToSqlクエリ式でソートする
            var query = from a in data select a;

            if (desc) {
                if (keyArray.Count() == 2) {
                    query = query.OrderByDescending(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null).GetType().GetProperty(keyArray[1]).GetValue(x.GetType().GetProperty(keyArray[0]).GetValue(x, null), null)).ThenBy(y => y.CarPurchaseOrderNumber);
                } else {
                    query = query.OrderByDescending(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null)).ThenBy(y => y.CarPurchaseOrderNumber);
                }
            } else {
                if (keyArray.Count() == 2) {
                    query = query.OrderBy(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null).GetType().GetProperty(keyArray[1]).GetValue(x.GetType().GetProperty(keyArray[0]).GetValue(x, null), null)).ThenBy(y => y.CarPurchaseOrderNumber);
                } else {
                    query = query.OrderBy(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null)).ThenBy(y => y.CarPurchaseOrderNumber);
                }
            }

            return query.ToList();
        }
        #endregion

        #endregion

        #region 車両発注
        /// <summary>
        /// 車両発注処理
        /// </summary>
        /// <param name="order">発注依頼データ</param>
        /// <history>
        /// 2018/07/31 #3918 yano.hiroki #3918 車両伝票の最終更新者、最終更新日を更新する処理の追加
        /// </history>
        private void CreatePurchaseOrder(CarPurchaseOrder order) {
            SalesCar salesCar = new SalesCar();

            salesCar.NewUsedType = "N";
            //管理番号未入力なら自動採番
            if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                string companyCode = "N/A";
                try { companyCode = new CarGradeDao(db).GetByKey(order.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
                salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            } else {
                salesCar.SalesCarNumber = order.SalesCarNumber;
            }
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            if (order.CarSalesHeader != null) {
                salesCar.CarGradeCode = order.CarSalesHeader.CarGradeCode;
                salesCar.ExteriorColorCode = order.CarSalesHeader.ExteriorColorCode;
                salesCar.InteriorColorCode = order.CarSalesHeader.InteriorColorCode;
                
                // 所有者情報を連携
                Customer owner = new CustomerDao(db).GetByKey(order.CarSalesHeader.PossesorCode);
                if (owner != null) {
                    salesCar.OwnerCode = order.CarSalesHeader.PossesorCode;
                    salesCar.PossesorName = owner.CustomerName;
                    salesCar.PossesorAddress = owner.Prefecture + owner.City + owner.Address1 + owner.Address2;
                }

                // 使用者情報を連携
                Customer user = new CustomerDao(db).GetByKey(order.CarSalesHeader.UserCode);
                if (user != null) {
                    salesCar.UserCode = user.CustomerCode;
                    salesCar.UserName = user.CustomerName;
                    salesCar.UserAddress = user.Prefecture + user.City + user.Address1 + user.Address2;
                } else {
                    // 使用者コードが指定されていなければ、顧客情報を連携
                    Customer customer = new CustomerDao(db).GetByKey(order.CarSalesHeader.CustomerCode);
                    if (customer != null) {
                        salesCar.UserCode = customer.CustomerCode;
                        salesCar.UserName = customer.CustomerName;
                        salesCar.UserAddress = customer.Prefecture + customer.City + customer.Address1 + customer.Address2;
                    }
                }
                

            } else {
                salesCar.CarGradeCode = order.CarGradeCode;
                salesCar.ExteriorColorCode = order.ExteriorColorCode;
                salesCar.InteriorColorCode = order.InteriorColorCode;
            }
            //db.SalesCar.InsertOnSubmit(salesCar);
            order.SalesCar = salesCar;

            //ここでSalesCarと紐づけるためにIDを挿入
            if (order.CarSalesHeader != null) {
                order.CarSalesHeader.SalesCarNumber = salesCar.SalesCarNumber;

                //Add 2018/07/31 yano.hiroki #3918
                order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.CarSalesHeader.LastUpdateDate = DateTime.Now;
            }
        }
        #endregion

        #region 引当処理
        /// <summary>
        /// 引当処理
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        /// <param name="row">行数</param>
        /// <history>
        /// 2020/08/29 yano #4061【車両マスタ】所有者変更時の車検案内情報の更新
        /// 2018/07/31 yano.hiroki #3918 車両伝票の最終更新者、最終更新日を更新するように修正
        /// </history>
        private void CreateReservation(CarPurchaseOrder order,bool purchaseOrderStatus) {

            //ステータス更新
            if (order.PurchaseOrderStatus != null && !order.PurchaseOrderStatus.Equals("1")) {
                order.PurchaseOrderStatus = "1";
            }

            //車両在庫ステータスを更新(引当済)
            if (order.SalesCar != null) {
                order.SalesCar.CarStatus = "003";

                // 所有者情報を連携
                Customer owner = new CustomerDao(db).GetByKey(order.CarSalesHeader.PossesorCode);
                if (owner != null) {
                    order.SalesCar.OwnerCode = order.CarSalesHeader.PossesorCode;
                    order.SalesCar.PossesorName = owner.CustomerName;
                    order.SalesCar.PossesorAddress = owner.Prefecture + owner.City + owner.Address1 + owner.Address2;
                }

                // 使用者情報を連携
                Customer user = new CustomerDao(db).GetByKey(order.CarSalesHeader.UserCode);
                if (user != null) {
                    order.SalesCar.UserCode = user.CustomerCode;
                    order.SalesCar.UserName = user.CustomerName;
                    order.SalesCar.UserAddress = user.Prefecture + user.City + user.Address1 + user.Address2;
                } else {
                    // 使用者コードが指定されていなければ、顧客情報を連携
                    Customer customer = new CustomerDao(db).GetByKey(order.CarSalesHeader.CustomerCode);
                    if (customer != null) {
                        order.SalesCar.UserCode = customer.CustomerCode;
                        order.SalesCar.UserName = customer.CustomerName;
                        order.SalesCar.UserAddress = customer.Prefecture + customer.City + customer.Address1 + customer.Address2;
                    }
                }

                //Add 2020/08/29 yano #4061
                //車検案内情報をクリアする
                order.SalesCar.InspectGuidFlag = "001";
                order.SalesCar.InspectGuidMemo = "";

                // 古い管理番号を履歴テーブルにコピー
                order.SalesCar.OwnershipChangeType = "099";
                order.SalesCar.OwnershipChangeDate = DateTime.Today;
                CommonUtils.CopyToSalesCarHistory(db, order.SalesCar);

            }

            //引当のみの場合、ここで車両伝票に管理番号を引当
            if (!purchaseOrderStatus && order.CarSalesHeader != null) {
                order.CarSalesHeader.SalesCarNumber = order.SalesCarNumber;
                order.CarSalesHeader.Vin = order.SalesCar != null ? order.SalesCar.Vin : "";

                //Add 2018/07/31 yano.hiroki #3918
                order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.CarSalesHeader.LastUpdateDate = DateTime.Now;
                
            }

            //受注引当の場合のみ引当依頼タスクを完了にする
            if (!string.IsNullOrEmpty(order.SlipNumber)) {
                List<Task> requestTaskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_RESERVATION_REQUEST, order.SlipNumber);
                foreach (var a in requestTaskList) {
                    a.TaskCompleteDate = DateTime.Now;
                }
                //引当確認タスク
                TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                task.ReserveConfirm(order);
            }

        }
        #endregion

        #region 仕入予定作成
        /// <summary>
        /// 仕入予定作成処理
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        /// <param name="row">行番号</param>
        /// <history>
        /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない 新規作成
        /// </history>
        private void CreatePurchasePlan(CarPurchaseOrder order) {
            //車両仕入データINSERT
            CarPurchase purchase = new CarPurchase();
            purchase.CarPurchaseId = Guid.NewGuid();
            purchase.CarPurchaseOrderNumber = order.CarPurchaseOrderNumber;
            purchase.SalesCarNumber = order.SalesCarNumber;
            purchase.SupplierCode = order.SupplierCode;
            purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchase.DelFlag = "0";
            purchase.CreateDate = DateTime.Now;

            //Mod 2018/04/10 arc yano #3879
            if (order.Location != null)
            {
                Department dep = CommonUtils.GetDepartmentFromWarehouse(db, order.Location.WarehouseCode).AsQueryable().FirstOrDefault();

                purchase.DepartmentCode = dep != null ? dep.DepartmentCode : "";
            }
            else
            {
                purchase.DepartmentCode = "";
            }

            //purchase.DepartmentCode = order.Location != null ? order.Location.DepartmentCode : "";
            purchase.PurchaseLocationCode = order.ArrivalLocationCode;
            purchase.PurchaseStatus = "001";
            purchase.Amount = order.Amount ?? 0m;
            purchase.DiscountPrice = order.DiscountAmount ?? 0m;
            purchase.VehiclePrice = order.VehiclePrice ?? 0m;
            purchase.FirmPrice = order.FirmMargin ?? 0m;
            purchase.MetallicPrice = order.MetallicPrice ?? 0m;
            purchase.OptionPrice = order.OptionPrice ?? 0m;

            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            purchase.LastEditScreen = "000";

            db.CarPurchase.InsertOnSubmit(purchase);

            purchase.CarPurchaseOrder = order;

            //車両仕入予定タスク作成
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.CarPurchasePlan(purchase);
        }
        #endregion

        #region 支払予定作成
        /// <summary>
        /// 支払予定データ作成
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        private void CreatePaymentPlan(CarPurchaseOrder order) {

            Account account = new AccountDao(db).GetByUsageType("CP");

            PaymentPlan plan = new PaymentPlan();
            plan.PaymentPlanId = Guid.NewGuid();
            plan.CreateDate = DateTime.Now;
            plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.DelFlag = "0";
            plan.LastUpdateDate = DateTime.Now;
            plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //plan.Amount = (order.Amount ?? 0) - (order.DiscountAmount ?? 0);              //支払金額＝仕入原価
            plan.Amount = order.Amount ?? 0;
            plan.PaymentableBalance = order.Amount ?? 0;
            plan.CompleteFlag = "0";
            plan.PaymentPlanDate = order.PayDueDate;                //支払期限
            plan.SupplierPaymentCode = order.SupplierPaymentCode;   //支払先
            plan.SlipNumber = order.SlipNumber;
            plan.OccurredDepartmentCode = order.CarSalesHeader != null ? order.CarSalesHeader.DepartmentCode : null;
            plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("PaymentableDepartmentCode").Value;
            plan.AccountCode = account.AccountCode;
            plan.CarPurchaseOrderNumber = order.CarPurchaseOrderNumber;
            db.PaymentPlan.InsertOnSubmit(plan);
        }
        #endregion

        #region 車両登録処理
        /// <summary>
        /// 車両登録処理
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        /// <param name="row">行番号</param>
        /// <history>
        /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない 類似対応
        /// </history>
        private void UpdateRegistration(CarPurchaseOrder order) {

            //Add 2018/05/30 arc yano #3889
            CarSalesHeader newHeader = new CarSalesHeader();
            //車両伝票の複製と既存伝票の削除

            CopyCarSalesHeader(order.CarSalesHeader, ref newHeader);

            //発注データに付け替え
            order.CarSalesHeader = newHeader;

            //伝票ステータスを登録済みに更新
            order.CarSalesHeader.SalesOrderStatus = "003";
            //
            /* Del 2018/05/30 arc yano #3889 更新ではなく、新規作成のため削除
            // Add 2015/05/11 arc.ookubo #3195 伝票ステータス登録済みに変更した人を追えるようにする
            order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            order.CarSalesHeader.LastUpdateDate = DateTime.Now;
            */
            //車両の登録日を更新
            order.SalesCar.RegistrationDate = order.RegistrationDate;
            //登録確認タスク
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.RegistrationConfirm(order);
        }
        #endregion

        #region 個別発注画面
        /// <summary>
        /// 発注入力画面
        /// </summary>
        /// <param name="id">発注依頼番号</param>
        /// <returns></returns>
        public ActionResult Entry2(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderEntry2", model);
        }

        /// <summary>
        /// 発注処理
        /// </summary>
        /// <param name="model">車両発注依頼引当データ</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry2(CarPurchaseOrder model,FormCollection form) {
            ValidateForEntry2(model);
            if (form["actionType"].Equals("Order")) {
                ValidateForPurchaseOrder(model);
            }
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderEntry2", model);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);
                UpdateModel(target);

                // 発注処理
                if (form["actionType"].Equals("Order")) {
                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);

                    // 発注ステータスを「済」にする
                    target.PurchaseOrderStatus = "1";

                    // 引当ステータスも「済」にする
                    target.ReservationStatus = "1";

                    // 仕入予定ステータスも「済」にする
                    target.PurchasePlanStatus = "1";

                    // グレードコードを割り当てる
                    target.CarGradeCode = target.CarSalesHeader != null ? target.CarSalesHeader.CarGradeCode : "";

                    // 発注処理（車両マスタ作成）
                    CreatePurchaseOrder(target);

                    // 仕入予定作成
                    CreatePurchasePlan(target);

                    // 支払予定作成
                    CreatePaymentPlan(target);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            return View("CarPurchaseOrderEntry2", model);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                        return View("Error");
                    }
                } 
            }
            ViewData["close"] = "1";
            return View("CarPurchaseOrderEntry2", model);
        }
        #endregion

        #region 個別引当入力
        /// <summary>
        /// 個別引当入力画面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ReservationEntry(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderReservationEntry", model);
        }

        /// <summary>
        /// 個別引当処理
        /// </summary>
        /// <param name="model"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ReservationEntry(CarPurchaseOrder model, FormCollection form) {
            ValidationForReservation(model);
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderReservationEntry", model);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);
                UpdateModel(target);
                
                if (form["actionType"].Equals("Reservation")) {

                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                    
                    // 引当済みに更新
                    target.ReservationStatus = "1";

                    // 引当処理
                    CreateReservation(target, false);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            return View("CarPurchaseOrderReservationEntry", model);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                        return View("Error");
                    }
                }
            }
            ViewData["close"] = "1";
            return View("CarPurchaseOrderReservationEntry", model);
        }
        #endregion

        #region 個別登録入力
        /// <summary>
        /// 個別登録画面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult RegistrationEntry(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderRegistrationEntry", model);
        }

        /// <summary>
        /// 個別登録処理
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <hisotry>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </hisotry>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RegistrationEntry(CarPurchaseOrder model, FormCollection form) {
            ValidationForRegistrationEntry(model);
            if (form["actionType"].Equals("Registration")) {
                ValidationForRegistration(model);
            }
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderRegistrationEntry", model);
            }

            // Add 2014/09/25 arc amii 西暦和暦対応 #3082 登録予定日と登録日を和暦→西暦に変換する
            // 登録予定日項目の和暦を西暦に変換
            if (!model.RegistrationPlanDateWareki.IsNull)
            {
                model.RegistrationPlanDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationPlanDateWareki, db); //Mod 2018/06/22 arc yano #3891
                //model.RegistrationPlanDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationPlanDateWareki);
            }


            // 登録日項目の和暦を西暦に変換
            if (!model.RegistrationDateWareki.IsNull)
            {
                model.RegistrationDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //model.RegistrationDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationDateWareki);
            }

            // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);

                // Add 2014/09/25 arc amii 西暦和暦対応 #3082 西暦変換された登録予定日と登録日を設定
                target.RegistrationPlanDate = model.RegistrationPlanDate;
                target.RegistrationDate = model.RegistrationDate;

                UpdateModel(target);

                if (form["actionType"].Equals("Registration")) {

                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);

                    // 登録済みに更新
                    target.RegistrationStatus = "1";

                    // 登録処理
                    UpdateRegistration(target);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetDataComponent(model);
                            return View("CarPurchaseOrderRegistrationEntry", model);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                        return View("Error");
                    }
                }
            }
            SetDataComponent(model);
            ViewData["close"] = "1";
            return View("CarPurchaseOrderRegistrationEntry", model);
        }
        #endregion

        /// <summary>
        /// 発注入力データコンポーネントの作成
        /// </summary>
        /// <param name="model"></param>
        private void SetDataComponent(CarPurchaseOrder model) {
            if (!string.IsNullOrEmpty(model.SlipNumber)) {
                model.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                model.ReceiptAmount = new JournalDao(db).GetTotalBySlipNumber(model.SlipNumber);
            }
            try { ViewData["EmployeeName"] = new EmployeeDao(db).GetByKey(model.EmployeeCode).EmployeeName; } catch { }
            try { ViewData["SupplierName"] = new SupplierDao(db).GetByKey(model.SupplierCode).SupplierName; } catch { }
            try { ViewData["SupplierPaymentName"] = new SupplierPaymentDao(db).GetByKey(model.SupplierPaymentCode).SupplierPaymentName; } catch { }
            try { ViewData["Vin"] = new SalesCarDao(db).GetByKey(model.SalesCarNumber).Vin; } catch { }
            //Add 2014/09/09 arc amii 西暦和暦対応 #3082 登録予定日と登録日のコンポーネントを設定
            // 登録予定日
            model.RegistrationPlanDateWareki = JapaneseDateUtility.GetJapaneseDate(model.RegistrationPlanDate);
            string registPlanDateGengou = "";
            if (model.RegistrationPlanDate != null)
            {
                registPlanDateGengou = model.RegistrationPlanDateWareki.Gengou.ToString();
            }

            ViewData["RegistrationPlanGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registPlanDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationPlanGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registPlanDateGengou, false);

            // 登録日
            model.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(model.RegistrationDate);
            string registDateGengou = "";
            if (model.RegistrationDate != null)
            {
                registDateGengou = model.RegistrationDateWareki.Gengou.ToString();
            }

            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registDateGengou, false);
            ViewData["SalesCarNumber"] = model.SalesCarNumber;
        }

        /// <summary>
        /// 更新時の共通項目変更
        /// </summary>
        /// <param name="model"></param>
        private void EditForUpdate(CarPurchaseOrder model) {
            model.LastUpdateDate = DateTime.Now;
            model.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        }

        #region 個別処理用Validation
        /// <summary>
        /// 発注入力保存時のValidationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidateForEntry2(CarPurchaseOrder model) {
            CommonValidate("VehiclePrice", "車両本体価格", model, false);
            CommonValidate("DiscountAmount", "ディスカウント", model, false);
            CommonValidate("MetallicPrice", "メタリック", model, false);
            CommonValidate("OptionPrice", "オプション", model, false);
        }
        /// <summary>
        /// 発注処理時のValidationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidateForPurchaseOrder(CarPurchaseOrder model) {
            CommonValidate("PurchaseOrderDate", "発注日", model, true);
            CommonValidate("SupplierCode", "仕入先", model, true);
            CommonValidate("ArrivalPlanDate", "入庫予定日", model, true);
            CommonValidate("ArrivalLocationCode", "入庫ロケーション", model, true);
            CommonValidate("SupplierPaymentCode", "支払先", model, true);
            CommonValidate("PayDueDate", "支払期限", model, true);
            CommonValidate("Amount", "仕入価格", model, true);
            if (string.IsNullOrEmpty(model.SlipNumber) || new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber).CarGrade == null) {
                ModelState.AddModelError("", "受注伝票のグレードが特定できないため発注できません");
            }
            if (!string.IsNullOrEmpty(model.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                if (salesCar != null) {
                    ModelState.AddModelError("SalesCarNumber", "指定された管理番号が既に存在するため発注できません");
                } else {
                    //マスタに存在しなくても、自動採番の範囲に含まれたらエラー
                    if (!new SerialNumberDao(db).CanUseSalesCarNumber(model.SalesCarNumber)) {
                        ModelState.AddModelError("SalesCarNumber", "指定された管理番号は自動採番で使用する範囲に含まれるため使用できません");
                    }
                }
            }
        }

        /// <summary>
        /// 引き当て処理時のValidationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForReservation(CarPurchaseOrder model) {
            if (string.IsNullOrEmpty(model.SalesCarNumber)) {
                //管理番号必須
                ModelState.AddModelError("SalesCarNumber", MessageUtils.GetMessage("E0001", "管理番号"));
            } else {
                //管理番号はマスタに存在し、在庫である必要がある
                SalesCar car = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                if (car == null) {
                    ModelState.AddModelError("SalesCarNumber", "指定された車両はマスタに存在しません");
                } else if (string.IsNullOrEmpty(car.LocationCode) || (car.CarStatus == null || !car.CarStatus.Equals("001"))) {
                    ModelState.AddModelError("SalesCarNumber", "指定された車両は在庫がないか商談中です");
                }
            }
        }

        /// <summary>
        /// 登録処理時のValidationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForRegistration(CarPurchaseOrder model) {
            // Mod 2014/09/25 arc amii 西暦和暦対応 #3082 登録日の入力チェックの方法を変更
            if (model.RegistrationDateWareki.IsNull == true)
            {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0001", "登録日"));
            }
            
        }

        /// <summary>
        /// 登録保存時のValidationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForRegistrationEntry(CarPurchaseOrder model) {
           
            // Mod 2014/09/25 arc amii 西暦和暦対応 #3082 登録予定日と登録日の入力チェックの方法を変更
            // 登録予定日の入力チェック
            if (checkjapaneseDate(model.RegistrationPlanDateWareki) == false)
            {
                ModelState.AddModelError("RegistrationPlanDateWareki.Year", MessageUtils.GetMessage("E0021", "登録予定日"));
            }
            // 登録日の入力チェック
            if (checkjapaneseDate(model.RegistrationDateWareki) == false)
            {
                ModelState.AddModelError("RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "登録日"));
            }

            if (!ModelState.IsValidField("DocumentPurchaseRequestDate")) {
                ModelState.AddModelError("DocumentPurchaseRequestDate", MessageUtils.GetMessage("E0003", "書類購入希望日"));
            }
            if (!ModelState.IsValidField("DocumentReceiptPlanDate")) {
                ModelState.AddModelError("DocumentReceiptPlanDate", MessageUtils.GetMessage("E0003", "書類到着予定日"));
            }
            if (!ModelState.IsValidField("DocumentReceiptDate")) {
                ModelState.AddModelError("DocumentReceiptDate", MessageUtils.GetMessage("E0003", "書類到着日"));
            }

            //Add 2017/02/03 arc nakayama #3481_車両伝票の新中区分の間違えないようにする
            //登録する車両と伝票の車両の新中区分が不一致だった場合エラー
            if (!string.IsNullOrEmpty(model.SlipNumber))
            {
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                if (header != null)
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                    if (SalesCarData != null)
                    {
                        if (header.NewUsedType != SalesCarData.NewUsedType)
                        {
                            ModelState.AddModelError("", "車両伝票の新中区分と登録を行う車両の新中区分が一致していません。");
                        }
                    }
                }
            }
        }

        // Add 2014/09/25 arc amii 西暦和暦対応 #3082 和暦項目のチェックを行う処理を追加
        /// <summary>
        /// 和暦項目の入力チェック
        /// </summary>
        /// <param name="jaDateWareki"></param>
        /// <returns>true:正常　false:エラー</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        private bool checkjapaneseDate(JapaneseDate jaDateWareki)
        {
            // 年月日が全て未入力の場合、エラーとしない
            if (jaDateWareki.Year == null && jaDateWareki.Month == null && jaDateWareki.Day == null)
            {
                return true;
            }

            // 日付変換できない場合エラーとする
            if (JapaneseDateUtility.GlobalDateTryParse(jaDateWareki, db) == false)  //Mod 2018/06/22 arc yano #3891
            //if (JapaneseDateUtility.GlobalDateTryParse(jaDateWareki) == false)
            {
                return false;
            }
            return true;
            
        }
    #endregion

    /// <summary>
    /// 既存の車両伝票伝票を複製して既存の車両伝票を削除
    /// </summary>
    /// <param name="header">サービス伝票ヘッダ</param>
    /// <returns></returns>
    /// <history>
    /// 2023/09/28 yano #4183 インボイス対応(経理対応)
    /// 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
    /// 2023/08/15 yano #4176 販売諸費用の修正
    /// 2022/06/23 yano #4140【車両伝票入力】注文書の登録名義人が表示されない不具合の対応
    /// 2021/06/09 yano #4091 【車両伝票】オプション行の区分追加(メンテナス・延長保証)
    /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理
    /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
    /// 2018/05/30 arc yano #3889 サービス伝票発注部品が引当されない 類似処理
    /// </history>
    private void CopyCarSalesHeader(CarSalesHeader header, ref CarSalesHeader newHeader)
        {
            EntitySet<CarSalesLine> newLines = new EntitySet<CarSalesLine>();

            EntitySet<CarSalesPayment> newPayments = new EntitySet<CarSalesPayment>();

            //-------------------------
            //車両伝票支払明細のコピー
            //-------------------------
            foreach (var p in header.CarSalesPayment)
            {
                CarSalesPayment payment = new CarSalesPayment();

                payment.SlipNumber = p.SlipNumber;
                payment.RevisionNumber = p.RevisionNumber + 1;
                payment.LineNumber = p.LineNumber;
                payment.CustomerClaimCode = p.CustomerClaimCode;
                payment.PaymentPlanDate = p.PaymentPlanDate;
                payment.Amount = p.Amount;
                payment.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                payment.CreateDate = DateTime.Now;
                payment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                payment.LastUpdateDate = DateTime.Now;
                payment.DelFlag = "0";
                payment.Memo = p.Memo;

                db.CarSalesPayment.InsertOnSubmit(payment);
                
                newPayments.Add(payment);

                //既存データは削除
                p.DelFlag = "1";
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                p.LastUpdateDate = DateTime.Now;
            }

            //------------------------
            //車両伝票明細のコピー
            //------------------------
            foreach (var l in header.CarSalesLine)
            {
                CarSalesLine line = new CarSalesLine();

                line.SlipNumber = l.SlipNumber;
                line.RevisionNumber = l.RevisionNumber + 1;
                line.LineNumber = l.LineNumber;
                line.CarOptionCode = l.CarOptionCode;
                line.CarOptionName = l.CarOptionName;
                line.OptionType = l.OptionType;
                line.Amount = l.Amount;
                line.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                line.CreateDate = DateTime.Now;
                line.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                line.LastUpdateDate = DateTime.Now;
                line.DelFlag = "0";
                line.TaxAmount = l.TaxAmount;
                line.ConsumptionTaxId = l.ConsumptionTaxId;
                line.Rate = l.Rate;

                db.CarSalesLine.InsertOnSubmit(line);
  
                newLines.Add(line);

                //既存データは削除
                l.DelFlag = "1";
                l.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                l.LastUpdateDate = DateTime.Now;
            }

            //------------------------------
            //車両伝票明細のヘッダのコピー
            //------------------------------
            newHeader.SlipNumber = header.SlipNumber;
            newHeader.RevisionNumber = header.RevisionNumber + 1;
            newHeader.QuoteDate = header.QuoteDate;
            newHeader.QuoteExpireDate = header.QuoteExpireDate;
            newHeader.SalesOrderDate = header.SalesOrderDate;
            newHeader.SalesOrderStatus = header.SalesOrderStatus;
            newHeader.ApprovalFlag = header.ApprovalFlag;
            newHeader.SalesDate = header.SalesDate;
            newHeader.CustomerCode = header.CustomerCode;
            newHeader.DepartmentCode = header.DepartmentCode;
            newHeader.EmployeeCode = header.EmployeeCode;
            newHeader.CampaignCode1 = header.CampaignCode1;
            newHeader.CampaignCode2 = header.CampaignCode2;
            newHeader.NewUsedType = header.NewUsedType;
            newHeader.SalesType = header.SalesType;
            newHeader.MakerName = header.MakerName;
            newHeader.CarBrandName = header.CarBrandName;
            newHeader.CarName = header.CarName;
            newHeader.CarGradeName = header.CarGradeName;
            newHeader.CarGradeCode = header.CarGradeCode;
            newHeader.ManufacturingYear = header.ManufacturingYear;
            newHeader.ExteriorColorCode = header.ExteriorColorCode;
            newHeader.ExteriorColorName = header.ExteriorColorName;
            newHeader.InteriorColorCode = header.InteriorColorCode;
            newHeader.InteriorColorName = header.InteriorColorName;
            newHeader.Vin = header.Vin;
            newHeader.UsVin = header.UsVin;
            newHeader.ModelName = header.ModelName;
            newHeader.Mileage = header.Mileage;
            newHeader.MileageUnit = header.MileageUnit;
            newHeader.RequestPlateNumber = header.RequestPlateNumber;
            newHeader.RegistPlanDate = header.RegistPlanDate;
            newHeader.HotStatus = header.HotStatus;
            newHeader.SalesCarNumber = header.SalesCarNumber;
            newHeader.RequestRegistDate = header.RequestRegistDate;
            newHeader.SalesPlanDate = header.SalesPlanDate;
            newHeader.RegistrationType = header.RegistrationType;
            newHeader.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            newHeader.OwnershipReservation = header.OwnershipReservation;
            newHeader.CarLiabilityInsuranceType = header.CarLiabilityInsuranceType;
            newHeader.SealSubmitDate = header.SealSubmitDate;
            newHeader.ProxySubmitDate = header.ProxySubmitDate;
            newHeader.ParkingSpaceSubmitDate = header.ParkingSpaceSubmitDate;
            newHeader.CarLiabilityInsuranceSubmitDate = header.CarLiabilityInsuranceSubmitDate;
            newHeader.OwnershipReservationSubmitDate = header.OwnershipReservationSubmitDate;
            newHeader.Memo = header.Memo;
            newHeader.SalesPrice = header.SalesPrice;
            newHeader.DiscountAmount = header.DiscountAmount;
            newHeader.TaxationAmount = header.TaxationAmount;
            newHeader.TaxAmount = header.TaxAmount;
            newHeader.ShopOptionAmount = header.ShopOptionAmount;
            newHeader.ShopOptionTaxAmount = header.ShopOptionTaxAmount;
            newHeader.MakerOptionAmount = header.MakerOptionAmount;
            newHeader.MakerOptionTaxAmount = header.MakerOptionTaxAmount;
            newHeader.OutSourceAmount = header.OutSourceAmount;
            newHeader.OutSourceTaxAmount = header.OutSourceTaxAmount;
            newHeader.SubTotalAmount = header.SubTotalAmount;
            newHeader.CarTax = header.CarTax;
            newHeader.CarLiabilityInsurance = header.CarLiabilityInsurance;
            newHeader.CarWeightTax = header.CarWeightTax;
            newHeader.AcquisitionTax = header.AcquisitionTax;
            newHeader.InspectionRegistCost = header.InspectionRegistCost;
            newHeader.ParkingSpaceCost = header.ParkingSpaceCost;
            newHeader.TradeInCost = header.TradeInCost;
            newHeader.RecycleDeposit = header.RecycleDeposit;
            newHeader.RecycleDepositTradeIn = header.RecycleDepositTradeIn;
            newHeader.NumberPlateCost = header.NumberPlateCost;
            newHeader.RequestNumberCost = header.RequestNumberCost;
            newHeader.TradeInFiscalStampCost = header.TradeInFiscalStampCost;
            newHeader.TaxFreeFieldName = header.TaxFreeFieldName;
            newHeader.TaxFreeFieldValue = header.TaxFreeFieldValue;
            newHeader.TaxFreeTotalAmount = header.TaxFreeTotalAmount;
            newHeader.InspectionRegistFee = header.InspectionRegistFee;
            newHeader.ParkingSpaceFee = header.ParkingSpaceFee;
            newHeader.TradeInFee = header.TradeInFee;
            newHeader.PreparationFee = header.PreparationFee;
            newHeader.RecycleControlFee = header.RecycleControlFee;
            newHeader.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn;
            newHeader.RequestNumberFee = header.RequestNumberFee;
            newHeader.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount;
            newHeader.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount;
            newHeader.TradeInAppraisalFee = header.TradeInAppraisalFee;
            newHeader.FarRegistFee = header.FarRegistFee;
            newHeader.OutJurisdictionRegistFee = header.OutJurisdictionRegistFee;   //Add 2023/08/15 yano #4176
            newHeader.TradeInMaintenanceFee = header.TradeInMaintenanceFee;
            newHeader.InheritedInsuranceFee = header.InheritedInsuranceFee;
            newHeader.TaxationFieldName = header.TaxationFieldName;
            newHeader.TaxationFieldValue = header.TaxationFieldValue;
            newHeader.SalesCostTotalAmount = header.SalesCostTotalAmount;
            newHeader.SalesCostTotalTaxAmount = header.SalesCostTotalTaxAmount;
            newHeader.OtherCostTotalAmount = header.OtherCostTotalAmount;
            newHeader.CostTotalAmount = header.CostTotalAmount;
            newHeader.TotalTaxAmount = header.TotalTaxAmount;
            newHeader.GrandTotalAmount = header.GrandTotalAmount;
            newHeader.PossesorCode = header.PossesorCode;
            newHeader.UserCode = header.UserCode;
            newHeader.PrincipalPlace = header.PrincipalPlace;
            newHeader.VoluntaryInsuranceType = header.VoluntaryInsuranceType;
            newHeader.VoluntaryInsuranceCompanyName = header.VoluntaryInsuranceCompanyName;
            newHeader.VoluntaryInsuranceAmount = header.VoluntaryInsuranceAmount;
            newHeader.VoluntaryInsuranceTermFrom = header.VoluntaryInsuranceTermFrom;
            newHeader.VoluntaryInsuranceTermTo = header.VoluntaryInsuranceTermTo;
            newHeader.PaymentPlanType = header.PaymentPlanType;
            newHeader.TradeInAmount1 = header.TradeInAmount1;
            newHeader.TradeInTax1 = header.TradeInTax1;
            newHeader.TradeInUnexpiredCarTax1 = header.TradeInUnexpiredCarTax1;
            newHeader.TradeInRemainDebt1 = header.TradeInRemainDebt1;
            newHeader.TradeInAppropriation1 = header.TradeInAppropriation1;
            newHeader.TradeInRecycleAmount1 = header.TradeInRecycleAmount1;
            newHeader.TradeInMakerName1 = header.TradeInMakerName1;
            newHeader.TradeInCarName1 = header.TradeInCarName1;
            newHeader.TradeInClassificationTypeNumber1 = header.TradeInClassificationTypeNumber1;
            newHeader.TradeInModelSpecificateNumber1 = header.TradeInModelSpecificateNumber1;
            newHeader.TradeInManufacturingYear1 = header.TradeInManufacturingYear1;
            newHeader.TradeInInspectionExpiredDate1 = header.TradeInInspectionExpiredDate1;
            newHeader.TradeInMileage1 = header.TradeInMileage1;
            newHeader.TradeInMileageUnit1 = header.TradeInMileageUnit1;
            newHeader.TradeInVin1 = header.TradeInVin1;
            newHeader.TradeInRegistrationNumber1 = header.TradeInRegistrationNumber1;
            newHeader.TradeInUnexpiredLiabilityInsurance1 = header.TradeInUnexpiredLiabilityInsurance1;
            newHeader.TradeInAmount2 = header.TradeInAmount2;
            newHeader.TradeInTax2 = header.TradeInTax2;
            newHeader.TradeInUnexpiredCarTax2 = header.TradeInUnexpiredCarTax2;
            newHeader.TradeInRemainDebt2 = header.TradeInRemainDebt2;
            newHeader.TradeInAppropriation2 = header.TradeInAppropriation2;
            newHeader.TradeInRecycleAmount2 = header.TradeInRecycleAmount2;
            newHeader.TradeInMakerName2 = header.TradeInMakerName2;
            newHeader.TradeInCarName2 = header.TradeInCarName2;
            newHeader.TradeInClassificationTypeNumber2 = header.TradeInClassificationTypeNumber2;
            newHeader.TradeInModelSpecificateNumber2 = header.TradeInModelSpecificateNumber2;
            newHeader.TradeInManufacturingYear2 = header.TradeInManufacturingYear2;
            newHeader.TradeInInspectionExpiredDate2 = header.TradeInInspectionExpiredDate2;
            newHeader.TradeInMileage2 = header.TradeInMileage2;
            newHeader.TradeInMileageUnit2 = header.TradeInMileageUnit2;
            newHeader.TradeInVin2 = header.TradeInVin2;
            newHeader.TradeInRegistrationNumber2 = header.TradeInRegistrationNumber2;
            newHeader.TradeInUnexpiredLiabilityInsurance2 = header.TradeInUnexpiredLiabilityInsurance2;
            newHeader.TradeInAmount3 = header.TradeInAmount3;
            newHeader.TradeInTax3 = header.TradeInTax3;
            newHeader.TradeInUnexpiredCarTax3 = header.TradeInUnexpiredCarTax3;
            newHeader.TradeInRemainDebt3 = header.TradeInRemainDebt3;
            newHeader.TradeInAppropriation3 = header.TradeInAppropriation3;
            newHeader.TradeInRecycleAmount3 = header.TradeInRecycleAmount3;
            newHeader.TradeInMakerName3 = header.TradeInMakerName3;
            newHeader.TradeInCarName3 = header.TradeInCarName3;
            newHeader.TradeInClassificationTypeNumber3 = header.TradeInClassificationTypeNumber3;
            newHeader.TradeInModelSpecificateNumber3 = header.TradeInModelSpecificateNumber3;
            newHeader.TradeInManufacturingYear3 = header.TradeInManufacturingYear3;
            newHeader.TradeInInspectionExpiredDate3 = header.TradeInInspectionExpiredDate3;
            newHeader.TradeInMileage3 = header.TradeInMileage3;
            newHeader.TradeInMileageUnit3 = header.TradeInMileageUnit3;
            newHeader.TradeInVin3 = header.TradeInVin3;
            newHeader.TradeInRegistrationNumber3 = header.TradeInRegistrationNumber3;
            newHeader.TradeInUnexpiredLiabilityInsurance3 = header.TradeInUnexpiredLiabilityInsurance3;
            newHeader.TradeInTotalAmount = header.TradeInTotalAmount;
            newHeader.TradeInTaxTotalAmount = header.TradeInTaxTotalAmount;
            newHeader.TradeInUnexpiredCarTaxTotalAmount = header.TradeInUnexpiredCarTaxTotalAmount;
            newHeader.TradeInRemainDebtTotalAmount = header.TradeInRemainDebtTotalAmount;
            newHeader.TradeInAppropriationTotalAmount = header.TradeInAppropriationTotalAmount;
            newHeader.PaymentTotalAmount = header.PaymentTotalAmount;
            newHeader.PaymentCashTotalAmount = header.PaymentCashTotalAmount;
            newHeader.LoanPrincipalAmount = header.LoanPrincipalAmount;
            newHeader.LoanFeeAmount = header.LoanFeeAmount;
            newHeader.LoanTotalAmount = header.LoanTotalAmount;
            newHeader.LoanCodeA = header.LoanCodeA;
            newHeader.PaymentFrequencyA = header.PaymentFrequencyA;
            newHeader.PaymentTermFromA = header.PaymentTermFromA;
            newHeader.PaymentTermToA = header.PaymentTermToA;
            newHeader.BonusMonthA1 = header.BonusMonthA1;
            newHeader.BonusMonthA2 = header.BonusMonthA2;
            newHeader.FirstAmountA = header.FirstAmountA;
            newHeader.SecondAmountA = header.SecondAmountA;
            newHeader.BonusAmountA = header.BonusAmountA;
            newHeader.CashAmountA = header.CashAmountA;
            newHeader.LoanPrincipalA = header.LoanPrincipalA;
            newHeader.LoanFeeA = header.LoanFeeA;
            newHeader.LoanTotalAmountA = header.LoanTotalAmountA;
            newHeader.AuthorizationNumberA = header.AuthorizationNumberA;
            newHeader.FirstDirectDebitDateA = header.FirstDirectDebitDateA;
            newHeader.SecondDirectDebitDateA = header.SecondDirectDebitDateA;
            newHeader.LoanCodeB = header.LoanCodeB;
            newHeader.PaymentFrequencyB = header.PaymentFrequencyB;
            newHeader.PaymentTermFromB = header.PaymentTermFromB;
            newHeader.PaymentTermToB = header.PaymentTermToB;
            newHeader.BonusMonthB1 = header.BonusMonthB1;
            newHeader.BonusMonthB2 = header.BonusMonthB2;
            newHeader.FirstAmountB = header.FirstAmountB;
            newHeader.SecondAmountB = header.SecondAmountB;
            newHeader.BonusAmountB = header.BonusAmountB;
            newHeader.CashAmountB = header.CashAmountB;
            newHeader.LoanPrincipalB = header.LoanPrincipalB;
            newHeader.LoanFeeB = header.LoanFeeB;
            newHeader.LoanTotalAmountB = header.LoanTotalAmountB;
            newHeader.AuthorizationNumberB = header.AuthorizationNumberB;
            newHeader.FirstDirectDebitDateB = header.FirstDirectDebitDateB;
            newHeader.SecondDirectDebitDateB = header.SecondDirectDebitDateB;
            newHeader.LoanCodeC = header.LoanCodeC;
            newHeader.PaymentFrequencyC = header.PaymentFrequencyC;
            newHeader.PaymentTermFromC = header.PaymentTermFromC;
            newHeader.PaymentTermToC = header.PaymentTermToC;
            newHeader.BonusMonthC1 = header.BonusMonthC1;
            newHeader.BonusMonthC2 = header.BonusMonthC2;
            newHeader.FirstAmountC = header.FirstAmountC;
            newHeader.SecondAmountC = header.SecondAmountC;
            newHeader.BonusAmountC = header.BonusAmountC;
            newHeader.CashAmountC = header.CashAmountC;
            newHeader.LoanPrincipalC = header.LoanPrincipalC;
            newHeader.LoanFeeC = header.LoanFeeC;
            newHeader.LoanTotalAmountC = header.LoanTotalAmountC;
            newHeader.AuthorizationNumberC = header.AuthorizationNumberC;
            newHeader.FirstDirectDebitDateC = header.FirstDirectDebitDateC;
            newHeader.SecondDirectDebitDateC = header.SecondDirectDebitDateC;
            newHeader.CancelDate = header.CancelDate;
            newHeader.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
            newHeader.CreateDate = DateTime.Now;
            newHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
            newHeader.LastUpdateDate = DateTime.Now;
            newHeader.DelFlag = "0";
            newHeader.InspectionRegistFeeTax = header.InspectionRegistFeeTax;
            newHeader.ParkingSpaceFeeTax = header.ParkingSpaceFeeTax;
            newHeader.TradeInFeeTax = header.TradeInFeeTax;
            newHeader.PreparationFeeTax = header.PreparationFeeTax;
            newHeader.RecycleControlFeeTax = header.RecycleControlFeeTax;
            newHeader.RecycleControlFeeTradeInTax = header.RecycleControlFeeTradeInTax;
            newHeader.RequestNumberFeeTax = header.RequestNumberFeeTax;
            newHeader.CarTaxUnexpiredAmountTax = header.CarTaxUnexpiredAmountTax;
            newHeader.CarLiabilityInsuranceUnexpiredAmountTax = header.CarLiabilityInsuranceUnexpiredAmountTax;
            newHeader.TradeInAppraisalFeeTax = header.TradeInAppraisalFeeTax;
            newHeader.FarRegistFeeTax = header.FarRegistFeeTax;
            newHeader.OutJurisdictionRegistFeeTax = header.OutJurisdictionRegistFeeTax;   //Add 2023/08/15 yano #4176
            newHeader.TradeInMaintenanceFeeTax = header.TradeInMaintenanceFeeTax;
            newHeader.InheritedInsuranceFeeTax = header.InheritedInsuranceFeeTax;
            newHeader.TaxationFieldValueTax = header.TaxationFieldValueTax;
            newHeader.TradeInEraseRegist1 = header.TradeInEraseRegist1;
            newHeader.TradeInEraseRegist2 = header.TradeInEraseRegist2;
            newHeader.TradeInEraseRegist3 = header.TradeInEraseRegist3;
            newHeader.RemainAmountA = header.RemainAmountA;
            newHeader.RemainAmountB = header.RemainAmountB;
            newHeader.RemainAmountC = header.RemainAmountC;
            newHeader.RemainFinalMonthA = header.RemainFinalMonthA;
            newHeader.RemainFinalMonthB = header.RemainFinalMonthB;
            newHeader.RemainFinalMonthC = header.RemainFinalMonthC;
            newHeader.LoanRateA = header.LoanRateA;
            newHeader.LoanRateB = header.LoanRateB;
            newHeader.LoanRateC = header.LoanRateC;
            newHeader.SalesTax = header.SalesTax;
            newHeader.DiscountTax = header.DiscountTax;
            newHeader.TradeInPrice1 = header.TradeInPrice1;
            newHeader.TradeInPrice2 = header.TradeInPrice2;
            newHeader.TradeInPrice3 = header.TradeInPrice3;
            newHeader.TradeInRecycleTotalAmount = header.TradeInRecycleTotalAmount;
            newHeader.ConsumptionTaxId = header.ConsumptionTaxId;
            newHeader.Rate = header.Rate;
            newHeader.RevenueStampCost = header.RevenueStampCost;
            newHeader.TradeInCarTaxDeposit = header.TradeInCarTaxDeposit;
            newHeader.LastEditScreen = header.LastEditScreen;
            newHeader.PaymentSecondFrequencyA = header.PaymentSecondFrequencyA;
            newHeader.PaymentSecondFrequencyB = header.PaymentSecondFrequencyB;
            newHeader.PaymentSecondFrequencyC = header.PaymentSecondFrequencyC;
            newHeader.ProcessSessionId = header.ProcessSessionId;
            newHeader.EPDiscountTaxId = header.EPDiscountTaxId; //Add 2019/09/04 yano #4011
            newHeader.CostAreaCode = header.CostAreaCode;       //2020/01/06 yano #4029
            //Add 2021/06/09 yano #4091
            newHeader.MaintenancePackageAmount = header.MaintenancePackageAmount;
            newHeader.MaintenancePackageTaxAmount = header.MaintenancePackageTaxAmount;
            newHeader.ExtendedWarrantyAmount = header.ExtendedWarrantyAmount;
            newHeader.ExtendedWarrantyTaxAmount = header.ExtendedWarrantyTaxAmount;

            //Add 2022/06/23 yano #4140
            newHeader.TradeInHolderName1 = header.TradeInHolderName1;
            newHeader.TradeInHolderName2 = header.TradeInHolderName2;
            newHeader.TradeInHolderName3 = header.TradeInHolderName3;

            //Add 2023/09/18 yano #4181
            newHeader.SurchargeAmount = header.SurchargeAmount;             //特別サーチャージ
            newHeader.SurchargeTaxAmount = header.SurchargeTaxAmount;       //特別サーチャージ(消費税)

            newHeader.SuspendTaxRecv = header.SuspendTaxRecv;               //消費税調整額   //Add 2023/09/28 yano #4183


            db.CarSalesHeader.InsertOnSubmit(newHeader);

            //既存データは削除
            header.DelFlag = "1";
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;

            db.SubmitChanges();
        }
    }
}
