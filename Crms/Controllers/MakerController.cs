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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace Crms.Controllers
{
    /// <summary>
    /// ���[�J�[�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MakerController : Controller
    {

        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���[�J�[�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���[�J�[�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;
        protected bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public MakerController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���[�J�[������ʕ\��
        /// </summary>
        /// <returns>���[�J�[�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���[�J�[������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[�J�[�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Maker> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���[�J�[������ʂ̕\��
            return View("MakerCriteria", list);
        }

        /// <summary>
        /// ���[�J�[�����_�C�A���O�\��
        /// </summary>
        /// <returns>���[�J�[�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���[�J�[�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[�J�[������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Maker> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];

            // ���[�J�[������ʂ̕\��
            return View("MakerCriteriaDialog", list);
        }

        /// <summary>
        /// ���[�J�[�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���[�J�[�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���[�J�[�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Maker maker;

            // �\���f�[�^�ݒ�(�ǉ��̏ꍇ)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                maker = new Maker();
            }
            // �\���f�[�^�ݒ�(�X�V�̏ꍇ)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                maker = new MakerDao(db).GetByKey(id, true);
            }

            // �o��
            return View("MakerEntry", maker);
        }

        /// <summary>
        /// ���[�J�[�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="maker">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���[�J�[�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Maker maker, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateMaker(maker);
            if (!ModelState.IsValid)
            {
                return View("MakerEntry", maker);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Maker targetMaker = new MakerDao(db).GetByKey(maker.MakerCode, true);
                UpdateModel(targetMaker);
                EditMakerForUpdate(targetMaker);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                maker = EditMakerForInsert(maker);

                // �f�[�^�ǉ�
                db.Maker.InsertOnSubmit(maker);
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
                        //Mod 2014/08/04 arc amii �G���[���O�Ή� �wtheow e�x����G���[�y�[�W�J�ڂɕύX
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
                    //Add 2014/08/04 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0010", new string[] { "���[�J�[�R�[�h", "�ۑ�" }));
                        return View("MakerEntry", maker);
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
            return Entry(maker.MakerCode);
        }

        /// <summary>
        /// ���[�J�[�R�[�h���烁�[�J�[�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���[�J�[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Maker maker = new MakerDao(db).GetByKey(code);
                if (maker != null)
                {
                    codeData.Code = maker.MakerCode;
                    codeData.Name = maker.MakerName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }



        /// <summary>
        /// ���[�J�[�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[�J�[�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Maker> GetSearchResultList(FormCollection form)
        {
            MakerDao makerDao = new MakerDao(db);
            Maker makerCondition = new Maker();
            makerCondition.MakerCode = form["MakerCode"];
            makerCondition.MakerName = form["MakerName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                makerCondition.DelFlag = form["DelFlag"];
            }
            return makerDao.GetListByCondition(makerCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="maker">���[�J�[�f�[�^</param>
        /// <returns>���[�J�[�f�[�^</returns>
        private Maker ValidateMaker(Maker maker)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(maker.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", "���[�J�[�R�[�h"));
            }
            if (string.IsNullOrEmpty(maker.MakerName))
            {
                ModelState.AddModelError("MakerName", MessageUtils.GetMessage("E0001", "���[�J�[��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("MakerCode") && !CommonUtils.IsAlphaNumeric(maker.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0012", "���[�J�[�R�[�h"));
            }

            return maker;
        }

        /// <summary>
        /// ���[�J�[�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="maker">���[�J�[�f�[�^(�o�^���e)</param>
        /// <returns>���[�J�[�}�X�^���f���N���X</returns>
        private Maker EditMakerForInsert(Maker maker)
        {
            maker.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.CreateDate = DateTime.Now;
            maker.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.LastUpdateDate = DateTime.Now;
            maker.DelFlag = "0";
            return maker;
        }

        /// <summary>
        /// ���[�J�[�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="maker">���[�J�[�f�[�^(�o�^���e)</param>
        /// <returns>���[�J�[�}�X�^���f���N���X</returns>
        private Maker EditMakerForUpdate(Maker maker)
        {
            maker.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            maker.LastUpdateDate = DateTime.Now;
            return maker;
        }


        //Add 2015/01/21 arc nakayama  �ڋqDM�w�E�����@�Ԏ햼�̈ꗗ�����[�J�[�R�[�h����擾
        /// <summary>
        /// ���[�J�[�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>���[�J�[�ꗗ</returns>
        public void GetCarMasterList(string code, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();
                if (!string.IsNullOrEmpty(code))
                {
                    CarList = new V_CarMasterDao(db).GetListBykey(code);
                }
                else
                {
                    if (code2.Equals("0")){
                        CarList = new V_CarMasterDao(db).GetCarListBykey(null, null);
                    }else{
                        CarList = new V_CarMasterDao(db).GetCarListBykey(null, code2);
                    }
                }

                Maker maker = new MakerDao(db).GetByKey(code);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        
        //Add 2015/02/27 arc nakayama  �Ԏ햼�̈ꗗ��S���擾
        /// <summary>
        /// �Ԏ햼���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>�Ԏ�ꗗ</returns>
        public void GetCarMasterListAll()
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();
                
               CarList = new V_CarMasterDao(db).GetCarListBykey(null);
                
                Maker maker = new MakerDao(db).GetByKey(null);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        //GetPrivateCarList

        //Add 2015/02/27 arc nakayama  �Ԏ햼�̈ꗗ��S���擾
        /// <summary>
        /// �Ԏ햼���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>�Ԏ�ꗗ</returns>
        public void GetPrivateCarList(string PrivateFlag)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> CarList = new List<V_CarMaster>();

                CarList = new V_CarMasterDao(db).GetCarListBykey(null, PrivateFlag);

                Maker maker = new MakerDao(db).GetByKey(null);
                if (maker == null)
                {
                    maker = new Maker();
                    codeDataList.Code = "";
                    codeDataList.Name = "";
                }
                else
                {
                    codeDataList.Code = maker.MakerCode;
                    codeDataList.Name = maker.MakerName;
                }
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var car in CarList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }



        //Add 2015/01/21 arc nakayama  �ڋqDM�w�E�����@���[�J���̈ꗗ���擾
        /// <summary>
        /// ���[�J�[�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>���[�J�[�ꗗ</returns>
        public void GetMakerMasterList(string PrivateFlag = null)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();
                List<V_CarMaster> MakerList = new V_CarMasterDao(db).GetPrivateListBykey(null, PrivateFlag);
                codeDataList.Code = "1";
                codeDataList.Name = "1";
                codeDataList.DataList.Add(new CodeData { Code = "", Name = "" });
                foreach (var Maker in MakerList)
                {
                    codeDataList.DataList.Add(new CodeData { Code = Maker.MakerCode, Name = Maker.MakerName });
                }
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(CodeDataList));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, codeDataList);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }
        }

        /// <summary>
        /// ���[�J�[�����_�C�A���O�\��
        /// </summary>
        /// <returns>���[�J�[�����_�C�A���O</returns>
        public ActionResult CriteriaDialog2()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //�f�t�H���g�Ń��O�C���S���҂̉�ЃR�[�h���Z�b�g
            Employee employee = (Employee)Session["Employee"];
            string companyCode = "";
            try { companyCode = employee.Department1.Office.CompanyCode; }
            catch (NullReferenceException) { }
            //form["CompanyCode"] = companyCode;

            return CriteriaDialog2(form);
        }

        //Add 2015/01/21 arc nakayama  �ڋqDM�w�E�����@���[�J������Ԏ�����������ꗗ���擾
        /// <summary>
        /// ���[�J�[�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���[�J�[������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog2(FormCollection form)
        {
            // �������ʃ��X�g�̎擾
            PaginatedList<V_CarMaster> list;
            if (criteriaInit)
            {
                list = new PaginatedList<V_CarMaster>();
                ViewData["PrivateFlag"] = "0";
            }
            else
            {
                list = GetSearchResultListForDialog(form);
            }

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["CarCode"] = form["CarCode"];
            ViewData["PrivateFlag"] = form["PrivateFlag"];

            List<CodeData> data = new List<CodeData>();
            List<Maker> MakerList = new MakerDao(db).GetMakerBykey();
            foreach (var item in MakerList)
            {
                data.Add(new CodeData { Code = item.MakerCode, Name = item.MakerName });
            }
            ViewData["MakerList"] = CodeUtils.GetSelectListByModel(data, form["MakerCode"], false);

            List<CodeData> carData = new List<CodeData>();
            if (form["MakerCode"] != null)
            {
                List<V_CarMaster> CarMaster = new V_CarMasterDao(db).GetListBykey(form["MakerCode"]);
                foreach (var car in CarMaster)
                {
                    carData.Add(new CodeData { Code = car.CarCode, Name = car.CarName });
                }
            }
            ViewData["CarList"] = CodeUtils.GetSelectListByModel(carData, form["CarCode"], false);


            // �O���[�h������ʂ̕\��
            return View("MakerCriteriaDialog2", list);
        }

        private PaginatedList<V_CarMaster> GetSearchResultListForDialog(FormCollection form)
        {
            V_CarMaster V_CarMasterCondition = new V_CarMaster();

            V_CarMasterCondition.MakerCode = form["MakerCode"];
            V_CarMasterCondition.CarCode = form["CarCode"];

            //���Ў������̃��[�J�݂̂��o�͂��邩�A�S�f�[�^���o�͂��邩�𕪂���
            if (form["PrivateFlag"] == "0"){
                return new V_CarMasterDao(db).GetListByCondition(V_CarMasterCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE); 
            }
            else{
                return new V_CarMasterDao(db).GetPrivateListByCondition(V_CarMasterCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }
        }

        /// <summary>
        /// ���[�J�[�R�[�h����ԗ������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���[�J�[�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterDetail(string code)
        {

            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                V_CarMaster CarMaster = new V_CarMasterDao(db).GetBykey(code);
                if (CarMaster != null && CarMaster.MakerDelFlag.Equals("0"))
                {
                    ret.Add("MakerName", CarMaster.CarGradeCode);
                    ret.Add("CarName", CarMaster.CarGradeName);
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

    }
}
