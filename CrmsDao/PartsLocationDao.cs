using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 部品ロケーションマスタアクセスクラス
    ///   部品ロケーションマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class PartsLocationDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public PartsLocationDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 部品ロケーションマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <returns>部品ロケーションマスタデータ(1件)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件の変更(部門コード→倉庫コード)
        /// </history>
        public PartsLocation GetByKey(string partsNumber, string warehouseCode, bool delflag = true)
        {
            // 部品ロケーションデータの取得
            PartsLocation partsLocation =
                (from a in db.PartsLocation
                 where a.PartsNumber.Equals(partsNumber)
                 && a.WarehouseCode.Equals(warehouseCode)
                 && (delflag == false || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (partsLocation != null)
            {
                partsLocation = EditModel(partsLocation);
            }

            // 部品ロケーションデータの返却
            return partsLocation;
        }
        /*
        public PartsLocation GetByKey(string partsNumber, string departmentCode, bool delflag = true)
        {
            // 部品ロケーションデータの取得
            PartsLocation partsLocation =
                (from a in db.PartsLocation
                 where a.PartsNumber.Equals(partsNumber)
                 && a.DepartmentCode.Equals(departmentCode)
                 && (delflag == false ||  a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (partsLocation != null)
            {
                partsLocation = EditModel(partsLocation);
            }

            // 部品ロケーションデータの返却
            return partsLocation;
        }
        */
        

        /// <summary>
        /// 部品ロケーションマスタデータ検索
        /// </summary>
        /// <param name="partsLocationCondition">部品ロケーション検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>部品ロケーションマスタデータ検索結果</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
        ///                            ①検索条件に倉庫を追加
        ///                            ②検索条件の部門の設定の変更(関連付けによるアクセスの廃止)
        ///                            ③データの取得をlinqからストアドに変更
        /// </history>
        public PaginatedList<PartsLocation> GetListByCondition(PartsLocation partsLocationCondition, int? pageIndex, int? pageSize)
        {
            string partsNumber = null;
            try { partsNumber = partsLocationCondition.Parts.PartsNumber; } catch (NullReferenceException) { }
            //Mod 2016/08/13 arc yano #3596
            //string departmentCode = null;
            //try { departmentCode = partsLocationCondition.Department.DepartmentCode; } catch (NullReferenceException) { }
            string departmentCode = partsLocationCondition.DepartmentCode;
            string warehouseCode = partsLocationCondition.WarehouseCode;
            string delFlag = partsLocationCondition.DelFlag;
            string locationCode = partsLocationCondition.LocationCode;

            // 部品ロケーションデータの取得
            /*
            IOrderedQueryable<PartsLocation> partsLocationList =
                    from a in db.PartsLocation
                    where (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Equals(partsNumber))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(locationCode) || a.LocationCode.Equals(locationCode))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.PartsNumber, a.DepartmentCode
                    select a;
             */

            //ストアドで取得
            var partsLocationList = db.GetPartsLocationList(partsNumber, departmentCode, warehouseCode, locationCode, delFlag);

            List<PartsLocation> list = new List<PartsLocation>();

            //データの詰め替え
            foreach(var pl in partsLocationList)
            {
                PartsLocation data = new PartsLocation();

                data.PartsNumber = pl.PartsNumber;          //部品番号
                data.PartsNameJp = pl.PartsNameJp;          //部品名
                data.DepartmentCode = pl.DepartmentCode;    //部門コード
                data.DepartmentName = pl.DepartmentName;    //部門名
                data.WarehouseCode = pl.WarehouseCode;      //倉庫コード
                data.WarehouseName = pl.WarehouseName;      //倉庫名
                data.LocationCode = pl.LocationCode;        //ロケーションコード
                data.LocationName = pl.LocationName;        //ロケーション名
                data.DelFlag = pl.DelFlag;

                //削除フラグの文言の設定
                data = EditModel(data);

                list.Add(data);
            }

            // ページング制御情報を付与した部品ロケーションデータの返却
            PaginatedList<PartsLocation> ret = new PaginatedList<PartsLocation>(list.AsQueryable(), pageIndex ?? 0, pageSize ?? 0);

            /*
            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }
            */

            // 出口
            return ret;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="partsLocation">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private PartsLocation EditModel(PartsLocation partsLocation)
        {
            // 内部コード項目の名称情報取得
            partsLocation.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, partsLocation.DelFlag);

            // 出口
            return partsLocation;
        }
    }
}
