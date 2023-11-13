using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// 部品割引率マスタ
    /// </summary>
    /// <remarks>2014/09/16 arc amii 部品価格一括更新対応 新規作成</remarks>
    public class PartsDiscountRateDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public PartsDiscountRateDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 部品割引率マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="discountCode">割引コード</param>
        /// <returns>部品割引率マスタデータ(1件)</returns>
        public PartsDiscountRate GetByKey(string discountCode)
        {
            // 部品データの取得
            PartsDiscountRate rate =
                (from a in db.PartsDiscountRate
                 where a.DiscountCode.Equals(discountCode)
                 select a
                ).FirstOrDefault();

        
            // 部品データの返却
            return rate;
        }
    }
}
