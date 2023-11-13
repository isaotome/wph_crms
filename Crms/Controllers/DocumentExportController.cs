using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Collections;
using System.Configuration;
using CrmsReport;
using System.Collections.Specialized;
using System.Xml.Linq;
using OfficeOpenXml;                //CSV用
using OfficeOpenXml.Style;          //OFFICE用 
using OfficeOpenXml.Style.XmlAccess;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;



namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DocumentExportController : Controller
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DocumentExportController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ダウンロード画面表示
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria(string id)
        {
            FormCollection form = new FormCollection();
            form["id"] = id;
            ViewData["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["TermFrom"] = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ViewData["TermTo"] = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
            // add 2016.05.20 nishimura.akira
            form["TargetDateY"] = DateTime.Today.Year.ToString().Substring(1, 3);
            form["TargetDateM"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            SetDataComponent(form);
            return View("DocumentExportCriteria");
        }

        /// <summary>
        /// ダウンロード対象を選択
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            DocumentExportCondition condition = SetCondition(form);
            switch (form["DocumentName"]) {
                case "CarSalesList":
                    List<CarSalesHeader> list1 = new CarSalesOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list1!=null ? list1.Count() : 0;
                    break;
                case "ServiceSalesList":
                    List<ServiceSalesHeader> list2 = new ServiceSalesOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list2 != null ? list2.Count() : 0;
                    break;
                case "NewCustomerList":
                    List<Customer> list3 = new CustomerDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list3 != null ? list3.Count() : 0;
                    break;
                case "ReceiptPlanList": //入金予定一覧
                    List<ReceiptPlan> list4 = new ReceiptPlanDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list4 != null ? list4.Count() : 0;
                    break;
                case "ReceiptList": //入金実績一覧
                    List<Journal> list5 = new JournalDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list5 != null ? list5.Count() : 0;
                    break;
                case "PaymentPlanList": //支払予定一覧
                    List<PaymentPlan> list6 = new PaymentPlanDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list6 != null ? list6.Count() : 0;
                    break;
                case "ReceiptionList": //受付一覧
                    List<CustomerReceiption> list7 = new CustomerReceiptionDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list7 != null ? list7.Count() : 0;
                    break;
                case "CarDM":
                case "ServiceDM": //DM
                    //condition.DmFlag = "001"; //DM許可
                    List<Customer> list8 = new CustomerDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list8 != null ? list8.Count() : 0;
                    break;
                case "CarStockList": //車両在庫表
                    List<SalesCar> list9 = new SalesCarDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list9 != null ? list9.Count() : 0;
                    break;
                case "PartsStockList": //部品在庫表
                    List<PartsStock> list10 = new PartsStockDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list10 != null ? list10.Count() : 0;
                    break;
                case "DeadStockPartsList": //デッドストック一覧
                    List<PartsStock> list11 = new PartsStockDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list11 != null ? list11.Count() : 0;
                    break;
                case "CarPurchaseOrderList": //発注EXCELデータ
                    List<CarPurchaseOrder> list12 =  new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list12 != null ? list12.Count() : 0;
                    break;
                case "CarStopList": //預かり車両一覧
                    condition.StopFlag = "1";
                    List<CarPurchaseOrder> list13 = new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list13 != null ? list13.Count() : 0;
                    break;
                case "PartsTransferList": //部品出庫集計表
                    List<Transfer> list14 = new TransferDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list14 != null ? list14.Count() : 0;
                    break;
                case "PartsPurchaseList": //部品入荷予定
                    List<PartsPurchase> list15 = new PartsPurchaseDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list15 != null ? list15.Count() : 0;
                    break;
                case "ServiceDailyReport": //サービス日報
                    List<ServiceDailyReport> list16 = new ServiceDailyReportDao(db).GetServiceDailyReportList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, condition.TermTo ?? DaoConst.SQL_DATETIME_MAX, condition.DepartmentCode);
                    ViewData["ItemCount"] = list16 != null ? list16.Count() : 0;
                    break;
                case "CSSurveyGR": //CSサーベイ入庫
                    List<CSSurveyGR> list17 = new CSSurveyGRDao(db).GetCSSurveyGRtList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list17 != null ? list17.Count() : 0;
                    break;
                case "JournalList": //入金実績リスト
                    List<JournalList> list18 = new JournalListDao(db).GetJournalList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list18 != null ? list18.Count() : 0;
                    break;
                case "AccountReceivableBalanceList": //残高リスト
                    List<AccountReceivableBalanceList> list19 = new AccountReceivableBalanceListDao(db).GetAccountReceivableBalanceListDao(condition.TargetDate, condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list19 != null ? list19.Count() : 0;
                    break;
                case "CardReceiptPlanList": //カード入金予定
                    List<CardReceiptPlanList> list20 = new CardReceiptPlanListDao(db).GetCardReceiptPlanList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list20 != null ? list20.Count() : 0;
                    break;
                case "PartsAverageCostList": //移動平均
                    List<PartsAverageCostList> list21 = new PartsAverageCostListDao(db).GetPartsAverageCostList(condition.TargetDate);
                    ViewData["ItemCount"] = list21 != null ? list21.Count() : 0;
                    break;
                case "CarStorageList": //預かり車一覧 //Add 2017/09/04 arc yano #3786
                    List<CarStorageList> list22 = new ServiceSalesOrderDao(db).GetCarStorageList(condition);
                    ViewData["ItemCount"] = list22 != null ? list22.Count() : 0;
                    break;
            }
            SetDataComponent(form);
            return View("DocumentExportCriteria");
        }

        /// <summary>
        /// ダウンロードを実行する
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2023/01/11 yano #4158 【車両伝票入力】任意保険料入力項目の追加　既存不具合の修正
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応
        /// 2014/09/19 arc amii エラーメッセージ対応 #3091 戻り値を設定させる為、 void → ActionResultに変更
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Download(FormCollection form)
        {
            DocumentExportCondition condition = SetCondition(form);
            SeparatedValueWriter writer = new SeparatedValueWriter(FieldSeparator.CSV);
            IEnumerable<XElement> data = GetFieldList(form["TargetName"]);
            ContentResult contentResult = new ContentResult(); //Add 2014/09/19 arc amii #3091
            FileContentResult filecontentResult = null;
            int return_val = 0;

            //テンプレートファイルを使用しないタイプで、出力フィールド定義データがなければ中止
            if (data == null && !form["TemplateUse"].Equals("1"))
            {
                return contentResult;
            }

            //Add 2023/01/11 yano #4158
            if(!string.IsNullOrWhiteSpace(form["TargetName"]) && !form["TargetName"].Equals("AccountReceivableBalanceList") && !form["TargetName"].Equals("PartsAverageCostList")){
                condition.TargetDate = null;
            }

            switch (form["TargetName"]) {
                case "CarSalesList": //車両伝票一覧
                    List<CarSalesHeader> list1 = new CarSalesOrderDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarSales", writer.GetBuffer(list1, data, condition));
                    break;
                case "ServiceSalesList": //サービス伝票一覧
                    List<ServiceSalesHeader> list2 = new ServiceSalesOrderDao(db).GetListByCondition(condition);
                    foreach (var item in list2) {
                        item.TotalCost = item.ServiceSalesLine.Sum(x => x.Cost ?? 0);
                    }
                    contentResult = FileDownload("ServiceSales", writer.GetBuffer(list2, data, condition));
                    break;
                case "NewCustomerList": //顧客増減分析
                    List<Customer> list3 = new CustomerDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("NewCustomerList", writer.GetBuffer(list3, data, condition));
                    break;
                case "ReceiptPlanList": //入金予定一覧
                    List<ReceiptPlan> list4 = new ReceiptPlanDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptPlanList", writer.GetBuffer(list4, data, condition));
                    break;
                case "ReceiptList": //入金実績一覧
                    List<Journal> list5 = new JournalDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptList", writer.GetBuffer(list5, data, condition));
                    break;
                case "PaymentPlanList": //支払予定一覧
                    List<PaymentPlan> list6 = new PaymentPlanDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PaymentPlanList", writer.GetBuffer(list6, data, condition));
                    break;
                case "ReceiptionList": //受付一覧
                    List<CustomerReceiption> list7 = new CustomerReceiptionDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptionList", writer.GetBuffer(list7, data, condition));
                    break;
                case "CarDM": //DM
                case "ServiceDM":
                    //condition.DmFlag = "001"; //DM許可
                    List<Customer> list8 = new CustomerDao(db).GetListByCondition(condition);
                    foreach (var item in list8) {
                        if (item.CustomerDM != null) {
                            item.CustomerName = item.CustomerDM.FirstName + " " + item.CustomerDM.LastName;
                            item.PostCode = item.CustomerDM.PostCode;
                            item.Prefecture = item.CustomerDM.Prefecture;
                            item.City = item.CustomerDM.City;
                            item.Address1 = item.CustomerDM.Address1;
                            item.Address2 = item.CustomerDM.Address2;
                        }
                    }
                    contentResult = FileDownload("Customer", writer.GetBuffer(list8, data, condition));
                    break;
                case "CarStockList": //車両在庫表
                    List<SalesCar> list9 = new SalesCarDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarStockList", writer.GetBuffer(list9, data, condition));
                    break;
                case "PartsStockList": //部品在庫表
                    List<PartsStock> list10 = new PartsStockDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsStockList", writer.GetBuffer(list10, data, condition));
                    break;
                case "DeadStockPartsList": //デッドストック一覧
                    List<PartsStock> list11 = new PartsStockDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsStockList", writer.GetBuffer(list11, data, condition));
                    break;
                case "CarPurchaseOrderList": //発注EXCELデータ
                    CarPurchaseOrderDao carPurchaseOrderDao = new CarPurchaseOrderDao(db);
                    List<CarPurchaseOrder> list12 = carPurchaseOrderDao.GetListByCondition(condition);
                    List<CarPurchaseOrder> targetList = carPurchaseOrderDao.SetCarPurchaseOrderList(list12);
                    contentResult = FileDownload("CarPurchaseOrderList", writer.GetBuffer(SetOrderRelation(targetList), data, condition));
                    break;
                case "CarStopList": //預かり車両一覧
                    condition.StopFlag = "1";
                    List<CarPurchaseOrder> list13 = new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarPurchaseOrderList", writer.GetBuffer(SetOrderRelation(list13), data, condition));
                    break;
                case "PartsTransferList": //部品出庫集計表
                    List<Transfer> list14 = new TransferDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsTransferList", writer.GetBuffer(list14, data, condition));
                    break;
                case "PartsPurchaseList": //部品入荷予定
                    List<PartsPurchase> list15 = new PartsPurchaseDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsPurchaseList", writer.GetBuffer(list15, data, condition));
                    break;
                case "ServiceDailyReport": //サービス日報
                    List<ServiceDailyReport> list16 = new ServiceDailyReportDao(db).GetServiceDailyReportList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, condition.TermTo ?? DaoConst.SQL_DATETIME_MAX, condition.DepartmentCode);
                    contentResult = FileDownload("ServiceDailyReport", writer.GetBuffer(list16, data, condition));
                    break;
                case "CSSurveyGR": //CSサーベイ入庫
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "JournalList": //入金実績
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "AccountReceivableBalanceList": //CSサーベイ入庫
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "CardReceiptPlanList": //CSサーベイ入庫
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "PartsAverageCostList": //移動平均
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "CarStorageList": //預かり車両一覧    //Add 2017/09/04 arc yano #3786
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;

            }

            //Add 2017/09/04 arc yano #3786 
            if (!ModelState.IsValid)    //検証エラーの場合は画面を返す
            {
                SetDataComponent(form);
                return View("DocumentExportCriteria");            
            }

            if (return_val == 0)
            {
                //Add 2014/09/19 arc amii エラーメッセージ対応 #3091 returnする値を設定
                return contentResult;
            }
            else
            {
                return filecontentResult;
            }

        }

        /// <summary>
        /// 車両発注依頼に車両伝票、入金合計金額を関連づける
        /// </summary>
        /// <param name="orderList">車両発注依頼リスト</param>
        /// <returns></returns>
        private List<CarPurchaseOrder> SetOrderRelation(List<CarPurchaseOrder> orderList) {
            CarSalesOrderDao salesDao = new CarSalesOrderDao(db);
            JournalDao journalDao = new JournalDao(db);
            List<CarPurchaseOrder> resultList = new List<CarPurchaseOrder>();
            foreach (var order in orderList) {
                CarPurchaseOrder result = order;
                result.CarSalesHeader = salesDao.GetBySlipNumber(order.SlipNumber);
                result.ReceiptAmount = journalDao.GetTotalBySlipNumber(order.SlipNumber);
                resultList.Add(result);
            }
            return resultList;
        }
        /// <summary>
        /// データ付き画面コンポーネントの作成
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2019/01/07 yano #3965 WE版新システム対応（Web.configによる処理の分岐)
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 検証エラーとなった時の考慮
        /// </history>
        private void SetDataComponent(FormCollection form) {
            CodeDao dao = new CodeDao(db);
            ViewData["DocumentList"] = CodeUtils.GetSelectList(CodeUtils.GetDocumentList(form["id"], db), (string.IsNullOrWhiteSpace(form["DocumentName"]) ? form["TargetName"] : form["DocumentName"]), false);    //Add 2017/09/04 arc yano #3786 //Mod 2019/01/07 yano #3965
            ViewData["SalesOrderStatusList"] = CodeUtils.GetSelectListByModel(dao.GetSalesOrderStatusAll(false), form["SalesOrderStatus"], true);
            ViewData["ServiceOrderStatusList"] = CodeUtils.GetSelectListByModel(dao.GetServiceOrderStatusAll(false), form["ServiceOrderStatus"], true);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), form["CustomerRank"], true);
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), form["CustomerType"], true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            ViewData["LocationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetLocationTypeAll(false), form["LocationType"], true);
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), form["CustomerClaimType"], true);
            ViewData["DepartmentList"] = new DepartmentDao(db).GetListAll();
            ViewData["CarList"] = new CarDao(db).GetListAll();
            ViewData["MorterViecleOfficialCodeList"] = CodeUtils.GetSelectList(CodeUtils.GetRegistraionNumberTypeList(), form["MorterViecleOfficialCode"], true);
            Dictionary<string,bool> departmentCheckList = new Dictionary<string,bool>();
            foreach(Department data in (List<Department>)ViewData["DepartmentList"]){
                bool flag = false;
                string checkbox = form[string.Format("dep[{0}]", data.DepartmentCode)];
                if (!string.IsNullOrEmpty(checkbox) && checkbox.Contains("true")) {
                    flag = true;
                }
                departmentCheckList.Add(data.DepartmentCode, flag);
            }
            ViewData["DepartmentCheckList"] = departmentCheckList;

            Dictionary<string, bool> carCheckList = new Dictionary<string, bool>();
            foreach(Car car in (List<Car>)ViewData["CarList"]){
                bool flag = false;
                string checkbox = form[string.Format("car[{0}]",car.CarCode)];
                if(!string.IsNullOrEmpty(checkbox) && checkbox.Contains("true")){
                    flag = true;
                }
                carCheckList.Add(car.CarCode,flag);
            }
            ViewData["CarCheckList"] = carCheckList;

            List<CodeData> accountList = new List<CodeData>();
            AccountDao accountDao = new AccountDao(db);
            Account cr = accountDao.GetByUsageType("CR"); 
            accountList.Add(new CodeData{ Code = cr.AccountCode, Name = cr.AccountName });
            Account sr = accountDao.GetByUsageType("SR");
            accountList.Add(new CodeData { Code = sr.AccountCode, Name = sr.AccountName });
            ViewData["ReceiptPlanTypeList"] = CodeUtils.GetSelectList(accountList, form["ReceiptPlanType"], true);

            List<CodeData> accountList2 = new List<CodeData>();
            Account cp = accountDao.GetByUsageType("CP");
            if (cp != null) {
                accountList2.Add(new CodeData { Code = cp.AccountCode, Name = cp.AccountName });
            }
            Account sp = accountDao.GetByUsageType("SP");
            if (sp != null) {
                accountList2.Add(new CodeData { Code = sp.AccountCode, Name = sp.AccountName });
            }
            ViewData["PaymentPlanTypeList"] = CodeUtils.GetSelectList(accountList2, form["PaymentPlanType"], true);

            ViewData["EmployeeCode"] = form["EmployeeCode"];
            Employee emp = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
            ViewData["EmployeeName"] = emp != null ? emp.EmployeeName : "";
            Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : "";
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["TargetName"] = form["DocumentName"];
            ViewData["TermFrom"] = form["TermFrom"];
            ViewData["TermTo"] = form["TermTo"];
            ViewData["LastUpdateDate"] = form["LastUpdateDate"];
            ViewData["id"] = form["id"];

            ViewData["SalesOrderStatus"] = form["SalesORderStatus"];
            ViewData["ServiceOrderStatus"] = form["ServiceOrderStatus"];
            ViewData["NewUsedType"] = form["NewUsedType"];
            ViewData["CustomerRank"] = form["CustomerRank"];
            ViewData["CustomerType"] = form["CustomerType"];
            ViewData["CarStatus"] = form["CarStatus"];
            ViewData["LocationType"] = form["LocationType"];
            ViewData["CustomerClaimType"] = form["CustomerClaimType"];
            ViewData["ReceiptPlanType"] = form["ReceiptPlanType"];
            ViewData["PaymentPlanType"] = form["PaymentPlanType"];

            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["SupplierCode"] = form["SupplierCode"];
            try { ViewData["SupplierName"] = new SupplierDao(db).GetByKey(form["SupplierCode"]).SupplierName; } catch (NullReferenceException) { }
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            try { ViewData["OfficeName"] = new OfficeDao(db).GetByKey(form["OfficeCode"]).OfficeName; } catch (NullReferenceException) { }
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            try { ViewData["CutomerClaimName"] = new CustomerClaimDao(db).GetByKey(form["CustomerClaimCode"]).CustomerClaimName; } catch (NullReferenceException) { }

            ViewData["FirstRegistrationDateFrom"] = form["FirstRegistrationDateFrom"];
            ViewData["FirstRegistrationDateTo"] = form["FirstRegistrationDateTo"];
            ViewData["ExpireDateFrom"] = form["ExpireDateFrom"];
            ViewData["ExpireDateTo"] = form["ExpireDateTo"];
            ViewData["NextInspectionDateFrom"] = form["NextInspectionDateFrom"];
            ViewData["NextInspectionDateTo"] = form["NextInspectionDateTo"];
            ViewData["RegistrationDateFrom"] = form["RegistrationDateFrom"];
            ViewData["RegistrationDateTo"] = form["RegistrationDateTo"];
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["SalesOrderDateFrom"] = form["SalesOrderDateFrom"];
            ViewData["SalesOrderDateTo"] = form["SalesORderDateTo"];
            ViewData["FirstReceiptionDateFrom"] = form["FirstReceiptionDateFrom"];
            ViewData["FirstReceiptionDateTo"] = form["FirstReceiptionDateTo"];
            ViewData["LastReceiptionDateFrom"] = form["LastReceiptionDateFrom"];
            ViewData["LastReceiptionDateTo"] = form["LastReceiptionDateTo"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            try { ViewData["CarBrandName"] = new BrandDao(db).GetByKey(form["CarBrandCode"]).CarBrandName; } catch (NullReferenceException) { }
            ViewData["CarCode"] = form["CarCode"];
            try { ViewData["CarName"] = new CarDao(db).GetByKey(form["CarCode"]).CarName; } catch (NullReferenceException) { }
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            try { ViewData["CarGradeName"] = new CarGradeDao(db).GetByKey(form["CarGradeCode"]).CarGradeName; } catch (NullReferenceException) { }
            ViewData["DmFlag"] = form["DmFlag"];
            ViewData["InterestedCustomer"] = form["InterestedCustomer"] != null && (form["InterestedCustomer"].Contains("true") || form["InterestedCustomer"].Contains("True"));
            //add 2016.05.20 nishimura.akira
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(false), form["TargetDateY"], true);
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(false), form["TargetDateM"], true);
            ViewData["TargetDateY"] = form["TargetDateY"];
            ViewData["TargetDateM"] = form["TargetDateM"];

            ViewData["TemplateUse"] = form["TemplateUse"];  //Add 2017/09/04 arc yano #3786

        }
        /// <summary>
        /// 検索条件をセットする
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <returns>検索条件オブジェクト</returns>
        /// <history>
        /// 2020/11/16 yano #4070 【店舗管理帳票】車両伝票一覧のデータ抽出条件の誤り
        /// </history>
        private DocumentExportCondition SetCondition(FormCollection form) {
            DocumentExportCondition condition = new DocumentExportCondition();
            CodeDao dao = new CodeDao(db);
            condition.CarStatus = form["CarStatus"];
            c_CarStatus carStatus = dao.GetCarStatusByKey(form["CarStatus"]);
            condition.CarStatusName = carStatus != null ? carStatus.Name : "";

            condition.CustomerRank = form["CustomerRank"];
            c_CustomerRank customerRank = dao.GetCustomerRankByKey(form["CustomerRank"]);
            condition.CustomerRankName = customerRank != null ? customerRank.Name : "";

            condition.CustomerType = form["CustomerType"];
            c_CustomerType customerType = dao.GetCustomerTypeByKey(form["CustomerType"]);
            condition.CustomerTypeName = customerType != null ? customerType.Name : "";

            condition.CustomerKind = form["CustomerKind"];
            c_CustomerKind customerKind = dao.GetCustomerKindByKey(form["CustomerKind"]);
            condition.CustomerKindName = customerKind != null ? customerKind.Name : "";

            condition.DepartmentCode = form["DepartmentCode"];
            Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            condition.DepartmentName = department != null ? department.DepartmentName : "";
            
            condition.EmployeeCode = form["EmployeeCode"];
            Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
            condition.EmployeeName = employee != null ? employee.EmployeeName : "";
            condition.LastUpdateDate = CommonUtils.StrToDateTime(form["LastUpdateDate"],DaoConst.SQL_DATETIME_MAX);
            condition.LocationType = form["LocationType"];
            c_LocationType locationType = dao.GetLocationTypeByKey(form["LocationType"]);
            condition.LocationTypeNane = locationType != null ? locationType.Name : "";

            condition.NewUsedType = form["NewUsedType"];
            c_NewUsedType newUsedType = dao.GetNewUsedTypeByKey(form["NewUsedType"]);
            //DELETE arc uchida vs2012対応
            //condition.NewUsedType = newUsedType!=null ? newUsedType.Name : "";

            condition.SalesOrderStatus = form["SalesOrderStatus"];
            c_SalesOrderStatus salesOrderStatus = dao.GetSalesOrderStatusByKey(form["SalesOrderStatus"]);
            condition.SalesOrderStatusName = salesOrderStatus!=null ? salesOrderStatus.Name : "";

            condition.ServiceOrderStatus = form["ServiceOrderStatus"];
            c_ServiceOrderStatus serviceOrderStatus = dao.GetServiceOrderStatusByKey(form["ServiceOrderStatus"]);
            condition.ServiceOrderStatusName = serviceOrderStatus != null ? serviceOrderStatus.Name : "";

            condition.TermFrom = CommonUtils.StrToDateTime(form["TermFrom"],DaoConst.SQL_DATETIME_MIN);
            condition.TermTo= CommonUtils.StrToDateTime(form["TermTo"],DaoConst.SQL_DATETIME_MAX);
            condition.CustomerClaimType = form["CustomerClaimType"];
            c_CustomerClaimType customerClaimType = dao.GetCustomerClaimTypeByKey(form["CustomerClaimType"]);
            condition.CustomerClaimTypeName = customerClaimType != null ? customerClaimType.Name : "";

            condition.ReceiptPlanType = form["ReceiptPlanType"];
            switch (form["ReceiptPlanType"]) {
                case "CR":
                    condition.ReceiptPlanTypeName = "売掛金";
                    break;
                case "SR":
                    condition.ReceiptPlanTypeName = "サービス売掛金";
                    break;
            }
            condition.PaymentPlanType = form["PaymentPlanType"];
            switch (form["PaymentPlanType"]) {
                case "CP":
                    condition.PaymentPlanTypeName = "買掛金";
                    break;
                case "SP":
                    condition.PaymentPlanTypeName = "サービス買掛金";
                    break;
            }
            condition.SlipNumber = form["SlipNumber"];
            condition.CustomerName = form["CustomerName"];
            condition.SupplierCode = form["SupplierCode"];
            Supplier supplier = new SupplierDao(db).GetByKey(form["SupplierCode"]);
            condition.SupplierName = supplier != null ? supplier.SupplierName : "";
            
            condition.OfficeCode = form["OfficeCode"];
            Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
            condition.OfficeName = office!=null ? office.OfficeName : "";

            condition.CustomerClaimCode = form["CustomerClaimCode"];
            CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(form["CustomerClaimCode"]);
            condition.CustomerClaimName = customerClaim != null ? customerClaim.CustomerClaimName : "";

            condition.FirstRegistrationFrom = form["FirstRegistrationDateFrom"];
            condition.FirstRegistrationTo = form["FirstRegistrationDateTo"];
            condition.RegistrationDateFrom = CommonUtils.StrToDateTime(form["RegistrationDateFrom"]);
            condition.RegistrationDateTo = CommonUtils.StrToDateTime(form["RegistrationDateTo"]);
            condition.ExpireDateFrom = CommonUtils.StrToDateTime(form["ExpireDateFrom"]);
            condition.ExpireDateTo = CommonUtils.StrToDateTime(form["ExpireDateTo"]);
            condition.NextInspectionDateFrom = CommonUtils.StrToDateTime(form["NextInspectionDateFrom"]);
            condition.NextInspectionDateTo = CommonUtils.StrToDateTime(form["NextInspectionDateTo"]);
            condition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"]);
            condition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"]);
            condition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"]);
            condition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"]);
            condition.FirstReceiptionDateFrom = CommonUtils.StrToDateTime(form["FirstReceiptionDateFrom"]);
            condition.FirstReceiptionDateTo = CommonUtils.StrToDateTime(form["FirstReceiptionDateTo"]);
            condition.LastReceiptionDateFrom = CommonUtils.StrToDateTime(form["LastReceiptionDateFrom"]);
            condition.LastReceiptionDateTo = CommonUtils.StrToDateTime(form["LastReceiptionDateTo"]);
            condition.NextInspectionDateFrom = CommonUtils.StrToDateTime(form["NextInspectionDateFrom"]);
            condition.NextInspectionDateTo = CommonUtils.StrToDateTime(form["NextInspectionDateTo"]);
            condition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];
            condition.CarBrandCode = form["CarBrandCode"];
            Brand brand = new BrandDao(db).GetByKey(form["CarBrandCode"]);
            condition.CarBrandName = brand != null ? brand.CarBrandName : "";

            condition.CarCode = form["CarCode"];
            Car car = new CarDao(db).GetByKey(form["CarCode"]);
            condition.CarName = car != null ? car.CarName : "";
            
            condition.CarGradeCode = form["CarGradeCode"];
            CarGrade grade = new CarGradeDao(db).GetByKey(form["CarGradeCode"]);
            condition.CarGradeName = grade != null ? grade.CarGradeName : "";

            condition.DmFlag = form["DmFlag"];
            if (form["DmFlag"] != null) {
                switch (form["DmFlag"]) {
                    case "001":
                        condition.DmFlagName = "可";
                        break;
                    case "002":
                        condition.DmFlagName = "非";
                        break;
                    default:
                        condition.DmFlagName = "全て";
                        break;
                }
            }

            List<Department> targetDepartmentList = new List<Department>();
            List<Department> departmentList = new DepartmentDao(db).GetListAll();
            foreach (Department data in departmentList) {
                string checkbox = form[string.Format("dep[{0}]", data.DepartmentCode)];
                if (!string.IsNullOrEmpty(checkbox) && (checkbox.Contains("true") || checkbox.Contains("True"))) {
                    targetDepartmentList.Add(data);
                }
            }
            condition.DepartmentList = targetDepartmentList;

            List<Car> targetCarList = new List<Car>();
            List<Car> carList = new CarDao(db).GetListAll();
            foreach (Car data in carList) {
                string checkbox = form[string.Format("car[{0}]", data.CarCode)];
                if (!string.IsNullOrEmpty(checkbox) && (checkbox.Contains("true") || checkbox.Contains("True"))) {
                    targetCarList.Add(data);
                }
            }
            condition.CarList = targetCarList;
            condition.InterestedCustomer = form["InterestedCustomer"] != null && (form["InterestedCustomer"].Contains("true") || form["InterestedCustomer"].Contains("True"));



            condition.AuthEmployee = (Employee)Session["Employee"];


            // add 2016.05.22 nishimura.akira
            condition.TargetDate = DateTime.Parse(dao.GetYear(false, form["TargetDateY"]).Name + "/" + dao.GetMonth(false, form["TargetDateM"]).Name + "/01");

            condition.DelFlag = "0";        //Add 2020/11/16 yano #4070

            return condition;
        }
        // Mod 2014/09/19 arc amii エラーメッセージ対応 #3091 戻り値を設定させる為、 void → ActionResultに変更
        /// <summary>
        /// CSVを作成しダウンロードダイアログを表示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Excel出力対象データリスト</param>
        /// <param name="fieldList">出力フィールドリスト</param>
        private ContentResult FileDownload(string prefix, string buffer) {
            string fileName = prefix + "_" + DateTime.Now.Ticks + ".csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/octet-stream";
            System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("Shift_JIS");
            // Del 2014/09/19 arc amii エラーメッセージ対応 #3091 Reponse.End()を行うと「HTTP ヘッダーが～」がログに出される為、削除
            //Response.BinaryWrite(encoding.GetBytes(buffer));
            //Response.Flush();
            ////vs2012対応 2014/04/24 arc ookubo
            ////Response.Close();
            //Response.End();

            return Content(buffer, "application/octet-stream", encoding);

        }

        /// <summary>
        /// フィールド定義データから出力フィールドリストを取得する
        /// </summary>
        /// <param name="documentName">帳票名</param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 データが取得できなかった時の考慮を追加
        /// </history>
        private IEnumerable<XElement> GetFieldList(string documentName) {
            XDocument xml = XDocument.Load(Server.MapPath("/Models/ExportFieldList.xml"));
            var query = (from x in xml.Descendants("Title")
                         where x.Attribute("ID").Value.Equals(documentName)
                         select x).FirstOrDefault();

            IEnumerable<XElement> list = null;

            //Add 2017/09/04 arc yano #3786
            if (query != null)
            {
                list = from a in query.Descendants("Name") select a;
            }

            return list;
        }
        /// <summary>
        /// 対象日の最終時間を返す
        /// </summary>
        /// <param name="documentName">最終時間を答える</param>
        /// <returns></returns>
        /// 
        
        private static DateTime EndOfDay(DateTime d)
        {
            string ds = d.ToShortDateString().Trim();
            return DateTime.Parse(ds + " 23:59:59");
        }

        /// <summary>
        /// エクセルのダウンロードを実行する
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 データが取得できなかった時の考慮を追加
        /// </history>
        public FileContentResult ExcelDownload(FormCollection form, DocumentExportCondition condition)
        {
            //ファイル名取得
            string fileName = GetFileName(form, condition);
            //データ取得
            byte[] excelData = MakeData(form, condition, fileName);

            if (excelData != null)
            {
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return new FileContentResult(excelData, contentType) { FileDownloadName = fileName };
            }
            else
            {
                return null;
            }

            //return new File(excelData, contentType, fileName);
        }

        /// <summary>
        /// ダウンロード用のファイル名を作成
        /// </summary>
        /// <param name="form">フォーム名</param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 預かり車のファイル名の追加
        /// </history>
        private string GetFileName(FormCollection form, DocumentExportCondition condition)
        {
            string fileName = "";
            string extension = ".xlsx";
            CodeDao dao = new CodeDao(db);
            switch (form["TargetName"])
            {
                case "CSSurveyGR":
                    fileName = form["TargetName"] + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "JournalList":
                    fileName = "入金消込" + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "AccountReceivableBalanceList": //CSサーベイ入庫
                    fileName = "売掛残高" + "_" + "対象月" + "_" + dao.GetYear(false, form["TargetDateY"]).Name + "年" + dao.GetMonth(false, form["TargetDateM"]).Name + "月";
                    fileName = fileName + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "CardReceiptPlanList": //カード入金予定
                    fileName = "カード入金予定" + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "PartsAverageCostList": //移動平均
                    fileName = "移動平均単価" + "_" + "対象月" + "_" + dao.GetYear(false, form["TargetDateY"]).Name + "年" + dao.GetMonth(false, form["TargetDateM"]).Name + "月";
                    fileName = fileName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "CarStorageList": //移動平均 //Add 2017/09/04 arc yano #3786
                    string departmentName = "";

                    if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
                    {
                        departmentName = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
                    }

                    fileName = form["DepartmentCode"] + "_" + departmentName + "預り車リスト_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;

            }
            return fileName;
        }

        /// <summary>
        /// エクセル用データを作成する
        /// </summary>
        /// <param name="form">フォームデータ</param>
        /// <param name="condition">検索条件</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 ファイル名を引数でもうラス
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private byte[] MakeData(FormCollection form, DocumentExportCondition condition, string fileName = "")
        {
            string SearchInfo = "";
            byte[] excelData;
            //fileName = "";
            bool ret;

            //ワークフォルダ取得
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];      //Add 2017/09/04 arc yano #3786

            CodeDao dao = new CodeDao(db);

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //エクセルファイルオープン
            ExcelPackage excelFile = dExport.MakeExcel(fileName);

            switch (form["TargetName"])
            {

                case "CSSurveyGR":  //CSサーベイ入庫
                    //データ設定
                    List<CSSurveyGR> list1 = new CSSurveyGRDao(db).GetCSSurveyGRtList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //日付設定
                    SearchInfo = "期間:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "～";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " 拠点:" + condition.DepartmentName;
                    }

                    ret = AddDataExcel<CSSurveyGR>(ref excelFile, list1, form["TargetName"], SearchInfo);
                    break;
                case "JournalList":
                    //データ設定
                    List<JournalList> list2 = new JournalListDao(db).GetJournalList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //日付設定
                    SearchInfo = "期間:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "～";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " 拠点:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<JournalList>(ref excelFile, list2, form["TargetName"], SearchInfo);
                    break;
                case "AccountReceivableBalanceList": 
                    List<AccountReceivableBalanceList> list3 = new AccountReceivableBalanceListDao(db).GetAccountReceivableBalanceListDao(condition.TargetDate, condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    SearchInfo = "締月:" + dao.GetYear(false, form["TargetDateY"]).Name + "年" + dao.GetMonth(false, form["TargetDateM"]).Name + "月";
                    SearchInfo = SearchInfo + "期間:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "～";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " 拠点:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<AccountReceivableBalanceList>(ref excelFile, list3, form["TargetName"], SearchInfo);
                    break;
                case "CardReceiptPlanList": //カード入金予定
                    List<CardReceiptPlanList> list4 = new CardReceiptPlanListDao(db).GetCardReceiptPlanList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //日付設定
                    SearchInfo = "期間:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "～";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " 拠点:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<CardReceiptPlanList>(ref excelFile, list4, form["TargetName"], SearchInfo);
                    break;
                case "PartsAverageCostList": //移動平均単価一覧
                    List<PartsAverageCostList> list5 = new PartsAverageCostListDao(db).GetPartsAverageCostList(condition.TargetDate);
                    //日付設定
                    SearchInfo = "締月:" + dao.GetYear(false, form["TargetDateY"]).Name + "年" + dao.GetMonth(false, form["TargetDateM"]).Name + "月";
                    ret = AddDataExcel<PartsAverageCostList>(ref excelFile, list5, form["TargetName"], SearchInfo);
                    break;

                case "CarStorageList": //預かり車両一覧    //Add 2017/09/04 arc yano #3786
                    condition.KeepsCarFlag = true;
                    List<CarStorageList> list6 = new ServiceSalesOrderDao(db).GetCarStorageList(condition);

                    //テンプレートファイルパス取得
                    string tfileName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarStorageList"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarStorageList"];

                    if(string.IsNullOrWhiteSpace(tfileName))
                    {
                        ModelState.AddModelError("", "テンプレートファイルのパスが設定されていません");

                        return null;
                    }

                    fileName = filePath + fileName;

                    ret = AddDataExcelWithTemplate<CarStorageList>(ref excelFile, list6, fileName, tfileName, form);

                    break;
            }

            excelData = excelFile.GetAsByteArray();

            //ファイル削除
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

        public bool AddDataExcel<T>(ref ExcelPackage excelFile, List<T> dataList, String SheetName, String SearchInfo)
        {
            const int dateType = 0;                       //データタイプ(帳票形式)
            bool ret;
            int rowcnt = 0;
            int linecnt = 0;

            //データ出力クラスのインスタンス化
            DataExport dExport = new DataExport();

            //----------------------------
            // 検索条件出力
            //----------------------------
            //出力位置の設定
            string setPos = "A1";

            ConfigLine configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //検索条件文字列を作成
            DataTable dtCondtion = MakeConditionRow(SearchInfo);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // ヘッダ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A2";

            //設定値
            configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //ヘッダ行の取得
            IEnumerable<XElement> data = GetFieldList(SheetName);

            //取得したヘッダ行リストをデータテーブルに設定
            DataTable dtHeader = MakeHeaderRow(data, ref rowcnt);

            //データ設定
            ret = dExport.SetData(ref excelFile, dtHeader, configLine);

            //----------------------------
            // データ行出力
            //----------------------------
            //出力位置の設定
            setPos = "A3";

            //設定値を取得
            configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //データ設定
            ret = dExport.SetData<T, T>(ref excelFile, dataList, configLine);

            //----------------------------
            // 書式設定
            //----------------------------
            //1列多いため調整
            rowcnt--;
            //ヘッダ2行追加
            linecnt = dataList.Count + 2;
            SetFormat(ref excelFile, rowcnt, linecnt, SheetName);

            return true;
        }

        /// <summary>
        /// エクセル用データを作成する(テンプレートファイルあり)
        /// </summary>
        /// <param name="excelFile">Excelデータ</param>
        /// <param name="dataList">一覧データ</param>
        /// <param name="fileName">ファイル名</param>
        /// <param name="tfileName">テンプレートファイル名</param>
        /// <param name="form">フォーム</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応　新規作成
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public bool AddDataExcelWithTemplate<T>(ref ExcelPackage excelFile, List<T> dataList, string fileName, string tfileName, FormCollection form)
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
            excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

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
                return false;
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
                return false;
            }


            //ワークシートオープン
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            ConfigLine cl = new ConfigLine();

            cl.DIndex = configLine.DIndex;
            cl.SheetName = configLine.SheetName;
            cl.Type = configLine.Type;

            
            //----------------------------
            // データ行出力
            //----------------------------
            //データ設定
            ret = dExport.SetData<T, T>(ref excelFile, dataList, configLine);


            //---------------------------
            //個別処理
            //---------------------------
            switch (form["TargetName"])
            {
               case "CarStorageList": //預かり車両一覧

                    //------------------------
                    // 棚卸実施日(=出力日)
                    //------------------------
                    //値を設定
                    worksheet.Cells["L1"].Value = DateTime.Today;

                    //------------------------
                    // 拠点
                    //------------------------
                    if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
                    {
                        Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                        string strdep = dep.DepartmentName + "(" + form["DepartmentCode"] + ")";

                        worksheet.Cells["L3"].Value = strdep;

                    }

                    break;

                default:
                    break;
            
            }

            return true;
        }



        #region ヘッダ行の作成(Excel出力用)
        /// <summary>
        /// ヘッダ行の作成(Excel出力用)
        /// </summary>
        /// <param name="list">列名リスト</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list, ref int i)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();

            //データテーブルにxmlの値を設定する
            i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region 検索条件文作成(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(string SearchInfo)
        {
            //出力バッファ用コレクション
            DataTable dt = new DataTable();
            //---------------------
            //　列定義
            //---------------------
            //１つの列を設定  
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //データ設定
            //---------------
            DataRow row = dt.NewRow();

            //作成したテキストをカラムに設定
            row["CondisionText"] = SearchInfo;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region フォーマット設定(Excel出力用)
        /// <summary>
        /// 検索条件文作成(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private void SetFormat(ref ExcelPackage excelFile, int rowcnt, int linecnt, string SheetName)
        {
            //ワークシート
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[SheetName];
            //cellのRangeを設定
            var headerCells = worksheet.Cells[2, 1, 2, rowcnt];
            //スタイルを適用
            headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //背景色設定
            headerCells.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            //中央配置
            headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //太字
            headerCells.Style.Font.Bold = true;
            var bodyCells = worksheet.Cells[2, 1, linecnt, rowcnt];
            //罫線を設定
            bodyCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        #endregion
    }
}
