using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarTaxDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarTaxDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 自動車税種別割リストを取得する(全件)
        /// </summary>
        /// <returns>自動車税種別割リスト</returns>
        /// <history>
        /// 2019/10/29 yano #4024 【車両伝票入力】オプション行追加・削除時にエラー発生した時の不具合対応
        /// </history>
        public List<CarTax> GetListAll() {
            IOrderedQueryable<CarTax> carTax =
                from a in db.CarTax
                //orderby a.Amount
                orderby a.CarTaxName, a.RegistMonth
                select a;
            return carTax.ToList<CarTax>();
        }

        /// <summary>
        /// 自動車税種別割データを取得する(PK指定)
        /// </summary>
        /// <param name="carTaxId">自動車税種別割ID</param>
        /// <returns>自動車税種別割データ</returns>
        public CarTax GetByKey(Guid carTaxId) {
            CarTax carTax =
                (from a in db.CarTax
                 where a.CarTaxId.Equals(carTaxId)
                 select a).FirstOrDefault();
            return carTax;
        }

        /// <summary>
        /// 総排気量、登録月、初年度登録から自動車税種別割を取得する
        /// </summary>
        /// <param name="displacement">総排気量(cc)</param>
        /// <param name="registMonth">登録月</param>
        /// <param name="firstregistrationyear">初年度登録</param>
        /// <returns>自動車税種別割データ</returns>
        /// <history>
        /// 2019/10/17 yano #4023 【車両伝票入力】中古車の自動車種別割の計算の誤り
        /// </history>
        public CarTax GetByDisplacement(decimal displacement, int registMonth, DateTime firstregistrationyear)
        {
            CarTax carTax =
                (from a in db.CarTax
                 where displacement > a.FromDisplacement 
                 && displacement <= a.ToDisplacement
                 && a.RegistMonth==registMonth
                 && a.FromAvailableDate <= firstregistrationyear       //Add 2019/10/17 yano #4023
                 && a.ToAvailableDate >= firstregistrationyear       //Add 2019/10/17 yano #4023
                 && !a.DelFlag.Equals("1")
                 select a).FirstOrDefault();

            return carTax;
        }
    }
}
