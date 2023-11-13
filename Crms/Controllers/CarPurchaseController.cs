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
using OfficeOpenXml;                    //Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
using System.Configuration;             //Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
using System.IO;                        //Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
using System.Data;                      //Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�

namespace Crms.Controllers {

    /// <summary>
    /// �ԗ��d���@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarPurchaseController : Controller {

        #region ������
        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��d��";               // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ��d���o�^";           // ������
        private static readonly decimal DECIMAL_MAX = 9999999999;              // ���iMAX
        private static readonly decimal DECIMAL_MIN = -9999999999;             // ���iMIN
        //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
        //private static readonly string LAST_EDIT_PURCHASE = "003";           // �ԗ��d����ʂōX�V�������̒l
        private static readonly string PROC_NAME_EXCEL = "�ԗ��d��Excel�o��";// ������ Add 2017/03/22 arc nakayama #3730_�d�����X�g

        //Add 2019/02/07 yano #3960
        private static readonly string PURCHASETYPE_TRADEIN = "001";         //�d���敪(�����)
        private static readonly string PURCHASETYPE_DIPOSAL = "005";         //�d���敪(�˔p)

        //Add 2021/08/09 yano #4086
        private static readonly string ACCOUNTTYPE_TRADEIN    = "013";      //�������(����ԁj 
        private static readonly string ACCOUNTTYPE_REMAINDEBT = "012";      //�������(�c�j

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarPurchaseController() {
            db = CrmsDataContext.GetDataContext();
        }

        #endregion

        #region �������
        /// <summary>
        /// �ԗ��d��������ʕ\��
        /// </summary>
        /// <returns>�ԗ��d���������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();

            form["RequestFlag"] = "1";

            return Criteria(form);
        }

        /// <summary>
        /// �ԗ��d��������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��d���������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            PaginatedList<CarPurchase> list = new PaginatedList<CarPurchase>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                case "1": // �����{�^��

                    // �������ʃ��X�g�̎擾
                    list = GetSearchResultList(form);
                    SetDataComponent(form);
                    return View("CarPurchaseCriteria", list);

                case "2": // Excel�{�^��
                    SetDataComponent(form);
                    return ExcelDownload(form);

                default:
                    // ������ʂ̕\��
                    SetDataComponent(form);
                    return View("CarPurchaseCriteria", list);
            }
        }

        #region ��ʃR���|�[�l���g�ݒ�
        private void SetDataComponent(FormCollection form)
        {
            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(form["SupplierCode"]);
                if (supplier != null)
                {
                    ViewData["SupplierName"] = supplier.SupplierName;
                }
            }
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("023", false), form["PurchaseStatus"], true);
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];
            ViewData["SlipDateFrom"] = form["SlipDateFrom"];
            ViewData["SlipDateTo"] = form["SlipDateTo"];
            ViewData["PurchaseDateFrom"] = form["PurchaseDateFrom"];
            ViewData["PurchaseDateTo"] = form["PurchaseDateTo"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["Vin"] = form["Vin"];
        }
        #endregion

        #endregion

        #region ���͉��
        /// <summary>
        /// �ԗ��d���e�[�u�����͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ��d���R�[�h(�R�s�[�o�^�܂��͍X�V���̂ݐݒ�)
        ///                  �y�уR�s�[�o�^�t���O(�R�s�[�o�^��"1"�A���L�ȊO�̎�"0")
        ///                  ���J���}��؂�</param>
        /// <returns>�ԗ��d���e�[�u�����͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            string[] idArr = CommonUtils.DefaultString(id).Split(new string[]{","}, StringSplitOptions.None);
            CarPurchase carPurchase;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {

                ViewData["update"] = "0";
                ViewData["copy"] = "0";
                carPurchase = new CarPurchase();
                carPurchase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                carPurchase.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
                carPurchase.LastEditScreen = "000";
                carPurchase.SalesCar = new SalesCar();

            // �R�s�[�o�^�̏ꍇ
            } else if (idArr.Count()>1 && CommonUtils.DefaultString(idArr[1]).Equals("1")) {

                ViewData["update"] = "0";
                ViewData["copy"] = "1";
                carPurchase = CreateCopyCarPurchase(new Guid(idArr[0]));

            // �X�V�̏ꍇ
            } else {
                ViewData["update"] = "1";
                ViewData["copy"] = "0";
                carPurchase = new CarPurchaseDao(db).GetByKey(new Guid(idArr[0]));
                if (string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                    carPurchase.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                }
                if (string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                    carPurchase.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
                // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                // �d���v��ς݁A�����ߏ����ς݂̌��̏ꍇ�A�ύX�s�t���O�𗧂Ă�
                if (carPurchase.PurchaseStatus != null && carPurchase.PurchaseStatus.Equals("002")
                    && !new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ViewData["ClosedMonth"] = "1";
                }
            }
            ViewData["PurchaseStatus"] = carPurchase.PurchaseStatus;

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(ref carPurchase, new FormCollection());

            // �o��
            return View("CarPurchaseEntry", carPurchase);
        }
        #endregion

        #region �ǉ��X�V
        /// <summary>
        /// �ԗ��d���e�[�u���ǉ��X�V
        /// </summary>
        /// <param name="carPurchase">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��d���e�[�u�����͉��</returns>
        /// <history>
        /// 2021/08/09 yano #4086�y�ԗ��d�����́z����Ԃ�ύX�����ۂ̓������у��X�g�폜�R��Ή�
        /// 2018/07/31 yano.hiroki #3919 ����Ԏd����̎ԑ�ԍ��ύX���̑Ή�
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// 2017/11/15 arc yano #3826 �ԗ��d�����́@�V���敪�𖢑I���ł��ۑ����s����
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή� ModelState.Clear()�Ή�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarPurchase carPurchase, FormCollection form,SalesCar salesCar) {

            // ���f���f�[�^�Z�o�ݒ荀�ڂ̐ݒ�
            //carPurchase.calculate();

            // Add 2017/02/14 arc yano #3641
            if (ModelState.IsValid)
            {
                ModelState.Clear();
            }

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];
            ViewData["copy"] = form["copy"];

            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �e��f�[�^�o�^����
            using (TransactionScope ts = new TransactionScope()) {

                CarPurchase targetCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

                //Add 2021/08/09 yano #4086
                string prevSlipNumber = "";   //�`�[�ԍ�(�ҏW�O�j
                string prevVin = "";          //�ԑ�ԍ�(�ҏW�O�j

                //����Ԃ̎d���f�[�^���쐬�ς̏ꍇ�́A���̃f�[�^�̓`�[�ԍ��Ǝԑ�ԍ����T���Ă���
                if(targetCarPurchase != null && !string.IsNullOrWhiteSpace(targetCarPurchase.CarPurchaseType) && targetCarPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN))
                {
                    prevSlipNumber = targetCarPurchase.SlipNumber;   
                    prevVin = targetCarPurchase.Vin;
                }


                // ����������(�d���X�e�[�^�X)�̎擾
                ViewData["PurchaseStatus"] = "";
                if (ViewData["update"].Equals("1"))
                {
                    CarPurchase dbCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                    if (dbCarPurchase != null)
                    {
                        ViewData["PurchaseStatus"] = dbCarPurchase.PurchaseStatus;
                    }
                }


                //�d���폜
                if (form["action"].Equals("DeleteStock"))
                {
                    //�폜����
                    //�f�[�^�`�F�b�N
                    Deletevalidation(carPurchase);
                    if (!ModelState.IsValid)
                    {
                        GetEntryViewData(ref carPurchase, form);
                        ViewData["PurchaseStatus"] = targetCarPurchase.PurchaseStatus;
                        return View("CarPurchaseEntry", carPurchase);
                    }

                    //-------------------------------------
                    //�폜����
                    //-------------------------------------
                    //�d���f�[�^�̍폜
                    //�Ώۂ�DelFlag �� 1 �ɂ���
                    targetCarPurchase.DelFlag = "1";                                                            //�폜�t���O
                    targetCarPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�ŏI�X�V��
                    targetCarPurchase.LastUpdateDate = DateTime.Now;                                            //�ŏI�X�V��

                    //����f�[�^�̍폜
                    if (targetCarPurchase.CarAppraisalId != null)
                    {
                        CarAppraisal targetCarAppraisal = new CarAppraisalDao(db).GetByKey((targetCarPurchase.CarAppraisalId ?? new Guid("00000000-0000-0000-0000-000000000000")));

                        //����f�[�^���擾�ł����ꍇ�͍���f�[�^�̍폜
                        if (targetCarAppraisal != null)
                        {
                            targetCarAppraisal.DelFlag = "1";
                            targetCarAppraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�ŏI�X�V��
                            targetCarAppraisal.LastUpdateDate = DateTime.Now;
                        }
                    }
                    //�ԗ��}�X�^�̍폜
                    if (!string.IsNullOrWhiteSpace(targetCarPurchase.SalesCarNumber))
                    {
                        SalesCar targetSalesCar = new SalesCarDao(db).GetByKey(targetCarPurchase.SalesCarNumber);

                        if (targetSalesCar != null)
                        {
                            targetSalesCar.DelFlag = "1";
                            targetSalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�ŏI�X�V��
                            targetSalesCar.LastUpdateDate = DateTime.Now;
                        }
                    }
                }
                else
                {
                    CarPurchase checkPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                    if (checkPurchase != null)
                    {
                        if (!string.IsNullOrEmpty(checkPurchase.PurchaseStatus) && checkPurchase.PurchaseStatus.Equals("002")
                        && !new InventoryScheduleDao(db).IsClosedInventoryMonth(checkPurchase.DepartmentCode, checkPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode) && form["action"].Equals("CancelStock"))
                        {
                            //���܂��Ă��āA���A�d����L�����Z���̎��̓`�F�b�N���Ȃ�
                        }
                        else
                        {
                            // �f�[�^�`�F�b�N                    
                            ValidateCarPurchase(carPurchase, form);
                            if (!ModelState.IsValid)
                            {
                                GetEntryViewData(ref carPurchase, form);
                                return View("CarPurchaseEntry", carPurchase);
                            }
                        }
                    }
                    else //�V�K�쐬�̏ꍇ�@//Add 2017/11/15 arc yano #3826
                    {
                        // �f�[�^�`�F�b�N                    
                        ValidateCarPurchase(carPurchase, form);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
                        }
                    }

                    //�d����L�����Z��
                    if (form["action"].Equals("CancelStock"))
                    {
                        Cancelvalidation(carPurchase);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            ViewData["PurchaseStatus"] = targetCarPurchase.PurchaseStatus;
                            return View("CarPurchaseEntry", carPurchase);
                        }

                        //�L�����Z���f�[�^�쐬
                        CarPurchase CancelPurchaseData = CreateCancelData(carPurchase);

                        //�L�����Z���f�[�^�ƌ��d����f�[�^�̊֘A��
                        targetCarPurchase.CancelCarPurchaseId = CancelPurchaseData.CarPurchaseId.ToString();
                    }
                }

                //�d���폜�����ȊO
                if (!form["action"].Equals("DeleteStock"))
                {
                    // �a��𐼗�ɕϊ�
                    if (!salesCar.IssueDateWareki.IsNull)
                    {
                        salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki);
                    }
                    if (!salesCar.RegistrationDateWareki.IsNull)
                    {
                        salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki);
                    }
                    salesCar.FirstRegistrationDateWareki.Day = 1;
                    if (!salesCar.FirstRegistrationDateWareki.IsNull)
                    {
                        DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                        //DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki);

                        if (firstRegistrationDate.HasValue)
                        {
                            salesCar.FirstRegistrationYear = firstRegistrationDate.Value.Year + "/" + firstRegistrationDate.Value.Month;
                        }
                    }
                    if (!salesCar.ExpireDateWareki.IsNull)
                    {
                        salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki,db); //Mod 2018/06/22 arc yano #3891
                        //salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki);
                    }
                    if (!salesCar.SalesDateWareki.IsNull)
                    {
                        salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki, db); //Mod 2018/06/22 arc yano #3891
                        //salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki);
                    }
                    if (!salesCar.InspectionDateWareki.IsNull)
                    {
                        salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                        //salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki);
                    }
                    if (!salesCar.NextInspectionDateWareki.IsNull)
                    {
                        salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                        //salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki);
                    }
                    // �ԗ��d���f�[�^�o�^����
                    if (form["update"].Equals("1"))
                    {
                        //CarPurchase targetCarPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
                        UpdateModel(targetCarPurchase);
                        //targetCarPurchase.Amount = carPurchase.Amount;
                        if (targetCarPurchase.CarPurchaseOrder != null)
                        {
                            targetCarPurchase.CarPurchaseOrder.Amount = carPurchase.Amount;
                            targetCarPurchase.CarPurchaseOrder.DiscountAmount = carPurchase.DiscountPrice;
                            targetCarPurchase.CarPurchaseOrder.FirmMargin = carPurchase.FirmPrice;
                            targetCarPurchase.CarPurchaseOrder.MetallicPrice = carPurchase.MetallicPrice;
                            targetCarPurchase.CarPurchaseOrder.OptionPrice = carPurchase.OptionPrice;
                            targetCarPurchase.CarPurchaseOrder.VehiclePrice = carPurchase.VehiclePrice;
                        }

                        //Mod 2021/08/09 yano #4086
                        carPurchase = EditCarPurchaseForUpdate(targetCarPurchase, form, prevSlipNumber, prevVin);
                        //carPurchase = EditCarPurchaseForUpdate(targetCarPurchase, form);
                        carPurchase.SalesCar.IssueDate = salesCar.IssueDate;
                        carPurchase.SalesCar.RegistrationDate = salesCar.RegistrationDate;
                        carPurchase.SalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                        carPurchase.SalesCar.ExpireDate = salesCar.ExpireDate;
                        carPurchase.SalesCar.SalesDate = salesCar.SalesDate;
                        carPurchase.SalesCar.InspectionDate = salesCar.InspectionDate;
                        carPurchase.SalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                    }
                    else
                    {
                        ValidateForInsert(carPurchase);
                        if (!ModelState.IsValid)
                        {
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
                        }
                        carPurchase = EditCarPurchaseForInsert(carPurchase, form);
                        carPurchase.SalesCar.IssueDate = salesCar.IssueDate;
                        carPurchase.SalesCar.RegistrationDate = salesCar.RegistrationDate;
                        carPurchase.SalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                        carPurchase.SalesCar.ExpireDate = salesCar.ExpireDate;
                        carPurchase.SalesCar.SalesDate = salesCar.SalesDate;
                        carPurchase.SalesCar.InspectionDate = salesCar.InspectionDate;
                        carPurchase.SalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                        db.CarPurchase.InsertOnSubmit(carPurchase);
                    }

                    carPurchase.Vin = carPurchase.SalesCar.Vin; //Add 2018/07/31 yano.hiroki #3919 

                    carPurchase.LastEditScreen = "000";

                    //�d���L�����Z���̏ꍇ
                    if (!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("CancelStock"))
                    {
                        carPurchase.SalesCar.DelFlag = "1";
                        carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�ŏI�X�V��
                        carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
                    }
                }

                
                
                #region ���R�[�h
                //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
                //����Ԃ̏ꍇ�͓`�[�ƍ���f�[�^���X�V����
                /*if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                {
                    CarSalesHeader CarSlipData = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);

                    //�[�ԑO�E�[�ԍς̎��͍���ɔ��f���Ȃ�
                    //Add 2017/01/13 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
                    if (!CarSlipData.SalesOrderStatus.Equals("004") && !CarSlipData.SalesOrderStatus.Equals("005"))
                    {
                        //����f�[�^�X�V
                        CarAppraisal CarAppraisalData = new CarAppraisalDao(db).GetBySlipNumberVin(carPurchase.SlipNumber, carPurchase.SalesCar.Vin);
                        if (CarAppraisalData != null)
                        {
                            if (CarAppraisalData.AppraisalPrice != carPurchase.TotalAmount || CarAppraisalData.CarTaxUnexpiredAmount != carPurchase.CarTaxAppropriateAmount || CarAppraisalData.RecycleDeposit != carPurchase.RecycleAmount)
                            {
                                CarAppraisalData.LastEditScreen = LAST_EDIT_PURCHASE;
                                carPurchase.LastEditScreen = "000";
                            }
                            else
                            {
                                carPurchase.LastEditScreen = "000";
                            }

                            CarAppraisalData.AppraisalPrice = carPurchase.TotalAmount;
                            CarAppraisalData.CarTaxUnexpiredAmount = carPurchase.CarTaxAppropriateAmount;
                            CarAppraisalData.RecycleDeposit = carPurchase.RecycleAmount;
                            CarAppraisalData.LastUpdateDate = DateTime.Now;
                            CarAppraisalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        }
                    }
                    else
                    {
                        carPurchase.LastEditScreen = "000";
                    }


                    //�ԗ��`�[�X�V
                    CreateTradeReceiptPlan(CarSlipData, carPurchase);
                }
                else
                {
                    carPurchase.LastEditScreen = "000";
                }*/
                #endregion

                // Add 2014/08/06 arc amii �G���[���O�Ή� catch���ChangeConflictException��ǉ�
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // Add 2014/08/06 arc amii �G���[���O�Ή� SqlException�������A�G���[���O�o�͂��鏈����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", (form["action"].Equals("saveStock") ? "�d���v��" : "�ۑ�")));
                            GetEntryViewData(ref carPurchase, form);
                            return View("CarPurchaseEntry", carPurchase);
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
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
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
            return Entry(carPurchase.CarPurchaseId.ToString());
        }
    #endregion

        #region ��ʃf�[�^�̎擾
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carPurchase">���f���f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// 2019/02/09 yano #3973 �ԗ��d�����́@�a��̓��͍��ڂ�������
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// </history>
        private void GetEntryViewData(ref CarPurchase carPurchase, FormCollection form) {

            SalesCar salesCar = carPurchase.SalesCar;

            Department department = null;       //Mod 2018/10/25 yano #3947


            // ���喼�̎擾
            if (!string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                DepartmentDao departmentDao = new DepartmentDao(db);
                department = departmentDao.GetByKey(carPurchase.DepartmentCode);    //Mod 2018/10/25 yano #3947
                if (department != null) {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }

            // �S���Җ��̎擾
            if (!string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                EmployeeDao employeeDao = new EmployeeDao(db);
                Employee employee = employeeDao.GetByKey(carPurchase.EmployeeCode);
                if (employee != null) {
                    ViewData["EmployeeName"] = employee.EmployeeName;
                    carPurchase.Employee = employee;
                }
            }

            // �d���於�̎擾
            if (!string.IsNullOrEmpty(carPurchase.SupplierCode)) {
                SupplierDao supplierDao = new SupplierDao(db);
                Supplier supplier = supplierDao.GetByKey(carPurchase.SupplierCode);
                if (supplier != null) {
                    ViewData["SupplierName"] = supplier.SupplierName;
                }
            }

            // ���Ƀ��P�[�V�������̎擾
            if (!string.IsNullOrEmpty(carPurchase.PurchaseLocationCode)) {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(carPurchase.PurchaseLocationCode);
                if (location != null) {
                    ViewData["PurchaseLocationName"] = location.LocationName;
                }
            }

            // ���ɋ敪���X�g�̎擾
            ViewData["CarPurchaseTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCarPurchaseTypeAll(false), carPurchase.CarPurchaseType, true);

            //// �d���X�e�[�^�X�ɂ�鏈������
            //if (CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002")) {

            //    // �ԗ����̎擾
            //    carPurchase.SalesCar = new SalesCarDao(db).GetByKey(carPurchase.SalesCarNumber);

            //} else {

            // �O���[�h���̎擾
            if (!string.IsNullOrEmpty(salesCar.CarGradeCode)) {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade = carGradeDao.GetByKey(salesCar.CarGradeCode);
                if (carGrade != null) {
                    salesCar.CarGradeName = carGrade.CarGradeName;
                    try { salesCar.CarName = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { salesCar.CarBrandName = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                    //ViewData["CarGradeName"] = carGrade.CarGradeName;
                    //try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    //try { ViewData["CarBrandName"] = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                }
            }

            // �ԗ����̎擾
            if (ViewData["update"].Equals("1")) {
                SalesCarDao salesCarDao = new SalesCarDao(db);
                SalesCar dbSalesCar = salesCarDao.GetByKey(salesCar.SalesCarNumber);
                if (dbSalesCar != null) {
                    //ViewData["SalesCarNumber"] = dbSalesCar.SalesCarNumber;
                    //try { ViewData["CarStatusName"] = dbSalesCar.c_CarStatus.Name; } catch (NullReferenceException) { }
                    //try { ViewData["LocationName"] = dbSalesCar.Location.LocationName; } catch (NullReferenceException) { }
                    ViewData["PossesorName"] = dbSalesCar.PossesorName;
                    ViewData["PossesorAddress"] = dbSalesCar.PossesorAddress;
                    ViewData["UserName"] = dbSalesCar.UserName;
                    ViewData["UserAddress"] = dbSalesCar.UserAddress;
                    ViewData["PrincipalPlace"] = dbSalesCar.PrincipalPlace;
                }
            }

            // �^�C�����̎擾
            PartsDao partsDao = new PartsDao(db);
            Parts parts;
            if (!string.IsNullOrEmpty(salesCar.Tire)) {
                parts = partsDao.GetByKey(salesCar.Tire);
                if (parts != null) {
                    ViewData["TireName"] = parts.PartsNameJp;
                }
            }

            // �I�C�����̎擾
            if (!string.IsNullOrEmpty(salesCar.Oil)) {
                try { ViewData["OilName"] = partsDao.GetByKey(salesCar.Oil).PartsNameJp; } catch (NullReferenceException) { }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), salesCar.NewUsedType, true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), salesCar.ColorType, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), salesCar.MileageUnit, false);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), salesCar.UsageType, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), salesCar.Usage, true);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), salesCar.CarClassification, true);
            ViewData["MakerWarrantyList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.MakerWarranty, true);
            ViewData["RecordingNoteList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecordingNote, true);
            ViewData["ReparationRecordList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ReparationRecord, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), salesCar.Figure, true);
            ViewData["ImportList"] = CodeUtils.GetSelectListByModel(dao.GetImportAll(false), salesCar.Import, true);
            ViewData["GuaranteeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Guarantee, true);
            ViewData["InstructionsList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.Instructions, true);
            ViewData["RecycleList"] = CodeUtils.GetSelectListByModel(dao.GetRecycleAll(false), salesCar.Recycle, true);
            ViewData["RecycleTicketList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.RecycleTicket, true);
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), salesCar.Steering, true);
            ViewData["ChangeColorList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.ChangeColor, true);
            ViewData["LightList"] = CodeUtils.GetSelectListByModel(dao.GetLightAll(false), salesCar.Light, true);
            ViewData["AwList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aw, true);
            ViewData["AeroList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Aero, true);
            ViewData["SrList"] = CodeUtils.GetSelectListByModel(dao.GetSrAll(false), salesCar.Sr, true);
            ViewData["CdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Cd, true);
            ViewData["MdList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.Md, true);
            ViewData["NaviTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), salesCar.NaviType, true);
            ViewData["NaviEquipmentList"] = CodeUtils.GetSelectListByModel(dao.GetNaviEquipmentAll(false), salesCar.NaviEquipment, true);
            ViewData["NaviDashboardList"] = CodeUtils.GetSelectListByModel(dao.GetNaviDashboardAll(false), salesCar.NaviDashboard, true);
            ViewData["SeatTypeList"] = CodeUtils.GetSelectListByModel(dao.GetSeatTypeAll(false), salesCar.SeatType, true);
            ViewData["DeclarationTypeList"] = CodeUtils.GetSelectListByModel(dao.GetDeclarationTypeAll(false), salesCar.DeclarationType, true);
            ViewData["AcquisitionReasonList"] = CodeUtils.GetSelectListByModel(dao.GetAcquisitionReasonAll(false), salesCar.AcquisitionReason, true);
            ViewData["TaxationTypeCarTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeCarTax, true);
            ViewData["TaxationTypeAcquisitionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetTaxationTypeAll(false), salesCar.TaxationTypeAcquisitionTax, true);
            ViewData["ExpireTypeList"] = CodeUtils.GetSelectListByModel(dao.GetExpireTypeAll(false), salesCar.ExpireType, false);
            ViewData["CouponPresenceList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.CouponPresence, true);
            ViewData["DocumentCompleteList"] = CodeUtils.GetSelectListByModel(dao.GetDocumentCompleteAll(false), salesCar.DocumentComplete, true);
            ViewData["EraseRegistList"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), salesCar.EraseRegist, true);
            ViewData["FinanceList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), salesCar.Finance, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), salesCar.Fuel, true);
            //Add 2014/08/15 arc amii DM�t���O�@�\�g���Ή� #3069 �R���{�{�b�N�X�ɐݒ肷��l���擾���鏈����ǉ�
            //Mod 2014/09/08 arc amii DM�t���O�@�\�g���Ή� #3069 �R���{�{�b�N�X�̋󔒍s�����Ȃ��悤�C��
            ViewData["InspectGuidFlagList"] = CodeUtils.GetSelectListByModel(dao.GetNeededAll(false), salesCar.InspectGuidFlag, false);
            ViewData["MakerWarrantyList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), salesCar.MakerWarranty, true);
            //Add 2014/08/15 arc amii �݌ɃX�e�[�^�X�ύX�Ή��Ή� #3071 ���͉�ʕ\�����A�݌ɃX�e�[�^�X�����擾���鏈����ǉ�
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);

            //Add 2014/10/16 arc yano �ԗ��X�e�[�^�X�ǉ��Ή��@���p�p�r�̃��X�g�{�b�N�X�ǉ�
            //Add 2014/10/30 arc amii �ԗ��X�e�[�^�X�ǉ��Ή�
            ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("004", false), salesCar.CarUsage, true);
            //ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCarUsageAll(false), salesCar.CarUsage, true);

            
            //ADD 2014/02/20 ookubo
            carPurchase.RateEnabled = true;
            //�Ǘ��Ҍ����̂ݏ���ŗ��g�p��
            Employee emp = HttpContext.Session["Employee"] as Employee;
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
            {
                carPurchase.RateEnabled = false;
            }
            //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
            if (carPurchase.ConsumptionTaxId == null)
            {
                carPurchase.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                carPurchase.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(carPurchase.ConsumptionTaxId));
            }
            //Add 2014/08/15 arc amii �݌ɃX�e�[�^�X�ύX�Ή� #3071 �Ǘ��Ҍ����̂ݍ݌ɃX�e�[�^�X�g�p�ɂ��鏈����ǉ�
            if ("999".Equals(emp.SecurityRoleCode))
            {
                salesCar.CarStatusEnabled = true;
            }
            else
            {
                salesCar.CarStatusEnabled = false;
            }
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carPurchase.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carPurchase.ConsumptionTaxId;
            ViewData["Rate"] = carPurchase.Rate;
            ViewData["ConsumptionTaxIdOld"] = carPurchase.ConsumptionTaxId;
            ViewData["PurchasePlanDateOld"] = carPurchase.PurchaseDate;
            //ADD ookubo.end
            
            carPurchase.MetallicAmount = carPurchase.MetallicAmount ?? 0m;
            carPurchase.MetallicTax = carPurchase.MetallicTax ?? 0m;

            carPurchase.OptionAmount = carPurchase.OptionAmount ?? 0m;
            carPurchase.OptionTax = carPurchase.OptionTax ?? 0m;

            carPurchase.OthersAmount = carPurchase.OthersAmount ?? 0m;
            carPurchase.OthersTax = carPurchase.OthersTax ?? 0m;

            carPurchase.VehicleAmount = carPurchase.VehicleAmount ?? 0m;
            carPurchase.VehicleTax = carPurchase.VehicleTax ?? 0m;

            carPurchase.AuctionFeeAmount = carPurchase.AuctionFeeAmount ?? 0m;
            carPurchase.AuctionFeePrice = carPurchase.AuctionFeePrice ?? 0m;
            carPurchase.AuctionFeeTax = carPurchase.AuctionFeeTax ?? 0m;

            carPurchase.FirmAmount = carPurchase.FirmAmount ?? 0m;
            carPurchase.FirmTax = carPurchase.FirmTax ?? 0m;

            carPurchase.CarTaxAppropriateAmount = carPurchase.CarTaxAppropriateAmount ?? 0m;
            carPurchase.CarTaxAppropriatePrice = carPurchase.CarTaxAppropriatePrice ?? 0m;
            carPurchase.CarTaxAppropriateTax = carPurchase.CarTaxAppropriateTax ?? 0m;
            
            carPurchase.DiscountAmount = carPurchase.DiscountAmount ?? 0m;
            carPurchase.DiscountTax = carPurchase.DiscountTax ?? 0m;
            
            carPurchase.EquipmentTax = carPurchase.EquipmentTax ?? 0m;
            carPurchase.EquipmentAmount = carPurchase.EquipmentAmount ?? 0m;

            carPurchase.TotalAmount = carPurchase.TotalAmount ?? 0m;
            
            carPurchase.RecycleAmount = carPurchase.RecycleAmount ?? 0m;
            carPurchase.RecyclePrice = carPurchase.RecyclePrice ?? 0m;

            carPurchase.RepairAmount = carPurchase.RepairAmount ?? 0m;
            carPurchase.RepairTax = carPurchase.RepairTax ?? 0m;

            
            //---------------------
            // ���s��
            //---------------------
            //Mod 2019/02/09 yano #3973
            //���s��(datetime�^)��null�łȂ��ꍇ�́A��������ɘa���ݒ�
            if (salesCar.IssueDate != null)
            {
                salesCar.IssueDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.IssueDate);
            }
          
            string issueDateGengou = "";

            if (salesCar.IssueDateWareki != null)
            {
                issueDateGengou = salesCar.IssueDateWareki.Gengou.ToString();
            }

            ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), issueDateGengou, false);     //Mod 2018/06/22 arc yano #3891
            //ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), issueDateGengou, false);

            //---------------------
            // �o�^�N����
            //---------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.RegistrationDate != null)
            {
                salesCar.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.RegistrationDate);
            }

            string registrationDateGengou = "";

            if (salesCar.RegistrationDateWareki != null) 
            {
                registrationDateGengou = salesCar.RegistrationDateWareki.Gengou.ToString();
            }
            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registrationDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registrationDateGengou, false);


            //---------------------
            // ���N�x�o�^
            //---------------------
            DateTime parseResult;
            DateTime? firstRegistrationDate = null;
            if (DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out parseResult)) {
                firstRegistrationDate = DateTime.Parse(salesCar.FirstRegistrationYear + "/01");
            }
            //Mod 2019/02/09 yano #3973
            if (firstRegistrationDate != null)
            {
                salesCar.FirstRegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(firstRegistrationDate);
            }
            string firstRegistrationDateGengou = "";
            if (salesCar.FirstRegistrationDateWareki != null) {
                firstRegistrationDateGengou = salesCar.FirstRegistrationDateWareki.Gengou.ToString();
            }
            ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), firstRegistrationDateGengou, false); //Mod 2018/06/22 arc yano #3891
            //ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), firstRegistrationDateGengou, false);


            //------------------------
            // �L�����Ԃ̖��������
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.ExpireDate != null)
            {
                salesCar.ExpireDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.ExpireDate);
            }
            
            string expireDateGengou = "";

            if (salesCar.ExpireDateWareki != null)
            {
                expireDateGengou = salesCar.ExpireDateWareki.Gengou.ToString();
            }

            ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), expireDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), expireDateGengou, false);

            //------------------------
            // �[�ԓ�
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.SalesDate != null)
            {
                salesCar.SalesDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.SalesDate);
            }
            
            string salesDateGengou = "";
            if (salesCar.SalesDateWareki != null)
            {
                salesDateGengou = salesCar.SalesDateWareki.Gengou.ToString();
            }
            ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), salesDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), salesDateGengou, false);

            //------------------------
            // �_����
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.InspectionDate != null)
            {
                salesCar.InspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.InspectionDate);
            }

            string inspectionDateGengou = "";

            if (salesCar.InspectionDateWareki != null)
            {
                inspectionDateGengou = salesCar.InspectionDateWareki.Gengou.ToString();
            }
            ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), inspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), inspectionDateGengou, false);

            //------------------------
            // ����_����
            //------------------------
            //Mod 2019/02/09 yano #3973
            if (salesCar.NextInspectionDate != null)
            {
                salesCar.NextInspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.NextInspectionDate);
            }
            
            string nextInspectionDateGengou = "";

            if (salesCar.NextInspectionDateWareki != null)
            {
                nextInspectionDateGengou = salesCar.NextInspectionDateWareki.Gengou.ToString();
            }

            ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), nextInspectionDateGengou, false); //Mod 2018/06/22 arc yano #3891

            //Mod 2021/08/02 yano #4097 �R�����g�A�E�g
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̌�����4���𒴂��Ă����ꍇ�A4���ŕ\������
            //if (CommonUtils.DefaultString(salesCar.ManufacturingYear).Length > 10)
            //{
            //    salesCar.ManufacturingYear = salesCar.ManufacturingYear.Substring(0, 10);
            //}

            //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
            //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            //�Ō�ɋ��z�̕ϓ�����������ʂ��ԗ��d����ʂłȂ���΃��b�Z�[�W�\��
            /*if (!carPurchase.LastEditScreen.Equals(LAST_EDIT_PURCHASE))
            {
                switch (carPurchase.LastEditScreen)
                {
                    case "001":
                        carPurchase.LastEditMessage = "�ԗ��`�[����d�����i(���z)�A���ŏ[���A���T�C�N���̊e���z���ύX����܂����B";
                        break;
                    case "002":
                        carPurchase.LastEditMessage = "�����ʂ���d�����i(���z)�A���ŏ[���A���T�C�N���̊e���z���ύX����܂����B";
                        break;
                    default:
                        carPurchase.LastEditMessage = "";
                        break;
                }

            }else{
                carPurchase.LastEditMessage = "";
            }*/
			//Add 2017/03/06 arc yano #3640 �ԗ��d�����z�̃`�F�b�N
            int ret = 0;

            decimal calcPrice = carPurchase.VehiclePrice + (carPurchase.AuctionFeePrice ?? 0m) + (carPurchase.CarTaxAppropriatePrice ?? 0m) +
                                (carPurchase.RecyclePrice ?? 0m) + carPurchase.OthersPrice + carPurchase.MetallicPrice + carPurchase.OptionPrice +
                                carPurchase.FirmPrice + carPurchase.DiscountPrice + carPurchase.EquipmentPrice + carPurchase.RepairPrice;

            if (carPurchase.Amount != calcPrice)
            {
                ret = 1;
            }

            decimal calcTotalAmount = (carPurchase.VehicleAmount ?? 0m) + (carPurchase.AuctionFeeAmount ?? 0m) + (carPurchase.CarTaxAppropriateAmount ?? 0m) +
                                 (carPurchase.RecycleAmount ?? 0m) + (carPurchase.OthersAmount ?? 0m) + (carPurchase.MetallicAmount ?? 0m) + (carPurchase.OptionAmount ?? 0m) +
                                (carPurchase.FirmAmount ?? 0m) + (carPurchase.DiscountAmount ?? 0m) + (carPurchase.EquipmentAmount ?? 0m) + (carPurchase.RepairAmount ?? 0m);

            if (carPurchase.TotalAmount != calcTotalAmount)
            {
                ret = 1;
            }

            if (ret == 1)
            {
                carPurchase.CalcResultMessage = "(z)�̋��z��(a)�`(k)�̋��z�̍��v�ƈ�v���܂���B��v������ɂ͍Čv�Z���s���K�v������܂��B";
            }
            
            //�L�����Z���f�[�^�擾�i�L��Γ��͕s�ɂ���j
            CarPurchase checkPurchase = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
            
            if (checkPurchase != null)
            {
                if(!string.IsNullOrEmpty(checkPurchase.CancelCarPurchaseId)){
                    ViewData["CancelFlag"] = "1"; //���͕s�ɂ���t���O
                }

                // �d���v��ς݁A�����ߏ����ς݂̌��̏ꍇ�A�ύX�s�t���O�𗧂Ă�
                if (!string.IsNullOrEmpty(checkPurchase.PurchaseStatus) && checkPurchase.PurchaseStatus.Equals("002")
                    && !new InventoryScheduleDao(db).IsClosedInventoryMonth(checkPurchase.DepartmentCode, checkPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ViewData["ClosedMonth"] = "1";
                }
            }

            if (carPurchase != null)
            {
                if (!string.IsNullOrEmpty(carPurchase.PurchaseStatus) && carPurchase.PurchaseStatus.Equals("003"))
                {
                    ViewData["CancelFlag"] = "1"; //���͕s�ɂ���t���O
                }
            }

        }
        #endregion

        #region ��������
        /// <summary>
        /// �ԗ��d���e�[�u���������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��d���e�[�u���������ʃ��X�g</returns>
        private PaginatedList<CarPurchase> GetSearchResultList(FormCollection form) {

            CarPurchaseDao carPurchaseDao = new CarPurchaseDao(db);
            CarPurchase carPurchaseCondition = new CarPurchase();
            carPurchaseCondition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.SlipDateFrom = CommonUtils.StrToDateTime(form["SlipDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.SlipDateTo = CommonUtils.StrToDateTime(form["SlipDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseDateFrom = CommonUtils.StrToDateTime(form["PurchaseDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseDateTo = CommonUtils.StrToDateTime(form["PurchaseDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseStatus = form["PurchaseStatus"];
            carPurchaseCondition.Department = new Department();
            carPurchaseCondition.Department.DepartmentCode = form["DepartmentCode"];
            carPurchaseCondition.Supplier = new Supplier();
            carPurchaseCondition.Supplier.SupplierCode = form["SupplierCode"];
            carPurchaseCondition.SalesCar = new SalesCar();
            carPurchaseCondition.SalesCar.SalesCarNumber = form["SalesCarNumber"];
            carPurchaseCondition.SalesCar.Vin = form["Vin"];
            carPurchaseCondition.SalesCar.CarGrade = new CarGrade();
            carPurchaseCondition.SalesCar.CarGrade.Car = new Car();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand = new Brand();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker = new Maker();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName = form["MakerName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.CarName = form["CarName"];
            carPurchaseCondition.SalesCar.CarGrade.CarGradeName = form["CarGradeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carPurchaseCondition.DelFlag = form["DelFlag"];
            }
            return carPurchaseDao.GetListByCondition(carPurchaseCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region �V�K�o�^����Validation
        private void ValidateForInsert(CarPurchase carPurchase) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(carPurchase.SalesCar.Vin);
            if (list != null && list.Count > 0 && !carPurchase.RegetVin) {
                ModelState.AddModelError("SalesCar.Vin", "�ԑ�ԍ�:" + carPurchase.SalesCar.Vin + "�͊��ɓo�^����Ă��܂�");
                ViewData["ErrorSalesCar"] = list;
            }
            for (int i = 0; i < list.Count(); i++) {
                if (list[i].CarStatus != null && !list[i].CarStatus.Equals("006") && !list[i].CarStatus.Equals("")) {
                    ModelState.AddModelError("SalesCar.Vin", list[i].Vin + " (" + (i + 1) + ")�̍݌ɃX�e�[�^�X���u" + list[i].c_CarStatus.Name + "�v�̂��ߊǗ��ԍ��̍Ď擾���o���܂���");
                }
            }
        }
    #endregion

        #region Validation
        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carPurchase">�ԗ��d���f�[�^</param>
        /// <returns>�ԗ��d���f�[�^</returns>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// 2020/11/17 yano #4065 �y�ԗ��`�[���́z�����\���E�}�X�^�̐ݒ�l���s���̏ꍇ�̑Ή�
        /// 2019/02/08 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���) we�ł̏ꍇ�́A����d���œ`�[�ԍ��̓��̓`�F�b�N���s��Ȃ�
        /// 2018/10/25 yano #3947 �ԗ��d�����́@���͍��ځi�Õ����������̊m�F���@�j�̒ǉ�
        /// 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\) �@�\���P�A ���ɋ敪�̕K�{�`�F�b�N��ǉ�
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// 2018/06/06 arc yano #3883 �^�}�\���P ���ÎԎd���̍ۂɏ��N�x�o�^��K�{���ڂ�
        /// 2018/04/26 arc yano #3816 �ԗ�������́@�Ǘ��ԍ���N/A�������Ă��܂�
        /// </history>
        private CarPurchase ValidateCarPurchase(CarPurchase carPurchase, FormCollection form)
        {

            SalesCar salesCar = carPurchase.SalesCar;

            //Add 2018/08/28 yano #3922
            if (string.IsNullOrEmpty(carPurchase.CarPurchaseType))
            {
                ModelState.AddModelError("CarPurchaseType", MessageUtils.GetMessage("E0001", "���ɋ敪"));
            }
            // �K�{�`�F�b�N(���l�E���t���ڂ͑����`�F�b�N�����˂�)
            if (string.IsNullOrEmpty(carPurchase.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            if (string.IsNullOrEmpty(carPurchase.EmployeeCode)) {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�d���S����"));
            }
            if (string.IsNullOrEmpty(carPurchase.SupplierCode)) {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "�d����"));
            }
            if (carPurchase.PurchaseDate == null)
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0003", "���ɓ�"));
            }
            if (string.IsNullOrEmpty(carPurchase.PurchaseLocationCode)) {
                ModelState.AddModelError("PurchaseLocationCode", MessageUtils.GetMessage("E0001", "���Ƀ��P�[�V����"));
            }
           
            //Add 2018/06/06 arc yano #3883
            //���ÎԂ̎d���v�㎞�A���N�x�o�^�͕K�{�Ƃ���
            if ((!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("saveStock")) && salesCar.NewUsedType.Equals("U"))
            {
                salesCar.FirstRegistrationDateWareki.Day = 1;

                if (salesCar.FirstRegistrationDateWareki.IsNull)
                {
                    ModelState.AddModelError("salesCar.FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0001", "���x�o�^�N��"));
                    ModelState.AddModelError("salesCar.FirstRegistrationDateWareki.Month", "");
                }
            }

            //Mod 2020/11/17 yano #4065 ���ɓ������������邽�߁A�R�����g�A�E�g
            //salesCar.FirstRegistrationDateWareki.Day = 1;
            //if (!salesCar.FirstRegistrationDateWareki.IsNull)
            //{
            //    if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db)) //Mod 2018/06/22 arc yano #3891
            //    //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki))
            //    {
            //        ModelState.AddModelError("FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "���x�o�^�N��"));
            //    }
            //}

            // Add 2014/09/11 arc amii �Ԍ��ē��`�F�b�N�Ή� �Ԍ��ē�=�u�ہv�̏ꍇ�A���l���̕K�{�`�F�b�N���s��
            if (!CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002"))
            {
                if (salesCar.InspectGuidFlag.Equals("002") && string.IsNullOrEmpty(salesCar.InspectGuidMemo))
                {
                    ModelState.AddModelError("SalesCar.InspectGuidMemo", MessageUtils.GetMessage("E0001", "�Ԍ��ē��������l��"));
                }
            }

            // Add 2016/02/05 ARC Mikami #3212 ���l����TextArea�ɕύX�A�������`�F�b�N���s���B
            if (!string.IsNullOrEmpty(carPurchase.Memo)) {
                if ( carPurchase.Memo.Length > 100 ) {
                    ModelState.AddModelError("Memo", "���l��100�����ȓ��œ��͂��ĉ������i���s��2�������Ƃ݂Ȃ���܂��j");
                    if (ModelState["Memo"].Errors.Count > 1) {
                        ModelState["Memo"].Errors.RemoveAt(0);
                    }
                }
            }
            if (!ModelState.IsValidField("VehiclePrice")) {
                ModelState.AddModelError("VehiclePrice", MessageUtils.GetMessage("E0002", new string[] { "�ԗ��{�̉��i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["VehiclePrice"].Errors.Count > 1) {
                    ModelState["VehiclePrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("VehicleTax")) {
                ModelState.AddModelError("VehicleTax", MessageUtils.GetMessage("E0002", new string[] { "�ԗ��{�̉��i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["VehicleTax"].Errors.Count > 1) {
                    ModelState["VehicleTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("VehicleAmount")) {
                ModelState.AddModelError("VehicleAmount", MessageUtils.GetMessage("E0002", new string[] { "�ԗ��{�̉��i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["VehicleAmount"].Errors.Count > 1) {
                    ModelState["VehicleAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("AuctionFeePrice")) {
                ModelState.AddModelError("AuctionFeePrice", MessageUtils.GetMessage("E0004", new string[] { "�I�[�N�V�������D��(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["AuctionFeePrice"].Errors.Count > 1) {
                    ModelState["AuctionFeePrice"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("AuctionFeeTax")) {
                ModelState.AddModelError("AuctionFeeTax", MessageUtils.GetMessage("E0004", new string[] { "�I�[�N�V�������D��(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["AuctionFeeTax"].Errors.Count > 1) {
                    ModelState["AuctionFeeTax"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("AuctionFeeAmount")) {
                ModelState.AddModelError("AuctionFeeAmount", MessageUtils.GetMessage("E0004", new string[] { "�I�[�N�V�������D��(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["AuctionFeeAmount"].Errors.Count > 1) {
                    ModelState["AuctionFeeAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicPrice")) {
                ModelState.AddModelError("MetallicPrice", MessageUtils.GetMessage("E0002", new string[] { "���^���b�N���i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["MetallicPrice"].Errors.Count > 1) {
                    ModelState["MetallicPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicTax")) {
                ModelState.AddModelError("MetallicTax", MessageUtils.GetMessage("E0002", new string[] { "���^���b�N���i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["MetallicTax"].Errors.Count > 1) {
                    ModelState["MetallicTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("MetallicAmount")) {
                ModelState.AddModelError("MetallicAmount", MessageUtils.GetMessage("E0002", new string[] { "���^���b�N���i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["MetallicAmount"].Errors.Count > 1) {
                    ModelState["MetallicAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionPrice")) {
                ModelState.AddModelError("OptionPrice", MessageUtils.GetMessage("E0002", new string[] { "�I�v�V�������i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["OptionPrice"].Errors.Count > 1) {
                    ModelState["OptionPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionTax")) {
                ModelState.AddModelError("OptionTax", MessageUtils.GetMessage("E0002", new string[] { "�I�v�V�������i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["OptionTax"].Errors.Count > 1) {
                    ModelState["OptionTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OptionAmount")) {
                ModelState.AddModelError("OptionAmount", MessageUtils.GetMessage("E0002", new string[] { "�I�v�V�������i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["OptionAmount"].Errors.Count > 1) {
                    ModelState["OptionAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmPrice")) {
                ModelState.AddModelError("FirmPrice", MessageUtils.GetMessage("E0002", new string[] { "�t�@�[�����i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["FirmPrice"].Errors.Count > 1) {
                    ModelState["FirmPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmTax")) {
                ModelState.AddModelError("FirmTax", MessageUtils.GetMessage("E0002", new string[] { "�t�@�[�����i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["FirmTax"].Errors.Count > 1) {
                    ModelState["FirmTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("FirmAmount")) {
                ModelState.AddModelError("FirmAmount", MessageUtils.GetMessage("E0002", new string[] { "�t�@�[�����i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["FirmAmount"].Errors.Count > 1) {
                    ModelState["FirmAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountPrice")) {
                ModelState.AddModelError("DiscountPrice", MessageUtils.GetMessage("E0002", new string[] { "�f�B�X�J�E���g���i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["DiscountPrice"].Errors.Count > 1) {
                    ModelState["DiscountPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountTax")) {
                ModelState.AddModelError("DiscountTax", MessageUtils.GetMessage("E0002", new string[] { "�f�B�X�J�E���g���i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["DiscountTax"].Errors.Count > 1) {
                    ModelState["DiscountTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("DiscountAmount")) {
                ModelState.AddModelError("DiscountAmount", MessageUtils.GetMessage("E0002", new string[] { "�f�B�X�J�E���g���i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["DiscountAmount"].Errors.Count > 1) {
                    ModelState["DiscountAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("EquipmentPrice")) {
                ModelState.AddModelError("EquipmentPrice", MessageUtils.GetMessage("E0002", new string[] { "�������i", "10���ȓ��̐����̂�" }));
                if (ModelState["EquipmentPrice"].Errors.Count > 1) {
                    ModelState["EquipmentPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("RepairPrice")) {
                ModelState.AddModelError("RepairPrice", MessageUtils.GetMessage("E0002", new string[] { "���C���i", "10���ȓ��̐����̂�" }));
                if (ModelState["RepairPrice"].Errors.Count > 1) {
                    ModelState["RepairPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersPrice")) {
                ModelState.AddModelError("OthersPrice", MessageUtils.GetMessage("E0002", new string[] { "���̑����i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["OthersPrice"].Errors.Count > 1) {
                    ModelState["OthersPrice"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersTax")) {
                ModelState.AddModelError("OthersTax", MessageUtils.GetMessage("E0002", new string[] { "���̑����i(�����)", "10���ȓ��̐����̂�" }));
                if (ModelState["OthersTax"].Errors.Count > 1) {
                    ModelState["OthersTax"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("OthersAmount")) {
                ModelState.AddModelError("OthersAmount", MessageUtils.GetMessage("E0002", new string[] { "���̑����i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["OthersAmount"].Errors.Count > 1) {
                    ModelState["OthersAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("Amount")) {
                ModelState.AddModelError("Amount", MessageUtils.GetMessage("E0002", new string[] { "�d�����i(�Ŕ�)", "10���ȓ��̐����̂�" }));
                if (ModelState["Amount"].Errors.Count > 1) {
                    ModelState["Amount"].Errors.RemoveAt(0);
                }
            } 
            if (!ModelState.IsValidField("TaxAmount")) {
                ModelState.AddModelError("TaxAmount", MessageUtils.GetMessage("E0002", new string[] { "�����", "10���ȓ��̐����̂�" }));
                if (ModelState["TaxAmount"].Errors.Count > 1) {
                    ModelState["TaxAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("TotalAmount")) {
                ModelState.AddModelError("TotalAmount", MessageUtils.GetMessage("E0002", new string[] { "�d�����i(�ō�)", "10���ȓ��̐����̂�" }));
                if (ModelState["TotalAmount"].Errors.Count > 1) {
                    ModelState["TotalAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("SlipDate")) {
                ModelState.AddModelError("SlipDate", MessageUtils.GetMessage("E0005", "�d����"));
                if (ModelState["SlipDate"].Errors.Count > 1) {
                    ModelState["SlipDate"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("CarTaxAppropriateAmount")) {
                ModelState.AddModelError("CarTaxAppropriateAmount", MessageUtils.GetMessage("E0004", new string[] { "���ŏ[��", "10���ȓ��̐����̂�" }));
                if (ModelState["CarTaxAppropriateAmount"].Errors.Count > 1) {
                    ModelState["CarTaxAppropriateAmount"].Errors.RemoveAt(0);
                }
            }
            if (!ModelState.IsValidField("RecycleAmount")) {
                ModelState.AddModelError("RecycleAmount", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N��", "10���ȓ��̐����̂�" }));
                if (ModelState["RecycleAmount"].Errors.Count > 1) {
                    ModelState["RecycleAmount"].Errors.RemoveAt(0);
                }
            }
            //if (!ViewData["PurchaseStatus"].Equals("002")) {
            if (string.IsNullOrEmpty(salesCar.CarGradeCode))
            {
                ModelState.AddModelError("SalesCar.CarGradeCode", MessageUtils.GetMessage("E0001", "�O���[�h"));
            }
            else //Add 2018/04/26 arc yano #3816 �O���[�h�R�[�h�����͂���Ă����ꍇ�̓}�X�^�`�F�b�N���s�� 
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("SalesCar.CarGradeCode", "�ԗ��O���[�h�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ�����");
                }
            }

            if (string.IsNullOrEmpty(salesCar.NewUsedType)) {
                ModelState.AddModelError("SalesCar.NewUsedType", MessageUtils.GetMessage("E0001", "�V���敪"));
            }
            if (string.IsNullOrEmpty(salesCar.Vin)) {
                ModelState.AddModelError("SalesCar.Vin", MessageUtils.GetMessage("E0001", "�ԑ�ԍ�"));
            }
            //}

            // �����`�F�b�N
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
                if (!ModelState.IsValidField("SalesCar.SalesPrice")) {
                    ModelState.AddModelError("SalesCar.SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "����10���ȓ��̐����̂�" }));
                }
                //Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
                if (!ModelState.IsValidField("SalesCar.ManufacturingYear"))
                {
                    ModelState.AddModelError("SalesCar.ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "�N��", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
                }
                //if (!ModelState.IsValidField("SalesCar.IssueDate")) {
                //    ModelState.AddModelError("SalesCar.IssueDate", MessageUtils.GetMessage("E0005", "���s��"));
                //}
                //if (!ModelState.IsValidField("SalesCar.RegistrationDate")) {
                //    ModelState.AddModelError("SalesCar.RegistrationDate", MessageUtils.GetMessage("E0005", "�o�^��"));
                //}
                if (!ModelState.IsValidField("SalesCar.Capacity")) {
                    ModelState.AddModelError("SalesCar.Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.MaximumLoadingWeight")) {
                    ModelState.AddModelError("SalesCar.MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "�ő�ύڗ�", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.CarWeight")) {
                    ModelState.AddModelError("SalesCar.CarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��d��", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.TotalCarWeight")) {
                    ModelState.AddModelError("SalesCar.TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ����d��", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.Length")) {
                    ModelState.AddModelError("SalesCar.Length", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.Width")) {
                    ModelState.AddModelError("SalesCar.Width", MessageUtils.GetMessage("E0004", new string[] { "��", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.Height")) {
                    ModelState.AddModelError("SalesCar.Height", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.FFAxileWeight")) {
                    ModelState.AddModelError("SalesCar.FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�O���d", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.FRAxileWeight")) {
                    ModelState.AddModelError("SalesCar.FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�㎲�d", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.RFAxileWeight")) {
                    ModelState.AddModelError("SalesCar.RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��O���d", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.RRAxileWeight")) {
                    ModelState.AddModelError("SalesCar.RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��㎲�d", "���̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.Displacement")) {
                    ModelState.AddModelError("SalesCar.Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐����̂�" }));
                }
                //if (!ModelState.IsValidField("SalesCar.ExpireDate")) {
                //    ModelState.AddModelError("SalesCar.ExpireDate", MessageUtils.GetMessage("E0005", "�L������"));
                //}
                if (!ModelState.IsValidField("SalesCar.Mileage")) {
                    ModelState.AddModelError("SalesCar.Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
                }
                if (!ModelState.IsValidField("SalesCar.CarTax")) {
                    ModelState.AddModelError("SalesCar.CarTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŎ�ʊ�", "����10���ȓ��̐����̂�" }));     //Mod 2019/09/04 yano #4011
                }
                if (!ModelState.IsValidField("SalesCar.CarWeightTax")) {
                    ModelState.AddModelError("SalesCar.CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԏd�ʐ�", "����10���ȓ��̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.AcquisitionTax")) {
                    ModelState.AddModelError("SalesCar.AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŊ����\��", "����10���ȓ��̐����̂�" }));�@//Mod 2019/09/04 yano #4011
                }
                if (!ModelState.IsValidField("SalesCar.CarLiabilityInsurance")) {
                    ModelState.AddModelError("SalesCar.CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "�����ӕی���", "����10���ȓ��̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.RecycleDeposit")) {
                    ModelState.AddModelError("SalesCar.RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "����10���ȓ��̐����̂�" }));
                }
                if (!ModelState.IsValidField("SalesCar.InspectionDate")) {
                    ModelState.AddModelError("SalesCar.InspectionDate", MessageUtils.GetMessage("E0005", "�_����"));
                }
                if (!ModelState.IsValidField("SalesCar.NextInspectionDate")) {
                    ModelState.AddModelError("SalesCar.NextInspectionDate", MessageUtils.GetMessage("E0005", "����_����"));
                }
                if (!ModelState.IsValidField("SalesCar.ProductionDate")) {
                    ModelState.AddModelError("SalesCar.ProductionDate", MessageUtils.GetMessage("E0005", "���Y��"));
                }
                if (!ModelState.IsValidField("SalesCar.ApprovedCarWarrantyDateFrom")) {
                    ModelState.AddModelError("SalesCar.ApprovedCarWarrantyDateFrom", MessageUtils.GetMessage("E0005", "�F�蒆�Îԕۏ؊���(�J�n)"));
                }
                if (!ModelState.IsValidField("SalesCar.ApprovedCarWarrantyDateTo")) {
                    ModelState.AddModelError("SalesCar.ApprovedCarWarrantyDateTo", MessageUtils.GetMessage("E0005", "�F�蒆�Îԕۏ؊���(�I��)"));
                }
 //           }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("VehicleAmount")) {
                if (!Regex.IsMatch(carPurchase.VehicleAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("VehicleAmount", MessageUtils.GetMessage("E0002", new string[] { "�ԗ��{�̉��i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("AuctionFeeAmount")) {
                if (carPurchase.AuctionFeeAmount!=null && !Regex.IsMatch(carPurchase.AuctionFeeAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("AuctionFeeAmount", MessageUtils.GetMessage("E0004", new string[] { "�I�[�N�V�������D��", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("MetallicPrice")) {
                if (!Regex.IsMatch(carPurchase.MetallicPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("MetallicPrice", MessageUtils.GetMessage("E0002", new string[] { "���^���b�N���i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("OptionPrice")) {
                if (!Regex.IsMatch(carPurchase.OptionPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("OptionPrice", MessageUtils.GetMessage("E0002", new string[] { "�I�v�V�������i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("FirmPrice")) {
                if (!Regex.IsMatch(carPurchase.FirmPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("FirmPrice", MessageUtils.GetMessage("E0002", new string[] { "�t�@�[�����i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("DiscountPrice")) {
                if (!Regex.IsMatch(carPurchase.DiscountPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("DiscountPrice", MessageUtils.GetMessage("E0002", new string[] { "�f�B�X�J�E���g���i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("EquipmentPrice")) {
                if (!Regex.IsMatch(carPurchase.EquipmentPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("EquipmentPrice", MessageUtils.GetMessage("E0002", new string[] { "�������i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RepairPrice")) {
                if (!Regex.IsMatch(carPurchase.RepairPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("RepairPrice", MessageUtils.GetMessage("E0002", new string[] { "���C���i", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("OthersPrice")) {
                if (!Regex.IsMatch(carPurchase.OthersPrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("OthersPrice", MessageUtils.GetMessage("E0002", new string[] { "���̑����i", "10���ȓ��̐����̂�" }));
                }
            }
            /*if (ModelState.IsValidField("TaxAmount")) {
                if (!Regex.IsMatch(carPurchase.TaxAmount.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("TaxAmount", MessageUtils.GetMessage("E0002", new string[] { "�����", "10���ȓ��̐����̂�" }));
                }
            }*/
            if (ModelState.IsValidField("CarTaxAppropriatePrice")) {
                if (carPurchase.CarTaxAppropriatePrice != null && !Regex.IsMatch(carPurchase.CarTaxAppropriatePrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("CarTaxAppropriatePrice", MessageUtils.GetMessage("E0004", new string[] { "���ŏ[��", "10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RecyclePrice")) {
                if (carPurchase.RecyclePrice != null && !Regex.IsMatch(carPurchase.RecyclePrice.ToString(), @"^-?\d{1,10}$")) {
                    ModelState.AddModelError("RecyclePrice", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N��", "10���ȓ��̐����̂�" }));
                }
            } 
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
                if (ModelState.IsValidField("SalesCar.SalesPrice") && salesCar.SalesPrice != null) {
                    if (!Regex.IsMatch(salesCar.SalesPrice.ToString(), @"^\d{1,10}$")) {
                        ModelState.AddModelError("SalesCar.SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "����10���ȓ��̐����̂�" }));
                    }
                }

            //Mod 2021/08/02 yano #4097 ���͉\�����t�H�[�}�b�g��ύX(���̐����S���̂݁����̐���4���A�܂��͐��̐���4��������2���ȓ�
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
            if (ModelState.IsValidField("SalesCar.ManufacturingYear") && CommonUtils.DefaultString(salesCar.ManufacturingYear).Equals("") == false)
            {
              if (((!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}\.\d{1,2}$"))
                && (!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}$")))
              )
              {
                  ModelState.AddModelError("SalesCar.ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "�N��", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
              }
            }

            if (ModelState.IsValidField("SalesCar.Mileage") && salesCar.Mileage != null) {
                if ((Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("SalesCar.Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.CarTax") && salesCar.CarTax != null) {
                if (!Regex.IsMatch(salesCar.CarTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŎ�ʊ�", "����10���ȓ��̐����̂�" })); //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("SalesCar.CarWeightTax") && salesCar.CarWeightTax != null) {
                if (!Regex.IsMatch(salesCar.CarWeightTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԏd�ʐ�", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.AcquisitionTax") && salesCar.AcquisitionTax != null) {
                if (!Regex.IsMatch(salesCar.AcquisitionTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŊ����\��", "����10���ȓ��̐����̂�" }));//Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("SalesCar.CarLiabilityInsurance") && salesCar.CarLiabilityInsurance != null) {
                if (!Regex.IsMatch(salesCar.CarLiabilityInsurance.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "�����ӕی���", "����10���ȓ��̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RecycleDeposit") && salesCar.RecycleDeposit != null) {
                if (!Regex.IsMatch(salesCar.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesCar.RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "����10���ȓ��̐����̂�" }));
                }
            }
            //�Ǘ��ԍ�����͂̏ꍇ�A�����̔ԂƔ��v�f���Ȃ���
            if (ModelState.IsValidField("SalesCar.SalesCarNumber") && !string.IsNullOrEmpty(salesCar.SalesCarNumber)) {
                SalesCar existsCar = new SalesCarDao(db).GetByKey(salesCar.SalesCarNumber);
                //�ԗ��}�X�^�V�K�o�^�̏ꍇ
                if (existsCar == null && !new SerialNumberDao(db).CanUseSalesCarNumber(salesCar.SalesCarNumber)) {
                    ModelState.AddModelError("SalesCar.SalesCarNumber", "�w�肳�ꂽ�Ǘ��ԍ��͎����̔ԂŎg�p����͈͂Ɋ܂܂�邽�ߎg�p�ł��܂���");
                }
            }

            //���ɓ����I�����ߏ����ς݂̌����ł���΃G���[
            if (carPurchase.PurchaseDate != null) {
                // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���ɓ��ł͎d���������ł��܂���");
                }
            }

            // �l�`�F�b�N
            if (ModelState.IsValidField("Amount")) {
                if (decimal.Compare(carPurchase.Amount, -9999999999m) < 0 || decimal.Compare(carPurchase.Amount, 9999999999m) > 0) {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0018", "�d�����z"));
                }
            }
//            if (!ViewData["PurchaseStatus"].Equals("002")) {
            if (ModelState.IsValidField("SalesCar.Capacity") && salesCar.Capacity != null) {
                if (salesCar.Capacity < 0) {
                    ModelState.AddModelError("SalesCar.Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.MaximumLoadingWeight") && salesCar.MaximumLoadingWeight != null) {
                if (salesCar.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("SalesCar.MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "�ő�ύڗ�", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.CarWeight") && salesCar.CarWeight != null) {
                if (salesCar.CarWeight < 0) {
                    ModelState.AddModelError("SalesCar.CarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.TotalCarWeight") && salesCar.TotalCarWeight != null) {
                if (salesCar.TotalCarWeight < 0) {
                    ModelState.AddModelError("SalesCar.TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ����d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Length") && salesCar.Length != null) {
                if (salesCar.Length < 0) {
                    ModelState.AddModelError("SalesCar.Length", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Width") && salesCar.Width != null) {
                if (salesCar.Width < 0) {
                    ModelState.AddModelError("SalesCar.Width", MessageUtils.GetMessage("E0004", new string[] { "��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Height") && salesCar.Height != null) {
                if (salesCar.Height < 0) {
                    ModelState.AddModelError("SalesCar.Height", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.FFAxileWeight") && salesCar.FFAxileWeight != null) {
                if (salesCar.FFAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.FRAxileWeight") && salesCar.FRAxileWeight != null) {
                if (salesCar.FRAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RFAxileWeight") && salesCar.RFAxileWeight != null) {
                if (salesCar.RFAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.RRAxileWeight") && salesCar.RRAxileWeight != null) {
                if (salesCar.RRAxileWeight < 0) {
                    ModelState.AddModelError("SalesCar.RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesCar.Displacement") && salesCar.Displacement != null) {
                if (salesCar.Displacement < 0) {
                    ModelState.AddModelError("SalesCar.Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐����̂�" }));
                }
            }

            // �a�����̕ϊ��`�F�b�N
            if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki)) {
                ModelState.AddModelError("SalesCar.IssueDateWareki.Year", MessageUtils.GetMessage("E0021", "���s��"));
            }
            if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki)) {
                ModelState.AddModelError("SalesCar.RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "�o�^�N�����^��t�N����"));
            }

            salesCar.FirstRegistrationDateWareki.Day = 1;
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {
                if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db)) {    //Mod 2018/06/22 arc yano #3891
                //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki)) {
                    ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "���x�o�^�N��"));
                    ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Month", "");
                }
                //Add 2020/11/17 yano #4065
                else
                {
                    DateTime? FirstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);

                    //���x�o�^�N�����{�����30���ȍ~�̓��t�Őݒ肳��Ă����ꍇ
                    if (FirstRegistrationDate != null && (((DateTime)(FirstRegistrationDate ?? DateTime.Today).Date - DateTime.Today.Date).TotalDays > 30))
                    {
                        ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Year", "���x�o�^�N���ɖ����̓��t�͐ݒ�ł��܂���");
                        ModelState.AddModelError("SalesCar.FirstRegistrationDateWareki.Month", "");
                    }
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki)) {
                ModelState.AddModelError("ExpireDateWareki.Year", MessageUtils.GetMessage("E0021", "�L������"));
            }
            if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki)) {
                ModelState.AddModelError("InspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "�_����"));
            }
            if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki, db)) {  //Mod 2018/06/22 arc yano #3891
            //if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki)) {
                ModelState.AddModelError("NextInspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "����_����"));
            }
            
            //�d���v�㎞�A���A���ɋ敪���u����ԁv�̏ꍇ�݂̂̃`�F�b�N

            //2019/02/08 yano #3965
            if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB")) //WE�ňȊO
            {
                //Add 2017/02/01 arc nakayama #3701_�ԗ��d���@���ɋ敪���u����ԁv�ȊO�Ŏd���v���A�ۑ����s���ƃV�X�e���G���[
                if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                {

                    //�`�[�ԍ��̕K�{�`�F�b�N
                    if (string.IsNullOrEmpty(carPurchase.SlipNumber))
                    {
                        ModelState.AddModelError("SlipNumber", "����Ԃ��d���v�シ��ꍇ�A�`�[�ԍ��͓��͕K�{�ł�");
                        return carPurchase;
                    }

                    //�`�[�̗L���m�F
                    CarSalesHeader CarSlip = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                    if (CarSlip == null)
                    {
                        ModelState.AddModelError("SlipNumber", "���͂��ꂽ�`�[�͑��݂��Ă��܂���");
                        return carPurchase;
                    }

                    //�d���悪�`�[�̌ڋq�ƈ�v���Ă��邩
                    if (!CarSlip.CustomerCode.Equals(carPurchase.SupplierCode))
                    {
                        ModelState.AddModelError("SupplierCode", "���͂��ꂽ�d���悪�`�[�̌ڋq�ƈ�v���Ă��܂���");
                    }

                    //�d���v�シ��ԗ����A�Y���̓`�[�ɑ��݂��Ă��邩�`�F�b�N����
                    List<TradeInVinList> VinList = new List<TradeInVinList>();

                    //�ԗ��`�[���S�Ẳ���Ԃ̎ԑ�ԍ����擾
                    for (int i = 1; i <= 3; i++)
                    {
                        object vin = CommonUtils.GetModelProperty(CarSlip, "TradeInVin" + i);
                        if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                        {
                            TradeInVinList TradeInVin = new TradeInVinList();
                            TradeInVin.Vin = vin.ToString();
                            VinList.Add(TradeInVin);
                        }
                    }

                    //����Ԃ̎ԑ�ԍ���������݂��Ȃ������ꍇ
                    if (VinList == null || VinList.Count <= 0)
                    {
                        ModelState.AddModelError("SlipNumber", "���͂���Ă���ԗ��`�[�ɉ���Ԃ����݂��Ă��܂���");
                    }
                    else
                    {
                        //����Ԃ̎ԑ�ԍ��͂��邪�A��v������̂��Ȃ������ꍇ
                        var ret = VinList.Where(x => x.Vin == salesCar.Vin).FirstOrDefault();

                        if (ret == null)
                        {
                            ModelState.AddModelError("salesCar_Vin", "���͂���Ă���ԑ�ԍ����ԗ��`�[�̉���Ԃɑ��݂��Ă��܂���");
                        }
                    }
                }
            }
//            }

            //�L�����Z���ȊO�̏ꍇ
            if (form["update"].Equals("1") && (string.IsNullOrWhiteSpace(form["action"]) || !form["action"].Equals("CancelStock")))
            {
                if (carPurchase.CancelDate != null)
                {
                    ModelState.AddModelError("CancelDate", "�d����L�����Z�����s��Ȃ��ꍇ�́A�L�����Z������o�^�ł��܂���");
                }

                if (!string.IsNullOrEmpty(carPurchase.CancelMemo))
                {
                    ModelState.AddModelError("CancelMemo", "�d����L�����Z�����s��Ȃ��ꍇ�́A�L�����Z��������o�^�ł��܂���");
                }
            }


            //Add 2018/10/25 yano #3947
            //------------------------------
            //�Õ�������̊m�F���@
            //------------------------------
            if (salesCar.NewUsedType.Equals("U"))   //���ÎԂ̏ꍇ
            {
                //�d���v��A�܂��͎d���ό�̕ۑ�
                if ((!string.IsNullOrWhiteSpace(form["action"]) && form["action"].Equals("saveStock")) ||
                     (!string.IsNullOrWhiteSpace(carPurchase.PurchaseStatus) && carPurchase.PurchaseStatus.Equals("002"))
                    )
                {    
                    if (salesCar.ConfirmDriverLicense.Equals(false) && salesCar.ConfirmCertificationSeal.Equals(false) && string.IsNullOrWhiteSpace(salesCar.ConfirmOther))
                    {
                        ModelState.AddModelError("salesCar.ConfirmDriverLicense", MessageUtils.GetMessage("E0001", "�Õ�������̊m�F���@"));
                        ModelState.AddModelError("salesCar.ConfirmCertificationSeal", "");
                        ModelState.AddModelError("salesCar.ConfirmOther", "");
                    }
                }
            }
  
            return carPurchase;
        }
        #endregion

        #region �R�s�[����
        /// <summary>
        /// �ԗ��d���e�[�u���R�s�[�f�[�^�쐬
        /// </summary>
        /// <param name="carPurchaseId">�R�s�[���ԗ��d��ID</param>
        /// <returns>�ԗ��d���e�[�u�����f���N���X</returns>
        /// <history>
        /// 2018/06/25 arc yano #3895 �ԗ��d�����́@�Ǘ��ԍ��̔񊈐����Ō������s��̏C��
        /// 2018/03/20 arc yano #3871 �ԗ��d�����́@�R�s�[�쐬���A�ԗ��{�̉��i�A�d�����i�̌v�Z���s���Ƌ��z��������
        /// </history>
        private CarPurchase CreateCopyCarPurchase(Guid carPurchaseId) {

            CarPurchase ret = new CarPurchase();
            ret.SalesCar = new SalesCar();

            CarPurchase src = new CarPurchaseDao(db).GetByKey(carPurchaseId);

            ret.PurchaseDate = src.PurchaseDate;
            ret.SupplierCode = src.SupplierCode;
            ret.PurchaseLocationCode = src.PurchaseLocationCode;
            ret.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            ret.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            ret.VehiclePrice = src.VehiclePrice;
            ret.VehicleTax = src.VehicleTax;
            ret.VehicleAmount = src.VehicleAmount;
            ret.MetallicPrice = src.MetallicPrice;
            ret.MetallicTax = src.MetallicTax;
            ret.MetallicAmount = src.MetallicAmount;
            ret.OptionPrice = src.OptionPrice;
            ret.OptionTax = src.OptionTax;
            ret.OptionAmount = src.OptionAmount;
            ret.FirmPrice = src.FirmPrice;
            ret.FirmTax = src.FirmTax;
            ret.FirmAmount = src.FirmAmount;
            ret.DiscountPrice = src.DiscountPrice;
            ret.DiscountTax = src.DiscountTax;
            ret.DiscountAmount = src.DiscountAmount;
            ret.EquipmentPrice = src.EquipmentPrice;
            ret.RepairPrice = src.RepairPrice;
            ret.OthersPrice = src.OthersPrice;
            ret.OthersTax = src.OthersTax;
            ret.OthersAmount = src.OthersAmount;
            ret.Amount = src.Amount;
            ret.TaxAmount = src.TaxAmount;
            ret.TotalAmount = src.TotalAmount;
            ret.CarTaxAppropriatePrice = src.CarTaxAppropriatePrice;
            ret.CarTaxAppropriateAmount = src.CarTaxAppropriateAmount;
            ret.CarPurchaseType = src.CarPurchaseType;
            
            //����ŗ�
            //Add 2018/06/25 arc yano #3895
            //Add 2014/06/24 arc yano ���t�̐ŗ������o�O�Ή�
            //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
            if (src.ConsumptionTaxId == null)
            {
                ret.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
            }
            else
            {
                ret.ConsumptionTaxId = src.ConsumptionTaxId;
            }
            //ret.ConsumptionTaxId = src.ConsumptionTaxId;
            ret.Rate = (src.Rate ?? int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(ret.ConsumptionTaxId)));        //Add 2018/03/20 arc yano #3871
            ret.RecyclePrice = src.RecyclePrice;
            ret.RecycleAmount = src.RecycleAmount;
            ret.AuctionFeePrice = src.AuctionFeePrice;
            ret.AuctionFeeTax = src.AuctionFeeTax;
            ret.AuctionFeeAmount = src.AuctionFeeAmount;
            ret.LastEditScreen = src.LastEditScreen;
            ret.SalesCar.CarGradeCode = src.SalesCar.CarGradeCode;
            ret.SalesCar.NewUsedType = src.SalesCar.NewUsedType;
            ret.SalesCar.ColorType = src.SalesCar.ColorType;
            ret.SalesCar.ExteriorColorCode = src.SalesCar.ExteriorColorCode;
            ret.SalesCar.ExteriorColorName = src.SalesCar.ExteriorColorName;
            ret.SalesCar.ChangeColor = src.SalesCar.ChangeColor;
            ret.SalesCar.InteriorColorCode = src.SalesCar.InteriorColorCode;
            ret.SalesCar.InteriorColorName = src.SalesCar.InteriorColorName;
            ret.SalesCar.ManufacturingYear = src.SalesCar.ManufacturingYear;
            ret.SalesCar.Steering = src.SalesCar.Steering;
            ret.SalesCar.SalesPrice = src.SalesCar.SalesPrice;
            ret.SalesCar.IssueDate = src.SalesCar.IssueDate;
            ret.SalesCar.MorterViecleOfficialCode = src.SalesCar.MorterViecleOfficialCode;
            ret.SalesCar.RegistrationNumberType = src.SalesCar.RegistrationNumberType;
            ret.SalesCar.RegistrationNumberKana = src.SalesCar.RegistrationNumberKana;
            ret.SalesCar.RegistrationNumberPlate = src.SalesCar.RegistrationNumberPlate;
            ret.SalesCar.RegistrationDate = src.SalesCar.RegistrationDate;
            ret.SalesCar.FirstRegistrationYear = src.SalesCar.FirstRegistrationYear;
            ret.SalesCar.CarClassification = src.SalesCar.CarClassification;
            ret.SalesCar.Usage = src.SalesCar.Usage;
            ret.SalesCar.UsageType = src.SalesCar.UsageType;
            ret.SalesCar.Figure = src.SalesCar.Figure;
            ret.SalesCar.MakerName = src.SalesCar.MakerName;
            ret.SalesCar.Capacity = src.SalesCar.Capacity;
            ret.SalesCar.MaximumLoadingWeight = src.SalesCar.MaximumLoadingWeight;
            ret.SalesCar.CarWeight = src.SalesCar.CarWeight;
            ret.SalesCar.TotalCarWeight = src.SalesCar.TotalCarWeight;
            ret.SalesCar.Vin = src.SalesCar.Vin;
            ret.SalesCar.Length = src.SalesCar.Length;
            ret.SalesCar.Width = src.SalesCar.Width;
            ret.SalesCar.Height = src.SalesCar.Height;
            ret.SalesCar.FFAxileWeight = src.SalesCar.FFAxileWeight;
            ret.SalesCar.FRAxileWeight = src.SalesCar.FRAxileWeight;
            ret.SalesCar.RFAxileWeight = src.SalesCar.RFAxileWeight;
            ret.SalesCar.RRAxileWeight = src.SalesCar.RRAxileWeight;
            ret.SalesCar.ModelName = src.SalesCar.ModelName;
            ret.SalesCar.EngineType = src.SalesCar.EngineType;
            ret.SalesCar.Displacement = src.SalesCar.Displacement;
            ret.SalesCar.Fuel = src.SalesCar.Fuel;
            ret.SalesCar.ModelSpecificateNumber = src.SalesCar.ModelSpecificateNumber;
            ret.SalesCar.ClassificationTypeNumber = src.SalesCar.ClassificationTypeNumber;
            ret.SalesCar.ExpireType = src.SalesCar.ExpireType;
            ret.SalesCar.ExpireDate = src.SalesCar.ExpireDate;
            ret.SalesCar.Mileage = src.SalesCar.Mileage;
            ret.SalesCar.MileageUnit = src.SalesCar.MileageUnit;
            ret.SalesCar.Memo = src.SalesCar.Memo;
            ret.SalesCar.InspectionDate = src.SalesCar.InspectionDate;
            ret.SalesCar.NextInspectionDate = src.SalesCar.NextInspectionDate;
            ret.SalesCar.UsVin = src.SalesCar.UsVin;
            ret.SalesCar.MakerWarranty = src.SalesCar.MakerWarranty;
            ret.SalesCar.RecordingNote = src.SalesCar.RecordingNote;
            ret.SalesCar.ProductionDate = src.SalesCar.ProductionDate;
            ret.SalesCar.ReparationRecord = src.SalesCar.ReparationRecord;
            ret.SalesCar.Tire = src.SalesCar.Tire;
            ret.SalesCar.KeyCode = src.SalesCar.KeyCode;
            ret.SalesCar.AudioCode = src.SalesCar.AudioCode;
            ret.SalesCar.Import = src.SalesCar.Import;
            ret.SalesCar.Guarantee = src.SalesCar.Guarantee;
            ret.SalesCar.Instructions = src.SalesCar.Instructions;
            ret.SalesCar.Recycle = src.SalesCar.Recycle;
            ret.SalesCar.RecycleTicket = src.SalesCar.RecycleTicket;
            ret.SalesCar.CouponPresence = src.SalesCar.CouponPresence;
            ret.SalesCar.Light = src.SalesCar.Light;
            ret.SalesCar.Aw = src.SalesCar.Aw;
            ret.SalesCar.Aero = src.SalesCar.Aero;
            ret.SalesCar.Sr = src.SalesCar.Sr;
            ret.SalesCar.Cd = src.SalesCar.Cd;
            ret.SalesCar.Md = src.SalesCar.Md;
            ret.SalesCar.NaviType = src.SalesCar.NaviType;
            ret.SalesCar.NaviEquipment = src.SalesCar.NaviEquipment;
            ret.SalesCar.NaviDashboard = src.SalesCar.NaviDashboard;
            ret.SalesCar.SeatColor = src.SalesCar.SeatColor;
            ret.SalesCar.SeatType = src.SalesCar.SeatType;
            ret.SalesCar.Memo1 = src.SalesCar.Memo1;
            ret.SalesCar.Memo2 = src.SalesCar.Memo2;
            ret.SalesCar.Memo3 = src.SalesCar.Memo3;
            ret.SalesCar.Memo4 = src.SalesCar.Memo4;
            ret.SalesCar.Memo5 = src.SalesCar.Memo5;
            ret.SalesCar.Memo6 = src.SalesCar.Memo6;
            ret.SalesCar.Memo7 = src.SalesCar.Memo7;
            ret.SalesCar.Memo8 = src.SalesCar.Memo8;
            ret.SalesCar.Memo9 = src.SalesCar.Memo9;
            ret.SalesCar.Memo10 = src.SalesCar.Memo10;
            ret.SalesCar.DeclarationType = src.SalesCar.DeclarationType;
            ret.SalesCar.AcquisitionReason = src.SalesCar.AcquisitionReason;
            ret.SalesCar.TaxationTypeCarTax = src.SalesCar.TaxationTypeCarTax;
            ret.SalesCar.TaxationTypeAcquisitionTax = src.SalesCar.TaxationTypeAcquisitionTax;
            ret.SalesCar.CarTax = src.SalesCar.CarTax;
            ret.SalesCar.CarWeightTax = src.SalesCar.CarWeightTax;
            ret.SalesCar.CarLiabilityInsurance = src.SalesCar.CarLiabilityInsurance;
            ret.SalesCar.AcquisitionTax = src.SalesCar.AcquisitionTax;
            ret.SalesCar.RecycleDeposit = src.SalesCar.RecycleDeposit;
            ret.SalesCar.EraseRegist = src.SalesCar.EraseRegist;
            return ret;
        }
        #endregion

        #region �f�[�^�ҏW
        /// <summary>
        /// �ԗ��d���e�[�u���ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carPurchase">�ԗ��d���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��d���e�[�u�����f���N���X</returns>
        /// <history>
        /// 2020/11/27 yano #4072 �����@�^�����̓G���A�̊g�� �����@�^���̐ݒ蕶�����ȏ����Ȃ������̔p�~
        /// 2019/02/08 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���)
        /// 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\)�@�@�\���P�A �������i�͎d�����z���烊�T�C�N�����������������i�Ƃ���
        /// 2018/06/06 arc yano #3883 �^�}�\���P �d�����i�ō������i���X�V����
        /// 2017/11/14 arc yano  #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s���� ����Ԃ̎d�����ɓ����\��̍č쐬���s���悤�ɏC��
        /// </history>
        private CarPurchase EditCarPurchaseForInsert(CarPurchase carPurchase, FormCollection form) {

            // �ԗ����ҏW
            if (string.IsNullOrEmpty(carPurchase.SalesCar.SalesCarNumber)) {
                string companyCode = "N/A";
                try { companyCode = new CarGradeDao(db).GetByKey(carPurchase.SalesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
                carPurchase.SalesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, carPurchase.SalesCar.NewUsedType);
            }//else {
            //    carPurchase.SalesCar.SalesCarNumber = carPurchase.SalesCarNumber;
            //}
            if (form["action"].Equals("saveStock")) {
                carPurchase.SalesCar.CarStatus = "001";
                carPurchase.SalesCar.LocationCode = carPurchase.PurchaseLocationCode;

                //Add 2019/02/08 yano #3965
                if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                { //WE�ňȊO
                    if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                    {
                        //����Ԃœ`�[�ԍ�������ꍇ�́A�����f�[�^���X�V����
                        //Add 2017/02/14 arc nakayama #3704_�ԗ��d���@�d���v��O�̃f�[�^��ۑ�����Ɠ������т��쐬�����
                        UpdateTradeCarJournal(carPurchase, form);
                        CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                        CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                    }
                }
            }
            // Vin�̕ϊ��@MOD 2014/10/16 arc ishii 
            //carPurchase.SalesCar.Vin = CommonUtils.abc123ToHankaku(carPurchase.SalesCar.Vin);
            carPurchase.SalesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.Vin));
            //ADD 2014/10/22 arc ishii Vin��20�����ȏ�̏ꍇ������20�����ڂ܂Ŕ����o��
            if (carPurchase.SalesCar.Vin.Length > 20)
            {
               carPurchase.SalesCar.Vin = carPurchase.SalesCar.Vin.Substring(0, 20);
            }
            carPurchase.SalesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.CreateDate = DateTime.Now;
            carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            carPurchase.SalesCar.DelFlag = "0";
            //EngineType�ϊ��@ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.EngineType));

            //Mod Mod 2020/11/27 yano #4072
            //Add 2015/03/26 arc iijima Null�����Ή��̂��ߔ���ǉ�
            //ADD 2014/10/22 arc ishii EngineType��10�����ȏ�̏ꍇ������10�����ڂ܂Ŕ����o��
            //if ((!string.IsNullOrEmpty(carPurchase.SalesCar.EngineType)) && (carPurchase.SalesCar.EngineType.Length > 10))
            //{
            //    carPurchase.SalesCar.EngineType = carPurchase.SalesCar.EngineType.Substring(0, 10);
            //}


            // �Â��Ǘ��ԍ��𗚗��e�[�u���Ɉړ�
            if (carPurchase.RegetVin) {
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(carPurchase.SalesCar.Vin);
                foreach (var item in salesCarList) {
                    CommonUtils.CopyToSalesCarHistory(db, item);
                    item.DelFlag = "1";
                }
            }

            // �ԗ��d�����ҏW
            carPurchase.CarPurchaseId = Guid.NewGuid();
            
            if (form["action"].Equals("saveStock")) {
                carPurchase.PurchaseStatus = "002";
            } else {
                carPurchase.PurchaseStatus = "001";
            }
            carPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.CreateDate = DateTime.Now;
            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.LastUpdateDate = DateTime.Now;
            carPurchase.DelFlag = "0";

            //Mod 2018/08/28 yano #3922
            //Add 2018/06/06 arc yano #3883
            ////�������i�����T�C�N���������������S�Ă̋��z�ōX�V
            //if (carPurchase.SalesCar != null && !string.IsNullOrWhiteSpace(carPurchase.SalesCar.NewUsedType) && carPurchase.SalesCar.NewUsedType.Equals("U"))
            //{

                decimal purchaseprice = 0m;

                purchaseprice = carPurchase.VehiclePrice +                              //�ԗ��{�̉��i(�Ŕ�)
                                (carPurchase.AuctionFeePrice ?? 0m) +                   //�I�[�N�V�������D��(�Ŕ�)
                                (carPurchase.CarTaxAppropriatePrice ?? 0m) +            //���ŏ[��(�Ŕ�)
                                carPurchase.OthersPrice +                               //���̑����i(�Ŕ�)
                                carPurchase.MetallicPrice +                             //���^���b�N���i(�Ŕ�)
                                carPurchase.OptionPrice +                               //�I�v�V�������i(�Ŕ�)     
                                carPurchase.FirmPrice +                                 //�t�@�[�����i(�Ŕ�)
                                carPurchase.DiscountPrice +                             //�f�B�X�J�E���g���i(�Ŕ�)
                                carPurchase.EquipmentPrice +                            //�������i(�Ŕ�)
                                carPurchase.RepairPrice;                                //���C���i(�Ŕ�)


                if (purchaseprice > 0)
                {
                    carPurchase.FinancialAmount = purchaseprice;
                }
                else
                {
                    carPurchase.FinancialAmount = 1m;
                }


                //�d�����i(�ō�)���烊�T�C�N�������������d�����i(�Ŕ�)���Z�o ���󖢎g�p
                ////�����_�̐ŗ��������Őݒ�
                //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(carPurchase.ConsumptionTaxId);

                ////�d���f�[�^�ɏ���ŗ�ID���ݒ肳��Ă���ꍇ�͂�������g��
                //if (rate != null)
                //{
                //    taxrate = rate.Rate;
                //}

                //decimal value = (carPurchase.TotalAmount ?? 0) - (carPurchase.RecycleAmount ?? 0);

                //if (value > 0)
                //{
                //    //�ō����z����Ŕ����z���v�Z(�[���؂�̂�)
                //    carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                //}
                //else
                //{
                //    if ((carPurchase.TotalAmount ?? 0) > 0)
                //    {
                //        carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax((carPurchase.TotalAmount ?? 0), taxrate, 1);
                //    }
                //    else
                //    {
                //        carPurchase.FinancialAmount = 1m;
                //    }
                //}
                
                //carPurchase.FinancialAmount = carPurchase.Amount;   
            //}
            
            return carPurchase;
        }

        /// <summary>
        /// �ԗ��d���e�[�u���X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carPurchase">�ԗ��d���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��d���e�[�u�����f���N���X</returns>
        /// <history>
        /// 2021/08/09 yano #4086�y�ԗ��d�����́z����Ԃ�ύX�����ۂ̓������у��X�g�폜�R��Ή� �����ǉ�
        /// 2020/11/27 yano #4072 �����@�^�����̓G���A�̊g���@�ݒ蕶�����ȏ����Ȃ������̔p�~
        /// 2019/02/08 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���)
        /// 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\)�@�@�\���P�A �������i�͎d�����z���烊�T�C�N�����������������i�Ƃ���
        /// 2018/06/06 arc yano #3883 �^�}�\���P �d�����i�ō������i���X�V����
        /// 2017/11/14 arc yano  #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s���� ����Ԃ̎d�����ɓ����\��̍č쐬���s���悤�ɏC��
        /// </history>
        private CarPurchase EditCarPurchaseForUpdate(CarPurchase carPurchase, FormCollection form, string prevSlipNumber, string prevVin) {//Mod 2021/08/09 yano #4086

            // �ԗ����ҏW
            if (!ViewData["PurchaseStatus"].Equals("002"))
            {
                if (form["action"].Equals("saveStock"))
                {
                    carPurchase.SalesCar.CarStatus = "001";
                    carPurchase.SalesCar.LocationCode = carPurchase.PurchaseLocationCode;


                    //Add  2019/02/08 yano #3965
                    if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                    { //WE�ňȊO
                        //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~ 
                        //Add 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                        if (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals("001"))
                        {
                            //����Ԃœ`�[�ԍ�������ꍇ�́A�����f�[�^���X�V����
                            UpdateTradeCarJournal(carPurchase, form);
                            CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);

                            CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                        }
                    }

                }
                carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            }
            else
            {
                //Add  2019/02/08 yano #3965
                if (Session["ConnectDB"] != null && !Session["ConnectDB"].Equals("WE_DB"))
                {
                    //Add 2021/08/09 yano #4086
                    //�ҏW�O������Ԃ̏ꍇ
                    if(!string.IsNullOrWhiteSpace(prevSlipNumber) && !string.IsNullOrWhiteSpace(prevVin))
                    {
                        //�`�[�ԍ��ύX�܂��͎d���敪������ԁ�����ԈȊO�̏ꍇ
                        if( !prevSlipNumber.Equals(carPurchase.SlipNumber) || (!string.IsNullOrWhiteSpace(carPurchase.CarPurchaseType) && !carPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN)))
                        {
                            //�����̉���������т��폜����
                            DeleteJournal(prevSlipNumber, prevVin);
                        }
                    }

                    //Add 2017/02/01 arc nakayama #3701_�ԗ��d���@���ɋ敪���u����ԁv�ȊO�Ŏd���v���A�ۑ����s���ƃV�X�e���G���[
                    if (!string.IsNullOrEmpty(carPurchase.SlipNumber) && (!string.IsNullOrEmpty(carPurchase.CarPurchaseType) && carPurchase.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN)))
                    {
                        //Add 2017/02/14 arc nakayama #3704_�ԗ��d���@�d���v��O�̃f�[�^��ۑ�����Ɠ������т��쐬�����
                        UpdateTradeCarJournal(carPurchase, form);

                        //Add 2017/11/14 arc yano  #3811
                        CarSalesHeader CarHeader = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber);
                        CreateTradeReceiptPlan(CarHeader, carPurchase); //Mod 2017/11/14 arc yano  #3811
                    }
                }
            }

            // �ԗ��d�����ҏW
            if (form["action"].Equals("saveStock")) {
                carPurchase.PurchaseStatus = "002";

            }

            //Mod 2018/08/28 yano #3922
            //Add 2018/06/06 arc yano #3883
            //�������i�����T�C�N���������������S�Ă̋��z�ōX�V
            
            //if(carPurchase.SalesCar != null && !string.IsNullOrWhiteSpace(carPurchase.SalesCar.NewUsedType) && carPurchase.SalesCar.NewUsedType.Equals("U"))
            //{

            //�t�@�C�i���X�f�[�^�捞�ςłȂ��ꍇ�͍X�V����
            if (string.IsNullOrWhiteSpace(carPurchase.FinancialAmountLocked) || !carPurchase.FinancialAmountLocked.Equals("1"))
            {
                decimal purchaseprice = 0m;

                purchaseprice = carPurchase.VehiclePrice +                              //�ԗ��{�̉��i(�Ŕ�)
                                (carPurchase.AuctionFeePrice ?? 0m) +                   //�I�[�N�V�������D��(�Ŕ�)
                                (carPurchase.CarTaxAppropriatePrice ?? 0m) +            //���ŏ[��(�Ŕ�)
                                carPurchase.OthersPrice +                               //���̑����i(�Ŕ�)
                                carPurchase.MetallicPrice +                             //���^���b�N���i(�Ŕ�)
                                carPurchase.OptionPrice +                               //�I�v�V�������i(�Ŕ�)     
                                carPurchase.FirmPrice +                                 //�t�@�[�����i(�Ŕ�)
                                carPurchase.DiscountPrice +                             //�f�B�X�J�E���g���i(�Ŕ�)
                                carPurchase.EquipmentPrice +                            //�������i(�Ŕ�)
                                carPurchase.RepairPrice;                                //���C���i(�Ŕ�)


                if (purchaseprice > 0)
                {
                    carPurchase.FinancialAmount = purchaseprice;
                }
                else
                {
                    carPurchase.FinancialAmount = 1;
                }
            }

           
                //�d�����z����擾����ꍇ(���󖢎g�p)
                //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(carPurchase.ConsumptionTaxId);

                ////�d���f�[�^�ɏ���ŗ�ID���ݒ肳��Ă���ꍇ�͂�������g��
                //if (rate != null)
                //{
                //    taxrate = rate.Rate;
                //}

                //decimal value = (carPurchase.TotalAmount ?? 0) - (carPurchase.RecycleAmount ?? 0);

                //if (value > 0)
                //{
                //    //�ō����z����Ŕ����z���v�Z(�[���؂�̂�)
                //    carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                //}
                //else
                //{
                //    if ((carPurchase.TotalAmount ?? 0) > 0)
                //    {
                //        carPurchase.FinancialAmount = CommonUtils.CalcAmountWithoutTax((carPurchase.TotalAmount ?? 0), taxrate, 1);
                //    }
                //    else
                //    {
                //        carPurchase.FinancialAmount = 1m;
                //    }
                //}

                //carPurchase.FinancialAmount = carPurchase.Amount;   
            //}
            
            // Vin�̕ϊ��@ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.Vin));
            //ADD 2014/10/22 arc ishii Vin��20�����ȏ�̏ꍇ������20�����ڂ܂Ŕ����o��
            if (carPurchase.SalesCar.Vin.Length > 20)
            {
                carPurchase.SalesCar.Vin = carPurchase.SalesCar.Vin.Substring(0, 20);
            }
            carPurchase.SalesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.SalesCar.LastUpdateDate = DateTime.Now;
            carPurchase.SalesCar.DelFlag = "0";
            //EngineType�ϊ��@ADD 2014/10/16 arc ishii 
            carPurchase.SalesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(carPurchase.SalesCar.EngineType));

            //Add 2015/03/26 arc iijima Null�����Ή��̂��ߔ���ǉ�

            //Mod 2020/11/27 yano #4072
            //ADD 2014/10/22 arc ishii EngineType��10�����ȏ�̏ꍇ������10�����ڂ܂Ŕ����o��
            //if ((!string.IsNullOrEmpty(carPurchase.SalesCar.EngineType)) && (carPurchase.SalesCar.EngineType.Length > 10))
            //{
            //    carPurchase.SalesCar.EngineType = carPurchase.SalesCar.EngineType.Substring(0, 10);
            //}

            carPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carPurchase.LastUpdateDate = DateTime.Now;

            return carPurchase;
        }
        #endregion

        /*#region ����Ԃ̓������т��쐬����
        private void CreateTradeCarJournal(CarPurchase carPurchase)
        {
            string CustomerClaimCode = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber).CustomerCode;
            string JournalDate = string.Format("{0:yyyy/MM/dd}", carPurchase.PurchaseDate);

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }


            Journal NewJournal = new Journal();
            NewJournal.JournalId = Guid.NewGuid();
            NewJournal.JournalType = "001";
            NewJournal.DepartmentCode = carPurchase.DepartmentCode;
            NewJournal.CustomerClaimCode = CustomerClaimCode;
            NewJournal.SlipNumber = carPurchase.SlipNumber;
            NewJournal.JournalDate = DateTime.Parse(JournalDate);
            NewJournal.AccountType = "013";//����
            NewJournal.AccountCode = carAccount.AccountCode;
            NewJournal.Amount = carPurchase.TotalAmount ?? 0;
            NewJournal.Summary = null;
            NewJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;
            NewJournal.CreateDate = DateTime.Now;
            NewJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;
            NewJournal.LastUpdateDate = DateTime.Now;
            NewJournal.DelFlag = "0";
            NewJournal.ReceiptPlanFlag = "1";
            NewJournal.TransferFlag = null;
            NewJournal.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode,false).OfficeCode;
            NewJournal.CashAccountCode = null;
            NewJournal.PaymentKindCode = null;
            NewJournal.CreditReceiptPlanId = null;

            db.Journal.InsertOnSubmit(NewJournal);
            db.SubmitChanges();
        }
        #endregion*/

        #region ����Ԃ̓������т��X�V����
        /// <summary>
        /// ����Ԃ̓������т��X�V����
        /// </summary>
        /// <param name="carPurchase">�ԗ��d���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2021/08/16 yano #4001 ����d���̓������т��쐬����Ȃ��B
        /// 2018/07/31 yano.hiroki #3919 ����Ԏd����̎ԑ�ԍ��ύX���̑Ή�
        /// 2017/11/14 arc yano  #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s���� ���ю擾�̃��\�b�h�̕ύX
        /// 2017/10/04 arc yano #3777 2��ڈȍ~�̉���Ԃ��d������Ɠ������т��쐬����Ȃ� ���тɎԑ�ԍ���ێ�����t�B�[���h��ǉ�
        /// 2017/02/14 arc nakayama #3704_�ԗ��d���@�d���v��O�̃f�[�^��ۑ�����Ɠ������т��쐬�����
        /// </history>
        private void UpdateTradeCarJournal(CarPurchase carPurchase, FormCollection form)
        {
            string CustomerClaimCode = new CarSalesOrderDao(db).GetBySlipNumber(carPurchase.SlipNumber).CustomerCode;
            string JournalDate = string.Format("{0:yyyy/MM/dd}", carPurchase.PurchaseDate);

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            //Mod 2021/08/16 yano #4001
            //Mod 2017/10/04 arc yano #3777
            //Mod 2018/07/31 yano.hiroki #3919
            //Mod 2017/11/14 arc yano  #3811
            //����Ԃ̎ԑ�ԍ��̍i���݂̒ǉ�   �����̓������т��擾����Ƃ��́A��{�I�ɂ͎ԗ��}�X�^�̎ԑ�ԍ��ł͂Ȃ��A�ԗ��d���f�[�^�̎ԑ�ԍ��ōi�����邪�A
            //�V�K�쐬���̏ꍇ�ȂǁA�ԗ��d���f�[�^�̎ԑ�ԍ�(CarPurchase.Vin)���󗓂̏ꍇ�͎ԗ��}�X�^����擾����

            string tradevin = !string.IsNullOrWhiteSpace(carPurchase.Vin) ? carPurchase.Vin : carPurchase.SalesCar.Vin;

            Journal JournalData = new JournalDao(db).GetTradeJournal(carPurchase.SlipNumber, "013", tradevin).FirstOrDefault();
            //Journal JournalData = new JournalDao(db).GetTradeJournal(carPurchase.SlipNumber, "013", carPurchase.Vin).FirstOrDefault();
       

            if (JournalData != null && form["update"].Equals("1"))
            {
                JournalData.Amount = carPurchase.TotalAmount ?? 0;
                JournalData.SlipNumber = carPurchase.SlipNumber;
                JournalData.DepartmentCode = carPurchase.DepartmentCode;
                JournalData.CustomerClaimCode = CustomerClaimCode;
                JournalData.JournalDate = DateTime.Parse(JournalDate);
                JournalData.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode, false).OfficeCode;
                JournalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                JournalData.LastUpdateDate = DateTime.Now;
                JournalData.TradeVin = carPurchase.SalesCar.Vin;        //Add2017/10/04 arc yano #3777
            }
            else
            {
                //Add 2017/02/14 arc nakayama #3704
                //�������т��Ȃ��Ă��A�쐬����̂́u�d���v��v�������ꂽ�Ƃ��A�܂��́A�d���ς̂Ƃ�
                if (form["action"].Equals("saveStock") || (carPurchase.PurchaseStatus.Equals("002") && JournalData == null))
                {
                    Journal NewJournal = new Journal();
                    NewJournal.JournalId = Guid.NewGuid();
                    NewJournal.JournalType = "001";
                    NewJournal.DepartmentCode = carPurchase.DepartmentCode;
                    NewJournal.CustomerClaimCode = CustomerClaimCode;
                    NewJournal.SlipNumber = carPurchase.SlipNumber;
                    NewJournal.JournalDate = DateTime.Parse(JournalDate);
                    NewJournal.AccountType = "013";//����
                    NewJournal.AccountCode = carAccount.AccountCode;
                    NewJournal.Amount = carPurchase.TotalAmount ?? 0;
                    NewJournal.Summary = null;
                    NewJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    NewJournal.CreateDate = DateTime.Now;
                    NewJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; ;
                    NewJournal.LastUpdateDate = DateTime.Now;
                    NewJournal.DelFlag = "0";
                    NewJournal.ReceiptPlanFlag = "1";
                    NewJournal.TransferFlag = null;
                    NewJournal.OfficeCode = new DepartmentDao(db).GetByKey(carPurchase.DepartmentCode, false).OfficeCode;
                    NewJournal.CashAccountCode = null;
                    NewJournal.PaymentKindCode = null;
                    NewJournal.CreditReceiptPlanId = null;
                    NewJournal.TradeVin = carPurchase.SalesCar.Vin;        //Add2017/10/04 arc yano #3777

                    db.Journal.InsertOnSubmit(NewJournal);
                }
            }

            db.SubmitChanges();
        }

        /// �����̉���Ԃ̓������т��폜����
        /// </summary>
        /// <param name="prevSlipNumber">�`�[�ԍ�</param>
        /// <param name="prevVin">�ԑ�ԍ�</param>
        /// <history>
        /// 2021/08/09 yano #4086�y�ԗ��d�����́z����Ԃ�ύX�����ۂ̓������у��X�g�폜�R��Ή� �V�K�쐬
        /// </history>
        private void DeleteJournal(string prevSlipNumber, string prevVin)
        {

            List<Journal> JournalList = new List<Journal>();
        
            JournalList = new JournalDao(db).GetListBySlipNumber(prevSlipNumber).Where(x => x.AccountType.Equals(ACCOUNTTYPE_TRADEIN) && x.TradeVin.Equals(prevVin)).ToList();
            
            foreach(Journal rec in JournalList)
            {
                rec.DelFlag = "1";
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now; ;
            }
           
            db.SubmitChanges();
        }

        #endregion

        #region ����Ԃ̓����\����č쐬����B�c������Ύc�̓����\����쐬����
        /// <summary>
        /// ����Ԃ̓����\����č쐬����B�c������Ύc�̓����\����쐬����
        /// </summary>
        /// <param name="carPurchase">�ԗ��d���f�[�^(�o�^���e)</param>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        /// <param name="targetCarPurchase">�ԗ��d���f�[�^</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 ����A����c�̓����\��č쐬�����̕���(�A���A���p�~�̂��߁A�ԗ��`�[�A�ԗ�����̏��͍X�V���Ȃ�)
        //  2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
        /// </history>
        private void CreateTradeReceiptPlan(CarSalesHeader header, CarPurchase targetCarPurchase)
        {
            //Mod 2017/11/14 arc yano #3811
            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }
   
            //---------------------------------------------
            //�����̓����\��̍폜
            //---------------------------------------------
            //�Ώێԗ��̉���A�c�̓����\����폜����
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => (x.ReceiptType == "012" || x.ReceiptType == "013") && !string.IsNullOrWhiteSpace(x.TradeVin) && x.TradeVin.Equals(targetCarPurchase.SalesCar.Vin)).ToList();
            foreach (var d in delList)
            {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //-------------------------------------------
            //�Ώێԗ��̓����\��̍č쐬
            //-------------------------------------------
            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()) && vin.ToString().Equals(targetCarPurchase.SalesCar.Vin))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

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

                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //����̓����z
                    decimal JournalDebtAmount = 0; //�c�̓����z


                    //����̓����z�擾
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //�c�̓����z�擾
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    
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
                    TradePlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

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
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                    //db.SubmitChanges();
                }
            }

            /*
            //�ԗ��`�[�̉�������X�V

            //�[�ԍς݂̎��͎ԗ��`�[�ɔ��f���Ȃ�
            //Add 2017/01/13 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            if (!header.SalesOrderStatus.Equals("004") && !header.SalesOrderStatus.Equals("005"))
            {
                //�����̓����\����폜
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => (x.ReceiptType == "012" || x.ReceiptType == "013")).ToList();
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
                        if (targetCarPurchase.SalesCar.Vin == vin.ToString())
                        {
                            TradeInAmount = targetCarPurchase.TotalAmount.ToString();
                            var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                            if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                            {
                                TradeInRemainDebt = varTradeInRemainDebt.ToString();
                            }

                            if (targetCarPurchase.CarTaxAppropriateAmount != null)
                            {
                                TradeInCarTaxAppropriateAmount = targetCarPurchase.CarTaxAppropriateAmount.ToString();
                            }

                            if (targetCarPurchase.RecycleAmount != null)
                            {
                                TradeInRecycleAmount = targetCarPurchase.RecycleAmount.ToString();
                            }

                            PlanAmount = targetCarPurchase.TotalAmount ?? 0m;
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
                                if (header.TradeInAmount1 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt1 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount1 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax1 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount1 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt1 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation1 = header.TradeInAmount1 ?? 0m - header.TradeInRemainDebt1 ?? 0m;//����ԑ��z(���承�i - ����c���z)
                                //Add 2017/01/10 arc nakayama #3688_�ԗ��d���@���T�C�N�����z���ō������A�������
                                header.TradeInRecycleAmount1 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax1 = decimal.Parse(TradeInCarTaxAppropriateAmount);
                                
                                break;

                            case 2:
                                if (header.TradeInAmount2 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt2 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount2 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax2 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount2 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt2 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation2 = header.TradeInAmount2 ?? 0m - header.TradeInRemainDebt2 ?? 0m;//����ԑ��z(���承�i - ����c���z)                            
                                //Add 2017/01/10 arc nakayama #3688_�ԗ��d���@���T�C�N�����z���ō������A�������
                                header.TradeInRecycleAmount2 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax2 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            case 3:
                                if (header.TradeInAmount3 != decimal.Parse(TradeInAmount) || header.TradeInRemainDebt3 != decimal.Parse(TradeInRemainDebt) || header.TradeInRecycleAmount3 != targetCarPurchase.RecycleAmount || header.TradeInUnexpiredCarTax3 != decimal.Parse(TradeInCarTaxAppropriateAmount))
                                {
                                    editflag = true;
                                }

                                header.TradeInAmount3 = decimal.Parse(TradeInAmount);//���承�i
                                header.TradeInRemainDebt3 = decimal.Parse(TradeInRemainDebt);//����Ԏc��
                                header.TradeInAppropriation3 = header.TradeInAmount3 ?? 0m - header.TradeInRemainDebt3 ?? 0m;//����ԑ��z(���承�i - ����c���z)
                                //Add 2017/01/10 arc nakayama #3688_�ԗ��d���@���T�C�N�����z���ō������A�������
                                header.TradeInRecycleAmount3 = decimal.Parse(TradeInRecycleAmount);
                                header.TradeInUnexpiredCarTax3 = decimal.Parse(TradeInCarTaxAppropriateAmount);

                                break;

                            default:
                                break;
                        }

                        if (editflag)
                        {
                            header.LastEditScreen = LAST_EDIT_PURCHASE;
                            targetCarPurchase.LastEditScreen = "000";
                        }
                        else
                        {
                            targetCarPurchase.LastEditScreen = "000";
                        }

                        //����ԍ��v
                        header.TradeInTotalAmount = header.TradeInAmount1 ?? 0m + header.TradeInAmount2 ?? 0m + header.TradeInAmount3 ?? 0m;
                        //�c���v
                        header.TradeInRemainDebtTotalAmount = header.TradeInRemainDebt1 ?? 0m + header.TradeInRemainDebt2 ?? 0m + header.TradeInRemainDebt3 ?? 0m;
                        //����ԏ[�������z���v
                        header.TradeInAppropriationTotalAmount = header.TradeInAppropriation1 ?? 0m + header.TradeInAppropriation2 ?? 0m + header.TradeInAppropriation3 ?? 0m;


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
            }*/
        }

        #endregion*/

        #region Excel�{�^������
        /// <summary>
        /// Excel�{�^������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>Excel�t�@�C��</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 �^�}�\���P
        /// 2017/03/22 arc nakayama #3730_�d�����X�g
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ExcelDownload(FormCollection form)
        {
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCEL);

            PaginatedList<CarPurchase> list = new PaginatedList<CarPurchase>();     //Mod 2017/04/13 arc yano

            //-------------------------------
            //Excel�o�͏���
            //-------------------------------
            string DownLoadTime = string.Format("{0:yyyyMMdd}", System.DateTime.Now);
            //�t�@�C����:�T�[�r�X�W�v�\_yyyyMMdd.xlsx
            string fileName = DownLoadTime + "_�ԗ��d�����X�g" + ".xlsx";

            //���[�N�t�H���_�擾
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //�e���v���[�g�t�@�C���擾
            string tfilePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CarPurchaseList"]) ? "" : ConfigurationManager.AppSettings["CarPurchaseList"];

            //�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��Ȃ��ꍇ
            if (tfilePath.Equals(""))
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");
                SetDataComponent(form);
                return View("CarPurchaseCriteria", list);
            }

            //�G�N�Z���f�[�^�쐬
            byte[] excelData = MakeExcelData(form, filePathName, tfilePath);

            //Add 2018/06/06 arc yano #3883 �^�}�\���P
            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View("CarPurchaseCriteria", list);
            }

            //�R���e���c�^�C�v�̐ݒ�
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);

        }
        #endregion

        #region �G�N�Z���f�[�^�쐬(�e���v���[�g�t�@�C������)
        /// <summary>
        /// �G�N�Z���f�[�^�쐬(�e���v���[�g�t�@�C������)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="fileName">���[��</param>
        /// <param name="tfileName">���[�e���v���[�g</param>
        /// <returns>�G�N�Z���f�[�^</returns>
        private byte[] MakeExcelData(FormCollection form, string fileName, string tfileName)
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
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //�e���v���[�g�t�@�C�������������ꍇ
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����������܂���ł����B");
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
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
                return excelData;
            }

            //���[�N�V�[�g�I�[�v��
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];

            //----------------------------
            // ���������o��
            //----------------------------
            configLine.SetPos[0] = "A1";

            //����������������쐬
            DataTable dtCondtion = MakeConditionRow(form);

            //�f�[�^�ݒ�
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // �f�[�^�s�o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            configLine = dExport.GetConfigLine(config, 2);

            //�������ʎ擾
            List<CarPurchase> list = GetSearchResultListForExcel(form);

            List<CarPurchaseExcel> elist = new List<CarPurchaseExcel>();

            elist = MakeExcelList(list, form);

            //�f�[�^�ݒ�
            ret = dExport.SetData<CarPurchaseExcel, CarPurchaseExcel>(ref excelFile, elist, configLine);

            excelData = excelFile.GetAsByteArray();

            //���[�N�t�@�C���폜
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
        #endregion

        #region �����������쐬(Excel�o�͗p)
        /// <summary>
        /// �����������쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns name = "dt" >��������</returns>
        private DataTable MakeConditionRow(FormCollection form)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();
            String conditionText = "";

            CodeDao dao = new CodeDao(db);

            //---------------------
            //�@���`
            //---------------------
            dt.Columns.Add("CondisionText", Type.GetType("System.String"));

            //---------------
            //�f�[�^�ݒ�
            //---------------
            DataRow row = dt.NewRow();

            //�Ǘ��ԍ�SalesCarNumber
            if (!string.IsNullOrWhiteSpace(form["SalesCarNumber"]))
            {

                conditionText += "�Ǘ��ԍ�=" + form["SalesCarNumber"];
            }

            //����
            if (!string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                Department dep = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);

                conditionText += "�@����=" + dep.DepartmentName + "(" + dep.DepartmentCode + ")";
            }

            //�d����
            if (!string.IsNullOrWhiteSpace(form["SupplierCode"]))
            {
                Supplier sup = new SupplierDao(db).GetByKey(form["SupplierCode"]);

                conditionText += "�@�d����=" + sup.SupplierName + "(" + sup.SupplierCode + ")";
            }

            //�d���X�e�[�^�X
            if (!string.IsNullOrWhiteSpace(form["PurchaseStatus"]))
            {
                string StatusName = new CodeDao(db).GetCodeNameByKey("023", form["PurchaseStatus"], false).Name;

                conditionText += "�@�d���X�e�[�^�X=" + StatusName;
            }
            //������
            if (!string.IsNullOrWhiteSpace(form["PurchaseOrderDateFrom"]) || !string.IsNullOrWhiteSpace(form["PurchaseOrderDateTo"]))
            {
                conditionText += "�@������=" + form["PurchaseOrderDateFrom"] + "�`" + form["PurchaseOrderDateTo"];
            }
            //�d����
            if (!string.IsNullOrWhiteSpace(form["SlipDateFrom"]) || !string.IsNullOrWhiteSpace(form["SlipDateTo"]))
            {
                conditionText += "�@�d����=" + form["SlipDateFrom"] + "�`" + form["SlipDateTo"];
            }
            //���ɓ�
            if (!string.IsNullOrWhiteSpace(form["PurchaseDateFrom"]) || !string.IsNullOrWhiteSpace(form["PurchaseDateTo"]))
            {
                conditionText += "�@���ɓ�=" + form["PurchaseDateFrom"] + "�`" + form["PurchaseDateTo"];
            }
            //���[�J�[��
            if (!string.IsNullOrWhiteSpace(form["MakerName"]))
            {

                conditionText += "�@���[�J�[��=" + form["MakerName"];
            }

            //�u�����h��
            if (!string.IsNullOrWhiteSpace(form["CarBrandName"]))
            {

                conditionText += "�@�u�����h��=" + form["CarBrandName"];
            }
            //�Ԏ햼
            if (!string.IsNullOrWhiteSpace(form["CarName"]))
            {

                conditionText += "�@�Ԏ햼=" + form["CarName"];
            }
            //�O���[�h��
            if (!string.IsNullOrWhiteSpace(form["CarGradeName"]))
            {

                conditionText += "�O���[�h��=" + form["CarGradeName"];
            }
            //�ԑ�ԍ�
            if (!string.IsNullOrWhiteSpace(form["Vin"]))
            {

                conditionText += "�@�ԑ�ԍ�=" + form["Vin"];
            }

            //�쐬�����e�L�X�g���J�����ɐݒ�
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);

            return dt;
        }
        #endregion

        #region �������ʂ�Excel�p�ɐ��`
        /// <summary>
        /// �������ʂ�Excel�p�ɐ��`
        /// </summary>
        /// <param name="list">��������</param>
        /// <returns name="elist">��������(Exel�o�͗p)</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 �^�}�\���P ���ɋ敪��̒ǉ�
        /// </history>
        private List<CarPurchaseExcel> MakeExcelList(List<CarPurchase> list, FormCollection form)
        {
            List<CarPurchaseExcel> elist = new List<CarPurchaseExcel>();

            foreach (var a in list)
            {
                CarPurchaseExcel PurchaseExcel = new CarPurchaseExcel();
                //���ɋ敪
                PurchaseExcel.CarPurchaseTypeName = (a.c_CarPurchaseType != null ? a.c_CarPurchaseType.Name : "");              //Add 2018/06/06 arc yano #3883
                //�Ǘ��ԍ�
                PurchaseExcel.SalesCarNumber = a.SalesCarNumber;
                //����
                if(a.Department != null){
                    PurchaseExcel.DepartmentName = a.Department.DepartmentName;
                }else{
                    PurchaseExcel.DepartmentName = "";
                }
                //�V���敪
                if(a.SalesCar != null && a.SalesCar.c_NewUsedType != null){
                    PurchaseExcel.NewUsedTypeName = a.SalesCar.c_NewUsedType.Name;
                }
                //�d����
                if(a.Supplier != null){
                    PurchaseExcel.SupplierName = a.Supplier.SupplierName;
                }
                //�d���S��
                if(a.Employee != null){
                    PurchaseExcel.PurchaseEmployeeName = a.Employee.EmployeeName;
                }
                //�d�����P�[�V����
                if(a.Location != null){
                    PurchaseExcel.LocationName = a.Location.LocationName;
                }
                //������
                if(a.CarPurchaseOrder != null){
                    PurchaseExcel.PurchaseOrderDate = a.CarPurchaseOrder.PurchaseOrderDate;
                }
                //�d����
                PurchaseExcel.SlipDate = a.SlipDate;
                //���ɓ�
                PurchaseExcel.PurchaseDate = a.PurchaseDate;
                //���[�J�[
                if (a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null && a.SalesCar.CarGrade.Car.Brand != null && a.SalesCar.CarGrade.Car.Brand.Maker != null)
                {
                    PurchaseExcel.MakerName = a.SalesCar.CarGrade.Car.Brand.Maker.MakerName;
                }
                //�u�����h
                if(a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null && a.SalesCar.CarGrade.Car.Brand != null){
                   PurchaseExcel.CarBrandName = a.SalesCar.CarGrade.Car.Brand.CarBrandName;
                }
                //�Ԏ�
                if (a.SalesCar != null && a.SalesCar.CarGrade != null && a.SalesCar.CarGrade.Car != null)
                {
                    PurchaseExcel.CarName = a.SalesCar.CarGrade.Car.CarName;
                }
                //�O���[�h
                if (a.SalesCar != null && a.SalesCar.CarGrade != null)
                {
                    PurchaseExcel.CarGradeName = a.SalesCar.CarGrade.CarGradeName;
                }
                //�V���敪
                if (a.SalesCar != null)
                {
                    PurchaseExcel.Vin = a.SalesCar.Vin;
                }
                //�ԗ��{�̉��i-�Ŕ�
                PurchaseExcel.VehiclePrice = a.VehiclePrice;
                //�ԗ��{�̉��i-�����
                PurchaseExcel.VehicleTax = a.VehicleTax;
                //�ԗ��{�̉��i-�ō�
                PurchaseExcel.VehicleAmount = a.VehicleAmount;
                //���D��-�Ŕ�
                PurchaseExcel.AuctionFeePrice = a.AuctionFeePrice;
                //���D��-�����
                PurchaseExcel.AuctionFeeTax = a.AuctionFeeTax;
                //���D��-�ō�
                PurchaseExcel.AuctionFeeAmount = a.AuctionFeeAmount;
                //���T�C�N��
                PurchaseExcel.RecycleAmount = a.RecycleAmount;
                //���ŏ[��-�Ŕ�
                PurchaseExcel.CarTaxAppropriatePrice = a.CarTaxAppropriatePrice;
                //���ŏ[��-�����
                PurchaseExcel.CarTaxAppropriateTax = a.CarTaxAppropriateTax;
                //���ŏ[��-�ō�
                PurchaseExcel.CarTaxAppropriateAmount = a.CarTaxAppropriateAmount;
                //�d�����z-�Ŕ�
                PurchaseExcel.Amount = a.Amount;
                //�d�����z-�����
                PurchaseExcel.TaxAmount = a.TaxAmount;
                //�d�����z-�ō�
                PurchaseExcel.TotalAmount = a.TotalAmount;
                if (a.Employee1 != null && !string.IsNullOrEmpty(a.Employee1.EmployeeName))
                {
                    PurchaseExcel.LastUpdateEmployeeName = a.Employee1.EmployeeName;
                }
                else
                {
                    PurchaseExcel.LastUpdateEmployeeName = "�V�X�e���X�V";
                }

                elist.Add(PurchaseExcel);
            }

            return elist;

        }
        #endregion

        #region �������� Excel�p
        /// <summary>
        /// �ԗ��d���e�[�u���������ʃ��X�g�擾 Excel�p
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��d���e�[�u���������ʃ��X�g</returns>
        private List<CarPurchase> GetSearchResultListForExcel(FormCollection form)
        {

            CarPurchaseDao carPurchaseDao = new CarPurchaseDao(db);
            CarPurchase carPurchaseCondition = new CarPurchase();
            carPurchaseCondition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.SlipDateFrom = CommonUtils.StrToDateTime(form["SlipDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.SlipDateTo = CommonUtils.StrToDateTime(form["SlipDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseDateFrom = CommonUtils.StrToDateTime(form["PurchaseDateFrom"], DaoConst.SQL_DATETIME_MAX);
            carPurchaseCondition.PurchaseDateTo = CommonUtils.StrToDateTime(form["PurchaseDateTo"], DaoConst.SQL_DATETIME_MIN);
            carPurchaseCondition.PurchaseStatus = form["PurchaseStatus"];
            carPurchaseCondition.Department = new Department();
            carPurchaseCondition.Department.DepartmentCode = form["DepartmentCode"];
            carPurchaseCondition.Supplier = new Supplier();
            carPurchaseCondition.Supplier.SupplierCode = form["SupplierCode"];
            carPurchaseCondition.SalesCar = new SalesCar();
            carPurchaseCondition.SalesCar.SalesCarNumber = form["SalesCarNumber"];
            carPurchaseCondition.SalesCar.Vin = form["Vin"];
            carPurchaseCondition.SalesCar.CarGrade = new CarGrade();
            carPurchaseCondition.SalesCar.CarGrade.Car = new Car();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand = new Brand();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker = new Maker();
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName = form["MakerName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            carPurchaseCondition.SalesCar.CarGrade.Car.CarName = form["CarName"];
            carPurchaseCondition.SalesCar.CarGrade.CarGradeName = form["CarGradeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carPurchaseCondition.DelFlag = form["DelFlag"];
            }
            return carPurchaseDao.GetListByConditionForExcel(carPurchaseCondition);
        }

        /// <summary>
        /// ���������ǂ������擾����B(Ajax��p�j
        /// </summary>
        /// <param name="processType">�������</param>
        /// <returns>��������</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();

                ret.Add("ProcessedTime", "��������");

                return Json(ret);
            }
            return new EmptyResult();
        }
        #endregion

        //Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        #region Excel�捞����
        /// <summary>
        /// Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<CarPurchaseExcelImportList> ImportList = new List<CarPurchaseExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("CarPurchaseImportDialog", ImportList);
        }

        /// <summary>
        /// Excel�ǂݍ���
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase ImportFile, List<CarPurchaseExcelImportList> ImportLine, FormCollection form)
        {
            List<CarPurchaseExcelImportList> ImportList = new List<CarPurchaseExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel�ǂݍ���
                //--------------
                case "1":
                    //Excel�ǂݍ��ݑO�̃`�F�b�N
                    ValidateImportFile(ImportFile);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    //Excel�ǂݍ���
                    ImportList = ReadExcelData(ImportFile, ImportList);

                    //�ǂݍ��ݎ��ɉ����G���[������΂����Ń��^�[��
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    //Excel�œǂݍ��񂾃f�[�^�̃o���f�[�g�`�F�b�N
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("CarPurchaseImportDialog", ImportList);
                    }

                    form["ErrFlag"] = "0";
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);

                //--------------
                //Excel��荞��
                //--------------
                case "2":

                    DBExecute(ImportLine, form);
                    form["ErrFlag"] = "1"; //��荞�񂾌�ɍēx[��荞��]�{�^���������Ȃ��悤�ɂ��邽��
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);
                //--------------
                //�L�����Z��
                //--------------
                case "3":

                    ImportList = new List<CarPurchaseExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[��荞��]�{�^���������Ȃ��悤�ɂ��邽��

                    return View("CarPurchaseImportDialog", ImportList);

                //----------------------------------
                //���̑�(�����ɓ��B���邱�Ƃ͂Ȃ�)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("CarPurchaseImportDialog", ImportList);
            }
        }
        #endregion

        #region �捞�t�@�C�����݃`�F�b�N
        /// <summary>
        /// �捞�t�@�C�����݃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        /// <returns></returns>
        private void ValidateImportFile(HttpPostedFileBase filePath)
        {
            // �K�{�`�F�b�N
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "�t�@�C����I�����Ă�������"));
            }
            else
            {
                // �g���q�`�F�b�N
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0 )
                {

                    if (stExtension.IndexOf("xlsm") < 0)
                {
                        ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "�t�@�C���̊g���q��xlsm�t�@�C���ł͂���܂���"));

                    }
                }
            }

            return;
        }
        #endregion

        #region �_�C�A���O�̃f�[�^�t���R���|�[�l���g�ݒ�
        /// �_�C�A���O�̃f�[�^�t���R���|�[�l���g�ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
        }
        #endregion

        #region Excel�f�[�^�擾&�ݒ�
        /// Excel�f�[�^�擾&�ݒ�
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        private List<CarPurchaseExcelImportList> ReadExcelData(HttpPostedFileBase ImportFile, List<CarPurchaseExcelImportList> ImportList)
        {
            //�J�����ԍ��ۑ��p
            int[] colNumber;
            colNumber = new int[70] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(ImportFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("ImportFile", "�Ώۂ̃t�@�C�����J����Ă��܂��B�t�@�C������Ă���A�ēx���s���ĉ�����");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImportFile", "�G���[���������܂����B" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // �f�[�^�V�[�g�擾
                //-----------------------------
                var ws = pck.Workbook.Worksheets["CarImportList"];

                //--------------------------------------
                //�ǂݍ��ރV�[�g�����݂��Ȃ������ꍇ
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "Excel�Ƀf�[�^������܂���B�V�[�g�����m�F���čēx���s���ĉ�����"));
                    return ImportList;
                }

                //------------------------------
                //�ǂݍ��ݍs��0���̏ꍇ
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "Excel�Ƀf�[�^������܂���B�X�V�������I�����܂���"));
                    return ImportList;
                }

                //�ǂݎ��̊J�n�ʒu�ƏI���ʒu���擾
                int StartRow = ws.Dimension.Start.Row + 2 ; //�s�̊J�n�ʒu
                int EndRow = ws.Dimension.End.Row;          //�s�̏I���ʒu
                int StartCol = ws.Dimension.Start.Column;   //��̊J�n�ʒu
                int EndCol = ws.Dimension.End.Column;       //��̏I���ʒu

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];
                colNumber = SetColNumber(headerRow, colNumber);
                //�^�C�g���s�A�w�b�_�s�����������ꍇ�͑����^�[������
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // �ǂݎ�菈��
                //------------------------------
                int datarow = 0;
                string[] Result = new string[colNumber.Count()]; 

                for (datarow = StartRow + 2; datarow < EndRow + 1; datarow++)
                {
                    CarPurchaseExcelImportList data = new CarPurchaseExcelImportList();

                    //�X�V�f�[�^�̎擾
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {

                        for (int i = 0; i < colNumber.Count(); i++)
                        {

                            if (col == colNumber[i])
                            {
                                Result[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }

                    //----------------------------------------
                    // �ǂݎ�茋�ʂ���ʂ̍��ڂɃZ�b�g����
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region �e���ڂ̗�ԍ��ݒ�
        /// <summary>
        /// �e���ڂ̗�ԍ��ݒ�
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //��������
            int cnt = 1;

            //��ԍ��ݒ�
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //�Ǘ��ԍ�("A"�Ŏ���)
                    if (cell.Text.Equals("SalesCarNumber"))
                    {
                        colNumber[0] = cnt;
                    }
                    //�ԑ�ԍ�
                    if (cell.Text.Equals("Vin"))
                    {
                        colNumber[1] = cnt;
                    }
                    
                    //VIN(�V���A��)
                    if (cell.Text.Equals("UsVin"))
                    {
                        colNumber[2] = cnt;
                    }
                    
                    //���[�J�[��
                    if (cell.Text.Equals("MakerName"))
                    {
                        colNumber[3] = cnt;
                    }
                    
                    //�ԗ��O���[�h�R�[�h
                    if (cell.Text.Equals("CarGradeCode"))
                    {
                        colNumber[4] = cnt;
                    }
                    
                    //�V���敪("N"or"U")
                    if (cell.Text.Equals("NewUsedType"))
                    {
                        colNumber[5] = cnt;
                    }

                    //�n���F
                    if (cell.Text.Equals("ColorType"))
                    {
                        colNumber[6] = cnt;
                    }
                    
                    //�O���F�R�[�h
                    if (cell.Text.Equals("ExteriorColorCode"))
                    {
                        colNumber[7] = cnt;
                    }
                    
                    //�O���F��
                    if (cell.Text.Equals("ExteriorColorName"))
                    {
                        colNumber[8] = cnt;
                    }
                    
                    //�����F�R�[�h
                    if (cell.Text.Equals("InteriorColorCode"))
                    {
                        colNumber[9] = cnt;
                    }
                    
                    //�����F��
                    if (cell.Text.Equals("InteriorColorName"))
                    {
                        colNumber[10] = cnt;
                    }
                    
                    //�N��
                    if (cell.Text.Equals("ManufacturingYear"))
                    {
                        colNumber[11] = cnt;
                    }
                    
                    //�n���h��
                    if (cell.Text.Equals("Steering"))
                    {
                        colNumber[12] = cnt;
                    }
                    
                    //�̔����i(�Ŕ��j
                    if (cell.Text.Equals("SalesPrice"))
                    {
                        colNumber[13] = cnt;
                    }
                    
                    //�^��
                    if (cell.Text.Equals("ModelName"))
                    {
                        colNumber[14] = cnt;
                    }
                    
                    //�����@�^��
                    if (cell.Text.Equals("EngineType"))
                    {
                        colNumber[15] = cnt;
                    }
                    //�r�C��
                    if (cell.Text.Equals("Displacement"))
                    {
                        colNumber[16] = cnt;
                    }
                    //�^���w��ԍ�
                    if (cell.Text.Equals("ModelSpecificateNumber"))
                    {
                        colNumber[17] = cnt;
                    }
                    
                    //�ޕʋ敪�ԍ�
                    if (cell.Text.Equals("ClassificationTypeNumber"))
                    {
                        colNumber[18] = cnt;
                    }
                    
                    //���l�P
                    if (cell.Text.Equals("Memo1"))
                    {
                        colNumber[19] = cnt;
                    }
                    
                    //���l�Q
                    if (cell.Text.Equals("Memo2"))
                    {
                        colNumber[20] = cnt;
                    }
                    
                    //���l�R
                    if (cell.Text.Equals("Memo3"))
                    {
                        colNumber[21] = cnt;
                    }
                    
                    //���l�S
                    if (cell.Text.Equals("Memo4"))
                    {
                        colNumber[22] = cnt;
                    }
                    
                    //���l�T
                    if (cell.Text.Equals("Memo5"))
                    {
                        colNumber[23] = cnt;
                    }
                    
                    //���l�U
                    if (cell.Text.Equals("Memo6"))
                    {
                        colNumber[24] = cnt;
                    }
                    
                    //���l�V
                    if (cell.Text.Equals("Memo7"))
                    {
                        colNumber[25] = cnt;
                    }
                    
                    //���l�W
                    if (cell.Text.Equals("Memo8"))
                    {
                        colNumber[26] = cnt;
                    }
                    
                    //���l�X
                    if (cell.Text.Equals("Memo9"))
                    {
                        colNumber[27] = cnt;
                    }
                    
                    //���l�P�O
                    if (cell.Text.Equals("Memo10"))
                    {
                        colNumber[28] = cnt;
                    }
                    
                    //�����Ԑ�
                    if (cell.Text.Equals("CarTax"))
                    {
                        colNumber[29] = cnt;
                    }
                    
                    //�����ԏd�ʐ�
                    if (cell.Text.Equals("CarWeightTax"))
                    {
                        colNumber[30] = cnt;
                    }
                    
                    //�����ӕی���
                    if (cell.Text.Equals("CarLiabilityInsurance"))
                    {
                        colNumber[31] = cnt;
                    }
                    
                    //�����ԐŊ����\��
                    if (cell.Text.Equals("AcquisitionTax"))
                    {
                        colNumber[32] = cnt;
                    }
                    
                    //���T�C�N���a����
                    if (cell.Text.Equals("RecycleDeposit"))
                    {
                        colNumber[33] = cnt;
                    }
                    
                    //�F�蒆�Î�No
                    if (cell.Text.Equals("ApprovedCarNumber"))
                    {
                        colNumber[34] = cnt;
                    }
                    
                    //�F�蒆�Îԕۏ؊���FROM
                    if (cell.Text.Equals("ApprovedCarWarrantyDateFrom"))
                    {
                        colNumber[35] = cnt;
                    }

                    //�F�蒆�Îԕۏ؊���TO
                    if (cell.Text.Equals("ApprovedCarWarrantyDateTo"))
                    {
                        colNumber[36] = cnt;
                    }
                    
                    //�d����
                    if (cell.Text.Equals("SlipDate"))
                    {
                        colNumber[37] = cnt;
                    }
                    
                    //���ɗ\���
                    if (cell.Text.Equals("PurchaseDate"))
                    {
                        colNumber[38] = cnt;
                    }
                    
                    //�d����R�[�h
                    if (cell.Text.Equals("SupplierCode"))
                    {
                        colNumber[39] = cnt;
                    }
                    
                    //�d�����P�[�V�����R�[�h
                    if (cell.Text.Equals("PurchaseLocationCode"))
                    {
                        colNumber[40] = cnt;
                    }
                    
                    //�ԗ��{�̉��i
                    if (cell.Text.Equals("VehiclePrice"))
                    {
                        colNumber[41] = cnt;
                    }
                    
                    //�ԗ��{�̏����
                    if (cell.Text.Equals("VehicleTax"))
                    {
                        colNumber[42] = cnt;
                    }
                    
                    //�ԗ��{�̐ō����i
                    if (cell.Text.Equals("VehicleAmount"))
                    {
                        colNumber[43] = cnt;
                    }
                    
                    //�I�v�V�������i
                    if (cell.Text.Equals("OptionPrice"))
                    {
                        colNumber[44] = cnt;
                    }
                    
                    //�I�v�V���������
                    if (cell.Text.Equals("OptionTax"))
                    {
                        colNumber[45] = cnt;
                    }
                    
                    //�I�v�V�����ō����i
                    if (cell.Text.Equals("OptionAmount"))
                    {
                        colNumber[46] = cnt;
                    }
                    
                    //�f�B�X�J�E���g���i
                    if (cell.Text.Equals("DiscountPrice"))
                    {
                        colNumber[47] = cnt;
                    }
                    
                    //�f�B�X�J�E���g�����
                    if (cell.Text.Equals("DiscountTax"))
                    {
                        colNumber[48] = cnt;
                    }
                    
                    //�f�B�X�J�E���g�ō����i
                    if (cell.Text.Equals("DiscountAmount"))
                    {
                        colNumber[49] = cnt;
                    }
                    
                    //�t�@�[�����i
                    if (cell.Text.Equals("FirmPrice"))
                    {
                        colNumber[50] = cnt;
                    }
                    
                    //�t�@�[�������
                    if (cell.Text.Equals("FirmTax"))
                    {
                        colNumber[51] = cnt;
                    }
                    
                    //�t�@�[���ō����i
                    if (cell.Text.Equals("FirmAmount"))
                    {
                        colNumber[52] = cnt;
                    }
                    
                    //���^���b�N���i
                    if (cell.Text.Equals("MetallicPrice"))
                    {
                        colNumber[53] = cnt;
                    }
                    
                    //���^���b�N�����
                    if (cell.Text.Equals("MetallicTax"))
                    {
                        colNumber[54] = cnt;
                    }
                    
                    //���^���b�N�ō����i
                    if (cell.Text.Equals("MetallicAmount"))
                    {
                        colNumber[55] = cnt;
                    }
                    
                    //�������i
                    if (cell.Text.Equals("EquipmentPrice"))
                    {
                        colNumber[56] = cnt;
                    }
                    
                    //���C���i
                    if (cell.Text.Equals("RepairPrice"))
                    {
                        colNumber[57] = cnt;
                    }
                    
                    //���̑����i
                    if (cell.Text.Equals("OthersPrice"))
                    {
                        colNumber[58] = cnt;
                    }
                    
                    //���̑������
                    if (cell.Text.Equals("OthersTax"))
                    {
                        colNumber[59] = cnt;
                    }
                    
                    //���̑��ō����i
                    if (cell.Text.Equals("OthersAmount"))
                    {
                        colNumber[60] = cnt;
                    }
                    
                    //���ŏ[��
                    if (cell.Text.Equals("CarTaxAppropriatePrice"))
                    {
                        colNumber[61] = cnt;
                    }
                    
                    //���T�C�N��
                    if (cell.Text.Equals("RecyclePrice"))
                    {
                        colNumber[62] = cnt;
                    }
                    
                    //�I�[�N�V�������D��
                    if (cell.Text.Equals("AuctionFeePrice"))
                    {
                        colNumber[63] = cnt;
                    }
                    
                    //�I�[�N�V�������D�������
                    if (cell.Text.Equals("AuctionFeeTax"))
                    {
                        colNumber[64] = cnt;
                    }
                    
                    //�I�[�N�V�������D���ō�
                    if (cell.Text.Equals("AuctionFeeAmount"))
                    {
                        colNumber[65] = cnt;
                    }
                    
                    //�d�����i
                    if (cell.Text.Equals("Amount"))
                    {
                        colNumber[66] = cnt;
                    }
                    
                    //�����
                    if (cell.Text.Equals("TaxAmount"))
                    {
                        colNumber[67] = cnt;
                    }
                    
                    //�d���ō����i
                    if (cell.Text.Equals("TotalAmount"))
                    {
                        colNumber[68] = cnt;
                    }
                    
                    //���l
                    if (cell.Text.Equals("Memo"))
                    {
                        colNumber[69] = cnt;
                    }

                }
                cnt++;
            }

            for (int i = 0; i < colNumber.Length; i++)
            {
                if (colNumber[i] == -1)
                {
                    ModelState.AddModelError("ImportFile", "�w�b�_�s������������܂���B");
                    break;
                }
            }


            return colNumber;
        }
        #endregion

        #region Excel�̓ǂݎ�茋�ʂ����X�g�ɐݒ肷��
        /// <summary>
        /// ���ʂ����X�g�ɐݒ肷��
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/12/01 arc nakayama #3663_�y�����z�ԗ��d���@Excel�捞�Ή�
        /// </history>
        public List<CarPurchaseExcelImportList> SetProperty(ref string[] Result, ref List<CarPurchaseExcelImportList> ImportList)
        {
            CarPurchaseExcelImportList SetLine = new CarPurchaseExcelImportList();

            //�Ǘ��ԍ��������Ă��Ȃ���΂����ŃZ�b�g����̂���߂�
            if (string.IsNullOrEmpty(Result[0].Trim()))
            {
                return ImportList;
            }

            //�Ǘ��ԍ�
            SetLine.SalesCarNumber = Result[0].Trim();

            //�ԑ�ԍ�
            SetLine.Vin = Result[1].Trim();
            
            //VIN(�V���A��)
            SetLine.UsVin = Result[2].Trim();
            
            //���[�J�[��
            SetLine.MakerName = Result[3].Trim();
            
            //�ԗ��O���[�h�R�[�h
            SetLine.CarGradeCode = Result[4].Trim();
            
            //�V���敪
            SetLine.NewUsedType = Result[5].Trim();
            
            //�n���F
            SetLine.ColorType = Result[6].Trim();
            
            //�O���F�R�[�h
            SetLine.ExteriorColorCode = Result[7].Trim();
            
            //�O���F��
            SetLine.ExteriorColorName = Result[8].Trim();
            
            //�����F�R�[�h
            SetLine.InteriorColorCode = Result[9].Trim();
            
            //�����F��
            SetLine.InteriorColorName = Result[10].Trim();
            
            //�N��
            SetLine.ManufacturingYear = Result[11].Trim();
            
            //�n���h��
            SetLine.Steering = Result[12].Trim();
            
            //�̔����i(�Ŕ��j
            SetLine.SalesPrice = Result[13].Trim();
            
            //�^��
            SetLine.ModelName = Result[14].Trim();
            
            //�����@�^��
            SetLine.EngineType = Result[15].Trim();
            
            //�r�C��
            SetLine.Displacement = Result[16].Trim();
            
            //�^���w��ԍ�
            SetLine.ModelSpecificateNumber = Result[17].Trim();
            
            //�ޕʋ敪�ԍ�
            SetLine.ClassificationTypeNumber = Result[18].Trim();
            
            //���l�P
            SetLine.Memo1 = Result[19].Trim();
            
            //���l�Q
            SetLine.Memo2 = Result[20].Trim();
            
            //���l�R
            SetLine.Memo3 = Result[21].Trim();
            
            //���l�S
            SetLine.Memo4 = Result[22].Trim();
            
            //���l�T
            SetLine.Memo5 = Result[23].Trim();
            
            //���l�U
            SetLine.Memo6 = Result[24].Trim();
            
            //���l�V
            SetLine.Memo7 = Result[25].Trim();
            
            //���l�W
            SetLine.Memo8 = Result[26].Trim();
            
            //���l�X
            SetLine.Memo9 = Result[27].Trim();
            
            //���l�P�O
            SetLine.Memo10 = Result[28].Trim();
            
            //�����Ԑ�
            SetLine.CarTax = Result[29].Trim();
            
            //�����ԏd�ʐ�
            SetLine.CarWeightTax = Result[30].Trim();
            
            //�����ӕی���
            SetLine.CarLiabilityInsurance = Result[31].Trim();
            
            //�����ԐŊ����\��
            SetLine.AcquisitionTax = Result[32].Trim();
            
            //���T�C�N���a����
            SetLine.RecycleDeposit = Result[33].Trim();
            
            //�F�蒆�Î�No
            SetLine.ApprovedCarNumber = Result[34].Trim();
            
            //�F�蒆�Îԕۏ؊���FROM
            SetLine.ApprovedCarWarrantyDateFrom = Result[35].Trim();
            
            //�F�蒆�Îԕۏ؊���TO
            SetLine.ApprovedCarWarrantyDateTo = Result[36].Trim();
            
            //�d����(�����͂Ȃ瓖�����t������)
            if (string.IsNullOrEmpty(Result[37]))
            {
                SetLine.SlipDate = string.Format("{0:yyyy/MM/dd}", DateTime.Now);
            }
            else
            {
                SetLine.SlipDate = Result[37].Trim();
            }
            //���ɗ\���
            SetLine.PurchaseDate = Result[38].Trim();
            
            //�d����R�[�h
            SetLine.SupplierCode = Result[39].Trim();
            
            //�d�����P�[�V�����R�[�h
            SetLine.PurchaseLocationCode = Result[40].Trim();
            
            //�ԗ��{�̉��i
            SetLine.VehiclePrice = Result[41].Trim();
            
            //�ԗ��{�̏����
            SetLine.VehicleTax = Result[42].Trim();
            
            //�ԗ��{�̐ō����i
            SetLine.VehicleAmount = Result[43].Trim();
            
            //�I�v�V�������i
            SetLine.OptionPrice = Result[44].Trim();
            
            //�I�v�V���������
            SetLine.OptionTax = Result[45].Trim();
            
            //�I�v�V�����ō����i
            SetLine.OptionAmount = Result[46].Trim();
            
            //�f�B�X�J�E���g���i
            SetLine.DiscountPrice = Result[47].Trim();
            
            //�f�B�X�J�E���g�����
            SetLine.DiscountTax = Result[48].Trim();
            
            //�f�B�X�J�E���g�ō����i
            SetLine.DiscountAmount = Result[49].Trim();
            
            //�t�@�[�����i
            SetLine.FirmPrice = Result[50].Trim();
            
            //�t�@�[�������
            SetLine.FirmTax = Result[51].Trim();
            
            //�t�@�[���ō����i
            SetLine.FirmAmount = Result[52].Trim();
            
            //���^���b�N���i
            SetLine.MetallicPrice = Result[53].Trim();
            
            //���^���b�N�����
            SetLine.MetallicTax = Result[54].Trim();
            
            //���^���b�N�ō����i
            SetLine.MetallicAmount = Result[55].Trim();
            
            //�������i
            SetLine.EquipmentPrice = Result[56].Trim();
            
            //���C���i
            SetLine.RepairPrice = Result[57].Trim();
            
            //���̑����i
            SetLine.OthersPrice = Result[58].Trim();
            
            //���̑������
            SetLine.OthersTax = Result[59].Trim();
            
            //���̑��ō����i
            SetLine.OthersAmount = Result[60].Trim();
            
            //���ŏ[��
            SetLine.CarTaxAppropriatePrice = Result[61].Trim();
            
            //���T�C�N��
            SetLine.RecyclePrice = Result[62].Trim();
            
            //�I�[�N�V�������D��
            SetLine.AuctionFeePrice = Result[63].Trim();
            
            //�I�[�N�V�������D�������
            SetLine.AuctionFeeTax = Result[64].Trim();
            
            //�I�[�N�V�������D���ō�
            SetLine.AuctionFeeAmount = Result[65].Trim();
            
            //�d�����i
            SetLine.Amount = Result[66].Trim();
            
            //�����
            SetLine.TaxAmount = Result[67].Trim();
            
            //�d���ō����i
            SetLine.TotalAmount = Result[68].Trim();
            
            //���l
            SetLine.Memo = Result[69].Trim();

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// <summary>
        ///  �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2020/11/27 yano #4072 �����@�^�����̓G���A�̊g�� �`�F�b�N�����̔��蕶������10 -> 25�ɕύX
        /// 2018/02/15 arc yano #3865 �ԗ��Ԃ����݁@�ԗ��}�X�^�d���G���[�s�
        /// 2017/11/11/ arc yano #3825 �ԗ��ꊇ�捞�d�l�ύX �F�n���A�O���F�A�����F�̃}�X�^�`�F�b�N�̔p�~
        /// </history>
        public void ValidateImportList(List<CarPurchaseExcelImportList> ImportList)
        {
            for (int i = 0; i < ImportList.Count; i++)
            {
                /*----------------*/
                /* ���K�{�`�F�b�N */
                /*----------------*/

                //�Ǘ��ԍ�
                if (string.IsNullOrEmpty(ImportList[i].SalesCarNumber))
                {
                    //ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̊Ǘ��ԍ������͂���Ă��܂���B�Ǘ��ԍ�"));
                    return;
                }

                //�ԑ�ԍ�
                if (string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Vin", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̎ԑ�ԍ������͂���Ă��܂���B�ԑ�ԍ�"));
                }

                //�V���敪
                if (string.IsNullOrEmpty(ImportList[i].NewUsedType))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].NewUsedType", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̐V���敪�����͂���Ă��܂���B�V���敪"));
                }

                //�O���[�h�R�[�h
                if (string.IsNullOrEmpty(ImportList[i].CarGradeCode))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̃O���[�h�R�[�h�����͂���Ă��܂���B�O���[�h�R�[�h"));
                }
                else
                {
                    CarGrade CGData = new CarGradeDao(db).GetByKey(ImportList[i].CarGradeCode);
                    if (CGData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", i + 1 + "�s�ڂ̎ԗ��O���[�h�R�[�h" + ImportList[i].CarGradeCode + "�͎ԗ��O���[�h�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                    }
                }

                /*------------------*/
                /* ���}�X�^�`�F�b�N */
                /*------------------*/

                /* //Mod 2017/11/11/ arc yano �n���F
                //�n���F�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].ColorType))
                {
                    c_ColorCategory ColorCategory = new CodeDao(db).GetColorCategoryByKey(ImportList[i].ColorType, false);
                    if (ColorCategory == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ColorTypeName", i + 1 + "�s�ڂ̌n���F" + ImportList[i].ColorType + "�̓}�X�^�ɓo�^����Ă��܂���B");
                    }
                }

                //�O���F�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorCode))
                {
                    CarColor CarColorData = new CarColorDao(db).GetByKey(ImportList[i].ExteriorColorCode, false);
                    if (CarColorData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorCode", i + 1 + "�s�ڂ̊O���F�R�[�h" + ImportList[i].ExteriorColorCode + "�͎ԗ��J���[�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                    }
                }

                //�����F�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorCode))
                {
                    CarColor CarColorData = new CarColorDao(db).GetByKey(ImportList[i].InteriorColorCode, false);
                    if (CarColorData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorCode", i + 1 + "�s�ڂ̓����F�R�[�h" + ImportList[i].InteriorColorCode + "�͎ԗ��J���[�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                    }
                }
                */

                //�d����R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].SupplierCode))
                {
                    Supplier SupplierData = new SupplierDao(db).GetByKey(ImportList[i].SupplierCode, false);
                    if (SupplierData == null)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SupplierCode", i + 1 + "�s�ڂ̎d����R�[�h" + ImportList[i].SupplierCode + "�͎d����}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                    }
                }

                //�d���惍�P�[�V����
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseLocationCode))
                {
                    Location LocationData = new LocationDao(db).GetByKey(ImportList[i].PurchaseLocationCode, false);
                    if (LocationData == null)
                    {
                        //Mod 2017/07/24 arc nakayama 3780: �y�ԗ��ꊇ�d����z���b�Z�[�W�ԈႢ�C��
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseLocationCode", i + 1 + "�s�ڂ̎d���惍�P�[�V�����R�[�h" + ImportList[i].PurchaseLocationCode + "�̓��P�[�V�����}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                    }
                }

                /*------------------*/
                /* ���d���`�F�b�N   */
                /*------------------*/

                //�Ǘ��ԍ��iDB���ɏd�����Ȃ����j
                //Mod 2018/02/15 arc yano #3865
                //�폜�f�[�^���݂Ō���
                SalesCar SalesCarData = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, true);
                //SalesCar SalesCarData = new SalesCarDao(db).GetByKey(ImportList[i].SalesCarNumber, false);
                if (SalesCarData != null)
                {
                    if (!string.IsNullOrWhiteSpace(SalesCarData.DelFlag) && SalesCarData.DelFlag.Equals("1"))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "�s�ڂ̊Ǘ��ԍ�" + ImportList[i].SalesCarNumber + "�͖����f�[�^�����݂��Ă��܂��B�V�X�e���ۂɕ����폜�˗������ĉ������B");   
                    }
                    else
                    {
                    ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "�s�ڂ̊Ǘ��ԍ�" + ImportList[i].SalesCarNumber + "�͊��ɑ��݂��Ă��܂��B�ԗ��}�X�^���m�F���Ă��������B");
                }
                }

                //�Ǘ��ԍ��iExcel���ɏd�����Ȃ����j
                var ret = from a in ImportList
                          where ImportList[i].SalesCarNumber.Equals(a.SalesCarNumber)
                          && !a.SalesCarNumber.Equals("A")
                          select a;
                
                if (ret.Count() > 1 )
                {
                    ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "�s�ڂ̊Ǘ��ԍ�" + ImportList[i].SalesCarNumber + "�͓���Excel����2�ȏ㑶�݂��Ă��܂��B�P�ɂ��ĉ������B");
                }

                //�ԑ�ԍ��@�݌ɃX�e�[�^�X���u�[�ԍρv�ȊO��������G���[
                if(!string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    SalesCar SalesCarCheck = new SalesCarDao(db).GetDataByVin(ImportList[i].Vin);
                    if (SalesCarCheck != null)
                    {
                        if (!string.IsNullOrEmpty(SalesCarCheck.CarStatus) && SalesCarCheck.CarStatus != "006")
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "�s�ڂ̎ԑ�ԍ�" + ImportList[i].Vin + "�͍݌ɂƂ��đ��݂��Ă���\��������܂��B�Y���̎ԗ����m�F���Ă���ēx���s���ĉ������B");
                        }
                    }

                }
                //�ԑ�ԍ��iExcel���ɏd�����Ȃ����j
                var vinret = from a in ImportList
                          where ImportList[i].Vin.Equals(a.Vin)
                          select a;

                if (vinret.Count() > 1)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "�s�ڂ̎ԑ�ԍ�" + ImportList[i].Vin + "�͓���Excel����2�ȏ㑶�݂��Ă��܂��B�P�ɂ��ĉ������B");
                }

                /*----------------------*/
                /* ���f�[�^���`�F�b�N   */
                /*----------------------*/

                //�Ǘ��ԍ�
                if (!string.IsNullOrEmpty(ImportList[i].SalesCarNumber))
                {
                    if(ImportList[i].SalesCarNumber.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesCarNumber", i + 1 + "�s�ڂ̊Ǘ��ԍ�" + ImportList[i].SalesCarNumber + "�͕������̐������I�[�o�[���Ă��܂��B�Ǘ��ԍ���50�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�ԑ�ԍ�
                if (!string.IsNullOrEmpty(ImportList[i].Vin))
                {
                    if (ImportList[i].Vin.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Vin", i + 1 + "�s�ڂ̎ԑ�ԍ�" + ImportList[i].Vin + "�͕������̐������I�[�o�[���Ă��܂��B�ԑ�ԍ���20�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //VIN(�V���A��)
                if (!string.IsNullOrEmpty(ImportList[i].UsVin))
                {
                    if (ImportList[i].UsVin.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].UsVin", i + 1 + "�s�ڂ�VIN(�V���A��)" + ImportList[i].Vin + "�͕������̐������I�[�o�[���Ă��܂��BVIN(�V���A��)��20�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //���[�J�[��
                if (!string.IsNullOrEmpty(ImportList[i].MakerName))
                {
                    if (ImportList[i].MakerName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MakerName", i + 1 + "�s�ڂ̃��[�J�[��" + ImportList[i].MakerName + "�͕������̐������I�[�o�[���Ă��܂��B���[�J�[����50�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�ԗ��O���[�h�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].CarGradeCode))
                {
                    if (ImportList[i].CarGradeCode.Length > 30)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarGradeCode", i + 1 + "�s�ڂ̎ԗ��O���[�h�R�[�h" + ImportList[i].CarGradeCode + "�͕������̐������I�[�o�[���Ă��܂��B�ԗ��O���[�h�R�[�h��30�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�V���敪
                if (!string.IsNullOrEmpty(ImportList[i].NewUsedType))
                {
                    if (ImportList[i].NewUsedType.Length > 3)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].NewUsedType", i + 1 + "�s�ڂ̐V���敪" + ImportList[i].NewUsedType + "�͕������̐������I�[�o�[���Ă��܂��B�V���敪��3�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�n���F
                if (!string.IsNullOrEmpty(ImportList[i].ColorType))
                {
                    if (ImportList[i].ColorType.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ColorTypenName", i + 1 + "�s�ڂ̌n���F" + ImportList[i].ColorType + "�͕������̐������I�[�o�[���Ă��܂��B�n���F��50�����ȓ��œ��͂��ĉ������B");

                    }
                }
                
                //�O���F�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorCode))
                {
                    if (ImportList[i].ExteriorColorCode.Length > 8)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorCode", i + 1 + "�s�ڂ̊O���F�R�[�h" + ImportList[i].ExteriorColorCode + "�͕������̐������I�[�o�[���Ă��܂��B�O���F�R�[�h��8�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�O���F��
                if (!string.IsNullOrEmpty(ImportList[i].ExteriorColorName))
                {
                    if (ImportList[i].ExteriorColorName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ExteriorColorName", i + 1 + "�s�ڂ̊O���F��" + ImportList[i].ExteriorColorName + "�͕������̐������I�[�o�[���Ă��܂��B�O���F����50�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�����F�R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorCode))
                {
                    if (ImportList[i].InteriorColorCode.Length > 8)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorCode", i + 1 + "�s�ڂ̓����F�R�[�h" + ImportList[i].InteriorColorCode + "�͕������̐������I�[�o�[���Ă��܂��B�����F�R�[�h��8�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�����F��
                if (!string.IsNullOrEmpty(ImportList[i].InteriorColorName))
                {
                    if (ImportList[i].InteriorColorName.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].InteriorColorName", i + 1 + "�s�ڂ̓����F��" + ImportList[i].InteriorColorName + "�͕������̐������I�[�o�[���Ă��܂��B�����F����50�����ȓ��œ��͂��ĉ������B");

                    }
                }

                //�N��
                if (!string.IsNullOrEmpty(ImportList[i].ManufacturingYear))
                {
                    if (ImportList[i].ManufacturingYear.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ManufacturingYear", i + 1 + "�s�ڂ̔N��" + ImportList[i].ManufacturingYear + "�͕������̐������I�[�o�[���Ă��܂��B�N����10�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //�n���h��
                if (!string.IsNullOrEmpty(ImportList[i].Steering))
                {
                    if (ImportList[i].Steering.Length > 3)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Steering", i + 1 + "�s�ڂ̃n���h��" + ImportList[i].Steering + "�͕������̐������I�[�o�[���Ă��܂��B�n���h����3�����ȓ��œ��͂��ĉ������B");
                    }
                }

                //�̔����i(�Ŕ��j
                if (!string.IsNullOrEmpty(ImportList[i].SalesPrice))
                {
                    string SalesPrice = ImportList[i].SalesPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(SalesPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SalesPrice", i + 1 + "�s�ڂ̔̔����i(�Ŕ��j" + ImportList[i].SalesPrice + "�𐔒l�ɕϊ��ł��܂���B�̔����i(�Ŕ��j��10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�̔����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].SalesPrice", i + 1 + "�s�ڂ̔̔����i(�Ŕ��j" + ImportList[i].SalesPrice + "�̌������I�[�o�[���Ă��܂��B�̔����i(�Ŕ��j��10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�^��
                if (!string.IsNullOrEmpty(ImportList[i].ModelName))
                {
                    if (ImportList[i].ModelName.Length > 20)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ModelName", i + 1 + "�s�ڂ̌^��" + ImportList[i].ModelName + "�͕������̐������I�[�o�[���Ă��܂��B�^����20�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //Mod 2020/11/27 yano #4072
                //�����@�^��
                if (!string.IsNullOrEmpty(ImportList[i].EngineType))
                {
                    if (ImportList[i].EngineType.Length > 25)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].EngineType", i + 1 + "�s�ڂ̌����@�^��" + ImportList[i].EngineType + "�͕������̐������I�[�o�[���Ă��܂��B�����@�^����10�����ȓ��œ��͂��ĉ�����");
                    }
                }
                
                //�r�C��
                if (!string.IsNullOrEmpty(ImportList[i].Displacement))
                {
                    decimal d;
                    if (!decimal.TryParse(ImportList[i].Displacement, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Displacement", i + 1 + "�s�ڂ̔r�C��" + ImportList[i].Displacement + "�𐔒l�ɕϊ��ł��܂���B�r�C�ʂɂ�10���ȓ��̐��l�œ��͂��ĉ�����");
                    }
                    else
                    {
                        //�p���ʂ�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < 0)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Displacement", i + 1 + "�s�ڂ̔r�C��" + ImportList[i].Displacement + "�̌������I�[�o�[���Ă��܂��B�r�C�ʂɂ�10���ȓ��̐��l�œ��͂��ĉ�����");
                        }
                    }
                }

                //�^���w��ԍ�
                if (!string.IsNullOrEmpty(ImportList[i].ModelSpecificateNumber))
                {
                    if (ImportList[i].ModelSpecificateNumber.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ModelSpecificateNumber", i + 1 + "�s�ڂ̌^���w��ԍ�" + ImportList[i].ModelSpecificateNumber + "�͕������̐������I�[�o�[���Ă��܂��B�^���w��ԍ���10�����ȓ��œ��͂��ĉ������B");
                    }

                }

                //�ޕʋ敪�ԍ�
                if (!string.IsNullOrEmpty(ImportList[i].ClassificationTypeNumber))
                {
                    if (ImportList[i].ClassificationTypeNumber.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ClassificationTypeNumber", i + 1 + "�s�ڂ̗ޕʋ敪�ԍ�" + ImportList[i].ClassificationTypeNumber + "�͕������̐������I�[�o�[���Ă��܂��B�ޕʋ敪�ԍ���10�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�P
                if (!string.IsNullOrEmpty(ImportList[i].Memo1))
                {
                    if (ImportList[i].Memo1.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo1", i + 1 + "�s�ڂ̔��l�P" + ImportList[i].Memo1 + "�͕������̐������I�[�o�[���Ă��܂��B���l�P��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�Q
                if (!string.IsNullOrEmpty(ImportList[i].Memo2))
                {
                    if (ImportList[i].Memo2.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo2", i + 1 + "�s�ڂ̔��l�Q" + ImportList[i].Memo2 + "�͕������̐������I�[�o�[���Ă��܂��B���l�Q��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�R
                if (!string.IsNullOrEmpty(ImportList[i].Memo3))
                {
                    if (ImportList[i].Memo3.Length > 100){ModelState.AddModelError("ImportLine[" + i + "].Memo3", i + 1 + "�s�ڂ̔��l�R" + ImportList[i].Memo3 + "�͕������̐������I�[�o�[���Ă��܂��B���l�R��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�S
                if (!string.IsNullOrEmpty(ImportList[i].Memo4))
                {
                    if (ImportList[i].Memo4.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo4", i + 1 + "�s�ڂ̔��l�S" + ImportList[i].Memo4 + "�͕������̐������I�[�o�[���Ă��܂��B���l�S��100�����ȓ��œ��͂��ĉ������B");
                    }
                }

                //���l�T
                if (!string.IsNullOrEmpty(ImportList[i].Memo5))
                {
                    if (ImportList[i].Memo5.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo5", i + 1 + "�s�ڂ̔��l�T" + ImportList[i].Memo5 + "�͕������̐������I�[�o�[���Ă��܂��B���l�T��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�U
                if (!string.IsNullOrEmpty(ImportList[i].Memo6))
                {
                    if (ImportList[i].Memo6.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo6", i + 1 + "�s�ڂ̔��l�U" + ImportList[i].Memo6 + "�͕������̐������I�[�o�[���Ă��܂��B���l�U��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�V
                if (!string.IsNullOrEmpty(ImportList[i].Memo7))
                {
                    if (ImportList[i].Memo7.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo7", i + 1 + "�s�ڂ̔��l�V" + ImportList[i].Memo7 + "�͕������̐������I�[�o�[���Ă��܂��B���l�V��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�W
                if (!string.IsNullOrEmpty(ImportList[i].Memo8))
                {
                    if (ImportList[i].Memo8.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo8", i + 1 + "�s�ڂ̔��l�W" + ImportList[i].Memo8 + "�͕������̐������I�[�o�[���Ă��܂��B���l�W��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�X
                if (!string.IsNullOrEmpty(ImportList[i].Memo9))
                {
                    if (ImportList[i].Memo9.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo9", i + 1 + "�s�ڂ̔��l�X" + ImportList[i].Memo9 + "�͕������̐������I�[�o�[���Ă��܂��B���l�X��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
                
                //���l�P�O
                if (!string.IsNullOrEmpty(ImportList[i].Memo10))
                {
                    if (ImportList[i].Memo10.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo10", i + 1 + "�s�ڂ̔��l�P�O" + ImportList[i].Memo10 + "�͕������̐������I�[�o�[���Ă��܂��B���l�P�O��100�����ȓ��œ��͂��ĉ������B");    
                    }
                }

                //�����Ԑ�
                if (!string.IsNullOrEmpty(ImportList[i].CarTax))
                {
                    string CarTax = ImportList[i].CarTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "�s�ڂ̎����ԐŎ�ʊ�" + ImportList[i].CarTax + "�𐔒l�ɕϊ��ł��܂���B�����ԐŎ�ʊ��ɂ�10���ȓ��̐��l����͂��ĉ�����");    //Mod 2019/09/04 yano #4011
                        //ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "�s�ڂ̎����Ԑ�" + ImportList[i].CarTax + "�𐔒l�ɕϊ��ł��܂���B�����Ԑłɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�����Ԑł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "�s�ڂ̎����ԐŎ�ʊ�" + ImportList[i].CarTax + "�̌������I�[�o�[���Ă��܂��B�����ԐŎ�ʊ��ɂ�10���ȓ��̐��l����͂��ĉ�����");  //Mod 2019/09/04 yano #4011
                            //ModelState.AddModelError("ImportLine[" + i + "].CarTax", i + 1 + "�s�ڂ̎����Ԑ�" + ImportList[i].CarTax + "�̌������I�[�o�[���Ă��܂��B�����Ԑłɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }

                }

                //�����ԏd�ʐ�
                if (!string.IsNullOrEmpty(ImportList[i].CarWeightTax))
                {
                    string CarWeightTax = ImportList[i].CarWeightTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarWeightTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarWeightTax", i + 1 + "�s�ڂ̎����ԏd�ʐ�" + ImportList[i].CarWeightTax + "�𐔒l�ɕϊ��ł��܂���B�����ԏd�ʐłɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�����ԏd�ʐł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarWeightTax", i + 1 + "�s�ڂ̎����ԏd�ʐ�" + ImportList[i].CarWeightTax + "�̌������I�[�o�[���Ă��܂��B�����ԏd�ʐłɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�����ӕی���
                if (!string.IsNullOrEmpty(ImportList[i].CarLiabilityInsurance))
                {
                    string CarLiabilityInsurance = ImportList[i].CarLiabilityInsurance.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarLiabilityInsurance, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarLiabilityInsurance", i + 1 + "�s�ڂ̎����ӕی���" + ImportList[i].CarLiabilityInsurance + "�𐔒l�ɕϊ��ł��܂���B�����ӕی����ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�����ӕی�����10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarLiabilityInsurance", i + 1 + "�s�ڂ̎����ԏd�ʐ�" + ImportList[i].CarLiabilityInsurance + "�̌������I�[�o�[���Ă��܂��B�����ӕی����ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�����ԐŊ����\��
                if (!string.IsNullOrEmpty(ImportList[i].AcquisitionTax))
                {
                    string AcquisitionTax = ImportList[i].AcquisitionTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AcquisitionTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AcquisitionTax", i + 1 + "�s�ڂ̎����ԐŊ����\��" + ImportList[i].AcquisitionTax + "�𐔒l�ɕϊ��ł��܂���B�����ԐŊ����\���ɂ�10���ȓ��̐��l����͂��ĉ�����");  //Mod 2019/09/04 yano #4011
                    }
                    else
                    {
                        //�����Ԋ����\����10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AcquisitionTax", i + 1 + "�s�ڂ̎����ԐŊ����\��" + ImportList[i].AcquisitionTax + "�̌������I�[�o�[���Ă��܂��B�����ԐŊ����\���ɂ�10���ȓ��̐��l����͂��ĉ�����");//Mod 2019/09/04 yano #4011
                        }
                    }
                }

                //���T�C�N���a����
                if (!string.IsNullOrEmpty(ImportList[i].RecycleDeposit))
                {
                    string RecycleDeposit = ImportList[i].RecycleDeposit.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RecycleDeposit, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RecycleDeposit", i + 1 + "�s�ڂ̃��T�C�N���a����" + ImportList[i].RecycleDeposit + "�𐔒l�ɕϊ��ł��܂���B���T�C�N���a�����ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���T�C�N���a������10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RecycleDeposit", i + 1 + "�s�ڂ̃��T�C�N���a����" + ImportList[i].RecycleDeposit + "�̌������I�[�o�[���Ă��܂��B���T�C�N���a�����ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�F�蒆�Î�No
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarNumber))
                {
                    if (ImportList[i].ApprovedCarNumber.Length > 50)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarNumber", i + 1 + "�s�ڂ̔F�蒆�Î�No" + ImportList[i].ApprovedCarNumber + "�͕������̐������I�[�o�[���Ă��܂��B�F�蒆�Î�No��50�����ȓ��œ��͂��ĉ������B");
                    }
                }

                //�F�蒆�Îԕۏ؊���FROM
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarWarrantyDateFrom))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].ApprovedCarWarrantyDateFrom, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarWarrantyDateFrom", i + 1 + "�s�ڂ̔F�蒆�Îԕۏ؊���FROM" + ImportList[i].ApprovedCarWarrantyDateFrom + "����t�ɕϊ��ł��܂���B�F�蒆�Îԕۏ؊���FROM�ɂ͓��t(YYYY/MM/DD)����͂��ĉ�����");
                    }
                }

                //�F�蒆�Îԕۏ؊���TO
                if (!string.IsNullOrEmpty(ImportList[i].ApprovedCarWarrantyDateTo))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].ApprovedCarWarrantyDateTo, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].ApprovedCarWarrantyDateTo", i + 1 + "�s�ڂ̔F�蒆�Îԕۏ؊���TO" + ImportList[i].ApprovedCarWarrantyDateTo + "����t�ɕϊ��ł��܂���B�F�蒆�Îԕۏ؊���TO�ɂ͓��t(YYYY/MM/DD)����͂��ĉ�����");
                    }
                }

                //�d����
                if (!string.IsNullOrEmpty(ImportList[i].SlipDate))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].SlipDate, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SlipDate", i + 1 + "�s�ڂ̎d����" + ImportList[i].SlipDate + "����t�ɕϊ��ł��܂���B�d�����ɂ͓��t(YYYY/MM/DD)����͂��ĉ�����");
                    }
                }

                //���ɗ\���
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseDate))
                {
                    DateTime d;
                    if (!DateTime.TryParse(ImportList[i].PurchaseDate, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseDate", i + 1 + "�s�ڂ̓��ɗ\���" + ImportList[i].PurchaseDate + "����t�ɕϊ��ł��܂���B���ɗ\����ɂ͓��t(YYYY/MM/DD)����͂��ĉ�����");
                    }
                }

                //�d����R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].SupplierCode))
                {
                    if (ImportList[i].SupplierCode.Length > 10)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].SupplierCode", i + 1 + "�s�ڂ̎d����R�[�h" + ImportList[i].SupplierCode + "�͕������̐������I�[�o�[���Ă��܂��B�d����R�[�h��10�����ȓ��œ��͂��ĉ������B");
                    }
                }

                //�d�����P�[�V�����R�[�h
                if (!string.IsNullOrEmpty(ImportList[i].PurchaseLocationCode))
                {
                    if (ImportList[i].PurchaseLocationCode.Length > 12)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].PurchaseLocationCode", i + 1 + "�s�ڂ̎d�����P�[�V�����R�[�h" + ImportList[i].PurchaseLocationCode + "�͕������̐������I�[�o�[���Ă��܂��B�d�����P�[�V�����R�[�h��12�����ȓ��œ��͂��ĉ������B");
                    }
                }

                //�ԗ��{�̉��i
                if (!string.IsNullOrEmpty(ImportList[i].VehiclePrice))
                {
                    string VehiclePrice = ImportList[i].VehiclePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehiclePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehiclePrice", i + 1 + "�s�ڂ̎ԗ��{�̉��i" + ImportList[i].VehiclePrice + "�𐔒l�ɕϊ��ł��܂���B�ԗ��{�̉��i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�ԗ��{�̉��i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehiclePrice", i + 1 + "�s�ڂ̎ԗ��{�̉��i" + ImportList[i].VehiclePrice + "�̌������I�[�o�[���Ă��܂��B�ԗ��{�̉��i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�ԗ��{�̏����
                if (!string.IsNullOrEmpty(ImportList[i].VehicleTax))
                {
                    string VehicleTax = ImportList[i].VehicleTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehicleTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehicleTax", i + 1 + "�s�ڂ̎ԗ��{�̏����" + ImportList[i].VehicleTax + "�𐔒l�ɕϊ��ł��܂���B�ԗ��{�̏���łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�ԗ��{�̏���ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehicleTax", i + 1 + "�s�ڂ̎ԗ��{�̉��i" + ImportList[i].VehicleTax + "�̌������I�[�o�[���Ă��܂��B�ԗ��{�̏���łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�ԗ��{�̐ō����i
                if (!string.IsNullOrEmpty(ImportList[i].VehicleAmount))
                {
                    string VehicleAmount = ImportList[i].VehicleAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(VehicleAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].VehicleAmount", i + 1 + "�s�ڂ̎ԗ��{�̐ō����i" + ImportList[i].VehicleAmount + "�𐔒l�ɕϊ��ł��܂���B�ԗ��{�̐ō����i�ɂ͐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�ԗ��{�̏���ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].VehicleAmount", i + 1 + "�s�ڂ̎ԗ��{�̐ō����i" + ImportList[i].VehicleAmount + "�̌������I�[�o�[���Ă��܂��B�ԗ��{�̐ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�v�V�������i
                if (!string.IsNullOrEmpty(ImportList[i].OptionPrice))
                {
                    string OptionPrice = ImportList[i].OptionPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionPrice", i + 1 + "�s�ڂ̃I�v�V�������i" + ImportList[i].OptionPrice + "�𐔒l�ɕϊ��ł��܂���B�I�v�V�������i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�v�V�������i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionPrice", i + 1 + "�s�ڂ̃I�v�V�������i" + ImportList[i].OptionPrice + "�̌������I�[�o�[���Ă��܂��B�I�v�V�������i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�v�V���������
                if (!string.IsNullOrEmpty(ImportList[i].OptionTax))
                {
                    string OptionTax = ImportList[i].OptionTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionTax", i + 1 + "�s�ڂ̃I�v�V���������" + ImportList[i].OptionTax + "�𐔒l�ɕϊ��ł��܂���B�I�v�V��������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�v�V��������ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionTax", i + 1 + "�s�ڂ̃I�v�V���������" + ImportList[i].OptionTax + "�̌������I�[�o�[���Ă��܂��B�I�v�V��������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�v�V�����ō����i
                if (!string.IsNullOrEmpty(ImportList[i].OptionAmount))
                {
                    string OptionAmount = ImportList[i].OptionAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OptionAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OptionAmount", i + 1 + "�s�ڂ̃I�v�V�����ō����i" + ImportList[i].OptionAmount + "�𐔒l�ɕϊ��ł��܂���B�I�v�V�����ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�v�V�����ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OptionAmount", i + 1 + "�s�ڂ̃I�v�V�����ō����i" + ImportList[i].OptionAmount + "�̌������I�[�o�[���Ă��܂��B�I�v�V�����ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�f�B�X�J�E���g���i
                if (!string.IsNullOrEmpty(ImportList[i].DiscountPrice))
                {
                    string DiscountPrice = ImportList[i].DiscountPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountPrice", i + 1 + "�s�ڂ̃f�B�X�J�E���g���i" + ImportList[i].DiscountPrice + "�𐔒l�ɕϊ��ł��܂���B�f�B�X�J�E���g���i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�f�B�X�J�E���g���i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountPrice", i + 1 + "�s�ڂ̃f�B�X�J�E���g���i" + ImportList[i].DiscountPrice + "�̌������I�[�o�[���Ă��܂��B�f�B�X�J�E���g���i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�f�B�X�J�E���g�����
                if (!string.IsNullOrEmpty(ImportList[i].DiscountTax))
                {
                    string DiscountTax = ImportList[i].DiscountTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountTax", i + 1 + "�s�ڂ̃f�B�X�J�E���g�����" + ImportList[i].DiscountTax + "�𐔒l�ɕϊ��ł��܂���B�f�B�X�J�E���g����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�f�B�X�J�E���g����ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountTax", i + 1 + "�s�ڂ̃f�B�X�J�E���g�����" + ImportList[i].DiscountTax + "�̌������I�[�o�[���Ă��܂��B�f�B�X�J�E���g����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�f�B�X�J�E���g�ō����i
                if (!string.IsNullOrEmpty(ImportList[i].DiscountAmount))
                {
                    string DiscountAmount = ImportList[i].DiscountAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(DiscountAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].DiscountAmount", i + 1 + "�s�ڂ̃f�B�X�J�E���g�ō����i" + ImportList[i].DiscountAmount + "�𐔒l�ɕϊ��ł��܂���B�f�B�X�J�E���g�ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�f�B�X�J�E���g�ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].DiscountAmount", i + 1 + "�s�ڂ̃f�B�X�J�E���g�ō����i" + ImportList[i].DiscountAmount + "�̌������I�[�o�[���Ă��܂��B�f�B�X�J�E���g�ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�t�@�[�����i
                if (!string.IsNullOrEmpty(ImportList[i].FirmPrice))
                {
                    string FirmPrice = ImportList[i].FirmPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmPrice", i + 1 + "�s�ڂ̃t�@�[�����i" + ImportList[i].FirmPrice + "�𐔒l�ɕϊ��ł��܂���B�t�@�[�����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�t�@�[�����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmPrice", i + 1 + "�s�ڂ̃t�@�[�����i" + ImportList[i].FirmPrice + "�̌������I�[�o�[���Ă��܂��B�t�@�[�����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�t�@�[�������
                if (!string.IsNullOrEmpty(ImportList[i].FirmTax))
                {
                    string FirmTax = ImportList[i].FirmTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmTax", i + 1 + "�s�ڂ̃t�@�[�������" + ImportList[i].FirmTax + "�𐔒l�ɕϊ��ł��܂���B�t�@�[������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�t�@�[������ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmTax", i + 1 + "�s�ڂ̃t�@�[�������" + ImportList[i].FirmTax + "�̌������I�[�o�[���Ă��܂��B�t�@�[������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�t�@�[���ō����i
                if (!string.IsNullOrEmpty(ImportList[i].FirmAmount))
                {
                    string FirmAmount = ImportList[i].FirmAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(FirmAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].FirmAmount", i + 1 + "�s�ڂ̃t�@�[���ō����i" + ImportList[i].FirmAmount + "�𐔒l�ɕϊ��ł��܂���B�t�@�[���ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�t�@�[���ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].FirmAmount", i + 1 + "�s�ڂ̃t�@�[���ō����i" + ImportList[i].FirmAmount + "�̌������I�[�o�[���Ă��܂��B�t�@�[���ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���^���b�N���i
                if (!string.IsNullOrEmpty(ImportList[i].MetallicPrice))
                {
                    string MetallicPrice = ImportList[i].MetallicPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicPrice", i + 1 + "�s�ڂ̃��^���b�N���i" + ImportList[i].MetallicPrice + "�𐔒l�ɕϊ��ł��܂���B���^���b�N���i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���^���b�N���i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicPrice", i + 1 + "�s�ڂ̃��^���b�N���i" + ImportList[i].MetallicPrice + "�̌������I�[�o�[���Ă��܂��B���^���b�N���i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���^���b�N�����
                if (!string.IsNullOrEmpty(ImportList[i].MetallicTax))
                {
                    string MetallicTax = ImportList[i].MetallicTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicTax", i + 1 + "�s�ڂ̃��^���b�N�����" + ImportList[i].MetallicTax + "�𐔒l�ɕϊ��ł��܂���B���^���b�N����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���^���b�N����ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicTax", i + 1 + "�s�ڂ̃��^���b�N���i" + ImportList[i].MetallicTax + "�̌������I�[�o�[���Ă��܂��B���^���b�N����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���^���b�N�ō����i
                if (!string.IsNullOrEmpty(ImportList[i].MetallicAmount))
                {
                    string MetallicAmount = ImportList[i].MetallicAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(MetallicAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].MetallicAmount", i + 1 + "�s�ڂ̃��^���b�N�ō����i" + ImportList[i].MetallicAmount + "�𐔒l�ɕϊ��ł��܂���B���^���b�N�ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���^���b�N�ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].MetallicAmount", i + 1 + "�s�ڂ̃��^���b�N�ō����i" + ImportList[i].MetallicAmount + "�̌������I�[�o�[���Ă��܂��B���^���b�N�ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�������i
                if (!string.IsNullOrEmpty(ImportList[i].EquipmentPrice))
                {
                    string EquipmentPrice = ImportList[i].EquipmentPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(EquipmentPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].EquipmentPrice", i + 1 + "�s�ڂ̉������i" + ImportList[i].EquipmentPrice + "�𐔒l�ɕϊ��ł��܂���B�������i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�������i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].EquipmentPrice", i + 1 + "�s�ڂ̉������i" + ImportList[i].EquipmentPrice + "�̌������I�[�o�[���Ă��܂��B�������i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }

                }

                //���C���i
                if (!string.IsNullOrEmpty(ImportList[i].RepairPrice))
                {
                    string RepairPrice = ImportList[i].RepairPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RepairPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RepairPrice", i + 1 + "�s�ڂ̉��C���i" + ImportList[i].RepairPrice + "�𐔒l�ɕϊ��ł��܂���B���C���i�ɂ͐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���C���i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RepairPrice", i + 1 + "�s�ڂ̉��C���i" + ImportList[i].RepairPrice + "�̌������I�[�o�[���Ă��܂��B���C���i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���̑����i
                if (!string.IsNullOrEmpty(ImportList[i].OthersPrice))
                {
                    string OthersPrice = ImportList[i].OthersPrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersPrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersPrice", i + 1 + "�s�ڂ̂��̑����i" + ImportList[i].OthersPrice + "�𐔒l�ɕϊ��ł��܂���B���̑����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���̑����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersPrice", i + 1 + "�s�ڂ̂��̑����i" + ImportList[i].OthersPrice + "�̌������I�[�o�[���Ă��܂��B���̑����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���̑������
                if (!string.IsNullOrEmpty(ImportList[i].OthersTax))
                {
                    string OthersTax = ImportList[i].OthersTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersTax", i + 1 + "�s�ڂ̂��̑������" + ImportList[i].OthersTax + "�𐔒l�ɕϊ��ł��܂���B���̑�����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���̑�����ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersTax", i + 1 + "�s�ڂ̂��̑������" + ImportList[i].OthersTax + "�̌������I�[�o�[���Ă��܂��B���̑�����łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���̑��ō����i
                if (!string.IsNullOrEmpty(ImportList[i].OthersAmount))
                {
                    string OthersAmount = ImportList[i].OthersAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(OthersAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].OthersAmount", i + 1 + "�s�ڂ̂��̑��ō����i" + ImportList[i].OthersAmount + "�𐔒l�ɕϊ��ł��܂���B���̑��ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���̑��ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].OthersAmount", i + 1 + "�s�ڂ̂��̑��ō����i" + ImportList[i].OthersAmount + "�̌������I�[�o�[���Ă��܂��B���̑��ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���ŏ[��
                if (!string.IsNullOrEmpty(ImportList[i].CarTaxAppropriatePrice))
                {
                    string CarTaxAppropriatePrice = ImportList[i].CarTaxAppropriatePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(CarTaxAppropriatePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].CarTaxAppropriatePrice", i + 1 + "�s�ڂ̎��ŏ[��" + ImportList[i].CarTaxAppropriatePrice + "�𐔒l�ɕϊ��ł��܂���B���ŏ[���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���ŏ[����10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].CarTaxAppropriatePrice", i + 1 + "�s�ڂ̎��ŏ[��" + ImportList[i].CarTaxAppropriatePrice + "�̌������I�[�o�[���Ă��܂��B���ŏ[���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���T�C�N��
                if (!string.IsNullOrEmpty(ImportList[i].RecyclePrice))
                {
                    string RecyclePrice = ImportList[i].RecyclePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(RecyclePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].RecyclePrice", i + 1 + "�s�ڂ̃��T�C�N��" + ImportList[i].RecyclePrice + "�𐔒l�ɕϊ��ł��܂���B���T�C�N���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //���T�C�N����10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].RecyclePrice", i + 1 + "�s�ڂ̃��T�C�N��" + ImportList[i].RecyclePrice + "�̌������I�[�o�[���Ă��܂��B���T�C�N���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�[�N�V�������D��
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeePrice))
                {
                    string AuctionFeePrice = ImportList[i].AuctionFeePrice.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeePrice, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeePrice", i + 1 + "�s�ڂ̃I�[�N�V�������D��" + ImportList[i].AuctionFeePrice + "�𐔒l�ɕϊ��ł��܂���B�I�[�N�V�������D���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�[�N�V�������D����10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeePrice", i + 1 + "�s�ڂ̃I�[�N�V�������D��" + ImportList[i].AuctionFeePrice + "�̌������I�[�o�[���Ă��܂��B�I�[�N�V�������D���ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�[�N�V�������D�������
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeeTax))
                {
                    string AuctionFeeTax = ImportList[i].AuctionFeeTax.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeeTax, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeTax", i + 1 + "�s�ڂ̃I�[�N�V�������D�������" + ImportList[i].AuctionFeeTax + "�𐔒l�ɕϊ��ł��܂���B�I�[�N�V�������D������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�[�N�V�������D������ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeTax", i + 1 + "�s�ڂ̃I�[�N�V�������D�������" + ImportList[i].AuctionFeeTax + "�̌������I�[�o�[���Ă��܂��B�I�[�N�V�������D������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�I�[�N�V�������D���ō�
                if (!string.IsNullOrEmpty(ImportList[i].AuctionFeeAmount))
                {
                    string AuctionFeeAmount = ImportList[i].AuctionFeeAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(AuctionFeeAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeAmount", i + 1 + "�s�ڂ̃I�[�N�V�������D���ō�" + ImportList[i].AuctionFeeAmount + "�𐔒l�ɕϊ��ł��܂���B�I�[�N�V�������D���ō��ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�I�[�N�V�������D���ō���10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].AuctionFeeAmount", i + 1 + "�s�ڂ̃I�[�N�V�������D���ō�" + ImportList[i].AuctionFeeAmount + "�̌������I�[�o�[���Ă��܂��B�I�[�N�V�������D���ō��ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�d�����i
                if (!string.IsNullOrEmpty(ImportList[i].Amount))
                {
                    string Amount = ImportList[i].Amount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(Amount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Amount", i + 1 + "�s�ڂ̎d�����i" + ImportList[i].Amount + "�𐔒l�ɕϊ��ł��܂���B�d�����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�d�����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].Amount", i + 1 + "�s�ڂ̎d�����i" + ImportList[i].Amount + "�̌������I�[�o�[���Ă��܂��B�d�����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�d�������
                if (!string.IsNullOrEmpty(ImportList[i].TaxAmount))
                {
                    string TaxAmount = ImportList[i].TaxAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(TaxAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].TaxAmount", i + 1 + "�s�ڂ̎d�������" + ImportList[i].TaxAmount + "�𐔒l�ɕϊ��ł��܂���B�d������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�d������ł�10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].TaxAmount", i + 1 + "�s�ڂ̎d�������" + ImportList[i].TaxAmount + "�̌������I�[�o�[���Ă��܂��B�d������łɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //�d���ō����i
                if (!string.IsNullOrEmpty(ImportList[i].TotalAmount))
                {
                    string TotalAmount = ImportList[i].TotalAmount.Replace(",", "");
                    decimal d;
                    if (!decimal.TryParse(TotalAmount, out d))
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].TotalAmount", i + 1 + "�s�ڂ̎d���ō����i" + ImportList[i].TotalAmount + "�𐔒l�ɕϊ��ł��܂���B�d���ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                    }
                    else
                    {
                        //�d���ō����i��10���I�[�o�[�̏ꍇ
                        if (d > DECIMAL_MAX || d < DECIMAL_MIN)
                        {
                            ModelState.AddModelError("ImportLine[" + i + "].TotalAmount", i + 1 + "�s�ڂ̎d���ō����i" + ImportList[i].TotalAmount + "�̌������I�[�o�[���Ă��܂��B�d���ō����i�ɂ�10���ȓ��̐��l����͂��ĉ�����");
                        }
                    }
                }

                //���l
                if (!string.IsNullOrEmpty(ImportList[i].Memo))
                {
                    if (ImportList[i].Memo.Length > 100)
                    {
                        ModelState.AddModelError("ImportLine[" + i + "].Memo", i + 1 + "�s�ڂ̔��l" + ImportList[i].Memo + "�͕������̐������I�[�o�[���Ă��܂��B���l��100�����ȓ��œ��͂��ĉ������B");
                    }
                }
            }
        }
        #endregion

        #region �ǂݍ��񂾃f�[�^��DB�ɓo�^
        /// <summary>
        /// DB�X�V
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\)�@�@�\���P�A �������i�̓��T�C�N�����������������i�Ƃ���
        /// 2018/06/06 arc yano #3883 �^�}�\���P �d�����i�ō������i���X�V����
        /// 2018/02/15 arc yano #3865 �ԗ��Ԃ����݁@�ԗ��}�X�^�d���G���[�s�
        /// </history>
        private void DBExecute(List<CarPurchaseExcelImportList> ImportLine, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {

                List<SalesCar> SalesCarList = new List<SalesCar>();
                List<CarPurchase> CarPurchaseList = new List<CarPurchase>();

                //�ԗ��}�X�^�Ǝԗ����׃e�[�u���ɓǂݍ��񂾏���o�^����
                foreach (var LineData in ImportLine)
                {
                    /*-----------------*/
                    /*   �ԗ��}�X�^    */
                    /*-----------------*/

                    //�ԑ�ԍ��Ŏԗ��}�X�^���������āA�f�[�^�����݂��A���A�[�ԍς������ꍇ�́A�Â��f�[�^��_���폜���āA��荞�܂����e��V�K�œo�^����
                    SalesCar SalesCarDataCheck = new SalesCar();
                    if (!string.IsNullOrEmpty(LineData.Vin))
                    {
                        SalesCarDataCheck = new SalesCarDao(db).GetDataByVin(LineData.Vin);
                        if (SalesCarDataCheck != null)
                        {
                            if (!string.IsNullOrEmpty(SalesCarDataCheck.CarStatus) && SalesCarDataCheck.CarStatus.Equals("006"))
                            {
                                SalesCarDataCheck.DelFlag = "1";    //�_���폜
                                LineData.NewUsedType = "U"; //���ɑ��݂��Ă����ԗ��̂��߁AExcel����DB�Ɏ�荞�ސV���敪��Excel�̓��e�Ɋ֌W�Ȃ��u���Îԁv�ɂ���
                                ModelState.AddModelError("", "�ԑ�ԍ��F " + LineData.Vin + " �����ɔ[�ԍς݂ŏd�����Ă������߁A�Ǘ��ԍ���U��Ȃ����܂����B");
                            }
                        }
                    }

                    SalesCar SalesCarData = new SalesCar();

                    //�Ǘ��ԍ�(A�Ȃ玩���̔�)
                    if (LineData.SalesCarNumber.Equals("A"))
                    {
                        string companyCode = "N/A";
                        try { companyCode = new CarGradeDao(db).GetByKey(LineData.CarGradeCode).Car.Brand.CompanyCode; }
                        catch (NullReferenceException) { }

                        SalesCarData.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, LineData.NewUsedType);
                    }
                    else
                    {
                        SalesCarData.SalesCarNumber = LineData.SalesCarNumber;
                    }
                    SalesCarData.CarGradeCode = LineData.CarGradeCode;                                                  //�ԗ��O���[�h�R�[�h
                    SalesCarData.NewUsedType = LineData.NewUsedType;                                                    //�V���敪
                    c_ColorCategory clorTypeData = new CodeDao(db).GetColorCategoryByKey(LineData.ColorType, false);
                    if (clorTypeData != null)
                    {
                        SalesCarData.ColorType = clorTypeData.Code ?? null;                                             //�n���F
                    }
                    else
                    {
                        SalesCarData.ColorType = null;
                    }
                    SalesCarData.ExteriorColorCode = LineData.ExteriorColorCode;                                        //�O���F�R�[�h
                    SalesCarData.ExteriorColorName = LineData.ExteriorColorName;                                        //�O���F��
                    SalesCarData.ChangeColor = null;                                                                    //�F��
                    SalesCarData.InteriorColorCode = LineData.InteriorColorCode;                                        //�����F�R�[�h
                    SalesCarData.InteriorColorName = LineData.InteriorColorName;                                        //�����F��
                    SalesCarData.ManufacturingYear = LineData.ManufacturingYear;                                        //�N��
                    SalesCarData.CarStatus = "001";                                                                     //�݌ɃX�e�[�^�X

                    if (!string.IsNullOrEmpty(LineData.PurchaseLocationCode))
                    {
                        SalesCarData.LocationCode = LineData.PurchaseLocationCode;                                      //�݌Ƀ��P�[�V����
                    }
                    else
                    {
                        SalesCarData.LocationCode = null;
                    }
                    SalesCarData.OwnerCode = null;                                                                      //���L�҃R�[�h(�ڋq�R�[�h)
                    SalesCarData.Steering = LineData.Steering;                                                          //�n���h��

                    if (string.IsNullOrEmpty(LineData.SalesPrice))
                    {
                        SalesCarData.SalesPrice = null;
                    }
                    else
                    {
                    SalesCarData.SalesPrice = decimal.Parse(LineData.SalesPrice.Replace(",", ""));                      //�̔����i
                    }
                    SalesCarData.IssueDate = null;                                                                      //�Ԍ��ؔ��s��
                    SalesCarData.MorterViecleOfficialCode = null;                                                       //���^�ǃR�[�h
                    SalesCarData.RegistrationNumberType = null;                                                         //�ԗ��o�^�ԍ�(���)
                    SalesCarData.RegistrationNumberKana = null;                                                         //�ԗ��o�^�ԍ�(����)
                    SalesCarData.RegistrationNumberPlate = null;                                                        //�ԗ��o�^�ԍ�(�v���[�g)
                    SalesCarData.RegistrationDate = null;                                                               //�o�^��
                    SalesCarData.FirstRegistrationYear = null;                                                          //���N�x�o�^
                    SalesCarData.CarClassification = null;                                                              //�����Ԏ��
                    SalesCarData.Usage = null;                                                                          //�p�r
                    SalesCarData.UsageType = null;                                                                      //�����敪
                    SalesCarData.Figure = null;                                                                         //�`��
                    SalesCarData.MakerName = LineData.MakerName;                                                        //���[�J�[��
                    SalesCarData.Capacity = null;                                                                       //���
                    SalesCarData.MaximumLoadingWeight = null;                                                           //�ő�ύڗ�
                    SalesCarData.CarWeight = null;                                                                      //�ԗ��d��
                    SalesCarData.TotalCarWeight = null;                                                                 //�ԗ����d��
                    SalesCarData.Vin = LineData.Vin;                                                                    //�ԑ�ԍ�
                    SalesCarData.Length = null;                                                                         //����
                    SalesCarData.Width = null;                                                                          //��
                    SalesCarData.Height = null;                                                                         //����
                    SalesCarData.FFAxileWeight = null;                                                                  //�O�O���d
                    SalesCarData.FRAxileWeight = null;                                                                  //�O�㎲�d
                    SalesCarData.RFAxileWeight = null;                                                                  //��O���d
                    SalesCarData.RRAxileWeight = null;                                                                  //��㎲�d
                    SalesCarData.ModelName = LineData.ModelName;                                                        //�^��
                    SalesCarData.EngineType = LineData.EngineType;                                                      //�����@�^��

                    if (string.IsNullOrEmpty(LineData.Displacement))
                    {
                        SalesCarData.Displacement = null;
                    }
                    else
                    {
                    SalesCarData.Displacement = decimal.Parse(LineData.Displacement);                                   //�r�C��
                    }
                    SalesCarData.Fuel = null;                                                                           //�R�����
                    SalesCarData.ModelSpecificateNumber = LineData.ModelSpecificateNumber;                              //�^���w��ԍ�
                    SalesCarData.ClassificationTypeNumber = LineData.ClassificationTypeNumber;                          //�ޕʋ敪�ԍ�
                    SalesCarData.PossesorName = null;                                                                   //���L�Ҏ���
                    SalesCarData.PossesorAddress = null;                                                                //���L�ҏZ��
                    SalesCarData.UserName = null;                                                                       //�g�p�Ҏ���
                    SalesCarData.UserAddress = null;                                                                    //�g�p�ҏZ��
                    SalesCarData.PrincipalPlace = null;                                                                 //�{���n
                    SalesCarData.ExpireType = null;                                                                     //�L���������
                    SalesCarData.ExpireDate = null;                                                                     //�L������
                    SalesCarData.Mileage = null;                                                                        //���s����
                    SalesCarData.MileageUnit = null;                                                                    //���s�����P��
                    SalesCarData.Memo = null;                                                                           //�Ԍ��ؔ��l
                    SalesCarData.DocumentComplete = null;                                                               //���ފ���
                    SalesCarData.DocumentRemarks = null;                                                                //���ޔ��l
                    SalesCarData.SalesDate = null;                                                                      //�[�ԓ�
                    SalesCarData.InspectionDate = null;                                                                 //�_����
                    SalesCarData.NextInspectionDate = null;                                                             //����_����
                    SalesCarData.UsVin = LineData.UsVin;                                                                //VIN(�k�ėp)
                    SalesCarData.MakerWarranty = null;                                                                  //���[�J�[�ۏ�
                    SalesCarData.RecordingNote = null;                                                                  //�L�^��(�L��)
                    SalesCarData.ProductionDate = null;                                                                 //���Y��(MDH)
                    SalesCarData.ReparationRecord = null;                                                               //�C����(�L��)
                    SalesCarData.Oil = null;                                                                            //���q�l�w��I�C��(���i�ԍ�)
                    SalesCarData.Tire = null;                                                                           //�^�C��(���i�ԍ�)
                    SalesCarData.KeyCode = null;                                                                        //�L�[�R�[�h
                    SalesCarData.AudioCode = null;                                                                      //�I�[�f�B�I�R�[�h
                    SalesCarData.Import = null;                                                                         //�A��
                    SalesCarData.Guarantee = null;                                                                      //�ۏ؏�
                    SalesCarData.Instructions = null;                                                                   //���
                    SalesCarData.Recycle = null;                                                                        //���T�C�N��
                    SalesCarData.RecycleTicket = null;                                                                  //���T�C�N����
                    SalesCarData.CouponPresence = null;                                                                 //�N�[�|���L��
                    SalesCarData.Light = null;                                                                          //���C�g
                    SalesCarData.Aw = null;                                                                             //�`�v
                    SalesCarData.Aero = null;                                                                           //�G�A��
                    SalesCarData.Sr = null;                                                                             //�r�q
                    SalesCarData.Cd = null;                                                                             //�b�c
                    SalesCarData.Md = null;                                                                             //�l�c
                    SalesCarData.NaviType = null;                                                                       //�i�r(�����E�����O)
                    SalesCarData.NaviEquipment = null;                                                                  //�i�r(HDD�A�������ADVD�ACD)
                    SalesCarData.NaviDashboard = null;                                                                  //�i�r(OnDash�AInDash)
                    SalesCarData.SeatColor = null;                                                                      //�V�[�g(�F)
                    SalesCarData.SeatType = null;                                                                       //�V�[�g
                    SalesCarData.Memo1 = LineData.Memo1;                                                                //���l�P
                    SalesCarData.Memo2 = LineData.Memo2;                                                                //���l�Q
                    SalesCarData.Memo3 = LineData.Memo3;                                                                //���l�R
                    SalesCarData.Memo4 = LineData.Memo4;                                                                //���l�S
                    SalesCarData.Memo5 = LineData.Memo5;                                                                //���l�T
                    SalesCarData.Memo6 = LineData.Memo6;                                                                //���l�U
                    SalesCarData.Memo7 = LineData.Memo7;                                                                //���l�V
                    SalesCarData.Memo8 = LineData.Memo8;                                                                //���l�W
                    SalesCarData.Memo9 = LineData.Memo9;                                                                //���l�X
                    SalesCarData.Memo10 = LineData.Memo10;                                                              //���l�P�O
                    SalesCarData.DeclarationType = null;                                                                //�\���敪
                    SalesCarData.AcquisitionReason = null;                                                              //�擾����
                    SalesCarData.TaxationTypeCarTax = null;                                                             //�ېŋ敪(�����ԐŎ�ʊ�)
                    SalesCarData.TaxationTypeAcquisitionTax = null;                                                     //�ېŋ敪(�����ԐŊ����\��)

                    if (string.IsNullOrEmpty(LineData.CarTax))
                    {
                        SalesCarData.CarTax = null;
                    }
                    else
                    {
                        SalesCarData.CarTax = decimal.Parse(LineData.CarTax.Replace(",", ""));                              //�����Ԑ�
                    }

                    if(string.IsNullOrEmpty(LineData.CarWeightTax))
                    {
                        SalesCarData.CarWeightTax = null;
                    }else{
                    SalesCarData.CarWeightTax = decimal.Parse(LineData.CarWeightTax.Replace(",", ""));                  //�����ԏd�ʐ�
                    }

                    if(string.IsNullOrEmpty(LineData.CarLiabilityInsurance))
                    {
                        SalesCarData.CarLiabilityInsurance = null;
                    }else{
                    SalesCarData.CarLiabilityInsurance = decimal.Parse(LineData.CarLiabilityInsurance.Replace(",", ""));//�����ӕی���
                    }

                    if (string.IsNullOrEmpty(LineData.AcquisitionTax))
                    {
                        LineData.AcquisitionTax = null;
                    }else{
                    SalesCarData.AcquisitionTax = decimal.Parse(LineData.AcquisitionTax.Replace(",", ""));              //�����ԐŊ����\��
                    }

                    if (string.IsNullOrEmpty(LineData.RecycleDeposit))
                    {
                        SalesCarData.RecycleDeposit = null;
                    }else{
                    SalesCarData.RecycleDeposit = decimal.Parse(LineData.RecycleDeposit.Replace(",", ""));              //���T�C�N���a����
                    }

                    
                    SalesCarData.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                     //�쐬��
                    SalesCarData.CreateDate = DateTime.Now;                                                             //�쐬����
                    SalesCarData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                 //�ŏI�X�V��
                    SalesCarData.LastUpdateDate = DateTime.Now;                                                         //�ŏI�X�V����
                    SalesCarData.DelFlag = "0";                                                                         //�폜�t���O
                    SalesCarData.EraseRegist = null;                                                                    //�����o�^
                    SalesCarData.UserCode = null;                                                                       //�g�p�҃R�[�h
                    SalesCarData.ApprovedCarNumber = LineData.ApprovedCarNumber;                                        //�F�蒆�Î�No

                    if (string.IsNullOrEmpty(LineData.ApprovedCarWarrantyDateFrom))
                    {
                        SalesCarData.ApprovedCarWarrantyDateFrom = null;
                    }
                    else
                    {
                    SalesCarData.ApprovedCarWarrantyDateFrom = DateTime.Parse(LineData.ApprovedCarWarrantyDateFrom);    //�F�蒆�Îԕۏ؊���FROM
                    }

                    if (string.IsNullOrEmpty(LineData.ApprovedCarWarrantyDateTo))
                    {
                        SalesCarData.ApprovedCarWarrantyDateTo = null;
                    }
                    else
                    {
                    SalesCarData.ApprovedCarWarrantyDateTo = DateTime.Parse(LineData.ApprovedCarWarrantyDateTo);        //�F�蒆�Îԕۏ؊���TO
                    }
                    
                    SalesCarData.Finance = null;                                                                        //�t�@�C�i���X
                    SalesCarData.InspectGuidFlag = "001";                                                               //�Ԍ��ē��ۃt���O
                    SalesCarData.InspectGuidMemo = null;                                                                //�Ԍ��ē��������l��
                    SalesCarData.CarUsage = null;                                                                       //���p�p�r
                    SalesCarData.FirstRegistrationDate = null;                                                          //���N�x�o�^��
                    SalesCarData.CompanyRegistrationFlag = null;                                                        //�o�^�t���O

                    SalesCarList.Add(SalesCarData);

                    /*-----------------------*/
                    /*   �ԗ����׃e�[�u��    */
                    /*-----------------------*/

                    CarPurchase CarPurchaseData = new CarPurchase();

                    CarPurchaseData.CarPurchaseId = Guid.NewGuid();                                                           //�ԗ��d��ID
                    CarPurchaseData.CarPurchaseOrderNumber = null;                                                            //�ԗ���������ID
                    CarPurchaseData.CarAppraisalId = null;                                                                    //�ԗ�����ID
                    CarPurchaseData.PurchaseStatus = "002";                                                                   //�d���X�e�[�^�X

                    if (string.IsNullOrEmpty(LineData.PurchaseDate))
                    {
                        CarPurchaseData.PurchaseDate = null;
                    }
                    else
                    {
                    CarPurchaseData.PurchaseDate = DateTime.Parse(LineData.PurchaseDate);                                     //���ɓ��i�o���̎d����v����A�����ԃ��[�J�[����d���ꂽ���j
                    }
                    
                    CarPurchaseData.SupplierCode = LineData.SupplierCode;                                                     //�d����R�[�h
                    CarPurchaseData.PurchaseLocationCode = LineData.PurchaseLocationCode;                                     //�d�����P�[�V�����R�[�h

                    Employee EmployeeData = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode, false);
                    if (EmployeeData != null && !string.IsNullOrEmpty(EmployeeData.DepartmentCode))
                    {

                        CarPurchaseData.DepartmentCode = EmployeeData.DepartmentCode;                                         //����R�[�h
                    }
                    else
                    {
                        CarPurchaseData.DepartmentCode = "";
                    }
                    CarPurchaseData.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                              //�S���҃R�[�h


                    if (string.IsNullOrEmpty(LineData.VehiclePrice))
                    {
                        CarPurchaseData.VehiclePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehiclePrice = decimal.Parse(LineData.VehiclePrice.Replace(",", ""));                     //�ԗ��{�̉��i
                    }

                    
                    if (string.IsNullOrEmpty(LineData.MetallicPrice))
                    {
                        CarPurchaseData.MetallicPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicPrice = decimal.Parse(LineData.MetallicPrice.Replace(",", ""));                   //���^���b�N���i
                    }

                    if (string.IsNullOrEmpty(LineData.OptionPrice))
                    {
                        CarPurchaseData.OptionPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionPrice = decimal.Parse(LineData.OptionPrice.Replace(",", ""));                       //�I�v�V�������i
                    }

                    if (string.IsNullOrEmpty(LineData.FirmPrice))
                    {
                        CarPurchaseData.FirmPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmPrice = decimal.Parse(LineData.FirmPrice.Replace(",", ""));                           //�t�@�[�����i
                    }

                    if (string.IsNullOrEmpty(LineData.DiscountPrice))
                    {
                        CarPurchaseData.DiscountPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountPrice = decimal.Parse(LineData.DiscountPrice.Replace(",", ""));                   //�f�B�X�J�E���g���i
                    }

                    if (string.IsNullOrEmpty(LineData.EquipmentPrice))
                    {
                        CarPurchaseData.EquipmentPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.EquipmentPrice = decimal.Parse(LineData.EquipmentPrice.Replace(",", ""));                 //�������i(�Ŕ�)
                    }

                    if (string.IsNullOrEmpty(LineData.RepairPrice))
                    {
                        CarPurchaseData.RepairPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.RepairPrice = decimal.Parse(LineData.RepairPrice.Replace(",", ""));                       //���C���i(�Ŕ�)
                    }

                    if (string.IsNullOrEmpty(LineData.OthersPrice))
                    {
                        CarPurchaseData.OthersPrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersPrice = decimal.Parse(LineData.OthersPrice.Replace(",", ""));                       //���̑����i
                    }


                    if (string.IsNullOrEmpty(LineData.Amount))
                    {
                        CarPurchaseData.Amount = 0;
                    }
                    else
                    {
                    CarPurchaseData.Amount = decimal.Parse(LineData.Amount.Replace(",", ""));                                 //�d�����i
                    }

                    if (string.IsNullOrEmpty(LineData.TaxAmount))
                    {
                        CarPurchaseData.TaxAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.TaxAmount = decimal.Parse(LineData.TaxAmount.Replace(",", ""));                           //�����
                    }
                    
                    CarPurchaseData.SalesCarNumber = SalesCarData.SalesCarNumber;                                             //�Ǘ��ԍ�
                    CarPurchaseData.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                        //�쐬��
                    CarPurchaseData.CreateDate = DateTime.Now;                                                                //�쐬����
                    CarPurchaseData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                    //�ŏI�X�V��
                    CarPurchaseData.LastUpdateDate = DateTime.Now;                                                            //�ŏI�X�V����
                    CarPurchaseData.DelFlag = "0";                                                                            //�폜�t���O
                    CarPurchaseData.EraseRegist = null;                                                                       //�����o�^
                    CarPurchaseData.Memo = LineData.Memo;                                                                     //���l

                    if (string.IsNullOrEmpty(LineData.SlipDate))
                    {
                        CarPurchaseData.SlipDate = null;
                    }
                    else
                    {
                    CarPurchaseData.SlipDate = DateTime.Parse(LineData.SlipDate);                                             //�d�����i�X�܂ɎԂ����ɂ������j
                    }
                    
                    CarPurchaseData.CarTaxAppropriateAmount = null;                                                           //���ŏ[���ō�
                    CarPurchaseData.RecycleAmount = null;                                                                     //���T�C�N���ō�
                    CarPurchaseData.CarPurchaseType = "004";                                                                  //���ɋ敪

                    if (string.IsNullOrEmpty(LineData.VehicleTax))
                    {
                        CarPurchaseData.VehicleTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehicleTax = decimal.Parse(LineData.VehicleTax.Replace(",", ""));                         //�ԗ��{�̏����
                    }

                    if (string.IsNullOrEmpty(LineData.VehicleAmount))
                    {
                        CarPurchaseData.VehicleAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.VehicleAmount = decimal.Parse(LineData.VehicleAmount.Replace(",", ""));                   //�ԗ��{�̐ō����i
                    }

                    if (string.IsNullOrEmpty(LineData.MetallicTax))
                    {
                        CarPurchaseData.MetallicTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicTax = decimal.Parse(LineData.MetallicTax.Replace(",", ""));                       //���^���b�N�����
                    }


                    if (string.IsNullOrEmpty(LineData.MetallicAmount))
                    {
                        CarPurchaseData.MetallicAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.MetallicAmount = decimal.Parse(LineData.MetallicAmount.Replace(",", ""));                 //���^���b�N�ō����i
                    }
                    
                    if (string.IsNullOrEmpty(LineData.OptionTax))
                    {
                        CarPurchaseData.OptionTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionTax = decimal.Parse(LineData.OptionTax.Replace(",", ""));                           //�I�v�V���������
                    }


                    if (string.IsNullOrEmpty(LineData.OptionAmount))
                    {
                        CarPurchaseData.OptionAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.OptionAmount = decimal.Parse(LineData.OptionAmount.Replace(",", ""));                     //�I�v�V�����ō����i
                    }
                    
                    if (string.IsNullOrEmpty(LineData.FirmTax))
                    {
                        CarPurchaseData.FirmTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmTax = decimal.Parse(LineData.FirmTax.Replace(",", ""));                               //�t�@�[�������
                    }
                    
                    if (string.IsNullOrEmpty(LineData.FirmAmount))
                    {
                        CarPurchaseData.FirmAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.FirmAmount = decimal.Parse(LineData.FirmAmount.Replace(",", ""));                         //�t�@�[���ō����i
                    }
                    
                    if (string.IsNullOrEmpty(LineData.DiscountTax))
                    {
                        CarPurchaseData.DiscountTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountTax = decimal.Parse(LineData.DiscountTax.Replace(",", ""));                       //�f�B�X�J�E���g�����
                    }

                    if (string.IsNullOrEmpty(LineData.DiscountAmount))
                    {
                        CarPurchaseData.DiscountAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.DiscountAmount = decimal.Parse(LineData.DiscountAmount.Replace(",", ""));                 //�f�B�X�J�E���g�ō����i
                    }

                    if (string.IsNullOrEmpty(LineData.OthersTax))
                    {
                        CarPurchaseData.OthersTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersTax = decimal.Parse(LineData.OthersTax.Replace(",", ""));                           //���̑������
                    }

                    if (string.IsNullOrEmpty(LineData.OthersAmount))
                    {
                        CarPurchaseData.OthersAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.OthersAmount = decimal.Parse(LineData.OthersAmount.Replace(",", ""));                     //���̑��ō����i
                    }

                    if (string.IsNullOrEmpty(LineData.TotalAmount))
                    {
                        CarPurchaseData.TotalAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.TotalAmount = decimal.Parse(LineData.TotalAmount.Replace(",", ""));                       //�d���ō����i
                    }
                    
                    if (string.IsNullOrEmpty(LineData.AuctionFeePrice))
                    {
                        CarPurchaseData.AuctionFeePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeePrice = decimal.Parse(LineData.AuctionFeePrice.Replace(",", ""));               //�I�[�N�V�������D��
                    }

                    if (string.IsNullOrEmpty(LineData.AuctionFeeTax))
                    {
                        CarPurchaseData.AuctionFeeTax = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeeTax = decimal.Parse(LineData.AuctionFeeTax.Replace(",", ""));                   //�I�[�N�V�������D�������
                    }

                    if (string.IsNullOrEmpty(LineData.AuctionFeeAmount))
                    {
                        CarPurchaseData.AuctionFeeAmount = 0;
                    }
                    else
                    {
                    CarPurchaseData.AuctionFeeAmount = decimal.Parse(LineData.AuctionFeeAmount.Replace(",", ""));             //�I�[�N�V�������D���ō�
                    }

                    if (string.IsNullOrEmpty(LineData.CarTaxAppropriatePrice))
                    {
                        CarPurchaseData.CarTaxAppropriatePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.CarTaxAppropriatePrice = decimal.Parse(LineData.CarTaxAppropriatePrice.Replace(",", "")); //���ŏ[��
                    }
                    
                    if (string.IsNullOrEmpty(LineData.RecyclePrice))
                    {
                        CarPurchaseData.RecyclePrice = 0;
                    }
                    else
                    {
                    CarPurchaseData.RecyclePrice = decimal.Parse(LineData.RecyclePrice.Replace(",", ""));                     //���T�C�N��
                    }
                    
                    CarPurchaseData.EquipmentTax = null;                                                                      //�������i(�����)
                    CarPurchaseData.EquipmentAmount = null;                                                                   //�������i(�ō�)
                    CarPurchaseData.RepairTax = null;                                                                         //���C���i(�����)
                    CarPurchaseData.RepairAmount = null;                                                                      //���C���i(�ō�)
                    CarPurchaseData.CarTaxAppropriateTax = null;                                                              
                    CarPurchaseData.ConsumptionTaxId = null;                                                                  //�ŗ�Id
                    CarPurchaseData.Rate = null;                                                                              //����ŗ�
                    CarPurchaseData.CancelFlag = null;                                                                        //�L�����Z���t���O
                    CarPurchaseData.LastEditScreen = "000";                                                                   //�ŏI�X�V���


                    //Mod 2018/08/28 yano #3922
                    //Add 2018/06/06 arc yano #3883
                    ////�������i�����T�C�N���������������S�Ă̋��z�ōX�V
                    //if (!string.IsNullOrWhiteSpace(SalesCarData.NewUsedType) && SalesCarData.NewUsedType.Equals("U"))
                    //{

                    decimal purchaseprice = 0m;

                    purchaseprice = CarPurchaseData.VehiclePrice +                              //�ԗ��{�̉��i(�Ŕ�)
                                    (CarPurchaseData.AuctionFeePrice ?? 0m) +                   //�I�[�N�V�������D��(�Ŕ�)
                                    (CarPurchaseData.CarTaxAppropriatePrice ?? 0m) +            //���ŏ[��(�Ŕ�)
                                    CarPurchaseData.OthersPrice +                               //���̑����i(�Ŕ�)
                                    CarPurchaseData.MetallicPrice +                             //���^���b�N���i(�Ŕ�)
                                    CarPurchaseData.OptionPrice +                               //�I�v�V�������i(�Ŕ�)     
                                    CarPurchaseData.FirmPrice +                                 //�t�@�[�����i(�Ŕ�)
                                    CarPurchaseData.DiscountPrice +                             //�f�B�X�J�E���g���i(�Ŕ�)
                                    CarPurchaseData.EquipmentPrice +                            //�������i(�Ŕ�)
                                    CarPurchaseData.RepairPrice;                                //���C���i(�Ŕ�)


                    if (purchaseprice > 0)
                    {
                        CarPurchaseData.FinancialAmount = purchaseprice;
                    }
                    else
                    {
                        CarPurchaseData.FinancialAmount = 1m;
                    }

                    //�d�����z(�ō�)���烊�T�C�N�����z���������d�����z(�Ŕ�)���擾
                    ////�����_�̐ŗ��������Őݒ�
                    //int taxrate = new ConsumptionTaxDao(db).GetByKey(new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Today)).Rate;

                    //ConsumptionTax rate = new ConsumptionTaxDao(db).GetByKey(CarPurchaseData.ConsumptionTaxId);

                    ////�d���f�[�^�ɏ���ŗ�ID���ݒ肳��Ă���ꍇ�͂�������g��
                    //if (rate != null)
                    //{
                    //    taxrate = rate.Rate;
                    //}

                    //decimal value = (CarPurchaseData.TotalAmount ?? 0) - (CarPurchaseData.RecycleAmount ?? 0);

                    //if (value > 0)
                    //{
                    //    CarPurchaseData.FinancialAmount = CommonUtils.CalcAmountWithoutTax(value, taxrate, 1);
                    //}
                    //else
                    //{
                    //    if ((CarPurchaseData.TotalAmount ?? 0) > 0)
                    //    {
                    //        CarPurchaseData.FinancialAmount = CommonUtils.CalcAmountWithoutTax((CarPurchaseData.TotalAmount ?? 0), taxrate, 1);
                    //    }
                    //    else
                    //    {
                    //        CarPurchaseData.FinancialAmount = 1m;
                    //    }
                    //}

                    ////CarPurchaseData.FinancialAmount = CarPurchaseData.Amount;                                             //�������i  
                //}

                CarPurchaseList.Add(CarPurchaseData);
               }

                db.SalesCar.InsertAllOnSubmit(SalesCarList);
                db.CarPurchase.InsertAllOnSubmit(CarPurchaseList);


                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //��荞�݊����̃��b�Z�[�W��\������
                    ModelState.AddModelError("", "��荞�݂��������܂����B");
                }
                catch (SqlException se)
                {
                    ModelState.AddModelError("", se.Message);   //Add 2018/02/15 arc yano #3865

                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);    //Add 2018/02/15 arc yano #3865

                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region �L�����Z���f�[�^�쐬
        /// <summary>
        /// �L�����Z���f�[�^�쐬
        /// </summary>
        /// <returns>�L�����Z���f�[�^</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 �^�}�\���P �������i�̍X�V�����̒ǉ�
        /// </history>
        private CarPurchase CreateCancelData(CarPurchase carPurchase)
        {
            //��ʂ���擾�ł��Ȃ��f�[�^�͌��ݕۑ��ς݂̃f�[�^����R�s�[����
            CarPurchase target = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

            CarPurchase CancelPurchase = new CarPurchase();
            CancelPurchase.CarPurchaseId = Guid.NewGuid();
            CancelPurchase.CarPurchaseOrderNumber = target.CarPurchaseOrderNumber;
            CancelPurchase.CarAppraisalId = carPurchase.CarAppraisalId;
            CancelPurchase.PurchaseStatus = "003";//�d����L�����Z��
            CancelPurchase.PurchaseDate = carPurchase.PurchaseDate;
            CancelPurchase.SupplierCode = carPurchase.SupplierCode;
            CancelPurchase.PurchaseLocationCode = carPurchase.PurchaseLocationCode;
            CancelPurchase.DepartmentCode = carPurchase.DepartmentCode;
            CancelPurchase.EmployeeCode = carPurchase.EmployeeCode;
            CancelPurchase.VehiclePrice = carPurchase.VehiclePrice * (-1);
            CancelPurchase.MetallicPrice = carPurchase.MetallicPrice * (-1);
            CancelPurchase.OptionPrice = carPurchase.OptionPrice * (-1);
            CancelPurchase.FirmPrice = carPurchase.FirmPrice * (-1);
            CancelPurchase.DiscountPrice = carPurchase.DiscountPrice * (-1);
            CancelPurchase.EquipmentPrice = carPurchase.EquipmentPrice * (-1);
            CancelPurchase.RepairPrice = carPurchase.RepairPrice * (-1);
            CancelPurchase.OthersPrice = carPurchase.OthersPrice * (-1);
            CancelPurchase.Amount = carPurchase.Amount * (-1);
            CancelPurchase.TaxAmount = carPurchase.TaxAmount * (-1);
            CancelPurchase.SalesCarNumber = carPurchase.SalesCar.SalesCarNumber;
            CancelPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            CancelPurchase.CreateDate = DateTime.Now;
            CancelPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            CancelPurchase.LastUpdateDate = DateTime.Now;
            CancelPurchase.DelFlag = "0";
            CancelPurchase.EraseRegist = carPurchase.SalesCar.EraseRegist;
            CancelPurchase.Memo = carPurchase.Memo;
            CancelPurchase.SlipDate = carPurchase.SlipDate;
            CancelPurchase.CarTaxAppropriateAmount = carPurchase.CarTaxAppropriateAmount * (-1);
            CancelPurchase.RecycleAmount = carPurchase.RecycleAmount * (-1);
            CancelPurchase.CarPurchaseType = carPurchase.CarPurchaseType;
            CancelPurchase.VehicleTax = carPurchase.VehicleTax * (-1);
            CancelPurchase.VehicleAmount = carPurchase.VehicleAmount * (-1);
            CancelPurchase.MetallicTax = carPurchase.MetallicTax * (-1);
            CancelPurchase.MetallicAmount = carPurchase.MetallicAmount * (-1);
            CancelPurchase.OptionTax = carPurchase.OptionTax * (-1);
            CancelPurchase.OptionAmount = carPurchase.OptionAmount * (-1);
            CancelPurchase.FirmTax = carPurchase.FirmTax * (-1);
            CancelPurchase.FirmAmount = carPurchase.FirmAmount * (-1);
            CancelPurchase.DiscountTax = carPurchase.DiscountTax * (-1);
            CancelPurchase.DiscountAmount = carPurchase.DiscountAmount * (-1);
            CancelPurchase.OthersTax = carPurchase.OthersTax * (-1);
            CancelPurchase.OthersAmount = carPurchase.OthersAmount * (-1);
            CancelPurchase.TotalAmount = carPurchase.TotalAmount * (-1);
            CancelPurchase.AuctionFeePrice = carPurchase.AuctionFeePrice * (-1);
            CancelPurchase.AuctionFeeTax = carPurchase.AuctionFeeTax * (-1);
            CancelPurchase.AuctionFeeAmount = carPurchase.AuctionFeeAmount * (-1);
            CancelPurchase.CarTaxAppropriatePrice = carPurchase.CarTaxAppropriatePrice * (-1);
            CancelPurchase.RecyclePrice = carPurchase.RecyclePrice * (-1);
            CancelPurchase.EquipmentTax = carPurchase.EquipmentTax * (-1);
            CancelPurchase.EquipmentAmount = carPurchase.EquipmentAmount * (-1);
            CancelPurchase.RepairTax = carPurchase.RepairTax * (-1);
            CancelPurchase.RepairAmount = carPurchase.RepairAmount * (-1);
            CancelPurchase.CarTaxAppropriateTax = carPurchase.CarTaxAppropriateTax * (-1);
            CancelPurchase.ConsumptionTaxId = carPurchase.ConsumptionTaxId;
            CancelPurchase.Rate = carPurchase.Rate;
            CancelPurchase.CancelFlag = carPurchase.CancelFlag;
            CancelPurchase.SlipNumber = carPurchase.SlipNumber;
            CancelPurchase.LastEditScreen = carPurchase.LastEditScreen;
            //CancelPurchase.RegistOwnFlag = carPurchase.RegistOwnFlag;
            CancelPurchase.CancelDate = carPurchase.CancelDate;
            CancelPurchase.CancelMemo = carPurchase.CancelMemo;

            //Add 2018/06/06 arc yano #3883
            CancelPurchase.FinancialAmount = carPurchase.FinancialAmount * (-1); ;     //�������i

            db.CarPurchase.InsertOnSubmit(CancelPurchase);

            return CancelPurchase;

        }
        #endregion

        #region �L�����Z���̃o���f�[�V����
        /// <summary>
        /// �L�����Z���̃o���f�[�V����
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2019/02/07 yano #3960 �ԗ��d�����́@���ɋ敪���u����ԁv�̖����ɂ̎d���f�[�^�̍폜���ł��Ȃ�
        /// 2017/08/10 arc nakayama #3782_�ԗ��d��_�L�����Z���@�\�ǉ�
        /// </history>
        private void Cancelvalidation(CarPurchase carPurchase)
        {
            CarPurchase targetData = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);
            
            if (targetData != null)
            {
                //if (!string.IsNullOrEmpty(targetData.CarPurchaseType) && (targetData.CarPurchaseType.Equals("001") || targetData.CarPurchaseType.Equals("005"))) 
                if (!string.IsNullOrEmpty(targetData.CarPurchaseType) && (targetData.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN) || targetData.CarPurchaseType.Equals(PURCHASETYPE_DIPOSAL))) 
                {
                    ModelState.AddModelError("CarPurchaseType", "����Ԃ܂��͈˔p�œ��Ɋm��������ԗ��͎d���L�����Z�����s�����Ƃ��ł��܂���");
                }
            }

            //�ԗ��}�X�^�̃X�e�[�^�X
            if (!string.IsNullOrEmpty(carPurchase.SalesCarNumber))
            {
                SalesCar SalesCarData = new SalesCarDao(db).GetByKey(carPurchase.SalesCarNumber);
                if (!SalesCarData.CarStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "�Y���̎ԗ��͍݌ɂƂ��đ��݂��Ă��Ȃ����߁A�d����L�����Z�����s�����Ƃ��ł��܂���");
                }
            }
            else
            {
                //�Ǘ��ԍ������o�^�Ȃ�`�F�b�N�ł��Ȃ�
            }

            //�L�����Z������(�������`�F�b�N)
            if (!string.IsNullOrEmpty(carPurchase.CancelMemo))
            {
                if (carPurchase.CancelMemo.Length > 100)
                {
                    ModelState.AddModelError("CancelMemo", "�L�����Z��������100�����ȓ��œ��͂��ĉ������i���s��2�������Ƃ݂Ȃ���܂��j");
                }
            }

            //�L�����Z�����̍i�߃`�F�b�N
            if (carPurchase.CancelDate == null)
            {
                ModelState.AddModelError("CancelDate", "�L�����Z�����s���ꍇ�A�L�����Z�����͕K�{�ł�");
            }
            else
            {
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(carPurchase.DepartmentCode, carPurchase.CancelDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("CancelDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���t�ł̓L�����Z�����s�����Ƃ��ł��܂���");
                }
            }


        }
        #endregion

        #region �폜����
        /// <summary>
        /// �폜���̃o���f�[�V����
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2019/02/07 yano #3960 �ԗ��d�����́@���ɋ敪���u����ԁv�̖����ɂ̎d���f�[�^�̍폜���ł��Ȃ�
        /// 2017/08/21 arc yano #3791 �ԗ��d�� �d���폜�@�\�̒ǉ�
        /// </history>
        private void Deletevalidation(CarPurchase carPurchase)
        {
            //�d����敪
            CarPurchase targetData = new CarPurchaseDao(db).GetByKey(carPurchase.CarPurchaseId);

            //Mod 2019/02/07 yano #3960
            if (targetData != null)
            {
                if (!string.IsNullOrWhiteSpace(targetData.CarPurchaseType))
                {
                    //�����
                    if(targetData.CarPurchaseType.Equals(PURCHASETYPE_TRADEIN))
                    {
                        int datecnt = new CarPurchaseDao(db).GetListBySlipNumberVin(targetData.SlipNumber, targetData.SalesCar.Vin).Where(x => x.PurchaseStatus.Equals("002") && !x.CarPurchaseId.Equals(targetData.CarPurchaseId)).ToList().Count;

                        //�Ώێԗ��ł��łɎd���σf�[�^�����݂��Ȃ��ꍇ�̓G���[
                        if (datecnt == 0)
                        {
                            ModelState.AddModelError("CarPurchaseType", "���ɋ敪������Ԃœo�^����Ă���d���f�[�^�͍폜�ł��܂���B���ɋ敪������ԁA�˔p�ȊO�œo�^��A�ēx�폜���������s���ĉ�����");
                        }
                    }
                    //�˔p
                    else if (targetData.CarPurchaseType.Equals(PURCHASETYPE_DIPOSAL))
                    {
                         ModelState.AddModelError("CarPurchaseType", "���ɋ敪���˔p�œo�^����Ă���d���f�[�^�͍폜�ł��܂���B���ɋ敪������ԁA�˔p�ȊO�œo�^��A�ēx�폜���������s���ĉ�����");
                    }
                }
            }
        }
        #endregion

    }
}
