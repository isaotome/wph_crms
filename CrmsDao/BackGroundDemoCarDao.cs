using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class BackGroundDemoCarDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public BackGroundDemoCarDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 管理番号より、変更履歴テーブルを取得する
        /// </summary>
        /// <param name="salesCarNumber"></param>
        /// <returns></returns>
        public BackGroundDemoCar GetBySalesCarNumber(string salesCarNumber)
        {
            var backGroundDemoCar = (from a in db.BackGroundDemoCar
                        where a.SalesCarNumber.Equals(salesCarNumber)
                        && a.DelFlag.Equals("0")
                        select a).FirstOrDefault();
            return backGroundDemoCar;
        }

        //Add 2015/02/16 arc yano 車両用途変更対応 変更履歴画面追加
        /// <summary>
        /// 変更履歴テーブル取得
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public IQueryable<BackGroundDemoCar> GetAllList()
        {
            var query = (from a in db.BackGroundDemoCar
                                     where a.DelFlag.Equals("0")
                                     select a).OrderByDescending(x => x.LastUpdateDate);
            return query;
        }
    }
}
