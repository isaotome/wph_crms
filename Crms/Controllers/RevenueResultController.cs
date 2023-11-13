using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Text.RegularExpressions;
using CrmsReport;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;    

namespace Crms.Controllers
{
    /// <summary>
    /// 入金実績リスト
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class RevenueResultController : Controller
    {
        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RevenueResultController()
        {
            this.db = new CrmsLinqDataContext();
        }
        #endregion

        #region 検索画面
        /// <summary>
        /// 入金実績検索画面表示
        /// </summary>
        /// <returns>入金実績検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            // del 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　デフォルトの日付削除
            return Criteria(form);
        }
        
        
        /// <summary>
        /// 入金実績検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金実績検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
                
            PaginatedList<V_RevenueResult> list = new PaginatedList<V_RevenueResult>();

            // Mod 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　必須項目がなくなったため、validationチェックをしない(日付の書式チェックはjsで行う)
            if (!criteriaInit) {
                SetDataComponent(form);
                list = GetSearchResultList(form);             
                                   
            }
            //表示項目の再セット
            SetDataComponent(form);         
            
                     
            return View("RevenueResultCriteria", list);
        }
        
        /// <summary>
        /// 入金実績検索結果リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金実績検索結果リスト</returns>
        private PaginatedList<V_RevenueResult> GetSearchResultList(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            V_RevenueResultDao v_RevenueResultDao = new V_RevenueResultDao(db);
            V_RevenueResult v_RevenueResultDaoCondition = new V_RevenueResult();
            v_RevenueResultDaoCondition.DepartmentCode = form["DepartmentCode"];
            v_RevenueResultDaoCondition.CustomerCode = form["CustomerCode"];
            v_RevenueResultDaoCondition.SlipNumber = form["SlipNumber"];
            // Mod 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　日付の検索は受注日のみになったためその他の日付項目削除　また　From～To検索にする
            v_RevenueResultDaoCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["TargetDateFrom"], DaoConst.SQL_DATETIME_MAX);      //対象日付（受注日）From
            v_RevenueResultDaoCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["TargetDateFromTo"], DaoConst.SQL_DATETIME_MAX);      //対象日付（受注日）To
            //開始日だけ入力されていた場合は終了日に同じ値をセットする
            if ((v_RevenueResultDaoCondition.SalesOrderDateFrom != null) && v_RevenueResultDaoCondition.SalesOrderDateTo == null)
            {
                v_RevenueResultDaoCondition.SalesOrderDateTo = v_RevenueResultDaoCondition.SalesOrderDateFrom;
            }
           

            return v_RevenueResultDao.GetListByCondition(v_RevenueResultDaoCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }


        public void SetDataComponent(FormCollection form)
        {
            // Mod 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　画面の項目数が減ったため一部削除
            CodeDao dao = new CodeDao(db);
            ViewData["TargetDateFrom"] = form["TargetDateFrom"];
            ViewData["TargetDateFromTo"] = form["TargetDateFromTo"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CustomerCode"] = form["CustomerCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(form["CustomerCode"]))
            {
                Customer customer = new CustomerDao(db).GetByKey(form["CustomerCode"]);
                if (customer != null)
                {
                    ViewData["CustomerName"] = customer.CustomerName;
                }
            }
        }

        // Mod 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　必須項目がなくなったため、validationチェックをしない(日付の書式チェックはjsで行う) ValidateRevenueResultメソッドを削除

        #endregion
        
        #region 詳細画面
        /// <summary>
        /// 入金実績詳細画面表示
        /// </summary>
        /// <returns>入金実績詳細画面</returns>
        [AuthFilter]
        public ActionResult Detail(string SlipNo, string CustomerCode, string ST)
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            form["CustomerCode"] = CustomerCode;
            form["SlipNumber"] = SlipNo;
            form["ST"] = ST;

            return Detail(form);
        }

        /// <summary>
        /// 入金実績詳細画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>入金実績詳細画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Detail(FormCollection form)
        {
            Customer customer = new Customer();

            if (!string.IsNullOrWhiteSpace(form["CustomerCode"].ToString()))
            {
                //顧客情報の取得
                customer = new CustomerDao(db).GetByKey(form["CustomerCode"].ToString()); 
            }
           
            //Viewの設定
            SetComponent(form);

            return View("RevenueResultDetail", customer);
        }

        /// <summary>
        /// 車両情報表示(子アクションのみ対応)
        /// </summary>
        /// <param name="carSalesNumber">管理番号</param>
        /// <returns>車両情報</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 30)]
        public ActionResult CarInformation(string customerCode, string slipNumber)
        {
            SalesCar salesCar = new SalesCar();

            if (!string.IsNullOrWhiteSpace(customerCode) && !string.IsNullOrWhiteSpace(slipNumber))
            {
                salesCar = new V_RevenueResultDao(db).GetCarInformation(customerCode, slipNumber);
            }
           
            return PartialView("_CarInformation", salesCar);
        }

        /// <summary>
        /// 入金予定一覧表示(子アクションのみ対応)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金予定一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult ReceiptPlanList(string customerCode, string slipNumber, FormCollection form)
        {
            //ページ番号取得
            int idindex = 0;
            int pageIndex = getPageIndex(form, idindex);

            //ページ番号の保持
            ViewData["PlanId"] = pageIndex;

            PaginatedList<V_ReceiptPlanList> list = new V_RevenueResultDao(db).GetReceiptPlanList(customerCode, slipNumber, pageIndex, DaoConst.PAGE_SIZE, false);

            return PartialView("_ReceiptPlanList", list);
        }

        /// <summary>
        /// 入金予定一覧(サマリ)表示(子アクションのみ対応)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金予定一覧（サマリ）</returns>
        /// <history>Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）サマリ表示追加</history>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult SumReceiptPlanList(string customerCode, string slipNumber, FormCollection form)
        {
            //ページ番号取得
            int idindex = 0;
            int pageIndex = getPageIndex(form, idindex);

            //ページ番号の保持
            ViewData["PlanId"] = pageIndex;

            PaginatedList<V_ReceiptPlanList> list = new V_RevenueResultDao(db).GetReceiptPlanList(customerCode, slipNumber, pageIndex, DaoConst.PAGE_SIZE, true);

            return PartialView("_SumReceiptPlanList", list);
        }



        /// <summary>
        /// 入金実績一覧表示(子アクションのみ対応)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金実績一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult ReceiptResultList(string customerCode, string slipNumber, FormCollection form)
        {
            //ページ番号取得
            int idindex = 1;
            int pageIndex = getPageIndex(form, idindex);

            //ページ番号の保持
            ViewData["ResultId"] = pageIndex;

             RecieptPlanReSultList list = new V_RevenueResultDao(db).GetReceiptResultList(customerCode, slipNumber, pageIndex, DaoConst.PAGE_SIZE);
            
            return PartialView("_ReceiptResultList", list);
        }

        
        /// <summary>
        /// 入金履歴一覧表示(子アクションのみ対応)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>入金履歴一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult ReceiptHistoryList(string customerCode, string slipNumber, FormCollection form)
        {
            //ページ番号取得
            int idindex = 2;
            int pageIndex = getPageIndex(form, idindex);

            //ページ番号の保持
            ViewData["HistoryId"] = pageIndex;

            PaginatedList<V_ReceiptList> list = new V_RevenueResultDao(db).GetReceiptHistoryList(customerCode, slipNumber, pageIndex , DaoConst.PAGE_SIZE);

            return PartialView("_ReceiptHistoryList", list);
        }
        
        /// <summary>
        /// カード入金実績一覧表示(子アクションのみ対応)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="form">フォームデータ</param>
        /// <returns>カード入金実績一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult ReceiptResultCardList(string customerCode, string slipNumber, FormCollection form)
        {

            //ページ番号取得
            int idindex = 3;
            int pageIndex = getPageIndex(form, idindex);

            //ページ番号の保持
            ViewData["ResultCardId"] = pageIndex;

            PaginatedList<V_ReceiptList> list = new V_RevenueResultDao(db).GetReceiptResultCardList(customerCode, slipNumber, pageIndex, DaoConst.PAGE_SIZE);

            return PartialView("_ReceiptResultCardList", list);
        }

        /// <summary>
        /// ページ番号取得
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>ページ番号</returns>
        private int getPageIndex(FormCollection form, int idindex)
        {
            
            int pageIndex = 0;
            string [] strPageIndex = null;

            if(form["id"] != null){
                
                strPageIndex = (form["id"] ?? "").Split(',');
                
                pageIndex = int.Parse(strPageIndex[idindex]);
            }
            
            return pageIndex;
        }

        /// <summary>
        /// ViewDataの設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetComponent(FormCollection form)
        {

            //viewDataの設定
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["indexid"] = form["indexid"];
            ViewData["ReceiptTargetFlag"] = "1";

            //#3190対応 2015/06/02  arc.ookubo
            //Add 2016/07/19 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）サマリ表示追加
            //入金実績が初期表示
            switch (form["indexid"] ?? "1")
            {
                case "1":   //入金実績
                    ViewData["ReceiptPlanListDisplay"] = false;
                    ViewData["ReceiptResultListDisplay"] = true;
                    ViewData["ReceiptHistoryListDisplay"] = false;
                    ViewData["ReceiptResultCardListDisplay"] = false;
                    ViewData["SumReceiptPlanListDisplay"] = false;
                    break;
                case "2":   //入金履歴
                    ViewData["ReceiptPlanListDisplay"] = false;
                    ViewData["ReceiptResultListDisplay"] = false;
                    ViewData["ReceiptHistoryListDisplay"] = true;
                    ViewData["ReceiptResultCardListDisplay"] = false;
                    ViewData["SumReceiptPlanListDisplay"] = false;
                    break;
                case "3":   //入金実績(カード)
                    ViewData["ReceiptPlanListDisplay"] = false;
                    ViewData["ReceiptResultListDisplay"] = false;
                    ViewData["ReceiptHistoryListDisplay"] = false;
                    ViewData["ReceiptResultCardListDisplay"] = true;
                    ViewData["SumReceiptPlanListDisplay"] = false;
                    break;
                case "4":   //入金予定(サマリ)
                    ViewData["ReceiptPlanListDisplay"] = false;
                    ViewData["ReceiptResultListDisplay"] = false;
                    ViewData["ReceiptHistoryListDisplay"] = false;
                    ViewData["ReceiptResultCardListDisplay"] = false;
                    ViewData["SumReceiptPlanListDisplay"] = true;
                    break;
                default:    //その他(入金予定)
                    ViewData["ReceiptPlanListDisplay"] = true;
                    ViewData["ReceiptResultListDisplay"] = false;
                    ViewData["ReceiptHistoryListDisplay"] = false;
                    ViewData["ReceiptResultCardListDisplay"] = false;
                    break;
            }
        }

       
        #endregion

    }
}
