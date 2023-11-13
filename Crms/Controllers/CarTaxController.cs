using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Data.Linq;                 //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 自動車税種別割マスタクラス
    /// </summary>
    /// <history>
    /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
    /// </history>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "自動車税種別割マスタ";     // 画面名        //Mod 2019/09/04 yano #4011
        private static readonly string PROC_NAME = "自動車税種別割マスタ登録"; // 処理名        //Mod 2019/09/04 yano #4011

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 自動車税種別割検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 自動車税種別割検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車税検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarTax> list = new CarTaxDao(db).GetListAll();
            return View("CarTaxCriteria", list);
        }

        /// <summary>
        /// 自動車税種別割検索ダイアログ表示
        /// </summary>
        /// <returns>自動車税種別割検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            List<CarTax> list = new CarTaxDao(db).GetListAll();
            return View("CarTaxCriteriaDialog", list);
        }

        /// <summary>
        /// 自動車税種別割入力画面表示
        /// </summary>
        /// <param name="id">自動車税種別割ID</param>
        /// <returns>自動車税種別割入力画面</returns>
        /// <history>
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarTax carTax = new CarTax();
            if (!string.IsNullOrEmpty(id)) {
                carTax = new CarTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125

            return View("CarTaxEntry", carTax);
        }

        /// <summary>
        /// 自動車税種別割登録処理
        /// </summary>
        /// <param name="carTax">自動車税種別割データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車税種別割入力画面</returns>
         /// <history>
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarTax carTax, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarTax(carTax);
            if(!ModelState.IsValid){
                GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
                return View("CarTaxEntry",carTax);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CarTax target = new CarTaxDao(db).GetByKey(carTax.CarTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(carTax);
                db.CarTax.InsertOnSubmit(carTax);
            }

            // Mod 2014/08/04 arc amii エラーログ対応 他Controllerとcatch句を合わせるよう修正
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
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
                    //Add 2014/08/04 arc amii エラーログ対応『throw e』からエラーログを出力する処理に変更
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/04 arc amii エラーログ対応 エラーログ出力処理追加
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "保存"));
                        GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
                        return View("CarTaxEntry", carTax);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
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
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            ViewData["update"] = "1";
            GetEntryViewData(carTax);   //Add 2022/01/27 yano #4125
            return View("CarTaxEntry",carTax);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="carTax">自動車税種別割データ</param>
        /// <history>
        /// 2019/10/21 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り
        /// </history>
        private void ValidateCarTax(CarTax carTax){
            //ADD 2014/06/09 arc uchida フォーマットチェックの追加
            if (!ModelState.IsValidField("RegistMonth"))
            {
                ModelState.AddModelError("RegistMonth", MessageUtils.GetMessage("E0004", new string[] { "登録月", "正の整数のみ" }));
            }
            else {
                CommonValidate("RegistMonth", "登録月", carTax, true);
            }
            
            CommonValidate("CarTaxName", "表示名", carTax, true);
            CommonValidate("Amount","金額(円)",carTax,true);

            //Add 2019/10/21 yano #4023
            CommonValidate("FromAvailableDate", "適用日FROM", carTax, true);
            CommonValidate("ToAvailableDate", "適用日TO", carTax, true);

            if (ModelState["FromAvailableDate"].Errors.Count > 1)
            {
                ModelState["FromAvailableDate"].Errors.RemoveAt(0);
            }
            if(ModelState["ToAvailableDate"].Errors.Count > 1)
            {
                ModelState["ToAvailableDate"].Errors.RemoveAt(0);
            }
               

            //CommonValidate("RegistMonth", "登録月", carTax, true);
        }

        /// <summary>
        /// 自動車税種別割マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車税種別割データ</param>
        /// <returns>自動車税種別割データ</returns>
        private CarTax EditForUpdate(CarTax carTax) {
            carTax.LastUpdateDate = DateTime.Now;
            carTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carTax;
        }

        /// <summary>
        /// 自動車税種別割マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車税種別割データ</param>
        /// <returns>自動車税種別割データ</returns>
        private CarTax EditForInsert(CarTax carTax) {
            carTax.CarTaxId = Guid.NewGuid();
            carTax.CreateDate = DateTime.Now;
            carTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carTax.DelFlag = "0";
            return EditForUpdate(carTax);
        }
        
        /// <summary>
        /// グレードコードと登録希望日から自動車税種別割金額を取得する(Ajax専用）
        /// </summary>
        /// <param name="carGradeCode">グレードコード</param>
        /// <param name="requestRegistDate">登録希望日</param>
        /// <param name="vin">車台番号</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2019/10/29 yano #4024 【車両伝票入力】オプション行追加・削除時にエラー発生した時の不具合対応
        /// 2019/10/17 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業 
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetCarTax(string carGradeCode, string requestRegistDate, string vin ) {
            if (Request.IsAjaxRequest()) {

                //Mod 2019/10/17 yano #4023
                DateTime firstrequestRegistDate = DateTime.Today;
                SalesCar sc = new SalesCar();

                //車台番号で車両マスタを検索して、初年度登録を設定する
                if (!string.IsNullOrWhiteSpace(vin))
                {
                    sc = new SalesCarDao(db).GetByVin(vin).FirstOrDefault();

                    firstrequestRegistDate = (sc != null ? (sc.FirstRegistrationDate ?? DateTime.Today) : DateTime.Today);
                }
               

                CarGrade carGrade = new CarGradeDao(db).GetByKey(carGradeCode);

                try {
                    int registMonth = DateTime.Parse(requestRegistDate).Month;
                    if (carGradeCode != null) {
                        //Add 2019/09/04 yano #4011 //電気の場合は排気量1.0以下と同じ
                        if (!string.IsNullOrWhiteSpace(carGrade.VehicleType) && carGrade.VehicleType.Equals("002"))
                        {
                            carGrade.Displacement = 1m;
                        }
                        //Mod 2019/10/29 yano #4024
                        //Mod 2019/10/17 yano #4023
                        CarTax carTax = new CarTaxDao(db).GetByDisplacement(sc.Displacement != null ? (sc.Displacement ?? 0m) : (carGrade.Displacement ?? 0m), registMonth, firstrequestRegistDate);
                        //CarTax carTax = new CarTaxDao(db).GetByDisplacement(carGrade.Displacement ?? 0m, registMonth, firstrequestRegistDate);
                        
                        return Json(carTax.Amount);
                    }
                } catch {
                }
            }
            return new EmptyResult();
        }

         /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="serviceWork">モデルデータ</param>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        private void GetEntryViewData(CarTax carTax)
        {     
            //車両マスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }
    }
}
