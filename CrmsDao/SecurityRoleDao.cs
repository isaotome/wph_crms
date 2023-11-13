using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class SecurityRoleDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public SecurityRoleDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// セキュリティロール取得（PK指定）
        /// </summary>
        /// <param name="SecurityRoleCode">セキュリティロールコード</param>
        /// <returns>セキュリティロールデータ（1件）</returns>
        public SecurityRole GetByKey(string SecurityRoleCode)
        {
            var query =
                (from s in db.SecurityRole
                where s.SecurityRoleCode.Equals(SecurityRoleCode)
                select s).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// セキュリティロールを全件取得する
        /// </summary>
        /// <returns>セキュリティロールデータ（全件）</returns>
        public List<SecurityRole> GetListAll()
        {
            var query =
                from s in db.SecurityRole
                orderby s.SecurityRoleCode
                select s;

            return query.ToList<SecurityRole>();
        }
    }
}
