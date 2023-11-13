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
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
using Microsoft.VisualBasic;	//全角／半角変換用


namespace Crms.Controllers {


    // Mod 2015/04/23 arc yano 部品棚卸不具合対応 部品棚卸画面追加(既設の部品棚卸画面のファイル(現在は未使用)をベースにして作成)
    /// 部品棚卸機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsInventoryController : Controller
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
        public PartsInventoryController() {
            db = new CrmsLinqDataContext();
            
            //Mod 2015/05/26 arc yano タイムアウト値の設定する場所を移動
            //タイムアウト値の設定
            //db.CommandTimeout = 300;
        }

        /// <summary>
        /// ダブルコーテーション置換文字
        /// </summary>
        private readonly string QuoteReplace = "@@@@";

        /// <summary>
        /// カンマ置換文字
        /// </summary>
        private readonly string CommaReplace = "?";

        private static readonly string STS_UNEXCUTED                = "000";            // 未実施(棚卸ステータス)
        private static readonly string STS_INACTION                 = "001";            // 実施中(棚卸ステータス)
        private static readonly string STS_DECIDED                  = "002";            // 確定(棚卸ステータス)
        private static readonly string STS_INVALID                  = "999";            // ステータスエラー(棚卸ステータス)   //Add 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応仕様変更②
        private static readonly string TYP_PARTS                    = "002";            // 部品(棚卸種別)
        private static readonly string FORM_NAME                    = "部品棚卸";       // 画面名
        private static readonly string PROC_NAME_SEARCH             = "部品棚卸検索";   // 処理名（ログ出力用）
        private static readonly string PROC_NAME_INVSTART           = "棚卸開始";       // 処理名(棚卸開始)
        private static readonly string PROC_NAME_EXCELDOWNLOAD      = "Excel出力";      // 処理名(Excel出力)
        private static readonly string PROC_NAME_EXCELIMPORT        = "Excel取込";      // 処理名(Excel取込)
        private static readonly string PROC_NAME_TEMPSTORED         = "一時保存";       // 処理名(一時保存)
        private static readonly string PROC_NAME_INVENTORY_DECIDED  = "棚卸確定";       // 処理名(棚卸確定)
        private static readonly string PROC_NAME_DELLINE            = "行削除";         // 処理名(行削除)            //Add 201707/26 arc yano #3781
        private static readonly string PROC_NAME_DATAEDIT           = "データ編集";     // 処理名(データ保存)        //Add 201707/26 arc yano #3781

        private static readonly int MAX_ERR_CNT = 100;                                  // エラー件数上限数(100件)
     
        /// <summary>
        /// 部品棚卸画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 受払表ステータス取得処理の追加
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 部品棚卸対象フラグによる絞込に変更
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部品在庫棚卸の管理を部門単位から倉庫単位へ変更
        /// </history>
        public ActionResult Criteria() {

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

            //対象年月、部品棚卸作業日
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDateDao(db).GetAllVal();

            //検索結果
            EntitySet<PartsInventory> line = new EntitySet<PartsInventory>();

            //部門(ログインユーザの部門を設定する)
            //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 部品棚卸対象外の部門のログインユーザの場合は部門コードは空欄とする
            //Department department = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode, includeDeleted : false, closeMonthFlag : "2");
            Department department = new DepartmentDao(db).GetByPartsInventory(((Employee)Session["Employee"]).DepartmentCode);
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

            // Add 2015/05/21 arc yano IPO対応(部品棚卸) 確定ボタンクリック可／不可状態追加
            ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "PartsInventoryCommit");
            if (ret.EnableFlag == true)
            {
                form["InventoryEndButtonEnable"] = bool.TrueString;      //棚卸確定ボタン押下できる
            }
            else
            {
                form["InventoryEndButtonEnable"] = bool.FalseString;      //棚卸確定ボタン押下できない
            }


            if (WorkingDate != null)
            {
                form["PartsInventoryWorkingDate"] = string.Format("{0:yyyy/MM/dd}", WorkingDate.InventoryWorkingDate);       //棚卸作業日

                //対象年月がnullの場合は表示しない
                if (WorkingDate.InventoryMonth != null)
                {
                    form["InventoryMonth"] = string.Format("{0:yyyy/MM}", WorkingDate.InventoryMonth);                           //対象年月
                }
                else
                {
                    form["InventoryMonth"] = "";
                    //Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更
                    ModelState.AddModelError("", "棚卸基準日が登録されていません");
                    SetComponent(form);
                    return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
                }
            }
            else
            {
                form["InventoryMonth"] = "";
                form["PartsInventoryWorkingDate"] = "";
                //Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日　に変更
                ModelState.AddModelError("", "棚卸基準日が登録されていません");
                SetComponent(form);
                return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }

            //Mod 2015/06/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日ではなく、棚卸開始日時を表示するように変更
            //----------------
            //棚卸開始日時
            //----------------
            InventoryScheduleParts condition = new InventoryScheduleParts();
            condition.InventoryMonth = (DateTime)WorkingDate.InventoryMonth;
            //Mod 2016/08/13 arc yano #3596
            //condition.Department = new Department();
            //condition.Department.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.WarehouseCode  = form["WarehouseCode"];       // ADD 2016/08/13 arc yano #3596

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);

            if (rec != null)
            {
                form["PartsInventoryStartDate"] = rec.StartDate == null ? "": string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
            }

            //Add 2017/07/26 arc yano #3781
            //受払表作成ステータスを取得
            InventoryMonthControlPartsBalance pbrec = new InventoryMonthControlPartsBalanceDao(db).GetByKey(string.Format("{0:yyyyMMdd}", WorkingDate.InventoryMonth));
            if (pbrec != null)
            {
                form["InventoryStatusPartsBalance"] = pbrec.InventoryStatus;
            }
            else
            {
                form["InventoryStatusPartsBalance"] = "";
            }

            return Criteria(form, line);
        }

        /// <summary>
        /// 部品棚卸画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応  棚卸の管理を部門単位から倉庫単位へ変更
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, EntitySet<PartsInventory> line)
        {
            ActionResult ret;

            db = new CrmsLinqDataContext();
			
			//Mod 2015/05/29 arc yano IPO対応(部品棚卸) タイムアウト設定する場所とタイムアウト値の変更(300→600秒)
            //タイムアウト値の設定
            db.CommandTimeout = 600;

            db.Log = new OutputWriter();

            //Mod 2016/08/13 arc yano #3596
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
            	//Mod 2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④ 棚卸作業日⇒棚卸基準日に変更
                ModelState.AddModelError("", "棚卸基準日が登録されていないため、棚卸を開始できません");
                line = new EntitySet<PartsInventory>();
                SetComponent(form);
                return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }

            //ReuquestFlagによる処理の振分け
            switch (form["RequestFlag"])
            {
                case "1":   //棚卸開始
                    ret = InventoryStart(form, inventoryMonth);
                    break;

                case "2":   //Excel出力
                    ret = Download(form, line, inventoryMonth);
                    break;

                case "3":   //棚卸作業結果一時保存
                    ret = TemporalilyStored(form, line, inventoryMonth);
                    break;

                case "4":   //棚卸確定
                    ret = InventoryDecided(form, line, inventoryMonth);
                    break;

                default:    //検索またはページング
                    //ページング
                    if (form["RequestFlag"].Equals("999"))
                    {
                        if(string.IsNullOrWhiteSpace(form["DepartmentCode"])){
                            ModelState.AddModelError("", "部門コードを入力してください");
                            SetComponent(form);
                            return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
                        }

                        //棚卸状況が実施中の場合で、編集中フラグがTrueの場合
                        if (GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth).Equals(STS_INACTION) && bool.Parse(form["EditFlag"]) == true)
                        {
                            ret = TemporalilyStored(form, line, inventoryMonth);
                        }
                    }

                    //検索処理実行
                    ret = SearchList(form, inventoryMonth);
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
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部品在庫棚卸の管理を部門単位から倉庫単位へ変更
        /// </history>
        [AuthFilter]
        private ActionResult SearchList(FormCollection form, DateTime inventoryMonth)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ModelState.Clear();
            
            //検索結果初期化
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //倉庫毎棚卸状況をチェック
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);       //Mod 2016/08/13 arc yano #3596

            // Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 部品棚卸対象外の部門のユーザの場合は部門コードを空欄にする
            // 検索結果リストの取得
            if ((criteriaInit == true) && ((status == STS_UNEXCUTED || status == STS_INVALID ) ) )    //初期表示時、かつ棚卸未実施の場合
            {
                //何もしない
            }
            else
            {
                //検索処理
                list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }

            //画面設定
            SetComponent(form);

            return View("PartsInventoryCriteria", list);
        }
        
        /// <summary>
        /// 検索実行
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>部品棚卸一覧</returns>
        private IQueryable<PartsInventory> GetSearchResultList(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            InventoryStock InventoryStockCondition = SetCondition(form, inventoryMonth);

          　//検索実行
            return new InventoryStockDao(db).GetListByCondition(InventoryStockCondition);
        }

        /// <summary>
        /// 検索条件設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件に倉庫コードを追加
        /// </history>
        private InventoryStock SetCondition(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //　検索項目の設定
            //---------------------
            InventoryStock Condition = new InventoryStock();
            Condition.InventoryMonth = inventoryMonth;
            Condition.DepartmentCode = form["DepartmentCode"];
            Condition.LocationCode = form["LocationCode"];
            Condition.LocationName = form["LocationName"];
            Condition.PartsNumber = form["PartsNumber"];
            Condition.PartsNameJp = form["PartsNameJp"];
            Condition.InventoryType = TYP_PARTS;                                                                     //棚卸種別 = 「部品」
            Condition.DiffQuantity = bool.Parse(form["DiffQuantity"].Contains("true") ? "true": "false");            //棚卸有無

            Condition.WarehouseCode = form["WarehouseCode"];                                                         //倉庫コード    //Add 2016/08/13 arc yano #3596


            //検索実行
            return Condition;
        }
        #endregion

        #region 棚卸作業
        /// <summary>
        /// 棚卸開始
        /// </summary>
        /// <param name="form"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns>検索処理された画面</returns
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        [AuthFilter]
        private ActionResult InventoryStart(FormCollection form, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVSTART);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //棚卸開始チェック
            ValidationForInventoryStart(form, inventoryMonth);
            if (!ModelState.IsValid)
            {
                //画面項目の設定
                SetComponent(form);
                return View("PartsInventoryCriteria", list); 
            }

            CodeDao dao = new CodeDao(db);

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockテーブルのレコード作成
                //------------------------------------------------------
                //InventoryStockテーブルはストアドプロシージャで更新する。
                //var ret = db.InsertInventoryStock(inventoryMonth, form["DepartmentCode"], ((Employee)Session["Employee"]).EmployeeCode);
                var ret = db.InsertInventoryStock(inventoryMonth, form["WarehouseCode"], ((Employee)Session["Employee"]).EmployeeCode);

                //------------------------------------------------------
                //InventorySchedulePartsのレコード作成
                //------------------------------------------------------

                InventoryScheduleParts rec = new InventoryScheduleParts();

                //rec.DepartmentCode = form["DepartmentCode"];    //部門コード
                rec.DepartmentCode = "";                          //部門コード
                rec.InventoryMonth = inventoryMonth;
                rec.InventoryType = TYP_PARTS;
                rec.InventoryStatus = STS_INACTION; //実施中
                rec.StartDate = DateTime.Now;
                rec.EndDate = null;
                rec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                rec.WarehouseCode = form["WarehouseCode"];      //Add 2016/08/13 arc yano #3596

                //InventorySchedulePartsにレコード追加
                db.InventoryScheduleParts.InsertOnSubmit(rec);

                //-------------------------------------------------
                //コミット処理
                //-------------------------------------------------
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
                            return View("PartsInventoryCriteria", list);
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

            //画面項目の設定
            SetComponent(form);

            //検索を実行する
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return View("PartsInventoryCriteria", list);
        }

        /// <summary>
        /// 一時保存
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">棚卸リスト(1ページ分)</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult TemporalilyStored(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_TEMPSTORED);

            ModelState.Clear();
            
            //検索結果の設定
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //部品在庫棚卸情報無し
            if (line == null)
            {
                ModelState.AddModelError("", "部品在庫棚卸情報が0件のため、更新できません");
            }

            //棚卸ステータスチェック
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                return View("PartsInventoryCriteria", list);
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockテーブルのレコード更新
                //------------------------------------------------------
                //棚卸月、倉庫コードをキーに棚卸情報を取得
                List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);
                //List<InventoryStock> isList = new InventoryStockDao(db).GetListByInvnentoryMonthDepartment(inventoryMonth, form["DepartmentCode"]);   //Mod 2016/08/13 arc yano #3596

                for (int k = 0; k < line.Count; k++)
                {
                    //画面で入力した実棚、コメントをInventoryStockテーブルに保存
                    //InventoryStock ivs = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[k].LocationCode, line[k].PartsNumber);
                    InventoryStock ivs = isList.Where(x => x.LocationCode.Equals(line[k].LocationCode) && x.PartsNumber.Equals(line[k].PartsNumber)).FirstOrDefault();

                    if (ivs != null)
                    {
                        //実棚数                        
                        if (ivs.PhysicalQuantity != line[k].PhysicalQuantity)
                        {
                            ivs.PhysicalQuantity = line[k].PhysicalQuantity;
                        }

                        //コメント
                        if (!string.IsNullOrWhiteSpace(ivs.Comment) || !string.IsNullOrWhiteSpace(line[k].Comment))
                        {
                            if (!string.IsNullOrWhiteSpace(ivs.Comment) && !string.IsNullOrWhiteSpace(line[k].Comment))
                            {
                                if (!ivs.Comment.Equals(line[k].Comment))
                                {
                                    ivs.Comment = line[k].Comment;
                                }
                            }
                            else
                            {
                                ivs.Comment = line[k].Comment;
                            }
                        }
                    }
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
                            SetComponent(form);
                            return View("PartsInventoryCriteria", list);
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
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            SetComponent(form);

            return View("PartsInventoryCriteria", list);
        }

        /// <summary>
        /// 棚卸確定
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        ///  2016/03/17 arc yano #3477 部品ロケーションマスタ　ロケーションマスタの自動更新 棚卸確定時に部品ロケーションマスタに反映するように修正
        /// </history>
        [AuthFilter]
        private ActionResult InventoryDecided(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //初期処理
            //-----------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVENTORY_DECIDED);

            //検索結果の設定
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validationチェック
            //-----------------------------
            //棚卸ステータスチェック
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                //画面項目の設定
                SetComponent(form);
                return View("PartsInventoryCriteria",  list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }
			
			//2015/05/14 arc yano IPO(部品棚卸)対応 Excel取込後、検索せずに確定処理を行うとシステムエラーの対応
            //-----------------------------
            //各パラメータの設定
            //-----------------------------

            //社員コード
            string employeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //部門コード
            //string departmentCode = form["DepartmentCode"];

            string warehouseCode = form["WarehouseCode"];               //Add 2016/08/13 arc yano #3596
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
            //Mod 2016/08/13 arc yano #3596
            List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);

                    for (int k = 0; k < line.Count; k++)
                    {
                        //画面で入力した実棚、コメントをInventoryStockテーブルに保存
                        InventoryStock ivs = isList.Where(x => x.LocationCode.Equals(line[k].LocationCode) && x.PartsNumber.Equals(line[k].PartsNumber)).FirstOrDefault();

                        if (ivs != null)
                        {
                            //実棚数                        
                            if (ivs.PhysicalQuantity != line[k].PhysicalQuantity)
                            {
                                ivs.PhysicalQuantity = line[k].PhysicalQuantity;
                            }

                            //コメント
                            if (!string.IsNullOrWhiteSpace(ivs.Comment) || !string.IsNullOrWhiteSpace(line[k].Comment))
                            {
                                if (!string.IsNullOrWhiteSpace(ivs.Comment) && !string.IsNullOrWhiteSpace(line[k].Comment))
                                {
                                    if (!ivs.Comment.Equals(line[k].Comment))
                                    {
                                        ivs.Comment = line[k].Comment;
                                    }
                                }
                                else
                                {
                                    ivs.Comment = line[k].Comment;
                                }
                            }
                        }
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
                            return View("PartsInventoryCriteria", list);
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
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {               
                //------------------------------------------------------
                //inventoryStockテーブル,PartsStockの更新
                //------------------------------------------------------
                //Mod 2016/08/13 arc yano #3596 
                //ストプロで実行する。
                var ret = db.InventoryDecided(inventoryMonth, warehouseCode, employeeCode);     //Mod 2016/08/13 arc yano #3596
                //var ret = db.InventoryDecided(inventoryMonth, departmentCode, employeeCode);

                //Mod Mod 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧ 仕掛データの抽出をビューテーブルからストアドに変更
                //------------------------------------------------------
                //inventoryParts_Shikakariの作成
                //------------------------------------------------------
                //ストプロで実行する。
                //var ret2 = db.Insert_InventoryParts_Shikakari(inventoryMonth.Date, departmentCode);
                //var ret2 = db.GetPartsWipStock(1 , inventoryMonth.Date, "departmentCode", "", "", "", "", "", "", "", "");
                var ret2 = db.GetPartsWipStock(1, inventoryMonth.Date, "", warehouseCode, "", "", "", "", "", "", "", "");  //Mod 2016/08/13 arc yano #3596

                //------------------------------------------------------
                //PartsLocationの反映 //Mod 2016/03/17 arc yano #3477
                //------------------------------------------------------
                //ストプロで実行する。
                //var ret3 = db.InsertPartsLocation(departmentCode, employeeCode);
                var ret3 = db.InsertPartsLocation(warehouseCode, employeeCode);     //Mod 2016/08/13 arc yano #3596

                //------------------------------------------------------
                //InventoryShcheduleの更新
                //------------------------------------------------------
                //InventoryScheduleParts isrec = new InventorySchedulePartsDao(db).GetByKey(form["DepartmentCode"], inventoryMonth, TYP_PARTS); //Mod 2016/08/13 arc yano #3596
                InventoryScheduleParts isrec = new InventorySchedulePartsDao(db).GetByKey(form["WarehouseCode"], inventoryMonth, TYP_PARTS);

                if (isrec != null)
                {
                    isrec.InventoryStatus = STS_DECIDED;                                                //ステータスを「完了」に更新
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
                            return View("PartsInventoryCriteria", list);
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

            //Add 2015/05/25 IPO対応(部品棚卸) 確定後、反映した値で再検索を実行するため、モデルを一度クリアする
            //一度ModelStateをクリア
            ModelState.Clear();

            //メッセージ設定
            ModelState.AddModelError("", "棚卸を確定しました");

            //再検索を実行する
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            //画面コンポーネントの設定
            SetComponent(form);
            
            return View("PartsInventoryCriteria", list);
        }

        #endregion

        //2015/06/09 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ Excelファイルのテンプレート化
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">棚卸一覧(1ページ分)</param>
        /// <param name="inventoryMonth">棚卸月</param>
        private ActionResult Download(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-------------------------------
            //初期処理
            //-------------------------------
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //検索結果の設定
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();
           
            //検索一覧を表示していた場合は表示した結果を設定する
            if (line != null)
            {
                list = new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            } 

            //-------------------------------
            //Excel出力処理
            //-------------------------------

            //ファイル名(PartsInventory_xxx(部門コード)_yyyyMM(対象年月)_yyyyMMddhhmiss(ダウンロード時刻))
            string fileName = "PartsInventory" + "_" + form["DepartmentCode"] + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsInventory"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsInventory"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetComponent(form);
                return View("PartsInventoryCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, inventoryMonth, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("PartsInventoryCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        //Mod 2015/06/09 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ Excelファイルのテンプレート化
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, DateTime inventoryMonth, string fileName, string tfileName)
        {
            
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            //string sheetName = "PartsInventory";    //シート名
            //int dateType = 0;                       //データタイプ(帳票形式)
            //string setPos = "A1";                   //設定位置
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
                //excelData = excelFile.GetAsByteArray();
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
            
            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";

            //検索条件取得
            InventoryStock condition = SetCondition(form, inventoryMonth);

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(condition);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            configLine.SetPos[0] = "A3";

            //棚卸情報の取得
            List<PartsInventory> list = GetSearchResultList(form, inventoryMonth).ToList();

            //データ設定
            ret = dExport.SetData<PartsInventory, PartsInventoryForExcel>(ref excelFile, list, configLine);
            
            //Mod 2015/07/26 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 理論在庫の単価、金額追加
            //計算式再設定
            worksheet.Cells[1, 9].Formula = worksheet.Cells[1, 9].Formula;
 
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
        /// フィールド定義データから出力フィールドリストを取得する
        /// </summary>
        /// <param name="documentName">帳票名</param>
        /// <returns></returns>
        private IEnumerable<XElement> GetFieldList(string documentName)
        {
            XDocument xml = XDocument.Load(Server.MapPath("/Models/ExportFieldList.xml"));
            var query = (from x in xml.Descendants("Title")
                         where x.Attribute("ID").Value.Equals(documentName)
                         select x).FirstOrDefault();
            if (query == null)
            {
                return null;
            }
            else
            {
                var list = from a in query.Descendants("Name") select a;

                return list;
            }
        }

        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 【大項目】部門棚統合対応 倉庫の検索条件を追加
        /// </history>
        private DataTable MakeConditionRow(InventoryStock condition)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            string departmentName = new DepartmentDao(db).GetByKey(condition.DepartmentCode, false).DepartmentName;

            //倉庫名取得
            string warehouseName = new WarehouseDao(db).GetByKey(condition.WarehouseCode).WarehouseName;        //Add 2016/08/13 arc yano #3596

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
            //部品番号
            if (!string.IsNullOrEmpty(condition.PartsNumber))
            {
                conditionText += string.Format("部品番号={0}　", condition.PartsNumber);
            }
            //部品名
            if (!string.IsNullOrEmpty(condition.PartsNameJp))
            {
                conditionText += string.Format("部品名={0}　", condition.PartsNameJp);
            }
            //棚差有無
            if (condition.DiffQuantity == true)
            {
                conditionText += string.Format("棚差有無=棚差があるレコードのみ表示　", condition.DiffQuantity);
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);   
         
            return dt;
        }

        /// <summary>
        /// ヘッダ行の作成(Excel出力用)
        /// </summary>
        /// <param name="list">列名リスト</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            
            //データテーブルにxmlの値を設定する
            int i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }

        #region 棚卸ステータス取得
        // 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部品在庫の棚卸の管理を部門毎→倉庫毎に変更
        /// <summary>
        /// 棚卸ステータスの返却(倉庫毎)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸ステータス</returns>
        private string GetInventoryStatusByWarehouse(string warehouseCode, DateTime inventoryMonth)
        {

            string ret = STS_UNEXCUTED;     //デフォルトは「未実施」

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(warehouseCode, inventoryMonth, TYP_PARTS);

            //対象月のレコードあり
            if (rec != null)
            {
                if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_INACTION)))
                {
                    ret = STS_INACTION;     //実施中
                }
                else
                {
                    ret = STS_DECIDED;     //完了
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
        /*
        /// <summary>
        /// 棚卸ステータスの返却(部門毎)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸ステータス</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成
        /// </history>
        private string GetInventoryStatusByDepartment(string departmentCode, DateTime inventoryMonth)
        {

            string ret = STS_UNEXCUTED;     //デフォルトは「未実施」

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(departmentCode, inventoryMonth, TYP_PARTS);

            //対象月のレコードあり
            if (rec != null)
            {
                if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_INACTION)))
                {
                    ret = STS_INACTION;     //実施中
                }
                else
                {
                    ret = STS_DECIDED;     //完了
                }
            }
            //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 部門コードが空欄の場合はステータスを無効とする。
            else
            {
                //部門コードがnullの場合は
                if (string.IsNullOrWhiteSpace(departmentCode))
                {
                    ret = STS_INVALID;      //ステータス無効
                }
            }

            return ret;
        }
        */

        #endregion

        
        #region Validationチェック
        /// <summary>
        /// 棚卸開始時のValidationチェック
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void ValidationForInventoryStart(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //部門毎棚卸状況をチェック
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            //棚卸ステータス=「実施中」
            if (status.Equals(STS_INACTION))
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の部品棚卸を実施中であるため、棚卸を開始できません");
            }
            else if (status.Equals(STS_DECIDED))
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の部品棚卸を終了しているため、棚卸を開始できません");
            }
            //--------------------------------
            //作業日のチェック
            //--------------------------------
            DateTime inventoryWorkingDate;

            inventoryWorkingDate = DateTime.Parse(form["PartsInventoryWorkingDate"]);
            //当日日付が作業日前の場合はエラー
            if (DateTime.Now.Date.CompareTo(inventoryWorkingDate) < 0)
            {
                ModelState.AddModelError("", "棚卸基準日になっていないため、棚卸を開始できません");
            }

            return;
        }

        /// <summary>
        /// 棚卸状況のValidationチェック
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void ValidateInventoryStatus(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //部門毎棚卸状況をチェック
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            //棚卸ステータス=「完了」
            if (status.Equals(STS_DECIDED))
            {
                ModelState.AddModelError("", "倉庫" + form["WarehouseName"] + "の対象年月の部品棚卸は終了しているため、更新できません。");
            }

            return;            
        }

        /// <summary>
        /// 棚卸棚卸編集時のvalidationチェック
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 新規作成
        /// </history>
        private void ValidateForDataEdit(FormCollection form, EntitySet<PartsInventory> line)
        {
            int cnt = 0;

            bool msg = false;


            //棚卸月
            DateTime inventorymonth = DateTime.Parse(form["InventoryMonth"]);

            if (line == null || line.Count == 0)
            {
                ModelState.AddModelError("", "明細が0行のため、保存できません");

                //即リターン
                return;
            }

            foreach (var a in line)
            {
                ///------------------------
                // 必須チェック
                //------------------------           
                //ロケーションコード
                if (string.IsNullOrWhiteSpace(a.LocationCode))
                {
                    ModelState.AddModelError("line[" + cnt + "].LocationCode", MessageUtils.GetMessage("E0001", cnt + 1 + "行目のロケーションコード"));
                }
                else
                {
                    Location loc = new LocationDao(db).GetByKey(a.LocationCode);

                    Warehouse warehouse = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);


                    //別の倉庫のロケーションコードが入力されている場合は
                    if (!loc.WarehouseCode.Equals(form["WarehouseCode"]))
                    {
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", warehouse.WarehouseName + "のロケーションコードを入力して下さい");
                    }
                }
                //部品番号
                if (string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("line[" + cnt + "].PartsNumber", MessageUtils.GetMessage("E0001", cnt + 1 + "行目の部品番号"));
                }
                //----------------------------------------------
                // 引当済数が数量を上回っていないかのチェック
                //----------------------------------------------
                if (a.PhysicalQuantity - a.ProvisionQuantity < 0)
                {
                    ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "数量より多い引当済数を設定できません");
                }
                //---------------------------------------------
                // 重複チェック
                //---------------------------------------------
                if (a.NewRecFlag.Equals(true))  //新規追加したレコード
                {
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventorymonth, a.LocationCode, a.PartsNumber, false);

                    //レコード存在する場合はvalidationエラー
                    if (rec != null)
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "ロケーションコード=" + line[cnt].LocationCode + ": 部品番号=" + line[cnt].PartsNumber + "は既に登録されています");
                    }
                }
                //---------------------------------------------
                // 重複チェック２（同一画面内に複数存在）
                //---------------------------------------------
                int count = line.Where(x => x.LocationCode.Equals(a.LocationCode) && x.PartsNumber.Equals(a.PartsNumber)).Count();

                if (count > 1)
                {
                    if (msg == false)
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "ロケーションコード=" + line[cnt].LocationCode + ": 部品番号=" + line[cnt].PartsNumber + "が編集画面上に複数行存在します");
                        msg = true;
                    }
                    else
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                    }
                    
                }


                cnt++;
            }

           

            return;
        }

        #endregion
        
        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 受払表ステータスの設定の追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            DateTime inventoryMonth;
            
            //項目の設定
            ViewData["InventoryMonth"] = form["InventoryMonth"];                                                            //対象年月
            ViewData["PartsInventoryWorkingDate"] = form["PartsInventoryWorkingDate"];                                      //棚卸作業日
            ViewData["DepartmentCode"] = form["DepartmentCode"];                                                            //部門コード
            
            //部門名
            if (form["DepartmentCode"] == null || string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                ViewData["DepartmentName"] = "";
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;
            }

            //Add 2016/08/13 arc yano #3596
            //倉庫コード
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            //倉庫名
            ViewData["WarehouseName"] = form["WarehouseName"];

            ViewData["LocationCode"] = form["LocationCode"];                                                                //ロケーションコード
            ViewData["LocationName"] = form["LocationName"];                                                                //ロケーション名
            ViewData["PartsNumber"] = form["PartsNumber"];                                                                  //部品番号
            ViewData["PartsNameJp"] = form["PartsNameJp"];                                                                  //部品名(日本語)
            ViewData["DiffQuantity"] = form["DiffQuantity"];                                                                //棚差有無
            ViewData["RequestFlag"] = "999";                                                                                //処理種別 ※デフォルト「ページング」に設定

            ViewData["id"] = form["id"];

            //棚卸状況ステータス(部門毎)
            if (true == DateTime.TryParse(ViewData["InventoryMonth"].ToString() + "/01", out inventoryMonth))
            {
                ViewData["InventoryStatus"] = GetInventoryStatusByWarehouse(ViewData["WarehouseCode"].ToString(), DateTime.Parse(ViewData["InventoryMonth"].ToString() + "/01"));
            }
            else
            {
                ViewData["InventoryStatus"] = STS_UNEXCUTED;    //未実施
            }
            
            // Add 2015/05/21 arc yano IPO対応(部品棚卸) 確定ボタンクリック可／不可状態追加
            //確定ボタンクリック可／不可状態
            ViewData["InventoryEndButtonEnable"] = bool.Parse(form["InventoryEndButtonEnable"]);

            //Mod 2015/06/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日ではなく、棚卸開始日に変更
            ViewData["PartsInventoryStartDate"] = form["PartsInventoryStartDate"];

            //Add 2017/07/26 arc yano #3781
            ViewData["InventoryStatusPartsBalance"] = form["InventoryStatusPartsBalance"];

            return;
        }
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 新規作成
        /// </history>
        private void SetComponentDataEdit(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //項目の設定
            ViewData["InventoryMonth"] = form["InventoryMonth"];                                                            //対象年月
            ViewData["WarehouseCode"] = form["WarehouseCode"];                                                              //倉庫コード
            ViewData["RequestFlag"] = "99";                                                                                 //処理種別 ※デフォルト「検索」に設定

            return;
        }

        #endregion

        // <summary>
        /// 編集画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）新規作成
        /// </history>
        public ActionResult DataEdit()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<PartsInventory> line = new EntitySet<PartsInventory>();

            //初期値の設定
            form["RequestFlag"] = "99";                                              // 99(リクエストの種類は「検索」)

            form["CreateFlag"] = Request["CreateFlag"];                              //新規作成フラグ

            form["InventoryMonth"] = Request["InventoryMonth"] + "/01";              //棚卸対象年月日
            form["WarehouseCode"] = Request["WarehouseCode"];                        //倉庫コード
            form["LocationCode"] = Request["LocationCode"];                          //ロケーションコード
            form["PartsNumber"] = Request["PartsNumber"];                            //部品番号

            return DataEdit(form, line);
        }

        /// <summary>
        /// データ編集保存
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DataEdit(FormCollection form, EntitySet<PartsInventory> line)
        {
            ModelState.Clear();

            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"]);

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            List<PartsInventory> list = new List<PartsInventory>();

            //新規作成の場合
            if (!string.IsNullOrWhiteSpace(form["CreateFlag"]) && form["CreateFlag"].Equals("1"))
            {
                ret = AddLine(form, line);
            }
            //更新の場合
            else
            {
                switch (form["RequestFlag"])
                {
                    case "10": //保存

                        //入力値のチェック
                        ValidateForDataEdit(form, line);

                        if (!ModelState.IsValid)
                        {
                            //画面設定
                            SetComponentDataEdit(form);
                            
                            if (line != null && line.Count > 0)
                            {
                                list = line.ToList();
                            }
                            
                            return View("PartsInventoryDataEdit", list);
                        }

                        //保存処理
                        ret = DataSave(form, line);
                        break;

                    case "11": //行追加

                        ret = AddLine(form, line);
                        break;

                    case "12": //行削除

                        ret = DelLine(form, line);
                        break;

                    default: //検索処理

                        InventoryStock condition = new InventoryStock();

                        condition.InventoryMonth = inventoryMonth;
                        condition.DepartmentCode = form["DepartmentCode"];
                        condition.LocationCode = form["LocationCode"];
                        condition.LocationName = form["LocationName"];
                        condition.PartsNumber = form["PartsNumber"];
                        condition.PartsNameJp = form["PartsNameJp"];
                        condition.InventoryType = TYP_PARTS;

                        condition.WarehouseCode = new LocationDao(db).GetByKey(form["LocationCode"]).WarehouseCode;

                        list = new InventoryStockDao(db).GetListByCondition(condition).ToList<PartsInventory>();

                        //list = GetSearchResultList(form, inventoryMonth).Where(x => x.LocationCode.Equals(form["LocationCode"]) && x.PartsNumber.Equals(form["PartsNumber"])).ToList<PartsInventory>();
                        ret = View("PartsInventoryDataEdit", list);
                        break;
                }
            }

            //画面設定
            SetComponentDataEdit(form);

            //コントロールの有効無効
            //GetViewResult(form);

            return ret;
        }

        /// <summary>
        /// 棚卸在庫データ保存処理
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano 棚卸在庫（部品在庫の修正）#3781 新規作成
        /// </history>
        private ActionResult DataSave(FormCollection form, EntitySet<PartsInventory> line)
        {
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"]);


            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCarテーブルのレコード更新
                //------------------------------------------------------
                List<InventoryStock> isList = new List<InventoryStock>();

                for (int k = 0; k < line.Count; k++)
                {
                    //ロケーションコード、部品番号から部品在庫棚卸テーブルを取得する
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[k].LocationCode, line[k].PartsNumber, true);

                    if (rec != null)
                    {
                        rec.PhysicalQuantity = line[k].PhysicalQuantity;                                //実棚数
                        rec.ProvisionQuantity = line[k].ProvisionQuantity;                              //引当済数
                        rec.Comment = line[k].Comment;                                                  //コメント
                        rec.DelFlag = "0";                                                              //削除フラグは常に有効にしておく
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;      //最終更新者
                        rec.LastUpdateDate = DateTime.Now;                                              //最終更新日
                    }
                    else
                    {
                        InventoryStock inventorystock = new InventoryStock();
                        inventorystock.InventoryId = Guid.NewGuid();
                        inventorystock.DepartmentCode = "";
                        inventorystock.InventoryMonth = inventoryMonth;
                        inventorystock.LocationCode = line[k].LocationCode;                                                     //ロケーションコード
                        inventorystock.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                             //担当者コード
                        inventorystock.InventoryType = "002";                                                                   //棚卸種別=「部品」
                        inventorystock.SalesCarNumber = null;                                                                   //車両管理番号
                        inventorystock.PartsNumber = line[k].PartsNumber;                                                       //部品番号
                        inventorystock.Quantity = line[k].PhysicalQuantity;                                                     //理論数(実棚数で登録)
                        inventorystock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    　　　             //作成者
                        inventorystock.CreateDate = DateTime.Now;                                                               //作成日
                        inventorystock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                   //最終更新者
                        inventorystock.LastUpdateDate = DateTime.Now;                                                           //最終更新日
                        inventorystock.DelFlag = "0";                                                                           //削除フラグ
                        inventorystock.Summary = "";                                                                            //備考
                        inventorystock.PhysicalQuantity = line[k].PhysicalQuantity;                                             //実棚数
                        inventorystock.Comment = line[k].Comment;                                                               //コメント
                        inventorystock.ProvisionQuantity = line[k].ProvisionQuantity;                                           //引当済数
                        inventorystock.WarehouseCode = new LocationDao(db).GetByKey(line[k].LocationCode, false).WarehouseCode; //倉庫コード

                        isList.Add(inventorystock);
                    }

                    //保存後は新規作成フラグを落とす
                    if (line[k].NewRecFlag.Equals(true))
                    {
                        line[k].NewRecFlag = false;
                    }
                }

                db.InventoryStock.InsertAllOnSubmit(isList);

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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_DATAEDIT, FORM_NAME, "");
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
                            OutputLogger.NLogError(se, PROC_NAME_DATAEDIT, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                            ts.Dispose();
                            return View("PartsInventoryDataEdit", line.ToList());
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(se, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME_DATAEDIT, FORM_NAME, "");
                        ts.Dispose();
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            ModelState.AddModelError("", "保存しました");

            return View("PartsInventoryDataEdit", line.ToList());
        }


        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <returns>Excel取込用ダイアログ</returns>
        [AuthFilter]
        public ActionResult ImportDialog()
        {

            ViewData["ElapsedHours"] = String.Format("{0:00}", 0);
            ViewData["ElapsedMinutes"] = String.Format("{0:00}", 0);
            ViewData["ElapsedSeconds"] = String.Format("{0:00}", 0);

            return View("PartsInventoryImportDialog");
        }

        /// <summary>
        ///  Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 Excel読取メソッドの引数追加
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            //---------------------------
            //初期化
            //---------------------------
            ModelState.Clear();

            //エクセルデータ格納用(1行分)
            PartsInventory data = new PartsInventory();

            //カラム番号保存用
            int[] colNumber;
            colNumber = new int[4] { -1, -1, -1, -1 };

            //ロケーションコード＋部品番号リスト(重複チェック用)
            List<string> olChkList = new List<string>();
           
            //ストップウォッチ生成
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //ストップウォッチを開始する
            sw.Start();

            //---------------------------
            // ファイルの存在チェック
            //--------------------------
             // ファイルの存在チェック
            ValidateImportFile(importFile);
            
            if (!ModelState.IsValid)
            {
                sw.Stop();
                ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
                ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
                ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
                return View("PartsInventoryImportDialog");
            }

            //---------------------------
            // Excelデータ読取
            //--------------------------
            int dataRowCnt = ReadExcelData(importFile, form);

            if (!ModelState.IsValid)
            {
                sw.Stop();
                ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
                ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
                ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
                return View("PartsInventoryImportDialog");
            }

            //---------------------------
            // DB更新
            //---------------------------
            int ret = DBExecute();

            //ストップウォッチを止める
            sw.Stop();

            if (ret == -1)
            {
                //エラー画面に遷移
                return View("Error");
            }
            
            //Mod 2015/05/14 arc yano IPO(部品棚卸)対応 正常終了した場合のみ、完了メッセージを表示
            //正常終了時はExcelデータの取込完了のメッセージを表示する。
            if (ret == 0)
            {
                //エラー画面に遷移
            //Mod 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更②取込件数を表示
            ModelState.AddModelError("", "Excelの取込を完了しました。取込件数は" + dataRowCnt + "件です");
            }

            ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
            ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
            ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
            return View("PartsInventoryImportDialog", ViewData);
        }

        /// <summary>
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>取り込んだ件数(エラーの場合は-1)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部品棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/06/11 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ Exlcelファイルテンプレート化対応
        ///                                       また、データ件数をエクセルの入力値から取得するように修正。
        /// 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② Excelで取込件数を返すように修正
        /// </history>
        private int ReadExcelData(HttpPostedFileBase importFile, FormCollection form)
        {

            //------------------------
            // 初期化
            //------------------------
            //返却値
            int ret = -1;       //エラーで初期化

            int dataLowCnt = 0; //データ取込件数
            
            //エクセルの検索条件行
            String conditionText = "";

            //部門コード
            string departmentCode = "";
            //対象年月
            DateTime inventoryMonth = DateTime.Parse("1900/01/01");     //適当な値を入れておく

            ConfigLine configLine;          //設定値           //Add 2015/06/11

            //カラム番号保存用
            int[] colNumber;
            colNumber = new int[4] { -1, -1, -1, -1 };

            //ロケーションコード＋部品番号リスト(重複チェック用)
            List<string> dpChkList = new List<string>();

            //ロケーションコードリスト
            List<string> sLocList = new List<string>();

            //Mod 2015/05/14 arc yano IPO(部品棚卸)対応 Excel取込時重複エラー対応(マスタチェック用のリスト追加)
            //マスタチェック用
            List<string> sLocList2 = new List<string>();

            //部品番号リスト
            List<string> sPartsList = new List<string>();

            //Mod 2015/05/14 arc yano IPO(部品棚卸)対応 Excel取込時重複エラー対応(マスタチェック用のリスト追加)
            //マスタチェック用
            List<string> sPartsList2 = new List<string>();

            //------------------------
            //Excel取込処理
            //------------------------
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
                        return ret;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ret;
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
                    ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");
                    return ret;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets[configLine.SheetName];


                //Mod 2015/07/26 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑦ 理論在庫の単価、金額追加
                //データ行数取得
                int rowCnt = ws.Cells[1, 9].GetValue<int>();

                //------------------------------
                //取込行が0件の場合
                //------------------------------
                if ((ws.Dimension == null) || (rowCnt == 0))
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ret;
                }

                //------------------------------
                //検索条件行取得
                //------------------------------
                //一番最初の行が検索条件行とする
                int row = ws.Dimension.Start.Row;
                int column = ws.Dimension.Start.Column;

                conditionText = ws.Cells[row, column].Text;

                //部門コード、対象年月のチェック
                ExtractInf(conditionText, ref departmentCode, ref inventoryMonth);

                //部門コードから倉庫を割り出す
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

                string warehouseCode = (dWarehouse != null ? dWarehouse.WarehouseCode : "");

                //------------------------------
                //タイトル行取得(2行目)
                //------------------------------
                //検索条件行の次の行がタイトル行
                row++;
                var headerRow = ws.Cells[row, column, row, ws.Dimension.End.Column];

                colNumber = SetColNumber(headerRow, colNumber);

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ret;
                }

                //------------------------------
                //データ行取得(3行目)
                //------------------------------
                //部品棚卸リストの取得
                //List<InventoryStock> isList = new InventoryStockDao(db).GetListByInvnentoryMonthDepartment(inventoryMonth, departmentCode);   //Mod 2016/08/13 arc yano #3596
                List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, warehouseCode);


                //Mod 2016/08/13 arc yano #3596
                //Mod 2015/05/14 arc yano IPO(部品棚卸)対応 パフォーマンス改善のため、クエリ変更
                //ロケーションコードリストの取得
                sLocList = (
                               from a in db.Location
                               where (string.IsNullOrWhiteSpace(warehouseCode) || warehouseCode.Equals(a.WarehouseCode))
                               && (a.DelFlag.Equals("0"))
                               select a.LocationCode
                           ).ToList();
                /*
                sLocList = (
                                from a in db.Location
                                where (string.IsNullOrWhiteSpace(departmentCode) || departmentCode.Equals(a.DepartmentCode))
                                && (a.DelFlag.Equals("0"))
                                select a.LocationCode
                            ).ToList();
                */
                //Mod 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ ロケーションコードに空白がある場合は取り除く
                //取得したロケーションコードを半角大文字に変換
                foreach (string a in sLocList)
                {
                    string locationCode = Strings.StrConv(a, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    sLocList2.Add(locationCode);
                }

                //Mod 2015/05/14 arc yano IPO(部品棚卸)対応 パフォーマンス改善のため、クエリ変更
                //部品番号リストの取得
                sPartsList = (
                                from a in db.Parts
                                where (a.DelFlag.Equals("0"))
                                select a.PartsNumber
                             ).ToList();

                //Mod 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ 部品番号に空白がある場合は取り除く
                //取得した部品番号を半角大文字に変換
                foreach (string a in sPartsList)
                {
                    string partsNumber = Strings.StrConv(a, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    sPartsList2.Add(partsNumber);
                }

                //Mod 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ ロケーションコード、部品番号に空白がある場合は取り除く
				//Mod 2015/05/14 arc yano IPO(部品棚卸)対応 Excel取込時重複エラー対応(ロケーションコード、部品番号の書式を半角大文字に)
                //ロケーションコード、部品番号をそれぞれ半角大文字に変換する。
                for (int i = 0; i < isList.Count(); i++ )
                {
                    isList[i].LocationCode = Strings.StrConv(isList[i].LocationCode, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    isList[i].PartsNumber = Strings.StrConv(isList[i].PartsNumber, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                }
               
                //タイトル行の次の行がデータ行
                row++;

                string[] array = new string[4];

                //Mod 2015/06/09 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ エクセルの最終行はテンプレートより取得する。
                //Mod 2015/06/01 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② エクセル取込完了時に取り込んだ件数を表示する
                int datarow = 0;

                for (datarow = row; datarow < rowCnt+ row; datarow++)
                //for (datarow = row; datarow <= ws.Dimension.End.Row; datarow++)
                {
                    PartsInventory data = new PartsInventory();

                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {

                        for (int i = 0; i < 4; i++)
                        {
                        
                            if (col == colNumber[i])
                            {
                                array[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }
                    

                    //Mod 2015/05/14 arc yano IPO対応(部品棚卸) パフォーマンス改善のため、引数の変更(値渡し→参照渡し)と関数化の廃止
                    //------------------------------------------------
                    // 入力値チェック＆入力項目の設定
                    //------------------------------------------------
                    //Mod 2015/05/15 arc yano IPO対応(部品棚卸) 引数追加(departmentCode)
                    //string[] conditions = ValidateDataProperty(array, ref data, ref sLocList, ref sLocList2, ref sPartsList, ref sPartsList2, departmentCode);        //Mod 2016/08/13 arc yano #3596
                    string[] conditions = ValidateDataProperty(array, ref data, ref sLocList, ref sLocList2, ref sPartsList, ref sPartsList2, warehouseCode);

                    //-------------------------------------------------
                    //重複チェック(パフォーマンスを考えて関数化を廃止)
                    //-------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(data.LocationCode) && !string.IsNullOrWhiteSpace(data.PartsNumber))
                    {
                        // ロケーションコード＋部品番号の同一組み合わせが存在するかチェック
                        string target = dpChkList.Where(x => x.Equals(data.LocationCode + ":" + data.PartsNumber)).FirstOrDefault();

                        if (target != null)
                        {
                            ModelState.AddModelError("", "ロケーションコード:" + data.LocationCode + "、 部品番号:" + data.PartsNumber + "のデータが重複しています。");
                        }
                        
                        dpChkList.Add(data.LocationCode + ":" + data.PartsNumber);
                    }

                    //データ設定
                    SetData(data, warehouseCode, inventoryMonth, isList, conditions);      //Mod 2016/08/13 arc yano #3596
                }

                //データ行の件数を取得
               dataLowCnt = datarow - row;

                //検証エラーが発生した場合
                if (!ModelState.IsValid)
                {
                    int errCnt = 0;         //エラー件数
                    int wkCnt = 0;          //再セットするエラー件数

                    //検証エラーの全て件数を取得する。
                    foreach (var key in ModelState.Keys)
                    {
                        errCnt += ModelState[key].Errors.Count;
                    }

                    //エラー件数が100件を超えた場合
                    if (errCnt > MAX_ERR_CNT)
                    {
                        KeyValuePair<string, ModelState>[] test = new KeyValuePair<string, System.Web.Mvc.ModelState>[ModelState.Count];

                        //発生したモデルエラーを待避
                        ModelState.CopyTo(test, 0);

                        //モデルを一度クリア
                        ModelState.Clear();

                        foreach (var a in test)
                        {
                            for (int i = 0; i < a.Value.Errors.Count; i++)
                            {
                                ModelState.AddModelError(a.Key, a.Value.Errors[i].ErrorMessage);
                                wkCnt++;

                                if (wkCnt >= MAX_ERR_CNT)
                                {
                                    break;
                                }
                            }

                            if (wkCnt >= MAX_ERR_CNT)
                            {
                                break;
                            }
                        }
                        
                        ModelState.AddModelError("ErrCnt", "エラーの総数は" + errCnt + "件です。100件まで表示しています");
                    }
                    else
                    {
                        ModelState.AddModelError("importFile", "エラーの総数は" + errCnt + "件です");
                    }
                }
            }

            //検証エラーが発生していない場合
            if (ModelState.IsValid)
            {
                ret = dataLowCnt;
            }

            return ret;
        }

        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        private int DBExecute()
        {
            
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    //Mod 2015/05/14 arc yano IPO対応(部品棚卸) 一意制約違反の場合はシステムエラー画面に遷移するように変更
                    /*
                    // 一意制約エラーの場合、メッセージを設定し、返す
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "部品番号", "保存" }));
                        ts.Dispose();
                        //ストップウォッチを止める
                        return 1;
                    }
                    else
                    {
                    */
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                        ts.Dispose();
                        return -1;
                    //}
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                    ts.Dispose();
                    // エラーページに遷移
                    return -1;
                }
            }       
            return 0;
        }


        /// <summary>
        /// 取込ファイル存在チェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
        /// 各項目の列番号設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //初期処理
            int cnt = 1;

            //列番号設定
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //ロケーションコード
                    if (cell.Text.Contains("ロケーションコード"))
                    {
                        colNumber[0] = cnt;
                    }
                    //部品番号
                    if (cell.Text.Contains("部品番号"))
                    {
                        colNumber[1] = cnt;
                    }
                    //実棚数
                    if (cell.Text.Contains("実棚数"))
                    {
                        colNumber[2] = cnt;
                    }
                    //コメント
                    if (cell.Text.Contains("コメント"))
                    {
                        colNumber[3] = cnt;
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

        /// <summary>
        /// Excelの抽出条件行から、部門コード、対象年月の情報を取得する。
        /// </summary>
        /// <param name="conditionLine">検索条件行のテキスト</param>
        /// <param name="departmentCode">部門コード(out)</param>
        /// <param name="inventoryMonth">対象年月(out)</param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void ExtractInf(string conditionLine, ref string departmentCode, ref DateTime inventoryMonth)
        {

            //--------------------------------
            //初期設定
            //--------------------------------
            string strInventoryMonth = "";      //対象年月(Excel入力値)z
            int pos = -1;                       //対象文字列の位置

            //--------------------------------
            //情報取得
            //--------------------------------

            if (!string.IsNullOrEmpty(conditionLine))
            {

                //部門コード取得
                pos = conditionLine.IndexOf("部門=");

                if ((pos >= 0) && (pos + (3 + 3) <= conditionLine.Length))
                {
                    departmentCode = conditionLine.Substring(pos + 3, 3);
                }

                //対象年月取得
                pos = conditionLine.IndexOf("対象年月=");

                if ((pos >= 0) && (pos + (5 + 7) <= conditionLine.Length))
                {
                    strInventoryMonth = conditionLine.Substring(pos + 5, 7);
                }
            }

            //--------------------------------
            // 部門コードチェック
            //--------------------------------
            //マスタ存在チェック(部門コードがマスタに登録されていない場合エラー ※空欄もエラー)
            Department dep = new DepartmentDao(db).GetByKey(departmentCode);
            if (dep == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "部門コードがマスタに登録されていません。部門マスタに登録後、再度実行してください" }));
            }

            //--------------------------------
            // 倉庫コードチェック
            //--------------------------------
            //マスタ存在チェック(該当部門が使用する倉庫が定義してあるかをチェックする)
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

            if (dWarehouse == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "入力した部門が使用する倉庫がマスタに登録されていません。部門・倉庫組合せマスタに登録後、再実行してください" }));
            }

            //--------------------------------
            // 対象年月チェック
            //--------------------------------
            //フォーマットチェック(対象年月をDateTime型に変換出来ない場合エラー ※空欄もエラー)
            bool result = DateTime.TryParse(strInventoryMonth + "/01", out inventoryMonth);
            if (result == false)
            {
                inventoryMonth = DateTime.Parse("1900/01/01");      //適当な値を設定する。
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "対象年月の形式が正しくありません。対象年月をyyyy/MMで入力してください" }));
            }

            else
            {
                string status = GetInventoryStatusByWarehouse( (dWarehouse != null ? dWarehouse.WarehouseCode : "") , inventoryMonth);
                //string status = GetInventoryStatusByDepartment(departmentCode, inventoryMonth);

                if (status == STS_UNEXCUTED) //棚卸未実施
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "対象月の棚卸が未実施のため、データの取り込みができません" }));
                }
                if (status == STS_DECIDED) //棚卸完了
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "対象月の棚卸が終了しているため、データの取り込みができません" }));
                }
            }

            return;
        }
		
        /// <summary>
        /// Excelデータの項目チェック
        /// </summary>
        /// <param name="ws">ワークシート</param>
        /// <param name="row">行番号</param>
        /// <param name="locCol">列番号[0…ロケーションコード、1…部品番号,2…実棚, 3…コメント]</param>
        /// <param name="data">Excel入力値</param>
        /// <param name="sLocList">ロケーションコードリスト(更新用)</param>
        /// <param name="sLocList2">ロケーションコードリスト(チェック用)</param>
        /// <param name="sPartsList">部品番号リスト(更新用)</param>
        /// <param name="sPartsList2">部品番号リスト(チェック用)</param>
        /// <return>検索キー([0]…ロケーションコード、[1]部品番号</return>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数を変更(departmentCode → warehouseCode)
        /// 2015/06/09 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑤ 部品番号、ロケーションコードの空白を取り除いてからチェックを行う 
        /// 2015/05/14 arc yano IPO対応(部品棚卸) Excel取込時システムエラーの対応(ロケーションコード、部品番号チェック用の引数追加、引数の渡し方を値渡しから参照渡しへ変更)
        /// </history>
        private string[] ValidateDataProperty(string[] array, ref PartsInventory data, ref List<string> sLocList, ref List<string> sLocList2, ref List<string> sPartsList, ref List<string> sPartsList2, string warehouseCode)
        //private string[] ValidateDataProperty(string[] array, ref PartsInventory data, ref List<string> sLocList, ref List<string> sLocList2, ref List<string> sPartsList, ref List<string> sPartsList2, string departmentCode)
        {
            //--------------------------
            //初期設定
            //--------------------------
            decimal wkdata;

            data = new PartsInventory();

            string[] conditions = new string[] { "", "" };
            string chkString = "";

            //--------------------------
            //入力値チェック
            //--------------------------            

            //--------------------------
            //ロケーションコード
            //--------------------------
            //マスタ存在チェック
            chkString = Strings.StrConv(array[0], VbStrConv.Narrow, 0x0411).ToUpper().Trim();      //Mod 2015/06/09

            //大文字で検索
            int locIndex = sLocList2.IndexOf(chkString);
            if (locIndex < 0)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "倉庫コード:" + warehouseCode + "に対して、ロケーションコード:" + array[0] + "はマスタに登録されていません。マスタ登録後に実行してください" }));
                //ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "部門コード:" + departmentCode + "に対して、ロケーションコード:" + array[0] + "はマスタに登録されていません。マスタ登録後に実行してください" }));
            }
            else
            {
                data.LocationCode = sLocList[locIndex];　//ロケーションコードを待避(マスタのロケーションコードを待避)
                conditions[0] = sLocList2[locIndex];
            }
                
            //--------------------------
            //部品番号
            //--------------------------
            chkString = Strings.StrConv(array[1], VbStrConv.Narrow, 0x0411).ToUpper().Trim();      //Mod 2015/06/09
                
            int partsIndex = sPartsList2.IndexOf(chkString);
            
            if (partsIndex < 0)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "部品番号:" + array[1] + "はマスタに登録されていません。マスタ登録後に実行してください" }));
            }
            else
            {
                data.PartsNumber = sPartsList[partsIndex];　//部品番号を待避(マスタの部品番号を待避)
                conditions[1] = sPartsList2[partsIndex];
            }

            //--------------------------
            //実棚数
            //--------------------------
            //フォーマットチェック
            if (string.IsNullOrWhiteSpace(array[2]) || !(Regex.IsMatch(array[2], @"^\d{1,7}$") || Regex.IsMatch(array[2], @"^\d{1,7}\.\d{1,2}$")) || false == decimal.TryParse(array[2], out wkdata))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "ロケーションコード:" + data.LocationCode + "、" + "部品番号:" + data.PartsNumber + "の実棚数は正の数[7桁以内の整数、または小数(整数7桁以内、小数2桁以内)]で入力してください" }));
            }
            else
            {
                data.PhysicalQuantity = wkdata;
            }
                
            //--------------------------
            //コメント
            //--------------------------
             //レングスチェック
               
            if (array[3].Length > 255)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "ロケーションコード:" + data.LocationCode + "、" + "部品番号:" + data.PartsNumber + "のコメントは全角、または半角255文字以内で入力してください" }));
            }
            else
            {
                data.Comment = array[3];
            }
                
            return conditions;
        }

        /// <summary>
        /// Excel入力値のDBへの設定
        /// </summary>
        /// <param name="data">Excel入力値</param>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="conditions">検索条件([0]…ロケーションコード [1]…部品番号)</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private void SetData(PartsInventory data, string warehouseCode, DateTime inventoryMonth, List<InventoryStock> isList, string[] conditions)
        //private void SetData(PartsInventory data, string departmentCode, DateTime inventoryMonth, List<InventoryStock> isList, string[] conditions)
        {
            // 部品棚卸レコードの存在チェック

            InventoryStock ivStock = isList.Where(x => x.LocationCode.Equals(conditions[0]) && x.PartsNumber.Equals(conditions[1])).FirstOrDefault();
            //InventoryStock ivStock = new InventoryStockDao(db).GetByLocParts(inventoryMonth, departmentCode, data.LocationCode, data.PartsNumber);

            if (ivStock != null)
            {
                // 存在した場合、更新処理を行う
                ivStock = EditivStockData(ivStock, data, warehouseCode, inventoryMonth, true);      //Mod 2016/08/13 arc yano #3596
            }
            else // 存在しなかった場合、登録処理を行う
            {
                //InventoryStockへの登録
                ivStock = new InventoryStock();
                ivStock = EditivStockData(ivStock, data, warehouseCode, inventoryMonth, false);     //Mos 2016/08/13 arc yano #3596
                db.InventoryStock.InsertOnSubmit(ivStock);
            }

            return;
        }

        /// <summary>
        /// 読込んだExcelデータをInventoryStock用に編集する
        /// </summary>
        /// <param name="ivstock">部品在庫棚卸</param>
        /// <param name="data">Excelデータ</param>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">対象年月</param>
        /// <param name="updateflag">true:更新 false:新規追加</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        private InventoryStock EditivStockData(InventoryStock ivstock, PartsInventory data, string warehouseCode, DateTime inventoryMonth, Boolean updateflag)
        //private InventoryStock EditivStockData(InventoryStock ivstock, PartsInventory data, string departmentCode, DateTime inventoryMonth, Boolean updateflag)
        {
            int update = 0;
            
            //実棚数がDBとExcelで異なる場合のみ更新
            if (ivstock.PhysicalQuantity != data.PhysicalQuantity)
            {
                ivstock.PhysicalQuantity = data.PhysicalQuantity;

                update = 1;
            }
            //コメント
            if (!string.IsNullOrWhiteSpace(ivstock.Comment) || (!string.IsNullOrWhiteSpace(data.Comment)))
            {
                //DB、Excel共に値が0や空文字でない場合のみ比較する。
                if (!string.IsNullOrWhiteSpace(ivstock.Comment) && !string.IsNullOrWhiteSpace(data.Comment))
                {
                    if (!data.Comment.Equals(ivstock.Comment))
                    {
                        ivstock.Comment = data.Comment;

                        update = 1;
                    }
                }
                else
                {
                    ivstock.Comment = data.Comment;
                    update = 1;
                }
            }

            if (update == 1)
            {
                //最終更新者
                ivstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //最終更新日
                ivstock.LastUpdateDate = DateTime.Now;
            }

            //新規作成の場合はその他の項目も設定する。
            if (updateflag == false)
            {
                //棚卸ID
                ivstock.InventoryId = Guid.NewGuid();

                //部門コード ※空文字にする
                ivstock.DepartmentCode = "";        //Mod 2016/08/13 arc yano #3596

                //棚卸月
                ivstock.InventoryMonth = inventoryMonth;
                
                //ロケーションコード
                ivstock.LocationCode = data.LocationCode;

                //社員コード
                ivstock.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                
                //棚卸タイプ
                ivstock.InventoryType = TYP_PARTS;

                //管理番号
                ivstock.SalesCarNumber = null;

                //部品番号
                ivstock.PartsNumber = data.PartsNumber;

                //数量(新規項目の場合は実棚数と同じ)
                ivstock.Quantity = data.PhysicalQuantity;
                
                //作成者
                ivstock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                
                //作成日
                ivstock.CreateDate = DateTime.Now;

                //削除フラグ
                ivstock.DelFlag= "0";

                //サマリー
                ivstock.Summary = null;

                //倉庫コード
                ivstock.WarehouseCode = warehouseCode;      //Add Mod 2016/08/13 arc yano #3596
            }

            return ivstock;
        }

        /// <summary>
        /// 読込んだExcelデータをPartsStock用に編集する
        /// </summary>
        /// <param name="ptStock">部品在庫</param>
        /// <param name="impData">Excesデータ</param>
        /// <returns></returns>
        private PartsStock EditptStockData(PartsStock ptStock, PartsInventory data)
        {
            //部品番号
            ptStock.PartsNumber = data.PartsNumber;

            //ロケーションコード
            ptStock.LocationCode = data.LocationCode;

            //数量
            ptStock.Quantity = data.PhysicalQuantity;

            //作成者
            ptStock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            
            //作成日
            ptStock.CreateDate = DateTime.Now;

            //最終更新者
            ptStock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //最終更新日
            ptStock.LastUpdateDate = DateTime.Now;

            //削除フラグ
            ptStock.DelFlag = "0";

            return ptStock;
        }

        /// <summary>
        /// ダブルコーテーションの排除処理
        /// </summary>
        /// <param name="quoteData"></param>
        /// <returns></returns>
        private string[] EditExcelQuoteData(string[] quoteData)
        {
            string[] splLine2 = new string[quoteData.Count()];
            ArrayList array2 = new ArrayList();
            string splData = "";

            // ArrayListに格納
            array2.AddRange(quoteData);

            // ダブルコーテーションの文字列を検索
            for (int i = 0; i < array2.Count; i++)
            {

                splData = array2[i].ToString();
                splData = splData.Replace("\"", "");
                splData = splData.Replace(QuoteReplace, "\"");
                splData = splData.Replace(CommaReplace, ",");
                splLine2[i] = splData;
            }

            return splLine2;
        }
		
		/*
        /// <summary>
        /// ロケーションコード、部品番号重複レコードのチェック処理
        /// </summary>
        /// <param name="locationCode">ロケーションコード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="chkstr">ロケーションコード＋部品番号の文字列</param>
        /// <returns></returns>
        private string ValidationOverLapData(PartsInventory data, string chkstr)
        {
         
            //初期設定
            int index = -1;

            //string str = "";

            //ロケーションコード、部品番号が共に有効な場合にのみチェックする。※無効の場合は他のvalidationエラーで表示
            if (!string.IsNullOrWhiteSpace(data.LocationCode) && !string.IsNullOrWhiteSpace(data.PartsNumber))
            {
                // ロケーションコード＋部品番号の同一組み合わせが存在するかチェック
                index = chkstr.IndexOf(data.LocationCode + ":" + data.PartsNumber);

                if (index >= 0)
                {
                    ModelState.AddModelError("", "ロケーションコード:" + data.LocationCode + "、 部品番号:" + data.PartsNumber + "のデータが重複しています。");
                }
                chkstr += (data.LocationCode + ":" + data.PartsNumber) + " ";
            }

            return chkstr;
        }
        */

        // Add 2014/12/08 arc yano IPO対応(部品棚卸)
        /// <summary>
        /// 項目内のダブルコーテーションを別文字に置換する処理
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string ReplaceQuoteCommaData(string line)
        {
            int Quote = 0;
            int start = 0;
            int end = 0;
            string before = "";
            string after = "";
            int strLen = 0;

            // 項目内のダブルコーテーションを別文字に変換
            line = line.Replace("\"\"", QuoteReplace);

            for (int j = 0; j < line.Length - 1; j++)
            {
                // ダブルコーテーションの検索
                if (line.IndexOf("\"", j) >= 0)
                {
                    Quote++;
                }
                else
                {
                    // ヒットしなかった場合、ループ終了し、次の読込データへ
                    j = line.Length;
                    continue;
                }

                // ダブルコーテーションの数によって分岐
                if (Quote == 1)
                {
                    // 1個目の場合

                    //ヒットした位置を覚えておく(開始位置)
                    start = line.IndexOf("\"", j);

                    // 検索位置をヒットした位置以降に設定
                    j = start;
                }
                else if (Quote == 2)
                {
                    // 2個目が見つかった場合
                    //ヒットした位置を覚えておく(終了位置)
                    end = line.IndexOf("\"", j);

                    // 検索位置をヒットした位置以降に設定
                    j = end;

                    // 終了位置 - 開始位置で文字数を取得
                    strLen = end - start;

                    // 文字列切り出し
                    before = line.Substring(start, strLen + 1);

                    // 切り出した文字列内のカンマを変換
                    after = before.Replace(",", CommaReplace);

                    // 変換前の文字列を変換後の文字列で置換する
                    line = line.Replace(before, after);
                    Quote = 0;
                }
            }

            return line;
        }

        #region 部品在庫行追加・削除
        /// <summary>
        /// 部品在庫行追加
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="line">部品在庫明細</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品棚卸（棚卸在庫の修正） 新規作成
        /// </history>
        private ActionResult AddLine(FormCollection form, EntitySet<PartsInventory> line)
        {
            if (line == null)
            {
                line = new EntitySet<PartsInventory>();
            }

            PartsInventory rec = new PartsInventory();

            //数量、引当済数の初期化
            rec.PhysicalQuantity = 0;
            rec.ProvisionQuantity = 0;

            rec.NewRecFlag = true;

            line.Add(rec);

            form["EditFlag"] = "true";

            SetComponentDataEdit(form);

            return View("PartsInventoryDataEdit", line.ToList());
        }

        /// <summary>
        /// 部品在庫データ行削除
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">明細データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品棚卸（棚卸在庫の修正） 新規作成
        /// </history>
        private ActionResult DelLine(FormCollection form, EntitySet<PartsInventory> line)
        {
            ModelState.Clear();

            DateTime inventoryMonth = DateTime.Parse(form["inventoryMonth"]);

            int targetId = int.Parse(form["DelLine"]);

            using (TransactionScope ts = new TransactionScope())
            {
                if (line[targetId].NewRecFlag.Equals(false))
                {
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[targetId].LocationCode, line[targetId].PartsNumber, false);

                    if (rec != null)
                    {
                        rec.DelFlag = "1";                                                          //削除フラグを立てる
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
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
                    OutputLogger.NLogFatal(se, PROC_NAME_DELLINE, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME_DELLINE, FORM_NAME, "");
                    ts.Dispose();
                }
            }

            SetComponentDataEdit(form);

            return View("PartsInventoryDataEdit", line.ToList());
        }

        #endregion

         #region コントロールの有効無効
        /// <summary>
        /// コントロールの有効無効状態を返す。
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品棚卸（棚卸在庫の修正） 新規作成ボタン表示・非表示フラグの処理の追加
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

            //Add 2017/07/18 arc yano #3781
            //データ編集ボタン表示・非表示処理　権限のあるユーザのみ（現状はシステム課ユーザ）表示
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "InventoryDataEdit");

            if (rec != null && rec.EnableFlag.Equals(true))
            {
                ViewData["DataEditButtonVisible"] = true;
            }
            else
            {
                ViewData["DataEditButtonVisible"] = false;
            }
            
            return;
        }
        #endregion

       /// <summary>
       /// 処理中かどうかを取得する。(Ajax専用）
       /// </summary>
       /// <param name="processType">処理種別</param>
       /// <returns>アイドリング状態</returns>
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
       /// 部門コード、対象年月から棚卸開始日時を取得する(Ajax専用）
       /// </summary>
       /// <param name="departmentCode">部門コード</param>
       /// <param name="inventoryMonth">対象年月</param>
       /// <returns>取得結果(取得できない場合でもnullではない)</returns>
       /// <history>
       /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 InventoryScheduleParts⇔Departmentの関連付の廃止 倉庫コードから棚卸開始日を取得する
       /// 2015/06/17 arc yano IPO対応(部品棚卸) 障害対応、仕様変更⑥ 棚卸作業日ではなく、棚卸開始日時を表示する
       /// </history>
       public ActionResult GetStartDate(string warehouseCode, string inventoryMonth)
       //public ActionResult GetStartDate(string departmentCode, string inventoryMonth)
       {
           if (Request.IsAjaxRequest())
           {
               InventoryScheduleParts condition = new InventoryScheduleParts();
               //Mod 2016/08/13 arc yano #3596
               //condition.Department = new Department();
               //condition.Department.DepartmentCode = departmentCode;
               condition.WarehouseCode = warehouseCode;
               condition.InventoryMonth = DateTime.Parse(inventoryMonth + "/01");

               CodeData codeData = new CodeData();
               InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);
               if (rec != null)
               {
                   codeData.Code = "";                          //とりあえず何も設定しない
                   codeData.Name = rec.StartDate == null ? "": string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
               }
               return Json(codeData);
           }
           return new EmptyResult();
       }
        
    }
}
