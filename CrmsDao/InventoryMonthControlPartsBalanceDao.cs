using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlClient;


//-------------------------------------------------------------------------
// 機能：InventoryMonthControlPartsBalanceテーブルのデータアクセスクラス
// 作成：2015/06/09 arc nakayama IPO対応(部品棚卸) 障害対応、仕様変更④
//
//-------------------------------------------------------------------------
namespace CrmsDao
{
    public class InventoryMonthControlPartsBalanceDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryMonthControlPartsBalanceDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 受け払い締め管理データ取得(PK指定)
        /// </summary>
        /// <param name="targetMonth">対象月</param>
        /// <returns>対象月のデータ取得(１件)</returns>
        public InventoryMonthControlPartsBalance GetByKey(string targetMonth)
        {
            // 受け払い締め状況データの取得
            InventoryMonthControlPartsBalance inventoryMonthControlParts =
               (from a in db.InventoryMonthControlPartsBalance
                where a.InventoryMonth.Equals(targetMonth)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();

            // 受け払い締め状況データの返却
            return inventoryMonthControlParts;
        }

    }
}
