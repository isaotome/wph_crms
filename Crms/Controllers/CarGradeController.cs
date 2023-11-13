using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �O���[�h�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarGradeController : Controller {

        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�O���[�h�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�O���[�h�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarGradeController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �O���[�h������ʕ\��
        /// </summary>
        /// <returns>�O���[�h�������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �O���[�h������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�O���[�h�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<CarGrade> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            ViewData["CarGradeName"] = form["CarGradeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �O���[�h������ʂ̕\��
            return View("CarGradeCriteria", list);
        }

        /// <summary>
        /// �O���[�h�����_�C�A���O�\��
        /// </summary>
        /// <returns>�O���[�h�����_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //�f�t�H���g�Ń��O�C���S���҂̉�ЃR�[�h���Z�b�g
            Employee employee = (Employee)Session["Employee"];
            string companyCode = "";
            try { companyCode = employee.Department1.Office.CompanyCode; } catch (NullReferenceException) { }
            form["CompanyCode"] = companyCode;

            return CriteriaDialog(form);
        }

        /// <summary>
        /// �O���[�h�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�O���[�h������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            //form["CarBrandCode"] = Request["CarBrandCode"];
            //form["CarBrandName"] = Request["CarBrandName"];
            //form["CarClassCode"] = Request["CarClassCode"];
            //form["CarClassName"] = Request["CarClassName"];
            //form["CarCode"] = Request["CarCode"];
            //form["CarName"] = Request["CarName"];
            //form["CarGradeCode"] = Request["CarGradeCode"];
            //form["CarGradeName"] = Request["CarGradeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            //form["ModelName"] = Request["ModelName"];

            // �������ʃ��X�g�̎擾
            PaginatedList<CarGrade> list;
            if (criteriaInit) {
                list = new PaginatedList<CarGrade>();
            } else {
                list = GetSearchResultListForDialog(form);
            }

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            //ViewData["CarBrandName"] = form["CarBrandName"];
            //ViewData["CarClassCode"] = form["CarClassCode"];
            //ViewData["CarClassName"] = form["CarClassName"];
            ViewData["CarCode"] = form["CarCode"];
            //ViewData["CarName"] = form["CarName"];
            ViewData["CarGradeCode"] = form["CarGradeCode"];
            //ViewData["CarGradeName"] = form["CarGradeName"];
            //ViewData["CompanyCode"] = form["CompanyCode"];
            //Company company = new CompanyDao(db).GetByKey(form["CompanyCode"]);
            //ViewData["CompanyName"] = company != null ? company.CompanyName : "";
            ViewData["ModelSpecificateNumber"] = form["ModelSpecificateNumber"];
            ViewData["ClassificationTypeNumber"] = form["ClassificationTypeNumber"];

            List<CodeData> data = new List<CodeData>();
            List<Brand> brandList = new BrandDao(db).GetListAll();
            foreach (var item in brandList) {
                data.Add(new CodeData { Code = item.CarBrandCode, Name = item.CarBrandName });
            }
            ViewData["CarBrandList"] = CodeUtils.GetSelectListByModel(data, form["CarBrandCode"], false);

            List<CodeData> carData = new List<CodeData>();
            if (form["CarBrandCode"] != null) {
                Brand brand = new BrandDao(db).GetByKey(form["CarBrandCode"]);
                if (brand != null && brand.Car != null && brand.Car.Count() > 0) {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    List<Car> carList = brand.Car.Where(x => CommonUtils.DefaultString(x.DelFlag).Equals("0")).OrderBy(x => x.DisplayOrder).ToList();
                    foreach (var car in carList) {
                        carData.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                    }
                }
            }
            ViewData["CarList"] = CodeUtils.GetSelectListByModel(carData, form["CarCode"], false);

            List<CodeData> modelData = new List<CodeData>();
            if(form["CarCode"]!=null){
                List<string> modelList = new CarGradeDao(db).GetModelNameList(form["CarCode"]);
                foreach(string item in modelList){
                    modelData.Add(new CodeData{Code=item,Name=item});
                }
            }
            ViewData["ModelNameList"] = CodeUtils.GetSelectListByModel(modelData, form["ModelName"], false);

            // �O���[�h������ʂ̕\��
            return View("CarGradeCriteriaDialog", list);
        }

        /// <summary>
        /// �O���[�h�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�O���[�h�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�O���[�h�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id) {

            CarGrade carGrade;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id)) {
                ViewData["update"] = "0";
                carGrade = new CarGrade();
            }
            
            // �X�V�̏ꍇ
            else {
                //MOD 2014/10/30 ishii �ۑ��{�^���Ή� carGrade�Ď擾�̂���
                db = CrmsDataContext.GetDataContext();
                ViewData["update"] = "1";
                carGrade = new CarGradeDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(carGrade);
            ViewData["ColorDisplay"] = false;
            ViewData["BasicDisplay"] = true;

            // �o��
            return View("CarGradeEntry", carGrade);
        }

        /// <summary>
        /// �R�s�[�@�\
        /// </summary>
        /// <param name="code">�R�s�[���O���[�h�R�[�h</param>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Copy(string code) {
            CarGrade carGrade = new CarGrade();
            if (!string.IsNullOrEmpty(code)) {
                CarGrade grade = new CarGradeDao(db).GetByKey(code);
                carGrade.ModelCode = grade.ModelCode;
                carGrade.CarCode = grade.CarCode;
                carGrade.CarClassCode = grade.CarClassCode;
                carGrade.ModelYear = grade.ModelYear;
                carGrade.Door = grade.Door;
                carGrade.TransMission = grade.TransMission;
                carGrade.Capacity = grade.Capacity;
                carGrade.SalesPrice = grade.SalesPrice;
                carGrade.SalesStartDate = grade.SalesStartDate;
                carGrade.SalesEndDate = grade.SalesEndDate;
                carGrade.MaximumLoadingWeight = grade.MaximumLoadingWeight;
                carGrade.CarWeight = grade.CarWeight;
                carGrade.TotalCarWeight = grade.TotalCarWeight;
                carGrade.DrivingName = grade.DrivingName;
                carGrade.ClassificationTypeNumber = grade.ClassificationTypeNumber;
                carGrade.ModelSpecificateNumber = grade.ModelSpecificateNumber;
                carGrade.Length = grade.Length;
                carGrade.Width = grade.Width;
                carGrade.Height = grade.Height;
                carGrade.FFAxileWeight = grade.FFAxileWeight;
                carGrade.FRAxileWeight = grade.FRAxileWeight;
                carGrade.RFAxileWeight = grade.RFAxileWeight;
                carGrade.RRAxileWeight = grade.RRAxileWeight;
                carGrade.ModelName = grade.ModelName;
                carGrade.EngineType = grade.EngineType;
                carGrade.Displacement = grade.Displacement;
                carGrade.Fuel = grade.Fuel;
                carGrade.VehicleType = grade.VehicleType;
                carGrade.InspectionRegistCost = grade.InspectionRegistCost;
                carGrade.RecycleDeposit = grade.RecycleDeposit;
                carGrade.Under24 = grade.Under24;
                carGrade.Under26 = grade.Under26;
                carGrade.Under28 = grade.Under28;
                carGrade.Under30 = grade.Under30;
                carGrade.Under36 = grade.Under36;
                carGrade.Under72 = grade.Under72;
                carGrade.Under84 = grade.Under84;
                carGrade.Over84 = grade.Over84;
                carGrade.CarClassification = grade.CarClassification;
                carGrade.Usage = grade.Usage;
                carGrade.UsageType = grade.UsageType;
                carGrade.Figure = grade.Figure;

                EntitySet<CarAvailableColor> carAvailableColorList = new EntitySet<CarAvailableColor>();
                foreach (var item in grade.CarAvailableColor) {
                    carAvailableColorList.Add(item);
                }
                carGrade.CarAvailableColor = carAvailableColorList;

            }
            GetEntryViewData(carGrade);
            ViewData["update"] = "0";
            ViewData["ColorDisplay"] = false;
            ViewData["BasicDisplay"] = true;
            return View("CarGradeEntry", carGrade);
        }
        /// <summary>
        /// �O���[�h�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="carGrade">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�O���[�h�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarGrade carGrade, EntitySet<CarAvailableColor> availableColor, FormCollection form) {

            //carGrade.CarAvailableColor = availableColor;

            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCarGrade(carGrade);
            if (!ModelState.IsValid) {
                carGrade.CarAvailableColor = availableColor;
                GetEntryViewData(carGrade);
                ViewData["ColorDisplay"] = false;
                ViewData["BasicDisplay"] = true;
                return View("CarGradeEntry", carGrade);
            }

            // Add 2014/08/01 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1")) {
                // �f�[�^�ҏW�E�X�V
                CarGrade targetCarGrade = new CarGradeDao(db).GetByKey(carGrade.CarGradeCode);
                if (targetCarGrade != null) {
                    foreach (var original in targetCarGrade.CarAvailableColor) {
                        IEnumerable<CarAvailableColor> query = null;
                        if (availableColor != null) {
                            query =
                                from a in availableColor
                                where a.CarColorCode.Equals(original.CarColorCode)
                                select a;
                        }
                        if (availableColor == null || query == null || query.Count() == 0) {
                            db.CarAvailableColor.DeleteOnSubmit(original);
                        }
                    }
                }
                if (availableColor != null) {
                    // ADD arc uchida vs2012�Ή�
                    foreach (var item1 in availableColor) {
                        int flag = 0;
                        foreach (var item2 in availableColor) {
                            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                            if (CommonUtils.DefaultString(item1.CarColorCode).Equals(CommonUtils.DefaultString(item2.CarColorCode)))
                            {
                                flag += 1; }
                        }
                        if (flag >= 2) {
                            ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0010", new string[] { item1.CarColorCode, "�ύX" }));
                            break;
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        foreach (var item in availableColor)
                        {
                            if (!string.IsNullOrEmpty(item.CarColorCode))
                            {
                                //�Ȃ����̂͒ǉ�����
                                CarAvailableColor target = new CarAvailableColorDao(db).GetByKey(carGrade.CarGradeCode, item.CarColorCode);
                                if (target == null)
                                {
                                    item.CarGradeCode = carGrade.CarGradeCode;
                                    item.CreateDate = DateTime.Now;
                                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                    item.LastUpdateDate = DateTime.Now;
                                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                    item.DelFlag = "0";
                                    db.CarAvailableColor.InsertOnSubmit(item);
                                }
                            }
                        }
                    }
                    else {
                        carGrade.CarAvailableColor = availableColor;
                        GetEntryViewData(carGrade);
                        ViewData["ColorDisplay"] = false;
                        ViewData["BasicDisplay"] = true;
                        return View("CarGradeEntry", carGrade);
                    }
                }

                UpdateModel(targetCarGrade);
                EditCarGradeForUpdate(targetCarGrade);
            }

            // �f�[�^�ǉ�����
            else {
                // �f�[�^�ҏW
                carGrade = EditCarGradeForInsert(carGrade);

                if (availableColor != null) {
                    foreach (var item in availableColor) {
                        if (!string.IsNullOrEmpty(item.CarColorCode)) {
                            //�Ȃ����̂͒ǉ�����
                            CarAvailableColor target = new CarAvailableColorDao(db).GetByKey(carGrade.CarGradeCode, item.CarColorCode);
                            if (target == null) {
                                item.CarGradeCode = carGrade.CarGradeCode;
                                item.CreateDate = DateTime.Now;
                                item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                item.LastUpdateDate = DateTime.Now;
                                item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                                item.DelFlag = "0";
                                db.CarAvailableColor.InsertOnSubmit(item);
                            }
                        }
                    }
                }
                // �f�[�^�ǉ�
                db.CarGrade.InsertOnSubmit(carGrade);
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
                        //Add 2014/08/04 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0010", new string[] { "�O���[�h�R�[�h", "�ۑ�" }));
                        GetEntryViewData(carGrade);
                        return View("CarGradeEntry", carGrade);
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
            //MOD 2014/10/29 ishii �ۑ��{�^���Ή�
            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //ViewData["ColorDisplay"] = false;
            //ViewData["BasicDisplay"] = true;
            //return Entry((string)null);
            return Entry(carGrade.CarGradeCode);

        }

        /// <summary>
        /// �s�ǉ��E�s�폜
        /// </summary>
        /// <param name="carGrade"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public ActionResult EditLine(CarGrade carGrade, EntitySet<CarAvailableColor> availableColor, FormCollection form) {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            if (form["DelLine"].Equals("-1")) {
                if (availableColor == null) {
                    availableColor = new EntitySet<CarAvailableColor>();
                }
                availableColor.Add(new CarAvailableColor { CarGradeCode = carGrade.CarGradeCode, DelFlag = "0" });
            } else if (availableColor != null && availableColor.Count() > 0 && form["DelLine"] != null) {
                availableColor.RemoveAt(int.Parse(form["DelLine"]));
            }
            for (int i = 0; i < availableColor.Count(); i++) {
                availableColor[i].CarColor = new CarColorDao(db).GetByKey(availableColor[i].CarColorCode);
            }
            carGrade.CarAvailableColor = availableColor;
            GetEntryViewData(carGrade);
            ViewData["ColorDisplay"] = true;
            ViewData["BasicDisplay"] = false;
            ModelState.Clear();
            return View("CarGradeEntry", carGrade);
        }

        private void UpdateCarAvailableColor(CarGrade carGrade) {

        }
        /// <summary>
        /// �O���[�h�R�[�h����O���[�h�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�O���[�h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                CarGrade carGrade = new CarGradeDao(db).GetByKey(code);
                if (carGrade != null && carGrade.DelFlag.Equals("0")) {
                    codeData.Code = carGrade.CarGradeCode;
                    codeData.Name = carGrade.CarGradeName;
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
        /// <param name="taxid">�ŗ�id(�󕶎��c�Čv�Z���Ȃ��A</param>
        /// <param name="requestregistdate">�o�^��]��</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2019/10/17 yano #4022 �y�ԗ��`�[���́z����̏������ł̊����\���̌v�Z
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C��Ɓ@�߂�l�̌^�̕ύX
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", DateTime? requestregistdate = null) //2019/10/17 yano #4022
        //public ActionResult GetAcquisitionTax(string code, decimal optionAmount, string taxid = "", decimal? salesPrice = null)
        {
       
            decimal amount = 0m;

            CarGrade carGrade = new CarGrade();
            
            if (Request.IsAjaxRequest()) {

                //Mod 2019/10/17 yano #4022
                if (!string.IsNullOrEmpty(code))
                {
                    carGrade = new CarGradeDao(db).GetByKey(code);

                    amount = (carGrade.SalesPrice ?? 0m);
                }

                ////Mod 2019/09/04 yano #4011
                //if (salesPrice != null)
                //{
                //    amount = (salesPrice ?? 0m);
                //}
                //else
                //{
                //    if (!string.IsNullOrEmpty(code))
                //    {
                //        carGrade = new CarGradeDao(db).GetByKey(code);

                //        amount = (carGrade.SalesPrice ?? 0m);
                //    }
                //}

                Tuple<string, decimal> acquisitionTax = CommonUtils.GetAcquisitionTax(amount, optionAmount, carGrade.VehicleType, "N", "", taxid );    //Mod 2019/09/04 yano #4011
                //decimal acquisitionTax = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0, optionAmount, carGrade.VehicleType, "N", "");

                return Json(acquisitionTax);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// �O���[�h�R�[�h����ԗ������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�O���[�h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C��� �߂�l�ύX�ɂ��C��
        /// </history>
        public ActionResult GetMasterDetail(string code) {

            if (Request.IsAjaxRequest()) {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                CarGrade carGrade = new CarGradeDao(db).GetByKey(code);
                if (carGrade != null && carGrade.DelFlag.Equals("0")) {
                    ret.Add("CarGradeCode", carGrade.CarGradeCode);
                    ret.Add("CarGradeName", carGrade.CarGradeName);
                    ret.Add("ModelCode", carGrade.ModelCode);
                    ret.Add("Capacity", CommonUtils.DefaultString(carGrade.Capacity));
                    ret.Add("MaximumLoadingWeight", CommonUtils.DefaultString(carGrade.MaximumLoadingWeight));
                    ret.Add("CarWeight", CommonUtils.DefaultString(carGrade.CarWeight));
                    ret.Add("TotalCarWeight", CommonUtils.DefaultString(carGrade.TotalCarWeight));
                    ret.Add("Length", CommonUtils.DefaultString(carGrade.Length));
                    ret.Add("Width", CommonUtils.DefaultString(carGrade.Width));
                    ret.Add("Height", CommonUtils.DefaultString(carGrade.Height));
                    ret.Add("FFAxileWeight", CommonUtils.DefaultString(carGrade.FFAxileWeight));
                    ret.Add("FRAxileWeight", CommonUtils.DefaultString(carGrade.FRAxileWeight));
                    ret.Add("RFAxileWeight", CommonUtils.DefaultString(carGrade.RFAxileWeight));
                    ret.Add("RRAxileWeight", CommonUtils.DefaultString(carGrade.RRAxileWeight));
                    ret.Add("ModelName", carGrade.ModelName);
                    ret.Add("EngineType", carGrade.EngineType);
                    ret.Add("Displacement", CommonUtils.DefaultString(carGrade.Displacement));
                    ret.Add("Fuel", carGrade.Fuel);
                    ret.Add("ModelSpecificateNumber", carGrade.ModelSpecificateNumber);
                    ret.Add("ClassificationTypeNumber", carGrade.ClassificationTypeNumber);
                    ret.Add("Door", carGrade.Door);
                    ret.Add("TransMission", carGrade.TransMission);
                    try { ret.Add("SalesPrice", carGrade.SalesPrice.ToString()); } catch (NullReferenceException) { }
                    //MOD 2014/02/20 ookubo ������rate�͈Ӗ��Ȃ��i�Ăь��ōČv�Z�j
                    string id = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    int rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(id));
                    try { ret.Add("SalesTax", Math.Truncate((carGrade.SalesPrice ?? 0m) * (rate / 100)).ToString()); }
                    //try { ret.Add("SalesTax", Math.Truncate((carGrade.SalesPrice ?? 0m) * 0.05m).ToString()); }
                    catch (NullReferenceException) { }
                    //MOD 2014/02/20 ookubo ������rate�͈Ӗ��Ȃ��i�Ăь��ōČv�Z�j
                    try { ret.Add("SalesPriceWithTax", (carGrade.SalesPrice + Math.Truncate((carGrade.SalesPrice ?? 0m) * rate)).ToString()); }
                    //try { ret.Add("SalesPriceWithTax", (carGrade.SalesPrice + Math.Truncate((carGrade.SalesPrice ?? 0m) * 0.05m)).ToString()); }
                    catch (NullReferenceException) { }
                    try { ret.Add("CarBrandCode", carGrade.Car.CarBrandCode); } catch (NullReferenceException) { }
                    try { ret.Add("CarName", carGrade.Car.CarName); } catch (NullReferenceException) { }
                    try { ret.Add("CarBrandName", carGrade.Car.Brand.CarBrandName); } catch (NullReferenceException) { }
                    try { ret.Add("MakerName", carGrade.Car.Brand.Maker.MakerName); } catch (NullReferenceException) { }
                    ret.Add("CarGradeFullName", ret["CarBrandName"] + " " + ret["CarName"] + " " + ret["CarGradeName"]);
                    try { ret.Add("LaborRate", carGrade.Car.Brand.LaborRate.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("InspectionRegistCost", carGrade.InspectionRegistCost.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("TradeInMaintenanceFee", ""); } catch (NullReferenceException) { }
                    try { ret.Add("InheritedInsuranceFee", carGrade.Under24.ToString()); } catch (NullReferenceException) { }
                    try { ret.Add("RecycleDeposit", carGrade.RecycleDeposit.ToString()); } catch (NullReferenceException) { }
                    ret.Add("ModelYear", carGrade.ModelYear);

                    //�����\��
                    //Mod 2019/09/04 yano #4011
                    Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, "N", "");
                    ret.Add("EPDiscountTaxList", retValue.Item1);
                    ret.Add("AcquisitionTax", string.Format("{0:0}", retValue.Item2.ToString()));
                    //ret.Add("AcquisitionTax", string.Format("{0:0}", CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, "N", "")));
                
                    //�����ӕی���(�O���[�h���Z�b�g�����ꍇ�͎����I�ɐV�ԂƂ݂Ȃ�)
                    CarLiabilityInsurance insurance = new CarLiabilityInsuranceDao(db).GetByNewDefault();
                    if (insurance != null) {
                        ret.Add("CarLiabilityInsurance", string.Format("{0:0}", insurance.Amount));
                    }

                    //�d�ʐ�(�O���[�h���Z�b�g�����ꍇ�͎����I�ɐV�ԂƂ݂Ȃ�)
                    CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, carGrade.CarWeight ?? 0);
                    if (weightTax != null) {
                        ret.Add("CarWeightTax", string.Format("{0:0}", weightTax.Amount));
                    }
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �Ԏ�R�[�h����^�����X�g���擾����
        /// </summary>
        /// <param name="carCode"></param>
        public void GetModelNameList(string carCode) {
            if (Request.IsAjaxRequest()) {
                List<string> modelList = new CarGradeDao(db).GetModelNameList(carCode);
                CodeDataList codeDataList = new CodeDataList();
                if (modelList != null) {
                    codeDataList.Code = carCode;
                    codeDataList.DataList = new List<CodeData>();
                    foreach (var item in modelList) {
                        codeDataList.DataList.Add(new CodeData() { Code = item, Name = item });
                    }
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }
        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="carGrade">���f���f�[�^</param>
        private void GetEntryViewData(CarGrade carGrade) {

            // �Ԏ햼�̎擾
            if (!string.IsNullOrEmpty(carGrade.CarCode)) {
                CarDao carDao = new CarDao(db);
                Car car = carDao.GetByKey(carGrade.CarCode);
                if (car != null) {
                    ViewData["CarName"] = car.CarName;
                }
            }

            // �ԗ��N���X���̎擾
            if (!string.IsNullOrEmpty(carGrade.CarClassCode)) {
                CarClassDao carClassDao = new CarClassDao(db);
                CarClass carClass = carClassDao.GetByKey(carGrade.CarClassCode);
                if (carClass != null) {
                    ViewData["CarClassName"] = carClass.CarClassName;
                }
            }

            //�Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["TransMissionList"] = CodeUtils.GetSelectListByModel(dao.GetTransMissionAll(false), carGrade.TransMission, true);
            ViewData["DrivingNameList"] = CodeUtils.GetSelectListByModel(dao.GetDrivingNameAll(false), carGrade.DrivingName, true);
            ViewData["VehicleTypeList"] = CodeUtils.GetSelectListByModel(dao.GetVehicleTypeAll(false), carGrade.VehicleType, true);
            ViewData["FuelList"] = CodeUtils.GetSelectListByModel(dao.GetFuelTypeAll(false), carGrade.Fuel, true);
            ViewData["UsageTypeList"] = CodeUtils.GetSelectListByModel(dao.GetUsageTypeAll(false), carGrade.UsageType, true);
            ViewData["UsageList"] = CodeUtils.GetSelectListByModel(dao.GetUsageAll(false), carGrade.Usage, true);
            ViewData["CarClassificationList"] = CodeUtils.GetSelectListByModel(dao.GetCarClassificationAll(false), carGrade.CarClassification, true);
            ViewData["FigureList"] = CodeUtils.GetSelectListByModel(dao.GetFigureAll(false), carGrade.Figure, true);

            // �ԗ��J���[���̎擾
            foreach (var item in carGrade.CarAvailableColor) {
                item.CarColor = new CarColorDao(db).GetByKey(item.CarColorCode);
            }
        }

        /// <summary>
        /// �O���[�h�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�O���[�h�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<CarGrade> GetSearchResultList(FormCollection form) {

            CarGradeDao carGradeDao = new CarGradeDao(db);
            CarGrade carGradeCondition = new CarGrade();
            carGradeCondition.CarGradeCode = form["CarGradeCode"];
            carGradeCondition.CarGradeName = form["CarGradeName"];
            carGradeCondition.Car = new Car();
            carGradeCondition.Car.CarCode = form["CarCode"];
            carGradeCondition.Car.CarName = form["CarName"];
            carGradeCondition.Car.Brand = new Brand();
            carGradeCondition.Car.Brand.CarBrandCode = form["CarBrandCode"];
            carGradeCondition.Car.Brand.CarBrandName = form["CarBrandName"];
            carGradeCondition.Car.Brand.CompanyCode = form["CompanyCode"];
            carGradeCondition.CarClass = new CarClass();
            carGradeCondition.CarClass.CarClassCode = form["CarClassCode"];
            carGradeCondition.CarClass.CarClassName = form["CarClassName"];
            carGradeCondition.ModelName = form["ModelName"];
            carGradeCondition.ModelSpecificateNumber = form["ModelSpecificateNumber"];
            carGradeCondition.ClassificationTypeNumber = form["ClassificationTypeNumber"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carGradeCondition.DelFlag = form["DelFlag"];
            }
            return carGradeDao.GetListByCondition(carGradeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        private PaginatedList<CarGrade> GetSearchResultListForDialog(FormCollection form) {
            CarGradeDao carGradeDao = new CarGradeDao(db);
            CarGrade carGradeCondition = new CarGrade();
            carGradeCondition.Car = new Car();
            carGradeCondition.Car.CarCode = form["CarCode"];
            carGradeCondition.Car.Brand = new Brand();
            carGradeCondition.Car.Brand.CarBrandCode = form["CarBrandCode"];
            carGradeCondition.ModelName = form["ModelName"];
            carGradeCondition.ModelSpecificateNumber = form["ModelSpecificateNumber"];
            carGradeCondition.ClassificationTypeNumber = form["ClassificationTypeNumber"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                carGradeCondition.DelFlag = form["DelFlag"];
            }
            return carGradeDao.GetListByConditionForDialog(carGradeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

        }
        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carGrade">�O���[�h�f�[�^</param>
        /// <returns>�O���[�h�f�[�^</returns>
        /// <history>
        /// 2021/08/02 yano #4097�y�O���[�h�}�X�^���́z�N���̕ۑ��̊g���@�\�i�N�I�[�^�[�Ή��j
        /// 2018/04/25 arc yano #3716 �y�ԁz�O���[�h�}�X�^�̃O���[�h�R�[�h�Ɂu�|�F�n�C�t���v�u�Q�F�A���_�[�o�[�v�t���o�^
        /// </history>
        private CarGrade ValidateCarGrade(CarGrade carGrade) {

            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(carGrade.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0001", "�O���[�h�R�[�h"));
            }
            if (string.IsNullOrEmpty(carGrade.CarGradeName)) {
                ModelState.AddModelError("CarGradeName", MessageUtils.GetMessage("E0001", "�O���[�h��"));
            }
            if (string.IsNullOrEmpty(carGrade.CarCode)) {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0001", "�Ԏ�"));
            }
            if (string.IsNullOrEmpty(carGrade.CarClassCode)) {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0001", "�ԗ��N���X"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Capacity")) {
                ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SalesPrice")) {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��{�̉��i", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SalesStartDate")) {
                ModelState.AddModelError("SalesStartDate", MessageUtils.GetMessage("E0005", "�̔��J�n��"));
            }
            if (!ModelState.IsValidField("SalesEndDate")) {
                ModelState.AddModelError("SalesEndDate", MessageUtils.GetMessage("E0005", "�̔��I����"));
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
            if (!ModelState.IsValidField("InspectionRegistCost")) {
                ModelState.AddModelError("InspectionRegistCost", MessageUtils.GetMessage("E0004", new string[] { "�����o�^�葱", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("RecycleDeposit")) {
                ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under24")) {
                ModelState.AddModelError("Under24", MessageUtils.GetMessage("E0004", new string[] { "24��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under26")) {
                ModelState.AddModelError("Under26", MessageUtils.GetMessage("E0004", new string[] { "26��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under28")) {
                ModelState.AddModelError("Under28", MessageUtils.GetMessage("E0004", new string[] { "28��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under30")) {
                ModelState.AddModelError("Under30", MessageUtils.GetMessage("E0004", new string[] { "30��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under36")) {
                ModelState.AddModelError("Under36", MessageUtils.GetMessage("E0004", new string[] { "36��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under72")) {
                ModelState.AddModelError("Under72", MessageUtils.GetMessage("E0004", new string[] { "72��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Under84")) {
                ModelState.AddModelError("Under84", MessageUtils.GetMessage("E0004", new string[] { "84��������", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Over84")) {
                ModelState.AddModelError("Over84", MessageUtils.GetMessage("E0004", new string[] { "84�����ȏ�", "���̐����̂�" }));
            }

            //Mod 2018/04/25 arc yano #3716
            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarGradeCode") && !CommonUtils.IsAlphaNumericBarUnderBar(carGrade.CarGradeCode))
            {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0031", "�O���[�h�R�[�h"));
            }
            /*
            if (ModelState.IsValidField("CarGradeCode") && !CommonUtils.IsAlphaNumeric(carGrade.CarGradeCode)) {
                ModelState.AddModelError("CarGradeCode", MessageUtils.GetMessage("E0012", "�O���[�h�R�[�h"));
            }
            */
            if (ModelState.IsValidField("SalesPrice") && carGrade.SalesPrice != null) {
                if (!Regex.IsMatch(carGrade.SalesPrice.ToString(), @"^\d{1,10}$")) {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��{�̉��i", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("InspectionRegistCost") && carGrade.InspectionRegistCost != null) {
                if (!Regex.IsMatch(carGrade.InspectionRegistCost.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("InspectionRegistCost", MessageUtils.GetMessage("E0004", new string[] { "�����o�^��p", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RecycleDeposit") && carGrade.RecycleDeposit != null) {
                if (!Regex.IsMatch(carGrade.RecycleDeposit.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("RecycleDeposit", MessageUtils.GetMessage("E0004", new string[] { "���T�C�N���a����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under24") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under24.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under24", MessageUtils.GetMessage("E0004", new string[] { "24��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under26") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under26.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under26", MessageUtils.GetMessage("E0004", new string[] { "26��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under28") && carGrade.Under24 != null) {
                if (!Regex.IsMatch(carGrade.Under28.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under28", MessageUtils.GetMessage("E0004", new string[] { "28��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under30") && carGrade.Under30 != null) {
                if (!Regex.IsMatch(carGrade.Under30.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under30", MessageUtils.GetMessage("E0004", new string[] { "30��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under36") && carGrade.Under36 != null) {
                if (!Regex.IsMatch(carGrade.Under36.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under36", MessageUtils.GetMessage("E0004", new string[] { "36��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under72") && carGrade.Under72 != null) {
                if (!Regex.IsMatch(carGrade.Under72.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under72", MessageUtils.GetMessage("E0004", new string[] { "72��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Under84") && carGrade.Under84 != null) {
                if (!Regex.IsMatch(carGrade.Under84.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Under84", MessageUtils.GetMessage("E0004", new string[] { "84��������", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Over84") && carGrade.Over84 != null) {
                if (!Regex.IsMatch(carGrade.Over84.ToString(), @"^\d|1,10}$")) {
                    ModelState.AddModelError("Over84", MessageUtils.GetMessage("E0004", new string[] { "84��������", "���̐����̂�" }));
                }
            }

            //Add 2021/08/02 yano #4097 ���͉\�����t�H�[�}�b�g��ύX(���̐����S���̂݁����̐���4���A�܂��͐��̐���4��������2���ȓ�
            if (ModelState.IsValidField("ModelYear") && CommonUtils.DefaultString(carGrade.ModelYear).Equals("") == false)
            {
                if (((!Regex.IsMatch(carGrade.ModelYear, @"^\d{4}\.\d{1,2}$"))
                                  && (!Regex.IsMatch(carGrade.ModelYear, @"^\d{4}$")))
                )
                {
                    ModelState.AddModelError("ModelYear", MessageUtils.GetMessage("E0004", new string[] { "���f���N", "���̐���4���܂��́A���̐���4��������2���ȓ�" }));
                }
            }

            // �l�`�F�b�N
            if (ModelState.IsValidField("Capacity") && carGrade.Capacity != null) {
                if (carGrade.Capacity < 0) {
                    ModelState.AddModelError("Capacity", MessageUtils.GetMessage("E0004", new string[] { "���", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("MaximumLoadingWeight") && carGrade.MaximumLoadingWeight != null) {
                if (carGrade.MaximumLoadingWeight < 0) {
                    ModelState.AddModelError("MaximumLoadingWeight", MessageUtils.GetMessage("E0004", new string[] { "�ő�ύڗ�", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("CarWeight") && carGrade.CarWeight != null) {
                if (carGrade.CarWeight < 0) {
                    ModelState.AddModelError("CarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ��d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("TotalCarWeight") && carGrade.TotalCarWeight != null) {
                if (carGrade.TotalCarWeight < 0) {
                    ModelState.AddModelError("TotalCarWeight", MessageUtils.GetMessage("E0004", new string[] { "�ԗ����d��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Length") && carGrade.Length != null) {
                if (carGrade.Length < 0) {
                    ModelState.AddModelError("Length", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Width") && carGrade.Width != null) {
                if (carGrade.Width < 0) {
                    ModelState.AddModelError("Width", MessageUtils.GetMessage("E0004", new string[] { "��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Height") && carGrade.Height != null) {
                if (carGrade.Height < 0) {
                    ModelState.AddModelError("Height", MessageUtils.GetMessage("E0004", new string[] { "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("FFAxileWeight") && carGrade.FFAxileWeight != null) {
                if (carGrade.FFAxileWeight < 0) {
                    ModelState.AddModelError("FFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("FRAxileWeight") && carGrade.FRAxileWeight != null) {
                if (carGrade.FRAxileWeight < 0) {
                    ModelState.AddModelError("FRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "�O�㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RFAxileWeight") && carGrade.RFAxileWeight != null) {
                if (carGrade.RFAxileWeight < 0) {
                    ModelState.AddModelError("RFAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��O���d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("RRAxileWeight") && carGrade.RRAxileWeight != null) {
                if (carGrade.RRAxileWeight < 0) {
                    ModelState.AddModelError("RRAxileWeight", MessageUtils.GetMessage("E0004", new string[] { "��㎲�d", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Displacement") && carGrade.Displacement != null) {
                if ((Regex.IsMatch(carGrade.Displacement.ToString(), @"^\d{1,10}\.\d{1,2}$"))
                    || (Regex.IsMatch(carGrade.Displacement.ToString(), @"^\d{1,10}$"))) {
                } else {
                    ModelState.AddModelError("Displacement", MessageUtils.GetMessage("E0004", new string[] { "�r�C��", "���̐���10���ȓ�������2���ȓ�" }));
                }
            }

            return carGrade;
        }

        /// <summary>
        /// �O���[�h�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carGrade">�O���[�h�f�[�^(�o�^���e)</param>
        /// <returns>�O���[�h�}�X�^���f���N���X</returns>
        private CarGrade EditCarGradeForInsert(CarGrade carGrade) {

            carGrade.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.CreateDate = DateTime.Now;
            carGrade.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.LastUpdateDate = DateTime.Now;
            carGrade.DelFlag = "0";
            return carGrade;
        }

        /// <summary>
        /// �O���[�h�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carGrade">�O���[�h�f�[�^(�o�^���e)</param>
        /// <returns>�O���[�h�}�X�^���f���N���X</returns>
        private CarGrade EditCarGradeForUpdate(CarGrade carGrade) {

            carGrade.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carGrade.LastUpdateDate = DateTime.Now;
            return carGrade;
        }

    }
}
