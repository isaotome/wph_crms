using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class TaskConfigDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public TaskConfigDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 全てのタスク設定を取得する
        /// </summary>
        /// <returns>タスク設定リスト</returns>
        public List<TaskConfig> GetAllList(bool includeUnVisibled) {
            var query =
                from t in db.TaskConfig
                where includeUnVisibled == true || t.Visible == true
                select t;
            return query.ToList<TaskConfig>();
        }

        /// <summary>
        /// タスクの有効無効を取得する
        /// </summary>
        /// <param name="taskConfigId">タスク設定ID</param>
        /// <returns></returns>
        public bool TaskEnabled(string taskConfigId) {
            var query =
                (from a in db.TaskConfig
                 where a.TaskConfigId.Equals(taskConfigId)
                 select a).FirstOrDefault();
            return !string.IsNullOrEmpty(query.DelFlag) && query.DelFlag.Equals("0") ? true : false;
        }

        /// <summary>
        /// タスクロールに存在しないタスク設定リストを取得する
        /// </summary>
        /// <returns>タスク設定リスト</returns>
        public List<TaskConfig> GetNotExistList() {
            var query =
                from a in db.TaskConfig
                where !(
                    from b in db.TaskRole
                    select b.TaskConfigId
                ).Contains(a.TaskConfigId)
                select a;
            return query.ToList();
        }
        /// <summary>
        /// タスク設定IDに紐付く担当者リストを取得する
        /// </summary>
        /// <param name="taskConfigId">タスク設定ID</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>担当者リスト</returns>
        public List<Employee> GetEmployeeByKey(string taskConfigId, string departmentCode) {
            
            //タスク設定データを取得する
            var query =
                from t in db.TaskRole
                where t.TaskConfigId.Equals(taskConfigId)
                && t.EnableFlag==true
                select t;

            var dep = 
                (from d in db.Department
                where d.DepartmentCode.Equals(departmentCode)
                && !d.DelFlag.Equals("1")
                select d).FirstOrDefault();

            //タスク設定に紐付くロールから担当者リストを取得する
            List<Employee> empList = new List<Employee>();
            IQueryable<Employee> emp;
            foreach (var a in query) {
                emp = null;
                switch (a.SecurityRole.SecurityLevelCode) {
                    case "001": //部門内
                        emp =
                            from e in db.Employee
                            where e.SecurityRoleCode.Equals(a.SecurityRoleCode)
                            && (e.DepartmentCode.Equals(departmentCode) || e.DepartmentCode1.Equals(departmentCode) || e.DepartmentCode2.Equals(departmentCode) || e.DepartmentCode3.Equals(departmentCode))
                            && !e.DelFlag.Equals("1")
                            select e;
                        break;
                    case "002": //事業部内
                        emp =
                            from e in db.Employee
                            where e.SecurityRoleCode.Equals(a.SecurityRoleCode)
                            && (e.Department1.OfficeCode.Equals(dep.OfficeCode) || e.AdditionalDepartment1.OfficeCode.Equals(dep.OfficeCode) || e.AdditionalDepartment2.OfficeCode.Equals(dep.OfficeCode) || e.AdditionalDepartment3.OfficeCode.Equals(dep.OfficeCode))
                            && !e.DelFlag.Equals("1")
                            select e;
                        break;
                    case "003": //会社内
                        emp =
                            from e in db.Employee
                            where e.SecurityRoleCode.Equals(a.SecurityRoleCode)
                            && e.Department1.Office.CompanyCode.Equals(dep.Office.CompanyCode)
                            && !e.DelFlag.Equals("1")
                            select e;
                        break;
                    case "004": //ALL
                        emp =
                            from e in db.Employee
                            where e.SecurityRoleCode.Equals(a.SecurityRoleCode)
                            && !e.DelFlag.Equals("1")
                            select e;
                        break;
                    default: emp = null;
                        break;
                }
                if (emp != null){
                    empList.AddRange(emp.ToList<Employee>());
                }
                
            }
            return empList;
        }
    }
}
