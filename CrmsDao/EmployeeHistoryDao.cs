using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CrmsDao
{
    /// <summary>
    /// 社員マスタ履歴データアクセスクラス
    /// </summary>
    /// <history>
    /// 2019/05/23 yano #3994【社員マスタ】社員マスタ登録・更新時の履歴機能の追加
    /// </history>
    public class EmployeeHistoryDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public EmployeeHistoryDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        ///最新履歴を取得する
        /// </summary>
        /// <param name="emplyeeCode"></param>
        public EmployeeHistory GetLatestHistory(string emplyeeCode)
        {
            var query = from a in db.EmployeeHistory
                        where a.EmployeeCode.Equals(emplyeeCode)
                        orderby a.RevisionNumber descending
                        select a;
            return query.FirstOrDefault();
        }
    }
}
