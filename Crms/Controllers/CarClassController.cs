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
using System.Transactions;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 車両クラスマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarClassController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両クラスマスタ";     // 画面名
        private static readonly string PROC_NAME = "車両クラスマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarClassController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 車両クラス検索画面表示
        /// </summary>
        /// <returns>車両クラス検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両クラス検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両クラス検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<CarClass> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 車両クラス検索画面の表示
            return View("CarClassCriteria", list);
        }

        /// <summary>
        /// 車両クラス検索ダイアログ表示
        /// </summary>
        /// <returns>車両クラス検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 車両クラス検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両クラス検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CarClassCode"] = Request["CarClassCode"];
            form["CarClassName"] = Request["CarClassName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<CarClass> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];

            // 車両クラス検索画面の表示
            return View("CarClassCriteriaDialog", list);
        }

        /// <summary>
        /// 車両クラスマスタ入力画面表示
        /// </summary>
        /// <param name="id">車両クラスコード(更新時のみ設定)</param>
        /// <returns>車両クラスマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarClass carClass;

            // 表示データ設定(追加の場合)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carClass = new CarClass();
            }
            // 表示データ設定(更新の場合)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                carClass = new CarClassDao(db).GetByKey(id, true);
            }

            // 出口
            return View("CarClassEntry", carClass);
        }

        /// <summary>
        /// 車両クラスマスタ追加更新
        /// </summary>
        /// <param name="carClass">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両クラスマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarClass carClass, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCarClass(carClass);
            if (!ModelState.IsValid)
            {
                return View("CarClassEntry", carClass);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope())
            {
                // データ更新処理
                if (form["update"].Equals("1"))
                {
                    //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                    CarClass targetCarClass = new CarClassDao(db).GetByKey(carClass.CarClassCode, true);
                    UpdateModel(targetCarClass);
                    EditCarClassForUpdate(targetCarClass);
                }
                // データ追加処理
                else
                {
                    // データ編集
                    carClass = EditCarClassForInsert(carClass);
                    // データ追加
                    db.CarClass.InsertOnSubmit(carClass);
                }

                // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        if (form["update"].Equals("1"))
                        {
                            db.ChangeServiceCostMulti(null, carClass.CarClassCode, carClass.DelFlag, carClass.LastUpdateEmployeeCode);
                        }
                        else
                        {
                            db.AddServiceCostMulti(null, carClass.CarClassCode, carClass.CreateEmployeeCode);
                        }

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
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Add 2014/08/04 arc amii エラーログ対応 セッションにSQL文を登録する処理追加
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/04 arc amii エラーログ対応 エラーログ出力処理追加
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0010", new string[] { "車両クラスコード", "保存" }));
                            return View("CarClassEntry", carClass);
                        }
                        else
                        {
                            //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carClass.CarClassCode);
        }

        /// <summary>
        /// 車両クラスコードから車両クラス名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">車両クラスコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarClass carClass = new CarClassDao(db).GetByKey(code);
                if (carClass != null)
                {
                    codeData.Code = carClass.CarClassCode;
                    codeData.Name = carClass.CarClassName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 車両クラスマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両クラスマスタ検索結果リスト</returns>
        private PaginatedList<CarClass> GetSearchResultList(FormCollection form)
        {
            CarClassDao carClassDao = new CarClassDao(db);
            CarClass carClassCondition = new CarClass();
            carClassCondition.CarClassCode = form["CarClassCode"];
            carClassCondition.CarClassName = form["CarClassName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carClassCondition.DelFlag = form["DelFlag"];
            }
            return carClassDao.GetListByCondition(carClassCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carClass">車両クラスデータ</param>
        /// <returns>車両クラスデータ</returns>
        private CarClass ValidateCarClass(CarClass carClass)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(carClass.CarClassCode))
            {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0001", "車両クラスコード"));
            }
            if (string.IsNullOrEmpty(carClass.CarClassName))
            {
                ModelState.AddModelError("CarClassName", MessageUtils.GetMessage("E0001", "車両クラス名"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CarClassCode") && !CommonUtils.IsAlphaNumeric(carClass.CarClassCode))
            {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0012", "車両クラスコード"));
            }

            return carClass;
        }

        /// <summary>
        /// 車両クラスマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carClass">車両クラスデータ(登録内容)</param>
        /// <returns>車両クラスマスタモデルクラス</returns>
        private CarClass EditCarClassForInsert(CarClass carClass)
        {
            carClass.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.CreateDate = DateTime.Now;
            carClass.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.LastUpdateDate = DateTime.Now;
            carClass.DelFlag = "0";
            return carClass;
        }

        /// <summary>
        /// 車両クラスマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carClass">車両クラスデータ(登録内容)</param>
        /// <returns>車両クラスマスタモデルクラス</returns>
        private CarClass EditCarClassForUpdate(CarClass carClass)
        {
            carClass.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.LastUpdateDate = DateTime.Now;
            return carClass;
        }

    }
}
