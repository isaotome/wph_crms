using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 車両固定資産テーブルアクセスクラス
    /// </summary>
    /// <history>
    /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
    /// </history>
    public class CarFixedAssetsDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarFixedAssetsDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両固定資産テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="salesCarNumber">車両管理番号/param>
        /// <returns>車両固定資産データ(1件)</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public CarFixedAssets GetByKey(string salesCarNumber, bool includeDeleted = false)
        {
            // 車両固定資産クラスデータの取得
            CarFixedAssets carFiexdAssets =
                (
                 from a in db.CarFixedAssets
                 where a.SalesCarNumber.Equals(salesCarNumber)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 車両固定資産クラスデータの返却
            return carFiexdAssets;
        }
    }
}
