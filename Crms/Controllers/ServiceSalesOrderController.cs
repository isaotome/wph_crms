using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Crms.Models;

using OfficeOpenXml;            //Add 2017/10/19 arc yano #3803
using System.Configuration;     //Add 2017/10/19 arc yano #3803

namespace Crms.Controllers
{
    /// <summary>
    /// �T�[�r�X�`�[
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceSalesOrderController : InheritedController {

	//Mod 2014/07/12 arc yano chrome�Ή� �萔�N���X�̐錾�ʒu��ServiceSalesOrderController���ֈړ�
    //Add 2014/07/02 arc yano �萔��`

        /// <summary>
        ///�萔
        /// </summary>     
       static class Constants
        {
            //���ƃR�[�h
            public const string svWkWaranty = "10501";      //�������e�B
            public const string svWkRecall = "10502";       //���R�[��

            //������R�[�h
            public const string cClaimFCJWaranty = "A000100174";      //�t�B�A�b�g�@�N���C�X���[�@�W���p���e�b�i�������e�B�i�������߂e�h�`�s�^�`�k�e�`�������e�B��p�j
        }

        #region ������
        //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�T�[�r�X�`�[";     // ��ʖ�
        private static readonly string PROC_NAME_AKA = "�ԓ`"; // ������
        private static readonly string PROC_NAME_SAVE = "�T�[�r�X�`�[�ۑ�"; // ������
        private static readonly string PROC_NAME_AKA_KURO = "�ԍ�"; // ������

        private static readonly string PROC_NAME_PURCHASEORDER_DOWNLOAD = "�������o��";  // ������      //Add 2017/10/19 arc yano #3803


        //Add 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����݌ɂ̊Ǘ����@�̕ύX
        private static readonly string TYPE_GENUINE = "001";                //�����i
        private static readonly string TYPE_NONGENUINE = "002";            //�ЊO�i



        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ����\���t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// �݌ɏ����T�[�r�X
        /// </summary>
        private IStockService stockService;

        /// <summary>
        /// �T�[�r�X�`�[�����T�[�r�X
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceSalesOrderController() {
            db = new CrmsLinqDataContext();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db); 
        }

        /// <summary>
        /// ������ʏ����\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            return Criteria(new FormCollection());
        }
        #endregion

        #region �R���g���[���̗L������
        /// <summary>
        /// �`�[�X�e�[�^�X����\������VIEW��Ԃ�
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/03/22 yano #4078�y�T�[�r�X�`�[���́z�[�Ԋm�F���ŏo�͂��钠�[�̎�ނ𓮓I�ɍi��
        /// 2017/01/21 arc yano #3657  ���Ϗ��̌l���̕\���^��\���̃`�F�b�N�{�b�N�X�̕\���E��\����ݒ� 
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���ߏ����󋵃`�F�b�N�����̈����̕ύX(SlipData.DepartmentCode �� warehouseCode)
        /// </history>
        private ActionResult GetViewResult(ServiceSalesHeader header) {
            Employee loginUser = (Employee)Session["Employee"];

            string departmentCode = header.DepartmentCode;
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";

            header.SlipEnabled = false;
            header.CustomerEnabled = false;
            header.CarEnabled = false;
            header.LineEnabled = false;
            header.CostEnabled = false;
            header.PaymentEnabled = false;
            header.SalesDateEnabled = false;
            header.SalesPlanDateEnabled = false;�@ //ADD 2014/02/20 ookubo
            header.RateEnabled = false;�@          //ADD 2014/02/20 ookubo
            header.PInfoChekEnabled = false;       // Add 2017/01/21 arc yano #3657
            header.ClaimReportChecked = false   ;       //Add 2021/03/22 yano #4078

            if (ViewData["Mode"] != null && ViewData["Mode"].Equals("1"))
            {
                // �ԓ`
                header.ShowMenuIndex = 10;
            } else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("2")) {
                // �ԍ�
                header.SlipEnabled = true;
                header.CarEnabled = true;
                header.CustomerEnabled = true;
                header.LineEnabled = true;
                header.CostEnabled = true;
                header.PaymentEnabled = true;
                header.SalesDateEnabled = true;
                header.RateEnabled = true;              //ADD 2014/02/20 ookubo
                header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                header.ShowMenuIndex = 11;
            
                // 2012.03.21 �ύX
                // ���O�C���S���҂̏�������܂��͌�������łȂ���ΕҏW�s��
                // ���O�C���S���҂̃Z�L�����e�B���x����004�FALL�Ȃ�ҏW��
            } else if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004")) {
                header.ShowMenuIndex = 8;
            } else if (header.DelFlag != null && header.DelFlag.Equals("1")) { 
                header.ShowMenuIndex = 7;
                header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
            } else if (!string.IsNullOrEmpty(header.LockEmployeeName)){
                header.SlipEnabled = false;
                header.CarEnabled = false;
                header.CustomerEnabled = false;
                header.LineEnabled = false;
                header.CostEnabled = false;
                header.PaymentEnabled = false;
                header.ShowMenuIndex = 8;
                ViewData["ProcessSessionError"] = "���̓`�[�͌���" + header.LockEmployeeName + "���񂪎g�p���Ă��邽�ߓǂݎ���p�ŕ\�����Ă��܂�";
            } else {
                switch (header.ServiceOrderStatus) {
                    case "001": //����
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;�@ //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 1;
                        header.KeepsCarFlagEnabled = true;�@  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "002": //��
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;�@ //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 2;
                        header.KeepsCarFlagEnabled = true;�@  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "003": //��ƒ�
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;�@ //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 3;
                        header.KeepsCarFlagEnabled = true;�@  //ADD 2014/11/25 #3135 ookubo
                        header.PInfoChekEnabled = true; // Add 2017/01/21 arc yano #3657
                        break;
                    case "004": //��Ɗ���
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;�@ //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 4;
                        header.KeepsCarFlagEnabled = true;�@  //ADD 2014/11/25 #3135 ookubo
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        break;
                    case "005": //�[�ԑO
                        header.SlipEnabled = true;
                        header.CarEnabled = true;
                        header.CustomerEnabled = true;
                        header.LineEnabled = true;
                        header.CostEnabled = true;
                        header.PaymentEnabled = true;
                        header.SalesDateEnabled = true;
                        header.SalesPlanDateEnabled = true;�@ //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@          //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 5;
                        header.KeepsCarFlagEnabled = true;�@  //ADD 2014/11/25 #3135 ookubo
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        break;
                    case "006": //�[��
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ClaimReportChecked = true;       //Add 2021/03/22 yano #4078
                        //�ύX�O�̓`�[���擾
                        ServiceSalesHeader SlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                        // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                        // Mod 2015/04/01 arc nakayama �[�ԓ��`�F�b�N�Ή� �o�����������߈ȏ�ł��[�ԓ����ύX�ł��Ȃ��悤�ɂ���B
                        // Mod 2015/04/15 arc yano�@�T�[�r�X�n�͒�����A���i�I�����藼���s���B�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�ύX�\�Ƃ���
                        int ret = 0;

                        //Mod 2016/08/13 arc yano #3596
                        //����R�[�h����g�p�q�ɂ����o��
                        DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                        ret = CheckTempClosedEdit(dWarehouse, SlipData.SalesDate);

                        //ret = CheckTempClosedEdit(SlipData.DepartmentCode, SlipData.SalesDate);

                        //Mod 2015/05/07 arc yano �������ҏW�����ǉ� �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�ύX�\�Ƃ���
                        if (ret != 0) //������=�����߈ȏ�
                        {
                            header.SalesDateEnabled = false;
                        }
                        else
                        {
                            header.SalesDateEnabled = true;
                        }

                        header.ShowMenuIndex = 6;
                        header.KeepsCarFlagEnabled = false;�@ //ADD 2014/11/25 #3135 ookubo

                        // Add 2015/03/18 arc nakayama �`�[�C���Ή��@�ߋ��ɏC�����s�������̂���`�[�������ꍇ�A���R����\������
                        if(service.CheckModifiedReason(header.SlipNumber)){
                            header.ModifiedReasonEnabled = true; 
                            service.GetModifiedHistory(header); //�C�������擾
                        }else{
                            header.ModifiedReasonEnabled = false;
                        }

                        // Add 2015/03/17 arc nakayama �`�[�C���Ή��@���O�C�����[�U�[���x�X���E�V�X�e���Ǘ��ҁ@���@�ԓ`�܂��͉ߋ��ɐԍ��������s�������`�[�łȂ������ꍇ�͓`�[�C���{�^���\��
                        // ���O�C�����[�U�[���`�[�C���������������Ă���@���@�ԓ`�܂��͉ߋ��ɐԍ��������s�������`�[�łȂ������ꍇ�͓`�[�C���{�^���\��
                        if (service.CheckApplicationRole(loginUser.EmployeeCode) && !header.SlipNumber.Contains("-1") && service.AkakuroCheck(header.SlipNumber)){
                            header.ModificationEnabled = true;  //[�`�[�C��]�{�^���\��
                        }else{
                            header.ModificationEnabled = false; //[�`�[�C��]�{�^����\��
                        }

                        // Add 2015/03/17 arc nakayama �C�����������ꍇ�͏C�������{�^���ƏC���L�����Z���{�^���̕\����؂�ւ���
                        if (service.CheckModification(header.SlipNumber, header.RevisionNumber)){
                            header.ModificationControl = true; //�C����
                            header.ModificationControlCancel = true; //[�C���L�����Z��]�{�^�� �\��

                            //�C�����ł��o���̒��ߏ����󋵂������߂��{���߂Ȃ�ύX�͕s�ɂ���[�C���L�����Z��]�{�^����[����]�{�^���݂̂̕\���ɂȂ�B���R�����\������Ȃ�
                            // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                            //if (new InventoryScheduleDao(db).IsClosedInventoryMonth(SlipData.DepartmentCode, SlipData.SalesDate, "001"))
                            if (ret != 2)
                            {
                                header.SlipEnabled = true;
                                header.CarEnabled = true;
                                header.CustomerEnabled = true;
                                header.CostEnabled = true;
                                header.PaymentEnabled = true;
                                header.SalesDateEnabled = true;
                                
                                header.ModificationControlCommit = true;        // [�C������]�{�^�� �\��
                                header.LineEnabled = true;                      //�@���׍s�͕ύX�\
                                
                                //�C�����ł����i�̒I���󋵂��m��Ȃ疾�ׂ̕ύX�͕s�ɂ���
                                if (ret != 1)
                                {
                                    header.LineEnabled = true;  //���׍s�ҏW��
                                }
                                else
                                {
                                    header.LineEnabled = false; //���׍s�ҏW�s��
                                }
                            }
                            else
                            {
                                header.SlipEnabled = false;
                                header.CarEnabled = false;
                                header.CustomerEnabled = false;
                                header.LineEnabled = false;
                                header.CostEnabled = false;
                                header.PaymentEnabled = false;
                                header.SalesDateEnabled = false;
                                header.ModificationControlCommit = false;// [�C������]�{�^�� ��\��
                            }
                        }

                        break;
                    case "007": //�L�����Z��
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 8;
                        break;
                    case "009": //��Ɨ���
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 8;
                        break;
                    case "010": //��ƒ��~
                        header.SlipEnabled = false;
                        header.CarEnabled = false;
                        header.CustomerEnabled = false;
                        header.LineEnabled = false;
                        header.CostEnabled = false;
                        header.PaymentEnabled = false;
                        header.ShowMenuIndex = 9;
                        break;
                }

                //�Ǘ��Ҍ����̂ݏ���ŗ��g�p��
                Employee emp = HttpContext.Session["Employee"] as Employee;
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
                {
                    header.RateEnabled = false;
                }

            }
            if (ViewData["EntryMode"] != null){
                if (ViewData["EntryMode"].Equals("FullScreen")) {
                    return View("ServiceSalesLineEntry", header);
                } else {
                    return View("ServiceSalesOrderEntry", header);
                }
            }

            return View("ServiceSalesOrderEntry", header);
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/14 yano #3912 �T�[�r�X�`�[�����@���������ɓ��͂����C�x���g�̖��̂��������s��A������
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            //form["EmployeeCode"] = (form["EmployeeCode"] == null ? ((Employee)Session["Employee"]).EmployeeCode : form["EmployeeCode"]);
            form["ServiceOrderStatus"] = Request["status"] == null ? form["ServiceOrderStatus"] : Request["status"];
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;

            PaginatedList<ServiceSalesHeader> list = GetSearchResultList(form);
            ViewData["SlipNumber"] = form["SlipNumber"];
            ViewData["QuoteDateFrom"] = form["QuoteDateFrom"];
            ViewData["QuoteDateTo"] = form["QuoteDateTo"];
            ViewData["SalesOrderDateFrom"] = form["SalesOrderDateFrom"];
            ViewData["SalesOrderDateTo"] = form["SalesOrderDateTo"];
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["CarBrandName"] = form["CarBrandName"];
            ViewData["TelNumber"] = form["TelNumber"];
            ViewData["PlateNumber"] = form["PlateNumber"];
            ViewData["Vin"] = form["Vin"];
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["ServiceWorkCode"] = form["ServiceWorkCode"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CustomerClaimCode"] = form["CustomerClaimCode"];
            ViewData["CustomerClaimName"] = form["CustomerClaimName"];
            ViewData["CustomerNameKana"] = form["CustomerNameKana"];

            //�\�����ڂ̃Z�b�g
            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                ViewData["EmployeeName"] = (new EmployeeDao(db)).GetByKey(form["EmployeeCode"], true).EmployeeName;
            }
            if (!string.IsNullOrEmpty(form["ServiceWorkCode"])) {
                ViewData["ServiceWorkName"] = (new ServiceWorkDao(db)).GetByKey(form["ServiceWorkCode"]).Name;
            }
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                ViewData["DepartmentName"] = (new DepartmentDao(db)).GetByKey(form["DepartmentCode"], true).DepartmentName;
            }
            //Add 2018/08/14 yano #3912�@//�C�x���g���̐ݒ�
            if (!string.IsNullOrEmpty(form["CampaignCode"]))
            {
                ViewData["CampaignName"] = (new CampaignDao(db)).GetByKey(form["CampaignCode"]).CampaignName;
            }

            CodeDao dao = new CodeDao(db);
            ViewData["ServiceOrderStatusList"] = CodeUtils.GetSelectListByModel<c_ServiceOrderStatus>(dao.GetServiceOrderStatusAll(false), form["ServiceOrderStatus"], true);

            return View("ServiceSalesOrderCriteria",list);
        }

        /// <summary>
        /// �T�[�r�X�`�[�����_�C�A���O
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(FormCollection form) {
            if (string.IsNullOrEmpty(form["CustomerCode"])) {
                form["CustomerCode"] = Request["customerCode"];
            }
            if (string.IsNullOrEmpty(form["VinFull"])) {
                form["VinFull"] = Request["vin"];
            }
            form["DelFlag"] = "0";
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["VinFull"] = form["VinFull"];
            ViewData["DelFlag"] = form["DelFlag"];
            PaginatedList<ServiceSalesHeader> list = GetSearchResultList(form);
            return View("ServiceSalesOrderCriteriaDialog", list);
        }
        /// <summary>
        /// �T�[�r�X�`�[�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�T�[�r�X�`�[�������ʃ��X�g</returns>
        /// <history>
        /// 2018/08/14 yano #3912 �T�[�r�X�`�[�����@���������ɓ��͂����C�x���g�̖��̂��������s��A������
        /// </history>
        private PaginatedList<ServiceSalesHeader> GetSearchResultList(FormCollection form) {
            ServiceSalesOrderDao serviceSalesOrderDao = new ServiceSalesOrderDao(db);
            ServiceSalesHeader salesHeaderCondition = new ServiceSalesHeader();
            salesHeaderCondition.FrontEmployee = new Employee();
            salesHeaderCondition.FrontEmployee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.ReceiptionEmployee = new Employee();
            salesHeaderCondition.ReceiptionEmployee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.Customer = new Customer();
            salesHeaderCondition.Customer.CustomerCode = form["CustomerCode"];
            salesHeaderCondition.Customer.CustomerName = form["CustomerName"];
            salesHeaderCondition.Customer.CustomerNameKana = form["CustomerNameKana"];
            salesHeaderCondition.Customer.TelNumber = form["TelNumber"];
            salesHeaderCondition.SlipNumber = form["SlipNumber"];
            salesHeaderCondition.CampaignCode1 = form["CampaignCode"];  //Mod 2018/08/14 yano #3912
            salesHeaderCondition.CampaignCode2 = form["CampaignCode"];  //Mod 2018/08/14 yano #3912
            salesHeaderCondition.ServiceOrderStatus = form["ServiceOrderStatus"];
            salesHeaderCondition.Vin = form["Vin"];
            salesHeaderCondition.VinFull = form["VinFull"];
            salesHeaderCondition.RegistrationNumberPlate = form["PlateNumber"];
            salesHeaderCondition.CarBrandName = form["CarBrandName"];
            salesHeaderCondition.QuoteDateFrom = CommonUtils.StrToDateTime(form["QuoteDateFrom"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.QuoteDateTo = CommonUtils.StrToDateTime(form["QuoteDateTo"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.ServiceWorkCode = form["ServiceWorkCode"];
            salesHeaderCondition.DepartmentCode = form["DepartmentCode"];
            salesHeaderCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"]);
            salesHeaderCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"]);
            salesHeaderCondition.WithoutAkaden = form["WithoutAkaden"] != null && form["WithoutAkaden"].Equals("0");
            salesHeaderCondition.CustomerClaimCode = form["CustomerClaimCode"];
            salesHeaderCondition.CustomerClaimName = form["CustomerClaimName"];

            if (form["DelFlag"] != null && (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1"))) {
                salesHeaderCondition.DelFlag = form["DelFlag"];
            }
            //salesHeaderCondition.SetAuthCondition((Employee)Session["Employee"]);
            return serviceSalesOrderDao.GetListByCondition(salesHeaderCondition, (Employee)Session["Employee"], int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region ���͉�ʕ\��
        /// <summary>
        /// �T�[�r�X�`�[���͉�ʕ\��
        /// </summary>
        /// <param name="SlipNo">�T�[�r�X�`�[�ԍ�</param>
        /// <param name="RevNo">�����ԍ�</param>
        /// <param name="OrgSlipNo">�ԗ��`�[�ԍ�</param>
        /// <history>
        /// 2017/01/21 arc yano #3657 ���Ϗ��̌l���̕\���E��\����؂�ւ���`�F�b�N�{�b�N�X��ǉ�
        /// 2015/10/28 arc yano #3289 ���i�d���@�\���P(�T�[�r�X�`�[����)
        /// </history>
        /// <returns></returns>
        public ActionResult Entry(string SlipNo, int? RevNo, string OrgSlipNo, string Mode) {

            //�T�[�r�X�`�[���w�肳��Ă���ꍇ�̓f�[�^���擾����
            ServiceSalesHeader header;
            if (string.IsNullOrEmpty(SlipNo)) {
                header = new ServiceSalesHeader();
                Employee employee = (Employee)Session["Employee"];
                header.QuoteDate = DateTime.Today;
                header.QuoteExpireDate = DateTime.Today.AddDays(6);
                header.Department = employee.Department1;
                header.DepartmentCode = employee.DepartmentCode;
                header.FrontEmployeeCode = employee.EmployeeCode;
                header.ReceiptionEmployeeCode = employee.EmployeeCode;
                header.ServiceOrderStatus = "001";
                header.RevisionNumber = 0;
                header.LaborRate = 0;
                header.ArrivalPlanDate = DateTime.Today;

                header.ServiceSalesLine = new EntitySet<ServiceSalesLine>();
                header.ServiceSalesLine.Add(new ServiceSalesLine { ServiceType = "001" });

                //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
                if (header.ConsumptionTaxId == null)
                {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
                }


                for (int i = 0; i < 4; i++) {
                    header.ServiceSalesLine.Add(new ServiceSalesLine { ServiceType = "004" });
                }

            } else {
                //���r�W�������w�肵�Ă��Ȃ��ꍇ�ŐV�`�[���擾
                if (RevNo == null) {
                    header = new ServiceSalesOrderDao(db).GetBySlipNumber(SlipNo);
                } else {
                    header = new ServiceSalesOrderDao(db).GetByKey(SlipNo, RevNo ?? 1);
                }

                // �ҏW����������ꍇ�̂݃��b�N����Ώ�
                Employee loginUser = (Employee)Session["Employee"];
                string departmentCode = header.DepartmentCode;
                string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
                // ������E��������P�`�R�E�Z�L�����e�B���x��ALL�̂ǂꂩ�ɊY�������烍�b�N����
                if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004")) { 
                    // �������Ȃ�
                } else {
                    // �����ȊO�����b�N���Ă���ꍇ�̓G���[�\������
                    string lockEmployeeName = service.GetProcessLockUser(header);
                    if (!string.IsNullOrEmpty(lockEmployeeName)) {
                        header.LockEmployeeName = lockEmployeeName;
                    } else {
                        // �`�[���b�N
                        service.ProcessLock(header);
                    }
                }

                //Add 2016/04/08 arc nakayama #3197�T�[�r�X�`�[�̎Ԍ��L������������_�������ԗ��}�X�^�փR�s�[����ۂ̊m�F���b�Z�[�W�@�\  ����_�����ƎԌ��L�������͎ԗ��}�X�^����擾����
                //�Ǘ��ԍ�������A�[�ԍρE�L�����Z���E��Ɨ����E��ƒ��~�łȂ��A�܂��́A�[�ԍςł��C�����������ꍇ�͎���_�����ƎԌ��L�������͎ԗ��}�X�^����擾����
                if ((!string.IsNullOrEmpty(header.SalesCarNumber) && header.ServiceOrderStatus != "006" && header.ServiceOrderStatus != "007" && header.ServiceOrderStatus != "009" && header.ServiceOrderStatus != "010") || (header.ServiceOrderStatus == "006" && service.CheckModification(header.SlipNumber, header.RevisionNumber)))
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                    if (SalesCarData != null)
                    {
                        header.NextInspectionDate = SalesCarData.NextInspectionDate;
                        header.InspectionExpireDate = SalesCarData.ExpireDate;
                    }
                }


                //Del 2015/10/28 arc yano #3289 SetDataComponent�ֈړ�
                /*
                PartsStockDao dao = new PartsStockDao(db);
                foreach (var a in header.ServiceSalesLine) {
                    if (!string.IsNullOrWhiteSpace(a.PartsNumber))
                    {
                        a.PartsStock = dao.GetStockQuantity(a.PartsNumber, header.DepartmentCode);
                    }   
                }
                 */
            }

            //�ԗ��`�[���w�肳��Ă���ꍇ�͎ԗ��`�[�̏��������p��
            if (!string.IsNullOrEmpty(OrgSlipNo)) {
                CarSalesHeader car = new CarSalesOrderDao(db).GetBySlipNumber(OrgSlipNo);
                if (car != null) {
                    header.CarSlipNumber = OrgSlipNo;
                    header.CarEmployeeCode = car.EmployeeCode;
                    header.CarSalesOrderDate = car.SalesOrderDate;
                    header.CustomerCode = car.CustomerCode;
                    header.CarGradeCode = car.CarGradeCode;
                    header.CarGradeName = car.CarGradeName;
                    header.CarName = car.CarName;
                    header.CarBrandName = car.CarBrandName;
                    header.ModelName = car.ModelName;
                    header.Mileage = car.Mileage;
                    header.MileageUnit = car.MileageUnit;
                    header.ManufacturingYear = car.ManufacturingYear;
                    header.Vin = car.Vin;
                    header.SalesCarNumber = car.SalesCarNumber;
                    header.LaborRate = 0;
                    //��ƈ˗���������p��
                    ServiceRequest request = new ServiceRequestDao(db).GetBySlipNumber(OrgSlipNo);
                    header.RequestContent = request.Memo;
                    foreach (var a in request.ServiceRequestLine) {
                        ServiceSalesLine line = new ServiceSalesLine();
                        Parts parts = new PartsDao(db).GetByKey(a.CarOptionCode);
                        if (parts != null) {
                            line.PartsNumber = parts.PartsNumber;
                        }
                        line.ServiceType = "003";
                        line.ServiceTypeName = "���i";
                        line.LineContents = a.CarOptionName;
                        line.Amount = a.Amount;
                        line.LineNumber = a.LineNumber;
                        line.RequestComment = a.RequestComment;
                        header.ServiceSalesLine.Add(line);
                    }
                }
            }

            

            //�T�[�r�X��t����̈����p��
            if (!string.IsNullOrEmpty(Request["customerCode"])) {
                header.CustomerCode = Request["customerCode"];

            }
            if (!string.IsNullOrEmpty(Request["employeeCode"])) {
                header.ReceiptionEmployeeCode = Request["employeeCode"];
            }
            if (!string.IsNullOrEmpty(Request["salesCarNumber"])) {
                SalesCar salesCar = new SalesCarDao(db).GetByKey(Request["salesCarNumber"]);
                header.Vin = salesCar.Vin;
                header.RegistrationNumberType = salesCar.RegistrationNumberType;
                header.RegistrationNumberPlate = salesCar.RegistrationNumberPlate;
                header.RegistrationNumberKana = salesCar.RegistrationNumberKana;
                header.NextInspectionDate = salesCar.NextInspectionDate;
                header.MorterViecleOfficialCode = salesCar.MorterViecleOfficialCode;
                header.ModelName = salesCar.ModelName;
                header.MileageUnit = salesCar.MileageUnit;
                header.Mileage = salesCar.Mileage;
                header.ManufacturingYear = salesCar.ManufacturingYear;
                header.FirstRegistration = salesCar.FirstRegistrationYear;
                header.EngineType = salesCar.EngineType;
                header.CarName = salesCar.CarGrade.Car.CarName;
                header.CarGradeName = salesCar.CarGrade.CarGradeName;
                header.CarGradeCode = salesCar.CarGradeCode;
                header.CarBrandName = salesCar.CarGrade.Car.Brand.CarBrandName;
                header.SalesCarNumber = salesCar.SalesCarNumber;
                if (salesCar.ExpireDate != null && salesCar.ExpireType != null && salesCar.ExpireType.Equals("001")) {
                    header.InspectionExpireDate = salesCar.ExpireDate;
                }
                try { header.LaborRate = salesCar.CarGrade.Car.Brand.LaborRate; } catch (NullReferenceException) { }
            }
            if (!string.IsNullOrEmpty(Request["arrivalPlanDate"])) {
                header.ArrivalPlanDate = DateTime.Parse(HttpUtility.HtmlDecode(Request["arrivalPlanDate"]));
            }
            if (!string.IsNullOrEmpty(Request["requestDetail"])) {
                header.RequestContent = Request["requestDetail"];
            } 

            //Add 2017/01/21 arc yano 
            if (header.ServiceOrderStatus.Equals("001"))    //�`�[�X�e�[�^�X=�u���ρv
            {
                header.DispPersonalInfo = false;
            }
            else ////�`�[�X�e�[�^�X���u���ρv
            {
                header.DispPersonalInfo = true;            
            }


            //�f�[�^�t����ʃR���|�[�l���g�̓ǂݍ���
            SetDataComponent(ref header);

            //�l���z��ō��\���ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //���v�v�Z
            //2014/03/19.ookubo�u[CRMs - �o�O #3006] �y�T�z�[�ԍς݂Ő������ς��v�Ή��̂��߃R�����g�A�E�g�i�����p�~���N���C�A���gjs�����ɂ܂����j
            //service.CalcLineAmount(header);

            ViewData["Mode"] = Mode;

            //���͉�ʂ�\��
            return GetViewResult(header);
        }

        /// <summary>
        /// �`�[���R�s�[���ē��͉�ʕ\���i���ׂ��������j
        /// </summary>
        /// <param name="SlipNo"></param>
        /// <param name="RevNo"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/22 arc yano #3887 Excel�捞(���i���i����)
        /// 2015/10/28 arc yano #3289 ���i�d���@�\���P(�T�[�r�X�`�[����)
        /// </history>
        public ActionResult Copy(string SlipNo, int RevNo) {
            ServiceSalesHeader header = new ServiceSalesHeader();
            Employee employee = (Employee)Session["Employee"];
            header = new ServiceSalesHeader();
            header.QuoteDate = DateTime.Today;
            header.QuoteExpireDate = DateTime.Today.AddDays(6);
            header.Department = employee.Department1;
            header.DepartmentCode = employee.DepartmentCode;
            header.FrontEmployeeCode = employee.EmployeeCode;
            header.ReceiptionEmployeeCode = employee.EmployeeCode;
            header.ServiceOrderStatus = "001";
            header.RevisionNumber = 0;
            header.LaborRate = 0;
            header.ArrivalPlanDate = DateTime.Today;

            //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
            if (header.ConsumptionTaxId == null)
            {
                header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }


            ServiceSalesHeader original = new ServiceSalesOrderDao(db).GetByKey(SlipNo, RevNo);
            if (original.ServiceSalesLine.Count() > 0) {
                foreach (var item in original.ServiceSalesLine) {
                    // ��������N���A
                    item.CustomerClaimCode = "";
                    // �����ϐ��A���������N���A
                    item.ProvisionQuantity = 0; // Add 2015/10/28 arc yano
                    item.OrderQuantity = 0; // Add 2015/10/28 arc yano

                    //Add 2018/05/22 arc yano #3887
                    //����(�P��)���Ď擾

                    if(!string.IsNullOrWhiteSpace(item.PartsNumber))
                    {
                        PartsMovingAverageCost condition = new PartsMovingAverageCost();

                        condition.PartsNumber = item.PartsNumber;
                        condition.CompanyCode = "001";

                        item.UnitCost = (new PartsMovingAverageCostDao(db).GetByKey(condition) != null ? new PartsMovingAverageCostDao(db).GetByKey(condition).Price : (new PartsDao(db).GetByKey(item.PartsNumber) != null ? new PartsDao(db).GetByKey(item.PartsNumber).Cost : item.UnitCost));
                        item.Cost = Math.Round((item.UnitCost ?? 0) * (item.Quantity ?? 0), 0, MidpointRounding.AwayFromZero);
                    }
                }

                // ���ׂ������p��
                header.ServiceSalesLine = original.ServiceSalesLine;
            }

            //�f�[�^�t����ʃR���|�[�l���g�̓ǂݍ���
            SetDataComponent(ref header);

            //�l���z��ō��\���ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //���v�v�Z
            //2014/03/19.ookubo�u[CRMs - �o�O #3006] �y�T�z�[�ԍς݂Ő������ς��v�Ή��̂��߃R�����g�A�E�g�i�����p�~���N���C�A���gjs�����ɂ܂����j
            //service.CalcLineAmount(header);

            return GetViewResult(header);
        }
        #endregion

        #region �Z�b�g���j���[�ǉ�
        /// <summary>
        /// ���Ƃ��I�����ꂽ�Ƃ��Z�b�g���j���[������Βǉ�����
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
        /// <param name="line">�T�[�r�X���׃f�[�^</param>
        /// <param name="pay">�x�����@�f�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        /// <history>
        /// arc yano #3596 �y�區�ځz����I�����Ή� �݌ɂ̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2016/04/14 arc yano #3480
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddSetMenu(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {
            int currentLineNumber = int.Parse(form["CurrentLineNumber"] ?? "0");
            ViewData["EntryMode"] = form["EntryMode"];

            //Add 2014/06/17 arc yano line��lineNumber���Ƀ\�[�g����B
            ModelState.Clear();
            //���׍s��lineNumber���ɕ��ёւ�
            Sortline(ref line);

            //�Z�b�g���j���[�R�[�h���擾
            string setMenuCode = line[currentLineNumber].SetMenuCode;
            string classification1 = line[currentLineNumber].Classification1;

            //�Z�b�g���j���[�̍s���폜
            line.RemoveAt(currentLineNumber);

            if (!string.IsNullOrEmpty(setMenuCode)) {
                //�Z�b�g���j���[�ɕR�t���Ă����ƍ��ڂ��擾
                List<SetMenuList> list = new SetMenuListDao(db).GetListByCondition(new SetMenuList() { SetMenuCode = setMenuCode });

                for (int i = 0; i < list.Count; i++) {
                    ServiceSalesLine addLine = new ServiceSalesLine();
                    addLine.ServiceType = list[i].ServiceType;
                    addLine.WorkType = list[i].WorkType;

                    switch (list[i].ServiceType){
                        case "001":
                            //����
                            addLine.ServiceWorkCode = list[i].ServiceWorkCode;
                            addLine.LineContents = list[i].ServiceWork.Name;
                            addLine.Classification1 = classification1; 
                            //Add #3480 ���Ƃ̏ꍇ�́A�����敪�ނ�ݒ肷��
                            if(!string.IsNullOrWhiteSpace(list[i].ServiceWorkCode))
                            {
                                addLine.ServiceWork = new ServiceWorkDao(db).GetByKey(list[i].ServiceWorkCode.Trim());
                                addLine.SWCustomerClaimClass = addLine.ServiceWork.CustomerClaimClass;
                            }
                            
                            break;
                        case "002":
                            //�T�[�r�X���j���[
                            addLine.ServiceMenuCode = list[i].ServiceMenuCode;
                            addLine.LineContents = list[i].ServiceMenu.ServiceMenuName;
                            addLine.Classification1 = classification1;
                            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                            //�O���[�h���w�肳��Ă�����H�����擾
                            if (!string.IsNullOrEmpty(header.CarGradeCode) && CommonUtils.DefaultString(list[i].AutoSetAmount).Equals("001"))
                            {
                                header.CarGrade = new CarGradeDao(db).GetByKey(header.CarGradeCode);
                                ServiceCost cost = new ServiceCostDao(db).GetByKey(list[i].ServiceMenuCode, header.CarGrade.CarClassCode);
                                addLine.ManPower = cost != null ? cost.Cost : 0;
                                int laborRate = 0;
                                if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                     laborRate = int.Parse(form["LaborRate"] ?? "0");
                                }
                                addLine.LaborRate = laborRate;
                                addLine.TechnicalFeeAmount = addLine.ManPower * addLine.LaborRate;
                            }
                            break;
                        case "003":
                            //���i
                            addLine.PartsNumber = list[i].PartsNumber;
                            addLine.LineContents = list[i].Parts != null ? (list[i].Parts.PartsNameJp ?? list[i].Parts.PartsNameEn) : "";
                            addLine.Classification1 = classification1;
                            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                            if (CommonUtils.DefaultString(list[i].AutoSetAmount).Equals("001"))
                            {
                                //�݌ɂƒP�����Z�b�g
                                addLine.Price = list[i].Parts.Price;
                                addLine.Quantity = 1;
                                addLine.Amount = addLine.Price * addLine.Quantity;

                                //Mod 2016/08/13 arc yano #3596
                                //����R�[�h����g�p�q�ɂ����o��
                                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                                addLine.PartsStock = new PartsStockDao(db).GetStockQuantity(list[i].PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));
                                //addLine.PartsStock = new PartsStockDao(db).GetStockQuantity(list[i].PartsNumber, header.DepartmentCode);
                            }
                            //ADD 2016/05/06 arc nakayama #3514_�T�[�r�X�`�[�@���f���uSP�v�݌ɔ��f���u�݌Ɂv�̑g�ݍ��킹����͂ł��Ă��܂��P�[�X������
                            //���f���uSP�v�������ꍇ�͍݌ɔ��f���uSP�v�ɂ���
                            if (addLine.WorkType == "015")
                            {
                                addLine.StockStatus = "997";//SP
                            }

                            break;
                        case "004":
                            //�R�����g
                            addLine.LineContents = list[i].Comment;
                            addLine.Classification1 = classification1;
                            break;
                    }
                    //addLine.c_ServiceType = new CodeDao(db).GetServiceTypeByKey("002");

                    line.Insert(currentLineNumber + i, addLine);
                }
            }
            //Add 2016/04/22 arc nakayama #3495_�T�[�r�X�`�[���́@�Z�b�g���j���[�I�����̏����̕s� LineNumber��displayOrder��U��Ȃ����ă\�[�g����
            for (int i = 0; i < line.Count; i++)
            {
                line[i].LineNumber = i;
                line[i].DisplayOrder = i;
            }
            Sortline(ref line);

            ModelState.Clear();

            //�x�����@��R�t��
            header.ServiceSalesPayment = pay;
           
            //���ׂƕR�t��
            header.ServiceSalesLine = line;

            //���v���v�Z����
            //2014/03/19.ookubo�u[CRMs - �o�O #3006] �y�T�z�[�ԍς݂Ő������ς��v�Ή��̂��߃R�����g�A�E�g�i�����p�~���N���C�A���gjs�����ɂ܂����j
            //service.CalcLineAmount(header);

            //��ʃR���|�[�l���g�̃Z�b�g
            SetDataComponent(ref header);

            //���͉�ʂ�\��
            return GetViewResult(header);
        }
        #endregion

        #region 
        /// <summary>
        /// �S��ʃ��[�h�ؑ�
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
        /// <param name="line">�T�[�r�X���׃f�[�^</param>
        /// <param name="pay">�x�����@�f�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o��
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ChangeEntryMode(
            ServiceSalesHeader header,
            EntitySet<ServiceSalesLine> line,
            EntitySet<ServiceSalesPayment> pay,
            FormCollection form) {

            //Add 2014/06/17 arc yano line��lineNumber���Ƀ\�[�g����B
            ModelState.Clear();
            //���׍s��lineNumber���ɕ��ёւ�
            Sortline(ref line);

            //Ad 2017/11/18 #3803
            SetOutputTragetFlag(ref line);


            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            ViewData["EntryMode"] = form["EntryMode"];
            //Add 2014/12/24 arc yano #3143 ���v�l�̍Čv�Z(SetDataComponent���Ŏ��s)�̑O�ɒl���z����U�A�Ŕ��\���ɖ߂��A�Čv�Z��ɁA�ēx�ō��\���ɕϊ�����B
            //�l���z��ō��\������A�Ŕ��\���ɕϊ�
            service.SetDiscountAmountWithoutTax(line);
            
            //��ʃR���|�[�l���g�̃Z�b�g
            SetDataComponent(ref header);

            //�l���z��ō��\���ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            // �W�����[�h
            return GetViewResult(header);
        }

        #endregion

        #region ��ƕ��i���׍s�ǉ��A�폜
        /*//Del 2014/06/12 arc yano ���׍s�̒ǉ��A�폜�̓N���C�A���g�ł����Ȃ����߁A�폜�B
        public ActionResult AddServiceLine(ServiceSalesHeader header,EntitySet<ServiceSalesLine> line,EntitySet<ServiceSalesPayment> pay,FormCollection form){
           
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;
            string classification1 = "";
            string customerClaimCode = "";
            if (line!=null && line.Count > 0) {
                classification1 = line[line.Count - 1].Classification1;
                customerClaimCode = line[line.Count - 1].CustomerClaimCode;
            }
            ViewData["lineScroll"] = form["lineScroll"];
            ViewData["EntryMode"] = form["EntryMode"];

            if (!string.IsNullOrEmpty(form["EditType"])) {
                ModelState.Clear();
                switch (form["EditType"]) {
                    case "add":
                        int addSize = int.Parse(form["AddSize"] ?? "1");
                        for (int i = 0; i < addSize; i++) {
                            ServiceSalesLine addLine = new ServiceSalesLine();
                            addLine.ServiceType = form["ServiceType"];
                            addLine.Classification1 = classification1;
                            addLine.CustomerClaimCode = customerClaimCode;
                            c_ServiceType serviceType = new CodeDao(db).GetServiceTypeByKey(form["ServiceType"]);
                            if (serviceType != null) {
                                addLine.ServiceTypeName = serviceType.ShortName;
                            }
                            if (addLine.ServiceType.Equals("002")) {
                                int laborRate = 0;
                                if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                    int.TryParse(form["LaborRate"], out laborRate);
                                }
                                addLine.LaborRate = laborRate;
                                if (line != null) {
                                    var query =
                                        (from a in line
                                         where a.ServiceType.Equals("002")
                                         orderby a.LineNumber descending
                                         select a).FirstOrDefault();
                                    addLine.EmployeeCode = query != null ? query.EmployeeCode : "";
                                }
                            }

                            header.ServiceSalesLine.Add(addLine);
                        }
                        break;
                    case "delete":
                        header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                        break;
                    case "insert":
                        ServiceSalesLine insertLine = new ServiceSalesLine();
                        insertLine.ServiceType = form["ServiceType"];
                        insertLine.Classification1 = classification1;
                        c_ServiceType insertServiceType = new CodeDao(db).GetServiceTypeByKey(form["ServiceType"]);
                        if (insertServiceType != null) {
                            insertLine.ServiceTypeName = insertServiceType.ShortName;
                        }
                        if (insertLine.ServiceType.Equals("002")) {
                            int laborRate = 0;
                            if (!string.IsNullOrEmpty(form["LaborRate"])) {
                                int.TryParse(form["LaborRate"], out laborRate);
                            }
                            insertLine.LaborRate = laborRate;
                            var query =
                                (from a in line
                                 where a.ServiceType.Equals("002")
                                 && a.LineNumber <= int.Parse(form["EditLine"])
                                 orderby a.LineNumber descending
                                 select a).FirstOrDefault();
                            insertLine.EmployeeCode = query!=null ? query.EmployeeCode : "";

                        }
                        header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]), insertLine);
                        break;
                    case "copy":
                        ServiceSalesLine copyLine = (ServiceSalesLine)header.ServiceSalesLine[int.Parse(form["EditLine"])].Clone();
                        header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) + 1, copyLine);
                        
                        break;
                    case "up":
                        if (int.Parse(form["EditLine"]) > 0) {
                            ServiceSalesLine upLine = header.ServiceSalesLine[int.Parse(form["EditLine"])];
                            header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                            header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) - 1, upLine);
                        }
                        break;
                    case "down":
                        if (int.Parse(form["EditLine"]) < header.ServiceSalesLine.Count-1) {
                            ServiceSalesLine upLine = header.ServiceSalesLine[int.Parse(form["EditLine"])];
                            header.ServiceSalesLine.RemoveAt(int.Parse(form["EditLine"]));
                            header.ServiceSalesLine.Insert(int.Parse(form["EditLine"]) + 1, upLine);
                        }
                        break;
                    default:
                        break;
                }
            }  
            
            //���v�v�Z
            //2014/03/19.ookubo�u[CRMs - �o�O #3006] �y�T�z�[�ԍς݂Ő������ς��v�Ή��̂��߃R�����g�A�E�g�i�����p�~���N���C�A���gjs�����ɂ܂����j
            //service.CalcLineAmount(header);
            
            //��ʃR���|�[�l���g�̃Z�b�g
            SetDataComponent(ref header);

            //���͉�ʂ̕\��
            return GetViewResult(header);
                        
        }
         */
        #endregion
        
        #region �x�����@�s�ǉ��A�폜
        public ActionResult AddPaymentLine(ServiceSalesHeader header,EntitySet<ServiceSalesLine> line,EntitySet<ServiceSalesPayment> pay,FormCollection form){
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            foreach (var p in header.ServiceSalesPayment) {
                p.DepositFlag = p.DepositFlag.Contains("true") ? "1" : "0";
            }

            if (!string.IsNullOrEmpty(form["DelPayLine"])) {
                ModelState.Clear();
                //DelPayLine��0�ȏゾ������w��s�폜
                if (Int32.Parse(form["DelPayLine"]) >= 0) {
                    header.ServiceSalesPayment.RemoveAt(Int32.Parse(form["DelPayLine"]));
                    header.PaymentTotalAmount = header.ServiceSalesPayment.Sum(x => x.Amount);
                } else {
                    ServiceSalesPayment addPayment = new ServiceSalesPayment();
                    if (!string.IsNullOrEmpty(header.CustomerCode)) {
                        header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    }
                    if (header.Customer != null && header.Customer.CustomerClaim != null) {
                        addPayment.CustomerClaimCode = header.Customer.CustomerClaim.CustomerClaimCode;
                    }
                    addPayment.DepositFlag = "0";
                    header.ServiceSalesPayment.Add(addPayment);
                }
            }

            //���v�v�Z
            //2014/03/19.ookubo�u[CRMs - �o�O #3006] �y�T�z�[�ԍς݂Ő������ς��v�Ή��̂��߃R�����g�A�E�g�i�����p�~���N���C�A���gjs�����ɂ܂����j
            //service.CalcLineAmount(header);

            //�\�����ڂ��ăZ�b�g
            SetDataComponent(ref header);

            //�x�����@���͉�ʂ������\���ɂ���
            ViewData["displayContents"] = "invoice";

            //�o��
            return GetViewResult(header);
        }
        #endregion

        #region ������ꊇ�ݒ�
        /// <summary>
        /// ��������ꊇ�o�^����
        /// </summary>
        /// <param name="header">�w�b�_</param>
        /// <param name="line">����</param>
        /// <param name="pay">�x�������@</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2016/04/14 arc yano #3480 �T�[�r�X�`�[�@�T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� �ݒ肷�鐿���悪���Ƃ̐����敪�ނƈ�v�������̂̂ݐݒ肷��
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SetCustomerClaim(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            //Add 2014/06/17 arc yano line��lineNumber���Ƀ\�[�g����B
            ModelState.Clear();
            //���׍s��lineNumber���ɕ��ёւ�
            Sortline(ref line);
            
            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;
            ModelState.Clear();
            ViewData["EntryMode"] = form["EntryMode"];
            if (!string.IsNullOrEmpty(header.CustomerCode)) {
                header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode);
            }
            if (header.Customer != null && header.Customer.CustomerClaim!=null) {
                for (int i = 0; i < header.ServiceSalesLine.Count; i++) {
                    ServiceWork rec = new ServiceWorkDao(db).GetByKey(header.ServiceSalesLine[i].ServiceWorkCode);

                    if (string.IsNullOrEmpty(header.ServiceSalesLine[i].CustomerClaimCode) && (string.IsNullOrWhiteSpace(header.ServiceSalesLine[i].ServiceWorkCode) || string.IsNullOrWhiteSpace(rec.CustomerClaimClass) || header.Customer.CustomerClaim.c_CustomerClaimType == null || string.IsNullOrWhiteSpace(header.Customer.CustomerClaim.c_CustomerClaimType.CustomerClaimClass) || rec.CustomerClaimClass.Equals(header.Customer.CustomerClaim.c_CustomerClaimType.CustomerClaimClass)))   //2016/04/14 arc yano #3480
                    {
                        header.ServiceSalesLine[i].CustomerClaimCode = header.Customer.CustomerClaim.CustomerClaimCode;
                    }
                }    
            }

            //Add 2014/12/24 arc yano #3143 ���v�l�̍Čv�Z(SetDataComponent���Ŏ��s)�̑O�ɒl���z����U�A�Ŕ��\���ɖ߂��A�Čv�Z��ɁA�ēx�ō��\���ɕϊ�����B
            //�l���z��ō��\������A�Ŕ��\���ɕϊ�
            service.SetDiscountAmountWithoutTax(line);

            SetDataComponent(ref header);

            //�l���z��ō��\���ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            return GetViewResult(header);
        }
        #endregion

        #region �`�[�ۑ�����

        /// <summary>
        /// �T�[�r�X�`�[�ۑ�����
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[</param>
        /// <param name="line">�T�[�r�X�`�[����</param>
        /// <param name="pay">�T�[�r�X�`�[�x��</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>
        /// 2023/08/22 yano #4177�y�T�[�r�X�`�[���́z������ʖ��ׂ̕\���s���̑Ή�
        /// 2018/05/28 arc yano #3889 �T�[�r�X�`�[���� �T�[�r�X�`�[�������i����������Ȃ�
        /// 2018/05/24 arc yano #3896 �T�[�r�X�`�[���́@�[�Ԍ�̔[�Ԋm�F��������ł��Ȃ�
        /// 2018/02/23 arc yano #3849  �V���[�g�p�[�c�A�Г����B�i�̐ԓ`�E�ԍ��������̃`�F�b�N �`�[�C������validation�`�F�b�N�Ɉ���(line)��ǉ�
        /// 2018/02/20 arc yano #3858 �T�[�r�X�`�[�@�[�Ԍ�̕ۑ������ŁA�[�ԓ����󗓂ŕۑ��ł��Ă��܂�
        /// 2017/11/03 arc yano #3732 �T�[�r�X�`�[ �ԍ��`�[�쐬�Łu���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂��� �v�\�� �����ǉ�
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o��
        /// 2016/04/21 arc yano #3496 �T�[�r�X�`�[���́@�s�ړ����̔������̍X�V�̕s�
        /// 2016/04/20 arc yano #3492 �T�[�r�X�`�[���́@�`�[�ۑ����̃G���[ 
        /// 2016/01/26 arc yano #3453 �T�[�r�X�`�[�@�݌ɊǗ��ΏۊO�̕��i��validation���s��Ȃ�
        /// 2016/02/22 arc yano #3434 �T�[�r�X�`�[  ���Օi(SP)�̑Ή�
        /// 2016/02/17 arc yano #3435 �T�[�r�X�`�[�@�����O�̕��i�̑Ή�
        /// 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����݌ɂ̊Ǘ����@�̕ύX�@�����ϐ��A�������̍X�V���s��
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            List<PartsPurchaseOrder> orderList = new List<PartsPurchaseOrder>(); //Add 2015/10/28 arc yano #3289

            //Add 2016/04/20 arc yano #3496
            if (header.ActionType == "Reflesh")
            {
                ModelState.Clear();

                header = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                //��ʃR���|�[�l���g���Z�b�g
                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            //Add 2015/03/17 arc nakayama �`�[�C���{�^���@�܂��́@�C���L�����Z���@�������ꂽ�ꍇ�͕ʏ����i�C�����ɂ���@�܂��́@�C���𒆎~����j
            if (header.ActionType == "ModificationStart" || header.ActionType == "ModificationCancel")
            {   
                //Add 2017/04/23 arc yano
                ModelState.Clear();

                //�`�[�C���{�^�����������O�̏��擾
                ServiceSalesHeader SlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                //�{���߂������ꍇ�̓G���[�A�C�����ɂ����Ȃ�
                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, SlipData.SalesDate, "001") && header.ActionType == "ModificationStart")
                {
                    ModelState.AddModelError("SalesDate", "�{���߂����s���ꂽ���߁A�`�[�C���͍s���܂���B");
                    //���׍s��lineNumber���ɕ��ёւ�
                    Sortline(ref line);

                    service.ProcessLockUpdate(header);
                    header.ServiceSalesLine = line;
                    header.ServiceSalesPayment = pay;
                    //Add 2015/04/02 arc nakayama �o�O�C���@���s�����̒P�ʂ��\������Ȃ��^�C�~���O������
                    header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                    //�l���z��ō��\������A�Ŕ��\���ɕϊ�
                    service.SetDiscountAmountWithoutTax(line);
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }
                service.SalesOrder(header, line, ref orderList);    //Mod 2015/10/28 arc yano #3289
                ModelState.Clear();
                //���׍s��lineNumber���ɕ��ёւ�
                Sortline(ref line);

                service.ProcessLockUpdate(header);
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                //Add 2015/04/02 arc nakayama �o�O�C���@���s�����̒P�ʂ��\������Ȃ��^�C�~���O������
                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                //Add 2016/04/08 arc nakayama #3197�T�[�r�X�`�[�̎Ԍ��L������������_�������ԗ��}�X�^�փR�s�[����ۂ̊m�F���b�Z�[�W�@�\ �C�����ɂ���Ƃ��͍ŐV�̓��t���擾����
                //�`�[�C���{�^����������A�Ǘ��ԍ�������ꍇ�͎ԗ��}�X�^���玟��_�����ƎԌ��L���������擾����
                if (header.ActionType == "ModificationStart")
                {
                    if (!string.IsNullOrEmpty(header.SalesCarNumber))
                    {
                        SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                        if (SalesCarData != null)
                        {
                            header.NextInspectionDate = SalesCarData.NextInspectionDate;
                            header.InspectionExpireDate = SalesCarData.ExpireDate;
                        }
                    }
                }
                else
                {
                  //�C���L�����Z���������ꂽ�ꍇ�́A�X�i�b�v�V���b�g�̓��t�ɖ߂�
                    ServiceSalesHeader PrevSlipData = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //Mod 2018/02/23 ardc yano #3741
                    header = PrevSlipData;
                    //header.NextInspectionDate = PrevSlipData.NextInspectionDate;
                    //header.InspectionExpireDate = PrevSlipData.InspectionExpireDate;
                }


                //�l���z��ō��\������A�Ŕ��\���ɕϊ�
                service.SetDiscountAmountWithoutTax(line);
                SetDataComponent(ref header);
                return GetViewResult(header);
            }



            //Add 2014/06/10 arc yano �������Ή�
            ModelState.Clear();
            //���׍s��lineNumber���ɕ��ёւ�
            Sortline(ref line);

            //Add 2017/10/19 arc yano #3803
            SetOutputTragetFlag(ref line);

            if (form["ForceUnLock"] != null && form["ForceUnLock"].Equals("1"))
                
            {
                service.ProcessLockUpdate(header);

                header = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);       //DB�����������

                /*
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                */
                ViewData["ForceUnLock"] = "1";

                //Add 2014/12/24 arc yano #3143 ���v�l�̍Čv�Z(SetDataComponent���Ŏ��s)�̑O�ɒl���z����U�A�Ŕ��\���ɖ߂��A�Čv�Z��ɁA�ēx�ō��\���ɕϊ�����B
                //�l���z��ō��\������A�Ŕ��\���ɕϊ�
                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //�l���z��ō��\���ɕϊ�
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
        
            }
            //Add 2014/06/17 arc yano �������Ή� �폜�Ώۍs�̃f�[�^�폜
            //Mod 2015/10/15 arc nakayama #3264_�T�[�r�X�`�[�̃R�s�[�ōė��p�����`�[�ŗL���ȃ��J�S���ҏC����u���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂��� �v�\�� �o���f�[�V�����`�F�b�N�̑O�Ɉړ�
            Delline(ref line);

            //Mod 2018/05/25 arc yano #3896 �[�ԍψȍ~�̕ۑ��E���[�̏o�͂̏ꍇ�̓`�F�b�N���Ȃ�
            //Mod 2018/02/20 arc yano #3858
            //Add 2015/08/05 arc nakayama #3221_�����ƂȂ��Ă��镔���ԗ������ݒ肳��Ă���[�Ԋm�F��������o���Ȃ�
            //Add 2015/10/23 arc nakayama #3254_��Ɨ���`�[�����p�ł���悤�ɂ����� ���ςɖ߂��{�^���������ꂽ�Ƃ����`�F�b�N���s��Ȃ�
            //�X�e�[�^�X���[�ԍς݂łȂ��A���A�[�Ԋm�F���{�^��/�ۑ��{�^����������Ă��Ȃ����A�܂��́A�`�[�폜��������Ă��Ȃ�������validation�`�F�b�N���s��
            if (
                 !(
                    (header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update")) || 
                     header.ActionType.Equals("Cancel") || 
                     header.ActionType.Equals("Restore")
                  )
               )
            {
                //Add 2017/10/19 arc yano #3803
                //�������o�͂̏ꍇ�͐�p�̏������s��
                if (header.Output.Equals(true))
                {
                    ValidateOutput(form, header, line);
                }

                //����Validation�`�F�b�N
                ValidateAllStatus(header, line, form);  //Mod 2017/11/03 arc yano #3732
            }

            //Add 2018/05/28 arc yano #3889
            //�L�����Z���̏ꍇ�̓G���[�`�F�b�N
            if (header.ActionType.Equals("Cancel"))
            {
                //------------------------------------------------------
                //�ʃ��[�U�ɂ��`�[���X�V����Ă����ꍇ�̓G���[�Ƃ���
                //------------------------------------------------------
                //DB����ŐV�̓`�[���擾
                ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                if (dbHeader != null && dbHeader.RevisionNumber > header.RevisionNumber)
                {
                    ModelState.AddModelError("RevisionNumber", "�폜���悤�Ƃ��Ă���`�[�͍ŐV�ł͂���܂���B�ŐV�̓`�[���J����������ō폜���s���ĉ�����");
                }
            }

            //Mod  2018/05/28 arc yano #3889
            if (!ModelState.IsValid)
            {
                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;
                //Add 2015/04/02 arc nakayama �o�O�C���@���s�����̒P�ʂ��\������Ȃ��^�C�~���O������
                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                //Add 2014/12/24 arc yano #3143 ���v�l�̍Čv�Z(SetDataComponent���Ŏ��s)�̑O�ɒl���z����U�A�Ŕ��\���ɖ߂��A�Čv�Z��ɁA�ēx�ō��\���ɕϊ�����B
                //�l���z��ō��\������A�Ŕ��\���ɕϊ�
                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //�l���z��ō��\���ɕϊ�
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }

            // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);

            //Add 2016/05/31 arc nakayama #3568_�y�T�[�r�X�`�[�z���ς���󒍂ɂ���ƃ^�C���A�E�g�ŗ�����
            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {
                //�l����Ŕ��ɕϊ�
                service.SetDiscountAmountWithoutTax(line);

                //���ϕۑ����Ƃ���ȊO�ŏ����𕪊�
                switch (header.ActionType)
                {
                    case "Quote":
                        // ���ϕۑ�
                        service.Quote(header, line);
                        break;

                    case "History":
                        // ��Ɨ���
                        service.History(header, line);
                        break;

                    case "Restore":
                        // ���ςɖ߂�
                        service.Quote(header, line);
                        //Add 2016/04/08 arc nakayama #3197�T�[�r�X�`�[�̎Ԍ��L������������_�������ԗ��}�X�^�փR�s�[����ۂ̊m�F���b�Z�[�W�@�\ ���ςɖ߂��������ŐV�̎���_�����ƎԌ��L�������ɐ؂�ւ���
                        if (!string.IsNullOrEmpty(header.SalesCarNumber))
                        {
                            SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber, false);
                            if (SalesCarData != null)
                            {
                                header.NextInspectionDate = SalesCarData.NextInspectionDate;
                                header.InspectionExpireDate = SalesCarData.ExpireDate;
                            }
                        }

                        break;
                    case "Stop":
                        // ��ƒ��~
                        service.Stop(header, line);
                        break;
                    case "Cancel":
                        // �`�[�L�����Z��
                        service.Cancel(header, line);
                        break;

                    //Add 2015/03/18 arc nakayama �`�[�C���Ή��@�C����������
                    case "ModificationEnd":�@//�C�������i�C���m��j

                        ValidateSalesOrder(header, line);
                        ValidateForModification(header, line); //�C������validation�`�F�b�N       //Mod 2018/02/22 arc yano #3741

                        //���P�[�V�����ɕs������������G���[
                        if (!ModelState.IsValid)
                        {
                            //�l����ō��ɕϊ�
                            service.SetDiscountAmountWithTax(line);

                            header.ServiceSalesLine = line;
                            header.ServiceSalesPayment = pay;

                            SetDataComponent(ref header);

                            return GetViewResult(header);
                        }

                        service.SalesOrder(header, line, ref orderList);    //Mod 2015/10/28 arc yano #3289
                        break;

                    default:
                        //Add 2015/08/05 arc nakayama #3221_�����ƂȂ��Ă��镔���ԗ������ݒ肳��Ă���[�Ԋm�F��������o���Ȃ�
                        //�X�e�[�^�X���[�ԍς݁A���A�[�Ԋm�F���{�^��/�ۑ��{�^���������ꂽ���͈����p�������������X�V����A�܂��A�[�ԓ����ҏW�\�ł���΍X�V����
                        if (header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update"))
                        {
                            //�[�ԓ��ƈ����p�������̕������̃o���f�[�V�����`�F�b�N
                            ValidateForMemoUpdate(header);
                            if (!ModelState.IsValid)
                            {
                                //�l����ō��ɕϊ�
                                service.SetDiscountAmountWithTax(line);

                                header.ServiceSalesLine = line;
                                header.ServiceSalesPayment = pay;
                                SetDataComponent(ref header);
                                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                                return GetViewResult(header);
                            }

                            //�����p�������X�V
                            service.UpDateMemoServiceSalesHeader(header);

                            //Mod 2023/08/22 #4177
                            //service.SetDiscountAmountWithTax(line);
                            
                            header.ServiceSalesLine = line;
                            header.ServiceSalesPayment = pay;
                        }
                        else
                        {
                            // �󒍈ȍ~�̋��ʏ���
                            ValidateSalesOrder(header, line);
                            
                            //���P�[�V�����ɕs������������G���[
                            if (!ModelState.IsValid)
                            {
                                //�l����ō��ɕϊ�
                                service.SetDiscountAmountWithTax(line);

                                header.ServiceSalesLine = line;
                                header.ServiceSalesPayment = pay;

                                SetDataComponent(ref header);

                                return GetViewResult(header);
                            }

                            /*  2016/04/21 dell #3496
                            //Mod 2015/10/28 arc yano #3289
                            ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
                            List<ServiceSalesLine> list  = new List<ServiceSalesLine>();

                            if (dbheader != null)
                            {
                                list = dbheader.ServiceSalesLine.ToList();
                            }

                            for (int cnt = 0; cnt < line.Count(); cnt++)
                            {
                                //Mod 2016/04/20 arc yano #3492 FirstOrDefault()��ǉ�
                                if (list.Count > 0 && list.Where(x => x.LineNumber.Equals(line[cnt].LineNumber)).FirstOrDefault() != null)
                                {
                                    line[cnt].OrderQuantity = list.Where(x => x.LineNumber.Equals(line[cnt].LineNumber)).FirstOrDefault().OrderQuantity;
                                }
                            }
                            */

                            service.SalesOrder(header, line, ref orderList); //Mod 2015/10/28 arc yano #3289

                            //

                        }
                        break;
                }

                //Add 2015/08/05 arc nakayama #3221_�����ƂȂ��Ă��镔���ԗ������ݒ肳��Ă���[�Ԋm�F��������o���Ȃ�
                //�X�e�[�^�X���[�ԍς݂łȂ��A���A�[�Ԋm�F���{�^��/�ۑ��{�^����������Ă��Ȃ�������validation�`�F�b�N���s��
                if (!(header.ServiceOrderStatus.Equals("006") && header.ActionType.Equals("Update")))
                {
                    //�w�b�_INSERT
                    db.ServiceSalesHeader.InsertOnSubmit(header);
                }
                //��ƈ˗��̃^�X�N���폜
                if (!string.IsNullOrEmpty(header.CarSlipNumber))
                {
                    List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.SERVICE_REQUEST, header.CarSlipNumber);
                    foreach (var a in taskList)
                    {
                        a.TaskCompleteDate = DateTime.Now;
                    }
                }

                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/08 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/08 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0023", "�L�[�d���̃f�[�^��"));
                        //Add 2014/08/08 arc amii �G���[���O�Ή� �X�e�[�^�X�E�`�[�ԍ��E�����ԍ���߂������ǉ�
                        header.ServiceOrderStatus = form["PreviousStatus"];
                        header.SlipNumber = form["PrvSlipNumber"];
                        header.RevisionNumber = Int32.Parse(form["PrvRevisionNumber"]);
                        header.ServiceSalesLine = line;
                        header.ServiceSalesPayment = pay;
                        SetDataComponent(ref header);

                        //Mod 2014/12/24 arc yano #3143 �l����ō��̕ϊ��������Čv�Z��ɍs���悤�Ɉړ�����
                        //�l����ō��ɕϊ�
                        service.SetDiscountAmountWithTax(line);

                        return GetViewResult(header);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    //Add 2014/08/08 arc amii �G���[���O�Ή� ��L�ȊO�̗�O�̏ꍇ�A�G���[���O���o�͂��鏈���ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            ModelState.Clear();

            //��ʃR���|�[�l���g���Z�b�g
            SetDataComponent(ref header);

            //Mod 2014/12/24 arc yano #3143 �l����ō��̕ϊ��������Čv�Z��ɍs���悤�Ɉړ�����
            //�l����ō��ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            //���[���
            if (!string.IsNullOrEmpty(form["PrintReport"])) {
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            }

            /*
            //Add 2017/10/19 arc yano #3803
            if (header.ActionType.Equals("Output"))
            {
                header.Output = true;
            }
            else
            {
                header.Output = false;
            }
            */

            //Add 2015/10/28 arc yano #3289
            if (orderList != null)
            {
                header = makeOrderUrl(header, orderList);
            } 
            
            return GetViewResult(header);
        }
        #endregion

        #region Excel�o�͋@�\
        /// <summary>
        /// Excel�t�@�C���̃_�E�����[�h
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">�����f�[�^����</param> 
        /// <returns>Excel�f�[�^</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o��
        /// </history>
        //[AcceptVerbs(HttpVerbs.Post)]
        public  ActionResult Download(FormCollection form, ServiceSalesHeader header, EntitySet<ServiceSalesLine> line)
        {
            //-------------------------------
            //�ϐ��錾
            //-------------------------------
            string fileName = "";               //�t�@�C����
            string filePathName = "";           //�p�X�{�t�@�C����
            string tfilePathName = "";          //�e���v���[�g�t�@�C����(�p�X�{�t�@�C����)

            //-------------------------------
            //��������
            //-------------------------------  
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_PURCHASEORDER_DOWNLOAD);

            ModelState.Clear();

            //DB����擾
            ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //-------------------------------
            //�t�@�C�����E�e���v���[�g�t�@�C���̐ݒ�
            //-------------------------------
            SetFileName(ref fileName, ref filePathName, ref tfilePathName, form);
   
            //�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��Ȃ��ꍇ
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");

                header.ServiceSalesLine = line;

                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //�l���z��ō��\���ɕϊ�
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }

            //�G�N�Z���f�[�^�쐬
            byte[] excelData = MakeExcelData(form, dbHeader, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                header.ServiceSalesLine = line;

                header.c_MileageUnit = new CodeDao(db).GetMileageUnit(header.MileageUnit);

                service.SetDiscountAmountWithoutTax(line);

                SetDataComponent(ref header);

                //�l���z��ō��\���ɕϊ�
                service.SetDiscountAmountWithTax(header.ServiceSalesLine);

                return GetViewResult(header);
            }


            //�����σt���O�̍X�V
            SetOrderedFlag(dbHeader, line, form);

            //�R���e���c�^�C�v�̐ݒ�
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelData, contentType, fileName);
        }

        /// <summary>
        /// �G�N�Z���f�[�^�쐬(�e���v���[�g�t�@�C������)
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="inventoryMonth">�I����</param>
        /// <param name="fileName">���[��</param>
        /// <param name="tfileName">���[�e���v���[�g</param>
        /// <returns>�G�N�Z���f�[�^</returns>
        protected byte[] MakeExcelData(FormCollection form, ServiceSalesHeader header, string fileName, string tfileName)
        {
            //----------------------------
            //��������
            //----------------------------
            ConfigLine hconfigLine = null;            //�ݒ�l(�w�b�_�A�t�b�^)
            ConfigLine lconfigLine = null;            //�ݒ�l(����)

            byte[] excelData = null;                  //�G�N�Z���f�[�^
            bool ret = false;
            bool tFileExists = true;                  //�e���v���[�g�t�@�C������^�Ȃ�(���ۂɂ��邩�ǂ���)


            //�f�[�^�o�̓N���X�̃C���X�^���X��
            DataExport dExport = new DataExport();

            //�G�N�Z���t�@�C���I�[�v��(�e���v���[�g�t�@�C������)
            ExcelPackage excelFile = dExport.MakeExcel(fileName, tfileName, ref tFileExists);

            //�e���v���[�g�t�@�C�������������ꍇ
            if (tFileExists == false)
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C����������܂���ł����B");
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
            // �ݒ�l�̎擾
            //----------------------------
            int columnLine = 2;             //��̈ʒu

            //�ݒ�t�@�C���Ǎ�
            ExcelWorksheet config = excelFile.Workbook.Worksheets["config"];

            //----------------------------------
            //�w�b�_�A�t�b�^�̐ݒ�l�̎擾
            //----------------------------------
            if (config != null)
            {
                hconfigLine = dExport.GetConfigLine(config, columnLine);
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

            columnLine++;

            //---------------------------
            //���אݒ�l�̎擾
            //---------------------------
            lconfigLine = dExport.GetConfigLine(config, columnLine);

            //----------------------------
            // �ݒ肷��f�[�^�̎擾
            //----------------------------
            List<PurchaseOrderExcelHeader> orderList = SetOrderList(header, header.ServiceSalesLine, form["StockCode"]);

            /*
            //�擾�����f�[�^��0���̏ꍇ�͏����I��
            if (orderList.Count == 0)
            {
                ModelState.AddModelError("", "�������ɕ\������f�[�^������܂���");

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
            */

            //�I���W�i���V�[�g�̎擾
            ExcelWorksheet originSheet = excelFile.Workbook.Worksheets[hconfigLine.SheetName];

            int sheetCnt = 4;                                       //�J�n�V�[�g�̈ʒu

            string sheetName = "";                                  //����Ώۂ̃V�[�g��

            string orgSheetName = hconfigLine.SheetName;           //�e���v���V�[�g


            //------------------------------
            //�f�[�^�ݒ�
            //------------------------------
            foreach (var order in orderList)
            {
                List<PurchaseOrderExcelHeader> headerList = new List<PurchaseOrderExcelHeader>();

                //�V�[�g��ǉ�����
                //�V�[�g���𔭒��敪�t�̂��̂ɕύX
                //sheetName = orgSheetName + "_" + order.sheetNameIdx.Replace("/", "");

                sheetName = order.sheetNameIdx.Replace("/", "");

                excelFile.Workbook.Worksheets.Add(sheetName, originSheet);

                //-------------------------
                //�w�b�_�[�A�t�b�^�[�̐ݒ�
                //-------------------------
                hconfigLine.SheetName = sheetName;

                headerList.Add(order);

                //�f�[�^�ݒ�
                ret = dExport.SetData<PurchaseOrderExcelHeader, PurchaseOrderExcelHeader>(ref excelFile, headerList, hconfigLine);

                //-------------------------
                //���ׂ̐ݒ�
                //-------------------------
                //�ݒ�l�̃V�[�g����V�����V�[�g���ɕύX
                lconfigLine.SheetName = sheetName;

                //�f�[�^�ݒ�
                ret = dExport.SetData<PurchaseOrderExcelLine, PurchaseOrderExcelLine>(ref excelFile, order.line, lconfigLine);

                sheetCnt++;
            }

            //�e���v���V�[�g�̍폜
            excelFile.Workbook.Worksheets.Delete(originSheet);

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
        /// Excel�̃w�b�_�[�A�t�b�^�[�����̐ݒ�
        /// </summary>
        /// <param name="header">�w�b�_���</param>
        /// <returns name ="eHeader"></returns>
        /// <history>
        /// 2017/12/14 arc yano #3834 ���i�������̕��i���̎Q�Ɛ�̕ύX ���i��(PartsNameJp)
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬
        /// </history>
        private List<PurchaseOrderExcelHeader> SetOrderList(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, string stockStatusCode)
        {
            //�w�b�_���
            List<PurchaseOrderExcelHeader> eHeaderList = new List<PurchaseOrderExcelHeader>();
            //���׏��
            List<PurchaseOrderExcelLine> eline = new List<PurchaseOrderExcelLine>();

            //�������ɏo�͂��閾�ׂ𒊏o
            eline = line.Where
                (
                            
                    x => 
                        x.OutputTargetFlag != null &&
                        x.OutputTargetFlag.Equals("1") && 
                        !string.IsNullOrWhiteSpace(x.StockStatus) && 
                        !string.IsNullOrWhiteSpace(x.PartsNumber)       
                ).Select
                (
                    x =>
                    
                        new PurchaseOrderExcelLine
                    
                        {

                            //Mod 2017/12/14 arc yano #3834 
                            
                            OrderPartsNumber = (x.Parts != null ? !string.IsNullOrWhiteSpace(x.Parts.MakerPartsNumber) ? x.Parts.MakerPartsNumber : x.PartsNumber : x.PartsNumber)      //���i�ԍ�                                                                            //�������i�ԍ�
                            ,
                            OrderPartsNameJp = (x.Parts != null ? !string.IsNullOrWhiteSpace(x.Parts.PartsNameJp) ? x.Parts.PartsNameJp : x.LineContents : x.LineContents)    //�������i�� //Mod 2017/12/14 arc yano #3834
                            ,
                            Quantity = (x.Quantity ?? 0)                                                                                                                                //����
                            ,
                            Price = (x.Parts != null ? x.Parts.Price != null ? x.Parts.Price : x.Price : x.Price)                                                                       //�艿
                            ,
                            Cost = null
                            ,
                            Memo = null
                            ,
                            StockStatus = x.StockStatus                                                                                                                                 //���f
                            ,
                        PartsNumber = x.PartsNumber
                        }
                ).ToList();

            //���o�������ׂ�����ɕ��i�ԍ��A���f���ɒ��o
            eline = eline.GroupBy(x => new { x.OrderPartsNumber, x.OrderPartsNameJp, x.StockStatus, x.Price, x.PartsNumber }).Select(group =>
                    new PurchaseOrderExcelLine
                    {
                        OrderPartsNumber = group.Key.OrderPartsNumber                           //�������i�ԍ�
                        ,
                        OrderPartsNameJp = group.Key.OrderPartsNameJp                           //�������i��
                        ,
                        Quantity = group.Sum( x => x.Quantity)                                  //���ʁi���v�l�j
                        ,
                        Price = group.Key.Price                                                 //�艿
                        ,
                        Cost = null                                                             //����
                        ,
                        Memo = null                                                             //���l
                        ,
                        StockStatus = group.Key.StockStatus                                     //���f
                        ,
                        PartsNumber = group.Key.PartsNumber                                     //���i�ԍ�
                    }
                    ).ToList();

            //���f�敪
            List<string> stockStatusCodeList = new List<string>();

            //���ׂŔ��f���ɃO���[�s���O���ă��X�g���擾(���i�݌ɏo�ɓ`�[�̏ꍇ�͔��f���݌ɂ̂�)
            stockStatusCodeList = line.Where
                                    (
                                        x => 
                                            x.OutputTargetFlag != null &&
                                            x.OutputTargetFlag.Equals("1") && 

                                            !string.IsNullOrWhiteSpace(x.StockStatus) &&
                                            (
                                              ( //���f = �uS/O���̔����v
                                                string.IsNullOrWhiteSpace(stockStatusCode) &&
                                                x.c_StockStatus.StatusType.Equals("001")
                                              ) ||
                                              x.StockStatus.Equals(stockStatusCode)     //���f = �u�݌Ɂv
                                            )

                                    ).GroupBy
                                    (
                                        x => 
                                            x.StockStatus
                                    ).Select
                                    (
                                        x =>
                                            x.Key
                                    ).ToList();

            //���f
            List<c_StockStatus> stockStatusList = new List<c_StockStatus>();

            foreach (var code in stockStatusCodeList)
            {
                 
                c_StockStatus rec = new CodeDao(db).GetStockStatus(false, code, "");

                if (rec != null)
                {
                    stockStatusList.Add(rec);
                }
            }

            //���f���Ƀf�[�^���쐬
            foreach (var stockStatus in stockStatusList)
            {
                //�����f�[�^�̐ݒ�
                eHeaderList = SetOrderData(stockStatus, header, line, eHeaderList, eline);
            }

            return eHeaderList;
        }

        /// <summary>
        /// �t�@�C�����A�e���v���[�g�t�@�C�����̐ݒ�
        /// </summary>
        /// <param name="fileName">�t�@�C����</param>
        /// <param name="filePathName">�p�X���{�t�@�C����</param>
        /// <param name="tfilePathName">�e���v���[�g�t�@�C����</param>
        /// <return></return>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬
        /// </history>
        private int SetFileName(ref string fileName , ref string filePathName, ref string tfilePathName, FormCollection form   )
        {
            string filePath = "";

            //���[�N�t�H���_�擾
            filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExport"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExport"];

            //���i�������o�͂̏ꍇ
            if (string.IsNullOrWhiteSpace(form["StockCode"]))
            {
                //�t�@�C����(���i������_yyyyMMddhhmiss(�_�E�����[�h����))
                fileName = "���i������" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

                filePathName = filePath + fileName;

                //�e���v���[�g�t�@�C���p�X�擾
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrderFromService"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrderFromService"];

            }
            else //���i�݌ɏo�ɓ`�[�̏ꍇ
            {
                //�t�@�C����(PartsPurchaseOrder_xxx(�����ԍ�)_yyyyMMddhhmiss(�_�E�����[�h����))
                fileName = "���i�݌ɏo�ɓ`�[" + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

                filePathName = filePath + fileName;

                //�e���v���[�g�t�@�C���p�X�擾
                tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsMaterialDocumentForService"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsMaterialDocumentForService"];
            }
            
            return 0;
        }

        /// <summary>
        /// Excel�̃w�b�_�[�A�t�b�^�[�����̐ݒ�
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
        /// <param name="line">�T�[�r�X�`�[����</param>
        /// <param name="eHeaderList">�������w�b�_</param>
        /// <param name="elin">����������</param>
        /// <returns name ="eHeaderList"></returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬
        /// </history>
        private List<PurchaseOrderExcelHeader> SetOrderData(c_StockStatus stockStatus, ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, List<PurchaseOrderExcelHeader> eHeaderList, List<PurchaseOrderExcelLine> eline)
        { 

            int offset = 0;     //�f�[�^�̎擾�ʒu
            int index = 1;      //�V�[�g����index

            string searchSlipNumber = header.SlipNumber.Substring(0, 8);        //���`�[�̏ꍇ�́u�|�v�ȍ~�������i���ԓ`�̏ꍇ�͖{�����ɂ͗��Ȃ��j

            //���[�v����
            while (true)
            {
                PurchaseOrderExcelHeader eHeader = new PurchaseOrderExcelHeader();

                //���喼
                if (!string.IsNullOrWhiteSpace(header.DepartmentCode))
                {
                    eHeader.DepartmentName = header.Department.DepartmentName;
                }

                //�t�����g�S����
                if (!string.IsNullOrWhiteSpace(header.FrontEmployeeCode))
                {
                    eHeader.FrontEmployeeName = header.FrontEmployee.EmployeeName;
                }

                //���J�A�܂��͊O��
                string employeeCode = null;
                string supplierCode = null;

                //���ׂ̒��ŁA���J�j�b�N�S���҃R�[�h�����͂���Ă��āA�s�ԍ�����ԎႢ���̂��擾����B
                employeeCode = line.OrderBy(x => x.LineNumber).Where(x => !string.IsNullOrWhiteSpace(x.EmployeeCode)).Select(x => x.EmployeeCode).FirstOrDefault();

                //�擾�ł����ꍇ
                if (!string.IsNullOrWhiteSpace(employeeCode))
                {
                    eHeader.EmployeeSupplierName = new EmployeeDao(db).GetByKey(employeeCode).EmployeeName;
                }
                else //�擾�ł��Ȃ������ꍇ
                {
                    //���ׂ̒��ŁA�O���҃R�[�h�����͂���Ă��āA�s�ԍ�����ԎႢ���̂��擾����
                    supplierCode = line.OrderBy(x => x.LineNumber).Where(x => !string.IsNullOrWhiteSpace(x.SupplierCode)).Select(x => x.SupplierCode).FirstOrDefault();

                    eHeader.EmployeeSupplierName = new SupplierDao(db).GetByKey(supplierCode) != null ? new SupplierDao(db).GetByKey(supplierCode).SupplierName : "";
                }

                //������
                eHeader.PurchaseOrderDate = DateTime.Now;

                //�`�[�ԍ�
                eHeader.SlipNumber = header.SlipNumber;

                //�ڋq��
                eHeader.CustomerName = header.Customer.CustomerName;

                //�Ԏ햼
                eHeader.CarName = header.CarName;

                //�^��
                eHeader.ModelName = header.ModelName;

                //���N�x�o�^
                eHeader.FirstRegistration = header.FirstRegistration;

                //�ԑ�ԍ�
                eHeader.Vin = header.Vin;

                //�I�[�_�[�敪(����)
                eHeader.OrderTypeName = stockStatus.Name;

                //���[�J�[�I�[�_�[�ԍ�
                eHeader.MakerOrderNumber = "";     //�����_�ł͖��Ή��i���[�U�Ɏ���͂��Ă��炤�j

                //�C���{�C�X�ԍ�
                eHeader.InvoiceNumber = "";        //�����_�ł͖��Ή��i���[�U�Ɏ���͂��Ă��炤�j

                //���ɓ�
                eHeader.ArrivalPlanDate = header.ArrivalPlanDate;

                //�����p������
                eHeader.Memo = header.Memo;

                //�����`�[�ԍ�
                eHeader.PurchaseOrderNumber = "";  //�����_�ł͖��Ή��i���[�U�Ɏ���͂��Ă��炤�j

                //����(���ׂ�10�����擾���āA10���������ꍇ�͎��̃f�[�^�ɐݒ�
                eHeader.line = eline.Where(x => !string.IsNullOrWhiteSpace(x.StockStatus) && x.StockStatus.Equals(stockStatus.Code)).Skip(offset).Take(10).ToList();

                //���l���̐ݒ�
                foreach (var l in eHeader.line)
                {
                    //�����ϐ�
                    decimal ? orderedQuantity = null;

                    orderedQuantity = new ServiceSalesOrderDao(db).GetOutputQuantity(searchSlipNumber, l.PartsNumber, l.StockStatus);

                    //�ߋ��̔����������ꍇ
                    if (orderedQuantity == null)
                    {
                        l.Memo = "�V�K�ǉ�";
                    }
                    else //�ߋ��̔�����������
                    {
                         //���񔭒����鐔�ʂƉߋ��̐��ʂ��ς��Ă����ꍇ
                        if (l.Quantity != orderedQuantity)
                        {
                            l.Memo = "���ʂ� " + orderedQuantity + " �� " + l.Quantity + " �ɕύX����܂���";
                        }
                    }
                }

                //�O���̏ꍇ�͏����𔲂���
                if (eHeader.line.Count == 0)
                {
                    break;
                }
                
                //�I�t�Z�b�g�l�̍X�V
                offset += 10;

                //�V�[�g���̓I�[�_�[��ʁ{index
                eHeader.sheetNameIdx = eHeader.OrderTypeName + "_" + index;

                index++;

                eHeaderList.Add(eHeader);
            }
            
            return eHeaderList;
        }

        /// <summary>
        /// �����σt���O�̐ݒ�
        /// </summary>
        /// <param name="dbHeader">�w�b�_(db����擾)</param>
        /// <param name="line">����</param>
        /// <param name="form">�t�H�[���l</param>
        /// <return></return>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬
        /// </history>
        private void SetOrderedFlag(ServiceSalesHeader dbHeader, EntitySet<ServiceSalesLine> line, FormCollection form)
        {
            //�����σt���O��ݒ�
            dbHeader.ServiceSalesLine.Where
               (
                   x =>
                       x.OutputTargetFlag != null &&
                       x.OutputTargetFlag.Equals("1") &&
                       (
                       //���f = �uS/O,E/O���̔����v
                           string.IsNullOrWhiteSpace(form["StockCode"]) &&
                           (
                               x.c_StockStatus.StatusType.Equals("001")
                           )
                           ||
                           x.StockStatus.Equals(form["StockCode"])     //���f = �u�݌Ɂv
                        )
                ).ToList().ForEach(x => x.OutputFlag = "1");

            int index = 0;

            //���׍s�̓o�^������
            EntitySet<ServiceSalesLine> eline = new EntitySet<ServiceSalesLine>();

            //�ēx�o�^
            foreach (var l in dbHeader.ServiceSalesLine)
            {
                ServiceSalesLine newline = new ServiceSalesLine();

                newline.SlipNumber = l.SlipNumber;                      //�`�[�ԍ�
                newline.RevisionNumber = l.RevisionNumber;              //�����ԍ�
                newline.LineNumber = l.LineNumber;                      //�s�ԍ�
                newline.ServiceType = l.ServiceType;                    //���
                newline.SetMenuCode = l.SetMenuCode;                    //�Z�b�g���j���[�R�[�h
                newline.ServiceWorkCode = l.ServiceWorkCode;            //���ƃR�[�h
                newline.ServiceMenuCode = l.ServiceMenuCode;            //�T�[�r�X���j���[�R�[�h
                newline.PartsNumber = l.PartsNumber;                    //���i�ԍ�
                newline.LineContents = l.LineContents;                  //���i��
                newline.RequestComment = l.RequestComment;             
                newline.WorkType = l.WorkType;                          //��Ƌ敪
                newline.LaborRate = l.LaborRate;                        //���o���[�x
                newline.ManPower = l.ManPower;                          //
                newline.TechnicalFeeAmount = l.TechnicalFeeAmount;      //�Z�p��
                newline.Quantity = l.Quantity;                          //����
                newline.Price = l.Price;                                //�P��
                newline.Amount = l.Amount;                              //���z
                newline.Cost = l.Cost;                                  //����
                newline.EmployeeCode = l.EmployeeCode;
                newline.SupplierCode = l.SupplierCode;
                newline.CustomerClaimCode = l.CustomerClaimCode;
                newline.StockStatus = l.StockStatus;
                newline.CreateEmployeeCode = l.CreateEmployeeCode;
                newline.CreateDate = l.CreateDate;
                newline.LastUpdateEmployeeCode = l.LastUpdateEmployeeCode;
                newline.LastUpdateDate = l.LastUpdateDate;
                newline.DelFlag = l.DelFlag;
                newline.Classification1 = l.Classification1;
                newline.TaxAmount = l.TaxAmount;
                newline.UnitCost = l.UnitCost;
                newline.LineType = l.LineType;
                newline.ConsumptionTaxId = l.ConsumptionTaxId;
                newline.Rate = l.Rate;
                newline.ProvisionQuantity = l.ProvisionQuantity;
                newline.OrderQuantity = l.OrderQuantity;
                newline.DisplayOrder = l.DisplayOrder;
                newline.OutputTargetFlag = l.OutputTargetFlag;
                newline.OutputFlag = l.OutputFlag;

                eline.Add(newline);

                index++;
            }

            db.ServiceSalesLine.DeleteAllOnSubmit(dbHeader.ServiceSalesLine);

            db.ServiceSalesLine.InsertAllOnSubmit(eline);

            //DB�o�^
            db.SubmitChanges();
        }



        #endregion

        #region Validation�`�F�b�N
        /// <summary>
        /// ����Validation�`�F�b�N
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
        /// <history>
        /// 2020/02/17 yano #4025�y�T�[�r�X�`�[�z��ږ��Ɏd��ł���悤�ɋ@�\�ǉ�
        /// 2019/02/06 yano #3959 �T�[�r�X�`�[���́@��������
        /// 2018/11/09 yano #3953 �T�[�r�X�`�[���́@�[�Ԏ��̃}�X�^�`�F�b�N�ŃV���[�g�p�[�c�A�Г����B���i�̓}�X�^�`�F�b�N���s��Ȃ�
        /// 2018/08/29 yano #3932 �T�[�r�X�`�[_�����斢���͂Ŕ[�ԏ������s����
        /// 2018/08/29 yano #3925 �T�[�r�X�`�[�@���ׂɃ}�X�^���o�^�̕��i�����݂����܂ܔ[�Ԃł���
        /// 2018/05/30 arc yano #3889 �T�[�r�X�`�[�������i����������Ȃ�
        /// 2018/05/17 arc yano #3884 �T�[�r�X�`�[�@�ԍ��������Ɍ��`�[�̔[�ԓ��̒��߃`�F�b�N���s���Ă���
        /// 2018/02/20 arc yano #3858 �T�[�r�X�`�[�@�[�Ԍ�̕ۑ������ŁA�[�ԓ����󗓂ŕۑ��ł��Ă��܂�
        /// 2017/11/03 arc yano #3732 �ԍ��`�[�쐬�Łu���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂��� �v�\�� ����(form)�ǉ��@�ԁE�ԍ��̏ꍇ�̓}�X�^�`�F�b�N���Ȃ�
        /// 2017/11/03 arc yano #3774 �T�[�r�X�`�[�̍��`�[�ŁA���ׂ̈�s�ڂ����ƈȊO�ɂ���Ɨ����� ���`�[�̎����`�F�b�N����悤�ɏ�����ύX����
        /// 2017/07/03 arc yano #3776 �T�[�r�X�`�[�@����q�ɂ��g�p���Ă��镔��ύX���̈����̕s�
        /// 2017/02/08 arc yano #3645 ���׊m�莞�̃G���[�Ή� LineContents�̒����̃`�F�b�N�l��25��50�ɕύX(DB�̃T�C�Y�ɍ��킹��)
        /// 2017/01/31 arc yano #3566 �T�[�r�X�`�[���́@����ύX���̍݌ɂ̍Ĉ��� ���傪�ύX���ꂽ�ꍇ�́A�����ϐ��͂O�Ƃ���validation�`�F�b�N���s��
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2016/01/26 arc yano #3406 �T�[�r�X�`�[�@��������validaion�G���[���b�Z�[�W�o�̗͂}�� �����`�F�b�N�̈����ǉ�
        /// </history>
        private void ValidateAllStatus(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines, FormCollection form) {

            if (header.RevisionNumber != 0) {
                ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
                string employeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                if (target.ProcessSessionControl != null && !target.ProcessSessionControl.EmployeeCode.Equals(employeeCode)) {
                    ModelState.AddModelError("", "���̓`�[��" + target.ProcessSessionControl.Employee.EmployeeName + "����ɂ���ĕҏW���ꂽ���ߕۑ��o���܂���B�u����v�{�^���œ`�[����ĉ������B");
                    return;
                }
            }

            //vs2012�Ή� 2014/04/17 arc.ookubo
            //if (!header.SlipNumber.Contains("-")) {
            if ((header.SlipNumber == null) || (!header.SlipNumber.Contains("-1"))) //Mod 2017/11/03 arc yano #3774
            {

                //vs2012�Ή� 2014/04/17 arc.ookubo
                if (header.CustomerCode == null)
                {
                    header.CustomerCode = string.Empty;
                }
                //�K�{�`�F�b�N
                CommonValidate("DepartmentCode", "����", header, true);
                CommonValidate("FrontEmployeeCode", "�t�����g�S����", header, true);
                CommonValidate("ReceiptionEmployeeCode", "��t�S����", header, true);
                CommonValidate("QuoteDate", "���ϓ�", header, true);
                CommonValidate("InspectionExpireDate", "�Ԍ��L������", header, false);
                CommonValidate("CarTax", "�����ԐŎ�ʊ�", header, false);    //Mod 2019/09/04 yano #4011
                CommonValidate("CarLiabilityInsurance", "�����ӕی���", header, false);
                CommonValidate("CarWeightTax", "�����ԏd�ʐ�", header, false);
                CommonValidate("NumberPlateCost", "�i���o�[��", header, false);
                CommonValidate("FiscalStampCost", "�󎆑�", header, false);

                CommonValidate("TaxFreeFieldValue", "���̑�", header, false);  //Mod 2020/02/17 yano #4025

                //Add 2020/02/17 yano #4025
                CommonValidate("OptionalInsurance", "�C�ӕی�", header, false);
                CommonValidate("SubscriptionFee", "�T�[�r�X������", header, false);
                CommonValidate("TaxableFreeFieldValue", "���̑�(�ې�)", header, false);
                
                
                // Add 2015/05/07 arc nakayama #3083_�T�[�r�X�œ`�[�����o���Ȃ� �ۑ����ɂ��ڋq�̕K�{�`�F�b�N���s��
                CommonValidate("CustomerCode", "�ڋq", header, true);
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("001"))
                {
                    CommonValidate("QuoteExpireDate", "���ϗL������", header, true);

                    //���ϗL�������͌��ϓ��ȍ~�����F�߂Ȃ�
                    if (ModelState.IsValidField("QuoteExpireDate") && header.QuoteDate != null && header.QuoteExpireDate != null) {
                        if (header.QuoteDate != null && DateTime.Compare(header.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, header.QuoteExpireDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                            ModelState.AddModelError("QuoteExpireDate", MessageUtils.GetMessage("E0013", new string[] { "���ϗL������", "���ϓ��ȍ~" }));
                        }
                    }
                }
                CommonValidate("ArrivalPlanDate", "���ɓ�", header, false);

                //���x�o�^
                if (!string.IsNullOrEmpty(header.FirstRegistration)) {
                    if (!Regex.IsMatch(header.FirstRegistration, "([0-9]{4})/([0-9]{2})")
                        && !Regex.IsMatch(header.FirstRegistration, "([0-9]{4}/[0-9]{1})")) {
                        ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "���x�o�^"));
                    }
                    DateTime result;
                    try {
                        DateTime.TryParse(header.FirstRegistration + "/01", out result);
                        if (result.CompareTo(DaoConst.SQL_DATETIME_MIN) < 0) {
                            ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "���x�o�^"));
                            if (ModelState["FirstRegistration"].Errors.Count() > 1) {
                                ModelState["FirstRegistration"].Errors.RemoveAt(0);
                            }
                        }
                    } catch {
                        ModelState.AddModelError("FirstRegistration", MessageUtils.GetMessage("E0019", "���x�o�^"));
                    }

                }
                //Edit 2014/06/16 arc yano �������Ή� �擪�ɍ폜�s�̉\�������邽�߁A����1�s�ڂ�line[0]�ł͂Ȃ�line[x]
                int count = 0;
                //����1�s�ڂ����ƕK�{
                if (lines == null || lines.Count == 0)  //���׍s�����A�����ꍇ
                {
                    ModelState.AddModelError(string.Format("line[{0}].{1}", "0", "LineContents"), "���ׂ�1�s�ڂ͎��Ƃł���K�v������܂�");
                }
                else
                {
                    for (count = 0; count < lines.Count; count++)
                    {
                        if (string.IsNullOrEmpty(lines[count].CliDelFlag) || ((lines[count].CliDelFlag != null) && !(lines[count].CliDelFlag.Equals("1"))))
                        {
                            break;
                        }
                    }
                    if (count >= lines.Count || (lines[count].ServiceType != null && !(lines[count].ServiceType.Equals("001"))))
                    {
                        ModelState.AddModelError(string.Format("line[{0}].{1}", "0", "LineContents"), "���ׂ�1�s�ڂ͎��Ƃł���K�v������܂�");
                    }
                }

                //int chkflg = 0;     //Add 2014/06/27 arc yano �T�[�r�X�`�[�`�F�b�N�V�V�X�e���Ή�
                int posServiceWork = 0;

                bool displayFlag = false;   //�\���σt���O     //Mod 2016/01/26 arc yano

                //decimal totalAmount = 0;            //���z���v
                //decimal totalTechFee = 0;           //�Z�p�����v
                //decimal totalSmCost = 0;            //����(���v)���v/�T�[�r�X���j���[
                //decimal totalPtCost = 0;            //����(���v)���v/���i

                //���׍s�̃`�F�b�N
                ServiceSalesHeader dbheader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);      //Add 2017/01/31 arc yano #3566

                if (lines != null && lines.Count > 0)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        ServiceSalesLine line = lines[i];
                        //Add 2014/06/16 arc yano ���͌��؂�CliDelFlag��1�ȊO(��\�����R�[�h)�ɑ΂��čs���B
                        if (string.IsNullOrEmpty(line.CliDelFlag) || ((line.CliDelFlag != null) && !(line.CliDelFlag.Equals("1"))))
                        {

                            //��ʂ��u���Ɓv�̏ꍇ
                            if (!string.IsNullOrEmpty(line.ServiceType) && line.ServiceType.Equals("001"))
                            {
                                if (string.IsNullOrEmpty(line.ServiceWorkCode))     //���ƃR�[�h��������
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ServiceWorkCode"), MessageUtils.GetMessage("E0001", "���ƃR�[�h"));
                                    //chkflg = 0;
                                }
                                else
                                {
                                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁAnull�`�F�b�N�ǉ�
                                    if (line.ServiceWorkCode != null && line.ServiceWorkCode.Substring(0, 1).Equals("2"))    //���ƃR�[�h���u�Г������v
                                    {
                                        //-----------------------
                                        //�Г������`�F�b�N
                                        //-----------------------
                                        string CustomerClaimType = null;
                                        if (!string.IsNullOrEmpty(line.CustomerClaimCode))
                                        {
                                            CustomerClaimType = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode).CustomerClaimType; //������̃^�C�v���擾
                                        }

                                        if (!string.IsNullOrEmpty(CustomerClaimType) && !CustomerClaimType.Equals("005")) //������^�C�v���Г��ȊO
                                        {
                                            if (!ModelState.ContainsKey(string.Format("line[{0}].{1}", i, "ServiceWorkCode")))   //���ƃR�[�h�ɑ΂��Č��؃G���[�������̏ꍇ�B
                                            {
                                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ServiceWorkCode"), "");
                                            }

                                            ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), MessageUtils.GetMessage("E0022", new string[] { "���ƃR�[�h���Г�����", "������ɐ�����^�C�v���Г��̂��̂�ݒ肷��K�v������܂��B" }));
                                        }

                                        //chkflg = 1;             //�Г��e���`�F�b�N���s  //Del 2014/07/17 arc yano �T�[�r�X�`�[�`�F�b�N�V�V�X�e���Ή� �Г��e���`�F�b�N�̓N���C�A���g�ōs���B
                                        posServiceWork = i;     //���ƃ��R�[�h�̈ʒu��Ҕ�
                                    }
                                    else
                                    {
                                        //chkflg = 0;             //�`�F�b�N�Ȃ�
                                    }
                                }
                            }

                            if (line.Quantity != null)
                            {
                                if ((Regex.IsMatch(line.Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$") || (Regex.IsMatch(line.Quantity.ToString(), @"^\d{1,7}$"))))
                                {
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "Quantity"), MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" }));
                                    if (ModelState[string.Format("line[{0}].{1}", i, "Quantity")].Errors.Count > 1)
                                    {
                                        ModelState[string.Format("line[{0}].{1}", i, "Quantity")].Errors.RemoveAt(0);
                                    }
                                }
                            }
                            if (line.ManPower != null)
                            {
                                if ((Regex.IsMatch(line.ManPower.ToString(), @"^\d{1,3}\.\d{1,2}$") || (Regex.IsMatch(line.ManPower.ToString(), @"^\d{1,5}$"))))
                                {
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Format("line[{0}].{1}", i, "ManPower"), MessageUtils.GetMessage("E0002", new string[] { "�H��", "���̐���5���ȓ�������2���ȓ�" }));
                                    if (ModelState[string.Format("line[{0}].{1}", i, "ManPower")].Errors.Count > 1)
                                    {
                                        ModelState[string.Format("line[{0}].{1}", i, "ManPower")].Errors.RemoveAt(0);
                                    }
                                }
                            }

                            //Mod 2017/02/08 arc yano #3645
                            if (!string.IsNullOrEmpty(line.LineContents) && line.LineContents.Length > 50)
                            {
                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "LineContents"), "�\������50�����ȓ��œ��͂��Ă�������");
                            }

                            //Mod 2015/10/28 arc yano #3289 �T�[�r�X�`�[ ���i�d���@�\�̉��P(�T�[�r�X�`�[����)
                            if (
                                header.ServiceOrderStatus.Equals("002") ||
                                header.ServiceOrderStatus.Equals("003") ||
                                header.ServiceOrderStatus.Equals("004") ||
                                header.ServiceOrderStatus.Equals("005")
                                )
                            {

                                //Add 2017/01/31 arc yano #3566
                                decimal? provisionQuantity;

                                //��ʓ��͒l�̕����DB�ɓo�^����Ă��镔�傪�قȂ�ꍇ(=���傪�ύX���ꂽ�ꍇ)
                                if (dbheader != null && !dbheader.DepartmentCode.Equals(header.DepartmentCode))
                                {
                                    //Mod 2107/07/03 arc yano #3776
                                    //�ύX�O����R�[�h����g�p�q�ɂ����o��
                                    DepartmentWarehouse prdWarehouse = CommonUtils.GetWarehouseFromDepartment(db, dbheader.DepartmentCode);

                                    //�ύX�㕔��R�[�h����g�p�q�ɂ����o��
                                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                                    //�ύX�O�̎g�p�q�ɂƕύX��̎g�p�q�ɂ��قȂ�ꍇ
                                    if (dWarehouse != null && !prdWarehouse.WarehouseCode.Equals(dWarehouse.WarehouseCode))
                                    {
                                        //�݌ɐ��̍Ď擾
                                        line.PartsStock = new PartsStockDao(db).GetStockQuantity(line.PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));

                                        //�����ϐ��̓��Z�b�g
                                        provisionQuantity = 0m;
                                    }
                                    else //�g�p�q�ɂ�����̏ꍇ
                                    {
                                        //�����ϐ��̓��Z�b�g���Ȃ�
                                        provisionQuantity = line.ProvisionQuantity;
                                    }
                                }
                                else
                                {
                                    provisionQuantity = line.ProvisionQuantity;
                                }

                                ValidateHikiate(line, i, ref displayFlag, provisionQuantity);
                            }

                            //Add 2018/08/29 yano #3925, #3932
                            //�[�ԏ����A�܂��͓`�[�C�������̏ꍇ
                            if (
                                   (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") && CommonUtils.DefaultString(header.ActionType).Equals("Sales")) ||
                                   (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd"))
                               )
                            {
                                //Mod 2018/11/09 yano #3953
                                //-------------------
                                //�}�X�^�`�F�b�N
                                //-------------------
                                //��ʂ����i�̏ꍇ�ō݌ɔ��f���u�Г����B�v�u�V���[�g�p�[�c�v�łȂ��ꍇ
                                if (line.ServiceType.Equals("003") && (!string.IsNullOrWhiteSpace(line.StockStatus) && !line.StockStatus.Equals("997") && !line.StockStatus.Equals("998")))
                                {
                                    //���i�}�X�^
                                    Parts parts = new PartsDao(db).GetByKey(line.PartsNumber);

                                    //�}�X�^�ɖ����ꍇ
                                    if (parts == null)
                                    {
                                        ModelState.AddModelError(string.Format("line[{0}].{1}", i, "PartsNumber"), "���i�}�X�^�ɖ��o�^�̕��i�̂��߁A�[�ԏ������s���܂���B���i�}�X�^�ɓo�^��ɍēx�[�ԏ������s���ĉ�����");
                                    }
                                }
                                //������}�X�^
                                //���Ƃ̏ꍇ�̂݃`�F�b�N���s��
                                if (line.ServiceType.Equals("001"))
                                {
                                    //������R�[�h�������͂̏ꍇ
                                    if (string.IsNullOrWhiteSpace(line.CustomerClaimCode))
                                    {
                                        ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "�����悪�����͂̂��߁A�[�ԏ������s���܂���B���������͌�ɍēx�[�ԏ������s���ĉ�����");
                                    }
                                    else //�}�X�^�`�F�b�N
                                    {
                                        CustomerClaim customerclaim = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode);

                                        if (customerclaim == null)
                                        {
                                            ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "������}�X�^�ɖ��o�^�̐�����̂��߁A�[�ԏ������s���܂���B������}�X�^�ɓo�^��ɍēx�[�ԏ������s���ĉ�����");
                                        }
                                        //Add 2019/02/06 yano #3959
                                        else
                                        {
                                            string makercode = (new DepartmentDao(db).GetByKey(header.DepartmentCode) != null ? new DepartmentDao(db).GetByKey(header.DepartmentCode).MainMakerCode : "");

                                            ServiceWorkCustomerClaim swCustomerClaim = new ServiceWorkCustomerClaimDao(db).GetByKey(line.ServiceWorkCode, makercode);

                                            ////���Ƃɑ΂��Đ����悪�ݒ肳��Ă���A�����ׂ̐����悪���Ƃɐݒ肳�ꂽ������ƈقȂ�ꍇ
                                            if (swCustomerClaim != null && !line.CustomerClaimCode.Equals(swCustomerClaim.CustomerClaimCode))
                                            {
                                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "CustomerClaimCode"), "���Ƃ� �y" + line.LineContents + "�z �̏ꍇ�͐������ �w" + swCustomerClaim.CustomerClaim.CustomerClaimName + "(�R�[�h=" + swCustomerClaim.CustomerClaimCode + ")�x ��ݒ肵�Ă�������");

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Mod 2018/05/17 arc yano #3884
            //Mod 2018/02/20 arc yano #3858
            //Mod 2015/05/07 arc yano �������ҏW�����ǉ� �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�[�ԓ��̕ύX���s����
            //                        ���̑��A������(�`�[�C���������̓��̓`�F�b�N�̍폜(��p�̃`�F�b�N����[ValidateForModification]�ōs��))
            //Mod 2015/04/02 arc nakayama �[�ԓ��`�F�b�N�Ή��@�ۑ������[�ԓ��̃`�F�b�N���s��
            //Mod 2015/03/25 arc nakayama �`�[�C���Ή��@�C���������������`�F�b�N��������悤�ɕύX
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //�[�ԏ������܂��͔[�ԍςł̕ۑ������͔[�ԓ��K�{
            if (
                (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") && CommonUtils.DefaultString(header.ActionType).Equals("Sales"))  ||
                (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && (CommonUtils.DefaultString(header.ActionType).Equals("Update") || CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd")))
                )
            //if ((CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("005") &&
            //    CommonUtils.DefaultString(header.ActionType).Equals("Sales")) || (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("Update")))
                //CommonUtils.DefaultString(header.ActionType).Equals("Sales") || CommonUtils.DefaultString(header.ActionType).Equals("ModificationEnd") || (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("006") && CommonUtils.DefaultString(header.ActionType).Equals("Update")))
            {
                int ret = 0;

                CommonValidate("SalesDate", "�[�ԓ�", header, true);
                if (header.SalesDate != null) {
                    
                    // Del 2016/08/13 arc yano �Â��R�[�h�͍폜
                    // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                    // Mod 2015/02/03 arc nakayama �`�[�C���Ή��@�C�������̏ꍇ�͏C���O�ƏC����ŕω����������������`�F�b�N���s��

                    //Mod 2016/08/13 arc yano #3596
                    //����R�[�h����g�p�q�ɂ����o��
                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode); ;

                    //�`�[�ۑ���
                    if (CommonUtils.DefaultString(header.ActionType).Equals("Update"))
                    {
                        //�@�ҏW�O�ƍ�������������`�F�b�N
                        if (header.SalesDate != new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber).SalesDate)
                        {
                            ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                        }
                    }
                    else //�[�ԏ�����
                    {
                        ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                    }
                    
                    switch (ret)
                    {
                        //���i�I���ςɂ��ҏW�s��
                        case 1:
                            ModelState.AddModelError("SalesDate", "���i�I�����������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                            break;
                           

                        //�������ςɂ��ҏW�s��
                        case 2:
                            ModelState.AddModelError("SalesDate", "���������������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                            break;
                            

                        default:
                            //�������Ȃ�
                            break;
                    }
                
                }
            }

            //-----------------------------------
            //�}�X�^�`�F�b�N
            //-----------------------------------
            //Mod 2017/11/03 arc yano #3732
            //�ԓ`�E�ԍ������ȊO�A�܂��͐ԓ`�A���`�̕ۑ��ȊO
            if ((string.IsNullOrWhiteSpace(form["Mode"]) || (!form["Mode"].Equals("1") && !form["Mode"].Equals("2"))) && !string.IsNullOrWhiteSpace(header.SlipNumber) && !header.SlipNumber.Contains("-") )
            {
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                //ADD 2014/02/20 ookubo
                //�[�ԑO�X�e�[�^�X�܂Ŕ[�ԗ\����K�{
                if (CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("001") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("002") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("003") ||
                    CommonUtils.DefaultString(header.ServiceOrderStatus).Equals("004"))
                {
                    CommonValidate("SalesPlanDate", "�[�ԗ\���", header, true);
                }

                // Add 2015/05/18 arc nakayama ���b�N�A�b�v�������Ή��@�����f�[�^�����͉\�ɂȂ邽�߁A�}�X�^�`�F�b�N���s��
                //�ԗ�
                if (!string.IsNullOrEmpty(header.SalesCarNumber))
                {
                    SalesCar SalesCarData = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                    if (SalesCarData == null)
                    {
                        ModelState.AddModelError("SalesCarNumber", "���͂���Ă���Ǘ��ԍ��̓}�X�^�ɑ��݂��Ă��܂���B");
                    }
                }

                //�ڋq
                Customer CustomerData = new CustomerDao(db).GetByKey(header.CustomerCode);
                if (CustomerData == null)
                {
                    ModelState.AddModelError("CustomerCode", "���͂���Ă���ڋq�R�[�h�̓}�X�^�ɑ��݂��Ă��܂���B");
                }

                //�Ј�
                if (!string.IsNullOrEmpty(header.ReceiptionEmployeeCode))
                {
                    Employee EmployeeData = new EmployeeDao(db).GetByKey(header.ReceiptionEmployeeCode);
                    if (EmployeeData == null)
                    {
                        ModelState.AddModelError("ReceiptionEmployeeCode", "���͂���Ă���Ј��R�[�h�̓}�X�^�ɑ��݂��Ă��܂���B");
                    }
                }

                //����
                if (!string.IsNullOrEmpty(header.DepartmentCode))
                {
                    Department DepartmentData = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                    if (DepartmentData == null)
                    {
                        ModelState.AddModelError("DepartmentCode", "���͂���Ă��镔��R�[�h�̓}�X�^�ɑ��݂��Ă��܂���B");
                    }
                }
                //���J�S����
                if (lines != null && lines.Count > 0)
                {
                    for (int i = 0; i < lines.Count; i++)
                    {
                        ServiceSalesLine line = lines[i];
                        if (!string.IsNullOrEmpty(line.EmployeeCode))
                        {
                            Employee EmployeeData = new EmployeeDao(db).GetByKey(line.EmployeeCode);
                            if (EmployeeData == null)
                            {
                                ModelState.AddModelError(string.Format("line[{0}].{1}", i, "EmployeeCode"), "���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂���");
                            }
                        }
                    }
                }
            }
            //Add 2015/05/22 arc nakayama #3208_�T�[�r�X�`�[�̈��p�����̕����������𒴂���ƃV�X�e���G���[�ɂȂ� �����p�������̕������`�F�b�N�ǉ��i200�����j
            if (header.Memo != null && header.Memo.Length > 400)
            {
                ModelState.AddModelError("Memo", "�����p��������400�����ȓ��œ��͂��ĉ������i���s��2�������Ƃ݂Ȃ���܂��j");
            }

            //Add 2017/04/27 arc nakayanma #3744_[����] ���|�����������o�b�`���N���ƂȂ�s����C
            if ((header.ActionType.Equals("SalesOrder") && !string.IsNullOrWhiteSpace(header.Vin)) || (!header.ServiceOrderStatus.Equals("001") && !string.IsNullOrWhiteSpace(header.Vin)))
            {
                if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
                {
                    ModelState.AddModelError("CustomerCode", "�󒍈ȍ~�͌ڋq�R�[�h�ɏ�l���g�p���邱�Ƃ͂ł��܂���B");
                }
            }

            //Add 2018/05/30 arc yano #3889
            //------------------------------------------------------
            //�ʃ��[�U�ɂ��`�[���X�V����Ă����ꍇ�̓G���[�Ƃ���
            //------------------------------------------------------
            //DB����ŐV�̓`�[���擾
            ServiceSalesHeader dbHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            if (dbHeader != null && dbHeader.RevisionNumber > header.RevisionNumber)
            {
                ModelState.AddModelError("RevisionNumber", "�ۑ����s�����Ƃ��Ă���`�[�͍ŐV�ł͂���܂���B�ŐV�̓`�[���J����������ŕҏW���s���ĉ�����");
            }

        }

        /// <summary>
        /// ���i�������o�͎���validation�`�F�b�N
        /// </summary>
        /// <param name="form">�t�H�[���̒l</param>
        /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
        /// <param name="lines">�T�[�r�X�`�[����</param>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬
        /// </history>
        private void ValidateOutput(FormCollection form, ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) 
        {
            
            int cnt = lines.Where
                    (
                        x => 
                            x.OutputTarget.Contains("true") && 
                            (
                                //���f = �uS/O,E/O���̔����v
                                string.IsNullOrWhiteSpace(form["StockCode"]) &&
                                (
                                    x.c_StockStatus != null && 
                                    x.c_StockStatus.StatusType.Equals("001") || 
                                    new CodeDao(db).GetStockStatus(false, x.StockStatus, "").StatusType.Equals("001")
                                )
                                ||
                                x.StockStatus.Equals(form["StockCode"])     //���f = �u�݌Ɂv
                             )
                     ).Count();


            //�擾�����f�[�^��0���̏ꍇ�͏����I��
            if (cnt == 0)
            {
                ModelState.AddModelError("", "���ׂɕ\������f�[�^������܂���");
            }

            return;
        }

        /// <summary>
        /// �������ҏW�����L�^���ɂ��f�[�^�ҏW�^�s���菈��
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="targetDate">�Ώۓ��t</param>
        /// <return>�`�F�b�N����(0:�ҏW�� 1:�ҏW�s��(������=���������A���i�I��=�����@���������ҏW�����Ȃ����[�U�����̏�ԂɂȂ�) 2:�ҏW�s��(��������))</return>
        /// <history>
        /// 2021/11/09 yano #4111�y�T�[�r�X�`�[���́z�������̔[�ԍϓ`�[�̔[�ԓ��ҏW�`�F�b�N�����̕s�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2015/05/07 arc yano �������ҏW�����ǉ�  �V�K�ǉ� �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�ł��A�ύX�\�Ƃ���
        /// </history>
        private int CheckTempClosedEdit(DepartmentWarehouse dWarehouse, DateTime? targetDate)
        {
            string securityCode = ((Employee)Session["Employee"]).SecurityRoleCode;     //�Z�L�����e�B�R�[�h
            bool editFlag = false;                                                      //�������ҏW����

            int ret = 0;                                                                //�ҏW��

            //����R�[�h
            string departmentCode = dWarehouse != null ? dWarehouse.DepartmentCode : "";
            //�q�ɃR�[�h
            string warehouseCode = dWarehouse != null ? dWarehouse.WarehouseCode : "";

            //�������ҏW�������擾
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(securityCode, "EditTempClosedData");

            if (rec != null)
            {
                editFlag = rec.EnableFlag;
            }

            //���ƂȂ镔��S�ĂŃ`�F�b�N���s���A��ł��Y��������̂�����΂�����̗p����
            //�ҏW��������̏ꍇ
            if (editFlag == true)
            {
                //�������̂݃`�F�b�N   ������P�ʂŃ`�F�b�N
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, targetDate, "001", securityCode))
                {
                    ret = 2;
                }
            }
            else
            {
                //���i�I���`�F�b�N �@���q�ɒP�ʂŃ`�F�b�N
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(warehouseCode, targetDate, "002"))
                {
                    ret = 1;
                }
                else //Mod 2021/11/09 yano #4111
                {
                    //���i�I���`�F�b�N�Ŗ��Ȃ��ꍇ�A�������`�F�b�N   ������P�ʂŃ`�F�b�N
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, targetDate, "001", securityCode))
                    {
                        ret = 2;
                    }
                }
            }
         
            return ret;
        }
        /// <summary>
        /// �󒍏����̎���Validation�`�F�b�N
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
        /// <param name="lines">�T�[�r�X�`�[�f�[�^(���׍s)</param>
        /// <history>
        /// 2017/01/31 arc yano #3566 �T�[�r�X�`�[���́@����ύX���̍݌ɂ̍Ĉ��� ���傪�ύX����Ă���ꍇ�͈����ϐ��͂O�Ƃ���validation�`�F�b�N���s��
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���P�[�V�����擾�̈����𕔖�R�[�h����q�ɃR�[�h�ɕύX
        /// 2016/04/11 arc yano #3487 �[�ԏ������̖�����validation�`�F�b�N ���������O��validation���s���悤�ɏC��
        /// 2016/01/26 arc yano #3406_�T�[�r�X�`�[ ��������validation�G���[���b�Z�[�W�o�̗͂}�� (#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�) ����(validation�����t���O)�ǉ�
        /// 2016/01/25 arc yano #3407_�T�[�r�X�`�[ ��������validation�`�F�b�N�̕s��Ή� (#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�) ����(�s��)�ǉ�
        /// </history>
        private void ValidateSalesOrder(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) {
            CommonValidate("CustomerCode", "�ڋq", header, true);

            if (header.GrandTotalAmount == null) {
                ModelState.AddModelError("GrandTotalAmount", "���z���������ݒ�ł��Ă��܂���");
            }
            //if (!header.GrandTotalAmount.Equals(header.PaymentTotalAmount)) {
            //    ModelState.AddModelError("PaymentTotalAmount", "�������v���x�����v�ɂȂ��Ă��܂���");
            //}
            //�Ȗڂ��擾
            Account serviceAccount = new AccountDao(db).GetByUsageType("SR");
            if (serviceAccount == null) {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
            }
            if (header.ServiceSalesPayment != null) {
                List<ServiceSalesPayment> pay = header.ServiceSalesPayment.ToList();
                for(int i=0;i<header.ServiceSalesPayment.Count;i++) {
                    CustomerClaim claim = new CustomerClaimDao(db).GetByKey(pay[i].CustomerClaimCode);
                    if (string.IsNullOrEmpty(claim.CustomerClaimType)) {
                        ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "�w�肳�ꂽ������̐������ʂ��ݒ肳��Ă��܂���");
                    }
                    //�N���W�b�g��Ђ̏ꍇ�A���Ϗ������K�{
                    if (claim != null && claim.CustomerClaimType != null && claim.CustomerClaimType.Equals("003")) {
                        if (claim.CustomerClaimable == null || claim.CustomerClaimable.Count == 0) {
                            ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "�w�肳�ꂽ������Ɍ��Ϗ������ݒ肳��Ă��܂���");
                        }
                    }
                }
            }
            /* //2016/02/10 �������P�[�V�����p�~�̂��߁Avalidation�������R�����g�A�E�g
                Location hikiateLocation = stockService.GetHikiateLocation(header.DepartmentCode);
                if (hikiateLocation == null) {
                    ModelState.AddModelError("", "������Ɉ������P�[�V������1���ݒ肳��Ă��܂���");
                }
            */

            //�d�|���P�[�V�������擾����
            //�`�[�̕���R�[�h����A�g�p�q�ɂ�����o��
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
           
            string warehouseCode = "";
            if (dWarehouse != null)
            {
                warehouseCode = dWarehouse.WarehouseCode;
            }

            //Location shikakariLocation = stockService.GetShikakariLocation(header.DepartmentCode);
            Location shikakariLocation = stockService.GetShikakariLocation(warehouseCode);
            if (shikakariLocation == null) {
                ModelState.AddModelError("", "�Y�����傪�g�p���Ă���q�ɓ��Ɏd�|���P�[�V������1���ݒ肳��Ă��܂���");
            }

            int index = 0;
            bool displayFlag = false;   //�\���σt���O     //Mod 2016/01/26 arc yano

            foreach (var l in lines)
            {
                //���f = �u�݌ɂ̏ꍇ�v
                if( !string.IsNullOrWhiteSpace(l.StockStatus) && l.StockStatus.Equals("999"))       //Mod 2016/01/25 arc yano
                {
                    decimal? provisionQuantity = l.ProvisionQuantity;

                    ValidateHikiate(l, index, ref displayFlag, provisionQuantity);
                }
                index++;
            }

            //Mod 2016/04/11 #3487 arc yano
            if (header.ActionType.Equals("Sales"))
            {
                ValidateForSales(header, lines);
            }
        }

        /// <summary>
        /// �[�ԏ�������Validation�`�F�b�N
        /// </summary>
        /// <param name="header"></param>
        /// <param name="lines"></param>
        /// <history>
        /// Add 2016/11/11 arc yano #3656 �[�ԏ�������validation�`�F�b�N�������O����
        /// </history>
        private void ValidateForSales(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines)
        {
            int cnt = 0;
            bool msgflg = false;

            foreach (var line in lines)
            {
                    //���׍s�̒��ł��̕��i���݌ɊǗ��ΏۂŁA�����O���i(�Г����B�i)����Օi�łȂ��ꍇ
                if (new PartsDao(db).IsInventoryParts(line.PartsNumber) && !line.StockStatus.Equals("998") && !line.StockStatus.Equals("997"))
                        {
                    //�������s�����`�F�b�N
                    if ((line.Quantity - line.ProvisionQuantity) > 0)
                            {
                        //ModelState.AddModelError("line[" + cnt + "].ProvisionQuantity", "���������i�����邽�߁A�[�Ԃł��܂���");
                        msgflg = true;
                        break;
                        }
                    }
                    cnt++;
                }

            if (msgflg == true)
            {
                //���׍s�̈����ϐ���DB�̒l�ōX�V����
                service.ResetProvisionQuantity(header, lines);
                ModelState.AddModelError("", "���������i�����邽�߁A�[�Ԃł��܂���ł����B�f�[�^��ǂݒ����܂����̂ŁA�����ϐ����m�F��A�ēx�[�ԏ������s���ĉ�����");
            }
        }

        /// <summary>
        /// �ԓ`��������Validation�`�F�b�N
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/02/22 arc yano #3849 �V���[�g�p�[�c�A�Г����B�i�̐ԓ`�E�ԍ��������̃`�F�b�N�������O
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���P�[�V�����擾�̈����𕔖�R�[�h����q�ɃR�[�h�ɕύX
        /// </history>
        private void ValidateForAkaden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> lines) {
            if (string.IsNullOrEmpty(header.Reason)) {
                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�ԓ`����", "���R" }));
            }
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "���R��1024�����ȓ��œ��͂��ĉ�����");
            }

            ServiceSalesHeader target = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            if (target != null) {
                ModelState.AddModelError("SlipNumber", "���łɏ�������Ă��܂�");
            }

            //Mod 2018/02/22 arc yano #3849 �V���[�g�p�[�c�A�Г����B�ɂ��Ă͑ΏۊO
            //Mod 2014/06/16 arc yano �������Ή� CliDelFlag��"1"�ȊO�̂��̂��`�F�b�N����B
            var partsList = from a in lines
                            where a.ServiceType.Equals("003")
                            && (a.DelFlag == null || !a.DelFlag.Equals("1"))
                            && (string.IsNullOrEmpty(a.CliDelFlag) || !a.CliDelFlag.Equals("1"))
                            && !string.IsNullOrEmpty(a.PartsNumber)
                            && !a.PartsNumber.Contains("DISCNT")
                            && (!string.IsNullOrWhiteSpace(a.StockStatus) && !a.StockStatus.Equals("997") && !a.StockStatus.Equals("998"))
                            group a by a.PartsNumber into parts
                            select new {
                                PartsNumber = parts.Key,
                                Quantity = parts.Sum(x => x.Quantity)
                            };

            //PartsLocation location = new PartsLocationDao(db).GetByKey(item.PartsNumber, header.DepartmentCode);

            //Mod 2016/08/13 arc yano #3596
            //Location location = stockService.GetDefaultLocation(item.PartsNumber, header.DepartmentCode);
            //����R�[�h����g�p���Ă���q�ɂ����o��
            DepartmentWarehouse dWarehous = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            string warehouseCode = "";
            if (dWarehous != null)
            {
                warehouseCode = dWarehous.WarehouseCode;
            }

            foreach (var item in partsList) {
                
                Location location = stockService.GetDefaultLocation(item.PartsNumber, warehouseCode);
                if (location == null) {
                    ModelState.AddModelError("", "���i" + item.PartsNumber + "�̃��P�[�V��������`����Ă���܂���B���P�[�V�������Ē�`��ɁA�ԓ`�i�ԍ��j�������ēx���{���肢�������܂��B");
                }
            }
        }

        #endregion
        
        //Add 2015/03/23 arc nakayama �`�[�C���Ή��@�`�[�C������validation�`�F�b�N�ǉ�
        #region �`�[�C������validation�`�F�b�N
        /// <summary>
        /// �`�[�C������validation�`�F�b�N
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/02/23 arc yano #3849  �V���[�g�p�[�c�A�Г����B�i�̐ԓ`�E�ԍ��������̃`�F�b�N �ގ� �`�[�C�����ɐ��ʂ��ς��Ă��āA���Ώۂ̃��P�[�V�����������ꍇ�̓G���[�Ƃ��� �����ǉ�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� ���ߏ����󋵂̃`�F�b�N�����̈�����ύX(header.DepartmentCode �� warehouseCode)
        /// </history>
        private void ValidateForModification(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line)
        {
            if (string.IsNullOrEmpty(header.Reason))
            {
                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�C������", "���R" }));
            }
            if (header.Reason.Length > 1024)
            {
                ModelState.AddModelError("Reason", "���R��1024�����ȓ��œ��͂��ĉ�����");
            }

            // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
            //�[�ԔN���̒��ߏ����󋵂������߈ȏゾ�����ꍇ�G���[
            /*
            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode, header.SalesDate, "001"))
            {
                ModelState.AddModelError("SalesDate", "�������ߏ��������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
            }
            */

            //Mod 2016/08/13 arc yano #3596
            //����R�[�h����g�p�q�ɂ����o��
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

            int ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);

            switch (ret)
            {
                //���i�I���ςɂ��ҏW�s��
                case 1:
                    ModelState.AddModelError("SalesDate", "���i�I�����������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                    break;

                //�������ςɂ��ҏW�s��
                case 2:
                    ModelState.AddModelError("SalesDate", "���������������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                    break;

                default:
                    //�������Ȃ�
                    break;
            }

            //Add 2018/02/23 arc yano #3849
            //-------------------------------
            //���P�[�V�����̃`�F�b�N
            //-------------------------------
            DepartmentWarehouse dWarehous = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
            string warehouseCode = "";
            if (dWarehous != null)
            {
                warehouseCode = dWarehous.WarehouseCode;
            }

            //�݌ɊǗ��Ώۂ̕��i�̖��׍s�����擾
            var partsLinesEntity = (line.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997")))).Select(x => new { PartsNumber = x.PartsNumber, Quantity = (x.Quantity ?? 0), ProvisionQuantity = (x.ProvisionQuantity ?? 0) });

            List<PartsStock> stockList = new List<PartsStock>();

            foreach (var l in partsLinesEntity)
            {
                //���ʂ�ύX�����ꍇ(���X�̈����ϐ��Ɠ��͒l�̐��ʂ��قȂ��Ă���ꍇ)
                if (l.Quantity > l.ProvisionQuantity) //�݌ɂ��������Ƃ��ꍇ
                {
                    //�ړ����͍݌ɂ̑������P�[�V�������珇�ԂɎg��
                    stockList = new PartsStockDao(db).GetListByWarehouse(warehouseCode, l.PartsNumber, true).OrderBy(x => x.DelFlag).OrderByDescending(x => x.Quantity).ToList();

                    if (stockList.Count == 0)
                    {
                        ModelState.AddModelError("", "���i" + l.PartsNumber + "�̍݌ɏ�񂪖������߁A���ʂ�ύX�ł��܂���");
                    }
                }
                else if (l.Quantity < l.ProvisionQuantity) //�݌ɂ�߂��ꍇ
                {
                    Location location = stockService.GetDefaultLocation(l.PartsNumber, warehouseCode);
                    if (location == null)
                    {
                        ModelState.AddModelError("", "���i" + l.PartsNumber + "�̃��P�[�V��������`����Ă���܂���B���P�[�V�������Ē�`��ɁA�`�[�C���������ēx���{���肢�������܂��B");
                    }
                }
            }

        }
        #endregion

        #region ����������Validation�`�F�b�N
        /// <summary>
        /// ����������Validation�`�F�b�N
        /// </summary>
        /// <param name="line">�T�[�r�X�`�[�f�[�^(����)</param>
        /// <param name="index">�v�f��</param>
        /// <param name="displayFlag">�\���t���O</param>
        /// <history>
        /// 2017/01/31 arc yano #3566 �T�[�r�X�`�[���́@����ύX���̍݌ɂ̍Ĉ��� ���傪�ύX���ꂽ�ꍇ�́A�����ϐ��͂O�Ƃ���validation�`�F�b�N���s��
        /// 2016/01/26 arc yano #3453 �T�[�r�X�`�[�@�݌ɊǗ��ΏۊO�̕��i��validation���s��Ȃ�
        /// 2016/01/26 arc yano #3435 �T�[�r�X�`�[�@�����O�̕��i�̑Ή� ���L�Ή����ɔ��o�����s��̑Ή�
        /// 2016/01/26 arc yano #3406_�T�[�r�X�`�[�@��������validaion�G���[���b�Z�[�W�o�̗͂}�� (#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�) �����`�F�b�N���̈����ǉ�
        /// </history>
        private void ValidateHikiate(ServiceSalesLine line, int index, ref bool displayFlag, decimal? provisionQuantity)
        {
            string strName = string.Format("line[{0}].StockStatus", index);

            //decimal? requireQuantity = (line.Quantity ?? 0) - (line.ProvisionQuantity ?? 0);                    //�K�v����  //Add 2016/01/26 arc yano
            decimal? requireQuantity = (line.Quantity ?? 0) - (provisionQuantity ?? 0);                           //�K�v����  //Mod 2017/01/31 arc yano #3566 

            bool ret = new PartsDao(db).IsInventoryParts(line.PartsNumber);                                     //Mod 2016/01/26 arc yano #3453

            //���f���u�݌Ɂv�̏ꍇ�ŁA�݌ɐ� < ���ʂ̏ꍇ�A�G���[���b�Z�[�W��\��
            if (line.StockStatus.Equals("999") && (requireQuantity > line.PartsStock) && (ret))        
            {
                if (displayFlag == false)
                {
                    ModelState.AddModelError(strName, "�݌ɂ�����܂���B���f�͔�����I�����Ă�������");       //Mod 2016/01/26 arc yano
                    displayFlag = true;
                }
                else
                {
                    ModelState.AddModelError(strName, "");       //Mod 2016/01/26 arc yano
                }
            }
        }
        #endregion

        #region�@�����p�������������X�V����Ƃ�(�[�ԍς݂̎�)��validation�`�F�b�N
        //Add 2015/08/05 arc nakayama #3221_�����ƂȂ��Ă��镔���ԗ������ݒ肳��Ă���[�Ԋm�F��������o���Ȃ�
        /// <summary>
        /// �����p�������������X�V����Ƃ�(�[�ԍς݂̎�)��validation�`�F�b�N
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/05/24 arc yano #3896 �T�[�r�X�`�[���́@�[�Ԍ�̔[�Ԋm�F��������ł��Ȃ�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        private void ValidateForMemoUpdate(ServiceSalesHeader header)
        {
            int ret = 0;

            CommonValidate("SalesDate", "�[�ԓ�", header, true);   //Add 2018/05/24 arc yano #3896

            //�@�ҏW�O�ƍ�������������`�F�b�N
            if (header.SalesDate != new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber).SalesDate)
            {
                //Mod 2016/08/13 arc yano #3596
                //����R�[�h����g�p�q�ɂ����o��
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
  
                ret = CheckTempClosedEdit(dWarehouse, header.SalesDate);
                
                //ret = CheckTempClosedEdit(header.DepartmentCode, header.SalesDate);
            }
            switch (ret)
            {
                //���i�I���ςɂ��ҏW�s��
                case 1:
                    ModelState.AddModelError("SalesDate", "���i�I�����������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                    break;

                //�������ςɂ��ҏW�s��
                case 2:
                    ModelState.AddModelError("SalesDate", "���������������s����Ă��邽�߁A�w�肳�ꂽ�[�ԓ��ł͔[�Ԃł��܂���");
                    break;

                default:
                    //�������Ȃ�
                    break;
            }

            //�����p�������̕�����
            if (header.Memo != null && header.Memo.Length > 400)
            {
                ModelState.AddModelError("Memo", "�����p��������400�����ȓ��œ��͂��ĉ������i���s��2�������Ƃ݂Ȃ���܂��j");
            }
        }
        #endregion
        #region ��ʕ\���p����
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <history>
        /// 
        /// 2020/02/17 yano #4025�y�T�[�r�X�`�[�z��ږ��Ɏd��ł���悤�ɋ@�\�ǉ�
        /// 2018/02/23 arc yano #3471 �T�[�r�X�`�[�@�敪�̍i���݂̑Ή� �敪�͎�ʂɂ��i�荞�݂��s��
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �}�X�^���o�^�ɂ�蔭���ł��Ȃ����i��\������
        /// 2017/02/03 arc yano #3426 �T�[�r�X�`�[�E�`�[�C�����@������ʕ\���̕s�
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// 2016/05/09 arc yano #3480 �T�[�r�X�`�[�̐���������Ƃ̓��e�ɂ��؂蕪���� �����敪�ނ�ݒ肷�鏈���̒ǉ�
        /// 2016/03/17 arc yano #3471 �T�[�r�X�`�[�@�敪�̍i���݂̑Ή� �敪�͎�ʂɂ��i�荞�݂��s�� ��2016/04/19 �s������̂��߁A�폜
        /// 2016/01/28 arc yano #3401 �T�[�r�X�`�[�@��֕i�ŕ��[���̖��׍s�̃\�[�g�����̒ǉ��@
        /// 2015/10/28 arc yano #3289 ���i�d���@�\���P(�T�[�r�X�`�[����)
        /// </history>
        /// <param name="salesCar">���f���f�[�^</param>
        private void SetDataComponent(ref ServiceSalesHeader header) {
            ViewData["displayContents"] = "main";
            // Mod 2015/05/18 arc nakayama ���b�N�A�b�v�������Ή��@�����f�[�^���\���͂�����(FrontEmployee/ReceiptionEmployee/Department/Customer)
            header.FrontEmployee = new EmployeeDao(db).GetByKey(header.FrontEmployeeCode, true);
            header.ReceiptionEmployee = new EmployeeDao(db).GetByKey(header.ReceiptionEmployeeCode, true);
            header.Department = new DepartmentDao(db).GetByKey(header.DepartmentCode, true);
            header.Customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);
            header.CustomerClaim = new CustomerClaimDao(db).GetByKey(header.CustomerClaimCode, true);
            header.c_ServiceOrderStatus = new CodeDao(db).GetServiceOrderStatusByKey(header.ServiceOrderStatus);

            //Add 2016/01/28 arc yano #3401 ���׍s����ʕ\�����ɕ��ёւ�
            EntitySet<ServiceSalesLine> wkEntity = new EntitySet<ServiceSalesLine>();
            wkEntity.AddRange(header.ServiceSalesLine.OrderBy(x => x.DisplayOrder).ThenBy(x => x.LineNumber));
            header.ServiceSalesLine = wkEntity;


            CodeDao dao = new CodeDao(db);

            PartsStockDao pDao = new PartsStockDao(db);

            //ADD 2014/02/21 ookubo
            if (header.ConsumptionTaxId == null || header.ConsumptionTaxId == "")
            {
                if (header.SalesDate != null)
                {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(header.SalesDate);
                }
                else
                {
                    if (header.SalesPlanDate != null)
                    {
                        header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(header.SalesPlanDate);
                    }
                    else
                    {
                        header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    }
                }
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), header.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = header.ConsumptionTaxId;
            ViewData["Rate"] = header.Rate;
            ViewData["ConsumptionTaxIdOld"] = header.ConsumptionTaxId;
            ViewData["SalesDateOld"] = header.SalesDate;
            ViewData["SalesPlanDateOld"] = header.SalesPlanDate;
            //ADD end
            
            ViewData["KeepsCarFlag"] = header.KeepsCarFlag;   //ADD 2014/11/25 #3135 ookubo

            //Add 2020/02/17 yano #4025
            if ((header.TaxableCostTotalAmount ?? 0m) != 0m)
            {
                //����p�i�ېŁj�̏���ŎZ�o
                header.TaxableCostSubTotalTaxAmount = Math.Truncate((header.TaxableCostTotalAmount ?? 0m) * (decimal)(header.Rate / (100 + header.Rate)));
                //����p�i�ېŁj�̐Ŕ�
                header.TaxableCostSubTotalAmount = (header.TaxableCostTotalAmount ?? 0m) - header.TaxableCostSubTotalTaxAmount;
            }
            else
            {
                //����p�i�ېŁj�̏���ŎZ�o
                header.TaxableCostSubTotalTaxAmount = 0m;
                //����p�i�ېŁj�̐Ŕ�
                header.TaxableCostSubTotalAmount = 0m;
            }
            
            List<IEnumerable<SelectListItem>> serviceTypeList = new List<IEnumerable<SelectListItem>>();
            List<c_ServiceType> serviceTypeListSrc = dao.GetServiceTypeAll(true);
            List<IEnumerable<SelectListItem>> workTypeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> stockStatusList = new List<IEnumerable<SelectListItem>>();
            //Mod 2015/10/28 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����敪�ɂ��A�݌ɔ��f���X�g�̒��g��ς���B���̒i�K�͏������̂ݍs��
            List<c_StockStatus> stockStatusListSrc = new List<c_StockStatus>();
            //List<c_StockStatus> stockStatusListSrc = dao.GetStockStatusAll(false);
            List<IEnumerable<SelectListItem>> lineTypeList = new List<IEnumerable<SelectListItem>>();
            List<CodeData> lineTypeListSrc = CodeUtils.GetLineTypeList();
            foreach (var line in header.ServiceSalesLine) {
                List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false);

                //Mod 2018/02/23 arc yano #3741
                //��ʂɂ��i��
                //��� = �T�[�r�X���j���[
                if (!string.IsNullOrWhiteSpace(line.ServiceType) && line.ServiceType.Equals("002"))
                {
                    workTypeListSrc = workTypeListSrc.Where(x => !string.IsNullOrWhiteSpace(x.ServiceMenuUse) && x.ServiceMenuUse.Equals("1")).ToList();

                }
                else if (!string.IsNullOrWhiteSpace(line.ServiceType) && line.ServiceType.Equals("003"))
                {
                    workTypeListSrc = workTypeListSrc.Where(x => !string.IsNullOrWhiteSpace(x.PartsUse) && x.PartsUse.Equals("1")).ToList();
                }

                //List<c_WorkType> workTypeListSrc = dao.GetWorkTypeAll(false, line.ServiceType);   //Mod 2016/03/17 arc yano   #3471 �@��2016/04/19 �s��̂��߁A�폜
                serviceTypeList.Add(CodeUtils.GetSelectListByModel(serviceTypeListSrc, line.ServiceType, false));
                line.CustomerClaim = new CustomerClaimDao(db).GetByKey(line.CustomerClaimCode);
                // Mod 2015/05/21 arc nakayama #3186_�T�[�r�X�`�[���ׂ̃��J�j�b�N�����\������Ȃ��@�\������Ƃ���DelFlag���l�����Ȃ�
                line.Employee = new EmployeeDao(db).GetByKey(line.EmployeeCode, true);
                workTypeList.Add(CodeUtils.GetSelectListByModel(workTypeListSrc, line.WorkType, true));
                //Add 2015/10/28 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����敪�ɂ��A�݌ɔ��f���X�g�̒��g��ς���

                if (!string.IsNullOrWhiteSpace(line.PartsNumber))
                {
                    Parts parts = new PartsDao(db).GetByKey(line.PartsNumber);

                    if (parts != null)
                    {
                        //Mod 2016/02/05 arc nakayama #3427_�T�[�r�X�`�[���͉�ʁ@���i�ԍ������͎��̔��f�\�� �����l�͎ЊO�i
                        //���i�}�X�^�ɏ����敪���ݒ肳��Ă��Ȃ��ꍇ�́A�f�t�H���g�u�ЊO�i�v�Őݒ肵�Ă���
                        stockStatusListSrc = dao.GetStockStatusAll(false);         //Mod 2017/10/19 arc yano #3803

                        //Add 2017/02/03 arc yano #3426
                        if (service.CheckModification(header.SlipNumber, header.RevisionNumber))   //�[�ԍς̏ꍇ
                        {
                            stockStatusListSrc = stockStatusListSrc.Where(x => !x.StatusType.Equals("001")).ToList();
                        }

                    }

                    //Mod 2016/08/13 arc yano #3596
                    //����R�[�h����g�p�q�ɂ����o��
                    DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);
                    line.PartsStock = pDao.GetStockQuantity(line.PartsNumber, (dWarehouse != null ? dWarehouse.WarehouseCode : ""));   //Add 2015/10/28 arc yano #3289

                }
                else //Add 2015/10/28 2015/10/28 arc yano #3289
                {
                    //Mod 2016/02/05 arc nakayama #3427_�T�[�r�X�`�[���͉�ʁ@���i�ԍ������͎��̔��f�\�� �����l�͎ЊO�i
                    stockStatusListSrc = dao.GetStockStatusAll(false); //Mod 2017/10/19 arc yano #3803

                    //Add 2017/02/03 arc yano #3426
                    if (service.CheckModification(header.SlipNumber, header.RevisionNumber))   //�[�ԍς̏ꍇ
                    {
                        stockStatusListSrc = stockStatusListSrc.Where(x => !x.StatusType.Equals("001")).ToList();
                    }
                }
                stockStatusList.Add(CodeUtils.GetSelectListByModel(stockStatusListSrc, line.StockStatus, false));
                line.ServiceTypeName = dao.GetServiceTypeByKey(line.ServiceType).Name;
                line.Supplier = new SupplierDao(db).GetByKey(line.SupplierCode);
                lineTypeList.Add(CodeUtils.GetSelectListByModel(lineTypeListSrc, line.LineType, false));

                //Add 2016/05/09 arc yano #3480
                //������敪(����)�̐ݒ�
                if (line.ServiceWork != null)
                {
                    line.SWCustomerClaimClass = line.ServiceWork.CustomerClaimClass;
                }
                //������敪(������)�̐ݒ�
                if (line.CustomerClaim != null && line.CustomerClaim.c_CustomerClaimType != null)
                {
                    line.CCCustomerClaimClass = line.CustomerClaim.c_CustomerClaimType.CustomerClaimClass;
                }

                //Add 2017/10/19 arc yano #3803
                
                //�`�[�X�e�[�^�X=�󒍁`�[�Ԋm�F������ς�
                if (!string.IsNullOrWhiteSpace(header.ServiceOrderStatus) && (header.ServiceOrderStatus.Equals("002") || header.ServiceOrderStatus.Equals("003") || header.ServiceOrderStatus.Equals("004") || header.ServiceOrderStatus.Equals("005")))
                {
                    //���f��S/O�Ȃǂ̔������I�΂�Ă���
                    if (line.c_StockStatus != null && (line.c_StockStatus.StatusType.Equals("001") || line.c_StockStatus.StatusType.Equals("010")))
                    {
                        //���i�ԍ������i�}�X�^�ɓo�^����Ă��Ȃ��ꍇ
                        if (line.Parts == null && !string.IsNullOrWhiteSpace(line.DelFlag) && !line.DelFlag.Equals("1"))
                        {
                            ViewData["UnregisteredPartsList"] += ("����" + line.LineNumber + "�s�ڂ̕��i�y���i�ԍ�=" + line.PartsNumber + "�z�̓}�X�^���o�^�̂��ߔ�����ʂɂ͕\������܂���<br/>");   //Add 2017/02/03 arc yano #3426
                        }
                    }
                }

            }
            for (int i = 0; i < header.ServiceSalesPayment.Count; i++) {
                header.ServiceSalesPayment[i].CustomerClaim = new CustomerClaimDao(db).GetByKey(header.ServiceSalesPayment[i].CustomerClaimCode);
            }
            ViewData["ServiceTypeLineList"] = serviceTypeList;
            ViewData["MileageUnit"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), header.MileageUnit, false);
            ViewData["ServiceTypeList"] = CodeUtils.GetSelectListByModel<c_ServiceType>(dao.GetServiceTypeAll(true), header.ServiceType, false);
            ViewData["WorkTypeList"] = workTypeList;
            ViewData["StockStatus"] = stockStatusList;
            ViewData["LineTypeList"] = lineTypeList;

            ViewData["JournalTotalAmount"] = 0m;
            if (!string.IsNullOrEmpty(header.SlipNumber)) {
                // Mod 2015/11/02 arc nakayama #3297_�T�[�r�X�`�[���͉�ʂ̓������ѐݒ�l�̕s�
                List<Journal> journalList = new JournalDao(db).GetJournalCalcListBySlipNumber(header.SlipNumber);
                ViewData["JournalTotalAmount"] = journalList != null ? journalList.Sum(x => x.Amount) : 0m;
            }
            header.BasicHasErrors = BasicHasErrors();
            header.InvoiceHasErrors = InvoiceHasErrors();
            header.TaxHasErrors = TaxHasErrors();

            //Mod 2020/02/17 yano #4025
            // ����p�̍��v��a������Ƃ��č쐬
            decimal depositTotal = (header.CarTax ?? 0m) + (header.CarWeightTax ?? 0m) + (header.CarLiabilityInsurance ?? 0m) +
                               (header.NumberPlateCost ?? 0m) + (header.FiscalStampCost ?? 0m) + (header.TaxFreeFieldValue ?? 0m) +
                               (header.OptionalInsurance ?? 0m) + (header.SubscriptionFee ?? 0m) + (header.TaxableFreeFieldValue ?? 0m);
      //decimal depositTotal = (header.CarTax ?? 0m) + (header.CarWeightTax ?? 0m) + (header.CarLiabilityInsurance ?? 0m) +
      //                    (header.NumberPlateCost ?? 0m) + (header.FiscalStampCost ?? 0m) + (header.TaxFreeFieldValue ?? 0m);


      //���ׂ𐿋��悲�Ƃɍ��v���z���W�v
      var query = from a in header.ServiceSalesLine
                    //  Edit 2014/06/12 arc yano �������Ή� CliDelFlag��1�̍s�͏W�v���Ȃ�
                    //where (a.ServiceType.Equals("002") || a.ServiceType.Equals("003"))
                  where ((a.ServiceType.Equals("002") || a.ServiceType.Equals("003")) && (string.IsNullOrEmpty(a.CliDelFlag) || (!(string.IsNullOrEmpty(a.CliDelFlag)) && !(a.CliDelFlag.Equals("1")))))
                  group a by new { CustomerClaimCode = a.CustomerClaimCode, ServiceWorkCode = a.ServiceWorkCode } into customerClaim
                  select new
                  {
                    customerClaim.Key,
                    TechnicalFeeAmount = customerClaim.Sum(y => service.IsDiscountRecord(y.ServiceMenuCode) ? (-1) * y.TechnicalFeeAmount : (y.TechnicalFeeAmount ?? 0m)),
                    PartsAmount = customerClaim.Sum(x => service.IsDiscountRecord(x.PartsNumber) ? (-1) * x.Amount : (x.Amount ?? 0m)),
                    TaxAmount = customerClaim.Sum(x => (service.IsDiscountRecord(x.PartsNumber) || service.IsDiscountRecord(x.ServiceMenuCode)) ? (-1) * x.TaxAmount : (x.TaxAmount ?? 0m)),
                    TechnicalCost = customerClaim.Sum(x => x.ServiceType.Equals("002") ? x.Cost : 0m),
                    PartsCost = customerClaim.Sum(x => x.ServiceType.Equals("003") ? x.Cost : 0m)
                  };

      header.ServiceClaimable = new List<ServiceClaimable>();
            // �ڋq�E�����悪�Z�b�g����Ă���ꍇ�̂�
            if (!string.IsNullOrEmpty(header.CustomerCode) && header.ServiceSalesLine.Where(x => x.CustomerClaimCode != null).Count() > 0) {
                bool ExistCustomerCode = false;
                //Add 2023/05/01 openwave #xxxx
                string CustomerClaimCode = header.CustomerCode;
                if (!string.IsNullOrEmpty(header.CustomerClaimCode)) {
                    CustomerClaimCode = header.CustomerClaimCode;
                }
                foreach (var item in query) {
                    if (item.Key.CustomerClaimCode != null) {
                        ServiceClaimable serviceClaimable = new ServiceClaimable();
                        serviceClaimable.CustomerClaimCode = item.Key.CustomerClaimCode;
                        serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(item.Key.CustomerClaimCode);
                        serviceClaimable.ServiceWorkCode = item.Key.ServiceWorkCode;
                        serviceClaimable.ServiceWork = new ServiceWorkDao(db).GetByKey(item.Key.ServiceWorkCode);
                        serviceClaimable.TechnicalFeeAmount = item.TechnicalFeeAmount ?? 0m;
                        serviceClaimable.PartsAmount = item.PartsAmount ?? 0m;
                        serviceClaimable.Amount = (item.TechnicalFeeAmount ?? 0m) + (item.PartsAmount ?? 0m);
                        serviceClaimable.AmountWithTax = serviceClaimable.Amount + (item.TaxAmount ?? 0m);
                        serviceClaimable.TechnicalCost = item.TechnicalCost ?? 0m;
                        serviceClaimable.PartsCost = item.PartsCost ?? 0m;
                        serviceClaimable.Cost = (item.TechnicalCost ?? 0m) + (item.PartsCost ?? 0m);
                        serviceClaimable.TechnicalMargin = serviceClaimable.TechnicalFeeAmount - serviceClaimable.TechnicalCost;
                        serviceClaimable.PartsMargin = serviceClaimable.PartsAmount - serviceClaimable.PartsCost;
                        serviceClaimable.Margin = serviceClaimable.Amount - serviceClaimable.Cost;
                        try {
                            serviceClaimable.MarginRate = Math.Round(Decimal.Divide(serviceClaimable.Margin, serviceClaimable.Amount) * 100, 1);
                        } catch {
                            serviceClaimable.MarginRate = 0;
                        }
                        //Mod 2023/05/01 openwave #xxxx
                        //if (item.Key.CustomerClaimCode.Equals(header.CustomerCode)) {
                        if (item.Key.CustomerClaimCode.Equals(CustomerClaimCode)) {
                            serviceClaimable.DepositAmount = depositTotal;
                            ExistCustomerCode = true;
                        }

                        header.ServiceClaimable.Add(serviceClaimable);
                    }
                }

                if (!ExistCustomerCode && depositTotal > 0) {
                    ServiceClaimable serviceClaimable = new ServiceClaimable();
                    //Mod 2023/05/01 openwave #xxxx
                    //serviceClaimable.CustomerClaimCode = header.CustomerCode;
                    //serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(header.CustomerCode);
                    serviceClaimable.CustomerClaimCode = CustomerClaimCode;
                    serviceClaimable.CustomerClaim = new CustomerClaimDao(db).GetByKey(CustomerClaimCode);
                    serviceClaimable.ServiceWorkCode = "";
                    serviceClaimable.ServiceWork = null;
                    serviceClaimable.TechnicalFeeAmount = 0;
                    serviceClaimable.PartsAmount = 0;
                    serviceClaimable.Amount = 0;
                    serviceClaimable.AmountWithTax = 0;
                    serviceClaimable.TechnicalCost = 0;
                    serviceClaimable.PartsCost = 0;
                    serviceClaimable.Cost = 0;
                    serviceClaimable.TechnicalMargin = 0;
                    serviceClaimable.PartsMargin = 0;
                    serviceClaimable.Margin = 0;
                    serviceClaimable.MarginRate = 0;
                    serviceClaimable.DepositAmount = depositTotal;
                    header.ServiceClaimable.Add(serviceClaimable);
                }
            }

            //Add 2014/10/29 arc amii �Z���Ċm�F���b�Z�[�W�Ή� #3119 �ڋq��񗓂ɕ\������Z���Ċm�F���b�Z�[�W��ݒ肷��
            ViewData["ReconfirmMessage"] = "";

            if (header.Customer != null && header.Customer.AddressReconfirm == true)
            {
                // �Z���Ċm�F�t���O�������Ă����ꍇ�A���b�Z�[�W��ݒ肷��
                ViewData["ReconfirmMessage"] = "�Z�����Ċm�F���Ă�������";
            }

            //Add 2017/10/19 arc yano #3803
            //validation�G���[�̏ꍇ�́A�������A�o�ɓ`�[�͏o�͂��Ȃ�
            if (!ModelState.IsValid)
            {
                header.Output = false;
            }
        }
        #endregion
        
        #region �^�u���Ƃ̃G���[�d��
        /// <summary>
        /// �^�u���Ƃ̃G���[�d��
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025�y�T�[�r�X�`�[�z��ږ��Ɏd��ł���悤�ɋ@�\�ǉ�
        /// </history>
        private bool BasicHasErrors() {
            var query = from a in ModelState
                        where (!a.Key.StartsWith("pay[")
                        && !a.Key.Equals("")
                        && !a.Key.Equals("PaymentTotalAmount")
                        && !a.Key.Equals("CarTax")
                        && !a.Key.Equals("CarLiabilityInsurance")
                        && !a.Key.Equals("CarWeightTax")
                        && !a.Key.Equals("NumberPlateCost")
                        && !a.Key.Equals("FiscalStampCost")
                        && !a.Key.Equals("TaxFreeFieldName")
                        && !a.Key.Equals("TaxFreeFieldValue")
                        && !a.Key.Equals("OptionalInsurance")           //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("SubscriptionFee")             //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("TaxableFreeFieldValue")       //Add 2020/02/17 yano #4025
                        && !a.Key.Equals("TaxableFreeFieldName"))       //Add 2020/02/17 yano #4025

                        && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }
        private bool InvoiceHasErrors() {
            var query = from a in ModelState
                        where (a.Key.StartsWith("pay[") || a.Key.Equals("PaymentTotalAmount"))
                        && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// �^�u���Ƃ̃G���[�d��
        /// </summary>
        /// <history>
        /// 2020/02/17 yano #4025�y�T�[�r�X�`�[�z��ږ��Ɏd��ł���悤�ɋ@�\�ǉ�
        /// </history>
        private bool TaxHasErrors() {
            if (ModelState["CarTax"] != null && ModelState["CarTax"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["CarLiabilityInsurance"] != null && ModelState["CarLiabilityInsurance"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["CarWeightTax"] != null && ModelState["CarWeightTax"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["NumberPlaceCost"] != null && ModelState["NumberPlateCost"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["FiscalStampCost"] != null && ModelState["FiscalStampCost"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["TaxFreeFieldName"] != null && ModelState["TaxFreeFieldName"].Errors.Count() > 0) {
                return true;
            }
            if (ModelState["TaxFreeFieldValue"] != null && ModelState["TaxFreeFieldValue"].Errors.Count() > 0) {
                return true;
            }
            //Add 2020/02/17 yano #4025
            if (ModelState["OptionalInsurance"] != null && ModelState["OptionalInsurance"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["SubscriptionFee"] != null && ModelState["SubscriptionFee"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["TaxableFreeFieldValue"] != null && ModelState["TaxableFreeFieldValue"].Errors.Count() > 0)
            {
                return true;
            }
            if (ModelState["TaxableFreeFieldName"] != null && ModelState["TaxableFreeFieldName"].Errors.Count() > 0)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region �ԍ�����
        /// <summary>
        /// �ԍ�������ʕ\��
        /// </summary>
        /// <returns></returns>
        [AkakuroAuthFilter]
        public ActionResult ModifyCriteria() {
            criteriaInit = true;
            return ModifyCriteria(new FormCollection());
        }
        /// <summary>
        /// �ԍ���������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ModifyCriteria(FormCollection form) {
            form["DelFlag"] = "0";
            form["ServiceOrderStatus"] = "006";
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["DepartmentName"] = "";
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["CustomerCode"] = form["CustomerCode"];
            ViewData["CustomerName"] = form["CustomerName"];
            ViewData["Vin"] = form["Vin"];
            ViewData["SlipNumber"] = form["SlipNumber"];
            PaginatedList<ServiceSalesHeader> list;

            //Mod 2015/05/07 arc yano �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
            int ret = 0;

            if (criteriaInit) {
                list = new PaginatedList<ServiceSalesHeader>();
            } else {
                list = GetSearchResultListModeAkaden(form);
            }
            foreach (var item in list) {
                /*
                // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                if (!new InventorySchedulePartsDao(db).IsClosedInventoryMonth(item.DepartmentCode, item.SalesDate, "002")){
                    // �[�ԓ����I�����ߖ�������������ԍ��ΏۊO
                    item.IsClosed = true;
                }
                */

                //Mod 2016/08/13 arc yano #3596
                //Mod 2015/05/07 arc yano �������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                //����R�[�h����g�p�q�ɂ����o��
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, item.DepartmentCode);
                ret = CheckTempClosedEdit(dWarehouse, item.SalesDate);
                //ret = CheckTempClosedEdit(item.DepartmentCode, item.SalesDate);
                
                if (ret != 0)
                {
                    item.IsClosed = true;
                }

                if (new ServiceSalesOrderDao(db).GetBySlipNumber(item.SlipNumber + "-1") != null) {
                    // �ԓ`�����A�ԍ��������Ă��Ȃ�������Ώ�
                    item.IsCreated = true;
                }
            }
            return View("ServiceSalesOrderModifyCriteria", list);
        }
        /// <summary>
        /// �ԍ���������
        /// </summary>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        private PaginatedList<ServiceSalesHeader> GetSearchResultListModeAkaden(FormCollection form) {
            form["WithoutAkaden"] = "0";
            return GetSearchResultList(form);
        }
        /// <summary>
        /// �ԓ`����
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akaden(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            //Add 2014/06/17 arc yano �������Ή�
            //�폜�Ώۍs�̃f�[�^�폜
            Delline(ref line);

            header.ServiceSalesLine = line;
            header.ServiceSalesPayment = pay;

            // �ԓ`�����͗��R���K�{
            ViewData["Mode"] = form["Mode"];
            ValidateForAkaden(header, line);
            if (ModelState.IsValid)
            {

                // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();
                stockService = new StockService(db);
                service = new ServiceSalesOrderService(db);

                
                service.ProcessUnLock(header);

                ServiceSalesHeader history2 = new ServiceSalesHeader();

                //Mod 2016/05/23 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@ �r����SubmitChanges�������邽�߃g�����U�N�V�����X�R�[�v�ǉ�
                //Add 2016/05/31 arc nakayama #3568_�y�T�[�r�X�`�[�z���ς���󒍂ɂ���ƃ^�C���A�E�g�ŗ�����
                double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
                {
                    // �ԓ`����
                    ServiceSalesHeader history = service.CreateAkaden(header);
                    CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "�T�[�r�X", header.Reason);

                    //Mod 2016/05/23 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@
                    //�������������ꍇ�A�������ѕ��̕ԋ��̓����\��쐬
                    service.CreateBackAmountAkaden(header.SlipNumber);

                    //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͂��s���ׁAtry catch����ǉ�
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        history2 = history;
                    }
                    catch (SqlException e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            //// ���O�ɏo��
                            OutputLogger.NLogError(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);

                            string message = "���[�ԍ�=" + header.SlipNumber + "�̃f�[�^�́A";
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0023", message));

                            //�X�e�[�^�X��߂�
                            header.ServiceOrderStatus = form["PreviousStatus"];
                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }

                    ViewData["CompleteMessage"] = "�`�[�ԍ��u" + header.SlipNumber + "�i���`�[�j�v�̐ԓ`�u" + history.SlipNumber + "�i�V�ԍ��j�v����������Ɋ������܂����B";
                    ViewData["Mode"] = null;
                    ModelState.Clear();
                }
                // �\�����ڂ̍ăZ�b�g
                SetDataComponent(ref history2);
                return GetViewResult(history2);
            }
            // �\�����ڂ̍ăZ�b�g
            //Add 2014/06/10 arc yano �������Ή� 
            Sortline(ref line);
            header.ServiceSalesLine = line;

            SetDataComponent(ref header);
            return GetViewResult(header);
        }

        /// <summary>
        /// �ԍ�����
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/03 arc yano #3732 �T�[�r�X�`�[ �ԍ��`�[�쐬�Łu���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂��� �v�\�� �����ǉ�
        /// 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �����݌ɂ̊Ǘ����@�̕ύX
        /// </history>
        [AkakuroAuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akakuro(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {

            // �ԍ������͗��R���K�{
            ViewData["Mode"] = form["Mode"];

            //Add 2014/06/17 arc yano line��lineNumber���Ƀ\�[�g����B
            ModelState.Clear();
            //���׍s��lineNumber���ɕ��ёւ�
            Sortline(ref line);

            //Add 2014/06/17 arc yano �������Ή�
            //Mod 2015/10/15 arc nakayama #3264_�T�[�r�X�`�[�̃R�s�[�ōė��p�����`�[�ŗL���ȃ��J�S���ҏC����u���͂���Ă��郁�J�S���҂̓}�X�^�ɑ��݂��Ă��܂��� �v�\�� �o���f�[�V�����`�F�b�N�̑O�Ɉړ�
            //�폜�Ώۍs�̍폜
            Delline(ref line);

            ValidateForAkaden(header, line);
            ValidateAllStatus(header, line, form);      //Mod 2017/11/03 arc yano #3732

            if (!ModelState.IsValid) {
                
                service.ProcessUnLock(header);

                //�l����ō��ɕϊ�
                service.SetDiscountAmountWithTax(line);

                header.ServiceSalesLine = line;
                header.ServiceSalesPayment = pay;

                // �\�����ڂ̍ăZ�b�g
                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            

            // Add 2014/08/08 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            stockService = new StockService(db);
            service = new ServiceSalesOrderService(db);


            //Mod 2016/05/23 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@ �r����SubmitChanges�������邽�߃g�����U�N�V�����X�R�[�v�ǉ�
            //Add 2016/05/31 arc nakayama #3568_�y�T�[�r�X�`�[�z���ς���󒍂ɂ���ƃ^�C���A�E�g�ŗ�����
            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {

                // �ԍ�����
                CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "�T�[�r�X", header.Reason);

                ServiceSalesHeader original = new ServiceSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);

                // �ԓ`�[�쐬
                ServiceSalesHeader history = service.CreateAkaden(original);

                // ���`�[�쐬
                header = service.CreateKuroden(header, line);       //2015/10/28 arc yano #3289 �߂�l���󂯎��
                //db.ServiceSalesHeader.InsertOnSubmit(header);

                //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�͂��s���ׁAtry catch����ǉ�
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //// ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);

                        string message = "���[�ԍ�=" + header.SlipNumber + "�̃f�[�^�́A";
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0023", message));

                        //�X�e�[�^�X��߂�
                        header.ServiceOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                    return View("Error");
                }

                ViewData["CompleteMessage"] = "�`�[�ԍ��u" + original.SlipNumber + "�i���`�[�j�v�̐ԍ��u" + header.SlipNumber + "�i�[�ԍςݓ`�[�̃R�s�[�j�v����������Ɋ������܂����B";
                ViewData["Mode"] = null;
                ModelState.Clear();
            }

            service.ProcessUnLock(header);

            //Add 2014/06/17 arc yano �������Ή� 
            header.ServiceSalesLine.Insert(0, new ServiceSalesLine { ServiceType = "001", CliDelFlag = "1" });

            //�l����ō��ɕϊ�
            service.SetDiscountAmountWithTax(line);

            // �\�����ڂ̍ăZ�b�g
            SetDataComponent(ref header);

            return GetViewResult(header);

        }

        #endregion

        #region �`�[���b�N����
        /// <summary>
        /// ��ʂ����ۂɌĂяo�����
        /// �i�`�[���b�N����������j
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        public ActionResult UnLock(ServiceSalesHeader header, EntitySet<ServiceSalesLine> line, EntitySet<ServiceSalesPayment> pay, FormCollection form) {
            //header.ServiceSalesLine = line;
            //header.ServiceSalesPayment = pay;
            if (header.RevisionNumber > 0) {
                service.ProcessUnLock(header);
            }
            ViewData["close"] = "1";

            //Add 2014/12/24 arc yano #3143 ���v�l�̍Čv�Z(SetDataComponent���Ŏ��s)�̑O�ɒl���z����U�A�Ŕ��\���ɖ߂��A�Čv�Z��ɁA�ēx�ō��\���ɕϊ�����B
            //�l���z��ō��\������A�Ŕ��\���ɕϊ�
            service.SetDiscountAmountWithoutTax(line);

            SetDataComponent(ref header);

            //�l���z��ō��\���ɕϊ�
            service.SetDiscountAmountWithTax(header.ServiceSalesLine);

            return GetViewResult(header);
        }
        

        #endregion

        //Add 2014/06/17 ardc yano �������Ή�
        #region �s�v�s�̍폜
        /// <summary>
        /// �폜�Ώۍs(�N���C�A���g�ō폜�����s)�̍폜���s���B
        /// </summary>
        /// <param name="line">���f���f�[�^</param>
        private void Delline(ref EntitySet<ServiceSalesLine> line)
        {
            //----------------------
            //�s�v�ȍs�̍폜
            //----------------------
            int i;
            for (i = line.Count - 1; i >= 0; i--)
            {
                //�폜�Ώۍs�̏ꍇ
                if (!string.IsNullOrEmpty(line[i].CliDelFlag) && (line[i].CliDelFlag.Equals("1")))
                {
                    line.RemoveAt(i);
                }
            }
        }
        #endregion

        #region ���׍s�\�[�g
        /// <summary>
        /// ���׍s�̃f�[�^�Z�b�g��lineNumber���ɕ��ёւ���B
        /// </summary>
        /// <param name="line">���f���f�[�^</param>
        /// <param name="resline">���X�g�A�p�f�[�^</param>
        /// <history>
        /// Mod 2016/01/28 arc yano #3401_�T�[�r�X�`�[�@��֕i�ŕ��[���̖��׍s�̃\�[�g�����̒ǉ� �\�[�g�ԍ��̐ݒ菈���̒ǉ�
        /// Add 2014/06/17 arc yano �������Ή� 
        /// </history>
        private void Sortline(ref EntitySet<ServiceSalesLine> line)
        {
            int cnt = 1;    //Add 2016/01/28 arc yano #340   
            
            //���׍s�f�[�^��LineNumber�Ń\�[�g����B
            ServiceSalesLine[] restoreline = (from a in line
                                orderby a.LineNumber 
                                select a).ToArray();

            line.Clear();   //���׍s�f�[�^�N���A

            foreach (ServiceSalesLine a in restoreline)
            {
                a.DisplayOrder = cnt++; //Add 2016/01/28 arc yano #340    
                line.Add(a);
            }
        }
        #endregion

        #region �����t���O�̕ϊ�
        /// <summary>
        /// ���׍s�̃f�[�^�Z�b�g��lineNumber���ɕ��ёւ���B
        /// </summary>
        /// <param name="line">���f���f�[�^</param>
        /// <param name="resline">���X�g�A�p�f�[�^</param>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������̏o�� �V�K�쐬 
        /// </history>
        private void SetOutputTragetFlag (ref EntitySet<ServiceSalesLine> line)
        {
            foreach (var l in line)
            {
                //�`�F�b�N�������Ă���ꍇ
                if (l.OutputTarget.ToLower().Contains("true"))
                {
                    l.OutputTargetFlag = "1";
                }
                else
                {
                    l.OutputTargetFlag = "0";
                }
            }
        }
        #endregion

        //Del 2015/07/31 arc yano #3231 �T�[�r�X�`�[��FA�������e�B�`�F�b�N���O��
        //Add 2014/07/03 ardc yano �T�[�r�X�`�[�`�F�b�N�V�V�X�e��
        #region �������e�B�`�F�b�N
        /// <summary>
        /// ���׍s�̃������e�B�`�F�b�N���s���B
        /// </summary>
        /// <param name="totalTechFee">�Z�p�����v</param>
        /// <param name="totalAmount">���z���v</param>
        /// <param name="totalSmCost">����(���v)/�T�[�r�X���j���[</param>
        /// <param name="totalPtCost">����(���v)/���i</param>
        /// <param name="posServiceWork">�G���[�Ώۃ��R�[�h</param>
        /// </summary>
        private void ValidateWaranty(ref decimal totalTechFee, ref decimal totalAmount, ref decimal totalSmCost, ref decimal totalPtCost, int posServiceWork)
        {
            /*
            int adderr = 0;     //�G���[���b�Z�[�W�ǉ��t���O

            //------------------------------------------
            //�T�[�r�X���j���[�ł̃`�F�b�N
            //------------------------------------------
            if (totalTechFee != totalSmCost)       //�Z�p��������(���v)/�T�[�r�X���j���[
            {
                adderr = 1;
            }
            //------------------------------------------
            //���i�ł̃`�F�b�N
            //------------------------------------------
            if (totalAmount != totalPtCost)       //���i������(���v)/���i
            {
                adderr = 1;
            }


            if (adderr == 1)
            {
                if (!ModelState.ContainsKey(string.Format("line[{0}].{1}", posServiceWork, "ServiceWorkCode")))   //���ƃR�[�h�ɑ΂��Č��؃G���[�������̏ꍇ�B
                {
                    ModelState.AddModelError(string.Format("line[{0}].{1}", posServiceWork, "ServiceWorkCode"), "");
                }

                ModelState.AddModelError("", MessageUtils.GetMessage("E0022", new string[] { "���Ƃ��������e�B�܂��̓��R�[��", "�Z�p���̍��v�ƌ���(���v)�̍��v�A�܂��͋��z�̍��v�ƌ���(���v)�̍��v����v������K�v������܂�" }));
            }
                
            //���Z�b�g
            totalTechFee = 0;
            totalSmCost = 0;
            totalAmount = 0;
            totalPtCost = 0;
            */
        }
        #endregion

        //Add 2015/10/28 arc yano #3289 �T�[�r�X�`�[ �������̍쐬���@�̕ύX
        #region �������url�쐬
        /// <summary>
        /// ���׍s�̃������e�B�`�F�b�N���s���B
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�w�b�_���</param>
        /// <param name="orderList">�������</param>
        /// </summary>
        private ServiceSalesHeader makeOrderUrl(ServiceSalesHeader header, List<PartsPurchaseOrder> orderList)
        { 
            header.orderUrlList = new List<string>();

            string strUrl = "";

            //--------------------------------
            //�����i��URL�쐬
            //--------------------------------
            //�I�[�_�[�敪���X�g�̎擾
            var oTypeList = orderList.GroupBy(x => x.OrderType).Select(x => new { key = x.Key });

            int cnt = 0;

            foreach (var oType in oTypeList)
            {
                cnt = 0;

                strUrl = "/PartsPurchaseOrder/Entry";       //���N�G�X�g��̐ݒ�

                //�I�[�_�[�敪�Ŕ��������i����
                var oList = orderList.Where(x => (x.OrderType.Equals(oType.key) && (x.GenuineType ?? "").Equals(TYPE_GENUINE))).GroupBy(x => new { x.PartsNumber, x.ServiceSlipNumber, x.DepartmentCode, x.OrderType }).Select
                    (x => new
                    {
                          PartsNumber = x.Key.PartsNumber
                        , ServiceSlipNumber = x.Key.ServiceSlipNumber
                        , DepartmentCode = x.Key.DepartmentCode
                        , OrderType = x.Key.OrderType
                        , Quantity = x.Sum(y => y.Quantity)
                    }).OrderBy(x => x.PartsNumber).ToList();

                if (oList.Count > 0)
                {
                    strUrl += "?ServiceSlipNumber=" + oList[cnt].ServiceSlipNumber + "&DepartmentCode=" + oList[cnt].DepartmentCode + "&OrderType=" + oList[cnt].OrderType;
                    foreach (var order in oList)
                    {
                        strUrl += "&partsList[" + cnt + "].PartsNumber=" + order.PartsNumber + "&partsList[" + cnt + "].Quantity=" + order.Quantity;
                        cnt++;
                    }

                    header.orderUrlList.Add(strUrl);
                }
            }

            //--------------------------------
            //�ЊO�i��URL�쐬
            //--------------------------------
            //�I�[�_�[�敪���X�g�̎擾
            var oTypeListNonGenuine = orderList.GroupBy(x => x.OrderType).Select(x => new { key = x.Key });

            foreach (var oType in oTypeListNonGenuine)
            {
                cnt = 0;

                strUrl = "/PartsPurchaseOrderNonGenuine/Entry";       //���N�G�X�g��̐ݒ�

                //�I�[�_�[�敪�Ŕ��������i����
                var oListNonGenuine = orderList.Where(x => (x.OrderType.Equals(oType.key) && (x.GenuineType ?? "").Equals(TYPE_NONGENUINE))).GroupBy(x => new { x.PartsNumber, x.ServiceSlipNumber, x.DepartmentCode, x.OrderType }).Select
                    (x => new
                    {
                        PartsNumber = x.Key.PartsNumber
                        ,
                        ServiceSlipNumber = x.Key.ServiceSlipNumber
                        ,
                        DepartmentCode = x.Key.DepartmentCode
                        ,
                        OrderType = x.Key.OrderType
                        ,
                        Quantity = x.Sum(y => y.Quantity)
                    }).OrderBy(x => x.PartsNumber).ToList();


                if (oListNonGenuine.Count > 0)
                {
                    strUrl += "?ServiceSlipNumber=" + oListNonGenuine[cnt].ServiceSlipNumber + "&DepartmentCode=" + oListNonGenuine[cnt].DepartmentCode + "&OrderType=" + oListNonGenuine[cnt].OrderType;
                    foreach (var order in oListNonGenuine)
                    {
                        strUrl += "&partsList[" + cnt + "].PartsNumber=" + order.PartsNumber + "&partsList[" + cnt + "].Quantity=" + order.Quantity;
                        cnt++;
                    }

                    header.orderUrlList.Add(strUrl);
                }
            }
            
            return header;
        }
        #endregion

        #region Ajax��p
        
        /// <summary>
        /// ���f�ꗗ���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>���f�ꗗ</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 �T�[�r�X�`�[ ���i�������o��
        /// 2017/02/03 arc yano #3426 �T�[�r�X�`�[�E�`�[�C�����@������ʕ\���̕s�
        /// 2015/10/28 arc yano #3289 �T�[�r�X�`�[ Ajax�ɂč݌ɔ��f�̈ꗗ���擾����
        /// </history>
        public ActionResult GetMasterList(string code, string slipnumber, int revisionnumber)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();

                var stcokList = new CodeDao(db).GetStockStatusAll(false); //Mod 2017/10/19 arc yano #3803

                //Mod 2017/10/19 arc yano #3803
                //Add 2017/02/03 arc yano #3426
                if (service.CheckModification(slipnumber, revisionnumber))   //�`�[�C�����̏ꍇ
                {
                    stcokList = stcokList.Where(x => !x.StatusType.Equals("001") && !x.StatusType.Equals("010")).ToList();
                }

                foreach (var rec in stcokList)
                {
                    CodeData codeData = new CodeData();

                    codeData.Code = rec.Code;
                    codeData.Name = rec.Name;
                    codeDataList.DataList.Add(codeData);
                }

                return Json(codeDataList);
            }
            return new EmptyResult();
        }

        // 
        /// <summary>
        /// �Ώۂ̖��׍s�̕��i�������ς��ǂ�����Ԃ�(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>������</returns>
        /// <history>
        /// Add 2016/02/12 arc yano #3429 �T�[�r�X�`�[�@���f�̊����^�񊈐��̐���̒ǉ�
        /// </history>
        public ActionResult IsOrdered(string code, int lineNumber)
        {
            if (Request.IsAjaxRequest())
            {

                bool ret = false;       //������

                //�Y���̖��׍s���擾
                ServiceSalesLine line = new ServiceSalesOrderDao(db).GetBySlipNumber(code).ServiceSalesLine.Where(x => x.LineNumber.Equals(lineNumber)).FirstOrDefault();

                //��������1�ł��������ꍇ�͔�����
                if (line != null && line.OrderQuantity > 0)     //Mod 2017/04/24 arc yano 
                {
                    ret = true;        //������
                }

                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �敪�ꗗ���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���</param>
        /// <returns>�敪�ꗗ</returns>
        /// <history>
        /// 2018/02/22 arc yano #3471 �T�[�r�X�`�[�@�敪�̍i���݂̑Ή��@�čX�V
        /// 2016/03/17 arc yano #3471 �T�[�r�X�`�[�@�敪�̍i���݂̑Ή�
        /// </history>
        public ActionResult GetWorkTypeList(string code)
        {
            if (Request.IsAjaxRequest())
            {
                CodeDataList codeDataList = new CodeDataList();
                codeDataList.DataList = new List<CodeData>();

                var workTypeList = new CodeDao(db).GetWorkTypeAll(false);

                //��ʂɂ��i��

                //��� = �T�[�r�X���j���[
                if (!string.IsNullOrWhiteSpace(code) && code.Equals("002"))
                {
                    workTypeList = workTypeList.Where(x => !string.IsNullOrWhiteSpace(x.ServiceMenuUse) && x.ServiceMenuUse.Equals("1")).ToList();

                }
                else if (!string.IsNullOrWhiteSpace(code) && code.Equals("003"))
                {
                    workTypeList = workTypeList.Where(x => !string.IsNullOrWhiteSpace(x.PartsUse) && x.PartsUse.Equals("1")).ToList();
                }

                //�󔒂̍��ڂ�ǉ�
                CodeData DefcodeData = new CodeData();
                DefcodeData.Code = "";
                DefcodeData.Name = "";
                codeDataList.DataList.Add(DefcodeData);

                foreach (var rec in workTypeList)
                {
                    CodeData codeData = new CodeData();

                    codeData.Code = rec.Code;
                    codeData.Name = rec.Name;
                    codeDataList.DataList.Add(codeData);
                }

                return Json(codeDataList);
            }
            return new EmptyResult();
        }

        //Add 2016/05/31 arc nakayama #3568_�y�T�[�r�X�`�[�z���ς���󒍂ɂ���ƃ^�C���A�E�g�ŗ�����
        /// <summary>
        /// ���������ǂ������擾����B(Ajax��p�j
        /// </summary>
        /// <param name="processType">�������</param>
        /// <returns>��������</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();

                ret.Add("ProcessedTime", "��������");

                return Json(ret);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ���Ƃ��琿������擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">���ƃR�[�h</param>
        /// <returns>��������</returns>
        /// <history>
        /// 2019/02/06 yano #3959 �T�[�r�X�`�[���́@��������@�V�K�쐬
        /// </history>
        public ActionResult GetCustomerClaimByServiceWork(string code, string departmentCode)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();

                string makerCode = (new DepartmentDao(db).GetByKey(departmentCode) != null ? new DepartmentDao(db).GetByKey(departmentCode).MainMakerCode : "");

                if (!string.IsNullOrWhiteSpace(makerCode))
                {
                    ServiceWorkCustomerClaim rec = new ServiceWorkCustomerClaimDao(db).GetByKey(code, makerCode);

                    if (rec != null)
                    {
                        codeData.Code = rec.CustomerClaimCode;
                        codeData.Name = (rec.CustomerClaim != null ? rec.CustomerClaim.CustomerClaimName : "");
                    }
                }

                return Json(codeData);
            }
            return new EmptyResult();
        }

        #endregion
    }
}