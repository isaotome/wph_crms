using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 車種マスタアクセスクラス
    ///   車種マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車種マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carCode">車種コード</param>
        /// <returns>車種マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public Car GetByKey(string carCode, bool includeDeleted = false)
        {
            // 車種データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Car car =
                (from a in db.Car
                 where a.CarCode.Equals(carCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (car != null)
            {
                car = EditModel(car);
            }

            // 車種データの返却
            return car;
        }

        /// <summary>
        /// 車種マスタデータ検索
        /// </summary>
        /// <param name="carCondition">車種検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車種マスタデータ検索結果</returns>
        public PaginatedList<Car> GetListByCondition(Car carCondition, int? pageIndex, int? pageSize)
        {
            string carCode = carCondition.CarCode;
            string carName = carCondition.CarName;
            string carBrandCode = null;
            try { carBrandCode = carCondition.Brand.CarBrandCode; } catch (NullReferenceException) { }
            string carBrandName = null;
            try { carBrandName = carCondition.Brand.CarBrandName; } catch (NullReferenceException) { }
            string delFlag = carCondition.DelFlag;

            // 車種データの取得
            IOrderedQueryable<Car> carList =
                    from a in db.Car
                    where (string.IsNullOrEmpty(carCode) || a.CarCode.Contains(carCode))
                    && (string.IsNullOrEmpty(carName) || a.CarName.Contains(carName))
                    && (string.IsNullOrEmpty(carBrandCode) || a.CarBrandCode.Contains(carBrandCode))
                    && (string.IsNullOrEmpty(carBrandName) || a.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CarBrandCode, a.CarCode
                    select a;

            // ページング制御情報を付与した車種データの返却
            PaginatedList<Car> ret = new PaginatedList<Car>(carList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// 車種データ全件取得
        /// </summary>
        /// <returns>車種データリスト</returns>
        public List<Car> GetListAll() {
            IOrderedQueryable<Car> query =
                from a in db.Car
                where a.DelFlag.Equals("0")
                orderby a.CarCode
                select a;
            return query.ToList<Car>();
        }
        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="car">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Car EditModel(Car car)
        {
            // 内部コード項目の名称情報取得
            car.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, car.DelFlag);

            // 出口
            return car;
        }
    }
}
