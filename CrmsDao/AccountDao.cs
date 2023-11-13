using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 科目マスタアクセスクラス
    ///   科目マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class AccountDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public AccountDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 科目マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="accountCode">科目コード</param>
        /// <returns>科目マスタデータ(1件)</returns>
        public Account GetByKey(string accountCode)
        {
            // 科目データの取得
            Account account =
                (from a in db.Account
                 where a.AccountCode.Equals(accountCode)
                 && a.DelFlag.Equals("0")
                 select a
                ).FirstOrDefault();

            // 科目データの返却
            return account;
        }

        /// <summary>
        /// 科目マスタデータ検索
        /// </summary>
        /// <param name="accountCondition">科目検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>科目マスタデータ検索結果</returns>
        public PaginatedList<Account> GetListByCondition(Account accountCondition, int? pageIndex, int? pageSize)
        {
            string delFlag = accountCondition.DelFlag;

            // 科目データの取得
            IOrderedQueryable<Account> accountList =
                    from a in db.Account
                    where (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(accountCondition.CreditFlag) || a.CreditFlag.Equals(accountCondition.CreditFlag))
                    && (string.IsNullOrEmpty(accountCondition.DebitFlag) || a.DebitFlag.EndsWith(accountCondition.DebitFlag))
                    orderby a.AccountCode
                    select a;

            // ページング制御情報を付与した科目データの返却
            PaginatedList<Account> ret = new PaginatedList<Account>(accountList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// 売掛金、サービス売掛金の科目を取得する
        /// </summary>
        /// <param name="usageType">C:売掛金、S:サービス売掛金</param>
        /// <returns>科目データ</returns>
        public Account GetByUsageType(string usageType) {
            Account query =
                (from a in db.Account
                 where (string.IsNullOrEmpty(usageType) || a.UsageType.Equals(usageType))
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return query;
        }
    }
}
