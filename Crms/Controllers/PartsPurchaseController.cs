using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Crms.Models;                      //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
using System.Data.Linq;                 //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加

using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsPurchaseController : Controller
    {
        //Add 2014/08/04 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "部品入荷";     // 画面名
        private static readonly string PROC_NAME = "データ更新"; // 処理名
        private static readonly string PROC_CANCEL_NAME = "入荷キャンセル";      // 処理名     // Add 2017/08/02 arc yano #3783

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// サービス伝票処理サービス
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// 在庫処理サービス
        /// </summary>
        private IStockService stockService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsPurchaseController(){
            db = new CrmsLinqDataContext();
            
        }

        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;


        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 部品入荷一覧画面表示
        /// <summary>
        /// 部品入荷一覧画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            criteriaInit = true;
            form["PurchaseStatus"] = "001";
            form["PurchaseType"] = "001";
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            form["DefaultDepartmentCode"] = form["DepartmentCode"];
            form["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
            form["DefaultPurchaseStatus"] = form["PurchaseStatus"];
            form["DefaultPurchaseType"] = form["PurchasePurchaseType"];

            return Criteria(form);
        }

        /// <summary>
        /// 部品入荷一覧画面表示
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            PaginatedList<GetPartsPurchase_Result> list = new PaginatedList<GetPartsPurchase_Result>();
            // 検索結果リストの取得
            if (criteriaInit)
            {
                //何もしない(初回表示)
            }
            else
            {
                list = GetSearchResultList(form);
            }
            // 検索項目の設定
            SetDataComponent(form);

            return View("PartsPurchaseCriteria", list);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 検索処理
        /// <summary>
        /// 検索処理
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
        /// </history>
        private PaginatedList<GetPartsPurchase_Result> GetSearchResultList(FormCollection form) 
        {
            PartsPurchaseSearchCondition condition = new PartsPurchaseSearchCondition();

            condition.PurchaseNumberFrom = form["PurchaseNumberFrom"];	            //入荷伝票番号From
            condition.PurchaseNumberTo = form["PurchaseNumberTo"];	                //入荷伝票番号To
            condition.PurchaseOrderNumberFrom = form["PurchaseOrderNumberFrom"];	//発注伝票番号From
            condition.PurchaseOrderNumberTo = form["PurchaseOrderNumberTo"];	    //発注伝票番号To
            condition.PurchaseOrderDateFrom = form["PurchaseOrderDateFrom"];	    //発注日From
            condition.PurchaseOrderDateTo = form["PurchaseOrderDateTo"];	        //発注日To
            condition.SlipNumberFrom = form["SlipNumberFrom"];	                    //伝票番号From
            condition.SlipNumberTo = form["SlipNumberTo"];	                        //伝票番号To
            condition.PurchaseType = form["PurchaseType"];	                        //仕入伝票区分
            condition.OrderType = form["OrderType"];	                            //発注区分
            condition.CustomerCode = form["CustomerCode"];	                        //顧客コード
            condition.PartsNumber = form["PartsNumber"];	                        //部品番号
            condition.PurchasePlanDateFrom = form["PurchasePlanDateFrom"];	        //入荷予定日From
            condition.PurchasePlanDateTo = form["PurchasePlanDateTo"];	            //入荷予定日To
            condition.PurchaseDateFrom = form["PurchaseDateFrom"];	                //入荷日From
            condition.PurchaseDateTo = form["PurchaseDateTo"];	                    //入荷日To
            condition.DepartmentCode = form["DepartmentCode"];	                    //部門コード
            condition.EmployeeCode = form["EmployeeCode"];	                        //社員コード
            condition.SupplierCode = form["SupplierCode"];	                        //仕入先コード
            condition.WebOrderNumber = form["WebOrderNumber"];	                    //WEBオーダー番号
            condition.MakerOrderNumber = form["MakerOrderNumber"];	                //メーカーオーダー番号
            condition.InvoiceNo = form["InvoiceNo"];	                            //インボイス番号
            condition.PurchaseStatus = form["PurchaseStatus"];	                    //仕入ステータス


            condition.LinkEntryCaptureDateFrom = form["LinkEntryCaptureDateFrom"];	//取込日From           //Add 2018/03/26 arc yano #3863
            condition.LinkEntryCaptureDateTo = form["LinkEntryCaptureDateTo"];	    //取込日To             //Add 2018/03/26 arc yano #3863


            return new PartsPurchaseDao(db).GetPurchaseListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 画面コンポーネント設定
        /// <summary>
        /// 画面コンポーネント設定
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
        /// </history>
        private void SetDataComponent(FormCollection form)
        {

            CodeDao dao = new CodeDao(db);
            ViewData["DefaultDepartmentCode"] = form["DefaultDepartmentCode"];
            ViewData["DefaultDepartmentName"] = form["DefaultDepartmentName"];
            ViewData["DefaultPurchaseStatus"] = form["DefaultPurchaseStatus"];
            ViewData["DefaultPurchaseType"] = form["DefaultPurchaseType"];

            ViewData["PurchaseNumberFrom"] = form["PurchaseNumberFrom"];
            ViewData["PurchaseNumberTo"] = form["PurchaseNumberTo"];
            ViewData["PurchaseOrderNumberFrom"] = form["PurchaseOrderNumberFrom"];
            ViewData["PurchaseOrderNumberTo"] = form["PurchaseOrderNumberTo"];
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];
            ViewData["SlipNumberFrom"] = form["SlipNumberFrom"];
            ViewData["SlipNumberTo"] = form["SlipNumberTo"];
            ViewData["PurchaseType"] = form["PurchaseType"];
            ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), form["PurchaseType"], false);
            ViewData["OrderType"] = form["OrderType"];
            ViewData["OrderTypeList"] = CodeUtils.GetSelectListByModel<c_OrderType>(dao.GetOrderTypeAll(false), form["OrderType"], true);

            ViewData["CustomerCode"] = form["CustomerCode"];
            if (!string.IsNullOrEmpty(form["CustomerCode"]))
            {
                Customer customer = new CustomerDao(db).GetByKey(form["CustomerCode"]);
                ViewData["CustomerName"] = customer != null ? customer.CustomerName : "";
            }

            ViewData["PartsNumber"] = form["PartsNumber"];
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                Parts parts = new PartsDao(db).GetByKey(form["PartsNumber"]);
                ViewData["PartsNameJp"] = parts != null ? parts.PartsNameJp : "";
            }

            ViewData["PurchasePlanDateFrom"] = form["PurchasePlanDateFrom"];
            ViewData["PurchasePlanDateTo"] = form["PurchasePlanDateTo"];
            ViewData["PurchaseDateFrom"] = form["PurchaseDateFrom"];
            ViewData["PurchaseDateTo"] = form["PurchaseDateTo"];

            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }

            ViewData["EmployeeCode"] = form["EmployeeCode"];
            if (!string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                ViewData["EmployeeName"] = employee != null ? employee.EmployeeName : "";
            }

            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(form["SupplierCode"]);
                ViewData["SupplierName"] = supplier != null ? supplier.SupplierName : "";
            }

            ViewData["WebOrderNumber"] = form["WebOrderNumber"];
            ViewData["MakerOrderNumber"] = form["MakerOrderNumber"];
            ViewData["InvoiceNo"] = form["InvoiceNo"];
            ViewData["PurchaseStatus"] = form["PurchaseStatus"];
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel<c_PurchaseStatus>(dao.GetPurchaseStatusAll(false), form["PurchaseStatus"], false);


            ViewData["LinkEntryCaptureDateFrom"] = form["LinkEntryCaptureDateFrom"];        //Add 2018/03/26 arc yano #3863
            ViewData["LinkEntryCaptureDateTo"] = form["LinkEntryCaptureDateTo"];            //Add 2018/03/26 arc yano #3863

        }
        #endregion

        #region チェックした項目の編集
        /// <summary>
        /// チェックした項目の編集
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// /// 2016/06/20 arc yano #3585 部品入荷一覧　引数追加(PurchaseStatus) 
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCheckedItemList(List<bool> check, List<PurchaseEntryKeyList> KeyList, string PurchaseStatus)
        {
            List<PurchaseEntryKeyList> CheckedList = new List<PurchaseEntryKeyList>();

            for (int i = 0; i < check.Count(); i++)
            {
                if (check[i] == true)   //チェックが入っていた場合
                {
                    CheckedList.Add(KeyList[i]);
                }
            }

            return Entry(CheckedList, PurchaseStatus);
        }
        #endregion


        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 入力画面表示
        /// <summary>
        /// 入力画面表示
        /// </summary>
        /// <param name="KeyList">入荷伝票番号・発注番号・部品番号</param>
        /// <returns>入力画面</returns>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力　入荷済データの編集可とする
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// 2017/12/20 arc yano #3848 部品入荷入力　部品入荷入力画面表示時のシステムエラー　発注データが取得できない場合は部門コードを設定しに
        /// 2016/08/08 arc yano #3624 引数(DepartmentCode)追加 ログインユーザの所属部門ではなく、選択した入荷・発注データの部門コードを入荷入力画面に表示する
        /// 2016/06/27 arc yano #3585 部品入荷一覧　入荷ステータス=「入荷済」の場合は、入荷レコードの取得方法を変更する
        /// 2016/06/20 arc yano #3585 部品入荷一覧　引数追加(PurchaseStatus) 
        /// 2016/04/21 arc nakayama #3493_部品入荷入力　仕掛ロケーションを持たない部門の選択時の不具合
        /// </history>
        public ActionResult Entry(List<PurchaseEntryKeyList> KeyList, string PurchaseStatus = "", bool OrderNumberClick = false, string PurchaseOrderNumber = "", string DepartmentCode = "")
        {            
            FormCollection form = new FormCollection();
            PartsPurchase_PurchaseList PList = new PartsPurchase_PurchaseList();
            List<GetPartsPurchaseList_Result> line = new List<GetPartsPurchaseList_Result>();

            //共通項目
            //Mod 2016/08/08 arc yano #3624
            //Mod 2016/04/21 arc nakayama #3493
            //Department department = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode, includeDeleted: false, closeMonthFlag: "2");

            string departmentCode = "";

            //データ編集の場合
            if (KeyList != null && KeyList.Count() > 0)
            {
                if (!string.IsNullOrWhiteSpace(KeyList[0].PurchaseNumber))
                {
                    PartsPurchase pprec = new PartsPurchaseDao(db).GetByKey(KeyList[0].PurchaseNumber);
                    //入荷レコードから部門コードを設定
                    departmentCode = pprec != null ? pprec.DepartmentCode : ""; //Mod 2017/12/20 arc yano #3848
                }
                else if (!string.IsNullOrWhiteSpace(KeyList[0].PurchaseOrderNumber))
                {
                    PartsPurchaseOrder porec = new PartsPurchaseOrderDao(db).GetByKey(KeyList[0].PurchaseOrderNumber);
                    //発注レコードから部門コードを設定
                    departmentCode = porec != null ? porec.DepartmentCode : "";      //Mod 2017/12/20 arc yano #3848
                }
                else
                {
                    //何もしない
                }
            }
           
            Department department = new DepartmentDao(db).GetByKey(departmentCode, includeDeleted: false, closeMonthFlag: "2");  
            
            if (department != null)
            {
                PList.DepartmentCode = department.DepartmentCode; //部門コード
                form["DepartmentCode"] = PList.DepartmentCode;
                PList.DepartmentName = department != null ? department.DepartmentName : ""; //部門名
                form["DepartmentName"] = PList.DepartmentName;
            }
            PList.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //社員コード
            form["CancelEmployeeCode"] = form["EmployeeCode"] = PList.EmployeeCode;     //Mod 2018/11/12 yano #3949
            if (!string.IsNullOrEmpty(PList.EmployeeCode))
            {
                Employee employee = new EmployeeDao(db).GetByKey(PList.EmployeeCode);
                PList.EmployeeName = employee != null ? employee.EmployeeName : ""; //社員名
                form["EmployeeName"] = PList.EmployeeName;
            }
            PList.PurchaseDate = string.Format("{0:yyyy/MM/dd}", DateAndTime.Today);  //入荷日(当日)
            
            form["CancelPurchaseDate"] = form["PurchaseDate"] = PList.PurchaseDate;                     //Mod 2018/11/12 yano #3949


            PList.PurchaseType = "001"; //入荷伝票区分（返品で保存されるケースがないため、初回で開くときは"入荷"固定）
            form["PurchaseType"] = PList.PurchaseType;

            form["PurchaseStatus"] = string.IsNullOrWhiteSpace(PurchaseStatus) ? "001" : PurchaseStatus;        //Add 2016/06/20 arc yano #3585

            List<GetPartsPurchaseList_Result> lineItem = new List<GetPartsPurchaseList_Result>();

            if (OrderNumberClick)
            {
                lineItem = new PartsPurchaseDao(db).GetPurchaseByPurchaseOrderNumber(PurchaseOrderNumber, PList.DepartmentCode);
                line.AddRange(lineItem);
            }
            else
            {
                foreach (var Key in KeyList)
                {

                    if (string.IsNullOrEmpty(Key.PurchaseNumber) && string.IsNullOrEmpty(Key.PurchaseOrderNumber) && string.IsNullOrEmpty(Key.PartsNumber))
                    {
                        //新規作成
                        PList = new PartsPurchase_PurchaseList();
                    }
                    else
                    {
                        //-----------------------------------------------------------------------
                        //チェックした項目の編集または、入荷伝票番号クリック
                        //-----------------------------------------------------------------------
                        lineItem = new PartsPurchaseDao(db).GetPurchaseByCondition(Key.PurchaseNumber, Key.PurchaseOrderNumber, Key.PartsNumber, PList.DepartmentCode, PurchaseStatus);
                        
                        //表示リストに追加
                        line.AddRange(lineItem);
                    }
                }
            }

            //Mod 2018/06/01 arc yano #3894
            //Add 2018/01/15 arc yano #3833
            //--------------------------------------------
            //純正品のデフォルトの仕入先を取得
            //--------------------------------------------
            form["GenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode").Value;
            form["GenuineSupplierName"] = new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]).SupplierName : "";

            form["JLRGenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode_JLR").Value;
            form["JLRGenuineSupplierName"] = new SupplierDao(db).GetByKey(form["JLRGenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["JLRGenuineSupplierCode"]).SupplierName : "";

            //Add 2018/01/15 arc yano
            foreach (var l in line)
            {
                //仕入先が空欄でかつ部品番号が空欄でない場合
                if (string.IsNullOrWhiteSpace(l.SupplierCode) && !string.IsNullOrWhiteSpace(l.PartsNumber))
                {
                    Parts parts = new PartsDao(db).GetByKey(l.PartsNumber, false);

                    //その部品が純正品の場合
                    if (parts != null && parts.GenuineType.Equals("001"))
                    {
                        //デフォルトの仕入先を設定
                        //l.SupplierCode = form["GenuineSupplierCode"];
                        //l.SupplierName = form["GenuineSupplierName"];

                        //部品のメーカーコードが「JG」または「LR」の場合は仕入先=JLR それ以外の場合はFCJ

                        if (parts.MakerCode.Equals("JG") || parts.MakerCode.Equals("LR"))
                        {
                            l.SupplierCode = form["JLRGenuineSupplierCode"];
                            l.SupplierName = form["JLRGenuineSupplierName"];
                        }
                        else
                        {
                            l.SupplierCode = form["GenuineSupplierCode"];
                            l.SupplierName = form["GenuineSupplierName"];
                        }
                    }
                }
            }

            PList.line = line;

            //Mod 2016/06/28 arc yano #3585 引数追加
            //入荷ステータス = 「入荷済」の場合は入荷レコードを元に設定する
            if (string.IsNullOrWhiteSpace(PurchaseStatus) || PurchaseStatus.Equals("001"))
            {
                SetEntryDataComponent(form);                
            }
            else
            {
                SetEntryDataComponent(form, PList.line);                
            }

            return View("PartsPurchaseEntry", PList);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 入荷処理
        /// <summary>
        /// 入荷処理
        /// </summary>
        /// <param name="purchase">仕入データ</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns>入力画面</returns>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力 入荷済データの編集
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 在庫更新処理の引数の追加
        /// 2017/11/06 arc yano #3808 部品入荷入力 Webオーダー番号欄の追加
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　発注部品番号のDB登録、取消・キャンセル処理の分岐の追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ロケーションの抽出条件の変更(部門→倉庫)
        /// 2016/08/05 arc yano #3625 部品入荷入力　サービス伝票に紐づく入荷処理の引当処理 引当処理の引数(入荷ロケーション)追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {

            //Mod 2018/11/12 yano #3949
            if (form["RequestFlag"].Equals("1") || form["RequestFlag"].Equals("2")) //入荷取消 or キャンセル
            {
                return Cancel(Plist, Purchase, form);
            }
            else if (form["RequestFlag"].Equals("3"))                               //入荷済データ保存
            {
                return Save(Plist, Purchase, form);
            }
            else                                                                    //入荷確定
            {
                return Confirm(Plist, Purchase, form);
            }

            ////Mod 2017/08/02 arc yano #3783
            ////入荷取消 or 入荷キャンセル
            //if (form["RequestFlag"].Equals("1") || form["RequestFlag"].Equals("2"))
            //{
            //   return Cancel(Plist, Purchase, form);
            //}
            //else
            //{
            //    //Validationチェック
            //    ValidateSave(Plist, Purchase, form);
            //    if (!ModelState.IsValid)
            //    {
            //        SetEntryDataComponent(form);
            //        Plist.line = Purchase;
            //        return View("PartsPurchaseEntry", Plist);
            //    }

            //    // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            //    db = new CrmsLinqDataContext();
            //    db.Log = new OutputWriter();

            //    stockService = new StockService(db);
            //    service = new ServiceSalesOrderService(db);

            //    using (TransactionScope ts = new TransactionScope())
            //    {
            //        for (int i = 0; i < Purchase.Count; i++)
            //        {
            //            //入荷数が未入力の場合は対象外（代替部品の元レコードがその対象になる）
            //            if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
            //            {
            //                PartsPurchase partspurchase = new PartsPurchase();
            //                //入荷伝票番号があれば存在チェック （入荷伝票番号がない/存在しなければ新規作成）
            //                if (!string.IsNullOrEmpty(Purchase[i].PurchaseNumber))
            //                {
            //                    partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);
            //                }
            //                else
            //                {
            //                    partspurchase = null;
            //                }

            //                string OrderPartsNumber; //発注部品番号(発注とは別の部品が入荷した場合、発注した部品番号をSETする、そうでなければ、入荷部品番号)
            //                if (Purchase[i].ChangeParts)
            //                {
            //                    OrderPartsNumber = Purchase[i].ChangePartsNumber;
            //                }
            //                else
            //                {
            //                    OrderPartsNumber = Purchase[i].PartsNumber;
            //                }


            //                if (partspurchase == null)
            //                {

            //                    //新規作成

            //                    //▼入荷テーブル
            //                    PartsPurchase purchase = new PartsPurchase();
            //                    purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //入荷伝票番号
            //                    purchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //発注伝票番号
            //                    purchase.PurchaseType = form["PurchaseType"];                                   //入荷伝票区分
            //                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
            //                    purchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;       //入荷予定日
            //                    purchase.PurchaseDate = DateTime.Parse(Plist.PurchaseDate);                     //入荷日
            //                    purchase.PurchaseStatus = "002";                                                //仕入ステータス
            //                    purchase.SupplierCode = Purchase[i].SupplierCode;                               //仕入先コード
            //                    purchase.EmployeeCode = form["EmployeeCode"];                                   //社員コード
            //                    purchase.DepartmentCode = form["DepartmentCode"];                               //部門コード
            //                    purchase.LocationCode = Purchase[i].LocationCode;                               //ロケーションコード
            //                    purchase.PartsNumber = Purchase[i].PartsNumber;                                 //部品番号
            //                    purchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //単価
            //                    purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //入荷数
            //                    purchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //金額
            //                    purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //納品書番号
            //                    purchase.Memo = Purchase[i].Memo;                                               //メモ
            //                    purchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //インボイス番号
            //                    purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //メーカーオーダー番号
            //                    purchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Webオーダー番号             //Add 2017/11/06 arc yano #3808
            //                    purchase.CreateDate = DateTime.Now;                                             //作成日
            //                    purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
            //                    purchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
            //                    purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
            //                    purchase.DelFlag = "0";
            //                    // Add 2014/09/10 arc amii 部品入荷履歴対応 改訂番号を1に設定
            //                    purchase.RevisionNumber = 1;

            //                    purchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //発注部品番号    //Add 2017/08/02 arc yano #3783


            //                    db.PartsPurchase.InsertOnSubmit(purchase);
            //                }
            //                else
            //                {
            //                    //更新
            //                    partspurchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //発注伝票番号
            //                    partspurchase.PurchaseType = form["PurchaseType"];                                   //入荷伝票区分

            //                    //入荷予定日が未設定だった場合、発注の入荷予定日で更新する
            //                    if (partspurchase.PurchasePlanDate == null)                                          //入荷予定日
            //                    {
            //                        PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
            //                        partspurchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;
            //                    }
            //                    partspurchase.PurchaseDate = DateTime.Parse(form["PurchaseDate"]);                   //入荷日
            //                    partspurchase.PurchaseStatus = "002";                                                //仕入ステータス
            //                    partspurchase.SupplierCode = Purchase[i].SupplierCode;                               //仕入先コード
            //                    partspurchase.DepartmentCode = form["DepartmentCode"];                               //部門コード
            //                    partspurchase.LocationCode = Purchase[i].LocationCode;                               //ロケーションコード
            //                    partspurchase.PartsNumber = Purchase[i].PartsNumber;                                 //部品番号 
            //                    partspurchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //単価
            //                    partspurchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //入荷数
            //                    partspurchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //金額
            //                    partspurchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //納品書番号
            //                    partspurchase.Memo = Purchase[i].Memo;                                               //メモ
            //                    partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //インボイス番号
            //                    partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //メーカーオーダー番号
            //                    partspurchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Webオーダー番号           //Add 2017/11/06 arc yano #3808 
            //                    partspurchase.CreateDate = DateTime.Now;                                             //作成日
            //                    partspurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
            //                    partspurchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
            //                    partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
            //                    partspurchase.DelFlag = "0";

            //                    partspurchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //発注部品番号    //Add 2017/08/02 arc yano #3783
            //                }

            //                //Mod 2016/08/13 arc yano #3596
            //                //部門コードから使用倉庫を割出
            //                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, Plist.DepartmentCode);

            //                string warehouseCode = "";
            //                if (dWarehouse != null)
            //                {
            //                    warehouseCode = dWarehouse.WarehouseCode;
            //                }

            //                //▼発注更新
            //                //発注伝票番号が入力されていたら、該当の発注データを更新する。該当するデータがない場合は何もしない
            //                if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
            //                {
            //                    UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, Purchase[i].PurchaseQuantity);     //Mod 2017/11/06 arc yano #3808
            //                }
            //                //▼部品ロケーション  該当する部品ロケーションが存在していなかったら新規登録/存在していたら更新
            //                //UpdatePartsLocation(Purchase[i].PartsNumber, form["DepartmentCode"], Purchase[i].LocationCode);
            //                UpdatePartsLocation(Purchase[i].PartsNumber, warehouseCode, Purchase[i].LocationCode);  //Mod 2016/08/13 arc yano #3596

            //                //▼部品在庫更新
            //                UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, form["PurchaseType"], Purchase[i].Price);     //Mod 2018/05/14 arc yano #3880

            //                //▼サービス伝票情報更新 受注伝票番号が存在した場合のみ更新
            //                if (!string.IsNullOrEmpty(Purchase[i].SlipNumber))
            //                {
            //                    string OrderType = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber).OrderType; //発注区分

            //                    ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetBySlipNumber(Purchase[i].SlipNumber);
            //                    EntitySet<ServiceSalesLine> ServiceLines = header.ServiceSalesLine;
            //                    //Mod 2016/08/13 arc yano #3596
            //                    //string shikakariLocation = stockService.GetShikakariLocation(Plist.DepartmentCode).LocationCode;
            //                    string shikakariLocation = stockService.GetShikakariLocation(warehouseCode).LocationCode;
            //                    service.PurchaseHikiate(ref header, ref ServiceLines, shikakariLocation, OrderType, OrderPartsNumber, Purchase[i].PartsNumber, Purchase[i].PurchaseQuantity, Purchase[i].Price, Purchase[i].Amount, ((Employee)Session["Employee"]).EmployeeCode, Purchase[i].SupplierCode, Purchase[i].LocationCode);
            //                }
            //            }
            //        }

            //        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            //        {
            //            try
            //            {
            //                db.SubmitChanges();
            //                //コミット
            //                ts.Complete();
            //                break;
            //            }
            //            catch (ChangeConflictException e)
            //            {
            //                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
            //                {
            //                    occ.Resolve(RefreshMode.KeepCurrentValues);
            //                }
            //                if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
            //                {
            //                    // セッションにSQL文を登録
            //                    Session["ExecSQL"] = OutputLogData.sqlText;
            //                    // ログに出力
            //                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
            //                    // エラーページに遷移
            //                    return View("Error");
            //                }
            //            }
            //            catch (SqlException e)
            //            {
            //                //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
            //                // セッションにSQL文を登録
            //                Session["ExecSQL"] = OutputLogData.sqlText;

            //                if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
            //                {
            //                    //Add 2014/08/08 arc amii エラーログ対応 ログ出力処理追加
            //                    OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

            //                    ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
            //                    Plist.line = Purchase;
            //                    SetEntryDataComponent(form);
            //                    return View("PartsPurchaseEntry", Plist);
            //                }
            //                else
            //                {
            //                    //Mod 2014/08/08 arc amii エラーログ対応 『theow e』からログ出力処理に変更
            //                    // ログに出力
            //                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
            //                    return View("Error");
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                //Add 2014/08/08 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
            //                // セッションにSQL文を登録
            //                Session["ExecSQL"] = OutputLogData.sqlText;
            //                // ログに出力
            //                OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
            //                // エラーページに遷移
            //                return View("Error");
            //            }
            //        }
            //    }

            //    SetEntryDataComponent(form);

            //    ViewData["close"] = "1";
            //    Plist.line = Purchase;
            //    return View("PartsPurchaseEntry", Plist);
            //}
        }
        #endregion



        #region 入荷保存処理
        /// <summary>
        /// 入荷保存
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力 入荷済データの編集 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult Save(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
          
            // エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {

                    PartsPurchase partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);

                    //特定の項目(インボイス番号、メーカーオーダー番号、備考、最終更新者、最終更新日)のみ更新
                    partspurchase.Memo = Purchase[i].Memo;                                               //メモ
                    partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //インボイス番号
                    partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //メーカーオーダー番号
                    partspurchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
                    partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者

                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
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
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii エラーログ対応 ログ出力処理追加
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii エラーログ対応 『theow e』からログ出力処理に変更
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            return View("PartsPurchaseEntry", Plist);

        }
        #endregion


        #region 入荷確定処理
         /// <summary>
        /// 入荷確定
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力 入荷済データの編集 確定処理を外だし
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult Confirm(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //--------------------------
            //入力値チェック
            //--------------------------
            //Validationチェック
            ValidateSave(Plist, Purchase, form);
            if (!ModelState.IsValid)
            {
                SetEntryDataComponent(form);
                Plist.line = Purchase;
                return View("PartsPurchaseEntry", Plist);
            }

            // Add 2014/08/08 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {
                    //入荷数が未入力の場合は対象外（代替部品の元レコードがその対象になる）
                    if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
                    {
                        PartsPurchase partspurchase = new PartsPurchase();
                        //入荷伝票番号があれば存在チェック （入荷伝票番号がない/存在しなければ新規作成）
                        if (!string.IsNullOrEmpty(Purchase[i].PurchaseNumber))
                        {
                            partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);
                        }
                        else
                        {
                            partspurchase = null;
                        }

                        string OrderPartsNumber; //発注部品番号(発注とは別の部品が入荷した場合、発注した部品番号をSETする、そうでなければ、入荷部品番号)
                        if (Purchase[i].ChangeParts)
                        {
                            OrderPartsNumber = Purchase[i].ChangePartsNumber;
                        }
                        else
                        {
                            OrderPartsNumber = Purchase[i].PartsNumber;
                        }


                        if (partspurchase == null)
                        {

                            //新規作成

                            //▼入荷テーブル
                            PartsPurchase purchase = new PartsPurchase();
                            purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //入荷伝票番号
                            purchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //発注伝票番号
                            purchase.PurchaseType = form["PurchaseType"];                                   //入荷伝票区分
                            PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
                            purchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;       //入荷予定日
                            purchase.PurchaseDate = DateTime.Parse(Plist.PurchaseDate);                     //入荷日
                            purchase.PurchaseStatus = "002";                                                //仕入ステータス
                            purchase.SupplierCode = Purchase[i].SupplierCode;                               //仕入先コード
                            purchase.EmployeeCode = form["EmployeeCode"];                                   //社員コード
                            purchase.DepartmentCode = form["DepartmentCode"];                               //部門コード
                            purchase.LocationCode = Purchase[i].LocationCode;                               //ロケーションコード
                            purchase.PartsNumber = Purchase[i].PartsNumber;                                 //部品番号
                            purchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //単価
                            purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //入荷数
                            purchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //金額
                            purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //納品書番号
                            purchase.Memo = Purchase[i].Memo;                                               //メモ
                            purchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //インボイス番号
                            purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //メーカーオーダー番号
                            purchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Webオーダー番号             //Add 2017/11/06 arc yano #3808
                            purchase.CreateDate = DateTime.Now;                                             //作成日
                            purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
                            purchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
                            purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                            purchase.DelFlag = "0";
                            // Add 2014/09/10 arc amii 部品入荷履歴対応 改訂番号を1に設定
                            purchase.RevisionNumber = 1;

                            purchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //発注部品番号    //Add 2017/08/02 arc yano #3783


                            db.PartsPurchase.InsertOnSubmit(purchase);
                        }
                        else
                        {
                            //更新
                            partspurchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //発注伝票番号
                            partspurchase.PurchaseType = form["PurchaseType"];                                   //入荷伝票区分

                            //入荷予定日が未設定だった場合、発注の入荷予定日で更新する
                            if (partspurchase.PurchasePlanDate == null)                                          //入荷予定日
                            {
                                PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
                                partspurchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;
                            }
                            partspurchase.PurchaseDate = DateTime.Parse(form["PurchaseDate"]);                   //入荷日
                            partspurchase.PurchaseStatus = "002";                                                //仕入ステータス
                            partspurchase.SupplierCode = Purchase[i].SupplierCode;                               //仕入先コード
                            partspurchase.DepartmentCode = form["DepartmentCode"];                               //部門コード
                            partspurchase.LocationCode = Purchase[i].LocationCode;                               //ロケーションコード
                            partspurchase.PartsNumber = Purchase[i].PartsNumber;                                 //部品番号 
                            partspurchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //単価
                            partspurchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //入荷数
                            partspurchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //金額
                            partspurchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //納品書番号
                            partspurchase.Memo = Purchase[i].Memo;                                               //メモ
                            partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //インボイス番号
                            partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //メーカーオーダー番号
                            partspurchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Webオーダー番号           //Add 2017/11/06 arc yano #3808 
                            partspurchase.CreateDate = DateTime.Now;                                             //作成日
                            partspurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
                            partspurchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
                            partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                            partspurchase.DelFlag = "0";

                            partspurchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //発注部品番号    //Add 2017/08/02 arc yano #3783
                        }

                        //Mod 2016/08/13 arc yano #3596
                        //部門コードから使用倉庫を割出
                        DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, Plist.DepartmentCode);

                        string warehouseCode = "";
                        if (dWarehouse != null)
                        {
                            warehouseCode = dWarehouse.WarehouseCode;
                        }

                        //▼発注更新
                        //発注伝票番号が入力されていたら、該当の発注データを更新する。該当するデータがない場合は何もしない
                        if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                        {
                            UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, Purchase[i].PurchaseQuantity);     //Mod 2017/11/06 arc yano #3808
                        }
                        //▼部品ロケーション  該当する部品ロケーションが存在していなかったら新規登録/存在していたら更新
                        //UpdatePartsLocation(Purchase[i].PartsNumber, form["DepartmentCode"], Purchase[i].LocationCode);
                        UpdatePartsLocation(Purchase[i].PartsNumber, warehouseCode, Purchase[i].LocationCode);  //Mod 2016/08/13 arc yano #3596

                        //▼部品在庫更新
                        UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, form["PurchaseType"], Purchase[i].Price);     //Mod 2018/05/14 arc yano #3880

                        //▼サービス伝票情報更新 受注伝票番号が存在した場合のみ更新
                        if (!string.IsNullOrEmpty(Purchase[i].SlipNumber))
                        {
                            string OrderType = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber).OrderType; //発注区分

                            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetBySlipNumber(Purchase[i].SlipNumber);
                            EntitySet<ServiceSalesLine> ServiceLines = header.ServiceSalesLine;
                            //Mod 2016/08/13 arc yano #3596
                            //string shikakariLocation = stockService.GetShikakariLocation(Plist.DepartmentCode).LocationCode;
                            string shikakariLocation = stockService.GetShikakariLocation(warehouseCode).LocationCode;
                            service.PurchaseHikiate(ref header, ref ServiceLines, shikakariLocation, OrderType, OrderPartsNumber, Purchase[i].PartsNumber, Purchase[i].PurchaseQuantity, Purchase[i].Price, Purchase[i].Amount, ((Employee)Session["Employee"]).EmployeeCode, Purchase[i].SupplierCode, Purchase[i].LocationCode);
                        }
                    }
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
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
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 SQL文をセッションに登録する処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii エラーログ対応 ログ出力処理追加
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii エラーログ対応 『theow e』からログ出力処理に変更
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii エラーログ対応 上記Exception以外の時のエラー処理追加
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            return View("PartsPurchaseEntry", Plist);
        
        }
        #endregion

        #region 削除・入荷キャンセル
        /// <summary>
        /// 入荷キャンセル
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力　入荷済データの編集可とする
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 在庫更新処理の引数の追加
        /// 2017/11/06 arc yano #3808 部品入荷入力　Webオーダー番号入力欄の追加
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //----------------------
            //Validationチェック
            //----------------------
            ValidateCancel(Plist, Purchase, form);
            if (!ModelState.IsValid)
            {
                SetEntryDataComponent(form);
                Plist.line = Purchase;
                return View("PartsPurchaseEntry", Plist);
            }

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            //----------------------------
            //削除・キャンセル処理
            //----------------------------
            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {
                    //-------------------------------
                    //▼入荷データの削除・キャンセル
                    //-------------------------------
                    //入荷レコードを取得する
                    PartsPurchase targetrec = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);

                    //入荷取消(入荷済レコードを論理削除)
                    if (!string.IsNullOrWhiteSpace(form["RequestFlag"]) && form["RequestFlag"].Equals("1"))
                    {
                        if (targetrec != null)
                        {
                            targetrec.DelFlag = "1";
                            targetrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            targetrec.LastUpdateDate = DateTime.Now;
                        }
                    }
                    else //入荷キャンセル
                    {
                        //▼入荷テーブル(返品用)
                        PartsPurchase purchase = new PartsPurchase();
                        purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();                                               //入荷伝票番号
                        purchase.PurchaseOrderNumber = targetrec.PurchaseOrderNumber;                                                           //発注伝票番号
                        purchase.PurchaseType = "002";                                                                                          //入荷伝票区分(=返品)
                        purchase.PurchasePlanDate = targetrec.PurchasePlanDate;                                                                 //入荷予定日
                        purchase.PurchaseDate = DateTime.Parse(form["CancelPurchaseDate"]);                                                     //キャンセル日     //Mod 2018/11/12 yano #3949                                           
                        purchase.PurchaseStatus = "002";                                                                                        //仕入ステータス
                        purchase.SupplierCode = Purchase[i].SupplierCode;                                                                       //仕入先コード
                        purchase.EmployeeCode = form["CancelEmployeeCode"];                                                                     //社員コード       //Mod 2018/11/12 yano #3949             
                        purchase.DepartmentCode = form["DepartmentCode"];                                                                       //部門コード
                        purchase.LocationCode = targetrec.LocationCode;                                                                         //ロケーションコード
                        purchase.PartsNumber = targetrec.PartsNumber;                                                                           //部品番号
                        purchase.Price = decimal.Parse(targetrec.Price.ToString());                                                             //単価
                        purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());                                             //入荷数
                        purchase.Amount = decimal.Parse(targetrec.Amount.ToString());                                                           //金額
                        purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                                                                     //納品書番号
                        purchase.Memo = Purchase[i].Memo;                                                                                       //メモ
                        purchase.InvoiceNo = Purchase[i].InvoiceNo;                                                                             //インボイス番号
                        purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                                                               //メーカーオーダー番号
                        purchase.CreateDate = DateTime.Now;                                                                                     //作成日
                        purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                             //作成者
                        purchase.LastUpdateDate = DateTime.Now;                                                                                 //最終更新日
                        purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                         //最終更新者
                        purchase.DelFlag = "0";
                        purchase.RevisionNumber = 1;
                        purchase.OrderPartsNumber = Purchase[i].ChangePartsNumber;                                                              //発注部品番号
                        db.PartsPurchase.InsertOnSubmit(purchase);
                    }

                    //▼発注更新
                    string OrderPartsNumber; //発注部品番号(発注とは別の部品が入荷した場合、発注した部品番号をSETする、そうでなければ、入荷部品番号)
                    if (!string.IsNullOrWhiteSpace(Purchase[i].ChangePartsNumber))
                    {
                        OrderPartsNumber = Purchase[i].ChangePartsNumber;
                    }
                    else
                    {
                        OrderPartsNumber = Purchase[i].PartsNumber;
                    }

                    //発注伝票番号が入力されていたら、該当の発注データを更新する。該当するデータがない場合は何もしない
                    if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                    {
                        UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, (Purchase[i].PurchaseQuantity * -1));   //Mod 2017/11/06 arc yano #3808
                    }

                    //▼部品在庫更新
                    UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, "002", targetrec.Price); // Mod 2018/05/14 arc yano #3880
                }
                    
                for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //コミット
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // セッションにSQL文を登録
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_CANCEL_NAME, FORM_NAME, "");
                            // エラーページに遷移
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_CANCEL_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "該当の"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            // ログに出力
                            OutputLogger.NLogFatal(e, PROC_CANCEL_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // セッションにSQL文を登録
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ログに出力
                        OutputLogger.NLogFatal(ex, PROC_CANCEL_NAME, FORM_NAME, "");
                        // エラーページに遷移
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            
            return View("PartsPurchaseEntry", Plist);
        }
        #endregion

        #region 部品入荷行追加・削除
        /// <summary>
        /// 部品入荷行追加
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3807 部品入荷入力 １０行ボタンの追加
        /// 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        public ActionResult AddLine(PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            if (Purchase == null)
            {
                Purchase = new List<GetPartsPurchaseList_Result>();
            }


            //Mod 2017/11/07 arc yano #3807
            int addLine = int.Parse(form["addLine"]);   

            for (int i = 0; i < addLine; i++)
            {
                GetPartsPurchaseList_Result Addline = new GetPartsPurchaseList_Result();
                Purchase.Add(Addline);
            }
            
            ModelState.Clear();

            Plist.line = Purchase;

            SetEntryDataComponent(form);
            return View("PartsPurchaseEntry", Plist);
        }

        /// <summary>
        /// 部品入荷行削除
        /// </summary>
        /// <param name="id">行番号</param>
        /// <returns></returns>
        public ActionResult DelLine(int id, PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            Purchase.RemoveAt(id);
            
            ModelState.Clear();

            Plist.line = Purchase;
            SetEntryDataComponent(form);
            return View("PartsPurchaseEntry", Plist);
        }
        #endregion


        #region 部品発注データ更新
        /// <summary>
        /// 部品発注データ更新
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/11/06 arc yano #3808 部品入荷入力　Webオーダー番号入力欄の追加 引数の変更
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　キャンセルした時のステータス戻し処理の追加
        /// 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        private void UpdatePartsPurchaseOrder(GetPartsPurchaseList_Result Purchase, string PartsNumber, decimal? PurchaseQuantity)
        {
            decimal? RemQuantity = 0; //発注残

            PartsPurchaseOrder purchaseOrderRet = new PartsPurchaseOrderDao(db).GetByKey(Purchase.PurchaseOrderNumber, PartsNumber);
            if (purchaseOrderRet != null)
            {
                //発注残 = 発注残 - 入荷数
                RemQuantity = (purchaseOrderRet.RemainingQuantity ?? 0) - (PurchaseQuantity ?? 0);

                //発注ステータス
                //(発注残 - 入荷数) == 0 または (発注残 - 入荷数) < 0　の場合は仕入済　
                if (RemQuantity == 0 || RemQuantity < 0)
                {
                    purchaseOrderRet.PurchaseOrderStatus = "004"; //仕入済
                }
                //Add 2017/08/02 arc yano #3783
                else if (RemQuantity == (purchaseOrderRet.Quantity ?? 0)) //発注残が発注数と同じになった場合
                {
                    purchaseOrderRet.PurchaseOrderStatus = "002"; //発注済
                }
                else
                {
                    purchaseOrderRet.PurchaseOrderStatus = "003"; //分納中
                }

                //Add 2017/11/06 arc yano #3808
                //Webオーダー番号の更新
                if (!string.IsNullOrWhiteSpace(Purchase.WebOrderNumber))
                {
                    purchaseOrderRet.WebOrderNumber = Purchase.WebOrderNumber;
                }

                purchaseOrderRet.RemainingQuantity = RemQuantity; //発注残
                purchaseOrderRet.LastUpdateDate = DateTime.Now;   //最終更新日
                purchaseOrderRet.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
            }
        }
        #endregion

        //Add 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 部品ロケーションの更新
        /// <summary>
        /// 部品ロケーションの更新
        /// </summary>
        /// <param name="PartsNumber">部品番号</param>
        /// <param name="DepartmentCode">部門コード</param>
        /// <param name="LocationCode">ロケーションコード</param>
        /// <returns>入力画面</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ロケーションの抽出条件の変更(部門→倉庫)
        /// 2016/08/05 arc yano #3625 部品入荷入力　サービス伝票に紐づく入荷処理の引当処理 引当処理の引数(入荷ロケーション)追加
        /// </history>
        private void UpdatePartsLocation(string PartsNumber, string WarehouseCode, string LocationCode)
        {
            PartsLocation partslocation = new PartsLocationDao(db).GetByKey(PartsNumber, WarehouseCode);        //Mod 2016/08/13 arc yano #3596
            if (partslocation == null)
            {
                PartsLocation Newpartslocation = new PartsLocation();
                Newpartslocation.PartsNumber = PartsNumber;                                             //部品番号
                Newpartslocation.DepartmentCode = "";                                                   //部門コード ※空文字を設定
                Newpartslocation.LocationCode = LocationCode;                                           //ロケーションコード
                Newpartslocation.CreateDate = DateTime.Now;                                             //作成日
                Newpartslocation.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
                Newpartslocation.LastUpdateDate = DateTime.Now;                                         //最終更新日
                Newpartslocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                Newpartslocation.DelFlag = "0";                                                         //削除フラグ
                Newpartslocation.WarehouseCode = WarehouseCode;                                         //倉庫コード
                db.PartsLocation.InsertOnSubmit(Newpartslocation);
            }
            else
            {
                //partslocation.PartsNumber = PartsNumber;                                              //部品番号
                partslocation.DepartmentCode = "";                                                      //部門コード ※空文字を設定
                partslocation.LocationCode = LocationCode;                                              //ロケーションコード
                partslocation.LastUpdateDate = DateTime.Now;                                            //最終更新日
                partslocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //最終更新者
                partslocation.WarehouseCode = WarehouseCode;                                            //倉庫コード
            }
        }

        #endregion

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region 在庫数量を更新
        /// <summary>
        /// 在庫数量を更新
        /// </summary>
        /// <param name="purchase">仕入データ</param>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 移動平均単価の計算
        /// 2017/02/02 arc yano #3857 部品入荷入力 サービス伝票から発注した部品の入荷処理
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除データ
        private void UpdatePartsStock(string PartsNumber, string LocationCode, decimal? PurchaseQuantity, string PurchaseType, decimal? PurchasePrice)
        {

            //在庫情報の取得(削除済データも取得する)
            PartsStock partsstock = new PartsStockDao(db).GetByKey(PartsNumber, LocationCode, true);   //Mod 2017/02/08 arc yano #3620
            if (partsstock == null)
            {
                PartsStock NewPartsStock = new PartsStock();
                NewPartsStock.PartsNumber = PartsNumber;                                             //部品番号
                NewPartsStock.LocationCode = LocationCode;                                           //ロケーションコード 
                NewPartsStock.Quantity = PurchaseQuantity;                                           //数量
                NewPartsStock.ProvisionQuantity = 0;                                                 //引当済数
                NewPartsStock.CreateDate = DateTime.Now;                                             //作成日
                NewPartsStock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
                NewPartsStock.LastUpdateDate = DateTime.Now;                                         //最終更新日
                NewPartsStock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                NewPartsStock.DelFlag = "0";
                db.PartsStock.InsertOnSubmit(NewPartsStock);
            }
            else
            {
                //Add 2017/02/08 arc yano #3620
                //Del 2016/04/22 arc yano #3506 キー項目の更新を止め
                //partsstock.PartsNumber = PartsNumber;                                            //部品番号
                //partsstock.LocationCode = LocationCode;                                          //ロケーションコード
                //入荷伝票区分が"入荷"の時は数量に加算、"返品"の場合は数量から減算
                

                //削除データの場合は初期化
                partsstock = new PartsStockDao(db).InitPartsStock(partsstock);

                if (PurchaseType.Equals("001"))                                                   //数量
                {
                    partsstock.Quantity = partsstock.Quantity + PurchaseQuantity;
                }
                else
                {
                    partsstock.Quantity = partsstock.Quantity - PurchaseQuantity;
                }
                partsstock.LastUpdateDate = DateTime.Now;                                         //最終更新日
                partsstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                partsstock.DelFlag = "0";
            }

            //Add 2018/05/14 arc yano #3880 //移動平均単価の更新
            new PartsMovingAverageCostDao(db).UpdateAverageCost(PartsNumber, "001", (!string.IsNullOrWhiteSpace(PurchaseType) && PurchaseType.Equals("002") ? PurchaseQuantity * -1 : PurchaseQuantity), PurchasePrice, ((Employee)Session["Employee"]).EmployeeCode);            

            db.SubmitChanges(); //Mod 2017/02/02 arc yano #3857
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region データ付き画面コンポーネントを作成
        /// <summary>
        /// データ付き画面コンポーネントを作成
        /// </summary>
        /// <param name="purchase">仕入伝票データ</param>
        /// <history>
        /// 2018/11/12 yano #3949 部品入荷入力　入荷済データの編集
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応 
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷キャンセルの場合は入荷種別＝返品が選択されるように設定
        /// 2016/06/27 arc yano #3585 引数追加
        /// 2016/06/20 arc yano #3585 入荷ステータス(PurchaseStatus)の設定処理追加
        /// </history>
        private void SetEntryDataComponent(FormCollection form, List<GetPartsPurchaseList_Result> list = null)
        {

            //Add 2018/01/15 arc yano #3833
            //純正品のデフォルト仕入先を設定
            ViewData["GenuineSupplierCode"] = form["GenuineSupplierCode"];
            ViewData["GenuineSupplierName"] = form["GenuineSupplierName"];

            //Add 2018/06/01 arc yano #3894
            ViewData["JLRGenuineSupplierCode"] = form["JLRGenuineSupplierCode"];
            ViewData["JLRGenuineSupplierName"] = form["JLRGenuineSupplierName"];

            //引数として入荷レコードのリストが渡されている場合は、そのリストを元にviewDataを設定する
            if (list != null)
            {
                PartsPurchase ret = new PartsPurchaseDao(db).GetByKey(list[0].PurchaseNumber);

                ViewData["DepartmentCode"] = ret.DepartmentCode;

                ViewData["DepartmentName"] = ret.Department != null ? ret.Department.DepartmentName : "";

                ViewData["EmployeeCode"] = ret.EmployeeCode;

                ViewData["EmployeeName"] = ret.Employee != null ? ret.Employee.EmployeeName : "";

                ViewData["PurchaseDate"] = string.Format("{0:yyyy/MM/dd}", ret.PurchaseDate);
                ViewData["PurchaseType"] = ret.PurchaseType;

                ViewData["PurchaseStatus"] = ret.PurchaseStatus;

                CodeDao dao = new CodeDao(db);
                ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), ret.PurchaseType, false);
            }
            else
            {
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                if (!string.IsNullOrEmpty(form["DepartmentCode"]))
                {
                    ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
                }
                ViewData["EmployeeCode"] = form["EmployeeCode"];
                if (!string.IsNullOrEmpty(form["EmployeeCode"]))
                {
                    ViewData["EmployeeName"] = new EmployeeDao(db).GetByKey(form["EmployeeCode"]).EmployeeName;
                }
                ViewData["PurchaseDate"] = form["PurchaseDate"];

                ViewData["PurchaseType"] = (string.IsNullOrWhiteSpace(form["PurchaseType"]) ? form["hdPurchaseType"] : form["PurchaseType"]);

                ViewData["PurchaseStatus"] = form["PurchaseStatus"]; //Add 2016/06/20 arc yano #3585

                CodeDao dao = new CodeDao(db);
                ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), form["PurchaseType"], false);
            }

            //Add 2017/08/02 arc yano #3783
            if (!string.IsNullOrWhiteSpace(form["PurchaseDate"]) && !string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
               
                DateTime purchaseDate = DateTime.Parse(ViewData["PurchaseDate"].ToString());
                DateTime inventoryMonth = new DateTime(purchaseDate.Year, purchaseDate.Month, 1);

                InventoryScheduleParts condition = new InventoryScheduleParts();

                condition.InventoryMonth = inventoryMonth;
                condition.WarehouseCode = new DepartmentWarehouseDao(db).GetByDepartment(form["DepartmentCode"]).WarehouseCode;
                condition.InventoryType = "002";

                InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);

                ViewData["InventoryStatus"] = rec != null ? rec.InventoryStatus : "";

                //入荷ステータス＝「入荷済」かつ棚卸ステータス=「確定」
                if (rec != null && rec.InventoryStatus.Equals("002") && !string.IsNullOrWhiteSpace(form["PurchaseStatus"]) && form["PurchaseStatus"].Equals("002"))
                {
                    //CodeDao dao = new CodeDao(db);
                    //ViewData["PurchaseType"] = "002";
                    //ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), "002", false);

                    //Mod 2018/11/12 yano #3949
                    ViewData["CancelPurchaseDate"] = form["CancelPurchaseDate"];
                    ViewData["CancelEmployeeCode"] = form["CancelEmployeeCode"];

                    if (!string.IsNullOrWhiteSpace(form["CancelEmployeeCode"]))
                    {
                        ViewData["CancelEmployeeName"] = new EmployeeDao(db).GetByKey(form["CancelEmployeeCode"]).EmployeeName;
                    }
                }
            }

        }
        #endregion        

        //Mod 2015/11/20 arc nakayama #3293_部品入荷入力(#3234_【大項目】部品仕入れ機能の改善)
        #region Validationチェック
        /// <summary>
        /// 入荷確定時のValidateionチェック
        /// </summary>
        /// <param name="purchase">仕入伝票データ</param>
        /// <history>
        /// 2021/02/22 yano #4075 【部品入荷】入荷確定処理の締めチェック漏れ
        /// 2017/03/30 arc yano #3740 部品入荷入力　引当済部品の返品も行える フリー在庫数でチェックするように修正
        /// 2016/03/22 arc yano Validationチェックの処理の不具合の修正
        /// </history>
        private void ValidateSave(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //更新データがあるかないか
            if (Purchase == null || Purchase.Count <= 0)
            {
                ModelState.AddModelError("", "更新対象のデータがありません");
                return;
            }

            //--共通項目
            //--必須チェック

            //担当者
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            //部門コード
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            //入荷日
            if (string.IsNullOrEmpty(form["PurchaseDate"]))
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0001", "入荷日"));
            }

            //--可変項目
            for (int i = 0; i < Purchase.Count; i++)
            {
                //入荷数が未入力の場合は対象外（代替部品の元レコードがその対象になる）
                if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
                {
                    //--必須チェック
                    //部品番号
                    if (string.IsNullOrEmpty(Purchase[i].PartsNumber))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PartsNumber", MessageUtils.GetMessage("E0001", "入荷部品番号"));
                    }
                    //ロケーション
                    if (string.IsNullOrEmpty(Purchase[i].LocationCode))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].LocationCode", MessageUtils.GetMessage("E0001", "ロケーション"));
                    }
                    //入荷数
                    if (Purchase[i].PurchaseQuantity == null)
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0001", "入荷数"));
                    }
                    //入荷単価
                    if (Purchase[i].Price == null)
                    {
                        ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0001", "入荷単価"));
                    }
                    //仕入先
                    if (string.IsNullOrEmpty(Purchase[i].SupplierCode))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].SupplierCode", MessageUtils.GetMessage("E0001", "仕入先"));
                    }

                    //--形式チェック
                    //入荷数
                    if ((Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}$"))))
                    {
                        if (Purchase[i].PurchaseQuantity == 0)  //数量が0の場合
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "入荷数", "0以外の正の整数7桁以内かつ小数2桁以内" }));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "入荷数", "正の整数7桁以内かつ小数2桁以内" }));
                    }

                    //入荷単価
                    if ((Regex.IsMatch(Purchase[i].Price.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].Price.ToString(), @"^\d{1,7}$"))))
                    {
                        if (Purchase[i].Price == 0)  //数量が0の場合
                        {
                            ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "入荷単価", "0以外の正の整数7桁以内かつ小数2桁以内" }));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "入荷単価", "正の整数7桁以内かつ小数2桁以内" }));
                    }


                    if (!string.IsNullOrEmpty(form["Purchase[" + i + "].ChangePartsFlag"]) && form["Purchase[" + i + "].ChangePartsFlag"].Equals("1"))
                    {
                        Purchase[i].ChangeParts = true;
                    }

                    //--存在チェック

                    //発注伝票番号
                    if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                    {
                        PartsPurchaseOrder purchaseorder = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber);    //Mod 2016/03/22 arc yano
                        //Mod 2016/04/25 arc nakayama #3494_部品入荷入力画面　発注情報のない入荷データでのエラー
                        //PartsPurchaseOrder purchaseorder = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber);
                        if (purchaseorder == null)
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseOrderNumber", "入力された発注伝票番号は存在していません");
                        }
                        else
                        {
                            PartsPurchaseOrder PurchaseorderPartsSet = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, Purchase[i].PartsNumber);

                            //入荷済みチェック
                            if (PurchaseorderPartsSet != null)
                            {
                                if (PurchaseorderPartsSet.RemainingQuantity <= 0)
                                {
                                    ModelState.AddModelError("", "発注伝票番号 " + Purchase[i].PurchaseOrderNumber + " 部品番号 " + Purchase[i].PartsNumber + " のレコードはすでに入荷済です。");
                                }
                            }
                        }
                    }

                    //発注部品番号
                    if (Purchase[i].ChangeParts)
                    {
                        if (string.IsNullOrEmpty(form["Purchase[" + i + "].ChangePartsNumber"]))
                        {
                            ModelState.AddModelError("ChangePartsNumber[" + i + "]", "チェックが入っていますが、発注部品番号が未入力です");
                        }

                    }
                    //入荷伝票区分が「返品」の場合
                    if (form["PurchaseType"] == "002")
                    {
                        PartsStock Pstock = new PartsStockDao(db).GetByKey(Purchase[i].PartsNumber, Purchase[i].LocationCode);
                        if (Pstock == null)
                        {
                            ModelState.AddModelError("Purchase[" + i + "].LocationCode", "返品する部品在庫が、入力されているロケーションに存在しません");
                        }
                        else if ((Pstock.Quantity - Pstock.ProvisionQuantity) < Purchase[i].PurchaseQuantity) //Mod 2017/03/30 arc yano #3740
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "指定のロケーション内に、入力された返品数量分の在庫がありません");
                        }
                    }
                }
            }

            //--締日チェック
            if (ModelState.IsValidField("PurchaseDate") && Plist.PurchaseDate != null)
            {
                // Mod 2015/04/20 arc yano 部品系のチェックは経理締、部品棚卸それぞれで締判定処理を行う。また経理締判定では、仮締中変更可能なユーザの場合、仮締めの場合でも変更可能とする
                // Mod 2015/04/15 arc yano　ログインユーザが経理課の場合は、仮締めの場合は、変更可能とする
                //--経理締判定
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "月次締め処理が終了しているので指定された入荷日では入荷できません");
                }
                else //--部品棚卸判定
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        //Mod 2021/02/22 yano #4075
                        //倉庫コードの取得
                        DepartmentWarehouse dw = new DepartmentWarehouseDao(db).GetByDepartment(Plist.DepartmentCode);

                        // Mod 2015/02/03 arc nakayama 部品棚卸情報を車両と分ける対応(InventorySchedule ⇒ InventoryScheduleParts)
                        //if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        {
                            ModelState.AddModelError("PurchaseDate", "部品棚卸処理が終了しているので指定された入荷日では入荷できません");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 入荷キャンセル時のValidateionチェック
        /// </summary>
        /// <param name="purchase">仕入伝票データ</param>
        /// <history>
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能　新規作成
        /// </history>
        private void ValidateCancel(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //部門コード
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            //入荷日
            if (string.IsNullOrEmpty(form["PurchaseDate"]))
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0001", "入荷日"));
            }

            for (int i = 0; i < Purchase.Count; i++)
            {
                //------------------
                //入荷数
                //------------------
                //必須チェック
                if (Purchase[i].PurchaseQuantity == null)
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0001", "入荷数"));
                }

                //形式チェック
                if ((Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}$"))))
                {
                    if (Purchase[i].PurchaseQuantity == 0)  //数量が0の場合
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "入荷数", "0以外の正の整数7桁以内かつ小数2桁以内" }));
                    }
                }
                else
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "入荷数", "正の整数7桁以内かつ小数2桁以内" }));
                }

                //-------------------------------------------------------------------------
                // 発注データチェック ※既に発注数分キャンセルされている場合はエラーとする
                //-------------------------------------------------------------------------
                PartsPurchaseOrder po = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, Purchase[i].PartsNumber);

                //キャンセル数 > 発注残数の場合はエラー
                if (po != null && (Purchase[i].PurchaseQuantity > (po.Quantity - po.RemainingQuantity) ))
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "入荷済数分以上のキャンセルは行えません");
                }

                //-------------------------------------------------------------
                //--入荷した部品が既に引当されている場合はエラーとする
                //-------------------------------------------------------------
                //入荷部品の在庫情報を取得する
                PartsStock rec = new PartsStockDao(db).GetByKey(Purchase[i].PartsNumber, Purchase[i].LocationCode, false);

                decimal? psQuantity = 0m;                   //数量
                decimal? psProvisionQuantity = 0m;          //引当済数
                
                if (rec != null)
                {
                    psQuantity = (rec.Quantity ?? 0);
                    psProvisionQuantity = (rec.ProvisionQuantity ?? 0);
                }
              
                //入荷数がフリー在庫数より多い場合は削除できない
                if (Purchase[i].PurchaseQuantity > (psQuantity - psProvisionQuantity))
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "対象のロケーションに部品の在庫がないため、取消できません");
                }
            }

            //--締日チェック
            if (ModelState.IsValidField("PurchaseDate") && Plist.PurchaseDate != null)
            {
                //--経理締判定
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "月次締め処理が終了しているので指定された入荷日ではキャンセルできません");
                }
                else //--部品棚卸判定
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        //倉庫コードの取得
                        DepartmentWarehouse dw = new DepartmentWarehouseDao(db).GetByDepartment(Plist.DepartmentCode);

                        if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        {
                            ModelState.AddModelError("PurchaseDate", "部品棚卸処理が終了しているので指定された入荷日ではキャンセルできません");
                        }
                    }
                }
            }
        }


        #endregion

        // Add 2015/12/09 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region Excel取込処理
        /// <summary>
        /// Excel取込用ダイアログ表示
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<PurchaseExcelImportList> ImportList = new List<PurchaseExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("PartsPurchaseImportDialog", ImportList);
        }

        /// <summary>
        /// Excel読み込み
        /// </summary>
        /// <param name="purchase">Excelデータ</param>
        /// <history>
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, List<PurchaseExcelImportList> ImportLine, FormCollection form)
        {
            List<PurchaseExcelImportList> ImportList = new List<PurchaseExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel読み込み
                //--------------
                case "1":
                    //Excel読み込み前のチェック
                    ValidateImportFile(importFile);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    //Excel読み込み
                    ImportList = ReadExcelData(importFile, ImportList);

                    //読み込み時に何かエラーがあればここでリターン
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    //Excelで読み込んだデータのバリデートチェック
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form); ;
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    form["ErrFlag"] = "0";
                    SetDialogDataComponent(form);
                    return View("PartsPurchaseImportDialog", ImportList);

                //--------------
                //Excel取り込み
                //--------------
                case "2":

                    DBExecute(ImportLine, form);
                    form["ErrFlag"] = "1"; //取り込んだ後に再度[取り込み]ボタンが押せないようにするため
                    SetDialogDataComponent(form); 
                    return View("PartsPurchaseImportDialog", ImportList);
                //--------------
                //キャンセル
                //--------------
                case "3":

                    ImportList = new List<PurchaseExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[取り込み]ボタンが押せないようにするため

                    return View("PartsPurchaseImportDialog", ImportList);
                //----------------------------------
                //その他(ここに到達することはない)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("PartsPurchaseImportDialog", ImportList);
            }
        }
        #endregion

       
        #region Excelデータ取得&設定
        /// Excelデータ取得&設定
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// Mod 2016/03/03 arc yano #3413 部品マスタ メーカー部品番号の重複 取得項目追加(顧客コード)
        /// Add 2015/12/14 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        private List<PurchaseExcelImportList> ReadExcelData(HttpPostedFileBase importFile, List<PurchaseExcelImportList> ImportList)
        {
            //カラム番号保存用
            int[] colNumber;
            colNumber = new int[9] { -1, -1, -1, -1, -1, -1, -1, -1, -1 };      //Mod 2016/03/03 arc yano #3413

            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "対象のファイルが開かれています。ファイルを閉じてから、再度実行して下さい");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "エラーが発生しました。" + ex.Message);
                    return ImportList;
                }                

                //-----------------------------
                // データシート取得
                //-----------------------------
                var ws = pck.Workbook.Worksheets["Page 1"];

                //Add 2016/03/28 arc nakayama Excel読み込み時にシート名が異なると落ちる対応
                //--------------------------------------
                //読み込むシートが存在しなかった場合
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。シート名を確認して再度実行して下さい"));
                    return ImportList;
                }
                
                //------------------------------
                //読み込み行が0件の場合
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excelにデータがありません。更新処理を終了しました"));
                    return ImportList;
                }

                //読み取りの開始位置と終了位置を取得
                int StartRow = ws.Dimension.Start.Row;　 //行の開始位置
                int EndRow = ws.Dimension.End.Row;       //行の終了位置
                int StartCol = ws.Dimension.Start.Column;//列の開始位置
                int EndCol = ws.Dimension.End.Column;    //列の終了位置

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];
                colNumber = SetColNumber(headerRow, colNumber);
                //タイトル行、ヘッダ行がおかしい場合は即リターンする
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // 読み取り処理
                //------------------------------
                int datarow = 0;
                string[] Result = new string[colNumber.Count()];        //Mod 2016/03/03 arc yano #3413 

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    PurchaseExcelImportList data = new PurchaseExcelImportList();

                    //更新データの取得
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)    //Mod 2016/03/03 arc yano #3413 
                    {

                        for (int i = 0; i < colNumber.Count(); i++)
                        {

                            if (col == colNumber[i])
                            {
                                Result[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }

                    //----------------------------------------
                    // 読み取り結果を画面の項目にセットする
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        // Add 2015/12/14 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region 取込ファイル存在チェック
        /// <summary>
        /// 取込ファイル存在チェック
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void ValidateImportFile(HttpPostedFileBase filePath)
        {
            // 必須チェック
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルを選択してください"));
            }
            else
            {
                // 拡張子チェック
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "ファイルの拡張子がxlsxファイルではありません"));
                }
            }

            return;
        }
        #endregion

        #region 読み込み結果のバリデーションチェック
        /// <summary>
        /// 読み込み結果のバリデーションチェック
        /// </summary>
        /// <param name="importFile">Excelデータ</param>
        /// <returns>なし</returns>
        /// <history>
        /// 2017/02/20 arc yano #3641 金額欄のカンマ表示対応 validationはカンマを除いた形でチェック
        /// 2015/12/14 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        public void ValidateImportList(List<PurchaseExcelImportList> ImportList)
        {
            for (int i = 0; i < ImportList.Count; i++)
            {
                //--------------
                // 必須チェック
                //--------------

                //インボイス番号
                if (string.IsNullOrEmpty(ImportList[i].InvoiceNo))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].InvoiceNo", MessageUtils.GetMessage("E0001", i + 1 +"行目のインボイス番号が入力されていません。インボイス番号"));
                }
                //メーカー部品番号
                if (string.IsNullOrEmpty(ImportList[i].MakerPartsNumber))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].MakerPartsNumber", MessageUtils.GetMessage("E0001", i + 1 + "行目のメーカー部品番号が入力されていません。メーカー部品番号"));
                }
                //仕入単価
                if (ImportList[i].Price == null)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0001", i + 1 + "行目の仕入単価が入力されていません。仕入単価"));
                }
                //数量
                if (ImportList[i].Quantity == null)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Quantity", MessageUtils.GetMessage("E0001", i + 1 + "行目の数量が入力されていません。数量"));
                }
                //マスタチェック　部品
                if (string.IsNullOrEmpty(ImportList[i].PartsNumber))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].MakerPartsNumber", i + 1 + "行目のメーカー部品番号" + ImportList[i].MakerPartsNumber + "は部品マスタに登録されていません。マスタ登録を行ってから再度実行して下さい。");
                }

                // Mod 2017/02/20 arc yano #3641
                /*
                //単価
                if (!Regex.IsMatch(ImportList[i].Price, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Price, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "単価", i + 1 + "行目の単価が正しくありません。正の整数7桁以内かつ小数2桁以内" }));
                }
                */

                string workPrice = string.IsNullOrWhiteSpace(ImportList[i].Price) ? "" : ImportList[i].Price.Replace(",", "");

                //単価
                if (!Regex.IsMatch(workPrice, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(workPrice, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "単価", i + 1 + "行目の単価が正しくありません。正の整数7桁以内かつ小数2桁以内" }));
                }
                //数量
                if (!Regex.IsMatch(ImportList[i].Quantity, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Quantity, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", i + 1 + "行目の数量が正しくありません。正の整数7桁以内かつ小数2桁以内" }));
                }
                // Mod 2017/02/20 arc yano #3641
                /*
                //金額
                if (!Regex.IsMatch(ImportList[i].Amount, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Amount, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", i + 1 + "行目の金額が正しくありません。正の整数7桁以内かつ小数2桁以内" }));
                }
                */

                string workAmount = string.IsNullOrWhiteSpace(ImportList[i].Amount) ? "" : ImportList[i].Amount.Replace(",", "");

                if (!Regex.IsMatch(workAmount, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(workAmount, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Amount", MessageUtils.GetMessage("E0002", new string[] { "金額", i + 1 + "行目の金額が正しくありません。正の整数7桁以内かつ小数2桁以内" }));
                }
            }

        }
        #endregion

        // Add 2015/12/14 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region 各項目の列番号設定
        /// <summary>
        /// 各項目の列番号設定
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/03/03 arc yano #3413 部品マスタ メーカー部品番号の重複 取得項目追加(顧客コード)
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //初期処理
            int cnt = 1;

            //列番号設定
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //インボイス番号
                    if (cell.Text.Contains("インボイス番号"))
                    {
                        colNumber[0] = cnt;
                    }
                    //日付
                    if (cell.Text.Contains("日付"))
                    {
                        colNumber[1] = cnt;
                    }
                    //オーダー番号
                    if (cell.Text.Contains("オーダー番号"))
                    {
                        colNumber[2] = cnt;
                    }
                    //発注伝票番号　Ref. Number[572753]
                    if (cell.Text.Contains("Ref. Number[572753]"))
                    {
                        colNumber[3] = cnt;
                    }
                    //メーカー部品番号　Part N.[572755]
                    if (cell.Text.Contains("Part N.[572755]"))
                    {
                        colNumber[4] = cnt;
                    }
                    //入荷予定数　Req. quantity[572759]
                    if (cell.Text.Contains("Req. quantity[572759]"))
                    {
                        colNumber[5] = cnt;
                    }
                    //単価 仕切価格
                    if (cell.Text.Contains("仕切価格"))
                    {
                        colNumber[6] = cnt;
                    }
                    //金額　合計金額
                    if (cell.Text.Contains("合計金額"))
                    {
                        colNumber[7] = cnt;
                    }
                    //顧客    //Mod 2016/03/03 arc yano #3413 
                    if (cell.Text.Contains("顧客"))
                    {
                        colNumber[8] = cnt;
                    }

                }
                cnt++;
            }

            for (int i = 0; i < colNumber.Length; i++)
            {
                if (colNumber[i] == -1)
                {
                    ModelState.AddModelError("importFile", "ヘッダ行が正しくありません。");
                    break;
                }
            }


            return colNumber;
        }
        #endregion

        // Add 2015/12/14 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region Excelの読み取り結果をリストに設定する
        /// <summary>
        /// 結果をリストに設定する
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/15 arc yano #3832 部品入荷　LinkEntryの[オーダー番号]を部品入荷データの[メーカーオーダー番号]に設定
        /// 2017/02/20 arc yano #3641 金額欄のカンマ表示対応 金額の項目はカンマを取り除かずに設定する
        /// 2016/03/03 arc yano #3468 発注データのない部品の入荷処理の場合、部門はInvoiceAfterSalesの顧客より取得する
        /// 
        /// 2016/03/03 arc yano #3413 部品マスタ メーカー部品番号の重複 取得項目追加(顧客コード) 部品データ取得のメソッドに
        ///                         　　　顧客コードを追加
        /// 2016/02/26 arc yano #3432 部品仕入　LinkEntry、返品行の取込スキップ対応
        /// 　　　　　　　　　　　 　　　　　　　　合計金額がマイナスの場合はリストの設定をスキップする。
        /// 2016/01/21 arc yano #3404_Excel取込時の純正区分の絞込み追加(#3397_【大項目】部品仕入機能改善 課題管理表対応)
        ///                         部品マスタ検索条件に純正区分 = 「純正品」追加
        /// </history>
        public List<PurchaseExcelImportList> SetProperty(ref string[] Result, ref List<PurchaseExcelImportList> ImportList)
        {
            PurchaseExcelImportList SetLine = new PurchaseExcelImportList();

            // インボイス番号
            SetLine.InvoiceNo = Result[0];

            // 入荷予定日
            SetLine.PurchasePlanDate = Result[1];

            
            // メーカーオーダー番号
            //SetLine.WebOrderNumber = Result[2];
            SetLine.MakerOrderNumber = Result[2];

            // 発注伝票番号
            SetLine.PurchaseOrderNumber = Result[3];

            // メーカー部品番号
            SetLine.MakerPartsNumber = Result[4];

            Parts PartsData = new PartsDao(db).GetByMakerPartsNumber(SetLine.MakerPartsNumber, "001", Result[8]);  //Mod 2016/01/21 arc yano #3234
 
            if (PartsData != null)
            {
                // 部品番号
                SetLine.PartsNumber = PartsData.PartsNumber;

                // 部品名
                SetLine.PartsNameJp = PartsData.PartsNameJp;
            }
            else
            {
                SetLine.PartsNumber = "";
                SetLine.PartsNameJp = "";
            }

            // 数量
            SetLine.Quantity = Result[5];

            //Mod 2017/02/20 arc yano #3641
            /*
            // 単価
            SetLine.Price = Result[6].Replace(",", "");

            // 金額
            SetLine.Amount = Result[7].Replace(",", "");
            */

            // 単価
            SetLine.Price = Result[6];

            // 金額
            SetLine.Amount = Result[7];


            //Add 2016/02/26 arc yano #3432 
            decimal workAmount;
            //文字列→decimalに変換
            bool ret = Decimal.TryParse(SetLine.Amount , out workAmount);

            if (ret != false)
            {
                if (workAmount < 0) //金額がマイナスの場合
                {
                    return ImportList;  //処理終了    
                }
            }

            //Mod 2016/03/14 arc yano #3468
            Department dep = new DepartmentDao(db).GetByLEUserCode(Result[8]);
       
            //発注伝票番号と部品番号が設定されているときのみ取得する
            if (!string.IsNullOrEmpty(SetLine.PurchaseOrderNumber) && !string.IsNullOrEmpty(SetLine.PartsNumber))
            {
                PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(SetLine.PurchaseOrderNumber, SetLine.PartsNumber);
                if (PurchaseOrder != null)
                {
                    // 仕入先コード
                    SetLine.SupplierCode = PurchaseOrder.SupplierCode;
                    // 仕入先名
                    SetLine.SupplierName = new SupplierDao(db).GetByKey(SetLine.SupplierCode).SupplierName ?? "";
                    // 部門コード
                    SetLine.DepartmentCode = PurchaseOrder.DepartmentCode;
                    // 受注伝票番号
                    SetLine.SlipNumber = PurchaseOrder.ServiceSlipNumber ?? "";
                    // 社員コード
                    SetLine.EmployeeCode = PurchaseOrder.EmployeeCode;
                }
                else
                {
                    // 仕入先コード
                    SetLine.SupplierCode = "";
                    // 仕入先名
                    SetLine.SupplierName = "";
                    // 部門コード        //Mod 2016/03/14 arc yano #3468
                    SetLine.DepartmentCode = (dep != null ? dep.DepartmentCode : "");
                    //SetLine.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                    // 受注伝票番号
                    SetLine.SlipNumber = "";
                    // 社員コード
                    SetLine.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

            }
            else
            {
                // 仕入先コード
                SetLine.SupplierCode = "";
                // 仕入先名
                SetLine.SupplierName = "";
                // 部門コード        //Mod 2016/03/14 arc yano #3468
                SetLine.DepartmentCode = (dep != null ? dep.DepartmentCode : "");
                //SetLine.DepartmentCode = "";
                // 受注伝票番号
                SetLine.SlipNumber = "";
                // 社員コード
                SetLine.EmployeeCode = "";
            }

            //入荷ステータス(未入荷固定)
            SetLine.PurchaseStatus = "001"; //未入荷
            SetLine.PurchaseStatusName = new CodeDao(db).GetPurchaseStatus("001", false).Name;

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        // Add 2015/12/15 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region ダイアログのデータ付きコンポーネント設定
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
        }
        #endregion

        #region 読み込んだデータをDBに登録
        /// <summary>
        /// DB更新
        /// </summary>
        /// <returns>戻り値(0:正常 1:エラー(部品棚卸画面へ遷移) -1:エラー(エラー画面へ遷移))</returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
        /// 2018/01/15 arc yano #3832 部品入荷　LinkEntryの[オーダー番号]を部品入荷データの[メーカーオーダー番号]に設定
        /// </history>
        private void DBExecute(List<PurchaseExcelImportList> ImportLine, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                string ChangePartsFlag = ""; //代替部品フラグ


                //Add 2016/04/21 arc yano #3503
                List<PartsPurchase> workList = new List<PartsPurchase>();

                //発注テーブルを更新して、入荷テーブルを登録する
                foreach (var LineData in ImportLine)
                {   
                    
                    ChangePartsFlag = "";　//初期化

                    //インボイス番号で現在の入荷テーブルを検索して、ヒットしたら重複あり
                    PartsPurchase InvoiceRet = new PartsPurchaseDao(db).GetByInvoiceNo(LineData.InvoiceNo);
                    
                    //ヒットしなかった場合のみDBに登録
                    if (InvoiceRet == null)
                    {
                        //発注伝票番号と部品番号が揃っているときだけ該当する発注レコードを更新する（それ以外は発注のない入荷と見なす）
                        if (!string.IsNullOrEmpty(LineData.PurchaseOrderNumber) && !string.IsNullOrEmpty(LineData.PartsNumber))
                        {
                            //----------------------------------------------------------------------------------
                            // 発注テーブル更新
                            // 更新時、発注と異なる部品が入荷した場合、代替部品フラグをセットする
                            //  ・発注伝票番号と部品番号で検索してヒットしたら、発注通りの部品が入荷
                            //　・発注伝票番号と部品番号で検索してヒットしなかったら、発注と異なる部品が入荷
                            //----------------------------------------------------------------------------------
                            PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(LineData.PurchaseOrderNumber, LineData.PartsNumber);
                            if (PurchaseOrder != null)
                            {
                                //PurchaseOrder.WebOrderNumber = LineData.WebOrderNumber;       //Del 2018/01/15 arc ynao #3832

                                ChangePartsFlag = "0";

                            }
                            else
                            {
                                ChangePartsFlag = "1";
                            }
                        }


                        //--------------------
                        // 入荷テーブル登録
                        //--------------------
                        PartsPurchase NewPurchase = new PartsPurchase();
                        NewPurchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //入荷伝票番号
                        NewPurchase.PurchaseOrderNumber = LineData.PurchaseOrderNumber;                    //発注伝票番号
                        NewPurchase.PurchaseType = "001";                                                  //入荷伝票区分
                        NewPurchase.PurchasePlanDate = DateTime.Parse(LineData.PurchasePlanDate);          //入荷予定日
                        NewPurchase.PurchaseDate = null;                                                   //入荷日
                        NewPurchase.PurchaseStatus = "001";                                                //仕入ステータス
                        NewPurchase.SupplierCode = LineData.SupplierCode ?? "";                            //仕入先コード
                        NewPurchase.EmployeeCode = LineData.EmployeeCode ?? "";                            //社員コード
                        NewPurchase.DepartmentCode = LineData.DepartmentCode ?? "";                        //部門コード
                        NewPurchase.LocationCode = "";                                                     //ロケーションコード
                        NewPurchase.PartsNumber = LineData.PartsNumber ?? "";                              //部品番号
                        NewPurchase.Price = decimal.Parse(LineData.Price);                                 //単価
                        NewPurchase.Quantity = decimal.Parse(LineData.Quantity);                           //入荷数
                        NewPurchase.Amount = decimal.Parse(LineData.Amount);                               //金額
                        NewPurchase.ReceiptNumber = "";                                                    //納品書番号
                        NewPurchase.Memo = "";                                                             //メモ
                        NewPurchase.InvoiceNo = LineData.InvoiceNo;                                        //インボイス番号
                        NewPurchase.MakerOrderNumber = LineData.MakerOrderNumber;                          //メーカーオーダー番号     //Mod 2018/01/15 arc yano #3832
                        NewPurchase.CreateDate = DateTime.Now;                                             //作成日
                        NewPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //作成者
                        NewPurchase.LastUpdateDate = DateTime.Now;                                         //最終更新日
                        NewPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //最終更新者
                        NewPurchase.DelFlag = "0";
                        NewPurchase.RevisionNumber = 1;
                        NewPurchase.ChangePartsFlag = ChangePartsFlag;                                      //代替部品フラグ   
                        NewPurchase.LinkEntryCaptureDate = DateTime.Today;                                  //取込追加              //Add 2018/03/26 arc yano #3863

                        workList.Add(NewPurchase);

                        //db.PartsPurchase.InsertOnSubmit(NewPurchase);      //Mod 2016/04/21 arc yano #3503
                    }
                    else
                    {
                        //ヒットしたら、取込後にメッセージを表示する
                        ModelState.AddModelError("", "インボイス番号" + LineData.InvoiceNo + "は既に取り込まれているため、取込対象から除外されました。");
                    }
                }

                //Mod 2016/04/21 arc yano #3503 
                db.PartsPurchase.InsertAllOnSubmit(workList);


                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //取り込み完了のメッセージを表示する
                    ModelState.AddModelError("", "取り込みが完了しました。");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        // Add 2015/12/09 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        #region "履歴テーブル登録編集"
        /// <summary>
        /// 履歴テーブル登録データの編集
        /// </summary>
        /// <param name="partsPurchase">登録データ</param>
        /// <returns> ADD 2014/09/10 arc amii 部品入荷履歴対応 新規追加</returns>
        private PartsPurchaseHistory SetRegistHistoryData(PartsPurchase partsPurchase)
        {
            PartsPurchaseHistory history = new PartsPurchaseHistory();

            history.PurchaseNumber = partsPurchase.PurchaseNumber;                 // 入荷伝票番号
            history.PurchaseOrderNumber = partsPurchase.PurchaseOrderNumber;       // 発注伝票番号
            history.PurchaseType = partsPurchase.PurchaseType;                     // 発注区分
            history.PurchasePlanDate = partsPurchase.PurchasePlanDate;             // 入荷予定日
            history.PurchaseDate = partsPurchase.PurchaseDate;                     // 入荷日
            history.PurchaseStatus = partsPurchase.PurchaseStatus;                 // 仕入ステータス
            history.SupplierCode = partsPurchase.SupplierCode;                     // 仕入先
            history.EmployeeCode = partsPurchase.EmployeeCode;                     // 担当者
            history.DepartmentCode = partsPurchase.DepartmentCode;                 // 部門コード
            history.LocationCode = partsPurchase.LocationCode;                     // ロケーションコード
            history.PartsNumber = partsPurchase.PartsNumber;                       // 部品番号
            history.Price = partsPurchase.Price;                                   // 仕入単価
            history.Quantity = partsPurchase.Quantity;                             // 数量
            history.Amount = partsPurchase.Amount;                                 // 金額
            history.ReceiptNumber = partsPurchase.ReceiptNumber;                   // 納品書番号
            history.Memo = partsPurchase.Memo;                                     // 備考
            history.CreateEmployeeCode = partsPurchase.CreateEmployeeCode;         // 作成者
            history.CreateDate = partsPurchase.CreateDate;                         // 作成日
            history.LastUpdateEmployeeCode = partsPurchase.LastUpdateEmployeeCode; // 最終更新者
            history.LastUpdateDate = partsPurchase.LastUpdateDate;                 // 最終更新日時
            history.DelFlag = "1";                                                 // 削除フラグ
            history.ServiceSlipNumber = partsPurchase.ServiceSlipNumber;           // サービス伝票番号
            history.RevisionNumber = partsPurchase.RevisionNumber;                 // 改訂番号


            return history;
        }

        #endregion

    }
}
