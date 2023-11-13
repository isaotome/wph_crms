using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class BankDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public BankDao(CrmsLinqDataContext context) {
            db = context;
        }
        /// <summary>
        /// 銀行マスタを検索する
        /// </summary>
        /// <param name="bankCondition">検索条件</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<Bank> GetByCondition(Bank bankCondition,int pageIndex,int pageSize) {
            var query = from a in db.Bank
                        where (string.IsNullOrEmpty(bankCondition.BankCode) || a.BankCode.Contains(bankCondition.BankCode))
                        && (string.IsNullOrEmpty(bankCondition.BankName) || a.BankName.Contains(bankCondition.BankName))
                        && (string.IsNullOrEmpty(bankCondition.DelFlag) || bankCondition.DelFlag.Equals("9") || a.DelFlag.Equals(bankCondition.DelFlag))
                        select a;
            PaginatedList<Bank> list = new PaginatedList<Bank>(query, pageIndex, pageSize);
            for (int i = 0; i < list.Count(); i++) {
                list[i] = EditModel(list[i]);
            }
            return list;
        }

        /// <summary>
        /// 銀行マスタを取得する（PK指定）
        /// </summary>
        /// <param name="bankCode"></param>
        /// <returns></returns>
        //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public Bank GetByKey(string bankCode, bool includeDeleted = false)
        {
            var bank = (from a in db.Bank
                        where a.BankCode.Equals(bankCode)
                        && ((includeDeleted) || a.DelFlag.Equals("0"))
                        select a).FirstOrDefault();
            return bank;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="brand">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Bank EditModel(Bank bank) {
            // 内部コード項目の名称情報取得
            bank.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, bank.DelFlag);

            // 出口
            return bank;
        }
    }
}
