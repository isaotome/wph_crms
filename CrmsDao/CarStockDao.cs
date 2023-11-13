using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlClient;


//------------------------------------------------------------
// 機能：車両管理データアクセスクラス
// 作成：2014/07/28 arc yano IPO対応 
//
//------------------------------------------------------------
namespace CrmsDao
{
    
    //車輛管理データ取得クラス
    public class CarStockDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public CarStockDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /// <summary>
        /// 車両管理データ取得(PK指定)
        /// </summary>
        /// <param name="processDate">処理年月日</param>
        /// <param name="salesCarNumber">管理番号</param>
        /// <returns>車両管理データ(1件)</returns>
        /// <history>
        /// 2018/08/28 yano #3922 引数の型をstring→DateTimeに変更
        /// </history>
        public CarStock GetByKey(DateTime targetMonth, string salesCarNumber)
        {
            // 車両管理データの取得
            CarStock carStock =
                (from a in db.CarStock
                 where a.ProcessDate.Equals(targetMonth)
                 && a.SalesCarNumber.Equals(salesCarNumber)
                 && a.DelFlag.Equals("0")
                 select a
                ).FirstOrDefault();

            // 車両管理データの返却
            return carStock;
        }

        /// <summary>
        /// 車両管理データ取得
        /// </summary>
        /// <param name="processMonth">処理年月</param>
        /// <returns>車両管理データ(複数件)</returns>
        /// <history>
        /// 2018/08/28 yano #3922 引数の型をstring→DateTimeに変更
        /// </history>
        public List<CarStock> GetByProcessDate(DateTime targetMonth)
        {
            // 車両管理データの取得
            List<CarStock> carStockList =
                (from a in db.CarStock
                 where a.ProcessDate.Equals(targetMonth)
                 && a.DelFlag.Equals("0")
                 select a).ToList<CarStock>();

            // 車両管理データの返却
            return carStockList;
        }

        //Del 2018/08/28 yano #3922 車両管理表(タマ表)　検索条件の変更 全てストアドプロシージャに任せるため、削除
        /// <summary>
        /// 仕入元データ、売上元データより車両在庫データの取得
        /// </summary>
        /// <param name="processDate">処理年月日</param>
        /// <param name="employeecode">社員コード</param>
        /// <param name="dateRange">日付の範囲 dataRage[0]…月初, dataRange[1]…月末</param>
        /// <param name="regFlag">DB登録フラグ(false:DBに登録しない true:DBに登録する)</param>
        /// <returns>車両管理データ</returns>
        /// <history>
        ///  2018/06/06 arc yano #3883 タマ表改善 参照する値を車両仕入価格→財務価格に変更
        ///  2017/08/21 arc yano #3782 車両キャンセル機能追加
        ///  2016/11/30 arc yano #3659 車両管理 
        ///                      ①メソッドの名前、引数、戻り値を変更
        ///                      ②DB登録処理を別メソッドでコール
        /// </history>
        //public List<CarStock> MakeCarStockData(string processDate, string employeecode, DateTime[] dateRange, bool regFlag)
        //{

        //    List<CarStock> carStockList = new List<CarStock>();
            
        //    DateTime pDateTo = new DateTime(dateRange[1].Year, dateRange[1].Month, 1).AddDays(-1);      //前月の月末

        //    //処理年月取得
        //    int targetYear = DateTime.ParseExact(processDate, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).Year;
        //    int targetMonth = DateTime.ParseExact(processDate, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).Month;


        //    var list = db.GetCarStock(dateRange[1], pDateTo);

        //    //----------------------------------------
        //    // 車両在庫レコードの作成
        //    //----------------------------------------
        //    foreach (var a in list)
        //    {
        //        //Mod 2014/10/01 IPO対応(車両管理) 
        //        decimal? Amount = 0;

        //        //Mod 2018/06/06 arc yano #3883
        //        //リサイクル料は引かない(確認済)
        //        Amount = a.FinancialAmount; 
                

        //        if ((a.RecyclePrice != null))
        //        {
        //            //Mod 2018/06/06 arc yano #3883
        //            Amount = a.FinancialAmount - a.RecycleAmount; //財務価格
        //            //Amount = a.Amount - a.RecycleAmount;     //仕入価格
        //        }
        //        else
        //        {
        //            Amount = a.FinancialAmount; //Mod 2018/06/06 arc yano #3883
        //            //Amount = a.Amount;
        //        }

        //        CarStock carstock = new CarStock();
        //        carstock.ProcessDate = processDate;                                                     //処理年月日
        //        carstock.SalesCarNumber = a.SalesCarNumber;                                             //管理番号
        //        carstock.NewUsedType = a.NewUsedType;                                                   //新中区分
        //        carstock.NewUsedTypeName = a.NewUsedTypeName;                                           //新中区分名

        //        carstock.BrandStore = a.BrandStoreCode;                                                 //ブランド別店舗
        //        carstock.PurchaseDate = a.PurchaseDate;                                                 //仕入日
        //        carstock.CarName = a.MakerName;                                                         //メーカー
        //        carstock.CarGradeCode = a.CarGradeCode;                                                 //グレードコード

        //        carstock.CarCarName = a.CarName;                                                        //車種名

        //        carstock.Vin = a.Vin;                                                                   //車台番号
        //        carstock.PurchaseLocationCode = a.PurchaseLocationCode;                                 //仕入拠点
        //        carstock.PurchaseLocationName = a.PurchaseLocationName;                                 //仕入拠名  //Add 2016/11/30 arc yano #3659

        //        if (!string.IsNullOrEmpty(a.LocationCode))
        //        {
        //            //Mod 2018/06/06 arc yano #3883
        //            Location loc = new LocationDao(db).GetByKey(a.LocationCode);

        //            if(loc != null)
        //            {
        //                carstock.Location = loc;
        //            }
                    
        //        }
               
        //        carstock.LocationCode = (a.LocationCode == null ? "棚卸中" : a.LocationCode);           //当月実棚 //Add 2016/11/30 arc yano #3659

        //        //Add 2015/04/08 arc yano #3164 車両管理 リサイクル料の追加
        //        carstock.RecycleAmount = a.RecycleAmount;               //リサイクル料

        //        //在庫拠点
        //        if (string.IsNullOrEmpty(a.ArrivalLocationCode))
        //        {
        //            carstock.CarStockLocationCode = a.PurchaseLocationCode;

        //            carstock.CarStockLocationName = a.PurchaseLocationName;                              //Add 2016/11/30 arc yano #3659
        //        }
        //        else
        //        {
        //            carstock.CarStockLocationCode = a.ArrivalLocationCode;

        //            carstock.CarStockLocationName = a.ArrivalLocationName;                              //Add 2016/11/30 arc yano #3659
        //        }

        //        carstock.CarPurchaseType = a.CarPurchaseType;                                           //仕入区分
        //        carstock.CarPurchaseTypeName = a.CarPurchaseTypeName;                                   //仕入区分名 //Add 2016/11/30 arc yano #3659
                

        //        carstock.SupplierCode = a.SupplierCode;                                                 //仕入先
        //        carstock.SupplierName = a.SupplierName;                                                 //仕入先名 //Add 2016/11/30 arc yano #3659
                

        //        //入庫日が当月の場合
        //        if ((a.PurchaseDate != null) && (a.PurchaseDate.Value.Year == targetYear) && (a.PurchaseDate.Value.Month == targetMonth))
        //        {
        //            carstock.BeginningInventory = null;                     //月初在庫

        //            //Mod 2015/04/08 arc yano #3164 車両管理　対応④(他勘定受入追加)
        //            //仕入区分＝自社仕入(自社登録は除く)の場合は他勘定受入とする
        //            if ((!string.IsNullOrWhiteSpace(a.CarPurchaseType) && a.CarPurchaseType.Equals("006")) &&
        //                (string.IsNullOrWhiteSpace(a.CompanyRegistrationFlag) || !a.CompanyRegistrationFlag.Equals("1")))
        //            {
        //                CarPurchase Cp = new CarPurchaseDao(db).GetBySalesCarNumber(a.SalesCarNumber);

        //                if (Cp != null)
        //                {
        //                    if (!string.IsNullOrEmpty(Cp.RegistOwnFlag) && Cp.RegistOwnFlag.Equals("1"))
        //                    {
        //                        carstock.MonthPurchase = Amount;                  //当月仕入
        //                        carstock.OtherAccount = null;                     //他勘定受入
        //                    }
        //                    else
        //                    {
        //                carstock.MonthPurchase = null;                      //当月仕入
        //                carstock.OtherAccount = Amount;                     //他勘定受入
        //            }
        //                }
        //                else
        //                {
        //                    carstock.MonthPurchase = null;                      //当月仕入
        //                    carstock.OtherAccount = Amount;                     //他勘定受入
        //                }
        //            }
        //            else
        //            {
        //                carstock.MonthPurchase = Amount;                    //当月仕入
        //                carstock.OtherAccount = null;                       //他勘定受入
        //            }
        //        }
        //        else
        //        {
        //            carstock.BeginningInventory = Amount;                   //月初在庫
        //            carstock.MonthPurchase = null;                          //当月仕入
        //            carstock.OtherAccount = null;                           //他勘定受入
        //        }

        //        //Mod 2018/06/06 arc yano #3883
        //        carstock.CancelPurchase = (a.CancelFinancialAmount ?? 0);
        //        //carstock.CancelPurchase = (a.CancelAmount ?? 0);
                    
                
            
        //        //納車日が当月の場合
        //        if ((a.SalesDate != null) && ((a.SalesDate.Value.Year == targetYear) && (a.SalesDate.Value.Month == targetMonth)))
        //        {
        //            carstock.SalesDate = a.SalesDate;                       //納入日
        //            carstock.SlipNumber = a.SlipNumber;                     //伝票番号
        //            carstock.CustomerCode = a.CustomerCode;                 //販売先
        //            carstock.CustomerName = a.CustomerName;                     //販売先名 //Add 2016/11/30 arc yano #3659
                    



        //            carstock.SalesPrice = a.SalesPrice;                     //車両本体価格
        //            carstock.DiscountAmount = a.DiscountAmount;             //値引
        //            //付属品
        //            if (a.MakerOptionAmount != null)
        //            {
        //                carstock.ShopOptionAmount = a.ShopOptionAmount + a.MakerOptionAmount;
        //            }
        //            else
        //            {
        //                carstock.ShopOptionAmount = a.ShopOptionAmount;
        //            }

        //            carstock.SalesCostTotalAmount = a.SalesCostTotalAmount; //販売諸費用合計

        //            //Mod 2015/01/19 IPO対応 売上総合計の計算バグの対応
        //            //carstock.SalesTotalAmount = a.SalesTotalAmount;         //売上総合計 = 車両本体価格 + 値引 + 付属品 + 販売諸費用合計
        //            carstock.SalesTotalAmount = carstock.SalesPrice + carstock.DiscountAmount + carstock.ShopOptionAmount + carstock.SalesCostTotalAmount;

        //            carstock.SalesCostAmount = Amount;                    //売上原価
        //            carstock.SalesProfits = (a.SalesTotalAmount == null ? 0 : a.SalesTotalAmount) - Amount;  //売上粗利

        //            //部門マスタのエリアコードが999以外の場合に、部門をセットする
        //            if (!string.IsNullOrEmpty(a.AreaCode) && !a.AreaCode.Equals("999"))
        //            {
        //                carstock.PurchaseDepartmentCode = a.DepartmentCode;     //販売店舗

        //                carstock.PurchaseDepartmentName = a.DepartmentName;     //販売店舗名 //Add 2016/11/30 arc yano #3659
        //            }
        //            else
        //            {
        //                carstock.PurchaseDepartmentCode = "";
        //            }

        //            if (!string.IsNullOrEmpty(a.SalesType) && (a.SalesType.Equals("001") || a.SalesType.Equals("002")))
        //            {
        //                carstock.SalesType = "一般";                            //販売先区分
        //            }
        //            else
        //            {
        //                carstock.SalesType = "業販";
        //            }
        //        }
        //        else
        //        {
        //            //Mod 2015/01/19 IPO対応 販売データ設定処理を関数化
        //            //販売データ設定(null設定)
        //            SetSalesData(ref carstock);
        //        }


        //        //Mod 2015/04/08 arc yano IPO対応 車両管理対応④
        //        //              ①仕入減少(レンタカー、業務車両、広報車)を追加
        //        //              ②仕入減少(自社登録、デモカー、代車)の判定条件の変更
        //        //自社登録フラグがONの場合
        //        if (!string.IsNullOrWhiteSpace(a.SalesType) && a.SalesType.Equals("005"))
        //        {
        //            carstock.SelfRegistration = Amount;                         //仕入減少_自社登録（中古車仕入）
        //        }
        //        else
        //        {
        //            carstock.SelfRegistration = null;
        //        }

        //        if (string.IsNullOrEmpty(a.DepartmentCode))
        //        {
        //            carstock.OtherDealer = Amount;       //仕入減少_他ﾃﾞｰﾗｰ
        //        }
        //        else
        //        {
        //            carstock.OtherDealer = null;
        //        }

        //        //利用用途 = 「デモカー」または顧客タイプ=「デモカー」
        //        if ((!string.IsNullOrEmpty(a.CarUsage) && a.CarUsage.Equals("001")) ||
        //            (!string.IsNullOrEmpty(a.CustomerType) && a.CustomerType.Equals("101")))
        //        {
        //            carstock.DemoCar = Amount;            //仕入減少_ﾃﾞﾓｶｰ
        //        }
        //        else
        //        {
        //            carstock.DemoCar = null;
        //        }

        //        //利用用途 = 「代車」または顧客タイプ=「代車」
        //        if ((!string.IsNullOrEmpty(a.CarUsage) && a.CarUsage.Equals("005")) ||
        //            (!string.IsNullOrEmpty(a.CustomerType) && a.CustomerType.Equals("102")))
        //        {
        //            carstock.TemporaryCar = Amount;       //仕入減少_代車
        //        }
        //        else
        //        {
        //            carstock.TemporaryCar = null;
        //        }

        //        //利用用途 = 「レンタカー」
        //        if (!string.IsNullOrEmpty(a.CarUsage) && a.CarUsage.Equals("002"))
        //        {
        //            carstock.RentalCar = Amount;       //仕入減少_レンタカー
        //        }
        //        else
        //        {
        //            carstock.RentalCar = null;
        //        }

        //        //利用用途 = 「業務車両」
        //        if (!string.IsNullOrEmpty(a.CarUsage) && a.CarUsage.Equals("003"))
        //        {
        //            carstock.BusinessCar = Amount;       //仕入減少_業務車両
        //        }
        //        else
        //        {
        //            carstock.BusinessCar = null;
        //        }

        //        //利用用途 = 「広報車」
        //        if (!string.IsNullOrEmpty(a.CarUsage) && a.CarUsage.Equals("004"))
        //        {
        //            carstock.PRCar = Amount;       //仕入減少_広報車
        //        }
        //        else
        //        {
        //            carstock.PRCar = null;
        //        }

        //        if (!string.IsNullOrEmpty(a.CancelFlag) && a.CancelFlag.Equals("1"))
        //        {
        //            carstock.CancelCar = Amount;          //仕入減少_仕入キャンセル
        //        }
        //        else
        //        {
        //            carstock.CancelCar = null;
        //        }

        //        //Mod 2015/04/09 arc yano 車両管理対応④ 仕入減少計の計算方法の変更(デモカー＋代車＋レンタカー＋業務車両＋広報車)
        //        //Mod 2015/01/09 arc yano IPO対応 仕入減少_計の計算に、代車、仕入ｷｬﾝｾﾙを追加。また、仕入減少計がNULLでない場合は、販売欄を非表示にする。
        //        carstock.ReductionTotal = (carstock.DemoCar == null ? 0 : carstock.DemoCar) + (carstock.TemporaryCar == null ? 0 : carstock.TemporaryCar) + (carstock.RentalCar == null ? 0 : carstock.RentalCar) + (carstock.BusinessCar == null ? 0 : carstock.BusinessCar) + (carstock.PRCar == null ? 0 : carstock.PRCar);  //固定資産振替計
        //        if (carstock.ReductionTotal == 0)
        //        {
        //            carstock.ReductionTotal = null;
        //        }

        //        //Mod 2015/04/08 arc yano 車両管理対応④ 仕入減少時の販売データの非表示化は画面で行うように変更
        //        /*
        //        else
        //        {
        //            //Mod 2015/01/19 IPO対応 販売データ設定処理を関数化
        //            //販売データ設定
        //            SetSalesData(ref carstock);
        //        }
        //        */

        //        if ((carstock.SalesTotalAmount == null) && (carstock.ReductionTotal == null))
        //        {
        //            //Mod 2017/08/21 arc yano #3782
        //            carstock.EndInventory = (Amount + (carstock.RecycleAmount ?? 0) + carstock.CancelPurchase);       //月末在庫
        //        }
        //        else
        //        {
        //            carstock.EndInventory = null;
        //        }

        //        carstock.CreateEmployeeCode = employeecode;     //作成者
        //        carstock.CreateDate = DateTime.Now;             //作成日時
        //        carstock.LastUpdateEmployeeCode = employeecode; //最終更新者
        //        carstock.LastUpdateDate = DateTime.Now;         //最終更新日時
        //        carstock.DelFlag = "0";                         //削除フラグ

        //        carStockList.Add(carstock);
        //    }

        //    //登録フラグ=ONの場合はDBに登録を行う
        //    if (regFlag == true)
        //    {
        //        //一旦当月のデータを削除
        //        List<CarStock> delList = GetListByProcessDate(processDate);
        //        db.CarStock.DeleteAllOnSubmit(delList);

        //        //挿入処理
        //        InsertCarStockData(carStockList);
        //    }

        //    return carStockList;
        //}


        //Del 2018/08/28 yano #3922 車両管理表(タマ表)　検索条件の変更 全てストアドプロシージャに任せるため、削除
        ///// <summary>
        ///// 車両管理データのＤＢ登録
        ///// </summary>
        ///// <param name="carStockList">車両管理データ</param>
        ///// <returns></returns>
        ///// <history>
        /////  Mod 2016/11/30 arc yano #3659 新規作成
        ///// </history>
        //private void InsertCarStockData(List<CarStock> carStockList)
        //{
          
        //    //dbインサート
        //    db.CarStock.InsertAllOnSubmit(carStockList);
            
        //    try
        //    {
        //        db.SubmitChanges();
        //    }
        //    catch (SqlException e)
        //    {
        //        ////Mod 2014/08/13 arc amii エラーログ対応 Exceptionを設定し、ログ出力を行うよう修正
        //        OutputLogData.exLog = e;
        //        OutputLogData.procName = "車両在庫レコード作成";
        //        throw e;
        //    }
        //}

        //Del 2018/08/28 yano #3922 車両管理表(タマ表)　検索条件の変更 全てストアドプロシージャに任せるため、削除
        /// <summary>
        /// 販売データ設定(初期化)
        /// </summary>
        /// <param name="carstock">車両在庫データ</param>
        /// <returns>初期化済データ設定</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　検索条件の変更
        /// 2016/11/30 arc yano #3659 車両管理 項目追加
        /// </history>
        //public void SetSalesData(ref CarStock carStock)
        //{
        //    carStock.SalesDate = null;
        //    carStock.SlipNumber = "";
        //    carStock.CustomerCode = "";
        //    carStock.CustomerName = "";                     // Add 2016/11/30 arc yano #3659
        //    carStock.SalesPrice = null;
        //    carStock.DiscountAmount = null;
        //    carStock.ShopOptionAmount = null;
        //    carStock.SalesCostTotalAmount = null;
        //    carStock.SalesTotalAmount = null;
        //    carStock.SalesCostAmount = null;
        //    carStock.SalesProfits = null;
        //    carStock.PurchaseDepartmentCode = "";
        //    carStock.PurchaseDepartmentName = "";           // Add 2016/11/30 arc yano #3659
        //    carStock.SalesType = "";
        //}

        /// <summary>
        /// 車両管理データ検索(本締め済データ)
        /// </summary>
        /// <param name="carCondition">車両管理データ検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両管理データ検索結果</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　ストアドを全面的に改修
        /// 2016/11/30 arc yano #3659 車両管理 メソッド名の変更 ソートキーの追加(管理番号)
        /// </history>
        public IQueryable<CarStock>  GetClosedListByCondition(CarStock carStockCondition)
        {
        
            IQueryable<CarStock> carStockList = null;

            //対象年月
            DateTime targetMonth;

            bool retvalue = DateTime.TryParse((carStockCondition.TargetMonth + "/01"), out targetMonth);

            //新中区分
            string newUsedType = carStockCondition.NewUsedType;
            
            //管理番号
            string salesCarNumber = carStockCondition.SalesCarNumber;

            //車台番号
            string vin = carStockCondition.Vin;

            //仕入先
            string supplierCode = carStockCondition.SupplierCode;

            //データを抽出する
            carStockList =
                    from a in db.CarStock
                    where
                        a.ProcessDate.Equals(targetMonth)
                        && (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
                        && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
                        && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                        && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
                        && a.DelFlag.Equals("0")
                    orderby a.PurchaseDate, a.SalesCarNumber
                    select a;


            //データ種別により抽出条件を変更する。
            switch (carStockCondition.DataKind)
            {

                case "001": //在庫

                    carStockList = carStockList.Where(x => x.EndInventory != null);

                    break;

                case "002": //販売

                    carStockList = carStockList.Where(x => x.EndInventory == null);

                    break;

                case "004": //仕入

                    DateTime targetNextMonth = targetMonth.AddMonths(1);

                    carStockList = carStockList.Where(x => DateTime.Compare(x.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, targetMonth) >= 0 && DateTime.Compare(x.SalesDate ?? DaoConst.SQL_DATETIME_MAX, targetNextMonth) < 0);

                    break;

                default:    //それ以外
                    //なし
                    break;
            }

            // 出口
            return carStockList;


            //IOrderedQueryable<CarStock> ret = null;

            ////対象年月
            //string targetMonth = carStockCondition.TargetMonth;
            ////新中区分
            //string newUsedType = carStockCondition.NewUsedType;

            ////日付の指定範囲
            //DateTime? DateFrom = carStockCondition.DateFrom;
            //DateTime? DateTo = carStockCondition.DateTo;

            ////管理番号
            //string salesCarNumber = carStockCondition.SalesCarNumber;

            ////車台番号
            //string vin = carStockCondition.Vin;

            ////仕入先
            //string supplierCode = carStockCondition.SupplierCode;

            ////データ種別により抽出条件を変更する。
            //switch (carStockCondition.DataKind)
            //{
            //    case "001": //在庫

            //        //Mod 2015/01/19 arc yano IPO対応 車両管理データのクエリを全面見直し
            //        IOrderedQueryable<CarStock> carStockList =

            //          from a in db.CarStock
            //          where
            //          !
            //          (
            //            (
            //                from aa in db.CarStock
            //                where (DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
            //                select aa.SalesCarNumber
            //            ).Contains(a.SalesCarNumber)
            //          )
            //          && (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //          && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //          && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //          && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //          && a.DelFlag.Equals("0")
            //          && a.ProcessDate.Contains(targetMonth)
            //          orderby a.PurchaseDate, a.SalesCarNumber  //Mod 2016/11/30 arc yano #3659
            //          select a;
                   
            //        ret = carStockList;
            //        break;

            //    case "002": //販売

            //        IOrderedQueryable<CarStock> carSalesStateList =

            //              from a in db.CarStock
            //              where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //              && ((DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, DateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
            //              && (DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0))
            //              && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //              && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //              && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //              && a.DelFlag.Equals("0")
            //              && a.ProcessDate.Contains(targetMonth)
            //              orderby a.PurchaseDate, a.SalesCarNumber //Mod 2016/11/30 arc yano #3659
            //              select a;

            //        ret = carSalesStateList;
            //        break;

            //    //Mod 2015/01/16 arc yano IPO対応 車両管理データのクエリを全面見直し
            //    case "003": //在庫＋販売

            //        IOrderedQueryable<CarStock> carSalesStockList =

            //            from a in db.CarStock 
            //            where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //            && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //            && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //            && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //            && a.DelFlag.Equals("0")     
            //            && a.ProcessDate.Contains(targetMonth)
            //            orderby a.PurchaseDate, a.SalesCarNumber  //Mod 2016/11/30 arc yano #3659  
            //            select a;

            //        ret = carSalesStockList;
            //        break;

            //    case "004": //仕入  
            //    //Add 2015/04/08 arc yano #3164 車両管理対応④ 仕入の追加
            //     IOrderedQueryable<CarStock> carPurchaseStockList =

            //            from a in db.CarStock
            //            where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //            && ((DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, DateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
            //            && (DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0))
            //            && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //            && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //            && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //            && a.DelFlag.Equals("0")
            //            && a.ProcessDate.Contains(targetMonth)
            //            orderby a.PurchaseDate, a.SalesCarNumber //Mod 2016/11/30 arc yano #3659
            //            select a;

            //         ret = carPurchaseStockList;
            //        break;

            //    default:    //それ以外
            //        //なし
            //        break;
            //}

            //// 出口
            //return ret;
        }

        /// <summary>
        /// 車両管理データ検索(未締めデータ)
        /// </summary>
        /// <param name="carCondition">車両管理データ検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両管理データ検索結果</returns>
        /// <history>
        /// 2018/08/28 yano #3922 車両管理表(タマ表)　機能改善 ストアドプロシージャの大幅な変更による修正
        /// 2016/11/30 arc yano #3659 車両管理 新規作成
        /// </history>
        public IQueryable<CarStock> GetNonClosedListByCondition(CarStock carStockCondition)
        {
            //Mod 2018/08/28 yano #3922

            IQueryable<CarStock> carStockList = null;

            //対象年月
            string targetMonth = carStockCondition.TargetMonth;

            //新中区分
            string newUsedType = carStockCondition.NewUsedType;

            //管理番号
            string salesCarNumber = carStockCondition.SalesCarNumber;

            //車台番号
            string vin = carStockCondition.Vin;

            //仕入先
            string supplierCode = carStockCondition.SupplierCode;

            //アクションフラグ
            int actionFlag = 0;

            //担当者コード
            string employeeCode = "";

            //データタイプ
            string dataKind = carStockCondition.DataKind;

            //ストプロで取得
            carStockList = db.GetCarStock(targetMonth, actionFlag, dataKind, newUsedType, supplierCode, salesCarNumber, vin, employeeCode).ToList().AsQueryable();

            return carStockList;
           
            //IOrderedQueryable<CarStock> ret = null;

            ////対象年月
            //string targetMonth = carStockCondition.TargetMonth;

            ////新中区分
            //string newUsedType = carStockCondition.NewUsedType;

            ////日付の指定範囲
            //DateTime? DateFrom = carStockCondition.DateFrom;
            //DateTime? DateTo = carStockCondition.DateTo;

            ////管理番号
            //string salesCarNumber = carStockCondition.SalesCarNumber;

            ////車台番号
            //string vin = carStockCondition.Vin;

            ////仕入先
            //string supplierCode = carStockCondition.SupplierCode;

            ////日付の範囲の設定
            //DateTime[] dateRange = new DateTime[2];

            //dateRange[0] = (DateFrom ?? DaoConst.SQL_DATETIME_MAX);
            //dateRange[1] = (DateTo ?? DaoConst.SQL_DATETIME_MAX);


            ////車両管理データの作成
            //List<CarStock> makeStockList = MakeCarStockData(targetMonth + "01", "", dateRange, false);


            ////データ種別により抽出条件を変更する。
            //switch (carStockCondition.DataKind)
            //{
            //    case "001": //在庫

            //        //Mod 2015/01/19 arc yano IPO対応 車両管理データのクエリを全面見直し
            //        IOrderedQueryable<CarStock> carStockList =

            //          from a in makeStockList.AsQueryable()
            //          where
            //          !
            //          (
            //            (
            //                from aa in makeStockList.AsQueryable()
            //                where (DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
            //                select aa.SalesCarNumber
            //            ).Contains(a.SalesCarNumber)
            //          )
            //          && (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //          && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //          && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //          && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //          && a.DelFlag.Equals("0")
            //          && a.ProcessDate.Contains(targetMonth)
            //          orderby a.PurchaseDate, a.SalesCarNumber
            //          select a;

            //        ret = carStockList;
            //        break;

            //    case "002": //販売

            //        IOrderedQueryable<CarStock> carSalesStateList =

            //              from a in makeStockList.AsQueryable()
            //              where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //              && ((DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, DateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
            //              && (DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0))
            //              && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //              && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //              && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //              && a.DelFlag.Equals("0")
            //              && a.ProcessDate.Contains(targetMonth)
            //              orderby a.PurchaseDate, a.SalesCarNumber
            //              select a;

            //        ret = carSalesStateList;
            //        break;

            //    //Mod 2015/01/16 arc yano IPO対応 車両管理データのクエリを全面見直し
            //    case "003": //在庫＋販売

            //        IOrderedQueryable<CarStock> carSalesStockList =

            //            from a in makeStockList.AsQueryable()
            //            where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //            && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //            && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //            && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //            && a.DelFlag.Equals("0")
            //            && a.ProcessDate.Contains(targetMonth)
            //            orderby a.PurchaseDate, a.SalesCarNumber
            //            select a;

            //        ret = carSalesStockList;
            //        break;

            //    case "004": //仕入  
            //        //Add 2015/04/08 arc yano #3164 車両管理対応④ 仕入の追加
            //        IOrderedQueryable<CarStock> carPurchaseStockList =

            //               from a in makeStockList.AsQueryable()
            //               where (string.IsNullOrEmpty(newUsedType) || a.NewUsedType.Equals(newUsedType))
            //               && ((DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN, DateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
            //               && (DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX, DateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0))
            //               && (string.IsNullOrEmpty(salesCarNumber) || a.SalesCarNumber.Contains(salesCarNumber))
            //               && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
            //               && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Contains(supplierCode))
            //               && a.DelFlag.Equals("0")
            //               && a.ProcessDate.Contains(targetMonth)
            //               orderby a.PurchaseDate, a.SalesCarNumber
            //               select a;

            //        ret = carPurchaseStockList;
            //        break;

            //    default:    //それ以外
            //        //なし
            //        break;
            //}

            //// 出口
            //return ret;
        }

         /// <summary>
        /// 車両管理データ検索(未締めデータ)
        /// </summary>
        /// <param name="processdate">対象年月</param>
        /// <returns>車両管理データ検索結果</returns>
        /// <history>
        /// Add 2016/11/30 arc yano #3659 車両管理 新規作成
        /// </history>
        public List<CarStock> GetListByProcessDate(string processdate)
        {
            List<CarStock> retList = new List<CarStock>();

            //データ取得
            retList = 
            (
                from a in db.CarStock
                where a.ProcessDate.Equals(processdate)
                select a
            ).ToList();

            return retList;
        }
    }
}
