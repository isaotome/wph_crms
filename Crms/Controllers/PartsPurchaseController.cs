using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Crms.Models;                      //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.Linq;                 //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

using OfficeOpenXml;
using System.Configuration;
using System.IO;
using System.Data;

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsPurchaseController : Controller
    {
        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���i����";     // ��ʖ�
        private static readonly string PROC_NAME = "�f�[�^�X�V"; // ������
        private static readonly string PROC_CANCEL_NAME = "���׃L�����Z��";      // ������     // Add 2017/08/02 arc yano #3783

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �T�[�r�X�`�[�����T�[�r�X
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// �݌ɏ����T�[�r�X
        /// </summary>
        private IStockService stockService;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsPurchaseController(){
            db = new CrmsLinqDataContext();
            
        }

        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;


        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ���i���׈ꗗ��ʕ\��
        /// <summary>
        /// ���i���׈ꗗ��ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            criteriaInit = true;
            form["PurchaseStatus"] = "001";
            form["PurchaseType"] = "001";
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            form["DefaultDepartmentCode"] = form["DepartmentCode"];
            form["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
            form["DefaultPurchaseStatus"] = form["PurchaseStatus"];
            form["DefaultPurchaseType"] = form["PurchasePurchaseType"];

            return Criteria(form);
        }

        /// <summary>
        /// ���i���׈ꗗ��ʕ\��
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            PaginatedList<GetPartsPurchase_Result> list = new PaginatedList<GetPartsPurchase_Result>();
            // �������ʃ��X�g�̎擾
            if (criteriaInit)
            {
                //�������Ȃ�(����\��)
            }
            else
            {
                list = GetSearchResultList(form);
            }
            // �������ڂ̐ݒ�
            SetDataComponent(form);

            return View("PartsPurchaseCriteria", list);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 ���i���ׁ@LinkEntry�捞���̒ǉ�
        /// </history>
        private PaginatedList<GetPartsPurchase_Result> GetSearchResultList(FormCollection form) 
        {
            PartsPurchaseSearchCondition condition = new PartsPurchaseSearchCondition();

            condition.PurchaseNumberFrom = form["PurchaseNumberFrom"];	            //���ד`�[�ԍ�From
            condition.PurchaseNumberTo = form["PurchaseNumberTo"];	                //���ד`�[�ԍ�To
            condition.PurchaseOrderNumberFrom = form["PurchaseOrderNumberFrom"];	//�����`�[�ԍ�From
            condition.PurchaseOrderNumberTo = form["PurchaseOrderNumberTo"];	    //�����`�[�ԍ�To
            condition.PurchaseOrderDateFrom = form["PurchaseOrderDateFrom"];	    //������From
            condition.PurchaseOrderDateTo = form["PurchaseOrderDateTo"];	        //������To
            condition.SlipNumberFrom = form["SlipNumberFrom"];	                    //�`�[�ԍ�From
            condition.SlipNumberTo = form["SlipNumberTo"];	                        //�`�[�ԍ�To
            condition.PurchaseType = form["PurchaseType"];	                        //�d���`�[�敪
            condition.OrderType = form["OrderType"];	                            //�����敪
            condition.CustomerCode = form["CustomerCode"];	                        //�ڋq�R�[�h
            condition.PartsNumber = form["PartsNumber"];	                        //���i�ԍ�
            condition.PurchasePlanDateFrom = form["PurchasePlanDateFrom"];	        //���ח\���From
            condition.PurchasePlanDateTo = form["PurchasePlanDateTo"];	            //���ח\���To
            condition.PurchaseDateFrom = form["PurchaseDateFrom"];	                //���ד�From
            condition.PurchaseDateTo = form["PurchaseDateTo"];	                    //���ד�To
            condition.DepartmentCode = form["DepartmentCode"];	                    //����R�[�h
            condition.EmployeeCode = form["EmployeeCode"];	                        //�Ј��R�[�h
            condition.SupplierCode = form["SupplierCode"];	                        //�d����R�[�h
            condition.WebOrderNumber = form["WebOrderNumber"];	                    //WEB�I�[�_�[�ԍ�
            condition.MakerOrderNumber = form["MakerOrderNumber"];	                //���[�J�[�I�[�_�[�ԍ�
            condition.InvoiceNo = form["InvoiceNo"];	                            //�C���{�C�X�ԍ�
            condition.PurchaseStatus = form["PurchaseStatus"];	                    //�d���X�e�[�^�X


            condition.LinkEntryCaptureDateFrom = form["LinkEntryCaptureDateFrom"];	//�捞��From           //Add 2018/03/26 arc yano #3863
            condition.LinkEntryCaptureDateTo = form["LinkEntryCaptureDateTo"];	    //�捞��To             //Add 2018/03/26 arc yano #3863


            return new PartsPurchaseDao(db).GetPurchaseListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ��ʃR���|�[�l���g�ݒ�
        /// <summary>
        /// ��ʃR���|�[�l���g�ݒ�
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 ���i���ׁ@LinkEntry�捞���̒ǉ�
        /// </history>
        private void SetDataComponent(FormCollection form)
        {

            CodeDao dao = new CodeDao(db);
            ViewData["DefaultDepartmentCode"] = form["DefaultDepartmentCode"];
            ViewData["DefaultDepartmentName"] = form["DefaultDepartmentName"];
            ViewData["DefaultPurchaseStatus"] = form["DefaultPurchaseStatus"];
            ViewData["DefaultPurchaseType"] = form["DefaultPurchaseType"];

            ViewData["PurchaseNumberFrom"] = form["PurchaseNumberFrom"];
            ViewData["PurchaseNumberTo"] = form["PurchaseNumberTo"];
            ViewData["PurchaseOrderNumberFrom"] = form["PurchaseOrderNumberFrom"];
            ViewData["PurchaseOrderNumberTo"] = form["PurchaseOrderNumberTo"];
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];
            ViewData["SlipNumberFrom"] = form["SlipNumberFrom"];
            ViewData["SlipNumberTo"] = form["SlipNumberTo"];
            ViewData["PurchaseType"] = form["PurchaseType"];
            ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), form["PurchaseType"], false);
            ViewData["OrderType"] = form["OrderType"];
            ViewData["OrderTypeList"] = CodeUtils.GetSelectListByModel<c_OrderType>(dao.GetOrderTypeAll(false), form["OrderType"], true);

            ViewData["CustomerCode"] = form["CustomerCode"];
            if (!string.IsNullOrEmpty(form["CustomerCode"]))
            {
                Customer customer = new CustomerDao(db).GetByKey(form["CustomerCode"]);
                ViewData["CustomerName"] = customer != null ? customer.CustomerName : "";
            }

            ViewData["PartsNumber"] = form["PartsNumber"];
            if (!string.IsNullOrEmpty(form["PartsNumber"]))
            {
                Parts parts = new PartsDao(db).GetByKey(form["PartsNumber"]);
                ViewData["PartsNameJp"] = parts != null ? parts.PartsNameJp : "";
            }

            ViewData["PurchasePlanDateFrom"] = form["PurchasePlanDateFrom"];
            ViewData["PurchasePlanDateTo"] = form["PurchasePlanDateTo"];
            ViewData["PurchaseDateFrom"] = form["PurchaseDateFrom"];
            ViewData["PurchaseDateTo"] = form["PurchaseDateTo"];

            ViewData["DepartmentCode"] = form["DepartmentCode"];
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }

            ViewData["EmployeeCode"] = form["EmployeeCode"];
            if (!string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                ViewData["EmployeeName"] = employee != null ? employee.EmployeeName : "";
            }

            ViewData["SupplierCode"] = form["SupplierCode"];
            if (!string.IsNullOrEmpty(form["SupplierCode"]))
            {
                Supplier supplier = new SupplierDao(db).GetByKey(form["SupplierCode"]);
                ViewData["SupplierName"] = supplier != null ? supplier.SupplierName : "";
            }

            ViewData["WebOrderNumber"] = form["WebOrderNumber"];
            ViewData["MakerOrderNumber"] = form["MakerOrderNumber"];
            ViewData["InvoiceNo"] = form["InvoiceNo"];
            ViewData["PurchaseStatus"] = form["PurchaseStatus"];
            ViewData["PurchaseStatusList"] = CodeUtils.GetSelectListByModel<c_PurchaseStatus>(dao.GetPurchaseStatusAll(false), form["PurchaseStatus"], false);


            ViewData["LinkEntryCaptureDateFrom"] = form["LinkEntryCaptureDateFrom"];        //Add 2018/03/26 arc yano #3863
            ViewData["LinkEntryCaptureDateTo"] = form["LinkEntryCaptureDateTo"];            //Add 2018/03/26 arc yano #3863

        }
        #endregion

        #region �`�F�b�N�������ڂ̕ҏW
        /// <summary>
        /// �`�F�b�N�������ڂ̕ҏW
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// /// 2016/06/20 arc yano #3585 ���i���׈ꗗ�@�����ǉ�(PurchaseStatus) 
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCheckedItemList(List<bool> check, List<PurchaseEntryKeyList> KeyList, string PurchaseStatus)
        {
            List<PurchaseEntryKeyList> CheckedList = new List<PurchaseEntryKeyList>();

            for (int i = 0; i < check.Count(); i++)
            {
                if (check[i] == true)   //�`�F�b�N�������Ă����ꍇ
                {
                    CheckedList.Add(KeyList[i]);
                }
            }

            return Entry(CheckedList, PurchaseStatus);
        }
        #endregion


        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ���͉�ʕ\��
        /// <summary>
        /// ���͉�ʕ\��
        /// </summary>
        /// <param name="KeyList">���ד`�[�ԍ��E�����ԍ��E���i�ԍ�</param>
        /// <returns>���͉��</returns>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��́@���׍σf�[�^�̕ҏW�Ƃ���
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή�
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// 2017/12/20 arc yano #3848 ���i���ד��́@���i���ד��͉�ʕ\�����̃V�X�e���G���[�@�����f�[�^���擾�ł��Ȃ��ꍇ�͕���R�[�h��ݒ肵��
        /// 2016/08/08 arc yano #3624 ����(DepartmentCode)�ǉ� ���O�C�����[�U�̏�������ł͂Ȃ��A�I���������ׁE�����f�[�^�̕���R�[�h����ד��͉�ʂɕ\������
        /// 2016/06/27 arc yano #3585 ���i���׈ꗗ�@���׃X�e�[�^�X=�u���׍ρv�̏ꍇ�́A���׃��R�[�h�̎擾���@��ύX����
        /// 2016/06/20 arc yano #3585 ���i���׈ꗗ�@�����ǉ�(PurchaseStatus) 
        /// 2016/04/21 arc nakayama #3493_���i���ד��́@�d�|���P�[�V�����������Ȃ�����̑I�����̕s�
        /// </history>
        public ActionResult Entry(List<PurchaseEntryKeyList> KeyList, string PurchaseStatus = "", bool OrderNumberClick = false, string PurchaseOrderNumber = "", string DepartmentCode = "")
        {            
            FormCollection form = new FormCollection();
            PartsPurchase_PurchaseList PList = new PartsPurchase_PurchaseList();
            List<GetPartsPurchaseList_Result> line = new List<GetPartsPurchaseList_Result>();

            //���ʍ���
            //Mod 2016/08/08 arc yano #3624
            //Mod 2016/04/21 arc nakayama #3493
            //Department department = new DepartmentDao(db).GetByKey(((Employee)Session["Employee"]).DepartmentCode, includeDeleted: false, closeMonthFlag: "2");

            string departmentCode = "";

            //�f�[�^�ҏW�̏ꍇ
            if (KeyList != null && KeyList.Count() > 0)
            {
                if (!string.IsNullOrWhiteSpace(KeyList[0].PurchaseNumber))
                {
                    PartsPurchase pprec = new PartsPurchaseDao(db).GetByKey(KeyList[0].PurchaseNumber);
                    //���׃��R�[�h���畔��R�[�h��ݒ�
                    departmentCode = pprec != null ? pprec.DepartmentCode : ""; //Mod 2017/12/20 arc yano #3848
                }
                else if (!string.IsNullOrWhiteSpace(KeyList[0].PurchaseOrderNumber))
                {
                    PartsPurchaseOrder porec = new PartsPurchaseOrderDao(db).GetByKey(KeyList[0].PurchaseOrderNumber);
                    //�������R�[�h���畔��R�[�h��ݒ�
                    departmentCode = porec != null ? porec.DepartmentCode : "";      //Mod 2017/12/20 arc yano #3848
                }
                else
                {
                    //�������Ȃ�
                }
            }
           
            Department department = new DepartmentDao(db).GetByKey(departmentCode, includeDeleted: false, closeMonthFlag: "2");  
            
            if (department != null)
            {
                PList.DepartmentCode = department.DepartmentCode; //����R�[�h
                form["DepartmentCode"] = PList.DepartmentCode;
                PList.DepartmentName = department != null ? department.DepartmentName : ""; //���喼
                form["DepartmentName"] = PList.DepartmentName;
            }
            PList.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�Ј��R�[�h
            form["CancelEmployeeCode"] = form["EmployeeCode"] = PList.EmployeeCode;     //Mod 2018/11/12 yano #3949
            if (!string.IsNullOrEmpty(PList.EmployeeCode))
            {
                Employee employee = new EmployeeDao(db).GetByKey(PList.EmployeeCode);
                PList.EmployeeName = employee != null ? employee.EmployeeName : ""; //�Ј���
                form["EmployeeName"] = PList.EmployeeName;
            }
            PList.PurchaseDate = string.Format("{0:yyyy/MM/dd}", DateAndTime.Today);  //���ד�(����)
            
            form["CancelPurchaseDate"] = form["PurchaseDate"] = PList.PurchaseDate;                     //Mod 2018/11/12 yano #3949


            PList.PurchaseType = "001"; //���ד`�[�敪�i�ԕi�ŕۑ������P�[�X���Ȃ����߁A����ŊJ���Ƃ���"����"�Œ�j
            form["PurchaseType"] = PList.PurchaseType;

            form["PurchaseStatus"] = string.IsNullOrWhiteSpace(PurchaseStatus) ? "001" : PurchaseStatus;        //Add 2016/06/20 arc yano #3585

            List<GetPartsPurchaseList_Result> lineItem = new List<GetPartsPurchaseList_Result>();

            if (OrderNumberClick)
            {
                lineItem = new PartsPurchaseDao(db).GetPurchaseByPurchaseOrderNumber(PurchaseOrderNumber, PList.DepartmentCode);
                line.AddRange(lineItem);
            }
            else
            {
                foreach (var Key in KeyList)
                {

                    if (string.IsNullOrEmpty(Key.PurchaseNumber) && string.IsNullOrEmpty(Key.PurchaseOrderNumber) && string.IsNullOrEmpty(Key.PartsNumber))
                    {
                        //�V�K�쐬
                        PList = new PartsPurchase_PurchaseList();
                    }
                    else
                    {
                        //-----------------------------------------------------------------------
                        //�`�F�b�N�������ڂ̕ҏW�܂��́A���ד`�[�ԍ��N���b�N
                        //-----------------------------------------------------------------------
                        lineItem = new PartsPurchaseDao(db).GetPurchaseByCondition(Key.PurchaseNumber, Key.PurchaseOrderNumber, Key.PartsNumber, PList.DepartmentCode, PurchaseStatus);
                        
                        //�\�����X�g�ɒǉ�
                        line.AddRange(lineItem);
                    }
                }
            }

            //Mod 2018/06/01 arc yano #3894
            //Add 2018/01/15 arc yano #3833
            //--------------------------------------------
            //�����i�̃f�t�H���g�̎d������擾
            //--------------------------------------------
            form["GenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode").Value;
            form["GenuineSupplierName"] = new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]).SupplierName : "";

            form["JLRGenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode_JLR").Value;
            form["JLRGenuineSupplierName"] = new SupplierDao(db).GetByKey(form["JLRGenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["JLRGenuineSupplierCode"]).SupplierName : "";

            //Add 2018/01/15 arc yano
            foreach (var l in line)
            {
                //�d���悪�󗓂ł����i�ԍ����󗓂łȂ��ꍇ
                if (string.IsNullOrWhiteSpace(l.SupplierCode) && !string.IsNullOrWhiteSpace(l.PartsNumber))
                {
                    Parts parts = new PartsDao(db).GetByKey(l.PartsNumber, false);

                    //���̕��i�������i�̏ꍇ
                    if (parts != null && parts.GenuineType.Equals("001"))
                    {
                        //�f�t�H���g�̎d�����ݒ�
                        //l.SupplierCode = form["GenuineSupplierCode"];
                        //l.SupplierName = form["GenuineSupplierName"];

                        //���i�̃��[�J�[�R�[�h���uJG�v�܂��́uLR�v�̏ꍇ�͎d����=JLR ����ȊO�̏ꍇ��FCJ

                        if (parts.MakerCode.Equals("JG") || parts.MakerCode.Equals("LR"))
                        {
                            l.SupplierCode = form["JLRGenuineSupplierCode"];
                            l.SupplierName = form["JLRGenuineSupplierName"];
                        }
                        else
                        {
                            l.SupplierCode = form["GenuineSupplierCode"];
                            l.SupplierName = form["GenuineSupplierName"];
                        }
                    }
                }
            }

            PList.line = line;

            //Mod 2016/06/28 arc yano #3585 �����ǉ�
            //���׃X�e�[�^�X = �u���׍ρv�̏ꍇ�͓��׃��R�[�h�����ɐݒ肷��
            if (string.IsNullOrWhiteSpace(PurchaseStatus) || PurchaseStatus.Equals("001"))
            {
                SetEntryDataComponent(form);                
            }
            else
            {
                SetEntryDataComponent(form, PList.line);                
            }

            return View("PartsPurchaseEntry", PList);
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ���׏���
        /// <summary>
        /// ���׏���
        /// </summary>
        /// <param name="purchase">�d���f�[�^</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>���͉��</returns>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��� ���׍σf�[�^�̕ҏW
        /// 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX �݌ɍX�V�����̈����̒ǉ�
        /// 2017/11/06 arc yano #3808 ���i���ד��� Web�I�[�_�[�ԍ����̒ǉ�
        /// 2017/08/02 arc yano #3783 ���i���ד��� ���׎���E�L�����Z���@�\�@�������i�ԍ���DB�o�^�A����E�L�����Z�������̕���̒ǉ�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���P�[�V�����̒��o�����̕ύX(���偨�q��)
        /// 2016/08/05 arc yano #3625 ���i���ד��́@�T�[�r�X�`�[�ɕR�Â����׏����̈������� ���������̈���(���׃��P�[�V����)�ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {

            //Mod 2018/11/12 yano #3949
            if (form["RequestFlag"].Equals("1") || form["RequestFlag"].Equals("2")) //���׎�� or �L�����Z��
            {
                return Cancel(Plist, Purchase, form);
            }
            else if (form["RequestFlag"].Equals("3"))                               //���׍σf�[�^�ۑ�
            {
                return Save(Plist, Purchase, form);
            }
            else                                                                    //���׊m��
            {
                return Confirm(Plist, Purchase, form);
            }

            ////Mod 2017/08/02 arc yano #3783
            ////���׎�� or ���׃L�����Z��
            //if (form["RequestFlag"].Equals("1") || form["RequestFlag"].Equals("2"))
            //{
            //   return Cancel(Plist, Purchase, form);
            //}
            //else
            //{
            //    //Validation�`�F�b�N
            //    ValidateSave(Plist, Purchase, form);
            //    if (!ModelState.IsValid)
            //    {
            //        SetEntryDataComponent(form);
            //        Plist.line = Purchase;
            //        return View("PartsPurchaseEntry", Plist);
            //    }

            //    // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            //    db = new CrmsLinqDataContext();
            //    db.Log = new OutputWriter();

            //    stockService = new StockService(db);
            //    service = new ServiceSalesOrderService(db);

            //    using (TransactionScope ts = new TransactionScope())
            //    {
            //        for (int i = 0; i < Purchase.Count; i++)
            //        {
            //            //���א��������͂̏ꍇ�͑ΏۊO�i��֕��i�̌����R�[�h�����̑ΏۂɂȂ�j
            //            if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
            //            {
            //                PartsPurchase partspurchase = new PartsPurchase();
            //                //���ד`�[�ԍ�������Α��݃`�F�b�N �i���ד`�[�ԍ����Ȃ�/���݂��Ȃ���ΐV�K�쐬�j
            //                if (!string.IsNullOrEmpty(Purchase[i].PurchaseNumber))
            //                {
            //                    partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);
            //                }
            //                else
            //                {
            //                    partspurchase = null;
            //                }

            //                string OrderPartsNumber; //�������i�ԍ�(�����Ƃ͕ʂ̕��i�����ׂ����ꍇ�A�����������i�ԍ���SET����A�����łȂ���΁A���ו��i�ԍ�)
            //                if (Purchase[i].ChangeParts)
            //                {
            //                    OrderPartsNumber = Purchase[i].ChangePartsNumber;
            //                }
            //                else
            //                {
            //                    OrderPartsNumber = Purchase[i].PartsNumber;
            //                }


            //                if (partspurchase == null)
            //                {

            //                    //�V�K�쐬

            //                    //�����׃e�[�u��
            //                    PartsPurchase purchase = new PartsPurchase();
            //                    purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //���ד`�[�ԍ�
            //                    purchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //�����`�[�ԍ�
            //                    purchase.PurchaseType = form["PurchaseType"];                                   //���ד`�[�敪
            //                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
            //                    purchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;       //���ח\���
            //                    purchase.PurchaseDate = DateTime.Parse(Plist.PurchaseDate);                     //���ד�
            //                    purchase.PurchaseStatus = "002";                                                //�d���X�e�[�^�X
            //                    purchase.SupplierCode = Purchase[i].SupplierCode;                               //�d����R�[�h
            //                    purchase.EmployeeCode = form["EmployeeCode"];                                   //�Ј��R�[�h
            //                    purchase.DepartmentCode = form["DepartmentCode"];                               //����R�[�h
            //                    purchase.LocationCode = Purchase[i].LocationCode;                               //���P�[�V�����R�[�h
            //                    purchase.PartsNumber = Purchase[i].PartsNumber;                                 //���i�ԍ�
            //                    purchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //�P��
            //                    purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //���א�
            //                    purchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //���z
            //                    purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //�[�i���ԍ�
            //                    purchase.Memo = Purchase[i].Memo;                                               //����
            //                    purchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //�C���{�C�X�ԍ�
            //                    purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //���[�J�[�I�[�_�[�ԍ�
            //                    purchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Web�I�[�_�[�ԍ�             //Add 2017/11/06 arc yano #3808
            //                    purchase.CreateDate = DateTime.Now;                                             //�쐬��
            //                    purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
            //                    purchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
            //                    purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
            //                    purchase.DelFlag = "0";
            //                    // Add 2014/09/10 arc amii ���i���ח���Ή� �����ԍ���1�ɐݒ�
            //                    purchase.RevisionNumber = 1;

            //                    purchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //�������i�ԍ�    //Add 2017/08/02 arc yano #3783


            //                    db.PartsPurchase.InsertOnSubmit(purchase);
            //                }
            //                else
            //                {
            //                    //�X�V
            //                    partspurchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //�����`�[�ԍ�
            //                    partspurchase.PurchaseType = form["PurchaseType"];                                   //���ד`�[�敪

            //                    //���ח\��������ݒ肾�����ꍇ�A�����̓��ח\����ōX�V����
            //                    if (partspurchase.PurchasePlanDate == null)                                          //���ח\���
            //                    {
            //                        PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
            //                        partspurchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;
            //                    }
            //                    partspurchase.PurchaseDate = DateTime.Parse(form["PurchaseDate"]);                   //���ד�
            //                    partspurchase.PurchaseStatus = "002";                                                //�d���X�e�[�^�X
            //                    partspurchase.SupplierCode = Purchase[i].SupplierCode;                               //�d����R�[�h
            //                    partspurchase.DepartmentCode = form["DepartmentCode"];                               //����R�[�h
            //                    partspurchase.LocationCode = Purchase[i].LocationCode;                               //���P�[�V�����R�[�h
            //                    partspurchase.PartsNumber = Purchase[i].PartsNumber;                                 //���i�ԍ� 
            //                    partspurchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //�P��
            //                    partspurchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //���א�
            //                    partspurchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //���z
            //                    partspurchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //�[�i���ԍ�
            //                    partspurchase.Memo = Purchase[i].Memo;                                               //����
            //                    partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //�C���{�C�X�ԍ�
            //                    partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //���[�J�[�I�[�_�[�ԍ�
            //                    partspurchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Web�I�[�_�[�ԍ�           //Add 2017/11/06 arc yano #3808 
            //                    partspurchase.CreateDate = DateTime.Now;                                             //�쐬��
            //                    partspurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
            //                    partspurchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
            //                    partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
            //                    partspurchase.DelFlag = "0";

            //                    partspurchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //�������i�ԍ�    //Add 2017/08/02 arc yano #3783
            //                }

            //                //Mod 2016/08/13 arc yano #3596
            //                //����R�[�h����g�p�q�ɂ����o
            //                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, Plist.DepartmentCode);

            //                string warehouseCode = "";
            //                if (dWarehouse != null)
            //                {
            //                    warehouseCode = dWarehouse.WarehouseCode;
            //                }

            //                //�������X�V
            //                //�����`�[�ԍ������͂���Ă�����A�Y���̔����f�[�^���X�V����B�Y������f�[�^���Ȃ��ꍇ�͉������Ȃ�
            //                if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
            //                {
            //                    UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, Purchase[i].PurchaseQuantity);     //Mod 2017/11/06 arc yano #3808
            //                }
            //                //�����i���P�[�V����  �Y�����镔�i���P�[�V���������݂��Ă��Ȃ�������V�K�o�^/���݂��Ă�����X�V
            //                //UpdatePartsLocation(Purchase[i].PartsNumber, form["DepartmentCode"], Purchase[i].LocationCode);
            //                UpdatePartsLocation(Purchase[i].PartsNumber, warehouseCode, Purchase[i].LocationCode);  //Mod 2016/08/13 arc yano #3596

            //                //�����i�݌ɍX�V
            //                UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, form["PurchaseType"], Purchase[i].Price);     //Mod 2018/05/14 arc yano #3880

            //                //���T�[�r�X�`�[���X�V �󒍓`�[�ԍ������݂����ꍇ�̂ݍX�V
            //                if (!string.IsNullOrEmpty(Purchase[i].SlipNumber))
            //                {
            //                    string OrderType = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber).OrderType; //�����敪

            //                    ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetBySlipNumber(Purchase[i].SlipNumber);
            //                    EntitySet<ServiceSalesLine> ServiceLines = header.ServiceSalesLine;
            //                    //Mod 2016/08/13 arc yano #3596
            //                    //string shikakariLocation = stockService.GetShikakariLocation(Plist.DepartmentCode).LocationCode;
            //                    string shikakariLocation = stockService.GetShikakariLocation(warehouseCode).LocationCode;
            //                    service.PurchaseHikiate(ref header, ref ServiceLines, shikakariLocation, OrderType, OrderPartsNumber, Purchase[i].PartsNumber, Purchase[i].PurchaseQuantity, Purchase[i].Price, Purchase[i].Amount, ((Employee)Session["Employee"]).EmployeeCode, Purchase[i].SupplierCode, Purchase[i].LocationCode);
            //                }
            //            }
            //        }

            //        for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            //        {
            //            try
            //            {
            //                db.SubmitChanges();
            //                //�R�~�b�g
            //                ts.Complete();
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
            //                    // �Z�b�V������SQL����o�^
            //                    Session["ExecSQL"] = OutputLogData.sqlText;
            //                    // ���O�ɏo��
            //                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
            //                    // �G���[�y�[�W�ɑJ��
            //                    return View("Error");
            //                }
            //            }
            //            catch (SqlException e)
            //            {
            //                //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
            //                // �Z�b�V������SQL����o�^
            //                Session["ExecSQL"] = OutputLogData.sqlText;

            //                if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
            //                {
            //                    //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͏����ǉ�
            //                    OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

            //                    ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
            //                    Plist.line = Purchase;
            //                    SetEntryDataComponent(form);
            //                    return View("PartsPurchaseEntry", Plist);
            //                }
            //                else
            //                {
            //                    //Mod 2014/08/08 arc amii �G���[���O�Ή� �wtheow e�x���烍�O�o�͏����ɕύX
            //                    // ���O�ɏo��
            //                    OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
            //                    return View("Error");
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                //Add 2014/08/08 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
            //                // �Z�b�V������SQL����o�^
            //                Session["ExecSQL"] = OutputLogData.sqlText;
            //                // ���O�ɏo��
            //                OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
            //                // �G���[�y�[�W�ɑJ��
            //                return View("Error");
            //            }
            //        }
            //    }

            //    SetEntryDataComponent(form);

            //    ViewData["close"] = "1";
            //    Plist.line = Purchase;
            //    return View("PartsPurchaseEntry", Plist);
            //}
        }
        #endregion



        #region ���וۑ�����
        /// <summary>
        /// ���וۑ�
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��� ���׍σf�[�^�̕ҏW �V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult Save(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
          
            // �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {

                    PartsPurchase partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);

                    //����̍���(�C���{�C�X�ԍ��A���[�J�[�I�[�_�[�ԍ��A���l�A�ŏI�X�V�ҁA�ŏI�X�V��)�̂ݍX�V
                    partspurchase.Memo = Purchase[i].Memo;                                               //����
                    partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //�C���{�C�X�ԍ�
                    partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //���[�J�[�I�[�_�[�ԍ�
                    partspurchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                    partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��

                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //�R�~�b�g
                        ts.Complete();
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
                        //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͏����ǉ�
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii �G���[���O�Ή� �wtheow e�x���烍�O�o�͏����ɕύX
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            return View("PartsPurchaseEntry", Plist);

        }
        #endregion


        #region ���׊m�菈��
         /// <summary>
        /// ���׊m��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��� ���׍σf�[�^�̕ҏW �m�菈�����O����
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private ActionResult Confirm(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //--------------------------
            //���͒l�`�F�b�N
            //--------------------------
            //Validation�`�F�b�N
            ValidateSave(Plist, Purchase, form);
            if (!ModelState.IsValid)
            {
                SetEntryDataComponent(form);
                Plist.line = Purchase;
                return View("PartsPurchaseEntry", Plist);
            }

            // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {
                    //���א��������͂̏ꍇ�͑ΏۊO�i��֕��i�̌����R�[�h�����̑ΏۂɂȂ�j
                    if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
                    {
                        PartsPurchase partspurchase = new PartsPurchase();
                        //���ד`�[�ԍ�������Α��݃`�F�b�N �i���ד`�[�ԍ����Ȃ�/���݂��Ȃ���ΐV�K�쐬�j
                        if (!string.IsNullOrEmpty(Purchase[i].PurchaseNumber))
                        {
                            partspurchase = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);
                        }
                        else
                        {
                            partspurchase = null;
                        }

                        string OrderPartsNumber; //�������i�ԍ�(�����Ƃ͕ʂ̕��i�����ׂ����ꍇ�A�����������i�ԍ���SET����A�����łȂ���΁A���ו��i�ԍ�)
                        if (Purchase[i].ChangeParts)
                        {
                            OrderPartsNumber = Purchase[i].ChangePartsNumber;
                        }
                        else
                        {
                            OrderPartsNumber = Purchase[i].PartsNumber;
                        }


                        if (partspurchase == null)
                        {

                            //�V�K�쐬

                            //�����׃e�[�u��
                            PartsPurchase purchase = new PartsPurchase();
                            purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //���ד`�[�ԍ�
                            purchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //�����`�[�ԍ�
                            purchase.PurchaseType = form["PurchaseType"];                                   //���ד`�[�敪
                            PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
                            purchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;       //���ח\���
                            purchase.PurchaseDate = DateTime.Parse(Plist.PurchaseDate);                     //���ד�
                            purchase.PurchaseStatus = "002";                                                //�d���X�e�[�^�X
                            purchase.SupplierCode = Purchase[i].SupplierCode;                               //�d����R�[�h
                            purchase.EmployeeCode = form["EmployeeCode"];                                   //�Ј��R�[�h
                            purchase.DepartmentCode = form["DepartmentCode"];                               //����R�[�h
                            purchase.LocationCode = Purchase[i].LocationCode;                               //���P�[�V�����R�[�h
                            purchase.PartsNumber = Purchase[i].PartsNumber;                                 //���i�ԍ�
                            purchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //�P��
                            purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //���א�
                            purchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //���z
                            purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //�[�i���ԍ�
                            purchase.Memo = Purchase[i].Memo;                                               //����
                            purchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //�C���{�C�X�ԍ�
                            purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //���[�J�[�I�[�_�[�ԍ�
                            purchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Web�I�[�_�[�ԍ�             //Add 2017/11/06 arc yano #3808
                            purchase.CreateDate = DateTime.Now;                                             //�쐬��
                            purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
                            purchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                            purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                            purchase.DelFlag = "0";
                            // Add 2014/09/10 arc amii ���i���ח���Ή� �����ԍ���1�ɐݒ�
                            purchase.RevisionNumber = 1;

                            purchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //�������i�ԍ�    //Add 2017/08/02 arc yano #3783


                            db.PartsPurchase.InsertOnSubmit(purchase);
                        }
                        else
                        {
                            //�X�V
                            partspurchase.PurchaseOrderNumber = Purchase[i].PurchaseOrderNumber;                 //�����`�[�ԍ�
                            partspurchase.PurchaseType = form["PurchaseType"];                                   //���ד`�[�敪

                            //���ח\��������ݒ肾�����ꍇ�A�����̓��ח\����ōX�V����
                            if (partspurchase.PurchasePlanDate == null)                                          //���ח\���
                            {
                                PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, OrderPartsNumber);
                                partspurchase.PurchasePlanDate = order != null ? order.ArrivalPlanDate : null;
                            }
                            partspurchase.PurchaseDate = DateTime.Parse(form["PurchaseDate"]);                   //���ד�
                            partspurchase.PurchaseStatus = "002";                                                //�d���X�e�[�^�X
                            partspurchase.SupplierCode = Purchase[i].SupplierCode;                               //�d����R�[�h
                            partspurchase.DepartmentCode = form["DepartmentCode"];                               //����R�[�h
                            partspurchase.LocationCode = Purchase[i].LocationCode;                               //���P�[�V�����R�[�h
                            partspurchase.PartsNumber = Purchase[i].PartsNumber;                                 //���i�ԍ� 
                            partspurchase.Price = decimal.Parse(Purchase[i].Price.ToString());                   //�P��
                            partspurchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());     //���א�
                            partspurchase.Amount = decimal.Parse(Purchase[i].Amount.ToString());                 //���z
                            partspurchase.ReceiptNumber = Purchase[i].ReceiptNumber;                             //�[�i���ԍ�
                            partspurchase.Memo = Purchase[i].Memo;                                               //����
                            partspurchase.InvoiceNo = Purchase[i].InvoiceNo;                                     //�C���{�C�X�ԍ�
                            partspurchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                       //���[�J�[�I�[�_�[�ԍ�
                            partspurchase.WebOrderNumber = Purchase[i].WebOrderNumber;                           //Web�I�[�_�[�ԍ�           //Add 2017/11/06 arc yano #3808 
                            partspurchase.CreateDate = DateTime.Now;                                             //�쐬��
                            partspurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
                            partspurchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                            partspurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                            partspurchase.DelFlag = "0";

                            partspurchase.OrderPartsNumber = (Purchase[i].ChangePartsNumber ?? "");              //�������i�ԍ�    //Add 2017/08/02 arc yano #3783
                        }

                        //Mod 2016/08/13 arc yano #3596
                        //����R�[�h����g�p�q�ɂ����o
                        DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, Plist.DepartmentCode);

                        string warehouseCode = "";
                        if (dWarehouse != null)
                        {
                            warehouseCode = dWarehouse.WarehouseCode;
                        }

                        //�������X�V
                        //�����`�[�ԍ������͂���Ă�����A�Y���̔����f�[�^���X�V����B�Y������f�[�^���Ȃ��ꍇ�͉������Ȃ�
                        if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                        {
                            UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, Purchase[i].PurchaseQuantity);     //Mod 2017/11/06 arc yano #3808
                        }
                        //�����i���P�[�V����  �Y�����镔�i���P�[�V���������݂��Ă��Ȃ�������V�K�o�^/���݂��Ă�����X�V
                        //UpdatePartsLocation(Purchase[i].PartsNumber, form["DepartmentCode"], Purchase[i].LocationCode);
                        UpdatePartsLocation(Purchase[i].PartsNumber, warehouseCode, Purchase[i].LocationCode);  //Mod 2016/08/13 arc yano #3596

                        //�����i�݌ɍX�V
                        UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, form["PurchaseType"], Purchase[i].Price);     //Mod 2018/05/14 arc yano #3880

                        //���T�[�r�X�`�[���X�V �󒍓`�[�ԍ������݂����ꍇ�̂ݍX�V
                        if (!string.IsNullOrEmpty(Purchase[i].SlipNumber))
                        {
                            string OrderType = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber).OrderType; //�����敪

                            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetBySlipNumber(Purchase[i].SlipNumber);
                            EntitySet<ServiceSalesLine> ServiceLines = header.ServiceSalesLine;
                            //Mod 2016/08/13 arc yano #3596
                            //string shikakariLocation = stockService.GetShikakariLocation(Plist.DepartmentCode).LocationCode;
                            string shikakariLocation = stockService.GetShikakariLocation(warehouseCode).LocationCode;
                            service.PurchaseHikiate(ref header, ref ServiceLines, shikakariLocation, OrderType, OrderPartsNumber, Purchase[i].PartsNumber, Purchase[i].PurchaseQuantity, Purchase[i].Price, Purchase[i].Amount, ((Employee)Session["Employee"]).EmployeeCode, Purchase[i].SupplierCode, Purchase[i].LocationCode);
                        }
                    }
                }

                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //�R�~�b�g
                        ts.Complete();
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
                        //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͏����ǉ�
                            OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            //Mod 2014/08/08 arc amii �G���[���O�Ή� �wtheow e�x���烍�O�o�͏����ɕύX
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Add 2014/08/08 arc amii �G���[���O�Ή� ��LException�ȊO�̎��̃G���[�����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            return View("PartsPurchaseEntry", Plist);
        
        }
        #endregion

        #region �폜�E���׃L�����Z��
        /// <summary>
        /// ���׃L�����Z��
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��́@���׍σf�[�^�̕ҏW�Ƃ���
        /// 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX �݌ɍX�V�����̈����̒ǉ�
        /// 2017/11/06 arc yano #3808 ���i���ד��́@Web�I�[�_�[�ԍ����͗��̒ǉ�
        /// 2017/08/02 arc yano #3783 ���i���ד��� ���׎���E�L�����Z���@�\�@�V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Cancel(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //----------------------
            //Validation�`�F�b�N
            //----------------------
            ValidateCancel(Plist, Purchase, form);
            if (!ModelState.IsValid)
            {
                SetEntryDataComponent(form);
                Plist.line = Purchase;
                return View("PartsPurchaseEntry", Plist);
            }

            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            //----------------------------
            //�폜�E�L�����Z������
            //----------------------------
            using (TransactionScope ts = new TransactionScope())
            {
                for (int i = 0; i < Purchase.Count; i++)
                {
                    //-------------------------------
                    //�����׃f�[�^�̍폜�E�L�����Z��
                    //-------------------------------
                    //���׃��R�[�h���擾����
                    PartsPurchase targetrec = new PartsPurchaseDao(db).GetByKey(Purchase[i].PurchaseNumber);

                    //���׎��(���׍σ��R�[�h��_���폜)
                    if (!string.IsNullOrWhiteSpace(form["RequestFlag"]) && form["RequestFlag"].Equals("1"))
                    {
                        if (targetrec != null)
                        {
                            targetrec.DelFlag = "1";
                            targetrec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                            targetrec.LastUpdateDate = DateTime.Now;
                        }
                    }
                    else //���׃L�����Z��
                    {
                        //�����׃e�[�u��(�ԕi�p)
                        PartsPurchase purchase = new PartsPurchase();
                        purchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();                                               //���ד`�[�ԍ�
                        purchase.PurchaseOrderNumber = targetrec.PurchaseOrderNumber;                                                           //�����`�[�ԍ�
                        purchase.PurchaseType = "002";                                                                                          //���ד`�[�敪(=�ԕi)
                        purchase.PurchasePlanDate = targetrec.PurchasePlanDate;                                                                 //���ח\���
                        purchase.PurchaseDate = DateTime.Parse(form["CancelPurchaseDate"]);                                                     //�L�����Z����     //Mod 2018/11/12 yano #3949                                           
                        purchase.PurchaseStatus = "002";                                                                                        //�d���X�e�[�^�X
                        purchase.SupplierCode = Purchase[i].SupplierCode;                                                                       //�d����R�[�h
                        purchase.EmployeeCode = form["CancelEmployeeCode"];                                                                     //�Ј��R�[�h       //Mod 2018/11/12 yano #3949             
                        purchase.DepartmentCode = form["DepartmentCode"];                                                                       //����R�[�h
                        purchase.LocationCode = targetrec.LocationCode;                                                                         //���P�[�V�����R�[�h
                        purchase.PartsNumber = targetrec.PartsNumber;                                                                           //���i�ԍ�
                        purchase.Price = decimal.Parse(targetrec.Price.ToString());                                                             //�P��
                        purchase.Quantity = decimal.Parse(Purchase[i].PurchaseQuantity.ToString());                                             //���א�
                        purchase.Amount = decimal.Parse(targetrec.Amount.ToString());                                                           //���z
                        purchase.ReceiptNumber = Purchase[i].ReceiptNumber;                                                                     //�[�i���ԍ�
                        purchase.Memo = Purchase[i].Memo;                                                                                       //����
                        purchase.InvoiceNo = Purchase[i].InvoiceNo;                                                                             //�C���{�C�X�ԍ�
                        purchase.MakerOrderNumber = Purchase[i].MakerOrderNumber;                                                               //���[�J�[�I�[�_�[�ԍ�
                        purchase.CreateDate = DateTime.Now;                                                                                     //�쐬��
                        purchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                             //�쐬��
                        purchase.LastUpdateDate = DateTime.Now;                                                                                 //�ŏI�X�V��
                        purchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                                         //�ŏI�X�V��
                        purchase.DelFlag = "0";
                        purchase.RevisionNumber = 1;
                        purchase.OrderPartsNumber = Purchase[i].ChangePartsNumber;                                                              //�������i�ԍ�
                        db.PartsPurchase.InsertOnSubmit(purchase);
                    }

                    //�������X�V
                    string OrderPartsNumber; //�������i�ԍ�(�����Ƃ͕ʂ̕��i�����ׂ����ꍇ�A�����������i�ԍ���SET����A�����łȂ���΁A���ו��i�ԍ�)
                    if (!string.IsNullOrWhiteSpace(Purchase[i].ChangePartsNumber))
                    {
                        OrderPartsNumber = Purchase[i].ChangePartsNumber;
                    }
                    else
                    {
                        OrderPartsNumber = Purchase[i].PartsNumber;
                    }

                    //�����`�[�ԍ������͂���Ă�����A�Y���̔����f�[�^���X�V����B�Y������f�[�^���Ȃ��ꍇ�͉������Ȃ�
                    if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                    {
                        UpdatePartsPurchaseOrder(Purchase[i], OrderPartsNumber, (Purchase[i].PurchaseQuantity * -1));   //Mod 2017/11/06 arc yano #3808
                    }

                    //�����i�݌ɍX�V
                    UpdatePartsStock(Purchase[i].PartsNumber, Purchase[i].LocationCode, Purchase[i].PurchaseQuantity, "002", targetrec.Price); // Mod 2018/05/14 arc yano #3880
                }
                    
                for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
                {
                    try
                    {
                        db.SubmitChanges();
                        //�R�~�b�g
                        ts.Complete();
                        break;
                    }
                    catch (ChangeConflictException e)
                    {
                        foreach (ObjectChangeConflict occ in db.ChangeConflicts)
                        {
                            occ.Resolve(RefreshMode.KeepCurrentValues);
                        }
                        if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                        {
                            // �Z�b�V������SQL����o�^
                            Session["ExecSQL"] = OutputLogData.sqlText;
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_CANCEL_NAME, FORM_NAME, "");
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
                            OutputLogger.NLogError(e, PROC_CANCEL_NAME, FORM_NAME, "");

                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            Plist.line = Purchase;
                            SetEntryDataComponent(form);
                            return View("PartsPurchaseEntry", Plist);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_CANCEL_NAME, FORM_NAME, "");
                            return View("Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(ex, PROC_CANCEL_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
            }

            SetEntryDataComponent(form);

            ViewData["close"] = "1";
            Plist.line = Purchase;
            
            return View("PartsPurchaseEntry", Plist);
        }
        #endregion

        #region ���i���׍s�ǉ��E�폜
        /// <summary>
        /// ���i���׍s�ǉ�
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3807 ���i���ד��� �P�O�s�{�^���̒ǉ�
        /// 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        /// </history>
        public ActionResult AddLine(PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            if (Purchase == null)
            {
                Purchase = new List<GetPartsPurchaseList_Result>();
            }


            //Mod 2017/11/07 arc yano #3807
            int addLine = int.Parse(form["addLine"]);   

            for (int i = 0; i < addLine; i++)
            {
                GetPartsPurchaseList_Result Addline = new GetPartsPurchaseList_Result();
                Purchase.Add(Addline);
            }
            
            ModelState.Clear();

            Plist.line = Purchase;

            SetEntryDataComponent(form);
            return View("PartsPurchaseEntry", Plist);
        }

        /// <summary>
        /// ���i���׍s�폜
        /// </summary>
        /// <param name="id">�s�ԍ�</param>
        /// <returns></returns>
        public ActionResult DelLine(int id, PartsPurchase_PurchaseList Plist, GetPartsPurchaseList_Result line, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            Purchase.RemoveAt(id);
            
            ModelState.Clear();

            Plist.line = Purchase;
            SetEntryDataComponent(form);
            return View("PartsPurchaseEntry", Plist);
        }
        #endregion


        #region ���i�����f�[�^�X�V
        /// <summary>
        /// ���i�����f�[�^�X�V
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2017/11/06 arc yano #3808 ���i���ד��́@Web�I�[�_�[�ԍ����͗��̒ǉ� �����̕ύX
        /// 2017/08/02 arc yano #3783 ���i���ד��� ���׎���E�L�����Z���@�\�@�L�����Z���������̃X�e�[�^�X�߂������̒ǉ�
        /// 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        /// </history>
        private void UpdatePartsPurchaseOrder(GetPartsPurchaseList_Result Purchase, string PartsNumber, decimal? PurchaseQuantity)
        {
            decimal? RemQuantity = 0; //�����c

            PartsPurchaseOrder purchaseOrderRet = new PartsPurchaseOrderDao(db).GetByKey(Purchase.PurchaseOrderNumber, PartsNumber);
            if (purchaseOrderRet != null)
            {
                //�����c = �����c - ���א�
                RemQuantity = (purchaseOrderRet.RemainingQuantity ?? 0) - (PurchaseQuantity ?? 0);

                //�����X�e�[�^�X
                //(�����c - ���א�) == 0 �܂��� (�����c - ���א�) < 0�@�̏ꍇ�͎d���ρ@
                if (RemQuantity == 0 || RemQuantity < 0)
                {
                    purchaseOrderRet.PurchaseOrderStatus = "004"; //�d����
                }
                //Add 2017/08/02 arc yano #3783
                else if (RemQuantity == (purchaseOrderRet.Quantity ?? 0)) //�����c���������Ɠ����ɂȂ����ꍇ
                {
                    purchaseOrderRet.PurchaseOrderStatus = "002"; //������
                }
                else
                {
                    purchaseOrderRet.PurchaseOrderStatus = "003"; //���[��
                }

                //Add 2017/11/06 arc yano #3808
                //Web�I�[�_�[�ԍ��̍X�V
                if (!string.IsNullOrWhiteSpace(Purchase.WebOrderNumber))
                {
                    purchaseOrderRet.WebOrderNumber = Purchase.WebOrderNumber;
                }

                purchaseOrderRet.RemainingQuantity = RemQuantity; //�����c
                purchaseOrderRet.LastUpdateDate = DateTime.Now;   //�ŏI�X�V��
                purchaseOrderRet.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
            }
        }
        #endregion

        //Add 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region ���i���P�[�V�����̍X�V
        /// <summary>
        /// ���i���P�[�V�����̍X�V
        /// </summary>
        /// <param name="PartsNumber">���i�ԍ�</param>
        /// <param name="DepartmentCode">����R�[�h</param>
        /// <param name="LocationCode">���P�[�V�����R�[�h</param>
        /// <returns>���͉��</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���P�[�V�����̒��o�����̕ύX(���偨�q��)
        /// 2016/08/05 arc yano #3625 ���i���ד��́@�T�[�r�X�`�[�ɕR�Â����׏����̈������� ���������̈���(���׃��P�[�V����)�ǉ�
        /// </history>
        private void UpdatePartsLocation(string PartsNumber, string WarehouseCode, string LocationCode)
        {
            PartsLocation partslocation = new PartsLocationDao(db).GetByKey(PartsNumber, WarehouseCode);        //Mod 2016/08/13 arc yano #3596
            if (partslocation == null)
            {
                PartsLocation Newpartslocation = new PartsLocation();
                Newpartslocation.PartsNumber = PartsNumber;                                             //���i�ԍ�
                Newpartslocation.DepartmentCode = "";                                                   //����R�[�h ���󕶎���ݒ�
                Newpartslocation.LocationCode = LocationCode;                                           //���P�[�V�����R�[�h
                Newpartslocation.CreateDate = DateTime.Now;                                             //�쐬��
                Newpartslocation.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
                Newpartslocation.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                Newpartslocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                Newpartslocation.DelFlag = "0";                                                         //�폜�t���O
                Newpartslocation.WarehouseCode = WarehouseCode;                                         //�q�ɃR�[�h
                db.PartsLocation.InsertOnSubmit(Newpartslocation);
            }
            else
            {
                //partslocation.PartsNumber = PartsNumber;                                              //���i�ԍ�
                partslocation.DepartmentCode = "";                                                      //����R�[�h ���󕶎���ݒ�
                partslocation.LocationCode = LocationCode;                                              //���P�[�V�����R�[�h
                partslocation.LastUpdateDate = DateTime.Now;                                            //�ŏI�X�V��
                partslocation.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;    //�ŏI�X�V��
                partslocation.WarehouseCode = WarehouseCode;                                            //�q�ɃR�[�h
            }
        }

        #endregion

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region �݌ɐ��ʂ��X�V
        /// <summary>
        /// �݌ɐ��ʂ��X�V
        /// </summary>
        /// <param name="purchase">�d���f�[�^</param>
        /// <history>
        /// 2018/05/14 arc yano #3880 ���㌴���v�Z�y�ђI���]���@�̕ύX �ړ����ϒP���̌v�Z
        /// 2017/02/02 arc yano #3857 ���i���ד��� �T�[�r�X�`�[���甭���������i�̓��׏���
        /// 2017/02/08 arc yano #3620 �T�[�r�X�`�[���́@�`�[�ۑ��A�폜�A�ԓ`���̕��i�̍݌ɂ̖߂��Ή� �폜�f�[�^
        private void UpdatePartsStock(string PartsNumber, string LocationCode, decimal? PurchaseQuantity, string PurchaseType, decimal? PurchasePrice)
        {

            //�݌ɏ��̎擾(�폜�σf�[�^���擾����)
            PartsStock partsstock = new PartsStockDao(db).GetByKey(PartsNumber, LocationCode, true);   //Mod 2017/02/08 arc yano #3620
            if (partsstock == null)
            {
                PartsStock NewPartsStock = new PartsStock();
                NewPartsStock.PartsNumber = PartsNumber;                                             //���i�ԍ�
                NewPartsStock.LocationCode = LocationCode;                                           //���P�[�V�����R�[�h 
                NewPartsStock.Quantity = PurchaseQuantity;                                           //����
                NewPartsStock.ProvisionQuantity = 0;                                                 //�����ϐ�
                NewPartsStock.CreateDate = DateTime.Now;                                             //�쐬��
                NewPartsStock.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
                NewPartsStock.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                NewPartsStock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                NewPartsStock.DelFlag = "0";
                db.PartsStock.InsertOnSubmit(NewPartsStock);
            }
            else
            {
                //Add 2017/02/08 arc yano #3620
                //Del 2016/04/22 arc yano #3506 �L�[���ڂ̍X�V���~��
                //partsstock.PartsNumber = PartsNumber;                                            //���i�ԍ�
                //partsstock.LocationCode = LocationCode;                                          //���P�[�V�����R�[�h
                //���ד`�[�敪��"����"�̎��͐��ʂɉ��Z�A"�ԕi"�̏ꍇ�͐��ʂ��猸�Z
                

                //�폜�f�[�^�̏ꍇ�͏�����
                partsstock = new PartsStockDao(db).InitPartsStock(partsstock);

                if (PurchaseType.Equals("001"))                                                   //����
                {
                    partsstock.Quantity = partsstock.Quantity + PurchaseQuantity;
                }
                else
                {
                    partsstock.Quantity = partsstock.Quantity - PurchaseQuantity;
                }
                partsstock.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                partsstock.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                partsstock.DelFlag = "0";
            }

            //Add 2018/05/14 arc yano #3880 //�ړ����ϒP���̍X�V
            new PartsMovingAverageCostDao(db).UpdateAverageCost(PartsNumber, "001", (!string.IsNullOrWhiteSpace(PurchaseType) && PurchaseType.Equals("002") ? PurchaseQuantity * -1 : PurchaseQuantity), PurchasePrice, ((Employee)Session["Employee"]).EmployeeCode);            

            db.SubmitChanges(); //Mod 2017/02/02 arc yano #3857
        }
        #endregion

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region �f�[�^�t����ʃR���|�[�l���g���쐬
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g���쐬
        /// </summary>
        /// <param name="purchase">�d���`�[�f�[�^</param>
        /// <history>
        /// 2018/11/12 yano #3949 ���i���ד��́@���׍σf�[�^�̕ҏW
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή� 
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// 2017/08/02 arc yano #3783 ���i���ד��� ���׃L�����Z���̏ꍇ�͓��׎�ʁ��ԕi���I�������悤�ɐݒ�
        /// 2016/06/27 arc yano #3585 �����ǉ�
        /// 2016/06/20 arc yano #3585 ���׃X�e�[�^�X(PurchaseStatus)�̐ݒ菈���ǉ�
        /// </history>
        private void SetEntryDataComponent(FormCollection form, List<GetPartsPurchaseList_Result> list = null)
        {

            //Add 2018/01/15 arc yano #3833
            //�����i�̃f�t�H���g�d�����ݒ�
            ViewData["GenuineSupplierCode"] = form["GenuineSupplierCode"];
            ViewData["GenuineSupplierName"] = form["GenuineSupplierName"];

            //Add 2018/06/01 arc yano #3894
            ViewData["JLRGenuineSupplierCode"] = form["JLRGenuineSupplierCode"];
            ViewData["JLRGenuineSupplierName"] = form["JLRGenuineSupplierName"];

            //�����Ƃ��ē��׃��R�[�h�̃��X�g���n����Ă���ꍇ�́A���̃��X�g������viewData��ݒ肷��
            if (list != null)
            {
                PartsPurchase ret = new PartsPurchaseDao(db).GetByKey(list[0].PurchaseNumber);

                ViewData["DepartmentCode"] = ret.DepartmentCode;

                ViewData["DepartmentName"] = ret.Department != null ? ret.Department.DepartmentName : "";

                ViewData["EmployeeCode"] = ret.EmployeeCode;

                ViewData["EmployeeName"] = ret.Employee != null ? ret.Employee.EmployeeName : "";

                ViewData["PurchaseDate"] = string.Format("{0:yyyy/MM/dd}", ret.PurchaseDate);
                ViewData["PurchaseType"] = ret.PurchaseType;

                ViewData["PurchaseStatus"] = ret.PurchaseStatus;

                CodeDao dao = new CodeDao(db);
                ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), ret.PurchaseType, false);
            }
            else
            {
                ViewData["DepartmentCode"] = form["DepartmentCode"];
                if (!string.IsNullOrEmpty(form["DepartmentCode"]))
                {
                    ViewData["DepartmentName"] = new DepartmentDao(db).GetByKey(form["DepartmentCode"]).DepartmentName;
                }
                ViewData["EmployeeCode"] = form["EmployeeCode"];
                if (!string.IsNullOrEmpty(form["EmployeeCode"]))
                {
                    ViewData["EmployeeName"] = new EmployeeDao(db).GetByKey(form["EmployeeCode"]).EmployeeName;
                }
                ViewData["PurchaseDate"] = form["PurchaseDate"];

                ViewData["PurchaseType"] = (string.IsNullOrWhiteSpace(form["PurchaseType"]) ? form["hdPurchaseType"] : form["PurchaseType"]);

                ViewData["PurchaseStatus"] = form["PurchaseStatus"]; //Add 2016/06/20 arc yano #3585

                CodeDao dao = new CodeDao(db);
                ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), form["PurchaseType"], false);
            }

            //Add 2017/08/02 arc yano #3783
            if (!string.IsNullOrWhiteSpace(form["PurchaseDate"]) && !string.IsNullOrWhiteSpace(form["DepartmentCode"]))
            {
               
                DateTime purchaseDate = DateTime.Parse(ViewData["PurchaseDate"].ToString());
                DateTime inventoryMonth = new DateTime(purchaseDate.Year, purchaseDate.Month, 1);

                InventoryScheduleParts condition = new InventoryScheduleParts();

                condition.InventoryMonth = inventoryMonth;
                condition.WarehouseCode = new DepartmentWarehouseDao(db).GetByDepartment(form["DepartmentCode"]).WarehouseCode;
                condition.InventoryType = "002";

                InventoryScheduleParts rec = new InventorySchedulePartsDao(db).GetByKey(condition);

                ViewData["InventoryStatus"] = rec != null ? rec.InventoryStatus : "";

                //���׃X�e�[�^�X���u���׍ρv���I���X�e�[�^�X=�u�m��v
                if (rec != null && rec.InventoryStatus.Equals("002") && !string.IsNullOrWhiteSpace(form["PurchaseStatus"]) && form["PurchaseStatus"].Equals("002"))
                {
                    //CodeDao dao = new CodeDao(db);
                    //ViewData["PurchaseType"] = "002";
                    //ViewData["PurchaseTypeList"] = CodeUtils.GetSelectListByModel<c_PurchaseType>(dao.GetPurchaseTypeAll(false), "002", false);

                    //Mod 2018/11/12 yano #3949
                    ViewData["CancelPurchaseDate"] = form["CancelPurchaseDate"];
                    ViewData["CancelEmployeeCode"] = form["CancelEmployeeCode"];

                    if (!string.IsNullOrWhiteSpace(form["CancelEmployeeCode"]))
                    {
                        ViewData["CancelEmployeeName"] = new EmployeeDao(db).GetByKey(form["CancelEmployeeCode"]).EmployeeName;
                    }
                }
            }

        }
        #endregion        

        //Mod 2015/11/20 arc nakayama #3293_���i���ד���(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region Validation�`�F�b�N
        /// <summary>
        /// ���׊m�莞��Validateion�`�F�b�N
        /// </summary>
        /// <param name="purchase">�d���`�[�f�[�^</param>
        /// <history>
        /// 2021/02/22 yano #4075 �y���i���ׁz���׊m�菈���̒��߃`�F�b�N�R��
        /// 2017/03/30 arc yano #3740 ���i���ד��́@�����ϕ��i�̕ԕi���s���� �t���[�݌ɐ��Ń`�F�b�N����悤�ɏC��
        /// 2016/03/22 arc yano Validation�`�F�b�N�̏����̕s��̏C��
        /// </history>
        private void ValidateSave(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //�X�V�f�[�^�����邩�Ȃ���
            if (Purchase == null || Purchase.Count <= 0)
            {
                ModelState.AddModelError("", "�X�V�Ώۂ̃f�[�^������܂���");
                return;
            }

            //--���ʍ���
            //--�K�{�`�F�b�N

            //�S����
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            //����R�[�h
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            //���ד�
            if (string.IsNullOrEmpty(form["PurchaseDate"]))
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0001", "���ד�"));
            }

            //--�ύ���
            for (int i = 0; i < Purchase.Count; i++)
            {
                //���א��������͂̏ꍇ�͑ΏۊO�i��֕��i�̌����R�[�h�����̑ΏۂɂȂ�j
                if (Purchase[i].PurchaseQuantity != null && Purchase[i].PurchaseQuantity != 0)
                {
                    //--�K�{�`�F�b�N
                    //���i�ԍ�
                    if (string.IsNullOrEmpty(Purchase[i].PartsNumber))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PartsNumber", MessageUtils.GetMessage("E0001", "���ו��i�ԍ�"));
                    }
                    //���P�[�V����
                    if (string.IsNullOrEmpty(Purchase[i].LocationCode))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].LocationCode", MessageUtils.GetMessage("E0001", "���P�[�V����"));
                    }
                    //���א�
                    if (Purchase[i].PurchaseQuantity == null)
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0001", "���א�"));
                    }
                    //���גP��
                    if (Purchase[i].Price == null)
                    {
                        ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0001", "���גP��"));
                    }
                    //�d����
                    if (string.IsNullOrEmpty(Purchase[i].SupplierCode))
                    {
                        ModelState.AddModelError("Purchase[" + i + "].SupplierCode", MessageUtils.GetMessage("E0001", "�d����"));
                    }

                    //--�`���`�F�b�N
                    //���א�
                    if ((Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}$"))))
                    {
                        if (Purchase[i].PurchaseQuantity == 0)  //���ʂ�0�̏ꍇ
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "���א�", "0�ȊO�̐��̐���7���ȓ�������2���ȓ�" }));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "���א�", "���̐���7���ȓ�������2���ȓ�" }));
                    }

                    //���גP��
                    if ((Regex.IsMatch(Purchase[i].Price.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].Price.ToString(), @"^\d{1,7}$"))))
                    {
                        if (Purchase[i].Price == 0)  //���ʂ�0�̏ꍇ
                        {
                            ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "���גP��", "0�ȊO�̐��̐���7���ȓ�������2���ȓ�" }));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Purchase[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "���גP��", "���̐���7���ȓ�������2���ȓ�" }));
                    }


                    if (!string.IsNullOrEmpty(form["Purchase[" + i + "].ChangePartsFlag"]) && form["Purchase[" + i + "].ChangePartsFlag"].Equals("1"))
                    {
                        Purchase[i].ChangeParts = true;
                    }

                    //--���݃`�F�b�N

                    //�����`�[�ԍ�
                    if (!string.IsNullOrEmpty(Purchase[i].PurchaseOrderNumber))
                    {
                        PartsPurchaseOrder purchaseorder = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber);    //Mod 2016/03/22 arc yano
                        //Mod 2016/04/25 arc nakayama #3494_���i���ד��͉�ʁ@�������̂Ȃ����׃f�[�^�ł̃G���[
                        //PartsPurchaseOrder purchaseorder = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber);
                        if (purchaseorder == null)
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseOrderNumber", "���͂��ꂽ�����`�[�ԍ��͑��݂��Ă��܂���");
                        }
                        else
                        {
                            PartsPurchaseOrder PurchaseorderPartsSet = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, Purchase[i].PartsNumber);

                            //���׍ς݃`�F�b�N
                            if (PurchaseorderPartsSet != null)
                            {
                                if (PurchaseorderPartsSet.RemainingQuantity <= 0)
                                {
                                    ModelState.AddModelError("", "�����`�[�ԍ� " + Purchase[i].PurchaseOrderNumber + " ���i�ԍ� " + Purchase[i].PartsNumber + " �̃��R�[�h�͂��łɓ��׍ςł��B");
                                }
                            }
                        }
                    }

                    //�������i�ԍ�
                    if (Purchase[i].ChangeParts)
                    {
                        if (string.IsNullOrEmpty(form["Purchase[" + i + "].ChangePartsNumber"]))
                        {
                            ModelState.AddModelError("ChangePartsNumber[" + i + "]", "�`�F�b�N�������Ă��܂����A�������i�ԍ��������͂ł�");
                        }

                    }
                    //���ד`�[�敪���u�ԕi�v�̏ꍇ
                    if (form["PurchaseType"] == "002")
                    {
                        PartsStock Pstock = new PartsStockDao(db).GetByKey(Purchase[i].PartsNumber, Purchase[i].LocationCode);
                        if (Pstock == null)
                        {
                            ModelState.AddModelError("Purchase[" + i + "].LocationCode", "�ԕi���镔�i�݌ɂ��A���͂���Ă��郍�P�[�V�����ɑ��݂��܂���");
                        }
                        else if ((Pstock.Quantity - Pstock.ProvisionQuantity) < Purchase[i].PurchaseQuantity) //Mod 2017/03/30 arc yano #3740
                        {
                            ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "�w��̃��P�[�V�������ɁA���͂��ꂽ�ԕi���ʕ��̍݌ɂ�����܂���");
                        }
                    }
                }
            }

            //--�����`�F�b�N
            if (ModelState.IsValidField("PurchaseDate") && Plist.PurchaseDate != null)
            {
                // Mod 2015/04/20 arc yano ���i�n�̃`�F�b�N�͌o�����A���i�I�����ꂼ��Œ����菈�����s���B�܂��o��������ł́A�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��ύX�\�Ƃ���
                // Mod 2015/04/15 arc yano�@���O�C�����[�U���o���ۂ̏ꍇ�́A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                //--�o��������
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���ד��ł͓��ׂł��܂���");
                }
                else //--���i�I������
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        //Mod 2021/02/22 yano #4075
                        //�q�ɃR�[�h�̎擾
                        DepartmentWarehouse dw = new DepartmentWarehouseDao(db).GetByDepartment(Plist.DepartmentCode);

                        // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                        //if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        {
                            ModelState.AddModelError("PurchaseDate", "���i�I���������I�����Ă���̂Ŏw�肳�ꂽ���ד��ł͓��ׂł��܂���");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ���׃L�����Z������Validateion�`�F�b�N
        /// </summary>
        /// <param name="purchase">�d���`�[�f�[�^</param>
        /// <history>
        /// 2017/08/02 arc yano #3783 ���i���ד��� ���׎���E�L�����Z���@�\�@�V�K�쐬
        /// </history>
        private void ValidateCancel(PartsPurchase_PurchaseList Plist, List<GetPartsPurchaseList_Result> Purchase, FormCollection form)
        {
            //����R�[�h
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            //���ד�
            if (string.IsNullOrEmpty(form["PurchaseDate"]))
            {
                ModelState.AddModelError("PurchaseDate", MessageUtils.GetMessage("E0001", "���ד�"));
            }

            for (int i = 0; i < Purchase.Count; i++)
            {
                //------------------
                //���א�
                //------------------
                //�K�{�`�F�b�N
                if (Purchase[i].PurchaseQuantity == null)
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0001", "���א�"));
                }

                //�`���`�F�b�N
                if ((Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(Purchase[i].PurchaseQuantity.ToString(), @"^\d{1,7}$"))))
                {
                    if (Purchase[i].PurchaseQuantity == 0)  //���ʂ�0�̏ꍇ
                    {
                        ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "���א�", "0�ȊO�̐��̐���7���ȓ�������2���ȓ�" }));
                    }
                }
                else
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", MessageUtils.GetMessage("E0002", new string[] { "���א�", "���̐���7���ȓ�������2���ȓ�" }));
                }

                //-------------------------------------------------------------------------
                // �����f�[�^�`�F�b�N �����ɔ��������L�����Z������Ă���ꍇ�̓G���[�Ƃ���
                //-------------------------------------------------------------------------
                PartsPurchaseOrder po = new PartsPurchaseOrderDao(db).GetByKey(Purchase[i].PurchaseOrderNumber, Purchase[i].PartsNumber);

                //�L�����Z���� > �����c���̏ꍇ�̓G���[
                if (po != null && (Purchase[i].PurchaseQuantity > (po.Quantity - po.RemainingQuantity) ))
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "���׍ϐ����ȏ�̃L�����Z���͍s���܂���");
                }

                //-------------------------------------------------------------
                //--���ׂ������i�����Ɉ�������Ă���ꍇ�̓G���[�Ƃ���
                //-------------------------------------------------------------
                //���ו��i�̍݌ɏ����擾����
                PartsStock rec = new PartsStockDao(db).GetByKey(Purchase[i].PartsNumber, Purchase[i].LocationCode, false);

                decimal? psQuantity = 0m;                   //����
                decimal? psProvisionQuantity = 0m;          //�����ϐ�
                
                if (rec != null)
                {
                    psQuantity = (rec.Quantity ?? 0);
                    psProvisionQuantity = (rec.ProvisionQuantity ?? 0);
                }
              
                //���א����t���[�݌ɐ���葽���ꍇ�͍폜�ł��Ȃ�
                if (Purchase[i].PurchaseQuantity > (psQuantity - psProvisionQuantity))
                {
                    ModelState.AddModelError("Purchase[" + i + "].PurchaseQuantity", "�Ώۂ̃��P�[�V�����ɕ��i�̍݌ɂ��Ȃ����߁A����ł��܂���");
                }
            }

            //--�����`�F�b�N
            if (ModelState.IsValidField("PurchaseDate") && Plist.PurchaseDate != null)
            {
                //--�o��������
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(Plist.DepartmentCode, DateTime.Parse(form["PurchaseDate"]), "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    ModelState.AddModelError("PurchaseDate", "�������ߏ������I�����Ă���̂Ŏw�肳�ꂽ���ד��ł̓L�����Z���ł��܂���");
                }
                else //--���i�I������
                {
                    ApplicationRole ret = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "EditTempClosedData");
                    if ((ret != null) && (ret.EnableFlag == false))
                    {
                        //�q�ɃR�[�h�̎擾
                        DepartmentWarehouse dw = new DepartmentWarehouseDao(db).GetByDepartment(Plist.DepartmentCode);

                        if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(dw.WarehouseCode, DateTime.Parse(form["PurchaseDate"]), "002"))
                        {
                            ModelState.AddModelError("PurchaseDate", "���i�I���������I�����Ă���̂Ŏw�肳�ꂽ���ד��ł̓L�����Z���ł��܂���");
                        }
                    }
                }
            }
        }


        #endregion

        // Add 2015/12/09 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region Excel�捞����
        /// <summary>
        /// Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        [AuthFilter]
        public ActionResult ImportDialog()
        {
            List<PurchaseExcelImportList> ImportList = new List<PurchaseExcelImportList>();
            FormCollection form = new FormCollection();
            ViewData["ErrFlag"] = "1";

            return View("PartsPurchaseImportDialog", ImportList);
        }

        /// <summary>
        /// Excel�ǂݍ���
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, List<PurchaseExcelImportList> ImportLine, FormCollection form)
        {
            List<PurchaseExcelImportList> ImportList = new List<PurchaseExcelImportList>();

            switch (CommonUtils.DefaultString(form["RequestFlag"]))
            {
                //--------------
                //Excel�ǂݍ���
                //--------------
                case "1":
                    //Excel�ǂݍ��ݑO�̃`�F�b�N
                    ValidateImportFile(importFile);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    //Excel�ǂݍ���
                    ImportList = ReadExcelData(importFile, ImportList);

                    //�ǂݍ��ݎ��ɉ����G���[������΂����Ń��^�[��
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    //Excel�œǂݍ��񂾃f�[�^�̃o���f�[�g�`�F�b�N
                    ValidateImportList(ImportList);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form); ;
                        return View("PartsPurchaseImportDialog", ImportList);
                    }

                    form["ErrFlag"] = "0";
                    SetDialogDataComponent(form);
                    return View("PartsPurchaseImportDialog", ImportList);

                //--------------
                //Excel��荞��
                //--------------
                case "2":

                    DBExecute(ImportLine, form);
                    form["ErrFlag"] = "1"; //��荞�񂾌�ɍēx[��荞��]�{�^���������Ȃ��悤�ɂ��邽��
                    SetDialogDataComponent(form); 
                    return View("PartsPurchaseImportDialog", ImportList);
                //--------------
                //�L�����Z��
                //--------------
                case "3":

                    ImportList = new List<PurchaseExcelImportList>();
                    form = new FormCollection();
                    ViewData["ErrFlag"] = "1";//[��荞��]�{�^���������Ȃ��悤�ɂ��邽��

                    return View("PartsPurchaseImportDialog", ImportList);
                //----------------------------------
                //���̑�(�����ɓ��B���邱�Ƃ͂Ȃ�)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("PartsPurchaseImportDialog", ImportList);
            }
        }
        #endregion

       
        #region Excel�f�[�^�擾&�ݒ�
        /// Excel�f�[�^�擾&�ݒ�
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// Mod 2016/03/03 arc yano #3413 ���i�}�X�^ ���[�J�[���i�ԍ��̏d�� �擾���ڒǉ�(�ڋq�R�[�h)
        /// Add 2015/12/14 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        /// </history>
        private List<PurchaseExcelImportList> ReadExcelData(HttpPostedFileBase importFile, List<PurchaseExcelImportList> ImportList)
        {
            //�J�����ԍ��ۑ��p
            int[] colNumber;
            colNumber = new int[9] { -1, -1, -1, -1, -1, -1, -1, -1, -1 };      //Mod 2016/03/03 arc yano #3413

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
                var ws = pck.Workbook.Worksheets["Page 1"];

                //Add 2016/03/28 arc nakayama Excel�ǂݍ��ݎ��ɃV�[�g�����قȂ�Ɨ�����Ή�
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
                int StartRow = ws.Dimension.Start.Row;�@ //�s�̊J�n�ʒu
                int EndRow = ws.Dimension.End.Row;       //�s�̏I���ʒu
                int StartCol = ws.Dimension.Start.Column;//��̊J�n�ʒu
                int EndCol = ws.Dimension.End.Column;    //��̏I���ʒu

                var headerRow = ws.Cells[StartRow, StartCol, StartRow, EndCol];
                colNumber = SetColNumber(headerRow, colNumber);
                //�^�C�g���s�A�w�b�_�s�����������ꍇ�͑����^�[������
                if (!ModelState.IsValid)
                {
                    return ImportList;
                }

                //------------------------------
                // �ǂݎ�菈��
                //------------------------------
                int datarow = 0;
                string[] Result = new string[colNumber.Count()];        //Mod 2016/03/03 arc yano #3413 

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    PurchaseExcelImportList data = new PurchaseExcelImportList();

                    //�X�V�f�[�^�̎擾
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)    //Mod 2016/03/03 arc yano #3413 
                    {

                        for (int i = 0; i < colNumber.Count(); i++)
                        {

                            if (col == colNumber[i])
                            {
                                Result[i] = ws.Cells[datarow, col].Text;
                                break;
                            }
                        }
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

        // Add 2015/12/14 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region �捞�t�@�C�����݃`�F�b�N
        /// <summary>
        /// �捞�t�@�C�����݃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private void ValidateImportFile(HttpPostedFileBase filePath)
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
        /// 2017/02/20 arc yano #3641 ���z���̃J���}�\���Ή� validation�̓J���}���������`�Ń`�F�b�N
        /// 2015/12/14 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        /// </history>
        public void ValidateImportList(List<PurchaseExcelImportList> ImportList)
        {
            for (int i = 0; i < ImportList.Count; i++)
            {
                //--------------
                // �K�{�`�F�b�N
                //--------------

                //�C���{�C�X�ԍ�
                if (string.IsNullOrEmpty(ImportList[i].InvoiceNo))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].InvoiceNo", MessageUtils.GetMessage("E0001", i + 1 +"�s�ڂ̃C���{�C�X�ԍ������͂���Ă��܂���B�C���{�C�X�ԍ�"));
                }
                //���[�J�[���i�ԍ�
                if (string.IsNullOrEmpty(ImportList[i].MakerPartsNumber))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].MakerPartsNumber", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̃��[�J�[���i�ԍ������͂���Ă��܂���B���[�J�[���i�ԍ�"));
                }
                //�d���P��
                if (ImportList[i].Price == null)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̎d���P�������͂���Ă��܂���B�d���P��"));
                }
                //����
                if (ImportList[i].Quantity == null)
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Quantity", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̐��ʂ����͂���Ă��܂���B����"));
                }
                //�}�X�^�`�F�b�N�@���i
                if (string.IsNullOrEmpty(ImportList[i].PartsNumber))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].MakerPartsNumber", i + 1 + "�s�ڂ̃��[�J�[���i�ԍ�" + ImportList[i].MakerPartsNumber + "�͕��i�}�X�^�ɓo�^����Ă��܂���B�}�X�^�o�^���s���Ă���ēx���s���ĉ������B");
                }

                // Mod 2017/02/20 arc yano #3641
                /*
                //�P��
                if (!Regex.IsMatch(ImportList[i].Price, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Price, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "�P��", i + 1 + "�s�ڂ̒P��������������܂���B���̐���7���ȓ�������2���ȓ�" }));
                }
                */

                string workPrice = string.IsNullOrWhiteSpace(ImportList[i].Price) ? "" : ImportList[i].Price.Replace(",", "");

                //�P��
                if (!Regex.IsMatch(workPrice, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(workPrice, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Price", MessageUtils.GetMessage("E0002", new string[] { "�P��", i + 1 + "�s�ڂ̒P��������������܂���B���̐���7���ȓ�������2���ȓ�" }));
                }
                //����
                if (!Regex.IsMatch(ImportList[i].Quantity, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Quantity, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", i + 1 + "�s�ڂ̐��ʂ�����������܂���B���̐���7���ȓ�������2���ȓ�" }));
                }
                // Mod 2017/02/20 arc yano #3641
                /*
                //���z
                if (!Regex.IsMatch(ImportList[i].Amount, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(ImportList[i].Amount, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", i + 1 + "�s�ڂ̋��z������������܂���B���̐���7���ȓ�������2���ȓ�" }));
                }
                */

                string workAmount = string.IsNullOrWhiteSpace(ImportList[i].Amount) ? "" : ImportList[i].Amount.Replace(",", "");

                if (!Regex.IsMatch(workAmount, @"^\d{1,7}\.\d{1,2}$") && !Regex.IsMatch(workAmount, @"^\d{1,7}$"))
                {
                    ModelState.AddModelError("ImportLine[" + i + "].Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", i + 1 + "�s�ڂ̋��z������������܂���B���̐���7���ȓ�������2���ȓ�" }));
                }
            }

        }
        #endregion

        // Add 2015/12/14 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region �e���ڂ̗�ԍ��ݒ�
        /// <summary>
        /// �e���ڂ̗�ԍ��ݒ�
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/03/03 arc yano #3413 ���i�}�X�^ ���[�J�[���i�ԍ��̏d�� �擾���ڒǉ�(�ڋq�R�[�h)
        /// </history>
        private int[] SetColNumber(ExcelRangeBase headerRow, int[] colNumber)
        {
            //��������
            int cnt = 1;

            //��ԍ��ݒ�
            foreach (var cell in headerRow)
            {

                if (cell != null)
                {
                    //�C���{�C�X�ԍ�
                    if (cell.Text.Contains("�C���{�C�X�ԍ�"))
                    {
                        colNumber[0] = cnt;
                    }
                    //���t
                    if (cell.Text.Contains("���t"))
                    {
                        colNumber[1] = cnt;
                    }
                    //�I�[�_�[�ԍ�
                    if (cell.Text.Contains("�I�[�_�[�ԍ�"))
                    {
                        colNumber[2] = cnt;
                    }
                    //�����`�[�ԍ��@Ref. Number[572753]
                    if (cell.Text.Contains("Ref. Number[572753]"))
                    {
                        colNumber[3] = cnt;
                    }
                    //���[�J�[���i�ԍ��@Part N.[572755]
                    if (cell.Text.Contains("Part N.[572755]"))
                    {
                        colNumber[4] = cnt;
                    }
                    //���ח\�萔�@Req. quantity[572759]
                    if (cell.Text.Contains("Req. quantity[572759]"))
                    {
                        colNumber[5] = cnt;
                    }
                    //�P�� �d�؉��i
                    if (cell.Text.Contains("�d�؉��i"))
                    {
                        colNumber[6] = cnt;
                    }
                    //���z�@���v���z
                    if (cell.Text.Contains("���v���z"))
                    {
                        colNumber[7] = cnt;
                    }
                    //�ڋq    //Mod 2016/03/03 arc yano #3413 
                    if (cell.Text.Contains("�ڋq"))
                    {
                        colNumber[8] = cnt;
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
        #endregion

        // Add 2015/12/14 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region Excel�̓ǂݎ�茋�ʂ����X�g�ɐݒ肷��
        /// <summary>
        /// ���ʂ����X�g�ɐݒ肷��
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/01/15 arc yano #3832 ���i���ׁ@LinkEntry��[�I�[�_�[�ԍ�]�𕔕i���׃f�[�^��[���[�J�[�I�[�_�[�ԍ�]�ɐݒ�
        /// 2017/02/20 arc yano #3641 ���z���̃J���}�\���Ή� ���z�̍��ڂ̓J���}����菜�����ɐݒ肷��
        /// 2016/03/03 arc yano #3468 �����f�[�^�̂Ȃ����i�̓��׏����̏ꍇ�A�����InvoiceAfterSales�̌ڋq���擾����
        /// 
        /// 2016/03/03 arc yano #3413 ���i�}�X�^ ���[�J�[���i�ԍ��̏d�� �擾���ڒǉ�(�ڋq�R�[�h) ���i�f�[�^�擾�̃��\�b�h��
        ///                         �@�@�@�ڋq�R�[�h��ǉ�
        /// 2016/02/26 arc yano #3432 ���i�d���@LinkEntry�A�ԕi�s�̎捞�X�L�b�v�Ή�
        /// �@�@�@�@�@�@�@�@�@�@�@ �@�@�@�@�@�@�@�@���v���z���}�C�i�X�̏ꍇ�̓��X�g�̐ݒ���X�L�b�v����B
        /// 2016/01/21 arc yano #3404_Excel�捞���̏����敪�̍i���ݒǉ�(#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�)
        ///                         ���i�}�X�^���������ɏ����敪 = �u�����i�v�ǉ�
        /// </history>
        public List<PurchaseExcelImportList> SetProperty(ref string[] Result, ref List<PurchaseExcelImportList> ImportList)
        {
            PurchaseExcelImportList SetLine = new PurchaseExcelImportList();

            // �C���{�C�X�ԍ�
            SetLine.InvoiceNo = Result[0];

            // ���ח\���
            SetLine.PurchasePlanDate = Result[1];

            
            // ���[�J�[�I�[�_�[�ԍ�
            //SetLine.WebOrderNumber = Result[2];
            SetLine.MakerOrderNumber = Result[2];

            // �����`�[�ԍ�
            SetLine.PurchaseOrderNumber = Result[3];

            // ���[�J�[���i�ԍ�
            SetLine.MakerPartsNumber = Result[4];

            Parts PartsData = new PartsDao(db).GetByMakerPartsNumber(SetLine.MakerPartsNumber, "001", Result[8]);  //Mod 2016/01/21 arc yano #3234
 
            if (PartsData != null)
            {
                // ���i�ԍ�
                SetLine.PartsNumber = PartsData.PartsNumber;

                // ���i��
                SetLine.PartsNameJp = PartsData.PartsNameJp;
            }
            else
            {
                SetLine.PartsNumber = "";
                SetLine.PartsNameJp = "";
            }

            // ����
            SetLine.Quantity = Result[5];

            //Mod 2017/02/20 arc yano #3641
            /*
            // �P��
            SetLine.Price = Result[6].Replace(",", "");

            // ���z
            SetLine.Amount = Result[7].Replace(",", "");
            */

            // �P��
            SetLine.Price = Result[6];

            // ���z
            SetLine.Amount = Result[7];


            //Add 2016/02/26 arc yano #3432 
            decimal workAmount;
            //������decimal�ɕϊ�
            bool ret = Decimal.TryParse(SetLine.Amount , out workAmount);

            if (ret != false)
            {
                if (workAmount < 0) //���z���}�C�i�X�̏ꍇ
                {
                    return ImportList;  //�����I��    
                }
            }

            //Mod 2016/03/14 arc yano #3468
            Department dep = new DepartmentDao(db).GetByLEUserCode(Result[8]);
       
            //�����`�[�ԍ��ƕ��i�ԍ����ݒ肳��Ă���Ƃ��̂ݎ擾����
            if (!string.IsNullOrEmpty(SetLine.PurchaseOrderNumber) && !string.IsNullOrEmpty(SetLine.PartsNumber))
            {
                PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(SetLine.PurchaseOrderNumber, SetLine.PartsNumber);
                if (PurchaseOrder != null)
                {
                    // �d����R�[�h
                    SetLine.SupplierCode = PurchaseOrder.SupplierCode;
                    // �d���於
                    SetLine.SupplierName = new SupplierDao(db).GetByKey(SetLine.SupplierCode).SupplierName ?? "";
                    // ����R�[�h
                    SetLine.DepartmentCode = PurchaseOrder.DepartmentCode;
                    // �󒍓`�[�ԍ�
                    SetLine.SlipNumber = PurchaseOrder.ServiceSlipNumber ?? "";
                    // �Ј��R�[�h
                    SetLine.EmployeeCode = PurchaseOrder.EmployeeCode;
                }
                else
                {
                    // �d����R�[�h
                    SetLine.SupplierCode = "";
                    // �d���於
                    SetLine.SupplierName = "";
                    // ����R�[�h        //Mod 2016/03/14 arc yano #3468
                    SetLine.DepartmentCode = (dep != null ? dep.DepartmentCode : "");
                    //SetLine.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
                    // �󒍓`�[�ԍ�
                    SetLine.SlipNumber = "";
                    // �Ј��R�[�h
                    SetLine.EmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

            }
            else
            {
                // �d����R�[�h
                SetLine.SupplierCode = "";
                // �d���於
                SetLine.SupplierName = "";
                // ����R�[�h        //Mod 2016/03/14 arc yano #3468
                SetLine.DepartmentCode = (dep != null ? dep.DepartmentCode : "");
                //SetLine.DepartmentCode = "";
                // �󒍓`�[�ԍ�
                SetLine.SlipNumber = "";
                // �Ј��R�[�h
                SetLine.EmployeeCode = "";
            }

            //���׃X�e�[�^�X(�����׌Œ�)
            SetLine.PurchaseStatus = "001"; //������
            SetLine.PurchaseStatusName = new CodeDao(db).GetPurchaseStatus("001", false).Name;

            ImportList.Add(SetLine);

            return ImportList;
        }
        #endregion

        // Add 2015/12/15 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region �_�C�A���O�̃f�[�^�t���R���|�[�l���g�ݒ�
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
        }
        #endregion

        #region �ǂݍ��񂾃f�[�^��DB�ɓo�^
        /// <summary>
        /// DB�X�V
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(���i�I����ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2018/03/26 arc yano #3863 ���i���ׁ@LinkEntry�捞���̒ǉ�
        /// 2018/01/15 arc yano #3832 ���i���ׁ@LinkEntry��[�I�[�_�[�ԍ�]�𕔕i���׃f�[�^��[���[�J�[�I�[�_�[�ԍ�]�ɐݒ�
        /// </history>
        private void DBExecute(List<PurchaseExcelImportList> ImportLine, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                string ChangePartsFlag = ""; //��֕��i�t���O


                //Add 2016/04/21 arc yano #3503
                List<PartsPurchase> workList = new List<PartsPurchase>();

                //�����e�[�u�����X�V���āA���׃e�[�u����o�^����
                foreach (var LineData in ImportLine)
                {   
                    
                    ChangePartsFlag = "";�@//������

                    //�C���{�C�X�ԍ��Ō��݂̓��׃e�[�u�����������āA�q�b�g������d������
                    PartsPurchase InvoiceRet = new PartsPurchaseDao(db).GetByInvoiceNo(LineData.InvoiceNo);
                    
                    //�q�b�g���Ȃ������ꍇ�̂�DB�ɓo�^
                    if (InvoiceRet == null)
                    {
                        //�����`�[�ԍ��ƕ��i�ԍ��������Ă���Ƃ������Y�����锭�����R�[�h���X�V����i����ȊO�͔����̂Ȃ����ׂƌ��Ȃ��j
                        if (!string.IsNullOrEmpty(LineData.PurchaseOrderNumber) && !string.IsNullOrEmpty(LineData.PartsNumber))
                        {
                            //----------------------------------------------------------------------------------
                            // �����e�[�u���X�V
                            // �X�V���A�����ƈقȂ镔�i�����ׂ����ꍇ�A��֕��i�t���O���Z�b�g����
                            //  �E�����`�[�ԍ��ƕ��i�ԍ��Ō������ăq�b�g������A�����ʂ�̕��i������
                            //�@�E�����`�[�ԍ��ƕ��i�ԍ��Ō������ăq�b�g���Ȃ�������A�����ƈقȂ镔�i������
                            //----------------------------------------------------------------------------------
                            PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(LineData.PurchaseOrderNumber, LineData.PartsNumber);
                            if (PurchaseOrder != null)
                            {
                                //PurchaseOrder.WebOrderNumber = LineData.WebOrderNumber;       //Del 2018/01/15 arc ynao #3832

                                ChangePartsFlag = "0";

                            }
                            else
                            {
                                ChangePartsFlag = "1";
                            }
                        }


                        //--------------------
                        // ���׃e�[�u���o�^
                        //--------------------
                        PartsPurchase NewPurchase = new PartsPurchase();
                        NewPurchase.PurchaseNumber = new SerialNumberDao(db).GetNewPurchaseNumber();       //���ד`�[�ԍ�
                        NewPurchase.PurchaseOrderNumber = LineData.PurchaseOrderNumber;                    //�����`�[�ԍ�
                        NewPurchase.PurchaseType = "001";                                                  //���ד`�[�敪
                        NewPurchase.PurchasePlanDate = DateTime.Parse(LineData.PurchasePlanDate);          //���ח\���
                        NewPurchase.PurchaseDate = null;                                                   //���ד�
                        NewPurchase.PurchaseStatus = "001";                                                //�d���X�e�[�^�X
                        NewPurchase.SupplierCode = LineData.SupplierCode ?? "";                            //�d����R�[�h
                        NewPurchase.EmployeeCode = LineData.EmployeeCode ?? "";                            //�Ј��R�[�h
                        NewPurchase.DepartmentCode = LineData.DepartmentCode ?? "";                        //����R�[�h
                        NewPurchase.LocationCode = "";                                                     //���P�[�V�����R�[�h
                        NewPurchase.PartsNumber = LineData.PartsNumber ?? "";                              //���i�ԍ�
                        NewPurchase.Price = decimal.Parse(LineData.Price);                                 //�P��
                        NewPurchase.Quantity = decimal.Parse(LineData.Quantity);                           //���א�
                        NewPurchase.Amount = decimal.Parse(LineData.Amount);                               //���z
                        NewPurchase.ReceiptNumber = "";                                                    //�[�i���ԍ�
                        NewPurchase.Memo = "";                                                             //����
                        NewPurchase.InvoiceNo = LineData.InvoiceNo;                                        //�C���{�C�X�ԍ�
                        NewPurchase.MakerOrderNumber = LineData.MakerOrderNumber;                          //���[�J�[�I�[�_�[�ԍ�     //Mod 2018/01/15 arc yano #3832
                        NewPurchase.CreateDate = DateTime.Now;                                             //�쐬��
                        NewPurchase.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;     //�쐬��
                        NewPurchase.LastUpdateDate = DateTime.Now;                                         //�ŏI�X�V��
                        NewPurchase.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode; //�ŏI�X�V��
                        NewPurchase.DelFlag = "0";
                        NewPurchase.RevisionNumber = 1;
                        NewPurchase.ChangePartsFlag = ChangePartsFlag;                                      //��֕��i�t���O   
                        NewPurchase.LinkEntryCaptureDate = DateTime.Today;                                  //�捞�ǉ�              //Add 2018/03/26 arc yano #3863

                        workList.Add(NewPurchase);

                        //db.PartsPurchase.InsertOnSubmit(NewPurchase);      //Mod 2016/04/21 arc yano #3503
                    }
                    else
                    {
                        //�q�b�g������A�捞��Ƀ��b�Z�[�W��\������
                        ModelState.AddModelError("", "�C���{�C�X�ԍ�" + LineData.InvoiceNo + "�͊��Ɏ�荞�܂�Ă��邽�߁A�捞�Ώۂ��珜�O����܂����B");
                    }
                }

                //Mod 2016/04/21 arc yano #3503 
                db.PartsPurchase.InsertAllOnSubmit(workList);


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

        // Add 2015/12/09 arc nakayama #3294_���i����Excel�捞�m�F(#3234_�y�區�ځz���i�d����@�\�̉��P)
        #region "�����e�[�u���o�^�ҏW"
        /// <summary>
        /// �����e�[�u���o�^�f�[�^�̕ҏW
        /// </summary>
        /// <param name="partsPurchase">�o�^�f�[�^</param>
        /// <returns> ADD 2014/09/10 arc amii ���i���ח���Ή� �V�K�ǉ�</returns>
        private PartsPurchaseHistory SetRegistHistoryData(PartsPurchase partsPurchase)
        {
            PartsPurchaseHistory history = new PartsPurchaseHistory();

            history.PurchaseNumber = partsPurchase.PurchaseNumber;                 // ���ד`�[�ԍ�
            history.PurchaseOrderNumber = partsPurchase.PurchaseOrderNumber;       // �����`�[�ԍ�
            history.PurchaseType = partsPurchase.PurchaseType;                     // �����敪
            history.PurchasePlanDate = partsPurchase.PurchasePlanDate;             // ���ח\���
            history.PurchaseDate = partsPurchase.PurchaseDate;                     // ���ד�
            history.PurchaseStatus = partsPurchase.PurchaseStatus;                 // �d���X�e�[�^�X
            history.SupplierCode = partsPurchase.SupplierCode;                     // �d����
            history.EmployeeCode = partsPurchase.EmployeeCode;                     // �S����
            history.DepartmentCode = partsPurchase.DepartmentCode;                 // ����R�[�h
            history.LocationCode = partsPurchase.LocationCode;                     // ���P�[�V�����R�[�h
            history.PartsNumber = partsPurchase.PartsNumber;                       // ���i�ԍ�
            history.Price = partsPurchase.Price;                                   // �d���P��
            history.Quantity = partsPurchase.Quantity;                             // ����
            history.Amount = partsPurchase.Amount;                                 // ���z
            history.ReceiptNumber = partsPurchase.ReceiptNumber;                   // �[�i���ԍ�
            history.Memo = partsPurchase.Memo;                                     // ���l
            history.CreateEmployeeCode = partsPurchase.CreateEmployeeCode;         // �쐬��
            history.CreateDate = partsPurchase.CreateDate;                         // �쐬��
            history.LastUpdateEmployeeCode = partsPurchase.LastUpdateEmployeeCode; // �ŏI�X�V��
            history.LastUpdateDate = partsPurchase.LastUpdateDate;                 // �ŏI�X�V����
            history.DelFlag = "1";                                                 // �폜�t���O
            history.ServiceSlipNumber = partsPurchase.ServiceSlipNumber;           // �T�[�r�X�`�[�ԍ�
            history.RevisionNumber = partsPurchase.RevisionNumber;                 // �����ԍ�


            return history;
        }

        #endregion

    }
}
