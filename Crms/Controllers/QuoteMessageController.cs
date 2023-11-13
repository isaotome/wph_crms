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

namespace Crms.Controllers
{
    /// <summary>
    /// 見積メッセージマスタアクセス機能コントローラクラス
    /// </summary>
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class QuoteMessageController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public QuoteMessageController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 見積メッセージ検索画面表示
        /// </summary>
        /// <returns>見積メッセージ検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 見積メッセージ検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>見積メッセージ検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<QuoteMessage> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), form["QuoteType"], true);
            ViewData["DelFlag"] = form["DelFlag"];

            // 見積メッセージ検索画面の表示
            return View("QuoteMessageCriteria", list);
        }

        /// <summary>
        /// 見積メッセージ検索ダイアログ表示
        /// </summary>
        /// <returns>見積メッセージ検索ダイアログ</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// 見積メッセージ検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>見積メッセージ検索画面ダイアログ</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["QuoteType"] = Request["QuoteType"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<QuoteMessage> list = GetSearchResultList(form);

            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), form["QuoteType"], true);

            // 見積メッセージ検索画面の表示
            return View("QuoteMessageCriteriaDialog", list);
        }

        /// <summary>
        /// 見積メッセージマスタ入力画面表示
        /// </summary>
        /// <param name="id">見積メッセージコード(更新時のみ設定)</param>
        /// <returns>見積メッセージマスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            QuoteMessage quoteMessage;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                quoteMessage = new QuoteMessage();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                string[] idArr = id.Split(new string[] { "," }, StringSplitOptions.None);
                quoteMessage = new QuoteMessageDao(db).GetByKey(idArr[0], idArr[1]);
            }

            // その他表示データの取得
            GetEntryViewData(quoteMessage);

            // 出口
            return View("QuoteMessageEntry", quoteMessage);
        }

        /// <summary>
        /// 見積メッセージマスタ追加更新
        /// </summary>
        /// <param name="quoteMessage">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>見積メッセージマスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(QuoteMessage quoteMessage, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateQuoteMessage(quoteMessage);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(quoteMessage);
                return View("QuoteMessageEntry", quoteMessage);
            }

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                QuoteMessage targetQuoteMessage = new QuoteMessageDao(db).GetByKey(quoteMessage.CompanyCode, quoteMessage.QuoteType);
                UpdateModel(targetQuoteMessage);
                EditQuoteMessageForUpdate(targetQuoteMessage);
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
                            throw e;
                        }
                    }
                }
            }
            // データ追加処理
            else
            {
                // データ編集
                quoteMessage = EditQuoteMessageForInsert(quoteMessage);

                // データ追加
                db.QuoteMessage.InsertOnSubmit(quoteMessage);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0010", new string[] { "会社,見積種別", "保存" }));
                        ModelState.AddModelError("QuoteType", "");
                        GetEntryViewData(quoteMessage);
                        return View("QuoteMessageEntry", quoteMessage);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            // 出口
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// 見積メッセージコードから見積メッセージ名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code1">会社コード</param>
        /// <param name="code2">見積種別</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        public ActionResult GetMaster(string code1, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                QuoteMessage quoteMessage = new QuoteMessageDao(db).GetByKey(code1, code2);
                if (quoteMessage != null)
                {
                    codeData.Code = quoteMessage.CompanyCode;
                    codeData.Code2 = quoteMessage.QuoteType;
                    codeData.Name = quoteMessage.Description;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="quoteMessage">モデルデータ</param>
        private void GetEntryViewData(QuoteMessage quoteMessage)
        {
            // 会社名の取得
            ViewData["CompanyName"] = "";
            if (!string.IsNullOrEmpty(quoteMessage.CompanyCode))
            {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(quoteMessage.CompanyCode);
                if (company != null)
                {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }

            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), quoteMessage.QuoteType, true);
        }

        /// <summary>
        /// 見積メッセージマスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>見積メッセージマスタ検索結果リスト</returns>
        private PaginatedList<QuoteMessage> GetSearchResultList(FormCollection form)
        {
            QuoteMessageDao quoteMessageDao = new QuoteMessageDao(db);
            QuoteMessage quoteMessageCondition = new QuoteMessage();
            quoteMessageCondition.QuoteType = form["QuoteType"];
            quoteMessageCondition.Company = new Company();
            quoteMessageCondition.Company.CompanyCode = form["CompanyCode"];
            quoteMessageCondition.Company.CompanyName = form["CompanyName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                quoteMessageCondition.DelFlag = form["DelFlag"];
            }
            return quoteMessageDao.GetListByCondition(quoteMessageCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="quoteMessage">見積メッセージデータ</param>
        /// <returns>見積メッセージデータ</returns>
        private QuoteMessage ValidateQuoteMessage(QuoteMessage quoteMessage)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(quoteMessage.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "会社"));
            }
            if (string.IsNullOrEmpty(quoteMessage.QuoteType))
            {
                ModelState.AddModelError("QuoteType", MessageUtils.GetMessage("E0001", "見積種別"));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("CompanyCode") && !CommonUtils.IsAlphaNumeric(quoteMessage.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0012", "会社"));
            }
            if (ModelState.IsValidField("QuoteType") && !CommonUtils.IsAlphaNumeric(quoteMessage.QuoteType))
            {
                ModelState.AddModelError("QuoteType", MessageUtils.GetMessage("E0012", "見積種別"));
            }

            // 重複チェック
            if (ModelState.IsValid)
            {
                if (new QuoteMessageDao(db).GetByKey(quoteMessage.CompanyCode, quoteMessage.QuoteType) != null)
                {
                    ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0006", "会社,見積種別"));
                    ModelState.AddModelError("QuoteType", "");
                }
            }
            
            return quoteMessage;
        }

        /// <summary>
        /// 見積メッセージマスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="quoteMessage">見積メッセージデータ(登録内容)</param>
        /// <returns>見積メッセージマスタモデルクラス</returns>
        private QuoteMessage EditQuoteMessageForInsert(QuoteMessage quoteMessage)
        {
            quoteMessage.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.CreateDate = DateTime.Now;
            quoteMessage.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.LastUpdateDate = DateTime.Now;
            quoteMessage.DelFlag = "0";
            return quoteMessage;
        }

        /// <summary>
        /// 見積メッセージマスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="quoteMessage">見積メッセージデータ(登録内容)</param>
        /// <returns>見積メッセージマスタモデルクラス</returns>
        private QuoteMessage EditQuoteMessageForUpdate(QuoteMessage quoteMessage)
        {
            quoteMessage.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.LastUpdateDate = DateTime.Now;
            return quoteMessage;
        }

    }
}
