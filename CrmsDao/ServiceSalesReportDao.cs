using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao
{
    public class ServiceSalesReportDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ServiceSalesReportDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        // Add 2015/09/11 arc nakayama #3165_サービス集計表
        /// <summary>
        /// サービス集計表の検索
        /// </summary>
        /// <param name="DepartmentCondition">サービス集計表検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>サービス集計表検索結果</returns>
        public ServiceSalesAmount GetSummaryByCondition(ServiceSalesSearchCondition condition)
        {
            // ストプロ実行
            var Ret = db.GetServiceSalesReport(string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),//納車日From
                                               string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),  //納車日To
                                               condition.WorkTypeCode,  //区分（社内/社外）
                                               condition.DepartmentCode //部門コード
                                               );

            ServiceSalesAmount table = new ServiceSalesAmount();
            List<GetServiceSalesReportResult> List = new List<GetServiceSalesReportResult>();

            //初期化
            table.SumTotalServiceAmount = 0;    //工賃売上の合計
            table.SumTotalPartsAmount = 0;      //部品売上の合計
            table.SumTotalTaxAmount = 0;        //消費税の合計
            table.SumGrandTotalAmount = 0;      //売上の総合計
            table.SumTotalServiceCost = 0;      //工賃原価の合計
            table.SumTotalPartsCost = 0;        //部品原価の合計
            table.SumGrandTotalCost = 0;        //原価の総合計
            table.SumTotalServiceProfits = 0;   //工賃粗利の合計
            table.SumTotalPartsProfits = 0;     //部品粗利の合計
            table.SumGrandTotalProfits = 0;     //粗利の総合計


            foreach (var ret in Ret)
            {
                GetServiceSalesReportResult line = new GetServiceSalesReportResult();
                line.SalesDate = ret.SalesDate;
                line.QuoteDate = ret.QuoteDate;
                line.ArrivalPlanDate = ret.ArrivalPlanDate;
                line.WorkingEndDate = ret.WorkingEndDate;
                line.SlipNumber = ret.SlipNumber;
                line.ServiceWorkCode = ret.ServiceWorkCode;
                line.WorkTypeName = ret.WorkTypeName;
                line.ServiceWorkName = ret.ServiceWorkName;
                line.DepartmentCode = ret.DepartmentCode;
                line.DepartmentName = ret.DepartmentName;
                line.ServiceAmount = ret.ServiceAmount;
                line.PartsAmount = ret.PartsAmount;
                line.TaxAmount = ret.TaxAmount;
                line.TotalAmount = ret.TotalAmount;
                line.ServiceCost = ret.ServiceCost;
                line.PartsCost = ret.PartsCost;
                line.TotalCost = ret.TotalCost;
                line.ServiceProfits = ret.ServiceProfits;
                line.PartsProfits = ret.PartsProfits;
                line.TotalProfits = ret.TotalProfits;
                line.FrontEmployeeName = ret.FrontEmployeeName;
                line.MechanicEmployeeName = ret.MechanicEmployeeName.Replace("\r\n", "<br />");
                line.CustomerClaimName = ret.CustomerClaimName;


                //工賃売上の集計
                if (ret.ServiceAmount != 0)
                {
                    table.SumTotalServiceAmount += ret.ServiceAmount;
                }
                //部品売上の集計
                if (ret.PartsAmount != 0)
                {
                    table.SumTotalPartsAmount += ret.PartsAmount;
                }
                //消費税の集計
                if (ret.TaxAmount != 0)
                {
                    table.SumTotalTaxAmount += ret.TaxAmount;
                }
                //売上の集計
                if (ret.TotalAmount != 0)
                {
                    table.SumGrandTotalAmount += ret.TotalAmount;
                }
                //工賃原価の集計
                if (ret.ServiceCost != 0)
                {
                    table.SumTotalServiceCost += ret.ServiceCost;
                }
                //部品原価の集計
                if (ret.PartsCost != 0)
                {
                    table.SumTotalPartsCost += ret.PartsCost;
                }
                //原価の集計
                if (ret.TotalCost != 0)
                {
                    table.SumGrandTotalCost += ret.TotalCost;
                }
                //工賃粗利の集計
                if (ret.ServiceProfits != 0)
                {
                    table.SumTotalServiceProfits += ret.ServiceProfits;
                }
                //部品粗利の集計
                if (ret.PartsProfits != 0)
                {
                    table.SumTotalPartsProfits += ret.PartsProfits;
                }
                //粗利の集計
                if (ret.TotalProfits != 0)
                {
                    table.SumGrandTotalProfits += ret.TotalProfits;
                }
                
                List.Add(line);
            }
            table.list = List;

            return table;

        }


        // Add 2015/09/11 arc nakayama #3165_サービス集計表
        /// <summary>
        /// サービス集計表の検索(Excel用)
        /// </summary>
        /// <param name="DepartmentCondition">サービス集計表検索条件</param>
        /// <returns>サービス集計表検索結果</returns>
        public List<ServiceSalesReportExcelResult> GetSummaryByConditionForExcel(ServiceSalesSearchCondition condition)
        {
            // ストプロ実行
            var Ret = db.GetServiceSalesReport(string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),//納車日From
                                               string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),  //納車日To
                                               condition.WorkTypeCode,  //区分（社内/社外）
                                               condition.DepartmentCode //部門コード
                                               );

            List<ServiceSalesReportExcelResult> list = new List<ServiceSalesReportExcelResult>();

            //初期化
            foreach (var ret in Ret)
            {
                ServiceSalesReportExcelResult ExcelResult = new ServiceSalesReportExcelResult();
                ExcelResult.SalesDate = string.Format("{0:yyyy/MM/dd}",ret.SalesDate);
                ExcelResult.QuoteDate = string.Format("{0:yyyy/MM/dd}",ret.QuoteDate);
                ExcelResult.ArrivalPlanDate = string.Format("{0:yyyy/MM/dd}",ret.ArrivalPlanDate);
                ExcelResult.WorkingEndDate = string.Format("{0:yyyy/MM/dd hh:mm:ss}", ret.WorkingEndDate);
                ExcelResult.SlipNumber = ret.SlipNumber;
                ExcelResult.ServiceWorkCode = ret.ServiceWorkCode;
                ExcelResult.WorkTypeName = ret.WorkTypeName;
                ExcelResult.ServiceWorkName = ret.ServiceWorkName;
                ExcelResult.DepartmentCode = ret.DepartmentCode;
                ExcelResult.DepartmentName = ret.DepartmentName;
                ExcelResult.ServiceAmount = string.Format("{0:N0}",ret.ServiceAmount);
                ExcelResult.PartsAmount = string.Format("{0:N0}",ret.PartsAmount);
                ExcelResult.TaxAmount = string.Format("{0:N0}",ret.TaxAmount);
                ExcelResult.TotalAmount = string.Format("{0:N0}",ret.TotalAmount);
                ExcelResult.ServiceCost = string.Format("{0:N0}",ret.ServiceCost);
                ExcelResult.PartsCost = string.Format("{0:N0}",ret.PartsCost);
                ExcelResult.TotalCost = string.Format("{0:N0}",ret.TotalCost);
                ExcelResult.ServiceProfits = string.Format("{0:N0}",ret.ServiceProfits);
                ExcelResult.PartsProfits = string.Format("{0:N0}",ret.PartsProfits);
                ExcelResult.TotalProfits = string.Format("{0:N0}",ret.TotalProfits);
                ExcelResult.FrontEmployeeName = ret.FrontEmployeeName;
                ExcelResult.MechanicEmployeeName = ret.MechanicEmployeeName;
                ExcelResult.CustomerClaimName = ret.CustomerClaimName;

                list.Add(ExcelResult);
            }

            return list;

        }

    }
}
