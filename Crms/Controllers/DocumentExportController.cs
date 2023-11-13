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
using OfficeOpenXml;                //CSV�p
using OfficeOpenXml.Style;          //OFFICE�p 
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
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public DocumentExportController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �_�E�����[�h��ʕ\��
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
        /// �_�E�����[�h�Ώۂ�I��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή�
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
                case "ReceiptPlanList": //�����\��ꗗ
                    List<ReceiptPlan> list4 = new ReceiptPlanDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list4 != null ? list4.Count() : 0;
                    break;
                case "ReceiptList": //�������шꗗ
                    List<Journal> list5 = new JournalDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list5 != null ? list5.Count() : 0;
                    break;
                case "PaymentPlanList": //�x���\��ꗗ
                    List<PaymentPlan> list6 = new PaymentPlanDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list6 != null ? list6.Count() : 0;
                    break;
                case "ReceiptionList": //��t�ꗗ
                    List<CustomerReceiption> list7 = new CustomerReceiptionDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list7 != null ? list7.Count() : 0;
                    break;
                case "CarDM":
                case "ServiceDM": //DM
                    //condition.DmFlag = "001"; //DM����
                    List<Customer> list8 = new CustomerDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list8 != null ? list8.Count() : 0;
                    break;
                case "CarStockList": //�ԗ��݌ɕ\
                    List<SalesCar> list9 = new SalesCarDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list9 != null ? list9.Count() : 0;
                    break;
                case "PartsStockList": //���i�݌ɕ\
                    List<PartsStock> list10 = new PartsStockDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list10 != null ? list10.Count() : 0;
                    break;
                case "DeadStockPartsList": //�f�b�h�X�g�b�N�ꗗ
                    List<PartsStock> list11 = new PartsStockDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list11 != null ? list11.Count() : 0;
                    break;
                case "CarPurchaseOrderList": //����EXCEL�f�[�^
                    List<CarPurchaseOrder> list12 =  new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list12 != null ? list12.Count() : 0;
                    break;
                case "CarStopList": //�a����ԗ��ꗗ
                    condition.StopFlag = "1";
                    List<CarPurchaseOrder> list13 = new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list13 != null ? list13.Count() : 0;
                    break;
                case "PartsTransferList": //���i�o�ɏW�v�\
                    List<Transfer> list14 = new TransferDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list14 != null ? list14.Count() : 0;
                    break;
                case "PartsPurchaseList": //���i���ח\��
                    List<PartsPurchase> list15 = new PartsPurchaseDao(db).GetListByCondition(condition);
                    ViewData["ItemCount"] = list15 != null ? list15.Count() : 0;
                    break;
                case "ServiceDailyReport": //�T�[�r�X����
                    List<ServiceDailyReport> list16 = new ServiceDailyReportDao(db).GetServiceDailyReportList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, condition.TermTo ?? DaoConst.SQL_DATETIME_MAX, condition.DepartmentCode);
                    ViewData["ItemCount"] = list16 != null ? list16.Count() : 0;
                    break;
                case "CSSurveyGR": //CS�T�[�x�C����
                    List<CSSurveyGR> list17 = new CSSurveyGRDao(db).GetCSSurveyGRtList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list17 != null ? list17.Count() : 0;
                    break;
                case "JournalList": //�������у��X�g
                    List<JournalList> list18 = new JournalListDao(db).GetJournalList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list18 != null ? list18.Count() : 0;
                    break;
                case "AccountReceivableBalanceList": //�c�����X�g
                    List<AccountReceivableBalanceList> list19 = new AccountReceivableBalanceListDao(db).GetAccountReceivableBalanceListDao(condition.TargetDate, condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list19 != null ? list19.Count() : 0;
                    break;
                case "CardReceiptPlanList": //�J�[�h�����\��
                    List<CardReceiptPlanList> list20 = new CardReceiptPlanListDao(db).GetCardReceiptPlanList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    ViewData["ItemCount"] = list20 != null ? list20.Count() : 0;
                    break;
                case "PartsAverageCostList": //�ړ�����
                    List<PartsAverageCostList> list21 = new PartsAverageCostListDao(db).GetPartsAverageCostList(condition.TargetDate);
                    ViewData["ItemCount"] = list21 != null ? list21.Count() : 0;
                    break;
                case "CarStorageList": //�a����Ԉꗗ //Add 2017/09/04 arc yano #3786
                    List<CarStorageList> list22 = new ServiceSalesOrderDao(db).GetCarStorageList(condition);
                    ViewData["ItemCount"] = list22 != null ? list22.Count() : 0;
                    break;
            }
            SetDataComponent(form);
            return View("DocumentExportCriteria");
        }

        /// <summary>
        /// �_�E�����[�h�����s����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2023/01/11 yano #4158 �y�ԗ��`�[���́z�C�ӕی������͍��ڂ̒ǉ��@�����s��̏C��
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή�
        /// 2014/09/19 arc amii �G���[���b�Z�[�W�Ή� #3091 �߂�l��ݒ肳����ׁA void �� ActionResult�ɕύX
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

            //�e���v���[�g�t�@�C�����g�p���Ȃ��^�C�v�ŁA�o�̓t�B�[���h��`�f�[�^���Ȃ���Β��~
            if (data == null && !form["TemplateUse"].Equals("1"))
            {
                return contentResult;
            }

            //Add 2023/01/11 yano #4158
            if(!string.IsNullOrWhiteSpace(form["TargetName"]) && !form["TargetName"].Equals("AccountReceivableBalanceList") && !form["TargetName"].Equals("PartsAverageCostList")){
                condition.TargetDate = null;
            }

            switch (form["TargetName"]) {
                case "CarSalesList": //�ԗ��`�[�ꗗ
                    List<CarSalesHeader> list1 = new CarSalesOrderDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarSales", writer.GetBuffer(list1, data, condition));
                    break;
                case "ServiceSalesList": //�T�[�r�X�`�[�ꗗ
                    List<ServiceSalesHeader> list2 = new ServiceSalesOrderDao(db).GetListByCondition(condition);
                    foreach (var item in list2) {
                        item.TotalCost = item.ServiceSalesLine.Sum(x => x.Cost ?? 0);
                    }
                    contentResult = FileDownload("ServiceSales", writer.GetBuffer(list2, data, condition));
                    break;
                case "NewCustomerList": //�ڋq��������
                    List<Customer> list3 = new CustomerDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("NewCustomerList", writer.GetBuffer(list3, data, condition));
                    break;
                case "ReceiptPlanList": //�����\��ꗗ
                    List<ReceiptPlan> list4 = new ReceiptPlanDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptPlanList", writer.GetBuffer(list4, data, condition));
                    break;
                case "ReceiptList": //�������шꗗ
                    List<Journal> list5 = new JournalDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptList", writer.GetBuffer(list5, data, condition));
                    break;
                case "PaymentPlanList": //�x���\��ꗗ
                    List<PaymentPlan> list6 = new PaymentPlanDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PaymentPlanList", writer.GetBuffer(list6, data, condition));
                    break;
                case "ReceiptionList": //��t�ꗗ
                    List<CustomerReceiption> list7 = new CustomerReceiptionDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("ReceiptionList", writer.GetBuffer(list7, data, condition));
                    break;
                case "CarDM": //DM
                case "ServiceDM":
                    //condition.DmFlag = "001"; //DM����
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
                case "CarStockList": //�ԗ��݌ɕ\
                    List<SalesCar> list9 = new SalesCarDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarStockList", writer.GetBuffer(list9, data, condition));
                    break;
                case "PartsStockList": //���i�݌ɕ\
                    List<PartsStock> list10 = new PartsStockDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsStockList", writer.GetBuffer(list10, data, condition));
                    break;
                case "DeadStockPartsList": //�f�b�h�X�g�b�N�ꗗ
                    List<PartsStock> list11 = new PartsStockDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsStockList", writer.GetBuffer(list11, data, condition));
                    break;
                case "CarPurchaseOrderList": //����EXCEL�f�[�^
                    CarPurchaseOrderDao carPurchaseOrderDao = new CarPurchaseOrderDao(db);
                    List<CarPurchaseOrder> list12 = carPurchaseOrderDao.GetListByCondition(condition);
                    List<CarPurchaseOrder> targetList = carPurchaseOrderDao.SetCarPurchaseOrderList(list12);
                    contentResult = FileDownload("CarPurchaseOrderList", writer.GetBuffer(SetOrderRelation(targetList), data, condition));
                    break;
                case "CarStopList": //�a����ԗ��ꗗ
                    condition.StopFlag = "1";
                    List<CarPurchaseOrder> list13 = new CarPurchaseOrderDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("CarPurchaseOrderList", writer.GetBuffer(SetOrderRelation(list13), data, condition));
                    break;
                case "PartsTransferList": //���i�o�ɏW�v�\
                    List<Transfer> list14 = new TransferDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsTransferList", writer.GetBuffer(list14, data, condition));
                    break;
                case "PartsPurchaseList": //���i���ח\��
                    List<PartsPurchase> list15 = new PartsPurchaseDao(db).GetListByCondition(condition);
                    contentResult = FileDownload("PartsPurchaseList", writer.GetBuffer(list15, data, condition));
                    break;
                case "ServiceDailyReport": //�T�[�r�X����
                    List<ServiceDailyReport> list16 = new ServiceDailyReportDao(db).GetServiceDailyReportList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, condition.TermTo ?? DaoConst.SQL_DATETIME_MAX, condition.DepartmentCode);
                    contentResult = FileDownload("ServiceDailyReport", writer.GetBuffer(list16, data, condition));
                    break;
                case "CSSurveyGR": //CS�T�[�x�C����
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "JournalList": //��������
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "AccountReceivableBalanceList": //CS�T�[�x�C����
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "CardReceiptPlanList": //CS�T�[�x�C����
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "PartsAverageCostList": //�ړ�����
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;
                case "CarStorageList": //�a����ԗ��ꗗ    //Add 2017/09/04 arc yano #3786
                    filecontentResult = ExcelDownload(form, condition);
                    return_val = 1;
                    break;

            }

            //Add 2017/09/04 arc yano #3786 
            if (!ModelState.IsValid)    //���؃G���[�̏ꍇ�͉�ʂ�Ԃ�
            {
                SetDataComponent(form);
                return View("DocumentExportCriteria");            
            }

            if (return_val == 0)
            {
                //Add 2014/09/19 arc amii �G���[���b�Z�[�W�Ή� #3091 return����l��ݒ�
                return contentResult;
            }
            else
            {
                return filecontentResult;
            }

        }

        /// <summary>
        /// �ԗ������˗��Ɏԗ��`�[�A�������v���z���֘A�Â���
        /// </summary>
        /// <param name="orderList">�ԗ������˗����X�g</param>
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
        /// �f�[�^�t����ʃR���|�[�l���g�̍쐬
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2019/01/07 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���)
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή� ���؃G���[�ƂȂ������̍l��
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
        /// �����������Z�b�g����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���������I�u�W�F�N�g</returns>
        /// <history>
        /// 2020/11/16 yano #4070 �y�X�܊Ǘ����[�z�ԗ��`�[�ꗗ�̃f�[�^���o�����̌��
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
            //DELETE arc uchida vs2012�Ή�
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
                    condition.ReceiptPlanTypeName = "���|��";
                    break;
                case "SR":
                    condition.ReceiptPlanTypeName = "�T�[�r�X���|��";
                    break;
            }
            condition.PaymentPlanType = form["PaymentPlanType"];
            switch (form["PaymentPlanType"]) {
                case "CP":
                    condition.PaymentPlanTypeName = "���|��";
                    break;
                case "SP":
                    condition.PaymentPlanTypeName = "�T�[�r�X���|��";
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
                        condition.DmFlagName = "��";
                        break;
                    case "002":
                        condition.DmFlagName = "��";
                        break;
                    default:
                        condition.DmFlagName = "�S��";
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
        // Mod 2014/09/19 arc amii �G���[���b�Z�[�W�Ή� #3091 �߂�l��ݒ肳����ׁA void �� ActionResult�ɕύX
        /// <summary>
        /// CSV���쐬���_�E�����[�h�_�C�A���O��\��
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">Excel�o�͑Ώۃf�[�^���X�g</param>
        /// <param name="fieldList">�o�̓t�B�[���h���X�g</param>
        private ContentResult FileDownload(string prefix, string buffer) {
            string fileName = prefix + "_" + DateTime.Now.Ticks + ".csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/octet-stream";
            System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("Shift_JIS");
            // Del 2014/09/19 arc amii �G���[���b�Z�[�W�Ή� #3091 Reponse.End()���s���ƁuHTTP �w�b�_�[���`�v�����O�ɏo�����ׁA�폜
            //Response.BinaryWrite(encoding.GetBytes(buffer));
            //Response.Flush();
            ////vs2012�Ή� 2014/04/24 arc ookubo
            ////Response.Close();
            //Response.End();

            return Content(buffer, "application/octet-stream", encoding);

        }

        /// <summary>
        /// �t�B�[���h��`�f�[�^����o�̓t�B�[���h���X�g���擾����
        /// </summary>
        /// <param name="documentName">���[��</param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή� �f�[�^���擾�ł��Ȃ��������̍l����ǉ�
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
        /// �Ώۓ��̍ŏI���Ԃ�Ԃ�
        /// </summary>
        /// <param name="documentName">�ŏI���Ԃ𓚂���</param>
        /// <returns></returns>
        /// 
        
        private static DateTime EndOfDay(DateTime d)
        {
            string ds = d.ToShortDateString().Trim();
            return DateTime.Parse(ds + " 23:59:59");
        }

        /// <summary>
        /// �G�N�Z���̃_�E�����[�h�����s����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή� �f�[�^���擾�ł��Ȃ��������̍l����ǉ�
        /// </history>
        public FileContentResult ExcelDownload(FormCollection form, DocumentExportCondition condition)
        {
            //�t�@�C�����擾
            string fileName = GetFileName(form, condition);
            //�f�[�^�擾
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
        /// �_�E�����[�h�p�̃t�@�C�������쐬
        /// </summary>
        /// <param name="form">�t�H�[����</param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή� �a����Ԃ̃t�@�C�����̒ǉ�
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
                    fileName = "��������" + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "AccountReceivableBalanceList": //CS�T�[�x�C����
                    fileName = "���|�c��" + "_" + "�Ώی�" + "_" + dao.GetYear(false, form["TargetDateY"]).Name + "�N" + dao.GetMonth(false, form["TargetDateM"]).Name + "��";
                    fileName = fileName + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "CardReceiptPlanList": //�J�[�h�����\��
                    fileName = "�J�[�h�����\��" + "_" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "-";
                    fileName = fileName + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX)) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "PartsAverageCostList": //�ړ�����
                    fileName = "�ړ����ϒP��" + "_" + "�Ώی�" + "_" + dao.GetYear(false, form["TargetDateY"]).Name + "�N" + dao.GetMonth(false, form["TargetDateM"]).Name + "��";
                    fileName = fileName + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;
                case "CarStorageList": //�ړ����� //Add 2017/09/04 arc yano #3786
                    string departmentName = "";

                    if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
                    {
                        departmentName = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
                    }

                    fileName = form["DepartmentCode"] + "_" + departmentName + "�a��ԃ��X�g_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + extension;
                    break;

            }
            return fileName;
        }

        /// <summary>
        /// �G�N�Z���p�f�[�^���쐬����
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="condition">��������</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή� �t�@�C�����������ł������X
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private byte[] MakeData(FormCollection form, DocumentExportCondition condition, string fileName = "")
        {
            string SearchInfo = "";
            byte[] excelData;
            //fileName = "";
            bool ret;

            //���[�N�t�H���_�擾
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];      //Add 2017/09/04 arc yano #3786

            CodeDao dao = new CodeDao(db);

            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

            //�G�N�Z���t�@�C���I�[�v��
            ExcelPackage excelFile = dExport.MakeExcel(fileName);

            switch (form["TargetName"])
            {

                case "CSSurveyGR":  //CS�T�[�x�C����
                    //�f�[�^�ݒ�
                    List<CSSurveyGR> list1 = new CSSurveyGRDao(db).GetCSSurveyGRtList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //���t�ݒ�
                    SearchInfo = "����:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "�`";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " ���_:" + condition.DepartmentName;
                    }

                    ret = AddDataExcel<CSSurveyGR>(ref excelFile, list1, form["TargetName"], SearchInfo);
                    break;
                case "JournalList":
                    //�f�[�^�ݒ�
                    List<JournalList> list2 = new JournalListDao(db).GetJournalList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //���t�ݒ�
                    SearchInfo = "����:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "�`";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " ���_:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<JournalList>(ref excelFile, list2, form["TargetName"], SearchInfo);
                    break;
                case "AccountReceivableBalanceList": 
                    List<AccountReceivableBalanceList> list3 = new AccountReceivableBalanceListDao(db).GetAccountReceivableBalanceListDao(condition.TargetDate, condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    SearchInfo = "����:" + dao.GetYear(false, form["TargetDateY"]).Name + "�N" + dao.GetMonth(false, form["TargetDateM"]).Name + "��";
                    SearchInfo = SearchInfo + "����:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "�`";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " ���_:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<AccountReceivableBalanceList>(ref excelFile, list3, form["TargetName"], SearchInfo);
                    break;
                case "CardReceiptPlanList": //�J�[�h�����\��
                    List<CardReceiptPlanList> list4 = new CardReceiptPlanListDao(db).GetCardReceiptPlanList(condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN, EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX), condition.DepartmentCode);
                    //���t�ݒ�
                    SearchInfo = "����:" + string.Format("{0:yyyyMMdd}", condition.TermFrom ?? DaoConst.SQL_DATETIME_MIN) + "�`";
                    SearchInfo = SearchInfo + string.Format("{0:yyyyMMdd}", EndOfDay(condition.TermTo ?? DaoConst.SQL_DATETIME_MAX));
                    if (!String.IsNullOrEmpty(condition.DepartmentCode))
                    {
                        SearchInfo = SearchInfo + " ���_:" + condition.DepartmentName;
                    }
                    ret = AddDataExcel<CardReceiptPlanList>(ref excelFile, list4, form["TargetName"], SearchInfo);
                    break;
                case "PartsAverageCostList": //�ړ����ϒP���ꗗ
                    List<PartsAverageCostList> list5 = new PartsAverageCostListDao(db).GetPartsAverageCostList(condition.TargetDate);
                    //���t�ݒ�
                    SearchInfo = "����:" + dao.GetYear(false, form["TargetDateY"]).Name + "�N" + dao.GetMonth(false, form["TargetDateM"]).Name + "��";
                    ret = AddDataExcel<PartsAverageCostList>(ref excelFile, list5, form["TargetName"], SearchInfo);
                    break;

                case "CarStorageList": //�a����ԗ��ꗗ    //Add 2017/09/04 arc yano #3786
                    condition.KeepsCarFlag = true;
                    List<CarStorageList> list6 = new ServiceSalesOrderDao(db).GetCarStorageList(condition);

                    //�e���v���[�g�t�@�C���p�X�擾
                    string tfileName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForCarStorageList"]) ? "" : ConfigurationManager.AppSettings["TemplateForCarStorageList"];

                    if(string.IsNullOrWhiteSpace(tfileName))
                    {
                        ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");

                        return null;
                    }

                    fileName = filePath + fileName;

                    ret = AddDataExcelWithTemplate<CarStorageList>(ref excelFile, list6, fileName, tfileName, form);

                    break;
            }

            excelData = excelFile.GetAsByteArray();

            //�t�@�C���폜
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
            const int dateType = 0;                       //�f�[�^�^�C�v(���[�`��)
            bool ret;
            int rowcnt = 0;
            int linecnt = 0;

            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

            //----------------------------
            // ���������o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            string setPos = "A1";

            ConfigLine configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //����������������쐬
            DataTable dtCondtion = MakeConditionRow(SearchInfo);

            //�f�[�^�ݒ�
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // �w�b�_�s�o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            setPos = "A2";

            //�ݒ�l
            configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //�w�b�_�s�̎擾
            IEnumerable<XElement> data = GetFieldList(SheetName);

            //�擾�����w�b�_�s���X�g���f�[�^�e�[�u���ɐݒ�
            DataTable dtHeader = MakeHeaderRow(data, ref rowcnt);

            //�f�[�^�ݒ�
            ret = dExport.SetData(ref excelFile, dtHeader, configLine);

            //----------------------------
            // �f�[�^�s�o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            setPos = "A3";

            //�ݒ�l���擾
            configLine = dExport.GetDefaultConfigLine(0, SheetName, dateType, setPos);

            //�f�[�^�ݒ�
            ret = dExport.SetData<T, T>(ref excelFile, dataList, configLine);

            //----------------------------
            // �����ݒ�
            //----------------------------
            //1�񑽂����ߒ���
            rowcnt--;
            //�w�b�_2�s�ǉ�
            linecnt = dataList.Count + 2;
            SetFormat(ref excelFile, rowcnt, linecnt, SheetName);

            return true;
        }

        /// <summary>
        /// �G�N�Z���p�f�[�^���쐬����(�e���v���[�g�t�@�C������)
        /// </summary>
        /// <param name="excelFile">Excel�f�[�^</param>
        /// <param name="dataList">�ꗗ�f�[�^</param>
        /// <param name="fileName">�t�@�C����</param>
        /// <param name="tfileName">�e���v���[�g�t�@�C����</param>
        /// <param name="form">�t�H�[��</param>
        /// <history>
        /// 2017/09/04 arc yano #3786 �a�����Excle�o�͑Ή��@�V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public bool AddDataExcelWithTemplate<T>(ref ExcelPackage excelFile, List<T> dataList, string fileName, string tfileName, FormCollection form)
        {
            //----------------------------
            //��������
            //----------------------------
            ConfigLine configLine;                  //�ݒ�l
            byte[] excelData = null;                //�G�N�Z���f�[�^
            bool ret = false;
            bool tFileExists = true;                //�e���v���[�g�t�@�C������^�Ȃ�(���ۂɂ��邩�ǂ���)


            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

            //�G�N�Z���t�@�C���I�[�v��(�e���v���[�g�t�@�C������)
            excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //�e���v���[�g�t�@�C�������������ꍇ
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����������܂���ł����B");

                //�t�@�C���폜
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
            // �ݒ�V�[�g�擾
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //�ݒ�f�[�^���擾(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //config�V�[�g�������ꍇ�̓G���[
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����config�V�[�g���݂���܂���");

                excelData = excelFile.GetAsByteArray();

                //�t�@�C���폜
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


            //���[�N�V�[�g�I�[�v��
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            ConfigLine cl = new ConfigLine();

            cl.DIndex = configLine.DIndex;
            cl.SheetName = configLine.SheetName;
            cl.Type = configLine.Type;

            
            //----------------------------
            // �f�[�^�s�o��
            //----------------------------
            //�f�[�^�ݒ�
            ret = dExport.SetData<T, T>(ref excelFile, dataList, configLine);


            //---------------------------
            //�ʏ���
            //---------------------------
            switch (form["TargetName"])
            {
               case "CarStorageList": //�a����ԗ��ꗗ

                    //------------------------
                    // �I�����{��(=�o�͓�)
                    //------------------------
                    //�l��ݒ�
                    worksheet.Cells["L1"].Value = DateTime.Today;

                    //------------------------
                    // ���_
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



        #region �w�b�_�s�̍쐬(Excel�o�͗p)
        /// <summary>
        /// �w�b�_�s�̍쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="list">�񖼃��X�g</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list, ref int i)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();

            //�f�[�^�e�[�u����xml�̒l��ݒ肷��
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

        #region �����������쐬(Excel�o�͗p)
        /// <summary>
        /// �����������쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="condition">��������</param>
        /// <returns></returns>
        private DataTable MakeConditionRow(string SearchInfo)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();
            //---------------------
            //�@���`
            //---------------------
            //�P�̗��ݒ�  
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //�f�[�^�ݒ�
            //---------------
            DataRow row = dt.NewRow();

            //�쐬�����e�L�X�g���J�����ɐݒ�
            row["CondisionText"] = SearchInfo;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region �t�H�[�}�b�g�ݒ�(Excel�o�͗p)
        /// <summary>
        /// �����������쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="condition">��������</param>
        /// <returns></returns>
        private void SetFormat(ref ExcelPackage excelFile, int rowcnt, int linecnt, string SheetName)
        {
            //���[�N�V�[�g
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[SheetName];
            //cell��Range��ݒ�
            var headerCells = worksheet.Cells[2, 1, 2, rowcnt];
            //�X�^�C����K�p
            headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //�w�i�F�ݒ�
            headerCells.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            //�����z�u
            headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //����
            headerCells.Style.Font.Bold = true;
            var bodyCells = worksheet.Cells[2, 1, linecnt, rowcnt];
            //�r����ݒ�
            bodyCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            bodyCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        #endregion
    }
}
