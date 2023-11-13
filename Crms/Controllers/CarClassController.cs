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
using System.Transactions;
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �ԗ��N���X�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarClassController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��N���X�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�ԗ��N���X�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarClassController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �ԗ��N���X������ʕ\��
        /// </summary>
        /// <returns>�ԗ��N���X�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ��N���X������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��N���X�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<CarClass> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �ԗ��N���X������ʂ̕\��
            return View("CarClassCriteria", list);
        }

        /// <summary>
        /// �ԗ��N���X�����_�C�A���O�\��
        /// </summary>
        /// <returns>�ԗ��N���X�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �ԗ��N���X�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��N���X������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CarClassCode"] = Request["CarClassCode"];
            form["CarClassName"] = Request["CarClassName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<CarClass> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CarClassCode"] = form["CarClassCode"];
            ViewData["CarClassName"] = form["CarClassName"];

            // �ԗ��N���X������ʂ̕\��
            return View("CarClassCriteriaDialog", list);
        }

        /// <summary>
        /// �ԗ��N���X�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�ԗ��N���X�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�ԗ��N���X�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CarClass carClass;

            // �\���f�[�^�ݒ�(�ǉ��̏ꍇ)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                carClass = new CarClass();
            }
            // �\���f�[�^�ݒ�(�X�V�̏ꍇ)
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                carClass = new CarClassDao(db).GetByKey(id, true);
            }

            // �o��
            return View("CarClassEntry", carClass);
        }

        /// <summary>
        /// �ԗ��N���X�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="carClass">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�ԗ��N���X�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarClass carClass, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCarClass(carClass);
            if (!ModelState.IsValid)
            {
                return View("CarClassEntry", carClass);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope())
            {
                // �f�[�^�X�V����
                if (form["update"].Equals("1"))
                {
                    //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                    CarClass targetCarClass = new CarClassDao(db).GetByKey(carClass.CarClassCode, true);
                    UpdateModel(targetCarClass);
                    EditCarClassForUpdate(targetCarClass);
                }
                // �f�[�^�ǉ�����
                else
                {
                    // �f�[�^�ҏW
                    carClass = EditCarClassForInsert(carClass);
                    // �f�[�^�ǉ�
                    db.CarClass.InsertOnSubmit(carClass);
                }

                // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        if (form["update"].Equals("1"))
                        {
                            db.ChangeServiceCostMulti(null, carClass.CarClassCode, carClass.DelFlag, carClass.LastUpdateEmployeeCode);
                        }
                        else
                        {
                            db.AddServiceCostMulti(null, carClass.CarClassCode, carClass.CreateEmployeeCode);
                        }

                        ts.Complete();
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

                            ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0010", new string[] { "�ԗ��N���X�R�[�h", "�ۑ�" }));
                            return View("CarClassEntry", carClass);
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
            }
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(carClass.CarClassCode);
        }

        /// <summary>
        /// �ԗ��N���X�R�[�h����ԗ��N���X�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ԗ��N���X�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CarClass carClass = new CarClassDao(db).GetByKey(code);
                if (carClass != null)
                {
                    codeData.Code = carClass.CarClassCode;
                    codeData.Name = carClass.CarClassName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �ԗ��N���X�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��N���X�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<CarClass> GetSearchResultList(FormCollection form)
        {
            CarClassDao carClassDao = new CarClassDao(db);
            CarClass carClassCondition = new CarClass();
            carClassCondition.CarClassCode = form["CarClassCode"];
            carClassCondition.CarClassName = form["CarClassName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                carClassCondition.DelFlag = form["DelFlag"];
            }
            return carClassDao.GetListByCondition(carClassCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="carClass">�ԗ��N���X�f�[�^</param>
        /// <returns>�ԗ��N���X�f�[�^</returns>
        private CarClass ValidateCarClass(CarClass carClass)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(carClass.CarClassCode))
            {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0001", "�ԗ��N���X�R�[�h"));
            }
            if (string.IsNullOrEmpty(carClass.CarClassName))
            {
                ModelState.AddModelError("CarClassName", MessageUtils.GetMessage("E0001", "�ԗ��N���X��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CarClassCode") && !CommonUtils.IsAlphaNumeric(carClass.CarClassCode))
            {
                ModelState.AddModelError("CarClassCode", MessageUtils.GetMessage("E0012", "�ԗ��N���X�R�[�h"));
            }

            return carClass;
        }

        /// <summary>
        /// �ԗ��N���X�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carClass">�ԗ��N���X�f�[�^(�o�^���e)</param>
        /// <returns>�ԗ��N���X�}�X�^���f���N���X</returns>
        private CarClass EditCarClassForInsert(CarClass carClass)
        {
            carClass.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.CreateDate = DateTime.Now;
            carClass.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.LastUpdateDate = DateTime.Now;
            carClass.DelFlag = "0";
            return carClass;
        }

        /// <summary>
        /// �ԗ��N���X�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="carClass">�ԗ��N���X�f�[�^(�o�^���e)</param>
        /// <returns>�ԗ��N���X�}�X�^���f���N���X</returns>
        private CarClass EditCarClassForUpdate(CarClass carClass)
        {
            carClass.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            carClass.LastUpdateDate = DateTime.Now;
            return carClass;
        }

    }
}
