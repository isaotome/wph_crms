using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
//Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao
{
    /// <summary>
    /// 車両査定ビューアクセスクラス
    ///   車両査定ビューの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class V_CarAppraisalDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_CarAppraisalDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両査定ビューデータ検索
        /// </summary>
        /// <param name="carAppraisalCondition">車両査定検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両査定ビューデータ検索結果</returns>
        public PaginatedList<V_CarAppraisal> GetListByCondition(V_CarAppraisal v_CarAppraisalCondition, int? pageIndex, int? pageSize) {

            string vin = v_CarAppraisalCondition.Vin;
            string slipNumber = v_CarAppraisalCondition.SlipNumber;
            string purchaseStatus = v_CarAppraisalCondition.PurchaseStatus;
            DateTime? createDateFrom = CommonUtils.GetDayStart(v_CarAppraisalCondition.CreateDateFrom);
            DateTime? createDateTo = CommonUtils.GetDayEnd(v_CarAppraisalCondition.CreateDateTo);

        
            //Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                // 車両査定データの取得
                IOrderedQueryable<V_CarAppraisal> carAppraisalList =
                        from a in db.V_CarAppraisal
                        where (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                        && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Contains(slipNumber))
                        && (string.IsNullOrEmpty(purchaseStatus) || a.PurchaseStatus.Equals(purchaseStatus))
                        && (createDateFrom == null || DateTime.Compare(a.CreateDate ?? DaoConst.SQL_DATETIME_MIN, createDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                        && (createDateTo == null || DateTime.Compare(a.CreateDate ?? DaoConst.SQL_DATETIME_MAX, createDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                        && a.DelFlag.Equals("0")
                        orderby a.SlipNumberCtrl, a.SlipNumber, a.CreateDate, a.CarAppraisalId
                        select a;

                // ページング制御情報を付与した車両査定データの返却
                PaginatedList<V_CarAppraisal> ret = new PaginatedList<V_CarAppraisal>(carAppraisalList, pageIndex ?? 0, pageSize ?? 0);

                // 出口
                return ret;
            }
        }
    }
}
