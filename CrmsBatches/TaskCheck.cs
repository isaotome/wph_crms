using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmsDao;
using System.Data.SqlClient;
using System.Transactions;
using System.Configuration;

namespace CrmsBatches {
    class TaskCheck {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// タスク作成担当者（system）
        /// </summary>
        private Employee employee = new Employee { EmployeeCode = "system", DelFlag = "0" };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TaskCheck() {
            
            db = new CrmsLinqDataContext(ConfigurationManager.AppSettings["connectionString"]);
        }

        /// <summary>
        /// タスクロール追加
        /// </summary>
        public void AddTaskRole() {
            using (TransactionScope ts = new TransactionScope()) {
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
                ts.Complete();
            }
        }

        /// <summary>
        /// 売掛金未回収チェック(902)
        /// </summary>
        public void ReceiptExpired() {
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("売掛金未回収チェック処理を行います。");
            Console.WriteLine("入金予定日を" + ConfigurationManager.AppSettings["ReceiptExpired"] + "日間経過している場合タスクに追加します。");
            Console.WriteLine("--------------------------------------------------------");
            using (TransactionScope ts = new TransactionScope()) {
                new TaskUtil(db, this.employee).ReceiptExpired();
                try {
                    db.SubmitChanges();
                } catch (SqlException) { }
                ts.Complete();
            }
        }

        /// <summary>
        /// HOT期限切れ通知(901)
        /// </summary>
        public void HotExpired() {
            ConfigurationSettingDao dao = new ConfigurationSettingDao(db);
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("HOT管理チェック処理を行います。");
            Console.WriteLine("AHOT：見積日から" + dao.GetByKey("HOTAExpireDays").Value + "日を過ぎたものをタスクに追加");
            Console.WriteLine("BHOT：見積日から" + dao.GetByKey("HOTBExpireDays").Value + "日を過ぎたものをタスクに追加");
            Console.WriteLine("CHOT：見積日から" + dao.GetByKey("HOTCExpireDays").Value + "日を過ぎたものをタスクに追加");
            Console.WriteLine("--------------------------------------------------------");

            using (TransactionScope ts = new TransactionScope()) {
                new TaskUtil(db, this.employee).HotNotifier();
                try {
                    db.SubmitChanges();
                } catch (SqlException) {}
                ts.Complete();
            }
        }

        /// <summary>
        /// 車検点検通知（営業）
        /// </summary>
        internal void CarInspectionNotifier() {
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("車検点検（営業）チェック処理を行います。");
            Console.WriteLine("車検まで6ヶ月を切った車両をタスクに追加");
            Console.WriteLine("車検まで3ヶ月を切った車両をタスクに追加");
            Console.WriteLine("--------------------------------------------------------");

            using (TransactionScope ts = new TransactionScope()) {
                new TaskUtil(db, this.employee).CarInspectionNotifier();
                try {
                    db.SubmitChanges();
                } catch (SqlException) { }
                ts.Complete();
            }
        }

        /// <summary>
        /// 車検点検通知（サービス）
        /// </summary>
        internal void ServiceInspectionNotifier() {
            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine("車検点検（サービス）チェック処理を行います。");
            Console.WriteLine("車検まで1ヶ月を切った車両をタスクに追加");
            Console.WriteLine("次回点検日まで1ヶ月を切った車両をタスクに追加");
            Console.WriteLine("--------------------------------------------------------");
            using (TransactionScope ts = new TransactionScope()) {
                new TaskUtil(db, this.employee).ServiceInspectionNotifier();
                try {
                    db.SubmitChanges();
                } catch (SqlException) { }
                ts.Complete();
            }
        }
    }
}
