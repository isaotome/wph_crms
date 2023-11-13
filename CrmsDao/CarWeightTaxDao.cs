using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarWeightTaxDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarWeightTaxDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 自動車重量税リストを取得する(全件)
        /// </summary>
        /// <returns>自動車重量税リスト</returns>
        public List<CarWeightTax> GetListAll() {
            IOrderedQueryable<CarWeightTax> carWeightTax =
                from a in db.CarWeightTax
                orderby a.InspectionYear descending,a.WeightFrom,a.WeightTo
                select a;
            return carWeightTax.ToList<CarWeightTax>();
        }

        /// <summary>
        /// 自動車重量税データを取得する(PK指定)
        /// </summary>
        /// <param name="carTaxId">自動車重量税ID</param>
        /// <returns>自動車重量税データ</returns>
        public CarWeightTax GetByKey(Guid carWeightTaxId) {
            CarWeightTax carWeightTax =
                (from a in db.CarWeightTax
                 where a.CarWeightTaxId.Equals(carWeightTaxId)
                 select a).FirstOrDefault();
            return carWeightTax;
        }

        /// <summary>
        /// 車検年数と車両重量から自動車税種別割データを取得する
        /// </summary>
        /// <param name="carWeight"></param>
        /// <returns></returns>
        public CarWeightTax GetByWeight(int inspectionYear,int carWeight) {
            CarWeightTax query =
                (from a in db.CarWeightTax
                 where carWeight >= a.WeightFrom
                 && carWeight <= a.WeightTo
                 && inspectionYear.Equals(a.InspectionYear)
                 select a).FirstOrDefault();
            return query;
        }
    }
}
