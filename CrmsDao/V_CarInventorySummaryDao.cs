using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace CrmsDao {

    /// <summary>
    /// 車両棚卸集計ビューアクセスクラス
    ///   車両棚卸集計ビューの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class V_CarInventorySummaryDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarInventorySummaryDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 車両棚卸集計ビューデータ検索
        /// </summary>
        /// <param name="v_CarInventorySummaryCondition">車両棚卸集計検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両棚卸集計ビューデータ検索結果</returns>
        public PaginatedList<V_CarInventorySummary> GetListByCondition(V_CarInventorySummary v_CarInventorySummaryCondition, int? pageIndex, int? pageSize) {

            string companyCode = v_CarInventorySummaryCondition.CompanyCode;
            string officeCode = v_CarInventorySummaryCondition.OfficeCode;
            string departmentCode = v_CarInventorySummaryCondition.DepartmentCode;
            DateTime inventoryMonth = v_CarInventorySummaryCondition.InventoryMonth;

            // 車両棚卸集計データの取得
            var query =
                 from a in db.V_CarInventorySummary
                 where (string.IsNullOrEmpty(companyCode) || a.CompanyCode.Equals(companyCode))
                 && (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Equals(officeCode))
                 && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                 && (a.InventoryMonth.Equals(inventoryMonth))
                 select a;

            ParameterExpression param = Expression.Parameter(typeof(V_CarInventorySummary), "x");
            Expression depExpression = v_CarInventorySummaryCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<V_CarInventorySummary, bool>>(depExpression, param));
            }
            Expression offExpression = v_CarInventorySummaryCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<V_CarInventorySummary, bool>>(offExpression, param));
            }
            Expression comExpression = v_CarInventorySummaryCondition.CreateExpressionForCompany(param, new string[] { "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<V_CarInventorySummary, bool>>(comExpression, param));
            }
            query = query.OrderBy(x => x.CompanyCode).ThenBy(x => x.OfficeCode).ThenBy(x => x.DepartmentCode);

            // ページング制御情報を付与したサービス受付対象データの返却
            return new PaginatedList<V_CarInventorySummary>(query, pageIndex ?? 0, pageSize ?? 0);
        }
    }
}
