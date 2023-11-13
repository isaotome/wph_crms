using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Transactions;
using Crms.Models;                      //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    //Add 2015/01/14 arc yano ���̃R���g���[���Ɠ������A�t�B���^����(��O�A�Z�L�����e�B�A�o�̓L���b�V��)��ǉ�
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BankController : InheritedController
    {

        //Add 2014/08/04 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "��s�}�X�^";     // ��ʖ�
        private static readonly string PROC_NAME = "��s�}�X�^�o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public BankController() {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ������ʕ\��
        /// </summary>
        /// <returns></returns>
        public ActionResult Criteria() {
            FormCollection form = new FormCollection();
            form["DelFlag"] = "0";
            return Criteria(form);
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["DelFlag"] = form["DelFlag"];
            PaginatedList<Bank> list = GetSearchResult(form);
            return View("BankCriteria", list);
        }

        /// <summary>
        /// �����_�C�A���O�\��
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �����_�C�A���O����
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["id"] = form["id"];
            PaginatedList<Bank> list = GetSearchResult(form);
            return View("BankCriteriaDialog", list);
        }

        /// <summary>
        /// �������ʂ��擾����
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private PaginatedList<Bank> GetSearchResult(FormCollection form) {
            Bank condition = new Bank();
            condition.BankCode = form["BankCode"];
            condition.BankName = form["BankName"];
            condition.DelFlag = form["DelFlag"];
            return new BankDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
        }

        /// <summary>
        /// ��s�}�X�^�ǉ��E�X�V��ʕ\��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Entry(string id) {
            Bank model;
            if (string.IsNullOrEmpty(id)) {
                model = new Bank();
                ViewData["update"] = "0";
            } else {
                //ADD 2014/10/29 ishii �ۑ��{�^���Ή�
                //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                db = new CrmsLinqDataContext();
                model = new BankDao(db).GetByKey(id, true);
                ViewData["update"] = "1";
            }
            return View("BankEntry", model);
        }

        /// <summary>
        /// ��s�}�X�^�ǉ��E�X�V
        /// </summary>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Bank model,EntitySet<Branch> branches, FormCollection form) {
            // ADD 2014/06/09 arc uchida �`�F�b�N�����̎��s
            ValidateBank(model);
            if (!ModelState.IsValid) {
                //ADD 2014/10/30 ishii �ۑ��{�^���Ή�
                model.Branch = branches;
                SetDataComponent(form);
                return View("BankEntry", model);
            }

            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {
                // �f�[�^�X�V����
                if (form["update"].Equals("1")) {
                    //Mod 2015/04/08 arc nakayama �����f�[�^���J���Ɨ�����Ή��@�X�V�̏ꍇ�͍l�����Ȃ��i�����f�[�^���J���Ȃ����߁j
                    Bank target = new BankDao(db).GetByKey(model.BankCode, true);

                    //ADD arc uchida vs2012�Ή�
                    if (branches != null)
                    {
                        foreach (var branch1 in branches)
                        {
                            int flag = 0;
                            foreach (var branch2 in branches)
                            {
                                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                                if (CommonUtils.DefaultString(branch1.BranchCode).Equals(CommonUtils.DefaultString(branch2.BranchCode)))
                                {
                                    flag += 1;
                                }
                            }
                            if (flag >= 2)
                            {
                                ModelState.AddModelError("Reason", MessageUtils.GetMessage("E0010", new string[] { branch1.BranchCode, "�ύX" }));
                                break;
                            }
                        }
                    }
                    if (ModelState.IsValid)
                    {
                        // �x�X���Ȃ��Ȃ��Ă�����̂͘_���폜����
                        // ���̃f�[�^�x�[�X
                        foreach (var branch in target.Branch)
                        {

                            //ADD arc uchida vs2012�Ή�
                            if (branches == null)
                            {
                                branch.DelFlag = "1";
                            }
                            else
                            {
                                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                                // ��ʂ̓��̓��X�g�ɑ��݂��邩
                                Branch updateBranch = (from a in branches
                                                       where CommonUtils.DefaultString(a.BranchCode).Equals(CommonUtils.DefaultString(branch.BranchCode))
                                                       select a).FirstOrDefault();

                                // ���݂��Ȃ�������폜����邱�ƂɂȂ�
                                if (updateBranch == null)
                                {
                                    branch.DelFlag = "1";

                                    // ���݂��邯�ǁA���̃f�[�^���_���폜����Ă����畜��������
                                }
                                //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                                else if (CommonUtils.DefaultString(branch.DelFlag).Equals("1"))
                                {
                                    // ADD arc uchida vs2012�Ή�
                                    if (branch.BranchCode != null && branch.BranchCode != "")
                                    {
                                        branch.BranchName = updateBranch.BranchName;
                                        branch.DelFlag = "0";
                                    }
                                }
                            }
                            EditForBranchUpdate(branch);

                        }
                        //ADD arc uchida vs2012�Ή�
                        if (branches != null)
                        {
                            foreach (var branch in branches)
                            {
                                // ADD arc uchida vs2012�Ή�
                                if (branch.BranchCode != "" && branch.BranchCode != null)
                                {
                                    Branch branchTarget = new BranchDao(db).GetByKey(branch.BranchCode, branch.BankCode);
                                    if (branchTarget != null)
                                    {
                                        branchTarget.BranchName = branch.BranchName;
                                        EditForBranchUpdate(branchTarget);
                                    }
                                    else
                                    {
                                        EditForBranchCreate(branch);
                                        db.Branch.InsertOnSubmit(branch);
                                    }
                                }
                            }
                        }
                        UpdateModel(target);
                    }
                    // ADD arc uchida vs2012�Ή�
                    else
                    {
                        model.Branch = branches;
                        SetDataComponent(form);
                        return View("BankEntry", model);
                    }
                } else {
                    model.CreateDate = DateTime.Now;
                    model.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    model.LastUpdateDate = DateTime.Now;
                    model.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    model.DelFlag = "0";
                    db.Bank.InsertOnSubmit(model);

                    if (branches != null) {
                        foreach (var branch in branches) {
                           db.Branch.InsertOnSubmit(branch);
                        }
                    }
                }
                // Mod 2014/08/04 arc amii �G���[���O�Ή� ���O�o�͂���ׁAtry catch����ǉ�
                for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
                {
                    try
                    {
                        db.SubmitChanges();
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
                    catch (SqlException se)
                    {
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        
                        if (se.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR)
                        {
                            // ���O�ɏo��
                            OutputLogger.NLogError(se, PROC_NAME, FORM_NAME, "");

                            ModelState.AddModelError("BankCode", MessageUtils.GetMessage("E0011", "�ۑ�"));
                            SetDataComponent(form);
                            return View("BankEntry", model);
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
                        // �Z�b�V������SQL����o�^
                        Session["ExecSQL"] = OutputLogData.sqlText;
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, "");
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                
            }
           
            //MOD 2014/10/29 ishii �ۑ��{�^���Ή�
            ModelState.Clear();
            SetDataComponent(form);
            ModelState.AddModelError("", MessageUtils.GetMessage("I0001"));
            //ViewData["close"] = "1";
            //return View("BankEntry", model);
            return Entry(model.BankCode);
        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="model"></param>
        private void ValidateBank(Bank model) {
            CommonValidate("BankCode", "��s�R�[�h", model, true);
        }

        /// <summary>
        /// ��ʃR���|�[�l���g
        /// </summary>
        /// <param name="form"></param>
        private void SetDataComponent(FormCollection form) {
            ViewData["update"] = form["update"];
        }

        /// <summary>
        /// CreateDate�X�V
        /// </summary>
        /// <param name="branch"></param>
        private void EditForBranchCreate(Branch branch) {
            branch.CreateDate = DateTime.Now;
            branch.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            branch.DelFlag = "0";
            EditForBranchUpdate(branch);
        }
        /// <summary>
        /// LastUpdate�X�V
        /// </summary>
        /// <param name="branch"></param>
        private void EditForBranchUpdate(Branch branch) {
            branch.LastUpdateDate = DateTime.Now;
            branch.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
        }
         
        /// <summary>
        /// �x�X���X�g��1�s�ǉ�����
        /// </summary>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult AddBranch(Bank model,EntitySet<Branch> branches, FormCollection form) {
            
            if (branches == null) {
                branches = new EntitySet<Branch>();
            }
            Branch branch = new Branch() { BankCode = model.BankCode, DelFlag = "0" };
            branches.Add(branch);

            ModelState.Clear();

            model.Branch = branches;
            SetDataComponent(form);
            return View("BankEntry", model);
        }

        /// <summary>
        /// �x�X���X�g��1�s�폜����
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="branches"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ActionResult DelBranch(int id,Bank model, EntitySet<Branch> branches, FormCollection form) {
            branches.RemoveAt(id);
            ModelState.Clear();
            model.Branch = branches;
            SetDataComponent(form); 
            return View("BankEntry", model);
        }

        /// <summary>
        /// ��s�R�[�h�����s�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�u�����h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Bank bank = new BankDao(db).GetByKey(code);
                if (bank != null) {
                    codeData.Code = bank.BankCode;
                    codeData.Name = bank.BankName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
    }
}
