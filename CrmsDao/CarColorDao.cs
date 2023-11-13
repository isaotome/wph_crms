using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 車両カラーマスタアクセスクラス
    ///   車両カラーマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarColorDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarColorDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両カラーマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="carColorCode">車両カラーコード</param>
        /// <returns>車両カラーマスタデータ(1件)</returns>
        //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　更新の場合は考慮しない（無効データが開けないため）
        public CarColor GetByKey(string carColorCode, bool includeDeleted = false)
        {
            // 車両カラーデータの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            CarColor carColor =
                (from a in db.CarColor
                 where a.CarColorCode.Equals(carColorCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (carColor != null)
            {
                carColor = EditModel(carColor);
            }

            // 車両カラーデータの返却
            return carColor;
        }

        /// <summary>
        /// 車両カラーマスタデータ検索
        /// </summary>
        /// <param name="carColorCondition">車両カラー検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両カラーマスタデータ検索結果</returns>
        public PaginatedList<CarColor> GetListByCondition(CarColor carColorCondition, int? pageIndex, int? pageSize)
        {
            string carColorCode = carColorCondition.CarColorCode;
            string carColorName = carColorCondition.CarColorName;
            string makerCode = null;
            try { makerCode = carColorCondition.Maker.MakerCode; } catch (NullReferenceException) { }
            string makerName = null;
            try { makerName = carColorCondition.Maker.MakerName; } catch (NullReferenceException) { }
            string colorCategory = carColorCondition.ColorCategory;
            string delFlag = carColorCondition.DelFlag;
            string makerColorCode = carColorCondition.MakerColorCode;
            string carGradeCode = carColorCondition.CarGradeCode;
            string interiorColorFlag = carColorCondition.InteriorColorFlag;
            string exteriorColorFlag = carColorCondition.ExteriorColorFlag;

            // 車両カラーデータの取得
            IOrderedQueryable<CarColor> carColorList =
                    from a in db.CarColor
                    where (string.IsNullOrEmpty(carColorCode) || a.CarColorCode.Contains(carColorCode))
                    && (string.IsNullOrEmpty(carColorName) || a.CarColorName.Contains(carColorName))
                    && (string.IsNullOrEmpty(makerCode) || a.MakerCode.Contains(makerCode))
                    && (string.IsNullOrEmpty(makerName) || a.Maker.MakerName.Contains(makerName))
                    && (string.IsNullOrEmpty(colorCategory) || a.ColorCategory.Equals(colorCategory))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(makerColorCode) || a.MakerColorCode.Contains(makerColorCode))
                    && (string.IsNullOrEmpty(interiorColorFlag) || a.InteriorColorFlag.Equals(interiorColorFlag))
                    && (string.IsNullOrEmpty(exteriorColorFlag) || a.ExteriorColorFlag.Equals(exteriorColorFlag))
                    && (string.IsNullOrEmpty(carGradeCode) ||
                    (from b in db.CarAvailableColor
                     where b.CarGradeCode.Equals(carGradeCode)
                     select b.CarColorCode).Contains(a.CarColorCode))
                    orderby a.MakerCode, a.ColorCategory, a.CarColorCode
                    select a;

            // ページング制御情報を付与した車両カラーデータの返却
            PaginatedList<CarColor> ret = new PaginatedList<CarColor>(carColorList, pageIndex ?? 0, pageSize ?? 0);

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
        /// <param name="carColor">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private CarColor EditModel(CarColor carColor)
        {
            // 内部コード項目の名称情報取得
            carColor.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, carColor.DelFlag);

            // 出口
            return carColor;
        }
    }
}
