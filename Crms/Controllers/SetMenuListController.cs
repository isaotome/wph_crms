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
using System.Transactions;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers {

    /// <summary>
    /// �Z�b�g���j���[�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class SetMenuListController : Controller {

        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�Z�b�g���j���[���X�g�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "�Z�b�g���j���[���X�g�}�X�^�o�^"; // ������
        private string errorFlg = ""; // �G���[��ʑJ�ڏ�� 0:�Ȃ� 1:����� 2�F�G���[���

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public SetMenuListController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ��������������ʕ\��
        /// </summary>
        /// <returns>���������������</returns>
        [AuthFilter]
        public ActionResult Criteria() {
            return Criteria(new List<SetMenuList>(), new FormCollection());
        }

        /// <summary>
        /// �Z�b�g���j���[������ʕ\��
        /// </summary>
        /// <param name="line">���f���f�[�^(�������̓o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���������������</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(List<SetMenuList> line, FormCollection form) {

            List<SetMenuList> list = null;

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            form["action"] = (form["action"] == null ? "" : form["action"]);

            // ����{�^���ɂ�鏈������
            switch (form["action"]) {

                // ���׍s�ǉ��y�эs�폜����
                case "line":

                    if (line == null) {
                        line = new List<SetMenuList>();
                    }

                    string delLine = form["DelLine"];
                    if (!string.IsNullOrEmpty(delLine)) {

                        // �w��s�폜�܂��͍ŏI�s�ǉ�
                        if (Int32.Parse(delLine) >= 0) {
                            line.RemoveAt(Int32.Parse(delLine));
                        } else {
                            SetMenuList setMenu = new SetMenuList();
                            setMenu.SetMenuCode = form["SetMenuCode"];

                            //2�s�ڈȍ~�Ȃ�O�s�̎�ނ��f�t�H���g�Z�b�g
                            if (line.Count > 0 && line[line.Count - 1] != null) {
                                setMenu.ServiceType = line[line.Count - 1].ServiceType;
                            } else {
                                setMenu.ServiceType = "001";
                            }

                            line.Add(setMenu);
                        }

                        // ���׍s���쌋�ʂ��������ʃ��X�g�Ƃ���
                        list = line;
                    }

                    break;

                // �Z�b�g���j���[�o�^����
                case "regist":

                    if (line != null && line.Count > 0) {

                        // �f�[�^�`�F�b�N
                        ValidateSetMenu(line, form);
                        if (!ModelState.IsValid) {
                            GetCriteriaViewData(line, form);
                            return View("SetMenuListCriteria", line);
                        }

                        // �f�[�^���֏���
                        ReplaceSetMenu(line);

                        // Add 2014/08/05 arc amii �G���[���O�Ή� ���֏����ŃG���[�̏ꍇ�A�Ή������G���[��ʂɑJ�ڂ���
                        if ("1".Equals(errorFlg)) {
                            // �Z�b�g���j���[���X�g��ʂɑJ�ڂ���
                            GetCriteriaViewData(line, form);
                            return View("SetMenuListCriteria", line);
                        }
                        else if ("2".Equals(errorFlg))
                        {
                            // �G���[��ʂɑJ�ڂ���
                            return View("Error");
                        }

                    }

                    // �������ʃ��X�g�̎擾
                    list = GetSearchResultList(form);

                    break;

                // �Z�b�g���j���[��������
                default:

                    // �������ʃ��X�g�̎擾
                    list = GetSearchResultList(form);

                    break;
            }

            // ���̑��o�͍��ڂ̐ݒ�
            GetCriteriaViewData(list, form);

            // �o��
            ModelState.Clear();
            return View("SetMenuListCriteria", list);
        }

        /// <summary>
        /// ������ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="line">���f���f�[�^(�������̓o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        private void GetCriteriaViewData(List<SetMenuList> line, FormCollection form) {

            CodeDao dao = new CodeDao(db);

            // �����������̉�ʕ\���f�[�^�擾
            ViewData["SetMenuCode"] = form["SetMenuCode"];
            if (!string.IsNullOrEmpty(form["SetMenuCode"])) {
                SetMenu setMenu = new SetMenuDao(db).GetByKey(form["SetMenuCode"]);
                if (setMenu != null) {
                    ViewData["SetMenuName"] = setMenu.SetMenuName;
                }
            }

            // �Z�b�g���j���[���ו��̉�ʕ\���f�[�^�擾
            List<IEnumerable<SelectListItem>> serviceTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> workTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> autoSetAmountList = new List<IEnumerable<SelectListItem>>();
            List<string> serviceWorkNameList = new List<string>();
            List<string> serviceMenuNameList = new List<string>();
            List<string> partsNameJpList = new List<string>();
            List<c_ServiceType> serviceTypeListSrc = dao.GetServiceTypeAll(false);
            List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false);
            List<c_OnOff> autoSetAmountSrc = dao.GetOnOffAll(false);
            ServiceWorkDao serviceWorkDao = new ServiceWorkDao(db);
            ServiceMenuDao serviceMenuDao = new ServiceMenuDao(db);
            PartsDao partsDao = new PartsDao(db);
            for (int i = 0; i < line.Count; i++) {
                
                //�T�[�r�X��ʃ��X�g
                serviceTypeList.Add(CodeUtils.GetSelectListByModel(serviceTypeListSrc, line[i].ServiceType, false));
                workTypeList.Add(CodeUtils.GetSelectListByModel(workTypeListSrc, line[i].WorkType, true));
                autoSetAmountList.Add(CodeUtils.GetSelectListByModel(autoSetAmountSrc, line[i].AutoSetAmount, false));

                //���Ɩ�
                string serviceWorkName = "";
                if (!string.IsNullOrEmpty(line[i].ServiceWorkCode)) {
                    ServiceWork serviceWork = serviceWorkDao.GetByKey(line[i].ServiceWorkCode);
                    if (serviceWork != null) {
                        serviceWorkName = serviceWork.Name;
                    }
                }
                serviceWorkNameList.Add(serviceWorkName);

                //�T�[�r�X���j���[��
                string serviceManuName = "";
                if (!string.IsNullOrEmpty(line[i].ServiceMenuCode)) {
                    ServiceMenu serviceMenu = serviceMenuDao.GetByKey(line[i].ServiceMenuCode);
                    if (serviceMenu != null) {
                        serviceManuName = serviceMenu.ServiceMenuName;
                    }
                }
                serviceMenuNameList.Add(serviceManuName);

                //���i��
                string partsNameJp = "";
                if (!string.IsNullOrEmpty(line[i].PartsNumber)) {
                    Parts parts = partsDao.GetByKey(line[i].PartsNumber);
                    if (parts != null) {
                        partsNameJp = parts.PartsNameJp;
                    }
                }

                partsNameJpList.Add(partsNameJp);
            }
            ViewData["ServiceWorkNameList"] = serviceWorkNameList;
            ViewData["ServiceTypeList"] = serviceTypeList;
            ViewData["WorkTypeList"] = workTypeList;
            ViewData["ServiceMenuNameList"] = serviceMenuNameList;
            ViewData["PartsNameJpList"] = partsNameJpList;
            ViewData["AutoSetAmountList"] = autoSetAmountList;
        }

        /// <summary>
        /// �Z�b�g���j���[�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���������������ʃ��X�g</returns>
        private List<SetMenuList> GetSearchResultList(FormCollection form) {

            SetMenuList setMenuCondition = new SetMenuList();
            setMenuCondition.SetMenuCode = form["SetMenuCode"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                setMenuCondition.DelFlag = form["DelFlag"];
            }
            return new SetMenuListDao(db).GetListByCondition(setMenuCondition);
        }

        /// <summary>
        /// �Z�b�g���j���[���̓`�F�b�N
        /// </summary>
        /// <param name="line">�Z�b�g���j���[���׃f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���������f�[�^</returns>
        private List<SetMenuList> ValidateSetMenu(List<SetMenuList> line, FormCollection form) {

            bool alreadyOutputMsgE0001a = false;
            bool alreadyOutputMsgE0001b = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0001c = false;
            bool alreadyOutputMsgE0003 = false;

            for (int i = 0; i < line.Count; i++) {

                SetMenuList setMenu = line[i];
                string prefix = "line[" + CommonUtils.IntToStr(i) + "].";
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (!string.IsNullOrEmpty(setMenu.ServiceWorkCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (!string.IsNullOrEmpty(setMenu.ServiceMenuCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (!string.IsNullOrEmpty(setMenu.PartsNumber)))
                    || (setMenu.InputDetailsNumber != null)) {

                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    // �K�{�`�F�b�N(���l/���t���ڂ͑����`�F�b�N�����˂�)
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (string.IsNullOrEmpty(setMenu.ServiceWorkCode))) {
                        ModelState.AddModelError(prefix + "ServiceWorkCode", (alreadyOutputMsgE0001c ? "" : MessageUtils.GetMessage("E0001", "����")));
                        alreadyOutputMsgE0001a = true;
                    }
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (string.IsNullOrEmpty(setMenu.ServiceMenuCode))) {
                        ModelState.AddModelError(prefix + "ServiceMenuCode", (alreadyOutputMsgE0001a ? "" : MessageUtils.GetMessage("E0001", "�T�[�r�X���j���[")));
                        alreadyOutputMsgE0001a = true;
                    }
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (string.IsNullOrEmpty(setMenu.PartsNumber))) {
                        ModelState.AddModelError(prefix + "PartsNumber", (alreadyOutputMsgE0001b ? "" : MessageUtils.GetMessage("E0001", "���i")));
                        alreadyOutputMsgE0001b = true;
                    }
                    if (setMenu.InputDetailsNumber == null) {
                        ModelState.AddModelError(prefix + "InputDetailsNumber", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "�\����", "���̐����̂�" })));
                        alreadyOutputMsgE0002 = true;
                    }

                    // �l�`�F�b�N
                    if (ModelState.IsValidField(prefix + "InputDetailsNumber")) {
                        if (setMenu.InputDetailsNumber < 0) {
                            ModelState.AddModelError(prefix + "InputDetailsNumber", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "�\����", "���̐����̂�" })));
                            alreadyOutputMsgE0002 = true;
                        }
                    }
                    if (setMenu.Quantity != null) {
                        if ((Regex.IsMatch(setMenu.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(setMenu.Quantity.ToString(), @"^\d{1,7}$")))) {
                        } else {
                            ModelState.AddModelError(prefix + "Quantity", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" })));
                            alreadyOutputMsgE0003 = true;
                        }
                    }
                }
            }

            return line;
        }

        /// <summary>
        /// �Z�b�g���j���[���֏���
        /// </summary>
        /// <param name="line">�Z�b�g���j���[�f�[�^</param>
        private void ReplaceSetMenu(List<SetMenuList> line) {

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            using (TransactionScope ts = new TransactionScope()) {

                // �������R�[�h�̍폜
                db.RemoveSetMenuList(line[0].SetMenuCode);

                // �V�f�[�^�̕ҏW
                line = EditSetMenuForReplace(line);

                // �V�f�[�^�̓o�^
                for (int i = 0; i < line.Count; i++) {
                    db.SetMenuList.InsertOnSubmit(line[i]);
                }

                // Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�͂���ׂ�try catch���ǉ�
                try
                {
                    errorFlg = "0";
                    // �g�����U�N�V�����̃R�~�b�g
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");

                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        errorFlg = "1";
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        errorFlg = "2";
                    }
                }
                catch (Exception e)
                {
                    // ��L�ȊO�̗�O�̏ꍇ�A�G���[���O�o�͂��A�G���[��ʂɑJ�ڂ���
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    errorFlg = "2";
                }
            }
        }

        /// <summary>
        /// �Z�b�g���j���[�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="list">�Z�b�g���j���[�f�[�^�S����(�o�^���e)</param>
        /// <returns>�Z�b�g���j���[�}�X�^���f���N���X</returns>
        private List<SetMenuList> EditSetMenuForReplace(List<SetMenuList> list) {

            // ���͕\�����ɂ��\�[�g
            list.Sort(delegate(SetMenuList x, SetMenuList y) { return (x.InputDetailsNumber ?? 0).CompareTo((y.InputDetailsNumber ?? 0)); });

            // ���͂̂������S���׍s�̒ǉ�
            int detailsNumber = 0;
            for (int i = 0; i < list.Count; i++) {

                SetMenuList setMenu = list[i];
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (((CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) && (!string.IsNullOrEmpty(setMenu.ServiceWorkCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) && (!string.IsNullOrEmpty(setMenu.ServiceMenuCode)))
                    || ((CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) && (!string.IsNullOrEmpty(setMenu.PartsNumber)))
                    || (setMenu.InputDetailsNumber != null)) {

                    list[i].DetailsNumber = ++detailsNumber;
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("001")) {
                        setMenu.PartsNumber = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("002")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.PartsNumber = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("003")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.Comment = null;
                    }
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(setMenu.ServiceType).Equals("004")) {
                        setMenu.ServiceWorkCode = null;
                        setMenu.ServiceMenuCode = null;
                        setMenu.PartsNumber = null;
                    }
                    setMenu.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.CreateDate = DateTime.Now;
                    setMenu.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    setMenu.LastUpdateDate = DateTime.Now;
                    setMenu.DelFlag = "0";
                }
            }

            return list;
        }
    }
}
