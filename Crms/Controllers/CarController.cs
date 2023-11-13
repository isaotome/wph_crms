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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �Ԏ�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�Ԏ�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�Ԏ�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �Ԏ팟����ʕ\��
        /// </summary>
        /// <returns>�Ԏ팟�����</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �Ԏ팟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ԏ팟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Car> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �Ԏ팟����ʂ̕\��
            return View("CarCriteria", list);
        }

        /// <summary>
        /// �Ԏ팟���_�C�A���O�\��
        /// </summary>
        /// <returns>�Ԏ팟���_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �Ԏ팟���_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ԏ팟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CarCode"] = Request["CarCode"];
            form["CarName"] = Request["CarName"];
            form["CarBrandCode"] = Request["CarBrandCode"];
            form["CarBrandName"] = Request["CarBrandName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Car> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarCode"] = form["CarCode"];
            ViewData["CarName"] = form["CarName"];
            ViewData["CarBrandCode"] = form["CarBrandCode"];
            ViewData["CarBrandName"] = form["CarBrandName"];

            // �Ԏ팟����ʂ̕\��
            return View("CarCriteriaDialog", list);
        }

        /// <summary>
        /// �Ԏ�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�Ԏ�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�Ԏ�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Car car;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                car = new Car();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                car = new CarDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(car);

            // �o��
            return View("CarEntry", car);
        }

        /// <summary>
        /// �Ԏ�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="car">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�Ԏ�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Car car, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCar(car);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(car);
                return View("CarEntry", car);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Car targetCar = new CarDao(db).GetByKey(car.CarCode, true);
                UpdateModel(targetCar);
                EditCarForUpdate(targetCar);
                
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                car = EditCarForInsert(car);
                // �f�[�^�ǉ�
                db.Car.InsertOnSubmit(car);
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

                        ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0010", new string[] { "�Ԏ�R�[�h", "�ۑ�" }));
                        GetEntryViewData(car);
                        return View("CarEntry", car);
                    }
                    else
                    {
                        //Mod 2014/08/04 arc amii �G���[���O�Ή� �wtheow e�x����G���[�y�[�W�J�ڂɕύX
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    //Add 2014/08/04 arc amii �G���[���O�Ή� ChangeConflictException�ȊO�̎��̃G���[�����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(car.CarCode);

        }

        /// <summary>
        /// �Ԏ�R�[�h����Ԏ햼���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ԏ�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Car car = new CarDao(db).GetByKey(code);
                if (car != null)
                {
                    codeData.Code = car.CarCode;
                    codeData.Name = car.CarName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        /// <summary>
        /// �u�����h�R�[�h����Ԏ탊�X�g���擾����iajax�j
        /// </summary>
        /// <param name="code">�u�����h�R�[�h</param>
        public void GetMasterList(string code) {
            if (Request.IsAjaxRequest()) {
                Brand brand = new BrandDao(db).GetByKey(code);
                CodeDataList codeDataList = new CodeDataList();
                if (brand != null) {
                    codeDataList.Code = brand.CarBrandCode;
                    codeDataList.Name = brand.CarBrandName;
                    if (brand.Car != null) {
                        codeDataList.DataList = new List<CodeData>();
                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        foreach (var a in brand.Car.Where(x => CommonUtils.DefaultString(x.DelFlag).Equals("0")).OrderBy(x => x.DisplayOrder))
                        {
                            codeDataList.DataList.Add(new CodeData() { Code = a.CarCode , Name = a.CarName });
                        }
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
        /// <param name="car">���f���f�[�^</param>
        private void GetEntryViewData(Car car)
        {
            // �u�����h���̎擾
            if (!string.IsNullOrEmpty(car.CarBrandCode))
            {
                BrandDao brandDao = new BrandDao(db);
                Brand brand = brandDao.GetByKey(car.CarBrandCode);
                if (brand != null)
                {
                    ViewData["CarBrandName"] = brand.CarBrandName;
                }
            }
        }

        /// <summary>
        /// �Ԏ�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ԏ�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Car> GetSearchResultList(FormCollection form)
        {
            CarDao carDao = new CarDao(db);
            Car carCondition = new Car();
            carCondition.CarCode = form["CarCode"];
            carCondition.CarName = form["CarName"];
            carCondition.Brand = new Brand();
            carCondition.Brand.CarBrandCode = form["CarBrandCode"];
            carCondition.Brand.CarBrandName = form["CarBrandName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carCondition.DelFlag = form["DelFlag"];
            }
            return carDao.GetListByCondition(carCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="car">�Ԏ�f�[�^</param>
        /// <returns>�Ԏ�f�[�^</returns>
        private Car ValidateCar(Car car)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(car.CarCode))
            {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0001", "�Ԏ�R�[�h"));
            }
            if (string.IsNullOrEmpty(car.CarName))
            {
                ModelState.AddModelError("CarName", MessageUtils.GetMessage("E0001", "�Ԏ햼"));
            }
            if (string.IsNullOrEmpty(car.CarBrandCode))
            {
                ModelState.AddModelError("CarBrandCode", MessageUtils.GetMessage("E0001", "�u�����h"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarCode") && !CommonUtils.IsAlphaNumeric(car.CarCode))
            {
                ModelState.AddModelError("CarCode", MessageUtils.GetMessage("E0012", "�Ԏ�R�[�h"));
            }

            return car;
        }

        /// <summary>
        /// �Ԏ�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="car">�Ԏ�f�[�^(�o�^���e)</param>
        /// <returns>�Ԏ�}�X�^���f���N���X</returns>
        private Car EditCarForInsert(Car car)
        {
            car.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.CreateDate = DateTime.Now;
            car.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.LastUpdateDate = DateTime.Now;
            car.DelFlag = "0";
            return car;
        }

        /// <summary>
        /// �Ԏ�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="car">�Ԏ�f�[�^(�o�^���e)</param>
        /// <returns>�Ԏ�}�X�^���f���N���X</returns>
        private Car EditCarForUpdate(Car car)
        {
            car.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            car.LastUpdateDate = DateTime.Now;
            return car;
        }

    }
}
