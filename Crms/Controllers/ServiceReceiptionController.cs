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
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �T�[�r�X��t�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceReceiptionController : Controller {

        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�T�[�r�X��t";     // ��ʖ�
        private static readonly string PROC_NAME = "�T�[�r�X��t�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �ԗ��}�X�^�ǉ�����������
        /// </summary>
        private bool addSalesCar = false;

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceReceiptionController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �T�[�r�X��t������ʕ\��
        /// </summary>
        /// <returns>�T�[�r�X��t�������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �T�[�r�X��t������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X��t�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            PaginatedList<V_ServiceReceiptTarget> list;
            if (criteriaInit) {
                list = new PaginatedList<V_ServiceReceiptTarget>();
            } else {
                // �������ʃ��X�g�̎擾
                list = GetSearchResultList(form);
            }
            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerNameKana"]         = form["CustomerNameKana"];
            ViewData["CustomerName"]             = form["CustomerName"];
            ViewData["TelNumber"]                = form["TelNumber"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"]   = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"]   = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"]  = form["RegistrationNumberPlate"];
            ViewData["Vin"]                      = form["Vin"];
            ViewData["ModelName"]                = form["ModelName"];
            ViewData["FirstReceiptionDateFrom"]  = form["FirstReceiptionDateFrom"];
            ViewData["FirstReceiptionDateTo"]    = form["FirstReceiptionDateTo"];
            ViewData["LastReceiptionDateFrom"]   = form["LastReceiptionDateFrom"];
            ViewData["LastReceiptionDateTo"]     = form["LastReceiptionDateTo"];

            // �T�[�r�X��t������ʂ̕\��
            return View("ServiceReceiptionCriteria", list);
        }

        /// <summary>
        /// �T�[�r�X��t���͉�ʕ\��
        /// </summary>
        /// <param name="id">�T�[�r�X��t�Ώۏ��(�ڋq�R�[�h�{","�{�Ǘ��ԍ�)</param>
        /// <returns>�T�[�r�X��t���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            // ���f���f�[�^�̐���
            string[] idArr = CommonUtils.DefaultString(id, ",").Split(new string[] { "," }, StringSplitOptions.None);
            CustomerReceiption customerReceiption = new CustomerReceiption();
            customerReceiption.CustomerCode = idArr[0];
            customerReceiption.SalesCarNumber = idArr[1];
            customerReceiption.ReceiptionDate = DateTime.Now.Date;
            customerReceiption.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            customerReceiption.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.ArrivalPlanDate = DateTime.Now.Date;

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(customerReceiption);

            // �o��
            return View("ServiceReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// �T�[�r�X��t�o�^
        /// </summary>
        /// <param name="customerReceiption">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�T�[�r�X��t�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerReceiption customerReceiption, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/07 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            // ����������̎擾
            if (!string.IsNullOrEmpty(customerReceiption.CustomerCode) && !string.IsNullOrEmpty(customerReceiption.Vin)) {
                List<SalesCar> salesCarList = new SalesCarDao(db).GetListByReceiption(customerReceiption.CustomerCode, customerReceiption.Vin);
                if (salesCarList.Count == 0) {
                    customerReceiption.SalesCarNumber = string.Empty; //�Ǘ��ԍ��̃N���A
                    addSalesCar = true;
                }
            }

            // �f�[�^�`�F�b�N
            ValidateServiceReceiption(customerReceiption);
            if (!ModelState.IsValid) {
                GetEntryViewData(customerReceiption);
                return View("ServiceReceiptionEntry", customerReceiption);
            }

            // �e��f�[�^�o�^����
            using (TransactionScope ts = new TransactionScope()) {

                // �ԗ��}�X�^�o�^����
                if (addSalesCar) {
                    ValidateForInsert(customerReceiption);
                    if (!ModelState.IsValid) {
                        GetEntryViewData(customerReceiption);
                        return View("ServiceReceiptionEntry", customerReceiption);
                    }
                    customerReceiption.SalesCar = EditSalesCarForInsert(customerReceiption);
                    db.SalesCar.InsertOnSubmit(customerReceiption.SalesCar);
                }

                // �T�[�r�X��t�f�[�^�o�^����
                customerReceiption = EditCustomerReceiptionForInsert(customerReceiption);
                db.CustomerReceiption.InsertOnSubmit(customerReceiption);

                // �ڋq�}�X�^���X���X�V����
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    EditCustomerForUpdate(customer);
                }

                //�ԗ��}�X�^���s�����X�V����
                if (!addSalesCar && !string.IsNullOrEmpty(customerReceiption.SalesCarNumber)) {
                    SalesCar originalSalesCar = new SalesCarDao(db).GetByKey(customerReceiption.SalesCarNumber);
                    if (originalSalesCar != null) {
                        if (
                            ((string.IsNullOrEmpty(originalSalesCar.Vin) && string.IsNullOrEmpty(customerReceiption.Vin)) || originalSalesCar.Vin.Equals(customerReceiption.Vin))
                            && ((string.IsNullOrEmpty(originalSalesCar.MorterViecleOfficialCode) && string.IsNullOrEmpty(customerReceiption.MorterViecleOfficialCode)) || originalSalesCar.MorterViecleOfficialCode.Equals(customerReceiption.MorterViecleOfficialCode))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberKana) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberKana)) || originalSalesCar.RegistrationNumberKana.Equals(customerReceiption.RegistrationNumberKana))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberPlate) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberPlate)) || originalSalesCar.RegistrationNumberPlate.Equals(customerReceiption.RegistrationNumberPlate))
                            && ((string.IsNullOrEmpty(originalSalesCar.RegistrationNumberType) && string.IsNullOrEmpty(customerReceiption.RegistrationNumberType)) || originalSalesCar.RegistrationNumberType.Equals(customerReceiption.RegistrationNumberType))
                            && ((string.IsNullOrEmpty(originalSalesCar.CarGradeCode) && string.IsNullOrEmpty(customerReceiption.CarGradeCode)) || originalSalesCar.CarGradeCode.Equals(customerReceiption.CarGradeCode))
                            ) {
                            //�ԑ�ԍ��A���^�ǃR�[�h�A�o�^�ԍ��i��ʁA���ȁA�v���[�g�j�A�O���[�h���ύX����Ă��Ȃ��ꍇ
                            //�ԗ��}�X�^�̑��s��������t���̓��͒l�ōX�V
                            originalSalesCar.Mileage = customerReceiption.Mileage;
                        }
                    }
                }
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        // DB�A�N�Z�X�̎��s
                        db.SubmitChanges();
                        // �R�~�b�g
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ChangeConflictException�������̏�����ǉ�
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // Mod 2014/08/07 arc amii �G���[���O�Ή� SqlException�������A���O�o�͂��s��������ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveQuote") ? "���ύ쐬" : "�ۑ�")));
                            GetEntryViewData(customerReceiption);
                            return View("ServiceReceiptionEntry", customerReceiption);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/04 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                

                
            }
            if (form["action"].Equals("saveQuote")) {
                ViewData["url"] = "/ServiceSalesOrder/Entry/?customerCode="+customerReceiption.CustomerCode+"&employeeCode="+customerReceiption.EmployeeCode+"&salesCarNumber="+customerReceiption.SalesCarNumber+"&requestDetail="+HttpUtility.UrlEncode(customerReceiption.RequestDetail)+"&arrivalPlanDate="+HttpUtility.UrlEncode(string.Format("{0:yyyy/MM/dd}",customerReceiption.ArrivalPlanDate));
            }

            // ��ʃR���|�[�l���g�̃Z�b�g
            GetEntryViewData(customerReceiption);
            
            // ���[���
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            } else {
                ViewData["close"] = "1";
            }
            return View("ServiceReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="customerReceiption">���f���f�[�^</param>
        private void GetEntryViewData(CustomerReceiption customerReceiption) {

            // �ڋq���̎擾
            if (!string.IsNullOrEmpty(customerReceiption.CustomerCode)) {
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    try { ViewData["CustomerRankName"] = customer.c_CustomerRank.Name; } catch (NullReferenceException) { }
                    ViewData["CustomerName"] = customer.CustomerName;
                    ViewData["CustomerNameKana"] = customer.CustomerNameKana;
                    ViewData["Prefecture"] = customer.Prefecture;
                    ViewData["City"] = customer.City;
                    ViewData["Address1"] = customer.Address1;
                    ViewData["Address2"] = customer.Address2;
                }
            }

            // ���喼�̎擾
            if (!string.IsNullOrEmpty(customerReceiption.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(customerReceiption.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // �S���Җ��̎擾
            if (!string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(customerReceiption.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    ViewData["EmployeeNumber"] = employee.EmployeeNumber;
                }
            }

            // �C�x���g��1,2�̎擾
            CampaignDao campaignDao = new CampaignDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode1)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode1);
                if (campaign != null) {
                    ViewData["CampaignName1"] = campaign.CampaignName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode2)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode2);
                if (campaign != null) {
                    ViewData["CampaignName2"] = campaign.CampaignName;
                }
            }

            // �ԗ����̎擾
            if (!string.IsNullOrEmpty(customerReceiption.SalesCarNumber)) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(customerReceiption.SalesCarNumber);
                if (salesCar != null) {
                    customerReceiption.SalesCarNumber = salesCar.SalesCarNumber;
                    customerReceiption.CarGradeCode = salesCar.CarGradeCode;
                    customerReceiption.Mileage = salesCar.Mileage;
                    customerReceiption.MileageUnit = salesCar.MileageUnit;
                    customerReceiption.RegistrationNumberKana = salesCar.RegistrationNumberKana;
                    customerReceiption.RegistrationNumberPlate = salesCar.RegistrationNumberPlate;
                    customerReceiption.RegistrationNumberType = salesCar.RegistrationNumberType;
                    customerReceiption.Vin = salesCar.Vin;
                    customerReceiption.MorterViecleOfficialCode = salesCar.MorterViecleOfficialCode;
                    customerReceiption.RegistrationDate = salesCar.RegistrationDate;
                    customerReceiption.FirstRegistrationYear = salesCar.FirstRegistrationYear;

                    try { ViewData["MakerName"] = salesCar.CarGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
                    try { ViewData["CarName"] = salesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { ViewData["CarGradeName"] = salesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }

                }
                
            } else {
                // �O���[�h���̎擾
                if (!string.IsNullOrEmpty(customerReceiption.CarGradeCode)) {
                    CarGrade carGrade = new CarGradeDao(db).GetByKey(customerReceiption.CarGradeCode);
                    if (carGrade != null) {
                        try { ViewData["MakerName"] = carGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
                        try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                        ViewData["CarGradeName"] = carGrade.CarGradeName;
                    }
                }
            }

            

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["ReceiptionStateList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionStateAll(false), customerReceiption.ReceiptionState, true);
            ViewData["ReceiptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionTypeAll(false), customerReceiption.ReceiptionType, true);
            ViewData["VisitOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetVisitOpportunityAll(false), customerReceiption.VisitOpportunity, true);
            ViewData["RequestContentList"] = CodeUtils.GetSelectListByModel(dao.GetRequestContentAll(false), customerReceiption.RequestContent, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), customerReceiption.MileageUnit, false);
        }

        /// <summary>
        /// �T�[�r�X��t�Ώۃf�[�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X��t�Ώۃf�[�^�������ʃ��X�g</returns>
        private PaginatedList<V_ServiceReceiptTarget> GetSearchResultList(FormCollection form) {

            V_ServiceReceiptTargetDao v_ServiceReceiptTargetDao       = new V_ServiceReceiptTargetDao(db);
            V_ServiceReceiptTarget v_ServiceReceiptTargetCondition    = new V_ServiceReceiptTarget();
            v_ServiceReceiptTargetCondition.CustomerNameKana          = form["CustomerNameKana"];
            v_ServiceReceiptTargetCondition.CustomerName              = form["CustomerName"];
            v_ServiceReceiptTargetCondition.TelNumber                 = form["TelNumber"];
            v_ServiceReceiptTargetCondition.MorterViecleOfficialCode  = form["MorterViecleOfficialCode"];
            v_ServiceReceiptTargetCondition.RegistrationNumberType    = form["RegistrationNumberType"];
            v_ServiceReceiptTargetCondition.RegistrationNumberKana    = form["RegistrationNumberKana"];
            v_ServiceReceiptTargetCondition.RegistrationNumberPlate   = form["RegistrationNumberPlate"];
            v_ServiceReceiptTargetCondition.Vin                       = form["Vin"];
            v_ServiceReceiptTargetCondition.ModelName                 = form["ModelName"];
            v_ServiceReceiptTargetCondition.FirstReceiptionDateFrom   = CommonUtils.StrToDateTime(form["FirstReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.FirstReceiptionDateTo     = CommonUtils.StrToDateTime(form["FirstReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_ServiceReceiptTargetCondition.LastReceiptionDateFrom    = CommonUtils.StrToDateTime(form["LastReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.LastReceiptionDateTo      = CommonUtils.StrToDateTime(form["LastReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            return v_ServiceReceiptTargetDao.GetListByCondition(v_ServiceReceiptTargetCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Mod 2015/09/10 arc yano #3252 �T�[�r�X�`�[���͉�ʂ̃}�X�^�{�^���̋����Ή�(�ގ��Ή�)  ErrorSalesCar�̌^��SalesCar��List<SalesCar>�ɕύX
        /// <summary>
        /// �V�K�o�^���̃}�X�^���݃`�F�b�N
        /// </summary>
        /// <param name="salesCar"></param>
        private void ValidateForInsert(CustomerReceiption customerReceiption) { 
            List<SalesCar> list = new SalesCarDao(db).GetByVin(customerReceiption.Vin);
            if (list != null && list.Count > 0) {
                ModelState.AddModelError("Vin", "�ԑ�ԍ�:" + customerReceiption.Vin + "�͊��ɓo�^����Ă��܂�");
                ViewData["ErrorSalesCar"] = list;
            }
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="customerReceiption">�T�[�r�X��t�f�[�^</param>
        /// <returns>�T�[�r�X��t�f�[�^</returns>
        /// <history>
        /// 2018/04/26 arc yano #3816 �ԗ�������́@�Ǘ��ԍ���N/A�������Ă��܂�
        /// </history>
        private CustomerReceiption ValidateServiceReceiption(CustomerReceiption customerReceiption) {

            // �K�{�`�F�b�N(���t���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(customerReceiption.CustomerCode)) {
                ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0001", "�ڋq�R�[�h"));
            }
            if (customerReceiption.ReceiptionDate == null) {
                ModelState.AddModelError("ReceiptionDate", MessageUtils.GetMessage("E0003", "��t��"));
            }
            if (string.IsNullOrEmpty(customerReceiption.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            if (string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            if (addSalesCar)    //Mod 2018/04/26 arc yano #3816
            {
                if (string.IsNullOrEmpty(customerReceiption.CarGradeCode))
                {
                    ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "�ڋq�R�[�h�Ǝԑ�ԍ��̐V�����g�ݍ��킹�o�^", "�O���[�h�R�[�h" }));
                }
                else //�O���[�h�R�[�h�����͂���Ă����ꍇ�́A�}�X�^�`�F�b�N���s��
                {
                    CarGrade rec = new CarGradeDao(db).GetByKey(customerReceiption.CarGradeCode);

                    if (rec == null)
                    {
                        ModelState.AddModelError("CarGradeCode", "�ԗ��O���[�h�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ�����");
                    }
                }
            }
            /*
            if (addSalesCar && string.IsNullOrEmpty(customerReceiption.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "�ڋq�R�[�h�Ǝԑ�ԍ��̐V�����g�ݍ��킹�o�^", "�O���[�h�R�[�h" }));
            }
            */
            // �����`�F�b�N
            if (!ModelState.IsValidField("ArrivalPlanDate")) {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0005", "���ɓ�"));
            }
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
            }
            if (!ModelState.IsValidField("RegistrationDate")) {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "�o�^��"));
            }
            if (!string.IsNullOrEmpty(customerReceiption.FirstRegistrationYear)) {
                if (!Regex.IsMatch(customerReceiption.FirstRegistrationYear, "([0-9]{4})/([0-9]{2})")
                    && !Regex.IsMatch(customerReceiption.FirstRegistrationYear, "([0-9]{4}/[0-9]{1})")) {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^�N��"));
                }
                DateTime result;
                try {
                    DateTime.TryParse(customerReceiption.FirstRegistrationYear + "/01", out result);
                    if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
                        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^�N��"));
                        if (ModelState["FirstRegistrationYear"].Errors.Count() > 1) {
                            ModelState["FirstRegistrationYear"].Errors.RemoveAt(0);
                        }
                    }
                } catch {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^�N��"));
                }

            }
            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("Mileage") && customerReceiption.Mileage != null) {
                if ((Regex.IsMatch(customerReceiption.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(customerReceiption.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }

            // �l�`�F�b�N
            /*if (ModelState.IsValidField("ArrivalPlanDate") && (customerReceiption.ArrivalPlanDate != null)) {
                if (DateTime.Compare(customerReceiption.ArrivalPlanDate ?? DateTime.MaxValue , DateTime.Now.Date) < 0) {
                    ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0013", new string[] { "���ɓ�", "�{���ȍ~" }));
                }
            }*/

            return customerReceiption;
        }

        /// <summary>
        /// �ڋq��t�e�[�u���ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="customerReceiption">�ڋq��t�e�[�u���f�[�^(�o�^���e)</param>
        /// <returns>�ڋq��t�e�[�u�����f���N���X</returns>
        private CustomerReceiption EditCustomerReceiptionForInsert(CustomerReceiption customerReceiption) {

            customerReceiption.CarReceiptionId = Guid.NewGuid();
            customerReceiption.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.CreateDate = DateTime.Now;
            customerReceiption.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerReceiption.LastUpdateDate = DateTime.Now;
            customerReceiption.DelFlag = "0";
            return customerReceiption;
        }

        /// <summary>
        /// �ԗ��}�X�^�ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="customerReceiption">�ڋq��t�e�[�u���f�[�^</param>
        /// <returns>�ԗ��}�X�^���f���N���X</returns>
        private SalesCar EditSalesCarForInsert(CustomerReceiption customerReceiption) {

            SalesCar salesCar = new SalesCar();
            salesCar.CarGradeCode = customerReceiption.CarGradeCode;
            salesCar.NewUsedType = "U"; // �V���敪:���Î�
            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            salesCar.Mileage = customerReceiption.Mileage;
            salesCar.MileageUnit = customerReceiption.MileageUnit;
            //salesCar.OwnerCode = customerReceiption.CustomerCode;
            salesCar.UserCode = customerReceiption.CustomerCode;
            salesCar.Vin = customerReceiption.Vin;
            salesCar.MorterViecleOfficialCode = customerReceiption.MorterViecleOfficialCode;
            salesCar.RegistrationNumberType = customerReceiption.RegistrationNumberType;
            salesCar.RegistrationNumberKana = customerReceiption.RegistrationNumberKana;
            salesCar.RegistrationNumberPlate = customerReceiption.RegistrationNumberPlate;
            salesCar.RegistrationDate = customerReceiption.RegistrationDate;
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            return salesCar;
        }

        /// <summary>
        /// �ڋq�}�X�^�X�V�f�[�^�ҏW(���X���̍X�V)
        /// </summary>
        /// <param name="customerReceiption">�ڋq�}�X�^�f�[�^</param>
        /// <returns>�ڋq�}�X�^���f���N���X</returns>
        private Customer EditCustomerForUpdate(Customer customer) {

            if (customer.FirstReceiptionDate == null) {
                customer.FirstReceiptionDate = DateTime.Now.Date;
            }
            customer.LastReceiptionDate = DateTime.Now.Date;
            customer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.LastUpdateDate = DateTime.Now;
            return customer;
        }

        public ActionResult History(string customerCode, string salesCarNumber,FormCollection form) {

            ServiceReceiptionHistory history = new ServiceReceiptionHistory();
            history.Customer = new CustomerDao(db).GetByKey(customerCode);
            history.SalesCar = new SalesCarDao(db).GetByKey(salesCarNumber);

            ServiceSalesHeader condition = new ServiceSalesHeader();
            condition.Customer = new Customer();
            condition.Customer.CustomerCode = customerCode;
            condition.SalesCarNumber = salesCarNumber;
            condition.DelFlag = "0";
            history.ServiceSalesHeader = new ServiceSalesOrderDao(db).GetListByCondition(condition, (Employee)Session["Employee"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            //history.V_ServiceReceiptionHistory = new V_ServiceReceiptionHistoryDao(db).GetListByCondition(customerCode, salesCarNumber);
            return View("ServiceReceiptionHistory", history);
        }
    }
}
