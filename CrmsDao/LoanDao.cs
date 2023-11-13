using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// ローンマスタアクセスクラス
    ///   ローンマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class LoanDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public LoanDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// ローンマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="loanCode">ローンコード</param>
        /// <returns>ローンマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public Loan GetByKey(string loanCode, bool includeDeleted = false)
        {
            // ローンデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Loan loan =
                (from a in db.Loan
                 where a.LoanCode.Equals(loanCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (loan != null)
            {
                loan = EditModel(loan);
            }

            // ローンデータの返却
            return loan;
        }

        /// <summary>
        /// ローンマスタデータ検索
        /// </summary>
        /// <param name="loanCondition">ローン検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>ローンマスタデータ検索結果</returns>
        public PaginatedList<Loan> GetListByCondition(Loan loanCondition, int? pageIndex, int? pageSize)
        {
            string loanCode = loanCondition.LoanCode;
            string loanName = loanCondition.LoanName;
            string customerClaimCode = null;
            try { customerClaimCode = loanCondition.CustomerClaim.CustomerClaimCode; } catch (NullReferenceException) { }
            string customerClaimName = null;
            try { customerClaimName = loanCondition.CustomerClaim.CustomerClaimName; } catch (NullReferenceException) { }
            string delFlag = loanCondition.DelFlag;

            // ローンデータの取得
            IOrderedQueryable<Loan> loanList =
                    from a in db.Loan
                    where (string.IsNullOrEmpty(loanCode) || a.LoanCode.Contains(loanCode))
                    && (string.IsNullOrEmpty(loanName) || a.LoanName.Contains(loanName))
                    && (string.IsNullOrEmpty(customerClaimCode) || a.CustomerClaimCode.Contains(customerClaimCode))
                    && (string.IsNullOrEmpty(customerClaimName) || a.CustomerClaim.CustomerClaimName.Contains(customerClaimName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CustomerClaimCode, a.LoanCode
                    select a;

            // ページング制御情報を付与したローンデータの返却
            PaginatedList<Loan> ret = new PaginatedList<Loan>(loanList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="loan">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Loan EditModel(Loan loan)
        {
            // 内部コード項目の名称情報取得
            loan.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, loan.DelFlag);

            // 出口
            return loan;
        }
    }
}
