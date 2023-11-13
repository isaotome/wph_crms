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
    /// 部品ロケーションマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsLocationController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "部品ロケーションマスタ";     // 画面名
        private static readonly string PROC_NAME = "部品ロケーションマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsLocationController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 部品ロケーション検索画面表示
        /// </summary>
        /// <returns>部品ロケーション検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 部品ロケーション検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品ロケーション検索画面</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件に「倉庫」追加
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<PartsLocation> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                PartsDao partsDao = new PartsDao(db);
                Parts parts = partsDao.GetByKey(form["PartsNumber"]);
                if (parts != null)
                {
                    ViewData["PartsNameJp"] = parts.PartsNameJp;
                }
            }
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(form["DepartmentCode"]);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(form["LocationCode"])) {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(form["LocationCode"]);
                if (location != null) {
                    ViewData["LocationName"] = location.LocationName;
                }
            }

            //Add 2016/08/13 arc yano #3596
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            if (!string.IsNullOrEmpty(form["WarehouseCode"]))
            {
                WarehouseDao WarehouseDao = new WarehouseDao(db);
                Warehouse warehouse = WarehouseDao.GetByKey(form["WarehouseCode"]);
                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }

            // 部品ロケーション検索画面の表示
            return View("PartsLocationCriteria", list);
        }

        /// <summary>
        /// 部品ロケーションマスタ入力画面表示
        /// </summary>
        /// <param name="id">部品ロケーションコード(更新時のみ設定)</param>
        /// <returns>部品ロケーションマスタ入力画面</returns>
        /// <history>
        /// 2017/01/19 arc yano #3694  部品ロケーション　新規作成表示時のシステムエラー
        /// 　　　　　　　　　　　　　検索画面で部門コードまたは倉庫コードが入力されていない場合は空欄で表示する　
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 登録項目の追加(倉庫コード)
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string Status, string PartsNumber, string DepartmentCode = "", string WarehouseCode = "")
        {
           
           // string[] idArr = CommonUtils.DefaultString(id, "0,,").Split(new string[] { "," }, StringSplitOptions.None);

            //Add 2017/01/19 arc yano #3694
            if (ModelState.IsValid)
            {
                ModelState.Clear();     
            }
            

            PartsLocation partsLocation;
            string warehouseCode = "";
            if (string.IsNullOrWhiteSpace(WarehouseCode))
            {
                DepartmentWarehouse dwhouse = CommonUtils.GetWarehouseFromDepartment(db, DepartmentCode);
                
                //Add 2017/01/19 arc yano #3694  
                if (dwhouse != null)
                {
                    warehouseCode = dwhouse.WarehouseCode;
                }   
            }
            else
            {
                warehouseCode = WarehouseCode;
            }

            // 追加の場合
            if (Status.Equals("0"))
            {
                ViewData["update"] = "0";
                partsLocation = new PartsLocation();
                partsLocation.PartsNumber = PartsNumber;
                partsLocation.DepartmentCode = "";
                partsLocation.WarehouseCode = warehouseCode;
                ViewData["fixedParts"] = string.IsNullOrEmpty(PartsNumber) ? "0" : "1";

                //Mod 2017/01/19 arc yano #3694
                ViewData["fixedWhouse"] = string.IsNullOrEmpty(warehouseCode) ? "0" : "1";
                //ViewData["fixedDept"] = string.IsNullOrEmpty(DepartmentCode) ? "0" : "1";
            }
            else // 更新の場合
            {
                ViewData["update"] = "1";
                partsLocation = new PartsLocationDao(db).GetByKey(PartsNumber, warehouseCode, false);
                ViewData["fixedParts"] = "1";
                //Mod 2017/01/19 arc yano #3694
                ViewData["fixedWhouse"] = "1";
                //ViewData["fixedDept"] = "1";
            }

            // その他表示データの取得
            GetEntryViewData(partsLocation);

            // 出口
            return View("PartsLocationEntry", partsLocation);
        }

        /// <summary>
        /// 部品ロケーションマスタ追加更新
        /// </summary>
        /// <param name="partsLocation">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>部品ロケーションマスタ入力画面</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索キーの変更(部門→倉庫)
        /// 2014/10/30 ishii ステータスが無効の情報の保存対応
        /// 2014/10/29 ishii 保存ボタン対応
        /// 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PartsLocation partsLocation, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];
            ViewData["fixedParts"] = form["fixedParts"];
            ViewData["fixedDept"] = form["fixedDept"];

            // データチェック
            ValidatePartsLocation(partsLocation, form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(partsLocation);
                return View("PartsLocationEntry", partsLocation);
            }

            // Add 2014/08/05 arc amii エラーログ対応
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //MOD 2014/10/30 ishii ステータスが無効の情報の保存対応
                //PartsLocation targetPartsLocation = new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.DepartmentCode,false);
                PartsLocation targetPartsLocation = new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.WarehouseCode, false);   //2016/08/13 arc yano #3596
                UpdateModel(targetPartsLocation);
                EditPartsLocationForUpdate(targetPartsLocation);
            }
            // データ追加処理
            else
            {
                // データ編集
                partsLocation = EditPartsLocationForInsert(partsLocation);

                // データ追加
                db.PartsLocation.InsertOnSubmit(partsLocation);
            }

            // Add 2014/08/05 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
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

                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { getErrorKeyItemName(form), "保存" }));
                        ModelState.AddModelError("WarehouseCode", ""); //Mod 2016/08/13 arc yano #3596
                        GetEntryViewData(partsLocation);
                        return View("PartsLocationEntry", partsLocation);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
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

            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //2016/08/13 arc yano #3596
            return Entry("1", partsLocation.PartsNumber, "",  partsLocation.WarehouseCode); 
            //return Entry("1," + partsLocation.PartsNumber + "," + partsLocation.DepartmentCode);  
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="partsLocation">モデルデータ</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 登録項目の変更(部門コード→倉庫コード)
        /// </history>
        private void GetEntryViewData(PartsLocation partsLocation)
        {
            // 部品名の取得
            if (!string.IsNullOrEmpty(partsLocation.PartsNumber))
            {
                PartsDao partsDao = new PartsDao(db);
                Parts parts = partsDao.GetByKey(partsLocation.PartsNumber);
                if (parts != null)
                {
                    ViewData["PartsNameJp"] = parts.PartsNameJp;
                }
            }

            // 倉庫名の取得
            if (!string.IsNullOrEmpty(partsLocation.WarehouseCode))
            {
                WarehouseDao warehouseDao = new WarehouseDao(db);
                Warehouse warehouse = warehouseDao.GetByKey(partsLocation.WarehouseCode);

                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }

            /*
            // 部門名の取得
            if (!string.IsNullOrEmpty(partsLocation.DepartmentCode))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(partsLocation.DepartmentCode);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            */
            // ロケーション名の取得
            if (!string.IsNullOrEmpty(partsLocation.LocationCode))
            {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(partsLocation.LocationCode);
                if (location != null)
                {
                    ViewData["LocationName"] = location.LocationName;
                }
            }
        }

        /// <summary>
        /// 部品ロケーションマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品ロケーションマスタ検索結果リスト</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
        ///                            ①検索条件に倉庫を追加
        ///                            ②検索条件の部門の設定の変更(関連付けによるアクセスの廃止)
        /// </history>
        private PaginatedList<PartsLocation> GetSearchResultList(FormCollection form)
        {
            PartsLocationDao partsLocationDao = new PartsLocationDao(db);
            PartsLocation partsLocationCondition = new PartsLocation();

            partsLocationCondition.Parts = new Parts();
            partsLocationCondition.Parts.PartsNumber = form["PartsNumber"];

            //Mod 2016/08/13 arc yano #3596
            //partsLocationCondition.Department = new Department();
            //partsLocationCondition.Department.DepartmentCode = form["DepartmentCode"];
            partsLocationCondition.DepartmentCode = form["DepartmentCode"];

            partsLocationCondition.LocationCode = form["LocationCode"];

            //Add 2016/08/13 arc yano #3596
            partsLocationCondition.WarehouseCode = form["WarehouseCode"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                partsLocationCondition.DelFlag = form["DelFlag"];
            }

            return partsLocationDao.GetListByCondition(partsLocationCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="partsLocation">部品ロケーションデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>部品ロケーションデータ</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 入力項目の変更(部門コード→倉庫コード)
        /// </history>
        private PartsLocation ValidatePartsLocation(PartsLocation partsLocation, FormCollection form)
        {
            // 必須チェック
            //部品番号
            if (string.IsNullOrEmpty(partsLocation.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", "部品"));
            }

            //Mod 2016/08/13 arc yano #3596
            //倉庫コード
            if (string.IsNullOrEmpty(partsLocation.WarehouseCode))
            {
                ModelState.AddModelError("WarehouseCode", MessageUtils.GetMessage("E0001", "倉庫"));
            }
            /*
            if (string.IsNullOrEmpty(partsLocation.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            */
            

            // フォーマットチェック
            //部品番号
            if (ModelState.IsValidField("PartsNumber") && !CommonUtils.IsAlphaNumeric(partsLocation.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0012", "部品"));
            }

            //Mod 2016/08/13 arc yano #3596
            /*
            if (ModelState.IsValidField("DepartmentCode") && !CommonUtils.IsAlphaNumeric(partsLocation.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0012", "部門"));
            }
            */

            // 重複チェック
            if (ModelState.IsValid && form["update"].Equals("0"))
            {
                //Mod 2016/08/13 arc yano #3596
                if (new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.WarehouseCode) != null)
                //if (new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.DepartmentCode) != null)
                {
                    ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0006", getErrorKeyItemName(form)));
                    ModelState.AddModelError("WarehouseCode", "");
                }
            }

            return partsLocation;
        }

        /// <summary>
        /// 部品ロケーションマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="partsLocation">部品ロケーションデータ(登録内容)</param>
        /// <returns>部品ロケーションマスタモデルクラス</returns>
        private PartsLocation EditPartsLocationForInsert(PartsLocation partsLocation)
        {
            partsLocation.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.CreateDate = DateTime.Now;
            partsLocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.LastUpdateDate = DateTime.Now;
            partsLocation.DelFlag = "0";
            return partsLocation;
        }

        /// <summary>
        /// 部品ロケーションマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="partsLocation">部品ロケーションデータ(登録内容)</param>
        /// <returns>部品ロケーションマスタモデルクラス</returns>
        private PartsLocation EditPartsLocationForUpdate(PartsLocation partsLocation)
        {
            partsLocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.LastUpdateDate = DateTime.Now;
            return partsLocation;
        }

        /// <summary>
        /// エラーメッセージ使用キー項目名取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>エラーメッセージ使用キー項目名</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 入力項目の変更(部門コード→倉庫コード)
        /// </history>
        private string getErrorKeyItemName(FormCollection form)
        {
            string itemName;
            if (form["fixedParts"].Equals("1"))
            {
                itemName = "部品";
            }
            else if (form["fixedWhouse"].Equals("1"))
            {
                itemName = "倉庫";
            }
            else
            {
                itemName = "部品,倉庫";
            }
            return itemName;
        }
    }
}
