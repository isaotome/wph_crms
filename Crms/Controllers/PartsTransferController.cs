
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Text.RegularExpressions;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.Linq;                 //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.SqlClient;            //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���i�ړ�
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsTransferController : Controller
    {
        //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���i�ړ�����";     // ��ʖ�
        private static readonly string PROC_NAME = "���Ɋm��"; // ������
        private static readonly string PROC_NAME_DISP = "���i�ړ����͉�ʕ\��"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsTransferController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ������ʏ����\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            FormCollection form = new FormCollection();
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            return Criteria(form);
        }

        /// <summary>
        /// ������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>���i�ړ��������ʉ��</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;
            PaginatedList<Transfer> list = GetSearchResultList(form);
            SetCriteriaDataComponent(form);
            return View("PartsTransferCriteria", list);
        }

        /// <summary>
        /// ���i�ړ����͉�ʏ����\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Entry(string id) {
            Transfer trans = new Transfer();
            trans.DepartureDate = DateTime.Today;
            trans.DepartureEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            trans.ArrivalPlanDate = DateTime.Today;
            SetDataComponent(trans);
            return View("PartsTransferEntry", trans);
        }



        /// <summary>
        /// ���i�ړ����͉�ʕ\��
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Transfer trans,FormCollection form)
        {
            //2017/02/03 arc nakayama #3636_���i�ړ����́@�o�Ƀ��P�[�V�����̍݌ɐ���validation�`�F�b�N�̒ǉ��@�����ǉ�
            ValidatePartsTransfer(trans, form);
            if (!ModelState.IsValid) {
                SetDataComponent(trans);
                return View("PartsTransferEntry", trans);
            }

            // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //�ړ��f�[�^���쐬���A�݌ɂ��X�V����
                new TransferDao(db).InsertTransfer(trans, ((Employee)Session["Employee"]).EmployeeCode);
                //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME_DISP, FORM_NAME, "");

                        SetDataComponent(trans);
                        return View("PartsTransferEntry", trans);
                    }
                    else
                    {
                        //Mod 2014/08/08 arc amii �G���[���O�Ή� �wtheow e�x���烍�O�o�͏����ɕύX
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_DISP, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    //Add 2014/08/08 arc amii �G���[���O�Ή� ��L�O��Exception�̏ꍇ�A���O�o�͂��s��
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_DISP, FORM_NAME, "");
                    return View("Error");
                }
                
            }
            SetDataComponent(trans);
            ViewData["close"] = "1";
            return View("PartsTransferEntry",trans);
        }

        /// <summary>
        /// ���Ɋm���ʂ�\������
        /// </summary>
        /// <returns></returns>
        public ActionResult Confirm(string id)
        {
            Transfer trans = new TransferDao(db).GetByKey(id);
            trans.ArrivalDate = DateTime.Today;
            trans.ArrivalEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            SetDataComponent(trans);
            return View("PartsTransferConfirm",trans);
        }

        /// <summary>
        /// ���Ɋm�菈��
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 �T�[�r�X�`�[���́@�`�[�ۑ��A�폜�A�ԓ`���̕��i�̍݌ɂ̖߂��Ή�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(Transfer trans,FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //Validation�`�F�b�N
            if(string.IsNullOrEmpty(form["ArrivalLocationCode"])){
                ModelState.AddModelError("ArrivalLocationCode",MessageUtils.GetMessage("E0001","���Ƀ��P�[�V����"));
            }
            if(string.IsNullOrEmpty(form["ArrivalEmployeeCode"])){
                ModelState.AddModelError("ArrivalEmployeeCode",MessageUtils.GetMessage("E0001","���ɒS����"));
            }
            if (string.IsNullOrEmpty(form["ArrivalDate"])) {
                ModelState.AddModelError("ArrivalDate", MessageUtils.GetMessage("E0001", "���ɓ�"));
            }
            if (!ModelState.IsValidField("ArrivalDate")) {
                ModelState.AddModelError("ArrivalDate", MessageUtils.GetMessage("E0003", "���ɓ�"));
                if (ModelState["ArrivalDate"].Errors.Count > 1) {
                    ModelState["ArrivalDate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("ArrivalDate") && trans.ArrivalDate != null) {

                //Mod 2016/08/13 arc yano #3596
                //���P�[�V�������q�ɂ����o��
                //string departmentCode = new LocationDao(db).GetByKey(trans.ArrivalLocationCode).DepartmentCode;       
                string warehouseCode = new LocationDao(db).GetByKey(trans.ArrivalLocationCode).WarehouseCode;
                
                //�Ώۂ̑q�ɂ��g�p���Ă��镔��̈ꗗ���擾����
                DepartmentWarehouse condition = new DepartmentWarehouse();
                condition.WarehouseCode = warehouseCode;

                List<DepartmentWarehouse> dWarehouseList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                // Mod 2015/04/20 arc yano �������ҏW�����̗L��^�����Ŕ����ύX����B
                //Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)

                int ret = 0;

                //ret = CheckTempClosedEdit(departmentCode, trans.ArrivalDate);
                ret = CheckTempClosedEdit(dWarehouseList, trans.ArrivalDate);   //Mod 2016/08/13 arc yano #3596
                    
                switch (ret)
                {
                    case 1:
                        ModelState.AddModelError("ArrivalDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                        break;

                    case 2:
                        ModelState.AddModelError("ArrivalDate", "���i�I���������I�����Ă���̂Ŏw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                        break;
                    
                    default:
                        //�������Ȃ�
                        break;
                }

                /*
                //--�o��������
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, trans.ArrivalDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("ArrivalDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���ד��ł͓��ׂł��܂���");
                }
                else //--���i�I������
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(departmentCode, trans.ArrivalDate, "002"))
                {
                            ModelState.AddModelError("ArrivalDate", "���i�I���������I�����Ă���̂Ŏw�肳�ꂽ���ɓ��ł͓��ɂł��܂���");
                        }
                }
            }
                */
            }
            if (!ModelState.IsValid) {
                ViewData["TransferTypeName"] = form["TransferTypeName"];
                SetDataComponent(trans);
                return View("PartsTransferConfirm", trans);
            }

            Transfer target = new TransferDao(db).GetByKey(form["TransferNumber"]);
            if (target != null) {

                //�ړ��`�[�̓��ɏ����X�V
                target.ArrivalLocationCode = form["ArrivalLocationCode"];
                target.ArrivalEmployeeCode = form["ArrivalEmployeeCode"];
                target.ArrivalDate = DateTime.Parse(form["ArrivalDate"]);
                target.LastUpdateDate = DateTime.Now;
                target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //Mod 2017/02/08 arc yano #3620
                //���Ƀ��P�[�V�����̍݌ɐ��ʍX�V(�폜�σf�[�^���擾����)
                PartsStock stock = new PartsStockDao(db).GetByKey(form["PartsNumber"], form["ArrivalLocationCode"], true);
                if (stock == null) {
                    stock = new PartsStock();
                    stock.CreateDate = DateTime.Now;
                    stock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    stock.DelFlag = "0";
                    stock.LastUpdateDate = DateTime.Now;
                    stock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    stock.LocationCode = target.ArrivalLocationCode;
                    stock.PartsNumber = target.PartsNumber;
                    stock.Quantity = target.Quantity;
                    db.PartsStock.InsertOnSubmit(stock);

                } else {

                    //�폜�f�[�^�̏ꍇ�͏�����
                    stock = new PartsStockDao(db).InitPartsStock(stock);      //Add 2017/02/08 arc yano #3620

                    stock.Quantity = stock.Quantity + target.Quantity;
                }

                //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        break;
                    }
                    catch (ChangeConflictException e)
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
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException e)
                    {
                        //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͏����ǉ�
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            SetDataComponent(trans);
                            return View("PartsTransferConfirm", trans);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii �G���[���O�Ή� �wtheow e�x���烍�O�o�͏����ɕύX
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }
            SetDataComponent(trans);
            ViewData["close"] = "1";
            return View("PartsTransferConfirm", trans);
        }
        /// <summary>
        /// ������ʂ̃f�[�^�t���R���|�[�l���g���쐬����
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <history>
        /// 2016/06/20 arc yano #3584 ��������(�󒍓`�[�ԍ�)�ǉ�
        /// </history>
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
            ViewData["TransferConfirm"] = !string.IsNullOrEmpty(form["TransferConfirm"]) && form["TransferConfirm"].Contains("true") ? true : false;
            ViewData["TransferUnConfirm"] = !string.IsNullOrEmpty(form["TransferUnConfirm"]) && form["TransferUnConfirm"].Contains("true") ? true : false;

            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                try { ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName; } catch (NullReferenceException) { }
            }

            ViewData["SlipNumber"] = form["SlipNumber"];    //Add 2016/06/20 arc yano #3584

        }
        /// <summary>
        /// �f�[�^�t���R���|�[�l���g�̍쐬
        /// </summary>
        /// <param name="trans"></param>
        /// <history>
        /// 2016/06/20 arc yano #3583 ���i�ړ����́@�ړ���ʂ̍i����
        /// </history>
        private void SetDataComponent(Transfer trans) {
            CodeDao dao = new CodeDao(db);
            ViewData["TransferTypeList"] = CodeUtils.GetSelectListByModel<c_TransferType>(dao.GetTransferTypeAll(false, "1"), trans.TransferType, true);    //Mod 2016/06/20 arc yano #3583

            trans.DepartureEmployee = new EmployeeDao(db).GetByKey(trans.DepartureEmployeeCode);
            trans.DepartureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);
            trans.ArrivalEmployee = new EmployeeDao(db).GetByKey(trans.ArrivalEmployeeCode);
            trans.ArrivalLocation = new LocationDao(db).GetByKey(trans.ArrivalLocationCode);

            Parts parts = new PartsDao(db).GetByKey(trans.PartsNumber);
            if (parts != null) {
                ViewData["PartsNameJp"] = parts.PartsNameJp;
            }
            PartsStock departureStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.DepartureLocationCode);
            ViewData["DepartureStockQuantity"] = 0;
            if (departureStock != null) {
                ViewData["DepartureStockQuantity"] = departureStock.Quantity;
            }
            PartsStock arrivalStock = new PartsStockDao(db).GetByKey(trans.PartsNumber, trans.ArrivalLocationCode);
            ViewData["ArrivalStockQuantity"] = 0;
            if (arrivalStock != null) {
                ViewData["ArrivalStockQuantity"] = arrivalStock.Quantity;
            }
            if (trans.c_TransferType != null) {
                ViewData["TransferTypeName"] = trans.c_TransferType.Name;
            }
        }
        /// <summary>
        /// ���i�ړ��f�[�^���������Č��ʃ��X�g��Ԃ�
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���P�[�V�����̍i���ݏ�����ύX(���偨���P�[�V�����ł͂Ȃ��A���偨�q�Ɂ����P�[�V�����ɕύX)
        /// 2016/06/20 arc yano #3584 ���i�ړ��ꗗ�@���������̒ǉ�
        /// </history>
        private PaginatedList<Transfer> GetSearchResultList(FormCollection form) {

            Transfer condition = new Transfer();
            condition.PartsNumber = form["PartsNumber"];
            condition.TransferType = form["TransferType"];
            condition.DepartureLocationCode = form["DepartureLocationCode"];
            condition.ArrivalLocationCode = form["ArrivalLocationCode"];
            DateTime departureDateFrom;
            if (!string.IsNullOrEmpty(form["DepartureDateFrom"])) {
                if (DateTime.TryParse(form["DepartureDateFrom"], out departureDateFrom)) {
                    condition.DepartureDateFrom = departureDateFrom;
                } else {
                    condition.DepartureDateFrom = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime departureDateTo;
            if (!string.IsNullOrEmpty(form["DepartureDateTo"])) {
                if (DateTime.TryParse(form["DepartureDateTo"], out departureDateTo)) {
                    condition.DepartureDateTo = departureDateTo;
                } else {
                    condition.DepartureDateTo = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime arrivalDateFrom;
            if (!string.IsNullOrEmpty(form["ArrivalDateFrom"])) {
                if (DateTime.TryParse(form["ArrivalDateFrom"], out arrivalDateFrom)) {
                    condition.ArrivalDateFrom = arrivalDateFrom;
                } else {
                    condition.ArrivalDateFrom = DaoConst.SQL_DATETIME_MAX;
                }
            }
            DateTime arrivalDateTo;
            if (!string.IsNullOrEmpty(form["ArrivalDateTo"])) {
                if (DateTime.TryParse(form["ArrivalDateTo"], out arrivalDateTo)) {
                    condition.ArrivalDateTo = arrivalDateTo;
                } else {
                    condition.ArrivalDateTo = DaoConst.SQL_DATETIME_MAX;
                }
            }
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
            condition.CarOrParts = "2";

            //Mod 2016/08/13 arc yano #3596
            //����R�[�h����q�ɂ��擾
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                condition.WarehouseCode = dWarehouse.WarehouseCode;
            }
            //condition.DepartmentCode = form["DepartmentCode"];

            condition.SetAuthCondition((Employee)Session["Employee"]);

            //�`�[�ԍ�
            condition.SlipNumber = form["SlipNumber"];  //Add 2016/06/20 arc yano #3584

            PaginatedList<Transfer> list = new TransferDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return list;
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="trans">���i�ړ��f�[�^</param>
        /// <history>
        /// 2017/02/03 arc nakayama #3636_���i�ړ����́@�o�Ƀ��P�[�V�����̍݌ɐ���validation�`�F�b�N�̒ǉ�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2015/04/20 arc yano ���i�n�̃`�F�b�N�͌o�����A���i�I�����ꂼ��Œ����菈�����s���B�܂��o��������ł́A�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�ύX�\�Ƃ���B
        /// 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
        /// </history>
        private void ValidatePartsTransfer(Transfer trans, FormCollection form)
        {
            if(string.IsNullOrEmpty(trans.DepartureLocationCode)){
                ModelState.AddModelError("DepartureLocationCode",MessageUtils.GetMessage("E0001","�o�Ƀ��P�[�V�����R�[�h"));
            }
            if(string.IsNullOrEmpty(trans.DepartureEmployeeCode)){
                ModelState.AddModelError("DepartureEmployeeCode",MessageUtils.GetMessage("E0001","�o�ɒS����"));
            }
            if (!ModelState.IsValidField("DepartureDate")) {
                ModelState.AddModelError("DepartureDate", MessageUtils.GetMessage("E0003", "�o�ɓ�"));
                if (ModelState["DepartureDate"].Errors.Count > 1) {
                    ModelState["DepartureDate"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("DepartureDate") && trans.DepartureDate != null) {
                Location departureLocation = new LocationDao(db).GetByKey(trans.DepartureLocationCode);

                //Mod 2015/04/20 arc yano
                //Mod 2015/02/03 arc nakayama

                int ret = 0;

                if (departureLocation != null)
                {
                    //Mod 2016/08/13 arc yano #3596
                    //���P�[�V��������q�ɃR�[�h���擾����
                    string warehouseCode = departureLocation.WarehouseCode;
                    //�q�ɃR�[�h����g�p���Ă��镔����擾����
                    DepartmentWarehouse condition = new DepartmentWarehouse();
                    condition.WarehouseCode = warehouseCode;

                    List<DepartmentWarehouse> dWarehouseList = new DepartmentWarehouseDao(db).GetByCondition(condition);

                    if (dWarehouseList != null)
                    {
                        //ret = CheckTempClosedEdit(departureLocation.DepartmentCode, trans.DepartureDate);
                        ret = CheckTempClosedEdit(dWarehouseList, trans.DepartureDate);
                    }
                    
                }

                switch (ret)
                {
                    case 1:
                        ModelState.AddModelError("ArrivalDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ�o�ɓ��ł͏o�ɂł��܂����");
                        break;

                    case 2:
                        ModelState.AddModelError("ArrivalDate", "���i�I���������I�����Ă���̂Ŏw�肳�ꂽ�o�ɓ��ł͏o�ɂł��܂���");
                        break;

                    default:
                        //�������Ȃ�
                        break;
                }

                /*
                //--�o��������
                if (departureLocation != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(departureLocation.DepartmentCode, trans.DepartureDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("DepartureDate", "�o�����������I�����Ă���̂Ŏw�肳�ꂽ�o�ɓ��ł͏o�ɂł��܂���");
                }
                else //--���i�I������
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                        if (departureLocation != null && !new InventorySchedulePartsDao(db).IsClosedInventoryMonth(departureLocation.DepartmentCode, trans.DepartureDate, "002"))
                        {
                    ModelState.AddModelError("DepartureDate", "�I�����ߏ������I�����Ă���̂Ŏw�肳�ꂽ�o�ɓ��ł͏o�ɂł��܂���");
                }
            }
                }
                */
                
            }
            if (string.IsNullOrEmpty(trans.ArrivalLocationCode)) {
                ModelState.AddModelError("ArrivalLocationCode", MessageUtils.GetMessage("E0001", "���Ƀ��P�[�V�����R�[�h"));
            }
            if (!ModelState.IsValidField("ArrivalPlanDate")) {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0003", "���ɗ\���"));
                if (ModelState["ArrivalPlanDate"].Errors.Count > 1) {
                    ModelState["ArrivalPlanDate"].Errors.RemoveAt(0);
                }
            }
            if (string.IsNullOrEmpty(trans.TransferType)) {
                ModelState.AddModelError("TransferType", MessageUtils.GetMessage("E0001", "�ړ����"));
            }
            if (string.IsNullOrEmpty(trans.PartsNumber)) {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", "���i�ԍ�"));
            }
            if (trans.Quantity == 0) {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "0�ȊO�̐��̐���7���ȓ�������2���ȓ�" }));
            }
            //�t�H�[�}�b�g�`�F�b�N
            if (!ModelState.IsValidField("Quantity")) {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" }));
                if (ModelState["Quantity"].Errors.Count > 1) {
                    ModelState["Quantity"].Errors.RemoveAt(0);
                }
            }
            if (ModelState.IsValidField("Quantity") &&
                (Regex.IsMatch(trans.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                        || (Regex.IsMatch(trans.Quantity.ToString(), @"^\d{1,7}$")))) {
            } else {
                ModelState.AddModelError("Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" }));
                if (ModelState["Quantity"].Errors.Count > 1) {
                    ModelState["Quantity"].Errors.RemoveAt(0);
                }
            }

            //2017/02/03 arc nakayama #3636_���i�ړ����́@�o�Ƀ��P�[�V�����̍݌ɐ���validation�`�F�b�N�̒ǉ� �ړ����� ���@�o�Ɍ��̃t���[�̍݌ɐ��̏ꍇ�G���[
            decimal? StockQuantity = decimal.Parse(form["StockQuantity"]);
            if (trans.Quantity > StockQuantity)
            {
                ModelState.AddModelError("Quantity", "�o�ɐ��ʂ��݌ɐ��𒴂��Ă��܂��B�o�ɐ��ʂ͍݌ɐ��ȉ��ɂ��Ă�������");
            }
        }

        /// <summary>
        /// �������ҏW�����L�^���ɂ����߃`�F�b�N����
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="targetDate">�Ώۓ��t</param>
        /// <return>�`�F�b�N����(0:�ҏW�� 1:�ҏW�s��(��������) 2:�ҏW�s��(���i�I����))</return>
        /// <history>
        /// 2021/11/09 yano #4111�y�T�[�r�X�`�[���́z�������̔[�ԍϓ`�[�̔[�ԓ��ҏW�`�F�b�N�����̕s�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ������ύX(departmentCode �� dWarehouseList)
        /// 2015/05/07 arc yano �������ҏW�����ǉ� �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�ύX�\�Ƃ���
        /// </history>
        private int CheckTempClosedEdit(List<DepartmentWarehouse> dWarehouseList, DateTime? targetDate)
        {
            string securityCode = ((Employee)Session["Employee"]).SecurityRoleCode;         //�Z�L�����e�B�R�[�h
            bool editFlag = false;                                                          //�������ҏW����

            int ret = 0;                                                                    //�ҏW��

            //�������ҏW�������擾
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(securityCode, "EditTempClosedData");

            if (rec != null)
            {
                editFlag = rec.EnableFlag;
            }

            //���ƂȂ镔��S�ĂŃ`�F�b�N���s���A��ł��Y��������̂�����΂�����̗p����
            foreach (var dw in dWarehouseList)
            {
                //�ҏW��������̏ꍇ
                if (editFlag == true)
                {
                    //�������̂݃`�F�b�N   ������P�ʂŃ`�F�b�N
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(dw.DepartmentCode, targetDate, "001", securityCode))
                    {
                        ret = 1;
                        break;      //�����𔲂���
                    }
                }
                else
                {
                    //���i�I���`�F�b�N �@���q�ɒP�ʂŃ`�F�b�N
                    if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, targetDate, "002"))
                    {
                        ret = 2;
                        break;  //Add 2021/11/09 yano #4111
                    }
                    else //Mod 2021/11/09 yano #4111
                    {
                        //�������`�F�b�N   ������P�ʂŃ`�F�b�N
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(dw.DepartmentCode, targetDate, "001", securityCode))
                        {
                            ret = 1;
                            break;      //�����𔲂���
                        }
                    }
                }
            }           

            return ret;
        }
    }
}
