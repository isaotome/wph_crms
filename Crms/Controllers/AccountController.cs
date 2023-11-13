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

namespace Crms.Controllers {

    /// <summary>
    /// �Ȗڃ}�X�^�A�N�Z�X�@�\�R���g���[���N���X
    /// </summary>
    [ExceptionFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class AccountController : Controller {

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public AccountController() {
            db = CrmsDataContext.GetDataContext();
        }

        /// <summary>
        /// �Ȗڌ����_�C�A���O�\��
        /// </summary>
        /// <returns>�Ȗڌ����_�C�A���O</returns>
        public ActionResult CriteriaDialog() {
            return CriteriaDialog(new FormCollection());
        }

        /// <summary>
        /// �Ȗڌ����_�C�A���O�\��
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ȗڌ�����ʃ_�C�A���O</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CriteriaDialog(FormCollection form) {

            // ���������̐ݒ�
            form["DelFlag"] = (form["DelFlag"] == null ? "0" : form["DelFlag"]);

            switch (Request["journalType"]) {
                case "001":
                    form["CreditFlag"] = "1";
                    break;
                case "002":
                    form["DebitFlag"] = "1";
                    break;
                default:
                    break;
            }
            // �������ʃ��X�g�̎擾
            PaginatedList<Account> list = GetSearchResultList(form);

            // �Ȗڌ�����ʂ̕\��
            return View("AccountCriteriaDialog", list);
        }

        /// <summary>
        /// �ȖڃR�[�h����Ȗږ����擾����(Ajax��p�j
        /// </summary>
        /// <param name="code">�ȖڃR�[�h</param>
        /// <returns>�擾����(�擾�ł��Ȃ��ꍇ�ł�null�ł͂Ȃ�)</returns>
        public ActionResult GetMaster(string code) {

            if (Request.IsAjaxRequest()) {
                CodeData codeData = new CodeData();
                Account account = new AccountDao(db).GetByKey(code);
                if (account != null) {
                    codeData.Code = account.AccountCode;
                    codeData.Name = account.AccountName;
                }
                return Json(codeData);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// �Ȗڃ}�X�^�������ʃ��X�g�擾
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^(��������)</param>
        /// <returns>�Ȗڃ}�X�^�������ʃ��X�g</returns>
        private PaginatedList<Account> GetSearchResultList(FormCollection form) {

            AccountDao accountDao = new AccountDao(db);
            Account accountCondition = new Account();
            if (form["DelFlag"].Equals("0") || form["DelFlag"].Equals("1")) {
                accountCondition.DelFlag = form["DelFlag"];
            }
            accountCondition.CreditFlag = form["CreditFlag"];
            accountCondition.DebitFlag = form["DebitFlag"];
            return accountDao.GetListByCondition(accountCondition, int.Parse(form["id"] ?? "0"), 20);
        }
    }
}
