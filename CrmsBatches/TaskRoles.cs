using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmsDao;
using System.Data.SqlClient;

namespace CrmsBatches {
    class TaskRoles {
        private CrmsLinqDataContext db;
        public TaskRoles(){
            db = new CrmsLinqDataContext();
        }
        public void AddTaskRole() {
            List<TaskConfig> configList = new TaskConfigDao(db).GetNotExistList();
            List<SecurityRole> secList = new SecurityRoleDao(db).GetListAll();
            foreach (var a in configList) {
                foreach (var s in secList) {
                    TaskRole role = new TaskRole();
                    role.TaskConfigId = a.TaskConfigId;
                    role.SecurityRoleCode = s.SecurityRoleCode;
                    role.EnableFlag = false;
                    db.TaskRole.InsertOnSubmit(role);
                }
            }

            try {
                db.SubmitChanges();
            } catch (SqlException e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
