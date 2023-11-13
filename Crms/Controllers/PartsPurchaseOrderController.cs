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
using System.Collections;               //Mod 2016/03/22 arc yno

/*-----------------------------------------------------------------------------------------------
 * 機能：部品発注一覧・部品発注入力画面に関するアクション
 * 更新履歴：
 *   2015/12/09 arc yano #3290 部品仕入機能改善(部品発注一覧)
 *                             検索条件・一覧の項目の変更
 *   2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 
 *                             部品発注データの管理方法の変更(1発注 = 1部品→1発注=複数部品)
 * ---------------------------------------------------------------------------------------------*/

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsPurchaseOrderController : InheritedController
    {
        //Add 2014/08/08 arc amii エラーログ対応 ログ出力の為に追加
        protected string FORM_NAME = "部品発注入力(純正)";                  // 画面名
        protected string PROC_NAME = "部品発注登録(純正)";                  // 処理名
        protected string PROC_NAME_SEARCH = "部品発注検索";                 // 処理名
        protected string PROC_NAME_EXCELDOWNLOAD = "Excel出力";             // 処理名

        //Add 2015/11/09 arc yano #3291
        //発注ステータス
        protected static readonly string STS_PURCHASEORDER_UNORDER = "001";     //未発注(発注ステータス)
        protected static readonly string STS_PURCHASEORDER_ORDERED = "002";     //発注済(発注ステータス)

        //純正区分
        protected string gGenuine;                                           
        
        //ビュー名
        protected string viewName = "PartsPurchaseOrderEntry";                  //デフォルトは純正品用の画面

        /// <summary>
        /// データコンテキスト
        /// </summary>
        protected CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsPurchaseOrderController()
        {
            db = new CrmsLinqDataContext();
            gGenuine = "001";                   //純正区分 =　「純正」
        }

        #region 検索機能
        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            //-------------------------
            //初期値の設定
            //-------------------------
            FormCollection form = new FormCollection();
            //部門
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;    //部門 = ログインユーザの所属部門
            form["PurchaseOrderStatus"] = STS_PURCHASEORDER_UNORDER;                    //発注ステータス=「未発注」
            form["GenuineType"] = gGenuine;                                          //純正区分=「純正」
            return Criteria(form);
        }

        /// <summary>
        /// 検索結果表示
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Add 2014/09/08 arc amii エラーログ対応 画面入力値をSysLogに出力する
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            PaginatedList<PartsPurchaseOrder> list = GetSearchResultList(form);

            //サービス伝票と紐付ける
            foreach (var item in list)
            {
                item.ServiceSalesHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(item.ServiceSlipNumber);
            }

            //画面項目の設定
            SetCriteriaComponent(form);

            return View("PartsPurchaseOrderCriteria", list);

        }

        /// <summary>
        /// 検索条件をセットして結果リストを返す
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>結果リスト</returns>
        private PaginatedList<PartsPurchaseOrder> GetSearchResultList(FormCollection form)
        {
            PartsPurchaseOrder condition = new PartsPurchaseOrder();
            
            condition = SetCriteriaCondition(form); //検索条件の設定

            return new PartsPurchaseOrderDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// 検索画面項目の設定
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// 2015/12/09 arc yano #3290 部品仕入機能改善(部品発注一覧) 新規作成
        /// </history>
        private void SetCriteriaComponent(FormCollection form)
        {
            //------------------
            //規定入力値の設定
            //------------------
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;
            ViewData["DefaultPurchaseOrderStatus"] = STS_PURCHASEORDER_UNORDER;
            ViewData["DefaultGenuineType"] = gGenuine;

            //------------------
            //検索条件の再設定
            //------------------
            ViewData["PurchaseOrderNumber"] = form["PurchaseOrderNumber"];      //発注番号
            ViewData["ServiceSlipNumber"] = form["ServiceSlipNumber"];          //受注伝票番号
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];  //発注日(From)
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];      //発注日(To)
            ViewData["DepartmentCode"] = form["DepartmentCode"];                //部門コード
            ViewData["EmployeeCode"] = form["EmployeeCode"];                    //担当者コード
            ViewData["SupplierCode"] = form["SupplierCode"];                    //仕入先コード
            ViewData["WebOrderNumber"] = form["WebOrderNumber"];                //Webオーダー番号
            ViewData["SupplierName"] = form["SupplierName"];                    //仕入先名

            CodeDao dao = new CodeDao(db);
            //発注ステータス
            ViewData["PurchaseOrderStatusList"] = CodeUtils.GetSelectListByModel<c_PurchaseOrderStatus>(dao.GetPurchaseOrderStatusAll(false), form["PurchaseOrderStatus"], true);
            //純正区分
            ViewData["GenuineTypeList"] = CodeUtils.GetSelectListByModel<c_GenuineType>(dao.GetGenuineTypeAll(false), form["GenuineType"], true);
            //担当者名
            if (!string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                ViewData["EmployeeName"] = employee != null ? employee.EmployeeName : "";
            }
            //部門名
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }
        }

        /// <summary>
        /// 検索条件の設定
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>検索条件</returns>
        /// <history>
        /// 2015/12/09 arc yano #3290 部品仕入機能改善(部品発注一覧) 新規作成
        /// </history>
        private PartsPurchaseOrder SetCriteriaCondition(FormCollection form)
        {

            PartsPurchaseOrder condition = new PartsPurchaseOrder();
            //発注番号
            condition.PurchaseOrderNumber = form["PurchaseOrderNumber"];
            //受注伝票番号
            condition.ServiceSlipNumber = form["ServiceSlipNumber"];            
            //発注日
            condition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            condition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            //発注ステータス
            condition.PurchaseOrderStatus = form["PurchaseOrderStatus"];
            //部門コード
            condition.DepartmentCode = form["DepartmentCode"];
            //担当者コード
            condition.EmployeeCode = form["EmployeeCode"];
            //仕入先コード
            condition.SupplierCode = form["SupplierCode"];
            //仕入先名
            condition.SupplierName = form["SupplierName"];
            //Webオーダー番号
            condition.WebOrderNumber = form["WebOrdernumber"];
            //純正区分
            condition.GenuineType = form["GenuineType"];

            condition.SetAuthCondition((Employee)Session["Employee"]);

            return condition;
        }
        #endregion

        #region 入力画面表示
        /// <summary>
        /// 入力画面表示
        /// </summary>
        /// <param name="param">発注情報</param>
        /// <param name="id">画面id</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応 FCJデフォルト仕入先の廃止
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        public ActionResult Entry(ParamOrder param)
        {
            List<PartsPurchaseOrder> orderList = new List<PartsPurchaseOrder>();         //発注リスト
            FormCollection form = new FormCollection();                                  //画面の入力値
            PartsPurchaseOrder orderView = new PartsPurchaseOrder();                     //画面表示用の値
            Parts parts = null;
            
            ModelState.Clear();

            //--------------------------------------------
            //純正品のデフォルトの仕入先を取得
            //-------------------------------------------
            //Del 2018/06/01 arc yano #3894
            ////Add 2018/01/15 arc yano #3833
            //if (gGenuine.Equals("001"))
            //{
            //    form["GenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode").Value;

            //    form["GenuineSupplierName"] = new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]).SupplierName : "";
            //}

            //発注番号が含まれている場合(作成済発注データの編集)は、部品発注テーブルを検索してデータを取得する
            if (!string.IsNullOrWhiteSpace(param.PurchaseOrderNumber))
            {
                orderList = new PartsPurchaseOrderDao(db).GetListByKey(param.PurchaseOrderNumber);

                orderView = orderList[0];
            }
            else if (param.partsList != null && param.partsList.Count > 0)
            {
                //共通項目の設定
                //受注伝票番号
                orderView.ServiceSlipNumber = param.ServiceSlipNumber;
                //部門コード
                //orderView.DepartmentCode = param.DepartmentCode;
                //オーダー区分
                orderView.OrderType = param.OrderType;
                //その他の項目の設定
                orderView = makePurchaseOrder(orderView, form);

                //個別項目(部品番号、発注数等)の設定
                foreach (var partsInfo in param.partsList)
                {
                    PartsPurchaseOrder order = new PartsPurchaseOrder();
                    
                    //部品番号
                    order.PartsNumber = partsInfo.PartsNumber;
                    //部品名称
                    parts = new PartsDao(db).GetByKey(partsInfo.PartsNumber ?? "");
                    
                    if (parts != null)
                    {
                        //部品名称
                        order.PartsNameJp = parts.PartsNameJp;
                        //定価
                        order.Price = parts.Price;
                        //原価
                        order.Cost = parts.Cost;
                    }
                    else
                    {
                        //部品名称
                        order.PartsNameJp = "";
                    }

                    //発注数量
                    order.Quantity = partsInfo.Quantity;
                    
                    //発注金額
                    order.Amount = (order.Quantity ?? 0) * (order.Cost ?? 0);

                    //その他の項目の設定
                    order = makePurchaseOrder(order, form);

                    orderList.Add(order);
                }
            }
            else
            {
                PartsPurchaseOrder order = new PartsPurchaseOrder();
                // 支払予定日はデフォルトで翌月末日
                order.PaymentPlanDate = CommonUtils.GetFinalDay(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month);
                order.ArrivalPlanDate = DateTime.Today.AddDays(1);
                orderList.Add(order);

                //その他の項目の設定
                orderView = makePurchaseOrder(orderView, form);
            }

            SetDataComponent(form, orderView);

            return View(viewName, orderList);
        }

        /// <summary>
        /// 発注データ作成(共通部分)
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応 FCJのデフォルト設定処理を廃止
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        private PartsPurchaseOrder makePurchaseOrder(PartsPurchaseOrder order, FormCollection form)
        {
            //発注ステータス(未発注)
            order.PurchaseOrderStatus = STS_PURCHASEORDER_UNORDER;
            order.c_PurchaseOrderStatus = new CodeDao(db).GetPurchaseOrderStatus(false, order.PurchaseOrderStatus);

            // 支払予定日はデフォルトで翌月末日
            order.PaymentPlanDate = CommonUtils.GetFinalDay(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month);
            order.ArrivalPlanDate = DateTime.Today.AddDays(1);

            //部門コード(ログインユーザの所属部門をデフォルト値として設定)
            order.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;

            //担当者
            Employee emp = (Employee)Session["Employee"];
            order.EmployeeCode = emp.EmployeeCode;
            //発注日
            order.PurchaseOrderDate = DateTime.Today;

            //純正区分=「純正」の場合はデフォルト仕入先を設定する
            if (gGenuine.Equals("001"))
            {
                // デフォルト仕入先をセット
                if (!string.IsNullOrWhiteSpace(order.DepartmentCode))
                {
                    //その部門の最新の入荷実績を元に仕入先を設定する
                    List<PartsPurchase> list = new PartsPurchaseDao(db).GetPurchaseResult(order.DepartmentCode, gGenuine);

                    //その部品の仕入実績があった場合
                    if (list.Count > 0)
                    {
                        //仕入先コード
                        order.SupplierCode = list.OrderByDescending(x => x.PurchaseDate).FirstOrDefault().SupplierCode;

                        //仕入名の設定
                        if (order.SupplierCode != null)
                        {
                            order.SupplierName = new SupplierDao(db).GetByKey(order.SupplierCode) != null ? new SupplierDao(db).GetByKey(order.SupplierCode).SupplierName : "";
                        }
                    }
                    //Del 2018/06/01 arc yano #3894
                    //else //実績が無い場合はシステムで登録しているデフォルト仕入先を設定   //Add 2018/01/15 arc yano #3833
                    //{
                    //    order.SupplierCode = form["GenuineSupplierCode"];
                    //    order.SupplierName = form["GenuineSupplierName"];
                    //}
                }
            }

            //仕入先が設定されている場合は支払先を取得
            if (!string.IsNullOrWhiteSpace(order.SupplierCode))
            {
                //支払先の設定
                SupplierPayment supplierPayment = new SupplierPaymentDao(db).GetByKey(order.SupplierCode);

                if (supplierPayment != null)
                {
                    order.SupplierPaymentCode = supplierPayment.SupplierPaymentCode;
                    order.SupplierPaymentName = supplierPayment.SupplierPaymentName;
                }
            }

            return order;
        }
        #endregion
        #region 登録機能
        /// <summary>
        /// 部品発注登録処理
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <param name="line">部品発注データ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3806 部品発注入力　１０行追加ボタンの追加
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form, List<PartsPurchaseOrder> line)
        {
            List<PartsPurchaseOrder> delList = null;
            List<PartsPurchaseOrder> list = null;
            
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            ModelState.Clear();

            //Validationチェック
            ValidateSave(form, line);

            if (!ModelState.IsValid)
            {
                form["OrderFlag"] = "0";
                SetDataComponent(form);
                return View(viewName, line);
            }

            //フォームの入力値(共通部分)を元に発注データに入力
            line = SetOrderList(line, form);

            string procName = "";
            
            procName = GetSysLogProcName(form);
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, procName);

            PaymentPlan plan = new PaymentPlan();

            using (TransactionScope ts = new TransactionScope())
            {
                int cnt = 0;
                int i = 0;
                
                //--------------------------------
                //発注データ更新処理(削除処理)
                //--------------------------------
                //DelFlag = '1'　のものを削除対象に
                delList = line.Where(x => (!string.IsNullOrWhiteSpace(x.DelFlag) && x.DelFlag.Equals("1"))).ToList();

                //Mod 2016/04/26 arc yano #3511
                //データベースにあって、入力値にないものは削除
                List<PartsPurchaseOrder> dList = new PartsPurchaseOrderDao(db).GetListByKey(form["PurchaseOrderNumber"]);

                if (dList != null && dList.Count > 0)
                {                    
                    foreach (var p in dList)
                    {
                        //データベースの部品番号、発注番号で入力データを検索する
                        var rec = line.Where(x => x.PurchaseOrderNumber.Equals(p.PurchaseOrderNumber) && x.PartsNumber.Equals(p.PartsNumber));

                        //データベースのレコードが入力値に無い場合は削除
                        if (rec == null || rec.ToList().Count == 0)
                        {
                            p.DelFlag = "1";        //削除フラグを立てる
                            delList.Add(p);
                        }
                    }
                }

                
                if (delList !=null && delList.Count > 0)
                {
                    DelOrder(delList);
                }
                
                //--------------------------------
                //発注データ更新処理(追加・更新)
                //--------------------------------
                list = line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0")) && !string.IsNullOrWhiteSpace(x.PartsNumber)).ToList();  //Mod 2017/11/07 arc yano #3806
                //list = line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).ToList(); 
                for (i = 0; i < list.Count; i++)
                {
                    PartsPurchaseOrder l = list[i];

                    //発注処理の場合は、発注済ステータスの更新と、支払予定データの作成を行う
                    if (form["OrderFlag"].Equals("1"))
                    {
                        //発注処理時のValidationチェック
                        ValidatePurchaseOrder(form, l, cnt);
                        
                        if (!ModelState.IsValid)
                        {
                            form["OrderFlag"] = "0";
                            SetDataComponent(form);
                            return View(viewName, line);
                        }
                      
                        //----------------------------
                        //発注データの編集
                        //----------------------------
                        l.PurchaseOrderStatus = STS_PURCHASEORDER_ORDERED;  //発注済みステータス
                        l.RemainingQuantity = l.Quantity;                   //発注残数

                        //----------------------------
                        //支払予定データの更新
                        //---------------------------- 
                        plan = CreatePaymentPlan(l, plan);

                        if (!string.IsNullOrWhiteSpace(l.ServiceSlipNumber))
                        {
                            //----------------------------
                            //サービス伝票発注数の更新
                            //---------------------------- 
                            updateOrderQuantity(l);
                        }
                    }

                    //------------------------------
                    //発注データのDB登録
                    //------------------------------
                    PartsPurchaseOrder order = (new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber) ?? new PartsPurchaseOrder());
                    order = SetOrder(l, order);
                                        
                    cnt++;                                  //カウントインクリメント
                }

                //最後に支払予定をinsert
                if (plan != null && !string.IsNullOrWhiteSpace(plan.PurchaseOrderNumber))
                {
                    db.PaymentPlan.InsertOnSubmit(plan);
                }
                try
                {
                    db.SubmitChanges();
                    //コミット
                    ts.Complete();
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
                        form["OrderFlag"] = "0";
                        SetDataComponent(form);
                        return View(viewName, line);
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
                    //Add 2014/08/04 arc amii エラーログ対応 SqlException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }

            if (form["OrderFlag"].Equals("1"))
            {
                ModelState.Clear();
                ModelState.AddModelError("", MessageUtils.GetMessage("I0003", "発注処理"));
                SetDataComponent(form, line[0]);
                return View(viewName, list);
            }
            else
            {
                ModelState.Clear();
                ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
                SetDataComponent(form);
                return View(viewName, list);
            }
        }

        /// <summary>
        /// 部品発注データの編集
        /// </summary>
        /// <param name="orderList">部品発注データ</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加</history>
        [AcceptVerbs(HttpVerbs.Post)]
        protected virtual List<PartsPurchaseOrder> SetOrderList(List<PartsPurchaseOrder> orderList, FormCollection form)
        {
            //共通部分をフォームの値よりセット

            //発注番号が空文字、またはnullの場合は新規採番する。
            if (string.IsNullOrWhiteSpace(form["PurchaseOrderNumber"]))
            {
                form["PurchaseOrderNumber"] = new SerialNumberDao(db).GetNewPartsPurchaseOrderNumber();
            }

            if(orderList != null && orderList.Count > 0)
            {
                for(int i=0; i < orderList.Count; i++)
                {
                    //受注伝票番号
                    orderList[i].ServiceSlipNumber = form["ServiceSlipNumber"];
                    //発注日
                    orderList[i].PurchaseOrderDate = !string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["PurchaseOrderDate"].ToString()) : null;
                    //発注ステータス
                    orderList[i].PurchaseOrderStatus = form["PurchaseOrderStatus"];
                    //部門コード
                    orderList[i].DepartmentCode = form["DepartmentCode"];
                    //担当者コード
                    orderList[i].EmployeeCode = form["EmployeeCode"];
                    //発注番号
                    orderList[i].PurchaseOrderNumber = form["PurchaseOrderNumber"];
                    //仕入先コード
                    orderList[i].SupplierCode = form["SupplierCode"];
                    //支払先コード
                    orderList[i].SupplierPaymentCode = form["SupplierPaymentCode"];
                    //Webオーダー番号
                    orderList[i].WebOrderNumber = form["WebOrderNumber"];
                    //入荷予定日
                    orderList[i].ArrivalPlanDate = !string.IsNullOrWhiteSpace(form["ArrivalPlanDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["ArrivalPlanDate"].ToString()) : null;
                    //支払予定日
                    orderList[i].PaymentPlanDate = !string.IsNullOrWhiteSpace(form["PaymentPlanDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["PaymentPlanDate"].ToString()) : null;
                    //オーダー区分
                    if (!string.IsNullOrWhiteSpace(form["OrderType"]))
                    {
                        orderList[i].OrderType = form["OrderType"];
                    }
                    else
                    {
                        orderList[i].OrderType = form["HdOrderType"];
                    }
                    
                    if (!string.IsNullOrWhiteSpace(orderList[i].SupplierCode))
                    {
                        //仕入先の設定
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierName))
                        {
                            orderList[i].SupplierName = new SupplierDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierDao(db).GetByKey(orderList[i].SupplierCode).SupplierName : "";
                        }
                        //支払先の設定
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierPaymentName))
                        {
                            orderList[i].SupplierPaymentName = new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode).SupplierPaymentName : "";
                        }
                    }
                }
            }

            return orderList;
        }

        /// <summary>
        /// 発注データ削除処理
        /// </summary>
        /// <param name="orderList">部品発注データ(画面の入力値)</param>
        /// <returns></returns>
        /// <history>
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private void DelOrder(List<PartsPurchaseOrder> list)
        {
            //リスト分ループ
            foreach(var l in list )
            {
                if (!string.IsNullOrWhiteSpace(l.PurchaseOrderNumber) && !string.IsNullOrWhiteSpace(l.PartsNumber))
                {
                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber);

                    //発注データ（画面の入力内容）がDBに登録されており、なおかつ削除済データの場合、復活させる
                    if (order != null)
                    {
                        order.DelFlag = l.DelFlag;
                    }
                }
            }
        }



        /// <summary>
        /// 部品発注データの編集(Excel用追加処理)
        /// </summary>
        /// <param name="orderList">部品発注データ</param>
        /// <param name="form">フォームの入力値</param>
        /// <returns></returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        protected virtual List<PartsPurchaseOrder> SetOrderListForExcel(List<PartsPurchaseOrder> orderList, FormCollection form)
        {
            if (orderList != null && orderList.Count > 0)
            {
                for (int i = 0; i < orderList.Count; i++)
                {
                    //-----------------------
                    //Excel出力用に設定する
                    //-----------------------
                    //担当者名
                    if (!string.IsNullOrWhiteSpace(orderList[i].EmployeeCode))
                    {
                        orderList[i].EmployeeName = new EmployeeDao(db).GetByKey(orderList[i].EmployeeCode) != null ? new EmployeeDao(db).GetByKey(orderList[i].EmployeeCode).EmployeeName : "";
                    }
                    //車台番号
                    if (!string.IsNullOrWhiteSpace(orderList[i].ServiceSlipNumber))
                    {
                        orderList[i].SalesCarNumber = new ServiceSalesOrderDao(db).GetBySlipNumber(orderList[i].ServiceSlipNumber) != null ? new ServiceSalesOrderDao(db).GetBySlipNumber(orderList[i].ServiceSlipNumber).SalesCarNumber : "";
                    }
                    //部門名
                    if (!string.IsNullOrWhiteSpace(orderList[i].DepartmentCode))
                    {
                        orderList[i].DepartmentName = new DepartmentDao(db).GetByKey(orderList[i].DepartmentCode) != null ? new DepartmentDao(db).GetByKey(orderList[i].DepartmentCode).DepartmentName : "";
                    }
                    //オーダー区分名
                    if (!string.IsNullOrWhiteSpace(orderList[i].OrderType))
                    {
                        orderList[i].OrderTypeName = new CodeDao(db).GetOrderType(orderList[i].OrderType) != null ? new CodeDao(db).GetOrderType(orderList[i].OrderType).Name : "";
                    }

                    //部品番号(WPH内の部品番号→メーカ内の部品番号へ変換)
                    //オーダー区分名
                    if (!string.IsNullOrWhiteSpace(orderList[i].PartsNumber))
                    {
                        orderList[i].MakerPartsNumber = new PartsDao(db).GetByKey(orderList[i].PartsNumber) != null ? new PartsDao(db).GetByKey(orderList[i].PartsNumber).MakerPartsNumber : "";
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// サービス伝票の発注数の更新
        /// </summary>
        /// <param name="line">部品発注データ（１行分）</param>
        /// <param name="workLines">サービス伝票(発注数未更新)</param>
        /// <returns>サービス伝票(発注数更新)</returns>
        /// <history>
        /// 2018/02/24 arc yano #3831 サービス伝票入力　部品発注→部品入荷後の再発注時の発注数の不具合
        /// 2016/02/01 arc yano #3419 部品発注入力　DelFlagが空文字で登録される不具合の対応
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private void updateOrderQuantity(PartsPurchaseOrder l)
        {
            //受注伝票番号、部品番号、判断からサービス伝票の明細行の更新
            List<ServiceSalesLine> workLines = new ServiceSalesOrderDao(db).GetLineByPartsNumber(l.ServiceSlipNumber, l.PartsNumber, l.OrderType);

            if (workLines.Count > 0)
            {
                decimal? orderQuantity = l.Quantity;

                //Mod 2018/02/24 arc yano #3831
                for (int k = 0; k < workLines.Count; k++)
                {
                    //未発注数を算出
                    decimal? notordering = (workLines[k].Quantity ?? 0m) - (workLines[k].OrderQuantity ?? 0m);

                    //未発注数量が1以上の場合
                    if (notordering > 0)
                    {
                        //発注数がサービス伝票の販売数以上の場合は販売数を設定
                        if (orderQuantity >= notordering)
                        {
                            workLines[k].OrderQuantity += notordering;
                            orderQuantity -= notordering;
                        }
                        else
                        {
                            workLines[k].OrderQuantity += orderQuantity;
                            orderQuantity = 0;
                            break;
                        }
                    }

                    //明細最終行の場合は発注数を全て加算
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity += orderQuantity;
                        orderQuantity = 0;
                        break;
                    }
                }

                /*
                for (int k = 0; k < workLines.Count; k++)
                {
                    //明細最終行の場合は発注数を全て加算
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity += orderQuantity;
                        orderQuantity = 0;
                        break;
                    }
                    else //最終行以外は、分割して更新する
                    {
                        //発注数がサービス伝票の販売数以上の場合は販売数を設定
                        if (orderQuantity >= workLines[k].Quantity)
                        {
                            workLines[k].OrderQuantity += workLines[k].Quantity;
                            orderQuantity -= workLines[k].Quantity;
                        }
                        else
                        {
                            workLines[k].OrderQuantity += orderQuantity;
                            orderQuantity = 0;
                            break;
                        }
                    }
                }
                */
            }

            return;
        }
        /// <summary>
        /// サービス伝票の発注数の取消
        /// </summary>
        /// <param name="l">部品発注データ（１行分）</param>
        /// <param name="workLines">サービス伝票(発注数未更新)</param>
        /// <returns>サービス伝票(発注数更新)</returns>
        /// <history>
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private List<ServiceSalesLine> cancelOrderQuantity(PartsPurchaseOrder l, List<ServiceSalesLine> workLines)
        {
            //受注伝票番号、部品番号、判断からサービス伝票の明細行の更新
            workLines = new ServiceSalesOrderDao(db).GetLineByPartsNumber(l.ServiceSlipNumber, l.PartsNumber, l.OrderType);

            if (workLines.Count > 0)
            {
                decimal? orderQuantity = l.Quantity;

                for (int k = 0; k < workLines.Count; k++)
                {
                    //明細最終行の場合は発注数を全てセット
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity -= orderQuantity;
                        orderQuantity -= orderQuantity;
                        break;
                    }
                    else //最終行以外は、分割して更新する
                    {
                        //発注数がサービス伝票の販売数以上の場合は販売数を設定
                        if (orderQuantity >= workLines[k].OrderQuantity)
                        {
                            orderQuantity -= workLines[k].OrderQuantity;
                            workLines[k].OrderQuantity -= workLines[k].OrderQuantity;
                        }
                        else
                        {
                            workLines[k].OrderQuantity -= orderQuantity;
                            orderQuantity -= orderQuantity;
                            break;
                        }
                    }
                }
            }

            return workLines;
        }

        /// <summary>
        /// 部品発注データの設定
        /// </summary>
        /// <param name="orderSource">発注データ(編集元)</param>
        /// <param name="orderDst">発注データ(編集先)</param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/02/01 arc yano #3419 部品発注入力　DelFlagが空文字で登録される不具合の対応 画面項目のDelFlagを更新する処理を追加
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private PartsPurchaseOrder SetOrder(PartsPurchaseOrder orderSrc , PartsPurchaseOrder orderDst)
        {
            //新規作成・更新
            orderDst.ServiceSlipNumber = orderSrc.ServiceSlipNumber;                                //受注伝票番号
            orderDst.SupplierCode = orderSrc.SupplierCode;                                          //仕入先コード
            orderDst.SupplierPaymentCode = orderSrc.SupplierPaymentCode;                            //支払先コード
            orderDst.EmployeeCode = orderSrc.EmployeeCode;                                          //社員コード
            orderDst.DepartmentCode = orderSrc.DepartmentCode;                                      //部門コード
            orderDst.WebOrderNumber = orderSrc.WebOrderNumber;                                      //Webオーダー番号
            orderDst.PurchaseOrderDate = orderSrc.PurchaseOrderDate;                                //発注日
            orderDst.PurchaseOrderStatus = orderSrc.PurchaseOrderStatus;                            //発注ステータス
            orderDst.PartsNumber = orderSrc.PartsNumber;                                            //部品番号
            orderDst.OrderType = orderSrc.OrderType;                                                //オーダー区分
            orderDst.Quantity = orderSrc.Quantity;                                                  //数量
            orderDst.Cost = orderSrc.Cost;                                                          //原価
            orderDst.Price = orderSrc.Price;                                                        //価格
            orderDst.Amount = orderSrc.Amount;                                                      //金額
            orderDst.ArrivalPlanDate = orderSrc.ArrivalPlanDate;                                    //入荷予定日
            orderDst.PaymentPlanDate = orderSrc.PaymentPlanDate;                                    //支払予定日
            orderDst.Memo = orderSrc.Memo;                                                          //備考欄
            orderDst.RemainingQuantity = orderSrc.RemainingQuantity;                                //発注残数
            orderDst.LastUpdateDate = DateTime.Now;                                                 //最終更新日
            orderDst.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;         //最終更新者
            orderDst.DelFlag = orderSrc.DelFlag;                                                    //削除フラグ       

            if (orderDst.PurchaseOrderNumber == null)
            {
                orderDst.PurchaseOrderNumber = orderSrc.PurchaseOrderNumber;                        //発注番号                      
                orderDst.CreateDate = DateTime.Now;                                                 //作成日時
                orderDst.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;         //作成者
                orderDst.DelFlag = orderSrc.DelFlag = "0";                                          //削除フラグ Mod 2016/02/01 arc yano #3419
                db.PartsPurchaseOrder.InsertOnSubmit(orderDst);
            }

            return orderSrc;
        }

        #endregion
        #region 登録取消機能
        /// <summary>
        /// 発注取消
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <param name="line">明細情報</param>
        /// <returns>画面</returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 新規作成
        /// </history>
        public ActionResult OrderCancel(FormCollection form, List<PartsPurchaseOrder> line)
        {
            List<ServiceSalesLine> workLines = new List<ServiceSalesLine>();
            PaymentPlan plan = new PaymentPlan();

            //発注データの再設定
            line = SetOrderList(line, form);

            //発注取消処理
            using (TransactionScope ts = new TransactionScope())
            {
                //明細部の行数分追加・更新を行う
                foreach (var l in line)
                {
                    //----------------------------
                    //発注データの削除
                    //----------------------------
                    
                    //発注データを取得
                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber);

                    order.DelFlag = "1";
                    //----------------------------
                    //支払予定データの更新
                    //---------------------------- 
                    plan = DeletePaymentPlan(order, plan);

                    //----------------------------
                    //サービス伝票発注数の更新
                    //---------------------------- 
                    if (!string.IsNullOrWhiteSpace(order.ServiceSlipNumber))
                    {
                        workLines = cancelOrderQuantity(order, workLines);
                    }
                }
                try
                {
                    db.SubmitChanges();
                    //コミット
                    ts.Complete();
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
                        SetDataComponent(form);
                        return View(viewName, line);
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
                    //Add 2014/08/04 arc amii エラーログ対応 SqlException以外の時のエラー処理追加
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
                    return View("Error");
                }
            }

            //画面項目の設定
            SetDataComponent(form);

            //取消フラグを設定
            ViewData["CancelFlag"] = "1";

            ModelState.AddModelError("", "発注を取り消しました");

            return View(viewName, line);
        }
        #endregion
        #region Excel出力機能
        /// <summary>
        /// Excelファイルのダウンロード
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="line">発注データ明細</param> 
        /// <returns>Excelデータ</returns>
        ///<history>
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        public virtual ActionResult Download(FormCollection form, List<PartsPurchaseOrder> line)
        {
            //-------------------------------
            //初期処理
            //-------------------------------  
            // Infoログ出力
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //-------------------------------
            //Excel出力処理
            //-------------------------------         
            //ファイル名(PartsPurchaseOrder_xxx(発注番号)_yyyyMMddhhmiss(ダウンロード時刻))
            string fileName = "PartsPurchaseOrder" + "_" + form["PurchaseOrderNumber"] + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExportPartPurchaseOrder"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExportPartPurchaseOrder"];

            string filePathName = filePath + fileName;

            //テンプレートファイルパス取得
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrder"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrder"];

            //テンプレートファイルのパスが設定されていない場合
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");
                SetDataComponent(form);
                return View(viewName, line);
            }

            line = SetOrderList(line, form);

            line = SetOrderListForExcel(line, form);    //Excel出力用

            //エクセルデータ作成
            byte[] excelData = MakeExcelData(form, line, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View(viewName, line);
            }

            //コンテンツタイプの設定
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// エクセルデータ作成(テンプレートファイルあり)
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="fileName">帳票名</param>
        /// <param name="tfileName">帳票テンプレート</param>
        /// <returns>エクセルデータ</returns>
        protected byte[] MakeExcelData(FormCollection form, List<PartsPurchaseOrder> line, string fileName, string tfileName)
        {
            //----------------------------
            //初期処理
            //----------------------------
            ConfigLine configLine;                   //設定値
            byte[] excelData = null;                 //エクセルデータ
            bool ret = false;
            bool tFileExists = true;                 //テンプレートファイルあり／なし(実際にあるかどうか)


            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン(テンプレートファイルあり)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //テンプレートファイルが無かった場合
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "テンプレートファイルが見つかりませんでした。");
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

            //------------------------------
            //出力データ整形
            //------------------------------
            //明細情報
            line = SetOrderList(line, form);
            //ヘッダ情報
            List<PartsPurchaseOrder> hList = new List<PartsPurchaseOrder>();
            hList.Add(line[0]);

            //----------------------------
            //明細情報シート出力
            //----------------------------
            //データ設定
            ret = dExport.SetData<PartsPurchaseOrder, InfoPartsExcel>(ref excelFile, line, configLine);
            //----------------------------
            //ヘッダ情報シート出力
            //----------------------------
            configLine = dExport.GetConfigLine(config, 3);

            //データ設定
            ret = dExport.SetData<PartsPurchaseOrder, PartsPurchaseOrderHeader>(ref excelFile, hList, configLine);

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

        #region 支払予定
        /// <summary>
        /// 支払予定データ作成(発注番号毎に作成する)
        /// </summary>
        /// <param name="order">部品発注データ</param>
        private PaymentPlan CreatePaymentPlan(PartsPurchaseOrder order, PaymentPlan plan)
        {
            PaymentPlan ret = null;

            //支払予定(既存)の発注番号と発注情報の発注番号が同じ場合
            if (plan.PurchaseOrderNumber == order.PurchaseOrderNumber)
            {
                plan.Amount += order.Amount;                //発注情報の金額を上乗せ
                plan.PaymentableBalance += order.Amount;    //発注情報の支払残高を上乗せ
                ret = plan;
            }
            else
            {
                //支払情報(既存)をinsert
                if (!string.IsNullOrWhiteSpace(plan.PurchaseOrderNumber))
                {
                    db.PaymentPlan.InsertOnSubmit(plan);        
                }
                
                //新規に支払情報を作成する
                Account account = new AccountDao(db).GetByUsageType("SP");
                ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("PaymentableDepartmentCode");

                PaymentPlan newplan = new PaymentPlan();
                newplan.AccountCode = account.AccountCode;
                newplan.Amount = order.Amount;
                newplan.CompleteFlag = "0";
                newplan.CreateDate = DateTime.Now;
                newplan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                newplan.DelFlag = "0";
                newplan.DepartmentCode = config.Value;
                newplan.LastUpdateDate = DateTime.Now;
                newplan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                newplan.OccurredDepartmentCode = order.DepartmentCode;
                newplan.PaymentableBalance = order.Amount;
                newplan.PaymentPlanDate = order.PaymentPlanDate;
                newplan.PaymentPlanId = Guid.NewGuid();
                newplan.SlipNumber = order.ServiceSlipNumber;
                newplan.SupplierPaymentCode = order.SupplierPaymentCode;
                newplan.PurchaseOrderNumber = order.PurchaseOrderNumber;

                ret = newplan;
            }

            return ret;
        }
        /// <summary>
        /// 支払予定データ削除
        /// </summary>
        /// <param name="order">部品発注データ</param>
        private PaymentPlan DeletePaymentPlan(PartsPurchaseOrder order, PaymentPlan plan)
        {
            PaymentPlan ret = plan;
            PaymentPlan condition = new PaymentPlan();

            //既存の支払情報と発注データの支払予定の発注伝票が異なる場合
            if (plan.PurchaseOrderNumber != order.PurchaseOrderNumber)
            {
                var rec = new PaymentPlanDao(db).GetByPuchaseOrderNumber(order.PurchaseOrderNumber);

                if (rec != null)
                {
                    rec.DelFlag = "1";      //削除フラグON
                }

                ret = rec;
            }

            return ret; 
        }

        #endregion

        #region ロケーション取得
        /// <summary>
        /// 引当ロケーション(ロケーション種別:002)を取得する
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private string GetHikiateLocation(string departmentCode)
        {
            List<Location> hikiateLocation = new LocationDao(db).GetListByLocationType("002", departmentCode, null);
            if (hikiateLocation == null || hikiateLocation.Count == 0)
            {
                ModelState.AddModelError("", "部門内に引当ロケーションが1つも設定されていません");
                return string.Empty;
            }
            else
            {
                return hikiateLocation[0].LocationCode;
            }
        }
        #endregion

        #region 画面コンポーネントの設定
        /// <summary>
        /// データ付き画面コンポーネントを作成
        /// </summary>
        /// <param name="form">フォーム</param>
        /// <param name="dSource">部品発注データ</param>
        /// <history>
        /// 2018/09/21 yano #3941 部品入荷　受注伝票に同一発注種別、かつ同一部品番号の明細が複数存在する状態で、発注→入荷を実行するとシステムエラー
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応 FCJのデフォルト設定を廃止
        /// 2018/01/15 arc yano #3833 部品発注入力・部品入荷入力　仕入先固定セット
        /// 2016/01/26 arc yano #3399 部品発注画面の[オーダー区分]の非活性化(#3397_【大項目】部品仕入機能改善 課題管理表対応)
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 画面の各項目のデータソースをmodelからviewdataに変更
        /// </history>
        protected void SetDataComponent(FormCollection form, PartsPurchaseOrder dSource = null)
        {

            CodeDao dao = new CodeDao(db);

            //----------------------------------
            //フォームの値の設定
            //----------------------------------
            if (dSource != null)
            {
                //発注番号
                form["PurchaseOrderNumber"] = dSource.PurchaseOrderNumber;
                //受注伝票番号
                form["ServiceSlipNumber"] = dSource.ServiceSlipNumber;
                //発注日
                form["PurchaseOrderDate"] = string.Format("{0:yyyy/MM/dd}", dSource.PurchaseOrderDate);
                //発注ステータス
                form["PurchaseOrderStatus"] = dSource.PurchaseOrderStatus;
                //部門コード
                form["DepartmentCode"] = dSource.DepartmentCode;
                //担当者コード
                form["EmployeeCode"] = dSource.EmployeeCode;
                //仕入先コード
                form["SupplierCode"] = dSource.SupplierCode;
                //支払先コード
                form["SupplierPaymentCode"] = dSource.SupplierPaymentCode;
                //Webオーダー番号
                form["WebOrderNumber"] = dSource.WebOrderNumber;
                //入荷予定日
                form["ArrivalPlanDate"] = string.Format("{0:yyyy/MM/dd}", dSource.ArrivalPlanDate);
                //支払予定日
                form["PaymentPlanDate"] = string.Format("{0:yyyy/MM/dd}", dSource.PaymentPlanDate);
                //オーダー区分
                form["OrderType"] = dSource.OrderType;
            }

            //--------------------------------
            //ヘッダ部分(共通項目)の設定
            //--------------------------------
            //発注番号
            ViewData["PurchaseOrderNumber"] = form["PurchaseOrderNumber"];
            //受注伝票番号
            ViewData["ServiceSlipNumber"] = form["ServiceSlipNumber"];
            //発注日
            ViewData["PurchaseOrderDate"] = form["PurchaseOrderDate"];
            //発注ステータス
            ViewData["PurchaseOrderStatus"] = form["PurchaseOrderStatus"];
            //発注ステータス名
            c_PurchaseOrderStatus status = dao.GetPurchaseOrderStatus(false, ViewData["PurchaseOrderStatus"] != null ? ViewData["PurchaseOrderStatus"].ToString() : "");
            if (status != null)
            {
                ViewData["PurchaseOrderStatusName"] = status.Name;
            }
            //部門コード
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            //部門名
            Department dep = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"] != null ? ViewData["DepartmentCode"].ToString() : "");
            if (dep != null)
            {
                ViewData["DepartmentName"] = dep.DepartmentName;
            }
            //担当者コード
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            //担当者
            Employee emp = new EmployeeDao(db).GetByKey(ViewData["EmployeeCode"] != null ? ViewData["EmployeeCode"].ToString() : "");
            if (emp != null)
            {
                ViewData["EmployeeNumber"] = emp.EmployeeNumber;
                ViewData["EmployeeName"] = emp.EmployeeName;
            }

             //Del 2018/06/01 arc yano #3894
            ////仕入先コード
            ////Add 2018/01/15 arc yano #3833 純正品で仕入先が空欄の場合
            //if (gGenuine.Equals("001") && string.IsNullOrWhiteSpace(form["SupplierCode"]))
            //{
            //    form["SupplierCode"] = form["GenuineSupplierCode"];
            //}
            
            ViewData["SupplierCode"] = form["SupplierCode"];

            //仕入先
            Supplier supplier = new SupplierDao(db).GetByKey(ViewData["SupplierCode"] != null ? ViewData["SupplierCode"].ToString() : "");
            if (supplier != null)
            {
                ViewData["SupplierName"] = supplier.SupplierName;
            }

            //支払先コード
            //Del 2018/06/01 arc yano #3894
            //Add 2018/01/15 arc yano #3833 純正品で仕入先が空欄の場合
            //if (gGenuine.Equals("001") && string.IsNullOrWhiteSpace(form["SupplierPaymentCode"]))
            //{
            //    form["SupplierPaymentCode"] = form["GenuineSupplierCode"];
            //}

            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            //支払先
            SupplierPayment payment = new SupplierPaymentDao(db).GetByKey(ViewData["SupplierPaymentCode"] != null ? ViewData["SupplierPaymentCode"].ToString() : null);
            if (payment != null)
            {
                ViewData["SupplierPaymentName"] = payment.SupplierPaymentName;
            }
            //Webオーダー番号
            ViewData["WebOrderNumber"] = form["WebOrderNumber"];
            //入荷予定日
            ViewData["ArrivalPlanDate"] = form["ArrivalPlanDate"];
            //支払予定日
            ViewData["PaymentPlanDate"] = form["PaymentPlanDate"];
            //オーダー区分
            ViewData["OrderTypeList"] = CodeUtils.GetSelectListByModel<c_OrderType>(dao.GetOrderTypeAll(false), (!string.IsNullOrWhiteSpace(form["OrderType"]) ? form["OrderType"] : form["HdOrderType"]), false);  //Mod 2018/09/21 yano #3941
            //発注処理フラグ
            ViewData["OrderFlag"] = form["OrderFlag"];

            ////Del 2018/06/01 arc yano #3894
            ////Add 2018/01/15 arc yano #3833
            //if (gGenuine.Equals("001"))
            //{
            //    //純正品のデフォルト仕入先コード
            //    ViewData["GenuineSupplierCode"] = form["GenuineSupplierCode"];
            //    //純正品のデフォルト仕入先名
            //    ViewData["GenuineSupplierName"] = form["GenuineSupplierName"];
            //}
            
            
            //Add 2016/01/26 arc yano
            //受注伝票番号がnullまたは空文字でない場合はオーダー区分を非活性
            if (!string.IsNullOrWhiteSpace(form["ServiceSlipNumber"]))
            {
                ViewData["OrderTypeEdit"] = "0";
            }
            else
            {
                ViewData["OrderTypeEdit"] = "1";
            }
        }
        #endregion

        #region Validationチェック
        /// <summary>
        /// Validatonチェック
        /// </summary>
        /// <param name="order">部品発注データ</param>
        /// <history>
        /// 2017/11/07 arc yano #3806 部品発注入力　１０行追加ボタンの追加
        /// 2016/04/26 arc yano #3511 部品発注入力　validationチェック見直し 有効な行のみvalidationチェックを行うように修正
        /// </history>
        private void ValidateSave(FormCollection form, List<PartsPurchaseOrder> line)
        {
            //----------------------------
            // ヘッダ項目の検証
            //----------------------------
            //必須チェック
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            //----------------------------
            // 明細項目の検証
            //----------------------------
            //明細行無し、または有効な明細行なし
            if (line == null || line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() == 0)
            {
                ModelState.AddModelError("", "発注リストは１行以上ある必要があります");
                return;
            }
            for (int i = 0; i < line.Count; i++)
            {
                string prefix = string.Format("line[{0}].", i);

                if (string.IsNullOrEmpty(line[i].DelFlag) || !line[i].DelFlag.Equals("1")) //Mod 2016/04/26 arc yano #3511
                {
                    //部品番号必須チェック
                    if (string.IsNullOrWhiteSpace(line[i].PartsNumber))
                    {
                        //ModelState.AddModelError(prefix + "PartsNumber", MessageUtils.GetMessage("E0001", "部品番号"));
                        //return;
                       //処理スキップ
                       continue;  //2017/11/07 arc yano #3806
                    }
                    else
                    {
                        if (gGenuine.Equals("001"))
                        {
                            //部品番号の重複チェック ※　純正区分 = 「純正」の場合のみチェックする
                            if (line.Where(x => ((x.PartsNumber ?? "").Equals(line[i].PartsNumber)) && (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() > 1)
                            {
                                ModelState.AddModelError(prefix + "PartsNumber", "同一部品番号は複数行に登録できません。部品番号：" + line[i].PartsNumber);
                            }
                        }
                    }
                    //数量
                    if (line[i].Quantity == null || line[i].Quantity <= 0)
                    {
                        ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" })); ;
                    }
                    else
                    {
                        if (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                                || (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}$")))
                        {
                        }
                        else
                        {
                            ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" }));
                        }
                    }
                }
            }
        }
        /*
        {
            //----------------------------
            // ヘッダ項目の検証
            //----------------------------
            //必須チェック
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "担当者"));
            }
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門"));
            }
            //----------------------------
            // 明細項目の検証
            //----------------------------
            //明細行無し、または有効な明細行なし
            if(line == null || line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() == 0)
            {
                ModelState.AddModelError("", "発注リストは１行以上ある必要があります");
                return;
            }
            for(int i = 0; i < line.Count; i++)
            {
                string prefix = string.Format("line[{0}].", i);

                if (string.IsNullOrEmpty(line[i].DelFlag) || !line[i].DelFlag.Equals("1")) //Mod 2016/04/26 arc yano #3511
                {
                    //部品番号必須チェック
                    if (string.IsNullOrWhiteSpace(line[i].PartsNumber))
                    {
                        ModelState.AddModelError(prefix + "PartsNumber", MessageUtils.GetMessage("E0001", "部品番号"));
                        return;
                    }
                    else
                    {
                        if (gGenuine.Equals("001"))
                        {
                            //部品番号の重複チェック ※　純正区分 = 「純正」の場合のみチェックする
                            if (line.Where(x => ((x.PartsNumber ?? "").Equals(line[i].PartsNumber)) && (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() > 1)
                            {
                                ModelState.AddModelError(prefix + "PartsNumber", "同一部品番号は複数行に登録できません。部品番号：" + line[i].PartsNumber);
                            }
                        }
                    }
                    //数量
                    if (line[i].Quantity == null || line[i].Quantity <= 0)
                    {
                        ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" })); ;
                    }
                    else
                    {
                        if (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                                || (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}$")))
                        {
                        }
                        else
                        {
                            ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "数量", "正の整数7桁以内かつ小数2桁以内" }));
                        }
                    }
                }
            }
        }
        */
        /// <summary>
        /// 発注処理時のValidateチェック
        /// </summary>
        /// <param name="form">フォーム入力値</param>
        /// <param name="line">発注情報</param>
        /// <param name="lineNo">行番号</param>
        /// <return></return>
        protected virtual void ValidatePurchaseOrder(FormCollection form, PartsPurchaseOrder line, int lineNo)
        {
            //---------------------
            //必須チェック
            //---------------------
            //仕入先
            if (string.IsNullOrEmpty(form["SupplierCode"]))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "仕入先"));
            }
            //支払先
            if (string.IsNullOrEmpty(form["SupplierPaymentCode"]))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0001", "支払先"));
            }
            //発注日
            if (string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]))
            {
                ModelState.AddModelError("PurchaseOrderDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "発注日" }));
            }
            //到着予定日
            if (string.IsNullOrWhiteSpace(form["ArrivalPlanDate"]))
            {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "入荷予定日" }));
            }
            //支払予定日
            if (string.IsNullOrWhiteSpace(form["PaymentPlanDate"]))
            {
                ModelState.AddModelError("PaymentPlanDate", MessageUtils.GetMessage("E0009", new string[] { "発注処理", "支払予定日" }));
            }
            
            return;
        }
        #endregion

        #region " システムログ_処理名取得 "
        /// <summary>
        /// システムログ出力用の処理名取得
        /// </summary>
        /// <param name="form"></param>
        /// <returns>2014/09/08 arc amii 追加</returns>
        private string GetSysLogProcName(FormCollection form)
        {
            // Add 2014/09/08 arc amii エラーログ対応 画面入力値をSysLogに出力する
            string procName = "";
            if (CommonUtils.DefaultString(form["Delflag"]).Equals("1"))
            {
                procName = "部品発注取消";
            }
            else if (CommonUtils.DefaultString(form["OrderFlag"]).Equals("1"))
            {
                procName = "部品発注";
            }
            else
            {
                procName = "保存";
            }

            return procName;
        }
        #endregion


        #region 行追加・削除
        /// <summary>
        /// 発注リストを1行追加する
        /// </summary>
        /// <param name="line"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3806 部品発注入力　１０行ボタンの追加
        /// 2017/02/14 arc yano #3641 金額欄のカンマ表示対応
        /// </history>
        public ActionResult AddOrder(List<PartsPurchaseOrder> line, FormCollection form)
        {
            if (line == null)
            {
                line = new List<PartsPurchaseOrder>();
            }
           
            //Mod 2017/11/07 arc yano #3806
            int addLine = int.Parse(form["addLine"]);

            for (int i = 0; i < addLine; i++)
            {
                //発注データを新規作成
                PartsPurchaseOrder order = new PartsPurchaseOrder();
                order.DelFlag = "0";     //削除フラグOFF

                line.Add(order);
            }


            //Del 2016/04/26 arc yano
            //入荷予定日と支払予定日の初期値をセットする
            //DateTime PurchaseOrderDate  = DateTime.Parse(form["PurchaseOrderDate"]);
            //order.PaymentPlanDate = CommonUtils.GetFinalDay(PurchaseOrderDate.AddMonths(1).Year, PurchaseOrderDate.AddMonths(1).Month);
            //order.ArrivalPlanDate = PurchaseOrderDate.AddDays(1);

            ModelState.Clear(); //Add 2017/02/14 arc yano #3641

            SetDataComponent(form);

            return View(viewName, line);
        }

        /// <summary>
        /// 発注リストを1行削除する
        /// </summary>
        /// <param name="id"></param>
        /// <param name="line"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/02/17 arc yano 入力検証のコメントアウト
        /// </history>
        public ActionResult DelOrder(int id, List<PartsPurchaseOrder> line, FormCollection form)
        {
            ModelState.Clear();

            //入力検証
            //ValidateSave(form, line);

            if (ModelState.IsValid)
            {
                line[id].DelFlag = "1";         //削除フラグON
            }

            ModelState.Clear();

            SetDataComponent(form);

            return View(viewName, line);
        }
        #endregion

        #region 
        /// <summary>
        /// 発注伝票番号と部品番号から発注残を取得する(Ajax専用）
        /// </summary>
        /// <param name="code">発注伝票番号</param>
        /// <param name="code">部品番号</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2016/03/22 arc yano 引数の型を変更(PurchaseOrderEntryKeyList→Dictionary)
        /// Mod 2016/04/25 arc nakayama #3494_部品入荷入力画面　発注情報のない入荷データでのエラー
        /// </history>
        public ActionResult GetRemainingQuantity(List<Dictionary<string, string>> KeyList)
        {
            if (Request.IsAjaxRequest())
            {
                List<PurchaseOrderEntryKeyList> PurchaseOrderRetList = new List<PurchaseOrderEntryKeyList>();
                PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrder();

                for (int i = 0; i < KeyList.Count; i++)
                {
                    PurchaseOrderEntryKeyList PurchaseOrderRet = new PurchaseOrderEntryKeyList();

                    //発注伝票番号または部品番号がなかった場合は発注残がNULLのリストをセットする
                    if (!string.IsNullOrWhiteSpace(KeyList[i]["PurchaseQuantity"]) && !string.IsNullOrWhiteSpace(KeyList[i]["PurchaseOrderNumber"]) && !string.IsNullOrWhiteSpace(KeyList[i]["PartsNumber"]))
                    {
                        PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(KeyList[i]["PurchaseOrderNumber"], KeyList[i]["PartsNumber"]);
                        if (PurchaseOrder != null)
                        {
                            PurchaseOrderRet.PurchaseOrderNumber = PurchaseOrder.PurchaseOrderNumber;
                            PurchaseOrderRet.PartsNumber = PurchaseOrder.PartsNumber;
                            PurchaseOrderRet.RemainingQuantity = PurchaseOrder.RemainingQuantity;
                        }
                        else
                        {
                            PurchaseOrderRet.PurchaseOrderNumber = KeyList[i]["PurchaseOrderNumber"];
                            PurchaseOrderRet.PartsNumber = KeyList[i]["PartsNumber"];
                            PurchaseOrderRet.RemainingQuantity = null;
                        }
                    }
                    else
                    {
                        PurchaseOrderRet.PurchaseOrderNumber = KeyList[i]["PurchaseOrderNumber"];
                        PurchaseOrderRet.PartsNumber = KeyList[i]["PartsNumber"];
                        PurchaseOrderRet.RemainingQuantity = null;
                    }
                    
                    PurchaseOrderRetList.Add(PurchaseOrderRet);

                }

                return Json(PurchaseOrderRetList);
            }
            else
            {
                return new EmptyResult();
            }

        }
        #endregion

        #region 仕入先取得(ajax)
        /// <summary>
        /// 部門コードから直近の部品入荷の仕入先を取得する(Ajax専用）
        /// </summary>
        /// <param name="DepartemnetCode">部門コード</param>
        /// <returns>取得結果(取得できない場合でもnullではない)</returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 部品入荷入力　JLR用デフォルト仕入先対応
        /// </history>
        public ActionResult GetSupplierCodeFromPartsPurchase(string DepartmentCode)
        {
            if (Request.IsAjaxRequest())
            {
                //その部門の最新の入荷実績を元に仕入先を設定する
                PartsPurchase rec = new PartsPurchaseDao(db).GetPurchaseResult(DepartmentCode, gGenuine).OrderByDescending(x => x.PurchaseDate).FirstOrDefault();

                CodeData data = new CodeData();

                if(rec != null)
                {
                    data.Code = rec.SupplierCode;
                    data.Name = rec.Supplier.SupplierName;
                }

                return Json(data);
            }
            else
            {
                return new EmptyResult();
            }

        }
        #endregion
    }
}
