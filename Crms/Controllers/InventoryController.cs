using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �ԗ��I���@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class InventoryController : Controller {

        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��I��";               // ��ʖ�
        private static readonly string PROC_NAME_FIX = "����";               // ������
        private static readonly string PROC_NAME_REC = "�ԗ��I�����n���o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public InventoryController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �ԗ��I��������ʕ\��
        /// </summary>
        /// <returns>�ԗ��I���������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ��I��������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(����������)</param>
        /// <returns>�ԗ��I���������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            try { form["CompanyCode"] = (form["CompanyCode"] == null ? ((Employee)Session["Employee"]).Department1.Office.CompanyCode : form["CompanyCode"]); } catch (NullReferenceException) { }
            try { form["OfficeCode"] = (form["OfficeCode"] == null ? ((Employee)Session["Employee"]).Department1.OfficeCode : form["OfficeCode"]); } catch (NullReferenceException) { }
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            form["InventoryMonth"] = (form["InventoryMonth"] == null ? string.Format("{0:yyyy/MM}", DateTime.Now.AddMonths(-1)) : form["InventoryMonth"]);
            form["actionType"] = (form["actionType"] == null ? "" : form["actionType"]);

            // ���ߏ���
            if (form["actionType"].Equals("fixInventory")) {

                // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                using (TransactionScope ts = new TransactionScope()) {

                    string departmentCode = CommonUtils.DefaultString(form["fixDepartment"]);
                    DateTime inventoryMonth = DateTime.Parse(form["fixMonth"] + "/01 00:00:00.000");

                    // �I���X�P�W���[���e�[�u���̎擾
                    InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);

                    //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                    try
                    {
                        // �m�肳��Ă��Ȃ��ꍇ
                        if (inventorySchedule != null && !inventorySchedule.InventoryStatus.Equals("003"))
                        {
                            // �����`�F�b�N
                            V_CarInventorySummary v_CarInventorySummary = GetCarInventorySummaryForFix(departmentCode, inventoryMonth);
                            if (v_CarInventorySummary.DifferentialQuantity == 0)
                            {
                                // �I�����уe�[�u���ւ̒ǉ��ƍ폜�E�I���X�P�W���[���e�[�u���̃X�e�[�^�X�X�V�̃X�g�A�h�v���V�[�W�����s
                                db.FixInventory(departmentCode, inventoryMonth, "001", ((Employee)Session["Employee"]).EmployeeCode);
                            }
                        }
                        // �g�����U�N�V�����̃R�~�b�g
                        ts.Complete();
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_FIX, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }

            // �������ʃ��X�g�̎擾
            PaginatedList<V_CarInventorySummary> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            GetCriteriaViewData(form);

            // �ԗ��I��������ʂ̕\��
            return View("InventoryCriteria", list);
        }

        /// <summary>
        /// �ԗ��I�����͉�ʕ\��
        /// </summary>
        /// <param name="id">����R�[�h,�I����</param>
        /// <returns>�ԗ��I�����͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            string[] idArr = CommonUtils.DefaultString(id).Split(new string[] { "," }, StringSplitOptions.None);
            string departmentCode = idArr[0];
            DateTime inventoryMonth = DateTime.Parse(idArr[1].Substring(0, 4) + "/" + idArr[1].Substring(4, 2) + "/01 00:00:00.000");

            // ���͑Ώۃ��X�g�̎擾
            V_CarInventoryInProcess condition = new V_CarInventoryInProcess();
            condition.DepartmentCode = departmentCode;
            condition.InventoryMonth = inventoryMonth;
            condition.DefferenceSelect = true;
            List<V_CarInventoryInProcess> list = new V_CarInventoryInProcessDao(db).GetListByCondition(condition);

            // ���̑��o�͍��ڂ̐ݒ�
            GetEntryViewData(departmentCode, inventoryMonth);

            // �o��
            return View("InventoryEntry", list);
        }

        /// <summary>
        /// �ԗ��I�����n���o�^
        /// </summary>
        /// <param name="line">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��I�����͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(List<V_CarInventoryInProcess> line, FormCollection form) {
           
            form["action"] = (form["action"] == null ? "" : form["action"]);
            string departmentCode = form["DepartmentCode"];
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");

            // ��ʑ���ɂ�鏈������
            switch (form["action"]) {

                // ���׍s�\�[�g
                case "sort":

                    // ���׍s�\�[�g
                    line = SortLine(line, form["sortKey"]);

                    // ���̑��o�͍��ڂ̐ݒ�
                    GetEntryViewData(departmentCode, inventoryMonth);

                    // �o��
                    ModelState.Clear();
                    return View("InventoryEntry", line);

                // ���n���o�^
                default:

                    // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                    db = new CrmsLinqDataContext();
                    db.Log = new OutputWriter();

                    using (TransactionScope ts = new TransactionScope()) {
                        // �I���X�P�W���[���e�[�u���̎擾
                        InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);

                        // �m�肳��Ă��Ȃ��ꍇ
                        if (inventorySchedule != null && !inventorySchedule.InventoryStatus.Equals("003"))
                        {
                            // �f�[�^�`�F�b�N
                            ValidateInventory(line, form);
                            if (!ModelState.IsValid)
                            {
                                GetEntryViewData(departmentCode, inventoryMonth);
                                return View("InventoryEntry", line);
                            }

                            // �f�[�^�o�^����
                            RegistInventory(line, form, inventorySchedule);
                        }
                        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                        {
                            try
                            {
                                // �f�[�^����̎��s�E�R�~�b�g
                                db.SubmitChanges();
                                // �g�����U�N�V�����̃R�~�b�g
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
                                    OutputLogger.NLogFatal(cfe, PROC_NAME_REC, FORM_NAME, "");
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
                                    OutputLogger.NLogError(e, PROC_NAME_REC, FORM_NAME, "");

                                    ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                                    // ���̑��o�͍��ڂ̐ݒ�
                                    GetEntryViewData(departmentCode, inventoryMonth);
                                    return View("InventoryEntry", line);
                                }
                                else
                                {
                                    // ���O�ɏo��
                                    OutputLogger.NLogFatal(e, PROC_NAME_REC, FORM_NAME, "");
                                    return View("Error");
                                }
                            }
                            catch (Exception e)
                            {
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                OutputLogger.NLogFatal(e, PROC_NAME_REC, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                    }
                    // ���̑��o�͍��ڂ̐ݒ�
                    GetEntryViewData(departmentCode, inventoryMonth);
                    // �o��
                    ModelState.Clear();
                    ViewData["close"] = "1";
                    return Entry(departmentCode + "," + string.Format("{0:yyyyMM}", inventoryMonth));
            }
        }

        /// <summary>
        /// �I���X�P�W���[�����擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>�I���X�P�W���[�����</returns>
        private InventorySchedule GetInventorySchedule(string departmentCode, DateTime inventoryMonth) {

            InventorySchedule inventoryScheduleCondition = new InventorySchedule();
            inventoryScheduleCondition.SetAuthCondition((Employee)Session["Employee"]);
            inventoryScheduleCondition.Department = new Department();
            inventoryScheduleCondition.Department.DepartmentCode = departmentCode;
            inventoryScheduleCondition.InventoryMonth = inventoryMonth;
            inventoryScheduleCondition.InventoryType = "001";

            return new InventoryScheduleDao(db).GetByKey(inventoryScheduleCondition);
        }

        /// <summary>
        /// �m��Ώێԗ��I���T�}���f�[�^�擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>�m��Ώێԗ��I���T�}���f�[�^</returns>
        private V_CarInventorySummary GetCarInventorySummaryForFix(string departmentCode, DateTime inventoryMonth) {

            V_CarInventorySummaryDao v_CarInventorySummaryDao = new V_CarInventorySummaryDao(db);
            V_CarInventorySummary v_CarInventorySummaryCondition = new V_CarInventorySummary();
            v_CarInventorySummaryCondition.DepartmentCode = departmentCode;
            v_CarInventorySummaryCondition.InventoryMonth = inventoryMonth;

            return v_CarInventorySummaryDao.GetListByCondition(v_CarInventorySummaryCondition, 0, DaoConst.PAGE_SIZE)[0];
        }

        /// <summary>
        /// �ԗ��I���������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��I���������ʃ��X�g</returns>
        private PaginatedList<V_CarInventorySummary> GetSearchResultList(FormCollection form) {

            V_CarInventorySummaryDao v_CarInventorySummaryDao = new V_CarInventorySummaryDao(db);
            V_CarInventorySummary v_CarInventorySummaryCondition = new V_CarInventorySummary();
            v_CarInventorySummaryCondition.SetAuthCondition((Employee)Session["Employee"]);
            v_CarInventorySummaryCondition.CompanyCode = form["CompanyCode"];
            v_CarInventorySummaryCondition.OfficeCode = form["OfficeCode"];
            v_CarInventorySummaryCondition.DepartmentCode = form["DepartmentCode"];
            v_CarInventorySummaryCondition.InventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");

            return v_CarInventorySummaryDao.GetListByCondition(v_CarInventorySummaryCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ������ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void GetCriteriaViewData(FormCollection form) {

            // �����������̉�ʕ\���f�[�^�擾
            ViewData["CompanyCode"] = form["CompanyCode"];
            if (!string.IsNullOrEmpty(form["CompanyCode"])) {
                Company company = new CompanyDao(db).GetByKey(form["CompanyCode"]);
                if (company != null) {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["InventoryMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetInventoryMonthsList(), form["InventoryMonth"], false);
        }

        /// <summary>
        /// ���͉�ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="inventoryMonth">�I����</param>
        private void GetEntryViewData(string departmentCode, DateTime inventoryMonth) {

            // �p���ێ�������̐ݒ�
            ViewData["DepartmentCode"] = departmentCode;
            ViewData["InventoryMonth"] = inventoryMonth;

            // �I���X�P�W���[���֘A���̎擾
            InventorySchedule inventorySchedule = GetInventorySchedule(departmentCode, inventoryMonth);
            if (inventorySchedule != null) {
                try { ViewData["CompanyName"] = inventorySchedule.Department.Office.Company.CompanyName; } catch (NullReferenceException) { }
                try { ViewData["OfficeName"] = inventorySchedule.Department.Office.OfficeName; } catch (NullReferenceException) { }
                try { ViewData["DepartmentName"] = inventorySchedule.Department.DepartmentName; } catch (NullReferenceException) { }
                ViewData["LastUpdateDate"] = inventorySchedule.LastUpdateDate;
            }
        }

        /// <summary>
        /// ���n�����͖��׃\�[�g
        /// </summary>
        /// <param name="line">���n�����͖���</param>
        /// <param name="sortKey">�\�[�g�L�[</param>
        /// <returns>�\�[�g�ςݎ��n�����͖���</returns>
        private List<V_CarInventoryInProcess> SortLine(List<V_CarInventoryInProcess> line, string sortKey) {

            line.Sort(delegate(V_CarInventoryInProcess x, V_CarInventoryInProcess y) {
                string xVal = CommonUtils.DefaultString(x.GetType().GetProperty(sortKey).GetValue(x, null));
                string yVal = CommonUtils.DefaultString(y.GetType().GetProperty(sortKey).GetValue(y, null));
                string xVin = CommonUtils.DefaultString(x.Vin);
                string yVin = CommonUtils.DefaultString(y.Vin);
                return (xVal.Equals(yVal) ? xVin.CompareTo(yVin) : xVal.CompareTo(yVal));
            });

            return line;
        }

        /// <summary>
        /// �ԗ��I�����̓`�F�b�N
        /// </summary>
        /// <param name="line">�ԗ��I�����n���̓f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��I���f�[�^</returns>
        private List<V_CarInventoryInProcess> ValidateInventory(List<V_CarInventoryInProcess> line, FormCollection form) {

            bool alreadyOutputMsgE0004 = false;
            for (int i = 0; i < line.Count; i++) {

                V_CarInventoryInProcess inventory = line[i];
                string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                // �����`�F�b�N
                if (!ModelState.IsValidField(prefix + "Quantity")) {
                    ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0004", new string[] { "���n��", "0�܂���1�̂�" })));
                    alreadyOutputMsgE0004 = true;
                }

                // �l�`�F�b�N
                if (ModelState.IsValidField(prefix + "Quantity") && inventory.Quantity != null) {
                    if ((inventory.Quantity == 0) || (inventory.Quantity == 1)) {
                    } else {
                        ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0004", new string[] { "���n��", "0�܂���1�̂�" })));
                        alreadyOutputMsgE0004 = true;
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// �ԗ��I�����דo�^����
        /// </summary>
        /// <param name="line">�ԗ��I�����n���̓f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventorySchedule">�I���X�P�W���[���f�[�^</param>
        private void RegistInventory(List<V_CarInventoryInProcess> line, FormCollection form, InventorySchedule inventorySchedule) {

            // ���n�����ׂ̓o�^
            for (int i = 0; i < line.Count; i++) {

                V_CarInventoryInProcess inventory = line[i];

                // �I���e�[�u���ǉ�
                if (string.IsNullOrEmpty(CommonUtils.DefaultString(inventory.InventoryId))) {

                    db.Inventory.InsertOnSubmit(EditInventoryForInsert(inventory, form));

                // �I���e�[�u���X�V
                } else {

                    EditInventoryForUpdate(new InventoryDao(db).GetByKey(inventory.InventoryId ?? new Guid()), inventory);
                }
            }

            // �I���X�P�W���[���e�[�u���X�V
            EditInventoryScheduleForUpdate(inventorySchedule);
        }

        /// <summary>
        /// �ԗ��I���ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="input">�ԗ��I�����n���̓f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="cashBalance">�ԗ��I���f�[�^(�o�^���e)</param>
        private Inventory EditInventoryForInsert(V_CarInventoryInProcess input, FormCollection form) {

            Inventory inventory = new Inventory();
            inventory.InventoryId = Guid.NewGuid();
            inventory.DepartmentCode = form["DepartmentCode"];
            inventory.InventoryMonth = DateTime.Parse(form["InventoryMonth"] + "/01 00:00:00.000");
            inventory.LocationCode = input.LocationCode;
            inventory.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.InventoryType = "001";
            inventory.SalesCarNumber = input.SalesCarNumber;
            inventory.Quantity = input.Quantity;
            inventory.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.CreateDate = DateTime.Now;
            inventory.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventory.LastUpdateDate = DateTime.Now;
            inventory.DelFlag = "0";

            return inventory;
        }

        /// <summary>
        /// �ԗ��I���X�V�f�[�^�ҏW
        /// </summary>
        /// <param name="oldData">�X�V�O�f�[�^</param>
        /// <param name="newData">�ԗ��I�����n���̓f�[�^</param>
        /// <param name="cashBalance">�ԗ��I���f�[�^(�o�^���e)</param>
        private Inventory EditInventoryForUpdate(Inventory oldData, V_CarInventoryInProcess newData) {

            oldData.Quantity = newData.Quantity;
            oldData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            oldData.LastUpdateDate = DateTime.Now;

            return oldData;
        }

        /// <summary>
        /// �ԗ��I���X�P�W���[���X�V�f�[�^�ҏW
        /// </summary>
        /// <param name="inventorySchedule">�ԗ��I���X�P�W���[���X�V�Ώۃf�[�^</param>
        /// <param name="cashBalance">�ԗ��I���X�P�W���[���f�[�^(�o�^���e)</param>
        private InventorySchedule EditInventoryScheduleForUpdate(InventorySchedule inventorySchedule) {

            inventorySchedule.InventoryStatus = "002";
            if (inventorySchedule.StartDate == null) {
                inventorySchedule.StartDate = DateTime.Now.Date;
            }
            inventorySchedule.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            inventorySchedule.LastUpdateDate = DateTime.Now;

            return inventorySchedule;
        }
    }
}
