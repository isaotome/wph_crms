using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;              //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.Linq;         //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.SqlClient;    //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano 他のコントローラと同じく、フィルタ属性(例外、セキュリティ、出力キャッシュ)を追加
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarReservationCancelController : Controller
    {

        //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "引当解除";   // 画面名
        private static readonly string PROC_NAME = "在庫に戻す"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarReservationCancelController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            return Criteria(new FormCollection(), null);
        }

        /// <summary>
        /// 検索処理
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form,List<CarSalesHeader> item) {

            if (form["ActionType"]!=null && form["ActionType"].Equals("update")) {
                // Add 2014/08/11 arc amii エラーログ対応 登録用にDataContextを設定する
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                string prefix = string.Format("item[{0}].",form["RowId"]);
                string locationCode = form[prefix+"LocationCode"];
                if (string.IsNullOrEmpty(locationCode)) {
                    //ModelState.AddModelError(prefix + "LocationCode", MessageUtils.GetMessage("E0001", "ロケーション"));
                    PaginatedList<CarSalesHeader> model = GetSearchResult(form);
                    return View("CarReservationCancelCriteria", model);
                }
                string salesCarNumber = form[prefix+"SalesCarNumber"];
                SalesCar target = new SalesCarDao(db).GetByKey(salesCarNumber);
                target.LocationCode = locationCode;
                target.CarStatus = "001";
                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //Add 2014/08/11 arc amii エラーログ対応 ログ出力の為のtry catch文を追加
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // データ操作の実行・コミット
                        db.SubmitChanges();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            ModelState.Clear();
            PaginatedList<CarSalesHeader> list = GetSearchResult(form);
            return View("CarReservationCancelCriteria", list);
        }
        private PaginatedList<CarSalesHeader> GetSearchResult(FormCollection form) {
            PaginatedList<CarSalesHeader> list = new CarSalesOrderDao(db).GetCancelList(int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            foreach (var header in list) {
                header.OriginalCarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber.Substring(0, header.SlipNumber.Length - 2));
                header.LocationCode = form[string.Format("item[{0}].LocationCode", header.SalesCarNumber)];
                header.LocationName = form[string.Format("item[{0}].LocationName", header.SalesCarNumber)];
            }
            return list;
        }
    }
}
