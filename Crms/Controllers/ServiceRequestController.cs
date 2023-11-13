using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Data.Linq;
using System.Transactions;
using System.Data.SqlClient;
using Crms.Models;                      //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceRequestController : Controller
    {
        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "��ƈ˗��쐬"; // ��ʖ�
        private static readonly string PROC_NAME = "��ƈ˗��o�^"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ServiceRequestController() {
            db = new CrmsLinqDataContext();
        }

        //Add 2017/02/17 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ�
        /// <summary>
        /// ������ʏ����\�������t���O
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// ������ʏ����\��
        /// </summary>
        /// <returns>��������</returns>
        public ActionResult Criteria() {
            //Add 2017/02/17 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ�
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// ������ʕ\��
        /// </summary>
        /// <param name="form">�t�H�[���̓��͒l</param>
        /// <returns>��������</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            //Add 2017/02/17 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ� ����\���Ō��������Ȃ�
            PaginatedList<ServiceRequest> list = new PaginatedList<ServiceRequest>();

            if (criteriaInit)
            {
                //����\���͌������Ȃ�
            }
            else
            {
                if (!string.IsNullOrEmpty(form["DepartmentCode"]))
                {
                    Department departemnt = new DepartmentDao(db).GetByKey(form["DepartmentCode"]);
                    ViewData["DepartmentName"] = departemnt.DepartmentName;
                }

                ViewData["DepartmentCode"] = form["DepartmentCode"];

                if (!string.IsNullOrEmpty(form["EmployeeCode"]))
                {
                    Employee employee = new EmployeeDao(db).GetByKey(form["EmployeeCode"]);
                    ViewData["EmployeeName"] = employee.EmployeeName;
                }
                ViewData["EmployeeCode"] = form["EmployeeCode"];
                ViewData["CustomerName"] = form["CustomerName"];
                ViewData["Vin"] = form["Vin"];
                ViewData["ArrivalPlanDateFrom"] = form["ArrivalPlanDateFrom"];
                ViewData["ArrivalPlanDateTo"] = form["ArrivalPlanDateTo"];
                ViewData["DeliveryRequirementFrom"] = form["DeliveryRequirementFrom"];
                ViewData["DeliveryRequirementTo"] = form["DeliveryRequirementTo"];

                ServiceRequest condition = new ServiceRequest();
                condition.DepartmentCode = form["DepartmentCode"];
                condition.EmployeeCode = form["EmployeeCode"];
                condition.Vin = form["Vin"];
                condition.CustomerName = form["CustomerName"];
                condition.CarPurchaseOrder = new CarPurchaseOrder();
                condition.ArrivalPlanDateFrom = CommonUtils.StrToDateTime(form["ArrivalPlanDateFrom"], DaoConst.SQL_DATETIME_MIN);
                condition.ArrivalPlanDateTo = CommonUtils.StrToDateTime(form["ArrivalPlanDateTo"], DaoConst.SQL_DATETIME_MAX);
                condition.DeliveryRequirementFrom = CommonUtils.StrToDateTime(form["DeliveryRequirementFrom"], DaoConst.SQL_DATETIME_MIN);
                condition.DeliveryRequirementTo = CommonUtils.StrToDateTime(form["DeliveryRequirementTo"], DaoConst.SQL_DATETIME_MAX);
                condition.SetAuthCondition((Employee)Session["Employee"]);
                list = new ServiceRequestDao(db).GetListByCondition(condition, int.Parse(form["id"] ?? "0"), DaoConst.PAGE_SIZE);
                CarSalesOrderDao cDao = new CarSalesOrderDao(db);
                CarPurchaseOrderDao pDao = new CarPurchaseOrderDao(db);

                foreach (var a in list)
                {
                    a.CarSalesHeader = cDao.GetBySlipNumber(a.OriginalSlipNumber);
                    a.CarPurchaseOrder = pDao.GetBySlipNumber(a.OriginalSlipNumber);
                }
            }
            return View("ServiceRequestCriteria", list);
        }

        /// <summary>
        /// ��ƈ˗����͉�ʂ�\��
        /// </summary>
        /// <param name="SlipNo">�ԗ��`�[�ԍ�</param>
        /// <returns>��ƈ˗����͉��</returns>
        public ActionResult Entry(string SlipNo) {

            //���N�G�X�g���ꂽ�`�[���擾
            //��ƈ˗�
            ServiceRequest req = new ServiceRequestDao(db).GetBySlipNumber(SlipNo);
            
            //���N�G�X�g���ꂽ��ƈ˗������݂��Ă���΂��̏���\������
            if (req != null) {
                //SetDataRelation(ref req);

                //�ԗ��`�[
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
                req.CarSalesHeader = header;

                //�ԗ������˗�����
                CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
                req.CarPurchaseOrder = order;

                Department department = new DepartmentDao(db).GetByKey(req.DepartmentCode);
                ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
                SetDataComponent(req);

                EntitySet<ServiceRequestLine> NewList = new EntitySet<ServiceRequestLine>();
                if (header != null && header.CarSalesLine != null)
                {
                    foreach (var h in header.CarSalesLine)
                    {
                        ServiceRequestLine line = new ServiceRequestLine();
                        line.CarOptionCode = h.CarOptionCode;
                        line.c_OptionType = new c_OptionType();
                        line.c_OptionType.Code = h.OptionType;
                        line.c_OptionType.Name = h.c_OptionType.Name;
                        line.Amount = h.Amount;
                        line.LineNumber = h.LineNumber;
                        line.CarOptionName = h.CarOptionName;
                        NewList.Add(line);
                    }

                    if (NewList != null)
                    {
                        req.ServiceRequestLine = NewList;
                    }
                }
                return View("ServiceRequestEntry", req);
            }
            req = new ServiceRequest();
            req.OriginalSlipNumber = SlipNo;
            SetDataRelation(ref req);
            return View("ServiceRequestEntry",req);
        }

        /// <summary>
        /// �ԗ��`�[�A�ԗ������˗��A��ƈ˗���R�Â���
        /// </summary>
        /// <param name="request"></param>
        private void SetDataRelation(ref ServiceRequest request) {

            //�ԗ��`�[
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(request.OriginalSlipNumber);
            request.CarSalesHeader = header;

            //�ԗ������˗�����
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(request.OriginalSlipNumber);
            request.CarPurchaseOrder = order;
            
            //�ԗ��`�[�̃I�v�V��������ƈ˗��ɕR�t��
            if (header != null && header.CarSalesLine != null) {
                foreach (var h in header.CarSalesLine) {
                    ServiceRequestLine line = new ServiceRequestLine();
                    line.CarOptionCode = h.CarOptionCode;
                    line.c_OptionType = new c_OptionType();
                    line.c_OptionType.Code = h.OptionType;
                    line.c_OptionType.Name = h.c_OptionType.Name;
                    line.Amount = h.Amount;
                    line.LineNumber = h.LineNumber;
                    line.CarOptionName = h.CarOptionName;
                    request.ServiceRequestLine.Add(line);
                }
            }

            //ViewData�̃Z�b�g
            SetDataComponent(request);
        }

        /// <summary>
        /// ��ƈ˗��o�^����
        /// </summary>
        /// <param name="request">��ƈ˗��`�[�f�[�^</param>
        /// <param name="line">��ƈ˗����׃f�[�^</param>
        /// <param name="form">�t�H�[�����͒l</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceRequest request,EntitySet<ServiceRequestLine> line,FormCollection form) {
            ValidateServiceRequest(request);
            if (!ModelState.IsValid) {
                SetDataRelation(ref request);
                return View("ServiceRequestEntry", request);
            }

            // Add 2014/08/07 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                //2017/02/21 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ�
                //�Â������폜����
                ServiceRequest PrevRequest = new ServiceRequestDao(db).GetBySlipNumber(request.OriginalSlipNumber);
                if (PrevRequest != null)
                {
                    PrevRequest.DelFlag = "1";

                    List<ServiceRequestLine> PrevLine = new ServiceRequestDao(db).GetLineByServiceRequestId(PrevRequest.ServiceRequestId.ToString());
                    foreach (var pline in PrevLine)
                    {
                        pline.DelFlag = "1";
                    }

                    db.SubmitChanges();
                }



                request.ServiceRequestLine = line;
                request.ServiceRequestId = Guid.NewGuid();
                request.CreateDate = DateTime.Today;
                request.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                request.LastUpdateDate = DateTime.Today;
                request.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                request.DelFlag = "0";

                foreach (var a in request.ServiceRequestLine)
                {
                    a.CreateDate = DateTime.Today;
                    a.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    a.LastUpdateDate = DateTime.Today;
                    a.LastUpdateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
                    a.DelFlag = "0";
                }

                db.ServiceRequest.InsertOnSubmit(request);

                //��ƈ˗��^�X�N�ǉ�
                TaskUtil task = new TaskUtil(db, ((Employee)Session["Employee"]));
                task.ServiceRequest(request);

                // Add 2014/08/07 arc amii �G���[���O�Ή� catch��Ƀ��O�o�͏�����ǉ�
                try {
                    db.SubmitChanges();
                    ts.Complete();
                } catch (SqlException e) {

                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        // ���O�ɏo��
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, request.OriginalSlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "�o�^"));

                        SetDataRelation(ref request);
                        return View("ServiceRequestEntry", request);
                    } else {
                        // ���O�ɏo��
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, request.OriginalSlipNumber);
                        // �G���[�y�[�W�ɑJ��
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // �Z�b�V������SQL����o�^
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ���O�ɏo��
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // �G���[�y�[�W�ɑJ��
                    return View("Error");
                }
                
            }
            ViewData["close"] = "1";
            if (!string.IsNullOrEmpty(form["PrintReport"]))
            {
                ViewData["reportName"] = form["PrintReport"];
                ViewData["close"] = "0";
            }
            SetDataRelation(ref request);
            return View("ServiceRequestEntry", request);
        }
        
        /// <summary>
        /// �f�[�^�t����ʃR���|�[�l���g�̐ݒ�
        /// </summary>
        /// <param name="salesCar">���f���f�[�^</param>
        private void SetDataComponent(ServiceRequest header) {
            CodeDao dao = new CodeDao(db);
            ViewData["InsuranceInheritanceList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), header.InsuranceInheritance, false);
            ViewData["AnnualInspectionList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), header.AnnualInspection, false);
            ViewData["OwnershipChangeList"] = CodeUtils.GetSelectListByModel<c_OwnershipChange>(dao.GetOwnershipChangeAll(false), header.OwnershipChange, false);

        }

        /// <summary>
        /// Validation�`�F�b�N
        /// </summary>
        /// <param name="header">��ƈ˗��`�[�f�[�^</param>
        private void ValidateServiceRequest(ServiceRequest header) {
            if (string.IsNullOrEmpty(header.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "����R�[�h"));
            }
            if (header.DeliveryRequirement == null) {
                ModelState.AddModelError("DeliveryRequirement", MessageUtils.GetMessage("E0001", "��]�[��"));
            }
            if (!ModelState.IsValidField("DeliveryRequirement")) {
                ModelState.AddModelError("DeliveryRequirement",MessageUtils.GetMessage("E0005","��]�[��"));
            }
            if (ModelState.IsValidField("DeliveryRequirement") && header.DeliveryRequirement!=null ? header.DeliveryRequirement.Value.CompareTo(DateTime.Today) < 0 : true) {
                ModelState.AddModelError("DeliveryRequirement", MessageUtils.GetMessage("E0013", new string[] { "��]�[��", "�{���ȍ~" }));
            }

        }

        #region �������A�j���[�V����
        //Add 2014/12/22 arc nakayama IPO�Ή�(�ڋqDM����) �������Ή�
        /// <summary>
        /// ���������ǂ������擾����B(Ajax��p�j
        /// </summary>
        /// <param name="processType">�������</param>
        /// <returns>��������</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retSearch = new Dictionary<string, string>();

                retSearch.Add("ProcessedTime", "��������");

                return Json(retSearch);
            }
            return new EmptyResult();
        }

        #endregion

        /// <summary>
        /// ��ƈ˗���ʂ�\��
        /// </summary>
        /// <param name="SlipNo">�ԗ��`�[�ԍ�</param>
        /// <returns>��ƈ˗����͉��</returns>
        public ActionResult Result(string SlipNo)
        {

            //���N�G�X�g���ꂽ�`�[���擾
            //��ƈ˗�
            ServiceRequest req = new ServiceRequestDao(db).GetBySlipNumber(SlipNo);

            //�ԗ��`�[
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
            req.CarSalesHeader = header;

            //�ԗ������˗�����
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
            req.CarPurchaseOrder = order;

            Department department = new DepartmentDao(db).GetByKey(req.DepartmentCode);
            ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            return View("ServiceRequestResult", req);
        }
    }
}
