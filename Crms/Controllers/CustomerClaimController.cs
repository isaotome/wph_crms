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
using Crms.Models;                      //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ������}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CustomerClaimController : Controller
    {
        //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "������}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "������}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CustomerClaimController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �����挟����ʕ\��
        /// </summary>
        /// <returns>�����挟�����</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �����挟����ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�����挟�����</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<CustomerClaim> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["DelFlag"] = form["DelFlag"];

            // �����挟����ʂ̕\��
            return View("CustomerClaimCriteria", list);
        }

        /// <summary>
        /// �����挟���_�C�A���O�\��
        /// </summary>
        /// <returns>�����挟���_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �����挟���_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�����挟����ʃ_�C�A���O</returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� �N�G���X�g�����O����A�����敪�ނ��擾���鏈����ǉ�
        /// Add 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            //ADD #3111 �����挟���_�C�A���O�ɐ�����ʒǉ��Ή��@2014/10/20 arc ishii
            CustomerClaim customerClaim;
            customerClaim = new CustomerClaim();
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);

            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["CustomerClaimCode"] = Request["CustomerClaimCode"];
            form["CustomerClaimName"] = Request["CustomerClaimName"];
            form["CustomerClaimType"] = Request["CustomerClaimType"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            //Mod 2016/04/14 arc yano #3480 ���Ƃ̐����敪�ނɂ��A���������̐������ʃ��X�g��ύX����
            form["SWCustomerClaimClass"] = Request["SWCustomerClaimClass"];   
            
            List<c_CustomerClaimType> typeList = new List<c_CustomerClaimType>();
            if (string.IsNullOrEmpty(form["SWCustomerClaimClass"]))          //������ނ��ݒ肳��Ă��Ȃ��ꍇ
            {
                typeList = null;
                ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);
            }
            else
            {
                typeList = dao.GetCustomerClaimTypeAll(false).Where(x => string.IsNullOrWhiteSpace(x.CustomerClaimClass) || x.CustomerClaimClass.Equals(form["SWCustomerClaimClass"])).ToList<c_CustomerClaimType>();

                /*
                if (form["SWCustomerClaimClass"].Equals("2"))  //�Г�
                {
                    ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, false);
                }
                else
                {
                    ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, true);
                }
                */

                ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(typeList, customerClaim.CustomerClaimType, true);

            }
        
            //ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);

            // �������ʃ��X�g�̎擾
            PaginatedList<CustomerClaim> list = GetSearchResultList(form, typeList);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            //ADD #3111 �����挟���_�C�A���O�ɐ�����ʒǉ��Ή��@2014/10/20 arc ishii
            ViewData["CustomerClaimType"] = form["CustomerClaimType"];
            ViewData["SWCustomerClaimClass"] = form["SWCustomerClaimClass"];    //Mod 2016/04/14 arc yano #3480
            // �����挟����ʂ̕\��
            return View("CustomerClaimCriteriaDialog", list);
        }

        /// <summary>
        /// ������}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">������R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>������}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            CustomerClaim customerClaim;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                customerClaim = new CustomerClaim();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                customerClaim = new CustomerClaimDao(db).GetByKey(id);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(customerClaim);

            // �o��
            return View("CustomerClaimEntry", customerClaim);
        }

        /// <summary>
        /// ������}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="customerClaim">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>������}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CustomerClaim customerClaim, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateCustomerClaim(customerClaim);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(customerClaim);
                return View("CustomerClaimEntry", customerClaim);
            }

            // Add 2014/08/12 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                CustomerClaim targetCustomerClaim = new CustomerClaimDao(db).GetByKey(customerClaim.CustomerClaimCode);
                UpdateModel(targetCustomerClaim);
                EditCustomerClaimForUpdate(targetCustomerClaim);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                customerClaim = EditCustomerClaimForInsert(customerClaim);
                // �f�[�^�ǉ�
                db.CustomerClaim.InsertOnSubmit(customerClaim);
            }

            //Add 2014/08/12 arc amii �G���[���O�Ή� SubmitChanges��������{�� & Exception���ɃG���[���O�o�͏����ǉ�
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
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0010", new string[] { "������R�[�h", "�ۑ�" }));
                        GetEntryViewData(customerClaim);
                        return View("CustomerClaimEntry", customerClaim);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
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

            // �o��
            ViewData["close"] = "1";
            return Entry((string)null);
        }

        /// <summary>
        /// ������R�[�h���琿���於���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">������R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2016/04/13 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� ���Ƒ啪�ނ��擾����
        /// </history>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(code);
                if (customerClaim != null)
                {
                    codeData.Code = customerClaim.CustomerClaimCode;
                    codeData.Name = customerClaim.CustomerClaimName;
                    codeData.Code2 = (customerClaim.c_CustomerClaimType != null ? customerClaim.c_CustomerClaimType.CustomerClaimClass != null ? customerClaim.c_CustomerClaimType.CustomerClaimClass : "" : "");       //Add 2016/04/13 arc yano #3480
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ������R�[�h���琿���於�A���ώ�ʃ��X�g���擾����(Ajax��p)
        /// </summary>
        /// <param name="code">������R�[�h</param>
        /// <returns></returns>
        public void GetMasterWithClaimable(string code) {
            if (Request.IsAjaxRequest()) {
                CustomerClaim claim = new CustomerClaimDao(db).GetByKey(code);
                CodeDataList codeDataList = new CodeDataList();
                if (claim != null){
                    codeDataList.Code = claim.CustomerClaimCode;
                    codeDataList.Name = claim.CustomerClaimName;
                    if (claim.CustomerClaimable != null) {
                        codeDataList.DataList = new List<CodeData>();
                        foreach (var a in claim.CustomerClaimable) {
                            codeDataList.DataList.Add(new CodeData() { Code = a.PaymentKindCode, Name = a.PaymentKind != null ? a.PaymentKind.PaymentKindName : "" });
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
        /// <param name="customerClaim">���f���f�[�^</param>
        private void GetEntryViewData(CustomerClaim customerClaim)
        {
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false), customerClaim.CustomerClaimType, true);
            ViewData["PaymentKindTypeList"] = CodeUtils.GetSelectListByModel(dao.GetOnOffAll(false), customerClaim.PaymentKindType, true);
            ViewData["RoundTypeList"] = CodeUtils.GetSelectListByModel(dao.GetRoundTypeAll(false), customerClaim.RoundType, true);

        }

        /// <summary>
        /// ������}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <param name="typeList">������^�C�v���X�g(��������)</param>
        /// <returns>������}�X�^�������ʃ��X�g</returns>
        /// <history>
        /// 2016/04/13 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� ������^�C�v�̃��X�g�������Ƃ��Ď󂯎��
        /// </history>
        private PaginatedList<CustomerClaim> GetSearchResultList(FormCollection form, List<c_CustomerClaimType> typeList = null)
        {
            CustomerClaimDao customerClaimDao = new CustomerClaimDao(db);
            CustomerClaim customerClaimCondition = new CustomerClaim();
            customerClaimCondition.CustomerClaimCode = form["CustomerClaimCode"];
            customerClaimCondition.CustomerClaimName = form["CustomerClaimName"];
            //ADD #3111 �����挟���_�C�A���O�ɐ�����ʌ����Ή��@2014/10/20 arc ishii
            customerClaimCondition.CustomerClaimType = form["CustomerClaimType"];

            //Mod 2016/04/14 #3480 arc yano
            if (typeList != null)
            {
                customerClaimCondition.CustomerClaimTypeList = typeList.Select(x => x.Code).ToList();
            }

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                customerClaimCondition.DelFlag = form["DelFlag"];
            }

            return customerClaimDao.GetListByCondition(customerClaimCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="customerClaim">������f�[�^</param>
        /// <returns>������f�[�^</returns>
        private CustomerClaim ValidateCustomerClaim(CustomerClaim customerClaim)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0001", "������R�[�h"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimName))
            {
                ModelState.AddModelError("CustomerClaimName", MessageUtils.GetMessage("E0001", "�����於"));
            }
            if (string.IsNullOrEmpty(customerClaim.CustomerClaimType))
            {
                ModelState.AddModelError("CustomerClaimType", MessageUtils.GetMessage("E0001", "�������"));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("CustomerClaimCode") && !CommonUtils.IsAlphaNumeric(customerClaim.CustomerClaimCode))
            {
                ModelState.AddModelError("CustomerClaimCode", MessageUtils.GetMessage("E0012", "������R�[�h"));
            }

            return customerClaim;
        }

        /// <summary>
        /// ������}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="customerClaim">������f�[�^(�o�^���e)</param>
        /// <returns>������}�X�^���f���N���X</returns>
        private CustomerClaim EditCustomerClaimForInsert(CustomerClaim customerClaim)
        {
            customerClaim.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.CreateDate = DateTime.Now;
            customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.LastUpdateDate = DateTime.Now;
            customerClaim.DelFlag = "0";
            return customerClaim;
        }

        /// <summary>
        /// ������}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="customerClaim">������f�[�^(�o�^���e)</param>
        /// <returns>������}�X�^���f���N���X</returns>
        private CustomerClaim EditCustomerClaimForUpdate(CustomerClaim customerClaim)
        {
            customerClaim.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            customerClaim.LastUpdateDate = DateTime.Now;
            return customerClaim;
        }

    }
}
