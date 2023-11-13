using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarAcquisitionTaxDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarAcquisitionTaxDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 自動車税環境性能割リストを取得する(全件)
        /// </summary>
        /// <returns>自動車税環境性能割リスト</returns>
        public List<CarAcquisitionTax> GetListAll() {
            IOrderedQueryable<CarAcquisitionTax> carTax =
                from a in db.CarAcquisitionTax
                where a.DelFlag.Equals("0")
                orderby a.ElapsedYears
                select a;
            return carTax.ToList<CarAcquisitionTax>();
        }

        /// <summary>
        /// 自動車税環境性能割データを取得する(PK指定)
        /// </summary>
        /// <param name="carTaxId">自動車税環境性能割ID</param>
        /// <returns>自動車税環境性能割データ</returns>
        public CarAcquisitionTax GetByKey(Guid carAcquisitionTaxId) {
            CarAcquisitionTax carTax =
                (from a in db.CarAcquisitionTax
                 where a.CarAcquisitionTaxId.Equals(carAcquisitionTaxId)
                 select a).FirstOrDefault();
            return carTax;
        }

        /// <summary>
        /// 経過年数から残価率を取得
        /// </summary>
        /// <param name="years">経過年数</param>
        /// <returns>残価率</returns>
        public decimal GetRateByYears(decimal years) {
            CarAcquisitionTax carTax =
                (from a in db.CarAcquisitionTax
                 where a.ElapsedYears.Equals(years)
                 select a).FirstOrDefault();
            return carTax.RemainRate;
        }

        /// <summary>
        /// 経過年数から自動車税環境性能割データを取得する
        /// </summary>
        /// <param name="elapsedYears"></param>
        /// <returns></returns>
        public CarAcquisitionTax GetByValue(decimal elapsedYears) {
            CarAcquisitionTax carTax =
                (from a in db.CarAcquisitionTax
                 where a.ElapsedYears == elapsedYears
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return carTax;
        }
    }
}
