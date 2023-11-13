using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace CrmsDao {

    /// <summary>
    /// 棚卸スケジュールテーブルアクセスクラス
    ///   棚卸スケジュールテーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class InventoryScheduleDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public InventoryScheduleDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        public InventorySchedule GetByKey(string departmentCode, DateTime inventoryMonth, string inventoryType) {

            InventorySchedule inventoryScheduleCondition = new InventorySchedule();
            inventoryScheduleCondition.Department = new Department();
            inventoryScheduleCondition.Department.DepartmentCode = departmentCode;
            inventoryScheduleCondition.InventoryMonth = inventoryMonth;
            inventoryScheduleCondition.InventoryType = inventoryType;
            return GetByKey(inventoryScheduleCondition);
        }
        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="inventoryMonth">棚卸月</param>
        public InventorySchedule GetByKey(string departmentCode, DateTime inventoryMonth) {
            return GetByKey(departmentCode, inventoryMonth, null);
        }
        /// <summary>
        /// //Mod 2015/04/15 arc yano 月締め処理　仮締め中編集可能なユーザの場合、締めステータス＝仮締め(002)の場合は締め処理済みとしない
        /// //Mod 2014/10/10 arc yano 月次締め処理 不具合対応 inventoryStatus=002(仮締め)の場合も、締め処理済として扱う。
        /// 指定日が棚卸締め処理済みかどうかを取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">指定日</param>
        /// <param name="inventoryType">001:車両、002:部品</param>
        /// <returns></returns>
        public bool IsClosedInventoryMonth(string departmentCode, DateTime? targetDate, string inventoryType, string securityCode = null) {
            DateTime tmpDate = targetDate ?? DateTime.Today;
            DateTime inventoryMonth = new DateTime(tmpDate.Year, tmpDate.Month, 1);

            //アプリケーションロールを取得
            ApplicationRole rec = new ApplicationRoleDao(db).GetByKey(securityCode, "EditTempClosedData");
            bool ret;


            if ((rec != null) && rec.EnableFlag == true)
            {
                var query =
                from a in db.InventorySchedule
                where a.DelFlag.Equals("0")
                && a.DepartmentCode.Equals(departmentCode)
                && a.InventoryType.Equals(inventoryType)
                && (a.InventoryStatus.Equals("003"))
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;

                ret = (query.Count() == 0);
            }
            else
            {
               var query =
               from a in db.InventorySchedule
               where a.DelFlag.Equals("0")
               && a.DepartmentCode.Equals(departmentCode)
               && a.InventoryType.Equals(inventoryType)
               && (a.InventoryStatus.Equals("003") || a.InventoryStatus.Equals("002"))
               && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
               select a;

               ret = (query.Count() == 0);
            }

            /*
            var query =
                from a in db.InventorySchedule
                where a.DelFlag.Equals("0")
                && a.DepartmentCode.Equals(departmentCode)
                && a.InventoryType.Equals(inventoryType)
                && (a.InventoryStatus.Equals("003") || a.InventoryStatus.Equals("002"))
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;
            return query.Count() == 0;
           */

            return ret;
        }

        /// <summary>
        /// 棚卸スケジュールテーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="inventoryScheduleCondition">検索条件(PK及び参照権限)</param>
        /// <returns>棚卸スケジュールテーブルデータ(1件)</returns>
        public InventorySchedule GetByKey(InventorySchedule inventoryScheduleCondition) {

            string departmentCode = null;
            try { departmentCode = inventoryScheduleCondition.Department.DepartmentCode; } catch (NullReferenceException) { }
            DateTime inventoryMonth = inventoryScheduleCondition.InventoryMonth;
            string inventoryType = inventoryScheduleCondition.InventoryType;

            // 棚卸スケジュールデータの取得
            var query =
                from a in db.InventorySchedule
                where (a.DepartmentCode.Equals(departmentCode))
                && (a.InventoryMonth.Equals(inventoryMonth))
                && (string.IsNullOrEmpty(inventoryType) || (a.InventoryType.Equals(inventoryType)))
                && (a.DelFlag.Equals("0"))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(InventorySchedule), "x");
            Expression depExpression = inventoryScheduleCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<InventorySchedule, bool>>(depExpression, param));
            }
            Expression offExpression = inventoryScheduleCondition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<InventorySchedule, bool>>(offExpression, param));
            }
            Expression comExpression = inventoryScheduleCondition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<InventorySchedule, bool>>(comExpression, param));
            }

            // 棚卸スケジュールデータの返却
            return query.FirstOrDefault<InventorySchedule>();
        }

        //Add 2014/08/28 arc yano IPO対応
        /// <summary>
        /// 棚卸スケジュールテーブルリスト取得
        /// </summary>
        /// <param name="inventoryScheduleCondition">検索条件</param>
        /// <returns>棚卸スケジュールテーブルデータ(複数件)</returns>
        public List<InventorySchedule> GetListByKey(DateTime inventoryMonth, string inventoryType)
        {
           
            // 棚卸スケジュールデータの取得
            var query =
                from a in db.InventorySchedule
                where (a.InventoryMonth.Equals(inventoryMonth))
                && (string.IsNullOrEmpty(inventoryType) || (a.InventoryType.Equals(inventoryType)))
                 && (a.DelFlag.Equals("0"))
                 select a;

            // 棚卸スケジュールデータの返却
            return query.ToList<InventorySchedule>();
        }

        // Add 2014/09/10 arc amii 部品入荷履歴対応 指定部門の最新の棚卸スケジュールを取得する処理追加
        /// <summary>
        /// 棚卸スケジュールデータ取得(最新締め月1件)
        /// </summary>
        /// <returns>棚卸スケジュールデータ</returns>
        public InventorySchedule GetLatestDate(string departmentCode, string inventoryType, string inventoryStatus)
        {
            // 棚卸スケジュールデータの取得
            InventorySchedule inventorySchedule =
                (from a in db.InventorySchedule
                 where a.InventoryMonth ==
                 ((from b in db.InventorySchedule
                   where b.DelFlag.Equals("0")
                         && ( (b.InventoryType.Equals(inventoryType)))
                         && ((!b.InventoryStatus.Equals(inventoryStatus)))
                         && ((b.DepartmentCode.Equals(departmentCode)))
                   select b).Max(b => b.InventoryMonth))
                 && (a.DelFlag.Equals("0"))
                 && ((a.DepartmentCode.Equals(departmentCode)))
                 select a).FirstOrDefault();

            // 棚卸スケジュールデータの返却
            return inventorySchedule;
        }

        //Add 2015/03/19 arc nakayama 伝票修正対応　仮締めかどうかを取得する
        /// <summary>
        /// 指定日が仮締めかどうかを取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">指定日</param>
        /// <param name="inventoryType">001:車両、002:部品</param>
        /// <returns></returns>
        public bool IsCloseStartInventoryMonth(string departmentCode, DateTime? targetDate, string inventoryType)
        {
            DateTime tmpDate = targetDate ?? DateTime.Today;
            DateTime inventoryMonth = new DateTime(tmpDate.Year, tmpDate.Month, 1);
            var query =
                from a in db.InventorySchedule
                where a.DelFlag.Equals("0")
                && a.DepartmentCode.Equals(departmentCode)
                && a.InventoryType.Equals(inventoryType)
                && a.InventoryStatus.Equals("002")
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;
            return query.Count() == 0;
        }

        //Add 2015/03/19 arc nakayama 伝票修正対応　本締めかどうかを取得する
        /// <summary>
        /// 指定日が本締めかどうかを取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="targetDate">指定日</param>
        /// <param name="inventoryType">001:車両、002:部品</param>
        /// <returns></returns>
        public bool IsCloseEndInventoryMonth(string departmentCode, DateTime? targetDate, string inventoryType)
        {
            DateTime tmpDate = targetDate ?? DateTime.Today;
            DateTime inventoryMonth = new DateTime(tmpDate.Year, tmpDate.Month, 1);
            var query =
                from a in db.InventorySchedule
                where a.DelFlag.Equals("0")
                && a.DepartmentCode.Equals(departmentCode)
                && a.InventoryType.Equals(inventoryType)
                && a.InventoryStatus.Equals("003")
                && DateTime.Compare(a.InventoryMonth, inventoryMonth) >= 0
                select a;
            return query.Count() == 0;
        }
    }
}
