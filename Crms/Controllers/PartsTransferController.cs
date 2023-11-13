
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.Linq;                 //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.SqlClient;            //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 部品移動
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsTransferController : Controller
    {
        //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "部品移動入力";     // 画面名
        private static readonly string PROC_NAME = "入庫確定"; // 処理名
        private static readonly string PROC_NAME_DISP = "部品移動入力画面表示"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsTransferController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            return Criteria(form);
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>部品移動検索結果画面</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;
            PaginatedList<Transfer> list = GetSearchResultList(form);
            SetCriteriaDataComponent(form);
            return View("PartsTransferCriteria", list);
        }

        /// <summary>
        /// 部品移動入力画面初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry(string id) {
            Transfer trans = new Transfer();
            trans.DepartureDate = DateTime.Today;
            trans.DepartureEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            trans.ArrivalPlanDate = DateTime.Today;
            SetDataComponent(trans);
            return View("PartsTransferEntry", trans);
        }



        /// <summary>
        /// 部品移動入力画面表示
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Transfer trans,FormCollection form)
        {
            //2017/02/03 arc nakayama #3636_部品移動入力　出庫ロケーションの在庫数のvalidationチェックの追加　引数追加
            ValidatePartsTransfer(trans, form);
            if (!ModelState.IsValid) {
                SetDataComponent(trans);
                return View("PartsTransferEntry", trans);
            }

            // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //移動データを作成し、在庫を更新する
                new TransferDao(db).InsertTransfer(trans, ((Employee)Session["Employee"]).EmployeeCode);
                //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME_DISP, FORM_NAME, "");

                        SetDataComponent(trans);
                        return View("PartsTransferEntry", trans);
                    }
                    else
                    {
                        //Mod 2014/08/08 arc amii エラーログ対応 『theow e』からログ出力処理に変更
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_DISP, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    //Add 2014/08/08 arc amii エラーログ対応 上記外のExceptionの場合、ログ出力を行う
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_DISP, FORM_NAME, "");
                    return View("Error");
                }
                
            }
            SetDataComponent(trans);
            ViewData["close"] = "1";
            return View("PartsTransferEntry",trans);
        }

        /// <summary>
        /// 入庫確定画面を表示する
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirm(string id)
        {
            Transfer trans = new TransferDao(db).GetByKey(id);
            trans.ArrivalDate = DateTime.Today;
            trans.ArrivalEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            SetDataComponent(trans);
            return View("PartsTransferConfirm",trans);
        }

        /// <summary>
        /// 入庫確定処理
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(Transfer trans,FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 車両移動登録不具合 類似対応 データコンテキスト生成処理をアクションリザルトの最初に移動
            // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //Validationチェック
            if(string.IsNullOrEmpty(form["ArrivalLocationCode"])){
                ModelState.AddModelError("ArrivalLocationCode",MessageUtils.GetMessage("E0001","入庫ロケーション"));
            }
            if(string.IsNullOrEmpty(form["ArrivalEmployeeCode"])){
                ModelState.AddModelError("ArrivalEmployeeCode",MessageUtils.GetMessage("E0001","入庫担当者"));
            }
            if (string.IsNullOrEmpty(form["ArrivalDate"])) {
                ModelState.AddModelError("ArrivalDate", MessageUtils.GetMessage("E0001", "入庫日"));
            }
            if (!ModelState.IsValidField("ArrivalDate")) {
                ModelState.AddModelError("ArrivalDate", MessageUtils.GetMessage("E0003", "入庫日"));
                if (ModelState["ArrivalDate"].Errors.Count > 1) {
                    ModelState["ArrivalDate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("ArrivalDate") && trans.ArrivalDate != null) {

                //Mod 2016/08/13 arc yano #3596
                //ロケーション→倉庫を割出す
                //string departmentCode = new LocationDao(db).GetByKey(trans.ArrivalLocationCode).DepartmentCode;       
                string warehouseCode = new LocationDao(db).GetByKey(trans.ArrivalLocationCode).WarehouseCode;
                
                //対象の倉庫を使用している部門の一覧を取得する
                DepartmentWarehouse condition = new DepartmentWarehouse();
                condition.WarehouseCode = warehouseCode;

                List<DepartmentWarehouse> dWarehouseList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                // Mod 2015/04/20 arc yano 仮締中編集権限の有り／無しで判定を変更する。
                //Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)

                int ret = 0;

                //ret = CheckTempClosedEdit(departmentCode, trans.ArrivalDate);
                ret = CheckTempClosedEdit(dWarehouseList, trans.ArrivalDate);   //Mod 2016/08/13 arc yano #3596
                    
                switch (ret)
                {
                    case 1:
                        ModelState.AddModelError("ArrivalDate", "月次締め処理が終了しているので指定された入庫日では入庫できません");
                        break;

                    case 2:
                        ModelState.AddModelError("ArrivalDate", "部品棚卸処理が終了しているので指定された入庫日では入庫できません");
                        break;
                    
                    default:
                        //何もしない
                        break;
                }

                /*
                //--経理締判定
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("ArrivalDate", "月次締め処理が終了しているので指定された入荷日では入荷できません");
                }
                else //--部品棚卸判定
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(departmentCode, trans.ArrivalDate, "002"))
                {
                            ModelState.AddModelError("ArrivalDate", "部品棚卸処理が終了しているので指定された入庫日では入庫できません");
                        }
                }
            }
                */
            }
            if (!ModelState.IsValid) {
                ViewData["TransferTypeName"] = form["TransferTypeName"];
                SetDataComponent(trans);
                return View("PartsTransferConfirm", trans);
            }

            Transfer target = new TransferDao(db).GetByKey(form["TransferNumber"]);
            if (target != null) {

                //移動伝票の入庫情報を更新
                target.ArrivalLocationCode = form["ArrivalLocationCode"];
                target.ArrivalEmployeeCode = form["ArrivalEmployeeCode"];
                target.ArrivalDate = DateTime.Parse(form["ArrivalDate"]);
                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //Mod 2017/02/08 arc yano #3620
                //入庫ロケーションの在庫数量更新(削除済データも取得する)
                PartsStock stock = new PartsStockDao(db).GetByKey(form["PartsNumber"], form["ArrivalLocationCode"], true);
                if (stock == null) {
                    stock = new PartsStock();
                    stock.CreateDate = DateTime.Now;
                    stock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    stock.DelFlag = "0";
                    stock.LastUpdateDate = DateTime.Now;
                    stock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    stock.LocationCode = target.ArrivalLocationCode;
                    stock.PartsNumber = target.PartsNumber;
                    stock.Quantity = target.Quantity;
                    db.PartsStock.InsertOnSubmit(stock);

                } else {

                    //削除データの場合は初期化
                    stock = new PartsStockDao(db).InitPartsStock(stock);      //Add 2017/02/08 arc yano #3620

                    stock.Quantity = stock.Quantity + target.Quantity;
                }

                //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        break;
                    }
                    catch (ChangeConflictException e)
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
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii エラーログ対応 ログ出力処理追加
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            SetDataComponent(trans);
                            return View("PartsTransferConfirm", trans);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii エラーログ対応 『theow e』からログ出力処理に変更
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }
            SetDataComponent(trans);
            ViewData["close"] = "1";
            return View("PartsTransferConfirm", trans);
        }
        /// <summary>
        /// 検索画面のデータ付きコンポーネントを作成する
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <history>
        /// 2016/06/20 arc yano #3584 検索条件(受注伝票番号)追加
        /// </history>
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
            ViewData["TransferConfirm"] = !string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true") ? true : false;
            ViewData["TransferUnConfirm"] = !string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true") ? true : false;

            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                try { ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName; } catch (NullReferenceException) { }
            }

            ViewData["SlipNumber"] = form["SlipNumber"];    //Add 2016/06/20 arc yano #3584

        }
        /// <summary>
        /// データ付きコンポーネントの作成
        /// </summary>
        /// <param name="trans"></param>
        /// <history>
        /// 2016/06/20 arc yano #3583 部品移動入力　移動種別の絞込み
        /// </history>
        private void SetDataComponent(Transfer trans) {
            CodeDao dao = new CodeDao(db);
            ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false, "1"), trans.TransferType, true);    //Mod 2016/06/20 arc yano #3583

            trans.DepartureEmployee = new EmployeeDao(db).GetByKey(trans.DepartureEmployeeCode);
            trans.DepartureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);
            trans.ArrivalEmployee = new EmployeeDao(db).GetByKey(trans.ArrivalEmployeeCode);
            trans.ArrivalLocation = new LocationDao(db).GetByKey(trans.ArrivalLocationCode);

            Parts parts = new PartsDao(db).GetByKey(trans.PartsNumber);
            if (parts != null) {
                ViewData["PartsNameJp"] = parts.PartsNameJp;
            }
            PartsStock departureStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.DepartureLocationCode);
            ViewData["DepartureStockQuantity"] = 0;
            if (departureStock != null) {
                ViewData["DepartureStockQuantity"] = departureStock.Quantity;
            }
            PartsStock arrivalStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.ArrivalLocationCode);
            ViewData["ArrivalStockQuantity"] = 0;
            if (arrivalStock != null) {
                ViewData["ArrivalStockQuantity"] = arrivalStock.Quantity;
            }
            if (trans.c_TransferType != null) {
                ViewData["TransferTypeName"] = trans.c_TransferType.Name;
            }
        }
        /// <summary>
        /// 部品移動データを検索して結果リストを返す
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ロケーションの絞込み条件を変更(部門→ロケーションではなく、部門→倉庫→ロケーションに変更)
        /// 2016/06/20 arc yano #3584 部品移動一覧　検索条件の追加
        /// </history>
        private PaginatedList<Transfer> GetSearchResultList(FormCollection form) {

            Transfer condition = new Transfer();
            condition.PartsNumber = form["PartsNumber"];
            condition.TransferType = form["TransferType"];
            condition.DepartureLocationCode = form["DepartureLocationCode"];
            condition.ArrivalLocationCode = form["ArrivalLocationCode"];
            DateTime departureDateFrom;
            if (!string.IsNullOrEmpty(form["DepartureDateFrom"])) {
                if (DateTime.TryParse(form["DepartureDateFrom"], out departureDateFrom)) {
                    condition.DepartureDateFrom = departureDateFrom;
                } else {
                    condition.DepartureDateFrom = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime departureDateTo;
            if (!string.IsNullOrEmpty(form["DepartureDateTo"])) {
                if (DateTime.TryParse(form["DepartureDateTo"], out departureDateTo)) {
                    condition.DepartureDateTo = departureDateTo;
                } else {
                    condition.DepartureDateTo = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime arrivalDateFrom;
            if (!string.IsNullOrEmpty(form["ArrivalDateFrom"])) {
                if (DateTime.TryParse(form["ArrivalDateFrom"], out arrivalDateFrom)) {
                    condition.ArrivalDateFrom = arrivalDateFrom;
                } else {
                    condition.ArrivalDateFrom = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime arrivalDateTo;
            if (!string.IsNullOrEmpty(form["ArrivalDateTo"])) {
                if (DateTime.TryParse(form["ArrivalDateTo"], out arrivalDateTo)) {
                    condition.ArrivalDateTo = arrivalDateTo;
                } else {
                    condition.ArrivalDateTo = DaoConst.SQL_DATETIME_MAX;
                }
            }
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
            condition.CarOrParts = "2";

            //Mod 2016/08/13 arc yano #3596
            //部門コードから倉庫を取得
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                condition.WarehouseCode = dWarehouse.WarehouseCode;
            }
            //condition.DepartmentCode = form["DepartmentCode"];

            condition.SetAuthCondition((Employee)Session["Employee"]);

            //伝票番号
            condition.SlipNumber = form["SlipNumber"];  //Add 2016/06/20 arc yano #3584

            PaginatedList<Transfer> list = new TransferDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return list;
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="trans">部品移動データ</param>
        /// <history>
        /// 2017/02/03 arc nakayama #3636_部品移動入力　出庫ロケーションの在庫数のvalidationチェックの追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/04/20 arc yano 部品系のチェックは経理締、部品棚卸それぞれで締判定処理を行う。また経理締判定では、仮締中変更可能なユーザの場合、仮締めの場合でも、変更可能とする。
        /// 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
        /// </history>
        private void ValidatePartsTransfer(Transfer trans, FormCollection form)
        {
            if(string.IsNullOrEmpty(trans.DepartureLocationCode)){
                ModelState.AddModelError("DepartureLocationCode",MessageUtils.GetMessage("E0001","出庫ロケーションコード"));
            }
            if(string.IsNullOrEmpty(trans.DepartureEmployeeCode)){
                ModelState.AddModelError("DepartureEmployeeCode",MessageUtils.GetMessage("E0001","出庫担当者"));
            }
            if (!ModelState.IsValidField("DepartureDate")) {
                ModelState.AddModelError("DepartureDate", MessageUtils.GetMessage("E0003", "出庫日"));
                if (ModelState["DepartureDate"].Errors.Count > 1) {
                    ModelState["DepartureDate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("DepartureDate") && trans.DepartureDate != null) {
                Location departureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);

                //Mod 2015/04/20 arc yano
                //Mod 2015/02/03 arc nakayama

                int ret = 0;

                if (departureLocation != null)
                {
                    //Mod 2016/08/13 arc yano #3596
                    //ロケーションから倉庫コードを取得する
                    string warehouseCode = departureLocation.WarehouseCode;
                    //倉庫コードから使用している部門を取得する
                    DepartmentWarehouse condition = new DepartmentWarehouse();
                    condition.WarehouseCode = warehouseCode;

                    List<DepartmentWarehouse> dWarehouseList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                    if (dWarehouseList != null)
                    {
                        //ret = CheckTempClosedEdit(departureLocation.DepartmentCode, trans.DepartureDate);
                        ret = CheckTempClosedEdit(dWarehouseList, trans.DepartureDate);
                    }
                    
                }

                switch (ret)
                {
                    case 1:
                        ModelState.AddModelError("ArrivalDate", "月次締め処理が終了しているので指定された出庫日では出庫できませんん");
                        break;

                    case 2:
                        ModelState.AddModelError("ArrivalDate", "部品棚卸処理が終了しているので指定された出庫日では出庫できません");
                        break;

                    default:
                        //何もしない
                        break;
                }

                /*
                //--経理締判定
                if (departureLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(departureLocation.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("DepartureDate", "経理締処理が終了しているので指定された出庫日では出庫できません");
                }
                else //--部品棚卸判定
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                        if (departureLocation != null && !new InventorySchedulePartsDao(db).IsClosedInventoryMonth(departureLocation.DepartmentCode, trans.DepartureDate, "002"))
                        {
                    ModelState.AddModelError("DepartureDate", "棚卸締め処理が終了しているので指定された出庫日では出庫できません");
                }
            }
                }
                */
                
            }
            if (string.IsNullOrEmpty(trans.ArrivalLocationCode)) {
                ModelState.AddModelError("ArrivalLocationCode", MessageUtils.GetMessage("E0001", "入庫ロケーションコード"));
            }
            if (!ModelState.IsValidField("ArrivalPlanDate")) {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0003", "入庫予定日"));
                if (ModelState["ArrivalPlanDate"].Errors.Count > 1) {
                    ModelState["ArrivalPlanDate"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(trans.TransferType)) {
                ModelState.AddModelError("TransferType", MessageUtils.GetMessage("E0001", "移動種別"));
            }
            if (string.IsNullOrEmpty(trans.PartsNumber)) {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", "部品番号"));
            }
            if (trans.Quantity == 0) {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "0以外の正の整数7桁以内かつ小数2桁以内" }));
            }
            //フォーマットチェック
            if (!ModelState.IsValidField("Quantity")) {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" }));
                if (ModelState["Quantity"].Errors.Count > 1) {
                    ModelState["Quantity"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("Quantity") &&
                (Regex.IsMatch(trans.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                        || (Regex.IsMatch(trans.Quantity.ToString(), @"^\d{1,7}$")))) {
            } else {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" }));
                if (ModelState["Quantity"].Errors.Count > 1) {
                    ModelState["Quantity"].Errors.RemoveAt(0);
                }
            }

            //2017/02/03 arc nakayama #3636_部品移動入力　出庫ロケーションの在庫数のvalidationチェックの追加 移動数量 ＞　出庫元のフリーの在庫数の場合エラー
            decimal? StockQuantity = decimal.Parse(form["StockQuantity"]);
            if (trans.Quantity > StockQuantity)
            {
                ModelState.AddModelError("Quantity", "出庫数量が在庫数を超えています。出庫数量は在庫数以下にしてください");
            }
        }

        /// <summary>
        /// 仮締中編集権限有／無による締めチェック処理
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">対象日付</param>
        /// <return>チェック結果(0:編集可 1:編集不可(月次締済) 2:編集不可(部品棚卸済))</return>
        /// <history>
        /// 2021/11/09 yano #4111【サービス伝票入力】仮締中の納車済伝票の納車日編集チェック処理の不具合
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数を変更(departmentCode → dWarehouseList)
        /// 2015/05/07 arc yano 仮締中編集権限追加 仮締中変更可能なユーザの場合、仮締めの場合でも、変更可能とする
        /// </history>
        private int CheckTempClosedEdit(List<DepartmentWarehouse> dWarehouseList, DateTime? targetDate)
        {
            string securityCode = ((Employee)Session["Employee"]).SecurityRoleCode;         //セキュリティコード
            bool editFlag = false;                                                          //仮締中編集権限

            int ret = 0;                                                                    //編集状況

            //仮締中編集権限を取得
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(securityCode, "EditTempClosedData");

            if (rec != null)
            {
                editFlag = rec.EnableFlag;
            }

            //候補となる部門全てでチェックを行い、一つでも該当するものがあればそれを採用する
            foreach (var dw in dWarehouseList)
            {
                //編集権限ありの場合
                if (editFlag == true)
                {
                    //月次締のみチェック   ※部門単位でチェック
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(dw.DepartmentCode, targetDate, "001", securityCode))
                    {
                        ret = 1;
                        break;      //処理を抜ける
                    }
                }
                else
                {
                    //部品棚卸チェック 　※倉庫単位でチェック
                    if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, targetDate, "002"))
                    {
                        ret = 2;
                        break;  //Add 2021/11/09 yano #4111
                    }
                    else //Mod 2021/11/09 yano #4111
                    {
                        //月次締チェック   ※部門単位でチェック
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(dw.DepartmentCode, targetDate, "001", securityCode))
                        {
                            ret = 1;
                            break;      //処理を抜ける
                        }
                    }
                }
            }           

            return ret;
        }
    }
}
