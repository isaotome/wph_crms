using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// サービスメニューマスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceMenuController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "サービスメニューマスタ";     // 画面名
        private static readonly string PROC_NAME = "サービスメニューマスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceMenuController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// サービスメニュー検索画面表示
        /// </summary>
        /// <returns>サービスメニュー検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// サービスメニュー検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービスメニュー検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<ServiceMenu> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["ServiceMenuCode"] = form["ServiceMenuCode"];
            ViewData["ServiceMenuName"] = form["ServiceMenuName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // サービスメニュー検索画面の表示
            return View("ServiceMenuCriteria", list);
        }

        /// <summary>
        /// サービスメニュー検索ダイアログ表示
        /// </summary>
        /// <returns>サービスメニュー検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// サービスメニュー検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービスメニュー検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["ServiceMenuCode"] = Request["ServiceMenuCode"];
            form["ServiceMenuName"] = Request["ServiceMenuName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<ServiceMenu> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["ServiceMenuCode"] = form["ServiceMenuCode"];
            ViewData["ServiceMenuName"] = form["ServiceMenuName"];

            // サービスメニュー検索画面の表示
            return View("ServiceMenuCriteriaDialog", list);
        }

        /// <summary>
        /// サービスメニューマスタ入力画面表示
        /// </summary>
        /// <param name="id">サービスメニューコード(更新時のみ設定)</param>
        /// <returns>サービスメニューマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            ServiceMenu serviceMenu;

            // 表示データ設定(追加の場合)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                serviceMenu = new ServiceMenu();
            }
            // 表示データ設定(更新の場合)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                serviceMenu = new ServiceMenuDao(db).GetByKey(id, true);
            }

            // 出口
            return View("ServiceMenuEntry", serviceMenu);
        }

        /// <summary>
        /// サービスメニューマスタ追加更新
        /// </summary>
        /// <param name="serviceMenu">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>サービスメニューマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceMenu serviceMenu, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateServiceMenu(serviceMenu);
            if (!ModelState.IsValid)
            {
                return View("ServiceMenuEntry", serviceMenu);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope())
            {
                // データ更新処理
                if (form["update"].Equals("1"))
                {
                    // データ編集・更新
                    //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                    ServiceMenu targetServiceMenu = new ServiceMenuDao(db).GetByKey(serviceMenu.ServiceMenuCode, true);
                    UpdateModel(targetServiceMenu);
                    EditServiceMenuForUpdate(targetServiceMenu);
                }
                // データ追加処理
                else
                {
                    // データ編集
                    serviceMenu = EditServiceMenuForInsert(serviceMenu);
                    // データ追加
                    db.ServiceMenu.InsertOnSubmit(serviceMenu);
                }

                // Add 2014/08/05 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        if (form["update"].Equals("1"))
                        {
                            db.ChangeServiceCostMulti(serviceMenu.ServiceMenuCode, null, serviceMenu.DelFlag, serviceMenu.LastUpdateEmployeeCode);
                        }
                        else
                        {
                            db.AddServiceCostMulti(serviceMenu.ServiceMenuCode, null, serviceMenu.CreateEmployeeCode);
                        }
                        ts.Complete();
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
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0010", new string[] { "サービスメニューコード", "保存" }));
                            return View("ServiceMenuEntry", serviceMenu);
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
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(serviceMenu.ServiceMenuCode);
        }

        /// <summary>
        /// サービスメニューコードからサービスメニュー名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">サービスメニューコード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                ServiceMenu serviceMenu = new ServiceMenuDao(db).GetByKey(code);
                if (serviceMenu != null)
                {
                    codeData.Code = serviceMenu.ServiceMenuCode;
                    codeData.Name = serviceMenu.ServiceMenuName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// サービスメニューマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>サービスメニューマスタ検索結果リスト</returns>
        private PaginatedList<ServiceMenu> GetSearchResultList(FormCollection form)
        {
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            ServiceMenu serviceMenuCondition = new ServiceMenu();
            serviceMenuCondition.ServiceMenuCode = form["ServiceMenuCode"];
            serviceMenuCondition.ServiceMenuName = form["ServiceMenuName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                serviceMenuCondition.DelFlag = form["DelFlag"];
            }
            return serviceMenuDao.GetListByCondition(serviceMenuCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="serviceMenu">サービスメニューデータ</param>
        /// <returns>サービスメニューデータ</returns>
        private ServiceMenu ValidateServiceMenu(ServiceMenu serviceMenu)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(serviceMenu.ServiceMenuCode))
            {
                ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0001", "サービスメニューコード"));
            }
            if (string.IsNullOrEmpty(serviceMenu.ServiceMenuName))
            {
                ModelState.AddModelError("ServiceMenuName", MessageUtils.GetMessage("E0001", "サービスメニュー名"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("ServiceMenuCode") && !CommonUtils.IsAlphaNumeric(serviceMenu.ServiceMenuCode))
            {
                ModelState.AddModelError("ServiceMenuCode", MessageUtils.GetMessage("E0012", "サービスメニューコード"));
            }

            return serviceMenu;
        }

        /// <summary>
        /// サービスメニューマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="serviceMenu">サービスメニューデータ(登録内容)</param>
        /// <returns>サービスメニューマスタモデルクラス</returns>
        private ServiceMenu EditServiceMenuForInsert(ServiceMenu serviceMenu)
        {
            serviceMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.CreateDate = DateTime.Now;
            serviceMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.LastUpdateDate = DateTime.Now;
            serviceMenu.DelFlag = "0";
            return serviceMenu;
        }

        /// <summary>
        /// サービスメニューマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="serviceMenu">サービスメニューデータ(登録内容)</param>
        /// <returns>サービスメニューマスタモデルクラス</returns>
        private ServiceMenu EditServiceMenuForUpdate(ServiceMenu serviceMenu)
        {
            serviceMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceMenu.LastUpdateDate = DateTime.Now;
            return serviceMenu;
        }

    }
}
