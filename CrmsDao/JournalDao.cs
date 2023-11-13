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
    /// 出納帳テーブルアクセスクラス
    ///   出納帳テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class JournalDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public JournalDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 入出金額合計取得（伝票番号指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入出金額合計</returns>
        /// <history>
        /// Mod 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public decimal GetTotalBySlipNumber(string slipNumber) {
            IQueryable<decimal> query =
                from a in db.Journal
                where a.SlipNumber.Equals(slipNumber)
                && a.DelFlag.Equals("0")
                && a.ReceiptPlanFlag.Equals("1")
                select decimal.Multiply(a.Amount, (a.JournalType.Equals("001") ? 1m : -1m));


            var result = CommonUtils.SelectWithUpdlock(db, query);

            decimal ret = 0m;

            foreach (var a in result.ToList<decimal>())
            {
                ret += a;
            }

            return ret;

            //return (query.Count<decimal>() > 0 ? query.Sum() : 0m);
        }

        /// <summary>
        /// 入金額合計取得（伝票番号・請求先指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="customerClaimCode">請求先</param>
        /// <returns>入出金額合計</returns>
        public decimal GetTotalByCustomerAndSlip(string slipNumber, string customerClaimCode) {
            return GetTotalByCondition(slipNumber, customerClaimCode, true);
        }

        /// <summary>
        /// 入金合計取得（伝票番号・請求先・クレジットローンを含むかどうか指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <param name="accountType"></param>
        /// <returns></returns>
        /// <history>
        /// Mod 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// //Add 2016/09/06 arc nakayama #3630_【製造】車両売掛金対応 下取・残債を含めるかどうかの引数追加(default：含めない)
        /// </history>
        public decimal GetTotalByCondition(string slipNumber, string customerClaimCode, bool includeCredit, bool includeTrade = false)
        {
            IQueryable<decimal> query =
                from a in db.Journal
                where a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && a.DelFlag.Equals("0")
                && a.JournalType.Equals("001")
                && a.ReceiptPlanFlag.Equals("1")
                && (includeCredit || (!a.AccountType.Equals("003") && !a.AccountType.Equals("004")))
                && (includeTrade || (!a.AccountType.Equals("012") && !a.AccountType.Equals("013")))
                select a.Amount;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            decimal ret = 0m;

            foreach (var a in result.ToList<decimal>())
            {
                ret += a;
            }

            return ret;

            //return (query.Count<decimal>() > 0 ? query.Sum() : 0m);
        }

        /// <summary>
        /// 入金合計取得（伝票番号・請求先・クレジットローンを含むかどうか指定・プラスかマイナスかを指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <param name="accountType"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/09/25 arc yano  #3798　入金実績リスト　マイナスの入金予定の考慮　計算過程で、一旦入金実績テーブルのレコードを持ってくるように変更
        /// 2017/06/16 arc nakayama #3772_【車両伝票】支払情報にマイナスの金額を入れた時の考慮 新規作成
        /// </history>
        public decimal GetPlusMinusTotalByCondition(string slipNumber, string customerClaimCode, bool includeCredit, bool PlusValue ,bool includeTrade = false)
        {
            //Mod 2017/09/25 arc yano  #3798
            //IQueryable<decimal> query;        
            IQueryable<Journal> query;

            if (PlusValue)
            {
                query =
                    from a in db.Journal
                    where a.SlipNumber.Equals(slipNumber)
                    && a.CustomerClaimCode.Equals(customerClaimCode)
                    && a.DelFlag.Equals("0")
                    && a.JournalType.Equals("001")
                    && a.ReceiptPlanFlag.Equals("1")
                    && (includeCredit || (!a.AccountType.Equals("003") && !a.AccountType.Equals("004")))
                    && (includeTrade || (!a.AccountType.Equals("012") && !a.AccountType.Equals("013")))
                    && a.Amount >= 0
                    // select a.Amount;  //Mod 2017/09/25 arc yano  #3798
                    select a;
            }
            else
            {
                query =
                    from a in db.Journal
                    where a.SlipNumber.Equals(slipNumber)
                    && a.CustomerClaimCode.Equals(customerClaimCode)
                    && a.DelFlag.Equals("0")
                    && a.JournalType.Equals("001")
                    && a.ReceiptPlanFlag.Equals("1")
                    && (includeCredit || (!a.AccountType.Equals("003") && !a.AccountType.Equals("004")))
                    && (includeTrade || (!a.AccountType.Equals("012") && !a.AccountType.Equals("013")))
                    && a.Amount < 0
                    //select a.Amount;  //Mod 2017/09/25 arc yano  #3798
                    select a;
            }

            var result = CommonUtils.SelectWithUpdlock(db, query);

            decimal ret = 0m;

            //foreach (var a in result.ToList<decimal>())   //Mod 2017/09/25 arc yano  #3798
            foreach (var a in result.ToList<Journal>())
            {
                ret += a.Amount;
            }

            return ret;

            //return (query.Count<decimal>() > 0 ? query.Sum() : 0m);
        }

        /// <summary>
        /// 入出金リストを取得する（伝票番号、請求先指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="customerClaimCode"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<Journal> GetListByCustomerAndSlip(string slipNumber, string customerClaimCode) {
            var query =
                from a in db.Journal
                where a.SlipNumber.Equals(slipNumber)
                && a.CustomerClaimCode.Equals(customerClaimCode)
                && a.DelFlag.Equals("0")
                && a.JournalType.Equals("001")
                && a.ReceiptPlanFlag.Equals("1")
                select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<Journal>();

            //return query.ToList();
        }

        /// <summary>
        /// 入金リストを取得する（伝票番号、部門指定）
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="departmentCode"></param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// </history>
        public List<Journal> GetListByDepartmentAndSlip(string slipNumber, string departmentCode) {
            var query =
                from a in db.Journal
                where a.SlipNumber.Equals(slipNumber)
                && a.DepartmentCode.Equals(departmentCode)
                && a.DelFlag.Equals("0")
                && a.JournalType.Equals("001")
                select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<Journal>();

            //return query.ToList();
        }

        /// <summary>
        /// 入出金リスト取得（伝票番号指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="excluedList">請求先種別除外リスト</param>
        /// <returns>入出金リスト</returns>
        /// <history>
        /// Mod 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// Mod 2016/04/05 arc yano #3441　カード入金消込時マイナスの入金予定ができてしまう 引数(excluedList)追加
        /// Mod 2016/10/03 arc nakayama #3630_【製造】車両売掛金対応 全額ローンで支払った場合、下取車が納車済だとマイナスの入金予定を作成してしまう。　引数(excluedAccounTypetList)追加
        /// </history>
        public List<Journal> GetListBySlipNumber(string slipNumber, List<string> excluedList = null, List<string> excluedAccountTypetList = null)
        {
            IQueryable<Journal> query =
                from a in db.Journal
                where a.SlipNumber.Equals(slipNumber)
                && a.DelFlag.Equals("0")
                && a.JournalType.Equals("001")
                //&& !string.IsNullOrEmpty(a.CustomerClaimCode)
                select a;

            //Mod 2016/04/04 arc yano #3441 請求先種別による絞り込み
            if (excluedList != null && excluedList.Count() > 0)
            {
                foreach (var exclude in excluedList)
                {
                    query = query.Where(x => x.CustomerClaimCode == null || !x.CustomerClaim.CustomerClaimType.Equals(exclude));
                }
            }

            //Mod 2016/10/03 arc nakayama #3630_【製造】車両売掛金対応
            if (excluedAccountTypetList != null && excluedAccountTypetList.Count() > 0)
            {
                foreach (var exAccount in excluedAccountTypetList)
                {
                    query = query.Where(x => x.AccountType == null || !x.AccountType.Equals(exAccount));
                }
            }

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<Journal>();
                
            //return query.ToList();
        }

        /// <summary>
        /// 入出金リスト取得（伝票番号指定　入金実績の計算に含める入金種別のみ）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入金リスト</returns>
        /// <history>
        /// Mod 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        // Add 2015/11/02 arc nakayama #3297_サービス伝票入力画面の入金実績設定値の不具合
        /// </history>
        public List<Journal> GetJournalCalcListBySlipNumber(string slipNumber)
        {
            IQueryable<Journal> query =
                (from a in db.Journal
                 where a.SlipNumber.Equals(slipNumber)
                 && a.DelFlag.Equals("0")
                 && a.JournalType.Equals("001")
                 && (from b in db.c_AccountType where b.JournalCalcFlag.Equals("1") select b.Code).Contains(a.AccountType)
                 select a);

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<Journal>();

            //return query.ToList();
        }

        /// <summary>
        /// 入出金額合計取得
        /// </summary>
        /// <param name="journalCondition">出納帳検索条件(参照権限，部門，期間)</param>
        /// <returns>入出金額合計</returns>
        public decimal GetDetailsTotal(Journal journalCondition) {

            string authCompanyCode = journalCondition.AuthCompanyCode;
            //string authOfficeCode = journalCondition.AuthOfficeCode;
            //string authOfficeCode1 = journalCondition.AuthOfficeCode1;
            //string authOfficeCode2 = journalCondition.AuthOfficeCode2;
            //string authOfficeCode3 = journalCondition.AuthOfficeCode3;
            //string authDepartmentCode = journalCondition.AuthDepartmentCode;
            string officeCode = journalCondition.OfficeCode;
            DateTime? journalDateFrom = journalCondition.JournalDateFrom;
            DateTime? journalDateTo = journalCondition.JournalDateTo;
            string accountType = journalCondition.AccountType;
            string cashAccountCode = journalCondition.CashAccountCode;
            string slipNumber = journalCondition.SlipNumber;

            // 入出金額合計の取得
            IQueryable<decimal> amountList =
                (from a in db.Journal
                 where (string.IsNullOrEmpty(authCompanyCode) || a.Office.CompanyCode.Equals(authCompanyCode))
                 //&& ((string.IsNullOrEmpty(authOfficeCode) || a.OfficeCode.Equals(authOfficeCode))
                 //|| (string.IsNullOrEmpty(authOfficeCode1) || a.OfficeCode.Equals(authOfficeCode1))
                 //|| (string.IsNullOrEmpty(authOfficeCode2) || a.OfficeCode.Equals(authOfficeCode2))
                 //|| (string.IsNullOrEmpty(authOfficeCode3) || a.OfficeCode.Equals(authOfficeCode3)))
                 && (a.OfficeCode.Equals(officeCode))
                 && (DateTime.Compare(a.JournalDate, journalDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                 && (DateTime.Compare(a.JournalDate, journalDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                 && (string.IsNullOrEmpty(accountType) || a.AccountType.Equals(accountType))
                 && (a.CashAccountCode.Equals(cashAccountCode))
                 && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Equals(slipNumber))
                 && (a.DelFlag.Equals("0"))
                 && (!a.AccountType.Equals("013")) //下取以外
                 select decimal.Multiply(a.Amount, (a.JournalType.Equals("001") ? 1m : -1m))
                );
            decimal detailsTotal = (amountList.Count<decimal>() > 0 ? amountList.Sum() : 0m);

            // 入出金額合計の返却
            return detailsTotal;
        }

        /// <summary>
        /// 出納帳テーブルデータ検索
        /// (ページング対応)
        /// </summary>
        /// <param name="journalCondition">出納帳検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>出納帳テーブルデータ検索結果</returns>
        public PaginatedList<Journal> GetListByCondition(Journal journalCondition, int? pageIndex, int? pageSize) {
            // ページング制御情報を付与した出納帳データの返却
            PaginatedList<Journal> ret = new PaginatedList<Journal>(GetQueryable(journalCondition), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }
        /// <summary>
        /// 出納長テーブルデータ検索（ページングなし）
        /// </summary>
        /// <param name="journalCondition"></param>
        /// <returns></returns>
        public List<Journal> GetListByCondition(Journal journalCondition) {
            return GetQueryable(journalCondition).ToList();
        }
        /// <summary>
        /// 出納帳テーブルデータ検索
        /// (ページング非対応)
        /// </summary>
        /// <param name="condition">帳票検索条件</param>
        /// <returns></returns>
        public List<Journal> GetListByCondition(DocumentExportCondition condition){
            Journal journalCondition = new Journal();
            journalCondition.JournalDateFrom = condition.TermFrom;
            journalCondition.JournalDateTo = condition.TermTo;
            journalCondition.DepartmentCode = condition.DepartmentCode;
            journalCondition.CustomerClaimCode = condition.CustomerClaimCode;
            journalCondition.OfficeCode = condition.OfficeCode;
            journalCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(journalCondition).ToList();
        }

        /// <summary>
        /// 出納帳データ検索
        /// </summary>
        /// <param name="journalCondition"></param>
        /// <returns></returns>
        private IQueryable<Journal> GetQueryable(Journal journalCondition){
            string authCompanyCode = journalCondition.AuthCompanyCode;
            //string authOfficeCode = journalCondition.AuthOfficeCode;
            //string authOfficeCode1 = journalCondition.AuthOfficeCode1;
            //string authOfficeCode2 = journalCondition.AuthOfficeCode2;
            //string authOfficeCode3 = journalCondition.AuthOfficeCode3;
            //string authDepartmentCode = journalCondition.AuthDepartmentCode;
            string officeCode = journalCondition.OfficeCode;
            string departmentCode = journalCondition.DepartmentCode;
            string journalType = journalCondition.JournalType;
            DateTime? journalDateFrom = journalCondition.JournalDateFrom;
            DateTime? journalDateTo = journalCondition.JournalDateTo;
            string accountType = journalCondition.AccountType;
            string cashAccount = journalCondition.CashAccountCode;
            string slipNumber = journalCondition.SlipNumber;
            string customerClaimCode = journalCondition.CustomerClaimCode;
            DateTime? journalDate = journalCondition.CondJournalDate;
            string accountCode = journalCondition.AccountCode;
            string summary = journalCondition.Summary;

            // 出納帳データの取得
            var journalList =
                    from a in db.Journal
                    where (string.IsNullOrEmpty(officeCode) || a.OfficeCode.Equals(officeCode))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(journalType) || a.JournalType.Equals(journalType))
                    && (journalDate==null || a.JournalDate.Equals(journalDate))
                    && (journalDateFrom == null || DateTime.Compare(a.JournalDate, journalDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (journalDateTo == null || DateTime.Compare(a.JournalDate, journalDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(accountType) || a.AccountType.Equals(accountType))
                    && (string.IsNullOrEmpty(cashAccount) || a.CashAccountCode.Equals(cashAccount))
                    // 2011.11.16 伝票番号を部分検索に変更
                    //&& (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Substring(0,8).Equals(slipNumber))
                    && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Contains(slipNumber))
                    && (string.IsNullOrEmpty(customerClaimCode) || a.CustomerClaimCode.Equals(customerClaimCode))
                    && (string.IsNullOrEmpty(accountCode) || a.AccountCode.Equals(accountCode))
                    && (string.IsNullOrEmpty(summary) || a.Summary.Contains(summary))
                    && (a.DelFlag.Equals("0"))
                    select a;

            ParameterExpression param = Expression.Parameter(typeof(Journal), "x");
            Expression offExpression = journalCondition.CreateExpressionForOffice(param, new string[] { "OfficeCode" });
            if (offExpression != null) {
                journalList = journalList.Where(Expression.Lambda<Func<Journal, bool>>(offExpression, param));
            }
            Expression comExpression = journalCondition.CreateExpressionForCompany(param, new string[] { "Office", "CompanyCode" });
            if (comExpression != null) {
                journalList = journalList.Where(Expression.Lambda<Func<Journal, bool>>(comExpression, param));
            }

            //return journalList.OrderBy(x => x.JournalDate).OrderBy(x => x.DepartmentCode).OrderBy(x => x.CreateDate).OrderBy(x => x.JournalId);
            return journalList.OrderBy(x => x.JournalDate).ThenBy(x => x.DepartmentCode).ThenBy(x => x.CreateDate).ThenBy(x => x.JournalId);
        }

        /// <summary>
        /// JournalIdから1件の出納帳データを取得する
        /// </summary>
        /// <param name="id">出納帳ID</param>
        /// <returns></returns>
        public Journal GetByKey(Guid id) {
            var query = (from a in db.Journal
                        where a.JournalId.Equals(id)
                           && a.DelFlag.Equals("0")
                         select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// JournalIdから1件の出納帳データを取得する
        /// </summary>
        /// <param name="id">出納帳ID</param>
        /// <returns></returns>
        /// <history>Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」の入金実績が削除された場合のバリデーション  既存のメソッドと引数の型が違うため新規作成</history>
        public Journal GetByStringKey(string id)
        {
            var query = (from a in db.Journal
                         where a.JournalId.Equals(id)
                            && a.DelFlag.Equals("0")
                         select a).FirstOrDefault();
            return query;
        }

        //Add 2015/03/18 arc yano 現金出納帳出力(エクセル)
        /// <summary>
        /// 現金出納帳データを取得する
        /// </summary>
        /// <param name="targetDateY">対象年月(年)</param>
        /// <param name="targetDateM">対象年月(月)</param>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="cashAccountCode">現金口座コード</param>
        /// <returns></returns>
        public List<GetJournalCashResult> GetGetJournalCash(int targetDateY, int targetDateM, string officeCode, string cashAccountCode)
        {
            var resut = db.GetJournalCash(targetDateY, targetDateM, officeCode, cashAccountCode);
            return resut.ToList();
        }

        /// <summary>
        /// ReceiptPlanIDから出納帳データを取得する
        /// </summary>
        /// <param name="id">入金予定ID</param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// Add 2016/05/16 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」の入金実績が削除された場合のバリデーション  既存のメソッドと引数の型が違うため新規作成
        /// </history>
        public List<Journal> GetByReceiptPlanID(string id)
        {
            var query = from a in db.Journal
                         where a.CreditReceiptPlanId.Equals(id)
                            && a.DelFlag.Equals("0")
                         select a;

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList<Journal>();

            //return query.ToList();
        }

        /// <summary>
        /// ReceiptPlanIDと入金種別から出納帳データを取得する
        /// </summary>
        /// <param name="id">入金予定ID</param>
        /// <param name="id">入金予定ID</param>
        /// <returns></returns>
        /// <history>
        /// 2017/04/11 arc yano #3753 デッドロック対策 暫定版
        /// Add 2016/06/02 arc nakayama #3544_入金種別をカード・ローンには変更させない 入金種別が「カード」の入金実績が削除された場合のバリデーション  既存のメソッドと引数の型が違うため新規作成
        /// </history>
        public Journal GetByPlanIDAccountType(string id, string AccountType)
        {
            var query = (from a in db.Journal
                        where a.CreditReceiptPlanId.Equals(id)
                           && a.AccountType.Equals(AccountType)
                           && a.DelFlag.Equals("0")
                        select a);

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.FirstOrDefault();

            //return query;
        }


        /// <summary>
        /// カード入金確認画面検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>リスト型の検索結果（ページングデータあり）</returns>
        /// <history>Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善</history>
        public PaginatedList<GetCreditJournal_Result> GetCreditJournal(GetCreditJournalSearchCondition condition, int? pageIndex, int? pageSize)
        {
            var RetData = db.GetCreditJournal(string.Format("{0:yyyy/MM/dd}", condition.JournalDateFrom),
                                          string.Format("{0:yyyy/MM/dd}", condition.JournalDateTo),
                                          string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                          string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                          condition.SlipType,
                                          condition.SlipNumber,
                                          condition.DepartmentCode,
                                          condition.CustomerCode,
                                          condition.CustomerClaimCode,
                                          condition.CompleteFlag);

            List<GetCreditJournal_Result> RetList = new List<GetCreditJournal_Result>();

            foreach (var ret in RetData)
            {
                GetCreditJournal_Result ResultData = new GetCreditJournal_Result();

                ResultData.SlipTypeName = ret.SlipTypeName;
                ResultData.SlipNumber = ret.SlipNumber;
                ResultData.StatusName = ret.StatusName;
                ResultData.OccurredDepartmentCode = ret.OccurredDepartmentCode;
                ResultData.DepartmentName = ret.DepartmentName;
                ResultData.SalesOrderDate = ret.SalesOrderDate;
                ResultData.SalesDate = ret.SalesDate;
                ResultData.CustomerCode = ret.CustomerCode;
                ResultData.CustomerName = ret.CustomerName;
                ResultData.JournalDate = ret.JournalDate;
                ResultData.CustomerClaimCode = ret.CustomerClaimCode;
                ResultData.CustomerClaimName = ret.CustomerClaimName;
                ResultData.Amount = ret.Amount;
                ResultData.CompleteFlagName = ret.CompleteFlagName;
                ResultData.Summary = ret.Summary;
                ResultData.AccountCode = ret.AccountCode;
                ResultData.AccountName = ret.AccountName;

                RetList.Add(ResultData);
            }

            return new PaginatedList<GetCreditJournal_Result>(RetList.AsQueryable<GetCreditJournal_Result>(), pageIndex ?? 0, pageSize ?? 0);

        }

        /// <summary>
        /// カード入金確認画面検索(Excel用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>リスト型の検索結果</returns>
        /// <history>Add 2016/08/17 arc nakayama #3595_【大項目】車両売掛金機能改善</history>
        public List<GetCreditJournal_ExcelResult> GetCreditJournalForExcel(GetCreditJournalSearchCondition condition)
        {
            var RetData = db.GetCreditJournal(string.Format("{0:yyyy/MM/dd}", condition.JournalDateFrom),
                                          string.Format("{0:yyyy/MM/dd}", condition.JournalDateTo),
                                          string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                          string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                          condition.SlipType,
                                          condition.SlipNumber,
                                          condition.DepartmentCode,
                                          condition.CustomerCode,
                                          condition.CustomerClaimCode,
                                          condition.CompleteFlag);

            List<GetCreditJournal_ExcelResult> RetList = new List<GetCreditJournal_ExcelResult>();

            foreach (var ret in RetData)
            {
                GetCreditJournal_ExcelResult ResultData = new GetCreditJournal_ExcelResult();

                ResultData.SlipTypeName = ret.SlipTypeName;
                ResultData.SlipNumber = ret.SlipNumber;
                ResultData.StatusName = ret.StatusName;
                ResultData.OccurredDepartmentCode = ret.OccurredDepartmentCode;
                ResultData.DepartmentName = ret.DepartmentName;
                ResultData.SalesOrderDate = ret.SalesOrderDate;
                ResultData.SalesDate = ret.SalesDate;
                ResultData.CustomerCode = ret.CustomerCode;
                ResultData.CustomerName = ret.CustomerName;
                ResultData.JournalDate = ret.JournalDate;
                ResultData.CustomerClaimCode = ret.CustomerClaimCode;
                ResultData.CustomerClaimName = ret.CustomerClaimName;
                ResultData.Amount = ret.Amount;
                ResultData.CompleteFlagName = ret.CompleteFlagName;
                ResultData.AccountCode = ret.AccountCode;
                ResultData.AccountName = ret.AccountName;
                ResultData.Summary = ret.Summary;

                RetList.Add(ResultData);
            }

            return RetList.ToList<GetCreditJournal_ExcelResult>();
        }

        
        /// <summary>
        /// 下取車関連の実績を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>リスト型の検索結果</returns>
        /// <history>
        /// 2017/11/14 arc yano  #3811 車両伝票－下取車の入金予定残高更新不整合 入金予定に車台番号を保持する列を追加
        /// </history>
        public List<Journal> GetTradeJournal(string slipNumber, string accountType, string tradeVin)
        {
            var query = (
                         from a in db.Journal
                         where a.SlipNumber.Equals(slipNumber)
                            &&( string.IsNullOrWhiteSpace(accountType) || a.AccountType.Equals(accountType))
                            &&( string.IsNullOrWhiteSpace(tradeVin) ||  a.TradeVin.Equals(tradeVin))
                            && a.ReceiptPlanFlag != null && a.ReceiptPlanFlag.Equals("1")
                            && a.DelFlag.Equals("0")
                         orderby a.LastUpdateDate descending
                         select a);

            var result = CommonUtils.SelectWithUpdlock(db, query);

            return result.ToList();
        }


         /// <summary>
        /// 実績振替リストを取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <history>
        /// 2018/01/23 arc yano  #3836 入金実績振替機能移行　新規作成
        /// </history>
        public List<JournalTransfer> GetJournalTrasnferList(string slipNumber)
        {
            List<JournalTransfer> list =
                    (from a in db.Journal
                    join b in db.V_ALL_SalesOrderList on a.SlipNumber equals b.SlipNumber
                    where a.SlipNumber.Equals(slipNumber)
                    && a.DelFlag.Equals("0")
                    && a.JournalType.Equals("001")
                     select new JournalTransfer()
                    {
                         SlipNumber = b.SlipNumber
                        ,
                         JournalDate = a.JournalDate
                        ,
                         CustomerName = b.CustomerName
                        ,
                         SalesStatusName = b.SalesStatusName
                        ,
                         AccountTypeName = a.c_AccountType != null ? a.c_AccountType.Name : ""
                        ,
                         Amount = a.Amount
                        ,
                         JournalId = a.JournalId
                        ,
                         DepartmentCode = a.DepartmentCode
                    }).ToList()
                    ;

            return list;
        }
    }
}
