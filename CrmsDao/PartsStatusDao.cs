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
//　機能　：パーツステータスデータアクセスクラス
//　作成日：2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
//
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class PartsStatusDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public PartsStatusDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /*
        /// <summary>
        /// データ検索
        /// (ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>データ検索結果</returns>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        public PaginatedList<GetPartsStatusResult> GetListByCondition(PartsStatusCondition condition, int? PageIndex, int? PageSize)
        {
            return new PaginatedList<GetPartsStatusResult>(GetQueryable(condition), PageIndex ?? 0, PageSize ?? 0);
        }
        */

        /// <summary>
        ///パーツステータスデータ取得
        /// </summary>
        /// <param name="targetDate">対象年月</param>
        /// <param name="searched">検索対象</param>
        /// <param name="action">アクション</param>
        /// <returns>パーツステータス</returns>
        /// <history>
        /// 2017/03/16 arc yano #3726 サブシステム移行（パーツステータス）
        /// </history>
        public List<GetPartsStatusResult> GetListByCondition(PartsStatusCondition condition)
        { 
            return db.GetPartsStatus(condition.Target, condition.TargetDateFrom, condition.DepartmentCode, condition.ServiceOrderStatus, condition.PartsNumber).ToList();
        }

    }
}
