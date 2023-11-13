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
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 主作業マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceWorkController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "主作業マスタ";     // 画面名
        private static readonly string PROC_NAME = "主作業マスタ登録"; // 処理名

        //Add 2016/04/14 arc yano #3480 サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み
        private static readonly string CATEGORYCODE_CUSTOMERCLAIMCLASS = "016";         // 請求先分類

        //サービス集計表・新車・中古車区分
        private static readonly string CATEGORYCODE_ACCOUNTCLASSCODE = "028";           //Add 2022/01/24 yano #4122

        private static readonly string APPLICATIONCODE_SERVICEMASTEREDIT = "ServiceMasterEdit";        //Add 2022/01/24 yano #4124

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceWorkController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 主作業検索画面表示
        /// </summary>
        /// <returns>主作業検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 主作業検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>主作業検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<ServiceWork> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), form["Classification1"], true);
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), form["Classification2"], true);
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["Name"] = form["Name"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 主作業検索画面の表示
            return View("ServiceWorkCriteria", list);
        }

        /// <summary>
        /// 主作業検索ダイアログ表示
        /// </summary>
        /// <returns>主作業検索ダイアログ</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 サービス伝票　初回表示時に大分類が指定されいる場合はformに設定する
        /// </history>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 主作業検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>主作業検索画面ダイアログ</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 請求先が入力されている場合は、請求先タイプで主作業を絞り込む
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["Classification1"] = Request["Classification1"];
            form["Classification2"] = Request["Classification2"];
            form["ServiceWorkCode"] = Request["ServiceWorkCode"];
            form["Name"] = Request["Name"];

            form["CCCustomerClaimClass"] = Request["CCCustomerClaimClass"];   //Add  2016/04/14 arc yano #3480

            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<ServiceWork> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), form["Classification1"], true);

            //Mod 2016/04/14 arc yano #3480
            ViewData["CCCustomerClaimClass"] = form["CCCustomerClaimClass"];
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), form["Classification2"], true);
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["Name"] = form["Name"];

            // 主作業検索画面の表示
            return View("ServiceWorkCriteriaDialog", list);
        }

        /// <summary>
        /// 主作業マスタ入力画面表示
        /// </summary>
        /// <param name="id">主作業コード(更新時のみ設定)</param>
        /// <returns>主作業マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            ServiceWork serviceWork;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                serviceWork = new ServiceWork();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                serviceWork = new ServiceWorkDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(serviceWork);

            // 出口
            return View("ServiceWorkEntry", serviceWork);
        }

        /// <summary>
        /// 主作業マスタ追加更新
        /// </summary>
        /// <param name="serviceWork">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>主作業マスタ入力画面</returns>
        /// <history>
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceWork serviceWork, FormCollection form)
        {
            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }
            

            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateServiceWork(serviceWork);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(serviceWork);
                return View("ServiceWorkEntry", serviceWork);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
                ServiceWork targetServiceWork = new ServiceWorkDao(db).GetByKey(serviceWork.ServiceWorkCode, true);
                UpdateModel(targetServiceWork);
                EditServiceWorkForUpdate(targetServiceWork);
                
            }
            // データ追加処理
            else
            {
                // データ編集
                serviceWork = EditServiceWorkForInsert(serviceWork);

                // データ追加
                db.ServiceWork.InsertOnSubmit(serviceWork);
                
            }
            // Add 2014/08/05 arc amii エラーログ対応 submitChangeを一本化 + エラーログ出力
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
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
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0010", new string[] { "主作業コード(小分類)", "保存" }));
                        GetEntryViewData(serviceWork);
                        return View("ServiceWorkEntry", serviceWork);
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

            //Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(serviceWork.ServiceWorkCode);
        }

        /// <summary>
        /// 主作業コードから主作業名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">主作業コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                ServiceWork serviceWork = new ServiceWorkDao(db).GetByKey(code);
                if (serviceWork != null)
                {
                    codeData.Code = serviceWork.ServiceWorkCode;
                    codeData.Name = serviceWork.Name;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// 主作業データの取得
        /// </summary>
        /// <param name="code">主作業コード</param>
        /// <history>
        /// Mod 2016/04/15 arc yano #3480 取得する主作業の情報に「請求先分類」を追加
        /// </history>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                ServiceWork work = new ServiceWorkDao(db).GetByKey(code);
                if (work != null) {
                    ret.Add("ServiceWorkCode", work.ServiceWorkCode);
                    ret.Add("ServiceWorkName", work.Name);
                    ret.Add("Classification1", work.Classification1);
                    ret.Add("ClassificationName1", work.c_ServiceWorkClass1.Name);
                    ret.Add("CustomerClaimClass", (work.CustomerClaimClass ?? ""));   //Add 2016/04/15 arc yano #3480
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="serviceWork">モデルデータ</param>
        /// <histroy>
        /// 2022/01/24 yano #4124【主作業マスタ入力】権限による保存機能の制限の実装
        /// 2022/01/24 yano #4122【主作業マスタ入力】新車・中古車を選択できる項目の追加
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み
        /// </histroy>
        private void GetEntryViewData(ServiceWork serviceWork)
        {
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["Classification1List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass1All(false), serviceWork.Classification1, true);
            ViewData["Classification2List"] = CodeUtils.GetSelectListByModel(dao.GetServiceWorkClass2All(false), serviceWork.Classification2, true);

            ViewData["CustomerClaimClassList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORYCODE_CUSTOMERCLAIMCLASS,false), serviceWork.CustomerClaimClass, true);  //Add 2016/04/14 arc yano #3480

            ViewData["AccountClassList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(CATEGORYCODE_ACCOUNTCLASSCODE,false), serviceWork.AccountClassCode, false);  //Add 2022/01/24 yano #4122

            //Add #4124 yano
            //サービスマスタ編集権限のあるユーザのみ保存ボタンを表示する。
            ViewData["ButtonVisible"] = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, APPLICATIONCODE_SERVICEMASTEREDIT).EnableFlag;

        }

        /// <summary>
        /// 主作業マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>主作業マスタ検索結果リスト</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業⇔請求先コード絞込み
        /// </history>
        private PaginatedList<ServiceWork> GetSearchResultList(FormCollection form)
        {
            ServiceWorkDao serviceWorkDao = new ServiceWorkDao(db);
            ServiceWork serviceWorkCondition = new ServiceWork();
            serviceWorkCondition.ServiceWorkCode = form["ServiceWorkCode"];
            serviceWorkCondition.Name = form["Name"];
            serviceWorkCondition.Classification1 = form["Classification1"];
            serviceWorkCondition.Classification2 = form["Classification2"];
            serviceWorkCondition.CustomerClaimClass = form["CCCustomerClaimClass"];     //Add 2016/04/14 arc yano #3480

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                serviceWorkCondition.DelFlag = form["DelFlag"];
            }
            return serviceWorkDao.GetListByCondition(serviceWorkCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="serviceWork">主作業データ</param>
        /// <returns>主作業データ</returns>
        private ServiceWork ValidateServiceWork(ServiceWork serviceWork)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(serviceWork.ServiceWorkCode))
            {
                ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0001", "主作業コード(小分類)"));
            }
            if (string.IsNullOrEmpty(serviceWork.Name))
            {
                ModelState.AddModelError("Name", MessageUtils.GetMessage("E0001", "主作業名(小分類)"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Price"))
            {
                ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { "サービス料金", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("ServiceWorkCode") && !CommonUtils.IsAlphaNumeric(serviceWork.ServiceWorkCode))
            {
                ModelState.AddModelError("ServiceWorkCode", MessageUtils.GetMessage("E0012", "主作業コード(小分類)"));
            }
            if (ModelState.IsValidField("Price") && serviceWork.Price != null)
            {
                if (!Regex.IsMatch(serviceWork.Price.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { "サービス料金", "正の整数のみ" }));
                }
            }

            return serviceWork;
        }

        /// <summary>
        /// 主作業マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="serviceWork">主作業データ(登録内容)</param>
        /// <returns>主作業マスタモデルクラス</returns>
        private ServiceWork EditServiceWorkForInsert(ServiceWork serviceWork)
        {
            serviceWork.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.CreateDate = DateTime.Now;
            serviceWork.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.LastUpdateDate = DateTime.Now;
            serviceWork.DelFlag = "0";
            return serviceWork;
        }

        /// <summary>
        /// 主作業マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="serviceWork">主作業データ(登録内容)</param>
        /// <returns>主作業マスタモデルクラス</returns>
        private ServiceWork EditServiceWorkForUpdate(ServiceWork serviceWork)
        {
            serviceWork.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            serviceWork.LastUpdateDate = DateTime.Now;
            return serviceWork;
        }

    }
}
