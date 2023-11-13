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
    /// 車両査定機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarAppraisalController : Controller {

        #region 初期化
        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両査定入力";     // 画面名
        private static readonly string PROC_NAME = "車両査定登録"; // 処理名
        //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
        //private static readonly string LAST_EDIT_APPRAISAL = "002";           // 査定画面で更新した時の値

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarAppraisalController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion

        #region 検索画面
        /// <summary>
        /// 車両査定検索画面表示
        /// </summary>
        /// <returns>車両査定検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両査定検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両査定検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["PurchaseStatus"] = (form["PurchaseStatus"] == null ? "001" : form["PurchaseStatus"]);
            ViewData["DefaultPurchaseStatus"] = "001";

            // 検索結果リストの取得
            PaginatedList<V_CarAppraisal> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["Vin"] = form["Vin"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["CreateDateFrom"] = form["CreateDateFrom"];
            ViewData["CreateDateTo"] = form["CreateDateTo"];
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(dao.GetPurchaseStatusAll(false), form["PurchaseStatus"], true);

            // 車両査定検索画面の表示
            return View("CarAppraisalCriteria", list);
        }
        /// <summary>
        /// 車両査定ビュー検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両査定ビュー検索結果リスト</returns>
        private PaginatedList<V_CarAppraisal> GetSearchResultList(FormCollection form) {

            V_CarAppraisalDao v_CarAppraisalDao = new V_CarAppraisalDao(db);
            V_CarAppraisal v_CarAppraisalCondition = new V_CarAppraisal();
            v_CarAppraisalCondition.Vin = form["Vin"];
            v_CarAppraisalCondition.SlipNumber = form["SlipNumber"];
            v_CarAppraisalCondition.CreateDateFrom = CommonUtils.GetDayStart(form["CreateDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_CarAppraisalCondition.CreateDateTo = CommonUtils.GetDayEnd(form["CreateDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_CarAppraisalCondition.PurchaseStatus = form["PurchaseStatus"];

            return v_CarAppraisalDao.GetListByCondition(v_CarAppraisalCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region 入力画面
        /// <summary>
        /// 車両査定入力画面表示
        /// </summary>
        /// <param name="id">車両査定ID</param>
        /// <returns>車両査定入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            CarAppraisal carAppraisal;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                carAppraisal = new CarAppraisal();
                carAppraisal.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carAppraisal.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //ookubo
                //消費税IDが未設定であれば、当日日付で消費税ID取得
                if (carAppraisal.ConsumptionTaxId == null)
                {
                    carAppraisal.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    carAppraisal.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carAppraisal.ConsumptionTaxId));
                }
                carAppraisal.LastEditScreen = "000";

            }
            // 更新の場合
            else {
                ViewData["update"] = "1";
                carAppraisal = new CarAppraisalDao(db).GetByKey(new Guid(id));
            }

            // その他表示データの取得
            GetEntryViewData(carAppraisal);

            // 出口
            return View("CarAppraisalEntry", carAppraisal);
        }

        /// <summary>
        /// 車両査定登録
        /// </summary>
        /// <param name="carAppraisal">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両査定テーブル入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarAppraisal carAppraisal, FormCollection form) {

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];
            //ViewData["OwnerCode"] = form["OwnerCode"];

            // データチェック
            ValidateCarAppraisal(carAppraisal);

            if (form["action"].Equals("savePurchase")) {

                ValidateForInsert(carAppraisal);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carAppraisal);
                    return View("CarAppraisalEntry", carAppraisal);
                }
            }
            if (!ModelState.IsValid) {
                GetEntryViewData(carAppraisal);
                return View("CarAppraisalEntry", carAppraisal);
            }

            // Add 2014/08/07 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // 各種データ登録処理
            using (TransactionScope ts = new TransactionScope()) {

                // 車両査定データ登録処理
                //更新
                if (form["update"].Equals("1")) {
                    CarAppraisal targetCarAppraisal = new CarAppraisalDao(db).GetByKey(carAppraisal.CarAppraisalId);
                    UpdateModel(targetCarAppraisal);
                    carAppraisal = EditCarAppraisalForUpdate(targetCarAppraisal, form);

                    //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
                    //Add 2016/09/17 arc nakayama #3630_【製造】車両売掛金対応
                    //更新時、下取車(伝票番号あり)の場合は該当車両の仕入データが存在した場合は更新する
                    /*if (!string.IsNullOrEmpty(targetCarAppraisal.SlipNumber))
                    {
                        CarSalesHeader SlipData = new CarSalesOrderDao(db).GetBySlipNumber(targetCarAppraisal.SlipNumber);

                        UpdateCarCarPurchase(targetCarAppraisal, targetCarAppraisal.SlipNumber, targetCarAppraisal.Vin, SlipData);

                        if (SlipData != null)
                        {
                            CreateTradeReceiptPlan(SlipData, targetCarAppraisal);
                        }
                    }
                    else
                    {
                        targetCarAppraisal.LastEditScreen = "000";
                    }*/
                }
                else 
                //新規登録
                {
                    carAppraisal = EditCarAppraisalForInsert(carAppraisal, form);
                    form["reportParam"] = carAppraisal.CarAppraisalId.ToString();
                    db.CarAppraisal.InsertOnSubmit(carAppraisal);
                }

                // 入荷予定データ登録処理
                if (form["action"].Equals("savePurchase")) {
                    // 古い管理番号を履歴テーブルに移動
                    if (carAppraisal.RegetVin) {
                        List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(carAppraisal.Vin);
                        foreach (var item in salesCarList) {
                            CommonUtils.CopyToSalesCarHistory(db, item);
                            item.DelFlag = "1";
                        }
                    }
                    SalesCar salesCar = EditSalesCarForInsert(carAppraisal);
                    db.SalesCar.InsertOnSubmit(salesCar);
                    CarPurchase carPurchase = EditCarPurchaseForInsert(carAppraisal, salesCar);
                    db.CarPurchase.InsertOnSubmit(carPurchase);
                }

                // DBアクセスの実行
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        // コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        // Mod 2014/08/07 arc amii エラーログ対応 ChangeConflictException発生時の処理を追加
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // Mod 2014/08/07 arc amii エラーログ対応 Exception発生時、ログ出力処理をするよう修正
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("savePurchase") ? "入荷予定作成" : "保存")));
                            GetEntryViewData(carAppraisal);
                            return View("CarAppraisalEntry", carAppraisal);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Mod 2014/08/07 arc amii エラーログ対応 Exception発生時、ログ出力処理をするよう修正
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                
            }
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
                ViewData["reportParam"] = form["reportParam"];
            } else {
                // 出口
                ViewData["close"] = "1";
            }
            ModelState.Clear();

            return Entry(carAppraisal.CarAppraisalId.ToString());
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carAppraisal">モデルデータ</param>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// </history>
        private void GetEntryViewData(CarAppraisal carAppraisal) {

            // 車両伝票情報の取得
            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber)) {
                CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(carAppraisal.SlipNumber);
                if (carSalesHeader != null) {
                    ViewData["SlipNumber"] = carSalesHeader.SlipNumber;
                    ViewData["SalesOrderDate"] = string.Format("{0:yyyy/MM/dd}", carSalesHeader.SalesOrderDate);
                    try { ViewData["OrderDepartmentName"] = carSalesHeader.Department.DepartmentName; } catch (NullReferenceException) { }
                    try { ViewData["OrderEmployeeName"] = carSalesHeader.Employee.EmployeeName; } catch (NullReferenceException) { }
                    try { ViewData["CustomerName"] = carSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }
                }
            }

            // 車両査定情報の取得
            if (carAppraisal.CarAppraisalId != null) {
                CarAppraisal dbCarAppraisal = new CarAppraisalDao(db).GetByKey(carAppraisal.CarAppraisalId);
                if (dbCarAppraisal != null) {
                    ViewData["PurchaseCreated"] = dbCarAppraisal.PurchaseCreated;

                }
            }

            // 車両入荷情報の取得
            if (carAppraisal.CarAppraisalId != null) {
                CarPurchase dbCarPurchase = new CarPurchaseDao(db).GetByCarAppraisalId(carAppraisal.CarAppraisalId);
                if (dbCarPurchase != null) {
                    ViewData["PurchaseStatus"] = dbCarPurchase.PurchaseStatus;
                    try { ViewData["PurchaseStatusName"] = dbCarPurchase.c_PurchaseStatus.Name; } catch (NullReferenceException) { }
                    ViewData["PurchaseDate"] = string.Format("{0:yyyy/MM/dd}", dbCarPurchase.PurchaseDate);
                }
            }

            // 部門名の取得
            if (!string.IsNullOrEmpty(carAppraisal.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(carAppraisal.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // 担当者名の取得
            if (!string.IsNullOrEmpty(carAppraisal.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(carAppraisal.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    carAppraisal.Employee = employee;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), carAppraisal.CarClassification, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), carAppraisal.Usage, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), carAppraisal.UsageType, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), carAppraisal.Figure, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), carAppraisal.MileageUnit, false);
            ViewData["DocumentCompleteList"] = CodeUtils.GetSelectListByModel(dao.GetDocumentCompleteAll(false), carAppraisal.DocumentComplete, true);
            ViewData["TransMissionList"] = CodeUtils.GetSelectListByModel(dao.GetTransMissionAll(false), carAppraisal.TransMission, true);
            ViewData["ChangeColorList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.ChangeColor, true);
            ViewData["GuaranteeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.Guarantee, true);
            ViewData["InstructionsList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.Instructions, true);
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), carAppraisal.Steering, true);
            ViewData["ImportList"] = CodeUtils.GetSelectListByModel(dao.GetImportAll(false), carAppraisal.Import, true);
            ViewData["LightList"] = CodeUtils.GetSelectListByModel(dao.GetLightAll(false), carAppraisal.Light, true);
            ViewData["AwList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Aw, true);
            ViewData["AeroList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Aero, true);
            ViewData["SrList"] = CodeUtils.GetSelectListByModel(dao.GetSrAll(false), carAppraisal.Sr, true);
            ViewData["CdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Cd, true);
            ViewData["MdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Md, true);
            ViewData["NaviTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.NaviType, true);
            ViewData["NaviEquipmentList"] = CodeUtils.GetSelectListByModel(dao.GetNaviEquipmentAll(false), carAppraisal.NaviEquipment, true);
            ViewData["NaviDashboardList"] = CodeUtils.GetSelectListByModel(dao.GetNaviDashboardAll(false), carAppraisal.NaviDashboard, true);
            ViewData["SeatTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSeatTypeAll(false), carAppraisal.SeatType, true);
            ViewData["RecycleList"] = CodeUtils.GetSelectListByModel(dao.GetRecycleAll(false), carAppraisal.Recycle, true);
            ViewData["RecycleTicketList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.RecycleTicket, true);
            ViewData["ReparationRecordList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.ReparationRecord, true);
            ViewData["EraseRegistList"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carAppraisal.EraseRegist, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), carAppraisal.Fuel, true);

            //ADD 2014/02/21 ookubo
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carAppraisal.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carAppraisal.ConsumptionTaxId;
            ViewData["Rate"] = carAppraisal.Rate;
            ViewData["ConsumptionTaxIdOld"] = carAppraisal.ConsumptionTaxId;
            ViewData["PurchasePlanDateOld"] = carAppraisal.PurchasePlanDate;
            //ADD end

            //Mod 2021/08/02 yano #4097 コメントアウト
            //Add 2014/09/08 arc amii 年式入力対応 #3076 モデル年の桁数が4桁を超えていた場合、4桁で表示する
            //if (CommonUtils.DefaultString(carAppraisal.ModelYear).Length > 10)
            //{
            //    carAppraisal.ModelYear = carAppraisal.ModelYear.Substring(0, 10);
            //}

            //Add 2016/09/05 arc nakayama #3630_【製造】車両売掛金対応 車両伝票・査定・仕入のデータ連携
            //伝票番号がある場合、仕入データをチェックして仕入済なら入力欄をリードオンリーにするフラグを立てる
            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber))
            {
                CarPurchase CarPurchase = new CarPurchaseDao(db).GetBySlipNumberVin(carAppraisal.SlipNumber, carAppraisal.Vin);
                if (CarPurchase != null)
                {
                    if (CarPurchase.PurchaseStatus == "002" && !new InventoryScheduleDao(db).IsClosedInventoryMonth(CarPurchase.DepartmentCode, CarPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["PurchasedFlag"] = "1";
                    }
                    else
                    {
                        ViewData["PurchasedFlag"] = "0";
                    }
                }
            }
            else
            {
                ViewData["PurchasedFlag"] = "0";
            }

            //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 
            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            //最後に金額の変動があった画面が車両仕入画面でなければメッセージ表示
            /*if (!carAppraisal.LastEditScreen.Equals(LAST_EDIT_APPRAISAL))
            {
                switch (carAppraisal.LastEditScreen)
                {
                    case "001":
                        carAppraisal.LastEditMessage = "車両伝票から査定価格、残債、未払自動車税、リサイクル預託金の各金額が変更されました。";
                        break;
                    case "003":
                        carAppraisal.LastEditMessage = "車両仕入画面から査定価格、残債、未払自動車税、リサイクル預託金の各金額が変更されました。";
                        break;
                    default:
                        carAppraisal.LastEditMessage = "";
                        break;
                }

            }
            else
            {
                carAppraisal.LastEditMessage = "";
            }*/
        }

        /// <summary>
        /// 仕入予定作成時のValidationチェック
        /// </summary>
        /// <param name="carAppraisal"></param>
        private void ValidateForInsert(CarAppraisal carAppraisal) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(carAppraisal.Vin);
            if (list != null && list.Count > 0 && !carAppraisal.RegetVin) {
                ModelState.AddModelError("Vin", "車台番号:" + carAppraisal.Vin + "は既に登録されています");
                ViewData["ErrorSalesCar"] = list;
            }
            for (int i = 0; i < list.Count(); i++) {
                if (list[i].CarStatus != null && !list[i].CarStatus.Equals("006") && !list[i].CarStatus.Equals("")) {
                    ModelState.AddModelError("Vin", list[i].Vin + " (" + (i + 1) + ")の在庫ステータスが「" + list[i].c_CarStatus.Name + "」のため管理番号の再取得が出来ません");
                }
            }

            if (string.IsNullOrEmpty(carAppraisal.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "仕入予定作成", "グレード" }));
            }
           
            if (carAppraisal.PurchasePlanDate == null && ModelState["PurchasePlanDate"].Errors.Count() == 0) {
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0007", new string[] { "仕入予定作成", "仕入予定日" }));
            }
        }
        /// <summary>
        /// 車両査定テーブル追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carAppraisal">車両査定テーブルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両査定テーブルモデルクラス</returns>
        private CarAppraisal EditCarAppraisalForInsert(CarAppraisal carAppraisal, FormCollection form) {

            carAppraisal.CarAppraisalId = Guid.NewGuid();
            if (form["action"].Equals("savePurchase")) {
                carAppraisal.PurchaseCreated = "1";
            } else {
                carAppraisal.PurchaseCreated = "0";
            }
            carAppraisal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.CreateDate = DateTime.Now;
            carAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.LastUpdateDate = DateTime.Now;
            carAppraisal.DelFlag = "0";
            return carAppraisal;
        }

        /// <summary>
        /// 車両査定テーブル更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carAppraisal">車両査定テーブルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両査定テーブルモデルクラス</returns>
        private CarAppraisal EditCarAppraisalForUpdate(CarAppraisal carAppraisal, FormCollection form) {

            if (form["action"].Equals("savePurchase")) {
                carAppraisal.PurchaseCreated = "1";
            }
            carAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.LastUpdateDate = DateTime.Now;
            return carAppraisal;
        }

        #region 車両マスタ作成
        /// <summary>
        /// 車両テーブル追加データ編集
        /// </summary>
        /// <param name="carAppraisal">車両査定テーブル追加/更新データ編集後の車両査定テーブルデータ(登録内容)</param>
        /// <returns>車両テーブルモデルクラス</returns>
        private SalesCar EditSalesCarForInsert(CarAppraisal carAppraisal) {

            SalesCar salesCar = new SalesCar();
            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(carAppraisal.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, "U");
            salesCar.CarGradeCode = carAppraisal.CarGradeCode;
            salesCar.NewUsedType = "U";
            salesCar.ExteriorColorName = carAppraisal.ExteriorColorName;
            salesCar.ChangeColor = carAppraisal.ChangeColor;
            salesCar.InteriorColorName = carAppraisal.InteriorColorName;
            salesCar.Steering = carAppraisal.Steering;
            salesCar.IssueDate = carAppraisal.IssueDate;
            salesCar.MorterViecleOfficialCode = carAppraisal.MorterViecleOfficialCode;
            salesCar.RegistrationNumberType = carAppraisal.RegistrationNumberType;
            salesCar.RegistrationNumberKana = carAppraisal.RegistrationNumberKana;
            salesCar.RegistrationNumberPlate = carAppraisal.RegistrationNumberPlate;
            salesCar.RegistrationDate = carAppraisal.RegistrationDate;
            salesCar.FirstRegistrationYear = carAppraisal.FirstRegistrationYear;
            salesCar.CarClassification = carAppraisal.CarClassification;
            salesCar.Usage = carAppraisal.Usage;
            salesCar.UsageType = carAppraisal.UsageType;
            salesCar.Figure = carAppraisal.Figure;
            salesCar.MakerName = carAppraisal.MakerName;
            salesCar.Capacity = carAppraisal.Capacity;
            salesCar.MaximumLoadingWeight = carAppraisal.MaximumLoadingWeight;
            salesCar.CarWeight = carAppraisal.CarWeight;
            salesCar.TotalCarWeight = carAppraisal.TotalCarWeight;
            salesCar.Vin = carAppraisal.Vin;
            salesCar.Length = carAppraisal.Length;
            salesCar.Width = carAppraisal.Width;
            salesCar.Height = carAppraisal.Height;
            salesCar.FFAxileWeight = carAppraisal.FFAxileWeight;
            salesCar.FRAxileWeight = carAppraisal.FRAxileWeight;
            salesCar.RFAxileWeight = carAppraisal.RFAxileWeight;
            salesCar.RRAxileWeight = carAppraisal.RRAxileWeight;
            salesCar.ModelName = carAppraisal.ModelName;
            salesCar.EngineType = carAppraisal.EngineType;
            salesCar.Displacement = carAppraisal.Displacement;
            salesCar.Fuel = carAppraisal.Fuel;
            salesCar.ModelSpecificateNumber = carAppraisal.ModelSpecificateNumber;
            salesCar.ClassificationTypeNumber = carAppraisal.ClassificationTypeNumber;
            salesCar.PossesorName = carAppraisal.PossesorName;
            salesCar.PossesorAddress = carAppraisal.PossesorAddress;
            salesCar.UserName = carAppraisal.UserName;
            salesCar.UserAddress = carAppraisal.UserAddress;
            salesCar.PrincipalPlace = carAppraisal.PrincipalPlace;
            salesCar.ExpireType = "001";
            salesCar.ExpireDate = carAppraisal.InspectionExpireDate;
            salesCar.Mileage = carAppraisal.Mileage;
            salesCar.MileageUnit = carAppraisal.MileageUnit;
            salesCar.Memo = carAppraisal.Memo;
            salesCar.DocumentComplete = carAppraisal.DocumentComplete;
            salesCar.DocumentRemarks = carAppraisal.DocumentRemarks;
            salesCar.UsVin = carAppraisal.UsVin;
            salesCar.ReparationRecord = carAppraisal.ReparationRecord;
            salesCar.Import = carAppraisal.Import;
            salesCar.Guarantee = carAppraisal.Guarantee;
            salesCar.Instructions = carAppraisal.Instructions;
            salesCar.Recycle = carAppraisal.Recycle;
            salesCar.RecycleTicket = carAppraisal.RecycleTicket;
            salesCar.Light = carAppraisal.Light;
            salesCar.Aw = carAppraisal.Aw;
            salesCar.Aero = carAppraisal.Aero;
            salesCar.Sr = carAppraisal.Sr;
            salesCar.Cd = carAppraisal.Cd;
            salesCar.Md = carAppraisal.Md;
            salesCar.NaviType = carAppraisal.NaviType;
            salesCar.NaviEquipment = carAppraisal.NaviEquipment;
            salesCar.NaviDashboard = carAppraisal.NaviDashboard;
            salesCar.SeatColor = carAppraisal.SeatColor;
            salesCar.SeatType = carAppraisal.SeatType;
            salesCar.RecycleDeposit = carAppraisal.RecycleDeposit;
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            salesCar.EraseRegist = carAppraisal.EraseRegist;
            return salesCar;
        }

        #endregion

        #region 入荷予定作成
        /// <summary>
        /// 車両入荷テーブル追加データ編集
        /// </summary>
        /// <param name="carAppraisal">車両査定テーブル追加/更新データ編集後の車両査定テーブルデータ(登録内容)</param>
        /// <param name="salesCar">車両テーブル追加データ編集後の車両テーブルデータ(登録内容)</param>
        /// <returns>車両入荷テーブルモデルクラス</returns>
        /// <hitory>
        /// 2021/08/16 yano #4001 下取仕入の入金実績が作成されない。
        /// 2018/04/04 arc ayno #3741 査定入力　仕入予定作成時の仕入金額（総額）と各項目の金額の合計の不一致について
        /// </hitory>
        private CarPurchase EditCarPurchaseForInsert(CarAppraisal carAppraisal, SalesCar salesCar) {

            CarPurchase carPurcahase = new CarPurchase();
            carPurcahase.CarPurchaseId = Guid.NewGuid();
            carPurcahase.CarAppraisalId = carAppraisal.CarAppraisalId;
            carPurcahase.PurchaseStatus = "001";
            //carPurcahase.VehiclePrice = carAppraisal.AppraisalPrice ?? 0m;

            // 仕入金額（税込）
            carPurcahase.TotalAmount = carAppraisal.AppraisalPrice ?? 0m;

            // 自税充当
            carPurcahase.CarTaxAppropriateAmount = carAppraisal.CarTaxUnexpiredAmount ?? 0m; // 自税充当＝未払自動車税(種別割)
            //#3037 2014/06/10 CarTaxAppropriatePriceに税抜額、CarTaxAppropriateTaxに税額を設定する　arc.ookubo
            //carPurcahase.CarTaxAppropriatePrice = carAppraisal.CarTaxUnexpiredAmount ?? 0m;
            carPurcahase.CarTaxAppropriateTax = Math.Truncate((carAppraisal.CarTaxUnexpiredAmount ?? 0m) * carAppraisal.Rate / (100 + carAppraisal.Rate));
            carPurcahase.CarTaxAppropriatePrice = (carPurcahase.CarTaxAppropriateAmount ?? 0m) - (carPurcahase.CarTaxAppropriateTax ?? 0m);

            // リサイクル
            carPurcahase.RecycleAmount = carAppraisal.RecycleDeposit ?? 0m;
            carPurcahase.RecyclePrice = carAppraisal.RecycleDeposit ?? 0m;

            // 車両本体価格
            carPurcahase.VehicleAmount = (carAppraisal.AppraisalPrice ?? 0m) - (carAppraisal.CarTaxUnexpiredAmount ?? 0m) - (carAppraisal.RecycleDeposit ?? 0m);
            //#3022] 車の査定価格を仕入へ反映させると５％で計算
            //MOD ookubo 2014/04/11
            //carPurcahase.VehicleTax = Math.Truncate((carPurcahase.VehicleAmount ?? 0m) * 5 / 105);
            carPurcahase.VehicleTax = Math.Truncate((carPurcahase.VehicleAmount ?? 0m) * carAppraisal.Rate / (100 + carAppraisal.Rate));
            carPurcahase.VehiclePrice = (carPurcahase.VehicleAmount ?? 0m) - (carPurcahase.VehicleTax ?? 0m);

            // オークション落札料
            carPurcahase.AuctionFeeAmount = 0m;
            carPurcahase.AuctionFeePrice = 0m;
            carPurcahase.AuctionFeeTax = 0m;

            // メタリック
            carPurcahase.MetallicPrice = 0m;
            carPurcahase.MetallicAmount = 0m;
            carPurcahase.MetallicTax = 0m;

            // オプション
            carPurcahase.OptionPrice = 0m;
            carPurcahase.OptionAmount = 0m;
            carPurcahase.OptionTax = 0m;

            // ファーム
            carPurcahase.FirmPrice = 0m;
            carPurcahase.FirmAmount = 0m;
            carPurcahase.FirmTax = 0m;

            // 値引き
            carPurcahase.DiscountPrice = 0m;
            carPurcahase.DiscountAmount = 0m;
            carPurcahase.DiscountTax = 0m;

            // 加装
            carPurcahase.EquipmentPrice = 0m;

            // 加修
            carPurcahase.RepairPrice = 0m;

            // その他
            carPurcahase.OthersPrice = 0m;
            carPurcahase.OthersAmount = 0m;
            carPurcahase.OthersTax = 0m;

            // Mod 2018/04/04 arc yano #3741
            // 消費税
            carPurcahase.TaxAmount = (carPurcahase.VehicleTax ?? 0m) + (carPurcahase.CarTaxAppropriateTax ?? 0m);
            //carPurcahase.TaxAmount = carPurcahase.VehicleTax ?? 0m;

            // 仕入金額（税抜）
            carPurcahase.Amount = (carPurcahase.TotalAmount ?? 0m) - carPurcahase.TaxAmount;

            //#3022] 車の査定価格を仕入へ反映させると５％で計算
            //ADD ookubo 2014/04/11　消費税IDと消費税率を追加
            // 消費税ID
            carPurcahase.ConsumptionTaxId = carAppraisal.ConsumptionTaxId;

            // 消費税率
            carPurcahase.Rate = carAppraisal.Rate;

            carPurcahase.SalesCarNumber = salesCar.SalesCarNumber;
            carPurcahase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurcahase.CreateDate = DateTime.Now;
            carPurcahase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurcahase.LastUpdateDate = DateTime.Now;
            carPurcahase.DelFlag = "0";
            carPurcahase.EraseRegist = carAppraisal.EraseRegist;
            carPurcahase.PurchaseDate = carAppraisal.PurchasePlanDate;

            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber)) {
                // 受注伝票と紐づいている時
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(carAppraisal.SlipNumber);
                if (header != null) {
                    carPurcahase.DepartmentCode = header.DepartmentCode;
                    carPurcahase.CarPurchaseType = "001";
                    //Add 2016/08/26 arc nakayama #3595_【大項目】車両売掛金機能改善
                    carPurcahase.SlipNumber = carAppraisal.SlipNumber;
                }
            } else {
                // 受注伝票と紐づいていない時
                carPurcahase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carPurcahase.CarPurchaseType = "002";
            }
            carPurcahase.LastEditScreen = "000";

            //Add 2021/08/16 yano #4001
            carPurcahase.Vin = carAppraisal.Vin;

            return carPurcahase;
        }
        #endregion

        //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 
        /*#region 査定に関連する車両仕入データと伝票データを更新する
        private void UpdateCarCarPurchase(CarAppraisal targetCarAppraisal, string SlipNumber, string vin, CarSalesHeader SlipData)
        {
            CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(SlipNumber, vin);

            //車両データ更新
            if (CarPurchaseData != null && !SlipData.SalesOrderStatus.Equals("004") && !SlipData.SalesOrderStatus.Equals("005"))
            {
                if (CarPurchaseData.TotalAmount != targetCarAppraisal.AppraisalPrice || CarPurchaseData.CarTaxAppropriateAmount != targetCarAppraisal.CarTaxUnexpiredAmount || CarPurchaseData.RecycleAmount != targetCarAppraisal.RecycleDeposit)
                {
                    CarPurchaseData.LastEditScreen = LAST_EDIT_APPRAISAL;
                    targetCarAppraisal.LastEditScreen = "000";
                }
                else
                {
                    targetCarAppraisal.LastEditScreen = "000";
                }

                CarPurchaseData.TotalAmount = targetCarAppraisal.AppraisalPrice; //仕入金額 = 査定金額
                CarPurchaseData.CarTaxAppropriateAmount = targetCarAppraisal.CarTaxUnexpiredAmount; //未払い自動車税
                CarPurchaseData.RecycleAmount = targetCarAppraisal.RecycleDeposit; //リサイクル(税込)
                CarPurchaseData.RecyclePrice = targetCarAppraisal.RecycleDeposit; // リサイクル(税抜)
                CarPurchaseData.LastUpdateDate = DateTime.Now; //最新更新日
                CarPurchaseData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).DepartmentCode; //最新更新者
            }
            else
            {
                targetCarAppraisal.LastEditScreen = "000";
            }
        }
        #endregion

        #region 下取車の入金予定を再作成する。残債があれば残債の入金予定も作成する
        private void CreateTradeReceiptPlan(CarSalesHeader header, CarAppraisal targetCarAppraisal)
        {
            //納車前・納車済の時は伝票に反映しない
            //Add 2017/01/13 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            if (!header.SalesOrderStatus.Equals("004") && !header.SalesOrderStatus.Equals("005"))
            {

                //既存の入金予定を削除
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => x.ReceiptType.Equals("012") || x.ReceiptType.Equals("013")).ToList();
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
                        string TradeInRecycleAmount = "0";
                        string TradeInCarTaxAppropriateAmount = "0";
                        decimal PlanAmount = 0;
                        decimal PlanRemainDebt = 0;

                        //査定の該当車両の場合は査定の金額をセットする
                        if (targetCarAppraisal.Vin == vin.ToString())
                        {
                            TradeInAmount = targetCarAppraisal.AppraisalPrice.ToString();
                            TradeInRemainDebt = targetCarAppraisal.RemainDebt.ToString() ?? "0";
                            TradeInRecycleAmount = targetCarAppraisal.RecycleDeposit.ToString() ?? "0";
                            TradeInCarTaxAppropriateAmount = targetCarAppraisal.CarTaxUnexpiredAmount.ToString() ?? "0";
                            PlanAmount = targetCarAppraisal.AppraisalPrice ?? 0m;
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }
                        else
                        {
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

                            var varCarTaxAppropriateAmount = CommonUtils.GetModelProperty(header, "TradeInUnexpiredCarTax" + i);
                            if (varCarTaxAppropriateAmount != null && !string.IsNullOrEmpty(varCarTaxAppropriateAmount.ToString()))
                            {
                                TradeInCarTaxAppropriateAmount = varCarTaxAppropriateAmount.ToString();
                            }

                            var varTradeInRecycleAmount = CommonUtils.GetModelProperty(header, "TradeInRecycleAmount" + i);
                            if (varTradeInRecycleAmount != null && !string.IsNullOrEmpty(varTradeInRecycleAmount.ToString()))
                            {
                                TradeInRecycleAmount = varTradeInRecycleAmount.ToString();
                            }

                            PlanAmount = decimal.Parse(TradeInAmount);
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }


                        //車両伝票の金額を更新
                        bool editflag = false;

                        switch (i)
                        {
                            case 1:
                                if (header.TradeInAmount1 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt1 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount1 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax1 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount1 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt1 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation1 = (header.TradeInAmount1 ?? 0m) - (header.TradeInRemainDebt1 ?? 0m);//下取車総額(下取価格 - 下取残債金額)
                                header.TradeInRecycleAmount1 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax1 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 2:
                                if (header.TradeInAmount2 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt2 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount2 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax2 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount2 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt2 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation2 = (header.TradeInAmount2 ?? 0m) - (header.TradeInRemainDebt2 ?? 0m);//下取車総額(下取価格 - 下取残債金額)
                                header.TradeInRecycleAmount2 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax2 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 3:
                                if (header.TradeInAmount3 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt3 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount3 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax3 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount3 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt3 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation3 = (header.TradeInAmount2 ?? 0m) - (header.TradeInRemainDebt2 ?? 0m);//下取車総額(下取価格 - 下取残債金額)
                                header.TradeInRecycleAmount3 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax3 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            default:
                                break;
                        }

                        if (editflag)
                        {
                            header.LastEditScreen = LAST_EDIT_APPRAISAL;
                        }
                        else
                        {
                            targetCarAppraisal.LastEditScreen = "000";
                        }

                        //下取車合計
                        header.TradeInTotalAmount = (header.TradeInAmount1 ?? 0m) + (header.TradeInAmount2 ?? 0m) + (header.TradeInAmount3 ?? 0m);
                        //残債合計
                        header.TradeInRemainDebtTotalAmount = (header.TradeInRemainDebt1 ?? 0m) + (header.TradeInRemainDebt2 ?? 0m) + (header.TradeInRemainDebt3 ?? 0m);
                        //下取車充当金総額合計
                        header.TradeInAppropriationTotalAmount = (header.TradeInAppropriation1 ?? 0m) + (header.TradeInAppropriation2 ?? 0m) + (header.TradeInAppropriation3 ?? 0m);


                        decimal JournalAmount = 0; //下取の入金額
                        decimal JournalDebtAmount = 0; //残債の入金額

                        //下取の入金額取得
                        Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                        if (JournalData != null)
                        {
                            JournalAmount = JournalData.Amount;
                        }

                        //残債の入金額取得
                        Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
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

                        db.ReceiptPlan.InsertOnSubmit(TradePlan);

                        //残債があった場合残債分の入金予定をマイナスで作成する
                        if (!string.IsNullOrEmpty(TradeInRemainDebt))
                        {
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

                            db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                        }
                        //db.SubmitChanges();
                    }
                }
            }
        }

        #endregion
        */

        #endregion

        #region Ajax
        // ADD 2016/02/16 ARC Mikami #3077 車両査定入力画面　車台番号から自動入力
        /// <summary>
        /// 車両コードから車両を取得する
        /// </summary>
        /// <param name="code">車両コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// </history>
        public ActionResult GetMasterDetail(string code, string SelectByCarSlip = "0")
        {

            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retCar = new Dictionary<string, string>();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null)
                {
                    retCar.Add("CarGradeCode", salesCar.CarGradeCode);
                    retCar.Add("MakerName", salesCar.CarGrade.Car.Brand.Maker.MakerName);
                    retCar.Add("CarBrandName", salesCar.CarGrade.Car.Brand.CarBrandName);
                    retCar.Add("CarGradeName", salesCar.CarGrade.CarGradeName);
                    retCar.Add("CarName", salesCar.CarGrade.Car.CarName);
                    retCar.Add("ExteriorColorCode", salesCar.ExteriorColorCode);
                    retCar.Add("ExteriorColorName", salesCar.ExteriorColorName == null ? "" : salesCar.ExteriorColorName);
                    retCar.Add("InteriorColorCode", salesCar.InteriorColorCode);
                    retCar.Add("InteriorColorName", salesCar.InteriorColorName == null ? "" : salesCar.InteriorColorName);
                    retCar.Add("Mileage", salesCar.Mileage.ToString());
                    retCar.Add("MileageUnit", salesCar.MileageUnit);
                    retCar.Add("ModelName", salesCar.ModelName);
                    retCar.Add("NewUsedType", salesCar.NewUsedType);
                    retCar.Add("SalesPrice", salesCar.SalesPrice.ToString());
                    //Mod 2015/07/28 arc nakayama #3217_デモカーが販売できてしまう問題の改善 　車両伝票の車台番号のルックアップから呼ばれた時　かつ　在庫ステータスが在庫の時だけ管理番号を返す
                    //Mod 2015/08/20 arc nakayama #3242_サービス伝票で車両マスタボタンを押しても車両マスタが表示されない 車両伝票から呼ばれた時だけ在庫ステータスを見るように修正
                    if (SelectByCarSlip.ToString().Equals("1"))
                    {
                        if (salesCar.CarStatus.Equals("001"))
                        {
                            retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                        }
                        else
                        {
                            retCar.Add("SalesCarNumber", "");
                        }
                    }
                    else
                    {
                        retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                    }

                    retCar.Add("Vin", salesCar.Vin);
                    retCar.Add("LocationName", salesCar.Location != null ? salesCar.Location.LocationName : "");
                    retCar.Add("InheritedInsuranceFee", "");
                    //retCar.Add("CarWeightTax", salesCar.CarWeightTax.ToString());
                    retCar.Add("RecycleDeposit", salesCar.RecycleDeposit.ToString());
                    //retCar.Add("CarLiabilityInsurance", salesCar.CarLiabilityInsurance.ToString());

                    retCar.Add("EngineType", salesCar.EngineType);

                    // ADD 2016/02/16 ARC Mikami #3077 車両査定入力画面用に追加
                    retCar.Add("FirstRegistrationYear", salesCar.FirstRegistrationYear);
                    retCar.Add("CarClassification", salesCar.CarClassification);
                    retCar.Add("Usage", salesCar.Usage);
                    retCar.Add("UsageType", salesCar.UsageType);
                    retCar.Add("Figure", salesCar.Figure);
                    retCar.Add("Capacity", salesCar.Capacity.ToString());
                    retCar.Add("MaximumLoadingWeight", salesCar.MaximumLoadingWeight.ToString());
                    retCar.Add("CarWeight", salesCar.CarWeight.ToString());
                    retCar.Add("TotalCarWeight", salesCar.TotalCarWeight.ToString());
                    retCar.Add("Length", salesCar.Length.ToString());
                    retCar.Add("Width", salesCar.Width.ToString());
                    retCar.Add("Height", salesCar.Height.ToString());
                    retCar.Add("FFAxileWeight", salesCar.FFAxileWeight.ToString());
                    retCar.Add("FRAxileWeight", salesCar.FRAxileWeight.ToString());
                    retCar.Add("RFAxileWeight", salesCar.RFAxileWeight.ToString());
                    retCar.Add("RRAxileWeight", salesCar.RRAxileWeight.ToString());
                    retCar.Add("ModelSpecificateNumber", salesCar.ModelSpecificateNumber);
                    retCar.Add("ClassificationTypeNumber", salesCar.ClassificationTypeNumber);
                    retCar.Add("Displacement", salesCar.Displacement.ToString());
                    retCar.Add("Fuel", salesCar.Fuel);
                    retCar.Add("OwnerCode", salesCar.OwnerCode);
                    retCar.Add("PossesorName", salesCar.PossesorName);
                    retCar.Add("PossesorAddress", salesCar.PossesorAddress);
                    retCar.Add("UserCode", salesCar.UserCode);
                    retCar.Add("UserName", salesCar.UserName);
                    retCar.Add("UserAddress", salesCar.UserAddress);
                    retCar.Add("PrincipalPlace", salesCar.PrincipalPlace);
                    retCar.Add("Memo", salesCar.Memo);
                    retCar.Add("DocumentRemarks", salesCar.DocumentRemarks);
                    retCar.Add("DocumentComplete", salesCar.DocumentComplete);
                    retCar.Add("IssueDate", string.Format("{0:yyyy/MM/dd}", salesCar.IssueDate));
                    retCar.Add("Guarantee", salesCar.Guarantee);
                    retCar.Add("Instructions", salesCar.Instructions);
                    retCar.Add("Steering", salesCar.Steering);
                    retCar.Add("Import", salesCar.Import);
                    retCar.Add("Light", salesCar.Light);
                    retCar.Add("Aw", salesCar.Aw);
                    retCar.Add("Aero", salesCar.Aero);
                    retCar.Add("Sr", salesCar.Sr);
                    retCar.Add("Cd", salesCar.Cd);
                    retCar.Add("Md", salesCar.Md);
                    retCar.Add("NaviType", salesCar.NaviType);
                    retCar.Add("NaviEquipment", salesCar.NaviEquipment);
                    retCar.Add("NaviDashboard", salesCar.NaviDashboard);
                    retCar.Add("SeatColor", salesCar.SeatColor);
                    retCar.Add("ChangeColor", salesCar.ChangeColor);
                    retCar.Add("SeatType", salesCar.SeatType);
                    retCar.Add("Recycle", salesCar.Recycle);
                    retCar.Add("RecycleTicket", salesCar.RecycleTicket);
                    
                    retCar.Add("ModelYear", salesCar.CarGrade.ModelYear);
                    retCar.Add("Door", salesCar.CarGrade.Door);
                    retCar.Add("TransMission", salesCar.CarGrade.TransMission);

                    // MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面用に削除
                    //retCar.Add("NextInspectionDate", string.Format("{0:yyyy/MM/dd}", salesCar.NextInspectionDate));

                    retCar.Add("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", salesCar.ExpireDate));
                    retCar.Add("UsVin", salesCar.UsVin);
                    retCar.Add("MorterViecleOfficialCode", salesCar.MorterViecleOfficialCode);
                    retCar.Add("RegistrationNumberType", salesCar.RegistrationNumberType);
                    retCar.Add("RegistrationNumberKana", salesCar.RegistrationNumberKana);
                    retCar.Add("RegistrationNumberPlate", salesCar.RegistrationNumberPlate);

                    // MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面用に削除
                    //retCar.Add("CustomerCode", salesCar.UserCode);

                    retCar.Add("CustomerName", salesCar.User != null ? salesCar.User.CustomerName : "");

                    // MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面用に削除
                    //retCar.Add("CustomerNameKana", salesCar.User != null ? salesCar.User.CustomerNameKana : "");
                    //retCar.Add("CustomerAddress", salesCar.User != null ? salesCar.User.Prefecture + salesCar.User.City + salesCar.User.Address1 + salesCar.User.Address2 : "");
                    //retCar.Add("LaborRate", salesCar.CarGrade != null && salesCar.CarGrade.Car != null && salesCar.CarGrade.Car.Brand != null ? salesCar.CarGrade.Car.Brand.LaborRate.ToString() : "");

                    // Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 車台番号から車両情報を取得できる項目の追加
                    retCar.Add("RegistrationDate", string.Format("{0:yyyy/MM/dd}", salesCar.RegistrationDate));         //登録年月日

                    // MOD 2016/02/16 ARC Mikami #3077 車両査定入力画面用に削除
                    //retCar.Add("CustomerTelNumber", salesCar.User != null ? salesCar.User.TelNumber : "");              //電話番号  //Mod 2015/09/17 arc yano  #3261 車両伝票の車両選択で「マスタ取得に失敗しました」と表示 NULLの場合は空文字に変換

                    //初年度登録(yyyy/mm)から年月を取得
                    decimal fee = 0m;
                    string firstRegistrationYear = salesCar.FirstRegistrationYear;
                    if (!string.IsNullOrEmpty(firstRegistrationYear))
                    {
                        if (firstRegistrationYear.Split('/').Length == 2)
                        {
                            string year = salesCar.FirstRegistrationYear.Split('/')[0];
                            string month = salesCar.FirstRegistrationYear.Split('/')[1];
                            DateTime firstRegist = new DateTime(int.Parse(year), int.Parse(month), 1);
                            DateTime today = DateTime.Today;
                            try
                            {
                                if (firstRegist.AddMonths(24).CompareTo(today) > 0)
                                {
                                    //24ヶ月未満
                                    fee = salesCar.CarGrade.Under24 ?? 0;
                                }
                                else if (firstRegist.AddMonths(26).CompareTo(today) > 0)
                                {
                                    //26ヵ月未満
                                    fee = salesCar.CarGrade.Under26 ?? 0;
                                }
                                else if (firstRegist.AddMonths(28).CompareTo(today) > 0)
                                {
                                    //28ヵ月未満
                                    fee = salesCar.CarGrade.Under28 ?? 0;
                                }
                                else if (firstRegist.AddMonths(30).CompareTo(today) > 0)
                                {
                                    //30ヶ月未満
                                    fee = salesCar.CarGrade.Under30 ?? 0;
                                }
                                else if (firstRegist.AddMonths(36).CompareTo(today) > 0)
                                {
                                    //36ヶ月未満
                                    fee = salesCar.CarGrade.Under36 ?? 0;
                                }
                                else if (firstRegist.AddMonths(72).CompareTo(today) > 0)
                                {
                                    //72ヶ月未満
                                    fee = salesCar.CarGrade.Under72 ?? 0;
                                }
                                else if (firstRegist.AddMonths(84).CompareTo(today) > 0)
                                {
                                    //84ヶ月未満
                                    fee = salesCar.CarGrade.Under84 ?? 0;
                                }
                                else
                                {
                                    //84ヶ月以上
                                    fee = salesCar.CarGrade.Over84 ?? 0;
                                }
                            }
                            catch (NullReferenceException)
                            {
                            }

                        }
                    }

                    retCar.Add("TradeInMaintenanceFee", fee.ToString());

                    //排気量から自動車税を取得する
                    //CarTax carTax = new CarTaxDao(db).GetByDisplacement(salesCar.Displacement ?? 0);
                    //retCar.Add("CarTax", carTax!=null ? carTax.Amount.ToString() : "0");

                    //環境性能割
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                    retCar.Add("EPDiscountTaxId", retValue.Item1);
                    retCar.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2.ToString()));
                    //retCar.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear)));
                    
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    //自賠責保険料および重量税
                    if (CommonUtils.DefaultString(salesCar.NewUsedType).Equals("N"))
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance != null ? insurance.Amount : 0m));
                        CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarGrade.CarWeight ?? 0);
                        retCar.Add("CarWeightTax", string.Format("{0:0}", weightTax != null ? weightTax.Amount : 0));
                    }
                    else
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance != null ? insurance.Amount : 0m));
                    }

                }
                return Json(retCar);
            }
            return new EmptyResult();
        }
    #endregion

        #region Validation
        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carAppraisal">車両査定データ</param>
        /// <returns>車両査定データ</returns>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2018/04/26 arc yano #3816 車両査定入力　管理番号にN/Aが入ってしまう
        /// </history>
        private CarAppraisal ValidateCarAppraisal(CarAppraisal carAppraisal) {

            // 必須チェック
            if (string.IsNullOrEmpty(carAppraisal.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            if (string.IsNullOrEmpty(carAppraisal.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            if (string.IsNullOrEmpty(carAppraisal.Vin)) {
                ModelState.AddModelError("Vin", MessageUtils.GetMessage("E0001", "車台番号"));
            }
            //ADD 2014/02/20 ookubo
            if (carAppraisal.PurchasePlanDate == null){
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0001", "仕入予定日"));
            }
            // 属性チェック
            if (!ModelState.IsValidField("IssueDate")) {
                ModelState.AddModelError("IssueDate", MessageUtils.GetMessage("E0005", "発行日"));
            }
            if (!ModelState.IsValidField("RegistrationDate")) {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "登録日"));
            }
            if (!ModelState.IsValidField("Capacity")) {
                ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("MaximumLoadingWeight")) {
                ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("CarWeight")) {
                ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("TotalCarWeight")) {
                ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Length")) {
                ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Width")) {
                ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Height")) {
                ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("FFAxileWeight")) {
                ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("FRAxileWeight")) {
                ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RFAxileWeight")) {
                ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RRAxileWeight")) {
                ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Displacement")) {
                ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "の整数10桁以内かつ少数2桁以内" }));
            }
            if (!ModelState.IsValidField("InspectionExpireDate")) {
                ModelState.AddModelError("InspectionExpireDate", MessageUtils.GetMessage("E0005", "有効期限"));
            }
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
            }
            //Mod 2021/08/02 yano #4097
            //Add 2014/09/08 arc amii 年式入力対応 #3076 モデル年の入力チェック(4桁数値以外はエラー)を追加
            if (!ModelState.IsValidField("ModelYear"))
            {
                ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "モデル年", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
            }

            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の10桁以内の整数のみ" }));
            }
            if (!ModelState.IsValidField("RemainDebt")) {
                ModelState.AddModelError("RemainDebt", MessageUtils.GetMessage("E0004", new string[] { "残債", "正の10桁以内の整数のみ" }));
            }
            if (!ModelState.IsValidField("CarTaxUnexpiredAmount")) {
                ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "自動車税種別割の残", "正の10桁以内の整数のみ" }));     //Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("AppraisalPrice")) {
                ModelState.AddModelError("AppraisalPrice", MessageUtils.GetMessage("E0004", new string[] { "査定価格", "正の10桁以内の整数のみ" }));
            }
            if (!ModelState.IsValidField("PurchasePlanDate")) {
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0005", "仕入予定日"));
            }
            if (!ModelState.IsValidField("AppraisalDate")) {
                ModelState.AddModelError("AppraisalDate", MessageUtils.GetMessage("E0005", "査定日"));
            }
            if (!ModelState.IsValidField("PurchaseAgreementDate")) {
                ModelState.AddModelError("PurchaseAgreementDate", MessageUtils.GetMessage("E0005", "買取契約日"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("Mileage") && carAppraisal.Mileage != null) {
                if ((Regex.IsMatch(carAppraisal.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carAppraisal.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
                }
            }
            //Mod 2021/08/02 yano #4097 入力可能文字フォーマットを変更(正の整数４桁のみ→正の整数4桁、または正の整数4桁かつ少数2桁以内
            //Add 2014/09/08 arc amii 年式入力対応 #3076 モデル年の入力チェック(4桁数値以外はエラー)を追加
            if (ModelState.IsValidField("ModelYear") && CommonUtils.DefaultString(carAppraisal.ModelYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(carAppraisal.ModelYear, @"^\d{4}\.\d{1,2}$"))
                  && (!Regex.IsMatch(carAppraisal.ModelYear, @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "モデル年", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
                }
            }
            //Add 2015/05/18 arc ookubo 初年度登録年月対応 #3204 入力チェックを追加
            if (ModelState.IsValidField("FirstRegistrationYear") && carAppraisal.FirstRegistrationYear != null)
            {
                if (Regex.IsMatch(carAppraisal.FirstRegistrationYear.ToString(), @"^(\d{4})/?0?([1-9]|1[012])$")) {
                }
                else
                {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", new string[] { "初年度登録年月" }));
                }
            }

            if (ModelState.IsValidField("RecycleDeposit") && carAppraisal.RecycleDeposit != null) {
                if (!Regex.IsMatch(carAppraisal.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RemainDebt") && carAppraisal.RemainDebt != null) {
                if (!Regex.IsMatch(carAppraisal.RemainDebt.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RemainDebt", MessageUtils.GetMessage("E0004", new string[] { "残債", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("CarTaxUnexpiredAmount") && carAppraisal.CarTaxUnexpiredAmount != null) {
                if (!Regex.IsMatch(carAppraisal.CarTaxUnexpiredAmount.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "自動車税種別割の残", "正の10桁以内の整数のみ" }));  //Mod 2019/09/04 yano #4011
                    //ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "自動車税の残", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("AppraisalPrice") && carAppraisal.AppraisalPrice != null) {
                if (!Regex.IsMatch(carAppraisal.AppraisalPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("AppraisalPrice", MessageUtils.GetMessage("E0004", new string[] { "査定価格", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && carAppraisal.Displacement != null) {
                if ((Regex.IsMatch(carAppraisal.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carAppraisal.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数10桁以内かつ少数2桁以内" }));
                }
            }

            //Add 2018/04/26 arc yano #3816 グレードコードが入力されている場合は、マスタチェックを行う    
            if (!string.IsNullOrWhiteSpace(carAppraisal.CarGradeCode))
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(carAppraisal.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("CarGradeCode", "入力したグレードコードは車両グレードマスタに登録されていません。マスタ登録を行ってから再度実行して下さい");
                }
            }

            return carAppraisal;
        }
        #endregion
    }
}
