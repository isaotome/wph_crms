using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data.Linq;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// サービス工数表マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceCostController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "サービス工数表マスタ";     // 画面名
        private static readonly string PROC_NAME = "サービス工数表マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceCostController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// サービス工数表検索画面表示
        /// </summary>
        /// <returns>サービス工数表検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// サービス工数表検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービス工数表検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // サービスメニューリストの取得
            List<ServiceMenu> list = new ServiceMenuDao(db).GetAll(false);

            // その他出力項目の設定
            ViewData["DelFlag"] = form["DelFlag"];

            // サービス工数表検索画面の表示
            return View("ServiceCostCriteria", list);
        }

        /// <summary>
        /// サービス工数表マスタ入力画面表示
        /// </summary>
        /// <param name="id">サービス工数表コード(更新時のみ設定)</param>
        /// <returns>サービス工数表マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            // 表示データ設定
            GetEntryDisplayData(null, null);

            // 出口
            return View("ServiceCostEntry");
        }

        /// <summary>
        /// サービス工数表マスタ追加更新
        /// </summary>
        /// <param name="serviceCost">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>サービス工数表マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form)
        {
            // データチェック
            List<string> errorItemList = ValidateServiceCost(form);
            if (errorItemList.Count > 0)
            {
                GetEntryDisplayData(form, errorItemList);
                return View("ServiceCostEntry");
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ登録処理
            ServiceCostDao serviceCostDao = new ServiceCostDao(db);
            
            foreach (string key in form.AllKeys)
            {
                string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                if (keyArr[0].Equals("Cost"))
                {
                    ServiceCost serviceCost = serviceCostDao.GetByKey(keyArr[1], keyArr[2]);

                    // データ追加
                    if (serviceCost == null)
                    {
                        db.AddServiceCost(keyArr[1], keyArr[2], 0m, ((Employee)Session["Employee"]).EmployeeCode);
                    }
                    // データ更新
                    else
                    {
                        EditServiceCostForUpdate(serviceCost, decimal.Parse(form[key]));
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
                    }
                }
            }
            //MOD 2014/10/28 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// 登録画面表示データの取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="errorItemList">エラー項目リスト</param>
        private void GetEntryDisplayData(FormCollection form, List<string> errorItemList)
        {
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            CarClassDao carClassDao = new CarClassDao(db);

            ViewData["ServiceMenuList"] = serviceMenuDao.GetAll(false);
            ViewData["CarClassList"] = carClassDao.GetAll(false);
            if (form == null)
            {
                ServiceCostDao serviceCostDao = new ServiceCostDao(db);
                ViewData["ServiceCostDic"] = serviceCostDao.GetAll(true);
            }
            else
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (string key in form.AllKeys)
                {
                    string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                    if (keyArr[0].Equals("Cost"))
                    {
                        dic.Add(keyArr[1] + "_" + keyArr[2], form[key]);
                    }
                }
                ViewData["ServiceCostDic"] = dic;
            }
            if (errorItemList == null)
            {
                ViewData["ErrorItemList"] = new List<string>();
            }
            else
            {
                ViewData["ErrorItemList"] = errorItemList;
            }
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="serviceCost">サービス工数表データ</param>
        /// <returns>エラー項目リスト</returns>
        private List<string> ValidateServiceCost(FormCollection form)
        {
            List<string> ret = new List<string>();
            foreach (string key in form.AllKeys)
            {
                string msg = MessageUtils.GetMessage("E0002", new string[] { "工数", "正の整数3桁以内かつ小数2桁以内" });
                string[] keyArr = key.Split(new string[] { "_" }, StringSplitOptions.None);
                if (keyArr[0].Equals("Cost"))
                {
                    // 必須チェック
                    if (string.IsNullOrEmpty(form[key]))
                    {
                        if (ret.Count == 0)
                        {
                            ModelState.AddModelError("", msg);
                        }
                        ret.Add(key);
                        continue;
                    }

                    // フォーマットチェック
                    if ((Regex.IsMatch(form[key], @"^\d{1,3}\.\d{1,2}$"))
                        || (Regex.IsMatch(form[key], @"^\d{1,3}$")))
                    {
                    }
                    else
                    {
                        if (ret.Count == 0)
                        {
                            ModelState.AddModelError("", msg);
                        }
                        ret.Add(key);
                        continue;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// サービス工数表マスタ追加データ編集
        /// </summary>
        /// <param name="serviceMenuCode">サービスメニューコード</param>
        /// <param name="carClassCode">車両クラスコード</param>
        /// <param name="cost">工数</param>
        /// <returns>サービス工数表マスタモデルクラス</returns>
        private ServiceCost EditServiceCostForInsert(string serviceMenuCode, string carClassCode, decimal cost)
        {
            ServiceCost serviceCost = new ServiceCost();
            serviceCost.ServiceMenuCode = serviceMenuCode;
            serviceCost.CarClassCode = carClassCode;
            serviceCost.Cost = cost;
            serviceCost.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.CreateDate = DateTime.Now;
            serviceCost.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.LastUpdateDate = DateTime.Now;
            serviceCost.DelFlag = "0";
            return serviceCost;
        }

        /// <summary>
        /// サービス工数表マスタ更新データ編集
        /// </summary>
        /// <param name="serviceCost">サービス工数表データ(更新前内容)</param>
        /// <param name="cost">工数</param>
        /// <returns>サービス工数表マスタモデルクラス</returns>
        private ServiceCost EditServiceCostForUpdate(ServiceCost serviceCost, decimal cost)
        {
            serviceCost.Cost = cost;
            serviceCost.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceCost.LastUpdateDate = DateTime.Now;
            return serviceCost;
        }

        /// <summary>
        /// グレードコード、サービスメニューコードから工数を取得する(Ajax専用）
        /// </summary>
        /// <param name="gradeCode">グレードコード</param>
        /// <param name="menuCode">サービスメニューコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMasterDetail(string gradeCode,string menuCode) {

            if (Request.IsAjaxRequest()) {
                string serviceMenuCode = "";
                string serviceMenuName = "";
                string costRate = "";
                Dictionary<string, string> retCost = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(menuCode)) {
                    ServiceMenu menu = new ServiceMenuDao(db).GetByKey(menuCode);
                    if (menu != null) {
                        serviceMenuCode = menu.ServiceMenuCode;
                        serviceMenuName = menu.ServiceMenuName;
                    }
                    CarGrade grade = new CarGradeDao(db).GetByKey(gradeCode);
                    if (grade != null) {
                        string carClass = grade.CarClassCode;

                        ServiceCost cost = new ServiceCostDao(db).GetByKey(menuCode, carClass);
                        if (cost != null) {
                            costRate = cost.Cost.ToString();
                        }
                    }
                }
                retCost.Add("ServiceMenuName", serviceMenuName);
                retCost.Add("ServiceMenuCode", serviceMenuCode);
                retCost.Add("Cost", costRate);
                return Json(retCost);
            }
            return new EmptyResult();
        }
    }
}
