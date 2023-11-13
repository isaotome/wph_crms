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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// グレードマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarGradeController : Controller {

        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "グレードマスタ";     // 画面名
        private static readonly string PROC_NAME = "グレードマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarGradeController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// グレード検索画面表示
        /// </summary>
        /// <returns>グレード検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// グレード検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>グレード検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<CarGrade> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // グレード検索画面の表示
            return View("CarGradeCriteria", list);
        }

        /// <summary>
        /// グレード検索ダイアログ表示
        /// </summary>
        /// <returns>グレード検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //デフォルトでログイン担当者の会社コードをセット
            Employee employee = (Employee)Session["Employee"];
            string companyCode = "";
            try { companyCode = employee.Department1.Office.CompanyCode; } catch (NullReferenceException) { }
            form["CompanyCode"] = companyCode;

            return CriteriaDialog(form);
        }

        /// <summary>
        /// グレード検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>グレード検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            //form["CarBrandCode"] = Request["CarBrandCode"];
            //form["CarBrandName"] = Request["CarBrandName"];
            //form["CarClassCode"] = Request["CarClassCode"];
            //form["CarClassName"] = Request["CarClassName"];
            //form["CarCode"] = Request["CarCode"];
            //form["CarName"] = Request["CarName"];
            //form["CarGradeCode"] = Request["CarGradeCode"];
            //form["CarGradeName"] = Request["CarGradeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            //form["ModelName"] = Request["ModelName"];

            // 検索結果リストの取得
            PaginatedList<CarGrade> list;
            if (criteriaInit) {
                list = new PaginatedList<CarGrade>();
            } else {
                list = GetSearchResultListForDialog(form);
            }

            // その他出力項目の設定
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            //ViewData["CarBrandName"] = form["CarBrandName"];
            //ViewData["CarClassCode"] = form["CarClassCode"];
            //ViewData["CarClassName"] = form["CarClassName"];
            ViewData["CarCode"] = form["CarCode"];
            //ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            //ViewData["CarGradeName"] = form["CarGradeName"];
            //ViewData["CompanyCode"] = form["CompanyCode"];
            //Company company = new CompanyDao(db).GetByKey(form["CompanyCode"]);
            //ViewData["CompanyName"] = company != null ? company.CompanyName : "";
            ViewData["ModelSpecificateNumber"] = form["ModelSpecificateNumber"];
            ViewData["ClassificationTypeNumber"] = form["ClassificationTypeNumber"];

            List<CodeData> data = new List<CodeData>();
            List<Brand> brandList = new BrandDao(db).GetListAll();
            foreach (var item in brandList) {
                data.Add(new CodeData { Code = item.CarBrandCode, Name = item.CarBrandName });
            }
            ViewData["CarBrandList"] = CodeUtils.GetSelectListByModel(data, form["CarBrandCode"], false);

            List<CodeData> carData = new List<CodeData>();
            if (form["CarBrandCode"] != null) {
                Brand brand = new BrandDao(db).GetByKey(form["CarBrandCode"]);
                if (brand != null && brand.Car != null && brand.Car.Count() > 0) {
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    List<Car> carList = brand.Car.Where(x => CommonUtils.DefaultString(x.DelFlag).Equals("0")).OrderBy(x => x.DisplayOrder).ToList();
                    foreach (var car in carList) {
                        carData.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                    }
                }
            }
            ViewData["CarList"] = CodeUtils.GetSelectListByModel(carData, form["CarCode"], false);

            List<CodeData> modelData = new List<CodeData>();
            if(form["CarCode"]!=null){
                List<string> modelList = new CarGradeDao(db).GetModelNameList(form["CarCode"]);
                foreach(string item in modelList){
                    modelData.Add(new CodeData{Code=item,Name=item});
                }
            }
            ViewData["ModelNameList"] = CodeUtils.GetSelectListByModel(modelData, form["ModelName"], false);

            // グレード検索画面の表示
            return View("CarGradeCriteriaDialog", list);
        }

        /// <summary>
        /// グレードマスタ入力画面表示
        /// </summary>
        /// <param name="id">グレードコード(更新時のみ設定)</param>
        /// <returns>グレードマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            CarGrade carGrade;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                carGrade = new CarGrade();
            }
            
            // 更新の場合
            else {
                //MOD 2014/10/30 ishii 保存ボタン対応 carGrade再取得のため
                db = CrmsDataContext.GetDataContext();
                ViewData["update"] = "1";
                carGrade = new CarGradeDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(carGrade);
            ViewData["ColorDisplay"] = false;
            ViewData["BasicDisplay"] = true;

            // 出口
            return View("CarGradeEntry", carGrade);
        }

        /// <summary>
        /// コピー機能
        /// </summary>
        /// <param name="code">コピー元グレードコード</param>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Copy(string code) {
            CarGrade carGrade = new CarGrade();
            if (!string.IsNullOrEmpty(code)) {
                CarGrade grade = new CarGradeDao(db).GetByKey(code);
                carGrade.ModelCode = grade.ModelCode;
                carGrade.CarCode = grade.CarCode;
                carGrade.CarClassCode = grade.CarClassCode;
                carGrade.ModelYear = grade.ModelYear;
                carGrade.Door = grade.Door;
                carGrade.TransMission = grade.TransMission;
                carGrade.Capacity = grade.Capacity;
                carGrade.SalesPrice = grade.SalesPrice;
                carGrade.SalesStartDate = grade.SalesStartDate;
                carGrade.SalesEndDate = grade.SalesEndDate;
                carGrade.MaximumLoadingWeight = grade.MaximumLoadingWeight;
                carGrade.CarWeight = grade.CarWeight;
                carGrade.TotalCarWeight = grade.TotalCarWeight;
                carGrade.DrivingName = grade.DrivingName;
                carGrade.ClassificationTypeNumber = grade.ClassificationTypeNumber;
                carGrade.ModelSpecificateNumber = grade.ModelSpecificateNumber;
                carGrade.Length = grade.Length;
                carGrade.Width = grade.Width;
                carGrade.Height = grade.Height;
                carGrade.FFAxileWeight = grade.FFAxileWeight;
                carGrade.FRAxileWeight = grade.FRAxileWeight;
                carGrade.RFAxileWeight = grade.RFAxileWeight;
                carGrade.RRAxileWeight = grade.RRAxileWeight;
                carGrade.ModelName = grade.ModelName;
                carGrade.EngineType = grade.EngineType;
                carGrade.Displacement = grade.Displacement;
                carGrade.Fuel = grade.Fuel;
                carGrade.VehicleType = grade.VehicleType;
                carGrade.InspectionRegistCost = grade.InspectionRegistCost;
                carGrade.RecycleDeposit = grade.RecycleDeposit;
                carGrade.Under24 = grade.Under24;
                carGrade.Under26 = grade.Under26;
                carGrade.Under28 = grade.Under28;
                carGrade.Under30 = grade.Under30;
                carGrade.Under36 = grade.Under36;
                carGrade.Under72 = grade.Under72;
                carGrade.Under84 = grade.Under84;
                carGrade.Over84 = grade.Over84;
                carGrade.CarClassification = grade.CarClassification;
                carGrade.Usage = grade.Usage;
                carGrade.UsageType = grade.UsageType;
                carGrade.Figure = grade.Figure;

                EntitySet<CarAvailableColor> carAvailableColorList = new EntitySet<CarAvailableColor>();
                foreach (var item in grade.CarAvailableColor) {
                    carAvailableColorList.Add(item);
                }
                carGrade.CarAvailableColor = carAvailableColorList;

            }
            GetEntryViewData(carGrade);
            ViewData["update"] = "0";
            ViewData["ColorDisplay"] = false;
            ViewData["BasicDisplay"] = true;
            return View("CarGradeEntry", carGrade);
        }
        /// <summary>
        /// グレードマスタ追加更新
        /// </summary>
        /// <param name="carGrade">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>グレードマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarGrade carGrade, EntitySet<CarAvailableColor> availableColor, FormCollection form) {

            //carGrade.CarAvailableColor = availableColor;

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCarGrade(carGrade);
            if (!ModelState.IsValid) {
                carGrade.CarAvailableColor = availableColor;
                GetEntryViewData(carGrade);
                ViewData["ColorDisplay"] = false;
                ViewData["BasicDisplay"] = true;
                return View("CarGradeEntry", carGrade);
            }

            // Add 2014/08/01 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1")) {
                // データ編集・更新
                CarGrade targetCarGrade = new CarGradeDao(db).GetByKey(carGrade.CarGradeCode);
                if (targetCarGrade != null) {
                    foreach (var original in targetCarGrade.CarAvailableColor) {
                        IEnumerable<CarAvailableColor> query = null;
                        if (availableColor != null) {
                            query =
                                from a in availableColor
                                where a.CarColorCode.Equals(original.CarColorCode)
                                select a;
                        }
                        if (availableColor == null || query == null || query.Count() == 0) {
                            db.CarAvailableColor.DeleteOnSubmit(original);
                        }
                    }
                }
                if (availableColor != null) {
                    // ADD arc uchida vs2012対応
                    foreach (var item1 in availableColor) {
                        int flag = 0;
                        foreach (var item2 in availableColor) {
                            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                            if (CommonUtils.DefaultString(item1.CarColorCode).Equals(CommonUtils.DefaultString(item2.CarColorCode)))
                            {
                                flag += 1; }
                        }
                        if (flag >= 2) {
                            ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0010", new string[] { item1.CarColorCode, "変更" }));
                            break;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        foreach (var item in availableColor)
                        {
                            if (!string.IsNullOrEmpty(item.CarColorCode))
                            {
                                //ないものは追加する
                                CarAvailableColor target = new CarAvailableColorDao(db).GetByKey(carGrade.CarGradeCode, item.CarColorCode);
                                if (target == null)
                                {
                                    item.CarGradeCode = carGrade.CarGradeCode;
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CarAvailableColor.InsertOnSubmit(item);
                                }
                            }
                        }
                    }
                    else {
                        carGrade.CarAvailableColor = availableColor;
                        GetEntryViewData(carGrade);
                        ViewData["ColorDisplay"] = false;
                        ViewData["BasicDisplay"] = true;
                        return View("CarGradeEntry", carGrade);
                    }
                }

                UpdateModel(targetCarGrade);
                EditCarGradeForUpdate(targetCarGrade);
            }

            // データ追加処理
            else {
                // データ編集
                carGrade = EditCarGradeForInsert(carGrade);

                if (availableColor != null) {
                    foreach (var item in availableColor) {
                        if (!string.IsNullOrEmpty(item.CarColorCode)) {
                            //ないものは追加する
                            CarAvailableColor target = new CarAvailableColorDao(db).GetByKey(carGrade.CarGradeCode, item.CarColorCode);
                            if (target == null) {
                                item.CarGradeCode = carGrade.CarGradeCode;
                                item.CreateDate = DateTime.Now;
                                item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                item.LastUpdateDate = DateTime.Now;
                                item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                item.DelFlag = "0";
                                db.CarAvailableColor.InsertOnSubmit(item);
                            }
                        }
                    }
                }
                // データ追加
                db.CarGrade.InsertOnSubmit(carGrade);
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

                        ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0010", new string[] { "グレードコード", "保存" }));
                        GetEntryViewData(carGrade);
                        return View("CarGradeEntry", carGrade);
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
            //MOD 2014/10/29 ishii 保存ボタン対応
            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //ViewData["ColorDisplay"] = false;
            //ViewData["BasicDisplay"] = true;
            //return Entry((string)null);
            return Entry(carGrade.CarGradeCode);

        }

        /// <summary>
        /// 行追加・行削除
        /// </summary>
        /// <param name="carGrade"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ActionResult EditLine(CarGrade carGrade, EntitySet<CarAvailableColor> availableColor, FormCollection form) {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            if (form["DelLine"].Equals("-1")) {
                if (availableColor == null) {
                    availableColor = new EntitySet<CarAvailableColor>();
                }
                availableColor.Add(new CarAvailableColor { CarGradeCode = carGrade.CarGradeCode, DelFlag = "0" });
            } else if (availableColor != null && availableColor.Count() > 0 && form["DelLine"] != null) {
                availableColor.RemoveAt(int.Parse(form["DelLine"]));
            }
            for (int i = 0; i < availableColor.Count(); i++) {
                availableColor[i].CarColor = new CarColorDao(db).GetByKey(availableColor[i].CarColorCode);
            }
            carGrade.CarAvailableColor = availableColor;
            GetEntryViewData(carGrade);
            ViewData["ColorDisplay"] = true;
            ViewData["BasicDisplay"] = false;
            ModelState.Clear();
            return View("CarGradeEntry", carGrade);
        }

        private void UpdateCarAvailableColor(CarGrade carGrade) {

        }
        /// <summary>
        /// グレードコードからグレード名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">グレードコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CarGrade carGrade = new CarGradeDao(db).GetByKey(code);
                if (carGrade != null && carGrade.DelFlag.Equals("0")) {
                    codeData.Code = carGrade.CarGradeCode;
                    codeData.Name = carGrade.CarGradeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// グレードコードから自動車税環境性能割を取得する(Ajax専用)
        /// </summary>
        /// <param name="code">グレードコード</param>
        /// <param name="optionAmount">メーカーオプション合計</param>
        /// <param name="taxid">税率id(空文字…再計算しない、</param>
        /// <param name="requestregistdate">登録希望日</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業　戻り値の型の変更
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", DateTime? requestregistdate = null) //2019/10/17 yano #4022
        //public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", decimal? salesPrice = null)
        {
       
            decimal amount = 0m;

            CarGrade carGrade = new CarGrade();
            
            if (Request.IsAjaxRequest()) {

                //Mod 2019/10/17 yano #4022
                if (!string.IsNullOrEmpty(code))
                {
                    carGrade = new CarGradeDao(db).GetByKey(code);

                    amount = (carGrade.SalesPrice ?? 0m);
                }

                ////Mod 2019/09/04 yano #4011
                //if (salesPrice != null)
                //{
                //    amount = (salesPrice ?? 0m);
                //}
                //else
                //{
                //    if (!string.IsNullOrEmpty(code))
                //    {
                //        carGrade = new CarGradeDao(db).GetByKey(code);

                //        amount = (carGrade.SalesPrice ?? 0m);
                //    }
                //}

                Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, carGrade.VehicleType, "N", "", taxid );    //Mod 2019/09/04 yano #4011
                //decimal acquisitionTax = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0, optionAmount, carGrade.VehicleType, "N", "");

                return Json(acquisitionTax);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// グレードコードから車両情報を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">グレードコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 戻り値変更による修正
        /// </history>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                CarGrade carGrade = new CarGradeDao(db).GetByKey(code);
                if (carGrade != null && carGrade.DelFlag.Equals("0")) {
                    ret.Add("CarGradeCode", carGrade.CarGradeCode);
                    ret.Add("CarGradeName", carGrade.CarGradeName);
                    ret.Add("ModelCode", carGrade.ModelCode);
                    ret.Add("Capacity", CommonUtils.DefaultString(carGrade.Capacity));
                    ret.Add("MaximumLoadingWeight", CommonUtils.DefaultString(carGrade.MaximumLoadingWeight));
                    ret.Add("CarWeight", CommonUtils.DefaultString(carGrade.CarWeight));
                    ret.Add("TotalCarWeight", CommonUtils.DefaultString(carGrade.TotalCarWeight));
                    ret.Add("Length", CommonUtils.DefaultString(carGrade.Length));
                    ret.Add("Width", CommonUtils.DefaultString(carGrade.Width));
                    ret.Add("Height", CommonUtils.DefaultString(carGrade.Height));
                    ret.Add("FFAxileWeight", CommonUtils.DefaultString(carGrade.FFAxileWeight));
                    ret.Add("FRAxileWeight", CommonUtils.DefaultString(carGrade.FRAxileWeight));
                    ret.Add("RFAxileWeight", CommonUtils.DefaultString(carGrade.RFAxileWeight));
                    ret.Add("RRAxileWeight", CommonUtils.DefaultString(carGrade.RRAxileWeight));
                    ret.Add("ModelName", carGrade.ModelName);
                    ret.Add("EngineType", carGrade.EngineType);
                    ret.Add("Displacement", CommonUtils.DefaultString(carGrade.Displacement));
                    ret.Add("Fuel", carGrade.Fuel);
                    ret.Add("ModelSpecificateNumber", carGrade.ModelSpecificateNumber);
                    ret.Add("ClassificationTypeNumber", carGrade.ClassificationTypeNumber);
                    ret.Add("Door", carGrade.Door);
                    ret.Add("TransMission", carGrade.TransMission);
                    try { ret.Add("SalesPrice", carGrade.SalesPrice.ToString()); } catch (NullReferenceException) { }
                    //MOD 2014/02/20 ookubo ここのrateは意味ない（呼び元で再計算）
                    string id = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    int rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(id));
                    try { ret.Add("SalesTax", Math.Truncate((carGrade.SalesPrice ?? 0m) * (rate / 100)).ToString()); }
                    //try { ret.Add("SalesTax", Math.Truncate((carGrade.SalesPrice ?? 0m) * 0.05m).ToString()); }
                    catch (NullReferenceException) { }
                    //MOD 2014/02/20 ookubo ここのrateは意味ない（呼び元で再計算）
                    try { ret.Add("SalesPriceWithTax", (carGrade.SalesPrice + Math.Truncate((carGrade.SalesPrice ?? 0m) * rate)).ToString()); }
                    //try { ret.Add("SalesPriceWithTax", (carGrade.SalesPrice + Math.Truncate((carGrade.SalesPrice ?? 0m) * 0.05m)).ToString()); }
                    catch (NullReferenceException) { }
                    try { ret.Add("CarBrandCode", carGrade.Car.CarBrandCode); } catch (NullReferenceException) { }
                    try { ret.Add("CarName", carGrade.Car.CarName); } catch (NullReferenceException) { }
                    try { ret.Add("CarBrandName", carGrade.Car.Brand.CarBrandName); } catch (NullReferenceException) { }
                    try { ret.Add("MakerName", carGrade.Car.Brand.Maker.MakerName); } catch (NullReferenceException) { }
                    ret.Add("CarGradeFullName", ret["CarBrandName"] + " " + ret["CarName"] + " " + ret["CarGradeName"]);
                    try { ret.Add("LaborRate", carGrade.Car.Brand.LaborRate.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("InspectionRegistCost", carGrade.InspectionRegistCost.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("TradeInMaintenanceFee", ""); } catch (NullReferenceException) { }
                    try { ret.Add("InheritedInsuranceFee", carGrade.Under24.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("RecycleDeposit", carGrade.RecycleDeposit.ToString()); } catch (NullReferenceException) { }
                    ret.Add("ModelYear", carGrade.ModelYear);

                    //環境性能割
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, "N", "");
                    ret.Add("EPDiscountTaxList", retValue.Item1);
                    ret.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2.ToString()));
                    //ret.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, "N", "")));
                
                    //自賠責保険料(グレードをセットした場合は自動的に新車とみなす)
                    CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                    if (insurance != null) {
                        ret.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance.Amount));
                    }

                    //重量税(グレードをセットした場合は自動的に新車とみなす)
                    CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, carGrade.CarWeight ?? 0);
                    if (weightTax != null) {
                        ret.Add("CarWeightTax", string.Format("{0:0}", weightTax.Amount));
                    }
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 車種コードから型式リストを取得する
        /// </summary>
        /// <param name="carCode"></param>
        public void GetModelNameList(string carCode) {
            if (Request.IsAjaxRequest()) {
                List<string> modelList = new CarGradeDao(db).GetModelNameList(carCode);
                CodeDataList codeDataList = new CodeDataList();
                if (modelList != null) {
                    codeDataList.Code = carCode;
                    codeDataList.DataList = new List<CodeData>();
                    foreach (var item in modelList) {
                        codeDataList.DataList.Add(new CodeData() { Code = item, Name = item });
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
        /// <param name="carGrade">モデルデータ</param>
        private void GetEntryViewData(CarGrade carGrade) {

            // 車種名の取得
            if (!string.IsNullOrEmpty(carGrade.CarCode)) {
                CarDao carDao = new CarDao(db);
                Car car = carDao.GetByKey(carGrade.CarCode);
                if (car != null) {
                    ViewData["CarName"] = car.CarName;
                }
            }

            // 車両クラス名の取得
            if (!string.IsNullOrEmpty(carGrade.CarClassCode)) {
                CarClassDao carClassDao = new CarClassDao(db);
                CarClass carClass = carClassDao.GetByKey(carGrade.CarClassCode);
                if (carClass != null) {
                    ViewData["CarClassName"] = carClass.CarClassName;
                }
            }

            //セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["TransMissionList"] = CodeUtils.GetSelectListByModel(dao.GetTransMissionAll(false), carGrade.TransMission, true);
            ViewData["DrivingNameList"] = CodeUtils.GetSelectListByModel(dao.GetDrivingNameAll(false), carGrade.DrivingName, true);
            ViewData["VehicleTypeList"] = CodeUtils.GetSelectListByModel(dao.GetVehicleTypeAll(false), carGrade.VehicleType, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel(dao.GetFuelTypeAll(false), carGrade.Fuel, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), carGrade.UsageType, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), carGrade.Usage, true);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), carGrade.CarClassification, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), carGrade.Figure, true);

            // 車両カラー名の取得
            foreach (var item in carGrade.CarAvailableColor) {
                item.CarColor = new CarColorDao(db).GetByKey(item.CarColorCode);
            }
        }

        /// <summary>
        /// グレードマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>グレードマスタ検索結果リスト</returns>
        private PaginatedList<CarGrade> GetSearchResultList(FormCollection form) {

            CarGradeDao carGradeDao = new CarGradeDao(db);
            CarGrade carGradeCondition = new CarGrade();
            carGradeCondition.CarGradeCode = form["CarGradeCode"];
            carGradeCondition.CarGradeName = form["CarGradeName"];
            carGradeCondition.Car = new Car();
            carGradeCondition.Car.CarCode = form["CarCode"];
            carGradeCondition.Car.CarName = form["CarName"];
            carGradeCondition.Car.Brand = new Brand();
            carGradeCondition.Car.Brand.CarBrandCode = form["CarBrandCode"];
            carGradeCondition.Car.Brand.CarBrandName = form["CarBrandName"];
            carGradeCondition.Car.Brand.CompanyCode = form["CompanyCode"];
            carGradeCondition.CarClass = new CarClass();
            carGradeCondition.CarClass.CarClassCode = form["CarClassCode"];
            carGradeCondition.CarClass.CarClassName = form["CarClassName"];
            carGradeCondition.ModelName = form["ModelName"];
            carGradeCondition.ModelSpecificateNumber = form["ModelSpecificateNumber"];
            carGradeCondition.ClassificationTypeNumber = form["ClassificationTypeNumber"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carGradeCondition.DelFlag = form["DelFlag"];
            }
            return carGradeDao.GetListByCondition(carGradeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        private PaginatedList<CarGrade> GetSearchResultListForDialog(FormCollection form) {
            CarGradeDao carGradeDao = new CarGradeDao(db);
            CarGrade carGradeCondition = new CarGrade();
            carGradeCondition.Car = new Car();
            carGradeCondition.Car.CarCode = form["CarCode"];
            carGradeCondition.Car.Brand = new Brand();
            carGradeCondition.Car.Brand.CarBrandCode = form["CarBrandCode"];
            carGradeCondition.ModelName = form["ModelName"];
            carGradeCondition.ModelSpecificateNumber = form["ModelSpecificateNumber"];
            carGradeCondition.ClassificationTypeNumber = form["ClassificationTypeNumber"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carGradeCondition.DelFlag = form["DelFlag"];
            }
            return carGradeDao.GetListByConditionForDialog(carGradeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carGrade">グレードデータ</param>
        /// <returns>グレードデータ</returns>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2018/04/25 arc yano #3716 【車】グレードマスタのグレードコードに「－：ハイフン」「＿：アンダーバー」付き登録
        /// </history>
        private CarGrade ValidateCarGrade(CarGrade carGrade) {

            // 必須チェック
            if (string.IsNullOrEmpty(carGrade.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0001", "グレードコード"));
            }
            if (string.IsNullOrEmpty(carGrade.CarGradeName)) {
                ModelState.AddModelError("CarGradeName", MessageUtils.GetMessage("E0001", "グレード名"));
            }
            if (string.IsNullOrEmpty(carGrade.CarCode)) {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0001", "車種"));
            }
            if (string.IsNullOrEmpty(carGrade.CarClassCode)) {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0001", "車両クラス"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Capacity")) {
                ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SalesPrice")) {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "車両本体価格", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SalesStartDate")) {
                ModelState.AddModelError("SalesStartDate", MessageUtils.GetMessage("E0005", "販売開始日"));
            }
            if (!ModelState.IsValidField("SalesEndDate")) {
                ModelState.AddModelError("SalesEndDate", MessageUtils.GetMessage("E0005", "販売終了日"));
            }
            if (!ModelState.IsValidField("MaximumLoadingWeight")) {
                ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("CarWeight")) {
                ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("TotalCarWeight")) {
                ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Length")) {
                ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Width")) {
                ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Height")) {
                ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("FFAxileWeight")) {
                ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("FRAxileWeight")) {
                ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RFAxileWeight")) {
                ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RRAxileWeight")) {
                ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Displacement")) {
                ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数10桁以内かつ小数2桁以内" }));
            }
            if (!ModelState.IsValidField("InspectionRegistCost")) {
                ModelState.AddModelError("InspectionRegistCost", MessageUtils.GetMessage("E0004", new string[] { "検査登録手続", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under24")) {
                ModelState.AddModelError("Under24", MessageUtils.GetMessage("E0004", new string[] { "24ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under26")) {
                ModelState.AddModelError("Under26", MessageUtils.GetMessage("E0004", new string[] { "26ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under28")) {
                ModelState.AddModelError("Under28", MessageUtils.GetMessage("E0004", new string[] { "28ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under30")) {
                ModelState.AddModelError("Under30", MessageUtils.GetMessage("E0004", new string[] { "30ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under36")) {
                ModelState.AddModelError("Under36", MessageUtils.GetMessage("E0004", new string[] { "36ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under72")) {
                ModelState.AddModelError("Under72", MessageUtils.GetMessage("E0004", new string[] { "72ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Under84")) {
                ModelState.AddModelError("Under84", MessageUtils.GetMessage("E0004", new string[] { "84ヶ月未満", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Over84")) {
                ModelState.AddModelError("Over84", MessageUtils.GetMessage("E0004", new string[] { "84ヶ月以上", "正の整数のみ" }));
            }

            //Mod 2018/04/25 arc yano #3716
            // フォーマットチェック
            if (ModelState.IsValidField("CarGradeCode") && !CommonUtils.IsAlphaNumericBarUnderBar(carGrade.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0031", "グレードコード"));
            }
            /*
            if (ModelState.IsValidField("CarGradeCode") && !CommonUtils.IsAlphaNumeric(carGrade.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0012", "グレードコード"));
            }
            */
            if (ModelState.IsValidField("SalesPrice") && carGrade.SalesPrice != null) {
                if (!Regex.IsMatch(carGrade.SalesPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "車両本体価格", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("InspectionRegistCost") && carGrade.InspectionRegistCost != null) {
                if (!Regex.IsMatch(carGrade.InspectionRegistCost.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("InspectionRegistCost", MessageUtils.GetMessage("E0004", new string[] { "検査登録費用", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RecycleDeposit") && carGrade.RecycleDeposit != null) {
                if (!Regex.IsMatch(carGrade.RecycleDeposit.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under24") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under24.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under24", MessageUtils.GetMessage("E0004", new string[] { "24ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under26") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under26.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under26", MessageUtils.GetMessage("E0004", new string[] { "26ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under28") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under28.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under28", MessageUtils.GetMessage("E0004", new string[] { "28ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under30") && carGrade.Under30 != null) {
                if (!Regex.IsMatch(carGrade.Under30.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under30", MessageUtils.GetMessage("E0004", new string[] { "30ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under36") && carGrade.Under36 != null) {
                if (!Regex.IsMatch(carGrade.Under36.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under36", MessageUtils.GetMessage("E0004", new string[] { "36ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under72") && carGrade.Under72 != null) {
                if (!Regex.IsMatch(carGrade.Under72.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under72", MessageUtils.GetMessage("E0004", new string[] { "72ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Under84") && carGrade.Under84 != null) {
                if (!Regex.IsMatch(carGrade.Under84.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under84", MessageUtils.GetMessage("E0004", new string[] { "84ヶ月未満", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Over84") && carGrade.Over84 != null) {
                if (!Regex.IsMatch(carGrade.Over84.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Over84", MessageUtils.GetMessage("E0004", new string[] { "84ヶ月未満", "正の整数のみ" }));
                }
            }

            //Add 2021/08/02 yano #4097 入力可能文字フォーマットを変更(正の整数４桁のみ→正の整数4桁、または正の整数4桁かつ少数2桁以内
            if (ModelState.IsValidField("ModelYear") && CommonUtils.DefaultString(carGrade.ModelYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(carGrade.ModelYear, @"^\d{4}\.\d{1,2}$"))
                                  && (!Regex.IsMatch(carGrade.ModelYear, @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "モデル年", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
                }
            }

            // 値チェック
            if (ModelState.IsValidField("Capacity") && carGrade.Capacity != null) {
                if (carGrade.Capacity < 0) {
                    ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("MaximumLoadingWeight") && carGrade.MaximumLoadingWeight != null) {
                if (carGrade.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("CarWeight") && carGrade.CarWeight != null) {
                if (carGrade.CarWeight < 0) {
                    ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("TotalCarWeight") && carGrade.TotalCarWeight != null) {
                if (carGrade.TotalCarWeight < 0) {
                    ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Length") && carGrade.Length != null) {
                if (carGrade.Length < 0) {
                    ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Width") && carGrade.Width != null) {
                if (carGrade.Width < 0) {
                    ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Height") && carGrade.Height != null) {
                if (carGrade.Height < 0) {
                    ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("FFAxileWeight") && carGrade.FFAxileWeight != null) {
                if (carGrade.FFAxileWeight < 0) {
                    ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("FRAxileWeight") && carGrade.FRAxileWeight != null) {
                if (carGrade.FRAxileWeight < 0) {
                    ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RFAxileWeight") && carGrade.RFAxileWeight != null) {
                if (carGrade.RFAxileWeight < 0) {
                    ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RRAxileWeight") && carGrade.RRAxileWeight != null) {
                if (carGrade.RRAxileWeight < 0) {
                    ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && carGrade.Displacement != null) {
                if ((Regex.IsMatch(carGrade.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carGrade.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数10桁以内かつ少数2桁以内" }));
                }
            }

            return carGrade;
        }

        /// <summary>
        /// グレードマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carGrade">グレードデータ(登録内容)</param>
        /// <returns>グレードマスタモデルクラス</returns>
        private CarGrade EditCarGradeForInsert(CarGrade carGrade) {

            carGrade.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.CreateDate = DateTime.Now;
            carGrade.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.LastUpdateDate = DateTime.Now;
            carGrade.DelFlag = "0";
            return carGrade;
        }

        /// <summary>
        /// グレードマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carGrade">グレードデータ(登録内容)</param>
        /// <returns>グレードマスタモデルクラス</returns>
        private CarGrade EditCarGradeForUpdate(CarGrade carGrade) {

            carGrade.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.LastUpdateDate = DateTime.Now;
            return carGrade;
        }

    }
}
