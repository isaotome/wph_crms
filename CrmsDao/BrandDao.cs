using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// ブランドマスタアクセスクラス
    ///   ブランドマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class BrandDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public BrandDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// ブランドマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carBrandCode">ブランドコード</param>
        /// <returns>ブランドマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public Brand GetByKey(string carBrandCode, bool includeDeleted = false)
        {
            // ブランドデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加

            Brand brand =
                (from a in db.Brand
                 where a.CarBrandCode.Equals(carBrandCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (brand != null)
            {
                brand = EditModel(brand);
            }

            // ブランドデータの返却
            return brand;
        }

        /// <summary>
        /// ブランドマスタデータ検索
        /// </summary>
        /// <param name="brandCondition">ブランド検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>ブランドマスタデータ検索結果</returns>
        public PaginatedList<Brand> GetListByCondition(Brand brandCondition, int? pageIndex, int? pageSize)
        {
            string carBrandCode = brandCondition.CarBrandCode;
            string carBrandName = brandCondition.CarBrandName;
            string makerCode = null;
            try { makerCode = brandCondition.Maker.MakerCode; } catch (NullReferenceException) { }
            string makerName = null;
            try { makerName = brandCondition.Maker.MakerName; } catch (NullReferenceException) { }
            string delFlag = brandCondition.DelFlag;

            // ブランドデータの取得
            IOrderedQueryable<Brand> brandList =
                    from a in db.Brand
                    where (string.IsNullOrEmpty(carBrandCode) || a.CarBrandCode.Contains(carBrandCode))
                    && (string.IsNullOrEmpty(carBrandName) || a.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
                    && (string.IsNullOrEmpty(makerName) || a.Maker.MakerName.Contains(makerName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.MakerCode, a.CarBrandCode
                    select a;

            // ページング制御情報を付与したブランドデータの返却
            PaginatedList<Brand> ret = new PaginatedList<Brand>(brandList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="brand">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Brand EditModel(Brand brand)
        {
            // 内部コード項目の名称情報取得
            brand.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, brand.DelFlag);

            // 出口
            return brand;
        }

        /// <summary>
        /// ブランド全件取得する
        /// </summary>
        /// <returns></returns>
        public List<Brand> GetListAll() {
            var query = from a in db.Brand
                        where a.DelFlag.Equals("0")
                        orderby a.DisplayOrder
                        select a;
            return query.ToList();
        }
    }
}
