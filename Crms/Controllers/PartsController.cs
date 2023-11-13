using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using Crms.Models;                      //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
using OfficeOpenXml;


namespace Crms.Controllers
{
    /// <summary>
    /// 部品マスタアクセス機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsController : Controller
    {
        //Add 2014/08/05 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "部品マスタ";     // 画面名
        private static readonly string PROC_NAME = "部品マスタ登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /*
        /// <summary>
        /// ダブルコーテーション置換文字
        /// </summary>
        private readonly string QuoteReplace = "@@@@";

        /// <summary>
        /// カンマ置換文字
        /// </summary>
        private readonly string CommaReplace = "?";
        */

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// 部品検索画面表示
        /// </summary>
        /// <returns>部品検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 部品検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品検索画面</returns>
        /// <history>
        /// 2021/02/22 yano #4083 【部品マスタ検索】検索処理のパフォーマンス改善対応
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // デフォルト値の設定
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            PaginatedList<PartsCriteria> list;      //Mod 2021/02/22 yano #4083 
            //PaginatedList<Parts> list;
            if (criteriaInit)
            {
                list = new PaginatedList<PartsCriteria>();  //Mod 2021/02/22 yano #4083 
            }
            else
            {
                list = GetSearchResultList(form);
            }

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DelFlag"] = form["DelFlag"];

            // 部品検索画面の表示
            return View("PartsCriteria", list);
        }

        /// <summary>
        /// 部品検索ダイアログ表示
        /// </summary>
        /// <returns>部品検索ダイアログ</returns>
        /// <history>
        ///  2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 純正区分の引数追加
        /// </history>
        public ActionResult CriteriaDialog(string GenuineType = null)
        {
            criteriaInit = true;

            FormCollection form = new FormCollection();

            form["GenuineType"] = (GenuineType ?? "");

            return CriteriaDialog(form);
        }

        /// <summary>
        /// 部品検索ダイアログ表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品検索画面ダイアログ</returns>
        /// <history>
        ///  2021/02/22 yano #4083 【部品マスタ検索】検索処理のパフォーマンス改善対応
        ///  2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 純正区分の検索条件追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // 検索条件の設定
            // (クエリストリングを検索条件に使用する為、Requestを使用。
            //  なおフォームが使用された場合、Requestにはフォームの値が格納されている。)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["PartsNumber"] = Request["PartsNumber"];
            form["PartsNameJp"] = Request["PartsNameJp"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // 検索結果リストの取得
            //Mod 2021/02/22 yano #4083
            PaginatedList<PartsCriteria> list;
            //PaginatedList<Parts> list;
            if (criteriaInit)
            {
                if ((string.IsNullOrEmpty(form["MakerCode"]))
                    && (string.IsNullOrEmpty(form["MakerName"]))
                    && (string.IsNullOrEmpty(form["PartsNumber"]))
                    && (string.IsNullOrEmpty(form["PartsNameJp"])))
                {
                    list = new PaginatedList<PartsCriteria>();  //Mod 2021/02/22 yano #4083
                }
                else
                {
                    list = GetSearchResultList(form);
                }
            }
            else
            {
                list = GetSearchResultList(form);
            }

            // その他出力項目の設定
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];

            //純正区分
            ViewData["GenuineType"] = form["GenuineType"];

            // 部品検索画面の表示
            return View("PartsCriteriaDialog", list);
        }

        /// <summary>
        /// 部品マスタ入力画面表示
        /// </summary>
        /// <param name="id">部品コード(更新時のみ設定)</param>
        /// <returns>部品マスタ入力画面</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Parts parts;

            // 追加の場合
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                parts = new Parts();
            }
            // 更新の場合
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                parts = new PartsDao(db).GetByKey(id, true);
            }

            // その他表示データの取得
            GetEntryViewData(parts);

            // 出口
            return View("PartsEntry", parts);
        }

        /// <summary>
        /// 部品マスタ追加更新
        /// </summary>
        /// <param name="parts">モデルデータ(登録内容)</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>部品マスタ入力画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Parts parts, FormCollection form)
        {
            // 継続保持する出力情報の設定
            ViewData["update"] = form["update"];

            // データチェック
            ValidateParts(parts, "");
            if (!ModelState.IsValid)
            {
                GetEntryViewData(parts);
                return View("PartsEntry", parts);
            }

            // Add 2014/08/05 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // データ更新処理
            if (form["update"].Equals("1"))
            {
                // データ編集・更新
                //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
                Parts targetParts = new PartsDao(db).GetByKey(parts.PartsNumber, true);
                UpdateModel(targetParts);
                EditPartsForUpdate(targetParts);
            }
            // データ追加処理
            else
            {
                // データ編集
                parts = EditPartsForInsert(parts);

                // データ追加
                db.Parts.InsertOnSubmit(parts);
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
                    // 更新時、クライアントの読み取り以降にDB値が更新された時、ローカルの値をDB値で上書きする
                    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                    {
                        occ.Resolve(RefreshMode.KeepCurrentValues);
                    }
                    // リトライ回数を超えた場合、エラーとする
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
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // 一意制約エラーの場合、メッセージを設定し、返す
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "部品番号", "保存" }));
                        GetEntryViewData(parts);
                        return View("PartsEntry", parts);
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
                    // 上記以外の例外の場合、エラーログ出力し、エラー画面に遷移する
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii 保存ボタン対応
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // 出口
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(parts.PartsNumber);
        }

        /// <summary>
        /// 部品コードから部品名を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部品コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// </history>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null)
                {
                    codeData.Code = parts.PartsNumber;
                    codeData.Name = parts.PartsNameJp;

                    codeData.Code2 = parts.GenuineType;     //2018/01/15 arc yano #3833
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 部品コードから部品マスタを取得する(Ajax専用）
        /// </summary>
        /// <param name="code">部品コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
        /// </history>
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null)
                {
                    ret.Add("PartsNameJp", parts.PartsNameJp);
                    ret.Add("PartsNameEn", parts.PartsNameEn);
                    ret.Add("PartsNumber", parts.PartsNumber);
                    ret.Add("Cost", parts.Cost.ToString());
                    ret.Add("Price", parts.Price.ToString());
                    ret.Add("MakerCode", parts.MakerCode);          //Add 2018/06/01 arc yano #3894
                    ret.Add("GenuineType", parts.GenuineType);      //Add 2018/06/01 arc yano #3894
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// 画面表示データの取得
        /// </summary>
        /// <param name="parts">モデルデータ</param>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更　移動平均単価の参照先をPartsAverageCost→PartsMovingAverageCostに変更
        /// 2016/01/21 arc yano #3403_部品マスタ入力　純正区分、メーカー部品番号の必須項目化 (#3397_【大項目】部品仕入機能改善 課題管理表対応) 
        ///                     純正区分ドロップダウンのブランクを選択できないようにする                
        /// </history>
        private void GetEntryViewData(Parts parts)
        {
            // メーカー名の取得
            if (!string.IsNullOrEmpty(parts.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(parts.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            //Mod 2018/05/14 arc yano #3880
            PartsMovingAverageCost condition = new PartsMovingAverageCost();
            condition.PartsNumber = parts.PartsNumber;
            condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;

            PartsMovingAverageCost PartsAvgCost = new PartsMovingAverageCostDao(db).GetByKey(condition);

            if (PartsAvgCost == null)
            {
                PartsAvgCost = new PartsMovingAverageCost();
                PartsAvgCost.Price = null;
            }

            /*
            // Add 2014/12/24 arc nakayama 移動平均単価以降
            //移動平均単価テーブル(PartsAverageCost)から、移動平均単価(Price)を取得
            PartsAverageCost PartsAvgCost = new PartsAverageCostDao(db).GetByKeyPartsNumber(parts);
            if (PartsAvgCost == null)
            {
                PartsAvgCost = new PartsAverageCost();
                PartsAvgCost.Price = null;
            }
            */

            ViewData["MoveAverageUnitPrice"] = PartsAvgCost.Price;
            // セレクトリストの取得
            CodeDao dao = new CodeDao(db);
            ViewData["GenuineTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), parts.GenuineType, false);   //Mod 2016/01/21 arc yano
            // Mod 2014/11/06 arc nakayama 部品項目追加対応
            ViewData["UnitCD1List"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("010", false), parts.UnitCD1, true);
        }

        /// <summary>
        /// 部品マスタ検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>部品マスタ検索結果リスト</returns>
        /// <history>
        ///  2021/02/22 yano #4083 【部品マスタ検索】検索処理のパフォーマンス改善対応
        ///  2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 純正区分の検索条件追加
        /// </history>
        private PaginatedList<PartsCriteria> GetSearchResultList(FormCollection form)
        {
            PartsDao partsDao = new PartsDao(db);
            Parts partsCondition = new Parts();
            partsCondition.PartsNumber = form["PartsNumber"];
            partsCondition.PartsNameJp = form["PartsNameJp"];
            partsCondition.Maker = new Maker();
            partsCondition.Maker.MakerCode = form["MakerCode"];
            partsCondition.Maker.MakerName = form["MakerName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                partsCondition.DelFlag = form["DelFlag"];
            }

            //純正区分
            partsCondition.GenuineType = form["GenuineType"];

            return partsDao.GetListByCondition(partsCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 入力チェック
        /// </summary>
        /// <param name="parts">部品データ</param>
        /// <returns>部品データ</returns>
        /// <history>
        /// Add 2016/01/21 arc yano #3403_部品マスタ入力　純正区分、メーカー部品番号の必須項目化(#3397_【大項目】部品仕入機能改善 課題管理表対応)
        ///                                               メーカー部品番号の必須チェックを追加
        /// </history>
        private Parts ValidateParts(Parts parts, String lineMsg)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(parts.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", lineMsg + "部品番号"));
            }
            if (string.IsNullOrEmpty(parts.PartsNameJp))
            {
                ModelState.AddModelError("PartsNameJp", MessageUtils.GetMessage("E0001", lineMsg + "部品名(日本語)"));
            }
            if (string.IsNullOrEmpty(parts.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", lineMsg + "メーカーコード"));
            }

            //Add 2016/01/21 arc yano
            if (string.IsNullOrEmpty(parts.MakerPartsNumber))
            {
                ModelState.AddModelError("MakerPartsNumber", MessageUtils.GetMessage("E0001", lineMsg + "メーカー部品番号"));
            }

            // 属性チェック
            if (!ModelState.IsValidField("Price"))
            {
                ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "定価", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "販売価格", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("SoPrice"))
            {
                ModelState.AddModelError("SoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "S/O価格", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "原価", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("ClaimPrice"))
            {
                ModelState.AddModelError("ClaimPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "クレーム申請部品代", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("MpPrice"))
            {
                ModelState.AddModelError("MpPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "MP価格", "正の整数のみ" }));
            }
            if (!ModelState.IsValidField("EoPrice"))
            {
                ModelState.AddModelError("EoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "E/O価格", "正の整数のみ" }));
            }
            // Add 2014/11/07 arc nakayama 部品項目追加対応
            if (!ModelState.IsValidField("QuantityPerUnit1"))
            {
                ModelState.AddModelError("QuantityPerUnit1", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "数量", "正の整数のみ" }));
            }

            // フォーマットチェック
            if (ModelState.IsValidField("PartsNumber") && !CommonUtils.IsAlphaNumeric(parts.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0012", lineMsg + "部品番号"));
            }
            if (ModelState.IsValidField("Price") && parts.Price != null)
            {
                if (!Regex.IsMatch(parts.Price.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "定価", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SalesPrice") && parts.SalesPrice != null)
            {
                if (!Regex.IsMatch(parts.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "販売価格", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("SoPrice") && parts.SoPrice != null)
            {
                if (!Regex.IsMatch(parts.SoPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "S/O価格", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("Cost") && parts.Cost != null)
            {
                if (!Regex.IsMatch(parts.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "原価", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("ClaimPrice") && parts.ClaimPrice != null)
            {
                if (!Regex.IsMatch(parts.ClaimPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("ClaimPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "クレーム申請部品代", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("MpPrice") && parts.MpPrice != null)
            {
                if (!Regex.IsMatch(parts.MpPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("MpPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "MP価格", "正の整数のみ" }));
                }
            }
            if (ModelState.IsValidField("EoPrice") && parts.EoPrice != null)
            {
                if (!Regex.IsMatch(parts.EoPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("EoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "E/O価格", "正の整数のみ" }));
                }
            }
            // Add 2014/11/07 arc nakayama 部品項目追加対応
            if (ModelState.IsValidField("QuantityPerUnit1") && parts.QuantityPerUnit1 != null)
            {
                if (!Regex.IsMatch(parts.QuantityPerUnit1.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("QuantityPerUnit1", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "数量", "正の整数のみ" }));
                }
            }

            return parts;
        }

        /// <summary>
        /// 部品マスタ追加データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="parts">部品データ(登録内容)</param>
        /// <returns>部品マスタモデルクラス</returns>
        /// <history>
        /// 2016/06/03 arc yano #3570 部品マスタ編集画面　在庫棚卸対象・非対象設定の項目追加
        /// </history>
        private Parts EditPartsForInsert(Parts parts)
        {
            parts.PartsNumber = Strings.StrConv(parts.PartsNumber, VbStrConv.Narrow, 0);
            parts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.CreateDate = DateTime.Now;
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.LastUpdateDate = DateTime.Now;
            parts.DelFlag = "0";

            //Add 2016/06/03 arc yano #3570
            if (parts.NonInventoryFlag.Equals("true"))
            {
                parts.NonInventoryFlag = "0";
            }
            else
            {
                parts.NonInventoryFlag = "1";
            }
            return parts;
        }

        /// <summary>
        /// 部品マスタ更新データ編集(フレームワーク外の補完編集)
        /// </summary>
        /// <param name="parts">部品データ(登録内容)</param>
        /// <returns>部品マスタモデルクラス</returns>
        /// <history>
        /// 2016/06/03 arc yano #3570 部品マスタ編集画面　在庫棚卸対象・非対象設定の項目追加
        /// </history>
        private Parts EditPartsForUpdate(Parts parts)
        {
            parts.PartsNumber = Strings.StrConv(parts.PartsNumber, VbStrConv.Narrow, 0);
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.LastUpdateDate = DateTime.Now;

            //Add 2016/06/03 arc yano #3570
            if (parts.NonInventoryFlag.Equals("true"))
            {
                parts.NonInventoryFlag = "0";
            }
            else
            {
                parts.NonInventoryFlag = "1";
            }
            return parts;
        }

        /*
        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 部品価格一括更新ダイアログ表示
        /// </summary>
        /// <returns>部品価格一括更新ダイアログ</returns>
        public ActionResult ImportDialog()
        {
            return View("PartsImportDialog");
        }

        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        ///  部品価格一括更新処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            string readText = "";
            ArrayList array = new ArrayList();
            string partsErrMsg = "";
            string line = "";
            string[] splitLine = null;
            string[] csvReadLine = null;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            ModelState.Clear();

            // ファイルの存在チェック
            ValidateImportFile(importFile);
            if (!ModelState.IsValid)
            {
                return View("PartsImportDialog");
            }

            //ストップウォッチを開始する
            sw.Start();

            byte[] data = new Byte[importFile.ContentLength];
            // 指定ファイル内のデータをバイト配列で取得
            importFile.InputStream.Read(data, 0, importFile.ContentLength);

            // バイト配列を文字列(Shift-JIS)変換
            readText = System.Text.Encoding.GetEncoding(932).GetString(data);
            readText = readText.Replace(Environment.NewLine, "\r");
            readText = readText.Trim('\r');
            string[] readLine = readText.Split('\r');

            // arrayListに変換する
            array.AddRange(readLine);

            // 見出し行削除
            array.RemoveAt(0);

            // 重複する部品番号の排除
            DeleteOverLapData(array);

            if (array.Count == 0)
            {
                //ストップウォッチを止める
                sw.Stop();
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "更新データが0件です。更新処理を終了しました"));
                ViewData["Message"] = "CSVファイルデータ0件";
                return View("PartsImportDialog", ViewData);
            }

            //db.Log = new OutputWriter();

            try
            {
                // 読込んだ行数分ループ
                for (int count = 0; count < array.Count; count++)
                {
                    {
                        line = array[count].ToString();
                        // ダブルコーテーションとカンマの検索と置換したデータを取得
                        line = ReplaceQuoteCommaData(array[count].ToString());

                        splitLine = line.Split(',');
                        csvReadLine = new string[splitLine.Count()];
                        csvReadLine = EditCsvQuoteData(splitLine);

                        // 使用する項目の属性チェック
                        ValidateCsvDataProperties(csvReadLine, count);
                        if (!ModelState.IsValid)
                        {
                            //ストップウォッチを止める
                            sw.Stop();
                            ViewData["Message"] = "CSVファイルデータ不備";
                            return View("PartsImportDialog", ViewData);
                        }

                        // 部品番号の存在チェック
                        Parts parts = new PartsDao(db).GetByKey("AR" + csvReadLine[0]);

                        if (parts != null)
                        {
                            // 存在した場合、更新処理を行う
                            EditPartsData(parts, csvReadLine, true);

                            partsErrMsg = "部品番号 = " + csvReadLine[0] + "の";

                            // 登録前にValidationを行う
                            ValidateParts(parts, partsErrMsg);
                            if (!ModelState.IsValid)
                            {
                                //ストップウォッチを止める
                                sw.Stop();
                                ViewData["Message"] = "登録前チェックエラー";
                                return View("PartsImportDialog", ViewData);
                            }

                            db.Parts.DeleteOnSubmit(parts);
                        }
                        else
                        {
                            parts = new Parts();
                            // 存在しなかった場合、登録処理を行う
                            EditPartsData(parts, csvReadLine, false);

                            partsErrMsg = "部品番号 = " + csvReadLine[0] + "の";

                            // 登録前にValidationを行う
                            ValidateParts(parts, partsErrMsg);
                            if (!ModelState.IsValid)
                            {
                                //ストップウォッチを止める
                                sw.Stop();
                                ViewData["Message"] = "登録前チェックエラー";
                                return View("PartsImportDialog", ViewData);
                            }
                        }

                        db.Parts.InsertOnSubmit(parts);
                    }
                }
                db.SubmitChanges();
            }
            catch (SqlException se)
            {
                Session["ExecSQL"] = OutputLogData.sqlText;
                // 一意制約エラーの場合、メッセージを設定し、返す
                if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                {
                    //ストップウォッチを止める
                    sw.Stop();
                    OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                    ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "部品番号", "保存" }));
                    ViewData["Message"] = "部品番号：" + csvReadLine[0] + "にてエラー";
                    return View("PartsImportDialog", ViewData);
                }
                else
                {
                    //ストップウォッチを止める
                    sw.Stop();
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                    return View("Error");
                }
            }
            catch (Exception e)
            {
                //ストップウォッチを止める
                sw.Stop();
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ログに出力
                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                // エラーページに遷移
                return View("Error");
            }
            finally
            {
                //ストップウォッチを止める
                sw.Stop();
            }


            ViewData["Message"] = "更新完了しました。　経過時間：" + String.Format("{0:00}:{1:00}:{2:00}", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds);
            return View("PartsImportDialog", ViewData);

        }
        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 取込ファイル存在チェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Boolean ValidateImportFile(HttpPostedFileBase filePath)
        {
            // 必須チェック
            if (filePath == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("csv") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がcsvファイルではありません"));
                }
            }

            return true;
        }

        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 読込んだCSVデータの項目チェック
        /// </summary>
        /// <param name="impData">CSVデータ(Split済)</param>
        /// <param name="count">レコードカウント</param>
        private void ValidateCsvDataProperties(String[] impData, int count)
        {
            string partsNumberMsg = "";

            partsNumberMsg = "部品番号 = " + impData[0] + "の";

            //必須チェック
            
            //部品番号
            if (string.IsNullOrEmpty(impData[0]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "部品番号が入力されていないCSVデータが存在します。部品番号は入力必須です"));
                return;
            }

            //名称
            if (string.IsNullOrEmpty(impData[1]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0001", partsNumberMsg + "部品名"));
            }

            // D.C
            if (string.IsNullOrEmpty(impData[4]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0001", partsNumberMsg + "DC"));
            }

            //希望小売価格　（税抜き）
            if (!Regex.IsMatch(impData[5], @"^\d{1,10}$"))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0004", new string[] { partsNumberMsg + "希望小売価格　（税抜き）", "正の10桁以内の整数のみ" }));
            }
        }
          
        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 読込んだcsvデータを編集する
        /// </summary>
        /// <param name="parts">parts</param>
        /// <param name="impData">csvデータ</param>
        /// <param name="updateflag">true:更新 false:新規追加</param>
        /// <returns></returns>
        private Parts EditPartsData(Parts parts,String[] impData, Boolean updateflag)
        {
            string dicountCode = impData[4]; // D.C
            Decimal soCost = Decimal.Zero;   // S/O原価
            Decimal eoCost = Decimal.Zero;   // E/O原価
            Decimal claimPrice = Decimal.Zero; // ｸﾚｰﾑ申請部品代
            Decimal mpPrice = Decimal.Zero;     // MP価格
            Decimal rate = Decimal.Zero;        // 各レート計算用
            
            // D.Cをキーに部品割引率を取得
            PartsDiscountRate pdRate = new PartsDiscountRateDao(db).GetByKey(dicountCode);
            if (pdRate != null)
            {
                
                // S/O原価計算
                if (pdRate.SoRate > 0) {
                    rate = Decimal.Divide(pdRate.SoRate, 100m);
                }
                soCost = Decimal.Parse(impData[5]) * (1 - rate);
                soCost = Math.Floor(Decimal.Add(soCost, 0.4m));
                parts.SoPrice = soCost;

                
                // E/O原価
                if (pdRate.EoRate > 0) {
                    rate = Decimal.Divide(pdRate.EoRate, 100m);
                }
                eoCost = Decimal.Parse(impData[5]) * (1 - rate);
                eoCost = Math.Floor(Decimal.Add(eoCost, 0.4m));
                parts.EoPrice = eoCost;

                // ｸﾚｰﾑ申請部品代
                if (pdRate.Warranty > 0)
                {
                    rate = Decimal.Divide(pdRate.Warranty, 100m);
                }
                claimPrice = Math.Floor((1 - rate) * Decimal.Parse(impData[5]) * 1.05m);
                parts.ClaimPrice = claimPrice;

                // MP価格
                mpPrice = Decimal.Parse(impData[5]) * 0.45m;
                mpPrice = Math.Floor(Decimal.Add(mpPrice, 0.4m));
                parts.MpPrice = mpPrice;
            }

            // 新規追加時のみ以下を行う
            if (updateflag == false)
            {
                // 部品番号
                parts.PartsNumber = "AR" + impData[0];

                // 部品名称(日本語)
                parts.PartsNameJp = impData[1];

                // 部品名称(英語)
                parts.PartsNameEn = impData[1];

                // メーカーコード
                parts.MakerCode = "AR";

                // メーカー部品番号
                parts.MakerPartsNumber = impData[0];

                // メーカー部品名称（J）
                parts.MakerPartsNameJp = impData[1];

                // メーカー部品名称（E）
                parts.MakerPartsNameEn = impData[1];
                
                // 純正区分
                parts.GenuineType = "001";

                // 備考（旧部品番号など）
                parts.Memo = "";
                
                // 作成者
                parts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                // 作成日時
                parts.CreateDate = DateTime.Now;

                // 削除フラグ
                parts.DelFlag = "0";
            }

            // 定価
            parts.Price = Decimal.Parse(impData[5]);

            // 販売価格
            parts.SalesPrice = Decimal.Parse(impData[5]);

            // 原価
            parts.Cost = soCost;

            // 最終更新者
            parts.LastUpdateDate = DateTime.Now;

            // 最終更新日時
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            return parts;
        }

        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// ダブルコーテーションの排除処理
        /// </summary>
        /// <param name="quoteData"></param>
        /// <returns></returns>
        private string[] EditCsvQuoteData(string[] quoteData)
        {
            string[] splLine2 = new string[quoteData.Count()];
            ArrayList array2 = new ArrayList();
            string splData = "";

            // ArrayListに格納
            array2.Clear();
            array2.AddRange(quoteData);

            // ダブルコーテーションの文字列を検索
            for (int i = 0; i < array2.Count; i++)
            {

                splData = array2[i].ToString();
                splData = splData.Replace("\"", "");
                splData = splData.Replace(QuoteReplace, "\"");
                splData = splData.Replace(CommaReplace, ",");
                splLine2[i] = splData;
            }

            return splLine2;
        }

        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 重複した部品番号レコードの排除処理
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private ArrayList DeleteOverLapData(ArrayList list)
        {
            ArrayList list2 = new ArrayList(list);
            ArrayList compList = new ArrayList();
            int index = 0;

            list.Clear();

            for (int cnt = 0; cnt < list2.Count; cnt++)
            {
                string strLine = list2[cnt].ToString();
                string[] strList = strLine.Split(',');

                // 同部品番号が存在するかチェック
                index = compList.IndexOf(strList[0]);

                if (index >= 0)
                {
                    // 同部品番号が存在した場合、上書き
                    compList[index] = strList[0];
                    list[index] = strLine;
                }
                else
                {
                    // 同部品番号が存在しなかった場合、新規に追加
                    compList.Add(strList[0]);
                    list.Add(strLine);
                }
            }

            return list;

        }

        // Add 2014/09/16 arc amii 部品価格一括更新対応 新規追加
        /// <summary>
        /// 項目内のダブルコーテーションを別文字に置換する処理
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string ReplaceQuoteCommaData(string line)
        {
            int Quote = 0;
            int start = 0;
            int end = 0;
            string before = "";
            string after = "";
            int strLen = 0;

            // 項目内のダブルコーテーションを別文字に変換
            line = line.Replace("\"\"", QuoteReplace);

            for (int j = 0; j < line.Length - 1; j++)
            {
                // ダブルコーテーションの検索
                if (line.IndexOf("\"", j) >= 0)
                {
                    Quote++;
                }
                else
                {
                    // ヒットしなかった場合、ループ終了し、次の読込データへ
                    j = line.Length;
                    continue;
                }

                // ダブルコーテーションの数によって分岐
                if (Quote == 1)
                {
                    // 1個目の場合

                    //ヒットした位置を覚えておく(開始位置)
                    start = line.IndexOf("\"", j);

                    // 検索位置をヒットした位置以降に設定
                    j = start;
                }
                else if (Quote == 2)
                {
                    // 2個目が見つかった場合
                    //ヒットした位置を覚えておく(終了位置)
                    end = line.IndexOf("\"", j);

                    // 検索位置をヒットした位置以降に設定
                    j = end;

                    // 終了位置 - 開始位置で文字数を取得
                    strLen = end - start;

                    // 文字列切り出し
                    before = line.Substring(start, strLen + 1);

                    // 切り出した文字列内のカンマを変換
                    after = before.Replace(",", CommaReplace);

                    // 変換前の文字列を変換後の文字列で置換する
                    line = line.Replace(before, after);
                    Quote = 0;
                }
            }

            return line;
        }
        */

        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<PartsExcelImportList> ImportList = new List<PartsExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("PartsImportDialog", ImportList);
        }


        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<PartsExcelImportList> ImportList = new List<PartsExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog");
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog");
                    }

                    //DB登録
                    DBExecute(ImportList, form);
                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("PartsImportDialog");

                //--------------
                //キャンセル
                //--------------
                case "2":

                    ImportList = new List<PartsExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため

                    return View("PartsImportDialog", ImportList);

                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("PartsImportDialog");
            }
        }
        #endregion

        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        private List<PartsExcelImportList> ReadExcelData(HttpPostedFileBase importFile, List<PartsExcelImportList> ImportList)
        {
            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets[1];

                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
                    return ImportList;
                }

                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　       //行の開始位置
                int EndRow = ws.Dimension.End.Row;             //行の終了位置
                int StartCol = ws.Dimension.Start.Column;      //列の開始位置
                int EndCol = ws.Dimension.End.Column;          //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];

                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[ws.Dimension.End.Column];

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        Result[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region Excel読み取り前のチェック
        /// <summary>
        /// Excel読み取り前のチェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        private void ValidateExcelFile(HttpPostedFileBase filePath, FormCollection form)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            if (string.IsNullOrWhiteSpace(form["Memo"]))
            {
                ModelState.AddModelError("Memo", "コメントは入力必須です");
            }

            return;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        public void ValidateImportList(List<PartsExcelImportList> ImportList)
        {
            //Parts condition = new Parts();
            //condition.DelFlag = "0";

            ////部品リストを取得する
            //List<Parts> partsList = new PartsDao(db).GetListByCondition(condition);

            //メーカーリストを取得する
            List<Maker> makerList = new MakerDao(db).GetMakerBykey();

            for (int i = 0; i < ImportList.Count; i++)
            {
                //----------------
                //部品番号
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].PartsNumber))  //必須チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の部品番号が入力されていません。部品番号"));
                }
                else if (ImportList[i].PartsNumber.Length > 25)   //データ長チェック
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目の部品番号", "25" }));
                }
                else //マスタチェック
                {
                    //Parts rec = partsList.Where(x => Strings.StrConv(x.PartsNumber, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].PartsNumber)).FirstOrDefault();
                    Parts rec = new PartsDao(db).GetByKey(ImportList[i].PartsNumber);

                    //部品リストから検索して見つからなかった場合は
                    if (rec == null)
                    {
                        ImportList[i].NewPartsFlag = true;
                    }
                }

                //新規部品の場合のみ部品のチェックを行う
                if (ImportList[i].NewPartsFlag)
                {
                    //----------------
                    //部品名
                    //----------------
                    //必須チェック
                    if (string.IsNullOrEmpty(ImportList[i].PartsNameJp))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目の部品名が入力されていません。部品名"));
                    }
                    else if (ImportList[i].PartsNameJp.Length > 50)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目の部品名", "50" }));
                    }
                    //--------------------
                    //部品名(英語)
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].PartsNameEn) && ImportList[i].PartsNameEn.Length > 50)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目の部品名(英語)", "50" }));
                    }
                    //-----------------
                    //メーカーコード
                    //-----------------
                    if (string.IsNullOrEmpty(ImportList[i].MakerCode)) //必須チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目のメーカーコードが入力されていません。メーカーコード"));
                    }
                    else //マスタチェック
                    {
                        Maker maker = makerList.Where(x => Strings.StrConv(x.MakerCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].MakerCode)).FirstOrDefault();

                        if (maker == null)
                        {
                            ModelState.AddModelError("", i + 1 + "行目のメーカーコードがマスタに登録されていません");
                        }
                    }
                    //--------------------
                    //メーカー部品番号
                    //--------------------
                    //必須チェック
                    if (string.IsNullOrEmpty(ImportList[i].MakerPartsNumber))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "行目のメーカー部品番号入力されていません。メーカー部品番号"));
                    }
                    else if (ImportList[i].MakerPartsNumber.Length > 25)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目のメーカー部品番号", "25" }));
                    }
                    //--------------------
                    //メーカー部品名
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].MakerPartsNameJp) && ImportList[i].MakerPartsNameJp.Length > 50)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目のメーカー部品名", "50" }));
                    }
                    //-----------------------
                    //メーカー部品名(英語)
                    //-----------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].MakerPartsNameEn) && ImportList[i].MakerPartsNameEn.Length > 50)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目のメーカー部品名(英語)", "50" }));
                    }
                    //-------------------
                    //純正区分
                    //-------------------
                    if (string.IsNullOrEmpty(ImportList[i].GenuineType) || (!ImportList[i].GenuineType.Equals("001") && !ImportList[i].GenuineType.Equals("002")))
                    {
                        ModelState.AddModelError("", i + 1 + "行目の純正区分には「001(純正品)」または「002(社外品)」を入力してください");
                    }
                    //--------------------
                    //単位
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].UnitCD1) && ImportList[i].UnitCD1.Length > 3)   //データ長チェック
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "行目の単位", "3" }));
                    }
                    //---------------------
                    //単位あたりの数量
                    //---------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].QuantityPerUnit1) && !Regex.IsMatch(ImportList[i].QuantityPerUnit1, @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目の単位あたりの数量", "正の整数10桁以内" }));
                    }
                }
                //--------------
                //価格
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].Price) && !Regex.IsMatch(ImportList[i].Price, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目の価格", "正の整数10桁以内" }));
                }
                //--------------
                //販売価格
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].SalesPrice) && !Regex.IsMatch(ImportList[i].SalesPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目の販売価格", "正の整数10桁以内" }));
                }
                //--------------
                //S/O価格
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].SoPrice) && !Regex.IsMatch(ImportList[i].SoPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目のS/O価格", "正の整数10桁以内" }));
                }
                //--------------
                //原価
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].Cost) && !Regex.IsMatch(ImportList[i].Cost, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目の原価", "正の整数10桁以内" }));
                }
                //---------------------
                //クレーム申請部品代
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].ClaimPrice) && !Regex.IsMatch(ImportList[i].ClaimPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目のクレーム申請部品代", "正の整数10桁以内" }));
                }
                //---------------------
                //MP価格
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].MpPrice) && !Regex.IsMatch(ImportList[i].MpPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目のMP価格", "正の整数10桁以内" }));
                }
                //---------------------
                //E/O価格
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].EoPrice) && !Regex.IsMatch(ImportList[i].EoPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "行目のE/O価格", "正の整数10桁以内" }));
                }
                //---------------------
                //棚卸対象外フラグ
                //---------------------
                if (string.IsNullOrWhiteSpace(ImportList[i].NonInventoryFlag) || (!ImportList[i].NonInventoryFlag.Equals("0") && !ImportList[i].NonInventoryFlag.Equals("1")))
                {
                    ModelState.AddModelError("", i + 1 + "行目の棚卸対象外フラグには0か1を入力してください");
                }
            }

            //-----------------
            //重複チェック
            //-----------------
            var ret = ImportList.GroupBy(x => x.PartsNumber).Select(c => new { PartsNumber = c.Key, Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in ret)
            {
                if (!string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("", "取込むファイルの中に部品番号" + a.PartsNumber + "が複数定義されています");
                }
            }

        }
        #endregion

        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        public List<PartsExcelImportList> SetProperty(ref string[] Result, ref List<PartsExcelImportList> ImportList)
        {
            PartsExcelImportList SetLine = new PartsExcelImportList();

            // 部品番号番号
            SetLine.PartsNumber = Result[0];
            // 部品名称(日)
            SetLine.PartsNameJp = Result[1];
            // 部品名称(英)
            SetLine.PartsNameEn = Result[2];
            // メーカーコード
            SetLine.MakerCode = Result[3];
            // メーカー部品番号
            SetLine.MakerPartsNumber = Result[4];
            // メーカー部品名(日)
            SetLine.MakerPartsNameJp = Result[5];
            // メーカー部品名(英)
            SetLine.MakerPartsNameEn = Result[6];
            // 価格
            SetLine.Price = Result[7];
            // 販売価格
            SetLine.SalesPrice = Result[8];
            // So価格
            SetLine.SoPrice = Result[9];
            // 原価
            SetLine.Cost = Result[10];
            // クレーム申請部品代
            SetLine.ClaimPrice = Result[11];
            // Mo価格
            SetLine.MpPrice = Result[12];
            // Eo価格
            SetLine.EoPrice = Result[13];
            // 純正区分
            SetLine.GenuineType = Result[14].PadLeft(3, '0');
            // 単位
            SetLine.UnitCD1 = Result[15];
            // 単位あたりの数量
            SetLine.QuantityPerUnit1 = Result[16];
            //棚卸対象外フラグ
            SetLine.NonInventoryFlag = Result[17];

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region ダイアログのデータ付きコンポーネント設定
        /// <summary>
        /// ダイアログのデータ付きコンポーネント設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["Memo"] = form["Memo"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel取込(部品価格改定)
        /// </history>
        private void DBExecute(List<PartsExcelImportList> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                List<Parts> workList = new List<Parts>();

                //部品マスタの取得
                //Parts condition = new Parts();

                //List<Parts> partsList = new PartsDao(db).GetListByCondition(condition);

                decimal ret = 0m; //string→decimalに変換された値

                //発注テーブルを更新して、入荷テーブルを登録する
                foreach (var LineData in ImportList)
                {
                    //新規登録分
                    if (LineData.NewPartsFlag)
                    {
                        //------------------------
                        //部品マスタ登録
                        //------------------------
                        Parts newParts = new Parts();
                        newParts.PartsNumber = LineData.PartsNumber;                  //部品番号
                        newParts.PartsNameJp = LineData.PartsNameJp;                  //部品名
                        newParts.PartsNameEn = LineData.PartsNameEn;                  //部品名(英語)
                        newParts.MakerCode = LineData.MakerCode;                      //メーカーコード
                        newParts.MakerPartsNumber = LineData.MakerPartsNumber;        //メーカー部品番号
                        newParts.MakerPartsNameJp = LineData.MakerPartsNameJp;        //メーカー部品名
                        newParts.MakerPartsNameEn = LineData.MakerPartsNameEn;        //メーカー部品名(英語)

                        //価格
                        if (Decimal.TryParse(LineData.Price, out ret))
                        {
                            newParts.Price = ret;
                        }
                        //販売価格
                        if (Decimal.TryParse(LineData.SalesPrice, out ret))
                        {
                            newParts.SalesPrice = ret;
                        }
                        //S/O価格
                        if (Decimal.TryParse(LineData.SoPrice, out ret))
                        {
                            newParts.SoPrice = ret;
                        }
                        //原価
                        if (Decimal.TryParse(LineData.Cost, out ret))
                        {
                            newParts.Cost = ret;
                        }
                        //クレーム申請部品代
                        if (Decimal.TryParse(LineData.ClaimPrice, out ret))
                        {
                            newParts.ClaimPrice = ret;
                        }
                        //MP価格
                        if (Decimal.TryParse(LineData.MpPrice, out ret))
                        {
                            newParts.MpPrice = ret;
                        }
                        //E/O価格
                        if (Decimal.TryParse(LineData.EoPrice, out ret))
                        {
                            newParts.EoPrice = ret;
                        }

                        newParts.GenuineType = LineData.GenuineType;                                      //純正区分
                        newParts.Memo = form["Memo"];                                                     //メモ
                        newParts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;       //作成者
                        newParts.CreateDate = DateTime.Now;                                               //作成日時
                        newParts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;   //最終更新者
                        newParts.LastUpdateDate = DateTime.Now;                                           //最終更新日
                        newParts.DelFlag = "0";                                                           //削除フラグ
                        newParts.UnitCD1 = LineData.UnitCD1;

                        //単位あたりの数量
                        if (Decimal.TryParse(LineData.QuantityPerUnit1, out ret))
                        {
                            newParts.QuantityPerUnit1 = ret;
                        }

                        newParts.NonInventoryFlag = LineData.NonInventoryFlag;                            //棚卸対象外フラグ

                        workList.Add(newParts);
                    }
                    else
                    {
                        //対象の部品マスタを取得
                        //Parts target = partsList.Where(x => x.PartsNumber.Equals(LineData.PartsNumber)).FirstOrDefault();
                        Parts target = new PartsDao(db).GetByKey(LineData.PartsNumber);


                        //部品番号以外を更新
                        target.PartsNameJp = LineData.PartsNameJp;                  //部品名
                        target.PartsNameEn = LineData.PartsNameEn;                  //部品名(英語)
                        target.MakerCode = LineData.MakerCode;                      //メーカーコード
                        target.MakerPartsNumber = LineData.MakerPartsNumber;        //メーカー部品番号
                        target.MakerPartsNameJp = LineData.MakerPartsNameJp;        //メーカー部品名
                        target.MakerPartsNameEn = LineData.MakerPartsNameEn;        //メーカー部品名(英語)

                        //価格
                        if (Decimal.TryParse(LineData.Price, out ret))
                        {
                            target.Price = ret;
                        }
                        //販売価格
                        if (Decimal.TryParse(LineData.SalesPrice, out ret))
                        {
                            target.SalesPrice = ret;
                        }
                        //S/O価格
                        if (Decimal.TryParse(LineData.SoPrice, out ret))
                        {
                            target.SoPrice = ret;
                        }
                        //原価
                        if (Decimal.TryParse(LineData.Cost, out ret))
                        {
                            target.Cost = ret;
                        }
                        //クレーム申請部品代
                        if (Decimal.TryParse(LineData.ClaimPrice, out ret))
                        {
                            target.ClaimPrice = ret;
                        }
                        //MP価格
                        if (Decimal.TryParse(LineData.MpPrice, out ret))
                        {
                            target.MpPrice = ret;
                        }
                        //E/O価格
                        if (Decimal.TryParse(LineData.EoPrice, out ret))
                        {
                            target.EoPrice = ret;
                        }

                        target.GenuineType = LineData.GenuineType;                                      //純正区分
                        target.Memo = form["Memo"];                                                     //メモ
                        target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;   //最終更新者
                        target.LastUpdateDate = DateTime.Now;                                           //最終更新日
                        target.DelFlag = "0";                                                           //削除フラグ
                        target.UnitCD1 = LineData.UnitCD1;

                        //単位あたりの数量
                        if (Decimal.TryParse(LineData.QuantityPerUnit1, out ret))
                        {
                            target.QuantityPerUnit1 = ret;
                        }

                        target.NonInventoryFlag = LineData.NonInventoryFlag;                            //棚卸対象外フラグ

                    }
                }

                db.Parts.InsertAllOnSubmit(workList);

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion
    }
}