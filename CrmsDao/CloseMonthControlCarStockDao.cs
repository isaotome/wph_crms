using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
using System.Linq.Expressions;

//Add 2015/04/08 arc yano　車両管理対応④ 車両管理の管理テーブルを追加
namespace CrmsDao
{
    public class CloseMonthControlCarStockDao
    {
        
         /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CloseMonthControlCarStockDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }
        
        /// <summary>
        /// 車両管理用締処理状況取得(PK指定)
        /// </summary>
        /// <param name="CloseMonth">棚卸月</param>
        /// <returns>車両管理用締処理状況テーブルデータ(1件)</returns>
        public CloseMonthControlCarStock GetByKey(string closeMonth)
        {
            // 棚卸スケジュールデータの取得
            var query =
                (
                    from a in db.CloseMonthControlCarStock
                    where (a.CloseMonth.Equals(closeMonth))
                    && (a.DelFlag.Equals("0"))
                    select a
                ).FirstOrDefault();

            // 棚卸スケジュールデータの返却
            return query;
        }

        /// <summary>
        /// 指定日が本締めかどうか（本締めでない：True 本締め：false）
        /// </summary>
        /// <param name="targetDate">対象年月(yyyyMMdd形式)</param>
        /// <returns>本締めでない：True 本締め：false</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善② 新規作成
        /// </history>
        public bool IsClosedMonth(string targetDate)
        {
            string strCloseMonth = targetDate.Replace("/", "");

            var query =
                from a in db.CloseMonthControlCarStock
                where a.DelFlag.Equals("0")
                && a.CloseStatus.Equals("002")
                && a.CloseMonth.Equals(strCloseMonth)
                select a;

            return query.Count() == 0;
        }
    }
}
