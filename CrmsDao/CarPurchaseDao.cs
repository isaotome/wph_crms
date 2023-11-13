using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace CrmsDao {

    /// <summary>
    /// 車両入荷テーブルアクセスクラス
    ///   車両入荷テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    public class CarPurchaseDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarPurchaseDao(CrmsLinqDataContext dataContext) {
            db = dataContext;
        }

        /// <summary>
        /// 車両入荷テーブルデータ取得(PK指定)
        /// </summary>
        /// <param name="carPurchaseId">車両入荷ID</param>
        /// <returns>車両入荷テーブルデータ(1件)</returns>
        public CarPurchase GetByKey(Guid carPurchaseId) {

            // 車両入荷データの取得
            CarPurchase carPurchase =
                (from a in db.CarPurchase
                 where a.CarPurchaseId.Equals(carPurchaseId)
                 select a
                ).FirstOrDefault();

            // 車両入荷データの返却
            return carPurchase;
        }

        /// <summary>
        /// 車両入荷テーブルデータ取得(車両査定ID指定)
        /// </summary>
        /// <param name="carAppraisalId">車両査定ID</param>
        /// <returns>車両入荷テーブルデータ(1件)</returns>
        public CarPurchase GetByCarAppraisalId(Guid carAppraisalId) {

            // 車両入荷データの取得
            CarPurchase carPurchase =
                (from a in db.CarPurchase
                 where a.CarAppraisalId.Equals(carAppraisalId)
                 select a
                ).FirstOrDefault();

            // 車両入荷データの返却
            return carPurchase;
        }

        /// <summary>
        /// 車両入荷テーブルデータ取得(車両ID指定)
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <returns>車両入荷テーブルデータ(1件)</returns>
        public CarPurchase GetBySalesCarNumber(string salesCarNumber) {

            CarPurchase query =
                (from a in db.CarPurchase
                 where a.SalesCarNumber.Equals(salesCarNumber)
                 select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 仕入計上済み車両入荷データリスト取得(車両伝票番号、Vin指定)
        /// </summary>
        /// <param name="slipNumber">車両伝票番号</param>
        /// <returns>車両入荷テーブルリスト</returns>
        public List<CarPurchase> GetBySlipNumber(string slipNumber,string vin) {
            IQueryable<CarPurchase> query =
                from a in db.CarPurchase
                where a.CarAppraisal.SlipNumber.Equals(slipNumber)
                && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Equals(vin))
                && a.PurchaseStatus.Equals("002")
                select a;
            return query.ToList();
        }
        /// <summary>
        /// 車両入荷テーブルデータ検索
        /// </summary>
        /// <param name="carPurchaseCondition">車両入荷検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両入荷テーブルデータ検索結果</returns>
        public PaginatedList<CarPurchase> GetListByCondition(CarPurchase carPurchaseCondition, int? pageIndex, int? pageSize) {

            string salesCarNumber = null;
            try { salesCarNumber = carPurchaseCondition.SalesCar.SalesCarNumber; } catch (NullReferenceException) { }
            string departmentCode = null;
            // MOD 2014/06/09 arc uchida コーディングの修正
            //try { departmentCode = carPurchaseCondition.DepartmentCode; } catch (NullReferenceException) { }
            try { departmentCode = carPurchaseCondition.Department.DepartmentCode; } catch (NullReferenceException) { }
            string supplierCode = null;
            try { supplierCode = carPurchaseCondition.Supplier.SupplierCode; } catch (NullReferenceException) { }
            string purchaseStatus = carPurchaseCondition.PurchaseStatus;
            DateTime? purchaseOrderDateFrom = carPurchaseCondition.PurchaseOrderDateFrom;
            DateTime? purchaseOrderDateTo = carPurchaseCondition.PurchaseOrderDateTo;
            DateTime? purchaseDateFrom = carPurchaseCondition.PurchaseDateFrom;
            DateTime? purchaseDateTo = carPurchaseCondition.PurchaseDateTo;
            DateTime? slipDateFrom = carPurchaseCondition.SlipDateFrom;
            DateTime? slipDateTo = carPurchaseCondition.SlipDateTo;
            string makerName = null;
            try { makerName = carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName; } catch (NullReferenceException) { }
            string carBrandName = null;
            try { carBrandName = carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName; } catch (NullReferenceException) { }
            string carName = null;
            try { carName = carPurchaseCondition.SalesCar.CarGrade.Car.CarName; } catch (NullReferenceException) { }
            string carGradeName = null;
            try { carGradeName = carPurchaseCondition.SalesCar.CarGrade.CarGradeName; } catch (NullReferenceException) { }
            string vin = carPurchaseCondition.SalesCar.Vin;
            string delFlag = carPurchaseCondition.DelFlag;

            // 車両入荷データの取得
            IOrderedQueryable<CarPurchase> carPurchaseList =
                    from a in db.CarPurchase
                    where (string.IsNullOrEmpty(salesCarNumber) || a.SalesCar.SalesCarNumber.Contains(salesCarNumber))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(supplierCode) || a.Supplier.SupplierCode.Equals(supplierCode))
                    && (string.IsNullOrEmpty(purchaseStatus) || a.PurchaseStatus.Equals(purchaseStatus))
                    && (purchaseOrderDateFrom == null || DateTime.Compare(a.CarPurchaseOrder.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MIN, purchaseOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (purchaseOrderDateTo == null || DateTime.Compare(a.CarPurchaseOrder.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MAX, purchaseOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (slipDateFrom == null || DateTime.Compare(a.SlipDate ?? DaoConst.SQL_DATETIME_MIN, slipDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (slipDateTo == null || DateTime.Compare(a.SlipDate ?? DaoConst.SQL_DATETIME_MAX, slipDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (purchaseDateFrom == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, purchaseDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (purchaseDateTo == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, purchaseDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(makerName) || a.SalesCar.CarGrade.Car.Brand.Maker.MakerName.Contains(makerName))
                    && (string.IsNullOrEmpty(carBrandName) || a.SalesCar.CarGrade.Car.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(carName) || a.SalesCar.CarGrade.Car.CarName.Contains(carName))
                    && (string.IsNullOrEmpty(carGradeName) || a.SalesCar.CarGrade.CarGradeName.Contains(carGradeName))
                    && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Contains(vin))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    /*orderby ((a.SalesCar.SalesCarNumber != null && !a.SalesCar.SalesCarNumber.Equals("")) ? "1" : "2")
                    , a.SalesCar.SalesCarNumber
                    , ((a.DepartmentCode != null && !a.DepartmentCode.Equals("")) ? "1" : "2")
                    , a.DepartmentCode
                    , ((a.PurchaseStatus != null && !a.PurchaseStatus.Equals("")) ? "1" : "2")
                    , a.PurchaseStatus
                    , a.SalesCar.NewUsedType
                    , ((a.SupplierCode != null && !a.SupplierCode.Equals("")) ? "1" : "2")
                    , a.SupplierCode
                    , (a.CarPurchaseOrder.PurchaseOrderDate != null ? "1" : "2")
                    , a.CarPurchaseOrder.PurchaseOrderDate
                    , (a.CarPurchaseOrder.InvoiceDate != null ? "1" : "2")
                    , a.CarPurchaseOrder.InvoiceDate
                    , (a.PurchaseDate != null ? "1" : "2")
                    , a.PurchaseDate
                    , ((a.SalesCar.CarGrade.Car.Brand.Maker.MakerCode != null && !a.SalesCar.CarGrade.Car.Brand.Maker.MakerCode.Equals("")) ? "1" : "2")
                    , a.SalesCar.CarGrade.Car.Brand.Maker.MakerCode
                    , ((a.SalesCar.CarGrade.Car.Brand.CarBrandCode != null && !a.SalesCar.CarGrade.Car.Brand.CarBrandCode.Equals("")) ? "1" : "2")
                    , a.SalesCar.CarGrade.Car.CarCode
                    , ((a.SalesCar.CarGrade.Car.CarCode != null && !a.SalesCar.CarGrade.Car.CarCode.Equals("")) ? "1" : "2")
                    , a.SalesCar.CarGrade.CarGradeCode
                    , ((a.SalesCar.CarGrade.CarGradeCode != null && !a.SalesCar.CarGrade.CarGradeCode.Equals("")) ? "1" : "2")
                    , a.SalesCar.CarGrade.Car.Brand.Maker.MakerCode
                    , a.SalesCar.Vin
                    , a.CarPurchaseId
                    */
                    orderby a.LastUpdateDate descending
                    select a;

            // ページング制御情報を付与した車両入荷データの返却
            PaginatedList<CarPurchase> ret = new PaginatedList<CarPurchase>(carPurchaseList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        public PaginatedList<V_CarPurchaseList> GetListByCondition(V_CarPurchaseList condition, int pageIndex, int pageSize) {
            DateTime? purchasePlanDateFrom = condition.PurchasePlanDateFrom;
            DateTime? purchasePlanDateTo = condition.PurchasePlanDateTo;
            string departmentCode = condition.DepartmentCode;
            string purchaseStatus = condition.PurchaseStatus;
            string vin = condition.Vin;
            string makerName = condition.MakerName;
            string carName = condition.CarName;

            var query = from a in db.V_CarPurchaseList
                        where (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                        && (string.IsNullOrEmpty(purchaseStatus) || a.PurchaseStatus.Equals(purchaseStatus))
                        && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                        && (string.IsNullOrEmpty(makerName) || a.MakerName.Contains(makerName))
                        && (string.IsNullOrEmpty(carName) || a.CarName.Contains(carName))
                        && (purchasePlanDateFrom == null || DateTime.Compare(purchasePlanDateFrom ?? DaoConst.SQL_DATETIME_MAX,a.PurchasePlanDate ?? DaoConst.SQL_DATETIME_MIN)<=0)
                        && (purchasePlanDateTo == null || DateTime.Compare(purchasePlanDateTo ?? DaoConst.SQL_DATETIME_MIN,a.PurchasePlanDate ?? DaoConst.SQL_DATETIME_MAX)>=0)
                        orderby a.PurchaseDate
                            ,a.SlipDate
                            ,a.DepartmentCode
                            ,a.PurchaseStatus
                            ,a.SalesCarNumber
                        select a;

            return new PaginatedList<V_CarPurchaseList>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// 車両入荷データリスト取得(車両伝票番号、Vin指定)
        /// </summary>
        /// <param name="slipNumber">車両伝票番号</param>
        /// <returns>車両入荷テーブルリスト</returns>
        public CarPurchase GetBySlipNumberVin(string slipNumber, string vin)
        {
            var query =
                (from a in db.CarPurchase
                where a.SlipNumber.Equals(slipNumber)
                && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Equals(vin))
                select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 車両入荷データリスト取得(車両伝票番号、Vin指定)
        /// </summary>
        /// <param name="slipNumber">車両伝票番号</param>
        /// <param name="vin">車台番号</param>
        /// <returns>車両入荷テーブルリスト</returns>
        /// <history>
        /// 2018/11/09 yano #3938 車両伝票入力　赤黒処理時の下取車仕入データの注文書番号の更新漏れ
        /// </history>
        public List<CarPurchase> GetListBySlipNumberVin(string slipNumber, string vin)
        {
            var query =
                (from a in db.CarPurchase
                 where a.SlipNumber.Equals(slipNumber)
                 && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Equals(vin))
                 select a).ToList();
            return query;
        }


        /// <summary>
        /// 車両入荷テーブルデータ検索 Excel用（ページング設定なし）
        /// </summary>
        /// <param name="carPurchaseCondition">車両入荷検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両入荷テーブルデータ検索結果</returns>
        public List<CarPurchase> GetListByConditionForExcel(CarPurchase carPurchaseCondition)
        {

            string salesCarNumber = null;
            try { salesCarNumber = carPurchaseCondition.SalesCar.SalesCarNumber; }
            catch (NullReferenceException) { }
            string departmentCode = null;
            // MOD 2014/06/09 arc uchida コーディングの修正
            //try { departmentCode = carPurchaseCondition.DepartmentCode; } catch (NullReferenceException) { }
            try { departmentCode = carPurchaseCondition.Department.DepartmentCode; }
            catch (NullReferenceException) { }
            string supplierCode = null;
            try { supplierCode = carPurchaseCondition.Supplier.SupplierCode; }
            catch (NullReferenceException) { }
            string purchaseStatus = carPurchaseCondition.PurchaseStatus;
            DateTime? purchaseOrderDateFrom = carPurchaseCondition.PurchaseOrderDateFrom;
            DateTime? purchaseOrderDateTo = carPurchaseCondition.PurchaseOrderDateTo;
            DateTime? purchaseDateFrom = carPurchaseCondition.PurchaseDateFrom;
            DateTime? purchaseDateTo = carPurchaseCondition.PurchaseDateTo;
            DateTime? slipDateFrom = carPurchaseCondition.SlipDateFrom;
            DateTime? slipDateTo = carPurchaseCondition.SlipDateTo;
            string makerName = null;
            try { makerName = carPurchaseCondition.SalesCar.CarGrade.Car.Brand.Maker.MakerName; }
            catch (NullReferenceException) { }
            string carBrandName = null;
            try { carBrandName = carPurchaseCondition.SalesCar.CarGrade.Car.Brand.CarBrandName; }
            catch (NullReferenceException) { }
            string carName = null;
            try { carName = carPurchaseCondition.SalesCar.CarGrade.Car.CarName; }
            catch (NullReferenceException) { }
            string carGradeName = null;
            try { carGradeName = carPurchaseCondition.SalesCar.CarGrade.CarGradeName; }
            catch (NullReferenceException) { }
            string vin = carPurchaseCondition.SalesCar.Vin;
            string delFlag = carPurchaseCondition.DelFlag;

            // 車両入荷データの取得
            IOrderedQueryable<CarPurchase> carPurchaseList =
                    from a in db.CarPurchase
                    where (string.IsNullOrEmpty(salesCarNumber) || a.SalesCar.SalesCarNumber.Contains(salesCarNumber))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(supplierCode) || a.Supplier.SupplierCode.Equals(supplierCode))
                    && (string.IsNullOrEmpty(purchaseStatus) || a.PurchaseStatus.Equals(purchaseStatus))
                    && (purchaseOrderDateFrom == null || DateTime.Compare(a.CarPurchaseOrder.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MIN, purchaseOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (purchaseOrderDateTo == null || DateTime.Compare(a.CarPurchaseOrder.PurchaseOrderDate ?? DaoConst.SQL_DATETIME_MAX, purchaseOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (slipDateFrom == null || DateTime.Compare(a.SlipDate ?? DaoConst.SQL_DATETIME_MIN, slipDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (slipDateTo == null || DateTime.Compare(a.SlipDate ?? DaoConst.SQL_DATETIME_MAX, slipDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (purchaseDateFrom == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, purchaseDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (purchaseDateTo == null || DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, purchaseDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(makerName) || a.SalesCar.CarGrade.Car.Brand.Maker.MakerName.Contains(makerName))
                    && (string.IsNullOrEmpty(carBrandName) || a.SalesCar.CarGrade.Car.Brand.CarBrandName.Contains(carBrandName))
                    && (string.IsNullOrEmpty(carName) || a.SalesCar.CarGrade.Car.CarName.Contains(carName))
                    && (string.IsNullOrEmpty(carGradeName) || a.SalesCar.CarGrade.CarGradeName.Contains(carGradeName))
                    && (string.IsNullOrEmpty(vin) || a.SalesCar.Vin.Contains(vin))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    orderby a.LastUpdateDate descending
                    select a;
            // 出口
            return carPurchaseList.ToList();
        }
    }
}
