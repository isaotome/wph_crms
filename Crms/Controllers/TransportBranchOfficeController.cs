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

namespace Crms.Controllers
{
    /// <summary>
    /// �^�A�x�ǃ}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TransportBranchOfficeController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TransportBranchOfficeController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �^�A�x�ǌ�����ʕ\��
        /// </summary>
        /// <returns>�^�A�x�ǌ������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �^�A�x�ǌ�����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�^�A�x�ǌ������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<TransportBranchOffice> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["TransportBranchOfficeCode"] = form["TransportBranchOfficeCode"];
            ViewData["TransportBranchOfficeName"] = form["TransportBranchOfficeName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �^�A�x�ǌ�����ʂ̕\��
            return View("TransportBranchOfficeCriteria", list);
        }

        /// <summary>
        /// �^�A�x�ǌ����_�C�A���O�\��
        /// </summary>
        /// <returns>�^�A�x�ǌ����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �^�A�x�ǌ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�^�A�x�ǌ�����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["TransportBranchOfficeCode"] = Request["TransportBranchOfficeCode"];
            form["TransportBranchOfficeName"] = Request["TransportBranchOfficeName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<TransportBranchOffice> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["TransportBranchOfficeCode"] = form["TransportBranchOfficeCode"];
            ViewData["TransportBranchOfficeName"] = form["TransportBranchOfficeName"];

            // �^�A�x�ǌ�����ʂ̕\��
            return View("TransportBranchOfficeCriteriaDialog", list);
        }

        /// <summary>
        /// �^�A�x�ǃ}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�^�A�x�ǃR�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�^�A�x�ǃ}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            TransportBranchOffice transportBranchOffice;

            // �\���f�[�^�ݒ�(�ǉ��̏ꍇ)
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                transportBranchOffice = new TransportBranchOffice();
            }
            // �\���f�[�^�ݒ�(�X�V�̏ꍇ)
            else
            {
                ViewData["update"] = "1";
                transportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(id);
            }

            // �o��
            return View("TransportBranchOfficeEntry", transportBranchOffice);
        }

        /// <summary>
        /// �^�A�x�ǃ}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="transportBranchOffice">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�^�A�x�ǃ}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(TransportBranchOffice transportBranchOffice, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateTransportBranchOffice(transportBranchOffice);
            if (!ModelState.IsValid)
            {
                return View("TransportBranchOfficeEntry", transportBranchOffice);
            }

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                TransportBranchOffice targetTransportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(transportBranchOffice.TransportBranchOfficeCode);
                UpdateModel(targetTransportBranchOffice);
                EditTransportBranchOfficeForUpdate(targetTransportBranchOffice);
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
                            throw e;
                        }
                    }
                }
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                transportBranchOffice = EditTransportBranchOfficeForInsert(transportBranchOffice);

                // �f�[�^�ǉ�
                db.TransportBranchOffice.InsertOnSubmit(transportBranchOffice);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0010", new string[] { "�^�A�x�ǃR�[�h", "�ۑ�" }));
                        return View("TransportBranchOfficeEntry", transportBranchOffice);
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// �^�A�x�ǃR�[�h����^�A�x�ǖ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�^�A�x�ǃR�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                TransportBranchOffice transportBranchOffice = new TransportBranchOfficeDao(db).GetByKey(code);
                if (transportBranchOffice != null)
                {
                    codeData.Code = transportBranchOffice.TransportBranchOfficeCode;
                    codeData.Name = transportBranchOffice.TransportBranchOfficeName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �^�A�x�ǃ}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�^�A�x�ǃ}�X�^�������ʃ��X�g</returns>
        private PaginatedList<TransportBranchOffice> GetSearchResultList(FormCollection form)
        {
            TransportBranchOfficeDao transportBranchOfficeDao = new TransportBranchOfficeDao(db);
            TransportBranchOffice transportBranchOfficeCondition = new TransportBranchOffice();
            transportBranchOfficeCondition.TransportBranchOfficeCode = form["TransportBranchOfficeCode"];
            transportBranchOfficeCondition.TransportBranchOfficeName = form["TransportBranchOfficeName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                transportBranchOfficeCondition.DelFlag = form["DelFlag"];
            }
            return transportBranchOfficeDao.GetListByCondition(transportBranchOfficeCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="transportBranchOffice">�^�A�x�ǃf�[�^</param>
        /// <returns>�^�A�x�ǃf�[�^</returns>
        private TransportBranchOffice ValidateTransportBranchOffice(TransportBranchOffice transportBranchOffice)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(transportBranchOffice.TransportBranchOfficeCode))
            {
                ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0001", "�^�A�x�ǃR�[�h"));
            }
            if (string.IsNullOrEmpty(transportBranchOffice.TransportBranchOfficeName))
            {
                ModelState.AddModelError("TransportBranchOfficeName", MessageUtils.GetMessage("E0001", "�^�A�x�ǖ�"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("NormalPaintPrice"))
            {
                ModelState.AddModelError("NormalPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@�m�[�}��(�y�C���g)", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("NormalFluorescencePrice"))
            {
                ModelState.AddModelError("NormalFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@�m�[�}��(����)", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("OrderPaintPrice"))
            {
                ModelState.AddModelError("OrderPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@��](�y�C���g)", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("OrderFluorescencePrice"))
            {
                ModelState.AddModelError("OrderFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@��](����)", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("TransportBranchOfficeCode") && !CommonUtils.IsAlphaNumeric(transportBranchOffice.TransportBranchOfficeCode))
            {
                ModelState.AddModelError("TransportBranchOfficeCode", MessageUtils.GetMessage("E0012", "�^�A�x�ǃR�[�h"));
            }
            if (ModelState.IsValidField("NormalPaintPrice") && transportBranchOffice.NormalPaintPrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.NormalPaintPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("NormalPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@�m�[�}��(�y�C���g)", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("NormalFluorescencePrice") && transportBranchOffice.NormalFluorescencePrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.NormalFluorescencePrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("NormalFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@�m�[�}��(����)", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("OrderPaintPrice") && transportBranchOffice.OrderPaintPrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.OrderPaintPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("OrderPaintPrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@��](�y�C���g)", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("OrderFluorescencePrice") && transportBranchOffice.OrderFluorescencePrice != null)
            {
                if (!Regex.IsMatch(transportBranchOffice.OrderFluorescencePrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("OrderFluorescencePrice", MessageUtils.GetMessage("E0004", new string[] { "�i���o�[�v���[�g��@��](����)", "���̐����̂�" }));
                }
            }
            
            return transportBranchOffice;
        }

        /// <summary>
        /// �^�A�x�ǃ}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="transportBranchOffice">�^�A�x�ǃf�[�^(�o�^���e)</param>
        /// <returns>�^�A�x�ǃ}�X�^���f���N���X</returns>
        private TransportBranchOffice EditTransportBranchOfficeForInsert(TransportBranchOffice transportBranchOffice)
        {
            transportBranchOffice.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.CreateDate = DateTime.Now;
            transportBranchOffice.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.LastUpdateDate = DateTime.Now;
            transportBranchOffice.DelFlag = "0";
            return transportBranchOffice;
        }

        /// <summary>
        /// �^�A�x�ǃ}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="transportBranchOffice">�^�A�x�ǃf�[�^(�o�^���e)</param>
        /// <returns>�^�A�x�ǃ}�X�^���f���N���X</returns>
        private TransportBranchOffice EditTransportBranchOfficeForUpdate(TransportBranchOffice transportBranchOffice)
        {
            transportBranchOffice.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            transportBranchOffice.LastUpdateDate = DateTime.Now;
            return transportBranchOffice;
        }

    }
}
