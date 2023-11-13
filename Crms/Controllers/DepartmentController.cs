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
    /// 部門マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepartmentController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "部門マスタ";     // 画面名
        private static readonly string PROC_NAME = "部門マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DepartmentController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// 部門検索画面表示
        /// </summary>
        /// <returns>部門検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 部門検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            //Del 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② closeMonthFlagの廃止
            //Add 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面判別用のフラグ追加
            //int closeMonthFlag = 0;
            
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            //Mod 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面判別用のフラグ追加
            PaginatedList<Department> list = GetSearchResultList(form);
            
            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 部門検索画面の表示
            return View("DepartmentCriteria", list);
        }

        /// <summary>
        /// 部門検索ダイアログ表示
        /// </summary>
        /// <returns>部門検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 部門検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面ダイアログ</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 車両棚卸フラグ、部品棚卸フラグによる絞り込み条件の追加
        /// 2015/06/11 arc yano その他 CriteriaDialog,CriteriaDialog2の統合
        /// 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 部門テーブルのCloseMonthFlagの検索条件を、クエリ文字で指定できるようにする
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            //Dell 2015/05/27 yano
            //Add 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面判別用のフラグ追加
            //int closeMonthFlag = 0;
            
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            //Add 2017/05/10 arc yano #3762
            form["CarInventoryFlag"] = Request["CarInventoryFlag"];
            form["PartsInventoryFlag"] = Request["PartsInventoryFlag"];
            

            //Mod 2015/05/27
            form["CloseMonthFlag"] = Request["CloseMonthFlag"];

            //Add 2015/06/10
            form["SearchIsNot"] = Request["SearchIsNot"];

            // 検索結果リストの取得
            //Mod 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面判別用のフラグ追加
            PaginatedList<Department> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            //Add 2017/05/10 arc yano #3762
            ViewData["CarInventoryFlag"] = form["CarInventoryFlag"];
            ViewData["PartsInventoryFlag"] = form["PartsInventoryFlag"];

            // 部門検索画面の表示
            return View("DepartmentCriteriaDialog", list);
        }

        //Mod 2015/06/11 arc yano 部門検索ダイアログ統合のため、CriteriaDialog2はコメントアウト
        /*
        //Add 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面用の検索処理追加
        /// <summary>
        /// 部門検索ダイアログ表示
        /// </summary>
        /// <returns>部門検索ダイアログ</returns>
        public ActionResult CriteriaDialog2()
        {
            return CriteriaDialog2(new FormCollection());
        }

        //Mod 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② 部門テーブルのCloseMonthFlagの検索条件を、クエリ文字で指定できるようにする
        //Add 2014/09/08 arc yano IPO対応その２ 月締め処理状況画面用の検索処理追加
        /// <summary>
        /// 部門検索ダイアログ表示２(月締め処理状況画面専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            //Del 2015/05/27
            //int closeMonthFlag = 1;
            
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            //Mod 2015/05/27
            form["CloseMonthFlag"] = "1";

            // 検索結果リストの取得
            PaginatedList<Department> list = GetSearchResultList(form);

            ViewData["FormAction"] = form["FormAction"] = "/Department/CriteriaDialog2";

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // 部門検索画面の表示
            return View("DepartmentCriteriaDialog", list);
        }

        //Add 2014/10/16 arc amii サブシステム仕訳機能移行対応 車両仕入リスト専用の検索ダイアログ追加
        /// <summary>
        /// 部門検索ダイアログ表示
        /// </summary>
        /// <returns>部門検索ダイアログ</returns>
        public ActionResult CriteriaDialog3()
        {
            return CriteriaDialog3(new FormCollection());
        }
        */

        //Add 2014/10/16 arc amii サブシステム仕訳機能移行対応 車両仕入リスト専用の検索ダイアログ追加
        /// <summary>
        /// 部門検索ダイアログ表示２(月締め処理状況画面専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog3(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            // 検索結果リストの取得
            PaginatedList<Department> list = GetSearchResultCarPurchaseList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // 部門検索画面の表示
            return View("DepartmentCriteriaDialog2", list);
        }


        //Add 2014/12/08 arc nakayama 部品在庫検索対応
        /// <summary>
        /// 部門検索ダイアログ表示２(月締め処理状況画面専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面ダイアログ</returns>
        //[AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog4(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["BusinessType"] = Request["BusinessType"];

            // 検索結果リストの取得
            PaginatedList<Department> list = GetSearchResultListForParts(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["BusinessType"] = form["BusinessType"];

            // 部門検索画面の表示
            return View("DepartmentCriteriaDialog", list);
        }


        //Add 2014/15/18 arc nakayama ルックアップ見直し対応
        /// <summary>
        /// 部門検索ダイアログ表示
        /// </summary>
        /// <returns>部門検索ダイアログ</returns>
        public ActionResult CriteriaDialog5()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            return CriteriaDialog5(form);
        }
        
        //Add 2014/15/18 arc nakayama ルックアップ見直し対応
        /// <summary>
        /// 部門検索ダイアログ表示（検索条件にDelFlagあり）
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog5(FormCollection form)
        {
            if (criteriaInit)
            {
                form["DelFlag"] = "0";
                ViewData["DelFlag"] = form["DelFlag"];
                // 検索結果リストの取得
                PaginatedList<Department> list = GetSearchResultListDelflag(form);
                // 部門検索画面の表示
                return View("DepartmentCriteriaDialog3", list);
            }
            else
            {
                // 検索条件の設定
                // (クエリストリングを検索条件に使用する為、Requestを使用。
                //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
                form["CompanyCode"] = Request["CompanyCode"];
                form["CompanyName"] = Request["CompanyName"];
                form["OfficeCode"] = Request["OfficeCode"];
                form["OfficeName"] = Request["OfficeName"];
                form["DepartmentCode"] = Request["DepartmentCode"];
                form["DepartmentName"] = Request["DepartmentName"];
                form["EmployeeName"] = Request["EmployeeName"];
                form["DelFlag"] = Request["DelFlag"];
                form["BusinessType"] = Request["BusinessType"];

                // 検索結果リストの取得
                PaginatedList<Department> list = GetSearchResultListDelflag(form);

                // その他出力項目の設定
                ViewData["CompanyCode"] = form["CompanyCode"];
                ViewData["CompanyName"] = form["CompanyName"];
                ViewData["OfficeCode"] = form["OfficeCode"];
                ViewData["OfficeName"] = form["OfficeName"];
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                ViewData["DepartmentName"] = form["DepartmentName"];
                ViewData["EmployeeName"] = form["EmployeeName"];
                ViewData["BusinessType"] = form["BusinessType"];
                ViewData["DelFlag"] = form["DelFlag"];

                // 部門検索画面の表示
                return View("DepartmentCriteriaDialog3", list);
            }
        }




        /// <summary>
        /// 部門マスタ入力画面表示
        /// </summary>
        /// <param name="id">部門コード(更新時のみ設定)</param>
        /// <returns>部門マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Department department;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                department = new Department();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                department = new DepartmentDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(department);

            // 出口
            return View("DepartmentEntry", department);
        }

        /// <summary>
        /// 部門マスタ追加更新
        /// </summary>
        /// <param name="department">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>部門マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Department department, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateDepartment(department);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(department);
                return View("DepartmentEntry", department);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                Department targetDepartment = new DepartmentDao(db).GetByKey(department.DepartmentCode, true);
                UpdateModel(targetDepartment);
                EditDepartmentForUpdate(targetDepartment);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                department = EditDepartmentForInsert(department);

                // データ追加
                db.Department.InsertOnSubmit(department);                
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

                        ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0010", new string[] { "部門コード", "保存" }));
                        GetEntryViewData(department);
                        return View("DepartmentEntry", department);
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
            return Entry(department.DepartmentCode);
        }

        
        /// <summary>
        /// 部門コードから部門名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2015/06/19 arc yano その他 includeDeletedのデフォルト値をtrue⇒falseに変更
        /// 2015/05/27 arc yano IPO対応(部品棚卸) closeMonthFlagを追加
        /// </history>
        public ActionResult GetMaster(string code, bool includeDeleted = false, string closeMonthFlag = "")
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted, closeMonthFlag);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;

                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部門コードから部門情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetail(string code, bool includeDeleted = false)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted);

                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部門コードから部門名を取得する(車両のみ) (Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster2(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByCarDepartment(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部門コードからを取得する(車両棚卸対象部門のみ) (Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult GetMasterForCarInventory(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByCarInventory(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部門コードから取得する(部品棚卸対象部門のみ) (Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public ActionResult GetMasterForPartsInventory(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByPartsInventory(code);
                if (department != null)
                {
                    codeData.Code = department.DepartmentCode;
                    codeData.Name = department.DepartmentName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部門コードからデフォルト仕入先を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2018/07/27 yano.hiroki #3923 部門マスタにデフォルト仕入先を設定　新規作成
        /// </history>
        public ActionResult GetSupplierFromDepCode(string code, bool includeDeleted = false, string closeMonthFlag = "")
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Department department = new DepartmentDao(db).GetByKey(code, includeDeleted, closeMonthFlag);

                if (department != null)
                {
                    Supplier supp = new SupplierDao(db).GetByKey(department.DefaultSupplierCode);

                    if (supp != null)
                    {
                        codeData.Code = supp.SupplierCode;
                        codeData.Name = supp.SupplierName;
                    }
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="department">モデルデータ</param>
        /// <history>
        ///  2018/07/27 yano.hiroki #3923 部門マスタにデフォルト仕入先を設定
        /// </history>
        private void GetEntryViewData(Department department)
        {
            // エリア名の取得
            if (!string.IsNullOrEmpty(department.AreaCode))
            {
                AreaDao areaDao = new AreaDao(db);
                Area area = areaDao.GetByKey(department.AreaCode);
                if (area != null)
                {
                    ViewData["AreaName"] = area.AreaName;
                }
            }

            // 事業所名の取得
            if (!string.IsNullOrEmpty(department.OfficeCode))
            {
                OfficeDao officeDao = new OfficeDao(db);
                Office office = officeDao.GetByKey(department.OfficeCode);
                if (office != null)
                {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }

            // 部門長名の取得
            if (!string.IsNullOrEmpty(department.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(department.EmployeeCode);
                if (employee != null)
                {
                    department.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // Add 2018/07/27 yano.hiroki #3923
            // 既定の仕入先名の取得
            if (!string.IsNullOrEmpty(department.DefaultSupplierCode))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(department.DefaultSupplierCode);
                if (supplier != null)
                {
                    ViewData["DefaultSupplierName"] = supplier.SupplierName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), department.DepositKind, true);
            ViewData["BusinessTypeList"] = CodeUtils.GetSelectListByModel(dao.GetBusinessTypeAll(false), department.BusinessType, true);

        }

       
        /// 部門マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門マスタ検索結果リスト</returns>
        /// <history>
        ///  2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        ///  2015/06/11 arc yano その他 CriteriaDialog,CriteriaDialog2の統合
        ///  2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更② クエリ文字列でCloseMonthFlagを設定できるように修正
        ///  2014/09/08 arc yano IPO対応その２ 月締め処理状況画面判別用のフラグ追加
        /// <summary>
        /// </history>
        private PaginatedList<Department> GetSearchResultList(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];

            //Mod 2015/05/27 arc yano
            departmentCondition.CloseMonthFlag = form["CloseMonthFlag"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            //Add 2017/05/10 arc yano #3762
            departmentCondition.PartsInventoryFlag = form["PartsInventoryFlag"];
            departmentCondition.CarInventoryFlag = form["CarInventoryFlag"];


            //Add 2015/06/11 arc yano
            //検索方法指定
            bool searchIsNot = bool.Parse(string.IsNullOrWhiteSpace(form["SearchIsNot"]) ? "false" : form["SearchIsNot"]);


            //Mod 2015/06/11 arc yano GetListByCondition, GetListByCondition2の統合
            /*
            //Mod 2015/05/27 arc yano
            //Add 2014/09/08 arc yano 月締め処理状況画面からダイアログを開いた場合は、検索処理を別にする。
            if (string.IsNullOrWhiteSpace(departmentCondition.CloseMonthFlag) || departmentCondition.CloseMonthFlag.Equals("0"))
            {
            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
            else
            {
                return departmentDao.GetListByCondition2(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
            */

            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE, searchIsNot);
            
        }

        /// <summary>
        /// 部門マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門マスタ検索結果リスト</returns>
        /// <history>
        /// 2014/10/16 arc amii サブシステム仕訳機能移行対応 車両仕入リスト専用の検索ダイアログ追加
        /// </history>
        private PaginatedList<Department> GetSearchResultCarPurchaseList(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByCondition3(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        
        //Add 2014/12/08 arc nakayama 部品在庫検索対応
        /// <summary>
        /// 部門マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門マスタ検索結果リスト</returns>
        private PaginatedList<Department> GetSearchResultListForParts(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByConditionForParts(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }

        //Add 2014/15/18 arc nakayama ルックアップ見直し対応
        /// <summary>
        /// 部門マスタ検索結果リスト取得（検索条件にDelFlagあり）
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部門マスタ検索結果リスト</returns>
        private PaginatedList<Department> GetSearchResultListDelflag(FormCollection form)
        {
            DepartmentDao departmentDao = new DepartmentDao(db);
            Department departmentCondition = new Department();
            departmentCondition.DepartmentCode = form["DepartmentCode"];
            departmentCondition.DepartmentName = form["DepartmentName"];
            departmentCondition.Office = new Office();
            departmentCondition.Office.OfficeCode = form["OfficeCode"];
            departmentCondition.Office.OfficeName = form["OfficeName"];
            departmentCondition.Office.Company = new Company();
            departmentCondition.Office.Company.CompanyCode = form["CompanyCode"];
            departmentCondition.Office.Company.CompanyName = form["CompanyName"];
            departmentCondition.Employee = new Employee();
            departmentCondition.Employee.EmployeeName = form["EmployeeName"];
            departmentCondition.BusinessType = form["BusinessType"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                departmentCondition.DelFlag = form["DelFlag"];
            }

            return departmentDao.GetListByCondition(departmentCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }


        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="department">部門データ</param>
        /// <returns>部門データ</returns>
        private Department ValidateDepartment(Department department)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(department.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門コード"));
            }
            if (string.IsNullOrEmpty(department.DepartmentName))
            {
                ModelState.AddModelError("DepartmentName", MessageUtils.GetMessage("E0001", "部門名"));
            }
            if (string.IsNullOrEmpty(department.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0001", "エリア"));
            }
            if (string.IsNullOrEmpty(department.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "事業所"));
            }
            if (string.IsNullOrEmpty(department.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "部門長"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("DepartmentCode") && !CommonUtils.IsAlphaNumeric(department.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0012", "部門コード"));
            }

            return department;
        }

        /// <summary>
        /// 部門マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="department">部門データ(登録内容)</param>
        /// <returns>部門マスタモデルクラス</returns>
        private Department EditDepartmentForInsert(Department department)
        {
            department.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            department.CreateDate = DateTime.Now;
            //department.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //department.LastUpdateDate = DateTime.Now;
            department.DelFlag = "0";
            return EditDepartmentForUpdate(department);
        }

        /// <summary>
        /// 部門マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="department">部門データ(登録内容)</param>
        /// <returns>部門マスタモデルクラス</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 車両棚卸フラグ、部品棚卸フラグの編集機能の追加
        /// </history>
        private Department EditDepartmentForUpdate(Department department)
        {
            department.PrintFlag = department.PrintFlag.Contains("true") ? "1" : "0";
            department.CarInventoryFlag = department.CarInventoryFlag.Contains("true") ? "1" : "0";     //Add  2017/05/10 arc yano #3762
            department.PartsInventoryFlag = department.PartsInventoryFlag.Contains("true") ? "1" : "0"; //Add  2017/05/10 arc yano #3762
            department.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            department.LastUpdateDate = DateTime.Now;
            return department;
        }

    }
}
