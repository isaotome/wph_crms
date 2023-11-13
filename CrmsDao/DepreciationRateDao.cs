using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 償却率マスタアクセスクラス
    /// </summary>
    /// <history>
    /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
    /// </history>
    public class DepreciationRateDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public DepreciationRateDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 償却率マスタテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="usefulLives">耐用年数</param>
        /// <returns>耐用年数データ(1件)</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public DepreciationRate GetByKey(int usefulLives, bool includeDeleted = false)
        {
            // 耐用年数マスタクラスデータの取得
            DepreciationRate rec =
                 (
                 from a in db.DepreciationRate
                 where a.UsefulLives.Equals(usefulLives)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 車両固定資産クラスデータの返却
            return rec;
        }

        /// <summary>
        /// 耐用年数マスタテーブルデータ取得
        /// </summary>
        /// <param name="usefulLives">耐用年数/param>
        /// <returns>耐用年数リスト</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public PaginatedList<DepreciationRate> GetListByCondition(DepreciationRate condition, int pageIndex, int pageSize)
        {
            IQueryable<DepreciationRate> query =
                from a in db.DepreciationRate
                orderby a.UsefulLives
                where ((condition.UsefulLives == 0) || a.UsefulLives.Equals(condition.UsefulLives))
                select a;

            return new PaginatedList<DepreciationRate>(query, pageIndex, pageSize);
        }
    }
}
