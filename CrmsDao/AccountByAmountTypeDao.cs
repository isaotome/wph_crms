using System.Linq;

namespace CrmsDao
{
    /// <summary>
    /// 勘定科目変換マスタクラス
    /// 科目マスタの各種検索メソッドを提供します。
    /// 更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2020/01/06 #4025_【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更 新規作成
    /// </history>
    public class AccountByAmountTypeDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public AccountByAmountTypeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 勘定科目変換マスタ検索（PK）
        /// </summary>
        /// <param name="AmountType">金額</param>
        /// <returns>勘定科目変換マスタデータ(1件)</returns>
        /// <history>
        /// 2020/01/06 #4025_【サービス伝票】諸費用の入金予定作成時の勘定奉行科目コードの設定値の変更 新規作成
        /// </history>
        public AccountByAmountType GetByKey(string AmountType)
        {
            AccountByAmountType rec =
                (from a in db.AccountByAmountType
                 where a.AmountType.Equals(AmountType)
                 && a.DelFlag.Equals("0")
                 select a
                ).FirstOrDefault();

            // 勘定科目変換データの返却
            return rec;
        }
    }
}
