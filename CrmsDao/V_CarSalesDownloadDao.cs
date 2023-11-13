using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class V_CarSalesDownloadDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarSalesDownloadDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        #region 検索
        
        /// <summary>
        /// 中古車売上(AA除く)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_CarSalesDownload> GetListByUserdCarNAA(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {
            // サービス受付対象データの取得
            IOrderedQueryable<V_CarSalesDownload> v_CarSalesList =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("U"))
                    && (!a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            // ページング制御情報を付与したサービス受付対象データの返却
            return new PaginatedList<V_CarSalesDownload>(v_CarSalesList, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// 新車売上の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_CarSalesDownload> GetListByNewCar(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {

            // サービス受付対象データの取得
            IOrderedQueryable<V_CarSalesDownload> v_CarSalesList =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("N"))
                    && (!a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            // ページング制御情報を付与したサービス受付対象データの返却
            return new PaginatedList<V_CarSalesDownload>(v_CarSalesList, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// 中古車売上(AA)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<V_CarSalesDownload> GetListByUserdCarAutoAuction(JournalExPortsCondition condition, int? pageIndex, int? pageSize)
        {

            // サービス受付対象データの取得
            IOrderedQueryable<V_CarSalesDownload> v_CarSalesList =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("U"))
                    && (a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            // ページング制御情報を付与したサービス受付対象データの返却
            return new PaginatedList<V_CarSalesDownload>(v_CarSalesList, pageIndex ?? 0, pageSize ?? 0);
        }
        #endregion

        #region CSVデータ取得
        /// <summary>
        /// 中古車売上(AA除く)のCSVデータ取得
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarSalesDownload> GetListUserdCarCsvNAA(JournalExPortsCondition condition)
        {
            IOrderedQueryable<V_CarSalesDownload> query =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("U"))
                    && (!a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            return query.ToList<V_CarSalesDownload>();
        }

        /// <summary>
        /// 新車売上の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarSalesDownload> GetListNewCarCsv(JournalExPortsCondition condition)
        {
            IOrderedQueryable<V_CarSalesDownload> query =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("N"))
                    && (!a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(condition.DivisionType) || a.BrandStoreCode.Equals(condition.DivisionType))
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            return query.ToList<V_CarSalesDownload>();
        }

        /// <summary>
        /// 中古車売上(AA)の一覧検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<V_CarSalesDownload> GetListUserdCarAutoAuctionCsv(JournalExPortsCondition condition)
        {

            IOrderedQueryable<V_CarSalesDownload> query =
                    from a in db.V_CarSalesDownload
                    where
                    (a.NewUsedType.Equals("U"))
                    && (a.CustomerType.Equals("201"))
                    && (condition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, condition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (condition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, condition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    orderby a.DepartmentCode, a.Name
                    , a.SalesDate
                    select a;

            return query.ToList<V_CarSalesDownload>();
        }
        #endregion
    }
}
