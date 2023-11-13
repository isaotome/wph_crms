using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    ///   主作業・請求先組合わせマスタ
    ///   主作業・請求先マスタの
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2019/02/06 yano #3959 サービス伝票入力　請求先誤り防止　新規作成
    /// </history>
    public class ServiceWorkCustomerClaimDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ServiceWorkCustomerClaimDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 主作業・請求先マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="serviceWorkCode">主作業コード</param>
        /// <param name="makerCode">メーカーコード</param>
        /// <returns>主作業・請求先マスタデータ(1件)</returns>
        public ServiceWorkCustomerClaim GetByKey(string serviceWorkCode, string makerCode, bool includeDeleted = false)
        {
            // 主作業・請求先データの取得
            ServiceWorkCustomerClaim swCustomerClaim =
                (from a in db.ServiceWorkCustomerClaim
                 where a.ServiceWorkCode.Equals(serviceWorkCode)
                 && a.MakerCode.Equals(makerCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 主作業・請求先データの返却
            return swCustomerClaim;
        }
    }
}
