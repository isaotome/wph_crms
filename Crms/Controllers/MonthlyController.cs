using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

//Add 2014/08/28 arc yano ���O�o�ׂ͂̈ɒǉ�
using System.Reflection;
using System.Data.Linq;
using System.Data.SqlClient;
using Crms.Models;

//Add 2014/09/02 arc yano IPO�Ή����̂Q
using System.Transactions;


namespace Crms.Controllers
{
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class MonthlyController : Controller
    {

        //Add 2014/08/28 arc yano �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�������ߏ�����";             // ��ʖ�
        private static readonly string PROC_NAME = "�������ߏ����󋵍쐬�E�X�V";   // ������
        
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        public CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public MonthlyController() {
            db = new CrmsLinqDataContext();
        }

        // Add 2015/03/19 arc nakayama �`�[�C���Ή�
        /// <summary>
        /// �T�[�r�X�`�[�����T�[�r�X
        /// </summary>
        private IServiceSalesOrderService service;

        /// <summary>
        /// �����̃X�e�[�^�X�ꗗ��\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            form["TargetYear"] = DateTime.Today.Year.ToString().Substring(1,3);
            form["TargetMonth"] = DateTime.Today.Month.ToString().PadLeft(3, '0');
            form["TargetRange"] = "0";
            form["RequestFlag"] = "0";

            return Criteria(form);
        }

        /// <summary>
        /// �w�茎�̃X�e�[�^�X�ꗗ��\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2018/03/26 arc yano #3855 �����o�[�� �����̌������ߑΏۃt���O�̒ǉ�
        /// 2017/05/10 arc yano #3762 �ԗ��I���@�\�ǉ��@�ԗ��I���󋵂�\��
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �I���̊Ǘ��𕔖�P�ʂ���q�ɒP�ʂɕύX
        /// </history>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            CodeDao dao = new CodeDao(db);
            DateTime targetMonth = DateTime.Parse(dao.GetYear(true, form["TargetYear"]).Name + "/" + dao.GetMonth(true, form["TargetMonth"]).Name);
            List<MonthlyStatus> list = new List<MonthlyStatus>();
            List<Department> departmentList = new DepartmentDao(db).GetListAll();

            //Mod 2014/08/27 IPO�Ή����̂Q �����ߏ����ǉ�
            //���ߏ������s
            if (form["RequestFlag"].Equals("1"))
            {
                int ret = ExecClose(form);

                if (ret == -1)// �쐬�E�X�V�G���[�̏ꍇ
                {
                    return View("Error");
                }
                else if (ret == 1) //�쐬�E�X�V�����𖞂����Ă��Ȃ��ꍇ
                {
                    //---------------------------------------
                    //�G���e�B�e�B�̃��[���o�b�N
                    //---------------------------------------
                    //InventorySchedule
                    List<InventorySchedule> listInventorySchedule = new InventoryScheduleDao(db).GetListByKey(new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");
                    //�X�V�����G���e�B�e�B�����ɖ߂�
                    db.Refresh(RefreshMode.OverwriteCurrentValues, listInventorySchedule);
                }
            }
            
            foreach (Department d in departmentList)
            {
                //Add 2016/08/13 arc yano #3596 
                //���傩��g�p�q�ɂ�����o��
                DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, d.DepartmentCode);
                
                //�q�ɃR�[�h�̐ݒ�
                string warehouseCode = "";
                if (dWarehouse != null)
                {
                    warehouseCode = dWarehouse.WarehouseCode;
                }
                
                MonthlyStatus status = new MonthlyStatus();

                //Mod 2018/03/26 arc yano #3855
                List<CashAccount> accountList = (new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode) != null ? new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode).Where(x => x.NonCloseTarget == null || !x.NonCloseTarget.Equals("1")).ToList() : new List<CashAccount>());
                //List<CashAccount> accountList = new CashAccountDao(db).GetListByOfficeCode(d.OfficeCode);
                foreach (var a in accountList)
                {
                    //Mod 2014/09/25 arc yano #3095 �����o�ݍ��f�[�^�擾���@�̕ύX �w�茎�����_�̌����ݍ����擾
                    CashBalance balance = new CashBalanceDao(db).GetLastMonthClosedData2(d.OfficeCode, a.CashAccountCode, targetMonth);
                    //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                    if (balance == null || !CommonUtils.DefaultString(balance.CloseFlag).Equals("1"))
                    {
                        status.CashBalance = null;
                        break;
                    }
                    status.CashBalance = balance;
                }
                status.InventorySchedule = new InventoryScheduleDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");
                // Mod 2015/02/03 arc nakayama ���i�I�������ԗ��ƕ�����Ή�(InventorySchedule �� InventoryScheduleParts)
                //status.InventoryPartsSchedule = new InventorySchedulePartsDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "002");
                status.InventoryPartsSchedule = new InventorySchedulePartsDao(db).GetByKey(warehouseCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "002");  //Mod 2016/08/13 arc yano #3596 

                status.InventoryCarSchedule = new InventoryScheduleCarDao(db).GetByKey(warehouseCode, new DateTime(targetMonth.Year, targetMonth.Month, 1));  //Add 2017/05/10 arc yano #3762
                
                status.Department = d;
                list.Add(status);                
            }
            SetDataComponent(form);
           
            return View("MonthlyCriteria", list);
        }

        //Add 2014/08/27 arc yano IPO�Ή����̂Q �����ߏ����ǉ�
        /// <summary>
        /// ���ߏ������s
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/28 yano #3922 �ԗ��Ǘ��\(�^�}�\) �@�\���P�A�@�{���ߎ��̎ԗ��Ǘ��X�i�b�v�V���b�g�̕ۑ������̔p�~
        /// 2016/11/30 #3659 �ԗ��Ǘ����ڒǉ� �{���ߎ��s���Ɏԗ��Ǘ��f�[�^�̃X�i�b�v�V���b�g��ۑ�����
        /// 2015/03/23 arc nakayama �`�[�C���Ή��@�{���߂��s������`�[�̏C�������폜����(�����ȑO�̔[�ԓ��̓`�[���ׂ�)
        /// </history>
        //[AcceptVerbs(HttpVerbs.Post)]
        private int ExecClose(FormCollection form)
        {
            int chkret = 0;
            int upsret = 0;
            int ret = 0;

            CodeDao dao = new CodeDao(db);
            DateTime targetMonth = new DateTime(int.Parse(dao.GetYear(true, form["TargetYear"]).Name), int.Parse(dao.GetMonth(true, form["TargetMonth"]).Name), 1);
            
            string closeType = form["CloseType"];               //���ߏ����^�C�v
            string targetClass = form["TargetRange"];           //���ߑΏ۔͈�(�S�́^���_��)
            string departmentCode = null;

            if (targetClass.Equals("1"))
            {
                if (!string.IsNullOrEmpty(form["DepartmentCode"]))
                {
                    departmentCode = form["DepartmentCode"];        //����R�[�h
                }
            }

            if (MvcApplication.CMOperateFlag == 0) //�f�[�^�쐬���Ŗ����ꍇ
            {
                lock (typeof(MvcApplication))
                {
                    //-----------------------------
                    //���b�N����
                    //-----------------------------
                    MvcApplication.CMOperateFlag = 1;
                    MvcApplication.CMOperateUser = new EmployeeDao(db).GetByKey(((Employee)Session["Employee"]).EmployeeCode).EmployeeName;

                    //-----------------------------
                    //�{����
                    //-----------------------------
                    //�g�����U�N�V��������
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //--------------------------------------------
                        // InventorySchedule�쐬�E�X�V
                        //--------------------------------------------
                        //CloseMonthControl�̒��ߏ����󋵂��`�F�b�N
                        chkret = JusdgeCloseProcess(targetMonth,  departmentCode, closeType);

                        //���ߏ������s�\�ȏꍇ
                        if (chkret == 0)
                        {
                            //�쐬�E�X�V����(IventorySchedule)
                            upsret = UpsertInventorySchedule(targetMonth, departmentCode, closeType);

                            //�G���[������
                            if (upsret != 0)
                            {
                                ret = upsret;
                            }
                            else
                            {
                                // �g�����U�N�V�����̃R�~�b�g        
                                ts.Complete();
                            }
                        }
                        else
                        {
                            ret = chkret;
                        }
						//--------------------------------------------
                        // CloseMonthControl�쐬�E�X�V
                        //--------------------------------------------
						//SQL�g���K�ōs�����߁A�����Ȃ�
                    }   

                    //-----------------------------
                    //���b�N����
                    //-----------------------------
                    MvcApplication.CMOperateUser = "";
                    MvcApplication.CMOperateFlag = 0;
                }
            }
            else    //�����[�U���쐬���̏ꍇ
            {
                //���쒆�t���OON
                form["hdCMOperateFlag"] = MvcApplication.CMOperateFlag.ToString();
                form["hdCMOperateUser"] = MvcApplication.CMOperateUser;
            }
            
            //Add 2015/03/23 arc nakayama �`�[�C���Ή�
            service = new ServiceSalesOrderService(db);
            if (closeType == "003"){
                // �C�����̓`�[�����폜����
                service.CloseEnd(targetMonth, departmentCode);

                //Del 2018/08/28 yano #3922
                //Add 2016/11/30 #3659
                //���t�̐ݒ�
                //DateTime[] dateRange = new DateTime[2];
                //dateRange[0] = targetMonth;                             //����
                //dateRange[1] = targetMonth.AddMonths(1).AddDays(-1);    //����

                //List<CarStock> retlist = new CarStockDao(db).MakeCarStockData(string.Format("{0:yyyyMMdd}", dateRange[0]), ((Employee)Session["Employee"]).EmployeeCode, dateRange, true);
            }

            return ret;
        }


        //Add 2014/08/27 arc yano IPO�Ή����̂Q
        /// <summary>
        /// �萔��`
        /// </summary>
        //�������
        private const string TYPE_CLOSE_CANCEL = "001";   //�����߉���    
        private const string TYPE_CLOSE_START = "002";    //������
        private const string TYPE_CLOSE_END = "003";      //�{����

        //ScheduleStatus
        private const string STS_CLOSE_CANCEL = "001";      //�����߉���    
        private const string STS_CLOSE_START = "002";       //������
        private const string STS_CLOSE_END = "003";         //�{����
        private const string STS_CLOSE_START_ON = "004";    //�����ߒ�
        

        /// <summar
        /// �I���X�P�W���[���f�[�^�쐬�E�X�V(��/����)
        /// </summary>
        /// <param name="targetMonth">�I����</param>
        /// <param name="departmentcode">����R�[�h</param>
        /// <param name="closeType">���ߏ����^�C�v</param>
        /// <returns></returns>
        private int UpsertInventorySchedule(DateTime targetMonth, string departmentcode, string closeType)
        {

            List<Department> departmentList = null;

            if (string.IsNullOrEmpty(departmentcode))
            {
                //�S��
                
                //Mod 2014/09/19 arc yano ���ߏ����Ώە���́Adepartment�ɓo�^����Ă��镔��S��(�A���ADelFlag��1�̂��̂͏���)�ɕύX
                //departmentList = new DepartmentDao(db).GetListAllCloseMonthFlag();
                departmentList = new DepartmentDao(db).GetListAll();
            }
            else
            {
                //��
                departmentList = new DepartmentDao(db).GetListByKey(departmentcode);
            }
            
            
            foreach (Department d in departmentList)
            {
                //�Ώە���A�Ώی��̃f�[�^���擾����B
                //�Ώی��̃��R�[�h
                //Mod 2014/11/05 arc nakayama �I����ʂ��u�ԗ��v�݂̂ɕύX
                InventorySchedule istargetmonth = new InventoryScheduleDao(db).GetByKey(d.DepartmentCode, new DateTime(targetMonth.Year, targetMonth.Month, 1), "001");

                
                if (istargetmonth == null)
                {
                    //--------------------------
                    //�f�[�^�ǉ�
                    //--------------------------

                    istargetmonth = new InventorySchedule();   //�V�K�쐬

                    //����
                    istargetmonth.DepartmentCode = d.DepartmentCode;
                    //�I����
                    istargetmonth.InventoryMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
                    //�I�����
                    istargetmonth.InventoryType = "001";
                    //�I���X�e�[�^�X
                    istargetmonth.InventoryStatus = closeType;
                    //�I���J�n��
                    istargetmonth.StartDate = System.DateTime.Now;
                    //�I���I����
                    istargetmonth.EndDate = System.DateTime.Now;
                    //�쐬��
                    istargetmonth.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //�쐬����
                    istargetmonth.CreateDate = System.DateTime.Now;
                    //�ŏI�X�V��
                    istargetmonth.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //�ŏI�X�V����
                    istargetmonth.LastUpdateDate = System.DateTime.Now;
                    //�폜�t���O
                    istargetmonth.DelFlag = "0";

                    db.InventorySchedule.InsertOnSubmit(istargetmonth);
                }
                else
                {
                    //---------------------------
                    //�X�V
                    //---------------------------
                    //UpdateModel(istargetmonth);

                    //�I���X�e�[�^�X
                    istargetmonth.InventoryStatus = closeType;

                    //�I���I����
                    istargetmonth.EndDate = System.DateTime.Now;

                    //�ŏI�X�V��
                    istargetmonth.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    //�ŏI�X�V����
                    istargetmonth.LastUpdateDate = System.DateTime.Now;
                }
                
            }

           //Db�T�u�~�b�g����
           return DbSubmit();
            
        }

        //Add 2014/08/27 arc yano IPO�Ή����̂Q
        /// <summary>
        /// �I���X�P�W���[���f�[�^�X�V�E�s�`�F�b�N
        /// </summary>
        /// <param name="targetMonth">�I����</param>
        /// <param name="departmentCode">����R�[�h</param>
        /// <param name="closeType">���ߏ����^�C�v</param>
        /// <returns></returns>
        private int JusdgeCloseProcess(DateTime targetMonth, string departmentCode, string closeType)
        {
            int ret = 0;

            string strtargetMonth = string.Format("{0:yyyyMMdd}", targetMonth); //����

            string strpreMonth = string.Format("{0:yyyyMMdd}", targetMonth.AddMonths(-1)); //�O��
            
            string strnexttMonth = string.Format("{0:yyyyMMdd}", targetMonth.AddMonths(1)); //����



            //�Ώی��̃��R�[�h
            CloseMonthControl cmctargetMonth = new CloseMonthControlDao(db).GetByKey(strtargetMonth);
            CloseMonthControl cmcnextMonth = null;
            CloseMonthControl cmcpreMonth = null;

            InventorySchedule ivstargetMonth = null;
            InventorySchedule ivsnextMonth = null;
            InventorySchedule ivspreMonth = null;

            
            //���ߏ����̎�ނɂ�镪��
            switch (closeType)
            {

                case TYPE_CLOSE_CANCEL: //�����߉���

                    //Mod 2014/09/19 arc yano �S�́^����ʂ̒��ߏ����ɂ��A�`�F�b�N������ύX����B
                    if (!string.IsNullOrEmpty(departmentCode)) //�����
                    {
                        //�����f�[�^�擾
                        ivsnextMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth.AddMonths(1));
                        //�����f�[�^�擾
                        ivstargetMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth);

                        //�����f�[�^�����݂��A���X�e�[�^�X�������߉����łȂ��ꍇ
                        if ((ivsnextMonth != null) && (!ivsnextMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̕���ʒ��ߏ����󋵂������߉����łȂ����߁A�����߉����͍s���܂���"));
                            ret = 1;
                        }

                        //�����f�[�^�����݂��Ȃ��A�܂��̓X�e�[�^�X�������łȂ��ꍇ
                        if ((ivstargetMonth == null) || (!ivstargetMonth.InventoryStatus.Equals(STS_CLOSE_START)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̕���ʒ��ߏ����󋵂��A�����߂łȂ����߁A�����߉����͍s���܂���"));
                            ret = 1;
                        }
                    }
                    else  //�S��
                    {
                        //�Ώی��̗����̃f�[�^���擾
                        cmcnextMonth = new CloseMonthControlDao(db).GetByKey(strnexttMonth);
                        
                        //�����f�[�^�����݂��A���X�e�[�^�X�������߉����łȂ��ꍇ
                        if ((cmcnextMonth != null) && (!(cmcnextMonth.CloseStatus.Equals(STS_CLOSE_CANCEL))))
                        {
                            if (cmcnextMonth.CloseStatus.Equals(STS_CLOSE_START_ON)) //�����X�e�[�^�X=�����ߒ�
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߉����łȂ����傪����܂��B�ꊇ�ŉ����߉������s���Ă�������"));
                            }
                            else
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߉����łȂ����߁A�����߉����͍s���܂���"));
                            }

                            ret = 1;
                        }
                        //�����f�[�^�����݂��Ȃ��A�܂��̓X�e�[�^�X�������A�������łȂ��ꍇ
                        if ((cmctargetMonth == null) || (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START)) && (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂��A�����߁A�܂��͉����ߒ��łȂ����߁A�����߉����͍s���܂���"));
                            ret = 1;
                        }
                    }
                    break;

                case TYPE_CLOSE_START: //������

                    
                    //Mod 2014/09/19 arc yano �S�́^����ʂ̒��ߏ����ɂ��A�`�F�b�N������ύX����B
                    if (!string.IsNullOrEmpty(departmentCode)) //�����
                    {
                        //�O���f�[�^�擾
                        ivspreMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth.AddMonths(-1));
                        //�����f�[�^�擾
                        ivstargetMonth = new InventoryScheduleDao(db).GetByKey(departmentCode, targetMonth);

                        //�O���f�[�^�����݂��Ȃ��A�܂��́A�X�e�[�^�X���A�����߂܂��͖{���߈ȊO(=�����߉���)�̏ꍇ
                        if ((ivspreMonth == null) || (ivspreMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�O���̕���ʒ��ߏ����󋵂������߁A�܂��͖{���߂łȂ����߁A�����߂͍s���܂���"));
                            ret = 1;
                        }

                        //�����f�[�^�����݂��A���X�e�[�^�X�������߉����łȂ��ꍇ
                        if ((ivstargetMonth != null) && (!ivstargetMonth.InventoryStatus.Equals(STS_CLOSE_CANCEL)))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̕���ʒ��ߏ����󋵂������߉����łȂ����߁A�����߂͍s���܂���"));
                            ret = 1;
                        }
                    }
                    else //�S��
                    {
                        //�Ώی��̑O���̃f�[�^���擾����B
                        cmcpreMonth = new CloseMonthControlDao(db).GetByKey(strpreMonth);

                        //�O���f�[�^�����݂��Ȃ�
                        if (cmcpreMonth == null)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�O���̒��ߏ����󋵂������߁A�܂��͖{���߂łȂ����߁A�����߂͍s���܂���"));
                            ret = 1;
                        }
                        else if (!(cmcpreMonth.CloseStatus.Equals(STS_CLOSE_START) || cmcpreMonth.CloseStatus.Equals(STS_CLOSE_END)))   //�O���̃X�e�[�^�X�������߁A�܂��͖{���߈ȊO�̏ꍇ
                        {
                            if(cmcpreMonth.CloseStatus.Equals(STS_CLOSE_START_ON))
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�O���̒��ߏ����󋵂������߁A�܂��͖{���߂łȂ����傪����܂��B�ꊇ�����߁A�܂��͖{���߂��s���Ă�������"));
                            }
                            else
                            {
                                ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�O���̒��ߏ����󋵂������߁A�܂��͖{���߂łȂ����߁A�����߂͍s���܂���"));
                            }
                            ret = 1;
                        }
                        else
                        {
                            //�������Ȃ�
                        }
                        //�����f�[�^�����݂��A���X�e�[�^�X�������߉����A�܂��͉����ߒ��łȂ��ꍇ
                        if ((cmctargetMonth != null) && ((!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_CANCEL)) && (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON))))
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߉����A�܂��͉����ߒ��łȂ����߁A�����߂͍s���܂���"));
                            ret = 1;
                        }
                    }
                    break;

                case TYPE_CLOSE_END: //�{����

                    //�Ώی��̑O���̃f�[�^���擾����B
                    cmcpreMonth = new CloseMonthControlDao(db).GetByKey(strpreMonth);

                    //�O���f�[�^�����݂��Ȃ��A�܂��́A�X�e�[�^�X���A�{���߂łȂ��ꍇ
                    if ((cmcpreMonth == null) || (!cmcpreMonth.CloseStatus.Equals(STS_CLOSE_END)))
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�O���̒��ߏ����󋵂��{���߂łȂ����߁A�{���߂͍s���܂���"));
                        ret = 1;
                    }

                    //Mod 2014/09/19 arc yano �ꊇ���ߏ����i�����߁A�����߉����j�̑Ώە���̕ύX�Ή� �X�e�[�^�X�������ߒ��̏ꍇ�̃G���[���b�Z�[�W��ǉ�
                    //�����̃f�[�^�����݂��Ȃ��ꍇ
                    if (cmctargetMonth == null)
                    {
                        ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߂łȂ����߁A�{���߂͍s���܂���"));
                        ret = 1;
                    }
                    else if (!cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START))�@//�����̃X�e�[�^�X�������߈ȊO�̏ꍇ
                    {
                        if (cmctargetMonth.CloseStatus.Equals(STS_CLOSE_START_ON)) //�X�e�[�^�X�������ߒ�(��ʂ̌����ڏ�͑S���剼���߂ɂȂ��Ă�ꍇ�����邽�߁A�ꊇ�����߂𑣂����b�Z�[�W��\������B)
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߂łȂ����傪����܂��B�ꊇ�����߂��s���Ă�������"));
                        }
                        else
                        {
                            ModelState.AddModelError("", MessageUtils.GetMessage("E0024", "�����̒��ߏ����󋵂������߂łȂ����߁A�{���߂͍s���܂���"));
                        }
                        ret = 1;
                    }
                    else
                    {
                        //�������Ȃ�
                    }
                    break;

                default:
                    //�����Ȃ�  �����ɗ��邱�Ƃ͂Ȃ�
                    break;
            }

            return ret;
        }

    
		//Add 2014/08/27 IPO�Ή����̂Q �����ߏ����ǉ�
        /// <summary>
        /// DB�R�~�b�g����
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        private int DbSubmit()
        {
           int ret = 0;

            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
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

                        ret = -1;
                    }
                }
                /*
                catch (SqlException e)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                    {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, "");

                        ModelState.AddModelError("DepartmentCode, InvenToryMonth", MessageUtils.GetMessage("E0010", new string[] { "����R�[�h�A�I����", "�ۑ�" }));

                        ret = 1;
                    }
                    else
                    {
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        ret = -1;
                    }
                }
                */
                catch (Exception ex)
                {
                    //ChangeConflictException�ȊO�̎��̃G���[�����ǉ�
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    ret = -1;
                }
            }
            return ret;
        }

        #region ��ʃR���|�[�l���g�ݒ�
        /// <summary>
        /// �e�R���g���[���̒l�̐ݒ�
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        private void SetDataComponent(FormCollection form) {

            CodeDao dao = new CodeDao(db);

            Department department = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
            if (department != null) {
                ViewData["DepartmentName"] = department.DepartmentName;
            }

            //ViewData["TargetMonthList"] = CodeUtils.GetSelectList(CodeUtils.GetMonthsTypeAll(), form["TargetMonth"], false);
            
            //Mod 2014/08/27 IPO�Ή����̂Q ���C�A�E�g�ύX�ɂ��A��ʃR���|�[�l���g�̐ݒ荀�ڂ̒ǉ�
            //���ߏ����^�C�v
            ViewData["CloseTypeList"] = CodeUtils.GetSelectListByModel(dao.GetCloseTypeAll(false), form["CloseType"], false);
            //�Ώ۔N
            ViewData["TargetYearList"] = CodeUtils.GetSelectListByModel(dao.GetYearAll(true), form["TargetYear"], false);
            
            //�Ώی�
            ViewData["TargetMonthList"] = CodeUtils.GetSelectListByModel(dao.GetMonthAll(true), form["TargetMonth"], false);

            //����R�[�h
            ViewData["DepartmentCode"] = form["DepartmentCode"];

            //�����͈�
            ViewData["TargetRange"] = form["TargetRange"]; 

            //����t���O
            ViewData["CMOperateFlag"] = form["hdCMOperateFlag"];
            
            //�����
            ViewData["CMOperateUser"] = form["hdCMOperateUser"];
        }
        #endregion

        #region ����R�[�h����Y������̒I���󋵂�ԋp����(Ajax��p�j
        /// <summary>
        /// ����R�[�h����Y������̒I���󋵂�ԋp����(Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 �y�區�ځz����I�����Ή� �����̕ύX(departmentCode �� warehouseCode)
        /// </history>
        public ActionResult PartsInventorySchedule(string WarehouseCode, DateTime InventoryMonth)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();
                //InventoryScheduleParts InventorySchedule = new InventorySchedulePartsDao(db).GetByKey(DepartmentCode, InventoryMonth, "002");
                InventoryScheduleParts InventorySchedule = new InventorySchedulePartsDao(db).GetByKey(WarehouseCode, InventoryMonth, "002");
                if (InventorySchedule != null)
                {
                    codeData.Code = InventorySchedule.InventoryStatus;
                    if (InventorySchedule.InventoryStatus == "001"){
                        codeData.Name = "���{��";
                    }else{
                        codeData.Name = "�m��";
                    }
                    
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
        #endregion

        #region ����R�[�h����Y������̎ԗ��I���󋵂�ԋp����(Ajax��p�j
        /// <summary>
        /// ����R�[�h����Y������̒I���󋵂�ԋp����(Ajax��p�j
        /// </summary>
        /// <param name="code">����R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 �ԗ��݌ɒI���@�\�ǉ� �V�K�쐬
        /// </history>
        public ActionResult CarInventorySchedule(string WarehouseCode, DateTime InventoryMonth)
        {
            if (Request.IsAjaxRequest())
            {
                CodeData codeData = new CodeData();

                if (!string.IsNullOrWhiteSpace(WarehouseCode))
                {
                    InventoryScheduleCar InventorySchedule = new InventoryScheduleCarDao(db).GetByKey(WarehouseCode, InventoryMonth);
                    if (InventorySchedule != null)
                    {
                        codeData.Code = InventorySchedule.InventoryStatus;

                        switch (codeData.Code)
                        {
                            case "001": //���{��
                                codeData.Name = "���{��";
                                break;
                            case "002": //���m��
                                codeData.Name = "���m��";
                                break;
                            case "003": //�{�m��
                                codeData.Name = "�m��";
                                break;
                            default:
                                codeData.Name = "";
                                break;
                        }
                    }
                    else
                    {
                        //���݂��Ȃ��ꍇ�͖����{�Ƃ��Ĉ���
                        codeData.Code = "000";
                        codeData.Name = "�����{";
                    }
                }
                else //�q�ɃR�[�h�������͂̏ꍇ�̓X�e�[�^�X�s���Ƃ���
                {
                    codeData.Code = "999";
                    codeData.Name = "�s��";
                }

                
                return Json(codeData);
            }
            return new EmptyResult();
        }
        #endregion

        //Add 2016/11/30 #3659 �ԗ��Ǘ� �������Ή�
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

    }
}
