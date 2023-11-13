using System.Linq;

namespace CrmsDao
{

    /// <summary>
    /// 入金実績振替履歴テーブルアクセスクラス
    ///   入金実績振替履歴テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
    /// </history>
    public class W_JournalChangeDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public W_JournalChangeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 入金振替履歴取得
        /// </summary>
        /// <param name="null"></param>
        /// <returns>入金振替履歴</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        public IQueryable<W_JournalChange> GetQueryable()
        {
            IQueryable<W_JournalChange> query =
                from a in db.W_JournalChange
                orderby a.LastUpdateDate descending
                select a;

            return query;

        }
    }
}
