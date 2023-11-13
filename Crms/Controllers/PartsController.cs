using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Text.RegularExpressions;
using System.Data.Linq;
using System.Data.SqlClient;
using Microsoft.VisualBasic;
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using OfficeOpenXml;


namespace Crms.Controllers
{
    /// <summary>
    /// ���i�}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���i�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���i�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /*
        /// <summary>
        /// �_�u���R�[�e�[�V�����u������
        /// </summary>
        private readonly string QuoteReplace = "@@@@";

        /// <summary>
        /// �J���}�u������
        /// </summary>
        private readonly string CommaReplace = "?";
        */

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���i������ʕ\��
        /// </summary>
        /// <returns>���i�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���i������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i�������</returns>
        /// <history>
        /// 2021/02/22 yano #4083 �y���i�}�X�^�����z���������̃p�t�H�[�}���X���P�Ή�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<PartsCriteria> list;      //Mod 2021/02/22 yano #4083 
            //PaginatedList<Parts> list;
            if (criteriaInit)
            {
                list = new PaginatedList<PartsCriteria>();  //Mod 2021/02/22 yano #4083 
            }
            else
            {
                list = GetSearchResultList(form);
            }

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];
            ViewData["DelFlag"] = form["DelFlag"];

            // ���i������ʂ̕\��
            return View("PartsCriteria", list);
        }

        /// <summary>
        /// ���i�����_�C�A���O�\��
        /// </summary>
        /// <returns>���i�����_�C�A���O</returns>
        /// <history>
        ///  2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �����敪�̈����ǉ�
        /// </history>
        public ActionResult CriteriaDialog(string GenuineType = null)
        {
            criteriaInit = true;

            FormCollection form = new FormCollection();

            form["GenuineType"] = (GenuineType ?? "");

            return CriteriaDialog(form);
        }

        /// <summary>
        /// ���i�����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i������ʃ_�C�A���O</returns>
        /// <history>
        ///  2021/02/22 yano #4083 �y���i�}�X�^�����z���������̃p�t�H�[�}���X���P�Ή�
        ///  2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �����敪�̌��������ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["MakerCode"] = Request["MakerCode"];
            form["MakerName"] = Request["MakerName"];
            form["PartsNumber"] = Request["PartsNumber"];
            form["PartsNameJp"] = Request["PartsNameJp"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            //Mod 2021/02/22 yano #4083
            PaginatedList<PartsCriteria> list;
            //PaginatedList<Parts> list;
            if (criteriaInit)
            {
                if ((string.IsNullOrEmpty(form["MakerCode"]))
                    && (string.IsNullOrEmpty(form["MakerName"]))
                    && (string.IsNullOrEmpty(form["PartsNumber"]))
                    && (string.IsNullOrEmpty(form["PartsNameJp"])))
                {
                    list = new PaginatedList<PartsCriteria>();  //Mod 2021/02/22 yano #4083
                }
                else
                {
                    list = GetSearchResultList(form);
                }
            }
            else
            {
                list = GetSearchResultList(form);
            }

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["MakerCode"] = form["MakerCode"];
            ViewData["MakerName"] = form["MakerName"];
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["PartsNameJp"] = form["PartsNameJp"];

            //�����敪
            ViewData["GenuineType"] = form["GenuineType"];

            // ���i������ʂ̕\��
            return View("PartsCriteriaDialog", list);
        }

        /// <summary>
        /// ���i�}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���i�R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���i�}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Parts parts;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                parts = new Parts();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                parts = new PartsDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(parts);

            // �o��
            return View("PartsEntry", parts);
        }

        /// <summary>
        /// ���i�}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="parts">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���i�}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Parts parts, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateParts(parts, "");
            if (!ModelState.IsValid)
            {
                GetEntryViewData(parts);
                return View("PartsEntry", parts);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                Parts targetParts = new PartsDao(db).GetByKey(parts.PartsNumber, true);
                UpdateModel(targetParts);
                EditPartsForUpdate(targetParts);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                parts = EditPartsForInsert(parts);

                // �f�[�^�ǉ�
                db.Parts.InsertOnSubmit(parts);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
                    break;
                }
                catch (ChangeConflictException ce)
                {
                    // �X�V���A�N���C�A���g�̓ǂݎ��ȍ~��DB�l���X�V���ꂽ���A���[�J���̒l��DB�l�ŏ㏑������
                    foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                    {
                        occ.Resolve(RefreshMode.KeepCurrentValues);
                    }
                    // ���g���C�񐔂𒴂����ꍇ�A�G���[�Ƃ���
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
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ��Ӑ���G���[�̏ꍇ�A���b�Z�[�W��ݒ肵�A�Ԃ�
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "���i�ԍ�", "�ۑ�" }));
                        GetEntryViewData(parts);
                        return View("PartsEntry", parts);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        return View("Error");
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
                    return View("Error");
                }
            }
            //MOD 2014/10/24 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(parts.PartsNumber);
        }

        /// <summary>
        /// ���i�R�[�h���畔�i�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���i�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// </history>
        public ActionResult GetMaster(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null)
                {
                    codeData.Code = parts.PartsNumber;
                    codeData.Name = parts.PartsNameJp;

                    codeData.Code2 = parts.GenuineType;     //2018/01/15 arc yano #3833
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ���i�R�[�h���畔�i�}�X�^���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���i�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή�
        /// </history>
        public ActionResult GetMasterDetail(string code)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                Parts parts = new PartsDao(db).GetByKey(code);
                if (parts != null)
                {
                    ret.Add("PartsNameJp", parts.PartsNameJp);
                    ret.Add("PartsNameEn", parts.PartsNameEn);
                    ret.Add("PartsNumber", parts.PartsNumber);
                    ret.Add("Cost", parts.Cost.ToString());
                    ret.Add("Price", parts.Price.ToString());
                    ret.Add("MakerCode", parts.MakerCode);          //Add 2018/06/01 arc yano #3894
                    ret.Add("GenuineType", parts.GenuineType);      //Add 2018/06/01 arc yano #3894
                }
                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="parts">���f���f�[�^</param>
        /// <history>
        /// 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX�@�ړ����ϒP���̎Q�Ɛ��PartsAverageCost��PartsMovingAverageCost�ɕύX
        /// 2016/01/21 arc yano #3403_���i�}�X�^���́@�����敪�A���[�J�[���i�ԍ��̕K�{���ډ� (#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�) 
        ///                     �����敪�h���b�v�_�E���̃u�����N��I���ł��Ȃ��悤�ɂ���                
        /// </history>
        private void GetEntryViewData(Parts parts)
        {
            // ���[�J�[���̎擾
            if (!string.IsNullOrEmpty(parts.MakerCode))
            {
                MakerDao makerDao = new MakerDao(db);
                Maker maker = makerDao.GetByKey(parts.MakerCode);
                if (maker != null)
                {
                    ViewData["MakerName"] = maker.MakerName;
                }
            }

            //Mod 2018/05/14 arc yano #3880
            PartsMovingAverageCost condition = new PartsMovingAverageCost();
            condition.PartsNumber = parts.PartsNumber;
            condition.CompanyCode = ((Employee)Session["Employee"]).Department1.Office.CompanyCode;

            PartsMovingAverageCost PartsAvgCost = new PartsMovingAverageCostDao(db).GetByKey(condition);

            if (PartsAvgCost == null)
            {
                PartsAvgCost = new PartsMovingAverageCost();
                PartsAvgCost.Price = null;
            }

            /*
            // Add 2014/12/24 arc nakayama �ړ����ϒP���ȍ~
            //�ړ����ϒP���e�[�u��(PartsAverageCost)����A�ړ����ϒP��(Price)���擾
            PartsAverageCost PartsAvgCost = new PartsAverageCostDao(db).GetByKeyPartsNumber(parts);
            if (PartsAvgCost == null)
            {
                PartsAvgCost = new PartsAverageCost();
                PartsAvgCost.Price = null;
            }
            */

            ViewData["MoveAverageUnitPrice"] = PartsAvgCost.Price;
            // �Z���N�g���X�g�̎擾
            CodeDao dao = new CodeDao(db);
            ViewData["GenuineTypeList"] = CodeUtils.GetSelectListByModel(dao.GetGenuineTypeAll(false), parts.GenuineType, false);   //Mod 2016/01/21 arc yano
            // Mod 2014/11/06 arc nakayama ���i���ڒǉ��Ή�
            ViewData["UnitCD1List"] = CodeUtils.GetSelectListByModel(new CodeDao(db).GetCodeName("010", false), parts.UnitCD1, true);
        }

        /// <summary>
        /// ���i�}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i�}�X�^�������ʃ��X�g</returns>
        /// <history>
        ///  2021/02/22 yano #4083 �y���i�}�X�^�����z���������̃p�t�H�[�}���X���P�Ή�
        ///  2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �����敪�̌��������ǉ�
        /// </history>
        private PaginatedList<PartsCriteria> GetSearchResultList(FormCollection form)
        {
            PartsDao partsDao = new PartsDao(db);
            Parts partsCondition = new Parts();
            partsCondition.PartsNumber = form["PartsNumber"];
            partsCondition.PartsNameJp = form["PartsNameJp"];
            partsCondition.Maker = new Maker();
            partsCondition.Maker.MakerCode = form["MakerCode"];
            partsCondition.Maker.MakerName = form["MakerName"];
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                partsCondition.DelFlag = form["DelFlag"];
            }

            //�����敪
            partsCondition.GenuineType = form["GenuineType"];

            return partsDao.GetListByCondition(partsCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="parts">���i�f�[�^</param>
        /// <returns>���i�f�[�^</returns>
        /// <history>
        /// Add 2016/01/21 arc yano #3403_���i�}�X�^���́@�����敪�A���[�J�[���i�ԍ��̕K�{���ډ�(#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�)
        ///                                               ���[�J�[���i�ԍ��̕K�{�`�F�b�N��ǉ�
        /// </history>
        private Parts ValidateParts(Parts parts, String lineMsg)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(parts.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", lineMsg + "���i�ԍ�"));
            }
            if (string.IsNullOrEmpty(parts.PartsNameJp))
            {
                ModelState.AddModelError("PartsNameJp", MessageUtils.GetMessage("E0001", lineMsg + "���i��(���{��)"));
            }
            if (string.IsNullOrEmpty(parts.MakerCode))
            {
                ModelState.AddModelError("MakerCode", MessageUtils.GetMessage("E0001", lineMsg + "���[�J�[�R�[�h"));
            }

            //Add 2016/01/21 arc yano
            if (string.IsNullOrEmpty(parts.MakerPartsNumber))
            {
                ModelState.AddModelError("MakerPartsNumber", MessageUtils.GetMessage("E0001", lineMsg + "���[�J�[���i�ԍ�"));
            }

            // �����`�F�b�N
            if (!ModelState.IsValidField("Price"))
            {
                ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�艿", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SalesPrice"))
            {
                ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�̔����i", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("SoPrice"))
            {
                ModelState.AddModelError("SoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "S/O���i", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("Cost"))
            {
                ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "����", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("ClaimPrice"))
            {
                ModelState.AddModelError("ClaimPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�N���[���\�����i��", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("MpPrice"))
            {
                ModelState.AddModelError("MpPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "MP���i", "���̐����̂�" }));
            }
            if (!ModelState.IsValidField("EoPrice"))
            {
                ModelState.AddModelError("EoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "E/O���i", "���̐����̂�" }));
            }
            // Add 2014/11/07 arc nakayama ���i���ڒǉ��Ή�
            if (!ModelState.IsValidField("QuantityPerUnit1"))
            {
                ModelState.AddModelError("QuantityPerUnit1", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "����", "���̐����̂�" }));
            }

            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("PartsNumber") && !CommonUtils.IsAlphaNumeric(parts.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0012", lineMsg + "���i�ԍ�"));
            }
            if (ModelState.IsValidField("Price") && parts.Price != null)
            {
                if (!Regex.IsMatch(parts.Price.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Price", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�艿", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SalesPrice") && parts.SalesPrice != null)
            {
                if (!Regex.IsMatch(parts.SalesPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SalesPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�̔����i", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("SoPrice") && parts.SoPrice != null)
            {
                if (!Regex.IsMatch(parts.SoPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("SoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "S/O���i", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("Cost") && parts.Cost != null)
            {
                if (!Regex.IsMatch(parts.Cost.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("Cost", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "����", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("ClaimPrice") && parts.ClaimPrice != null)
            {
                if (!Regex.IsMatch(parts.ClaimPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("ClaimPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "�N���[���\�����i��", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("MpPrice") && parts.MpPrice != null)
            {
                if (!Regex.IsMatch(parts.MpPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("MpPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "MP���i", "���̐����̂�" }));
                }
            }
            if (ModelState.IsValidField("EoPrice") && parts.EoPrice != null)
            {
                if (!Regex.IsMatch(parts.EoPrice.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("EoPrice", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "E/O���i", "���̐����̂�" }));
                }
            }
            // Add 2014/11/07 arc nakayama ���i���ڒǉ��Ή�
            if (ModelState.IsValidField("QuantityPerUnit1") && parts.QuantityPerUnit1 != null)
            {
                if (!Regex.IsMatch(parts.QuantityPerUnit1.ToString(), @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("QuantityPerUnit1", MessageUtils.GetMessage("E0004", new string[] { lineMsg + "����", "���̐����̂�" }));
                }
            }

            return parts;
        }

        /// <summary>
        /// ���i�}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="parts">���i�f�[�^(�o�^���e)</param>
        /// <returns>���i�}�X�^���f���N���X</returns>
        /// <history>
        /// 2016/06/03 arc yano #3570 ���i�}�X�^�ҏW��ʁ@�݌ɒI���ΏہE��Ώېݒ�̍��ڒǉ�
        /// </history>
        private Parts EditPartsForInsert(Parts parts)
        {
            parts.PartsNumber = Strings.StrConv(parts.PartsNumber, VbStrConv.Narrow, 0);
            parts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.CreateDate = DateTime.Now;
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.LastUpdateDate = DateTime.Now;
            parts.DelFlag = "0";

            //Add 2016/06/03 arc yano #3570
            if (parts.NonInventoryFlag.Equals("true"))
            {
                parts.NonInventoryFlag = "0";
            }
            else
            {
                parts.NonInventoryFlag = "1";
            }
            return parts;
        }

        /// <summary>
        /// ���i�}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="parts">���i�f�[�^(�o�^���e)</param>
        /// <returns>���i�}�X�^���f���N���X</returns>
        /// <history>
        /// 2016/06/03 arc yano #3570 ���i�}�X�^�ҏW��ʁ@�݌ɒI���ΏہE��Ώېݒ�̍��ڒǉ�
        /// </history>
        private Parts EditPartsForUpdate(Parts parts)
        {
            parts.PartsNumber = Strings.StrConv(parts.PartsNumber, VbStrConv.Narrow, 0);
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            parts.LastUpdateDate = DateTime.Now;

            //Add 2016/06/03 arc yano #3570
            if (parts.NonInventoryFlag.Equals("true"))
            {
                parts.NonInventoryFlag = "0";
            }
            else
            {
                parts.NonInventoryFlag = "1";
            }
            return parts;
        }

        /*
        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// ���i���i�ꊇ�X�V�_�C�A���O�\��
        /// </summary>
        /// <returns>���i���i�ꊇ�X�V�_�C�A���O</returns>
        public ActionResult ImportDialog()
        {
            return View("PartsImportDialog");
        }

        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        ///  ���i���i�ꊇ�X�V����
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            string readText = "";
            ArrayList array = new ArrayList();
            string partsErrMsg = "";
            string line = "";
            string[] splitLine = null;
            string[] csvReadLine = null;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            ModelState.Clear();

            // �t�@�C���̑��݃`�F�b�N
            ValidateImportFile(importFile);
            if (!ModelState.IsValid)
            {
                return View("PartsImportDialog");
            }

            //�X�g�b�v�E�H�b�`���J�n����
            sw.Start();

            byte[] data = new Byte[importFile.ContentLength];
            // �w��t�@�C�����̃f�[�^���o�C�g�z��Ŏ擾
            importFile.InputStream.Read(data, 0, importFile.ContentLength);

            // �o�C�g�z��𕶎���(Shift-JIS)�ϊ�
            readText = System.Text.Encoding.GetEncoding(932).GetString(data);
            readText = readText.Replace(Environment.NewLine, "\r");
            readText = readText.Trim('\r');
            string[] readLine = readText.Split('\r');

            // arrayList�ɕϊ�����
            array.AddRange(readLine);

            // ���o���s�폜
            array.RemoveAt(0);

            // �d�����镔�i�ԍ��̔r��
            DeleteOverLapData(array);

            if (array.Count == 0)
            {
                //�X�g�b�v�E�H�b�`���~�߂�
                sw.Stop();
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "�X�V�f�[�^��0���ł��B�X�V�������I�����܂���"));
                ViewData["Message"] = "CSV�t�@�C���f�[�^0��";
                return View("PartsImportDialog", ViewData);
            }

            //db.Log = new OutputWriter();

            try
            {
                // �Ǎ��񂾍s�������[�v
                for (int count = 0; count < array.Count; count++)
                {
                    {
                        line = array[count].ToString();
                        // �_�u���R�[�e�[�V�����ƃJ���}�̌����ƒu�������f�[�^���擾
                        line = ReplaceQuoteCommaData(array[count].ToString());

                        splitLine = line.Split(',');
                        csvReadLine = new string[splitLine.Count()];
                        csvReadLine = EditCsvQuoteData(splitLine);

                        // �g�p���鍀�ڂ̑����`�F�b�N
                        ValidateCsvDataProperties(csvReadLine, count);
                        if (!ModelState.IsValid)
                        {
                            //�X�g�b�v�E�H�b�`���~�߂�
                            sw.Stop();
                            ViewData["Message"] = "CSV�t�@�C���f�[�^�s��";
                            return View("PartsImportDialog", ViewData);
                        }

                        // ���i�ԍ��̑��݃`�F�b�N
                        Parts parts = new PartsDao(db).GetByKey("AR" + csvReadLine[0]);

                        if (parts != null)
                        {
                            // ���݂����ꍇ�A�X�V�������s��
                            EditPartsData(parts, csvReadLine, true);

                            partsErrMsg = "���i�ԍ� = " + csvReadLine[0] + "��";

                            // �o�^�O��Validation���s��
                            ValidateParts(parts, partsErrMsg);
                            if (!ModelState.IsValid)
                            {
                                //�X�g�b�v�E�H�b�`���~�߂�
                                sw.Stop();
                                ViewData["Message"] = "�o�^�O�`�F�b�N�G���[";
                                return View("PartsImportDialog", ViewData);
                            }

                            db.Parts.DeleteOnSubmit(parts);
                        }
                        else
                        {
                            parts = new Parts();
                            // ���݂��Ȃ������ꍇ�A�o�^�������s��
                            EditPartsData(parts, csvReadLine, false);

                            partsErrMsg = "���i�ԍ� = " + csvReadLine[0] + "��";

                            // �o�^�O��Validation���s��
                            ValidateParts(parts, partsErrMsg);
                            if (!ModelState.IsValid)
                            {
                                //�X�g�b�v�E�H�b�`���~�߂�
                                sw.Stop();
                                ViewData["Message"] = "�o�^�O�`�F�b�N�G���[";
                                return View("PartsImportDialog", ViewData);
                            }
                        }

                        db.Parts.InsertOnSubmit(parts);
                    }
                }
                db.SubmitChanges();
            }
            catch (SqlException se)
            {
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ��Ӑ���G���[�̏ꍇ�A���b�Z�[�W��ݒ肵�A�Ԃ�
                if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                {
                    //�X�g�b�v�E�H�b�`���~�߂�
                    sw.Stop();
                    OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");
                    ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "���i�ԍ�", "�ۑ�" }));
                    ViewData["Message"] = "���i�ԍ��F" + csvReadLine[0] + "�ɂăG���[";
                    return View("PartsImportDialog", ViewData);
                }
                else
                {
                    //�X�g�b�v�E�H�b�`���~�߂�
                    sw.Stop();
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                    return View("Error");
                }
            }
            catch (Exception e)
            {
                //�X�g�b�v�E�H�b�`���~�߂�
                sw.Stop();
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ���O�ɏo��
                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                // �G���[�y�[�W�ɑJ��
                return View("Error");
            }
            finally
            {
                //�X�g�b�v�E�H�b�`���~�߂�
                sw.Stop();
            }


            ViewData["Message"] = "�X�V�������܂����B�@�o�ߎ��ԁF" + String.Format("{0:00}:{1:00}:{2:00}", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds);
            return View("PartsImportDialog", ViewData);

        }
        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// �捞�t�@�C�����݃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Boolean ValidateImportFile(HttpPostedFileBase filePath)
        {
            // �K�{�`�F�b�N
            if (filePath == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "�t�@�C����I�����Ă�������"));
            }
            else
            {
                // �g���q�`�F�b�N
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("csv") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "�t�@�C���̊g���q��csv�t�@�C���ł͂���܂���"));
                }
            }

            return true;
        }

        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// �Ǎ���CSV�f�[�^�̍��ڃ`�F�b�N
        /// </summary>
        /// <param name="impData">CSV�f�[�^(Split��)</param>
        /// <param name="count">���R�[�h�J�E���g</param>
        private void ValidateCsvDataProperties(String[] impData, int count)
        {
            string partsNumberMsg = "";

            partsNumberMsg = "���i�ԍ� = " + impData[0] + "��";

            //�K�{�`�F�b�N
            
            //���i�ԍ�
            if (string.IsNullOrEmpty(impData[0]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0024", "���i�ԍ������͂���Ă��Ȃ�CSV�f�[�^�����݂��܂��B���i�ԍ��͓��͕K�{�ł�"));
                return;
            }

            //����
            if (string.IsNullOrEmpty(impData[1]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0001", partsNumberMsg + "���i��"));
            }

            // D.C
            if (string.IsNullOrEmpty(impData[4]))
            {
                ModelState.AddModelError("ImportFile", MessageUtils.GetMessage("E0001", partsNumberMsg + "DC"));
            }

            //��]�������i�@�i�Ŕ����j
            if (!Regex.IsMatch(impData[5], @"^\d{1,10}$"))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0004", new string[] { partsNumberMsg + "��]�������i�@�i�Ŕ����j", "����10���ȓ��̐����̂�" }));
            }
        }
          
        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// �Ǎ���csv�f�[�^��ҏW����
        /// </summary>
        /// <param name="parts">parts</param>
        /// <param name="impData">csv�f�[�^</param>
        /// <param name="updateflag">true:�X�V false:�V�K�ǉ�</param>
        /// <returns></returns>
        private Parts EditPartsData(Parts parts,String[] impData, Boolean updateflag)
        {
            string dicountCode = impData[4]; // D.C
            Decimal soCost = Decimal.Zero;   // S/O����
            Decimal eoCost = Decimal.Zero;   // E/O����
            Decimal claimPrice = Decimal.Zero; // �ڰѐ\�����i��
            Decimal mpPrice = Decimal.Zero;     // MP���i
            Decimal rate = Decimal.Zero;        // �e���[�g�v�Z�p
            
            // D.C���L�[�ɕ��i���������擾
            PartsDiscountRate pdRate = new PartsDiscountRateDao(db).GetByKey(dicountCode);
            if (pdRate != null)
            {
                
                // S/O�����v�Z
                if (pdRate.SoRate > 0) {
                    rate = Decimal.Divide(pdRate.SoRate, 100m);
                }
                soCost = Decimal.Parse(impData[5]) * (1 - rate);
                soCost = Math.Floor(Decimal.Add(soCost, 0.4m));
                parts.SoPrice = soCost;

                
                // E/O����
                if (pdRate.EoRate > 0) {
                    rate = Decimal.Divide(pdRate.EoRate, 100m);
                }
                eoCost = Decimal.Parse(impData[5]) * (1 - rate);
                eoCost = Math.Floor(Decimal.Add(eoCost, 0.4m));
                parts.EoPrice = eoCost;

                // �ڰѐ\�����i��
                if (pdRate.Warranty > 0)
                {
                    rate = Decimal.Divide(pdRate.Warranty, 100m);
                }
                claimPrice = Math.Floor((1 - rate) * Decimal.Parse(impData[5]) * 1.05m);
                parts.ClaimPrice = claimPrice;

                // MP���i
                mpPrice = Decimal.Parse(impData[5]) * 0.45m;
                mpPrice = Math.Floor(Decimal.Add(mpPrice, 0.4m));
                parts.MpPrice = mpPrice;
            }

            // �V�K�ǉ����݈̂ȉ����s��
            if (updateflag == false)
            {
                // ���i�ԍ�
                parts.PartsNumber = "AR" + impData[0];

                // ���i����(���{��)
                parts.PartsNameJp = impData[1];

                // ���i����(�p��)
                parts.PartsNameEn = impData[1];

                // ���[�J�[�R�[�h
                parts.MakerCode = "AR";

                // ���[�J�[���i�ԍ�
                parts.MakerPartsNumber = impData[0];

                // ���[�J�[���i���́iJ�j
                parts.MakerPartsNameJp = impData[1];

                // ���[�J�[���i���́iE�j
                parts.MakerPartsNameEn = impData[1];
                
                // �����敪
                parts.GenuineType = "001";

                // ���l�i�����i�ԍ��Ȃǁj
                parts.Memo = "";
                
                // �쐬��
                parts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                // �쐬����
                parts.CreateDate = DateTime.Now;

                // �폜�t���O
                parts.DelFlag = "0";
            }

            // �艿
            parts.Price = Decimal.Parse(impData[5]);

            // �̔����i
            parts.SalesPrice = Decimal.Parse(impData[5]);

            // ����
            parts.Cost = soCost;

            // �ŏI�X�V��
            parts.LastUpdateDate = DateTime.Now;

            // �ŏI�X�V����
            parts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            return parts;
        }

        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// �_�u���R�[�e�[�V�����̔r������
        /// </summary>
        /// <param name="quoteData"></param>
        /// <returns></returns>
        private string[] EditCsvQuoteData(string[] quoteData)
        {
            string[] splLine2 = new string[quoteData.Count()];
            ArrayList array2 = new ArrayList();
            string splData = "";

            // ArrayList�Ɋi�[
            array2.Clear();
            array2.AddRange(quoteData);

            // �_�u���R�[�e�[�V�����̕����������
            for (int i = 0; i < array2.Count; i++)
            {

                splData = array2[i].ToString();
                splData = splData.Replace("\"", "");
                splData = splData.Replace(QuoteReplace, "\"");
                splData = splData.Replace(CommaReplace, ",");
                splLine2[i] = splData;
            }

            return splLine2;
        }

        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// �d���������i�ԍ����R�[�h�̔r������
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private ArrayList DeleteOverLapData(ArrayList list)
        {
            ArrayList list2 = new ArrayList(list);
            ArrayList compList = new ArrayList();
            int index = 0;

            list.Clear();

            for (int cnt = 0; cnt < list2.Count; cnt++)
            {
                string strLine = list2[cnt].ToString();
                string[] strList = strLine.Split(',');

                // �����i�ԍ������݂��邩�`�F�b�N
                index = compList.IndexOf(strList[0]);

                if (index >= 0)
                {
                    // �����i�ԍ������݂����ꍇ�A�㏑��
                    compList[index] = strList[0];
                    list[index] = strLine;
                }
                else
                {
                    // �����i�ԍ������݂��Ȃ������ꍇ�A�V�K�ɒǉ�
                    compList.Add(strList[0]);
                    list.Add(strLine);
                }
            }

            return list;

        }

        // Add 2014/09/16 arc amii ���i���i�ꊇ�X�V�Ή� �V�K�ǉ�
        /// <summary>
        /// ���ړ��̃_�u���R�[�e�[�V������ʕ����ɒu�����鏈��
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string ReplaceQuoteCommaData(string line)
        {
            int Quote = 0;
            int start = 0;
            int end = 0;
            string before = "";
            string after = "";
            int strLen = 0;

            // ���ړ��̃_�u���R�[�e�[�V������ʕ����ɕϊ�
            line = line.Replace("\"\"", QuoteReplace);

            for (int j = 0; j < line.Length - 1; j++)
            {
                // �_�u���R�[�e�[�V�����̌���
                if (line.IndexOf("\"", j) >= 0)
                {
                    Quote++;
                }
                else
                {
                    // �q�b�g���Ȃ������ꍇ�A���[�v�I�����A���̓Ǎ��f�[�^��
                    j = line.Length;
                    continue;
                }

                // �_�u���R�[�e�[�V�����̐��ɂ���ĕ���
                if (Quote == 1)
                {
                    // 1�ڂ̏ꍇ

                    //�q�b�g�����ʒu���o���Ă���(�J�n�ʒu)
                    start = line.IndexOf("\"", j);

                    // �����ʒu���q�b�g�����ʒu�ȍ~�ɐݒ�
                    j = start;
                }
                else if (Quote == 2)
                {
                    // 2�ڂ����������ꍇ
                    //�q�b�g�����ʒu���o���Ă���(�I���ʒu)
                    end = line.IndexOf("\"", j);

                    // �����ʒu���q�b�g�����ʒu�ȍ~�ɐݒ�
                    j = end;

                    // �I���ʒu - �J�n�ʒu�ŕ��������擾
                    strLen = end - start;

                    // ������؂�o��
                    before = line.Substring(start, strLen + 1);

                    // �؂�o������������̃J���}��ϊ�
                    after = before.Replace(",", CommaReplace);

                    // �ϊ��O�̕������ϊ���̕�����Œu������
                    line = line.Replace(before, after);
                    Quote = 0;
                }
            }

            return line;
        }
        */

        #region Excel�捞����
        /// <summary>
        /// Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<PartsExcelImportList> ImportList = new List<PartsExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("PartsImportDialog", ImportList);
        }


        /// <summary>
        /// Excel�ǂݍ���
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<PartsExcelImportList> ImportList = new List<PartsExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel�ǂݍ���
                //--------------
                case "1":
                    //Excel�ǂݍ��ݑO�̃`�F�b�N
                    ValidateExcelFile(importFile, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog", ImportList);
                    }

                    //Excel�ǂݍ���
                    ImportList = ReadExcelData(importFile, ImportList);

                    //�ǂݍ��ݎ��ɉ����G���[������΂����Ń��^�[��
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog");
                    }

                    //Excel�œǂݍ��񂾃f�[�^�̃o���f�[�g�`�F�b�N
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsImportDialog");
                    }

                    //DB�o�^
                    DBExecute(ImportList, form);
                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("PartsImportDialog");

                //--------------
                //�L�����Z��
                //--------------
                case "2":

                    ImportList = new List<PartsExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[��荞��]�{�^���������Ȃ��悤�ɂ��邽��

                    return View("PartsImportDialog", ImportList);

                //----------------------------------
                //���̑�(�����ɓ��B���邱�Ƃ͂Ȃ�)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("PartsImportDialog");
            }
        }
        #endregion

        #region Excel�f�[�^�擾&�ݒ�
        /// Excel�f�[�^�擾&�ݒ�
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        private List<PartsExcelImportList> ReadExcelData(HttpPostedFileBase importFile, List<PartsExcelImportList> ImportList)
        {
            using (var pck = new ExcelPackage())
            {
                try
                {
                    pck.Load(importFile.InputStream);
                }
                catch (System.IO.IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process."))
                    {
                        ModelState.AddModelError("importFile", "�Ώۂ̃t�@�C�����J����Ă��܂��B�t�@�C������Ă���A�ēx���s���ĉ�����");
                        return ImportList;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "�G���[���������܂����B" + ex.Message);
                    return ImportList;
                }

                //-----------------------------
                // �f�[�^�V�[�g�擾
                //-----------------------------
                var ws = pck.Workbook.Worksheets[1];

                //--------------------------------------
                //�ǂݍ��ރV�[�g�����݂��Ȃ������ꍇ
                //--------------------------------------
                if (ws == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excel�Ƀf�[�^������܂���B�V�[�g�����m�F���čēx���s���ĉ�����"));
                    return ImportList;
                }

                //------------------------------
                //�ǂݍ��ݍs��0���̏ꍇ
                //------------------------------
                if (ws.Dimension == null)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excel�Ƀf�[�^������܂���B�X�V�������I�����܂���"));
                    return ImportList;
                }

                //�ǂݎ��̊J�n�ʒu�ƏI���ʒu���擾
                int StartRow = ws.Dimension.Start.Row;�@       //�s�̊J�n�ʒu
                int EndRow = ws.Dimension.End.Row;             //�s�̏I���ʒu
                int StartCol = ws.Dimension.Start.Column;      //��̊J�n�ʒu
                int EndCol = ws.Dimension.End.Column;          //��̏I���ʒu

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];

                //�^�C�g���s�A�w�b�_�s�����������ꍇ�͑����^�[������
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // �ǂݎ�菈��
                //------------------------------
                int datarow = 0;
                string[] Result = new string[ws.Dimension.End.Column];

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    //�X�V�f�[�^�̎擾
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        Result[col - 1] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
                    }

                    //----------------------------------------
                    // �ǂݎ�茋�ʂ���ʂ̍��ڂɃZ�b�g����
                    //----------------------------------------
                    ImportList = SetProperty(ref Result, ref ImportList);
                }
            }
            return ImportList;

        }
        #endregion

        #region Excel�ǂݎ��O�̃`�F�b�N
        /// <summary>
        /// Excel�ǂݎ��O�̃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        private void ValidateExcelFile(HttpPostedFileBase filePath, FormCollection form)
        {
            // �K�{�`�F�b�N
            if (filePath == null || string.IsNullOrEmpty(filePath.FileName))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "�t�@�C����I�����Ă�������"));
            }
            else
            {
                // �g���q�`�F�b�N
                System.IO.FileInfo cFileInfo = new System.IO.FileInfo(filePath.FileName);
                string stExtension = cFileInfo.Extension;

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "�t�@�C���̊g���q��xlsx�t�@�C���ł͂���܂���"));
                }
            }

            if (string.IsNullOrWhiteSpace(form["Memo"]))
            {
                ModelState.AddModelError("Memo", "�R�����g�͓��͕K�{�ł�");
            }

            return;
        }
        #endregion

        #region �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// <summary>
        /// �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        public void ValidateImportList(List<PartsExcelImportList> ImportList)
        {
            //Parts condition = new Parts();
            //condition.DelFlag = "0";

            ////���i���X�g���擾����
            //List<Parts> partsList = new PartsDao(db).GetListByCondition(condition);

            //���[�J�[���X�g���擾����
            List<Maker> makerList = new MakerDao(db).GetMakerBykey();

            for (int i = 0; i < ImportList.Count; i++)
            {
                //----------------
                //���i�ԍ�
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].PartsNumber))  //�K�{�`�F�b�N
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̕��i�ԍ������͂���Ă��܂���B���i�ԍ�"));
                }
                else if (ImportList[i].PartsNumber.Length > 25)   //�f�[�^���`�F�b�N
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̕��i�ԍ�", "25" }));
                }
                else //�}�X�^�`�F�b�N
                {
                    //Parts rec = partsList.Where(x => Strings.StrConv(x.PartsNumber, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].PartsNumber)).FirstOrDefault();
                    Parts rec = new PartsDao(db).GetByKey(ImportList[i].PartsNumber);

                    //���i���X�g���猟�����Č�����Ȃ������ꍇ��
                    if (rec == null)
                    {
                        ImportList[i].NewPartsFlag = true;
                    }
                }

                //�V�K���i�̏ꍇ�̂ݕ��i�̃`�F�b�N���s��
                if (ImportList[i].NewPartsFlag)
                {
                    //----------------
                    //���i��
                    //----------------
                    //�K�{�`�F�b�N
                    if (string.IsNullOrEmpty(ImportList[i].PartsNameJp))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̕��i�������͂���Ă��܂���B���i��"));
                    }
                    else if (ImportList[i].PartsNameJp.Length > 50)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̕��i��", "50" }));
                    }
                    //--------------------
                    //���i��(�p��)
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].PartsNameEn) && ImportList[i].PartsNameEn.Length > 50)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̕��i��(�p��)", "50" }));
                    }
                    //-----------------
                    //���[�J�[�R�[�h
                    //-----------------
                    if (string.IsNullOrEmpty(ImportList[i].MakerCode)) //�K�{�`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̃��[�J�[�R�[�h�����͂���Ă��܂���B���[�J�[�R�[�h"));
                    }
                    else //�}�X�^�`�F�b�N
                    {
                        Maker maker = makerList.Where(x => Strings.StrConv(x.MakerCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(ImportList[i].MakerCode)).FirstOrDefault();

                        if (maker == null)
                        {
                            ModelState.AddModelError("", i + 1 + "�s�ڂ̃��[�J�[�R�[�h���}�X�^�ɓo�^����Ă��܂���");
                        }
                    }
                    //--------------------
                    //���[�J�[���i�ԍ�
                    //--------------------
                    //�K�{�`�F�b�N
                    if (string.IsNullOrEmpty(ImportList[i].MakerPartsNumber))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̃��[�J�[���i�ԍ����͂���Ă��܂���B���[�J�[���i�ԍ�"));
                    }
                    else if (ImportList[i].MakerPartsNumber.Length > 25)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̃��[�J�[���i�ԍ�", "25" }));
                    }
                    //--------------------
                    //���[�J�[���i��
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].MakerPartsNameJp) && ImportList[i].MakerPartsNameJp.Length > 50)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̃��[�J�[���i��", "50" }));
                    }
                    //-----------------------
                    //���[�J�[���i��(�p��)
                    //-----------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].MakerPartsNameEn) && ImportList[i].MakerPartsNameEn.Length > 50)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̃��[�J�[���i��(�p��)", "50" }));
                    }
                    //-------------------
                    //�����敪
                    //-------------------
                    if (string.IsNullOrEmpty(ImportList[i].GenuineType) || (!ImportList[i].GenuineType.Equals("001") && !ImportList[i].GenuineType.Equals("002")))
                    {
                        ModelState.AddModelError("", i + 1 + "�s�ڂ̏����敪�ɂ́u001(�����i)�v�܂��́u002(�ЊO�i)�v����͂��Ă�������");
                    }
                    //--------------------
                    //�P��
                    //--------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].UnitCD1) && ImportList[i].UnitCD1.Length > 3)   //�f�[�^���`�F�b�N
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0032", new string[] { i + 1 + "�s�ڂ̒P��", "3" }));
                    }
                    //---------------------
                    //�P�ʂ�����̐���
                    //---------------------
                    if (!string.IsNullOrWhiteSpace(ImportList[i].QuantityPerUnit1) && !Regex.IsMatch(ImportList[i].QuantityPerUnit1, @"^\d{1,10}$"))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ̒P�ʂ�����̐���", "���̐���10���ȓ�" }));
                    }
                }
                //--------------
                //���i
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].Price) && !Regex.IsMatch(ImportList[i].Price, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ̉��i", "���̐���10���ȓ�" }));
                }
                //--------------
                //�̔����i
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].SalesPrice) && !Regex.IsMatch(ImportList[i].SalesPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ̔̔����i", "���̐���10���ȓ�" }));
                }
                //--------------
                //S/O���i
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].SoPrice) && !Regex.IsMatch(ImportList[i].SoPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ�S/O���i", "���̐���10���ȓ�" }));
                }
                //--------------
                //����
                //--------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].Cost) && !Regex.IsMatch(ImportList[i].Cost, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ̌���", "���̐���10���ȓ�" }));
                }
                //---------------------
                //�N���[���\�����i��
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].ClaimPrice) && !Regex.IsMatch(ImportList[i].ClaimPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ̃N���[���\�����i��", "���̐���10���ȓ�" }));
                }
                //---------------------
                //MP���i
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].MpPrice) && !Regex.IsMatch(ImportList[i].MpPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ�MP���i", "���̐���10���ȓ�" }));
                }
                //---------------------
                //E/O���i
                //---------------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].EoPrice) && !Regex.IsMatch(ImportList[i].EoPrice, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s�ڂ�E/O���i", "���̐���10���ȓ�" }));
                }
                //---------------------
                //�I���ΏۊO�t���O
                //---------------------
                if (string.IsNullOrWhiteSpace(ImportList[i].NonInventoryFlag) || (!ImportList[i].NonInventoryFlag.Equals("0") && !ImportList[i].NonInventoryFlag.Equals("1")))
                {
                    ModelState.AddModelError("", i + 1 + "�s�ڂ̒I���ΏۊO�t���O�ɂ�0��1����͂��Ă�������");
                }
            }

            //-----------------
            //�d���`�F�b�N
            //-----------------
            var ret = ImportList.GroupBy(x => x.PartsNumber).Select(c => new { PartsNumber = c.Key, Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in ret)
            {
                if (!string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("", "�捞�ރt�@�C���̒��ɕ��i�ԍ�" + a.PartsNumber + "��������`����Ă��܂�");
                }
            }

        }
        #endregion

        #region Excel�̓ǂݎ�茋�ʂ����X�g�ɐݒ肷��
        /// <summary>
        /// ���ʂ����X�g�ɐݒ肷��
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        public List<PartsExcelImportList> SetProperty(ref string[] Result, ref List<PartsExcelImportList> ImportList)
        {
            PartsExcelImportList SetLine = new PartsExcelImportList();

            // ���i�ԍ��ԍ�
            SetLine.PartsNumber = Result[0];
            // ���i����(��)
            SetLine.PartsNameJp = Result[1];
            // ���i����(�p)
            SetLine.PartsNameEn = Result[2];
            // ���[�J�[�R�[�h
            SetLine.MakerCode = Result[3];
            // ���[�J�[���i�ԍ�
            SetLine.MakerPartsNumber = Result[4];
            // ���[�J�[���i��(��)
            SetLine.MakerPartsNameJp = Result[5];
            // ���[�J�[���i��(�p)
            SetLine.MakerPartsNameEn = Result[6];
            // ���i
            SetLine.Price = Result[7];
            // �̔����i
            SetLine.SalesPrice = Result[8];
            // So���i
            SetLine.SoPrice = Result[9];
            // ����
            SetLine.Cost = Result[10];
            // �N���[���\�����i��
            SetLine.ClaimPrice = Result[11];
            // Mo���i
            SetLine.MpPrice = Result[12];
            // Eo���i
            SetLine.EoPrice = Result[13];
            // �����敪
            SetLine.GenuineType = Result[14].PadLeft(3, '0');
            // �P��
            SetLine.UnitCD1 = Result[15];
            // �P�ʂ�����̐���
            SetLine.QuantityPerUnit1 = Result[16];
            //�I���ΏۊO�t���O
            SetLine.NonInventoryFlag = Result[17];

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        #region �_�C�A���O�̃f�[�^�t���R���|�[�l���g�ݒ�
        /// <summary>
        /// �_�C�A���O�̃f�[�^�t���R���|�[�l���g�ݒ�
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["Memo"] = form["Memo"];
        }
        #endregion

        #region �ǂݍ��񂾃f�[�^��DB�ɓo�^
        /// <summary>
        /// DB�X�V
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// </history>
        private void DBExecute(List<PartsExcelImportList> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                List<Parts> workList = new List<Parts>();

                //���i�}�X�^�̎擾
                //Parts condition = new Parts();

                //List<Parts> partsList = new PartsDao(db).GetListByCondition(condition);

                decimal ret = 0m; //string��decimal�ɕϊ����ꂽ�l

                //�����e�[�u�����X�V���āA���׃e�[�u����o�^����
                foreach (var LineData in ImportList)
                {
                    //�V�K�o�^��
                    if (LineData.NewPartsFlag)
                    {
                        //------------------------
                        //���i�}�X�^�o�^
                        //------------------------
                        Parts newParts = new Parts();
                        newParts.PartsNumber = LineData.PartsNumber;                  //���i�ԍ�
                        newParts.PartsNameJp = LineData.PartsNameJp;                  //���i��
                        newParts.PartsNameEn = LineData.PartsNameEn;                  //���i��(�p��)
                        newParts.MakerCode = LineData.MakerCode;                      //���[�J�[�R�[�h
                        newParts.MakerPartsNumber = LineData.MakerPartsNumber;        //���[�J�[���i�ԍ�
                        newParts.MakerPartsNameJp = LineData.MakerPartsNameJp;        //���[�J�[���i��
                        newParts.MakerPartsNameEn = LineData.MakerPartsNameEn;        //���[�J�[���i��(�p��)

                        //���i
                        if (Decimal.TryParse(LineData.Price, out ret))
                        {
                            newParts.Price = ret;
                        }
                        //�̔����i
                        if (Decimal.TryParse(LineData.SalesPrice, out ret))
                        {
                            newParts.SalesPrice = ret;
                        }
                        //S/O���i
                        if (Decimal.TryParse(LineData.SoPrice, out ret))
                        {
                            newParts.SoPrice = ret;
                        }
                        //����
                        if (Decimal.TryParse(LineData.Cost, out ret))
                        {
                            newParts.Cost = ret;
                        }
                        //�N���[���\�����i��
                        if (Decimal.TryParse(LineData.ClaimPrice, out ret))
                        {
                            newParts.ClaimPrice = ret;
                        }
                        //MP���i
                        if (Decimal.TryParse(LineData.MpPrice, out ret))
                        {
                            newParts.MpPrice = ret;
                        }
                        //E/O���i
                        if (Decimal.TryParse(LineData.EoPrice, out ret))
                        {
                            newParts.EoPrice = ret;
                        }

                        newParts.GenuineType = LineData.GenuineType;                                      //�����敪
                        newParts.Memo = form["Memo"];                                                     //����
                        newParts.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;       //�쐬��
                        newParts.CreateDate = DateTime.Now;                                               //�쐬����
                        newParts.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;   //�ŏI�X�V��
                        newParts.LastUpdateDate = DateTime.Now;                                           //�ŏI�X�V��
                        newParts.DelFlag = "0";                                                           //�폜�t���O
                        newParts.UnitCD1 = LineData.UnitCD1;

                        //�P�ʂ�����̐���
                        if (Decimal.TryParse(LineData.QuantityPerUnit1, out ret))
                        {
                            newParts.QuantityPerUnit1 = ret;
                        }

                        newParts.NonInventoryFlag = LineData.NonInventoryFlag;                            //�I���ΏۊO�t���O

                        workList.Add(newParts);
                    }
                    else
                    {
                        //�Ώۂ̕��i�}�X�^���擾
                        //Parts target = partsList.Where(x => x.PartsNumber.Equals(LineData.PartsNumber)).FirstOrDefault();
                        Parts target = new PartsDao(db).GetByKey(LineData.PartsNumber);


                        //���i�ԍ��ȊO���X�V
                        target.PartsNameJp = LineData.PartsNameJp;                  //���i��
                        target.PartsNameEn = LineData.PartsNameEn;                  //���i��(�p��)
                        target.MakerCode = LineData.MakerCode;                      //���[�J�[�R�[�h
                        target.MakerPartsNumber = LineData.MakerPartsNumber;        //���[�J�[���i�ԍ�
                        target.MakerPartsNameJp = LineData.MakerPartsNameJp;        //���[�J�[���i��
                        target.MakerPartsNameEn = LineData.MakerPartsNameEn;        //���[�J�[���i��(�p��)

                        //���i
                        if (Decimal.TryParse(LineData.Price, out ret))
                        {
                            target.Price = ret;
                        }
                        //�̔����i
                        if (Decimal.TryParse(LineData.SalesPrice, out ret))
                        {
                            target.SalesPrice = ret;
                        }
                        //S/O���i
                        if (Decimal.TryParse(LineData.SoPrice, out ret))
                        {
                            target.SoPrice = ret;
                        }
                        //����
                        if (Decimal.TryParse(LineData.Cost, out ret))
                        {
                            target.Cost = ret;
                        }
                        //�N���[���\�����i��
                        if (Decimal.TryParse(LineData.ClaimPrice, out ret))
                        {
                            target.ClaimPrice = ret;
                        }
                        //MP���i
                        if (Decimal.TryParse(LineData.MpPrice, out ret))
                        {
                            target.MpPrice = ret;
                        }
                        //E/O���i
                        if (Decimal.TryParse(LineData.EoPrice, out ret))
                        {
                            target.EoPrice = ret;
                        }

                        target.GenuineType = LineData.GenuineType;                                      //�����敪
                        target.Memo = form["Memo"];                                                     //����
                        target.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;   //�ŏI�X�V��
                        target.LastUpdateDate = DateTime.Now;                                           //�ŏI�X�V��
                        target.DelFlag = "0";                                                           //�폜�t���O
                        target.UnitCD1 = LineData.UnitCD1;

                        //�P�ʂ�����̐���
                        if (Decimal.TryParse(LineData.QuantityPerUnit1, out ret))
                        {
                            target.QuantityPerUnit1 = ret;
                        }

                        target.NonInventoryFlag = LineData.NonInventoryFlag;                            //�I���ΏۊO�t���O

                    }
                }

                db.Parts.InsertAllOnSubmit(workList);

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                    //��荞�݊����̃��b�Z�[�W��\������
                    ModelState.AddModelError("", "��荞�݂��������܂����B");
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion
    }
}