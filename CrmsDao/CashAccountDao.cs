using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CashAccountDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CashAccountDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 事業所で利用可能な現金口座コードを取得する
        /// </summary>
        /// <param name="officeCode">事業所コード</param>
        /// <returns></returns>
        /// <history>
        /// 2018/05/28 arc yano #3886 Excel取込(ワランティ消込) 事業所コードを任意の項目に変更
        /// </history>
        public List<CashAccount> GetListByOfficeCode(string officeCode){
            var query =
                from a in db.CashAccount
                where (string.IsNullOrWhiteSpace(officeCode) || a.OfficeCode.Equals(officeCode))
                && a.DelFlag.Equals("0")
                select a;
            return query.ToList<CashAccount>();
        }

        /// <summary>
        /// 現金口座データを取得する（PK指定）
        /// </summary>
        /// <param name="officeCode">事業所コード</param>
        /// <param name="cashAccountCode">現金口座コード</param>
        /// <returns></returns>
        public CashAccount GetByKey(string officeCode, string cashAccountCode) {
            var query =
                (from a in db.CashAccount
                 where a.OfficeCode.Equals(officeCode)
                 && a.CashAccountCode.EndsWith(cashAccountCode)
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return query;
        }
    }
}
