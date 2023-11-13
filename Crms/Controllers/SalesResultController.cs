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
using Microsoft.VisualBasic;
using System.Transactions;
using System.Text.RegularExpressions;


namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加		
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]		
    public class SalesResultController : Controller
    {

        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        //Add 2014/10/08 arc yano #3080_顧客検索機能の新設対応その２
        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SalesResultController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion
        //
        // GET: /SalesResult/

        #region 販売実績検索
        /// <summary>
        /// 販売実績検索画面表示
        /// </summary>
        /// <returns>販売実績検索画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;

            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 販売実績検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>販売実績検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {

            PaginatedList<SalesResult> list = new PaginatedList<SalesResult>();
            if (criteriaInit == true)  //画面遷移時
            {
                //何もしない
            }
            else　//検索ボタンクリック時
            {
                // デフォルト値の設定
                form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

                // 検索結果リストの取得
                list = GetSalesResultList(form);
            }
            // その他出力項目の設定
            SetComponent(form);

            // 顧客検索画面の表示
            return View("SalesResultCriteria", list);
        }
        #endregion

        //Add 2014/09/17 arc yano #3080 (顧客検索機能の新設)
        /// <summary>
        /// 販売実績リスト取得
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>販売実績リスト</returns>
        private PaginatedList<SalesResult> GetSalesResultList(FormCollection form)
        {

            V_SalesResultDao vSalesResult = new V_SalesResultDao(db);

            SalesResult sresultCondition = new SalesResult();
            sresultCondition.CustomerCode = form["CustomerCode"];
            sresultCondition.CustomerName = form["CustomerName"];
            sresultCondition.SalesCarNumber = form["SalesCarNumber"];
            sresultCondition.Vin = form["Vin"];
            sresultCondition.CarSlipNumber = form["CarSlipNumber"];
            sresultCondition.CarSalesDateFrom = CommonUtils.StrToDateTime(form["CarSalesDateFrom"], DaoConst.SQL_DATETIME_MAX);
            sresultCondition.CarSalesDateTo = CommonUtils.StrToDateTime(form["CarSalesDateTo"], DaoConst.SQL_DATETIME_MIN);
            //Mod 2014/10/07 arc yano #3080_顧客検索機能の新設対応その２　開始日のみ入力されている場合は、終了日も同じ値を設定する。
            if ((sresultCondition.CarSalesDateFrom != null) && (sresultCondition.CarSalesDateTo == null))
            {
                sresultCondition.CarSalesDateTo = sresultCondition.CarSalesDateFrom;
            }

            sresultCondition.ServiceSlipNumber = form["ServiceSlipNumber"];
            sresultCondition.ServiceSalesDateFrom = CommonUtils.StrToDateTime(form["ServiceSalesDateFrom"], DaoConst.SQL_DATETIME_MAX);
            sresultCondition.ServiceSalesDateTo = CommonUtils.StrToDateTime(form["ServiceSalesDateTo"], DaoConst.SQL_DATETIME_MIN);
            //Mod 2014/10/07 arc yano #3080_顧客検索機能の新設対応その２　開始日のみ入力されている場合は、終了日も同じ値を設定する。
            if ((sresultCondition.ServiceSalesDateFrom != null) && (sresultCondition.ServiceSalesDateTo == null))
            {
                sresultCondition.ServiceSalesDateTo = sresultCondition.ServiceSalesDateFrom;
            }

            //Add 2014/10/07 arc yano #3080_顧客検索機能の新設対応その２ 顧客名(カナ)、登録番号【種別、かな、プレート】、型式の追加
            sresultCondition.RegistrationNumberType = form["RegistrationNumberType"];
            sresultCondition.RegistrationNumberKana = form["RegistrationNumberKana"];
            sresultCondition.RegistrationNumberPlate = form["RegistrationNumberPlate"];
            sresultCondition.CustomerNameKana = form["CustomerName"];
            sresultCondition.ModelName = form["ModelName"];

            //Add 2014/10/29 arc yano #3080_顧客検索機能の新設対応その３ 検索条件「陸運局コード」の追加
            sresultCondition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];

            return vSalesResult.GetResult(int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE, sresultCondition);
        }


        #region 画面コンポーネント設定
        /// <summary>
        /// 各コントロールの値の設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        private void SetComponent(FormCollection form)
        {
            // その他出力項目の設定
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["Vin"] = form["Vin"];
            ViewData["CarSlipNumber"] = form["CarSlipNumber"];
            ViewData["CarSalesDateFrom"] = form["CarSalesDateFrom"];
            ViewData["CarSalesDateTo"] = form["CarSalesDateTo"];
            ViewData["ServiceSlipNumber"] = form["ServiceSlipNumber"];
            ViewData["ServiceSalesDateFrom"] = form["ServiceSalesDateFrom"];
            ViewData["ServiceSalesDateTo"] = form["ServiceSalesDateTo"];

            //Add 2014/10/07 arc yano #3080_顧客検索機能の新設対応その２ 顧客名(カナ)、登録番号【種別、かな、プレート】、型式の追加
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            ViewData["ModelName"] = form["ModelName"];

            //Add 2014/10/29 arc yano #3080_顧客検索機能の新設対応その３ 検索条件「陸運局コード」の追加
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
        }
        #endregion
    }
}
