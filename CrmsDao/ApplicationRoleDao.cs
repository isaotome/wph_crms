using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class ApplicationRoleDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public ApplicationRoleDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// アプリケーションロールリストを取得する
        /// </summary>
        /// <param name="role">アプリケーションロール</param>
        /// <returns>アプリケーションロールリスト</returns>
        public List<ApplicationRole> GetListByKey(ApplicationRole role)
        {
            var query =
                from r in db.ApplicationRole
                where role.SecurityRoleCode.Equals(r.SecurityRoleCode)
                && (role.Application == null || role.Application.ApplicationCode == null || role.Application.ApplicationCode.Equals(r.Application.ApplicationCode))
                orderby r.Application.DisplayOrder
                select r;
            return query.ToList<ApplicationRole>();
        }

        /// <summary>
        /// アプリケーションロールデータを取得する（PK指定）
        /// </summary>
        /// <param name="SecurityRoleCode">セキュリティロールコード</param>
        /// <param name="MenuControlCode">メニューコントロールコード</param>
        /// <returns></returns>
        public ApplicationRole GetByKey(string SecurityRoleCode, string applicationCode)
        {
            var query =
                (from r in db.ApplicationRole
                 where r.SecurityRoleCode.Equals(SecurityRoleCode) 
                 && r.ApplicationCode.Equals(applicationCode)
                 select r).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// アプリケーションロールリストを取得する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        /// <returns>アプリケーションロールリスト</returns>
        public List<ApplicationRole> GetListBySecurityRole(SecurityRole role)
        {
            var query =
                from r in db.ApplicationRole
                where role.SecurityRoleCode.Equals(r.SecurityRoleCode)
                orderby r.Application.DisplayOrder
                select r;

            return query.ToList<ApplicationRole>();
        }

        /// <summary>
        /// アプリケーションロールリスト全件取得する
        /// </summary>
        /// <returns>アプリケーションロールリスト</returns>
        public List<ApplicationRole> GeListAll()
        {
            var query =
                from r in db.ApplicationRole
                orderby r.SecurityRoleCode
                select r;
            return query.ToList<ApplicationRole>();
        }
    }
}
