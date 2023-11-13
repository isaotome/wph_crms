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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 車種マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車種マスタ";     // 画面名
        private static readonly string PROC_NAME = "車種マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 車種検索画面表示
        /// </summary>
        /// <returns>車種検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車種検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車種検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Car> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 車種検索画面の表示
            return View("CarCriteria", list);
        }

        /// <summary>
        /// 車種検索ダイアログ表示
        /// </summary>
        /// <returns>車種検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 車種検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車種検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CarCode"] = Request["CarCode"];
            form["CarName"] = Request["CarName"];
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<Car> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];

            // 車種検索画面の表示
            return View("CarCriteriaDialog", list);
        }

        /// <summary>
        /// 車種マスタ入力画面表示
        /// </summary>
        /// <param name="id">車種コード(更新時のみ設定)</param>
        /// <returns>車種マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Car car;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                car = new Car();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                car = new CarDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(car);

            // 出口
            return View("CarEntry", car);
        }

        /// <summary>
        /// 車種マスタ追加更新
        /// </summary>
        /// <param name="car">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車種マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Car car, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCar(car);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(car);
                return View("CarEntry", car);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Car targetCar = new CarDao(db).GetByKey(car.CarCode, true);
                UpdateModel(targetCar);
                EditCarForUpdate(targetCar);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                car = EditCarForInsert(car);
                // データ追加
                db.Car.InsertOnSubmit(car);
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

                        ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0010", new string[] { "車種コード", "保存" }));
                        GetEntryViewData(car);
                        return View("CarEntry", car);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii エラーログ対応 『theow e』からエラーページ遷移に変更
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/04 arc amii エラーログ対応 ChangeConflictException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(car.CarCode);

        }

        /// <summary>
        /// 車種コードから車種名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">車種コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Car car = new CarDao(db).GetByKey(code);
                if (car != null)
                {
                    codeData.Code = car.CarCode;
                    codeData.Name = car.CarName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// ブランドコードから車種リストを取得する（ajax）
        /// </summary>
        /// <param name="code">ブランドコード</param>
        public void GetMasterList(string code) {
            if (Request.IsAjaxRequest()) {
                Brand brand = new BrandDao(db).GetByKey(code);
                CodeDataList codeDataList = new CodeDataList();
                if (brand != null) {
                    codeDataList.Code = brand.CarBrandCode;
                    codeDataList.Name = brand.CarBrandName;
                    if (brand.Car != null) {
                        codeDataList.DataList = new List<CodeData>();
                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        foreach (var a in brand.Car.Where(x => CommonUtils.DefaultString(x.DelFlag).Equals("0")).OrderBy(x => x.DisplayOrder))
                        {
                            codeDataList.DataList.Add(new CodeData() { Code = a.CarCode , Name = a.CarName });
                        }
                    }
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="car">モデルデータ</param>
        private void GetEntryViewData(Car car)
        {
            // ブランド名の取得
            if (!string.IsNullOrEmpty(car.CarBrandCode))
            {
                BrandDao brandDao = new BrandDao(db);
                Brand brand = brandDao.GetByKey(car.CarBrandCode);
                if (brand != null)
                {
                    ViewData["CarBrandName"] = brand.CarBrandName;
                }
            }
        }

        /// <summary>
        /// 車種マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車種マスタ検索結果リスト</returns>
        private PaginatedList<Car> GetSearchResultList(FormCollection form)
        {
            CarDao carDao = new CarDao(db);
            Car carCondition = new Car();
            carCondition.CarCode = form["CarCode"];
            carCondition.CarName = form["CarName"];
            carCondition.Brand = new Brand();
            carCondition.Brand.CarBrandCode = form["CarBrandCode"];
            carCondition.Brand.CarBrandName = form["CarBrandName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carCondition.DelFlag = form["DelFlag"];
            }
            return carDao.GetListByCondition(carCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="car">車種データ</param>
        /// <returns>車種データ</returns>
        private Car ValidateCar(Car car)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(car.CarCode))
            {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0001", "車種コード"));
            }
            if (string.IsNullOrEmpty(car.CarName))
            {
                ModelState.AddModelError("CarName", MessageUtils.GetMessage("E0001", "車種名"));
            }
            if (string.IsNullOrEmpty(car.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "ブランド"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CarCode") && !CommonUtils.IsAlphaNumeric(car.CarCode))
            {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0012", "車種コード"));
            }

            return car;
        }

        /// <summary>
        /// 車種マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="car">車種データ(登録内容)</param>
        /// <returns>車種マスタモデルクラス</returns>
        private Car EditCarForInsert(Car car)
        {
            car.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.CreateDate = DateTime.Now;
            car.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.LastUpdateDate = DateTime.Now;
            car.DelFlag = "0";
            return car;
        }

        /// <summary>
        /// 車種マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="car">車種データ(登録内容)</param>
        /// <returns>車種マスタモデルクラス</returns>
        private Car EditCarForUpdate(Car car)
        {
            car.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.LastUpdateDate = DateTime.Now;
            return car;
        }

    }
}
