using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao
{
    public class InventoryStockDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public InventoryStockDao(CrmsLinqDataContext context)
        {
            db = context;
        }


        /// <summary>
        /// 棚卸データを取得する(主キーによる取得)
        /// </summary>
        /// <param name="inventoryId">棚卸ID</param>
        /// <returns>部品在庫棚卸</returns>
        /// <history>
        /// 2017/07/18 arc yano #3779 部品在庫（部品在庫の修正） 新規作成
        /// </history>
        public InventoryStock GetByKey(Guid inventoryId)
        {
            var query =
                (from a in db.InventoryStock
                 where a.InventoryId.Equals(inventoryId)
                 select a);

            return query.FirstOrDefault();
        }



        /// <summary>
        /// 部品棚卸在庫を取得する
        /// </summary>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="warehouseCode">倉庫コード</param>
        /// <returns>部品ロケーション在庫データ</returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 部品棚卸の管理を部門単位から倉庫単位へ変更
        /// </history>
        public List<InventoryStock> GetListByInventoryMonthWarehouse(DateTime inventoryMonth, string warehouseCode)
        {
            var query =
                (from a in db.InventoryStock
                 where a.InventoryMonth.Date.Equals(inventoryMonth.Date)
                 && a.WarehouseCode.Equals(warehouseCode)
                 orderby a.LocationCode, a.PartsNumber
                 select a);

            return query.ToList();
        }
        /*
        public List<InventoryStock> GetListByInvnentoryMonthDepartment(DateTime inventoryMonth, string departmentCode)
        {
            var query =
                (from a in db.InventoryStock
                 where a.InventoryMonth.Date.Equals(inventoryMonth.Date)
                 && a.DepartmentCode.Equals(departmentCode)
                 orderby a.LocationCode, a.PartsNumber
                 select a);

            return query.ToList();
        }
        */
        /// <summary>
        /// 部品棚卸在庫を取得する
        /// </summary>
        /// <param name="inventoryMonth">棚卸月</param>
        /// <param name="locationCode">ロケーションコード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="includeDeleted">削除データを含むかどうか</param>
        /// <returns>部品ロケーション在庫データ</returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 削除フラグを参照するように変更
        /// </history>
        public InventoryStock GetByLocParts(DateTime inventoryMonth, string locationCode, string partsNumber, bool includeDeleted = false)
        {
            var query =
                (from a in db.InventoryStock
                 where a.InventoryMonth.Date.Equals(inventoryMonth.Date)
                 && a.LocationCode.Equals(locationCode)
                 && a.PartsNumber.Equals(partsNumber)
                 && ((includeDeleted) || a.DelFlag.Equals("0"))
                 select a);

            return query.FirstOrDefault();
        }

        /// <summary>
        /// 部品在庫検索
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<PartsInventory> GetListByCondition(InventoryStock condition, int pageIndex, int pageSize)
        {
            return new PaginatedList<PartsInventory>(GetQueryable(condition), pageIndex, pageSize);
        }
        //Mod 2015/06/09 arc yano  IPO対応(部品棚卸) 障害対応、仕様変更⑤ 曖昧検索対応
        //Add 2015/04/23 arc yano　IPO対応(部品棚卸) 不具合対応
        /// <summary>
        /// 部品在庫検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public IQueryable<PartsInventory> GetListByCondition(InventoryStock condition)
        {
            return GetQueryable(condition);
        }

        /// <summary>
        /// 部品在庫検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正） 引当済数も取得する
        /// 2017/07/26 arc yano #3781 部品在庫棚卸（棚卸在庫の修正）  棚卸IDも取得するように変更
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 棚卸の管理を部門単位から倉庫単位に変更
        /// 2015/05/27 arc yano IPO対応(部品棚卸) 障害対応、仕様変更
        ///                                       ロケーションタイプ -> ロケーションコード -> 部品番号の順に並び替え
        /// 2015/04/25 arc yano IPO対応(部品棚卸) 検索クエリの見直し
        /// </history>
        private IQueryable<PartsInventory> GetQueryable(InventoryStock condition)
        {
            string departmentCode = condition.DepartmentCode;
            DateTime inventoryMonth = condition.InventoryMonth;
            string locationCode = condition.LocationCode;
            string partsNumber = condition.PartsNumber;
            string inventoryType = condition.InventoryType;
            string locationName = condition.LocationName;
            string partsNameJp = condition.PartsNameJp;
            bool diffQuantity = condition.DiffQuantity;

            //Add 2016/08/13 arc yano #3596
            string warehouseCode = condition.WarehouseCode;

            List<PartsInventory> list = new List<PartsInventory>();

            //Mod 2016/08/13 arc yano #3596
            //ベースクエリ(必須項目による絞り込み)
            var query =
                   (
                from a in db.InventoryStock
                join b in db.Location on a.LocationCode equals b.LocationCode
                join c in db.Parts on a.PartsNumber equals c.PartsNumber
                join d in db.PartsBalance on new
                {
                    InventoryMonth = a.InventoryMonth
                    ,
                    WarehouseCode = a.WarehouseCode
                    ,
                    PartsNumber = a.PartsNumber
                } equals new
                {
                    InventoryMonth = d.CloseMonth
                    ,
                    WarehouseCode = d.WarehouseCode
                    ,
                    PartsNumber = d.PartsNumber
                } into dd
                from ddd in dd.DefaultIfEmpty()   //Mod 2015/05/27 arc yano 
                orderby b.LocationType, a.LocationCode, a.PartsNumber
                where a.WarehouseCode.Equals(warehouseCode)
                 && a.InventoryMonth.Equals(inventoryMonth)
                 && (a.DelFlag != null && a.DelFlag.Equals("0"))
                select new
                {
                    a.InventoryId            //Add 2017/07/26 arc yano #3781
                        ,
                    a.LocationCode
                        ,
                    b.LocationName
                        ,
                    a.PartsNumber
                        ,
                    c.PartsNameJp
                        ,
                    a.Quantity
                        ,
                    PostCost = ddd.PostCost != null ? ddd.PostCost : (c.SoPrice != null ? c.SoPrice : (c.Cost != null ? c.Cost : 0))                              //Mod 2015/07/15 arc yano
                        ,
                    CalcAmount = a.Quantity * (ddd.PostCost != null ? ddd.PostCost : (c.SoPrice != null ? c.SoPrice : (c.Cost != null ? c.Cost : 0)))          //Mod 2015/07/15 arc yano
                        ,
                    a.PhysicalQuantity
                        ,
                    a.Comment
                        ,
                    a.ProvisionQuantity     //Add 2017/07/26 arc yano #3781
                }
                   );
            /*
            //Mod 2015/07/10 arc yano IPO対応(部品棚卸)　障害対応、仕様変更⑦ 理論在庫の単価、金額を追加
            //Mod 2015/07/10 arc yano IPO対応(部品棚卸)　検索時の絞込み条件でDelFlagが抜けている不具合の対応
            //ベースクエリ(必須項目による絞り込み)
            var query =
                   (
                from a in db.InventoryStock
                        join b in db.Location on a.LocationCode equals b.LocationCode
                        join c in db.Parts on a.PartsNumber equals c.PartsNumber
                join d in db.PartsBalance on new
                { 
                      InventoryMonth = a.InventoryMonth
                    , DepartmentCode = a.DepartmentCode
                    , PartsNumber = a.PartsNumber 
                } equals new 
                { 
                      InventoryMonth = d.CloseMonth
                    , DepartmentCode = d.DepartmentCode
                    , PartsNumber = d.PartsNumber 
                } into dd from ddd in dd.DefaultIfEmpty()   //Mod 2015/05/27 arc yano 
                orderby b.LocationType, a.LocationCode, a.PartsNumber	
                        where a.DepartmentCode.Equals(departmentCode)
                && a.InventoryMonth.Equals(inventoryMonth)
            && (a.DelFlag != null && a.DelFlag.Equals("0"))
                        select new
                        {
                              	a.LocationCode
			                    ,
			                    b.LocationName
			                    ,
			                    a.PartsNumber
			                    ,
			                    c.PartsNameJp
			                    ,
			                    a.Quantity
			                    ,
			                    PostCost = ddd.PostCost != null ? ddd.PostCost : (c.SoPrice != null? c.SoPrice: (c.Cost != null ? c.Cost: 0))                              //Mod 2015/07/15 arc yano
			                    ,
			                    CalcAmount = a.Quantity * (ddd.PostCost != null ? ddd.PostCost : (c.SoPrice != null ? c.SoPrice : (c.Cost != null ? c.Cost : 0)))          //Mod 2015/07/15 arc yano
			                    ,
			                    a.PhysicalQuantity
			                    ,
			                    a.Comment
                        }
                   );
            */
            //Mod 2015/06/09
            //任意の検索条件
            //ロケーションコード
            if(!string.IsNullOrEmpty(locationCode))
            {
                query = query.Where(x => x.LocationCode.Contains(locationCode));
            }
            //ロケーション名
            if (!string.IsNullOrEmpty(locationName))
            {
                query = query.Where(x => x.LocationName.Contains(locationName));
            }
            //部品番号
            if (!string.IsNullOrEmpty(partsNumber))
            {
                query = query.Where(x => x.PartsNumber.Equals(partsNumber));
            }
            //部品番号
            if (!string.IsNullOrEmpty(partsNameJp))
            {
                query = query.Where(x => x.PartsNameJp.Contains(partsNameJp));
            }
            //棚差有無チェックあり
            if (diffQuantity == true)
            {
                query = query.Where(x => x.Quantity != x.PhysicalQuantity);
            }

            foreach (var a in query)
            {
                PartsInventory rec = new PartsInventory();
                rec.LocationCode = a.LocationCode;
                rec.LocationName = a.LocationName;
                rec.PartsNumber  = a.PartsNumber;
                rec.PartsNameJp = a.PartsNameJp;
                rec.Quantity = a.Quantity;
                rec.PhysicalQuantity = a.PhysicalQuantity;
                rec.Comment = a.Comment;
                rec.PostCost = a.PostCost;          //2015/07/15 arc yano
                rec.CalcAmount = a.CalcAmount;      //2015/07/15 arc yano
                rec.InventoryId = a.InventoryId;    // Add 2017/07/26 arc yano #3781

                rec.ProvisionQuantity = a.ProvisionQuantity; // Add 2017/07/26 arc yano #3781
                
                list.Add(rec);
            }


            return list.AsQueryable();
        }


        /// <summary>
        /// 部品在庫データを取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="LocationCode">ロケーションコード</param>
        /// <param name="PartsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        /// 2018/11/09 yano #3937 部品在庫検索　金額の計算方法の修正漏れ対応
        /// 2018/06/01 arc yano #3900 部品在庫検索　編集画面を表示すると同一の在庫情報が２行表示される
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 データ取得用のメソッドを新規追加
        /// </history>
        public List<CommonPartsStock> GetListAllByCondition(CommonPartsStockSearch condition)
        {
            /*
            var InventoryStockList =
                from a in db.InventoryStock
                join b in db.Parts on a.PartsNumber equals b.PartsNumber into bb from bbb in bb.DefaultIfEmpty()
                join c in db.PartsAverageCost on new { ps = a.PartsNumber, dt = a.InventoryMonth } equals new { ps = c.PartsNumber, dt = c.CloseMonth } into cc from ccc in cc.DefaultIfEmpty()
                join d in db.Location on a.LocationCode equals d.LocationCode into dd from ddd in dd.DefaultIfEmpty()
                join e in db.Department on a.DepartmentCode equals e.DepartmentCode
                where (string.IsNullOrEmpty(CommonPartsStockSearchCondition.DepartmentCode) || a.DepartmentCode.Equals(CommonPartsStockSearchCondition.DepartmentCode))
                && (a.InventoryMonth.Date.Equals(CommonPartsStockSearchCondition.TargetDate))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationCode) || a.LocationCode.Contains(CommonPartsStockSearchCondition.LocationCode))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationName) || ddd.LocationName.Contains(CommonPartsStockSearchCondition.LocationName))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNumber) || a.PartsNumber.Contains(CommonPartsStockSearchCondition.PartsNumber))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNameJp) || bbb.PartsNameJp.Contains(CommonPartsStockSearchCondition.PartsNameJp))
                && (a.InventoryType.Equals("002"))
                && (a.DelFlag.Equals("0"))
                orderby a.LocationCode, a.PartsNumber
                select new
                {
                    a.PhysicalQuantity,
                    a.Comment,
                    e.DepartmentName,
                    a.LocationCode,
                    ddd.LocationName,
                    ddd.LocationType,           //Addd 2016/02/01 arc yano #3409
                    a.PartsNumber,
                    bbb.PartsNameJp,
                    ccc.Price,
                    a.Quantity,
                    a.ProvisionQuantity,        //Addd 2016/02/01 arc yano #3409
                    StandardPrice = bbb.Cost    //Mod 2015/03/20 arc iijima
                    //StandardPrice = bbb.Price
                };

             */

            //部門コード
            string departmentCode = condition.DepartmentCode;
            //対象年月
            DateTime? targetDate = condition.TargetDate;
            //倉庫コード
            DepartmentWarehouse dWarehouse = CommonUtils.GetWarehouseFromDepartment(db, departmentCode);
            
            string warehouseCode = dWarehouse != null ? dWarehouse.WarehouseCode : "";

            //ロケーションコード
            string locationCode = condition.LocationCode;
            //ロケーション名
            string locationName = condition.LocationName;
            //部品番号
            string partsNumber = condition.PartsNumber;
            //部品名
            string partsNameJp = condition.PartsNameJp;
            //在庫０表示するかどうか
            string stockZeroVisibility = condition.StockZeroVisibility;
            //ストアド呼び出し
            var InventoryStockList = db.GetInventoryStock(departmentCode, warehouseCode, targetDate, locationCode, locationName, partsNumber, partsNameJp, stockZeroVisibility);
            
            //一覧
            List<CommonPartsStock> list = new List<CommonPartsStock>();
            //全部門のステータスを取得
            InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetByKey(string.Format("{0:yyyyMMdd}", condition.TargetDate)); //対象年月の棚卸データ取得(月１件)
            
            string status = "";

            if(rec != null){
                status = rec.InventoryStatus;    
            }

            foreach (var InventoryStock in InventoryStockList)
            {
                CommonPartsStock CommonPartsStockList = new CommonPartsStock();
                CommonPartsStockList.PhysicalQuantity = InventoryStock.PhysicalQuantity;     //実棚
                CommonPartsStockList.Comment = InventoryStock.Comment;                       //コメント
                CommonPartsStockList.DepartmentCode = InventoryStock.DepartmentCode;         //部門コード    //Add 2018/06/01 arc yano #3900
                CommonPartsStockList.DepartmentName = InventoryStock.DepartmentName;         //部門名称
                CommonPartsStockList.LocationCode = InventoryStock.LocationCode;             //ロケーションコード
                CommonPartsStockList.LocationName = InventoryStock.LocationName;             //ロケーション名称
                CommonPartsStockList.LocationType = InventoryStock.LocationType;             //ロケーション種別
                CommonPartsStockList.PartsNumber = InventoryStock.PartsNumber;               //部品番号
                CommonPartsStockList.PartsNameJp = InventoryStock.PartsNameJp;               //部品名(日本語)
                CommonPartsStockList.Quantity = InventoryStock.Quantity;                     //数量     

                CommonPartsStockList.ProvisionQuantity = InventoryStock.ProvisionQuantity;   //引当済数
               
                CommonPartsStockList.StandardPrice = InventoryStock.StandardPrice;           //標準原価

                CommonPartsStockList.MoveAverageUnitPrice = InventoryStock.AverageCost;      //移動平均原価
                if (InventoryStock.AverageCost != null)
                {
                    CommonPartsStockList.Price = InventoryStock.AverageCost * (InventoryStock.PhysicalQuantity ?? 0);    //金額(移動平均原価 * 実棚数)
                }
                else if (InventoryStock.StandardPrice != null)
                {
                    CommonPartsStockList.Price = InventoryStock.StandardPrice * InventoryStock.Quantity; ; //金額(標準原価 * 数量)
                }
                else
                {
                    CommonPartsStockList.Price = null;
                }
                
                //リスト追加
                list.Add(CommonPartsStockList);
            }

            return list;
        }

        /*
        /// <summary>
        /// 部品在庫データを取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="LocationCode">ロケーションコード</param>
        /// <param name="PartsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        /// Mod 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 金額は実棚数×移動平均原価にする
        /// Mod 2016/02/01 arc yano #3409 部品在庫検索 フリー在庫表示対応 引当済数、ロケーションタイプの追加
        /// Mod 2015/03/20 arc iijima 標準原価を定価から原価へ変更
        /// Mod 2015/03/10 arc yano #3160(部門コードを任意入力に変更)
        /// Mod 2015/03/03 arc iijima Parts,PartsAverageCost,Locationをleft inner join に変更
        /// Mod 2015/02/22 arc iijima 部門名称表示対応
        /// Mod 2015/02/12 arc yano 棚卸確定時も、標準原価を設定するように修正
        /// Add 2014/12/04 arc nakayama 新規部品在庫検索対応
        /// </history>
        public List<CommonPartsStock> GetListByDepartmentAll(CommonPartsStockSearch CommonPartsStockSearchCondition)
        {
            var InventoryStockList =
                from a in db.InventoryStock
                join b in db.Parts on a.PartsNumber equals b.PartsNumber into bb from bbb in bb.DefaultIfEmpty()
                join c in db.PartsAverageCost on new { ps = a.PartsNumber, dt = a.InventoryMonth } equals new { ps = c.PartsNumber, dt = c.CloseMonth } into cc from ccc in cc.DefaultIfEmpty()
                join d in db.Location on a.LocationCode equals d.LocationCode into dd from ddd in dd.DefaultIfEmpty()
                join e in db.Department on a.DepartmentCode equals e.DepartmentCode
                where (string.IsNullOrEmpty(CommonPartsStockSearchCondition.DepartmentCode) || a.DepartmentCode.Equals(CommonPartsStockSearchCondition.DepartmentCode))
                && (a.InventoryMonth.Date.Equals(CommonPartsStockSearchCondition.TargetDate))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationCode) || a.LocationCode.Contains(CommonPartsStockSearchCondition.LocationCode))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationName) || ddd.LocationName.Contains(CommonPartsStockSearchCondition.LocationName))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNumber) || a.PartsNumber.Contains(CommonPartsStockSearchCondition.PartsNumber))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNameJp) || bbb.PartsNameJp.Contains(CommonPartsStockSearchCondition.PartsNameJp))
                && (a.InventoryType.Equals("002"))
                && (a.DelFlag.Equals("0"))
                orderby a.LocationCode, a.PartsNumber
                select new
                {
                    a.PhysicalQuantity,
                    a.Comment,
                    e.DepartmentName,
                    a.LocationCode,
                    ddd.LocationName,
                    ddd.LocationType,           //Addd 2016/02/01 arc yano #3409
                    a.PartsNumber,
                    bbb.PartsNameJp,
                    ccc.Price,
                    a.Quantity,
                    a.ProvisionQuantity,        //Addd 2016/02/01 arc yano #3409
                    StandardPrice = bbb.Cost    //Mod 2015/03/20 arc iijima
                    //StandardPrice = bbb.Price
                };

            List<CommonPartsStock> list = new List<CommonPartsStock>();

            //全部門のステータスを取得
            InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetByKey(string.Format("{0:yyyyMMdd}", CommonPartsStockSearchCondition.TargetDate)); //対象年月の棚卸データ取得(月１件)
            string status = "";

            if(rec != null){
                status = rec.InventoryStatus;    
            }

            foreach (var InventoryStock in InventoryStockList)
            {
                CommonPartsStock CommonPartsStockList = new CommonPartsStock();
                CommonPartsStockList.PhysicalQuantity = InventoryStock.PhysicalQuantity;     //実棚
                CommonPartsStockList.Comment = InventoryStock.Comment;                       //コメント
                CommonPartsStockList.DepartmentName = InventoryStock.DepartmentName;         //部門名称
                CommonPartsStockList.LocationCode = InventoryStock.LocationCode;             //ロケーションコード
                CommonPartsStockList.LocationName = InventoryStock.LocationName;             //ロケーション名称
                CommonPartsStockList.LocationType = InventoryStock.LocationType;             //ロケーション種別   //Add 2016/02/01 arc yano #3409
                CommonPartsStockList.PartsNumber = InventoryStock.PartsNumber;               //部品番号
                CommonPartsStockList.PartsNameJp = InventoryStock.PartsNameJp;               //部品名(日本語)
                CommonPartsStockList.Quantity = InventoryStock.Quantity;                     //数量     

                CommonPartsStockList.ProvisionQuantity = InventoryStock.ProvisionQuantity;   //引当済数     //Mod 2016/02/01 arc yano
               
                CommonPartsStockList.StandardPrice = InventoryStock.StandardPrice;           //標準原価     //Mod 2015/02/12 arc yano

                CommonPartsStockList.MoveAverageUnitPrice = InventoryStock.Price;            //移動平均原価
                if (InventoryStock.Price != null)
                {
                    CommonPartsStockList.Price = InventoryStock.Price * (InventoryStock.PhysicalQuantity ?? 0);    //金額(移動平均原価 * 実棚数)   //Mod 2016/07/05 arc yano #3598
                }
                else if (InventoryStock.StandardPrice != null)
                {
                    CommonPartsStockList.Price = InventoryStock.StandardPrice * InventoryStock.Quantity; ; //金額(標準原価 * 数量)
                }
                else
                {
                    CommonPartsStockList.Price = null;
                }
                
                //CommonPartsStockList.InventoryStatus = "002";                                //全体の棚卸ステータス(確定(002)固定)

                //リスト追加
                list.Add(CommonPartsStockList);
            }

            //return new PaginatedList<CommonPartsStock>(list.AsQueryable<CommonPartsStock>(), pageIndex ?? 0, pageSize ?? 0);
            return list;
        }

        /// <summary>
        /// 部門内に存在する部品在庫データを取得する(在庫ゼロ対象外)
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <param name="LocationCode">ロケーションコード</param>
        /// <param name="PartsNumber">部品番号</param>
        /// <returns></returns>
        /// <history>
        ///  Mod 2016/07/05 arc yano #3598 部品在庫検索　Excel出力機能追加 金額は実棚数×移動平均原価にする
        ///  Mod 2016/02/01 arc yano #3409 部品在庫検索 フリー在庫表示対応 引当済数、ロケーションタイプの追加
        ///  Mod 2015/03/20 arc iijima 標準原価を定価から原価へ変更
        ///  Mod 2015/03/10 arc yano #3160(部門コードを任意入力に変更)
        ///  Mod 2015/03/03 arc iijima Parts,PartsAverageCost,Locationをleft inner join に変更
        ///  Mod 2015/02/19 arc iijima 部門名称表示対応
        ///  Mod 2015/02/12 arc yano 棚卸確定時も、標準原価を設定するように修正
        ///  Mod 2015/02/12 arc yano 部品在庫検索対応 条件文に、数量(Quantity)がnullでない場合も追加
        ///  Add 2014/12/04 arc nakayama 新規部品在庫検索対応
        /// </history>
        public List<CommonPartsStock> GetListByDepartmentAllNotQuantityZero(CommonPartsStockSearch CommonPartsStockSearchCondition)
        {
            var InventoryStockList =
                from a in db.InventoryStock
                join b in db.Parts on a.PartsNumber equals b.PartsNumber into bb from bbb in bb.DefaultIfEmpty()
                join c in db.PartsAverageCost on new { ps = a.PartsNumber, dt = a.InventoryMonth } equals new { ps = c.PartsNumber, dt = c.CloseMonth } into cc from ccc in cc.DefaultIfEmpty()
                join d in db.Location on a.LocationCode equals d.LocationCode into dd from ddd in dd.DefaultIfEmpty()
                join e in db.Department on a.DepartmentCode equals e.DepartmentCode
                where (string.IsNullOrEmpty(CommonPartsStockSearchCondition.DepartmentCode) || a.DepartmentCode.Equals(CommonPartsStockSearchCondition.DepartmentCode))
                && (a.InventoryMonth.Date.Equals(CommonPartsStockSearchCondition.TargetDate))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationCode) || a.LocationCode.Contains(CommonPartsStockSearchCondition.LocationCode))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.LocationName) || ddd.LocationName.Contains(CommonPartsStockSearchCondition.LocationName))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNumber) || a.PartsNumber.Contains(CommonPartsStockSearchCondition.PartsNumber))
                && (string.IsNullOrEmpty(CommonPartsStockSearchCondition.PartsNameJp) || bbb.PartsNameJp.Contains(CommonPartsStockSearchCondition.PartsNameJp))
                //&& ((a.Quantity != null) && (a.Quantity != 0))
                && ((a.PhysicalQuantity != null) && (a.PhysicalQuantity != 0))  //Mod 2016/07/05 arc yano #3598
                && (a.InventoryType.Equals("002"))
                && (a.DelFlag.Equals("0"))
                orderby a.LocationCode, a.PartsNumber
                select new
                {
                    a.PhysicalQuantity,
                    a.Comment,
                    e.DepartmentName,
                    a.LocationCode,
                    ddd.LocationName,
                    ddd.LocationType,           //Add 2016/02/01 arc yano #3409
                    a.PartsNumber,
                    bbb.PartsNameJp,
                    a.Quantity,
                    a.ProvisionQuantity,        //Addd 2016/02/01 arc yano #3409
                    ccc.Price,
                    StandardPrice = bbb.Cost    //Mod 2015/03/20 arc iijima
                    //StandardPrice = bbb.Price
                };

            List<CommonPartsStock> list = new List<CommonPartsStock>();

             //全部門のステータスを取得
            InventoryMonthControlParts rec = new InventoryMonthControlPartsDao(db).GetByKey(string.Format("{0:yyyyMMdd}", CommonPartsStockSearchCondition.TargetDate)); //対象年月の棚卸データ取得(月１件)
            string status = "";

            if(rec != null){
                status = rec.InventoryStatus;    
            }

            foreach (var InventoryStock in InventoryStockList)
            {
                CommonPartsStock CommonPartsStockList = new CommonPartsStock();
                CommonPartsStockList.PhysicalQuantity = InventoryStock.PhysicalQuantity;                    //実棚
                CommonPartsStockList.Comment = InventoryStock.Comment;                                      //コメント
                CommonPartsStockList.DepartmentName = InventoryStock.DepartmentName;                        //部門名
                CommonPartsStockList.LocationCode = InventoryStock.LocationCode;                            //ロケーションコード
                CommonPartsStockList.LocationName = InventoryStock.LocationName;                            //ロケーション名称
                CommonPartsStockList.LocationType = InventoryStock.LocationType;                            //ロケーション種別   //Add 2016/02/01 arc yano #3409
                CommonPartsStockList.PartsNumber = InventoryStock.PartsNumber;                              //部品番号
                CommonPartsStockList.PartsNameJp = InventoryStock.PartsNameJp;                              //部品名(日本語)
                CommonPartsStockList.Quantity = InventoryStock.Quantity;                                    //数量
                CommonPartsStockList.ProvisionQuantity = InventoryStock.ProvisionQuantity;                  //引当済数     //Mod 2016/02/01 arc yano

                CommonPartsStockList.StandardPrice = InventoryStock.StandardPrice;                          //標準原価     //Mod 2015/02/12 arc yano

                CommonPartsStockList.MoveAverageUnitPrice = InventoryStock.Price;                           //移動平均原価
                if(InventoryStock.Price != null){
                    CommonPartsStockList.Price = InventoryStock.Price * (InventoryStock.PhysicalQuantity ?? 0);    //金額(移動平均原価 * 実棚数)   //Mod 2016/07/05 arc yano #3598
                }
                else if (InventoryStock.StandardPrice != null)
                {
                    CommonPartsStockList.Price = InventoryStock.StandardPrice * InventoryStock.Quantity;    //金額(標準原価 * 数量)
                }
                else
                {
                    CommonPartsStockList.Price = null;
                }

                //CommonPartsStockList.InventoryStatus = "002";                                //全体の棚卸ステータス(確定(002)固定)

                //リスト追加
                list.Add(CommonPartsStockList);
            }

            return list;
        }
        */
    }
}
