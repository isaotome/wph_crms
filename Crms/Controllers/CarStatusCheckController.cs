using System.Collections.Generic;
using System.Web.Mvc;
using CrmsDao;

using Crms.Models;



//----------------------------------------------------------------------
//機能　：車両追跡
//作成日：2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
//
//----------------------------------------------------------------------
namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarStatusCheckController : Controller
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
        public CarStatusCheckController()
        {
            db = new CrmsLinqDataContext();
        }


        private static readonly string FORM_NAME = "車両追跡";                                  // 画面名
        private static readonly string PROC_NAME_SEARCH = "検索処理"; 			                // 処理名（ログ出力用）

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            return Criteria(form);
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            ModelState.Clear();

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            CarStatusCheck ret = new CarStatusCheck();

            //初回表示以外の場合は検索処理を行う
            if (!criteriaInit)
            {
                //車台番号の取得
                SalesCar rec = new SalesCarDao(db).GetByKey(form["SalesCarNumber"], true);

                if (rec != null)
                {
                    form["Vin"] = rec.Vin;
                }
                

                ret = SearchList(form, ret);
            }

            //画面設定
            SetComponent(form);

            return View("CarStatusCheckCriteria", ret);
        }

        /// <summary>
        /// 車両追跡データ検索
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// /// <param name="ret">車両追跡結果</param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/19 arc yano #3721 サブシステム移行(車両追跡) 新規作成
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private CarStatusCheck SearchList(FormCollection form, CarStatusCheck ret)
        {
            CodeDao dao = new CodeDao(db);

            //管理番号または車台番号が入力されている場合に検索を実行する
            if (!string.IsNullOrWhiteSpace(form["SalesCarNumber"]) || !string.IsNullOrWhiteSpace(form["Vin"]))
            {
                //車両基本情報取得
                ret.GetCarBasicInfoResult = new CarStatusCheckDao(db).GetCarBasicInfo(form["SalesCarNumber"], form["Vin"]);

                //車両遷移
                ret.GetCarStatusTransitionResult = new CarStatusCheckDao(db).GetCarStatusTransition(form["SalesCarNumber"], form["Vin"]);

                //車両販売伝票
                ret.GetCarSalesSlipResult = new CarStatusCheckDao(db).GetCarSalesSlip(form["SalesCarNumber"], form["Vin"]);
            }

            return ret;
        }

        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        private void SetComponent(FormCollection form)
        {
            //管理番号
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];

            //車台番号
            ViewData["Vin"] = form["Vin"];

            return;
        }
        #endregion

    }
}
