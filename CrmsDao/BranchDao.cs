using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class BranchDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public BranchDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 支店マスタを取得する（PK指定）
        /// </summary>
        /// <param name="branchCode">支店コード</param>
        /// <param name="bankCode">銀行コード</param>
        /// <returns></returns>
        //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
        public Branch GetByKey(string branchCode, string bankCode) {
            var query = (from a in db.Branch
                         where a.BranchCode.Equals(branchCode)
                         && a.BankCode.Equals(bankCode)
                         && a.DelFlag.Equals("0")
                         select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 支店マスタ検索
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<Branch> GetByCondition(Branch condition,int pageIndex,int pageSize) {
            string branchCode = condition.BranchCode;
            string branchName = condition.BranchName;
            string bankCode = "";
            try { bankCode = condition.Bank.BankCode; } catch (NullReferenceException) { }
            string bankName = "";
            try { bankName = condition.Bank.BankName; } catch (NullReferenceException) { }

            var query = from a in db.Branch
                        where (string.IsNullOrEmpty(branchCode) || a.BranchCode.Contains(branchCode))
                        && (string.IsNullOrEmpty(branchName) || a.BranchName.Contains(branchName))
                        && (string.IsNullOrEmpty(bankCode) || a.Bank.BankCode.Contains(bankCode))
                        && (string.IsNullOrEmpty(bankName) || a.Bank.BankName.Contains(bankName))
                        && a.DelFlag.Equals("0")
                        orderby a.BankCode, a.BranchCode
                        select a;
            return new PaginatedList<Branch>(query, pageIndex, pageSize);
        }
    }
}
