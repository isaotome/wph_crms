using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao {
    public class PartsPurchaseOrderDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public PartsPurchaseOrderDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 部品発注データを検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        ///  2015/12/09 arc yano #3290 部品仕入機能改善(部品発注一覧) の検索条件、クエリを全面的に変更
        /// </history>
        public PaginatedList<PartsPurchaseOrder> GetListByCondition(PartsPurchaseOrder condition, int pageIndex, int pageSize) {

            //発注番号
            string purchaseOrderNumber = string.Format("{0:00000000}", condition.PurchaseOrderNumber);
            //受注伝票番号
            string serviceSlipNumber = condition.ServiceSlipNumber;
            //発注日(From)
            DateTime? purchaseOrderDateFrom = condition.PurchaseOrderDateFrom;
            //発注日(To)
            DateTime? purchaseOrderDateTo = condition.PurchaseOrderDateTo ?? purchaseOrderDateFrom;
            //発注ステータス
            string purchaseOrderStatus = condition.PurchaseOrderStatus;
            //部門コード
            string departmentCode = condition.DepartmentCode;
            //担当者コード
            string employeeCode = condition.EmployeeCode;
            //仕入先コード
            string supplierCode = condition.SupplierCode;
            //仕入先名
            string supplierName = condition.SupplierName;
            //Webオーダー番号
            string webOrderNumber = condition.WebOrderNumber;
            //純正区分
            string genuineType = condition.GenuineType;
            
            var query =
                from a in db.PartsPurchaseOrder
                orderby a.PurchaseOrderNumber descending
                where (string.IsNullOrEmpty(purchaseOrderNumber) || a.PurchaseOrderNumber.Equals(purchaseOrderNumber))
                && (string.IsNullOrEmpty(serviceSlipNumber) || a.ServiceSlipNumber.Equals(serviceSlipNumber))
                && (purchaseOrderDateFrom == null || DateTime.Compare(a.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MIN, purchaseOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (purchaseOrderDateTo == null || DateTime.Compare(a.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MAX,purchaseOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(purchaseOrderStatus) || a.PurchaseOrderStatus.Equals(purchaseOrderStatus))
                && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(employeeCode) || a.EmployeeCode.Equals(employeeCode))
                && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Equals(supplierCode))
                && (string.IsNullOrEmpty(supplierName) || a.Supplier.SupplierName.Contains(supplierName))
                && (string.IsNullOrEmpty(webOrderNumber) || a.WebOrderNumber.Contains(webOrderNumber))
                && (string.IsNullOrEmpty(genuineType) || a.Parts.GenuineType.Contains(genuineType))
                && a.DelFlag.Equals("0")
             
                select a;

            ParameterExpression param = Expression.Parameter(typeof(PartsPurchaseOrder), "x");
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchaseOrder, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchaseOrder, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchaseOrder, bool>>(comExpression, param));
            }
            return new PaginatedList<PartsPurchaseOrder>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// 部品発注データを取得する（PK指定）
        /// </summary>
        /// <param name="purchaseOrderNumber">発注番号</param>
        /// <returns>部品発注データ</returns>
        /// <hisotory>Mod 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) 検索条件として、部品番号を追加</hisotory>
        public PartsPurchaseOrder GetByKey(string purchaseOrderNumber, string partsNumber = null) {
            var query =
                (
                 from a in db.PartsPurchaseOrder
                 where a.PurchaseOrderNumber.Equals(purchaseOrderNumber)
                 && ( string.IsNullOrWhiteSpace(partsNumber) || a.PartsNumber.Equals(partsNumber))
                 select a
                ).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 部品発注番号リストに該当する部品発注リストを取得する
        /// </summary>
        /// <param name="keyList">発注番号リスト</param>
        /// <returns>部品発注リスト</returns>
        public List<PartsPurchaseOrder> GetListByKeyList(string[] keyList) {
            var query =
                from a in db.PartsPurchaseOrder
                select a;

            ParameterExpression param = Expression.Parameter(typeof(PartsPurchaseOrder), "x");
            MemberExpression left = Expression.Property(param, "PurchaseOrderNumber");
            BinaryExpression body = null;
            foreach (var b in keyList) {
                ConstantExpression right = Expression.Constant(b, typeof(string));
                if (body == null) {
                    body = Expression.Equal(left, right);
                } else {
                    body = Expression.OrElse(body, Expression.Equal(left, right));
                }
            }
            query = query.Where(Expression.Lambda<Func<PartsPurchaseOrder, bool>>(body, param));
            query = query.OrderByDescending(x => x.PurchaseOrderNumber);
            return query.ToList<PartsPurchaseOrder>();
        }

        //Add 2015/11/09 arc yano #3291　部品発注データの管理方法の変更
        /// <summary>
        /// 部品発注データを取得する（PK指定）
        /// </summary>
        /// <param name="purchaseOrderNumber">発注番号</param>
        /// <param name="includeDeleted">削除データ含む／含まないフラグ</param>
        /// <returns>部品発注データリスト/returns>
        public List<PartsPurchaseOrder> GetListByKey(string purchaseOrderNumber, bool includeDeleted = false) {
            var query =
                 from a in db.PartsPurchaseOrder
                 where a.PurchaseOrderNumber.Equals(purchaseOrderNumber)
                 && (includeDeleted || a.DelFlag.Equals("0"))
                 select a;
            return query.ToList();
        }

        /// <summary>
        /// 部品発注データを取得する（受注伝票番号・部品番号・発注区分）
        /// </summary>
        /// <param name="ServiceSlipNumber">受注伝票番号</param>
        /// <param name="PartsNumber">部品番号</param>
        /// <param name="OrderType">発注区分</param>
        /// <returns>部品発注データ</returns>
        /// <history>
        /// Mod 2016/11/11 arc yano #3656 データ取得条件の変更(発注ステータス≠未発注にする)
        /// Add 2016/05/09 arc nakayama #3513_サービス伝票入力から発注取消を行うと、伝票上の発注数が更新されない 受注伝票番号・部品番号・発注区分で検索するメソッド作成
        /// </hisotory>
        public List<PartsPurchaseOrder> GetListByKeys(string ServiceSlipNumber, string PartsNumber, string OrderType, bool includeDeleted = false)
        {
            var query =                
                 from a in db.PartsPurchaseOrder
                 where a.ServiceSlipNumber.Equals(ServiceSlipNumber)
                 && a.PartsNumber.Equals(PartsNumber)
                 && a.OrderType.Equals(OrderType)
                 && !a.PurchaseOrderStatus.Equals("001")
                 && (includeDeleted || a.DelFlag.Equals("0"))
                 select a;

            return query.ToList();
        }
    }
}
