using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlClient;


//------------------------------------------------------------
// 機能 データアクセスクラス
// 作成：2014/07/28 arc yano IPO対応 
//
//------------------------------------------------------------
namespace CrmsDao
{

    //部品棚卸状況データ取得クラス
    public class InventoryMonthControlPartsDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryMonthControlPartsDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 部品棚卸状況データ取得(PK指定)
        /// </summary>
        /// <param name="targetMonth">棚卸月</param>
        /// <returns>部品棚卸状況データ(1件)</returns>
        public InventoryMonthControlParts GetByKey(string targetMonth)
        {
            // 部品棚卸状況データの取得
            InventoryMonthControlParts inventoryMonthControlParts =
               (from a in db.InventoryMonthControlParts
                where a.InventoryMonth.Equals(targetMonth)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();


            // 月締処理状況データの返却
            return inventoryMonthControlParts;
        }
        
        /// <summary>
        /// 部品棚卸データ取得(最新締め月1件)
        /// </summary>
        /// <returns>部品棚卸データ</returns>
        public InventoryMonthControlParts GetLatestInventoryMonth()
        {
            InventoryMonthControlParts InventoryMonthControlParts = 
                (from a in db.InventoryMonthControlParts
                 where a.InventoryMonth ==
                 ((from b in db.InventoryMonthControlParts
                   where b.DelFlag.Equals("0")
                   select b).Max(b => b.InventoryMonth))
                 && (a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();

            return InventoryMonthControlParts;
        }

        /// <summary>
        /// 部品棚卸データ取得(最新締めが確定の月1件)
        /// </summary>
        /// <returns>部品棚卸データ</returns>
        public InventoryMonthControlParts GetLatestCloseInventoryMonth()
        {
            InventoryMonthControlParts InventoryMonthControlParts =
                (from a in db.InventoryMonthControlParts
                 where (a.InventoryStatus.Equals("002"))
                 && (a.DelFlag.Equals("0"))
                 orderby a.InventoryMonth descending
                 select a).FirstOrDefault();

            return InventoryMonthControlParts;
        }

    }
}
