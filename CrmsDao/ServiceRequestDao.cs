using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Linq.Expressions;
//Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao {
    public class ServiceRequestDao {
        private CrmsLinqDataContext db;
        public ServiceRequestDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 車両伝票番号から作業依頼データを1件抽出する
        /// </summary>
        /// <param name="slipNumber">車両伝票番号</param>
        /// <returns>作業依頼データ</returns>
        public ServiceRequest GetBySlipNumber(string slipNumber) {
            var query =
                (from a in db.ServiceRequest
                where a.OriginalSlipNumber.Equals(slipNumber)
                && !a.DelFlag.Equals("1")
                select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 作業依頼リストデータを取得する
        /// (見積作成済みのものは除外)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        /// 2019/01/21 yano #3965 WE版新システム対応
        /// </history>
        public PaginatedList<ServiceRequest> GetListByCondition(ServiceRequest condition,int pageIndex,int pageSize) {

            //依頼部門(CarSalesHeaderを検索)
            string departmentCode = condition.DepartmentCode;
            //依頼担当者(CarSalesHeaderを検索)
            string employeeCode = condition.EmployeeCode;
            //顧客名(CarSalesHeaderを検索)
            string customerName = condition.CustomerName;
            //車台番号(CarSalesHeaderを検索)
            string vin = condition.Vin;
            
            //入庫予定日(CarPurchaseOrderを検索)
            DateTime? arrivalPlanDateFrom = condition.ArrivalPlanDateFrom;
            DateTime? arrivalPlanDateTo = condition.ArrivalPlanDateTo;

            //希望納期
            DateTime? deliveryRequirementFrom = condition.DeliveryRequirementFrom;
            DateTime? deliveryRequirementTo = condition.DeliveryRequirementTo;

            //Add 2017/02/23 arc nakayama #3626_【車】車両伝票の「作業依頼書」へ受注後に追加されない　新規作成
            //Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var RetList = db.GetServiceRequestList(departmentCode
                                                   , employeeCode
                                                   , customerName
                                                   , vin
                                                   , string.Format("{0:yyyy/MM/dd}",arrivalPlanDateFrom)
                                                   , string.Format("{0:yyyy/MM/dd}",arrivalPlanDateTo)
                                                   , string.Format("{0:yyyy/MM/dd}",deliveryRequirementFrom)
                                                   , string.Format("{0:yyyy/MM/dd}",deliveryRequirementTo)
                                                   );

                List<ServiceRequest> list = new List<ServiceRequest>();
                foreach(var RetData in RetList){
                    ServiceRequest SrData = new ServiceRequest();
                    //Mod 2019/01/21 yano #3965
                    SrData.Department = new DepartmentDao(db).GetByKey(RetData.DepartmentCode, true);
                    if (SrData.Department == null)
                    {
                        SrData.Department = new Department();
                        SrData.Department.DepartmentCode = RetData.DepartmentCode;
                        SrData.Department.DepartmentName = RetData.DepartmentName;
                    }

                    SrData.CarSalesHeader = new CarSalesHeader();
                    SrData.CarSalesHeader.Employee = new Employee();
                    SrData.CarSalesHeader.Customer = new Customer();
                    SrData.CarPurchaseOrder = new CarPurchaseOrder();
                    SrData.CarSalesHeader.Employee.EmployeeName = RetData.EmployeeName;
                    SrData.OriginalSlipNumber = RetData.OriginalSlipNumber;
                    SrData.CarSalesHeader.Customer.CustomerName = RetData.CustomerName;
                    SrData.CarSalesHeader.CarName = RetData.CarName;
                    SrData.CarPurchaseOrder.ArrivalPlanDate = RetData.ArrivalPlanDate;
                    SrData.DeliveryRequirement = RetData.DeliveryRequirement;
                    SrData.Memo = RetData.Memo;

                    list.Add(SrData);
                }

                #region 古いクエリ
                //var query =
                //    from a in db.ServiceRequest
                //    join b in db.CarPurchaseOrder on a.OriginalSlipNumber equals b.SlipNumber into ord
                //    from b in ord.DefaultIfEmpty()
                //    join d in db.CarSalesHeader on new { b.SlipNumber, DelFlag = "0" } equals new { d.SlipNumber, d.DelFlag } into sales
                //    from d in sales.DefaultIfEmpty()
                //    where (arrivalPlanDateFrom == null || DateTime.Compare(b.ArrivalPlanDate ?? DaoConst.SQL_DATETIME_MIN, arrivalPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                //        && (arrivalPlanDateTo == null || DateTime.Compare(b.ArrivalPlanDate ?? DaoConst.SQL_DATETIME_MAX, arrivalPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                //        && (deliveryRequirementFrom == null || DateTime.Compare(a.DeliveryRequirement ?? DaoConst.SQL_DATETIME_MIN, deliveryRequirementFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                //        && (deliveryRequirementTo == null || DateTime.Compare(a.DeliveryRequirement ?? DaoConst.SQL_DATETIME_MAX, deliveryRequirementTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                //        && (string.IsNullOrEmpty(departmentCode) || d.DepartmentCode.Equals(departmentCode))
                //        && (string.IsNullOrEmpty(employeeCode) || d.EmployeeCode.Equals(employeeCode))
                //        && (string.IsNullOrEmpty(vin) || d.Vin.Contains(vin))
                //        && (string.IsNullOrEmpty(customerName) || d.Customer.CustomerName.Contains(customerName))
                //        //&& (string.IsNullOrEmpty(authCompanyCode) || a.Department.Office.CompanyCode.Equals(authCompanyCode) || d.Department.Office.CompanyCode.Equals(authCompanyCode))
                //        //&& ((string.IsNullOrEmpty(authOfficeCode) || a.Department.OfficeCode.Equals(authOfficeCode) || d.Department.OfficeCode.Equals(authOfficeCode))
                //        //|| (string.IsNullOrEmpty(authOfficeCode1) || a.Department.OfficeCode.Equals(authOfficeCode1) || d.Department.OfficeCode.Equals(authOfficeCode1))
                //        //|| (string.IsNullOrEmpty(authOfficeCode2) || a.Department.OfficeCode.Equals(authOfficeCode2) || d.Department.OfficeCode.Equals(authOfficeCode2))
                //        //|| (string.IsNullOrEmpty(authOfficeCode3) || a.Department.OfficeCode.Equals(authOfficeCode3) || d.Department.OfficeCode.Equals(authOfficeCode3)))
                //        //&& ((string.IsNullOrEmpty(authDepartmentCode) || a.DepartmentCode.Equals(authDepartmentCode) || d.DepartmentCode.Equals(authDepartmentCode))
                //        //|| (string.IsNullOrEmpty(authDepartmentCode1) || a.DepartmentCode.Equals(authDepartmentCode1) || d.DepartmentCode.Equals(authDepartmentCode1))
                //        //|| (string.IsNullOrEmpty(authDepartmentCode2) || a.DepartmentCode.Equals(authDepartmentCode2) || d.DepartmentCode.Equals(authDepartmentCode2))
                //        //|| (string.IsNullOrEmpty(authDepartmentCode3) || a.DepartmentCode.Equals(authDepartmentCode3) || d.DepartmentCode.Equals(authDepartmentCode3)))
                //        && !(
                //                from c in db.ServiceSalesHeader
                //                where c.CarSlipNumber != null
                //                select c.CarSlipNumber
                //            ).Contains(a.OriginalSlipNumber)

                //    select a;
                #endregion

                IQueryable<ServiceRequest> query = list.AsQueryable<ServiceRequest>();

                ParameterExpression param = Expression.Parameter(typeof(ServiceRequest), "x");
                Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
                if (depExpression != null)
                {
                    query = query.Where(Expression.Lambda<Func<ServiceRequest, bool>>(depExpression, param));
                }
                Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
                if (offExpression != null)
                {
                    query = query.Where(Expression.Lambda<Func<ServiceRequest, bool>>(offExpression, param));
                }
                Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
                if (comExpression != null)
                {
                    query = query.Where(Expression.Lambda<Func<ServiceRequest, bool>>(comExpression, param));
                }

                return new PaginatedList<ServiceRequest>(query, pageIndex, pageSize);
            }
        }

        /// <summary>
        /// 作業以来IDから作業依頼明細データを抽出する
        /// </summary>
        /// <param name="slipNumber">作業以来ID</param>
        /// <returns>作業依頼明細データ</returns>
        public List<ServiceRequestLine> GetLineByServiceRequestId(string ServiceRequestId)
        {
            var query =
                 from a in db.ServiceRequestLine
                 where a.ServiceRequestId.Equals(ServiceRequestId)
                 && a.DelFlag.Equals("0")
                 select a;
            return query.ToList();
        }
    }
}
