using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao
{
    /// <summary>
    /// 販売車両マスタアクセスクラス
    ///   販売車両マスタの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class SalesCarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public SalesCarDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 販売車両マスタデータ取得(PK指定)
        /// </summary>
        /// <param name="salesCarNumber">販売車両コード</param>
        /// <returns>販売車両マスタデータ(1件)</returns>
        public SalesCar GetByKey(string salesCarNumber, bool includeDeleted = false)
        {
            // 販売車両データの取得
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            //Mod 2015/04/08 arc nakayama 無効データを開くと落ちる対応　デフォルト引数追加（削除フラグを考慮するかどうかのフラグ追加（デフォルト考慮する））
            SalesCar salesCar =
                (from a in db.SalesCar
                 where a.SalesCarNumber.Equals(salesCarNumber)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a
                ).FirstOrDefault();

            // 内部コード項目の名称情報取得
            if (salesCar != null)
            {
                salesCar = EditModel(salesCar);
            }

            // 販売車両データの返却
            return salesCar;
        }

        /// <summary>
        /// 車台番号から販売車両リストを取得する
        /// </summary>
        /// <param name="vin">車台番号</param>
        /// <returns></returns>
        public List<SalesCar> GetByVin(string vin) {
            var query =
                from a in db.SalesCar
                where a.Vin.Equals(vin)
                && a.DelFlag.Equals("0")
                select a;
            return query.ToList();
        }

        /// <summary>
        /// 販売車両マスタデータ取得(所有者＋車台番号指定)
        /// </summary>
        /// <param name="userCode">使用者コード</param>
        /// <param name="vin">車台番号</param>
        /// <returns>販売車両マスタデータ</returns>
        public List<SalesCar> GetListByReceiption(string userCode, string vin) {

            // 販売車両データの取得
            List<SalesCar> salesCarList =
                (from a in db.SalesCar
                 where a.UserCode.Equals(userCode)
                 && a.Vin.Equals(vin)
                 orderby a.SalesCarNumber, a.Vin
                 select a
                ).ToList<SalesCar>();

            // 内部コード項目の名称情報取得
            for (int i = 0; i < salesCarList.Count; i++) {
                salesCarList[i] = EditModel(salesCarList[i]);
            }

            // 販売車両データの返却
            return salesCarList;
        }

        /// <summary>
        /// 販売車両マスタデータ検索
        /// （ページング対応）
        /// </summary>
        /// <param name="salesCarCondition">販売車両検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>販売車両マスタデータ検索結果</returns>
        public PaginatedList<SalesCar> GetListByCondition(SalesCar salesCarCondition, int? pageIndex, int? pageSize)
        {
            // ページング制御情報を付与した販売車両データの返却
            PaginatedList<SalesCar> ret = new PaginatedList<SalesCar>(GetQueryable(salesCarCondition), pageIndex ?? 0, pageSize ?? 0);

            // 内部コード項目の名称情報取得
            for (int i = 0; i < ret.Count; i++) {
                ret[i] = EditModel(ret[i]);
            }

            // 出口
            return ret;
        }

        /// <summary>
        /// 車両マスタデータ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(部門コード)は不要のため、廃止
        /// </history>
        public List<SalesCar> GetListByCondition(DocumentExportCondition condition){
            SalesCar salesCarCondition = new SalesCar();
            salesCarCondition.CarStatus = condition.CarStatus;
            salesCarCondition.NewUsedType = condition.NewUsedType;
            salesCarCondition.Location = new Location();
            //salesCarCondition.Location.DepartmentCode = condition.DepartmentCode; //Del 2016/08/13 arc yano #3596
            return GetQueryable(salesCarCondition).ToList();
        }

        /// <summary>
        /// 車両マスタデータ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 検索条件(部門コード)は不要のため、廃止
        /// </history>
        public List<SalesCar> GetListByCondition(SalesCar condition) {
            return GetQueryable(condition).ToList();
        }
        /// <summary>
        /// 車両検索条件式を取得する
        /// </summary>
        /// <param name="salesCarCondition">検索条件</param>
        /// <returns></returns>
        private IQueryable<SalesCar> GetQueryable(SalesCar salesCarCondition){
            string salesCarNumber = salesCarCondition.SalesCarNumber;
            string carBrandName = null;
            try { carBrandName = salesCarCondition.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
            string carName = null;
            try { carName = salesCarCondition.CarGrade.Car.CarName; } catch (NullReferenceException) { }
            string carGradeName = null;
            try { carGradeName = salesCarCondition.CarGrade.CarGradeName; } catch (NullReferenceException) { }
            string newUsedType = salesCarCondition.NewUsedType;
            string colorType = salesCarCondition.ColorType;
            string exteriorColorCode = salesCarCondition.ExteriorColorCode;
            string exteriorColorName = salesCarCondition.ExteriorColorName;
            string interiorColorCode = salesCarCondition.InteriorColorCode;
            string interiorColorName = salesCarCondition.InteriorColorName;
            string manufacturingYear = salesCarCondition.ManufacturingYear;
            string carStatus = salesCarCondition.CarStatus;
            // Mod 2014/10/16 arc yano  車両管理ステータス追加対応　検索条件に利用用途(CarUsage)を追加
            string carUsage = salesCarCondition.CarUsage;
            string locationName = null;
            try { locationName = salesCarCondition.Location.LocationName; } catch (NullReferenceException) { }
            string customerName = null;
            try { customerName = salesCarCondition.Customer.CustomerName; } catch (NullReferenceException) { }
            string vin = salesCarCondition.Vin;
            string morterViecleOfficialCode = salesCarCondition.MorterViecleOfficialCode;
            string registrationNumberType = salesCarCondition.RegistrationNumberType;
            string registrationNumberKana = salesCarCondition.RegistrationNumberKana;
            string registrationNumberPlate = salesCarCondition.RegistrationNumberPlate;
            string steering = salesCarCondition.Steering;

            // Del 2016/08/13 arc yano #3596
            // string departmentCode = null;
            // try { departmentCode = salesCarCondition.Location.DepartmentCode; } catch (NullReferenceException) { }
            string delFlag = salesCarCondition.DelFlag;

            string expireType = salesCarCondition.ExpireType;
            DateTime? expireDateFrom = salesCarCondition.ExpireDateFrom;
            DateTime? expireDateTo = salesCarCondition.ExpireDateTo;
            DateTime? nextInspectionDateFrom = salesCarCondition.NextInspectionDateFrom;
            DateTime? nextInspectionDateTo = salesCarCondition.NextInspectionDateTo;

            string userName = "";
            try { userName = salesCarCondition.User.CustomerName; } catch (NullReferenceException) { }
            string userNameKana = "";
            try { userNameKana = salesCarCondition.User.CustomerNameKana; } catch (NullReferenceException) { }

            // Mod 2014/10/16 arc yano  車両管理ステータス追加対応　検索条件に利用用途(CarUsage)を追加
            // 販売車両データの取得
            IOrderedQueryable<SalesCar> salesCarList =
                    from a in db.SalesCar
                    where (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
                    && (string.IsNullOrEmpty(carBrandName) || a.CarGrade.Car.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(carName) || a.CarGrade.Car.CarName.Contains(carName))
                    && (string.IsNullOrEmpty(carGradeName) || a.CarGrade.CarGradeName.Contains(carGradeName))
                    && (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
                    && (string.IsNullOrEmpty(colorType) || a.ColorType.Equals(colorType))
                    && (string.IsNullOrEmpty(manufacturingYear) || a.ManufacturingYear.Contains(manufacturingYear))
                    && (string.IsNullOrEmpty(exteriorColorCode) || a.ExteriorColorCode.Contains(exteriorColorCode))
                    && (string.IsNullOrEmpty(exteriorColorName) || a.ExteriorColorName.Contains(exteriorColorName))
                    && (string.IsNullOrEmpty(interiorColorCode) || a.InteriorColorCode.Contains(interiorColorCode))
                    && (string.IsNullOrEmpty(interiorColorName) || a.InteriorColorName.Contains(interiorColorName))
                    && (string.IsNullOrEmpty(carStatus) || a.CarStatus.Equals(carStatus))
                    && (string.IsNullOrEmpty(carUsage) || a.CarUsage.Equals(carUsage))
                    && (string.IsNullOrEmpty(carBrandName) || a.CarGrade.Car.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(locationName) || a.Location.LocationName.Contains(locationName))
                    && (string.IsNullOrEmpty(customerName) || a.Customer.CustomerName.Contains(customerName))
                    && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                    && (string.IsNullOrEmpty(morterViecleOfficialCode) || a.MorterViecleOfficialCode.Contains(morterViecleOfficialCode))
                    && (string.IsNullOrEmpty(registrationNumberType) || a.RegistrationNumberType.Contains(registrationNumberType))
                    && (string.IsNullOrEmpty(registrationNumberKana) || a.RegistrationNumberKana.Contains(registrationNumberKana))
                    && (string.IsNullOrEmpty(registrationNumberPlate) || a.RegistrationNumberPlate.Equals(registrationNumberPlate))
                    && (string.IsNullOrEmpty(steering) || a.Steering.Contains(steering))
                        //       && (string.IsNullOrEmpty(departmentCode) || a.Location.DepartmentCode.Equals(departmentCode))             // Del 2016/08/13 arc yano #3596
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(expireType) || a.ExpireType.Equals(expireType))
                    && (expireDateFrom==null || DateTime.Compare(a.ExpireDate ?? DaoConst.SQL_DATETIME_MAX,expireDateFrom ?? DaoConst.SQL_DATETIME_MIN)>=0)
                    && (expireDateTo==null || DateTime.Compare(a.ExpireDate ?? DaoConst.SQL_DATETIME_MIN ,expireDateTo ?? DaoConst.SQL_DATETIME_MAX)<=0)
                    && (nextInspectionDateFrom==null || DateTime.Compare(a.NextInspectionDate ?? DaoConst.SQL_DATETIME_MAX,nextInspectionDateFrom ?? DaoConst.SQL_DATETIME_MIN)>=0)
                    && (nextInspectionDateTo==null || DateTime.Compare(a.NextInspectionDate ?? DaoConst.SQL_DATETIME_MIN,nextInspectionDateTo ?? DaoConst.SQL_DATETIME_MAX)<=0)
                    && (string.IsNullOrEmpty(userName) || a.User.CustomerName.Contains(userName))
                    && (string.IsNullOrEmpty(userNameKana) || a.User.CustomerNameKana.Contains(userNameKana))
                    orderby a.SalesCarNumber, a.Vin
                    select a;
            return salesCarList;

            
        }

        /*  //2016/08/13 arc yano 未使用のため、コメントアウト
        /// <summary>
        /// 在庫表を検索する
        /// </summary>
        /// <param name="departmentCode"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PaginatedList<SalesCar> GetStockList(string departmentCode, string newUsedType, int pageIndex, int pageSize) {
            var query = from a in db.SalesCar
                        orderby a.CarGrade.Car.CarBrandCode,a.CarGrade.CarCode,a.CarGradeCode,a.SalesPrice
                        where a.LocationCode!=null
                        && (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
                        && (string.IsNullOrEmpty(departmentCode) || a.Location.DepartmentCode.Equals(departmentCode))
                        && a.DelFlag.Equals("0")
                        && !a.CarStatus.Equals("006")
                        select a;
            return new PaginatedList<SalesCar>(query, pageIndex, pageSize);
        }
        */

        /// <summary>
        /// モデルデータの編集
        /// </summary>
        /// <param name="salesCar">モデルデータ</param>
        /// <returns>編集後モデルデータ</returns>
        private SalesCar EditModel(SalesCar salesCar)
        {
            // 内部コード項目の名称情報取得
            salesCar.DelFlagName = CodeUtils.GetName(CodeUtils.DelFlag, salesCar.DelFlag);

            // 出口
            return salesCar;
        }

        /// <summary>
        /// 車台番号から車両情報（一件）を取得する
        /// </summary>
        /// <param name="vin">車台番号</param>
        /// <returns>車両マスタデータ（一件）</returns>
        public SalesCar GetDataByVin(string vin)
        {
            var query =
                (from a in db.SalesCar
                where a.Vin.Equals(vin)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 車台番号から販売車両リストを取得する
        /// </summary>
        /// <param name="vin">車台番号</param>
        /// <returns>車両リスト</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public List<SalesCar> GetByLikeVin(string vin)
        {
            var query =
                from a in db.SalesCar
                where a.Vin.Contains(vin)
                && a.DelFlag.Equals("0")
                select a;
            return query.ToList();
        }
    }
}
