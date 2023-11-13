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
    /// �ԗ�����@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarAppraisalController : Controller {

        #region ������
        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��������";     // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ�����o�^"; // ������
        //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
        //private static readonly string LAST_EDIT_APPRAISAL = "002";           // �����ʂōX�V�������̒l

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarAppraisalController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion

        #region �������
        /// <summary>
        /// �ԗ����茟����ʕ\��
        /// </summary>
        /// <returns>�ԗ����茟�����</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ����茟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ����茟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["PurchaseStatus"] = (form["PurchaseStatus"] == null ? "001" : form["PurchaseStatus"]);
            ViewData["DefaultPurchaseStatus"] = "001";

            // �������ʃ��X�g�̎擾
            PaginatedList<V_CarAppraisal> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["Vin"] = form["Vin"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["CreateDateFrom"] = form["CreateDateFrom"];
            ViewData["CreateDateTo"] = form["CreateDateTo"];
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(dao.GetPurchaseStatusAll(false), form["PurchaseStatus"], true);

            // �ԗ����茟����ʂ̕\��
            return View("CarAppraisalCriteria", list);
        }
        /// <summary>
        /// �ԗ�����r���[�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ�����r���[�������ʃ��X�g</returns>
        private PaginatedList<V_CarAppraisal> GetSearchResultList(FormCollection form) {

            V_CarAppraisalDao v_CarAppraisalDao = new V_CarAppraisalDao(db);
            V_CarAppraisal v_CarAppraisalCondition = new V_CarAppraisal();
            v_CarAppraisalCondition.Vin = form["Vin"];
            v_CarAppraisalCondition.SlipNumber = form["SlipNumber"];
            v_CarAppraisalCondition.CreateDateFrom = CommonUtils.GetDayStart(form["CreateDateFrom"], DaoConst.SQL_DATETIME_MAX);
            v_CarAppraisalCondition.CreateDateTo = CommonUtils.GetDayEnd(form["CreateDateTo"], DaoConst.SQL_DATETIME_MIN);
            v_CarAppraisalCondition.PurchaseStatus = form["PurchaseStatus"];

            return v_CarAppraisalDao.GetListByCondition(v_CarAppraisalCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region ���͉��
        /// <summary>
        /// �ԗ�������͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ�����ID</param>
        /// <returns>�ԗ�������͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            CarAppraisal carAppraisal;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                carAppraisal = new CarAppraisal();
                carAppraisal.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carAppraisal.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //ookubo
                //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
                if (carAppraisal.ConsumptionTaxId == null)
                {
                    carAppraisal.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    carAppraisal.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carAppraisal.ConsumptionTaxId));
                }
                carAppraisal.LastEditScreen = "000";

            }
            // �X�V�̏ꍇ
            else {
                ViewData["update"] = "1";
                carAppraisal = new CarAppraisalDao(db).GetByKey(new Guid(id));
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(carAppraisal);

            // �o��
            return View("CarAppraisalEntry", carAppraisal);
        }

        /// <summary>
        /// �ԗ�����o�^
        /// </summary>
        /// <param name="carAppraisal">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ�����e�[�u�����͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarAppraisal carAppraisal, FormCollection form) {

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];
            //ViewData["OwnerCode"] = form["OwnerCode"];

            // �f�[�^�`�F�b�N
            ValidateCarAppraisal(carAppraisal);

            if (form["action"].Equals("savePurchase")) {

                ValidateForInsert(carAppraisal);
                if (!ModelState.IsValid) {
                    GetEntryViewData(carAppraisal);
                    return View("CarAppraisalEntry", carAppraisal);
                }
            }
            if (!ModelState.IsValid) {
                GetEntryViewData(carAppraisal);
                return View("CarAppraisalEntry", carAppraisal);
            }

            // Add 2014/08/07 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �e��f�[�^�o�^����
            using (TransactionScope ts = new TransactionScope()) {

                // �ԗ�����f�[�^�o�^����
                //�X�V
                if (form["update"].Equals("1")) {
                    CarAppraisal targetCarAppraisal = new CarAppraisalDao(db).GetByKey(carAppraisal.CarAppraisalId);
                    UpdateModel(targetCarAppraisal);
                    carAppraisal = EditCarAppraisalForUpdate(targetCarAppraisal, form);

                    //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
                    //Add 2016/09/17 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                    //�X�V���A�����(�`�[�ԍ�����)�̏ꍇ�͊Y���ԗ��̎d���f�[�^�����݂����ꍇ�͍X�V����
                    /*if (!string.IsNullOrEmpty(targetCarAppraisal.SlipNumber))
                    {
                        CarSalesHeader SlipData = new CarSalesOrderDao(db).GetBySlipNumber(targetCarAppraisal.SlipNumber);

                        UpdateCarCarPurchase(targetCarAppraisal, targetCarAppraisal.SlipNumber, targetCarAppraisal.Vin, SlipData);

                        if (SlipData != null)
                        {
                            CreateTradeReceiptPlan(SlipData, targetCarAppraisal);
                        }
                    }
                    else
                    {
                        targetCarAppraisal.LastEditScreen = "000";
                    }*/
                }
                else 
                //�V�K�o�^
                {
                    carAppraisal = EditCarAppraisalForInsert(carAppraisal, form);
                    form["reportParam"] = carAppraisal.CarAppraisalId.ToString();
                    db.CarAppraisal.InsertOnSubmit(carAppraisal);
                }

                // ���ח\��f�[�^�o�^����
                if (form["action"].Equals("savePurchase")) {
                    // �Â��Ǘ��ԍ��𗚗��e�[�u���Ɉړ�
                    if (carAppraisal.RegetVin) {
                        List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(carAppraisal.Vin);
                        foreach (var item in salesCarList) {
                            CommonUtils.CopyToSalesCarHistory(db, item);
                            item.DelFlag = "1";
                        }
                    }
                    SalesCar salesCar = EditSalesCarForInsert(carAppraisal);
                    db.SalesCar.InsertOnSubmit(salesCar);
                    CarPurchase carPurchase = EditCarPurchaseForInsert(carAppraisal, salesCar);
                    db.CarPurchase.InsertOnSubmit(carPurchase);
                }

                // DB�A�N�Z�X�̎��s
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        // �R�~�b�g
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        // Mod 2014/08/07 arc amii �G���[���O�Ή� ChangeConflictException�������̏�����ǉ�
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        // Mod 2014/08/07 arc amii �G���[���O�Ή� Exception�������A���O�o�͏���������悤�C��
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("savePurchase") ? "���ח\��쐬" : "�ۑ�")));
                            GetEntryViewData(carAppraisal);
                            return View("CarAppraisalEntry", carAppraisal);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Mod 2014/08/07 arc amii �G���[���O�Ή� Exception�������A���O�o�͏���������悤�C��
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, carAppraisal.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                
            }
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
                ViewData["reportParam"] = form["reportParam"];
            } else {
                // �o��
                ViewData["close"] = "1";
            }
            ModelState.Clear();

            return Entry(carAppraisal.CarAppraisalId.ToString());
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carAppraisal">���f���f�[�^</param>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// </history>
        private void GetEntryViewData(CarAppraisal carAppraisal) {

            // �ԗ��`�[���̎擾
            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber)) {
                CarSalesHeader carSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(carAppraisal.SlipNumber);
                if (carSalesHeader != null) {
                    ViewData["SlipNumber"] = carSalesHeader.SlipNumber;
                    ViewData["SalesOrderDate"] = string.Format("{0:yyyy/MM/dd}", carSalesHeader.SalesOrderDate);
                    try { ViewData["OrderDepartmentName"] = carSalesHeader.Department.DepartmentName; } catch (NullReferenceException) { }
                    try { ViewData["OrderEmployeeName"] = carSalesHeader.Employee.EmployeeName; } catch (NullReferenceException) { }
                    try { ViewData["CustomerName"] = carSalesHeader.Customer.CustomerName; } catch (NullReferenceException) { }
                }
            }

            // �ԗ�������̎擾
            if (carAppraisal.CarAppraisalId != null) {
                CarAppraisal dbCarAppraisal = new CarAppraisalDao(db).GetByKey(carAppraisal.CarAppraisalId);
                if (dbCarAppraisal != null) {
                    ViewData["PurchaseCreated"] = dbCarAppraisal.PurchaseCreated;

                }
            }

            // �ԗ����׏��̎擾
            if (carAppraisal.CarAppraisalId != null) {
                CarPurchase dbCarPurchase = new CarPurchaseDao(db).GetByCarAppraisalId(carAppraisal.CarAppraisalId);
                if (dbCarPurchase != null) {
                    ViewData["PurchaseStatus"] = dbCarPurchase.PurchaseStatus;
                    try { ViewData["PurchaseStatusName"] = dbCarPurchase.c_PurchaseStatus.Name; } catch (NullReferenceException) { }
                    ViewData["PurchaseDate"] = string.Format("{0:yyyy/MM/dd}", dbCarPurchase.PurchaseDate);
                }
            }

            // ���喼�̎擾
            if (!string.IsNullOrEmpty(carAppraisal.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(carAppraisal.DepartmentCode);
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // �S���Җ��̎擾
            if (!string.IsNullOrEmpty(carAppraisal.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(carAppraisal.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    carAppraisal.Employee = employee;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), carAppraisal.CarClassification, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), carAppraisal.Usage, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), carAppraisal.UsageType, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), carAppraisal.Figure, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), carAppraisal.MileageUnit, false);
            ViewData["DocumentCompleteList"] = CodeUtils.GetSelectListByModel(dao.GetDocumentCompleteAll(false), carAppraisal.DocumentComplete, true);
            ViewData["TransMissionList"] = CodeUtils.GetSelectListByModel(dao.GetTransMissionAll(false), carAppraisal.TransMission, true);
            ViewData["ChangeColorList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.ChangeColor, true);
            ViewData["GuaranteeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.Guarantee, true);
            ViewData["InstructionsList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.Instructions, true);
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), carAppraisal.Steering, true);
            ViewData["ImportList"] = CodeUtils.GetSelectListByModel(dao.GetImportAll(false), carAppraisal.Import, true);
            ViewData["LightList"] = CodeUtils.GetSelectListByModel(dao.GetLightAll(false), carAppraisal.Light, true);
            ViewData["AwList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Aw, true);
            ViewData["AeroList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Aero, true);
            ViewData["SrList"] = CodeUtils.GetSelectListByModel(dao.GetSrAll(false), carAppraisal.Sr, true);
            ViewData["CdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Cd, true);
            ViewData["MdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.Md, true);
            ViewData["NaviTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), carAppraisal.NaviType, true);
            ViewData["NaviEquipmentList"] = CodeUtils.GetSelectListByModel(dao.GetNaviEquipmentAll(false), carAppraisal.NaviEquipment, true);
            ViewData["NaviDashboardList"] = CodeUtils.GetSelectListByModel(dao.GetNaviDashboardAll(false), carAppraisal.NaviDashboard, true);
            ViewData["SeatTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSeatTypeAll(false), carAppraisal.SeatType, true);
            ViewData["RecycleList"] = CodeUtils.GetSelectListByModel(dao.GetRecycleAll(false), carAppraisal.Recycle, true);
            ViewData["RecycleTicketList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.RecycleTicket, true);
            ViewData["ReparationRecordList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), carAppraisal.ReparationRecord, true);
            ViewData["EraseRegistList"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carAppraisal.EraseRegist, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), carAppraisal.Fuel, true);

            //ADD 2014/02/21 ookubo
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carAppraisal.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carAppraisal.ConsumptionTaxId;
            ViewData["Rate"] = carAppraisal.Rate;
            ViewData["ConsumptionTaxIdOld"] = carAppraisal.ConsumptionTaxId;
            ViewData["PurchasePlanDateOld"] = carAppraisal.PurchasePlanDate;
            //ADD end

            //Mod 2021/08/02 yano #4097 �R�����g�A�E�g
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 ���f���N�̌�����4���𒴂��Ă����ꍇ�A4���ŕ\������
            //if (CommonUtils.DefaultString(carAppraisal.ModelYear).Length > 10)
            //{
            //    carAppraisal.ModelYear = carAppraisal.ModelYear.Substring(0, 10);
            //}

            //Add 2016/09/05 arc nakayama #3630_�y�����z�ԗ����|���Ή� �ԗ��`�[�E����E�d���̃f�[�^�A�g
            //�`�[�ԍ�������ꍇ�A�d���f�[�^���`�F�b�N���Ďd���ςȂ���͗������[�h�I�����[�ɂ���t���O�𗧂Ă�
            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber))
            {
                CarPurchase CarPurchase = new CarPurchaseDao(db).GetBySlipNumberVin(carAppraisal.SlipNumber, carAppraisal.Vin);
                if (CarPurchase != null)
                {
                    if (CarPurchase.PurchaseStatus == "002" && !new InventoryScheduleDao(db).IsClosedInventoryMonth(CarPurchase.DepartmentCode, CarPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["PurchasedFlag"] = "1";
                    }
                    else
                    {
                        ViewData["PurchasedFlag"] = "0";
                    }
                }
            }
            else
            {
                ViewData["PurchasedFlag"] = "0";
            }

            //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~ 
            //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            //�Ō�ɋ��z�̕ϓ�����������ʂ��ԗ��d����ʂłȂ���΃��b�Z�[�W�\��
            /*if (!carAppraisal.LastEditScreen.Equals(LAST_EDIT_APPRAISAL))
            {
                switch (carAppraisal.LastEditScreen)
                {
                    case "001":
                        carAppraisal.LastEditMessage = "�ԗ��`�[���獸�艿�i�A�c�A���������ԐŁA���T�C�N���a�����̊e���z���ύX����܂����B";
                        break;
                    case "003":
                        carAppraisal.LastEditMessage = "�ԗ��d����ʂ��獸�艿�i�A�c�A���������ԐŁA���T�C�N���a�����̊e���z���ύX����܂����B";
                        break;
                    default:
                        carAppraisal.LastEditMessage = "";
                        break;
                }

            }
            else
            {
                carAppraisal.LastEditMessage = "";
            }*/
        }

        /// <summary>
        /// �d���\��쐬����Validation�`�F�b�N
        /// </summary>
        /// <param name="carAppraisal"></param>
        private void ValidateForInsert(CarAppraisal carAppraisal) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(carAppraisal.Vin);
            if (list != null && list.Count > 0 && !carAppraisal.RegetVin) {
                ModelState.AddModelError("Vin", "�ԑ�ԍ�:" + carAppraisal.Vin + "�͊��ɓo�^����Ă��܂�");
                ViewData["ErrorSalesCar"] = list;
            }
            for (int i = 0; i < list.Count(); i++) {
                if (list[i].CarStatus != null && !list[i].CarStatus.Equals("006") && !list[i].CarStatus.Equals("")) {
                    ModelState.AddModelError("Vin", list[i].Vin + " (" + (i + 1) + ")�̍݌ɃX�e�[�^�X���u" + list[i].c_CarStatus.Name + "�v�̂��ߊǗ��ԍ��̍Ď擾���o���܂���");
                }
            }

            if (string.IsNullOrEmpty(carAppraisal.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0007", new string[] { "�d���\��쐬", "�O���[�h" }));
            }
           
            if (carAppraisal.PurchasePlanDate == null && ModelState["PurchasePlanDate"].Errors.Count() == 0) {
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0007", new string[] { "�d���\��쐬", "�d���\���" }));
            }
        }
        /// <summary>
        /// �ԗ�����e�[�u���ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carAppraisal">�ԗ�����e�[�u���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ�����e�[�u�����f���N���X</returns>
        private CarAppraisal EditCarAppraisalForInsert(CarAppraisal carAppraisal, FormCollection form) {

            carAppraisal.CarAppraisalId = Guid.NewGuid();
            if (form["action"].Equals("savePurchase")) {
                carAppraisal.PurchaseCreated = "1";
            } else {
                carAppraisal.PurchaseCreated = "0";
            }
            carAppraisal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.CreateDate = DateTime.Now;
            carAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.LastUpdateDate = DateTime.Now;
            carAppraisal.DelFlag = "0";
            return carAppraisal;
        }

        /// <summary>
        /// �ԗ�����e�[�u���X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carAppraisal">�ԗ�����e�[�u���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ�����e�[�u�����f���N���X</returns>
        private CarAppraisal EditCarAppraisalForUpdate(CarAppraisal carAppraisal, FormCollection form) {

            if (form["action"].Equals("savePurchase")) {
                carAppraisal.PurchaseCreated = "1";
            }
            carAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carAppraisal.LastUpdateDate = DateTime.Now;
            return carAppraisal;
        }

        #region �ԗ��}�X�^�쐬
        /// <summary>
        /// �ԗ��e�[�u���ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="carAppraisal">�ԗ�����e�[�u���ǉ�/�X�V�f�[�^�ҏW��̎ԗ�����e�[�u���f�[�^(�o�^���e)</param>
        /// <returns>�ԗ��e�[�u�����f���N���X</returns>
        private SalesCar EditSalesCarForInsert(CarAppraisal carAppraisal) {

            SalesCar salesCar = new SalesCar();
            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(carAppraisal.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, "U");
            salesCar.CarGradeCode = carAppraisal.CarGradeCode;
            salesCar.NewUsedType = "U";
            salesCar.ExteriorColorName = carAppraisal.ExteriorColorName;
            salesCar.ChangeColor = carAppraisal.ChangeColor;
            salesCar.InteriorColorName = carAppraisal.InteriorColorName;
            salesCar.Steering = carAppraisal.Steering;
            salesCar.IssueDate = carAppraisal.IssueDate;
            salesCar.MorterViecleOfficialCode = carAppraisal.MorterViecleOfficialCode;
            salesCar.RegistrationNumberType = carAppraisal.RegistrationNumberType;
            salesCar.RegistrationNumberKana = carAppraisal.RegistrationNumberKana;
            salesCar.RegistrationNumberPlate = carAppraisal.RegistrationNumberPlate;
            salesCar.RegistrationDate = carAppraisal.RegistrationDate;
            salesCar.FirstRegistrationYear = carAppraisal.FirstRegistrationYear;
            salesCar.CarClassification = carAppraisal.CarClassification;
            salesCar.Usage = carAppraisal.Usage;
            salesCar.UsageType = carAppraisal.UsageType;
            salesCar.Figure = carAppraisal.Figure;
            salesCar.MakerName = carAppraisal.MakerName;
            salesCar.Capacity = carAppraisal.Capacity;
            salesCar.MaximumLoadingWeight = carAppraisal.MaximumLoadingWeight;
            salesCar.CarWeight = carAppraisal.CarWeight;
            salesCar.TotalCarWeight = carAppraisal.TotalCarWeight;
            salesCar.Vin = carAppraisal.Vin;
            salesCar.Length = carAppraisal.Length;
            salesCar.Width = carAppraisal.Width;
            salesCar.Height = carAppraisal.Height;
            salesCar.FFAxileWeight = carAppraisal.FFAxileWeight;
            salesCar.FRAxileWeight = carAppraisal.FRAxileWeight;
            salesCar.RFAxileWeight = carAppraisal.RFAxileWeight;
            salesCar.RRAxileWeight = carAppraisal.RRAxileWeight;
            salesCar.ModelName = carAppraisal.ModelName;
            salesCar.EngineType = carAppraisal.EngineType;
            salesCar.Displacement = carAppraisal.Displacement;
            salesCar.Fuel = carAppraisal.Fuel;
            salesCar.ModelSpecificateNumber = carAppraisal.ModelSpecificateNumber;
            salesCar.ClassificationTypeNumber = carAppraisal.ClassificationTypeNumber;
            salesCar.PossesorName = carAppraisal.PossesorName;
            salesCar.PossesorAddress = carAppraisal.PossesorAddress;
            salesCar.UserName = carAppraisal.UserName;
            salesCar.UserAddress = carAppraisal.UserAddress;
            salesCar.PrincipalPlace = carAppraisal.PrincipalPlace;
            salesCar.ExpireType = "001";
            salesCar.ExpireDate = carAppraisal.InspectionExpireDate;
            salesCar.Mileage = carAppraisal.Mileage;
            salesCar.MileageUnit = carAppraisal.MileageUnit;
            salesCar.Memo = carAppraisal.Memo;
            salesCar.DocumentComplete = carAppraisal.DocumentComplete;
            salesCar.DocumentRemarks = carAppraisal.DocumentRemarks;
            salesCar.UsVin = carAppraisal.UsVin;
            salesCar.ReparationRecord = carAppraisal.ReparationRecord;
            salesCar.Import = carAppraisal.Import;
            salesCar.Guarantee = carAppraisal.Guarantee;
            salesCar.Instructions = carAppraisal.Instructions;
            salesCar.Recycle = carAppraisal.Recycle;
            salesCar.RecycleTicket = carAppraisal.RecycleTicket;
            salesCar.Light = carAppraisal.Light;
            salesCar.Aw = carAppraisal.Aw;
            salesCar.Aero = carAppraisal.Aero;
            salesCar.Sr = carAppraisal.Sr;
            salesCar.Cd = carAppraisal.Cd;
            salesCar.Md = carAppraisal.Md;
            salesCar.NaviType = carAppraisal.NaviType;
            salesCar.NaviEquipment = carAppraisal.NaviEquipment;
            salesCar.NaviDashboard = carAppraisal.NaviDashboard;
            salesCar.SeatColor = carAppraisal.SeatColor;
            salesCar.SeatType = carAppraisal.SeatType;
            salesCar.RecycleDeposit = carAppraisal.RecycleDeposit;
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            salesCar.EraseRegist = carAppraisal.EraseRegist;
            return salesCar;
        }

        #endregion

        #region ���ח\��쐬
        /// <summary>
        /// �ԗ����׃e�[�u���ǉ��f�[�^�ҏW
        /// </summary>
        /// <param name="carAppraisal">�ԗ�����e�[�u���ǉ�/�X�V�f�[�^�ҏW��̎ԗ�����e�[�u���f�[�^(�o�^���e)</param>
        /// <param name="salesCar">�ԗ��e�[�u���ǉ��f�[�^�ҏW��̎ԗ��e�[�u���f�[�^(�o�^���e)</param>
        /// <returns>�ԗ����׃e�[�u�����f���N���X</returns>
        /// <hitory>
        /// 2021/08/16 yano #4001 ����d���̓������т��쐬����Ȃ��B
        /// 2018/04/04 arc ayno #3741 ������́@�d���\��쐬���̎d�����z�i���z�j�Ɗe���ڂ̋��z�̍��v�̕s��v�ɂ���
        /// </hitory>
        private CarPurchase EditCarPurchaseForInsert(CarAppraisal carAppraisal, SalesCar salesCar) {

            CarPurchase carPurcahase = new CarPurchase();
            carPurcahase.CarPurchaseId = Guid.NewGuid();
            carPurcahase.CarAppraisalId = carAppraisal.CarAppraisalId;
            carPurcahase.PurchaseStatus = "001";
            //carPurcahase.VehiclePrice = carAppraisal.AppraisalPrice ?? 0m;

            // �d�����z�i�ō��j
            carPurcahase.TotalAmount = carAppraisal.AppraisalPrice ?? 0m;

            // ���ŏ[��
            carPurcahase.CarTaxAppropriateAmount = carAppraisal.CarTaxUnexpiredAmount ?? 0m; // ���ŏ[�������������Ԑ�(��ʊ�)
            //#3037 2014/06/10 CarTaxAppropriatePrice�ɐŔ��z�ACarTaxAppropriateTax�ɐŊz��ݒ肷��@arc.ookubo
            //carPurcahase.CarTaxAppropriatePrice = carAppraisal.CarTaxUnexpiredAmount ?? 0m;
            carPurcahase.CarTaxAppropriateTax = Math.Truncate((carAppraisal.CarTaxUnexpiredAmount ?? 0m) * carAppraisal.Rate / (100 + carAppraisal.Rate));
            carPurcahase.CarTaxAppropriatePrice = (carPurcahase.CarTaxAppropriateAmount ?? 0m) - (carPurcahase.CarTaxAppropriateTax ?? 0m);

            // ���T�C�N��
            carPurcahase.RecycleAmount = carAppraisal.RecycleDeposit ?? 0m;
            carPurcahase.RecyclePrice = carAppraisal.RecycleDeposit ?? 0m;

            // �ԗ��{�̉��i
            carPurcahase.VehicleAmount = (carAppraisal.AppraisalPrice ?? 0m) - (carAppraisal.CarTaxUnexpiredAmount ?? 0m) - (carAppraisal.RecycleDeposit ?? 0m);
            //#3022] �Ԃ̍��艿�i���d���֔��f������ƂT���Ōv�Z
            //MOD ookubo 2014/04/11
            //carPurcahase.VehicleTax = Math.Truncate((carPurcahase.VehicleAmount ?? 0m) * 5 / 105);
            carPurcahase.VehicleTax = Math.Truncate((carPurcahase.VehicleAmount ?? 0m) * carAppraisal.Rate / (100 + carAppraisal.Rate));
            carPurcahase.VehiclePrice = (carPurcahase.VehicleAmount ?? 0m) - (carPurcahase.VehicleTax ?? 0m);

            // �I�[�N�V�������D��
            carPurcahase.AuctionFeeAmount = 0m;
            carPurcahase.AuctionFeePrice = 0m;
            carPurcahase.AuctionFeeTax = 0m;

            // ���^���b�N
            carPurcahase.MetallicPrice = 0m;
            carPurcahase.MetallicAmount = 0m;
            carPurcahase.MetallicTax = 0m;

            // �I�v�V����
            carPurcahase.OptionPrice = 0m;
            carPurcahase.OptionAmount = 0m;
            carPurcahase.OptionTax = 0m;

            // �t�@�[��
            carPurcahase.FirmPrice = 0m;
            carPurcahase.FirmAmount = 0m;
            carPurcahase.FirmTax = 0m;

            // �l����
            carPurcahase.DiscountPrice = 0m;
            carPurcahase.DiscountAmount = 0m;
            carPurcahase.DiscountTax = 0m;

            // ����
            carPurcahase.EquipmentPrice = 0m;

            // ���C
            carPurcahase.RepairPrice = 0m;

            // ���̑�
            carPurcahase.OthersPrice = 0m;
            carPurcahase.OthersAmount = 0m;
            carPurcahase.OthersTax = 0m;

            // Mod 2018/04/04 arc yano #3741
            // �����
            carPurcahase.TaxAmount = (carPurcahase.VehicleTax ?? 0m) + (carPurcahase.CarTaxAppropriateTax ?? 0m);
            //carPurcahase.TaxAmount = carPurcahase.VehicleTax ?? 0m;

            // �d�����z�i�Ŕ��j
            carPurcahase.Amount = (carPurcahase.TotalAmount ?? 0m) - carPurcahase.TaxAmount;

            //#3022] �Ԃ̍��艿�i���d���֔��f������ƂT���Ōv�Z
            //ADD ookubo 2014/04/11�@�����ID�Ə���ŗ���ǉ�
            // �����ID
            carPurcahase.ConsumptionTaxId = carAppraisal.ConsumptionTaxId;

            // ����ŗ�
            carPurcahase.Rate = carAppraisal.Rate;

            carPurcahase.SalesCarNumber = salesCar.SalesCarNumber;
            carPurcahase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurcahase.CreateDate = DateTime.Now;
            carPurcahase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurcahase.LastUpdateDate = DateTime.Now;
            carPurcahase.DelFlag = "0";
            carPurcahase.EraseRegist = carAppraisal.EraseRegist;
            carPurcahase.PurchaseDate = carAppraisal.PurchasePlanDate;

            if (!string.IsNullOrEmpty(carAppraisal.SlipNumber)) {
                // �󒍓`�[�ƕR�Â��Ă��鎞
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(carAppraisal.SlipNumber);
                if (header != null) {
                    carPurcahase.DepartmentCode = header.DepartmentCode;
                    carPurcahase.CarPurchaseType = "001";
                    //Add 2016/08/26 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                    carPurcahase.SlipNumber = carAppraisal.SlipNumber;
                }
            } else {
                // �󒍓`�[�ƕR�Â��Ă��Ȃ���
                carPurcahase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carPurcahase.CarPurchaseType = "002";
            }
            carPurcahase.LastEditScreen = "000";

            //Add 2021/08/16 yano #4001
            carPurcahase.Vin = carAppraisal.Vin;

            return carPurcahase;
        }
        #endregion

        //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~ 
        /*#region ����Ɋ֘A����ԗ��d���f�[�^�Ɠ`�[�f�[�^���X�V����
        private void UpdateCarCarPurchase(CarAppraisal targetCarAppraisal, string SlipNumber, string vin, CarSalesHeader SlipData)
        {
            CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(SlipNumber, vin);

            //�ԗ��f�[�^�X�V
            if (CarPurchaseData != null && !SlipData.SalesOrderStatus.Equals("004") && !SlipData.SalesOrderStatus.Equals("005"))
            {
                if (CarPurchaseData.TotalAmount != targetCarAppraisal.AppraisalPrice || CarPurchaseData.CarTaxAppropriateAmount != targetCarAppraisal.CarTaxUnexpiredAmount || CarPurchaseData.RecycleAmount != targetCarAppraisal.RecycleDeposit)
                {
                    CarPurchaseData.LastEditScreen = LAST_EDIT_APPRAISAL;
                    targetCarAppraisal.LastEditScreen = "000";
                }
                else
                {
                    targetCarAppraisal.LastEditScreen = "000";
                }

                CarPurchaseData.TotalAmount = targetCarAppraisal.AppraisalPrice; //�d�����z = ������z
                CarPurchaseData.CarTaxAppropriateAmount = targetCarAppraisal.CarTaxUnexpiredAmount; //�����������Ԑ�
                CarPurchaseData.RecycleAmount = targetCarAppraisal.RecycleDeposit; //���T�C�N��(�ō�)
                CarPurchaseData.RecyclePrice = targetCarAppraisal.RecycleDeposit; // ���T�C�N��(�Ŕ�)
                CarPurchaseData.LastUpdateDate = DateTime.Now; //�ŐV�X�V��
                CarPurchaseData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).DepartmentCode; //�ŐV�X�V��
            }
            else
            {
                targetCarAppraisal.LastEditScreen = "000";
            }
        }
        #endregion

        #region ����Ԃ̓����\����č쐬����B�c������Ύc�̓����\����쐬����
        private void CreateTradeReceiptPlan(CarSalesHeader header, CarAppraisal targetCarAppraisal)
        {
            //�[�ԑO�E�[�ԍς̎��͓`�[�ɔ��f���Ȃ�
            //Add 2017/01/13 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            if (!header.SalesOrderStatus.Equals("004") && !header.SalesOrderStatus.Equals("005"))
            {

                //�����̓����\����폜
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => x.ReceiptType.Equals("012") || x.ReceiptType.Equals("013")).ToList();
                foreach (var d in delList)
                {
                    //Add 2016/09/05 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                    //�c�Ɖ���ȊO�̓����\��
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                //�Ȗڃf�[�^�擾
                Account carAccount = new AccountDao(db).GetByUsageType("CR");
                if (carAccount == null)
                {
                    ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                    return;
                }

                for (int i = 1; i <= 3; i++)
                {
                    object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                    if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                    {
                        string TradeInAmount = "0";
                        string TradeInRemainDebt = "0";
                        string TradeInRecycleAmount = "0";
                        string TradeInCarTaxAppropriateAmount = "0";
                        decimal PlanAmount = 0;
                        decimal PlanRemainDebt = 0;

                        //����̊Y���ԗ��̏ꍇ�͍���̋��z���Z�b�g����
                        if (targetCarAppraisal.Vin == vin.ToString())
                        {
                            TradeInAmount = targetCarAppraisal.AppraisalPrice.ToString();
                            TradeInRemainDebt = targetCarAppraisal.RemainDebt.ToString() ?? "0";
                            TradeInRecycleAmount = targetCarAppraisal.RecycleDeposit.ToString() ?? "0";
                            TradeInCarTaxAppropriateAmount = targetCarAppraisal.CarTaxUnexpiredAmount.ToString() ?? "0";
                            PlanAmount = targetCarAppraisal.AppraisalPrice ?? 0m;
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }
                        else
                        {
                            var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                            if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                            {
                                TradeInAmount = varTradeInAmount.ToString();
                            }

                            var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                            if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                            {
                                TradeInRemainDebt = varTradeInRemainDebt.ToString();
                            }

                            var varCarTaxAppropriateAmount = CommonUtils.GetModelProperty(header, "TradeInUnexpiredCarTax" + i);
                            if (varCarTaxAppropriateAmount != null && !string.IsNullOrEmpty(varCarTaxAppropriateAmount.ToString()))
                            {
                                TradeInCarTaxAppropriateAmount = varCarTaxAppropriateAmount.ToString();
                            }

                            var varTradeInRecycleAmount = CommonUtils.GetModelProperty(header, "TradeInRecycleAmount" + i);
                            if (varTradeInRecycleAmount != null && !string.IsNullOrEmpty(varTradeInRecycleAmount.ToString()))
                            {
                                TradeInRecycleAmount = varTradeInRecycleAmount.ToString();
                            }

                            PlanAmount = decimal.Parse(TradeInAmount);
                            PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                        }


                        //�ԗ��`�[�̋��z���X�V
                        bool editflag = false;

                        switch (i)
                        {
                            case 1:
                                if (header.TradeInAmount1 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt1 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount1 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax1 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount1 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt1 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation1 = (header.TradeInAmount1 ?? 0m) - (header.TradeInRemainDebt1 ?? 0m);//����ԑ��z(���承�i - ����c���z)
                                header.TradeInRecycleAmount1 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax1 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 2:
                                if (header.TradeInAmount2 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt2 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount2 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax2 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount2 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt2 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation2 = (header.TradeInAmount2 ?? 0m) - (header.TradeInRemainDebt2 ?? 0m);//����ԑ��z(���承�i - ����c���z)
                                header.TradeInRecycleAmount2 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax2 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 3:
                                if (header.TradeInAmount3 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt3 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount3 != decimal.Parse(TradeInRecycleAmount) || header.TradeInUnexpiredCarTax3 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount3 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt3 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation3 = (header.TradeInAmount2 ?? 0m) - (header.TradeInRemainDebt2 ?? 0m);//����ԑ��z(���承�i - ����c���z)
                                header.TradeInRecycleAmount3 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax3 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            default:
                                break;
                        }

                        if (editflag)
                        {
                            header.LastEditScreen = LAST_EDIT_APPRAISAL;
                        }
                        else
                        {
                            targetCarAppraisal.LastEditScreen = "000";
                        }

                        //����ԍ��v
                        header.TradeInTotalAmount = (header.TradeInAmount1 ?? 0m) + (header.TradeInAmount2 ?? 0m) + (header.TradeInAmount3 ?? 0m);
                        //�c���v
                        header.TradeInRemainDebtTotalAmount = (header.TradeInRemainDebt1 ?? 0m) + (header.TradeInRemainDebt2 ?? 0m) + (header.TradeInRemainDebt3 ?? 0m);
                        //����ԏ[�������z���v
                        header.TradeInAppropriationTotalAmount = (header.TradeInAppropriation1 ?? 0m) + (header.TradeInAppropriation2 ?? 0m) + (header.TradeInAppropriation3 ?? 0m);


                        decimal JournalAmount = 0; //����̓����z
                        decimal JournalDebtAmount = 0; //�c�̓����z

                        //����̓����z�擾
                        Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                        if (JournalData != null)
                        {
                            JournalAmount = JournalData.Amount;
                        }

                        //�c�̓����z�擾
                        Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                        if (JournalData2 != null)
                        {
                            JournalDebtAmount = JournalData2.Amount;
                        }

                        ReceiptPlan TradePlan = new ReceiptPlan();
                        TradePlan.ReceiptPlanId = Guid.NewGuid();
                        TradePlan.DepartmentCode = header.DepartmentCode;
                        TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                        TradePlan.CustomerClaimCode = header.CustomerCode;
                        TradePlan.SlipNumber = header.SlipNumber;
                        TradePlan.ReceiptType = "013"; //����
                        TradePlan.ReceiptPlanDate = null;
                        TradePlan.AccountCode = carAccount.AccountCode;
                        TradePlan.Amount = decimal.Parse(TradeInAmount);
                        TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //���v�Z
                        if (TradePlan.ReceivableBalance == 0m)
                        {
                            TradePlan.CompleteFlag = "1";
                        }
                        else
                        {
                            TradePlan.CompleteFlag = "0";
                        }
                        TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        TradePlan.CreateDate = DateTime.Now;
                        TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        TradePlan.LastUpdateDate = DateTime.Now;
                        TradePlan.DelFlag = "0";
                        TradePlan.Summary = "";
                        TradePlan.JournalDate = null;
                        TradePlan.DepositFlag = "0";
                        TradePlan.PaymentKindCode = "";
                        TradePlan.CommissionRate = null;
                        TradePlan.CommissionAmount = null;
                        TradePlan.CreditJournalId = "";

                        db.ReceiptPlan.InsertOnSubmit(TradePlan);

                        //�c���������ꍇ�c���̓����\����}�C�i�X�ō쐬����
                        if (!string.IsNullOrEmpty(TradeInRemainDebt))
                        {
                            ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                            RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                            RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�c�͌o���ɐU��ւ�
                            RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                            RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                            RemainDebtPlan.SlipNumber = header.SlipNumber;
                            RemainDebtPlan.ReceiptType = "012"; //�c��
                            RemainDebtPlan.ReceiptPlanDate = null;
                            RemainDebtPlan.AccountCode = carAccount.AccountCode;
                            RemainDebtPlan.Amount = PlanRemainDebt;
                            RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //�v�Z
                            if (RemainDebtPlan.ReceivableBalance == 0m)
                            {
                                RemainDebtPlan.CompleteFlag = "1";
                            }
                            else
                            {
                                RemainDebtPlan.CompleteFlag = "0";
                            }
                            RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            RemainDebtPlan.CreateDate = DateTime.Now;
                            RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            RemainDebtPlan.LastUpdateDate = DateTime.Now;
                            RemainDebtPlan.DelFlag = "0";
                            RemainDebtPlan.Summary = "";
                            RemainDebtPlan.JournalDate = null;
                            RemainDebtPlan.DepositFlag = "0";
                            RemainDebtPlan.PaymentKindCode = "";
                            RemainDebtPlan.CommissionRate = null;
                            RemainDebtPlan.CommissionAmount = null;
                            RemainDebtPlan.CreditJournalId = "";

                            db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                        }
                        //db.SubmitChanges();
                    }
                }
            }
        }

        #endregion
        */

        #endregion

        #region Ajax
        // ADD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʁ@�ԑ�ԍ����玩������
        /// <summary>
        /// �ԗ��R�[�h����ԗ����擾����
        /// </summary>
        /// <param name="code">�ԗ��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// </history>
        public ActionResult GetMasterDetail(string code, string SelectByCarSlip = "0")
        {

            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retCar = new Dictionary<string, string>();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null)
                {
                    retCar.Add("CarGradeCode", salesCar.CarGradeCode);
                    retCar.Add("MakerName", salesCar.CarGrade.Car.Brand.Maker.MakerName);
                    retCar.Add("CarBrandName", salesCar.CarGrade.Car.Brand.CarBrandName);
                    retCar.Add("CarGradeName", salesCar.CarGrade.CarGradeName);
                    retCar.Add("CarName", salesCar.CarGrade.Car.CarName);
                    retCar.Add("ExteriorColorCode", salesCar.ExteriorColorCode);
                    retCar.Add("ExteriorColorName", salesCar.ExteriorColorName == null ? "" : salesCar.ExteriorColorName);
                    retCar.Add("InteriorColorCode", salesCar.InteriorColorCode);
                    retCar.Add("InteriorColorName", salesCar.InteriorColorName == null ? "" : salesCar.InteriorColorName);
                    retCar.Add("Mileage", salesCar.Mileage.ToString());
                    retCar.Add("MileageUnit", salesCar.MileageUnit);
                    retCar.Add("ModelName", salesCar.ModelName);
                    retCar.Add("NewUsedType", salesCar.NewUsedType);
                    retCar.Add("SalesPrice", salesCar.SalesPrice.ToString());
                    //Mod 2015/07/28 arc nakayama #3217_�f���J�[���̔��ł��Ă��܂����̉��P �@�ԗ��`�[�̎ԑ�ԍ��̃��b�N�A�b�v����Ă΂ꂽ���@���@�݌ɃX�e�[�^�X���݌ɂ̎������Ǘ��ԍ���Ԃ�
                    //Mod 2015/08/20 arc nakayama #3242_�T�[�r�X�`�[�Ŏԗ��}�X�^�{�^���������Ă��ԗ��}�X�^���\������Ȃ� �ԗ��`�[����Ă΂ꂽ�������݌ɃX�e�[�^�X������悤�ɏC��
                    if (SelectByCarSlip.ToString().Equals("1"))
                    {
                        if (salesCar.CarStatus.Equals("001"))
                        {
                            retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                        }
                        else
                        {
                            retCar.Add("SalesCarNumber", "");
                        }
                    }
                    else
                    {
                        retCar.Add("SalesCarNumber", salesCar.SalesCarNumber);
                    }

                    retCar.Add("Vin", salesCar.Vin);
                    retCar.Add("LocationName", salesCar.Location != null ? salesCar.Location.LocationName : "");
                    retCar.Add("InheritedInsuranceFee", "");
                    //retCar.Add("CarWeightTax", salesCar.CarWeightTax.ToString());
                    retCar.Add("RecycleDeposit", salesCar.RecycleDeposit.ToString());
                    //retCar.Add("CarLiabilityInsurance", salesCar.CarLiabilityInsurance.ToString());

                    retCar.Add("EngineType", salesCar.EngineType);

                    // ADD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʗp�ɒǉ�
                    retCar.Add("FirstRegistrationYear", salesCar.FirstRegistrationYear);
                    retCar.Add("CarClassification", salesCar.CarClassification);
                    retCar.Add("Usage", salesCar.Usage);
                    retCar.Add("UsageType", salesCar.UsageType);
                    retCar.Add("Figure", salesCar.Figure);
                    retCar.Add("Capacity", salesCar.Capacity.ToString());
                    retCar.Add("MaximumLoadingWeight", salesCar.MaximumLoadingWeight.ToString());
                    retCar.Add("CarWeight", salesCar.CarWeight.ToString());
                    retCar.Add("TotalCarWeight", salesCar.TotalCarWeight.ToString());
                    retCar.Add("Length", salesCar.Length.ToString());
                    retCar.Add("Width", salesCar.Width.ToString());
                    retCar.Add("Height", salesCar.Height.ToString());
                    retCar.Add("FFAxileWeight", salesCar.FFAxileWeight.ToString());
                    retCar.Add("FRAxileWeight", salesCar.FRAxileWeight.ToString());
                    retCar.Add("RFAxileWeight", salesCar.RFAxileWeight.ToString());
                    retCar.Add("RRAxileWeight", salesCar.RRAxileWeight.ToString());
                    retCar.Add("ModelSpecificateNumber", salesCar.ModelSpecificateNumber);
                    retCar.Add("ClassificationTypeNumber", salesCar.ClassificationTypeNumber);
                    retCar.Add("Displacement", salesCar.Displacement.ToString());
                    retCar.Add("Fuel", salesCar.Fuel);
                    retCar.Add("OwnerCode", salesCar.OwnerCode);
                    retCar.Add("PossesorName", salesCar.PossesorName);
                    retCar.Add("PossesorAddress", salesCar.PossesorAddress);
                    retCar.Add("UserCode", salesCar.UserCode);
                    retCar.Add("UserName", salesCar.UserName);
                    retCar.Add("UserAddress", salesCar.UserAddress);
                    retCar.Add("PrincipalPlace", salesCar.PrincipalPlace);
                    retCar.Add("Memo", salesCar.Memo);
                    retCar.Add("DocumentRemarks", salesCar.DocumentRemarks);
                    retCar.Add("DocumentComplete", salesCar.DocumentComplete);
                    retCar.Add("IssueDate", string.Format("{0:yyyy/MM/dd}", salesCar.IssueDate));
                    retCar.Add("Guarantee", salesCar.Guarantee);
                    retCar.Add("Instructions", salesCar.Instructions);
                    retCar.Add("Steering", salesCar.Steering);
                    retCar.Add("Import", salesCar.Import);
                    retCar.Add("Light", salesCar.Light);
                    retCar.Add("Aw", salesCar.Aw);
                    retCar.Add("Aero", salesCar.Aero);
                    retCar.Add("Sr", salesCar.Sr);
                    retCar.Add("Cd", salesCar.Cd);
                    retCar.Add("Md", salesCar.Md);
                    retCar.Add("NaviType", salesCar.NaviType);
                    retCar.Add("NaviEquipment", salesCar.NaviEquipment);
                    retCar.Add("NaviDashboard", salesCar.NaviDashboard);
                    retCar.Add("SeatColor", salesCar.SeatColor);
                    retCar.Add("ChangeColor", salesCar.ChangeColor);
                    retCar.Add("SeatType", salesCar.SeatType);
                    retCar.Add("Recycle", salesCar.Recycle);
                    retCar.Add("RecycleTicket", salesCar.RecycleTicket);
                    
                    retCar.Add("ModelYear", salesCar.CarGrade.ModelYear);
                    retCar.Add("Door", salesCar.CarGrade.Door);
                    retCar.Add("TransMission", salesCar.CarGrade.TransMission);

                    // MOD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʗp�ɍ폜
                    //retCar.Add("NextInspectionDate", string.Format("{0:yyyy/MM/dd}", salesCar.NextInspectionDate));

                    retCar.Add("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", salesCar.ExpireDate));
                    retCar.Add("UsVin", salesCar.UsVin);
                    retCar.Add("MorterViecleOfficialCode", salesCar.MorterViecleOfficialCode);
                    retCar.Add("RegistrationNumberType", salesCar.RegistrationNumberType);
                    retCar.Add("RegistrationNumberKana", salesCar.RegistrationNumberKana);
                    retCar.Add("RegistrationNumberPlate", salesCar.RegistrationNumberPlate);

                    // MOD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʗp�ɍ폜
                    //retCar.Add("CustomerCode", salesCar.UserCode);

                    retCar.Add("CustomerName", salesCar.User != null ? salesCar.User.CustomerName : "");

                    // MOD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʗp�ɍ폜
                    //retCar.Add("CustomerNameKana", salesCar.User != null ? salesCar.User.CustomerNameKana : "");
                    //retCar.Add("CustomerAddress", salesCar.User != null ? salesCar.User.Prefecture + salesCar.User.City + salesCar.User.Address1 + salesCar.User.Address2 : "");
                    //retCar.Add("LaborRate", salesCar.CarGrade != null && salesCar.CarGrade.Car != null && salesCar.CarGrade.Car.Brand != null ? salesCar.CarGrade.Car.Brand.LaborRate.ToString() : "");

                    // Mod 2015/09/14 arc yano #3252 �T�[�r�X�`�[���͉�ʂ̃}�X�^�{�^���̋���(�ގ��Ή�) �ԑ�ԍ�����ԗ������擾�ł��鍀�ڂ̒ǉ�
                    retCar.Add("RegistrationDate", string.Format("{0:yyyy/MM/dd}", salesCar.RegistrationDate));         //�o�^�N����

                    // MOD 2016/02/16 ARC Mikami #3077 �ԗ�������͉�ʗp�ɍ폜
                    //retCar.Add("CustomerTelNumber", salesCar.User != null ? salesCar.User.TelNumber : "");              //�d�b�ԍ�  //Mod 2015/09/17 arc yano  #3261 �ԗ��`�[�̎ԗ��I���Łu�}�X�^�擾�Ɏ��s���܂����v�ƕ\�� NULL�̏ꍇ�͋󕶎��ɕϊ�

                    //���N�x�o�^(yyyy/mm)����N�����擾
                    decimal fee = 0m;
                    string firstRegistrationYear = salesCar.FirstRegistrationYear;
                    if (!string.IsNullOrEmpty(firstRegistrationYear))
                    {
                        if (firstRegistrationYear.Split('/').Length == 2)
                        {
                            string year = salesCar.FirstRegistrationYear.Split('/')[0];
                            string month = salesCar.FirstRegistrationYear.Split('/')[1];
                            DateTime firstRegist = new DateTime(int.Parse(year), int.Parse(month), 1);
                            DateTime today = DateTime.Today;
                            try
                            {
                                if (firstRegist.AddMonths(24).CompareTo(today) > 0)
                                {
                                    //24��������
                                    fee = salesCar.CarGrade.Under24 ?? 0;
                                }
                                else if (firstRegist.AddMonths(26).CompareTo(today) > 0)
                                {
                                    //26��������
                                    fee = salesCar.CarGrade.Under26 ?? 0;
                                }
                                else if (firstRegist.AddMonths(28).CompareTo(today) > 0)
                                {
                                    //28��������
                                    fee = salesCar.CarGrade.Under28 ?? 0;
                                }
                                else if (firstRegist.AddMonths(30).CompareTo(today) > 0)
                                {
                                    //30��������
                                    fee = salesCar.CarGrade.Under30 ?? 0;
                                }
                                else if (firstRegist.AddMonths(36).CompareTo(today) > 0)
                                {
                                    //36��������
                                    fee = salesCar.CarGrade.Under36 ?? 0;
                                }
                                else if (firstRegist.AddMonths(72).CompareTo(today) > 0)
                                {
                                    //72��������
                                    fee = salesCar.CarGrade.Under72 ?? 0;
                                }
                                else if (firstRegist.AddMonths(84).CompareTo(today) > 0)
                                {
                                    //84��������
                                    fee = salesCar.CarGrade.Under84 ?? 0;
                                }
                                else
                                {
                                    //84�����ȏ�
                                    fee = salesCar.CarGrade.Over84 ?? 0;
                                }
                            }
                            catch (NullReferenceException)
                            {
                            }

                        }
                    }

                    retCar.Add("TradeInMaintenanceFee", fee.ToString());

                    //�r�C�ʂ��玩���Ԑł��擾����
                    //CarTax carTax = new CarTaxDao(db).GetByDisplacement(salesCar.Displacement ?? 0);
                    //retCar.Add("CarTax", carTax!=null ? carTax.Amount.ToString() : "0");

                    //�����\��
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                    retCar.Add("EPDiscountTaxId", retValue.Item1);
                    retCar.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2.ToString()));
                    //retCar.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear)));
                    
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    //�����ӕی�������яd�ʐ�
                    if (CommonUtils.DefaultString(salesCar.NewUsedType).Equals("N"))
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance != null ? insurance.Amount : 0m));
                        CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarGrade.CarWeight ?? 0);
                        retCar.Add("CarWeightTax", string.Format("{0:0}", weightTax != null ? weightTax.Amount : 0));
                    }
                    else
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance != null ? insurance.Amount : 0m));
                    }

                }
                return Json(retCar);
            }
            return new EmptyResult();
        }
    #endregion

        #region Validation
        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carAppraisal">�ԗ�����f�[�^</param>
        /// <returns>�ԗ�����f�[�^</returns>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// 2018/04/26 arc yano #3816 �ԗ�������́@�Ǘ��ԍ���N/A�������Ă��܂�
        /// </history>
        private CarAppraisal ValidateCarAppraisal(CarAppraisal carAppraisal) {

            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(carAppraisal.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            if (string.IsNullOrEmpty(carAppraisal.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            if (string.IsNullOrEmpty(carAppraisal.Vin)) {
                ModelState.AddModelError("Vin", MessageUtils.GetMessage("E0001", "�ԑ�ԍ�"));
            }
            //ADD 2014/02/20 ookubo
            if (carAppraisal.PurchasePlanDate == null){
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0001", "�d���\���"));
            }
            // �����`�F�b�N
            if (!ModelState.IsValidField("IssueDate")) {
                ModelState.AddModelError("IssueDate", MessageUtils.GetMessage("E0005", "���s��"));
            }
            if (!ModelState.IsValidField("RegistrationDate")) {
                ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "�o�^��"));
            }
            if (!ModelState.IsValidField("Capacity")) {
                ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("MaximumLoadingWeight")) {
                ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "�ő�ύڗ�", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("CarWeight")) {
                ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��d��", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("TotalCarWeight")) {
                ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ����d��", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Length")) {
                ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Width")) {
                ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "��", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Height")) {
                ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("FFAxileWeight")) {
                ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�O���d", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("FRAxileWeight")) {
                ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�㎲�d", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("RFAxileWeight")) {
                ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��O���d", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("RRAxileWeight")) {
                ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��㎲�d", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Displacement")) {
                ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "�̐���10���ȓ�������2���ȓ�" }));
            }
            if (!ModelState.IsValidField("InspectionExpireDate")) {
                ModelState.AddModelError("InspectionExpireDate", MessageUtils.GetMessage("E0005", "�L������"));
            }
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
            }
            //Mod 2021/08/02 yano #4097
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 ���f���N�̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
            if (!ModelState.IsValidField("ModelYear"))
            {
                ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "���f���N", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
            }

            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "����10���ȓ��̐����̂�" }));
            }
            if (!ModelState.IsValidField("RemainDebt")) {
                ModelState.AddModelError("RemainDebt", MessageUtils.GetMessage("E0004", new string[] { "�c��", "����10���ȓ��̐����̂�" }));
            }
            if (!ModelState.IsValidField("CarTaxUnexpiredAmount")) {
                ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŎ�ʊ��̎c", "����10���ȓ��̐����̂�" }));     //Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("AppraisalPrice")) {
                ModelState.AddModelError("AppraisalPrice", MessageUtils.GetMessage("E0004", new string[] { "���艿�i", "����10���ȓ��̐����̂�" }));
            }
            if (!ModelState.IsValidField("PurchasePlanDate")) {
                ModelState.AddModelError("PurchasePlanDate", MessageUtils.GetMessage("E0005", "�d���\���"));
            }
            if (!ModelState.IsValidField("AppraisalDate")) {
                ModelState.AddModelError("AppraisalDate", MessageUtils.GetMessage("E0005", "�����"));
            }
            if (!ModelState.IsValidField("PurchaseAgreementDate")) {
                ModelState.AddModelError("PurchaseAgreementDate", MessageUtils.GetMessage("E0005", "����_���"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("Mileage") && carAppraisal.Mileage != null) {
                if ((Regex.IsMatch(carAppraisal.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carAppraisal.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }
            //Mod 2021/08/02 yano #4097 ���͉\�����t�H�[�}�b�g��ύX(���̐����S���̂݁����̐���4���A�܂��͐��̐���4��������2���ȓ�
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 ���f���N�̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
            if (ModelState.IsValidField("ModelYear") && CommonUtils.DefaultString(carAppraisal.ModelYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(carAppraisal.ModelYear, @"^\d{4}\.\d{1,2}$"))
                  && (!Regex.IsMatch(carAppraisal.ModelYear, @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "���f���N", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
                }
            }
            //Add 2015/05/18 arc ookubo ���N�x�o�^�N���Ή� #3204 ���̓`�F�b�N��ǉ�
            if (ModelState.IsValidField("FirstRegistrationYear") && carAppraisal.FirstRegistrationYear != null)
            {
                if (Regex.IsMatch(carAppraisal.FirstRegistrationYear.ToString(), @"^(\d{4})/?0?([1-9]|1[012])$")) {
                }
                else
                {
                    ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", new string[] { "���N�x�o�^�N��" }));
                }
            }

            if (ModelState.IsValidField("RecycleDeposit") && carAppraisal.RecycleDeposit != null) {
                if (!Regex.IsMatch(carAppraisal.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RemainDebt") && carAppraisal.RemainDebt != null) {
                if (!Regex.IsMatch(carAppraisal.RemainDebt.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RemainDebt", MessageUtils.GetMessage("E0004", new string[] { "�c��", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("CarTaxUnexpiredAmount") && carAppraisal.CarTaxUnexpiredAmount != null) {
                if (!Regex.IsMatch(carAppraisal.CarTaxUnexpiredAmount.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŎ�ʊ��̎c", "����10���ȓ��̐����̂�" }));  //Mod 2019/09/04 yano #4011
                    //ModelState.AddModelError("CarTaxUnexpiredAmount", MessageUtils.GetMessage("E0004", new string[] { "�����Ԑł̎c", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("AppraisalPrice") && carAppraisal.AppraisalPrice != null) {
                if (!Regex.IsMatch(carAppraisal.AppraisalPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("AppraisalPrice", MessageUtils.GetMessage("E0004", new string[] { "���艿�i", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && carAppraisal.Displacement != null) {
                if ((Regex.IsMatch(carAppraisal.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carAppraisal.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }

            //Add 2018/04/26 arc yano #3816 �O���[�h�R�[�h�����͂���Ă���ꍇ�́A�}�X�^�`�F�b�N���s��    
            if (!string.IsNullOrWhiteSpace(carAppraisal.CarGradeCode))
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(carAppraisal.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("CarGradeCode", "���͂����O���[�h�R�[�h�͎ԗ��O���[�h�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ�����");
                }
            }

            return carAppraisal;
        }
        #endregion
    }
}
