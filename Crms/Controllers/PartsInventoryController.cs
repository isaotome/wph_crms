using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.SqlClient;
using Crms.Models;
using System.Data.Linq;
using System.Transactions;
using System.Xml.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;
using Microsoft.VisualBasic;	//�S�p�^���p�ϊ��p


namespace Crms.Controllers {


    // Mod 2015/04/23 arc yano ���i�I���s��Ή� ���i�I����ʒǉ�(���݂̕��i�I����ʂ̃t�@�C��(���݂͖��g�p)���x�[�X�ɂ��č쐬)
    /// ���i�I���@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsInventoryController : Controller
    {

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsInventoryController() {
            db = new CrmsLinqDataContext();
            
            //Mod 2015/05/26 arc yano �^�C���A�E�g�l�̐ݒ肷��ꏊ���ړ�
            //�^�C���A�E�g�l�̐ݒ�
            //db.CommandTimeout = 300;
        }

        /// <summary>
        /// �_�u���R�[�e�[�V�����u������
        /// </summary>
        private readonly string QuoteReplace = "@@@@";

        /// <summary>
        /// �J���}�u������
        /// </summary>
        private readonly string CommaReplace = "?";

        private static readonly string STS_UNEXCUTED                = "000";            // �����{(�I���X�e�[�^�X)
        private static readonly string STS_INACTION                 = "001";            // ���{��(�I���X�e�[�^�X)
        private static readonly string STS_DECIDED                  = "002";            // �m��(�I���X�e�[�^�X)
        private static readonly string STS_INVALID                  = "999";            // �X�e�[�^�X�G���[(�I���X�e�[�^�X)   //Add 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��d�l�ύX�A
        private static readonly string TYP_PARTS                    = "002";            // ���i(�I�����)
        private static readonly string FORM_NAME                    = "���i�I��";       // ��ʖ�
        private static readonly string PROC_NAME_SEARCH             = "���i�I������";   // �������i���O�o�͗p�j
        private static readonly string PROC_NAME_INVSTART           = "�I���J�n";       // ������(�I���J�n)
        private static readonly string PROC_NAME_EXCELDOWNLOAD      = "Excel�o��";      // ������(Excel�o��)
        private static readonly string PROC_NAME_EXCELIMPORT        = "Excel�捞";      // ������(Excel�捞)
        private static readonly string PROC_NAME_TEMPSTORED         = "�ꎞ�ۑ�";       // ������(�ꎞ�ۑ�)
        private static readonly string PROC_NAME_INVENTORY_DECIDED  = "�I���m��";       // ������(�I���m��)
        private static readonly string PROC_NAME_DELLINE            = "�s�폜";         // ������(�s�폜)            //Add 201707/26 arc yano #3781
        private static readonly string PROC_NAME_DATAEDIT           = "�f�[�^�ҏW";     // ������(�f�[�^�ۑ�)        //Add 201707/26 arc yano #3781

        private static readonly int MAX_ERR_CNT = 100;                                  // �G���[���������(100��)
     
        /// <summary>
        /// ���i�I����ʏ����\��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j �󕥕\�X�e�[�^�X�擾�����̒ǉ�
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� ���i�I���Ώۃt���O�ɂ��i���ɕύX
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���i�݌ɒI���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂ֕ύX
        /// </history>
        public ActionResult Criteria() {

            //-----------------------
            //�����l�̐ݒ�
            //-----------------------
            criteriaInit = true;                            //�����\���t���OON�@
            FormCollection form = new FormCollection();     //�t�H�[������
            
            //�������
            form["RequestFlag"] = "";
            //�I���L���`�F�b�N
            form["DiffQuantity"] = "false";     //�S�ĕ\��

            //�\���y�[�W��
            form["id"] = "0";

            //�Ώ۔N���A���i�I����Ɠ�
            PartsInventoryWorkingDate WorkingDate = new PartsInventoryWorkingDateDao(db).GetAllVal();

            //��������
            EntitySet<PartsInventory> line = new EntitySet<PartsInventory>();

            //����(���O�C�����[�U�̕����ݒ肷��)
            //Mod 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A ���i�I���ΏۊO�̕���̃��O�C�����[�U�̏ꍇ�͕���R�[�h�͋󗓂Ƃ���
            //Department department = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode, includeDeleted : false, closeMonthFlag : "2");
            Department department = new DepartmentDao(db).GetByPartsInventory(((Employee)Session["Employee"]).DepartmentCode);
            if (department != null)
            {
                form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            }
            else
            {
                form["DepartmentCode"] = "";
            }

            //�q�ɂ̎擾
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                form["WarehouseCode"] = dWarehouse.WarehouseCode;
            }
            else
            {
                form["WarehouseCode"] = "";
            }

            // Add 2015/05/21 arc yano IPO�Ή�(���i�I��) �m��{�^���N���b�N�^�s��Ԓǉ�
            ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "PartsInventoryCommit");
            if (ret.EnableFlag == true)
            {
                form["InventoryEndButtonEnable"] = bool.TrueString;      //�I���m��{�^�������ł���
            }
            else
            {
                form["InventoryEndButtonEnable"] = bool.FalseString;      //�I���m��{�^�������ł��Ȃ�
            }


            if (WorkingDate != null)
            {
                form["PartsInventoryWorkingDate"] = string.Format("{0:yyyy/MM/dd}", WorkingDate.InventoryWorkingDate);       //�I����Ɠ�

                //�Ώ۔N����null�̏ꍇ�͕\�����Ȃ�
                if (WorkingDate.InventoryMonth != null)
                {
                    form["InventoryMonth"] = string.Format("{0:yyyy/MM}", WorkingDate.InventoryMonth);                           //�Ώ۔N��
                }
                else
                {
                    form["InventoryMonth"] = "";
                    //Mod 2015/06/09 arc nakayama IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�C �I����Ɠ��˒I������@�ɕύX
                    ModelState.AddModelError("", "�I��������o�^����Ă��܂���");
                    SetComponent(form);
                    return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
                }
            }
            else
            {
                form["InventoryMonth"] = "";
                form["PartsInventoryWorkingDate"] = "";
                //Mod 2015/06/09 arc nakayama IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�C �I����Ɠ��˒I������@�ɕύX
                ModelState.AddModelError("", "�I��������o�^����Ă��܂���");
                SetComponent(form);
                return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }

            //Mod 2015/06/17 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�E �I����Ɠ��ł͂Ȃ��A�I���J�n������\������悤�ɕύX
            //----------------
            //�I���J�n����
            //----------------
            InventoryScheduleParts condition = new InventoryScheduleParts();
            condition.InventoryMonth = (DateTime)WorkingDate.InventoryMonth;
            //Mod 2016/08/13 arc yano #3596
            //condition.Department = new Department();
            //condition.Department.DepartmentCode = form["DepartmentCode"];
            condition.DepartmentCode = form["DepartmentCode"];
            condition.WarehouseCode  = form["WarehouseCode"];       // ADD 2016/08/13 arc yano #3596

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);

            if (rec != null)
            {
                form["PartsInventoryStartDate"] = rec.StartDate == null ? "": string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
            }

            //Add 2017/07/26 arc yano #3781
            //�󕥕\�쐬�X�e�[�^�X���擾
            InventoryMonthControlPartsBalance pbrec = new InventoryMonthControlPartsBalanceDao(db).GetByKey(string.Format("{0:yyyyMMdd}", WorkingDate.InventoryMonth));
            if (pbrec != null)
            {
                form["InventoryStatusPartsBalance"] = pbrec.InventoryStatus;
            }
            else
            {
                form["InventoryStatusPartsBalance"] = "";
            }

            return Criteria(form, line);
        }

        /// <summary>
        /// ���i�I����ʕ\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή�  �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂ֕ύX
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form, EntitySet<PartsInventory> line)
        {
            ActionResult ret;

            db = new CrmsLinqDataContext();
			
			//Mod 2015/05/29 arc yano IPO�Ή�(���i�I��) �^�C���A�E�g�ݒ肷��ꏊ�ƃ^�C���A�E�g�l�̕ύX(300��600�b)
            //�^�C���A�E�g�l�̐ݒ�
            db.CommandTimeout = 600;

            db.Log = new OutputWriter();

            //Mod 2016/08/13 arc yano #3596
            //---------------------------------
            //���傩��q�ɂ̐ݒ�
            //---------------------------------
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

            if (dWarehouse != null)
            {
                //�q�ɃR�[�h
                form["WarehouseCode"] = dWarehouse.WarehouseCode;
                //�q�ɖ�
                form["WarehouseName"] = dWarehouse.Warehouse.WarehouseName;
            }
            else
            {
                //�q�ɃR�[�h
                form["WarehouseCode"] = "";
                //�q�ɖ�
                form["WarehouseName"] = "";
            }

            //�I���Ώی��̎擾(Datetime�^)
            DateTime inventoryMonth;

            if (false == DateTime.TryParse(form["InventoryMonth"] + "/01", out inventoryMonth))
            {
            	//Mod 2015/06/09 arc nakayama IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�C �I����Ɠ��˒I������ɕύX
                ModelState.AddModelError("", "�I��������o�^����Ă��Ȃ����߁A�I�����J�n�ł��܂���");
                line = new EntitySet<PartsInventory>();
                SetComponent(form);
                return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }

            //ReuquestFlag�ɂ�鏈���̐U����
            switch (form["RequestFlag"])
            {
                case "1":   //�I���J�n
                    ret = InventoryStart(form, inventoryMonth);
                    break;

                case "2":   //Excel�o��
                    ret = Download(form, line, inventoryMonth);
                    break;

                case "3":   //�I����ƌ��ʈꎞ�ۑ�
                    ret = TemporalilyStored(form, line, inventoryMonth);
                    break;

                case "4":   //�I���m��
                    ret = InventoryDecided(form, line, inventoryMonth);
                    break;

                default:    //�����܂��̓y�[�W���O
                    //�y�[�W���O
                    if (form["RequestFlag"].Equals("999"))
                    {
                        if(string.IsNullOrWhiteSpace(form["DepartmentCode"])){
                            ModelState.AddModelError("", "����R�[�h����͂��Ă�������");
                            SetComponent(form);
                            return View("PartsInventoryCriteria", new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
                        }

                        //�I���󋵂����{���̏ꍇ�ŁA�ҏW���t���O��True�̏ꍇ
                        if (GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth).Equals(STS_INACTION) && bool.Parse(form["EditFlag"]) == true)
                        {
                            ret = TemporalilyStored(form, line, inventoryMonth);
                        }
                    }

                    //�����������s
                    ret = SearchList(form, inventoryMonth);
                    break;
            }

            //�R���g���[���̗L������
            GetViewResult(form);

            return ret;
        }
        #region  ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���i�݌ɒI���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂ֕ύX
        /// </history>
        [AuthFilter]
        private ActionResult SearchList(FormCollection form, DateTime inventoryMonth)
        {
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            ModelState.Clear();
            
            //�������ʏ�����
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //�q�ɖ��I���󋵂��`�F�b�N
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);       //Mod 2016/08/13 arc yano #3596

            // Mod 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A ���i�I���ΏۊO�̕���̃��[�U�̏ꍇ�͕���R�[�h���󗓂ɂ���
            // �������ʃ��X�g�̎擾
            if ((criteriaInit == true) && ((status == STS_UNEXCUTED || status == STS_INVALID ) ) )    //�����\�����A���I�������{�̏ꍇ
            {
                //�������Ȃ�
            }
            else
            {
                //��������
                list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            }

            //��ʐݒ�
            SetComponent(form);

            return View("PartsInventoryCriteria", list);
        }
        
        /// <summary>
        /// �������s
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>���i�I���ꗗ</returns>
        private IQueryable<PartsInventory> GetSearchResultList(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //�@�������ڂ̐ݒ�
            //---------------------
            InventoryStock InventoryStockCondition = SetCondition(form, inventoryMonth);

          �@//�������s
            return new InventoryStockDao(db).GetListByCondition(InventoryStockCondition);
        }

        /// <summary>
        /// ���������ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>��������</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���������ɑq�ɃR�[�h��ǉ�
        /// </history>
        private InventoryStock SetCondition(FormCollection form, DateTime inventoryMonth)
        {
            CodeDao dao = new CodeDao(db);

            //---------------------
            //�@�������ڂ̐ݒ�
            //---------------------
            InventoryStock Condition = new InventoryStock();
            Condition.InventoryMonth = inventoryMonth;
            Condition.DepartmentCode = form["DepartmentCode"];
            Condition.LocationCode = form["LocationCode"];
            Condition.LocationName = form["LocationName"];
            Condition.PartsNumber = form["PartsNumber"];
            Condition.PartsNameJp = form["PartsNameJp"];
            Condition.InventoryType = TYP_PARTS;                                                                     //�I����� = �u���i�v
            Condition.DiffQuantity = bool.Parse(form["DiffQuantity"].Contains("true") ? "true": "false");            //�I���L��

            Condition.WarehouseCode = form["WarehouseCode"];                                                         //�q�ɃR�[�h    //Add 2016/08/13 arc yano #3596


            //�������s
            return Condition;
        }
        #endregion

        #region �I�����
        /// <summary>
        /// �I���J�n
        /// </summary>
        /// <param name="form"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns>�����������ꂽ���</returns
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        [AuthFilter]
        private ActionResult InventoryStart(FormCollection form, DateTime inventoryMonth)
        {
            //-----------------------------
            //��������
            //-----------------------------
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVSTART);

            ModelState.Clear();

            //�������ʂ̐ݒ�
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validation�`�F�b�N
            //-----------------------------
            //�I���J�n�`�F�b�N
            ValidationForInventoryStart(form, inventoryMonth);
            if (!ModelState.IsValid)
            {
                //��ʍ��ڂ̐ݒ�
                SetComponent(form);
                return View("PartsInventoryCriteria", list); 
            }

            CodeDao dao = new CodeDao(db);

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStock�e�[�u���̃��R�[�h�쐬
                //------------------------------------------------------
                //InventoryStock�e�[�u���̓X�g�A�h�v���V�[�W���ōX�V����B
                //var ret = db.InsertInventoryStock(inventoryMonth, form["DepartmentCode"], ((Employee)Session["Employee"]).EmployeeCode);
                var ret = db.InsertInventoryStock(inventoryMonth, form["WarehouseCode"], ((Employee)Session["Employee"]).EmployeeCode);

                //------------------------------------------------------
                //InventoryScheduleParts�̃��R�[�h�쐬
                //------------------------------------------------------

                InventoryScheduleParts rec = new InventoryScheduleParts();

                //rec.DepartmentCode = form["DepartmentCode"];    //����R�[�h
                rec.DepartmentCode = "";                          //����R�[�h
                rec.InventoryMonth = inventoryMonth;
                rec.InventoryType = TYP_PARTS;
                rec.InventoryStatus = STS_INACTION; //���{��
                rec.StartDate = DateTime.Now;
                rec.EndDate = null;
                rec.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.CreateDate = DateTime.Now;
                rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                rec.LastUpdateDate = DateTime.Now;
                rec.DelFlag = "0";

                rec.WarehouseCode = form["WarehouseCode"];      //Add 2016/08/13 arc yano #3596

                //InventoryScheduleParts�Ƀ��R�[�h�ǉ�
                db.InventoryScheduleParts.InsertOnSubmit(rec);

                //-------------------------------------------------
                //�R�~�b�g����
                //-------------------------------------------------
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVSTART, FORM_NAME, "");
                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            return View("PartsInventoryCriteria", list);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_INVSTART, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_INVSTART, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }

            }

            //���������ꍇ�́A���b�Z�[�W��\������B
            ModelState.AddModelError("", "�I�����J�n���܂���");

            //��ʍ��ڂ̐ݒ�
            SetComponent(form);

            //���������s����
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

            return View("PartsInventoryCriteria", list);
        }

        /// <summary>
        /// �ꎞ�ۑ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">�I�����X�g(1�y�[�W��)</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult TemporalilyStored(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //��������
            //-----------------------------
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_TEMPSTORED);

            ModelState.Clear();
            
            //�������ʂ̐ݒ�
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validation�`�F�b�N
            //-----------------------------
            //���i�݌ɒI����񖳂�
            if (line == null)
            {
                ModelState.AddModelError("", "���i�݌ɒI�����0���̂��߁A�X�V�ł��܂���");
            }

            //�I���X�e�[�^�X�`�F�b�N
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                return View("PartsInventoryCriteria", list);
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStock�e�[�u���̃��R�[�h�X�V
                //------------------------------------------------------
                //�I�����A�q�ɃR�[�h���L�[�ɒI�������擾
                List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);
                //List<InventoryStock> isList = new InventoryStockDao(db).GetListByInvnentoryMonthDepartment(inventoryMonth, form["DepartmentCode"]);   //Mod 2016/08/13 arc yano #3596

                for (int k = 0; k < line.Count; k++)
                {
                    //��ʂœ��͂������I�A�R�����g��InventoryStock�e�[�u���ɕۑ�
                    //InventoryStock ivs = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[k].LocationCode, line[k].PartsNumber);
                    InventoryStock ivs = isList.Where(x => x.LocationCode.Equals(line[k].LocationCode) && x.PartsNumber.Equals(line[k].PartsNumber)).FirstOrDefault();

                    if (ivs != null)
                    {
                        //���I��                        
                        if (ivs.PhysicalQuantity != line[k].PhysicalQuantity)
                        {
                            ivs.PhysicalQuantity = line[k].PhysicalQuantity;
                        }

                        //�R�����g
                        if (!string.IsNullOrWhiteSpace(ivs.Comment) || !string.IsNullOrWhiteSpace(line[k].Comment))
                        {
                            if (!string.IsNullOrWhiteSpace(ivs.Comment) && !string.IsNullOrWhiteSpace(line[k].Comment))
                            {
                                if (!ivs.Comment.Equals(line[k].Comment))
                                {
                                    ivs.Comment = line[k].Comment;
                                }
                            }
                            else
                            {
                                ivs.Comment = line[k].Comment;
                            }
                        }
                    }
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_TEMPSTORED, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            SetComponent(form);
                            return View("PartsInventoryCriteria", list);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_TEMPSTORED, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            //�ۑ����܂����B�̃��b�Z�[�W���o��
            ModelState.AddModelError("", "�I���f�[�^�̈ꎞ�ۑ����s���܂���");

            //�Č��������s����
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            SetComponent(form);

            return View("PartsInventoryCriteria", list);
        }

        /// <summary>
        /// �I���m��
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <param name="inventoryMonth"></param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        ///  2016/03/17 arc yano #3477 ���i���P�[�V�����}�X�^�@���P�[�V�����}�X�^�̎����X�V �I���m�莞�ɕ��i���P�[�V�����}�X�^�ɔ��f����悤�ɏC��
        /// </history>
        [AuthFilter]
        private ActionResult InventoryDecided(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-----------------------------
            //��������
            //-----------------------------
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_INVENTORY_DECIDED);

            //�������ʂ̐ݒ�
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();

            //-----------------------------
            //validation�`�F�b�N
            //-----------------------------
            //�I���X�e�[�^�X�`�F�b�N
            ValidateInventoryStatus(form, inventoryMonth);

            if (!ModelState.IsValid)
            {
                //��ʍ��ڂ̐ݒ�
                SetComponent(form);
                return View("PartsInventoryCriteria",  list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            }
			
			//2015/05/14 arc yano IPO(���i�I��)�Ή� Excel�捞��A���������Ɋm�菈�����s���ƃV�X�e���G���[�̑Ή�
            //-----------------------------
            //�e�p�����[�^�̐ݒ�
            //-----------------------------

            //�Ј��R�[�h
            string employeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //����R�[�h
            //string departmentCode = form["DepartmentCode"];

            string warehouseCode = form["WarehouseCode"];               //Add 2016/08/13 arc yano #3596
            //------------------------------
            //���͒l�̍X�V
            //------------------------------
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //�ҏW�t���O��ON�̏ꍇ
                if (bool.Parse(form["EditFlag"]) == true)
                {
            //------------------------------------------------------
            //��x���͒l�ŁAInventoryStock�̃f�[�^���X�V
            //------------------------------------------------------
            //Mod 2016/08/13 arc yano #3596
            List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, form["WarehouseCode"]);

                    for (int k = 0; k < line.Count; k++)
                    {
                        //��ʂœ��͂������I�A�R�����g��InventoryStock�e�[�u���ɕۑ�
                        InventoryStock ivs = isList.Where(x => x.LocationCode.Equals(line[k].LocationCode) && x.PartsNumber.Equals(line[k].PartsNumber)).FirstOrDefault();

                        if (ivs != null)
                        {
                            //���I��                        
                            if (ivs.PhysicalQuantity != line[k].PhysicalQuantity)
                            {
                                ivs.PhysicalQuantity = line[k].PhysicalQuantity;
                            }

                            //�R�����g
                            if (!string.IsNullOrWhiteSpace(ivs.Comment) || !string.IsNullOrWhiteSpace(line[k].Comment))
                            {
                                if (!string.IsNullOrWhiteSpace(ivs.Comment) && !string.IsNullOrWhiteSpace(line[k].Comment))
                                {
                                    if (!ivs.Comment.Equals(line[k].Comment))
                                    {
                                        ivs.Comment = line[k].Comment;
                                    }
                                }
                                else
                                {
                                    ivs.Comment = line[k].Comment;
                                }
                            }
                        }
                    }

                
                    for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                    {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            return View("PartsInventoryCriteria", list);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }
            }

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {               
                //------------------------------------------------------
                //inventoryStock�e�[�u��,PartsStock�̍X�V
                //------------------------------------------------------
                //Mod 2016/08/13 arc yano #3596 
                //�X�g�v���Ŏ��s����B
                var ret = db.InventoryDecided(inventoryMonth, warehouseCode, employeeCode);     //Mod 2016/08/13 arc yano #3596
                //var ret = db.InventoryDecided(inventoryMonth, departmentCode, employeeCode);

                //Mod Mod 2015/09/18 arc yano ���i�d�|�݌� ��Q�Ή��E�d�l�ύX�G �d�|�f�[�^�̒��o���r���[�e�[�u������X�g�A�h�ɕύX
                //------------------------------------------------------
                //inventoryParts_Shikakari�̍쐬
                //------------------------------------------------------
                //�X�g�v���Ŏ��s����B
                //var ret2 = db.Insert_InventoryParts_Shikakari(inventoryMonth.Date, departmentCode);
                //var ret2 = db.GetPartsWipStock(1 , inventoryMonth.Date, "departmentCode", "", "", "", "", "", "", "", "");
                var ret2 = db.GetPartsWipStock(1, inventoryMonth.Date, "", warehouseCode, "", "", "", "", "", "", "", "");  //Mod 2016/08/13 arc yano #3596

                //------------------------------------------------------
                //PartsLocation�̔��f //Mod 2016/03/17 arc yano #3477
                //------------------------------------------------------
                //�X�g�v���Ŏ��s����B
                //var ret3 = db.InsertPartsLocation(departmentCode, employeeCode);
                var ret3 = db.InsertPartsLocation(warehouseCode, employeeCode);     //Mod 2016/08/13 arc yano #3596

                //------------------------------------------------------
                //InventoryShchedule�̍X�V
                //------------------------------------------------------
                //InventoryScheduleParts isrec = new InventorySchedulePartsDao(db).GetByKey(form["DepartmentCode"], inventoryMonth, TYP_PARTS); //Mod 2016/08/13 arc yano #3596
                InventoryScheduleParts isrec = new InventorySchedulePartsDao(db).GetByKey(form["WarehouseCode"], inventoryMonth, TYP_PARTS);

                if (isrec != null)
                {
                    isrec.InventoryStatus = STS_DECIDED;                                                //�X�e�[�^�X���u�����v�ɍX�V
                    isrec.EndDate = DateTime.Now;
                    isrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    isrec.LastUpdateDate = isrec.EndDate;
                }
                
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");

                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_INVSTART, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            return View("PartsInventoryCriteria", list);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_INVENTORY_DECIDED, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            //Add 2015/05/25 IPO�Ή�(���i�I��) �m���A���f�����l�ōČ��������s���邽�߁A���f������x�N���A����
            //��xModelState���N���A
            ModelState.Clear();

            //���b�Z�[�W�ݒ�
            ModelState.AddModelError("", "�I�����m�肵�܂���");

            //�Č��������s����
            list = new PaginatedList<PartsInventory>(GetSearchResultList(form, inventoryMonth), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            
            //��ʃR���|�[�l���g�̐ݒ�
            SetComponent(form);
            
            return View("PartsInventoryCriteria", list);
        }

        #endregion

        //2015/06/09 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D Excel�t�@�C���̃e���v���[�g��
        /// <summary>
        /// Excel�t�@�C���̃_�E�����[�h
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">�I���ꗗ(1�y�[�W��)</param>
        /// <param name="inventoryMonth">�I����</param>
        private ActionResult Download(FormCollection form, EntitySet<PartsInventory> line, DateTime inventoryMonth)
        {
            //-------------------------------
            //��������
            //-------------------------------
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //�������ʂ̐ݒ�
            PaginatedList<PartsInventory> list = new PaginatedList<PartsInventory>();
           
            //�����ꗗ��\�����Ă����ꍇ�͕\���������ʂ�ݒ肷��
            if (line != null)
            {
                list = new PaginatedList<PartsInventory>(line.AsQueryable(), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            } 

            //-------------------------------
            //Excel�o�͏���
            //-------------------------------

            //�t�@�C����(PartsInventory_xxx(����R�[�h)_yyyyMM(�Ώ۔N��)_yyyyMMddhhmiss(�_�E�����[�h����))
            string fileName = "PartsInventory" + "_" + form["DepartmentCode"] + "_" + string.Format("{0:yyyyMM}", inventoryMonth) + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //���[�N�t�H���_�擾
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            string filePathName = filePath + fileName;

            //�e���v���[�g�t�@�C���p�X�擾
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsInventory"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsInventory"];

            //�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��Ȃ��ꍇ
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");
                SetComponent(form);
                return View("PartsInventoryCriteria", list);
            }

            //�G�N�Z���f�[�^�쐬
            byte[] excelData = MakeExcelData(form, inventoryMonth, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetComponent(form);
                return View("PartsInventoryCriteria", list);
            }

            //�R���e���c�^�C�v�̐ݒ�
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        //Mod 2015/06/09 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D Excel�t�@�C���̃e���v���[�g��
        /// <summary>
        /// �G�N�Z���f�[�^�쐬(�e���v���[�g�t�@�C������)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <param name="fileName">���[��</param>
        /// <param name="tfileName">���[�e���v���[�g</param>
        /// <returns>�G�N�Z���f�[�^</returns>
        private byte[] MakeExcelData(FormCollection form, DateTime inventoryMonth, string fileName, string tfileName)
        {
            
            //----------------------------
            //��������
            //----------------------------
            ConfigLine configLine;                  //�ݒ�l
            byte[] excelData = null;                //�G�N�Z���f�[�^
            //string sheetName = "PartsInventory";    //�V�[�g��
            //int dateType = 0;                       //�f�[�^�^�C�v(���[�`��)
            //string setPos = "A1";                   //�ݒ�ʒu
            bool ret = false;
            bool tFileExists = true;                //�e���v���[�g�t�@�C������^�Ȃ�(���ۂɂ��邩�ǂ���)


            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

             //�G�N�Z���t�@�C���I�[�v��(�e���v���[�g�t�@�C������)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //�e���v���[�g�t�@�C�������������ꍇ
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����������܂���ł����B");
                //excelData = excelFile.GetAsByteArray();
                //�t�@�C���폜
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //----------------------------
            // �ݒ�V�[�g�擾
            //----------------------------
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //�ݒ�f�[�^���擾(config)
            if (config != null)
            {
                configLine = dExport.GetConfigLine(config, 2);
            }
            else //config�V�[�g�������ꍇ�̓G���[
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����config�V�[�g���݂���܂���");
                
                excelData = excelFile.GetAsByteArray();
                
                //�t�@�C���폜
                try
                {
                    dExport.DeleteFileStream(fileName);
                }
                catch
                {
                    //
                }
                return excelData;
            }

            //���[�N�V�[�g�I�[�v��
            var worksheet = excelFile.Workbook.Worksheets[configLine.SheetName];
            
            //----------------------------
            // ���������o��
            //----------------------------
            configLine.SetPos[0] = "A1";

            //���������擾
            InventoryStock condition = SetCondition(form, inventoryMonth);

            //����������������쐬
            DataTable dtCondtion = MakeConditionRow(condition);

            //�f�[�^�ݒ�
            ret = dExport.SetData(ref excelFile, dtCondtion, configLine);

            //----------------------------
            // �f�[�^�s�o��
            //----------------------------
            //�o�͈ʒu�̐ݒ�
            configLine.SetPos[0] = "A3";

            //�I�����̎擾
            List<PartsInventory> list = GetSearchResultList(form, inventoryMonth).ToList();

            //�f�[�^�ݒ�
            ret = dExport.SetData<PartsInventory, PartsInventoryForExcel>(ref excelFile, list, configLine);
            
            //Mod 2015/07/26 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�F ���_�݌ɂ̒P���A���z�ǉ�
            //�v�Z���Đݒ�
            worksheet.Cells[1, 9].Formula = worksheet.Cells[1, 9].Formula;
 
            excelData = excelFile.GetAsByteArray();

            //���[�N�t�@�C���폜
            try
            {
                excelFile.Stream.Close();
                excelFile.Dispose();
                dExport.DeleteFileStream(fileName);
            }
            catch
            {
                //
            }

            return excelData;
        }

        /// <summary>
        /// �t�B�[���h��`�f�[�^����o�̓t�B�[���h���X�g���擾����
        /// </summary>
        /// <param name="documentName">���[��</param>
        /// <returns></returns>
        private IEnumerable<XElement> GetFieldList(string documentName)
        {
            XDocument xml = XDocument.Load(Server.MapPath("/Models/ExportFieldList.xml"));
            var query = (from x in xml.Descendants("Title")
                         where x.Attribute("ID").Value.Equals(documentName)
                         select x).FirstOrDefault();
            if (query == null)
            {
                return null;
            }
            else
            {
                var list = from a in query.Descendants("Name") select a;

                return list;
            }
        }

        /// <summary>
        /// �����������쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="condition">��������</param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �q�ɂ̌���������ǉ�
        /// </history>
        private DataTable MakeConditionRow(InventoryStock condition)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();
            String conditionText = "";

            string departmentName = new DepartmentDao(db).GetByKey(condition.DepartmentCode, false).DepartmentName;

            //�q�ɖ��擾
            string warehouseName = new WarehouseDao(db).GetByKey(condition.WarehouseCode).WarehouseName;        //Add 2016/08/13 arc yano #3596

            //---------------------
            //�@���`
            //---------------------
            //�P�̗��ݒ�  
            if (condition.InventoryMonth != null)
            {
                dt.Columns.Add("CondisionText", Type.GetType("System.String"));
            }
           
            //---------------
            //�f�[�^�ݒ�
            //---------------
            DataRow row = dt.NewRow();
            //����R�[�h
            if (!string.IsNullOrEmpty(condition.DepartmentCode))
            {
                conditionText += string.Format("����={0}:{1}�@", condition.DepartmentCode, departmentName);
            }
            //Add 2016/08/13 arc yano #3596
            //�q�ɃR�[�h     
            if (!string.IsNullOrEmpty(condition.WarehouseCode))
            {
                conditionText += string.Format("�q��={0}:{1}�@", condition.WarehouseCode, warehouseName);
            }
            if (condition.InventoryMonth != null)
            {
                conditionText += string.Format("�Ώ۔N��={0:yyyy/MM}�@", condition.InventoryMonth);
            }
            //���P�[�V�����R�[�h
            if (!string.IsNullOrEmpty(condition.LocationCode))
            {
                conditionText += string.Format("���P�[�V�����R�[�h={0}�@", condition.LocationCode);
            }
            //���P�[�V������
            if (!string.IsNullOrEmpty(condition.LocationName))
            {
                conditionText += string.Format("���P�[�V������={0}�@", condition.LocationName);
            }
            //���i�ԍ�
            if (!string.IsNullOrEmpty(condition.PartsNumber))
            {
                conditionText += string.Format("���i�ԍ�={0}�@", condition.PartsNumber);
            }
            //���i��
            if (!string.IsNullOrEmpty(condition.PartsNameJp))
            {
                conditionText += string.Format("���i��={0}�@", condition.PartsNameJp);
            }
            //�I���L��
            if (condition.DiffQuantity == true)
            {
                conditionText += string.Format("�I���L��=�I�������郌�R�[�h�̂ݕ\���@", condition.DiffQuantity);
            }

            //�쐬�����e�L�X�g���J�����ɐݒ�
            row["CondisionText"] = conditionText;

            dt.Rows.Add(row);   
         
            return dt;
        }

        /// <summary>
        /// �w�b�_�s�̍쐬(Excel�o�͗p)
        /// </summary>
        /// <param name="list">�񖼃��X�g</param>
        /// <returns></returns>
        private DataTable MakeHeaderRow(IEnumerable<XElement> list)
        {
            //�o�̓o�b�t�@�p�R���N�V����
            DataTable dt = new DataTable();
            
            //�f�[�^�e�[�u����xml�̒l��ݒ肷��
            int i = 1;
            DataRow row = dt.NewRow();
            foreach (var header in list)
            {
                dt.Columns.Add("Column" + i, Type.GetType("System.String"));
                row["Column" + i] = header.Value;
                i++;
            }

            dt.Rows.Add(row);

            return dt;
        }

        #region �I���X�e�[�^�X�擾
        // 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���i�݌ɂ̒I���̊Ǘ��𕔖喈���q�ɖ��ɕύX
        /// <summary>
        /// �I���X�e�[�^�X�̕ԋp(�q�ɖ�)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>�I���X�e�[�^�X</returns>
        private string GetInventoryStatusByWarehouse(string warehouseCode, DateTime inventoryMonth)
        {

            string ret = STS_UNEXCUTED;     //�f�t�H���g�́u�����{�v

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(warehouseCode, inventoryMonth, TYP_PARTS);

            //�Ώی��̃��R�[�h����
            if (rec != null)
            {
                if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_INACTION)))
                {
                    ret = STS_INACTION;     //���{��
                }
                else
                {
                    ret = STS_DECIDED;     //����
                }
            }
            else
            {
                //����R�[�h��null�̏ꍇ��
                if (string.IsNullOrWhiteSpace(warehouseCode))
                {
                    ret = STS_INVALID;      //�X�e�[�^�X����
                }
            }

            return ret;
        }
        /*
        /// <summary>
        /// �I���X�e�[�^�X�̕ԋp(���喈)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns>�I���X�e�[�^�X</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬
        /// </history>
        private string GetInventoryStatusByDepartment(string departmentCode, DateTime inventoryMonth)
        {

            string ret = STS_UNEXCUTED;     //�f�t�H���g�́u�����{�v

            InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(departmentCode, inventoryMonth, TYP_PARTS);

            //�Ώی��̃��R�[�h����
            if (rec != null)
            {
                if (!string.IsNullOrEmpty(rec.InventoryStatus) && (rec.InventoryStatus.Equals(STS_INACTION)))
                {
                    ret = STS_INACTION;     //���{��
                }
                else
                {
                    ret = STS_DECIDED;     //����
                }
            }
            //Mod 2015/05/27 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A ����R�[�h���󗓂̏ꍇ�̓X�e�[�^�X�𖳌��Ƃ���B
            else
            {
                //����R�[�h��null�̏ꍇ��
                if (string.IsNullOrWhiteSpace(departmentCode))
                {
                    ret = STS_INVALID;      //�X�e�[�^�X����
                }
            }

            return ret;
        }
        */

        #endregion

        
        #region Validation�`�F�b�N
        /// <summary>
        /// �I���J�n����Validation�`�F�b�N
        /// </summary>
        /// <param name="form">�t�H�[��</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void ValidationForInventoryStart(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //���喈�I���󋵂��`�F�b�N
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            //�I���X�e�[�^�X=�u���{���v
            if (status.Equals(STS_INACTION))
            {
                ModelState.AddModelError("", "�q��" + form["WarehouseName"] + "�̑Ώ۔N���̕��i�I�������{���ł��邽�߁A�I�����J�n�ł��܂���");
            }
            else if (status.Equals(STS_DECIDED))
            {
                ModelState.AddModelError("", "�q��" + form["WarehouseName"] + "�̑Ώ۔N���̕��i�I�����I�����Ă��邽�߁A�I�����J�n�ł��܂���");
            }
            //--------------------------------
            //��Ɠ��̃`�F�b�N
            //--------------------------------
            DateTime inventoryWorkingDate;

            inventoryWorkingDate = DateTime.Parse(form["PartsInventoryWorkingDate"]);
            //�������t����Ɠ��O�̏ꍇ�̓G���[
            if (DateTime.Now.Date.CompareTo(inventoryWorkingDate) < 0)
            {
                ModelState.AddModelError("", "�I������ɂȂ��Ă��Ȃ����߁A�I�����J�n�ł��܂���");
            }

            return;
        }

        /// <summary>
        /// �I���󋵂�Validation�`�F�b�N
        /// </summary>
        /// <param name="form">�t�H�[��</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void ValidateInventoryStatus(FormCollection form, DateTime inventoryMonth)
        {
            //--------------------------------
            //���喈�I���󋵂��`�F�b�N
            //--------------------------------
            string status = GetInventoryStatusByWarehouse(form["WarehouseCode"], inventoryMonth);

            //�I���X�e�[�^�X=�u�����v
            if (status.Equals(STS_DECIDED))
            {
                ModelState.AddModelError("", "�q��" + form["WarehouseName"] + "�̑Ώ۔N���̕��i�I���͏I�����Ă��邽�߁A�X�V�ł��܂���B");
            }

            return;            
        }

        /// <summary>
        /// �I���I���ҏW����validation�`�F�b�N
        /// </summary>
        /// <param name="form"></param>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j �V�K�쐬
        /// </history>
        private void ValidateForDataEdit(FormCollection form, EntitySet<PartsInventory> line)
        {
            int cnt = 0;

            bool msg = false;


            //�I����
            DateTime inventorymonth = DateTime.Parse(form["InventoryMonth"]);

            if (line == null || line.Count == 0)
            {
                ModelState.AddModelError("", "���ׂ�0�s�̂��߁A�ۑ��ł��܂���");

                //�����^�[��
                return;
            }

            foreach (var a in line)
            {
                ///------------------------
                // �K�{�`�F�b�N
                //------------------------           
                //���P�[�V�����R�[�h
                if (string.IsNullOrWhiteSpace(a.LocationCode))
                {
                    ModelState.AddModelError("line[" + cnt + "].LocationCode", MessageUtils.GetMessage("E0001", cnt + 1 + "�s�ڂ̃��P�[�V�����R�[�h"));
                }
                else
                {
                    Location loc = new LocationDao(db).GetByKey(a.LocationCode);

                    Warehouse warehouse = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);


                    //�ʂ̑q�ɂ̃��P�[�V�����R�[�h�����͂���Ă���ꍇ��
                    if (!loc.WarehouseCode.Equals(form["WarehouseCode"]))
                    {
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", warehouse.WarehouseName + "�̃��P�[�V�����R�[�h����͂��ĉ�����");
                    }
                }
                //���i�ԍ�
                if (string.IsNullOrWhiteSpace(a.PartsNumber))
                {
                    ModelState.AddModelError("line[" + cnt + "].PartsNumber", MessageUtils.GetMessage("E0001", cnt + 1 + "�s�ڂ̕��i�ԍ�"));
                }
                //----------------------------------------------
                // �����ϐ������ʂ������Ă��Ȃ����̃`�F�b�N
                //----------------------------------------------
                if (a.PhysicalQuantity - a.ProvisionQuantity < 0)
                {
                    ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "���ʂ�葽�������ϐ���ݒ�ł��܂���");
                }
                //---------------------------------------------
                // �d���`�F�b�N
                //---------------------------------------------
                if (a.NewRecFlag.Equals(true))  //�V�K�ǉ��������R�[�h
                {
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventorymonth, a.LocationCode, a.PartsNumber, false);

                    //���R�[�h���݂���ꍇ��validation�G���[
                    if (rec != null)
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "���P�[�V�����R�[�h=" + line[cnt].LocationCode + ": ���i�ԍ�=" + line[cnt].PartsNumber + "�͊��ɓo�^����Ă��܂�");
                    }
                }
                //---------------------------------------------
                // �d���`�F�b�N�Q�i�����ʓ��ɕ������݁j
                //---------------------------------------------
                int count = line.Where(x => x.LocationCode.Equals(a.LocationCode) && x.PartsNumber.Equals(a.PartsNumber)).Count();

                if (count > 1)
                {
                    if (msg == false)
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "���P�[�V�����R�[�h=" + line[cnt].LocationCode + ": ���i�ԍ�=" + line[cnt].PartsNumber + "���ҏW��ʏ�ɕ����s���݂��܂�");
                        msg = true;
                    }
                    else
                    {
                        ModelState.AddModelError("line[" + cnt + "].PartsNumber", "");
                        ModelState.AddModelError("line[" + cnt + "].LocationCode", "");
                    }
                    
                }


                cnt++;
            }

           

            return;
        }

        #endregion
        
        #region ��ʃR���|�[�l���g�ݒ�
        /// <summary>
        /// �e�R���g���[���̒l�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j �󕥕\�X�e�[�^�X�̐ݒ�̒ǉ�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void SetComponent(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            DateTime inventoryMonth;
            
            //���ڂ̐ݒ�
            ViewData["InventoryMonth"] = form["InventoryMonth"];                                                            //�Ώ۔N��
            ViewData["PartsInventoryWorkingDate"] = form["PartsInventoryWorkingDate"];                                      //�I����Ɠ�
            ViewData["DepartmentCode"] = form["DepartmentCode"];                                                            //����R�[�h
            
            //���喼
            if (form["DepartmentCode"] == null || string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
                ViewData["DepartmentName"] = "";
            }
            else
            {
                ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"].ToString()).DepartmentName;
            }

            //Add 2016/08/13 arc yano #3596
            //�q�ɃR�[�h
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            //�q�ɖ�
            ViewData["WarehouseName"] = form["WarehouseName"];

            ViewData["LocationCode"] = form["LocationCode"];                                                                //���P�[�V�����R�[�h
            ViewData["LocationName"] = form["LocationName"];                                                                //���P�[�V������
            ViewData["PartsNumber"] = form["PartsNumber"];                                                                  //���i�ԍ�
            ViewData["PartsNameJp"] = form["PartsNameJp"];                                                                  //���i��(���{��)
            ViewData["DiffQuantity"] = form["DiffQuantity"];                                                                //�I���L��
            ViewData["RequestFlag"] = "999";                                                                                //������� ���f�t�H���g�u�y�[�W���O�v�ɐݒ�

            ViewData["id"] = form["id"];

            //�I���󋵃X�e�[�^�X(���喈)
            if (true == DateTime.TryParse(ViewData["InventoryMonth"].ToString() + "/01", out inventoryMonth))
            {
                ViewData["InventoryStatus"] = GetInventoryStatusByWarehouse(ViewData["WarehouseCode"].ToString(), DateTime.Parse(ViewData["InventoryMonth"].ToString() + "/01"));
            }
            else
            {
                ViewData["InventoryStatus"] = STS_UNEXCUTED;    //�����{
            }
            
            // Add 2015/05/21 arc yano IPO�Ή�(���i�I��) �m��{�^���N���b�N�^�s��Ԓǉ�
            //�m��{�^���N���b�N�^�s���
            ViewData["InventoryEndButtonEnable"] = bool.Parse(form["InventoryEndButtonEnable"]);

            //Mod 2015/06/17 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�E �I����Ɠ��ł͂Ȃ��A�I���J�n���ɕύX
            ViewData["PartsInventoryStartDate"] = form["PartsInventoryStartDate"];

            //Add 2017/07/26 arc yano #3781
            ViewData["InventoryStatusPartsBalance"] = form["InventoryStatusPartsBalance"];

            return;
        }
        /// <summary>
        /// �e�R���g���[���̒l�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j �V�K�쐬
        /// </history>
        private void SetComponentDataEdit(FormCollection form)
        {
            CodeDao dao = new CodeDao(db);

            //���ڂ̐ݒ�
            ViewData["InventoryMonth"] = form["InventoryMonth"];                                                            //�Ώ۔N��
            ViewData["WarehouseCode"] = form["WarehouseCode"];                                                              //�q�ɃR�[�h
            ViewData["RequestFlag"] = "99";                                                                                 //������� ���f�t�H���g�u�����v�ɐݒ�

            return;
        }

        #endregion

        // <summary>
        /// �ҏW��ʏ����\��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j�V�K�쐬
        /// </history>
        public ActionResult DataEdit()
        {
            criteriaInit = true;
            FormCollection form = new FormCollection();
            EntitySet<PartsInventory> line = new EntitySet<PartsInventory>();

            //�����l�̐ݒ�
            form["RequestFlag"] = "99";                                              // 99(���N�G�X�g�̎�ނ́u�����v)

            form["CreateFlag"] = Request["CreateFlag"];                              //�V�K�쐬�t���O

            form["InventoryMonth"] = Request["InventoryMonth"] + "/01";              //�I���Ώ۔N����
            form["WarehouseCode"] = Request["WarehouseCode"];                        //�q�ɃR�[�h
            form["LocationCode"] = Request["LocationCode"];                          //���P�[�V�����R�[�h
            form["PartsNumber"] = Request["PartsNumber"];                            //���i�ԍ�

            return DataEdit(form, line);
        }

        /// <summary>
        /// �f�[�^�ҏW�ۑ�
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j�V�K�쐬
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DataEdit(FormCollection form, EntitySet<PartsInventory> line)
        {
            ModelState.Clear();

            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"]);

            ActionResult ret = null;

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            List<PartsInventory> list = new List<PartsInventory>();

            //�V�K�쐬�̏ꍇ
            if (!string.IsNullOrWhiteSpace(form["CreateFlag"]) && form["CreateFlag"].Equals("1"))
            {
                ret = AddLine(form, line);
            }
            //�X�V�̏ꍇ
            else
            {
                switch (form["RequestFlag"])
                {
                    case "10": //�ۑ�

                        //���͒l�̃`�F�b�N
                        ValidateForDataEdit(form, line);

                        if (!ModelState.IsValid)
                        {
                            //��ʐݒ�
                            SetComponentDataEdit(form);
                            
                            if (line != null && line.Count > 0)
                            {
                                list = line.ToList();
                            }
                            
                            return View("PartsInventoryDataEdit", list);
                        }

                        //�ۑ�����
                        ret = DataSave(form, line);
                        break;

                    case "11": //�s�ǉ�

                        ret = AddLine(form, line);
                        break;

                    case "12": //�s�폜

                        ret = DelLine(form, line);
                        break;

                    default: //��������

                        InventoryStock condition = new InventoryStock();

                        condition.InventoryMonth = inventoryMonth;
                        condition.DepartmentCode = form["DepartmentCode"];
                        condition.LocationCode = form["LocationCode"];
                        condition.LocationName = form["LocationName"];
                        condition.PartsNumber = form["PartsNumber"];
                        condition.PartsNameJp = form["PartsNameJp"];
                        condition.InventoryType = TYP_PARTS;

                        condition.WarehouseCode = new LocationDao(db).GetByKey(form["LocationCode"]).WarehouseCode;

                        list = new InventoryStockDao(db).GetListByCondition(condition).ToList<PartsInventory>();

                        //list = GetSearchResultList(form, inventoryMonth).Where(x => x.LocationCode.Equals(form["LocationCode"]) && x.PartsNumber.Equals(form["PartsNumber"])).ToList<PartsInventory>();
                        ret = View("PartsInventoryDataEdit", list);
                        break;
                }
            }

            //��ʐݒ�
            SetComponentDataEdit(form);

            //�R���g���[���̗L������
            //GetViewResult(form);

            return ret;
        }

        /// <summary>
        /// �I���݌Ƀf�[�^�ۑ�����
        /// </summary>
        /// <param name="form"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano �I���݌Ɂi���i�݌ɂ̏C���j#3781 �V�K�쐬
        /// </history>
        private ActionResult DataSave(FormCollection form, EntitySet<PartsInventory> line)
        {
            DateTime inventoryMonth = DateTime.Parse(form["InventoryMonth"]);


            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                //------------------------------------------------------
                //inventoryStockCar�e�[�u���̃��R�[�h�X�V
                //------------------------------------------------------
                List<InventoryStock> isList = new List<InventoryStock>();

                for (int k = 0; k < line.Count; k++)
                {
                    //���P�[�V�����R�[�h�A���i�ԍ����畔�i�݌ɒI���e�[�u�����擾����
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[k].LocationCode, line[k].PartsNumber, true);

                    if (rec != null)
                    {
                        rec.PhysicalQuantity = line[k].PhysicalQuantity;                                //���I��
                        rec.ProvisionQuantity = line[k].ProvisionQuantity;                              //�����ϐ�
                        rec.Comment = line[k].Comment;                                                  //�R�����g
                        rec.DelFlag = "0";                                                              //�폜�t���O�͏�ɗL���ɂ��Ă���
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;      //�ŏI�X�V��
                        rec.LastUpdateDate = DateTime.Now;                                              //�ŏI�X�V��
                    }
                    else
                    {
                        InventoryStock inventorystock = new InventoryStock();
                        inventorystock.InventoryId = Guid.NewGuid();
                        inventorystock.DepartmentCode = "";
                        inventorystock.InventoryMonth = inventoryMonth;
                        inventorystock.LocationCode = line[k].LocationCode;                                                     //���P�[�V�����R�[�h
                        inventorystock.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                             //�S���҃R�[�h
                        inventorystock.InventoryType = "002";                                                                   //�I�����=�u���i�v
                        inventorystock.SalesCarNumber = null;                                                                   //�ԗ��Ǘ��ԍ�
                        inventorystock.PartsNumber = line[k].PartsNumber;                                                       //���i�ԍ�
                        inventorystock.Quantity = line[k].PhysicalQuantity;                                                     //���_��(���I���œo�^)
                        inventorystock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    �@�@�@             //�쐬��
                        inventorystock.CreateDate = DateTime.Now;                                                               //�쐬��
                        inventorystock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                   //�ŏI�X�V��
                        inventorystock.LastUpdateDate = DateTime.Now;                                                           //�ŏI�X�V��
                        inventorystock.DelFlag = "0";                                                                           //�폜�t���O
                        inventorystock.Summary = "";                                                                            //���l
                        inventorystock.PhysicalQuantity = line[k].PhysicalQuantity;                                             //���I��
                        inventorystock.Comment = line[k].Comment;                                                               //�R�����g
                        inventorystock.ProvisionQuantity = line[k].ProvisionQuantity;                                           //�����ϐ�
                        inventorystock.WarehouseCode = new LocationDao(db).GetByKey(line[k].LocationCode, false).WarehouseCode; //�q�ɃR�[�h

                        isList.Add(inventorystock);
                    }

                    //�ۑ���͐V�K�쐬�t���O�𗎂Ƃ�
                    if (line[k].NewRecFlag.Equals(true))
                    {
                        line[k].NewRecFlag = false;
                    }
                }

                db.InventoryStock.InsertAllOnSubmit(isList);

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException cfe)
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_DATAEDIT, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            ts.Dispose();
                            return View("PartsInventoryDataEdit", line.ToList());
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_DATAEDIT, FORM_NAME, "");
                            ts.Dispose();
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_DATAEDIT, FORM_NAME, "");
                        ts.Dispose();
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            ModelState.AddModelError("", "�ۑ����܂���");

            return View("PartsInventoryDataEdit", line.ToList());
        }


        /// <summary>
        /// Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <returns>Excel�捞�p�_�C�A���O</returns>
        [AuthFilter]
        public ActionResult ImportDialog()
        {

            ViewData["ElapsedHours"] = String.Format("{0:00}", 0);
            ViewData["ElapsedMinutes"] = String.Format("{0:00}", 0);
            ViewData["ElapsedSeconds"] = String.Format("{0:00}", 0);

            return View("PartsInventoryImportDialog");
        }

        /// <summary>
        ///  Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� Excel�ǎ惁�\�b�h�̈����ǉ�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            //---------------------------
            //������
            //---------------------------
            ModelState.Clear();

            //�G�N�Z���f�[�^�i�[�p(1�s��)
            PartsInventory data = new PartsInventory();

            //�J�����ԍ��ۑ��p
            int[] colNumber;
            colNumber = new int[4] { -1, -1, -1, -1 };

            //���P�[�V�����R�[�h�{���i�ԍ����X�g(�d���`�F�b�N�p)
            List<string> olChkList = new List<string>();
           
            //�X�g�b�v�E�H�b�`����
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            //�X�g�b�v�E�H�b�`���J�n����
            sw.Start();

            //---------------------------
            // �t�@�C���̑��݃`�F�b�N
            //--------------------------
             // �t�@�C���̑��݃`�F�b�N
            ValidateImportFile(importFile);
            
            if (!ModelState.IsValid)
            {
                sw.Stop();
                ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
                ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
                ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
                return View("PartsInventoryImportDialog");
            }

            //---------------------------
            // Excel�f�[�^�ǎ�
            //--------------------------
            int dataRowCnt = ReadExcelData(importFile, form);

            if (!ModelState.IsValid)
            {
                sw.Stop();
                ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
                ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
                ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
                return View("PartsInventoryImportDialog");
            }

            //---------------------------
            // DB�X�V
            //---------------------------
            int ret = DBExecute();

            //�X�g�b�v�E�H�b�`���~�߂�
            sw.Stop();

            if (ret == -1)
            {
                //�G���[��ʂɑJ��
                return View("Error");
            }
            
            //Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� ����I�������ꍇ�̂݁A�������b�Z�[�W��\��
            //����I������Excel�f�[�^�̎捞�����̃��b�Z�[�W��\������B
            if (ret == 0)
            {
                //�G���[��ʂɑJ��
            //Mod 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A�捞������\��
            ModelState.AddModelError("", "Excel�̎捞���������܂����B�捞������" + dataRowCnt + "���ł�");
            }

            ViewData["ElapsedHours"] = String.Format("{0:00}", sw.Elapsed.Hours);
            ViewData["ElapsedMinutes"] = String.Format("{0:00}", sw.Elapsed.Minutes);
            ViewData["ElapsedSeconds"] = String.Format("{0:00}", sw.Elapsed.Seconds);
            return View("PartsInventoryImportDialog", ViewData);
        }

        /// <summary>
        /// Excel�f�[�^�擾&�ݒ�
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>��荞�񂾌���(�G���[�̏ꍇ��-1)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���i�I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2015/06/11 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D Exlcel�t�@�C���e���v���[�g���Ή�
        ///                                       �܂��A�f�[�^�������G�N�Z���̓��͒l����擾����悤�ɏC���B
        /// 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A Excel�Ŏ捞������Ԃ��悤�ɏC��
        /// </history>
        private int ReadExcelData(HttpPostedFileBase importFile, FormCollection form)
        {

            //------------------------
            // ������
            //------------------------
            //�ԋp�l
            int ret = -1;       //�G���[�ŏ�����

            int dataLowCnt = 0; //�f�[�^�捞����
            
            //�G�N�Z���̌��������s
            String conditionText = "";

            //����R�[�h
            string departmentCode = "";
            //�Ώ۔N��
            DateTime inventoryMonth = DateTime.Parse("1900/01/01");     //�K���Ȓl�����Ă���

            ConfigLine configLine;          //�ݒ�l           //Add 2015/06/11

            //�J�����ԍ��ۑ��p
            int[] colNumber;
            colNumber = new int[4] { -1, -1, -1, -1 };

            //���P�[�V�����R�[�h�{���i�ԍ����X�g(�d���`�F�b�N�p)
            List<string> dpChkList = new List<string>();

            //���P�[�V�����R�[�h���X�g
            List<string> sLocList = new List<string>();

            //Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� Excel�捞���d���G���[�Ή�(�}�X�^�`�F�b�N�p�̃��X�g�ǉ�)
            //�}�X�^�`�F�b�N�p
            List<string> sLocList2 = new List<string>();

            //���i�ԍ����X�g
            List<string> sPartsList = new List<string>();

            //Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� Excel�捞���d���G���[�Ή�(�}�X�^�`�F�b�N�p�̃��X�g�ǉ�)
            //�}�X�^�`�F�b�N�p
            List<string> sPartsList2 = new List<string>();

            //------------------------
            //Excel�捞����
            //------------------------
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
                        return ret;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("importFile", "�G���[���������܂����B" + ex.Message);
                    return ret;
                }
               
                //----------------------------
                // �ݒ�V�[�g�擾
                //----------------------------
                ExcelWorksheet config = pck.Workbook.Worksheets["config"];

                DataExport dExport = new DataExport();

                //�ݒ�f�[�^���擾(config)
                if (config != null)
                {
                    configLine = dExport.GetConfigLine(config, 2);
                }
                else //config�V�[�g�������ꍇ�̓G���[
                {
                    ModelState.AddModelError("", "�e���v���[�g�t�@�C����config�V�[�g���݂���܂���");
                    return ret;
                }

                //-----------------------------
                // �f�[�^�V�[�g�擾
                //-----------------------------
                var ws = pck.Workbook.Worksheets[configLine.SheetName];


                //Mod 2015/07/26 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�F ���_�݌ɂ̒P���A���z�ǉ�
                //�f�[�^�s���擾
                int rowCnt = ws.Cells[1, 9].GetValue<int>();

                //------------------------------
                //�捞�s��0���̏ꍇ
                //------------------------------
                if ((ws.Dimension == null) || (rowCnt == 0))
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "Excel�Ƀf�[�^������܂���B�X�V�������I�����܂���"));
                    return ret;
                }

                //------------------------------
                //���������s�擾
                //------------------------------
                //��ԍŏ��̍s�����������s�Ƃ���
                int row = ws.Dimension.Start.Row;
                int column = ws.Dimension.Start.Column;

                conditionText = ws.Cells[row, column].Text;

                //����R�[�h�A�Ώ۔N���̃`�F�b�N
                ExtractInf(conditionText, ref departmentCode, ref inventoryMonth);

                //����R�[�h����q�ɂ�����o��
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

                string warehouseCode = (dWarehouse != null ? dWarehouse.WarehouseCode : "");

                //------------------------------
                //�^�C�g���s�擾(2�s��)
                //------------------------------
                //���������s�̎��̍s���^�C�g���s
                row++;
                var headerRow = ws.Cells[row, column, row, ws.Dimension.End.Column];

                colNumber = SetColNumber(headerRow, colNumber);

                //�^�C�g���s�A�w�b�_�s�����������ꍇ�͑����^�[������
                if (!ModelState.IsValid)
                {
                    return ret;
                }

                //------------------------------
                //�f�[�^�s�擾(3�s��)
                //------------------------------
                //���i�I�����X�g�̎擾
                //List<InventoryStock> isList = new InventoryStockDao(db).GetListByInvnentoryMonthDepartment(inventoryMonth, departmentCode);   //Mod 2016/08/13 arc yano #3596
                List<InventoryStock> isList = new InventoryStockDao(db).GetListByInventoryMonthWarehouse(inventoryMonth, warehouseCode);


                //Mod 2016/08/13 arc yano #3596
                //Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� �p�t�H�[�}���X���P�̂��߁A�N�G���ύX
                //���P�[�V�����R�[�h���X�g�̎擾
                sLocList = (
                               from a in db.Location
                               where (string.IsNullOrWhiteSpace(warehouseCode) || warehouseCode.Equals(a.WarehouseCode))
                               && (a.DelFlag.Equals("0"))
                               select a.LocationCode
                           ).ToList();
                /*
                sLocList = (
                                from a in db.Location
                                where (string.IsNullOrWhiteSpace(departmentCode) || departmentCode.Equals(a.DepartmentCode))
                                && (a.DelFlag.Equals("0"))
                                select a.LocationCode
                            ).ToList();
                */
                //Mod 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D ���P�[�V�����R�[�h�ɋ󔒂�����ꍇ�͎�菜��
                //�擾�������P�[�V�����R�[�h�𔼊p�啶���ɕϊ�
                foreach (string a in sLocList)
                {
                    string locationCode = Strings.StrConv(a, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    sLocList2.Add(locationCode);
                }

                //Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� �p�t�H�[�}���X���P�̂��߁A�N�G���ύX
                //���i�ԍ����X�g�̎擾
                sPartsList = (
                                from a in db.Parts
                                where (a.DelFlag.Equals("0"))
                                select a.PartsNumber
                             ).ToList();

                //Mod 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D ���i�ԍ��ɋ󔒂�����ꍇ�͎�菜��
                //�擾�������i�ԍ��𔼊p�啶���ɕϊ�
                foreach (string a in sPartsList)
                {
                    string partsNumber = Strings.StrConv(a, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    sPartsList2.Add(partsNumber);
                }

                //Mod 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D ���P�[�V�����R�[�h�A���i�ԍ��ɋ󔒂�����ꍇ�͎�菜��
				//Mod 2015/05/14 arc yano IPO(���i�I��)�Ή� Excel�捞���d���G���[�Ή�(���P�[�V�����R�[�h�A���i�ԍ��̏����𔼊p�啶����)
                //���P�[�V�����R�[�h�A���i�ԍ������ꂼ�ꔼ�p�啶���ɕϊ�����B
                for (int i = 0; i < isList.Count(); i++ )
                {
                    isList[i].LocationCode = Strings.StrConv(isList[i].LocationCode, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                    isList[i].PartsNumber = Strings.StrConv(isList[i].PartsNumber, VbStrConv.Narrow, 0x0411).ToUpper().Trim();
                }
               
                //�^�C�g���s�̎��̍s���f�[�^�s
                row++;

                string[] array = new string[4];

                //Mod 2015/06/09 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D �G�N�Z���̍ŏI�s�̓e���v���[�g���擾����B
                //Mod 2015/06/01 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�A �G�N�Z���捞�������Ɏ�荞�񂾌�����\������
                int datarow = 0;

                for (datarow = row; datarow < rowCnt+ row; datarow++)
                //for (datarow = row; datarow <= ws.Dimension.End.Row; datarow++)
                {
                    PartsInventory data = new PartsInventory();

                    //�X�V�f�[�^�̎擾
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {

                        for (int i = 0; i < 4; i++)
                        {
                        
                            if (col == colNumber[i])
                            {
                                array[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
                    }
                    

                    //Mod 2015/05/14 arc yano IPO�Ή�(���i�I��) �p�t�H�[�}���X���P�̂��߁A�����̕ύX(�l�n�����Q�Ɠn��)�Ɗ֐����̔p�~
                    //------------------------------------------------
                    // ���͒l�`�F�b�N�����͍��ڂ̐ݒ�
                    //------------------------------------------------
                    //Mod 2015/05/15 arc yano IPO�Ή�(���i�I��) �����ǉ�(departmentCode)
                    //string[] conditions = ValidateDataProperty(array, ref data, ref sLocList, ref sLocList2, ref sPartsList, ref sPartsList2, departmentCode);        //Mod 2016/08/13 arc yano #3596
                    string[] conditions = ValidateDataProperty(array, ref data, ref sLocList, ref sLocList2, ref sPartsList, ref sPartsList2, warehouseCode);

                    //-------------------------------------------------
                    //�d���`�F�b�N(�p�t�H�[�}���X���l���Ċ֐�����p�~)
                    //-------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(data.LocationCode) && !string.IsNullOrWhiteSpace(data.PartsNumber))
                    {
                        // ���P�[�V�����R�[�h�{���i�ԍ��̓���g�ݍ��킹�����݂��邩�`�F�b�N
                        string target = dpChkList.Where(x => x.Equals(data.LocationCode + ":" + data.PartsNumber)).FirstOrDefault();

                        if (target != null)
                        {
                            ModelState.AddModelError("", "���P�[�V�����R�[�h:" + data.LocationCode + "�A ���i�ԍ�:" + data.PartsNumber + "�̃f�[�^���d�����Ă��܂��B");
                        }
                        
                        dpChkList.Add(data.LocationCode + ":" + data.PartsNumber);
                    }

                    //�f�[�^�ݒ�
                    SetData(data, warehouseCode, inventoryMonth, isList, conditions);      //Mod 2016/08/13 arc yano #3596
                }

                //�f�[�^�s�̌������擾
               dataLowCnt = datarow - row;

                //���؃G���[�����������ꍇ
                if (!ModelState.IsValid)
                {
                    int errCnt = 0;         //�G���[����
                    int wkCnt = 0;          //�ăZ�b�g����G���[����

                    //���؃G���[�̑S�Č������擾����B
                    foreach (var key in ModelState.Keys)
                    {
                        errCnt += ModelState[key].Errors.Count;
                    }

                    //�G���[������100���𒴂����ꍇ
                    if (errCnt > MAX_ERR_CNT)
                    {
                        KeyValuePair<string, ModelState>[] test = new KeyValuePair<string, System.Web.Mvc.ModelState>[ModelState.Count];

                        //�����������f���G���[��Ҕ�
                        ModelState.CopyTo(test, 0);

                        //���f������x�N���A
                        ModelState.Clear();

                        foreach (var a in test)
                        {
                            for (int i = 0; i < a.Value.Errors.Count; i++)
                            {
                                ModelState.AddModelError(a.Key, a.Value.Errors[i].ErrorMessage);
                                wkCnt++;

                                if (wkCnt >= MAX_ERR_CNT)
                                {
                                    break;
                                }
                            }

                            if (wkCnt >= MAX_ERR_CNT)
                            {
                                break;
                            }
                        }
                        
                        ModelState.AddModelError("ErrCnt", "�G���[�̑�����" + errCnt + "���ł��B100���܂ŕ\�����Ă��܂�");
                    }
                    else
                    {
                        ModelState.AddModelError("importFile", "�G���[�̑�����" + errCnt + "���ł�");
                    }
                }
            }

            //���؃G���[���������Ă��Ȃ��ꍇ
            if (ModelState.IsValid)
            {
                ret = dataLowCnt;
            }

            return ret;
        }

        /// <summary>
        /// DB�X�V
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        private int DBExecute()
        {
            
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(10.0)))
            {
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    //Mod 2015/05/14 arc yano IPO�Ή�(���i�I��) ��Ӑ���ᔽ�̏ꍇ�̓V�X�e���G���[��ʂɑJ�ڂ���悤�ɕύX
                    /*
                    // ��Ӑ���G���[�̏ꍇ�A���b�Z�[�W��ݒ肵�A�Ԃ�
                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        OutputLogger.NLogError(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { "���i�ԍ�", "�ۑ�" }));
                        ts.Dispose();
                        //�X�g�b�v�E�H�b�`���~�߂�
                        return 1;
                    }
                    else
                    {
                    */
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                        ts.Dispose();
                        return -1;
                    //}
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELIMPORT, FORM_NAME, "");
                    ts.Dispose();
                    // �G���[�y�[�W�ɑJ��
                    return -1;
                }
            }       
            return 0;
        }


        /// <summary>
        /// �捞�t�@�C�����݃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void ValidateImportFile(HttpPostedFileBase filePath)
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

                if (stExtension.IndexOf("xlsx") < 0)
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", "�t�@�C���̊g���q��xlsx�t�@�C���ł͂���܂���"));
                }
            }

            return;
        }

        /// <summary>
        /// �e���ڂ̗�ԍ��ݒ�
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //��������
            int cnt = 1;

            //��ԍ��ݒ�
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //���P�[�V�����R�[�h
                    if (cell.Text.Contains("���P�[�V�����R�[�h"))
                    {
                        colNumber[0] = cnt;
                    }
                    //���i�ԍ�
                    if (cell.Text.Contains("���i�ԍ�"))
                    {
                        colNumber[1] = cnt;
                    }
                    //���I��
                    if (cell.Text.Contains("���I��"))
                    {
                        colNumber[2] = cnt;
                    }
                    //�R�����g
                    if (cell.Text.Contains("�R�����g"))
                    {
                        colNumber[3] = cnt;
                    }
                }
                cnt++;
            }

            for (int i = 0; i < colNumber.Length; i++)
            {
                if (colNumber[i] == -1)
                {
                    ModelState.AddModelError("importFile", "�w�b�_�s������������܂���B");
                    break;
                }
            }
                

            return colNumber;
        }

        /// <summary>
        /// Excel�̒��o�����s����A����R�[�h�A�Ώ۔N���̏����擾����B
        /// </summary>
        /// <param name="conditionLine">���������s�̃e�L�X�g</param>
        /// <param name="departmentCode">����R�[�h(out)</param>
        /// <param name="inventoryMonth">�Ώ۔N��(out)</param>
        /// <returns></returns>
        /// <history>
        ///  2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void ExtractInf(string conditionLine, ref string departmentCode, ref DateTime inventoryMonth)
        {

            //--------------------------------
            //�����ݒ�
            //--------------------------------
            string strInventoryMonth = "";      //�Ώ۔N��(Excel���͒l)z
            int pos = -1;                       //�Ώە�����̈ʒu

            //--------------------------------
            //���擾
            //--------------------------------

            if (!string.IsNullOrEmpty(conditionLine))
            {

                //����R�[�h�擾
                pos = conditionLine.IndexOf("����=");

                if ((pos >= 0) && (pos + (3 + 3) <= conditionLine.Length))
                {
                    departmentCode = conditionLine.Substring(pos + 3, 3);
                }

                //�Ώ۔N���擾
                pos = conditionLine.IndexOf("�Ώ۔N��=");

                if ((pos >= 0) && (pos + (5 + 7) <= conditionLine.Length))
                {
                    strInventoryMonth = conditionLine.Substring(pos + 5, 7);
                }
            }

            //--------------------------------
            // ����R�[�h�`�F�b�N
            //--------------------------------
            //�}�X�^���݃`�F�b�N(����R�[�h���}�X�^�ɓo�^����Ă��Ȃ��ꍇ�G���[ ���󗓂��G���[)
            Department dep = new DepartmentDao(db).GetByKey(departmentCode);
            if (dep == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "����R�[�h���}�X�^�ɓo�^����Ă��܂���B����}�X�^�ɓo�^��A�ēx���s���Ă�������" }));
            }

            //--------------------------------
            // �q�ɃR�[�h�`�F�b�N
            //--------------------------------
            //�}�X�^���݃`�F�b�N(�Y�����傪�g�p����q�ɂ���`���Ă��邩���`�F�b�N����)
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

            if (dWarehouse == null)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "���͂������傪�g�p����q�ɂ��}�X�^�ɓo�^����Ă��܂���B����E�q�ɑg�����}�X�^�ɓo�^��A�Ď��s���Ă�������" }));
            }

            //--------------------------------
            // �Ώ۔N���`�F�b�N
            //--------------------------------
            //�t�H�[�}�b�g�`�F�b�N(�Ώ۔N����DateTime�^�ɕϊ��o���Ȃ��ꍇ�G���[ ���󗓂��G���[)
            bool result = DateTime.TryParse(strInventoryMonth + "/01", out inventoryMonth);
            if (result == false)
            {
                inventoryMonth = DateTime.Parse("1900/01/01");      //�K���Ȓl��ݒ肷��B
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "�Ώ۔N���̌`��������������܂���B�Ώ۔N����yyyy/MM�œ��͂��Ă�������" }));
            }

            else
            {
                string status = GetInventoryStatusByWarehouse( (dWarehouse != null ? dWarehouse.WarehouseCode : "") , inventoryMonth);
                //string status = GetInventoryStatusByDepartment(departmentCode, inventoryMonth);

                if (status == STS_UNEXCUTED) //�I�������{
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "�Ώی��̒I���������{�̂��߁A�f�[�^�̎�荞�݂��ł��܂���" }));
                }
                if (status == STS_DECIDED) //�I������
                {
                    ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "�Ώی��̒I�����I�����Ă��邽�߁A�f�[�^�̎�荞�݂��ł��܂���" }));
                }
            }

            return;
        }
		
        /// <summary>
        /// Excel�f�[�^�̍��ڃ`�F�b�N
        /// </summary>
        /// <param name="ws">���[�N�V�[�g</param>
        /// <param name="row">�s�ԍ�</param>
        /// <param name="locCol">��ԍ�[0�c���P�[�V�����R�[�h�A1�c���i�ԍ�,2�c���I, 3�c�R�����g]</param>
        /// <param name="data">Excel���͒l</param>
        /// <param name="sLocList">���P�[�V�����R�[�h���X�g(�X�V�p)</param>
        /// <param name="sLocList2">���P�[�V�����R�[�h���X�g(�`�F�b�N�p)</param>
        /// <param name="sPartsList">���i�ԍ����X�g(�X�V�p)</param>
        /// <param name="sPartsList2">���i�ԍ����X�g(�`�F�b�N�p)</param>
        /// <return>�����L�[([0]�c���P�[�V�����R�[�h�A[1]���i�ԍ�</return>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ������ύX(departmentCode �� warehouseCode)
        /// 2015/06/09 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�D ���i�ԍ��A���P�[�V�����R�[�h�̋󔒂���菜���Ă���`�F�b�N���s�� 
        /// 2015/05/14 arc yano IPO�Ή�(���i�I��) Excel�捞���V�X�e���G���[�̑Ή�(���P�[�V�����R�[�h�A���i�ԍ��`�F�b�N�p�̈����ǉ��A�����̓n������l�n������Q�Ɠn���֕ύX)
        /// </history>
        private string[] ValidateDataProperty(string[] array, ref PartsInventory data, ref List<string> sLocList, ref List<string> sLocList2, ref List<string> sPartsList, ref List<string> sPartsList2, string warehouseCode)
        //private string[] ValidateDataProperty(string[] array, ref PartsInventory data, ref List<string> sLocList, ref List<string> sLocList2, ref List<string> sPartsList, ref List<string> sPartsList2, string departmentCode)
        {
            //--------------------------
            //�����ݒ�
            //--------------------------
            decimal wkdata;

            data = new PartsInventory();

            string[] conditions = new string[] { "", "" };
            string chkString = "";

            //--------------------------
            //���͒l�`�F�b�N
            //--------------------------            

            //--------------------------
            //���P�[�V�����R�[�h
            //--------------------------
            //�}�X�^���݃`�F�b�N
            chkString = Strings.StrConv(array[0], VbStrConv.Narrow, 0x0411).ToUpper().Trim();      //Mod 2015/06/09

            //�啶���Ō���
            int locIndex = sLocList2.IndexOf(chkString);
            if (locIndex < 0)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "�q�ɃR�[�h:" + warehouseCode + "�ɑ΂��āA���P�[�V�����R�[�h:" + array[0] + "�̓}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^��Ɏ��s���Ă�������" }));
                //ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "����R�[�h:" + departmentCode + "�ɑ΂��āA���P�[�V�����R�[�h:" + array[0] + "�̓}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^��Ɏ��s���Ă�������" }));
            }
            else
            {
                data.LocationCode = sLocList[locIndex];�@//���P�[�V�����R�[�h��Ҕ�(�}�X�^�̃��P�[�V�����R�[�h��Ҕ�)
                conditions[0] = sLocList2[locIndex];
            }
                
            //--------------------------
            //���i�ԍ�
            //--------------------------
            chkString = Strings.StrConv(array[1], VbStrConv.Narrow, 0x0411).ToUpper().Trim();      //Mod 2015/06/09
                
            int partsIndex = sPartsList2.IndexOf(chkString);
            
            if (partsIndex < 0)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "���i�ԍ�:" + array[1] + "�̓}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^��Ɏ��s���Ă�������" }));
            }
            else
            {
                data.PartsNumber = sPartsList[partsIndex];�@//���i�ԍ���Ҕ�(�}�X�^�̕��i�ԍ���Ҕ�)
                conditions[1] = sPartsList2[partsIndex];
            }

            //--------------------------
            //���I��
            //--------------------------
            //�t�H�[�}�b�g�`�F�b�N
            if (string.IsNullOrWhiteSpace(array[2]) || !(Regex.IsMatch(array[2], @"^\d{1,7}$") || Regex.IsMatch(array[2], @"^\d{1,7}\.\d{1,2}$")) || false == decimal.TryParse(array[2], out wkdata))
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "���P�[�V�����R�[�h:" + data.LocationCode + "�A" + "���i�ԍ�:" + data.PartsNumber + "�̎��I���͐��̐�[7���ȓ��̐����A�܂��͏���(����7���ȓ��A����2���ȓ�)]�œ��͂��Ă�������" }));
            }
            else
            {
                data.PhysicalQuantity = wkdata;
            }
                
            //--------------------------
            //�R�����g
            //--------------------------
             //�����O�X�`�F�b�N
               
            if (array[3].Length > 255)
            {
                ModelState.AddModelError("importFile", MessageUtils.GetMessage("E0024", new string[] { "���P�[�V�����R�[�h:" + data.LocationCode + "�A" + "���i�ԍ�:" + data.PartsNumber + "�̃R�����g�͑S�p�A�܂��͔��p255�����ȓ��œ��͂��Ă�������" }));
            }
            else
            {
                data.Comment = array[3];
            }
                
            return conditions;
        }

        /// <summary>
        /// Excel���͒l��DB�ւ̐ݒ�
        /// </summary>
        /// <param name="data">Excel���͒l</param>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <param name="conditions">��������([0]�c���P�[�V�����R�[�h [1]�c���i�ԍ�)</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void SetData(PartsInventory data, string warehouseCode, DateTime inventoryMonth, List<InventoryStock> isList, string[] conditions)
        //private void SetData(PartsInventory data, string departmentCode, DateTime inventoryMonth, List<InventoryStock> isList, string[] conditions)
        {
            // ���i�I�����R�[�h�̑��݃`�F�b�N

            InventoryStock ivStock = isList.Where(x => x.LocationCode.Equals(conditions[0]) && x.PartsNumber.Equals(conditions[1])).FirstOrDefault();
            //InventoryStock ivStock = new InventoryStockDao(db).GetByLocParts(inventoryMonth, departmentCode, data.LocationCode, data.PartsNumber);

            if (ivStock != null)
            {
                // ���݂����ꍇ�A�X�V�������s��
                ivStock = EditivStockData(ivStock, data, warehouseCode, inventoryMonth, true);      //Mod 2016/08/13 arc yano #3596
            }
            else // ���݂��Ȃ������ꍇ�A�o�^�������s��
            {
                //InventoryStock�ւ̓o�^
                ivStock = new InventoryStock();
                ivStock = EditivStockData(ivStock, data, warehouseCode, inventoryMonth, false);     //Mos 2016/08/13 arc yano #3596
                db.InventoryStock.InsertOnSubmit(ivStock);
            }

            return;
        }

        /// <summary>
        /// �Ǎ���Excel�f�[�^��InventoryStock�p�ɕҏW����
        /// </summary>
        /// <param name="ivstock">���i�݌ɒI��</param>
        /// <param name="data">Excel�f�[�^</param>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="targetDate">�Ώ۔N��</param>
        /// <param name="updateflag">true:�X�V false:�V�K�ǉ�</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private InventoryStock EditivStockData(InventoryStock ivstock, PartsInventory data, string warehouseCode, DateTime inventoryMonth, Boolean updateflag)
        //private InventoryStock EditivStockData(InventoryStock ivstock, PartsInventory data, string departmentCode, DateTime inventoryMonth, Boolean updateflag)
        {
            int update = 0;
            
            //���I����DB��Excel�ňقȂ�ꍇ�̂ݍX�V
            if (ivstock.PhysicalQuantity != data.PhysicalQuantity)
            {
                ivstock.PhysicalQuantity = data.PhysicalQuantity;

                update = 1;
            }
            //�R�����g
            if (!string.IsNullOrWhiteSpace(ivstock.Comment) || (!string.IsNullOrWhiteSpace(data.Comment)))
            {
                //DB�AExcel���ɒl��0��󕶎��łȂ��ꍇ�̂ݔ�r����B
                if (!string.IsNullOrWhiteSpace(ivstock.Comment) && !string.IsNullOrWhiteSpace(data.Comment))
                {
                    if (!data.Comment.Equals(ivstock.Comment))
                    {
                        ivstock.Comment = data.Comment;

                        update = 1;
                    }
                }
                else
                {
                    ivstock.Comment = data.Comment;
                    update = 1;
                }
            }

            if (update == 1)
            {
                //�ŏI�X�V��
                ivstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                //�ŏI�X�V��
                ivstock.LastUpdateDate = DateTime.Now;
            }

            //�V�K�쐬�̏ꍇ�͂��̑��̍��ڂ��ݒ肷��B
            if (updateflag == false)
            {
                //�I��ID
                ivstock.InventoryId = Guid.NewGuid();

                //����R�[�h ���󕶎��ɂ���
                ivstock.DepartmentCode = "";        //Mod 2016/08/13 arc yano #3596

                //�I����
                ivstock.InventoryMonth = inventoryMonth;
                
                //���P�[�V�����R�[�h
                ivstock.LocationCode = data.LocationCode;

                //�Ј��R�[�h
                ivstock.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                
                //�I���^�C�v
                ivstock.InventoryType = TYP_PARTS;

                //�Ǘ��ԍ�
                ivstock.SalesCarNumber = null;

                //���i�ԍ�
                ivstock.PartsNumber = data.PartsNumber;

                //����(�V�K���ڂ̏ꍇ�͎��I���Ɠ���)
                ivstock.Quantity = data.PhysicalQuantity;
                
                //�쐬��
                ivstock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                
                //�쐬��
                ivstock.CreateDate = DateTime.Now;

                //�폜�t���O
                ivstock.DelFlag= "0";

                //�T�}���[
                ivstock.Summary = null;

                //�q�ɃR�[�h
                ivstock.WarehouseCode = warehouseCode;      //Add Mod 2016/08/13 arc yano #3596
            }

            return ivstock;
        }

        /// <summary>
        /// �Ǎ���Excel�f�[�^��PartsStock�p�ɕҏW����
        /// </summary>
        /// <param name="ptStock">���i�݌�</param>
        /// <param name="impData">Exces�f�[�^</param>
        /// <returns></returns>
        private PartsStock EditptStockData(PartsStock ptStock, PartsInventory data)
        {
            //���i�ԍ�
            ptStock.PartsNumber = data.PartsNumber;

            //���P�[�V�����R�[�h
            ptStock.LocationCode = data.LocationCode;

            //����
            ptStock.Quantity = data.PhysicalQuantity;

            //�쐬��
            ptStock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            
            //�쐬��
            ptStock.CreateDate = DateTime.Now;

            //�ŏI�X�V��
            ptStock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //�ŏI�X�V��
            ptStock.LastUpdateDate = DateTime.Now;

            //�폜�t���O
            ptStock.DelFlag = "0";

            return ptStock;
        }

        /// <summary>
        /// �_�u���R�[�e�[�V�����̔r������
        /// </summary>
        /// <param name="quoteData"></param>
        /// <returns></returns>
        private string[] EditExcelQuoteData(string[] quoteData)
        {
            string[] splLine2 = new string[quoteData.Count()];
            ArrayList array2 = new ArrayList();
            string splData = "";

            // ArrayList�Ɋi�[
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
		
		/*
        /// <summary>
        /// ���P�[�V�����R�[�h�A���i�ԍ��d�����R�[�h�̃`�F�b�N����
        /// </summary>
        /// <param name="locationCode">���P�[�V�����R�[�h</param>
        /// <param name="partsNumber">���i�ԍ�</param>
        /// <param name="chkstr">���P�[�V�����R�[�h�{���i�ԍ��̕�����</param>
        /// <returns></returns>
        private string ValidationOverLapData(PartsInventory data, string chkstr)
        {
         
            //�����ݒ�
            int index = -1;

            //string str = "";

            //���P�[�V�����R�[�h�A���i�ԍ������ɗL���ȏꍇ�ɂ̂݃`�F�b�N����B�������̏ꍇ�͑���validation�G���[�ŕ\��
            if (!string.IsNullOrWhiteSpace(data.LocationCode) && !string.IsNullOrWhiteSpace(data.PartsNumber))
            {
                // ���P�[�V�����R�[�h�{���i�ԍ��̓���g�ݍ��킹�����݂��邩�`�F�b�N
                index = chkstr.IndexOf(data.LocationCode + ":" + data.PartsNumber);

                if (index >= 0)
                {
                    ModelState.AddModelError("", "���P�[�V�����R�[�h:" + data.LocationCode + "�A ���i�ԍ�:" + data.PartsNumber + "�̃f�[�^���d�����Ă��܂��B");
                }
                chkstr += (data.LocationCode + ":" + data.PartsNumber) + " ";
            }

            return chkstr;
        }
        */

        // Add 2014/12/08 arc yano IPO�Ή�(���i�I��)
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

        #region ���i�݌ɍs�ǉ��E�폜
        /// <summary>
        /// ���i�݌ɍs�ǉ�
        /// </summary>
        /// <param name="form">�t�H�[��</param>
        /// <param name="line">���i�݌ɖ���</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�I���i�I���݌ɂ̏C���j �V�K�쐬
        /// </history>
        private ActionResult AddLine(FormCollection form, EntitySet<PartsInventory> line)
        {
            if (line == null)
            {
                line = new EntitySet<PartsInventory>();
            }

            PartsInventory rec = new PartsInventory();

            //���ʁA�����ϐ��̏�����
            rec.PhysicalQuantity = 0;
            rec.ProvisionQuantity = 0;

            rec.NewRecFlag = true;

            line.Add(rec);

            form["EditFlag"] = "true";

            SetComponentDataEdit(form);

            return View("PartsInventoryDataEdit", line.ToList());
        }

        /// <summary>
        /// ���i�݌Ƀf�[�^�s�폜
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">���׃f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�I���i�I���݌ɂ̏C���j �V�K�쐬
        /// </history>
        private ActionResult DelLine(FormCollection form, EntitySet<PartsInventory> line)
        {
            ModelState.Clear();

            DateTime inventoryMonth = DateTime.Parse(form["inventoryMonth"]);

            int targetId = int.Parse(form["DelLine"]);

            using (TransactionScope ts = new TransactionScope())
            {
                if (line[targetId].NewRecFlag.Equals(false))
                {
                    InventoryStock rec = new InventoryStockDao(db).GetByLocParts(inventoryMonth, line[targetId].LocationCode, line[targetId].PartsNumber, false);

                    if (rec != null)
                    {
                        rec.DelFlag = "1";                                                          //�폜�t���O�𗧂Ă�
                        rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        rec.LastUpdateDate = DateTime.Now;
                    }
                }
                try
                {
                    //�G���e�B�e�B�̍폜����
                    line.RemoveAt(targetId);

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(se, PROC_NAME_DELLINE, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_DELLINE, FORM_NAME, "");
                    ts.Dispose();
                }
            }

            SetComponentDataEdit(form);

            return View("PartsInventoryDataEdit", line.ToList());
        }

        #endregion

         #region �R���g���[���̗L������
        /// <summary>
        /// �R���g���[���̗L��������Ԃ�Ԃ��B
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 ���i�I���i�I���݌ɂ̏C���j �V�K�쐬�{�^���\���E��\���t���O�̏����̒ǉ�
        /// </history>
        private void GetViewResult(FormCollection form)
        {
            Employee loginUser = (Employee)Session["Employee"];
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            //�Z�L�����e�B���x����4�łȂ����[�U�́A�{�^���������s�ɂ���B
            if (!securityLevel.Equals("004"))
            {
                ViewData["ButtonEnabled"] = false;
            }
            else
            {
                ViewData["ButtonEnabled"] = true;
            }

            //Add 2017/07/18 arc yano #3781
            //�f�[�^�ҏW�{�^���\���E��\�������@�����̂��郆�[�U�̂݁i����̓V�X�e���ۃ��[�U�j�\��
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "InventoryDataEdit");

            if (rec != null && rec.EnableFlag.Equals(true))
            {
                ViewData["DataEditButtonVisible"] = true;
            }
            else
            {
                ViewData["DataEditButtonVisible"] = false;
            }
            
            return;
        }
        #endregion

       /// <summary>
       /// ���������ǂ������擾����B(Ajax��p�j
       /// </summary>
       /// <param name="processType">�������</param>
       /// <returns>�A�C�h�����O���</returns>
       public ActionResult GetProcessed(string processType)
       {
           if (Request.IsAjaxRequest())
           {
               Dictionary<string, string> retParts = new Dictionary<string, string>();
                
               retParts.Add("ProcessedTime", "�A�C�h�����O���c");
                
               return Json(retParts);
           }
           return new EmptyResult();
       }

       /// <summary>
       /// ����R�[�h�A�Ώ۔N������I���J�n�������擾����(Ajax��p�j
       /// </summary>
       /// <param name="departmentCode">����R�[�h</param>
       /// <param name="inventoryMonth">�Ώ۔N��</param>
       /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
       /// <history>
       /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� InventoryScheduleParts��Department�̊֘A�t�̔p�~ �q�ɃR�[�h����I���J�n�����擾����
       /// 2015/06/17 arc yano IPO�Ή�(���i�I��) ��Q�Ή��A�d�l�ύX�E �I����Ɠ��ł͂Ȃ��A�I���J�n������\������
       /// </history>
       public ActionResult GetStartDate(string warehouseCode, string inventoryMonth)
       //public ActionResult GetStartDate(string departmentCode, string inventoryMonth)
       {
           if (Request.IsAjaxRequest())
           {
               InventoryScheduleParts condition = new InventoryScheduleParts();
               //Mod 2016/08/13 arc yano #3596
               //condition.Department = new Department();
               //condition.Department.DepartmentCode = departmentCode;
               condition.WarehouseCode = warehouseCode;
               condition.InventoryMonth = DateTime.Parse(inventoryMonth + "/01");

               CodeData codeData = new CodeData();
               InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);
               if (rec != null)
               {
                   codeData.Code = "";                          //�Ƃ肠���������ݒ肵�Ȃ�
                   codeData.Name = rec.StartDate == null ? "": string.Format("{0:yyyy/MM/dd HH:mm:ss}", rec.StartDate);
               }
               return Json(codeData);
           }
           return new EmptyResult();
       }
        
    }
}
