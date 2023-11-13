using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// クーポンマスタアクセスクラス
    ///   クーポンマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CouponDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CouponDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// クーポンマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="couponCode">クーポンコード</param>
        /// <returns>クーポンマスタデータ(1件)</returns>
        public Coupon GetByKey(string couponCode)
        {
            // クーポンデータの取得
            Coupon coupon =
                (from a in db.Coupon
                 where a.CouponCode.Equals(couponCode)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (coupon != null)
            {
                coupon = EditModel(coupon);
            }

            // クーポンデータの返却
            return coupon;
        }

        /// <summary>
        /// クーポンマスタデータ検索
        /// </summary>
        /// <param name="couponCondition">クーポン検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>クーポンマスタデータ検索結果</returns>
        public PaginatedList<Coupon> GetListByCondition(Coupon couponCondition, int? pageIndex, int? pageSize)
        {
            string couponCode = couponCondition.CouponCode;
            string couponName = couponCondition.CouponName;
            string carBrandCode = null;
            try { carBrandCode = couponCondition.Brand.CarBrandCode; } catch (NullReferenceException) { }
            string carBrandName = null;
            try { carBrandName = couponCondition.Brand.CarBrandName; } catch (NullReferenceException) { }
            string delFlag = couponCondition.DelFlag;

            // クーポンデータの取得
            IOrderedQueryable<Coupon> couponList =
                    from a in db.Coupon
                    where (string.IsNullOrEmpty(couponCode) || a.CouponCode.Contains(couponCode))
                    && (string.IsNullOrEmpty(couponName) || a.CouponName.Contains(couponName))
                    && (string.IsNullOrEmpty(carBrandCode) || a.CarBrandCode.Contains(carBrandCode))
                    && (string.IsNullOrEmpty(carBrandName) || a.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.CarBrandCode, a.CouponCode
                    select a;

            // ページング制御情報を付与したクーポンデータの返却
            PaginatedList<Coupon> ret = new PaginatedList<Coupon>(couponList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="coupon">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Coupon EditModel(Coupon coupon)
        {
            // 内部コード項目の名称情報取得
            coupon.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, coupon.DelFlag);

            // 出口
            return coupon;
        }
    }
}
