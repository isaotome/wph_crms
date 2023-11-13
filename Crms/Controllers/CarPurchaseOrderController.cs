using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarPurchaseOrderController : InheritedController {
        #region ������
        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ������˗�����";             // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ������˗��o�^";             // ������
        private static readonly string PROC_NAME_IKKATSU = "�����˗������ꊇ�o�^"; // ������
        private static readonly string PROC_NAME_HACCHU = "����";                  // ������
        private static readonly string PROC_NAME_HIKIATE = "�ʈ���";             // ������
        private static readonly string PROC_NAME_TOROKU = "�ʓo�^";              // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarPurchaseOrderController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;
        #endregion

        #region �������
        /// <summary>
        /// �ԗ������˗�������ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ������˗���������
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (dep != null) {
                    ViewData["DepartmentName"] = dep.DepartmentName;
                }
            }
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                Employee emp = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                if (emp != null) {
                    ViewData["EmployeeName"] = emp.EmployeeName;
                }
            }
            ViewData["SalesCarNumberFrom"] = form["SalesCarNumberFrom"];
            ViewData["SalesCarNumberTo"] = form["SalesCarNumberTo"];
            ViewData["SlipNumberFrom"] = form["SlipNumberFrom"];
            ViewData["SlipNumberTo"] = form["SlipNumberTo"];
            ViewData["SalesOrderDateFrom"] = form["SalesOrderDateFrom"];
            ViewData["SalesOrderDateTo"] = form["SalesOrderDateTo"];
            ViewData["MakerOrderNumberFrom"] = form["MakerOrderNumberFrom"];
            ViewData["MakerOrderNumberTo"] = form["MakerOrderNumberTo"];
            ViewData["RegistrationPlanDateFrom"] = form["RegistrationPlanDateFrom"];
            ViewData["RegistrationPlanDateTo"] = form["RegistrationPlanDateTo"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["ModelCode"] = form["ModelCode"];
            ViewData["ModelName"] = form["ModelName"];
            ViewData["ApprovalFlag"] = !string.IsNullOrEmpty(form["ApprovalFlag"]) && form["ApprovalFlag"].Contains("true");
            ViewData["PurchaseOrderStatus"] = !string.IsNullOrEmpty(form["PurchaseOrderStatus"]) && form["PurchaseOrderStatus"].Contains("true");
            ViewData["ReservationStatus"] = !string.IsNullOrEmpty(form["ReservationStatus"]) && form["ReservationStatus"].Contains("true");
            ViewData["RegistrationStatus"] = !string.IsNullOrEmpty(form["RegistrationStatus"]) && form["RegistrationStatus"].Contains("true");
            ViewData["StopFlag"] = !string.IsNullOrEmpty(form["StopFlag"]) && form["StopFlag"].Contains("true");
            ViewData["NoRegistration"] = !string.IsNullOrEmpty(form["NoRegistration"]) && form["NoRegistration"].Contains("true");
            ViewData["CancelFlag"] = !string.IsNullOrEmpty(form["CancelFlag"]) && form["CancelFlag"].Contains("true");
            ViewData["NoReservation"] = !string.IsNullOrEmpty(form["NoReservation"]) && form["NoReservation"].Contains("true");
            ViewData["Vin"] = form["Vin"];

            CodeDao dao = new CodeDao(db);
            ViewData["RegistMonth"] = CodeUtils.GetSelectListByModel<c_RegistMonth>(dao.GetRegistMonthAll(false), form["RegistMonth"], true);

            if (!string.IsNullOrEmpty(form["CurrentMonth"]) && form["CurrentMonth"].Contains("true")) {
                ViewData["CurrentMonth"] = "true";
            } else {
                ViewData["CurrentMonth"] = "false";
            }

            PaginatedList<CarPurchaseOrder> list;
            if (criteriaInit) {
                list = new PaginatedList<CarPurchaseOrder>();
            } else {
                 list = GetSearchResultList(form);
            }
            return View("CarPurchaseOrderCriteria",list );
        }
        /// <summary>
        /// �t�H�[���̓��͒l����f�[�^��������
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>�����˗��f�[�^���X�g</returns>
        private PaginatedList<CarPurchaseOrder> GetSearchResultList(FormCollection form) {
            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            CarPurchaseOrder carPurchaseOrderCondition = new CarPurchaseOrder();
            carPurchaseOrderCondition.CarSalesHeader = new CarSalesHeader();
            //���F�t���O
            if (form["ApprovalFlag"].Contains("true")) {
                carPurchaseOrderCondition.CarSalesHeader.ApprovalFlag = "1";
            }
            //�����t���O
            if (form["PurchaseOrderStatus"].Contains("true")) {
                carPurchaseOrderCondition.PurchaseOrderStatus = "1";
            }
            //�����t���O
            if (form["ReservationStatus"].Contains("true")) {
                carPurchaseOrderCondition.ReservationStatus = "1";
            }
            //�o�^�t���O
            if (form["RegistrationStatus"].Contains("true")) {
                carPurchaseOrderCondition.RegistrationStatus = "1";
            }
            //�a��
            if (form["StopFlag"].Contains("true")) {
                carPurchaseOrderCondition.StopFlag = "1";
            }
            //������
            if (form["NoReservation"].Contains("true")) {
                carPurchaseOrderCondition.NoReservation = "1";
            }
            //���o�^
            if (form["NoRegistration"].Contains("true")) {
                carPurchaseOrderCondition.NoRegistration = "1";
            }

            //�L�����Z�����\��
            if (!form["CancelFlag"].Contains("true")) {
                carPurchaseOrderCondition.CancelFlag = "1";
            }

            //����
            carPurchaseOrderCondition.DepartmentCode = form["DepartmentCode"];

            //�S����
            carPurchaseOrderCondition.EmployeeCode = form["EmployeeCode"];

            //�Ǘ��ԍ�
            carPurchaseOrderCondition.SalesCarNumberFrom = form["SalesCarNumberFrom"];
            carPurchaseOrderCondition.SalesCarNumberTo = form["SalesCarNumberTo"];

            //�`�[�ԍ�
            carPurchaseOrderCondition.SlipNumberFrom = form["SlipNumberFrom"];
            carPurchaseOrderCondition.SlipNumberTo = form["SlipNumberTo"];

            //�󒍓�
            carPurchaseOrderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseOrderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MAX);

            //�I�[�_�[�ԍ�
            carPurchaseOrderCondition.MakerOrderNumberFrom = form["MakerOrderNumberFrom"];
            carPurchaseOrderCondition.MakerOrderNumberTo = form["MakerOrderNumberTo"];

            //�o�^�\���
            carPurchaseOrderCondition.RegistrationPlanDateFrom = CommonUtils.StrToDateTime(form["RegistrationPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseOrderCondition.RegistrationPlanDateTo = CommonUtils.StrToDateTime(form["RegistrationPlanDateTo"], DaoConst.SQL_DATETIME_MIN);

            //���[�J�[��
            carPurchaseOrderCondition.MakerName = form["MakerName"];

            //�Ԏ�
            carPurchaseOrderCondition.CarName = form["CarName"];

            //���f���R�[�h
            carPurchaseOrderCondition.ModelCode = form["ModelCode"];

            //�^��
            carPurchaseOrderCondition.ModelName = form["ModelName"];

            //Vin
            carPurchaseOrderCondition.Vin = form["Vin"];

            //�{�������̐ݒ�
            Employee employee = (Employee)Session["Employee"];
            carPurchaseOrderCondition.SetAuthCondition(employee);

            PaginatedList<CarPurchaseOrder> list = dao.GetListByCondition(carPurchaseOrderCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            CarSalesOrderDao salesDao = new CarSalesOrderDao(db);
            foreach (var a in list) {
                a.CarSalesHeader = salesDao.GetBySlipNumber(a.SlipNumber);
            }
            return list;
        }
        #endregion

        #region �V�K�����˗�
        /// <summary>
        /// �V�K�����˗��o�^
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry()
        {
            CarPurchaseOrder order = new CarPurchaseOrder();
            order.Employee = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode);
            order.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DepartmentName"] = ((Employee)Session["Employee"]).Department1.DepartmentName;
            order.PurchaseOrderDate = DateTime.Today;

            //�e�X�g�p
            //order.CarGradeCode = "100010112001";
            //order.ExteriorColorCode = "10103";
            //order.InteriorColorCode = "10103";
            //order.PurchaseAmount = 2000000m;
            //order.PayDueDate = DateTime.Parse("2010/03/10");
            //order.SupplierCode = "0000000001";
            //order.SupplierPaymentCode = "0000000001";
            SetDataComponent(order, new FormCollection());
            return View("CarPurchaseOrderEntry",order);
        }

        /// <summary>
        /// �ԗ������˗��o�^����
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarPurchaseOrder order, FormCollection form) {
            ValidateEntry(order,form);
            if (!ModelState.IsValid) {
                SetDataComponent(order,form);
                return View("CarPurchaseOrderEntry", order);
            }
            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //�����˗��f�[�^��V�K�쐬
                order.CarPurchaseOrderNumber = new SerialNumberDao(db).GetNewCarPurchaseOrderNumber();
                
                //��������
                CreatePurchaseOrder(order);

                //���������������ɍs��
                //order.ReservationStatus = "1";

                //�x���\��쐬
                CreatePaymentPlan(order);

                //�����˗�INSERT
                order.CreateDate = DateTime.Now;
                order.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.LastUpdateDate = DateTime.Now;
                order.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.DelFlag = "0";
                order.PurchaseOrderStatus = "1";
                
                db.CarPurchaseOrder.InsertOnSubmit(order);

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // �G���[���x���wERROR�x�Ń��O�o��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        SetDataComponent(order, form);
                        return View("CarPurchaseOrderEntry", order);
                    }
                    else
                    {
                        // �G���[���x���wFATAL�x�Ń��O�o��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    return View("Error");
                }
            }
            SetDataComponent(order,form);
            ViewData["close"] = "1";
            return View("CarPurchaseOrderEntry", order);
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void SetDataComponent(CarPurchaseOrder order ,FormCollection form) {

            ViewData["CarGradeCode"] = string.IsNullOrEmpty(order.CarGradeCode ?? form["CarGradeCode"]) ? null : order.CarGradeCode ?? form["CarGradeCode"];
            ViewData["carGrade"] = new CarGradeDao(db).GetByKey(order.CarGradeCode ?? form["CarGradeCode"]);
            Department dep = new DepartmentDao(db).GetByKey(order.DepartmentCode ?? form["DepartmentCode"]);
            ViewData["DepartmentName"] = dep != null ? dep.DepartmentName : "";
            //ADD 2014/10/30  arc ishii �ۑ��{�^���Ή��@�ۑ����ʃf�[�^�\���̂���
            CarColor InteriorColor = new CarColorDao(db).GetByKey(order.InteriorColorCode ?? form["InteriorColorCode"] );
            ViewData["InteriorColorName"] = InteriorColor != null ? InteriorColor.CarColorName : "";
            CarColor ExteriorColor = new CarColorDao(db).GetByKey(order.ExteriorColorCode ?? form["ExteriorColorCode"]);
            ViewData["ExteriorColorName"] = ExteriorColor != null ? ExteriorColor.CarColorName : "";
            order.Employee = new EmployeeDao(db).GetByKey(order.EmployeeCode ?? form["EmployeeCode"]);
            order.Supplier = new SupplierDao(db).GetByKey(order.SupplierCode ?? form["SupplierCode"]);
            order.SupplierPayment = new SupplierPaymentDao(db).GetByKey(order.SupplierPaymentCode ?? form["SupplierPaymentCode"]);
        }
        /// <summary>
        /// �����˗��f�[�^�V�K�쐬����Validation�`�F�b�N
        /// </summary>
        /// <param name="order">�����˗��f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void ValidateEntry(CarPurchaseOrder order, FormCollection form) {
            CommonValidate("CarGradeCode", "�O���[�h", order, true);
            CommonValidate("PurchaseOrderDate", "������", order, true);
            CommonValidate("DepartmentCode", "����", order, true);
            CommonValidate("EmployeeCode", "�S����", order, true);
            CommonValidate("SupplierCode", "�d����", order, true);
            CommonValidate("SupplierPaymentCode", "�x����", order, true);
            CommonValidate("PayDueDate", "�x������", order, true);
            CommonValidate("Amount", "�d�����i", order, true);

            if (!string.IsNullOrEmpty(order.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                if (salesCar != null) {
                    ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�Ǘ��ԍ������ɑ��݂��邽�ߔ����ł��܂���");
                } else {
                    //�}�X�^�ɑ��݂��Ȃ��Ă��A�����̔Ԃ͈̔͂Ɋ܂܂ꂽ��G���[
                    if (!new SerialNumberDao(db).CanUseSalesCarNumber(order.SalesCarNumber)) {
                        ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�Ǘ��ԍ��͎����̔ԂŎg�p����͈͂Ɋ܂܂�邽�ߎg�p�ł��܂���");
                    }
                }
            }

        }

        #endregion

        #region �����˗��ꊇ���
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="orderList">�����˗����X�g�f�[�^</param>
        private void SetDataComponent(List<CarPurchaseOrder> orderList) {
            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            dao.SetCarPurchaseOrderList(orderList);
        }
        
        /// <summary>
        /// �ԗ������˗��������X�g���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ������˗��ԍ����X�g(�J���}��؂�)</param>
        /// <returns></returns>
        public ActionResult ListEntry()
        {
            string id = Request["OrderId"];
            string[] keyList = id.Split(',');
            List<CarPurchaseOrder> list = new CarPurchaseOrderDao(db).GetListByKeyList(keyList);
            SetDataComponent(list);
            //2010.08.19�\�[�g���̎w��
            var orderdList = from a in list
                             orderby a.CarSalesHeader.SalesOrderDate, a.ReceiptAmount descending
                             select a;
            return View("CarPurchaseOrderListEntry",orderdList.ToList());
        }

        /// <summary>
        /// �����˗������ꊇ�o�^����
        /// </summary>
        /// <param name="data">�����˗��f�[�^���X�g</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ListEntry(List<CarPurchaseOrder> data, FormCollection form) {
            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            CarPurchaseOrderDao dao = new CarPurchaseOrderDao(db);
            CarSalesOrderDao sales = new CarSalesOrderDao(db);
            JournalDao journalDao = new JournalDao(db);
            EmployeeDao employeeDao = new EmployeeDao(db);
            SupplierDao supplierDao = new SupplierDao(db);
            SupplierPaymentDao supplierPaymentDao = new SupplierPaymentDao(db);
            CarPurchaseDao purchaseDao = new CarPurchaseDao(db);
            switch (form["action"]) {
                case "sort":
                    foreach (var a in data) {
                        a.CarSalesHeader = sales.GetBySlipNumber(a.SlipNumber);
                        a.ReceiptAmount = journalDao.GetTotalBySlipNumber(a.SlipNumber);
                        a.Employee = employeeDao.GetByKey(a.EmployeeCode);
                        a.Supplier = supplierDao.GetByKey(a.SupplierCode);
                        a.SupplierPayment = supplierPaymentDao.GetByKey(a.SupplierPaymentCode);
                        if (a.CarSalesHeader == null) a.CarSalesHeader = new CarSalesHeader();
                        if (a.Employee == null) a.Employee = new Employee();
                        if (a.Supplier == null) a.Supplier = new Supplier();
                        if (a.SupplierPayment == null) a.SupplierPayment = new SupplierPayment();
                    }
                    data = SortData(data, form["sortkey"], form["desc"].Equals("1"));

                    ViewData["sortKey"] = form["sortkey"];
                    ViewData["desc"] = form["desc"];
                    ModelState.Clear();
                    SetDataComponent(data);
                    return View("CarPurchaseOrderListEntry", data);
                default:
                    //Validation�`�F�b�N
                    ValidatePurchaseOrder(data);
                    if (!ModelState.IsValid) {
                        SetDataComponent(data);
                        return View("CarPurchaseOrderListEntry", data);
                    }

                    

                    using (TransactionScope ts = new TransactionScope()) {
                        CodeDao codeDao = new CodeDao(db);

                        foreach (CarPurchaseOrder formData in data) {

                            //�X�V�Ώۂ��擾
                            CarPurchaseOrder target = dao.GetByKey(formData.CarPurchaseOrderNumber);

                            //�ԗ��`�[�ƕR�Â���
                            if (!string.IsNullOrEmpty(formData.SlipNumber)) {
                                //formData.CarSalesHeader = sales.GetBySlipNumber(formData.SlipNumber);
                                target.CarSalesHeader = formData.CarSalesHeader;
                            }

                            //�����˗��f�[�^�̍X�V
                            target.ArrangementNumber = formData.ArrangementNumber;
                            target.ArrivalLocationCode = formData.ArrivalLocationCode;
                            target.ArrivalPlanDate = formData.ArrivalPlanDate;
                            target.DocumentReceiptDate = formData.DocumentReceiptDate;
                            target.DocumentReceiptPlanDate = formData.DocumentReceiptPlanDate;
                            target.EmployeeCode = formData.EmployeeCode;
                            target.GracePeriod = formData.GracePeriod;
                            target.IncentiveOfficeCode = formData.IncentiveOfficeCode;
                            target.InspectionDate = formData.InspectionDate;
                            target.InspectionInformation = formData.InspectionInformation;
                            target.LastUpdateDate = DateTime.Now;
                            target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            target.MakerShipmentDate = formData.MakerShipmentDate;
                            target.MakerShipmentPlanDate = formData.MakerShipmentPlanDate;
                            target.PayDueDate = formData.PayDueDate;
                            target.PDIDepartureDate = formData.PDIDepartureDate;
                            target.PurchaseOrderDate = formData.PurchaseOrderDate;
                            target.RegistrationDate = formData.RegistrationDate;
                            target.RegistrationPlanDate = formData.RegistrationPlanDate;
                            target.RegistrationPlanMonth = formData.RegistrationPlanMonth;
                            target.ReserveLocationCode = formData.ReserveLocationCode;
                            target.MakerOrderNumber = formData.MakerOrderNumber;
                            if (!string.IsNullOrEmpty(formData.StopFlag) && formData.StopFlag.Contains("true")) {
                                target.StopFlag = "1";
                            } else {
                                target.StopFlag = "0";
                            }
                            target.SupplierCode = formData.SupplierCode;
                            target.SupplierPaymentCode = formData.SupplierPaymentCode;
                            target.SalesCarNumber = formData.SalesCarNumber;
                            target.Vin = formData.Vin;
                            target.DocumentPurchaseRequestDate = formData.DocumentPurchaseRequestDate;
                            target.RegistMonth = formData.RegistMonth;
                            target.Firm = formData.Firm;
                            target.DiscountAmount = formData.DiscountAmount;
                            target.Amount = formData.Amount;
                            target.FirmMargin = formData.FirmMargin;
                            target.MetallicPrice = formData.MetallicPrice;
                            target.VehiclePrice = formData.VehiclePrice;
                            target.OptionPrice = formData.OptionPrice;

                            //�d���f�[�^�𒊏o
                            CarPurchase purchase = purchaseDao.GetBySalesCarNumber(formData.SalesCarNumber);

                            if (purchase != null) {
                                if (!purchase.FirmPrice.Equals(formData.FirmMargin)) {
                                    purchase.FirmPrice = formData.FirmMargin ?? 0m;
                                }
                                if (!purchase.DiscountPrice.Equals(formData.DiscountAmount)) {
                                    purchase.DiscountPrice = formData.DiscountAmount ?? 0m;
                                }
                                if (!purchase.MetallicPrice.Equals(formData.MetallicPrice)) {
                                    purchase.MetallicPrice = formData.MetallicPrice ?? 0m;
                                }
                                if (!purchase.OptionPrice.Equals(formData.OptionPrice)) {
                                    purchase.OptionPrice = formData.OptionPrice ?? 0m;
                                }
                                if (!purchase.Amount.Equals(formData.Amount)) {
                                    purchase.Amount = formData.Amount ?? 0m;
                                }
                                if (!purchase.VehiclePrice.Equals(formData.VehiclePrice)) {
                                    purchase.VehiclePrice = formData.VehiclePrice ?? 0m;
                                }
                            }


                            //��������
                            if (formData.PurchaseOrderStatus != null && formData.PurchaseOrderStatus.Contains("true")) {
                                target.PurchaseOrderStatus = "1";
                                target.CarGradeCode = target.CarSalesHeader.CarGradeCode;

                                //��������
                                CreatePurchaseOrder(target);

                                //�x���\��쐬
                                CreatePaymentPlan(target);
                            }

                            //��������
                            if (formData.ReservationStatus != null && formData.ReservationStatus.Contains("true")) {
                                target.ReservationStatus = "1";
                                if (formData.PurchaseOrderStatus.Contains("true")) {
                                    CreateReservation(target, true);
                                } else {
                                    CreateReservation(target, false);
                                }
                            }

                            //�d���\��쐬
                            if (formData.PurchasePlanStatus != null && formData.PurchasePlanStatus.Contains("true")) {
                                target.PurchasePlanStatus = "1";
                                CreatePurchasePlan(target);
                            }

                            // �o�^����
                            if (formData.RegistrationStatus != null && formData.RegistrationStatus.Contains("true")) {
                                target.RegistrationStatus = "1";
                                UpdateRegistration(target);
                            }
                        }
                        try {
                            db.SubmitChanges();
                            ts.Complete();
                        } catch (SqlException e) {
                            //Add 2014/08/11 arc amii �G���[���O�Ή� Exception�������A�G���[���O�o�͂���悤�C��
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;

                            if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�o�^"));

                                //�ēxValidation�`�F�b�N�i�Ǘ��ԍ����d�����Ă���\���j
                                ValidatePurchaseOrder(data);
                                SetDataComponent(data);
                                return View("CarPurchaseOrderListEntry", data);
                            }
                            else
                            {
                                // ���O�ɏo��
                                OutputLogger.NLogFatal(e, PROC_NAME_IKKATSU, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                        catch (Exception e)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(e, PROC_NAME_IKKATSU, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    SetDataComponent(data);
                    ViewData["close"] = "1";
                    return View("CarPurchaseOrderListEntry", data);
            }
        }


        #region Validation
        /// <summary>
        /// �ꊇ������������Validation�`�F�b�N
        /// </summary>
        /// <param name="order">�����˗����X�g</param>
        private void ValidatePurchaseOrder(List<CarPurchaseOrder> orderList) {
            //�Ǘ��ԍ����d�����͂���Ă�����G���[
            var query = orderList.Where(x=>!string.IsNullOrEmpty(x.SalesCarNumber))
                .GroupBy(x => x.SalesCarNumber)
                .Select(g => new { SalesCarNumber = g.Key, Num = g.Count() })
                .Where(n => 1 < n.Num);
            if (query.Count() > 0) {
                ModelState.AddModelError("", "�Ǘ��ԍ����d�����Ă��邽�ߕۑ��ł��܂���");
            }

            for (int i = 0; i < orderList.Count; i++) {
                CarPurchaseOrder order = orderList[i];
                order.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(order.SlipNumber);

                #region ����������
                if (order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) {
                    //�������͕K�{����
                    if (order.PurchaseOrderDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchaseOrderDate", i), MessageUtils.GetMessage("E0009", new string[] { "��������", "������" }));
                    } else if (!ModelState.IsValidField("PurchaseOrderDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchaseOrderDate", i), MessageUtils.GetMessage("E0005", "������"));
                        if (ModelState[string.Format("data[{0}].PurchaseOrderDate", i)].Errors.Count > 1) {
                            ModelState[string.Format("data[{0}].PurchaseOrderDate", i)].Errors.RemoveAt(0);
                        }
                    }
                    //�d����͕K�{����
                    if (string.IsNullOrEmpty(order.SupplierCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierCode", i), MessageUtils.GetMessage("E0007", new string[] { "��������", "�d����R�[�h" }));
                    }

                    //�x����͕K�{����
                    if (string.IsNullOrEmpty(order.SupplierPaymentCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierPaymentCode", i), MessageUtils.GetMessage("E0007", new string[] { "��������", "�x����R�[�h" }));
                    }

                    //�O���[�h���}�X�^�����o���Ȃ��ꍇ��NG
                    if (order.CarSalesHeader.CarGrade == null) {
                        ModelState.AddModelError("", "�O���[�h������o���Ȃ����ߔ����ł��܂���B");
                    }

                    //�x�������͕K�{����
                    if (order.PayDueDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].PayDueDate", i), MessageUtils.GetMessage("E0009", new string[] { "��������", "�x������" }));
                    } else if (!ModelState.IsValidField("PayDueDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].PayDueDate", i), MessageUtils.GetMessage("E0005", "�x������"));
                    }

                    //�d�����i�͕K�{����
                    if (order.Amount == null) {
                        ModelState.AddModelError(string.Format("data[{0}].Amount", i), MessageUtils.GetMessage("E0007", new string[] { "��������", "�ԗ��d�����i" }));
                    }

                    //�������ɊǗ��ԍ��͓��͂���Ă�����}�X�^�`�F�b�N
                    //�}�X�^�ɑ��݂�����G���[
                    if (!string.IsNullOrEmpty(order.SalesCarNumber)) {
                        SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                        if (salesCar != null) {
                            ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "�w�肳�ꂽ�Ǘ��ԍ������ɑ��݂��邽�ߔ����ł��܂���");
                        } else {
                        //�}�X�^�ɑ��݂��Ȃ��Ă��A�����̔Ԃ͈̔͂Ɋ܂܂ꂽ��G���[
                            if (!new SerialNumberDao(db).CanUseSalesCarNumber(order.SalesCarNumber)) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "�w�肳�ꂽ�Ǘ��ԍ��͎����̔ԂŎg�p����͈͂Ɋ܂܂�邽�ߎg�p�ł��܂���");
                            }
                        }
                    }
                }
                #endregion

                #region ����������
                if (order.ReservationStatus!=null && order.ReservationStatus.Contains("true")) {
                    //�����𓯎��ɂ��Ȃ��ꍇ
                    if(order.PurchaseOrderStatus==null || !order.PurchaseOrderStatus.Contains("true")){
                        if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                            //�Ǘ��ԍ��K�{
                            ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), MessageUtils.GetMessage("E0001", "�Ǘ��ԍ�"));
                        } else {
                            //�Ǘ��ԍ��̓}�X�^�ɑ��݂��A�݌ɂł���K�v������
                            SalesCar car = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                            if (car == null) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "�w�肳�ꂽ�ԗ��̓}�X�^�ɑ��݂��܂���");
                            } else if (string.IsNullOrEmpty(car.LocationCode) || (car.CarStatus == null || !car.CarStatus.Equals("001"))) {
                                ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), "�w�肳�ꂽ�ԗ��͍݌ɂ��Ȃ������k���ł�");
                            }
                        }
                    }
                }
                #endregion

                #region �d���\��쐬��
                if (order.PurchasePlanStatus!=null && order.PurchasePlanStatus.Contains("true")) {
                    if ((order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) || (order.ReservationStatus!=null && order.ReservationStatus.Contains("true"))) {
                        ModelState.AddModelError(string.Format("data[{0}].PurchasePlanStatus", i), "����������Ɠ����Ɏd���\��͍쐬�ł��܂���");
                    }
                    if (string.IsNullOrEmpty(order.ArrivalLocationCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalLocationCode", i), MessageUtils.GetMessage("E0001", "���Ƀ��P�[�V����"));
                    }
                    if (order.ArrivalPlanDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalPlanDate", i), MessageUtils.GetMessage("E0001", "���ɗ\���"));
                    }
                    if (!ModelState.IsValidField("ArrivalPlanDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].ArrivalPlanDate", i), MessageUtils.GetMessage("E0002", "���ɗ\���"));
                    }
                    if (string.IsNullOrEmpty(order.SupplierCode)) {
                        ModelState.AddModelError(string.Format("data[{0}].SupplierCode", i), MessageUtils.GetMessage("E0001", "�d����R�[�h"));
                    }

                    if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                        ModelState.AddModelError(string.Format("data[{0}].SalesCarNumber", i), MessageUtils.GetMessage("E0001", "��������"));
                    }
                }
                #endregion

                #region �ԗ��o�^��
                if (order.RegistrationStatus!=null && order.RegistrationStatus.Contains("true")) {
                    if((order.PurchaseOrderStatus!=null && order.PurchaseOrderStatus.Contains("true")) || (order.ReservationStatus!=null && order.ReservationStatus.Contains("true"))){
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationStatus",i),"����������Ɠ����Ɏԗ��o�^�͂ł��܂���");
                    }
                    if (order.RegistrationDate == null) {
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationDate", i), MessageUtils.GetMessage("E0001", "�o�^��"));
                    }
                    if (!ModelState.IsValidField("RegistrationDate")) {
                        ModelState.AddModelError(string.Format("data[{0}].RegistrationDate", i), MessageUtils.GetMessage("E0003", "�o�^��"));
                    }

                }
                #endregion

            }
        }
        #endregion

        #region Sort
        /// <summary>
        /// �����˗����X�g�̃\�[�g
        /// </summary>
        /// <param name="data">�ԗ������˗����X�g�f�[�^</param>
        /// <param name="sortKey">�\�[�g�L�[</param>
        /// <returns></returns>
        private List<CarPurchaseOrder> SortData(List<CarPurchaseOrder> data, string sortKey, bool desc) {

            string[] keyArray = sortKey.Split('.');

            //LinqToSql�N�G�����Ń\�[�g����
            var query = from a in data select a;

            if (desc) {
                if (keyArray.Count() == 2) {
                    query = query.OrderByDescending(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null).GetType().GetProperty(keyArray[1]).GetValue(x.GetType().GetProperty(keyArray[0]).GetValue(x, null), null)).ThenBy(y => y.CarPurchaseOrderNumber);
                } else {
                    query = query.OrderByDescending(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null)).ThenBy(y => y.CarPurchaseOrderNumber);
                }
            } else {
                if (keyArray.Count() == 2) {
                    query = query.OrderBy(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null).GetType().GetProperty(keyArray[1]).GetValue(x.GetType().GetProperty(keyArray[0]).GetValue(x, null), null)).ThenBy(y => y.CarPurchaseOrderNumber);
                } else {
                    query = query.OrderBy(x => x.GetType().GetProperty(keyArray[0]).GetValue(x, null)).ThenBy(y => y.CarPurchaseOrderNumber);
                }
            }

            return query.ToList();
        }
        #endregion

        #endregion

        #region �ԗ�����
        /// <summary>
        /// �ԗ���������
        /// </summary>
        /// <param name="order">�����˗��f�[�^</param>
        /// <history>
        /// 2018/07/31 #3918 yano.hiroki #3918 �ԗ��`�[�̍ŏI�X�V�ҁA�ŏI�X�V�����X�V���鏈���̒ǉ�
        /// </history>
        private void CreatePurchaseOrder(CarPurchaseOrder order) {
            SalesCar salesCar = new SalesCar();

            salesCar.NewUsedType = "N";
            //�Ǘ��ԍ������͂Ȃ玩���̔�
            if (string.IsNullOrEmpty(order.SalesCarNumber)) {
                string companyCode = "N/A";
                try { companyCode = new CarGradeDao(db).GetByKey(order.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
                salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            } else {
                salesCar.SalesCarNumber = order.SalesCarNumber;
            }
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            if (order.CarSalesHeader != null) {
                salesCar.CarGradeCode = order.CarSalesHeader.CarGradeCode;
                salesCar.ExteriorColorCode = order.CarSalesHeader.ExteriorColorCode;
                salesCar.InteriorColorCode = order.CarSalesHeader.InteriorColorCode;
                
                // ���L�ҏ���A�g
                Customer owner = new CustomerDao(db).GetByKey(order.CarSalesHeader.PossesorCode);
                if (owner != null) {
                    salesCar.OwnerCode = order.CarSalesHeader.PossesorCode;
                    salesCar.PossesorName = owner.CustomerName;
                    salesCar.PossesorAddress = owner.Prefecture + owner.City + owner.Address1 + owner.Address2;
                }

                // �g�p�ҏ���A�g
                Customer user = new CustomerDao(db).GetByKey(order.CarSalesHeader.UserCode);
                if (user != null) {
                    salesCar.UserCode = user.CustomerCode;
                    salesCar.UserName = user.CustomerName;
                    salesCar.UserAddress = user.Prefecture + user.City + user.Address1 + user.Address2;
                } else {
                    // �g�p�҃R�[�h���w�肳��Ă��Ȃ���΁A�ڋq����A�g
                    Customer customer = new CustomerDao(db).GetByKey(order.CarSalesHeader.CustomerCode);
                    if (customer != null) {
                        salesCar.UserCode = customer.CustomerCode;
                        salesCar.UserName = customer.CustomerName;
                        salesCar.UserAddress = customer.Prefecture + customer.City + customer.Address1 + customer.Address2;
                    }
                }
                

            } else {
                salesCar.CarGradeCode = order.CarGradeCode;
                salesCar.ExteriorColorCode = order.ExteriorColorCode;
                salesCar.InteriorColorCode = order.InteriorColorCode;
            }
            //db.SalesCar.InsertOnSubmit(salesCar);
            order.SalesCar = salesCar;

            //������SalesCar�ƕR�Â��邽�߂�ID��}��
            if (order.CarSalesHeader != null) {
                order.CarSalesHeader.SalesCarNumber = salesCar.SalesCarNumber;

                //Add 2018/07/31 yano.hiroki #3918
                order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.CarSalesHeader.LastUpdateDate = DateTime.Now;
            }
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
        /// <param name="row">�s��</param>
        /// <history>
        /// 2020/08/29 yano #4061�y�ԗ��}�X�^�z���L�ҕύX���̎Ԍ��ē����̍X�V
        /// 2018/07/31 yano.hiroki #3918 �ԗ��`�[�̍ŏI�X�V�ҁA�ŏI�X�V�����X�V����悤�ɏC��
        /// </history>
        private void CreateReservation(CarPurchaseOrder order,bool purchaseOrderStatus) {

            //�X�e�[�^�X�X�V
            if (order.PurchaseOrderStatus != null && !order.PurchaseOrderStatus.Equals("1")) {
                order.PurchaseOrderStatus = "1";
            }

            //�ԗ��݌ɃX�e�[�^�X���X�V(������)
            if (order.SalesCar != null) {
                order.SalesCar.CarStatus = "003";

                // ���L�ҏ���A�g
                Customer owner = new CustomerDao(db).GetByKey(order.CarSalesHeader.PossesorCode);
                if (owner != null) {
                    order.SalesCar.OwnerCode = order.CarSalesHeader.PossesorCode;
                    order.SalesCar.PossesorName = owner.CustomerName;
                    order.SalesCar.PossesorAddress = owner.Prefecture + owner.City + owner.Address1 + owner.Address2;
                }

                // �g�p�ҏ���A�g
                Customer user = new CustomerDao(db).GetByKey(order.CarSalesHeader.UserCode);
                if (user != null) {
                    order.SalesCar.UserCode = user.CustomerCode;
                    order.SalesCar.UserName = user.CustomerName;
                    order.SalesCar.UserAddress = user.Prefecture + user.City + user.Address1 + user.Address2;
                } else {
                    // �g�p�҃R�[�h���w�肳��Ă��Ȃ���΁A�ڋq����A�g
                    Customer customer = new CustomerDao(db).GetByKey(order.CarSalesHeader.CustomerCode);
                    if (customer != null) {
                        order.SalesCar.UserCode = customer.CustomerCode;
                        order.SalesCar.UserName = customer.CustomerName;
                        order.SalesCar.UserAddress = customer.Prefecture + customer.City + customer.Address1 + customer.Address2;
                    }
                }

                //Add 2020/08/29 yano #4061
                //�Ԍ��ē������N���A����
                order.SalesCar.InspectGuidFlag = "001";
                order.SalesCar.InspectGuidMemo = "";

                // �Â��Ǘ��ԍ��𗚗��e�[�u���ɃR�s�[
                order.SalesCar.OwnershipChangeType = "099";
                order.SalesCar.OwnershipChangeDate = DateTime.Today;
                CommonUtils.CopyToSalesCarHistory(db, order.SalesCar);

            }

            //�����݂̂̏ꍇ�A�����Ŏԗ��`�[�ɊǗ��ԍ�������
            if (!purchaseOrderStatus && order.CarSalesHeader != null) {
                order.CarSalesHeader.SalesCarNumber = order.SalesCarNumber;
                order.CarSalesHeader.Vin = order.SalesCar != null ? order.SalesCar.Vin : "";

                //Add 2018/07/31 yano.hiroki #3918
                order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.CarSalesHeader.LastUpdateDate = DateTime.Now;
                
            }

            //�󒍈����̏ꍇ�݈̂����˗��^�X�N�������ɂ���
            if (!string.IsNullOrEmpty(order.SlipNumber)) {
                List<Task> requestTaskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_RESERVATION_REQUEST, order.SlipNumber);
                foreach (var a in requestTaskList) {
                    a.TaskCompleteDate = DateTime.Now;
                }
                //�����m�F�^�X�N
                TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                task.ReserveConfirm(order);
            }

        }
        #endregion

        #region �d���\��쐬
        /// <summary>
        /// �d���\��쐬����
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
        /// <param name="row">�s�ԍ�</param>
        /// <history>
        /// 2018/04/10 arc yano #3879 �ԗ��`�[�@���P�[�V�����}�X�^�ɕ���R�[�h��ݒ肵�Ă��Ȃ��ƁA�[�ԏ������s���Ȃ� �V�K�쐬
        /// </history>
        private void CreatePurchasePlan(CarPurchaseOrder order) {
            //�ԗ��d���f�[�^INSERT
            CarPurchase purchase = new CarPurchase();
            purchase.CarPurchaseId = Guid.NewGuid();
            purchase.CarPurchaseOrderNumber = order.CarPurchaseOrderNumber;
            purchase.SalesCarNumber = order.SalesCarNumber;
            purchase.SupplierCode = order.SupplierCode;
            purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchase.DelFlag = "0";
            purchase.CreateDate = DateTime.Now;

            //Mod 2018/04/10 arc yano #3879
            if (order.Location != null)
            {
                Department dep = CommonUtils.GetDepartmentFromWarehouse(db, order.Location.WarehouseCode).AsQueryable().FirstOrDefault();

                purchase.DepartmentCode = dep != null ? dep.DepartmentCode : "";
            }
            else
            {
                purchase.DepartmentCode = "";
            }

            //purchase.DepartmentCode = order.Location != null ? order.Location.DepartmentCode : "";
            purchase.PurchaseLocationCode = order.ArrivalLocationCode;
            purchase.PurchaseStatus = "001";
            purchase.Amount = order.Amount ?? 0m;
            purchase.DiscountPrice = order.DiscountAmount ?? 0m;
            purchase.VehiclePrice = order.VehiclePrice ?? 0m;
            purchase.FirmPrice = order.FirmMargin ?? 0m;
            purchase.MetallicPrice = order.MetallicPrice ?? 0m;
            purchase.OptionPrice = order.OptionPrice ?? 0m;

            //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            purchase.LastEditScreen = "000";

            db.CarPurchase.InsertOnSubmit(purchase);

            purchase.CarPurchaseOrder = order;

            //�ԗ��d���\��^�X�N�쐬
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.CarPurchasePlan(purchase);
        }
        #endregion

        #region �x���\��쐬
        /// <summary>
        /// �x���\��f�[�^�쐬
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
        private void CreatePaymentPlan(CarPurchaseOrder order) {

            Account account = new AccountDao(db).GetByUsageType("CP");

            PaymentPlan plan = new PaymentPlan();
            plan.PaymentPlanId = Guid.NewGuid();
            plan.CreateDate = DateTime.Now;
            plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.DelFlag = "0";
            plan.LastUpdateDate = DateTime.Now;
            plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //plan.Amount = (order.Amount ?? 0) - (order.DiscountAmount ?? 0);              //�x�����z���d������
            plan.Amount = order.Amount ?? 0;
            plan.PaymentableBalance = order.Amount ?? 0;
            plan.CompleteFlag = "0";
            plan.PaymentPlanDate = order.PayDueDate;                //�x������
            plan.SupplierPaymentCode = order.SupplierPaymentCode;   //�x����
            plan.SlipNumber = order.SlipNumber;
            plan.OccurredDepartmentCode = order.CarSalesHeader != null ? order.CarSalesHeader.DepartmentCode : null;
            plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("PaymentableDepartmentCode").Value;
            plan.AccountCode = account.AccountCode;
            plan.CarPurchaseOrderNumber = order.CarPurchaseOrderNumber;
            db.PaymentPlan.InsertOnSubmit(plan);
        }
        #endregion

        #region �ԗ��o�^����
        /// <summary>
        /// �ԗ��o�^����
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
        /// <param name="row">�s�ԍ�</param>
        /// <history>
        /// 2018/05/30 arc yano #3889 �T�[�r�X�`�[�������i����������Ȃ� �ގ��Ή�
        /// </history>
        private void UpdateRegistration(CarPurchaseOrder order) {

            //Add 2018/05/30 arc yano #3889
            CarSalesHeader newHeader = new CarSalesHeader();
            //�ԗ��`�[�̕����Ɗ����`�[�̍폜

            CopyCarSalesHeader(order.CarSalesHeader, ref newHeader);

            //�����f�[�^�ɕt���ւ�
            order.CarSalesHeader = newHeader;

            //�`�[�X�e�[�^�X��o�^�ς݂ɍX�V
            order.CarSalesHeader.SalesOrderStatus = "003";
            //
            /* Del 2018/05/30 arc yano #3889 �X�V�ł͂Ȃ��A�V�K�쐬�̂��ߍ폜
            // Add 2015/05/11 arc.ookubo #3195 �`�[�X�e�[�^�X�o�^�ς݂ɕύX�����l��ǂ���悤�ɂ���
            order.CarSalesHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            order.CarSalesHeader.LastUpdateDate = DateTime.Now;
            */
            //�ԗ��̓o�^�����X�V
            order.SalesCar.RegistrationDate = order.RegistrationDate;
            //�o�^�m�F�^�X�N
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.RegistrationConfirm(order);
        }
        #endregion

        #region �ʔ������
        /// <summary>
        /// �������͉��
        /// </summary>
        /// <param name="id">�����˗��ԍ�</param>
        /// <returns></returns>
        public ActionResult Entry2(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderEntry2", model);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="model">�ԗ������˗������f�[�^</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry2(CarPurchaseOrder model,FormCollection form) {
            ValidateForEntry2(model);
            if (form["actionType"].Equals("Order")) {
                ValidateForPurchaseOrder(model);
            }
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderEntry2", model);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);
                UpdateModel(target);

                // ��������
                if (form["actionType"].Equals("Order")) {
                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(target.SlipNumber);

                    // �����X�e�[�^�X���u�ρv�ɂ���
                    target.PurchaseOrderStatus = "1";

                    // �����X�e�[�^�X���u�ρv�ɂ���
                    target.ReservationStatus = "1";

                    // �d���\��X�e�[�^�X���u�ρv�ɂ���
                    target.PurchasePlanStatus = "1";

                    // �O���[�h�R�[�h�����蓖�Ă�
                    target.CarGradeCode = target.CarSalesHeader != null ? target.CarSalesHeader.CarGradeCode : "";

                    // ���������i�ԗ��}�X�^�쐬�j
                    CreatePurchaseOrder(target);

                    // �d���\��쐬
                    CreatePurchasePlan(target);

                    // �x���\��쐬
                    CreatePaymentPlan(target);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
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
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            return View("CarPurchaseOrderEntry2", model);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HACCHU, FORM_NAME, target.SlipNumber);
                        return View("Error");
                    }
                } 
            }
            ViewData["close"] = "1";
            return View("CarPurchaseOrderEntry2", model);
        }
        #endregion

        #region �ʈ�������
        /// <summary>
        /// �ʈ������͉��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ReservationEntry(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderReservationEntry", model);
        }

        /// <summary>
        /// �ʈ�������
        /// </summary>
        /// <param name="model"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ReservationEntry(CarPurchaseOrder model, FormCollection form) {
            ValidationForReservation(model);
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderReservationEntry", model);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);
                UpdateModel(target);
                
                if (form["actionType"].Equals("Reservation")) {

                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                    
                    // �����ς݂ɍX�V
                    target.ReservationStatus = "1";

                    // ��������
                    CreateReservation(target, false);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
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
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            return View("CarPurchaseOrderReservationEntry", model);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                        return View("Error");
                    }
                }
            }
            ViewData["close"] = "1";
            return View("CarPurchaseOrderReservationEntry", model);
        }
        #endregion

        #region �ʓo�^����
        /// <summary>
        /// �ʓo�^���
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult RegistrationEntry(string id) {
            CarPurchaseOrder model = new CarPurchaseOrderDao(db).GetByKey(id);
            SetDataComponent(model);
            return View("CarPurchaseOrderRegistrationEntry", model);
        }

        /// <summary>
        /// �ʓo�^����
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <hisotry>
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// </hisotry>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RegistrationEntry(CarPurchaseOrder model, FormCollection form) {
            ValidationForRegistrationEntry(model);
            if (form["actionType"].Equals("Registration")) {
                ValidationForRegistration(model);
            }
            if (!ModelState.IsValid) {
                SetDataComponent(model);
                return View("CarPurchaseOrderRegistrationEntry", model);
            }

            // Add 2014/09/25 arc amii ����a��Ή� #3082 �o�^�\����Ɠo�^����a�����ɕϊ�����
            // �o�^�\������ڂ̘a��𐼗�ɕϊ�
            if (!model.RegistrationPlanDateWareki.IsNull)
            {
                model.RegistrationPlanDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationPlanDateWareki, db); //Mod 2018/06/22 arc yano #3891
                //model.RegistrationPlanDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationPlanDateWareki);
            }


            // �o�^�����ڂ̘a��𐼗�ɕϊ�
            if (!model.RegistrationDateWareki.IsNull)
            {
                model.RegistrationDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //model.RegistrationDate = JapaneseDateUtility.GetGlobalDate(model.RegistrationDateWareki);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                CarPurchaseOrder target = new CarPurchaseOrderDao(db).GetByKey(model.CarPurchaseOrderNumber);

                // Add 2014/09/25 arc amii ����a��Ή� #3082 ����ϊ����ꂽ�o�^�\����Ɠo�^����ݒ�
                target.RegistrationPlanDate = model.RegistrationPlanDate;
                target.RegistrationDate = model.RegistrationDate;

                UpdateModel(target);

                if (form["actionType"].Equals("Registration")) {

                    target.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);

                    // �o�^�ς݂ɍX�V
                    target.RegistrationStatus = "1";

                    // �o�^����
                    UpdateRegistration(target);

                    EditForUpdate(target);
                }

                //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
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
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(cfe, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(e, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetDataComponent(model);
                            return View("CarPurchaseOrderRegistrationEntry", model);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_TOROKU, FORM_NAME, model.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_HIKIATE, FORM_NAME, model.SlipNumber);
                        return View("Error");
                    }
                }
            }
            SetDataComponent(model);
            ViewData["close"] = "1";
            return View("CarPurchaseOrderRegistrationEntry", model);
        }
        #endregion

        /// <summary>
        /// �������̓f�[�^�R���|�[�l���g�̍쐬
        /// </summary>
        /// <param name="model"></param>
        private void SetDataComponent(CarPurchaseOrder model) {
            if (!string.IsNullOrEmpty(model.SlipNumber)) {
                model.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                model.ReceiptAmount = new JournalDao(db).GetTotalBySlipNumber(model.SlipNumber);
            }
            try { ViewData["EmployeeName"] = new EmployeeDao(db).GetByKey(model.EmployeeCode).EmployeeName; } catch { }
            try { ViewData["SupplierName"] = new SupplierDao(db).GetByKey(model.SupplierCode).SupplierName; } catch { }
            try { ViewData["SupplierPaymentName"] = new SupplierPaymentDao(db).GetByKey(model.SupplierPaymentCode).SupplierPaymentName; } catch { }
            try { ViewData["Vin"] = new SalesCarDao(db).GetByKey(model.SalesCarNumber).Vin; } catch { }
            //Add 2014/09/09 arc amii ����a��Ή� #3082 �o�^�\����Ɠo�^���̃R���|�[�l���g��ݒ�
            // �o�^�\���
            model.RegistrationPlanDateWareki = JapaneseDateUtility.GetJapaneseDate(model.RegistrationPlanDate);
            string registPlanDateGengou = "";
            if (model.RegistrationPlanDate != null)
            {
                registPlanDateGengou = model.RegistrationPlanDateWareki.Gengou.ToString();
            }

            ViewData["RegistrationPlanGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registPlanDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationPlanGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registPlanDateGengou, false);

            // �o�^��
            model.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(model.RegistrationDate);
            string registDateGengou = "";
            if (model.RegistrationDate != null)
            {
                registDateGengou = model.RegistrationDateWareki.Gengou.ToString();
            }

            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registDateGengou, false);
            ViewData["SalesCarNumber"] = model.SalesCarNumber;
        }

        /// <summary>
        /// �X�V���̋��ʍ��ڕύX
        /// </summary>
        /// <param name="model"></param>
        private void EditForUpdate(CarPurchaseOrder model) {
            model.LastUpdateDate = DateTime.Now;
            model.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        }

        #region �ʏ����pValidation
        /// <summary>
        /// �������͕ۑ�����Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidateForEntry2(CarPurchaseOrder model) {
            CommonValidate("VehiclePrice", "�ԗ��{�̉��i", model, false);
            CommonValidate("DiscountAmount", "�f�B�X�J�E���g", model, false);
            CommonValidate("MetallicPrice", "���^���b�N", model, false);
            CommonValidate("OptionPrice", "�I�v�V����", model, false);
        }
        /// <summary>
        /// ������������Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidateForPurchaseOrder(CarPurchaseOrder model) {
            CommonValidate("PurchaseOrderDate", "������", model, true);
            CommonValidate("SupplierCode", "�d����", model, true);
            CommonValidate("ArrivalPlanDate", "���ɗ\���", model, true);
            CommonValidate("ArrivalLocationCode", "���Ƀ��P�[�V����", model, true);
            CommonValidate("SupplierPaymentCode", "�x����", model, true);
            CommonValidate("PayDueDate", "�x������", model, true);
            CommonValidate("Amount", "�d�����i", model, true);
            if (string.IsNullOrEmpty(model.SlipNumber) || new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber).CarGrade == null) {
                ModelState.AddModelError("", "�󒍓`�[�̃O���[�h������ł��Ȃ����ߔ����ł��܂���");
            }
            if (!string.IsNullOrEmpty(model.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                if (salesCar != null) {
                    ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�Ǘ��ԍ������ɑ��݂��邽�ߔ����ł��܂���");
                } else {
                    //�}�X�^�ɑ��݂��Ȃ��Ă��A�����̔Ԃ͈̔͂Ɋ܂܂ꂽ��G���[
                    if (!new SerialNumberDao(db).CanUseSalesCarNumber(model.SalesCarNumber)) {
                        ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�Ǘ��ԍ��͎����̔ԂŎg�p����͈͂Ɋ܂܂�邽�ߎg�p�ł��܂���");
                    }
                }
            }
        }

        /// <summary>
        /// �������ď�������Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForReservation(CarPurchaseOrder model) {
            if (string.IsNullOrEmpty(model.SalesCarNumber)) {
                //�Ǘ��ԍ��K�{
                ModelState.AddModelError("SalesCarNumber", MessageUtils.GetMessage("E0001", "�Ǘ��ԍ�"));
            } else {
                //�Ǘ��ԍ��̓}�X�^�ɑ��݂��A�݌ɂł���K�v������
                SalesCar car = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                if (car == null) {
                    ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�ԗ��̓}�X�^�ɑ��݂��܂���");
                } else if (string.IsNullOrEmpty(car.LocationCode) || (car.CarStatus == null || !car.CarStatus.Equals("001"))) {
                    ModelState.AddModelError("SalesCarNumber", "�w�肳�ꂽ�ԗ��͍݌ɂ��Ȃ������k���ł�");
                }
            }
        }

        /// <summary>
        /// �o�^��������Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForRegistration(CarPurchaseOrder model) {
            // Mod 2014/09/25 arc amii ����a��Ή� #3082 �o�^���̓��̓`�F�b�N�̕��@��ύX
            if (model.RegistrationDateWareki.IsNull == true)
            {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0001", "�o�^��"));
            }
            
        }

        /// <summary>
        /// �o�^�ۑ�����Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidationForRegistrationEntry(CarPurchaseOrder model) {
           
            // Mod 2014/09/25 arc amii ����a��Ή� #3082 �o�^�\����Ɠo�^���̓��̓`�F�b�N�̕��@��ύX
            // �o�^�\����̓��̓`�F�b�N
            if (checkjapaneseDate(model.RegistrationPlanDateWareki) == false)
            {
                ModelState.AddModelError("RegistrationPlanDateWareki.Year", MessageUtils.GetMessage("E0021", "�o�^�\���"));
            }
            // �o�^���̓��̓`�F�b�N
            if (checkjapaneseDate(model.RegistrationDateWareki) == false)
            {
                ModelState.AddModelError("RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "�o�^��"));
            }

            if (!ModelState.IsValidField("DocumentPurchaseRequestDate")) {
                ModelState.AddModelError("DocumentPurchaseRequestDate", MessageUtils.GetMessage("E0003", "���ލw����]��"));
            }
            if (!ModelState.IsValidField("DocumentReceiptPlanDate")) {
                ModelState.AddModelError("DocumentReceiptPlanDate", MessageUtils.GetMessage("E0003", "���ޓ����\���"));
            }
            if (!ModelState.IsValidField("DocumentReceiptDate")) {
                ModelState.AddModelError("DocumentReceiptDate", MessageUtils.GetMessage("E0003", "���ޓ�����"));
            }

            //Add 2017/02/03 arc nakayama #3481_�ԗ��`�[�̐V���敪�̊ԈႦ�Ȃ��悤�ɂ���
            //�o�^����ԗ��Ɠ`�[�̎ԗ��̐V���敪���s��v�������ꍇ�G���[
            if (!string.IsNullOrEmpty(model.SlipNumber))
            {
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(model.SlipNumber);
                if (header != null)
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(model.SalesCarNumber);
                    if (SalesCarData != null)
                    {
                        if (header.NewUsedType != SalesCarData.NewUsedType)
                        {
                            ModelState.AddModelError("", "�ԗ��`�[�̐V���敪�Ɠo�^���s���ԗ��̐V���敪����v���Ă��܂���B");
                        }
                    }
                }
            }
        }

        // Add 2014/09/25 arc amii ����a��Ή� #3082 �a��ڂ̃`�F�b�N���s��������ǉ�
        /// <summary>
        /// �a��ڂ̓��̓`�F�b�N
        /// </summary>
        /// <param name="jaDateWareki"></param>
        /// <returns>true:����@false:�G���[</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// </history>
        private bool checkjapaneseDate(JapaneseDate jaDateWareki)
        {
            // �N�������S�Ė����͂̏ꍇ�A�G���[�Ƃ��Ȃ�
            if (jaDateWareki.Year == null && jaDateWareki.Month == null && jaDateWareki.Day == null)
            {
                return true;
            }

            // ���t�ϊ��ł��Ȃ��ꍇ�G���[�Ƃ���
            if (JapaneseDateUtility.GlobalDateTryParse(jaDateWareki, db) == false)  //Mod 2018/06/22 arc yano #3891
            //if (JapaneseDateUtility.GlobalDateTryParse(jaDateWareki) == false)
            {
                return false;
            }
            return true;
            
        }
    #endregion

    /// <summary>
    /// �����̎ԗ��`�[�`�[�𕡐����Ċ����̎ԗ��`�[���폜
    /// </summary>
    /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
    /// <returns></returns>
    /// <history>
    /// 2023/09/28 yano #4183 �C���{�C�X�Ή�(�o���Ή�)
    /// 2023/09/18 yano #4181�y�ԗ��`�[�z�I�v�V�����敪�ǉ��i�T�[�`���[�W�j
    /// 2023/08/15 yano #4176 �̔�����p�̏C��
    /// 2022/06/23 yano #4140�y�ԗ��`�[���́z�������̓o�^���`�l���\������Ȃ��s��̑Ή�
    /// 2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�ǉ�(�����e�i�X�E�����ۏ�)
    /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
    /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
    /// 2018/05/30 arc yano #3889 �T�[�r�X�`�[�������i����������Ȃ� �ގ�����
    /// </history>
    private void CopyCarSalesHeader(CarSalesHeader header, ref CarSalesHeader newHeader)
        {
            EntitySet<CarSalesLine> newLines = new EntitySet<CarSalesLine>();

            EntitySet<CarSalesPayment> newPayments = new EntitySet<CarSalesPayment>();

            //-------------------------
            //�ԗ��`�[�x�����ׂ̃R�s�[
            //-------------------------
            foreach (var p in header.CarSalesPayment)
            {
                CarSalesPayment payment = new CarSalesPayment();

                payment.SlipNumber = p.SlipNumber;
                payment.RevisionNumber = p.RevisionNumber + 1;
                payment.LineNumber = p.LineNumber;
                payment.CustomerClaimCode = p.CustomerClaimCode;
                payment.PaymentPlanDate = p.PaymentPlanDate;
                payment.Amount = p.Amount;
                payment.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                payment.CreateDate = DateTime.Now;
                payment.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                payment.LastUpdateDate = DateTime.Now;
                payment.DelFlag = "0";
                payment.Memo = p.Memo;

                db.CarSalesPayment.InsertOnSubmit(payment);
                
                newPayments.Add(payment);

                //�����f�[�^�͍폜
                p.DelFlag = "1";
                p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                p.LastUpdateDate = DateTime.Now;
            }

            //------------------------
            //�ԗ��`�[���ׂ̃R�s�[
            //------------------------
            foreach (var l in header.CarSalesLine)
            {
                CarSalesLine line = new CarSalesLine();

                line.SlipNumber = l.SlipNumber;
                line.RevisionNumber = l.RevisionNumber + 1;
                line.LineNumber = l.LineNumber;
                line.CarOptionCode = l.CarOptionCode;
                line.CarOptionName = l.CarOptionName;
                line.OptionType = l.OptionType;
                line.Amount = l.Amount;
                line.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                line.CreateDate = DateTime.Now;
                line.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                line.LastUpdateDate = DateTime.Now;
                line.DelFlag = "0";
                line.TaxAmount = l.TaxAmount;
                line.ConsumptionTaxId = l.ConsumptionTaxId;
                line.Rate = l.Rate;

                db.CarSalesLine.InsertOnSubmit(line);
  
                newLines.Add(line);

                //�����f�[�^�͍폜
                l.DelFlag = "1";
                l.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                l.LastUpdateDate = DateTime.Now;
            }

            //------------------------------
            //�ԗ��`�[���ׂ̃w�b�_�̃R�s�[
            //------------------------------
            newHeader.SlipNumber = header.SlipNumber;
            newHeader.RevisionNumber = header.RevisionNumber + 1;
            newHeader.QuoteDate = header.QuoteDate;
            newHeader.QuoteExpireDate = header.QuoteExpireDate;
            newHeader.SalesOrderDate = header.SalesOrderDate;
            newHeader.SalesOrderStatus = header.SalesOrderStatus;
            newHeader.ApprovalFlag = header.ApprovalFlag;
            newHeader.SalesDate = header.SalesDate;
            newHeader.CustomerCode = header.CustomerCode;
            newHeader.DepartmentCode = header.DepartmentCode;
            newHeader.EmployeeCode = header.EmployeeCode;
            newHeader.CampaignCode1 = header.CampaignCode1;
            newHeader.CampaignCode2 = header.CampaignCode2;
            newHeader.NewUsedType = header.NewUsedType;
            newHeader.SalesType = header.SalesType;
            newHeader.MakerName = header.MakerName;
            newHeader.CarBrandName = header.CarBrandName;
            newHeader.CarName = header.CarName;
            newHeader.CarGradeName = header.CarGradeName;
            newHeader.CarGradeCode = header.CarGradeCode;
            newHeader.ManufacturingYear = header.ManufacturingYear;
            newHeader.ExteriorColorCode = header.ExteriorColorCode;
            newHeader.ExteriorColorName = header.ExteriorColorName;
            newHeader.InteriorColorCode = header.InteriorColorCode;
            newHeader.InteriorColorName = header.InteriorColorName;
            newHeader.Vin = header.Vin;
            newHeader.UsVin = header.UsVin;
            newHeader.ModelName = header.ModelName;
            newHeader.Mileage = header.Mileage;
            newHeader.MileageUnit = header.MileageUnit;
            newHeader.RequestPlateNumber = header.RequestPlateNumber;
            newHeader.RegistPlanDate = header.RegistPlanDate;
            newHeader.HotStatus = header.HotStatus;
            newHeader.SalesCarNumber = header.SalesCarNumber;
            newHeader.RequestRegistDate = header.RequestRegistDate;
            newHeader.SalesPlanDate = header.SalesPlanDate;
            newHeader.RegistrationType = header.RegistrationType;
            newHeader.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            newHeader.OwnershipReservation = header.OwnershipReservation;
            newHeader.CarLiabilityInsuranceType = header.CarLiabilityInsuranceType;
            newHeader.SealSubmitDate = header.SealSubmitDate;
            newHeader.ProxySubmitDate = header.ProxySubmitDate;
            newHeader.ParkingSpaceSubmitDate = header.ParkingSpaceSubmitDate;
            newHeader.CarLiabilityInsuranceSubmitDate = header.CarLiabilityInsuranceSubmitDate;
            newHeader.OwnershipReservationSubmitDate = header.OwnershipReservationSubmitDate;
            newHeader.Memo = header.Memo;
            newHeader.SalesPrice = header.SalesPrice;
            newHeader.DiscountAmount = header.DiscountAmount;
            newHeader.TaxationAmount = header.TaxationAmount;
            newHeader.TaxAmount = header.TaxAmount;
            newHeader.ShopOptionAmount = header.ShopOptionAmount;
            newHeader.ShopOptionTaxAmount = header.ShopOptionTaxAmount;
            newHeader.MakerOptionAmount = header.MakerOptionAmount;
            newHeader.MakerOptionTaxAmount = header.MakerOptionTaxAmount;
            newHeader.OutSourceAmount = header.OutSourceAmount;
            newHeader.OutSourceTaxAmount = header.OutSourceTaxAmount;
            newHeader.SubTotalAmount = header.SubTotalAmount;
            newHeader.CarTax = header.CarTax;
            newHeader.CarLiabilityInsurance = header.CarLiabilityInsurance;
            newHeader.CarWeightTax = header.CarWeightTax;
            newHeader.AcquisitionTax = header.AcquisitionTax;
            newHeader.InspectionRegistCost = header.InspectionRegistCost;
            newHeader.ParkingSpaceCost = header.ParkingSpaceCost;
            newHeader.TradeInCost = header.TradeInCost;
            newHeader.RecycleDeposit = header.RecycleDeposit;
            newHeader.RecycleDepositTradeIn = header.RecycleDepositTradeIn;
            newHeader.NumberPlateCost = header.NumberPlateCost;
            newHeader.RequestNumberCost = header.RequestNumberCost;
            newHeader.TradeInFiscalStampCost = header.TradeInFiscalStampCost;
            newHeader.TaxFreeFieldName = header.TaxFreeFieldName;
            newHeader.TaxFreeFieldValue = header.TaxFreeFieldValue;
            newHeader.TaxFreeTotalAmount = header.TaxFreeTotalAmount;
            newHeader.InspectionRegistFee = header.InspectionRegistFee;
            newHeader.ParkingSpaceFee = header.ParkingSpaceFee;
            newHeader.TradeInFee = header.TradeInFee;
            newHeader.PreparationFee = header.PreparationFee;
            newHeader.RecycleControlFee = header.RecycleControlFee;
            newHeader.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn;
            newHeader.RequestNumberFee = header.RequestNumberFee;
            newHeader.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount;
            newHeader.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount;
            newHeader.TradeInAppraisalFee = header.TradeInAppraisalFee;
            newHeader.FarRegistFee = header.FarRegistFee;
            newHeader.OutJurisdictionRegistFee = header.OutJurisdictionRegistFee;   //Add 2023/08/15 yano #4176
            newHeader.TradeInMaintenanceFee = header.TradeInMaintenanceFee;
            newHeader.InheritedInsuranceFee = header.InheritedInsuranceFee;
            newHeader.TaxationFieldName = header.TaxationFieldName;
            newHeader.TaxationFieldValue = header.TaxationFieldValue;
            newHeader.SalesCostTotalAmount = header.SalesCostTotalAmount;
            newHeader.SalesCostTotalTaxAmount = header.SalesCostTotalTaxAmount;
            newHeader.OtherCostTotalAmount = header.OtherCostTotalAmount;
            newHeader.CostTotalAmount = header.CostTotalAmount;
            newHeader.TotalTaxAmount = header.TotalTaxAmount;
            newHeader.GrandTotalAmount = header.GrandTotalAmount;
            newHeader.PossesorCode = header.PossesorCode;
            newHeader.UserCode = header.UserCode;
            newHeader.PrincipalPlace = header.PrincipalPlace;
            newHeader.VoluntaryInsuranceType = header.VoluntaryInsuranceType;
            newHeader.VoluntaryInsuranceCompanyName = header.VoluntaryInsuranceCompanyName;
            newHeader.VoluntaryInsuranceAmount = header.VoluntaryInsuranceAmount;
            newHeader.VoluntaryInsuranceTermFrom = header.VoluntaryInsuranceTermFrom;
            newHeader.VoluntaryInsuranceTermTo = header.VoluntaryInsuranceTermTo;
            newHeader.PaymentPlanType = header.PaymentPlanType;
            newHeader.TradeInAmount1 = header.TradeInAmount1;
            newHeader.TradeInTax1 = header.TradeInTax1;
            newHeader.TradeInUnexpiredCarTax1 = header.TradeInUnexpiredCarTax1;
            newHeader.TradeInRemainDebt1 = header.TradeInRemainDebt1;
            newHeader.TradeInAppropriation1 = header.TradeInAppropriation1;
            newHeader.TradeInRecycleAmount1 = header.TradeInRecycleAmount1;
            newHeader.TradeInMakerName1 = header.TradeInMakerName1;
            newHeader.TradeInCarName1 = header.TradeInCarName1;
            newHeader.TradeInClassificationTypeNumber1 = header.TradeInClassificationTypeNumber1;
            newHeader.TradeInModelSpecificateNumber1 = header.TradeInModelSpecificateNumber1;
            newHeader.TradeInManufacturingYear1 = header.TradeInManufacturingYear1;
            newHeader.TradeInInspectionExpiredDate1 = header.TradeInInspectionExpiredDate1;
            newHeader.TradeInMileage1 = header.TradeInMileage1;
            newHeader.TradeInMileageUnit1 = header.TradeInMileageUnit1;
            newHeader.TradeInVin1 = header.TradeInVin1;
            newHeader.TradeInRegistrationNumber1 = header.TradeInRegistrationNumber1;
            newHeader.TradeInUnexpiredLiabilityInsurance1 = header.TradeInUnexpiredLiabilityInsurance1;
            newHeader.TradeInAmount2 = header.TradeInAmount2;
            newHeader.TradeInTax2 = header.TradeInTax2;
            newHeader.TradeInUnexpiredCarTax2 = header.TradeInUnexpiredCarTax2;
            newHeader.TradeInRemainDebt2 = header.TradeInRemainDebt2;
            newHeader.TradeInAppropriation2 = header.TradeInAppropriation2;
            newHeader.TradeInRecycleAmount2 = header.TradeInRecycleAmount2;
            newHeader.TradeInMakerName2 = header.TradeInMakerName2;
            newHeader.TradeInCarName2 = header.TradeInCarName2;
            newHeader.TradeInClassificationTypeNumber2 = header.TradeInClassificationTypeNumber2;
            newHeader.TradeInModelSpecificateNumber2 = header.TradeInModelSpecificateNumber2;
            newHeader.TradeInManufacturingYear2 = header.TradeInManufacturingYear2;
            newHeader.TradeInInspectionExpiredDate2 = header.TradeInInspectionExpiredDate2;
            newHeader.TradeInMileage2 = header.TradeInMileage2;
            newHeader.TradeInMileageUnit2 = header.TradeInMileageUnit2;
            newHeader.TradeInVin2 = header.TradeInVin2;
            newHeader.TradeInRegistrationNumber2 = header.TradeInRegistrationNumber2;
            newHeader.TradeInUnexpiredLiabilityInsurance2 = header.TradeInUnexpiredLiabilityInsurance2;
            newHeader.TradeInAmount3 = header.TradeInAmount3;
            newHeader.TradeInTax3 = header.TradeInTax3;
            newHeader.TradeInUnexpiredCarTax3 = header.TradeInUnexpiredCarTax3;
            newHeader.TradeInRemainDebt3 = header.TradeInRemainDebt3;
            newHeader.TradeInAppropriation3 = header.TradeInAppropriation3;
            newHeader.TradeInRecycleAmount3 = header.TradeInRecycleAmount3;
            newHeader.TradeInMakerName3 = header.TradeInMakerName3;
            newHeader.TradeInCarName3 = header.TradeInCarName3;
            newHeader.TradeInClassificationTypeNumber3 = header.TradeInClassificationTypeNumber3;
            newHeader.TradeInModelSpecificateNumber3 = header.TradeInModelSpecificateNumber3;
            newHeader.TradeInManufacturingYear3 = header.TradeInManufacturingYear3;
            newHeader.TradeInInspectionExpiredDate3 = header.TradeInInspectionExpiredDate3;
            newHeader.TradeInMileage3 = header.TradeInMileage3;
            newHeader.TradeInMileageUnit3 = header.TradeInMileageUnit3;
            newHeader.TradeInVin3 = header.TradeInVin3;
            newHeader.TradeInRegistrationNumber3 = header.TradeInRegistrationNumber3;
            newHeader.TradeInUnexpiredLiabilityInsurance3 = header.TradeInUnexpiredLiabilityInsurance3;
            newHeader.TradeInTotalAmount = header.TradeInTotalAmount;
            newHeader.TradeInTaxTotalAmount = header.TradeInTaxTotalAmount;
            newHeader.TradeInUnexpiredCarTaxTotalAmount = header.TradeInUnexpiredCarTaxTotalAmount;
            newHeader.TradeInRemainDebtTotalAmount = header.TradeInRemainDebtTotalAmount;
            newHeader.TradeInAppropriationTotalAmount = header.TradeInAppropriationTotalAmount;
            newHeader.PaymentTotalAmount = header.PaymentTotalAmount;
            newHeader.PaymentCashTotalAmount = header.PaymentCashTotalAmount;
            newHeader.LoanPrincipalAmount = header.LoanPrincipalAmount;
            newHeader.LoanFeeAmount = header.LoanFeeAmount;
            newHeader.LoanTotalAmount = header.LoanTotalAmount;
            newHeader.LoanCodeA = header.LoanCodeA;
            newHeader.PaymentFrequencyA = header.PaymentFrequencyA;
            newHeader.PaymentTermFromA = header.PaymentTermFromA;
            newHeader.PaymentTermToA = header.PaymentTermToA;
            newHeader.BonusMonthA1 = header.BonusMonthA1;
            newHeader.BonusMonthA2 = header.BonusMonthA2;
            newHeader.FirstAmountA = header.FirstAmountA;
            newHeader.SecondAmountA = header.SecondAmountA;
            newHeader.BonusAmountA = header.BonusAmountA;
            newHeader.CashAmountA = header.CashAmountA;
            newHeader.LoanPrincipalA = header.LoanPrincipalA;
            newHeader.LoanFeeA = header.LoanFeeA;
            newHeader.LoanTotalAmountA = header.LoanTotalAmountA;
            newHeader.AuthorizationNumberA = header.AuthorizationNumberA;
            newHeader.FirstDirectDebitDateA = header.FirstDirectDebitDateA;
            newHeader.SecondDirectDebitDateA = header.SecondDirectDebitDateA;
            newHeader.LoanCodeB = header.LoanCodeB;
            newHeader.PaymentFrequencyB = header.PaymentFrequencyB;
            newHeader.PaymentTermFromB = header.PaymentTermFromB;
            newHeader.PaymentTermToB = header.PaymentTermToB;
            newHeader.BonusMonthB1 = header.BonusMonthB1;
            newHeader.BonusMonthB2 = header.BonusMonthB2;
            newHeader.FirstAmountB = header.FirstAmountB;
            newHeader.SecondAmountB = header.SecondAmountB;
            newHeader.BonusAmountB = header.BonusAmountB;
            newHeader.CashAmountB = header.CashAmountB;
            newHeader.LoanPrincipalB = header.LoanPrincipalB;
            newHeader.LoanFeeB = header.LoanFeeB;
            newHeader.LoanTotalAmountB = header.LoanTotalAmountB;
            newHeader.AuthorizationNumberB = header.AuthorizationNumberB;
            newHeader.FirstDirectDebitDateB = header.FirstDirectDebitDateB;
            newHeader.SecondDirectDebitDateB = header.SecondDirectDebitDateB;
            newHeader.LoanCodeC = header.LoanCodeC;
            newHeader.PaymentFrequencyC = header.PaymentFrequencyC;
            newHeader.PaymentTermFromC = header.PaymentTermFromC;
            newHeader.PaymentTermToC = header.PaymentTermToC;
            newHeader.BonusMonthC1 = header.BonusMonthC1;
            newHeader.BonusMonthC2 = header.BonusMonthC2;
            newHeader.FirstAmountC = header.FirstAmountC;
            newHeader.SecondAmountC = header.SecondAmountC;
            newHeader.BonusAmountC = header.BonusAmountC;
            newHeader.CashAmountC = header.CashAmountC;
            newHeader.LoanPrincipalC = header.LoanPrincipalC;
            newHeader.LoanFeeC = header.LoanFeeC;
            newHeader.LoanTotalAmountC = header.LoanTotalAmountC;
            newHeader.AuthorizationNumberC = header.AuthorizationNumberC;
            newHeader.FirstDirectDebitDateC = header.FirstDirectDebitDateC;
            newHeader.SecondDirectDebitDateC = header.SecondDirectDebitDateC;
            newHeader.CancelDate = header.CancelDate;
            newHeader.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
            newHeader.CreateDate = DateTime.Now;
            newHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
            newHeader.LastUpdateDate = DateTime.Now;
            newHeader.DelFlag = "0";
            newHeader.InspectionRegistFeeTax = header.InspectionRegistFeeTax;
            newHeader.ParkingSpaceFeeTax = header.ParkingSpaceFeeTax;
            newHeader.TradeInFeeTax = header.TradeInFeeTax;
            newHeader.PreparationFeeTax = header.PreparationFeeTax;
            newHeader.RecycleControlFeeTax = header.RecycleControlFeeTax;
            newHeader.RecycleControlFeeTradeInTax = header.RecycleControlFeeTradeInTax;
            newHeader.RequestNumberFeeTax = header.RequestNumberFeeTax;
            newHeader.CarTaxUnexpiredAmountTax = header.CarTaxUnexpiredAmountTax;
            newHeader.CarLiabilityInsuranceUnexpiredAmountTax = header.CarLiabilityInsuranceUnexpiredAmountTax;
            newHeader.TradeInAppraisalFeeTax = header.TradeInAppraisalFeeTax;
            newHeader.FarRegistFeeTax = header.FarRegistFeeTax;
            newHeader.OutJurisdictionRegistFeeTax = header.OutJurisdictionRegistFeeTax;   //Add 2023/08/15 yano #4176
            newHeader.TradeInMaintenanceFeeTax = header.TradeInMaintenanceFeeTax;
            newHeader.InheritedInsuranceFeeTax = header.InheritedInsuranceFeeTax;
            newHeader.TaxationFieldValueTax = header.TaxationFieldValueTax;
            newHeader.TradeInEraseRegist1 = header.TradeInEraseRegist1;
            newHeader.TradeInEraseRegist2 = header.TradeInEraseRegist2;
            newHeader.TradeInEraseRegist3 = header.TradeInEraseRegist3;
            newHeader.RemainAmountA = header.RemainAmountA;
            newHeader.RemainAmountB = header.RemainAmountB;
            newHeader.RemainAmountC = header.RemainAmountC;
            newHeader.RemainFinalMonthA = header.RemainFinalMonthA;
            newHeader.RemainFinalMonthB = header.RemainFinalMonthB;
            newHeader.RemainFinalMonthC = header.RemainFinalMonthC;
            newHeader.LoanRateA = header.LoanRateA;
            newHeader.LoanRateB = header.LoanRateB;
            newHeader.LoanRateC = header.LoanRateC;
            newHeader.SalesTax = header.SalesTax;
            newHeader.DiscountTax = header.DiscountTax;
            newHeader.TradeInPrice1 = header.TradeInPrice1;
            newHeader.TradeInPrice2 = header.TradeInPrice2;
            newHeader.TradeInPrice3 = header.TradeInPrice3;
            newHeader.TradeInRecycleTotalAmount = header.TradeInRecycleTotalAmount;
            newHeader.ConsumptionTaxId = header.ConsumptionTaxId;
            newHeader.Rate = header.Rate;
            newHeader.RevenueStampCost = header.RevenueStampCost;
            newHeader.TradeInCarTaxDeposit = header.TradeInCarTaxDeposit;
            newHeader.LastEditScreen = header.LastEditScreen;
            newHeader.PaymentSecondFrequencyA = header.PaymentSecondFrequencyA;
            newHeader.PaymentSecondFrequencyB = header.PaymentSecondFrequencyB;
            newHeader.PaymentSecondFrequencyC = header.PaymentSecondFrequencyC;
            newHeader.ProcessSessionId = header.ProcessSessionId;
            newHeader.EPDiscountTaxId = header.EPDiscountTaxId; //Add 2019/09/04 yano #4011
            newHeader.CostAreaCode = header.CostAreaCode;       //2020/01/06 yano #4029
            //Add 2021/06/09 yano #4091
            newHeader.MaintenancePackageAmount = header.MaintenancePackageAmount;
            newHeader.MaintenancePackageTaxAmount = header.MaintenancePackageTaxAmount;
            newHeader.ExtendedWarrantyAmount = header.ExtendedWarrantyAmount;
            newHeader.ExtendedWarrantyTaxAmount = header.ExtendedWarrantyTaxAmount;

            //Add 2022/06/23 yano #4140
            newHeader.TradeInHolderName1 = header.TradeInHolderName1;
            newHeader.TradeInHolderName2 = header.TradeInHolderName2;
            newHeader.TradeInHolderName3 = header.TradeInHolderName3;

            //Add 2023/09/18 yano #4181
            newHeader.SurchargeAmount = header.SurchargeAmount;             //���ʃT�[�`���[�W
            newHeader.SurchargeTaxAmount = header.SurchargeTaxAmount;       //���ʃT�[�`���[�W(�����)

            newHeader.SuspendTaxRecv = header.SuspendTaxRecv;               //����Œ����z   //Add 2023/09/28 yano #4183


            db.CarSalesHeader.InsertOnSubmit(newHeader);

            //�����f�[�^�͍폜
            header.DelFlag = "1";
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;

            db.SubmitChanges();
        }
    }
}
