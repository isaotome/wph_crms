using Crms.Models;                      //Add 2014/08/06 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using CrmsDao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Web.Mvc;

namespace Crms.Controllers
{
  /// <summary>
  /// �ԗ��`�[
  /// </summary>
  [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class CarSalesOrderController : InheritedController {
        
        #region ������
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;
        private bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public CarSalesOrderController()
        {
            this.db = new CrmsLinqDataContext();
        }

        //Add 2014/08/06 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�ԗ��`�[����";     // ��ʖ�
        private static readonly string PROC_NAME_REPORT = "�̔��񍐃f�[�^�o�^"; // ������
        private static readonly string PROC_NAME_CONFIRM = "���F"; // ������
        private static readonly string PROC_NAME_ORDER = "��"; // ������
        private static readonly string PROC_NAME_SAVE = "�`�[�ۑ��i�󒍈ȊO�j"; // ������
        private static readonly string PROC_NAME_AKA = "�ԓ`"; // ������
        private static readonly string PROC_NAME_AKA_KURO = "�ԍ�"; // ������
        private static readonly int DEFAULT_GYO_COUNT = 5;  //�I�v�V�����̃f�t�H���g�s�� 

        //Add 2016/04/05 arc yano #3441 ������^�C�v�̒�`��ǉ�
        private static readonly List<string> excludeList = new List<string>() { "003" }; //������^�C�v = �N���W�b�g
        private static readonly List<string> excluedAccountTypetList = new List<string>() { "012", "013" }; //�����^�C�v = �c��, ����
        private static readonly List<string> excluedAccountTypetList2 = new List<string>() { "004", "012", "013" }; //�����^�C�v = �c��, ����

        private static readonly string ACCOUNTTYPE_LOAN = "004";   //�����^�C�v=���[��    //Add 2022/08/30 yano #4150

        //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
        private static readonly string LAST_EDIT_CARSALSEORDER = "001";           // �ԗ��`�[���͉�ʂōX�V�������̒l


        private static readonly string PTN_CANCEL_ALL= "1";                       //�S�ĉ���           //Add 2018/08/07 yano #3911
        private static readonly string PTN_CANCEL_REGISTRATION = "2";             //�o�^�E��������     //Add 2018/08/07 yano #3911
        private static readonly string PTN_CANCEL_RESERVATION = "99";             //��������           //Add 2018/08/07 yano #3911

        private static readonly string CANCEL_FROM_CANCEL = "2";                  //�󒍌�L�����Z���ɂ���������   //Add 2018/08/07 yano #3911
        private static readonly string CANCEL_FROM_AKADEN = "3";                  //�ԓ`�����ɂ���������           //Add 2018/08/07 yano #3911

        private static readonly string SALESTYPE_BUSINESSSALES = "003";           //�Ɩ��̔�        //Add 2020/11/17 yano #4059
        private static readonly string SALESTYPE_AUTOAUCTION = "004";             //AA              //Add 2020/11/17 yano #4059

        #endregion



        #region �������
        /// <summary>
        /// �ԗ��`�[������ʕ\��
        /// </summary>
        /// <returns>�ԗ��`�[�������</returns>
        [AuthFilter]
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �ԗ��`�[������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��`�[�������</returns>
        /// <history>
        /// 2020/01/14 yano #3982 �y�ԗ��`�[�ꗗ�z�g�p�҂œ`�[�������ł���悤�ɂ��ė~����
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // �f�t�H���g�l�̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);
            //form["EmployeeCode"] = (form["EmployeeCode"] == null ? ((Employee)Session["Employee"]).EmployeeCode : form["EmployeeCode"]);
            form["SalesOrderStatus"] = Request["status"] == null ? form["SalesOrderStatus"] : Request["status"];
            form["DepartmentCode"] = (form["DepartmentCode"] == null ? ((Employee)Session["Employee"]).DepartmentCode : form["DepartmentCode"]);
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;

            PaginatedList<CarSalesHeader>list = GetSearchResultList(form);
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
            ViewData["CampaignCode"] = form["CampaignCode"];
            ViewData["DelFlag"] = form["DelFlag"];
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["Vin"] = form["Vin"];

            //Add 2020/01/14 yano #3982
            ViewData["UserCode"] = form["UserCode"];
            ViewData["UserName"] = form["UserName"];

            //�\�����ڂ̃Z�b�g

            if (!string.IsNullOrEmpty(form["EmployeeCode"])) {
                ViewData["EmployeeName"] = (new EmployeeDao(db)).GetByKey(form["EmployeeCode"], true).EmployeeName;
            }
            //Mod 2015/07/06 arc nakayama DelFlag�Ή��̘R��iGetByKey�̑�Q������False��True�ɕύX�j
            if (!string.IsNullOrEmpty(form["DepartmentCode"])) {
                ViewData["DepartmentName"] = (new DepartmentDao(db)).GetByKey(form["DepartmentCode"], true).DepartmentName;
            }
            CodeDao dao = new CodeDao(db);
            ViewData["SalesOrderStatusList"] = CodeUtils.GetSelectListByModel(dao.GetSalesOrderStatusAll(false),form["SalesOrderStatus"], true);

            return View("CarSalesOrderCriteria",list);
        }

        /// <summary>
        /// �ԗ��`�[�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�ԗ��`�[�������ʃ��X�g</returns>
        /// <history>
        /// 2022/08/30 yano #4079�y�ԗ��`�[���́z�u�����h���Ō������s���Ȃ�
        /// 2020/01/14 yano #3982 �y�ԗ��`�[�ꗗ�z�g�p�҂œ`�[�������ł���悤�ɂ��ė~����
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\ ���̃��[�U���ҏW���Ă���ꍇ�͓��͍��ڂ�񊈐��Ƃ���
        /// </history>
        private PaginatedList<CarSalesHeader> GetSearchResultList(FormCollection form) {
            CarSalesOrderDao carSalesOrderDao = new CarSalesOrderDao(db);
            CarSalesHeader salesHeaderCondition = new CarSalesHeader();
            salesHeaderCondition.Employee = new Employee();
            salesHeaderCondition.Employee.EmployeeCode = form["EmployeeCode"];
            salesHeaderCondition.Customer = new Customer();
            salesHeaderCondition.Customer.CustomerCode = form["CustomerCode"];
            salesHeaderCondition.Customer.CustomerName = form["CustomerName"];
            salesHeaderCondition.Customer.TelNumber = form["TelNumber"];
            salesHeaderCondition.SlipNumber = form["SlipNumber"];
            salesHeaderCondition.CampaignCode1 = form["CampaignCode1"];
            salesHeaderCondition.CampaignCode2 = form["CampaignCode2"];
            salesHeaderCondition.SalesOrderStatus = form["SalesOrderStatus"];
            salesHeaderCondition.QuoteDateFrom = CommonUtils.StrToDateTime(form["QuoteDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.QuoteDateTo = CommonUtils.StrToDateTime(form["QuoteDateTo"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.SalesOrderDateFrom = CommonUtils.StrToDateTime(form["SalesOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesOrderDateTo = CommonUtils.StrToDateTime(form["SalesOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            salesHeaderCondition.DepartmentCode = form["DepartmentCode"];
            salesHeaderCondition.Vin = form["Vin"];
            salesHeaderCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"], DaoConst.SQL_DATETIME_MAX);
            salesHeaderCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"], DaoConst.SQL_DATETIME_MIN);

            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                salesHeaderCondition.DelFlag = form["DelFlag"];
            }
            salesHeaderCondition.SetAuthCondition((Employee)Session["Employee"]);
            salesHeaderCondition.IsAkaKuro = form["AkaKuro"] != null && form["AkaKuro"].Equals("1");

            salesHeaderCondition.User = new Customer();
            salesHeaderCondition.User.CustomerCode = form["UserCode"];                   //Add 2020/01/14 yano #3982
            salesHeaderCondition.User.CustomerName = form["UserName"];          //Add 2020/01/14 yano #3982

            salesHeaderCondition.CarBrandName = form["CarBrandName"];           //Mod 2022/08/30 yano #4079  

            return carSalesOrderDao.GetListByCondition(salesHeaderCondition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }
        #endregion

        #region ��ʃR���g���[���̗L���E����
        /// <summary>
        /// �`�[�X�e�[�^�X����\������View��Ԃ�
        /// </summary>
        /// <param name="header">CarSalesHeader�I�u�W�F�N�g</param>
        /// <returns>�\������r���[</returns
        /// <history>
        /// 2021/08/06 #4088�y�ԗ��`�[���́z���[�������`�F�b�N�R��Ή�
        /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// 2017/01/21 arc yano #3657  ���Ϗ��̌l���̕\���^��\���̃`�F�b�N�{�b�N�X�̕\���E��\����ݒ�
        /// </history>
        private ActionResult GetViewResult(CarSalesHeader header) {
            
            //�R���g���[���̏�����
            header.BasicEnabled = false;
            header.CarEnabled = false;
            header.OptionEnabled = false;
            header.CostEnabled = false;
            header.CustomerEnabled = false;
            header.TotalEnabled = false;
            header.OwnerEnabled = false;
            header.RequestEnabled = false;
            header.RegistEnabled = false;
            header.SalesDateEnabled = false;
            header.SalesPlanDateEnabled = false;�@ //ADD 2014/02/20 ookubo
            header.RateEnabled = false;�@          //ADD 2014/02/20 ookubo
            header.PaymentEnabled = false;
            header.UsedEnabled = false;
            header.InsuranceEnabled = false;
            header.LoanEnabled = false;
            header.CarRegEnabled = false;         //ADD 2016/07/08 nishimura
            header.PInfoChekEnabled = false;     //ADD 2017/01/21 arc yano #3657
            header.ReasonEnabled = false; //Add 2017/11/10 arc yano #3787
        
            header.CarVinEnabled = false;       //Add 2018/08/07 yano #3911
            header.RegistButtonVisible = false; //Add 2018/08/07 yano #3911

            header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029


            //Add 2018/08/07 yano #3911
            bool reservedFlag = false;          //�����σt���O
            bool registeredFlag = false;        //�o�^�σt���O

          
            //�ԗ����o�^�ς̏ꍇ�͎ԑ�ԍ���ύX�ł��Ȃ��悤�ɂ���
            CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //�����ς̏ꍇ�͈����σt���O��ON
            if (rec != null && !string.IsNullOrWhiteSpace(rec.ReservationStatus) && rec.ReservationStatus.Equals("1"))
            {
                reservedFlag = true;
            }
            //�o�^�ς̏ꍇ�͓o�^�σt���O��ON
            if (rec != null && !string.IsNullOrWhiteSpace(rec.RegistrationStatus) && rec.RegistrationStatus.Equals("1"))
            {
                registeredFlag = true;
            }

            
            if (!string.IsNullOrEmpty(header.DelFlag) && header.DelFlag.Equals("1")) {
                header.ShowMenuIndex = 2;
                header.PInfoChekEnabled = true;    //ADD 2017/01/21 arc yano #3657
            
            }
            else if (!string.IsNullOrEmpty(header.LockEmployeeName)) //Mod 2017/11/10 arc yano #3787
            {
                header.ShowMenuIndex = 11;
                ViewData["ProcessSessionError"] = "���̓`�[�͌���" + header.LockEmployeeName + "���񂪎g�p���Ă��邽�ߓǂݎ���p�ŕ\�����Ă��܂�";
            }
            else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("1"))
            {
                // �ԓ`
                header.ShowMenuIndex = 8;
                header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787
            }
            else if (ViewData["Mode"] != null && ViewData["Mode"].Equals("2"))
            {
                // �ԍ�
                header.ShowMenuIndex = 9;
                header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787
            }
            else
            {
                switch (header.SalesOrderStatus)
                {
                    case "001": //����

                        //�o�^���ȊO�S��Enable
                        header.BasicEnabled = true;
                        header.CarEnabled = true;
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.CustomerEnabled = true;
                        header.TotalEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@         //ADD 2014/02/20 ookubo
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;

                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.RequestEnabled = true;
                        header.ShowMenuIndex = 1;
                        header.SalesOrderDateEnabled = true;
                        header.CarRegEnabled = true;        //ADD 2016/07/08 nishimura

                        header.PInfoChekEnabled = true;    //ADD 2017/01/21 arc yano #3657

                        //Add 2020/01/06 yano #4029
                        //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                        if (string.IsNullOrWhiteSpace(header.SalesType) ||
                           (
                            !header.SalesType.Equals("003") &&
                            !header.SalesType.Equals("004") &&
                            !header.SalesType.Equals("008")
                            )
                        )
                        {
                            header.CostAreaEnabled = true;
                        }
                        

                        //Add 2018/08/07 yano #3911
                        if (!reservedFlag && !registeredFlag)
                        {
                            header.CarVinEnabled = true;
                        }


                        break;
                    case "002": //��
                        //��{�A�ԗ��A�ڋqDisable
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.TotalEnabled = true;
                        header.OwnerEnabled = true;
                        header.RegistEnabled = true;
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;


                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@         //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 4;
                        header.SalesOrderDateEnabled = true;
                        header.CarRegEnabled = true;         //ADD 2016/07/08 nishimura

                        //Add 2020/01/06 yano #4029
                        //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                        //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                        if (string.IsNullOrWhiteSpace(header.SalesType) ||
                           (
                            !header.SalesType.Equals("003") &&
                            !header.SalesType.Equals("004") &&
                            !header.SalesType.Equals("008")
                            )
                        )
                        {
                            header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                        }
                        //Add 2018/08/07 yano #3911
                        if (registeredFlag)
                        {
                            header.RegistButtonVisible = true;

                            //�ԗ��o�^�ς�validation�G���[���b�Z�[�W��\�����Ȃ��ꍇ�͈ȉ��̃��b�Z�[�W��\��
                            if (ModelState.IsValid)
                            {
                                ViewData["MessageCarRegisted"] = "���Ɏԗ��o�^�ς̂��߁u�o�^�ς֐i�߂�v�{�^�����N���b�N���ēo�^�ςɂ��ĉ�����";
                            }
                        }

                        break;
                    case "003": //�o�^�ς�
                        //��{�A�ԗ��A�ڋq�A�o�^���A���L�ҏ���Disable
                        header.OptionEnabled = true;
                        header.CostEnabled = true;
                        header.TotalEnabled = true;
                        header.PaymentEnabled = true;
                        header.UsedEnabled = true;


                        header.InsuranceEnabled = true;
                        header.LoanEnabled = true;
                        header.SalesDateEnabled = false;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@         //ADD 2014/02/20 ookubo
                        header.ShowMenuIndex = 5;
                        header.SalesOrderDateEnabled = true;

                        header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029
                        break;
                    case "004": //�[�Ԋm�F������ς�
                        //�[�ԓ��ȊODisable
                        header.SalesDateEnabled = true;
                        header.ShowMenuIndex = 6;
                        header.SalesPlanDateEnabled = true;  //ADD 2014/02/20 ookubo
                        header.RateEnabled = true;�@         //ADD 2014/02/20 ookubo
                        header.SalesOrderDateEnabled = true;

                        header.CostAreaEnabled = false;     //Add 2020/01/06 yano #4029

                        // Add 2015/03/18 arc nakayama �`�[�C���Ή��@�ߋ��ɏC�����s�������̂���`�[�������ꍇ�A���R����\������
                        if (CheckModifiedReason(header.SlipNumber))
                        {
                            header.ModifiedReasonEnabled = true;
                            GetModifiedHistory(header); //�C�������擾
                        }
                        else
                        {
                            header.ModifiedReasonEnabled = false;
                        }

                        //�C�������ǂ����̃`�F�b�N
                        if (CheckModification(header.SlipNumber, header.RevisionNumber))
                        {
                            header.ModificationControl = true; //�C����

                            header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787

                            //�`�[�C��������������Γ��̓G���A���J������
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode))
                            {
                                header.ModificationControlCancel = true; //[�C���L�����Z��]�{�^�� �\��
                                header.ModificationControlCommit = true; // [�C������]�{�^�� �\��
                                header.BasicEnabled = true;
                                header.CarEnabled = true;
                                header.OptionEnabled = true;
                                header.CostEnabled = true;
                                header.CustomerEnabled = true;
                                header.TotalEnabled = true;
                                header.OwnerEnabled = true;
                                header.RequestEnabled = true;
                                header.RegistEnabled = true;
                                header.SalesDateEnabled = true;
                                header.SalesPlanDateEnabled = true;
                                header.RateEnabled = true;
                                header.PaymentEnabled = true;
                                header.UsedEnabled = true;
                                header.InsuranceEnabled = true;
                                header.LoanEnabled = true;
                                header.CarRegEnabled = true;
                                header.PInfoChekEnabled = true;

                                //Add 2020/01/06 yano #4029
                                //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                                //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                                if (string.IsNullOrWhiteSpace(header.SalesType) ||
                                   (
                                    !header.SalesType.Equals("003") &&
                                    !header.SalesType.Equals("004") &&
                                    !header.SalesType.Equals("008")
                                    )
                                )
                                {
                                    header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                                }
                            }
                        }
                        else
                        {
                            header.ModificationControl = false; //�C�����łȂ�

                            // ���O�C�����[�U�[���`�[�C���������������Ă���@���@�ԓ`�܂��͉ߋ��ɐԍ��������s�������`�[�łȂ������ꍇ�͓`�[�C���{�^���\��
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode) && !header.SlipNumber.Contains("-1") && AkakuroCheck(header.SlipNumber))
                            {
                                header.ModificationEnabled = true;  //[�`�[�C��]�{�^���\��
                            }
                            else
                            {
                                header.ModificationEnabled = false; //[�`�[�C��]�{�^����\��
                            }

                        }

                        break;
                    case "005": //�[�ԍς�
                        //�S��Disable
                        header.ShowMenuIndex = 10;

                        bool IsClose = new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, header.SalesDate, "001");

                        // Add 2015/03/18 arc nakayama �`�[�C���Ή��@�ߋ��ɏC�����s�������̂���`�[�������ꍇ�A���R����\������
                        if (CheckModifiedReason(header.SlipNumber))
                        {
                            header.ModifiedReasonEnabled = true;
                            GetModifiedHistory(header); //�C�������擾
                        }
                        else
                        {
                            header.ModifiedReasonEnabled = false;
                        }

                        //�C�������ǂ����̃`�F�b�N
                        if (CheckModification(header.SlipNumber, header.RevisionNumber))
                        {
                            header.ModificationControl = true; //�C���� 

                            header.ReasonEnabled = true;     //Add 2017/11/10 arc yano #3787

                            //�C�����ł��o���̒��ߏ����󋵂������߂��{���߂Ȃ�ύX�͕s�ɂ���[�C���L�����Z��]�{�^����[����]�{�^���݂̂̕\���ɂȂ�B���R�����\������Ȃ�
                            if (IsClose)
                            {
                                //�`�[�C��������������Γ��̓G���A���J������
                                if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode))
                                {
                                    header.ModificationControlCommit = true; // [�C������]�{�^�� �\��
                                    header.ModificationControlCancel = true; //[�C���L�����Z��]�{�^�� �\��
                                    header.BasicEnabled = true;
                                    header.CarEnabled = true;
                                    header.OptionEnabled = true;
                                    header.CostEnabled = true;
                                    header.CustomerEnabled = true;
                                    header.TotalEnabled = true;
                                    header.OwnerEnabled = true;
                                    header.RequestEnabled = true;
                                    header.RegistEnabled = true;
                                    header.SalesDateEnabled = true;
                                    header.SalesPlanDateEnabled = true;
                                    header.RateEnabled = true;
                                    header.PaymentEnabled = true;
                                    header.UsedEnabled = true;
                                    header.InsuranceEnabled = true;
                                    header.LoanEnabled = true;
                                    header.CarRegEnabled = true;
                                    header.PInfoChekEnabled = true;
                                    
                                    //Add 2020/01/06 yano #4029
                                    //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                                    //�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO
                                    if (string.IsNullOrWhiteSpace(header.SalesType) ||
                                       (
                                        !header.SalesType.Equals("003") &&
                                        !header.SalesType.Equals("004") &&
                                        !header.SalesType.Equals("008")
                                        )
                                    )
                                    {
                                        header.CostAreaEnabled = true;     //Add 2020/01/06 yano #4029
                                    }
                                }
                            }
                            else
                            {
                                header.ModificationControlCommit = false;        // [�C������]�{�^�� ��\��
                            }
                        }
                        else
                        {
                            header.ModificationControl = false; //�C�����łȂ�

                            // ���O�C�����[�U�[���`�[�C���������������Ă���@���@�ԓ`�܂��͉ߋ��ɐԍ��������s�������`�[�łȂ������ꍇ�͓`�[�C���{�^���\��
                            if (CheckApplicationRole(((Employee)Session["Employee"]).EmployeeCode) && !header.SlipNumber.Contains("-1") && AkakuroCheck(header.SlipNumber))
                            {
                                header.ModificationEnabled = true;  //[�`�[�C��]�{�^���\��
                            }
                            else
                            {
                                header.ModificationEnabled = false; //[�`�[�C��]�{�^����\��
                            }
                        }

                        //�̔��񍐓��͉�ʂ�\��
                        //return View("CarSalesReportEntry", header);
                        break;
                    case "006": //��ݾ�
                        //�S��Disable 
                        header.ShowMenuIndex = 8;
                        break;
                }

                //�Ǘ��Ҍ����̂ݏ���ŗ��g�p��
                Employee emp = HttpContext.Session["Employee"] as Employee;
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (!CommonUtils.DefaultString(emp.SecurityRoleCode).Equals("999"))
                {
                    header.RateEnabled = false;
                }

                //Add 2021/08/06 #4088
                //���[�������ς��ǂ������`�F�b�N����B
                if (!string.IsNullOrWhiteSpace(header.SlipNumber))
                {
                    List<Journal> jlist = new JournalDao(db).GetListBySlipNumber(header.SlipNumber).Where(x => x.AccountType.Equals("004")).ToList();
                    //���[�������ς̏ꍇ�͕ύX�����Ȃ��B
                    if (jlist.Count > 0)
                    {
                        header.LoanEnabled = false;

                        header.LoanCompleted = true;
                    }
                }
            }
            return View("CarSalesOrderEntry", header);
        }

        #endregion

        #region ���͉�� 
        /// <summary>
        /// �ԗ��`�[���͉�ʕ\��
        /// </summary>
        /// <param name="SlipNo">�`�[�ԍ�</param>
        /// <param name="RevNo">�����ԍ�</param>
        /// <returns>�ԗ��`�[���͉��</returns>
        /// <history>
        /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// 2017/01/21 arc yano #3657 ������̌l���̕\���^��\���`�F�b�N�{�b�N�X�̃f�t�H���g��ݒ�
        /// </history>
        [AuthFilter]
        public ActionResult Entry(string SlipNo, int? RevNo, string customerCode, string employeeCode, string Mode)
        {
            Employee employee;
            
            if (string.IsNullOrEmpty(employeeCode)) {
                //���O�C���S����
                employee = ((Employee)Session["Employee"]);
            } else {
                //�S���҃R�[�h�t���ŌĂяo���ꂽ�Ƃ�
                employee = new EmployeeDao(db).GetByKey(employeeCode);
            }

            //�ڋq�R�[�h�t���ŌĂяo���ꂽ�Ƃ�
            Customer customer = new Customer();
            if (!string.IsNullOrEmpty(customerCode)) {
                customer = new CustomerDao(db).GetByKey(customerCode);
            }

            CarSalesHeader header;

            if (string.IsNullOrEmpty(SlipNo))
            {
                //�V�K�쐬
                header = new CarSalesHeader();

                if (!string.IsNullOrEmpty(Request["salesCarNumber"])) {
                    header.SalesCarNumber = Request["salesCarNumber"];

                    // Mod 2015/05/18 arc nakayama ���b�N�A�b�v�������Ή��@�����f�[�^���\���͂�����(salesCarNumber)
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(Request["salesCarNumber"], true);
                    if (salesCar != null) {
                        header.NewUsedType = salesCar.NewUsedType; 
                        header.MakerName = salesCar.MakerName;
                        header.CarGradeCode = salesCar.CarGradeCode;
                        try { header.CarBrandName = salesCar.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
                        try { header.CarName = salesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
                        try { header.CarGradeName = salesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
                        header.ExteriorColorCode = salesCar.ExteriorColorCode;
                        header.ExteriorColorName = salesCar.ExteriorColorName;
                        header.InteriorColorCode = salesCar.InteriorColorCode;
                        header.InteriorColorName = salesCar.InteriorColorName;
                        header.ModelName = salesCar.ModelName;
                        header.Mileage = salesCar.Mileage;
                        header.MileageUnit = salesCar.MileageUnit;
                        header.Vin = salesCar.Vin;
                        try { header.SalesPrice = salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice; } catch (NullReferenceException) { }

                        //MOD 2014/02/20 ookubo
                        header.SalesTax = 0;   //Decimal.Floor((salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m) * ((Decimal)header.Rate / 100)); } catch (NullReferenceException) { }
                        //try { header.SalesTax = Decimal.Floor((salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m) * 0.05m); } catch (NullReferenceException) { }
                        try { header.InspectionRegistCost = salesCar.CarGrade.InspectionRegistCost; } catch (NullReferenceException) { }
                        try { header.RecycleDeposit = salesCar.RecycleDeposit ?? salesCar.CarGrade.RecycleDeposit; } catch (NullReferenceException) { }

                        //Mod 2019/09/04 yano #4011
                        Tuple<string, decimal> retValue = CommonUtils.GetAcquisitionTax(salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                        header.EPDiscountTaxId = retValue.Item1;
                        header.AcquisitionTax = retValue.Item2;
                        //header.AcquisitionTax = CommonUtils.GetAcquisitionTax(salesCar.SalesPrice ?? salesCar.CarGrade.SalesPrice ?? 0m, 0m, salesCar.CarGrade.VehicleType, salesCar.NewUsedType, salesCar.FirstRegistrationYear);
                       
                        if (salesCar.NewUsedType != null && salesCar.NewUsedType.Equals("U")) {
                            try { header.CarLiabilityInsurance = salesCar.CarLiabilityInsurance ?? new CarLiabilityInsuranceDao(db).GetByUsedDefault().Amount; } catch (NullReferenceException) { }
                            header.CarWeightTax = salesCar.CarWeightTax;
                        } else if (salesCar.NewUsedType != null && salesCar.NewUsedType.Equals("N")) {
                            try { header.CarLiabilityInsurance = salesCar.CarLiabilityInsurance ?? new CarLiabilityInsuranceDao(db).GetByNewDefault().Amount; } catch (NullReferenceException) { }
                            CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, salesCar.CarWeight ?? (salesCar.CarGrade != null ? salesCar.CarGrade.CarWeight : 0) ?? 0);
                            header.CarWeightTax = salesCar.CarWeightTax ?? weightTax.Amount;
                        }        
                    }
                }

                //�����l�ݒ�
                header.RevisionNumber = 0; //�����ԍ��̃��Z�b�g
                header.Employee = employee; //���O�C���S����
                header.Department = employee.Department1;
                header.DepartmentCode = employee.Department1.DepartmentCode;
                header.Customer = customer;
                header.CustomerCode = customer.CustomerCode;
                header.SalesOrderStatus = "001";
                header.HotStatus = "A";
                header.QuoteDate = DateTime.Today;
                header.QuoteExpireDate = DateTime.Today.AddDays(6);
                header.SalesOrderDate = DateTime.Today;
                //ookubo
                //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
                if (header.ConsumptionTaxId == null) {
                    header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                    header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
                }

                // �I�v�V������5�i�쐬
                header.CarSalesLine = new EntitySet<CarSalesLine>();
                for (int i = 1; i <= DEFAULT_GYO_COUNT; i++)
                {
                    header.CarSalesLine.Add(new CarSalesLine());
                }

                //2020/01/06 yano #4029 Mod
                //����p�Œ�l�Z�b�g
                //ConfigurationSetting numberPlateCost = new ConfigurationSettingDao(db).GetByKey("NumberPlateCost");
                //if (numberPlateCost != null) {
                //    header.NumberPlateCost = decimal.Parse(numberPlateCost.Value);
                //}

                ConfigurationSetting stampCost = new ConfigurationSettingDao(db).GetByKey("TradeInFiscalStampCost");
                if(stampCost!=null){
                    header.TradeInFiscalStampCost = decimal.Parse(stampCost.Value);
                }

                // Add 2014/07/29 arc amii �ۑ�Ή� �����󎆑�ɏ����l�ݒ�
                ConfigurationSetting revStampCost = new ConfigurationSettingDao(db).GetByKey("RevenueStampCost");
                if (revStampCost != null)
                {
                    header.RevenueStampCost = decimal.Parse(revStampCost.Value);
                }

                //Mod 2020/01/06 yano #4029
                //if (header.Department != null) {
                //    Office office = header.Department.Office;
                //    if (office.CostArea != null) {
                //        header.InspectionRegistFee = office.CostArea.InspectionRegistFee;
                //        //header.TradeInFee = office.CostArea.TradeInFee;
                //        header.PreparationFee = office.CostArea.PreparationFee;
                //        header.RecycleControlFee = office.CostArea.RecycleControlFee;
                //        header.RequestNumberFee = office.CostArea.RequestNumberFee;
                //        header.TradeInAppraisalFee = office.CostArea.AppraisalFee;
                //        header.ParkingSpaceFee = office.CostArea.ParkingSpaceCost;
                //        header.RequestNumberCost = office.CostArea.RequestNumberCost;
                        
                //    }
                //}

                //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
                header.LastEditScreen = "000";
            }
            else
            {
                //�ҏW�̏ꍇ�͓`�[���Ăяo��
                if (RevNo == null) {
                    header = new CarSalesOrderDao(db).GetBySlipNumber(SlipNo);
                } else {
                    header = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo ?? 1);
                }
                header.CarPurchaseOrder = new CarPurchaseOrderDao(db).GetBySlipNumber(SlipNo);
                if (header.SalesCar != null) {
                    header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
                }
                ViewData["update"] = "1";
                ViewData["Mode"] = Mode;


                //Add 2017/11/10 arc yano #3787
                // �ҏW����������ꍇ�̂݃��b�N����Ώ�
                Employee loginUser = (Employee)Session["Employee"];
                string departmentCode = header.DepartmentCode;
                string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
                // ������E��������P�`�R�E�Z�L�����e�B���x��ALL�̂ǂꂩ�ɊY�������烍�b�N����
                if (!departmentCode.Equals(loginUser.DepartmentCode)
                && !departmentCode.Equals(loginUser.DepartmentCode1)
                && !departmentCode.Equals(loginUser.DepartmentCode2)
                && !departmentCode.Equals(loginUser.DepartmentCode3)
                && !securityLevel.Equals("004"))
                {
                    // �������Ȃ�
                }
                else
                {
                    // �����ȊO�����b�N���Ă���ꍇ�̓G���[�\������
                    string lockEmployeeName = GetProcessLockUser(header);
                    if (!string.IsNullOrEmpty(lockEmployeeName))
                    {
                        header.LockEmployeeName = lockEmployeeName;
                    }
                    else
                    {
                        // �`�[���b�N
                        ProcessLock(header);
                    }
                }

            }
            
            //Add 2017/01/21 arc yano 
            if (header.SalesOrderStatus.Equals("001"))    //�`�[�X�e�[�^�X=�u���ρv
            {
                header.DispPersonalInfo = false;
            }
            else ////�`�[�X�e�[�^�X���u���ρv
            {
                header.DispPersonalInfo = true;
            }

            //�\�����ڂ̃Z�b�g
            SetDataComponent(ref header);

            //�X�e�[�^�X�ɂ���ēK�؂�VIEW��\��
            return GetViewResult(header);
        }

        #endregion

        #region �̔��񍐏�
        /// <summary>
        /// �̔��񍐏����͉�ʂ�\��
        /// </summary>
        /// <param name="SlipNo">�`�[�ԍ�</param>
        /// <param name="RevNo">�����ԍ�</param>
        /// <returns></returns>
        [AuthFilter]
        public ActionResult Report(string SlipNo, int? RevNo) {
            CarSalesHeader header = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo);
            header.CarPurchaseOrder = new CarPurchaseOrderDao(db).GetBySlipNumber(SlipNo);
            if (header.SalesCar != null) {
                header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
            }
            if (header.CarSalesReport != null) {
                ViewData["update"] = "1";
            }
            SetDataComponent(ref header);
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }
        /// <summary>
        /// �̔��񍐖��׍s�ǉ��E�폜
        /// </summary>
        /// <param name="header">�`�[�w�b�_���</param>
        /// <param name="report">�̔��񍐖���</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddReportLine(CarSalesHeader header, EntitySet<CarSalesReport> report, FormCollection form) {
            header.CarSalesReport = report;
            if (!string.IsNullOrEmpty(header.SalesCarNumber)) {
                header.SalesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                header.CarPurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);
            }

            string delLine = form["DelLine"];
            ModelState.Clear();

            //DelLine��0�ȏゾ������w��s�폜
            if (Int32.Parse(delLine) >= 0) {
                header.CarSalesReport.RemoveAt(Int32.Parse(delLine));
            } else {
                header.CarSalesReport.Add(new CarSalesReport());
            }
            ViewData["update"] = form["update"];
            //�\�����ڂ��ăZ�b�g
            SetDataComponent(ref header);
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }

        /// <summary>
        /// �̔��񍐃f�[�^�̓o�^
        /// </summary>
        /// <param name="header">�`�[�w�b�_���</param>
        /// <param name="line">�I�v�V�������׏��</param>
        /// <param name="report">�̔��񍐖��׏��</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SalesReport(CarSalesHeader header, EntitySet<CarSalesReport> report, FormCollection form) {
            using (TransactionScope ts = new TransactionScope()) {

                // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                db = new CrmsLinqDataContext();
                db.Log = new OutputWriter();

                //�X�V����Delete&Insert
                if (form["update"] != null && form["update"].Equals("1")) {
                    List<CarSalesReport> delList = new CarSalesReportDao(db).GetBySlipNumber(header.SlipNumber);
                    db.CarSalesReport.DeleteAllOnSubmit(delList);
                }
                try {
                    // Add 2014/07/23 arc amii �o�^����f�[�^���P�����������A�V�X�e���G���[�ɂȂ�Ȃ��悤�C��
                    // �o�^����f�[�^������ꍇ�̂݁AINSERT���s��
                    if (report != null) {
                        db.CarSalesReport.InsertAllOnSubmit(report);
                    }

                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    //Add 2014/08/06 arc amii �G���[���O�Ή��wthrow e�x����G���[���O���o�͂��鏈���ɕύX
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        //Add 2014/08/06 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(se, PROC_NAME_REPORT, FORM_NAME, "");
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�ۑ�"));
                        //�X�e�[�^�X��߂�
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_REPORT, FORM_NAME, header.SlipNumber);
                        return View("Error");
                    }
                }
                catch (Exception e) {
                    //Add 2014/08/06 arc amii �G���[���O�Ή��wthrow e�x����G���[���O���o�͂��鏈���ɕύX
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_REPORT, FORM_NAME, header.SlipNumber);
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }
            header.CarSalesReport = report;
            SetDataComponent(ref header);
            ViewData["close"] = "1";
            header.ShowMenuIndex = 7;
            return View("CarSalesReportEntry", header);
        }
        #endregion

        #region �ԗ��`�[�R�s�[�@�\
        /// <summary>
        /// �`�[���R�s�[���ē��͉�ʕ\��
        /// �p�r�̈Ⴂ����L�����Z���E�󒍌�L�����Z���Ƃ���ȊO�ŃR�s�[���鍀�ڂ�ς���B
        /// �L�����Z���E�󒍌�L�����Z���`�[�̃R�s�[�c�قڑS�Ă̍��ڂ��R�s�[����
        /// ��L�ȊO�c�̔��ԗ����A�I�v�V�������A�ŋ����̂݃R�s�[
        /// </summary>
        /// <param name="SlipNo">�`�[�ԍ�</param>
        /// <param name="RevNo">�����ԍ�</param>
        /// <returns></returns>
        /// <history>
        /// 2022/05/20 yano #4069�y�ԗ��`�[���́z�ԑ�ԍ�����͂������̎d�l�ύX
        /// 2017/11/08 arc yano #3553 �ԗ��`�[�̃R�s�[�@�\�ǉ� �V�K�쐬
        /// </history>
        public ActionResult Copy(string SlipNo, int RevNo)
        {
            CarSalesHeader header = new CarSalesHeader();

            Employee employee = (Employee)Session["Employee"];

            CarSalesHeader original = new CarSalesOrderDao(db).GetByKey(SlipNo, RevNo);

            //(�󒍌�)�L�����Z���̓`�[���R�s�[����ꍇ
            if (original.SalesOrderStatus.Equals("006") || original.SalesOrderStatus.Equals("007"))
            {
                header = MakeCopyDataFromCancel(header, original);
            }
            else //(�󒍌�)�L�����Z���ȊO�̓`�[���R�s�[����ꍇ
            {
                header = MakeCopyDataFromExceptCancel(header, original);
            }

            SetDataComponent(ref header);

            header.FromCopy = true;   //Add 2022/05/20 yano #4069

            return GetViewResult(header);
        }

        /// <summary>
        /// (�󒍌�)�L�����Z���`�[����R�s�[����
        /// �قڑS�Ă̍��ڂ��R�s�[����
        /// </summary>
        /// <param name="header">�`�[</param>
        /// <param name="orgheader">�R�s�[���`�[</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/08 arc yano #3553 �ԗ��`�[�̃R�s�[�@�\�ǉ� �V�K�쐬
        /// </history>
        public CarSalesHeader MakeCopyDataFromCancel(CarSalesHeader header, CarSalesHeader original)
        {
            Employee employee = (Employee)Session["Employee"];

            //�w�b�_�ɐݒ�
            header = original;

            //���׏��̏�����
            foreach (var l in header.CarSalesLine)
            {
                //�`�[�ԍ��A���r�W����������������
                l.SlipNumber = "";                  //�`�[�ԍ�
                l.RevisionNumber = 0;
            }

            //�x�����̏�����
            foreach (var p in header.CarSalesPayment)
            {
                //�`�[�ԍ��A���r�W����������������
                p.SlipNumber = "";                  //�`�[�ԍ�
                p.RevisionNumber = 0;
            }

            //----------------------------------------------
            //���̏�����(�R�s�[�l���g�p���Ȃ����ڂ̐ݒ�)
            //----------------------------------------------
            header = SetSlipData(header);

            //�Ǘ��ԍ��̐ݒ�
            SalesCar car = new SalesCarDao(db).GetDataByVin(original.Vin);

            //�Ώێԗ��̍݌ɃX�e�[�^�X���u�݌Ɂv�ȊO�̏ꍇ�͊Ǘ��ԍ��͐ݒ肵�Ȃ�
            if (car != null && !string.IsNullOrWhiteSpace(car.CarStatus) && !car.CarStatus.Equals("001"))
            {
                //�Ǘ��ԍ��͋󕶎��ɂ���
                header.SalesCarNumber = "";
            }

            return header;
        }

        /// <summary>
        /// (�󒍌�)�L�����Z���ȊO�̓`�[���R�s�[���ē��͉�ʕ\��
        /// �̔��ԗ����A�I�v�V�������A�ŋ����̍��ڂ̂݃R�s�[
        /// </summary>
        /// <param name="header">�`�[</param>
        /// <param name="orgheader">�R�s�[���`�[</param>
        /// <returns></returns>
        /// <history>
        /// 2023/01/11 yano #4158 �y�ԗ��`�[���́z�C�ӕی������͍��ڂ̒ǉ�
        /// 2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�ǉ�(�����e�i�X�E�����ۏ�)
        /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���  �O�o�������I�v�V�������v���C��
        /// 2017/11/08 arc yano #3553 �ԗ��`�[�̃R�s�[�@�\�ǉ� �V�K�쐬
        /// </history>
        public CarSalesHeader MakeCopyDataFromExceptCancel(CarSalesHeader header, CarSalesHeader original)
        {
            Employee employee = (Employee)Session["Employee"];

            CarGrade carGrade = new CarGradeDao(db).GetByKey(original.CarGradeCode);

            //----------------------------------------------
            //��{���̐ݒ�
            //----------------------------------------------
            header = SetSlipData(header);
            
            header.Employee = employee;                                                 //�S����      
            header.Department = employee.Department1;                                   //����
            header.DepartmentCode = employee.Department1.DepartmentCode;                //����R�[�h
            header.HotStatus = "A";                                                     //HOT�Ǘ�

            //------------------------------
            //�̔��ԗ����̐ݒ�
            //------------------------------
            header.NewUsedType = original.NewUsedType;                                  //�V���敪

            header.SalesType = "001";                                                   //�̔��敪

            header.CarGradeCode = original.CarGradeCode;                                //�O���[�h�R�[�h

            header.CarGrade = carGrade;

            try { header.MakerName = carGrade.Car.Brand.Maker.MakerName; }              //���[�J�[��
            catch (NullReferenceException) { }
            try { header.CarBrandName = carGrade.Car.Brand.CarBrandName; }              //�u�����h��
            catch (NullReferenceException) { }
            try { header.CarName = carGrade.Car.CarName; }                              //�Ԏ햼
            catch (NullReferenceException) { }
            try { header.CarGradeName = carGrade.CarGradeName; }                        //�O���[�h��
            catch (NullReferenceException) { }
            try { header.ModelName = carGrade.ModelName; }                              //�^����
            catch (NullReferenceException) { }
            try { header.SalesPrice = (carGrade.SalesPrice ?? 0m); }                    //�ԗ��{�̉��i
            catch (NullReferenceException) { header.SalesPrice = 0m; }

            //�ԗ��{�̉��i(�����)
            header.SalesTax = CommonUtils.CalculateConsumptionTax(header.SalesPrice, header.Rate);
                    
            //�l���z
            header.DiscountAmount = 0;
            header.DiscountTax = 0;

            //------------------------------
            //�I�v�V�����̐ݒ�
            //------------------------------
            //���׏����擾����
            header.CarSalesLine = original.CarSalesLine;

            foreach (var l in header.CarSalesLine)
            {
                //�`�[�ԍ��A���r�W����������������
                l.SlipNumber = "";                          //�`�[�ԍ�
                l.RevisionNumber = 0;                       //�����ԍ�

                //�I�v�V�������i(�����)
                l.TaxAmount = CommonUtils.CalculateConsumptionTax(l.Amount, header.Rate);
            }

            //-----------------------------
            //�ŋ���
            //-----------------------------
            //�o�^��]��
            header.RequestRegistDate = null;

            //�����ԐŊ����\��
            header.AcquisitionTax = original.AcquisitionTax;
            
            //�����\���ŗ�
            header.EPDiscountTaxId = original.EPDiscountTaxId;      //Add 2019/09/04 yano #4011

            /*
            if (carGrade != null)
            {
                header.AcquisitionTax = CommonUtils.GetAcquisitionTax(carGrade.SalesPrice ?? 0m, 0m, carGrade.VehicleType, header.NewUsedType, "");
            }
            */

            //�����ӕی��� 
            header.CarLiabilityInsurance = original.CarLiabilityInsurance;
            /*
            if (header.NewUsedType != null && header.NewUsedType.Equals("U"))
            {
                try { header.CarLiabilityInsurance =  new CarLiabilityInsuranceDao(db).GetByUsedDefault().Amount; }
                catch (NullReferenceException) { }
            }
            else if (header.NewUsedType != null && header.NewUsedType.Equals("N"))
            {
                try { header.CarLiabilityInsurance = new CarLiabilityInsuranceDao(db).GetByNewDefault().Amount; }
                catch (NullReferenceException) { }
                CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, (carGrade != null ? carGrade.CarWeight : 0) ?? 0);
                header.CarWeightTax = weightTax.Amount;
            }
            */

            //�����Ԑ�(��ʊ��j
            header.CarTax = original.CarTax;

            //�����ԏd�ʐ�
            header.CarWeightTax = original.CarWeightTax;
            /*
            if (carGrade != null)
            {
                CarWeightTax weightTax = new CarWeightTaxDao(db).GetByWeight(3, (carGrade != null ? carGrade.CarWeight : 0) ?? 0);
                header.CarWeightTax = weightTax.Amount;
            }
            */

            //-----------------------------
            //���̑���ې�
            //-----------------------------
            //�Ԍɏؖ��؎���
            //header.ParkingSpaceCost = original.ParkingSpaceCost;      //Mod 2020/01/06 yano #4029
                                                                        
            //�����o�^�󎆑� 
            header.InspectionRegistCost = original.InspectionRegistCost;

            //header.InspectionRegistCost = carGrade != null && carGrade.InspectionRegistCost != null ? carGrade.InspectionRegistCost : original.InspectionRegistCost;      

            //���ް��ڰđ�(���)
            //header.NumberPlateCost = original.NumberPlateCost;       //Mod 2020/01/06 yano #4029

            /*
            ConfigurationSetting numberPlateCost = new ConfigurationSettingDao(db).GetByKey("NumberPlateCost");
            if (numberPlateCost != null)
            {
                header.NumberPlateCost = decimal.Parse(numberPlateCost.Value);                                                                                          
            }
            */

            //����ԓo�^�󎆑�(����Ԃ͂O��Ȃ̂�null)
            header.TradeInCost = null;                                                                                                                                  

            //���T�C�N���a����
            header.RecycleDeposit = original.RecycleDeposit;
            //header.RecycleDeposit = carGrade != null && carGrade.RecycleDeposit != null ? carGrade.RecycleDeposit : original.RecycleDeposit;

            //���ް��ڰđ�(��]) ���̔�����p���Őݒ� 
            //�����󎆑�
            header.RevenueStampCost = original.RevenueStampCost;

            /*
            ConfigurationSetting revStampCost = new ConfigurationSettingDao(db).GetByKey("RevenueStampCost");
            if (revStampCost != null)
            {
                header.RevenueStampCost = decimal.Parse(revStampCost.Value);
            }
            */

            //���掩���Ԑ�(��ʊ�)�a���(����Ԃ͂O��Ȃ̂�null)
            header.TradeInCarTaxDeposit = null;

            //���̑�(���ږ�)
            header.TaxFreeFieldName = "";

            //���̑�(���z)
            header.TaxFreeFieldValue = null;
            
            //�C�ӕی�
            header.VoluntaryInsuranceAmount = null;   //Add 2023/01/11 yano #4158 

            //-----------------------------
            //�̔�����p
            //-----------------------------
            header.InspectionRegistFee = original.InspectionRegistFee;                              //�����o�^�葱��s��p(�Ŕ�)
            header.RequestNumberFee = original.RequestNumberFee;                                    //��]�i���o�[�\���萔���i�Ŕ��j
            header.PreparationFee = original.PreparationFee;                                        //�[�ԏ�����p

            header.TradeInMaintenanceFee = original.TradeInMaintenanceFee;                         //���Îԓ_���E������p�i�Ŕ��j
            header.FarRegistFee = original.FarRegistFee;                                            //�����o�^��s��p�i�Ŕ��j
            header.TradeInFee = null;                                                               //����ԏ��葱��p�i�Ŕ��j

            header.RecycleControlFee = original.RecycleControlFee;                                  //���T�C�N�������Ǘ����i�Ŕ��j
            header.TradeInAppraisalFee = null;                                                      //����ԍ����p�i�Ŕ��j

            header.ParkingSpaceFee = original.ParkingSpaceFee;                                      //�Ԍɏؖ��葱��s��p�i�Ŕ��j

            header.InheritedInsuranceFee = original.InheritedInsuranceFee;                          //���ÎԌp��������p�i�Ŕ��j
            
            header.TaxationFieldName = "";                                                          //���̑�(���ږ�)

            header.TaxationFieldValue = null;                                                       //���̑�(�Ŕ����z)

            //header.RequestNumberCost = original.RequestNumberCost;                                  //���ް��ڰđ�(��]) //Mod 2020/01/06 yano #4029

            /*
            if (header.Department != null)
            {
                Office office = header.Department.Office;
                if (office.CostArea != null)
                {
                    header.InspectionRegistFee = office.CostArea.InspectionRegistFee;               //�����o�^�葱��s��p(�Ŕ�)
                    header.RequestNumberFee = office.CostArea.RequestNumberFee;                     //��]�i���o�[�\���萔���i�Ŕ��j
                    header.PreparationFee = office.CostArea.PreparationFee;                         //�[�ԏ�����p

                    header.TradeInMaintenanceFee = null;                                            //���Îԓ_���E������p�i�Ŕ��j
                    header.FarRegistFee = null;                                                     //�����o�^��s��p�i�Ŕ��j
                    header.TradeInFee = null;                                                       //����ԏ��葱��p�i�Ŕ��j

                    header.RecycleControlFee = office.CostArea.RecycleControlFee;                   //���T�C�N�������Ǘ����i�Ŕ��j
                    header.TradeInAppraisalFee = office.CostArea.AppraisalFee;                      //����ԍ����p�i�Ŕ��j

                    header.ParkingSpaceFee = office.CostArea.ParkingSpaceCost;                      //�Ԍɏؖ��葱��s��p�i�Ŕ��j

                    try { header.InheritedInsuranceFee = carGrade.Under24; }                        //���ÎԌp��������p�i�Ŕ��j
                    catch (NullReferenceException) { }

                    header.TaxationFieldName = original.TaxationFieldName;                          //���̑�(���ږ�)

                    header.TaxationFieldValue = original.TaxationFieldValue;                        //���̑�(�Ŕ����z)

                    header.RequestNumberCost = office.CostArea.RequestNumberCost;                   //���ް��ڰđ�(��])
                }
            }
            */

            return header;
        }
                    
        /// <summary>
        /// ���ʍ��ڂ̐ݒ�
        /// </summary>
        /// <param name="header">�`�[</param>
        /// <param name="orgheader">�R�s�[���`�[</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/08 arc yano #3553 �ԗ��`�[�̃R�s�[�@�\�ǉ� �V�K�쐬
        /// </history>
        public CarSalesHeader SetSlipData(CarSalesHeader header)
        {
            //----------------------------------------------
            //���̏�����(�R�s�[�l���g�p���Ȃ����ڂ̐ݒ�)
            //----------------------------------------------
            header.SlipNumber = "";                                         //�`�[�ԍ�
            header.RevisionNumber = 0;                                      //�����ԍ�

            header.QuoteDate = DateTime.Today;                              //���ϓ�
            header.QuoteExpireDate = DateTime.Today.AddDays(6);             //���ϗL������
            header.SalesOrderDate = DateTime.Today;                         //�󒍓�

            header.SalesPlanDate = null;                                    //�[�ԗ\���
            header.SalesDate = null;                                        //�[�ԓ�

            header.SalesOrderStatus = "001";                                //�`�[�X�e�[�^�X=����

            //����ŗ�
            //�����ID�����ݒ�ł���΁A�������t�ŏ����ID�擾
            if (header.ConsumptionTaxId == null)
            {
                header.ConsumptionTaxId = new ConsumptionTaxDao(db).GetConsumptionTaxIDByDate(System.DateTime.Today);
                header.Rate = int.Parse(new ConsumptionTaxDao(db).GetConsumptionTaxRateByKey(header.ConsumptionTaxId));
            }

            return header;
        }

        #endregion

        #region ���F���
        /// <summary>
        /// �ԗ��`�[�̏��F��ʂ�\��
        /// </summary>
        /// <param name="SlipNo">�`�[�ԍ�</param>
        /// <returns>�ԗ��`�[���F���</returns>
        [AuthFilter]
        public ActionResult Confirm(string SlipNo) {
            CarSalesHeader header = (new CarSalesOrderDao(db).GetBySlipNumber(SlipNo));

            //���͍��ڂ͑S��Disable
            header.BasicEnabled = false;
            header.CarEnabled = false;
            header.OptionEnabled = false;
            header.CostEnabled = false;
            header.CustomerEnabled = false;
            header.TotalEnabled = false;
            header.OwnerEnabled = false;
            header.RegistEnabled = false;
            header.SalesDateEnabled = false;
            header.RateEnabled = false;         //ADD 2014/02/20 ookubo
            header.PaymentEnabled = false;
            header.UsedEnabled = false;
            header.InsuranceEnabled = false;
            header.LoanEnabled = false;
            header.ShowMenuIndex = 3;
            SetDataComponent(ref header);
            return View("CarSalesOrderEntry",header);
        }

        /// <summary>
        /// ���F�{�^�������������̏���
        /// </summary>
        /// <param name="SlipNo">�`�[�ԍ�</param>
        /// <returns>�ԗ��`�[���F���</returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Confirm(FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(form["SlipNumber"]);
            header.ApprovalFlag = "1";


            using (TransactionScope ts = new TransactionScope()) {
                List<Task> list = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_PURCHASE_APPROVAL, form["SlipNumber"]);
                foreach (var a in list) {
                    a.TaskCompleteDate = DateTime.Now;
                    a.LastUpdateDate = DateTime.Now;
                    a.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                // Add 2014/08/06 arc amii �G���[���O�Ή� catch���ChangeConflictException��ǉ�
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_CONFIRM, FORM_NAME, header.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_CONFIRM, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                
                
            }
            ViewData["close"] = "1";
            SetDataComponent(ref header);
            return View("CarSalesOrderEntry", header);
        }
        #endregion

        #region �I�v�V�����ǉ��폜
        /// <summary>
        /// �I�v�V�����s�ǉ��E�s�폜�{�^�������������Ƃ��̏���
        /// </summary>
        /// <param name="header">�`�[�w�b�_</param>
        /// <param name="line">�I�v�V��������</param>
        /// <param name="pay">�x������</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <history>
        /// 2020/11/19 yano #4060 �y�ԗ��`�[���́z�I�v�V�����s�ǉ��E�폜���̊����\���v�Z�̕s�
        /// 2019/10/22 yano #4024 �y�ԗ��`�[���́z�I�v�V�����s�ǉ��E�폜���ɃG���[�����������̕s��Ή�
        /// 2019/10/17 yano #4022 �y�ԗ��`�[���́z����̏������ł̊����\���̌v�Z
        /// 2016/01/14 arc yano #3354 �ԗ��`�[�̃I�v�V�����s��25�s�L�鎞�̍폜�̕s�
        ///                     �s���`�F�b�N�͍s�ǉ��̏ꍇ�̂ݍs���悤�ɏC��
        /// </history>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Option(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {
            
            //Add 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }
            
            //Add 2014/05/16 yano vs2012�ڍs���Ή� �x���s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }            
            
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;

            string delLine = form["DelLine"];
            ModelState.Clear();
            
            //2016/01/14 arc yano #3354

            //DelLine��0�ȏゾ������w��s�폜
            if (Int32.Parse(delLine) >= 0) {
                header.CarSalesLine.RemoveAt(Int32.Parse(delLine));
                header = CalcAmount(header);
            } else {
                
                if (line != null && line.Count > 24)
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0014", new string[] { "�I�v�V����", "25" }));
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }

                header.CarSalesLine.Add(new CarSalesLine());
            }
            
			//�s�ǉ�/�폜����5�s�����ɂȂ�ꍇ�A�T�s�ɂȂ�悤�ɍs��ǉ�����  
            if (header.CarSalesLine.Count() < DEFAULT_GYO_COUNT)
            {
                int AddCount = DEFAULT_GYO_COUNT - header.CarSalesLine.Count();
                for (int i = 0; i < AddCount; i++)
                {
                    header.CarSalesLine.Add(new CarSalesLine());
                }
            }

            //�\�����ڂ��ăZ�b�g
            SetDataComponent(ref header);

            //Add 2019/10/17 yano #4022
            string firstregist = "";

            SalesCar sc = header.SalesCar;
            CarGrade cg = header.CarGrade;

            if (sc == null)
            {
                //sc = new SalesCarDao(db).GetByVin(header.Vin).Where(x => !x.CarStatus.Equals("006")).FirstOrDefault();
                sc = new SalesCarDao(db).GetByVin(header.Vin).Where(x => x.CarStatus == null || !x.CarStatus.Equals("006")).FirstOrDefault();   //Mod 2019/10/22 yano #4024
            }

            if (cg == null)
            {
                cg = new CarGradeDao(db).GetByKey(header.CarGradeCode);
            }

            if (sc != null)
            {
                firstregist = string.Format("{0:yyyy/MM}", sc.FirstRegistrationDate);
            }

            //Mod 2020/11/19 yano #4060
            //�����\���ŗ����󗓂łȂ��ꍇ�̂݌v�Z���s��
            if (string.IsNullOrWhiteSpace(header.EPDiscountTaxId) || !header.EPDiscountTaxId.Equals("999"))
            {
                header.AcquisitionTax = CommonUtils.GetAcquisitionTax((cg != null ? (cg.SalesPrice ?? 0m) : 0m), header.MakerOptionAmount ?? 0m, (cg != null ? cg.VehicleType : ""), header.NewUsedType, firstregist, header.EPDiscountTaxId, header.RequestRegistDate).Item2;
            }
            
            //�o��
            return GetViewResult(header);
        }
        #endregion

        #region �x�����@�ǉ��폜
        /// <summary>
        /// �x�����@�̍s�ǉ��E�s�폜�{�^�����������ꂽ�Ƃ��̏���
        /// </summary>
        /// <param name="header">�`�[�w�b�_</param>
        /// <param name="line">�I�v�V��������</param>
        /// <param name="pay">�x������</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Payment(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }


            //Add 2014/05/16 yano vs2012�ڍs���Ή� �x���s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;


            string delPayLine = form["DelPayLine"];
            ModelState.Clear();

            //DelPayLine��0�ȏゾ������w��s�폜
            if (Int32.Parse(delPayLine) >= 0) {
                header.CarSalesPayment.RemoveAt(Int32.Parse(delPayLine));
                header = CalcAmount(header);
            } else {
                CarSalesPayment addLine = new CarSalesPayment();
                if (!string.IsNullOrEmpty(header.CustomerCode)) {
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    if (customer != null && customer.CustomerClaim!=null) {
                        addLine.CustomerClaimCode = customer.CustomerClaim.CustomerClaimCode;
                    }
                }
                header.CarSalesPayment.Add(addLine);
            }

            //�\�����ڂ��ăZ�b�g
            SetDataComponent(ref header);

            //�x�����@���͉�ʂ������\���ɂ���
            ViewData["displayContents"] = "invoice";

            //�o��
            return GetViewResult(header);

        }
        #endregion

        #region ��
        /// <summary>
        /// �󒍏���
        /// </summary>
        /// <param name="header">�`�[�w�b�_</param>
        /// <param name="line">�I�v�V��������</param>
        /// <param name="pay">�x������</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <hisotry>
        /// 2023/09/05 #4162 �C���{�C�X�Ή� �C���{�C�X�p�̏���Ōv�Z������ǉ�
        /// 2018/08/07 yano #3911 �o�^�ώԗ��̎ԗ��`�[�X�e�[�^�X�C���ɂ��ā@�ԗ��o�^�ϓ`�[�̏ꍇ�́A�󒍏����̃^�C�~���O�ŃX�e�[�^�X��o�^�ςɂ���B
        /// 2017/04/24 arc yano #3755 ���z���̓��͎��̃J�[�\���ʒu�̕s� ModelState.Clear()�̒ǉ�
        /// 2016/04/08 arc yano #3482 �����|�����P�Ή����ԗ��`�[���� �����\��쐬�O�Ɋ����̓����\�肪���݂���ꍇ�́A�폜���� ���o���p���R�[�h�͏���
        /// </hisotry>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Order(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012�ڍs���Ή� �x���s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }  

            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
            //��ʂ̒l���N���A����
            ModelState.Clear(); //2017/04/24 arc yano #3755

            //����Validation�`�F�b�N
            ////Add 2016/12/08 arc nakayama #3674_�x�����z�ƌ����̔����v�̍����`�F�b�N���󒍈ȍ~�ɕύX����@�����ǉ�
            ValidateAllStatus(header, form, true);
            if (!ModelState.IsValid) {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //�󒍏�����p���̓`�F�b�N
            ValidateCarSalesOrder(header);
            if (!ModelState.IsValid) {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //ADD 2014/02/20 ookubo
            for (int i = 0; i < header.CarSalesLine.Count; i++)
            {
                header.CarSalesLine[i].ConsumptionTaxId = header.ConsumptionTaxId;
                header.CarSalesLine[i].Rate = header.Rate;
            }

            using (TransactionScope ts = new TransactionScope()) {
                
                //�V�����`�[�f�[�^���쐬
                CreateCarSalesOrder(header);

                //����Ԃ̍���f�[�^��INSERT����
                for (int i = 1; i <= 3; i++)
                {
                    CreateAppraisalData(header, i, true);
                }

                //Mod 2018/08/07 yano #3911
                //�����̎ԗ������˗��f�[�^���擾����
                CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

                //�����f�[�^�����݂��Ȃ��ꍇ
                if (rec == null)
                {
                    //�ԗ������˗��f�[�^��INSERT����
                    CreatePurchaseOrderData(header);
                }

                //Mod 2016/04/12 arc yano #3482
                //�����̓����\����폜
                List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
                foreach (var d in delList)
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                //Add 2016/09/06 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                //����ԂɊւ�������\����쐬����
                CreateTradeReceiptPlan(header);

                //�����\��f�[�^��INSERT����
                CreatePaymentPlan(header);

                header.Department = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                header.Employee = new EmployeeDao(db).GetByKey(header.EmployeeCode);

                if (!ModelState.IsValid) {
                    SetDataComponent(ref header);
                    return GetViewResult(header);
                }
 
                //�f�[�^�C���T�[�g
                header.SalesOrderStatus = "002";
                
                //header.SalesOrderDate = DateTime.Today;
                db.CarSalesHeader.InsertOnSubmit(header);

                //Add 2023/09/05 #4162
                InsertInvoiceConsumptionTax(header);

                try {
                    db.SubmitChanges();
                    //�R�~�b�g
                    ts.Complete();

                } catch (SqlException se) {
                    //Add 2014/08/06 arc amii �G���[���O�Ή�SQL�����Z�b�V�����ɓo�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        //Add 2014/08/06 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(se, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        //�X�e�[�^�X��߂�
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    } else {
                        //Add 2014/08/06 arc amii �G���[���O�Ή�SQL�����Z�b�V�����ɓo�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_ORDER, FORM_NAME, header.SlipNumber);
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            //�S�Đ���������󒍑���𑗐M����
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            task.SendSalesOrderFlash(header);

            //���܂��Ȃ�
            ModelState.Clear();

            //�\�����ڂ̍ăZ�b�g
            SetDataComponent(ref header);
            //ViewData["close"] = "1";

            List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();


            //��������-������ꗗ�쐬�p
            List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

            bool Mflag = false;
            foreach (string JClaim in JournalClaimList)
            {
                var Ret = from a in payList
                          where a.CustomerClaimCode.Equals(JClaim)
                          select a;

                if (Ret.Count() == 0)
                {
                    Mflag = true;
                    break;
                }
            }

            if (Mflag)
            {
                ModelState.AddModelError("", "�����͊������܂������A�x����񗓂ɐݒ肳��Ă��Ȃ������悩��������т�����܂��B");
            }

            return GetViewResult(header);
        }
        #endregion

        #region �`�[�ۑ��i�󒍈ȊO�j
        /// <summary>
        /// �ԗ��`�[�̕ۑ�
        /// </summary>
        /// <param name="header">�`�[�w�b�_���</param>
        /// <param name="line">�I�v�V�������</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2023/09/05 #4162 �C���{�C�X�Ή�
        /// 2020/01/09 yano #4030�y�ԗ��`�[���́z�ԍ������Ŕ[�ԏ������s���Ǝԗ��}�X�^�̔[�ԓ����X�V����Ă��܂��B
        /// 2018/12/21 yano #3965 WE�ŐV�V�X�e���\�z �[�Ԋm�F���͕\�����Ȃ��A�X�e�[�^�X�J�ڂ̂�
        /// 2018/11/07 yano #3939 �ԗ��`�[���́@�[�ԑO�i�[�Ԋm�F���o�͌�j�̒������o�͎��ɉ����ԍ���i�߂Ȃ�
        /// 2018/08/14 yano #3910 �ԗ��`�[�@�f���J�[�̔[�ԍϓ`�[�C�����Ɏԗ��}�X�^�̃��P�[�V������������
        /// 2018/08/07 yano #3911 �o�^�ώԗ��̎ԗ��`�[�X�e�[�^�X�C���ɂ���
        /// 2018/06/22 arc yano #3898 �ԗ��}�X�^�@AA�̔��Ŕ[�Ԍ�L�����Z���ƂȂ����ꍇ�̍݌ɃX�e�[�^�X�ɂ���
        /// 2018/05/30 arc yano #3889 �T�[�r�X�`�[�������i����������Ȃ�
        /// 2018/02/20 arc yano #3858 �T�[�r�X�`�[�@�[�Ԍ�̕ۑ������ŁA�[�ԓ����󗓂ŕۑ��ł��Ă��܂�
        /// 2017/12/22 arc yano #3793 �����\��č쐬���̕s�
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// 2017/10/10 arc yano #3802 �������ѕ\���@�������т̂���ԗ��`�[���󒍌�L�����Z�������ꍇ�̕\���ɂ���
        /// 2016/09/27 arc nakayama #3630_�y�����z�ԗ����|���Ή�
        /// 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs
        /// 2017/01/24 arc nakayama #3690_���q�w���\����������o�[���֓o�^������A���q�`�[�̎x�����@�ɓ������z�����o�^�ł��[�ԍςɂł��Ă��܂��B
        /// 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
        /// 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
        /// 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
        /// 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(CarSalesHeader header,EntitySet<CarSalesLine> line,EntitySet<CarSalesPayment> pay ,FormCollection form)
        {
            // Mod 2014/11/14 arc yano #3129
            // Add 2014/08/06 arc amii
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

   
            //Add 2014/05/16 yano vs2012�ڍs���Ή�
      if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012�ڍs���Ή�
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;

            //Add 2017/01/24 arc nakayama #3690
            //��ʂ̒l���N���A����
            ModelState.Clear();

            //Add 2017/05/24 arc nakayama #3761
            if (header.ActionType == "ModificationStart" || header.ActionType == "ModificationCancel")
            {
                //�`�[���C�����E�C��������������
                if (header.ActionType == "ModificationStart")
                {
                    //�[�ԍρA�܂��́A�[�ԑO�ł��[�ԓ������͂���Ă�������߂̃`�F�b�N���s��
                    if (header.SalesOrderStatus.Equals("005") || (header.SalesOrderStatus.Equals("004") && header.SalesDate != null))
                    {
                        //�{���߂������ꍇ�̓G���[�A�C�����ɂ����Ȃ�
                        if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(header.DepartmentCode, header.SalesDate, "001") && header.ActionType == "ModificationStart")
                        {
                            ModelState.AddModelError("SalesDate", "�{���߂����s���ꂽ���߁A�`�[�C���͍s���܂���B");

                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                    }

                    //Add 2017/11/10 arc yano #3787
                    //���̃��[�U�ɓ`�[�����b�N����Ă��邩���`�F�b�N����B
                    ValidateProcessLock(header, form);
                    if (!ModelState.IsValid)
                    {
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }

                    ModificationStart(header);
                }
                //�C���L�����Z���������ꂽ�ꍇ�͏C�������폜����i�C�����L�����Z������j
                if (header.ActionType == "ModificationCancel")
                {
                    //Add 2017/11/10 arc yano #3787
                    //���̃��[�U�ɓ`�[�����b�N����Ă��邩���`�F�b�N����B
                    ValidateProcessLock(header, form);
                    if (!ModelState.IsValid)
                    {
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }

                    ModificationCancel(header);

                    CarSalesHeader Prevheader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    if (Prevheader.CarSalesLine == null)
                    {
                        Prevheader.CarSalesLine = getCarSalesLinebyReq();
                    }

                    if (Prevheader.CarSalesPayment== null)
                    {
                        Prevheader.CarSalesPayment = getCarSalesPaymentbyReq();
                    }

                    SetDataComponent(ref Prevheader);
                    return GetViewResult(Prevheader);
                }

                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //Add 2017/11/10 arc yano #3787
            if (form["ForceUnLock"] != null && form["ForceUnLock"].Equals("1"))
            {
                //DB����ŐV�̎ԗ��`�[���擾����
                header = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                ProcessLockUpdate(header);

                //�ԓ`�E�ԍ��������Ƃ��̍l��
                if (!string.IsNullOrWhiteSpace(form["Mode"]))
                {
                    ViewData["Mode"] = form["Mode"];
                }

                SetDataComponent(ref header);

                return GetViewResult(header);
            }

            //Add 2017/11/10 arc yano #3787
            //�`�[���b�N�`�F�b�N
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                header.SalesOrderStatus = form["PreviousStatus"];   //Add 2018/05/30 arc yano #3889
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            //����Validation�`�F�b�N
            ValidateAllStatus(header, form);

            //Mod 2014/08/14 arc amii �G���[���O�Ή�
            //�󒍈ȍ~�͎󒍂Ɠ���Validation�`�F�b�N���s��
            if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
            {
                ValidateCarSalesOrder(header);
            }

            for (int i = 0; i < header.CarSalesLine.Count; i++)
            {
                header.CarSalesLine[i].ConsumptionTaxId = header.ConsumptionTaxId;
                header.CarSalesLine[i].Rate = header.Rate;
            }

            //Validation�G���[�����݂���ꍇ
            if (!ModelState.IsValid)
            {
                //Mod 2014/08/14 arc amii �G���[���O�Ή�
                //�[�ԏ����̎��̓f�[�^���Ď擾����
                if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
                {
                    header = new CarSalesOrderDao(db).GetByKey(header.SlipNumber,header.RevisionNumber);
                    //�[�ԓ����ăZ�b�g
                    header.SalesDate = CommonUtils.StrToDateTime(form["SalesDate"]);

                    //Add 2018/02/20 arc yano #3858 �`�[�C���̗��R��ݒ�
                    if (!string.IsNullOrWhiteSpace(form["Reason"]))
                    {
                        header.Reason = form["Reason"];
                    }
                }
                //�X�e�[�^�X��߂�
                header.SalesOrderStatus = form["PreviousStatus"];
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

          
            //�g�����U�N�V���������J�n
            using (TransactionScope ts = new TransactionScope()) {

                //Mod 2018/11/07 yano #3939
                CarSalesHeader target = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);
               
                //DB�o�^�ς̍ŐV�`�[�̃X�e�[�^�X���u�[�ԑO�v�u�[�ԍρv�̏ꍇ�ł��`�[�C�������łȂ��ꍇ
                if ((target != null && (target.SalesOrderStatus.Equals("004") || target.SalesOrderStatus.Equals("005"))) && 
                    (string.IsNullOrWhiteSpace(header.ActionType) || !header.ActionType.Equals("ModificationEnd")) )
                {
                    ModelState.Clear();
                    
                    //UpdateModel(target.CarSalesLine); 
                    //UpdateModel(target.CarSalesPayment);

                    target.ProcessSessionControl = new ProcessSessionControlDao(db).GetByKey(header.ProcessSessionId);

                    //target = CopyCarSalesHeader(header, target);

                    UpdateModel(target);

                    target.LastUpdateEmployeeCode = header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    target.LastUpdateDate = header.LastUpdateDate = DateTime.Now;

                    foreach (var l in target.CarSalesLine)
                    {
                        l.LastUpdateDate = DateTime.Now;
                        l.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                    foreach (var p in target.CarSalesPayment)
                    {
                        p.LastUpdateDate = DateTime.Now;
                        p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                }
                else
                {
                    //�V�����`�[�f�[�^���쐬
                    CreateCarSalesOrder(header);

                    //Mod 2018/11/07 yano #3939 ���i�ړ�
                    //�f�[�^�C���T�[�g
                    db.CarSalesHeader.InsertOnSubmit(header);
                }


                //�L�����Z�������̎��̓L�����Z�����t���Z�b�g
                if (!string.IsNullOrEmpty(form["Cancel"]) && form["Cancel"].Equals("1")) {

                    //Mod 2018/06/22 arc yano #3898 
                    //-----------------------------
                    //�ԗ���������
                    //-----------------------------
                    //�ԗ��������������̗L�����擾
                    bool cancelAuth = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "ReservationCancel").EnableFlag;    //Add 2018/08/07 yano #3911

                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);

                    string customerType = customer != null ? customer.CustomerType : "";

                    //Mod 2018/08/07 yano #3911  
                    //�󒍌�L�����Z���̏ꍇ�̔��敪��AA�̔��A�Ɣ́A�˔p�̏ꍇ�݈̂���������
                    if (
                          header.SalesType.Equals("003") ||     //�̔��敪=�u�Ɣ́v
                          header.SalesType.Equals("009") ||     //�̔��敪=�u�X�Ԉړ��v
                         (header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) ||  //�̔��敪=�uAA�v���ڋq�敪=�uAA�v
                         (header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))     //�̔��敪=�u�˔p�v���ڋq�敪=�u�p���v
                        )
                    {
                        //�S�ĉ���
                        ReleaseProvision(header, PTN_CANCEL_ALL, CANCEL_FROM_CANCEL);
                    }
                    else
                    {
                        //�ԗ��̈�����Ԃ��`�F�b�N
                        // �����ς݂̎ԗ��͈�������
                        CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
                        
                        //�o�^�ς̏ꍇ
                        if (order != null && !string.IsNullOrWhiteSpace(order.RegistrationStatus) && order.RegistrationStatus.Equals("1"))
                        {
                            //�o�^�E��������
                            ReleaseProvision(header, PTN_CANCEL_REGISTRATION, CANCEL_FROM_CANCEL);
                        }
                        else if (order != null && !string.IsNullOrWhiteSpace(order.ReservationStatus) && order.ReservationStatus.Equals("1"))
                        {
                            //�����̂݉���
                            ReleaseProvision(header, PTN_CANCEL_RESERVATION, CANCEL_FROM_CANCEL);
                        }
                    }

                    //Add 2018/06/22 arc yano #3898
                    //-----------------------------
                    //���э폜
                    //-----------------------------
                    DeleteAAJournal(header);

                    // �����ς݂̎ԗ��͈�������
                    //CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
                    //if (order != null && order.ReservationStatus != null && order.ReservationStatus.Equals("1")) {
                    //    order.ReservationStatus = "0";
                    //    SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);
                       
                    //    if (salesCar != null && salesCar.CarStatus.Equals("003")) {
                    //        salesCar.CarStatus = "001";
                    //    }
                    //    order.SalesCarNumber = string.Empty;
                    //}
                    

                    //�����\����폜
                    List<ReceiptPlan> planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
                    foreach (var p in planList) {
                        p.DelFlag = "1";
                        p.LastUpdateDate = DateTime.Now;
                        p.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                    
                    //Add 2016/09/27 arc nakayama #3630
                    //�c�Ɖ���̓������т͍폜
                    List<Journal> DelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("012") || x.AccountType.Equals("013"))).ToList();
                    foreach (var d in DelJournal)
                    {
                        d.DelFlag = "1";
                        d.LastUpdateDate = DateTime.Now;
                        d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }

                    db.SubmitChanges();

                    //�����ς݂�����΃A���[�g
                    List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber);
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    foreach (var j in journalList) {

                        // �ߓ����̃A���[�g���쐬
                        task.CarOverReceive(header, j.CustomerClaimCode, j.Amount);

                        //Add 2017/10/10 arc yano #3802
                        // ���ѕ��̓����\����쐬
                        ReceiptPlan plusPlan = new ReceiptPlan();
                        plusPlan.ReceiptPlanId = Guid.NewGuid();
                        plusPlan.DepartmentCode = j.DepartmentCode;
                        plusPlan.OccurredDepartmentCode = j.DepartmentCode;
                        plusPlan.CustomerClaimCode = j.CustomerClaimCode;
                        plusPlan.SlipNumber = j.SlipNumber;
                        plusPlan.ReceiptType = j.AccountType;
                        plusPlan.ReceiptPlanDate = DateTime.Today;
                        plusPlan.AccountCode = j.AccountCode;
                        plusPlan.Amount = j.Amount;
                        plusPlan.ReceivableBalance = 0;             //�c�� = 0(�Œ�)
                        plusPlan.CompleteFlag = "1";                //���������t���O = 0(�Œ�)
                        plusPlan.CreateDate = DateTime.Now;
                        plusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plusPlan.LastUpdateDate = DateTime.Now;
                        plusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plusPlan.DelFlag = "0";
                        plusPlan.JournalDate = DateTime.Today;
                        plusPlan.DepositFlag = "0";
                        db.ReceiptPlan.InsertOnSubmit(plusPlan);


                        // �ߓ������̃}�C�i�X�����\����쐬
                        ReceiptPlan minusPlan = new ReceiptPlan();
                        minusPlan.ReceiptPlanId = Guid.NewGuid();
                        minusPlan.DepartmentCode = j.DepartmentCode;
                        minusPlan.OccurredDepartmentCode = j.DepartmentCode;
                        minusPlan.CustomerClaimCode = j.CustomerClaimCode;
                        minusPlan.SlipNumber = j.SlipNumber;
                        minusPlan.ReceiptType = j.AccountType;
                        minusPlan.ReceiptPlanDate = DateTime.Today;
                        minusPlan.AccountCode = j.AccountCode;
                        minusPlan.Amount = -1 * j.Amount;
                        minusPlan.ReceivableBalance = -1 * j.Amount;
                        minusPlan.CompleteFlag = "0";
                        minusPlan.CreateDate = DateTime.Now;
                        minusPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        minusPlan.LastUpdateDate = DateTime.Now;
                        minusPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        minusPlan.DelFlag = "0";
                        minusPlan.JournalDate = DateTime.Today;
                        minusPlan.DepositFlag = "0";
                        db.ReceiptPlan.InsertOnSubmit(minusPlan);
                    }

                    // �L�����Z�������X�V
                    header.CancelDate = DateTime.Today;
                    header.LastEditScreen = "000";

                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    // �󒍈ȍ~�́u007:�󒍌�L�����Z���v�ɍX�V
                    if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
                    {
                        header.SalesOrderStatus = "006";
                    } else {
                        header.SalesOrderStatus = "007";

                        // �L�����Z�����[���𑗐M
                        ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("CancelAlertMailAddress");
                        if (config != null) {
                            SendMail mail = new SendMail();
                            Department department = new DepartmentDao(db).GetByKey(header.DepartmentCode);
                            Employee employee = new EmployeeDao(db).GetByKey(header.EmployeeCode);
                            c_NewUsedType newUsedType = new CodeDao(db).GetNewUsedTypeByKey(header.NewUsedType);

                            string customerName = "";
                            // Add 2014/08/06 arc amii �G���[���O�Ή� catch���Exception��ǉ�
                            try
                            {
                                customerName = new CustomerDao(db).GetByKey(header.CustomerCode).CustomerName;
                            }
                            catch (NullReferenceException ne)
                            {
                                // �G���[�ɂȂ��Ă��������s������B�G���[�̂ݏo��
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ���O�ɏo��
                                OutputLogger.NLogError(ne, PROC_NAME_SAVE+"�̌ڋq���擾", FORM_NAME, header.SlipNumber);
                            }
                            catch (Exception e)
                            {
                                // �Z�b�V������SQL����o�^
                                Session["ExecSQL"] = OutputLogData.sqlText;
                                // ���O�ɏo��
                                OutputLogger.NLogFatal(e, PROC_NAME_SAVE + "�̌ڋq���擾", FORM_NAME, header.SlipNumber);
                                // �G���[�y�[�W�ɑJ��
                                return View("Error");
                            }
                            string title = "�ySYSTEM Information�z" + department.DepartmentName + "�󒍑���";
                            string msg = "���L�����Z��\r\n";
                            msg += "�󒍓� : " + string.Format("{0:yyyy/MM/dd}", header.SalesOrderDate) + "\r\n";
                            msg += "�`�[�ԍ� : " + header.SlipNumber + "\r\n";
                            msg += "�ڋq�� : " + customerName + "\r\n";
                            msg += "�S���� : " + department.DepartmentName + ":" + employee.EmployeeName + "\r\n";
                            msg += "�Ԏ�@ : " + header.MakerName + header.CarName + header.CarGradeName + "\r\n";
                            msg += "�F�@�@ : " + header.ExteriorColorName + "/" + header.InteriorColorName + "\r\n";
                            msg += "�ԑ�No : " + header.Vin + "\r\n";
                            msg += "�V���� : " + newUsedType.Name;
                            mail.Send(title, config.Value, msg);
                        }

                        //Add 2016/09/05 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                        //�󒍌�L�����Z�����s��ꂽ�Ƃ��A����Ԃ̍���f�[�^�Ǝd���f�[�^���������݂����ꍇ�A�������Ȃ��A����f�[�^�݂̂̏ꍇ�A�폜
                        for (int i = 1; i <= 3; i++)
                        {

                            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                            {
                                CarAppraisal CarAppraisalData = new CarAppraisalDao(db).GetBySlipNumberVin(header.SlipNumber, vin.ToString());
                                if (CarAppraisalData != null)
                                {
                                    CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(header.SlipNumber, vin.ToString());

                                    if (CarPurchaseData == null)
                                    {
                                        CarAppraisalData.DelFlag = "1";
                                    }
                                    else
                                    {
                                        //�Ȃɂ����Ȃ��B�ԗ��̍���Ǝd���f�[�^�͓X�܂̕��ɔC����
                                    }
                                }
                            }
                        }
                        //����ԂƂ��̎c�Ɋւ�������\��f�[�^���폜����
                        List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, header.DepartmentCode).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
                        foreach (var d in delList)
                        {
                            //�c�Ɖ���̓����\��
                            d.DelFlag = "1";
                            d.LastUpdateDate = DateTime.Now;
                            d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        }

                    }

                    // �L�����Z���^�X�N���쐬����
                    task.CarCancel(header);


                } else {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    //�󒍈ȍ~�̏ꍇ�͓����\����č쐬
                    if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001")
                        //&& !CommonUtils.DefaultString(header.SlipNumber).Contains("-2")       //Mod 2017/12/22 arc yano #3793
                        )
                    {
                        //Add 2016/09/06 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                        //����ԂɊւ�������\����쐬����
                        CreateTradeReceiptPlan(header);

                        CreatePaymentPlan(header);

                        // ������č쐬(2011.02.18�ǉ�)
                        // �����̂��̂��폜���Ă���
                        CarAppraisalDao appraisalDao = new CarAppraisalDao(db);
                        for (int i = 1; i <= 3; i++)
                        {
                            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                            //bool editFlag = false;
                            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                            {

                                // �d���\��쐬�ς݂̍��肪�Ȃ���΍쐬������
                                List<CarAppraisal> appraisalList = appraisalDao.GetListBySlipNumberVin(header.SlipNumber, vin.ToString(), "1");
                                if (appraisalList == null || appraisalList.Count() == 0)
                                {

                                    // �폜���Ă���
                                    DeleteAppraisalData(header, i);

                                    // ��蒼��
                                    CreateAppraisalData(header, i, false);

                                    db.SubmitChanges();


                                }
                                //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
                                //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
                            }
                        }
                    }
                }
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                //�[�Ԏ��ł����`�[�o�Ȃ��ꍇ�͍݌ɃX�e�[�^�X�X�V(�[�ԍς�)
                if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
                {
                    SalesCar salesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                    salesCar.CarStatus = "006";    //�[�ԍς�

                    //Mod 2018/08/14 yano #3910 //���p�p�r����(���Œ莑�Y)�̏ꍇ
                    if (string.IsNullOrWhiteSpace(salesCar.CarUsage))
                    {
                        salesCar.Location = null;  //���P�[�V�����X�V
                    }

                    //Mod 2019/01/09 yano #4030 �ԍ��`�[�̔[�Ԏ��ɂ͎ԗ��}�X�^�̔[�ԓ��͍X�V���Ȃ�
                    if (!CommonUtils.DefaultString(header.SlipNumber).Contains("-2"))
                    {
                        salesCar.SalesDate = header.SalesDate; // �[�ԓ��X�V
                    }

                    salesCar.LastUpdateDate = DateTime.Now;
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                    // �ڋq��ʂ̍X�V
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode);
                    if (string.IsNullOrEmpty(customer.CustomerKind) || !customer.CustomerKind.Equals("002")) {
                        customer.CustomerKind = "002";
                    }
                    //Del 2017/12/22 arc yano #3793
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    //if (CommonUtils.DefaultString(header.SlipNumber).Contains("-2")) {
                    //    ResetReceiptPlan(header);
                    //}
                }

                if (header.ActionType.Equals("ModificationEnd"))
                {
                    //�C������ǉ�
                    CreateModifiedHistory(header);
                }

                //Mod 2018/11/07 yano #3939
                //�f�[�^�C���T�[�g
                //db.CarSalesHeader.InsertOnSubmit(header);

                //Add 2023/09/05 #4162
                InsertInvoiceConsumptionTax(header);
              
                try {
                    db.SubmitChanges();
                    //�R�~�b�g
                    ts.Complete();
                } catch (SqlException se) {
                    //Add 2014/08/06 arc amii �G���[���O�Ή� SQL�����Z�b�V�����ɓo�^���鏈���ǉ�
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                       //Add 2014/08/06 arc amii �G���[���O�Ή� �G���[���O�o�͏����ǉ�
                        OutputLogger.NLogError(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    } else {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_SAVE, FORM_NAME, header.SlipNumber);
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            //���܂��Ȃ�
            ModelState.Clear();

            //Add2018/12/21 yano #3965
            if (Session["ConnectDB"] != null && Session["ConnectDB"].Equals("WE_DB"))   //WE�ł̏ꍇ
            {
                if (header.SalesOrderStatus.Equals("004") && !string.IsNullOrWhiteSpace(header.ActionType) && header.ActionType.Equals("CarDeliveryReport"))
                {
                    ModelState.AddModelError("", "�{�V�X�e���ł͔[�Ԋm�F�����o�͂��܂���B�[�Ԋm�F���͎w��̂��̂����g�p�������B");
                    header.ActionType = "";
                }
            }

            
            //�\�����ڂ̍ăZ�b�g
            SetDataComponent(ref header);

            //���[���
            if(!string.IsNullOrEmpty(form["PrintReport"])){
                ViewData["close"] = "";
                ViewData["reportName"] = form["PrintReport"];
            }else{
                if (form["requestType"].Equals("requestService")) {
                    ViewData["url"] = "/ServiceRequest/Entry?SlipNo=" + header.SlipNumber;
                }
                //ViewData["close"] = "1";
            }

            //Add 2016/12/08 arc nakayama #3674_�x�����z�ƌ����̔����v�̍����`�F�b�N���󒍈ȍ~�ɕύX����
            //Mod 2016/12/13 arc nakayama #3678_�ԗ��`�[�@�x�������v�ƌ����̔����v�̃o���f�[�V�����ŁA�x�������v�Ƀ��[���萔�����܂܂�Ă���

            if (string.IsNullOrEmpty(header.PaymentPlanType))
            {
                if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && header.SalesOrderStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "�ۑ��͊������܂������A���x�������v�ƌ����̔����v����v���Ă��܂���B");
                    ModelState.AddModelError("", "�󒍈ȍ~�͑��x�������v�ƌ����̔����v�ƈ�v����悤�ɂ��ĉ������B");
                }
            }
            else
            {
                if (header.GrandTotalAmount != header.LoanPrincipalAmount + header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && header.SalesOrderStatus.Equals("001"))
                {
                    ModelState.AddModelError("", "�ۑ��͊������܂������A���x�������v�ƌ����̔����v����v���Ă��܂���B");
                    ModelState.AddModelError("", "�󒍈ȍ~�͑��x�������v�ƌ����̔����v�ƈ�v����悤�ɂ��ĉ������B");
                }
            }

            //�󒍂̎��Ƀ`�F�b�N����
            if (header.SalesOrderStatus.Equals("002"))
            {
                List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();

                //��������-������ꗗ�쐬�p
                List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

                bool Mflag = false;
                foreach (string JClaim in JournalClaimList)
                {
                    var Ret = from a in payList
                              where a.CustomerClaimCode.Equals(JClaim)
                              select a;

                    if (Ret.Count() == 0)
                    {
                        Mflag = true;
                        break;
                    }
                }

                if (Mflag)
                {
                    ModelState.AddModelError("", "�����͊������܂������A�x����񗓂ɐݒ肳��Ă��Ȃ������悩��������т�����܂��B");
                }
            }

            //�o��
            return GetViewResult(header);
        }
        #endregion

        #region �ԍ��Ώی���
        /// <summary>
        /// �ԍ�������ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult ModifyCriteria() {
            criteriaInit = true;
            return ModifyCriteria(new FormCollection());
        }
        /// <summary>
        /// �ԍ���������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ModifyCriteria(FormCollection form) {
            form["DelFlag"] = "0";
            form["SalesOrderStatus"] = "005";
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
            form["AkaKuro"] = "1";
            PaginatedList<CarSalesHeader> list;
            if (criteriaInit) {
                list = new PaginatedList<CarSalesHeader>();
            } else {
                list = GetSearchResultList(form);
            }
            foreach (var item in list) {
                // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(item.DepartmentCode, item.SalesDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                {
                    // �[�ԓ����I�����ߖ�������������ԍ��ΏۊO
                    item.IsClosed = true;
                }
                if (new CarSalesOrderDao(db).GetBySlipNumber(item.SlipNumber + "-1") != null) {
                    // �ԓ`�����A�ԍ��������Ă��Ȃ�������Ώ�
                    item.IsCreated = true;
                }
            }
            return View("CarSalesOrderModifyCriteria", list);
        }
        #endregion

        #region �ԓ`

        /// <summary>
        /// �ԓ`����
        /// </summary>
        /// <param name="header"></param>
        /// <param name="line"></param>
        /// <param name="pay"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/22 arc yano  #3898 �ԗ��}�X�^�@AA�̔��Ŕ[�Ԍ�L�����Z���ƂȂ����ꍇ�̍݌ɃX�e�[�^�X�ɂ���
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akaden(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            //Add 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }

            //Add 2014/05/16 yano vs2012�ڍs���Ή� �x���s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
            

            // �ԓ`�����E�ԍ��������͗��R���K�{
            ViewData["Mode"] = form["Mode"];


            //Add 2017/11/10 arc yano #3787
            //���̃��[�U�ɓ`�[�����b�N����Ă��邩���`�F�b�N����B
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            if (string.IsNullOrEmpty(header.Reason)) {
                if (form["Mode"].Equals("1")) {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�ԓ`����", "���R" }));
                } else {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�ԍ�����", "���R" }));
                }
            }
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "���R��1024�����ȓ��œ��͂��ĉ�����");
            }
            CarSalesHeader target = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            if (target != null) {
                ModelState.AddModelError("SlipNumber", "���łɏ�������Ă��܂�");
            }

            
            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            if (ModelState.IsValid) {


                CarSalesHeader history2 = new CarSalesHeader();
                double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

                // �ԓ`����

                using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
                {

                    //Add 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                    List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
                    foreach (var d in delList)
                    {
                        //�c�Ɖ���̓����\��
                        d.DelFlag = "1";
                        d.LastUpdateDate = DateTime.Now;
                        d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }

                    //Add 2018/06/22 arc yano #3898
                    //------------------------------
                    //AA���E�̓������т̍폜
                    //------------------------------
                    DeleteAAJournal(header);


                    //Add 2018/06/22 arc yano  #3898
                    //----------------------------
                    //�ԗ�������������
                    //----------------------------
                    Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);

                    string customerType = customer != null ? customer.CustomerType : "";

                    //Mod 2018/08/07 yano #3911
                    //�󒍌�L�����Z���̏ꍇ�̔��敪��AA�̔��A�Ɣ́A�˔p�̏ꍇ�݈̂���������
                    if (
                          header.SalesType.Equals("003") ||     //�̔��敪=�u�Ɣ́v
                          header.SalesType.Equals("009") ||     //�̔��敪=�u�X�Ԉړ��v
                         (header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) ||  //�̔��敪=�uAA�v���ڋq�敪=�uAA�v
                         (header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))     //�̔��敪=�u�˔p�v���ڋq�敪=�u�p���v
                        )
                    {
                        ReleaseProvision(header, PTN_CANCEL_ALL, CANCEL_FROM_AKADEN);  //�S�ĉ���
                    }
                    else
                    {
                        //������������
                        ReleaseProvision(header, PTN_CANCEL_REGISTRATION, CANCEL_FROM_AKADEN); //�o�^�E�����̂݉���(�����͉������Ȃ�)

                    }

                    db.SubmitChanges();

                    CarSalesHeader history = CreateAkaden(header, false);

                    db.SubmitChanges();

                    //Add 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                    //�ԓ`�ƌ��`�[�̍������Ƃ��ĕԋ��̓����\����쐬����
                    db.CreateBackAmountAkaden(header.SlipNumber, ((Employee)Session["Employee"]).EmployeeCode);
                    CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "�ԗ�", header.Reason);

                    // Add 2014/08/06 arc amii �G���[���O�Ή� try catch���ƃG���[���O������ǉ�
                    try
                    {
                        db.SubmitChanges();
                        ts.Complete();
                        history2 = history;
                    }
                    catch (SqlException se)
                    {
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�o��
                            OutputLogger.NLogError(se, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                            header.SalesOrderStatus = form["PreviousStatus"];
                            SetDataComponent(ref header);
                            return GetViewResult(header);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME_AKA, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }

                    ViewData["CompleteMessage"] = "�`�[�ԍ��u" + header.SlipNumber + "�i���`�[�j�v�̐ԓ`�u" + history.SlipNumber + "�i�V�ԍ��j�v����������Ɋ������܂����B";
                    ViewData["Mode"] = null;
                    ModelState.Clear();
                }
                //�\�����ڂ̍ăZ�b�g
                SetDataComponent(ref history2);
                return GetViewResult(history2);
            }

            //�\�����ڂ̍ăZ�b�g
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
        /// 2018/11/09 yano #3945 �ԗ��`�[�@�ԍ����������`�[�����ςɖ߂��A�󒍂ɐi�߂Ă��u�o�^�ς֐i�߂�v�{�^�����\������Ȃ�
        /// 2018/11/09 yano #3938 �ԗ��`�[���́@�ԍ��������̉���Ԏd���f�[�^�̒������ԍ��̍X�V�R��Ή�
        /// 2018/08/14 yano #3910 �ԗ��`�[�@�f���J�[�̔[�ԍϓ`�[�C�����Ɏԗ��}�X�^�̃��P�[�V������������
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Akakuro(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form) {

            // Mod 2014/11/14 arc yano #3129 �ԗ��ړ��o�^�s� �ގ��Ή� �f�[�^�R���e�L�X�g�����������A�N�V�������U���g�̍ŏ��Ɉړ�
            // Add 2014/08/06 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();
            
            //Add 2014/05/16 yano vs2012�ڍs���Ή� �I�v�V�����s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (line == null)
            {
                header.CarSalesLine = getCarSalesLinebyReq();    //post�f�[�^���擾�����I�v�V�����s�̃f�[�^���Z�b�g
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesLine = line;
            }


            //Add 2014/05/16 yano vs2012�ڍs���Ή� �x���s�̃o�C���h�Ɏ��s���́APOST�f�[�^����l���擾����B
            if (pay == null)
            {
                header.CarSalesPayment = getCarSalesPaymentbyReq();
            }
            else  //line��null�łȂ��ꍇ
            {
                header.CarSalesPayment = pay;
            }  
            //header.CarSalesLine = line;
            //header.CarSalesPayment = pay;
 
            // �ԓ`�����E�ԍ��������͗��R���K�{
            ViewData["Mode"] = form["Mode"];

            //Add 2017/11/10 arc yano #3787
            //���̃��[�U�ɓ`�[�����b�N����Ă��邩���`�F�b�N����B
            ValidateProcessLock(header, form);
            if (!ModelState.IsValid)
            {
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            if (string.IsNullOrEmpty(header.Reason)) {
                if (form["Mode"].Equals("1")) {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�ԓ`����", "���R" }));
                } else {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�ԍ�����", "���R" }));
                }
            } 
            if (header.Reason.Length > 1024) {
                ModelState.AddModelError("Reason", "���R��1024�����ȓ��œ��͂��ĉ�����");
            }
            CarSalesHeader target1 = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-1");
            CarSalesHeader target2 = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber + "-2");
            if (target1 != null || target2 != null) {
                ModelState.AddModelError("SlipNumber", "���łɏ�������Ă��܂�");
            }

            if (!ModelState.IsValid) {

                ProcessUnLock(header);  //Add 2017/11/10 arc yano #3787 

                //�\�����ڂ̍ăZ�b�g
                SetDataComponent(ref header);
                return GetViewResult(header);
            }

            double TimeOutMinutes = CommonUtils.GetTimeOutMinutes();

            // �ԍ��`����
            using (TransactionScope ts = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromMinutes(TimeOutMinutes)))
            {

                // �ԓ`����
                CommonUtils.InsertAkakuroReason(db, header.SlipNumber, "�ԗ�", header.Reason);

                CarSalesHeader history = CreateAkaden(header, true);

                //Add 2018/11/09 yano #3945
                string originalSlipNumber = header.SlipNumber;

                CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(originalSlipNumber);

                //�ԗ������˗��f�[�^�����݂����ꍇ�͓`�[�ԍ������`�[�ɐU��
                if (rec != null)
                {
                    rec.SlipNumber = originalSlipNumber + "-2";
                    rec.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    rec.LastUpdateDate = DateTime.Now;
                }

                // ������
                header.SlipNumber = header.SlipNumber + "-2";
                header.RevisionNumber = 1;
                header.SalesOrderStatus = "003";
                header.CreateDate = DateTime.Now;
                header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.LastUpdateDate = DateTime.Now;
                header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.DelFlag = "0";
                header.LastEditScreen = "000";

                foreach (var item in header.CarSalesLine)
                {
                    item.CreateDate = DateTime.Now;
                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.DelFlag = "0";
                    //MOD �ԓ`����ŗ��s��Ή� 2014/11/12 ookubo
                    item.ConsumptionTaxId = header.ConsumptionTaxId;
                    item.Rate = header.Rate;
                }
                foreach (var item in header.CarSalesPayment)
                {
                    item.CreateDate = DateTime.Now;
                    item.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.DelFlag = "0";
                }

                SalesCar salesCar = new SalesCarDao(db).GetByKey(header.SalesCarNumber);
                if (salesCar != null)
                {
                    //Mod 2018/08/14 yano #3910 �݌ɃX�e�[�^�X�́u�݌Ɂv�ł͂Ȃ��u�o�^�ρv
                    //salesCar.CarStatus = "001";
                    salesCar.CarStatus = "003";     //������
                    salesCar.LastUpdateDate = DateTime.Now;
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                    //Mod 2018/08/14 yano #3910
                    //�ԗ��}�X�^�̃��P�[�V�����R�[�h���󗓂̏ꍇ�A���偨�q�Ɂ����P�[�V�����̍ŏ���ݒ�
                    if (string.IsNullOrWhiteSpace(salesCar.LocationCode))
                    {
                        Department department = new DepartmentDao(db).GetByKey(header.DepartmentCode);

                        if (department != null)
                        {
                            DepartmentWarehouse ret = CommonUtils.GetWarehouseFromDepartment(db, department.DepartmentCode);

                            if(ret != null)
                            {
                                Location loc = new LocationDao(db).GetListByLocationType("001", ret.WarehouseCode, "").FirstOrDefault();

                                salesCar.LocationCode = loc != null ? loc.LocationCode : "";

                                ////Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                                //try { salesCar.LocationCode = department.Location.Where(x => CommonUtils.DefaultString(x.LocationType).Equals("001")).FirstOrDefault().LocationCode; }
                                //catch (NullReferenceException) { }
                            }
                            
                        }
                    }
                }

                //Add 2016/08/29 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                TransferJournal(header.SlipNumber);

                //Add 2016/08/29 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                TransfeTraderJournal(header);

                //�c�Ɖ���̓����\��쐬
                CreateTradeReceiptPlan(header);

                // �����\��쐬
                CreatePaymentPlan(header);


                //Add 2018/11/09 yano #3938
                List<CarPurchase> purchaseList = new CarPurchaseDao(db).GetListBySlipNumberVin(originalSlipNumber, "");

                if (purchaseList != null && purchaseList.Count > 0)
                {
                    foreach (var a in purchaseList)
                    {
                        a.SlipNumber = header.SlipNumber;
                        a.LastUpdateEmployeeCode =  ((Employee)Session["Employee"]).EmployeeCode;
                        a.LastUpdateDate = DateTime.Now;
                    }
                }

                //Add 2016/08/29 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
                //�J�[�h��Ђ���̓����\������ɐU��ւ���
                CreateKuroCreditPlan(header.SlipNumber);

                db.CarSalesHeader.InsertOnSubmit(header);

                // Add 2014/08/06 arc amii �G���[���O�Ή� try catch���ƃG���[���O������ǉ�
                try
                {
                    db.SubmitChanges();
                    ts.Complete();
                }
                catch (SqlException se)
                {
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�o��
                        OutputLogger.NLogError(se, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�Y����"));
                        header.SalesOrderStatus = form["PreviousStatus"];
                        SetDataComponent(ref header);
                        return GetViewResult(header);
                    }
                    else
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(se, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_AKA_KURO, FORM_NAME, header.SlipNumber);
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }

                ViewData["CompleteMessage"] = "�`�[�ԍ��u" + form["SlipNumber"] + "�i���`�[�j�v�̐ԍ��u" + header.SlipNumber + "�i�[�ԍςݓ`�[�̃R�s�[�j�v����������Ɋ������܂����B";
            }

            ProcessUnLock(header);  //Add 2017/11/10 arc yano #3787 

            //�\�����ڂ̍ăZ�b�g
            SetDataComponent(ref header);

            ModelState.Clear();
            
            ViewData["Mode"] = null;

            return GetViewResult(header);
        }

    /// <summary>
    /// �}�C�i�X�̓`�[���쐬����
    /// </summary>
    /// <param name="header">���`�[</param>
    /// <returns>�V�`�[</returns>
    /// <history>
    /// 2023/09/28 yano #4183 �C���{�C�X�Ή�(�o���Ή�)
    /// 2023/09/18 yano #4181�y�ԗ��`�[�z�I�v�V�����敪�ǉ��i�T�[�`���[�W�j
    /// 2023/08/15 yano #4176 �̔�����p�̏C��
    /// 2022/06/23 yano #4140�y�ԗ��`�[���́z�������̓o�^���`�l���\������Ȃ��s��̑Ή�
    /// 2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�ǉ�(�����e�i�X�E�����ۏ�)
    /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
    /// 2017/12/27 arc yano #3820 �ԗ��`�[�|�J�[�h��Ђ���̓����\��A�������т̂���ԗ��`�[�̐ԓ`�����s�
    /// </history>
    private CarSalesHeader CreateAkaden(CarSalesHeader header, bool Akakuro) {
            // �ԓ`����
            CarSalesHeader history = new CarSalesHeader();
            history.SlipNumber = header.SlipNumber + "-1";
            history.RevisionNumber = 1;
            history.QuoteDate = header.QuoteDate;
            history.QuoteExpireDate = header.QuoteExpireDate;
            history.SalesOrderDate = header.SalesOrderDate;
            history.SalesOrderStatus = "005"; // �X�e�[�^�X�͔[�ԍς�
            history.ApprovalFlag = header.ApprovalFlag;
            history.SalesDate = DateTime.Today; // �[�ԓ����V�X�e�����t
            history.CustomerCode = header.CustomerCode;
            history.DepartmentCode = header.DepartmentCode;
            history.EmployeeCode = header.EmployeeCode;
            history.CampaignCode1 = header.CampaignCode1;
            history.CampaignCode2 = header.CampaignCode2;
            history.NewUsedType = header.NewUsedType;
            history.SalesType = header.SalesType;
            history.MakerName = header.MakerName;
            history.CarBrandName = header.CarBrandName;
            history.CarName = header.CarName;
            history.CarGradeName = header.CarGradeName;
            history.CarGradeCode = header.CarGradeCode;
            history.ManufacturingYear = header.ManufacturingYear;
            history.ExteriorColorCode = header.ExteriorColorCode;
            history.ExteriorColorName = header.ExteriorColorName;
            history.Vin = header.Vin;
            history.UsVin = header.UsVin;
            history.ModelName = header.ModelName;
            history.Mileage = header.Mileage;
            history.MileageUnit = header.MileageUnit;
            history.RequestPlateNumber = header.RequestPlateNumber;
            history.RegistPlanDate = header.RegistPlanDate;
            history.HotStatus = header.HotStatus;
            history.SalesCarNumber = header.SalesCarNumber;
            history.RequestRegistDate = header.RequestRegistDate;
            history.SalesPlanDate = header.SalesPlanDate;
            history.RegistrationType = header.RegistrationType;
            history.MorterViecleOfficialCode = header.MorterViecleOfficialCode;
            history.OwnershipReservation = header.OwnershipReservation;
            history.CarLiabilityInsuranceType = header.CarLiabilityInsuranceType;
            history.SealSubmitDate = header.SealSubmitDate;
            history.ProxySubmitDate = header.ProxySubmitDate;
            history.ParkingSpaceSubmitDate = header.ParkingSpaceSubmitDate;
            history.CarLiabilityInsuranceSubmitDate = header.CarLiabilityInsuranceSubmitDate;
            history.OwnershipReservationSubmitDate = header.OwnershipReservationSubmitDate;
            history.Memo = header.Memo;
            history.SalesPrice = header.SalesPrice * (-1);
            history.DiscountAmount = header.DiscountAmount * (-1);
            history.TaxationAmount = header.TaxationAmount * (-1);
            history.TaxAmount = header.TaxAmount * (-1);
            history.ShopOptionAmount = header.ShopOptionAmount * (-1);
            history.ShopOptionTaxAmount = header.ShopOptionTaxAmount * (-1);
            history.MakerOptionAmount = header.MakerOptionAmount * (-1);
            history.MakerOptionTaxAmount = header.MakerOptionTaxAmount * (-1);
            history.OutSourceAmount = header.OutSourceAmount * (-1);
            history.OutSourceTaxAmount = header.OutSourceTaxAmount * (-1);
            history.SubTotalAmount = header.SubTotalAmount * (-1);
            history.CarTax = header.CarTax * (-1);
            history.CarLiabilityInsurance = header.CarLiabilityInsurance * (-1);
            history.CarWeightTax = header.CarWeightTax * (-1);
            history.AcquisitionTax = header.AcquisitionTax * (-1);
            history.InspectionRegistCost = header.InspectionRegistCost * (-1);
            history.ParkingSpaceCost = header.ParkingSpaceCost * (-1);
            history.TradeInCost = header.TradeInCost * (-1);
            history.RecycleDeposit = header.RecycleDeposit * (-1);
            history.RecycleDepositTradeIn = header.RecycleDepositTradeIn * (-1);
            history.NumberPlateCost = header.NumberPlateCost * (-1);
            history.RequestNumberCost = header.RequestNumberCost * (-1);
            history.TradeInFiscalStampCost = header.TradeInFiscalStampCost * (-1);
            // Add 2014/07/25 �ۑ�Ή� #3018 �����󎆑�Ɖ��掩���Ԑŗa����ǉ�
            history.RevenueStampCost = header.RevenueStampCost * (-1);
            history.TradeInCarTaxDeposit = header.TradeInCarTaxDeposit * (-1);
            history.TaxFreeFieldName = header.TaxFreeFieldName;
            history.TaxFreeFieldValue = header.TaxFreeFieldValue * (-1);
            history.TaxFreeTotalAmount = header.TaxFreeTotalAmount * (-1);
            history.InspectionRegistFee = header.InspectionRegistFee * (-1);
            history.ParkingSpaceFee = header.ParkingSpaceFee * (-1);
            history.TradeInFee = header.TradeInFee * (-1);
            history.PreparationFee = header.PreparationFee * (-1);
            history.RecycleControlFee = header.RecycleControlFee * (-1);
            history.RecycleControlFeeTradeIn = header.RecycleControlFeeTradeIn * (-1);
            history.RequestNumberFee = header.RequestNumberFee * (-1);
            history.CarTaxUnexpiredAmount = header.CarTaxUnexpiredAmount * (-1);
            history.CarLiabilityInsuranceUnexpiredAmount = header.CarLiabilityInsuranceUnexpiredAmount * (-1);
            history.TradeInAppraisalFee = header.TradeInAppraisalFee * (-1);
            history.FarRegistFee = header.FarRegistFee * (-1);
            history.OutJurisdictionRegistFee = header.OutJurisdictionRegistFee * (-1);    //Add 2023/08/15 yano #4176
            history.TradeInMaintenanceFee = header.TradeInMaintenanceFee * (-1);
            history.InheritedInsuranceFee = header.InheritedInsuranceFee * (-1);
            history.TaxationFieldName = header.TaxationFieldName;
            history.TaxationFieldValue = header.TaxationFieldValue * (-1);
            history.SalesCostTotalAmount = header.SalesCostTotalAmount * (-1);
            history.SalesCostTotalTaxAmount = header.SalesCostTotalTaxAmount * (-1);
            history.OtherCostTotalAmount = header.OtherCostTotalAmount * (-1);
            history.CostTotalAmount = header.CostTotalAmount * (-1);
            history.TotalTaxAmount = header.TotalTaxAmount * (-1);
            history.GrandTotalAmount = header.GrandTotalAmount * (-1);
            history.PossesorCode = header.PossesorCode;
            history.UserCode = header.UserCode;
            history.PrincipalPlace = header.PrincipalPlace;
            history.VoluntaryInsuranceType = header.VoluntaryInsuranceType;
            history.VoluntaryInsuranceCompanyName = header.VoluntaryInsuranceCompanyName;
            history.VoluntaryInsuranceAmount = header.VoluntaryInsuranceAmount * (-1);
            history.VoluntaryInsuranceTermFrom = header.VoluntaryInsuranceTermFrom;
            history.VoluntaryInsuranceTermTo = header.VoluntaryInsuranceTermTo;
            history.PaymentPlanType = header.PaymentPlanType;
            history.TradeInAmount1 = header.TradeInAmount1 * (-1);
            history.TradeInTax1 = header.TradeInTax1 * (-1);
            history.TradeInUnexpiredCarTax1 = header.TradeInUnexpiredCarTax1 * (-1);
            history.TradeInRemainDebt1 = header.TradeInRemainDebt1 * (-1);
            history.TradeInAppropriation1 = header.TradeInAppropriation1 * (-1);
            history.TradeInRecycleAmount1 = header.TradeInRecycleAmount1 * (-1);
            history.TradeInMakerName1 = header.TradeInMakerName1;
            history.TradeInCarName1 = header.TradeInCarName1;
            history.TradeInClassificationTypeNumber1 = header.TradeInClassificationTypeNumber1;
            history.TradeInModelSpecificateNumber1 = header.TradeInModelSpecificateNumber1;
            history.TradeInManufacturingYear1 = header.TradeInManufacturingYear1;
            history.TradeInInspectionExpiredDate1 = header.TradeInInspectionExpiredDate1;
            history.TradeInMileage1 = header.TradeInMileage1;
            history.TradeInMileageUnit1 = header.TradeInMileageUnit1;
            history.TradeInVin1 = header.TradeInVin1;
            history.TradeInRegistrationNumber1 = header.TradeInRegistrationNumber1;
            history.TradeInUnexpiredLiabilityInsurance1 = header.TradeInUnexpiredLiabilityInsurance1 * (-1);
            history.TradeInAmount2 = header.TradeInAmount2 * (-1);
            history.TradeInTax2 = header.TradeInTax2 * (-1);
            history.TradeInUnexpiredCarTax2 = header.TradeInUnexpiredCarTax2 * (-1);
            history.TradeInRemainDebt2 = header.TradeInRemainDebt2 * (-1);
            history.TradeInAppropriation2 = header.TradeInAppropriation2 * (-1);
            history.TradeInRecycleAmount2 = header.TradeInRecycleAmount2 * (-1);
            history.TradeInMakerName2 = header.TradeInMakerName2;
            history.TradeInCarName2 = header.TradeInCarName2;
            history.TradeInClassificationTypeNumber2 = header.TradeInClassificationTypeNumber2;
            history.TradeInModelSpecificateNumber2 = header.TradeInModelSpecificateNumber2;
            history.TradeInManufacturingYear2 = header.TradeInManufacturingYear2;
            history.TradeInInspectionExpiredDate2 = header.TradeInInspectionExpiredDate2;
            history.TradeInMileage2 = header.TradeInMileage2;
            history.TradeInMileageUnit2 = header.TradeInMileageUnit2;
            history.TradeInVin2 = header.TradeInVin2;
            history.TradeInRegistrationNumber2 = header.TradeInRegistrationNumber2;
            history.TradeInUnexpiredLiabilityInsurance2 = header.TradeInUnexpiredLiabilityInsurance2 * (-1);
            history.TradeInAmount3 = header.TradeInAmount3 * (-1);
            history.TradeInTax3 = header.TradeInTax3 * (-1);
            history.TradeInUnexpiredCarTax3 = header.TradeInUnexpiredCarTax3 * (-1);
            history.TradeInRemainDebt3 = header.TradeInRemainDebt3 * (-1);
            history.TradeInAppropriation3 = header.TradeInAppropriation3 * (-1);
            history.TradeInRecycleAmount3 = header.TradeInRecycleAmount3 * (-1);
            history.TradeInMakerName3 = header.TradeInMakerName3;
            history.TradeInCarName3 = header.TradeInCarName3;
            history.TradeInClassificationTypeNumber3 = header.TradeInClassificationTypeNumber3;
            history.TradeInModelSpecificateNumber3 = header.TradeInModelSpecificateNumber3;
            history.TradeInManufacturingYear3 = header.TradeInManufacturingYear3;
            history.TradeInInspectionExpiredDate3 = header.TradeInInspectionExpiredDate3;
            history.TradeInMileage3 = header.TradeInMileage3;
            history.TradeInMileageUnit3 = header.TradeInMileageUnit3;
            history.TradeInVin3 = header.TradeInVin3;
            history.TradeInRegistrationNumber3 = header.TradeInRegistrationNumber3;
            history.TradeInUnexpiredLiabilityInsurance3 = header.TradeInUnexpiredLiabilityInsurance3 * (-1);
            history.TradeInTotalAmount = header.TradeInTotalAmount * (-1);
            history.TradeInTaxTotalAmount = header.TradeInTaxTotalAmount * (-1);
            history.TradeInUnexpiredCarTaxTotalAmount = header.TradeInUnexpiredCarTaxTotalAmount * (-1);
            history.TradeInRemainDebtTotalAmount = header.TradeInRemainDebtTotalAmount * (-1);
            history.TradeInAppropriationTotalAmount = header.TradeInAppropriationTotalAmount * (-1);
            history.PaymentTotalAmount = header.PaymentTotalAmount * (-1);
            history.PaymentCashTotalAmount = header.PaymentCashTotalAmount * (-1);
            history.LoanPrincipalAmount = header.LoanPrincipalAmount * (-1);
            history.LoanFeeAmount = header.LoanFeeAmount * (-1);
            history.LoanTotalAmount = header.LoanTotalAmount * (-1);
            history.LoanCodeA = header.LoanCodeA;
            history.PaymentFrequencyA = header.PaymentFrequencyA;
            //Add 20170/02/02 arc nakayama #3489_�ԗ��`�[�̎����Ԓ����\�����̃��[���̂Q��ڈȍ~�̉񐔂̕\�L
            history.PaymentSecondFrequencyA = header.PaymentSecondFrequencyA;
            history.PaymentTermFromA = header.PaymentTermFromA;
            history.PaymentTermToA = header.PaymentTermToA;
            history.BonusMonthA1 = header.BonusMonthA1;
            history.BonusMonthA2 = header.BonusMonthA2;
            history.FirstAmountA = header.FirstAmountA * (-1);
            history.SecondAmountA = header.SecondAmountA * (-1);
            history.BonusAmountA = header.BonusAmountA * (-1);
            history.CashAmountA = header.CashAmountA * (-1);
            history.LoanPrincipalA = header.LoanPrincipalA * (-1);
            history.LoanFeeA = header.LoanFeeA * (-1);
            history.LoanTotalAmountA = header.LoanTotalAmountA * (-1);
            history.AuthorizationNumberA = header.AuthorizationNumberA;
            history.FirstDirectDebitDateA = header.FirstDirectDebitDateA;
            history.SecondDirectDebitDateA = header.SecondDirectDebitDateA;
            history.LoanCodeB = header.LoanCodeB;
            history.PaymentFrequencyB = header.PaymentFrequencyB;
            //Add 20170/02/02 arc nakayama #3489_�ԗ��`�[�̎����Ԓ����\�����̃��[���̂Q��ڈȍ~�̉񐔂̕\�L
            history.PaymentSecondFrequencyB = header.PaymentSecondFrequencyB;
            history.PaymentTermFromB = header.PaymentTermFromB;
            history.PaymentTermToB = header.PaymentTermToB;
            history.BonusMonthB1 = header.BonusMonthB1;
            history.BonusMonthB2 = header.BonusMonthB2;
            history.FirstAmountB = header.FirstAmountB * (-1);
            history.SecondAmountB = header.SecondAmountB * (-1);
            history.BonusAmountB = header.BonusAmountB * (-1);
            history.CashAmountB = header.CashAmountB * (-1);
            history.LoanPrincipalB = header.LoanPrincipalB * (-1);
            history.LoanFeeB = header.LoanFeeB * (-1);
            history.LoanTotalAmountB = header.LoanTotalAmountB * (-1);
            history.AuthorizationNumberB = header.AuthorizationNumberB;
            history.FirstDirectDebitDateB = header.FirstDirectDebitDateB;
            history.SecondDirectDebitDateB = header.SecondDirectDebitDateB;
            history.LoanCodeC = header.LoanCodeC;
            history.PaymentFrequencyC = header.PaymentFrequencyC;
            //Add 20170/02/02 arc nakayama #3489_�ԗ��`�[�̎����Ԓ����\�����̃��[���̂Q��ڈȍ~�̉񐔂̕\�L
            history.PaymentSecondFrequencyC = header.PaymentSecondFrequencyC;       //Mod 2018/11/07 yano #3939�Ή����Ɍ������o�O�̏C��
            history.PaymentTermFromC = header.PaymentTermFromC;
            history.PaymentTermToC = header.PaymentTermToC;
            history.BonusMonthC1 = header.BonusMonthC1;
            history.BonusMonthC2 = header.BonusMonthC2;
            history.FirstAmountC = header.FirstAmountC;
            history.SecondAmountC = header.SecondAmountC * (-1);
            history.BonusAmountC = header.BonusAmountC * (-1);
            history.CashAmountC = header.CashAmountC * (-1);
            history.LoanPrincipalC = header.LoanPrincipalC * (-1);
            history.LoanFeeC = header.LoanFeeC * (-1);
            history.LoanTotalAmountC = header.LoanTotalAmountC * (-1);
            history.AuthorizationNumberC = header.AuthorizationNumberC;
            history.FirstDirectDebitDateC = header.FirstDirectDebitDateC;
            history.SecondDirectDebitDateC = header.SecondDirectDebitDateC;
            history.CancelDate = header.CancelDate;
            history.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            history.CreateDate = DateTime.Now;
            history.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            history.LastUpdateDate = DateTime.Now;
            history.DelFlag = "0";
            history.InspectionRegistFeeTax = header.InspectionRegistFeeTax * (-1);
            history.ParkingSpaceFeeTax = header.ParkingSpaceFeeTax * (-1);
            history.TradeInFeeTax = header.TradeInFeeTax * (-1);
            history.PreparationFeeTax = header.PreparationFeeTax * (-1);
            history.RecycleControlFeeTax = header.RecycleControlFeeTax * (-1);
            history.RecycleControlFeeTradeInTax = header.RecycleControlFeeTradeInTax * (-1);
            history.RequestNumberFeeTax = header.RequestNumberFeeTax * (-1);
            history.CarTaxUnexpiredAmountTax = header.CarTaxUnexpiredAmountTax * (-1);
            history.CarLiabilityInsuranceUnexpiredAmountTax = header.CarLiabilityInsuranceUnexpiredAmountTax * (-1);
            history.TradeInAppraisalFeeTax = header.TradeInAppraisalFeeTax * (-1);
            history.FarRegistFeeTax = header.FarRegistFeeTax * (-1);
            history.OutJurisdictionRegistFeeTax = header.OutJurisdictionRegistFeeTax * (-1);  //Add 2023/08/15 yano #4176
            history.TradeInMaintenanceFeeTax = header.TradeInMaintenanceFeeTax * (-1);
            history.InheritedInsuranceFeeTax = header.InheritedInsuranceFeeTax * (-1);
            history.TaxationFieldValueTax = header.TaxationFieldValueTax * (-1);
            history.TradeInEraseRegist1 = header.TradeInEraseRegist1;
            history.TradeInEraseRegist2 = header.TradeInEraseRegist2;
            history.TradeInEraseRegist3 = header.TradeInEraseRegist3;
            history.RemainAmountA = header.RemainAmountA * (-1);
            history.RemainAmountB = header.RemainAmountB * (-1);
            history.RemainAmountC = header.RemainAmountC * (-1);
            history.RemainFinalMonthA = header.RemainFinalMonthA;
            history.RemainFinalMonthB = header.RemainFinalMonthB;
            history.RemainFinalMonthC = header.RemainFinalMonthC;
            history.LoanRateA = header.LoanRateA;
            history.LoanRateB = header.LoanRateB;
            history.LoanRateC = header.LoanRateC;
            history.SalesTax = header.SalesTax * (-1);
            history.DiscountTax = header.DiscountTax * (-1);
            history.TradeInPrice1 = header.TradeInPrice1 * (-1);
            history.TradeInPrice2 = header.TradeInPrice2 * (-1);
            history.TradeInPrice3 = header.TradeInPrice3 * (-1);
            history.TradeInRecycleTotalAmount = header.TradeInRecycleTotalAmount * (-1);
            history.Reason = header.Reason;
            //ADD 2014/02/20 ookubo
            history.ConsumptionTaxId = header.ConsumptionTaxId;
            history.Rate = header.Rate;
            history.LastEditScreen = "000";

            history.EPDiscountTaxId = header.EPDiscountTaxId;   //Add 2019/09/04 yano #4011

            //Add 2021/06/09 yano #4091
            history.CostAreaCode = header.CostAreaCode;
            history.MaintenancePackageAmount = header.MaintenancePackageAmount * (-1);
            history.MaintenancePackageTaxAmount = header.MaintenancePackageTaxAmount * (-1);
            history.ExtendedWarrantyAmount = header.ExtendedWarrantyAmount * (-1);
            history.ExtendedWarrantyTaxAmount = header.ExtendedWarrantyTaxAmount * (-1);

            //Add 2022/06/23 yano #4140
            history.TradeInHolderName1 = header.TradeInHolderName1;
            history.TradeInHolderName2 = header.TradeInHolderName2;
            history.TradeInHolderName3 = header.TradeInHolderName3;

            //Add 2023/09/18 yano #4181
            history.SurchargeAmount = header.SurchargeAmount * (-1);
            history.SurchargeTaxAmount = header.SurchargeTaxAmount * (-1);

            history.SuspendTaxRecv = header.SuspendTaxRecv * (-1);    //Add 2023/09/28 yano #4183

      db.CarSalesHeader.InsertOnSubmit(history);

            // �I�v�V����
            foreach (var item in header.CarSalesLine) {
                CarSalesLine history_line = new CarSalesLine();
                history_line.SlipNumber = history.SlipNumber;
                history_line.RevisionNumber = 1;
                history_line.LineNumber = item.LineNumber;
                history_line.CarOptionCode = item.CarOptionCode;
                history_line.CarOptionName = item.CarOptionName;
                history_line.OptionType = item.OptionType;
                history_line.Amount = item.Amount * (-1);
                history_line.CreateDate = DateTime.Now;
                history_line.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_line.LastUpdateDate = DateTime.Now;
                history_line.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_line.DelFlag = "0";
                history_line.TaxAmount = item.TaxAmount * (-1);
                //ADD 2014/02/20 ookubo
                //MOD �ԓ`����ŗ��s��Ή� 2014/11/12 ookubo
                history_line.ConsumptionTaxId = history.ConsumptionTaxId;
                history_line.Rate = history.Rate;

                db.CarSalesLine.InsertOnSubmit(history_line);
            }

            // �x�����@
            foreach (var payment in header.CarSalesPayment) {
                CarSalesPayment history_pay = new CarSalesPayment();
                history_pay.SlipNumber = history.SlipNumber;
                history_pay.RevisionNumber = 1;
                history_pay.LineNumber = payment.LineNumber;
                history_pay.CustomerClaimCode = payment.CustomerClaimCode;
                history_pay.PaymentPlanDate = payment.PaymentPlanDate;
                history_pay.Amount = payment.Amount * (-1);
                history_pay.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_pay.CreateDate = DateTime.Now;
                history_pay.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                history_pay.LastUpdateDate = DateTime.Now;
                history_pay.DelFlag = "0";
                history_pay.Memo = payment.Memo;
                db.CarSalesPayment.InsertOnSubmit(history_pay);
            }

            // �֘A��������\��f�[�^

            List<ReceiptPlan> planList = new List<ReceiptPlan>();

            if (Akakuro)
            {
                //�ԍ������̏ꍇ�͑S�Ă̓����\��
                planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber);
            }
            else
            {
                //�Ԃ̏ꍇ�͉����Ǝc�ƃJ�[�h��Ђ���̓����\��������������\��
                planList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => (!x.ReceiptType.Equals("011") && !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013"))).ToList(); //Mod 2017/12/27 arc yano #3820
            }
            
            foreach (var item in planList) {
                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptPlanId = Guid.NewGuid();
                plan.DepartmentCode = item.DepartmentCode;
                plan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                plan.CustomerClaimCode = item.CustomerClaimCode;
                plan.SlipNumber = history.SlipNumber;
                plan.ReceiptType = item.ReceiptType;
                plan.ReceiptPlanDate = item.ReceiptPlanDate;
                plan.AccountCode = item.AccountCode;
                plan.Amount = item.Amount * (-1);
                plan.ReceivableBalance = item.Amount * (-1);
                plan.CompleteFlag = "1"; //�����Ǘ���ʂɕ\�����Ȃ���
                plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.CreateDate = DateTime.Now;
                plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                plan.LastUpdateDate = DateTime.Now;
                plan.DelFlag = item.DelFlag;
                plan.Summary = "�`�[�ԍ�" + header.SlipNumber + "�̐ԓ`������";
                plan.JournalDate = item.JournalDate;
                plan.DepositFlag = item.DepositFlag;
                plan.PaymentKindCode = item.PaymentKindCode;
                plan.CommissionRate = item.CommissionRate;
                plan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(plan);
            }

            //���`�[�̓����\��͓����ς݁iCompleteFlag = "1"�j�ɂ���@����ȏ㌳�`�[�ɑ΂��ē����ł��Ȃ��悤�ɂ��邽��
            List<ReceiptPlan> headerPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var item in headerPlanList)
            {
                item.CompleteFlag = "1";
                item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                item.LastUpdateDate = DateTime.Now;
            }

            //�ԍ������̐ԓ`�̏ꍇ�͉����Ǝc�̓����\��������ς݂ɂ���
            if (Akakuro)
            {
                List<ReceiptPlan> TradeplanList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber).Where(x => (x.ReceiptType.Equals("012") || x.ReceiptType.Equals("013"))).ToList();
                foreach (var item in TradeplanList)
                {
                    item.CompleteFlag = "1";
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    item.LastUpdateDate = DateTime.Now;
                }
            }

            //Add 2016/05/23 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@
            //������ʂ��u�J�[�h��Ђ���̓����v�ɂȂ��Ă�������\����}�C�i�X���̓����\����쐬����B
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var item in CreditPlanList)
            {
                ReceiptPlan Creditplan = new ReceiptPlan();
                Creditplan.ReceiptPlanId = Guid.NewGuid();
                Creditplan.DepartmentCode = item.DepartmentCode;
                Creditplan.OccurredDepartmentCode = item.OccurredDepartmentCode;
                Creditplan.CustomerClaimCode = item.CustomerClaimCode;
                Creditplan.SlipNumber = history.SlipNumber;
                Creditplan.ReceiptType = item.ReceiptType;
                Creditplan.ReceiptPlanDate = item.ReceiptPlanDate;
                Creditplan.AccountCode = item.AccountCode;
                Creditplan.Amount = item.Amount * (-1);
                Creditplan.ReceivableBalance = item.Amount * (-1);
                Creditplan.CompleteFlag = "1";//���������̑ΏۊO�ɂ��邽��
                Creditplan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                Creditplan.CreateDate = DateTime.Now;
                Creditplan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                Creditplan.LastUpdateDate = DateTime.Now;
                Creditplan.DelFlag = item.DelFlag;
                Creditplan.Summary = "�`�[�ԍ�" + header.SlipNumber + "�̐ԓ`������";
                Creditplan.JournalDate = item.JournalDate;
                Creditplan.DepositFlag = item.DepositFlag;
                Creditplan.PaymentKindCode = item.PaymentKindCode;
                Creditplan.CommissionRate = item.CommissionRate;
                Creditplan.CommissionAmount = item.CommissionAmount * (-1);
                db.ReceiptPlan.InsertOnSubmit(Creditplan);
            }
            //���`�[�̓����\��͓����ς݁iCompleteFlag = "1"�j�ɂ���@����ȏ㌳�`�[�ɑ΂��ē����ł��Ȃ��悤�ɂ��邽��
            List<ReceiptPlan> headerCreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "011");
            foreach (var CreditItem in headerCreditPlanList)
            {
                CreditItem.CompleteFlag = "1";
                CreditItem.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                CreditItem.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            return history;
        }
        /// <summary>
        /// ���`�[���̓����\��f�[�^�쐬����
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// Mod 2016/04/05 arc yano #3441 �J�[�h�����������}�C�i�X�̓����\�肪�ł��Ă��܂� �������ю擾���͐������� =�@�u�N���W�b�g�v�͏���
        /// </history>
        private void ResetReceiptPlan(CarSalesHeader header) {

            #region ���O����

            //�Ȗڂ��擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            //�����̓����\����폜
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            foreach (var d in delList) {
                d.DelFlag = "1";
            }
            #endregion

            //������ꗗ�쐬�p
            List<string> customerClaimList = new List<string>();

            //**�����̓����\���(��)�쐬����**
            //������A�����\����̏��ŕ��ёւ�
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y) {
                return
                    !x.CustomerClaimCode.Equals(y.CustomerClaimCode) ? x.CustomerClaimCode.CompareTo(y.CustomerClaimCode) : //�����揇
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //�����\�����
                    (0);
            });

            decimal akaAmount = 0m;
            for (int i = 0; i < payList.Count; i++) {

                // �����惊�X�g�ɒǉ�
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                // ����or�����悪�ς�����Ƃ��A�ԓ`�̓����\����z���擾����
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    akaAmount = new ReceiptPlanDao(db).GetAmountByCustomerClaim(CommonUtils.DefaultString(header.SlipNumber).Replace("-2", "-1"), payList[i].CustomerClaimCode);
                }

                // ���|�c
                decimal balanceAmount = 0m;

                if (payList[i].Amount >= Math.Abs(akaAmount)) {
                    // �\��z >= ���ъz
                    balanceAmount = ((payList[i].Amount ?? 0m) + akaAmount);
                    akaAmount = 0m;
                } else {
                    // �\��z < ���ъz
                    balanceAmount = 0m;
                    akaAmount = akaAmount + (payList[i].Amount ?? 0m);

                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
                    if (i == payList.Count() - 1 || 
                        (i < payList.Count() - 1 && 
                        !CommonUtils.DefaultString((payList[i].CustomerClaimCode)).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode)))) {
                        balanceAmount = akaAmount;
                    }
                }

                CreateReceiptPlan(payList[i], balanceAmount, carAccount.AccountCode);
            }

            //�����ς݂̐����悪����̓`�[����Ȃ��Ȃ��Ă�����̂�ʒm
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList);  //Mod 2016/04/05 arc yano #3441
            foreach (Journal a in journalList) {
                if (!string.IsNullOrEmpty(a.CustomerClaimCode) && customerClaimList.IndexOf(a.CustomerClaimCode) < 0) {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    //task.CarOverReceive(header, a.CustomerClaimCode, a.Amount);

                    // �}�C�i�X�œ����\��쐬
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.ReceiptType = "001"; // ����
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                }
            }

            //Dell 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P �ԍ����s���ɓ����\��͍폜���Ȃ�
        }
        #endregion

        #region �V�K�I�u�W�F�N�g�쐬
        /// <summary>
        /// �V�����`�[�f�[�^���쐬
        /// </summary>
        /// <param name="header">�`�[�f�[�^</param>
        private void CreateCarSalesOrder(CarSalesHeader header) {

            //�V�K�̎��͓`�[�ԍ����̔Ԃ���
            if (header.RevisionNumber == 0) {
                header.SlipNumber = (new SerialNumberDao(db)).GetNewSlipNumber();
                header.ApprovalFlag = "0";
                header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                header.CreateDate = DateTime.Now;
            }

            //�Â����r�W�����͍폜
            DateTime updateDate = DateTime.Now;
            string updateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            List<CarSalesHeader> delList = (new CarSalesOrderDao(db)).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber);
            foreach (var d in delList) {
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                if (!CommonUtils.DefaultString(d.DelFlag).Equals("1"))
                {
                    d.LastUpdateDate = updateDate;
                    d.LastUpdateEmployeeCode = updateEmployeeCode;
                    d.DelFlag = "1";
                }
                foreach (var l in d.CarSalesLine) {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (!CommonUtils.DefaultString(l.DelFlag).Equals("1")) {
                        l.LastUpdateDate = updateDate;
                        l.LastUpdateEmployeeCode = updateEmployeeCode;
                        l.DelFlag = "1";
                    }
                }
                foreach (var p in d.CarSalesPayment) {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (!CommonUtils.DefaultString(p.DelFlag).Equals("1")) {
                        p.LastUpdateDate = updateDate;
                        p.LastUpdateEmployeeCode = updateEmployeeCode;
                        p.DelFlag = "1";
                    }
                }
            }
            header.RevisionNumber++;
            header.CreateDate = DateTime.Now;
            header.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;
            header.DelFlag = "0";

            foreach (var l in header.CarSalesLine) {
                l.CreateDate = DateTime.Now;
                l.CreateEmployeeCode = header.CreateEmployeeCode;
                l.LastUpdateDate = DateTime.Now;
                l.LastUpdateEmployeeCode = updateEmployeeCode;
                l.DelFlag = "0";
            }
            foreach (var p in header.CarSalesPayment) {
                p.CreateDate = DateTime.Now;
                p.CreateEmployeeCode = header.CreateEmployeeCode;
                p.LastUpdateDate = DateTime.Now;
                p.LastUpdateEmployeeCode = updateEmployeeCode;
                p.DelFlag = "0";
            }
        }
        #endregion

        #region �����˗��f�[�^���쐬
        /// <summary>
        /// �����˗��f�[�^���쐬����
        /// </summary>
        /// <param name="header">�ԗ��`�[</param>
        private void CreatePurchaseOrderData(CarSalesHeader header)
        {
            CarPurchaseOrder purchaseOrder = new CarPurchaseOrder();
            purchaseOrder.CarPurchaseOrderNumber = new SerialNumberDao(db).GetNewCarPurchaseOrderNumber();
            purchaseOrder.SlipNumber = header.SlipNumber;
            purchaseOrder.CarSalesHeader = header;
            purchaseOrder.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));;
            purchaseOrder.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchaseOrder.LastUpdateDate = DateTime.Now;
            purchaseOrder.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            purchaseOrder.DelFlag = "0";
            db.CarPurchaseOrder.InsertOnSubmit(purchaseOrder);

            header.CarPurchaseOrder = purchaseOrder;

            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            //�������F�˗��^�X�N��ǉ�����
            task.CarPurchaseApproval(header);

            //�����˗��^�X�N��ǉ�����
            task.CarPurchaseOrderRequest(header);
        }
        #endregion

        #region ����f�[�^���쐬
        /// <summary>
        /// ����ԂR�䕪�̍���f�[�^���쐬����
        /// </summary>
        /// <param name="header">�ԗ��`�[</param>
        private void CreateAppraisalData(CarSalesHeader header, int i, bool OrderFlag)
        {
            TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
            if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
            {
                CarAppraisal appraisal = new CarAppraisal();
                appraisal.CarAppraisalId = Guid.NewGuid();
                appraisal.SlipNumber = header.SlipNumber;
                object makerName = CommonUtils.GetModelProperty(header, "TradeInMakerName" + i);
                appraisal.MakerName = makerName != null ? makerName.ToString() : "";
                object tradeInMileage = CommonUtils.GetModelProperty(header, "TradeInMileage" + i);
                appraisal.Mileage = tradeInMileage != null ? decimal.Parse(tradeInMileage.ToString()) : 0m;
                object tradeInMileageUnit = CommonUtils.GetModelProperty(header, "TradeInMileageUnit" + i);
                appraisal.MileageUnit = tradeInMileageUnit != null ? tradeInMileageUnit.ToString() : "";
                object tradeInInspectionExpiredDate = CommonUtils.GetModelProperty(header, "TradeInInspectionExpiredDate" + i);
                appraisal.InspectionExpireDate = tradeInInspectionExpiredDate != null ? CommonUtils.StrToDateTime(tradeInInspectionExpiredDate.ToString()) : null;
                appraisal.Vin = vin.ToString();
                appraisal.DelFlag = "0";
                appraisal.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                appraisal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                appraisal.LastUpdateDate = DateTime.Now;
                appraisal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                appraisal.PurchaseCreated = "0";
                object classification = CommonUtils.GetModelProperty(header, "TradeInClassificationTypeNumber" + i);
                appraisal.ClassificationTypeNumber = classification != null ? classification.ToString() : "";
                object modelSpecificate = CommonUtils.GetModelProperty(header, "TradeInModelSpecificateNumber" + i);
                appraisal.ModelSpecificateNumber = modelSpecificate != null ? modelSpecificate.ToString() : "";
                appraisal.DepartmentCode = header.DepartmentCode;
                object eraseRegist = CommonUtils.GetModelProperty(header, "TradeInEraseRegist" + i);
                appraisal.EraseRegist = eraseRegist != null ? eraseRegist.ToString() : "";
                //#3036 ����f�[�^�ɏ����ID�Ɛŗ���ݒ�ǉ�  2014/06/09 arc.ookubo
                appraisal.ConsumptionTaxId = header.ConsumptionTaxId;
                appraisal.Rate = (int)header.Rate;

                //���z�̘A�g
                object tradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                appraisal.AppraisalPrice = tradeInAmount != null ? decimal.Parse(tradeInAmount.ToString()) : 0m;

                object tradeInUnexpiredCarTax = CommonUtils.GetModelProperty(header, "TradeInUnexpiredCarTax" + i);
                appraisal.CarTaxUnexpiredAmount = tradeInUnexpiredCarTax != null ? decimal.Parse(tradeInUnexpiredCarTax.ToString()) : 0m;

                object tradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                appraisal.RemainDebt = tradeInRemainDebt != null ? decimal.Parse(tradeInRemainDebt.ToString()) : 0m;

                object tradeInRecycleAmount = CommonUtils.GetModelProperty(header, "TradeInRecycleAmount" + i);
                appraisal.RecycleDeposit = tradeInRecycleAmount != null ? decimal.Parse(tradeInRecycleAmount.ToString()) : 0m;

                //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
                //�󒍎��͏���쐬�Ȃ̂ōX�V���b�Z�[�W�͏o���Ȃ�
                if (OrderFlag)
                {
                    appraisal.LastEditScreen = "000";
                }
                else
                {
                    appraisal.LastEditScreen = LAST_EDIT_CARSALSEORDER;
                    header.LastEditScreen = "000";
                }

                //����f�[�^INSERT
                db.CarAppraisal.InsertOnSubmit(appraisal);

                //����˗��쐬
                task.CarAppraisal(appraisal);

            }
        }
        #endregion

        #region �����̍���f�[�^���폜
        /// <summary>
        /// �����̍���f�[�^���폜����
        /// </summary>
        /// <param name="header"></param>
        private void DeleteAppraisalData(CarSalesHeader header, int i) {
            CarAppraisalDao dao = new CarAppraisalDao(db);
            List<CarAppraisal> appraisalList;
            object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
            if (vin != null && !string.IsNullOrEmpty(vin.ToString())) {
                appraisalList = dao.GetListBySlipNumberVin(header.SlipNumber, vin.ToString(), "0");
                foreach (var item in appraisalList) {
                    item.DelFlag = "1";
                    item.LastUpdateDate = DateTime.Now;
                    item.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }
        }
        #endregion

        #region �����\��f�[�^���쐬
        /// <summary>
        /// �����\��f�[�^���쐬����
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/12/22 arc yano #3793 �����\��č쐬���̕s�
        /// 2017/12/22 arc yano #3703 �ԗ��`�[�@�����ύX����Ɛ������z���{�ɂȂ���
        /// 2016/04/05 arc yano #3441 �J�[�h�����������}�C�i�X�̓����\�肪�ł��Ă��܂� �������ю擾���͐������� =�@�u�N���W�b�g�v�͏���
        /// </history>
        private void CreatePaymentPlan(CarSalesHeader header) {

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null) {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            //�����̓����\����폜
            //Mod 2017/12/22 arc yano #3703
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "001");
            //List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber,header.DepartmentCode);
            foreach (var d in delList) {
                //Add 2016/09/05 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                //�c�Ɖ���ȊO�̓����\��
                if (!d.ReceiptType.Equals("012") && !d.ReceiptType.Equals("013"))
                {
                    d.DelFlag = "1";
                    d.LastUpdateDate = DateTime.Now;
                    d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }
            }

            //���[���̊��������\����폜
            //Mod 2017/12/22 arc yano #3793
            //2017/01/05 arc yano test
            List<ReceiptPlan> delLoanList = new ReceiptPlanDao(db).GetBySlipNumber(header.SlipNumber, new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value);
            //List<ReceiptPlan> delLoanList = new ReceiptPlanDao(db).GetCashBySlipNumber(header.SlipNumber, "004");
            foreach (var d in delLoanList) {
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //**�����̓����\���(��)�쐬����**
            //������A�����\����̏��ŕ��ёւ�
            List<CarSalesPayment> payList = header.CarSalesPayment.ToList();
            payList.Sort(delegate(CarSalesPayment x, CarSalesPayment y) {
                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                return
                    !CommonUtils.DefaultString(x.CustomerClaimCode).Equals(CommonUtils.DefaultString(y.CustomerClaimCode)) ? CommonUtils.DefaultString(x.CustomerClaimCode).CompareTo(CommonUtils.DefaultString(y.CustomerClaimCode)) : //�����揇
                    !x.PaymentPlanDate.Equals(y.PaymentPlanDate) ? DateTime.Compare(x.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, y.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX) : //�����\�����
                    (0);
            });
            
            //������ꗗ�쐬�p
            List<string> customerClaimList = new List<string>();

            // �������ъz
            //Add 2017/06/16 arc nakayama #3772_�y�ԗ��`�[�z�x�����Ƀ}�C�i�X�̋��z����ꂽ���̍l��
            decimal PlusjournalAmount = 0m;
            decimal MinusjournalAmount = 0m;
            for (int i = 0; i < payList.Count; i++) {

                //�����惊�X�g�ɒǉ�
                customerClaimList.Add(payList[i].CustomerClaimCode);

                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                //�����悪�ς����������ς݋��z���i�āj�擾����
                if (i == 0 || (i > 0 && !CommonUtils.DefaultString(payList[i - 1].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i].CustomerClaimCode))))
                {
                    //Add 2017/06/16 arc nakayama #3772_�y�ԗ��`�[�z�x�����Ƀ}�C�i�X�̋��z����ꂽ���̍l��
                    PlusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, true);
                    MinusjournalAmount = new JournalDao(db).GetPlusMinusTotalByCondition(header.SlipNumber, payList[i].CustomerClaimCode, false, false);
                }

                // ���|�c
                decimal balanceAmount = 0m;

                //Add 2017/06/16 arc nakayama #3772_�y�ԗ��`�[�z�x�����Ƀ}�C�i�X�̋��z����ꂽ���̍l��
                if (payList[i].Amount >= 0)
                {

                    if (payList[i].Amount >= PlusjournalAmount)
                    {
                        // �\��z >= ���ъz
                        balanceAmount = ((payList[i].Amount ?? 0m) - PlusjournalAmount);
                        PlusjournalAmount = 0m;
                    }
                    else
                    {
                        // �\��z < ���ъz
                        balanceAmount = 0m;
                        PlusjournalAmount = PlusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = PlusjournalAmount * (-1);
                        }
                    }
                }
                else
                {
                    if (payList[i].Amount >= MinusjournalAmount)
                    {
                        // �\��z >= ���ъz
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = MinusjournalAmount - (payList[i].Amount ?? 0m);

                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        // ���̐����悪�قȂ�ꍇ�A�����Ń}�C�i�X�̔��|�����쐬���Ă���
                        if (i == payList.Count() - 1 || (i < payList.Count() - 1 && !CommonUtils.DefaultString(payList[i].CustomerClaimCode).Equals(CommonUtils.DefaultString(payList[i + 1].CustomerClaimCode))))
                        {
                            balanceAmount = MinusjournalAmount * (-1);
                        }
                        
                    }
                    else
                    {
                        // �\��z < ���ъz
                        balanceAmount = ((payList[i].Amount ?? 0m) - MinusjournalAmount);
                        MinusjournalAmount = 0m;
                    }
                }

                CreateReceiptPlan(payList[i], balanceAmount, carAccount.AccountCode);
            }


            //���[���̓����\��
            decimal journalAmount = 0m; //�����ς݋��z

            //�v�������I������Ă���Ƃ���������
            if(!string.IsNullOrEmpty(header.PaymentPlanType)){
                //���[���R�[�h���擾����
                object loanCode = header.GetType().GetProperty("LoanCode" + header.PaymentPlanType).GetValue(header, null);

                //���[���R�[�h�͕K�{
                if (loanCode != null && !loanCode.Equals("")) {
                    Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());

                    //�}�X�^�ɑ��݂��邱��
                    if (loan != null) {

                        //�����惊�X�g�ɒǉ�
                        customerClaimList.Add(loan.CustomerClaimCode);

                        //�����ς݋��z���擾
                        journalAmount = new JournalDao(db).GetTotalByCustomerAndSlip(header.SlipNumber, loan.CustomerClaimCode);
                        
                        //���[������
                        decimal loanAmount = decimal.Parse(CommonUtils.GetModelProperty(header, "LoanPrincipal" + header.PaymentPlanType).ToString());
                        
                        //�����c
                        decimal receivableBalance = loanAmount - journalAmount;

                        ReceiptPlan plan = new ReceiptPlan();
                        plan.CreateDate = DateTime.Now;
                        plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.DelFlag = "0";
                        plan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�o���ɓ����\��
                        plan.LastUpdateDate = DateTime.Now;
                        plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        plan.ReceiptType = "004"; //���[��
                        plan.SlipNumber = header.SlipNumber;
                        plan.OccurredDepartmentCode = header.DepartmentCode;
                        plan.CustomerClaimCode = loan.CustomerClaimCode;
                        plan.ReceiptPlanDate = CommonUtils.GetPaymentPlanDate(header.SalesOrderDate ?? DateTime.Today, loan.CustomerClaim);
                        plan.AccountCode = carAccount.AccountCode;

                        plan.ReceiptPlanId = Guid.NewGuid();
                        plan.Amount = loanAmount;
                        plan.ReceivableBalance = receivableBalance;
                        if (receivableBalance == 0m) {
                            plan.CompleteFlag = "1";
                        }else{
                            plan.CompleteFlag = "0";
                        }
                        db.ReceiptPlan.InsertOnSubmit(plan);
                    }
                }
            }

            //Mod 2016/04/05 arc yano #3441
            //�����ς݂̐����悪����̓`�[����Ȃ��Ȃ��Ă�����̂�ʒm
            List<Journal> journalList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, excludeList, excluedAccountTypetList);
            foreach (Journal a in journalList) {
                if (customerClaimList.IndexOf(a.CustomerClaimCode) < 0) {
                    TaskUtil task = new TaskUtil(db, (Employee)Session["Employee"]);
                    task.CarOverReceive(header,a.CustomerClaimCode,a.Amount);
                    
                    // �}�C�i�X�œ����\��쐬
                    ReceiptPlan plan = new ReceiptPlan();
                    plan.Amount = a.Amount * (-1m);
                    plan.ReceivableBalance = a.Amount * (-1m);
                    plan.ReceiptPlanId = Guid.NewGuid();
                    plan.CreateDate = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)); ;
                    plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.DelFlag = "0";
                    plan.CompleteFlag = "0";
                    plan.LastUpdateDate = DateTime.Now;
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    plan.ReceiptType = "001"; // ����
                    plan.SlipNumber = a.SlipNumber;
                    plan.OccurredDepartmentCode = a.DepartmentCode;
                    plan.AccountCode = a.AccountCode;
                    plan.CustomerClaimCode = a.CustomerClaimCode;
                    plan.DepartmentCode = a.DepartmentCode;
                    plan.ReceiptPlanDate = a.JournalDate;
                    plan.Summary = a.Summary;
                    plan.JournalDate = a.JournalDate;
                    db.ReceiptPlan.InsertOnSubmit(plan);
                  }
            }
        }
        private void CreateReceiptPlan(CarSalesPayment payment, decimal planAmount, string accountCode) {
            ReceiptPlan plan = new ReceiptPlan();
            plan.ReceiptPlanId = Guid.NewGuid();
            plan.CreateDate = DateTime.Now;
            plan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.SlipNumber = payment.CarSalesHeader.SlipNumber;
            plan.LastUpdateDate = DateTime.Now;
            plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            plan.DelFlag = "0";
            plan.DepartmentCode = payment.CarSalesHeader.DepartmentCode;
            plan.ReceiptPlanDate = payment.PaymentPlanDate;
            plan.ReceiptType = "001";
            plan.Amount = payment.Amount;
            plan.CustomerClaimCode = payment.CustomerClaimCode;
            plan.OccurredDepartmentCode = payment.CarSalesHeader.DepartmentCode;
            plan.AccountCode = accountCode;
            plan.ReceivableBalance = planAmount;
            plan.Summary = payment.Memo;
            if (planAmount.Equals(0m)) {
                plan.CompleteFlag = "1";
            } else {
                plan.CompleteFlag = "0";
            }
            plan.JournalDate = payment.CarSalesHeader.SalesOrderDate ?? DateTime.Today;
            db.ReceiptPlan.InsertOnSubmit(plan);
        }
        #endregion


        #region ����Ԃ̓����\����č쐬����B�c������Ύc�̓����\����쐬����
        /// <summary>
        /// ����Ԃ̓����\����č쐬����
        /// </summary>
        /// <histtory>
        /// 2017/11/14 arc yano  #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s���� �����\��Ɏԑ�ԍ���ێ�������ǉ�
        /// </histtory>
        private void CreateTradeReceiptPlan(CarSalesHeader header)
        {
            //�����̓����\����폜
            List<ReceiptPlan> delList = new ReceiptPlanDao(db).GetListByslipNumber(header.SlipNumber).Where(x => x.ReceiptType == "012" || x.ReceiptType == "013").ToList();
            foreach (var d in delList)
            {
                //Add 2016/09/05 arc nakayama #3630_�y�����z�ԗ����|���Ή�
                //�c�Ɖ���ȊO�̓����\��
                d.DelFlag = "1";
                d.LastUpdateDate = DateTime.Now;
                d.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            }

            //�Ȗڃf�[�^�擾
            Account carAccount = new AccountDao(db).GetByUsageType("CR");
            if (carAccount == null)
            {
                ModelState.AddModelError("", "�Ȗڐݒ肪����������܂���B�V�X�e���Ǘ��҂ɘA�����ĉ������B");
                return;
            }

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var varTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (varTradeInAmount != null && !string.IsNullOrEmpty(varTradeInAmount.ToString()))
                    {
                        TradeInAmount = varTradeInAmount.ToString();
                    }

                    var varTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (varTradeInRemainDebt != null && !string.IsNullOrEmpty(varTradeInRemainDebt.ToString()))
                    {
                        TradeInRemainDebt = varTradeInRemainDebt.ToString();
                    }

                    //string TradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i).ToString();
                    //string TradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i).ToString();
                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);
                    decimal JournalAmount = 0; //����̓����z
                    decimal JournalDebtAmount = 0; //�c�̓����z

                    //Mod 2017/11/14 arc yano #3811
                    //����̓����z�擾
                    Journal JournalData = new JournalDao(db).GetTradeJournal(header.SlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal JournalData = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (JournalData != null)
                    {
                        JournalAmount = JournalData.Amount;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //�c�̓����z�擾
                    Journal JournalData2 = new JournalDao(db).GetTradeJournal(header.SlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal JournalData2 = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (JournalData2 != null)
                    {
                        JournalDebtAmount = JournalData2.Amount;
                    }

                    ReceiptPlan TradePlan = new ReceiptPlan();
                    TradePlan.ReceiptPlanId = Guid.NewGuid();
                    TradePlan.DepartmentCode = header.DepartmentCode;
                    TradePlan.OccurredDepartmentCode = header.DepartmentCode;
                    TradePlan.CustomerClaimCode = header.CustomerCode;
                    TradePlan.SlipNumber = header.SlipNumber;
                    TradePlan.ReceiptType = "013"; //����
                    TradePlan.ReceiptPlanDate = null;
                    TradePlan.AccountCode = carAccount.AccountCode;
                    TradePlan.Amount = decimal.Parse(TradeInAmount);
                    TradePlan.ReceivableBalance = decimal.Subtract(TradePlan.Amount ?? 0m, JournalAmount); //���v�Z
                    if (TradePlan.ReceivableBalance == 0m)
                    {
                        TradePlan.CompleteFlag = "1";
                    }
                    else
                    {
                        TradePlan.CompleteFlag = "0";
                    }
                    TradePlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.CreateDate = DateTime.Now;
                    TradePlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    TradePlan.LastUpdateDate = DateTime.Now;
                    TradePlan.DelFlag = "0";
                    TradePlan.Summary = "";
                    TradePlan.JournalDate = null;
                    TradePlan.DepositFlag = "0";
                    TradePlan.PaymentKindCode = "";
                    TradePlan.CommissionRate = null;
                    TradePlan.CommissionAmount = null;
                    TradePlan.CreditJournalId = "";
                    TradePlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                    db.ReceiptPlan.InsertOnSubmit(TradePlan);

                    //�c���������ꍇ�c���̓����\����}�C�i�X�ō쐬����
                    if(!string.IsNullOrEmpty(TradeInRemainDebt)){
                        ReceiptPlan RemainDebtPlan = new ReceiptPlan();
                        RemainDebtPlan.ReceiptPlanId = Guid.NewGuid();
                        RemainDebtPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�c�͌o���ɐU��ւ�
                        RemainDebtPlan.OccurredDepartmentCode = header.DepartmentCode;
                        RemainDebtPlan.CustomerClaimCode = header.CustomerCode;
                        RemainDebtPlan.SlipNumber = header.SlipNumber;
                        RemainDebtPlan.ReceiptType = "012"; //�c��
                        RemainDebtPlan.ReceiptPlanDate = null;
                        RemainDebtPlan.AccountCode = carAccount.AccountCode;
                        RemainDebtPlan.Amount = PlanRemainDebt;
                        RemainDebtPlan.ReceivableBalance = decimal.Subtract(PlanRemainDebt, JournalDebtAmount); //�v�Z
                        if (RemainDebtPlan.ReceivableBalance == 0m)
                        {
                            RemainDebtPlan.CompleteFlag = "1";
                        }
                        else
                        {
                            RemainDebtPlan.CompleteFlag = "0";
                        }
                        RemainDebtPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.CreateDate = DateTime.Now;
                        RemainDebtPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        RemainDebtPlan.LastUpdateDate = DateTime.Now;
                        RemainDebtPlan.DelFlag = "0";
                        RemainDebtPlan.Summary = "";
                        RemainDebtPlan.JournalDate = null;
                        RemainDebtPlan.DepositFlag = "0";
                        RemainDebtPlan.PaymentKindCode = "";
                        RemainDebtPlan.CommissionRate = null;
                        RemainDebtPlan.CommissionAmount = null;
                        RemainDebtPlan.CreditJournalId = "";
                        RemainDebtPlan.TradeVin = vin.ToString();            //Add 2017/11/14 arc yano #3811

                        db.ReceiptPlan.InsertOnSubmit(RemainDebtPlan);
                    }
                    //db.SubmitChanges();
                }   
            }
        }

        #endregion

        #region Validation
        /// <summary>
        /// �󒍏������̓��̓`�F�b�N
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        private void ValidateCarSalesOrder(CarSalesHeader header)
        {
            CommonValidate("NewUsedType", "�V���敪", header, true);
            CommonValidate("CarGradeCode", "�O���[�h�R�[�h", header, true);
            CommonValidate("CustomerCode", "�ڋq�R�[�h", header, true);
            string[] str = new string[] { "A", "B", "C" };

            //Add 2014/05/16 arc yano vs2012�Ή� header.PaymentPlanType�̃`�F�b�N��ǉ�
            if ((header.PaymentPlanType != null) && (header.PaymentPlanType != "")) //header.PaymentPlanType��null��u���[���Ȃ��v�łȂ��ꍇ
            {
                for (int i = 0; i < 3; i++)
                {
                    //if (header.PaymentPlanType.Equals(str[i])) {
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (CommonUtils.DefaultString(header.PaymentPlanType).Equals(str[i]))
                    {
                        CommonValidate("LoanCode" + str[i], "���[���R�[�h" + str[i], header, true);
                        object loanCode = header.GetType().GetProperty("LoanCode" + str[i]).GetValue(header, null);
                        if (loanCode != null && !string.IsNullOrEmpty(loanCode.ToString()))
                        {
                            Loan loan = new LoanDao(db).GetByKey(loanCode.ToString());
                            if (loan == null)
                            {
                                ModelState.AddModelError("LoanCode" + str[i], MessageUtils.GetMessage("E0016", "���[��" + str[i]));
                            }
                            else
                            {
                                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                                if (loan.CustomerClaim == null ||
                                    CommonUtils.DefaultString(loan.CustomerClaim.CustomerClaimType).Equals("001"))
                                {
                                    ModelState.AddModelError("LoanCode" + str[i], "�w�肳�ꂽ���[���̐����悪�������ݒ肳��Ă��܂���");
                                }
                                if (loan.CustomerClaim.CustomerClaimable.Count == 0)
                                {
                                    ModelState.AddModelError("LoanCode" + str[i], "�w�肳�ꂽ���[���̐�����Ɍ��Ϗ������ݒ肳��Ă��܂���");
                                }
                            }
                        }
                    }
                }
            }
                    
            // �����
            if (header.TradeInAmount1 != null && string.IsNullOrEmpty(header.TradeInVin1)) {
                ModelState.AddModelError("TradeInVin1", MessageUtils.GetMessage("E0001", "�����(1���)�̎ԑ�ԍ�" ));
            }
            if (header.TradeInAmount2 != null && string.IsNullOrEmpty(header.TradeInVin2)) {
                ModelState.AddModelError("TradeInVin2", MessageUtils.GetMessage("E0001", "�����(2���)�̎ԑ�ԍ�"));
            }
            if (header.TradeInAmount3 != null && string.IsNullOrEmpty(header.TradeInVin3)) {
                ModelState.AddModelError("TradeInVin3", MessageUtils.GetMessage("E0001", "�����(3���)�̎ԑ�ԍ�"));
            }

            if (header.CarSalesPayment.Count == 0 && string.IsNullOrEmpty(header.PaymentPlanType)) {
                ModelState.AddModelError("LoanPrincipalAmount", "�x�����@�����͂���Ă��܂���");
            }
            if (header.LoanPrincipalAmount > header.LoanTotalAmount) {
                ModelState.AddModelError("LoanPrincipalAmount","�x�����@�����������͂���Ă��܂���");
            }
            //2017/04/27 arc nakayama #3744_[����] ���|�����������o�b�`���N���ƂȂ�s����C
            if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
            {
                ModelState.AddModelError("CustomerCode", "�󒍈ȍ~�͌ڋq�R�[�h�ɏ�l���g�p���邱�Ƃ͂ł��܂���B");
            }

            for (int i = 0; i < header.CarSalesPayment.Count; i++)
            {
                if (!string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode) && header.CarSalesPayment[i].CustomerClaimCode.Equals("000001"))
                {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "�󒍈ȍ~�͐�����ɏ�l���g�p���邱�Ƃ͂ł��܂���B");
                }
            }
        }

        /// <summary>
        /// �S�X�e�[�^�X���ʂ̓��̓`�F�b�N
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        /// <history>
        /// 2023/08/15 yano #4176 �̔�����p�̏C��
        /// 2020/11/17 yano #4059 �Ɣ̓`�[�̃X�e�[�^�X�����X�V�i�[�ԑO�j�̕s��ɂ���
        /// 2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// 2018/08/14 yano #3910 �ԗ��`�[�@�f���J�[�̔[�ԍϓ`�[�C�����Ɏԗ��}�X�^�̃��P�[�V������������
        /// 2018/08/07 yano #3911 �o�^�ώԗ��̎ԗ��`�[�X�e�[�^�X�C���ɂ���
        /// 2018/04/10 arc yano #3879 �ԗ��`�[�@���P�[�V�����}�X�^�ɕ���R�[�h��ݒ肵�Ă��Ȃ��ƁA�[�ԏ������s���Ȃ�
        /// 2018/02/20 arc yano #3858 �T�[�r�X�`�[�@�[�Ԍ�̕ۑ������ŁA�[�ԓ����󗓂ŕۑ��ł��Ă��܂�
        /// 2018/01/17 arc yano #3813 �ԗ��`�[�ɂă��[��������0�~�̂܂ܕς��Ȃ����ۇA
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// </history>
        private void ValidateAllStatus(CarSalesHeader header, FormCollection form, bool OrderFlag = false)
        {
            //��{���
            CommonValidate("DepartmentCode", "����R�[�h", header, true);
            CommonValidate("EmployeeCode", "�S���҃R�[�h", header, true);
            //CommonValidate("CustomerCode", "�ڋq�R�[�h", header, true);
            CommonValidate("QuoteDate", "���ϓ�", header, true);
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //���ϕۑ��̂Ƃ��L�������K�{
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001"))
            {
                CommonValidate("QuoteExpireDate", "���ϗL������", header, true);

                //���ϗL�������͌��ϓ��ȍ~�����F�߂Ȃ�
                if (ModelState.IsValidField("QuoteExpireDate") && header.QuoteDate != null && header.QuoteExpireDate != null) {
                    if (header.QuoteDate != null && DateTime.Compare(header.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, header.QuoteExpireDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                        ModelState.AddModelError("QuoteExpireDate", MessageUtils.GetMessage("E0013", new string[] { "���ϗL������", "���ϓ��ȍ~" }));
                    }
                }
            }
            //Mod 2018/02/20 arc yano #3858 �[�ԓ��̃`�F�b�N�͓`�[�C�������s��
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //�[�ԏ����̏ꍇ�[�ԓ��K�{�A�݌ɂ����ꕔ����ɂȂ��Ƃ����Ȃ�
            //Add 2017/05/25 �`�[�C���Ή� �`�[�C�����̓��P�[�V�����̃`�F�b�N���s��Ȃ��i���ɔ[�ԍςȂ̂Ń��P�[�V������NULL�ɂȂ��Ă��邽�߁j
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005"))
            //if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("005") && !header.ActionType.Equals("ModificationEnd"))
            {
                //Mod 2018/08/14 yano #3910
                //�`�[�C�����܂��́A�f���o�^���̓��P�[�V�����̃`�F�b�N���s��Ȃ�
                //if (!header.ActionType.Equals("ModificationEnd"))
                if (!header.ActionType.Equals("ModificationEnd") &&
                    !(
                        !string.IsNullOrWhiteSpace(header.SalesType) &&
                        (
                            header.SalesType.Equals("006") ||                           //�f���o�^
                            header.SalesType.Equals("010") ||                           //�����^�J�[�o�^
                            header.SalesType.Equals("011") ||                           //��ԓo�^
                            header.SalesType.Equals("012") ||                           //�L��ԓo�^
                            header.SalesType.Equals("013") ||                           //�Ɩ��ԓo�^
                            header.SalesType.Equals(SALESTYPE_BUSINESSSALES) ||         //�Ɣ�            //Add 2020/11/17 yano #4059
                            header.SalesType.Equals(SALESTYPE_AUTOAUCTION)              //AA              //Add 2020/11/17 yano #4059
                        )
                    )
                )
                {
                    if (header.SalesCarNumber == null)
                    {
                        ModelState.AddModelError("", "�ԗ����������Ă��Ȃ����ߔ[�Ԃł��܂���");
                    }
                    else
                    {
                        SalesCar car = new SalesCarDao(db).GetByKey(header.SalesCarNumber);

                        //Mod 2018/04/10 arc yano #3879
                        if (car == null || car.Location == null)
                        {
                            ModelState.AddModelError("", "�ԗ��݌ɂ�������̃��P�[�V�����ɑ��݂��Ȃ����ߔ[�Ԃł��܂���");
                        }
                        else
                        {
                            DepartmentWarehouse dw = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

                            //�g�p�q�ɂ���������Ȃ����A����̎g�p�q�ɂƎԗ��̍݌ɂ̂���q�ɂ��قȂ�ꍇ
                            if (dw == null || !car.Location.WarehouseCode.Equals(dw.WarehouseCode))
                            {
                                ModelState.AddModelError("", "�ԗ��݌ɂ�������̃��P�[�V�����ɑ��݂��Ȃ����ߔ[�Ԃł��܂���");
                            }
                        }
                        /*
                        if (car == null || car.Location == null || car.Location.Department == null || !car.Location.DepartmentCode.Equals(header.DepartmentCode))
                        {
                            ModelState.AddModelError("", "�ԗ��݌ɂ�������̃��P�[�V�����ɑ��݂��Ȃ����ߔ[�Ԃł��܂���");
                        }
                        */
                    }
                }
                CommonValidate("SalesDate", "�[�ԓ�", header, true);
                //�[�ԓ����I�����ߏ������ꂽ���ł���΃G���[
                if (header.SalesDate != null) {
                    // Mod 2015/04/15 arc yano�@�������ύX�\�ȃ��[�U�̏ꍇ�A�����߂̏ꍇ�́A�ύX�\�Ƃ���
                    if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode, header.SalesDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                    {
                        ModelState.AddModelError("SalesDate", "�������ߏ������I�����Ă��邽�ߎw�肳�ꂽ�[�ԓ��ł̔[�Ԃ͂ł��܂���");
                    }
                }
            }
            if (!ModelState.IsValidField("SalesOrderDate")) {
                if (ModelState["SalesOrderDate"].Errors.Count() > 0) {
                    ModelState["SalesOrderDate"].Errors.RemoveAt(0);
                }
                ModelState.AddModelError("SalesOrderDate", MessageUtils.GetMessage("E0005", "�󒍓�"));
            }
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //�󒍈ȍ~�̃X�e�[�^�X�ł͎󒍓��K�{
            if (!CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001")
                && !CommonUtils.DefaultString(header.SalesOrderStatus).Equals("006") 
                && header.SalesOrderDate == null)
            {
                ModelState.AddModelError("SalesOrderDate", MessageUtils.GetMessage("E0009", new string[] { "�󒍈ȍ~", "�󒍓�" }));
            }

            //�󒍓������ߏ����ς݂̌��̏ꍇNG
            //if (header.SalesOrderDate != null && !new InventoryScheduleDao(db).IsClosedInventoryMonth(header.DepartmentCode,header.SalesOrderDate,"001")) {
            //    ModelState.AddModelError("SalesOrderDate", "�w�肳�ꂽ�󒍓��͒I�����ߏ������I�����Ă��܂�");
            //}

            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //�V�Ԃ̏ꍇ�A�ԗ��̃O���[�h���̔��Ώۊ��ԂłȂ���΂����Ȃ�
            if (CommonUtils.DefaultString(header.NewUsedType).Equals("N"))
            {
                CarGrade grade = new CarGradeDao(db).GetByKey(header.CarGradeCode);
                if (grade != null) {
                    if (DateTime.Compare(DateTime.Today, grade.SalesStartDate ?? DaoConst.SQL_DATETIME_MIN) < 0 || DateTime.Compare(DateTime.Today, grade.SalesEndDate ?? DaoConst.SQL_DATETIME_MAX) > 0) {
                        ModelState.AddModelError("CarGradeCode", "�w�肳�ꂽ�O���[�h�͔̔����ԊO�ł�");
                    }
                }
            }
            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
            //ADD 2014/02/20 ookubo
            //�[�ԑO�X�e�[�^�X�܂Ŕ[�ԗ\����K�{
            if (CommonUtils.DefaultString(header.SalesOrderStatus).Equals("001") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("002") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("003") 
                || CommonUtils.DefaultString(header.SalesOrderStatus).Equals("004"))
            {
                CommonValidate("SalesPlanDate", "�[�ԗ\���", header, true);
            }
            
            //�̔��ԗ�
            CommonValidate("Mileage", "���s����", header, false);

            //��ېŁE�s�ېō���
            CommonValidate("CarTax", "�����ԐŎ�ʊ�", header, false);    //Mod 2019/09/04 yano #4011
            CommonValidate("CarLiabilityInsurance", "�����ӕی���", header, false);
            CommonValidate("CarWeightTax", "�����ԏd�ʐ�", header, false);
            CommonValidate("AcquisitionTax", "�����ԐŊ����\��", header, false);//Mod 2019/09/04 yano #4011
            CommonValidate("InspectionRegistCost", "�����o�^��p", header, false);
            CommonValidate("ParkingSpaceCost", "�Ԍɏؖ���p", header, false);
            CommonValidate("TradeInCost", "����Ԕ�p", header, false);
            CommonValidate("RecycleDeposit", "���T�C�N���a����", header, false);
            CommonValidate("RecycleDepositTradeIn", "���T�C�N���a��������", header, false);
            CommonValidate("NumberPlateCost", "�i���o�[�v���[�g��", header, false);
            // Add 2014/07/25 arc amii �ۑ�Ή� # 3018 �����󎆑�Ɖ��掩���Ԑŗa�����ǉ�
            CommonValidate("RevenueStampCost", "�����󎆑�", header, false);
            CommonValidate("TradeInCarTaxDeposit", "���掩���ԐŎ�ʊ��a���", header, false);  //Mod 2019/09/04 yano #4011

            // Add 2014/07/29 arc amii �ۑ�Ή� #3018
            // ��ېŎ��R���ږ���null or �� or �󔒕����̏ꍇ
            if (string.IsNullOrWhiteSpace(header.TaxFreeFieldName))
            {
                
                // ��ېŎ��R���ڒl�����͂���Ă����ꍇ
                if (header.TaxFreeFieldValue != null)
                {
                    ModelState.AddModelError("TaxFreeFieldName", "��ېŎ��R���ږ��������͂ł�");
                }

            }
            CommonValidate("TaxFreeFieldValue", "��ېŎ��R���ڒl", header, false);

            //�ېō���
            CommonValidate("InspectionRegistFee", "�����o�^�葱", header, false);
            CommonValidate("ParkingSpaceFee", "�Ԍɏؖ��葱", header, false);
            CommonValidate("TradeInFee", "����ԏ���p", header, false);
            CommonValidate("PreparationFee", "�[�ԏ�����p", header, false);
            CommonValidate("CarTaxUnexpiredAmount", "�����ԐŎ�ʊ����o�ߑ����z", header, false);  //Mod 2019/09/04 yano #4011
            CommonValidate("CarLiabilityInsuranceUnexpiredAmount", "�����Ӗ��o�ߑ����z", header, false);
            CommonValidate("RecycleControlFee", "���T�C�N�������Ǘ���", header, false);
            CommonValidate("RecycleControlFeeTradeIn", "���T�C�N�������Ǘ�������", header, false);
            CommonValidate("RequestNumberFee", "��]�ԍ���p", header, false);

            // Add 2014/07/29 arc amii �ۑ�Ή� #3018
            // �ېŎ��R���ږ���null or �� or �󔒕����̏ꍇ
            if (string.IsNullOrWhiteSpace(header.TaxationFieldName))
            {
                // �ېŎ��R���ڒl�����͂���Ă����ꍇ
                // Mod 2023/08/15 yano #4176
                if(form["TaxFreeFieldValueWithTax"] != null)
                //if (header.TaxationFieldValue != null)
                {
                    ModelState.AddModelError("TaxationFieldName", "�ېŎ��R���ږ��������͂ł�");
                }

            }

            //Add 2020/01/06 yano #4029
            //�̔��敪���Ɣ́AAA�A�˔p�ȊO�̏ꍇ�́A�o�^��s���{���̕K�{�`�F�b�N���s��
            if (!string.IsNullOrWhiteSpace(header.SalesType) &&
               !header.SalesType.Equals("003") &&
               !header.SalesType.Equals("004") &&
               !header.SalesType.Equals("008")
            )
            {
                CommonValidate("CostAreaCode", "�̔��敪���u�Ɣ́v�uAA�v�u�˔p�v�ȊO�̏ꍇ�A�s���{���R�[�h", header, true);
            }

            CommonValidate("TaxationFieldValue", "�ېŎ��R���ڒl", header, false);

            //�̔����z
            CommonValidate("SalesPrice", "�ԗ��{�̉��i", header, true);
            CommonValidate("DiscountAmount", "�l�����z", header, false);

            //�o�^���
            CommonValidate("RequestRegistDate", "�o�^��]��", header, false);
            //20140219  �[�ԓ��A�[�ԗ\�������{���֋L�q�ړ� ookubo
            //CommonValidate("SalesPlanDate", "�[�ԗ\���", header, false);
            ////CommonValidate("SalesDate", "�[�ԓ�", header, false);
            CommonValidate("SealSubmitDate", "��ӏؖ�", header, false);
            CommonValidate("ProxySubmitDate", "�ϔC��", header, false);
            CommonValidate("ParkingSpaceSubmitDate", "�Ԍɏؖ�", header, false);
            CommonValidate("CarLiabilityInsuranceSubmitDate", "������", header, false);
            CommonValidate("OwnershipReservationSubmitDate", "���L�����ۏ���", header, false);

            //�C�ӕی�
            CommonValidate("VoluntaryInsuranceAmount", "�ی����z(�N�z)", header, false);
            CommonValidate("VoluntaryInsuranceTermFrom", "�ی��_��J�n��", header, false);
            CommonValidate("VoluntaryInsuranceTermTo", "�ی��_�񖞗���", header, false);

            string[] loan = new string[] { "", "A", "B", "C" };
            for (int i = 1; i <= 3; i++) {
                //�����
                CommonValidate("TradeInAmount" + i, "����ԉ��i(" + i + "���)", header, false);
                CommonValidate("TradeInTax" + i, "����ԏ����(" + i + "���)", header, false);
                CommonValidate("TradeInUnexpiredCarTax" + i, "����Ԗ��������ԐŎ�ʊ�(" + i + "���)", header, false);  //Mod 2019/09/04 yano #4011
                CommonValidate("TradeInRemainDebt" + i, "����Ԏc��(" + i + "���)", header, false);
                CommonValidate("TradeInRecycleAmount" + i, "����ԃ��T�C�N��(" + i + "���)", header, false);
                CommonValidate("TradeInInspectionExpiredDate" + i, "����ԎԌ�������(" + i + "���)", header, false);
                CommonValidate("TradeInMileage" + i, "����ԑ��s����(" + i + "���)", header, false);

                //���[��
                CommonValidate("PaymentFrequency" + loan[i], "���[��" + loan[i] + "�x����", header, false);
                CommonValidate("PaymentTermFrom" + loan[i], "���[��" + loan[i] + "�x���J�n��" + loan[i], header, false);
                CommonValidate("PaymentTermTo" + loan[i], "���[��" + loan[i] + "�x���I����" + loan[i], header, false);
                CommonValidate("BonusMonth" + loan[i] + "1", "���[��" + loan[i] + "�{�[�i�X���P", header, false);
                CommonValidate("BonusMonth" + loan[i] + "2", "���[��" + loan[i] + "�{�[�i�X���Q", header, false);
                CommonValidate("FirstAmount" + loan[i], "���[��" + loan[i] + "������z", header, false);
                CommonValidate("FirstDirectDebitDate" + loan[i], "���[��" + loan[i] + "���������", header, false);
                CommonValidate("SecondDirectDebitDate" + loan[i], "���[��" + loan[i] + "2��ڈȍ~������", header, false);
                CommonValidate("RemainAmount" + loan[i], "���[��" + loan[i] + "�c�����z", header, false);
                CommonValidate("RemainFinalMonth" + loan[i], "���[��" + loan[i] + "�c���ŏI��", header, false);
                //Add 20170/02/02 arc nakayama #3489_�ԗ��`�[�̎����Ԓ����\�����̃��[���̂Q��ڈȍ~�̉񐔂̕\�L
                CommonValidate("PaymentSecondFrequency" + loan[i], "���[��" + loan[i] + "2��ڈȍ~�̎x����", header, false);

                string fieldName = "LoanRate" + loan[i];
                object value = CommonUtils.GetModelProperty(header, "LoanRate" + loan[i]);
                if (!ModelState.IsValidField(fieldName) || (value != null &&
                                                (!Regex.IsMatch(value.ToString(), @"^\d{1,3}\.\d{1,3}$") && !Regex.IsMatch(value.ToString(), @"^\d{1,3}$")))) {
                    ModelState.AddModelError(fieldName, MessageUtils.GetMessage("E0004", new string[] { "���[������" + loan[i], "���̐���3���ȓ�������3���ȓ�" }));
                }
            }

            //�x�����@
            for (int i = 0; i < header.CarSalesPayment.Count; i++) {
                if (string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode)) {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), MessageUtils.GetMessage("E0001", "������R�[�h"));
                }
                if (new CustomerClaimDao(db).GetByKey(header.CarSalesPayment[i].CustomerClaimCode) == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), MessageUtils.GetMessage("E0016", "������R�[�h"));
                }
                if (header.CarSalesPayment[i].PaymentPlanDate == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].PaymentPlanDate", i), MessageUtils.GetMessage("E0001", "�����\���"));
                }
                if (!ModelState.IsValidField(string.Format("pay[{0}].PaymantPlanDate", i))) {
                    ModelState.AddModelError(string.Format("pay[{0}].PaymentPlanDate", i), MessageUtils.GetMessage("E0005", "�����\���"));
                }
                if (header.CarSalesPayment[i].Amount == null) {
                    ModelState.AddModelError(string.Format("pay[{0}].Amount", i), MessageUtils.GetMessage("E0001", "�������z"));
                }
                if (Regex.IsMatch(header.CarSalesPayment[i].Amount.ToString(), @"\.")) {
                    ModelState.AddModelError(string.Format("pay[{0}].Amount", i), MessageUtils.GetMessage("E0004", new string[] { "�������z", "���̐����̂�" }));
                }
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
            if (!string.IsNullOrEmpty(header.EmployeeCode))
            {
                Employee EmployeeData = new EmployeeDao(db).GetByKey(header.EmployeeCode);
                if (EmployeeData == null)
                {
                    ModelState.AddModelError("EmployeeCode", "���͂���Ă���Ј��R�[�h�̓}�X�^�ɑ��݂��Ă��܂���B");
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

            //Add 2016/08/17 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P�@�����̔����v�Ǝx�����z���s��v�������ꍇ�A�������ɐi�߂Ȃ��悤�ɂ���
            //Mod 2016/12/08 arc nakayama #3674_�x�����z�ƌ����̔����v�̍����`�F�b�N���󒍈ȍ~�ɕύX����
            //Mod 2016/12/13 arc nakayama #3678_�ԗ��`�[�@�x�������v�ƌ����̔����v�̃o���f�[�V�����ŁA�x�������v�Ƀ��[���萔�����܂܂�Ă���
            //if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount)
            if (string.IsNullOrEmpty(header.PaymentPlanType))
            {
                if (header.GrandTotalAmount != header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && ((OrderFlag && header.SalesOrderStatus.Equals("001")) || (header.SalesOrderStatus != "001")))
                {
                    ModelState.AddModelError("", "�����̔����v�Ǝx�����z����v���Ă��܂���B");
                    ModelState.AddModelError("", "����[�������v + ����(�\�������܂�) �������̔����v�ƈ�v����悤�ɂ��Ă��������B");
                }
            }
            else
            {
                if (header.GrandTotalAmount != header.LoanPrincipalAmount + header.PaymentCashTotalAmount + header.TradeInAppropriationTotalAmount && ((OrderFlag && header.SalesOrderStatus.Equals("001")) || (header.SalesOrderStatus != "001")))
                {
                    ModelState.AddModelError("", "�����̔����v�Ǝx�����z����v���Ă��܂���B");
                    ModelState.AddModelError("", "����[�������v + ����(�\�������܂�) + �x���c�z(�����[������) �������̔����v�ƈ�v����悤�ɂ��Ă��������B");
                }
            }

            //Add 2018/01/17 arc yano #3813
            //���[���v�������I������Ă���ɂ��ւ�炸�x���c��(���[������)��0�̏ꍇ�̓��b�Z�[�W��\������
            if (!string.IsNullOrWhiteSpace(header.PaymentPlanType))
            {
                if (header.LoanPrincipalAmount == 0)
                {
                    ModelState.AddModelError("LoanPrincipalAmount", "�x���c��(���[������)�������ꍇ�A���[���v�����͐ݒ�ł��܂���B");
                }
            }

            //Add 2017/01/23 arc nakayama #3690_���q�w���\����������o�[���֓o�^������A���q�`�[�̎x�����@�ɓ������z�����o�^�ł��[�ԍςɂł��Ă��܂��B
            //�o�^�ς̎��Ƀ`�F�b�N����
            if (form["PreviousStatus"].Equals("003"))
            {
                List<CarSalesPayment> payList = header.CarSalesPayment.Distinct().ToList();


                //��������-������ꗗ�쐬�p
                List<string> JournalClaimList = new JournalDao(db).GetListBySlipNumber(header.SlipNumber, null, excluedAccountTypetList2).Select(x => x.CustomerClaimCode).Distinct().ToList();

                bool Mflag = false;
                foreach (string JClaim in JournalClaimList)
                {
                    var Ret = from a in payList
                              where a.CustomerClaimCode.Equals(JClaim)
                              select a;

                    if(Ret.Count() == 0){
                        Mflag = true;
                        break;
                    }
                }

                if (Mflag)
                {
                    ModelState.AddModelError("", "�����ς̓������т��x�����ɐݒ肳��Ă��܂���B");
                    ModelState.AddModelError("", "�x�����ɓ������тƓ���������ɑ΂���x������ݒ肵�ĉ������B");
                }
            }

            //Add 2017/04/27 arc nakayama #3744_[����] ���|�����������o�b�`���N���ƂȂ�s����C
            if (!form["PreviousStatus"].Equals("001"))
            {
                if (!string.IsNullOrWhiteSpace(header.CustomerCode) && header.CustomerCode.Equals("000001"))
                {
                    ModelState.AddModelError("CustomerCode", "�󒍈ȍ~�͌ڋq�R�[�h�ɏ�l���g�p���邱�Ƃ͂ł��܂���B");
                }

                for (int i = 0; i < header.CarSalesPayment.Count; i++)
                {
                    if (!string.IsNullOrEmpty(header.CarSalesPayment[i].CustomerClaimCode) && header.CarSalesPayment[i].CustomerClaimCode.Equals("000001"))
                    {
                        ModelState.AddModelError(string.Format("pay[{0}].CustomerClaimCode", i), "�󒍈ȍ~�͐�����ɏ�l���g�p���邱�Ƃ͂ł��܂���B");
                    }
                }
            }

            //�C���������̃o���f�[�V�����`�F�b�N
            if (header.ActionType.Equals("ModificationEnd"))
            {
                if (string.IsNullOrEmpty(header.Reason))
                {
                    ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0007", new string[] { "�C������", "���R" }));
                }
                if (!string.IsNullOrEmpty(header.Reason) && header.Reason.Length > 1024)
                {
                    ModelState.AddModelError("Reason", "���R��1024�����ȓ��œ��͂��ĉ�����");
                }
            }

            //Add 2017/11/10 arc yano #3787
            ValidateProcessLock(header, form);

            //Add 2018/08/07 yano #3911
            //�L�����Z������validation�`�F�b�N
            if (!string.IsNullOrEmpty(form["Cancel"]) && form["Cancel"].Equals("1"))
            {
                //�ڋq���擾
                Customer customer = new CustomerDao(db).GetByKey(header.CustomerCode, true);
                string customerType = customer != null ? customer.CustomerType : "";

                if (
                      !header.SalesType.Equals("003") &&     //�̔��敪=�u�Ɣ́v�ȊO
                      !header.SalesType.Equals("009") &&     //�̔��敪=�u�X�Ԉړ��v�ȊO
                      !(header.SalesType.Equals("004") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("201")) &&    //�̔��敪=�uAA�v���ڋq�敪=�uAA�v�ȊO
                      !(header.SalesType.Equals("008") && !string.IsNullOrWhiteSpace(customerType) && customerType.Equals("202"))         //�̔��敪=�u�˔p�v���ڋq�敪=�u�p���v�ȊO
                    )
                {
                    //�ԗ��̓o�^�E������Ԃ��`�F�b�N
                    CarPurchaseOrder rec = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //�o�^��Ԃ̏ꍇ
                    if (rec != null && !string.IsNullOrWhiteSpace(rec.RegistrationStatus) && rec.RegistrationStatus.Equals("1"))
                    {
                        bool cancelAuth = new ApplicationRoleDao(db).GetByKey(((Employee)Session["Employee"]).SecurityRoleCode, "ReservationCancel").EnableFlag;

                        //�o�^���������������ꍇ
                        if (!cancelAuth)
                        {
                            ModelState.AddModelError("", "�������������߁A�ԗ��o�^�ς̓`�[���L�����Z���ł��܂���B�V�X�e���ۂɈ˗����Ă�������");
                        }
                    }
                }
            }
        }

         /// <summary>
        /// �`�[���b�N�`�F�b�N
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        /// <param name="form">��ʃf�[�^</param>
        /// <history>
        /// 2018/07/31 yano.hiroki #3918�@�ŏI�X�V�����`�F�b�N���鏈����ǉ�
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\
        /// </history>
        private void ValidateProcessLock(CarSalesHeader header, FormCollection form)
        {
            // �ҏW����������ꍇ�̂݃��b�N����Ώ�
            Employee loginUser = (Employee)Session["Employee"];
            string departmentCode = header.DepartmentCode;
            string securityLevel = loginUser.SecurityRole != null ? loginUser.SecurityRole.SecurityLevelCode : "";
            // ������E��������P�`�R�E�Z�L�����e�B���x��ALL�̂ǂꂩ�ɊY�������烍�b�N����
            if (!departmentCode.Equals(loginUser.DepartmentCode)
            && !departmentCode.Equals(loginUser.DepartmentCode1)
            && !departmentCode.Equals(loginUser.DepartmentCode2)
            && !departmentCode.Equals(loginUser.DepartmentCode3)
            && !securityLevel.Equals("004"))
            {
                // �������Ȃ�
            }
            else
            {
                // �����ȊO�����b�N���Ă���ꍇ�̓G���[�\������
                string lockEmployeeName = GetProcessLockUser(header);
                if (!string.IsNullOrEmpty(lockEmployeeName))
                {
                    header.LockEmployeeName = lockEmployeeName;
                    ModelState.AddModelError("", "");
                }
                else
                {
                    //���r�W�����`�F�b�N
                    //db����ŐV�̓`�[���擾����
                    CarSalesHeader dbHeader = new CarSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

                    //Mod 2018/07/31 yano.hiroki #3918 �ŏI�X�V�����`�F�b�N����悤�ɏC��
                    if (dbHeader != null)
                    {
                        //���ꂼ��̎����̃~���b��؂�̂�
                        DateTime dbtime = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", (dbHeader.LastUpdateDate ?? DateTime.Now)));
                        DateTime formtime = DateTime.Parse(string.Format("{0:yyyy/MM/dd HH:mm:ss}", (header.LastUpdateDate ?? DateTime.Now)));

                        //���r�W�����Ⴂ�̏ꍇ
                        if (dbHeader.RevisionNumber > header.RevisionNumber)
                        {
                            ModelState.AddModelError("RevisionNumber", "�ۑ����s�����Ƃ��Ă���`�[�͍ŐV�ł͂���܂���B�ŐV�̓`�[���J����������ŕҏW���s���ĉ�����");
                        }
                        else if (dbtime > formtime)
                        {
                            ModelState.AddModelError("", "�ۑ����s�����Ƃ��Ă���`�[�͍ŐV�ł͂���܂���B�ŐV�̓`�[���J����������ŕҏW���s���ĉ�����");
                        }
                        else
                        {
                            // �`�[���b�N
                            ProcessLock(header);
                        }
                    }
                    
                }
            }
        }

        #endregion

        #region ��ʃR���|�[�l���g�̐ݒ�
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="salesCar">���f���f�[�^</param>
        /// <history>
        ///  2020/01/06 yano #4029 �i���o�[�v���[�g�i��ʁj�̒n�斈�̊Ǘ�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// </history>
        private void SetDataComponent(ref CarSalesHeader carSalesHeader)
        {
            ViewData["displayContents"] = "main";

            CodeDao dao = new CodeDao(db);
            ViewData["NewUsedTypeList"] = CodeUtils.GetSelectListByModel(dao.GetNewUsedTypeAll(false), carSalesHeader.NewUsedType, false);
            ViewData["SalesTypeList"] = CodeUtils.GetSelectListByModel<c_SalesType>(dao.GetSalesTypeAll(false), carSalesHeader.SalesType, false);
            ViewData["MileageUnit"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.MileageUnit, false);
            ViewData["TradeInMileageUnit1"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit1, false);
            ViewData["TradeInMileageUnit2"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit2, false);
            ViewData["TradeInMileageUnit3"] = CodeUtils.GetSelectListByModel<c_MileageUnit>(dao.GetMileageUnitAll(false), carSalesHeader.TradeInMileageUnit3, false);
            ViewData["RegistrationTypeList"] = CodeUtils.GetSelectListByModel<c_RegistrationType>(dao.GetRegistrationTypeAll(false), carSalesHeader.RegistrationType, false);
            ViewData["CarLiabilityInsuranceTypeList"] = CodeUtils.GetSelectListByModel<c_CarLiabilityInsuranceType>(dao.GetCarLiabilityInsuranceTypeAll(false), carSalesHeader.CarLiabilityInsuranceType, false);
            ViewData["OwnershipReservationList"] = CodeUtils.GetSelectListByModel<c_OwnershipReservation>(dao.GetOwnershipReservationAll(false), carSalesHeader.OwnershipReservation, false);
            ViewData["VoluntaryInsuranceTypeList"] = CodeUtils.GetSelectListByModel<c_VoluntaryInsuranceType>(dao.GetVoluntaryInsuranceTypeAll(false), carSalesHeader.VoluntaryInsuranceType, false);

            ViewData["TradeInEraseRegistList1"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist1, true);
            ViewData["TradeInEraseRegistList2"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist2, true);
            ViewData["TradeInEraseRegistList3"] = CodeUtils.GetSelectListByModel<c_EraseRegist>(dao.GetEraseRegistAll(false), carSalesHeader.TradeInEraseRegist3, true);

            //ADD 2014/02/21 ookubo
            ViewData["ConsumptionTaxList"] = CodeUtils.GetSelectListByModel(dao.GetConsumptionTaxList(false), carSalesHeader.ConsumptionTaxId, false);
            ViewData["ConsumptionTaxId"] = carSalesHeader.ConsumptionTaxId;
            ViewData["Rate"] = carSalesHeader.Rate;
            ViewData["ConsumptionTaxIdOld"] = carSalesHeader.ConsumptionTaxId;
            ViewData["SalesDateOld"] = carSalesHeader.SalesDate;
            ViewData["SalesPlanDateOld"] = carSalesHeader.SalesPlanDate;
            //ADD end

            //Add 2019/09/04 yano #4011
            ViewData["EPDiscountTaxList"] = CodeUtils.GetSelectListByModel(dao.GetEPDiscountTaxList(false, DateTime.Now), carSalesHeader.EPDiscountTaxId, false);

            // Mod 2015/05/18 arc nakayama ���b�N�A�b�v�������Ή��@�����f�[�^���\���͂�����(Employee/Department/Customer)
            carSalesHeader.Department = new DepartmentDao(db).GetByKey(carSalesHeader.DepartmentCode, true);
            if (carSalesHeader.Department != null)
            {
                ViewData["DepartmentName"] = carSalesHeader.Department.DepartmentName;
            }
            carSalesHeader.Employee = new EmployeeDao(db).GetByKey(carSalesHeader.EmployeeCode, true);
            if (carSalesHeader.Employee != null)
            {
                ViewData["EmployeeName"] = carSalesHeader.Employee.EmployeeName;
            }
            carSalesHeader.ExteriorColor = new CarColorDao(db).GetByKey(carSalesHeader.ExteriorColorCode);
            if (carSalesHeader.ExteriorColor != null)
            {
                ViewData["ExteriorColorName"] = carSalesHeader.ExteriorColor.CarColorName;
            }
            carSalesHeader.InteriorColor = new CarColorDao(db).GetByKey(carSalesHeader.InteriorColorCode);
            if (carSalesHeader.InteriorColor != null)
            {
                ViewData["InteriorColorName"] = carSalesHeader.InteriorColor.CarColorName;
            }
            carSalesHeader.Customer = new CustomerDao(db).GetByKey(carSalesHeader.CustomerCode, true);
            if (carSalesHeader.Customer != null)
            {
                ViewData["CustomerName"] = carSalesHeader.Customer.CustomerName; //carSalesHeader.Customer.FamilyName + "&nbsp;" + carSalesHeader.Customer.FirstName;
                ViewData["CustomerAddress"] = carSalesHeader.Customer.Prefecture + carSalesHeader.Customer.City + carSalesHeader.Customer.Address1 + carSalesHeader.Customer.Address2;
                // Add 2014/09/26 arc amii �o�^���Z���Ċm�F�`�F�b�N�Ή� #3098 �Z���Ċm�F�t���O���擾����
                ViewData["AddressReconfirm"] = carSalesHeader.Customer.AddressReconfirm;

                //Add 2014/10/29 arc amii �Z���Ċm�F���b�Z�[�W�Ή� #3119 �Z���Ċm�F�t���O�������Ă����ꍇ�A���b�Z�[�W��ݒ肷��
                if (carSalesHeader.Customer.AddressReconfirm == true)
                {
                    ViewData["ReconfirmMessage"] = "�Z�����Ċm�F���Ă�������";
                }
                else
                {
                    ViewData["ReconfirmMessage"] = "";
                }
            }
            else
            {
                ViewData["AddressReconfirm"] = false;
                ViewData["ReconfirmMessage"] = "";
            }
            carSalesHeader.Possesor = new CustomerDao(db).GetByKey(carSalesHeader.PossesorCode);
            if (carSalesHeader.Possesor != null)
            {
                ViewData["PossesorName"] = carSalesHeader.Possesor.CustomerName;
                ViewData["PossesorAddress"] = carSalesHeader.Possesor.Prefecture + carSalesHeader.Possesor.City + carSalesHeader.Possesor.Address1 + carSalesHeader.Possesor.Address2;
            }
            carSalesHeader.User = new CustomerDao(db).GetByKey(carSalesHeader.UserCode);
            if (carSalesHeader.User != null)
            {
                ViewData["UserName"] = carSalesHeader.User.CustomerName;
                ViewData["UserAddress"] = carSalesHeader.User.Prefecture + carSalesHeader.User.City + carSalesHeader.User.Address1 + carSalesHeader.User.Address2;
            }

            carSalesHeader.LoanA = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeA);
            if (carSalesHeader.LoanA != null)
            {
                ViewData["LoanNameA"] = carSalesHeader.LoanA.LoanName;
            }
            carSalesHeader.LoanB = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeB);
            if (carSalesHeader.LoanB != null)
            {
                ViewData["LoanNameB"] = carSalesHeader.LoanB.LoanName;
            }
            carSalesHeader.LoanC = new LoanDao(db).GetByKey(carSalesHeader.LoanCodeC);
            if (carSalesHeader.LoanC != null)
            {
                ViewData["LoanNameC"] = carSalesHeader.LoanC.LoanName;
            }

            carSalesHeader.c_SalesOrderStatus = dao.GetSalesOrderStatusByKey(carSalesHeader.SalesOrderStatus);

            for(int i=0;i<carSalesHeader.CarSalesLine.Count;i++)
            {
                ViewData["OptionTypeList[" + i + "]"] = CodeUtils.GetSelectListByModel(dao.GetOptionTypeAll(false), carSalesHeader.CarSalesLine[i].OptionType, false);
            }

            decimal TradeInAppropriationTotal = (carSalesHeader.TradeInAppropriation1 ?? 0) + (carSalesHeader.TradeInAppropriation2 ?? 0) + (carSalesHeader.TradeInAppropriation3 ?? 0);
            decimal PaymentCashAmount = new decimal(0);
            for (int i = 0; i < carSalesHeader.CarSalesPayment.Count; i++)
            {
                carSalesHeader.CarSalesPayment[i].CustomerClaim = new CustomerClaimDao(db).GetByKey(carSalesHeader.CarSalesPayment[i].CustomerClaimCode);
                if(carSalesHeader.CarSalesPayment[i].CustomerClaim!=null){
                    ViewData["CustomerClaimName[" + i + "]"] = carSalesHeader.CarSalesPayment[i].CustomerClaim.CustomerClaimName;
                    PaymentCashAmount += carSalesHeader.CarSalesPayment[i].Amount ?? 0;
                }
            }

            ViewData["TradeInAppropriationTotal"] = TradeInAppropriationTotal;
            ViewData["PaymentTotalAmount"] = carSalesHeader.GrandTotalAmount - TradeInAppropriationTotal;
            ViewData["PaymentCashAmount"] = PaymentCashAmount;
            ViewData["PaymentRemainAmount"] = carSalesHeader.GrandTotalAmount - TradeInAppropriationTotal - PaymentCashAmount;

            carSalesHeader = CalcAmount(carSalesHeader);

            carSalesHeader.BasicHasErrors = BasicHasErrors();
            carSalesHeader.UsedHasErrors = UsedHasErrors();
            carSalesHeader.InvoiceHasErrors = InvoiceHasErrors();
            carSalesHeader.LoanHasErrors = LoanHasErrors();
            carSalesHeader.VoluntaryInsuranceHasErrors = VoluntaryInsuranceHasErrors();

            //����Ԃ��A�d���ςȂ�ҏW�s�ɂ���
            for (int i = 1; i <= 3; i++)
            {
                ViewData["TradeInVinLock" + i] = "0";
                object vin = CommonUtils.GetModelProperty(carSalesHeader, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrWhiteSpace(vin.ToString()))
                {
                    CarPurchase CarPurchaseData = new CarPurchaseDao(db).GetBySlipNumberVin(carSalesHeader.SlipNumber, vin.ToString());
                    if (CarPurchaseData != null)
                    {
                        //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
                        if (CarPurchaseData.PurchaseStatus == "002" && !new InventoryScheduleDao(db).IsClosedInventoryMonth(CarPurchaseData.DepartmentCode, CarPurchaseData.PurchaseDate, "001", ((Employee)Session["Employee"]).SecurityRoleCode))
                        {

                            ViewData["TradeInVinLock" + i] = "1";
                        }
                    }
                }
            }

            //Add 2017/06/29 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs
            //���쑮�����N���A����
            carSalesHeader.ActionType = "";

            //Del 2017/03/28 arc nakayama #3739_�ԗ��`�[�E�ԗ�����E�ԗ��d���̘A���p�~
            //Add 2017/01/16 arc nakayama #3689_�y�l���R��z�[�ԍό�ɉ���Ԃ̎d�����s���ƁA�[�ԍς݂̓`�[�ɋ��z�����f����Ă��܂�
            //�Ō�ɋ��z�̕ϓ�����������ʂ��ԗ��d����ʂłȂ���΃��b�Z�[�W�\��
            /*if (!carSalesHeader.LastEditScreen.Equals(LAST_EDIT_CARSALSEORDER))
            {
                switch (carSalesHeader.LastEditScreen)
                {
                    case "002":
                        carSalesHeader.LastEditMessage = "�����ʂ��牺��ԏ[�����A���ŏ[���A���T�C�N���̊e���z���ύX����܂����B";
                        break;
                    case "003":
                        carSalesHeader.LastEditMessage = "�ԗ��d����ʂ��牺��ԏ[�����A���ŏ[���A���T�C�N���̊e���z���ύX����܂����B";
                        break;
                    default:
                        carSalesHeader.LastEditMessage = "";
                        break;
                }

            }
            else
            {
                carSalesHeader.LastEditMessage = "";
            }*/


            //Add 2017/11/21 arc yano 
            //�C�x���g�P
            carSalesHeader.CampaignName1 = new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode1) != null ? new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode1).CampaignName : "";

            //�C�x���g�Q
            carSalesHeader.CampaignName2 = new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode2) != null ? new CampaignDao(db).GetByKey(carSalesHeader.CampaignCode2).CampaignName : "";

            //Add 2020/01/06 yano #4029
            carSalesHeader.CostArea = new CostAreaDao(db).GetByKey(carSalesHeader.CostAreaCode);
        }
    #endregion

    #region �v�Z���W�b�N
    /// <summary>
    /// ���v���z���v�Z����
    /// </summary>
    /// <param name="header">�ԗ��`�[�f�[�^</param>
    /// <returns>���v���z���㏑�������f�[�^</returns>
    /// <history>
    /// 2023/09/18 yano #4181�y�ԗ��`�[�z�I�v�V�����敪�ǉ��i�T�[�`���[�W�j
    /// 2023/09/05 yano #4162 �C���{�C�X�Ή�
    /// 2023/08/15 yano #4176 �̔�����p�̏C��
    /// 2023/01/11 yano #4158 �y�ԗ��`�[���́z�C�ӕی������͍��ڂ̒ǉ�
    /// 2022/08/30 yano #4150�y�ԗ��`�[���́z���������ς̃��[���������X�V�����s��̑Ή� �������f�̕ύX
    /// 2021/06/09 yano #4091 �y�ԗ��`�[�z�I�v�V�����s�̋敪�ǉ�(�����e�i�X�E�����ۏ�)
    /// 2014/07/25 arc amii �ۑ�Ή� #3018 ��ېō��v�Ɏ����󎆑�Ɖ��掩���Ԑŗa�������ǉ�
    /// </history>
    private CarSalesHeader CalcAmount(CarSalesHeader header) {

            #region �I�v�V�������v�v�Z
            decimal shopOptionTotal = new decimal(0); //�̔��X�I�v�V�������v
            decimal makerOptionTotal = new decimal(0); //���[�J�[�I�v�V�������v
            decimal shopOptionTaxTotal = new decimal(0); //�̔��X�I�v�V��������ō��v
            decimal makerOptionTaxTotal = new decimal(0); //���[�J�[�I�v�V��������ō��v

            //Add 2021/06/09 #4091 yano
            decimal maintenancePackageAmount = 0;
            decimal maintenancePackageTaxAmount = 0;
            decimal extendedWarrantyAmount = 0;
            decimal extendedWarrantyTaxAmount = 0;

            //Add 2023/09/18 yano #4181
            decimal surchargeAmount = 0;
            decimal surchargeTaxAmount = 0;

            foreach (var a in header.CarSalesLine) {
                switch(a.OptionType){
                    
                    //Mod 2023/09/18 yano #4181
                   //case "001": //�̔��X
                   //     shopOptionTotal += a.Amount ?? 0;
                   //     shopOptionTaxTotal += a.TaxAmount ?? 0;
                   //     break;

                    case "002": //���[�J�[
                        makerOptionTotal += a.Amount ?? 0;
                        makerOptionTaxTotal += a.TaxAmount ?? 0;
                        break;

                    //Add 2021/06/09 yano #4091
                    case "004": //�����e�i���X
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      maintenancePackageAmount += a.Amount ?? 0;
                      maintenancePackageTaxAmount += a.TaxAmount ?? 0;
                      break;

                    case "005": //�����ۏ�
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      extendedWarrantyAmount += a.Amount ?? 0;
                      extendedWarrantyTaxAmount += a.TaxAmount ?? 0;
                      break;

                     //Add 2023/09/18 yano #4181
                     case "006": //�T�[�`���[�W
                      shopOptionTotal += a.Amount ?? 0;
                      shopOptionTaxTotal += a.TaxAmount ?? 0;
                      surchargeAmount += a.Amount ?? 0;
                      surchargeTaxAmount += a.TaxAmount ?? 0;
                      break;

                     //Add 2023/09/18 yano #4181
                     default: //�̔��X(001)�A�b��ǉ���
                        shopOptionTotal += a.Amount ?? 0;
                        shopOptionTaxTotal += a.TaxAmount ?? 0;
                        break;
                }
            }
            //�I�v�V�������v���X�V
            //����ŗ������ǉ��@2014/02/20 ookubo
            header.ShopOptionAmount = shopOptionTotal;
            header.MakerOptionAmount = makerOptionTotal;
            //header.ShopOptionAmount = CommonUtils.CalculateConsumptionTax(shopOptionTotal, header.Rate);
            //header.MakerOptionAmount = CommonUtils.CalculateConsumptionTax(makerOptionTotal, header.Rate);
            //header.ShopOptionAmount = shopOptionTotal;
            //header.MakerOptionAmount = makerOptionTotal;

            //�I�v�V��������ł��X�V
            //����ŗ������ǉ��@2014/02/20 ookubo
            header.ShopOptionTaxAmount = shopOptionTaxTotal;
            header.MakerOptionTaxAmount = makerOptionTaxTotal;
            //header.ShopOptionTaxAmount = CommonUtils.CalculateConsumptionTax(shopOptionTaxTotal, header.Rate);
            //header.MakerOptionTaxAmount = CommonUtils.CalculateConsumptionTax(makerOptionTaxTotal, header.Rate);
            //header.ShopOptionTaxAmount = shopOptionTaxTotal;
            //header.MakerOptionTaxAmount = makerOptionTaxTotal;

            #endregion

            #region �ŋ������v
            header.TaxFreeTotalAmount = (header.CarTax ?? 0) + (header.CarLiabilityInsurance ?? 0) + (header.AcquisitionTax ?? 0) + (header.CarWeightTax ?? 0);
            #endregion

            #region ���̑���ېō��v
            // Mod 2014/07/25 arc amii
            header.OtherCostTotalAmount = (header.InspectionRegistCost ?? 0) + (header.ParkingSpaceCost ?? 0)
                + (header.TradeInCost ?? 0) + (header.RecycleDeposit ?? 0) //+ (header.RecycleDepositTradeIn ?? 0)
                + (header.NumberPlateCost ?? 0) + (header.RequestNumberCost ?? 0) //+ (header.TradeInFiscalStampCost ?? 0)
                + (header.RevenueStampCost ?? 0)
                + (header.TradeInCarTaxDeposit ?? 0)
                + (header.TaxFreeFieldValue ?? 0)
                + (header.VoluntaryInsuranceAmount ?? 0);   // Add 2023/01/11 yano #4158
            #endregion

            #region �̔�����p
            //Mod 2023/08/15 yano #4176 comment out
            //header.InspectionRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.InspectionRegistFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.PreparationFeeTax = CommonUtils.CalculateConsumptionTax(header.PreparationFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.FarRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.FarRegistFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.RecycleControlFeeTax = CommonUtils.CalculateConsumptionTax(header.RecycleControlFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.ParkingSpaceFeeTax = CommonUtils.CalculateConsumptionTax(header.ParkingSpaceFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.RequestNumberFeeTax = CommonUtils.CalculateConsumptionTax(header.RequestNumberFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.TradeInMaintenanceFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInMaintenanceFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.TradeInFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.TradeInAppraisalFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInAppraisalFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.InheritedInsuranceFeeTax = CommonUtils.CalculateConsumptionTax(header.InheritedInsuranceFee, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo
            //header.TaxationFieldValueTax = CommonUtils.CalculateConsumptionTax(header.TaxationFieldValue, header.Rate);//����ŗ������ǉ��@2014/02/20 ookubo

            //header.InspectionRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.InspectionRegistFee);
            //header.PreparationFeeTax = CommonUtils.CalculateConsumptionTax(header.PreparationFee);
            //header.FarRegistFeeTax = CommonUtils.CalculateConsumptionTax(header.FarRegistFee);
            //header.RecycleControlFeeTax = CommonUtils.CalculateConsumptionTax(header.RecycleControlFee);
            //header.ParkingSpaceFeeTax = CommonUtils.CalculateConsumptionTax(header.ParkingSpaceFee);
            //header.RequestNumberFeeTax = CommonUtils.CalculateConsumptionTax(header.RequestNumberFee);
            //header.TradeInMaintenanceFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInMaintenanceFee);
            //header.TradeInFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInFee);
            //header.TradeInAppraisalFeeTax = CommonUtils.CalculateConsumptionTax(header.TradeInAppraisalFee);
            //header.InheritedInsuranceFeeTax = CommonUtils.CalculateConsumptionTax(header.InheritedInsuranceFee);
            //header.TaxationFieldValueTax = CommonUtils.CalculateConsumptionTax(header.TaxationFieldValue);

            header.SalesCostTotalAmount = (header.InspectionRegistFee ?? 0) + (header.PreparationFee ?? 0) + (header.FarRegistFee ?? 0)
                                        + (header.RecycleControlFee ?? 0) + (header.ParkingSpaceFee ?? 0) + (header.RequestNumberFee ?? 0)
                                        + (header.TradeInMaintenanceFee ?? 0) + (header.TradeInFee ?? 0)
                                        + (header.OutJurisdictionRegistFee ?? 0) + (header.InheritedInsuranceFee ?? 0) + (header.TaxationFieldValue ?? 0)
                                        + (header.CarTaxUnexpiredAmount ?? 0) + (header.CarLiabilityInsuranceUnexpiredAmount ?? 0);                   //Add 2023/09/05  #4162
                                        //+ (header.TradeInAppraisalFee ?? 0) + (header.InheritedInsuranceFee ?? 0) + (header.TaxationFieldValue ?? 0);

            //�̔�����p�����
            header.SalesCostTotalTaxAmount = (header.InspectionRegistFeeTax ?? 0) + +(header.PreparationFeeTax ?? 0) + (header.FarRegistFeeTax ?? 0)
                                           + (header.RecycleControlFeeTax ?? 0) + (header.ParkingSpaceFeeTax ?? 0) + (header.RequestNumberFeeTax ?? 0)
                                           + (header.TradeInMaintenanceFeeTax ?? 0) + (header.TradeInFeeTax ?? 0)
                                           + (header.OutJurisdictionRegistFeeTax ?? 0) + (header.InheritedInsuranceFeeTax ?? 0) + (header.TaxationFieldValueTax ?? 0)
                                           + (header.CarTaxUnexpiredAmountTax ?? 0) + (header.CarLiabilityInsuranceUnexpiredAmountTax ?? 0);                   //Add 2023/09/05  #4162
                                           //+ (header.TradeInAppraisalFeeTax ?? 0) + (header.InheritedInsuranceFeeTax ?? 0) + (header.TaxationFieldValueTax ?? 0);
            #endregion

            //����p���v
            header.CostTotalAmount = (header.TaxFreeTotalAmount ?? 0) + (header.OtherCostTotalAmount ?? 0) + (header.SalesCostTotalAmount ?? 0);

            //�ԗ��{�̉��i
            header.SalesPrice = header.SalesPrice ?? 0;
            header.SalesTax = header.SalesTax ?? 0;
            
            //�l���z
            header.DiscountAmount = header.DiscountAmount ?? 0;
            header.DiscountTax = header.DiscountTax ?? 0;

            //�ېőΏۊz
            header.TaxationAmount = header.SalesPrice - header.DiscountAmount + header.MakerOptionAmount;

            //�ԗ��̔����i���v
            header.SubTotalAmount = shopOptionTotal + makerOptionTotal + header.SalesPrice - header.DiscountAmount;

            //����ō��v���X�V
            header.TotalTaxAmount = (header.SalesTax ?? 0) - (header.DiscountTax ?? 0) + (header.ShopOptionTaxAmount ?? 0) + (header.MakerOptionTaxAmount ?? 0)
                    + (header.OutSourceTaxAmount ?? 0) + (header.SalesCostTotalTaxAmount ?? 0);

            //�������v
            header.GrandTotalAmount = (header.SubTotalAmount ?? 0) + (header.CostTotalAmount ?? 0) + (header.TotalTaxAmount ?? 0);

            //�x�����@�ύX���p
            decimal paymentAmount = new decimal(0);
            foreach (var a in header.CarSalesPayment) {
                paymentAmount += a.Amount ?? 0;
            }
            header.PaymentCashTotalAmount = paymentAmount;
            
            //���[�����I������Ă���ꍇ�A���[����Ђ��炷�łɓ����ς݂��ǂ������`�F�b�N����
            Loan LoanData = new Loan();

            if(!string.IsNullOrEmpty(header.LoanCodeA)){

                LoanData = new LoanDao(db).GetByKey(header.LoanCodeA, false);
            }
            else if (!string.IsNullOrEmpty(header.LoanCodeB))
            {
                LoanData = new LoanDao(db).GetByKey(header.LoanCodeB, false);

            }
            else if (!string.IsNullOrEmpty(header.LoanCodeC))
            {
                LoanData = new LoanDao(db).GetByKey(header.LoanCodeB, false);
            }
            else
            {
                //���[���I�����A���[���R�[�h�͕K�{�Ȃ̂ł����ɓ��B���邱�Ƃ͂Ȃ�
                LoanData = null;
            }

            if (LoanData != null)
            {
                //�Y���`�[�̃��[���̓����\����擾����
                ReceiptPlan LoanDataPlan = new ReceiptPlanDao(db).GetByCustomerClaim(header.SlipNumber, LoanData.CustomerClaimCode);
                if (LoanDataPlan != null)
                {
                    if (!string.IsNullOrEmpty(LoanDataPlan.CompleteFlag) && !LoanDataPlan.CompleteFlag.Equals("1"))
                    {
                        header.LoanPrincipalAmount = header.PaymentTotalAmount - paymentAmount;
                    }
                }
                else
                {
                    //�������Ȃ��i��ʏ�̒l���g�p�j
                }
            }
            else
            {
                header.LoanPrincipalAmount = header.PaymentTotalAmount - paymentAmount;
            }

            header.LoanPrincipalA = header.LoanPrincipalAmount;
            header.LoanPrincipalB = header.LoanPrincipalAmount;
            header.LoanPrincipalC = header.LoanPrincipalAmount;
            header.LoanTotalAmountA = (header.LoanFeeA ?? 0) + (header.LoanPrincipalA ?? 0);
            header.LoanTotalAmountB = (header.LoanFeeB ?? 0) + (header.LoanPrincipalB ?? 0);
            header.LoanTotalAmountC = (header.LoanFeeC ?? 0) + (header.LoanPrincipalC ?? 0);
            return header;
        }
        #endregion

        #region ���`�[�̓����\��쐬�O�ɓ������т����`�[�ɐU��Ԃ�
        /// <summary>
        /// ���`�[�̓����\��쐬�O�ɓ������т����`�[�ɐU��Ԃ�
        /// </summary>
        /// <param name="SlipNumber">���`�[�ԍ�</param>
        /// <history>
        /// Add 2016/09/29 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P
        /// </history>
        private void TransferJournal(string SlipNumber)
        {
            //���`�[�̓`�[�ԍ��Ō������邽��replace����
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //���`�[�̓������т�S�č��`�[�ɐU��ւ���
            List<Journal> OriginalJournalList = new JournalDao(db).GetListBySlipNumber(OriginalSlipNumber);

            foreach (var OriginJournal in OriginalJournalList)
            {
                OriginJournal.SlipNumber = SlipNumber; //���`�[�ԍ�
                OriginJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                OriginJournal.LastUpdateDate = DateTime.Now;
            }

            db.SubmitChanges();

            //���`�[�̓������т��S�č��ɐU��ւ�������߁A���Ƃ̓����\��̎c����S�ē����O�ɖ߂�
            List<ReceiptPlan> OriginalPlanList = new ReceiptPlanDao(db).GetBySlipNumber(OriginalSlipNumber);

            foreach (var OriginalPlan in OriginalPlanList)
            {
                OriginalPlan.ReceivableBalance = OriginalPlan.Amount;
                OriginalPlan.CompleteFlag = "1"; //���`�[�ɑ΂��ē��������Ȃ����ߊ����t���O�𗧂Ă�
                OriginalPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                OriginalPlan.LastUpdateDate = DateTime.Now;
            }
        }

        #endregion


        #region ���`�[�쐬�O�ɉ���ԂƎc�̓������т����`�[�ɐU��ւ���
        /// <summary>
        /// ���`�[�̓����\��쐬�O�ɓ������т����`�[�ɐU��Ԃ�
        /// </summary>
        /// <param name="SlipNumber">���`�[�ԍ�</param>
        /// <history>
        /// 2017/11/14 arc yano #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s����
        /// 2016/09/29 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P �V�K�쐬
        /// </history>
        private void TransfeTraderJournal(CarSalesHeader header)
        {
            //���`�[�̓`�[�ԍ��Ō������邽��replace����
            string OriginalSlipNumber = header.SlipNumber.Replace("-2", "");

            for (int i = 1; i <= 3; i++)
            {
                object vin = CommonUtils.GetModelProperty(header, "TradeInVin" + i);
                if (vin != null && !string.IsNullOrEmpty(vin.ToString()))
                {
                    string TradeInAmount = "0";
                    string TradeInRemainDebt = "0";

                    var RetTradeInAmount = CommonUtils.GetModelProperty(header, "TradeInAmount" + i);
                    if (RetTradeInAmount != null)
                    {
                        TradeInAmount = RetTradeInAmount.ToString();
                    }

                    var RetTradeInRemainDebt = CommonUtils.GetModelProperty(header, "TradeInRemainDebt" + i);
                    if (RetTradeInRemainDebt != null)
                    {
                        TradeInRemainDebt = RetTradeInRemainDebt.ToString();
                    }

                    decimal PlanAmount = decimal.Parse(TradeInAmount);
                    decimal PlanRemainDebt = decimal.Parse(TradeInRemainDebt) * (-1);

                    //Mod 2017/11/14 arc yano #3811 
                    //����̓������ю擾
                    Journal OriginalJournalData = new JournalDao(db).GetTradeJournal(OriginalSlipNumber, "013", vin.ToString()).FirstOrDefault();
                    //Journal OriginalJournalData = new JournalDao(db).GetListByCustomerAndSlip(OriginalSlipNumber, header.CustomerCode).Where(x => x.AccountType == "013" && x.Amount.Equals(PlanAmount)).FirstOrDefault();
                    if (OriginalJournalData != null)
                    {
                        OriginalJournalData.SlipNumber = header.SlipNumber;
                        OriginalJournalData.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        OriginalJournalData.LastUpdateDate = DateTime.Now;
                    }

                    //Mod 2017/11/14 arc yano #3811
                    //�c�̓����z�擾
                    Journal OriginalJournalData2 = new JournalDao(db).GetTradeJournal(OriginalSlipNumber, "012", vin.ToString()).FirstOrDefault();
                    //Journal OriginalJournalData2 = new JournalDao(db).GetListByCustomerAndSlip(OriginalSlipNumber, header.CustomerCode).Where(x => x.AccountType == "012" && x.Amount.Equals(PlanRemainDebt)).FirstOrDefault();
                    if (OriginalJournalData2 != null)
                    {
                        OriginalJournalData2.SlipNumber = header.SlipNumber;
                        OriginalJournalData2.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        OriginalJournalData2.LastUpdateDate = DateTime.Now;
                    }
                }
            }
        }

        #endregion

        #region �J�[�h��Ђ���̓����\������`�[���ō쐬����
        /// <summary>
        /// �J�[�h��Ђ���̓����\������`�[���ō쐬����
        /// </summary>
        /// <param name="SlipNumber">���`�[�ԍ�</param>
        /// <history>
        /// Add 2016/05/25 arc nakayama #3418_�ԍ��`�[���s���̍��`�[�̓����\��iReceiptPlan�j�̎c���̌v�Z���@
        /// </history>
        private void CreateKuroCreditPlan(string SlipNumber)
        {
            //���`�[�̓`�[�ԍ��Ō������邽��replace����
            string OriginalSlipNumber = SlipNumber.Replace("-2", "");

            //�J�[�h��Ђ���̓����\��擾
            List<ReceiptPlan> CreditPlanList = new ReceiptPlanDao(db).GetCashBySlipNumber(OriginalSlipNumber, "011");

            foreach (var CreditPlan in CreditPlanList)
            {
                decimal? ReceivableBalance = CreditPlan.ReceivableBalance;
                //�J�[�h��Ђ���̓����\��ɑ΂��ē������т��������ꍇ�͎c�����X�V����
                Journal JournalData = new JournalDao(db).GetByPlanIDAccountType(CreditPlan.ReceiptPlanId.ToString().ToUpper(), "011");
                if (JournalData != null)
                {
                    ReceivableBalance -= JournalData.Amount;
                }

                ReceiptPlan KuroCreditPlan = new ReceiptPlan();
                KuroCreditPlan.ReceiptPlanId = Guid.NewGuid();
                KuroCreditPlan.DepartmentCode = CreditPlan.DepartmentCode;
                KuroCreditPlan.OccurredDepartmentCode = CreditPlan.OccurredDepartmentCode;
                KuroCreditPlan.CustomerClaimCode = CreditPlan.CustomerClaimCode;
                KuroCreditPlan.SlipNumber = SlipNumber;
                KuroCreditPlan.ReceiptType = CreditPlan.ReceiptType;
                KuroCreditPlan.ReceiptPlanDate = CreditPlan.ReceiptPlanDate;
                KuroCreditPlan.AccountCode = CreditPlan.AccountCode;
                KuroCreditPlan.Amount = CreditPlan.Amount;
                KuroCreditPlan.ReceivableBalance = ReceivableBalance;
                if (ReceivableBalance.Equals(0m))
                {
                    KuroCreditPlan.CompleteFlag = "1";
                }
                else
                {
                    KuroCreditPlan.CompleteFlag = "0";
                }
                KuroCreditPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                KuroCreditPlan.CreateDate = DateTime.Now;
                KuroCreditPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                KuroCreditPlan.LastUpdateDate = DateTime.Now;
                KuroCreditPlan.DelFlag = "0";
                KuroCreditPlan.Summary = CreditPlan.Summary;
                KuroCreditPlan.JournalDate = CreditPlan.JournalDate;
                KuroCreditPlan.DepositFlag = CreditPlan.DepositFlag;
                KuroCreditPlan.PaymentKindCode = CreditPlan.PaymentKindCode;
                KuroCreditPlan.CommissionRate = CreditPlan.CommissionRate;
                KuroCreditPlan.CommissionAmount = CreditPlan.CommissionAmount;
                KuroCreditPlan.CreditJournalId = CreditPlan.CreditJournalId;
                db.ReceiptPlan.InsertOnSubmit(KuroCreditPlan);

                //�J�[�h�A�J�[�h��Ђ���̓����A�̎��т��������ꍇ�����\��ID��V���������\��ID�ɍX�V����
                List<Journal> CardJournalList = new JournalDao(db).GetByReceiptPlanID(CreditPlan.ReceiptPlanId.ToString());
                if (CardJournalList != null)
                {
                    foreach (var CardJournal in CardJournalList)
                    {
                        CardJournal.CreditReceiptPlanId = KuroCreditPlan.ReceiptPlanId.ToString().ToUpper();
                        CardJournal.LastUpdateDate = DateTime.Now;
                        CardJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    }
                }

                //�J�[�h��Ђ���̓������т��U��ւ��邽�߁A���`�[�́u�J�[�h��Ђ���̓����v�ɑ΂�����тƂ̕R�t�����폜
                CreditPlan.CreditJournalId = "";
            }



        }
        #endregion

        //Add 2014/05/20 arc yano  vs2008��2012�Ή��@�I�v�V�����s�̃o�C���h���s���ɁA����POST�f�[�^���I�v�V�����s�f�[�^���擾����B
        #region Request���I�v�V�����s�̃f�[�^�擾(�I�v�V����)
        private EntitySet<CarSalesLine> getCarSalesLinebyReq(){
            EntitySet<CarSalesLine> etmpline = new EntitySet<CarSalesLine>();

            int lineCount = 0;                              //���׍s��
            int i;                                          //�J�E���^

            //���݂̃I�v�V�����s�̍s�����擾
            if (!string.IsNullOrEmpty(Request["LineCount"]))
            {
                lineCount = Int32.Parse(Request["LineCount"]);
            }
            else
            {
                lineCount = 0;      //0�s�Ƃ���B
            }

            //------------------------------------------------
            //POST�f�[�^���I�v�V�����s�f�[�^���擾����B
            //------------------------------------------------
            for (i = 0; i<lineCount; i++)
            {

                CarSalesLine tmpline = new CarSalesLine();
                string lineprefix = string.Format("line[{0}].", i);

                if (!string.IsNullOrEmpty(Request["Slipnumber"]))                                        //�`�[�ԍ�
                {
                    tmpline.SlipNumber = Request["Slipnumber"];
                }

                if (!string.IsNullOrEmpty(Request["RevisionNumber"]))                                   //�����ԍ�
                {
                    tmpline.RevisionNumber = Int32.Parse(Request["RevisionNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "LineNumber"]))                          //�s�ԍ�
                {
                    tmpline.LineNumber = Int32.Parse(Request[lineprefix + "LineNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CarOptionCode"]))                       //�ԗ��I�v�V�����R�[�h
                {
                    tmpline.CarOptionCode = Request[lineprefix + "CarOptionCode"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CarOptionName"]))                       //�ԗ��I�v�V������
                {
                    tmpline.CarOptionName = Request[lineprefix + "CarOptionName"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "OptionType"]))                          //�I�v�V�������
                {
                    tmpline.OptionType = Request[lineprefix + "OptionType"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Amount"]))                              //�̔��P��
                {
                    tmpline.Amount = Decimal.Parse(Request[lineprefix + "Amount"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "TaxAmount"]))                           //�����
                {
                    tmpline.TaxAmount = Decimal.Parse(Request[lineprefix + "TaxAmount"]);
                }

                if (!string.IsNullOrEmpty(Request["Rate"]))                                             //����ŗ�
                {
                    tmpline.Rate = Int32.Parse(Request["Rate"]);
                }

                etmpline.Add(tmpline);    //�R���N�V�����ɍs�ǉ�
            }

            return etmpline;
        }
        #endregion

 
        //Add 2014/05/20 arc yano  vs2008��2012�Ή��@�I�v�V�����s�̃o�C���h���s���ɁA����POST�f�[�^���x�����@�̍s�f�[�^���擾����B
        #region Request���x�����̍s�f�[�^�擾
        private EntitySet<CarSalesPayment> getCarSalesPaymentbyReq()
        {
            EntitySet<CarSalesPayment> etmpline = new EntitySet<CarSalesPayment>();
            

            int lineCount = 0;                              //���׍s��
            int i;                                          //�J�E���^

            //�x�����̌��݂̍s�����擾
            if (!string.IsNullOrEmpty(Request["PayLineCount"]))
            {
                lineCount = Int32.Parse(Request["PayLineCount"]);
            }
            else
            {
                lineCount = 0;      //0�s�Ƃ���B
            }
            //------------------------------------------------
            //POST�f�[�^���x�����s�f�[�^���擾����B
            //------------------------------------------------
            for (i = 0; i < lineCount; i++)
            {
                CarSalesPayment tmpline = new CarSalesPayment();
                string lineprefix = string.Format("pay[{0}].", i);

                if (!string.IsNullOrEmpty(Request["Slipnumber"]))                                        //�`�[�ԍ�
                {
                    tmpline.SlipNumber = Request["Slipnumber"];
                }

                if (!string.IsNullOrEmpty(Request["RevisionNumber"]))                                   //�����ԍ�
                {
                    tmpline.RevisionNumber = Int32.Parse(Request["RevisionNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "LineNumber"]))                          //�s�ԍ�
                {
                    tmpline.LineNumber = Int32.Parse(Request[lineprefix + "LineNumber"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "CustomerClaimCode"]))                   //������R�[�h
                {
                    tmpline.CustomerClaimCode = Request[lineprefix + "CustomerClaimCode"];
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "PaymentPlanDate"]))                    //�����\���
                {
                    tmpline.PaymentPlanDate = DateTime.Parse(Request[lineprefix + "PaymentPlanDate"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Amount"]))                              //�����\����z
                {
                    tmpline.Amount = Decimal.Parse(Request[lineprefix + "Amount"]);
                }

                if (!string.IsNullOrEmpty(Request[lineprefix + "Memo"]))                                //����
                {
                    tmpline.Memo = Request[lineprefix + "Memo"];
                }

                etmpline.Add(tmpline);    //�R���N�V�����ɍs�ǉ�
            }

            return etmpline;
        }
        #endregion

        #region �^�u���Ƃ̃G���[�d��
        private bool BasicHasErrors() {
            var query = from a in ModelState
                        where (!a.Key.StartsWith("TradeIn")
                        && !a.Key.StartsWith("pay[")
                        && !a.Key.Equals("LoanPrincipalAmount")
                        && !a.Key.EndsWith("A")
                        && !a.Key.EndsWith("B")
                        && !a.Key.EndsWith("C"))
                        && a.Value.Errors.Count() > 0
                        select a;
            return query != null && query.Count() > 0;
        }

        private bool UsedHasErrors() {
            var query = from a in ModelState
                        where a.Key.Contains("TradeIn") && a.Value.Errors.Count()>0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        private bool InvoiceHasErrors() {
            var query = from a in ModelState
                        where (a.Key.Contains("pay[") || a.Key.Equals("LoanPrincipalAmount"))
                        && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        private bool LoanHasErrors() {
            var query = from a in ModelState
                        where (a.Key.EndsWith("A") && a.Value.Errors.Count() > 0)
                        || (a.Key.EndsWith("B") && a.Value.Errors.Count() > 0)
                        || (a.Key.EndsWith("C") && a.Value.Errors.Count() > 0)
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;

        }
        private bool VoluntaryInsuranceHasErrors() {
            var query = from a in ModelState
                        where a.Key.StartsWith("VoluntaryInsurance") && a.Value.Errors.Count() > 0
                        select a;
            if (query != null && query.Count() > 0) {
                return true;
            }
            return false;
        }
        #endregion

        #region  �`�[���C�����ɂ���i���R�[�h�쐬�j
        /// <summary>
        /// �`�[���C�����ɂ���i���R�[�h�쐬�j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�Ȃ�</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public void ModificationStart(CarSalesHeader header)
        {
            ModificationControl Modification = new ModificationControl();
            Modification.SlipNumber = header.SlipNumber;
            Modification.RevisionNumber = header.RevisionNumber;
            Modification.SlipType = "0";
            Modification.SalesDate = header.SalesDate;
            Modification.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            Modification.CreateDate = DateTime.Now;
            Modification.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            Modification.LastUpdateDate = DateTime.Now;
            Modification.DelFlag = "0";
            db.ModificationControl.InsertOnSubmit(Modification);
            db.SubmitChanges();

        }
        #endregion

        #region  �`�[�̏C�����L�����Z������i�C�����s���Ȃ��悤�ɂ���j
        /// <summary>
        /// �`�[�̏C�����L�����Z������i�C�����s���Ȃ��悤�ɂ���j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�Ȃ�</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public void ModificationCancel(CarSalesHeader header)
        {
            List<ModificationControl> ModifiRet = new CarSalesOrderDao(db).GetModificationStatusAll(header.SlipNumber);
            if (ModifiRet != null)
            {
                foreach (var ModRet in ModifiRet)
                {
                    db.ModificationControl.DeleteOnSubmit(ModRet);

                }
                db.SubmitChanges();
            }
        }
        #endregion

        #region �ߋ��ɏC���������s�����`�[�łȂ����`�F�b�N����i�C����������:True  �C�������Ȃ�:False�j
        /// <summary>
        /// �ߋ��ɏC���������s�����`�[�łȂ����`�F�b�N����i�C����������:True  �C�������Ȃ�:False�j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�C����������:True  �C�������Ȃ�:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public bool CheckModifiedReason(string SlipNumber)
        {
            ModifiedReason ModifiedRec = new CarSalesOrderDao(db).GetLatestModifiedReason(SlipNumber);

            if (ModifiedRec != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region �C���������擾����i�Y���`�[�̑S�����j
        /// <summary>
        /// �C���������擾����i�Y���`�[�̑S�����j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�C�������i�C�����ԁE�C���ҁE�C�����R�j</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public void GetModifiedHistory(CarSalesHeader header)
        {
            List<ModifiedReason> ModifiedRec = new CarSalesOrderDao(db).GetModifiedReason(header.SlipNumber);
            header.ModifiedReasonList = new List<CarSalesModifiedReason>();

            if (ModifiedRec != null)
            {
                foreach (var Mod in ModifiedRec)
                {
                    CarSalesModifiedReason ModData = new CarSalesModifiedReason();
                    ModData.ModifiedTime = Mod.CreateDate;
                    ModData.ModifiedEmployeeName = new EmployeeDao(db).GetByKey(Mod.CreateEmployeeCode).EmployeeName;
                    ModData.ModifiedReason = Mod.Reason;
                    header.ModifiedReasonList.Add(ModData);
                }
            }
        }
        #endregion

        #region  �C�������ǂ����̃`�F�b�N�i�C����:True  ����ȊO:False�j
        /// <summary>
        /// �C�������ǂ����̃`�F�b�N�i�C����:True  ����ȊO:False�j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�C����:True  ����ȊO:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history> 
        public bool CheckModification(string SlipNumber, int RevisionNumber)
        {
            ModificationControl ModifiRet = new CarSalesOrderDao(db).GetModificationStatus(SlipNumber, RevisionNumber);

            if (ModifiRet != null)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region  �����̃`�F�b�N
        /// <summary>
        /// �����̃`�F�b�N
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�`�[�C���������������Ă����:True  ����ȊO:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public bool CheckApplicationRole(string EmployeeCode)
        {
            //���O�C�����[�U���擾
            Employee loginUser = new EmployeeDao(db).GetByKey(EmployeeCode);
            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "SlipModification"); //�`�[�C�������������邩�Ȃ���

            // �`�[�C���������������true�����łȂ����false
            if (AppRole.EnableFlag)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region  �ߋ��ɐԍ��������s�������`�[�łȂ����`�F�b�N����i�ԍ��o���Ȃ�:True  ����ȊO:False�j
        /// <summary>
        /// �ߋ��ɐԍ��������s�������`�[�łȂ����`�F�b�N����i�ԍ��o���Ȃ�:True  ����ȊO:False�j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�ԍ��o���Ȃ�:True  �ԍ��o������:False</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history> 
        public bool AkakuroCheck(string SlipNumber)
        {
            AkakuroReason AkaKuroRec = new ServiceSalesOrderDao(db).GetAkakuroReason(SlipNumber);

            if (AkaKuroRec != null)
            {
                return false;
            }
            return true;

        }
        #endregion

        #region  �C���������쐬����i���R�[�h�쐬�j
        /// <summary>
        /// �C���������쐬����i���R�[�h�쐬�j
        /// </summary>
        /// <param name="code"></param>
        /// <returns>�Ȃ�</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_�T�u�V�X�e���̓`�[�߂��̈ڍs</history>
        public void CreateModifiedHistory(CarSalesHeader header)
        {
            ModifiedReason ModifiedHistory = new ModifiedReason();
            ModifiedHistory.SlipNumber = header.SlipNumber;
            ModifiedHistory.RevisionNumber = header.RevisionNumber;
            ModifiedHistory.SlipType = "0";
            ModifiedHistory.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            ModifiedHistory.CreateDate = DateTime.Now;
            ModifiedHistory.Reason = header.Reason;
            ModifiedHistory.DelFlag = "0";
            db.ModifiedReason.InsertOnSubmit(ModifiedHistory);
            db.SubmitChanges();
        }
        #endregion

        #region �`�[���b�N
        /// <summary>
        /// ��ʂ����ۂɌĂяo�����
        /// �i�`�[���b�N����������j
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\�@�V�K�쐬
        /// </history>
        public ActionResult UnLock(CarSalesHeader header, EntitySet<CarSalesLine> line, EntitySet<CarSalesPayment> pay, FormCollection form)
        {
            if (header.RevisionNumber > 0)
            {
                ProcessUnLock(header);
            }

            ViewData["close"] = "1";

            //SetDataComponent(ref header);
 
            //return GetViewResult(header);
            return new EmptyResult();
        }

        /// <summary>
        /// �`�[�����b�N����
        /// �����P�F�����̓`�[�ł��邱��
        /// �����Q�F�ŐV�̃��r�W�����ł��邱�ƁiDelFlag='0'�j
        /// �����R�F���Ɏ��������b�N���Ă��Ȃ�����
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\�@�V�K�쐬
        /// </history>
        private void ProcessLock(CarSalesHeader header)
        {
            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.DelFlag.Equals("0"))
            {
                string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

                target.ProcessSessionControl = new ProcessSessionControl();
                target.ProcessSessionControl.ProcessSessionId = Guid.NewGuid();
                target.ProcessSessionControl.TableName = "CarSalesHeader";
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                target.ProcessSessionControl.CreateDate = DateTime.Now;

                db.SubmitChanges();
            }
        }

        /// <summary>
        /// �`�[���b�N����
        /// �����P�F���b�N���Ă���̂������ł��邱��
        /// �����Q�F�������́A���������ł��邱��
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\�@�V�K�쐬
        /// </history>
        private void ProcessUnLock(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if ((target.ProcessSessionControl != null && target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode)))
            {
                ProcessSessionControl control = new ProcessSessionControlDao(db).GetByKey(target.ProcessSessionId);
                db.ProcessSessionControl.DeleteOnSubmit(control);
                target.ProcessSessionControl = null;

                db.SubmitChanges();
            }
        }
        /// <summary>
        /// �`�[�����b�N����Ă��邩
        /// �����P�FProcessSessionId!=null
        /// �����Q�FProcessSessionControl!=null
        /// �����R�F�����ȊO�����b�N���Ă���
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\�@�V�K�쐬
        /// </history>
        private string GetProcessLockUser(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target != null && target.ProcessSessionId != null &&
                target.ProcessSessionControl != null &&
                !target.ProcessSessionControl.EmployeeCode.Equals(sessionEmployeeCode))
            {
                return target.ProcessSessionControl.Employee.EmployeeName;
            }

            return null;
        }

        /// <summary>
        /// ���b�N�������̂��̂ɂ���
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2017/11/10 arc yano #3787 �ԗ��`�[�ŌÂ��`�[�ŏ㏑���h�~����@�\�@�V�K�쐬
        /// </history>
        private void ProcessLockUpdate(CarSalesHeader header)
        {
            string sessionEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            CarSalesHeader target = new CarSalesOrderDao(db).GetByKey(header.SlipNumber, header.RevisionNumber);
            if (target.ProcessSessionControl != null)
            {
                target.ProcessSessionControl.EmployeeCode = sessionEmployeeCode;
                db.SubmitChanges();
            }
        }
        #endregion

        #region ������������
        /// <summary>
        /// ������������
        /// </summary>
        /// <param name="header">�ԗ��`�[�w�b�_</param>
        /// <param name="cancelPattern">�������@</param>
        /// <param name="cause">�����v��</param>
        /// <history>
        /// 2018/08/07 yano #3911 �o�^�ώԗ��̎ԗ��`�[�X�e�[�^�X�C���ɂ��ā@�����p�^�[���̈�����ǉ�
        /// 2018/06/22 arc yano  #3898 �ԗ��}�X�^�@AA�̔��Ŕ[�Ԍ�L�����Z���ƂȂ����ꍇ�̍݌ɃX�e�[�^�X�ɂ��ā@�V�K�쐬
        /// </history>
        private void ReleaseProvision(CarSalesHeader header, string cancelPattern, string cause)
        {
            //-------------------------
            //�ԗ��`�[
            //-------------------------
            header.ApprovalFlag = "0";
            header.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            header.LastUpdateDate = DateTime.Now;

            //Mod 2018/08/07 yano #3911
            //--------------------------
            //�ԗ������˗�����
            //--------------------------
            // �����ς݂̎ԗ��͈�������
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(header.SlipNumber);
            if (order != null && order.ReservationStatus != null && order.ReservationStatus.Equals("1"))
            {
                switch (cancelPattern)
                {
                    case "1":   //�S�ĉ���
                        order.ReservationStatus = "0";
                        order.RegistrationStatus = "0";
                        order.PurchaseOrderStatus = "0";
                        order.PurchasePlanStatus = "0";
                        break;

                    case "2":   //�o�^�A����������
                        order.ReservationStatus = "0";
                        order.RegistrationStatus = "0";
                        break;

                    default:   //�����̂݉���
                        order.ReservationStatus = "0";
                        break;                
                }

                //--------------------------
                //�ԗ��}�X�^
                //--------------------------
                SalesCar salesCar = new SalesCarDao(db).GetByKey(order.SalesCarNumber);

                //�ԗ��}�X�^�̍݌ɃX�e�[�^�X���u�����ρv�u�d�|���v�u�[�ԏ����v�u�[�ԍς݁v�̏ꍇ�́u�݌Ɂv�ɖ߂�
                if (salesCar != null && (salesCar.CarStatus.Equals("003") || salesCar.CarStatus.Equals("004") || salesCar.CarStatus.Equals("005") || salesCar.CarStatus.Equals("006")))
                {
                    //�݌ɃX�e�[�^�X���u�݌Ɂv
                    salesCar.CarStatus = "001";                                                         //�݌�                                   

                    //���P�[�V�������󗓂̏ꍇ
                    if (string.IsNullOrWhiteSpace(salesCar.LocationCode))
                    {
                        Location loc = new LocationDao(db).GetByKey(header.DepartmentCode);

                        if (loc != null)
                        {
                            salesCar.LocationCode = loc.LocationCode;
                        }
                        else
                        {
                            //�ԗ��d���f�[�^���擾���āA���Ƀ��P�[�V������ݒ肷��
                            CarPurchase carpurchase = new CarPurchaseDao(db).GetBySalesCarNumber(header.SalesCarNumber);

                            if (carpurchase != null)
                            {
                                salesCar.LocationCode = carpurchase.PurchaseLocationCode;
                            }
                            else
                            {
                                salesCar.LocationCode = "";
                            }
                        }
                    }
                    
                    salesCar.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    salesCar.LastUpdateDate = DateTime.Now;
                }

                order.SalesCarNumber = string.Empty;
                order.Vin = string.Empty;
                order.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                order.LastUpdateDate = DateTime.Now;
            }

            //Add 2018/08/07 yano #3911
            //AA�̔����̈��������ȊO�̏ꍇ�̓��[���𑗐M
            if(!cancelPattern.Equals(PTN_CANCEL_ALL))
            {
                CommonUtils.SendCancelReservationMail(db, header, cancelPattern, cause);
            }
            
        }

        #endregion

        #region AA���E�̓������т̍폜
        /// <summary>
        /// AA���E�̓������т̍폜
        /// </summary>
        /// <param name="header"></param>
        /// <history>
        /// 2018/06/22 arc yano  #3898 �ԗ��}�X�^�@AA�̔��Ŕ[�Ԍ�L�����Z���ƂȂ����ꍇ�̍݌ɃX�e�[�^�X�ɂ��ā@�V�K�쐬
        /// </history>
        private void DeleteAAJournal(CarSalesHeader header)
        {
            CustomerClaim cc = new CustomerClaimDao(db).GetByKey(header.CustomerCode);

            if (cc != null && cc.CustomerClaimType.Equals("201"))
            {
                List<Journal> AADelJournal = new JournalDao(db).GetListByCustomerAndSlip(header.SlipNumber, header.CustomerCode).Where(x => (x.AccountType.Equals("006"))).ToList();

                foreach (var aa in AADelJournal)
                {
                    aa.DelFlag = "1";
                    aa.LastUpdateDate = DateTime.Now;
                    aa.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                }

                db.SubmitChanges();
            }
        }

        #endregion

        #region �Ώۓ`�[�̃��[����Ђ���̓����������ς݂��ǂ�����Ԃ�(Ajax��p�j
        /// <summary>
        /// �Ώۓ`�[�̃��[����Ђ���̓����������ς݂��ǂ�����Ԃ�(Ajax��p�j
        /// </summary>
        /// <param name="code">�Ȃ�</param>
        /// <returns>�����ρFTrue �������FFalse</returns>
        /// <history>
        /// 2022/08/30 yano #4150�y�ԗ��`�[���́z���������ς̃��[���������X�V�����s��̑Ή� ���[�����������̔��f�̕ύX
        /// 2016/09/08 arc nakayama #3595_�y�區�ځz�ԗ����|���@�\���P ���[����Ђ�����������ɂ���Ă����烍�[�������ɐU��ւ��Ȃ��悤�ɂ���
        /// </history>
        public ActionResult LoanCompleteCheck(string SlipNumber, string LoanCode)
        {
            if (Request.IsAjaxRequest())
            {

                bool ret = false;       //������

                if (!string.IsNullOrEmpty(LoanCode))
                {
                    //���[���R�[�h�́A�����{�A���t�@�x�b�g�P�����@�̍\���ɂȂ��Ă��邽�ߖ����̃A���t�@�x�b�g���폜����

                    //string MasterLoanCode = LoanCode.Remove(leng, 1); //�����̃A���t�@�x�b�g�폜
                    Loan LoanDataPlan = new LoanDao(db).GetByKey(LoanCode, false);

                    //�Y���`�[�̓����\����擾����
                    ReceiptPlan LoanData = new ReceiptPlanDao(db).GetByCustomerClaim(SlipNumber, LoanDataPlan.CustomerClaimCode);

                    if (LoanData != null)
                    {
                        if (LoanData.CompleteFlag.Equals("1"))
                        {
                            ret = true; //���������t���O�������Ă�����True
                        }
                    }
                    //Mod 2022/08/30 yano #4150
                    else
                    {
                        //�Y���`�[�̓����\�肪���݂��Ȃ��ꍇ�A�������т��m�F
                        List<Journal> LoanList = new JournalDao(db).GetListByCustomerAndSlip(SlipNumber, LoanDataPlan.CustomerClaimCode).Where(x => x.AccountType.Equals(ACCOUNTTYPE_LOAN)).ToList();
                        
                        if(LoanList != null && LoanList.Count > 0) 
                        {
                            ret = true; //���т���Ȃ�True
                        }
                    }
                }
                return Json(ret);
            }
            return new EmptyResult();
        }
        #endregion

        #region
         /// <summary>
        /// �`�[�R�s�[
        /// </summary>
        /// <param name="sourceHeader">�R�s�[��</param>
        /// <param name="destHeader">�R�s�[��</param>
        /// <returns>�R�s�[��</returns>
        /// <history>
        /// 2018/11/07 yano #3939 �ԗ��`�[�[�ԑO�i�[�Ԋm�F���o�͌�j�̒������o�͎��ɉ����ԍ��X�V�s�Ƃ���
        /// </history>
        private CarSalesHeader CopyCarSalesHeader(CarSalesHeader sourceHeader, CarSalesHeader destHeader) 
        {
			destHeader.SlipNumber = sourceHeader.SlipNumber;
            destHeader.RevisionNumber = sourceHeader.RevisionNumber;
			destHeader.QuoteDate = sourceHeader.QuoteDate;
			destHeader.QuoteExpireDate = sourceHeader.QuoteExpireDate;
			destHeader.SalesOrderDate = sourceHeader.SalesOrderDate;
			destHeader.SalesOrderStatus = sourceHeader.SalesOrderStatus;
			destHeader.ApprovalFlag = sourceHeader.ApprovalFlag;
			destHeader.SalesDate = DateTime.Today;
			destHeader.CustomerCode = sourceHeader.CustomerCode;
			destHeader.DepartmentCode = sourceHeader.DepartmentCode;
			destHeader.EmployeeCode = sourceHeader.EmployeeCode;
			destHeader.CampaignCode1 = sourceHeader.CampaignCode1;
			destHeader.CampaignCode2 = sourceHeader.CampaignCode2;
			destHeader.NewUsedType = sourceHeader.NewUsedType;
			destHeader.SalesType = sourceHeader.SalesType;
			destHeader.MakerName = sourceHeader.MakerName;
			destHeader.CarBrandName = sourceHeader.CarBrandName;
			destHeader.CarName = sourceHeader.CarName;
			destHeader.CarGradeName = sourceHeader.CarGradeName;
			destHeader.CarGradeCode = sourceHeader.CarGradeCode;
			destHeader.ManufacturingYear = sourceHeader.ManufacturingYear;
			destHeader.ExteriorColorCode = sourceHeader.ExteriorColorCode;
			destHeader.ExteriorColorName = sourceHeader.ExteriorColorName;
            destHeader.InteriorColorCode = sourceHeader.InteriorColorCode;
            destHeader.InteriorColorName = sourceHeader.InteriorColorName;
			destHeader.Vin = sourceHeader.Vin;
			destHeader.UsVin = sourceHeader.UsVin;
			destHeader.ModelName = sourceHeader.ModelName;
			destHeader.Mileage = sourceHeader.Mileage;
			destHeader.MileageUnit = sourceHeader.MileageUnit;
			destHeader.RequestPlateNumber = sourceHeader.RequestPlateNumber;
			destHeader.RegistPlanDate = sourceHeader.RegistPlanDate;
			destHeader.HotStatus = sourceHeader.HotStatus;
			destHeader.SalesCarNumber = sourceHeader.SalesCarNumber;
			destHeader.RequestRegistDate = sourceHeader.RequestRegistDate;
			destHeader.SalesPlanDate = sourceHeader.SalesPlanDate;
			destHeader.RegistrationType = sourceHeader.RegistrationType;
			destHeader.MorterViecleOfficialCode = sourceHeader.MorterViecleOfficialCode;
			destHeader.OwnershipReservation = sourceHeader.OwnershipReservation;
			destHeader.CarLiabilityInsuranceType = sourceHeader.CarLiabilityInsuranceType;
			destHeader.SealSubmitDate = sourceHeader.SealSubmitDate;
			destHeader.ProxySubmitDate = sourceHeader.ProxySubmitDate;
			destHeader.ParkingSpaceSubmitDate = sourceHeader.ParkingSpaceSubmitDate;
			destHeader.CarLiabilityInsuranceSubmitDate = sourceHeader.CarLiabilityInsuranceSubmitDate;
			destHeader.OwnershipReservationSubmitDate = sourceHeader.OwnershipReservationSubmitDate;
			destHeader.Memo = sourceHeader.Memo;
			destHeader.SalesPrice = sourceHeader.SalesPrice;
			destHeader.DiscountAmount = sourceHeader.DiscountAmount;
			destHeader.TaxationAmount = sourceHeader.TaxationAmount;
			destHeader.TaxAmount = sourceHeader.TaxAmount;
			destHeader.ShopOptionAmount = sourceHeader.ShopOptionAmount;
			destHeader.ShopOptionTaxAmount = sourceHeader.ShopOptionTaxAmount;
			destHeader.MakerOptionAmount = sourceHeader.MakerOptionAmount;
			destHeader.MakerOptionTaxAmount = sourceHeader.MakerOptionTaxAmount;
			destHeader.OutSourceAmount = sourceHeader.OutSourceAmount;
			destHeader.OutSourceTaxAmount = sourceHeader.OutSourceTaxAmount;
			destHeader.SubTotalAmount = sourceHeader.SubTotalAmount;
			destHeader.CarTax = sourceHeader.CarTax;
			destHeader.CarLiabilityInsurance = sourceHeader.CarLiabilityInsurance;
			destHeader.CarWeightTax = sourceHeader.CarWeightTax;
			destHeader.AcquisitionTax = sourceHeader.AcquisitionTax;
			destHeader.InspectionRegistCost = sourceHeader.InspectionRegistCost;
			destHeader.ParkingSpaceCost = sourceHeader.ParkingSpaceCost;
			destHeader.TradeInCost = sourceHeader.TradeInCost;
			destHeader.RecycleDeposit = sourceHeader.RecycleDeposit;
			destHeader.RecycleDepositTradeIn = sourceHeader.RecycleDepositTradeIn;
			destHeader.NumberPlateCost = sourceHeader.NumberPlateCost;
			destHeader.RequestNumberCost = sourceHeader.RequestNumberCost;
			destHeader.TradeInFiscalStampCost = sourceHeader.TradeInFiscalStampCost;
			destHeader.TaxFreeFieldName = sourceHeader.TaxFreeFieldName;
			destHeader.TaxFreeFieldValue = sourceHeader.TaxFreeFieldValue;
			destHeader.TaxFreeTotalAmount = sourceHeader.TaxFreeTotalAmount;
			destHeader.InspectionRegistFee = sourceHeader.InspectionRegistFee;
			destHeader.ParkingSpaceFee = sourceHeader.ParkingSpaceFee;
			destHeader.TradeInFee = sourceHeader.TradeInFee;
			destHeader.PreparationFee = sourceHeader.PreparationFee;
			destHeader.RecycleControlFee = sourceHeader.RecycleControlFee;
			destHeader.RecycleControlFeeTradeIn = sourceHeader.RecycleControlFeeTradeIn;
			destHeader.RequestNumberFee = sourceHeader.RequestNumberFee;
			destHeader.CarTaxUnexpiredAmount = sourceHeader.CarTaxUnexpiredAmount;
			destHeader.CarLiabilityInsuranceUnexpiredAmount = sourceHeader.CarLiabilityInsuranceUnexpiredAmount;
			destHeader.TradeInAppraisalFee = sourceHeader.TradeInAppraisalFee;
			destHeader.FarRegistFee = sourceHeader.FarRegistFee;
			destHeader.TradeInMaintenanceFee = sourceHeader.TradeInMaintenanceFee;
			destHeader.InheritedInsuranceFee = sourceHeader.InheritedInsuranceFee;
			destHeader.TaxationFieldName = sourceHeader.TaxationFieldName;
			destHeader.TaxationFieldValue = sourceHeader.TaxationFieldValue;
			destHeader.SalesCostTotalAmount = sourceHeader.SalesCostTotalAmount;
			destHeader.SalesCostTotalTaxAmount = sourceHeader.SalesCostTotalTaxAmount;
			destHeader.OtherCostTotalAmount = sourceHeader.OtherCostTotalAmount;
			destHeader.CostTotalAmount = sourceHeader.CostTotalAmount;
			destHeader.TotalTaxAmount = sourceHeader.TotalTaxAmount;
			destHeader.GrandTotalAmount = sourceHeader.GrandTotalAmount;
			destHeader.PossesorCode = sourceHeader.PossesorCode;
			destHeader.UserCode = sourceHeader.UserCode;
			destHeader.PrincipalPlace = sourceHeader.PrincipalPlace;
			destHeader.VoluntaryInsuranceType = sourceHeader.VoluntaryInsuranceType;
			destHeader.VoluntaryInsuranceCompanyName = sourceHeader.VoluntaryInsuranceCompanyName;
			destHeader.VoluntaryInsuranceAmount = sourceHeader.VoluntaryInsuranceAmount;
			destHeader.VoluntaryInsuranceTermFrom = sourceHeader.VoluntaryInsuranceTermFrom;
			destHeader.VoluntaryInsuranceTermTo = sourceHeader.VoluntaryInsuranceTermTo;
			destHeader.PaymentPlanType = sourceHeader.PaymentPlanType;
			destHeader.TradeInAmount1 = sourceHeader.TradeInAmount1;
			destHeader.TradeInTax1 = sourceHeader.TradeInTax1;
			destHeader.TradeInUnexpiredCarTax1 = sourceHeader.TradeInUnexpiredCarTax1;
			destHeader.TradeInRemainDebt1 = sourceHeader.TradeInRemainDebt1;
			destHeader.TradeInAppropriation1 = sourceHeader.TradeInAppropriation1;
			destHeader.TradeInRecycleAmount1 = sourceHeader.TradeInRecycleAmount1;
			destHeader.TradeInMakerName1 = sourceHeader.TradeInMakerName1;
			destHeader.TradeInCarName1 = sourceHeader.TradeInCarName1;
			destHeader.TradeInClassificationTypeNumber1 = sourceHeader.TradeInClassificationTypeNumber1;
			destHeader.TradeInModelSpecificateNumber1 = sourceHeader.TradeInModelSpecificateNumber1;
			destHeader.TradeInManufacturingYear1 = sourceHeader.TradeInManufacturingYear1;
			destHeader.TradeInInspectionExpiredDate1 = sourceHeader.TradeInInspectionExpiredDate1;
			destHeader.TradeInMileage1 = sourceHeader.TradeInMileage1;
			destHeader.TradeInMileageUnit1 = sourceHeader.TradeInMileageUnit1;
			destHeader.TradeInVin1 = sourceHeader.TradeInVin1;
			destHeader.TradeInRegistrationNumber1 = sourceHeader.TradeInRegistrationNumber1;
			destHeader.TradeInUnexpiredLiabilityInsurance1 = sourceHeader.TradeInUnexpiredLiabilityInsurance1;
			destHeader.TradeInAmount2 = sourceHeader.TradeInAmount2;
			destHeader.TradeInTax2 = sourceHeader.TradeInTax2;
			destHeader.TradeInUnexpiredCarTax2 = sourceHeader.TradeInUnexpiredCarTax2;
			destHeader.TradeInRemainDebt2 = sourceHeader.TradeInRemainDebt2;
			destHeader.TradeInAppropriation2 = sourceHeader.TradeInAppropriation2;
			destHeader.TradeInRecycleAmount2 = sourceHeader.TradeInRecycleAmount2;
			destHeader.TradeInMakerName2 = sourceHeader.TradeInMakerName2;
			destHeader.TradeInCarName2 = sourceHeader.TradeInCarName2;
			destHeader.TradeInClassificationTypeNumber2 = sourceHeader.TradeInClassificationTypeNumber2;
			destHeader.TradeInModelSpecificateNumber2 = sourceHeader.TradeInModelSpecificateNumber2;
			destHeader.TradeInManufacturingYear2 = sourceHeader.TradeInManufacturingYear2;
			destHeader.TradeInInspectionExpiredDate2 = sourceHeader.TradeInInspectionExpiredDate2;
			destHeader.TradeInMileage2 = sourceHeader.TradeInMileage2;
			destHeader.TradeInMileageUnit2 = sourceHeader.TradeInMileageUnit2;
			destHeader.TradeInVin2 = sourceHeader.TradeInVin2;
			destHeader.TradeInRegistrationNumber2 = sourceHeader.TradeInRegistrationNumber2;
			destHeader.TradeInUnexpiredLiabilityInsurance2 = sourceHeader.TradeInUnexpiredLiabilityInsurance2;
			destHeader.TradeInAmount3 = sourceHeader.TradeInAmount3;
			destHeader.TradeInTax3 = sourceHeader.TradeInTax3;
			destHeader.TradeInUnexpiredCarTax3 = sourceHeader.TradeInUnexpiredCarTax3;
			destHeader.TradeInRemainDebt3 = sourceHeader.TradeInRemainDebt3;
			destHeader.TradeInAppropriation3 = sourceHeader.TradeInAppropriation3;
			destHeader.TradeInRecycleAmount3 = sourceHeader.TradeInRecycleAmount3;
			destHeader.TradeInMakerName3 = sourceHeader.TradeInMakerName3;
			destHeader.TradeInCarName3 = sourceHeader.TradeInCarName3;
			destHeader.TradeInClassificationTypeNumber3 = sourceHeader.TradeInClassificationTypeNumber3;
			destHeader.TradeInModelSpecificateNumber3 = sourceHeader.TradeInModelSpecificateNumber3;
			destHeader.TradeInManufacturingYear3 = sourceHeader.TradeInManufacturingYear3;
			destHeader.TradeInInspectionExpiredDate3 = sourceHeader.TradeInInspectionExpiredDate3;
			destHeader.TradeInMileage3 = sourceHeader.TradeInMileage3;
			destHeader.TradeInMileageUnit3 = sourceHeader.TradeInMileageUnit3;
			destHeader.TradeInVin3 = sourceHeader.TradeInVin3;
			destHeader.TradeInRegistrationNumber3 = sourceHeader.TradeInRegistrationNumber3;
			destHeader.TradeInUnexpiredLiabilityInsurance3 = sourceHeader.TradeInUnexpiredLiabilityInsurance3;
			destHeader.TradeInTotalAmount = sourceHeader.TradeInTotalAmount;
			destHeader.TradeInTaxTotalAmount = sourceHeader.TradeInTaxTotalAmount;
			destHeader.TradeInUnexpiredCarTaxTotalAmount = sourceHeader.TradeInUnexpiredCarTaxTotalAmount;
			destHeader.TradeInRemainDebtTotalAmount = sourceHeader.TradeInRemainDebtTotalAmount;
			destHeader.TradeInAppropriationTotalAmount = sourceHeader.TradeInAppropriationTotalAmount;
			destHeader.PaymentTotalAmount = sourceHeader.PaymentTotalAmount;
			destHeader.PaymentCashTotalAmount = sourceHeader.PaymentCashTotalAmount;
			destHeader.LoanPrincipalAmount = sourceHeader.LoanPrincipalAmount;
			destHeader.LoanFeeAmount = sourceHeader.LoanFeeAmount;
			destHeader.LoanTotalAmount = sourceHeader.LoanTotalAmount;
			destHeader.LoanCodeA = sourceHeader.LoanCodeA;
			destHeader.PaymentFrequencyA = sourceHeader.PaymentFrequencyA;
			destHeader.PaymentTermFromA = sourceHeader.PaymentTermFromA;
			destHeader.PaymentTermToA = sourceHeader.PaymentTermToA;
			destHeader.BonusMonthA1 = sourceHeader.BonusMonthA1;
			destHeader.BonusMonthA2 = sourceHeader.BonusMonthA2;
			destHeader.FirstAmountA = sourceHeader.FirstAmountA;
			destHeader.SecondAmountA = sourceHeader.SecondAmountA;
			destHeader.BonusAmountA = sourceHeader.BonusAmountA;
			destHeader.CashAmountA = sourceHeader.CashAmountA;
			destHeader.LoanPrincipalA = sourceHeader.LoanPrincipalA;
			destHeader.LoanFeeA = sourceHeader.LoanFeeA;
			destHeader.LoanTotalAmountA = sourceHeader.LoanTotalAmountA;
			destHeader.AuthorizationNumberA = sourceHeader.AuthorizationNumberA;
			destHeader.FirstDirectDebitDateA = sourceHeader.FirstDirectDebitDateA;
			destHeader.SecondDirectDebitDateA = sourceHeader.SecondDirectDebitDateA;
			destHeader.LoanCodeB = sourceHeader.LoanCodeB;
			destHeader.PaymentFrequencyB = sourceHeader.PaymentFrequencyB;
			destHeader.PaymentTermFromB = sourceHeader.PaymentTermFromB;
			destHeader.PaymentTermToB = sourceHeader.PaymentTermToB;
			destHeader.BonusMonthB1 = sourceHeader.BonusMonthB1;
			destHeader.BonusMonthB2 = sourceHeader.BonusMonthB2;
			destHeader.FirstAmountB = sourceHeader.FirstAmountB;
			destHeader.SecondAmountB = sourceHeader.SecondAmountB;
			destHeader.BonusAmountB = sourceHeader.BonusAmountB;
			destHeader.CashAmountB = sourceHeader.CashAmountB;
			destHeader.LoanPrincipalB = sourceHeader.LoanPrincipalB;
			destHeader.LoanFeeB = sourceHeader.LoanFeeB;
			destHeader.LoanTotalAmountB = sourceHeader.LoanTotalAmountB;
			destHeader.AuthorizationNumberB = sourceHeader.AuthorizationNumberB;
			destHeader.FirstDirectDebitDateB = sourceHeader.FirstDirectDebitDateB;
			destHeader.SecondDirectDebitDateB = sourceHeader.SecondDirectDebitDateB;
			destHeader.LoanCodeC = sourceHeader.LoanCodeC;
			destHeader.PaymentFrequencyC = sourceHeader.PaymentFrequencyC;
			destHeader.PaymentTermFromC = sourceHeader.PaymentTermFromC;
			destHeader.PaymentTermToC = sourceHeader.PaymentTermToC;
			destHeader.BonusMonthC1 = sourceHeader.BonusMonthC1;
			destHeader.BonusMonthC2 = sourceHeader.BonusMonthC2;
			destHeader.FirstAmountC = sourceHeader.FirstAmountC;
			destHeader.SecondAmountC = sourceHeader.SecondAmountC;
			destHeader.BonusAmountC = sourceHeader.BonusAmountC;
			destHeader.CashAmountC = sourceHeader.CashAmountC;
			destHeader.LoanPrincipalC = sourceHeader.LoanPrincipalC;
			destHeader.LoanFeeC = sourceHeader.LoanFeeC;
			destHeader.LoanTotalAmountC = sourceHeader.LoanTotalAmountC;
			destHeader.AuthorizationNumberC = sourceHeader.AuthorizationNumberC;
			destHeader.FirstDirectDebitDateC = sourceHeader.FirstDirectDebitDateC;
			destHeader.SecondDirectDebitDateC = sourceHeader.SecondDirectDebitDateC;
			destHeader.CancelDate = sourceHeader.CancelDate;
			destHeader.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
			destHeader.CreateDate = DateTime.Now;
			destHeader.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
			destHeader.LastUpdateDate = DateTime.Now;
			destHeader.DelFlag = "0";
			destHeader.InspectionRegistFeeTax = sourceHeader.InspectionRegistFeeTax;
			destHeader.ParkingSpaceFeeTax = sourceHeader.ParkingSpaceFeeTax;
			destHeader.TradeInFeeTax = sourceHeader.TradeInFeeTax;
			destHeader.PreparationFeeTax = sourceHeader.PreparationFeeTax;
			destHeader.RecycleControlFeeTax = sourceHeader.RecycleControlFeeTax;
			destHeader.RecycleControlFeeTradeInTax = sourceHeader.RecycleControlFeeTradeInTax;
			destHeader.RequestNumberFeeTax = sourceHeader.RequestNumberFeeTax;
			destHeader.CarTaxUnexpiredAmountTax = sourceHeader.CarTaxUnexpiredAmountTax;
			destHeader.CarLiabilityInsuranceUnexpiredAmountTax = sourceHeader.CarLiabilityInsuranceUnexpiredAmountTax;
			destHeader.TradeInAppraisalFeeTax = sourceHeader.TradeInAppraisalFeeTax;
			destHeader.FarRegistFeeTax = sourceHeader.FarRegistFeeTax;
			destHeader.TradeInMaintenanceFeeTax = sourceHeader.TradeInMaintenanceFeeTax;
			destHeader.InheritedInsuranceFeeTax = sourceHeader.InheritedInsuranceFeeTax;
			destHeader.TaxationFieldValueTax = sourceHeader.TaxationFieldValueTax;
			destHeader.TradeInEraseRegist1 = sourceHeader.TradeInEraseRegist1;
			destHeader.TradeInEraseRegist2 = sourceHeader.TradeInEraseRegist2;
			destHeader.TradeInEraseRegist3 = sourceHeader.TradeInEraseRegist3;
			destHeader.RemainAmountA = sourceHeader.RemainAmountA;
			destHeader.RemainAmountB = sourceHeader.RemainAmountB;
			destHeader.RemainAmountC = sourceHeader.RemainAmountC;
			destHeader.RemainFinalMonthA = sourceHeader.RemainFinalMonthA;
			destHeader.RemainFinalMonthB = sourceHeader.RemainFinalMonthB;
			destHeader.RemainFinalMonthC = sourceHeader.RemainFinalMonthC;
			destHeader.LoanRateA = sourceHeader.LoanRateA;
			destHeader.LoanRateB = sourceHeader.LoanRateB;
			destHeader.LoanRateC = sourceHeader.LoanRateC;
			destHeader.SalesTax = sourceHeader.SalesTax;
			destHeader.DiscountTax = sourceHeader.DiscountTax;
			destHeader.TradeInPrice1 = sourceHeader.TradeInPrice1;
			destHeader.TradeInPrice2 = sourceHeader.TradeInPrice2;
			destHeader.TradeInPrice3 = sourceHeader.TradeInPrice3;
			destHeader.TradeInRecycleTotalAmount = sourceHeader.TradeInRecycleTotalAmount;
            destHeader.ConsumptionTaxId = sourceHeader.ConsumptionTaxId;
            destHeader.Rate = sourceHeader.Rate;
            destHeader.RevenueStampCost = sourceHeader.RevenueStampCost;
            destHeader.TradeInCarTaxDeposit = sourceHeader.TradeInCarTaxDeposit;
            destHeader.LastEditScreen = sourceHeader.LastEditScreen;
            destHeader.PaymentSecondFrequencyA = sourceHeader.PaymentSecondFrequencyA;
            destHeader.PaymentSecondFrequencyB = sourceHeader.PaymentSecondFrequencyB;
            destHeader.PaymentSecondFrequencyC = sourceHeader.PaymentSecondFrequencyC;
            destHeader.ProcessSessionControl = new ProcessSessionControlDao(db).GetByKey(sourceHeader.ProcessSessionId);;
            destHeader.ProcessSessionId = sourceHeader.ProcessSessionId;
            //destHeader.ProcessSessionId = sourceHeader.ProcessSessionId;
			
            return destHeader;
        }
    #endregion

      #region �K�i�������p�̏���Ōv�Z�A�o�^����
        /// <summary>
        /// �K�i������(�C���{�C�X)�p�̏���ŁE���z�̓o�^
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�w�b�_</param>
        /// <param name="lines">�T�[�r�X�`�[����</param>
        /// <param name="paymentList">�T�[�r�X�`�[�x��</param>
        /// <returns>�x���\��</returns>
        /// <history>
        /// 2023/09/28 yano #4183 �C���{�C�X�Ή�(�o���Ή�)
        /// 2023/09/05 yano #4162 �C���{�C�X�Ή� �V�K�쐬
        /// </history>
        private void InsertInvoiceConsumptionTax(CarSalesHeader header)
        {
            //�o�^���X�g
            List<InvoiceConsumptionTax> registList = new List<InvoiceConsumptionTax>();

            //-----------------------
            //�Â����X�g�̍폜
            //-----------------------
            List<InvoiceConsumptionTax> delList = new List<InvoiceConsumptionTax>();

            delList = new InvoiceConsumptionTaxDao(db).GetBySlipNumber(header.SlipNumber);

            foreach (var d in delList)
            {
                d.DelFlag = "1";
            }

            //�ېō��v���z�Z�o�y�����̔����v(�ō�) - �ŋ����@- ���̑���ېŁz
            decimal amountwithTax = (header.GrandTotalAmount ?? 0m) - (header.TaxFreeTotalAmount ?? 0m) - (header.OtherCostTotalAmount ?? 0m);
           
            decimal amount = 0m;
            decimal taxmount= 0m;

            int taxrate = header.Rate ?? 0;

            //�ō����z����Ŕ����z���v�Z
            amount = CommonUtils.CalcAmountWithoutTax(amountwithTax, taxrate, 0);

            //����Ōv�Z
            taxmount = amountwithTax - amount;

            InvoiceConsumptionTax  el = new InvoiceConsumptionTax();
                
            el.InvoiceConsumptionTaxId = Guid.NewGuid();                                            //���j�[�NID
            el.SlipNumber = header.SlipNumber;                                                      //�`�[�ԍ�
            el.CustomerClaimCode = header.CustomerCode;                                             //������R�[�h
            el.Rate = header.Rate ?? 0;                                                             //����ŗ�
            el.InvoiceConsumptionTaxAmount = taxmount;                                              //�C���{�C�X�����
            el.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;                  //�쐬��
            el.CreateDate = DateTime.Now;                                                           //�쐬��
            el.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;;              //�ŏI�X�V��
            el.LastUpdateDate = DateTime.Now;                                                       //�ŏI�X�V��
            el.DelFlag = "0";                                                                       //�폜�t���O

            registList.Add(el);

            db.InvoiceConsumptionTax.InsertAllOnSubmit(registList);

            header.SuspendTaxRecv = taxmount - (header.TotalTaxAmount ?? 0m);                       //����ō��z(�C���{�C�X����Ł|�����ێ������) //Add 2023/09/28 yano #4183

        }
    #endregion

  }
}
