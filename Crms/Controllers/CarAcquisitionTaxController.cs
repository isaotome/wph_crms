using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;                 //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
using Crms.Models;                      //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarAcquisitionTaxController : InheritedController
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "自動車税環境性能割マスタ";     // 画面名      //Mod 2019/09/04 yano #4011
        private static readonly string PROC_NAME = "自動車税環境性能割マスタ登録"; // 処理名      //Mod 2019/09/04 yano #4011

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarAcquisitionTaxController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 自動車税環境性能割検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 自動車税環境性能割検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車税環境性能割検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarAcquisitionTax> list = new CarAcquisitionTaxDao(db).GetListAll();
            return View("CarAcquisitionTaxCriteria", list);
        }

        /// <summary>
        /// 自動車税環境性能割入力画面表示
        /// </summary>
        /// <param name="id">自動車税環境性能割ID</param>
        /// <returns>自動車税環境性能割入力画面</returns>
        /// <history>
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarAcquisitionTax carTax = new CarAcquisitionTax();
            if (!string.IsNullOrEmpty(id)) {
                carTax = new CarAcquisitionTaxDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }
            GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
            return View("CarAcquisitionTaxEntry", carTax);
        }

        /// <summary>
        /// 自動車税環境性能割登録処理
        /// </summary>
        /// <param name="carTax">自動車税環境性能割データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>自動車税環境性能割入力画面</returns>
        /// <history>
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarAcquisitionTax carTax, FormCollection form) {
            ViewData["update"] = form["update"];

            // Add 2014/08/01 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                ValidateCarTax(carTax);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                    return View("CarAcquisitionTaxEntry", carTax);
                }
                CarAcquisitionTax target = new CarAcquisitionTaxDao(db).GetByKey(carTax.CarAcquisitionTaxId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                CarAcquisitionTax target = new CarAcquisitionTaxDao(db).GetByValue(carTax.ElapsedYears);
                if (target != null) {
                    ModelState.AddModelError("ElapsedYears", "指定された経過年数は既に登録されています");
                    if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                        ModelState["ElapsedYears"].Errors.RemoveAt(0);
                    }
                }
                ValidateCarTax(carTax);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                    return View("CarAcquisitionTaxEntry", carTax);
                }
                EditForInsert(carTax);
                db.CarAcquisitionTax.InsertOnSubmit(carTax);
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

                        GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
                        
                        return View("CarAcquisitionTaxEntry", carTax);
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
            
            GetEntryViewData(carTax);           //Add 2022/01/27 yano #4125
            return View("CarAcquisitionTaxEntry", carTax);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="carTax">自動車環境性能割データ</param>
        private void ValidateCarTax(CarAcquisitionTax carTax) {
            //CommonValidate("ElapsedYears", "経過年数", carTax, true);
            //CommonValidate("RemainRate","残価率",carTax,true);

            if (!ModelState.IsValidField("ElapsedYears")) {
                ModelState.AddModelError("ElapsedYears", "経過年数は0以上100未満かつ少数1桁以内（入力必須）です");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("ElapsedYears") &&
                (Regex.IsMatch(carTax.ElapsedYears.ToString(), @"^\d{1,2}\.\d{1,1}$")
                        || (Regex.IsMatch(carTax.ElapsedYears.ToString(), @"^\d{1,1}$")))) {
            } else {
                ModelState.AddModelError("ElapsedYears", "経過年数は0以上100未満かつ少数1桁以内（入力必須）です");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }
            if (carTax.ElapsedYears < 0 || carTax.ElapsedYears >= 100) {
                ModelState.AddModelError("ElapsedYears", "経過年数は0以上100未満かつ少数1桁以内（入力必須）です");
                if (ModelState["ElapsedYears"].Errors.Count() > 1) {
                    ModelState["ElapsedYears"].Errors.RemoveAt(0);
                }
            }

            if (!ModelState.IsValidField("RemainRate")) {
                ModelState.AddModelError("RemainRate", "残価率は0以上10未満かつ少数3桁以内（入力必須）です");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("RemainRate") &&
                (Regex.IsMatch(carTax.RemainRate.ToString(), @"^\d{1,2}\.\d{1,3}$")
                        || (Regex.IsMatch(carTax.RemainRate.ToString(), @"^\d{1,3}$")))) {
            } else {
                ModelState.AddModelError("RemainRate", "残価率は0以上10未満かつ少数3桁以内（入力必須）です");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }


            if (carTax.RemainRate < 0 || carTax.RemainRate >= 10) {
                ModelState.AddModelError("RemainRate", "残価率は0以上10未満かつ少数3桁以内（入力必須）です");
                if (ModelState["RemainRate"].Errors.Count() > 1) {
                    ModelState["RemainRate"].Errors.RemoveAt(0);
                }
            }


        }

        /// <summary>
        /// 自動車税環境性能割マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車税環境性能割データ</param>
        /// <returns>自動車税環境性能割データ</returns>
        private CarAcquisitionTax EditForUpdate(CarAcquisitionTax carTax) {
            carTax.LastUpdateDate = DateTime.Now;
            carTax.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return carTax;
        }

        /// <summary>
        /// 自動車税環境性能割マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自動車税環境性能割データ</param>
        /// <returns>自動車税環境性能割データ</returns>
        private CarAcquisitionTax EditForInsert(CarAcquisitionTax carTax) {
            carTax.CarAcquisitionTaxId = Guid.NewGuid();
            carTax.CreateDate = DateTime.Now;
            carTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carTax.DelFlag = "0";
            return EditForUpdate(carTax);
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="carAcquisitionTax">モデルデータ</param>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        private void GetEntryViewData(CarAcquisitionTax carAcquisitionTax)
        {     
            //車両マスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }
    }
}
