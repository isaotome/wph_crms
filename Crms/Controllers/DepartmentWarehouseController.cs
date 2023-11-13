using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Transactions;
using Crms.Models;

namespace Crms.Controllers
{
    /// <summary>
    /// 部門・倉庫組合せマスタ
    /// </summary>
    /// <returns></returns>
    /// <history>
    /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepartmentWarehouseController : InheritedController
    {
        private static readonly string FORM_NAME = "部門・倉庫組合せマスタ";                 // 画面名
        private static readonly string PROC_NAME_CRITERIA = "部門・倉庫組合せマスタ検索";  　// 処理名
        private static readonly string PROC_NAME_ENTRY = "部門・倉庫組合せマスタ登録";       // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DepartmentWarehouseController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();
            form["DelFlag"] = "0";
            return Criteria(form);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            ViewData["WarehouseName"] = form["WarehouseName"];
            ViewData["DelFlag"] = form["DelFlag"];
            PaginatedList<DepartmentWarehouse> list = GetSearchResult(form);
            return View("DepartmentWarehouseCriteria", list);
        }

        /// <summary>
        /// 検索ダイアログ表示
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 検索ダイアログ処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            ViewData["WarehouseName"] = form["WarehouseName"];
            ViewData["id"] = form["id"];
            PaginatedList<DepartmentWarehouse> list = GetSearchResult(form);
            return View("DepartmentWarehouseCriteriaDialog", list);
        }

        /// <summary>
        /// 検索結果を取得する
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<DepartmentWarehouse> GetSearchResult(FormCollection form)
        {
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CRITERIA);

            DepartmentWarehouse condition = new DepartmentWarehouse();
            
            condition.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentName = form["DepartmentName"];
            condition.WarehouseCode = form["WarehouseCode"];
            condition.WarehouseName = form["WarehouseName"];
            condition.DelFlag = form["DelFlag"];

            return new DepartmentWarehouseDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 部門・倉庫組合せマスタ追加・更新画面表示
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <returns>入力画面</returns>
        public ActionResult Entry(string departmentCode, string warehouseCode)
        {
            DepartmentWarehouse model;
            FormCollection form = new FormCollection();

            if (string.IsNullOrEmpty(departmentCode) || string.IsNullOrEmpty(warehouseCode))
            {
                model = new DepartmentWarehouse();
                ViewData["update"] = "0";
            }
            else
            {
                model = new DepartmentWarehouseDao(db).GetByKey(departmentCode, warehouseCode, true);
                ViewData["update"] = form["update"] = "1";

                //更新時はViewDataに設定する
                form["DepartmentCode"] = departmentCode;
                form["WarehouseCode"] = warehouseCode;
                SetDataComponent(form);
            }

            return View("DepartmentWarehouseEntry", model);
        }

        /// <summary>
        /// 部門・倉庫組合せマスタ追加・更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(DepartmentWarehouse model, FormCollection form)
        {
            ValidateDepartmentWarehouse(model);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("DepartmentWarehouseEntry", model);
            }

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_ENTRY);

            using (TransactionScope ts = new TransactionScope())
            {
                // データ更新処理
                if (form["update"].Equals("1"))
                {
                    // データ編集・更新 //１部門１倉庫暫定対応のため、部門コードでのみデータを取得する
                    //DepartmentWarehouse target = new DepartmentWarehouseDao(db).GetByKey(model.DepartmentCode, model.WarehouseCode, true);
                    DepartmentWarehouse target = new DepartmentWarehouseDao(db).GetByDepartment(model.DepartmentCode, true);
                    UpdateModel(target);
                    EditDepartmentWarehouseForUpdate(target);
                }
                // データ追加処理
                else
                {
                    // データ編集
                    model = EditDepartmentWarehouseForInsert(model);
                    // データ追加
                    db.DepartmentWarehouse.InsertOnSubmit(model);
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
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_ENTRY, FORM_NAME, "");
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
                            // ログに出力
                            OutputLogger.NLogError(e, PROC_NAME_ENTRY, FORM_NAME, "");

                            ModelState.AddModelError("WarehouseCode", MessageUtils.GetMessage("E0010", new string[] { "倉庫コード", "保存" }));
                            return View("DepartmentWarehouseEntry", model);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME_ENTRY, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME_ENTRY, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
                // 出口

            }

            ModelState.Clear();
            SetDataComponent(form);
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            return Entry(model.DepartmentCode, model.WarehouseCode);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidateDepartmentWarehouse(DepartmentWarehouse model)
        {
            CommonValidate("DepartmentCode", "部門コード", model, true);
            CommonValidate("WarehouseCode", "倉庫コード", model, true);
        }

        /// <summary>
        /// 画面コンポーネント
        /// </summary>
        /// <param name="form"></param>
        private void SetDataComponent(FormCollection form)
        {
            ViewData["update"] = form["update"];
            //部門名
            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            ViewData["DepartmentName"] = (dep != null ? dep.DepartmentName : "");

            //倉庫名
            Warehouse whouse = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);
            ViewData["WarehouseName"] = (whouse != null ? whouse.WarehouseName : "");
        }

        /// <summary>
        /// 部門コードから部門・倉庫組合せを取得する(Ajax専用） ※現状、１部門１倉庫のための暫定対応
        /// </summary>
        /// <param name="code">倉庫コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, code); 

                if (departmentWarehouse != null)
                {
                    codeData.Code = departmentWarehouse.WarehouseCode;
                    codeData.Name = departmentWarehouse.Warehouse.WarehouseName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 倉庫マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="departmentWarehouse">倉庫データ(登録内容)</param>
        /// <returns>倉庫マスタモデルクラス</returns>
        private DepartmentWarehouse EditDepartmentWarehouseForInsert(DepartmentWarehouse departmentWarehouse)
        {
            departmentWarehouse.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            departmentWarehouse.CreateDate = DateTime.Now;
            departmentWarehouse.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            departmentWarehouse.LastUpdateDate = DateTime.Now;
            departmentWarehouse.DelFlag = "0";
            return departmentWarehouse;
        }

        /// <summary>
        /// 部門・倉庫組合せマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="departmentWarehouse">倉庫データ(登録内容)</param>
        /// <returns>倉庫マスタモデルクラス</returns>
        private DepartmentWarehouse EditDepartmentWarehouseForUpdate(DepartmentWarehouse departmentWarehouse)
        {
            departmentWarehouse.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            departmentWarehouse.LastUpdateDate = DateTime.Now;
            return departmentWarehouse;
        }
    }
}
