using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// 部門・倉庫組合せマスタデータ取得クラス
    /// </summary>
    /// <history>
    /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    /// </history>
    public class DepartmentWarehouseDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public DepartmentWarehouseDao(CrmsLinqDataContext context)
        {
            db = context;
        }
        /// <summary>
        /// 部門・倉庫組合せマスタを検索する
        /// </summary>
        /// <param name="WarehouseCondition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>検索結果一覧(ページ情報付)</returns>
        public PaginatedList<DepartmentWarehouse> GetByCondition(DepartmentWarehouse Condition, int pageIndex, int pageSize)
        {
            var query = from a in db.DepartmentWarehouse
                        where (string.IsNullOrEmpty(Condition.DepartmentCode) || a.DepartmentCode.Equals(Condition.DepartmentCode))
                        && (string.IsNullOrEmpty(Condition.DepartmentName) || a.Department.DepartmentName.Contains(Condition.DepartmentName))
                        && (string.IsNullOrEmpty(Condition.WarehouseCode) || a.WarehouseCode.Contains(Condition.WarehouseCode))
                        && (string.IsNullOrEmpty(Condition.WarehouseName) || a.Warehouse.WarehouseName.Contains(Condition.WarehouseName))
                        && (string.IsNullOrEmpty(Condition.DelFlag) || a.DelFlag.Equals(Condition.DelFlag))
                        select a;

            PaginatedList<DepartmentWarehouse> list = new PaginatedList<DepartmentWarehouse>(query, pageIndex, pageSize);
            for (int i = 0; i < list.Count(); i++)
            {
                list[i] = EditModel(list[i]);
            }
            return list;
        }

        /// <summary>
        /// 部門・倉庫組合せマスタを検索する
        /// </summary>
        /// <param name="WarehouseCondition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>検索結果一覧</returns>
        public List<DepartmentWarehouse> GetByCondition(DepartmentWarehouse Condition)
        {
            var query = from a in db.DepartmentWarehouse
                        where (string.IsNullOrEmpty(Condition.DepartmentCode) || a.DepartmentCode.Equals(Condition.DepartmentCode))
                        && (string.IsNullOrEmpty(Condition.DepartmentName) || a.Department.DepartmentName.Contains(Condition.DepartmentName))
                        && (string.IsNullOrEmpty(Condition.WarehouseCode) || a.WarehouseCode.Contains(Condition.WarehouseCode))
                        && (string.IsNullOrEmpty(Condition.WarehouseName) || a.Warehouse.WarehouseName.Contains(Condition.WarehouseName))
                        && (string.IsNullOrEmpty(Condition.DelFlag) || a.DelFlag.Equals(Condition.DelFlag))
                        select a;

            return query.ToList();
        }

        /// <summary>
        /// 部門・倉庫組合せマスタを取得する（PK指定）
        /// </summary>
        /// <param name="WarehouseCode"></param>
        /// <returns></returns>
        public DepartmentWarehouse GetByKey(string DepartmentCode, string WarehouseCode, bool includeDeleted = false)
        {
            var DepartmentWarehouse = (from a in db.DepartmentWarehouse
                                       where a.DepartmentCode.Equals(DepartmentCode)
                             && a.WarehouseCode.Equals(WarehouseCode)
                             && ((includeDeleted) || a.DelFlag.Equals("0"))
                             select a).FirstOrDefault();

            return DepartmentWarehouse;
        }

        /// <summary>
        /// 部門・倉庫組合せマスタを取得する(部門指定)
        /// </summary>
        /// <param name="DepartmentCode">部門コード</param>
        /// <param name="includeDeleted">削除データも含むかどうか(true:含む false:含まない)</param>
        /// <returns></returns>
        public DepartmentWarehouse GetByDepartment(string DepartmentCode, bool includeDeleted = false)
        {
            var DepartmentWarehouse = (from a in db.DepartmentWarehouse
                                       where a.DepartmentCode.Equals(DepartmentCode)
                                       && ((includeDeleted) || a.DelFlag.Equals("0"))
                                       select a).FirstOrDefault();
            return DepartmentWarehouse;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="brand">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private DepartmentWarehouse EditModel(DepartmentWarehouse DepartmentWarehouse)
        {
            // 内部コード項目の名称情報取得
            DepartmentWarehouse.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, DepartmentWarehouse.DelFlag);

            // 出口
            return DepartmentWarehouse;
        }
    }
}
