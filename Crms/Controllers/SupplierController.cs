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
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// �d����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SupplierController : Controller
    {

        private static readonly string FORM_NAME = "�d����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�d����}�X�^�o�^"; // ������


        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SupplierController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �d���挟����ʕ\��
        /// </summary>
        /// <returns>�d���挟�����</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �d���挟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�d���挟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Supplier> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["SupplierCode"] = form["SupplierCode"];
            ViewData["SupplierName"] = form["SupplierName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �d���挟����ʂ̕\��
            return View("SupplierCriteria", list);
        }

        /// <summary>
        /// �d���挟���_�C�A���O�\��
        /// </summary>
        /// <returns>�d���挟���_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �d���挟���_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�d���挟����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["SupplierCode"] = Request["SupplierCode"];
            form["SupplierName"] = Request["SupplierName"];
            form["OutsourceFlag"] = Request["OutsourceFlag"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, form["OutsourceFlag"] , true);

            // �������ʃ��X�g�̎擾
            PaginatedList<Supplier> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["SupplierCode"] = form["SupplierCode"];
            ViewData["SupplierName"] = form["SupplierName"];

            // �d���挟����ʂ̕\��
            return View("SupplierCriteriaDialog", list);
        }

        /// <summary>
        /// �d����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">�d����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>�d����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Supplier supplier;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                supplier = new Supplier();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                supplier = new SupplierDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(supplier);

            // �o��
            return View("SupplierEntry", supplier);
        }

        /// <summary>
        /// �d����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="supplier">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�d����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Supplier supplier, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateSupplier(supplier);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(supplier);
                return View("SupplierEntry", supplier);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Supplier targetSupplier = new SupplierDao(db).GetByKey(supplier.SupplierCode, true);
                UpdateModel(targetSupplier);
                EditSupplierForUpdate(targetSupplier);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                supplier = EditSupplierForInsert(supplier);
                // �f�[�^�ǉ�
                db.Supplier.InsertOnSubmit(supplier);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
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
                        OutputLogger.NLogFatal(ce, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (SqlException se)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {   
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0010", new string[] { "�d����R�[�h", "�ۑ�" }));
                        GetEntryViewData(supplier);
                        return View("SupplierEntry", supplier);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
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
            return Entry(supplier.SupplierCode);
        }

        /// <summary>
        /// �d����R�[�h����d���於���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�d����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Supplier supplier = new SupplierDao(db).GetByKey(code);
                if (supplier != null)
                {
                    codeData.Code = supplier.SupplierCode;
                    codeData.Name = supplier.SupplierName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="supplier">���f���f�[�^</param>
        private void GetEntryViewData(Supplier supplier)
        {
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["OutsourceFlagList"] = CodeUtils.GetSelectList(CodeUtils.OutsourceFlag, supplier.OutsourceFlag, true);
        }

        /// <summary>
        /// �d����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�d����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Supplier> GetSearchResultList(FormCollection form)
        {
            SupplierDao supplierDao = new SupplierDao(db);
            Supplier supplierCondition = new Supplier();
            supplierCondition.SupplierCode = form["SupplierCode"];
            supplierCondition.SupplierName = form["SupplierName"];
            supplierCondition.OutsourceFlag = form["OutsourceFlag"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                supplierCondition.DelFlag = form["DelFlag"];
            }
            return supplierDao.GetListByCondition(supplierCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="supplier">�d����f�[�^</param>
        /// <returns>�d����f�[�^</returns>
        private Supplier ValidateSupplier(Supplier supplier)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(supplier.SupplierCode))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "�d����R�[�h"));
            }
            if (string.IsNullOrEmpty(supplier.SupplierName))
            {
                ModelState.AddModelError("SupplierName", MessageUtils.GetMessage("E0001", "�d���於"));
            }
            if (string.IsNullOrEmpty(supplier.OutsourceFlag))
            {
                ModelState.AddModelError("OutsourceFlag", MessageUtils.GetMessage("E0001", "�O���t���O"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("SupplierCode") && !CommonUtils.IsAlphaNumeric(supplier.SupplierCode))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0012", "�d����R�[�h"));
            }

            return supplier;
        }

        /// <summary>
        /// �d����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="supplier">�d����f�[�^(�o�^���e)</param>
        /// <returns>�d����}�X�^���f���N���X</returns>
        private Supplier EditSupplierForInsert(Supplier supplier)
        {
            supplier.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.CreateDate = DateTime.Now;
            supplier.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.LastUpdateDate = DateTime.Now;
            supplier.DelFlag = "0";
            return supplier;
        }

        /// <summary>
        /// �d����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="supplier">�d����f�[�^(�o�^���e)</param>
        /// <returns>�d����}�X�^���f���N���X</returns>
        private Supplier EditSupplierForUpdate(Supplier supplier)
        {
            supplier.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            supplier.LastUpdateDate = DateTime.Now;
            return supplier;
        }

    }
}
