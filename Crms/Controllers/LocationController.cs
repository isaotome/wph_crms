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
    /// ���P�[�V�����}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class LocationController : Controller
    {
        //Add 2014/08/05 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���P�[�V�����}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "���P�[�V�����}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public LocationController()
        {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ���P�[�V����������ʕ\��
        /// </summary>
        /// <returns>���P�[�V�����������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ���P�[�V����������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���P�[�V�����������</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 ���������ɑq�ɃR�[�h�A�q�ɖ���ǉ�
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            // �������ʃ��X�g�̎擾
            PaginatedList<Location> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];
            ViewData["DelFlag"] = form["DelFlag"];

            ViewData["WarehouseCode"] = form["WarehouseCode"];    //Add 2016/08/13 arc yano #3596
            ViewData["WarehouseName"] = form["WarehouseName"];    //Add 2016/08/13 arc yano #3596

            // ���P�[�V����������ʂ̕\��
            return View("LocationCriteria", list);
        }

        /// <summary>
        /// ���P�[�V���������_�C�A���O�\��
        /// </summary>
        /// <returns>���P�[�V���������_�C�A���O</returns>
        public ActionResult CriteriaDialog()
        {
            FormCollection form = new FormCollection();
            form["ConditionsHold"] = Request["ConditionsHold"];
            form["HoldBusinessType"] = Request["BusinessType"];
            form["BusinessType"] = Request["BusinessType"];

            return CriteriaDialog(form);
        }

        /// <summary>
        /// ���P�[�V���������_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���P�[�V����������ʃ_�C�A���O</returns>
        /// <history>
        /// 2017/07/27 arc yano #3781 ���i�݌ɒI���i�I���݌ɂ̏C���j�q�ɖ������͂���Ă��Ȃ��ꍇ�̓}�X�^����擾����
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �V�K�쐬 ���������ɑq�ɃR�[�h�A�q�ɖ���ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["DepartmentCode"] = Request["DepartmentCode"];
            form["DepartmentName"] = Request["DepartmentName"];
            form["LocationCode"] = Request["LocationCode"];
            form["LocationName"] = Request["LocationName"];
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);            
            form["LocationType"] = Request["LocationType"];

            form["WarehouseCode"] = Request["WarehouseCode"]; //Add 2016/08/13 arc yano #3596
            form["WarehouseName"] = Request["WarehouseName"]; //Add 2016/08/13 arc yano #3596

            if (!string.IsNullOrWhiteSpace(form["WarehouseCode"]) && !form["WarehouseCode"].Equals("undefined") && string.IsNullOrWhiteSpace(form["WarehouseName"]))
            {
                Warehouse warehouse = new WarehouseDao(db).GetByKey(form["WarehouseCode"]);

                form["WarehouseName"] = warehouse != null ? warehouse.WarehouseName : "";
            }

            //Add 2016/08/13 arc yano #3596
            //�q�ɃR�[�h�A�q�ɖ����ɖ����͂������ꍇ
            if (string.IsNullOrWhiteSpace(form["WarehouseCode"]) && string.IsNullOrWhiteSpace(form["WarehouseName"]))
            {
                //�q�ɏ��̎擾
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, form["DepartmentCode"]);

                form["WarehouseCode"] = (dWarehouse != null ? dWarehouse.WarehouseCode : "");
                form["WarehouseName"] = (dWarehouse != null ? dWarehouse.Warehouse.WarehouseName : "");
            }

            if (form["ConditionsHold"] != null && form["ConditionsHold"].ToString().Equals("1"))
            {
                form["BusinessType"] = form["HoldBusinessType"];
            }
            else
            {
                if (!string.IsNullOrEmpty(form["DepartmentCode"]) || !string.IsNullOrEmpty(form["DepartmentName"]) || !string.IsNullOrEmpty(form["WarehouseCode"]) || !string.IsNullOrEmpty(form["WarehouseName"]) || !string.IsNullOrEmpty(form["LocationCode"]) || !string.IsNullOrEmpty(form["LocationName"]))
                {
                    form["BusinessType"] = "";
                }
                else
                {
                    form["BusinessType"] = form["HoldBusinessType"];
                }
            }


            // �������ʃ��X�g�̎擾
            PaginatedList<Location> list = GetSearchResultList(form);

            // ���̑��o�͍��ڂ̐ݒ�
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["DepartmentName"] = form["DepartmentName"];
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];

            //Add 2017/02/03 arc nakayama #3594_���i�ړ����́@�o�ɁE���Ƀ��P�[�V�����̍i���݇A
            ViewData["ConditionsHold"] = form["ConditionsHold"];
            ViewData["BusinessType"] = form["BusinessType"];
            ViewData["HoldBusinessType"] = form["HoldBusinessType"];

            ViewData["LocationType"] = form["Locationtype"];

            //Add 2016/08/13 arc yano #3596
            //�q�ɏ��̐ݒ�
            ViewData["WarehouseCode"] = form["WarehouseCode"];
            ViewData["WarehouseName"] = form["WarehouseName"];

            // ���P�[�V����������ʂ̕\��
            return View("LocationCriteriaDialog", list);
        }

        //Add 2014/11/07 arc yano �ԗ��X�e�[�^�X�ύX�Ή��@�ԗ��X�e�[�^�X�ύX��ʐ�p�̌����_�C�A���O��ǉ�
        /// <summary>
        /// ���P�[�V���������_�C�A���O�\��(�ԗ��X�e�[�^�X�ύX��ʐ�p)
        /// </summary>
        /// <returns>���P�[�V���������_�C�A���O</returns>
        public ActionResult CriteriaDialogForCarUsage()
        {
            return CriteriaDialogForCarUsage(new FormCollection());
        }

        /// <summary>
        /// ���P�[�V���������_�C�A���O�\��(�ԗ��X�e�[�^�X�ύX��ʐ�p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���P�[�V����������ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialogForCarUsage(FormCollection form)
        {
            // ���������̐ݒ�
            // (�N�G���X�g�����O�����������Ɏg�p����ׁARequest���g�p�B
            //  �Ȃ��t�H�[�����g�p���ꂽ�ꍇ�ARequest�ɂ̓t�H�[���̒l���i�[����Ă���B)
            form["LocationCode"] = Request["LocationCode"];
            form["LocationName"] = Request["LocationName"];

            // �������ʃ��X�g�̎擾
            PaginatedList<V_LocationListForCarUsage> list = GetSearchResultListForCarUsage(form);

            //���̑��̍��ڂ̐ݒ�
            ViewData["LocationCode"] = form["LocationCode"];
            ViewData["LocationName"] = form["LocationName"];

            // ���P�[�V����������ʂ̕\��
            return View("LocationCriteriaDialogForCarUsage", list);
        }


        /// <summary>
        /// ���P�[�V�����}�X�^���͉�ʕ\��
        /// </summary>
        /// <param name="id">���P�[�V�����R�[�h(�X�V���̂ݐݒ�)</param>
        /// <returns>���P�[�V�����}�X�^���͉��</returns>
        [AuthFilter]
        public ActionResult Entry(string id)
        {
            Location location;

            // �ǉ��̏ꍇ
            if (string.IsNullOrEmpty(id))
            {
                ViewData["update"] = "0";
                location = new Location();
            }
            // �X�V�̏ꍇ
            else
            {
                ViewData["update"] = "1";
                //Mod 2015/07/06 arc nakayama ���P�[�V�����̖����f�[�^�̍X�V�ŗ����邽�ߏC���@�X�V�͖����f�[�^���܂ނ悤�ɂ���
                location = new LocationDao(db).GetByKey(id, true);
            }

            // ���̑��\���f�[�^�̎擾
            GetEntryViewData(location);

            // �o��
            return View("LocationEntry", location);
        }

        /// <summary>
        /// ���P�[�V�����}�X�^�ǉ��X�V
        /// </summary>
        /// <param name="location">���f���f�[�^(�o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���P�[�V�����}�X�^���͉��</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Location location, FormCollection form)
        {
            // �p���ێ�����o�͏��̐ݒ�
            ViewData["update"] = form["update"];

            // �f�[�^�`�F�b�N
            ValidateLocation(location);
            if (!ModelState.IsValid)
            {
                GetEntryViewData(location);
                return View("LocationEntry", location);
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            // �f�[�^�X�V����
            if (form["update"].Equals("1"))
            {
                // �f�[�^�ҏW�E�X�V
                //Mod 2015/07/06 arc nakayama ���P�[�V�����̖����f�[�^�̍X�V�ŗ����邽�ߏC���@�X�V�͖����f�[�^���܂ނ悤�ɂ���
                Location targetLocation = new LocationDao(db).GetByKey(location.LocationCode, true);
                UpdateModel(targetLocation);
                EditLocationForUpdate(targetLocation);
            }
            // �f�[�^�ǉ�����
            else
            {
                // �f�[�^�ҏW
                location = EditLocationForInsert(location);

                // �f�[�^�ǉ�
                db.Location.InsertOnSubmit(location);                
            }

            // Add 2014/08/05 arc amii �G���[���O�Ή� submitChange����{�� + �G���[���O�o��(�X�V�ƒǉ��œ����Ă���SubmitChanges�𓝍�)
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
                        ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0010", new string[] { "���P�[�V�����R�[�h", "�ۑ�" }));
                        GetEntryViewData(location);
                        return View("LocationEntry", location);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error"); ;
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
            //MOD 2014/10/28 ishii �ۑ��{�^���Ή�
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            // �o��
            //ViewData["close"] = "1";
            //return Entry((string)null);
            return Entry(location.LocationCode);

        }

        /// <summary>
        /// ���P�[�V�����R�[�h���烍�P�[�V���������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���P�[�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2021/08/03 yano #4098 �y���P�[�V�����}�X�^�z���P�[�V�����𖳌��ɕύX�������̃`�F�b�N�����̒ǉ�
        /// 2016/04/26 arc yano #3510 ���i���ד��́@���׃��P�[�V�����̍i���� ���P�[�V������ʂ��擾���鏈����ǉ�
        /// </history>
        public ActionResult GetMaster(string code, bool includeDeleted = false)
        {
            string businessType = Request["BusinessType"];
            string locationType = Request["LocationType"];


            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                Location location = new LocationDao(db).GetByBusinessType(code, businessType, locationType, includeDeleted);    //Mod 2021/08/03 yano #4098
                if (location != null)
                {
                    codeData.Code = location.LocationCode;
                    codeData.Name = location.LocationName;
                    codeData.Code2 = location.LocationType;     //Add 2016/04/26 #3510
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        //Add 2014/11/11 arc yano �ԗ��X�e�[�^�X�ύX�Ή�
        /// <summary>
        /// ���P�[�V�����R�[�h���烍�P�[�V���������擾����(Ajax��p�j���ԗ��X�e�[�^�X���͉�ʐ�p
        /// </summary>
        /// <param name="code">���P�[�V�����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMasterForCarUsage(string code)
        {
            string businessType = Request["BusinessType"];
            string locationType = Request["LocationType"];


            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                V_LocationListForCarUsage location = new LocationDao(db).GetByKeyForCarUsage(code);
                if (location != null)
                {
                    codeData.Code = location.LocationCode;
                    codeData.Name = location.LocationName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ��ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="location">���f���f�[�^</param>
        /// <history>
        /// 2016/08/13 arc yano #3596�y�區�ځz����I�����Ή� �o�^���ڂ𕔖傩��q�ɂɕύX
        /// </history>
        private void GetEntryViewData(Location location)
        {
            //Mod 2016/08/13 arc yano #3596
            //�q�ɖ��̎擾
            if (!string.IsNullOrEmpty(location.WarehouseCode))
            {
                WarehouseDao warehouseDao = new WarehouseDao(db);
                Warehouse warehouse = warehouseDao.GetByKey(location.WarehouseCode);
                if (warehouse != null)
                {
                    ViewData["WarehouseName"] = warehouse.WarehouseName;
                }
            }
            /*
            // ���喼�̎擾
            if (!string.IsNullOrEmpty(location.DepartmentCode))
            {
                DepartmentDao departmentDao = new DepartmentDao(db);
                Department department = departmentDao.GetByKey(location.DepartmentCode);
                if (department != null)
                {
                    ViewData["DepartmentName"] = department.DepartmentName;
                }
            }
            */
            CodeDao dao = new CodeDao(db);
            ViewData["LocationTypeList"] = CodeUtils.GetSelectListByModel<c_LocationType>(dao.GetLocationTypeAll(false), location.LocationType,false);
        }

        /// <summary>
        /// ���P�[�V�����}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���P�[�V�����}�X�^�������ʃ��X�g</returns>
        /// <history>
        /// 2016/08/17 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� 
        ///                                       �@���������ɑq�ɂ�ǉ�
        ///                                       �A���������̐ݒ���@�̕ύX
        /// </history>
        private PaginatedList<Location> GetSearchResultList(FormCollection form)
        {
            LocationDao locationDao = new LocationDao(db);
            Location locationCondition = new Location();
            locationCondition.LocationCode = form["LocationCode"];
            locationCondition.LocationName = form["LocationName"];
            locationCondition.DepartmentCode = form["DepartmentCode"];
            locationCondition.DepartmentName = form["DepartmentName"];
            locationCondition.BusinessType = form["BusinessType"];
            locationCondition.LocationType = form["Locationtype"];
            locationCondition.WarehouseCode = form["WarehouseCode"];
            locationCondition.WarehouseName = form["WarehouseName"];

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))
            {
                locationCondition.DelFlag = form["DelFlag"];
            }

            /*
            locationCondition.Department = new Department();
            locationCondition.Department.DepartmentCode = form["DepartmentCode"];
            locationCondition.Department.DepartmentName = form["DepartmentName"];
            locationCondition.Department.BusinessType = form["BusinessType"];
            */
            
            return locationDao.GetListByCondition(locationCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        //Add 2014/11/07 arc yano  �ԗ��X�e�[�^�X�ύX�Ή� �ԗ��X�e�[�^�X�ύX��ʐ�p�̃��P�[�V�����ꗗ�擾
        /// <summary>
        /// ���P�[�V�����}�X�^�������ʃ��X�g�擾(�ԗ��X�e�[�^�X�ύX��p)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���P�[�V�����}�X�^�������ʃ��X�g</returns>
        private PaginatedList<V_LocationListForCarUsage> GetSearchResultListForCarUsage(FormCollection form)
        {
            LocationDao locationDao = new LocationDao(db);
            V_LocationListForCarUsage locationlist = new V_LocationListForCarUsage();
            locationlist.LocationCode = form["LocationCode"];
            locationlist.LocationName = form["LocationName"];

            return locationDao.GetListForCarUsageByCondition(locationlist, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }


        /// <summary>
        /// ���̓`�F�b�N
        /// </summary>
        /// <param name="location">���P�[�V�����f�[�^</param>
        /// <returns>���P�[�V�����f�[�^</returns>
        /// <history>
        /// 2021/08/03 yano #4098 �y���P�[�V�����}�X�^�z���P�[�V�����𖳌��ɕύX�������̃`�F�b�N�����̒ǉ�
        /// 2016/08/13 arc yano #3596�y�區�ځz����I�����Ή� �o�^���ڂ𕔖偨�q�ɂɕύX
        /// 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
        /// </history>
        private Location ValidateLocation(Location location)
        {
            // �K�{�`�F�b�N
            if (string.IsNullOrEmpty(location.LocationCode))
            {
                ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0001", "���P�[�V�����R�[�h"));
            }
            if (string.IsNullOrEmpty(location.LocationName))
            {
                ModelState.AddModelError("LocationName", MessageUtils.GetMessage("E0001", "���P�[�V������"));
            }
            //Mod 2016/08/13 arc yano #3596
            if (string.IsNullOrEmpty(location.WarehouseCode))
            {
                ModelState.AddModelError("WarehouseCode", MessageUtils.GetMessage("E0001", "�q�ɃR�[�h"));
            }
            /*
            if (string.IsNullOrEmpty(location.DepartmentCode))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����R�[�h"));
            }
            */
            // �t�H�[�}�b�g�`�F�b�N
            if (ModelState.IsValidField("LocationCode") && !CommonUtils.IsAlphaNumericBar(location.LocationCode))
            {
                ModelState.AddModelError("LocationCode", MessageUtils.GetMessage("E0020", "���P�[�V�����R�[�h"));
            }

            //Mod 2014/08/14 arc amii �G���[���O�Ή� 
            // ���P�[�V������ʂ��i�����A�d�|�j�̏ꍇ�A���ꕔ����ɕ����ݒ�ł��Ȃ�
            if (!CommonUtils.DefaultString(location.LocationType).Equals("001"))
            {
                //Mod 2016/08/13 arc yano #3596
                //List<Location> locationList = new LocationDao(db).GetListByLocationType(location.LocationType, location.DepartmentCode, location.LocationCode);
                List<Location> locationList = new LocationDao(db).GetListByLocationType(location.LocationType, location.WarehouseCode, location.LocationCode);
                if (locationList.Count > 0) {
                    ModelState.AddModelError("LocationType", "����q�ɓ��ɓ���ȃ��P�[�V����(�d�|�A���̑�)�͕����ݒ�ł��܂���");
                }
            }

            //Add 2021/08/03 yano #4098
            //db���猻�݂̗L���E������Ԃ��擾
            Location dblocaiont = new LocationDao(db).GetByKey(location.LocationCode, true);
            //�L�����疳���ɕύX�����ꍇ
            if(location.DelFlag.Equals("1") && !location.DelFlag.Equals(dblocaiont.DelFlag))
            {
                //�Ώۂ̃��P�[�V�����̕��i�݌ɗL�����`�F�b�N
                if (new PartsStockDao(db).getPresencePartsStock(location.LocationCode))
                {
                     ModelState.AddModelError("DelFlag", "�Ώۃ��P�[�V�����ɕ��i�݌ɂ����݂��邽�߁A�����ɂł��܂���");
                }
            }

            return location;
        }

        /// <summary>
        /// ���P�[�V�����}�X�^�ǉ��f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="location">���P�[�V�����f�[�^(�o�^���e)</param>
        /// <returns>���P�[�V�����}�X�^���f���N���X</returns>
        private Location EditLocationForInsert(Location location)
        {
            location.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.CreateDate = DateTime.Now;
            location.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.LastUpdateDate = DateTime.Now;
            location.DelFlag = "0";
            return location;
        }

        /// <summary>
        /// ���P�[�V�����}�X�^�X�V�f�[�^�ҏW(�t���[�����[�N�O�̕⊮�ҏW)
        /// </summary>
        /// <param name="location">���P�[�V�����f�[�^(�o�^���e)</param>
        /// <returns>���P�[�V�����}�X�^���f���N���X</returns>
        private Location EditLocationForUpdate(Location location)
        {
            location.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            location.LastUpdateDate = DateTime.Now;
            return location;
        }

    }
}
