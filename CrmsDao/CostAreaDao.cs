using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CostAreaDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public CostAreaDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 諸費用設定エリアリストを取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns>諸費用設定エリアリスト</returns>
        public PaginatedList<CostArea> GetListByCondition(CostArea condition,int pageIndex,int pageSize) {
            IQueryable<CostArea> query =
                from a in db.CostArea
                orderby a.CostAreaCode
                where (string.IsNullOrEmpty(condition.CostAreaCode) || a.CostAreaCode.Contains(condition.CostAreaCode))
                && (string.IsNullOrEmpty(condition.CostAreaName) || a.CostAreaName.Contains(condition.CostAreaName))
                select a;
            return new PaginatedList<CostArea>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// 諸費用設定エリアデータを取得する（PK指定）
        /// </summary>
        /// <param name="costAreaCode">諸費用設定エリアコード</param>
        /// <returns>諸費用設定エリアデータ</returns>
        public CostArea GetByKey(string costAreaCode) {
            CostArea query =
                (from a in db.CostArea
                 where a.CostAreaCode.Equals(costAreaCode)
                 select a).FirstOrDefault();
            return query;
        }

    }
}
