using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// サービス工数表マスタアクセスクラス
    ///   サービス工数表マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class ServiceCostDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public ServiceCostDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// サービス工数表マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="serviceMenuCode">サービスメニューコード</param>
        /// <param name="carClassCode">車両クラスコード</param>
        /// <returns>サービス工数表マスタデータ(1件)</returns>
        public ServiceCost GetByKey(string serviceMenuCode, string carClassCode)
        {
            // 部品データの取得
            return
                (from a in db.ServiceCost
                 where a.ServiceMenuCode.Equals(serviceMenuCode)
                 && a.CarClassCode.Equals(carClassCode)
                 select a
                ).FirstOrDefault();
        }

        /// <summary>
        /// サービス工数表マスタデータ取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>サービス工数表マスタデータ</returns>
        public Dictionary<string, string> GetAll(bool includeDeleted)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            // サービス工数表データの取得
            List<ServiceCost> serviceCostList =
                    (from a in db.ServiceCost
                    where (includeDeleted) || a.DelFlag.Equals("0")
                    orderby a.ServiceMenuCode, a.CarClassCode
                    select a).ToList<ServiceCost>();

            foreach (ServiceCost serviceCost in serviceCostList)
            {
                ret.Add(serviceCost.ServiceMenuCode + "_" + serviceCost.CarClassCode, string.Format("{0:f2}", serviceCost.Cost));
            }

            // 出口
            return ret;
        }
    }
}
