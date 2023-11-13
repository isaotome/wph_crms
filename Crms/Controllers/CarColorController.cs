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
using Crms.Models;                      //Add 2014/08/01 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 車両カラーマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarColorController : Controller
    {
        //Add 2014/08/01 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両カラーマスタ";     // 画面名
        private static readonly string PROC_NAME = "車両カラーマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarColorController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 車両カラー検索画面表示
        /// </summary>
        /// <returns>車両カラー検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両カラー検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両カラー検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<CarColor> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorCategory"], true);
            ViewData["CarColorCode"] = form["CarColorCode"];
            ViewData["CarColorName"] = form["CarColorName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["MakerColorCOde"] = form["MakerColorCode"];

            // 車両カラー検索画面の表示
            return View("CarColorCriteria", list);
        }

        /// <summary>
        /// 車両カラー検索ダイアログ表示
        /// </summary>
        /// <returns>車両カラー検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 車両カラー検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両カラー検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["ColorCategory"] = Request["ColorCategory"];
            form["CarColorCode"] = Request["CarColorCode"];
            form["CarColorName"] = Request["CarColorName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["CarGradeCode"] = Request["CarGradeCode"];
            form["InteriorColorFlag"] = Request["InteriorColorFlag"];
            form["ExteriorColorFlag"] = Request["ExteriorColorFlag"];
            // 検索結果リストの取得
            PaginatedList<CarColor> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorCategory"], true);
            ViewData["CarColorCode"] = form["CarColorCode"];
            ViewData["CarColorName"] = form["CarColorName"];
            ViewData["MakerColorCode"] = form["MakerColorCode"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            ViewData["InteriorColorFlag"] = form["InteriorColorFlag"]!=null && form["InteriorColorFlag"].Contains("true");
            ViewData["ExteriorColorFlag"] = form["ExteriorColorFlag"]!=null && form["ExteriorColorFlag"].Contains("true");
            // 車両カラー検索画面の表示
            return View("CarColorCriteriaDialog", list);
        }

        /// <summary>
        /// 車両カラーマスタ入力画面表示
        /// </summary>
        /// <param name="id">車両カラーコード(更新時のみ設定)</param>
        /// <returns>車両カラーマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarColor carColor;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carColor = new CarColor();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                carColor = new CarColorDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(carColor);

            // 出口
            return View("CarColorEntry", carColor);
        }

        /// <summary>
        /// 車両カラーマスタ追加更新
        /// </summary>
        /// <param name="carColor">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両カラーマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarColor carColor, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCarColor(carColor);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(carColor);
                return View("CarColorEntry", carColor);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                CarColor targetCarColor = new CarColorDao(db).GetByKey(carColor.CarColorCode, true);
                UpdateModel(targetCarColor);
                EditCarColorForUpdate(targetCarColor, form);
            }
            // データ追加処理
            else
            {
                // データ編集
                carColor = EditCarColorForInsert(carColor, form);
                // データ追加
                db.CarColor.InsertOnSubmit(carColor);
            }

            // Add 2014/08/04 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
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
                        ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0010", new string[] { "車両カラーコード", "保存" }));
                        GetEntryViewData(carColor);
                        return View("CarColorEntry", carColor);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
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
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carColor.CarColorCode);
        }

        /// <summary>
        /// 車両カラーコードから車両カラー名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">車両カラーコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarColor carColor = new CarColorDao(db).GetByKey(code);
                if (carColor != null)
                {
                    codeData.Code = carColor.CarColorCode;
                    codeData.Name = carColor.CarColorName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        public ActionResult GetMaster2(string carGradeCode, string carColorCode) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CarAvailableColor carColor = new CarAvailableColorDao(db).GetByKey(carGradeCode, carColorCode);
                if (carColor != null) {
                    codeData.Code = carColor.CarGradeCode;
                    codeData.Code2 = carColor.CarColorCode;
                    codeData.Name = carColor.CarColor.CarColorName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carColor">モデルデータ</param>
        private void GetEntryViewData(CarColor carColor)
        {
            // メーカー名の取得
            if (!string.IsNullOrEmpty(carColor.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(carColor.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["ColorCategoryList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), carColor.ColorCategory, true);
        }

        /// <summary>
        /// 車両カラーマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両カラーマスタ検索結果リスト</returns>
        private PaginatedList<CarColor> GetSearchResultList(FormCollection form)
        {
            CarColorDao carColorDao = new CarColorDao(db);
            CarColor carColorCondition = new CarColor();
            carColorCondition.CarColorCode = form["CarColorCode"];
            carColorCondition.CarColorName = form["CarColorName"];
            carColorCondition.Maker = new Maker();
            carColorCondition.Maker.MakerCode = form["MakerCode"];
            carColorCondition.Maker.MakerName = form["MakerName"];
            carColorCondition.ColorCategory = form["ColorCategory"];
            carColorCondition.MakerColorCode = form["MakerColorCode"];
            carColorCondition.CarGradeCode = form["CarGradeCode"];
            carColorCondition.InteriorColorFlag = form["InteriorColorFlag"]!=null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColorCondition.ExteriorColorFlag = form["ExteriorColorFlag"]!=null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carColorCondition.DelFlag = form["DelFlag"];
            }
            return carColorDao.GetListByCondition(carColorCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carColor">車両カラーデータ</param>
        /// <returns>車両カラーデータ</returns>
        private CarColor ValidateCarColor(CarColor carColor)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(carColor.CarColorCode))
            {
                ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0001", "車両カラーコード"));
            }
            if (string.IsNullOrEmpty(carColor.CarColorName))
            {
                ModelState.AddModelError("CarColorName", MessageUtils.GetMessage("E0001", "車両カラー名"));
            }
            if (string.IsNullOrEmpty(carColor.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "メーカー"));
            }
            if (string.IsNullOrEmpty(carColor.MakerColorCode)) {
                ModelState.AddModelError("MakerColorCode", MessageUtils.GetMessage("E0001", "メーカーカラーコード"));
            }
            // フォーマットチェック
            if (ModelState.IsValidField("CarColorCode") && !CommonUtils.IsAlphaNumeric(carColor.CarColorCode))
            {
                ModelState.AddModelError("CarColorCode", MessageUtils.GetMessage("E0012", "車両カラーコード"));
            }

            return carColor;
        }

        /// <summary>
        /// 車両カラーマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carColor">車両カラーデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両カラーマスタモデルクラス</returns>
        private CarColor EditCarColorForInsert(CarColor carColor, FormCollection form)
        {
            carColor.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.CreateDate = DateTime.Now;
            carColor.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.LastUpdateDate = DateTime.Now;
            carColor.DelFlag = "0";
            //2015.10.14 nishimura.akira　内装色、外装色の登録対応
            carColor.InteriorColorFlag = form["InteriorColorFlag"] != null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColor.ExteriorColorFlag = form["ExteriorColorFlag"] != null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";
            return carColor;
        }

        /// <summary>
        /// 車両カラーマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carColor">車両カラーデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両カラーマスタモデルクラス</returns>
        private CarColor EditCarColorForUpdate(CarColor carColor, FormCollection form)
        {
            carColor.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carColor.LastUpdateDate = DateTime.Now;
            //2015.10.14 nishimura.akira　内装色、外装色の登録対応
            carColor.InteriorColorFlag = form["InteriorColorFlag"] != null && form["InteriorColorFlag"].Contains("true") ? "1" : "";
            carColor.ExteriorColorFlag = form["ExteriorColorFlag"] != null && form["ExteriorColorFlag"].Contains("true") ? "1" : "";
            return carColor;
        }

    }
}
