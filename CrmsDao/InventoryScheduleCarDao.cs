using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace CrmsDao
{

    /// <summary>
    ///   車両棚卸スケジュールテーブルアクセスクラス
    ///   車両棚卸スケジュールテーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class InventoryScheduleCarDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryScheduleCarDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両棚卸機能追加
        /// </history>
        public InventoryScheduleCar GetByKey(string warehouseCode, DateTime inventoryMonth)
        {
            InventoryScheduleCar inventoryScheduleCarCondition = new InventoryScheduleCar();

            inventoryScheduleCarCondition.WarehouseCode = warehouseCode;
            inventoryScheduleCarCondition.InventoryMonth = inventoryMonth;
            return GetByKey(inventoryScheduleCarCondition);
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="inventoryScheduleCondition">検索条件(PK及び参照権限)</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両棚卸機能追加
        /// </history>
        public InventoryScheduleCar GetByKey(InventoryScheduleCar InventoryScheduleCarCondition)
        {
            string warehouseCode = null;
          
            warehouseCode = InventoryScheduleCarCondition.WarehouseCode;
            DateTime inventoryMonth = InventoryScheduleCarCondition.InventoryMonth;

            // 棚卸スケジュールデータの取得
            var query =
                from a in db.InventoryScheduleCar
                where (a.WarehouseCode.Equals(warehouseCode))
                && (a.InventoryMonth.Equals(inventoryMonth))
                && (a.DelFlag.Equals("0"))
                select a;

            // 棚卸スケジュールデータの返却
            return query.FirstOrDefault<InventoryScheduleCar>();
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得
        /// </summary>
        /// <param name="inventoryMonth"></param>
        /// 
        /// <returns>棚卸スケジュールリスト)</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両棚卸機能追加
        /// </history>
        public List<InventoryScheduleCar> GetListByInventoryMonth(DateTime inventoryMonth)
        {
            // 棚卸スケジュールデータの取得
            var query =
                from a in db.InventoryScheduleCar
                where (a.InventoryMonth.Equals(inventoryMonth))
                && (a.DelFlag.Equals("0"))
                select a;

            return query.ToList();
        }

        /// <summary>
        /// 指定日が棚卸締め処理済みかどうかを取得する
        /// </summary>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="targetDate">指定日</param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両棚卸機能追加
        /// </history>
        /// <returns></returns>
        public bool IsClosedInventoryMonth(string warehouseCode, DateTime? targetDate)
        {
            DateTime tmpDate = targetDate ?? DateTime.Today;
            DateTime inventoryMonth = new DateTime(tmpDate.Year, tmpDate.Month, 1);
            var query =
                from a in db.InventoryScheduleCar
                where a.DelFlag.Equals("0")
                && a.WarehouseCode.Equals(warehouseCode)
                && (a.InventoryStatus.Equals("002"))
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;
            return query.Count() == 0;
        }
    }
}