using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao {

    /// <summary>
    /// 棚卸テーブルアクセスクラス
    ///   棚卸テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class InventoryDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 棚卸テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸テーブルデータ(1件)</returns>
        public Inventory GetByKey(Guid inventoryId) {

            return
                (from a in db.Inventory
                 where a.InventoryId.Equals(inventoryId)
                 select a
                ).FirstOrDefault();
        }
    }
}
