using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrmsDao;
using System.Configuration;

namespace CrmsBatches {
    class MainClass {
        static void Main(string[] args) {
            if (args.Count() == 0) {
                Console.WriteLine("バッチIDを指定して下さい");
                return;
            }
            TaskCheck task = new TaskCheck();
            switch (args[0]) {
                
                case "900": //ロール作成
                    task.AddTaskRole();
                    break;
                case "901": //ホット通知
                    task.HotExpired();
                    break;
                case "902": //売掛金未回収
                    task.ReceiptExpired();
                    break;
                case "903": //車検点検通知(営業)
                    task.CarInspectionNotifier();
                    break;
                case "904": //車検点検通知(サービス)
                    task.ServiceInspectionNotifier();
                    break;
                default:
                    Console.WriteLine("存在するバッチIDを指定して下さい");
                    break;
            }

        }
    }
}
