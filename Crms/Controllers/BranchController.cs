using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;

namespace Crms.Controllers
{

    //Add 2015/01/14 arc yano ���̃R���g���[���Ɠ������A�t�B���^����(��O�A�Z�L�����e�B�A�o�̓L���b�V��)��ǉ�
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class BranchController : Controller
    {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public BranchController() {
            db = new CrmsLinqDataContext();
        }
        /// <summary>
        /// �����_�C�A���O�\��
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id){
            FormCollection form = new FormCollection();
            form["BankCode"] = id;
            return CriteriaDialog(form);
        }
        /// <summary>
        /// �����_�C�A���O
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {
            ViewData["BankCode"] = form["BankCode"];
            ViewData["BankName"] = form["BankName"];
            ViewData["BranchCode"] = form["BranchCode"];
            ViewData["BranchName"] = form["BranchName"];

            Branch condition = new Branch();
            condition.Bank = new Bank();
            condition.Bank.BankCode = form["BankCode"];
            condition.Bank.BankName = form["BankName"];
            condition.BranchCode = form["BranchCode"];
            condition.BranchName = form["BranchName"];
            PaginatedList<Branch> list = new BranchDao(db).GetByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
            return View("BranchCriteriaDialog", list);
        }

        /// <summary>
        /// �x�X�R�[�h����x�X�����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�u�����h�R�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string bankCode,string branchCode) {
            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Branch branch = new BranchDao(db).GetByKey(branchCode, bankCode);
                if (branch != null) {
                    codeData.Code = branch.BranchCode;
                    codeData.Name = branch.BranchName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }
    }
}
