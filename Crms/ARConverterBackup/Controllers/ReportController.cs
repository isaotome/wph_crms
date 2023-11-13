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
using Crms.Models;                      //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加

namespace Crms.Controllers
{
    /// <summary>
    /// 帳票出力用コントローラ
    /// </summary>
    [ExceptionFilter]
    [AuthFilter]
    [OutputCache(Duration = 0, VaryByParam = "none")]
    public class ReportController : Controller
    {
        //Add 2014/08/07 arc amii エラーログ対応 ログ出力の為に追加
        private static readonly string FORM_NAME = "帳票出力";     // 画面名
        private static readonly string PROC_NAME = "出力"; // 処理名

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// ActiveReport用データソース
        /// </summary>


        /// <summary>
        /// コンストラクタ
        /// 接続文字列を設定ファイルから取得してデータソースにセット
        /// タイムアウトを30秒でセット
        /// </summary>
        public ReportController()
        {
            source = new GrapeCity.ActiveReports.Data.SqlDBDataSource();
            source.ConnectionString = ConfigurationManager.ConnectionStrings["CrmsDao.Properties.Settings.CRMSConnectionString"].ToString();
            source.CommandTimeout = 30;

            db = new CrmsLinqDataContext();
        }

        /// <summary>
        /// 担当者が権限のある部門かどうかを判定する
        /// </summary>
        /// <param name="employee">担当者</param>
        /// <param name="department">部門</param>
        /// <returns>閲覧可能かどうか</returns>
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
        /// 帳票出力用入り口
        /// </summary>
        /// <param name="id">帳票の種類</param>
        /// <returns>空のVIEW</returns>
        /// <history>
        /// 2021/03/22 yano #4078【サービス伝票入力】納車確認書で出力する帳票の種類を動的に絞る
        /// 2017/01/21 arc yano #3657 引数により顧客の個人情報の表示／非表示を行うように修正
        /// </history>
        public ActionResult Print()
        {
            //引数が指定されていなければ中止
            if (string.IsNullOrEmpty(Request["reportName"]) || string.IsNullOrEmpty(Request["reportParam"])) return new EmptyResult();

            string id = Request["reportName"];
            string[] param = Request["reportParam"].Split(',');
            Employee employee = (Employee)Session["Employee"];

            //Add 2017/01/21 arc yano #3657
            bool dispPersonalInfo = Request["dispPersonalInfo"] != null ? bool.Parse(Request["dispPersonalInfo"]) : false;

            //Add 2021/03/22 yano #4078
            bool claimReportOutPut = Request["claimReportOutPut"] != null ? bool.Parse(Request["claimReportOutPut"]) : false;

            //Add 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            FileContentResult result = null;
            object target;
            // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
            string reportName = "";

            // Add 2014/08/07 arc amii エラーログ対応 SQL文を取得する処理追加
            db.Log = new OutputWriter();

            try
            {
                //string inventoryMonth = "";
                switch (id)
                {
                    //車両見積書
                    case "CarQuote":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "車両見積書";

                        //閲覧権限がなければ中止
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarQuoteReport(param[0], param[1], dispPersonalInfo);      //Mod 2017/01/21 arc yano #3657
                        break;

                    //車両注文書
                    case "CarSalesOrder":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "車両注文書";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarSalesOrderReport(param[0], param[1]);
                        break;

                    //車両登録依頼書
                    case "CarRegistRequest":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "車両登録依頼書";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1] ?? "0"));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/08/14 arc amii エラーログ対応 null落ちを防ぐ為、CommonUtils.DefaultStringを追加
                        if (((CarSalesHeader)target).Customer != null &&
                                (CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("103") ||
                                    CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("102") ||
                                    CommonUtils.DefaultString(((CarSalesHeader)target).Customer.CustomerType).Equals("101")))
                        {
                            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                            result = PrintCarOwnregistReport(param[0], param[1]);
                        }
                        else
                        {
                            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                            result = PrintCarRegistReport(param[0], param[1]);
                        }
                        break;

                    //納車確認書
                    case "CarDeliveryReport":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "納車確認書";
                        target = new CarSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1] ?? "0"));
                        if (target == null || !CanViewData(employee, ((CarSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarDeliveryReport(param[0], param[1]);
                        break;
                    /*
                                                  //車両棚卸原票
                                                  case "CarInventorySrc":
                                                      // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                                                      reportName = "車両棚卸原票";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintCarInventorySrcReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //車両棚卸誤差表
                                                  case "CarInventoryDiff":
                                                      // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                                                      reportName = "車両棚卸誤差表";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");

                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintCarInventoryDiffReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //部品棚卸原票
                                                  case "PartsInventorySrc":
                                                      // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                                                      reportName = "部品棚卸原票";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintPartsInventorySrcReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;

                                                  //部品棚卸誤差表
                                                  case "PartsInventoryDiff":
                                                      // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                                                      reportName = "部品棚卸誤差表";
                                                      target = new DepartmentDao(db).GetByKey(param[0]);
                                                      if (target == null || !CanViewData(employee, (Department)target)) return RedirectToAction("AuthenticationError", "Error");
                                                      inventoryMonth = param[1].Substring(0, 4) + "/" + param[1].Substring(4, 2) + "/01";
                                                      PrintPartsInventoryDiffReport(param[0], DateTime.Parse(inventoryMonth));
                                                      break;
                    */
                    //作業指示書
                    case "ServiceInstruction":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "作業指示書";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintServiceInstructionReport(param[0], param[1]);
                        break;

                    //サービス見積
                    case "ServiceQuote":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "サービス見積";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintServiceQuoteReport(param[0], param[1], dispPersonalInfo);     //Mod 2017/01/21 arc yano #3657
                        break;

                    //売上伝票
                    case "ServiceSales":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "売上伝票";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintServiceSalesReport(param[0], param[1], claimReportOutPut); //Mod 2021/03/22 yano #4078 
                                                                                                 //result = PrintServiceSalesReport(param[0], param[1]);
                        break;

                    //外注依頼書
                    case "OutSourceRequest":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "外注依頼書";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintOutSourceRequestReport(param[0], param[1]);
                        break;

                    //部品発注依頼書
                    case "PartsPurchaseOrderRequest":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "部品発注依頼書";
                        target = new ServiceSalesOrderDao(db).GetByKey(param[0], int.Parse(param[1]));
                        if (target == null || !CanViewData(employee, ((ServiceSalesHeader)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintPartsPurchaseOrderRequestReport(param[0], param[1]);
                        break;
                    //請求書
                    case "Invoice":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "請求書";
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintInvoiceReport(param);
                        break;
                    // 入庫連絡票
                    case "CarArrival":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "入庫連絡票";
                        target = new CarPurchaseDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarPurchase)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarArrivalReport(param[0]);
                        break;
                    // 査定票
                    case "CarAppraisal":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "査定票";
                        target = new CarAppraisalDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarAppraisal)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarAppraisalReport(param[0]);
                        break;
                    // 車両買取契約書
                    case "CarPurchaseAgreement":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "車両買取契約書";
                        target = new CarAppraisalDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CarAppraisal)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintCarPurchaseAgreementReport(param[0]);
                        break;
                    // サービス承り書
                    case "ServiceReceiption":
                        // Add 2014/08/07 arc amii エラーログ対応 ログ出力の処理名(帳票名)を取得する
                        reportName = "サービス承り書";
                        target = new CustomerReceiptionDao(db).GetByKey(new Guid(param[0]));
                        if (target == null || !CanViewData(employee, ((CustomerReceiption)target).Department)) return RedirectToAction("AuthenticationError", "Error");
                        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
                        result = PrintServiceReceiptionReport(param[0]);
                        break;
                    //車両作業依頼書
                    case "ServiceRequest":
                        reportName = "作業依頼書";
                        result = PrintServiceRequestReport(param[0]);
                        break;
                    default:
                        break;

                }
            }
            catch (GrapeCity.ActiveReports.ReportException re)
            {
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ログに出力
                OutputLogger.NLogError(re, reportName + PROC_NAME, FORM_NAME, "");
                // エラーページに遷移
                return View("Error");
            }
            catch (Exception e)
            {
                // セッションにSQL文を登録
                Session["ExecSQL"] = OutputLogData.sqlText;
                // ログに出力
                OutputLogger.NLogFatal(e, reportName + PROC_NAME, FORM_NAME, "");

                // エラーページに遷移
                return View("Error");
            }
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応　ここでpdfファイルを返すように設定する。
            return result;
            //return new EmptyResult();
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// サービス承り書を印刷する
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

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, "サービス承り書", "サービス承り書");

        }

        /// <summary>
        /// 車輌買取契約書を印刷する
        /// </summary>
        /// <param name="p"></param>
        /// <history>
        ///  2018/12/21 yano #3965 WE版新システム対応（Web.configによる処理の分岐) 接続先DBにより、社名ロゴの取得ロゴを変更する。
        ///  2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// </history>
        private FileContentResult PrintCarPurchaseAgreementReport(string carAppraisalId)
        {
            //Add 2018/12/21 yano #3965
            string filePath = db.Logo.Where(x => ("CarPurchaseAgreementReport").Equals(x.LogoName)).Select(x => x.FilePath).FirstOrDefault();


            string sql = "select * from V_CarPurchaseAgreementReport where CarAppraisalId='" + carAppraisalId + "'";
            source.SQL = sql;
            CarPurchaseAgreementReport report = new CarPurchaseAgreementReport(filePath);  //Mod 2018/12/21 yano #3965
            report.HikaeName = "（お客様控え）";
            report.DataSource = source;

            CarPurchaseAgreementReport_ura ura = new CarPurchaseAgreementReport_ura();

            CarPurchaseAgreementReport report2 = new CarPurchaseAgreementReport(filePath); //Mod 2018/12/21 yano #3965
            report2.HikaeName = "（店舗控え）";
            report2.DataSource = source;

            try
            {
                report.Run();
                report2.Run();
                ura.Run();
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            report.Document.Pages.AddRange(ura.Document.Pages);

            // 店舗控え
            report.Document.Pages.AddRange(report2.Document.Pages);
            report.Document.Pages.AddRange(ura.Document.Pages);

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, "車輌買取契約書", "車輌買取契約書");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 査定票を印刷する
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, "査定票", "査定票");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 入庫連絡票を印刷する
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, "入庫連絡票", "入庫連絡票");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 入金確認書を印刷する
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, slipNumber + "_" + revisionNumber, "入金確認書");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 請求書を印刷する
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
                //TODO:請求書印刷フラグ更新処理を記述

                //請求書対象リストに追加
                invoiceList.Add(plan);
            }

            //請求先別に集約
            var query =
                from a in invoiceList
                group a by a.CustomerClaimCode into claim
                select claim;

            //請求先ごとに請求データを抽出
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(invoice.Document, "Invoice", "請求書");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 部品発注依頼書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
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

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(order.Document, slipNumber + "_" + revisionNumber, "部品発注依頼書");

        }

        /// <summary>
        /// 外注依頼書を印刷する
        /// </summary>
        /// <param name="SlipNumber">伝票番号</param>
        /// <param name="RevisionNumber">改訂番号</param>
        /// <history>
        /// 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
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

            //ゼロ件の場合、空のデータをセット
            if (outSource.Document.Pages.Count == 0)
            {
                OutSourceRequestReport report = new OutSourceRequestReport();
                string sql = "select * from V_ServiceSalesReport where 0=1";
                source.SQL = sql;
                report.DataSource = source;
                report.Run(false);
                outSource.Document.Pages.AddRange(report.Document.Pages);
            }

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(outSource.Document, slipNumber + "_" + revisionNumber, "外注依頼書");

        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 車両納車確認書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
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
            carDeliveryReport1.HikaeTitle = "お客様控え";
            carDeliveryReport1.DataSource = source;

            CarDeliveryReport carDeliveryReport2 = new CarDeliveryReport();
            carDeliveryReport2.SlipNumber = slipNumber;
            carDeliveryReport2.RevisionNumber = revisionNumber;
            carDeliveryReport2.HikaeTitle = "会社控え";
            carDeliveryReport2.DataSource = source;

            try
            {
                carDeliveryReport1.Run(false);
                carDeliveryReport2.Run(false);
            }
            catch (GrapeCity.ActiveReports.ReportException) { }

            carDeliveryReport1.Document.Pages.AddRange(carDeliveryReport2.Document.Pages);

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(carDeliveryReport1.Document, slipNumber + "_" + revisionNumber, "車両納車確認書");

        }

        /// <summary>
        /// サービス見積書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <history>
        /// 2023/06/08 openwave #4141【サービス伝票入力】請求書関連の修正
        /// 2020/06/08 yano #3665【サービス】サービス伝票の見積もりへ振込先印刷
        /// 2020/02/17 yano #4025【サービス伝票】費目毎に仕訳できるように機能追加
        /// 2019/08/30 yano #3976 サービス伝票入力　受付担当の文言変更
        /// 2017/01/21 arc yano #3657 引数により顧客の個人情報の表示／非表示を行うように修正
        /// 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
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

                if (dispPersonalInfo) //個人情報を表示する場合    
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
                // 請求先別に見積を分けて作成
                foreach (var d in query)
                {

                    //Mod2017/01/21 arc yano #3657
                    string claimSql = "select ";

                    if (dispPersonalInfo) //個人情報を表示する場合
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(report.Document, slipNumber + "_" + revisionNumber, "サービス見積書");
        }


        /// <summary>
        /// サービス売上伝票、納品請求書、請求明細書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <history>
        /// 2023/06/08 openwave #4141 請求先毎に明細請求書を出力できるようにする。
        /// 2021/03/22 #4078【サービス伝票入力】納車確認書で出力する帳票の種類を動的に絞る
        /// 2015/05/27 arc nakayama #3210 サブシステムの「サービス」⇒「ワランティ納品書」のメニューの移植 納車確認書の出力時にワランティ納品書を出力する
        /// 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
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

            ////売上伝票
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

                //請求明細書
                if (rec.ServiceClaimDetailReport)
                {
                    ServiceClaimDetailReport claimDetail = new ServiceClaimDetailReport();
                    claimDetail.DataSource = source;

                    try
                    {
                        claimDetail.Run(false);
                    }
                    catch (GrapeCity.ActiveReports.ReportException) { }

                    //請求明細書を追加する
                    sales.Document.Pages.AddRange(claimDetail.Document.Pages);
                }

                //納品請求書
                if (rec.ServiceClaimReport || claimReportOutPut)
                {
                    ServiceClaimReport claim = new ServiceClaimReport();
                    claim.DataSource = source;

                    try
                    {
                        claim.Run(false);
                    }
                    catch (GrapeCity.ActiveReports.ReportException) { }

                    //納品請求書を追加する
                    sales.Document.Pages.AddRange(claim.Document.Pages);
                }
            }

            //Add 2015/05/27 arc nakayama #3210 サブシステムの「サービス」⇒「ワランティ納品書」のメニューの移植 請求先を取得する
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


                //請求明細書(ワランティ分)
                ServiceClaimDetailReport WarrantyData = new ServiceClaimDetailReport();
                WarrantyData.DataSource = source;

                try
                {
                    WarrantyData.Run(false);
                }
                catch (GrapeCity.ActiveReports.ReportException) { }

                //請求明細書(ワランティ分)を追加する
                sales.Document.Pages.AddRange(WarrantyData.Document.Pages);
            }

            return PrintCommonMethod(sales.Document, slipNumber + "_" + revisionNumber, "サービス納車確認書");

        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 作業指示書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
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

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(inst.Document, slipNumber + "_" + revisionNumber, "作業指示書");

        }
        /*
                    /// <summary>
                    /// 車両棚卸原票を印刷する
                    /// </summary>
                    /// <param name="departmentCode">部門コード</param>
                    /// <param name="inventoryMonth">棚卸月</param>
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

                        PrintCommonMethod(carInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "車両棚卸原票");
                    }

                    /// <summary>
                    /// 車両棚卸誤差票を印刷する
                    /// </summary>
                    /// <param name="departmentCode">部門コード</param>
                    /// <param name="inventoryMonth">棚卸月</param>
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

                        PrintCommonMethod(carInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "車両棚卸誤差表");
                    }

                    /// <summary>
                    /// 部品棚卸原票を印刷する
                    /// </summary>
                    /// <param name="departmentCode">部門コード</param>
                    /// <param name="inventoryMonth">棚卸月</param>
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

                        PrintCommonMethod(partsInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "部品棚卸原票");
                    }

                    /// <summary>
                    /// 部品棚卸誤差票を印刷する
                    /// </summary>
                    /// <param name="departmentCode">部門コード</param>
                    /// <param name="inventoryMonth">棚卸月</param>
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

                        PrintCommonMethod(partsInv.Document, departmentCode + "_" + string.Format("{0:yyyyMM}", inventoryMonth), "部品棚卸誤差表");
                    }
        */
        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 自社登録依頼書を印刷する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
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

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(carRegist.Document, slipNumber + "_" + revisionNumber, "自社登録依頼書");
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 車両登録依頼書を印刷する
        /// （改訂番号NULLの場合は最新リビジョンを印刷します）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
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
            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(carRegist.Document, slipNumber + "_" + revisionNumber, "車両登録依頼書");


        }


        /// <summary>
        /// 車両見積書を印刷する
        /// （改訂番号NULLの場合は最新リビジョンを印刷します）
        /// </summary>
        /// <param name="SlipNumber">伝票番号</param>
        /// <param name="RevisionNumber">改訂番号</param>
        /// <history>
        /// 2023/08/15 yano #4176 販売諸費用の修正
        /// 2022/06/23 yano #4140【車両伝票入力】注文書の登録名義人が表示されない不具合の対応
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
        /// 2017/01/21 arc yano #3657 引数により顧客の個人情報の表示／非表示を行うように修正
        /// 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 
        ///                           ①メソッドの型をvoid→FileContentResultに変更
        ///                           ②リターン値の設定
        /// </history>
        private FileContentResult PrintCarQuoteReport(string slipNumber, string revisionNumber, bool dispPersonalInfo = false)
        {

            //Mod 2017/01/21 arc yano #3657
            string sql = "select ";

            if (dispPersonalInfo) //個人情報を表示する場合
            {
                sql += "* ";    //前列取得
            }
            else //顧客情報は空文字にする
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

            return PrintCommonMethod(carQuote.Document, slipNumber + "_" + revisionNumber, "車両見積書"); //Mod 2014/09/22 arc yano
        }

        //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 メソッドの型をvoid→FileContentResultに変更
        /// <summary>
        /// 車両注文書を印刷する
        /// （改訂番号NULLの場合は最新リビジョンを印刷します）
        /// </summary>
        /// <param name="SlipNumber">伝票番号</param>
        /// <param name="RevisionNumber">改訂番号</param>
        /// <history>
        /// 2019/09/04 yano #4011 消費税、自動車税、自動車取得税変更に伴う改修作業
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
            carSalesOrder.ReportName = "①お客様控え";
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
            carSalesOrderHikae.ReportName = "②店舗控え";
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

            //Mod 2014/09/22 arc yano #3091 Exceptionログ大量発生対応 リターン値の設定
            return PrintCommonMethod(carSalesOrder.Document, slipNumber + "_" + revisionNumber, "車両注文書");

        }

        //Add 2017/02/23 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない
        /// <summary>
        /// 車両作業依頼書する
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

            return PrintCommonMethod(report.Document, "車両作業依頼書", "車両作業依頼書");

        }

        /// <summary>
        /// 印刷共通メソッド
        /// </summary>
        /// <param name="doc">ActiveReportのドキュメントオブジェクト</param>
        private FileContentResult PrintCommonMethod(GrapeCity.ActiveReports.Document.SectionDocument doc, string param, string docName)
        {

            //ヘッダのクリア
            //Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();

            // ファイル名
            string fileName = string.Format("{0}_{1:yyyyMMddhhmmss}", param, DateTime.Now) + ".pdf";


            // PDF出力履歴を保存
            InsertPrintPdfHistory(param, docName, fileName);

            //  ブラウザに対してPDFドキュメントの適切なビューワを使用するように指定します。
            Response.ContentType = "application/pdf";
            //Response.ContentType = "octet-stream";

            Response.AddHeader("content-disposition", "inline; filename=" + fileName);
            // 次のコードに置き換えると新しいウィンドウを開きます：
            //Response.AddHeader("content-disposition", "attachment; filename=MyPDF.PDF");

            // PDFエクスポートクラスのインスタンスを作成します。
            PdfExport pdf = new PdfExport();

            // PDFの出力用のメモリストリームを作成します。
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();

            // メモリストリームにPDFエクスポートを行います。
            pdf.Export(doc, memStream);

            // ファイルに保存する
            string directoryName = Server.MapPath("/Pdf/");
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            string filePath = directoryName + fileName;
            pdf.Export(doc, filePath);

            // PDF出力履歴をDBに保存する
            //PdfPrintLog log = new PdfPrintLog();
            //log.PrintId = Guid.NewGuid();
            //log.CreateDate = DateTime.Now;
            //log.CreateEmployeeCode = ((Employee)Session["Employee"]).EmployeeCode;
            //log.FileName = fileName;
            //log.DocumentName = param;

            //db.PdfPrintLog.InsertOnSubmit(log);

            // 出力ストリームにPDFのストリームを出力します。
            Response.BinaryWrite(memStream.ToArray());

            // バッファリングされているすべての内容をクライアントへ送信します。
            //Response.End();
            //base.HttpContext.ApplicationInstance.CompleteRequest();
            //Response.Close();

            /*
             Mod 2014/10/14 arc amii エラーメッセージ(IE8)対応 #3091 IE8ではFlushを使用しないと帳票が表示されなかった為、復活
                                         IE8が使用されなくなった時、このFlushは不要となる。ExceptionFilterAttribute.csも同様記述あり
             */
            // 2023.07.07 openwave
            //Response.Flush();
            //Response.End();

            return File(memStream.ToArray(), "application/pdf");
        }

        /// <summary>
        /// PDF出力履歴を作成する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="docName">出力書類名</param>
        /// <param name="fileName">出力ファイル名</param>
        private void InsertPrintPdfHistory(string slipNumber, string docName, string fileName)
        {
            // Add 2014/08/04 arc amii エラーログ対応 登録用にDataContextを設定する
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
