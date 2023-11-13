using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using CrmsReport;
using GrapeCity.ActiveReports.Export.Pdf.Section;
using System.Configuration;
 
using CrmsDao;
using System.IO;
//using DataDynamics.ActiveReports;
using System.Threading;
using System.Collections;
using GrapeCity.ActiveReports;
using Crms.Models;                      //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�

namespace Crms.Controllers
{
    /// <summary>
    /// ���[�o�͗p�R���g���[��
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ReportController : Controller
    {
        //Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�ׂ͂̈ɒǉ�
        private static readonly string FORM_NAME = "���[�o��";     // ��ʖ�
        private static readonly string PROC_NAME = "�o��"; // ������

        /// <summary>
        /// �f�[�^�R���e�L�X�g
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ActiveReport�p�f�[�^�\�[�X
        /// </summary>


        /// <summary>
        /// �R���X�g���N�^
        /// �ڑ��������ݒ�t�@�C������擾���ăf�[�^�\�[�X�ɃZ�b�g
        /// �^�C���A�E�g��30�b�ŃZ�b�g
        /// </summary>
        public ReportController()
        {
            source = new GrapeCity.ActiveReports.Data.SqlDBDataSource();
            source.ConnectionString = ConfigurationManager.ConnectionStrings["CrmsDao.Properties.Settings.CRMSConnectionString"].ToString();
            source.CommandTimeout = 30;

            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// �S���҂������̂��镔�傩�ǂ����𔻒肷��
        /// </summary>
        /// <param name="employee">�S����</param>
        /// <param name="department">����</param>
        /// <returns>�{���\���ǂ���</returns>
        private bool CanViewData(Employee employee, Department department)
        {
            if (employee == null) return false;
            switch (employee.SecurityRole.SecurityLevelCode)
            {
                case "001":
                    if (employee.DepartmentCode.Equals(department.DepartmentCode) ||
                        employee.DepartmentCode1.Equals(department.DepartmentCode) ||
                        employee.DepartmentCode2.Equals(department.DepartmentCode) ||
                        employee.DepartmentCode3.Equals(department.DepartmentCode))
                    {
                        return true;
                    }
                    break;
                case "002":
                    if (employee.Department1.OfficeCode.Equals(department.OfficeCode) ||
                        (employee.AdditionalDepartment1 != null && employee.AdditionalDepartment1.OfficeCode.Equals(department.OfficeCode)) ||
                        (employee.AdditionalDepartment2 != null && employee.AdditionalDepartment2.OfficeCode.Equals(department.OfficeCode)) ||
                        (employee.AdditionalDepartment3 != null && employee.AdditionalDepartment3.OfficeCode.Equals(department.OfficeCode)))
                    {
                        return true;
                    }
                    break;
                case "003":
                    if (employee.Department1.Office.CompanyCode.Equals(department.Office.CompanyCode) ||
                        (employee.AdditionalDepartment1 != null && employee.AdditionalDepartment1.Office.CompanyCode.Equals(department.Office.CompanyCode)) ||
                        (employee.AdditionalDepartment2 != null && employee.AdditionalDepartment2.Office.CompanyCode.Equals(department.Office.CompanyCode)) ||
                        (employee.AdditionalDepartment3 != null && employee.AdditionalDepartment3.Office.CompanyCode.Equals(department.Office.CompanyCode)))
                    {
                        return true;
                    }
                    break;
                case "004":
                    return true;
            }
            return false;
        }
        /// <summary>
        /// ���[�o�͗p�����
        /// </summary>
        /// <param name="id">���[�̎��</param>
        /// <returns>���VIEW</returns>
        /// <history>
        /// 2021/03/22 yano #4078�y�T�[�r�X�`�[���́z�[�Ԋm�F���ŏo�͂��钠�[�̎�ނ𓮓I�ɍi��
        /// 2017/01/21 arc yano #3657 �����ɂ��ڋq�̌l���̕\���^��\�����s���悤�ɏC��
        /// </history>
        public ActionResult Print()
        {
            //�������w�肳��Ă��Ȃ���Β��~
            if (string.IsNullOrEmpty(Request["reportName"]) || string.IsNullOrEmpty(Request["reportParam"])) return new EmptyResult();

            string id = Request["reportName"];
            string[] param = Request["reportParam"].Split(',');
            Employee employee = (Employee)Session["Employee"];

            //Add 2017/01/21 arc yano #3657
            bool dispPersonalInfo = Request["dispPersonalInfo"] != null ? bool.Parse(Request["dispPersonalInfo"]) : false;

            //Add 2021/03/22 yano #4078
            bool claimReportOutPut = Request["claimReportOutPut"] != null ? bool.Parse(Request["claimReportOutPut"]) : false;

            //Add 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            FileContentResult result = null;
            object target;
            // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
            string reportName = "";

            // Add 2014/08/07 arc amii �G���[���O�Ή� SQL�����擾���鏈���ǉ�
            db.Log = new OutputWriter();

            try
            {
                //string inventoryMonth = "";
                switch (id)
                {
                    //�ԗ����Ϗ�
                    case "CarQuote":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�ԗ����Ϗ�";

                        //�{���������Ȃ���Β��~
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarQuoteReport(param[0], param[1], dispPersonalInfo);      //Mod 2017/01/21 arc yano #3657
                        break;

                    //�ԗ�������
                    case "CarSalesOrder":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�ԗ�������";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarSalesOrderReport(param[0], param[1]);
                        break;

                    //�ԗ��o�^�˗���
                    case "CarRegistRequest":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�ԗ��o�^�˗���";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1] ?? "0"));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/08/14 arc amii �G���[���O�Ή� null������h���ׁACommonUtils.DefaultString��ǉ�
                        if (((CarSalesHeader)target).Customer != null &&
                                (CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("103") ||
                                    CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("102") ||
                                    CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("101")))
                        {
                            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                            result = PrintCarOwnregistReport(param[0], param[1]);
                        }
                        else
                        {
                            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                            result = PrintCarRegistReport(param[0], param[1]);
                        }
                        break;

                    //�[�Ԋm�F��
                    case "CarDeliveryReport":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�[�Ԋm�F��";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1] ?? "0"));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarDeliveryReport(param[0], param[1]);
                        break;
                    /*
                                                  //�ԗ��I�����[
                                                  case "CarInventorySrc":
                                                      // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                                                      reportName = "�ԗ��I�����[";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintCarInventorySrcReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //�ԗ��I���덷�\
                                                  case "CarInventoryDiff":
                                                      // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                                                      reportName = "�ԗ��I���덷�\";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");

                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintCarInventoryDiffReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //���i�I�����[
                                                  case "PartsInventorySrc":
                                                      // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                                                      reportName = "���i�I�����[";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintPartsInventorySrcReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //���i�I���덷�\
                                                  case "PartsInventoryDiff":
                                                      // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                                                      reportName = "���i�I���덷�\";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintPartsInventoryDiffReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;
                    */
                    //��Ǝw����
                    case "ServiceInstruction":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "��Ǝw����";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintServiceInstructionReport(param[0], param[1]);
                        break;

                    //�T�[�r�X����
                    case "ServiceQuote":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�T�[�r�X����";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintServiceQuoteReport(param[0], param[1], dispPersonalInfo);     //Mod 2017/01/21 arc yano #3657
                        break;

                    //����`�[
                    case "ServiceSales":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "����`�[";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintServiceSalesReport(param[0], param[1], claimReportOutPut); //Mod 2021/03/22 yano #4078 
                                                                                                 //result = PrintServiceSalesReport(param[0], param[1]);
                        break;

                    //�O���˗���
                    case "OutSourceRequest":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�O���˗���";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintOutSourceRequestReport(param[0], param[1]);
                        break;

                    //���i�����˗���
                    case "PartsPurchaseOrderRequest":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "���i�����˗���";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintPartsPurchaseOrderRequestReport(param[0], param[1]);
                        break;
                    //������
                    case "Invoice":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "������";
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintInvoiceReport(param);
                        break;
                    // ���ɘA���[
                    case "CarArrival":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "���ɘA���[";
                        target = new CarPurchaseDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarPurchase)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarArrivalReport(param[0]);
                        break;
                    // ����[
                    case "CarAppraisal":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "����[";
                        target = new CarAppraisalDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarAppraisal)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarAppraisalReport(param[0]);
                        break;
                    // �ԗ�����_��
                    case "CarPurchaseAgreement":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�ԗ�����_��";
                        target = new CarAppraisalDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarAppraisal)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintCarPurchaseAgreementReport(param[0]);
                        break;
                    // �T�[�r�X���菑
                    case "ServiceReceiption":
                        // Add 2014/08/07 arc amii �G���[���O�Ή� ���O�o�͂̏�����(���[��)���擾����
                        reportName = "�T�[�r�X���菑";
                        target = new CustomerReceiptionDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CustomerReceiption)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
                        result = PrintServiceReceiptionReport(param[0]);
                        break;
                    //�ԗ���ƈ˗���
                    case "ServiceRequest":
                        reportName = "��ƈ˗���";
                        result = PrintServiceRequestReport(param[0]);
                        break;
                    default:
                        break;

                }
            }
            catch (GrapeCity.ActiveReports.ReportException re)
            {
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ���O�ɏo��
                OutputLogger.NLogError(re, reportName + PROC_NAME, FORM_NAME, "");
                // �G���[�y�[�W�ɑJ��
                return View("Error");
            }
            catch (Exception e)
            {
                // �Z�b�V������SQL����o�^
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ���O�ɏo��
                OutputLogger.NLogFatal(e, reportName + PROC_NAME, FORM_NAME, "");

                // �G���[�y�[�W�ɑJ��
                return View("Error");
            }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή��@������pdf�t�@�C����Ԃ��悤�ɐݒ肷��B
            return result;
            //return new EmptyResult();
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// �T�[�r�X���菑���������
        /// </summary>
        /// <param name="receiptionId"></param>
        private FileContentResult PrintServiceReceiptionReport(string receiptionId)
        {
            string sql = "select * from V_ServiceReceiptionReport where CarReceiptionId='" + receiptionId + "'";
            source.SQL = sql;
            ServiceReceiptionReport report = new ServiceReceiptionReport();
            report.DataSource = source;
            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, "�T�[�r�X���菑", "�T�[�r�X���菑");

        }

        /// <summary>
        /// ���q����_�񏑂��������
        /// </summary>
        /// <param name="p"></param>
        /// <history>
        ///  2018/12/21 yano #3965 WE�ŐV�V�X�e���Ή��iWeb.config�ɂ�鏈���̕���) �ڑ���DB�ɂ��A�Ж����S�̎擾���S��ύX����B
        ///  2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// </history>
        private FileContentResult PrintCarPurchaseAgreementReport(string carAppraisalId)
        {
            //Add 2018/12/21 yano #3965
            string filePath = db.Logo.Where(x => ("CarPurchaseAgreementReport").Equals(x.LogoName)).Select(x => x.FilePath).FirstOrDefault();


            string sql = "select * from V_CarPurchaseAgreementReport where CarAppraisalId='" + carAppraisalId + "'";
            source.SQL = sql;
            CarPurchaseAgreementReport report = new CarPurchaseAgreementReport(filePath);  //Mod 2018/12/21 yano #3965
            report.HikaeName = "�i���q�l�T���j";
            report.DataSource = source;

            CarPurchaseAgreementReport_ura ura = new CarPurchaseAgreementReport_ura();

            CarPurchaseAgreementReport report2 = new CarPurchaseAgreementReport(filePath); //Mod 2018/12/21 yano #3965
            report2.HikaeName = "�i�X�܍T���j";
            report2.DataSource = source;

            try
            {
                report.Run();
                report2.Run();
                ura.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            report.Document.Pages.AddRange(ura.Document.Pages);

            // �X�܍T��
            report.Document.Pages.AddRange(report2.Document.Pages);
            report.Document.Pages.AddRange(ura.Document.Pages);

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, "���q����_��", "���q����_��");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ����[���������
        /// </summary>
        /// <param name="carAppraisalId"></param>
        private FileContentResult PrintCarAppraisalReport(string carAppraisalId)
        {
            string sql = "select * from V_CarAppraisalReport where CarAppraisalId='" + carAppraisalId + "'";
            source.SQL = sql;
            CarAppraisalReport report = new CarAppraisalReport();
            report.DataSource = source;
            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException) { }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, "����[", "����[");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ���ɘA���[���������
        /// </summary>
        /// <param name="carPurchaseId"></param>
        private FileContentResult PrintCarArrivalReport(string carPurchaseId)
        {
            string sql = "select * from V_CarArrivalReport where CarPurchaseId='" + carPurchaseId + "'";
            source.SQL = sql;
            CarArrivalReport report = new CarArrivalReport();
            report.DataSource = source;
            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException)
            {
            }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, "���ɘA���[", "���ɘA���[");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// �����m�F�����������
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="revisionNumber"></param>
        private FileContentResult PrintCarReceiptReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_CarReceiptReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;

            CarReceiptReport report = new CarReceiptReport();
            report.DataSource = source;
            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException)
            {

            }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, slipNumber + "_" + revisionNumber, "�����m�F��");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ���������������
        /// </summary>
        /// <param name="keyList"></param>
        private FileContentResult PrintInvoiceReport(string[] keyList)
        {
            Invoice invoice = new Invoice();
            List<ReceiptPlan> invoiceList = new List<ReceiptPlan>();
            foreach (var key in keyList)
            {
                Guid guid = new Guid(key);
                ReceiptPlan plan = new ReceiptPlanDao(db).GetByKey(guid);
                //TODO:����������t���O�X�V�������L�q

                //�������Ώۃ��X�g�ɒǉ�
                invoiceList.Add(plan);
            }

            //������ʂɏW��
            var query =
                from a in invoiceList
                group a by a.CustomerClaimCode into claim
                select claim;

            //�����悲�Ƃɐ����f�[�^�𒊏o
            foreach (var data in query)
            {

                var list =
                    from a in invoiceList
                    where a.CustomerClaimCode.Equals(data.Key)
                    select a;
                string sql = "select * from V_InvoiceReport where ReceiptPlanId in (";

                List<string> targetIdArray = new List<string>();
                foreach (var target in list)
                {
                    targetIdArray.Add("'" + target.ReceiptPlanId.ToString() + "'");
                }
                sql += string.Join(",", targetIdArray.ToArray()) + ") and CustomerClaimCode='" + data.Key + "' order by SlipNumber";

                source.SQL = sql;
                Invoice inv = new Invoice();
                inv.DataSource = source;

                try
                {
                    inv.Run(false);
                }
                catch (GrapeCity.ActiveReports.ReportException)
                {
                }
                invoice.Document.Pages.AddRange(inv.Document.Pages);
            }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(invoice.Document, "Invoice", "������");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ���i�����˗������������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        private FileContentResult PrintPartsPurchaseOrderRequestReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_PartsPurchaseOrderReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;

            PartsPurchaseOrderReport order = new PartsPurchaseOrderReport();
            order.DataSource = source;

            try
            {
                order.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(order.Document, slipNumber + "_" + revisionNumber, "���i�����˗���");

        }

        /// <summary>
        /// �O���˗������������
        /// </summary>
        /// <param name="SlipNumber">�`�[�ԍ�</param>
        /// <param name="RevisionNumber">�����ԍ�</param>
        /// <history>
        /// 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// </history>
        private FileContentResult PrintOutSourceRequestReport(string slipNumber, string revisionNumber)
        {
            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetByKey(slipNumber, int.Parse(revisionNumber));
            var line = from a in header.ServiceSalesLine
                       where !string.IsNullOrEmpty(a.SupplierCode) && a.Supplier.OutsourceFlag.Equals("1")
                       group a by a.SupplierCode into sup
                       select sup;
            OutSourceRequestReport outSource = new OutSourceRequestReport();

            foreach (var target in line)
            {
                OutSourceRequestReport report = new OutSourceRequestReport();
                string sql = "select * from V_ServiceSalesReport where SlipNumber='" + slipNumber + "'";
                if (revisionNumber == null)
                {
                    sql += " and DelFlag='0'";
                }
                else
                {
                    sql += " and RevisionNumber=" + revisionNumber;
                }
                sql += " and SupplierCode='" + target.Key + "'";

                source.SQL = sql;
                report.DataSource = source;

                try
                {
                    report.Run(false);
                }
                catch (GrapeCity.ActiveReports.ReportException) { }

                outSource.Document.Pages.AddRange(report.Document.Pages);
                report.Dispose();
            }

            //�[�����̏ꍇ�A��̃f�[�^���Z�b�g
            if (outSource.Document.Pages.Count == 0)
            {
                OutSourceRequestReport report = new OutSourceRequestReport();
                string sql = "select * from V_ServiceSalesReport where 0=1";
                source.SQL = sql;
                report.DataSource = source;
                report.Run(false);
                outSource.Document.Pages.AddRange(report.Document.Pages);
            }

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(outSource.Document, slipNumber + "_" + revisionNumber, "�O���˗���");

        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// �ԗ��[�Ԋm�F�����������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        private FileContentResult PrintCarDeliveryReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_CarSalesReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;
            CarDeliveryReport carDeliveryReport1 = new CarDeliveryReport();
            carDeliveryReport1.SlipNumber = slipNumber;
            carDeliveryReport1.RevisionNumber = revisionNumber;
            carDeliveryReport1.HikaeTitle = "���q�l�T��";
            carDeliveryReport1.DataSource = source;

            CarDeliveryReport carDeliveryReport2 = new CarDeliveryReport();
            carDeliveryReport2.SlipNumber = slipNumber;
            carDeliveryReport2.RevisionNumber = revisionNumber;
            carDeliveryReport2.HikaeTitle = "��ЍT��";
            carDeliveryReport2.DataSource = source;

            try
            {
                carDeliveryReport1.Run(false);
                carDeliveryReport2.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            carDeliveryReport1.Document.Pages.AddRange(carDeliveryReport2.Document.Pages);

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(carDeliveryReport1.Document, slipNumber + "_" + revisionNumber, "�ԗ��[�Ԋm�F��");

        }

        /// <summary>
        /// �T�[�r�X���Ϗ����������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        /// <history>
        /// 2023/06/08 openwave #4141�y�T�[�r�X�`�[���́z�������֘A�̏C��
        /// 2020/06/08 yano #3665�y�T�[�r�X�z�T�[�r�X�`�[�̌��ς���֐U������
        /// 2020/02/17 yano #4025�y�T�[�r�X�`�[�z��ږ��Ɏd��ł���悤�ɋ@�\�ǉ�
        /// 2019/08/30 yano #3976 �T�[�r�X�`�[���́@��t�S���̕����ύX
        /// 2017/01/21 arc yano #3657 �����ɂ��ڋq�̌l���̕\���^��\�����s���悤�ɏC��
        /// 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// </history>
        private FileContentResult PrintServiceQuoteReport(string slipNumber, string revisionNumber, bool dispPersonalInfo = false)
        {

            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetByKey(slipNumber, int.Parse(revisionNumber));

            ServiceQuoteReport report = new ServiceQuoteReport();

            //Mod 2023/05/01 openwave #4141
            //var query =
            //  from a in header.ServiceSalesLine
            //  group a by a.CustomerClaimCode into c
            //  select new
            //  {
            //    c.Key
            //  };
            //var query =
            //  (from l in db.ServiceSalesLine
            //   where l.SlipNumber.Equals(slipNumber) && l.RevisionNumber.Equals(revisionNumber)
            //   &&    l.CustomerClaimCode != null
            //   select l.CustomerClaimCode)
            //  .Union
            //  (from h in db.ServiceSalesHeader
            //   where h.SlipNumber.Equals(slipNumber) && h.RevisionNumber.Equals(revisionNumber)
            //   &&    h.CustomerClaimCode != null
            //   select h.CustomerClaimCode)
            //;
            var query =
              from a in db.V_ServiceSalesClaimCode
              where a.SlipNumber.Equals(slipNumber) && a.RevisionNumber.Equals(revisionNumber)
              select a.CustomerClaimCode
              ;

            //if (query.Count() == 1 && header.ServiceSalesLine.Where(x => x.CustomerClaimCode.Equals(header.CustomerCode)).Count() > 0)
            if (query.Count() == 1)
            {
                //Mod2017/01/21 arc yano #3657
                string sql = "select ";

                if (dispPersonalInfo) //�l����\������ꍇ    
                {

                    sql += "* ";
                }
                else
                {
                    sql += " SlipNumber, RevisionNumber, LineNumber, CompanyName, OfficeName, OfficeFullName, DepartmentCode, DepartmentName, DepartmentFullName";
                    sql += ", DepartmentPostCode, DepartmentPrefecture, DepartmentCity, DepartmentAddress1, DepartmentAddress2, DepartmentTelNumber1, DepartmentFaxNumber";
                    sql += ", CustomerCode, CustomerName, CustomerPostCode, CustomerPrefecture, CustomerCity, CustomerAddress1, CustomerAddress2, CustomerTelNumber, CustomerMobileNumber";
                    sql += ", SalesDate, ReceiptionEmployeeName, FrontEmployeeName, CarName, Mileage, MileageUnit, EngineType, FirstRegistration, NextInspectionDate, ModelName, Vin";
                    sql += ", ClassificationTypeNumber, ServiceWorkCode, LineContents, LineContents2, WorkType, TechnicalFeeAmount, Quantity, Price, Amount";
                    sql += ", SalesPlanDate, ConsumptionTaxId, Rate, EngineerEmployeeName, CostTotalAmount, CarBrandName, ArrivalPlanDate, SupplierCode, SupplierName, TaxTotalAmount, EngineerTotalAmount, PartsTotalAmount";
                    sql += ", CustomerClaimCode, CarEmployeeName";      //Mod 2019/08/30 yano #3976
                    sql += ", '' AS CustomerClaimName";
                    sql += ", '' AS CustomerClaimPostCode";
                    sql += ", '' AS CustomerClaimPrefecture";
                    sql += ", '' AS CustomerClaimCity";
                    sql += ", '' AS CustomerClaimAddress1";
                    sql += ", '' AS CustomerClaimAddress2";
                    sql += ", '' AS CustomerClaimTelNumber1";
                    sql += ", '' AS CustomerClaimTelNumber2";
                    sql += ", CustomerClaimFaxNumber";
                    sql += ", DisablePriceFlag, CarLiabilityInsurance, CarWeightTax, FiscalStampCost, DepositTotalAmount, ClaimTotalAmount, MorterViecleOfficialCode, RegistrationNumberType, RegistrationNumberKana";
                    sql += ", RegistrationNumberPlate, TopEngineerName, CarTax, NumberPlateCost, TaxFreeFieldName, TaxFreeFieldValue, UsVin, InspectionExpireDate";
                    sql += ", OptionalInsurance, SubscriptionFee, TaxableCostTotalAmount, CarTaxMemo, CarLiabilityInsuranceMemo, CarWeightTaxMemo, NumberPlateCostMemo, FiscalStampCostMemo, OptionalInsuranceMemo, SubscriptionFeeMemo, TaxableFreeFieldValue, TaxableFreeFieldName";           //Add 2020/02/17 yano #4025
                    sql += ", AccountInformation, AccountName";                 //Add 2020/06/08 yano #3665
                }

                sql += " from V_ServiceQuoteReport where SlipNumber='" + slipNumber + "'";


                if (revisionNumber == null)
                {
                    sql += " and DelFlag='0'";
                }
                else
                {
                    sql += " and RevisionNumber=" + revisionNumber;
                }
                sql += " order by LineNumber";
                ServiceQuoteReport quote = new ServiceQuoteReport();
                source.SQL = sql;
                quote.DataSource = source;
                try
                {
                    quote.Run(false);
                }
                catch (GrapeCity.ActiveReports.ReportException) { }
                report.Document.Pages.AddRange(quote.Document.Pages);

            }
            else
            {
                // ������ʂɌ��ς𕪂��č쐬
                foreach (var d in query)
                {

                    //Mod2017/01/21 arc yano #3657
                    string claimSql = "select ";

                    if (dispPersonalInfo) //�l����\������ꍇ
                    {
                        claimSql += "* ";
                    }
                    else
                    {
                        claimSql += "  SlipNumber, RevisionNumber, LineNumber, CompanyName, OfficeName, OfficeFullName";
                        claimSql += ", DepartmentCode, DepartmentFullName, DepartmentName, DepartmentPostCode, DepartmentPrefecture, DepartmentCity, DepartmentAddress1, DepartmentAddress2, DepartmentTelNumber1, DepartmentFaxNumber";
                        claimSql += ", CustomerCode, CustomerName, CustomerPostCode, CustomerPrefecture, CustomerCity, CustomerAddress1, CustomerAddress2, CustomerTelNumber, CustomerMobileNumber";
                        claimSql += ", SalesDate, ReceiptionEmployeeName, FrontEmployeeName, CarName, Mileage, MileageUnit, EngineType, FirstRegistration, NextInspectionDate, ModelName, Vin";
                        claimSql += ", ClassificationTypeNumber, ServiceWorkCode, LineContents, LineContents2, WorkType, TechnicalFeeAmount, Quantity, Price, Amount, SalesPlanDate, ConsumptionTaxId, Rate";
                        claimSql += ", EngineerEmployeeName, CostTotalAmount, CarBrandName, ArrivalPlanDate, SupplierCode, SupplierName, TaxTotalAmount, EngineerTotalAmount, PartsTotalAmount";
                        claimSql += ", CustomerClaimCode, CarEmployeeName";      //Mod 2019/08/30 yano #3976
                        claimSql += ", '' AS CustomerClaimName";
                        claimSql += ", '' AS CustomerClaimPostCode";
                        claimSql += ", '' AS CustomerClaimPrefecture";
                        claimSql += ", '' AS CustomerClaimCity";
                        claimSql += ", '' AS CustomerClaimAddress1";
                        claimSql += ", '' AS CustomerClaimAddress2";
                        claimSql += ", '' AS CustomerClaimTelNumber1";
                        claimSql += ", '' AS CustomerClaimTelNumber2";
                        claimSql += ", CustomerClaimFaxNumber";
                        claimSql += ", DisablePriceFlag, CarLiabilityInsurance, CarWeightTax, FiscalStampCost, DepositTotalAmount, CarTax, NumberPlateCost, TaxFreeFieldName, TaxFreeFieldValue";
                        claimSql += ", ClaimTotalAmount, MorterViecleOfficialCode, RegistrationNumberType, RegistrationNumberKana, RegistrationNumberPlate, AccountInformation, AccountName";
                        claimSql += ", PrintFlag, TopEngineerName, UsVin, InspectionExpireDate, ServiceWorkCount, WarrantyFlag";
                        claimSql += ", OptionalInsurance, SubscriptionFee, TaxableCostTotalAmount, CarTaxMemo, CarLiabilityInsuranceMemo, CarWeightTaxMemo, NumberPlateCostMemo, FiscalStampCostMemo, OptionalInsuranceMemo, SubscriptionFeeMemo, TaxableFreeFieldValue, TaxableFreeFieldName";           //Add 2020/02/17 yano #4025
                        claimSql += ", AccountInformation, AccountName";                 //Add 2020/06/08 yano #3665
                    }

                    claimSql += " from V_ServiceClaimReport where SlipNumber='" + slipNumber + "'";

                    if (revisionNumber == null)
                    {
                        claimSql += " and DelFlag='0'";
                    }
                    else
                    {
                        claimSql += " and RevisionNumber=" + revisionNumber;
                    }
                    //claimSql += " and CustomerClaimCode='" + d.Key + "'";
                    claimSql += " and CustomerClaimCode='" + d + "'";
                    claimSql += " order by LineNumber";
                    source.SQL = claimSql;

                    ServiceQuoteReportForClaim quote = new ServiceQuoteReportForClaim();
                    quote.DataSource = source;
                    try
                    {
                        quote.Run(false);
                    }
                    catch (GrapeCity.ActiveReports.ReportException) { }
                    report.Document.Pages.AddRange(quote.Document.Pages);
                }
            }
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(report.Document, slipNumber + "_" + revisionNumber, "�T�[�r�X���Ϗ�");
        }


        /// <summary>
        /// �T�[�r�X����`�[�A�[�i�������A�������׏����������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        /// <history>
        /// 2023/06/08 openwave #4141 �����斈�ɖ��א��������o�͂ł���悤�ɂ���B
        /// 2021/03/22 #4078�y�T�[�r�X�`�[���́z�[�Ԋm�F���ŏo�͂��钠�[�̎�ނ𓮓I�ɍi��
        /// 2015/05/27 arc nakayama #3210 �T�u�V�X�e���́u�T�[�r�X�v�ˁu�������e�B�[�i���v�̃��j���[�̈ڐA �[�Ԋm�F���̏o�͎��Ƀ������e�B�[�i�����o�͂���
        /// 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// </history>
        private FileContentResult PrintServiceSalesReport(string slipNumber, string revisionNumber, bool claimReportOutPut)
        {
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            //Mod 2021/03/22 yano #4078
            //string sql = "select * from V_ServiceSalesReport where SlipNumber='" + slipNumber + "'";
            //if (revisionNumber == null)
            //{
            //    sql += " and DelFlag='0'";
            //}
            //else
            //{
            //    sql += " and RevisionNumber=" + revisionNumber;
            //}
            //sql += " order by LineNumber";
            //source.SQL = sql;

            ////����`�[
            ServiceSalesReport sales = new ServiceSalesReport();
            //sales.DataSource = source;
            //try
            //{
            //    sales.Run(false);
            //}
            //catch (GrapeCity.ActiveReports.ReportException) { }

            List<ServiceReportByCutomerClaimType> list = db.ServiceReportByCutomerClaimType.ToList();

            ServiceSalesHeader header = new ServiceSalesOrderDao(db).GetByKey(slipNumber, int.Parse(revisionNumber));
            //Mod 2023/05/01 openwave #4141
            //var query =
            //  from a in header.ServiceSalesLine
            //  group a by a.CustomerClaimCode into c
            //  select new
            //  {
            //    c.Key
            //  };
            //var query =
            //  (from l in db.ServiceSalesLine
            //   where l.SlipNumber.Equals(slipNumber) && l.RevisionNumber.Equals(revisionNumber)
            //   &&    l.CustomerClaimCode != null
            //   select l.CustomerClaimCode)
            //  .Union
            //  (from h in db.ServiceSalesHeader
            //   where h.SlipNumber.Equals(slipNumber) && h.RevisionNumber.Equals(revisionNumber)
            //   &&    h.CustomerClaimCode != null
            //   select h.CustomerClaimCode)
            //;
            var query =
              from a in db.V_ServiceSalesClaimCode
              where a.SlipNumber.Equals(slipNumber) && a.RevisionNumber.Equals(revisionNumber)
              select a.CustomerClaimCode
              ;

            foreach (var d in query)
            {
                string customerClaimSql = "select v.* , 0 AS REPORT_TYPE from V_ServiceClaimReport v where v.SlipNumber='" + slipNumber + "'";
                if (revisionNumber == null)
                {
                    customerClaimSql += " and DelFlag='0'";
                }
                else
                {
                    customerClaimSql += " and RevisionNumber=" + revisionNumber;
                }

                //customerClaimSql += " and CustomerClaimCode='" + d.Key + "'";
                customerClaimSql += " and CustomerClaimCode='" + d + "'";
                customerClaimSql += " order by LineNumber";
                source.SQL = customerClaimSql;

                //Mod 2021/03/22 yano #4078
                //CustomerClaim cc = db.CustomerClaim.Where(x => x.CustomerClaimCode.Equals(d.Key)).FirstOrDefault();
                CustomerClaim cc = db.CustomerClaim.Where(x => x.CustomerClaimCode.Equals(d)).FirstOrDefault();
                ServiceReportByCutomerClaimType rec = db.ServiceReportByCutomerClaimType.Where(x => x.CusotmerClaimType.Equals(cc.CustomerClaimType)).FirstOrDefault();

                //�������׏�
                if (rec.ServiceClaimDetailReport)
                {
                    ServiceClaimDetailReport claimDetail = new ServiceClaimDetailReport();
                    claimDetail.DataSource = source;

                    try
                    {
                        claimDetail.Run(false);
                    }
                    catch (GrapeCity.ActiveReports.ReportException) { }

                    //�������׏���ǉ�����
                    sales.Document.Pages.AddRange(claimDetail.Document.Pages);
                }

                //�[�i������
                if (rec.ServiceClaimReport || claimReportOutPut)
                {
                    ServiceClaimReport claim = new ServiceClaimReport();
                    claim.DataSource = source;

                    try
                    {
                        claim.Run(false);
                    }
                    catch (GrapeCity.ActiveReports.ReportException) { }

                    //�[�i��������ǉ�����
                    sales.Document.Pages.AddRange(claim.Document.Pages);
                }
            }

            //Add 2015/05/27 arc nakayama #3210 �T�u�V�X�e���́u�T�[�r�X�v�ˁu�������e�B�[�i���v�̃��j���[�̈ڐA ��������擾����
            var CustomerClaimCode =
                from a in header.ServiceSalesLine
                join b in db.ServiceWork on a.ServiceWorkCode equals b.ServiceWorkCode
                where (b.Classification2 == "006")
                group a by a.CustomerClaimCode into c
                select new
                {
                    c.Key
                };

            foreach (var w in CustomerClaimCode)
            {
                string customerClaimSql = "select v.*, 1 AS REPORT_TYPE from V_ServiceClaimReport v where v.SlipNumber='" + slipNumber + "'";
                if (revisionNumber == null)
                {
                    customerClaimSql += " and DelFlag='0'";
                }
                else
                {
                    customerClaimSql += " and RevisionNumber=" + revisionNumber;
                    customerClaimSql += " and WarrantyFlag = 1";
                }

                customerClaimSql += " and CustomerClaimCode='" + w.Key + "'";
                customerClaimSql += " order by LineNumber";
                source.SQL = customerClaimSql;


                //�������׏�(�������e�B��)
                ServiceClaimDetailReport WarrantyData = new ServiceClaimDetailReport();
                WarrantyData.DataSource = source;

                try
                {
                    WarrantyData.Run(false);
                }
                catch (GrapeCity.ActiveReports.ReportException) { }

                //�������׏�(�������e�B��)��ǉ�����
                sales.Document.Pages.AddRange(WarrantyData.Document.Pages);
            }

            return PrintCommonMethod(sales.Document, slipNumber + "_" + revisionNumber, "�T�[�r�X�[�Ԋm�F��");

        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ��Ǝw�������������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        private FileContentResult PrintServiceInstructionReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_ServiceSalesReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += " and DelFlag='0'";
            }
            else
            {
                sql += " and RevisionNumber=" + revisionNumber;
            }
            sql += " order by LineNumber";
            source.SQL = sql;
            ServiceInstruction inst = new ServiceInstruction();
            inst.DataSource = source;

            try
            {
                inst.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(inst.Document, slipNumber + "_" + revisionNumber, "��Ǝw����");

        }
        /*
                    /// <summary>
                    /// �ԗ��I�����[���������
                    /// </summary>
                    /// <param name="departmentCode">����R�[�h</param>
                    /// <param name="inventoryMonth">�I����</param>
                    private void PrintCarInventorySrcReport(string departmentCode, DateTime inventoryMonth)
                    {
                        string sql = "select * from V_CarInventoryInProcess where DepartmentCode='" + departmentCode + "' and InventoryMonth='" + inventoryMonth.ToString("yyyy/MM/dd") + "' ";
                        sql += "order by LocationCode,MakerCode,CarBrandCode,CarCode,CarGradeCode,Vin";
                        source.SQL = sql;
                        CarInventorySrc carInv = new CarInventorySrc();
                        carInv.DataSource = source;
                        try
                        {
                            carInv.Run(false);
                        }
                        catch (GrapeCity.ActiveReports.ReportException) { }

                        PrintCommonMethod(carInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "�ԗ��I�����[");
                    }

                    /// <summary>
                    /// �ԗ��I���덷�[���������
                    /// </summary>
                    /// <param name="departmentCode">����R�[�h</param>
                    /// <param name="inventoryMonth">�I����</param>
                    private void PrintCarInventoryDiffReport(string departmentCode, DateTime inventoryMonth)
                    {
                        string sql = "select * from V_CarInventoryInProcess where DepartmentCode='" + departmentCode + "' and InventoryMonth='" + inventoryMonth.ToString("yyyy/MM/dd") + "' ";
                        sql += "and DifferentialQuantity<>0";
                        sql += "order by LocationCode,MakerCode,CarBrandCode,CarCode,CarGradeCode,Vin";
                        source.SQL = sql;
                        CarInventoryDiff carInv = new CarInventoryDiff();
                        carInv.DataSource = source;
                        try
                        {
                            carInv.Run(false);
                        }
                        catch (GrapeCity.ActiveReports.ReportException) { }

                        PrintCommonMethod(carInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "�ԗ��I���덷�\");
                    }

                    /// <summary>
                    /// ���i�I�����[���������
                    /// </summary>
                    /// <param name="departmentCode">����R�[�h</param>
                    /// <param name="inventoryMonth">�I����</param>
                    private void PrintPartsInventorySrcReport(string departmentCode, DateTime inventoryMonth)
                    {
                        string sql = "select * from V_PartsInventoryInProcess where DepartmentCode='" + departmentCode + "' and InventoryMonth='" + inventoryMonth.ToString("yyyy/MM/dd") + "' order by LocationCode,MakerCode,PartsNumber";
                        source.SQL = sql;
                        PartsInventorySrc partsInv = new PartsInventorySrc();
                        partsInv.DataSource = source;
                        try
                        {
                            partsInv.Run(false);
                        }
                        catch (GrapeCity.ActiveReports.ReportException) { }

                        PrintCommonMethod(partsInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "���i�I�����[");
                    }

                    /// <summary>
                    /// ���i�I���덷�[���������
                    /// </summary>
                    /// <param name="departmentCode">����R�[�h</param>
                    /// <param name="inventoryMonth">�I����</param>
                    private void PrintPartsInventoryDiffReport(string departmentCode, DateTime inventoryMonth)
                    {
                        string sql = "select * from V_PartsInventoryInProcess where DepartmentCode='" + departmentCode + "' and InventoryMonth='" + inventoryMonth.ToString("yyyy/MM/dd") + "' ";
                        sql += "and DifferentialQuantity<>0 ";
                        sql += "order by LocationCode,MakerCode,PartsNumber";
                        source.SQL = sql;
                        PartsInventoryDiff partsInv = new PartsInventoryDiff();
                        partsInv.DataSource = source;

                        try
                        {
                            partsInv.Run(false);
                        }
                        catch (GrapeCity.ActiveReports.ReportException) { }

                        PrintCommonMethod(partsInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "���i�I���덷�\");
                    }
        */
        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// ���Гo�^�˗������������
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        private FileContentResult PrintCarOwnregistReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_CarRegistRequest where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;
            CarOwnRegistRequest carRegist = new CarOwnRegistRequest();
            carRegist.SlipNumber = slipNumber;
            carRegist.RevisionNumber = revisionNumber;
            carRegist.DataSource = source;
            try
            {
                carRegist.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(carRegist.Document, slipNumber + "_" + revisionNumber, "���Гo�^�˗���");
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// �ԗ��o�^�˗������������
        /// �i�����ԍ�NULL�̏ꍇ�͍ŐV���r�W������������܂��j
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="revisionNumber">�����ԍ�</param>
        private FileContentResult PrintCarRegistReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_CarRegistRequest where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;
            CarRegistRequest carRegist = new CarRegistRequest();
            carRegist.SlipNumber = slipNumber;
            carRegist.RevisionNumber = revisionNumber;
            carRegist.DataSource = source;
            try
            {
                carRegist.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            sql = "select * from V_CarReceiptReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;

            CarReceiptReport report = new CarReceiptReport();
            report.SlipNumber = slipNumber;
            report.RevisionNumber = revisionNumber;
            report.DataSource = source;

            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException)
            {
            }

            carRegist.Document.Pages.AddRange(report.Document.Pages);
            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(carRegist.Document, slipNumber + "_" + revisionNumber, "�ԗ��o�^�˗���");


        }


        /// <summary>
        /// �ԗ����Ϗ����������
        /// �i�����ԍ�NULL�̏ꍇ�͍ŐV���r�W������������܂��j
        /// </summary>
        /// <param name="SlipNumber">�`�[�ԍ�</param>
        /// <param name="RevisionNumber">�����ԍ�</param>
        /// <history>
        /// 2023/08/15 yano #4176 �̔�����p�̏C��
        /// 2022/06/23 yano #4140�y�ԗ��`�[���́z�������̓o�^���`�l���\������Ȃ��s��̑Ή�
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// 2017/01/21 arc yano #3657 �����ɂ��ڋq�̌l���̕\���^��\�����s���悤�ɏC��
        /// 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� 
        ///                           �@���\�b�h�̌^��void��FileContentResult�ɕύX
        ///                           �A���^�[���l�̐ݒ�
        /// </history>
        private FileContentResult PrintCarQuoteReport(string slipNumber, string revisionNumber, bool dispPersonalInfo = false)
        {

            //Mod 2017/01/21 arc yano #3657
            string sql = "select ";

            if (dispPersonalInfo) //�l����\������ꍇ
            {
                sql += "* ";    //�O��擾
            }
            else //�ڋq���͋󕶎��ɂ���
            {
                sql += " SlipNumber, RevisionNumber, QuoteDate, QuoteExpireDate, SalesOrderDate, MakerName, CarBrandName, CarName, CarGradeName, ModelName, Vin, Mileage, SalesPrice";
                sql += ", TaxationAmount, DiscountAmount, TaxAmount, ShopOptionAmount, MakerOptionAmount, OutSourceAmount, OutSourceTaxAmount, ShopOptionTaxAmount, MakerOptionTaxAmount, OptionTotalAmount";
                sql += ", SubTotalAmount, CarTax, CarLiabilityInsurance, CarWeightTax, AcquisitionTax, InspectionRegistCost, ParkingSpaceCost, TradeInCost, RecycleDeposit, RecycleDepositTradeIn, NumberPlateCost";
                sql += ", TaxFreeFieldName, TaxFreeFieldValue, TaxFreeTotalAmount, InspectionRegistFee, ParkingSpaceFee, TradeInFee, PreparationFee, RecycleControlFee, RecycleControlFeeTradeIn, RequestNumberFee";
                sql += ", CarTaxUnexpiredAmount, CarLiabilityInsuranceUnexpiredAmount, TaxationFieldName, TaxationFieldValue, SalesCostTotalAmount, SalesCostTotalTaxAmount, OtherCostTotalAmount, CostTotalAmount";
                sql += ", TotalTaxAmount, GrandTotalAmount, VoluntaryInsuranceAmount, PaymentCashTotalAmount, RequestNumberCost, TradeInFiscalStampCost, TradeInAppraisalFee, FarRegistFee, TradeInMaintenanceFee";
                sql += ", InheritedInsuranceFee, TradeInAmount, TradeInTax, TradeInUnexpiredCarTax, TradeInRemainDebt, TradeInAppropriation, TradeInRecycleAmount, LoanFeeAmount, FirstAmountA, SecondAmountA, PaymentFrequencyA";
                sql += ", BonusAmountA, FirstAmountB, SecondAmountB, PaymentFrequencyB, BonusAmountB, FirstAmountC, SecondAmountC, PaymentFrequencyC, BonusAmountC, PaymentType, VoluntaryInsuranceCompanyName, VoluntaryInsuranceTermTo";
                sql += ", DelFlag, ExteriorColorName, InteriorColorName, CustomerCode, UserCode, ConsumptionTaxId, Rate, DepartmentName, DepartmentPostCode, DepartmentPrefecture, DepartmentCity, DepartmentAddress1, DepartmentAddress2";
                sql += ", DepartmentTelNumber1, DepartmentTelNumber2, DepartmentFaxNumber, DepartmentFullName, OfficeName, OfficeFullName, OfficeTelNumber1, OfficeTelNumber2, CompanyName, CompanyPostCode, CompanyAddress, CompanyTelNumber";
                sql += ", PresidentName, EmployeeName, Door, Displacement, Fuel, ModelYear, RequestRegistDate"; //Mod 2021/08/02 yano #4097  //Mod 2019/09/04 yano #4011
                sql += ", '' AS CustomerName";
                sql += ", '' AS CustomerNameKana";
                sql += ", CustomerType";
                sql += ", FirstName";
                sql += ", LastName";
                sql += ", '' AS CustomerBirthday";
                sql += ", '' AS CustomerPostCode";
                sql += ", '' AS CustomerPrefecture";
                sql += ", '' AS CustomerCity";
                sql += ", '' AS CustomerAddress1";
                sql += ", '' AS CustomerAddress2";
                sql += ", '' AS CustomerTelNumber";
                sql += ", CustomerMobileNumber";
                sql += ", CustomerFaxNumber";
                sql += ", CustomerMailAddress";
                sql += ", Sex";
                sql += ", CustomerMobileNumber";
                sql += ", WorkingCompanyName";
                sql += ", WorkingCompanyAddress";
                sql += ", WorkingCompanyTelNumber";
                sql += ", PositionName";
                sql += ", CorporationType, NewUsedTypeCode, NewUsedType, TransMission";
                sql += ", '' AS CustomerSex";
                sql += ", MorterViecleOfficialCode, RegistrationNumberType, RegistrationNumberKana, RegistrationNumberPlate, SalesCarNumber, InspectionExpireDate, ManufacturingYear, Steering, MileageUnitName, RegistrationType";
                sql += ", VoluntaryInsuranceType, LoanName, LoanCompanyName, UserName, UserPostCode, UserPrefecture, UserCity, UserAddress1, UserAddress2, LoanPrincipalAmount, LoanTotalAmount, PaymentTermFrom, PaymentTermTo";
                sql += ", PaymentFrequency, PaymentFrequency2, FirstAmount, SecondAmount, BonusMonth1, BonusMonth2, BonusAmount, RemainAmount, LoanRate, TradeInMakerName1, TradeInCarName1, TradeInManufacturingYear1";
                sql += ", TradeInModelSpecificateNumber1, TradeInRegistrationNumber1, TradeInInspectionExpiredDate1, TradeInMileage1, TradeInMileageUnit1, TradeInVin1, TradeInCount, AccountInformation";
                sql += ", AccountName, PrintFlag, RevenueStampCost, TradeInCarTaxDeposit, TaxationAmountTax, PaymentSecondFrequencyA, PaymentSecondFrequencyB, PaymentSecondFrequencyC";
                sql += ", TradeInHolderName1, TradeInHolderName2, TradeInHolderName3";//Add 2022/06/23 yano #4140
                sql += ", OutJurisdictionRegistFee";//Mod 2023/08/15 yano #4176
            }

            sql += " from V_CarSalesReport where SlipNumber='" + slipNumber + "'";

            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }

            source.SQL = sql;
            CarQuoteReport carQuote = new CarQuoteReport();
            carQuote.SlipNumber = slipNumber;
            carQuote.RevisionNumber = revisionNumber;
            carQuote.DataSource = source;

            try
            {
                carQuote.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            return PrintCommonMethod(carQuote.Document, slipNumber + "_" + revisionNumber, "�ԗ����Ϗ�"); //Mod 2014/09/22 arc yano
        }

        //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���\�b�h�̌^��void��FileContentResult�ɕύX
        /// <summary>
        /// �ԗ����������������
        /// �i�����ԍ�NULL�̏ꍇ�͍ŐV���r�W������������܂��j
        /// </summary>
        /// <param name="SlipNumber">�`�[�ԍ�</param>
        /// <param name="RevisionNumber">�����ԍ�</param>
        /// <history>
        /// 2019/09/04 yano #4011 ����ŁA�����ԐŁA�����Ԏ擾�ŕύX�ɔ������C���
        /// </history>
        private FileContentResult PrintCarSalesOrderReport(string slipNumber, string revisionNumber)
        {
            string sql = "select * from V_CarSalesReport where SlipNumber='" + slipNumber + "'";
            if (revisionNumber == null)
            {
                sql += "and DelFlag='0'";
            }
            else
            {
                sql += "and RevisionNumber=" + revisionNumber;
            }
            source.SQL = sql;
            CarSalesOrderReport carSalesOrder = new CarSalesOrderReport();

            carSalesOrder.SlipNumber = slipNumber;
            carSalesOrder.RevisionNumber = revisionNumber;
            carSalesOrder.ReportName = "�@���q�l�T��";
            carSalesOrder.FurikomiTesuryoIsVisible = true;
            carSalesOrder.DataSource = source;
            carSalesOrder.VisibleCustomer = true;

            CarSalesOrderReport_ura ura = new CarSalesOrderReport_ura();

            //Mod 2019/09/04 yano #4011
            ura.DataSource = source;
            //ura.DataSource = null;

            CarSalesOrderReport carSalesOrderHikae = new CarSalesOrderReport();
            carSalesOrderHikae.SlipNumber = slipNumber;
            carSalesOrderHikae.RevisionNumber = revisionNumber;
            carSalesOrderHikae.ReportName = "�A�X�܍T��";
            carSalesOrderHikae.FurikomiTesuryoIsVisible = false;
            carSalesOrderHikae.DataSource = source;
            carSalesOrderHikae.VisibleCustomer = false;

            try
            {
                carSalesOrder.Run(false);
                ura.Run(false);
                carSalesOrderHikae.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            carSalesOrder.Document.Pages.AddRange(ura.Document.Pages);
            carSalesOrder.Document.Pages.AddRange(carSalesOrderHikae.Document.Pages);
            carSalesOrder.Document.Pages.AddRange(ura.Document.Pages);

            //Mod 2014/09/22 arc yano #3091 Exception���O��ʔ����Ή� ���^�[���l�̐ݒ�
            return PrintCommonMethod(carSalesOrder.Document, slipNumber + "_" + revisionNumber, "�ԗ�������");

        }

        //Add 2017/02/23 arc nakayama #3626_�y�ԁz�ԗ��`�[�́u��ƈ˗����v�֎󒍌�ɒǉ�����Ȃ�
        /// <summary>
        /// �ԗ���ƈ˗�������
        /// </summary>
        /// <param name="receiptionId"></param>
        private FileContentResult PrintServiceRequestReport(string OriginalSlipNumber)
        {
            string sql = "EXEC [dbo].[GetServiceRequestReport] @OriginalSlipNumber = N'" + OriginalSlipNumber + "'";
            source.SQL = sql;
            ServiceRequestReport report = new ServiceRequestReport();
            report.DataSource = source;
            try
            {
                report.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            return PrintCommonMethod(report.Document, "�ԗ���ƈ˗���", "�ԗ���ƈ˗���");

        }

        /// <summary>
        /// ������ʃ��\�b�h
        /// </summary>
        /// <param name="doc">ActiveReport�̃h�L�������g�I�u�W�F�N�g</param>
        private FileContentResult PrintCommonMethod(GrapeCity.ActiveReports.Document.SectionDocument doc, string param, string docName)
        {

            //�w�b�_�̃N���A
            //Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();

            // �t�@�C����
            string fileName = string.Format("{0}_{1:yyyyMMddhhmmss}", param, DateTime.Now) + ".pdf";


            // PDF�o�͗�����ۑ�
            InsertPrintPdfHistory(param, docName, fileName);

            //  �u���E�U�ɑ΂���PDF�h�L�������g�̓K�؂ȃr���[�����g�p����悤�Ɏw�肵�܂��B
            Response.ContentType = "application/pdf";
            //Response.ContentType = "octet-stream";

            Response.AddHeader("content-disposition", "inline; filename=" + fileName);
            // ���̃R�[�h�ɒu��������ƐV�����E�B���h�E���J���܂��F
            //Response.AddHeader("content-disposition", "attachment; filename=MyPDF.PDF");

            // PDF�G�N�X�|�[�g�N���X�̃C���X�^���X���쐬���܂��B
            PdfExport pdf = new PdfExport();

            // PDF�̏o�͗p�̃������X�g���[�����쐬���܂��B
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();

            // �������X�g���[����PDF�G�N�X�|�[�g���s���܂��B
            pdf.Export(doc, memStream);

            // �t�@�C���ɕۑ�����
            string directoryName = Server.MapPath("/Pdf/");
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            string filePath = directoryName + fileName;
            pdf.Export(doc, filePath);

            // PDF�o�͗�����DB�ɕۑ�����
            //PdfPrintLog log = new PdfPrintLog();
            //log.PrintId = Guid.NewGuid();
            //log.CreateDate = DateTime.Now;
            //log.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //log.FileName = fileName;
            //log.DocumentName = param;

            //db.PdfPrintLog.InsertOnSubmit(log);

            // �o�̓X�g���[����PDF�̃X�g���[�����o�͂��܂��B
            Response.BinaryWrite(memStream.ToArray());

            // �o�b�t�@�����O����Ă��邷�ׂĂ̓��e���N���C�A���g�֑��M���܂��B
            //Response.End();
            //base.HttpContext.ApplicationInstance.CompleteRequest();
            //Response.Close();

            /*
             Mod 2014/10/14 arc amii �G���[���b�Z�[�W(IE8)�Ή� #3091 IE8�ł�Flush���g�p���Ȃ��ƒ��[���\������Ȃ������ׁA����
                                         IE8���g�p����Ȃ��Ȃ������A����Flush�͕s�v�ƂȂ�BExceptionFilterAttribute.cs�����l�L�q����
             */
            // 2023.07.07 openwave
            //Response.Flush();
            //Response.End();

            return File(memStream.ToArray(), "application/pdf");
        }

        /// <summary>
        /// PDF�o�͗������쐬����
        /// </summary>
        /// <param name="slipNumber">�`�[�ԍ�</param>
        /// <param name="docName">�o�͏��ޖ�</param>
        /// <param name="fileName">�o�̓t�@�C����</param>
        private void InsertPrintPdfHistory(string slipNumber, string docName, string fileName)
        {
            // Add 2014/08/04 arc amii �G���[���O�Ή� �o�^�p��DataContext��ݒ肷��
            db = new CrmsLinqDataContext();
            db.Log = new OutputWriter();

            PrintPdfHistory history = new PrintPdfHistory();
            history.PdfId = Guid.NewGuid();
            history.SlipNumber = slipNumber;
            history.DocName = docName;
            history.FileName = fileName;
            history.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            history.CreateDate = DateTime.Now;
            db.PrintPdfHistory.InsertOnSubmit(history);
            db.SubmitChanges();
        }

        private GrapeCity.ActiveReports.Data.SqlDBDataSource source;
    }
}
