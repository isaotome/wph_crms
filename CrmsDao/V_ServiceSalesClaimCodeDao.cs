using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{

    /// <summary>
    ///  サービス受付伝票請求先アクセスクラス
    ///  全伝票ビューの各種検索メソッドを提供します。
    ///  更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2023/07/31 openwave #xxxx サービス帳票（見積書、請求明細書）用請求先取得
    /// </history>
    public class V_ServiceSalesClaimCodeDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ServiceSalesClaimCodeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 全伝票ビュー取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>伝票情報</returns>
        /// <history>
        /// </history>
        public V_ServiceSalesClaimCode GetByKey(string slipNumber)
        {
            V_ServiceSalesClaimCode rec =
               (
                from a in db.V_ServiceSalesClaimCode
                where a.SlipNumber.Equals(slipNumber)
                select a
               ).FirstOrDefault();

            return rec;
        }
    }
}
