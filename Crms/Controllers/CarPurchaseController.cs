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
using System.Transactions;
using Crms.Models;                      //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
using OfficeOpenXml;                    //Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
using System.Configuration;             //Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
using System.IO;                        //Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
using System.Data;                      //Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応

namespace Crms.Controllers {

    /// <summary>
    /// 車両仕入機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarPurchaseController : Controller {

        #region 初期化
        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "車両仕入";               // 画面名
        private static readonly string PROC_NAME = "車両仕入登録";           // 処理名
        private static readonly decimal DECIMAL_MAX = 9999999999;              // 価格MAX
        private static readonly decimal DECIMAL_MIN = -9999999999;             // 価格MIN
        //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
        //private static readonly string LAST_EDIT_PURCHASE = "003";           // 車両仕入画面で更新した時の値
        private static readonly string PROC_NAME_EXCEL = "車両仕入Excel出力";// 処理名 Add 2017/03/22 arc nakayama #3730_仕入リスト

        //Add 2019/02/07 yano #3960
        private static readonly string PURCHASETYPE_TRADEIN = "001";         //仕入区分(下取車)
        private static readonly string PURCHASETYPE_DIPOSAL = "005";         //仕入区分(依廃)

        //Add 2021/08/09 yano #4086
        private static readonly string ACCOUNTTYPE_TRADEIN    = "013";      //入金種別(下取車） 
        private static readonly string ACCOUNTTYPE_REMAINDEBT = "012";      //入金種別(残債）

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarPurchaseController() {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 検索画面
        /// <summary>
        /// 車両仕入検索画面表示
        /// </summary>
        /// <returns>車両仕入検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();

            form["RequestFlag"] = "1";

            return Criteria(form);
        }

        /// <summary>
        /// 車両仕入検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両仕入検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            PaginatedList<CarPurchase> list = new PaginatedList<CarPurchase>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                case "1": // 検索ボタン

                    // 検索結果リストの取得
                    list = GetSearchResultList(form);
                    SetDataComponent(form);
                    return View("CarPurchaseCriteria", list);

                case "2": // Excelボタン
                    SetDataComponent(form);
                    return ExcelDownload(form);

                default:
                    // 検索画面の表示
                    SetDataComponent(form);
                    return View("CarPurchaseCriteria", list);
            }
        }

        #region 画面コンポーネント設定
        private void SetDataComponent(FormCollection form)
        {
            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(form["SupplierCode"]);
                if (supplier != null)
                {
                    ViewData["SupplierName"] = supplier.SupplierName;
                }
            }
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("023", false), form["PurchaseStatus"], true);
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];
            ViewData["SlipDateFrom"] = form["SlipDateFrom"];
            ViewData["SlipDateTo"] = form["SlipDateTo"];
            ViewData["PurchaseDateFrom"] = form["PurchaseDateFrom"];
            ViewData["PurchaseDateTo"] = form["PurchaseDateTo"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["Vin"] = form["Vin"];
        }
        #endregion

        #endregion

        #region 入力画面
        /// <summary>
        /// 車両仕入テーブル入力画面表示
        /// </summary>
        /// <param name="id">車両仕入コード(コピー登録または更新時のみ設定)
        ///                  及びコピー登録フラグ(コピー登録時"1"、左記以外の時"0")
        ///                  ※カンマ区切り</param>
        /// <returns>車両仕入テーブル入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            string[] idArr = CommonUtils.DefaultString(id).Split(new string[]{","}, StringSplitOptions.None);
            CarPurchase carPurchase;

            // 追加の場合
            if (string.IsNullOrEmpty(id)) {

                ViewData["update"] = "0";
                ViewData["copy"] = "0";
                carPurchase = new CarPurchase();
                carPurchase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carPurchase.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
                carPurchase.LastEditScreen = "000";
                carPurchase.SalesCar = new SalesCar();

            // コピー登録の場合
            } else if (idArr.Count()>1 && CommonUtils.DefaultString(idArr[1]).Equals("1")) {

                ViewData["update"] = "0";
                ViewData["copy"] = "1";
                carPurchase = CreateCopyCarPurchase(new Guid(idArr[0]));

            // 更新の場合
            } else {
                ViewData["update"] = "1";
                ViewData["copy"] = "0";
                carPurchase = new CarPurchaseDao(db).GetByKey(new Guid(idArr[0]));
                if (string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                    carPurchase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                }
                if (string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                    carPurchase.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
                // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                // 仕入計上済み、且つ締め処理済みの月の場合、変更不可フラグを立てる
                if (carPurchase.PurchaseStatus != null && carPurchase.PurchaseStatus.Equals("002")
                    && !new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ViewData["ClosedMonth"] = "1";
                }
            }
            ViewData["PurchaseStatus"] = carPurchase.PurchaseStatus;

            // その他表示データの取得
            GetEntryViewData(ref carPurchase, new FormCollection());

            // 出口
            return View("CarPurchaseEntry", carPurchase);
        }
        #endregion

        #region 追加更新
        /// <summary>
        /// 車両仕入テーブル追加更新
        /// </summary>
        /// <param name="carPurchase">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両仕入テーブル入力画面</returns>
        /// <history>
        /// 2021/08/09 yano #4086【車両仕入入力】下取車を変更した際の入金実績リスト削除漏れ対応
        /// 2018/07/31 yano.hiroki #3919 下取車仕入後の車台番号変更自の対応
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// 2017/11/15 arc yano #3826 車両仕入入力　新中区分を未選択でも保存が行える
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応 ModelState.Clear()対応
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarPurchase carPurchase, FormCollection form,SalesCar salesCar) {

            // モデルデータ算出設定項目の設定
            //carPurchase.calculate();

            // Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];
            ViewData["copy"] = form["copy"];

            // Add 2014/08/06 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // 各種データ登録処理
            using (TransactionScope ts = new TransactionScope()) {

                CarPurchase targetCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

                //Add 2021/08/09 yano #4086
                string prevSlipNumber = "";   //伝票番号(編集前）
                string prevVin = "";          //車台番号(編集前）

                //下取車の仕入データが作成済の場合は、そのデータの伝票番号と車台番号を控えておく
                if(targetCarPurchase != null && !string.IsNullOrWhiteSpace(targetCarPurchase.CarPurchaseType) && targetCarPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN))
                {
                    prevSlipNumber = targetCarPurchase.SlipNumber;   
                    prevVin = targetCarPurchase.Vin;
                }


                // 処理制御情報(仕入ステータス)の取得
                ViewData["PurchaseStatus"] = "";
                if (ViewData["update"].Equals("1"))
                {
                    CarPurchase dbCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                    if (dbCarPurchase != null)
                    {
                        ViewData["PurchaseStatus"] = dbCarPurchase.PurchaseStatus;
                    }
                }


                //仕入削除
                if (form["action"].Equals("DeleteStock"))
                {
                    //削除処理
                    //データチェック
                    Deletevalidation(carPurchase);
                    if (!ModelState.IsValid)
                    {
                        GetEntryViewData(ref carPurchase, form);
                        ViewData["PurchaseStatus"] = targetCarPurchase.PurchaseStatus;
                        return View("CarPurchaseEntry", carPurchase);
                    }

                    //-------------------------------------
                    //削除処理
                    //-------------------------------------
                    //仕入データの削除
                    //対象のDelFlag を 1 にする
                    targetCarPurchase.DelFlag = "1";                                                            //削除フラグ
                    targetCarPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //最終更新者
                    targetCarPurchase.LastUpdateDate = DateTime.Now;                                            //最終更新日

                    //査定データの削除
                    if (targetCarPurchase.CarAppraisalId != null)
                    {
                        CarAppraisal targetCarAppraisal = new CarAppraisalDao(db).GetByKey((targetCarPurchase.CarAppraisalId ?? new Guid("00000000-0000-0000-0000-000000000000")));

                        //査定データが取得できた場合は査定データの削除
                        if (targetCarAppraisal != null)
                        {
                            targetCarAppraisal.DelFlag = "1";
                            targetCarAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //最終更新者
                            targetCarAppraisal.LastUpdateDate = DateTime.Now;
                        }
                    }
                    //車両マスタの削除
                    if (!string.IsNullOrWhiteSpace(targetCarPurchase.SalesCarNumber))
                    {
                        SalesCar targetSalesCar = new SalesCarDao(db).GetByKey(targetCarPurchase.SalesCarNumber);

                        if (targetSalesCar != null)
                        {
                            targetSalesCar.DelFlag = "1";
                            targetSalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //最終更新者
                            targetSalesCar.LastUpdateDate = DateTime.Now;
                        }
                    }
                }
                else
                {
                    CarPurchase checkPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                    if (checkPurchase != null)
                    {
                        if (!string.IsNullOrEmpty(checkPurchase.PurchaseStatus) && checkPurchase.PurchaseStatus.Equals("002")
                        && !new InventoryScheduleDao(db).IsClosedInventoryMonth(checkPurchase.DepartmentCode, checkPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode) && form["action"].Equals("CancelStock"))
                        {
                            //締まっていて、かつ、仕入れキャンセルの時はチェックしない
                        }
                        else
                        {
                            // データチェック                    
                            ValidateCarPurchase(carPurchase, form);
                            if (!ModelState.IsValid)
                            {
                                GetEntryViewData(ref carPurchase, form);
                                return View("CarPurchaseEntry", carPurchase);
                            }
                        }
                    }
                    else //新規作成の場合　//Add 2017/11/15 arc yano #3826
                    {
                        // データチェック                    
                        ValidateCarPurchase(carPurchase, form);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
                        }
                    }

                    //仕入れキャンセル
                    if (form["action"].Equals("CancelStock"))
                    {
                        Cancelvalidation(carPurchase);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            ViewData["PurchaseStatus"] = targetCarPurchase.PurchaseStatus;
                            return View("CarPurchaseEntry", carPurchase);
                        }

                        //キャンセルデータ作成
                        CarPurchase CancelPurchaseData = CreateCancelData(carPurchase);

                        //キャンセルデータと元仕入れデータの関連つけ
                        targetCarPurchase.CancelCarPurchaseId = CancelPurchaseData.CarPurchaseId.ToString();
                    }
                }

                //仕入削除処理以外
                if (!form["action"].Equals("DeleteStock"))
                {
                    // 和暦を西暦に変換
                    if (!salesCar.IssueDateWareki.IsNull)
                    {
                        salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki);
                    }
                    if (!salesCar.RegistrationDateWareki.IsNull)
                    {
                        salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki);
                    }
                    salesCar.FirstRegistrationDateWareki.Day = 1;
                    if (!salesCar.FirstRegistrationDateWareki.IsNull)
                    {
                        DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki);

                        if (firstRegistrationDate.HasValue)
                        {
                            salesCar.FirstRegistrationYear = firstRegistrationDate.Value.Year + "/" + firstRegistrationDate.Value.Month;
                        }
                    }
                    if (!salesCar.ExpireDateWareki.IsNull)
                    {
                        salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki,db); //Mod 2018/06/22 arc yano #3891
                        //salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki);
                    }
                    if (!salesCar.SalesDateWareki.IsNull)
                    {
                        salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki, db); //Mod 2018/06/22 arc yano #3891
                        //salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki);
                    }
                    if (!salesCar.InspectionDateWareki.IsNull)
                    {
                        salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                        //salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki);
                    }
                    if (!salesCar.NextInspectionDateWareki.IsNull)
                    {
                        salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                        //salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki);
                    }
                    // 車両仕入データ登録処理
                    if (form["update"].Equals("1"))
                    {
                        //CarPurchase targetCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                        UpdateModel(targetCarPurchase);
                        //targetCarPurchase.Amount = carPurchase.Amount;
                        if (targetCarPurchase.CarPurchaseOrder != null)
                        {
                            targetCarPurchase.CarPurchaseOrder.Amount = carPurchase.Amount;
                            targetCarPurchase.CarPurchaseOrder.DiscountAmount = carPurchase.DiscountPrice;
                            targetCarPurchase.CarPurchaseOrder.FirmMargin = carPurchase.FirmPrice;
                            targetCarPurchase.CarPurchaseOrder.MetallicPrice = carPurchase.MetallicPrice;
                            targetCarPurchase.CarPurchaseOrder.OptionPrice = carPurchase.OptionPrice;
                            targetCarPurchase.CarPurchaseOrder.VehiclePrice = carPurchase.VehiclePrice;
                        }

                        //Mod 2021/08/09 yano #4086
                        carPurchase = EditCarPurchaseForUpdate(targetCarPurchase, form, prevSlipNumber, prevVin);
                        //carPurchase = EditCarPurchaseForUpdate(targetCarPurchase, form);
                        carPurchase.SalesCar.IssueDate = salesCar.IssueDate;
                        carPurchase.SalesCar.RegistrationDate = salesCar.RegistrationDate;
                        carPurchase.SalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                        carPurchase.SalesCar.ExpireDate = salesCar.ExpireDate;
                        carPurchase.SalesCar.SalesDate = salesCar.SalesDate;
                        carPurchase.SalesCar.InspectionDate = salesCar.InspectionDate;
                        carPurchase.SalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                    }
                    else
                    {
                        ValidateForInsert(carPurchase);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
                        }
                        carPurchase = EditCarPurchaseForInsert(carPurchase, form);
                        carPurchase.SalesCar.IssueDate = salesCar.IssueDate;
                        carPurchase.SalesCar.RegistrationDate = salesCar.RegistrationDate;
                        carPurchase.SalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                        carPurchase.SalesCar.ExpireDate = salesCar.ExpireDate;
                        carPurchase.SalesCar.SalesDate = salesCar.SalesDate;
                        carPurchase.SalesCar.InspectionDate = salesCar.InspectionDate;
                        carPurchase.SalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                        db.CarPurchase.InsertOnSubmit(carPurchase);
                    }

                    carPurchase.Vin = carPurchase.SalesCar.Vin; //Add 2018/07/31 yano.hiroki #3919 

                    carPurchase.LastEditScreen = "000";

                    //仕入キャンセルの場合
                    if (!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("CancelStock"))
                    {
                        carPurchase.SalesCar.DelFlag = "1";
                        carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //最終更新者
                        carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
                    }
                }

                
                
                #region 旧コード
                //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
                //下取車の場合は伝票と査定データも更新する
                /*if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                {
                    CarSalesHeader CarSlipData = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);

                    //納車前・納車済の時は査定に反映しない
                    //Add 2017/01/13 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
                    if (!CarSlipData.SalesOrderStatus.Equals("004") && !CarSlipData.SalesOrderStatus.Equals("005"))
                    {
                        //査定データ更新
                        CarAppraisal CarAppraisalData = new CarAppraisalDao(db).GetBySlipNumberVin(carPurchase.SlipNumber, carPurchase.SalesCar.Vin);
                        if (CarAppraisalData != null)
                        {
                            if (CarAppraisalData.AppraisalPrice != carPurchase.TotalAmount || CarAppraisalData.CarTaxUnexpiredAmount != carPurchase.CarTaxAppropriateAmount || CarAppraisalData.RecycleDeposit != carPurchase.RecycleAmount)
                            {
                                CarAppraisalData.LastEditScreen = LAST_EDIT_PURCHASE;
                                carPurchase.LastEditScreen = "000";
                            }
                            else
                            {
                                carPurchase.LastEditScreen = "000";
                            }

                            CarAppraisalData.AppraisalPrice = carPurchase.TotalAmount;
                            CarAppraisalData.CarTaxUnexpiredAmount = carPurchase.CarTaxAppropriateAmount;
                            CarAppraisalData.RecycleDeposit = carPurchase.RecycleAmount;
                            CarAppraisalData.LastUpdateDate = DateTime.Now;
                            CarAppraisalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        }
                    }
                    else
                    {
                        carPurchase.LastEditScreen = "000";
                    }


                    //車両伝票更新
                    CreateTradeReceiptPlan(CarSlipData, carPurchase);
                }
                else
                {
                    carPurchase.LastEditScreen = "000";
                }*/
                #endregion

                // Add 2014/08/06 arc amii エラーログ対応 catch句にChangeConflictExceptionを追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // DBアクセスの実行
                        db.SubmitChanges();
                        // コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // Add 2014/08/06 arc amii エラーログ対応 SqlException発生時、エラーログ出力する処理を追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveStock") ? "仕入計上" : "保存")));
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
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
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
                ViewData["reportParam"] = form["reportParam"];
            } else {
                // 出口
                ViewData["close"] = "1";
            }
            return Entry(carPurchase.CarPurchaseId.ToString());
        }
    #endregion

        #region 画面データの取得
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carPurchase">モデルデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2019/02/09 yano #3973 車両仕入入力　和暦の入力項目が消える
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// </history>
        private void GetEntryViewData(ref CarPurchase carPurchase, FormCollection form) {

            SalesCar salesCar = carPurchase.SalesCar;

            Department department = null;       //Mod 2018/10/25 yano #3947


            // 部門名の取得
            if (!string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                department = departmentDao.GetByKey(carPurchase.DepartmentCode);    //Mod 2018/10/25 yano #3947
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // 担当者名の取得
            if (!string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(carPurchase.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    carPurchase.Employee = employee;
                }
            }

            // 仕入先名の取得
            if (!string.IsNullOrEmpty(carPurchase.SupplierCode)) {
                SupplierDao supplierDao = new SupplierDao(db);
                Supplier supplier = supplierDao.GetByKey(carPurchase.SupplierCode);
                if (supplier != null) {
                    ViewData["SupplierName"] = supplier.SupplierName;
                }
            }

            // 入庫ロケーション名の取得
            if (!string.IsNullOrEmpty(carPurchase.PurchaseLocationCode)) {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(carPurchase.PurchaseLocationCode);
                if (location != null) {
                    ViewData["PurchaseLocationName"] = location.LocationName;
                }
            }

            // 入庫区分リストの取得
            ViewData["CarPurchaseTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCarPurchaseTypeAll(false), carPurchase.CarPurchaseType, true);

            //// 仕入ステータスによる処理制御
            //if (CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002")) {

            //    // 車両情報の取得
            //    carPurchase.SalesCar = new SalesCarDao(db).GetByKey(carPurchase.SalesCarNumber);

            //} else {

            // グレード名の取得
            if (!string.IsNullOrEmpty(salesCar.CarGradeCode)) {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade = carGradeDao.GetByKey(salesCar.CarGradeCode);
                if (carGrade != null) {
                    salesCar.CarGradeName = carGrade.CarGradeName;
                    try { salesCar.CarName = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { salesCar.CarBrandName = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                    //ViewData["CarGradeName"] = carGrade.CarGradeName;
                    //try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    //try { ViewData["CarBrandName"] = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                }
            }

            // 車両情報の取得
            if (ViewData["update"].Equals("1")) {
                SalesCarDao salesCarDao = new SalesCarDao(db);
                SalesCar dbSalesCar = salesCarDao.GetByKey(salesCar.SalesCarNumber);
                if (dbSalesCar != null) {
                    //ViewData["SalesCarNumber"] = dbSalesCar.SalesCarNumber;
                    //try { ViewData["CarStatusName"] = dbSalesCar.c_CarStatus.Name; } catch (NullReferenceException) { }
                    //try { ViewData["LocationName"] = dbSalesCar.Location.LocationName; } catch (NullReferenceException) { }
                    ViewData["PossesorName"] = dbSalesCar.PossesorName;
                    ViewData["PossesorAddress"] = dbSalesCar.PossesorAddress;
                    ViewData["UserName"] = dbSalesCar.UserName;
                    ViewData["UserAddress"] = dbSalesCar.UserAddress;
                    ViewData["PrincipalPlace"] = dbSalesCar.PrincipalPlace;
                }
            }

            // タイヤ名の取得
            PartsDao partsDao = new PartsDao(db);
            Parts parts;
            if (!string.IsNullOrEmpty(salesCar.Tire)) {
                parts = partsDao.GetByKey(salesCar.Tire);
                if (parts != null) {
                    ViewData["TireName"] = parts.PartsNameJp;
                }
            }

            // オイル名の取得
            if (!string.IsNullOrEmpty(salesCar.Oil)) {
                try { ViewData["OilName"] = partsDao.GetByKey(salesCar.Oil).PartsNameJp; } catch (NullReferenceException) { }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), salesCar.NewUsedType, true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), salesCar.ColorType, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), salesCar.MileageUnit, false);
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
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), salesCar.Fuel, true);
            //Add 2014/08/15 arc amii DMフラグ機能拡張対応 #3069 コンボボックスに設定する値を取得する処理を追加
            //Mod 2014/09/08 arc amii DMフラグ機能拡張対応 #3069 コンボボックスの空白行を入れないよう修正
            ViewData["InspectGuidFlagList"] = CodeUtils.GetSelectListByModel(dao.GetNeededAll(false), salesCar.InspectGuidFlag, false);
            ViewData["MakerWarrantyList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.MakerWarranty, true);
            //Add 2014/08/15 arc amii 在庫ステータス変更対応対応 #3071 入力画面表示時、在庫ステータス情報を取得する処理を追加
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);

            //Add 2014/10/16 arc yano 車両ステータス追加対応　利用用途のリストボックス追加
            //Add 2014/10/30 arc amii 車両ステータス追加対応
            ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("004", false), salesCar.CarUsage, true);
            //ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCarUsageAll(false), salesCar.CarUsage, true);

            
            //ADD 2014/02/20 ookubo
            carPurchase.RateEnabled = true;
            //管理者権限のみ消費税率使用可
            Employee emp = HttpContext.Session["Employee"] as Employee;
            //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
            if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
            {
                carPurchase.RateEnabled = false;
            }
            //消費税IDが未設定であれば、当日日付で消費税ID取得
            if (carPurchase.ConsumptionTaxId == null)
            {
                carPurchase.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                carPurchase.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carPurchase.ConsumptionTaxId));
            }
            //Add 2014/08/15 arc amii 在庫ステータス変更対応 #3071 管理者権限のみ在庫ステータス使用可にする処理を追加
            if ("999".Equals(emp.SecurityRoleCode))
            {
                salesCar.CarStatusEnabled = true;
            }
            else
            {
                salesCar.CarStatusEnabled = false;
            }
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carPurchase.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carPurchase.ConsumptionTaxId;
            ViewData["Rate"] = carPurchase.Rate;
            ViewData["ConsumptionTaxIdOld"] = carPurchase.ConsumptionTaxId;
            ViewData["PurchasePlanDateOld"] = carPurchase.PurchaseDate;
            //ADD ookubo.end
            
            carPurchase.MetallicAmount = carPurchase.MetallicAmount ?? 0m;
            carPurchase.MetallicTax = carPurchase.MetallicTax ?? 0m;

            carPurchase.OptionAmount = carPurchase.OptionAmount ?? 0m;
            carPurchase.OptionTax = carPurchase.OptionTax ?? 0m;

            carPurchase.OthersAmount = carPurchase.OthersAmount ?? 0m;
            carPurchase.OthersTax = carPurchase.OthersTax ?? 0m;

            carPurchase.VehicleAmount = carPurchase.VehicleAmount ?? 0m;
            carPurchase.VehicleTax = carPurchase.VehicleTax ?? 0m;

            carPurchase.AuctionFeeAmount = carPurchase.AuctionFeeAmount ?? 0m;
            carPurchase.AuctionFeePrice = carPurchase.AuctionFeePrice ?? 0m;
            carPurchase.AuctionFeeTax = carPurchase.AuctionFeeTax ?? 0m;

            carPurchase.FirmAmount = carPurchase.FirmAmount ?? 0m;
            carPurchase.FirmTax = carPurchase.FirmTax ?? 0m;

            carPurchase.CarTaxAppropriateAmount = carPurchase.CarTaxAppropriateAmount ?? 0m;
            carPurchase.CarTaxAppropriatePrice = carPurchase.CarTaxAppropriatePrice ?? 0m;
            carPurchase.CarTaxAppropriateTax = carPurchase.CarTaxAppropriateTax ?? 0m;
            
            carPurchase.DiscountAmount = carPurchase.DiscountAmount ?? 0m;
            carPurchase.DiscountTax = carPurchase.DiscountTax ?? 0m;
            
            carPurchase.EquipmentTax = carPurchase.EquipmentTax ?? 0m;
            carPurchase.EquipmentAmount = carPurchase.EquipmentAmount ?? 0m;

            carPurchase.TotalAmount = carPurchase.TotalAmount ?? 0m;
            
            carPurchase.RecycleAmount = carPurchase.RecycleAmount ?? 0m;
            carPurchase.RecyclePrice = carPurchase.RecyclePrice ?? 0m;

            carPurchase.RepairAmount = carPurchase.RepairAmount ?? 0m;
            carPurchase.RepairTax = carPurchase.RepairTax ?? 0m;

            
            //---------------------
            // 発行日
            //---------------------
            //Mod 2019/02/09 yano #3973
            //発行日(datetime型)がnullでない場合は、それを元に和暦を設定
            if (salesCar.IssueDate != null)
            {
                salesCar.IssueDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.IssueDate);
            }
          
            string issueDateGengou = "";

            if (salesCar.IssueDateWareki != null)
            {
                issueDateGengou = salesCar.IssueDateWareki.Gengou.ToString();
            }

            ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), issueDateGengou, false);     //Mod 2018/06/22 arc yano #3891
            //ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), issueDateGengou, false);

            //---------------------
            // 登録年月日
            //---------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.RegistrationDate != null)
            {
                salesCar.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.RegistrationDate);
            }

            string registrationDateGengou = "";

            if (salesCar.RegistrationDateWareki != null) 
            {
                registrationDateGengou = salesCar.RegistrationDateWareki.Gengou.ToString();
            }
            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registrationDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registrationDateGengou, false);


            //---------------------
            // 初年度登録
            //---------------------
            DateTime parseResult;
            DateTime? firstRegistrationDate = null;
            if (DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out parseResult)) {
                firstRegistrationDate = DateTime.Parse(salesCar.FirstRegistrationYear + "/01");
            }
            //Mod 2019/02/09 yano #3973
            if (firstRegistrationDate != null)
            {
                salesCar.FirstRegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(firstRegistrationDate);
            }
            string firstRegistrationDateGengou = "";
            if (salesCar.FirstRegistrationDateWareki != null) {
                firstRegistrationDateGengou = salesCar.FirstRegistrationDateWareki.Gengou.ToString();
            }
            ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), firstRegistrationDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), firstRegistrationDateGengou, false);


            //------------------------
            // 有効期間の満了する日
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.ExpireDate != null)
            {
                salesCar.ExpireDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.ExpireDate);
            }
            
            string expireDateGengou = "";

            if (salesCar.ExpireDateWareki != null)
            {
                expireDateGengou = salesCar.ExpireDateWareki.Gengou.ToString();
            }

            ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), expireDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), expireDateGengou, false);

            //------------------------
            // 納車日
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.SalesDate != null)
            {
                salesCar.SalesDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.SalesDate);
            }
            
            string salesDateGengou = "";
            if (salesCar.SalesDateWareki != null)
            {
                salesDateGengou = salesCar.SalesDateWareki.Gengou.ToString();
            }
            ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), salesDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), salesDateGengou, false);

            //------------------------
            // 点検日
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.InspectionDate != null)
            {
                salesCar.InspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.InspectionDate);
            }

            string inspectionDateGengou = "";

            if (salesCar.InspectionDateWareki != null)
            {
                inspectionDateGengou = salesCar.InspectionDateWareki.Gengou.ToString();
            }
            ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), inspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), inspectionDateGengou, false);

            //------------------------
            // 次回点検日
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.NextInspectionDate != null)
            {
                salesCar.NextInspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.NextInspectionDate);
            }
            
            string nextInspectionDateGengou = "";

            if (salesCar.NextInspectionDateWareki != null)
            {
                nextInspectionDateGengou = salesCar.NextInspectionDateWareki.Gengou.ToString();
            }

            ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), nextInspectionDateGengou, false); //Mod 2018/06/22 arc yano #3891

            //Mod 2021/08/02 yano #4097 コメントアウト
            //Add 2014/09/08 arc amii 年式入力対応 #3076 年式の桁数が4桁を超えていた場合、4桁で表示する
            //if (CommonUtils.DefaultString(salesCar.ManufacturingYear).Length > 10)
            //{
            //    salesCar.ManufacturingYear = salesCar.ManufacturingYear.Substring(0, 10);
            //}

            //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
            //Add 2017/01/16 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            //最後に金額の変動があった画面が車両仕入画面でなければメッセージ表示
            /*if (!carPurchase.LastEditScreen.Equals(LAST_EDIT_PURCHASE))
            {
                switch (carPurchase.LastEditScreen)
                {
                    case "001":
                        carPurchase.LastEditMessage = "車両伝票から仕入価格(総額)、自税充当、リサイクルの各金額が変更されました。";
                        break;
                    case "002":
                        carPurchase.LastEditMessage = "査定画面から仕入価格(総額)、自税充当、リサイクルの各金額が変更されました。";
                        break;
                    default:
                        carPurchase.LastEditMessage = "";
                        break;
                }

            }else{
                carPurchase.LastEditMessage = "";
            }*/
			//Add 2017/03/06 arc yano #3640 車両仕入金額のチェック
            int ret = 0;

            decimal calcPrice = carPurchase.VehiclePrice + (carPurchase.AuctionFeePrice ?? 0m) + (carPurchase.CarTaxAppropriatePrice ?? 0m) +
                                (carPurchase.RecyclePrice ?? 0m) + carPurchase.OthersPrice + carPurchase.MetallicPrice + carPurchase.OptionPrice +
                                carPurchase.FirmPrice + carPurchase.DiscountPrice + carPurchase.EquipmentPrice + carPurchase.RepairPrice;

            if (carPurchase.Amount != calcPrice)
            {
                ret = 1;
            }

            decimal calcTotalAmount = (carPurchase.VehicleAmount ?? 0m) + (carPurchase.AuctionFeeAmount ?? 0m) + (carPurchase.CarTaxAppropriateAmount ?? 0m) +
                                 (carPurchase.RecycleAmount ?? 0m) + (carPurchase.OthersAmount ?? 0m) + (carPurchase.MetallicAmount ?? 0m) + (carPurchase.OptionAmount ?? 0m) +
                                (carPurchase.FirmAmount ?? 0m) + (carPurchase.DiscountAmount ?? 0m) + (carPurchase.EquipmentAmount ?? 0m) + (carPurchase.RepairAmount ?? 0m);

            if (carPurchase.TotalAmount != calcTotalAmount)
            {
                ret = 1;
            }

            if (ret == 1)
            {
                carPurchase.CalcResultMessage = "(z)の金額が(a)～(k)の金額の合計と一致しません。一致させるには再計算を行う必要があります。";
            }
            
            //キャンセルデータ取得（有れば入力不可にする）
            CarPurchase checkPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
            
            if (checkPurchase != null)
            {
                if(!string.IsNullOrEmpty(checkPurchase.CancelCarPurchaseId)){
                    ViewData["CancelFlag"] = "1"; //入力不可にするフラグ
                }

                // 仕入計上済み、且つ締め処理済みの月の場合、変更不可フラグを立てる
                if (!string.IsNullOrEmpty(checkPurchase.PurchaseStatus) && checkPurchase.PurchaseStatus.Equals("002")
                    && !new InventoryScheduleDao(db).IsClosedInventoryMonth(checkPurchase.DepartmentCode, checkPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ViewData["ClosedMonth"] = "1";
                }
            }

            if (carPurchase != null)
            {
                if (!string.IsNullOrEmpty(carPurchase.PurchaseStatus) && carPurchase.PurchaseStatus.Equals("003"))
                {
                    ViewData["CancelFlag"] = "1"; //入力不可にするフラグ
                }
            }

        }
        #endregion

        #region 検索処理
        /// <summary>
        /// 車両仕入テーブル検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両仕入テーブル検索結果リスト</returns>
        private PaginatedList<CarPurchase> GetSearchResultList(FormCollection form) {

            CarPurchaseDao carPurchaseDao = new CarPurchaseDao(db);
            CarPurchase carPurchaseCondition = new CarPurchase();
            carPurchaseCondition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.SlipDateFrom = CommonUtils.StrToDateTime(form["SlipDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.SlipDateTo = CommonUtils.StrToDateTime(form["SlipDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseDateFrom = CommonUtils.StrToDateTime(form["PurchaseDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseDateTo = CommonUtils.StrToDateTime(form["PurchaseDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseStatus = form["PurchaseStatus"];
            carPurchaseCondition.Department = new Department();
            carPurchaseCondition.Department.DepartmentCode = form["DepartmentCode"];
            carPurchaseCondition.Supplier = new Supplier();
            carPurchaseCondition.Supplier.SupplierCode = form["SupplierCode"];
            carPurchaseCondition.SalesCar = new SalesCar();
            carPurchaseCondition.SalesCar.SalesCarNumber = form["SalesCarNumber"];
            carPurchaseCondition.SalesCar.Vin = form["Vin"];
            carPurchaseCondition.SalesCar.CarGrade = new CarGrade();
            carPurchaseCondition.SalesCar.CarGrade.Car = new Car();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand = new Brand();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker = new Maker();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName = form["MakerName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.CarName = form["CarName"];
            carPurchaseCondition.SalesCar.CarGrade.CarGradeName = form["CarGradeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carPurchaseCondition.DelFlag = form["DelFlag"];
            }
            return carPurchaseDao.GetListByCondition(carPurchaseCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region 新規登録時のValidation
        private void ValidateForInsert(CarPurchase carPurchase) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(carPurchase.SalesCar.Vin);
            if (list != null && list.Count > 0 && !carPurchase.RegetVin) {
                ModelState.AddModelError("SalesCar.Vin", "車台番号:" + carPurchase.SalesCar.Vin + "は既に登録されています");
                ViewData["ErrorSalesCar"] = list;
            }
            for (int i = 0; i < list.Count(); i++) {
                if (list[i].CarStatus != null && !list[i].CarStatus.Equals("006") && !list[i].CarStatus.Equals("")) {
                    ModelState.AddModelError("SalesCar.Vin", list[i].Vin + " (" + (i + 1) + ")の在庫ステータスが「" + list[i].c_CarStatus.Name + "」のため管理番号の再取得が出来ません");
                }
            }
        }
    #endregion

        #region Validation
        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="carPurchase">車両仕入データ</param>
        /// <returns>車両仕入データ</returns>
        /// <history>
        /// 2021/08/02 yano #4097【グレードマスタ入力】年式の保存の拡張機能（クオーター対応）
        /// 2020/11/17 yano #4065 【車両伝票入力】環境性能割・マスタの設定値が不正の場合の対応
        /// 2019/02/08 yano #3965 WE版新システム対応（Web.configによる処理の分岐) we版の場合は、下取仕入で伝票番号の入力チェックを行わない
        /// 2018/10/25 yano #3947 車両仕入入力　入力項目（古物取引時相手の確認方法）の追加
        /// 2018/08/28 yano #3922 車両管理表(タマ表) 機能改善② 入庫区分の必須チェックを追加
        /// 2018/06/22 arc yano #3891 元号対応 DBから取得するように変更
        /// 2018/06/06 arc yano #3883 タマ表改善 中古車仕入の際に初年度登録を必須項目に
        /// 2018/04/26 arc yano #3816 車両査定入力　管理番号にN/Aが入ってしまう
        /// </history>
        private CarPurchase ValidateCarPurchase(CarPurchase carPurchase, FormCollection form)
        {

            SalesCar salesCar = carPurchase.SalesCar;

            //Add 2018/08/28 yano #3922
            if (string.IsNullOrEmpty(carPurchase.CarPurchaseType))
            {
                ModelState.AddModelError("CarPurchaseType", MessageUtils.GetMessage("E0001", "入庫区分"));
            }
            // 必須チェック(数値・日付項目は属性チェックも兼ねる)
            if (string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            if (string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "仕入担当者"));
            }
            if (string.IsNullOrEmpty(carPurchase.SupplierCode)) {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "仕入先"));
            }
            if (carPurchase.PurchaseDate == null)
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0003", "入庫日"));
            }
            if (string.IsNullOrEmpty(carPurchase.PurchaseLocationCode)) {
                ModelState.AddModelError("PurchaseLocationCode", MessageUtils.GetMessage("E0001", "入庫ロケーション"));
            }
           
            //Add 2018/06/06 arc yano #3883
            //中古車の仕入計上時、初年度登録は必須とする
            if ((!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("saveStock")) && salesCar.NewUsedType.Equals("U"))
            {
                salesCar.FirstRegistrationDateWareki.Day = 1;

                if (salesCar.FirstRegistrationDateWareki.IsNull)
                {
                    ModelState.AddModelError("salesCar.FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0001", "初度登録年月"));
                    ModelState.AddModelError("salesCar.FirstRegistrationDateWareki.Month", "");
                }
            }

            //Mod 2020/11/17 yano #4065 下に同じ処理があるため、コメントアウト
            //salesCar.FirstRegistrationDateWareki.Day = 1;
            //if (!salesCar.FirstRegistrationDateWareki.IsNull)
            //{
            //    if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db)) //Mod 2018/06/22 arc yano #3891
            //    //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki))
            //    {
            //        ModelState.AddModelError("FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "初度登録年月"));
            //    }
            //}

            // Add 2014/09/11 arc amii 車検案内チェック対応 車検案内=「否」の場合、備考欄の必須チェックを行う
            if (!CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002"))
            {
                if (salesCar.InspectGuidFlag.Equals("002") && string.IsNullOrEmpty(salesCar.InspectGuidMemo))
                {
                    ModelState.AddModelError("SalesCar.InspectGuidMemo", MessageUtils.GetMessage("E0001", "車検案内発送備考欄"));
                }
            }

            // Add 2016/02/05 ARC Mikami #3212 備考欄をTextAreaに変更、文字数チェックを行う。
            if (!string.IsNullOrEmpty(carPurchase.Memo)) {
                if ( carPurchase.Memo.Length > 100 ) {
                    ModelState.AddModelError("Memo", "備考は100文字以内で入力して下さい（改行は2文字分とみなされます）");
                    if (ModelState["Memo"].Errors.Count > 1) {
                        ModelState["Memo"].Errors.RemoveAt(0);
                    }
                }
            }
            if (!ModelState.IsValidField("VehiclePrice")) {
                ModelState.AddModelError("VehiclePrice", MessageUtils.GetMessage("E0002", new string[] { "車両本体価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["VehiclePrice"].Errors.Count > 1) {
                    ModelState["VehiclePrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("VehicleTax")) {
                ModelState.AddModelError("VehicleTax", MessageUtils.GetMessage("E0002", new string[] { "車両本体価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["VehicleTax"].Errors.Count > 1) {
                    ModelState["VehicleTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("VehicleAmount")) {
                ModelState.AddModelError("VehicleAmount", MessageUtils.GetMessage("E0002", new string[] { "車両本体価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["VehicleAmount"].Errors.Count > 1) {
                    ModelState["VehicleAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("AuctionFeePrice")) {
                ModelState.AddModelError("AuctionFeePrice", MessageUtils.GetMessage("E0004", new string[] { "オークション落札料(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["AuctionFeePrice"].Errors.Count > 1) {
                    ModelState["AuctionFeePrice"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("AuctionFeeTax")) {
                ModelState.AddModelError("AuctionFeeTax", MessageUtils.GetMessage("E0004", new string[] { "オークション落札料(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["AuctionFeeTax"].Errors.Count > 1) {
                    ModelState["AuctionFeeTax"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("AuctionFeeAmount")) {
                ModelState.AddModelError("AuctionFeeAmount", MessageUtils.GetMessage("E0004", new string[] { "オークション落札料(税込)", "10桁以内の整数のみ" }));
                if (ModelState["AuctionFeeAmount"].Errors.Count > 1) {
                    ModelState["AuctionFeeAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicPrice")) {
                ModelState.AddModelError("MetallicPrice", MessageUtils.GetMessage("E0002", new string[] { "メタリック価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["MetallicPrice"].Errors.Count > 1) {
                    ModelState["MetallicPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicTax")) {
                ModelState.AddModelError("MetallicTax", MessageUtils.GetMessage("E0002", new string[] { "メタリック価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["MetallicTax"].Errors.Count > 1) {
                    ModelState["MetallicTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicAmount")) {
                ModelState.AddModelError("MetallicAmount", MessageUtils.GetMessage("E0002", new string[] { "メタリック価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["MetallicAmount"].Errors.Count > 1) {
                    ModelState["MetallicAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionPrice")) {
                ModelState.AddModelError("OptionPrice", MessageUtils.GetMessage("E0002", new string[] { "オプション価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["OptionPrice"].Errors.Count > 1) {
                    ModelState["OptionPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionTax")) {
                ModelState.AddModelError("OptionTax", MessageUtils.GetMessage("E0002", new string[] { "オプション価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["OptionTax"].Errors.Count > 1) {
                    ModelState["OptionTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionAmount")) {
                ModelState.AddModelError("OptionAmount", MessageUtils.GetMessage("E0002", new string[] { "オプション価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["OptionAmount"].Errors.Count > 1) {
                    ModelState["OptionAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmPrice")) {
                ModelState.AddModelError("FirmPrice", MessageUtils.GetMessage("E0002", new string[] { "ファーム価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["FirmPrice"].Errors.Count > 1) {
                    ModelState["FirmPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmTax")) {
                ModelState.AddModelError("FirmTax", MessageUtils.GetMessage("E0002", new string[] { "ファーム価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["FirmTax"].Errors.Count > 1) {
                    ModelState["FirmTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmAmount")) {
                ModelState.AddModelError("FirmAmount", MessageUtils.GetMessage("E0002", new string[] { "ファーム価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["FirmAmount"].Errors.Count > 1) {
                    ModelState["FirmAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountPrice")) {
                ModelState.AddModelError("DiscountPrice", MessageUtils.GetMessage("E0002", new string[] { "ディスカウント価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["DiscountPrice"].Errors.Count > 1) {
                    ModelState["DiscountPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountTax")) {
                ModelState.AddModelError("DiscountTax", MessageUtils.GetMessage("E0002", new string[] { "ディスカウント価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["DiscountTax"].Errors.Count > 1) {
                    ModelState["DiscountTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountAmount")) {
                ModelState.AddModelError("DiscountAmount", MessageUtils.GetMessage("E0002", new string[] { "ディスカウント価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["DiscountAmount"].Errors.Count > 1) {
                    ModelState["DiscountAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("EquipmentPrice")) {
                ModelState.AddModelError("EquipmentPrice", MessageUtils.GetMessage("E0002", new string[] { "加装価格", "10桁以内の整数のみ" }));
                if (ModelState["EquipmentPrice"].Errors.Count > 1) {
                    ModelState["EquipmentPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("RepairPrice")) {
                ModelState.AddModelError("RepairPrice", MessageUtils.GetMessage("E0002", new string[] { "加修価格", "10桁以内の整数のみ" }));
                if (ModelState["RepairPrice"].Errors.Count > 1) {
                    ModelState["RepairPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersPrice")) {
                ModelState.AddModelError("OthersPrice", MessageUtils.GetMessage("E0002", new string[] { "その他価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["OthersPrice"].Errors.Count > 1) {
                    ModelState["OthersPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersTax")) {
                ModelState.AddModelError("OthersTax", MessageUtils.GetMessage("E0002", new string[] { "その他価格(消費税)", "10桁以内の整数のみ" }));
                if (ModelState["OthersTax"].Errors.Count > 1) {
                    ModelState["OthersTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersAmount")) {
                ModelState.AddModelError("OthersAmount", MessageUtils.GetMessage("E0002", new string[] { "その他価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["OthersAmount"].Errors.Count > 1) {
                    ModelState["OthersAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("Amount")) {
                ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "仕入価格(税抜)", "10桁以内の整数のみ" }));
                if (ModelState["Amount"].Errors.Count > 1) {
                    ModelState["Amount"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("TaxAmount")) {
                ModelState.AddModelError("TaxAmount", MessageUtils.GetMessage("E0002", new string[] { "消費税", "10桁以内の整数のみ" }));
                if (ModelState["TaxAmount"].Errors.Count > 1) {
                    ModelState["TaxAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("TotalAmount")) {
                ModelState.AddModelError("TotalAmount", MessageUtils.GetMessage("E0002", new string[] { "仕入価格(税込)", "10桁以内の整数のみ" }));
                if (ModelState["TotalAmount"].Errors.Count > 1) {
                    ModelState["TotalAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("SlipDate")) {
                ModelState.AddModelError("SlipDate", MessageUtils.GetMessage("E0005", "仕入日"));
                if (ModelState["SlipDate"].Errors.Count > 1) {
                    ModelState["SlipDate"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("CarTaxAppropriateAmount")) {
                ModelState.AddModelError("CarTaxAppropriateAmount", MessageUtils.GetMessage("E0004", new string[] { "自税充当", "10桁以内の整数のみ" }));
                if (ModelState["CarTaxAppropriateAmount"].Errors.Count > 1) {
                    ModelState["CarTaxAppropriateAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("RecycleAmount")) {
                ModelState.AddModelError("RecycleAmount", MessageUtils.GetMessage("E0004", new string[] { "リサイクル", "10桁以内の整数のみ" }));
                if (ModelState["RecycleAmount"].Errors.Count > 1) {
                    ModelState["RecycleAmount"].Errors.RemoveAt(0);
                }
            }
            //if (!ViewData["PurchaseStatus"].Equals("002")) {
            if (string.IsNullOrEmpty(salesCar.CarGradeCode))
            {
                ModelState.AddModelError("SalesCar.CarGradeCode", MessageUtils.GetMessage("E0001", "グレード"));
            }
            else //Add 2018/04/26 arc yano #3816 グレードコードが入力されていた場合はマスタチェックを行う 
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("SalesCar.CarGradeCode", "車両グレードマスタに登録されていません。マスタ登録を行ってから再度実行して下さい");
                }
            }

            if (string.IsNullOrEmpty(salesCar.NewUsedType)) {
                ModelState.AddModelError("SalesCar.NewUsedType", MessageUtils.GetMessage("E0001", "新中区分"));
            }
            if (string.IsNullOrEmpty(salesCar.Vin)) {
                ModelState.AddModelError("SalesCar.Vin", MessageUtils.GetMessage("E0001", "車台番号"));
            }
            //}

            // 属性チェック
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
                if (!ModelState.IsValidField("SalesCar.SalesPrice")) {
                    ModelState.AddModelError("SalesCar.SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の10桁以内の整数のみ" }));
                }
                //Add 2014/09/08 arc amii 年式入力対応 #3076 年式の入力チェック(4桁数値以外はエラー)を追加
                if (!ModelState.IsValidField("SalesCar.ManufacturingYear"))
                {
                    ModelState.AddModelError("SalesCar.ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "年式", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
                }
                //if (!ModelState.IsValidField("SalesCar.IssueDate")) {
                //    ModelState.AddModelError("SalesCar.IssueDate", MessageUtils.GetMessage("E0005", "発行日"));
                //}
                //if (!ModelState.IsValidField("SalesCar.RegistrationDate")) {
                //    ModelState.AddModelError("SalesCar.RegistrationDate", MessageUtils.GetMessage("E0005", "登録日"));
                //}
                if (!ModelState.IsValidField("SalesCar.Capacity")) {
                    ModelState.AddModelError("SalesCar.Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.MaximumLoadingWeight")) {
                    ModelState.AddModelError("SalesCar.MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.CarWeight")) {
                    ModelState.AddModelError("SalesCar.CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.TotalCarWeight")) {
                    ModelState.AddModelError("SalesCar.TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.Length")) {
                    ModelState.AddModelError("SalesCar.Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.Width")) {
                    ModelState.AddModelError("SalesCar.Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.Height")) {
                    ModelState.AddModelError("SalesCar.Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.FFAxileWeight")) {
                    ModelState.AddModelError("SalesCar.FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.FRAxileWeight")) {
                    ModelState.AddModelError("SalesCar.FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.RFAxileWeight")) {
                    ModelState.AddModelError("SalesCar.RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.RRAxileWeight")) {
                    ModelState.AddModelError("SalesCar.RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.Displacement")) {
                    ModelState.AddModelError("SalesCar.Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数のみ" }));
                }
                //if (!ModelState.IsValidField("SalesCar.ExpireDate")) {
                //    ModelState.AddModelError("SalesCar.ExpireDate", MessageUtils.GetMessage("E0005", "有効期限"));
                //}
                if (!ModelState.IsValidField("SalesCar.Mileage")) {
                    ModelState.AddModelError("SalesCar.Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
                }
                if (!ModelState.IsValidField("SalesCar.CarTax")) {
                    ModelState.AddModelError("SalesCar.CarTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税種別割", "正の10桁以内の整数のみ" }));     //Mod 2019/09/04 yano #4011
                }
                if (!ModelState.IsValidField("SalesCar.CarWeightTax")) {
                    ModelState.AddModelError("SalesCar.CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "自動車重量税", "正の10桁以内の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.AcquisitionTax")) {
                    ModelState.AddModelError("SalesCar.AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税環境性能割", "正の10桁以内の整数のみ" }));　//Mod 2019/09/04 yano #4011
                }
                if (!ModelState.IsValidField("SalesCar.CarLiabilityInsurance")) {
                    ModelState.AddModelError("SalesCar.CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "自賠責保険料", "正の10桁以内の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.RecycleDeposit")) {
                    ModelState.AddModelError("SalesCar.RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の10桁以内の整数のみ" }));
                }
                if (!ModelState.IsValidField("SalesCar.InspectionDate")) {
                    ModelState.AddModelError("SalesCar.InspectionDate", MessageUtils.GetMessage("E0005", "点検日"));
                }
                if (!ModelState.IsValidField("SalesCar.NextInspectionDate")) {
                    ModelState.AddModelError("SalesCar.NextInspectionDate", MessageUtils.GetMessage("E0005", "次回点検日"));
                }
                if (!ModelState.IsValidField("SalesCar.ProductionDate")) {
                    ModelState.AddModelError("SalesCar.ProductionDate", MessageUtils.GetMessage("E0005", "生産日"));
                }
                if (!ModelState.IsValidField("SalesCar.ApprovedCarWarrantyDateFrom")) {
                    ModelState.AddModelError("SalesCar.ApprovedCarWarrantyDateFrom", MessageUtils.GetMessage("E0005", "認定中古車保証期間(開始)"));
                }
                if (!ModelState.IsValidField("SalesCar.ApprovedCarWarrantyDateTo")) {
                    ModelState.AddModelError("SalesCar.ApprovedCarWarrantyDateTo", MessageUtils.GetMessage("E0005", "認定中古車保証期間(終了)"));
                }
 //           }

            // フォーマットチェック
            if (ModelState.IsValidField("VehicleAmount")) {
                if (!Regex.IsMatch(carPurchase.VehicleAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("VehicleAmount", MessageUtils.GetMessage("E0002", new string[] { "車両本体価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("AuctionFeeAmount")) {
                if (carPurchase.AuctionFeeAmount!=null && !Regex.IsMatch(carPurchase.AuctionFeeAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("AuctionFeeAmount", MessageUtils.GetMessage("E0004", new string[] { "オークション落札料", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("MetallicPrice")) {
                if (!Regex.IsMatch(carPurchase.MetallicPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("MetallicPrice", MessageUtils.GetMessage("E0002", new string[] { "メタリック価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("OptionPrice")) {
                if (!Regex.IsMatch(carPurchase.OptionPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("OptionPrice", MessageUtils.GetMessage("E0002", new string[] { "オプション価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("FirmPrice")) {
                if (!Regex.IsMatch(carPurchase.FirmPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("FirmPrice", MessageUtils.GetMessage("E0002", new string[] { "ファーム価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("DiscountPrice")) {
                if (!Regex.IsMatch(carPurchase.DiscountPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("DiscountPrice", MessageUtils.GetMessage("E0002", new string[] { "ディスカウント価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("EquipmentPrice")) {
                if (!Regex.IsMatch(carPurchase.EquipmentPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("EquipmentPrice", MessageUtils.GetMessage("E0002", new string[] { "加装価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RepairPrice")) {
                if (!Regex.IsMatch(carPurchase.RepairPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("RepairPrice", MessageUtils.GetMessage("E0002", new string[] { "加修価格", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("OthersPrice")) {
                if (!Regex.IsMatch(carPurchase.OthersPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("OthersPrice", MessageUtils.GetMessage("E0002", new string[] { "その他価格", "10桁以内の整数のみ" }));
                }
            }
            /*if (ModelState.IsValidField("TaxAmount")) {
                if (!Regex.IsMatch(carPurchase.TaxAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("TaxAmount", MessageUtils.GetMessage("E0002", new string[] { "消費税", "10桁以内の整数のみ" }));
                }
            }*/
            if (ModelState.IsValidField("CarTaxAppropriatePrice")) {
                if (carPurchase.CarTaxAppropriatePrice != null && !Regex.IsMatch(carPurchase.CarTaxAppropriatePrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("CarTaxAppropriatePrice", MessageUtils.GetMessage("E0004", new string[] { "自税充当", "10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("RecyclePrice")) {
                if (carPurchase.RecyclePrice != null && !Regex.IsMatch(carPurchase.RecyclePrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("RecyclePrice", MessageUtils.GetMessage("E0004", new string[] { "リサイクル", "10桁以内の整数のみ" }));
                }
            } 
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
                if (ModelState.IsValidField("SalesCar.SalesPrice") && salesCar.SalesPrice != null) {
                    if (!Regex.IsMatch(salesCar.SalesPrice.ToString(), @"^\d{1,10}$")) {
                        ModelState.AddModelError("SalesCar.SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "販売価格", "正の10桁以内の整数のみ" }));
                    }
                }

            //Mod 2021/08/02 yano #4097 入力可能文字フォーマットを変更(正の整数４桁のみ→正の整数4桁、または正の整数4桁かつ少数2桁以内
            //Add 2014/09/08 arc amii 年式入力対応 #3076 年式の入力チェック(4桁数値以外はエラー)を追加
            if (ModelState.IsValidField("SalesCar.ManufacturingYear") && CommonUtils.DefaultString(salesCar.ManufacturingYear).Equals("") == false)
            {
              if (((!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}\.\d{1,2}$"))
                && (!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}$")))
              )
              {
                  ModelState.AddModelError("SalesCar.ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "年式", "正の整数4桁または、正の整数4桁かつ少数2桁以内" }));
              }
            }

            if (ModelState.IsValidField("SalesCar.Mileage") && salesCar.Mileage != null) {
                if ((Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("SalesCar.Mileage", MessageUtils.GetMessage("E0004", new string[] { "走行距離", "正の整数10桁以内かつ小数2桁以内" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.CarTax") && salesCar.CarTax != null) {
                if (!Regex.IsMatch(salesCar.CarTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税種別割", "正の10桁以内の整数のみ" })); //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("SalesCar.CarWeightTax") && salesCar.CarWeightTax != null) {
                if (!Regex.IsMatch(salesCar.CarWeightTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "自動車重量税", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.AcquisitionTax") && salesCar.AcquisitionTax != null) {
                if (!Regex.IsMatch(salesCar.AcquisitionTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "自動車税環境性能割", "正の10桁以内の整数のみ" }));//Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("SalesCar.CarLiabilityInsurance") && salesCar.CarLiabilityInsurance != null) {
                if (!Regex.IsMatch(salesCar.CarLiabilityInsurance.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "自賠責保険料", "正の10桁以内の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RecycleDeposit") && salesCar.RecycleDeposit != null) {
                if (!Regex.IsMatch(salesCar.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "リサイクル預託金", "正の10桁以内の整数のみ" }));
                }
            }
            //管理番号手入力の場合、自動採番と被る要素がないか
            if (ModelState.IsValidField("SalesCar.SalesCarNumber") && !string.IsNullOrEmpty(salesCar.SalesCarNumber)) {
                SalesCar existsCar = new SalesCarDao(db).GetByKey(salesCar.SalesCarNumber);
                //車両マスタ新規登録の場合
                if (existsCar == null && !new SerialNumberDao(db).CanUseSalesCarNumber(salesCar.SalesCarNumber)) {
                    ModelState.AddModelError("SalesCar.SalesCarNumber", "指定された管理番号は自動採番で使用する範囲に含まれるため使用できません");
                }
            }

            //入庫日が棚卸締め処理済みの月内であればエラー
            if (carPurchase.PurchaseDate != null) {
                // Mod 2015/04/15 arc yano　仮締中変更可能なユーザの場合、仮締めの場合は、変更可能とする
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "月次締め処理が終了しているので指定された入庫日では仕入処理ができません");
                }
            }

            // 値チェック
            if (ModelState.IsValidField("Amount")) {
                if (decimal.Compare(carPurchase.Amount, -9999999999m) < 0 || decimal.Compare(carPurchase.Amount, 9999999999m) > 0) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0018", "仕入金額"));
                }
            }
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
            if (ModelState.IsValidField("SalesCar.Capacity") && salesCar.Capacity != null) {
                if (salesCar.Capacity < 0) {
                    ModelState.AddModelError("SalesCar.Capacity", MessageUtils.GetMessage("E0004", new string[] { "定員", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.MaximumLoadingWeight") && salesCar.MaximumLoadingWeight != null) {
                if (salesCar.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("SalesCar.MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "最大積載量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.CarWeight") && salesCar.CarWeight != null) {
                if (salesCar.CarWeight < 0) {
                    ModelState.AddModelError("SalesCar.CarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.TotalCarWeight") && salesCar.TotalCarWeight != null) {
                if (salesCar.TotalCarWeight < 0) {
                    ModelState.AddModelError("SalesCar.TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "車両総重量", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Length") && salesCar.Length != null) {
                if (salesCar.Length < 0) {
                    ModelState.AddModelError("SalesCar.Length", MessageUtils.GetMessage("E0004", new string[] { "長さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Width") && salesCar.Width != null) {
                if (salesCar.Width < 0) {
                    ModelState.AddModelError("SalesCar.Width", MessageUtils.GetMessage("E0004", new string[] { "幅", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Height") && salesCar.Height != null) {
                if (salesCar.Height < 0) {
                    ModelState.AddModelError("SalesCar.Height", MessageUtils.GetMessage("E0004", new string[] { "高さ", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.FFAxileWeight") && salesCar.FFAxileWeight != null) {
                if (salesCar.FFAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.FRAxileWeight") && salesCar.FRAxileWeight != null) {
                if (salesCar.FRAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "前後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RFAxileWeight") && salesCar.RFAxileWeight != null) {
                if (salesCar.RFAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後前軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RRAxileWeight") && salesCar.RRAxileWeight != null) {
                if (salesCar.RRAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "後後軸重", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Displacement") && salesCar.Displacement != null) {
                if (salesCar.Displacement < 0) {
                    ModelState.AddModelError("SalesCar.Displacement", MessageUtils.GetMessage("E0004", new string[] { "排気量", "正の整数のみ" }));
                }
            }

            // 和暦→西暦の変換チェック
            if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki)) {
                ModelState.AddModelError("SalesCar.IssueDateWareki.Year", MessageUtils.GetMessage("E0021", "発行日"));
            }
            if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki)) {
                ModelState.AddModelError("SalesCar.RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "登録年月日／交付年月日"));
            }

            salesCar.FirstRegistrationDateWareki.Day = 1;
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {
                if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db)) {    //Mod 2018/06/22 arc yano #3891
                //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki)) {
                    ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "初度登録年月"));
                    ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Month", "");
                }
                //Add 2020/11/17 yano #4065
                else
                {
                    DateTime? FirstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);

                    //初度登録年月が本日より30日以降の日付で設定されていた場合
                    if (FirstRegistrationDate != null && (((DateTime)(FirstRegistrationDate ?? DateTime.Today).Date - DateTime.Today.Date).TotalDays > 30))
                    {
                        ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Year", "初度登録年月に未来の日付は設定できません");
                        ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Month", "");
                    }
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki)) {
                ModelState.AddModelError("ExpireDateWareki.Year", MessageUtils.GetMessage("E0021", "有効期限"));
            }
            if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki)) {
                ModelState.AddModelError("InspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "点検日"));
            }
            if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki)) {
                ModelState.AddModelError("NextInspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "次回点検日"));
            }
            
            //仕入計上時、かつ、入庫区分が「下取車」の場合のみのチェック

            //2019/02/08 yano #3965
            if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")) //WE版以外
            {
                //Add 2017/02/01 arc nakayama #3701_車両仕入　入庫区分を「下取車」以外で仕入計上後、保存を行うとシステムエラー
                if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                {

                    //伝票番号の必須チェック
                    if (string.IsNullOrEmpty(carPurchase.SlipNumber))
                    {
                        ModelState.AddModelError("SlipNumber", "下取車を仕入計上する場合、伝票番号は入力必須です");
                        return carPurchase;
                    }

                    //伝票の有無確認
                    CarSalesHeader CarSlip = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                    if (CarSlip == null)
                    {
                        ModelState.AddModelError("SlipNumber", "入力された伝票は存在していません");
                        return carPurchase;
                    }

                    //仕入先が伝票の顧客と一致しているか
                    if (!CarSlip.CustomerCode.Equals(carPurchase.SupplierCode))
                    {
                        ModelState.AddModelError("SupplierCode", "入力された仕入先が伝票の顧客と一致していません");
                    }

                    //仕入計上する車両が、該当の伝票に存在しているかチェックする
                    List<TradeInVinList> VinList = new List<TradeInVinList>();

                    //車両伝票内全ての下取車の車台番号を取得
                    for (int i = 1; i <= 3; i++)
                    {
                        object vin = CommonUtils.GetModelProperty(CarSlip, "TradeInVin" + i);
                        if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                        {
                            TradeInVinList TradeInVin = new TradeInVinList();
                            TradeInVin.Vin = vin.ToString();
                            VinList.Add(TradeInVin);
                        }
                    }

                    //下取車の車台番号が一つも存在しなかった場合
                    if (VinList == null || VinList.Count <= 0)
                    {
                        ModelState.AddModelError("SlipNumber", "入力されている車両伝票に下取車が存在していません");
                    }
                    else
                    {
                        //下取車の車台番号はあるが、一致するものがなかった場合
                        var ret = VinList.Where(x => x.Vin == salesCar.Vin).FirstOrDefault();

                        if (ret == null)
                        {
                            ModelState.AddModelError("salesCar_Vin", "入力されている車台番号が車両伝票の下取車に存在していません");
                        }
                    }
                }
            }
//            }

            //キャンセル以外の場合
            if (form["update"].Equals("1") && (string.IsNullOrWhiteSpace(form["action"]) || !form["action"].Equals("CancelStock")))
            {
                if (carPurchase.CancelDate != null)
                {
                    ModelState.AddModelError("CancelDate", "仕入れキャンセルを行わない場合は、キャンセル日を登録できません");
                }

                if (!string.IsNullOrEmpty(carPurchase.CancelMemo))
                {
                    ModelState.AddModelError("CancelMemo", "仕入れキャンセルを行わない場合は、キャンセルメモを登録できません");
                }
            }


            //Add 2018/10/25 yano #3947
            //------------------------------
            //古物取引時の確認方法
            //------------------------------
            if (salesCar.NewUsedType.Equals("U"))   //中古車の場合
            {
                //仕入計上、または仕入済後の保存
                if ((!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("saveStock")) ||
                     (!string.IsNullOrWhiteSpace(carPurchase.PurchaseStatus) && carPurchase.PurchaseStatus.Equals("002"))
                    )
                {    
                    if (salesCar.ConfirmDriverLicense.Equals(false) && salesCar.ConfirmCertificationSeal.Equals(false) && string.IsNullOrWhiteSpace(salesCar.ConfirmOther))
                    {
                        ModelState.AddModelError("salesCar.ConfirmDriverLicense", MessageUtils.GetMessage("E0001", "古物取引時の確認方法"));
                        ModelState.AddModelError("salesCar.ConfirmCertificationSeal", "");
                        ModelState.AddModelError("salesCar.ConfirmOther", "");
                    }
                }
            }
  
            return carPurchase;
        }
        #endregion

        #region コピー処理
        /// <summary>
        /// 車両仕入テーブルコピーデータ作成
        /// </summary>
        /// <param name="carPurchaseId">コピー元車両仕入ID</param>
        /// <returns>車両仕入テーブルモデルクラス</returns>
        /// <history>
        /// 2018/06/25 arc yano #3895 車両仕入入力　管理番号の非活性化で見つけた不具合の修正
        /// 2018/03/20 arc yano #3871 車両仕入入力　コピー作成時、車両本体価格、仕入価格の計算を行うと金額が消える
        /// </history>
        private CarPurchase CreateCopyCarPurchase(Guid carPurchaseId) {

            CarPurchase ret = new CarPurchase();
            ret.SalesCar = new SalesCar();

            CarPurchase src = new CarPurchaseDao(db).GetByKey(carPurchaseId);

            ret.PurchaseDate = src.PurchaseDate;
            ret.SupplierCode = src.SupplierCode;
            ret.PurchaseLocationCode = src.PurchaseLocationCode;
            ret.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            ret.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            ret.VehiclePrice = src.VehiclePrice;
            ret.VehicleTax = src.VehicleTax;
            ret.VehicleAmount = src.VehicleAmount;
            ret.MetallicPrice = src.MetallicPrice;
            ret.MetallicTax = src.MetallicTax;
            ret.MetallicAmount = src.MetallicAmount;
            ret.OptionPrice = src.OptionPrice;
            ret.OptionTax = src.OptionTax;
            ret.OptionAmount = src.OptionAmount;
            ret.FirmPrice = src.FirmPrice;
            ret.FirmTax = src.FirmTax;
            ret.FirmAmount = src.FirmAmount;
            ret.DiscountPrice = src.DiscountPrice;
            ret.DiscountTax = src.DiscountTax;
            ret.DiscountAmount = src.DiscountAmount;
            ret.EquipmentPrice = src.EquipmentPrice;
            ret.RepairPrice = src.RepairPrice;
            ret.OthersPrice = src.OthersPrice;
            ret.OthersTax = src.OthersTax;
            ret.OthersAmount = src.OthersAmount;
            ret.Amount = src.Amount;
            ret.TaxAmount = src.TaxAmount;
            ret.TotalAmount = src.TotalAmount;
            ret.CarTaxAppropriatePrice = src.CarTaxAppropriatePrice;
            ret.CarTaxAppropriateAmount = src.CarTaxAppropriateAmount;
            ret.CarPurchaseType = src.CarPurchaseType;
            
            //消費税率
            //Add 2018/06/25 arc yano #3895
            //Add 2014/06/24 arc yano 日付⇔税率矛盾バグ対応
            //消費税IDが未設定であれば、当日日付で消費税ID取得
            if (src.ConsumptionTaxId == null)
            {
                ret.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
            }
            else
            {
                ret.ConsumptionTaxId = src.ConsumptionTaxId;
            }
            //ret.ConsumptionTaxId = src.ConsumptionTaxId;
            ret.Rate = (src.Rate ?? int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(ret.ConsumptionTaxId)));        //Add 2018/03/20 arc yano #3871
            ret.RecyclePrice = src.RecyclePrice;
            ret.RecycleAmount = src.RecycleAmount;
            ret.AuctionFeePrice = src.AuctionFeePrice;
            ret.AuctionFeeTax = src.AuctionFeeTax;
            ret.AuctionFeeAmount = src.AuctionFeeAmount;
            ret.LastEditScreen = src.LastEditScreen;
            ret.SalesCar.CarGradeCode = src.SalesCar.CarGradeCode;
            ret.SalesCar.NewUsedType = src.SalesCar.NewUsedType;
            ret.SalesCar.ColorType = src.SalesCar.ColorType;
            ret.SalesCar.ExteriorColorCode = src.SalesCar.ExteriorColorCode;
            ret.SalesCar.ExteriorColorName = src.SalesCar.ExteriorColorName;
            ret.SalesCar.ChangeColor = src.SalesCar.ChangeColor;
            ret.SalesCar.InteriorColorCode = src.SalesCar.InteriorColorCode;
            ret.SalesCar.InteriorColorName = src.SalesCar.InteriorColorName;
            ret.SalesCar.ManufacturingYear = src.SalesCar.ManufacturingYear;
            ret.SalesCar.Steering = src.SalesCar.Steering;
            ret.SalesCar.SalesPrice = src.SalesCar.SalesPrice;
            ret.SalesCar.IssueDate = src.SalesCar.IssueDate;
            ret.SalesCar.MorterViecleOfficialCode = src.SalesCar.MorterViecleOfficialCode;
            ret.SalesCar.RegistrationNumberType = src.SalesCar.RegistrationNumberType;
            ret.SalesCar.RegistrationNumberKana = src.SalesCar.RegistrationNumberKana;
            ret.SalesCar.RegistrationNumberPlate = src.SalesCar.RegistrationNumberPlate;
            ret.SalesCar.RegistrationDate = src.SalesCar.RegistrationDate;
            ret.SalesCar.FirstRegistrationYear = src.SalesCar.FirstRegistrationYear;
            ret.SalesCar.CarClassification = src.SalesCar.CarClassification;
            ret.SalesCar.Usage = src.SalesCar.Usage;
            ret.SalesCar.UsageType = src.SalesCar.UsageType;
            ret.SalesCar.Figure = src.SalesCar.Figure;
            ret.SalesCar.MakerName = src.SalesCar.MakerName;
            ret.SalesCar.Capacity = src.SalesCar.Capacity;
            ret.SalesCar.MaximumLoadingWeight = src.SalesCar.MaximumLoadingWeight;
            ret.SalesCar.CarWeight = src.SalesCar.CarWeight;
            ret.SalesCar.TotalCarWeight = src.SalesCar.TotalCarWeight;
            ret.SalesCar.Vin = src.SalesCar.Vin;
            ret.SalesCar.Length = src.SalesCar.Length;
            ret.SalesCar.Width = src.SalesCar.Width;
            ret.SalesCar.Height = src.SalesCar.Height;
            ret.SalesCar.FFAxileWeight = src.SalesCar.FFAxileWeight;
            ret.SalesCar.FRAxileWeight = src.SalesCar.FRAxileWeight;
            ret.SalesCar.RFAxileWeight = src.SalesCar.RFAxileWeight;
            ret.SalesCar.RRAxileWeight = src.SalesCar.RRAxileWeight;
            ret.SalesCar.ModelName = src.SalesCar.ModelName;
            ret.SalesCar.EngineType = src.SalesCar.EngineType;
            ret.SalesCar.Displacement = src.SalesCar.Displacement;
            ret.SalesCar.Fuel = src.SalesCar.Fuel;
            ret.SalesCar.ModelSpecificateNumber = src.SalesCar.ModelSpecificateNumber;
            ret.SalesCar.ClassificationTypeNumber = src.SalesCar.ClassificationTypeNumber;
            ret.SalesCar.ExpireType = src.SalesCar.ExpireType;
            ret.SalesCar.ExpireDate = src.SalesCar.ExpireDate;
            ret.SalesCar.Mileage = src.SalesCar.Mileage;
            ret.SalesCar.MileageUnit = src.SalesCar.MileageUnit;
            ret.SalesCar.Memo = src.SalesCar.Memo;
            ret.SalesCar.InspectionDate = src.SalesCar.InspectionDate;
            ret.SalesCar.NextInspectionDate = src.SalesCar.NextInspectionDate;
            ret.SalesCar.UsVin = src.SalesCar.UsVin;
            ret.SalesCar.MakerWarranty = src.SalesCar.MakerWarranty;
            ret.SalesCar.RecordingNote = src.SalesCar.RecordingNote;
            ret.SalesCar.ProductionDate = src.SalesCar.ProductionDate;
            ret.SalesCar.ReparationRecord = src.SalesCar.ReparationRecord;
            ret.SalesCar.Tire = src.SalesCar.Tire;
            ret.SalesCar.KeyCode = src.SalesCar.KeyCode;
            ret.SalesCar.AudioCode = src.SalesCar.AudioCode;
            ret.SalesCar.Import = src.SalesCar.Import;
            ret.SalesCar.Guarantee = src.SalesCar.Guarantee;
            ret.SalesCar.Instructions = src.SalesCar.Instructions;
            ret.SalesCar.Recycle = src.SalesCar.Recycle;
            ret.SalesCar.RecycleTicket = src.SalesCar.RecycleTicket;
            ret.SalesCar.CouponPresence = src.SalesCar.CouponPresence;
            ret.SalesCar.Light = src.SalesCar.Light;
            ret.SalesCar.Aw = src.SalesCar.Aw;
            ret.SalesCar.Aero = src.SalesCar.Aero;
            ret.SalesCar.Sr = src.SalesCar.Sr;
            ret.SalesCar.Cd = src.SalesCar.Cd;
            ret.SalesCar.Md = src.SalesCar.Md;
            ret.SalesCar.NaviType = src.SalesCar.NaviType;
            ret.SalesCar.NaviEquipment = src.SalesCar.NaviEquipment;
            ret.SalesCar.NaviDashboard = src.SalesCar.NaviDashboard;
            ret.SalesCar.SeatColor = src.SalesCar.SeatColor;
            ret.SalesCar.SeatType = src.SalesCar.SeatType;
            ret.SalesCar.Memo1 = src.SalesCar.Memo1;
            ret.SalesCar.Memo2 = src.SalesCar.Memo2;
            ret.SalesCar.Memo3 = src.SalesCar.Memo3;
            ret.SalesCar.Memo4 = src.SalesCar.Memo4;
            ret.SalesCar.Memo5 = src.SalesCar.Memo5;
            ret.SalesCar.Memo6 = src.SalesCar.Memo6;
            ret.SalesCar.Memo7 = src.SalesCar.Memo7;
            ret.SalesCar.Memo8 = src.SalesCar.Memo8;
            ret.SalesCar.Memo9 = src.SalesCar.Memo9;
            ret.SalesCar.Memo10 = src.SalesCar.Memo10;
            ret.SalesCar.DeclarationType = src.SalesCar.DeclarationType;
            ret.SalesCar.AcquisitionReason = src.SalesCar.AcquisitionReason;
            ret.SalesCar.TaxationTypeCarTax = src.SalesCar.TaxationTypeCarTax;
            ret.SalesCar.TaxationTypeAcquisitionTax = src.SalesCar.TaxationTypeAcquisitionTax;
            ret.SalesCar.CarTax = src.SalesCar.CarTax;
            ret.SalesCar.CarWeightTax = src.SalesCar.CarWeightTax;
            ret.SalesCar.CarLiabilityInsurance = src.SalesCar.CarLiabilityInsurance;
            ret.SalesCar.AcquisitionTax = src.SalesCar.AcquisitionTax;
            ret.SalesCar.RecycleDeposit = src.SalesCar.RecycleDeposit;
            ret.SalesCar.EraseRegist = src.SalesCar.EraseRegist;
            return ret;
        }
        #endregion

        #region データ編集
        /// <summary>
        /// 車両仕入テーブル追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carPurchase">車両仕入データ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両仕入テーブルモデルクラス</returns>
        /// <history>
        /// 2020/11/27 yano #4072 原動機型式入力エリアの拡張 原動機型式の設定文字数以上入らない処理の廃止
        /// 2019/02/08 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 財務価格は仕入総額からリサイクル料金を除いた価格とする
        /// 2018/06/06 arc yano #3883 タマ表改善 仕入価格で財務価格を更新する
        /// 2017/11/14 arc yano  #3811 車両伝票－下取車の入金予定残高更新不整合 下取車の仕入時に入金予定の再作成を行うように修正
        /// </history>
        private CarPurchase EditCarPurchaseForInsert(CarPurchase carPurchase, FormCollection form) {

            // 車両情報編集
            if (string.IsNullOrEmpty(carPurchase.SalesCar.SalesCarNumber)) {
                string companyCode = "N/A";
                try { companyCode = new CarGradeDao(db).GetByKey(carPurchase.SalesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
                carPurchase.SalesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, carPurchase.SalesCar.NewUsedType);
            }//else {
            //    carPurchase.SalesCar.SalesCarNumber = carPurchase.SalesCarNumber;
            //}
            if (form["action"].Equals("saveStock")) {
                carPurchase.SalesCar.CarStatus = "001";
                carPurchase.SalesCar.LocationCode = carPurchase.PurchaseLocationCode;

                //Add 2019/02/08 yano #3965
                if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                { //WE版以外
                    if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                    {
                        //下取車で伝票番号がある場合は、入金データを更新する
                        //Add 2017/02/14 arc nakayama #3704_車両仕入　仕入計上前のデータを保存すると入金実績が作成される
                        UpdateTradeCarJournal(carPurchase, form);
                        CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                        CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                    }
                }
            }
            // Vinの変換　MOD 2014/10/16 arc ishii 
            //carPurchase.SalesCar.Vin = CommonUtils.abc123ToHankaku(carPurchase.SalesCar.Vin);
            carPurchase.SalesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.Vin));
            //ADD 2014/10/22 arc ishii Vinが20文字以上の場合左から20文字目まで抜き出す
            if (carPurchase.SalesCar.Vin.Length > 20)
            {
               carPurchase.SalesCar.Vin = carPurchase.SalesCar.Vin.Substring(0, 20);
            }
            carPurchase.SalesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.CreateDate = DateTime.Now;
            carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            carPurchase.SalesCar.DelFlag = "0";
            //EngineType変換　ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.EngineType));

            //Mod Mod 2020/11/27 yano #4072
            //Add 2015/03/26 arc iijima Null落ち対応のため判定追加
            //ADD 2014/10/22 arc ishii EngineTypeが10文字以上の場合左から10文字目まで抜き出す
            //if ((!string.IsNullOrEmpty(carPurchase.SalesCar.EngineType)) && (carPurchase.SalesCar.EngineType.Length > 10))
            //{
            //    carPurchase.SalesCar.EngineType = carPurchase.SalesCar.EngineType.Substring(0, 10);
            //}


            // 古い管理番号を履歴テーブルに移動
            if (carPurchase.RegetVin) {
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(carPurchase.SalesCar.Vin);
                foreach (var item in salesCarList) {
                    CommonUtils.CopyToSalesCarHistory(db, item);
                    item.DelFlag = "1";
                }
            }

            // 車両仕入情報編集
            carPurchase.CarPurchaseId = Guid.NewGuid();
            
            if (form["action"].Equals("saveStock")) {
                carPurchase.PurchaseStatus = "002";
            } else {
                carPurchase.PurchaseStatus = "001";
            }
            carPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.CreateDate = DateTime.Now;
            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.LastUpdateDate = DateTime.Now;
            carPurchase.DelFlag = "0";

            //Mod 2018/08/28 yano #3922
            //Add 2018/06/06 arc yano #3883
            ////財務価格をリサイクル料金を除いた全ての金額で更新
            //if (carPurchase.SalesCar != null && !string.IsNullOrWhiteSpace(carPurchase.SalesCar.NewUsedType) && carPurchase.SalesCar.NewUsedType.Equals("U"))
            //{

                decimal purchaseprice = 0m;

                purchaseprice = carPurchase.VehiclePrice +                              //車両本体価格(税抜)
                                (carPurchase.AuctionFeePrice ?? 0m) +                   //オークション落札量(税抜)
                                (carPurchase.CarTaxAppropriatePrice ?? 0m) +            //自税充当(税抜)
                                carPurchase.OthersPrice +                               //その他価格(税抜)
                                carPurchase.MetallicPrice +                             //メタリック価格(税抜)
                                carPurchase.OptionPrice +                               //オプション価格(税抜)     
                                carPurchase.FirmPrice +                                 //ファーム価格(税抜)
                                carPurchase.DiscountPrice +                             //ディスカウント価格(税抜)
                                carPurchase.EquipmentPrice +                            //加装価格(税抜)
                                carPurchase.RepairPrice;                                //加修価格(税抜)


                if (purchaseprice > 0)
                {
                    carPurchase.FinancialAmount = purchaseprice;
                }
                else
                {
                    carPurchase.FinancialAmount = 1m;
                }


                //仕入価格(税込)からリサイクル料を除いた仕入価格(税抜)を算出 現状未使用
                ////現時点の税率を初期で設定
                //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(carPurchase.ConsumptionTaxId);

                ////仕入データに消費税率IDが設定されている場合はそちらを使う
                //if (rate != null)
                //{
                //    taxrate = rate.Rate;
                //}

                //decimal value = (carPurchase.TotalAmount ?? 0) - (carPurchase.RecycleAmount ?? 0);

                //if (value > 0)
                //{
                //    //税込金額から税抜金額を計算(端数切り捨て)
                //    carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                //}
                //else
                //{
                //    if ((carPurchase.TotalAmount ?? 0) > 0)
                //    {
                //        carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax((carPurchase.TotalAmount ?? 0), taxrate, 1);
                //    }
                //    else
                //    {
                //        carPurchase.FinancialAmount = 1m;
                //    }
                //}
                
                //carPurchase.FinancialAmount = carPurchase.Amount;   
            //}
            
            return carPurchase;
        }

        /// <summary>
        /// 車両仕入テーブル更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carPurchase">車両仕入データ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>車両仕入テーブルモデルクラス</returns>
        /// <history>
        /// 2021/08/09 yano #4086【車両仕入入力】下取車を変更した際の入金実績リスト削除漏れ対応 引数追加
        /// 2020/11/27 yano #4072 原動機型式入力エリアの拡張　設定文字数以上入らない処理の廃止
        /// 2019/02/08 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 財務価格は仕入総額からリサイクル料金を除いた価格とする
        /// 2018/06/06 arc yano #3883 タマ表改善 仕入価格で財務価格を更新する
        /// 2017/11/14 arc yano  #3811 車両伝票－下取車の入金予定残高更新不整合 下取車の仕入時に入金予定の再作成を行うように修正
        /// </history>
        private CarPurchase EditCarPurchaseForUpdate(CarPurchase carPurchase, FormCollection form, string prevSlipNumber, string prevVin) {//Mod 2021/08/09 yano #4086

            // 車両情報編集
            if (!ViewData["PurchaseStatus"].Equals("002"))
            {
                if (form["action"].Equals("saveStock"))
                {
                    carPurchase.SalesCar.CarStatus = "001";
                    carPurchase.SalesCar.LocationCode = carPurchase.PurchaseLocationCode;


                    //Add  2019/02/08 yano #3965
                    if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                    { //WE版以外
                        //Del 2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止 
                        //Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
                        if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                        {
                            //下取車で伝票番号がある場合は、入金データを更新する
                            UpdateTradeCarJournal(carPurchase, form);
                            CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);

                            CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                        }
                    }

                }
                carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            }
            else
            {
                //Add  2019/02/08 yano #3965
                if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                {
                    //Add 2021/08/09 yano #4086
                    //編集前が下取車の場合
                    if(!string.IsNullOrWhiteSpace(prevSlipNumber) && !string.IsNullOrWhiteSpace(prevVin))
                    {
                        //伝票番号変更または仕入区分が下取車→下取車以外の場合
                        if( !prevSlipNumber.Equals(carPurchase.SlipNumber) || (!string.IsNullOrWhiteSpace(carPurchase.CarPurchaseType) && !carPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN)))
                        {
                            //既存の下取入金実績を削除する
                            DeleteJournal(prevSlipNumber, prevVin);
                        }
                    }

                    //Add 2017/02/01 arc nakayama #3701_車両仕入　入庫区分を「下取車」以外で仕入計上後、保存を行うとシステムエラー
                    if (!string.IsNullOrEmpty(carPurchase.SlipNumber) && (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN)))
                    {
                        //Add 2017/02/14 arc nakayama #3704_車両仕入　仕入計上前のデータを保存すると入金実績が作成される
                        UpdateTradeCarJournal(carPurchase, form);

                        //Add 2017/11/14 arc yano  #3811
                        CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                        CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                    }
                }
            }

            // 車両仕入情報編集
            if (form["action"].Equals("saveStock")) {
                carPurchase.PurchaseStatus = "002";

            }

            //Mod 2018/08/28 yano #3922
            //Add 2018/06/06 arc yano #3883
            //財務価格をリサイクル料金を除いた全ての金額で更新
            
            //if(carPurchase.SalesCar != null && !string.IsNullOrWhiteSpace(carPurchase.SalesCar.NewUsedType) && carPurchase.SalesCar.NewUsedType.Equals("U"))
            //{

            //ファイナンスデータ取込済でない場合は更新する
            if (string.IsNullOrWhiteSpace(carPurchase.FinancialAmountLocked) || !carPurchase.FinancialAmountLocked.Equals("1"))
            {
                decimal purchaseprice = 0m;

                purchaseprice = carPurchase.VehiclePrice +                              //車両本体価格(税抜)
                                (carPurchase.AuctionFeePrice ?? 0m) +                   //オークション落札量(税抜)
                                (carPurchase.CarTaxAppropriatePrice ?? 0m) +            //自税充当(税抜)
                                carPurchase.OthersPrice +                               //その他価格(税抜)
                                carPurchase.MetallicPrice +                             //メタリック価格(税抜)
                                carPurchase.OptionPrice +                               //オプション価格(税抜)     
                                carPurchase.FirmPrice +                                 //ファーム価格(税抜)
                                carPurchase.DiscountPrice +                             //ディスカウント価格(税抜)
                                carPurchase.EquipmentPrice +                            //加装価格(税抜)
                                carPurchase.RepairPrice;                                //加修価格(税抜)


                if (purchaseprice > 0)
                {
                    carPurchase.FinancialAmount = purchaseprice;
                }
                else
                {
                    carPurchase.FinancialAmount = 1;
                }
            }

           
                //仕入総額から取得する場合(現状未使用)
                //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(carPurchase.ConsumptionTaxId);

                ////仕入データに消費税率IDが設定されている場合はそちらを使う
                //if (rate != null)
                //{
                //    taxrate = rate.Rate;
                //}

                //decimal value = (carPurchase.TotalAmount ?? 0) - (carPurchase.RecycleAmount ?? 0);

                //if (value > 0)
                //{
                //    //税込金額から税抜金額を計算(端数切り捨て)
                //    carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                //}
                //else
                //{
                //    if ((carPurchase.TotalAmount ?? 0) > 0)
                //    {
                //        carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax((carPurchase.TotalAmount ?? 0), taxrate, 1);
                //    }
                //    else
                //    {
                //        carPurchase.FinancialAmount = 1m;
                //    }
                //}

                //carPurchase.FinancialAmount = carPurchase.Amount;   
            //}
            
            // Vinの変換　ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.Vin));
            //ADD 2014/10/22 arc ishii Vinが20文字以上の場合左から20文字目まで抜き出す
            if (carPurchase.SalesCar.Vin.Length > 20)
            {
                carPurchase.SalesCar.Vin = carPurchase.SalesCar.Vin.Substring(0, 20);
            }
            carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            carPurchase.SalesCar.DelFlag = "0";
            //EngineType変換　ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.EngineType));

            //Add 2015/03/26 arc iijima Null落ち対応のため判定追加

            //Mod 2020/11/27 yano #4072
            //ADD 2014/10/22 arc ishii EngineTypeが10文字以上の場合左から10文字目まで抜き出す
            //if ((!string.IsNullOrEmpty(carPurchase.SalesCar.EngineType)) && (carPurchase.SalesCar.EngineType.Length > 10))
            //{
            //    carPurchase.SalesCar.EngineType = carPurchase.SalesCar.EngineType.Substring(0, 10);
            //}

            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.LastUpdateDate = DateTime.Now;

            return carPurchase;
        }
        #endregion

        /*#region 下取車の入金実績を作成する
        private void CreateTradeCarJournal(CarPurchase carPurchase)
        {
            string CustomerClaimCode = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber).CustomerCode;
            string JournalDate = string.Format("{0:yyyy/MM/dd}", carPurchase.PurchaseDate);

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }


            Journal NewJournal = new Journal();
            NewJournal.JournalId = Guid.NewGuid();
            NewJournal.JournalType = "001";
            NewJournal.DepartmentCode = carPurchase.DepartmentCode;
            NewJournal.CustomerClaimCode = CustomerClaimCode;
            NewJournal.SlipNumber = carPurchase.SlipNumber;
            NewJournal.JournalDate = DateTime.Parse(JournalDate);
            NewJournal.AccountType = "013";//下取
            NewJournal.AccountCode = carAccount.AccountCode;
            NewJournal.Amount = carPurchase.TotalAmount ?? 0;
            NewJournal.Summary = null;
            NewJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;
            NewJournal.CreateDate = DateTime.Now;
            NewJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;
            NewJournal.LastUpdateDate = DateTime.Now;
            NewJournal.DelFlag = "0";
            NewJournal.ReceiptPlanFlag = "1";
            NewJournal.TransferFlag = null;
            NewJournal.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode,false).OfficeCode;
            NewJournal.CashAccountCode = null;
            NewJournal.PaymentKindCode = null;
            NewJournal.CreditReceiptPlanId = null;

            db.Journal.InsertOnSubmit(NewJournal);
            db.SubmitChanges();
        }
        #endregion*/

        #region 下取車の入金実績を更新する
        /// <summary>
        /// 下取車の入金実績を更新する
        /// </summary>
        /// <param name="carPurchase">車両仕入データ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2021/08/16 yano #4001 下取仕入の入金実績が作成されない。
        /// 2018/07/31 yano.hiroki #3919 下取車仕入後の車台番号変更自の対応
        /// 2017/11/14 arc yano  #3811 車両伝票－下取車の入金予定残高更新不整合 実績取得のメソッドの変更
        /// 2017/10/04 arc yano #3777 2台目以降の下取車を仕入すると入金実績が作成されない 実績に車台番号を保持するフィールドを追加
        /// 2017/02/14 arc nakayama #3704_車両仕入　仕入計上前のデータを保存すると入金実績が作成される
        /// </history>
        private void UpdateTradeCarJournal(CarPurchase carPurchase, FormCollection form)
        {
            string CustomerClaimCode = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber).CustomerCode;
            string JournalDate = string.Format("{0:yyyy/MM/dd}", carPurchase.PurchaseDate);

            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }

            //Mod 2021/08/16 yano #4001
            //Mod 2017/10/04 arc yano #3777
            //Mod 2018/07/31 yano.hiroki #3919
            //Mod 2017/11/14 arc yano  #3811
            //下取車の車台番号の絞込みの追加   既存の入金実績を取得するときは、基本的には車両マスタの車台番号ではなく、車両仕入データの車台番号で絞込するが、
            //新規作成時の場合など、車両仕入データの車台番号(CarPurchase.Vin)が空欄の場合は車両マスタから取得する

            string tradevin = !string.IsNullOrWhiteSpace(carPurchase.Vin) ? carPurchase.Vin : carPurchase.SalesCar.Vin;

            Journal JournalData = new JournalDao(db).GetTradeJournal(carPurchase.SlipNumber, "013", tradevin).FirstOrDefault();
            //Journal JournalData = new JournalDao(db).GetTradeJournal(carPurchase.SlipNumber, "013", carPurchase.Vin).FirstOrDefault();
       

            if (JournalData != null && form["update"].Equals("1"))
            {
                JournalData.Amount = carPurchase.TotalAmount ?? 0;
                JournalData.SlipNumber = carPurchase.SlipNumber;
                JournalData.DepartmentCode = carPurchase.DepartmentCode;
                JournalData.CustomerClaimCode = CustomerClaimCode;
                JournalData.JournalDate = DateTime.Parse(JournalDate);
                JournalData.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode, false).OfficeCode;
                JournalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                JournalData.LastUpdateDate = DateTime.Now;
                JournalData.TradeVin = carPurchase.SalesCar.Vin;        //Add2017/10/04 arc yano #3777
            }
            else
            {
                //Add 2017/02/14 arc nakayama #3704
                //入金実績がなくても、作成するのは「仕入計上」が押されたとき、または、仕入済のとき
                if (form["action"].Equals("saveStock") || (carPurchase.PurchaseStatus.Equals("002") && JournalData == null))
                {
                    Journal NewJournal = new Journal();
                    NewJournal.JournalId = Guid.NewGuid();
                    NewJournal.JournalType = "001";
                    NewJournal.DepartmentCode = carPurchase.DepartmentCode;
                    NewJournal.CustomerClaimCode = CustomerClaimCode;
                    NewJournal.SlipNumber = carPurchase.SlipNumber;
                    NewJournal.JournalDate = DateTime.Parse(JournalDate);
                    NewJournal.AccountType = "013";//下取
                    NewJournal.AccountCode = carAccount.AccountCode;
                    NewJournal.Amount = carPurchase.TotalAmount ?? 0;
                    NewJournal.Summary = null;
                    NewJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    NewJournal.CreateDate = DateTime.Now;
                    NewJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    NewJournal.LastUpdateDate = DateTime.Now;
                    NewJournal.DelFlag = "0";
                    NewJournal.ReceiptPlanFlag = "1";
                    NewJournal.TransferFlag = null;
                    NewJournal.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode, false).OfficeCode;
                    NewJournal.CashAccountCode = null;
                    NewJournal.PaymentKindCode = null;
                    NewJournal.CreditReceiptPlanId = null;
                    NewJournal.TradeVin = carPurchase.SalesCar.Vin;        //Add2017/10/04 arc yano #3777

                    db.Journal.InsertOnSubmit(NewJournal);
                }
            }

            db.SubmitChanges();
        }

        /// 既存の下取車の入金実績を削除する
        /// </summary>
        /// <param name="prevSlipNumber">伝票番号</param>
        /// <param name="prevVin">車台番号</param>
        /// <history>
        /// 2021/08/09 yano #4086【車両仕入入力】下取車を変更した際の入金実績リスト削除漏れ対応 新規作成
        /// </history>
        private void DeleteJournal(string prevSlipNumber, string prevVin)
        {

            List<Journal> JournalList = new List<Journal>();
        
            JournalList = new JournalDao(db).GetListBySlipNumber(prevSlipNumber).Where(x => x.AccountType.Equals(ACCOUNTTYPE_TRADEIN) && x.TradeVin.Equals(prevVin)).ToList();
            
            foreach(Journal rec in JournalList)
            {
                rec.DelFlag = "1";
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now; ;
            }
           
            db.SubmitChanges();
        }

        #endregion

        #region 下取車の入金予定を再作成する。残債があれば残債の入金予定も作成する
        /// <summary>
        /// 下取車の入金予定を再作成する。残債があれば残債の入金予定も作成する
        /// </summary>
        /// <param name="carPurchase">車両仕入データ(登録内容)</param>
        /// <param name="header">車両伝票データ</param>
        /// <param name="targetCarPurchase">車両仕入データ</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 下取、下取残債の入金予定再作成処理の復活(但し連動廃止のため、車両伝票、車両査定の情報は更新しない)
        //  2017/03/28 arc nakayama #3739_車両伝票・車両査定・車両仕入の連動廃止
        /// </history>
        private void CreateTradeReceiptPlan(CarSalesHeader header, CarPurchase targetCarPurchase)
        {
            //Mod 2017/11/14 arc yano #3811
            //科目データ取得
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                return;
            }
   
            //---------------------------------------------
            //既存の入金予定の削除
            //---------------------------------------------
            //対象車両の下取、残債の入金予定を削除する
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => (x.ReceiptType == "012" || x.ReceiptType == "013") && !string.IsNullOrWhiteSpace(x.TradeVin) && x.TradeVin.Equals(targetCarPurchase.SalesCar.Vin)).ToList();
            foreach (var d in delList)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //-------------------------------------------
            //対象車両の入金予定の再作成
            //-------------------------------------------
            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()) && vin.ToString().Equals(targetCarPurchase.SalesCar.Vin))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                    {
                        TradeInAmount = varTradeInAmount.ToString();
                    }

                    var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                    {
                        TradeInRemainDebt = varTradeInRemainDebt.ToString();
                    }

                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //下取の入金額
                    decimal JournalDebtAmount = 0; //残債の入金額


                    //下取の入金額取得
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //残債の入金額取得
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013"; //下取
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //☆計算
                    if (TradePlan.ReceivableBalance == 0m)
                    {
                        TradePlan.CompleteFlag = "1";
                    }
                    else
                    {
                        TradePlan.CompleteFlag = "0";
                    }
                    TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.CreateDate = DateTime.Now;
                    TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.LastUpdateDate = DateTime.Now;
                    TradePlan.DelFlag = "0";
                    TradePlan.Summary = "";
                    TradePlan.JournalDate = null;
                    TradePlan.DepositFlag = "0";
                    TradePlan.PaymentKindCode = "";
                    TradePlan.CommissionRate = null;
                    TradePlan.CommissionAmount = null;
                    TradePlan.CreditJournalId = "";
                    TradePlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                    db.ReceiptPlan.InsertOnSubmit(TradePlan);

                    //残債があった場合残債分の入金予定をマイナスで作成する
                    if (!string.IsNullOrEmpty(TradeInRemainDebt))
                    {
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //残債は経理に振り替え
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012"; //残債
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //計算
                        if (RemainDebtPlan.ReceivableBalance == 0m)
                        {
                            RemainDebtPlan.CompleteFlag = "1";
                        }
                        else
                        {
                            RemainDebtPlan.CompleteFlag = "0";
                        }
                        RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.CreateDate = DateTime.Now;
                        RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.LastUpdateDate = DateTime.Now;
                        RemainDebtPlan.DelFlag = "0";
                        RemainDebtPlan.Summary = "";
                        RemainDebtPlan.JournalDate = null;
                        RemainDebtPlan.DepositFlag = "0";
                        RemainDebtPlan.PaymentKindCode = "";
                        RemainDebtPlan.CommissionRate = null;
                        RemainDebtPlan.CommissionAmount = null;
                        RemainDebtPlan.CreditJournalId = "";
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                    //db.SubmitChanges();
                }
            }

            /*
            //車両伝票の下取情報を更新

            //納車済みの時は車両伝票に反映しない
            //Add 2017/01/13 arc nakayama #3689_【考慮漏れ】納車済後に下取車の仕入を行うと、納車済みの伝票に金額が反映されてしまう
            if (!header.SalesOrderStatus.Equals("004") && !header.SalesOrderStatus.Equals("005"))
            {
                //既存の入金予定を削除
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => (x.ReceiptType == "012" || x.ReceiptType == "013")).ToList();
                foreach (var d in delList)
                {
                    //Add 2016/09/05 arc nakayama #3630_【製造】車両売掛金対応
                    //残債と下取以外の入金予定
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                //科目データ取得
                Account carAccount = new AccountDao(db).GetByUsageType("CR");
                if (carAccount == null)
                {
                    ModelState.AddModelError("", "科目設定が正しくありません。システム管理者に連絡して下さい。");
                    return;
                }

                for (int i = 1; i <= 3; i++)
                {
                    object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                    if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                    {
                        string TradeInAmount = "0";
                        string TradeInRemainDebt = "0";
                        string TradeInRecycleAmount = "0";
                        string TradeInCarTaxAppropriateAmount = "0";
                        decimal PlanAmount = 0;
                        decimal PlanRemainDebt = 0;

                        //査定の該当車両の場合は査定の金額をセットする
                        if (targetCarPurchase.SalesCar.Vin == vin.ToString())
                        {
                            TradeInAmount = targetCarPurchase.TotalAmount.ToString();
                            var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                            if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                            {
                                TradeInRemainDebt = varTradeInRemainDebt.ToString();
                            }

                            if (targetCarPurchase.CarTaxAppropriateAmount != null)
                            {
                                TradeInCarTaxAppropriateAmount = targetCarPurchase.CarTaxAppropriateAmount.ToString();
                            }

                            if (targetCarPurchase.RecycleAmount != null)
                            {
                                TradeInRecycleAmount = targetCarPurchase.RecycleAmount.ToString();
                            }

                            PlanAmount = targetCarPurchase.TotalAmount ?? 0m;
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }
                        else
                        {
                            var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                            if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                            {
                                TradeInAmount = varTradeInAmount.ToString();
                            }

                            var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                            if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                            {
                                TradeInRemainDebt = varTradeInRemainDebt.ToString();
                            }

                            var varCarTaxAppropriateAmount = CommonUtils.GetModelProperty(header, "TradeInUnexpiredCarTax" + i);
                            if (varCarTaxAppropriateAmount != null && !string.IsNullOrEmpty(varCarTaxAppropriateAmount.ToString()))
                            {
                                TradeInCarTaxAppropriateAmount = varCarTaxAppropriateAmount.ToString();
                            }

                            var varTradeInRecycleAmount = CommonUtils.GetModelProperty(header, "TradeInRecycleAmount" + i);
                            if (varTradeInRecycleAmount != null && !string.IsNullOrEmpty(varTradeInRecycleAmount.ToString()))
                            {
                                TradeInRecycleAmount = varTradeInRecycleAmount.ToString();
                            }

                            PlanAmount = decimal.Parse(TradeInAmount);
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }

                        //車両伝票の金額を更新
                        bool editflag = false;
                        switch (i)
                        {
                            case 1:
                                if (header.TradeInAmount1 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt1 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount1 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax1 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount1 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt1 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation1 = header.TradeInAmount1 ?? 0m - header.TradeInRemainDebt1 ?? 0m;//下取車総額(下取価格 - 下取残債金額)
                                //Add 2017/01/10 arc nakayama #3688_車両仕入　リサイクル金額が税込だけ連動される
                                header.TradeInRecycleAmount1 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax1 = decimal.Parse(TradeInCarTaxAppropriateAmount);
                                
                                break;

                            case 2:
                                if (header.TradeInAmount2 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt2 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount2 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax2 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount2 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt2 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation2 = header.TradeInAmount2 ?? 0m - header.TradeInRemainDebt2 ?? 0m;//下取車総額(下取価格 - 下取残債金額)                            
                                //Add 2017/01/10 arc nakayama #3688_車両仕入　リサイクル金額が税込だけ連動される
                                header.TradeInRecycleAmount2 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax2 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 3:
                                if (header.TradeInAmount3 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt3 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount3 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax3 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount3 = decimal.Parse(TradeInAmount);//下取価格
                                header.TradeInRemainDebt3 = decimal.Parse(TradeInRemainDebt);//下取車残債
                                header.TradeInAppropriation3 = header.TradeInAmount3 ?? 0m - header.TradeInRemainDebt3 ?? 0m;//下取車総額(下取価格 - 下取残債金額)
                                //Add 2017/01/10 arc nakayama #3688_車両仕入　リサイクル金額が税込だけ連動される
                                header.TradeInRecycleAmount3 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax3 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            default:
                                break;
                        }

                        if (editflag)
                        {
                            header.LastEditScreen = LAST_EDIT_PURCHASE;
                            targetCarPurchase.LastEditScreen = "000";
                        }
                        else
                        {
                            targetCarPurchase.LastEditScreen = "000";
                        }

                        //下取車合計
                        header.TradeInTotalAmount = header.TradeInAmount1 ?? 0m + header.TradeInAmount2 ?? 0m + header.TradeInAmount3 ?? 0m;
                        //残債合計
                        header.TradeInRemainDebtTotalAmount = header.TradeInRemainDebt1 ?? 0m + header.TradeInRemainDebt2 ?? 0m + header.TradeInRemainDebt3 ?? 0m;
                        //下取車充当金総額合計
                        header.TradeInAppropriationTotalAmount = header.TradeInAppropriation1 ?? 0m + header.TradeInAppropriation2 ?? 0m + header.TradeInAppropriation3 ?? 0m;


                        decimal JournalAmount = 0; //下取の入金額
                        decimal JournalDebtAmount = 0; //残債の入金額

                        //下取の入金額取得
                        Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                        if (JournalData != null)
                        {
                            JournalAmount = JournalData.Amount;
                        }

                        //残債の入金額取得
                        Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                        if (JournalData2 != null)
                        {
                            JournalDebtAmount = JournalData2.Amount;
                        }

                        ReceiptPlan TradePlan = new ReceiptPlan();
                        TradePlan.ReceiptPlanId = Guid.NewGuid();
                        TradePlan.DepartmentCode = header.DepartmentCode;
                        TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                        TradePlan.CustomerClaimCode = header.CustomerCode;
                        TradePlan.SlipNumber = header.SlipNumber;
                        TradePlan.ReceiptType = "013"; //下取
                        TradePlan.ReceiptPlanDate = null;
                        TradePlan.AccountCode = carAccount.AccountCode;
                        TradePlan.Amount = decimal.Parse(TradeInAmount);
                        TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //☆計算
                        if (TradePlan.ReceivableBalance == 0m)
                        {
                            TradePlan.CompleteFlag = "1";
                        }
                        else
                        {
                            TradePlan.CompleteFlag = "0";
                        }
                        TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        TradePlan.CreateDate = DateTime.Now;
                        TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        TradePlan.LastUpdateDate = DateTime.Now;
                        TradePlan.DelFlag = "0";
                        TradePlan.Summary = "";
                        TradePlan.JournalDate = null;
                        TradePlan.DepositFlag = "0";
                        TradePlan.PaymentKindCode = "";
                        TradePlan.CommissionRate = null;
                        TradePlan.CommissionAmount = null;
                        TradePlan.CreditJournalId = "";

                        db.ReceiptPlan.InsertOnSubmit(TradePlan);

                        //残債があった場合残債分の入金予定をマイナスで作成する
                        if (!string.IsNullOrEmpty(TradeInRemainDebt))
                        {
                            ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                            RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                            RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //残債は経理に振り替え
                            RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                            RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                            RemainDebtPlan.SlipNumber = header.SlipNumber;
                            RemainDebtPlan.ReceiptType = "012"; //残債
                            RemainDebtPlan.ReceiptPlanDate = null;
                            RemainDebtPlan.AccountCode = carAccount.AccountCode;
                            RemainDebtPlan.Amount = PlanRemainDebt;
                            RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //計算
                            if (RemainDebtPlan.ReceivableBalance == 0m)
                            {
                                RemainDebtPlan.CompleteFlag = "1";
                            }
                            else
                            {
                                RemainDebtPlan.CompleteFlag = "0";
                            }
                            RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            RemainDebtPlan.CreateDate = DateTime.Now;
                            RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            RemainDebtPlan.LastUpdateDate = DateTime.Now;
                            RemainDebtPlan.DelFlag = "0";
                            RemainDebtPlan.Summary = "";
                            RemainDebtPlan.JournalDate = null;
                            RemainDebtPlan.DepositFlag = "0";
                            RemainDebtPlan.PaymentKindCode = "";
                            RemainDebtPlan.CommissionRate = null;
                            RemainDebtPlan.CommissionAmount = null;
                            RemainDebtPlan.CreditJournalId = "";

                            db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                        }
                        //db.SubmitChanges();
                    }
                }
            }*/
        }

        #endregion*/

        #region Excelボタン押下
        /// <summary>
        /// Excelボタン押下
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>Excelファイル</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善
        /// 2017/03/22 arc nakayama #3730_仕入リスト
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExcelDownload(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCEL);

            PaginatedList<CarPurchase> list = new PaginatedList<CarPurchase>();     //Mod 2017/04/13 arc yano

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名:サービス集計表_yyyyMMdd.xlsx
            string fileName = DownLoadTime + "_車両仕入リスト" + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイル取得
            string tfilePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CarPurchaseList"]) ? "" : ConfigurationManager.AppSettings["CarPurchaseList"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePath.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);
                return View("CarPurchaseCriteria", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePath);

            //Add 2018/06/06 arc yano #3883 タマ表改善
            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("CarPurchaseCriteria", list);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        #region エクセルデータ作成(テンプレートファイルあり)
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(form);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //検索結果取得
            List<CarPurchase> list = GetSearchResultListForExcel(form);

            List<CarPurchaseExcel> elist = new List<CarPurchaseExcel>();

            elist = MakeExcelList(list, form);

            //データ設定
            ret = dExport.SetData<CarPurchaseExcel, CarPurchaseExcel>(ref excelFile, elist, configLine);

            excelData = excelFile.GetAsByteArray();

            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }
        #endregion

        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns name = "dt" >検索条件</returns>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            CodeDao dao = new CodeDao(db);

            //---------------------
            //　列定義
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();

            //管理番号SalesCarNumber
            if (!string.IsNullOrWhiteSpace(form["SalesCarNumber"]))
            {

                conditionText += "管理番号=" + form["SalesCarNumber"];
            }

            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "　部門=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }

            //仕入先
            if (!string.IsNullOrWhiteSpace(form["SupplierCode"]))
            {
                Supplier sup = new SupplierDao(db).GetByKey(form["SupplierCode"]);

                conditionText += "　仕入先=" + sup.SupplierName + "(" + sup.SupplierCode + ")";
            }

            //仕入ステータス
            if (!string.IsNullOrWhiteSpace(form["PurchaseStatus"]))
            {
                string StatusName = new CodeDao(db).GetCodeNameByKey("023", form["PurchaseStatus"], false).Name;

                conditionText += "　仕入ステータス=" + StatusName;
            }
            //発注日
            if (!string.IsNullOrWhiteSpace(form["PurchaseOrderDateFrom"]) || !string.IsNullOrWhiteSpace(form["PurchaseOrderDateTo"]))
            {
                conditionText += "　発注日=" + form["PurchaseOrderDateFrom"] + "～" + form["PurchaseOrderDateTo"];
            }
            //仕入日
            if (!string.IsNullOrWhiteSpace(form["SlipDateFrom"]) || !string.IsNullOrWhiteSpace(form["SlipDateTo"]))
            {
                conditionText += "　仕入日=" + form["SlipDateFrom"] + "～" + form["SlipDateTo"];
            }
            //入庫日
            if (!string.IsNullOrWhiteSpace(form["PurchaseDateFrom"]) || !string.IsNullOrWhiteSpace(form["PurchaseDateTo"]))
            {
                conditionText += "　入庫日=" + form["PurchaseDateFrom"] + "～" + form["PurchaseDateTo"];
            }
            //メーカー名
            if (!string.IsNullOrWhiteSpace(form["MakerName"]))
            {

                conditionText += "　メーカー名=" + form["MakerName"];
            }

            //ブランド名
            if (!string.IsNullOrWhiteSpace(form["CarBrandName"]))
            {

                conditionText += "　ブランド名=" + form["CarBrandName"];
            }
            //車種名
            if (!string.IsNullOrWhiteSpace(form["CarName"]))
            {

                conditionText += "　車種名=" + form["CarName"];
            }
            //グレード名
            if (!string.IsNullOrWhiteSpace(form["CarGradeName"]))
            {

                conditionText += "グレード名=" + form["CarGradeName"];
            }
            //車台番号
            if (!string.IsNullOrWhiteSpace(form["Vin"]))
            {

                conditionText += "　車台番号=" + form["Vin"];
            }

            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region 検索結果をExcel用に成形
        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 入庫区分列の追加
        /// </history>
        private List<CarPurchaseExcel> MakeExcelList(List<CarPurchase> list, FormCollection form)
        {
            List<CarPurchaseExcel> elist = new List<CarPurchaseExcel>();

            foreach (var a in list)
            {
                CarPurchaseExcel PurchaseExcel = new CarPurchaseExcel();
                //入庫区分
                PurchaseExcel.CarPurchaseTypeName = (a.c_CarPurchaseType != null ? a.c_CarPurchaseType.Name : "");              //Add 2018/06/06 arc yano #3883
                //管理番号
                PurchaseExcel.SalesCarNumber = a.SalesCarNumber;
                //部門
                if(a.Department != null){
                    PurchaseExcel.DepartmentName = a.Department.DepartmentName;
                }else{
                    PurchaseExcel.DepartmentName = "";
                }
                //新中区分
                if(a.SalesCar != null && a.SalesCar.c_NewUsedType != null){
                    PurchaseExcel.NewUsedTypeName = a.SalesCar.c_NewUsedType.Name;
                }
                //仕入先
                if(a.Supplier != null){
                    PurchaseExcel.SupplierName = a.Supplier.SupplierName;
                }
                //仕入担当
                if(a.Employee != null){
                    PurchaseExcel.PurchaseEmployeeName = a.Employee.EmployeeName;
                }
                //仕入ロケーション
                if(a.Location != null){
                    PurchaseExcel.LocationName = a.Location.LocationName;
                }
                //発注日
                if(a.CarPurchaseOrder != null){
                    PurchaseExcel.PurchaseOrderDate = a.CarPurchaseOrder.PurchaseOrderDate;
                }
                //仕入日
                PurchaseExcel.SlipDate = a.SlipDate;
                //入庫日
                PurchaseExcel.PurchaseDate = a.PurchaseDate;
                //メーカー
                if (a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null && a.SalesCar.CarGrade.Car.Brand != null && a.SalesCar.CarGrade.Car.Brand.Maker != null)
                {
                    PurchaseExcel.MakerName = a.SalesCar.CarGrade.Car.Brand.Maker.MakerName;
                }
                //ブランド
                if(a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null && a.SalesCar.CarGrade.Car.Brand != null){
                   PurchaseExcel.CarBrandName = a.SalesCar.CarGrade.Car.Brand.CarBrandName;
                }
                //車種
                if (a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null)
                {
                    PurchaseExcel.CarName = a.SalesCar.CarGrade.Car.CarName;
                }
                //グレード
                if (a.SalesCar != null && a.SalesCar.CarGrade != null)
                {
                    PurchaseExcel.CarGradeName = a.SalesCar.CarGrade.CarGradeName;
                }
                //新中区分
                if (a.SalesCar != null)
                {
                    PurchaseExcel.Vin = a.SalesCar.Vin;
                }
                //車両本体価格-税抜
                PurchaseExcel.VehiclePrice = a.VehiclePrice;
                //車両本体価格-消費税
                PurchaseExcel.VehicleTax = a.VehicleTax;
                //車両本体価格-税込
                PurchaseExcel.VehicleAmount = a.VehicleAmount;
                //落札料-税抜
                PurchaseExcel.AuctionFeePrice = a.AuctionFeePrice;
                //落札料-消費税
                PurchaseExcel.AuctionFeeTax = a.AuctionFeeTax;
                //落札料-税込
                PurchaseExcel.AuctionFeeAmount = a.AuctionFeeAmount;
                //リサイクル
                PurchaseExcel.RecycleAmount = a.RecycleAmount;
                //自税充当-税抜
                PurchaseExcel.CarTaxAppropriatePrice = a.CarTaxAppropriatePrice;
                //自税充当-消費税
                PurchaseExcel.CarTaxAppropriateTax = a.CarTaxAppropriateTax;
                //自税充当-税込
                PurchaseExcel.CarTaxAppropriateAmount = a.CarTaxAppropriateAmount;
                //仕入金額-税抜
                PurchaseExcel.Amount = a.Amount;
                //仕入金額-消費税
                PurchaseExcel.TaxAmount = a.TaxAmount;
                //仕入金額-税込
                PurchaseExcel.TotalAmount = a.TotalAmount;
                if (a.Employee1 != null && !string.IsNullOrEmpty(a.Employee1.EmployeeName))
                {
                    PurchaseExcel.LastUpdateEmployeeName = a.Employee1.EmployeeName;
                }
                else
                {
                    PurchaseExcel.LastUpdateEmployeeName = "システム更新";
                }

                elist.Add(PurchaseExcel);
            }

            return elist;

        }
        #endregion

        #region 検索処理 Excel用
        /// <summary>
        /// 車両仕入テーブル検索結果リスト取得 Excel用
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両仕入テーブル検索結果リスト</returns>
        private List<CarPurchase> GetSearchResultListForExcel(FormCollection form)
        {

            CarPurchaseDao carPurchaseDao = new CarPurchaseDao(db);
            CarPurchase carPurchaseCondition = new CarPurchase();
            carPurchaseCondition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.SlipDateFrom = CommonUtils.StrToDateTime(form["SlipDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.SlipDateTo = CommonUtils.StrToDateTime(form["SlipDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseDateFrom = CommonUtils.StrToDateTime(form["PurchaseDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseDateTo = CommonUtils.StrToDateTime(form["PurchaseDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseStatus = form["PurchaseStatus"];
            carPurchaseCondition.Department = new Department();
            carPurchaseCondition.Department.DepartmentCode = form["DepartmentCode"];
            carPurchaseCondition.Supplier = new Supplier();
            carPurchaseCondition.Supplier.SupplierCode = form["SupplierCode"];
            carPurchaseCondition.SalesCar = new SalesCar();
            carPurchaseCondition.SalesCar.SalesCarNumber = form["SalesCarNumber"];
            carPurchaseCondition.SalesCar.Vin = form["Vin"];
            carPurchaseCondition.SalesCar.CarGrade = new CarGrade();
            carPurchaseCondition.SalesCar.CarGrade.Car = new Car();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand = new Brand();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker = new Maker();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName = form["MakerName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.CarName = form["CarName"];
            carPurchaseCondition.SalesCar.CarGrade.CarGradeName = form["CarGradeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carPurchaseCondition.DelFlag = form["DelFlag"];
            }
            return carPurchaseDao.GetListByConditionForExcel(carPurchaseCondition);
        }

        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>処理完了</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();

                ret.Add("ProcessedTime", "処理完了");

                return Json(ret);
            }
            return new EmptyResult();
        }
        #endregion

        //Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<CarPurchaseExcelImportList> ImportList = new List<CarPurchaseExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("CarPurchaseImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase ImportFile, List<CarPurchaseExcelImportList> ImportLine, FormCollection form)
        {
            List<CarPurchaseExcelImportList> ImportList = new List<CarPurchaseExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateImportFile(ImportFile);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(ImportFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    form["ErrFlag"] = "0";
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);

                //--------------
                //Excel取り込み
                //--------------
                case "2":

                    DBExecute(ImportLine, form);
                    form["ErrFlag"] = "1"; //取り込んだ後に再度[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);
                //--------------
                //キャンセル
                //--------------
                case "3":

                    ImportList = new List<CarPurchaseExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため

                    return View("CarPurchaseImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);
            }
        }
        #endregion

        #region 取込ファイル存在チェック
        /// <summary>
        /// 取込ファイル存在チェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        /// <returns></returns>
        private void ValidateImportFile(HttpPostedFileBase filePath)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0 )
                {

                    if (stExtension.IndexOf("xlsm") < 0)
                {
                        ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsmファイルではありません"));

                    }
                }
            }

            return;
        }
        #endregion

        #region ダイアログのデータ付きコンポーネント設定
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
        }
        #endregion

        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        private List<CarPurchaseExcelImportList> ReadExcelData(HttpPostedFileBase ImportFile, List<CarPurchaseExcelImportList> ImportList)
        {
            //カラム番号保存用
            int[] colNumber;
            colNumber = new int[70] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(ImportFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("ImportFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImportFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets["CarImportList"];

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
                    return ImportList;
                }

                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row + 2 ; //行の開始位置
                int EndRow = ws.Dimension.End.Row;          //行の終了位置
                int StartCol = ws.Dimension.Start.Column;   //列の開始位置
                int EndCol = ws.Dimension.End.Column;       //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];
                colNumber = SetColNumber(headerRow, colNumber);
                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[colNumber.Count()]; 

                for (datarow = StartRow + 2; datarow < EndRow + 1; datarow++)
                {
                    CarPurchaseExcelImportList data = new CarPurchaseExcelImportList();

                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {

                        for (int i = 0; i < colNumber.Count(); i++)
                        {

                            if (col == colNumber[i])
                            {
                                Result[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region 各項目の列番号設定
        /// <summary>
        /// 各項目の列番号設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //初期処理
            int cnt = 1;

            //列番号設定
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //管理番号("A"で自動)
                    if (cell.Text.Equals("SalesCarNumber"))
                    {
                        colNumber[0] = cnt;
                    }
                    //車台番号
                    if (cell.Text.Equals("Vin"))
                    {
                        colNumber[1] = cnt;
                    }
                    
                    //VIN(シリアル)
                    if (cell.Text.Equals("UsVin"))
                    {
                        colNumber[2] = cnt;
                    }
                    
                    //メーカー名
                    if (cell.Text.Equals("MakerName"))
                    {
                        colNumber[3] = cnt;
                    }
                    
                    //車両グレードコード
                    if (cell.Text.Equals("CarGradeCode"))
                    {
                        colNumber[4] = cnt;
                    }
                    
                    //新中区分("N"or"U")
                    if (cell.Text.Equals("NewUsedType"))
                    {
                        colNumber[5] = cnt;
                    }

                    //系統色
                    if (cell.Text.Equals("ColorType"))
                    {
                        colNumber[6] = cnt;
                    }
                    
                    //外装色コード
                    if (cell.Text.Equals("ExteriorColorCode"))
                    {
                        colNumber[7] = cnt;
                    }
                    
                    //外装色名
                    if (cell.Text.Equals("ExteriorColorName"))
                    {
                        colNumber[8] = cnt;
                    }
                    
                    //内装色コード
                    if (cell.Text.Equals("InteriorColorCode"))
                    {
                        colNumber[9] = cnt;
                    }
                    
                    //内装色名
                    if (cell.Text.Equals("InteriorColorName"))
                    {
                        colNumber[10] = cnt;
                    }
                    
                    //年式
                    if (cell.Text.Equals("ManufacturingYear"))
                    {
                        colNumber[11] = cnt;
                    }
                    
                    //ハンドル
                    if (cell.Text.Equals("Steering"))
                    {
                        colNumber[12] = cnt;
                    }
                    
                    //販売価格(税抜）
                    if (cell.Text.Equals("SalesPrice"))
                    {
                        colNumber[13] = cnt;
                    }
                    
                    //型式
                    if (cell.Text.Equals("ModelName"))
                    {
                        colNumber[14] = cnt;
                    }
                    
                    //原動機型式
                    if (cell.Text.Equals("EngineType"))
                    {
                        colNumber[15] = cnt;
                    }
                    //排気量
                    if (cell.Text.Equals("Displacement"))
                    {
                        colNumber[16] = cnt;
                    }
                    //型式指定番号
                    if (cell.Text.Equals("ModelSpecificateNumber"))
                    {
                        colNumber[17] = cnt;
                    }
                    
                    //類別区分番号
                    if (cell.Text.Equals("ClassificationTypeNumber"))
                    {
                        colNumber[18] = cnt;
                    }
                    
                    //備考１
                    if (cell.Text.Equals("Memo1"))
                    {
                        colNumber[19] = cnt;
                    }
                    
                    //備考２
                    if (cell.Text.Equals("Memo2"))
                    {
                        colNumber[20] = cnt;
                    }
                    
                    //備考３
                    if (cell.Text.Equals("Memo3"))
                    {
                        colNumber[21] = cnt;
                    }
                    
                    //備考４
                    if (cell.Text.Equals("Memo4"))
                    {
                        colNumber[22] = cnt;
                    }
                    
                    //備考５
                    if (cell.Text.Equals("Memo5"))
                    {
                        colNumber[23] = cnt;
                    }
                    
                    //備考６
                    if (cell.Text.Equals("Memo6"))
                    {
                        colNumber[24] = cnt;
                    }
                    
                    //備考７
                    if (cell.Text.Equals("Memo7"))
                    {
                        colNumber[25] = cnt;
                    }
                    
                    //備考８
                    if (cell.Text.Equals("Memo8"))
                    {
                        colNumber[26] = cnt;
                    }
                    
                    //備考９
                    if (cell.Text.Equals("Memo9"))
                    {
                        colNumber[27] = cnt;
                    }
                    
                    //備考１０
                    if (cell.Text.Equals("Memo10"))
                    {
                        colNumber[28] = cnt;
                    }
                    
                    //自動車税
                    if (cell.Text.Equals("CarTax"))
                    {
                        colNumber[29] = cnt;
                    }
                    
                    //自動車重量税
                    if (cell.Text.Equals("CarWeightTax"))
                    {
                        colNumber[30] = cnt;
                    }
                    
                    //自賠責保険料
                    if (cell.Text.Equals("CarLiabilityInsurance"))
                    {
                        colNumber[31] = cnt;
                    }
                    
                    //自動車税環境性能割
                    if (cell.Text.Equals("AcquisitionTax"))
                    {
                        colNumber[32] = cnt;
                    }
                    
                    //リサイクル預託金
                    if (cell.Text.Equals("RecycleDeposit"))
                    {
                        colNumber[33] = cnt;
                    }
                    
                    //認定中古車No
                    if (cell.Text.Equals("ApprovedCarNumber"))
                    {
                        colNumber[34] = cnt;
                    }
                    
                    //認定中古車保証期間FROM
                    if (cell.Text.Equals("ApprovedCarWarrantyDateFrom"))
                    {
                        colNumber[35] = cnt;
                    }

                    //認定中古車保証期間TO
                    if (cell.Text.Equals("ApprovedCarWarrantyDateTo"))
                    {
                        colNumber[36] = cnt;
                    }
                    
                    //仕入日
                    if (cell.Text.Equals("SlipDate"))
                    {
                        colNumber[37] = cnt;
                    }
                    
                    //入庫予定日
                    if (cell.Text.Equals("PurchaseDate"))
                    {
                        colNumber[38] = cnt;
                    }
                    
                    //仕入先コード
                    if (cell.Text.Equals("SupplierCode"))
                    {
                        colNumber[39] = cnt;
                    }
                    
                    //仕入ロケーションコード
                    if (cell.Text.Equals("PurchaseLocationCode"))
                    {
                        colNumber[40] = cnt;
                    }
                    
                    //車両本体価格
                    if (cell.Text.Equals("VehiclePrice"))
                    {
                        colNumber[41] = cnt;
                    }
                    
                    //車両本体消費税
                    if (cell.Text.Equals("VehicleTax"))
                    {
                        colNumber[42] = cnt;
                    }
                    
                    //車両本体税込価格
                    if (cell.Text.Equals("VehicleAmount"))
                    {
                        colNumber[43] = cnt;
                    }
                    
                    //オプション価格
                    if (cell.Text.Equals("OptionPrice"))
                    {
                        colNumber[44] = cnt;
                    }
                    
                    //オプション消費税
                    if (cell.Text.Equals("OptionTax"))
                    {
                        colNumber[45] = cnt;
                    }
                    
                    //オプション税込価格
                    if (cell.Text.Equals("OptionAmount"))
                    {
                        colNumber[46] = cnt;
                    }
                    
                    //ディスカウント価格
                    if (cell.Text.Equals("DiscountPrice"))
                    {
                        colNumber[47] = cnt;
                    }
                    
                    //ディスカウント消費税
                    if (cell.Text.Equals("DiscountTax"))
                    {
                        colNumber[48] = cnt;
                    }
                    
                    //ディスカウント税込価格
                    if (cell.Text.Equals("DiscountAmount"))
                    {
                        colNumber[49] = cnt;
                    }
                    
                    //ファーム価格
                    if (cell.Text.Equals("FirmPrice"))
                    {
                        colNumber[50] = cnt;
                    }
                    
                    //ファーム消費税
                    if (cell.Text.Equals("FirmTax"))
                    {
                        colNumber[51] = cnt;
                    }
                    
                    //ファーム税込価格
                    if (cell.Text.Equals("FirmAmount"))
                    {
                        colNumber[52] = cnt;
                    }
                    
                    //メタリック価格
                    if (cell.Text.Equals("MetallicPrice"))
                    {
                        colNumber[53] = cnt;
                    }
                    
                    //メタリック消費税
                    if (cell.Text.Equals("MetallicTax"))
                    {
                        colNumber[54] = cnt;
                    }
                    
                    //メタリック税込価格
                    if (cell.Text.Equals("MetallicAmount"))
                    {
                        colNumber[55] = cnt;
                    }
                    
                    //加装価格
                    if (cell.Text.Equals("EquipmentPrice"))
                    {
                        colNumber[56] = cnt;
                    }
                    
                    //加修価格
                    if (cell.Text.Equals("RepairPrice"))
                    {
                        colNumber[57] = cnt;
                    }
                    
                    //その他価格
                    if (cell.Text.Equals("OthersPrice"))
                    {
                        colNumber[58] = cnt;
                    }
                    
                    //その他消費税
                    if (cell.Text.Equals("OthersTax"))
                    {
                        colNumber[59] = cnt;
                    }
                    
                    //その他税込価格
                    if (cell.Text.Equals("OthersAmount"))
                    {
                        colNumber[60] = cnt;
                    }
                    
                    //自税充当
                    if (cell.Text.Equals("CarTaxAppropriatePrice"))
                    {
                        colNumber[61] = cnt;
                    }
                    
                    //リサイクル
                    if (cell.Text.Equals("RecyclePrice"))
                    {
                        colNumber[62] = cnt;
                    }
                    
                    //オークション落札料
                    if (cell.Text.Equals("AuctionFeePrice"))
                    {
                        colNumber[63] = cnt;
                    }
                    
                    //オークション落札料消費税
                    if (cell.Text.Equals("AuctionFeeTax"))
                    {
                        colNumber[64] = cnt;
                    }
                    
                    //オークション落札料税込
                    if (cell.Text.Equals("AuctionFeeAmount"))
                    {
                        colNumber[65] = cnt;
                    }
                    
                    //仕入価格
                    if (cell.Text.Equals("Amount"))
                    {
                        colNumber[66] = cnt;
                    }
                    
                    //消費税
                    if (cell.Text.Equals("TaxAmount"))
                    {
                        colNumber[67] = cnt;
                    }
                    
                    //仕入税込価格
                    if (cell.Text.Equals("TotalAmount"))
                    {
                        colNumber[68] = cnt;
                    }
                    
                    //備考
                    if (cell.Text.Equals("Memo"))
                    {
                        colNumber[69] = cnt;
                    }

                }
                cnt++;
            }

            for (int i = 0; i < colNumber.Length; i++)
            {
                if (colNumber[i] == -1)
                {
                    ModelState.AddModelError("ImportFile", "ヘッダ行が正しくありません。");
                    break;
                }
            }


            return colNumber;
        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_【製造】車両仕入　Excel取込対応
        /// </history>
        public List<CarPurchaseExcelImportList> SetProperty(ref string[] Result, ref List<CarPurchaseExcelImportList> ImportList)
        {
            CarPurchaseExcelImportList SetLine = new CarPurchaseExcelImportList();

            //管理番号が入っていなければそこでセットするのをやめる
            if (string.IsNullOrEmpty(Result[0].Trim()))
            {
                return ImportList;
            }

            //管理番号
            SetLine.SalesCarNumber = Result[0].Trim();

            //車台番号
            SetLine.Vin = Result[1].Trim();
            
            //VIN(シリアル)
            SetLine.UsVin = Result[2].Trim();
            
            //メーカー名
            SetLine.MakerName = Result[3].Trim();
            
            //車両グレードコード
            SetLine.CarGradeCode = Result[4].Trim();
            
            //新中区分
            SetLine.NewUsedType = Result[5].Trim();
            
            //系統色
            SetLine.ColorType = Result[6].Trim();
            
            //外装色コード
            SetLine.ExteriorColorCode = Result[7].Trim();
            
            //外装色名
            SetLine.ExteriorColorName = Result[8].Trim();
            
            //内装色コード
            SetLine.InteriorColorCode = Result[9].Trim();
            
            //内装色名
            SetLine.InteriorColorName = Result[10].Trim();
            
            //年式
            SetLine.ManufacturingYear = Result[11].Trim();
            
            //ハンドル
            SetLine.Steering = Result[12].Trim();
            
            //販売価格(税抜）
            SetLine.SalesPrice = Result[13].Trim();
            
            //型式
            SetLine.ModelName = Result[14].Trim();
            
            //原動機型式
            SetLine.EngineType = Result[15].Trim();
            
            //排気量
            SetLine.Displacement = Result[16].Trim();
            
            //型式指定番号
            SetLine.ModelSpecificateNumber = Result[17].Trim();
            
            //類別区分番号
            SetLine.ClassificationTypeNumber = Result[18].Trim();
            
            //備考１
            SetLine.Memo1 = Result[19].Trim();
            
            //備考２
            SetLine.Memo2 = Result[20].Trim();
            
            //備考３
            SetLine.Memo3 = Result[21].Trim();
            
            //備考４
            SetLine.Memo4 = Result[22].Trim();
            
            //備考５
            SetLine.Memo5 = Result[23].Trim();
            
            //備考６
            SetLine.Memo6 = Result[24].Trim();
            
            //備考７
            SetLine.Memo7 = Result[25].Trim();
            
            //備考８
            SetLine.Memo8 = Result[26].Trim();
            
            //備考９
            SetLine.Memo9 = Result[27].Trim();
            
            //備考１０
            SetLine.Memo10 = Result[28].Trim();
            
            //自動車税
            SetLine.CarTax = Result[29].Trim();
            
            //自動車重量税
            SetLine.CarWeightTax = Result[30].Trim();
            
            //自賠責保険料
            SetLine.CarLiabilityInsurance = Result[31].Trim();
            
            //自動車税環境性能割
            SetLine.AcquisitionTax = Result[32].Trim();
            
            //リサイクル預託金
            SetLine.RecycleDeposit = Result[33].Trim();
            
            //認定中古車No
            SetLine.ApprovedCarNumber = Result[34].Trim();
            
            //認定中古車保証期間FROM
            SetLine.ApprovedCarWarrantyDateFrom = Result[35].Trim();
            
            //認定中古車保証期間TO
            SetLine.ApprovedCarWarrantyDateTo = Result[36].Trim();
            
            //仕入日(未入力なら当日日付を入れる)
            if (string.IsNullOrEmpty(Result[37]))
            {
                SetLine.SlipDate = string.Format("{0:yyyy/MM/dd}", DateTime.Now);
            }
            else
            {
                SetLine.SlipDate = Result[37].Trim();
            }
            //入庫予定日
            SetLine.PurchaseDate = Result[38].Trim();
            
            //仕入先コード
            SetLine.SupplierCode = Result[39].Trim();
            
            //仕入ロケーションコード
            SetLine.PurchaseLocationCode = Result[40].Trim();
            
            //車両本体価格
            SetLine.VehiclePrice = Result[41].Trim();
            
            //車両本体消費税
            SetLine.VehicleTax = Result[42].Trim();
            
            //車両本体税込価格
            SetLine.VehicleAmount = Result[43].Trim();
            
            //オプション価格
            SetLine.OptionPrice = Result[44].Trim();
            
            //オプション消費税
            SetLine.OptionTax = Result[45].Trim();
            
            //オプション税込価格
            SetLine.OptionAmount = Result[46].Trim();
            
            //ディスカウント価格
            SetLine.DiscountPrice = Result[47].Trim();
            
            //ディスカウント消費税
            SetLine.DiscountTax = Result[48].Trim();
            
            //ディスカウント税込価格
            SetLine.DiscountAmount = Result[49].Trim();
            
            //ファーム価格
            SetLine.FirmPrice = Result[50].Trim();
            
            //ファーム消費税
            SetLine.FirmTax = Result[51].Trim();
            
            //ファーム税込価格
            SetLine.FirmAmount = Result[52].Trim();
            
            //メタリック価格
            SetLine.MetallicPrice = Result[53].Trim();
            
            //メタリック消費税
            SetLine.MetallicTax = Result[54].Trim();
            
            //メタリック税込価格
            SetLine.MetallicAmount = Result[55].Trim();
            
            //加装価格
            SetLine.EquipmentPrice = Result[56].Trim();
            
            //加修価格
            SetLine.RepairPrice = Result[57].Trim();
            
            //その他価格
            SetLine.OthersPrice = Result[58].Trim();
            
            //その他消費税
            SetLine.OthersTax = Result[59].Trim();
            
            //その他税込価格
            SetLine.OthersAmount = Result[60].Trim();
            
            //自税充当
            SetLine.CarTaxAppropriatePrice = Result[61].Trim();
            
            //リサイクル
            SetLine.RecyclePrice = Result[62].Trim();
            
            //オークション落札料
            SetLine.AuctionFeePrice = Result[63].Trim();
            
            //オークション落札料消費税
            SetLine.AuctionFeeTax = Result[64].Trim();
            
            //オークション落札料税込
            SetLine.AuctionFeeAmount = Result[65].Trim();
            
            //仕入価格
            SetLine.Amount = Result[66].Trim();
            
            //消費税
            SetLine.TaxAmount = Result[67].Trim();
            
            //仕入税込価格
            SetLine.TotalAmount = Result[68].Trim();
            
            //備考
            SetLine.Memo = Result[69].Trim();

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        ///  読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2020/11/27 yano #4072 原動機型式入力エリアの拡張 チェック処理の判定文字数を10 -> 25に変更
        /// 2018/02/15 arc yano #3865 車両ぶっこみ　車両マスタ重複エラー不具合
        /// 2017/11/11/ arc yano #3825 車両一括取込仕様変更 色系統、外装色、内装色のマスタチェックの廃止
        /// </history>
        public void ValidateImportList(List<CarPurchaseExcelImportList> ImportList)
        {
            for (int i = 0; i < ImportList.Count; i++)
            {
                /*----------------*/
                /* ▼必須チェック */
                /*----------------*/

                //管理番号
                if (string.IsNullOrEmpty(ImportList[i].SalesCarNumber))
                {
                    //ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", MessageUtils.GetMessage("E0001", i + 1 + "行目の管理番号が入力されていません。管理番号"));
                    return;
                }

                //車台番号
                if (string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Vin", MessageUtils.GetMessage("E0001", i + 1 + "行目の車台番号が入力されていません。車台番号"));
                }

                //新中区分
                if (string.IsNullOrEmpty(ImportList[i].NewUsedType))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].NewUsedType", MessageUtils.GetMessage("E0001", i + 1 + "行目の新中区分が入力されていません。新中区分"));
                }

                //グレードコード
                if (string.IsNullOrEmpty(ImportList[i].CarGradeCode))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", MessageUtils.GetMessage("E0001", i + 1 + "行目のグレードコードが入力されていません。グレードコード"));
                }
                else
                {
                    CarGrade CGData = new CarGradeDao(db).GetByKey(ImportList[i].CarGradeCode);
                    if (CGData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", i + 1 + "行目の車両グレードコード" + ImportList[i].CarGradeCode + "は車両グレードマスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                    }
                }

                /*------------------*/
                /* ▼マスタチェック */
                /*------------------*/

                /* //Mod 2017/11/11/ arc yano 系統色
                //系統色コード
                if (!string.IsNullOrEmpty(ImportList[i].ColorType))
                {
                    c_ColorCategory ColorCategory = new CodeDao(db).GetColorCategoryByKey(ImportList[i].ColorType, false);
                    if (ColorCategory == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ColorTypeName", i + 1 + "行目の系統色" + ImportList[i].ColorType + "はマスタに登録されていません。");
                    }
                }

                //外装色コード
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorCode))
                {
                    CarColor CarColorData = new CarColorDao(db).GetByKey(ImportList[i].ExteriorColorCode, false);
                    if (CarColorData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorCode", i + 1 + "行目の外装色コード" + ImportList[i].ExteriorColorCode + "は車両カラーマスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                    }
                }

                //内装色コード
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorCode))
                {
                    CarColor CarColorData = new CarColorDao(db).GetByKey(ImportList[i].InteriorColorCode, false);
                    if (CarColorData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorCode", i + 1 + "行目の内装色コード" + ImportList[i].InteriorColorCode + "は車両カラーマスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                    }
                }
                */

                //仕入先コード
                if (!string.IsNullOrEmpty(ImportList[i].SupplierCode))
                {
                    Supplier SupplierData = new SupplierDao(db).GetByKey(ImportList[i].SupplierCode, false);
                    if (SupplierData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SupplierCode", i + 1 + "行目の仕入先コード" + ImportList[i].SupplierCode + "は仕入先マスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                    }
                }

                //仕入先ロケーション
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseLocationCode))
                {
                    Location LocationData = new LocationDao(db).GetByKey(ImportList[i].PurchaseLocationCode, false);
                    if (LocationData == null)
                    {
                        //Mod 2017/07/24 arc nakayama 3780: 【車両一括仕入れ】メッセージ間違い修正
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseLocationCode", i + 1 + "行目の仕入先ロケーションコード" + ImportList[i].PurchaseLocationCode + "はロケーションマスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                    }
                }

                /*------------------*/
                /* ▼重複チェック   */
                /*------------------*/

                //管理番号（DB内に重複がないか）
                //Mod 2018/02/15 arc yano #3865
                //削除データ込みで検索
                SalesCar SalesCarData = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, true);
                //SalesCar SalesCarData = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, false);
                if (SalesCarData != null)
                {
                    if (!string.IsNullOrWhiteSpace(SalesCarData.DelFlag) && SalesCarData.DelFlag.Equals("1"))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "行目の管理番号" + ImportList[i].SalesCarNumber + "は無効データが存在しています。システム課に物理削除依頼をして下さい。");   
                    }
                    else
                    {
                    ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "行目の管理番号" + ImportList[i].SalesCarNumber + "は既に存在しています。車両マスタを確認してください。");
                }
                }

                //管理番号（Excel内に重複がないか）
                var ret = from a in ImportList
                          where ImportList[i].SalesCarNumber.Equals(a.SalesCarNumber)
                          && !a.SalesCarNumber.Equals("A")
                          select a;
                
                if (ret.Count() > 1 )
                {
                    ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "行目の管理番号" + ImportList[i].SalesCarNumber + "は同一Excel内に2つ以上存在しています。１つにして下さい。");
                }

                //車台番号　在庫ステータスが「納車済」以外だったらエラー
                if(!string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    SalesCar SalesCarCheck = new SalesCarDao(db).GetDataByVin(ImportList[i].Vin);
                    if (SalesCarCheck != null)
                    {
                        if (!string.IsNullOrEmpty(SalesCarCheck.CarStatus) && SalesCarCheck.CarStatus != "006")
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "行目の車台番号" + ImportList[i].Vin + "は在庫として存在している可能性があります。該当の車両を確認してから再度実行して下さい。");
                        }
                    }

                }
                //車台番号（Excel内に重複がないか）
                var vinret = from a in ImportList
                          where ImportList[i].Vin.Equals(a.Vin)
                          select a;

                if (vinret.Count() > 1)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "行目の車台番号" + ImportList[i].Vin + "は同一Excel内に2つ以上存在しています。１つにして下さい。");
                }

                /*----------------------*/
                /* ▼データ長チェック   */
                /*----------------------*/

                //管理番号
                if (!string.IsNullOrEmpty(ImportList[i].SalesCarNumber))
                {
                    if(ImportList[i].SalesCarNumber.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "行目の管理番号" + ImportList[i].SalesCarNumber + "は文字数の制限をオーバーしています。管理番号は50文字以内で入力して下さい。");

                    }
                }

                //車台番号
                if (!string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    if (ImportList[i].Vin.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "行目の車台番号" + ImportList[i].Vin + "は文字数の制限をオーバーしています。車台番号は20文字以内で入力して下さい。");

                    }
                }

                //VIN(シリアル)
                if (!string.IsNullOrEmpty(ImportList[i].UsVin))
                {
                    if (ImportList[i].UsVin.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].UsVin", i + 1 + "行目のVIN(シリアル)" + ImportList[i].Vin + "は文字数の制限をオーバーしています。VIN(シリアル)は20文字以内で入力して下さい。");

                    }
                }

                //メーカー名
                if (!string.IsNullOrEmpty(ImportList[i].MakerName))
                {
                    if (ImportList[i].MakerName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MakerName", i + 1 + "行目のメーカー名" + ImportList[i].MakerName + "は文字数の制限をオーバーしています。メーカー名は50文字以内で入力して下さい。");

                    }
                }

                //車両グレードコード
                if (!string.IsNullOrEmpty(ImportList[i].CarGradeCode))
                {
                    if (ImportList[i].CarGradeCode.Length > 30)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", i + 1 + "行目の車両グレードコード" + ImportList[i].CarGradeCode + "は文字数の制限をオーバーしています。車両グレードコードは30文字以内で入力して下さい。");

                    }
                }

                //新中区分
                if (!string.IsNullOrEmpty(ImportList[i].NewUsedType))
                {
                    if (ImportList[i].NewUsedType.Length > 3)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].NewUsedType", i + 1 + "行目の新中区分" + ImportList[i].NewUsedType + "は文字数の制限をオーバーしています。新中区分は3文字以内で入力して下さい。");

                    }
                }

                //系統色
                if (!string.IsNullOrEmpty(ImportList[i].ColorType))
                {
                    if (ImportList[i].ColorType.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ColorTypenName", i + 1 + "行目の系統色" + ImportList[i].ColorType + "は文字数の制限をオーバーしています。系統色は50文字以内で入力して下さい。");

                    }
                }
                
                //外装色コード
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorCode))
                {
                    if (ImportList[i].ExteriorColorCode.Length > 8)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorCode", i + 1 + "行目の外装色コード" + ImportList[i].ExteriorColorCode + "は文字数の制限をオーバーしています。外装色コードは8文字以内で入力して下さい。");

                    }
                }

                //外装色名
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorName))
                {
                    if (ImportList[i].ExteriorColorName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorName", i + 1 + "行目の外装色名" + ImportList[i].ExteriorColorName + "は文字数の制限をオーバーしています。外装色名は50文字以内で入力して下さい。");

                    }
                }

                //内装色コード
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorCode))
                {
                    if (ImportList[i].InteriorColorCode.Length > 8)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorCode", i + 1 + "行目の内装色コード" + ImportList[i].InteriorColorCode + "は文字数の制限をオーバーしています。内装色コードは8文字以内で入力して下さい。");

                    }
                }

                //内装色名
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorName))
                {
                    if (ImportList[i].InteriorColorName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorName", i + 1 + "行目の内装色名" + ImportList[i].InteriorColorName + "は文字数の制限をオーバーしています。内装色名は50文字以内で入力して下さい。");

                    }
                }

                //年式
                if (!string.IsNullOrEmpty(ImportList[i].ManufacturingYear))
                {
                    if (ImportList[i].ManufacturingYear.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ManufacturingYear", i + 1 + "行目の年式" + ImportList[i].ManufacturingYear + "は文字数の制限をオーバーしています。年式は10文字以内で入力して下さい。");
                    }
                }
                
                //ハンドル
                if (!string.IsNullOrEmpty(ImportList[i].Steering))
                {
                    if (ImportList[i].Steering.Length > 3)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Steering", i + 1 + "行目のハンドル" + ImportList[i].Steering + "は文字数の制限をオーバーしています。ハンドルは3文字以内で入力して下さい。");
                    }
                }

                //販売価格(税抜）
                if (!string.IsNullOrEmpty(ImportList[i].SalesPrice))
                {
                    string SalesPrice = ImportList[i].SalesPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(SalesPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesPrice", i + 1 + "行目の販売価格(税抜）" + ImportList[i].SalesPrice + "を数値に変換できません。販売価格(税抜）に10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //販売価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].SalesPrice", i + 1 + "行目の販売価格(税抜）" + ImportList[i].SalesPrice + "の桁数がオーバーしています。販売価格(税抜）に10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //型式
                if (!string.IsNullOrEmpty(ImportList[i].ModelName))
                {
                    if (ImportList[i].ModelName.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ModelName", i + 1 + "行目の型式" + ImportList[i].ModelName + "は文字数の制限をオーバーしています。型式は20文字以内で入力して下さい。");
                    }
                }
                
                //Mod 2020/11/27 yano #4072
                //原動機型式
                if (!string.IsNullOrEmpty(ImportList[i].EngineType))
                {
                    if (ImportList[i].EngineType.Length > 25)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].EngineType", i + 1 + "行目の原動機型式" + ImportList[i].EngineType + "は文字数の制限をオーバーしています。原動機型式は10文字以内で入力して下さい");
                    }
                }
                
                //排気量
                if (!string.IsNullOrEmpty(ImportList[i].Displacement))
                {
                    decimal d;
                    if (!decimal.TryParse(ImportList[i].Displacement, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Displacement", i + 1 + "行目の排気量" + ImportList[i].Displacement + "を数値に変換できません。排気量には10桁以内の数値で入力して下さい");
                    }
                    else
                    {
                        //廃棄量が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < 0)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Displacement", i + 1 + "行目の排気量" + ImportList[i].Displacement + "の桁数がオーバーしています。排気量には10桁以内の数値で入力して下さい");
                        }
                    }
                }

                //型式指定番号
                if (!string.IsNullOrEmpty(ImportList[i].ModelSpecificateNumber))
                {
                    if (ImportList[i].ModelSpecificateNumber.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ModelSpecificateNumber", i + 1 + "行目の型式指定番号" + ImportList[i].ModelSpecificateNumber + "は文字数の制限をオーバーしています。型式指定番号は10文字以内で入力して下さい。");
                    }

                }

                //類別区分番号
                if (!string.IsNullOrEmpty(ImportList[i].ClassificationTypeNumber))
                {
                    if (ImportList[i].ClassificationTypeNumber.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ClassificationTypeNumber", i + 1 + "行目の類別区分番号" + ImportList[i].ClassificationTypeNumber + "は文字数の制限をオーバーしています。類別区分番号は10文字以内で入力して下さい。");
                    }
                }
                
                //備考１
                if (!string.IsNullOrEmpty(ImportList[i].Memo1))
                {
                    if (ImportList[i].Memo1.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo1", i + 1 + "行目の備考１" + ImportList[i].Memo1 + "は文字数の制限をオーバーしています。備考１は100文字以内で入力して下さい。");
                    }
                }
                
                //備考２
                if (!string.IsNullOrEmpty(ImportList[i].Memo2))
                {
                    if (ImportList[i].Memo2.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo2", i + 1 + "行目の備考２" + ImportList[i].Memo2 + "は文字数の制限をオーバーしています。備考２は100文字以内で入力して下さい。");
                    }
                }
                
                //備考３
                if (!string.IsNullOrEmpty(ImportList[i].Memo3))
                {
                    if (ImportList[i].Memo3.Length > 100){ModelState.AddModelError("ImportLine[" + i + "].Memo3", i + 1 + "行目の備考３" + ImportList[i].Memo3 + "は文字数の制限をオーバーしています。備考３は100文字以内で入力して下さい。");
                    }
                }
                
                //備考４
                if (!string.IsNullOrEmpty(ImportList[i].Memo4))
                {
                    if (ImportList[i].Memo4.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo4", i + 1 + "行目の備考４" + ImportList[i].Memo4 + "は文字数の制限をオーバーしています。備考４は100文字以内で入力して下さい。");
                    }
                }

                //備考５
                if (!string.IsNullOrEmpty(ImportList[i].Memo5))
                {
                    if (ImportList[i].Memo5.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo5", i + 1 + "行目の備考５" + ImportList[i].Memo5 + "は文字数の制限をオーバーしています。備考５は100文字以内で入力して下さい。");
                    }
                }
                
                //備考６
                if (!string.IsNullOrEmpty(ImportList[i].Memo6))
                {
                    if (ImportList[i].Memo6.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo6", i + 1 + "行目の備考６" + ImportList[i].Memo6 + "は文字数の制限をオーバーしています。備考６は100文字以内で入力して下さい。");
                    }
                }
                
                //備考７
                if (!string.IsNullOrEmpty(ImportList[i].Memo7))
                {
                    if (ImportList[i].Memo7.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo7", i + 1 + "行目の備考７" + ImportList[i].Memo7 + "は文字数の制限をオーバーしています。備考７は100文字以内で入力して下さい。");
                    }
                }
                
                //備考８
                if (!string.IsNullOrEmpty(ImportList[i].Memo8))
                {
                    if (ImportList[i].Memo8.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo8", i + 1 + "行目の備考８" + ImportList[i].Memo8 + "は文字数の制限をオーバーしています。備考８は100文字以内で入力して下さい。");
                    }
                }
                
                //備考９
                if (!string.IsNullOrEmpty(ImportList[i].Memo9))
                {
                    if (ImportList[i].Memo9.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo9", i + 1 + "行目の備考９" + ImportList[i].Memo9 + "は文字数の制限をオーバーしています。備考９は100文字以内で入力して下さい。");
                    }
                }
                
                //備考１０
                if (!string.IsNullOrEmpty(ImportList[i].Memo10))
                {
                    if (ImportList[i].Memo10.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo10", i + 1 + "行目の備考１０" + ImportList[i].Memo10 + "は文字数の制限をオーバーしています。備考１０は100文字以内で入力して下さい。");    
                    }
                }

                //自動車税
                if (!string.IsNullOrEmpty(ImportList[i].CarTax))
                {
                    string CarTax = ImportList[i].CarTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "行目の自動車税種別割" + ImportList[i].CarTax + "を数値に変換できません。自動車税種別割には10桁以内の数値を入力して下さい");    //Mod 2019/09/04 yano #4011
                        //ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "行目の自動車税" + ImportList[i].CarTax + "を数値に変換できません。自動車税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //自動車税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "行目の自動車税種別割" + ImportList[i].CarTax + "の桁数がオーバーしています。自動車税種別割には10桁以内の数値を入力して下さい");  //Mod 2019/09/04 yano #4011
                            //ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "行目の自動車税" + ImportList[i].CarTax + "の桁数がオーバーしています。自動車税には10桁以内の数値を入力して下さい");
                        }
                    }

                }

                //自動車重量税
                if (!string.IsNullOrEmpty(ImportList[i].CarWeightTax))
                {
                    string CarWeightTax = ImportList[i].CarWeightTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarWeightTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarWeightTax", i + 1 + "行目の自動車重量税" + ImportList[i].CarWeightTax + "を数値に変換できません。自動車重量税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //自動車重量税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarWeightTax", i + 1 + "行目の自動車重量税" + ImportList[i].CarWeightTax + "の桁数がオーバーしています。自動車重量税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //自賠責保険料
                if (!string.IsNullOrEmpty(ImportList[i].CarLiabilityInsurance))
                {
                    string CarLiabilityInsurance = ImportList[i].CarLiabilityInsurance.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarLiabilityInsurance, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarLiabilityInsurance", i + 1 + "行目の自賠責保険料" + ImportList[i].CarLiabilityInsurance + "を数値に変換できません。自賠責保険料には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //自賠責保険料が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarLiabilityInsurance", i + 1 + "行目の自動車重量税" + ImportList[i].CarLiabilityInsurance + "の桁数がオーバーしています。自賠責保険料には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //自動車税環境性能割
                if (!string.IsNullOrEmpty(ImportList[i].AcquisitionTax))
                {
                    string AcquisitionTax = ImportList[i].AcquisitionTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AcquisitionTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AcquisitionTax", i + 1 + "行目の自動車税環境性能割" + ImportList[i].AcquisitionTax + "を数値に変換できません。自動車税環境性能割には10桁以内の数値を入力して下さい");  //Mod 2019/09/04 yano #4011
                    }
                    else
                    {
                        //自動車環境性能割が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AcquisitionTax", i + 1 + "行目の自動車税環境性能割" + ImportList[i].AcquisitionTax + "の桁数がオーバーしています。自動車税環境性能割には10桁以内の数値を入力して下さい");//Mod 2019/09/04 yano #4011
                        }
                    }
                }

                //リサイクル預託金
                if (!string.IsNullOrEmpty(ImportList[i].RecycleDeposit))
                {
                    string RecycleDeposit = ImportList[i].RecycleDeposit.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RecycleDeposit, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RecycleDeposit", i + 1 + "行目のリサイクル預託金" + ImportList[i].RecycleDeposit + "を数値に変換できません。リサイクル預託金には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //リサイクル預託金が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RecycleDeposit", i + 1 + "行目のリサイクル預託金" + ImportList[i].RecycleDeposit + "の桁数がオーバーしています。リサイクル預託金には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //認定中古車No
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarNumber))
                {
                    if (ImportList[i].ApprovedCarNumber.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarNumber", i + 1 + "行目の認定中古車No" + ImportList[i].ApprovedCarNumber + "は文字数の制限をオーバーしています。認定中古車Noは50文字以内で入力して下さい。");
                    }
                }

                //認定中古車保証期間FROM
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarWarrantyDateFrom))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].ApprovedCarWarrantyDateFrom, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarWarrantyDateFrom", i + 1 + "行目の認定中古車保証期間FROM" + ImportList[i].ApprovedCarWarrantyDateFrom + "を日付に変換できません。認定中古車保証期間FROMには日付(YYYY/MM/DD)を入力して下さい");
                    }
                }

                //認定中古車保証期間TO
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarWarrantyDateTo))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].ApprovedCarWarrantyDateTo, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarWarrantyDateTo", i + 1 + "行目の認定中古車保証期間TO" + ImportList[i].ApprovedCarWarrantyDateTo + "を日付に変換できません。認定中古車保証期間TOには日付(YYYY/MM/DD)を入力して下さい");
                    }
                }

                //仕入日
                if (!string.IsNullOrEmpty(ImportList[i].SlipDate))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].SlipDate, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SlipDate", i + 1 + "行目の仕入日" + ImportList[i].SlipDate + "を日付に変換できません。仕入日には日付(YYYY/MM/DD)を入力して下さい");
                    }
                }

                //入庫予定日
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseDate))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].PurchaseDate, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseDate", i + 1 + "行目の入庫予定日" + ImportList[i].PurchaseDate + "を日付に変換できません。入庫予定日には日付(YYYY/MM/DD)を入力して下さい");
                    }
                }

                //仕入先コード
                if (!string.IsNullOrEmpty(ImportList[i].SupplierCode))
                {
                    if (ImportList[i].SupplierCode.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SupplierCode", i + 1 + "行目の仕入先コード" + ImportList[i].SupplierCode + "は文字数の制限をオーバーしています。仕入先コードは10文字以内で入力して下さい。");
                    }
                }

                //仕入ロケーションコード
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseLocationCode))
                {
                    if (ImportList[i].PurchaseLocationCode.Length > 12)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseLocationCode", i + 1 + "行目の仕入ロケーションコード" + ImportList[i].PurchaseLocationCode + "は文字数の制限をオーバーしています。仕入ロケーションコードは12文字以内で入力して下さい。");
                    }
                }

                //車両本体価格
                if (!string.IsNullOrEmpty(ImportList[i].VehiclePrice))
                {
                    string VehiclePrice = ImportList[i].VehiclePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehiclePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehiclePrice", i + 1 + "行目の車両本体価格" + ImportList[i].VehiclePrice + "を数値に変換できません。車両本体価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //車両本体価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehiclePrice", i + 1 + "行目の車両本体価格" + ImportList[i].VehiclePrice + "の桁数がオーバーしています。車両本体価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //車両本体消費税
                if (!string.IsNullOrEmpty(ImportList[i].VehicleTax))
                {
                    string VehicleTax = ImportList[i].VehicleTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehicleTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehicleTax", i + 1 + "行目の車両本体消費税" + ImportList[i].VehicleTax + "を数値に変換できません。車両本体消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //車両本体消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehicleTax", i + 1 + "行目の車両本体価格" + ImportList[i].VehicleTax + "の桁数がオーバーしています。車両本体消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //車両本体税込価格
                if (!string.IsNullOrEmpty(ImportList[i].VehicleAmount))
                {
                    string VehicleAmount = ImportList[i].VehicleAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehicleAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehicleAmount", i + 1 + "行目の車両本体税込価格" + ImportList[i].VehicleAmount + "を数値に変換できません。車両本体税込価格には数値を入力して下さい");
                    }
                    else
                    {
                        //車両本体消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehicleAmount", i + 1 + "行目の車両本体税込価格" + ImportList[i].VehicleAmount + "の桁数がオーバーしています。車両本体税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オプション価格
                if (!string.IsNullOrEmpty(ImportList[i].OptionPrice))
                {
                    string OptionPrice = ImportList[i].OptionPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionPrice", i + 1 + "行目のオプション価格" + ImportList[i].OptionPrice + "を数値に変換できません。オプション価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オプション価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionPrice", i + 1 + "行目のオプション価格" + ImportList[i].OptionPrice + "の桁数がオーバーしています。オプション価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オプション消費税
                if (!string.IsNullOrEmpty(ImportList[i].OptionTax))
                {
                    string OptionTax = ImportList[i].OptionTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionTax", i + 1 + "行目のオプション消費税" + ImportList[i].OptionTax + "を数値に変換できません。オプション消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オプション消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionTax", i + 1 + "行目のオプション消費税" + ImportList[i].OptionTax + "の桁数がオーバーしています。オプション消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オプション税込価格
                if (!string.IsNullOrEmpty(ImportList[i].OptionAmount))
                {
                    string OptionAmount = ImportList[i].OptionAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionAmount", i + 1 + "行目のオプション税込価格" + ImportList[i].OptionAmount + "を数値に変換できません。オプション税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オプション税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionAmount", i + 1 + "行目のオプション税込価格" + ImportList[i].OptionAmount + "の桁数がオーバーしています。オプション税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ディスカウント価格
                if (!string.IsNullOrEmpty(ImportList[i].DiscountPrice))
                {
                    string DiscountPrice = ImportList[i].DiscountPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountPrice", i + 1 + "行目のディスカウント価格" + ImportList[i].DiscountPrice + "を数値に変換できません。ディスカウント価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ディスカウント価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountPrice", i + 1 + "行目のディスカウント価格" + ImportList[i].DiscountPrice + "の桁数がオーバーしています。ディスカウント価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ディスカウント消費税
                if (!string.IsNullOrEmpty(ImportList[i].DiscountTax))
                {
                    string DiscountTax = ImportList[i].DiscountTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountTax", i + 1 + "行目のディスカウント消費税" + ImportList[i].DiscountTax + "を数値に変換できません。ディスカウント消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ディスカウント消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountTax", i + 1 + "行目のディスカウント消費税" + ImportList[i].DiscountTax + "の桁数がオーバーしています。ディスカウント消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ディスカウント税込価格
                if (!string.IsNullOrEmpty(ImportList[i].DiscountAmount))
                {
                    string DiscountAmount = ImportList[i].DiscountAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountAmount", i + 1 + "行目のディスカウント税込価格" + ImportList[i].DiscountAmount + "を数値に変換できません。ディスカウント税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ディスカウント税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountAmount", i + 1 + "行目のディスカウント税込価格" + ImportList[i].DiscountAmount + "の桁数がオーバーしています。ディスカウント税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ファーム価格
                if (!string.IsNullOrEmpty(ImportList[i].FirmPrice))
                {
                    string FirmPrice = ImportList[i].FirmPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmPrice", i + 1 + "行目のファーム価格" + ImportList[i].FirmPrice + "を数値に変換できません。ファーム価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ファーム価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmPrice", i + 1 + "行目のファーム価格" + ImportList[i].FirmPrice + "の桁数がオーバーしています。ファーム価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ファーム消費税
                if (!string.IsNullOrEmpty(ImportList[i].FirmTax))
                {
                    string FirmTax = ImportList[i].FirmTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmTax", i + 1 + "行目のファーム消費税" + ImportList[i].FirmTax + "を数値に変換できません。ファーム消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ファーム消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmTax", i + 1 + "行目のファーム消費税" + ImportList[i].FirmTax + "の桁数がオーバーしています。ファーム消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //ファーム税込価格
                if (!string.IsNullOrEmpty(ImportList[i].FirmAmount))
                {
                    string FirmAmount = ImportList[i].FirmAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmAmount", i + 1 + "行目のファーム税込価格" + ImportList[i].FirmAmount + "を数値に変換できません。ファーム税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //ファーム税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmAmount", i + 1 + "行目のファーム税込価格" + ImportList[i].FirmAmount + "の桁数がオーバーしています。ファーム税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //メタリック価格
                if (!string.IsNullOrEmpty(ImportList[i].MetallicPrice))
                {
                    string MetallicPrice = ImportList[i].MetallicPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicPrice", i + 1 + "行目のメタリック価格" + ImportList[i].MetallicPrice + "を数値に変換できません。メタリック価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //メタリック価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicPrice", i + 1 + "行目のメタリック価格" + ImportList[i].MetallicPrice + "の桁数がオーバーしています。メタリック価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //メタリック消費税
                if (!string.IsNullOrEmpty(ImportList[i].MetallicTax))
                {
                    string MetallicTax = ImportList[i].MetallicTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicTax", i + 1 + "行目のメタリック消費税" + ImportList[i].MetallicTax + "を数値に変換できません。メタリック消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //メタリック消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicTax", i + 1 + "行目のメタリック価格" + ImportList[i].MetallicTax + "の桁数がオーバーしています。メタリック消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //メタリック税込価格
                if (!string.IsNullOrEmpty(ImportList[i].MetallicAmount))
                {
                    string MetallicAmount = ImportList[i].MetallicAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicAmount", i + 1 + "行目のメタリック税込価格" + ImportList[i].MetallicAmount + "を数値に変換できません。メタリック税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //メタリック税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicAmount", i + 1 + "行目のメタリック税込価格" + ImportList[i].MetallicAmount + "の桁数がオーバーしています。メタリック税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //加装価格
                if (!string.IsNullOrEmpty(ImportList[i].EquipmentPrice))
                {
                    string EquipmentPrice = ImportList[i].EquipmentPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(EquipmentPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].EquipmentPrice", i + 1 + "行目の加装価格" + ImportList[i].EquipmentPrice + "を数値に変換できません。加装価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //加装価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].EquipmentPrice", i + 1 + "行目の加装価格" + ImportList[i].EquipmentPrice + "の桁数がオーバーしています。加装価格には10桁以内の数値を入力して下さい");
                        }
                    }

                }

                //加修価格
                if (!string.IsNullOrEmpty(ImportList[i].RepairPrice))
                {
                    string RepairPrice = ImportList[i].RepairPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RepairPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RepairPrice", i + 1 + "行目の加修価格" + ImportList[i].RepairPrice + "を数値に変換できません。加修価格には数値を入力して下さい");
                    }
                    else
                    {
                        //加修価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RepairPrice", i + 1 + "行目の加修価格" + ImportList[i].RepairPrice + "の桁数がオーバーしています。加修価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //その他価格
                if (!string.IsNullOrEmpty(ImportList[i].OthersPrice))
                {
                    string OthersPrice = ImportList[i].OthersPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersPrice", i + 1 + "行目のその他価格" + ImportList[i].OthersPrice + "を数値に変換できません。その他価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //その他価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersPrice", i + 1 + "行目のその他価格" + ImportList[i].OthersPrice + "の桁数がオーバーしています。その他価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //その他消費税
                if (!string.IsNullOrEmpty(ImportList[i].OthersTax))
                {
                    string OthersTax = ImportList[i].OthersTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersTax", i + 1 + "行目のその他消費税" + ImportList[i].OthersTax + "を数値に変換できません。その他消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //その他消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersTax", i + 1 + "行目のその他消費税" + ImportList[i].OthersTax + "の桁数がオーバーしています。その他消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //その他税込価格
                if (!string.IsNullOrEmpty(ImportList[i].OthersAmount))
                {
                    string OthersAmount = ImportList[i].OthersAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersAmount", i + 1 + "行目のその他税込価格" + ImportList[i].OthersAmount + "を数値に変換できません。その他税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //その他税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersAmount", i + 1 + "行目のその他税込価格" + ImportList[i].OthersAmount + "の桁数がオーバーしています。その他税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //自税充当
                if (!string.IsNullOrEmpty(ImportList[i].CarTaxAppropriatePrice))
                {
                    string CarTaxAppropriatePrice = ImportList[i].CarTaxAppropriatePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarTaxAppropriatePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarTaxAppropriatePrice", i + 1 + "行目の自税充当" + ImportList[i].CarTaxAppropriatePrice + "を数値に変換できません。自税充当には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //自税充当が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarTaxAppropriatePrice", i + 1 + "行目の自税充当" + ImportList[i].CarTaxAppropriatePrice + "の桁数がオーバーしています。自税充当には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //リサイクル
                if (!string.IsNullOrEmpty(ImportList[i].RecyclePrice))
                {
                    string RecyclePrice = ImportList[i].RecyclePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RecyclePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RecyclePrice", i + 1 + "行目のリサイクル" + ImportList[i].RecyclePrice + "を数値に変換できません。リサイクルには10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //リサイクルが10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RecyclePrice", i + 1 + "行目のリサイクル" + ImportList[i].RecyclePrice + "の桁数がオーバーしています。リサイクルには10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オークション落札料
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeePrice))
                {
                    string AuctionFeePrice = ImportList[i].AuctionFeePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeePrice", i + 1 + "行目のオークション落札料" + ImportList[i].AuctionFeePrice + "を数値に変換できません。オークション落札料には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オークション落札料が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeePrice", i + 1 + "行目のオークション落札料" + ImportList[i].AuctionFeePrice + "の桁数がオーバーしています。オークション落札料には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オークション落札料消費税
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeeTax))
                {
                    string AuctionFeeTax = ImportList[i].AuctionFeeTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeeTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeTax", i + 1 + "行目のオークション落札料消費税" + ImportList[i].AuctionFeeTax + "を数値に変換できません。オークション落札料消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オークション落札料消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeTax", i + 1 + "行目のオークション落札料消費税" + ImportList[i].AuctionFeeTax + "の桁数がオーバーしています。オークション落札料消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //オークション落札料税込
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeeAmount))
                {
                    string AuctionFeeAmount = ImportList[i].AuctionFeeAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeeAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeAmount", i + 1 + "行目のオークション落札料税込" + ImportList[i].AuctionFeeAmount + "を数値に変換できません。オークション落札料税込には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //オークション落札料税込が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeAmount", i + 1 + "行目のオークション落札料税込" + ImportList[i].AuctionFeeAmount + "の桁数がオーバーしています。オークション落札料税込には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //仕入価格
                if (!string.IsNullOrEmpty(ImportList[i].Amount))
                {
                    string Amount = ImportList[i].Amount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(Amount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Amount", i + 1 + "行目の仕入価格" + ImportList[i].Amount + "を数値に変換できません。仕入価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //仕入価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Amount", i + 1 + "行目の仕入価格" + ImportList[i].Amount + "の桁数がオーバーしています。仕入価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //仕入消費税
                if (!string.IsNullOrEmpty(ImportList[i].TaxAmount))
                {
                    string TaxAmount = ImportList[i].TaxAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(TaxAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].TaxAmount", i + 1 + "行目の仕入消費税" + ImportList[i].TaxAmount + "を数値に変換できません。仕入消費税には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //仕入消費税が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].TaxAmount", i + 1 + "行目の仕入消費税" + ImportList[i].TaxAmount + "の桁数がオーバーしています。仕入消費税には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //仕入税込価格
                if (!string.IsNullOrEmpty(ImportList[i].TotalAmount))
                {
                    string TotalAmount = ImportList[i].TotalAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(TotalAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].TotalAmount", i + 1 + "行目の仕入税込価格" + ImportList[i].TotalAmount + "を数値に変換できません。仕入税込価格には10桁以内の数値を入力して下さい");
                    }
                    else
                    {
                        //仕入税込価格が10桁オーバーの場合
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].TotalAmount", i + 1 + "行目の仕入税込価格" + ImportList[i].TotalAmount + "の桁数がオーバーしています。仕入税込価格には10桁以内の数値を入力して下さい");
                        }
                    }
                }

                //備考
                if (!string.IsNullOrEmpty(ImportList[i].Memo))
                {
                    if (ImportList[i].Memo.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo", i + 1 + "行目の備考" + ImportList[i].Memo + "は文字数の制限をオーバーしています。備考は100文字以内で入力して下さい。");
                    }
                }
            }
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 財務価格はリサイクル料金を除いた価格とする
        /// 2018/06/06 arc yano #3883 タマ表改善 仕入価格で財務価格を更新する
        /// 2018/02/15 arc yano #3865 車両ぶっこみ　車両マスタ重複エラー不具合
        /// </history>
        private void DBExecute(List<CarPurchaseExcelImportList> ImportLine, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {

                List<SalesCar> SalesCarList = new List<SalesCar>();
                List<CarPurchase> CarPurchaseList = new List<CarPurchase>();

                //車両マスタと車両入荷テーブルに読み込んだ情報を登録する
                foreach (var LineData in ImportLine)
                {
                    /*-----------------*/
                    /*   車両マスタ    */
                    /*-----------------*/

                    //車台番号で車両マスタを検索して、データが存在し、かつ、納車済だった場合は、古いデータを論理削除して、取り込まれる内容を新規で登録する
                    SalesCar SalesCarDataCheck = new SalesCar();
                    if (!string.IsNullOrEmpty(LineData.Vin))
                    {
                        SalesCarDataCheck = new SalesCarDao(db).GetDataByVin(LineData.Vin);
                        if (SalesCarDataCheck != null)
                        {
                            if (!string.IsNullOrEmpty(SalesCarDataCheck.CarStatus) && SalesCarDataCheck.CarStatus.Equals("006"))
                            {
                                SalesCarDataCheck.DelFlag = "1";    //論理削除
                                LineData.NewUsedType = "U"; //既に存在していた車両のため、ExcelからDBに取り込む新中区分はExcelの内容に関係なく「中古車」にする
                                ModelState.AddModelError("", "車台番号： " + LineData.Vin + " が既に納車済みで重複していたため、管理番号を振りなおしました。");
                            }
                        }
                    }

                    SalesCar SalesCarData = new SalesCar();

                    //管理番号(Aなら自動採番)
                    if (LineData.SalesCarNumber.Equals("A"))
                    {
                        string companyCode = "N/A";
                        try { companyCode = new CarGradeDao(db).GetByKey(LineData.CarGradeCode).Car.Brand.CompanyCode; }
                        catch (NullReferenceException) { }

                        SalesCarData.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, LineData.NewUsedType);
                    }
                    else
                    {
                        SalesCarData.SalesCarNumber = LineData.SalesCarNumber;
                    }
                    SalesCarData.CarGradeCode = LineData.CarGradeCode;                                                  //車両グレードコード
                    SalesCarData.NewUsedType = LineData.NewUsedType;                                                    //新中区分
                    c_ColorCategory clorTypeData = new CodeDao(db).GetColorCategoryByKey(LineData.ColorType, false);
                    if (clorTypeData != null)
                    {
                        SalesCarData.ColorType = clorTypeData.Code ?? null;                                             //系統色
                    }
                    else
                    {
                        SalesCarData.ColorType = null;
                    }
                    SalesCarData.ExteriorColorCode = LineData.ExteriorColorCode;                                        //外装色コード
                    SalesCarData.ExteriorColorName = LineData.ExteriorColorName;                                        //外装色名
                    SalesCarData.ChangeColor = null;                                                                    //色替
                    SalesCarData.InteriorColorCode = LineData.InteriorColorCode;                                        //内装色コード
                    SalesCarData.InteriorColorName = LineData.InteriorColorName;                                        //内装色名
                    SalesCarData.ManufacturingYear = LineData.ManufacturingYear;                                        //年式
                    SalesCarData.CarStatus = "001";                                                                     //在庫ステータス

                    if (!string.IsNullOrEmpty(LineData.PurchaseLocationCode))
                    {
                        SalesCarData.LocationCode = LineData.PurchaseLocationCode;                                      //在庫ロケーション
                    }
                    else
                    {
                        SalesCarData.LocationCode = null;
                    }
                    SalesCarData.OwnerCode = null;                                                                      //所有者コード(顧客コード)
                    SalesCarData.Steering = LineData.Steering;                                                          //ハンドル

                    if (string.IsNullOrEmpty(LineData.SalesPrice))
                    {
                        SalesCarData.SalesPrice = null;
                    }
                    else
                    {
                    SalesCarData.SalesPrice = decimal.Parse(LineData.SalesPrice.Replace(",", ""));                      //販売価格
                    }
                    SalesCarData.IssueDate = null;                                                                      //車検証発行日
                    SalesCarData.MorterViecleOfficialCode = null;                                                       //陸運局コード
                    SalesCarData.RegistrationNumberType = null;                                                         //車両登録番号(種別)
                    SalesCarData.RegistrationNumberKana = null;                                                         //車両登録番号(かな)
                    SalesCarData.RegistrationNumberPlate = null;                                                        //車両登録番号(プレート)
                    SalesCarData.RegistrationDate = null;                                                               //登録日
                    SalesCarData.FirstRegistrationYear = null;                                                          //初年度登録
                    SalesCarData.CarClassification = null;                                                              //自動車種別
                    SalesCarData.Usage = null;                                                                          //用途
                    SalesCarData.UsageType = null;                                                                      //事自区分
                    SalesCarData.Figure = null;                                                                         //形状
                    SalesCarData.MakerName = LineData.MakerName;                                                        //メーカー名
                    SalesCarData.Capacity = null;                                                                       //定員
                    SalesCarData.MaximumLoadingWeight = null;                                                           //最大積載量
                    SalesCarData.CarWeight = null;                                                                      //車両重量
                    SalesCarData.TotalCarWeight = null;                                                                 //車両総重量
                    SalesCarData.Vin = LineData.Vin;                                                                    //車台番号
                    SalesCarData.Length = null;                                                                         //長さ
                    SalesCarData.Width = null;                                                                          //幅
                    SalesCarData.Height = null;                                                                         //高さ
                    SalesCarData.FFAxileWeight = null;                                                                  //前前軸重
                    SalesCarData.FRAxileWeight = null;                                                                  //前後軸重
                    SalesCarData.RFAxileWeight = null;                                                                  //後前軸重
                    SalesCarData.RRAxileWeight = null;                                                                  //後後軸重
                    SalesCarData.ModelName = LineData.ModelName;                                                        //型式
                    SalesCarData.EngineType = LineData.EngineType;                                                      //原動機型式

                    if (string.IsNullOrEmpty(LineData.Displacement))
                    {
                        SalesCarData.Displacement = null;
                    }
                    else
                    {
                    SalesCarData.Displacement = decimal.Parse(LineData.Displacement);                                   //排気量
                    }
                    SalesCarData.Fuel = null;                                                                           //燃料種類
                    SalesCarData.ModelSpecificateNumber = LineData.ModelSpecificateNumber;                              //型式指定番号
                    SalesCarData.ClassificationTypeNumber = LineData.ClassificationTypeNumber;                          //類別区分番号
                    SalesCarData.PossesorName = null;                                                                   //所有者氏名
                    SalesCarData.PossesorAddress = null;                                                                //所有者住所
                    SalesCarData.UserName = null;                                                                       //使用者氏名
                    SalesCarData.UserAddress = null;                                                                    //使用者住所
                    SalesCarData.PrincipalPlace = null;                                                                 //本拠地
                    SalesCarData.ExpireType = null;                                                                     //有効期限種別
                    SalesCarData.ExpireDate = null;                                                                     //有効期限
                    SalesCarData.Mileage = null;                                                                        //走行距離
                    SalesCarData.MileageUnit = null;                                                                    //走行距離単位
                    SalesCarData.Memo = null;                                                                           //車検証備考
                    SalesCarData.DocumentComplete = null;                                                               //書類完備
                    SalesCarData.DocumentRemarks = null;                                                                //書類備考
                    SalesCarData.SalesDate = null;                                                                      //納車日
                    SalesCarData.InspectionDate = null;                                                                 //点検日
                    SalesCarData.NextInspectionDate = null;                                                             //次回点検日
                    SalesCarData.UsVin = LineData.UsVin;                                                                //VIN(北米用)
                    SalesCarData.MakerWarranty = null;                                                                  //メーカー保証
                    SalesCarData.RecordingNote = null;                                                                  //記録簿(有無)
                    SalesCarData.ProductionDate = null;                                                                 //生産日(MDH)
                    SalesCarData.ReparationRecord = null;                                                               //修復歴(有無)
                    SalesCarData.Oil = null;                                                                            //お客様指定オイル(部品番号)
                    SalesCarData.Tire = null;                                                                           //タイヤ(部品番号)
                    SalesCarData.KeyCode = null;                                                                        //キーコード
                    SalesCarData.AudioCode = null;                                                                      //オーディオコード
                    SalesCarData.Import = null;                                                                         //輸入
                    SalesCarData.Guarantee = null;                                                                      //保証書
                    SalesCarData.Instructions = null;                                                                   //取説
                    SalesCarData.Recycle = null;                                                                        //リサイクル
                    SalesCarData.RecycleTicket = null;                                                                  //リサイクル券
                    SalesCarData.CouponPresence = null;                                                                 //クーポン有無
                    SalesCarData.Light = null;                                                                          //ライト
                    SalesCarData.Aw = null;                                                                             //ＡＷ
                    SalesCarData.Aero = null;                                                                           //エアロ
                    SalesCarData.Sr = null;                                                                             //ＳＲ
                    SalesCarData.Cd = null;                                                                             //ＣＤ
                    SalesCarData.Md = null;                                                                             //ＭＤ
                    SalesCarData.NaviType = null;                                                                       //ナビ(純正・純正外)
                    SalesCarData.NaviEquipment = null;                                                                  //ナビ(HDD、メモリ、DVD、CD)
                    SalesCarData.NaviDashboard = null;                                                                  //ナビ(OnDash、InDash)
                    SalesCarData.SeatColor = null;                                                                      //シート(色)
                    SalesCarData.SeatType = null;                                                                       //シート
                    SalesCarData.Memo1 = LineData.Memo1;                                                                //備考１
                    SalesCarData.Memo2 = LineData.Memo2;                                                                //備考２
                    SalesCarData.Memo3 = LineData.Memo3;                                                                //備考３
                    SalesCarData.Memo4 = LineData.Memo4;                                                                //備考４
                    SalesCarData.Memo5 = LineData.Memo5;                                                                //備考５
                    SalesCarData.Memo6 = LineData.Memo6;                                                                //備考６
                    SalesCarData.Memo7 = LineData.Memo7;                                                                //備考７
                    SalesCarData.Memo8 = LineData.Memo8;                                                                //備考８
                    SalesCarData.Memo9 = LineData.Memo9;                                                                //備考９
                    SalesCarData.Memo10 = LineData.Memo10;                                                              //備考１０
                    SalesCarData.DeclarationType = null;                                                                //申告区分
                    SalesCarData.AcquisitionReason = null;                                                              //取得原因
                    SalesCarData.TaxationTypeCarTax = null;                                                             //課税区分(自動車税種別割)
                    SalesCarData.TaxationTypeAcquisitionTax = null;                                                     //課税区分(自動車税環境性能割)

                    if (string.IsNullOrEmpty(LineData.CarTax))
                    {
                        SalesCarData.CarTax = null;
                    }
                    else
                    {
                        SalesCarData.CarTax = decimal.Parse(LineData.CarTax.Replace(",", ""));                              //自動車税
                    }

                    if(string.IsNullOrEmpty(LineData.CarWeightTax))
                    {
                        SalesCarData.CarWeightTax = null;
                    }else{
                    SalesCarData.CarWeightTax = decimal.Parse(LineData.CarWeightTax.Replace(",", ""));                  //自動車重量税
                    }

                    if(string.IsNullOrEmpty(LineData.CarLiabilityInsurance))
                    {
                        SalesCarData.CarLiabilityInsurance = null;
                    }else{
                    SalesCarData.CarLiabilityInsurance = decimal.Parse(LineData.CarLiabilityInsurance.Replace(",", ""));//自賠責保険料
                    }

                    if (string.IsNullOrEmpty(LineData.AcquisitionTax))
                    {
                        LineData.AcquisitionTax = null;
                    }else{
                    SalesCarData.AcquisitionTax = decimal.Parse(LineData.AcquisitionTax.Replace(",", ""));              //自動車税環境性能割
                    }

                    if (string.IsNullOrEmpty(LineData.RecycleDeposit))
                    {
                        SalesCarData.RecycleDeposit = null;
                    }else{
                    SalesCarData.RecycleDeposit = decimal.Parse(LineData.RecycleDeposit.Replace(",", ""));              //リサイクル預託金
                    }

                    
                    SalesCarData.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                     //作成者
                    SalesCarData.CreateDate = DateTime.Now;                                                             //作成日時
                    SalesCarData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                 //最終更新者
                    SalesCarData.LastUpdateDate = DateTime.Now;                                                         //最終更新日時
                    SalesCarData.DelFlag = "0";                                                                         //削除フラグ
                    SalesCarData.EraseRegist = null;                                                                    //抹消登録
                    SalesCarData.UserCode = null;                                                                       //使用者コード
                    SalesCarData.ApprovedCarNumber = LineData.ApprovedCarNumber;                                        //認定中古車No

                    if (string.IsNullOrEmpty(LineData.ApprovedCarWarrantyDateFrom))
                    {
                        SalesCarData.ApprovedCarWarrantyDateFrom = null;
                    }
                    else
                    {
                    SalesCarData.ApprovedCarWarrantyDateFrom = DateTime.Parse(LineData.ApprovedCarWarrantyDateFrom);    //認定中古車保証期間FROM
                    }

                    if (string.IsNullOrEmpty(LineData.ApprovedCarWarrantyDateTo))
                    {
                        SalesCarData.ApprovedCarWarrantyDateTo = null;
                    }
                    else
                    {
                    SalesCarData.ApprovedCarWarrantyDateTo = DateTime.Parse(LineData.ApprovedCarWarrantyDateTo);        //認定中古車保証期間TO
                    }
                    
                    SalesCarData.Finance = null;                                                                        //ファイナンス
                    SalesCarData.InspectGuidFlag = "001";                                                               //車検案内可否フラグ
                    SalesCarData.InspectGuidMemo = null;                                                                //車検案内発送備考欄
                    SalesCarData.CarUsage = null;                                                                       //利用用途
                    SalesCarData.FirstRegistrationDate = null;                                                          //初年度登録日
                    SalesCarData.CompanyRegistrationFlag = null;                                                        //登録フラグ

                    SalesCarList.Add(SalesCarData);

                    /*-----------------------*/
                    /*   車両入荷テーブル    */
                    /*-----------------------*/

                    CarPurchase CarPurchaseData = new CarPurchase();

                    CarPurchaseData.CarPurchaseId = Guid.NewGuid();                                                           //車両仕入ID
                    CarPurchaseData.CarPurchaseOrderNumber = null;                                                            //車両発注引当ID
                    CarPurchaseData.CarAppraisalId = null;                                                                    //車両査定ID
                    CarPurchaseData.PurchaseStatus = "002";                                                                   //仕入ステータス

                    if (string.IsNullOrEmpty(LineData.PurchaseDate))
                    {
                        CarPurchaseData.PurchaseDate = null;
                    }
                    else
                    {
                    CarPurchaseData.PurchaseDate = DateTime.Parse(LineData.PurchaseDate);                                     //入庫日（経理の仕入れ計上日、自動車メーカーから仕入れた日）
                    }
                    
                    CarPurchaseData.SupplierCode = LineData.SupplierCode;                                                     //仕入先コード
                    CarPurchaseData.PurchaseLocationCode = LineData.PurchaseLocationCode;                                     //仕入ロケーションコード

                    Employee EmployeeData = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode, false);
                    if (EmployeeData != null && !string.IsNullOrEmpty(EmployeeData.DepartmentCode))
                    {

                        CarPurchaseData.DepartmentCode = EmployeeData.DepartmentCode;                                         //部門コード
                    }
                    else
                    {
                        CarPurchaseData.DepartmentCode = "";
                    }
                    CarPurchaseData.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                              //担当者コード


                    if (string.IsNullOrEmpty(LineData.VehiclePrice))
                    {
                        CarPurchaseData.VehiclePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehiclePrice = decimal.Parse(LineData.VehiclePrice.Replace(",", ""));                     //車両本体価格
                    }

                    
                    if (string.IsNullOrEmpty(LineData.MetallicPrice))
                    {
                        CarPurchaseData.MetallicPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicPrice = decimal.Parse(LineData.MetallicPrice.Replace(",", ""));                   //メタリック価格
                    }

                    if (string.IsNullOrEmpty(LineData.OptionPrice))
                    {
                        CarPurchaseData.OptionPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionPrice = decimal.Parse(LineData.OptionPrice.Replace(",", ""));                       //オプション価格
                    }

                    if (string.IsNullOrEmpty(LineData.FirmPrice))
                    {
                        CarPurchaseData.FirmPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmPrice = decimal.Parse(LineData.FirmPrice.Replace(",", ""));                           //ファーム価格
                    }

                    if (string.IsNullOrEmpty(LineData.DiscountPrice))
                    {
                        CarPurchaseData.DiscountPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountPrice = decimal.Parse(LineData.DiscountPrice.Replace(",", ""));                   //ディスカウント価格
                    }

                    if (string.IsNullOrEmpty(LineData.EquipmentPrice))
                    {
                        CarPurchaseData.EquipmentPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.EquipmentPrice = decimal.Parse(LineData.EquipmentPrice.Replace(",", ""));                 //加装価格(税抜)
                    }

                    if (string.IsNullOrEmpty(LineData.RepairPrice))
                    {
                        CarPurchaseData.RepairPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.RepairPrice = decimal.Parse(LineData.RepairPrice.Replace(",", ""));                       //加修価格(税抜)
                    }

                    if (string.IsNullOrEmpty(LineData.OthersPrice))
                    {
                        CarPurchaseData.OthersPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersPrice = decimal.Parse(LineData.OthersPrice.Replace(",", ""));                       //その他価格
                    }


                    if (string.IsNullOrEmpty(LineData.Amount))
                    {
                        CarPurchaseData.Amount = 0;
                    }
                    else
                    {
                    CarPurchaseData.Amount = decimal.Parse(LineData.Amount.Replace(",", ""));                                 //仕入価格
                    }

                    if (string.IsNullOrEmpty(LineData.TaxAmount))
                    {
                        CarPurchaseData.TaxAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.TaxAmount = decimal.Parse(LineData.TaxAmount.Replace(",", ""));                           //消費税
                    }
                    
                    CarPurchaseData.SalesCarNumber = SalesCarData.SalesCarNumber;                                             //管理番号
                    CarPurchaseData.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                        //作成者
                    CarPurchaseData.CreateDate = DateTime.Now;                                                                //作成日時
                    CarPurchaseData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                    //最終更新者
                    CarPurchaseData.LastUpdateDate = DateTime.Now;                                                            //最終更新日時
                    CarPurchaseData.DelFlag = "0";                                                                            //削除フラグ
                    CarPurchaseData.EraseRegist = null;                                                                       //抹消登録
                    CarPurchaseData.Memo = LineData.Memo;                                                                     //備考

                    if (string.IsNullOrEmpty(LineData.SlipDate))
                    {
                        CarPurchaseData.SlipDate = null;
                    }
                    else
                    {
                    CarPurchaseData.SlipDate = DateTime.Parse(LineData.SlipDate);                                             //仕入日（店舗に車が入庫した日）
                    }
                    
                    CarPurchaseData.CarTaxAppropriateAmount = null;                                                           //自税充当税込
                    CarPurchaseData.RecycleAmount = null;                                                                     //リサイクル税込
                    CarPurchaseData.CarPurchaseType = "004";                                                                  //入庫区分

                    if (string.IsNullOrEmpty(LineData.VehicleTax))
                    {
                        CarPurchaseData.VehicleTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehicleTax = decimal.Parse(LineData.VehicleTax.Replace(",", ""));                         //車両本体消費税
                    }

                    if (string.IsNullOrEmpty(LineData.VehicleAmount))
                    {
                        CarPurchaseData.VehicleAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehicleAmount = decimal.Parse(LineData.VehicleAmount.Replace(",", ""));                   //車両本体税込価格
                    }

                    if (string.IsNullOrEmpty(LineData.MetallicTax))
                    {
                        CarPurchaseData.MetallicTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicTax = decimal.Parse(LineData.MetallicTax.Replace(",", ""));                       //メタリック消費税
                    }


                    if (string.IsNullOrEmpty(LineData.MetallicAmount))
                    {
                        CarPurchaseData.MetallicAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicAmount = decimal.Parse(LineData.MetallicAmount.Replace(",", ""));                 //メタリック税込価格
                    }
                    
                    if (string.IsNullOrEmpty(LineData.OptionTax))
                    {
                        CarPurchaseData.OptionTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionTax = decimal.Parse(LineData.OptionTax.Replace(",", ""));                           //オプション消費税
                    }


                    if (string.IsNullOrEmpty(LineData.OptionAmount))
                    {
                        CarPurchaseData.OptionAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionAmount = decimal.Parse(LineData.OptionAmount.Replace(",", ""));                     //オプション税込価格
                    }
                    
                    if (string.IsNullOrEmpty(LineData.FirmTax))
                    {
                        CarPurchaseData.FirmTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmTax = decimal.Parse(LineData.FirmTax.Replace(",", ""));                               //ファーム消費税
                    }
                    
                    if (string.IsNullOrEmpty(LineData.FirmAmount))
                    {
                        CarPurchaseData.FirmAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmAmount = decimal.Parse(LineData.FirmAmount.Replace(",", ""));                         //ファーム税込価格
                    }
                    
                    if (string.IsNullOrEmpty(LineData.DiscountTax))
                    {
                        CarPurchaseData.DiscountTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountTax = decimal.Parse(LineData.DiscountTax.Replace(",", ""));                       //ディスカウント消費税
                    }

                    if (string.IsNullOrEmpty(LineData.DiscountAmount))
                    {
                        CarPurchaseData.DiscountAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountAmount = decimal.Parse(LineData.DiscountAmount.Replace(",", ""));                 //ディスカウント税込価格
                    }

                    if (string.IsNullOrEmpty(LineData.OthersTax))
                    {
                        CarPurchaseData.OthersTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersTax = decimal.Parse(LineData.OthersTax.Replace(",", ""));                           //その他消費税
                    }

                    if (string.IsNullOrEmpty(LineData.OthersAmount))
                    {
                        CarPurchaseData.OthersAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersAmount = decimal.Parse(LineData.OthersAmount.Replace(",", ""));                     //その他税込価格
                    }

                    if (string.IsNullOrEmpty(LineData.TotalAmount))
                    {
                        CarPurchaseData.TotalAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.TotalAmount = decimal.Parse(LineData.TotalAmount.Replace(",", ""));                       //仕入税込価格
                    }
                    
                    if (string.IsNullOrEmpty(LineData.AuctionFeePrice))
                    {
                        CarPurchaseData.AuctionFeePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeePrice = decimal.Parse(LineData.AuctionFeePrice.Replace(",", ""));               //オークション落札料
                    }

                    if (string.IsNullOrEmpty(LineData.AuctionFeeTax))
                    {
                        CarPurchaseData.AuctionFeeTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeeTax = decimal.Parse(LineData.AuctionFeeTax.Replace(",", ""));                   //オークション落札料消費税
                    }

                    if (string.IsNullOrEmpty(LineData.AuctionFeeAmount))
                    {
                        CarPurchaseData.AuctionFeeAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeeAmount = decimal.Parse(LineData.AuctionFeeAmount.Replace(",", ""));             //オークション落札料税込
                    }

                    if (string.IsNullOrEmpty(LineData.CarTaxAppropriatePrice))
                    {
                        CarPurchaseData.CarTaxAppropriatePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.CarTaxAppropriatePrice = decimal.Parse(LineData.CarTaxAppropriatePrice.Replace(",", "")); //自税充当
                    }
                    
                    if (string.IsNullOrEmpty(LineData.RecyclePrice))
                    {
                        CarPurchaseData.RecyclePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.RecyclePrice = decimal.Parse(LineData.RecyclePrice.Replace(",", ""));                     //リサイクル
                    }
                    
                    CarPurchaseData.EquipmentTax = null;                                                                      //加装価格(消費税)
                    CarPurchaseData.EquipmentAmount = null;                                                                   //加装価格(税込)
                    CarPurchaseData.RepairTax = null;                                                                         //加修価格(消費税)
                    CarPurchaseData.RepairAmount = null;                                                                      //加修価格(税込)
                    CarPurchaseData.CarTaxAppropriateTax = null;                                                              
                    CarPurchaseData.ConsumptionTaxId = null;                                                                  //税率Id
                    CarPurchaseData.Rate = null;                                                                              //消費税率
                    CarPurchaseData.CancelFlag = null;                                                                        //キャンセルフラグ
                    CarPurchaseData.LastEditScreen = "000";                                                                   //最終更新画面


                    //Mod 2018/08/28 yano #3922
                    //Add 2018/06/06 arc yano #3883
                    ////財務価格をリサイクル料金を除いた全ての金額で更新
                    //if (!string.IsNullOrWhiteSpace(SalesCarData.NewUsedType) && SalesCarData.NewUsedType.Equals("U"))
                    //{

                    decimal purchaseprice = 0m;

                    purchaseprice = CarPurchaseData.VehiclePrice +                              //車両本体価格(税抜)
                                    (CarPurchaseData.AuctionFeePrice ?? 0m) +                   //オークション落札量(税抜)
                                    (CarPurchaseData.CarTaxAppropriatePrice ?? 0m) +            //自税充当(税抜)
                                    CarPurchaseData.OthersPrice +                               //その他価格(税抜)
                                    CarPurchaseData.MetallicPrice +                             //メタリック価格(税抜)
                                    CarPurchaseData.OptionPrice +                               //オプション価格(税抜)     
                                    CarPurchaseData.FirmPrice +                                 //ファーム価格(税抜)
                                    CarPurchaseData.DiscountPrice +                             //ディスカウント価格(税抜)
                                    CarPurchaseData.EquipmentPrice +                            //加装価格(税抜)
                                    CarPurchaseData.RepairPrice;                                //加修価格(税抜)


                    if (purchaseprice > 0)
                    {
                        CarPurchaseData.FinancialAmount = purchaseprice;
                    }
                    else
                    {
                        CarPurchaseData.FinancialAmount = 1m;
                    }

                    //仕入金額(税込)からリサイクル金額を除いた仕入金額(税抜)を取得
                    ////現時点の税率を初期で設定
                    //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                    //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(CarPurchaseData.ConsumptionTaxId);

                    ////仕入データに消費税率IDが設定されている場合はそちらを使う
                    //if (rate != null)
                    //{
                    //    taxrate = rate.Rate;
                    //}

                    //decimal value = (CarPurchaseData.TotalAmount ?? 0) - (CarPurchaseData.RecycleAmount ?? 0);

                    //if (value > 0)
                    //{
                    //    CarPurchaseData.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                    //}
                    //else
                    //{
                    //    if ((CarPurchaseData.TotalAmount ?? 0) > 0)
                    //    {
                    //        CarPurchaseData.FinancialAmount = CommonUtils.CalcAmountWithoutTax((CarPurchaseData.TotalAmount ?? 0), taxrate, 1);
                    //    }
                    //    else
                    //    {
                    //        CarPurchaseData.FinancialAmount = 1m;
                    //    }
                    //}

                    ////CarPurchaseData.FinancialAmount = CarPurchaseData.Amount;                                             //財務価格  
                //}

                CarPurchaseList.Add(CarPurchaseData);
               }

                db.SalesCar.InsertAllOnSubmit(SalesCarList);
                db.CarPurchase.InsertAllOnSubmit(CarPurchaseList);


                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    ModelState.AddModelError("", se.Message);   //Add 2018/02/15 arc yano #3865

                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);    //Add 2018/02/15 arc yano #3865

                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region キャンセルデータ作成
        /// <summary>
        /// キャンセルデータ作成
        /// </summary>
        /// <returns>キャンセルデータ</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 財務価格の更新処理の追加
        /// </history>
        private CarPurchase CreateCancelData(CarPurchase carPurchase)
        {
            //画面から取得できないデータは現在保存済みのデータからコピーする
            CarPurchase target = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

            CarPurchase CancelPurchase = new CarPurchase();
            CancelPurchase.CarPurchaseId = Guid.NewGuid();
            CancelPurchase.CarPurchaseOrderNumber = target.CarPurchaseOrderNumber;
            CancelPurchase.CarAppraisalId = carPurchase.CarAppraisalId;
            CancelPurchase.PurchaseStatus = "003";//仕入れキャンセル
            CancelPurchase.PurchaseDate = carPurchase.PurchaseDate;
            CancelPurchase.SupplierCode = carPurchase.SupplierCode;
            CancelPurchase.PurchaseLocationCode = carPurchase.PurchaseLocationCode;
            CancelPurchase.DepartmentCode = carPurchase.DepartmentCode;
            CancelPurchase.EmployeeCode = carPurchase.EmployeeCode;
            CancelPurchase.VehiclePrice = carPurchase.VehiclePrice * (-1);
            CancelPurchase.MetallicPrice = carPurchase.MetallicPrice * (-1);
            CancelPurchase.OptionPrice = carPurchase.OptionPrice * (-1);
            CancelPurchase.FirmPrice = carPurchase.FirmPrice * (-1);
            CancelPurchase.DiscountPrice = carPurchase.DiscountPrice * (-1);
            CancelPurchase.EquipmentPrice = carPurchase.EquipmentPrice * (-1);
            CancelPurchase.RepairPrice = carPurchase.RepairPrice * (-1);
            CancelPurchase.OthersPrice = carPurchase.OthersPrice * (-1);
            CancelPurchase.Amount = carPurchase.Amount * (-1);
            CancelPurchase.TaxAmount = carPurchase.TaxAmount * (-1);
            CancelPurchase.SalesCarNumber = carPurchase.SalesCar.SalesCarNumber;
            CancelPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            CancelPurchase.CreateDate = DateTime.Now;
            CancelPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            CancelPurchase.LastUpdateDate = DateTime.Now;
            CancelPurchase.DelFlag = "0";
            CancelPurchase.EraseRegist = carPurchase.SalesCar.EraseRegist;
            CancelPurchase.Memo = carPurchase.Memo;
            CancelPurchase.SlipDate = carPurchase.SlipDate;
            CancelPurchase.CarTaxAppropriateAmount = carPurchase.CarTaxAppropriateAmount * (-1);
            CancelPurchase.RecycleAmount = carPurchase.RecycleAmount * (-1);
            CancelPurchase.CarPurchaseType = carPurchase.CarPurchaseType;
            CancelPurchase.VehicleTax = carPurchase.VehicleTax * (-1);
            CancelPurchase.VehicleAmount = carPurchase.VehicleAmount * (-1);
            CancelPurchase.MetallicTax = carPurchase.MetallicTax * (-1);
            CancelPurchase.MetallicAmount = carPurchase.MetallicAmount * (-1);
            CancelPurchase.OptionTax = carPurchase.OptionTax * (-1);
            CancelPurchase.OptionAmount = carPurchase.OptionAmount * (-1);
            CancelPurchase.FirmTax = carPurchase.FirmTax * (-1);
            CancelPurchase.FirmAmount = carPurchase.FirmAmount * (-1);
            CancelPurchase.DiscountTax = carPurchase.DiscountTax * (-1);
            CancelPurchase.DiscountAmount = carPurchase.DiscountAmount * (-1);
            CancelPurchase.OthersTax = carPurchase.OthersTax * (-1);
            CancelPurchase.OthersAmount = carPurchase.OthersAmount * (-1);
            CancelPurchase.TotalAmount = carPurchase.TotalAmount * (-1);
            CancelPurchase.AuctionFeePrice = carPurchase.AuctionFeePrice * (-1);
            CancelPurchase.AuctionFeeTax = carPurchase.AuctionFeeTax * (-1);
            CancelPurchase.AuctionFeeAmount = carPurchase.AuctionFeeAmount * (-1);
            CancelPurchase.CarTaxAppropriatePrice = carPurchase.CarTaxAppropriatePrice * (-1);
            CancelPurchase.RecyclePrice = carPurchase.RecyclePrice * (-1);
            CancelPurchase.EquipmentTax = carPurchase.EquipmentTax * (-1);
            CancelPurchase.EquipmentAmount = carPurchase.EquipmentAmount * (-1);
            CancelPurchase.RepairTax = carPurchase.RepairTax * (-1);
            CancelPurchase.RepairAmount = carPurchase.RepairAmount * (-1);
            CancelPurchase.CarTaxAppropriateTax = carPurchase.CarTaxAppropriateTax * (-1);
            CancelPurchase.ConsumptionTaxId = carPurchase.ConsumptionTaxId;
            CancelPurchase.Rate = carPurchase.Rate;
            CancelPurchase.CancelFlag = carPurchase.CancelFlag;
            CancelPurchase.SlipNumber = carPurchase.SlipNumber;
            CancelPurchase.LastEditScreen = carPurchase.LastEditScreen;
            //CancelPurchase.RegistOwnFlag = carPurchase.RegistOwnFlag;
            CancelPurchase.CancelDate = carPurchase.CancelDate;
            CancelPurchase.CancelMemo = carPurchase.CancelMemo;

            //Add 2018/06/06 arc yano #3883
            CancelPurchase.FinancialAmount = carPurchase.FinancialAmount * (-1); ;     //財務価格

            db.CarPurchase.InsertOnSubmit(CancelPurchase);

            return CancelPurchase;

        }
        #endregion

        #region キャンセルのバリデーション
        /// <summary>
        /// キャンセルのバリデーション
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2019/02/07 yano #3960 車両仕入入力　入庫区分＝「下取車」の未入庫の仕入データの削除ができない
        /// 2017/08/10 arc nakayama #3782_車両仕入_キャンセル機能追加
        /// </history>
        private void Cancelvalidation(CarPurchase carPurchase)
        {
            CarPurchase targetData = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
            
            if (targetData != null)
            {
                //if (!string.IsNullOrEmpty(targetData.CarPurchaseType) && (targetData.CarPurchaseType.Equals("001") || targetData.CarPurchaseType.Equals("005"))) 
                if (!string.IsNullOrEmpty(targetData.CarPurchaseType) && (targetData.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN) || targetData.CarPurchaseType.Equals(PURCHASETYPE_DIPOSAL))) 
                {
                    ModelState.AddModelError("CarPurchaseType", "下取車または依廃で入庫確定をした車両は仕入キャンセルを行うことができません");
                }
            }

            //車両マスタのステータス
            if (!string.IsNullOrEmpty(carPurchase.SalesCarNumber))
            {
                SalesCar SalesCarData = new SalesCarDao(db).GetByKey(carPurchase.SalesCarNumber);
                if (!SalesCarData.CarStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "該当の車両は在庫として存在していないため、仕入れキャンセルを行うことができません");
                }
            }
            else
            {
                //管理番号が未登録ならチェックできない
            }

            //キャンセルメモ(文字数チェック)
            if (!string.IsNullOrEmpty(carPurchase.CancelMemo))
            {
                if (carPurchase.CancelMemo.Length > 100)
                {
                    ModelState.AddModelError("CancelMemo", "キャンセルメモは100文字以内で入力して下さい（改行は2文字分とみなされます）");
                }
            }

            //キャンセル日の絞めチェック
            if (carPurchase.CancelDate == null)
            {
                ModelState.AddModelError("CancelDate", "キャンセルを行う場合、キャンセル日は必須です");
            }
            else
            {
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.CancelDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("CancelDate", "月次締め処理が終了しているので指定された日付ではキャンセルを行うことができません");
                }
            }


        }
        #endregion

        #region 削除処理
        /// <summary>
        /// 削除時のバリデーション
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2019/02/07 yano #3960 車両仕入入力　入庫区分＝「下取車」の未入庫の仕入データの削除ができない
        /// 2017/08/21 arc yano #3791 車両仕入 仕入削除機能の追加
        /// </history>
        private void Deletevalidation(CarPurchase carPurchase)
        {
            //仕入れ区分
            CarPurchase targetData = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

            //Mod 2019/02/07 yano #3960
            if (targetData != null)
            {
                if (!string.IsNullOrWhiteSpace(targetData.CarPurchaseType))
                {
                    //下取車
                    if(targetData.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN))
                    {
                        int datecnt = new CarPurchaseDao(db).GetListBySlipNumberVin(targetData.SlipNumber, targetData.SalesCar.Vin).Where(x => x.PurchaseStatus.Equals("002") && !x.CarPurchaseId.Equals(targetData.CarPurchaseId)).ToList().Count;

                        //対象車両ですでに仕入済データが存在しない場合はエラー
                        if (datecnt == 0)
                        {
                            ModelState.AddModelError("CarPurchaseType", "入庫区分が下取車で登録されている仕入データは削除できません。入庫区分を下取車、依廃以外で登録後、再度削除処理を実行して下さい");
                        }
                    }
                    //依廃
                    else if (targetData.CarPurchaseType.Equals(PURCHASETYPE_DIPOSAL))
                    {
                         ModelState.AddModelError("CarPurchaseType", "入庫区分が依廃で登録されている仕入データは削除できません。入庫区分を下取車、依廃以外で登録後、再度削除処理を実行して下さい");
                    }
                }
            }
        }
        #endregion

    }
}
