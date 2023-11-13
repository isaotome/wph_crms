using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using System.Data.Linq;            //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using Crms.Models;                 //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarTransferController : InheritedController
    {
        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��ړ�����";     // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ��ړ��o�^"; // ������
        private static readonly string PROC_NAME_CONF = "���Ɋm��"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarTransferController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �ԗ��ړ�������ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ��ړ���������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            SetCriteriaDataComponent(form);
            PaginatedList<Transfer> list = GetSearchResult(form);
            return View("CarTransferCriteria", list);
        }
        /// <summary>
        /// ������ʂ̃f�[�^�t���R���|�[�l���g���쐬����
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        private void SetCriteriaDataComponent(FormCollection form) {
            CodeDao dao = new CodeDao(db);
            ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false), form["TransferType"], true);
            ViewData["DepartureLocationCode"] = form["DepartureLocationCode"];
            if (!string.IsNullOrEmpty(form["DepartureLocationCode"])) {
                ViewData["DepartureLocationName"] = new LocationDao(db).GetByKey(form["DepartureLocationCode"]).LocationName;
            }
            ViewData["ArrivalLocationCode"] = form["ArrivalLocationCode"];
            if (!string.IsNullOrEmpty(form["ArrivalLocationCode"])) {
                ViewData["ArrivalLocationName"] = new LocationDao(db).GetByKey(form["ArrivalLocationCode"]).LocationName;
            }
            ViewData["DepartureDateFrom"] = form["DepartureDateFrom"];
            ViewData["DepartureDateTo"] = form["DepartureDateTo"];
            ViewData["ArrivalDateFrom"] = form["ArrivalDateFrom"];
            ViewData["ArrivalDateTo"] = form["ArrivalDateTo"];
            ViewData["Vin"] = form["Vin"];
            ViewData["TransferConfirm"] = !string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true") ? true : false;
            ViewData["TransferUnConfirm"] = !string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true") ? true : false;
        }
        /// <summary>
        /// �w�肵���Ǘ��ԍ��̈ړ�������\������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id) {
            
            List<Transfer> list = new TransferDao(db).GetBySalesCarNumber(id);
            CarPurchase purchase = new CarPurchaseDao(db).GetBySalesCarNumber(id);
            if (purchase != null) {
                Transfer trans = new Transfer();
                trans.TransferNumber = "�d��";
                trans.ArrivalDate = purchase.PurchaseDate;
                trans.ArrivalLocation = purchase.Location;

                //�擪�Ɏd����}��
                list.Insert(0, trans);
            }
            return View("CarTransferCriteriaDialog", list);
        }
        /// <summary>
        /// �ԗ��ړ����͉�ʕ\��
        /// </summary>
        /// <param name="id">�ړ��`�[�ԍ�</param>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            Transfer transfer = new TransferDao(db).GetByKey(id);
            if (transfer == null) {
                transfer = new Transfer();
                transfer.DepartureDate = DateTime.Today;
                transfer.ArrivalPlanDate = DateTime.Today;
                transfer.DepartureEmployee = (Employee)Session["Employee"];
            } else {
                ViewData["update"] = "1";
            }
            SetDataComponent(transfer);
            return View("CarTransferEntry",transfer);
        }

        /// <summary>
        /// �ԗ��ړ��o�^����
        /// </summary>
        /// <param name="transfer">�ԗ��ړ��f�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Transfer transfer,FormCollection form){

            // Mod 2014/11/14 arc yano �ԗ��ړ��o�^�s� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            //�ԗ��݌Ƀ��P�[�V�������擾
            //if (salesCar!=null && string.IsNullOrEmpty(salesCar.LocationCode)) {
            //    ModelState.AddModelError("SalesCarNumber", "�ԗ��݌ɂ��������Ȃ����߈ړ��ł��܂���");
            //}

            ValidateTransfer(transfer, form);

            if (!ModelState.IsValid) {
                SetDataComponent(transfer);
                return View("CarTransferEntry", transfer);
            }

            using (TransactionScope ts = new TransactionScope()) {
                Transfer target;
                if (form["update"] != null && form["update"].Equals("1")) {
                    target = new TransferDao(db).GetByKey(transfer.TransferNumber);
                    UpdateModel<Transfer>(target);
                    EditForUpdate(target);
                } else {
                    target = new Transfer();
                    target.TransferNumber = new SerialNumberDao(db).GetNewTransferNumber();
                    target.TransferType = transfer.TransferType;
                    target.ArrivalPlanDate = transfer.ArrivalPlanDate;
                    target.CarOrParts = "001";
                    target.DepartureDate = transfer.DepartureDate;
                    target.DepartureLocationCode = salesCar.LocationCode;
                    target.DepartureEmployeeCode = transfer.DepartureEmployeeCode;
                    target.ArrivalLocationCode = transfer.ArrivalLocationCode;
                    target.Quantity = 1;
                    target.SalesCarNumber = transfer.SalesCarNumber;
                    
                    EditForInsert(target);
                    
                    //2014/08/07 arc amii �o�O�Ή� �ړ���ʂ������͂̏ꍇ�A�V�X�e���G���[�ŗ�����̂��C��
                    //�p���E�I�����X�̏ꍇ�A�����I�ɓ��Ɋm�肵�A�݌ɂ𗎂Ƃ�
                    if ("004".Equals(transfer.TransferType) || "005".Equals(transfer.TransferType))
                    {
                        target.ArrivalDate = transfer.DepartureDate;
                        salesCar.LocationCode = "";
                        salesCar.CarStatus = "";
                    }
                    
                    db.Transfer.InsertOnSubmit(target);
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        //Add 2014/08/07 arc amii �G���[���O�Ή� ChangeConflictException�������̏����ǉ�
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            //���̃��[�U�ɂ���ĕύX���ꂽ�l�͖�������A���Y�N���C�A���g����̕ύX��L���Ƃ���
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Mod 2014/08/07 arc amii �G���[���O�Ή� Exception�������A���O���o�͂���悤�C��
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetDataComponent(transfer);
                            return View("CarTransferEntry", transfer);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, target.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/07 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, target.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                
                
            }
            SetDataComponent(transfer);
            ViewData["close"] = "1";
            return View("CarTransferEntry", transfer);
        }

        /// <summary>
        /// ���Ɋm����͉�ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirm(string id)
        {
            Transfer transfer = new TransferDao(db).GetByKey(id);
            transfer.ArrivalDate = DateTime.Today;
            transfer.ArrivalEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            SetDataComponent(transfer, true);
            return View("CarTransferConfirm",transfer);
        }

        /// <summary>
        /// ���Ɋm�菈��
        /// </summary>
        /// <param name="transfer">�ԗ��ړ��f�[�^</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(Transfer transfer, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //Add 2017/07/18 arc nakayama #3778_�ԗ��ړ��f�[�^�̕ҏW���폜�@�\�ǉ��@�o���f�[�V�����`�F�b�N���O�����ɂ���
            if (transfer.ActionType.Equals("Complete"))
            {
                ValidateTransfer(transfer, form);
                if (!ModelState.IsValid)
                {
                    SetDataComponent(transfer, true);
                    return View("CarTransferConfirm", transfer);
                }
            }

            using (TransactionScope ts = new TransactionScope()) {

                Transfer target = new TransferDao(db).GetByKey(transfer.TransferNumber);

                if (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Delete"))
                {
                    target.DelFlag = "1";
                    target.LastUpdateDate = DateTime.Now;
                    target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; 
                }
                else
                {
                    
                    //target.ArrivalEmployeeCode = transfer.ArrivalEmployeeCode;
                    //target.ArrivalDate = transfer.ArrivalDate;
                    //target.ArrivalLocationCode = transfer.ArrivalLocationCode;
                    UpdateModel(target);
                    EditForUpdate(target);

                    //�ԗ��}�X�^�̍݌Ƀ��P�[�V��������Ƀ��P�[�V�����ɍX�V
                    SalesCar car = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
                    car.LocationCode = transfer.ArrivalLocationCode;
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        //Add 2014/08/07 arc amii �G���[���O�Ή� ChangeConflictException�������̏����ǉ�
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            //���̃��[�U�ɂ���ĕύX���ꂽ�l�͖�������A���Y�N���C�A���g����̕ύX��L���Ƃ���
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Mod 2014/08/07 arc amii �G���[���O�Ή� Exception�������A���O���o�͂���悤�C��
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogError(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/07 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME_CONF, FORM_NAME, transfer.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }
            SetDataComponent(transfer, true);
            ViewData["close"] = "1";
            return View("CarTransferConfirm", transfer);
        }
        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="transfer">�ԗ��ړ��f�[�^</param>
        /// <history>
        /// 2018/06/26 arc yano #3908 �ԗ��ړ��@���Ɋm�菈�����s���ƃV�X�e���G���[
        /// 2018/04/10 arc yano #3879 �ԗ��`�[�@���P�[�V�����}�X�^�ɕ���R�[�h��ݒ肵�Ă��Ȃ��ƁA�[�ԏ������s���Ȃ�
        /// </history>
        private void ValidateTransfer(Transfer transfer,FormCollection form) {

            //���ʂ̃`�F�b�N
            CommonValidate("DepartureDate", "�o�ɓ�", transfer, true);
            CommonValidate("ArrivalPlanDate", "���ɗ\���", transfer, true);
            //CommonValidate("DepartureLocationCode", "�o�Ƀ��P�[�V����", transfer, true);
            CommonValidate("SalesCarNumber", "�Ǘ��ԍ�", transfer, true);
            CommonValidate("DepartureEmployeeCode", "�o�ɒS����",transfer, true);
            CommonValidate("TransferType", "�ړ����", transfer, true);
            CommonValidate("ArrivalLocationCode", "���Ƀ��P�[�V����", transfer, true);

            //�ԗ��݌Ƀ��P�[�V�������擾
            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            if (salesCar != null && string.IsNullOrEmpty(salesCar.LocationCode))
            {
                ModelState.AddModelError("SalesCarNumber", "�ԗ��݌ɂ��������Ȃ����߈ړ��ł��܂���");
            }

            //Add 2017/02/07 arc nakayama #3045_�ԗ��ړ��Ɋւ��ĕ�����ړ��ł��Ă��܂�
            if ((form["update"] != null) || (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Complete")) && !string.IsNullOrEmpty(transfer.SalesCarNumber))
            {
                List<Transfer> TranDataList = new List<Transfer>();

                if (string.IsNullOrEmpty(transfer.TransferNumber))
                {
                    TranDataList = new TransferDao(db).GetBySalesCarNumber(transfer.SalesCarNumber).Where(x => x.TransferType.Equals("001")).ToList();
                }
                else
                {
                    TranDataList = new TransferDao(db).GetBySalesCarNumber(transfer.SalesCarNumber).Where(x => x.TransferType.Equals("001") && !x.TransferNumber.Equals(transfer.TransferNumber)).ToList();
                }


                bool TransferFlag = false;

                foreach (var TranData in TranDataList)
                {
                    if (TranData.ArrivalDate == null)
                    {
                        TransferFlag = true;
                        break;
                    }
                }

                if (TransferFlag)
                {
                    ModelState.AddModelError("SalesCarNumber", "�Y���̎ԗ��͌��݈ړ����ł��B");
                }
            }


            //SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            if (salesCar != null && !string.IsNullOrEmpty(salesCar.LocationCode))
            {

                //Mod 2018/06/26 arc yano #3908
                Location loc = new LocationDao(db).GetByKey(salesCar.LocationCode);
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, loc != null ? loc.WarehouseCode : "");

                //Mod 2018/04/10 arc yano #3879
                //List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, salesCar.Location.WarehouseCode);

                foreach (var a in dDep)
                {
                    //string departmentCode = new LocationDao(db).GetByKey(salesCar.LocationCode).DepartmentCode;
                    // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("DepartureDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ�o�ɓ��ł͏o�ɂł��܂���");

                        break;
                    }
                }
                
            }

            //���Ɋm�莞�̃`�F�b�N
            if (!string.IsNullOrEmpty(transfer.ActionType) && transfer.ActionType.Equals("Complete"))
            {
                CommonValidate("ArrivalEmployeeCode", "���ɒS����", transfer, true);
                CommonValidate("ArrivalDate", "���ɓ�", transfer, true);

                //���ߓ��`�F�b�N
                if (!string.IsNullOrEmpty(transfer.ArrivalLocationCode) && transfer.ArrivalDate != null)
                {
                    Location LocationData = new LocationDao(db).GetByKey(transfer.ArrivalLocationCode);

                    // Mod 2018/04/10 arc yano #3879
                    // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���

                    List<Department> DepList = CommonUtils.GetDepartmentFromWarehouse(db, LocationData.WarehouseCode);
                    foreach (var a in DepList)
                    {
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                        {
                            ModelState.AddModelError("ArrivalDate", "�������ߏ������I�����Ă��邽�ߎw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");

                            break;
                        }
                    }

                    if (!new InventoryScheduleCarDao(db).IsClosedInventoryMonth(LocationData.WarehouseCode, transfer.ArrivalDate))
                    {

                        ModelState.AddModelError("ArrivalDate", "�ԗ��I�����I�����Ă��邽�ߎw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                    }

                }
            }

        }
        /// <summary>
        /// �ԗ��ړ��������ʂ��擾����
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>�ԗ��ړ��f�[�^���X�g</returns>
        private PaginatedList<Transfer> GetSearchResult(FormCollection form) {
            Transfer condition = new Transfer();
            condition.DepartureDateFrom = CommonUtils.StrToDateTime(form["DepartureDateFrom"],DaoConst.SQL_DATETIME_MIN);
            condition.DepartureDateTo = CommonUtils.StrToDateTime(form["DepartureDateTo"], DaoConst.SQL_DATETIME_MAX);
            condition.DepartureLocationCode = form["DepartureLocationCode"];
            condition.ArrivalDateFrom = CommonUtils.StrToDateTime(form["ArrivalDateFrom"], DaoConst.SQL_DATETIME_MIN);
            condition.ArrivalDateTo = CommonUtils.StrToDateTime(form["ArrivalDateTo"], DaoConst.SQL_DATETIME_MAX);
            condition.ArrivalLocationCode = form["ArrivalLocationCode"];
            condition.CarOrParts = "1";
            condition.SalesCar = new SalesCar();
            condition.SalesCar.Vin = form["Vin"];
            condition.TransferType = form["TransferType"];
            if (!string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true")) {
                condition.TransferConfirm = true;
            } else {
                condition.TransferConfirm = false;
            }
            if (!string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true")) {
                condition.TransferUnConfirm = true;
            } else {
                condition.TransferUnConfirm = false;
            }
            condition.SetAuthCondition((Employee)Session["Employee"]);
            return new TransferDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g
        /// </summary>
        /// <param name="trans">�ԗ��ړ��f�[�^</param>
        /// <history>
        /// 2018/04/10 arc yano #3879 �ԗ��`�[�@���P�[�V�����}�X�^�ɕ���R�[�h��ݒ肵�Ă��Ȃ��ƁA�[�ԏ������s���Ȃ�
        /// 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
        /// </history>
        private void SetDataComponent(Transfer trans, bool ConfirmFlag = false)
        {
            CodeDao dao = new CodeDao(db);
            if (ConfirmFlag)
            {
                ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false).Where(x => x.Code.Equals("001")).ToList<c_TransferType>(), trans.TransferType, false);
            }
            else
            {
                ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false).Where(x => x.Code.Equals("001") || x.Code.Equals("004") || x.Code.Equals("005")).ToList<c_TransferType>(), trans.TransferType, false);
            }
            trans.DepartureEmployee = new EmployeeDao(db).GetByKey(trans.DepartureEmployeeCode);
            trans.DepartureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);
            trans.ArrivalEmployee = new EmployeeDao(db).GetByKey(trans.ArrivalEmployeeCode);
            trans.ArrivalLocation = new LocationDao(db).GetByKey(trans.ArrivalLocationCode);
            if (trans.SalesCar==null && !string.IsNullOrEmpty(trans.SalesCarNumber)) {
                trans.SalesCar = new SalesCarDao(db).GetByKey(trans.SalesCarNumber);
            }
            // Mod 2018/04/10 arc yano #3879
            // Mod 2015/04/15 arc yano
            //�ړ�������̒��߃`�F�b�N
            if(trans.DepartureLocation != null)
            {
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, trans.DepartureLocation.WarehouseCode);

                foreach (var a in dDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["ClosedMonth"] = "1";
                    }
                }

            }
            //�ړ��敔��̒��߃`�F�b�N
            if (trans.ArrivalLocation != null)
            {
                List<Department> aDep = CommonUtils.GetDepartmentFromWarehouse(db, trans.ArrivalLocation.WarehouseCode);

                foreach (var a in aDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ViewData["ClosedMonth"] = "1";
                    }
                }
            }

            /*
            if ((trans.DepartureLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(trans.DepartureLocation.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode)) ||
                (trans.ArrivalLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(trans.ArrivalLocation.DepartmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode)))
            {
                ViewData["ClosedMonth"] = "1";
            }
            */
        }

        /// <summary>
        /// INSERT�p�f�[�^�쐬
        /// </summary>
        /// <param name="transfer"></param>
        private void EditForInsert(Transfer transfer) {
            
            transfer.CreateDate = DateTime.Now;
            transfer.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            EditForUpdate(transfer);
        }

        /// <summary>
        /// UPDATE�p�f�[�^�쐬
        /// </summary>
        /// <param name="transfer"></param>
        /// <history>
        /// 2020/06/19 yano #4054 �y�ԗ��ړ��z�ړ��ϓ`�[���C�����̕s��Ή�
        /// </history>
        private void EditForUpdate(Transfer transfer) {
            SalesCar salesCar = new SalesCarDao(db).GetByKey(transfer.SalesCarNumber);
            
            transfer.CarOrParts = "1";
            transfer.Quantity = 1;
            transfer.LastUpdateDate = DateTime.Now;
            transfer.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //Mod 2020/06/19 yano #4054
            if (transfer.ArrivalDate == null)   //���Ɋ������Ă��Ȃ��ꍇ
            {
                transfer.DepartureLocationCode = salesCar.LocationCode;
            }

            transfer.DelFlag = "0";
        }

        /// <summary>
        /// �o���f�[�V�����`�F�b�N
        /// </summary>
        /// <param name="transfer"></param>
        /// <history>
        /// 2018/04/10 arc yano #3879 �ԗ��`�[�@���P�[�V�����}�X�^�ɕ���R�[�h��ݒ肵�Ă��Ȃ��ƁA�[�ԏ������s���Ȃ�
        /// 2017/07/18 arc nakayama #3778_�ԗ��ړ��f�[�^�̕ҏW���폜�@�\�ǉ�
        /// </history>
        private void ConfirmValidation(Transfer transfer)
        {
            CommonValidate("ArrivalLocationCode", "���Ƀ��P�[�V����", transfer, true);
            CommonValidate("ArrivalEmployeeCode", "���ɒS����", transfer, true);
            CommonValidate("ArrivalDate", "���ɓ�", transfer, true);

            //���ߓ��`�F�b�N
            if (!string.IsNullOrEmpty(transfer.ArrivalLocationCode) && transfer.ArrivalDate != null)
            {
                Location LocationData = new LocationDao(db).GetByKey(transfer.ArrivalLocationCode);
                
                // Mod 2018/04/10 arc yano #3879    
                // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                List<Department> dDep = CommonUtils.GetDepartmentFromWarehouse(db, (LocationData != null ? LocationData.WarehouseCode : ""));

                foreach (var a in dDep)
                {
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(a.DepartmentCode, transfer.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("ArrivalDate", "�������ߏ������I�����Ă��邽�ߎw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                        break;
                    }
                }

                if(!new InventoryScheduleCarDao(db).IsClosedInventoryMonth(LocationData.WarehouseCode, transfer.ArrivalDate)){

                    ModelState.AddModelError("ArrivalDate", "�ԗ��I�����I�����Ă��邽�ߎw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                }

            }

            return;
        }


    }
}
