using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao {
    public class PaymentPlanDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public PaymentPlanDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 支払予定を検索する
        /// (ページング対応)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<PaymentPlan> GetListByCondition(PaymentPlan condition,int pageIndex,int pageSize) {
            return new PaginatedList<PaymentPlan>(GetQueryable(condition), pageIndex, pageSize);
        }
        /// <summary>
        /// 支払予定を検索する
        /// (ページング非対応)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<PaymentPlan> GetListByCondition(DocumentExportCondition condition){
            PaymentPlan paymentPlanCondition = new PaymentPlan();
            paymentPlanCondition.PaymentPlanDateFrom = condition.TermFrom;
            paymentPlanCondition.PaymentPlanDateTo = condition.TermTo;
            paymentPlanCondition.DepartmentCode = condition.DepartmentCode;
            paymentPlanCondition.AccountCode = condition.PaymentPlanType;
            paymentPlanCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(paymentPlanCondition).ToList();
        }

        /// <summary>
        /// 支払予定の検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 検索条件に発注番号を追加
        /// </history>
        private IQueryable<PaymentPlan> GetQueryable(PaymentPlan condition){
            string supplierPaymentCode = null;
            try { supplierPaymentCode = condition.SupplierPayment.SupplierPaymentCode; } catch (NullReferenceException) { }
            string supplierPaymentType = null;
            try { supplierPaymentType = condition.SupplierPayment.SupplierPaymentType; } catch (NullReferenceException) { }
            DateTime paymentPlanDateFrom = condition.PaymentPlanDateFrom ?? DaoConst.SQL_DATETIME_MIN;
            DateTime paymentPlanDateTo = condition.PaymentPlanDateTo ?? DaoConst.SQL_DATETIME_MAX;
            string accountCode = condition.AccountCode;
            string departmentCode = condition.DepartmentCode;
            string purchaseOrderNumber = condition.PurchaseOrderNumber;

            var query =
                from a in db.PaymentPlan
                where (string.IsNullOrEmpty(supplierPaymentCode) || a.SupplierPaymentCode.Equals(supplierPaymentCode))
                && (string.IsNullOrEmpty(supplierPaymentType) || a.SupplierPayment.SupplierPaymentType.Equals(supplierPaymentType))
                && ((paymentPlanDateFrom == null) || DateTime.Compare(a.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX,paymentPlanDateFrom)>=0)
                && ((paymentPlanDateTo == null) || DateTime.Compare(a.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN,paymentPlanDateTo)<=0)
                && (string.IsNullOrEmpty(accountCode) || a.AccountCode.Equals(accountCode))
                && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(purchaseOrderNumber) || a.DepartmentCode.Equals(purchaseOrderNumber))
                && (a.CompleteFlag.Equals("0"))
                && (a.DelFlag.Equals("0"))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(PaymentPlan), "x");
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<PaymentPlan, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<PaymentPlan, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<PaymentPlan, bool>>(comExpression, param));
            }
            return query.OrderBy(x => x.PaymentPlanDate);

        }

        /// <summary>
        /// 支払予定データを取得する(PK指定)
        /// </summary>
        /// <param name="paymentPlanId">支払予定ID</param>
        /// <returns>支払予定データ</returns>
        public PaymentPlan GetByKey(Guid paymentPlanId) {
            PaymentPlan query =
                (from a in db.PaymentPlan
                 where a.PaymentPlanId.Equals(paymentPlanId)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 残高合計額取得
        /// </summary>
        /// <param name="receiptPlanCondition">支払予定検索条件</param>
        /// <returns>残高合計額</returns>
        public decimal GetTotalBalance(PaymentPlan paymentPlanCondition) {

            string departmentCode = null;
            try { departmentCode = paymentPlanCondition.DepartmentCode; } catch (NullReferenceException) { }
            string occurredDepartmentCode = null;
            try { occurredDepartmentCode = paymentPlanCondition.Department.DepartmentCode; } catch (NullReferenceException) { }
            DateTime? receiptPlanDateFrom = paymentPlanCondition.PaymentPlanDateFrom;
            DateTime? receiptPlanDateTo = paymentPlanCondition.PaymentPlanDateTo;
            string slipNumber = paymentPlanCondition.SlipNumber;
            string supplierPaymentCode = null;
            try { supplierPaymentCode = paymentPlanCondition.SupplierPayment.SupplierPaymentCode; } catch (NullReferenceException) { }

            // 残高合計額の取得
            IQueryable<decimal> amountList =
                (from a in db.PaymentPlan
                 where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                 && (string.IsNullOrEmpty(occurredDepartmentCode) || a.OccurredDepartmentCode.Equals(occurredDepartmentCode))
                 && (receiptPlanDateFrom == null || DateTime.Compare(a.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MIN, receiptPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                 && (receiptPlanDateTo == null || DateTime.Compare(a.PaymentPlanDate ?? DaoConst.SQL_DATETIME_MAX, receiptPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                 && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Equals(slipNumber))
                 && (string.IsNullOrEmpty(supplierPaymentCode) || a.SupplierPaymentCode.Equals(supplierPaymentCode))
                 && (a.CompleteFlag.Equals("0"))
                 && (a.DelFlag.Equals("0"))
                 select a.PaymentableBalance ?? 0m
                );
            decimal detailsTotal = (amountList.Count<decimal>() > 0 ? amountList.Sum() : 0m);

            // 残高合計額の返却
            return detailsTotal;
        }

        /// <summary>
        /// 発注番号からデータを取得する
        /// </summary>
        /// <param name="paymentPlanId">支払予定ID</param>
        /// <returns>支払予定データ</returns>
        /// <shitory><
        public PaymentPlan GetByPuchaseOrderNumber(string puchaseOrdreNumer)
        {
            PaymentPlan query =
                (
                 from a in db.PaymentPlan
                 where (string.IsNullOrWhiteSpace(puchaseOrdreNumer) || a.PurchaseOrderNumber.Equals(puchaseOrdreNumer))
                 select a
            ).FirstOrDefault();
            
            return query;
        }
    }
}
