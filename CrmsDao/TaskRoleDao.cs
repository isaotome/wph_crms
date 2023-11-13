using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class TaskRoleDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public TaskRoleDao(CrmsLinqDataContext context) {
            db = context;
        }
        /// <summary>
        /// タスクロールリストを取得する
        /// </summary>
        /// <param name="role">セキュリティロール</param>
        /// <returns>タスクロールリスト</returns>
        public List<TaskRole> GetListBySecurityRole(SecurityRole role) {
            var query =
                from a in db.TaskRole
                where string.IsNullOrEmpty(role.SecurityRoleCode) || a.SecurityRoleCode.Equals(role.SecurityRoleCode)
                select a;

            return query.ToList<TaskRole>();
        }
    }
}
