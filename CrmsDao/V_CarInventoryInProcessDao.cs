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
    /// 車両棚卸ビューアクセスクラス
    ///   車両棚卸ビューの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class V_CarInventoryInProcessDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarInventoryInProcessDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 車両棚卸ビューデータ検索
        /// </summary>
        /// <param name="v_CarInventoryInProcessCondition">車両棚卸検索条件</param>
        /// <returns>車両棚卸ビューデータ検索結果</returns>
        public List<V_CarInventoryInProcess> GetListByCondition(V_CarInventoryInProcess v_CarInventoryInProcessCondition) {

            string departmentCode = v_CarInventoryInProcessCondition.DepartmentCode;
            DateTime? inventoryMonth = v_CarInventoryInProcessCondition.InventoryMonth;

            // 車両棚卸データの取得
            var query =
                (from a in db.V_CarInventoryInProcess
                 where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (a.InventoryMonth.Equals(inventoryMonth))
                 select a);
            if (v_CarInventoryInProcessCondition.DefferenceSelect) {
                ParameterExpression param = Expression.Parameter(typeof(V_CarInventoryInProcess), "x");
                System.Linq.Expressions.BinaryExpression body = Expression.NotEqual(Expression.Property(param, "DifferentialQuantity"), Expression.Constant(0m, typeof(decimal?)));
                query = query.Where(Expression.Lambda<Func<V_CarInventoryInProcess, bool>>(body, param));
            }
            return query.OrderBy(x => x.LocationCode).ThenBy(x => x.MakerCode).ThenBy(x => x.CarBrandCode).ThenBy(x => x.CarCode).ThenBy(x => x.CarGradeCode).ThenBy(x => x.Vin).ThenBy(x => x.CarStatus).ToList<V_CarInventoryInProcess>();
        }
    }
}