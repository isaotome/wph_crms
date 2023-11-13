using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using Crms.Models;                      //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.SqlClient;            //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
using System.Data.Linq;                 //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class TaskController : Controller
    {
        //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "�^�X�N";   // ��ʖ�
        private static readonly string PROC_NAME = "�^�X�N���s"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TaskController()
        {
            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// ������ʕ\��
        /// </summary>
        /// <returns>�^�X�N�������</returns>
        public ActionResult Criteria()
        {
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// �^�X�N����
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            List<SelectListItem> conf = new List<SelectListItem>();
            List<TaskConfig> taskConfigList = new TaskConfigDao(db).GetAllList(false);
            conf.Add(new SelectListItem{Text="",Value=""});
            foreach (var a in taskConfigList) {
                conf.Add(new SelectListItem { Text = a.TaskName, Value = a.TaskConfigId, Selected = a.TaskConfigId.Equals(form["TaskConfigId"]) });
            }
            ViewData["TaskConfigList"] = conf;

            Task taskCondition = new Task();
            if (!string.IsNullOrEmpty(form["TaskStatus"]) && form["TaskStatus"].Contains("true")) {
                taskCondition.TaskStatus = true;
            } else {
                taskCondition.TaskStatus = false;
            }
            ViewData["TaskStatus"] = taskCondition.TaskStatus;
            taskCondition.TaskConfigId = form["TaskConfigId"];
            List<Task> list = ((new TaskDao(db)).GetListByEmployeeCode(taskCondition,((Employee)Session["Employee"]).EmployeeCode, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE));
            foreach (var item in list) {
                if (!string.IsNullOrEmpty(item.SlipNumber)) {
                    CarSalesHeader car = new CarSalesOrderDao(db).GetBySlipNumber(item.SlipNumber);
                    if (car != null) {
                        try {
                            item.CustomerName = car.Customer.CustomerName;
                        } catch { }
                    } else {
                        ServiceSalesHeader service = new ServiceSalesOrderDao(db).GetBySlipNumber(item.SlipNumber);
                        try {
                            item.CustomerName = service.Customer.CustomerName;
                        } catch { }
                    }
                }
            }
            return View("TaskCriteria", list);
        }

        /// <summary>
        /// �^�X�N���s
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Entry(string id)
        {
            Task t = ((new TaskDao(db)).GetByKey(new Guid(id)));
            return View("TaskEntry",t);
        }

        /// <summary>
        /// �^�X�N���s���̏���
        /// </summary>
        /// <param name="form">�t�H�[���f�[�^</param>
        /// <returns>�^�X�N�\�����</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(Task task,FormCollection form) {
            if (form["TaskType"].Equals("003")) {
                if (string.IsNullOrEmpty(form["ActionResult"])) {
                    ModelState.AddModelError("", "�������e����͂��ĉ�����");
                    
                    return View("TaskEntry",new TaskDao(db).GetByKey(new Guid(form["TaskId"])));
                }
            }

            // Add 2014/08/11 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            Task t = new TaskDao(db).GetByKey(new Guid(form["TaskId"]));
            t.ActionResult = form["ActionResult"];
            t.TaskCompleteDate = DateTime.Now;
            t.LastUpdateDate = DateTime.Now;
            t.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;

            //Add 2014/08/12 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈�trycatch���ǉ�
            for (int i = 0; i < DaoConst.MAX_RETRY_COUNT; i++)
            {
                try
                {
                    db.SubmitChanges();
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
                        OutputLogger.NLogFatal(cfe, PROC_NAME, FORM_NAME, "");
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
            
            ViewData["close"] = "1";
            return View("TaskEntry", t);
        }
        
        /// <summary>
        /// �^�X�N�ǉ��|�b�v�A�b�v
        /// </summary>
        /// <returns></returns>
        public ActionResult CriteriaDialog(string id) {
            Task task;
            if (string.IsNullOrEmpty(id)) {
                task = new Task();
            } else {
                task = new TaskDao(db).GetByKey(new Guid(id));
            }
            return View("TaskDialog",task);
        }

    }
}
