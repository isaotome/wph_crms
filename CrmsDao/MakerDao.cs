using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// メーカーマスタアクセスクラス
    ///   メーカーマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class MakerDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public MakerDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// メーカーマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="makerCode">メーカーコード</param>
        /// <returns>メーカーマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public Maker GetByKey(string makerCode, bool includeDeleted = false)
        {
            // メーカーデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Maker maker =
                (from a in db.Maker
                 where a.MakerCode.Equals(makerCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (maker != null)
            {
                maker = EditModel(maker);
            }

            // メーカーデータの返却
            return maker;
        }

        /// <summary>
        /// メーカーマスタデータ検索
        /// </summary>
        /// <param name="makerCondition">メーカー検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>メーカーマスタデータ検索結果</returns>
        public PaginatedList<Maker> GetListByCondition(Maker makerCondition, int? pageIndex, int? pageSize)
        {
            string makerCode = makerCondition.MakerCode;
            string makerName = makerCondition.MakerName;
            string delFlag = makerCondition.DelFlag;

            // メーカーデータの取得
            IOrderedQueryable<Maker> makerList =
                    from a in db.Maker
                    where (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
                    && (string.IsNullOrEmpty(makerName) || a.MakerName.Contains(makerName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.MakerCode
                    select a;

            // ページング制御情報を付与したメーカーデータの返却
            PaginatedList<Maker> ret = new PaginatedList<Maker>(makerList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="maker">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Maker EditModel(Maker maker)
        {
            // 内部コード項目の名称情報取得
            maker.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, maker.DelFlag);

            // 出口
            return maker;
        }

        /// <summary>
        /// メーカリスト取得
        /// </summary>
        /// <param name="maker">なし</param>
        /// <returns>メーカーデータ(全件)</returns>
        public List<Maker> GetMakerBykey()
        {
            var query =
                from a in db.Maker
                where (a.DelFlag == "0")
                select a;

            return query.ToList<Maker>();
        }
    }
}
