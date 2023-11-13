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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �ڋq�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerController : Controller {

        //Add 2014/08/01 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ڋq�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�ڋq�}�X�^�o�^"; // ������

        #region ������
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CustomerController() {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region �ڋq����
        /// <summary>
        /// �ڋq������ʕ\��
        /// </summary>
        /// <returns>�ڋq�������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ڋq������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ڋq�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Customer> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), form["CustomerRank"], true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), form["CustomerType"], true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), form["Sex"], true);
            ViewData["BirthdayFrom"] = form["BirthdayFrom"];
            ViewData["BirthdayTo"] = form["BirthdayTo"];
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), form["Occupation"], true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), form["CarOwner"], true);
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];

            // �ڋq������ʂ̕\��
            return View("CustomerCriteria", list);
        }

        /// <summary>
        /// �ڋq�����_�C�A���O�\��
        /// </summary>
        /// <returns>�ڋq�����_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �ڋq�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ڋq������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CustomerCode"] = Request["CustomerCode"];
            form["CustomerRank"] = Request["CustomerRank"];
            form["CustomerKind"] = Request["CustomerKind"];
            form["CustomerName"] = Request["CustomerName"];
            form["CustomerType"] = Request["CustomerType"];
            form["Sex"] = Request["Sex"];
            form["BirthdayFrom"] = Request["BirthdayFrom"];
            form["BirthdayTo"] = Request["BirthdayTo"];
            form["Occupation"] = Request["Occupation"];
            form["CarOwner"] = Request["CarOwner"];
            form["TelNumber"] = Request["TelNumber"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Customer> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), form["CustomerRank"], true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), form["CustomerKind"], true);
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), form["CustomerType"], true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), form["Sex"], true);
            ViewData["BirthdayFrom"] = form["BirthdayFrom"];
            ViewData["BirthdayTo"] = form["BirthdayTo"];
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), form["Occupation"], true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), form["CarOwner"], true);
            ViewData["TelNumber"] = form["TelNumber"];

            // �ڋq������ʂ̕\��
            return View("CustomerCriteriaDialog", list);
        }
        #endregion

        #region ���͉��
        /// <summary>
        /// �ڋq�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ڋq�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�ڋq�}�X�^���͉��</returns>
        /// <remarks>2014/08/04 ���g�p</remarks>
        [AuthFilter]
        public ActionResult Entry(string id) {

            Customer customer;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                customer = new Customer();
                customer.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                customer.CustomerType = "001";
            }
                // �X�V�̏ꍇ
            else {
                ViewData["update"] = "1";
                customer = new CustomerDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(customer);

            //�����戶���̒ǉ�
            ViewData["NewCustomerClaimName"] = customer.CustomerClaim != null ? customer.CustomerClaim.CustomerClaimName : "";

            // �o��
            return View("CustomerEntry", customer);
        }

        /// <summary>
        /// �ڋq�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="customer">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ڋq�}�X�^���͉��</returns>
        /// <remarks>2014/08/04 ���g�p</remarks>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Customer customer, FormCollection form) {

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];
            ViewData["NewCustomerClaimName"] = form["NewCustomerClaimName"];
            ViewData["UpdateCustomerClaim"] = !string.IsNullOrEmpty(form["UpdateCustomerClaim"]) && form["UpdateCustomerClaim"].Contains("true") ? true : false;
            
            // �f�[�^�`�F�b�N
            ValidateCustomer(customer, form);
            if (!ModelState.IsValid) {
                GetEntryViewData(customer);
                return View("CustomerEntry", customer);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� ���O�t�@�C����SQL���o�͂��鏈���ǉ�
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            

            // �f�[�^�X�V����
            if (form["update"].Equals("1")) {
                // �f�[�^�ҏW�E�X�V
                Customer targetCustomer = new CustomerDao(db).GetByKey(customer.CustomerCode);
                UpdateModel(targetCustomer);
                EditCustomerForUpdate(targetCustomer);

                //������ɂ��R�s�[����`�F�b�N
                if (!string.IsNullOrEmpty(form["UpdateCustomerClaim"]) && form["UpdateCustomerClaim"].Contains("true")) {
                    if (targetCustomer.CustomerClaim != null) {
                        targetCustomer.CustomerClaim.CustomerClaimName = form["CustomerClaimName"];
                        targetCustomer.CustomerClaim.PostCode = customer.PostCode;
                        targetCustomer.CustomerClaim.Prefecture = customer.Prefecture;
                        targetCustomer.CustomerClaim.City = customer.City;
                        targetCustomer.CustomerClaim.Address1 = customer.Address1;
                        targetCustomer.CustomerClaim.Address2 = customer.Address2;
                        targetCustomer.CustomerClaim.TelNumber1 = customer.TelNumber;
                        targetCustomer.CustomerClaim.FaxNumber = customer.FaxNumber;
                    }
                }
            }
                // �f�[�^�ǉ�����
            else 
            {
                // �f�[�^�ҏW
                customer = EditCustomerForInsert(customer);
                
                //������̒ǉ�
                CustomerClaim customerClaim = CreateCustomerClaim(customer);
                if (customerClaim != null) {
                    db.CustomerClaim.InsertOnSubmit(customerClaim);
                    if (string.IsNullOrEmpty(customer.CustomerClaimCode)) {
                        customer.CustomerClaimCode = customerClaim.CustomerClaimCode;
                    }
                }
                // �f�[�^�ǉ�
                db.Customer.InsertOnSubmit(customer);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0011", "�ۑ�"));
                        GetEntryViewData(customer);
                        return View("CustomerEntry", customer);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // ��L�ȊO�̗�O�̏ꍇ�A�G���[���O�o�͂��A�G���[��ʂɑJ�ڂ���
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            
            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        #endregion

        #region Ajax
        /// <summary>
        /// �ڋq�R�[�h����ڋq�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ڋq�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Customer customer = new CustomerDao(db).GetByKey(code);
                if (customer != null) {
                    codeData.Code = customer.CustomerCode;
                    codeData.Name = customer.CustomerName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �ڋq�R�[�h����ڋq�ڍ׏����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ڋq�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retCustomer = new Dictionary<string, string>();
                Customer customer = new CustomerDao(db).GetByKey(code);
                if (customer != null) {
                    retCustomer.Add("CustomerCode", customer.CustomerCode);
                    retCustomer.Add("CustomerName", customer.CustomerName);
                    retCustomer.Add("CustomerAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("CustomerNameKana", customer.CustomerNameKana);
                    retCustomer.Add("Prefecture", customer.Prefecture);
                    retCustomer.Add("City", customer.City);
                    retCustomer.Add("Address1", customer.Address1);
                    retCustomer.Add("Address2", customer.Address2);
                    retCustomer.Add("OwnerCode", customer.CustomerCode);
                    retCustomer.Add("PossesorCode", customer.CustomerCode);
                    retCustomer.Add("PossesorName", customer.CustomerName);
                    retCustomer.Add("PossesorAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("UserCode", customer.CustomerCode);
                    retCustomer.Add("UserName", customer.CustomerName);
                    retCustomer.Add("UserAddress", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("PrincipalPlace", customer.Prefecture + customer.City + customer.Address1 + customer.Address2);
                    retCustomer.Add("CustomerRankName", (customer.c_CustomerRank == null ? null : customer.c_CustomerRank.Name));
                    retCustomer.Add("CustomerTelNumber", customer.TelNumber);
                    // Add 2014/09/26 arc amii �o�^���Z���Ċm�F�`�F�b�N�Ή� #3098 �T�[�r�X or �ԗ��`�[�Ōx���\���Ɏg�p����Z���Ċm�F�t���O�̎擾��ǉ�
                    retCustomer.Add("AddressReconfirm", CommonUtils.DefaultString(customer.AddressReconfirm));

                    retCustomer.Add("CustomerMemo", customer.Memo); //Add 2022/02/09 yano 

                    if (customer.AddressReconfirm == true)
                    {
                        //ViewData["ReconfirmMessage"] = "�Z�����Ċm�F���Ă�������";
                        retCustomer.Add("ReconfirmMessage", "�Z�����Ċm�F���Ă�������");
                    }
                    else
                    {
                        ViewData["ReconfirmMessage"] = "";
                        retCustomer.Add("ReconfirmMessage", "");
                    }

                }
                return Json(retCustomer);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �ڋq�R�[�h����ڋq�ڍ׏����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ڋq�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetailForCustomerIntegrate(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retCustomer = new Dictionary<string, string>();
                GetCustomerIntegrateDataResult customer = new CustomerDao(db).GetCustomerIntegrateData(code);
                if (customer != null)
                {
                    //retCustomer.Add("0", customer.CustomerCode);
                    retCustomer.Add("0", customer.CustomerName);
                    retCustomer.Add("1", customer.TelNumber);
                    retCustomer.Add("2", customer.MobileNumber);
                    retCustomer.Add("3", customer.PostCode);
                    retCustomer.Add("4", customer.Prefecture);
                    retCustomer.Add("5", customer.City);
                    retCustomer.Add("6", customer.Address1);
                    retCustomer.Add("7", customer.Address2);
                    if (customer.CarCnt != null)
                    {
                        retCustomer.Add("8", customer.CarCnt.ToString());
                    }
                    else
                    {
                        retCustomer.Add("8", "");
                    }
                    if (customer.ServiceCnt != null)
                    {
                        retCustomer.Add("9", customer.ServiceCnt.ToString());
                    }
                    else
                    {
                        retCustomer.Add("9", "");
                    }
                }
                return Json(retCustomer);
            }
            return new EmptyResult();
        }

        #endregion

        #region ��ʃR���|�[�l���g
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="customer">���f���f�[�^</param>
        /// <history>
        /// 2019/01/18 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���)
        /// </history>
        private void GetEntryViewData(Customer customer) {

            // �����於�̎擾
            if (!string.IsNullOrEmpty(customer.CustomerClaimCode)) {
                CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
                CustomerClaim customerClaim = customerClaimDao.GetByKey(customer.CustomerClaimCode);
                if (customerClaim != null) {
                    ViewData["CustomerClaimName"] = customerClaim.CustomerClaimName;
                }
            }

            // ���喼�̎擾
            DepartmentDao departmentDao = new DepartmentDao(db);
            if (!string.IsNullOrEmpty(customer.DepartmentCode)) {
                
                Department department = departmentDao.GetByKey(customer.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(customer.ServiceDepartmentCode)) {
                Department serviceDepartment = departmentDao.GetByKey(customer.ServiceDepartmentCode);
                if (serviceDepartment != null) {
                    ViewData["ServiceDepartmentName"] = serviceDepartment.DepartmentName;
                }
            }

            // �c�ƒS���ҁC�T�[�r�X�S���Җ��̎擾
            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employee;
            if (!string.IsNullOrEmpty(customer.CarEmployeeCode)) {
                employee = employeeDao.GetByKey(customer.CarEmployeeCode);
                if (employee != null) {
                    ViewData["CarEmployeeName"] = employee.EmployeeName;
                }
            }
            if (!string.IsNullOrEmpty(customer.ServiceEmployeeCode)) {
                employee = employeeDao.GetByKey(customer.ServiceEmployeeCode);
                if (employee != null) {
                    ViewData["ServiceEmployeeName"] = employee.EmployeeName;
                }
            }

            //�Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerRankList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerRankAll(false), customer.CustomerRank, true);
            ViewData["CustomerKindList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerKindAll(false), customer.CustomerKind, true);
            ViewData["CustomerTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerTypeAll(false), customer.CustomerType, true);
            ViewData["PaymentKindList"] = CodeUtils.GetSelectListByModel(dao.GetPaymentKindAll(false), customer.PaymentKind, true);
            ViewData["SexList"] = CodeUtils.GetSelectListByModel(dao.GetSexAll(false), customer.Sex, true);
            ViewData["OccupationList"] = CodeUtils.GetSelectListByModel(dao.GetOccupationAll(false), customer.Occupation, true);
            ViewData["CarOwnerList"] = CodeUtils.GetSelectListByModel(dao.GetCarOwnerAll(false), customer.CarOwner, true);
            //Mod 2015/01/08 arc nakayama �ڋqDM�w�E�����E�E�G�@DM�ۂ��E�s�̂ǂ��炩�ɂ���(���E�X�y�[�X�͍폜)
            ViewData["DmFlagList"] = CodeUtils.GetSelectListByModel(dao.GetAllowanceAll(false), customer.DmFlag, false);
            ViewData["CorporationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCorporationTypeAll(false), customer.CorporationType, true);

            ViewData["dm.CorporationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCorporationTypeAll(false), (customer.CustomerDM != null ? customer.CustomerDM.CorporationType : ""), true);   //Mod 2019/01/18 yano #3965

        }

        #endregion


        /// <summary>
        /// �ڋq�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ڋq�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Customer> GetSearchResultList(FormCollection form) {

            CustomerDao customerDao = new CustomerDao(db);
            Customer customerCondition = new Customer();
            customerCondition.CustomerCode = form["CustomerCode"];
            customerCondition.CustomerRank = form["CustomerRank"];
            customerCondition.CustomerKind = form["CustomerKind"];
            customerCondition.CustomerName = form["CustomerName"];
            customerCondition.CustomerNameKana = form["CustomerNameKana"];
            customerCondition.CustomerType = form["CustomerType"];
            customerCondition.Sex = form["Sex"];
            customerCondition.BirthdayFrom = CommonUtils.StrToDateTime(form["BirthdayFrom"], DaoConst.SQL_DATETIME_MAX);
            customerCondition.BirthdayTo = CommonUtils.StrToDateTime(form["BirthdayTo"], DaoConst.SQL_DATETIME_MIN);
            customerCondition.Occupation = form["Occupation"];
            customerCondition.CarOwner = form["CarOwner"];
            customerCondition.TelNumber = form["TelNumber"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                customerCondition.DelFlag = form["DelFlag"];
            }
            customerCondition.CustomerNameKana = form["CustomerNameKana"];
            return customerDao.GetListByCondition(customerCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        #region Validation
        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ڋq�f�[�^</returns>
        private Customer ValidateCustomer(Customer customer, FormCollection form) {

            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(customer.FirstName)) {
                ModelState.AddModelError("FirstName", MessageUtils.GetMessage("E0001", "�ڋq��1(��)"));
            }
            if (string.IsNullOrEmpty(customer.FirstNameKana)) {
                ModelState.AddModelError("FirstNameKana", MessageUtils.GetMessage("E0001", "�ڋq��1(���J�i)"));
            }
            if (string.IsNullOrEmpty(customer.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����" ));
            }
            if (string.IsNullOrEmpty(customer.ServiceDepartmentCode)) {
                ModelState.AddModelError("ServiceDepartmentCode", MessageUtils.GetMessage("E0001", "�T�[�r�X�S������"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Birthday")) {
                ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0005", "���N����"));
            }
            if (!ModelState.IsValidField("FirstReceiptionDate")) {
                ModelState.AddModelError("FirstReceiptionDate", MessageUtils.GetMessage("E0005", "���񗈓X��"));
            }
            if (!ModelState.IsValidField("LastReceiptionDate")) {
                ModelState.AddModelError("LastReceiptionDate", MessageUtils.GetMessage("E0005", "�O�񗈓X��"));
            }

            // �`���`�F�b�N
            // Add 2015/01/09 arc nakayama �ڋqDM�w�E�����C �o�^���̗X�֔ԍ��̓n�C�t���Ȃ��̂V�����A�n�C�t������̂W���ɂ���B�܂��A�V���Ńn�C�t���������Ă��Ȃ���΃n�C�t��������
            if (!string.IsNullOrEmpty(customer.PostCode)){
                if ((!Regex.IsMatch(customer.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customer.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("PostCode", MessageUtils.GetMessage("E0026", "�X�֔ԍ�"));
                }
            }

            // ADD 2016/02/08 ARC Mikami #3428_�ڋq�}�X�^_���N�����o���f�[�V�����`�F�b�N
            if (customer.Birthday.HasValue) {
                if (customer.Birthday >= DateTime.Parse("1753/01/01") && customer.Birthday <= DateTime.Parse("9999/12/31")) {
                } else {
                    ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0030", "���N����"));
                }
            }
            
            return customer;
        }

        /// <summary>
        /// ������̓��̓`�F�b�N
        /// </summary>
        /// <param name="customerClaim"></param>
        private void ValidateCustomerClaim(CustomerClaim customerClaim,EntitySet<CustomerClaimable> customerClaimable) {
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimName)) {
                ModelState.AddModelError("claim.CustomerClaimName", MessageUtils.GetMessage("E0001", "�����於"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimType)) {
                ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0001", "�������"));
            }
            //�N���W�b�g�܂��̓��[���Ȃ猈�ώ�ʕK�{
            if(customerClaim.CustomerClaimType!=null && (customerClaim.CustomerClaimType.Equals("003") || customerClaim.CustomerClaimType.Equals("004"))){
                if (customerClaimable == null || customerClaimable.Count() == 0) {
                    ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0007", new string[] { "�N���W�b�g�܂��̓��[��", "���ώ��" }));

                } else {
                    var targetClaimable =
                        from a in customerClaimable
                        where !string.IsNullOrEmpty(a.PaymentKindCode)
                        select a;

                    if (targetClaimable == null || targetClaimable.Count() == 0) {
                        ModelState.AddModelError("claim.CustomerClaimType", MessageUtils.GetMessage("E0007", new string[] { "�N���W�b�g�܂��̓��[��", "���ώ��" }));
                    }
                }
            }
            if (customerClaimable != null) {
                var query =
                    from a in customerClaimable
                    group a by a.PaymentKindCode into kind
                    select kind;
                var duplication = query.Where(x => x.Count() > 1);
                if (duplication == null || duplication.Count() > 0) {
                    ModelState.AddModelError("", "����x����ʂ������o�^����Ă��܂�");
                }
            }
            // �`���`�F�b�N
            // Add 2015/02/02 arc nakayama �ڋqDM�w�E�����C �o�^���̗X�֔ԍ��̓n�C�t���Ȃ��œo�^����
            if (!string.IsNullOrEmpty(customerClaim.PostCode))
            {
                if ((!Regex.IsMatch(customerClaim.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customerClaim.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("claim.PostCode", MessageUtils.GetMessage("E0026", "�X�֔ԍ�"));
                }
            }

        }

        /// <summary>
        /// �d����̓��̓`�F�b�N
        /// </summary>
        /// <param name="supplier">�d����</param>
        private void ValidateSupplier(Supplier supplier) {
            if (string.IsNullOrEmpty(supplier.SupplierName)) {
                ModelState.AddModelError("sup.SupplierName", MessageUtils.GetMessage("E0001", "�d���於"));
            }
            if (string.IsNullOrEmpty(supplier.OutsourceFlag)) {
                ModelState.AddModelError("sup.OutsourceFlag", MessageUtils.GetMessage("E0001", "�O���t���O"));
            }
            // �`���`�F�b�N
            // Add 2015/02/02 arc nakayama �ڋqDM�w�E�����C �o�^���̗X�֔ԍ��̓n�C�t���Ȃ��œo�^����
            if (!string.IsNullOrEmpty(supplier.PostCode))
            {
                if ((!Regex.IsMatch(supplier.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(supplier.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("sup.PostCode", MessageUtils.GetMessage("E0026", "�X�֔ԍ�"));
                }
            }
        }

        /// <summary>
        /// �x����̓��̓`�F�b�N
        /// </summary>
        /// <param name="payment"></param>
        private void ValidateSupplierPayment(SupplierPayment supplierPayment,FormCollection form) {

            // �K�{�`�F�b�N(���l�K�{���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentName)) {
                ModelState.AddModelError("pay.SupplierPaymentName", MessageUtils.GetMessage("E0001", "�x���於"));
            }
            if (string.IsNullOrEmpty(supplierPayment.SupplierPaymentType)) {
                ModelState.AddModelError("pay.SupplierPaymentType", MessageUtils.GetMessage("E0001", "�x������"));
            }
            if (string.IsNullOrEmpty(supplierPayment.PaymentType)) {
                ModelState.AddModelError("pay.PaymentType", MessageUtils.GetMessage("E0001", "�x���敪"));
            }
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            if ((CommonUtils.DefaultString(supplierPayment.PaymentType).Equals("003")) && (supplierPayment.PaymentDayCount == null))
            {
                ModelState.AddModelError("pay.PaymentDayCount", MessageUtils.GetMessage("E0008", new string[] { "�x���敪��n����", "����", "5�`240" }));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("pay.PaymentDay")) {
                ModelState.AddModelError("pay.PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "�x����", "0�`31" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod1")) {
                ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0004", new string[] { "�P�\����1", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod2")) {
                ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0004", new string[] { "�P�\����2", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod3")) {
                ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0004", new string[] { "�P�\����3", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod4")) {
                ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0004", new string[] { "�P�\����4", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod5")) {
                ModelState.AddModelError("pay.PaymentPeriod5", MessageUtils.GetMessage("E0004", new string[] { "�P�\����5", "0�ȊO�̐��̐���" }));
            }
            if (!ModelState.IsValidField("pay.PaymentPeriod6")) {
                ModelState.AddModelError("pay.PaymentPeriod6", MessageUtils.GetMessage("E0004", new string[] { "�P�\����6", "0�ȊO�̐��̐���" }));
            }

            // �t�H�[�}�b�g�`�F�b�N

            if (string.IsNullOrEmpty(form["pay.PaymentRate1"]) || (Regex.IsMatch(form["pay.PaymentRate1"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate1"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate1", MessageUtils.GetMessage("E0004", new string[] { "��������1", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate2"]) || (Regex.IsMatch(form["pay.PaymentRate2"], @"^\d{1,3}\.\d{1,3}$"))
                            || (Regex.IsMatch(form["pay.PaymentRate2"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate2", MessageUtils.GetMessage("E0004", new string[] { "��������2", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate3"]) || (Regex.IsMatch(form["pay.PaymentRate3"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate3"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate3", MessageUtils.GetMessage("E0004", new string[] { "��������3", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate4"]) || (Regex.IsMatch(form["pay.PaymentRate4"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate4"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate4", MessageUtils.GetMessage("E0004", new string[] { "��������4", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if ((string.IsNullOrEmpty(form["pay.PaymentRate5"]) || Regex.IsMatch(form["pay.PaymentRate5"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate5"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate5", MessageUtils.GetMessage("E0004", new string[] { "��������5", "���̐���3���ȓ�������3���ȓ�" }));
            }
            if (string.IsNullOrEmpty(form["pay.PaymentRate6"]) || (Regex.IsMatch(form["pay.PaymentRate6"], @"^\d{1,3}\.\d{1,3}$"))
                || (Regex.IsMatch(form["pay.PaymentRate6"], @"^\d{1,3}$"))) {
            } else {
                ModelState.AddModelError("pay.PaymentRate6", MessageUtils.GetMessage("E0004", new string[] { "��������6", "���̐���3���ȓ�������3���ȓ�" }));
            }


            //�P�\���������͂���Ă���ꍇ�A�����͓��͕K�{
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod1"]) && string.IsNullOrEmpty(form["pay.PaymentRate1"])) {
                ModelState.AddModelError("pay.PaymentRate1", MessageUtils.GetMessage("E0001", new string[] { "��������1" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod2"]) && string.IsNullOrEmpty(form["pay.PaymentRate2"])) {
                ModelState.AddModelError("pay.PaymentRate2", MessageUtils.GetMessage("E0001", new string[] { "��������2" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod3"]) && string.IsNullOrEmpty(form["pay.PaymentRate3"])) {
                ModelState.AddModelError("pay.PaymentRate3", MessageUtils.GetMessage("E0001", new string[] { "��������3" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod4"]) && string.IsNullOrEmpty(form["pay.PaymentRate4"])) {
                ModelState.AddModelError("pay.PaymentRate4", MessageUtils.GetMessage("E0001", new string[] { "��������4" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod5"]) && string.IsNullOrEmpty(form["pay.PaymentRate5"])) {
                ModelState.AddModelError("pay.PaymentRate5", MessageUtils.GetMessage("E0001", new string[] { "��������5" }));
            }
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod6"]) && string.IsNullOrEmpty(form["pay.PaymentRate6"])) {
                ModelState.AddModelError("pay.PaymentRate6", MessageUtils.GetMessage("E0001", new string[] { "��������6" }));
            }


            // �l�`�F�b�N
            if (ModelState.IsValidField("pay.PaymentDayCount")) {
                if (supplierPayment.PaymentDayCount < 5 || supplierPayment.PaymentDayCount > 240) {
                    ModelState.AddModelError("pay.PaymentDayCount", MessageUtils.GetMessage("E0004", new string[] { "����", "5�`240" }));
                }
            }
            if (ModelState.IsValidField("pay.PaymentDay")) {
                if (supplierPayment.PaymentDay < 0 || supplierPayment.PaymentDay > 31) {
                    ModelState.AddModelError("pay.PaymentDay", MessageUtils.GetMessage("E0004", new string[] { "�x����", "0�`31" }));
                }
            }

            //�O������̃`�F�b�N
            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (supplierPayment.PaymentPeriod1 >= supplierPayment.PaymentPeriod2) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", "�P�\����2�͗P�\����1���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (supplierPayment.PaymentPeriod2 >= supplierPayment.PaymentPeriod3) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", "�P�\����3�͗P�\����2���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (supplierPayment.PaymentPeriod3 >= supplierPayment.PaymentPeriod4) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", "�P�\����4�͗P�\����3���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod5"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "�P�\����4" }));
                    }
                }
                if (supplierPayment.PaymentPeriod4 >= supplierPayment.PaymentPeriod5) {
                    if (ModelState["pay.PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod5", "�P�\����5�͗P�\����4���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }

            if (!string.IsNullOrEmpty(form["pay.PaymentPeriod6"])) {
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod1"])) {
                    if (ModelState["pay.PaymentPeriod1"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod1", MessageUtils.GetMessage("E0001", new string[] { "�P�\����1" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod2"])) {
                    if (ModelState["pay.PaymentPeriod2"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod2", MessageUtils.GetMessage("E0001", new string[] { "�P�\����2" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod3"])) {
                    if (ModelState["pay.PaymentPeriod3"].Errors.Count == 0) {
                        ModelState.AddModelError("PaymentPeriod3", MessageUtils.GetMessage("E0001", new string[] { "�P�\����3" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod4"])) {
                    if (ModelState["pay.PaymentPeriod4"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod4", MessageUtils.GetMessage("E0001", new string[] { "�P�\����4" }));
                    }
                }
                if (string.IsNullOrEmpty(form["pay.PaymentPeriod5"])) {
                    if (ModelState["pay.PaymentPeriod5"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod5", MessageUtils.GetMessage("E0001", new string[] { "�P�\����5" }));
                    }
                }
                if (supplierPayment.PaymentPeriod5 >= supplierPayment.PaymentPeriod6) {
                    if (ModelState["pay.PaymentPeriod6"].Errors.Count == 0) {
                        ModelState.AddModelError("pay.PaymentPeriod6", "�P�\����6�͗P�\����5���傫�Ȑ����ł���K�v������܂�");
                    }
                }
            }
        }
        private void ValidateCustomerDM(CustomerDM customerDM) {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(customerDM.FirstName)) {
                ModelState.AddModelError("dm.FirstName", MessageUtils.GetMessage("E0001", "�ڋq��1(��)"));
            }
            if (string.IsNullOrEmpty(customerDM.FirstNameKana)) {
                ModelState.AddModelError("dm.FirstNameKana", MessageUtils.GetMessage("E0001", "�ڋq��1(���J�i)"));
            }
            // �`���`�F�b�N
            // Add 2015/01/09 arc nakayama �ڋqDM�w�E�����C �o�^���̗X�֔ԍ��̓n�C�t���Ȃ��œo�^����
            if (!string.IsNullOrEmpty(customerDM.PostCode))
            {
                if ((!Regex.IsMatch(customerDM.PostCode.ToString(), @"\d{7}")) && (!Regex.IsMatch(customerDM.PostCode.ToString(), @"\d{3}-\d{4}")))
                {
                    ModelState.AddModelError("dm.PostCode", MessageUtils.GetMessage("E0026", "�X�֔ԍ�"));
                }
            }
        }
        #endregion


        /// <summary>
        /// �ڋq�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^(�o�^���e)</param>
        /// <returns>�ڋq�}�X�^���f���N���X</returns>
        private Customer EditCustomerForInsert(Customer customer) {

            customer.CustomerCode = new SerialNumberDao(db).GetNewCustomerCode(customer.DepartmentCode);
            customer.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.CreateDate = DateTime.Now;

            customer.DelFlag = "0";
            return EditCustomerForUpdate(customer);
        }

        /// <summary>
        /// �ڋq�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^(�o�^���e)</param>
        /// <returns>�ڋq�}�X�^���f���N���X</returns>
        private Customer EditCustomerForUpdate(Customer customer) {

            customer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customer.LastUpdateDate = DateTime.Now;

            //----20100513�ǉ�
            //Add 2017/02/08 arc nakayama #3627_�ڋq�}�X�^��������_���͕����̕������� ���͒l��ޔ�
            string PrevFirstName = customer.FirstName;
            string PrevLastName = customer.LastName;
 
            customer.FirstName = Strings.StrConv(customer.FirstName.Trim(),VbStrConv.Wide,0);
            customer.LastName = Strings.StrConv(customer.LastName.Trim(),VbStrConv.Wide,0);

            //Add 2017/02/08 arc nakayama #3627_�ڋq�}�X�^��������_���͕����̕������� �ϊ��Ɏ��s����Ɓu�H�v�ɂȂ邽�߁A�u�H�v���܂܂�Ă�������͒l��DB�ɓo�^����
            if (customer.FirstName.Contains("�H") || customer.FirstName.Contains("?"))
            {
                customer.FirstName = PrevFirstName;
            }
            if (customer.LastName.Contains("�H") || customer.LastName.Contains("�H"))
            {
                customer.LastName = PrevLastName;
            }

            customer.FirstNameKana = Strings.StrConv(customer.FirstNameKana.Trim(),VbStrConv.Wide,0);
            customer.LastNameKana = Strings.StrConv(customer.LastNameKana.Trim(),VbStrConv.Wide,0);
            customer.CustomerName = customer.FirstName + " " + customer.LastName;
            customer.CustomerNameKana = customer.FirstNameKana + " " + customer.LastNameKana;
            //----

            return customer;
        }

        /// <summary>
        /// �V�K�ǉ�����ڋq�Ɠ����������ǉ�����
        /// �i���ɑ��݂���R�[�h��������NULL��Ԃ��j
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <returns>������f�[�^</returns>
        private CustomerClaim CreateCustomerClaim(Customer customer) {

            CustomerClaimDao dao = new CustomerClaimDao(db);
            CustomerClaim customerClaim = dao.GetByKey(customer.CustomerCode);
            if (customerClaim == null) {
                customerClaim = new CustomerClaim();
                customerClaim.CustomerClaimCode = customer.CustomerCode;
                customerClaim.CustomerClaimName = customer.CustomerName;
                customerClaim.CustomerClaimType = !string.IsNullOrEmpty(customer.CustomerType) && (customer.CustomerType.Equals("001") || customer.CustomerType.Equals("002")) ? customer.CustomerType : "001";
                customerClaim.CreateDate = DateTime.Now;
                customerClaim.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                customerClaim.LastUpdateDate = DateTime.Now;
                customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                customerClaim.DelFlag = "0";
            }
            return customerClaim;

        }

        #region �������
        /// <summary>
        /// �ڋq�������͉�ʂ̕\��
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public ActionResult IntegrateEntry(string id) {

            Customer customer;
            if (!string.IsNullOrEmpty(id)) {
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                customer = new CustomerDao(db).GetByKey(id, true);
                ViewData["update"] = "1";
            } else {
                customer = new Customer();
                ViewData["update"] = "0";
            }

            if (customer.CustomerClaim == null) {
                customer.CustomerClaim = new CustomerClaim() { CustomerClaimCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["claimUpdate"] = "0";
            } else {
                ViewData["claimUpdate"] = "1";
            }
            if (customer.Supplier == null) {
                customer.Supplier = new Supplier() { SupplierCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["supplierUpdate"] = "0";
            } else {
                ViewData["supplierUpdate"] = "1";
            }
            if (customer.SupplierPayment == null) {
                customer.SupplierPayment = new SupplierPayment() { SupplierPaymentCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["paymentUpdate"] = "0";
            } else {
                ViewData["paymentUpdate"] = "1";
            }
            if (customer.CustomerDM == null) {
                customer.CustomerDM = new CustomerDM() { CustomerCode = customer.CustomerCode, DelFlag = "1" };
                ViewData["customerDMUpdate"] = "0";
            } else {
                ViewData["customerDMUpdate"] = "1";
            }
            if (customer.CustomerUpdateLog == null) {
                customer.CustomerUpdateLog = new EntitySet<CustomerUpdateLog>();
            }
            GetEntryViewData(customer);
            SetCustomerClaim(customer);
            ViewData["CustomerBasicDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// ������ʂł̓o�^����
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="claim">������f�[�^</param>
        /// <param name="claimable">���Ϗ���</param>
        /// <param name="sup">�d����f�[�^</param>
        /// <param name="pay">�x����f�[�^</param>
        /// <param name="updateLog">�S���Ґ��ڃf�[�^</param>
        /// <param name="form">�t�H�[�����͒l�f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/03 arc yano #3804 �ڋq�������́@�T�[�r�X�S�������ύX���Ă��A�S���Ґ��ڂɔ��f����Ȃ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult IntegrateEntry(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {

            //�ڋq�f�[�^��Validation�`�F�b�N
            ValidateCustomer(customer, form);

            if (form["CustomerClaimEnabled"].Contains("true")) {
                ValidateCustomerClaim(claim,claimable);
            }
            if (form["SupplierEnabled"].Contains("true")) {
                ValidateSupplier(sup);
            }
            if (form["SupplierPaymentEnabled"].Contains("true")) {
                ValidateSupplierPayment(pay, form);
            }
            if (form["CustomerDMEnabled"].Contains("true")) {
                ValidateCustomerDM(dm);
            }
            if (!ModelState.IsValid) {
                customer.CustomerClaim = claim;
                customer.CustomerClaim.CustomerClaimable = claimable;
                customer.Supplier = sup;
                customer.SupplierPayment = pay;
                customer.CustomerUpdateLog = updateLog;
                customer.CustomerDM = dm;

                GetEntryViewData(customer);
                SetCustomerClaim(customer, form);
                ViewData["CustomerBasicDisplay"] = true;
                return View("CustomerIntegrateEntry", customer);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            Employee employee = (Employee)Session["Employee"];
            using (TransactionScope ts = new TransactionScope()) {


                //�ڋq�̍X�V
                if (form["update"].Equals("1")) {

                    //������̍X�V
                    if (form["claimUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                        CustomerClaim targetClaim = new CustomerClaimDao(db).GetByKey(claim.CustomerClaimCode, true);
                        UpdateModel(targetClaim,"claim");
                        targetClaim.LastUpdateDate = DateTime.Now;
                        targetClaim.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/02/02 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        targetClaim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                        if (form["CustomerClaimEnabled"].Contains("true")) {
                            targetClaim.DelFlag = "0";
                        } else {
                            targetClaim.DelFlag = "1";
                        }
                    } else {
                        //�V�K�쐬
                        if (form["CustomerClaimEnabled"].Contains("true")) {
                            claim.CustomerClaimCode = customer.CustomerCode;
                            claim.CreateDate = DateTime.Now;
                            claim.CreateEmployeeCode = employee.EmployeeCode;
                            claim.LastUpdateDate = DateTime.Now;
                            claim.LastUpdateEmployeeCode = employee.EmployeeCode;
                            claim.DelFlag = "0";
                            claim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                            db.CustomerClaim.InsertOnSubmit(claim);
                        } else {
                            //�쐬���Ȃ�
                            customer.CustomerClaim = null;
                        }
                    }

                    //�d����̍X�V
                    if (form["supplierUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                        Supplier targetSupplier = new SupplierDao(db).GetByKey(sup.SupplierCode, true);
                        UpdateModel(targetSupplier,"sup");
                        targetSupplier.LastUpdateDate = DateTime.Now;
                        targetSupplier.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/02/02 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        targetSupplier.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                        if (form["SupplierEnabled"].Contains("true")) {
                            targetSupplier.DelFlag = "0";
                        } else {
                            targetSupplier.DelFlag = "1";
                        }

                    } else {
                        //�V�K�쐬
                        if (form["SupplierEnabled"].Contains("true")) {
                            sup.SupplierCode = customer.CustomerCode;
                            sup.CreateDate = DateTime.Now;
                            sup.CreateEmployeeCode = employee.EmployeeCode;
                            sup.LastUpdateDate = DateTime.Now;
                            sup.LastUpdateEmployeeCode = employee.EmployeeCode;
                            sup.DelFlag = "0";
                            //Add 2015/02/02 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                            sup.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                            db.Supplier.InsertOnSubmit(sup);
                        } else {
                            //�쐬���Ȃ�
                            customer.Supplier = null;
                        }
                    }

                    //�x����̍X�V
                    if (form["paymentUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                        SupplierPayment targetPayment = new SupplierPaymentDao(db).GetByKey(pay.SupplierPaymentCode, true);
                        UpdateModel(targetPayment,"pay");
                        targetPayment.LastUpdateDate = DateTime.Now;
                        targetPayment.LastUpdateEmployeeCode = employee.EmployeeCode;
                        if (form["SupplierPaymentEnabled"].Contains("true")) {
                            targetPayment.DelFlag = "0";
                        } else {
                            targetPayment.DelFlag = "1";
                        }
                    } else {
                        //�V�K�쐬
                        if (form["SupplierPaymentEnabled"].Contains("true")) {
                            pay.SupplierPaymentCode = customer.CustomerCode;
                            pay.CreateDate = DateTime.Now;
                            pay.CreateEmployeeCode = employee.EmployeeCode;
                            pay.LastUpdateDate = DateTime.Now;
                            pay.LastUpdateEmployeeCode = employee.EmployeeCode;
                            pay.DelFlag = "0";
                            db.SupplierPayment.InsertOnSubmit(pay);
                        } else {
                            //�쐬���Ȃ�
                            customer.SupplierPayment = null;
                        }
                    }

                    //DM������̍X�V
                    if (form["customerDMUpdate"].Equals("1")) {
                        //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                        CustomerDM targetDM = new CustomerDMDao(db).GetByKey(customer.CustomerCode, true);
                        UpdateModel(targetDM, "dm");
                        targetDM.LastUpdateDate = DateTime.Now;
                        targetDM.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/01/29 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        targetDM.PostCode = CommonUtils.InsertHyphenInPostCode(dm.PostCode);
                        if (form["CustomerDMEnabled"].Contains("true")) {
                            targetDM.DelFlag = "0";
                        } else {
                            targetDM.DelFlag = "1";
                        }
                    } else {
                        if (form["CustomerDMEnabled"].Contains("true")) {
                            dm.CustomerCode = customer.CustomerCode;
                            dm.CreateDate = DateTime.Now;
                            dm.CreateEmployeeCode = employee.EmployeeCode;
                            dm.LastUpdateDate = DateTime.Now;
                            dm.LastUpdateEmployeeCode = employee.EmployeeCode;
                            dm.DelFlag = "0";
                            db.CustomerDM.InsertOnSubmit(dm);
                        }else{
                            customer.CustomerDM = null;
                        }
                    }
                    //���Ϗ����̍X�V
                    //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                    CustomerClaim customerClaimableTarget = new CustomerClaimDao(db).GetByKey(claim.CustomerClaimCode, true);
                    //�Ȃ��Ȃ��Ă�����͍̂폜����
                    if (customerClaimableTarget != null) {
                        foreach (var original in customerClaimableTarget.CustomerClaimable) {
                            IEnumerable<CustomerClaimable> query = null;
                            if (claimable != null) {
                                query =
                                    from a in claimable
                                    where a.PaymentKindCode.Equals(original.PaymentKindCode)
                                    select a;
                            }
                            if (claimable == null || query == null || query.Count() == 0) {
                                db.CustomerClaimable.DeleteOnSubmit(original);
                            }
                        }
                    }
                    if (claimable != null) {
                        foreach (var item in claimable) {
                            if (!string.IsNullOrEmpty(item.PaymentKindCode)) {
                                //�Ȃ����̂͒ǉ�����
                                CustomerClaimable target = new CustomerClaimableDao(db).GetByKey(claim.CustomerClaimCode, item.PaymentKindCode);
                                if (target == null) {
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = employee.EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = employee.EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CustomerClaimable.InsertOnSubmit(item);
                                }
                            }
                        }
                    }

                    //�ڋq�f�[�^�̍X�V
                    //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                    Customer targetCustomer = new CustomerDao(db).GetByKey(customer.CustomerCode, true);
                    string departmentFrom = targetCustomer.DepartmentCode;
                    string serviceDepartmentFrom = targetCustomer.ServiceDepartmentCode;        //Add 2017/11/03 arc yano #3804

                    string carEmployeeFrom = targetCustomer.CarEmployeeCode;
                    string serviceEmployeeFrom = targetCustomer.ServiceEmployeeCode;
                    UpdateModel(targetCustomer);
                    EditCustomerForUpdate(targetCustomer);
                    //Add�@2015/01/29 arc nakayama �ڋqDM�w�E�����C���@�o�^�܂��͍X�V�O�ɗX�֔ԍ��Ƀn�C�t���������ĂȂ���΃n�C�t��������
                    targetCustomer.PostCode = CommonUtils.InsertHyphenInPostCode(customer.PostCode);

                    //Mod 2017/11/03 arc yano #3804
                    //�S���Ґ��ڂ̍쐬(�c�ƒS������)
                    if (!CommonUtils.DefaultString(departmentFrom).Equals(CommonUtils.DefaultString(customer.DepartmentCode))) {
                        Department fromDepartment = new DepartmentDao(db).GetByKey(departmentFrom);
                        Department toDepartment = new DepartmentDao(db).GetByKey(customer.DepartmentCode);
                        CreateUpdateLog(
                            customer,
                            fromDepartment!=null ? fromDepartment.DepartmentName : "",
                            toDepartment!=null ? toDepartment.DepartmentName : "",
                            "�c�ƒS������"        
                            );
                    }

                    //�S���Ґ��ڂ̍쐬(�T�[�r�X�S������)
                    if (!CommonUtils.DefaultString(serviceDepartmentFrom).Equals(CommonUtils.DefaultString(customer.ServiceDepartmentCode)))
                    {
                        Department fromDepartment = new DepartmentDao(db).GetByKey(serviceDepartmentFrom);
                        Department toDepartment = new DepartmentDao(db).GetByKey(customer.ServiceDepartmentCode);
                        CreateUpdateLog(
                            customer,
                            fromDepartment != null ? fromDepartment.DepartmentName : "",
                            toDepartment != null ? toDepartment.DepartmentName : "",
                            "�T�[�r�X�S������"
                            );
                    }

                    if (!CommonUtils.DefaultString(carEmployeeFrom).Equals(CommonUtils.DefaultString(customer.CarEmployeeCode))) {
                        Employee fromEmployee = new EmployeeDao(db).GetByKey(carEmployeeFrom);
                        Employee toEmployee = new EmployeeDao(db).GetByKey(customer.CarEmployeeCode);
                        CreateUpdateLog(
                            customer,
                            fromEmployee!=null ? fromEmployee.EmployeeName : "",
                            toEmployee!=null ? toEmployee.EmployeeName : "",
                            "�c�ƒS��"
                            );
                    }
                    if (!CommonUtils.DefaultString(serviceEmployeeFrom).Equals(CommonUtils.DefaultString(customer.ServiceEmployeeCode))) {
                        Employee fromEmployee = new EmployeeDao(db).GetByKey(serviceEmployeeFrom);
                        Employee toEmployee = new EmployeeDao(db).GetByKey(customer.ServiceEmployeeCode);
                        CreateUpdateLog(
                            customer, 
                            fromEmployee!=null ? fromEmployee.EmployeeName : "", 
                            toEmployee!=null ? toEmployee.EmployeeName : "",
                            "�T�[�r�X�S��"
                            );
                    }

                } else {
                    //�ڋq�R�[�h�̍̔�
                    EditCustomerForInsert(customer);

                    //�x����
                    if (form["SupplierPaymentEnabled"].Contains("true")) {
                        pay.SupplierPaymentCode = customer.CustomerCode;
                        pay.CreateDate = DateTime.Now;
                        pay.CreateEmployeeCode = employee.EmployeeCode;
                        pay.LastUpdateDate = DateTime.Now;
                        pay.LastUpdateEmployeeCode = employee.EmployeeCode;
                        pay.DelFlag = "0";
                        customer.SupplierPayment = pay;
                    } else {
                        customer.SupplierPayment = null;
                    }

                    //�d����
                    if (form["SupplierEnabled"].Contains("true")) {
                        sup.SupplierCode = customer.CustomerCode;
                        sup.CreateDate = DateTime.Now;
                        sup.CreateEmployeeCode = employee.EmployeeCode;
                        sup.LastUpdateDate = DateTime.Now;
                        sup.LastUpdateEmployeeCode = employee.EmployeeCode;
                        sup.DelFlag = "0";
                        //Add 2015/01/29 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        sup.PostCode = CommonUtils.InsertHyphenInPostCode(sup.PostCode);
                        customer.Supplier = sup;
                    } else {
                        customer.Supplier = null;
                    }

                    //������
                    if (form["CustomerClaimEnabled"].Contains("true")) {
                        claim.CustomerClaimCode = customer.CustomerCode;
                        claim.CreateDate = DateTime.Now;
                        claim.CreateEmployeeCode = employee.EmployeeCode;
                        claim.LastUpdateDate = DateTime.Now;
                        claim.LastUpdateEmployeeCode = employee.EmployeeCode;
                        claim.DelFlag = "0";
                        //Add 2015/01/29 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        claim.PostCode = CommonUtils.InsertHyphenInPostCode(claim.PostCode);
                        customer.CustomerClaim = claim;
                        if (claimable != null) {
                            foreach (var item in claimable) {
                                // ADD 2014/05/21 arc uchida vs2012�Ή�
                                if (item.PaymentKindCode != null)
                                {
                                    item.CustomerClaimCode = customer.CustomerCode;
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = employee.EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = employee.EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CustomerClaimable.InsertOnSubmit(item);
                                }
                            }
                        }
                    } else {
                        customer.CustomerClaim = null;
                    }

                    //DM������̍X�V
                    if (form["CustomerDMEnabled"].Contains("true")) {
                        dm.CustomerCode = customer.CustomerCode;
                        dm.CreateDate = DateTime.Now;
                        dm.CreateEmployeeCode = employee.EmployeeCode;
                        dm.LastUpdateDate = DateTime.Now;
                        dm.LastUpdateEmployeeCode = employee.EmployeeCode;
                        //Add 2015/01/29 arc nakayama �ڋqDM�Ή��@�n�C�t�����Ȃ���Γ����
                        dm.PostCode = CommonUtils.InsertHyphenInPostCode(dm.PostCode);
                        dm.DelFlag = "0";
                        db.CustomerDM.InsertOnSubmit(dm);
                    } else {
                        customer.CustomerDM = null;
                    }

                    //Add�@2015/01/29 arc nakayama �ڋqDM�w�E�����C���@�o�^�܂��͍X�V�O�ɗX�֔ԍ��Ƀn�C�t���������ĂȂ���΃n�C�t��������
                    customer.PostCode = CommonUtils.InsertHyphenInPostCode(customer.PostCode);
                    db.Customer.InsertOnSubmit(customer);
                }

                // Mod 2014/08/04 arc amii �G���[���O�Ή� catch���ǉ��B�wthrow e�x�����O�t�@�C���ɃG���[���o�͂��鏈���ɕύX
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException ce)
                    {
                        // �X�V���A�N���C�A���g�̓ǂݎ��ȍ~��DB�l���X�V���ꂽ���A���[�J���̒l��DB�l�ŏ㏑������
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        // ���g���C�񐔂𒴂����ꍇ�A�G���[�Ƃ���
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("CustomerCode", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            customer.CustomerClaim = claim;
                            customer.Supplier = sup;
                            customer.SupplierPayment = pay;
                            GetEntryViewData(customer);
                            SetCustomerClaim(customer, form);
                            ViewData["CustomerBasicDisplay"] = true;
                            return View("CustomerIntegrateEntry", customer);
                        }
                        else
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["close"] = "1";
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// �S���Ґ��ڂ��쐬����
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="fromValue">�ύX�O</param>
        /// <param name="toValue">�ύX��</param>
        /// <param name="updateColumn">�X�V�Ώۍ��ږ�</param>
        private void CreateUpdateLog(Customer customer,string fromValue,string toValue,string updateColumn) {
            Employee employee = (Employee)Session["Employee"];

            CustomerUpdateLog log = new CustomerUpdateLog();
            log.CustomerUpdateLogId = Guid.NewGuid();
            log.UpdateDate = DateTime.Now;
            log.CustomerCode = customer.CustomerCode;
            log.UpdateColumn = updateColumn;
            log.UpdateEmployeeCode = employee.EmployeeCode;
            log.UpdateValueFrom = string.IsNullOrEmpty(fromValue) ? "" : fromValue;
            log.UpdateValueTo = string.IsNullOrEmpty(toValue) ? "" : toValue;
            db.CustomerUpdateLog.InsertOnSubmit(log);
        }

        #region ������ʃR���|�[�l���g
        /// <summary>
        /// ������ʃR���|�[�l���g�̃Z�b�g(update�t���O�t��)
        /// </summary>
        /// <param name="customer"></param>
        private void SetCustomerClaim(Customer customer,FormCollection form) {

            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];



            SetCustomerClaim(customer);
        }

        /// <summary>
        /// ������ʃR���|�[�l���g�̃Z�b�g
        /// </summary>
        /// <param name="customer"></param>
        /// <history>
        /// 2019/01/18 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���)
        /// </history>
        private void SetCustomerClaim(Customer customer){


            if (customer.CustomerClaim != null && customer.CustomerClaim.CustomerClaimable != null && customer.CustomerClaim.CustomerClaimable != null) {
                foreach (var claimable in customer.CustomerClaim.CustomerClaimable) {
                    claimable.PaymentKind = new PaymentKindDao(db).GetByKey(claimable.PaymentKindCode);
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customer.CustomerClaim.CustomerClaimType, true);
            ViewData["PaymentKindTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), customer.CustomerClaim.PaymentKindType, true);
            ViewData["RoundTypeList"] = CodeUtils.GetSelectListByModel(dao.GetRoundTypeAll(false), customer.CustomerClaim.RoundType, true);

            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, customer.Supplier.OutsourceFlag, true);
            ViewData["SupplierPaymentTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSupplierPaymentTypeAll(false), customer.SupplierPayment.SupplierPaymentType, true);
            ViewData["PaymentType2List"] = CodeUtils.GetSelectListByModel(dao.GetPaymentType2All(false), customer.SupplierPayment.PaymentType, true);

            if (customer.SupplierPayment != null) {
                try { ViewData["BankName"] = new BankDao(db).GetByKey(customer.SupplierPayment.BankCode).BankName; } catch { }
                try { ViewData["BranchName"] = new BranchDao(db).GetByKey(customer.SupplierPayment.BranchCode, customer.SupplierPayment.BankCode).BranchName; } catch { }
                try { ViewData["DepositKindList"] = CodeUtils.GetSelectListByModel(dao.GetDepositKindAll(false), customer.SupplierPayment.DepositKind, true); } catch { }

            }
            ViewData["CustomerBasicDisplay"] = false;
            ViewData["CustomerSalesDisplay"] = false;
            ViewData["CustomerClaimDisplay"] = false;
            ViewData["SupplierDisplay"] = false;
            ViewData["SupplierPaymentDisplay"] = false;
            ViewData["CustomerDMDisplay"] = false;

            //Add 20115/03/06 arc iijima ��{����DM������̓o�^�L���̒ǉ�

            //Mod 2019/01/18 yano #3965
            if (customer.CustomerDM != null && (string.IsNullOrWhiteSpace(customer.CustomerDM.DelFlag) || customer.CustomerDM.DelFlag.Equals("0")))
            {
                ViewData["DMEnabledmessage"] = "DM������ʓr�o�^����";
            }
            else{
                ViewData["DMEnabledmessage"] = "  ";
            }


            customer.BasicHasErrors = BasicHasErrors();
            customer.CustomerHasErrors = CustomerHasErrors();
            customer.CustomerClaimHasErrors = CustomerClaimHasErrors();
            customer.SupplierHasErrors = SupplierHasErrors();
            customer.SupplierPaymentHasErrors = SupplierPaymentHasErrors();
            customer.CustomerDMHasErrors = CustomerDMHasErrors();

        }
        #endregion

        #region ������E�d����E�x����̗L���^��������
        /// <summary>
        /// �������L���ɂ���
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="claim">������f�[�^</param>
        /// <param name="claimable">���Ϗ����f�[�^</param>
        /// <param name="sup">�d����f�[�^</param>
        /// <param name="pay">�x����f�[�^</param>
        /// <param name="updateLog">�S���Ґ��ڃf�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        public ActionResult ClaimEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.CustomerDM = dm; 

            if (form["CustomerClaimEnabled"].Contains("true")) {
                customer.CustomerClaim.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.CustomerClaim.CustomerClaimName)) {
                    customer.CustomerClaim.CustomerClaimName = customer.FirstName + customer.LastName;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.PostCode)) {
                    customer.CustomerClaim.PostCode = customer.PostCode;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Prefecture)) {
                    customer.CustomerClaim.Prefecture = customer.Prefecture;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.City)) {
                    customer.CustomerClaim.City = customer.City;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Address1)) {
                    customer.CustomerClaim.Address1 = customer.Address1;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.Address2)) {
                    customer.CustomerClaim.Address2 = customer.Address2;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.TelNumber1)) {
                    customer.CustomerClaim.TelNumber1 = customer.TelNumber;
                //}
                //if (string.IsNullOrEmpty(customer.CustomerClaim.FaxNumber)) {
                    customer.CustomerClaim.FaxNumber = customer.FaxNumber;
                //}
            } else {
                customer.CustomerClaim.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["CustomerClaimDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// �d�����L���ɂ���
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="claim"></param>
        /// <param name="claimable"></param>
        /// <param name="sup"></param>
        /// <param name="pay"></param>
        /// <param name="updateLog"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult SupplierEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["SupplierEnabled"].Contains("true")) {
                customer.Supplier.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.Supplier.SupplierName)) {
                    customer.Supplier.SupplierName = customer.FirstName + customer.LastName;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.PostCode)) {
                    customer.Supplier.PostCode = customer.PostCode;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Prefecture)) {
                    customer.Supplier.Prefecture = customer.Prefecture;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.City)) {
                    customer.Supplier.City = customer.City;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Address1)) {
                    customer.Supplier.Address1 = customer.Address1;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.Address2)) {
                    customer.Supplier.Address2 = customer.Address2;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.TelNumber1)) {
                    customer.Supplier.TelNumber1 = customer.TelNumber;
                //}
                //if (string.IsNullOrEmpty(customer.Supplier.FaxNumber)) {
                    customer.Supplier.FaxNumber = customer.FaxNumber;
                //}
            } else {
                customer.Supplier.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["SupplierDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        /// <summary>
        /// �x�����L���ɂ���
        /// </summary>
        /// <param name="customer">�ڋq�f�[�^</param>
        /// <param name="claim">������f�[�^</param>
        /// <param name="claimable">���Ϗ����f�[�^</param>
        /// <param name="sup">�d����f�[�^</param>
        /// <param name="pay">�x����f�[�^</param>
        /// <param name="updateLog">�S���Ґ��ڃf�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        public ActionResult SupplierPaymentEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["SupplierPaymentEnabled"].Contains("true")) {
                customer.SupplierPayment.DelFlag = "0";
                //if (string.IsNullOrEmpty(customer.SupplierPayment.SupplierPaymentName)) {
                    customer.SupplierPayment.SupplierPaymentName = customer.FirstName + customer.LastName;
                //}
            } else {
                customer.SupplierPayment.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["SupplierPaymentDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }

        public ActionResult DMEnabled(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {
            ViewData["update"] = form["update"];
            ViewData["claimUpdate"] = form["claimUpdate"];
            ViewData["supplierUpdate"] = form["supplierUpdate"];
            ViewData["paymentUpdate"] = form["paymentUpdate"];
            ViewData["customerDMUpdate"] = form["customerDMUpdate"];

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerUpdateLog = updateLog;
            customer.CustomerDM = dm;
            if (form["CustomerDMEnabled"].Contains("true")) {
                customer.CustomerDM.DelFlag = "0";
                if (string.IsNullOrEmpty(customer.CustomerDM.CorporationType)) {
                    customer.CustomerDM.CorporationType = customer.CorporationType;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FirstName)) {
                    customer.CustomerDM.FirstName = customer.FirstName;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FirstNameKana)) {
                    customer.CustomerDM.FirstNameKana = customer.FirstNameKana;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.LastName)) {
                    customer.CustomerDM.LastName = customer.LastName;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.LastNameKana)) {
                    customer.CustomerDM.LastNameKana = customer.LastNameKana;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.PostCode)) {
                    customer.CustomerDM.PostCode = customer.PostCode;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Prefecture)) {
                    customer.CustomerDM.Prefecture = customer.Prefecture;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.City)) {
                    customer.CustomerDM.City = customer.City;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Address1)) {
                    customer.CustomerDM.Address1 = customer.Address1;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.Address2)) {
                    customer.CustomerDM.Address2 = customer.Address2;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.TelNumber)) {
                    customer.CustomerDM.TelNumber = customer.TelNumber;
                }
                if (string.IsNullOrEmpty(customer.CustomerDM.FaxNumber)) {
                    customer.CustomerDM.FaxNumber = customer.FaxNumber;
                }
            } else {
                customer.CustomerDM.DelFlag = "1";
            }

            GetEntryViewData(customer);
            SetCustomerClaim(customer, form);
            ViewData["CustomerDMDisplay"] = true;
            ModelState.Clear();
            return View("CustomerIntegrateEntry", customer);
        }
        #endregion

        #region ���Ϗ����̍s�ǉ��E�s�폜
        public ActionResult AddClaimable(Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog,FormCollection form) {
             
            if (claimable == null) {
                claimable = new EntitySet<CustomerClaimable>();
            }
            CustomerClaimable newClaimable = new CustomerClaimable() { CustomerClaimCode = claim.CustomerClaimCode, DelFlag = "0" };
            claimable.Add(newClaimable);

            ModelState.Clear();

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;
            GetEntryViewData(customer);
            SetCustomerClaim(customer,form);
            ViewData["CustomerClaimDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }

        public ActionResult DelClaimable(int id, Customer customer, CustomerClaim claim, EntitySet<CustomerClaimable> claimable, Supplier sup, SupplierPayment pay, CustomerDM dm, EntitySet<CustomerUpdateLog> updateLog, FormCollection form) {

            claimable.RemoveAt(id);

            ModelState.Clear();

            customer.CustomerClaim = claim;
            customer.CustomerClaim.CustomerClaimable = claimable;
            customer.Supplier = sup;
            customer.SupplierPayment = pay;
            customer.CustomerDM = dm;
            customer.CustomerUpdateLog = updateLog;
            GetEntryViewData(customer);
            SetCustomerClaim(customer, form);
            ViewData["CustomerClaimDisplay"] = true;
            return View("CustomerIntegrateEntry", customer);
        }
        #endregion

        private bool BasicHasErrors() {
            if (
                ModelStateIsInvalid("CustomerCode") ||
                ModelStateIsInvalid("CustomerType") ||
                ModelStateIsInvalid("CorporationType") ||
                ModelStateIsInvalid("FirstName") ||
                ModelStateIsInvalid("LastName") ||
                ModelStateIsInvalid("FirstNameKana") ||
                ModelStateIsInvalid("LastNameKana") ||
                ModelStateIsInvalid("PostCode") ||
                ModelStateIsInvalid("Prefecture") ||
                ModelStateIsInvalid("City") ||
                ModelStateIsInvalid("Address1") ||
                ModelStateIsInvalid("Address2") ||
                ModelStateIsInvalid("TelNumber") ||
                ModelStateIsInvalid("FaxNumber") ||
                ModelStateIsInvalid("FirstReceiptionDate") ||
                ModelStateIsInvalid("LastReceiptionDate") ||
                ModelStateIsInvalid("Memo") ||
                ModelStateIsInvalid("DelFlag")) {
                return true;
            }
            return false;

        }
        private bool CustomerHasErrors() {
            if(
                ModelStateIsInvalid("CustomerRank") ||
                ModelStateIsInvalid("CustomerKind") ||
                ModelStateIsInvalid("PaymentKind") ||
                ModelStateIsInvalid("Sex") ||
                ModelStateIsInvalid("Birthday") ||
                ModelStateIsInvalid("Occupation") ||
                ModelStateIsInvalid("CarOwner") ||
                ModelStateIsInvalid("MailAddress") ||
                ModelStateIsInvalid("MobileNumber") ||
                ModelStateIsInvalid("MobileAddress") ||
                ModelStateIsInvalid("DmFlag") ||
                ModelStateIsInvalid("DmMemo") ||
                ModelStateIsInvalid("DepartmentCode") ||
                ModelStateIsInvalid("CarEmployeeCode") ||
                ModelStateIsInvalid("ServiceDepartmentCode") ||
                ModelStateIsInvalid("ServiceEmployeeCode") ||
                ModelStateIsInvalid("WorkingCompanyName") ||
                ModelStateIsInvalid("WorkingCompanyAddress") || 
                ModelStateIsInvalid("WorkingCompanyTelNumber") ||
                ModelStateIsInvalid("PositionName") ||
                ModelStateIsInvalid("CustomerEmployeeName") ||
                ModelStateIsInvalid("AccountEmployeeName"))
            {
                return true;
            }
            return false;
        }

        private bool CustomerClaimHasErrors() {
            var query = from a in ModelState
                        where (a.Key.StartsWith("claim.") && a.Value.Errors.Count() > 0)
                        || (a.Key.StartsWith("claimable[") && a.Value.Errors.Count() > 0)
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool SupplierHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("sup.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool SupplierPaymentHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("pay.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool CustomerDMHasErrors(){
            var query = from a in ModelState
                        where a.Key.StartsWith("dm.") && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool ModelStateIsInvalid(string keyName){
            return ModelState[keyName]!=null && ModelState[keyName].Errors.Count()>0;
        }
        #endregion
    }

}
