using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.Linq;

namespace CrmsDao
{
    public class V_RevenueResultDao
    {
        //----------------------------------------------------
        //機能　：入金実績リスト表示用データアクセスクラス
        //作成日：
        //作成者：
        //備考　：
        //----------------------------------------------------
        
        
         /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

         /// <summary>
        /// 定数
        /// </summary>
        /*------口座種別------*/
        private const string ATYPE_CASH     = "001";        //現金
        private const string ATYPE_TRANSFER = "002";        //振込
        private const string ATYPE_CARD     = "003";        //カード
        private const string ATYPE_LOAN     = "004";        //ローン
        private const string ATYPE_OTHER    = "009";        //その他
        private const string ATYPE_REMAINDEBT = "012";        //残債
        private const string ATYPE_TRADE      = "013";        //下取

        /// <summary>
        ///入金実績一覧の取得
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_RevenueResultDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }
        public PaginatedList<V_RevenueResult> GetListByCondition(V_RevenueResult Condition, int? pageIndex, int? pageSize) 
        {
            // Mod 2015/04/09 arc nakayama 入金実績リスト指摘事項修正　画面の項目数が減ったため一部削除
            IQueryable<V_RevenueResult> revenueResultList =
                from a in db.V_RevenueResult
                where (string.IsNullOrEmpty(Condition.SlipNumber) || a.SlipNumber.Contains(Condition.SlipNumber))
                && (string.IsNullOrEmpty(Condition.DepartmentCode) || a.DepartmentCode.Equals(Condition.DepartmentCode))
                && (Condition.SalesOrderDateFrom == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN, Condition.SalesOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (Condition.SalesOrderDateTo == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MAX, Condition.SalesOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(Condition.CustomerCode) || a.CustomerCode.Equals(Condition.CustomerCode))
                select a;
            //受注日によって並び替え 
            revenueResultList = revenueResultList.OrderByDescending(x => x.SalesOrderDate).ThenBy(x => x.SlipNumber);

            // ページング制御情報を付与した顧客データの返却
            PaginatedList<V_RevenueResult> ret = new PaginatedList<V_RevenueResult>(revenueResultList, pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// 車両情報取得
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入金予定一覧</returns>
        /// <history>
        /// 2018/08/22 yano #3927 入金実績詳細　車両情報の表示が販売車両ではなく保有車両
        /// </history>
        public SalesCar GetCarInformation(string customerCode, string slipNumber)
        {
            SalesCar salesCar = new SalesCar();
            SalesCar salesCarbySlipNumber = null;

            if (!string.IsNullOrEmpty(slipNumber))
            {
                 
                salesCarbySlipNumber =
                        (
                          from a in db.SalesCar
                          where
                          (
                              from b in db.CarSalesHeader
                              where b.SlipNumber.Equals(slipNumber)
                              && b.DelFlag.Equals("0")
                              select b.SalesCarNumber
                          ).Contains(a.SalesCarNumber)
                          && a.DelFlag.Equals("0")
                          select a
                        ).FirstOrDefault();

                //車両販売情報あるか
                if (salesCarbySlipNumber != null)
                {
                    salesCar = salesCarbySlipNumber;
                }
                else //無い場合は伝票の入力値から取得する
                {
                    CarSalesHeader ch = new CarSalesOrderDao(db).GetBySlipNumber(slipNumber);

                    //車両伝票の場合
                    if (ch != null)
                    {
                        salesCar.MakerName = ch.MakerName;                                  //メーカ名
                        salesCar.CarName = ch.CarName;                                      //車種名
                        salesCar.MorterViecleOfficialCode = ch.MorterViecleOfficialCode;    //陸運局コード
                        salesCar.RegistrationNumberType = "";                               //登録番号(種別)
                        salesCar.RegistrationNumberKana = "";                               //登録番号(かな)
                        salesCar.RegistrationNumberPlate = "";                              //登録番号(プレート)
                        salesCar.Vin = ch.Vin;                                              //車台番号
                    }
                    else
                    {
                        //サービス伝票を検索

                        salesCarbySlipNumber =
                         (
                           from a in db.SalesCar
                           where
                           (
                               from b in db.ServiceSalesHeader
                               where b.SlipNumber.Equals(slipNumber)
                               && b.DelFlag.Equals("0")
                               select b.SalesCarNumber
                           ).Contains(a.SalesCarNumber)
                           && a.DelFlag.Equals("0")
                           select a
                         ).FirstOrDefault();

                        if (salesCarbySlipNumber != null)
                        {
                            salesCar = salesCarbySlipNumber;
                        }
                    }
                }
            }

            //メーカー名
            salesCar.MakerName = (salesCar.CarGrade != null && salesCar.CarGrade.Car != null &&  salesCar.CarGrade.Car.Brand != null && salesCar.CarGrade.Car.Brand.Maker != null ? salesCar.CarGrade.Car.Brand.Maker.MakerName : salesCar.MakerName);

            //車種名
            salesCar.CarName = salesCar.CarGrade != null && salesCar.CarGrade.Car != null ? salesCar.CarGrade.Car.CarName : salesCar.CarName;

            return salesCar;

            /*
            SalesCar salesCar = new SalesCar();
            SalesCar salesCarbyCustomer = null;
            SalesCar salesCarbySlipNumber = null;

            salesCarbyCustomer =
                        (
                            from a in db.SalesCar
                            where a.UserCode.Equals(customerCode)
                            select a
                        ).FirstOrDefault();

            if (!string.IsNullOrEmpty(slipNumber))
            {
                salesCarbySlipNumber =
                        (
                          from a in db.SalesCar
                          where
                          (
                              from b in db.CarSalesHeader
                              where b.SlipNumber.Equals(slipNumber)
                              && b.DelFlag.Equals("0")
                              select b.SalesCarNumber
                          ).Contains(a.SalesCarNumber)
                          && a.DelFlag.Equals("0")
                          select a
                        ).FirstOrDefault();

            }

            if (salesCarbySlipNumber != null)
            {
                salesCar = salesCarbySlipNumber;
            }
            else
            {
                salesCar = salesCarbyCustomer;
            }

            return salesCar;
            */
        }


       /// <summary>
        /// 入金予定一覧取得
        /// （ページング対応）
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>入金予定一覧</returns>
        public PaginatedList<V_ReceiptPlanList> GetReceiptPlanList(string customerCode, string slipNumber, int? pageIndex, int? pageSize, bool SummaryFlag)
        {
            // ページング制御情報を付与した入金予定データの返却
            PaginatedList<V_ReceiptPlanList> ret = new PaginatedList<V_ReceiptPlanList>(GetReceiptPlan(customerCode, slipNumber, SummaryFlag), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// 入金予定一覧を取得する
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns></returns>
        public IQueryable<V_ReceiptPlanList> GetReceiptPlan(string customerCode, string slipNumber, bool SummaryFlag)
        {
            var query = db.GetReceiptPlan(customerCode, slipNumber, SummaryFlag);

            List<V_ReceiptPlanList> list = new List<V_ReceiptPlanList>();

            foreach (V_ReceiptPlanList a in query)
            {
                list.Add(a);
            }


            return list.AsQueryable();
        }

        /// <summary>
        /// 入金実績一覧取得
        /// （ページング対応）
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>入金実績一覧</returns>
        public RecieptPlanReSultList GetReceiptResultList(string customerCode, string slipNumber, int? pageIndex, int? pageSize)
        {
            // ページング制御情報を付与した入金実績データの返却
            RecieptPlanReSultList ret = new RecieptPlanReSultList();

            ret = GetReceiptResult(customerCode, slipNumber);

            ret.pagelist = new PaginatedList<RecieptPlanReSult>(ret.list.AsQueryable(), pageIndex ?? 0, pageSize ?? 0);
           
            // 出口
            return ret;
        }

        /// <summary>
        /// 入金実績一覧を取得する
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入金予定・実績一覧</returns>
        public RecieptPlanReSultList GetReceiptResult(string customerCode, string slipNumber)
        {
            var query = db.GetReceiptResult(customerCode, slipNumber);

            RecieptPlanReSultList prList = new RecieptPlanReSultList();
            decimal ? tempAmountCash = 0;
            decimal ? tempAmountTransfer = 0;
            decimal ? tempAmountCard = 0;
            decimal ? tempAmountLoan = 0;
            decimal ? tempAmountOther = 0;
            decimal? tempAmount = 0;
            decimal? tempAmountTrade = 0;
            decimal? tempAmountRemainDebt = 0;

            //画面表示用のクラスに値をセット
            foreach (var a in query)
            {
                RecieptPlanReSult rec = new RecieptPlanReSult();

                rec.ST = a.ST;                      //ST

                if (a.ST.Equals("0"))               //区分
                {
                    rec.STName = "請求金額";
                }
                else
                {
                    rec.STName = "入金金額";
                }

                rec.SlipNumber = a.SlipNumber;      //伝票番号
                rec.ReceiptDate = a.ReceiptDate;    //入金日
                rec.AccountType = a.AccountType;    //口座種別
                rec.AccountCode = a.AccountCode;    //科目コード
                rec.AccountName = a.AccountName;    //科目名
                rec.Amount = a.Amount;
                
                //金額
                if (a.ST.Equals("0"))
                {
                    tempAmount = a.Amount;
                }
                else{
                    tempAmount = a.Amount * -1;
                }
                

                if (rec.Amount == null)
                {
                    continue;   //処理スキップ（次のレコードへ）
                }
                
                //口座種別毎の金額の合計値の計算
                switch (a.AccountType)
                {

                    case ATYPE_CASH:        //現金
                        tempAmountCash += tempAmount;
                        break;

                    case ATYPE_TRANSFER:    //振込
                        tempAmountTransfer += tempAmount;
                        break;

                    case ATYPE_CARD:        //カード
                        tempAmountCard += tempAmount;
                        break;

                    case ATYPE_LOAN:        //ローン
                        tempAmountLoan += tempAmount;
                        break;

                    case ATYPE_REMAINDEBT: //残債
                        tempAmountRemainDebt += tempAmount;
                        break;

                    case ATYPE_TRADE:     //下取仕入
                        tempAmountTrade += tempAmount;
                        break;

                    case ATYPE_OTHER:       //その他
                        tempAmountOther += tempAmount;
                        break;

                    default:                //上記以外
                        //処理なし
                        break;
                }

                prList.list.Add(rec);   //レコード設定
            }

            //合計値の設定
            prList.AmountCash = tempAmountCash;
            prList.AmountTransfer = tempAmountTransfer;
            prList.AmountCard = tempAmountCard;
            prList.AmountLoan = tempAmountLoan;
            prList.AmountOther = tempAmountOther;
            prList.AmountRemainDebt = tempAmountRemainDebt;
            prList.AmountTrade = tempAmountTrade;

            return prList;
        }

        /// <summary>
        /// カード入金実績一覧取得
        /// （ページング対応）
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>入金実績一覧</returns>
        public PaginatedList<V_ReceiptList> GetReceiptResultCardList(string customerCode, string slipNumber, int? pageIndex, int? pageSize)
        {
            // ページング制御情報を付与したカード入金実績データの返却
            PaginatedList<V_ReceiptList> ret = new PaginatedList<V_ReceiptList>();

            ret = new PaginatedList<V_ReceiptList>(GetResultCardList(customerCode, slipNumber), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// カード入金実績一覧取得（伝票の顧客指定）
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <returns>カード入金実績一覧</returns>
        public IQueryable<V_ReceiptList> GetResultCardList(string customerCode, string slipNumber)
        {
            var query =
                from a in db.V_ReceiptList
                orderby a.JournalDate, a.SlipNumber, a.AccountType
                where
                    (
                        from b in db.V_ALL_SalesOrderList
                        where b.CustomerCode.Equals(customerCode)
                        select b.SlipNumber
                    ).Contains(a.SlipNumber)
                && (string.IsNullOrEmpty(slipNumber) || slipNumber.Equals(a.SlipNumber))
                && !a.AccountType.Equals("099") //破棄
                && a.AccountType.Equals("011")  //カード会社からの入金
                select a;

            return query;
        }


        /// <summary>
        /// 入金履歴履歴一覧取得
        /// （ページング対応）
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>入金履歴一覧</returns>
        public PaginatedList<V_ReceiptList> GetReceiptHistoryList(string customerCode, string slipNumber, int? pageIndex, int? pageSize)
        {
            // ページング制御情報を付与した入金予定データの返却
            PaginatedList<V_ReceiptList> ret = new PaginatedList<V_ReceiptList>();

            ret = new PaginatedList<V_ReceiptList>(GetReceiptHistory(customerCode, slipNumber), pageIndex ?? 0, pageSize ?? 0);

            // 出口
            return ret;
        }

        /// <summary>
        /// 入金履歴一覧を取得する
        /// </summary>
        /// <param name="customerCode">顧客コード</param>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>入金実績一覧</returns>
        public IQueryable<V_ReceiptList> GetReceiptHistory(string customerCode, string slipNumber)
        {
            var query =
                from a in db.V_ReceiptList
                where
                    (
                        from b in db.V_ALL_SalesOrderList
                        where b.CustomerCode.Equals(customerCode)
                        select b.SlipNumber
                    ).Contains(a.SlipNumber)
                && (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Equals(slipNumber))
                orderby a.JournalDate
                select a;

            return query;
        }


    }
}
