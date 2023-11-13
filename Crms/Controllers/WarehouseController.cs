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
    /// 倉庫マスタ
    /// </summary>
    /// <returns></returns>
    /// <history>
    /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 新規作成
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class WarehouseController : InheritedController
    {
        private static readonly string FORM_NAME = "倉庫マスタ";                 // 画面名
        private static readonly string PROC_NAME_CRITERIA = "倉庫マスタ検索";  　// 処理名
        private static readonly string PROC_NAME_ENTRY = "倉庫マスタ登録";       // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WarehouseController()
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
            PaginatedList<Warehouse> list = GetSearchResult(form);
            return View("WarehouseCriteria", list);
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
            PaginatedList<Warehouse> list = GetSearchResult(form);
            return View("WarehouseCriteriaDialog", list);
        }

        /// <summary>
        /// 検索結果を取得する
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<Warehouse> GetSearchResult(FormCollection form)
        {
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_CRITERIA);

            Warehouse condition = new Warehouse();
            condition.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentName = form["DepartmentName"];
            condition.WarehouseCode = form["WarehouseCode"];
            condition.WarehouseName = form["WarehouseName"];
            condition.DelFlag = form["DelFlag"];
            return new WarehouseDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 倉庫マスタ追加・更新画面表示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            Warehouse model;

            if (string.IsNullOrEmpty(id))
            {
                model = new Warehouse();
                ViewData["update"] = "0";
            }
            else
            {
                model = new WarehouseDao(db).GetByKey(id, true);
                ViewData["update"] = "1";
                ViewData["DepartmentName"] = model.Department.DepartmentName;
            }
            return View("WarehouseEntry", model);
        }

        /// <summary>
        /// 倉庫マスタ追加・更新
        /// </summary>
        /// <param name="model"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Warehouse model, FormCollection form)
        {
            ValidateWarehouse(model);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("WarehouseEntry", model);
            }

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_ENTRY);

            using (TransactionScope ts = new TransactionScope())
            {
                // データ更新処理
                if (form["update"].Equals("1"))
                {
                    // データ編集・更新
                    Warehouse target = new WarehouseDao(db).GetByKey(model.WarehouseCode, true);
                    UpdateModel(target);
                    EditWarehouseForUpdate(target);
                }
                // データ追加処理
                else
                {
                    // データ編集
                    model = EditWarehouseForInsert(model);
                    // データ追加
                    db.Warehouse.InsertOnSubmit(model);
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
                            return View("WarehouseEntry", model);
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
            return Entry(model.WarehouseCode);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="model"></param>
        private void ValidateWarehouse(Warehouse model)
        {
            CommonValidate("WarehouseCode", "倉庫コード", model, true);
            CommonValidate("WarehouseName", "倉庫名", model, true);
            CommonValidate("DepartmentCode", "倉庫管理部門コード", model, true);
        }

        /// <summary>
        /// 画面コンポーネント
        /// </summary>
        /// <param name="form"></param>
        private void SetDataComponent(FormCollection form)
        {
            ViewData["update"] = form["update"];
            
            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

            ViewData["DepartmentName"] = (dep != null ? dep.DepartmentName : "");
        }

        /// <summary>
        /// 倉庫コードから倉庫名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">倉庫コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Warehouse Warehouse = new WarehouseDao(db).GetByKey(code);
                if (Warehouse != null)
                {
                    codeData.Code = Warehouse.WarehouseCode;
                    codeData.Name = Warehouse.WarehouseName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 倉庫マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="brand">倉庫データ(登録内容)</param>
        /// <returns>倉庫マスタモデルクラス</returns>
        private Warehouse EditWarehouseForInsert(Warehouse warehouse)
        {
            warehouse.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            warehouse.CreateDate = DateTime.Now;
            warehouse.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            warehouse.LastUpdateDate = DateTime.Now;
            warehouse.DelFlag = "0";
            return warehouse;
        }

        /// <summary>
        /// 倉庫マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="brand">倉庫データ(登録内容)</param>
        /// <returns>倉庫マスタモデルクラス</returns>
        private Warehouse EditWarehouseForUpdate(Warehouse warehouse)
        {
            warehouse.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            warehouse.LastUpdateDate = DateTime.Now;
            return warehouse;
        }
    }
}
