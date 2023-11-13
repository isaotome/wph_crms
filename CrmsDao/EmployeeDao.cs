using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 社員マスタアクセスクラス
    ///   社員マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class EmployeeDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public EmployeeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 社員マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="EmployeeCode">社員コード</param>
        /// <returns>社員マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため)
        public Employee GetByKey(string employeeCode, bool includeDeleted = false)
        {
            // 社員データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Employee employee =
                (from a in db.Employee
                 where a.EmployeeCode.Equals(employeeCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (employee != null)
            {
                employee = EditModel(employee);
            }

            // 社員データの返却
            return employee;
        }

        /// <summary>
        /// 社員番号から社員を取得する
        /// </summary>
        /// <param name="employeeNumber">社員番号</param>
        /// <returns>社員マスタデータ</returns>
        public Employee GetByEmployeeNumber(string employeeNumber) {
            var query =
                (from a in db.Employee
                 where a.DelFlag.Equals("0")
                 && a.EmployeeNumber.Equals(employeeNumber)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 社員マスタデータ検索
        /// </summary>
        /// <param name="EmployeeCondition">社員検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>社員マスタデータ検索結果</returns>
        public PaginatedList<Employee> GetListByCondition(Employee employeeCondition, int? pageIndex, int? pageSize)
        {
            string employeeCode = employeeCondition.EmployeeCode;
            string employeeNumber = employeeCondition.EmployeeNumber;
            string employeeName = employeeCondition.EmployeeName;
            string departmentCode = null;
            try { departmentCode = employeeCondition.Department1.DepartmentCode; } catch (NullReferenceException) { }
            string departmentName = null;
            try { departmentName = employeeCondition.Department1.DepartmentName; } catch (NullReferenceException) { }
            string securityRoleCode = null;
            try { securityRoleCode = employeeCondition.SecurityRole.SecurityRoleCode; } catch (NullReferenceException) { }
            string securityRoleName = null;
            try { securityRoleName = employeeCondition.SecurityRole.SecurityRoleName; } catch (NullReferenceException) { }
            string delFlag = employeeCondition.DelFlag;

            // 社員データの取得
            IOrderedQueryable<Employee> employeeList =
                    from a in db.Employee
                    where (string.IsNullOrEmpty(employeeCode) || a.EmployeeCode.Contains(employeeCode))
                    && (string.IsNullOrEmpty(employeeNumber) || a.EmployeeNumber.Contains(employeeNumber))
                    && (string.IsNullOrEmpty(employeeName) || a.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.Department1.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(securityRoleCode) || a.SecurityRoleCode.Contains(securityRoleCode))
                    && (string.IsNullOrEmpty(securityRoleName) || a.SecurityRole.SecurityRoleName.Contains(securityRoleName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.EmployeeCode
                    select a;

            // ページング制御情報を付与した社員データの返却
            PaginatedList<Employee> ret = new PaginatedList<Employee>(employeeList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="employee">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Employee EditModel(Employee employee)
        {
            // 内部コード項目の名称情報取得
            employee.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, employee.DelFlag);

            // 出口
            return employee;
        }
    }
}
