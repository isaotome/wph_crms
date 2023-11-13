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
    /// ����ŗ��}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ConsumptionTaxController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ConsumptionTaxController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        ///// <summary>
        ///// ����ŗ�������ʕ\��
        ///// </summary>
        ///// <returns>����ŗ��������</returns>
        //[AuthFilter]
        //public ActionResult Criteria()
        //{
        //    return Criteria(new FormCollection());
        //}

        ///// <summary>
        ///// ����ŗ�������ʕ\��
        ///// </summary>
        ///// <param name="form">�t�H�[���f�[�^(��������)</param>
        ///// <returns>����ŗ��������</returns>
        //[AuthFilter]
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Criteria(FormCollection form)
        //{
        //    // �f�t�H���g�l�̐ݒ�
        //    form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

        //     �������ʃ��X�g�̎擾
        //    PaginatedList<ConsumptionTax> list = GetSearchResultList(form);

        //     ���̑��o�͍��ڂ̐ݒ�
        //    ViewData["ConsumptionTaxId"] = form["ConsumptionTaxId"];
        //    ViewData["Rate"] = form["Rate"];
        //    ViewData["EmployeeName"] = form["EmployeeName"];
        //    ViewData["DelFlag"] = form["DelFlag"];

        //     ����ŗ�������ʂ̕\��
        //    return View("ConsumptionTaxCriteria", list);
        //}

        ///// <summary>
        ///// ����ŗ������_�C�A���O�\��
        ///// </summary>
        ///// <returns>����ŗ������_�C�A���O</returns>
        //public ActionResult CriteriaDialog()
        //{
        //    return CriteriaDialog(new FormCollection());
        //}

        ///// <summary>
        ///// ����ŗ������_�C�A���O�\��
        ///// </summary>
        ///// <param name="form">�t�H�[���f�[�^(��������)</param>
        ///// <returns>����ŗ�������ʃ_�C�A���O</returns>
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult CriteriaDialog(FormCollection form)
        //{
        //    // ���������̐ݒ�
        //    // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
        //    //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
        //    form["ConsumptionTaxId"] = Request["ConsumptionTaxId"];
        //    form["Rate"] = Request["Rate"];
        //    form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

        //    // �������ʃ��X�g�̎擾
        //    PaginatedList<ConsumptionTax> list = GetSearchResultList(form);

        //    // ���̑��o�͍��ڂ̐ݒ�
        //    ViewData["ConsumptionTaxId"] = form["ConsumptionTaxId"];
        //    ViewData["Rate"] = form["Rate"];
        //    ViewData["EmployeeName"] = form["EmployeeName"];

        //    // ����ŗ�������ʂ̕\��
        //    return View("ConsumptionTaxCriteriaDialog", list);
        //}

        ///// <summary>
        ///// ����ŗ��}�X�^���͉�ʕ\��
        ///// </summary>
        ///// <param name="id">����ŗ��R�[�h(�X�V���̂ݐݒ�)</param>
        ///// <returns>����ŗ��}�X�^���͉��</returns>
        //[AuthFilter]
        //public ActionResult Entry(string id)
        //{
        //    ConsumptionTax ConsumptionTax;

        //    // �ǉ��̏ꍇ
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        ViewData["update"] = "0";
        //        ConsumptionTax = new ConsumptionTax();
        //    }
        //    // �X�V�̏ꍇ
        //    else
        //    {
        //        ViewData["update"] = "1";
        //        ConsumptionTax = new ConsumptionTaxDao(db).GetByKey(id);
        //    }

        //    // ���̑��\���f�[�^�̎擾
        //    GetEntryViewData(ConsumptionTax);

        //    // �o��
        //    return View("ConsumptionTaxEntry", ConsumptionTax);
        //}
        
        ///// <summary>
        ///// ��ʕ\���f�[�^�̎擾
        ///// </summary>
        ///// <param name="ConsumptionTax">���f���f�[�^</param>
        //private void GetEntryViewData(ConsumptionTax ConsumptionTax)
        //{
        //    // ����ŗ��f�[�^�쐬�҂̎擾
        //    if (!string.IsNullOrEmpty(ConsumptionTax.CreateEmployeeCode))
        //    {
        //        EmployeeDao employeeDao = new EmployeeDao(db);
        //        Employee employee = employeeDao.GetByKey(ConsumptionTax.CreateEmployeeCode);
        //        if (employee != null)
        //        {
        //            //ConsumptionTax.CreateEmployeeCode = CreateEmployeeCode;
        //            //ViewData["EmployeeName"] = employee.EmployeeName;
        //        }
        //    }
        //}

        ///// <summary>
        ///// ����ŗ��}�X�^�ǉ��X�V
        ///// </summary>
        ///// <param name="ConsumptionTax">���f���f�[�^(�o�^���e)</param>
        ///// <param name="form">�t�H�[���f�[�^</param>
        ///// <returns>����ŗ��}�X�^���͉��</returns>
        //[AuthFilter]
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Entry(ConsumptionTax ConsumptionTax, FormCollection form)
        //{
        //    // �p���ێ�����o�͏��̐ݒ�
        //    ViewData["update"] = form["update"];

        //    // �f�[�^�`�F�b�N
        //    ValidateConsumptionTax(ConsumptionTax);
        //    if (!ModelState.IsValid)
        //    {
        //        GetEntryViewData(ConsumptionTax);
        //        return View("ConsumptionTaxEntry", ConsumptionTax);
        //    }

        //    // �f�[�^�X�V����
        //    if (form["update"].Equals("1"))
        //    {
        //        // �f�[�^�ҏW�E�X�V
        //        ConsumptionTax targetConsumptionTax = new ConsumptionTaxDao(db).GetByKey(ConsumptionTax.ConsumptionTaxId);
        //        UpdateModel(targetConsumptionTax);
        //        //EditConsumptionTaxForUpdate(targetConsumptionTax);
        //        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
        //        {
        //            try
        //            {
        //                db.SubmitChanges();
        //                break;
        //            }
        //            catch (ChangeConflictException e)
        //            {
        //                foreach (ObjectChangeConflict occ in db.ChangeConflicts)
        //                {
        //                    occ.Resolve(RefreshMode.KeepCurrentValues);
        //                }
        //                if (i + 1 >= DaoConst.MAX_RETRY_COUNT)
        //                {
        //                    throw e;
        //                }
        //            }
        //        }
        //    }
        //    // �f�[�^�ǉ�����
        //    else
        //    {
        //        // �f�[�^�ҏW
        //        ConsumptionTax = EditConsumptionTaxForInsert(ConsumptionTax);

        //        // �f�[�^�ǉ�
        //        db.ConsumptionTax.InsertOnSubmit(ConsumptionTax);
        //        try
        //        {
        //            db.SubmitChanges();
        //        }
        //        catch (SqlException e)
        //        {
        //            if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
        //            {
        //                ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0010", new string[] { "����ŗ��R�[�h", "�ۑ�" }));
        //                GetEntryViewData(ConsumptionTax);
        //                return View("ConsumptionTaxEntry", ConsumptionTax);
        //            }
        //            else
        //            {
        //                throw e;
        //            }
        //        }
        //    }

        //    // �o��
        //    ViewData["close"] = "1";
        //    return Entry((string)null);
        //}


        ///// <summary>
        ///// ����ŗ��}�X�^�������ʃ��X�g�擾
        ///// </summary>
        ///// <param name="form">�t�H�[���f�[�^(��������)</param>
        ///// <returns>����ŗ��}�X�^�������ʃ��X�g</returns>
        //private PaginatedList<ConsumptionTax> GetSearchResultList(FormCollection form)
        //{
        //    ConsumptionTaxDao ConsumptionTaxDao = new ConsumptionTaxDao(db);
        //    ConsumptionTax ConsumptionTaxCondition = new ConsumptionTax();
        //    ConsumptionTaxCondition.ConsumptionTaxId = form["ConsumptionTaxId"];
        //    ConsumptionTaxCondition.RateName = form["RateName"];
        //    ConsumptionTaxCondition.Rate = form["Rate"];
        //    ConsumptionTaxCondition.FromAvailableDate = form["FromAvailableDate"];
        //    ConsumptionTaxCondition.ToAvailableDate = form["ToAvailableDate"];
        //    if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
        //    {
        //        ConsumptionTaxCondition.DelFlag = form["DelFlag"];
        //    }
        //    return ConsumptionTaxDao.GetListByCondition(ConsumptionTaxCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        //}

        ///// <summary>
        ///// ���̓`�F�b�N
        ///// </summary>
        ///// <param name="ConsumptionTax">����ŗ��f�[�^</param>
        ///// <returns>����ŗ��f�[�^</returns>
        //private ConsumptionTax ValidateConsumptionTax(ConsumptionTax ConsumptionTax)
        //{
        //    // �K�{�`�F�b�N
        //    if (string.IsNullOrEmpty(ConsumptionTax.ConsumptionTaxId))
        //    {
        //        ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0001", "����ŗ��R�[�h"));
        //    }
        //    if (string.IsNullOrEmpty(ConsumptionTax.RateName))
        //    {
        //        ModelState.AddModelError("RateName", MessageUtils.GetMessage("E0001", "����ŗ���"));
        //    }
        //    //if (string.IsNullOrEmpty.(ConsumptionTax.Rate))
        //    //{
        //    //    ModelState.AddModelError("Rate", MessageUtils.GetMessage("E0001", "����ŗ�"));
        //    //}

        //    // �t�H�[�}�b�g�`�F�b�N
        //    if (ModelState.IsValidField("ConsumptionTaxId") && !CommonUtils.IsAlphaNumeric(ConsumptionTax.ConsumptionTaxId))
        //    {
        //        ModelState.AddModelError("ConsumptionTaxId", MessageUtils.GetMessage("E0012", "����ŗ��R�[�h"));
        //    }

        //    return ConsumptionTax;
        //}

        ///// <summary>
        ///// ����ŗ��}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        ///// </summary>
        ///// <param name="ConsumptionTax">����ŗ��f�[�^(�o�^���e)</param>
        ///// <returns>����ŗ��}�X�^���f���N���X</returns>
        //private ConsumptionTax EditConsumptionTaxForInsert(ConsumptionTax ConsumptionTax)
        //{
        //    ConsumptionTax.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        //    ConsumptionTax.CreateDate = DateTime.Now;
        //    ConsumptionTax.DelFlag = "0";
        //    return ConsumptionTax;
        //}

        /// <summary>
        /// ����ŗ�ID�������ŗ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">����ŗ��R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        [AcceptVerbs(HttpVerbs.Post)]           //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetRateById (string ConsumptionTaxId)
        {
            System.Nullable<int> Rate = null;
            
            if (Request.IsAjaxRequest())
            {
                //Edit 2014/06/20 arc yano �ŗ��ύX�o�O�Ή� ConsumptionTaxId��null�̑Ώ�
                if (ConsumptionTaxId != null)
                {
                    Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(ConsumptionTaxId));
                }
                
                Dictionary<string, System.Nullable<int>> ret = new Dictionary<string, System.Nullable<int>>();
                if (Rate != null)
                {
                    ret.Add("Rate", Rate);
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        [AcceptVerbs(HttpVerbs.Post)]   //Add 2014/05/27 arc yano vs2012�Ή�
        public ActionResult GetIdByDate(string strDt)
        {
            string ConsumptionTaxId = null;
           
            if (Request.IsAjaxRequest())
            {
                
                //Add 2014/06/20 arc yano �ŗ��ύX�o�O�Ή� strDt��null�̑Ώ�
                if (strDt != null)
                {
                    ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(DateTime.Parse(strDt));
                }
                
                Dictionary<string, string> ret = new Dictionary<string, string>();
                
                if (ConsumptionTaxId != null)
                {
                    ret.Add("ConsumptionTaxId", ConsumptionTaxId);
                }

                return Json(ret);
            }
            return new EmptyResult();
        }

    }
}
