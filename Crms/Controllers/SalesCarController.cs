using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers {

    /// <summary>
    /// 車両マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SalesCarController : Controller {

        #region 初期化
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両マスタ";     // 画面名
        private static readonly string PROC_NAME = "車両マスタ登録"; // 処理名

        private static readonly string CARPURCHASE_NOTPURCHASED = "001";  //車両仕入ステータス = 未仕入 //Add 2022/01/13 yano #4123
      
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SalesCarController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion

        #region 検索
        /// <summary>
        /// 車両検索画面表示
        /// </summary>
        /// <returns>車両検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 車両検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // ステータス変更処理
            if (form["action"].Equals("change")) {
                string[] targetArr = CommonUtils.DefaultString(form["chkTarget"]).Replace("false", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string carStatus = form["ChangeCarStatus"];
                foreach (string target in targetArr) {
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(target);
                    salesCar.CarStatus = carStatus;
                    for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++) {
                        EditSalesCarForUpdate(salesCar);
                        try {
                            db.SubmitChanges();
                            break;
                        } catch (ChangeConflictException e) {
                            //Add 2014/08/04 arc amii エラーログ対応 ログ出力処理追加
                            foreach (ObjectChangeConflict occ in db.ChangeConflicts) {
                                occ.Resolve(RefreshMode.KeepCurrentValues);
                            }
                            if (i + 1 >= DaoConst.MAX_RETRY_COUNT) {
                                // セッションにSQL文を登録
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ログに出力
                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                        catch (Exception ex)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                }
            }

            // 検索結果リストの取得
            PaginatedList<SalesCar> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorType"], true);
            ViewData["ExteriorColorCode"] = form["ExteriorColorCode"];
            ViewData["ExteriorColorName"] = form["ExteriorColorName"];
            ViewData["InteriorColorCode"] = form["InteriorColorCode"];
            ViewData["InteriorColorName"] = form["InteriorColorName"];
            ViewData["ManufacturingYear"] = form["ManufacturingYear"];
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            ViewData["LocationName"] = form["LocationName"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), form["Steering"], true);
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["ChangeCarStatus"], true);
            ViewData["UserName"] = form["UserName"];
            ViewData["UserNameKana"] = form["UserNameKana"];

            ////Mod 2014/10/16 arc yano 車両ステータス追加対応
            ViewData["CarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("004", false), form["CarUsage"], true);

            // 車両検索画面の表示
            return View("SalesCarCriteria", list);
        }

        /// <summary>
        /// 車両検索ダイアログ表示
        /// </summary>
        /// <returns>車両検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 車両検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["SalesCarNumber"] = Request["SalesCarNumber"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["CarName"] = Request["CarName"];
            form["CarGradeName"] = Request["CarGradeName"];
            form["NewUsedType"] = Request["NewUsedType"];
            form["ColorType"] = Request["ColorType"];
            form["ExteriorColorCode"] = Request["ExteriorColorCode"];
            form["ExteriorColorName"] = Request["ExteriorColorName"];
            form["InteriorColorCode"] = Request["InteriorColorCode"];
            form["InteriorColorName"] = Request["InteriorColorName"];
            form["ManufacturingYear"] = Request["ManufacturingYear"];
            form["CarStatus"] = Request["CarStatus"];
            //Add 2014/10/16 arc yano 車両ステータス追加対応 検索条件に利用用途(CarUsage)を追加 
            form["CarUsage"] = Request["CarUsage"];
            form["LocationName"] = Request["LocationName"];
            form["CustomerName"] = Request["CustomerName"];
            form["Vin"] = Request["Vin"];
            form["MorterViecleOfficialCode"] = Request["MorterViecleOfficialCode"];
            form["RegistrationNumberType"] = Request["RegistrationNumberType"];
            form["RegistrationNumberKana"] = Request["RegistrationNumberKana"];
            form["RegistrationNumberPlate"] = Request["RegistrationNumberPlate"];
            form["Steering"] = Request["Steering"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["UserName"] = Request["UserName"];
            form["UserNameKana"] = Request["UserNameKana"];

            // 検索結果リストの取得
            PaginatedList<SalesCar> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorType"], true);
            ViewData["ExteriorColorCode"] = form["ExteriorColorCode"];
            ViewData["ExteriorColorName"] = form["ExteriorColorName"];
            ViewData["InteriorColorCode"] = form["InteriorColorCode"];
            ViewData["InteriorColorName"] = form["InteriorColorName"];
            ViewData["ManufacturingYear"] = form["ManufacturingYear"];
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            //Add 2014/10/16 arc yano 車両ステータス追加対応 検索条件に利用用途(CarUsage)を追加 
            ViewData["CarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("004", false), form["CarUsage"], true);
            ViewData["LocationName"] = form["LocationName"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), form["Steering"], true);

            // 車両検索画面の表示
            return View("SalesCarCriteriaDialog", list);
        }
        
        /*  //Del 2016/08/13 arc yano 未使用のため、コメントアウト
        /// <summary>
        /// 在庫表
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult List() {
            criteriaInit = true;
            return List(new FormCollection());
        }

        /// <summary>
        /// 在庫表検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult List(FormCollection form) {

            // ログイン者の部門
            Department selfDepartment = ((Employee)Session["Employee"]).Department1;
            if (criteriaInit) {
                form["DepartmentCode"] = selfDepartment.DepartmentCode;
            }
            PaginatedList<SalesCar> list = new PaginatedList<SalesCar>();
            // 自部門が営業店舗、フォームで部門を選択している場合のみ選択
            if (selfDepartment.BusinessType != null && (selfDepartment.BusinessType.Equals("001") || selfDepartment.BusinessType.Equals("009")) && !string.IsNullOrEmpty(form["DepartmentCode"])) {
                SalesCarDao dao = new SalesCarDao(db);
                list = dao.GetStockList(form["DepartmentCode"], form["NewUsedType"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
            // 営業のみ
            List<Department> department = new DepartmentDao(db).GetListAll("001");
            List<Department> honbu = new DepartmentDao(db).GetListAll("009");
            department.AddRange(honbu);
            List<CodeData> dataList = new List<CodeData>();
            foreach (var item in department) {
                dataList.Add(new CodeData { Code = item.DepartmentCode, Name = item.DepartmentName });
            }
            
            ViewData["DepartmentList"] = CodeUtils.GetSelectList(dataList, form["DepartmentCode"], true);
            //ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["NewUsedType"] = form["NewUsedType"];
            return View("SalesCarList", list);
        }
        */
        #endregion

        #region 入力
        /// <summary>
        /// 車両マスタ入力画面表示
        /// </summary>
        /// <param name="id">車両コード(更新時のみ設定)</param>
        /// <returns>車両マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            ViewData["Master"] = Request["Master"];
            SalesCar salesCar;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {
                salesCar = new SalesCar();
                ViewData["update"] = "0";
                ViewData["LocationName"] = "";
            }
                // 更新の場合
            else {
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                salesCar = new SalesCarDao(db).GetByKey(id, true);
                salesCar.OwnershipChangeDate = DateTime.Today;
                ViewData["Closed"] = "";
                //try {
                //    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(salesCar.Location.DepartmentCode, salesCar.SalesDate, "001")) {
                //        ViewData["Closed"] = "1";
                //    }
                //} catch { }
                ViewData["update"] = "1";
                ViewData["LocationName"] = "";
                try { ViewData["LocationName"] = salesCar.Location.LocationName; } catch (NullReferenceException) { }
            }

            // その他表示データの取得
            GetEntryViewData(salesCar);

            // 出口
            return View("SalesCarEntry", salesCar);
        }

        /// <summary>
        /// 車両マスタ追加更新
        /// </summary>
        /// <param name="salesCar">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両マスタ入力画面</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SalesCar salesCar, FormCollection form) {

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["Master"] = string.IsNullOrEmpty(form["Master"]) ? null : form["Master"];

            // データチェック
            ValidateSalesCar(salesCar);
            if (!ModelState.IsValid) {
                GetEntryViewData(salesCar);
                return View("SalesCarEntry", salesCar);
            }

            // 和暦を西暦に変換
            if (!salesCar.IssueDateWareki.IsNull) {
                salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki);
            }
            if (!salesCar.RegistrationDateWareki.IsNull) {
                salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki);
            }
            salesCar.FirstRegistrationDateWareki.Day = 1; 
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {

                DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                //DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki);

                if (firstRegistrationDate.HasValue) {
                    salesCar.FirstRegistrationYear = firstRegistrationDate.Value.Year + "/" + firstRegistrationDate.Value.Month;
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull) {
                salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki);
            }
            if (!salesCar.SalesDateWareki.IsNull) {
                salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki);
            }
            if (!salesCar.InspectionDateWareki.IsNull) {
                salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki);
            }
            if (!salesCar.NextInspectionDateWareki.IsNull) {
                salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki, db); //Mod 2018/06/22 arc yano #3891
                //salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1")) {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                SalesCar targetSalesCar = new SalesCarDao(db).GetByKey(salesCar.SalesCarNumber, true);
                targetSalesCar.OwnershipChangeDate = salesCar.OwnershipChangeDate;
                targetSalesCar.OwnershipChangeMemo = salesCar.OwnershipChangeMemo;
                targetSalesCar.OwnershipChangeType = salesCar.OwnershipChangeType;
                targetSalesCar.IssueDate = salesCar.IssueDate;
                targetSalesCar.RegistrationDate = salesCar.RegistrationDate;
                targetSalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                targetSalesCar.ExpireDate = salesCar.ExpireDate;
                targetSalesCar.SalesDate = salesCar.SalesDate;
                targetSalesCar.InspectionDate = salesCar.InspectionDate;
                targetSalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                // 履歴テーブルにコピー
                CommonUtils.CopyToSalesCarHistory(db, targetSalesCar);

                UpdateModel(targetSalesCar);
                EditSalesCarForUpdate(targetSalesCar);
                
            }
                // データ追加処理
            else {
                ValidateForInsert(salesCar);
                if (!ModelState.IsValid) {
                    GetEntryViewData(salesCar);
                    return View("SalesCarEntry", salesCar);
                }
                // データ編集
                salesCar = EditSalesCarForInsert(salesCar);

                // データ追加
                db.SalesCar.InsertOnSubmit(salesCar);
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
                        //Add 2014/08/01 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                        GetEntryViewData(salesCar);
                        return View("SalesCarEntry", salesCar);
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
            //MOD 2014/11/04 ishii 保存ボタン対応
            ModelState.Clear();
            // 出口
            //ViewData["close"] = "1";
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //return Entry((string)null);
            return Entry(salesCar.SalesCarNumber);
        }

        /// <summary>
        /// 車両マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="salesCar">車両データ(登録内容)</param>
        /// <returns>車両マスタモデルクラス</returns>
        /// <history>
        /// 2020/11/27 yano #4072 原動機型式入力エリアの拡張 最大文字数での切り取り処理の廃止
        /// 2019/5/23 yano #3992 車両マスタ】原動機の型式の最大文字数の変更(10→15)
        /// </history>
        private SalesCar EditSalesCarForInsert(SalesCar salesCar) {

            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            //ADD 2014/10/16 arc ishii VINを全角->半角変換,小文字⇒大文字変換する
            // salesCar.Vin = CommonUtils.abc123ToHankaku(salesCar.Vin);
            salesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.Vin));
            //ADD 2014/10/22 arc ishii Vinが20文字以上の場合左から20文字目まで抜き出す
            if (salesCar.Vin.Length > 20)
            {
                salesCar.Vin = salesCar.Vin.Substring(0, 20);
            }
            //ADD 2014/10/21 arc ishii EngineTypeを全角->半角変換,小文字⇒大文字変換する
            salesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.EngineType));


            //Mod 2020/11/27 yano #4072
            //Mod 2019/5/23 yano #3992 15文字以上の場合に15文字まで切り出す
            //Add 2015/03/26 arc iijima Null落ち対応のため判定追加
            //ADD 2014/10/22 arc ishii EngineTypeが10文字以上の場合左から10文字目まで抜き出す
            //if ((!string.IsNullOrWhiteSpace(salesCar.EngineType)) && (salesCar.EngineType.Length > 15))
            //{
            //    salesCar.EngineType = salesCar.EngineType.Substring(0, 15);
            //}
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            return salesCar;
        }

        /// <summary>
        /// 車両マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="salesCar">車両データ(登録内容)</param>
        /// <returns>車両マスタモデルクラス</returns>
        /// <history>
        ///  2020/11/27 yano #4072 原動機型式入力エリアの拡張 最大文字数での切り取り処理の廃止
        /// 2019/5/23 yano #3992 車両マスタ】原動機の型式の最大文字数の変更(10→15)
        /// </history>
        private SalesCar EditSalesCarForUpdate(SalesCar salesCar) {

            // VINを全角->半角変換,小文字⇒大文字変換する ADD 2014/10/16 arc ishii 
            //salesCar.Vin = CommonUtils.abc123ToHankaku(salesCar.Vin);
            salesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.Vin));
            //Mod 2015/02/16 arc yano Vinがnullまたは空文字の場合は処理を行わない
            //ADD 2014/10/22 arc ishii Vinが20文字以上の場合左から20文字目まで抜き出す
            if ((!string.IsNullOrWhiteSpace(salesCar.Vin)) && (salesCar.Vin.Length > 20) )
            {
                salesCar.Vin = salesCar.Vin.Substring(0, 20);
            }
            //ADD 2014/10/21 arc ishii EngineTypeを全角->半角変換,小文字⇒大文字変換する
            salesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.EngineType));

            //Mod 2020/11/27 yano #4072
            //Mod 2019/5/23 yano #3992 15文字以上の場合に15文字まで切り出す
            //Mod 2015/02/16 arc yano Vinがnullまたは空文字の場合は処理を行わない
            //ADD 2014/10/22 arc ishii EngineTypeが10文字以上の場合左から10文字目まで抜き出す
            //if ((!string.IsNullOrWhiteSpace(salesCar.EngineType)) && (salesCar.EngineType.Length > 15))
            //{
            //    salesCar.EngineType = salesCar.EngineType.Substring(0, 15);
            //}
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            return salesCar;
        }
        #endregion

        #region Ajax
        /// <summary>
        /// 車両コードから車両名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">車両コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null) {
                    codeData.Code = salesCar.SalesCarNumber;
                    codeData.Name = salesCar.Vin;
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
       /// <param name="taxid">環境性能割・税率ID</param>
       /// <param name="requestregistdate">登録希望日</param>
       /// <returns>取得結果(取得できない場合でもnullではない)</returns>
       /// <history>
       /// 2019/10/17 yano #4022 【車両伝票入力】特定の条件下での環境性能割の計算
       /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業　戻り値の型の変更
        /// 2014/05/27 arc yano vs2012対応
       /// </history>
       [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", DateTime? requestregistdate = null)
        //public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", decimal? salesPrice = null)
        {
            //Add 2019/09/04 yano #4011
            decimal amount = 0m;
            SalesCar salesCar = new SalesCar();

            if (Request.IsAjaxRequest())
            {
                salesCar = new SalesCarDao(db).GetByKey(code);

                //Mod 2019/10/17 yano #4022
                //新車、中古車に限らずグレードマスタの販売価格を設定する
                try
                {
                    amount = (salesCar.CarGrade.SalesPrice ?? 0m);
                }
                catch
                {
                }

                ////Mod 2019/09/04 yano #4011
                ////中古車の場合、または販売価格がNULL、空文字の場合はグレードマスタから設定
                //if (salesCar.NewUsedType.Equals("U") || salesPrice == null)
                //{
                //    try
                //    {
                //        amount = (salesCar.CarGrade.SalesPrice ?? 0m);
                //    }
                //    catch
                //    {
                //    }
                //}
                //else
                //{
                //    amount = (salesPrice ?? 0m);
                //}

                //Mod 2019/10/17 yano #4022
                Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear, taxid, requestregistdate);
                //Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear, taxid);

                return Json(acquisitionTax);
            }

            return new EmptyResult();
        }
        /// <summary>
        /// 車両コードから車両を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">車両コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2022/07/06 yano #4145【サービス伝票】車台番号入力した際に顧客情報が表示されない不具合の対応
        /// 2022/01/13 yano #4123 【サービス伝票入力】未仕入の車両が選択できる不具合の対応
        /// 2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
        /// 2019/10/22 yano #4024 【車両伝票入力】オプション行追加・削除時にエラー発生した時の不具合対応
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
       public ActionResult GetMasterDetail(string code, string SelectByCarSlip = "0")
       {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retCar = new Dictionary<string, string>();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null) {
                    
                    retCar.Add("CarGradeCode", salesCar.CarGradeCode);
                    retCar.Add("MakerName", salesCar.CarGrade.Car.Brand.Maker.MakerName);
                    retCar.Add("CarBrandName", salesCar.CarGrade.Car.Brand.CarBrandName);
                    retCar.Add("CarGradeName", salesCar.CarGrade.CarGradeName);
                    retCar.Add("CarName", salesCar.CarGrade.Car.CarName);
                    retCar.Add("ExteriorColorCode", salesCar.ExteriorColorCode);
                    retCar.Add("ExteriorColorName", salesCar.ExteriorColorName==null ? "" : salesCar.ExteriorColorName);
                    retCar.Add("InteriorColorCode", salesCar.InteriorColorCode);
                    retCar.Add("InteriorColorName", salesCar.InteriorColorName==null ? "" : salesCar.InteriorColorName);
                    retCar.Add("Mileage", salesCar.Mileage.ToString());
                    retCar.Add("MileageUnit", salesCar.MileageUnit);
                    retCar.Add("ModelName", salesCar.ModelName);
                    retCar.Add("NewUsedType", salesCar.NewUsedType);
                    retCar.Add("SalesPrice", salesCar.SalesPrice.ToString());
                    //Mod 2015/07/28 arc nakayama #3217_デモカーが販売できてしまう問題の改善 　車両伝票の車台番号のルックアップから呼ばれた時　かつ　在庫ステータスが在庫の時だけ管理番号を返す
                    //Mod 2015/08/20 arc nakayama #3242_サービス伝票で車両マスタボタンを押しても車両マスタが表示されない 車両伝票から呼ばれた時だけ在庫ステータスを見るように修正
                    if (SelectByCarSlip.ToString().Equals("1"))
                    {
                        //if (salesCar.CarStatus.Equals("001"))
                        if (salesCar.CarStatus == null || salesCar.CarStatus.Equals("001"))   //Mod 2019/10/22 yano #4024
                        {
                            retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                        }
                        else
                        {
                            retCar.Add("SalesCarNumber", "");
                        }
                    }
                    else
                    {
                        retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                    }
                    retCar.Add("Vin", salesCar.Vin);
                    retCar.Add("LocationName", salesCar.Location!=null ? salesCar.Location.LocationName : "");
                    retCar.Add("InheritedInsuranceFee", "");
                    //retCar.Add("CarWeightTax", salesCar.CarWeightTax.ToString());
                    retCar.Add("RecycleDeposit", salesCar.RecycleDeposit.ToString());
                    //retCar.Add("CarLiabilityInsurance", salesCar.CarLiabilityInsurance.ToString());

                    retCar.Add("EngineType", salesCar.EngineType);
                    retCar.Add("FirstRegistration", salesCar.FirstRegistrationYear);
                    retCar.Add("NextInspectionDate", string.Format("{0:yyyy/MM/dd}",salesCar.NextInspectionDate));
                    retCar.Add("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", salesCar.ExpireDate));
                    retCar.Add("UsVin", salesCar.UsVin);
                    retCar.Add("MorterViecleOfficialCode", salesCar.MorterViecleOfficialCode);
                    retCar.Add("RegistrationNumberType", salesCar.RegistrationNumberType);
                    retCar.Add("RegistrationNumberKana", salesCar.RegistrationNumberKana);
                    retCar.Add("RegistrationNumberPlate", salesCar.RegistrationNumberPlate);
                    retCar.Add("CustomerCode", salesCar.UserCode);
                    retCar.Add("CustomerName", salesCar.User != null ? salesCar.User.CustomerName : "");
                    retCar.Add("CustomerNameKana", salesCar.User != null ? salesCar.User.CustomerNameKana : "");
                    retCar.Add("CustomerAddress", salesCar.User != null ? salesCar.User.Prefecture + salesCar.User.City + salesCar.User.Address1 + salesCar.User.Address2 : "");

                    retCar.Add("CustomerMemo", salesCar.User != null ? salesCar.User.Memo : "");    //Add 2022/07/06 yano #4145


                    retCar.Add("LaborRate",salesCar.CarGrade!=null && salesCar.CarGrade.Car!=null && salesCar.CarGrade.Car.Brand!=null ? salesCar.CarGrade.Car.Brand.LaborRate.ToString() : "");

                    // Mod 2015/09/14 arc yano #3252 サービス伝票入力画面のマスタボタンの挙動(類似対応) 車台番号から車両情報を取得できる項目の追加
                    retCar.Add("RegistrationDate", string.Format("{0:yyyy/MM/dd}", salesCar.RegistrationDate));         //登録年月日
                    retCar.Add("CustomerTelNumber", salesCar.User != null ? salesCar.User.TelNumber : "");              //電話番号  //Mod 2015/09/17 arc yano  #3261 車両伝票の車両選択で「マスタ取得に失敗しました」と表示 NULLの場合は空文字に変換

                    //Add 2017/05/10 arc yano #3762
                    retCar.Add("RegistrationNumber", salesCar.MorterViecleOfficialCode + " " + salesCar.RegistrationNumberType + " " + salesCar.RegistrationNumberKana + " " + salesCar.RegistrationNumberPlate);
                    retCar.Add("ColorType", salesCar.c_ColorCategory != null ? salesCar.c_ColorCategory.Name : "");



                    //Mod 2022/01/13 yano #4123
                    //Add 2020/06/09 yano #4052
                    CarPurchase rec = new CarPurchaseDao(db).GetBySalesCarNumber(code);
                    
                    //対象の仕入データが未仕入の場合
                    if(rec != null && rec.PurchaseStatus.Equals(CARPURCHASE_NOTPURCHASED))
                    {
                        salesCar.CarStatus = "999";       //未仕入
                    }
                    
                    retCar.Add("CarStatus", (salesCar.CarStatus ?? ""));

                    //初年度登録(yyyy/mm)から年月を取得
                    decimal fee = 0m;
                    string firstRegistrationYear = salesCar.FirstRegistrationYear;
                    if (!string.IsNullOrEmpty(firstRegistrationYear)) {
                        if (firstRegistrationYear.Split('/').Length == 2) {
                            string year = salesCar.FirstRegistrationYear.Split('/')[0];
                            string month = salesCar.FirstRegistrationYear.Split('/')[1];
                            DateTime firstRegist = new DateTime(int.Parse(year), int.Parse(month), 1);
                            DateTime today = DateTime.Today;
                            try {
                                if (firstRegist.AddMonths(24).CompareTo(today) > 0) {
                                    //24ヶ月未満
                                    fee = salesCar.CarGrade.Under24 ?? 0;
                                } else if (firstRegist.AddMonths(26).CompareTo(today) > 0) {
                                    //26ヵ月未満
                                    fee = salesCar.CarGrade.Under26 ?? 0;
                                } else if (firstRegist.AddMonths(28).CompareTo(today) > 0){
                                    //28ヵ月未満
                                    fee = salesCar.CarGrade.Under28 ?? 0;
                                } else if (firstRegist.AddMonths(30).CompareTo(today) > 0) {
                                    //30ヶ月未満
                                    fee = salesCar.CarGrade.Under30 ?? 0;
                                } else if (firstRegist.AddMonths(36).CompareTo(today) > 0) {
                                    //36ヶ月未満
                                    fee = salesCar.CarGrade.Under36 ?? 0;
                                } else if (firstRegist.AddMonths(72).CompareTo(today) > 0) {
                                    //72ヶ月未満
                                    fee = salesCar.CarGrade.Under72 ?? 0;
                                } else if (firstRegist.AddMonths(84).CompareTo(today) > 0) {
                                    //84ヶ月未満
                                    fee = salesCar.CarGrade.Under84 ?? 0;
                                } else {
                                    //84ヶ月以上
                                    fee = salesCar.CarGrade.Over84 ?? 0;
                                }
                            } catch (NullReferenceException) {
                            }
                            
                        }
                    }

                    retCar.Add("TradeInMaintenanceFee", fee.ToString());

                    //排気量から自動車税を取得する
                    //CarTax carTax = new CarTaxDao(db).GetByDisplacement(salesCar.Displacement ?? 0);
                    //retCar.Add("CarTax", carTax!=null ? carTax.Amount.ToString() : "0");

                    //自動車環境性能割
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax((salesCar.CarGrade.SalesPrice ?? 0m), 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                    retCar.Add("EPDiscountTaxList", retValue.Item1);
                    retCar.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2));
                    //retCar.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear)));
                    
                    //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                    //自賠責保険料および重量税
                    if (CommonUtils.DefaultString(salesCar.NewUsedType).Equals("N"))
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance!=null ? insurance.Amount : 0m));
                        CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarGrade.CarWeight ?? 0);
                        retCar.Add("CarWeightTax",string.Format("{0:0}",weightTax!=null ? weightTax.Amount : 0 ));
                    } else {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance!=null ? insurance.Amount : 0m));
                    }
                }
                return Json(retCar);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 車検有効期限から次回車検日を計算して返す
        /// </summary>
        /// <param name="registrationNumberType">登録番号（種別）</param>
        /// <param name="gengou">元号</param>
        /// <param name="year">年（和暦）</param>
        /// <param name="month">月（和暦）</param>
        /// <param name="day">日（和暦）</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetNextInspectionDate(string registrationNumberType, DateTime? expireDate, int? gengou, int? year, int? month, int? day) {
            if (expireDate == null) {
                expireDate = JapaneseDateUtility.GetGlobalDate(gengou, year, month, day, db);   //Mod 2018/06/22 arc yano #3891
                //expireDate = JapaneseDateUtility.GetGlobalDate(gengou, year, month, day);
            }
            if (expireDate == null) return new EmptyResult();
            
            DateTime returnDate;

            if (!string.IsNullOrEmpty(registrationNumberType) && (registrationNumberType.Substring(0, 1).Equals("1") || registrationNumberType.Substring(0, 1).Equals("4"))) {
                if (expireDate.Value.AddMonths(-18) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-18);
                } else if (expireDate.Value.AddMonths(-12) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-12);
                } else if (expireDate.Value.AddMonths(-6) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-6);
                } else {
                    returnDate = expireDate.Value.AddMonths(6);
                }
            } else {
                if (expireDate.Value.AddYears(-2) > DateTime.Today) {
                    returnDate = expireDate.Value.AddYears(-2);
                } else if (expireDate.Value.AddYears(-1) > DateTime.Today) {
                    returnDate = expireDate.Value.AddYears(-1);
                } else {
                    returnDate = expireDate.Value.AddYears(1);
                }
            }


            CodeDao dao = new CodeDao(db);
            JapaneseDate returnDateWareki = JapaneseDateUtility.GetJapaneseDate(returnDate);
            Dictionary<string, string> nextInspectionDate = new Dictionary<string, string>();
            nextInspectionDate.Add("Seireki", string.Format("{0:yyyy/MM/dd}",returnDate));
            nextInspectionDate.Add("GengouName", CodeUtils.GetName(CodeUtils.GetGengouList(db), returnDateWareki.Gengou.ToString()));   //Mod 2018/06/22 arc yano #3891
            //nextInspectionDate.Add("GengouName", CodeUtils.GetName(CodeUtils.GetGengouList(), returnDateWareki.Gengou.ToString()));
            nextInspectionDate.Add("Gengou", returnDateWareki.Gengou.ToString());
            nextInspectionDate.Add("Year", returnDateWareki.Year.ToString());
            nextInspectionDate.Add("Month", returnDateWareki.Month.ToString());
            nextInspectionDate.Add("Day", returnDateWareki.Day.ToString());
            return Json(nextInspectionDate);
        }


        // Add 
        /// <summary>
        /// 車台番号から管理番号を取得・表示する
        /// </summary>
        /// <param name="vinCode">車台番号</param>
        /// <returns></returns>
        /// <history>
        /// 2022/01/08 yano #4121 【サービス伝票入力】Chrome・明細行の部品在庫情報取得の不具合対応
        /// 2020/06/09 yano #4052 車両伝票入力】車台番号入力時のチェック漏れ対応
        /// 2014/07/24 arc amii 既存バグ対応 車台番号を手入力した時、管理番号を取得する処理を追加
        /// </history>
        public ActionResult GetSalesCarNumberFromVin(string vinCode)
        {
            if (Request.IsAjaxRequest())
            {

                // 車台番号をキーにレコードを取得
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(vinCode);
                Dictionary<string, string> ret = new Dictionary<string, string>();

                string cnt = "0";
                string vin = "";
                string number = "";
                //string status = "";   //Mod 2022/01/08 yano #4121

                // データがある場合、車台番号と管理番号と件数を設定する
                if (salesCarList != null && salesCarList.Count > 0)
                {
                    cnt = salesCarList.Count.ToString();              // 件数
                    vin = salesCarList[0].Vin;                        // 車台番号
                    number = salesCarList[0].SalesCarNumber;          // 管理番号
                    //status = (salesCarList[0].CarStatus ?? "");       // 在庫ステータス  //Mod 2022/01/08 yano #4121  //Add 2020/06/09 yano #4052
                }

                ret.Add("vin", vin);
                ret.Add("salesCarNumber", number);
                ret.Add("count", cnt);
                //ret.Add("status", status);                          //Mod 2022/01/08 yano #4121 //Add 2020/06/09 yano #4052

                return Json(ret);
            }
            
            return new EmptyResult();
        }


    #endregion

          #region 画面データ取得
          /// <summary>
          /// 画面表示データの取得
          /// </summary>
          /// <param name="salesCar">モデルデータ</param>
          /// <history>
          /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
          /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
          /// </history>
          private void GetEntryViewData(SalesCar salesCar) {

            // ブランド名，車種名，グレード名の取得
            if (!string.IsNullOrEmpty(salesCar.CarGradeCode)) {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade = carGradeDao.GetByKey(salesCar.CarGradeCode);
                if (carGrade != null) {
                    ViewData["CarGradeName"] = carGrade.CarGradeName;
                    try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { ViewData["CarBrandName"] = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                }
            }

            // お客様指定オイル，タイヤ名の取得
            PartsDao partsDao = new PartsDao(db);
            Parts parts;
            if (!string.IsNullOrEmpty(salesCar.Oil)) {
                parts = partsDao.GetByKey(salesCar.Oil);
                if (parts != null) {
                    ViewData["OilName"] = parts.PartsNameJp;
                }
            }
            if (!string.IsNullOrEmpty(salesCar.Tire)) {
                parts = partsDao.GetByKey(salesCar.Tire);
                if (parts != null) {
                    ViewData["TireName"] = parts.PartsNameJp;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), salesCar.NewUsedType, true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), salesCar.ColorType, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), salesCar.MileageUnit, false);
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), salesCar.UsageType, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), salesCar.Usage, true);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), salesCar.CarClassification, true);
            ViewData["MakerWarrantyList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.MakerWarranty, true);
            ViewData["RecordingNoteList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecordingNote, true);
            ViewData["ReparationRecordList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ReparationRecord, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), salesCar.Figure, true);
            ViewData["ImportList"] = CodeUtils.GetSelectListByModel(dao.GetImportAll(false), salesCar.Import, true);
            ViewData["GuaranteeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Guarantee, true);
            ViewData["InstructionsList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Instructions, true);
            ViewData["RecycleList"] = CodeUtils.GetSelectListByModel(dao.GetRecycleAll(false), salesCar.Recycle, true);
            ViewData["RecycleTicketList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecycleTicket, true);
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), salesCar.Steering, true);
            ViewData["ChangeColorList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ChangeColor, true);
            ViewData["LightList"] = CodeUtils.GetSelectListByModel(dao.GetLightAll(false), salesCar.Light, true);
            ViewData["AwList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aw, true);
            ViewData["AeroList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aero, true);
            ViewData["SrList"] = CodeUtils.GetSelectListByModel(dao.GetSrAll(false), salesCar.Sr, true);
            ViewData["CdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Cd, true);
            ViewData["MdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Md, true);
            ViewData["NaviTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.NaviType, true);
            ViewData["NaviEquipmentList"] = CodeUtils.GetSelectListByModel(dao.GetNaviEquipmentAll(false), salesCar.NaviEquipment, true);
            ViewData["NaviDashboardList"] = CodeUtils.GetSelectListByModel(dao.GetNaviDashboardAll(false), salesCar.NaviDashboard, true);
            ViewData["SeatTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSeatTypeAll(false), salesCar.SeatType, true);
            ViewData["DeclarationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetDeclarationTypeAll(false), salesCar.DeclarationType, true);
            ViewData["AcquisitionReasonList"] = CodeUtils.GetSelectListByModel(dao.GetAcquisitionReasonAll(false), salesCar.AcquisitionReason, true);
            ViewData["TaxationTypeCarTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeCarTax, true);
            ViewData["TaxationTypeAcquisitionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeAcquisitionTax, true);
            ViewData["ExpireTypeList"] = CodeUtils.GetSelectListByModel(dao.GetExpireTypeAll(false), salesCar.ExpireType, false);
            ViewData["CouponPresenceList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.CouponPresence, true);
            ViewData["DocumentCompleteList"] = CodeUtils.GetSelectListByModel(dao.GetDocumentCompleteAll(false), salesCar.DocumentComplete, true);
            ViewData["EraseRegistList"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), salesCar.EraseRegist, true);
            ViewData["FinanceList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), salesCar.Finance, true);
            ViewData["OwnershipChangeTypeList"] = CodeUtils.GetSelectListByModel<c_OwnershipChangeType>(dao.GetOwnershipChangeTypeAll(false), salesCar.OwnershipChangeType, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), salesCar.Fuel, true);
            //Add 2014/08/15 arc amii DMフラグ機能拡張対応 #3069 コンボボックスに設定する値を取得する処理を追加
            //Mod 2014/09/08 arc amii DMフラグ機能拡張対応 #3069 コンボボックスの空白行を入れないよう修正
            ViewData["InspectGuidFlagList"] = CodeUtils.GetSelectListByModel(dao.GetNeededAll(false), salesCar.InspectGuidFlag, false);
            //Add 2014/08/15 arc amii 在庫ステータス変更対応対応 #3071 入力画面表示時、在庫ステータス情報を取得する処理を追加
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);

            //Add 2014/10/16 arc yano 車両ステータス追加対応　利用用途のリストボックス追加
            //Mod 2014/10/30 arc amii 車両ステータス追加対応
            ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("004", false), salesCar.CarUsage, true);
            //ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCarUsageAll(false), salesCar.CarUsage, true);

            //Add 2014/08/15 arc amii 在庫ステータス変更対応 #3071 管理者権限のみ在庫ステータス使用可にする処理を追加
            //Mod 2015/07/29 arc nakayama #3217_デモカーが販売できてしまう問題の改善   在庫ステータス/利用用途は変更の権限があるユーザーのみ使用可にする。
            Employee emp = HttpContext.Session["Employee"] as Employee;
            //ログインユーザ情報取得
            Employee loginUser = new EmployeeDao(db).GetByKey(emp.EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "ChangeSalesCarStatus"); //車両マスタステータス変更権限があるかないか

            // 権限があればtrueそうでなければfalse 
            if (AppRole.EnableFlag){
                salesCar.CarStatusEnabled = true;
                salesCar.CarUsageEnabled = true;
            } else {
                salesCar.CarStatusEnabled = false;
                salesCar.CarUsageEnabled = false;
            }

            salesCar.IssueDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.IssueDate);
            string issueDateGengou = "";
            if (salesCar.IssueDateWareki != null) {
                issueDateGengou = salesCar.IssueDateWareki.Gengou.ToString();
            }
            ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), issueDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), issueDateGengou, false);

            salesCar.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.RegistrationDate);
            string registrationDateGengou = "";
            if (salesCar.RegistrationDateWareki != null) {
                registrationDateGengou = salesCar.RegistrationDateWareki.Gengou.ToString();
            }
            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registrationDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registrationDateGengou, false);

            DateTime parseResult;
            DateTime? firstRegistrationDate = null;
            if (DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out parseResult)) {
                firstRegistrationDate = DateTime.Parse(salesCar.FirstRegistrationYear + "/01");
            }
            salesCar.FirstRegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(firstRegistrationDate);
            string firstRegistrationDateGengou = "";
            if (salesCar.FirstRegistrationDateWareki != null) {
                firstRegistrationDateGengou = salesCar.FirstRegistrationDateWareki.Gengou.ToString();
            }
            ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), firstRegistrationDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), firstRegistrationDateGengou, false);

            salesCar.ExpireDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.ExpireDate);
            string expireDateGengou = "";
            if(salesCar.ExpireDate!=null){
                expireDateGengou = salesCar.ExpireDateWareki.Gengou.ToString();
            }

            ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), expireDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), expireDateGengou, false);

            salesCar.SalesDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.SalesDate);
            string salesDateGengou = "";
            if (salesCar.SalesDate != null)
            {
                salesDateGengou = salesCar.SalesDateWareki.Gengou.ToString();
            }
            ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), salesDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), salesDateGengou, false);

            salesCar.InspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.InspectionDate);
            string inspectionDateGengou = "";
            if (salesCar.InspectionDate != null) {
                inspectionDateGengou = salesCar.InspectionDateWareki.Gengou.ToString();
            }
            ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), inspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), inspectionDateGengou, false);

            salesCar.NextInspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.NextInspectionDate);
            string nextInspectionDateGengou = "";
            if (salesCar.NextInspectionDate != null) {
                nextInspectionDateGengou = salesCar.NextInspectionDateWareki.Gengou.ToString();
            }
            ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), nextInspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
                                                                                                                                            //ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), nextInspectionDateGengou, false);
            ////Mod 2021/08/02 yano #4097
            ////Add 2014/09/08 arc amii 年式入力対応 #3076 年式の桁数が4桁を超えていた場合、4桁で表示する
            //if (CommonUtils.DefaultString(salesCar.ManufacturingYear).Length > 10)
            //{
            //    salesCar.ManufacturingYear = salesCar.ManufacturingYear.Substring(0, 10);
            //}

        }
        #endregion

        #region 検索共通
        /// <summary>
        /// 車両マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両マスタ検索結果リスト</returns>
        private PaginatedList<SalesCar> GetSearchResultList(FormCollection form) {

            SalesCarDao salesCarDao = new SalesCarDao(db);
            SalesCar salesCarCondition = new SalesCar();
            salesCarCondition.SalesCarNumber = form["SalesCarNumber"];
            salesCarCondition.NewUsedType = form["NewUsedType"];
            salesCarCondition.ColorType = form["ColorType"];
            salesCarCondition.ExteriorColorCode = form["ExteriorColorCode"];
            salesCarCondition.ExteriorColorName = form["ExteriorColorName"];
            salesCarCondition.InteriorColorCode = form["InteriorColorCode"];
            salesCarCondition.InteriorColorName = form["InteriorColorName"];
            salesCarCondition.ManufacturingYear = form["ManufacturingYear"];
            salesCarCondition.CarStatus = form["CarStatus"];
            salesCarCondition.Vin = form["Vin"];
            salesCarCondition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];
            salesCarCondition.RegistrationNumberType = form["RegistrationNumberType"];
            salesCarCondition.RegistrationNumberKana = form["RegistrationNumberKana"];
            salesCarCondition.RegistrationNumberPlate = form["RegistrationNumberPlate"];
            salesCarCondition.Steering = form["Steering"];
            salesCarCondition.CarGrade = new CarGrade();
            salesCarCondition.CarGrade.Car = new Car();
            salesCarCondition.CarGrade.Car.Brand = new Brand();
            salesCarCondition.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            salesCarCondition.CarGrade.Car.CarName = form["CarName"];
            salesCarCondition.CarGrade.CarGradeName = form["CarGradeName"];
            salesCarCondition.Location = new Location();
            salesCarCondition.Location.LocationName = form["LocationName"];
            salesCarCondition.Customer = new Customer();
            salesCarCondition.Customer.CustomerName = form["CustomerName"];
            salesCarCondition.User = new Customer();
            salesCarCondition.User.CustomerName = form["UserName"];
            salesCarCondition.User.CustomerNameKana = form["UserNameKana"];

            //Mod 2014/10/16 arc yano 車両管理ステータス追加対応　検索条件に、利用用途を追加
            salesCarCondition.CarUsage = form["CarUsage"];

            if (!string.IsNullOrEmpty(form["DelFlag"]) && (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))) {
                salesCarCondition.DelFlag = form["DelFlag"];
            }
            return salesCarDao.GetListByCondition(salesCarCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region Validation
        /// <summary>
        /// 新規登録時のマスタ存在チェック
        /// </summary>
        /// <param name="salesCar"></param>
        private void ValidateForInsert(SalesCar salesCar) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(salesCar.Vin);
            if (list != null && list.Count > 0) {
                ModelState.AddModelError("Vin", "車台番号:" + salesCar.Vin + "は既に登録されています");
                ViewData["ErrorSalesCar"] = list;
            }
        }
        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="salesCar">車両データ</param>
        /// <returns>車両データ</returns>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2020/11/17 yano #4065 【車両伝票入力】環境性能割・マスタの設定値が不正の場合の対応
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// 2018/04/26 arc yano #3816 車両査定入力　管理番号にN/Aが入ってしまう
        /// </history>
        private SalesCar ValidateSalesCar(SalesCar salesCar) {

            // 必須チェック
            if (string.IsNullOrEmpty(salesCar.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0001", "グレード"));
            }
            else //Add 2018/04/26 arc yano #3816 グレードコードが入力されている場合はマスタチェックを行う
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("CarGradeCode", "車両グレードマスタに登録されていません。マスタ登録を行ってから再度実行して下さい");
                }
            }

            if (string.IsNullOrEmpty(salesCar.NewUsedType)) {
                ModelState.AddModelError("NewUsedType", MessageUtils.GetMessage("E0001", "新中区分"));
            }
            if (string.IsNullOrEmpty(salesCar.Vin)) {
                ModelState.AddModelError("Vin", MessageUtils.GetMessage("E0001", "車台番号"));
            }
            // Add 2014/09/11 arc amii 車検案内チェック対応 車検案内=「否」の場合、備考欄の必須チェックを行う
            if (!CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002"))
            {
                if (salesCar.InspectGuidFlag.Equals("002") && string.IsNullOrEmpty(salesCar.InspectGuidMemo))
                {
                    ModelState.AddModelError("InspectGuidMemo", MessageUtils.GetMessage("E0001", "車検案内発送備考欄"));
                }
            }
           
            if (ViewData["update"] != null && ViewData["update"].Equals("1")){
                if (string.IsNullOrEmpty(salesCar.OwnershipChangeType) && string.IsNullOrEmpty(salesCar.OwnershipChangeMemo)) {
                    ModelState.AddModelError("OwnershipChangeType", MessageUtils.GetMessage("E0001", "変更区分または変更理由のいずれか"));
                }
                if (!ModelState.IsValidField("OwnershipChangeDate")) {
                    ModelState.AddModelError("OwnershipChangeDate", MessageUtils.GetMessage("E0005", "変更日"));
                }
            }


            // 属性チェック
            if (!ModelState.IsValidField("SalesPrice")) {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の整数のみ" }));
            }

            //2021/08/02 yano #4097
            //Add 2014/09/08 arc amii 年式入力対応 #3076 年式の入力チェック(4桁数値以外はエラー)を追加
            if (!ModelState.IsValidField("ManufacturingYear"))
            {
                ModelState.AddModelError("ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "年式", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
            }
            //if (!ModelState.IsValidField("IssueDate")) {
            //    ModelState.AddModelError("IssueDate", MessageUtils.GetMessage("E0005", "発行日"));
            //}
            //if (!ModelState.IsValidField("RegistrationDate")) {
            //    ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "登録日"));
            //}
            if (!ModelState.IsValidField("Capacity")) {
                ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
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
            //if (!ModelState.IsValidField("ExpireDate")) {
            //    ModelState.AddModelError("ExpireDate", MessageUtils.GetMessage("E0005", "有効期限"));
            //}
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
            }
            if (!ModelState.IsValidField("CarTax")) {
                ModelState.AddModelError("CarTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税種別割", "正の整数のみ" }));    //Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("CarWeightTax")) {
                ModelState.AddModelError("CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "自動車重量税", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("AcquisitionTax")) {
                ModelState.AddModelError("AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税環境性能割", "正の整数のみ" }));//Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("CarLiabilityInsurance")) {
                ModelState.AddModelError("CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "自賠責保険料", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SalesDate")) {
                ModelState.AddModelError("SalesDate", MessageUtils.GetMessage("E0005", "納車日"));
            }
            if (!ModelState.IsValidField("InspectionDate")) {
                ModelState.AddModelError("InspectionDate", MessageUtils.GetMessage("E0005", "点検日"));
            }
            if (!ModelState.IsValidField("NextInspectionDate")) {
                ModelState.AddModelError("NextInspectionDate", MessageUtils.GetMessage("E0005", "次回点検日"));
            }
            if (!ModelState.IsValidField("ProductionDate")) {
                ModelState.AddModelError("ProductionDate", MessageUtils.GetMessage("E0005", "生産日"));
            }
            if (!ModelState.IsValidField("ApprovedCarWarrantyDateFrom")){
                ModelState.AddModelError("ApprovedCarWarrantyDateFrom", MessageUtils.GetMessage("E0005", "認定中古車保証期間(開始)"));
            }
            if (!ModelState.IsValidField("ApprovedCarWarrantyDateTo")) {
                ModelState.AddModelError("ApprovedCarWarrantyDateTo", MessageUtils.GetMessage("E0005", "認定中古車保証期間(終了)"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("SalesPrice") && salesCar.SalesPrice != null) {
                if (!Regex.IsMatch(salesCar.SalesPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の整数のみ" }));
                }
            }


            //Mod 2021/08/02 yano #4097
            //Add 2014/09/08 arc amii 年式入力対応 #3076 年式の入力チェック(4桁数値以外はエラー)を追加
            if (ModelState.IsValidField("ManufacturingYear") && CommonUtils.DefaultString(salesCar.ManufacturingYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}\.\d{1,2}$"))
                    && (!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "年式", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
                }
            }

            if (ModelState.IsValidField("Mileage") && salesCar.Mileage != null) {
                if ((Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
                }
            }
            if (ModelState.IsValidField("CarTax") && salesCar.CarTax != null) {
                if (!Regex.IsMatch(salesCar.CarTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税", "正の整数のみ" }));    //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("CarWeightTax") && salesCar.CarWeightTax != null) {
                if (!Regex.IsMatch(salesCar.CarWeightTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "自動車重量税", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("AcquisitionTax") && salesCar.AcquisitionTax != null) {
                if (!Regex.IsMatch(salesCar.AcquisitionTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税環境性能割", "正の整数のみ" }));   //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("CarLiabilityInsurance") && salesCar.CarLiabilityInsurance != null) {
                if (!Regex.IsMatch(salesCar.CarLiabilityInsurance.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "自賠責保険料", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RecycleDeposit") && salesCar.RecycleDeposit != null) {
                if (!Regex.IsMatch(salesCar.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の整数のみ" }));
                }
            }
            //if (!string.IsNullOrEmpty(salesCar.FirstRegistrationYear)) {
            //    if (!Regex.IsMatch(salesCar.FirstRegistrationYear, "([0-9]{4})/([0-9]{2})")) {
            //        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録"));
            //    }
            //    DateTime result;
            //    try {
            //        DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out result);
            //        if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
            //            ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録"));
            //            if (ModelState["FirstRegistrationYear"].Errors.Count() > 1) {
            //                ModelState["FirstRegistrationYear"].Errors.RemoveAt(0);
            //            }
            //        }
            //    } catch {
            //        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "初度登録"));
            //    }

            //}

            // 値チェック
            if (ModelState.IsValidField("Capacity") && salesCar.Capacity != null) {
                if (salesCar.Capacity < 0) {
                    ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("MaximumLoadingWeight") && salesCar.MaximumLoadingWeight != null) {
                if (salesCar.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("CarWeight") && salesCar.CarWeight != null) {
                if (salesCar.CarWeight < 0) {
                    ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("TotalCarWeight") && salesCar.TotalCarWeight != null) {
                if (salesCar.TotalCarWeight < 0) {
                    ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Length") && salesCar.Length != null) {
                if (salesCar.Length < 0) {
                    ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Width") && salesCar.Width != null) {
                if (salesCar.Width < 0) {
                    ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Height") && salesCar.Height != null) {
                if (salesCar.Height < 0) {
                    ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("FFAxileWeight") && salesCar.FFAxileWeight != null) {
                if (salesCar.FFAxileWeight < 0) {
                    ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("FRAxileWeight") && salesCar.FRAxileWeight != null) {
                if (salesCar.FRAxileWeight < 0) {
                    ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RFAxileWeight") && salesCar.RFAxileWeight != null) {
                if (salesCar.RFAxileWeight < 0) {
                    ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RRAxileWeight") && salesCar.RRAxileWeight != null) {
                if (salesCar.RRAxileWeight < 0) {
                    ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && salesCar.Displacement != null) {
                if ((Regex.IsMatch(salesCar.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数10桁以内かつ少数2桁以内" }));
                }
            }
            // 和暦→西暦の変換チェック
            if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki)) {
                ModelState.AddModelError("IssueDateWareki.Year", MessageUtils.GetMessage("E0021", "発行日"));
            }
            if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki)) {
                ModelState.AddModelError("RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "登録年月日／交付年月日"));
            }
            salesCar.FirstRegistrationDateWareki.Day = 1; 
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {
                if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
                {
                    //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki)) {
                    ModelState.AddModelError("FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "初度登録年月"));
                    ModelState.AddModelError("FirstRegistrationDateWareki.Month", "");
                }
                //Add 2020/11/17 yano #4065
                else
                {
                    DateTime? FirstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);

                    //初度登録年月が本日より30日以降の日付で設定されていた場合
                    if (FirstRegistrationDate != null && (((DateTime)(FirstRegistrationDate ?? DateTime.Today).Date - DateTime.Today.Date).TotalDays > 30))
                    {
                        ModelState.AddModelError("FirstRegistrationDateWareki.Year", "初度登録年月には未来の日付は設定できません");
                        ModelState.AddModelError("FirstRegistrationDateWareki.Month", "");
                    }
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki)) {
                ModelState.AddModelError("ExpireDateWareki.Year", MessageUtils.GetMessage("E0021", "有効期限"));
            }
            if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki)) {
                ModelState.AddModelError("InspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "点検日"));
            }

            if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki, db))      //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki)) {
                ModelState.AddModelError("NextInspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "次回点検日"));
            }
            return salesCar;
        }
        #endregion
    }
}
