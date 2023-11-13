using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 車両査定テーブルアクセスクラス
    ///   車両査定テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarAppraisalDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarAppraisalDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両査定テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="carAppraisalId">車両査定ID</param>
        /// <returns>車両査定テーブルデータ(1件)</returns>
        public CarAppraisal GetByKey(Guid carAppraisalId)
        {
            // 車両査定データの取得
            CarAppraisal carAppraisal =
                (from a in db.CarAppraisal
                 where a.CarAppraisalId.Equals(carAppraisalId)
                 select a
                ).FirstOrDefault();

            // 車両査定データの返却
            return carAppraisal;
        }

        /// <summary>
        /// 車両査定テーブルデータ取得(伝票番号・車台番号指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="vin">車台番号</param>
        /// <returns></returns>
        public CarAppraisal GetBySlipNumberVin(string slipNumber, string vin, bool includeDeleted = false) {
            CarAppraisal query =
                (from a in db.CarAppraisal
                 where a.SlipNumber.Equals(slipNumber)
                 && a.Vin.Equals(vin)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 車輌査定テーブルリスト取得
        /// </summary>
        /// <param name="slipNumber"></param>
        /// <param name="vin"></param>
        /// <returns></returns>
        public List<CarAppraisal> GetListBySlipNumberVin(string slipNumber, string vin, string purchaseCreated) {
            var query =
                from a in db.CarAppraisal
                where a.SlipNumber.Equals(slipNumber)
                && a.Vin.Equals(vin)
                && a.DelFlag.Equals("0")
                && (string.IsNullOrEmpty(purchaseCreated) || a.PurchaseCreated.Equals(purchaseCreated))
                select a;
            return query.ToList();
        }
    }
}
