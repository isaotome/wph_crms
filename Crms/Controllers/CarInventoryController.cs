using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using Crms.Models;
using System.Data.Linq;
using System.Transactions;
using OfficeOpenXml;
using System.Configuration;
using System.Data;


namespace Crms.Controllers
{

    /// <summary>
    /// 車両棚卸機能コントローラクラス
    /// </summary>
    /// <history>
    /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarInventoryController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarInventoryController()
        {
            db = new CrmsLinqDataContext();
        }

        private static readonly string STS_UNEXCUTED   = "000";                         // 未実施(棚卸ステータス)
        private static readonly string STS_INACTION    = "001";                         // 実施中(棚卸ステータス)
        private static readonly string STS_TMPDECIDED  = "002";                         // 仮確定(棚卸ステータス)
        private static readonly string STS_DECIDED     = "003";                         // 本確定(棚卸ステータス)
        private static readonly string STS_INVALID     = "999";                         // ステータスエラー(棚卸ステータス)

        private static readonly string CATEGORY_CARSTOCK = "020";                       // カテゴリコード(在庫区分)

        private static readonly string FORM_NAME = "車両棚卸";                          // 画面名
        private static readonly string PROC_NAME_SEARCH = "検索処理";                   // 処理名(検索処理)
        private static readonly string PROC_NAME_INVSTART = "棚卸開始";                 // 処理名(棚卸開始)
        private static readonly string PROC_NAME_EXCELDOWNLOAD = "棚卸表出力";          // 処理名(棚卸表出力)
        private static readonly string PROC_NAME_EXCELIMPORT = "棚卸表取込";            // 処理名(棚卸表取込)
        private static readonly string PROC_NAME_TEMPSTORED = "一時保存";               // 処理名(一時保存)
        private static readonly string PROC_NAME_INVENTORY_TMPDECIDED = "棚卸仮確定";   // 処理名(棚卸仮確定)
        private static readonly string PROC_NAME_INVENTORY_DECIDED = "棚卸確定";        // 処理名(棚卸確定)
        private static readonly string PROC_NAME_INVENTORY_CANCEL = "棚卸仮確定取消";   // 処理名(棚卸仮確定取消)  //2018/08/01 yano #3926

        private static readonly string DESCRIPTION_EXCELDOWNLOAD = "( リストに存在しない車両はここから下に入力 管理番号・車台番号を入力してください。管理番号がわからない場合には必ず車台番号を入力してください。車台番号は車両が一意になる桁数まで入力してください。 )";            // 説明文

        /// <summary>
        /// 車両棚卸画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult Criteria()
        {
            //-----------------------
            //初期値の設定
            //-----------------------
            criteriaInit = true;                            //初期表示フラグON　
            FormCollection form = new FormCollection();     //フォーム生成

            //処理種別
            form["RequestFlag"] = "";
            //棚差有無チェック
            form["DiffQuantity"] = "false";     //全て表示

            //表示ページ数
            form["id"] = "0";

            //対象年月、棚卸作業日
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDateDao(db).GetAllVal();

            //検索結果(フォーム値)
            EntitySet<CarInventory> line = new EntitySet<CarInventory>();

            //検索結果
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //部門(ログインユーザの部門が車両棚卸対象の部門の場合は部門コードを設定する)
            Department department = new DepartmentDao(db).GetByCarInventory(((Employee)Session["Employee"]).DepartmentCode);
            if (department != null)
            {
                form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            }
            else
            {
                form["DepartmentCode"] = "";
            }

            //倉庫の取得
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                form["WarehouseCode"] = dWarehouse.WarehouseCode;
            }
            else
            {
                form["WarehouseCode"] = "";
            }

            //-------------------
            //棚卸確定(仮)
            //-------------------
            ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "CarInventoryTempDecided");
            if (ret.EnableFlag == true)
            {
                form["InventoryTempDecidedVisible"] = bool.TrueString.ToLower();       //棚卸仮確定ボタン押下できる
            }
            else
            {
                form["InventoryTempDecidedVisible"] = bool.FalseString.ToLower();      //棚卸仮確定ボタン押下できない
            }

            //-------------------
            //棚卸確定
            //-------------------
            ApplicationRole ret2 = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "CarInventoryDecided");
            if (ret2.EnableFlag == true)
            {
                form["InventoryDecidedVisible"] = bool.TrueString.ToLower();           //棚卸確定ボタン押下できる
            }
            else
            {
                form["InventoryDecidedVisible"] = bool.FalseString.ToLower();          //棚卸確定ボタン押下できない
            }

            //--------------------
            //棚卸基準日
            //--------------------
            if (WorkingDate != null)
            {
                form["InventoryWorkingDate"] = string.Format("{0:yyyy/MM/dd}", WorkingDate.InventoryWorkingDate);

                //対象年月がnullの場合は表示しない
                if (WorkingDate.InventoryMonth != null)
                {
                    form["InventoryMonth"] = string.Format("{0:yyyy/MM}", WorkingDate.InventoryMonth);
                }
                else
                {
                    form["InventoryMonth"] = "";
                    ModelState.AddModelError("", "棚卸基準日が登録されていません");
                    SetComponent(form, list);
                    return View("CarInventoryCriteria", new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
                }
            }
            else
            {
                form["InventoryMonth"] = "";
                form["InventoryWorkingDate"] = "";
                ModelState.AddModelError("", "棚卸基準日が登録されていません");
                SetComponent(form, list);
                return View("CarInventoryCriteria", new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }
            //----------------
            //棚卸開始日時
            //----------------
            InventoryScheduleCar rec = new InventoryScheduleCarDao(db).GetByKey(form["WarehouseCode"], (DateTime)WorkingDate.InventoryMonth);

            if (rec != null)
            {
                form["InventoryStartDate"] = rec.StartDate == null ? "" : string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
            }

            return Criteria(form, line);
        }

        /// <summary>
        /// 棚卸画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/01 yano #3926 車両棚卸　棚卸確定取消ボタン追加
        /// 2017/09/07 arc yano #3784 車両在庫棚卸　画面リスト出力機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, EntitySet<CarInventory> line)
        {
            ActionResult ret;

            db = new CrmsLinqDataContext();

            //タイムアウト値の設定
            db.CommandTimeout = 600;

            db.Log = new OutputWriter();

            //検索結果
            PaginatedList<CarInventory> list = ((line == null) ? new PaginatedList<CarInventory>() : new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));

            //---------------------------------
            //部門から倉庫の設定
            //---------------------------------
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                //倉庫コード
                form["WarehouseCode"] = dWarehouse.WarehouseCode;
                //倉庫名
                form["WarehouseName"] = dWarehouse.Warehouse.WarehouseName;
            }
            else
            {
                //倉庫コード
                form["WarehouseCode"] = "";
                //倉庫名
                form["WarehouseName"] = "";
            }

            //棚卸対象月の取得(Datetime型)
            DateTime inventoryMonth;

            if (false == DateTime.TryParse(form["InventoryMonth"] + "/01", out inventoryMonth))
            {
                ModelState.AddModelError("", "棚卸基準日が登録されていないため、棚卸を開始できません");
                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            //ReuquestFlagによる処理の振分け
            switch (form["RequestFlag"])
            {
                case "1":   //棚卸開始
                    ret = InventoryStart(form, line, inventoryMonth);
                    break;

                case "2":   //棚卸表出力
                case "5":   //画面リスト出力
                    ret = Download(form, line, inventoryMonth);
                    break;

                case "3":   //棚卸作業結果一時保存
                    ret = TemporalilyStored(form, line, inventoryMonth);
                    break;

                case "4":   //棚卸仮確定
                    ret = InventoryTempDecided(form, line, inventoryMonth);
                    break;

                case "6":   //棚卸仮確定取消
                    ret = InventoryCancel(form, line, inventoryMonth);
                    break;

                case "11":   //行追加
                    ret = AddLine(form, line);
                    break;

                case "12":   //行削除
                    ret = DelLine(form, line);
                    break;

                default:    //検索またはページング
                    //ページング
                    if (form["RequestFlag"].Equals("999"))
                    {
                        if (string.IsNullOrWhiteSpace(form["DepartmentCode"]))
                        {
                            ModelState.AddModelError("", "部門コードを入力してください");
                            SetComponent(form, list);
                            return View("CarInventoryCriteria", list);
                        }
                    }

                    //検索処理実行
                    ret = SearchList(form, line, inventoryMonth);
                    break;
            }

            //コントロールの有効無効
            GetViewResult(form);

            return ret;
        }
        #region  検索処理
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        private ActionResult SearchList(FormCollection form, EntitySet<CarInventory> line , DateTime inventoryMonth)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ModelState.Clear();

            //検索結果初期化
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //倉庫毎棚卸状況をチェック
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            // 検索結果リストの取得
            if ((criteriaInit == true) && ((status == STS_UNEXCUTED || status == STS_INVALID)))    //初期表示時、かつ棚卸未実施の場合
            {
                //何もしない
            }
            else
            {
                //検索処理
                list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }

            //画面設定
            SetComponent(form, list);

            return View("CarInventoryCriteria", list);
        }

        /// <summary>
        /// 検索実行
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸一覧</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private IQueryable<CarInventory> GetSearchResultList(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            CarInventory InventoryStockCondition = SetCondition(form, inventoryMonth);

            //検索実行
            return new InventoryStockCarDao(db).GetListByCondition(InventoryStockCondition);
        }

        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2017/09/07 arc yano #3784 車両在庫棚卸　棚差（システム≠実棚）のある車両一覧表示機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private CarInventory SetCondition(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            CarInventory Condition = new CarInventory();
            Condition.InventoryMonth = inventoryMonth;                      //棚卸月
            Condition.DepartmentCode = form["DepartmentCode"];              //部門コード
            Condition.WarehouseCode = form["WarehouseCode"];                //倉庫コード
            Condition.LocationCode = form["LocationCode"];                  //ロケーションコード
            Condition.SalesCarNumber = form["SalesCarNumber"];              //管理番号
            Condition.Vin = form["Vin"];                                    //車台番号
            Condition.NewUsedType = form["NewUsedType"];                    //新中区分
            Condition.CarStatus = form["CarStatus"];                        //在庫区分

            //棚差有無
            if (!string.IsNullOrWhiteSpace(form["InventoryDiff"]) && form["InventoryDiff"].Contains("true"))
            {
                Condition.InventoryDiff = true;
            }
            else
            {
                Condition.InventoryDiff = false;
            }
            
            //検索実行
            return Condition;
        }
        #endregion

        #region 棚卸作業
        /// <summary>
        /// 棚卸開始
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns>検索処理された画面</returns
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        private ActionResult InventoryStart(FormCollection form, EntitySet<CarInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVSTART);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //棚卸開始チェック
            ValidationForInventoryStart(form, inventoryMonth);
            if (!ModelState.IsValid)
            {
                //画面項目の設定
                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            CodeDao dao = new CodeDao(db);

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCarテーブルのレコード作成
                //------------------------------------------------------
                var ret = db.InsertInventoryStockCar(inventoryMonth, form["WarehouseCode"], ((Employee)Session["Employee"]).EmployeeCode);

                //------------------------------------------------------
                //InventoryScheduleCarのレコード作成
                //------------------------------------------------------

                InventoryScheduleCar rec = new InventoryScheduleCar();

                rec.DepartmentCode = "";                                            //部門コード
                rec.WarehouseCode = form["WarehouseCode"];                          //倉庫コード
                rec.InventoryMonth = inventoryMonth;
                rec.InventoryStatus = STS_INACTION;                                 //実施中
                rec.StartDate = DateTime.Now;
                rec.EndDate = null;
                rec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                //InventoryScheduleCarにレコード追加
                db.InventoryScheduleCar.InsertOnSubmit(rec);

                //-----------------------------------------------
                //コミット処理
                //-----------------------------------------------
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVSTART, FORM_NAME, "");
                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            SetComponent(form, list);
                            return View("CarInventoryCriteria", list);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_INVSTART, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_INVSTART, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                    
                }

            }

            //成功した場合は、メッセージを表示する。
            ModelState.AddModelError("", "棚卸を開始しました");

            //検索を実行する
            list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            //画面項目の設定
            SetComponent(form, list);

            return View("CarInventoryCriteria", list);
        }

        /// <summary>
        /// 一時保存
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">棚卸リスト(1ページ分)</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult TemporalilyStored(FormCollection form, EntitySet<CarInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_TEMPSTORED);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //在庫棚卸情報無し
            if (line == null)
            {
                ModelState.AddModelError("", "車両在庫棚卸情報が0件のため、更新できません");
            }

            //棚卸ステータスチェック
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                if(line != null)
                {
                    list = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }

                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCarテーブルのレコード更新
                //------------------------------------------------------
                //棚卸月、倉庫コードをキーに棚卸情報を取得
                List<InventoryStockCar> isList = new InventoryStockCarDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);

                for (int k = 0; k < line.Count; k++)
                {
                    //入力値チェック
                    ValidationInputForm(line[k], k);

                    SetLineData(line[k], isList, form, inventoryMonth);
                }

                if (!ModelState.IsValid)
                {
                    list = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                    SetComponent(form, list);
                    return View("CarInventoryCriteria", list);
                }

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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_TEMPSTORED, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            SetComponent(form, list);
                            return View("CarInventoryCriteria", list);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            //保存しました。のメッセージを出力
            ModelState.AddModelError("", "棚卸データの一時保存を行いました");

            //再検索を実行する
            list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            //編集フラグをoff
            form["EditFlag"] = "false";

            SetComponent(form, list);

            return View("CarInventoryCriteria", list);
        }

        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">棚卸一覧(1ページ分)</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <history>
        /// 2017/09/07 arc yano #3784 車両在庫棚卸　棚差（システム≠実棚）のある車両一覧表示機能の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private ActionResult Download(FormCollection form, EntitySet<CarInventory> line, DateTime inventoryMonth)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<CarInventory> list = ((line == null) ? new PaginatedList<CarInventory>() : new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string excelName = "";
            string tfilePathName = "";


            //Mod  2017/09/07 arc yano #3784
            if (form["RequestFlag"].Equals("2"))    //棚卸表リスト
            {
                excelName = "CarInventoryList";

                //テンプレートファイルパス取得
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarInventoryList"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarInventoryList"];
            }
            else                                    //画面リスト
            {
                excelName = "CarInventory";

                //テンプレートファイルパス取得
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarInventory"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarInventory"];
            }


            //ファイル名(xxx(部門コード)_xxxx(部門名)_yyyyMM(対象年月)_CarInventoryList_yyyyMMddhhmiss(ダウンロード時刻))
            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            
            string departmentCode = "";             //部門コード
            string departmentName = "";             //部門名

            if (dep != null)
            {
                departmentCode = dep.DepartmentCode;
                departmentName = dep.DepartmentName;
            }

            string fileName = departmentCode + "_" + departmentName + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + excelName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, inventoryMonth, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// 棚卸仮確定
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/18 arc yano #3840 車両在庫棚卸　実棚数の初期値の変更
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        private ActionResult InventoryTempDecided(FormCollection form, EntitySet<CarInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVENTORY_TMPDECIDED);

            //検索結果の設定
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //棚卸ステータスチェック
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                //画面項目の設定
                SetComponent(form, list);
                return View("CarInventoryCriteria", list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }

            //-----------------------------
            //各パラメータの設定
            //-----------------------------

            //社員コード
            string employeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //倉庫コード
            string warehouseCode = form["WarehouseCode"];

            //------------------------------
            //入力値の更新
            //------------------------------
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //編集フラグがONの場合
                if (bool.Parse(form["EditFlag"]) == true)
                {
                    //------------------------------------------------------
                    //一度入力値で、InventoryStockのデータを更新
                    //------------------------------------------------------
                    //棚卸月、倉庫コードをキーに棚卸情報を取得
                    List<InventoryStockCar> isList = new InventoryStockCarDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);

                    for (int k = 0; k < line.Count; k++)
                    {
                        //入力値チェック
                        ValidationInputForm(line[k], k);

                        SetLineData(line[k], isList, form, inventoryMonth);
                    }

                    if (!ModelState.IsValid)
                    {
                        list = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                        SetComponent(form, list);
                        return View("CarInventoryCriteria", list);
                    }

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
                                OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                                ts.Dispose();
                                // エラーページに遷移
                                return View("Error");
                            }
                        }
                        catch (SqlException se)
                        {
                            Session["ExecSQL"] = OutputLogData.sqlText;

                            if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                            {
                                OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                                ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                                ts.Dispose();
                                return View("CarInventoryCriteria", list);
                            }
                            else
                            {
                                // ログに出力
                                OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                                ts.Dispose();
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }

                    //編集フラグをoff
                    form["EditFlag"] = "false";
                }
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {

                //実棚未入力チェック
                ValidationTempDecided(form, inventoryMonth);        //Add 2017/12/17 arc yano #3840

                if (!ModelState.IsValid)
                {
                    list = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                    SetComponent(form, list);
                    return View("CarInventoryCriteria", list);
                }

                //---------------------------
                //InventoryShcheduleの更新
                //---------------------------
                InventoryScheduleCar isrec = new InventoryScheduleCarDao(db).GetByKey(form["WarehouseCode"], inventoryMonth);

                if (isrec != null)
                {
                    isrec.InventoryStatus = STS_TMPDECIDED;                                         //ステータスを「仮確定」に更新
                    isrec.EndDate = DateTime.Now;
                    isrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    isrec.LastUpdateDate = isrec.EndDate;
                }

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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            return View("CarInventoryCriteria", list);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            //一度ModelStateをクリア
            ModelState.Clear();

            //メッセージ設定
            ModelState.AddModelError("", "棚卸を確定しました");

            //再検索を実行する
            list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            //画面コンポーネントの設定
            SetComponent(form, list);

            return View("CarInventoryCriteria", list);
        }

        /// <summary>
        /// 棚卸仮確定取消
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns>検索処理された画面</returns
        /// <history>
        /// 2018/08/01 yano #3926 車両棚卸　棚卸確定取消ボタン追加 新規作成
        /// </history>
        [AuthFilter]
        private ActionResult InventoryCancel(FormCollection form, EntitySet<CarInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVENTORY_CANCEL);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<CarInventory> list = new PaginatedList<CarInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //棚卸仮確定取消チェック
            ValidationForInventoryCancel(form, inventoryMonth);
            if (!ModelState.IsValid)
            {
                //画面項目の設定
                SetComponent(form, list);
                return View("CarInventoryCriteria", list);
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {

                //---------------------------
                //InventoryShcheduleCarの更新
                //---------------------------
                InventoryScheduleCar isrec = new InventoryScheduleCarDao(db).GetByKey(form["WarehouseCode"], inventoryMonth);

                if (isrec != null)
                {
                    isrec.InventoryStatus = STS_INACTION;                                         //ステータスを「実施中」に戻す
                    isrec.EndDate = DateTime.Now;
                    isrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    isrec.LastUpdateDate = isrec.EndDate;
                }

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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                            ts.Dispose();
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            return View("CarInventoryCriteria", list);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            //一度メッセージをクリア
            ModelState.Clear();

            //成功した場合は、メッセージを表示する。
            ModelState.AddModelError("", "棚卸仮確定を取消しました");

            //検索を実行する
            list = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            //画面項目の設定
            SetComponent(form, list);

            return View("CarInventoryCriteria", list);
        }

        /// <summary>
        /// 棚卸本確定用のダイアログ表示
        /// </summary>
        /// <param></param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        public ActionResult DecidedDialog()
        {
            FormCollection form = new FormCollection();

            List<CarInventory> line = new List<CarInventory>();

            criteriaInit = true;

            //棚卸月
            form["InventoryMonth"]= Request["InventoryMonth"];

            //ボタン押下不可フラグ
            form["ButtonDisable"] = "1";

            //ボタン押下不可フラグ
            form["UpdateButtonDisable"] = "0";

            //処理フラグ
            form["RequestFlag"] = "1";

            return DecidedDialog(form, line);
        }


        /// <summary>
        /// 棚卸本確定
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DecidedDialog(FormCollection form, List<CarInventory> line)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVENTORY_DECIDED);

            //検索結果の設定
            List<CarInventory> list = new List<CarInventory>();

            ModelState.Clear();

            //棚卸月
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01");

            //-----------------------------
            //処理分け
            //-----------------------------
            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //-----------------------
                //棚卸データ重複チェック
                //-----------------------
                case "1":

                    //画面の値で更新する
                    if(criteriaInit != true){

                        int cnt = 0;
                        //validationチェック
                        foreach (var rec in line)
                        {
                            ValidationInputForm(rec, cnt);

                            cnt++;
                        }

                        if (!ModelState.IsValid)
                        {
                            //棚卸月
                            ViewData["InventoryMonth"] = form["InventoryMonth"];
                            //ボタン押下可・不可像対
                            ViewData["ButtonDisable"] = form["ButtonDisable"];

                            return View("CarInventoryDecidedDialog", list);
                        }

                        EditDataExeCute(line, inventoryMonth);
                    }

                    //今月の棚卸データで重複があるかどうかをチェックする
                    list = new InventoryStockCarDao(db).GetListDuplicationData(inventoryMonth);

                    if (list.Count > 0)
                    {
                        ModelState.AddModelError("", "同一の車両が複数拠点に存在します。一つの拠点のみ車両が存在するように棚卸データを編集して下さい");
                        SetDropDown(list);
                    }
                    else
                    {
                        ModelState.AddModelError("", "棚卸確定を行うことが可能です。確定ボタンをクリックして下さい");
                        form["ButtonDisable"] = "0";
                        form["UpdateButtonDisable"] = "1";
                    }

                    break;

                //--------------
                //確定処理
                //--------------
                case "2":
 
                    using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
                    {

                        List<InventoryScheduleCar> sList = new InventoryScheduleCarDao(db).GetListByInventoryMonth(inventoryMonth);

                        foreach (var a in sList)
                        {
                            a.InventoryStatus = STS_DECIDED;
                            a.EndDate = DateTime.Now;
                            a.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            a.LastUpdateDate = a.EndDate;
                        }

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
                                    OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                                    ts.Dispose();
                                    // エラーページに遷移
                                    return View("Error");
                                }
                            }
                            catch (SqlException se)
                            {
                                Session["ExecSQL"] = OutputLogData.sqlText;

                                if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                                {
                                    OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                                    ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                                    ts.Dispose();
                                    return View("CarInventoryCriteria", list);
                                }
                                else
                                {
                                    // ログに出力
                                    OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                                    ts.Dispose();
                                    return View("Error");
                                }
                            }
                            catch (Exception e)
                            {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                                ts.Dispose();
                                // エラーページに遷移
                                return View("Error");
                            }

                        }
                    }
                    //メッセージ設定
                    ModelState.AddModelError("", "棚卸を確定しました");

                    form["ButtonDisable"] = "1";
                    form["UpdateButtonDisable"] = "1";

                    break;
                //--------------
                //キャンセル
                //--------------
                case "3":
                    form["ButtonDisable"] = "1";
                    break;

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    //何もしない
                    break;
            }


            //画面コントロールの設定

            //棚卸月
            ViewData["InventoryMonth"] = form["InventoryMonth"];
            //ボタン押下可・不可
            ViewData["ButtonDisable"] = form["ButtonDisable"];
            //データ更新ボタン押下可・不可
            ViewData["UpdateButtonDisable"] = form["UpdateButtonDisable"];

            return View("CarInventoryDecidedDialog", list);
        }

        #endregion

        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param></param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<CarInventoryExcelImportList> ImportList = new List<CarInventoryExcelImportList>();
            FormCollection form = new FormCollection();

            //棚卸月
            ViewData["InventoryMonth"] = Request["InventoryMonth"] + "/01";
            
            //倉庫コード
            ViewData["WarehouseCode"] = form["WarehouseCode"] = Request["WarehouseCode"];

            /*
            //ロケーションコード
            Location loc = new Location();

            loc = GetLocation(Request["DepartmentCode"], form);

            ViewData["LocationCode"] = (loc != null ? loc.LocationCode : "");
            */
            
            ViewData["ErrFlag"] = "1";

            return View("CarInventoryImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary> 
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, List<CarInventoryExcelImportList> line, FormCollection form)
        {
            List<CarInventoryExcelImportList> ImportList = new List<CarInventoryExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateImportFile(importFile);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarInventoryImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        form["ErrFlag"] = "1";
                        SetDialogDataComponent(form);
                        return View("CarInventoryImportDialog", ImportList);
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        form["ErrFlag"] = "1";
                        SetDialogDataComponent(form); ;
                        return View("CarInventoryImportDialog", ImportList);
                    }

                    form["ErrFlag"] = "0";
                    SetDialogDataComponent(form);
                    return View("CarInventoryImportDialog", ImportList);

                //--------------
                //Excel取り込み
                //--------------
                case "2":

                    DBExecute(line, form);
                    form["ErrFlag"] = "1";                                  //取り込んだ後に再度[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("CarInventoryImportDialog", ImportList);
                //--------------
                //キャンセル
                //--------------
                case "3":
                    ImportList = new List<CarInventoryExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";          //[取り込み]ボタンが押せないようにするため

                    return View("CarInventoryImportDialog", ImportList);
                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("CarInventoryImportDialog", ImportList);
            }
        }
        #endregion


        #region 棚卸ステータス取得
        /// <summary>
        /// 棚卸ステータスの返却(倉庫毎)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸ステータス</returns>
        /// <history>
        /// 2018/08/01 yano #3926 車両棚卸　棚卸確定取消ボタン追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private string GetInventoryStatusByWarehouse(string warehouseCode, DateTime inventoryMonth)
        {

            string ret = STS_UNEXCUTED;     //デフォルトは「未実施」

            InventoryScheduleCar rec = new InventoryScheduleCarDao(db).GetByKey(warehouseCode, inventoryMonth);

            
            //対象月のレコードあり
            if (rec != null)
            {
                if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_INACTION)))
                {
                    ret = STS_INACTION;     //実施中
                }
                else if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_DECIDED)))   //Add  2017/05/10 arc yano #3762
                {
                    ret = STS_DECIDED;      //完了
                }
                else
                {
                    ret = STS_TMPDECIDED;    //仮完了
                }
            }
            else
            {
                //部門コードがnullの場合は
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    ret = STS_INVALID;      //ステータス無効
                }
            }

            return ret;
        }

        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="list">検索結果</param>
        /// <history>
        /// 2017/09/07 arc yano #3784 棚差の表示
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetComponent(FormCollection form, PaginatedList<CarInventory> list)
        {
            CodeDao dao = new CodeDao(db);

            DateTime inventoryMonth;

            //項目の設定
            ViewData["InventoryMonth"] = form["InventoryMonth"];                                                            //対象年月
            ViewData["InventoryWorkingDate"] = form["InventoryWorkingDate"];                                                //棚卸作業日
            ViewData["DepartmentCode"] = form["DepartmentCode"];                                                            //部門コード

            //編集フラグ
            ViewData["EditFlag"] = false;
            if (list.Where(x => x.NewRecFlag.Equals(true)).Count() > 0)
            {
                ViewData["EditFlag"] = true;
            }

            //部門名
            if (form["DepartmentCode"] == null || string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                ViewData["DepartmentName"] = "";
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;
            }

            ViewData["WarehouseCode"] = form["WarehouseCode"];                                                                                      //倉庫コード
            ViewData["WarehouseName"] = form["WarehouseName"];                                                                                      //倉庫名

            ViewData["LocationCode"] = form["LocationCode"];                                                                                        //ロケーションコード

            //ロケーション名
            if (!string.IsNullOrWhiteSpace(form["LocationCode"]))
            {
                Location loc = new LocationDao(db).GetByKey(form["LocationCode"]);
                ViewData["LocationName"] = loc != null ? loc.LocationName : "";
                
            }

            ViewData["SalesCarNumber"] = form["SalesCarNumber"];                                                                                    //管理番号
            ViewData["Vin"] = form["Vin"];                                                                                                          //車台番号

            ViewData["NewUsedType"] = form["NewUsedType"];                                                                                          //新中区分
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);                  //新中区分リスト

            ViewData["CarStatus"] = form["CarStatus"];                                                                                              //在庫区分
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORY_CARSTOCK, false), form["CarStatus"], true);         //在庫区分リスト

            ViewData["RequestFlag"] = "999";                                                                                                        //処理種別 ※デフォルト「ページング」に設定

            ViewData["id"] = form["id"];

            ViewData["InventoryDiff"] = form["InventoryDiff"];                                                                                      //棚差有無 //Add 2017/09/07 arc yano #3784


            //棚卸状況ステータス(部門毎)
            if (true == DateTime.TryParse(ViewData["InventoryMonth"].ToString() + "/01", out inventoryMonth))
            {
                ViewData["InventoryStatus"] = GetInventoryStatusByWarehouse(ViewData["WarehouseCode"].ToString(), DateTime.Parse(ViewData["InventoryMonth"].ToString() + "/01"));
            }
            else
            {
                ViewData["InventoryStatus"] = STS_UNEXCUTED;    //未実施
            }

            //棚卸開始日
            ViewData["InventoryStartDate"] = form["InventoryStartDate"];

            //仮確定ボタンクリック可／不可状態
            ViewData["InventoryTempDecidedVisible"] = bool.Parse(form["InventoryTempDecidedVisible"]);

            //確定ボタンクリック可／不可状態
            ViewData["InventoryDecidedVisible"] = bool.Parse(form["InventoryDecidedVisible"]);

            string strdate = (ViewData["InventoryMonth"].ToString() + "/01").Replace("/", "");

            InventoryMonthControlCar rec = new InventoryMonthControlCarDao(db).GetByKey(strdate);

            //部門全体の棚卸ステータスが「仮確定」の場合押下可能
            if (rec != null && rec.InventoryStatus.Equals("002"))
            {
                ViewData["InventoryDecidedClickable"] = true;
            }
            else
            {
                ViewData["InventoryDecidedClickable"] = false;
            }
            
            //各ボタンの押下・不可設定
            SetButtonStatus(form);

            //ドロップダウンの設定
            SetDropDown(list);

            return;
        }

        /// <summary>
        /// ドロップダウンの値の設定
        /// </summary>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetDropDown(List<CarInventory> list)
        {
            CodeDao dao = new CodeDao(db);

            //コメントリスト
            List<IEnumerable<SelectListItem>> CommentList = new List<IEnumerable<SelectListItem>>();

            //新中区分リスト
            List<IEnumerable<SelectListItem>> NewUsedTypeList = new List<IEnumerable<SelectListItem>>();

            //在庫区分リスト
            List<IEnumerable<SelectListItem>> CarStatusList = new List<IEnumerable<SelectListItem>>();

            List<c_CodeName> clist = new CodeDao(db).GetCodeName("022", false);

            List<c_NewUsedType> nlist = new CodeDao(db).GetNewUsedTypeAll(false);

            List<c_CodeName> slist = new CodeDao(db).GetCodeName("020", false);


            //各明細行のドロップダウンの設定
            foreach (var a in list)
            {
                //コメントリスト
                CommentList.Add(CodeUtils.GetSelectListByModel(clist, a.Comment, true));

                //新中区分リスト
                NewUsedTypeList.Add(CodeUtils.GetSelectListByModel(nlist, a.NewUsedType, false));

                //在庫区分リスト
                CarStatusList.Add(CodeUtils.GetSelectListByModel(slist, a.CarStatus, false));
            }

            ViewData["CommentList"] = CommentList;
            ViewData["NewUsedTypeListLine"] = NewUsedTypeList;
            ViewData["CarStatusListLine"] = CarStatusList;

            return;
        }

        /// <summary>
        /// ボタン押下可／不可の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2018/08/01 yano #3926 車両棚卸　棚卸確定取消ボタン追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetButtonStatus(FormCollection form)
        {
            //---------------------------
            //ステータス管理    
            //---------------------------
            //棚卸開始ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["StartStatus"] = "disabled=\"disabled\"";

            //棚卸表出力ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["OutputStatus"] = "disabled=\"disabled\"";

            //棚卸表取込ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["InputStatus"] = "disabled=\"disabled\"";

            //棚卸作業一時保存ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["TempStatus"] = "disabled=\"disabled\"";

            //棚卸確定ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["TempDecidedStatus"] = "disabled=\"disabled\"";

            //棚卸確定ボタン押下可／不可状態 ※デフォルト非表示
            ViewData["TempDecidedVisible"] = "style=\"visibility:hidden\"";

            //棚卸本確定ボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["DecidedStatus"] = "disabled=\"disabled\"";

            //棚卸確定ボタン押下可／不可状態 ※デフォルト非表示
            ViewData["DecidedVisible"] = "style=\"visibility:hidden\"";

            //Add 2018/08/01 yano #3926
            //棚卸確定キャンセルボタン押下可／不可状態 ※デフォルト押下不可
            ViewData["CancelStatus"] = "disabled=\"disabled\"";

            //棚卸確定キャンセルボタン押下可／不可状態 ※デフォルト非表示
            ViewData["CancelVisible"] = "style=\"visibility:hidden\"";

            //明細－ロケーションコード
            ViewData["LineLocationCode"] = "readonly=\"readonly\" class=\"readonly alphanumeric\"";
            //明細－車台番号
            ViewData["LineVin"] = "readonly=\"readonly\" class=\"readonly alphanumeric\"";
            //明細－新中区分
            ViewData["LineNewUsedType"] = "disabled=\"disabled\""; ;
            //明細－在庫区分
            ViewData["LineCarStatus"] = "disabled=\"disabled\"";
            //明細－実棚
            ViewData["LinePhysicalQuantity"] = "readonly=\"readonly\" class=\"readonly alphanumeric\"";
            //明細－コメント
            ViewData["LineComment"] = "disabled=\"disabled\"";
            //明細－備考
            ViewData["LineSummary"] = "readonly=\"readonly\" class=\"readonly\"";


            //対象月の棚卸ステータスが「未実施」で棚卸作業日以降の場合は棚卸開始ボタンは押下可
            if ((ViewData["InventoryWorkingDate"] != null && !string.IsNullOrWhiteSpace(ViewData["InventoryWorkingDate"].ToString()) && (DateTime.Today.Date >= DateTime.Parse(ViewData["InventoryWorkingDate"].ToString()).AddHours(1))) && ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("000"))
            {
                ViewData["StartStatus"] = "";
            }
            //検索した部門の棚卸ステータスが実施中(001)だった場合ボタンを活性、そうでなければ非活性
            if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("001"))
            {
                ViewData["OutputStatus"] = "";
                ViewData["InputStatus"] = "";
                ViewData["TempStatus"] = "";
                ViewData["TempDecidedStatus"] = "";

                ViewData["LineLocationCode"] = "class=\"alphanumeric\"";
                ViewData["LineVin"] = "class=\"alphanumeric\""; ;
                ViewData["LineNewUsedType"] = "";
                ViewData["LineCarStatus"] = "";
                ViewData["LinePhysicalQuantity"] = "class=\"numeric\""; ;
                ViewData["LineComment"] = "";
                ViewData["LineSummary"] = "";
            }
            else //棚卸ステータス≠実施中
            {
                //棚卸ステータス=「確定」
                if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("002"))
                {
                    ViewData["OutputStatus"] = "";

                    ViewData["CancelStatus"] = "";      //Add 2018/08/01 yano #3926
                }

                //Add 2018/08/01 yano #3926
                 //棚卸ステータス=「本確定」
                if (ViewData["InventoryStatus"] != null && ViewData["InventoryStatus"].Equals("003"))
                {
                    ViewData["OutputStatus"] = "";
                }
            }

            //棚卸確定(仮)ボタンが押下可能の場合
            if (ViewData["InventoryTempDecidedVisible"] != null && (bool)ViewData["InventoryTempDecidedVisible"] == true)
            {
                //ボタン表示を行う
                ViewData["TempDecidedVisible"] = "";
            }

            //棚卸確定ボタンが押下可能の場合
            if (ViewData["InventoryDecidedVisible"] != null && (bool)(ViewData["InventoryDecidedVisible"]) == true)
            {
                //ボタン表示を行う
                ViewData["DecidedVisible"] = "";

                //棚卸確定ボタンが押下可能の場合は、棚卸仮確定キャンセルボタンも押下可能
                ViewData["CancelVisible"] = "";
            }

           
            //棚卸し確定ボタン活性化条件
            string strdate = (form["InventoryMonth"] + "/01").Replace("/", "");

            
            //棚卸確定ボタン
            if (ViewData["InventoryDecidedClickable"].Equals(true))
            {
                ViewData["DecidedStatus"] = "";
            }

            return;
        }

        /// <summary>
        /// 画面の編集した結果を設定
        /// </summary>
        /// <param name="line">在庫棚卸</param>
        /// <param name="inventoryMonth">行カウンタ</param>
        /// <param name="ilist">既存の棚卸レコード</param>
        /// <history>
        ///  2021/02/22 yano #4081 【車両在庫棚卸】画面から棚卸データ追加時の不具合
        ///  2020/08/29 yano #4049 【車両在庫棚卸】新中区分を更新できない不具合の対応
        ///  2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更
        ///  2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetLineData(CarInventory line, List<InventoryStockCar> isList, FormCollection form, DateTime inventoryMonth)
        {            
            //車台番号のスペース除去
            line.Vin = line.Vin.Trim();

            //画面で入力した実棚、コメントをInventoryStockCarテーブルに保存
            //InventoryStockCar ivs = isList.Where(x => x.LocationCode.ToUpper().Equals(line.LocationCode.ToUpper()) && x.Vin.ToUpper().Equals(line.Vin.ToUpper())).FirstOrDefault();
            InventoryStockCar ivs = isList.Where(x => x.InventoryId.Equals(line.InventoryId)).FirstOrDefault();

            if (ivs != null)
            {

                //----------------------
                //ロケーションコード
                //----------------------
                if (ivs.LocationCode != line.LocationCode)
                {
                    ivs.LocationCode = line.LocationCode;
                }

                //----------------------
                //実棚数
                //----------------------
                if (ivs.PhysicalQuantity != line.PhysicalQuantity)
                {
                    ivs.PhysicalQuantity = line.PhysicalQuantity;
                }

                //Add 2017/12/15 arc yano #3839
                //----------------------
                //在庫区分
                //----------------------
                if (ivs.CarUsage != line.CarStatus)
                {
                    ivs.CarUsage = line.CarStatus;
                }

                //Add 2020/08/29 yano #4049
                //----------------------
                //新中区分
                //----------------------
                //システム管理者の場合
                if (!string.IsNullOrWhiteSpace(form["InventoryDecidedVisible"]) && form["InventoryDecidedVisible"].ToLower().Equals("true"))
                    if (ivs.NewUsedType != line.NewUsedType)
                {
                    ivs.NewUsedType = line.NewUsedType;
                }

                //----------------------
                //誤差理由
                //----------------------
                if (!string.IsNullOrWhiteSpace(ivs.Comment) || !string.IsNullOrWhiteSpace(line.Comment))
                {
                    if (!string.IsNullOrWhiteSpace(ivs.Comment) && !string.IsNullOrWhiteSpace(line.Comment))
                    {
                        if (!ivs.Comment.Equals(line.Comment))
                        {
                            ivs.Comment = line.Comment;
                        }
                    }
                    else
                    {
                        ivs.Comment = line.Comment;
                    }
                }
                //-------------------
                //備考
                //-------------------
                if (!string.IsNullOrWhiteSpace(ivs.Summary) || !string.IsNullOrWhiteSpace(line.Summary))
                {
                    if (!string.IsNullOrWhiteSpace(ivs.Summary) && !string.IsNullOrWhiteSpace(line.Summary))
                    {
                        if (!ivs.Summary.Equals(line.Summary))
                        {
                            ivs.Summary = line.Summary;
                        }
                    }
                    else
                    {
                        ivs.Summary = line.Summary;
                    }
                }
            }
            else
            {
                InventoryStockCar NewRec = new InventoryStockCar();

                NewRec = EditInventoryStockCar(form, line, inventoryMonth);

                db.InventoryStockCar.InsertOnSubmit(NewRec);
            }

            return;
        }

        #endregion

        #region Validationチェック
        /// <summary>
        /// 棚卸開始時のValidationチェック
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <history>
        ///  2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void ValidationForInventoryStart(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //部門毎棚卸状況をチェック
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            if (status.Equals(STS_INACTION)) //棚卸ステータス=「実施中」
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の棚卸を実施中であるため、棚卸を開始できません");
            }
            else if (status.Equals(STS_DECIDED) || status.Equals(STS_TMPDECIDED)) //棚卸ステータス=「仮確定」OR 「確定」
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の棚卸を終了しているため、棚卸を開始できません");
            }
            //--------------------------------
            //作業日のチェック
            //--------------------------------
            DateTime inventoryWorkingDate;

            inventoryWorkingDate = DateTime.Parse(form["InventoryWorkingDate"]);
            //当日日付が作業日前の場合はエラー
            if (DateTime.Now.Date.CompareTo(inventoryWorkingDate) < 0)
            {
                ModelState.AddModelError("", "棚卸基準日になっていないため、棚卸を開始できません");
            }

            return;
        }

        /// <summary>
        /// 棚卸開始時のValidationチェック
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void ValidateInventoryStatus(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //部門毎棚卸状況をチェック
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            //棚卸ステータス=「仮確定」or「確定」
            if (status.Equals(STS_DECIDED) || status.Equals(STS_TMPDECIDED))
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の車両棚卸は終了しているため、更新できません。");
            }

            return;
        }

        /// <summary>
        /// 一時保存時のValidationチェック
        /// </summary>
        /// <param name="line">在庫棚卸</param>
        /// <param name="inventoryMonth">行カウンタ</param>
        /// <history>
        ///  2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void ValidationInputForm(CarInventory line, int cnt)
        {
            ///------------------------
            // 必須チェック
            //------------------------
            //車台番号
            if (string.IsNullOrWhiteSpace(line.Vin))
            {
                ModelState.AddModelError("line[" + cnt + "].Vin", MessageUtils.GetMessage("E0001", cnt + 1 + "行目の車台番号"));
            }
            //ロケーションコード
            if (string.IsNullOrWhiteSpace(line.LocationCode))
            {
                ModelState.AddModelError("line[" + cnt + "].LocationCode", MessageUtils.GetMessage("E0001", cnt + 1 + "行目のロケーションコード"));
            }
            else
            {
                Location rec = new LocationDao(db).GetByKey(line.LocationCode);
                if (rec == null)
                {
                    ModelState.AddModelError("line[" + cnt + "].LocationCode", cnt + 1 + "のロケーションがマスタに登録されていません。");
                }
            }

            return;
        }

        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="ImportList">読取データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2021/10/19 yano #4107【車両棚卸】エラーメッセージ文言の改善
        /// 2017/12/18 arc yano #3840 車両在庫棚卸　車両在庫棚卸　実棚数の初期値の変更 実棚数の必須チェックの廃止
        /// 2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更 新中区分のvalidationチェックの廃止
        /// 2017/07/26 arc yano #3762 車両在庫棚卸機能追加 預かり車の場合はチェックを行わない
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public void ValidateImportList(List<CarInventoryExcelImportList> ImportList, FormCollection form)
        {
            bool locationErr = false;

            //車台番号重複リスト
            List<string> vinList = new List<string>();

            List<c_ColorCategory> ccList = new CodeDao(db).GetColorCategoryAll(false);

            Location loc = new LocationDao(db).GetByKey(ImportList[0].LocationCode);

            string locationName = (loc != null ? loc.LocationName : "");

            for (int i = 0; i < ImportList.Count; i++)
            {
                List<SalesCar> slist = new List<SalesCar>();

                //預かり車両の場合はチェックを行わない
                if (string.IsNullOrWhiteSpace(ImportList[i].CarStatusName) || !ImportList[i].CarStatusName.Equals("預かり車"))
                {
                    //------------------------
                    // 必須チェック
                    //------------------------
                    //車台番号
                    if (string.IsNullOrWhiteSpace(ImportList[i].Vin))
                    {
                        ModelState.AddModelError("line[" + i + "].Vin", MessageUtils.GetMessage("E0001", i + 1 + "行目の車台番号"));
                    }
                    else //車台番号が入力されている場合はマスタチェックを行う
                    {
                        //車両マスタ検索
                        slist = new SalesCarDao(db).GetByLikeVin(ImportList[i].Vin);

                        //検索結果が0件の場合
                        if (slist.Count == 0)
                        {
                            ModelState.AddModelError("line[" + i + "].Vin", i + 1 + "行目の車台番号はマスタに登録されていません。");
                        }
                        else if (slist.Count == 1)
                        {
                            //管理番号
                            SalesCar rec = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, true);

                            if (rec != null)
                            {
                                //管理番号で引き直したデータの車台番号と、エクセルの車台番号が異なる場合は
                                if (!rec.Vin.Equals(ImportList[i].Vin))
                                {
                                    ModelState.AddModelError("line[" + i + "].SalesCarNumber", i + 1 + "行目の車台番号に対する管理番号が不正です。マスタに登録されている管理番号を入力して下さい");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("line[" + i + "].SalesCarNumber", i + 1 + "行目の車台番号に対する管理番号が不正です。マスタに登録されている管理番号を入力して下さい");
                            }
                        }
                        else//検索結果が複数存在した場合
                        {
                            //Mod 2021/10/19 yano #4107
                            ModelState.AddModelError("line[" + i + "].Vin", i + 1 + "行目の車台番号に該当する車両マスタが複数有効になっています。車両マスタを編集後、再度取り込みを行って下さい");
                          
                            ////管理番号が未入力の場合
                            //if (string.IsNullOrWhiteSpace(ImportList[i].SalesCarNumber))
                            //{
                            //    ModelState.AddModelError("line[" + i + "].Vin", i + 1 + "行目の車台番号に該当する車両がマスタに複数存在します。車両マスタを編集後、再度取込を行ってください");
                            //}
                            //else
                            //{
                            //    //管理番号
                            //    SalesCar rec = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, true);

                            //    if (rec != null)
                            //    {
                            //        //管理番号で引き直したデータの車台番号と、エクセルの車台番号が異なる場合は
                            //        if (!rec.Vin.Equals(ImportList[i].Vin))
                            //        {
                            //            ModelState.AddModelError("line[" + i + "].SalesCarNumber", i + 1 + "行目の車台番号に対する管理番号が不正です。マスタに登録されている管理番号を入力して下さい");
                            //        }
                            //    }
                            //    else
                            //    {
                            //        ModelState.AddModelError("line[" + i + "].SalesCarNumber", i + 1 + "行目の車台番号に対する管理番号が不正です。マスタに登録されている管理番号を入力して下さい");
                            //    }
                            //}
                        }
                    }

                    //ロケーションコードが未入力
                    if (string.IsNullOrWhiteSpace(ImportList[i].LocationCode))
                    {
                        if (locationErr.Equals(false))
                        {
                            ModelState.AddModelError("", "ロケーションコードが入力されていません。棚卸表にロケーションコードを入力してください");

                            locationErr = true;
                        }

                    }
                    else
                    {
                        //ロケーションコード
                        Location rec = new LocationDao(db).GetByKey(ImportList[i].LocationCode);
                        if (rec == null)
                        {
                            if (locationErr.Equals(false))
                            {
                                ModelState.AddModelError("", "ロケーションがマスタに登録されていません。棚卸表のロケーションコードを編集してください。");

                                locationErr = true;
                            }
                        }
                        else
                        {
                            //棚卸対象部門以外のロケーションのデータを取込んだ場合
                            if (!rec.WarehouseCode.Equals(form["WarehouseCode"]))
                            {
                                if (locationErr.Equals(false))
                                {
                                    ModelState.AddModelError("", "ロケーションが対象部門のロケーションではありません。棚卸表のロケーションコードを編集してください。");

                                    locationErr = true;
                                }
                            }
                        }
                    }

                    //Del 2017/12/15 arc yano #3839
                    /*
                    //車両区分
                    if (string.IsNullOrWhiteSpace(ImportList[i].NewUsedType))
                    {
                        ModelState.AddModelError("line[" + i + "].NewUsedTypeName", MessageUtils.GetMessage("E0001", i + 1 + "行目の車両区分"));
                    }
                    */
                    //在庫区分
                    if (string.IsNullOrWhiteSpace(ImportList[i].CarStatusName)) //Mod 2017/07/14 チェック対象をCarStatusNameに変更
                    {
                        ModelState.AddModelError("line[" + i + "].CarStatusName", MessageUtils.GetMessage("E0001", i + 1 + "行目の在庫区分"));
                    }

                    //実棚数
                    //Mod 2017/12/18 arc yano #3840
                    /*
                    if (ImportList[i].PhysicalQuantity == null)
                    {
                        ModelState.AddModelError("line[" + i + "].PhysicalQuantity", MessageUtils.GetMessage("E0001", i + 1 + "行目の実棚"));
                    }
                    else
                    {
                        //実棚が入力されている場合は入力値のチェックを行う
                        if (ImportList[i].PhysicalQuantity != 0 && ImportList[i].PhysicalQuantity != 1)
                        {
                            ModelState.AddModelError("line[" + i + "].PhysicalQuantity", MessageUtils.GetMessage("E0002", new string[] { "実棚", i + 1 + "行目の実棚が正しくありません。0または1" }));
                        }
                    }
                    */
                    //実棚が入力されている場合は入力値のチェックを行う
                    if (ImportList[i].PhysicalQuantity != null && ImportList[i].PhysicalQuantity != 0 && ImportList[i].PhysicalQuantity != 1)
                    {
                        ModelState.AddModelError("line[" + i + "].PhysicalQuantity", MessageUtils.GetMessage("E0002", new string[] { "実棚", i + 1 + "行目の実棚が正しくありません。0または1" }));
                    }

                    //レコード件数が２件以上
                    if (ImportList.Where(x => x.Vin.Equals(ImportList[i].Vin)).Count() > 1)
                    {
                        //重複エラーリストの中に存在しない場合
                        if (!vinList.Contains(ImportList[i].Vin))
                        {
                            ModelState.AddModelError("line[" + i + "].Vin", "車台番号=" + ImportList[i].Vin + " が重複しています。");

                            vinList.Add(ImportList[i].Vin);
                        }
                        else
                        {
                            ModelState.AddModelError("line[" + i + "].Vin", "");
                        }
                    }

                    //取得した車両データが１件の場合　　　
                    if (slist.Count == 1)
                    {
                        ImportList[i] = SetPropertyFromDB(ImportList[i], slist[0], ccList);
                    }
                }
            }
        }

        /// <summary>
        /// 棚卸仮確定時のValidationチェック
        /// </summary>
        /// <param name="line">在庫棚卸</param>
        /// <param name="form">フォーム入力値</param>
        /// <history>
        ///  2020/02/21 yano #4041 【車両棚卸】棚卸確定時のチェック漏れ対応
        ///  2017/12/18 arc yano #3840 車両在庫棚卸　実棚数の初期値の変更
        /// </history>
        private void ValidationTempDecided(FormCollection form, DateTime inventoryMonth)
        {

            //Mod 2020/02/21 yano #4041
            //実棚数が未入力のデータを取得する
            CarInventory InventoryStockCondition = new CarInventory();

            InventoryStockCondition.InventoryMonth = inventoryMonth;
            InventoryStockCondition.DepartmentCode = form["DepartmentCode"];              //部門コード
            InventoryStockCondition.WarehouseCode = form["WarehouseCode"];                //倉庫コード

            //検索実行
            var list = new InventoryStockCarDao(db).GetListByCondition(InventoryStockCondition).Where(x => x.PhysicalQuantity == null);

            //var list = GetSearchResultList(form, inventoryMonth).Where(x => x.PhysicalQuantity == null);

            if (list != null)
            {
                foreach (var rec in list)
                {
                    ModelState.AddModelError("", "ロケーションコード=" + rec.LocationCode + ", 車台番号=" + rec.Vin + "の実棚数を入力後、再度確定ボタンをクリックしてください");
                }
            }

            return;
        }

        /// <summary>
        /// 取込ファイル存在チェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void ValidateImportFile(HttpPostedFileBase filePath)
        {
            // 必須チェック
            if (filePath == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            return;
        }

        /// <summary>
        /// 棚卸データ重複チェック
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private List<CarInventory> ValidateDeulication(DateTime inventoryMonth)
        {
            //今月の棚卸データで重複があるかどうかをチェックする
            List<CarInventory> list = new List<CarInventory>();

            list = new InventoryStockCarDao(db).GetListDuplicationData(inventoryMonth);

            // 重複データが存在が存在する場合はメッセージを表示する
            if (list.Count > 0)
            {
                ModelState.AddModelError("", "同一の車両が複数拠点に存在します。一つの拠点のみ車両が存在するように棚卸データを編集して下さい");
            }

            return list;
        }

        /// <summary>
        /// 棚卸仮確定取消時のValidationチェック
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <history>
        ///  2018/08/01 yano #3926 車両棚卸　棚卸確定取消ボタン追加 新規追加
        /// </history>
        private void ValidationForInventoryCancel(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //部門毎棚卸状況をチェック
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            if (!status.Equals(STS_TMPDECIDED)) //棚卸ステータス=「仮確定」
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の棚卸が仮確定ではないため、取消できません");
            }

            return;
        }

        #endregion

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        /// <history>
        /// 2017/09/07 arc yano #3784 画面リスト出力処理の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private byte[] MakeExcelData(FormCollection form, DateTime inventoryMonth, string fileName, string tfileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


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
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
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


            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            
            ConfigLine cl = new ConfigLine();

            cl.DIndex = configLine.DIndex;
            cl.SheetName = configLine.SheetName;
            cl.Type = configLine.Type;
  
            //----------------------------
            // データ行出力
            //----------------------------
            //棚卸情報の取得
            List<CarInventory> list = GetSearchResultList(form, inventoryMonth).ToList();

            //出力するExcelによる分岐
            if (form["RequestFlag"].Equals("2"))  //棚卸表出力
            {
                //データ設定
                ret = dExport.SetData<CarInventory, CarInventoryForExcel>(ref excelFile, list, configLine);

                //------------------------
                // ロケーションコード
                //------------------------

                //設定位置
                string pos = "Q2";

                cl.SetPos.Add(pos);

                //タイプ
                cl.Type = 0;

                List<LocationInfo> slist = new List<LocationInfo>();

                LocationInfo line = new LocationInfo();

                //ロケーション取得
                Location rec = new Location();

                rec = GetLocation(form["DepartmentCode"], form);

                line.LocationCode = rec != null ? rec.LocationCode : "";

                line.LocationName = rec != null ? rec.LocationName : "";

                slist.Add(line);

                //データ設定
                ret = dExport.SetData<LocationInfo, LocationInfo>(ref excelFile, slist, cl);

                //----------------------------
                // 説明行出力
                //----------------------------

                //説明文設定
                List<DescriptionBlock> dblist = new List<DescriptionBlock>();

                DescriptionBlock block = new DescriptionBlock();

                block.Description = DESCRIPTION_EXCELDOWNLOAD;

                dblist.Add(block);

                for (int i = 0; i < configLine.SetPosRowCol.Count; i++)
                {
                    Tuple<int, int> setpos = new Tuple<int, int>(configLine.SetPosRowCol[i].Item1 + list.Count, configLine.SetPosRowCol[i].Item2);

                    configLine.SetPosRowCol[i] = setpos;
                }

                //データ設定
                ret = dExport.SetData<DescriptionBlock, DescriptionBlock>(ref excelFile, dblist, configLine);

                excelFile.Workbook.Worksheets["masterList"].Cells[27, 2].Formula = excelFile.Workbook.Worksheets["masterList"].Cells[27, 2].Formula;
            }
            else   //画面リスト出力
            {
                //データ設定
                ret = dExport.SetData<CarInventory, CarInventoryForExcelList>(ref excelFile, list, configLine);

                //----------------------------
                // 検索条件出力
                //----------------------------
                configLine.SetPos[0] = "A1";

                //検索条件取得
                CarInventory condition = SetCondition(form, inventoryMonth);

                //検索条件文字列を作成
                DataTable dtCondtion = MakeConditionRow(condition);

                //データ設定
                ret = dExport.SetData(ref excelFile, dtCondtion, configLine);
            
            }

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

        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private List<CarInventoryExcelImportList> ReadExcelData(HttpPostedFileBase importFile, List<CarInventoryExcelImportList> ImportList)
        {
            //カラム番号保存用
            int[] colNumber;

            colNumber = new int[7] { -1, -1, -1, -1, -1, -1, -1 };

            ConfigLine configLine = new ConfigLine();

            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }
                //----------------------------
                // 設定シート取得
                //----------------------------
                ExcelWorksheet config = pck.Workbook.Worksheets["config"];

                DataExport dExport = new DataExport();

                //設定データを取得(config)
                if (config != null)
                {
                    configLine = dExport.GetConfigLine(config, 2);
                }
                else //configシートが無い場合はエラー
                {
                    ModelState.AddModelError("", "棚卸表のconfigシートがみつかりません");
                    return ImportList;
                }

                //--------------------------------------
                //データ行数読込
                //--------------------------------------
                ExcelWorksheet master = pck.Workbook.Worksheets["masterList"];

                if (master == null)
                {
                    ModelState.AddModelError("", "棚卸表のmasterListシートがみつかりません");
                    return ImportList;
                }

                int dataCnt = master.Cells[27, 2].GetValue<int>();

                //データ行数が0の場合
                if (dataCnt == 0)
                {
                    ModelState.AddModelError("", "データ行数を読み込めません。一度ファイルを保存してから取込を行って下さい");
                    return ImportList;
                }


                //--------------------------------------
                //データシート読込
                //--------------------------------------
                ExcelWorksheet ws = pck.Workbook.Worksheets[configLine.SheetName];

                //ロケーションコード読込
                string locationcode = pck.Workbook.Worksheets[configLine.SheetName].Cells[2,17].Text;

                string locationname = "";

                if (!string.IsNullOrWhiteSpace(locationcode))
                {
                    Location rec = new LocationDao(db).GetByKey(locationcode);

                    locationname = rec != null ? rec.LocationName : "";
                }

                //読み込むシートが存在しなかった場合
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
                    return ImportList;
                }

                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = configLine.SetPosRowCol[0].Item1;            //行の開始位置
                int StartCol = configLine.SetPosRowCol[0].Item2;            //列の開始位置

                //int EndRow = StartRow + (dataCnt - 1);                      //行の終了位置
                int EndCol = ws.Dimension.End.Column;                       //列の終了位置

                //ヘッダ行(読取位置の行の１行前をヘッダ行とする
                var headerRow = ws.Cells[(StartRow - 1), StartCol, (StartRow - 1), EndCol];
                colNumber = SetColNumber(headerRow, colNumber, StartCol);

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                string[] Result = new string[colNumber.Count()];
                bool dspflg = false;

                for (int dcnt = 0, datarow = StartRow; dcnt < dataCnt ; datarow++)
                {
                    CarInventoryExcelImportList data = new CarInventoryExcelImportList();

                    //更新データの取得
                    for (int col = StartCol; col <= EndCol; col++)
                    {
                        //説明文の場合は
                        if (!string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) && ws.Cells[datarow, col].Text.Contains(DESCRIPTION_EXCELDOWNLOAD))
                        {
                            dspflg = true;
                            break;
                        }

                        for (int i = 0; i < colNumber.Count(); i++)
                        {
                            if (col == colNumber[i])
                            {
                                Result[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }
                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    if (dspflg == true)
                    {
                        dspflg = false;
                    }
                    else
                    {
                        ImportList = SetProperty(ref Result, ref ImportList, locationcode, locationname);
                        dcnt++;     //読込データ数
                    }
                }
            }
            return ImportList;

        }
        #endregion

        #region

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/07 arc yano #3784 画面リスト出力処理の追加
        /// </history>
        private DataTable MakeConditionRow(CarInventory condition)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";
            
            string departmentName = "";
            string warehouseName = "";

            //部門名取得
            Department dep = new DepartmentDao(db).GetByKey(condition.DepartmentCode, false);
            departmentName = dep != null ? dep.DepartmentName : "";


            //倉庫名取得
            Warehouse wareh = new WarehouseDao(db).GetByKey(condition.WarehouseCode);
            warehouseName = wareh != null ? wareh.WarehouseName : "";

            //---------------------
            //　列定義
            //---------------------
            //１つの列を設定  
            if (condition.InventoryMonth != null)
            {
                dt.Columns.Add("CondisionText", Type.GetType("System.String"));
            }

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();
            //部門コード
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                conditionText += string.Format("部門={0}:{1}　", condition.DepartmentCode, departmentName);
            }
            //Add 2016/08/13 arc yano #3596
            //倉庫コード     
            if (!string.IsNullOrEmpty(condition.WarehouseCode))
            {
                conditionText += string.Format("倉庫={0}:{1}　", condition.WarehouseCode, warehouseName);
            }
            if (condition.InventoryMonth != null)
            {
                conditionText += string.Format("対象年月={0:yyyy/MM}　", condition.InventoryMonth);
            }
            //ロケーションコード
            if (!string.IsNullOrEmpty(condition.LocationCode))
            {
                conditionText += string.Format("ロケーションコード={0}　", condition.LocationCode);
            }
            //ロケーション名
            if (!string.IsNullOrEmpty(condition.LocationName))
            {
                conditionText += string.Format("ロケーション名={0}　", condition.LocationName);
            }
            //管理番号
            if (!string.IsNullOrEmpty(condition.SalesCarNumber))
            {
                conditionText += string.Format("管理番号={0}　", condition.SalesCarNumber);
            }
            //車台番号
            if (!string.IsNullOrEmpty(condition.Vin))
            {
                conditionText += string.Format("車台番号={0}　", condition.Vin);
            }
            //新中区分
            if (!string.IsNullOrEmpty(condition.NewUsedTypeName))
            {
                conditionText += string.Format("新中区分={0}　", condition.NewUsedTypeName);
            }
            //在庫区分
            if (!string.IsNullOrEmpty(condition.CarStatusName))
            {
                conditionText += string.Format("在庫区分={0}　", condition.CarStatusName);
            }
            //棚差有無
            if (condition.InventoryDiff.Equals(true))
            {
                conditionText += string.Format("棚差有無=棚差があるレコードのみ表示　");
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion


        #region 各項目の列番号設定
        /// <summary>
        ///     
        /// </summary>
        /// <param name="headerRow">ヘッダ行</param>
        /// <param name="colNumber">設定する列番号</param>
        /// <param name="startCol">開始列番号</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber, int startCol)
        {
            //開始処理
            int cnt = startCol;

            //列番号設定
            foreach (var cell in headerRow)
            {
                if (cell != null)
                {
                    //管理番号
                    if (cell.Text.Contains("管理番号"))
                    {
                        colNumber[0] = cnt;
                    }
                    //車台番号
                    if (cell.Text.Contains("車台番号"))
                    {
                        colNumber[1] = cnt;
                    }
                    //車両区分
                    if (cell.Text.Contains("車両区分"))
                    {
                        colNumber[2] = cnt;
                    }
                    //在庫区分
                    if (cell.Text.Contains("在庫区分"))
                    {
                        colNumber[3] = cnt;
                    }
                    //実棚
                    if (cell.Text.Contains("実棚"))
                    {
                        colNumber[4] = cnt;
                    }
                    //誤差理由・備考
                    if (cell.Text.Contains("誤差理由"))
                    {
                        //誤差理由
                        colNumber[5] = cnt;

                        //備考
                        colNumber[6] = cnt + 1;
                    }

                }
                cnt++;
            }

            for (int i = 0; i < colNumber.Length; i++)
            {
                if (colNumber[i] == -1)
                {
                    ModelState.AddModelError("importFile", "ヘッダ行が正しくありません。");
                    break;
                }
            }


            return colNumber;
        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="Result">読取結果</param>
        /// <param name="ImportList"></param>
        /// <param name="LocationCode"></param>
        /// <param name="LocationName"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/10/19 yano #4107 【車両棚卸】エラーメッセージ文言の改善
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public List<CarInventoryExcelImportList> SetProperty(ref string[] Result, ref List<CarInventoryExcelImportList> ImportList, string LocationCode, string LocationName)
        {
            CarInventoryExcelImportList SetLine = new CarInventoryExcelImportList();

            List<SalesCar> slist = new SalesCarDao(db).GetByLikeVin(!string.IsNullOrWhiteSpace(Result[1]) ? Result[1].Trim().ToUpper() : "");

            // 車台番号
            SetLine.Vin = (slist.Count >= 1) ? slist.FirstOrDefault().Vin : ""; //Mod 2021/10/19 yano #4107 

            //  管理番号
            SetLine.SalesCarNumber = !string.IsNullOrWhiteSpace(Result[0]) ? Result[0].Trim().ToUpper() : "";

            // 在庫区分名
            SetLine.CarStatusName = Result[3];

            // 在庫区分名から在庫区分コードを取得
            c_CodeName rec2 = new CodeDao(db).GetCodeFromName("020", SetLine.CarStatusName);

            SetLine.CarStatus = rec2 != null ? rec2.Code : "";

            // 車両区分名
            SetLine.NewUsedTypeName = Result[2];

            // 車両区分名から車両区分コードを取得
            c_NewUsedType rec = new CodeDao(db).GetNewUsedTypeByName(SetLine.NewUsedTypeName);
            
            SetLine.NewUsedType = rec != null ? rec.Code : "";

            // 実棚
            decimal workQuantity;

            //文字列→decimalに変換
            bool ret = Decimal.TryParse(Result[4], out workQuantity);

            if (ret != false)
            {
                SetLine.PhysicalQuantity = decimal.Parse(Result[4]);
            }

            // 誤差理由
            c_CodeName rec3 = new CodeDao(db).GetCodeFromName("022", Result[5]); 

            SetLine.Comment = rec3 != null ? rec3.Code : "";

            SetLine.CommentName = Result[5];

            // 備考
            SetLine.Summary = Result[6];

            //ロケーション
            SetLine.LocationCode = LocationCode;

            //ロケーション
            SetLine.LocationName = LocationName;

            //---------------------
            //入力補完処理
            //---------------------
            
            //管理番号がNULLの場合は車台番号から車両マスタを参照して補完する
            if (string.IsNullOrWhiteSpace(SetLine.SalesCarNumber))
            {
                //車台番号から取得した車両マスタのレコード件数が１件の場合
                if (slist != null && slist.Count == 1)
                {
                    SetLine.SalesCarNumber = slist[0].SalesCarNumber;
                }
            }
            
            //車両区分が未入力の場合は補完する
            if (string.IsNullOrWhiteSpace(SetLine.NewUsedTypeName))
            {
                if (slist != null && slist.Count == 1)
                {
                    SetLine.NewUsedType = slist[0].NewUsedType;
                    SetLine.NewUsedTypeName = slist[0].c_NewUsedType.Name;
                }
            }

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        /// <summary>
        /// その他の項目をＤＢから設定する
        /// </summary>
        /// <param name="SetLine"></param>
        /// <param name="rec"></param>
        /// <param name="cclist"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public CarInventoryExcelImportList SetPropertyFromDB(CarInventoryExcelImportList SetLine, SalesCar rec, List<c_ColorCategory>cclist)
        {
            // 車台番号(車台番号は全桁入力されていない場合があるため、DBの値で上書きする）
            //SetLine.Vin = rec.Vin;

            // ブランド名
            SetLine.CarBrandName = rec.CarGrade.Car.Brand.CarBrandName;

            // 車種名
            SetLine.CarName = rec.CarGrade.Car.CarName;

            // 系統色名
            c_ColorCategory line = cclist.Where(x => x.Code.Equals(rec.ColorType)).FirstOrDefault();
               
            SetLine.ColorType = line != null ? line.Name : "";

            // 車両カラーコード
            SetLine.ExteriorColorCode = rec.ExteriorColorCode;

            // 車両カラー名
            SetLine.ExteriorColorName = rec.ExteriorColorName;

            // 車両登録番号
            SetLine.RegistrationNumber = rec.MorterViecleOfficialCode + " " + rec.RegistrationNumberType + " " + rec.RegistrationNumberKana + " " + rec.RegistrationNumberPlate;

            return SetLine;
        }

        #region ダイアログのデータ付きコンポーネント設定
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];

            //棚卸し追記
            ViewData["InventoryMonth"] = form["InventoryMonth"];

            //倉庫コード
            ViewData["WarehouseCode"] = form["WarehouseCode"];

            //ロケーションコード
            //ViewData["LocationCode"] = form["LocationCode"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <param name="form"></param>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2020/08/29 yano #4049 【車両在庫棚卸】新中区分を更新できない不具合の対応
        /// 2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void DBExecute(List<CarInventoryExcelImportList> ImportLine, FormCollection form)
        {
            //棚卸月
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"]);
 
            using (TransactionScope ts = new TransactionScope())
            {
                List<InventoryStockCar> workList = new List<InventoryStockCar>();

                string msg = "";

                //車両棚卸テーブルを更新して、入荷テーブルを登録する
                foreach (var LineData in ImportLine)
                {
                    //Mod 2017/07/14 arc yano 在庫区分名が預かり車の場合は登録を行わない
                    if (!LineData.CarStatusName.Equals("預かり車"))
                    {
                        //車両マスタ検索
                        SalesCar srec = new SalesCarDao(db).GetByKey(LineData.SalesCarNumber, true);

                        //車台番号で車両棚卸テーブルを検索する。
                        InventoryStockCar ret = new InventoryStockCarDao(db).GetByLocVin(inventoryMonth, LineData.LocationCode, srec.Vin);

                        //ヒットしなかった場合は新規作成
                        if (ret == null)
                        {
                            //---------------------------------
                            // InventoryStockCarテーブル登録
                            //---------------------------------
                            InventoryStockCar NewRec = new InventoryStockCar();
                            NewRec.InventoryId = Guid.NewGuid();                                                                                                    //棚卸ID
                            NewRec.DepartmentCode = form["DepartmentCode"];                                                                                         //棚卸部門
                            NewRec.InventoryMonth = inventoryMonth;                                                                                                 //棚卸月
                            NewRec.LocationCode = LineData.LocationCode;                                                                                            //ロケーションコード
                            NewRec.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                                                     //棚卸ユーザ
                            NewRec.SalesCarNumber = srec.SalesCarNumber;                                                                                            //管理番号
                            NewRec.Vin = srec.Vin;                                                                                                                  //車台番号
                            NewRec.NewUsedType = !string.IsNullOrWhiteSpace(LineData.NewUsedType) ? LineData.NewUsedType : srec != null ? srec.NewUsedType : "";    //新中区分      //Mod 2020/08/29 yano #4049 //Mod 2017/12/15 arc yano #3839
                            NewRec.CarUsage = LineData.CarStatus;                                                                                                   //利用用途
                            NewRec.Quantity = 0;                                                                                                                    //数量(デフォルトは0)
                            NewRec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                                               //作成者
                            NewRec.CreateDate = DateTime.Now;                                                                                                       //作成日付
                            NewRec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                                           //作成者
                            NewRec.CreateDate = DateTime.Now;                                                                                                       //最終更新者
                            NewRec.LastUpdateDate = DateTime.Now;                                                                                                   //最終更新日付
                            NewRec.DelFlag = "0";                                                                                                                   //削除フラグ
                            NewRec.Summary = LineData.Summary;                                                                                                      //備考
                            NewRec.PhysicalQuantity = LineData.PhysicalQuantity;                                                                                    //実棚
                            NewRec.Comment = LineData.Comment;                                                                                                      //誤差理由
                            NewRec.WarehouseCode = form["WarehouseCode"];                                                                                           //倉庫コード

                            workList.Add(NewRec);
                        }
                        else //ヒットした場合は更新する
                        {
                            //---------------------------------
                            // InventoryStockCarテーブル更新
                            //---------------------------------
                            ret.CarUsage = LineData.CarStatus;                                              //在庫区分
                            ret.PhysicalQuantity = LineData.PhysicalQuantity;                               //実棚
                            ret.Comment = LineData.Comment;                                                 //誤差理由
                            ret.Summary = LineData.Summary;                                                 //備考
                            ret.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;      //最終更新者
                            ret.LastUpdateDate = DateTime.Now;                                              //最終更新日
                        }
                    }
                    else
                    {
                        msg = "車台番号=" + LineData.Vin + "の車両は預かり車のため取込をスキップしました";
                        ModelState.AddModelError("", msg);
                    }
                }

                db.InventoryStockCar.InsertAllOnSubmit(workList);

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region 編集したデータの更新(確定時)
        /// <summary>
        /// 編集データのDB更新(確定時)
        /// </summary>
        /// <param name="form"></param>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void EditDataExeCute(List<CarInventory> line, DateTime inventoryMonth)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                foreach(var rec in line)
                {
                 
                    //車台番号で車両棚卸テーブルを検索する。
                    InventoryStockCar ret = new InventoryStockCarDao(db).GetByKey(rec.InventoryId);

                    if (ret != null)
                    {
                        ret.LocationCode = rec.LocationCode;                                            //ロケーションコード
                        ret.Vin = rec.Vin;                                                              //車台番号
                        ret.NewUsedType = rec.NewUsedType;                                              //新中区分
                        ret.CarUsage = rec.CarStatus;                                                   //在庫区分
                        ret.PhysicalQuantity = rec.PhysicalQuantity;                                    //実棚
                        ret.Comment = rec.Comment;                                                      //誤差理由
                        ret.Summary = rec.Summary;                                                      //備考
                        ret.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;      //最終更新者
                        ret.LastUpdateDate = DateTime.Now;                                              //最終更新日
                    }
                }

                if (!ModelState.IsValid)
                {
                    return;
                }

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region コントロールの有効無効
        /// <summary>
        /// コントロールの有効無効状態を返す。
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private void GetViewResult(FormCollection form)
        {
            Employee loginUser = (Employee)Session["Employee"];
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            //セキュリティレベルが4でないユーザは、ボタンを押下不可にする。
            if (!securityLevel.Equals("004"))
            {
                ViewData["ButtonEnabled"] = false;
            }
            else
            {
                ViewData["ButtonEnabled"] = true;
            }

            return;
        }
        #endregion

        #region その他共通処理
        /// <summary>
        /// ロケーションコード取得処理
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private Location GetLocation(string deparmtnetCode, FormCollection form)
        {
            Department dep = new DepartmentDao(db).GetByKey(deparmtnetCode);

            Location rec = new Location();

            //ロケーションコードが入力されている場合
            if (!string.IsNullOrWhiteSpace(form["LocationCode"]))
            {
                rec = new LocationDao(db).GetByKey(form["LocationCode"]);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dep.CloseMonthFlag) && dep.CloseMonthFlag.Equals("2"))
                {
                    rec = new LocationDao(db).GetListByLocationType("003", form["WarehouseCode"], "").FirstOrDefault();
                }
                else
                {
                    rec = new LocationDao(db).GetListByLocationType("001", form["WarehouseCode"], "").FirstOrDefault();
                }
            }

            return rec;
        }
        #endregion

        /// <summary>
        /// 車両棚卸レコード設定
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="line">車両棚卸明細</param>
        /// <returns></returns>
        /// <history>
        /// 2021/02/22 yano #4081 【車両在庫棚卸】画面から棚卸データ追加時の不具合
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private InventoryStockCar EditInventoryStockCar(FormCollection form, CarInventory line, DateTime inventoryMonth)
        {
            //---------------------------------
            // InventoryStockCarテーブル登録
            //---------------------------------
            InventoryStockCar NewRec = new InventoryStockCar();
            NewRec.InventoryId = Guid.NewGuid();                                            //棚卸ID
            NewRec.DepartmentCode = form["DepartmentCode"];                                 //棚卸部門
            NewRec.InventoryMonth = inventoryMonth;                                         //棚卸月
            NewRec.LocationCode = line.LocationCode;                                        //ロケーションコード
            NewRec.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;             //棚卸ユーザ
            NewRec.SalesCarNumber = line.SalesCarNumber;                                    //管理番号
            NewRec.Vin = line.Vin;                                                          //車台番号
            NewRec.NewUsedType = (string.IsNullOrWhiteSpace(line.NewUsedType) ? line.hdNewUsedType : line.NewUsedType);         //新中区分（新中区分がnullの場合は、隠し項目の値を設定）
            NewRec.CarUsage = line.CarStatus;                                               //利用用途
            NewRec.Quantity = 0;                                                            //数量(デフォルトは0)
            NewRec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;       //作成者
            NewRec.CreateDate = DateTime.Now;                                               //作成日付
            NewRec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;   //作成者
            NewRec.CreateDate = DateTime.Now;                                               //最終更新者
            NewRec.LastUpdateDate = DateTime.Now;                                           //最終更新日付
            NewRec.DelFlag = "0";                                                           //削除フラグ
            NewRec.Summary = line.Summary;                                                  //備考
            NewRec.PhysicalQuantity = line.PhysicalQuantity;                                //実棚
            NewRec.Comment = line.Comment;                                                  //誤差理由
            NewRec.WarehouseCode = form["WarehouseCode"];                                   //倉庫コード

            return NewRec;
        }

        #region 車両棚卸行追加・削除
        /// <summary>
        /// 車両棚卸行追加
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="line">車両棚卸明細</param>
        /// <returns></returns>
        /// <history>
        /// 2019/09/02 yano #3999 【車両棚卸】行追加ボタン押下時の動作の不具合　新規行は１行目に追加
        /// 2017/12/18 arc yano #3840 車両在庫棚卸　実棚数の初期値の変更 実棚数はnullで初期化
        /// 2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更 在庫区分の初期化(在庫)
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private ActionResult AddLine(FormCollection form, EntitySet<CarInventory> line)
        {
            if (line == null)
            {
                line = new EntitySet<CarInventory>();
            }

            CarInventory rec = new CarInventory();

            rec.NewRecFlag = true;

            rec.PhysicalQuantity = null;    //Mod 2017/12/18 arc yano #3840

            rec.CarStatus = "999";           //Add 2017/12/15 arc yano #3839

            //Mod 2019/09/02 yano #3999
            ModelState.Clear();
            line.Insert(0, rec);
            //line.Add(rec);

            PaginatedList<CarInventory> pageList = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            form["EditFlag"] = "true";

            SetComponent(form, pageList);

            return View("CarInventoryCriteria", pageList);
        }

        /// <summary>
        /// 車両棚卸行削除
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">明細データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private ActionResult DelLine(FormCollection form, EntitySet<CarInventory> line)
        {
            ModelState.Clear();

            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01");

            int targetId = int.Parse(form["DelLine"]);

            using (TransactionScope ts = new TransactionScope())
            {
                if (line[targetId].NewRecFlag.Equals(false))
                {
                    //InventoryStockCar rec = new InventoryStockCarDao(db).GetByLocVin(inventoryMonth, line[targetId].LocationCode, line[targetId].Vin);

                    InventoryStockCar rec = new InventoryStockCarDao(db).GetByKey(line[targetId].InventoryId);

                    if (rec != null)
                    {
                        rec.DelFlag = "1";          //削除フラグを立てる
                    }
                }

                try
                {
                    //エンティティの削除処理
                    line.RemoveAt(targetId);

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                    ts.Dispose();
                }
            }

            //再検索を行う
            PaginatedList<CarInventory> pageList = new PaginatedList<CarInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            //PaginatedList<CarInventory> pageList = new PaginatedList<CarInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            SetComponent(form, pageList);

            return View("CarInventoryCriteria", pageList);
        }

        /// <summary>
        /// 車両棚卸行追加行削除（棚卸確定）
        /// </summary>
        /// <param name="targetId">行番号</param>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">明細データ</param>
        /// <returns></returns>
        /// <returns></returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult DelLineDecided(int targetId, FormCollection form, List<CarInventory> line)
        {
            ModelState.Clear();

            using (TransactionScope ts = new TransactionScope())
            {
               
                InventoryStockCar rec = new InventoryStockCarDao(db).GetByKey(line[targetId].InventoryId);

                if (rec != null)
                {
                    rec.DelFlag = "1";          //削除フラグを立てる
                }

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //エンティティの削除処理
                    line.RemoveAt(targetId);
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                    ts.Dispose();
                }
            }

            return DecidedDialog(form, line);
        }

        #endregion

        #region Ajax処理
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>アイドリング状態</returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retParts = new Dictionary<string, string>();

                retParts.Add("ProcessedTime", "アイドリング中…");

                return Json(retParts);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 倉庫コード、対象年月から棚卸開始日時を取得する(Ajax専用）
        /// </summary>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="inventoryMonth">対象年月</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// Add 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult GetStartDate(string warehouseCode, string inventoryMonth)
        {
            if (Request.IsAjaxRequest())
            {
                InventoryScheduleCar condition = new InventoryScheduleCar();
                condition.WarehouseCode = warehouseCode;
                condition.InventoryMonth = DateTime.Parse(inventoryMonth + "/01");

                CodeData codeData = new CodeData();
                InventoryScheduleCar rec = new InventoryScheduleCarDao(db).GetByKey(condition);
                if (rec != null)
                {
                    codeData.Code = "";                          //とりあえず何も設定しない
                    codeData.Name = rec.StartDate == null ? "" : string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        #endregion
    }

}