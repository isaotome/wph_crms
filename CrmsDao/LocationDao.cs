using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.Linq;

namespace CrmsDao
{
    /// <summary>
    /// ロケーションマスタアクセスクラス
    ///   ロケーションマスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class LocationDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public LocationDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// ロケーションマスタデータ取得(PK指定)
        /// </summary>
        /// <param name="locationCode">ロケーションコード</param>
        /// <param name="includeDeleted">削除データを含むか否か(true:含む、false:含まない)</param>
        /// <returns>ロケーションマスタデータ(1件)</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 データ取得方法をlinq→ストアドに変更
        /// 2015/04/27 IPO対応(部品棚卸) arc yano ロケーションコードの取得方法変更(DelFlagを引数についか) 
        /// </history>
        public Location GetByKey(string locationCode, bool includeDeleted = false)
        {
            // ロケーションデータの取得
            Location location =
                (from a in db.Location
                 where a.LocationCode.Equals(locationCode)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (location != null)
            {
                location = EditModel(location);
            }

            // ロケーションデータの返却
            return location;
        }

        /// <summary>
        /// ロケーションマスタデータ取得
        /// </summary>
        /// <param name="code"></param>
        /// <param name="businessType"></param>
        /// <param name="locationType"></param>
        /// <param name="includedeleted"></param>
        /// <returns></returns>
        /// <history>
        /// 2021/08/03 yano #4098 【ロケーションマスタ】ロケーションを無効に変更した時のチェック処理の追加
        /// </history>
        public Location GetByBusinessType(string code, string businessType, string locationType, bool includedeleted = false) {
            // Add 2014/10/08 arc amii 無効データ検索対応 #3073 DelFlagの検索条件を追加
            var query = from a in db.Location
                        where a.LocationCode.Equals(code)
                        && (string.IsNullOrEmpty(locationType) || a.LocationType.Equals(locationType))
                        && (includedeleted || a.DelFlag.Equals("0"))    //Mod 2021/08/03 yano #4098
                        //&& a.DelFlag.Equals("0")
                        select a;

            if (!string.IsNullOrEmpty(businessType)) {
                string[] keyList = businessType.Split(',');
                ParameterExpression param = Expression.Parameter(typeof(Location), "x");
                BinaryExpression body = null;
                MemberExpression left = Expression.Property(Expression.Property(param, "Department"), "BusinessType");
                foreach (var b in keyList) {
                    ConstantExpression right = Expression.Constant(b, typeof(string));
                    if (body == null) {
                        body = Expression.Equal(left, right);
                    } else {
                        body = Expression.OrElse(body, Expression.Equal(left, right));
                    }
                }
                query = query.Where(Expression.Lambda<Func<Location, bool>>(body, param));
            }
            return query.FirstOrDefault();
        }
        /*
        /// <summary>
        /// ロケーションマスタデータ検索
        /// </summary>
        /// <param name="locationCondition">ロケーション検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>ロケーションマスタデータ検索結果</returns>
        public PaginatedList<Location> GetListByCondition(Location locationCondition, int? pageIndex, int? pageSize)
        {
            string locationCode = locationCondition.LocationCode;
            string locationName = locationCondition.LocationName;
            string departmentCode = null;
            try { departmentCode = locationCondition.Department.DepartmentCode; } catch (NullReferenceException) { }
            string departmentName = null;
            try { departmentName = locationCondition.Department.DepartmentName; } catch (NullReferenceException) { }
            string delFlag = locationCondition.DelFlag;
            string businessType = null;
            try { businessType = locationCondition.Department.BusinessType; } catch (NullReferenceException) { }
            string locationType = locationCondition.LocationType;

            // ロケーションデータの取得
            var locationList =
                    from a in db.Location
                    where (string.IsNullOrEmpty(locationCode) || a.LocationCode.Contains(locationCode))
                    && (string.IsNullOrEmpty(locationName) || a.LocationName.Contains(locationName))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && (string.IsNullOrEmpty(departmentName) || a.Department.DepartmentName.Contains(departmentName))
                    && (string.IsNullOrEmpty(locationType) || a.LocationType.Equals(locationType))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    //orderby a.DepartmentCode, a.LocationCode
                    select a;

            if (!string.IsNullOrEmpty(businessType)) {
                string[] keyList = businessType.Split(',');
                ParameterExpression param = Expression.Parameter(typeof(Location), "x");
                BinaryExpression body = null;
                MemberExpression left = Expression.Property(Expression.Property(param, "Department"), "BusinessType");
                foreach (var b in keyList) {
                    ConstantExpression right = Expression.Constant(b, typeof(string));
                    if (body == null) {
                        body = Expression.Equal(left, right);
                    } else {
                        body = Expression.OrElse(body, Expression.Equal(left, right));
                    }
                }
                locationList = locationList.Where(Expression.Lambda<Func<Location, bool>>(body, param));
            }
            locationList = locationList.OrderBy(x=>x.LocationCode).OrderBy(x => x.DepartmentCode);

            // ページング制御情報を付与したロケーションデータの返却
            PaginatedList<Location> ret = new PaginatedList<Location>(locationList, pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++)
            {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }
        */
        /// <summary>
        /// ロケーションマスタデータ検索
        /// </summary>
        /// <param name="locationCondition">ロケーション検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>ロケーションマスタデータ検索結果</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応
        ///                                     ①ロケーション情報をストアドで取得するように対応
        ///                                     ②検索条件に倉庫コード、倉庫名を追加
        ///                                     ③検索条件の設定方法の変更(関連付けによる設定をやめる)
        /// </history>
        public PaginatedList<Location> GetListByCondition(Location locationCondition, int? pageIndex, int? pageSize)
        {

            string locationCode = locationCondition.LocationCode;
            string locationName = locationCondition.LocationName;
            string departmentCode = locationCondition.DepartmentCode;
            string departmentName = locationCondition.DepartmentName;
            string delFlag = locationCondition.DelFlag;
            string businessType = locationCondition.BusinessType;
            string locationType = locationCondition.LocationType;
            string warehouseCode = locationCondition.WarehouseCode;
            string warehouseName = locationCondition.WarehouseName;

            /*
            string departmentCode = null;
            try { departmentCode = locationCondition.Department.DepartmentCode; }
            catch (NullReferenceException) { }
            string departmentName = null;
            try { departmentName = locationCondition.Department.DepartmentName; }
            catch (NullReferenceException) { }
            string delFlag = locationCondition.DelFlag;
            string businessType = null;
            try { businessType = locationCondition.Department.BusinessType; }
            catch (NullReferenceException) { }
            string locationType = locationCondition.LocationType;
            */
           
            ISingleResult<GetLocationListResult> retLocationList = db.GetLocationList(departmentCode, departmentName, warehouseCode, warehouseName, locationCode, locationName, locationType, delFlag);

            List<Location> list = new List<Location>();

            //ロケーションクラスへ詰め替え
            foreach (var l in retLocationList)
            {
                Location location = new Location();

                location.LocationCode = l.LocationCode;
                location.LocationName = l.LocationName;
                location.BusinessType = l.BusinessType;
                location.LocationTypeName = l.LocationTypeName;
                location.DepartmentCode = l.DepartmentCode;
                location.DepartmentName = l.DepartmentName;
                location.WarehouseCode = l.WarehouseCode;
                location.WarehouseName = l.WarehouseName;
                location.DelFlag = l.DelFlag;

                //削除フラグ名(有効／無効)の設定
                location = EditModel(location);

                list.Add(location);
            }

            var locationList = list.AsQueryable();

            //業務区分による絞り込み
            if (!string.IsNullOrEmpty(businessType))
            {
                string[] keyList = businessType.Split(',');
                ParameterExpression param = Expression.Parameter(typeof(Location), "x");
                BinaryExpression body = null;
                MemberExpression left = Expression.Property(param ,"BusinessType");
                foreach (var b in keyList)
                {
                    ConstantExpression right = Expression.Constant(b, typeof(string));
                    if (body == null)
                    {
                        body = Expression.Equal(left, right);
                    }
                    else
                    {
                        body = Expression.OrElse(body, Expression.Equal(left, right));
                    }
                }
                locationList = locationList.Where(Expression.Lambda<Func<Location, bool>>(body, param));
            }
            locationList = locationList.OrderBy(x => x.LocationCode).OrderBy(x => x.WarehouseCode);

            // ページング制御情報を付与したロケーションデータの返却
            PaginatedList<Location> ret = new PaginatedList<Location>(locationList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

       
        /// <summary>
        /// 同一倉庫内の同一ロケーション種別が設定されているロケーションリストを取得する
        /// </summary>
        /// <param name="locationType">ロケーション種別</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="currentLocationCode">除外するロケーションコード</param>
        /// <returns>
        /// 2016/08/13 arc yano #3596【大項目】部門棚統合対応 新規作成
        /// </returns>
        /// <history>
        /// </history>
        public List<Location> GetListByLocationType(string locationType, string warehouseCode, string currentLocationCode)
        {
            var query =
                from a in db.Location
                where (string.IsNullOrEmpty(locationType) || a.LocationType.Equals(locationType))
                && (string.IsNullOrEmpty(warehouseCode) || a.WarehouseCode.Equals(warehouseCode))
                && (string.IsNullOrEmpty(currentLocationCode) || !a.LocationCode.Equals(currentLocationCode))
                && !a.DelFlag.Equals("1")
                select a;
            return query.ToList<Location>();
        }
        /*
        /// <summary>
        /// 同一部門内の同一ロケーション種別が設定されているロケーションリストを取得する
        /// </summary>
        /// <param name="locationType">ロケーション種別</param>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="currentLocationCode">除外するロケーションコード</param>
        /// <returns></returns>
        /// <history>
        /// </history>
        public List<Location> GetListByLocationType(string locationType, string departmentCode, string currentLocationCode) {
            var query =
                from a in db.Location
                where (string.IsNullOrEmpty(locationType) || a.LocationType.Equals(locationType))
                && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(currentLocationCode) || !a.LocationCode.Equals(currentLocationCode))
                && !a.DelFlag.Equals("1")
                select a;
            return query.ToList<Location>();
        }
        */


        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="location">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private Location EditModel(Location location)
        {
            // 内部コード項目の名称情報取得
            //mod 2015/12/21 arc.ookubo ロケーションマスタ一覧画面のステータスが常に有効になる不具合対応
            //location.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, CommonUtils.IntToStr(0));
            location.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, location.DelFlag);

            // 出口
            return location;
        }

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="location">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数・戻り値の型違いのメソッドの追加
        /// </history>
        private GetLocationListResult EditModel(GetLocationListResult location)
        {
            location.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, location.DelFlag);
            // 出口
            return location;
        }

        //Add 2014/11/07 arc yano 車両ステータス変更対応 車両ステータス変更画面用のロケーション一覧を取得する。
        /// <summary>
        /// ロケーションマスタデータ検索(車両ステータス入力画面専用)
        /// </summary>
        /// <param name="locationCondition">ロケーション検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>ロケーションマスタデータ検索結果</returns>
        public PaginatedList<V_LocationListForCarUsage> GetListForCarUsageByCondition(V_LocationListForCarUsage locationCondition, int? pageIndex, int? pageSize)
        {
            string locationCode = locationCondition.LocationCode;
            string locationName = locationCondition.LocationName;
          
            // ロケーションデータの取得
            var locationList =
                    from a in db.V_LocationListForCarUsage
                    where (string.IsNullOrEmpty(locationCode) || a.LocationCode.Contains(locationCode))
                    && (string.IsNullOrEmpty(locationName) || a.LocationName.Contains(locationName))
                    select a;

            locationList = locationList.OrderBy(x => x.LocationCode);

            // ページング制御情報を付与したロケーションデータの返却
            PaginatedList<V_LocationListForCarUsage> ret = new PaginatedList<V_LocationListForCarUsage>(locationList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        //Add 2014/11/07 arc yano 車両ステータス変更対応 車両ステータス変更画面用のロケーションデータを取得する。
        /// <summary>
        /// ロケーションマスタデータ検索(車両ステータス入力画面専用)
        /// </summary>
        /// <param name="locationCode">ロケーション検索条件</param>
        /// <returns>ロケーションマスタデータ検索結果</returns>
        public V_LocationListForCarUsage GetByKeyForCarUsage(string locationCode)
        {
            // ロケーションデータの取得
            V_LocationListForCarUsage location =
                    (from a in db.V_LocationListForCarUsage
                    where (string.IsNullOrEmpty(locationCode) || a.LocationCode.Equals(locationCode))
                    select a).FirstOrDefault();


            // 出口
            return location;
        }
    }
}
