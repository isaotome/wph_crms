using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao
{

    /// <summary>
    ///   車両在庫棚卸テーブルアクセスクラス
    ///   車両在庫棚卸テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    /// </history>
    public class InventoryStockCarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public InventoryStockCarDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 棚卸在庫を取得(PK指定)
        /// </summary>
        /// <param name="inventoryId">棚卸ID</param>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public InventoryStockCar GetByKey(Guid ? inventoryId)
        {
            var query =
                (from a in db.InventoryStockCar
                 where a.InventoryId.Equals(inventoryId)
                 && (a.DelFlag != null && !a.DelFlag.Equals("1"))
                 select a);

            return query.FirstOrDefault();
        }


        /// <summary>
        /// 棚卸在庫を取得する
        /// </summary>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <returns>棚卸データ</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public List<InventoryStockCar> GetListByInventoryMonthWarehouse(DateTime inventoryMonth, string warehouseCode)
        {
            var query =
                (from a in db.InventoryStockCar
                 where a.InventoryMonth.Date.Equals(inventoryMonth.Date)
                 && a.WarehouseCode.Equals(warehouseCode)
                 && (a.DelFlag != null && !a.DelFlag.Equals("1"))
                 orderby a.LocationCode, a.SalesCarNumber
                 select a);

            return query.ToList();
        }

        /// <summary>
        /// 車台番号より棚卸在庫を取得する
        /// </summary>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="locationCode">ロケーションコード</param>
        /// <param name="vin">車台番号</param>
        /// <returns>棚卸データ</returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public InventoryStockCar GetByLocVin(DateTime inventoryMonth, string locationCode, string vin)
        {
            var query =
                (from a in db.InventoryStockCar
                 where a.InventoryMonth.Date.Equals(inventoryMonth.Date)
                 && a.LocationCode.Equals(locationCode)
                 && a.Vin.Equals(vin)
                 && (a.DelFlag != null && !a.DelFlag.Equals("1"))
                 select a);

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 在庫検索
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public PaginatedList<CarInventory> GetListByCondition(CarInventory condition, int pageIndex, int pageSize)
        {
            return new PaginatedList<CarInventory>(GetQueryable(condition), pageIndex, pageSize);
        }
      
        /// <summary>
        /// 在庫検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public IQueryable<CarInventory> GetListByCondition(CarInventory condition)
        {
            return GetQueryable(condition);
        }

        /// <summary>
        /// 在庫検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2017/12/15 arc yano #3839 車両在庫棚卸　デモカーの場合の新中区分の表示変更
        /// 2017/09/07 arc yano #3784 車両在庫棚卸 差異表示 倉庫コードを絞込み必須項目から除外
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private IQueryable<CarInventory> GetQueryable(CarInventory condition)
        {
            //-----------------
            //検索条件取得
            //-----------------
            string departmentCode = condition.DepartmentCode;
            DateTime inventoryMonth = condition.InventoryMonth;
            string locationCode = condition.LocationCode;
            string vin = condition.Vin;
            string salesCarNumber = condition.SalesCarNumber;
            string newUsedType = condition.NewUsedType;
            string carStatus = condition.CarStatus;
            string carBrandName = condition.CarBrandName;
            string carName = condition.CarName;
            string colorType = condition.ColorType;
            string exteriorColorCode = condition.ExteriorColorCode;
            string exteriorColorName = condition.ExteriorColorName;
            string registrationNumber = condition.RegistrationNumber;

            string warehouseCode = condition.WarehouseCode;

            bool inventoryDiff = condition.InventoryDiff;           //Add 2017/09/07 arc yano #3784


            List<CarInventory> list = new List<CarInventory>();

            //ベースクエリ(必須項目による絞り込み)
            var query =
                   (
                from a in db.InventoryStockCar
                join b in db.Location on a.LocationCode equals b.LocationCode
                join c in db.SalesCar on a.SalesCarNumber equals c.SalesCarNumber
                join d in db.c_NewUsedType on a.NewUsedType equals d.Code
                join e in db.V_CarMaster on c.CarGradeCode equals e.CarGradeCode
                join f in db.c_ColorCategory on c.ColorType equals f.Code into colortype
                from f in colortype.DefaultIfEmpty()
                join g in db.c_CodeName on a.CarUsage equals g.Code
                join h in db.c_CodeName on new { a = a.Comment, b = "022" } equals new { a = h.Code, b = h.CategoryCode } into comment
                from h in comment.DefaultIfEmpty()
                orderby a.LocationCode, g.DisplayOrder, a.Vin
                where 
               (string.IsNullOrWhiteSpace(warehouseCode) || a.WarehouseCode.Equals(warehouseCode))
                 && a.InventoryMonth.Equals(inventoryMonth)
                 && (a.DelFlag != null && !a.DelFlag.Equals("1"))
                 && g.CategoryCode.Equals("020")
                 && b.DelFlag.Equals("0")
                select new CarInventory()
                {
                    InventoryId = a.InventoryId                             //棚卸ID
                        ,
                    DepartmentCode = a.DepartmentCode                       //部門コード
                        ,
                    WarehouseCode = a.WarehouseCode                         //倉庫コード
                        ,
                    LocationCode = a.LocationCode                           //ロケーションコード
                        ,
                    LocationName = b.LocationName                           //ロケーション名
                        ,
                    SalesCarNumber = a.SalesCarNumber                       //管理番号
                        ,
                    Vin = a.Vin                                             //車台番号
                        ,
                    NewUsedType = a.NewUsedType                             //新中区分
                        ,
                    NewUsedTypeName = (a.CarUsage != null && (a.CarUsage.Equals("999") || a.CarUsage.Equals("006")) ? d.Name : "--")       //新中区分名  //Mod 2017/12/15 arc yano #3839
                        ,
                    CarStatus = a.CarUsage                                  //在庫区分
                        ,
                    CarStatusName = g.Name                                  //在庫区分名
                        ,
                    CarBrandName = e.CarBrandName                           //ブランド名
                        ,
                    CarName = e.CarName                                     //車種名
                        ,
                    ColorType = f.Name                                      //系統色
                        ,
                    ExteriorColorCode = c.ExteriorColorCode                 //外装色コード
                        ,
                    ExteriorColorName = c.ExteriorColorName                 //外装色名
                        ,
                    RegistrationNumber = c.MorterViecleOfficialCode + ' ' + c.RegistrationNumberType + ' ' + c.RegistrationNumberKana + ' ' + c.RegistrationNumberPlate     //登録番号
                        ,
                    MorterViecleOfficialCode = c.MorterViecleOfficialCode   //陸運局コード
                        ,
                    RegistrationNumberType = c.RegistrationNumberType       //種別
                        ,
                    RegistrationNumberKana = c.RegistrationNumberKana       //かな
                        ,
                    RegistrationNumberPlate = c.RegistrationNumberPlate     //プレート番号
                        ,
                    PhysicalQuantity = a.PhysicalQuantity                   //実棚
                        ,
                    Comment = a.Comment                                     //コメント
                        ,
                    CommentName = h.Name                                    //コメント文言
                        ,
                    Summary = a.Summary                                     //備考
                        ,
                    Quantity = a.Quantity                                   //理論数        //Add 2017/09/07 arc yano #3784
                }
             );
           
            //ロケーションコード
            if (!string.IsNullOrEmpty(locationCode))
            {
                query = query.Where(x => x.LocationCode.Contains(locationCode));
            }
            //管理番号
            if (!string.IsNullOrEmpty(salesCarNumber))
            {
                query = query.Where(x => x.SalesCarNumber.Equals(salesCarNumber));
            }
            //車台番号
            if (!string.IsNullOrEmpty(vin))
            {
                query = query.Where(x => x.Vin.Contains(vin));
            }
            //新中区分
            if (!string.IsNullOrEmpty(newUsedType))
            {
                query = query.Where(x => x.NewUsedType.Equals(newUsedType));
            }
            //在庫区分
            if (!string.IsNullOrEmpty(carStatus))
            {
                query = query.Where(x => x.CarStatus.Equals(carStatus));
            }
            //ブランド名
            if (!string.IsNullOrEmpty(carBrandName))
            {
                query = query.Where(x => x.CarBrandName.Contains(carBrandName));
            }
            //車種名
            if (!string.IsNullOrEmpty(carName))
            {
                query = query.Where(x => x.CarName.Contains(carName));
            }
            //系統色
            if (!string.IsNullOrEmpty(colorType))
            {
                query = query.Where(x => x.ColorType.Equals(colorType));
            }
            //車両カラーコード
            if (!string.IsNullOrEmpty(exteriorColorCode))
            {
                query = query.Where(x => x.ExteriorColorCode.Equals(exteriorColorCode));
            }
            //登録番号
            if (!string.IsNullOrEmpty(registrationNumber))
            {
                query = query.Where(x => x.RegistrationNumber.Equals(registrationNumber));
            }
            //差異のみ表示                        //Add 2017/09/07 arc yano #3784
            if (inventoryDiff.Equals(true))
            {
                query = query.Where(x => !x.PhysicalQuantity.Equals(x.Quantity));
            }

            var result = CommonUtils.SelectWithUpdlock(db, query);

            foreach (var a in result)
            {
                CarInventory rec = new CarInventory();

                rec.InventoryId = a.InventoryId;
                rec.DepartmentCode = a.DepartmentCode;
                rec.WarehouseCode = a.WarehouseCode;
                rec.LocationCode = a.LocationCode;
                rec.LocationName = a.LocationName;
                rec.SalesCarNumber = a.SalesCarNumber;
                rec.Vin = a.Vin;
                rec.NewUsedType = a.NewUsedType;
                rec.NewUsedTypeName = a.NewUsedTypeName;
                rec.CarStatus = a.CarStatus;
                rec.CarStatusName = a.CarStatusName;
                rec.CarBrandName = a.CarBrandName;
                rec.CarName = a.CarName;
                rec.ColorType = a.ColorType;
                rec.ExteriorColorCode = a.ExteriorColorCode;
                rec.ExteriorColorName = a.ExteriorColorName;
                rec.RegistrationNumber = a.RegistrationNumber;
                rec.MorterViecleOfficialCode = a.MorterViecleOfficialCode;
                rec.RegistrationNumberType = a.RegistrationNumberType;
                rec.RegistrationNumberKana = a.RegistrationNumberKana;
                rec.RegistrationNumberPlate = a.RegistrationNumberPlate;
                rec.PhysicalQuantity = a.PhysicalQuantity;
                rec.Comment = a.Comment;
                rec.CommentName = a.CommentName;
                rec.Summary = a.Summary;
                rec.Quantity = a.Quantity;                          //Add 2017/09/07 arc yano #3784
                list.Add(rec);
            }

            return list.AsQueryable();
        }

        /// <summary>
        /// 棚卸重複データ取得
        /// </summary>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        public List<CarInventory> GetListDuplicationData(DateTime inventoryMonth)
        {
            List<CarInventory> list = new List<CarInventory>();

            var query = db.GetDuplicationCarInventory(inventoryMonth);

            foreach (var a in query)
            {
                CarInventory rec = new CarInventory();

                rec.InventoryId = a.InventoryID;
                rec.DepartmentCode = a.DepartmentCode;
                rec.WarehouseCode = a.WarehouseCode;
                rec.LocationCode = a.LocationCode;
                rec.LocationName = a.LocationName;
                rec.SalesCarNumber = a.SalesCarNumber;
                rec.Vin = a.Vin;
                rec.NewUsedType = a.NewUsedType;
                rec.NewUsedTypeName = a.NewUsedTypeName;
                rec.CarStatus = a.CarUsage;
                rec.CarStatusName = a.CarStatusName;
                rec.CarBrandName = a.CarBrandName;
                rec.CarName = a.CarName;
                rec.ColorType = a.ColorType;
                rec.ExteriorColorCode = a.ExteriorColorCode;
                rec.ExteriorColorName = a.ExteriorColorName;
                rec.RegistrationNumber = a.RegistrationNumber;
                rec.MorterViecleOfficialCode = a.MorterViecleOfficialCode;
                rec.RegistrationNumberType = a.RegistrationNumberType;
                rec.RegistrationNumberKana = a.RegistrationNumberKana;
                rec.RegistrationNumberPlate = a.RegistrationNumberPlate;
                rec.PhysicalQuantity = a.PhysicalQuantity;
                rec.Comment = a.Comment;
                rec.CommentName = a.CommentName;
                rec.Summary = a.Summary;
                list.Add(rec);
            }

            return list;
        }
    }
}
