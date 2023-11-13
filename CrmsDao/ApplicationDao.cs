using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    /// <summary>
    /// アプリケーションロール
    /// </summary>
    public class ApplicationDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public ApplicationDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// アプリケーションロールを取得する（PK指定）
        /// </summary>
        /// <param name="applicationCode">アプリケーションコード</param>
        /// <returns>アプリケーションロールデータ</returns>
        public Application GetByKey(string applicationCode) {
            var query =
                (from a in db.Application
                 where string.IsNullOrEmpty(applicationCode) || a.ApplicationCode.Equals(applicationCode)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// アプリケーションロール全件取得する
        /// </summary>
        /// <returns>アプリケーションロールリスト</returns>
        public List<Application> GetListAll() {
            var query =
                from a in db.Application
                select a;
            return query.ToList<Application>();
        }
    }
}
