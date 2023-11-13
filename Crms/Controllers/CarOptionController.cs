using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// オプションマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarOptionController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "オプションマスタ";     // 画面名
        private static readonly string PROC_NAME = "オプションマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarOptionController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// オプション検索画面表示
        /// </summary>
        /// <returns>オプション検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// オプション検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>オプション検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["RequiredFlag"] = (form["RequiredFlag"] == null ? "1" : form["RequiredFlag"]);
            form["ActionFlag"] = "0";
            // 検索結果リストの取得
            PaginatedList<GetCarOptionMaster_Result> list = GetSearchResultList(form);

            // その他出力項目の設定
            //Add 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarOptionCode"] = form["CarOptionCode"];
            ViewData["CarOptionName"] = form["CarOptionName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["RequiredFlag"] = form["RequiredFlag"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["CarGradeCode"]))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(form["CarGradeCode"]);
                if (CarGradedata != null)
                {
                    ViewData["CarGradeCodeName"] = CarGradedata.CarGradeName;
                }
                else
                {
                    ViewData["CarGradeCodeName"] = "";
                }
            }

            // オプション検索画面の表示
            return View("CarOptionCriteria", list);
        }

        /// <summary>
        /// オプション検索ダイアログ表示
        /// </summary>
        /// <returns>オプション検索ダイアログ</returns>
        public ActionResult CriteriaDialog(string CarGradeCode = "")
        {
            return CriteriaDialog(new FormCollection(), CarGradeCode);
        }

        /// <summary>
        /// オプション検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>オプション検索画面ダイアログ</returns>
        //Mod 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form, string CarGradeCode = "")
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)

            //グレードコードが渡されたら該当のメーカー名とメーカーコードを入れる
            if (!string.IsNullOrEmpty(CarGradeCode))
            {
                CarGrade carGrade = new CarGradeDao(db).GetByKey(CarGradeCode);
                form["MakerCode"] = carGrade.Car.Brand.Maker.MakerCode;
                form["MakerName"] = carGrade.Car.Brand.Maker.MakerName;
                form["CarGradeCode"] = CarGradeCode;

            }
            else
            {
                form["MakerCode"] = Request["MakerCode"];
                form["MakerName"] = Request["MakerName"];
                form["CarGradeCode"] = Request["CarGradeCode"];
            }
            form["CarOptionCode"] = Request["CarOptionCode"];
            form["CarOptionName"] = Request["CarOptionName"];
            form["RequiredFlag"] = Request["RequiredFlag"];
            form["RequiredFlag"] = (form["RequiredFlag"] == null ? "0" : form["RequiredFlag"]);
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["ActionFlag"] = "1";
            // 検索結果リストの取得
            PaginatedList<GetCarOptionMaster_Result> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarOptionCode"] = form["CarOptionCode"];
            ViewData["CarOptionName"] = form["CarOptionName"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["RequiredFlag"] = form["RequiredFlag"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["CarGradeCode"]))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(form["CarGradeCode"]);
                if (CarGradedata != null)
                {
                    ViewData["CarGradeName"] = CarGradedata.CarGradeName;
                }
                else
                {
                    ViewData["CarGradeName"] = "";
                }
            }

            // オプション検索画面の表示
            return View("CarOptionCriteriaDialog", list);
        }

        /// <summary>
        /// オプションマスタ入力画面表示
        /// </summary>
        /// <param name="id">オプションコード(更新時のみ設定)</param>
        /// <returns>オプションマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarOption carOption;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carOption = new CarOption();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                carOption = new CarOptionDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(carOption);

            // 出口
            return View("CarOptionEntry", carOption);
        }

        /// <summary>
        /// オプションマスタ追加更新
        /// </summary>
        /// <param name="carOption">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>オプションマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarOption carOption, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCarOption(carOption);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(carOption);
                return View("CarOptionEntry", carOption);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                CarOption targetCarOption = new CarOptionDao(db).GetByKey(carOption.CarOptionCode);
                //車種コードが未入力の場合は全車種共通オプションとし、任意オプションとする 
                UpdateModel(targetCarOption);
                if (string.IsNullOrEmpty(carOption.CarGradeCode))
                {
                    targetCarOption.CarGradeCode = "";
                    targetCarOption.RequiredFlag = "0";
                }
                EditCarOptionForUpdate(targetCarOption);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                carOption = EditCarOptionForInsert(carOption);

                // データ追加
                db.CarOption.InsertOnSubmit(carOption);
            
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
                    // 更新時、クライアントの読み取り以降にDB値が更新された時、ローカルの値をDB値で上書きする
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
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0010", new string[] { "オプションコード", "保存" }));
                        GetEntryViewData(carOption);
                        return View("CarOptionEntry", carOption);
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
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carOption.CarOptionCode);
        }

        /// <summary>
        /// オプションコードからオプション名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">オプションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarOption carOption = new CarOptionDao(db).GetByKey(code);
                if (carOption != null)
                {
                    codeData.Code = carOption.CarOptionCode;
                    codeData.Name = carOption.CarOptionName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// オプションコードからオプション詳細情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">オプションコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retOption = new Dictionary<string, string>();
                CarOption carOption = new CarOptionDao(db).GetByKey(code);
                if (carOption != null)
                {
                    retOption.Add("CarOptionCode", carOption.CarOptionCode);
                    retOption.Add("CarOptionName", carOption.CarOptionName);
                    retOption.Add("SalesPrice", carOption.SalesPrice.ToString());
                }
                return Json(retOption);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carOption">モデルデータ</param>
        private void GetEntryViewData(CarOption carOption)
        {
            // メーカー名の取得
            if (!string.IsNullOrEmpty(carOption.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(carOption.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }
            //Add 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
            // 車種名の取得
            if (!string.IsNullOrEmpty(carOption.CarGradeCode))
            {
                CarGrade CarGradedata = new CarGradeDao(db).GetByKey(carOption.CarGradeCode);
                if (CarGradedata != null)
                {
                    carOption.CarGradeName = CarGradedata.CarGradeName;
                }
                else
                {
                    carOption.CarGradeName = "";
                }
            }

            CodeDao dao = new CodeDao(db);
            // 区分の取得
            ViewData["OptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOptionTypeAll(false), carOption.OptionType, false);


        }

        /// <summary>
        /// オプションマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>オプションマスタ検索結果リスト</returns>
        //Mod 2016/02/22 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
        private PaginatedList<GetCarOptionMaster_Result> GetSearchResultList(FormCollection form)
        {
            CarOptionDao carOptionDao = new CarOptionDao(db);
            CarOption carOptionCondition = new CarOption();
            carOptionCondition.CarOptionCode = form["CarOptionCode"];
            carOptionCondition.CarOptionName = form["CarOptionName"];
            carOptionCondition.Maker = new Maker();
            carOptionCondition.Maker.MakerCode = form["MakerCode"];
            carOptionCondition.Maker.MakerName = form["MakerName"];
            carOptionCondition.CarGradeCode = form["CarGradeCode"];
            if (!string.IsNullOrEmpty(form["ActionFlag"]))
            {
                carOptionCondition.ActionFlag = form["ActionFlag"];
            }
            else
            {
                carOptionCondition.ActionFlag = "0";
            }
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carOptionCondition.DelFlag = form["DelFlag"];
            }
            if (form["RequiredFlag"].Equals("0") || form["RequiredFlag"].Equals("1"))
            {
                carOptionCondition.RequiredFlag = form["RequiredFlag"];
            }
            return carOptionDao.GetListByCondition(carOptionCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carOption">オプションデータ</param>
        /// <returns>オプションデータ</returns>
        private CarOption ValidateCarOption(CarOption carOption)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(carOption.CarOptionCode))
            {
                ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0001", "オプションコード"));
            }
            if (string.IsNullOrEmpty(carOption.CarOptionName))
            {
                ModelState.AddModelError("CarOptionName", MessageUtils.GetMessage("E0001", "オプション名"));
            }
            if (string.IsNullOrEmpty(carOption.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "メーカー"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "原価", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CarOptionCode") && !CommonUtils.IsAlphaNumeric(carOption.CarOptionCode))
            {
                ModelState.AddModelError("CarOptionCode", MessageUtils.GetMessage("E0012", "オプションコード"));
            }
            if (ModelState.IsValidField("Cost") && carOption.Cost != null)
            {
                if (!Regex.IsMatch(carOption.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { "原価", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesPrice") && carOption.SalesPrice != null)
            {
                if (!Regex.IsMatch(carOption.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の整数のみ" }));
                }
            }

            return carOption;
        }

        /// <summary>
        /// オプションマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carOption">オプションデータ(登録内容)</param>
        /// <returns>オプションマスタモデルクラス</returns>
        private CarOption EditCarOptionForInsert(CarOption carOption)
        {
            carOption.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.CreateDate = DateTime.Now;
            carOption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.LastUpdateDate = DateTime.Now;
            carOption.DelFlag = "0";
            //車種コードが未入力の場合は全車種共通オプションとし、任意オプションとする
            if (string.IsNullOrEmpty(carOption.CarGradeCode))
            {
                carOption.CarGradeCode = "";
                carOption.RequiredFlag = "0";
            }
            return carOption;
        }

        /// <summary>
        /// オプションマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carOption">オプションデータ(登録内容)</param>
        /// <returns>オプションマスタモデルクラス</returns>
        private CarOption EditCarOptionForUpdate(CarOption carOption)
        {
            carOption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carOption.LastUpdateDate = DateTime.Now;
            return carOption;
        }

        //Add 2016/02/15 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
        /// <summary>
        /// グレードコードから必須のオプション情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="GradeCode">グレードコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetRequiredOptionByCarGradeCode(string GradeCode)
        {
            if (Request.IsAjaxRequest())
            {
                CarGrade carGrade = new CarGradeDao(db).GetByKey(GradeCode);

                if (carGrade != null)
                {
                    //該当車種の必須オプションを取得する
                    List<GetCarOptionSetListResult> OptionSet = new CarOptionDao(db).GetRequiredOptionByCarCode(GradeCode, carGrade.Car.Brand.Maker.MakerCode);
                    return Json(OptionSet);
                }
                else
                {
                    return new EmptyResult();
                }
            }
            return new EmptyResult();
        }

        //Add 2016/02/15 arc nakayama #3415_車両伝票作成時のオプションのデフォルト設定
        /// <summary>
        /// 車台番号から必須のオプション情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="VinCode">車台番号</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetRequiredOptionByVin(string VinCode)
        {
            if (Request.IsAjaxRequest())
            {
                // 車台番号をキーにレコードを取得
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(VinCode);

                string GradeCode = "";

                // データがある場合、グレードコードを設定する
                if (salesCarList != null && salesCarList.Count > 0)
                {
                    GradeCode = salesCarList[0].CarGradeCode;      // グレードコード
                }
                else
                {
                    return new EmptyResult();
                }

                //新車の場合のみオプションを選択する
                if (salesCarList[0].NewUsedType == "N")
                {
                    //取得したグレードコードから車種コードとメーカーコードを取得
                    CarGrade carGrade = new CarGradeDao(db).GetByKey(GradeCode);

                    if (carGrade != null)
                    {
                        //該当車種の必須オプションを取得する
                        List<GetCarOptionSetListResult> OptionSet = new CarOptionDao(db).GetRequiredOptionByCarCode(GradeCode, carGrade.Car.Brand.Maker.MakerCode);
                        return Json(OptionSet);
                    }
                    else
                    {
                        return new EmptyResult();
                    }
                }
                else
                {
                    return new EmptyResult();
                }
            }
            return new EmptyResult();
        }
    }
}
