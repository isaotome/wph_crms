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
    /// 現金在高テーブルアクセスクラス
    ///   現金在高テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CashBalanceDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CashBalanceDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 現金在高テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="departmentCode">事業所コード</param>
        /// <param name="closedDate">締日</param>
        /// <returns>現金在高テーブルデータ(1件)</returns>
        public CashBalance GetByKey(string officeCode, string cashAccountCode, DateTime closedDate) {
            CashBalance cashBalanceCondition = new CashBalance();
            cashBalanceCondition.OfficeCode = officeCode;
            cashBalanceCondition.ClosedDate = closedDate;
            cashBalanceCondition.CashAccountCode = cashAccountCode;
            return GetByKey(cashBalanceCondition);
        }

        /// <summary>
        /// 現金在高テーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="cashBalanceCondition">検索条件(PK及び参照権限)</param>
        /// <returns>現金在高テーブルデータ(1件)</returns>
        public CashBalance GetByKey(CashBalance cashBalanceCondition) {

            string officeCode = cashBalanceCondition.OfficeCode;
            DateTime closedDate = cashBalanceCondition.ClosedDate;
            string cashAccountCode = cashBalanceCondition.CashAccountCode;

            // 現金在高データの取得
            var query =
                from a in db.CashBalance
                where (a.OfficeCode.Equals(officeCode))
                && (a.CashAccountCode.Equals(cashAccountCode))
                && (a.ClosedDate.Equals(closedDate))
                && (a.DelFlag.Equals("0"))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(CashBalance), "x");
            Expression depExpression = cashBalanceCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(depExpression, param));
            }
            Expression offExpression = cashBalanceCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(offExpression, param));
            }
            Expression comExpression = cashBalanceCondition.CreateExpressionForCompany(param, new string[] { "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(comExpression, param));
            }

            CashBalance cashBalance = query.FirstOrDefault();

            // 現金在高データの返却
            return cashBalance;
        }

        /// <summary>
        /// 現金在高テーブル最終締めレコード取得(部門指定)
        /// </summary>
        /// <param name="cashBalanceCondition">検索条件</param>
        /// <returns>現金在高テーブルデータ(1件)</returns>
        public CashBalance GetLatestClosedData(CashBalance cashBalanceCondition) {

            string officeCode = cashBalanceCondition.OfficeCode;
            string cashAccountCode = cashBalanceCondition.CashAccountCode;

            // 現金在高データの取得
            var query =
                from a in db.CashBalance
                 where (a.OfficeCode.Equals(officeCode))
                 && (a.CashAccountCode.Equals(cashAccountCode))
                 && (a.CloseFlag.Equals("1"))
                 && (a.DelFlag.Equals("0"))
                 select a;


            ParameterExpression param = Expression.Parameter(typeof(CashBalance), "x");
            Expression depExpression = cashBalanceCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(depExpression, param));
            }
            Expression offExpression = cashBalanceCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(offExpression, param));
            }
            Expression comExpression = cashBalanceCondition.CreateExpressionForCompany(param, new string[] { "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<CashBalance, bool>>(comExpression, param));
            }
            CashBalance cashBalance = query.OrderByDescending(x => x.ClosedDate).FirstOrDefault();

            // 現金在高データの返却
            return cashBalance;
        }

        /// <summary>
        /// 指定月末時点の現金在高を取得する
        /// </summary>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="targetMonth">指定月</param>
        /// <returns></returns>
        public CashBalance GetLastMonthClosedData(string officeCode, string cashAccountCode, DateTime targetMonth)
        {
            DateTime targetDate = new DateTime(targetMonth.Year, targetMonth.Month, 1).AddDays(-1);
            CashBalance cashBalance =
                (from a in db.CashBalance
                 where a.OfficeCode.Equals(officeCode)
                 && a.CashAccountCode.Equals(cashAccountCode)
                 && a.ClosedDate.Equals(targetDate)
                 && a.CloseFlag.Equals("1")
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return cashBalance;
        }

        ///Add 2014/09/25 arc yano #3095 現金出在高データ取得方法の変更 指定月末時点の現金在高を取得
        /// <summary>
        /// 指定月末時点の現金在高を取得する
        /// </summary>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="targetMonth">指定月</param>
        /// <returns></returns>
        public CashBalance GetLastMonthClosedData2(string officeCode, string cashAccountCode, DateTime targetMonth)
        {
            DateTime targetDate = new DateTime(targetMonth.Year, targetMonth.Month, 1).AddMonths(1);
            targetDate = targetDate.AddDays(-1);
            CashBalance cashBalance =
                (from a in db.CashBalance
                 where a.OfficeCode.Equals(officeCode)
                 && a.CashAccountCode.Equals(cashAccountCode)
                 && a.ClosedDate.Equals(targetDate)
                 && a.CloseFlag.Equals("1")
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return cashBalance;
        }

        ///Add 2015/03/18 arc yano 現金出納帳出力(エクセル)
        /// <summary>
        /// 対象年月の金種表を取得する
        /// </summary>
        /// <param name="targetDateY">対象年月(年)</param>
        /// <param name="targetDateM">対象年月(月)</param>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="cashAccountCode">現金口座コード</param>
        /// <returns>結果</returns>
        public List<T_CashBalanceSheet> GetCashBalance(int targetDateY, int targetDateM, string officeCode , string cashAccountCode)
        {
            var result = db.GetCashBalance(targetDateY, targetDateM, officeCode, cashAccountCode);
            return result.ToList();
        }

        //Add 2015/03/18 arc yano 現金出納帳出力(エクセル)
        /// <summary>
        /// 対象年月の前月分の繰越金額を取得
        /// </summary>
        /// <param name="targetDateY">対象年月(年)</param>
        /// <param name="targetDateM">対象年月(月)</param>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="cashAccountCode">現金口座コード</param>
        /// <returns>結果</returns>
        public GetPreAccountResult GetPreAccount(int targetDateY, int targetDateM, string officeCode, string cashAccountCode)
        {
            var result = db.GetPreAccount(targetDateY, targetDateM, officeCode, cashAccountCode).FirstOrDefault();
            return result;
        }

    }
}
