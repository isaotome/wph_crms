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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 請求先マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerClaimController : Controller
    {
        //Add 2014/08/12 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "請求先マスタ";     // 画面名
        private static readonly string PROC_NAME = "請求先マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CustomerClaimController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 請求先検索画面表示
        /// </summary>
        /// <returns>請求先検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 請求先検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>請求先検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<CustomerClaim> list = GetSearchResultList(form);

            // その他出力項目の設定
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 請求先検索画面の表示
            return View("CustomerClaimCriteria", list);
        }

        /// <summary>
        /// 請求先検索ダイアログ表示
        /// </summary>
        /// <returns>請求先検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 請求先検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>請求先検索画面ダイアログ</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける クエリストリングから、請求先分類を取得する処理を追加
        /// Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            //ADD #3111 請求先検索ダイアログに請求種別追加対応　2014/10/20 arc ishii
            CustomerClaim customerClaim;
            customerClaim = new CustomerClaim();
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);

            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CustomerClaimCode"] = Request["CustomerClaimCode"];
            form["CustomerClaimName"] = Request["CustomerClaimName"];
            form["CustomerClaimType"] = Request["CustomerClaimType"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            //Mod 2016/04/14 arc yano #3480 主作業の請求先分類により、検索条件の請求先種別リストを変更する
            form["SWCustomerClaimClass"] = Request["SWCustomerClaimClass"];   
            
            List<c_CustomerClaimType> typeList = new List<c_CustomerClaimType>();
            if (string.IsNullOrEmpty(form["SWCustomerClaimClass"]))          //請求先類が設定されていない場合
            {
                typeList = null;
                ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);
            }
            else
            {
                typeList = dao.GetCustomerClaimTypeAll(false).Where(x => string.IsNullOrWhiteSpace(x.CustomerClaimClass) || x.CustomerClaimClass.Equals(form["SWCustomerClaimClass"])).ToList<c_CustomerClaimType>();

                /*
                if (form["SWCustomerClaimClass"].Equals("2"))  //社内
                {
                    ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, false);
                }
                else
                {
                    ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, true);
                }
                */

                ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, true);

            }
        
            //ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);

            // 検索結果リストの取得
            PaginatedList<CustomerClaim> list = GetSearchResultList(form, typeList);

            // その他出力項目の設定
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            //ADD #3111 請求先検索ダイアログに請求種別追加対応　2014/10/20 arc ishii
            ViewData["CustomerClaimType"] = form["CustomerClaimType"];
            ViewData["SWCustomerClaimClass"] = form["SWCustomerClaimClass"];    //Mod 2016/04/14 arc yano #3480
            // 請求先検索画面の表示
            return View("CustomerClaimCriteriaDialog", list);
        }

        /// <summary>
        /// 請求先マスタ入力画面表示
        /// </summary>
        /// <param name="id">請求先コード(更新時のみ設定)</param>
        /// <returns>請求先マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CustomerClaim customerClaim;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                customerClaim = new CustomerClaim();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                customerClaim = new CustomerClaimDao(db).GetByKey(id);
            }

            // その他表示データの取得
            GetEntryViewData(customerClaim);

            // 出口
            return View("CustomerClaimEntry", customerClaim);
        }

        /// <summary>
        /// 請求先マスタ追加更新
        /// </summary>
        /// <param name="customerClaim">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>請求先マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerClaim customerClaim, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateCustomerClaim(customerClaim);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(customerClaim);
                return View("CustomerClaimEntry", customerClaim);
            }

            // Add 2014/08/12 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                CustomerClaim targetCustomerClaim = new CustomerClaimDao(db).GetByKey(customerClaim.CustomerClaimCode);
                UpdateModel(targetCustomerClaim);
                EditCustomerClaimForUpdate(targetCustomerClaim);
            }
            // データ追加処理
            else
            {
                // データ編集
                customerClaim = EditCustomerClaimForInsert(customerClaim);
                // データ追加
                db.CustomerClaim.InsertOnSubmit(customerClaim);
            }

            //Add 2014/08/12 arc amii エラーログ対応 SubmitChanges処理を一本化 & Exception時にエラーログ出力処理追加
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
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0010", new string[] { "請求先コード", "保存" }));
                        GetEntryViewData(customerClaim);
                        return View("CustomerClaimEntry", customerClaim);
                    }
                    else
                    {
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// 請求先コードから請求先名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">請求先コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2016/04/13 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 主作業大分類を取得する
        /// </history>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(code);
                if (customerClaim != null)
                {
                    codeData.Code = customerClaim.CustomerClaimCode;
                    codeData.Name = customerClaim.CustomerClaimName;
                    codeData.Code2 = (customerClaim.c_CustomerClaimType != null ? customerClaim.c_CustomerClaimType.CustomerClaimClass != null ? customerClaim.c_CustomerClaimType.CustomerClaimClass : "" : "");       //Add 2016/04/13 arc yano #3480
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 請求先コードから請求先名、決済種別リストを取得する(Ajax専用)
        /// </summary>
        /// <param name="code">請求先コード</param>
        /// <returns></returns>
        public void GetMasterWithClaimable(string code) {
            if (Request.IsAjaxRequest()) {
                CustomerClaim claim = new CustomerClaimDao(db).GetByKey(code);
                CodeDataList codeDataList = new CodeDataList();
                if (claim != null){
                    codeDataList.Code = claim.CustomerClaimCode;
                    codeDataList.Name = claim.CustomerClaimName;
                    if (claim.CustomerClaimable != null) {
                        codeDataList.DataList = new List<CodeData>();
                        foreach (var a in claim.CustomerClaimable) {
                            codeDataList.DataList.Add(new CodeData() { Code = a.PaymentKindCode, Name = a.PaymentKind != null ? a.PaymentKind.PaymentKindName : "" });
                        }
                    }
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }
        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="customerClaim">モデルデータ</param>
        private void GetEntryViewData(CustomerClaim customerClaim)
        {
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);
            ViewData["PaymentKindTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), customerClaim.PaymentKindType, true);
            ViewData["RoundTypeList"] = CodeUtils.GetSelectListByModel(dao.GetRoundTypeAll(false), customerClaim.RoundType, true);

        }

        /// <summary>
        /// 請求先マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <param name="typeList">請求先タイプリスト(検索条件)</param>
        /// <returns>請求先マスタ検索結果リスト</returns>
        /// <history>
        /// 2016/04/13 arc yano #3480 サービス伝票　サービス伝票の請求先を主作業の内容により切り分ける 請求先タイプのリストを引数として受け取る
        /// </history>
        private PaginatedList<CustomerClaim> GetSearchResultList(FormCollection form, List<c_CustomerClaimType> typeList = null)
        {
            CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
            CustomerClaim customerClaimCondition = new CustomerClaim();
            customerClaimCondition.CustomerClaimCode = form["CustomerClaimCode"];
            customerClaimCondition.CustomerClaimName = form["CustomerClaimName"];
            //ADD #3111 請求先検索ダイアログに請求種別検索対応　2014/10/20 arc ishii
            customerClaimCondition.CustomerClaimType = form["CustomerClaimType"];

            //Mod 2016/04/14 #3480 arc yano
            if (typeList != null)
            {
                customerClaimCondition.CustomerClaimTypeList = typeList.Select(x => x.Code).ToList();
            }

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                customerClaimCondition.DelFlag = form["DelFlag"];
            }

            return customerClaimDao.GetListByCondition(customerClaimCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="customerClaim">請求先データ</param>
        /// <returns>請求先データ</returns>
        private CustomerClaim ValidateCustomerClaim(CustomerClaim customerClaim)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0001", "請求先コード"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimName))
            {
                ModelState.AddModelError("CustomerClaimName", MessageUtils.GetMessage("E0001", "請求先名"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimType))
            {
                ModelState.AddModelError("CustomerClaimType", MessageUtils.GetMessage("E0001", "請求種別"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CustomerClaimCode") && !CommonUtils.IsAlphaNumeric(customerClaim.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0012", "請求先コード"));
            }

            return customerClaim;
        }

        /// <summary>
        /// 請求先マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="customerClaim">請求先データ(登録内容)</param>
        /// <returns>請求先マスタモデルクラス</returns>
        private CustomerClaim EditCustomerClaimForInsert(CustomerClaim customerClaim)
        {
            customerClaim.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.CreateDate = DateTime.Now;
            customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.LastUpdateDate = DateTime.Now;
            customerClaim.DelFlag = "0";
            return customerClaim;
        }

        /// <summary>
        /// 請求先マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="customerClaim">請求先データ(登録内容)</param>
        /// <returns>請求先マスタモデルクラス</returns>
        private CustomerClaim EditCustomerClaimForUpdate(CustomerClaim customerClaim)
        {
            customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.LastUpdateDate = DateTime.Now;
            return customerClaim;
        }

    }
}
