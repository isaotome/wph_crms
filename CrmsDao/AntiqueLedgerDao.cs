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
//　機能　：古物台帳データアクセスクラス
//　作成日：2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳)
//
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class AntiqueLedgerDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public AntiqueLedgerDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 古物台帳データ取得
        /// </summary>
        /// <param name="targetDate">対象年月</param>
        /// <param name="searched">検索対象</param>
        /// <param name="action">アクション</param>
        /// <returns>古物台帳</returns>
        /// <history>
        /// 2017/03/07 arc yano #3731 サブシステム機能移行(古物台帳) 新規作成
        /// </history>
        public List<GetAntiqueLedgerListResult> GetList(DateTime targetDate, string searched, bool antiqueledger)
        {
            List<GetAntiqueLedgerListResult> list = new List<GetAntiqueLedgerListResult>();
            
            //ストアドプロシージャ実行
            ISingleResult<GetAntiqueLedgerListResult> result = db.GetAntiqueLedgerList(targetDate, searched);

            foreach (var ret in result)
            {
                //古物台帳出力以外の場合は、職業と誕生日はNULLにする
                if (antiqueledger != true)
                {
                    ret.OccupationName = null;
                    ret.Birthday = null;
                }

                list.Add(ret);
            }

            return list;
        }

    }
}
