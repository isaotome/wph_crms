using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlClient;


//------------------------------------------------------------
// 機能 データアクセスクラス
// 作成：2014/07/28 arc yano IPO対応 
//
//------------------------------------------------------------
namespace CrmsDao
{

    //月締処理状況データ取得クラス
    public class CloseMonthControlDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CloseMonthControlDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 月締処理状況データ取得(PK指定)
        /// </summary>
        /// <param name="closeMonth">締め月</param>
        /// <returns>月締処理状況データ(1件)</returns>
        public CloseMonthControl GetByKey(string closeMonth)
        {
            // 月締処理状況データの取得
            CloseMonthControl closeMonthControl =
               (from a in db.CloseMonthControl
                where a.CloseMonth.Equals(closeMonth)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();


            // 月締処理状況データの返却
            return closeMonthControl;
        }

        /// <summary>
        /// 月締処理状況データ取得(PK指定)
        /// </summary>
        /// <param name="closeMonth">締め月</param>
        /// <returns>月締処理状況データ(1件)リスト型</returns>
        public List<CloseMonthControl> GetListByKey(string closeMonth)
        {
            // 月締処理状況データの取得
            var query =
                from a in db.CloseMonthControl
                where a.CloseMonth.Equals(closeMonth)
                && a.DelFlag.Equals("0")
                select a;

            // 月締処理状況データの返却
            return query.ToList<CloseMonthControl>();
        }


        /// <summary>
        /// 月締処理状況データ取得(最新締め月1件)
        /// </summary>
        /// <returns>月締処理状況データ(1件)</returns>
        public CloseMonthControl GetLatestDate()
        {
            // 月締処理状況データの取得
            CloseMonthControl closeMonthControl =
                (from a in db.CloseMonthControl
                 where a.CloseMonth == 
                 ((from b in db.CloseMonthControl
                 where b.DelFlag.Equals("0")
                 select b).Max(b => b.CloseMonth))
                 && (a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();

            // 月締処理状況データの返却
            return closeMonthControl;
        }

        /// <summary>
        /// 指定日が本締めかどうか（本締めでない：True 本締め：false）
        /// </summary>
        /// <returns>本締めでない：True 本締め：false</returns>
        /// <history>Add 2016/05/18 arc nakayama #3536_現金出納帳　店舗締め解除処理ボタンの追加</history>
        public bool IsCloseEndInventoryMonth(string targetDate)
        {
            DateTime CloseDate;
            bool Ret = DateTime.TryParse(targetDate, out CloseDate);
            //日付と関係ない文字列が送られてきた場合はFalseを返す
            if (!Ret)
            {
                return false;
            }
            CloseDate = DateTime.Parse(targetDate);
            DateTime CloseMonth = new DateTime(CloseDate.Year, CloseDate.Month, 1);
            string stCloseMonth = string.Format("{0:yyyyMMdd}", CloseMonth);

            var query =
                from a in db.CloseMonthControl
                where a.DelFlag.Equals("0")
                && a.CloseStatus.Equals("003")
                && a.CloseMonth.Equals(stCloseMonth)
                select a;

            return query.Count() == 0;
        }
    }
}
