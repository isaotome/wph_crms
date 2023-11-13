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
    /// 事業所マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class OfficeController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "事業所マスタ";     // 画面名
        private static readonly string PROC_NAME = "事業所マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OfficeController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 事業所検索画面表示
        /// </summary>
        /// <returns>事業所検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 事業所検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>事業所検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Office> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 事業所検索画面の表示
            return View("OfficeCriteria", list);
        }

        /// <summary>
        /// 事業所検索ダイアログ表示
        /// </summary>
        /// <returns>事業所検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 事業所検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>事業所検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["EmployeeName"] = Request["EmployeeName"];

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Office> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // 事業所検索画面の表示
            return View("OfficeCriteriaDialog", list);
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所検索ダイアログ表示 (勘定奉行データ出力画面専用)
        /// </summary>
        /// <returns>事業所検索ダイアログ</returns>
        public ActionResult CriteriaDialog2()
        {
            return CriteriaDialog2(new FormCollection());
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所検索ダイアログ表示 (勘定奉行データ出力画面専用)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>事業所検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {

            string divisionType = CommonUtils.DefaultString(Request["DivisionType"]);
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["OfficeCode"] = Request["OfficeCode"];
            form["OfficeName"] = Request["OfficeName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DivisionType"] = divisionType;

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Office> list = GetSearchDivisionResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            ViewData["OfficeName"] = form["OfficeName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DivisionType"] = form["DivisionType"];

            // 事業所検索画面の表示
            return View("OfficeCriteriaDialog2", list);
        }

        /// <summary>
        /// 事業所マスタ入力画面表示
        /// </summary>
        /// <param name="id">事業所コード(更新時のみ設定)</param>
        /// <returns>事業所マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Office office;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                office = new Office();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                office = new OfficeDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(office);

            // 出口
            return View("OfficeEntry", office);
        }

        /// <summary>
        /// 事業所マスタ追加更新
        /// </summary>
        /// <param name="area">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>事業所マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Office office, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateOffice(office);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(office);
                return View("OfficeEntry", office);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
                Office targetOffice = new OfficeDao(db).GetByKey(office.OfficeCode, true);
                UpdateModel(targetOffice);
                EditOfficeForUpdate(targetOffice);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                office = EditOfficeForInsert(office);

                // データ追加
                db.Office.InsertOnSubmit(office);
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

                        ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0010", new string[] { "事業所コード", "保存" }));
                        GetEntryViewData(office);
                        return View("OfficeEntry", office);
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

            //MOD 2014/10/29 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(office.OfficeCode);
        }

        /// <summary>
        /// 事業所コードから事業所名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">事業所コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Office office = new OfficeDao(db).GetByKey(code);
                if (office != null)
                {
                    codeData.Code = office.OfficeCode;
                    codeData.Name = office.OfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所コードから事業所名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">事業所コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster2(string code, string divType)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Office office = new OfficeDao(db).GetByDivisionKey(code, divType);
                if (office != null)
                {
                    codeData.Code = office.OfficeCode;
                    codeData.Name = office.OfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="office">モデルデータ</param>
        private void GetEntryViewData(Office office)
        {
            // 会社名の取得
            if (!string.IsNullOrEmpty(office.CompanyCode))
            {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(office.CompanyCode);
                if (company != null)
                {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }

            // 事業所長名の取得
            if (!string.IsNullOrEmpty(office.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(office.EmployeeCode);
                if (employee != null)
                {
                    office.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), office.DepositKind, true);
        }

        /// <summary>
        /// 事業所マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>事業所マスタ検索結果リスト</returns>
        private PaginatedList<Office> GetSearchResultList(FormCollection form)
        {
            OfficeDao officeDao = new OfficeDao(db);
            Office officeCondition = new Office();
            officeCondition.OfficeCode = form["OfficeCode"];
            officeCondition.OfficeName = form["OfficeName"];
            officeCondition.Employee = new Employee();
            officeCondition.Employee.EmployeeName = form["EmployeeName"];
            officeCondition.Company = new Company();
            officeCondition.Company.CompanyCode = form["CompanyCode"];
            officeCondition.Company.CompanyName = form["CompanyName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                officeCondition.DelFlag = form["DelFlag"];
            }
            return officeDao.GetListByCondition(officeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Add 2014/10/27 arc amii サブシステム仕訳機能移行対応 勘定奉行データ出力画面専用の検索ダイアログ追加
        /// <summary>
        /// 事業所マスタ検索結果リスト取得(拠点指定)
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>事業所マスタ検索結果リスト</returns>
        private PaginatedList<Office> GetSearchDivisionResultList(FormCollection form)
        {
            OfficeDao officeDao = new OfficeDao(db);
            Office officeCondition = new Office();
            officeCondition.OfficeCode = form["OfficeCode"];
            officeCondition.OfficeName = form["OfficeName"];
            officeCondition.Employee = new Employee();
            officeCondition.Employee.EmployeeName = form["EmployeeName"];
            officeCondition.Company = new Company();
            officeCondition.Company.CompanyCode = form["CompanyCode"];
            officeCondition.Company.CompanyName = form["CompanyName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                officeCondition.DelFlag = form["DelFlag"];
            }
            return officeDao.GetListDivCondition(officeCondition, form["DivisionType"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="office">事業所データ</param>
        /// <returns>事業所データ</returns>
        private Office ValidateOffice(Office office)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(office.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "事業所コード"));
            }
            if (string.IsNullOrEmpty(office.OfficeName))
            {
                ModelState.AddModelError("OfficeName", MessageUtils.GetMessage("E0001", "事業所名"));
            }
            if (string.IsNullOrEmpty(office.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "会社"));
            }
            if (string.IsNullOrEmpty(office.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "事業所長"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("OfficeCode") && !CommonUtils.IsAlphaNumeric(office.OfficeCode))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0012", "事業所コード"));
            }

            return office;
        }

        /// <summary>
        /// 事業所マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="office">事業所データ(登録内容)</param>
        /// <returns>事業所マスタモデルクラス</returns>
        private Office EditOfficeForInsert(Office office)
        {
            office.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            office.CreateDate = DateTime.Now;
            //office.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //office.LastUpdateDate = DateTime.Now;
            office.DelFlag = "0";
            return EditOfficeForUpdate(office);
        }

        /// <summary>
        /// 事業所マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="office">事業所データ(登録内容)</param>
        /// <returns>事業所マスタモデルクラス</returns>
        private Office EditOfficeForUpdate(Office office)
        {
            //office.PrintFlagCar = office.PrintFlagCar.Contains("true") ? "1" : "0";
            //office.PrintFlagService = office.PrintFlagService.Contains("true") ? "1" : "0";
            office.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            office.LastUpdateDate = DateTime.Now;
            return office;
        }

    }
}
