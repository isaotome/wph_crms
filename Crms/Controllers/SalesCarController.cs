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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �ԗ��}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SalesCarController : Controller {

        #region ������
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ��}�X�^�o�^"; // ������

        private static readonly string CARPURCHASE_NOTPURCHASED = "001";  //�ԗ��d���X�e�[�^�X = ���d�� //Add 2022/01/13 yano #4123
      
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SalesCarController() {
            db = CrmsDataContext.GetDataContext();
        }
        #endregion

        #region ����
        /// <summary>
        /// �ԗ�������ʕ\��
        /// </summary>
        /// <returns>�ԗ��������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ�������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // �X�e�[�^�X�ύX����
            if (form["action"].Equals("change")) {
                string[] targetArr = CommonUtils.DefaultString(form["chkTarget"]).Replace("false", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                string carStatus = form["ChangeCarStatus"];
                foreach (string target in targetArr) {
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(target);
                    salesCar.CarStatus = carStatus;
                    for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++) {
                        EditSalesCarForUpdate(salesCar);
                        try {
                            db.SubmitChanges();
                            break;
                        } catch (ChangeConflictException e) {
                            //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�͏����ǉ�
                            foreach (ObjectChangeConflict occ in db.ChangeConflicts) {
                                occ.Resolve(RefreshMode.KeepCurrentValues);
                            }
                            if (i + 1 >= DaoConst.MAX_RETRY_COUNT) {
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ���O�ɏo��
                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                                return View("Error");
                            }
                        }
                        catch (Exception ex)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                }
            }

            // �������ʃ��X�g�̎擾
            PaginatedList<SalesCar> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorType"], true);
            ViewData["ExteriorColorCode"] = form["ExteriorColorCode"];
            ViewData["ExteriorColorName"] = form["ExteriorColorName"];
            ViewData["InteriorColorCode"] = form["InteriorColorCode"];
            ViewData["InteriorColorName"] = form["InteriorColorName"];
            ViewData["ManufacturingYear"] = form["ManufacturingYear"];
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            ViewData["LocationName"] = form["LocationName"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), form["Steering"], true);
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["ChangeCarStatus"], true);
            ViewData["UserName"] = form["UserName"];
            ViewData["UserNameKana"] = form["UserNameKana"];

            ////Mod 2014/10/16 arc yano �ԗ��X�e�[�^�X�ǉ��Ή�
            ViewData["CarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("004", false), form["CarUsage"], true);

            // �ԗ�������ʂ̕\��
            return View("SalesCarCriteria", list);
        }

        /// <summary>
        /// �ԗ������_�C�A���O�\��
        /// </summary>
        /// <returns>�ԗ������_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �ԗ������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ�������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["SalesCarNumber"] = Request["SalesCarNumber"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["CarName"] = Request["CarName"];
            form["CarGradeName"] = Request["CarGradeName"];
            form["NewUsedType"] = Request["NewUsedType"];
            form["ColorType"] = Request["ColorType"];
            form["ExteriorColorCode"] = Request["ExteriorColorCode"];
            form["ExteriorColorName"] = Request["ExteriorColorName"];
            form["InteriorColorCode"] = Request["InteriorColorCode"];
            form["InteriorColorName"] = Request["InteriorColorName"];
            form["ManufacturingYear"] = Request["ManufacturingYear"];
            form["CarStatus"] = Request["CarStatus"];
            //Add 2014/10/16 arc yano �ԗ��X�e�[�^�X�ǉ��Ή� ���������ɗ��p�p�r(CarUsage)��ǉ� 
            form["CarUsage"] = Request["CarUsage"];
            form["LocationName"] = Request["LocationName"];
            form["CustomerName"] = Request["CustomerName"];
            form["Vin"] = Request["Vin"];
            form["MorterViecleOfficialCode"] = Request["MorterViecleOfficialCode"];
            form["RegistrationNumberType"] = Request["RegistrationNumberType"];
            form["RegistrationNumberKana"] = Request["RegistrationNumberKana"];
            form["RegistrationNumberPlate"] = Request["RegistrationNumberPlate"];
            form["Steering"] = Request["Steering"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["UserName"] = Request["UserName"];
            form["UserNameKana"] = Request["UserNameKana"];

            // �������ʃ��X�g�̎擾
            PaginatedList<SalesCar> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["SalesCarNumber"] = form["SalesCarNumber"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), form["ColorType"], true);
            ViewData["ExteriorColorCode"] = form["ExteriorColorCode"];
            ViewData["ExteriorColorName"] = form["ExteriorColorName"];
            ViewData["InteriorColorCode"] = form["InteriorColorCode"];
            ViewData["InteriorColorName"] = form["InteriorColorName"];
            ViewData["ManufacturingYear"] = form["ManufacturingYear"];
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), form["CarStatus"], true);
            //Add 2014/10/16 arc yano �ԗ��X�e�[�^�X�ǉ��Ή� ���������ɗ��p�p�r(CarUsage)��ǉ� 
            ViewData["CarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCodeName("004", false), form["CarUsage"], true);
            ViewData["LocationName"] = form["LocationName"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["MorterViecleOfficialCode"] = form["MorterViecleOfficialCode"];
            ViewData["RegistrationNumberType"] = form["RegistrationNumberType"];
            ViewData["RegistrationNumberKana"] = form["RegistrationNumberKana"];
            ViewData["RegistrationNumberPlate"] = form["RegistrationNumberPlate"];
            ViewData["SteeringList"] = CodeUtils.GetSelectListByModel(dao.GetSteeringAll(false), form["Steering"], true);

            // �ԗ�������ʂ̕\��
            return View("SalesCarCriteriaDialog", list);
        }
        
        /*  //Del 2016/08/13 arc yano ���g�p�̂��߁A�R�����g�A�E�g
        /// <summary>
        /// �݌ɕ\
        /// </summary>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult List() {
            criteriaInit = true;
            return List(new FormCollection());
        }

        /// <summary>
        /// �݌ɕ\����
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult List(FormCollection form) {

            // ���O�C���҂̕���
            Department selfDepartment = ((Employee)Session["Employee"]).Department1;
            if (criteriaInit) {
                form["DepartmentCode"] = selfDepartment.DepartmentCode;
            }
            PaginatedList<SalesCar> list = new PaginatedList<SalesCar>();
            // �����傪�c�ƓX�܁A�t�H�[���ŕ����I�����Ă���ꍇ�̂ݑI��
            if (selfDepartment.BusinessType != null && (selfDepartment.BusinessType.Equals("001") || selfDepartment.BusinessType.Equals("009")) && !string.IsNullOrEmpty(form["DepartmentCode"])) {
                SalesCarDao dao = new SalesCarDao(db);
                list = dao.GetStockList(form["DepartmentCode"], form["NewUsedType"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
            // �c�Ƃ̂�
            List<Department> department = new DepartmentDao(db).GetListAll("001");
            List<Department> honbu = new DepartmentDao(db).GetListAll("009");
            department.AddRange(honbu);
            List<CodeData> dataList = new List<CodeData>();
            foreach (var item in department) {
                dataList.Add(new CodeData { Code = item.DepartmentCode, Name = item.DepartmentName });
            }
            
            ViewData["DepartmentList"] = CodeUtils.GetSelectList(dataList, form["DepartmentCode"], true);
            //ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetNewUsedTypeAll(false), form["NewUsedType"], true);
            ViewData["NewUsedType"] = form["NewUsedType"];
            return View("SalesCarList", list);
        }
        */
        #endregion

        #region ����
        /// <summary>
        /// �ԗ��}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ��R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�ԗ��}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            ViewData["Master"] = Request["Master"];
            SalesCar salesCar;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {
                salesCar = new SalesCar();
                ViewData["update"] = "0";
                ViewData["LocationName"] = "";
            }
                // �X�V�̏ꍇ
            else {
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                salesCar = new SalesCarDao(db).GetByKey(id, true);
                salesCar.OwnershipChangeDate = DateTime.Today;
                ViewData["Closed"] = "";
                //try {
                //    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(salesCar.Location.DepartmentCode, salesCar.SalesDate, "001")) {
                //        ViewData["Closed"] = "1";
                //    }
                //} catch { }
                ViewData["update"] = "1";
                ViewData["LocationName"] = "";
                try { ViewData["LocationName"] = salesCar.Location.LocationName; } catch (NullReferenceException) { }
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(salesCar);

            // �o��
            return View("SalesCarEntry", salesCar);
        }

        /// <summary>
        /// �ԗ��}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="salesCar">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��}�X�^���͉��</returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(SalesCar salesCar, FormCollection form) {

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["Master"] = string.IsNullOrEmpty(form["Master"]) ? null : form["Master"];

            // �f�[�^�`�F�b�N
            ValidateSalesCar(salesCar);
            if (!ModelState.IsValid) {
                GetEntryViewData(salesCar);
                return View("SalesCarEntry", salesCar);
            }

            // �a��𐼗�ɕϊ�
            if (!salesCar.IssueDateWareki.IsNull) {
                salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.IssueDate = JapaneseDateUtility.GetGlobalDate(salesCar.IssueDateWareki);
            }
            if (!salesCar.RegistrationDateWareki.IsNull) {
                salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //salesCar.RegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.RegistrationDateWareki);
            }
            salesCar.FirstRegistrationDateWareki.Day = 1; 
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {

                DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);  //Mod 2018/06/22 arc yano #3891
                //DateTime? firstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki);

                if (firstRegistrationDate.HasValue) {
                    salesCar.FirstRegistrationYear = firstRegistrationDate.Value.Year + "/" + firstRegistrationDate.Value.Month;
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull) {
                salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.ExpireDate = JapaneseDateUtility.GetGlobalDate(salesCar.ExpireDateWareki);
            }
            if (!salesCar.SalesDateWareki.IsNull) {
                salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki, db);   //Mod 2018/06/22 arc yano #3891
                //salesCar.SalesDate = JapaneseDateUtility.GetGlobalDate(salesCar.SalesDateWareki);
            }
            if (!salesCar.InspectionDateWareki.IsNull) {
                salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki, db);    //Mod 2018/06/22 arc yano #3891
                //salesCar.InspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.InspectionDateWareki);
            }
            if (!salesCar.NextInspectionDateWareki.IsNull) {
                salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki, db); //Mod 2018/06/22 arc yano #3891
                //salesCar.NextInspectionDate = JapaneseDateUtility.GetGlobalDate(salesCar.NextInspectionDateWareki);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1")) {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                SalesCar targetSalesCar = new SalesCarDao(db).GetByKey(salesCar.SalesCarNumber, true);
                targetSalesCar.OwnershipChangeDate = salesCar.OwnershipChangeDate;
                targetSalesCar.OwnershipChangeMemo = salesCar.OwnershipChangeMemo;
                targetSalesCar.OwnershipChangeType = salesCar.OwnershipChangeType;
                targetSalesCar.IssueDate = salesCar.IssueDate;
                targetSalesCar.RegistrationDate = salesCar.RegistrationDate;
                targetSalesCar.FirstRegistrationYear = salesCar.FirstRegistrationYear;
                targetSalesCar.ExpireDate = salesCar.ExpireDate;
                targetSalesCar.SalesDate = salesCar.SalesDate;
                targetSalesCar.InspectionDate = salesCar.InspectionDate;
                targetSalesCar.NextInspectionDate = salesCar.NextInspectionDate;

                // �����e�[�u���ɃR�s�[
                CommonUtils.CopyToSalesCarHistory(db, targetSalesCar);

                UpdateModel(targetSalesCar);
                EditSalesCarForUpdate(targetSalesCar);
                
            }
                // �f�[�^�ǉ�����
            else {
                ValidateForInsert(salesCar);
                if (!ModelState.IsValid) {
                    GetEntryViewData(salesCar);
                    return View("SalesCarEntry", salesCar);
                }
                // �f�[�^�ҏW
                salesCar = EditSalesCarForInsert(salesCar);

                // �f�[�^�ǉ�
                db.SalesCar.InsertOnSubmit(salesCar);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
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
                    //Add 2014/08/04 arc amii �G���[���O�Ή� �Z�b�V������SQL����o�^���鏈���ǉ�
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/01 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                        GetEntryViewData(salesCar);
                        return View("SalesCarEntry", salesCar);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii �G���[���O�Ή� �wtheow e�x����G���[�y�[�W�J�ڂɕύX
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/11/04 ishii �ۑ��{�^���Ή�
            ModelState.Clear();
            // �o��
            //ViewData["close"] = "1";
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //return Entry((string)null);
            return Entry(salesCar.SalesCarNumber);
        }

        /// <summary>
        /// �ԗ��}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="salesCar">�ԗ��f�[�^(�o�^���e)</param>
        /// <returns>�ԗ��}�X�^���f���N���X</returns>
        /// <history>
        /// 2020/11/27 yano #4072 �����@�^�����̓G���A�̊g�� �ő啶�����ł̐؂��菈���̔p�~
        /// 2019/5/23 yano #3992 �ԗ��}�X�^�z�����@�̌^���̍ő啶�����̕ύX(10��15)
        /// </history>
        private SalesCar EditSalesCarForInsert(SalesCar salesCar) {

            string companyCode = "N/A";
            try { companyCode = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode).Car.Brand.CompanyCode; } catch (NullReferenceException) { }
            salesCar.SalesCarNumber = new SerialNumberDao(db).GetNewSalesCarNumber(companyCode, salesCar.NewUsedType);
            //ADD 2014/10/16 arc ishii VIN��S�p->���p�ϊ�,�������ˑ啶���ϊ�����
            // salesCar.Vin = CommonUtils.abc123ToHankaku(salesCar.Vin);
            salesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.Vin));
            //ADD 2014/10/22 arc ishii Vin��20�����ȏ�̏ꍇ������20�����ڂ܂Ŕ����o��
            if (salesCar.Vin.Length > 20)
            {
                salesCar.Vin = salesCar.Vin.Substring(0, 20);
            }
            //ADD 2014/10/21 arc ishii EngineType��S�p->���p�ϊ�,�������ˑ啶���ϊ�����
            salesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.EngineType));


            //Mod 2020/11/27 yano #4072
            //Mod 2019/5/23 yano #3992 15�����ȏ�̏ꍇ��15�����܂Ő؂�o��
            //Add 2015/03/26 arc iijima Null�����Ή��̂��ߔ���ǉ�
            //ADD 2014/10/22 arc ishii EngineType��10�����ȏ�̏ꍇ������10�����ڂ܂Ŕ����o��
            //if ((!string.IsNullOrWhiteSpace(salesCar.EngineType)) && (salesCar.EngineType.Length > 15))
            //{
            //    salesCar.EngineType = salesCar.EngineType.Substring(0, 15);
            //}
            salesCar.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.CreateDate = DateTime.Now;
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            salesCar.DelFlag = "0";
            return salesCar;
        }

        /// <summary>
        /// �ԗ��}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="salesCar">�ԗ��f�[�^(�o�^���e)</param>
        /// <returns>�ԗ��}�X�^���f���N���X</returns>
        /// <history>
        ///  2020/11/27 yano #4072 �����@�^�����̓G���A�̊g�� �ő啶�����ł̐؂��菈���̔p�~
        /// 2019/5/23 yano #3992 �ԗ��}�X�^�z�����@�̌^���̍ő啶�����̕ύX(10��15)
        /// </history>
        private SalesCar EditSalesCarForUpdate(SalesCar salesCar) {

            // VIN��S�p->���p�ϊ�,�������ˑ啶���ϊ����� ADD 2014/10/16 arc ishii 
            //salesCar.Vin = CommonUtils.abc123ToHankaku(salesCar.Vin);
            salesCar.Vin = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.Vin));
            //Mod 2015/02/16 arc yano Vin��null�܂��͋󕶎��̏ꍇ�͏������s��Ȃ�
            //ADD 2014/10/22 arc ishii Vin��20�����ȏ�̏ꍇ������20�����ڂ܂Ŕ����o��
            if ((!string.IsNullOrWhiteSpace(salesCar.Vin)) && (salesCar.Vin.Length > 20) )
            {
                salesCar.Vin = salesCar.Vin.Substring(0, 20);
            }
            //ADD 2014/10/21 arc ishii EngineType��S�p->���p�ϊ�,�������ˑ啶���ϊ�����
            salesCar.EngineType = CommonUtils.myReplacer(CommonUtils.LowercaseToUppercase(salesCar.EngineType));

            //Mod 2020/11/27 yano #4072
            //Mod 2019/5/23 yano #3992 15�����ȏ�̏ꍇ��15�����܂Ő؂�o��
            //Mod 2015/02/16 arc yano Vin��null�܂��͋󕶎��̏ꍇ�͏������s��Ȃ�
            //ADD 2014/10/22 arc ishii EngineType��10�����ȏ�̏ꍇ������10�����ڂ܂Ŕ����o��
            //if ((!string.IsNullOrWhiteSpace(salesCar.EngineType)) && (salesCar.EngineType.Length > 15))
            //{
            //    salesCar.EngineType = salesCar.EngineType.Substring(0, 15);
            //}
            salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            salesCar.LastUpdateDate = DateTime.Now;
            return salesCar;
        }
        #endregion

        #region Ajax
        /// <summary>
        /// �ԗ��R�[�h����ԗ������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ԗ��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null) {
                    codeData.Code = salesCar.SalesCarNumber;
                    codeData.Name = salesCar.Vin;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }


       /// <summary>
       /// �O���[�h�R�[�h���玩���ԐŊ����\�����擾����(Ajax��p)
       /// </summary>
       /// <param name="code">�O���[�h�R�[�h</param>
       /// <param name="optionAmount">���[�J�[�I�v�V�������v</param>
       /// <param name="taxid">�����\���E�ŗ�ID</param>
       /// <param name="requestregistdate">�o�^��]��</param>
       /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
       /// <history>
       /// 2019/10/17 yano #4022 �y�ԗ��`�[���́z����̏������ł̊����\���̌v�Z
       /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C��Ɓ@�߂�l�̌^�̕ύX
        /// 2014/05/27 arc yano vs2012�Ή�
       /// </history>
       [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", DateTime? requestregistdate = null)
        //public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", decimal? salesPrice = null)
        {
            //Add 2019/09/04 yano #4011
            decimal amount = 0m;
            SalesCar salesCar = new SalesCar();

            if (Request.IsAjaxRequest())
            {
                salesCar = new SalesCarDao(db).GetByKey(code);

                //Mod 2019/10/17 yano #4022
                //�V�ԁA���ÎԂɌ��炸�O���[�h�}�X�^�̔̔����i��ݒ肷��
                try
                {
                    amount = (salesCar.CarGrade.SalesPrice ?? 0m);
                }
                catch
                {
                }

                ////Mod 2019/09/04 yano #4011
                ////���ÎԂ̏ꍇ�A�܂��͔̔����i��NULL�A�󕶎��̏ꍇ�̓O���[�h�}�X�^����ݒ�
                //if (salesCar.NewUsedType.Equals("U") || salesPrice == null)
                //{
                //    try
                //    {
                //        amount = (salesCar.CarGrade.SalesPrice ?? 0m);
                //    }
                //    catch
                //    {
                //    }
                //}
                //else
                //{
                //    amount = (salesPrice ?? 0m);
                //}

                //Mod 2019/10/17 yano #4022
                Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear, taxid, requestregistdate);
                //Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear, taxid);

                return Json(acquisitionTax);
            }

            return new EmptyResult();
        }
        /// <summary>
        /// �ԗ��R�[�h����ԗ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ԗ��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2022/07/06 yano #4145�y�T�[�r�X�`�[�z�ԑ�ԍ����͂����ۂɌڋq��񂪕\������Ȃ��s��̑Ή�
        /// 2022/01/13 yano #4123 �y�T�[�r�X�`�[���́z���d���̎ԗ����I���ł���s��̑Ή�
        /// 2020/06/09 yano #4052 �ԗ��`�[���́z�ԑ�ԍ����͎��̃`�F�b�N�R��Ή�
        /// 2019/10/22 yano #4024 �y�ԗ��`�[���́z�I�v�V�����s�ǉ��E�폜���ɃG���[�����������̕s��Ή�
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �V�K�쐬
        /// </history>
       public ActionResult GetMasterDetail(string code, string SelectByCarSlip = "0")
       {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> retCar = new Dictionary<string, string>();
                SalesCar salesCar = new SalesCarDao(db).GetByKey(code);
                if (salesCar != null) {
                    
                    retCar.Add("CarGradeCode", salesCar.CarGradeCode);
                    retCar.Add("MakerName", salesCar.CarGrade.Car.Brand.Maker.MakerName);
                    retCar.Add("CarBrandName", salesCar.CarGrade.Car.Brand.CarBrandName);
                    retCar.Add("CarGradeName", salesCar.CarGrade.CarGradeName);
                    retCar.Add("CarName", salesCar.CarGrade.Car.CarName);
                    retCar.Add("ExteriorColorCode", salesCar.ExteriorColorCode);
                    retCar.Add("ExteriorColorName", salesCar.ExteriorColorName==null ? "" : salesCar.ExteriorColorName);
                    retCar.Add("InteriorColorCode", salesCar.InteriorColorCode);
                    retCar.Add("InteriorColorName", salesCar.InteriorColorName==null ? "" : salesCar.InteriorColorName);
                    retCar.Add("Mileage", salesCar.Mileage.ToString());
                    retCar.Add("MileageUnit", salesCar.MileageUnit);
                    retCar.Add("ModelName", salesCar.ModelName);
                    retCar.Add("NewUsedType", salesCar.NewUsedType);
                    retCar.Add("SalesPrice", salesCar.SalesPrice.ToString());
                    //Mod 2015/07/28 arc nakayama #3217_�f���J�[���̔��ł��Ă��܂����̉��P �@�ԗ��`�[�̎ԑ�ԍ��̃��b�N�A�b�v����Ă΂ꂽ���@���@�݌ɃX�e�[�^�X���݌ɂ̎������Ǘ��ԍ���Ԃ�
                    //Mod 2015/08/20 arc nakayama #3242_�T�[�r�X�`�[�Ŏԗ��}�X�^�{�^���������Ă��ԗ��}�X�^���\������Ȃ� �ԗ��`�[����Ă΂ꂽ�������݌ɃX�e�[�^�X������悤�ɏC��
                    if (SelectByCarSlip.ToString().Equals("1"))
                    {
                        //if (salesCar.CarStatus.Equals("001"))
                        if (salesCar.CarStatus == null || salesCar.CarStatus.Equals("001"))   //Mod 2019/10/22 yano #4024
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
                    retCar.Add("LocationName", salesCar.Location!=null ? salesCar.Location.LocationName : "");
                    retCar.Add("InheritedInsuranceFee", "");
                    //retCar.Add("CarWeightTax", salesCar.CarWeightTax.ToString());
                    retCar.Add("RecycleDeposit", salesCar.RecycleDeposit.ToString());
                    //retCar.Add("CarLiabilityInsurance", salesCar.CarLiabilityInsurance.ToString());

                    retCar.Add("EngineType", salesCar.EngineType);
                    retCar.Add("FirstRegistration", salesCar.FirstRegistrationYear);
                    retCar.Add("NextInspectionDate", string.Format("{0:yyyy/MM/dd}",salesCar.NextInspectionDate));
                    retCar.Add("InspectionExpireDate", string.Format("{0:yyyy/MM/dd}", salesCar.ExpireDate));
                    retCar.Add("UsVin", salesCar.UsVin);
                    retCar.Add("MorterViecleOfficialCode", salesCar.MorterViecleOfficialCode);
                    retCar.Add("RegistrationNumberType", salesCar.RegistrationNumberType);
                    retCar.Add("RegistrationNumberKana", salesCar.RegistrationNumberKana);
                    retCar.Add("RegistrationNumberPlate", salesCar.RegistrationNumberPlate);
                    retCar.Add("CustomerCode", salesCar.UserCode);
                    retCar.Add("CustomerName", salesCar.User != null ? salesCar.User.CustomerName : "");
                    retCar.Add("CustomerNameKana", salesCar.User != null ? salesCar.User.CustomerNameKana : "");
                    retCar.Add("CustomerAddress", salesCar.User != null ? salesCar.User.Prefecture + salesCar.User.City + salesCar.User.Address1 + salesCar.User.Address2 : "");

                    retCar.Add("CustomerMemo", salesCar.User != null ? salesCar.User.Memo : "");    //Add 2022/07/06 yano #4145


                    retCar.Add("LaborRate",salesCar.CarGrade!=null && salesCar.CarGrade.Car!=null && salesCar.CarGrade.Car.Brand!=null ? salesCar.CarGrade.Car.Brand.LaborRate.ToString() : "");

                    // Mod 2015/09/14 arc yano #3252 �T�[�r�X�`�[���͉�ʂ̃}�X�^�{�^���̋���(�ގ��Ή�) �ԑ�ԍ�����ԗ������擾�ł��鍀�ڂ̒ǉ�
                    retCar.Add("RegistrationDate", string.Format("{0:yyyy/MM/dd}", salesCar.RegistrationDate));         //�o�^�N����
                    retCar.Add("CustomerTelNumber", salesCar.User != null ? salesCar.User.TelNumber : "");              //�d�b�ԍ�  //Mod 2015/09/17 arc yano  #3261 �ԗ��`�[�̎ԗ��I���Łu�}�X�^�擾�Ɏ��s���܂����v�ƕ\�� NULL�̏ꍇ�͋󕶎��ɕϊ�

                    //Add 2017/05/10 arc yano #3762
                    retCar.Add("RegistrationNumber", salesCar.MorterViecleOfficialCode + " " + salesCar.RegistrationNumberType + " " + salesCar.RegistrationNumberKana + " " + salesCar.RegistrationNumberPlate);
                    retCar.Add("ColorType", salesCar.c_ColorCategory != null ? salesCar.c_ColorCategory.Name : "");



                    //Mod 2022/01/13 yano #4123
                    //Add 2020/06/09 yano #4052
                    CarPurchase rec = new CarPurchaseDao(db).GetBySalesCarNumber(code);
                    
                    //�Ώۂ̎d���f�[�^�����d���̏ꍇ
                    if(rec != null && rec.PurchaseStatus.Equals(CARPURCHASE_NOTPURCHASED))
                    {
                        salesCar.CarStatus = "999";       //���d��
                    }
                    
                    retCar.Add("CarStatus", (salesCar.CarStatus ?? ""));

                    //���N�x�o�^(yyyy/mm)����N�����擾
                    decimal fee = 0m;
                    string firstRegistrationYear = salesCar.FirstRegistrationYear;
                    if (!string.IsNullOrEmpty(firstRegistrationYear)) {
                        if (firstRegistrationYear.Split('/').Length == 2) {
                            string year = salesCar.FirstRegistrationYear.Split('/')[0];
                            string month = salesCar.FirstRegistrationYear.Split('/')[1];
                            DateTime firstRegist = new DateTime(int.Parse(year), int.Parse(month), 1);
                            DateTime today = DateTime.Today;
                            try {
                                if (firstRegist.AddMonths(24).CompareTo(today) > 0) {
                                    //24��������
                                    fee = salesCar.CarGrade.Under24 ?? 0;
                                } else if (firstRegist.AddMonths(26).CompareTo(today) > 0) {
                                    //26��������
                                    fee = salesCar.CarGrade.Under26 ?? 0;
                                } else if (firstRegist.AddMonths(28).CompareTo(today) > 0){
                                    //28��������
                                    fee = salesCar.CarGrade.Under28 ?? 0;
                                } else if (firstRegist.AddMonths(30).CompareTo(today) > 0) {
                                    //30��������
                                    fee = salesCar.CarGrade.Under30 ?? 0;
                                } else if (firstRegist.AddMonths(36).CompareTo(today) > 0) {
                                    //36��������
                                    fee = salesCar.CarGrade.Under36 ?? 0;
                                } else if (firstRegist.AddMonths(72).CompareTo(today) > 0) {
                                    //72��������
                                    fee = salesCar.CarGrade.Under72 ?? 0;
                                } else if (firstRegist.AddMonths(84).CompareTo(today) > 0) {
                                    //84��������
                                    fee = salesCar.CarGrade.Under84 ?? 0;
                                } else {
                                    //84�����ȏ�
                                    fee = salesCar.CarGrade.Over84 ?? 0;
                                }
                            } catch (NullReferenceException) {
                            }
                            
                        }
                    }

                    retCar.Add("TradeInMaintenanceFee", fee.ToString());

                    //�r�C�ʂ��玩���Ԑł��擾����
                    //CarTax carTax = new CarTaxDao(db).GetByDisplacement(salesCar.Displacement ?? 0);
                    //retCar.Add("CarTax", carTax!=null ? carTax.Amount.ToString() : "0");

                    //�����Ԋ����\��
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax((salesCar.CarGrade.SalesPrice ?? 0m), 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                    retCar.Add("EPDiscountTaxList", retValue.Item1);
                    retCar.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2));
                    //retCar.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear)));
                    
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    //�����ӕی�������яd�ʐ�
                    if (CommonUtils.DefaultString(salesCar.NewUsedType).Equals("N"))
                    {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance!=null ? insurance.Amount : 0m));
                        CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarGrade.CarWeight ?? 0);
                        retCar.Add("CarWeightTax",string.Format("{0:0}",weightTax!=null ? weightTax.Amount : 0 ));
                    } else {
                        CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByUsedDefault();
                        retCar.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance!=null ? insurance.Amount : 0m));
                    }
                }
                return Json(retCar);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �Ԍ��L���������玟��Ԍ������v�Z���ĕԂ�
        /// </summary>
        /// <param name="registrationNumberType">�o�^�ԍ��i��ʁj</param>
        /// <param name="gengou">����</param>
        /// <param name="year">�N�i�a��j</param>
        /// <param name="month">���i�a��j</param>
        /// <param name="day">���i�a��j</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetNextInspectionDate(string registrationNumberType, DateTime? expireDate, int? gengou, int? year, int? month, int? day) {
            if (expireDate == null) {
                expireDate = JapaneseDateUtility.GetGlobalDate(gengou, year, month, day, db);   //Mod 2018/06/22 arc yano #3891
                //expireDate = JapaneseDateUtility.GetGlobalDate(gengou, year, month, day);
            }
            if (expireDate == null) return new EmptyResult();
            
            DateTime returnDate;

            if (!string.IsNullOrEmpty(registrationNumberType) && (registrationNumberType.Substring(0, 1).Equals("1") || registrationNumberType.Substring(0, 1).Equals("4"))) {
                if (expireDate.Value.AddMonths(-18) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-18);
                } else if (expireDate.Value.AddMonths(-12) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-12);
                } else if (expireDate.Value.AddMonths(-6) > DateTime.Today) {
                    returnDate = expireDate.Value.AddMonths(-6);
                } else {
                    returnDate = expireDate.Value.AddMonths(6);
                }
            } else {
                if (expireDate.Value.AddYears(-2) > DateTime.Today) {
                    returnDate = expireDate.Value.AddYears(-2);
                } else if (expireDate.Value.AddYears(-1) > DateTime.Today) {
                    returnDate = expireDate.Value.AddYears(-1);
                } else {
                    returnDate = expireDate.Value.AddYears(1);
                }
            }


            CodeDao dao = new CodeDao(db);
            JapaneseDate returnDateWareki = JapaneseDateUtility.GetJapaneseDate(returnDate);
            Dictionary<string, string> nextInspectionDate = new Dictionary<string, string>();
            nextInspectionDate.Add("Seireki", string.Format("{0:yyyy/MM/dd}",returnDate));
            nextInspectionDate.Add("GengouName", CodeUtils.GetName(CodeUtils.GetGengouList(db), returnDateWareki.Gengou.ToString()));   //Mod 2018/06/22 arc yano #3891
            //nextInspectionDate.Add("GengouName", CodeUtils.GetName(CodeUtils.GetGengouList(), returnDateWareki.Gengou.ToString()));
            nextInspectionDate.Add("Gengou", returnDateWareki.Gengou.ToString());
            nextInspectionDate.Add("Year", returnDateWareki.Year.ToString());
            nextInspectionDate.Add("Month", returnDateWareki.Month.ToString());
            nextInspectionDate.Add("Day", returnDateWareki.Day.ToString());
            return Json(nextInspectionDate);
        }


        // Add 
        /// <summary>
        /// �ԑ�ԍ�����Ǘ��ԍ����擾�E�\������
        /// </summary>
        /// <param name="vinCode">�ԑ�ԍ�</param>
        /// <returns></returns>
        /// <history>
        /// 2022/01/08 yano #4121 �y�T�[�r�X�`�[���́zChrome�E���׍s�̕��i�݌ɏ��擾�̕s��Ή�
        /// 2020/06/09 yano #4052 �ԗ��`�[���́z�ԑ�ԍ����͎��̃`�F�b�N�R��Ή�
        /// 2014/07/24 arc amii �����o�O�Ή� �ԑ�ԍ�������͂������A�Ǘ��ԍ����擾���鏈����ǉ�
        /// </history>
        public ActionResult GetSalesCarNumberFromVin(string vinCode)
        {
            if (Request.IsAjaxRequest())
            {

                // �ԑ�ԍ����L�[�Ƀ��R�[�h���擾
                List<SalesCar> salesCarList = new SalesCarDao(db).GetByVin(vinCode);
                Dictionary<string, string> ret = new Dictionary<string, string>();

                string cnt = "0";
                string vin = "";
                string number = "";
                //string status = "";   //Mod 2022/01/08 yano #4121

                // �f�[�^������ꍇ�A�ԑ�ԍ��ƊǗ��ԍ��ƌ�����ݒ肷��
                if (salesCarList != null && salesCarList.Count > 0)
                {
                    cnt = salesCarList.Count.ToString();              // ����
                    vin = salesCarList[0].Vin;                        // �ԑ�ԍ�
                    number = salesCarList[0].SalesCarNumber;          // �Ǘ��ԍ�
                    //status = (salesCarList[0].CarStatus ?? "");       // �݌ɃX�e�[�^�X  //Mod 2022/01/08 yano #4121  //Add 2020/06/09 yano #4052
                }

                ret.Add("vin", vin);
                ret.Add("salesCarNumber", number);
                ret.Add("count", cnt);
                //ret.Add("status", status);                          //Mod 2022/01/08 yano #4121 //Add 2020/06/09 yano #4052

                return Json(ret);
            }
            
            return new EmptyResult();
        }


    #endregion

          #region ��ʃf�[�^�擾
          /// <summary>
          /// ��ʕ\���f�[�^�̎擾
          /// </summary>
          /// <param name="salesCar">���f���f�[�^</param>
          /// <history>
          /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
          /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
          /// </history>
          private void GetEntryViewData(SalesCar salesCar) {

            // �u�����h���C�Ԏ햼�C�O���[�h���̎擾
            if (!string.IsNullOrEmpty(salesCar.CarGradeCode)) {
                CarGradeDao carGradeDao = new CarGradeDao(db);
                CarGrade carGrade = carGradeDao.GetByKey(salesCar.CarGradeCode);
                if (carGrade != null) {
                    ViewData["CarGradeName"] = carGrade.CarGradeName;
                    try { ViewData["CarName"] = carGrade.Car.CarName; } catch (NullReferenceException) { }
                    try { ViewData["CarBrandName"] = carGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                }
            }

            // ���q�l�w��I�C���C�^�C�����̎擾
            PartsDao partsDao = new PartsDao(db);
            Parts parts;
            if (!string.IsNullOrEmpty(salesCar.Oil)) {
                parts = partsDao.GetByKey(salesCar.Oil);
                if (parts != null) {
                    ViewData["OilName"] = parts.PartsNameJp;
                }
            }
            if (!string.IsNullOrEmpty(salesCar.Tire)) {
                parts = partsDao.GetByKey(salesCar.Tire);
                if (parts != null) {
                    ViewData["TireName"] = parts.PartsNameJp;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), salesCar.NewUsedType, true);
            ViewData["ColorTypeList"] = CodeUtils.GetSelectListByModel(dao.GetColorCategoryAll(false), salesCar.ColorType, true);
            ViewData["MileageUnitList"] = CodeUtils.GetSelectListByModel(dao.GetMileageUnitAll(false), salesCar.MileageUnit, false);
            ViewData["CarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);
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
            ViewData["OwnershipChangeTypeList"] = CodeUtils.GetSelectListByModel<c_OwnershipChangeType>(dao.GetOwnershipChangeTypeAll(false), salesCar.OwnershipChangeType, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel<c_Fuel>(dao.GetFuelTypeAll(false), salesCar.Fuel, true);
            //Add 2014/08/15 arc amii DM�t���O�@�\�g���Ή� #3069 �R���{�{�b�N�X�ɐݒ肷��l���擾���鏈����ǉ�
            //Mod 2014/09/08 arc amii DM�t���O�@�\�g���Ή� #3069 �R���{�{�b�N�X�̋󔒍s�����Ȃ��悤�C��
            ViewData["InspectGuidFlagList"] = CodeUtils.GetSelectListByModel(dao.GetNeededAll(false), salesCar.InspectGuidFlag, false);
            //Add 2014/08/15 arc amii �݌ɃX�e�[�^�X�ύX�Ή��Ή� #3071 ���͉�ʕ\�����A�݌ɃX�e�[�^�X�����擾���鏈����ǉ�
            ViewData["ChangeCarStatusList"] = CodeUtils.GetSelectListByModel(dao.GetCarStatusAll(false), salesCar.CarStatus, true);

            //Add 2014/10/16 arc yano �ԗ��X�e�[�^�X�ǉ��Ή��@���p�p�r�̃��X�g�{�b�N�X�ǉ�
            //Mod 2014/10/30 arc amii �ԗ��X�e�[�^�X�ǉ��Ή�
            ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("004", false), salesCar.CarUsage, true);
            //ViewData["ChangeCarUsageList"] = CodeUtils.GetSelectListByModel(dao.GetCarUsageAll(false), salesCar.CarUsage, true);

            //Add 2014/08/15 arc amii �݌ɃX�e�[�^�X�ύX�Ή� #3071 �Ǘ��Ҍ����̂ݍ݌ɃX�e�[�^�X�g�p�ɂ��鏈����ǉ�
            //Mod 2015/07/29 arc nakayama #3217_�f���J�[���̔��ł��Ă��܂����̉��P   �݌ɃX�e�[�^�X/���p�p�r�͕ύX�̌��������郆�[�U�[�̂ݎg�p�ɂ���B
            Employee emp = HttpContext.Session["Employee"] as Employee;
            //���O�C�����[�U���擾
            Employee loginUser = new EmployeeDao(db).GetByKey(emp.EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "ChangeSalesCarStatus"); //�ԗ��}�X�^�X�e�[�^�X�ύX���������邩�Ȃ���

            // �����������true�����łȂ����false 
            if (AppRole.EnableFlag){
                salesCar.CarStatusEnabled = true;
                salesCar.CarUsageEnabled = true;
            } else {
                salesCar.CarStatusEnabled = false;
                salesCar.CarUsageEnabled = false;
            }

            salesCar.IssueDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.IssueDate);
            string issueDateGengou = "";
            if (salesCar.IssueDateWareki != null) {
                issueDateGengou = salesCar.IssueDateWareki.Gengou.ToString();
            }
            ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), issueDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["IssueGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), issueDateGengou, false);

            salesCar.RegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.RegistrationDate);
            string registrationDateGengou = "";
            if (salesCar.RegistrationDateWareki != null) {
                registrationDateGengou = salesCar.RegistrationDateWareki.Gengou.ToString();
            }
            ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), registrationDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["RegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), registrationDateGengou, false);

            DateTime parseResult;
            DateTime? firstRegistrationDate = null;
            if (DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out parseResult)) {
                firstRegistrationDate = DateTime.Parse(salesCar.FirstRegistrationYear + "/01");
            }
            salesCar.FirstRegistrationDateWareki = JapaneseDateUtility.GetJapaneseDate(firstRegistrationDate);
            string firstRegistrationDateGengou = "";
            if (salesCar.FirstRegistrationDateWareki != null) {
                firstRegistrationDateGengou = salesCar.FirstRegistrationDateWareki.Gengou.ToString();
            }
            ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), firstRegistrationDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["FirstRegistrationGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), firstRegistrationDateGengou, false);

            salesCar.ExpireDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.ExpireDate);
            string expireDateGengou = "";
            if(salesCar.ExpireDate!=null){
                expireDateGengou = salesCar.ExpireDateWareki.Gengou.ToString();
            }

            ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), expireDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["ExpireGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), expireDateGengou, false);

            salesCar.SalesDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.SalesDate);
            string salesDateGengou = "";
            if (salesCar.SalesDate != null)
            {
                salesDateGengou = salesCar.SalesDateWareki.Gengou.ToString();
            }
            ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), salesDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["SalesGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), salesDateGengou, false);

            salesCar.InspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.InspectionDate);
            string inspectionDateGengou = "";
            if (salesCar.InspectionDate != null) {
                inspectionDateGengou = salesCar.InspectionDateWareki.Gengou.ToString();
            }
            ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), inspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
            //ViewData["InspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), inspectionDateGengou, false);

            salesCar.NextInspectionDateWareki = JapaneseDateUtility.GetJapaneseDate(salesCar.NextInspectionDate);
            string nextInspectionDateGengou = "";
            if (salesCar.NextInspectionDate != null) {
                nextInspectionDateGengou = salesCar.NextInspectionDateWareki.Gengou.ToString();
            }
            ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(db), nextInspectionDateGengou, false);   //Mod 2018/06/22 arc yano #3891
                                                                                                                                            //ViewData["NextInspectionGengouList"] = CodeUtils.GetSelectList(CodeUtils.GetGengouList(), nextInspectionDateGengou, false);
            ////Mod 2021/08/02 yano #4097
            ////Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̌�����4���𒴂��Ă����ꍇ�A4���ŕ\������
            //if (CommonUtils.DefaultString(salesCar.ManufacturingYear).Length > 10)
            //{
            //    salesCar.ManufacturingYear = salesCar.ManufacturingYear.Substring(0, 10);
            //}

        }
        #endregion

        #region ��������
        /// <summary>
        /// �ԗ��}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��}�X�^�������ʃ��X�g</returns>
        private PaginatedList<SalesCar> GetSearchResultList(FormCollection form) {

            SalesCarDao salesCarDao = new SalesCarDao(db);
            SalesCar salesCarCondition = new SalesCar();
            salesCarCondition.SalesCarNumber = form["SalesCarNumber"];
            salesCarCondition.NewUsedType = form["NewUsedType"];
            salesCarCondition.ColorType = form["ColorType"];
            salesCarCondition.ExteriorColorCode = form["ExteriorColorCode"];
            salesCarCondition.ExteriorColorName = form["ExteriorColorName"];
            salesCarCondition.InteriorColorCode = form["InteriorColorCode"];
            salesCarCondition.InteriorColorName = form["InteriorColorName"];
            salesCarCondition.ManufacturingYear = form["ManufacturingYear"];
            salesCarCondition.CarStatus = form["CarStatus"];
            salesCarCondition.Vin = form["Vin"];
            salesCarCondition.MorterViecleOfficialCode = form["MorterViecleOfficialCode"];
            salesCarCondition.RegistrationNumberType = form["RegistrationNumberType"];
            salesCarCondition.RegistrationNumberKana = form["RegistrationNumberKana"];
            salesCarCondition.RegistrationNumberPlate = form["RegistrationNumberPlate"];
            salesCarCondition.Steering = form["Steering"];
            salesCarCondition.CarGrade = new CarGrade();
            salesCarCondition.CarGrade.Car = new Car();
            salesCarCondition.CarGrade.Car.Brand = new Brand();
            salesCarCondition.CarGrade.Car.Brand.CarBrandName = form["CarBrandName"];
            salesCarCondition.CarGrade.Car.CarName = form["CarName"];
            salesCarCondition.CarGrade.CarGradeName = form["CarGradeName"];
            salesCarCondition.Location = new Location();
            salesCarCondition.Location.LocationName = form["LocationName"];
            salesCarCondition.Customer = new Customer();
            salesCarCondition.Customer.CustomerName = form["CustomerName"];
            salesCarCondition.User = new Customer();
            salesCarCondition.User.CustomerName = form["UserName"];
            salesCarCondition.User.CustomerNameKana = form["UserNameKana"];

            //Mod 2014/10/16 arc yano �ԗ��Ǘ��X�e�[�^�X�ǉ��Ή��@���������ɁA���p�p�r��ǉ�
            salesCarCondition.CarUsage = form["CarUsage"];

            if (!string.IsNullOrEmpty(form["DelFlag"]) && (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))) {
                salesCarCondition.DelFlag = form["DelFlag"];
            }
            return salesCarDao.GetListByCondition(salesCarCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region Validation
        /// <summary>
        /// �V�K�o�^���̃}�X�^���݃`�F�b�N
        /// </summary>
        /// <param name="salesCar"></param>
        private void ValidateForInsert(SalesCar salesCar) {
            List<SalesCar> list = new SalesCarDao(db).GetByVin(salesCar.Vin);
            if (list != null && list.Count > 0) {
                ModelState.AddModelError("Vin", "�ԑ�ԍ�:" + salesCar.Vin + "�͊��ɓo�^����Ă��܂�");
                ViewData["ErrorSalesCar"] = list;
            }
        }
        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="salesCar">�ԗ��f�[�^</param>
        /// <returns>�ԗ��f�[�^</returns>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// 2020/11/17 yano #4065 �y�ԗ��`�[���́z�����\���E�}�X�^�̐ݒ�l���s���̏ꍇ�̑Ή�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// 2018/06/22 arc yano #3891 �����Ή� DB����擾����悤�ɕύX
        /// 2018/04/26 arc yano #3816 �ԗ�������́@�Ǘ��ԍ���N/A�������Ă��܂�
        /// </history>
        private SalesCar ValidateSalesCar(SalesCar salesCar) {

            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(salesCar.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0001", "�O���[�h"));
            }
            else //Add 2018/04/26 arc yano #3816 �O���[�h�R�[�h�����͂���Ă���ꍇ�̓}�X�^�`�F�b�N���s��
            {
                CarGrade rec = new CarGradeDao(db).GetByKey(salesCar.CarGradeCode);

                if (rec == null)
                {
                    ModelState.AddModelError("CarGradeCode", "�ԗ��O���[�h�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ�����");
                }
            }

            if (string.IsNullOrEmpty(salesCar.NewUsedType)) {
                ModelState.AddModelError("NewUsedType", MessageUtils.GetMessage("E0001", "�V���敪"));
            }
            if (string.IsNullOrEmpty(salesCar.Vin)) {
                ModelState.AddModelError("Vin", MessageUtils.GetMessage("E0001", "�ԑ�ԍ�"));
            }
            // Add 2014/09/11 arc amii �Ԍ��ē��`�F�b�N�Ή� �Ԍ��ē�=�u�ہv�̏ꍇ�A���l���̕K�{�`�F�b�N���s��
            if (!CommonUtils.DefaultString(ViewData["PurchaseStatus"]).Equals("002"))
            {
                if (salesCar.InspectGuidFlag.Equals("002") && string.IsNullOrEmpty(salesCar.InspectGuidMemo))
                {
                    ModelState.AddModelError("InspectGuidMemo", MessageUtils.GetMessage("E0001", "�Ԍ��ē��������l��"));
                }
            }
           
            if (ViewData["update"] != null && ViewData["update"].Equals("1")){
                if (string.IsNullOrEmpty(salesCar.OwnershipChangeType) && string.IsNullOrEmpty(salesCar.OwnershipChangeMemo)) {
                    ModelState.AddModelError("OwnershipChangeType", MessageUtils.GetMessage("E0001", "�ύX�敪�܂��͕ύX���R�̂����ꂩ"));
                }
                if (!ModelState.IsValidField("OwnershipChangeDate")) {
                    ModelState.AddModelError("OwnershipChangeDate", MessageUtils.GetMessage("E0005", "�ύX��"));
                }
            }


            // �����`�F�b�N
            if (!ModelState.IsValidField("SalesPrice")) {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "���̐����̂�" }));
            }

            //2021/08/02 yano #4097
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
            if (!ModelState.IsValidField("ManufacturingYear"))
            {
                ModelState.AddModelError("ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "�N��", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
            }
            //if (!ModelState.IsValidField("IssueDate")) {
            //    ModelState.AddModelError("IssueDate", MessageUtils.GetMessage("E0005", "���s��"));
            //}
            //if (!ModelState.IsValidField("RegistrationDate")) {
            //    ModelState.AddModelError("RegistrationDate", MessageUtils.GetMessage("E0005", "�o�^��"));
            //}
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
                ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐���10���ȓ�������2���ȓ�" }));
            }
            //if (!ModelState.IsValidField("ExpireDate")) {
            //    ModelState.AddModelError("ExpireDate", MessageUtils.GetMessage("E0005", "�L������"));
            //}
            if (!ModelState.IsValidField("Mileage")) {
                ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
            }
            if (!ModelState.IsValidField("CarTax")) {
                ModelState.AddModelError("CarTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŎ�ʊ�", "���̐����̂�" }));    //Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("CarWeightTax")) {
                ModelState.AddModelError("CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԏd�ʐ�", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("AcquisitionTax")) {
                ModelState.AddModelError("AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŊ����\��", "���̐����̂�" }));//Mod 2019/09/04 yano #4011
            }
            if (!ModelState.IsValidField("CarLiabilityInsurance")) {
                ModelState.AddModelError("CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "�����ӕی���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SalesDate")) {
                ModelState.AddModelError("SalesDate", MessageUtils.GetMessage("E0005", "�[�ԓ�"));
            }
            if (!ModelState.IsValidField("InspectionDate")) {
                ModelState.AddModelError("InspectionDate", MessageUtils.GetMessage("E0005", "�_����"));
            }
            if (!ModelState.IsValidField("NextInspectionDate")) {
                ModelState.AddModelError("NextInspectionDate", MessageUtils.GetMessage("E0005", "����_����"));
            }
            if (!ModelState.IsValidField("ProductionDate")) {
                ModelState.AddModelError("ProductionDate", MessageUtils.GetMessage("E0005", "���Y��"));
            }
            if (!ModelState.IsValidField("ApprovedCarWarrantyDateFrom")){
                ModelState.AddModelError("ApprovedCarWarrantyDateFrom", MessageUtils.GetMessage("E0005", "�F�蒆�Îԕۏ؊���(�J�n)"));
            }
            if (!ModelState.IsValidField("ApprovedCarWarrantyDateTo")) {
                ModelState.AddModelError("ApprovedCarWarrantyDateTo", MessageUtils.GetMessage("E0005", "�F�蒆�Îԕۏ؊���(�I��)"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("SalesPrice") && salesCar.SalesPrice != null) {
                if (!Regex.IsMatch(salesCar.SalesPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�̔����i", "���̐����̂�" }));
                }
            }


            //Mod 2021/08/02 yano #4097
            //Add 2014/09/08 arc amii �N�����͑Ή� #3076 �N���̓��̓`�F�b�N(4�����l�ȊO�̓G���[)��ǉ�
            if (ModelState.IsValidField("ManufacturingYear") && CommonUtils.DefaultString(salesCar.ManufacturingYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}\.\d{1,2}$"))
                    && (!Regex.IsMatch(salesCar.ManufacturingYear.ToString(), @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ManufacturingYear", MessageUtils.GetMessage("E0004", new string[] { "�N��", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
                }
            }

            if (ModelState.IsValidField("Mileage") && salesCar.Mileage != null) {
                if ((Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Mileage.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Mileage", MessageUtils.GetMessage("E0004", new string[] { "���s����", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }
            if (ModelState.IsValidField("CarTax") && salesCar.CarTax != null) {
                if (!Regex.IsMatch(salesCar.CarTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarTax", MessageUtils.GetMessage("E0004", new string[] { "�����Ԑ�", "���̐����̂�" }));    //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("CarWeightTax") && salesCar.CarWeightTax != null) {
                if (!Regex.IsMatch(salesCar.CarWeightTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarWeightTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԏd�ʐ�", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("AcquisitionTax") && salesCar.AcquisitionTax != null) {
                if (!Regex.IsMatch(salesCar.AcquisitionTax.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("AcquisitionTax", MessageUtils.GetMessage("E0004", new string[] { "�����ԐŊ����\��", "���̐����̂�" }));   //Mod 2019/09/04 yano #4011
                }
            }
            if (ModelState.IsValidField("CarLiabilityInsurance") && salesCar.CarLiabilityInsurance != null) {
                if (!Regex.IsMatch(salesCar.CarLiabilityInsurance.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("CarLiabilityInsurance", MessageUtils.GetMessage("E0004", new string[] { "�����ӕی���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RecycleDeposit") && salesCar.RecycleDeposit != null) {
                if (!Regex.IsMatch(salesCar.RecycleDeposit.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "���̐����̂�" }));
                }
            }
            //if (!string.IsNullOrEmpty(salesCar.FirstRegistrationYear)) {
            //    if (!Regex.IsMatch(salesCar.FirstRegistrationYear, "([0-9]{4})/([0-9]{2})")) {
            //        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^"));
            //    }
            //    DateTime result;
            //    try {
            //        DateTime.TryParse(salesCar.FirstRegistrationYear + "/01", out result);
            //        if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
            //            ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^"));
            //            if (ModelState["FirstRegistrationYear"].Errors.Count() > 1) {
            //                ModelState["FirstRegistrationYear"].Errors.RemoveAt(0);
            //            }
            //        }
            //    } catch {
            //        ModelState.AddModelError("FirstRegistrationYear", MessageUtils.GetMessage("E0019", "���x�o�^"));
            //    }

            //}

            // �l�`�F�b�N
            if (ModelState.IsValidField("Capacity") && salesCar.Capacity != null) {
                if (salesCar.Capacity < 0) {
                    ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("MaximumLoadingWeight") && salesCar.MaximumLoadingWeight != null) {
                if (salesCar.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "�ő�ύڗ�", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("CarWeight") && salesCar.CarWeight != null) {
                if (salesCar.CarWeight < 0) {
                    ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("TotalCarWeight") && salesCar.TotalCarWeight != null) {
                if (salesCar.TotalCarWeight < 0) {
                    ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ����d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Length") && salesCar.Length != null) {
                if (salesCar.Length < 0) {
                    ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Width") && salesCar.Width != null) {
                if (salesCar.Width < 0) {
                    ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Height") && salesCar.Height != null) {
                if (salesCar.Height < 0) {
                    ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("FFAxileWeight") && salesCar.FFAxileWeight != null) {
                if (salesCar.FFAxileWeight < 0) {
                    ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("FRAxileWeight") && salesCar.FRAxileWeight != null) {
                if (salesCar.FRAxileWeight < 0) {
                    ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RFAxileWeight") && salesCar.RFAxileWeight != null) {
                if (salesCar.RFAxileWeight < 0) {
                    ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RRAxileWeight") && salesCar.RRAxileWeight != null) {
                if (salesCar.RRAxileWeight < 0) {
                    ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && salesCar.Displacement != null) {
                if ((Regex.IsMatch(salesCar.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(salesCar.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }
            // �a�����̕ϊ��`�F�b�N
            if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.IssueDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.IssueDateWareki)) {
                ModelState.AddModelError("IssueDateWareki.Year", MessageUtils.GetMessage("E0021", "���s��"));
            }
            if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.RegistrationDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.RegistrationDateWareki)) {
                ModelState.AddModelError("RegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "�o�^�N�����^��t�N����"));
            }
            salesCar.FirstRegistrationDateWareki.Day = 1; 
            if (!salesCar.FirstRegistrationDateWareki.IsNull) {
                if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki, db))  //Mod 2018/06/22 arc yano #3891
                {
                    //if (!JapaneseDateUtility.GlobalDateTryParse(salesCar.FirstRegistrationDateWareki)) {
                    ModelState.AddModelError("FirstRegistrationDateWareki.Year", MessageUtils.GetMessage("E0021", "���x�o�^�N��"));
                    ModelState.AddModelError("FirstRegistrationDateWareki.Month", "");
                }
                //Add 2020/11/17 yano #4065
                else
                {
                    DateTime? FirstRegistrationDate = JapaneseDateUtility.GetGlobalDate(salesCar.FirstRegistrationDateWareki, db);

                    //���x�o�^�N�����{�����30���ȍ~�̓��t�Őݒ肳��Ă����ꍇ
                    if (FirstRegistrationDate != null && (((DateTime)(FirstRegistrationDate ?? DateTime.Today).Date - DateTime.Today.Date).TotalDays > 30))
                    {
                        ModelState.AddModelError("FirstRegistrationDateWareki.Year", "���x�o�^�N���ɂ͖����̓��t�͐ݒ�ł��܂���");
                        ModelState.AddModelError("FirstRegistrationDateWareki.Month", "");
                    }
                }
            }
            if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.ExpireDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.ExpireDateWareki)) {
                ModelState.AddModelError("ExpireDateWareki.Year", MessageUtils.GetMessage("E0021", "�L������"));
            }
            if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki, db))  //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.InspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.InspectionDateWareki)) {
                ModelState.AddModelError("InspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "�_����"));
            }

            if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki, db))      //Mod 2018/06/22 arc yano #3891
            {
            //if (!salesCar.NextInspectionDateWareki.IsNull && !JapaneseDateUtility.GlobalDateTryParse(salesCar.NextInspectionDateWareki)) {
                ModelState.AddModelError("NextInspectionDateWareki.Year", MessageUtils.GetMessage("E0021", "����_����"));
            }
            return salesCar;
        }
        #endregion
    }
}
