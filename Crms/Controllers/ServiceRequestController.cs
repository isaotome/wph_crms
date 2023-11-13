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
using Crms.Models;                      //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    [ExceptionFilter]
    [AuthFilter]
    [ValidateInput(false)]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ServiceRequestController : Controller
    {
        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "作業依頼作成"; // 画面名
        private static readonly string PROC_NAME = "作業依頼登録"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ServiceRequestController() {
            db = new CrmsLinqDataContext();
        }

        //Add 2017/02/17 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
        /// <summary>
        /// 検索画面初期表示処理フラグ
        /// </summary>
        private bool criteriaInit = false;

        /// <summary>
        /// 検索画面初期表示
        /// </summary>
        /// <returns>検索結果</returns>
        public ActionResult Criteria() {
            //Add 2017/02/17 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
            criteriaInit = true;
            return Criteria(new FormCollection());
        }

        /// <summary>
        /// 検索画面表示
        /// </summary>
        /// <param name="form">フォームの入力値</param>
        /// <returns>検索結果</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Criteria(FormCollection form) {

            //Add 2017/02/17 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない 初回表示で検索させない
            PaginatedList<ServiceRequest> list = new PaginatedList<ServiceRequest>();

            if (criteriaInit)
            {
                //初回表示は検索しない
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
        /// 作業依頼入力画面を表示
        /// </summary>
        /// <param name="SlipNo">車両伝票番号</param>
        /// <returns>作業依頼入力画面</returns>
        public ActionResult Entry(string SlipNo) {

            //リクエストされた伝票を取得
            //作業依頼
            ServiceRequest req = new ServiceRequestDao(db).GetBySlipNumber(SlipNo);
            
            //リクエストされた作業依頼が存在していればその情報を表示する
            if (req != null) {
                //SetDataRelation(ref req);

                //車両伝票
                CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
                req.CarSalesHeader = header;

                //車両発注依頼引当
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
        /// 車両伝票、車両発注依頼、作業依頼を紐づける
        /// </summary>
        /// <param name="request"></param>
        private void SetDataRelation(ref ServiceRequest request) {

            //車両伝票
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(request.OriginalSlipNumber);
            request.CarSalesHeader = header;

            //車両発注依頼引当
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(request.OriginalSlipNumber);
            request.CarPurchaseOrder = order;
            
            //車両伝票のオプションを作業依頼に紐付け
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

            //ViewDataのセット
            SetDataComponent(request);
        }

        /// <summary>
        /// 作業依頼登録処理
        /// </summary>
        /// <param name="request">作業依頼伝票データ</param>
        /// <param name="line">作業依頼明細データ</param>
        /// <param name="form">フォーム入力値</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Entry(ServiceRequest request,EntitySet<ServiceRequestLine> line,FormCollection form) {
            ValidateServiceRequest(request);
            if (!ModelState.IsValid) {
                SetDataRelation(ref request);
                return View("ServiceRequestEntry", request);
            }

            // Add 2014/08/07 arc amii エラーログ対応 登録用にDataContextを設定する
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            using (TransactionScope ts = new TransactionScope()) {

                //2017/02/21 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
                //古い情報を削除する
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

                //作業依頼タスク追加
                TaskUtil task = new TaskUtil(db, ((Employee)Session["Employee"]));
                task.ServiceRequest(request);

                // Add 2014/08/07 arc amii エラーログ対応 catch句にログ出力処理を追加
                try {
                    db.SubmitChanges();
                    ts.Complete();
                } catch (SqlException e) {

                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;

                    if (e.Number == DaoConst.DUP_VAL_ON_INDEX_ERROR) {
                        // ログに出力
                        OutputLogger.NLogError(e, PROC_NAME, FORM_NAME, request.OriginalSlipNumber);

                        ModelState.AddModelError("", MessageUtils.GetMessage("E0011", "登録"));

                        SetDataRelation(ref request);
                        return View("ServiceRequestEntry", request);
                    } else {
                        // ログに出力
                        OutputLogger.NLogFatal(e, PROC_NAME, FORM_NAME, request.OriginalSlipNumber);
                        // エラーページに遷移
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // セッションにSQL文を登録
                    Session["ExecSQL"] = OutputLogData.sqlText;
                    // ログに出力
                    OutputLogger.NLogFatal(ex, PROC_NAME, FORM_NAME, "");
                    // エラーページに遷移
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
        /// データ付き画面コンポーネントの設定
        /// </summary>
        /// <param name="salesCar">モデルデータ</param>
        private void SetDataComponent(ServiceRequest header) {
            CodeDao dao = new CodeDao(db);
            ViewData["InsuranceInheritanceList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), header.InsuranceInheritance, false);
            ViewData["AnnualInspectionList"] = CodeUtils.GetSelectListByModel<c_OnOff>(dao.GetOnOffAll(false), header.AnnualInspection, false);
            ViewData["OwnershipChangeList"] = CodeUtils.GetSelectListByModel<c_OwnershipChange>(dao.GetOwnershipChangeAll(false), header.OwnershipChange, false);

        }

        /// <summary>
        /// Validationチェック
        /// </summary>
        /// <param name="header">作業依頼伝票データ</param>
        private void ValidateServiceRequest(ServiceRequest header) {
            if (string.IsNullOrEmpty(header.DepartmentCode)) {
                ModelState.AddModelError("DepartmentCode", MessageUtils.GetMessage("E0001", "部門コード"));
            }
            if (header.DeliveryRequirement == null) {
                ModelState.AddModelError("DeliveryRequirement", MessageUtils.GetMessage("E0001", "希望納期"));
            }
            if (!ModelState.IsValidField("DeliveryRequirement")) {
                ModelState.AddModelError("DeliveryRequirement",MessageUtils.GetMessage("E0005","希望納期"));
            }
            if (ModelState.IsValidField("DeliveryRequirement") && header.DeliveryRequirement!=null ? header.DeliveryRequirement.Value.CompareTo(DateTime.Today) < 0 : true) {
                ModelState.AddModelError("DeliveryRequirement", MessageUtils.GetMessage("E0013", new string[] { "希望納期", "本日以降" }));
            }

        }

        #region 検索中アニメーション
        //Add 2014/12/22 arc nakayama IPO対応(顧客DM検索) 処理中対応
        /// <summary>
        /// 処理中かどうかを取得する。(Ajax専用）
        /// </summary>
        /// <param name="processType">処理種別</param>
        /// <returns>処理完了</returns>
        public ActionResult GetProcessed(string processType)
        {
            if (Request.IsAjaxRequest())
            {
                Dictionary<string, string> retSearch = new Dictionary<string, string>();

                retSearch.Add("ProcessedTime", "処理完了");

                return Json(retSearch);
            }
            return new EmptyResult();
        }

        #endregion

        /// <summary>
        /// 作業依頼画面を表示
        /// </summary>
        /// <param name="SlipNo">車両伝票番号</param>
        /// <returns>作業依頼入力画面</returns>
        public ActionResult Result(string SlipNo)
        {

            //リクエストされた伝票を取得
            //作業依頼
            ServiceRequest req = new ServiceRequestDao(db).GetBySlipNumber(SlipNo);

            //車両伝票
            CarSalesHeader header = new CarSalesOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
            req.CarSalesHeader = header;

            //車両発注依頼引当
            CarPurchaseOrder order = new CarPurchaseOrderDao(db).GetBySlipNumber(req.OriginalSlipNumber);
            req.CarPurchaseOrder = order;

            Department department = new DepartmentDao(db).GetByKey(req.DepartmentCode);
            ViewData["DepartmentName"] = department != null ? department.DepartmentName : "";
            return View("ServiceRequestResult", req);
        }
    }
}
