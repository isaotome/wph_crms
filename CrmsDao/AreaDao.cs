using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// エリアマスタアクセスクラス
    ///   エリアマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class AreaDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public AreaDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// エリアマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="areaCode">エリアコード</param>
        /// <returns>エリアマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため
        public Area GetByKey(string areaCode, bool includeDeleted = false)
        {
            // エリアデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            Area area =
                (from a in db.Area
                 where a.AreaCode.Equals(areaCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (area != null)
            {
                area = EditModel(area);
            }

            // エリアデータの返却
            return area;
        }

        /// <summary>
        /// エリアマスタデータ検索
        /// </summary>
        /// <param name="areaCondition">エリア検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>エリアマスタデータ検索結果</returns>
        public PaginatedList<Area> GetListByCondition(Area areaCondition, int? pageIndex, int? pageSize)
        {
            string areaCode = areaCondition.AreaCode;
            string areaName = areaCondition.AreaName;
            string employeeName = null;
            try { employeeName = areaCondition.Employee.EmployeeName; } catch (NullReferenceException) { }
            string delFlag = areaCondition.DelFlag;

            // エリアデータの取得
            IOrderedQueryable<Area> areaList =
                    from a in db.Area
                    where (string.IsNullOrEmpty(areaCode) || a.AreaCode.Contains(areaCode))
                    && (string.IsNullOrEmpty(areaName) || a.AreaName.Contains(areaName))
                    && (string.IsNullOrEmpty(employeeName) || a.Employee.EmployeeName.Contains(employeeName))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.AreaCode
                    select a;

            // ページング制御情報を付与したエリアデータの返却
            PaginatedList<Area> ret = new PaginatedList<Area>(areaList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="area">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Area EditModel(Area area)
        {
            // 内部コード項目の名称情報取得
            area.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, area.DelFlag);

            // 出口
            return area;
        }
    }
}
