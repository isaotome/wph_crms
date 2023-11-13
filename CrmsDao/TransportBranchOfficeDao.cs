using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 運輸支局マスタアクセスクラス
    ///   運輸支局マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class TransportBranchOfficeDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public TransportBranchOfficeDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 運輸支局マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="transportBranchOfficeCode">運輸支局コード</param>
        /// <returns>運輸支局マスタデータ(1件)</returns>
        public TransportBranchOffice GetByKey(string transportBranchOfficeCode)
        {
            // 運輸支局データの取得
            TransportBranchOffice transportBranchOffice =
                (from a in db.TransportBranchOffice
                 where a.TransportBranchOfficeCode.Equals(transportBranchOfficeCode)
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (transportBranchOffice != null)
            {
                transportBranchOffice = EditModel(transportBranchOffice);
            }

            // 運輸支局データの返却
            return transportBranchOffice;
        }

        /// <summary>
        /// 運輸支局マスタデータ検索
        /// </summary>
        /// <param name="transportBranchOfficeCondition">運輸支局検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>運輸支局マスタデータ検索結果</returns>
        public PaginatedList<TransportBranchOffice> GetListByCondition(TransportBranchOffice transportBranchOfficeCondition, int? pageIndex, int? pageSize)
        {
            string transportBranchOfficeCode = transportBranchOfficeCondition.TransportBranchOfficeCode;
            string transportBranchOfficeName = transportBranchOfficeCondition.TransportBranchOfficeName;
            string delFlag = transportBranchOfficeCondition.DelFlag;

            // 運輸支局データの取得
            IOrderedQueryable<TransportBranchOffice> transportBranchOfficeList =
                    from a in db.TransportBranchOffice
                    where (string.IsNullOrEmpty(transportBranchOfficeCode) || a.TransportBranchOfficeCode.Contains(transportBranchOfficeCode))
                    && (string.IsNullOrEmpty(transportBranchOfficeName) || a.TransportBranchOfficeName.Contains(transportBranchOfficeName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.TransportBranchOfficeCode
                    select a;

            // ページング制御情報を付与した運輸支局データの返却
            PaginatedList<TransportBranchOffice> ret = new PaginatedList<TransportBranchOffice>(transportBranchOfficeList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="transportBranchOffice">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private TransportBranchOffice EditModel(TransportBranchOffice transportBranchOffice)
        {
            // 内部コード項目の名称情報取得
            transportBranchOffice.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, transportBranchOffice.DelFlag);

            // 出口
            return transportBranchOffice;
        }
    }
}
