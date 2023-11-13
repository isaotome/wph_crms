using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;

using System.Data.SqlClient;
using Crms.Models;
using System.Data.Linq;
using System.Transactions;
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;


//----------------------------------------------------------------------
//機能　：メカニックランキング
//作成日：2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
//
//----------------------------------------------------------------------
namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MechanicRankingController : Controller
    {
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
        public MechanicRankingController()
        {
            db = new CrmsLinqDataContext();
        }

        private static readonly string FORM_NAME = "メカニックランキング";                        // 画面名
        private static readonly string PROC_NAME_SEARCH = "メカニックランキング検索";           // 処理名（ログ出力用）
        

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
        /// </history>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //初期値の設定
            //対象年月(デフォルトは当月)
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["DefaultTargetDateY"] = form["TargetDateY"];
            form["DefaultTargetDateM"] = form["TargetDateM"];

            return Criteria(form);
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            ModelState.Clear();

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            PaginatedList<GetMechanicRankingResult> list = new PaginatedList<GetMechanicRankingResult>();

            //初回表示の場合
            if (criteriaInit)
            {
                ret = View("MechanicRankingCriteria", list);
            }
            else
            {
                list = new PaginatedList<GetMechanicRankingResult>(SearchList(form).AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE); ;
                ret = View("MechanicRankingCriteria", list);
            }

            //画面設定
            SetComponent(form);

            return ret;
        }

        /// <summary>
        /// メカニックランキング検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private List<GetMechanicRankingResult> SearchList(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //対象年月の設定
            DateTime TargetDateFrom = DateTime.Parse(dao.GetYear(true, form["TargetDateY"]).Name + "/" + dao.GetMonth(true, form["TargetDateM"]).Name + "/01");  //年と月をつなげる     

            List<GetMechanicRankingResult> list = new MechanicRankingDao(db).GetList(TargetDateFrom);

            return list;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            //------------
            //対象年
            //------------
            //対象年(ドロップダウン)
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetDateY"], false);
            //対象年(テキスト)
            ViewData["TextTargetDateY"] = dao.GetYear(true, form["TargetDateY"]).Name;
            //デフォルト対象年
            ViewData["DefaultTargetDateY"] = form["DefaultTargetDateY"];

            //------------
            //対象月
            //------------
            //対象月(ドロップダウン)
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetDateM"], false);
            //対象月(テキスト)
            ViewData["TextTargetDateM"] = dao.GetMonth(true, form["TargetDateM"]).Name;
            //デフォルト対象月
            ViewData["DefaultTargetDateM"] = form["DefaultTargetDateM"];
            return;
        }
        #endregion
    }
}
