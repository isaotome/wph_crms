using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 決済条件マスタアクセスクラス
    ///   決済条件マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CustomerClaimableDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CustomerClaimableDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 決済条件マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="customerClaimCode">請求先コード</param>
        /// <param name="paymentKindCode">支払種別コード</param>
        /// <returns>決済条件マスタデータ(1件)</returns>
        public CustomerClaimable GetByKey(string customerClaimCode, string paymentKindCode)
        {
            // 決済条件データの取得
            CustomerClaimable customerClaimable =
                (from a in db.CustomerClaimable
                 where (a.CustomerClaimCode.Equals(customerClaimCode))
                 && (a.PaymentKindCode.Equals(paymentKindCode))
                 select a
                ).FirstOrDefault();

            // 決済条件データの返却
            return customerClaimable;
        }

        /// <summary>
        /// 決済条件マスタデータ検索
        /// </summary>
        /// <param name="customerClaimableCondition">決済条件検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>決済条件マスタデータ検索結果</returns>
        public PaginatedList<CustomerClaimable> GetListByCondition(CustomerClaimable customerClaimableCondition, int? pageIndex, int? pageSize)
        {
            string customerClaimCode = customerClaimableCondition.CustomerClaimCode;
            string delFlag = customerClaimableCondition.DelFlag;

            // 決済条件データの取得
            IOrderedQueryable<CustomerClaimable> customerClaimableList =
                    from a in db.CustomerClaimable
                    where (a.CustomerClaimCode.Equals(customerClaimCode))
                    orderby a.CustomerClaimCode, a.PaymentKindCode
                    select a;

            // ページング制御情報を付与した決済条件データの返却
            PaginatedList<CustomerClaimable> ret = new PaginatedList<CustomerClaimable>(customerClaimableList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }
    }
}
