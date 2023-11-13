using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// イベント対象車両テーブルアクセスクラス
    ///   イベント対象車両テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CampaignCarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CampaignCarDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// イベント対象車両テーブルデータ取得(イベント指定)
        /// </summary>
        /// <param name="campaignCode">イベントコード</param>
        /// <returns>イベント対象車両テーブルデータ</returns>
        public List<CampaignCar> GetByCampaign(string campaignCode)
        {
            // イベント対象車両データの取得
            List<CampaignCar> ret =
                (from a in db.CampaignCar
                 where (a.CampaignCode.Equals(campaignCode))
                 orderby a.CarGradeCode
                 select a
                ).ToList<CampaignCar>();

            // イベント対象車両データの返却
            return ret;
        }
    }
}
