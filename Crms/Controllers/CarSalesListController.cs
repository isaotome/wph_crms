using Crms.Models;
using CrmsDao;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Crms.Controllers
{
  //Create 2017/03/09 arc nakayama #3723_納車リスト
  /// <summary>
  /// 車両納車リスト検索コントロールクラス
  /// </summary>
  [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarSalesListController : Controller
    {
        #region 定数
        private static readonly string FORM_NAME = "車両納車リスト";            // 画面名（ログ出力用）
        private static readonly string PROC_NAME_SEARCH = "車両納車リスト検索"; // 処理名（ログ出力用）
        private static readonly string PROC_NAME_EXCEL = "納車リスト(明細)EXCEL出力"; //処理名
        #endregion


        #region 初期化
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CarSalesListController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion


        #region 納車リスト検索画面表示
        /// <summary>
        /// 納車リスト検索画面表示
        /// </summary>
        /// <returns>納車リスト検索画面</returns>
        /// <history>
        /// 2020/01/16 yano #4027【納車リスト】明細画面表示の不具合対応
        /// </history>
        [AuthFilter]
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();
            DateTime Now = DateTime.Today;
            int Month = Now.Month;


            //Mod 2020/01/16 yano #4027
            //7月以降であれば年度はプラス１する
            if (Month >= 7)
            {
                form["TargetYear"] = Now.AddYears(1).Year.ToString().Substring(1, 3);
            }
            else
            {
                form["TargetYear"] = DateTime.Today.Year.ToString().Substring(1, 3); //初期値
            }

            ////7月以前なら年度は1年前
            //if (Month <= 7)
            //{
            //    form["TargetYear"] = Now.AddYears(-1).Year.ToString().Substring(1,3);
            //}
            //else
            //{
            //    form["TargetYear"] = DateTime.Today.Year.ToString().Substring(1, 3); //初期値
            //}

            

            return Criteria(form);
        }


        /// <summary>
        /// 納車リスト検索画面表示
        /// </summary>
        /// <param name="form">フォームデータ(検索条件)</param>
        /// <returns>納車リスト検索画面</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            // 検索項目の設定
            SetComponent(form);
            return View("CarSalesListCriteria");
        }

        /// <summary>
        /// 納車リスト　すべて(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult Ret0_List(string TargetYearCode)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            //List<GetCarSalesList_Result> list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "0");

            CarSalseList list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "0");

            ViewData["TargetYear"] = TargetYearCode;

            return PartialView("_Ret0_List", list);
        }

        /// <summary>
        /// 納車リスト　一般(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 2)]
        public ActionResult Ret1_List(string TargetYearCode)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            //List<GetCarSalesList_Result> list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "1");
            CarSalseList list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "1");


            return PartialView("_Ret1_List", list);
        }

        /// <summary>
        /// 納車リスト　ＡＡ・業販(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 3)]
        public ActionResult Ret2_List(string TargetYearCode)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            //List<GetCarSalesList_Result> list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "2");

            CarSalseList list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "2");

            return PartialView("_Ret2_List", list);
        }

        /// <summary>
        /// 納車リスト　デモ・自登(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 4)]
        public ActionResult Ret3_List(string TargetYearCode)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            //List<GetCarSalesList_Result> list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "3");

            CarSalseList list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "3");

            return PartialView("_Ret3_List", list);
        }

        /// <summary>
        /// 納車リスト　依廃・他(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// </history>
        [ChildActionOnly]
        [OutputCache(Duration = 5)]
        public ActionResult Ret4_List(string TargetYearCode)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            //List<GetCarSalesList_Result> list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "4");

            CarSalseList list = new CarSalesOrderDao(db).GetCarSalesListResult(TargetYear, "4");

            return PartialView("_Ret4_List", list);
        }

        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            ViewData["TargetYear"] = form["TargetYear"];
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetYear"], false);
            ViewData["indexid"] = form["indexid"];

            //すべてが初期表示
            switch (form["indexid"] ?? "0")
            {
                case "0":   //すべて
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "1":   //一般
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = true;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "2":   //ＡＡ・業販
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = true;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "3":   //デモ・自登
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = true;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "4":   //依廃・他
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = true;
                    break;
                default:    //すべて
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
            }

        }
        #endregion

        #region 納車リスト（明細）
        /// <summary>
        /// 納車リスト（明細）ダイアログ
        /// </summary>
        /// <returns>納車リスト（明細）</returns>
        /// <history>
        /// 2020/01/16 #4027【納車リスト】明細画面表示の不具合対応
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// 2017/04/14 arc nakayama #3751_納車リスト(明細)画面＿検索時の初期値誤り
        /// </history>
        public ActionResult CarSalesDetailCriteriaDialog()
        {
            FormCollection form = new FormCollection();

            if (Request["SelectYearCode"] != null)
            {
                form["SelectYearCode"] = Request["SelectYearCode"];
            }
            if (Request["SelectMonthCode"] != null)
            {
                form["SelectMonthCode"] = Request["SelectMonthCode"];
            }
            if (Request["DepartmentCode"] != null)
            {
                form["DepartmentCode"] = Request["DepartmentCode"];
            }
            if (Request["DepartmentName"] != null)
            {
                form["DepartmentName"] = Request["DepartmentName"];
            }
            if (Request["RequestFlag"] != null)
            {
                form["RequestFlag"] = Request["RequestFlag"];
            }

            //Mod 2020/01/16 #4027
            //販売区分
            if (Request["AAType"] != null)
            {
                form["AAType"] = Request["AAType"];
            }

            //form["AAType"] = "002";         //Add 2017/10/14 arc yano #3790

            //Add 2017/04/14 arc nakayama #3751
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, form["SelectYearCode"].ToString()).Name;

            //Mod 2017/10/14 arc yano #3790 月が指定されている時のみ設定する
            if (!string.IsNullOrWhiteSpace(form["SelectMonthCode"]))
            {
                string TargetMonth = dao.GetMonth(true, form["SelectMonthCode"].ToString()).Name;
                int iTargetMonth = int.Parse(TargetMonth);

                //Mod 2020/01/16 #4027
                //7月以降なら前の年
                if (iTargetMonth >= 7)
                {
                    int iTargetYear = int.Parse(TargetYear);
                    iTargetYear--;
                    TargetYear = iTargetYear.ToString();
                    form["SelectYearCode"] = dao.GetYearByName(true, TargetYear.ToString()).Code;
                }

                ////1月～6月なら次の年
                //if (iTargetMonth < 7)
                //{
                //    int iTargetYear = int.Parse(TargetYear);
                //    iTargetYear++;
                //    TargetYear = iTargetYear.ToString();
                //    form["SelectYearCode"] = dao.GetYearByName(true, TargetYear.ToString()).Code;
                //}
            }

            return CarSalesDetailCriteriaDialog(form);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CarSalesDetailCriteriaDialog(FormCollection form)
        {
            PaginatedList<GetCarSalesHeaderListResult> list = new PaginatedList<GetCarSalesHeaderListResult>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                case "1": // 検索ボタン

                    list = new PaginatedList<GetCarSalesHeaderListResult>(CarSalesDetailSearch(form).AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                    SetDataCriteriaComponent(form);
                    return View("CarSalesDetailCriteriaDialog", list);

                case "2": // Excelボタン
                    return ExcelDownload(form);

                default:
                    // 検索項目の設定
                    SetDataCriteriaComponent(form);
                    // 検索画面の表示
                    return View("CarSalesDetailCriteriaDialog", list);
            }

        }

        #region 検索処理
        public List<GetCarSalesHeaderListResult> CarSalesDetailSearch(FormCollection form)
        {
            List<GetCarSalesHeaderListResult> Ret = new List<GetCarSalesHeaderListResult>();

            CarSalesHeaderListSearchCondition condition = Setcondition(form);

            Ret = new CarSalesOrderDao(db).GetCarSalesHeaderListByCondition(condition);
            return Ret;
        }
        #endregion

        #region 納車リスト（明細）コンポーネント設定
        /// <summary>
        /// 納車リスト（明細）コンポーネント設定
        /// </summary>
        /// <returns>納車リスト（明細）</returns>
        /// <history>
        /// 2020/01/16 yano #4027【納車リスト】明細画面表示の不具合対応
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// 2017/04/14 arc nakayama #3751_納車リスト(明細)画面＿検索時の初期値誤り
        /// </history>
        public void SetDataCriteriaComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department depData = new DepartmentDao(db).GetByKey(form["DepartmentCode"], false);
                if (depData != null)
                {
                    ViewData["DepartmentName"] = depData.DepartmentName;
                }
            }
            else
            {
                ViewData["DepartmentName"] = "";
            }
            ViewData["RequestFlag"] = form["RequestFlag"];
            
            ViewData["SelectYearCode"] = form["SelectYearCode"];
            ViewData["SelectMonthCode"] = form["SelectMonthCode"];
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["SelectYearCode"], false);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["SelectMonthCode"], true);    //Mod 2017/10/14 arc yano #3790

            //新中区分
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);      //Add 2017/10/14 arc yano #3790

            //Mod 2020/01/16 yano #4027
            ViewData["AATypeList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("027", false), form["AAType"], false);
            ////AA含む
            //ViewData["AATypeList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("024", false), form["AAType"], false);      //Add 2017/10/14 arc yano #3790

        }
        #endregion

        #endregion

        #region 検索条件セット
        /// <summary>
        /// 検索条件セット
        /// </summary>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加 月が選択されていない場合の考慮
        /// </history>
        public CarSalesHeaderListSearchCondition Setcondition(FormCollection form)
        {
            CarSalesHeaderListSearchCondition condition = new CarSalesHeaderListSearchCondition();

            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, form["SelectYearCode"].ToString()).Name;

            //Mod 2017/10/14 arc yano #379
            c_Month month = dao.GetMonth(true, form["SelectMonthCode"].ToString());
            string TargetMonth = (month != null ? month.Name : "");

            condition.SelectYear = TargetYear;
            condition.SelectMonth = TargetMonth;
            condition.DepartmentCode = form["DepartmentCode"];

            condition.NewUsedType = form["NewUsedType"];        //Add 2017/10/14 arc yano #3790
            condition.AAType = form["AAType"];                  //Add 2017/10/14 arc yano #3790

            return condition;
        }
        #endregion

        #region 納車リスト（担当者別）
        /// <summary>
        /// 納車リスト（明細）ダイアログ
        /// </summary>
        /// <returns>納車リスト（明細）</returns>
        /// <history>
        /// 2020/01/16 yano #4027【納車リスト】明細画面表示の不具合対応
        /// </history>
        public ActionResult CarSalesDetailEmployeeCriteriaDialog()
        {
            FormCollection form = new FormCollection();

            if (Request["SelectYearCode"] != null)
            {
                form["SelectYearCode"] = Request["SelectYearCode"];
            }
            if (Request["DepartmentCode"] != null)
            {
                form["DepartmentCode"] = Request["DepartmentCode"];
            }
            if (Request["DepartmentName"] != null)
            {
                form["DepartmentName"] = Request["DepartmentName"];
            }
            if (Request["Jul_Cnt"] != null)
            {
                form["Jul_Cnt"] = Request["Jul_Cnt"];
            }
            if (Request["Aug_Cnt"] != null)
            {
                form["Aug_Cnt"] = Request["Aug_Cnt"];
            }
            if (Request["Sep_Cnt"] != null)
            {
                form["Sep_Cnt"] = Request["Sep_Cnt"];
            }
            if (Request["Oct_Cnt"] != null)
            {
                form["Oct_Cnt"] = Request["Oct_Cnt"];
            }
            if (Request["Nov_Cnt"] != null)
            {
                form["Nov_Cnt"] = Request["Nov_Cnt"];
            }
            if (Request["Dec_Cnt"] != null)
            {
                form["Dec_Cnt"] = Request["Dec_Cnt"];
            }
            if (Request["Jan_Cnt"] != null)
            {
                form["Jan_Cnt"] = Request["Jan_Cnt"];
            }
            if (Request["Feb_Cnt"] != null)
            {
                form["Feb_Cnt"] = Request["Feb_Cnt"];
            }
            if (Request["Mar_Cnt"] != null)
            {
                form["Mar_Cnt"] = Request["Mar_Cnt"];
            }
            if (Request["Apr_Cnt"] != null)
            {
                form["Apr_Cnt"] = Request["Apr_Cnt"];
            }
            if (Request["May_Cnt"] != null)
            {
                form["May_Cnt"] = Request["May_Cnt"];
            }
            if (Request["Jun_Cnt"] != null)
            {
                form["Jun_Cnt"] = Request["Jun_Cnt"];
            }
            //Add 2020/01/16 yano #4027
            if (Request["indexid"] != null)
            {
                form["indexid"] = Request["indexid"];
            }

            return CarSalesDetailEmployeeCriteriaDialog(form);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CarSalesDetailEmployeeCriteriaDialog(FormCollection form)
        {
            List<GetCarSalesEmployeeList_Result> list = new List<GetCarSalesEmployeeList_Result>();

            // 検索ボタン
            SetDataEmployeeCriteriaComponent(form);
            return View("CarSalesDetailEmployeeCriteriaDialog", list);


        }

        #region 納車リスト（担当者別）コンポーネント設定
        public void SetDataEmployeeCriteriaComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);
            ViewData["SelectYearCode"] = form["SelectYearCode"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["Jul_Cnt"] = form["Jul_Cnt"];
            ViewData["Aug_Cnt"] = form["Aug_Cnt"];
            ViewData["Sep_Cnt"] = form["Sep_Cnt"];
            ViewData["Oct_Cnt"] = form["Oct_Cnt"];
            ViewData["Nov_Cnt"] = form["Nov_Cnt"];
            ViewData["Dec_Cnt"] = form["Dec_Cnt"];
            ViewData["Jan_Cnt"] = form["Jan_Cnt"];
            ViewData["Feb_Cnt"] = form["Feb_Cnt"];
            ViewData["Mar_Cnt"] = form["Mar_Cnt"];
            ViewData["Apr_Cnt"] = form["Apr_Cnt"];
            ViewData["May_Cnt"] = form["May_Cnt"];
            ViewData["Jun_Cnt"] = form["Jun_Cnt"];
            ViewData["indexid"] = form["indexid"];

            //すべてが初期表示
            switch (form["indexid"] ?? "0")
            {
                case "0":   //すべて
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "1":   //一般
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = true;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "2":   //ＡＡ・業販
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = true;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "3":   //デモ・自登
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = true;
                    ViewData["4_ListDisplay"] = false;
                    break;
                case "4":   //依廃・他
                    ViewData["0_ListDisplay"] = false;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = true;
                    break;
                default:    //その他(入金予定)
                    ViewData["0_ListDisplay"] = true;
                    ViewData["1_ListDisplay"] = false;
                    ViewData["2_ListDisplay"] = false;
                    ViewData["3_ListDisplay"] = false;
                    ViewData["4_ListDisplay"] = false;
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 納車リスト　すべて(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 1)]
        public ActionResult Ret0_EmployeeList(string TargetYearCode, string DepartmentCode, string DepartmentName)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            TotalCarSalesEmployeeList_Result list = new CarSalesOrderDao(db).GetCarSalesEmployeeList(TargetYear, DepartmentCode, "0");

            ViewData["DepartmentCode"] = DepartmentCode;
            ViewData["DepartmentName"] = DepartmentName;

            return PartialView("_Ret0_EmployeeList", list);
        }

        /// <summary>
        /// 納車リスト　一般(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 2)]
        public ActionResult Ret1_EmployeeList(string TargetYearCode, string DepartmentCode, string DepartmentName)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            TotalCarSalesEmployeeList_Result list = new CarSalesOrderDao(db).GetCarSalesEmployeeList(TargetYear, DepartmentCode, "1");

            ViewData["DepartmentCode"] = DepartmentCode;
            ViewData["DepartmentName"] = DepartmentName;

            return PartialView("_Ret1_EmployeeList", list);
        }

        /// <summary>
        /// 納車リスト　ＡＡ・業販(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 3)]
        public ActionResult Ret2_EmployeeList(string TargetYearCode, string DepartmentCode, string DepartmentName)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            TotalCarSalesEmployeeList_Result list = new CarSalesOrderDao(db).GetCarSalesEmployeeList(TargetYear, DepartmentCode, "2");

            ViewData["DepartmentCode"] = DepartmentCode;
            ViewData["DepartmentName"] = DepartmentName;

            return PartialView("_Ret2_EmployeeList", list);
        }

        /// <summary>
        /// 納車リスト　デモ・自登(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 4)]
        public ActionResult Ret3_EmployeeList(string TargetYearCode, string DepartmentCode, string DepartmentName)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            TotalCarSalesEmployeeList_Result list = new CarSalesOrderDao(db).GetCarSalesEmployeeList(TargetYear, DepartmentCode, "3");

            ViewData["DepartmentCode"] = DepartmentCode;
            ViewData["DepartmentName"] = DepartmentName;

            return PartialView("_Ret3_EmployeeList", list);
        }

        /// <summary>
        /// 納車リスト　依廃・他(子アクションのみ対応)
        /// </summary>
        /// <param name="TargetYear">指定年度</param>
        /// <returns>納車リスト一覧</returns>
        [ChildActionOnly]
        [OutputCache(Duration = 5)]
        public ActionResult Ret4_EmployeeList(string TargetYearCode, string DepartmentCode, string DepartmentName)
        {
            CodeDao dao = new CodeDao(db);
            string TargetYear = dao.GetYear(true, TargetYearCode).Name;
            TotalCarSalesEmployeeList_Result list = new CarSalesOrderDao(db).GetCarSalesEmployeeList(TargetYear, DepartmentCode, "4");

            ViewData["DepartmentCode"] = DepartmentCode;
            ViewData["DepartmentName"] = DepartmentName;

            return PartialView("_Ret4_EmployeeList", list);
        }

        #endregion

        #region Excelボタン押下
        /// <summary>
        /// Excelボタン押下
        /// </summary>
        /// <returns>出力したExcel</returns>
        /// <history>
        /// 2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）
        /// 2017/10/14 arc yano #3790 納車リスト
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExcelDownload(FormCollection form)
        {
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCEL);

            PaginatedList<GetCarSalesHeaderListResult> list = new PaginatedList<GetCarSalesHeaderListResult>();

            //-------------------------------
            //Excel出力処理
            //-------------------------------
            //Mod 2023/01/16 yano #4138
            CodeDao dao = new CodeDao(db);
            //対象年
            string targetYear = dao.GetYear(true, form["SelectYearCode"]).Name + "年";
            //対象月
            string targetMonth = !string.IsNullOrWhiteSpace(form["SelectMonthCode"]) ? dao.GetMonth(true, form["SelectMonthCode"]).Name + "月" : "";

            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //ファイル名:納車リスト_yyyy年_xx月.xlsx
            string fileName = DownLoadTime + "_納車リスト_" + targetYear + targetMonth + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //テンプレートファイル取得
            string tfilePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CarSalesHeaderList"]) ? "" : ConfigurationManager.AppSettings["CarSalesHeaderList"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePath.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataCriteriaComponent(form);
                return View("CarSalesDetailCriteriaDialog", list);
            }

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, filePathName, tfilePath);

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        #region エクセルデータ作成(テンプレートファイルあり)
        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
        {

            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                  //設定値
            byte[] excelData = null;                //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // 設定シート取得
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //設定データを取得(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //configシートが無い場合はエラー
            {
                ModelState.AddModelError("", "テンプレートファイルのconfigシートがみつかりません");

                excelData = excelFile.GetAsByteArray();

                //ファイル削除
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //----------------------------
            // 検索条件出力
            //----------------------------
            configLine.SetPos[0] = "A1";

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(form);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            configLine = dExport.GetConfigLine(config, 2);

            //検索結果取得
            CarSalesHeaderListSearchCondition condition = Setcondition(form);

            List<GetCarSalesHeaderListResult> list = new CarSalesOrderDao(db).GetCarSalesHeaderListByCondition(condition);

            List<GetCarSalesHeaderList_ExcelResult> elist = new List<GetCarSalesHeaderList_ExcelResult>();

            elist = MakeExcelList(list, form);

            //データ設定
            ret = dExport.SetData<GetCarSalesHeaderList_ExcelResult, GetCarSalesHeaderList_ExcelResult>(ref excelFile, elist, configLine);

            excelData = excelFile.GetAsByteArray();

            //ワークファイル削除
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }
        #endregion

        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns name = "dt" >検索条件</returns>
        /// <history>
        /// 2020/01/16 yano #4027【納車リスト】明細画面表示の不具合対応
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加 新中区分、AAの検索条件文字列の追加
        /// </history>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            String conditionText = "";

            CodeDao dao = new CodeDao(db);

            //---------------------
            //　列定義
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();

            //対象年月
            if (!string.IsNullOrWhiteSpace(form["SelectYearCode"]) && !string.IsNullOrWhiteSpace(form["SelectMonthCode"]))
            {
                conditionText += "対象年月=" + dao.GetYear(true, form["SelectYearCode"]).Name + "/" + dao.GetMonth(true, form["SelectMonthCode"]).Name;
            }

            //部門
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "　部門=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }

            //新中区分
            if (!string.IsNullOrWhiteSpace(form["NewUsedType"]))
            {
                c_NewUsedType nutype = new CodeDao(db).GetNewUsedTypeByKey(form["NewUsedType"]);

                conditionText += "　新中区分=" + nutype.Name;
            }


            //Mod 2020/01/16 yano #4027
            //販売区分
            if (!string.IsNullOrWhiteSpace(form["AATYPE"]))
            {
                c_CodeName aatype = new CodeDao(db).GetCodeNameByKey("027", form["AATYPE"], false);

                conditionText += "　区分=" + aatype.Name;
            }

            ////新中区分
            //if (!string.IsNullOrWhiteSpace(form["AATYPE"]))
            //{
            //    c_CodeName aatype = new CodeDao(db).GetCodeNameByKey("024", form["AATYPE"], false);

            //    conditionText += "　AA分=" + aatype.Name;
            //}


            //作成したテキストをカラムに設定
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region 検索結果をExcel用に成形
        /// <summary>
        /// 検索結果をExcel用に成形
        /// </summary>
        /// <param name="list">検索結果</param>
        /// <returns name="elist">検索結果(Exel出力用)</returns>
        /// <history>
        /// 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
        /// 2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加 列（AA諸費用、下取車台番号(1)-(3)）追加
        /// </history>
        private List<GetCarSalesHeaderList_ExcelResult> MakeExcelList(List<GetCarSalesHeaderListResult> list, FormCollection form)
        {
            List<GetCarSalesHeaderList_ExcelResult> elist = new List<GetCarSalesHeaderList_ExcelResult>();

            //対象年月の指定がある場合は実棚を数量に

            elist = list.Select(x =>
                    new GetCarSalesHeaderList_ExcelResult()
                    {
                        //納車日
                        SalesDate = x.SalesDate,
                        //新中区分
                        NewUsedTypeName = x.NewUsedTypeName,
                        //伝票番号
                        SlipNumber = x.SlipNumber,
                        //管理番号
                        SalesCarNumber = x.SalesCarNumber,
                        //車台番号
                        Vin = x.Vin,
                        //顧客名
                        CustomerName = x.CustomerName,
                        //部門コード
                        DepartmentCode = x.DepartmentCode,
                        //部門名
                        DepartmentName = x.DepartmentName,
                        //担当者名
                        Employeename = x.Employeename,
                        //車両本体価格
                        SalesPrice = x.SalesPrice,
                        //販売店オプション
                        ShopOptionAmountWithTax = x.ShopOptionAmountWithTax,
                        //メンテナンスパッケージ
                        MaintenancePackageAmount = x.MaintenancePackageAmount,
                        //延長保証
                        ExtendedWarrantyAmount = x.ExtendedWarrantyAmount,
                        //特別サーチャージ
                        SurchargeAmount = x.SurchargeAmount,                               //Add 2023/09/18 yano #4181
                        //メーカーオプション
                        MakerOptionAmount = x.MakerOptionAmount,
                        //AA諸費用
                        AAAmount = x.AAAmount,                                              //Add 2017/10/14 arc yano #3790
                        //諸費用課税
                        SalesCostTotalAmount = x.SalesCostTotalAmount,
                        //値引
                        DiscountAmount = x.DiscountAmount,
                        //非課税諸費用
                        OtherCostTotalAmount = x.OtherCostTotalAmount,
                        //税金
                        TaxFreeTotalAmount = x.TaxFreeTotalAmount,
                        //自賠責
                        CarLiabilityInsurance = x.CarLiabilityInsurance,
                        //リサイクル
                        RecycleDeposit = x.RecycleDeposit,
                        //販売合計
                        GrandTotalAmount = x.GrandTotalAmount,
                        //下取本体
                        TradeInTotalAmountNotTax = x.TradeInTotalAmountNotTax,
                        //下取車台番号1
                        TradeInVin1 = x.TradeInVin1,                                        //Add 2017/10/14 arc yano #3790
                        //下取車台番号2
                        TradeInVin2 = x.TradeInVin2,                                        //Add 2017/10/14 arc yano #3790
                        //下取車台番号3
                        TradeInVin3 = x.TradeInVin3,                                        //Add 2017/10/14 arc yano #3790
                        //未払自動車税(種別割)
                        TradeInUnexpiredCarTaxTotalAmount = x.TradeInUnexpiredCarTaxTotalAmount,
                        //残債
                        TradeInRemainDebtTotalAmount = x.TradeInRemainDebtTotalAmount,
                        //充当合計
                        TradeInAppropriationTotalAmount = x.TradeInAppropriationTotalAmount,
                        //車種
                        CarName = x.CarName,
                        //ブランド
                        CarBrandName = x.CarBrandName
                    }
            ).ToList();

            return elist;

        }
        #endregion
    }
}
