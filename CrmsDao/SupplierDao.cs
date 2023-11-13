using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 仕入先マスタアクセスクラス
    ///   仕入先マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class SupplierDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public SupplierDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 仕入先マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="supplierCode">仕入先コード</param>
        /// <returns>仕入先マスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
        public Supplier GetByKey(string supplierCode, bool includeDeleted = false)
        {
            // 仕入先データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Supplier supplier =
                (from a in db.Supplier
                 where a.SupplierCode.Equals(supplierCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (supplier != null)
            {
                supplier = EditModel(supplier);
            }

            // 仕入先データの返却
            return supplier;
        }

        /// <summary>
        /// 仕入先マスタデータ検索
        /// </summary>
        /// <param name="supplierCondition">仕入先検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>仕入先マスタデータ検索結果</returns>
        public PaginatedList<Supplier> GetListByCondition(Supplier supplierCondition, int? pageIndex, int? pageSize)
        {
            string supplierCode = supplierCondition.SupplierCode;
            string supplierName = supplierCondition.SupplierName;
            string outsourceFlag = supplierCondition.OutsourceFlag;
            string delFlag = supplierCondition.DelFlag;

            // 仕入先データの取得
            IOrderedQueryable<Supplier> supplierList =
                    from a in db.Supplier
                    where (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
                    && (string.IsNullOrEmpty(supplierName) || a.SupplierName.Contains(supplierName))
                    && (string.IsNullOrEmpty(outsourceFlag) || a.OutsourceFlag.Equals(outsourceFlag))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.SupplierCode
                    select a;

            // ページング制御情報を付与した仕入先データの返却
            PaginatedList<Supplier> ret = new PaginatedList<Supplier>(supplierList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="supplier">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Supplier EditModel(Supplier supplier)
        {
            // 内部コード項目の名称情報取得
            supplier.OutsourceFlagName = CodeUtils.GetName(CodeUtils.OutsourceFlag, supplier.OutsourceFlag);
            supplier.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, supplier.DelFlag);

            // 出口
            return supplier;
        }
    }
}
