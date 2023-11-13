using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 耐用年数マスタアクセスクラス
    /// </summary>
    /// <history>
    /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
    /// </history>
    public class UsefulLivesDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public UsefulLivesDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 耐用年数マスタテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="usefulLives">耐用年数/param>
        /// <returns>耐用年数データ(1件)</returns>
        /// <history>
        /// 2018/06/06 arc yano #3883 タマ表改善 新規作成
        /// </history>
        public UsefulLives GetByKey(int usefulLives, bool includeDeleted = false)
        {
            // 耐用年数マスタクラスデータの取得
            UsefulLives rec =
                 (
                 from a in db.UsefulLives
                 where a.Years.Equals(usefulLives)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 車両固定資産クラスデータの返却
            return rec;
        }
    }
}
