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
    /// 入金予定テーブルアクセスクラス
    ///   入金予定テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class ReceiptPlanDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ReceiptPlanDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 入金予定テーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="receiptPlanId">入金予定ID</param>
        /// <returns>入金予定テーブルデータ(1件)</returns>
        public ReceiptPlan GetByKey(Guid receiptPlanId) {

            // 入金予定データの取得
            ReceiptPlan receiptPlan =
                (from a in db.ReceiptPlan
                 where (a.ReceiptPlanId.Equals(receiptPlanId))
                 && (a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 入金予定データの返却
            return receiptPlan;
        }

        /// <summary>
        /// 入金予定テーブルデータ取得(PK及び権限指定)
        /// </summary>
        /// <param name="receiptPlanId">string型の入金予定ID</param>
        /// <returns>入金予定テーブルデータ(1件)</returns>
        /// <history>Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」の入金実績が削除された場合のバリデーション  既存のメソッドと引数の型が違うため新規作成</history>
        public ReceiptPlan GetByStringKey(string receiptPlanId)
        {

            // 入金予定データの取得
            ReceiptPlan receiptPlan =
                (from a in db.ReceiptPlan
                 where (a.ReceiptPlanId.Equals(receiptPlanId))
                 && (a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 入金予定データの返却
            return receiptPlan;
        }

        /// <summary>
        /// 残高合計額取得
        /// </summary>
        /// <param name="receiptPlanCondition">入金予定検索条件</param>
        /// <returns>残高合計額</returns>
        public decimal GetTotalBalance(ReceiptPlan receiptPlanCondition, bool isShopDeposit)
        {

            /*string officeCode = receiptPlanCondition.OfficeCode;
            string departmentCode = receiptPlanCondition.DepartmentCode;
            string occurredDepartmentCode =  receiptPlanCondition.OccurredDepartmentCode;
            DateTime? journalDateFrom = receiptPlanCondition.JournalDateFrom;
            DateTime? journalDateTo = receiptPlanCondition.JournalDateTo;
            DateTime? receiptPlanDateFrom = receiptPlanCondition.ReceiptPlanDateFrom;
            DateTime? receiptPlanDateTo = receiptPlanCondition.ReceiptPlanDateTo;
            string slipNumber = receiptPlanCondition.SlipNumber;
            string customerClaimCode = null;
            try { customerClaimCode = receiptPlanCondition.CustomerClaim.CustomerClaimCode; } catch (NullReferenceException) { }
            string paymentKindCode = receiptPlanCondition.PaymentKindCode;

            // 残高合計額の取得
           // IQueryable<decimal> amountList =
            var query = 
                    from a in db.ReceiptPlan
                    where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(occurredDepartmentCode) || a.OccurredDepartmentCode.Equals(occurredDepartmentCode))
                    && (string.IsNullOrEmpty(officeCode) || a.OccurredDepartment.OfficeCode.Equals(officeCode))
                    && (journalDateFrom == null || DateTime.Compare(a.JournalDate ?? DaoConst.SQL_DATETIME_MIN,journalDateFrom ?? DaoConst.SQL_DATETIME_MAX)>=0)
                    && (journalDateTo == null || DateTime.Compare(a.JournalDate ?? DaoConst.SQL_DATETIME_MAX,journalDateTo ?? DaoConst.SQL_DATETIME_MIN)<=0)
                    && (receiptPlanDateFrom == null || DateTime.Compare(a.ReceiptPlanDate ?? DaoConst.SQL_DATETIME_MIN, receiptPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (receiptPlanDateTo == null || DateTime.Compare(a.ReceiptPlanDate ?? DaoConst.SQL_DATETIME_MAX, receiptPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Equals(slipNumber))
                    && (string.IsNullOrEmpty(customerClaimCode) || a.CustomerClaimCode.Equals(customerClaimCode))
                    && (a.CompleteFlag.Equals("0"))
                    && (a.DelFlag.Equals("0"))
                    && (string.IsNullOrEmpty(paymentKindCode) || a.PaymentKindCode.Equals(paymentKindCode))
                    select a;

            ParameterExpression param = Expression.Parameter(typeof(ReceiptPlan), "x");
            Expression depExpression = receiptPlanCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(depExpression, param));
            }
            Expression offExpression = receiptPlanCondition.CreateExpressionForOffice(param, new string[] { "ReceiptDepartment", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(offExpression, param));
            }
            Expression comExpression = receiptPlanCondition.CreateExpressionForCompany(param, new string[] { "ReceiptDepartment", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(comExpression, param));
            }
            */

            IQueryable<ReceiptPlan> query;

            if (isShopDeposit)
            {
                query = GetQueryable(receiptPlanCondition).Where(x => x.CustomerClaim != null
                                                                   && x.CustomerClaim.c_CustomerClaimType != null
                                                                   && x.CustomerClaim.c_CustomerClaimType.CustomerClaimFilter.Equals("000")     //請求先タイプ ≠「クレジット／ローン／社内」
                                                                   && (!x.ReceiptType.Equals("012") && !x.ReceiptType.Equals("013"))
                                                                   ).AsQueryable();
            }
            else
            {
                query = GetQueryable(receiptPlanCondition);
            }
            //var query = GetQueryable(receiptPlanCondition);
            var amountList = query.Select(x=>x.ReceivableBalance ?? 0m);
            decimal detailsTotal = (amountList.Count<decimal>() > 0 ? amountList.Sum() : 0m);

            // 残高合計額の返却
            return detailsTotal;
        }

        /// <summary>
        /// 入金予定テーブルデータ検索
        /// （ページング対応）
        /// </summary>
        /// <param name="receiptPlanCondition">入金予定検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>入金予定テーブルデータ検索結果</returns>
        public PaginatedList<ReceiptPlan> GetListByCondition(ReceiptPlan receiptPlanCondition, int? pageIndex, int? pageSize) {
            // ページング制御情報を付与した入金予定データの返却
            PaginatedList<ReceiptPlan> ret = new PaginatedList<ReceiptPlan>(GetQueryable(receiptPlanCondition), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }
        /// <summary>
        /// 入金予定テーブルデータ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">入金予定検索条件</param>
        /// <returns></returns>
        public List<ReceiptPlan> GetListByCondition(DocumentExportCondition condition){
            ReceiptPlan receiptPlanCondition = new ReceiptPlan();
            receiptPlanCondition.ReceiptPlanDateFrom = condition.TermFrom;
            receiptPlanCondition.ReceiptPlanDateTo = condition.TermTo;
            receiptPlanCondition.CustomerClaim = new CustomerClaim();
            receiptPlanCondition.CustomerClaim.CustomerClaimType = condition.CustomerClaimType;
            receiptPlanCondition.DepartmentCode = condition.DepartmentCode;
            receiptPlanCondition.AccountCode = condition.ReceiptPlanType;
            receiptPlanCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(receiptPlanCondition).ToList();
        }

        /// <summary>
        /// 入金予定テーブルデータ検索
        /// （ページング非対応２）
        /// </summary>
        /// <param name="condition">入金予定検索条件</param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/05/17 arc yano #3542 店舗入金消込　一覧の絞込み条件の変更 
        /// </history>
        public List<ReceiptPlan> GetListByCondition(ReceiptPlan condition)
        {
            return GetQueryable(condition).ToList();
        }

        /// <summary>
        /// 入金予定検索条件式を取得する
        /// </summary>
        /// <param name="receiptPlanCondition">検索条件</param>
        /// <returns></returns>
        /// <history>
        ///  2016/05/18 arc yano #3558 入金管理 請求先タイプの絞込み追加
        /// </history>
        private IQueryable<ReceiptPlan> GetQueryable(ReceiptPlan receiptPlanCondition){
            string departmentCode = receiptPlanCondition.DepartmentCode;

            string officeCode = receiptPlanCondition.OfficeCode;
            string occurredDepartmentCode = receiptPlanCondition.OccurredDepartmentCode;
            DateTime? journalDateFrom = receiptPlanCondition.JournalDateFrom;
            DateTime? journalDateTo = receiptPlanCondition.JournalDateTo;
            DateTime? receiptPlanDateFrom = receiptPlanCondition.ReceiptPlanDateFrom;
            DateTime? receiptPlanDateTo = receiptPlanCondition.ReceiptPlanDateTo;
            string slipNumber = receiptPlanCondition.SlipNumber;
            string customerClaimCode = null;
            try { customerClaimCode = receiptPlanCondition.CustomerClaim.CustomerClaimCode; } catch (NullReferenceException) { }
            string customerClaimType = "";
            try { customerClaimType = receiptPlanCondition.CustomerClaim.CustomerClaimType; } catch (NullReferenceException) { }
            string accountCode = receiptPlanCondition.AccountCode;
            string accountUsageType = "";
            try { accountUsageType = receiptPlanCondition.Account.UsageType; } catch (NullReferenceException) { }
            string authCompanyCode = receiptPlanCondition.AuthCompanyCode;
            string receiptType = receiptPlanCondition.ReceiptType;
            string paymentKindCode = receiptPlanCondition.PaymentKindCode;
            DateTime? salesDateFrom = receiptPlanCondition.SalesDateFrom;
            DateTime? salesDateTo = receiptPlanCondition.SalesDateTo;

            //Add 2016/05/18 arc yano #3558
            string customerClaimFilter = null;
            customerClaimFilter = receiptPlanCondition.CustomerClaimFilter;
            string DispType = receiptPlanCondition.DispType;


            // 入金予定データの取得
            var query =
                    from a in db.ReceiptPlan
                    where //(string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    (string.IsNullOrEmpty(occurredDepartmentCode) || a.OccurredDepartmentCode.Equals(occurredDepartmentCode))
                    && (string.IsNullOrEmpty(officeCode) || a.OccurredDepartment.OfficeCode.Equals(officeCode))
                    && (journalDateFrom == null || DateTime.Compare(a.JournalDate ?? DaoConst.SQL_DATETIME_MIN, journalDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (journalDateTo == null || DateTime.Compare(a.JournalDate ?? DaoConst.SQL_DATETIME_MAX, journalDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (receiptPlanDateFrom == null || DateTime.Compare(a.ReceiptPlanDate ?? DaoConst.SQL_DATETIME_MIN, receiptPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (receiptPlanDateTo == null || DateTime.Compare(a.ReceiptPlanDate ?? DaoConst.SQL_DATETIME_MAX, receiptPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        //&& (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Substring(0,8).Equals(slipNumber))
                        // 2011.11.16 伝票番号を部分検索に変更
                    && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Contains(slipNumber))
                    && (string.IsNullOrEmpty(customerClaimCode) || a.CustomerClaimCode.Equals(customerClaimCode))
                    && (string.IsNullOrEmpty(customerClaimType) || a.CustomerClaim.CustomerClaimType.Equals(customerClaimType))
                    && (string.IsNullOrEmpty(customerClaimFilter) || a.CustomerClaim.c_CustomerClaimType.CustomerClaimFilter.Equals(customerClaimFilter))    //Add 2016/05/18 arc yano #3558
                    && (string.IsNullOrEmpty(accountCode) || a.AccountCode.Equals(accountCode))
                    && (string.IsNullOrEmpty(accountUsageType) || a.Account.UsageType.Equals(accountUsageType))
                    && (string.IsNullOrEmpty(authCompanyCode) || a.ReceiptDepartment.Office.CompanyCode.Equals(authCompanyCode))
                        //&& ((string.IsNullOrEmpty(authOfficeCode) || a.ReceiptDepartment.OfficeCode.Equals(authOfficeCode))
                        //|| (string.IsNullOrEmpty(authOfficeCode1) || a.ReceiptDepartment.OfficeCode.Equals(authOfficeCode1))
                        //|| (string.IsNullOrEmpty(authOfficeCode2) || a.ReceiptDepartment.OfficeCode.Equals(authOfficeCode2))
                        //|| (string.IsNullOrEmpty(authOfficeCode3) || a.ReceiptDepartment.OfficeCode.Equals(authOfficeCode3)))
                        //&& ((string.IsNullOrEmpty(authDepartmentCode) || a.DepartmentCode.Equals(authDepartmentCode))
                        //|| (string.IsNullOrEmpty(authDepartmentCode1) || a.DepartmentCode.Equals(authDepartmentCode1))
                        //|| (string.IsNullOrEmpty(authDepartmentCode2) || a.DepartmentCode.Equals(authDepartmentCode2))
                        //|| (string.IsNullOrEmpty(authDepartmentCode3) || a.DepartmentCode.Equals(authDepartmentCode3)))
                    && (a.CompleteFlag.Equals("0"))
                    && (a.DelFlag.Equals("0"))
                    && (string.IsNullOrEmpty(receiptType) || a.ReceiptType.Equals(receiptType))
                    && (string.IsNullOrEmpty(paymentKindCode) || a.PaymentKindCode.Equals(paymentKindCode))
                    && (salesDateFrom == null ||
                        (from c in db.CarSalesHeader
                            where DateTime.Compare(c.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0
                            select c.SlipNumber).Contains(a.SlipNumber) ||
                        (from s in db.ServiceSalesHeader
                            where DateTime.Compare(s.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0
                            select s.SlipNumber).Contains(a.SlipNumber))
                    && (salesDateTo == null ||
                        (from c in db.CarSalesHeader
                            where DateTime.Compare(c.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0
                            select c.SlipNumber).Contains(a.SlipNumber) ||
                        (from s in db.ServiceSalesHeader
                            where DateTime.Compare(s.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0
                            select s.SlipNumber).Contains(a.SlipNumber))
                    //orderby a.ReceiptPlanDate
                    //, a.DepartmentCode
                    //, ((a.CustomerClaim.CustomerClaimType != null && !a.CustomerClaim.CustomerClaimType.Equals("")) ? "1" : "2")
                    //, a.CustomerClaim.CustomerClaimType
                    //, ((a.SlipNumber != null && !a.SlipNumber.Equals("")) ? "1" : "2")
                    //, a.SlipNumber
                    //, a.CustomerClaimCode
                    //, a.ReceiptPlanId
                    select a;

            ParameterExpression param = Expression.Parameter(typeof(ReceiptPlan), "x");
            Expression depExpression = receiptPlanCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(depExpression, param));
            }
            Expression offExpression = receiptPlanCondition.CreateExpressionForOffice(param, new string[] { "ReceiptDepartment", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(offExpression, param));
            }
            Expression comExpression = receiptPlanCondition.CreateExpressionForCompany(param, new string[] { "ReceiptDepartment", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<ReceiptPlan, bool>>(comExpression, param));
            }

            var receiptPlanList = query
                .OrderBy(x => x.ReceiptPlanDate)
                .OrderBy(x => x.CustomerClaimCode)
                .OrderBy(x => db.CarSalesHeader.Where(y => y.SlipNumber.Equals(x.SlipNumber) && y.DelFlag.Equals("0")).FirstOrDefault().SalesDate != null ? 
                    db.CarSalesHeader.Where(y => y.SlipNumber.Equals(x.SlipNumber) && y.DelFlag.Equals("0")).FirstOrDefault().SalesDate :
                    db.ServiceSalesHeader.Where(y => y.SlipNumber.Equals(x.SlipNumber) && y.DelFlag.Equals("0")).FirstOrDefault().SalesDate)
                .OrderBy(x => x.ReceiptDepartment.OfficeCode);

                //.OrderBy(x => x.DepartmentCode)
                //.OrderBy(x => x.CustomerClaim.CustomerClaimType != null && !x.CustomerClaim.CustomerClaimType.Equals("") ? "1" : "2")
                //.OrderBy(x => x.CustomerClaim.CustomerClaimType)
                //.OrderBy(x => x.SlipNumber != null && x.SlipNumber.Equals("") ? "1" : "2")
                //.OrderBy(x => x.ReceiptPlanId);
            return receiptPlanList;

         
        }

        /// <summary>
        /// 入金予定日からN日経過した入金予定データを取得する
        /// （既にタスクとして存在するものは除外する）
        /// </summary>
        /// <param name="conditionDate">基準となる日付</param>
        /// <returns>入金予定データリスト</returns>
        public List<ReceiptPlan> GetExpiredList(DateTime? conditionDate,double n) {
            DateTime limitDate = (conditionDate ?? DateTime.Today).AddDays(n);
            var query =
                from a in db.ReceiptPlan
                where (conditionDate == null || DateTime.Compare(limitDate, a.ReceiptPlanDate ?? DaoConst.SQL_DATETIME_MAX) > 0)
                && a.DelFlag.Equals("0")
                && (
                    (from b in db.Task
                     where !b.DelFlag.Equals("1")
                     select b.SlipNumber).Distinct()
                    ).Contains(a.SlipNumber)
                select a;
            return query.ToList();
        }

        /// <summary>
        /// 入金予定データを取得（伝票番号指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入金予定データリスト</returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<ReceiptPlan> GetBySlipNumber(string slipNumber) {
            IQueryable<ReceiptPlan> query =
                from a in db.ReceiptPlan
                orderby a.ReceiptPlanDate
                where a.SlipNumber.Equals(slipNumber)
                && !a.DelFlag.Equals("1")
                select a;


            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<ReceiptPlan>();
            //return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// 入金予定データを取得（伝票番号、部門指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="departmentCode"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<ReceiptPlan> GetBySlipNumber(string slipNumber, string departmentCode) {
            IQueryable<ReceiptPlan> query =
                from a in db.ReceiptPlan
                orderby a.Amount
                where a.SlipNumber.Equals(slipNumber)
                && a.DepartmentCode.Equals(departmentCode)
                && a.DelFlag.Equals("0")
                select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<ReceiptPlan>();
            //return query.ToList<ReceiptPlan>();
        }


        /// <summary>
        /// 入金予定データを取得（伝票番号、金種）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="receiptType"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<ReceiptPlan> GetCashBySlipNumber(string slipNumber, string receiptType) {
            IQueryable<ReceiptPlan> query =
                from a in db.ReceiptPlan
                where a.SlipNumber.Equals(slipNumber)
                && a.ReceiptType.Equals(receiptType)
                && a.DelFlag.Equals("0")
                select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<ReceiptPlan>();
            //return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// 売掛残高を取得（伝票番号、請求先）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public decimal GetAmountByCustomerClaim(string slipNumber, string customerClaimCode) {
            var query =
                from a in db.ReceiptPlan
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                select a;
            return query.Sum(x => x.ReceivableBalance) ?? 0m;
        }

        /// <summary>
        /// 現金だけの売掛残高を取得（伝票番号、請求先）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        /// <history>Add arc nakayama 2016/06/30 #3593_伝票に対して同一請求先が複数あった場合の考慮</history>
        public decimal GetCashAmountByCustomerClaim(string slipNumber, string customerClaimCode)
        {
            var query =
                from a in db.ReceiptPlan
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && a.ReceiptType.Equals("001") //現金のみ
                select a;
            return query.Sum(x => x.ReceivableBalance) ?? 0m;
        }


        /// <summary>
        /// 預かり金の入金予定データを取得する（伝票番号指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <returns></returns>
        /// <history>Mod 2016/05/17 arc nakayama #3538_諸費用以外も消込保存が行えるようにする 諸費用フラグを条件から外す</history>
        /// <history>Mod 2016/06/29 arc nakayama #3593_伝票に対して同一請求先が複数あった場合の考慮 入金種別が「カード会社からの入金」は除外する</history>
        public List<ReceiptPlan> GetDepositBySlipNumber(string slipNumber, string departmentCode) {
            IQueryable<ReceiptPlan> query =
                from a in db.ReceiptPlan
                orderby a.Amount
                where a.SlipNumber.Equals(slipNumber)
                && a.ReceiptType.Equals("001") //入金種別が「現金」の予定のみ
                //&& a.DepositFlag.Equals("1")
                && a.DelFlag.Equals("0")
                select a;
            return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// 入金予定データを取得（伝票番号、請求先）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public ReceiptPlan GetByCustomerClaim(string slipNumber, string customerClaimCode)
        {
            var query =
                (from a in db.ReceiptPlan
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 現金の入金予定データを取得（伝票番号、請求先）//金額の多い順
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public List<ReceiptPlan> GetCashListByCustomerClaimDesc(string slipNumber, string customerClaimCode)
        {
            var query =
                from a in db.ReceiptPlan
                orderby a.Amount descending
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && a.ReceiptType.Equals("001") //現金のみ
                select a;
            return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// 現金の入金予定データを取得（伝票番号、請求先）//金額の少ない順
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public List<ReceiptPlan> GetCashListByCustomerClaim(string slipNumber, string customerClaimCode)
        {
            var query =
                from a in db.ReceiptPlan
                orderby a.Amount
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && a.ReceiptType.Equals("001") //現金のみ
                select a;
            return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// カード会社からの入金以外の予定データを取得（伝票番号、請求先）//金額の多い順
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public List<ReceiptPlan> GetListByCustomerClaimDesc(string slipNumber, string customerClaimCode)
        {
            var query =
                from a in db.ReceiptPlan
                orderby a.Amount descending
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && !a.ReceiptType.Equals("011") //カード会社からの入金以外
                select a;
            return query.ToList<ReceiptPlan>();
        }

        /// <summary>
        /// カード会社からの入金以外の予定データを取得（伝票番号、請求先）//金額の少ない順
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        public List<ReceiptPlan> GetListByCustomerClaim(string slipNumber, string customerClaimCode)
        {
            var query =
                from a in db.ReceiptPlan
                orderby a.Amount
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && !a.ReceiptType.Equals("011") //カード会社からの入金以外
                select a;
            return query.ToList<ReceiptPlan>();
        }


        /// <summary>
        /// 入金予定テーブルデータ検索　伝票と請求先の合計で検索した場合
        /// （ストプロから取得）
        /// </summary>
        /// <param name="condition">入金予定検索条件</param>
        /// <returns></returns>
        /// <history>
        /// Add 2016/07/20 arc nakayama #3580_入金予定のサマリ表示（入金実績リスト出力・店舗入金・入金管理）
        /// </history>
        public List<ReceiptPlan> GetSummaryListByCondition(ReceiptPlan condition)
        {
            var query = db.GetSummaryReceiptPlan(condition.OfficeCode,
                                                  string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                                  string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                                  string.Format("{0:yyyy/MM/dd}", condition.JournalDateFrom),
                                                  string.Format("{0:yyyy/MM/dd}", condition.JournalDateTo),
                                                  string.Format("{0:yyyy/MM/dd}", condition.ReceiptPlanDateFrom),
                                                  string.Format("{0:yyyy/MM/dd}", condition.ReceiptPlanDateTo),
                                                  condition.SlipNumber,
                                                  condition.CustomerClaim.CustomerClaimCode,
                                                  condition.CustomerClaim.CustomerClaimType,
                                                  condition.Account.UsageType,
                                                  condition.ReceiptType,
                                                  condition.PaymentKindCode,
                                                  condition.CustomerClaimFilter,
                                                  condition.IsShopDeposit);

            List<ReceiptPlan> RetList = new List<ReceiptPlan>();

            foreach (var ret in query)
            {
                ReceiptPlan plan = new ReceiptPlan();
                plan.ReceiptDepartment = new Department();
                
                plan.CustomerClaim = new CustomerClaim();
                plan.CarSalesHeader = new CarSalesHeader();
                plan.ServiceSalesHeader = new ServiceSalesHeader();

                plan.strReceiptPlanId = ret.ReceiptPlanId;
                plan.ReceiptDepartment.DepartmentCode = ret.OccurredDepartmentCode;
                plan.ReceiptDepartment.OfficeCode = ret.OfficeCode;
                Office off = new Office();
                off.OfficeCode = ret.OfficeCode;
                plan.ReceiptDepartment.Office = off;
                plan.ReceiptDepartment.Office.OfficeName = ret.OfficeName;
                plan.OccurredDepartmentCode = ret.OccurredDepartmentCode;
                plan.CustomerClaim.CustomerClaimCode = ret.CustomerClaimCode;
                plan.CustomerClaim.CustomerClaimName = ret.CustomerClaimName;
                plan.SlipNumber = ret.SlipNumber;
                plan.ReceiptType = ret.ReceiptType;
                plan.ReceiptPlanDate = ret.ReceiptPlanDate;
                plan.AccountCode = ret.AccountCode;
                plan.Amount = ret.Amount;
                plan.ReceivableBalance = ret.ReceivableBalance;
                plan.Summary = ret.Summary;
                plan.JournalDate = ret.JournalDate;
                plan.PaymentKindCode = ret.PaymentKindCode;
                plan.CommissionRate = ret.CommissionRate;
                plan.CommissionAmount = ret.CommissionAmount;
                plan.CreditJournalId = ret.CreditJournalId;
                if (ret.accountUsageType == "CR")
                {
                    plan.CarSalesHeader.SalesDate = ret.SalesDate;
                    plan.CarSalesHeader.SalesPlanDate = ret.SalesPlanDate;
                }
                else
                {
                    plan.ServiceSalesHeader.SalesDate = ret.SalesDate;
                    plan.ServiceSalesHeader.SalesPlanDate = ret.SalesPlanDate;
                }

                RetList.Add(plan);
            }

            return RetList;
        }

        /// <summary>
        ///入金予定データを取得（伝票番号）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<ReceiptPlan> GetListByslipNumber(string slipNumber)
        {
            var query =
                from a in db.ReceiptPlan
                where a.DelFlag.Equals("0")
                && a.SlipNumber.Equals(slipNumber)
                select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<ReceiptPlan>();

            //return query.ToList<ReceiptPlan>();
        }
    }
}
