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
    /// �X�܎�t�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarReceiptionController : Controller {

        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�X�܎�t����"; // ��ʖ�
        private static readonly string PROC_NAME = "�X�܎�t�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarReceiptionController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �X�܎�t������ʕ\��
        /// </summary>
        /// <returns>�X�܎�t�������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �X�܎�t������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�X�܎�t�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �������ʃ��X�g�̎擾
            PaginatedList<V_ServiceReceiptTarget> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            //ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["Vin"] = form["Vin"];
            //ViewData["CarName"] = form["CarName"];
            ViewData["ModelName"] = form["ModelName"];
            ViewData["FirstReceiptionDateFrom"] = form["FirstReceiptionDateFrom"];
            ViewData["FirstReceiptionDateTo"] = form["FirstReceiptionDateTo"];
            ViewData["LastReceiptionDateFrom"] = form["LastReceiptionDateFrom"];
            ViewData["LastReceiptionDateTo"] = form["LastReceiptionDateTo"];

            // �X�܎�t������ʂ̕\��
            return View("CarReceiptionCriteria", list);
        }

        /// <summary>
        /// ���X������ʕ\��
        /// </summary>
        /// <param name="id">�ڋq�R�[�h</param>
        /// <returns>���X�������</returns>
        public ActionResult History(string id) {

            // �������ʃ��X�g�̎擾
            List<CustomerReceiption> list = new CustomerReceiptionDao(db).GetHistoryByCustomer(id);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerCode"] = id;
            if (!string.IsNullOrEmpty(id)) {
                Customer customer = new CustomerDao(db).GetByKey(id);
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

            // ���X������ʂ̕\��
            return View("CarReceiptionHistory", list);
        }

        /// <summary>
        /// �X�܎�t���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ڋq�R�[�h</param>
        /// <returns>�X�܎�t���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            // ���f���f�[�^�̐���
            CustomerReceiption customerReceiption = new CustomerReceiption();
            customerReceiption.CustomerCode = id;
            customerReceiption.ReceiptionDate = DateTime.Now.Date;
            customerReceiption.QuestionnarieEntryDate = DateTime.Now.Date;
            customerReceiption.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            customerReceiption.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(customerReceiption);

            // �o��
            return View("CarReceiptionEntry", customerReceiption);
        }

        /// <summary>
        /// �X�܎�t�o�^
        /// </summary>
        /// <param name="customerReceiption">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�X�܎�t�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerReceiption customerReceiption, FormCollection form) {

            // �f�[�^�`�F�b�N
            ValidateCarReceiption(customerReceiption);
            if (!ModelState.IsValid) {
                GetEntryViewData(customerReceiption);
                return View("CarReceiptionEntry", customerReceiption);
            }

            // Add 2014/08/07 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �e��f�[�^�o�^����
            using (TransactionScope ts = new TransactionScope()) {

                // �X�܎�t�f�[�^�o�^����
                customerReceiption = EditCustomerReceiptionForInsert(customerReceiption);
                db.CustomerReceiption.InsertOnSubmit(customerReceiption);

                // �ڋq�}�X�^���X���X�V����
                Customer customer = new CustomerDao(db).GetByKey(customerReceiption.CustomerCode);
                if (customer != null) {
                    EditCustomerForUpdate(customer);
                }

                
                // DB�A�N�Z�X�̎��s
                try
                {
                    db.SubmitChanges();
                    // �R�~�b�g
                    ts.Complete();
                }
                catch (SqlException e)
                {
                    // Add 2014/08/07 arc amii �G���[���O�Ή� Exception���Ƀ��O�o�͏����ǉ�
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveQuote") ? "���ύ쐬" : "�ۑ�")));
                        GetEntryViewData(customerReceiption);
                        return View("CarReceiptionEntry", customerReceiption);
                    }
                    else
                    {
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // Add 2014/08/07 arc amii �G���[���O�Ή� ��L�ȊO��Exception���Ƀ��O�o�͂��鏈���ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/10/23 arc ishii �ۑ��{�^���Ή�
            // �o��
            if (form["action"].Equals("saveQuote")) {
                ModelState.AddModelError("", MessageUtils.GetMessage("I0003", "���ύ쐬"));
                ViewData["url"] = "/CarSalesOrder/Entry/?customerCode=" + customerReceiption.CustomerCode + "&employeeCode=" + customerReceiption.EmployeeCode;
            }
            
            ViewData["close"] = "1";
                        
            return Entry((string)null);
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
                    customerReceiption.Customer = customer;
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
                    customerReceiption.Department = department;
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // �S���Җ��̎擾
            if (!string.IsNullOrEmpty(customerReceiption.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(customerReceiption.EmployeeCode);
                if (employee != null) {
                    customerReceiption.Employee = employee;
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
            }

            // �C�x���g��1,2�̎擾
            CampaignDao campaignDao = new CampaignDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode1)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode1);
                if (campaign != null) {
                    customerReceiption.Campaign1 = campaign;
                    ViewData["CampaignName1"] = campaign.CampaignName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.CampaignCode2)) {
                Campaign campaign = campaignDao.GetByKey(customerReceiption.CampaignCode2);
                if (campaign != null) {
                    customerReceiption.Campaign2 = campaign;
                    ViewData["CampaignName2"] = campaign.CampaignName;
                }
            }

            // �����̂��鏤�i1�`4�̎擾
            CarDao carDao = new CarDao(db);
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar1)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar1);
                if (car != null) {
                    customerReceiption.Car1 = car;
                    ViewData["InterestedCarName1"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar2)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar2);
                if (car != null) {
                    customerReceiption.Car2 = car;
                    ViewData["InterestedCarName2"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar3)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar3);
                if (car != null) {
                    customerReceiption.Car3 = car;
                    ViewData["InterestedCarName3"] = car.CarName;
                }
            }
            if (!string.IsNullOrEmpty(customerReceiption.InterestedCar4)) {
                Car car = carDao.GetByKey(customerReceiption.InterestedCar4);
                if (car != null) {
                    customerReceiption.Car4 = car;
                    ViewData["InterestedCarName4"] = car.CarName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["ReceiptionTypeList"] = CodeUtils.GetSelectListByModel(dao.GetReceiptionTypeAll(false), customerReceiption.ReceiptionType, true);
            ViewData["PurposeList"] = CodeUtils.GetSelectListByModel(dao.GetPurposeAll(false), customerReceiption.Purpose, true);
            ViewData["VisitOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetVisitOpportunityAll(false), customerReceiption.VisitOpportunity, true);
            ViewData["KnowOpportunityList"] = CodeUtils.GetSelectListByModel(dao.GetKnowOpportunityAll(false), customerReceiption.KnowOpportunity, true);
            ViewData["AttractivePointList"] = CodeUtils.GetSelectListByModel(dao.GetAttractivePointAll(false), customerReceiption.AttractivePoint, true);
            ViewData["DemandList"] = CodeUtils.GetSelectListByModel(dao.GetDemandAll(false), customerReceiption.Demand, true);
            ViewData["QuestionnarieList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), customerReceiption.Questionnarie, true);
        }

        /// <summary>
        /// �ڋq�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ڋq�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<V_ServiceReceiptTarget> GetSearchResultList(FormCollection form) {

            V_ServiceReceiptTargetDao v_ServiceReceiptTargetDao = new V_ServiceReceiptTargetDao(db);
            V_ServiceReceiptTarget v_ServiceReceiptTargetCondition = new V_ServiceReceiptTarget();
            v_ServiceReceiptTargetCondition.CustomerNameKana = form["CustomerNameKana"];
            v_ServiceReceiptTargetCondition.CustomerName = form["CustomerName"];
            v_ServiceReceiptTargetCondition.TelNumber = form["TelNumber"];
            v_ServiceReceiptTargetCondition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];
            v_ServiceReceiptTargetCondition.RegistrationNumberType = form["RegistrationNumberType"];
            v_ServiceReceiptTargetCondition.RegistrationNumberKana = form["RegistrationNumberKana"];
            v_ServiceReceiptTargetCondition.RegistrationNumberPlate = form["RegistrationNumberPlate"];
            v_ServiceReceiptTargetCondition.Vin = form["Vin"];
            v_ServiceReceiptTargetCondition.ModelName = form["ModelName"];
            v_ServiceReceiptTargetCondition.FirstReceiptionDateFrom = CommonUtils.StrToDateTime(form["FirstReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.FirstReceiptionDateTo = CommonUtils.StrToDateTime(form["FirstReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_ServiceReceiptTargetCondition.LastReceiptionDateFrom = CommonUtils.StrToDateTime(form["LastReceiptionDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_ServiceReceiptTargetCondition.LastReceiptionDateTo = CommonUtils.StrToDateTime(form["LastReceiptionDateTo"], DaoConst.SQL_DATETIME_MIN);
            return v_ServiceReceiptTargetDao.GetListByCondition(v_ServiceReceiptTargetCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="customerReceiption">�X�܎�t�f�[�^</param>
        /// <returns>�X�܎�t�f�[�^</returns>
        private CustomerReceiption ValidateCarReceiption(CustomerReceiption customerReceiption) {

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

            // �����`�F�b�N
            if (!ModelState.IsValidField("QuestionnarieEntryDate")) {
                ModelState.AddModelError("QuestionnarieEntryDate", MessageUtils.GetMessage("E0005", "�A���P�[�g�L����"));
            }

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
    }
}
