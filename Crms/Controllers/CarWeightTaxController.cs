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
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarWeightTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "自動車重量税マスタ";     // 画面名
        private static readonly string PROC_NAME = "自動車重量税マスタ登録"; // 処理名

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarWeightTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 自動車重量税検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 自動車重量税検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車重量税検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarWeightTax> list = new CarWeightTaxDao(db).GetListAll();
            return View("CarWeightTaxCriteria", list);
        }

        /// <summary>
        /// 自動車重量税検索ダイアログ表示
        /// </summary>
        /// <returns>自動車重量税検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            List<CarWeightTax> list = new CarWeightTaxDao(db).GetListAll();
            return View("CarWeightTaxCriteriaDialog", list);
        }

        /// <summary>
        /// 自動車重量税入力画面表示
        /// </summary>
        /// <param name="id">自動車税ID</param>
        /// <returns>自動車税入力画面</returns>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarWeightTax carWeightTax = new CarWeightTax();
            if (!string.IsNullOrEmpty(id)) {
                carWeightTax = new CarWeightTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
            return View("CarWeightTaxEntry", carWeightTax);
        }

        /// <summary>
        /// 自動車重量税登録処理
        /// </summary>
        /// <param name="carTax">自動車重量税データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車重量税入力画面</returns>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarWeightTax carWeightTax, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarWeightTax(carWeightTax);
            if(!ModelState.IsValid){
                GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                return View("CarWeightTaxEntry",carWeightTax);
            }

            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CarWeightTax target = new CarWeightTaxDao(db).GetByKey(carWeightTax.CarWeightTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{

                CarWeightTax tax = new CarWeightTaxDao(db).GetByWeight(carWeightTax.InspectionYear, carWeightTax.WeightFrom);
                if (tax != null) {
                    ModelState.AddModelError("WeightFrom", "登録済みの設定と重複しています");
                }

                tax = new CarWeightTaxDao(db).GetByWeight(carWeightTax.InspectionYear, carWeightTax.WeightTo);
                if (tax != null) {
                    ModelState.AddModelError("WeightTo", "登録済みの設定と重複しています");
                }
                if (!ModelState.IsValid) {
                    GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                    return View("CarWeightTaxEntry", carWeightTax);
                }
                EditForInsert(carWeightTax);
                db.CarWeightTax.InsertOnSubmit(carWeightTax);
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
                    // Add 2014/08/04 arc amii エラーログ対応 ChangeConflictException処理追加
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
                        GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
                        return View("CarWeightTaxEntry", carWeightTax);
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
            GetEntryViewData(carWeightTax); 		//Add 2022/01/27 yano #4125
            return View("CarWeightTaxEntry",carWeightTax);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="carTax">自動車重量税データ</param>
        private void ValidateCarWeightTax(CarWeightTax carWeightTax){
            //CommonValidate("CarWeightTaxName", "表示名", carWeightTax, true);
            CommonValidate("Amount","金額(円)",carWeightTax,true);
            CommonValidate("InspectionYear", "車検年数", carWeightTax, true);
            CommonValidate("WeightFrom", "重量(kg)", carWeightTax, true);
            CommonValidate("WeightTo", "重量(kg)", carWeightTax, true);
            if (carWeightTax.InspectionYear <= 0) {
                ModelState.AddModelError("InspectionYear", MessageUtils.GetMessage("E0004", new string[] { "車検年数", "1以上の正の整数" }));
            }
            if (carWeightTax.WeightFrom >= carWeightTax.WeightTo) {
                ModelState.AddModelError("WeightFrom", "重量(kg)の範囲が不正です");
            }
            if (carWeightTax.WeightFrom < 0) {
                ModelState.AddModelError("WeightFrom", MessageUtils.GetMessage("E0004", new string[] { "重量(kg)", "正の整数" }));
            }
            if (carWeightTax.WeightTo < 0) {
                ModelState.AddModelError("WeightTo", MessageUtils.GetMessage("E0004", new string[] { "重量(kg)", "正の整数" }));
            }
        }

        /// <summary>
        /// 自動車重量税マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車重量税データ</param>
        /// <returns>自動車重量税データ</returns>
        private CarWeightTax EditForUpdate(CarWeightTax carWeightTax) {
            carWeightTax.LastUpdateDate = DateTime.Now;
            carWeightTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carWeightTax;
        }

        /// <summary>
        /// 自動車重量税マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車重量税データ</param>
        /// <returns>自動車重量税データ</returns>
        private CarWeightTax EditForInsert(CarWeightTax carWeightTax) {
            carWeightTax.CarWeightTaxId = Guid.NewGuid();
            carWeightTax.CreateDate = DateTime.Now;
            carWeightTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carWeightTax.DelFlag = "0";
            return EditForUpdate(carWeightTax);
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carWeightTax">モデルデータ</param>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        private void GetEntryViewData(CarWeightTax carWeightTax)
        {     
            //車両マスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }

    }
}
