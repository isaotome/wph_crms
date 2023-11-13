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
    /// エリアマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class AreaController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "エリアマスタ";     // 画面名
        private static readonly string PROC_NAME = "エリアマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AreaController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// エリア検索画面表示
        /// </summary>
        /// <returns>エリア検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// エリア検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>エリア検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Area> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["AreaCode"] = form["AreaCode"];
            ViewData["AreaName"] = form["AreaName"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // エリア検索画面の表示
            return View("AreaCriteria", list);
        }

        /// <summary>
        /// エリア検索ダイアログ表示
        /// </summary>
        /// <returns>エリア検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// エリア検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>エリア検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["AreaCode"] = Request["AreaCode"];
            form["AreaName"] = Request["AreaName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Area> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["AreaCode"] = form["AreaCode"];
            ViewData["AreaName"] = form["AreaName"];
            ViewData["EmployeeName"] = form["EmployeeName"];

            // エリア検索画面の表示
            return View("AreaCriteriaDialog", list);
        }

        /// <summary>
        /// エリアマスタ入力画面表示
        /// </summary>
        /// <param name="id">エリアコード(更新時のみ設定)</param>
        /// <returns>エリアマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Area area;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                area = new Area();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                area = new AreaDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(area);

            // 出口
            return View("AreaEntry", area);
        }

        /// <summary>
        /// エリアマスタ追加更新
        /// </summary>
        /// <param name="area">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>エリアマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Area area, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateArea(area);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(area);
                return View("AreaEntry", area);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                Area targetArea = new AreaDao(db).GetByKey(area.AreaCode, true);
                UpdateModel(targetArea);
                EditAreaForUpdate(targetArea);
            }
            // データ追加処理
            else
            {
                // データ編集
                area = EditAreaForInsert(area);

                // データ追加
                db.Area.InsertOnSubmit(area);                
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

                        ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0010", new string[] { "エリアコード", "保存" }));
                        GetEntryViewData(area);
                        return View("AreaEntry", area);
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
            return Entry(area.AreaCode);
        }

        /// <summary>
        /// エリアコードからエリア名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">エリアコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Area area = new AreaDao(db).GetByKey(code);
                if (area != null)
                {
                    codeData.Code = area.AreaCode;
                    codeData.Name = area.AreaName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="area">モデルデータ</param>
        private void GetEntryViewData(Area area)
        {
            // エリア長名の取得
            if (!string.IsNullOrEmpty(area.EmployeeCode))
            {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(area.EmployeeCode);
                if (employee != null)
                {
                    area.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }
        }

        /// <summary>
        /// エリアマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>エリアマスタ検索結果リスト</returns>
        private PaginatedList<Area> GetSearchResultList(FormCollection form)
        {
            AreaDao areaDao = new AreaDao(db);
            Area areaCondition = new Area();
            areaCondition.AreaCode = form["AreaCode"];
            areaCondition.AreaName = form["AreaName"];
            areaCondition.Employee = new Employee();
            areaCondition.Employee.EmployeeName = form["EmployeeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                areaCondition.DelFlag = form["DelFlag"];
            }
            return areaDao.GetListByCondition(areaCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="area">エリアデータ</param>
        /// <returns>エリアデータ</returns>
        private Area ValidateArea(Area area)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(area.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0001", "エリアコード"));
            }
            if (string.IsNullOrEmpty(area.AreaName))
            {
                ModelState.AddModelError("AreaName", MessageUtils.GetMessage("E0001", "エリア名"));
            }
            if (string.IsNullOrEmpty(area.EmployeeCode))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "エリア長"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("AreaCode") && !CommonUtils.IsAlphaNumeric(area.AreaCode))
            {
                ModelState.AddModelError("AreaCode", MessageUtils.GetMessage("E0012", "エリアコード"));
            }

            return area;
        }

        /// <summary>
        /// エリアマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="area">エリアデータ(登録内容)</param>
        /// <returns>エリアマスタモデルクラス</returns>
        private Area EditAreaForInsert(Area area)
        {
            area.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.CreateDate = DateTime.Now;
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.LastUpdateDate = DateTime.Now;
            area.DelFlag = "0";
            return area;
        }

        /// <summary>
        /// エリアマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="area">エリアデータ(登録内容)</param>
        /// <returns>エリアマスタモデルクラス</returns>
        private Area EditAreaForUpdate(Area area)
        {
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.LastUpdateDate = DateTime.Now;
            return area;
        }

    }
}
