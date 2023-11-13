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
using Crms.Models;                      //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���i���P�[�V�����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsLocationController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���i���P�[�V�����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���i���P�[�V�����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsLocationController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���i���P�[�V����������ʕ\��
        /// </summary>
        /// <returns>���i���P�[�V�����������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���i���P�[�V����������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i���P�[�V�����������</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���������Ɂu�q�Ɂv�ǉ�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<PartsLocation> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["PartsNumber"] = form["PartsNumber"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                PartsDao partsDao = new PartsDao(db);
                Parts parts = partsDao.GetByKey(form["PartsNumber"]);
                if (parts != null)
                {
                    ViewData["PartsNameJp"] = parts.PartsNameJp;
                }
            }
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(form["DepartmentCode"]);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            if (!string.IsNullOrEmpty(form["LocationCode"])) {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(form["LocationCode"]);
                if (location != null) {
                    ViewData["LocationName"] = location.LocationName;
                }
            }

            //Add 2016/08/13 arc yano #3596
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            if (!string.IsNullOrEmpty(form["WarehouseCode"]))
            {
                WarehouseDao WarehouseDao = new WarehouseDao(db);
                Warehouse warehouse = WarehouseDao.GetByKey(form["WarehouseCode"]);
                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }

            // ���i���P�[�V����������ʂ̕\��
            return View("PartsLocationCriteria", list);
        }

        /// <summary>
        /// ���i���P�[�V�����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���i���P�[�V�����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���i���P�[�V�����}�X�^���͉��</returns>
        /// <history>
        /// 2017/01/19 arc yano #3694  ���i���P�[�V�����@�V�K�쐬�\�����̃V�X�e���G���[
        /// �@�@�@�@�@�@�@�@�@�@�@�@�@������ʂŕ���R�[�h�܂��͑q�ɃR�[�h�����͂���Ă��Ȃ��ꍇ�͋󗓂ŕ\������@
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �o�^���ڂ̒ǉ�(�q�ɃR�[�h)
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string Status, string PartsNumber, string DepartmentCode = "", string WarehouseCode = "")
        {
           
           // string[] idArr = CommonUtils.DefaultString(id, "0,,").Split(new string[] { "," }, StringSplitOptions.None);

            //Add 2017/01/19 arc yano #3694
            if (ModelState.IsValid)
            {
                ModelState.Clear();     
            }
            

            PartsLocation partsLocation;
            string warehouseCode = "";
            if (string.IsNullOrWhiteSpace(WarehouseCode))
            {
                DepartmentWarehouse dwhouse = CommonUtils.GetWarehouseFromDepartment(db, DepartmentCode);
                
                //Add 2017/01/19 arc yano #3694  
                if (dwhouse != null)
                {
                    warehouseCode = dwhouse.WarehouseCode;
                }   
            }
            else
            {
                warehouseCode = WarehouseCode;
            }

            // �ǉ��̏ꍇ
            if (Status.Equals("0"))
            {
                ViewData["update"] = "0";
                partsLocation = new PartsLocation();
                partsLocation.PartsNumber = PartsNumber;
                partsLocation.DepartmentCode = "";
                partsLocation.WarehouseCode = warehouseCode;
                ViewData["fixedParts"] = string.IsNullOrEmpty(PartsNumber) ? "0" : "1";

                //Mod 2017/01/19 arc yano #3694
                ViewData["fixedWhouse"] = string.IsNullOrEmpty(warehouseCode) ? "0" : "1";
                //ViewData["fixedDept"] = string.IsNullOrEmpty(DepartmentCode) ? "0" : "1";
            }
            else // �X�V�̏ꍇ
            {
                ViewData["update"] = "1";
                partsLocation = new PartsLocationDao(db).GetByKey(PartsNumber, warehouseCode, false);
                ViewData["fixedParts"] = "1";
                //Mod 2017/01/19 arc yano #3694
                ViewData["fixedWhouse"] = "1";
                //ViewData["fixedDept"] = "1";
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(partsLocation);

            // �o��
            return View("PartsLocationEntry", partsLocation);
        }

        /// <summary>
        /// ���i���P�[�V�����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="partsLocation">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���i���P�[�V�����}�X�^���͉��</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �����L�[�̕ύX(���偨�q��)
        /// 2014/10/30 ishii �X�e�[�^�X�������̏��̕ۑ��Ή�
        /// 2014/10/29 ishii �ۑ��{�^���Ή�
        /// 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PartsLocation partsLocation, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];
            ViewData["fixedParts"] = form["fixedParts"];
            ViewData["fixedDept"] = form["fixedDept"];

            // �f�[�^�`�F�b�N
            ValidatePartsLocation(partsLocation, form);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(partsLocation);
                return View("PartsLocationEntry", partsLocation);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή�
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //MOD 2014/10/30 ishii �X�e�[�^�X�������̏��̕ۑ��Ή�
                //PartsLocation targetPartsLocation = new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.DepartmentCode,false);
                PartsLocation targetPartsLocation = new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.WarehouseCode, false);   //2016/08/13 arc yano #3596
                UpdateModel(targetPartsLocation);
                EditPartsLocationForUpdate(targetPartsLocation);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                partsLocation = EditPartsLocationForInsert(partsLocation);

                // �f�[�^�ǉ�
                db.PartsLocation.InsertOnSubmit(partsLocation);
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

                        ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0010", new string[] { getErrorKeyItemName(form), "�ۑ�" }));
                        ModelState.AddModelError("WarehouseCode", ""); //Mod 2016/08/13 arc yano #3596
                        GetEntryViewData(partsLocation);
                        return View("PartsLocationEntry", partsLocation);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
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

            ModelState.Clear();
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //2016/08/13 arc yano #3596
            return Entry("1", partsLocation.PartsNumber, "",  partsLocation.WarehouseCode); 
            //return Entry("1," + partsLocation.PartsNumber + "," + partsLocation.DepartmentCode);  
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="partsLocation">���f���f�[�^</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �o�^���ڂ̕ύX(����R�[�h���q�ɃR�[�h)
        /// </history>
        private void GetEntryViewData(PartsLocation partsLocation)
        {
            // ���i���̎擾
            if (!string.IsNullOrEmpty(partsLocation.PartsNumber))
            {
                PartsDao partsDao = new PartsDao(db);
                Parts parts = partsDao.GetByKey(partsLocation.PartsNumber);
                if (parts != null)
                {
                    ViewData["PartsNameJp"] = parts.PartsNameJp;
                }
            }

            // �q�ɖ��̎擾
            if (!string.IsNullOrEmpty(partsLocation.WarehouseCode))
            {
                WarehouseDao warehouseDao = new WarehouseDao(db);
                Warehouse warehouse = warehouseDao.GetByKey(partsLocation.WarehouseCode);

                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }

            /*
            // ���喼�̎擾
            if (!string.IsNullOrEmpty(partsLocation.DepartmentCode))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(partsLocation.DepartmentCode);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            */
            // ���P�[�V�������̎擾
            if (!string.IsNullOrEmpty(partsLocation.LocationCode))
            {
                LocationDao locationDao = new LocationDao(db);
                Location location = locationDao.GetByKey(partsLocation.LocationCode);
                if (location != null)
                {
                    ViewData["LocationName"] = location.LocationName;
                }
            }
        }

        /// <summary>
        /// ���i���P�[�V�����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���i���P�[�V�����}�X�^�������ʃ��X�g</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή�
        ///                            �@���������ɑq�ɂ�ǉ�
        ///                            �A���������̕���̐ݒ�̕ύX(�֘A�t���ɂ��A�N�Z�X�̔p�~)
        /// </history>
        private PaginatedList<PartsLocation> GetSearchResultList(FormCollection form)
        {
            PartsLocationDao partsLocationDao = new PartsLocationDao(db);
            PartsLocation partsLocationCondition = new PartsLocation();

            partsLocationCondition.Parts = new Parts();
            partsLocationCondition.Parts.PartsNumber = form["PartsNumber"];

            //Mod 2016/08/13 arc yano #3596
            //partsLocationCondition.Department = new Department();
            //partsLocationCondition.Department.DepartmentCode = form["DepartmentCode"];
            partsLocationCondition.DepartmentCode = form["DepartmentCode"];

            partsLocationCondition.LocationCode = form["LocationCode"];

            //Add 2016/08/13 arc yano #3596
            partsLocationCondition.WarehouseCode = form["WarehouseCode"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                partsLocationCondition.DelFlag = form["DelFlag"];
            }

            return partsLocationDao.GetListByCondition(partsLocationCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="partsLocation">���i���P�[�V�����f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���i���P�[�V�����f�[�^</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���͍��ڂ̕ύX(����R�[�h���q�ɃR�[�h)
        /// </history>
        private PartsLocation ValidatePartsLocation(PartsLocation partsLocation, FormCollection form)
        {
            // �K�{�`�F�b�N
            //���i�ԍ�
            if (string.IsNullOrEmpty(partsLocation.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0001", "���i"));
            }

            //Mod 2016/08/13 arc yano #3596
            //�q�ɃR�[�h
            if (string.IsNullOrEmpty(partsLocation.WarehouseCode))
            {
                ModelState.AddModelError("WarehouseCode", MessageUtils.GetMessage("E0001", "�q��"));
            }
            /*
            if (string.IsNullOrEmpty(partsLocation.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            */
            

            // �t�H�[�}�b�g�`�F�b�N
            //���i�ԍ�
            if (ModelState.IsValidField("PartsNumber") && !CommonUtils.IsAlphaNumeric(partsLocation.PartsNumber))
            {
                ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0012", "���i"));
            }

            //Mod 2016/08/13 arc yano #3596
            /*
            if (ModelState.IsValidField("DepartmentCode") && !CommonUtils.IsAlphaNumeric(partsLocation.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0012", "����"));
            }
            */

            // �d���`�F�b�N
            if (ModelState.IsValid && form["update"].Equals("0"))
            {
                //Mod 2016/08/13 arc yano #3596
                if (new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.WarehouseCode) != null)
                //if (new PartsLocationDao(db).GetByKey(partsLocation.PartsNumber, partsLocation.DepartmentCode) != null)
                {
                    ModelState.AddModelError("PartsNumber", MessageUtils.GetMessage("E0006", getErrorKeyItemName(form)));
                    ModelState.AddModelError("WarehouseCode", "");
                }
            }

            return partsLocation;
        }

        /// <summary>
        /// ���i���P�[�V�����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="partsLocation">���i���P�[�V�����f�[�^(�o�^���e)</param>
        /// <returns>���i���P�[�V�����}�X�^���f���N���X</returns>
        private PartsLocation EditPartsLocationForInsert(PartsLocation partsLocation)
        {
            partsLocation.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.CreateDate = DateTime.Now;
            partsLocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.LastUpdateDate = DateTime.Now;
            partsLocation.DelFlag = "0";
            return partsLocation;
        }

        /// <summary>
        /// ���i���P�[�V�����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="partsLocation">���i���P�[�V�����f�[�^(�o�^���e)</param>
        /// <returns>���i���P�[�V�����}�X�^���f���N���X</returns>
        private PartsLocation EditPartsLocationForUpdate(PartsLocation partsLocation)
        {
            partsLocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            partsLocation.LastUpdateDate = DateTime.Now;
            return partsLocation;
        }

        /// <summary>
        /// �G���[���b�Z�[�W�g�p�L�[���ږ��擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�G���[���b�Z�[�W�g�p�L�[���ږ�</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���͍��ڂ̕ύX(����R�[�h���q�ɃR�[�h)
        /// </history>
        private string getErrorKeyItemName(FormCollection form)
        {
            string itemName;
            if (form["fixedParts"].Equals("1"))
            {
                itemName = "���i";
            }
            else if (form["fixedWhouse"].Equals("1"))
            {
                itemName = "�q��";
            }
            else
            {
                itemName = "���i,�q��";
            }
            return itemName;
        }
    }
}
