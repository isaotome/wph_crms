using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using System.Data.Linq;            //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
using Crms.Models;                 //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarTransferController : InheritedController
    {
        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両移動入力";     // 画面名
        private static readonly string PROC_NAME = "車両移動登録"; // 処理名
        private static readonly string PROC_NAME_CONF = "入庫確定"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarTransferController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 車両移動検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両移動検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            SetCriteriaDataComponent(form);
            PaginatedList<Transfer> list = GetSearchResult(form);
            return View("CarTransferCriteria", list);
        }
        /// <summary>
        /// 検索画面のデータ付きコンポーネントを作成する
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        private void SetCriteriaDataComponent(FormCollection form) {
            CodeDao dao = new CodeDao(db);
            ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false), form["TransferType"], true);
            ViewData["DepartureLocationCode"] = form["DepartureLocationCode"];
            if (!string.IsNullOrEmpty(form["DepartureLocationCode"])) {
                ViewData["DepartureLocationName"] = new LocationDao(db).GetByKey(form["DepartureLocationCode"]).LocationName;
            }
            ViewData["ArrivalLocationCode"] = form["ArrivalLocationCode"];
            if (!string.IsNullOrEmpty(form["ArrivalLocationCode"])) {
                ViewData["ArrivalLocationName"] = new LocationDao(db).GetByKey(form["ArrivalLocationCode"]).LocationName;
            }
            ViewData["DepartureDateFrom"] = form["DepartureDateFrom"];
            ViewData["DepartureDateTo"] = form["DepartureDateTo"];
            ViewData["ArrivalDateFrom"] = form["ArrivalDateFrom"];
            ViewData["ArrivalDateTo"] = form["ArrivalDateTo"];
            ViewData["Vin"] = form["Vin"];
            ViewData["TransferConfirm"] = !string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true") ? true : false;
            ViewData["TransferUnConfirm"] = !string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true") ? true : false;
        }
        /// <summary>
        /// 指定した管理番号の移動履歴を表示する
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id) {
            
            List<Transfer> list = new TransferDao(db).GetBySalesCarNumber(id);
            CarPurchase purchase = new CarPurchaseDao(db).GetBySalesCarNumber(id);
            if (purchase != null) {
                Transfer trans = new Transfer();
                trans.TransferNumber = "仕入";
                trans.ArrivalDate = purchase.PurchaseDate;
                trans.ArrivalLocation = purchase.Location;

                //先頭に仕入を挿入
                list.Insert(0, trans);
            }
            return View("CarTransferCriteriaDialog", list);
        }
        /// <summary>
        /// 車両移動入力画面表示
        /// </summary>
        /// <param name="id">移動伝票番号</param>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            Transfer transfer = new TransferDao(db).GetByKey(id);
            if (transfer == null) {
                transfer = new Transfer();
                transfer.DepartureDate = DateTime.Today;
                transfer.ArrivalPlanDate = DateTime.Today;
                transfer.DepartureEmployee = (Employee)Session["Employee"];
            } else {
                ViewData["update"] = "1";
            }
            SetDataComponent(transfer);
            return View("CarTransferEntry",transfer);
        }

        /// <summary>
        /// 車両移動登録処理
        /// </summary>
        /// <param name="transfer">車両移動データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Transfer transfer,FormCollection form){

            // Mod 2014/11/14 arc yano 車両移動登録不具合 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            //車両在庫ロケーションを取得
            //if (salesCar!=null && string.IsNullOrEmpty(salesCar.LocationCode)) {
            //    ModelState.AddModelError("SalesCarNumber", "車両在庫が正しくないため移動できません");
            //}

            ValidateTransfer(transfer, form);

            if (!ModelState.IsValid) {
                SetDataComponent(transfer);
                return View("CarTransferEntry", transfer);
            }

            using (TransactionScope ts = new TransactionScope()) {
                Transfer target;
                if (form["update"] != null && form["update"].Equals("1")) {
                    target = new TransferDao(db).GetByKey(transfer.TransferNumber);
                    UpdateModel<Transfer>(target);
                    EditForUpdate(target);
                } else {
                    target = new Transfer();
                    target.TransferNumber = new SerialNumberDao(db).GetNewTransferNumber();
                    target.TransferType = transfer.TransferType;
                    target.ArrivalPlanDate = transfer.ArrivalPlanDate;
                    target.CarOrParts = "001";
                    target.DepartureDate = transfer.DepartureDate;
                    target.DepartureLocationCode = salesCar.LocationCode;
                    target.DepartureEmployeeCode = transfer.DepartureEmployeeCode;
                    target.ArrivalLocationCode = transfer.ArrivalLocationCode;
                    target.Quantity = 1;
                    target.SalesCarNumber = transfer.SalesCarNumber;
                    
                    EditForInsert(target);
                    
                    //2014/08/07 arc amii バグ対応 移動種別が未入力の場合、システムエラーで落ちるのを修正
                    //廃棄・棚卸ロスの場合、自動的に入庫確定し、在庫を落とす
                    if ("004".Equals(transfer.TransferType) || "005".Equals(transfer.TransferType))
                    {
                        target.ArrivalDate = transfer.DepartureDate;
                        salesCar.LocationCode = "";
                        salesCar.CarStatus = "";
                    }
                    
                    db.Transfer.InsertOnSubmit(target);
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        //Add 2014/08/07 arc amii エラーログ対応 ChangeConflictException発生時の処理追加
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            //他のユーザによって変更された値は無視され、当該クライアントからの変更を有効とする
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Mod 2014/08/07 arc amii エラーログ対応 Exception発生時、ログを出力するよう修正
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetDataComponent(transfer);
                            return View("CarTransferEntry", transfer);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/07 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, target.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                
                
            }
            SetDataComponent(transfer);
            ViewData["close"] = "1";
            return View("CarTransferEntry", transfer);
        }

        /// <summary>
        /// 入庫確定入力画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirm(string id)
        {
            Transfer transfer = new TransferDao(db).GetByKey(id);
            transfer.ArrivalDate = DateTime.Today;
            transfer.ArrivalEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            SetDataComponent(transfer, true);
            return View("CarTransferConfirm",transfer);
        }

        /// <summary>
        /// 入庫確定処理
        /// </summary>
        /// <param name="transfer">車両移動データ</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(Transfer transfer, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //Add 2017/07/18 arc nakayama #3778_車両移動データの編集＆削除機能追加　バリデーションチェックを外だしにする
            if (transfer.ActionType.Equals("Complete"))
            {
                ValidateTransfer(transfer, form);
                if (!ModelState.IsValid)
                {
                    SetDataComponent(transfer, true);
                    return View("CarTransferConfirm", transfer);
                }
            }

            using (TransactionScope ts = new TransactionScope()) {

                Transfer target = new TransferDao(db).GetByKey(transfer.TransferNumber);

                if (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Delete"))
                {
                    target.DelFlag = "1";
                    target.LastUpdateDate = DateTime.Now;
                    target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; 
                }
                else
                {
                    
                    //target.ArrivalEmployeeCode = transfer.ArrivalEmployeeCode;
                    //target.ArrivalDate = transfer.ArrivalDate;
                    //target.ArrivalLocationCode = transfer.ArrivalLocationCode;
                    UpdateModel(target);
                    EditForUpdate(target);

                    //車両マスタの在庫ロケーションを入庫ロケーションに更新
                    SalesCar car = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
                    car.LocationCode = transfer.ArrivalLocationCode;
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        //Add 2014/08/07 arc amii エラーログ対応 ChangeConflictException発生時の処理追加
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            //他のユーザによって変更された値は無視され、当該クライアントからの変更を有効とする
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Mod 2014/08/07 arc amii エラーログ対応 Exception発生時、ログを出力するよう修正
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ログに出力
                            OutputLogger.NLogError(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/07 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }
            SetDataComponent(transfer, true);
            ViewData["close"] = "1";
            return View("CarTransferConfirm", transfer);
        }
        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="transfer">車両移動データ</param>
        /// <history>
        /// 2018/06/26 arc yano #3908 車両移動　入庫確定処理を行うとシステムエラー
        /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない
        /// </history>
        private void ValidateTransfer(Transfer transfer,FormCollection form) {

            //共通のチェック
            CommonValidate("DepartureDate", "出庫日", transfer, true);
            CommonValidate("ArrivalPlanDate", "入庫予定日", transfer, true);
            //CommonValidate("DepartureLocationCode", "出庫ロケーション", transfer, true);
            CommonValidate("SalesCarNumber", "管理番号", transfer, true);
            CommonValidate("DepartureEmployeeCode", "出庫担当者",transfer, true);
            CommonValidate("TransferType", "移動種別", transfer, true);
            CommonValidate("ArrivalLocationCode", "入庫ロケーション", transfer, true);

            //車両在庫ロケーションを取得
            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            if (salesCar != null && string.IsNullOrEmpty(salesCar.LocationCode))
            {
                ModelState.AddModelError("SalesCarNumber", "車両在庫が正しくないため移動できません");
            }

            //Add 2017/02/07 arc nakayama #3045_車両移動に関して複数回移動できてしまう
            if ((form["update"] != null) || (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Complete")) && !string.IsNullOrEmpty(transfer.SalesCarNumber))
            {
                List<Transfer> TranDataList = new List<Transfer>();

                if (string.IsNullOrEmpty(transfer.TransferNumber))
                {
                    TranDataList = new TransferDao(db).GetBySalesCarNumber(transfer.SalesCarNumber).Where(x => x.TransferType.Equals("001")).ToList();
                }
                else
                {
                    TranDataList = new TransferDao(db).GetBySalesCarNumber(transfer.SalesCarNumber).Where(x => x.TransferType.Equals("001") && !x.TransferNumber.Equals(transfer.TransferNumber)).ToList();
                }


                bool TransferFlag = false;

                foreach (var TranData in TranDataList)
                {
                    if (TranData.ArrivalDate == null)
                    {
                        TransferFlag = true;
                        break;
                    }
                }

                if (TransferFlag)
                {
                    ModelState.AddModelError("SalesCarNumber", "該当の車両は現在移動中です。");
                }
            }


            //SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            if (salesCar != null && !string.IsNullOrEmpty(salesCar.LocationCode))
            {

                //Mod 2018/06/26 arc yano #3908
                Location loc = new LocationDao(db).GetByKey(salesCar.LocationCode);
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, loc != null ? loc.WarehouseCode : "");

                //Mod 2018/04/10 arc yano #3879
                //List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, salesCar.Location.WarehouseCode);

                foreach (var a in dDep)
                {
                    //string departmentCode = new LocationDao(db).GetByKey(salesCar.LocationCode).DepartmentCode;
                    // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("DepartureDate", "月次締め処理が終了しているので指定された出庫日では出庫できません");

                        break;
                    }
                }
                
            }

            //入庫確定時のチェック
            if (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Complete"))
            {
                CommonValidate("ArrivalEmployeeCode", "入庫担当者", transfer, true);
                CommonValidate("ArrivalDate", "入庫日", transfer, true);

                //締め日チェック
                if (!string.IsNullOrEmpty(transfer.ArrivalLocationCode) && transfer.ArrivalDate != null)
                {
                    Location LocationData = new LocationDao(db).GetByKey(transfer.ArrivalLocationCode);

                    // Mod 2018/04/10 arc yano #3879
                    // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする

                    List<Department> DepList = CommonUtils.GetDepartmentFromWarehouse(db, LocationData.WarehouseCode);
                    foreach (var a in DepList)
                    {
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                        {
                            ModelState.AddModelError("ArrivalDate", "月次締め処理が終了しているため指定された入庫日では入庫できません");

                            break;
                        }
                    }

                    if (!new InventoryScheduleCarDao(db).IsClosedInventoryMonth(LocationData.WarehouseCode, transfer.ArrivalDate))
                    {

                        ModelState.AddModelError("ArrivalDate", "車両棚卸が終了しているため指定された入庫日では入庫できません");
                    }

                }
            }

        }
        /// <summary>
        /// 車両移動検索結果を取得する
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>車両移動データリスト</returns>
        private PaginatedList<Transfer> GetSearchResult(FormCollection form) {
            Transfer condition = new Transfer();
            condition.DepartureDateFrom = CommonUtils.StrToDateTime(form["DepartureDateFrom"],DaoConst.SQL_DATETIME_MIN);
            condition.DepartureDateTo = CommonUtils.StrToDateTime(form["DepartureDateTo"], DaoConst.SQL_DATETIME_MAX);
            condition.DepartureLocationCode = form["DepartureLocationCode"];
            condition.ArrivalDateFrom = CommonUtils.StrToDateTime(form["ArrivalDateFrom"], DaoConst.SQL_DATETIME_MIN);
            condition.ArrivalDateTo = CommonUtils.StrToDateTime(form["ArrivalDateTo"], DaoConst.SQL_DATETIME_MAX);
            condition.ArrivalLocationCode = form["ArrivalLocationCode"];
            condition.CarOrParts = "1";
            condition.SalesCar = new SalesCar();
            condition.SalesCar.Vin = form["Vin"];
            condition.TransferType = form["TransferType"];
            if (!string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true")) {
                condition.TransferConfirm = true;
            } else {
                condition.TransferConfirm = false;
            }
            if (!string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true")) {
                condition.TransferUnConfirm = true;
            } else {
                condition.TransferUnConfirm = false;
            }
            condition.SetAuthCondition((Employee)Session["Employee"]);
            return new TransferDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// データ付き画面コンポーネント
        /// </summary>
        /// <param name="trans">車両移動データ</param>
        /// <history>
        /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない
        /// 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
        /// </history>
        private void SetDataComponent(Transfer trans, bool ConfirmFlag = false)
        {
            CodeDao dao = new CodeDao(db);
            if (ConfirmFlag)
            {
                ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false).Where(x => x.Code.Equals("001")).ToList<c_TransferType>(), trans.TransferType, false);
            }
            else
            {
                ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false).Where(x => x.Code.Equals("001") || x.Code.Equals("004") || x.Code.Equals("005")).ToList<c_TransferType>(), trans.TransferType, false);
            }
            trans.DepartureEmployee = new EmployeeDao(db).GetByKey(trans.DepartureEmployeeCode);
            trans.DepartureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);
            trans.ArrivalEmployee = new EmployeeDao(db).GetByKey(trans.ArrivalEmployeeCode);
            trans.ArrivalLocation = new LocationDao(db).GetByKey(trans.ArrivalLocationCode);
            if (trans.SalesCar==null && !string.IsNullOrEmpty(trans.SalesCarNumber)) {
                trans.SalesCar = new SalesCarDao(db).GetByKey(trans.SalesCarNumber);
            }
            // Mod 2018/04/10 arc yano #3879
            // Mod 2015/04/15 arc yano
            //移動元部門の締めチェック
            if(trans.DepartureLocation != null)
            {
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, trans.DepartureLocation.WarehouseCode);

                foreach (var a in dDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["ClosedMonth"] = "1";
                    }
                }

            }
            //移動先部門の締めチェック
            if (trans.ArrivalLocation != null)
            {
                List<Department> aDep = CommonUtils.GetDepartmentFromWarehouse(db, trans.ArrivalLocation.WarehouseCode);

                foreach (var a in aDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["ClosedMonth"] = "1";
                    }
                }
            }

            /*
            if ((trans.DepartureLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(trans.DepartureLocation.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode)) ||
                (trans.ArrivalLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(trans.ArrivalLocation.DepartmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode)))
            {
                ViewData["ClosedMonth"] = "1";
            }
            */
        }

        /// <summary>
        /// INSERT用データ作成
        /// </summary>
        /// <param name="transfer"></param>
        private void EditForInsert(Transfer transfer) {
            
            transfer.CreateDate = DateTime.Now;
            transfer.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            EditForUpdate(transfer);
        }

        /// <summary>
        /// UPDATE用データ作成
        /// </summary>
        /// <param name="transfer"></param>
        /// <history>
        /// 2020/06/19 yano #4054 【車両移動】移動済伝票を修正時の不具合対応
        /// </history>
        private void EditForUpdate(Transfer transfer) {
            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            
            transfer.CarOrParts = "1";
            transfer.Quantity = 1;
            transfer.LastUpdateDate = DateTime.Now;
            transfer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //Mod 2020/06/19 yano #4054
            if (transfer.ArrivalDate == null)   //入庫完了していない場合
            {
                transfer.DepartureLocationCode = salesCar.LocationCode;
            }

            transfer.DelFlag = "0";
        }

        /// <summary>
        /// バリデーションチェック
        /// </summary>
        /// <param name="transfer"></param>
        /// <history>
        /// 2018/04/10 arc yano #3879 車両伝票　ロケーションマスタに部門コードを設定していないと、納車処理を行えない
        /// 2017/07/18 arc nakayama #3778_車両移動データの編集＆削除機能追加
        /// </history>
        private void ConfirmValidation(Transfer transfer)
        {
            CommonValidate("ArrivalLocationCode", "入庫ロケーション", transfer, true);
            CommonValidate("ArrivalEmployeeCode", "入庫担当者", transfer, true);
            CommonValidate("ArrivalDate", "入庫日", transfer, true);

            //締め日チェック
            if (!string.IsNullOrEmpty(transfer.ArrivalLocationCode) && transfer.ArrivalDate != null)
            {
                Location LocationData = new LocationDao(db).GetByKey(transfer.ArrivalLocationCode);
                
                // Mod 2018/04/10 arc yano #3879    
                // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, (LocationData != null ? LocationData.WarehouseCode : ""));

                foreach (var a in dDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("ArrivalDate", "月次締め処理が終了しているため指定された入庫日では入庫できません");
                        break;
                    }
                }

                if(!new InventoryScheduleCarDao(db).IsClosedInventoryMonth(LocationData.WarehouseCode, transfer.ArrivalDate)){

                    ModelState.AddModelError("ArrivalDate", "車両棚卸が終了しているため指定された入庫日では入庫できません");
                }

            }

            return;
        }


    }
}
