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
//　機能　：外注支払一覧データアクセスクラス
//　作成日：2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
//
//
//-----------------------------------------------------------------------------
namespace CrmsDao
{
    public class SupplierPaymentListDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext"></param>
        public SupplierPaymentListDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// データ検索
        /// (ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>データ検索結果</returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
        /// </history>
        public PaginatedList<GetSupplierPaymentListResult> GetListByCondition(GetSupplierPaymentListCondition condition, int? PageIndex, int? PageSize)
        {
            return new PaginatedList<GetSupplierPaymentListResult>(GetList(condition).AsQueryable(), PageIndex ?? 0, PageSize ?? 0);
        }

        /// <summary>
        /// データ検索
        /// </summary>
        /// <param name="targetDate">検索条件</param>
        /// <param name="action">アクション</param>
        /// <returns>整備履歴</returns>
        /// <history>
        /// 2017/03/23 arc yano #3729 サブシステム機能移行(外注支払一覧)
        /// </history>
        public List<GetSupplierPaymentListResult> GetList(GetSupplierPaymentListCondition condition)
        {
            //ストアドプロシージャ実行
            ISingleResult<GetSupplierPaymentListResult> result = db.GetSupplierPaymentList(condition.Target, condition.TargetDateFrom, condition.TargetDateTo, condition.DepartmentCode, condition.ServiceWorkCode, condition.SlipNumber, condition.Vin, condition.CustomerCode, condition.CustomerName, condition.SupplierCode, condition.SupplierName);

            return result.ToList();
        }

    }
}
