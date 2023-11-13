using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace CrmsDao
{

    /// <summary>
    ///   部品棚卸スケジュールテーブルアクセスクラス
    ///   部品棚卸スケジュールテーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class InventorySchedulePartsDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventorySchedulePartsDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 暫定【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// </history>
        public InventoryScheduleParts GetByKey(string warehouseCode, DateTime inventoryMonth, string inventoryType)
        //public InventoryScheduleParts GetByKey(string departmentCode, DateTime inventoryMonth, string inventoryType)
        {

            InventoryScheduleParts inventorySchedulePratsCondition = new InventoryScheduleParts();
            //Mod 2016/08/13 arc yano #3596
            //inventorySchedulePratsCondition.Department = new Department();
            //inventorySchedulePratsCondition.Department.DepartmentCode = departmentCode;
            inventorySchedulePratsCondition.WarehouseCode = warehouseCode;
            inventorySchedulePratsCondition.InventoryMonth = inventoryMonth;
            inventorySchedulePratsCondition.InventoryType = inventoryType;
            return GetByKey(inventorySchedulePratsCondition);
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="inventoryScheduleCondition">検索条件(PK及び参照権限)</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 暫定【大項目】部門棚統合対応 部品棚卸の管理を部門から倉庫に変更
        /// </history>
        public InventoryScheduleParts GetByKey(InventoryScheduleParts InventorySchedulePartseCondition)
        {
            string warehouseCode = null;
            //Mod 2016/08/13 arc yano #3596
            //try { departmentCode = InventorySchedulePartseCondition.Department.DepartmentCode; }
            //catch (NullReferenceException) { }
            warehouseCode = InventorySchedulePartseCondition.WarehouseCode;
            DateTime inventoryMonth = InventorySchedulePartseCondition.InventoryMonth;
            string inventoryType = InventorySchedulePartseCondition.InventoryType;

            // 棚卸スケジュールデータの取得
            var query =
                from a in db.InventoryScheduleParts
                where (a.WarehouseCode.Equals(warehouseCode))
                && (a.InventoryMonth.Equals(inventoryMonth))
                && (string.IsNullOrEmpty(inventoryType) || (a.InventoryType.Equals(inventoryType)))
                && (a.DelFlag.Equals("0"))
                select a;

            /*
            ParameterExpression param = Expression.Parameter(typeof(InventoryScheduleParts), "x");
            Expression depExpression = InventorySchedulePartseCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<InventoryScheduleParts, bool>>(depExpression, param));
            }
            Expression offExpression = InventorySchedulePartseCondition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<InventoryScheduleParts, bool>>(offExpression, param));
            }
            Expression comExpression = InventorySchedulePartseCondition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<InventoryScheduleParts, bool>>(comExpression, param));
            }
            */

            // 棚卸スケジュールデータの返却
            return query.FirstOrDefault<InventoryScheduleParts>();
        }

        /// <summary>
        /// 指定日が部品棚卸締め処理済みかどうかを取得する
        /// </summary>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="targetDate">指定日</param>
        /// <param name="inventoryType">001:車両、002:部品</param>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数を変更(departmentCode → warehouseCode)
        /// </history>
        /// <returns></returns>
        public bool IsClosedInventoryMonth(string warehouseCode, DateTime? targetDate, string inventoryType)
        {
            DateTime tmpDate = targetDate ?? DateTime.Today;
            DateTime inventoryMonth = new DateTime(tmpDate.Year, tmpDate.Month, 1);
            var query =
                from a in db.InventoryScheduleParts
                where a.DelFlag.Equals("0")
                && a.WarehouseCode.Equals(warehouseCode)
                && a.InventoryType.Equals(inventoryType)
                && (a.InventoryStatus.Equals("002"))
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;
            return query.Count() == 0;
        }


        /*　2016/08/13 arc yano #3596　現在未使用のため削除
        /// <summary>
        /// 棚卸スケジュールデータ取得(最新締め月1件)
        /// </summary>
        /// <returns>棚卸スケジュールデータ</returns>
        public InventoryScheduleParts GetLatestDate(string departmentCode, string inventoryType, string inventoryStatus)
        {
            // 棚卸スケジュールデータの取得
            InventoryScheduleParts InventoryScheduleParts =
                (from a in db.InventoryScheduleParts
                 where a.InventoryMonth ==
                 ((from b in db.InventorySchedule
                   where b.DelFlag.Equals("0")
                         && ((b.InventoryType.Equals(inventoryType)))
                         && ((!b.InventoryStatus.Equals(inventoryStatus)))
                         && ((b.DepartmentCode.Equals(departmentCode)))
                   select b).Max(b => b.InventoryMonth))
                 && (a.DelFlag.Equals("0"))
                 && ((a.DepartmentCode.Equals(departmentCode)))
                 select a).FirstOrDefault();

            // 棚卸スケジュールデータの返却
            return InventoryScheduleParts;
        }
        */

    }
}