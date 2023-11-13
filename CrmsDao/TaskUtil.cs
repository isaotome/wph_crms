using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsDao;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CrmsDao
{
    /// <summary>
    /// タスクの作成用クラス
    /// </summary>
    public class TaskUtil {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// タスク作成者
        /// </summary>
        private Employee employee;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TaskUtil(CrmsLinqDataContext context, Employee createEmployee) {
            this.db = context;
            this.employee = createEmployee;
        }

        /// <summary>
        /// 共通のプロパティを設定する
        /// </summary>
        /// <returns>タスクオブジェクト</returns>
        private Task SetCommonProperties() {
            Task task = new Task();
            task.TaskId = Guid.NewGuid();
            task.CreateDate = DateTime.Now;
            task.CreateEmployeeCode = employee.EmployeeCode;
            task.LastUpdateDate = DateTime.Now;
            task.LastUpdateEmployeeCode = employee.EmployeeCode;
            task.DelFlag = "0";
            return task;
        }

        /// <summary>
        /// 車両発注承認依頼タスク作成
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarPurchaseApproval(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_PURCHASE_APPROVAL)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_PURCHASE_APPROVAL, header.DepartmentCode))) {

                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_PURCHASE_APPROVAL;
                task.SlipNumber = header.SlipNumber;
                task.NavigateUrl = "/CarSalesOrder/Confirm?SlipNo=" + header.SlipNumber;
                task.TaskName = "該当の伝票を表示して承認してください。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両受注速報を送信する
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void SendSalesOrderFlash(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_SALES_NEWS)) {
                return;
            }
            try {
                //インスタンス作成（POP Before SMTPならここでログイン)
                SendMail mail = new SendMail();
                foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_SALES_NEWS, header.DepartmentCode))) {

                    //メールアドレスが設定されていなければ送信しない
                    if (!string.IsNullOrEmpty(a.MailAddress)) {

                        //string msg = "";
                        //msg += "部門:" + header.Department.DepartmentName + "\r\n";
                        //msg += "担当者:" + header.Employee.EmployeeName + "\r\n";
                        //msg += "メーカー:" + header.MakerName + "\r\n";
                        //msg += "ブランド:" + header.CarBrandName + "\r\n";
                        //msg += "車種:" + header.CarName + "\r\n";
                        //msg += "グレード:" + header.CarGradeName + "\r\n";
                        //msg += "車両販売価格:" + string.Format("{0:N0}", header.GrandTotalAmount);

                        string title = "【SYSTEM Information】" + header.Department.DepartmentName + "受注速報";
                        string msg = "■受注\r\n";
                        msg += "受注日 : " + string.Format("{0:yyyy/MM/dd}", header.SalesOrderDate) + "\r\n";
                        msg += "担当者 : " + header.Department.DepartmentName + ":" + header.Employee.EmployeeName + "\r\n";
                        msg += "車種　 : " + header.MakerName + header.CarName + header.CarGradeName + "\r\n";
                        msg += "色　　 : " + header.ExteriorColorName + "/" + header.InteriorColorName + "\r\n";
                        msg += "車台No : " + header.Vin + "\r\n";
                        msg += "新中区 : " + header.c_NewUsedType.Name;

                        //メール送信処理
                        //ThreadPool.QueueUserWorkItem(x =>
                        mail.Send(title, a.MailAddress, msg);
                        //, null);
                    }
                }
            } catch (Exception e) {
                SmtpClient smtp = new SmtpClient();

                smtp.Host = "smtp.willplus.co.jp";
                smtp.Port = 25;
                smtp.Credentials = new NetworkCredential("sys_test@willplus.co.jp", "OmW1goiPKo");
                smtp.EnableSsl = true;
                MailMessage oMsg = new MailMessage("sysplus@willplus.co.jp", "sysplus@willplus.co.jp", "メール送信エラー発生", e.ToString());
                smtp.Send(oMsg);
            }
        }

        /// <summary>
        /// 車両引当依頼タスク作成
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarPurchaseOrderRequest(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_RESERVATION_REQUEST)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_RESERVATION_REQUEST, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_RESERVATION_REQUEST;
                task.SlipNumber = header.SlipNumber;
                task.TaskName = "車両受注が入りましたので車両引当を実施してください。";
                task.NavigateUrl = "/CarPurchaseOrder/ListEntry?OrderId=" + header.CarPurchaseOrder.CarPurchaseOrderNumber;
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両作業依頼タスク作成
        /// </summary>
        /// <param name="header">作業依頼データ</param>
        public void ServiceRequest(ServiceRequest header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_REQUEST)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_REQUEST, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.SERVICE_REQUEST;
                task.SlipNumber = header.OriginalSlipNumber;
                task.NavigateUrl = "/ServiceSalesOrder/Entry?OrgSlipNo=" + header.OriginalSlipNumber;
                task.TaskName = "営業から作業依頼が来ています。見積を作成して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両引当確認タスク作成
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        public void ReserveConfirm(CarPurchaseOrder order) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_RESERVATION_CONFIRM)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_RESERVATION_CONFIRM, order.CarSalesHeader.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = order.CarSalesHeader.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_RESERVATION_CONFIRM;
                task.SlipNumber = order.CarSalesHeader.SlipNumber;
                //task.NavigateUrl = "/CarSalesOrder/Entry/?SlipNo=";
                task.TaskName = "車両の引当が完了しました。引当結果を確認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両登録確認タスク作成
        /// </summary>
        /// <param name="order">車両発注依頼データ</param>
        public void RegistrationConfirm(CarPurchaseOrder order) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_REGISTRATION_CONFIRM)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_REGISTRATION_CONFIRM, order.CarSalesHeader.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = order.CarSalesHeader.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_REGISTRATION_CONFIRM;
                task.SlipNumber = order.CarSalesHeader.SlipNumber;
                task.TaskName = "車両の登録が完了しました。登録結果を確認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /*  //Del 2016/08/13 arc yano 現在未使用のため、削除
        /// <summary>
        /// 車両入庫予定タスク作成
        /// </summary>
        /// <param name="transfer">入出庫データ</param>
        public void CarReceiptPlan(Transfer transfer) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_RECEIPT_PLAN)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_RECEIPT_PLAN, transfer.ArrivalLocation.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = transfer.ArrivalLocation.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_RECEIPT_PLAN;
                task.SlipNumber = transfer.TransferNumber;
                task.NavigateUrl = "/CarTransfer/Entry?transferNo=" + transfer.TransferNumber;
                task.TaskName = "車両の入庫予定が作成されました。入庫処理を実施して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }
        */


        /// <summary>
        /// アフターフォロー（車両）タスク作成
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarAfterFollow(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_AFTER_FOLLOW)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_AFTER_FOLLOW, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_AFTER_FOLLOW;
                task.SlipNumber = header.SlipNumber;
                task.NavigateUrl = "/CarSalesOrder/Entry?SlipNo=" + header.SlipNumber;
                task.TaskName = "アフターフォローを実施して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両査定タスク作成
        /// </summary>
        /// <param name="appraisal">車両査定データ</param>
        public void CarAppraisal(CarAppraisal appraisal) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_APPRAISAL)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_APPRAISAL, appraisal.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = appraisal.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_APPRAISAL;
                task.SlipNumber = appraisal.SlipNumber;
                task.NavigateUrl = "/CarAppraisal/Entry/" + appraisal.CarAppraisalId;
                task.TaskName = "車両伝票から下取車査定データが作成されました。査定を実施して下さい";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両仕入予定タスク作成
        /// </summary>
        /// <param name="purchase">車両仕入データ</param>
        public void CarPurchasePlan(CarPurchase purchase) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_PURCHASE_PLAN)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_PURCHASE_PLAN, purchase.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = purchase.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_PURCHASE_PLAN;
                task.SlipNumber = purchase.CarPurchaseOrder!=null ? purchase.CarPurchaseOrder.SlipNumber : "";
                task.NavigateUrl = "/CarPurchase/Entry/" + purchase.CarPurchaseId;
                task.TaskName = "車両の仕入予定が作成されました。仕入を実施して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 車両キャンセル通知
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarCancel(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_CANCEL)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_CANCEL, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_CANCEL;
                task.SlipNumber = header.SlipNumber;
                task.TaskName = "車両伝票がキャンセルされました。";
                db.Task.InsertOnSubmit(task);
            }
        }


        /// <summary>
        /// 車両見積期限超過通知
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarQuoteExpire(CarSalesHeader header) {
        }

        /// <summary>
        /// 車両伝票過入金通知
        /// </summary>
        /// <param name="header">車両伝票データ</param>
        public void CarOverReceive(CarSalesHeader header,string customerClaimCode,decimal amount) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_OVER_RECEIVE)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_OVER_RECEIVE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = a.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_OVER_RECEIVE;
                task.TaskName = "車両伝票が受注後に修正され、過入金が発生しています。";
                task.Description = "請求先コード：" + customerClaimCode + "\r\n" + "金額：" + amount.ToString();
                task.SlipNumber = header.SlipNumber;
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// 部品発注承認タスク作成
        /// </summary>
        /// <param name="order"></param>
        public void PartsPurchaseApproval(PartsPurchaseOrder order) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.PARTS_PURCHASE_APPROVAL)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.PARTS_PURCHASE_APPROVAL, order.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = order.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.PARTS_PURCHASE_APPROVAL;
                task.TaskName = "該当伝票を表示して承認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 部品発注依頼タスク作成
        /// </summary>
        /// <param name="order"></param>
        public void PartsPurchaseOrderRequest(PartsPurchaseOrder order) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.PARTS_PURCHASE_REQUEST)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.PARTS_PURCHASE_REQUEST, order.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = order.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.PARTS_PURCHASE_REQUEST;
                task.SlipNumber = order.PurchaseOrderNumber;
                task.NavigateUrl = "/PartsPurchaseOrder/Entry/" + order.PurchaseOrderNumber;
                task.TaskName = "在庫が不足しているため、部品の発注データが自動作成されました。内容を確認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// 部品仕入予定タスク作成
        /// </summary>
        /// <param name="purchase"></param>
        public void PartsPurchasePlan(PartsPurchase purchase) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.PARTS_PURCHASE_PLAN)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.PARTS_PURCHASE_PLAN, purchase.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = purchase.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.PARTS_PURCHASE_PLAN;
                task.SlipNumber = purchase.PurchaseNumber;
                task.NavigateUrl = "/PartsPurchase/Entry/" + purchase.PurchaseNumber;
                task.TaskName = "部品の仕入予定が作成されました。仕入を実施して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /*  //Del 2016/08/13 arc yano 現在未使用のため、コメントアウト
        /// <summary>
        /// 部品入庫予定タスク作成
        /// </summary>
        /// <param name="transfer"></param>
        public void PartsReceiptPlan(Transfer transfer) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.PARTS_RECEIPT_PLAN)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.PARTS_RECEIPT_PLAN, transfer.ArrivalLocation.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = transfer.ArrivalLocation.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.PARTS_RECEIPT_PLAN;
                task.SlipNumber = transfer.TransferNumber;
                task.NavigateUrl = "/PartsTransfer/Entry/" + transfer.TransferNumber;
                task.TaskName = "部品の入庫予定が作成されました。入庫処理を行って下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }
        */

        /// <summary>
        /// サービス伝票修正確認タスク作成
        /// </summary>
        /// <param name="header"></param>
        public void ServiceSalesOrderChange(ServiceSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_SALES_CHANGE)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_SALES_CHANGE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.SlipNumber = header.SlipNumber;
                task.NavigateUrl = "/ServiceSalesOrder/Entry?SlipNo=" + header.SlipNumber;
                task.TaskName = "サービス伝票が修正されました。在庫引当、発注内容を確認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// サービスキャンセル通知
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        public void ServiceCancel(ServiceSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_CANCEL)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_CANCEL, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_CANCEL;
                task.SlipNumber = header.SlipNumber;
                task.TaskName = "サービス伝票がキャンセルされました。";
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// アフターフォロー（サービス）タスク作成
        /// </summary>
        /// <param name="header"></param>
        public void ServiceAfterFollow(ServiceSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_AFTER_FOLLOW)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_AFTER_FOLLOW, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.SERVICE_AFTER_FOLLOW;
                task.SlipNumber = header.SlipNumber;
                task.TaskName = "アフターフォローを実施して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// サービス伝票過入金通知
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        public void ServiceOverReceive(ServiceSalesHeader header, string customerClaimCode, decimal amount) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = a.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE;
                task.TaskName = "サービス伝票が受注後に修正され、過入金が発生しています。";
                task.Description = "請求先コード：" + customerClaimCode + "\r\n" + "金額：" + amount.ToString();
                task.SlipNumber = header.SlipNumber;
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// ホット期限超過タスク作成
        /// </summary>
        public void HotNotifier() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.HOT)) {
                return;
            }
            List<CarSalesHeader> targetList = new List<CarSalesHeader>();
            ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
            
            //AHOT
            targetList.AddRange(new CarSalesOrderDao(db).GetListByCondition(new CarSalesHeader { SalesOrderStatus = "001", HotStatus = "A", DelFlag = "0", QuoteDateTo = DateTime.Today.AddDays(int.Parse(dao.GetByKey("HOTAExpireDays").Value) * (-1)) }));

            //BHOT
            targetList.AddRange(new CarSalesOrderDao(db).GetListByCondition(new CarSalesHeader { SalesOrderStatus = "001", HotStatus = "B", DelFlag = "0", QuoteDateTo = DateTime.Today.AddDays(int.Parse(dao.GetByKey("HOTBExpireDays").Value) * (-1)) }));

            //CHOT
            targetList.AddRange(new CarSalesOrderDao(db).GetListByCondition(new CarSalesHeader { SalesOrderStatus = "001", HotStatus = "C", DelFlag = "0", QuoteDateTo = DateTime.Today.AddDays(int.Parse(dao.GetByKey("HOTCExpireDays").Value) * (-1)) }));

            foreach (var hot in targetList) {
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.HOT, hot.SlipNumber);
                //まだタスク追加されていない場合のみ追加する
                if (taskList == null || taskList.Count==0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.HOT, hot.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.DepartmentCode = a.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.TaskConfigId = DaoConst.TaskConfigId.HOT;
                        task.SlipNumber = hot.SlipNumber;
                        task.TaskName = "HOT期限を過ぎています";
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// 売掛金未回収タスク作成
        /// </summary>
        public void ReceiptExpired() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.RECEIPT_PLAN_EXPIRE)) {
                return;
            }
            string expiredDays = ConfigurationManager.AppSettings["ReceiptExpired"] ?? "7";
            List<ReceiptPlan> plan = new ReceiptPlanDao(db).GetExpiredList(DateTime.Today, int.Parse(expiredDays));
            foreach(var p in plan){
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.RECEIPT_PLAN_EXPIRE, p.SlipNumber);
                if (taskList == null || taskList.Count == 0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.RECEIPT_PLAN_EXPIRE, p.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.DepartmentCode = p.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.TaskConfigId = DaoConst.TaskConfigId.RECEIPT_PLAN_EXPIRE;
                        task.SlipNumber = p.SlipNumber;
                        task.TaskName = "入金予定を"+ expiredDays + "日以上経過している売掛金が存在します。入金確認を実施して下さい。";
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// 車検点検通知（営業）タスク作成
        /// </summary>
        public void CarInspectionNotifier() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_INSPECTION)) {
                return;
            }
            //車検まで3ヶ月切っている車両
            List<SalesCar> list3 = new SalesCarDao(db).GetListByCondition(new SalesCar { ExpireType = "001", ExpireDateFrom = DateTime.Today, ExpireDateTo = DateTime.Today.AddMonths(3) });
            var target3 =
                from a in list3
                where a.ExpireDate != null
                && a.Customer!=null && a.Customer.Employee!=null
                select a;

            foreach (var s in target3) {
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_INSPECTION, s.SalesCarNumber);
                if (taskList == null || taskList.Count==0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_INSPECTION,s.Customer.Employee.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.TaskConfigId = DaoConst.TaskConfigId.CAR_INSPECTION;
                        task.DepartmentCode = s.Customer.Employee.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.SlipNumber = s.SalesCarNumber;
                        task.TaskName = "車検まで3ヶ月を切っています";
                        task.Description = "管理番号:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }

            //車検まで6ヶ月切っている車両
            List<SalesCar> list6 = new SalesCarDao(db).GetListByCondition(new SalesCar { ExpireType = "001", ExpireDateFrom = DateTime.Today, ExpireDateTo = DateTime.Today.AddMonths(6) });
            
            //3ヶ月切っている車両を除外する
            var target6 =
                from a in list6
                where !(
                    from b in list3
                    select b.SalesCarNumber
                    ).Contains(a.SalesCarNumber)
                && a.NextInspectionDate!=null
                && a.Customer!=null && a.Customer.Employee!=null
                select a;

            foreach (var s in target6) {
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_INSPECTION, s.SalesCarNumber);
                if (taskList == null || taskList.Count==0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_INSPECTION, s.Customer.Employee.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.TaskConfigId = DaoConst.TaskConfigId.CAR_INSPECTION;
                        task.DepartmentCode = s.Customer.Employee.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.SlipNumber = s.SalesCarNumber;
                        task.TaskName = "車検まで6ヶ月を切っています";
                        task.Description = "管理番号:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// 車検点検通知（サービス）タスク作成
        /// </summary>
        public void ServiceInspectionNotifier() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_INSPECTION)) {
                return;
            }

            //次回点検日まで1ヶ月を切っている車両
            List<SalesCar> nextInspectionList = new SalesCarDao(db).GetListByCondition(new SalesCar { NextInspectionDateFrom = DateTime.Today, NextInspectionDateTo = DateTime.Today.AddMonths(1) });
            var target =
                from a in nextInspectionList
                where a.ExpireDate != null
                && a.Customer != null && a.Customer.Employee != null
                select a;

            foreach (var s in target) {
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_INSPECTION, s.SalesCarNumber);
                if (taskList == null || taskList.Count == 0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_INSPECTION, s.Customer.Employee.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.TaskConfigId = DaoConst.TaskConfigId.CAR_INSPECTION;
                        task.DepartmentCode = s.Customer.Employee.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.SlipNumber = s.SalesCarNumber;
                        task.TaskName = "次回点検日まで1ヶ月を切っています";
                        task.Description = "管理番号:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }


            //車検まで1ヶ月を切っている車両
            List<SalesCar> inspectionList = new SalesCarDao(db).GetListByCondition(new SalesCar { ExpireType = "001", ExpireDateFrom = DateTime.Today, ExpireDateTo = DateTime.Today.AddMonths(1) });
            
            //3ヶ月切っている車両を除外する
            var target2 =
                from a in inspectionList
                where !(
                    from b in nextInspectionList
                    select b.SalesCarNumber
                    ).Contains(a.SalesCarNumber)
                && a.NextInspectionDate != null
                && a.Customer != null && a.Customer.Employee != null
                select a;

            foreach (var s in target2) {
                List<Task> taskList = new TaskDao(db).GetListByIdAndSlipNumber(DaoConst.TaskConfigId.CAR_INSPECTION, s.SalesCarNumber);
                if (taskList == null || taskList.Count == 0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_INSPECTION, s.Customer.Employee.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.TaskConfigId = DaoConst.TaskConfigId.CAR_INSPECTION;
                        task.DepartmentCode = s.Customer.Employee.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.SlipNumber = s.SalesCarNumber;
                        task.TaskName = "車検まで1ヶ月を切っています";
                        task.Description = "管理番号:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// サービス伝票部品変更通知タスク作成
        /// </summary>
        /// <param name="header">サービス伝票</param>
        /// <param name="p">部品</param>
        /// <param name="quantity">変更された数量</param>
        public void PartsChange(ServiceSalesHeader header, Parts p, decimal? quantity) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.PARTS_CHANGE)) {
                return;
            }
            if(p == null || quantity == null || quantity == 0) {
                return;
            }
            
            foreach (var t in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.PARTS_CHANGE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = header.DepartmentCode;
                task.EmployeeCode = t.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.PARTS_CHANGE;
                task.SlipNumber = header.SlipNumber;
                task.NavigateUrl = "/ServiceSalesOrder/Entry?SlipNo=" + header.SlipNumber;

                string msg = "部品番号:" + p.PartsNumber + "\r\n"
                            + "部品名:" + p.PartsNameJp + "\r\n"
                            + "の数量が" + quantity + "変更されました。";

                task.Description = msg + "\r\n" + "引当内容、発注を確認して下さい。";
                db.Task.InsertOnSubmit(task);
            }
        }
    }
}
