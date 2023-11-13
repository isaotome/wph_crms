using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 支払先マスタアクセスクラス
    ///   支払先マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class SupplierPaymentDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public SupplierPaymentDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 支払先マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="supplierPaymentCode">支払先コード</param>
        /// <returns>支払先マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public SupplierPayment GetByKey(string supplierPaymentCode, bool includeDeleted = false)
        {
            // 支払先データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            SupplierPayment supplierPayment =
                (from a in db.SupplierPayment
                 where a.SupplierPaymentCode.Equals(supplierPaymentCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (supplierPayment != null)
            {
                supplierPayment = EditModel(supplierPayment);
            }

            // 支払先データの返却
            return supplierPayment;
        }

        /// <summary>
        /// 支払先マスタデータ検索
        /// </summary>
        /// <param name="supplierPaymentCondition">支払先検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>支払先マスタデータ検索結果</returns>
        public PaginatedList<SupplierPayment> GetListByCondition(SupplierPayment supplierPaymentCondition, int? pageIndex, int? pageSize)
        {
            string supplierPaymentCode = supplierPaymentCondition.SupplierPaymentCode;
            string supplierPaymentName = supplierPaymentCondition.SupplierPaymentName;
            string delFlag = supplierPaymentCondition.DelFlag;

            // 支払先データの取得
            IOrderedQueryable<SupplierPayment> supplierPaymentList =
                    from a in db.SupplierPayment
                    where (string.IsNullOrEmpty(supplierPaymentCode) || a.SupplierPaymentCode.Contains(supplierPaymentCode))
                    && (string.IsNullOrEmpty(supplierPaymentName) || a.SupplierPaymentName.Contains(supplierPaymentName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.SupplierPaymentCode
                    select a;

            // ページング制御情報を付与した支払先データの返却
            PaginatedList<SupplierPayment> ret = new PaginatedList<SupplierPayment>(supplierPaymentList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="supplierPayment">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private SupplierPayment EditModel(SupplierPayment supplierPayment)
        {
            // 内部コード項目の名称情報取得
            supplierPayment.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, supplierPayment.DelFlag);

            // 出口
            return supplierPayment;
        }
    }
}
