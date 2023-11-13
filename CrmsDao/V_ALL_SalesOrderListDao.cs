using System.Linq;

namespace CrmsDao
{

    /// <summary>
    ///  全伝票ビューの各種検索メソッドを提供します。
    ///  更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
    /// </history>
    public class V_ALL_SalesOrderListDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ALL_SalesOrderListDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 全伝票ビュー取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>伝票情報</returns>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        public V_ALL_SalesOrderList GetByKey(string slipNumber)
        {
            V_ALL_SalesOrderList rec =
               (
                from a in db.V_ALL_SalesOrderList
                where a.SlipNumber.Equals(slipNumber)
                select a
               ).FirstOrDefault();

            return rec;
        }
    }
}
