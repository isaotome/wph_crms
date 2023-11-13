using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// イベントテーブルアクセスクラス
    ///   イベントテーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CampaignDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CampaignDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// イベントテーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="campaignCode">イベントコード</param>
        /// <returns>イベントテーブルデータ(1件)</returns>
        public Campaign GetByKey(string campaignCode)
        {
            // イベントデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Campaign campaign =
                (from a in db.Campaign
                 where a.CampaignCode.Equals(campaignCode)
                 && a.DelFlag.Equals("0")
                 select a
                ).FirstOrDefault();

            // イベントデータの返却
            return campaign;
        }

        /// <summary>
        /// イベントテーブルデータ検索
        /// </summary>
        /// <param name="campaignCondition">イベント検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>イベントテーブルデータ検索結果</returns>
        public PaginatedList<Campaign> GetListByCondition(Campaign campaignCondition, int? pageIndex, int? pageSize)
        {
            string campaignCode = campaignCondition.CampaignCode;
            string campaignName = campaignCondition.CampaignName;
            string employeeName = null;
            try { employeeName = campaignCondition.Employee.EmployeeName; } catch (NullReferenceException) { }
            DateTime? campaignStartDateFrom = campaignCondition.CampaignStartDateFrom;
            DateTime? campaignStartDateTo = campaignCondition.CampaignStartDateTo;
            DateTime? campaignEndDateFrom = campaignCondition.CampaignEndDateFrom;
            DateTime? campaignEndDateTo = campaignCondition.CampaignEndDateTo;
            string delFlag = campaignCondition.DelFlag;

            // イベントデータの取得
            IOrderedQueryable<Campaign> campaignList =
                    from a in db.Campaign
                    where (string.IsNullOrEmpty(campaignCode) || a.CampaignCode.Contains(campaignCode))
                    && (string.IsNullOrEmpty(campaignName) || a.CampaignName.Contains(campaignName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (campaignStartDateFrom == null || DateTime.Compare(a.CampaignStartDate ?? DaoConst.SQL_DATETIME_MIN, campaignStartDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (campaignStartDateTo == null || DateTime.Compare(a.CampaignStartDate ?? DaoConst.SQL_DATETIME_MAX, campaignStartDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (campaignEndDateFrom == null || DateTime.Compare(a.CampaignEndDate ?? DaoConst.SQL_DATETIME_MIN, campaignEndDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (campaignEndDateTo == null || DateTime.Compare(a.CampaignEndDate ?? DaoConst.SQL_DATETIME_MAX, campaignEndDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CampaignCode
                    select a;

            // ページング制御情報を付与したイベントデータの返却
            PaginatedList<Campaign> ret = new PaginatedList<Campaign>(campaignList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }
    }
}
