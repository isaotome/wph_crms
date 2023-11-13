using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//Add 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao
{
    public class PartsWipStockDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public PartsWipStockDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }


        /// <summary>
        /// 部品仕掛データの検索
        /// </summary>
        /// <param name="DepartmentCondition">部品仕掛検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部品仕掛ビュー検索結果</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ストアドの引数追加(倉庫コード)
        /// 2015/09/18 arc yano 部品仕掛在庫 障害対応・仕様変更⑧ 仕掛在庫データの取得方法をビューテーブルからストアドに変更
        /// 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
        /// 2015/06/24 src nakayama 経理からの要望対応③　仕掛在庫表対応 検索項目に明細種別追加
        /// 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
        /// 2014/11/11 arc nakayama 部品仕掛在庫検索対応
        /// </history>
        public PaginatedList<InventoryParts_Shikakari> GetListByCondition(InventoryParts_Shikakari condition, int? pageIndex, int? pageSize)
        { 
            //入庫日
            string ArrivalPlanDateFrom = string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateFrom);
            string ArrivalPlanDateTo = string.Format("{0:yyyy/MM/dd}",condition.ArrivalPlanDateTo);
            
            //倉庫コードの取得
            string warehouseCode = "";

            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, condition.DepartmentCode);

            if (dWarehouse != null)
            {
                warehouseCode = dWarehouse.WarehouseCode;
            }

            // 部品仕掛データの取得
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var ret = db.GetPartsWipStock(
                                                0
                                                , condition.InventoryMonth
                                                , condition.DepartmentCode
                                                , warehouseCode
                                                , condition.ServiceType
                                                , ArrivalPlanDateFrom
                                                , ArrivalPlanDateTo
                                                , condition.SlipNumber
                                                , condition.PartsNumber
                                                , condition.LineContents1
                                                , condition.Vin
                                                , condition.CustomerName
                                                ).ToList();

                // ページング制御情報を付与した部門データの返却
                return new PaginatedList<InventoryParts_Shikakari>(ret.AsQueryable(), pageIndex ?? 0, pageSize ?? 0);
            }
        }
       
        /// <summary>
        /// 部品仕掛データの検索(Excel用)
        /// </summary>
        /// <param name="condition">部品仕掛検索条件</param>
        /// <returns>部品仕掛検索結果</returns>
        /// <history>
        /// Mod 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 ストアドの引数追加(倉庫コード)
        /// Add 2016/10/28 arc nakayama #3653_仕掛在庫のExcel出力で行うと100件しか表示されない
        /// </history>
        public List<InventoryParts_Shikakari> GetListByConditionForExcel(InventoryParts_Shikakari condition)
        {
            //入庫日
            string ArrivalPlanDateFrom = string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateFrom);
            string ArrivalPlanDateTo = string.Format("{0:yyyy/MM/dd}", condition.ArrivalPlanDateTo);

            //Add 2016/08/13 arc yano #3596
            //倉庫コードの取得
            string warehouseCode = "";

            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, condition.DepartmentCode);

            if (dWarehouse != null)
            {
                warehouseCode = dWarehouse.WarehouseCode;
            }

            // 部品仕掛データの取得
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var ret = db.GetPartsWipStock(
                                                0
                                                , condition.InventoryMonth
                                                , condition.DepartmentCode
                                                , warehouseCode
                                                , condition.ServiceType
                                                , ArrivalPlanDateFrom
                                                , ArrivalPlanDateTo
                                                , condition.SlipNumber
                                                , condition.PartsNumber
                                                , condition.LineContents1
                                                , condition.Vin
                                                , condition.CustomerName
                                                ).ToList();

                return ret.ToList();
            }
        }

        // Add 2015/06/25 arc nakayama 経理からの要望③　仕掛在庫表
        /// <summary>
        /// 仕掛在庫表の検索
        /// </summary>
        /// <param name="DepartmentCondition">部品仕掛検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>仕掛在庫表検索結果</returns>
        public PartsWipStockAmount GetSummaryByCondition(DateTime TargetDate, string NowFlag)
        {
            // ストプロ実行
            var Ret = db.GetShikakariSummary(TargetDate, NowFlag);

            PartsWipStockAmount table = new PartsWipStockAmount();
            List<GetShikakariSummaryResult> List = new List<GetShikakariSummaryResult>();

            //初期化
            table.SumTotalAmount = 0;
            table.SumTotalCost = 0;
            table.SumGrandTotalAmount = 0;

            foreach (GetShikakariSummaryResult ret in Ret)
            {
                GetShikakariSummaryResult line = new GetShikakariSummaryResult();
                line.DepartmentCode = ret.DepartmentCode;
                line.DepartmentName = ret.DepartmentName;
                line.PartsTotalAmount = ret.PartsTotalAmount;
                line.TotalCost = ret.TotalCost;
                line.GrandTotalAmount = ret.GrandTotalAmount;

                if (ret.PartsTotalAmount != null)
                {
                    table.SumTotalAmount += ret.PartsTotalAmount;
                }
                if (ret.TotalCost != null)
                {
                    table.SumTotalCost += ret.TotalCost;
                }
                if (ret.GrandTotalAmount != null)
                {
                    table.SumGrandTotalAmount += ret.GrandTotalAmount;
                }
                List.Add(line);
            }
            table.list = List;

            return table;

        }

    }
}
