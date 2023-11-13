using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao {
    public class PartsStockDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public PartsStockDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 部品在庫検索
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<PartsStock> GetListByCondition(PartsStock condition,int pageIndex,int pageSize) {
            return new PaginatedList<PartsStock>(GetQueryable(condition), pageIndex, pageSize);
        }

        /// <summary>
        /// 部品在庫検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2019/02/12 yano #3997 部品移動検索】特定の入庫ロケーション、または出庫ロケーションの移動履歴が表示されない
        /// </history>
        public List<PartsStock> GetListByCondition(DocumentExportCondition condition){
            PartsStock partsStockCondition = new PartsStock();
            partsStockCondition.Location = new Location();

            //Mod 2019/02/12 yano #3997
            //部門コードから倉庫を取得
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, condition.DepartmentCode);

            if (dWarehouse != null)
            {
                partsStockCondition.Location.WarehouseCode = dWarehouse.WarehouseCode;
            }
            //partsStockCondition.Location.DepartmentCode = condition.DepartmentCode;
            
            partsStockCondition.Location.LocationType = condition.LocationType;
            partsStockCondition.LastUpdateDate = condition.LastUpdateDate;
            partsStockCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(partsStockCondition).ToList();
        }

        /// <summary>
        /// 部品在庫検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2019/02/12 yano #3997 部品移動検索】特定の入庫ロケーション、または出庫ロケーションの移動履歴が表示されない
        /// </history>
        private IQueryable<PartsStock> GetQueryable(PartsStock condition){
            string makerCode = condition.Parts != null && condition.Parts.Maker!=null ? condition.Parts.Maker.MakerCode : "" ;
            string makerName = condition.Parts != null && condition.Parts.Maker!= null ? condition.Parts.Maker.MakerName : "";
            string partsName = condition.Parts != null ? condition.Parts.PartsNameJp : "";
            string partsNumber = condition.Parts != null ? condition.Parts.PartsNumber : "";
            string brandCode = condition.Parts != null ? condition.Parts.CarBrandCode : "";
            string brandName = condition.Parts != null ? condition.Parts.CarBrandName : "";
            string locationType = null;
            try { locationType = condition.Location.LocationType; } catch (NullReferenceException) { }
            string departmentCode = null;
            try { departmentCode = condition.Location.DepartmentCode; } catch (NullReferenceException) { }
            string locationCode = null;
            try { locationCode = condition.Location.LocationCode; } catch (NullReferenceException) { }

            //Mod 2019/02/12 yano #3997
            string warehouseCode = null;
            try { warehouseCode = condition.Location.LocationCode; } catch (NullReferenceException) { }

            var query =
                from a in db.PartsStock
                orderby a.Location.DepartmentCode,a.LocationCode,a.PartsNumber
                where (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Contains(partsNumber))
                && (string.IsNullOrEmpty(partsName) || a.Parts.PartsNameJp.Contains(partsName))
                && (string.IsNullOrEmpty(makerCode) || a.Parts.Maker.MakerCode.Contains(makerCode))
                && (string.IsNullOrEmpty(makerName) || a.Parts.Maker.MakerName.Contains(makerName))
                && (string.IsNullOrEmpty(locationCode) || a.LocationCode.Equals(locationCode))
                && (string.IsNullOrEmpty(locationType) || a.Location.LocationType.Equals(locationType))
                && (string.IsNullOrEmpty(warehouseCode) || a.Location.WarehouseCode.Equals(warehouseCode))  //Mod 2019/02/12 yano #3997
                //&& (string.IsNullOrEmpty(departmentCode) || a.Location.DepartmentCode.Equals(departmentCode))
                && (condition.LastUpdateDate==null || DateTime.Compare(a.LastUpdateDate ?? DaoConst.SQL_DATETIME_MIN,condition.LastUpdateDate ?? DaoConst.SQL_DATETIME_MAX)<=0)
                //&& a.Quantity!=0
                && (string.IsNullOrEmpty(brandName) || 
                    (from b in db.Brand
                     where b.CarBrandName.Contains(brandName)
                     select b.MakerCode).Contains(a.Parts.MakerCode))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(PartsStock), "x");
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "Location", "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsStock, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Location", "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsStock, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Location", "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsStock, bool>>(comExpression, param));
            }
            return query;
        }

        /// <summary>
        /// 部品ロケーション在庫を取得する（PK指定）
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="locationCode">ロケーションコード</param>
        /// <returns>部品ロケーション在庫データ</returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除データも取得するかを引数で判断
        /// </history>
        public PartsStock GetByKey(string partsNumber, string locationCode, bool includeDeleted = false)
        {
            //Add 2015/03/23 arc iijima 無効データ検索対応 DelFlagの検索条件を追加
            var query =
                (from a in db.PartsStock
                where a.PartsNumber.Equals(partsNumber)
                && a.LocationCode.Equals(locationCode)
                && ((includeDeleted) || a.DelFlag.Equals("0"))
                select a).FirstOrDefault();
            return query;
        }


        /// <summary>
        /// 倉庫内に存在する部品在庫データを取得する
        /// (ロケーション種別が在庫のものだけ対象とする)
        /// </summary>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="includeDeleted">削除済データも含むフラグ</param>
        /// <param name="notRefine">ロケーションタイプの絞込みを行わないフラグ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） ロケーションタイプを絞るかどうかの引数を追加
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除データを含むかどうかの引数を追加
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 DelFlagによる絞り込み条件の追加
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数の変更(部門→倉庫) 新規作成
        /// </history>
        public List<PartsStock> GetListByWarehouse(string warehouseCode, string partsNumber, bool includeDeleted = false, bool notRefine = false) {
            var query =
                from a in db.PartsStock
                where (string.IsNullOrEmpty(warehouseCode) || a.Location.Warehouse.WarehouseCode.Equals(warehouseCode))
                && (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Equals(partsNumber))
                && ((notRefine) || a.Location.LocationType.Equals("001")) 
                && ( (includeDeleted) ||a.DelFlag.Equals("0"))
                orderby a.LocationCode
                select a;
            return query.ToList();
        }

        /*
        /// <summary>
        /// 部門内に存在する部品在庫データを取得する
        /// (ロケーション種別が在庫のものだけ対象とする)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 DelFlagによる絞り込み条件の追加
        /// </history>
        public List<PartsStock> GetListByDepartment(string departmentCode, string partsNumber)
        {
            var query =
                from a in db.PartsStock
                where (string.IsNullOrEmpty(departmentCode) || a.Location.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Equals(partsNumber))
                && a.Location.LocationType.Equals("001")
                && a.DelFlag.Equals("0")
                orderby a.LocationCode
                select a;
            return query.ToList();
        }
        */

        /*  //Del 2016/08/13 arc yano 未使用のため、コメントアウト
        /// <summary>
        /// 部門内に存在する部品在庫データを取得する
        /// (ロケーション種別が在庫のものだけ対象とする)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <returns></returns>
        public List<PartsStock> GetAllListByDepartment(string departmentCode, string partsNumber)
        {
            var query =
                from a in db.PartsStock
                where (string.IsNullOrEmpty(departmentCode) || a.Location.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Equals(partsNumber))
                orderby a.LocationCode
                select a;
            return query.ToList();
        }
        */


        /// <summary>
        /// 部品の部門在庫数を取得する
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns>在庫数量</returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応　バグ対応
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数を変更(departmentCode → warehouseCode)
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更 在庫数はフリーのみ(数量から引当済数を差し引いたもの)
        /// </history>
        public decimal GetStockQuantity(string partsNumber, string warehouseCode)
        {
            decimal ret = 0;

            var query =
               
                from a in db.PartsStock
                where a.Location.WarehouseCode.Equals(warehouseCode)    //2016/08/13 arc yano #3596
                && a.PartsNumber.Equals(partsNumber)
                && a.Location.LocationType.Equals("001")
                && a.DelFlag.Equals("0")                                //Add 2017/02/08 arc yano #3620
                select new { Quantity = (a.Quantity ?? 0) - (a.ProvisionQuantity ?? 0)};

            if (query.ToList().Count > 0)
            {
                ret = query.Select(x => x.Quantity).Sum();
            }

            return ret;
        }


        /// <summary>
        /// 部品在庫データを取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="LocationCode">対象年月</param>
        /// <returns></returns>
        /// <history>
        ///   2018/11/09 yano #3937 部品在庫検索　金額の計算方法の修正漏れ対応
        ///   2018/06/01 arc yano #3900 部品在庫検索　編集画面を表示すると同一の在庫情報が２行表示される
        ///   2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 移動平均単価を取得するように修正
        /// 　2016/08/13 arc yano #3596 【大項目】部門棚統合対応 データ取得用のメソッドを新規追加
        /// </history>
        public List<CommonPartsStock> GetListAllByCondition(PartsStock condition, DateTime TargetDate)
        {
            //検索条件の設定

            //部門コード
            string departmentCode = condition.DepartmentCode;
            //倉庫コード
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);

            string warehouseCode = dWarehouse != null ? dWarehouse.WarehouseCode : "";

            //ロケーションコード
            string locationCode = condition.Location.LocationCode;
            //ロケーション名
            string locationName = condition.Location.LocationName;
            //部品番号
            string partsNumber = condition.Parts.PartsNumber;
            //部品名
            string partsNameJp = condition.Parts.PartsNameJp;
            //在庫０表示するかどうか
            string stockZeroVisibility = condition.StockZeroVisibility;
            //ストアドを呼ぶ
            var PartsStockList = db.GetPartsStock(departmentCode, warehouseCode, locationCode, locationName, partsNumber, partsNameJp, stockZeroVisibility);

            /*
            var PartsStockList =
                from a in db.PartsStock
                join c in db.Parts on a.PartsNumber equals c.PartsNumber into cc
                from ccc in cc.DefaultIfEmpty()
                join d in db.Location on a.LocationCode equals d.LocationCode into dd
                from ddd in dd.DefaultIfEmpty()
                join e in db.Department on ddd.DepartmentCode equals e.DepartmentCode into ee
                from eee in ee.DefaultIfEmpty()
                where (string.IsNullOrEmpty(condition.Location.DepartmentCode) || a.Location.DepartmentCode.Equals(condition.Location.DeparktmentCode))
                && (string.IsNullOrEmpty(condition.LocationCode) || a.LocationCode.Contains(condition.Location.LocationCode))
                && (string.IsNullOrEmpty(condition.Location.LocationName) || ddd.LocationName.Contains(condition.Location.LocationName))
                && (string.IsNullOrEmpty(condition.Parts.PartsNumber) || a.PartsNumber.Contains(condition.Parts.PartsNumber))
                && (string.IsNullOrEmpty(condition.Parts.PartsNameJp) || ccc.PartsNameJp.Contains(condition.Parts.PartsNameJp))
                && (a.DelFlag.Equals("0"))
                && (ccc.DelFlag.Equals("0"))
                && (ddd.DelFlag.Equals("0"))
                orderby a.LocationCode, a.PartsNumber
                select new
                {
                    PhysicalQuantity = 0,
                    Comment = "",
                    eee.DepartmentName,
                    a.LocationCode,
                    ddd.LocationName,
                    ddd.LocationType,
                    a.PartsNumber,
                    ccc.PartsNameJp,
                    a.Quantity,
                    a.ProvisionQuantity,
                    SaveQuantity = 0,
                    ccc.Price,
                    ccc.Cost
                };
            */

            List<CommonPartsStock> list = new List<CommonPartsStock>();

            foreach (var PartsStock in PartsStockList)
            {
                CommonPartsStock CommonPartsStockList = new CommonPartsStock();
                CommonPartsStockList.PhysicalQuantity = 0;                                                                                               //実棚
                CommonPartsStockList.Comment = "";                                                                                                       //コメント
                CommonPartsStockList.DepartmentCode = PartsStock.DepartmentCode;                                                                         //部門コード         //Add 2018/06/01 arc yano #3900
                CommonPartsStockList.DepartmentName = PartsStock.DepartmentName;                                                                         //部門名称
                CommonPartsStockList.LocationCode = PartsStock.LocationCode;                                                                             //ロケーションコード
                CommonPartsStockList.LocationName = PartsStock.LocationName;                                                                             //ロケーション名称
                CommonPartsStockList.LocationType = PartsStock.LocationType;                                                                             //ロケーション種別   //Add 2016/02/01 arc yano #3409

                CommonPartsStockList.PartsNumber = PartsStock.PartsNumber;                                                                               //部品番号
                CommonPartsStockList.PartsNameJp = PartsStock.PartsNameJp;                                                                               //部品名(日本語)
                CommonPartsStockList.Quantity = PartsStock.Quantity;
                CommonPartsStockList.SaveQuantity = 0;                                                                                                   //数量(棚卸開始時の数量)

                CommonPartsStockList.ProvisionQuantity = PartsStock.ProvisionQuantity;                                                                   //引当済数

                CommonPartsStockList.MoveAverageUnitPrice = PartsStock.MovingAverageCost;                                                                 //移動平均単価              //Add 2018/05/14 arc yano #3880

                CommonPartsStockList.StandardPrice = PartsStock.Cost;                                                                                    //標準原価                   //Mod 2018/05/14 arc yano #3880

                CommonPartsStockList.Price = (PartsStock.MovingAverageCost != null ? PartsStock.MovingAverageCost : PartsStock.Cost) * (PartsStock.Quantity != null ? PartsStock.Quantity : 0);  //金額(移動平均短歌 × 数量)  //Mod 2018/11/09 yano #3937
                //CommonPartsStockList.Price = (PartsStock.Cost != null ? PartsStock.Cost : 0) * (PartsStock.Quantity != null ? PartsStock.Quantity : 0);  //金額(移動平均単価 * 数量)  //Mod 2018/05/14 arc yano #3880

                //リスト追加
                list.Add(CommonPartsStockList);
            }
            return list;
        }

        /// <summary>
        /// 削除済み在庫情報の初期化
        /// </summary>
        /// <param name="partsStock">在庫情報</param>
        /// <returns>在庫情報(有効化)</returns>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 新規作成
        /// </history>
        public PartsStock InitPartsStock(PartsStock partsstock)
        {

            //削除済データの場合は
            if (!string.IsNullOrWhiteSpace(partsstock.DelFlag) && partsstock.DelFlag.Equals("1"))
            {
                //在庫情報の数量・引当済数を初期化する
                partsstock.Quantity = 0;
                partsstock.ProvisionQuantity = 0;

                partsstock.DelFlag = "0";               //有効にする
            }

            return partsstock;
        }


    /// <summary>
    /// 対象のロケーションに部品在庫が存在するかのチェック
    /// </summary>
    /// <param name="LocationCode">ロケーションコード</param>
    /// <returns>在庫の有無(true…有、falese…無)</returns>
    /// <history>
    /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 新規作成
    /// </history>
    public bool getPresencePartsStock(string LocationCode)
    {

      bool ret = (db.PartsStock.Where(x => x.LocationCode.Equals(LocationCode) && x.Quantity > 0 && x.DelFlag.Equals("0")).ToList().Count > 0);

      return ret;
    }

    //Dell 2021/08/03 yano #4098 コメントアウトされていたメソッドを削除

    //Del 2016/08/13 arc yano #3596 不要のため、削除
  }
}
