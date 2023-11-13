using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;                 //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CostAreaController : InheritedController
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "諸費用設定エリア";     // 画面名
        private static readonly string PROC_NAME = "諸費用設定エリア登録"; // 処理名

        private static readonly string APPLICATIONCODE_MASTEREDIT = "MasterEdit";         //Add 2022/01/27 yano #4126

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CostAreaController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 諸費用設定エリア検索画面表示
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 諸費用設定エリア検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>諸費用設定エリア検索結果</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            CostArea condition = new CostArea();
            PaginatedList<CostArea> list = new CostAreaDao(db).GetListByCondition(condition,int.Parse(form["id"] ?? "0"),DaoConst.PAGE_SIZE);
            return View("CostAreaCriteria", list);
        }
        /// <summary>
        /// 諸費用設定エリア検索ダイアログ表示
        /// </summary>
        /// <returns>諸費用設定エリア検索ダイアログ</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 諸費用設定エリア検索ダイアログ検索処理
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>諸費用設定エリア検索結果</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            CostArea condition = new CostArea();
            PaginatedList<CostArea> list = new CostAreaDao(db).GetListByCondition(condition,int.Parse(form["id"] ?? "0"),DaoConst.PAGE_SIZE);
            return View("CostAreaCriteriaDialog", list);
        }
        /// <summary>
        /// 諸費用設定エリア入力画面表示
        /// </summary>
        /// <param name="id">諸費用設定エリアコード</param>
        /// <returns>諸費用設定エリア入力画面</returns>
        /// <histroy>
        /// 2022/01/27 yano #4126【諸費用設定エリア】権限による保存機能の制限の実装
        /// </histroy>
        [AuthFilter]
        public ActionResult Entry(string id) {
            CostArea area = new CostArea();
            if (!string.IsNullOrEmpty(id)) {
                area = new CostAreaDao(db).GetByKey(id);
                ViewData["update"] = "1";
            } else {
                ViewData["update"] = "0";
            }

            GetEntryViewData(area);			//Add 2022/01/27 yano #4126
            return View("CostAreaEntry", area);
        }

        /// <summary>
        /// 諸費用設定エリア登録処理
        /// </summary>
        /// <param name="area">諸費用設定エリアデータ</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>諸費用設定エリア入力画面</returns>
        /// <history>
        /// 2022/01/27 yano #4126【諸費用設定エリア】権限による保存機能の制限の実装
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CostArea area, FormCollection form) {

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }
            
            ViewData["update"] = form["update"];
            ValidateCostArea(area);
            if(!ModelState.IsValid){
                GetEntryViewData(area);			//Add 2022/01/27 yano #4126
                return View("CostAreaEntry",area);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (form["update"].Equals("1")) {
                CostArea target = new CostAreaDao(db).GetByKey(area.CostAreaCode);
                UpdateModel(target);
                EditForUpdate(target);
            }else{
                EditForInsert(area);
                db.CostArea.InsertOnSubmit(area);
            }

            //Mod 2014/08/05 arc amii エラーログ対応 SubmitChangesのtry catch処理を修正
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    // DBアクセスの実行
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        ModelState.AddModelError("CampaignCode", MessageUtils.GetMessage("E0010", new string[] { "諸費用設定エリアコード", "保存" }));
                        GetEntryViewData(area);			//Add 2022/01/27 yano #4126
                        return View("CostAreaEntry", area);
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
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            ViewData["update"] = "1";
            GetEntryViewData(area);			//Add 2022/01/27 yano #4126
            return View("CostAreaEntry", area);
        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <history>
        /// 2023/10/05 yano #4184【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除
        /// </history>
        /// <param name="area">諸費用設定エリアデータ</param>
        private void ValidateCostArea(CostArea area) {

            //Mod 2023/10/05 yano #4184
            CommonValidate("CostAreaCode", "諸費用設定エリアコード", area, true);
            CommonValidate("CostAreaName", "諸費用設定エリア名", area, true);
            CommonValidate("RequestNumberCost", "ナンバープレート代(希望)", area, false);
            CommonValidate("ParkingSpaceCost", "車庫証明証紙代", area, false);
            CommonValidate("InspectionRegistFeeWithTax", "検査登録手続代行費用", area, false);
            CommonValidate("TradeInFeeWithTax", "下取車所有権解除手続費用", area, false);
            CommonValidate("PreparationFeeWithTax", "納車費用", area, false);
            CommonValidate("RequestNumberFeeWithTax", "希望ナンバー申請手数料", area, false);
            CommonValidate("OutJurisdictionRegistFeeWithTax", "管轄外登録手続費用", area, false);
            CommonValidate("FarRegistFeeWithTax", "県外登録手続代行費用", area, false);
            CommonValidate("ParkingSpaceFeeWithTax", "車庫証明手続代行費用", area, false);
            CommonValidate("RecycleControlFeeWithTax", "リサイクル資金管理料", area, false);

            //CommonValidate("CostAreaCode", "諸費用設定エリアコード", area, true);
            //CommonValidate("CostAreaName", "諸費用設定エリア名", area, true);
            //CommonValidate("RequestNumberCost", "ナンバープレート代(希望)", area, false);
            //CommonValidate("ParkingSpaceCost", "車庫証明証紙代", area, false);
            //CommonValidate("InspectionRegistFee", "検査・登録手続代行費用", area, false);
            //CommonValidate("TradeInFee", "下取車諸手続費用", area, false);
            //CommonValidate("PreparationFee", "納車準備費用", area, false);
            //CommonValidate("AppraisalFee", "下取車査定費用", area, false);
            ////CommonValidate("RecycleControlFee", "リサイクル資金管理料", area, false);
            //CommonValidate("RequestNumberFee", "希望ナンバー申請手数料", area, false);
        }

        /// <summary>
        /// 諸費用設定エリア更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">諸費用設定エリアデータ</param>
        /// <returns>諸費用設定エリアデータ</returns>
        private CostArea EditForUpdate(CostArea area) {
            area.LastUpdateDate = DateTime.Now;
            area.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            return area;
        }

        /// <summary>
        /// 諸費用設定エリアマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="carTax">諸費用設定エリアデータ</param>
        /// <returns>諸費用設定エリアデータ</returns>
        private CostArea EditForInsert(CostArea area) {
            area.CreateDate = DateTime.Now;
            area.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            area.DelFlag = "0";
            return EditForUpdate(area);
        }

        /// <summary>
        /// 諸費用設定エリアコードから諸費用設定エリア名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">諸費用設定エリアコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CostArea area = new CostAreaDao(db).GetByKey(code);
                if (area != null) {
                    codeData.Code = area.CostAreaCode;
                    codeData.Name = area.CostAreaName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }


        /// <summary>
        /// 諸費用設定エリアコードから諸費用設定を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">諸費用設定エリアコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2023/10/05 yano #4184 【車両伝票入力】販売諸費用の[中古車点検・整備費用][中古車継承整備費用]の削除
        /// 2023/08/15 yano #4176 販売諸費用の修正
        /// 2020/01/06 yano #4029 ナンバープレート（一般）の地域毎の管理 項目の追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012対応
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CostArea area = new CostAreaDao(db).GetByKey(code);

                Dictionary<string, string> ret = new Dictionary<string, string>();

                if (area != null)
                {
                    //Mod 2023/10/05 yano #4184
                    //Mod 2023/08/15 yano #4176
                    ret.Add("CostAreaCode", area.CostAreaCode);
                    ret.Add("CostAreaName", area.CostAreaName);
                    ret.Add("RequestNumberCost", (area.RequestNumberCost ?? 0).ToString());
                    ret.Add("ParkingSpaceFeeWithTax", (area.ParkingSpaceFeeWithTax ?? 0).ToString());
                    ret.Add("InspectionRegistFeeWithTax", (area.InspectionRegistFeeWithTax ?? 0).ToString());
                    ret.Add("TradeInFeeWithTax", (area.TradeInFeeWithTax ?? 0).ToString());
                    ret.Add("PreparationFeeWithTax", (area.PreparationFeeWithTax ?? 0).ToString());
                    ret.Add("RequestNumberFeeWithTax", (area.RequestNumberFeeWithTax ?? 0).ToString());
                    ret.Add("ParkingSpaceCost", (area.ParkingSpaceCost ?? 0).ToString());
                    ret.Add("NumberPlateCost", (area.NumberPlateCost ?? 0).ToString());
                    ret.Add("RecycleControlFeeWithTax", (area.RecycleControlFeeWithTax ?? 0).ToString());
                    ret.Add("FarRegistFeeWithTax", (area.FarRegistFeeWithTax ?? 0).ToString());
                    ret.Add("OutJurisdictionRegistFeeWithTax", (area.OutJurisdictionRegistFeeWithTax ?? 0).ToString());

                    //ret.Add("CostAreaCode", area.CostAreaCode);
                    //ret.Add("CostAreaName", area.CostAreaName);
                    //ret.Add("RequestNumberCost", (area.RequestNumberCost ?? 0).ToString());
                    //ret.Add("ParkingSpaceFeeWithTax", (area.ParkingSpaceFeeWithTax ?? 0).ToString());
                    //ret.Add("InspectionRegistFeeWithTax", (area.InspectionRegistFeeWithTax ?? 0).ToString());
                    //ret.Add("TradeInFeeWithTax", (area.TradeInFeeWithTax ?? 0).ToString());
                    //ret.Add("PreparationFeeWithTax", (area.PreparationFeeWithTax ?? 0).ToString());
                    //ret.Add("RequestNumberFeeWithTax", (area.RequestNumberFeeWithTax ?? 0).ToString());
                    //ret.Add("ParkingSpaceCost", (area.ParkingSpaceCost ?? 0).ToString());
                    //ret.Add("NumberPlateCost", (area.NumberPlateCost ?? 0).ToString());
                    //ret.Add("RecycleControlFeeWithTax", (area.RecycleControlFeeWithTax ?? 0).ToString());
                    //ret.Add("FarRegistFeeWithTax", (area.FarRegistFeeWithTax ?? 0).ToString());
                    //ret.Add("TradeInMaintenanceFeeWithTax", (area.TradeInMaintenanceFeeWithTax ?? 0).ToString());
                    //ret.Add("InheritedInsuranceFeeWithTax", (area.InheritedInsuranceFeeWithTax ?? 0).ToString());
                    //ret.Add("OutJurisdictionRegistFeeWithTax", (area.OutJurisdictionRegistFeeWithTax ?? 0).ToString());

                  
                }

                return Json(ret);
            }

            return new EmptyResult();
        }
            
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="serviceWork">モデルデータ</param>
        /// <histroy
        /// 2022/01/27 yano #4126【諸費用設定エリア】権限による保存機能の制限の実装
        /// </histroy>
        private void GetEntryViewData(CostArea area)
        {     
            //車両マスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_MASTEREDIT).EnableFlag;
        }
    }
}
