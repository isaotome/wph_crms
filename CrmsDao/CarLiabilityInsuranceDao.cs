using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarLiabilityInsuranceDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarLiabilityInsuranceDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 自賠責保険料リストを取得する(全件)
        /// </summary>
        /// <returns>自賠責保険料リスト</returns>
        /// <history>
        /// 2017/06/19 arc nakayama #3748_自賠責保険料マスタ　削除フラグが効かない
        /// 2017/02/23 arc yano #3715 自賠責保険料一覧　ソート順の変更    
        /// </history>
        public List<CarLiabilityInsurance> GetListAll(bool includeDeleted = false)
        {
            IOrderedQueryable<CarLiabilityInsurance> query =
                from a in db.CarLiabilityInsurance
                where ((includeDeleted) || a.DelFlag.Equals("0"))
                orderby a.CarLiabilityInsuranceId descending  //Mod 2017/02/23 arc yano #3715
                //orderby a.Amount descending
                select a;
            return query.ToList<CarLiabilityInsurance>();
        }

        /// <summary>
        /// 自賠責保険料データを取得する(PK指定)
        /// </summary>
        /// <param name="carTaxId">自賠責保険料ID</param>
        /// <returns>自賠責保険料データ</returns>
        public CarLiabilityInsurance GetByKey(Guid carLiabilityInsuranceId) {
            CarLiabilityInsurance query =
                (from a in db.CarLiabilityInsurance
                 where a.CarLiabilityInsuranceId.Equals(carLiabilityInsuranceId)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 自賠責保険料新車のデフォルトデータを取得する
        /// </summary>
        /// <returns></returns>
        public CarLiabilityInsurance GetByNewDefault(bool includeDeleted = false) {
            CarLiabilityInsurance query =
                (from a in db.CarLiabilityInsurance
                 where a.NewDefaultFlag.Equals("1")
                    && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 自賠責保険料中古車のデフォルトデータを取得する
        /// </summary>
        /// <returns></returns>
        public CarLiabilityInsurance GetByUsedDefault(bool includeDeleted = false) {
            CarLiabilityInsurance query =
                (from a in db.CarLiabilityInsurance
                 where a.UsedDefaultFlag.Equals("1")
                    && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();
            return query;
        }
    }
}
