using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 車両クラスマスタアクセスクラス
    ///   車両クラスマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarClassDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarClassDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両クラスマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carClassCode">車両クラスコード</param>
        /// <returns>車両クラスマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public CarClass GetByKey(string carClassCode, bool includeDeleted = false)
        {
            // 車両クラスデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            CarClass carClass =
                (from a in db.CarClass
                 where a.CarClassCode.Equals(carClassCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (carClass != null)
            {
                carClass = EditModel(carClass);
            }

            // 車両クラスデータの返却
            return carClass;
        }

        /// <summary>
        /// 車両クラスマスタデータ全件取得
        /// </summary>
        /// <param name="includeDeleted">削除データを含むか否か</param>
        /// <returns>車両クラスマスタデータ(全件)</returns>
        public List<CarClass> GetAll(bool includeDeleted)
        {
            // 車両クラスデータの取得
            List<CarClass> ret =
                (from a in db.CarClass
                 where (includeDeleted) || a.DelFlag.Equals("0")
                 select a
                ).ToList<CarClass>();

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 車両クラスデータの返却
            return ret;
        }

        /// <summary>
        /// 車両クラスマスタデータ検索
        /// </summary>
        /// <param name="carClassCondition">車両クラス検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両クラスマスタデータ検索結果</returns>
        public PaginatedList<CarClass> GetListByCondition(CarClass carClassCondition, int? pageIndex, int? pageSize)
        {
            string carClassCode = carClassCondition.CarClassCode;
            string carClassName = carClassCondition.CarClassName;
            string delFlag = carClassCondition.DelFlag;

            // 車両クラスデータの取得
            IOrderedQueryable<CarClass> carClassList =
                    from a in db.CarClass
                    where (string.IsNullOrEmpty(carClassCode) || a.CarClassCode.Contains(carClassCode))
                    && (string.IsNullOrEmpty(carClassName) || a.CarClassName.Contains(carClassName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CarClassCode
                    select a;

            // ページング制御情報を付与した車両クラスデータの返却
            PaginatedList<CarClass> ret = new PaginatedList<CarClass>(carClassList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="carClass">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private CarClass EditModel(CarClass carClass)
        {
            // 内部コード項目の名称情報取得
            carClass.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, carClass.DelFlag);

            // 出口
            return carClass;
        }
    }
}
