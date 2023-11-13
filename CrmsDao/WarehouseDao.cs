using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// 倉庫マスタデータ取得クラス
    /// </summary>
    /// <history>
    /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
    /// </history>
    public class WarehouseDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public WarehouseDao(CrmsLinqDataContext context)
        {
            db = context;
        }
        /// <summary>
        /// 倉庫マスタを検索する
        /// </summary>
        /// <param name="WarehouseCondition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<Warehouse> GetByCondition(Warehouse WarehouseCondition, int pageIndex, int pageSize)
        {
            var query = from a in db.Warehouse
                        where (string.IsNullOrEmpty(WarehouseCondition.WarehouseCode) || a.WarehouseCode.Contains(WarehouseCondition.WarehouseCode))
                        && (string.IsNullOrEmpty(WarehouseCondition.WarehouseName) || a.WarehouseName.Contains(WarehouseCondition.WarehouseName))
                        && (string.IsNullOrEmpty(WarehouseCondition.DepartmentCode) || a.DepartmentCode.Equals(WarehouseCondition.DepartmentCode))
                        && (string.IsNullOrEmpty(WarehouseCondition.DepartmentName) || a.Department.DepartmentName.Contains(WarehouseCondition.DepartmentName))
                        && (string.IsNullOrEmpty(WarehouseCondition.DelFlag) || a.DelFlag.Equals(WarehouseCondition.DelFlag))
                        select a;

            PaginatedList<Warehouse> list = new PaginatedList<Warehouse>(query, pageIndex, pageSize);
            for (int i = 0; i < list.Count(); i++)
            {
                list[i] = EditModel(list[i]);
            }
            return list;
        }

        /// <summary>
        /// 倉庫マスタを取得する（PK指定）
        /// </summary>
        /// <param name="WarehouseCode"></param>
        /// <returns></returns>
        public Warehouse GetByKey(string WarehouseCode, bool includeDeleted = false)
        {
            var Warehouse = (from a in db.Warehouse
                             where a.WarehouseCode.Equals(WarehouseCode)
                             && ((includeDeleted) || a.DelFlag.Equals("0"))
                             select a).FirstOrDefault();
            return Warehouse;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="brand">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Warehouse EditModel(Warehouse Warehouse)
        {
            // 内部コード項目の名称情報取得
            Warehouse.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, Warehouse.DelFlag);

            // 出口
            return Warehouse;
        }
    }
}
