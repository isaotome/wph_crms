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
using Microsoft.VisualBasic;
using Crms.Models;

namespace Crms.Controllers
{
    /// <summary>
    /// 仕掛品在庫表検索機能コントローラクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsShikakariReportController : Controller
    {

        private static readonly string FORM_NAME = "仕掛在庫表";     // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "仕掛在庫表検索"; // 処理名（ログ出力用）

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsShikakariReportController()
        {
            db = CrmsDataContext.GetDataContext();
        }


        /// <summary>
        /// 仕掛在庫表画面表示
        /// </summary>
        /// <returns>仕掛在庫表画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);   //初期値に当日の年をセットする
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');  //初期値に当日の月をセットする
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            return Criteria(form);
        }


        /// <summary>
        /// 仕掛在庫表検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>仕掛在庫表検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);


            // 検索結果リストの取得
            PartsWipStockAmount list = new PartsWipStockAmount();
            if (criteriaInit)
            {
                //何もしない
            }
            else
            {

                string ToDateYear = DateTime.Today.Year.ToString().Substring(1, 3);     //当日の年
                string ToDateMonth = DateTime.Today.Month.ToString().PadLeft(3, '0');   //当日の月  

                CodeDao dao = new CodeDao(db);
                DateTime TargetDate = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる          
                string strTargetDate = string.Format("{0:yyyyMMdd}", TargetDate);
                InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetByKey(strTargetDate); //対象年月の棚卸データ取得(月１件)
                string NowFlag = ""; //現在のデータか過去データかのフラグ
                //レコードがnullだった場合、空文字にする
                if (rec == null)
                {
                    rec = new InventoryMonthControlParts();
                    rec.InventoryStatus = "";
                }

                //部品棚卸ステータスが確定(002)なら過去データ(InventoryParts_Shikakariテーブル)を取得する
                if (rec.InventoryStatus == "002" && !string.IsNullOrEmpty(rec.InventoryStatus)){
                    NowFlag = "0";
                }else{
                    NowFlag = "1";
                }

                list = GetSearchResultList(form, TargetDate, NowFlag);

            }
            //画面データ再設定
            SetDataComponent(form);

            // 部品検索画面の表示
            return View("PartsShikakariReportCriteria", list);
        }

        /// <summary>
        /// 仕掛在庫表検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>仕掛在庫表検索結果リスト（現在のデータ）</returns>
        private PartsWipStockAmount GetSearchResultList(FormCollection form, DateTime TargetDate, string NowFlag)
        {
            PartsWipStockDao PartsWipStockDao = new PartsWipStockDao(db);
            PartsWipStockAmount Ret = new PartsWipStockAmount();
            Ret = PartsWipStockDao.GetSummaryByCondition(TargetDate, NowFlag);
            Ret.plist = new PaginatedList<GetShikakariSummaryResult>(Ret.list.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return Ret;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetDataComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);  　//対象年
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);　//対象月
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];          //対象日付(yyyy)デフォルト値
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];          //対象日付(MM)デフォルト値
            ViewData["TargetDateY"] = form["TargetDateY"];          //対象日付(yyyy)
            ViewData["TargetDateM"] = form["TargetDateM"];          //対象日付(MM)
        }
        #endregion
    }
}
