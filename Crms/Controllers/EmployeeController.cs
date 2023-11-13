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
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �Ј��}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class EmployeeController : Controller {

        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�Ј��}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�Ј��}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public EmployeeController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// �Ј�������ʕ\��
        /// </summary>
        /// <returns>�Ј��������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �Ј�������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ј��������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Employee> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["EmployeeNumber"] = form["EmployeeNumber"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
            ViewData["SecurityRoleName"] = form["SecurityRoleName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �Ј�������ʂ̕\��
            return View("EmployeeCriteria", list);
        }

        /// <summary>
        /// �Ј������_�C�A���O�\��
        /// </summary>
        /// <returns>�Ј������_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �Ј������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ј�������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["EmployeeCode"] = Request["EmployeeCode"];
            form["EmployeeNumber"] = Request["EmployeeNumber"];
            form["EmployeeName"] = Request["EmployeeName"];
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["SecurityRoleCode"] = Request["SecurityRoleCode"];
            form["SecurityRoleName"] = Request["SecurityRoleName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Employee> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["EmployeeNumber"] = form["EmployeeNumber"];
            ViewData["EmployeeName"] = form["EmployeeName"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
            ViewData["SecurityRoleName"] = form["SecurityRoleName"];

            // �Ј�������ʂ̕\��
            return View("EmployeeCriteriaDialog", list);
        }

        /// <summary>
        /// �Ј������_�C�A���O�\��
        /// </summary>
        /// <returns>�Ј������_�C�A���O</returns>
        public ActionResult CriteriaDialog2()
        {
            criteriaInit = true;
            return CriteriaDialog2(new FormCollection());
        }

        /// <summary>
        /// �Ј������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ј�������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            if (criteriaInit)
            {
                form["DelFlag"] = "0";
                ViewData["DelFlag"] = form["DelFlag"];
                // �������ʃ��X�g�̎擾
                PaginatedList<Employee> list = GetSearchResultList(form);
                // ���匟����ʂ̕\��
                return View("EmployeeCriteriaDialog2", list);
            }
            else
            {

                // ���������̐ݒ�
                // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
                //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
                form["EmployeeCode"] = Request["EmployeeCode"];
                form["EmployeeNumber"] = Request["EmployeeNumber"];
                form["EmployeeName"] = Request["EmployeeName"];
                form["DepartmentCode"] = Request["DepartmentCode"];
                form["DepartmentName"] = Request["DepartmentName"];
                form["SecurityRoleCode"] = Request["SecurityRoleCode"];
                form["SecurityRoleName"] = Request["SecurityRoleName"];
                form["DelFlag"] = Request["DelFlag"];

                // �������ʃ��X�g�̎擾
                PaginatedList<Employee> list = GetSearchResultList(form);

                // ���̑��o�͍��ڂ̐ݒ�
                ViewData["EmployeeCode"] = form["EmployeeCode"];
                ViewData["EmployeeNumber"] = form["EmployeeNumber"];
                ViewData["EmployeeName"] = form["EmployeeName"];
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                ViewData["DepartmentName"] = form["DepartmentName"];
                ViewData["SecurityRoleCode"] = form["SecurityRoleCode"];
                ViewData["SecurityRoleName"] = form["SecurityRoleName"];
                ViewData["DelFlag"] = form["DelFlag"];

                // �Ј�������ʂ̕\��
                return View("EmployeeCriteriaDialog2", list);
            }
        }

        /// <summary>
        /// �Ј��}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�Ј��R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�Ј��}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            Employee employee;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                employee = new Employee();
            }
                // �X�V�̏ꍇ
            else {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                employee = new EmployeeDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(employee);

            // �o��
            return View("EmployeeEntry", employee);
        }

        /// <summary>
        /// �Ј��}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="area">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Ј��}�X�^���͉��</returns>
        /// <history>
        /// 2019/05/24 yano #3994 �y�Ј��}�X�^�z�Ј��}�X�^�o�^�E�X�V���̗����@�\�̒ǉ�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Employee employee, FormCollection form) {

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateEmployee(employee);
            if (!ModelState.IsValid) {
                GetEntryViewData(employee);
                return View("EmployeeEntry", employee);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1")) {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ�����)
                Employee targetEmployee = new EmployeeDao(db).GetByKey(employee.EmployeeCode, true);

                InsertEmployeeHisttory(targetEmployee);     //Mod 2019/05/24 yano #3994

                UpdateModel(targetEmployee);
                EditEmployeeForUpdate(targetEmployee);
                
            }
                // �f�[�^�ǉ�����
            else {
                // �f�[�^�ҏW
                employee = EditEmployeeForInsert(employee);

                // �f�[�^�ǉ�
                db.Employee.InsertOnSubmit(employee);                
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��(�X�V�ƒǉ��œ����Ă���SubmitChanges�𓝍�)
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
                } catch (SqlException se) {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0010", new string[] { "�Ј��R�[�h", "�ۑ�" }));
                        GetEntryViewData(employee);
                        return View("EmployeeEntry", employee);
                    } else {
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
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(employee.EmployeeCode);
        }

        /// <summary>
        /// �Ј��R�[�h����Ј������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ј��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code, bool includeDeleted = false)
        {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Employee employee = new EmployeeDao(db).GetByKey(code, includeDeleted);
                if (employee != null) {
                    codeData.Code = employee.EmployeeCode;
                    codeData.Name = employee.EmployeeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        public ActionResult GetMasterDetail(string code) {
            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> data = new Dictionary<string, string>();
                Employee employee;
                employee = new EmployeeDao(db).GetByEmployeeNumber(code);
                //�Ј��ԍ��Ńq�b�g���Ȃ�������Ј��R�[�h�Ō���������
                if (employee == null) {
                    employee = new EmployeeDao(db).GetByKey(code);
                }

                if (employee != null) {
                    data.Add("EmployeeCode", employee.EmployeeCode);
                    data.Add("EmployeeNumber", employee.EmployeeNumber);
                    data.Add("EmployeeName", employee.EmployeeName);
                    data.Add("ReceiptionEmployeeCode", employee.EmployeeCode);
                    data.Add("ReceiptionEmployeeNumber", employee.EmployeeNumber);
                    data.Add("ReceiptionEmployeeName", employee.EmployeeName);
                    data.Add("FrontEmployeeCode", employee.EmployeeCode);
                    data.Add("FrontEmployeeNumber", employee.EmployeeNumber);
                    data.Add("FrontEmployeeName", employee.EmployeeName);
                    data.Add("DepartureEmployeeCode", employee.EmployeeCode);
                    data.Add("DepartureEmployeeNumber", employee.EmployeeNumber);
                    data.Add("DepartureEmployeeName", employee.EmployeeName);
                    data.Add("ArrivalEmployeeCode", employee.EmployeeCode);
                    data.Add("ArrivalEmployeeNumber", employee.EmployeeNumber);
                    data.Add("ArrivalEmployeeName", employee.EmployeeName);
                }
                return Json(data);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="employee">���f���f�[�^</param>
        private void GetEntryViewData(Employee employee) {

            // ���喼�̎擾
            if (!string.IsNullOrEmpty(employee.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode1)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode1);
                if (department != null) {
                    ViewData["DepartmentName1"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode2)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode2);
                if (department != null) {
                    ViewData["DepartmentName2"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(employee.DepartmentCode3)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(employee.DepartmentCode3);
                if (department != null) {
                    ViewData["DepartmentName3"] = department.DepartmentName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["SecurityRoleList"] = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetListAll(), "SecurityRoleCode", "SecurityRoleName", employee.SecurityRoleCode, true);
            ViewData["EmployeeTypeList"] = CodeUtils.GetSelectListByModel(dao.GetEmployeeTypeAll(false), employee.EmployeeType, true);
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="employee">���f���f�[�^</param>
        private void SetDataComponent(Employee employee) {

            CodeDao dao = new CodeDao(db);

            // �Z�L�����e�B���[���Z���N�g���X�g
            ViewData["SecurityRoleList"] = CodeUtils.GetSelectListByModel(new SecurityRoleDao(db).GetListAll(), "SecurityRoleCode", "SecurityRoleName", employee.SecurityRoleCode, true);

            // �ٗp��ʃZ���N�g���X�g
            ViewData["EmployeeTypeList"] = CodeUtils.GetSelectListByModel(dao.GetEmployeeTypeAll(false), employee.EmployeeType, true);
        }

        /// <summary>
        /// �Ј��}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ј��}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Employee> GetSearchResultList(FormCollection form) {

            EmployeeDao employeeDao = new EmployeeDao(db);
            Employee employeeCondition = new Employee();
            employeeCondition.EmployeeCode = form["EmployeeCode"];
            employeeCondition.EmployeeNumber = form["EmployeeNumber"];
            employeeCondition.EmployeeName = form["EmployeeName"];
            employeeCondition.Department1 = new Department();
            employeeCondition.Department1.DepartmentCode = form["DepartmentCode"];
            employeeCondition.Department1.DepartmentName = form["DepartmentName"];
            employeeCondition.SecurityRole = new SecurityRole();
            employeeCondition.SecurityRole.SecurityRoleCode = form["SecurityRoleCode"];
            employeeCondition.SecurityRole.SecurityRoleName = form["SecurityRoleName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                employeeCondition.DelFlag = form["DelFlag"];
            }
            return employeeDao.GetListByCondition(employeeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="employee">�Ј��f�[�^</param>
        /// <returns>�Ј��f�[�^</returns>
        private Employee ValidateEmployee(Employee employee) {

            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(employee.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�Ј��R�[�h"));
            }
            if (string.IsNullOrEmpty(employee.EmployeeName)) {
                ModelState.AddModelError("EmployeeName", MessageUtils.GetMessage("E0001", "����"));
            }
            if (string.IsNullOrEmpty(employee.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            if (string.IsNullOrEmpty(employee.SecurityRoleCode)) {
                ModelState.AddModelError("SecurityRoleCode", MessageUtils.GetMessage("E0001", "�Z�L�����e�B���[��"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Birthday")) {
                ModelState.AddModelError("Birthday", MessageUtils.GetMessage("E0005", "���N����"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("EmployeeCode")) {
                if (!Regex.IsMatch(CommonUtils.DefaultString(employee.EmployeeCode), @"^[0-9A-Za-z\.]*$")) {
                    ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0015", "�Ј��R�[�h"));
                }
            }

            return employee;
        }

        /// <summary>
        /// �Ј��}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="employee">�Ј��f�[�^(�o�^���e)</param>
        /// <returns>�Ј��}�X�^���f���N���X</returns>
        private Employee EditEmployeeForInsert(Employee employee) {

            employee.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.CreateDate = DateTime.Now;
            employee.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.LastUpdateDate = DateTime.Now;
            employee.DelFlag = "0";
            return employee;
        }

        /// <summary>
        /// �Ј��}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="employee">�Ј��f�[�^(�o�^���e)</param>
        /// <returns>�Ј��}�X�^���f���N���X</returns>
        private Employee EditEmployeeForUpdate(Employee employee) {

            employee.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            employee.LastUpdateDate = DateTime.Now;
            return employee;
        }

        /// <summary>
        /// �Ј��}�X�^�X�V����o�^����
        /// </summary>
        /// <param name="employee">�X�V�O�Ј��f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2019/05/24 yano #3994 �y�Ј��}�X�^�z�Ј��}�X�^�o�^�E�X�V���̗����@�\�̒ǉ�
        /// </history>
        private void InsertEmployeeHisttory(Employee employee)
        {
            EmployeeHistory history = new EmployeeHistory();

            //�����e�[�u���̍ŐV�ԍ����擾
            int revNumber = (new EmployeeHistoryDao(db).GetLatestHistory(employee.EmployeeCode) != null ? (new EmployeeHistoryDao(db).GetLatestHistory(employee.EmployeeCode).RevisionNumber + 1) : 1);

            //�����e�[�u���Ńf�[�^�쐬
            history.EmployeeCode = employee.EmployeeCode;
            history.RevisionNumber = revNumber;
            history.EmployeeNumber = employee.EmployeeNumber;
            history.EmployeeName = employee.EmployeeName;
            history.EmployeeNameKana = employee.EmployeeNameKana;
            history.DepartmentCode = employee.DepartmentCode;
            history.SecurityRoleCode = employee.SecurityRoleCode;
            history.MobileNumber = employee.MobileNumber;
            history.MobileMailAddress = employee.MobileMailAddress;
            history.MailAddress = employee.MailAddress;
            history.EmployeeType = employee.EmployeeType;
            history.Birthday = employee.Birthday;
            history.LastLoginDateTime = employee.LastLoginDateTime;
            history.CreateEmployeeCode = employee.CreateEmployeeCode;
            history.CreateDate = employee.CreateDate;
            history.LastUpdateEmployeeCode = employee.LastUpdateEmployeeCode;
            history.LastUpdateDate = employee.LastUpdateDate;
            history.DelFlag = employee.DelFlag;
            history.DepartmentCode1 = employee.DepartmentCode1;
            history.DepartmentCode2 = employee.DepartmentCode2;
            history.DepartmentCode3 = employee.DepartmentCode3;

            db.EmployeeHistory.InsertOnSubmit(history);
        }

    }
}
