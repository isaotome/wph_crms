using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    /// <summary>
    /// 帳票リストデータアクセスクラス
    /// </summary>
    /// <history>
    /// 2019/01/07 yano #3965 WE版新システム対応（Web.configによる処理の分岐) 新規作成
    /// </history>
    public class DocumentListDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public DocumentListDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 帳票を取得する（PK指定）
        /// </summary>
        /// <param name="categoryCode">分類コード</param>
        /// <param name="DocumentCode">文書コード</param>
        /// <returns>帳票</returns>
        /// <history>
        /// 2019/01/07 yano #3965 WE版新システム対応（Web.configによる処理の分岐)  新規作成
        /// </history>
        public DocumentList GetByKey(string categoryCode, string documentCode)
        {
            var query = (from a in db.DocumentList
                         where a.CategoryCode.Equals(categoryCode)
                         && a.DocumentCode.Equals(documentCode)
                         && a.DelFlag.Equals("0")
                         select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 帳票リストを取得する
        /// </summary>
        /// <param name="categoryCode">分類コード</param>
        /// <param name="DocumentCode">文書コード</param>
        /// <returns>帳票リスト</returns>
        /// <history>
        /// 2019/01/07 yano #3965 WE版新システム対応（Web.configによる処理の分岐)  新規作成
        /// </history>
        public IQueryable<DocumentList> GetListByCategoryCode(string categoryCode)
        {
            var query = (from a in db.DocumentList
                         where a.CategoryCode.Equals(categoryCode)
                         && a.DelFlag.Equals("0")
                         select a);
            return query;
        }
    }
}
