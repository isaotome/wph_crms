using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao
{

    /// <summary>
    ///   車両在庫テーブルアクセスクラス
    ///   車両在庫棚卸テーブルの各種検索メソッドを提供します。
    ///   更新系データ操作はコントローラに記述する為、提供しません。
    /// </summary>
    /// <history>
    /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
    /// </history>
    public class CarStockListDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarStockListDao(CrmsLinqDataContext context)
        {
            db = context;
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
        public PaginatedList<GetStockCarListResult> GetListByCondition(CarInventory condition, int pageIndex, int pageSize)
        {
            return new PaginatedList<GetStockCarListResult>(GetQueryable(condition), pageIndex, pageSize);
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
        public IQueryable<GetStockCarListResult> GetListByCondition(CarInventory condition)
        {
            return GetQueryable(condition);
        }

        /// <summary>
        /// 在庫検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2017/05/10 arc yano #3762 車両在庫棚卸機能追加 新規作成
        /// </history>
        private IQueryable<GetStockCarListResult> GetQueryable(CarInventory condition)
        {
            //-----------------
            //検索条件取得
            //-----------------
            string inventoryMonth = condition.strInventoryMonth;                                    //棚卸月

            string departmentCode = condition.DepartmentCode;                                       //部門コード
                        
            string locationCode = condition.LocationCode;                                           //ロケーションコード

            string saleCarNumber = condition.SalesCarNumber;                                        //管理番号

            string vin = condition.Vin;                                                             //車台番号

            string newUsedType = condition.NewUsedType;                                             //新中区分

            string carStatus = condition.CarStatus;                                                 //在庫区分

            string carBrandName = condition.CarBrandName;                                           //ブランド名

            string carName = condition.CarName;                                                     //車種名

            string carGradeName= condition.CarGradeName;                                            //グレード名

            string registrationNumber = condition.RegistrationNumber;                               //登録番号

            string stockFlag = condition.StockFlag;                                                 //在庫フラグ

            List<CarInventory> list = new List<CarInventory>();

            //ベースクエリ(必須項目による絞り込み)
            ISingleResult<GetStockCarListResult> ret = db.GetStockCarList(inventoryMonth, departmentCode, locationCode, saleCarNumber, vin, newUsedType, carStatus, carBrandName, carName, carGradeName, registrationNumber, stockFlag);

            return ret.ToList().AsQueryable();
        }
    }
}
