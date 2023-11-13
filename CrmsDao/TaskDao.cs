using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao
{
    public class TaskDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public TaskDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 担当者コードからタスクリストを取得する
        /// </summary>
        /// <param name="EmployeeCode">担当者コード</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public List<Task> GetListByEmployeeCode(Task taskCondition,string EmployeeCode,int? pageIndex,int? pageSize)
        {
            var query =
                from t in db.Task
                orderby t.Priority,t.TaskCreateDate descending
                where t.EmployeeCode.Equals(EmployeeCode)
                && !t.DelFlag.Equals("1")
                && (string.IsNullOrEmpty(taskCondition.TaskConfigId) || t.TaskConfigId.Equals(taskCondition.TaskConfigId))
                select t;

            //完了済みのものも抽出対象とする場合
            if (!taskCondition.TaskStatus) {
                ParameterExpression param = Expression.Parameter(typeof(Task), "x");
                BinaryExpression body = Expression.Equal(Expression.Property(param, "TaskCompleteDate"), Expression.Constant(null));
                query = query.Where(Expression.Lambda<Func<Task, bool>>(body, param));
            }
            query = query.OrderByDescending(x => x.CreateDate);
            return new PaginatedList<Task>(query, pageIndex ?? 0, pageSize ?? 0);
        }

        /// <summary>
        /// 担当者コードからタスクIDリストを取得する
        /// </summary>
        /// <param name="EmployeeCode">担当者コード</param>
        /// <returns>タスクIDリスト</returns>
        public List<Guid> GetIdListByEmployeeCode(string EmployeeCode) {
            var query =
                from t in db.Task
                where t.EmployeeCode.Equals(EmployeeCode)
                && !t.DelFlag.Equals("1")
                && t.TaskCompleteDate == null
                && t.TaskConfig.c_OnOff.Code.Equals("001")
                select t.TaskId;
            return query.ToList<Guid>();
        }
        /// <summary>
        /// タスクデータ取得（PK指定）
        /// </summary>
        /// <param name="taskId">タスクID</param>
        /// <returns>タスクデータ</returns>
        public Task GetByKey(Guid taskId)
        {
            Task query =
                (from t in db.Task
                where t.TaskId.Equals(taskId)
                select t).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 伝票番号（またはそれに代わるキー項目）に該当するタスクを取得する
        /// </summary>
        /// <param name="taskConfigId">タスク設定ID</param>
        /// <param name="slipNumber">伝票番号（またはそれに代わるキー項目）</param>
        /// <param name="employeeCode">タスクを完了させた担当者コード</param>
        public List<Task> GetListByIdAndSlipNumber(string taskConfigId, string slipNumber) {
            var query =
                from t in db.Task
                where t.TaskConfigId.Equals(taskConfigId)
                && t.SlipNumber.Equals(slipNumber)
                select t;
            return query.ToList<Task>();

        }
    }
}
