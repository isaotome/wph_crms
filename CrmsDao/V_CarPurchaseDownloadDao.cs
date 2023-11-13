using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_CarPurchaseDownloadDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarPurchaseDownloadDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        #region 検索
        
        /// <summary>
        /// 車両仕入リストの一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_CarPurchaseDownload> GetListCarPurchase(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            IQueryable<V_CarPurchaseDownload> query =
                        from a in db.V_CarPurchaseDownload
                        where
                           (condition.PurchaseDateFrom == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, condition.PurchaseDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                        && (condition.PurchaseDateTo == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, condition.PurchaseDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        && (string.IsNullOrEmpty(condition.DepartmentCode) || a.PurchaseLocationCode.Equals(condition.DepartmentCode))
                        && (string.IsNullOrEmpty(condition.PurchaseStatus) || a.PurchaseStatus.Equals(condition.PurchaseStatus))
                        orderby a.PurchaseDate
                        select a;


            return new PaginatedList<V_CarPurchaseDownload>(query, pageIndex ?? 0, pageSize ?? 0);
        }
        #endregion

        #region CSVデータ取得
        /// <summary>
        /// 車両仕入リストのCSVデータ検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarPurchaseDownload> GetListCarPurchaseCsv(JournalExPortsCondition condition)
        {
            IQueryable<V_CarPurchaseDownload> query =
                        from a in db.V_CarPurchaseDownload
                        where
                           (condition.PurchaseDateFrom == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, condition.PurchaseDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                        && (condition.PurchaseDateTo == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, condition.PurchaseDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        && (string.IsNullOrEmpty(condition.DepartmentCode) || a.PurchaseLocationCode.Equals(condition.DepartmentCode))
                        && (string.IsNullOrEmpty(condition.PurchaseStatus) || a.PurchaseStatus.Equals(condition.PurchaseStatus))
                        orderby a.PurchaseDate
                        select a;


            return query.ToList<V_CarPurchaseDownload>();
        }
        #endregion
    }
}
