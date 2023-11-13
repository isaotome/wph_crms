using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Data.Linq;


//-----------------------------------------------------------------------------
//　機能　：メカニックランキングデータアクセスクラス
//　作成日：2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class MechanicRankingDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public MechanicRankingDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// メカニックランキングデータ取得
        /// </summary>
        /// <param name="targetDate">対象年月</param>
        /// <returns>メカニックランキング</returns>
        /// <history>
        /// 2017/03/18 arc yano #3727 サブシステム移行(メカニックランキング)
        /// </history>
        public List<GetMechanicRankingResult> GetList(DateTime targetDateFrom)
        {
            //ストアドプロシージャ実行
            ISingleResult<GetMechanicRankingResult> list = db.GetMechanicRanking(targetDateFrom);

            return list.ToList();
        }
    }
}
