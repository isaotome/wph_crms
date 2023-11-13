using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
//Add 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao {
    public class ServiceSalesOrderDao {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public ServiceSalesOrderDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// サービス伝票データを取得する（PK指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <returns></returns>
        public ServiceSalesHeader GetByKey(string slipNumber, int revisionNumber) {
            var query =
                (from a in db.ServiceSalesHeader
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber.Equals(revisionNumber)
                select a).FirstOrDefault();
            return query; 
        }

        /// <summary>
        /// 部品番号から受注数量を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <param name="partsNumber">数量</param>
        /// <returns>数量:Quantity ,件数:Count</returns>
        public Dictionary<string,decimal> GetPartsQuantityByPartsNumber(string slipNumber, int revisionNumber, string partsNumber) {
            var query =
                from a in db.ServiceSalesLine
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber.Equals(revisionNumber)
                && (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Equals(partsNumber))
                //&& !a.DelFlag.Equals("1")
                select a;
            Dictionary<string, decimal> ret = new Dictionary<string, decimal>();
            ret.Add("Quantity", query.Sum(x => x.Quantity) ?? 0);
            ret.Add("Count", query.Count());
            return ret;

        }

        public List<ServiceSalesLine> GetNotExistListByKey(string slipNumber, int revisionNumber,EntitySet<ServiceSalesLine> partsList) {
            var query =
                from a in db.ServiceSalesLine
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber.Equals(revisionNumber)
                select a;

            ParameterExpression param = Expression.Parameter(typeof(ServiceSalesLine), "x");
            BinaryExpression body = null;
            foreach (var b in partsList) {
                MemberExpression left = Expression.Property(param, "PartsNumber");
                ConstantExpression right = Expression.Constant(b.PartsNumber, typeof(string));
                body = Expression.NotEqual(left, right);
                query = query.Where(Expression.Lambda<Func<ServiceSalesLine, bool>>(body, param));
            }
            return query.ToList<ServiceSalesLine>();
        }
        /// <summary>
        /// サービス伝票データ検索
        /// (ページング対応）
        /// </summary>
        /// <param name="areaCondition">サービス伝票検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>サービス伝票データ検索結果</returns>
        public PaginatedList<ServiceSalesHeader> GetListByCondition(ServiceSalesHeader salesHeaderCondition, Employee employee, int? PageIndex, int? PageSize) {
            //Add 2015/07/21 arc nakayama ダーティーリード（ReadUncommitted）追加
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                return new PaginatedList<ServiceSalesHeader>(GetQueryable(salesHeaderCondition), PageIndex ?? 0, PageSize ?? 0);
            }
       }

        /// <summary>
        /// サービス伝票データ検索
        /// </summary>
        /// <param name="condition">サービス伝票検索条件</param>
        /// <returns>サービス伝票リストデータ</returns>
        /// <history>
        /// 2018/04/26 arc yano #3003 電話番号検索で携帯電話もヒットして欲しい
        /// </history>
        public List<ServiceSalesHeader> GetListByCondition(DocumentExportCondition condition) {
            ServiceSalesHeader salesHeaderCondition = new ServiceSalesHeader();
            salesHeaderCondition.SetAuthCondition(condition.AuthEmployee);
            salesHeaderCondition.ServiceOrderStatus = condition.ServiceOrderStatus;
            salesHeaderCondition.SalesOrderDateFrom = condition.TermFrom;
            salesHeaderCondition.SalesOrderDateTo = condition.TermTo;

            return GetQueryable(salesHeaderCondition).ToList();
        }

        /// <summary>
        /// サービス伝票の検索条件式を取得する
        /// </summary>
        /// <param name="salesHeaderCondition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2018/08/14 yano #3912 サービス伝票検索　検索条件に入力したイベントの名称が検索実行後、消える
        /// </history>
        private IQueryable<ServiceSalesHeader> GetQueryable(ServiceSalesHeader salesHeaderCondition){
            //伝票番号
            string slipNumber = salesHeaderCondition.SlipNumber;

            //見積日
            DateTime? quoteDateFrom = salesHeaderCondition.QuoteDateFrom;
            DateTime? quoteDateTo = salesHeaderCondition.QuoteDateTo;

            //受注日
            DateTime? salesOrderDateFrom = salesHeaderCondition.SalesOrderDateFrom;
            DateTime? salesOrderDateTo = salesHeaderCondition.SalesOrderDateTo;

            //納車日
            DateTime? salesDateFrom = salesHeaderCondition.SalesDateFrom;
            DateTime? salesDateTo = salesHeaderCondition.SalesDateTo;

            //部門コード
            string departmentCode = salesHeaderCondition.DepartmentCode;

            //担当者コード(OR条件)
            string frontEmployeeCode = "";
            try { frontEmployeeCode = salesHeaderCondition.FrontEmployee.EmployeeCode; } catch (NullReferenceException) { }

            string receiptionEmployeeCode = "";
            try { receiptionEmployeeCode = salesHeaderCondition.ReceiptionEmployee.EmployeeCode; } catch (NullReferenceException) { }
            

            //顧客コード
            string customerCode = "";
            try { customerCode = salesHeaderCondition.Customer.CustomerCode; } catch (NullReferenceException) { }

            //顧客名
            string customerName = "";
            try { customerName = salesHeaderCondition.Customer.CustomerName; } catch (NullReferenceException) { }

            //顧客名（カナ）
            string customerNameKana = "";
            try { customerNameKana = salesHeaderCondition.Customer.CustomerNameKana; } catch (NullReferenceException) { }

            //請求先コード
            string customerClaimCode = salesHeaderCondition.CustomerClaimCode;

            //請求先名
            string customerClaimName = salesHeaderCondition.CustomerClaimName;

            //ブランド名
            string brandName = salesHeaderCondition.CarBrandName;

            //電話番号
            string telNumber = "";
            try { telNumber = salesHeaderCondition.Customer.TelNumber; } catch (NullReferenceException) { }

            //登録番号(プレート)
            string plateNumber = "";
            try { plateNumber = salesHeaderCondition.RegistrationNumberPlate; } catch (NullReferenceException) { }

            //車台番号下9桁
            string vin = salesHeaderCondition.Vin;

            //車台番号フル桁
            string vinFull = salesHeaderCondition.VinFull;

            //ステータス
            string serviceOrderStatus = salesHeaderCondition.ServiceOrderStatus;

            //イベント（OR条件）
            string campaignCode1 = salesHeaderCondition.CampaignCode1;
            string campaignCode2 = salesHeaderCondition.CampaignCode2;

            string delFlag = salesHeaderCondition.DelFlag;

            //主作業コード
            string serviceWorkCode = salesHeaderCondition.ServiceWorkCode;

            //会社コード
            string authCampanyCode = salesHeaderCondition.AuthCompanyCode;

            //車両預り中フラグ
            bool keepsCarFlag = salesHeaderCondition.KeepsCarFlag;      //Add 2017/09/04 arc yano #3786

            //検索条件に合致する伝票全てを取得する
            var query =
                from a in db.ServiceSalesHeader
                from c in db.Customer.Where(x => x.CustomerCode == a.CustomerCode).DefaultIfEmpty()
                orderby a.SlipNumber descending
                where
                    //(string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Substring(0,8).Equals(slipNumber))
                    // 2011.11.16 伝票番号を部分検索に変更
                    (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Contains(slipNumber))
                    && (quoteDateFrom == null || DateTime.Compare(a.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, quoteDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (quoteDateTo == null || DateTime.Compare(a.QuoteDate ?? DaoConst.SQL_DATETIME_MAX, quoteDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (salesOrderDateFrom == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN, salesOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (salesOrderDateTo == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MAX, salesOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (salesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (salesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && ((string.IsNullOrEmpty(frontEmployeeCode) || a.FrontEmployee.EmployeeCode.Equals(frontEmployeeCode)) || (string.IsNullOrEmpty(receiptionEmployeeCode) || a.ReceiptionEmployee.EmployeeCode.Equals(receiptionEmployeeCode)))
                    && (string.IsNullOrEmpty(brandName) || a.CarGrade.Car.Brand.CarBrandName.Contains(brandName))
                    && (string.IsNullOrEmpty(plateNumber) || a.RegistrationNumberPlate.Equals(plateNumber))
                    // Mod arc amii 2014/09/08 車台番号入力制限対応 #3085 下8桁条件の制限を削除
                    && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                    //&& (string.IsNullOrEmpty(vin) || a.Vin.Substring(a.Vin.Length - 8, 8).Equals(vin))
                    && (string.IsNullOrEmpty(vinFull) || a.Vin.Equals(vinFull))
                    // Mod 2015/05/07 arc nakayama #3083_サービスで伝票検索出来ない
                    && (string.IsNullOrEmpty(customerCode) || a.CustomerCode.Equals(customerCode))
                    && (string.IsNullOrEmpty(customerName) || c.CustomerName.Contains(customerName))
                    && (string.IsNullOrEmpty(customerNameKana) || c.CustomerNameKana.Contains(customerNameKana))
                    && (string.IsNullOrEmpty(telNumber) || (c.TelNumber.Substring(c.TelNumber.Length - 4, 4).Equals(telNumber) || c.MobileNumber.Substring(c.MobileNumber.Length - 4, 4).Equals(telNumber)))    //2018/04/26 arc yano #3003
                    //&& (string.IsNullOrEmpty(telNumber) || c.TelNumber.Substring(c.TelNumber.Length - 4, 4).Equals(telNumber))
                    && (string.IsNullOrEmpty(serviceOrderStatus) || a.ServiceOrderStatus.Equals(serviceOrderStatus))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (    //Add 2018/08/14 yano #3912
                             ( string.IsNullOrWhiteSpace(campaignCode1) && string.IsNullOrWhiteSpace(campaignCode2)) ||
                             (
                                (!string.IsNullOrWhiteSpace(campaignCode1) && campaignCode1.Equals(a.CampaignCode1)) ||
                                (!string.IsNullOrWhiteSpace(campaignCode1) && campaignCode2.Equals(a.CampaignCode2))
                             )
                        )
                    && (from b in db.ServiceSalesLine
                        where (string.IsNullOrEmpty(serviceWorkCode) || b.ServiceWorkCode.Equals(serviceWorkCode))
                        && (string.IsNullOrEmpty(customerClaimCode) || b.CustomerClaimCode.Equals(customerClaimCode))
                        && (string.IsNullOrEmpty(customerClaimName) || b.CustomerClaim.CustomerClaimName.Contains(customerClaimName))
                        select new { b.SlipNumber, b.RevisionNumber }
                        ).Contains(new { a.SlipNumber, a.RevisionNumber })
                    && (salesHeaderCondition.WithoutAkaden ? !a.SlipNumber.Contains("-") : true)
                 
                select a;

            ParameterExpression param = Expression.Parameter(typeof(ServiceSalesHeader), "x");
            Expression depExpression = salesHeaderCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<ServiceSalesHeader, bool>>(depExpression, param));
            }
            Expression offExpression = salesHeaderCondition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<ServiceSalesHeader, bool>>(offExpression, param));
            }
            Expression comExpression = salesHeaderCondition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<ServiceSalesHeader, bool>>(comExpression, param));
            }

            return query;
            
        }

         
        /// <summary>
        /// 指定した改訂番号以下のサービス伝票リストを取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号（対象を含む）</param>
        /// <returns>サービス伝票リストデータ</returns>
        public List<ServiceSalesHeader> GetListByLessThanRevision(string slipNumber, int? revisionNumber) {
            var query =
                from a in db.ServiceSalesHeader
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber <= revisionNumber
                //&& a.DelFlag.Equals("0")
                select a;
            return query.ToList<ServiceSalesHeader>();
        }
        /// <summary>
        /// 伝票番号から最新伝票を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>１件のサービス伝票</returns>
        public ServiceSalesHeader GetBySlipNumber(string slipNumber) {
            var query =
                (from a in db.ServiceSalesHeader
                 where a.SlipNumber.Equals(slipNumber)
                 && !a.DelFlag.Equals("1")
                 orderby a.RevisionNumber descending
                 select a).FirstOrDefault();

            return query;
        }


        // Add 2015/03/17 arc nakayama 伝票修正対応
        /// <summary>
        /// 修正中の伝票情報取得（レコードがあったら修正中）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>１件のサービス伝票</returns>
        public ModificationControl GetModificationStatus(string slipNumber, int RevisionNumber)
        {
            var query =
                (from a in db.ModificationControl
                where slipNumber.Equals(a.SlipNumber)
                && RevisionNumber.Equals(a.RevisionNumber)
                select a).FirstOrDefault();

            return query;
        }

        // Add 2015/03/24 arc nakayama 伝票修正対応
        /// <summary>
        /// 修正中の伝票情報取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>複数件のサービス伝票</returns>
        public List<ModificationControl> GetModificationStatusAll(string slipNumber)
        {
            var query =
                from a in db.ModificationControl
                 where (string.IsNullOrEmpty(slipNumber) || slipNumber.Equals(a.SlipNumber))
                 select a;

            return query.ToList<ModificationControl>();
        }

        // Add 2015/03/17 arc nakayama 伝票修正対応
        /// <summary>
        /// 赤黒履歴から検索を行う
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>１件の赤黒履歴</returns>
        public AkakuroReason GetAkakuroReason(string SlipNumber)
        {
            var query =
                (from a in db.AkakuroReason
                 where SlipNumber.Equals(a.SlipNumber)
                 select a).FirstOrDefault();

            return query;
        }

        // Add 2015/03/18 arc nakayama 伝票修正対応
        /// <summary>
        /// 修正履歴１件を取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>修正履歴</returns>
        public ModifiedReason GetLatestModifiedReason(string slipNumber)
        {
            var query =
                (from a in db.ModifiedReason
                 where slipNumber.Equals(a.SlipNumber)
                 select a).FirstOrDefault();

            return query;
        }

        //Add 2015/03/18 arc nakayama 伝票修正対応　修正履歴を取得する
        /// <summary>
        /// 修正履歴を取得する（該当伝票の全履歴）
        /// </summary>
        /// <param name="code">伝票番号</param>
        /// <returns>修正履歴</returns>
        public List<ModifiedReason> GetModifiedReason(string slipNumber)
        {
            var query =
                from a in db.ModifiedReason
                 where slipNumber.Equals(a.SlipNumber)
                 orderby a.CreateDate descending
                 select a;

            return query.ToList<ModifiedReason>();

        }

        // Add 2015/03/18 arc nakayama 伝票修正対応 納車年月と部門コードから伝票を検索する
        /// <summary>
        /// 納車年月と部門コードから伝票を検索する
        /// </summary>
        /// <param name="code">対象年月(YYYY/MM/DD)</param>
        /// <param name="code">部門コード</param>
        /// <returns>伝票データ</returns>
        public List<ServiceSalesHeader> GetBySalesDateAndTargetDate(DateTime targetMonth, string departmentCode)
        {
            var query =
                from a in db.ServiceSalesHeader
                where (a.SalesDate.Value.Year <= targetMonth.Year && a.SalesDate.Value.Month <= targetMonth.Month)
                && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && a.DelFlag.Equals("0")
                select a;

            return query.ToList<ServiceSalesHeader>();
        }

        // Add 2015/03/27 arc nakayama 伝票修正対応 伝票番号・改訂番号・ラインナンバーから部品の数量を取得する
        /// <summary>
        /// 伝票番号・改訂番号・ラインナンバーから部品の数量を取得する
        /// </summary>
        /// <param name="code">伝票番号</param>
        /// <param name="code">改訂番号</param>
        /// <param name="code">ラインナンバー</param>
        /// <returns>明細データ</returns>
        public List<ServiceSalesLine> GetPartsQuantity(string slipNumber, int RevisionNumber, string PartsNumber)
        {
            var query =
                from a in db.ServiceSalesLine
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber.Equals(RevisionNumber)
                && (string.IsNullOrEmpty(PartsNumber) || a.PartsNumber.Equals(PartsNumber))
                select a;

            return query.ToList<ServiceSalesLine>();
        }
        /// <summary>
        /// 伝票番号・部品番号から最新のサービス伝票の明細を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="orderType">オーダー区分</param>
        /// <returns>明細データ</returns>
        /// <history>
        /// Mod 2016/11/11 arc yano #3656 部品番号、発注区分(=在庫判断)をデフォルト引数に変更
        /// Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加
        /// </history>
        public List<ServiceSalesLine> GetLineByPartsNumber(string slipNumber, string partsNumber = null, string orderType = null)
        {
            var query =
                from a in db.ServiceSalesLine
                where a.SlipNumber.Equals(slipNumber)
                && (string.IsNullOrWhiteSpace(partsNumber) || (a.PartsNumber.Equals(partsNumber)))
                && (string.IsNullOrWhiteSpace(orderType) || (a.StockStatus.Equals(orderType)))
                && a.DelFlag.Equals("0")
                select a;

            return query.ToList<ServiceSalesLine>();
        }
        
         /// <summary>
        /// 預り車両一覧取得
        /// </summary>
        /// <param name="condition">サービス伝票検索条件</param>
        /// <returns>サービス伝票リストデータ</returns>
        /// <history>
        /// 2017/09/04 arc yano #3786 預かり車Excle出力対応 預り車フラグによる絞込の追加
        /// </history>
        public List<CarStorageList> GetCarStorageList(DocumentExportCondition condition)
        {
            //カウンタ
            decimal cnt = 0m;

            //リスト
            List<CarStorageList> list = new List<CarStorageList>();

            //部門コード
            string departmentCode = condition.DepartmentCode;

            //車両預り中フラグ
            bool keepsCarFlag = true;

            //検索条件に合致する伝票全てを取得する
            var query = 
                    from a in db.ServiceSalesHeader
                    where
                    (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Contains(departmentCode))
                    && a.KeepsCarFlag.Equals(keepsCarFlag)
                    && a.Vin != null && !a.Vin.Equals("")
                    && a.DelFlag.Equals("0")
                    && !(
                        a.ServiceOrderStatus.Equals("007") ||
                        a.ServiceOrderStatus.Equals("009") ||
                        a.ServiceOrderStatus.Equals("010")
                        )
                orderby
                    a.ArrivalPlanDate
                select a;

            foreach(var a in query)
            {
                CarStorageList rec = new CarStorageList();

                rec.StrCarStorage = "預り";                                                       //預り文言
                rec.Index = ++cnt;                                                                //行番号
                rec.SalesCarNumber = a.SalesCarNumber;                                            //管理番号
                rec.ArrivalPlanDate = a.ArrivalPlanDate;                                          //入庫日
                rec.CarName = a.CarName;                                                          //車種名
                rec.RegistrationNumber = (a.MorterViecleOfficialCode ?? "") + " " +  (a.RegistrationNumberType ?? "") + " " + (a.RegistrationNumberKana ?? "") + " " + (a.RegistrationNumberPlate ?? "");  //登録番号
                rec.Vin = a.Vin;                                                                  //車台番号
                rec.CustomerName = a.Customer.CustomerName;                                       //顧客名
                rec.SlipNumber = a.SlipNumber;                                                    //伝票番号
                rec.ServiceOrderStatusName = a.c_ServiceOrderStatus.Name;                         //伝票ステータス名

                list.Add(rec);
            }

            return list;
        }


        /// <summary>
        /// 部品番号から当該部門の引当済数合計を取得する
        /// </summary>
        /// <param name="DepartmentCode">部門コード</param>
        /// <param name="partsNumber">部品番号</param>
        /// <returns>引当済数(合計)</returns>
        public decimal GetPartsProvisionQuantityByPartsNumber(string departmentCode, string partsNumber)
        {
            var query =
                from a in db.ServiceSalesLine
                join b in db.ServiceSalesHeader on new { a.SlipNumber, a.RevisionNumber } equals new { b.SlipNumber, b.RevisionNumber}
                where b.DepartmentCode.Equals(departmentCode)
                && a.PartsNumber.Equals(partsNumber)
                && a.DelFlag.Equals("0")
                && b.DelFlag.Equals("0")
                && (b.ServiceOrderStatus.Equals("002") || b.ServiceOrderStatus.Equals("003") || b.ServiceOrderStatus.Equals("004") || b.ServiceOrderStatus.Equals("005")) //伝票ステータス=「受注」～「納車確認書印刷済」
                select (a.ProvisionQuantity ?? 0);

            //decimal ret = (query.Sum(x => x.ProvisionQuantity ?? 0) == null ? 0m : query.Sum(x => x.ProvisionQuantity ?? 0));

            return (query.Count() > 0 ? query.Sum(x => x) : 0);
        }
        // <summary>
        /// 対象の部品の過去の発注数を確認する
        /// 赤黒伝票の伝票の場合は、黒伝票、元伝票全てを検索して差分をチェックする
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="stockStatus">発注区分</param>
        /// <returns>発注済数</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        public decimal? GetOutputQuantity(string slipNumber, string partsNumber, string stockStatus)
        {
            var query =
                from a in db.ServiceSalesLine
                where a.SlipNumber.Contains(slipNumber)
                && !a.SlipNumber.Contains("-1")
                && a.PartsNumber.Equals(partsNumber)
                && (a.StockStatus != null && a.StockStatus.Equals(stockStatus))
                && (a.OutputFlag != null && a.OutputFlag.Equals("1"))
                group a by new { SlipNumber = a.SlipNumber, RevisionNumber = a.RevisionNumber, PartsNumber = a.PartsNumber, StockStatus = a.StockStatus } into b
                orderby b.Key.SlipNumber descending , b.Key.RevisionNumber descending
                select new
                {
                    Quantity = b.Sum(x => (x.Quantity ?? 0))
                };

            decimal? orderedQuantity = null;

            //対象のレコードが存在する場合は
            if (query.FirstOrDefault() != null)
            {
                orderedQuantity = query.FirstOrDefault().Quantity;
            }
                
            return orderedQuantity;
        }

        // <summary>
        /// 主作業単位でサービス伝票一覧を表示する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="partsNumber">部品番号</param>
        /// <param name="stockStatus">発注区分</param>
        /// <returns>発注済数</returns>
        /// <history>
        /// 2017/10/19 arc yano #3803 サービス伝票 部品発注書の出力 新規作成
        /// </history>
        public IQueryable<GetServiceSalesSlipResult> GetServiceSalesSlip(GetServiceSalesSlipResult condition)
        {
            var query = db.GetServiceSalesSlip(condition.SlipNumber, condition.ArrivalPlanDateFrom, condition.ArrivalPlanDateTo, condition.DepartmentCode, condition.ServiceOrderStatus, condition.ServiceWorkCode, condition.CustomerCode, condition.CustomerName);

            return query.ToList().AsQueryable();
        }
    }
}