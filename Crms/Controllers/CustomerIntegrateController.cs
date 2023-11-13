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
using Crms.Models;
using System.Xml.Linq;
using System.IO;
using System.Configuration;
using OfficeOpenXml;
using System.Data;
namespace Crms.Controllers
{
    //Create 2017/03/18 arc nakayama #3722_名寄せツール
    /// <summary>
    /// 名寄せツールコントロールクラス
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerIntegrateController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "名寄せツール";    // 画面名（ログ出力用）
        private static readonly string PROC_NAME_EXEC = "名寄せ処理"; // 処理名（ログ出力用）
        #endregion


        #region 初期化
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
        public CustomerIntegrateController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region 名寄せツール画面表示
        /// <summary>
        /// 名寄せツール画面表示
        /// </summary>
        /// <returns>名寄せツール画面</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            return Criteria(form);
        }

        /// <summary>
        /// 名寄せツール画面表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>名寄せツール画面表示</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, FORM_NAME);

            // 検索項目の設定
            if (criteriaInit)
            {
                //初回表示は何もしない
            }
            else
            {
                //バリデーション
                ValidateCheck(form);
                if (!ModelState.IsValid)
                {
                    SetDataComponent(form);
                    return View("CustomerIntegrateCriteria");
                }
                //名寄せ処理実行
                OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXEC);
                CustomerIntegrate(form);
                ModelState.AddModelError("", "名寄せ処理が完了しました。");
                return View("CustomerIntegrateCriteria");
            }

            SetDataComponent(form);
            return View("CustomerIntegrateCriteria");
        }
        #endregion

        #region バリデーション
        private void ValidateCheck(FormCollection form)
        {
            // 必須チェック
            if (string.IsNullOrEmpty(form["CustomerCode1"]))
            {
                ModelState.AddModelError("CustomerCode1", MessageUtils.GetMessage("E0001", "残したい顧客の顧客コード"));
            }
            if (string.IsNullOrEmpty(form["CustomerCode2"]))
            {
                ModelState.AddModelError("CustomerCode2", MessageUtils.GetMessage("E0001", "消したい顧客の顧客コード"));
            }

            if (form["CustomerCode1"] == form["CustomerCode2"])
            {
                ModelState.AddModelError("", "残したい顧客コードと消したい顧客コードが同じです。");
            }
        }
        #endregion

        #region 名寄せ処理
        private void CustomerIntegrate(FormCollection form)
        {
            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {
                var ret = db.CustomerIntegrate(form["CustomerCode1"], form["CustomerCode2"], ((Employee)Session["Employee"]).EmployeeCode);
                ts.Complete();
            }
        }
        #endregion

        #region 画面コンポーネントセット
        private void SetDataComponent(FormCollection form)
        {
            ViewData["CustomerCode1"] = form["CustomerCode1"];
            ViewData["CustomerCode2"] = form["CustomerCode2"];
            ViewData["CustomerName1"] = form["CustomerName1"];
            ViewData["CustomerName2"] = form["CustomerName2"];
            ViewData["TelNumber1"] = form["TelNumber1"];
            ViewData["TelNumber2"] = form["TelNumber2"];
            ViewData["MobileNumber1"] = form["MobileNumber1"];
            ViewData["MobileNumber2"] = form["MobileNumber2"];
            ViewData["PostCode1"] = form["PostCode1"];
            ViewData["PostCode2"] = form["PostCode2"];
            ViewData["Prefecture1"] = form["Prefecture1"];
            ViewData["Prefecture2"] = form["Prefecture2"];
            ViewData["City1"] = form["City1"];
            ViewData["City2"] = form["City2"];
            ViewData["Address1_1"] = form["Address1_1"];
            ViewData["Address1_2"] = form["Address1_2"];
            ViewData["Address2_1"] = form["Address2_1"];
            ViewData["Address2_2"] = form["Address2_2"];
            ViewData["CarCnt1"] = form["CarCnt1"];
            ViewData["CarCnt2"] = form["CarCnt2"];
            ViewData["ServiceCnt1"] = form["ServiceCnt1"];
            ViewData["ServiceCnt2"] = form["ServiceCnt2"];
        }
        #endregion
    }

}
