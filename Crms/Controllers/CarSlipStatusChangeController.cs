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
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarSlipStatusChangeController : Controller
    {
        //Create 2017/05/09 arc nakayama #3761_サブシステムの伝票戻しの移行
        /// <summary>
        /// 車両伝票戻しコントロールクラス
        /// </summary>

        #region 定数
        private static readonly string FORM_NAME = "車両伝票戻し";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "車両伝票戻し検索"; // 処理名（ログ出力用）

        //private static readonly string CANCEL_FROM_CARSLIPSTATUSCHANGE = "1";     //ステータス戻しによる引当解除     //Add 2018/08/07 yano #3911
        //private static readonly string PTN_CANCEL_RESERVATION = "99";             //引当解除           //Add 2018/08/07 yano #3911

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
        public CarSlipStatusChangeController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion


        /// <summary>
        /// 車両伝票戻し検索画面表示
        /// </summary>
        /// <returns>車両伝票戻し検索画面</returns>
        /// <history>
        /// 2020/08/03 yano #4048 【車両伝票ステータス修正】伝票を戻す機能の非表示化
        /// </history>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //Add 2020/08/03 yano #4048
            SecurityRole securityRole = ((Employee)Session["Employee"]).SecurityRole;
            form["DeliveredSlipStatusChange"] = new ApplicationRoleDao(db).GetByKey(securityRole.SecurityRoleCode, "DeliveredSlipStatusChange").EnableFlag.ToString();

            return Criteria(form);
        }

        /// <summary>
        /// 車両伝票戻し検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>車両伝票戻し検索画面</returns>
        /// <history>
        /// 2020/08/03 yano #4048 【車両伝票ステータス修正】伝票を戻す機能の非表示化
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);
            CarSalesHeader header = new CarSalesHeader();
            // 検索結果リストの取得
            if (criteriaInit)
            {
                //何もしない(初回表示)
                header = null;
            }
            else
            {
                switch (CommonUtils.DefaultString(form["RequestFlag"]))
                {
                    case "1": // 検索ボタン

                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048 
                        break;

                    case "2": //修正する

                        CarSlipStatusChange(form);
                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048
                        break;

                    case "3": //伝票を戻す

                        CarSlipStatus_Return(form);
                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048
                        break;

                    case "4": //表示を消す
                        CarSlipStatus_DispFlag(form, "2");
                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048
                        break;

                    case "5": //進行中に戻す
                        CarSlipStatus_DispFlag(form, "1");
                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048
                        break;

                    default:
                        header = Search(form["SearchSlipNumber"], form["DeliveredSlipStatusChange"]); //Mod 2020/08/03 yano #4048
                        break;
                }
            }
            // 検索項目の設定
            SetComponent(form, header);
            return View("CarSlipStatusChangeCriteria");
        }


        #region 伝票検索処理
        /// <summary>
        /// 伝票検索処理
        /// </summary>
        /// <param name="SearchSlipNumber">検索条件の伝票番号</param>
        /// <returns></returns>
        /// <history>
        /// 2020/08/27 yano #4058 【車両伝票ステータス修正】ログインユーザの権限処理の不具合対応
        /// 2020/08/03 yano #4048 【車両伝票ステータス修正】伝票を戻す機能の非表示化
        /// 2018/09/07 yano #3940 車両伝票ステータス修正　次長権限ユーザで全ての伝票が検索できない
        /// 2018/08/22 yano #3931 車両伝票ステータス修正　兼務部門の対応
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について
        /// 2018/06/18 arc yano #3897 車両伝票ステータス修正　権限による機能の制限
        /// </history>
        private CarSalesHeader Search(string SearchSlipNumber, string AdminFlag) 
        {
            if (!string.IsNullOrWhiteSpace(SearchSlipNumber))
            {
                //Add 2018/06/18 arc yano #3897
                SecurityRole securityRole = ((Employee)Session["Employee"]).SecurityRole;

                CarSalesHeader ret = new CarSalesOrderDao(db).GetBySlipNumber(SearchSlipNumber);

                //納車済伝票ステータス戻しの権限のないユーザの場合
                //Mod 2020/08/03 yano #4048
                if (!bool.Parse(AdminFlag))
                //if (!new ApplicationRoleDao(db).GetByKey(securityRole.SecurityRoleCode, "DeliveredSlipStatusChange").EnableFlag)
                {
                    //Add 2018/08/22 yano #3931
                    //セッションから兼務部門を取得する
                    List<string> DepList = new List<string>();

                    //Mod 2018/09/07 yano #3940 ユーザの閲覧範囲をチェック
                    //参照範囲が「部門内」または「事業部内」の場合
                    if (!string.IsNullOrWhiteSpace(securityRole.SecurityLevelCode) && (securityRole.SecurityLevelCode.Equals("001") || securityRole.SecurityLevelCode.Equals("002")))
                    {
                        //部門
                        DepList.Add(((Employee)Session["Employee"]).DepartmentCode);

                        //兼務部門１
                        if (!string.IsNullOrWhiteSpace(((Employee)Session["Employee"]).DepartmentCode1))
                        {
                            DepList.Add(((Employee)Session["Employee"]).DepartmentCode1);
                        }
                        //兼務部門２
                        if (!string.IsNullOrWhiteSpace(((Employee)Session["Employee"]).DepartmentCode2))
                        {
                            DepList.Add(((Employee)Session["Employee"]).DepartmentCode2);
                        }
                        //兼務部門３
                        if (!string.IsNullOrWhiteSpace(((Employee)Session["Employee"]).DepartmentCode3))
                        {
                            DepList.Add(((Employee)Session["Employee"]).DepartmentCode3);
                        }

                        //Add 2020/08/27 yano #4058
                        //閲覧範囲が「事業所」の場合は、同一事業所の部門も閲覧可能とする
                        if (securityRole.SecurityLevelCode.Equals("002"))
                        {
                            string officeCode = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode).OfficeCode;

                            DepList.AddRange(new DepartmentDao(db).GetListAll().Where(x => x.OfficeCode.Equals(officeCode)).Select(x => x.DepartmentCode).ToList());
                        }
                        
                    }

                    //Mod 2018/08/07 yano #3911
                    //伝票が存在し、伝票ステータスが「納車済」の場合はnullにする
                    if (ret != null)
                    {
                        //伝票ステータスが納車済の場合
                        if (ret.SalesOrderStatus.Equals("005"))
                        {
                            ModelState.AddModelError("", "権限が無いため納車済伝票のステータス戻しを行うことができません。システム課に依頼して下さい");
                            ret = null;
                        }
                        else if (DepList.Count > 0 && DepList.Where(x => x.Equals(ret.DepartmentCode)).Count() == 0) //Mod 2018/08/22 yano #3931 //部門、兼務部門以外の伝票の場合
                        {
                            ModelState.AddModelError("", "権限が無いため自部門(兼務部門含む)以外の伝票のステータス戻しを行うことができません。システム課に依頼して下さい");
                            ret = null;
                        }
                    }
                }

                return ret;

                //return new CarSalesOrderDao(db).GetBySlipNumber(SearchSlipNumber);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 画面コンポーネント設定
        /// <summary>
        /// 画面コンポーネント設定
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="header">車両伝票データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/08/03 yano #4048 【車両伝票ステータス修正】伝票を戻す機能の非表示化
        /// </history>
        private void SetComponent(FormCollection form, CarSalesHeader header)
        {
            ViewData["Message"] = "";

            if (header != null)
            {
                //納車日が入っていた場合
                if (header.SalesDate != null)
                {
                    ViewData["SalesDate"] = string.Format("{0:yyyy/MM/dd}", header.SalesDate);

                    //一度も締め処理されていない部門です。
                    bool Ret = new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, header.SalesDate, "001");
                    //if (Ret == null)
                    //{
                    //    ViewData["Message"] = "一度も締め処理されていない部門です";
                    //}
                    //伝票が締め処理されている為修正できません
                    if (!Ret)
                    {
                        ViewData["ErrFlag"] = "1";
                        ViewData["Message"] = "伝票が締め処理されている為修正できません";
                    }
                }
                //納車日が入っていなかった場合
                else
                {
                    ViewData["SalesDate"] = "";
                }
                ViewData["SlipNumber"] = header.SlipNumber;
                ViewData["CustomerName"] = header.Customer.CustomerName;
                ViewData["SalesOrderStatusName"] = header.c_SalesOrderStatus.Name;
                ViewData["DepartmentName"] = header.Department.DepartmentName;
                ViewData["Employeename"] = header.Employee.EmployeeName;

                ViewData["SalesOrderStatus"] = header.SalesOrderStatus;
                ViewData["DepartmentCode"] = header.DepartmentCode;

                //修正の必要はありません(見積なら修正不要)
                if (!criteriaInit)
                {
                    if (header.SalesOrderStatus == "001")
                    {
                        ViewData["ErrFlag"] = "1";
                        ViewData["Message"] = "修正の必要はありません";
                    }
                }

                //キャンセル・受注後キャンセルされている場合は修正不可
                if (header.SalesOrderStatus.Equals("006") || header.SalesOrderStatus.Equals("007"))
                {
                    ViewData["ErrFlag"] = "1";
                    ViewData["Message"] = "伝票がキャンセルされているため修正できません";
                }
            }
            else
            {
                ViewData["SalesDate"] = "";
                ViewData["SlipNumber"] = "";
                ViewData["CustomerName"] = "";
                ViewData["SalesOrderStatusName"] = "";
                ViewData["DepartmentName"] = "";
                ViewData["Employeename"] = "";

                ViewData["SalesOrderStatus"] = "";
                ViewData["DepartmentCode"] = "";
                ViewData["ErrFlag"] = "1";

                if (criteriaInit)
                {
                    //初回表示は何も表示しない
                    ViewData["Message"] = "";
                }
                else
                {
                    
                    ViewData["Message"] = "伝票がありません";
                }
            }

            ViewData["SearchSlipNumber"] = form["SearchSlipNumber"];
            ViewData["TargetSlipNumber"] = ViewData["SlipNumber"];
            ViewData["indexid"] = form["indexid"];
            if (!string.IsNullOrEmpty(form["RequestFlag"]) && form["RequestFlag"].Equals("2"))
            {
                ViewData["RequestUserName"] = ""; //クリア
            }
            else
            {
                ViewData["RequestUserName"] = form["RequestUserName"];
            }
            
            ViewData["StatusChangeCode"] = form["StatusChangeCode"];

            //すべてが初期表示
            switch (form["indexid"] ?? "0")
            {
                case "0":   //進行中
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    break;
                case "1":   //履歴
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = true;
                    break;
                default:    //進行中
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    break;
            }

            ViewData["DeliveredSlipStatusChange"] = form["DeliveredSlipStatusChange"]; //add 2020/08/03 yano #4048
        }
        #endregion

        #region 進行中のデータ表示
        /// <summary>
        /// 進行中のデータ表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="header">車両伝票データ</param>
        /// <returns></returns>
        /// <history>
        /// 2020/08/03 yano #4048 車両伝票ステータス修正】伝票を戻す機能の非表示化
        /// 2018/06/18 arc yano #3897 車両伝票ステータス修正　権限による機能の制限
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult Ret0_List(string ChangeStatus)
        {
            //Add 2018/06/18 arc yano #3897
            string securityRoloCode = ((Employee)Session["Employee"]).SecurityRoleCode;

            bool enableFlag = new ApplicationRoleDao(db).GetByKey(securityRoloCode, "DeliveredSlipStatusChange").EnableFlag;

            //リスト取得
            List<Get_CarSlipStatusChangeResult> list = new CarSalesOrderDao(db).Get_CarSlipStatusChange(ChangeStatus, enableFlag, ((Employee)Session["Employee"]).EmployeeCode);

            ViewData["flgDeliveredSlipStatusChange"] = enableFlag; //Add 2020/08/03 yano #4048

            return PartialView("_Ret0_List", list);
        }
        #endregion

        #region 修正履歴のデータ表示
        /// <summary>
        /// 修正履歴のデータ表示
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="header">車両伝票データ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/18 arc yano #3897 車両伝票ステータス修正　権限による機能の制限
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult Ret1_List(string ChangeStatus)
        {
            //Add 2018/06/18 arc yano #3897
            string securityRoloCode = ((Employee)Session["Employee"]).SecurityRoleCode;

            bool enableFlag = new ApplicationRoleDao(db).GetByKey(securityRoloCode, "DeliveredSlipStatusChange").EnableFlag;

            List<Get_CarSlipStatusChangeResult> list = new CarSalesOrderDao(db).Get_CarSlipStatusChange(ChangeStatus, enableFlag, ((Employee)Session["Employee"]).EmployeeCode);

            return PartialView("_Ret1_List", list);
        }
        #endregion


        #region 修正するボタン押下
        /// <summary>
        /// 修正するボタン押下
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について
        /// </history>
        private void CarSlipStatusChange(FormCollection form)
        {
            var Ret = db.CarSlipStatusChange(form["TargetSlipNumber"], ((Employee)Session["Employee"]).EmployeeCode, form["SalesOrderStatus"], form["RequestUserName"]);

            //Del 2018/08/23 yano ステータス戻しで引当解除は行わない
            //Add 2018/08/07 yano #3911
            //引当解除を行った場合には、通知メールを送信
            //if (Ret == 1)
            //{
            //    CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(form["TargetSlipNumber"]);

            //    if (header != null)
            //    {
            //        CommonUtils.SendCancelReservationMail(db ,header, PTN_CANCEL_RESERVATION, CANCEL_FROM_CARSLIPSTATUSCHANGE);                
            //    }

            //}
        }
        #endregion

        #region 伝票を戻すボタン押下
        /// <summary>
        /// 伝票を戻すボタン押下
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        private void CarSlipStatus_Return(FormCollection form)
        {
            CarSalesHeader SlipData = new CarSalesOrderDao(db).GetBySlipNumber(form["TargetSlipNumber"]);

            if (SlipData != null)
            {
                if (SlipData.SalesOrderStatus.Equals("007"))
                {
                    ModelState.AddModelError("", "伝票番号：" + form["TargetSlipNumber"] + " は現在 " + SlipData.c_SalesOrderStatus.Name + " になっているため、元のステータスに戻すことができません。");
                    return;
                }
            }

            //引当されているかのチェック
            /*if (SlipData != null)
            {
                if (!string.IsNullOrEmpty(SlipData.SalesCarNumber))
                {
                    string SalesCarNumber = SlipData.SalesCarNumber;
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(SalesCarNumber);
                    if (SalesCarData != null)
                    {
                        if (SalesCarData.CarStatus.Equals("001") || SalesCarData.CarStatus.Equals("002"))
                        {
                            ModelState.AddModelError("", "伝票番号：" + form["TargetSlipNumber"] + "に対して車両(" + SlipData.SalesCarNumber + ")が引当されていません。");
                            return;
                        }
                    }
                }
            }*/

            var Ret = db.CarSlipStatus_Return(form["TargetSlipNumber"], ((Employee)Session["Employee"]).EmployeeCode, form["SalesOrderStatus"], form["StatusChangeCode"]);
        }
        #endregion

        #region 表示を消すボタン押下
        /// <summary>
        /// 表示を消すボタン押下
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns></returns>
        private void CarSlipStatus_DispFlag(FormCollection form, string ChangeStatus)
        {
            //進行中に戻す場合にチェックする
            if (ChangeStatus.Equals("1"))
            {
                //対象伝票の現在のステータスを取得
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(form["TargetSlipNumber"]);
                if (header != null)
                {
                    //登録済または、納車前または、納車済または、キャンセルまたは、受注後キャンセルになっていた場合、進行中に戻せない
                    if (header.SalesOrderStatus.Equals("003") || header.SalesOrderStatus.Equals("004") || header.SalesOrderStatus.Equals("005") || header.SalesOrderStatus.Equals("006") || header.SalesOrderStatus.Equals("007"))
                    {
                        ModelState.AddModelError("", "伝票番号：" + form["TargetSlipNumber"] + " は現在 " + header.c_SalesOrderStatus.Name +" になっているため、進行中に戻すことができません。");
                        return;
                    }
                }
            }

            var Ret = db.CarSlipStatus_DispFlag(form["TargetSlipNumber"], ((Employee)Session["Employee"]).EmployeeCode, ChangeStatus, form["StatusChangeCode"]);
        }
        #endregion

    }
}
