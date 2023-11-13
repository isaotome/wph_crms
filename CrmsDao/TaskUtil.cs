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
    /// �^�X�N�̍쐬�p�N���X
    /// </summary>
    public class TaskUtil {
        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// �^�X�N�쐬��
        /// </summary>
        private Employee employee;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TaskUtil(CrmsLinqDataContext context, Employee createEmployee) {
            this.db = context;
            this.employee = createEmployee;
        }

        /// <summary>
        /// ���ʂ̃v���p�e�B��ݒ肷��
        /// </summary>
        /// <returns>�^�X�N�I�u�W�F�N�g</returns>
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
        /// �ԗ��������F�˗��^�X�N�쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
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
                task.TaskName = "�Y���̓`�[��\�����ď��F���Ă��������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ��󒍑���𑗐M����
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        public void SendSalesOrderFlash(CarSalesHeader header) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_SALES_NEWS)) {
                return;
            }
            try {
                //�C���X�^���X�쐬�iPOP Before SMTP�Ȃ炱���Ń��O�C��)
                SendMail mail = new SendMail();
                foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_SALES_NEWS, header.DepartmentCode))) {

                    //���[���A�h���X���ݒ肳��Ă��Ȃ���Α��M���Ȃ�
                    if (!string.IsNullOrEmpty(a.MailAddress)) {

                        //string msg = "";
                        //msg += "����:" + header.Department.DepartmentName + "\r\n";
                        //msg += "�S����:" + header.Employee.EmployeeName + "\r\n";
                        //msg += "���[�J�[:" + header.MakerName + "\r\n";
                        //msg += "�u�����h:" + header.CarBrandName + "\r\n";
                        //msg += "�Ԏ�:" + header.CarName + "\r\n";
                        //msg += "�O���[�h:" + header.CarGradeName + "\r\n";
                        //msg += "�ԗ��̔����i:" + string.Format("{0:N0}", header.GrandTotalAmount);

                        string title = "�ySYSTEM Information�z" + header.Department.DepartmentName + "�󒍑���";
                        string msg = "����\r\n";
                        msg += "�󒍓� : " + string.Format("{0:yyyy/MM/dd}", header.SalesOrderDate) + "\r\n";
                        msg += "�S���� : " + header.Department.DepartmentName + ":" + header.Employee.EmployeeName + "\r\n";
                        msg += "�Ԏ�@ : " + header.MakerName + header.CarName + header.CarGradeName + "\r\n";
                        msg += "�F�@�@ : " + header.ExteriorColorName + "/" + header.InteriorColorName + "\r\n";
                        msg += "�ԑ�No : " + header.Vin + "\r\n";
                        msg += "�V���� : " + header.c_NewUsedType.Name;

                        //���[�����M����
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
                MailMessage oMsg = new MailMessage("sysplus@willplus.co.jp", "sysplus@willplus.co.jp", "���[�����M�G���[����", e.ToString());
                smtp.Send(oMsg);
            }
        }

        /// <summary>
        /// �ԗ������˗��^�X�N�쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
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
                task.TaskName = "�ԗ��󒍂�����܂����̂Ŏԗ����������{���Ă��������B";
                task.NavigateUrl = "/CarPurchaseOrder/ListEntry?OrderId=" + header.CarPurchaseOrder.CarPurchaseOrderNumber;
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ���ƈ˗��^�X�N�쐬
        /// </summary>
        /// <param name="header">��ƈ˗��f�[�^</param>
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
                task.TaskName = "�c�Ƃ����ƈ˗������Ă��܂��B���ς��쐬���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ������m�F�^�X�N�쐬
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
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
                task.TaskName = "�ԗ��̈������������܂����B�������ʂ��m�F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ��o�^�m�F�^�X�N�쐬
        /// </summary>
        /// <param name="order">�ԗ������˗��f�[�^</param>
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
                task.TaskName = "�ԗ��̓o�^���������܂����B�o�^���ʂ��m�F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /*  //Del 2016/08/13 arc yano ���ݖ��g�p�̂��߁A�폜
        /// <summary>
        /// �ԗ����ɗ\��^�X�N�쐬
        /// </summary>
        /// <param name="transfer">���o�Ƀf�[�^</param>
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
                task.TaskName = "�ԗ��̓��ɗ\�肪�쐬����܂����B���ɏ��������{���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }
        */


        /// <summary>
        /// �A�t�^�[�t�H���[�i�ԗ��j�^�X�N�쐬
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
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
                task.TaskName = "�A�t�^�[�t�H���[�����{���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ�����^�X�N�쐬
        /// </summary>
        /// <param name="appraisal">�ԗ�����f�[�^</param>
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
                task.TaskName = "�ԗ��`�[���牺��ԍ���f�[�^���쐬����܂����B��������{���ĉ�����";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ��d���\��^�X�N�쐬
        /// </summary>
        /// <param name="purchase">�ԗ��d���f�[�^</param>
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
                task.TaskName = "�ԗ��̎d���\�肪�쐬����܂����B�d�������{���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �ԗ��L�����Z���ʒm
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
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
                task.TaskName = "�ԗ��`�[���L�����Z������܂����B";
                db.Task.InsertOnSubmit(task);
            }
        }


        /// <summary>
        /// �ԗ����ϊ������ߒʒm
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        public void CarQuoteExpire(CarSalesHeader header) {
        }

        /// <summary>
        /// �ԗ��`�[�ߓ����ʒm
        /// </summary>
        /// <param name="header">�ԗ��`�[�f�[�^</param>
        public void CarOverReceive(CarSalesHeader header,string customerClaimCode,decimal amount) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_OVER_RECEIVE)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.CAR_OVER_RECEIVE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = a.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.CAR_OVER_RECEIVE;
                task.TaskName = "�ԗ��`�[���󒍌�ɏC������A�ߓ������������Ă��܂��B";
                task.Description = "������R�[�h�F" + customerClaimCode + "\r\n" + "���z�F" + amount.ToString();
                task.SlipNumber = header.SlipNumber;
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// ���i�������F�^�X�N�쐬
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
                task.TaskName = "�Y���`�[��\�����ď��F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// ���i�����˗��^�X�N�쐬
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
                task.TaskName = "�݌ɂ��s�����Ă��邽�߁A���i�̔����f�[�^�������쐬����܂����B���e���m�F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// ���i�d���\��^�X�N�쐬
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
                task.TaskName = "���i�̎d���\�肪�쐬����܂����B�d�������{���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /*  //Del 2016/08/13 arc yano ���ݖ��g�p�̂��߁A�R�����g�A�E�g
        /// <summary>
        /// ���i���ɗ\��^�X�N�쐬
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
                task.TaskName = "���i�̓��ɗ\�肪�쐬����܂����B���ɏ������s���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }
        */

        /// <summary>
        /// �T�[�r�X�`�[�C���m�F�^�X�N�쐬
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
                task.TaskName = "�T�[�r�X�`�[���C������܂����B�݌Ɉ����A�������e���m�F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }

        /// <summary>
        /// �T�[�r�X�L�����Z���ʒm
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
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
                task.TaskName = "�T�[�r�X�`�[���L�����Z������܂����B";
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// �A�t�^�[�t�H���[�i�T�[�r�X�j�^�X�N�쐬
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
                task.TaskName = "�A�t�^�[�t�H���[�����{���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// �T�[�r�X�`�[�ߓ����ʒm
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[�f�[�^</param>
        public void ServiceOverReceive(ServiceSalesHeader header, string customerClaimCode, decimal amount) {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE)) {
                return;
            }
            foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE, header.DepartmentCode))) {
                Task task = SetCommonProperties();
                task.DepartmentCode = a.DepartmentCode;
                task.EmployeeCode = a.EmployeeCode;
                task.TaskConfigId = DaoConst.TaskConfigId.SERVICE_OVER_RECEIVE;
                task.TaskName = "�T�[�r�X�`�[���󒍌�ɏC������A�ߓ������������Ă��܂��B";
                task.Description = "������R�[�h�F" + customerClaimCode + "\r\n" + "���z�F" + amount.ToString();
                task.SlipNumber = header.SlipNumber;
                db.Task.InsertOnSubmit(task);
            }
        }
        /// <summary>
        /// �z�b�g�������߃^�X�N�쐬
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
                //�܂��^�X�N�ǉ�����Ă��Ȃ��ꍇ�̂ݒǉ�����
                if (taskList == null || taskList.Count==0) {
                    foreach (var a in (new TaskConfigDao(db).GetEmployeeByKey(DaoConst.TaskConfigId.HOT, hot.DepartmentCode))) {
                        Task task = SetCommonProperties();
                        task.DepartmentCode = a.DepartmentCode;
                        task.EmployeeCode = a.EmployeeCode;
                        task.TaskConfigId = DaoConst.TaskConfigId.HOT;
                        task.SlipNumber = hot.SlipNumber;
                        task.TaskName = "HOT�������߂��Ă��܂�";
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// ���|��������^�X�N�쐬
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
                        task.TaskName = "�����\���"+ expiredDays + "���ȏ�o�߂��Ă��锄�|�������݂��܂��B�����m�F�����{���ĉ������B";
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// �Ԍ��_���ʒm�i�c�Ɓj�^�X�N�쐬
        /// </summary>
        public void CarInspectionNotifier() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_INSPECTION)) {
                return;
            }
            //�Ԍ��܂�3�����؂��Ă���ԗ�
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
                        task.TaskName = "�Ԍ��܂�3������؂��Ă��܂�";
                        task.Description = "�Ǘ��ԍ�:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }

            //�Ԍ��܂�6�����؂��Ă���ԗ�
            List<SalesCar> list6 = new SalesCarDao(db).GetListByCondition(new SalesCar { ExpireType = "001", ExpireDateFrom = DateTime.Today, ExpireDateTo = DateTime.Today.AddMonths(6) });
            
            //3�����؂��Ă���ԗ������O����
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
                        task.TaskName = "�Ԍ��܂�6������؂��Ă��܂�";
                        task.Description = "�Ǘ��ԍ�:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// �Ԍ��_���ʒm�i�T�[�r�X�j�^�X�N�쐬
        /// </summary>
        public void ServiceInspectionNotifier() {
            if(!new TaskConfigDao(db).TaskEnabled(DaoConst.TaskConfigId.CAR_INSPECTION)) {
                return;
            }

            //����_�����܂�1������؂��Ă���ԗ�
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
                        task.TaskName = "����_�����܂�1������؂��Ă��܂�";
                        task.Description = "�Ǘ��ԍ�:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }


            //�Ԍ��܂�1������؂��Ă���ԗ�
            List<SalesCar> inspectionList = new SalesCarDao(db).GetListByCondition(new SalesCar { ExpireType = "001", ExpireDateFrom = DateTime.Today, ExpireDateTo = DateTime.Today.AddMonths(1) });
            
            //3�����؂��Ă���ԗ������O����
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
                        task.TaskName = "�Ԍ��܂�1������؂��Ă��܂�";
                        task.Description = "�Ǘ��ԍ�:" + s.SalesCarNumber;
                        db.Task.InsertOnSubmit(task);
                    }
                }
            }
        }

        /// <summary>
        /// �T�[�r�X�`�[���i�ύX�ʒm�^�X�N�쐬
        /// </summary>
        /// <param name="header">�T�[�r�X�`�[</param>
        /// <param name="p">���i</param>
        /// <param name="quantity">�ύX���ꂽ����</param>
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

                string msg = "���i�ԍ�:" + p.PartsNumber + "\r\n"
                            + "���i��:" + p.PartsNameJp + "\r\n"
                            + "�̐��ʂ�" + quantity + "�ύX����܂����B";

                task.Description = msg + "\r\n" + "�������e�A�������m�F���ĉ������B";
                db.Task.InsertOnSubmit(task);
            }
        }
    }
}
