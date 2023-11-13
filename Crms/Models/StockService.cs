/*
 * 在庫処理に関するサービスを提供する
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CrmsDao;

namespace Crms.Models {

    /// <summary>
    /// インターフェース
    /// </summary>
    public interface IStockService {
        /// <summary>
        /// デフォルトロケーションを取得する
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(キー項目を部門コード→倉庫コードに変更)
        /// </history>
        Location GetDefaultLocation(string partsNumber, string warehouseCode);
        /// <summary>
        /// //引当ロケーション(ロケーション種別:002)を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns></returns>
        Location GetHikiateLocation(string departmentCode);
        /// <summary>
        /// 仕掛ロケーション(ロケーション種別:003)を取得する
        /// </summary>
        /// <param name="departmentCode">部門コード</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数の変更(部門→倉庫)
        /// </history>
        Location GetShikakariLocation(string warehouseCode);

        void UpdatePartsStock(List<ServiceSalesLine> lines, string shikakariLocationCode);

        //Add 2015/03/18 arc nakayama 伝票修正対応　伝票修正時の在庫更新処理
        /// <summary>
        /// 伝票修正時の在庫更新処理
        /// </summary>
        /// <param name="lines">サービス明細</param>
        /// <param name="shikakariLocationCode">仕掛ロケーションコード</param>
        void UpdatePartsStockForModification(List<ServiceSalesLine> lines, ServiceSalesHeader header);
    }

    /// <summary>
    /// 実装クラス
    /// </summary>
    public class StockService : IStockService {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public StockService(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 定数
        /// </summary>
        /// <history>
        /// Add 2016/06/13 arc yano #3571 定数追加
        /// </history>
        private const string TTYPE_WORK             = "003";      //移動種別 =「仕掛」                                    
        private const string TTYPE_CANCELWORK       = "007";      //移動種別 =「仕掛取消」                                
        private const string TTYPE_PROVISION        = "006";      //移動種別 =「自動引当」
        private const string TTYPE_CANCELPROVISION  = "008";      //移動種別 =「引当解除」


        /// <summary>
        /// 部品のデフォルトロケーションを取得する
        /// </summary>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="departmentCode">部門コード</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫リストの取得方法の変更(キー項目を部門コード→倉庫コードに変更)
        /// </history>
        public Location GetDefaultLocation(string partsNumber, string warehouseCode)
        {
            if (string.IsNullOrEmpty(partsNumber) || string.IsNullOrEmpty(warehouseCode))
            {
                return null;
            }

            PartsLocation location = new PartsLocationDao(db).GetByKey(partsNumber, warehouseCode); //Mod 2016/08/13 arc yano #3596
            if (location != null)
            {
                return location.Location;
            }

            // 仕掛・引当ロケーションも含めて在庫の更新履歴からロケーションを取得する
            List<PartsStock> stockList = new PartsStockDao(db).GetListByWarehouse(warehouseCode, partsNumber);      //Mod  2016/08/13 arc yano #3596
            if (stockList != null && stockList.Count() > 0)
            {
                PartsStock stock = stockList.OrderByDescending(x => x.LastUpdateDate).FirstOrDefault();
                if (stock != null)
                {
                    return stock.Location;
                }
            }
            return null;
        }
        /*
        public Location GetDefaultLocation(string partsNumber, string departmentCode) {
            if (string.IsNullOrEmpty(partsNumber) || string.IsNullOrEmpty(departmentCode)) {
                return null;
            }
            PartsLocation location = new PartsLocationDao(db).GetByKey(partsNumber, departmentCode);
            if (location != null) {
                return location.Location;
            }

            // 仕掛・引当ロケーションも含めて在庫の更新履歴からロケーションを取得する
            List<PartsStock> stockList = new PartsStockDao(db).GetListByDepartment(departmentCode, partsNumber);
            if (stockList != null && stockList.Count() > 0) {
                PartsStock stock = stockList.OrderByDescending(x => x.LastUpdateDate).FirstOrDefault();
                if (stock != null) {
                    return stock.Location;
                }
            }
            return null;
        }
        */
        /// <summary>
        /// //引当ロケーション(ロケーション種別:002)を取得する
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <returns></returns>
        public Location GetHikiateLocation(string departmentCode) {
            List<Location> hikiateLocation = new LocationDao(db).GetListByLocationType("002", departmentCode, null);
            return hikiateLocation.FirstOrDefault();
        }
        /// <summary>
        /// 仕掛ロケーション(ロケーション種別:003)を取得する
        /// </summary>
        /// <param name="header">サービス伝票データ</param>
        /// <returns></returns>
        /// <history>
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 引数の変更(部門→倉庫)
        /// </history>
        public Location GetShikakariLocation(string warehouseCode) {

            List<Location> shikakariLocation = new LocationDao(db).GetListByLocationType("003", warehouseCode, null);
            //List<Location> shikakariLocation = new LocationDao(db).GetListByLocationType("003", departmentCode, null);
            return shikakariLocation.FirstOrDefault();
        }


        //Mod 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数の更新を行う
        /// <summary>
        /// 納車処理時の在庫更新処理
        /// </summary>
        /// <param name="lines">サービス明細</param>
        /// <param name="shikakariLocationCode">仕掛ロケーションコード</param>
        /// <history>
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 削除済データも取得する
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数の更新を行う
        /// </history>
        public void UpdatePartsStock(List<ServiceSalesLine> lines, string shikakariLocationCode) {
            PartsStockDao stockDao = new PartsStockDao(db);
            var sumLine = from a in lines
                          where !string.IsNullOrEmpty(a.PartsNumber)
                          group a by a.PartsNumber into parts
                          select new { PartsNumber = parts.Key, ProvisionQuantity = parts.Sum(x => x.ProvisionQuantity) }; //Mod 2015/10/28 arc yano #3289

            foreach (var l in sumLine) {
                if (!string.IsNullOrEmpty(l.PartsNumber)) {

                    //Mod 2017/02/08 arc yano #3620
                    //削除済データも取得する
                    PartsStock stock = stockDao.GetByKey(l.PartsNumber, shikakariLocationCode, true);
                    if (stock != null) {

                        //削除データの場合は初期化
                        stock = new PartsStockDao(db).InitPartsStock(stock);	//Add 016/06/13 arc yano #3571                       

                        stock.Quantity -= l.ProvisionQuantity;                  //Mod 2015/10/28 arc yano #3289
                        stock.ProvisionQuantity -= l.ProvisionQuantity;         //Mod 2015/10/28 arc yano #3289
                        stock.LastUpdateDate = DateTime.Now;
                        stock.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                    } else {
                        //Mod 2015/03/17 arc nakayama 作業開始のタイミングでマスタの有無チェックを行う(#3171)
                        //マスタに存在していれば新規挿入し、在庫を更新する
                        Parts PartsData = new PartsDao(db).GetByKey(l.PartsNumber);
                        if (PartsData != null)
                        {
                            //仕掛ロケーションのレコードが存在しなかったら新規挿入する(マイナス在庫発生)
                            stock = new PartsStock();
                            stock.CreateDate = DateTime.Now;
                            stock.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                            stock.DelFlag = "0";
                            stock.LastUpdateDate = DateTime.Now;
                            stock.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                            stock.LocationCode = shikakariLocationCode;
                            stock.PartsNumber = l.PartsNumber;
                            stock.Quantity = l.ProvisionQuantity * (-1);             //Mod 2015/10/28 arc yano #3289
                            stock.ProvisionQuantity = l.ProvisionQuantity * (-1);    //Mod 2015/10/28 arc yano #3289
                            db.PartsStock.InsertOnSubmit(stock);
                        }
                        else
                        {
                            //何もしない。伝票はそのままにするが、在庫は更新しない(#3171  第１段対応：伝票は作成出来るように対応するが、在庫として反映させないようにする。)
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 伝票修正時の在庫更新処理
        /// </summary>
        /// <param name="lines">サービス伝票明細</param>
        /// <param name="header">サービス伝票ヘッダ</param>
        /// <history>
        /// 2018/05/14 arc yano #3880 売上原価計算及び棚卸評価法の変更 移動平均単価の計算
        /// 2017/02/08 arc yano #3620 サービス伝票入力　伝票保存、削除、赤伝等の部品の在庫の戻し対応 戻し先の設定
        /// 2017/01/31 arc yano #3566 サービス伝票入力　部門変更時の在庫の再引当 伝票修正時は在庫の戻し先はDBに登録されている部門の在庫へ戻すように修正
        /// 2016/08/13 arc yano #3596 【大項目】部門棚統合対応 在庫棚の取得方法の変更(キー項目を部門→倉庫に変更)
        /// 2016/06/13 arc yano #3571 部品在庫棚卸確定時の不具合対応 伝票修正時の在庫の戻しの際に移動伝票を作成する
        /// 2016/02/22 arc yano #3434 サービス伝票 消耗品(SP)の対応 原価０の部品の対応と同様の対応を行う
        /// 2016/02/17 arc yano #3435 サービス伝票 原価０の部品の対応
        /// 2015/10/28 arc yano #3289 サービス伝票 引当在庫の管理方法の変更　引当済数の更新を行う
        /// 2015/03/18 arc nakayama 伝票修正対応　伝票修正時の在庫更新処理 新規追加
        /// </history>
        public void UpdatePartsStockForModification(List<ServiceSalesLine> lines, ServiceSalesHeader header)
        {
            PartsStockDao stockDao = new PartsStockDao(db);

            //Mod 2017/01/31 arc yano #3566
            //Mod 2016/08/13 arc yano #3596
            //Add 2016/06/13 arc yano #3571
            //----------------------------
            //戻し先の設定
            //----------------------------
            
            //DBの最新リビジョンの明細行の取得
            ServiceSalesHeader ret  = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            //明細行の設定
            List<ServiceSalesLine> wklines;

            //DBに登録されている部門と入力された部門が異なる場合(=部門が変更された場合)
            if (ret != null && !ret.DepartmentCode.Equals(header.DepartmentCode))
            {
                wklines = new List<ServiceSalesLine>();

                //部門が変更された場合は一旦全ての引当を解除するため、明細の引当済数は０に設定する
                foreach (var l in lines)
                {
                    l.ProvisionQuantity = 0m;
                }
            }
            else
            {
                wklines = lines;
            }
            
            //部門から倉庫を特定する
            DepartmentWarehouse retdepartmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, ret.DepartmentCode);

            //仕掛ロケーションを取得する
            Location retshikakariLocation = GetShikakariLocation(retdepartmentWarehouse != null ? retdepartmentWarehouse.WarehouseCode : "");

            //仕掛ロケーションコードの設定
            string retshikakariLocationCode = (retshikakariLocation != null ? retshikakariLocation.LocationCode : "");

            IServiceSalesOrderService serviceSalesOrderService = new ServiceSalesOrderService(db);

            string locationCode = "";
            string departureLocationCode = "";        //出発ロケーションコード
            string arrivalLocationCode = "";          //到着ロケーションコード
            string transferType = "";                 //移動種別
            decimal quantity = 0;                     //数量


            //------------------------------------------
            //在庫の戻しの前処理
            //------------------------------------------
            //--------------------------------------------
            //DBの最新リビジョンの明細行の取得
            //--------------------------------------------
            //var ret = new ServiceSalesOrderDao(db).GetBySlipNumber(header.SlipNumber);

            decimal difQuantity = 0;

            //DBに登録されている場合のみ処理実行(行追加の場合は前処理は必要なし)
            if (ret != null)
            {
                //Mod 2016/02/22 arc yano #3434
                //Mod 2016/02/17 arc yano #3435
                //在庫管理対象の部品の明細行だけ取得
                var partsLinesDb = (ret.ServiceSalesLine.Where(x => new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997"))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                
                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsDb = partsLinesDb.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //--------------------------------------------
                //エンティティの明細行(=画面の入力値)の取得
                //--------------------------------------------   
                //Mod 2017/01/31 arc yano #3566
                //Mod 2016/02/22 arc yano #3434
                //Mod 2016/02/17 arc yano #3435
                //在庫管理対象の部品の明細行だけ取得
                var partsLinesEntity = (wklines.Where(x => (new PartsDao(db).IsInventoryParts(x.PartsNumber) && !x.StockStatus.Equals("998") && !x.StockStatus.Equals("997")))).Select(x => new { PartsNumber = x.PartsNumber, ProvisionQuantity = x.ProvisionQuantity ?? 0 });
                //明細行の引当済数を部品毎にグルーピング
                var lGroupByPartsEntity = partsLinesEntity.GroupBy(x => x.PartsNumber).Select(x => new { PartsNumber = x.Key, sumQuantity = x.Select(y => y.ProvisionQuantity).Sum() });

                //エンティティの部品をループ
                foreach (var l in lGroupByPartsDb)
                {
                    //対象部品の絞込み
                    var rec = (lGroupByPartsEntity.Where(x => x.PartsNumber.Equals(l.PartsNumber))).FirstOrDefault();

                    difQuantity = (rec == null ? 0 : rec.sumQuantity) - l.sumQuantity;

                    //エンティティの行削除、または部品名称変更による、差分が発生した場
                    if (difQuantity < 0)
                    {
                        difQuantity *= -1;      //正の値に変換

                        //Mod 2017/02/08 arc yano #3620
                        //Mod 2017/01/31 arc yano #3566
                        //Mod 2016/08/13 arc yano #3596
                        
                        //戻し先の取得
                        List<PartsStock> releseStockList = new List<PartsStock>();

                        //サービス伝票の引当履歴より戻し先を取得する
                        releseStockList = serviceSalesOrderService.GetStockListByTransfer(header, l.PartsNumber);

                        //戻し先が取得できなかった場合
                        if (releseStockList.Count() == 0)
                        {
                            //倉庫全体から在庫情報を取得する。※削除された在庫情報を含む
                            releseStockList = new PartsStockDao(db).GetListByWarehouse(retdepartmentWarehouse != null ? retdepartmentWarehouse.WarehouseCode : "", l.PartsNumber, true).OrderByDescending(x => x.Quantity).ToList();
                        }

                        if (releseStockList != null && releseStockList.Count() > 0)
                        {
                            //一番最初のロケーションに戻す
                            PartsStock stock = releseStockList.FirstOrDefault();                    //戻し先のロケーションを取得

                            //削除済データの場合は初期化した上で有効化
                            stock = new PartsStockDao(db).InitPartsStock(stock);                    //Add 2017/02/08 arc yano #3620
                            
                            stock.Quantity = stock.Quantity + difQuantity;                          //発生した差分の数だけ在庫を戻す
                            stock.LastUpdateDate = DateTime.Now;
                            stock.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;

                            locationCode = stock.LocationCode;
                            
                            //db.SubmitChanges();
                        }
                        else //戻し先が見つからなかった場合は新規作成(普通はありえないが、、、)
                        {
                            //Mod 2017/01/31 arc yano #3566
                            //Mod 2016/08/13 arc yano #3596
                            //当該部品のデフォルトロケーションを取得
                            PartsLocation location = new PartsLocationDao(db).GetByKey(l.PartsNumber, retdepartmentWarehouse != null ? retdepartmentWarehouse.WarehouseCode : "");  //Mod 2017/01/31 arc yano #3566

                            if(location != null)
                            {
                                locationCode = location.LocationCode;   
                            }
                            else
                            {
                                //Mod 2016/08/13 arc yano #3596
                                Location loc = new LocationDao(db).GetListByLocationType("001", retdepartmentWarehouse != null ? retdepartmentWarehouse.WarehouseCode : "", "").OrderBy(x => x.LocationCode).FirstOrDefault();  //Mod 2017/01/31 arc yano #3566
                               
                                if(loc != null)
                                {
                                    locationCode = loc.LocationCode;
                                }
                            }

                            PartsStock stock = new PartsStock();
                            stock.CreateDate = DateTime.Now;
                            stock.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                            stock.DelFlag = "0";
                            stock.LastUpdateDate = DateTime.Now;
                            stock.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                            stock.LocationCode = locationCode;
                            stock.PartsNumber = l.PartsNumber;
                            stock.Quantity = difQuantity;
                            stock.ProvisionQuantity = 0;
                            db.PartsStock.InsertOnSubmit(stock);
                        }

                        //Add 2018/05/14 arc yano #3880
                        //------------------------
                        //売上原価の取得
                        //------------------------
                        //納車済のサービス伝票で一番古いものを取得
                        List<ServiceSalesHeader> shList = new ServiceSalesOrderDao(db).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber).Where(x => x.ServiceOrderStatus.Equals("006")).OrderByDescending(x => x.RevisionNumber).ToList();

                        decimal? slCost = 0m;

                        foreach (var sh in shList)
                        {
                            ServiceSalesLine sl = sh.ServiceSalesLine.Where(x => !string.IsNullOrWhiteSpace(x.PartsNumber) && x.PartsNumber.Equals(l.PartsNumber)).FirstOrDefault();

                            if (sl != null)
                            {
                                slCost = sl.UnitCost;
                                break;
                            }
                        }

                        //移動平均単価の更新
                        new PartsMovingAverageCostDao(db).UpdateAverageCost(l.PartsNumber, "001", difQuantity, slCost, ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode);            

                        //Add 2016/06/13 arc yano #3571
                        //移動伝票作成①(仕掛→フリーロケーション)
                        serviceSalesOrderService.PartsTransfer(retshikakariLocationCode, locationCode, l.PartsNumber, difQuantity, header.SlipNumber, TTYPE_CANCELWORK, updatePartsStock: false);  //Mod 2017/01/31 arc yano #3566

                        //移動伝票作成②(フリーロケーション→フリーロケーション)
                        serviceSalesOrderService.PartsTransfer(locationCode, locationCode, l.PartsNumber, difQuantity, header.SlipNumber, TTYPE_CANCELPROVISION, updatePartsStock: false);
                    }
                }
            }

            //----------------------------------------------------------------
            //引当・引当解除処理(本処理)          
            //---------------------------------------------------------------- 
            //入力値の部門の倉庫を特定する
            DepartmentWarehouse departmentWarehouse = CommonUtils.GetWarehouseFromDepartment(db, header.DepartmentCode);

            //仕掛ロケーション
            string shikakariLocationCode = "";
            Location shikakariLocation = GetShikakariLocation(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "");
            shikakariLocationCode = shikakariLocation.LocationCode;

            for (int index = 0; index < lines.Count(); index++)
            {
                //サービス伝票明細行抜出
                ServiceSalesLine l = lines[index];

                //引当済数・発注数の初期化
                //引当済数、発注数がnullの場合(初期登録の場合は０に設定する)
                if (l.ProvisionQuantity == null)
                {
                    l.ProvisionQuantity = 0;
                }
                if (l.OrderQuantity == null)
                {
                    l.OrderQuantity = 0;
                }

                //部品マスタを取得する
                Parts parts = new PartsDao(db).GetByKey(l.PartsNumber);

                //Mod 2017/02/08 arc yano #3620
                //Mod 2016/08/13 arc yano #3596
                //Mod 2016/02/22 arc yano #3434
                //Mod 2016/02/17 arc yano #3435 
                //マスタに存在する部品、かつ在庫管理対象の部品のみ引当を行う
                if (!string.IsNullOrEmpty(l.PartsNumber) && new PartsDao(db).IsInventoryParts(l.PartsNumber) && !l.StockStatus.Equals("998") && !l.StockStatus.Equals("997"))
                {
                    //今回必要数量の算出（販売数－引当済数）
                    decimal requireQuantity = (l.Quantity ?? 0) - (l.ProvisionQuantity ?? 0);

                    List<PartsStock> stockList = new List<PartsStock>();
                    
                    //必要数量がマイナス(=仕掛解除・引当解除を行う)
                    if (requireQuantity <= 0)
                    {
                        //サービス伝票の引当履歴から戻し先を特定する
                        stockList = serviceSalesOrderService.GetStockListByTransfer(header, l.PartsNumber);

                        if (stockList.Count() == 0)
                        {
                            //移動元は在庫の多いロケーションから順番に使う ※削除データ取得する
                            stockList = new PartsStockDao(db).GetListByWarehouse(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "", l.PartsNumber, true).OrderBy(x => x.DelFlag).OrderByDescending(x => x.Quantity).ToList();
                        }
                        
                        //Add 2018/02/23 arc yano #3741
                        //それでも在庫情報が無い場合は、部品ロケーションマスタからロケーションを取得して作成する
                        if (stockList.Count() == 0)
                        {
                            Location location = GetDefaultLocation(l.PartsNumber, departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "");
                            if (location != null)
                            {
                                PartsStock newRec = new PartsStock();

                                newRec.CreateDate = DateTime.Now;
                                newRec.CreateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                                newRec.DelFlag = "0";
                                newRec.LastUpdateDate = DateTime.Now;
                                newRec.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;
                                newRec.LocationCode = location.LocationCode;
                                newRec.PartsNumber = l.PartsNumber;
                                newRec.Quantity = 0;
                                newRec.ProvisionQuantity = 0;
                                db.PartsStock.InsertOnSubmit(newRec);
                                db.SubmitChanges();

                                //在庫リストに追加
                                stockList.Add(newRec);
                            }
                        }

                        //Add 2018/05/14 arc yano #3880
                        //------------------------
                        //売上原価の取得
                        //------------------------
                        //納車済のサービス伝票で一番古いものを取得
                        List<ServiceSalesHeader> shList = new ServiceSalesOrderDao(db).GetListByLessThanRevision(header.SlipNumber, header.RevisionNumber).Where(x => x.ServiceOrderStatus.Equals("006")).OrderByDescending(x => x.RevisionNumber).ToList();

                        decimal? slCost = 0m;

                        foreach (var sh in shList)
                        {
                            ServiceSalesLine sl = sh.ServiceSalesLine.Where(x => !string.IsNullOrWhiteSpace(x.PartsNumber) && x.PartsNumber.Equals(l.PartsNumber)).FirstOrDefault();

                            if (sl != null)
                            {
                                slCost = sl.UnitCost;
                                break;
                            }
                        }

                        //移動平均単価の更新
                        new PartsMovingAverageCostDao(db).UpdateAverageCost(l.PartsNumber, "001", (requireQuantity * -1) , slCost, ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode);            

                    }
                    else //必要数量がプラス(=引当・仕掛を行う)
                    {
                        //移動元は在庫の多いロケーションから順番に使う ※削除データ取得しない
                        stockList = new PartsStockDao(db).GetListByWarehouse(departmentWarehouse != null ? departmentWarehouse.WarehouseCode : "", l.PartsNumber, false).OrderByDescending(x => x.Quantity).ToList();
                    }

                    //---------------------------------------------------------------------------------------------
                    //引当(解除)処理
                    //---------------------------------------------------------------------------------------------
                    decimal incOrDecQuantity = 0;             //増減数

                    //必要数全てを引き当てられるか、全てのロケーションから引当終えたら終了
                    for (int i = 0; i < stockList.Count; i++)
                    {

                        //削除フラグがＯＮの場合
                        if (!string.IsNullOrWhiteSpace(stockList[i].DelFlag) && stockList[i].DelFlag.Equals("1"))
                        {
                            //数量、引当済数を０に設定
                            stockList[i].Quantity = 0;
                            stockList[i].ProvisionQuantity = 0;
                        }

                        //在庫の引当可能数を算出。
                        decimal allowableQuantity = (stockList[i].Quantity ?? 0) - (stockList[i].ProvisionQuantity ?? 0);

                        //必要数 > 0 で引当可能数 <= 0の場合は次の在庫ロケーションへ
                        if (requireQuantity > 0 && allowableQuantity <= 0)
                        {
                            continue;
                        }
                        //---------------------------------------------------------
                        ///増減数の決定
                        // ①対象ロケーションの引当可能数が必要数より少ない場合
                        //   →増減数 = 引当可能数
                        // ②対象ロケーションの引当可能数が必要数より多い場合
                        //   →増減数 = 必要数
                        //　※但し、最後のロケーションまで行っても差し引けない
                        //    場合は、強制的に－する(マイナス在庫が発生する)
                        //---------------------------------------------------------
                        incOrDecQuantity = ((i != stockList.Count -1) && (allowableQuantity - requireQuantity < 0)) ? allowableQuantity : requireQuantity;

                        //増減数で明細行、在庫情報の引当済数と必要数を更新する
                        l.ProvisionQuantity += incOrDecQuantity;
                        l.LastUpdateDate = DateTime.Now;
                        l.LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;

                        
                        //----------------------------
                        //引当(解除)の移動伝票作成
                        //----------------------------
                        if (incOrDecQuantity < 0)
                        {
                            transferType = TTYPE_CANCELPROVISION;
                            quantity = incOrDecQuantity * (-1);
                        }
                        else
                        {
                            transferType = TTYPE_PROVISION;
                            quantity = incOrDecQuantity;
                        
                        }
                        //移動伝票作成①(フリーロケーション→フリーロケーション)
                        serviceSalesOrderService.PartsTransfer(stockList[i].LocationCode, stockList[i].LocationCode, l.PartsNumber, quantity, header.SlipNumber, transferType, updatePartsStock: false);    //Add 2016/06/13 arc yano #3571
                        
                        //----------------------------
                        //仕掛(解除)の移動伝票作成
                        //----------------------------
                        if (incOrDecQuantity < 0)
                        {
                            transferType = TTYPE_CANCELWORK;
                            quantity = incOrDecQuantity * (-1);
                            departureLocationCode = shikakariLocationCode;
                            arrivalLocationCode = stockList[i].LocationCode;
                        }
                        else
                        {
                            transferType = TTYPE_WORK;
                            quantity = incOrDecQuantity;
                            departureLocationCode = stockList[i].LocationCode;
                            arrivalLocationCode = shikakariLocationCode;
                        }

                        //移動伝票作成②(フリーロケーション→仕掛)
                        serviceSalesOrderService.PartsTransfer(departureLocationCode, arrivalLocationCode, l.PartsNumber, quantity, header.SlipNumber, transferType, updatePartsStock: false);    //Add 2016/06/13 arc yano #3571

                        stockList[i].Quantity -= incOrDecQuantity;      //数量を直接差し引く

                        stockList[i].LastUpdateDate = DateTime.Now;
                        stockList[i].LastUpdateEmployeeCode = ((Employee)HttpContext.Current.Session["Employee"]).EmployeeCode;

                        //Add 2017/02/08 arc yano #3620
                        if (!string.IsNullOrWhiteSpace(stockList[i].DelFlag) && stockList[i].DelFlag.Equals("1"))
                        {
                            stockList[i].DelFlag = "0";             //在庫情報を有効化する
                        }

                        requireQuantity -= incOrDecQuantity;

                        //必要数全部引当済の場合は終了
                        if (requireQuantity == 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    //何もしない。伝票はそのままにするが、在庫は更新しない(#3171  第１段対応：伝票は作成出来るように対応するが、在庫として反映させないようにする。)
                }
            }
        }

        //Del 2017/01/31 arc yano #3566 コメントとして残っていたところを削除
        /* //Mod 2015/10/28 arc yano #3289　コメントアウト
        
        */
    }
}
