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
//　機能　：翼）整備履歴
//　作成日：2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
//
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class ServiceSalesHistoryDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public ServiceSalesHistoryDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 整備履歴データ検索
        /// (ページング対応）
        /// </summary>
        /// <param name="condition">整備履歴検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>整備履歴データ検索結果</returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public PaginatedList<GetServiceSalesHistoryHeaderResult> GetListByCondition(ServiceSalesHistoryCondition condition, int? PageIndex, int? PageSize)
        {
            return new PaginatedList<GetServiceSalesHistoryHeaderResult>(GetQueryable(condition), PageIndex ?? 0, PageSize ?? 0);
        }

        /// <summary>
        /// 整備履歴データ取得
        /// </summary>
        /// <param name="targetDate">対象年月</param>
        /// <param name="searched">検索対象</param>
        /// <param name="action">アクション</param>
        /// <returns>整備履歴</returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public IQueryable<GetServiceSalesHistoryHeaderResult> GetQueryable(ServiceSalesHistoryCondition condition)
        {
            List<GetServiceSalesHistoryHeaderResult> list = new List<GetServiceSalesHistoryHeaderResult>();

            //ストアドプロシージャ実行
            ISingleResult<GetServiceSalesHistoryHeaderResult> result = db.GetServiceSalesHistoryHeader(condition.DivType, condition.DepartmentName, condition.SlipNumber, condition.Vin, condition.RegistNumber, condition.CustomerName, condition.CustomerNameKana);

            return result.ToList().AsQueryable();
        }


        /// <summary>
        /// 整備履歴(明細)データ検索
        /// (ページング対応）
        /// </summary>
        /// <param name="condition">整備履歴検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>整備履歴データ検索結果</returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public PaginatedList<GetServiceSalesHistoryLineResult> GetLineListByCondition(ServiceSalesHistoryCondition condition, int? PageIndex, int? PageSize)
        {
            return new PaginatedList<GetServiceSalesHistoryLineResult>(GetLineQueryable(condition), PageIndex ?? 0, PageSize ?? 0);
        }

        /// <summary>
        /// 整備履歴(明細)データ取得
        /// </summary>
        /// <param name="targetDate">対象年月</param>
        /// <param name="searched">検索対象</param>
        /// <param name="action">アクション</param>
        /// <returns>整備履歴</returns>
        /// <history>
        /// 2017/03/13 arc yano #3725 サブシステム移行(整備履歴)
        /// </history>
        public IQueryable<GetServiceSalesHistoryLineResult> GetLineQueryable(ServiceSalesHistoryCondition condition)
        {
            List<GetServiceSalesHistoryLineResult> list = new List<GetServiceSalesHistoryLineResult>();
            //ストアドプロシージャ実行
            ISingleResult<GetServiceSalesHistoryLineResult> result = db.GetServiceSalesHistoryLine(condition.DivType, condition.SlipNumber);

            return result.ToList().AsQueryable();
        }

    }
}
