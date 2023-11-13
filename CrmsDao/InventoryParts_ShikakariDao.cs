using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class InventoryParts_ShikakariDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryParts_ShikakariDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }


        //Add 2014/11/11 arc nakayama 部品仕掛在庫検索対応
        /// <summary>
        /// 部品仕掛データの検索
        /// </summary>
        /// <param name="DepartmentCondition">部品仕掛検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部品仕掛在庫テーブル検索結果</returns>
        public PaginatedList<V_PartsWipStock> GetListByConditionPast(InventoryParts_Shikakari InventoryParts_ShikakariCondition, string PartsNameJp, int? pageIndex, int? pageSize)
        {

            //Mod 2015/01/09 arc yano 部品在庫棚卸対応　部品仕掛在庫検索画面の検索条件の入庫日を、範囲指定に変更
            //Mod 2015/06/24 arc nakayama 部品仕掛在庫画面　過去月の結果を出力すると明細種別が"部品"のものしか表示されないバグ修正 & 検索項目に明細種別追加
            //入庫日
            DateTime? ArrivalPlanDateFrom = InventoryParts_ShikakariCondition.ArrivalPlanDateFrom;
            DateTime? ArrivalPlanDateTo = InventoryParts_ShikakariCondition.ArrivalPlanDateTo;

            var PartsWipStockList =
                (from a in db.InventoryParts_Shikakari
                 from b in db.Parts.Where(x => x.PartsNumber == a.PartsNumber).DefaultIfEmpty()
                 where (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.DepartmentCode) || a.DepartmentCode.Equals(InventoryParts_ShikakariCondition.DepartmentCode))
                   && (a.InventoryMonth.Equals(InventoryParts_ShikakariCondition.InventoryMonth))
                   && (ArrivalPlanDateFrom == null || DateTime.Compare(a.ArrivalPlanDate ?? DaoConst.SQL_DATETIME_MIN, ArrivalPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                   && (ArrivalPlanDateTo == null || DateTime.Compare(a.ArrivalPlanDate ?? DaoConst.SQL_DATETIME_MAX, ArrivalPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                   && (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.PartsNumber) || a.PartsNumber.Contains(InventoryParts_ShikakariCondition.PartsNumber))
                   && (string.IsNullOrEmpty(PartsNameJp) || b.PartsNameJp.Contains(PartsNameJp))
                   && (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.SlipNumber) || a.SlipNumber.Contains(InventoryParts_ShikakariCondition.SlipNumber))
                   && (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.CustomerName) || a.CustomerName.Contains(InventoryParts_ShikakariCondition.CustomerName))
                   && (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.Vin) || a.Vin.Contains(InventoryParts_ShikakariCondition.Vin))
                   && (string.IsNullOrEmpty(InventoryParts_ShikakariCondition.ServiceType) || a.ServiceType.Equals(InventoryParts_ShikakariCondition.ServiceType))
                 select new 
                 { 
                    a.Amount,
                    a.ArrivalPlanDate,
                    a.CarName,
                    a.Cost,
                    a.CustomerCode,
                    a.CustomerName,
                    a.DepartmentCode,
                    a.FrontEmployeeName,
                    a.InventoryMonth,
                    a.LineContents1,
                    a.LineContents2, 
                    a.LineNumber,
                    a.MekaEmployeeName,
                    a.PartsArravalPlanDate,
                    a.PartsNumber,
                    a.Price,
                    a.PurchaseDate,
                    a.PurchaseOrderDate,
                    a.Quantity,
                    a.ServiceOrderStatus,
                    a.ServiceOrderStatusName,
                    a.ServiceType,
                    a.ServiceTypeName,
                    a.ServiceWorkCode,
                    a.ServiceWorksName,
                    a.SlipNumber,
                    a.StockTypeName,
                    a.SupplierName,
                    a.Vin,
                    b.PartsNameJp
                 });

            List<V_PartsWipStock> list = new List<V_PartsWipStock>();
            
            foreach (var a in PartsWipStockList)
            {
                V_PartsWipStock PartsWipStock = new V_PartsWipStock();
                
                PartsWipStock.ArrivalPlanDate = a.ArrivalPlanDate;
                PartsWipStock.SlipNumber = a.SlipNumber;
                PartsWipStock.LineNumber = a.LineNumber;
                PartsWipStock.ServiceOrderStatus = a.ServiceOrderStatus;
                PartsWipStock.Name = a.ServiceWorksName;
                PartsWipStock.EmployeeName = a.FrontEmployeeName;
                PartsWipStock.EmployeeName2 = a.MekaEmployeeName;
                PartsWipStock.CustomerName = a.CustomerName;
                PartsWipStock.CarName = a.CarName;
                PartsWipStock.Vin = a.Vin;
                PartsWipStock.ServiceType1 = a.ServiceTypeName;
                PartsWipStock.StockStatus = a.StockTypeName;
                PartsWipStock.PurchaseOrderDate = a.PurchaseOrderDate;
                PartsWipStock.PurchaseDate = a.PurchaseDate;
                PartsWipStock.PartsNumber = a.PartsNumber;
                PartsWipStock.PartsNameJp = a.PartsNameJp;
                PartsWipStock.Price = (decimal)a.Price;
                PartsWipStock.Quantity = (decimal)a.Quantity;
                PartsWipStock.TotalAmount = a.Amount;
                PartsWipStock.SupplierName = a.SupplierName;
                PartsWipStock.LineContents = a.LineContents1;
                PartsWipStock.OutOrderCost = a.Cost;
                PartsWipStock.CloseMonth = a.InventoryMonth;

                //リスト追加
                list.Add(PartsWipStock);
            }

            return new PaginatedList<V_PartsWipStock>(list.AsQueryable<V_PartsWipStock>(), pageIndex ?? 0, pageSize ?? 0);
        }


    }
}
