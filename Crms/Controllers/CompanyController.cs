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
    /// 会社マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0,VaryByParam="none")]
    public class CompanyController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "会社マスタ";     // 画面名
        private static readonly string PROC_NAME = "会社マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CompanyController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 会社検索画面表示
        /// </summary>
        /// <returns>会社検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 会社検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>会社検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Company> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 会社検索画面の表示
            return View("CompanyCriteria", list);
        }

        /// <summary>
        /// 会社検索ダイアログ表示
        /// </summary>
        /// <returns>会社検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 会社検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>会社検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Company> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // 会社検索画面の表示
            return View("CompanyCriteriaDialog", list);
        }

        /// <summary>
        /// 会社マスタ入力画面表示
        /// </summary>
        /// <param name="id">会社コード(更新時のみ設定)</param>
        /// <returns>会社マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Company company;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                company = new Company();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                company = new CompanyDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(company);

            // 出口
            return View("CompanyEntry", company);
        }

        /// <summary>
        /// 会社マスタ追加更新
        /// </summary>
        /// <param name="area">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>会社マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Company company, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCompany(company,form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(company);
                return View("CompanyEntry", company);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                Company targetCompany = new CompanyDao(db).GetByKey(company.CompanyCode, true);
                UpdateModel(targetCompany);
                EditCompanyForUpdate(targetCompany);
            }
            // データ追加処理
            else
            {
                // データ編集
                company = EditCompanyForInsert(company);

                // データ追加
                db.Company.InsertOnSubmit(company);                
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

                        ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0010", new string[] { "会社コード", "保存" }));
                        GetEntryViewData(company);
                        return View("CompanyEntry", company);
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
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(company.CompanyCode);
        }

        /// <summary>
        /// 会社コードから会社名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">会社コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Company company = new CompanyDao(db).GetByKey(code);
                if (company != null)
                {
                    codeData.Code = company.CompanyCode;
                    codeData.Name = company.CompanyName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="company">モデルデータ</param>
        private void GetEntryViewData(Company company)
        {
            // 代表者名の取得
            if (!string.IsNullOrEmpty(company.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(company.EmployeeCode);
                if (employee != null)
                {
                    company.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }
        }

        /// <summary>
        /// 会社マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>会社マスタ検索結果リスト</returns>
        private PaginatedList<Company> GetSearchResultList(FormCollection form)
        {
            CompanyDao companyDao = new CompanyDao(db);
            Company companyCondition = new Company();
            companyCondition.CompanyCode = form["CompanyCode"];
            companyCondition.CompanyName = form["CompanyName"];
            companyCondition.Employee = new Employee();
            companyCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                companyCondition.DelFlag = form["DelFlag"];
            }
            return companyDao.GetListByCondition(companyCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="company">会社データ</param>
        /// <returns>会社データ</returns>
        private Company ValidateCompany(Company company,FormCollection form)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(company.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "会社コード"));
            }
            if (string.IsNullOrEmpty(company.CompanyName))
            {
                ModelState.AddModelError("CompanyName", MessageUtils.GetMessage("E0001", "会社名"));
            }
            if (string.IsNullOrEmpty(company.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "代表者"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CompanyCode") && !CommonUtils.IsAlphaNumeric(company.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0012", "会社コード"));
            }

            // 値チェック
            if (!form["update"].Equals("1") && ModelState.IsValidField("CompanyCode") && !new SerialNumberDao(db).CanUserCompanyCode(company.CompanyCode)) {
                ModelState.AddModelError("CompanyCode", "指定された会社コードは既存の車両マスタのコード体系と重複するため使用できません");
            }
            return company;
        }

        /// <summary>
        /// 会社マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="company">会社データ(登録内容)</param>
        /// <returns>会社マスタモデルクラス</returns>
        private Company EditCompanyForInsert(Company company)
        {
            company.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.CreateDate = DateTime.Now;
            company.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.LastUpdateDate = DateTime.Now;
            company.DelFlag = "0";
            return company;
        }

        /// <summary>
        /// 会社マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="company">会社データ(登録内容)</param>
        /// <returns>会社マスタモデルクラス</returns>
        private Company EditCompanyForUpdate(Company company)
        {
            company.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            company.LastUpdateDate = DateTime.Now;
            return company;
        }

    }
}
