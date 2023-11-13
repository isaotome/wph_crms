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

namespace Crms.Controllers
{
    /// <summary>
    /// ���σ��b�Z�[�W�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class QuoteMessageController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public QuoteMessageController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���σ��b�Z�[�W������ʕ\��
        /// </summary>
        /// <returns>���σ��b�Z�[�W�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���σ��b�Z�[�W������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���σ��b�Z�[�W�������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<QuoteMessage> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), form["QuoteType"], true);
            ViewData["DelFlag"] = form["DelFlag"];

            // ���σ��b�Z�[�W������ʂ̕\��
            return View("QuoteMessageCriteria", list);
        }

        /// <summary>
        /// ���σ��b�Z�[�W�����_�C�A���O�\��
        /// </summary>
        /// <returns>���σ��b�Z�[�W�����_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// ���σ��b�Z�[�W�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���σ��b�Z�[�W������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CompanyCode"] = Request["CompanyCode"];
            form["CompanyName"] = Request["CompanyName"];
            form["QuoteType"] = Request["QuoteType"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<QuoteMessage> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            CodeDao dao = new CodeDao(db);
            ViewData["CompanyCode"] = form["CompanyCode"];
            ViewData["CompanyName"] = form["CompanyName"];
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), form["QuoteType"], true);

            // ���σ��b�Z�[�W������ʂ̕\��
            return View("QuoteMessageCriteriaDialog", list);
        }

        /// <summary>
        /// ���σ��b�Z�[�W�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���σ��b�Z�[�W�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���σ��b�Z�[�W�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            QuoteMessage quoteMessage;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                quoteMessage = new QuoteMessage();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                string[] idArr = id.Split(new string[] { "," }, StringSplitOptions.None);
                quoteMessage = new QuoteMessageDao(db).GetByKey(idArr[0], idArr[1]);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(quoteMessage);

            // �o��
            return View("QuoteMessageEntry", quoteMessage);
        }

        /// <summary>
        /// ���σ��b�Z�[�W�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="quoteMessage">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���σ��b�Z�[�W�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(QuoteMessage quoteMessage, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateQuoteMessage(quoteMessage);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(quoteMessage);
                return View("QuoteMessageEntry", quoteMessage);
            }

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                QuoteMessage targetQuoteMessage = new QuoteMessageDao(db).GetByKey(quoteMessage.CompanyCode, quoteMessage.QuoteType);
                UpdateModel(targetQuoteMessage);
                EditQuoteMessageForUpdate(targetQuoteMessage);
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
                quoteMessage = EditQuoteMessageForInsert(quoteMessage);

                // �f�[�^�ǉ�
                db.QuoteMessage.InsertOnSubmit(quoteMessage);
                try
                {
                    db.SubmitChanges();
                }
                catch (SqlException e)
                {
                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0010", new string[] { "���,���ώ��", "�ۑ�" }));
                        ModelState.AddModelError("QuoteType", "");
                        GetEntryViewData(quoteMessage);
                        return View("QuoteMessageEntry", quoteMessage);
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
        /// ���σ��b�Z�[�W�R�[�h���猩�σ��b�Z�[�W�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code1">��ЃR�[�h</param>
        /// <param name="code2">���ώ��</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code1, string code2)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                QuoteMessage quoteMessage = new QuoteMessageDao(db).GetByKey(code1, code2);
                if (quoteMessage != null)
                {
                    codeData.Code = quoteMessage.CompanyCode;
                    codeData.Code2 = quoteMessage.QuoteType;
                    codeData.Name = quoteMessage.Description;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="quoteMessage">���f���f�[�^</param>
        private void GetEntryViewData(QuoteMessage quoteMessage)
        {
            // ��Ж��̎擾
            ViewData["CompanyName"] = "";
            if (!string.IsNullOrEmpty(quoteMessage.CompanyCode))
            {
                CompanyDao companyDao = new CompanyDao(db);
                Company company = companyDao.GetByKey(quoteMessage.CompanyCode);
                if (company != null)
                {
                    ViewData["CompanyName"] = company.CompanyName;
                }
            }

            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["QuoteTypeList"] = CodeUtils.GetSelectListByModel(dao.GetQuoteTypeAll(false), quoteMessage.QuoteType, true);
        }

        /// <summary>
        /// ���σ��b�Z�[�W�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���σ��b�Z�[�W�}�X�^�������ʃ��X�g</returns>
        private PaginatedList<QuoteMessage> GetSearchResultList(FormCollection form)
        {
            QuoteMessageDao quoteMessageDao = new QuoteMessageDao(db);
            QuoteMessage quoteMessageCondition = new QuoteMessage();
            quoteMessageCondition.QuoteType = form["QuoteType"];
            quoteMessageCondition.Company = new Company();
            quoteMessageCondition.Company.CompanyCode = form["CompanyCode"];
            quoteMessageCondition.Company.CompanyName = form["CompanyName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                quoteMessageCondition.DelFlag = form["DelFlag"];
            }
            return quoteMessageDao.GetListByCondition(quoteMessageCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="quoteMessage">���σ��b�Z�[�W�f�[�^</param>
        /// <returns>���σ��b�Z�[�W�f�[�^</returns>
        private QuoteMessage ValidateQuoteMessage(QuoteMessage quoteMessage)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(quoteMessage.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0001", "���"));
            }
            if (string.IsNullOrEmpty(quoteMessage.QuoteType))
            {
                ModelState.AddModelError("QuoteType", MessageUtils.GetMessage("E0001", "���ώ��"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CompanyCode") && !CommonUtils.IsAlphaNumeric(quoteMessage.CompanyCode))
            {
                ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0012", "���"));
            }
            if (ModelState.IsValidField("QuoteType") && !CommonUtils.IsAlphaNumeric(quoteMessage.QuoteType))
            {
                ModelState.AddModelError("QuoteType", MessageUtils.GetMessage("E0012", "���ώ��"));
            }

            // �d���`�F�b�N
            if (ModelState.IsValid)
            {
                if (new QuoteMessageDao(db).GetByKey(quoteMessage.CompanyCode, quoteMessage.QuoteType) != null)
                {
                    ModelState.AddModelError("CompanyCode", MessageUtils.GetMessage("E0006", "���,���ώ��"));
                    ModelState.AddModelError("QuoteType", "");
                }
            }
            
            return quoteMessage;
        }

        /// <summary>
        /// ���σ��b�Z�[�W�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="quoteMessage">���σ��b�Z�[�W�f�[�^(�o�^���e)</param>
        /// <returns>���σ��b�Z�[�W�}�X�^���f���N���X</returns>
        private QuoteMessage EditQuoteMessageForInsert(QuoteMessage quoteMessage)
        {
            quoteMessage.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.CreateDate = DateTime.Now;
            quoteMessage.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.LastUpdateDate = DateTime.Now;
            quoteMessage.DelFlag = "0";
            return quoteMessage;
        }

        /// <summary>
        /// ���σ��b�Z�[�W�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="quoteMessage">���σ��b�Z�[�W�f�[�^(�o�^���e)</param>
        /// <returns>���σ��b�Z�[�W�}�X�^���f���N���X</returns>
        private QuoteMessage EditQuoteMessageForUpdate(QuoteMessage quoteMessage)
        {
            quoteMessage.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            quoteMessage.LastUpdateDate = DateTime.Now;
            return quoteMessage;
        }

    }
}
