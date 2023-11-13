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
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// ロケーションマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class LocationController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "ロケーションマスタ";     // 画面名
        private static readonly string PROC_NAME = "ロケーションマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LocationController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ロケーション検索画面表示
        /// </summary>
        /// <returns>ロケーション検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ロケーション検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ロケーション検索画面</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成 検索条件に倉庫コード、倉庫名を追加
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Location> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["DelFlag"] = form["DelFlag"];

            ViewData["WarehouseCode"] = form["WarehouseCode"];    //Add 2016/08/13 arc yano #3596
            ViewData["WarehouseName"] = form["WarehouseName"];    //Add 2016/08/13 arc yano #3596

            // ロケーション検索画面の表示
            return View("LocationCriteria", list);
        }

        /// <summary>
        /// ロケーション検索ダイアログ表示
        /// </summary>
        /// <returns>ロケーション検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            FormCollection form = new FormCollection();
            form["ConditionsHold"] = Request["ConditionsHold"];
            form["HoldBusinessType"] = Request["BusinessType"];
            form["BusinessType"] = Request["BusinessType"];

            return CriteriaDialog(form);
        }

        /// <summary>
        /// ロケーション検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ロケーション検索画面ダイアログ</returns>
        /// <history>
        /// 2017/07/27 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）倉庫名が入力されていない場合はマスタから取得する
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成 検索条件に倉庫コード、倉庫名を追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["LocationCode"] = Request["LocationCode"];
            form["LocationName"] = Request["LocationName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);            
            form["LocationType"] = Request["LocationType"];

            form["WarehouseCode"] = Request["WarehouseCode"]; //Add 2016/08/13 arc yano #3596
            form["WarehouseName"] = Request["WarehouseName"]; //Add 2016/08/13 arc yano #3596

            if (!string.IsNullOrWhiteSpace(form["WarehouseCode"]) && !form["WarehouseCode"].Equals("undefined") && string.IsNullOrWhiteSpace(form["WarehouseName"]))
            {
                Warehouse warehouse = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);

                form["WarehouseName"] = warehouse != null ? warehouse.WarehouseName : "";
            }

            //Add 2016/08/13 arc yano #3596
            //倉庫コード、倉庫名共に未入力だった場合
            if (string.IsNullOrWhiteSpace(form["WarehouseCode"]) && string.IsNullOrWhiteSpace(form["WarehouseName"]))
            {
                //倉庫情報の取得
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

                form["WarehouseCode"] = (dWarehouse != null ? dWarehouse.WarehouseCode : "");
                form["WarehouseName"] = (dWarehouse != null ? dWarehouse.Warehouse.WarehouseName : "");
            }

            if (form["ConditionsHold"] != null && form["ConditionsHold"].ToString().Equals("1"))
            {
                form["BusinessType"] = form["HoldBusinessType"];
            }
            else
            {
                if (!string.IsNullOrEmpty(form["DepartmentCode"]) || !string.IsNullOrEmpty(form["DepartmentName"]) || !string.IsNullOrEmpty(form["WarehouseCode"]) || !string.IsNullOrEmpty(form["WarehouseName"]) || !string.IsNullOrEmpty(form["LocationCode"]) || !string.IsNullOrEmpty(form["LocationName"]))
                {
                    form["BusinessType"] = "";
                }
                else
                {
                    form["BusinessType"] = form["HoldBusinessType"];
                }
            }


            // 検索結果リストの取得
            PaginatedList<Location> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];

            //Add 2017/02/03 arc nakayama #3594_部品移動入力　出庫・入庫ロケーションの絞込み②
            ViewData["ConditionsHold"] = form["ConditionsHold"];
            ViewData["BusinessType"] = form["BusinessType"];
            ViewData["HoldBusinessType"] = form["HoldBusinessType"];

            ViewData["LocationType"] = form["Locationtype"];

            //Add 2016/08/13 arc yano #3596
            //倉庫情報の設定
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            ViewData["WarehouseName"] = form["WarehouseName"];

            // ロケーション検索画面の表示
            return View("LocationCriteriaDialog", list);
        }

        //Add 2014/11/07 arc yano 車両ステータス変更対応　車両ステータス変更画面専用の検索ダイアログを追加
        /// <summary>
        /// ロケーション検索ダイアログ表示(車両ステータス変更画面専用)
        /// </summary>
        /// <returns>ロケーション検索ダイアログ</returns>
        public ActionResult CriteriaDialogForCarUsage()
        {
            return CriteriaDialogForCarUsage(new FormCollection());
        }

        /// <summary>
        /// ロケーション検索ダイアログ表示(車両ステータス変更画面専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ロケーション検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialogForCarUsage(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["LocationCode"] = Request["LocationCode"];
            form["LocationName"] = Request["LocationName"];

            // 検索結果リストの取得
            PaginatedList<V_LocationListForCarUsage> list = GetSearchResultListForCarUsage(form);

            //その他の項目の設定
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];

            // ロケーション検索画面の表示
            return View("LocationCriteriaDialogForCarUsage", list);
        }


        /// <summary>
        /// ロケーションマスタ入力画面表示
        /// </summary>
        /// <param name="id">ロケーションコード(更新時のみ設定)</param>
        /// <returns>ロケーションマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Location location;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                location = new Location();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/07/06 arc nakayama ロケーションの無効データの更新で落ちるため修正　更新は無効データも含むようにする
                location = new LocationDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(location);

            // 出口
            return View("LocationEntry", location);
        }

        /// <summary>
        /// ロケーションマスタ追加更新
        /// </summary>
        /// <param name="location">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>ロケーションマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Location location, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateLocation(location);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(location);
                return View("LocationEntry", location);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/07/06 arc nakayama ロケーションの無効データの更新で落ちるため修正　更新は無効データも含むようにする
                Location targetLocation = new LocationDao(db).GetByKey(location.LocationCode, true);
                UpdateModel(targetLocation);
                EditLocationForUpdate(targetLocation);
            }
            // データ追加処理
            else
            {
                // データ編集
                location = EditLocationForInsert(location);

                // データ追加
                db.Location.InsertOnSubmit(location);                
            }

            // Add 2014/08/05 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力(更新と追加で入っていたSubmitChangesを統合)
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0010", new string[] { "ロケーションコード", "保存" }));
                        GetEntryViewData(location);
                        return View("LocationEntry", location);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error"); ;
                    }
                }
                catch (Exception e)
                {
                    // 上記以外の例外の場合、エラーログ出力し、エラー画面に遷移する
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(location.LocationCode);

        }

        /// <summary>
        /// ロケーションコードからロケーション名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">ロケーションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2021/08/03 yano #4098 【ロケーションマスタ】ロケーションを無効に変更した時のチェック処理の追加
        /// 2016/04/26 arc yano #3510 部品入荷入力　入荷ロケーションの絞込み ロケーション種別を取得する処理を追加
        /// </history>
        public ActionResult GetMaster(string code, bool includeDeleted = false)
        {
            string businessType = Request["BusinessType"];
            string locationType = Request["LocationType"];


            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Location location = new LocationDao(db).GetByBusinessType(code, businessType, locationType, includeDeleted);    //Mod 2021/08/03 yano #4098
                if (location != null)
                {
                    codeData.Code = location.LocationCode;
                    codeData.Name = location.LocationName;
                    codeData.Code2 = location.LocationType;     //Add 2016/04/26 #3510
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        //Add 2014/11/11 arc yano 車両ステータス変更対応
        /// <summary>
        /// ロケーションコードからロケーション名を取得する(Ajax専用）※車両ステータス入力画面専用
        /// </summary>
        /// <param name="code">ロケーションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterForCarUsage(string code)
        {
            string businessType = Request["BusinessType"];
            string locationType = Request["LocationType"];


            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                V_LocationListForCarUsage location = new LocationDao(db).GetByKeyForCarUsage(code);
                if (location != null)
                {
                    codeData.Code = location.LocationCode;
                    codeData.Name = location.LocationName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="location">モデルデータ</param>
        /// <history>
        /// 2016/08/13 arc yano #3596【大項目】部門棚統合対応 登録項目を部門から倉庫に変更
        /// </history>
        private void GetEntryViewData(Location location)
        {
            //Mod 2016/08/13 arc yano #3596
            //倉庫名の取得
            if (!string.IsNullOrEmpty(location.WarehouseCode))
            {
                WarehouseDao warehouseDao = new WarehouseDao(db);
                Warehouse warehouse = warehouseDao.GetByKey(location.WarehouseCode);
                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }
            /*
            // 部門名の取得
            if (!string.IsNullOrEmpty(location.DepartmentCode))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(location.DepartmentCode);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            */
            CodeDao dao = new CodeDao(db);
            ViewData["LocationTypeList"] = CodeUtils.GetSelectListByModel<c_LocationType>(dao.GetLocationTypeAll(false), location.LocationType,false);
        }

        /// <summary>
        /// ロケーションマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ロケーションマスタ検索結果リスト</returns>
        /// <history>
        /// 2016/08/17 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 
        ///                                       ①検索条件に倉庫を追加
        ///                                       ②検索条件の設定方法の変更
        /// </history>
        private PaginatedList<Location> GetSearchResultList(FormCollection form)
        {
            LocationDao locationDao = new LocationDao(db);
            Location locationCondition = new Location();
            locationCondition.LocationCode = form["LocationCode"];
            locationCondition.LocationName = form["LocationName"];
            locationCondition.DepartmentCode = form["DepartmentCode"];
            locationCondition.DepartmentName = form["DepartmentName"];
            locationCondition.BusinessType = form["BusinessType"];
            locationCondition.LocationType = form["Locationtype"];
            locationCondition.WarehouseCode = form["WarehouseCode"];
            locationCondition.WarehouseName = form["WarehouseName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                locationCondition.DelFlag = form["DelFlag"];
            }

            /*
            locationCondition.Department = new Department();
            locationCondition.Department.DepartmentCode = form["DepartmentCode"];
            locationCondition.Department.DepartmentName = form["DepartmentName"];
            locationCondition.Department.BusinessType = form["BusinessType"];
            */
            
            return locationDao.GetListByCondition(locationCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Add 2014/11/07 arc yano  車両ステータス変更対応 車両ステータス変更画面専用のロケーション一覧取得
        /// <summary>
        /// ロケーションマスタ検索結果リスト取得(車両ステータス変更専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>ロケーションマスタ検索結果リスト</returns>
        private PaginatedList<V_LocationListForCarUsage> GetSearchResultListForCarUsage(FormCollection form)
        {
            LocationDao locationDao = new LocationDao(db);
            V_LocationListForCarUsage locationlist = new V_LocationListForCarUsage();
            locationlist.LocationCode = form["LocationCode"];
            locationlist.LocationName = form["LocationName"];

            return locationDao.GetListForCarUsageByCondition(locationlist, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }


        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="location">ロケーションデータ</param>
        /// <returns>ロケーションデータ</returns>
        /// <history>
        /// 2021/08/03 yano #4098 【ロケーションマスタ】ロケーションを無効に変更した時のチェック処理の追加
        /// 2016/08/13 arc yano #3596【大項目】部門棚統合対応 登録項目を部門→倉庫に変更
        /// 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
        /// </history>
        private Location ValidateLocation(Location location)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(location.LocationCode))
            {
                ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0001", "ロケーションコード"));
            }
            if (string.IsNullOrEmpty(location.LocationName))
            {
                ModelState.AddModelError("LocationName", MessageUtils.GetMessage("E0001", "ロケーション名"));
            }
            //Mod 2016/08/13 arc yano #3596
            if (string.IsNullOrEmpty(location.WarehouseCode))
            {
                ModelState.AddModelError("WarehouseCode", MessageUtils.GetMessage("E0001", "倉庫コード"));
            }
            /*
            if (string.IsNullOrEmpty(location.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門コード"));
            }
            */
            // フォーマットチェック
            if (ModelState.IsValidField("LocationCode") && !CommonUtils.IsAlphaNumericBar(location.LocationCode))
            {
                ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0020", "ロケーションコード"));
            }

            //Mod 2014/08/14 arc amii エラーログ対応 
            // ロケーション種別が（引当、仕掛）の場合、同一部門内に複数設定できない
            if (!CommonUtils.DefaultString(location.LocationType).Equals("001"))
            {
                //Mod 2016/08/13 arc yano #3596
                //List<Location> locationList = new LocationDao(db).GetListByLocationType(location.LocationType, location.DepartmentCode, location.LocationCode);
                List<Location> locationList = new LocationDao(db).GetListByLocationType(location.LocationType, location.WarehouseCode, location.LocationCode);
                if (locationList.Count > 0) {
                    ModelState.AddModelError("LocationType", "同一倉庫内に特殊なロケーション(仕掛、その他)は複数設定できません");
                }
            }

            //Add 2021/08/03 yano #4098
            //dbから現在の有効・無効状態を取得
            Location dblocaiont = new LocationDao(db).GetByKey(location.LocationCode, true);
            //有効から無効に変更した場合
            if(location.DelFlag.Equals("1") && !location.DelFlag.Equals(dblocaiont.DelFlag))
            {
                //対象のロケーションの部品在庫有無をチェック
                if (new PartsStockDao(db).getPresencePartsStock(location.LocationCode))
                {
                     ModelState.AddModelError("DelFlag", "対象ロケーションに部品在庫が存在するため、無効にできません");
                }
            }

            return location;
        }

        /// <summary>
        /// ロケーションマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="location">ロケーションデータ(登録内容)</param>
        /// <returns>ロケーションマスタモデルクラス</returns>
        private Location EditLocationForInsert(Location location)
        {
            location.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.CreateDate = DateTime.Now;
            location.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.LastUpdateDate = DateTime.Now;
            location.DelFlag = "0";
            return location;
        }

        /// <summary>
        /// ロケーションマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="location">ロケーションデータ(登録内容)</param>
        /// <returns>ロケーションマスタモデルクラス</returns>
        private Location EditLocationForUpdate(Location location)
        {
            location.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.LastUpdateDate = DateTime.Now;
            return location;
        }

    }
}
