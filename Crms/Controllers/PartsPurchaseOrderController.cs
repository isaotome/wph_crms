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
using System.Collections;               //Mod 2016/03/22 arc yno

/*-----------------------------------------------------------------------------------------------
 * �@�\�F���i�����ꗗ�E���i�������͉�ʂɊւ���A�N�V����
 * �X�V�����F
 *   2015/12/09 arc yano #3290 ���i�d���@�\���P(���i�����ꗗ)
 *                             ���������E�ꗗ�̍��ڂ̕ύX
 *   2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) 
 *                             ���i�����f�[�^�̊Ǘ����@�̕ύX(1���� = 1���i��1����=�������i)
 * ---------------------------------------------------------------------------------------------*/

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class PartsPurchaseOrderController : InheritedController
    {
        //Add 2014/08/08 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        protected string FORM_NAME = "���i��������(����)";                  // ��ʖ�
        protected string PROC_NAME = "���i�����o�^(����)";                  // ������
        protected string PROC_NAME_SEARCH = "���i��������";                 // ������
        protected string PROC_NAME_EXCELDOWNLOAD = "Excel�o��";             // ������

        //Add 2015/11/09 arc yano #3291
        //�����X�e�[�^�X
        protected static readonly string STS_PURCHASEORDER_UNORDER = "001";     //������(�����X�e�[�^�X)
        protected static readonly string STS_PURCHASEORDER_ORDERED = "002";     //������(�����X�e�[�^�X)

        //�����敪
        protected string gGenuine;                                           
        
        //�r���[��
        protected string viewName = "PartsPurchaseOrderEntry";                  //�f�t�H���g�͏����i�p�̉��

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        protected CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsPurchaseOrderController()
        {
            db = new CrmsLinqDataContext();
            gGenuine = "001";                   //�����敪 =�@�u�����v
        }

        #region �����@�\
        /// <summary>
        /// ������ʏ����\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria()
        {
            //-------------------------
            //�����l�̐ݒ�
            //-------------------------
            FormCollection form = new FormCollection();
            //����
            form["DepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;    //���� = ���O�C�����[�U�̏�������
            form["PurchaseOrderStatus"] = STS_PURCHASEORDER_UNORDER;                    //�����X�e�[�^�X=�u�������v
            form["GenuineType"] = gGenuine;                                          //�����敪=�u�����v
            return Criteria(form);
        }

        /// <summary>
        /// �������ʕ\��
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form)
        {
            // Add 2014/09/08 arc amii �G���[���O�Ή� ��ʓ��͒l��SysLog�ɏo�͂���
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_SEARCH);

            PaginatedList<PartsPurchaseOrder> list = GetSearchResultList(form);

            //�T�[�r�X�`�[�ƕR�t����
            foreach (var item in list)
            {
                item.ServiceSalesHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(item.ServiceSlipNumber);
            }

            //��ʍ��ڂ̐ݒ�
            SetCriteriaComponent(form);

            return View("PartsPurchaseOrderCriteria", list);

        }

        /// <summary>
        /// �����������Z�b�g���Č��ʃ��X�g��Ԃ�
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>���ʃ��X�g</returns>
        private PaginatedList<PartsPurchaseOrder> GetSearchResultList(FormCollection form)
        {
            PartsPurchaseOrder condition = new PartsPurchaseOrder();
            
            condition = SetCriteriaCondition(form); //���������̐ݒ�

            return new PartsPurchaseOrderDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ������ʍ��ڂ̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>
        /// 2015/12/09 arc yano #3290 ���i�d���@�\���P(���i�����ꗗ) �V�K�쐬
        /// </history>
        private void SetCriteriaComponent(FormCollection form)
        {
            //------------------
            //�K����͒l�̐ݒ�
            //------------------
            ViewData["DefaultDepartmentCode"] = ((Employee)Session["Employee"]).DepartmentCode;
            ViewData["DefaultDepartmentName"] = new DepartmentDao(db).GetByKey(ViewData["DefaultDepartmentCode"].ToString()).DepartmentName;
            ViewData["DefaultPurchaseOrderStatus"] = STS_PURCHASEORDER_UNORDER;
            ViewData["DefaultGenuineType"] = gGenuine;

            //------------------
            //���������̍Đݒ�
            //------------------
            ViewData["PurchaseOrderNumber"] = form["PurchaseOrderNumber"];      //�����ԍ�
            ViewData["ServiceSlipNumber"] = form["ServiceSlipNumber"];          //�󒍓`�[�ԍ�
            ViewData["PurchaseOrderDateFrom"] = form["PurchaseOrderDateFrom"];  //������(From)
            ViewData["PurchaseOrderDateTo"] = form["PurchaseOrderDateTo"];      //������(To)
            ViewData["DepartmentCode"] = form["DepartmentCode"];                //����R�[�h
            ViewData["EmployeeCode"] = form["EmployeeCode"];                    //�S���҃R�[�h
            ViewData["SupplierCode"] = form["SupplierCode"];                    //�d����R�[�h
            ViewData["WebOrderNumber"] = form["WebOrderNumber"];                //Web�I�[�_�[�ԍ�
            ViewData["SupplierName"] = form["SupplierName"];                    //�d���於

            CodeDao dao = new CodeDao(db);
            //�����X�e�[�^�X
            ViewData["PurchaseOrderStatusList"] = CodeUtils.GetSelectListByModel<c_PurchaseOrderStatus>(dao.GetPurchaseOrderStatusAll(false), form["PurchaseOrderStatus"], true);
            //�����敪
            ViewData["GenuineTypeList"] = CodeUtils.GetSelectListByModel<c_GenuineType>(dao.GetGenuineTypeAll(false), form["GenuineType"], true);
            //�S���Җ�
            if (!string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                ViewData["EmployeeName"] = employee != null ? employee.EmployeeName : "";
            }
            //���喼
            if (!string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            }
        }

        /// <summary>
        /// ���������̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>��������</returns>
        /// <history>
        /// 2015/12/09 arc yano #3290 ���i�d���@�\���P(���i�����ꗗ) �V�K�쐬
        /// </history>
        private PartsPurchaseOrder SetCriteriaCondition(FormCollection form)
        {

            PartsPurchaseOrder condition = new PartsPurchaseOrder();
            //�����ԍ�
            condition.PurchaseOrderNumber = form["PurchaseOrderNumber"];
            //�󒍓`�[�ԍ�
            condition.ServiceSlipNumber = form["ServiceSlipNumber"];            
            //������
            condition.PurchaseOrderDateFrom = CommonUtils.StrToDateTime(form["PurchaseOrderDateFrom"], DaoConst.SQL_DATETIME_MAX);
            condition.PurchaseOrderDateTo = CommonUtils.StrToDateTime(form["PurchaseOrderDateTo"], DaoConst.SQL_DATETIME_MIN);
            //�����X�e�[�^�X
            condition.PurchaseOrderStatus = form["PurchaseOrderStatus"];
            //����R�[�h
            condition.DepartmentCode = form["DepartmentCode"];
            //�S���҃R�[�h
            condition.EmployeeCode = form["EmployeeCode"];
            //�d����R�[�h
            condition.SupplierCode = form["SupplierCode"];
            //�d���於
            condition.SupplierName = form["SupplierName"];
            //Web�I�[�_�[�ԍ�
            condition.WebOrderNumber = form["WebOrdernumber"];
            //�����敪
            condition.GenuineType = form["GenuineType"];

            condition.SetAuthCondition((Employee)Session["Employee"]);

            return condition;
        }
        #endregion

        #region ���͉�ʕ\��
        /// <summary>
        /// ���͉�ʕ\��
        /// </summary>
        /// <param name="param">�������</param>
        /// <param name="id">���id</param>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή� FCJ�f�t�H���g�d����̔p�~
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�
        /// </history>
        public ActionResult Entry(ParamOrder param)
        {
            List<PartsPurchaseOrder> orderList = new List<PartsPurchaseOrder>();         //�������X�g
            FormCollection form = new FormCollection();                                  //��ʂ̓��͒l
            PartsPurchaseOrder orderView = new PartsPurchaseOrder();                     //��ʕ\���p�̒l
            Parts parts = null;
            
            ModelState.Clear();

            //--------------------------------------------
            //�����i�̃f�t�H���g�̎d������擾
            //-------------------------------------------
            //Del 2018/06/01 arc yano #3894
            ////Add 2018/01/15 arc yano #3833
            //if (gGenuine.Equals("001"))
            //{
            //    form["GenuineSupplierCode"] = new ConfigurationSettingDao(db).GetByKey("GenuineSupplierCode").Value;

            //    form["GenuineSupplierName"] = new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]) != null ? new SupplierDao(db).GetByKey(form["GenuineSupplierCode"]).SupplierName : "";
            //}

            //�����ԍ����܂܂�Ă���ꍇ(�쐬�ϔ����f�[�^�̕ҏW)�́A���i�����e�[�u�����������ăf�[�^���擾����
            if (!string.IsNullOrWhiteSpace(param.PurchaseOrderNumber))
            {
                orderList = new PartsPurchaseOrderDao(db).GetListByKey(param.PurchaseOrderNumber);

                orderView = orderList[0];
            }
            else if (param.partsList != null && param.partsList.Count > 0)
            {
                //���ʍ��ڂ̐ݒ�
                //�󒍓`�[�ԍ�
                orderView.ServiceSlipNumber = param.ServiceSlipNumber;
                //����R�[�h
                //orderView.DepartmentCode = param.DepartmentCode;
                //�I�[�_�[�敪
                orderView.OrderType = param.OrderType;
                //���̑��̍��ڂ̐ݒ�
                orderView = makePurchaseOrder(orderView, form);

                //�ʍ���(���i�ԍ��A��������)�̐ݒ�
                foreach (var partsInfo in param.partsList)
                {
                    PartsPurchaseOrder order = new PartsPurchaseOrder();
                    
                    //���i�ԍ�
                    order.PartsNumber = partsInfo.PartsNumber;
                    //���i����
                    parts = new PartsDao(db).GetByKey(partsInfo.PartsNumber ?? "");
                    
                    if (parts != null)
                    {
                        //���i����
                        order.PartsNameJp = parts.PartsNameJp;
                        //�艿
                        order.Price = parts.Price;
                        //����
                        order.Cost = parts.Cost;
                    }
                    else
                    {
                        //���i����
                        order.PartsNameJp = "";
                    }

                    //��������
                    order.Quantity = partsInfo.Quantity;
                    
                    //�������z
                    order.Amount = (order.Quantity ?? 0) * (order.Cost ?? 0);

                    //���̑��̍��ڂ̐ݒ�
                    order = makePurchaseOrder(order, form);

                    orderList.Add(order);
                }
            }
            else
            {
                PartsPurchaseOrder order = new PartsPurchaseOrder();
                // �x���\����̓f�t�H���g�ŗ�������
                order.PaymentPlanDate = CommonUtils.GetFinalDay(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month);
                order.ArrivalPlanDate = DateTime.Today.AddDays(1);
                orderList.Add(order);

                //���̑��̍��ڂ̐ݒ�
                orderView = makePurchaseOrder(orderView, form);
            }

            SetDataComponent(form, orderView);

            return View(viewName, orderList);
        }

        /// <summary>
        /// �����f�[�^�쐬(���ʕ���)
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή� FCJ�̃f�t�H���g�ݒ菈����p�~
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�
        /// </history>
        private PartsPurchaseOrder makePurchaseOrder(PartsPurchaseOrder order, FormCollection form)
        {
            //�����X�e�[�^�X(������)
            order.PurchaseOrderStatus = STS_PURCHASEORDER_UNORDER;
            order.c_PurchaseOrderStatus = new CodeDao(db).GetPurchaseOrderStatus(false, order.PurchaseOrderStatus);

            // �x���\����̓f�t�H���g�ŗ�������
            order.PaymentPlanDate = CommonUtils.GetFinalDay(DateTime.Today.AddMonths(1).Year, DateTime.Today.AddMonths(1).Month);
            order.ArrivalPlanDate = DateTime.Today.AddDays(1);

            //����R�[�h(���O�C�����[�U�̏���������f�t�H���g�l�Ƃ��Đݒ�)
            order.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;

            //�S����
            Employee emp = (Employee)Session["Employee"];
            order.EmployeeCode = emp.EmployeeCode;
            //������
            order.PurchaseOrderDate = DateTime.Today;

            //�����敪=�u�����v�̏ꍇ�̓f�t�H���g�d�����ݒ肷��
            if (gGenuine.Equals("001"))
            {
                // �f�t�H���g�d������Z�b�g
                if (!string.IsNullOrWhiteSpace(order.DepartmentCode))
                {
                    //���̕���̍ŐV�̓��׎��т����Ɏd�����ݒ肷��
                    List<PartsPurchase> list = new PartsPurchaseDao(db).GetPurchaseResult(order.DepartmentCode, gGenuine);

                    //���̕��i�̎d�����т��������ꍇ
                    if (list.Count > 0)
                    {
                        //�d����R�[�h
                        order.SupplierCode = list.OrderByDescending(x => x.PurchaseDate).FirstOrDefault().SupplierCode;

                        //�d�����̐ݒ�
                        if (order.SupplierCode != null)
                        {
                            order.SupplierName = new SupplierDao(db).GetByKey(order.SupplierCode) != null ? new SupplierDao(db).GetByKey(order.SupplierCode).SupplierName : "";
                        }
                    }
                    //Del 2018/06/01 arc yano #3894
                    //else //���т������ꍇ�̓V�X�e���œo�^���Ă���f�t�H���g�d�����ݒ�   //Add 2018/01/15 arc yano #3833
                    //{
                    //    order.SupplierCode = form["GenuineSupplierCode"];
                    //    order.SupplierName = form["GenuineSupplierName"];
                    //}
                }
            }

            //�d���悪�ݒ肳��Ă���ꍇ�͎x������擾
            if (!string.IsNullOrWhiteSpace(order.SupplierCode))
            {
                //�x����̐ݒ�
                SupplierPayment supplierPayment = new SupplierPaymentDao(db).GetByKey(order.SupplierCode);

                if (supplierPayment != null)
                {
                    order.SupplierPaymentCode = supplierPayment.SupplierPaymentCode;
                    order.SupplierPaymentName = supplierPayment.SupplierPaymentName;
                }
            }

            return order;
        }
        #endregion
        #region �o�^�@�\
        /// <summary>
        /// ���i�����o�^����
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <param name="line">���i�����f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3806 ���i�������́@�P�O�s�ǉ��{�^���̒ǉ�
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(FormCollection form, List<PartsPurchaseOrder> line)
        {
            List<PartsPurchaseOrder> delList = null;
            List<PartsPurchaseOrder> list = null;
            
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            ModelState.Clear();

            //Validation�`�F�b�N
            ValidateSave(form, line);

            if (!ModelState.IsValid)
            {
                form["OrderFlag"] = "0";
                SetDataComponent(form);
                return View(viewName, line);
            }

            //�t�H�[���̓��͒l(���ʕ���)�����ɔ����f�[�^�ɓ���
            line = SetOrderList(line, form);

            string procName = "";
            
            procName = GetSysLogProcName(form);
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, procName);

            PaymentPlan plan = new PaymentPlan();

            using (TransactionScope ts = new TransactionScope())
            {
                int cnt = 0;
                int i = 0;
                
                //--------------------------------
                //�����f�[�^�X�V����(�폜����)
                //--------------------------------
                //DelFlag = '1'�@�̂��̂��폜�Ώۂ�
                delList = line.Where(x => (!string.IsNullOrWhiteSpace(x.DelFlag) && x.DelFlag.Equals("1"))).ToList();

                //Mod 2016/04/26 arc yano #3511
                //�f�[�^�x�[�X�ɂ����āA���͒l�ɂȂ����͍̂폜
                List<PartsPurchaseOrder> dList = new PartsPurchaseOrderDao(db).GetListByKey(form["PurchaseOrderNumber"]);

                if (dList != null && dList.Count > 0)
                {                    
                    foreach (var p in dList)
                    {
                        //�f�[�^�x�[�X�̕��i�ԍ��A�����ԍ��œ��̓f�[�^����������
                        var rec = line.Where(x => x.PurchaseOrderNumber.Equals(p.PurchaseOrderNumber) && x.PartsNumber.Equals(p.PartsNumber));

                        //�f�[�^�x�[�X�̃��R�[�h�����͒l�ɖ����ꍇ�͍폜
                        if (rec == null || rec.ToList().Count == 0)
                        {
                            p.DelFlag = "1";        //�폜�t���O�𗧂Ă�
                            delList.Add(p);
                        }
                    }
                }

                
                if (delList !=null && delList.Count > 0)
                {
                    DelOrder(delList);
                }
                
                //--------------------------------
                //�����f�[�^�X�V����(�ǉ��E�X�V)
                //--------------------------------
                list = line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0")) && !string.IsNullOrWhiteSpace(x.PartsNumber)).ToList();  //Mod 2017/11/07 arc yano #3806
                //list = line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).ToList(); 
                for (i = 0; i < list.Count; i++)
                {
                    PartsPurchaseOrder l = list[i];

                    //���������̏ꍇ�́A�����σX�e�[�^�X�̍X�V�ƁA�x���\��f�[�^�̍쐬���s��
                    if (form["OrderFlag"].Equals("1"))
                    {
                        //������������Validation�`�F�b�N
                        ValidatePurchaseOrder(form, l, cnt);
                        
                        if (!ModelState.IsValid)
                        {
                            form["OrderFlag"] = "0";
                            SetDataComponent(form);
                            return View(viewName, line);
                        }
                      
                        //----------------------------
                        //�����f�[�^�̕ҏW
                        //----------------------------
                        l.PurchaseOrderStatus = STS_PURCHASEORDER_ORDERED;  //�����ς݃X�e�[�^�X
                        l.RemainingQuantity = l.Quantity;                   //�����c��

                        //----------------------------
                        //�x���\��f�[�^�̍X�V
                        //---------------------------- 
                        plan = CreatePaymentPlan(l, plan);

                        if (!string.IsNullOrWhiteSpace(l.ServiceSlipNumber))
                        {
                            //----------------------------
                            //�T�[�r�X�`�[�������̍X�V
                            //---------------------------- 
                            updateOrderQuantity(l);
                        }
                    }

                    //------------------------------
                    //�����f�[�^��DB�o�^
                    //------------------------------
                    PartsPurchaseOrder order = (new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber) ?? new PartsPurchaseOrder());
                    order = SetOrder(l, order);
                                        
                    cnt++;                                  //�J�E���g�C���N�������g
                }

                //�Ō�Ɏx���\���insert
                if (plan != null && !string.IsNullOrWhiteSpace(plan.PurchaseOrderNumber))
                {
                    db.PaymentPlan.InsertOnSubmit(plan);
                }
                try
                {
                    db.SubmitChanges();
                    //�R�~�b�g
                    ts.Complete();
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
                        form["OrderFlag"] = "0";
                        SetDataComponent(form);
                        return View(viewName, line);
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
                    //Add 2014/08/04 arc amii �G���[���O�Ή� SqlException�ȊO�̎��̃G���[�����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            if (form["OrderFlag"].Equals("1"))
            {
                ModelState.Clear();
                ModelState.AddModelError("", MessageUtils.GetMessage("I0003", "��������"));
                SetDataComponent(form, line[0]);
                return View(viewName, list);
            }
            else
            {
                ModelState.Clear();
                ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
                SetDataComponent(form);
                return View(viewName, list);
            }
        }

        /// <summary>
        /// ���i�����f�[�^�̕ҏW
        /// </summary>
        /// <param name="orderList">���i�����f�[�^</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>Add 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�</history>
        [AcceptVerbs(HttpVerbs.Post)]
        protected virtual List<PartsPurchaseOrder> SetOrderList(List<PartsPurchaseOrder> orderList, FormCollection form)
        {
            //���ʕ������t�H�[���̒l���Z�b�g

            //�����ԍ����󕶎��A�܂���null�̏ꍇ�͐V�K�̔Ԃ���B
            if (string.IsNullOrWhiteSpace(form["PurchaseOrderNumber"]))
            {
                form["PurchaseOrderNumber"] = new SerialNumberDao(db).GetNewPartsPurchaseOrderNumber();
            }

            if(orderList != null && orderList.Count > 0)
            {
                for(int i=0; i < orderList.Count; i++)
                {
                    //�󒍓`�[�ԍ�
                    orderList[i].ServiceSlipNumber = form["ServiceSlipNumber"];
                    //������
                    orderList[i].PurchaseOrderDate = !string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["PurchaseOrderDate"].ToString()) : null;
                    //�����X�e�[�^�X
                    orderList[i].PurchaseOrderStatus = form["PurchaseOrderStatus"];
                    //����R�[�h
                    orderList[i].DepartmentCode = form["DepartmentCode"];
                    //�S���҃R�[�h
                    orderList[i].EmployeeCode = form["EmployeeCode"];
                    //�����ԍ�
                    orderList[i].PurchaseOrderNumber = form["PurchaseOrderNumber"];
                    //�d����R�[�h
                    orderList[i].SupplierCode = form["SupplierCode"];
                    //�x����R�[�h
                    orderList[i].SupplierPaymentCode = form["SupplierPaymentCode"];
                    //Web�I�[�_�[�ԍ�
                    orderList[i].WebOrderNumber = form["WebOrderNumber"];
                    //���ח\���
                    orderList[i].ArrivalPlanDate = !string.IsNullOrWhiteSpace(form["ArrivalPlanDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["ArrivalPlanDate"].ToString()) : null;
                    //�x���\���
                    orderList[i].PaymentPlanDate = !string.IsNullOrWhiteSpace(form["PaymentPlanDate"]) ? (Nullable<DateTime>)DateTime.Parse(form["PaymentPlanDate"].ToString()) : null;
                    //�I�[�_�[�敪
                    if (!string.IsNullOrWhiteSpace(form["OrderType"]))
                    {
                        orderList[i].OrderType = form["OrderType"];
                    }
                    else
                    {
                        orderList[i].OrderType = form["HdOrderType"];
                    }
                    
                    if (!string.IsNullOrWhiteSpace(orderList[i].SupplierCode))
                    {
                        //�d����̐ݒ�
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierName))
                        {
                            orderList[i].SupplierName = new SupplierDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierDao(db).GetByKey(orderList[i].SupplierCode).SupplierName : "";
                        }
                        //�x����̐ݒ�
                        if (string.IsNullOrWhiteSpace(orderList[i].SupplierPaymentName))
                        {
                            orderList[i].SupplierPaymentName = new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode) != null ? new SupplierPaymentDao(db).GetByKey(orderList[i].SupplierCode).SupplierPaymentName : "";
                        }
                    }
                }
            }

            return orderList;
        }

        /// <summary>
        /// �����f�[�^�폜����
        /// </summary>
        /// <param name="orderList">���i�����f�[�^(��ʂ̓��͒l)</param>
        /// <returns></returns>
        /// <history>
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private void DelOrder(List<PartsPurchaseOrder> list)
        {
            //���X�g�����[�v
            foreach(var l in list )
            {
                if (!string.IsNullOrWhiteSpace(l.PurchaseOrderNumber) && !string.IsNullOrWhiteSpace(l.PartsNumber))
                {
                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber);

                    //�����f�[�^�i��ʂ̓��͓��e�j��DB�ɓo�^����Ă���A�Ȃ����폜�σf�[�^�̏ꍇ�A����������
                    if (order != null)
                    {
                        order.DelFlag = l.DelFlag;
                    }
                }
            }
        }



        /// <summary>
        /// ���i�����f�[�^�̕ҏW(Excel�p�ǉ�����)
        /// </summary>
        /// <param name="orderList">���i�����f�[�^</param>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns></returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �V�K�ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        protected virtual List<PartsPurchaseOrder> SetOrderListForExcel(List<PartsPurchaseOrder> orderList, FormCollection form)
        {
            if (orderList != null && orderList.Count > 0)
            {
                for (int i = 0; i < orderList.Count; i++)
                {
                    //-----------------------
                    //Excel�o�͗p�ɐݒ肷��
                    //-----------------------
                    //�S���Җ�
                    if (!string.IsNullOrWhiteSpace(orderList[i].EmployeeCode))
                    {
                        orderList[i].EmployeeName = new EmployeeDao(db).GetByKey(orderList[i].EmployeeCode) != null ? new EmployeeDao(db).GetByKey(orderList[i].EmployeeCode).EmployeeName : "";
                    }
                    //�ԑ�ԍ�
                    if (!string.IsNullOrWhiteSpace(orderList[i].ServiceSlipNumber))
                    {
                        orderList[i].SalesCarNumber = new ServiceSalesOrderDao(db).GetBySlipNumber(orderList[i].ServiceSlipNumber) != null ? new ServiceSalesOrderDao(db).GetBySlipNumber(orderList[i].ServiceSlipNumber).SalesCarNumber : "";
                    }
                    //���喼
                    if (!string.IsNullOrWhiteSpace(orderList[i].DepartmentCode))
                    {
                        orderList[i].DepartmentName = new DepartmentDao(db).GetByKey(orderList[i].DepartmentCode) != null ? new DepartmentDao(db).GetByKey(orderList[i].DepartmentCode).DepartmentName : "";
                    }
                    //�I�[�_�[�敪��
                    if (!string.IsNullOrWhiteSpace(orderList[i].OrderType))
                    {
                        orderList[i].OrderTypeName = new CodeDao(db).GetOrderType(orderList[i].OrderType) != null ? new CodeDao(db).GetOrderType(orderList[i].OrderType).Name : "";
                    }

                    //���i�ԍ�(WPH���̕��i�ԍ������[�J���̕��i�ԍ��֕ϊ�)
                    //�I�[�_�[�敪��
                    if (!string.IsNullOrWhiteSpace(orderList[i].PartsNumber))
                    {
                        orderList[i].MakerPartsNumber = new PartsDao(db).GetByKey(orderList[i].PartsNumber) != null ? new PartsDao(db).GetByKey(orderList[i].PartsNumber).MakerPartsNumber : "";
                    }
                }
            }
            return orderList;
        }

        /// <summary>
        /// �T�[�r�X�`�[�̔������̍X�V
        /// </summary>
        /// <param name="line">���i�����f�[�^�i�P�s���j</param>
        /// <param name="workLines">�T�[�r�X�`�[(���������X�V)</param>
        /// <returns>�T�[�r�X�`�[(�������X�V)</returns>
        /// <history>
        /// 2018/02/24 arc yano #3831 �T�[�r�X�`�[���́@���i���������i���׌�̍Ĕ������̔������̕s�
        /// 2016/02/01 arc yano #3419 ���i�������́@DelFlag���󕶎��œo�^�����s��̑Ή�
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private void updateOrderQuantity(PartsPurchaseOrder l)
        {
            //�󒍓`�[�ԍ��A���i�ԍ��A���f����T�[�r�X�`�[�̖��׍s�̍X�V
            List<ServiceSalesLine> workLines = new ServiceSalesOrderDao(db).GetLineByPartsNumber(l.ServiceSlipNumber, l.PartsNumber, l.OrderType);

            if (workLines.Count > 0)
            {
                decimal? orderQuantity = l.Quantity;

                //Mod 2018/02/24 arc yano #3831
                for (int k = 0; k < workLines.Count; k++)
                {
                    //�����������Z�o
                    decimal? notordering = (workLines[k].Quantity ?? 0m) - (workLines[k].OrderQuantity ?? 0m);

                    //���������ʂ�1�ȏ�̏ꍇ
                    if (notordering > 0)
                    {
                        //���������T�[�r�X�`�[�̔̔����ȏ�̏ꍇ�͔̔�����ݒ�
                        if (orderQuantity >= notordering)
                        {
                            workLines[k].OrderQuantity += notordering;
                            orderQuantity -= notordering;
                        }
                        else
                        {
                            workLines[k].OrderQuantity += orderQuantity;
                            orderQuantity = 0;
                            break;
                        }
                    }

                    //���׍ŏI�s�̏ꍇ�͔�������S�ĉ��Z
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity += orderQuantity;
                        orderQuantity = 0;
                        break;
                    }
                }

                /*
                for (int k = 0; k < workLines.Count; k++)
                {
                    //���׍ŏI�s�̏ꍇ�͔�������S�ĉ��Z
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity += orderQuantity;
                        orderQuantity = 0;
                        break;
                    }
                    else //�ŏI�s�ȊO�́A�������čX�V����
                    {
                        //���������T�[�r�X�`�[�̔̔����ȏ�̏ꍇ�͔̔�����ݒ�
                        if (orderQuantity >= workLines[k].Quantity)
                        {
                            workLines[k].OrderQuantity += workLines[k].Quantity;
                            orderQuantity -= workLines[k].Quantity;
                        }
                        else
                        {
                            workLines[k].OrderQuantity += orderQuantity;
                            orderQuantity = 0;
                            break;
                        }
                    }
                }
                */
            }

            return;
        }
        /// <summary>
        /// �T�[�r�X�`�[�̔������̎��
        /// </summary>
        /// <param name="l">���i�����f�[�^�i�P�s���j</param>
        /// <param name="workLines">�T�[�r�X�`�[(���������X�V)</param>
        /// <returns>�T�[�r�X�`�[(�������X�V)</returns>
        /// <history>
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �V�K�쐬
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private List<ServiceSalesLine> cancelOrderQuantity(PartsPurchaseOrder l, List<ServiceSalesLine> workLines)
        {
            //�󒍓`�[�ԍ��A���i�ԍ��A���f����T�[�r�X�`�[�̖��׍s�̍X�V
            workLines = new ServiceSalesOrderDao(db).GetLineByPartsNumber(l.ServiceSlipNumber, l.PartsNumber, l.OrderType);

            if (workLines.Count > 0)
            {
                decimal? orderQuantity = l.Quantity;

                for (int k = 0; k < workLines.Count; k++)
                {
                    //���׍ŏI�s�̏ꍇ�͔�������S�ăZ�b�g
                    if (k == (workLines.Count - 1))
                    {
                        workLines[k].OrderQuantity -= orderQuantity;
                        orderQuantity -= orderQuantity;
                        break;
                    }
                    else //�ŏI�s�ȊO�́A�������čX�V����
                    {
                        //���������T�[�r�X�`�[�̔̔����ȏ�̏ꍇ�͔̔�����ݒ�
                        if (orderQuantity >= workLines[k].OrderQuantity)
                        {
                            orderQuantity -= workLines[k].OrderQuantity;
                            workLines[k].OrderQuantity -= workLines[k].OrderQuantity;
                        }
                        else
                        {
                            workLines[k].OrderQuantity -= orderQuantity;
                            orderQuantity -= orderQuantity;
                            break;
                        }
                    }
                }
            }

            return workLines;
        }

        /// <summary>
        /// ���i�����f�[�^�̐ݒ�
        /// </summary>
        /// <param name="orderSource">�����f�[�^(�ҏW��)</param>
        /// <param name="orderDst">�����f�[�^(�ҏW��)</param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/02/01 arc yano #3419 ���i�������́@DelFlag���󕶎��œo�^�����s��̑Ή� ��ʍ��ڂ�DelFlag���X�V���鏈����ǉ�
        /// Add 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        private PartsPurchaseOrder SetOrder(PartsPurchaseOrder orderSrc , PartsPurchaseOrder orderDst)
        {
            //�V�K�쐬�E�X�V
            orderDst.ServiceSlipNumber = orderSrc.ServiceSlipNumber;                                //�󒍓`�[�ԍ�
            orderDst.SupplierCode = orderSrc.SupplierCode;                                          //�d����R�[�h
            orderDst.SupplierPaymentCode = orderSrc.SupplierPaymentCode;                            //�x����R�[�h
            orderDst.EmployeeCode = orderSrc.EmployeeCode;                                          //�Ј��R�[�h
            orderDst.DepartmentCode = orderSrc.DepartmentCode;                                      //����R�[�h
            orderDst.WebOrderNumber = orderSrc.WebOrderNumber;                                      //Web�I�[�_�[�ԍ�
            orderDst.PurchaseOrderDate = orderSrc.PurchaseOrderDate;                                //������
            orderDst.PurchaseOrderStatus = orderSrc.PurchaseOrderStatus;                            //�����X�e�[�^�X
            orderDst.PartsNumber = orderSrc.PartsNumber;                                            //���i�ԍ�
            orderDst.OrderType = orderSrc.OrderType;                                                //�I�[�_�[�敪
            orderDst.Quantity = orderSrc.Quantity;                                                  //����
            orderDst.Cost = orderSrc.Cost;                                                          //����
            orderDst.Price = orderSrc.Price;                                                        //���i
            orderDst.Amount = orderSrc.Amount;                                                      //���z
            orderDst.ArrivalPlanDate = orderSrc.ArrivalPlanDate;                                    //���ח\���
            orderDst.PaymentPlanDate = orderSrc.PaymentPlanDate;                                    //�x���\���
            orderDst.Memo = orderSrc.Memo;                                                          //���l��
            orderDst.RemainingQuantity = orderSrc.RemainingQuantity;                                //�����c��
            orderDst.LastUpdateDate = DateTime.Now;                                                 //�ŏI�X�V��
            orderDst.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;         //�ŏI�X�V��
            orderDst.DelFlag = orderSrc.DelFlag;                                                    //�폜�t���O       

            if (orderDst.PurchaseOrderNumber == null)
            {
                orderDst.PurchaseOrderNumber = orderSrc.PurchaseOrderNumber;                        //�����ԍ�                      
                orderDst.CreateDate = DateTime.Now;                                                 //�쐬����
                orderDst.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;         //�쐬��
                orderDst.DelFlag = orderSrc.DelFlag = "0";                                          //�폜�t���O Mod 2016/02/01 arc yano #3419
                db.PartsPurchaseOrder.InsertOnSubmit(orderDst);
            }

            return orderSrc;
        }

        #endregion
        #region �o�^����@�\
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <param name="line">���׏��</param>
        /// <returns>���</returns>
        /// <history>
        /// Add 2015/11/09 arc yano #3291 �V�K�쐬
        /// </history>
        public ActionResult OrderCancel(FormCollection form, List<PartsPurchaseOrder> line)
        {
            List<ServiceSalesLine> workLines = new List<ServiceSalesLine>();
            PaymentPlan plan = new PaymentPlan();

            //�����f�[�^�̍Đݒ�
            line = SetOrderList(line, form);

            //�����������
            using (TransactionScope ts = new TransactionScope())
            {
                //���ו��̍s�����ǉ��E�X�V���s��
                foreach (var l in line)
                {
                    //----------------------------
                    //�����f�[�^�̍폜
                    //----------------------------
                    
                    //�����f�[�^���擾
                    PartsPurchaseOrder order = new PartsPurchaseOrderDao(db).GetByKey(l.PurchaseOrderNumber, l.PartsNumber);

                    order.DelFlag = "1";
                    //----------------------------
                    //�x���\��f�[�^�̍X�V
                    //---------------------------- 
                    plan = DeletePaymentPlan(order, plan);

                    //----------------------------
                    //�T�[�r�X�`�[�������̍X�V
                    //---------------------------- 
                    if (!string.IsNullOrWhiteSpace(order.ServiceSlipNumber))
                    {
                        workLines = cancelOrderQuantity(order, workLines);
                    }
                }
                try
                {
                    db.SubmitChanges();
                    //�R�~�b�g
                    ts.Complete();
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
                        SetDataComponent(form);
                        return View(viewName, line);
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
                    //Add 2014/08/04 arc amii �G���[���O�Ή� SqlException�ȊO�̎��̃G���[�����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
            }

            //��ʍ��ڂ̐ݒ�
            SetDataComponent(form);

            //����t���O��ݒ�
            ViewData["CancelFlag"] = "1";

            ModelState.AddModelError("", "�������������܂���");

            return View(viewName, line);
        }
        #endregion
        #region Excel�o�͋@�\
        /// <summary>
        /// Excel�t�@�C���̃_�E�����[�h
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <param name="line">�����f�[�^����</param> 
        /// <returns>Excel�f�[�^</returns>
        ///<history>
        /// Add 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) �T�[�r�X�`�[�o�R���̔�����ʕ\���p�̃A�N�V�����ǉ�
        /// </history>
        public virtual ActionResult Download(FormCollection form, List<PartsPurchaseOrder> line)
        {
            //-------------------------------
            //��������
            //-------------------------------  
            // Info���O�o��
            OutputLogger.NLogInputDataInfo(form, FORM_NAME, PROC_NAME_EXCELDOWNLOAD);

            ModelState.Clear();

            //-------------------------------
            //Excel�o�͏���
            //-------------------------------         
            //�t�@�C����(PartsPurchaseOrder_xxx(�����ԍ�)_yyyyMMddhhmiss(�_�E�����[�h����))
            string fileName = "PartsPurchaseOrder" + "_" + form["PurchaseOrderNumber"] + "_" + string.Format("{0:yyyyMMddHHmmss}", DateTime.Now) + ".xlsx";

            //���[�N�t�H���_�擾
            string filePath = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryExcelExportPartPurchaseOrder"]) ? "" : ConfigurationManager.AppSettings["TemporaryExcelExportPartPurchaseOrder"];

            string filePathName = filePath + fileName;

            //�e���v���[�g�t�@�C���p�X�擾
            string tfilePathName = string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrder"]) ? "" : ConfigurationManager.AppSettings["TemplateForPartsPurchaseOrder"];

            //�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��Ȃ��ꍇ
            if (tfilePathName.Equals(""))
            {
                ModelState.AddModelError("", "�e���v���[�g�t�@�C���̃p�X���ݒ肳��Ă��܂���");
                SetDataComponent(form);
                return View(viewName, line);
            }

            line = SetOrderList(line, form);

            line = SetOrderListForExcel(line, form);    //Excel�o�͗p

            //�G�N�Z���f�[�^�쐬
            byte[] excelData = MakeExcelData(form, line, filePathName, tfilePathName);

            if (!ModelState.IsValid)
            {
                SetDataComponent(form);
                return View(viewName, line);
            }

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
        protected byte[] MakeExcelData(FormCollection form, List<PartsPurchaseOrder> line, string fileName, string tfileName)
        {
            //----------------------------
            //��������
            //----------------------------
            ConfigLine configLine;                   //�ݒ�l
            byte[] excelData = null;                 //�G�N�Z���f�[�^
            bool ret = false;
            bool tFileExists = true;                 //�e���v���[�g�t�@�C������^�Ȃ�(���ۂɂ��邩�ǂ���)


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

            //------------------------------
            //�o�̓f�[�^���`
            //------------------------------
            //���׏��
            line = SetOrderList(line, form);
            //�w�b�_���
            List<PartsPurchaseOrder> hList = new List<PartsPurchaseOrder>();
            hList.Add(line[0]);

            //----------------------------
            //���׏��V�[�g�o��
            //----------------------------
            //�f�[�^�ݒ�
            ret = dExport.SetData<PartsPurchaseOrder, InfoPartsExcel>(ref excelFile, line, configLine);
            //----------------------------
            //�w�b�_���V�[�g�o��
            //----------------------------
            configLine = dExport.GetConfigLine(config, 3);

            //�f�[�^�ݒ�
            ret = dExport.SetData<PartsPurchaseOrder, PartsPurchaseOrderHeader>(ref excelFile, hList, configLine);

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
        #endregion

        #region �x���\��
        /// <summary>
        /// �x���\��f�[�^�쐬(�����ԍ����ɍ쐬����)
        /// </summary>
        /// <param name="order">���i�����f�[�^</param>
        private PaymentPlan CreatePaymentPlan(PartsPurchaseOrder order, PaymentPlan plan)
        {
            PaymentPlan ret = null;

            //�x���\��(����)�̔����ԍ��Ɣ������̔����ԍ��������ꍇ
            if (plan.PurchaseOrderNumber == order.PurchaseOrderNumber)
            {
                plan.Amount += order.Amount;                //�������̋��z����悹
                plan.PaymentableBalance += order.Amount;    //�������̎x���c������悹
                ret = plan;
            }
            else
            {
                //�x�����(����)��insert
                if (!string.IsNullOrWhiteSpace(plan.PurchaseOrderNumber))
                {
                    db.PaymentPlan.InsertOnSubmit(plan);        
                }
                
                //�V�K�Ɏx�������쐬����
                Account account = new AccountDao(db).GetByUsageType("SP");
                ConfigurationSetting config = new ConfigurationSettingDao(db).GetByKey("PaymentableDepartmentCode");

                PaymentPlan newplan = new PaymentPlan();
                newplan.AccountCode = account.AccountCode;
                newplan.Amount = order.Amount;
                newplan.CompleteFlag = "0";
                newplan.CreateDate = DateTime.Now;
                newplan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                newplan.DelFlag = "0";
                newplan.DepartmentCode = config.Value;
                newplan.LastUpdateDate = DateTime.Now;
                newplan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                newplan.OccurredDepartmentCode = order.DepartmentCode;
                newplan.PaymentableBalance = order.Amount;
                newplan.PaymentPlanDate = order.PaymentPlanDate;
                newplan.PaymentPlanId = Guid.NewGuid();
                newplan.SlipNumber = order.ServiceSlipNumber;
                newplan.SupplierPaymentCode = order.SupplierPaymentCode;
                newplan.PurchaseOrderNumber = order.PurchaseOrderNumber;

                ret = newplan;
            }

            return ret;
        }
        /// <summary>
        /// �x���\��f�[�^�폜
        /// </summary>
        /// <param name="order">���i�����f�[�^</param>
        private PaymentPlan DeletePaymentPlan(PartsPurchaseOrder order, PaymentPlan plan)
        {
            PaymentPlan ret = plan;
            PaymentPlan condition = new PaymentPlan();

            //�����̎x�����Ɣ����f�[�^�̎x���\��̔����`�[���قȂ�ꍇ
            if (plan.PurchaseOrderNumber != order.PurchaseOrderNumber)
            {
                var rec = new PaymentPlanDao(db).GetByPuchaseOrderNumber(order.PurchaseOrderNumber);

                if (rec != null)
                {
                    rec.DelFlag = "1";      //�폜�t���OON
                }

                ret = rec;
            }

            return ret; 
        }

        #endregion

        #region ���P�[�V�����擾
        /// <summary>
        /// �������P�[�V����(���P�[�V�������:002)���擾����
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        private string GetHikiateLocation(string departmentCode)
        {
            List<Location> hikiateLocation = new LocationDao(db).GetListByLocationType("002", departmentCode, null);
            if (hikiateLocation == null || hikiateLocation.Count == 0)
            {
                ModelState.AddModelError("", "������Ɉ������P�[�V������1���ݒ肳��Ă��܂���");
                return string.Empty;
            }
            else
            {
                return hikiateLocation[0].LocationCode;
            }
        }
        #endregion

        #region ��ʃR���|�[�l���g�̐ݒ�
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g���쐬
        /// </summary>
        /// <param name="form">�t�H�[��</param>
        /// <param name="dSource">���i�����f�[�^</param>
        /// <history>
        /// 2018/09/21 yano #3941 ���i���ׁ@�󒍓`�[�ɓ��ꔭ����ʁA�����ꕔ�i�ԍ��̖��ׂ��������݂����ԂŁA���������ׂ����s����ƃV�X�e���G���[
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή� FCJ�̃f�t�H���g�ݒ��p�~
        /// 2018/01/15 arc yano #3833 ���i�������́E���i���ד��́@�d����Œ�Z�b�g
        /// 2016/01/26 arc yano #3399 ���i������ʂ�[�I�[�_�[�敪]�̔񊈐���(#3397_�y�區�ځz���i�d���@�\���P �ۑ�Ǘ��\�Ή�)
        /// 2015/11/09 arc yano #3291 ���i�d���@�\���P(���i��������) ��ʂ̊e���ڂ̃f�[�^�\�[�X��model����viewdata�ɕύX
        /// </history>
        protected void SetDataComponent(FormCollection form, PartsPurchaseOrder dSource = null)
        {

            CodeDao dao = new CodeDao(db);

            //----------------------------------
            //�t�H�[���̒l�̐ݒ�
            //----------------------------------
            if (dSource != null)
            {
                //�����ԍ�
                form["PurchaseOrderNumber"] = dSource.PurchaseOrderNumber;
                //�󒍓`�[�ԍ�
                form["ServiceSlipNumber"] = dSource.ServiceSlipNumber;
                //������
                form["PurchaseOrderDate"] = string.Format("{0:yyyy/MM/dd}", dSource.PurchaseOrderDate);
                //�����X�e�[�^�X
                form["PurchaseOrderStatus"] = dSource.PurchaseOrderStatus;
                //����R�[�h
                form["DepartmentCode"] = dSource.DepartmentCode;
                //�S���҃R�[�h
                form["EmployeeCode"] = dSource.EmployeeCode;
                //�d����R�[�h
                form["SupplierCode"] = dSource.SupplierCode;
                //�x����R�[�h
                form["SupplierPaymentCode"] = dSource.SupplierPaymentCode;
                //Web�I�[�_�[�ԍ�
                form["WebOrderNumber"] = dSource.WebOrderNumber;
                //���ח\���
                form["ArrivalPlanDate"] = string.Format("{0:yyyy/MM/dd}", dSource.ArrivalPlanDate);
                //�x���\���
                form["PaymentPlanDate"] = string.Format("{0:yyyy/MM/dd}", dSource.PaymentPlanDate);
                //�I�[�_�[�敪
                form["OrderType"] = dSource.OrderType;
            }

            //--------------------------------
            //�w�b�_����(���ʍ���)�̐ݒ�
            //--------------------------------
            //�����ԍ�
            ViewData["PurchaseOrderNumber"] = form["PurchaseOrderNumber"];
            //�󒍓`�[�ԍ�
            ViewData["ServiceSlipNumber"] = form["ServiceSlipNumber"];
            //������
            ViewData["PurchaseOrderDate"] = form["PurchaseOrderDate"];
            //�����X�e�[�^�X
            ViewData["PurchaseOrderStatus"] = form["PurchaseOrderStatus"];
            //�����X�e�[�^�X��
            c_PurchaseOrderStatus status = dao.GetPurchaseOrderStatus(false, ViewData["PurchaseOrderStatus"] != null ? ViewData["PurchaseOrderStatus"].ToString() : "");
            if (status != null)
            {
                ViewData["PurchaseOrderStatusName"] = status.Name;
            }
            //����R�[�h
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            //���喼
            Department dep = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"] != null ? ViewData["DepartmentCode"].ToString() : "");
            if (dep != null)
            {
                ViewData["DepartmentName"] = dep.DepartmentName;
            }
            //�S���҃R�[�h
            ViewData["EmployeeCode"] = form["EmployeeCode"];
            //�S����
            Employee emp = new EmployeeDao(db).GetByKey(ViewData["EmployeeCode"] != null ? ViewData["EmployeeCode"].ToString() : "");
            if (emp != null)
            {
                ViewData["EmployeeNumber"] = emp.EmployeeNumber;
                ViewData["EmployeeName"] = emp.EmployeeName;
            }

             //Del 2018/06/01 arc yano #3894
            ////�d����R�[�h
            ////Add 2018/01/15 arc yano #3833 �����i�Ŏd���悪�󗓂̏ꍇ
            //if (gGenuine.Equals("001") && string.IsNullOrWhiteSpace(form["SupplierCode"]))
            //{
            //    form["SupplierCode"] = form["GenuineSupplierCode"];
            //}
            
            ViewData["SupplierCode"] = form["SupplierCode"];

            //�d����
            Supplier supplier = new SupplierDao(db).GetByKey(ViewData["SupplierCode"] != null ? ViewData["SupplierCode"].ToString() : "");
            if (supplier != null)
            {
                ViewData["SupplierName"] = supplier.SupplierName;
            }

            //�x����R�[�h
            //Del 2018/06/01 arc yano #3894
            //Add 2018/01/15 arc yano #3833 �����i�Ŏd���悪�󗓂̏ꍇ
            //if (gGenuine.Equals("001") && string.IsNullOrWhiteSpace(form["SupplierPaymentCode"]))
            //{
            //    form["SupplierPaymentCode"] = form["GenuineSupplierCode"];
            //}

            ViewData["SupplierPaymentCode"] = form["SupplierPaymentCode"];
            //�x����
            SupplierPayment payment = new SupplierPaymentDao(db).GetByKey(ViewData["SupplierPaymentCode"] != null ? ViewData["SupplierPaymentCode"].ToString() : null);
            if (payment != null)
            {
                ViewData["SupplierPaymentName"] = payment.SupplierPaymentName;
            }
            //Web�I�[�_�[�ԍ�
            ViewData["WebOrderNumber"] = form["WebOrderNumber"];
            //���ח\���
            ViewData["ArrivalPlanDate"] = form["ArrivalPlanDate"];
            //�x���\���
            ViewData["PaymentPlanDate"] = form["PaymentPlanDate"];
            //�I�[�_�[�敪
            ViewData["OrderTypeList"] = CodeUtils.GetSelectListByModel<c_OrderType>(dao.GetOrderTypeAll(false), (!string.IsNullOrWhiteSpace(form["OrderType"]) ? form["OrderType"] : form["HdOrderType"]), false);  //Mod 2018/09/21 yano #3941
            //���������t���O
            ViewData["OrderFlag"] = form["OrderFlag"];

            ////Del 2018/06/01 arc yano #3894
            ////Add 2018/01/15 arc yano #3833
            //if (gGenuine.Equals("001"))
            //{
            //    //�����i�̃f�t�H���g�d����R�[�h
            //    ViewData["GenuineSupplierCode"] = form["GenuineSupplierCode"];
            //    //�����i�̃f�t�H���g�d���於
            //    ViewData["GenuineSupplierName"] = form["GenuineSupplierName"];
            //}
            
            
            //Add 2016/01/26 arc yano
            //�󒍓`�[�ԍ���null�܂��͋󕶎��łȂ��ꍇ�̓I�[�_�[�敪��񊈐�
            if (!string.IsNullOrWhiteSpace(form["ServiceSlipNumber"]))
            {
                ViewData["OrderTypeEdit"] = "0";
            }
            else
            {
                ViewData["OrderTypeEdit"] = "1";
            }
        }
        #endregion

        #region Validation�`�F�b�N
        /// <summary>
        /// Validaton�`�F�b�N
        /// </summary>
        /// <param name="order">���i�����f�[�^</param>
        /// <history>
        /// 2017/11/07 arc yano #3806 ���i�������́@�P�O�s�ǉ��{�^���̒ǉ�
        /// 2016/04/26 arc yano #3511 ���i�������́@validation�`�F�b�N������ �L���ȍs�̂�validation�`�F�b�N���s���悤�ɏC��
        /// </history>
        private void ValidateSave(FormCollection form, List<PartsPurchaseOrder> line)
        {
            //----------------------------
            // �w�b�_���ڂ̌���
            //----------------------------
            //�K�{�`�F�b�N
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            //----------------------------
            // ���׍��ڂ̌���
            //----------------------------
            //���׍s�����A�܂��͗L���Ȗ��׍s�Ȃ�
            if (line == null || line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() == 0)
            {
                ModelState.AddModelError("", "�������X�g�͂P�s�ȏ゠��K�v������܂�");
                return;
            }
            for (int i = 0; i < line.Count; i++)
            {
                string prefix = string.Format("line[{0}].", i);

                if (string.IsNullOrEmpty(line[i].DelFlag) || !line[i].DelFlag.Equals("1")) //Mod 2016/04/26 arc yano #3511
                {
                    //���i�ԍ��K�{�`�F�b�N
                    if (string.IsNullOrWhiteSpace(line[i].PartsNumber))
                    {
                        //ModelState.AddModelError(prefix + "PartsNumber", MessageUtils.GetMessage("E0001", "���i�ԍ�"));
                        //return;
                       //�����X�L�b�v
                       continue;  //2017/11/07 arc yano #3806
                    }
                    else
                    {
                        if (gGenuine.Equals("001"))
                        {
                            //���i�ԍ��̏d���`�F�b�N ���@�����敪 = �u�����v�̏ꍇ�̂݃`�F�b�N����
                            if (line.Where(x => ((x.PartsNumber ?? "").Equals(line[i].PartsNumber)) && (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() > 1)
                            {
                                ModelState.AddModelError(prefix + "PartsNumber", "���ꕔ�i�ԍ��͕����s�ɓo�^�ł��܂���B���i�ԍ��F" + line[i].PartsNumber);
                            }
                        }
                    }
                    //����
                    if (line[i].Quantity == null || line[i].Quantity <= 0)
                    {
                        ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" })); ;
                    }
                    else
                    {
                        if (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                                || (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}$")))
                        {
                        }
                        else
                        {
                            ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" }));
                        }
                    }
                }
            }
        }
        /*
        {
            //----------------------------
            // �w�b�_���ڂ̌���
            //----------------------------
            //�K�{�`�F�b�N
            if (string.IsNullOrEmpty(form["EmployeeCode"]))
            {
                ModelState.AddModelError("EmployeeCode", MessageUtils.GetMessage("E0001", "�S����"));
            }
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����"));
            }
            //----------------------------
            // ���׍��ڂ̌���
            //----------------------------
            //���׍s�����A�܂��͗L���Ȗ��׍s�Ȃ�
            if(line == null || line.Where(x => (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() == 0)
            {
                ModelState.AddModelError("", "�������X�g�͂P�s�ȏ゠��K�v������܂�");
                return;
            }
            for(int i = 0; i < line.Count; i++)
            {
                string prefix = string.Format("line[{0}].", i);

                if (string.IsNullOrEmpty(line[i].DelFlag) || !line[i].DelFlag.Equals("1")) //Mod 2016/04/26 arc yano #3511
                {
                    //���i�ԍ��K�{�`�F�b�N
                    if (string.IsNullOrWhiteSpace(line[i].PartsNumber))
                    {
                        ModelState.AddModelError(prefix + "PartsNumber", MessageUtils.GetMessage("E0001", "���i�ԍ�"));
                        return;
                    }
                    else
                    {
                        if (gGenuine.Equals("001"))
                        {
                            //���i�ԍ��̏d���`�F�b�N ���@�����敪 = �u�����v�̏ꍇ�̂݃`�F�b�N����
                            if (line.Where(x => ((x.PartsNumber ?? "").Equals(line[i].PartsNumber)) && (string.IsNullOrWhiteSpace(x.DelFlag) || x.DelFlag.Equals("0"))).Count() > 1)
                            {
                                ModelState.AddModelError(prefix + "PartsNumber", "���ꕔ�i�ԍ��͕����s�ɓo�^�ł��܂���B���i�ԍ��F" + line[i].PartsNumber);
                            }
                        }
                    }
                    //����
                    if (line[i].Quantity == null || line[i].Quantity <= 0)
                    {
                        ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" })); ;
                    }
                    else
                    {
                        if (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}\.\d{1,2}$")
                                || (Regex.IsMatch(line[i].Quantity.ToString(), @"^\d{1,7}$")))
                        {
                        }
                        else
                        {
                            ModelState.AddModelError(prefix + "Quantity", MessageUtils.GetMessage("E0002", new string[] { "����", "���̐���7���ȓ�������2���ȓ�" }));
                        }
                    }
                }
            }
        }
        */
        /// <summary>
        /// ������������Validate�`�F�b�N
        /// </summary>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <param name="line">�������</param>
        /// <param name="lineNo">�s�ԍ�</param>
        /// <return></return>
        protected virtual void ValidatePurchaseOrder(FormCollection form, PartsPurchaseOrder line, int lineNo)
        {
            //---------------------
            //�K�{�`�F�b�N
            //---------------------
            //�d����
            if (string.IsNullOrEmpty(form["SupplierCode"]))
            {
                ModelState.AddModelError("SupplierCode", MessageUtils.GetMessage("E0001", "�d����"));
            }
            //�x����
            if (string.IsNullOrEmpty(form["SupplierPaymentCode"]))
            {
                ModelState.AddModelError("SupplierPaymentCode", MessageUtils.GetMessage("E0001", "�x����"));
            }
            //������
            if (string.IsNullOrWhiteSpace(form["PurchaseOrderDate"]))
            {
                ModelState.AddModelError("PurchaseOrderDate", MessageUtils.GetMessage("E0009", new string[] { "��������", "������" }));
            }
            //�����\���
            if (string.IsNullOrWhiteSpace(form["ArrivalPlanDate"]))
            {
                ModelState.AddModelError("ArrivalPlanDate", MessageUtils.GetMessage("E0009", new string[] { "��������", "���ח\���" }));
            }
            //�x���\���
            if (string.IsNullOrWhiteSpace(form["PaymentPlanDate"]))
            {
                ModelState.AddModelError("PaymentPlanDate", MessageUtils.GetMessage("E0009", new string[] { "��������", "�x���\���" }));
            }
            
            return;
        }
        #endregion

        #region " �V�X�e�����O_�������擾 "
        /// <summary>
        /// �V�X�e�����O�o�͗p�̏������擾
        /// </summary>
        /// <param name="form"></param>
        /// <returns>2014/09/08 arc amii �ǉ�</returns>
        private string GetSysLogProcName(FormCollection form)
        {
            // Add 2014/09/08 arc amii �G���[���O�Ή� ��ʓ��͒l��SysLog�ɏo�͂���
            string procName = "";
            if (CommonUtils.DefaultString(form["Delflag"]).Equals("1"))
            {
                procName = "���i�������";
            }
            else if (CommonUtils.DefaultString(form["OrderFlag"]).Equals("1"))
            {
                procName = "���i����";
            }
            else
            {
                procName = "�ۑ�";
            }

            return procName;
        }
        #endregion


        #region �s�ǉ��E�폜
        /// <summary>
        /// �������X�g��1�s�ǉ�����
        /// </summary>
        /// <param name="line"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/07 arc yano #3806 ���i�������́@�P�O�s�{�^���̒ǉ�
        /// 2017/02/14 arc yano #3641 ���z���̃J���}�\���Ή�
        /// </history>
        public ActionResult AddOrder(List<PartsPurchaseOrder> line, FormCollection form)
        {
            if (line == null)
            {
                line = new List<PartsPurchaseOrder>();
            }
           
            //Mod 2017/11/07 arc yano #3806
            int addLine = int.Parse(form["addLine"]);

            for (int i = 0; i < addLine; i++)
            {
                //�����f�[�^��V�K�쐬
                PartsPurchaseOrder order = new PartsPurchaseOrder();
                order.DelFlag = "0";     //�폜�t���OOFF

                line.Add(order);
            }


            //Del 2016/04/26 arc yano
            //���ח\����Ǝx���\����̏����l���Z�b�g����
            //DateTime PurchaseOrderDate  = DateTime.Parse(form["PurchaseOrderDate"]);
            //order.PaymentPlanDate = CommonUtils.GetFinalDay(PurchaseOrderDate.AddMonths(1).Year, PurchaseOrderDate.AddMonths(1).Month);
            //order.ArrivalPlanDate = PurchaseOrderDate.AddDays(1);

            ModelState.Clear(); //Add 2017/02/14 arc yano #3641

            SetDataComponent(form);

            return View(viewName, line);
        }

        /// <summary>
        /// �������X�g��1�s�폜����
        /// </summary>
        /// <param name="id"></param>
        /// <param name="line"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/02/17 arc yano ���͌��؂̃R�����g�A�E�g
        /// </history>
        public ActionResult DelOrder(int id, List<PartsPurchaseOrder> line, FormCollection form)
        {
            ModelState.Clear();

            //���͌���
            //ValidateSave(form, line);

            if (ModelState.IsValid)
            {
                line[id].DelFlag = "1";         //�폜�t���OON
            }

            ModelState.Clear();

            SetDataComponent(form);

            return View(viewName, line);
        }
        #endregion

        #region 
        /// <summary>
        /// �����`�[�ԍ��ƕ��i�ԍ����甭���c���擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�����`�[�ԍ�</param>
        /// <param name="code">���i�ԍ�</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2016/03/22 arc yano �����̌^��ύX(PurchaseOrderEntryKeyList��Dictionary)
        /// Mod 2016/04/25 arc nakayama #3494_���i���ד��͉�ʁ@�������̂Ȃ����׃f�[�^�ł̃G���[
        /// </history>
        public ActionResult GetRemainingQuantity(List<Dictionary<string, string>> KeyList)
        {
            if (Request.IsAjaxRequest())
            {
                List<PurchaseOrderEntryKeyList> PurchaseOrderRetList = new List<PurchaseOrderEntryKeyList>();
                PartsPurchaseOrder PurchaseOrder = new PartsPurchaseOrder();

                for (int i = 0; i < KeyList.Count; i++)
                {
                    PurchaseOrderEntryKeyList PurchaseOrderRet = new PurchaseOrderEntryKeyList();

                    //�����`�[�ԍ��܂��͕��i�ԍ����Ȃ������ꍇ�͔����c��NULL�̃��X�g���Z�b�g����
                    if (!string.IsNullOrWhiteSpace(KeyList[i]["PurchaseQuantity"]) && !string.IsNullOrWhiteSpace(KeyList[i]["PurchaseOrderNumber"]) && !string.IsNullOrWhiteSpace(KeyList[i]["PartsNumber"]))
                    {
                        PurchaseOrder = new PartsPurchaseOrderDao(db).GetByKey(KeyList[i]["PurchaseOrderNumber"], KeyList[i]["PartsNumber"]);
                        if (PurchaseOrder != null)
                        {
                            PurchaseOrderRet.PurchaseOrderNumber = PurchaseOrder.PurchaseOrderNumber;
                            PurchaseOrderRet.PartsNumber = PurchaseOrder.PartsNumber;
                            PurchaseOrderRet.RemainingQuantity = PurchaseOrder.RemainingQuantity;
                        }
                        else
                        {
                            PurchaseOrderRet.PurchaseOrderNumber = KeyList[i]["PurchaseOrderNumber"];
                            PurchaseOrderRet.PartsNumber = KeyList[i]["PartsNumber"];
                            PurchaseOrderRet.RemainingQuantity = null;
                        }
                    }
                    else
                    {
                        PurchaseOrderRet.PurchaseOrderNumber = KeyList[i]["PurchaseOrderNumber"];
                        PurchaseOrderRet.PartsNumber = KeyList[i]["PartsNumber"];
                        PurchaseOrderRet.RemainingQuantity = null;
                    }
                    
                    PurchaseOrderRetList.Add(PurchaseOrderRet);

                }

                return Json(PurchaseOrderRetList);
            }
            else
            {
                return new EmptyResult();
            }

        }
        #endregion

        #region �d����擾(ajax)
        /// <summary>
        /// ����R�[�h���璼�߂̕��i���ׂ̎d������擾����(Ajax��p�j
        /// </summary>
        /// <param name="DepartemnetCode">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2018/06/01 arc yano #3894 ���i���ד��́@JLR�p�f�t�H���g�d����Ή�
        /// </history>
        public ActionResult GetSupplierCodeFromPartsPurchase(string DepartmentCode)
        {
            if (Request.IsAjaxRequest())
            {
                //���̕���̍ŐV�̓��׎��т����Ɏd�����ݒ肷��
                PartsPurchase rec = new PartsPurchaseDao(db).GetPurchaseResult(DepartmentCode, gGenuine).OrderByDescending(x => x.PurchaseDate).FirstOrDefault();

                CodeData data = new CodeData();

                if(rec != null)
                {
                    data.Code = rec.SupplierCode;
                    data.Name = rec.Supplier.SupplierName;
                }

                return Json(data);
            }
            else
            {
                return new EmptyResult();
            }

        }
        #endregion
    }
}
