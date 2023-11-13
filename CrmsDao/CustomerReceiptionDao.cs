using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace CrmsDao
{
    /// <summary>
    /// 顧客受付テーブルアクセスクラス
    ///   顧客受付テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CustomerReceiptionDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CustomerReceiptionDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 顧客受付テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="carReceiptionId">ユニークID</param>
        /// <returns>顧客受付テーブルデータ(1件)</returns>
        public CustomerReceiption GetByKey(Guid carReceiptionId) {
            // 顧客受付データの取得
            CustomerReceiption customerReceiption =
                (from a in db.CustomerReceiption
                 where a.CarReceiptionId.Equals(carReceiptionId)
                 select a
                ).FirstOrDefault();

            // 顧客受付データの返却
            return customerReceiption;
        }

        /// <summary>
        /// 顧客受付テーブルデータ取得(顧客指定)
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <returns>顧客受付テーブルデータ</returns>
        public List<CustomerReceiption> GetHistoryByCustomer(string customerCode) {
            // 顧客受付データの取得
            List<CustomerReceiption> ret =
                (from a in db.CustomerReceiption
                 where (a.CustomerCode.Equals(customerCode))
                 && (a.DelFlag.Equals("0"))
                 orderby a.ReceiptionDate descending, a.LastUpdateDate descending
                 select a
                ).ToList<CustomerReceiption>();

            // 顧客受付データの返却
            return ret;
        }

        /// <summary>
        /// 顧客受付データを検索する
        /// (ページング非対応)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<CustomerReceiption> GetListByCondition(DocumentExportCondition condition) {
            CustomerReceiption receiptionCondition = new CustomerReceiption();
            receiptionCondition.DepartmentCode = condition.DepartmentCode;
            receiptionCondition.EmployeeCode = condition.EmployeeCode;
            receiptionCondition.ReceiptionDateFrom = condition.TermFrom;
            receiptionCondition.ReceiptionDateTo = condition.TermTo;
            receiptionCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(receiptionCondition).ToList();
        }

        /// <summary>
        /// 顧客受付検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private IQueryable<CustomerReceiption> GetQueryable(CustomerReceiption condition) {
            string departmentCode = condition.DepartmentCode;
            string employeeCode = condition.EmployeeCode;
            DateTime? receiptDateFrom = condition.ReceiptionDateFrom;
            DateTime? receiptDateTo = condition.ReceiptionDateTo;

            var query =
                from a in db.CustomerReceiption
                orderby a.CreateDate descending, a.DepartmentCode
                where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(employeeCode) || a.EmployeeCode.Equals(employeeCode))
                && (receiptDateFrom == null || DateTime.Compare(a.ReceiptionDate, receiptDateFrom ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                && (receiptDateTo == null || DateTime.Compare(a.ReceiptionDate, receiptDateTo ?? DaoConst.SQL_DATETIME_MAX) <= 0)
                select a;

            ParameterExpression param = Expression.Parameter(typeof(CustomerReceiption), "x");
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<CustomerReceiption, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<CustomerReceiption, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<CustomerReceiption, bool>>(comExpression, param));
            }

            return query;
        }
    }
}
