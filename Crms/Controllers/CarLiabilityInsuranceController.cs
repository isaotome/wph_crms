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
    public class CarLiabilityInsuranceController : InheritedController
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "自賠責保険料マスタ";     // 画面名
        private static readonly string PROC_NAME = "自賠責保険料マスタ登録"; // 処理名

        private static readonly string APPLICATIONCODE_CARMASTEREDIT = "CarMasterEdit";         //Add 2022/01/27 yano #4125

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarLiabilityInsuranceController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 自賠責保険料検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 自賠責保険料検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>自賠責保険料検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            List<CarLiabilityInsurance> list = new CarLiabilityInsuranceDao(db).GetListAll();
            return View("CarLiabilityInsuranceCriteria", list);
        }

        /// <summary>
        /// 自賠責保険料検索ダイアログ表示
        /// </summary>
        /// <returns>自賠責保険料検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            List<CarLiabilityInsurance> list = new CarLiabilityInsuranceDao(db).GetListAll();
            return View("CarLiabilityInsuranceCriteriaDialog", list);
        }

        /// <summary>
        /// 自賠責保険料入力画面表示
        /// </summary>
        /// <param name="id">自賠責保険料ID</param>
        /// <returns>自賠責保険料入力画面</returns>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CarLiabilityInsurance insurance = new CarLiabilityInsurance();
            if (!string.IsNullOrEmpty(id)) {
                insurance = new CarLiabilityInsuranceDao(db).GetByKey(new Guid(id));
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
            return View("CarLiabilityInsuranceEntry", insurance);
        }

        /// <summary>
        /// 自賠責保険料登録処理
        /// </summary>
        /// <param name="insurance">自賠責保険料データ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>自賠責保険料入力画面</returns>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarLiabilityInsurance insurance, FormCollection form) {
            ViewData["update"] = form["update"];
            ValidateCarLiabilityInsurance(insurance);
            if(!ModelState.IsValid){
                GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
                return View("CarLiabilityInsuranceEntry",insurance);
            }
            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            if (form["update"].Equals("1")) {
                CarLiabilityInsurance target = new CarLiabilityInsuranceDao(db).GetByKey(insurance.CarLiabilityInsuranceId);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(insurance);
                db.CarLiabilityInsurance.InsertOnSubmit(insurance);
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
                        GetEntryViewData(insurance);			//Add 2022/01/27 yano #4125
                        return View("CarLiabilityInsuranceEntry", insurance);
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
            //return View("CarLiabilityInsuranceEntry",insurance);
            return Entry(insurance.CarLiabilityInsuranceId.ToString());
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="insurance">自賠責保険料データ</param>
        private void ValidateCarLiabilityInsurance(CarLiabilityInsurance insurance){
            CommonValidate("CarLiabilityInsuranceName", "表示名", insurance, true);
            CommonValidate("Amount","金額(円)",insurance,true);
            if (!string.IsNullOrEmpty(insurance.NewDefaultFlag) && insurance.NewDefaultFlag.Contains("true")) {
                CarLiabilityInsurance carLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (carLiabilityInsurance != null
                    && !CommonUtils.DefaultString(carLiabilityInsurance.CarLiabilityInsuranceId).Equals(CommonUtils.DefaultString(insurance.CarLiabilityInsuranceId)))
                {
                    ModelState.AddModelError("NewDefaultFlag", "他の金額でデフォルト設定されているためこのデータはデフォルト設定にはできません");
                }
            }
            if (!string.IsNullOrEmpty(insurance.UsedDefaultFlag) && insurance.UsedDefaultFlag.Contains("true")) {
                CarLiabilityInsurance carLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                if (carLiabilityInsurance != null && !CommonUtils.DefaultString(carLiabilityInsurance.CarLiabilityInsuranceId).Equals(CommonUtils.DefaultString(insurance.CarLiabilityInsuranceId)))
                {
                    ModelState.AddModelError("UsedDefaultFlag", "他の金額でデフォルト設定されているためこのデータはデフォルト設定にはできません。");
                }
            }
        }

        /// <summary>
        /// 自賠責保険料マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="insurance">自賠責保険料データ</param>
        /// <returns>自賠責保険料データ</returns>
        private CarLiabilityInsurance EditForUpdate(CarLiabilityInsurance insurance) {
            insurance.LastUpdateDate = DateTime.Now;
            insurance.LastUpdateEmployee = ((Employee)Session["Employee"]).EmployeeCode;
            if (!string.IsNullOrEmpty(insurance.NewDefaultFlag)) {
                if (insurance.NewDefaultFlag.Contains("true")) {
                    insurance.NewDefaultFlag = "1";
                } else {
                    insurance.NewDefaultFlag = "0";
                }
            } else {
                insurance.NewDefaultFlag = "0";
            }

            if (!string.IsNullOrEmpty(insurance.UsedDefaultFlag)) {
                if (insurance.UsedDefaultFlag.Contains("true")) {
                    insurance.UsedDefaultFlag = "1";
                } else {
                    insurance.UsedDefaultFlag = "0";
                }
            } else {
                insurance.UsedDefaultFlag = "0";
            }
            return insurance;
        }

        /// <summary>
        /// 自賠責保険料マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">自賠責保険料データ</param>
        /// <returns>自賠責保険料データ</returns>
        private CarLiabilityInsurance EditForInsert(CarLiabilityInsurance insurance) {
            insurance.CarLiabilityInsuranceId = Guid.NewGuid();
            insurance.CreateDate = DateTime.Now;
            insurance.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            insurance.DelFlag = "0";
            return EditForUpdate(insurance);
        }
        
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="insurance">モデルデータ</param>
        /// <histroy
        /// 2022/01/27 yano #4125【自動車税関連マスタ】権限による保存機能の制限の実装
        /// </histroy>
        private void GetEntryViewData(CarLiabilityInsurance insurance)
        {     
            //車両マスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_CARMASTEREDIT).EnableFlag;

        }

    }
}
