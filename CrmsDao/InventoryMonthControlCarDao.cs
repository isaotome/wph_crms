using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlClient;


//------------------------------------------------------------
// 機能 データアクセスクラス
// 作成：2017/05/10 arc yano #3762 車両棚卸機能追加 
//
//------------------------------------------------------------
namespace CrmsDao
{

    //部品棚卸状況データ取得クラス
    public class InventoryMonthControlCarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryMonthControlCarDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 棚卸状況データ取得(PK指定)
        /// </summary>
        /// <param name="targetMonth">棚卸月</param>
        /// <returns>棚卸状況データ(1件)</returns>
        public InventoryMonthControlCar GetByKey(string targetMonth)
        {
            // 棚卸状況データの取得
            InventoryMonthControlCar inventoryMonthControlCar =
               (from a in db.InventoryMonthControlCar
                where a.InventoryMonth.Equals(targetMonth)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();

            return inventoryMonthControlCar;
        }

        /// <summary>
        /// 棚卸データ取得(最新締め月1件)
        /// </summary>
        /// <returns>棚卸データ</returns>
        public InventoryMonthControlCar GetLatestInventoryMonth()
        {
            InventoryMonthControlCar InventoryMonthControlCar =
                (from a in db.InventoryMonthControlCar
                 where a.InventoryMonth ==
                 ((from b in db.InventoryMonthControlCar
                   where b.DelFlag.Equals("0")
                   select b).Max(b => b.InventoryMonth))
                 && (a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();

            return InventoryMonthControlCar;
        }

        /// <summary>
        /// 棚卸データ取得(最新締めが確定の月1件)
        /// </summary>
        /// <returns>棚卸データ</returns>
        public InventoryMonthControlCar GetLatestCloseInventoryMonth()
        {
            
            InventoryMonthControlCar InventoryMonthControlCar =
                (from a in db.InventoryMonthControlCar
                 where (a.InventoryStatus.Equals("003"))
                 && (a.DelFlag.Equals("0"))
                 orderby a.InventoryMonth descending
                 select a).FirstOrDefault();

            return InventoryMonthControlCar;
        }

    }
}
