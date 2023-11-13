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
using Crms.Models;                      //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

using System.Xml.Linq;                  //Add 2018/05/28 arc yano #3886
using OfficeOpenXml;                    //Add 2018/05/28 arc yano #3886
using System.Configuration;             //Add 2018/05/28 arc yano #3886
using Microsoft.VisualBasic;            //Add 2018/05/28 arc yano #3886



namespace Crms.Controllers {

    /// <summary>
    /// ���������@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class DepositController : Controller {
        //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�����Ǘ�";               // ��ʖ�
        private static readonly string PROC_NAME = "�����������דo�^";           // ������ 
        private static readonly string PROC_NAME_KOBETSU = "�ʓ�������";           // ������
        private static readonly string PROC_NAME_EXCELUPLOAD = "�������e�B�ꊇ����";       // ������(Excel�捞)     //Add 2018/05/28 arc yano #3886
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ������ʖ�
        /// </summary>
        protected string criteriaName = "DepositCriteria";
        protected bool isShopDeposit = false;
        protected bool criteriaInit = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public DepositController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// ��������������ʕ\��
        /// </summary>
        /// <returns>���������������</returns>
        /// <history>
        /// 2016/05/18 arc yano #3558 �����Ǘ� ������^�C�v�̍i���ݒǉ�
        /// </history>
        [AuthFilter]
        public ActionResult Criteria() {
            criteriaInit = true;
            FormCollection form = new FormCollection();

            //Add 2016/05/18 arc yano #3558
            if (!isShopDeposit) //�����Ǘ���ʂ̏ꍇ
            {
                form["CustomerClaimFilter"] = "001";        //�f�t�H���g�l�̐ݒ�
            }
            //Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�\���P�ʂ�ύX�ł��錏�����ǉ�
            form["DispFlag"] = "0"; //�\���P�ʂ̃f�t�H���g�l�@�i�O�F���ׁ@�P�F�T�}���j
            form["DefaultDispFlag"] = form["DispFlag"];

            return Criteria(new EntitySet<Journal>(), form);
        }

        /// <summary>
        /// ��������������ʕ\��
        /// </summary>
        /// <param name="line">���f���f�[�^(�������̓o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���������������</returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 �X�ܓ��������[�T�}���[�\���ł̓��������s�� �����z���v���X�̏ꍇ�ƃ}�C�i�X�̏ꍇ�ŏ����Ώۂ�ύX����
        /// 2016/05/17 arc yano #3543 �X�ܓ��������@���������o�^�����̃X�L�b�v ���z���O�̏ꍇ�A���т��쐬���Ȃ��悤�ɏC��
        /// </history>
        [AuthFilter]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(EntitySet<Journal> line, FormCollection form) {

            // �f�t�H���g�l�̐ݒ�
            form["action"] = (form["action"] == null ? "" : form["action"]);
            
            
            // �����������דo�^����
            if (form["action"].Equals("regist") && line != null) {

                // �f�[�^�`�F�b�N
                ValidateJournal(line, form);

                // �f�[�^�o�^����
                if (ModelState.IsValid) {

                    for (int i = 0; i < line.Count; i++) {

                        string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                        //Mod 2016/01/07 arc nakayama #3303_���������o�^�Ń`�F�b�N���t���Ă��Ȃ����̂���������Ă��܂�
                        //�`�F�b�N�������Ă��鍀�ڂ̂ݍX�V�ΏۂƂ���
                        if ((!string.IsNullOrEmpty(form["line[" + i + "].targetFlag"]) && form["line[" + i + "].targetFlag"].Equals("1")))
                        {
                                Journal journal = line[i];

                                // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
                                db = new CrmsLinqDataContext();
                                db.Log = new OutputWriter();

                                using (TransactionScope ts = new TransactionScope())
                                {

                                    ReceiptPlan targetReceiptPlan = new ReceiptPlan();

                                    List<ReceiptPlan> TargetPlanList = new List<ReceiptPlan>();

                                    //Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�\���P�ʂ�ύX�ł��錏�����ǉ�
                                    //���ו\���ŏ������ޏꍇ�͑I�����ꂽ�����\��1���R�[�h�ɑ΂��ē����������s��
                                    if (form[prefix + "AccountType"].Equals("011") || form["KeePDispFlag"].Equals("0"))
                                    {
                                        //Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j �L�[�̓K�C�h�^�ł͂Ȃ�String�^�ɕύX
                                        //�ꌏ�̓����\��擾
                                        targetReceiptPlan = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);
                                        
                                        // �o�[���o�^
                                        InsertJournal(targetReceiptPlan, journal);

                                        // ��������
                                        UpdateReceiptPlan(targetReceiptPlan, journal);
                                    }
                                    else //�����łȂ���΁i�T�}���\���Ȃ�j�A���z�̏��������ɏ�������ł���
                                    {
                                        //Mod 2017/03/06 arc nakayama #3719_�X�ܓ��������@�c�Ɖ������������ł���
                                        //���z�̏��������ɏ������s��
                                        //�@�`�[�ԍ��Ɛ����悩������\����擾
                                        if (isShopDeposit)
                                        {
                                            TargetPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(form[prefix + "SlipNumber"], form[prefix + "CustomerClaimCode"]).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList();
                                        }
                                        else
                                        {
                                            //Mod 2018/02/07 arc yano #3818
                                            //ReceiptPlanId��'00000000-0000-0000-0000-000000000000'�ȊO�̏ꍇ
                                            if (!string.IsNullOrWhiteSpace(form["line[" + i + "].ReceiptPlanId"]) && !(form["line[" + i + "].ReceiptPlanId"]).Equals("00000000-0000-0000-0000-000000000000"))
                                            {
                                                ReceiptPlan rp = new ReceiptPlanDao(db).GetByKey(new Guid(form["line[" + i + "].ReceiptPlanId"]));

                                                TargetPlanList.Add(rp);
                                            }
                                            else
                                            {
                                                TargetPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(form[prefix + "SlipNumber"], form[prefix + "CustomerClaimCode"]).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList();
                                            }
                                        }
                                        // �o�[���o�^
                                        InsertJournal(TargetPlanList.FirstOrDefault(), journal);

                                        /*
                                        //Mod 2017/12/12 arc yano #3818 ���т��v���X�̏ꍇ�̓v���X�̗\����A�}�C�i�X�̏ꍇ�̓}�C�i�X�̗\�����������
                                        if (journal.Amount >= 0)
                                        {
                                            TargetPlanList = TargetPlanList.Where(x => x.Amount >= 0).ToList();
                                        }
                                        else
                                        {
                                            TargetPlanList = TargetPlanList.Where(x => x.Amount < 0).ToList();
                                        }
                                        */

                                        //���Ȃ����ɏ���
                                        Payment(TargetPlanList, journal.Amount);

                                    }

                                    //Add 2014/08/11 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�try catch����ǉ�
                                    for (int j = 0; j < DaoConst.MAX_RETRY_COUNT; j++)
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
                                            if (j + 1 >= DaoConst.MAX_RETRY_COUNT)
                                            {
                                                // �Z�b�V������SQL����o�^
                                                Session["ExecSQL"] = OutputLogData.sqlText;
                                                // ���O�ɏo��

                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
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
                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
                                                ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "���������o�^"));
                                            }
                                            else
                                            {
                                                // ���O�ɏo��
                                                if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                                }
                                                else
                                                {
                                                    OutputLogger.NLogFatal(se, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                                }
                                                // �G���[�y�[�W�ɑJ��
                                                return View("Error");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            // �Z�b�V������SQL����o�^
                                            Session["ExecSQL"] = OutputLogData.sqlText;
                                            if (form[prefix + "AccountType"].Equals("011") || form["DispType"].Equals("0"))
                                            {
                                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, targetReceiptPlan.SlipNumber);
                                            }
                                            else
                                            {
                                                OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, form[prefix + "SlipNumber"]);
                                            }
                                            return View("Error");
                                        }
                                    }

                                }
                            //} //Del 2016/05/17 arc yano #3543
                        }
                    }
                    ModelState.Clear();
                }
            } else {
                ModelState.Clear();
            }

            // �������ʃ��X�g�̎擾
            PaginatedList<ReceiptPlan> list;
            if (criteriaInit) {
                list = new PaginatedList<ReceiptPlan>();
            } else {
                list = GetSearchResultList(form);
            }

            // ���̑��o�͍��ڂ̐ݒ�
            GetCriteriaViewData(list, line, form);

            // ��������������ʂ̕\��
            return View(criteriaName, list);
        }

        /// <summary>
        /// �������̈��
        /// </summary>
        public void PrintInvoice() {
            string id = Request["PlanId"];
            Response.Redirect("/Report/Print?reportName=Invoice&reportParam=" + id);
        }

        #region �ʓ���
        /// <summary>
        /// �ʓ�����ʂ̕\��
        /// </summary>
        /// <param name="id">�����\��ID</param>
        /// <returns></returns>
        /// <history>Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�L�[�̓K�C�h�^�ł͂Ȃ�String�^�ɕύX</history>
        public ActionResult Detail(string strReceiptPlanId, string SlipNumber, string CustomerClaimCode, string KeePDispFlag)
        {
            ReceiptPlan receiptPlan = new ReceiptPlan();

            if (KeePDispFlag.Equals("0"))
            {
                receiptPlan = new ReceiptPlanDao(db).GetByStringKey(strReceiptPlanId);

            }
            else
            {
                receiptPlan = new ReceiptPlanDao(db).GetByCustomerClaim(SlipNumber, CustomerClaimCode);
                //���v���z�Ǝc�������W�v�����l�ɒu�������āA�����\������󔒂ɂ���AID����ɂ���
                
                ReceiptPlan Condition = new ReceiptPlan();
                Condition.CustomerClaim = new CustomerClaim();
                Condition.Account = new Account();

                Condition.SlipNumber = SlipNumber;
                Condition.CustomerClaim.CustomerClaimCode = CustomerClaimCode;
                Condition.CustomerClaimFilter = "000";
                Condition.IsShopDeposit = true;

                List<ReceiptPlan> SumPlan = new ReceiptPlanDao(db).GetSummaryListByCondition(Condition);
                receiptPlan.ReceiptPlanId = Guid.Empty;
                receiptPlan.Amount = SumPlan.Sum(x => x.Amount) ?? 0m;
                receiptPlan.ReceivableBalance = SumPlan.Sum(x => x.ReceivableBalance) ?? 0m;
                receiptPlan.ReceiptPlanDate = null;
            }
            EntitySet<Journal> journalList = new EntitySet<Journal>();
            ViewData["DepartmentCode"] = receiptPlan.DepartmentCode;
            ViewData["OfficeCode"] = receiptPlan.ReceiptDepartment.OfficeCode;
            SetModelData(receiptPlan, journalList, new FormCollection());

            return View("ShopDepositDetail", receiptPlan);
        }
        /// <summary>
        /// �ʓ�����������
        /// </summary>
        /// <param name="receiptPlan">�����\��f�[�^</param>
        /// <param name="journalList">���������f�[�^���X�g</param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 ���т��v���X�̏ꍇ�̓v���X�̗\����A�}�C�i�X�̏ꍇ�̓}�C�i�X�̗\�����������
        /// 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�L�[�̓K�C�h�^�ł͂Ȃ�String�^�ɕύX
        /// 2016/05/18 arc yano #3560 �J�[�h���� �J�[�h�������́A���тɂ͗\���ID���A�\��ɂ͎��т�ID��ݒ肵�A�R�t���s��
        /// 2016/05/18 arc yano #3559 �u�J�[�h�v�ˁu�J�[�h��Ђ���̓����v�ڋq����̃J�[�h��������
        /// �@�@�@�@�@�@�@�@�@�@�@�@�@�쐬�����o���p���R�[�h�̓�����ʂ́u�J�[�h��Ђ���̓����v�ɕύX����
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Detail(ReceiptPlan receiptPlan,EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];

            ValidateDepositDetail(receiptPlan, line, form);
            if (!ModelState.IsValid) {
                SetModelData(receiptPlan, line, form);
                return View("ShopDepositDetail", receiptPlan);
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                //Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�L�[�̓K�C�h�^�ł͂Ȃ�String�^�ɕύX

                ReceiptPlan targetReceiptPlan = new ReceiptPlan();
                List<ReceiptPlan> targetReceiptPlanList = new List<ReceiptPlan>();

                //���ɂȂ�����\��
                if (receiptPlan.ReceiptPlanId != Guid.Empty)
                {
                    targetReceiptPlan = new ReceiptPlanDao(db).GetByStringKey(receiptPlan.ReceiptPlanId.ToString());
                }
                else
                {
                    targetReceiptPlan = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode).FirstOrDefault();
                    targetReceiptPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode).Where(x => !x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013")).ToList(); //2017/12/12 arc yano #3818
                    //targetReceiptPlanList = new ReceiptPlanDao(db).GetListByCustomerClaim(receiptPlan.SlipNumber, receiptPlan.CustomerClaimCode);
                }


                //�o�[���ւ̒ǉ�
                foreach (Journal item in line) {

                    //Journal�ւ̓]�L
                    Journal newJournal = new Journal();
                    newJournal.JournalId = Guid.NewGuid();
                    newJournal.JournalDate = item.JournalDate;
                    newJournal.JournalType = "001";
                    newJournal.AccountType = item.AccountType;
                    newJournal.AccountCode = item.AccountCode;
                    newJournal.CustomerClaimCode = receiptPlan.CustomerClaimCode;
                    newJournal.DepartmentCode = form["DepartmentCode"];
                    newJournal.OfficeCode = form["OfficeCode"];
                    newJournal.CashAccountCode = item.CashAccountCode;
                    newJournal.SlipNumber = receiptPlan.SlipNumber;
                    newJournal.Amount = item.Amount;
                    newJournal.Summary = item.Summary;
                    newJournal.CreateDate = DateTime.Now;
                    newJournal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newJournal.LastUpdateDate = DateTime.Now;
                    newJournal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    newJournal.DelFlag = "0";
                    newJournal.ReceiptPlanFlag = "1"; //�����ς�
                   
                    //�ʔ��|���ւ̐U��ւ�
                    if (item.AccountType != null && item.AccountType.Equals("003") && isShopDeposit) {
                        ReceiptPlan newPlan = new ReceiptPlan();
                        newPlan.ReceiptPlanId = Guid.NewGuid();
                        newPlan.AccountCode = item.AccountCode;
                        newPlan.Amount = item.Amount;
                        newPlan.CompleteFlag = "0";
                        newPlan.CustomerClaimCode = item.CustomerClaimCode;
                        newPlan.DepartmentCode = new ConfigurationSettingDao(db).GetByKey("AccountingDepartmentCode").Value; //�o���ɐU��ւ�
                        newPlan.OccurredDepartmentCode = receiptPlan.DepartmentCode;
                        newPlan.ReceiptPlanDate = CommonUtils.GetPaymentPlanDate(item.JournalDate, new CustomerClaimDao(db).GetByKey(item.CustomerClaimCode));
                        //newPlan.ReceiptType = "003"; //�N���W�b�g
                        newPlan.ReceiptType = "011"; //�J�[�h��Ђ���̓���        //Mod 2016/05/18 arc yano #3559
                        newPlan.ReceivableBalance = item.Amount;
                        newPlan.SlipNumber = receiptPlan.SlipNumber;
                        newPlan.Summary = item.Summary;
                        newPlan.CreateDate = DateTime.Now;
                        newPlan.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        newPlan.LastUpdateDate = DateTime.Now;
                        newPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                        newPlan.DelFlag = "0";

                        //Mod 2016/05/18 arc yano #3560
                        //�J�[�h�������s�����������тɍ쐬���ꂽ�o���p���R�[�h�̓����\��ID��ݒ�
                        newJournal.CreditReceiptPlanId = newPlan.ReceiptPlanId.ToString().ToUpper();
                        
                        // ���Ϗ�����ǉ�
                        newPlan.PaymentKindCode = item.PaymentKindCode;
                        PaymentKind paymentKind = new PaymentKindDao(db).GetByKey(item.PaymentKindCode);
                        newPlan.CommissionRate = paymentKind != null ? paymentKind.CommissionRate : 0m;
                        newPlan.CommissionAmount = paymentKind != null ? Math.Truncate(decimal.Multiply(decimal.Multiply(paymentKind.CommissionRate, item.Amount), 0.01m)) : 0m;


                        // �����������ϓ��ƂȂ�
                        newPlan.JournalDate = item.JournalDate;

                        db.ReceiptPlan.InsertOnSubmit(newPlan);

                        newJournal.TransferFlag = "1"; // �]�L�t���O
                    }
                    db.Journal.InsertOnSubmit(newJournal);
                }

                //�����\��f�[�^�̍X�V
                Journal summary = new Journal();
                summary.Amount = line.Sum(x => x.Amount);

                if (receiptPlan.ReceiptPlanId != Guid.Empty)
                {
                    UpdateReceiptPlan(targetReceiptPlan, summary);
                }
                else
                {
                    //���Ȃ����ɏ���
                    Payment(targetReceiptPlanList, summary.Amount);
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
                            OutputLogger.NLogFatal(cfe, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (SqlException se)
                    {
                        // Add 2014/08/06 arc amii �G���[���O�Ή� SqlException�������A�G���[���O�o�͂��鏈����ǉ�
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;

                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            OutputLogger.NLogError(se, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "���������o�^"));
                            SetModelData(receiptPlan, new EntitySet<Journal>(), form);
                            return View("ShopDepositDetail", receiptPlan);
                        }
                        else
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogFatal(se, PROC_NAME_KOBETSU, FORM_NAME, targetReceiptPlan.SlipNumber);
                            // �G���[�y�[�W�ɑJ��
                            return View("Error");
                        }
                    }
                    catch (Exception e)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        OutputLogger.NLogFatal(e, PROC_NAME_KOBETSU, FORM_NAME, "");
                        return View("Error");
                    }
                }
            }
            SetModelData(receiptPlan, new EntitySet<Journal>(), form);
            ViewData["close"] = "1";
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// �ʓ�����ʍs�ǉ�
        /// </summary>
        /// <param name="receiptPlan">�����\��f�[�^</param>
        /// <param name="item">�����f�[�^���X�g</param>
        /// <returns></returns>
        public ActionResult AddLine(ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];

            Journal journal = new Journal();
            journal.JournalDate = DateTime.Today ;
            journal.Account = new AccountDao(db).GetByKey(receiptPlan.AccountCode);
            journal.CustomerClaim = new CustomerClaimDao(db).GetByKey(receiptPlan.CustomerClaimCode);

            if (line == null) {
                line = new EntitySet<Journal>();
            }
            line.Add(journal);
            SetModelData(receiptPlan, line, form);
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// �ʓ�����ʍs�폜
        /// </summary>
        /// <param name="id">�s�ԍ�</param>
        /// <param name="receiptPlan">�����\��f�[�^</param>
        /// <param name="item">�����f�[�^���X�g</param>
        /// <returns></returns>
        public ActionResult DelLine(int id, ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            ViewData["DepartmentCode"] = form["DepartmentCode"];
            ViewData["OfficeCode"] = form["OfficeCode"];
            
            ModelState.Clear();
            line.RemoveAt(id);
            SetModelData(receiptPlan, line, form);
            return View("ShopDepositDetail", receiptPlan);
        }

        /// <summary>
        /// �ʓ�����ʃf�[�^�R���|�[�l���g�̃Z�b�g
        /// </summary>
        /// <param name="receiptPlan"></param>
        /// <param name="item"></param>
        /// <history>
        /// 2016/05/19 arc yano #3549 �X�ܓ�������(�ʏ���)�@������ʃ��X�g�̕ύX
        /// </history>
        private void SetModelData(ReceiptPlan receiptPlan, EntitySet<Journal> line,FormCollection form) {
            if (line == null) {
                line = new EntitySet<Journal>();
            }
            ViewData["JournalList"] = line;

            CodeDao dao = new CodeDao(db);
            List<IEnumerable<SelectListItem>> accountTypeList = new List<IEnumerable<SelectListItem>>();
            //List<c_AccountType> accountTypeListSrc = dao.GetAccountTypeAll(true);
            List<c_AccountType> accountTypeListSrc = dao.GetAccountType("003");   //Mod 2016/05/19 arc yano #3549
            List<IEnumerable<SelectListItem>> cashAccountCodeList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> paymentKindCodeList = new List<IEnumerable<SelectListItem>>();

            foreach (var journal in line) {
                journal.CustomerClaim = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                journal.Account = new AccountDao(db).GetByKey(journal.AccountCode);
                accountTypeList.Add(CodeUtils.GetSelectListByModel(accountTypeListSrc, journal.AccountType, true));
                List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(form["OfficeCode"]);
                List<CodeData> accountDataList = new List<CodeData>();
                foreach (var a in accountList) {
                    CodeData data = new CodeData();
                    data.Code = a.CashAccountCode;
                    data.Name = a.CashAccountName;
                    accountDataList.Add(data);
                }
                cashAccountCodeList.Add(CodeUtils.GetSelectList(accountDataList,journal.CashAccountCode,false));

                if (journal.CustomerClaim != null && journal.CustomerClaim.CustomerClaimable != null) {
                    List<CodeData> customerClaimableDataList = new List<CodeData>();
                    foreach (var customerClaimable in journal.CustomerClaim.CustomerClaimable) {
                        CodeData claimable = new CodeData();
                        claimable.Code = customerClaimable.PaymentKindCode;
                        claimable.Name = customerClaimable.PaymentKind != null ? customerClaimable.PaymentKind.PaymentKindName : "";
                        customerClaimableDataList.Add(claimable);
                    }
                    paymentKindCodeList.Add(CodeUtils.GetSelectListByModel(customerClaimableDataList, journal.PaymentKindCode, true));
                }
            }
            ViewData["AccountTypeList"] = accountTypeList;
            ViewData["CashAccountCodeList"] = cashAccountCodeList;
            ViewData["PaymentKindCodeList"] = paymentKindCodeList;
            receiptPlan.CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            receiptPlan.Account = new AccountDao(db).GetByKey(receiptPlan.AccountCode);
            receiptPlan.CustomerClaim = new CustomerClaimDao(db).GetByKey(receiptPlan.CustomerClaimCode);
            if (ViewData["DepartmentCode"] != null && !ViewData["DepartmentCode"].Equals("")) {
                Department department = new DepartmentDao(db).GetByKey(ViewData["DepartmentCode"].ToString());
                try { ViewData["DepartmentName"] = department.DepartmentName; } catch (NullReferenceException) { }
            }
            if (ViewData["OfficeCode"] != null && !ViewData["OfficeCode"].Equals("")) {
                Office office = new OfficeDao(db).GetByKey(ViewData["OfficeCode"].ToString());
                try { ViewData["OfficeName"] = office.OfficeName; } catch (NullReferenceException) { }
            }

            //Add 2017/04/19 arc nakayama #3754_�X�ܓ��������i�ʁj�s�ǉ��Ŕ[�ԗ\����Ɣ[�ԓ���������ꍇ������
            ServiceSalesHeader sh = new ServiceSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            CarSalesHeader ch = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
            if (sh != null)
            {
                receiptPlan.ServiceSalesHeader = new ServiceSalesHeader();
                receiptPlan.ServiceSalesHeader.SalesDate = sh.SalesDate;
                receiptPlan.ServiceSalesHeader.SalesPlanDate = sh.SalesPlanDate;
            }
            else if (ch != null)
            {
                receiptPlan.CarSalesHeader = new CarSalesHeader();
                receiptPlan.CarSalesHeader.SalesDate = ch.SalesDate;
                receiptPlan.CarSalesHeader.SalesPlanDate = ch.SalesPlanDate;
            }
        }

        /// <summary>
        /// �ʓ�������Validation�`�F�b�N
        /// </summary>
        /// <param name="item">�������׃f�[�^</param>
        /// <history>
        /// 2019/02/07 yano #3966 �X�ܓ�������(�����Ǘ�)�@�������O�̓`�[�C���ɂ������G���[
        /// </history>
        private void ValidateDepositDetail(ReceiptPlan receiptPlan, EntitySet<Journal> line, FormCollection form)
        {
            bool alreadyOutputMsgE0001 = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0003 = false;
            bool alreadyOutputMsgE0004 = false;
            if (string.IsNullOrEmpty(form["DepartmentCode"]))
            {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "�v�㕔��"));
            }
            if (string.IsNullOrEmpty(form["OfficeCode"]))
            {
                ModelState.AddModelError("OfficeCode", MessageUtils.GetMessage("E0001", "�������Ə�"));
            }
            if (line == null)
            {
                ModelState.AddModelError("", "���ׂ�ǉ����Ă�������");
                return;
            }

            //Add 2019/02/07 yano #3966
            ReceiptPlan target = new ReceiptPlanDao(db).GetByKey(receiptPlan.ReceiptPlanId);

            if (target == null)
            {
                ModelState.AddModelError("", "�`�[���X�V���ꂽ���߁A�������s�����Ƃ��ł��܂���B�Č�����ɏ������s���ĉ�����");
            }


            for (int i = 0; i < line.Count(); i++)
            {
                string prefix = string.Format("line[{0}].", i);
                Journal journal = line[i];

                // �K�{�`�F�b�N(���l/���t���ڂ͑����`�F�b�N�����˂�)
                if (!ModelState.IsValidField(prefix + "JournalDate"))
                {
                    ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "������")));
                    alreadyOutputMsgE0003 = true;
                    if (ModelState[prefix + "JournalDate"].Errors.Count > 1)
                    {
                        ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                    }
                }
                if (string.IsNullOrEmpty(journal.AccountType))
                {
                    ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "�������")));
                    alreadyOutputMsgE0001 = true;
                }
                if (!ModelState.IsValidField(prefix + "Amount"))
                {
                    ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                    alreadyOutputMsgE0002 = true;
                    if (ModelState[prefix + "Amount"].Errors.Count > 1)
                    {
                        ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                    }
                }
                if (string.IsNullOrEmpty(journal.CustomerClaimCode))
                {
                    ModelState.AddModelError(prefix + "CustomerClaimCode", (alreadyOutputMsgE0004 ? "" : MessageUtils.GetMessage("E0001", "������")));
                    if (ModelState[prefix + "CustomerClaimCode"].Errors.Count > 1)
                    {
                        ModelState[prefix + "CustomerClaimCode"].Errors.RemoveAt(0);
                    }
                }
                else
                {
                    CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(journal.CustomerClaimCode);
                    //�����悪�}�X�^�ɑ��݂��Ȃ���΃G���[
                    if (customerClaim == null)
                    {
                        ModelState.AddModelError(prefix + "CustomerClaimCode", MessageUtils.GetMessage("E0016", journal.CustomerClaimCode));
                    }
                    else
                    {
                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        //�����悪�l�܂��͖@�l�Ȃ̂ɃJ�[�h��I��ł�����G���[
                        if ((CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("001")
                            || CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("002"))
                            && CommonUtils.DefaultString(journal.AccountType).Equals("003"))
                        {
                            ModelState.AddModelError(prefix + "AccountType", "�w�肵��������̎�ʂ̓J�[�h������I�Ԃ��Ƃ͂ł��܂���");
                        }
                        else
                        {
                            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                            // �����悪�J�[�h��Ђ̏ꍇ�̓J�[�h�����ȊO�s��
                            if ((CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("003")
                                || CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("004"))
                                && !CommonUtils.DefaultString(journal.AccountType).Equals("003"))
                            {
                                ModelState.AddModelError(prefix + "AccountType", "�w�肵��������̎�ʂ̓J�[�h�����ȊO�I�Ԃ��Ƃ͂ł��܂���");
                            }
                            //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                            //�J�[�h��Ђ̏ꍇ���Ϗ����͕K�{�ݒ�
                            if (CommonUtils.DefaultString(customerClaim.CustomerClaimType).Equals("003"))
                            {
                                // �}�X�^�ɂȂ�
                                if (customerClaim.CustomerClaimable == null || customerClaim.CustomerClaimable.Count == 0)
                                {
                                    ModelState.AddModelError(prefix + "CustomerClaimCode", "�w�肵��������͌��Ϗ������ݒ肳��Ă��Ȃ����ߗ��p�ł��܂���");
                                }
                                else if (string.IsNullOrEmpty(journal.PaymentKindCode))
                                {
                                    // �I�����Ă��Ȃ�
                                    ModelState.AddModelError(prefix + "PaymentKindCode", MessageUtils.GetMessage("E0007", new string[] { "�J�[�h����", "���Ϗ���" }));
                                }
                            }
                        }

                    }
                }
                // �t�H�[�}�b�g�`�F�b�N
                if (ModelState.IsValidField(prefix + "Amount"))
                {
                    if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$"))
                    {
                        ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                        alreadyOutputMsgE0002 = true;
                    }
                }

                // �l�`�F�b�N
                if (ModelState.IsValidField(prefix + "JournalDate"))
                {
                    if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0)
                    {
                        ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�{���ȑO" }));
                    }
                    CashBalance cashBalance = GetLatestClosedData(form["OfficeCode"], journal.CashAccountCode);
                    DateTime latestClosedDate = (cashBalance == null ? DaoConst.SQL_DATETIME_MIN : cashBalance.ClosedDate);
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (journal.JournalDate.CompareTo(latestClosedDate) <= 0 && CommonUtils.DefaultString(journal.AccountType).Equals("001"))
                    {
                        ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�O����ߓ�����" }));
                    }

                    // Add 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ� �S�̂ł̏������ʏ������o�����߂�����悤�ɂ���
                    string departmentCode = "";
                    CarSalesHeader carHeader = new CarSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
                    ServiceSalesHeader serviceHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(receiptPlan.SlipNumber);
                    if (carHeader != null){
                        departmentCode = carHeader.DepartmentCode;
                    }else if (serviceHeader != null){
                        departmentCode = serviceHeader.DepartmentCode;
                    }

                    //���O�C�����[�U���擾
                    Employee loginUser = ((Employee)Session["Employee"]);
                    ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //�����f�[�^�ҏW���������邩�Ȃ���

                    //�����f�[�^�ҏW����������Ζ{���߂̎��̂݃G���[
                    if (AppRole.EnableFlag){
                        if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(departmentCode, journal.JournalDate, "001"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�O��̌������ߏ��������s���ꂽ�N������" }));
                        }
                    }else{
                        // �����łȂ���Ή����߈ȏ�ŃG���[
                        if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, journal.JournalDate, "001"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�O��̌������ߏ��������s���ꂽ�N������" }));
                        }
                    }
                    
                    if (ModelState.IsValidField(prefix + "Amount"))
                    {
                        if (journal.Amount.Equals(0m))
                        {
                            ModelState.AddModelError(prefix + "Amount", MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" }));
                        }
                    }
                    CashAccount cashAccount = new CashAccountDao(db).GetByKey(form["OfficeCode"], journal.CashAccountCode);
                    if (cashAccount == null)
                    {
                        ModelState.AddModelError(prefix + "CashAccountCode", "�w�肵�����Ə��ƌ��������̑g�ݍ��킹���s���ł�");
                    }
                }
                //if (receiptPlan.ReceivableBalance < line.Sum(x => x.Amount)) {
                //    ModelState.AddModelError("", "�����z�������\��z�����������ߏ��������ł��܂���");
                //}
            }
        }
        #endregion

        /// <summary>
        /// ������ʕ\���f�[�^�̎擾
        /// </summary>
        /// <param name="list">�����\�茟�����ʃ��X�g</param>
        /// <param name="line">���f���f�[�^(�������̓o�^���e)</param>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <history>
        /// 2016/05/18 arc yano #3558 �����Ǘ�  ������^�C�v�̍i���ݒǉ�
        /// 2016/05/18 arc yano #3562 �����Ǘ��@�ꗗ�̓�����ʂ̏����\�� �ꗗ�̓�����ʂ̏����\��������\��̓�����ʂŕ\������(�����͏���)
        /// 2016/05/18 arc yano #3459 �J�[�h�̏������s���Ȃ��悤�ɂ���
        /// 2016/05/18 arc yano #3546 �X�ܓ��������^�����Ǘ��ɂ��A�I���ł��������ʂ�ύX����
        /// </history>
        private void GetCriteriaViewData(PaginatedList<ReceiptPlan> list, EntitySet<Journal> line, FormCollection form) {

            CodeDao dao = new CodeDao(db);

            // �����������̉�ʕ\���f�[�^�擾
            ViewData["CondDepartmentCode"] = form["CondDepartmentCode"];
            if (!string.IsNullOrEmpty(form["CondDepartmentCode"])) {
                Department department = new DepartmentDao(db).GetByKey(form["CondDepartmentCode"]);
                if (department != null) {
                    ViewData["CondDepartmentName"] = department.DepartmentName;
                }
            }
            ViewData["OfficeCode"] = form["OfficeCode"];
            if (!string.IsNullOrEmpty(form["OfficeCode"])) {
                Office office = new OfficeDao(db).GetByKey(form["OfficeCode"]);
                if (office != null) {
                    ViewData["OfficeName"] = office.OfficeName;
                }
            }
            ViewData["SalesDateFrom"] = form["SalesDateFrom"];
            ViewData["SalesDateTo"] = form["SalesDateTo"];
            ViewData["JournalDateFrom"] = form["JournalDateFrom"];
            ViewData["JournalDateTo"] = form["JournalDateTo"];
            ViewData["ReceiptPlanDateFrom"] = form["ReceiptPlanDateFrom"];
            ViewData["ReceiptPlanDateTo"] = form["ReceiptPlanDateTo"];
            ViewData["CondSlipNumber"] = form["CondSlipNumber"];
            ViewData["CondCustomerClaimCode"] = form["CondCustomerClaimCode"];

            ViewData["CustomerClaimFilter"] = form["CustomerClaimFilter"];      //Add 2016/05/18 arc yano #3558

            List<CodeData> paymentKindList = new List<CodeData>();
            if (!string.IsNullOrEmpty(form["CondCustomerClaimCode"])) {
                CustomerClaim customerClaim = new CustomerClaimDao(db).GetByKey(form["CondCustomerClaimCode"]);
                if (customerClaim != null) {
                    ViewData["CondCustomerClaimName"] = customerClaim.CustomerClaimName;
                }

                foreach (var item in customerClaim.CustomerClaimable) {
                    paymentKindList.Add(new CodeData() { Code = item.PaymentKindCode, Name = item.PaymentKind != null ? item.PaymentKind.PaymentKindName : "" });
                }
                
            }
            ViewData["CustomerClaimTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCustomerClaimTypeAll(false, form["CustomerClaimFilter"]), form["CustomerClaimType"], true);   //Mod 2016/05/18 arc yano #3558
            ViewData["PaymentKindList"] = CodeUtils.GetSelectListByModel(paymentKindList, form["PaymentKindCode"], true);
            ViewData["AccountUsageTypeList"] = CodeUtils.GetSelectListByModel<c_AccountUsageType>(dao.GetAccountUsageTypeAll(false), form["AccountUsageType"], true);


            ViewData["ReceiptTypeList"] = CodeUtils.GetSelectListByModel<c_ReceiptType>(dao.GetReceiptTypeAll(false), form["ReceiptType"], true).Where(x => !x.Value.Equals("013")); //����͕\�����Ȃ�

            // �o�[�����דo�^�y�ь������ʕ��̉�ʕ\���f�[�^�擾
            List<Journal> journalList = new List<Journal>();
            List<IEnumerable<SelectListItem>> accountTypeList = new List<IEnumerable<SelectListItem>>();
            //List<c_AccountType> accountTypeListSrc = dao.GetAccountTypeAll(isShopDeposit ? false : true);
            
            //Mod 2016/05/18 arc yano #3546
            List<c_AccountType> accountTypeListSrc = null;
            
            if (isShopDeposit)
            {
                accountTypeListSrc = dao.GetAccountType("002");
            }
            else
            {
                accountTypeListSrc = dao.GetAccountType("001");        //Mod 2016/05/19 arc yano #3549
            }
            
            List<IEnumerable<SelectListItem>> cashAccountList = new List<IEnumerable<SelectListItem>>();

            for (int i = 0; i < list.Count; i++) { // �o�^�����̓G���[�̏ꍇ��ModelState���N���A���Ȃ��ׁAJournal�������ɂ��Ă͉�ʔ��f����Ȃ��B
                Journal journal = new Journal();
                journal.JournalDate = DateTime.Now.Date;
                journalList.Add(journal);
                string accountType = journal.AccountType;
                if (string.IsNullOrEmpty(accountType)) {
                    switch (list[i].ReceiptType) {
                        case "001":
                            accountType = "";
                            break;

                        /* Mod 2016/05/18 arc yano #3562 �����ȊO�͓����\��̓�����ʂ�ݒ�
                        case "003":
                            accountType = "003";  
                            break;
                        case "004":
                            accountType = "004";
                            break;
                        */
                        default:
                            accountType = list[i].ReceiptType;
                            break;
                    }
                }
                accountTypeList.Add(CodeUtils.GetSelectListByModel(accountTypeListSrc, accountType, true));
                list[i].CarSalesHeader = new CarSalesOrderDao(db).GetBySlipNumber(list[i].SlipNumber);
                list[i].ServiceSalesHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(list[i].SlipNumber);

                List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(list[i].ReceiptDepartment.OfficeCode);
                List<CodeData> accountDataList = new List<CodeData>();
                foreach (var a in accountList) {
                    CodeData data = new CodeData();
                    data.Code = a.CashAccountCode;
                    data.Name = a.CashAccountName;
                    accountDataList.Add(data);
                }
                cashAccountList.Add(CodeUtils.GetSelectList(accountDataList, journal.CashAccountCode, false));
            }

           

            ViewData["JournalList"] = journalList;
            ViewData["AccountTypeList"] = accountTypeList;
            ViewData["TotalBalance"] = list.Count()>0 ? GetTotalBalance(form) : 0m;
            ViewData["CalcTotalBalance"] = list.Count() > 0 ? GetTotalBalance(form) : 0m;
            ViewData["CashAccountList"] = cashAccountList;
            ViewData["TotaljournalAmount"] = "0"; //������̓`�F�b�N�����Ă��Ȃ����߂O������
            //Add 2016/07/20 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j�\���P�ʂ�ύX�ł��錏�����ǉ�
            ViewData["DispFlag"] = form["DispFlag"];
            ViewData["DefaultDispFlag"] = form["DefaultDispFlag"];
            ViewData["KeePDispFlag"] = form["DispFlag"];

        }

        /// <summary>
        /// ���������������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���������������ʃ��X�g</returns>
        /// <history>
        /// 2016/05/17 arc yano #3542 �ꗗ�̍i���ݏ����̕ύX �X�ܓ���������ʂ̏ꍇ�A������^�C�v���u�N���W�b�g�v�u���[���v�u�Г��v��\�����Ȃ�
        /// </history>
        private PaginatedList<ReceiptPlan> GetSearchResultList(FormCollection form) {

            PaginatedList<ReceiptPlan> list = new PaginatedList<ReceiptPlan>();

            //Add 2016/05/17 arc yano #3542 
            //Mod 2016/07/19 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j
            
            //���ו\���̏ꍇ�i�����\���1���R�[�h�Â擾�j
            //�X�ܓ���������ʂ̏ꍇ
            if (form["DispFlag"].Equals("0"))
            {
                if (isShopDeposit)
                {
                    var ret = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form)).Where(
                            x => x.CustomerClaim != null && x.CustomerClaim.c_CustomerClaimType != null
                                                                    && x.CustomerClaim.c_CustomerClaimType.CustomerClaimFilter.Equals("000")     //������^�C�v ���u�N���W�b�g�^���[���^�Г��v
                                                                    && (!x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013"))
                                                                    ).AsQueryable();

                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
                else
                {
                    var ret = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form)).Where(x => !x.ReceiptType.Equals("013")).AsQueryable();


                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                    //list = new ReceiptPlanDao(db).GetListByCondition(GetSearchCondition(form), int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
            }
            else
            {
                if (isShopDeposit)
                {
                    var ret = new ReceiptPlanDao(db).GetSummaryListByCondition(GetSearchCondition(form)).AsQueryable();
                        
                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                }
                else
                {
                    var ret = new ReceiptPlanDao(db).GetSummaryListByCondition(GetSearchCondition(form)).AsQueryable();

                    list = new PaginatedList<ReceiptPlan>(ret, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);

                }
            }
            return list;
           
        }

        /// <summary>
        /// �c�����v�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>���������������ʃ��X�g</returns>
        private decimal GetTotalBalance(FormCollection form) {
            return new ReceiptPlanDao(db).GetTotalBalance(GetSearchCondition(form), isShopDeposit);
        }

        /// <summary>
        /// ���������ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>��������</returns>
        /// <history>
        /// 2016/05/18 arc yano #3558 �����Ǘ� ������^�C�v�̍i���ݒǉ�
        /// 2016/05/17 arc yano #3543 �X�ܓ��������@���������o�^�����̃X�L�b�v
        /// </history>
        private ReceiptPlan GetSearchCondition(FormCollection form) {

            ReceiptPlan receiptPlanCondition = new ReceiptPlan();
            receiptPlanCondition.JournalDateFrom = CommonUtils.GetDayStart(form["JournalDateFrom"], DaoConst.SQL_DATETIME_MAX);
            receiptPlanCondition.JournalDateTo = CommonUtils.GetDayEnd(form["JournalDateTo"], DaoConst.SQL_DATETIME_MIN);
            //receiptPlanCondition.DepartmentCode = ((Employee)Session["Employee"]).DepartmentCode;
            receiptPlanCondition.ReceiptPlanDateFrom = CommonUtils.GetDayStart(form["ReceiptPlanDateFrom"], DaoConst.SQL_DATETIME_MAX);
            receiptPlanCondition.ReceiptPlanDateTo = CommonUtils.GetDayEnd(form["ReceiptPlanDateTo"], DaoConst.SQL_DATETIME_MIN);
            if (!string.IsNullOrEmpty(form["CondSlipNumber"])) {
                try { receiptPlanCondition.SlipNumber = string.Format("{0:00000000}", decimal.Parse(CommonUtils.DefaultString(form["CondSlipNumber"]))); } catch (FormatException) { receiptPlanCondition.SlipNumber = form["CondSlipNumber"]; }
            }
            //receiptPlanCondition.Department = new Department();
            //receiptPlanCondition.Department.DepartmentCode = form["CondDepartmentCode"];
            receiptPlanCondition.OccurredDepartmentCode = form["CondDepartmentCode"];
            receiptPlanCondition.OfficeCode = form["OfficeCode"];
            receiptPlanCondition.CustomerClaim = new CustomerClaim();
            receiptPlanCondition.CustomerClaim.CustomerClaimCode = form["CondCustomerClaimCode"];
            receiptPlanCondition.CustomerClaim.CustomerClaimType = form["CustomerClaimType"];

            receiptPlanCondition.Account = new Account();
            receiptPlanCondition.Account.UsageType = form["AccountUsageType"];
            receiptPlanCondition.ReceiptType = form["ReceiptType"];
            receiptPlanCondition.SetAuthCondition((Employee)Session["Employee"]);
            receiptPlanCondition.PaymentKindCode = form["PaymentKindCode"];
            receiptPlanCondition.SalesDateFrom = CommonUtils.StrToDateTime(form["SalesDateFrom"]);
            receiptPlanCondition.SalesDateTo = CommonUtils.StrToDateTime(form["SalesDateTo"]);

            //Add 2016/05/18 arc yano #3558
            if(!string.IsNullOrWhiteSpace(form["CustomerClaimFilter"]))
            {
                //receiptPlanCondition.CustomerClaim = new CustomerClaim();
                //receiptPlanCondition.CustomerClaim.c_CustomerClaimType = new c_CustomerClaimType();
                receiptPlanCondition.CustomerClaimFilter = form["CustomerClaimFilter"];
            }

            //Add 2016/07/19 arc nakayama #3580_�����\��̃T�}���\���i�������у��X�g�o�́E�X�ܓ����E�����Ǘ��j
            if (isShopDeposit)
            {
                receiptPlanCondition.CustomerClaimFilter = "000";
            }            
            receiptPlanCondition.DispType = form["DispFlag"];
            receiptPlanCondition.IsShopDeposit = isShopDeposit;

            return receiptPlanCondition;
        }

        /// <summary>
        /// �����������̓`�F�b�N
        /// </summary>
        /// <param name="journal">���������f�[�^</param>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>���������f�[�^</returns>
        /// <history>
        /// 2019/02/14 yano #3972 �����o�[���@�o���f�[�^�o�^���ɓ`�[�ԍ��݂̂ŕR�Â��邱�Ƃ��ł��Ȃ��B
        /// 2019/02/07 yano #3966 �X�ܓ�������(�����Ǘ�)�@�������O�̓`�[�C���ɂ������G���[
        /// 2016/05/18 arc yano #3557 ���������o�^  �������=011�i�J�[�h��Ђ���̓����j�Ł@�����z = �����c���łȂ��ꍇ�G���[�Ƃ���
        /// </history>
        private EntitySet<Journal> ValidateJournal(EntitySet<Journal> line, FormCollection form) {

            bool alreadyOutputMsgE0001 = false;
            bool alreadyOutputMsgE0002 = false;
            bool alreadyOutputMsgE0003 = false;
            bool alreadyOutputMsgE0013a = false;
            bool alreadyOutputMsgAmount = false;
            bool alreadyOutputMsgCustomerClaimCode = false;
            bool alreadyOutputMsgE0001OfficeCode = false;            //Add 2019/02/14 yano #3972
            bool alreadyOutputMsgE0001CashAccountCode = false;       //Add 2019/02/14 yano #3972
 
            for (int i = 0; i < line.Count; i++)
            {

                //�`�F�b�N�������Ă�f�[�^�̂݃`�F�b�N�̑Ώۂɂ���
                //Add 2016/01/07 arc nakayama #3303_���������o�^�Ń`�F�b�N���t���Ă��Ȃ����̂���������Ă��܂�
                if (!string.IsNullOrEmpty(form["line[" + i + "].targetFlag"]) && form["line[" + i + "].targetFlag"].Equals("1"))
                {
                    Journal journal = line[i];
                    string prefix = "line[" + CommonUtils.IntToStr(i) + "].";

                    //Add 2019/02/07 yano #3966
                    ReceiptPlan target = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);
                    if (target == null)
                    {
                        ModelState.AddModelError("", "�`�[���X�V���ꂽ���߁A�������s�����Ƃ��ł��܂���B�Č�����ɏ������s���ĉ������B");
                    }

                    /*  //Del 2016/05/17 arc yano #3582
                    if ((!CommonUtils.DefaultString(form[prefix + "JournalDate"]).Equals(string.Format("{0:yyyy/MM/dd}", DateTime.Now.Date))))
                        //|| (!string.IsNullOrEmpty(form[prefix + "AccountType"]))
                        //|| (!CommonUtils.DefaultString(form[prefix + "Amount"]).Equals("0")))     
                    {
                    */

                        // �K�{�`�F�b�N(���l/���t���ڂ͑����`�F�b�N�����˂�)
                        if (!ModelState.IsValidField(prefix + "JournalDate"))
                        {
                            ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0003 ? "" : MessageUtils.GetMessage("E0003", "������")));
                            alreadyOutputMsgE0003 = true;
                            if (ModelState[prefix + "JournalDate"].Errors.Count > 1)
                            {
                                ModelState[prefix + "JournalDate"].Errors.RemoveAt(0);
                            }
                        }
                        if (string.IsNullOrEmpty(journal.AccountType))
                        {
                            ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgE0001 ? "" : MessageUtils.GetMessage("E0001", "�������")));
                            alreadyOutputMsgE0001 = true;
                        }
                        if (!ModelState.IsValidField(prefix + "Amount"))
                        {
                            ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                            alreadyOutputMsgE0002 = true;
                            if (ModelState[prefix + "Amount"].Errors.Count > 1)
                            {
                                ModelState[prefix + "Amount"].Errors.RemoveAt(0);
                            }
                        }

                        //Addd 2019/02/14 yano #3972
                        if (!isShopDeposit)
                        {
                            //-------------------
                            // �������Ə�
                            //-------------------
                            if (string.IsNullOrWhiteSpace(form[prefix + "OfficeCode"]))
                            {
                                ModelState.AddModelError(prefix + "OfficeCode", (alreadyOutputMsgE0001OfficeCode ? "" : MessageUtils.GetMessage("E0001", "�������Ə�")));
                                alreadyOutputMsgE0001OfficeCode = true;
                            }
                            //----------------
                            // ��������
                            //----------------
                            string test = form[prefix + "CashAccountCode"];

                            if (string.IsNullOrWhiteSpace(form[prefix + "CashAccountCode"]))
                            {
                                ModelState.AddModelError(prefix + "CashAccountCode", (alreadyOutputMsgE0001CashAccountCode ? "" : MessageUtils.GetMessage("E0001", "����")));
                                alreadyOutputMsgE0001CashAccountCode = true;
                            }
                        }

                       

                        // �t�H�[�}�b�g�`�F�b�N
                        if (ModelState.IsValidField(prefix + "Amount"))
                        {
                            if (!Regex.IsMatch(journal.Amount.ToString(), @"^[-]?\d{1,10}$"))
                            {
                                ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                                alreadyOutputMsgE0002 = true;
                            }
                        }

                       
                        //Add 2016/05/18 arc yano #3557   
                        //������ʁ��u�J�[�h��Ђ���̓����v�̏ꍇ�̃`�F�b�N����
                        if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
                        {
                            //�����\����擾
                            ReceiptPlan plan = new ReceiptPlanDao(db).GetByStringKey(form[prefix + "ReceiptPlanId"]);

                            //������^�C�v�̃`�F�b�N
                            CustomerClaim rec = new CustomerClaimDao(db).GetByKey(plan.CustomerClaimCode);

                            //������̃^�C�v���u�N���W�b�g�v�̏ꍇ�A�G���[
                            if (rec != null && !string.IsNullOrWhiteSpace(rec.CustomerClaimType) && !rec.CustomerClaimType.Equals("003"))
                            {
                                ModelState.AddModelError(prefix + "AccountType", (alreadyOutputMsgCustomerClaimCode ? "" : "������敪���u�N���W�b�g�v�ȊO�̐�����̏ꍇ�A������ʂ́u�J�[�h��Ђ���̓����v�͑I���ł��܂���"));
                                alreadyOutputMsgCustomerClaimCode = true;
                            }
                                
                            /*---------------------
                             *�����z�̃`�F�b�N
                             ---------------------*/                            
                            //�����z�������c���̏ꍇ�G���[
                            if ((plan.ReceivableBalance ?? 0m) != journal.Amount)
                            {
                                ModelState.AddModelError(prefix + "Amount", ( alreadyOutputMsgAmount ? "" : "�J�[�h��Ђ���̓����̏ꍇ�A�����z�͐����z�S�z��ݒ肷��K�v������܂�"));
                                alreadyOutputMsgAmount = true;
                            }  
                        }
                           

                        // �l�`�F�b�N
                        if (ModelState.IsValidField(prefix + "JournalDate"))
                        {
                            if (journal.JournalDate.CompareTo(DateTime.Now.Date) > 0)
                            {
                                ModelState.AddModelError(prefix + "JournalDate", (alreadyOutputMsgE0013a ? "" : MessageUtils.GetMessage("E0013", new string[] { "������", "�{���ȑO" })));
                                alreadyOutputMsgE0013a = true;
                            }
                            // ���ߏ����ς݂��ǂ����`�F�b�N�i2012.05.10�C���j
                            // �Ō�̌������ߏ��������擾
                            CashBalance cashBalance = GetLatestClosedData(journal.OfficeCode, journal.CashAccountCode);

                            // �`�[���t�����������O����������ߏ����ς݈����i�����j
                            if (journal.AccountType != null && journal.AccountType.Equals("001"))
                            {
                                if (cashBalance != null && journal.JournalDate <= cashBalance.ClosedDate)
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�O��̌������ߓ�����" }));
                                }
                            }
                            // Add 2014/04/17 arc nakayama #3096 ���ߏ�����ł��X�ܓ����������ʂ��Ɖ\�ƂȂ� �S�̂ł̏������ʏ������o�����߂�����悤�ɂ���
                            string departmentCode = "";
                            CarSalesHeader carHeader = new CarSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                            ServiceSalesHeader serviceHeader = new ServiceSalesOrderDao(db).GetBySlipNumber(journal.SlipNumber);
                            // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                            if (carHeader != null)
                            {
                                departmentCode = carHeader.DepartmentCode;
                            }
                            else if (serviceHeader != null)
                            {
                                departmentCode = serviceHeader.DepartmentCode;
                            }

                            //���O�C�����[�U���擾
                            Employee loginUser = ((Employee)Session["Employee"]);
                            ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //�����f�[�^�ҏW���������邩�Ȃ���

                            //�����f�[�^�ҏW����������Ζ{���߂̎��̂݃G���[
                            if (AppRole.EnableFlag)
                            {
                                if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(departmentCode, journal.JournalDate, "001"))
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", (MessageUtils.GetMessage("E0013", new string[] { "������", "�O��̌������ߏ��������s���ꂽ�N������" })));
                                }
                            }
                            else
                            {
                                // �����łȂ���Ή����߈ȏ�ŃG���[
                                if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(departmentCode, journal.JournalDate, "001"))
                                {
                                    ModelState.AddModelError(prefix + "JournalDate", MessageUtils.GetMessage("E0013", new string[] { "������", "�O��̌������ߏ��������s���ꂽ�N������" }));
                                }
                            }
                        }
                        if (ModelState.IsValidField(prefix + "Amount"))
                        {
                            if (journal.Amount.Equals(0m))
                            {
                                ModelState.AddModelError(prefix + "Amount", (alreadyOutputMsgE0002 ? "" : MessageUtils.GetMessage("E0002", new string[] { "���z", "0�ȊO��10���ȓ��̐����̂�" })));
                                alreadyOutputMsgE0002 = true;
                            }
                        }
                    //} //Del 2016/05/17 arc yano #3582
                }
            }

            return line;
        }

        /// <summary>
        /// ���߂̒��ߏ��擾
        /// </summary>
        /// <param name="departmentCode">����R�[�h</param>
        /// <returns>���߂̒��ߏ��</returns>
        private CashBalance GetLatestClosedData(string officeCode, string cashAccountCode) {

            CashBalance receiptPlanCondition = new CashBalance();
            receiptPlanCondition.OfficeCode = officeCode;
            receiptPlanCondition.CashAccountCode = cashAccountCode;
            return new CashBalanceDao(db).GetLatestClosedData(receiptPlanCondition);
        }

        /// <summary>
        /// �����������דo�^����
        /// </summary>
        /// <param name="targetReceiptPlan">�����Ώۓ����\�胂�f���f�[�^</param>
        /// <param name="journal">�o�[�����f���f�[�^</param>
        /// <history>
        /// 2019/02/14 yano #3978 �y�����Ǘ��z�������Ə��A����������ҏW�ł��Ȃ�
        /// 2017/11/14 arc yano #3811 �ԗ��`�[�|����Ԃ̓����\��c���X�V�s����
        /// 2016/05/30 arc yano #3567 �J�[�h��Ђ���̓����\��̏��� �J�[�h��Ђ���̓����\��̏ꍇ�A
        ///                     �������т�CreditReceiptPlanID�ɓ����\���ID��ݒ肷��
        /// </history>
        private void InsertJournal(ReceiptPlan targetReceiptPlan, Journal journal) {

            // JournalDate,AccountType,Amount�̓t���[�����[�N�ɂĕҏW�ς�
            journal.JournalId = Guid.NewGuid();
            journal.JournalType = "001";
            //Mod  2019/02/14 yano #3978
            if (isShopDeposit)
            {
                journal.OfficeCode = targetReceiptPlan.ReceiptDepartment.OfficeCode;
            }
            else
            {
                journal.OfficeCode = journal.OfficeCode;    
            }
            journal.DepartmentCode = targetReceiptPlan.DepartmentCode;
            journal.CustomerClaimCode = targetReceiptPlan.CustomerClaimCode;
            journal.SlipNumber = targetReceiptPlan.SlipNumber;
            journal.AccountCode = targetReceiptPlan.AccountCode;
            journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.CreateDate = DateTime.Now;
            journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            journal.LastUpdateDate = DateTime.Now;
            journal.DelFlag = "0";
            journal.ReceiptPlanFlag = "1";
            //journal.Summary = targetReceiptPlan.Summary;

            journal.TradeVin = targetReceiptPlan.TradeVin;          //Add 2017/11/14 arc yano #3811

            //Add 2016/05/30 arc yano #3567
            //������� =�u�J�[�h��Ђ���̓����v
            if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
            {
                journal.CreditReceiptPlanId = targetReceiptPlan.ReceiptPlanId.ToString().ToUpper(); //�������т�CreditReceiptPlanID�ɓ����\���ID��ݒ肷��
            }
            db.Journal.InsertOnSubmit(journal);

        }

        /// <summary>
        /// �����\��e�[�u���X�V����
        /// </summary>
        /// <param name="targetReceiptPlan">�����Ώۓ����\�胂�f���f�[�^</param>
        /// <param name="journal">�o�[�����f���f�[�^</param>
        /// <history>
        /// 2016/05/30 arc yano #3567 �J�[�h��Ђ���̓����\��̏��� �J�[�h��Ђ���̓����\��̏ꍇ�A
        ///                     �����\���CreditJournalID�ɓ������т�ID��ݒ肷��
        /// </history>
        private void UpdateReceiptPlan(ReceiptPlan targetReceiptPlan, Journal journal) {

            targetReceiptPlan.ReceivableBalance = decimal.Subtract(targetReceiptPlan.ReceivableBalance ?? 0m, journal.Amount);
            if ((targetReceiptPlan.ReceivableBalance ?? 0m).Equals(0m)) {
                targetReceiptPlan.CompleteFlag = "1";
            }
            targetReceiptPlan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            targetReceiptPlan.LastUpdateDate = DateTime.Now;

            // Add 2016/05/30 arc yano #3567
            if (!string.IsNullOrWhiteSpace(journal.AccountType) && journal.AccountType.Equals("011"))
            {
                targetReceiptPlan.CreditJournalId = journal.JournalId.ToString().ToUpper();     //�����\���CreditJournalID�ɓ������т�ID��ݒ肷��
            }

        }
 
        #region Excel�捞����
        /// <summary>
        /// Excel�捞�p�_�C�A���O�\��
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        [AuthFilter]
        public ActionResult ImportDialog(string id)
        {
            List<JournalExcelImport> ImportList = new List<JournalExcelImport>();
            FormCollection form = new FormCollection();

            ViewData["ErrFlag"] = "1";

            return View("ShopDepositImportDialog", ImportList);
        }

        /// <summary>
        /// Excel�ǂݍ���
        /// </summary>
        /// <param name="purchase">Excel�f�[�^</param>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImportDialog(HttpPostedFileBase importFile, FormCollection form)
        {
            List<JournalExcelImport> ImportList = new List<JournalExcelImport>();

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
                        return View("ShopDepositImportDialog", ImportList);
                    }

                    //Excel�ǂݍ���
                    ImportList = ReadExcelData(importFile, ImportList);

                    //�ǂݍ��ݎ��ɉ����G���[������΂����Ń��^�[��
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("ShopDepositImportDialog");
                    }

                    //Excel�œǂݍ��񂾃f�[�^�̃o���f�[�g�`�F�b�N
                    ValidateImportList(ImportList, form);
                    if (!ModelState.IsValid)
                    {
                        SetDialogDataComponent(form);
                        return View("ShopDepositImportDialog");
                    }

                    //DB�o�^
                    DBExecute(ImportList, form);

                    form["ErrFlag"] = "1";
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog");

                //--------------
                //�L�����Z��
                //--------------
                case "2":
                    ImportList = new List<JournalExcelImport>();
                    ViewData["ErrFlag"] = "1";//[��荞��]�{�^���������Ȃ��悤�ɂ��邽��
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog", ImportList);

                //----------------------------------
                //���̑�(�����ɓ��B���邱�Ƃ͂Ȃ�)
                //----------------------------------
                default:
                    SetDialogDataComponent(form);
                    return View("ShopDepositImportDialog");
            }
        }
        #endregion

        #region Excel�f�[�^�擾&�ݒ�
        /// Excel�f�[�^�擾&�ݒ�
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        private List<JournalExcelImport> ReadExcelData(HttpPostedFileBase importFile, List<JournalExcelImport> ImportList)
        {
            //�J�����ԍ��ۑ��p
            int[] colNumber = new int[11] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1};

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
                string[] Result = new string[colNumber.Count()];

                for (datarow = StartRow + 1; datarow < EndRow + 1; datarow++)
                {
                    CarPurchaseExcelImportList data = new CarPurchaseExcelImportList();

                    //�X�V�f�[�^�̎擾
                    for (int col = 1; col <= ws.Dimension.End.Column; col++)
                    {
                        for (int i = 0; i < colNumber.Count(); i++)
                        {
                            if (col == colNumber[i])
                            {
                                Result[i] = !string.IsNullOrWhiteSpace(ws.Cells[datarow, col].Text) ? Strings.StrConv(ws.Cells[datarow, col].Text.Trim(), VbStrConv.Narrow, 0x0411).ToUpper() : "";
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

        #region �e���ڂ̗�ԍ��ݒ�
        /// <summary>
        /// �e���ڂ̗�ԍ��ݒ�
        /// </summary>
        /// <param name="headerRow">�w�b�_�s</param>
        /// <param name="colNumber">��ԍ�</param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
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
                    //�������
                    if (cell.Text.Equals("JournalType"))
                    {
                        colNumber[0] = cnt;
                    }
                    //����R�[�h
                    if (cell.Text.Equals("DepartmentCode"))
                    {
                        colNumber[1] = cnt;
                    }
                    //������R�[�h
                    if (cell.Text.Equals("CustomerClaimCode"))
                    {
                        colNumber[2] = cnt;
                    }
                    //�`�[�ԍ�
                    if (cell.Text.Equals("SlipNumber"))
                    {
                        colNumber[3] = cnt;
                    }
                    //������
                    if (cell.Text.Equals("JournalDate"))
                    {
                        colNumber[4] = cnt;
                    }
                    //�������
                    if (cell.Text.Equals("AccountType"))
                    {
                        colNumber[5] = cnt;
                    }
                    //�ȖڃR�[�h
                    if (cell.Text.Equals("AccountCode"))
                    {
                        colNumber[6] = cnt;
                    }
                    //�����z
                    if (cell.Text.Equals("Amount"))
                    {
                        colNumber[7] = cnt;
                    }
                    //���������t���O
                    if (cell.Text.Equals("ReceiptPlanFlag"))
                    {
                        colNumber[8] = cnt;
                    }
                    //�]�L�t���O
                    if (cell.Text.Equals("TransferFlag"))
                    {
                        colNumber[9] = cnt;
                    }
                    //���Ə��R�[�h
                    if (cell.Text.Equals("OfficeCode"))
                    {
                        colNumber[10] = cnt;
                    }
                    /*
                    //���������R�[�h
                    if (cell.Text.Equals("CashAccountCode"))
                    {
                        colNumber[11] = cnt;
                    }
                    */

                    cnt++;
                }
            }
            return colNumber;
        }
    #endregion

        #region Excel�ǂݎ��O�̃`�F�b�N
        /// <summary>
        /// Excel�ǂݎ��O�̃`�F�b�N
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
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

            if (string.IsNullOrWhiteSpace(form["Summary"]))
            {
                ModelState.AddModelError("Summary", "�R�����g�͓��͕K�{�ł�");
            }

            return;
        }
        #endregion

        #region �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// <summary>
        /// �ǂݍ��݌��ʂ̃o���f�[�V�����`�F�b�N
        /// </summary>
        /// <param name="importFile">Excel�f�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns>�Ȃ�</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        public void ValidateImportList(List<JournalExcelImport> ImportList, FormCollection form)
        {
            //ReceiptPlan rpcondition = new ReceiptPlan();

            //�����\�胊�X�g���擾����
            //List<ReceiptPlan> receiptPlanList = new ReceiptPlanDao(db).GetListByCondition(rpcondition);

            //Journal jcondition = new Journal();

            //List<Journal> journalList = new JournalDao(db).GetListByCondition(jcondition);


            DateTime dtRet;

            string svDepartmentCode = "";
            string officeCode = "";
            string cashAccountCode = "";
            string accountType = "";
            string slipNumber = "";
            string customerClaimCode = "";
            string amount = "";

            //�P�s���`�F�b�N
            for (int i = 0; i < ImportList.Count; i++)
            {
                //������
                svDepartmentCode = "";
                officeCode = "";
                cashAccountCode = "";
                accountType = "";
                slipNumber = "";
                customerClaimCode = "";
                amount = "";

                //----------------
                //������R�[�h
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].CustomerClaimCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̐�����R�[�h�����͂���Ă��܂���B������R�[�h"));
                }
                else //�}�X�^�`�F�b�N
                {
                    CustomerClaim rec = new CustomerClaimDao(db).GetByKey(ImportList[i].CustomerClaimCode);

                    if (rec == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s�ڂ̐�����R�[�h�̓}�X�^�ɓo�^����Ă��܂���");
                    }
                    else
                    {
                        //������R�[�h��ޔ����Ă���
                        customerClaimCode = ImportList[i].CustomerClaimCode;
                    }
                }
                //----------------
                //�`�[�ԍ�
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].SlipNumber))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s�ڂ̓`�[�ԍ������͂���Ă��܂���B�`�[�ԍ�"));
                }
                else //�}�X�^�`�F�b�N
                {
                    ServiceSalesHeader sv = new ServiceSalesOrderDao(db).GetBySlipNumber(ImportList[i].SlipNumber);

                    if (sv == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s�ڂ̓`�[�ԍ��͑��݂��܂���");
                    }
                    else
                    {
                        //�T�[�r�X�`�[�̓`�[�ԍ��A����R�[�h��ޔ����Ă���
                        svDepartmentCode = sv.DepartmentCode;
                        slipNumber = ImportList[i].SlipNumber;
                    }
                }

                //----------------
                //�������
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].JournalType) || (!ImportList[i].JournalType.Equals("001") && !ImportList[i].JournalType.Equals("002")))
                {
                    ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓�����ʂɂ́u001�v�܂��́u002�v����͂��Ă�������");
                }
                //----------------
                //����R�[�h
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].DepartmentCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̕���R�[�h�����͂���Ă��܂���B����R�[�h"));
                }
                else //�}�X�^�`�F�b�N
                {
                    Department dep = new DepartmentDao(db).GetByKey(ImportList[i].DepartmentCode);

                    if (dep == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̕���R�[�h�̓}�X�^�ɓo�^����Ă��܂���");
                    }
                }
                //----------------
                //�������
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].AccountType))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̌�����ʂ����͂���Ă��܂���B�������"));
                }
                else //�}�X�^�`�F�b�N
                {
                    c_AccountType rec = new CodeDao(db).GetAccountTypeByKey(ImportList[i].AccountType, false);

                    if (rec == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̌�����ʂ̓}�X�^�ɓo�^����Ă��܂���");
                    }
                    else
                    {
                        accountType = ImportList[i].AccountType;
                    }
                }
                //----------------
                //�ȖڃR�[�h
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].AccountCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̉ȖڃR�[�h�����͂���Ă��܂���B�ȖڃR�[�h"));
                }
                else //�}�X�^�`�F�b�N
                {
                    Account account = new AccountDao(db).GetByKey(ImportList[i].AccountCode);

                    if (account == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̉ȖڃR�[�h�̓}�X�^�ɓo�^����Ă��܂���");
                    }
                }
                //----------------
                //�����z
                //----------------
                if (!Regex.IsMatch(ImportList[i].Amount, @"^\d{1,10}$"))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0004", new string[] { i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓����z", "���̐���10���ȓ�" }));
                }
                else
                {
                    amount = ImportList[i].Amount;
                }
                //----------------
                //���������t���O
                //----------------
                if (string.IsNullOrWhiteSpace(ImportList[i].ReceiptPlanFlag) || !ImportList[i].ReceiptPlanFlag.Equals("1"))
                {
                    ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓��������t���O�́u1�v����͂��ĉ�����");
                }
                //----------------
                //�]�L�t���O
                //----------------
                if (!string.IsNullOrWhiteSpace(ImportList[i].TransferFlag) && !ImportList[i].TransferFlag.Equals("0") && !ImportList[i].TransferFlag.Equals("1"))
                {
                    ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓]�L�t���O�́u0�v�܂��́u1�v����͂��ĉ�����");
                }
                //----------------
                //���Ə��R�[�h
                //----------------
                if (string.IsNullOrEmpty(ImportList[i].OfficeCode))
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0001", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̎��Ə��R�[�h�����͂���Ă��܂���B���Ə��R�[�h"));
                }
                else //�}�X�^�`�F�b�N
                {
                    Office office = new OfficeDao(db).GetByKey(ImportList[i].OfficeCode);

                    if (office == null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̎��Ə��R�[�h�̓}�X�^�ɓo�^����Ă��܂���");
                    }
                    else
                    {
                        officeCode = ImportList[i].OfficeCode;
                    }
                }
                /*
                //----------------
                //���������R�[�h
                //----------------
                if(!string.IsNullOrEmpty(ImportList[i].CashAccountCode)) //�}�X�^�`�F�b�N
                {
                    //���Ə��R�[�h���}�X�^�o�^����Ă���ꍇ
                    if (!string.IsNullOrWhiteSpace(officeCode))
                    {
                        CashAccount cashAccount = new CashAccountDao(db).GetByKey(officeCode, ImportList[i].CashAccountCode);

                        if (cashAccount == null)
                        {
                            ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̌��������R�[�h�̓}�X�^�ɓo�^����Ă��܂���");
                        }
                        else
                        {
                            cashAccountCode = ImportList[i].CashAccountCode;
                        }
                    }
                }
                */
                //----------------
                //������
                //----------------
                if (!DateTime.TryParse(ImportList[i].JournalDate, out dtRet)) //�t�H�[�}�b�g�`�F�b�N
                {
                    ModelState.AddModelError("", MessageUtils.GetMessage("E0003", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓����������t�̌`���ł͂���܂���B������"));
                }
                else //���`�F�b�N
                {
                    if (!string.IsNullOrWhiteSpace(svDepartmentCode))
                    {
                        //--------------------
                        //��������
                        //--------------------
                        //���͂��ꂽ���Ə��R�[�h���}�X�^�o�^����Ă���ꍇ
                        if (!string.IsNullOrWhiteSpace(officeCode) && !string.IsNullOrWhiteSpace(cashAccountCode))
                        {
                            // �Ō�̌������ߏ��������擾
                            CashBalance cashBalance = GetLatestClosedData(officeCode, cashAccountCode);

                            // �`�[���t�����������O����������ߏ����ς݈����i�����j
                            if (!string.IsNullOrWhiteSpace(accountType) && accountType.Equals("001"))
                            {
                                if (cashBalance != null && dtRet <= cashBalance.ClosedDate)
                                {
                                    ModelState.AddModelError("", MessageUtils.GetMessage("E0013", new string[] { i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̂̓�����", "�O��̌������ߓ�����" }));
                                }
                            }
                        
                        }

                        //���O�C�����[�U���擾
                        Employee loginUser = ((Employee)Session["Employee"]);
                        ApplicationRole AppRole = new ApplicationRoleDao(db).GetByKey(loginUser.SecurityRoleCode, "EditTempClosedData"); //�����f�[�^�ҏW���������邩�Ȃ���

                        //�����f�[�^�ҏW����������Ζ{���߂̎��̂݃G���[
                        if (AppRole.EnableFlag)
                        {
                            if (!new InventoryScheduleDao(db).IsCloseEndInventoryMonth(svDepartmentCode, dtRet, "001"))
                            {
                                ModelState.AddModelError("", (MessageUtils.GetMessage("E0013", new string[] { i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓�����", "�O��̌������ߏ��������s���ꂽ�N������" })));
                            }
                        }
                        else
                        {
                            // �����łȂ���Ή����߈ȏ�ŃG���[
                            if (!new InventoryScheduleDao(db).IsClosedInventoryMonth(svDepartmentCode, dtRet, "001"))
                            {
                                ModelState.AddModelError("", (MessageUtils.GetMessage("E0013", new string[] { i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓�����", "�O��̌������ߏ��������s���ꂽ�N������" })));
                            }
                        }
                    }
                }

                //-----------------
                //�����\��`�F�b�N
                //-----------------
                if (!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerClaimCode))
                {
                    List<ReceiptPlan> rpList = new ReceiptPlanDao(db).GetListByCustomerClaim(slipNumber, customerClaimCode);
                    //List<ReceiptPlan> rpList = receiptPlanList.Where(x => !string.IsNullOrWhiteSpace(x.SlipNumber) && x.SlipNumber.Equals(slipNumber) && !string.IsNullOrWhiteSpace(x.CustomerClaimCode) && Strings.StrConv(x.CustomerClaimCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(customerClaimCode)).ToList();

                    if (rpList == null || rpList.Count == 0)
                    {
                        ModelState.AddModelError("", i + 1 + "�s�ڂ̓`�[�ԍ�(" + slipNumber + ")�A������(" + customerClaimCode + ")�ɑ΂��鐿�������݂��Ȃ����߁A�����ł��܂���");
                    }
                    else if (rpList.Count > 1)
                    {
                        ModelState.AddModelError("", i + 1 + "�s�ڂ̓`�[�ԍ�(" + slipNumber + ")�A������(" + customerClaimCode + ")�ɑ΂��鐿�����������݂��Ă��邽�߁A�����ł��܂���");
                    }
                    else
                    {
                        //�����\��̐����z�Ɠ����z���قȂ��Ă���ꍇ�̓G���[
                        if (!string.IsNullOrWhiteSpace(amount) && rpList[0].Amount != decimal.Parse(amount))
                        {
                            ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̋��z�������z�ƈ�v���܂���");
                        }
                    }
                }

                //------------------
                //�������у`�F�b�N
                //------------------
                if(!string.IsNullOrWhiteSpace(slipNumber) && !string.IsNullOrWhiteSpace(customerClaimCode))
                {
                    Journal journal = new JournalDao(db).GetListByCustomerAndSlip(slipNumber, customerClaimCode).FirstOrDefault();

                    //Journal journal = journalList.Where(x => !string.IsNullOrWhiteSpace(x.SlipNumber) && x.SlipNumber.Equals(slipNumber) && !string.IsNullOrWhiteSpace(x.CustomerClaimCode) && Strings.StrConv(x.CustomerClaimCode, VbStrConv.Narrow, 0x0411).ToUpper().Equals(customerClaimCode)).FirstOrDefault();

                    //���Ɏ��т����݂��Ă���ꍇ
                    if (journal != null)
                    {
                        ModelState.AddModelError("", i + 1 + "�s��(�`�[�ԍ��F(" + slipNumber + "�A������F" + customerClaimCode + ")�̓������т͊��ɓo�^����Ă��܂�");
                    }
                }

            }

            //-----------------
            //�d���`�F�b�N
            //-----------------
            var ret = ImportList.GroupBy(x => new { SlipNumber = x.SlipNumber, CustomerClaimeCode = x.CustomerClaimCode }).Select(c => new { SlipNumber = c.Key.SlipNumber, CustomerClaimCode = c.Key.CustomerClaimeCode,  Count = c.Count() }).Where(c => c.Count > 1);

            foreach (var a in ret)
            {
                if (!string.IsNullOrWhiteSpace(a.SlipNumber) && !string.IsNullOrWhiteSpace(a.CustomerClaimCode))
                {
                    ModelState.AddModelError("", "�捞�ރt�@�C���̒��ɓ���̓`�[�ԍ��A������(�`�[�ԍ��F" + a.SlipNumber + "�A������F" + a.CustomerClaimCode + ")��������`����Ă��܂�");
                }
            }
        }
        #endregion

        #region Excel�̓ǂݎ�茋�ʂ����X�g�ɐݒ肷��
        /// <summary>
        /// ���ʂ����X�g�ɐݒ肷��
        /// </summary>
        /// <param name="Result">Excel�Z���̒l</param>
        /// <param name="ImportList"></param>
        /// <returns></returns>
        /// <history>
        ///  2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        public List<JournalExcelImport> SetProperty(ref string[] Result, ref List<JournalExcelImport> ImportList)
        {
            JournalExcelImport SetLine = new JournalExcelImport();

            //������� 
            SetLine.JournalType = Result[0];
            //����R�[�h
            SetLine.DepartmentCode = Result[1];
            //������R�[�h
            SetLine.CustomerClaimCode = Result[2];
            //�`�[�ԍ�
            SetLine.SlipNumber = Result[3];
            //������
            SetLine.JournalDate = Result[4];
            //�������
            SetLine.AccountType = Result[5];
            //�����R�[�h
            SetLine.AccountCode = Result[6];
            //�����z
            SetLine.Amount = Result[7];
            //���������t���O
            SetLine.ReceiptPlanFlag = Result[8];
            //�]�L�t���O
            SetLine.TransferFlag = Result[9];
            //���Ə��R�[�h
            SetLine.OfficeCode = Result[10];

            //���������R�[�h
            //SetLine.CashAccountCode = Result[11];
           
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
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        private void SetDialogDataComponent(FormCollection form)
        {
            ViewData["ErrFlag"] = form["ErrFlag"];
            ViewData["RequestFlag"] = form["RequestFlag"];
            ViewData["Summary"] = form["Summary"];
        }
        #endregion

        #region �ǂݍ��񂾃f�[�^��DB�ɓo�^
        /// <summary>
        /// DB�X�V
        /// </summary>
        /// <returns>�߂�l(0:���� 1:�G���[(�������e�B�ꊇ�捞��ʂ֑J��) -1:�G���[(�G���[��ʂ֑J��))</returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel�捞(�������e�B����)
        /// </history>
        private void DBExecute(List<JournalExcelImport> ImportList, FormCollection form)
        {
            using (TransactionScope ts = new TransactionScope())
            {
                List<CashAccount> caList = new CashAccountDao(db).GetListByOfficeCode("");

                List<Journal> workList = new List<Journal>();

                //-------------------------------
                //�������уf�[�^�̍쐬
                //-------------------------------
                foreach (var LineData in ImportList)
                {
                    Journal journal = new Journal();

                    CashAccount rec = caList.Where(x => x.OfficeCode.Equals(LineData.OfficeCode) && x.CashAccountName.Trim().Equals("���޽����")).FirstOrDefault();

                    string cashAccountCode = rec != null ? rec.CashAccountCode : "";

                    journal.JournalId = Guid.NewGuid();                                                         //��������ID
                    journal.JournalType = LineData.JournalType;                                                 //�������
                    journal.DepartmentCode = LineData.DepartmentCode;                                           //����R�[�h
                    journal.CustomerClaimCode = LineData.CustomerClaimCode;                                     //������R�[�h
                    journal.SlipNumber = LineData.SlipNumber;                                                   //�`�[�ԍ�
                    journal.JournalDate = DateTime.Parse(LineData.JournalDate);                                 //������
                    journal.AccountType = LineData.AccountType;                                                 //�������
                    journal.AccountCode = LineData.AccountCode;                                                 //�ȖڃR�[�h
                    journal.Amount = decimal.Parse(LineData.Amount);                                            //���z
                    journal.Summary = form["Summary"];                                                          //�E�v
                    journal.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;                  //�쐬��
                    journal.CreateDate = DateTime.Now;                                                          //�쐬��
                    journal.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;              //�ŏI�X�V��
                    journal.LastUpdateDate = DateTime.Now;                                                      //�ŏI�X�V��
                    journal.DelFlag = "0";                                                                      //�폜�t���O�@���u0�v�Œ�
                    journal.ReceiptPlanFlag = LineData.ReceiptPlanFlag;                                         //���������t���O
                    journal.TransferFlag = LineData.TransferFlag;                                               //�]�L�t���O
                    journal.OfficeCode = LineData.OfficeCode;                                                   //���Ə��R�[�h
                    journal.CashAccountCode = cashAccountCode;                                                  //���������R�[�h
                    journal.PaymentKindCode = "";                                                               //�x����ʃR�[�h�@���u�󕶎��v�Œ�
                    journal.CreditReceiptPlanId = "";                                                           //�N���W�b�g�^�C�v�̓����\��ID�@���u�󕶎��v�Œ�
                    journal.TradeVin = "";                                                                      //����ԑ�ԍ��@���u�󕶎��v�Œ�

                    workList.Add(journal);
                }

                db.Journal.InsertAllOnSubmit(workList);

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
                    OutputLogger.NLogFatal(se, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                }
                catch (Exception e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(e, PROC_NAME_EXCELUPLOAD, FORM_NAME, "");
                    ts.Dispose();
                }
            }
        }
        #endregion

        #region Ajax
        public ActionResult GetReceiptBalance(string slipNumber, string customerClaimCode) {
            if (Request.IsAjaxRequest()) {
                decimal balance = new ReceiptPlanDao(db).GetCashAmountByCustomerClaim(slipNumber, customerClaimCode);
                return Json(balance);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// ������^�C�v���X�g�̎擾
        /// </summary>
        /// <param name="customerClaimFilter">�������ʃt�B���^</param>
        /// <returns>���X�g</returns>
        public ActionResult CustomerClaimTypeByCustomerClaimFilter(string customerClaimFilter)
        {
            if (Request.IsAjaxRequest())
            {
                List<c_CustomerClaimType> list = new CodeDao(db).GetCustomerClaimTypeAll(false, customerClaimFilter);

                CodeDataList clist = new CodeDataList();
                clist.DataList = new List<CodeData>();

                foreach (var l in list)
                {
                    CodeData data = new CodeData();

                    data.Code = l.Code;
                    data.Name = l.Name;

                    clist.DataList.Add(data);
                }

                return Json(clist);
            }
            return new EmptyResult();
        }
        #endregion


        #region �����\��z�̏��Ȃ����ɓ������s��
        /// <summary>
        /// �����\��z�̏��Ȃ����ɓ������s���i�����\�胊�X�g, �����z�j
        /// </summary>
        /// <param name="ReceiptPlanList">�����\�胊�X�g</param>
        /// <param name="JournalAmount">�����z</param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/12 arc yano #3818 ���т��v���X�̏ꍇ�̓v���X�̗\����A�}�C�i�X�̏ꍇ�̓}�C�i�X�̗\�����������
        /// 2016/06/29 arc nakayama #3593_�`�[�ɑ΂��ē��ꐿ���悪�����������ꍇ�̍l��
        /// </history>
        private void Payment(List<ReceiptPlan> ReceiptPlanList, decimal JournalAmount)
        {
            decimal remainAmount = JournalAmount; //�����z

            List<ReceiptPlan> targetList = new List<ReceiptPlan>();

            if (JournalAmount > 0)
            {
                targetList = ReceiptPlanList.Where(x => x.Amount > 0).ToList();
            }
            else
            {
                targetList = ReceiptPlanList.Where(x => x.Amount <= 0).ToList();
            }

            //������0�̏ꍇ�A�����\�胊�X�g�����̂܂܃Z�b�g
            if (targetList.Count == 0)
            {
                targetList = ReceiptPlanList;
            }

            foreach (var plan in targetList)
            {
                if (plan.CompleteFlag == "0")
                {
                    //�c�� - �����z = 0 �Ȃ炻�̂܂܌��Z����
                    if (decimal.Subtract(plan.ReceivableBalance ?? 0m ,remainAmount).Equals(0m))
                    {
                        plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                        remainAmount = 0m;
                    }
                    else
                    {
                        //�����łȂ���΁A�c�����S�Ĉ������߁A�����z = �����z - �c���@�����Ďc�����S�Č��Z�������Ƃɂ���
                        remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);

                        //�c�����v���X�̒l�̎���
                        if (plan.ReceivableBalance > 0)
                        {
                            //�����z - �c���@= �}�C�i�X�̒l�ɂȂ��Ă�������������Ă���̂Ł�
                            if (remainAmount < 0)
                            {
                                //�����z = �����z + �c�� �����Ďc��̓����z�����߂遫
                                remainAmount = remainAmount + plan.ReceivableBalance ?? 0m;
                                //�c�����狁�߂������z�����Z����
                                plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                                remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);
                            }
                            else
                            {
                                //�����łȂ���΁A�܂������z���c���Ă��邪�Y���̓����\��̎c�������ς��܂Ō��Z�������ƂɂȂ�̂Ŏc����0�ɍX�V����
                                plan.ReceivableBalance = 0;
                            }
                        }
                        else //�c�����}�C�i�X�̒l�̎���
                        {
                            //�����z - �c���@= �v���X�̒l�ɂȂ��Ă�������������Ă���̂Ł�
                            if (remainAmount > 0)
                            {
                                //�����z = �����z + �c�� �����Ďc��̓����z�����߂遫
                                remainAmount = remainAmount + plan.ReceivableBalance ?? 0m;
                                //�c�����狁�߂������z�����Z����
                                plan.ReceivableBalance = decimal.Subtract(plan.ReceivableBalance ?? 0m, remainAmount);
                            }
                            else
                            {
                                //�����łȂ���΁A�܂������z���c���Ă��邪�Y���̓����\��̎c�������ς��܂Ō��Z�������ƂɂȂ�̂Ŏc����0�ɍX�V����
                                plan.ReceivableBalance = 0;
                                remainAmount = decimal.Subtract(remainAmount, plan.ReceivableBalance ?? 0m);
                            }
                        }
                    }


                    //�Ō�Ɏc�����O�ɂȂ��Ă�������������t���O�𗧂Ă�
                    if (plan.ReceivableBalance.Equals(0m))
                    {
                        plan.CompleteFlag = "1";
                    }
                    plan.LastUpdateDate = DateTime.Now; //�ŏI�X�V��
                    plan.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;�@//�ŏI�X�V��
                }
                //�����z���O�ɂȂ��Ă�����A������
                if (remainAmount.Equals(0m))
                {
                    break;
                }
            }

        }
        #endregion
    }
}
