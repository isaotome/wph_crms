using System.Collections.Generic;
using System.Web.Mvc;
using CrmsDao;

using Crms.Models;



//----------------------------------------------------------------------
//機能　：翼）整備履歴
//作成日：2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
//
//----------------------------------------------------------------------
namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceSalesHistoryController : Controller
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
        public ServiceSalesHistoryController()
        {
            db = new CrmsLinqDataContext();
        }


        private static readonly string FORM_NAME = "翼）整備履歴";                                // 画面名
        private static readonly string PROC_NAME_SEARCH = "整備履歴検索"; 			            // 処理名（ログ出力用）
        private static readonly string PROC_NAME_LINESEARCH = "整備履歴検索(明細)"; 			// 処理名（ログ出力用）

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //初期設定
            form["DivType"] = "018";

            return Criteria(form);
        }

        /// <summary>
        /// 整備履歴画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
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

            PaginatedList<GetServiceSalesHistoryHeaderResult> list = new PaginatedList<GetServiceSalesHistoryHeaderResult>();

            //初回表示の場合
            if (criteriaInit)
            {
                ret = View("ServiceSalesHistoryCriteria", list);
            }
            else
            {
                list = SearchList(form);
                ret = View("ServiceSalesHistoryCriteria", list);
            }

            //画面設定
            SetComponent(form);

            return ret;
        }

        /// <summary>
        /// 整備履歴明細画面初期表示
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public ActionResult LineCriteria(string SlipNumber, string DivType)
        {
            FormCollection form = new FormCollection();

            //初期設定
            form["SlipNumber"] = SlipNumber;        //伝票番号
            form["DivType"] = DivType;              //拠点タイプ

            return LineCriteria(form);
        }

        /// <summary>
        /// 整備履歴明細画面表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LineCriteria(FormCollection form)
        {
            ModelState.Clear();

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_LINESEARCH);

            ServiceSalesHistoryRet result = new ServiceSalesHistoryRet();

            //ヘッダ部取得
            result.headerList = SearchList(form);

            result.lineList = SearchLineList(form);

            //画面設定
            SetComponent(form);

            return View("ServiceSalesHistoryLineCriteria", result);
        }


        /// <summary>
        /// 整備履歴検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private PaginatedList<GetServiceSalesHistoryHeaderResult> SearchList(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);


            //検索条件
            ServiceSalesHistoryCondition condition = new ServiceSalesHistoryCondition();
            //拠点タイプの設定
            condition.DivType = form["DivType"];
            //部門名の設定
            condition.DepartmentName = (dao.GetCodeNameByKey(form["DivType"], form["Department"], false) != null ? dao.GetCodeNameByKey(form["DivType"], form["Department"], false).Name : "");
            //伝票番号
            condition.SlipNumber = form["SlipNumber"];
            //車台番号
            condition.Vin = form["Vin"];
            //ナンバープレート
            condition.RegistNumber = form["RegistNumber"];
            //顧客名
            condition.CustomerName = form["CustomerName"];
            //顧客名（かな）
            condition.CustomerNameKana = form["CustomerNameKana"];

            PaginatedList<GetServiceSalesHistoryHeaderResult> list = new ServiceSalesHistoryDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return list;
        }

        /// <summary>
        /// 整備履歴(明細)検索
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private PaginatedList<GetServiceSalesHistoryLineResult> SearchLineList(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //検索条件
            ServiceSalesHistoryCondition condition = new ServiceSalesHistoryCondition();
            //拠点タイプの設定
            condition.DivType = form["DivType"];
            //部門名の設定
            condition.DepartmentName = (dao.GetCodeNameByKey(form["DivType"], form["Department"], false) != null ? dao.GetCodeNameByKey(form["DivType"], form["Department"], false).Name : "");
            //伝票番号
            condition.SlipNumber = form["SlipNumber"];
            //車台番号
            condition.Vin = form["Vin"];
            //ナンバープレート
            condition.RegistNumber = form["RegistNumber"];
            //顧客名
            condition.CustomerName = form["CustomerName"];
            //顧客名（かな）
            condition.CustomerNameKana = form["CustomerNameKana"];

            PaginatedList<GetServiceSalesHistoryLineResult> list = new ServiceSalesHistoryDao(db).GetLineListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return list;
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
            CodeDao dao = new CodeDao(db);

            //拠点タイプ
            ViewData["DivType"] = form["DivType"];

            //拠点名
            ViewData["Department"] = form["Department"];

            //拠点リスト
            ViewData["DepartmentNameList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName(form["DivType"], false), form["Department"], true);

            //伝票番号
            ViewData["SlipNumber"] = form["SlipNumber"];

            //車台番号
            ViewData["Vin"] = form["Vin"];

            //ナンバープレート
            ViewData["RegistNumber"] = form["RegistNumber"];

            //顧客名
            ViewData["CustomerName"] = form["CustomerName"];

            //顧客名（かな）
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];


            return;
        }
        #endregion

        #region Ajax専用

        /// <summary>
        /// 拠点リストを取得する(Ajax専用）
        /// </summary>
        /// <param name="code">なし</param>
        /// <returns>拠点リスト</returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public ActionResult GetCodeMasterList(string categorycode)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();

                var codeList = new CodeDao(db).GetCodeName(categorycode, false);

                foreach (var rec in codeList)
                {
                    CodeData codeData = new CodeData();

                    codeData.Code = rec.Code;
                    codeData.Name = rec.Name;
                    codeDataList.DataList.Add(codeData);
                }

                return Json(codeDataList);
            }
            return new EmptyResult();
        }
        #endregion
    }
}
