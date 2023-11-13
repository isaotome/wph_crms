using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
//Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
using System.Transactions;

namespace CrmsDao
{
    public class CarSalesOrderDao {
        private CrmsLinqDataContext db;

        public CarSalesOrderDao(CrmsLinqDataContext dataContext) {
            this.db = dataContext;
        }
        /// <summary>
        /// 車両伝票データ取得（PK指定）
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号</param>
        /// <returns>エリアマスタデータ検索結果</returns>
        public CarSalesHeader GetByKey(string slipNumber, int? revisionNumber) {
            var query =
                (from a in db.CarSalesHeader
                 where a.SlipNumber.Equals(slipNumber)
                 && a.RevisionNumber == revisionNumber
                 select a).FirstOrDefault();
            return query;
        }
        /// <summary>
        /// 車両伝票データ検索
        /// （ページング対応）
        /// </summary>
        /// <param name="areaCondition">車両伝票検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">1ページあたりの表示行数</param>
        /// <returns>車両伝票データ検索結果</returns>
        public PaginatedList<CarSalesHeader> GetListByCondition(CarSalesHeader salesHeaderCondition, int PageIndex, int PageSize) {
            //Add 2015/07/28 arc nakayama ダーティーリード（ReadUncommitted）追加
            using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                //ページ表示分だけ返す
                return new PaginatedList<CarSalesHeader>(GetQueryable(salesHeaderCondition), PageIndex, PageSize);
            }
        }

        /// <summary>
        /// 車両伝票データ検索
        /// （帳票出力）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <history>
        /// 2020/11/16 yano #4070 【店舗管理帳票】車両伝票一覧のデータ抽出条件の誤り
        /// </history>
        public List<CarSalesHeader> GetListByCondition(DocumentExportCondition condition) {
            CarSalesHeader salesHeaderCondition = new CarSalesHeader();
            salesHeaderCondition.SalesOrderStatus = condition.SalesOrderStatus;
            salesHeaderCondition.SalesOrderDateFrom = condition.TermFrom;
            salesHeaderCondition.SalesOrderDateTo = condition.TermTo;
            salesHeaderCondition.Customer = new Customer();
            salesHeaderCondition.Customer.CustomerType = condition.CustomerType;
            salesHeaderCondition.SetAuthCondition(condition.AuthEmployee);

            salesHeaderCondition.DelFlag = condition.DelFlag;       //Add 2020/11/16 yano #4070
            
            //全件返す
            return GetQueryable(salesHeaderCondition).ToList();
        }

        /// <summary>
        /// 車両伝票データ検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns>車両伝票検索結果</returns>
        /// <history>
        /// 2020/01/14 yano #3982 【車両伝票一覧】使用者で伝票を検索できるようにして欲しい
        /// 2018/04/26 arc yano #3003 電話番号検索で携帯電話もヒットして欲しい
        /// </history>
        public List<CarSalesHeader> GetListByCondition(CarSalesHeader condition) {
            return GetQueryable(condition).ToList();
        }
        /// <summary>
        /// 車両伝票検索式を取得する
        /// </summary>
        /// <param name="salesHeaderCondition">車両伝票検索条件</param>
        /// <returns>IQUeryable</returns>
        /// <history>
        /// 2022/08/30 yano #4079【車両伝票入力】ブランド名で検索を行えない
        /// 2020/01/14 yano #3982車両伝票一覧】使用者で伝票を検索できるようにして欲しい
        /// </history>
        private IQueryable<CarSalesHeader> GetQueryable(CarSalesHeader salesHeaderCondition) {

            //伝票番号
            string slipNumber = salesHeaderCondition.SlipNumber;

            //見積日
            DateTime? quoteDateFrom = salesHeaderCondition.QuoteDateFrom;
            DateTime? quoteDateTo = salesHeaderCondition.QuoteDateTo;

            //受注日
            DateTime? salesOrderDateFrom = salesHeaderCondition.SalesOrderDateFrom;
            DateTime? salesOrderDateTo = salesHeaderCondition.SalesOrderDateTo;

            //部門コード
            string departmentCode = salesHeaderCondition.DepartmentCode;

            //担当者コード
            string employeeCode = "";
            try { employeeCode = salesHeaderCondition.Employee.EmployeeCode; } catch (NullReferenceException) { }

            //顧客コード
            string customerCode = "";
            try { customerCode = salesHeaderCondition.Customer.CustomerCode; } catch (NullReferenceException) { }

            //顧客名
            string customerName = "";
            try { customerName = salesHeaderCondition.Customer.CustomerName; } catch (NullReferenceException) { }

            //顧客種別
            string customerType = "";
            try { customerType = salesHeaderCondition.Customer.CustomerType; } catch (NullReferenceException) { }

            //ブランド名
            string brandName = salesHeaderCondition.CarBrandName;

            //電話番号
            string telNumber = "";
            try { telNumber = salesHeaderCondition.Customer.TelNumber; } catch (NullReferenceException) { }

            //ステータス
            string salesOrderStatus = salesHeaderCondition.SalesOrderStatus;

            //イベント（OR条件）
            string campaignCode1 = salesHeaderCondition.CampaignCode1;
            string campaignCode2 = salesHeaderCondition.CampaignCode2;

            string delFlag = salesHeaderCondition.DelFlag;

            //会社コード
            string authCompanyCode = salesHeaderCondition.AuthCompanyCode;

            //事業所コード
            /*string authOfficeCode = salesHeaderCondition.AuthOfficeCode;
            string authOfficeCode1 = salesHeaderCondition.AuthOfficeCode1;
            string authOfficeCode2 = salesHeaderCondition.AuthOfficeCode2;
            string authOfficeCode3 = salesHeaderCondition.AuthOfficeCode3;

            //部門コード
            string authDepartmentCode = salesHeaderCondition.AuthDepartmentCode;
            string authDepartmentCode1 = salesHeaderCondition.AuthDepartmentCode1;
            string authDepartmentCode2 = salesHeaderCondition.AuthDepartmentCode2;
            string authDepartmentCode3 = salesHeaderCondition.AuthDepartmentCode3;
            */
            //ホット管理
            string hotStatus = salesHeaderCondition.HotStatus;

            //車台番号
            string vin = salesHeaderCondition.Vin;

            DateTime? salesDateFrom = salesHeaderCondition.SalesDateFrom;
            DateTime? salesDateTo = salesHeaderCondition.SalesDateTo;

            //Add 2020/01/14 yano #3982
            //使用者コード
            string userCode = salesHeaderCondition.User != null ? salesHeaderCondition.User.CustomerCode : "";
            //使用者名
            string userName = "";
            try { userName = salesHeaderCondition.User.CustomerName; }
            catch (NullReferenceException) { }


            //検索条件に合致する伝票全てを取得する
            var query =
                from a in db.CarSalesHeader
                where
                    // 2011.11.16 伝票番号を部分検索に変更
                    // (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Substring(0,8).Equals(slipNumber))
                    (string.IsNullOrEmpty(slipNumber) || a.SlipNumber.Contains(slipNumber))
                    && (quoteDateFrom == null || DateTime.Compare(a.QuoteDate ?? DaoConst.SQL_DATETIME_MIN, quoteDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (quoteDateTo == null || DateTime.Compare(a.QuoteDate ?? DaoConst.SQL_DATETIME_MAX, quoteDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (salesOrderDateFrom == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN, salesOrderDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (salesOrderDateTo == null || DateTime.Compare(a.SalesOrderDate ?? DaoConst.SQL_DATETIME_MAX, salesOrderDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (string.IsNullOrEmpty(employeeCode) || a.Employee.EmployeeCode.Equals(employeeCode))
                    && (string.IsNullOrEmpty(customerCode) || a.Customer.CustomerCode.Contains(customerCode))
                    && (string.IsNullOrEmpty(customerName) || a.Customer.CustomerName.Contains(customerName))
                    && (string.IsNullOrEmpty(customerType) || a.Customer.CustomerType.Equals(customerType))
                    && (string.IsNullOrEmpty(telNumber) || (a.Customer.TelNumber.Substring(a.Customer.TelNumber.Length - 4, 4).Equals(telNumber) || a.Customer.MobileNumber.Substring(a.Customer.MobileNumber.Length - 4, 4).Equals(telNumber)))    //2018/04/26 arc yano #3003
                    //&& (string.IsNullOrEmpty(telNumber) || a.Customer.TelNumber.Substring(a.Customer.TelNumber.Length - 4, 4).Equals(telNumber))
                    && (string.IsNullOrEmpty(salesOrderStatus) || a.SalesOrderStatus.Equals(salesOrderStatus))
                    && (string.IsNullOrEmpty(hotStatus) || a.HotStatus.Equals(hotStatus))
                    && (string.IsNullOrEmpty(delFlag) || a.DelFlag.Equals(delFlag))
                    && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                    && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin))
                    && (salesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                    && (salesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                    && (!salesHeaderCondition.IsAkaKuro || (salesHeaderCondition.IsAkaKuro && !a.SlipNumber.Contains("-1") && !a.SlipNumber.Contains("-2")))
                    && (string.IsNullOrEmpty(userCode) || (a.UserCode != null && a.UserCode.Equals(userCode)))          //Add 2020/01/14 yano #3982
                    && (string.IsNullOrEmpty(userName) || a.User.CustomerName.Contains(userName))                       //Add 2020/01/14 yano #3982
                    && (string.IsNullOrEmpty(brandName) || a.CarBrandName.Contains(brandName))                          //Add 2022/08/30 yano #4079
                select a;

            ParameterExpression param = Expression.Parameter(typeof(CarSalesHeader), "x");
            Expression depExpression = salesHeaderCondition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<CarSalesHeader, bool>>(depExpression, param));
            }
            Expression offExpression = salesHeaderCondition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<CarSalesHeader, bool>>(offExpression, param));
            }
            Expression comExpression = salesHeaderCondition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<CarSalesHeader, bool>>(comExpression, param));
            }

            return query.OrderByDescending(x => x.SlipNumber);
        }


        /// <summary>
        /// 指定した改訂番号以下の車両伝票リストを取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <param name="revisionNumber">改訂番号（対象を含む）</param>
        /// <returns>車両伝票リストデータ</returns>
        public List<CarSalesHeader> GetListByLessThanRevision(string slipNumber, int? revisionNumber) {
            var query =
                from a in db.CarSalesHeader
                where a.SlipNumber.Equals(slipNumber)
                && a.RevisionNumber <= revisionNumber
                //&& !a.DelFlag.Equals("1")
                select a;
            return query.ToList<CarSalesHeader>();
        }

        /// <summary>
        /// 伝票番号から最新伝票を取得する
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>１件の車両伝票</returns>
        public CarSalesHeader GetBySlipNumber(string slipNumber) {
            var query =
                (from a in db.CarSalesHeader
                 where a.SlipNumber.Equals(slipNumber)
                 && !a.DelFlag.Equals("1")
                 orderby a.RevisionNumber descending
                 select a).FirstOrDefault();

            return query;
        }

        public PaginatedList<CarSalesHeader> GetCancelList(int pageIndex, int pageSize) {
            var query = from a in db.CarSalesHeader
                        where a.SlipNumber.Contains("-1")
                        && !(
                        from b in db.CarSalesHeader
                        where b.SlipNumber.Contains("-2")
                        select b.SlipNumber).Contains(a.SlipNumber)
                        && a.SalesCar.CarStatus.Equals("006")
                        select a;
            return new PaginatedList<CarSalesHeader>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// 指定した伝票番号が車両伝票かサービス伝票に存在するか
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>存在する場合TRUE</returns>
        public bool IsExistSlipNumber(string slipNumber) {
            var query = (from a in db.CarSalesHeader
                         where a.SlipNumber.Equals(slipNumber)
                         && a.DelFlag.Equals("0")
                         select new { a.SlipNumber })
                        .Union
                        (from b in db.ServiceSalesHeader
                         where b.SlipNumber.Equals(slipNumber)
                         && b.DelFlag.Equals("0")
                         select new { b.SlipNumber });
            return query.Count() > 0;
        }

        //Add 2014/10/23 arc yano 車両ステータス追加対応
        /// <summary>
        /// 伝票番号(左１桁='2')の最新伝票番号を取得する。
        /// </summary>
        /// <returns>最新の車両伝票番号(1件)</returns>
        public String GetLatestSlipNumber()
        {
            var query =
                (from a in db.CarSalesHeader
                 where a.SlipNumber.Substring(0,1).Equals("2")
                 //&& !a.DelFlag.Equals("1")
                 select a).Max(a => a.SlipNumber);

            return query;
        }

        /*
        /// <summary>
        /// 車両納車リスト取得
        /// </summary>
        /// <returns>期首から期末の納車リスト</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// 2017/03/09 arc nakayama #3723_納車リスト　新規作成
        /// </history>
        public List<GetCarSalesList_Result> GetCarSalesListResult(string targetyear, string salestype)
        {
            var Ret = db.GetCarSalesList(targetyear, salestype);

            List<GetCarSalesList_Result> RetList = new List<GetCarSalesList_Result>();
        
            foreach (var Data in Ret)
            {
                GetCarSalesList_Result CarSalesData = new GetCarSalesList_Result();
                
                CarSalesData.DepartmentCode = Data.DepartmentCode;
                CarSalesData.DepartmentName = Data.DepartmentName;
                
                CarSalesData.Jul_Cnt = Data.Jul_Cnt;
                CarSalesData.Aug_Cnt = Data.Aug_Cnt;
                CarSalesData.Sep_Cnt = Data.Sep_Cnt;
                CarSalesData.Oct_Cnt = Data.Oct_Cnt;
                CarSalesData.Nov_Cnt = Data.Nov_Cnt;
                CarSalesData.Dec_Cnt = Data.Dec_Cnt;
                CarSalesData.Jan_Cnt = Data.Jan_Cnt;
                CarSalesData.Feb_Cnt = Data.Feb_Cnt;
                CarSalesData.Mar_Cnt = Data.Mar_Cnt;
                CarSalesData.Apr_Cnt = Data.Apr_Cnt;
                CarSalesData.May_Cnt = Data.May_Cnt;
                CarSalesData.Jun_Cnt = Data.Jun_Cnt;
                RetList.Add(CarSalesData);

            }


            //Add 2017/10/14 arc yano #3790
            CarSalseList list = new CarSalseList();

            //店舗全体の月別の合計
            list.TotalJul_Cnt = RetList.Sum(x => x.Jul_Cnt);
            list.TotalAug_Cnt = RetList.Sum(x => x.Aug_Cnt);
            list.TotalSep_Cnt = RetList.Sum(x => x.Sep_Cnt);
            list.TotalOct_Cnt = RetList.Sum(x => x.Oct_Cnt);
            list.TotalNov_Cnt = RetList.Sum(x => x.Nov_Cnt);
            list.TotalDec_Cnt = RetList.Sum(x => x.Dec_Cnt);
            list.TotalJan_Cnt = RetList.Sum(x => x.Jan_Cnt);
            list.TotalFeb_Cnt = RetList.Sum(x => x.Feb_Cnt);
            list.TotalMar_Cnt = RetList.Sum(x => x.Mar_Cnt);
            list.TotalApr_Cnt = RetList.Sum(x => x.Apr_Cnt);
            list.TotalMay_Cnt = RetList.Sum(x => x.May_Cnt);
            list.TotalJun_Cnt = RetList.Sum(x => x.Jun_Cnt);

            //店舗全体の年度の合計
            list.TotalCount += list.TotalJul_Cnt;
            list.TotalCount += list.TotalAug_Cnt;
            list.TotalCount += list.TotalSep_Cnt;
            list.TotalCount += list.TotalOct_Cnt;
            list.TotalCount += list.TotalNov_Cnt;
            list.TotalCount += list.TotalDec_Cnt;
            list.TotalCount += list.TotalJan_Cnt;
            list.TotalCount += list.TotalFeb_Cnt;
            list.TotalCount += list.TotalMar_Cnt;
            list.TotalCount += list.TotalApr_Cnt;
            list.TotalCount += list.TotalMay_Cnt;
            list.TotalCount += list.TotalJun_Cnt;

            return RetList;
        }
        */

        /// <summary>
        /// 車両納車リスト取得
        /// </summary>
        /// <returns>期首から期末の納車リスト</returns>
        /// <history>
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// 2017/03/09 arc nakayama #3723_納車リスト　新規作成
        /// </history>
        public CarSalseList GetCarSalesListResult(string targetyear, string salestype)
        {
            var Ret = db.GetCarSalesList(targetyear, salestype);

            List<GetCarSalesList_Result> RetList = new List<GetCarSalesList_Result>();

            foreach (var Data in Ret)
            {
                GetCarSalesList_Result CarSalesData = new GetCarSalesList_Result();

                CarSalesData.DepartmentCode = Data.DepartmentCode;
                CarSalesData.DepartmentName = Data.DepartmentName;

                CarSalesData.Jul_Cnt = Data.Jul_Cnt;
                CarSalesData.Aug_Cnt = Data.Aug_Cnt;
                CarSalesData.Sep_Cnt = Data.Sep_Cnt;
                CarSalesData.Oct_Cnt = Data.Oct_Cnt;
                CarSalesData.Nov_Cnt = Data.Nov_Cnt;
                CarSalesData.Dec_Cnt = Data.Dec_Cnt;
                CarSalesData.Jan_Cnt = Data.Jan_Cnt;
                CarSalesData.Feb_Cnt = Data.Feb_Cnt;
                CarSalesData.Mar_Cnt = Data.Mar_Cnt;
                CarSalesData.Apr_Cnt = Data.Apr_Cnt;
                CarSalesData.May_Cnt = Data.May_Cnt;
                CarSalesData.Jun_Cnt = Data.Jun_Cnt;
                RetList.Add(CarSalesData);

            }


            //Add 2017/10/14 arc yano #3790
            CarSalseList list = new CarSalseList();

            list.RetLine = RetList;

            //店舗全体の月別の合計
            list.TotalJul_Cnt = RetList.Sum(x => x.Jul_Cnt);
            list.TotalAug_Cnt = RetList.Sum(x => x.Aug_Cnt);
            list.TotalSep_Cnt = RetList.Sum(x => x.Sep_Cnt);
            list.TotalOct_Cnt = RetList.Sum(x => x.Oct_Cnt);
            list.TotalNov_Cnt = RetList.Sum(x => x.Nov_Cnt);
            list.TotalDec_Cnt = RetList.Sum(x => x.Dec_Cnt);
            list.TotalJan_Cnt = RetList.Sum(x => x.Jan_Cnt);
            list.TotalFeb_Cnt = RetList.Sum(x => x.Feb_Cnt);
            list.TotalMar_Cnt = RetList.Sum(x => x.Mar_Cnt);
            list.TotalApr_Cnt = RetList.Sum(x => x.Apr_Cnt);
            list.TotalMay_Cnt = RetList.Sum(x => x.May_Cnt);
            list.TotalJun_Cnt = RetList.Sum(x => x.Jun_Cnt);

            //店舗全体の年度の合計
            list.TotalCount += list.TotalJul_Cnt;
            list.TotalCount += list.TotalAug_Cnt;
            list.TotalCount += list.TotalSep_Cnt;
            list.TotalCount += list.TotalOct_Cnt;
            list.TotalCount += list.TotalNov_Cnt;
            list.TotalCount += list.TotalDec_Cnt;
            list.TotalCount += list.TotalJan_Cnt;
            list.TotalCount += list.TotalFeb_Cnt;
            list.TotalCount += list.TotalMar_Cnt;
            list.TotalCount += list.TotalApr_Cnt;
            list.TotalCount += list.TotalMay_Cnt;
            list.TotalCount += list.TotalJun_Cnt;

            return list;
        }


        /// <summary>
        /// 車両納車リスト取得 担当者別
        /// </summary>
        /// <returns>期首から期末の納車リスト</returns>
        /// <history>2017/03/09 arc nakayama #3723_納車リスト　新規作成</history>
        public TotalCarSalesEmployeeList_Result GetCarSalesEmployeeList(string targetyear, string DepartmentCode, string salestype)
        {
            var Ret = db.GetCarSalesEmployeeList(targetyear, DepartmentCode, salestype);

            TotalCarSalesEmployeeList_Result Total = new TotalCarSalesEmployeeList_Result();

            List<GetCarSalesEmployeeList_Result> RetList = new List<GetCarSalesEmployeeList_Result>();

            //初期化
            Total.TotalJul_Cnt = 0;
            Total.TotalAug_Cnt = 0;
            Total.TotalSep_Cnt = 0;
            Total.TotalOct_Cnt = 0;
            Total.TotalNov_Cnt = 0;
            Total.TotalDec_Cnt = 0;
            Total.TotalJan_Cnt = 0;
            Total.TotalFeb_Cnt = 0;
            Total.TotalMar_Cnt = 0;
            Total.TotalApr_Cnt = 0;
            Total.TotalMay_Cnt = 0;
            Total.TotalJun_Cnt = 0;

            foreach (GetCarSalesEmployeeList_Result Data in Ret)
            {
                GetCarSalesEmployeeList_Result CarSalesData = new GetCarSalesEmployeeList_Result();

                CarSalesData.EmployeeNumber = Data.EmployeeNumber;
                CarSalesData.EmployeeCode = Data.EmployeeCode;
                CarSalesData.EmployeeName = Data.EmployeeName;
                CarSalesData.Jul_Cnt = Data.Jul_Cnt;
                CarSalesData.Aug_Cnt = Data.Aug_Cnt;
                CarSalesData.Sep_Cnt = Data.Sep_Cnt;
                CarSalesData.Oct_Cnt = Data.Oct_Cnt;
                CarSalesData.Nov_Cnt = Data.Nov_Cnt;
                CarSalesData.Dec_Cnt = Data.Dec_Cnt;
                CarSalesData.Jan_Cnt = Data.Jan_Cnt;
                CarSalesData.Feb_Cnt = Data.Feb_Cnt;
                CarSalesData.Mar_Cnt = Data.Mar_Cnt;
                CarSalesData.Apr_Cnt = Data.Apr_Cnt;
                CarSalesData.May_Cnt = Data.May_Cnt;
                CarSalesData.Jun_Cnt = Data.Jun_Cnt;


                if (Data.Jul_Cnt != null)
                {
                    Total.TotalJul_Cnt += Data.Jul_Cnt;
                }
                if(Data.Aug_Cnt != null){
                    Total.TotalAug_Cnt += Data.Aug_Cnt;
                }
                if (Data.Sep_Cnt != null)
                {
                    Total.TotalSep_Cnt += Data.Sep_Cnt;
                }
                if (Data.Oct_Cnt != null)
                {
                    Total.TotalOct_Cnt += Data.Oct_Cnt;
                }
                if (Data.Nov_Cnt != null)
                {
                    Total.TotalNov_Cnt += Data.Nov_Cnt;
                }
                if (Data.Dec_Cnt != null)
                {
                    Total.TotalDec_Cnt += Data.Dec_Cnt;
                }
                if (Data.Jan_Cnt != null)
                {
                    Total.TotalJan_Cnt += Data.Jan_Cnt;
                }
                if (Data.Feb_Cnt != null)
                {
                    Total.TotalFeb_Cnt += Data.Feb_Cnt;
                }
                if (Data.Mar_Cnt != null)
                {
                    Total.TotalMar_Cnt += Data.Mar_Cnt;
                }
                if (Data.Apr_Cnt != null)
                {
                    Total.TotalApr_Cnt += Data.Apr_Cnt;
                }
                if (Data.May_Cnt != null)
                {
                    Total.TotalMay_Cnt += Data.May_Cnt;
                }
                if (Data.Jun_Cnt != null)
                {
                    Total.TotalJun_Cnt += Data.Jun_Cnt;
                }

                RetList.Add(CarSalesData);

            }

            Total.list = new List<GetCarSalesEmployeeList_Result>();
            Total.list = RetList;

            return Total;
        }

        /// <summary>
        /// 車両納車リスト(明細)取得
        /// </summary>
        /// <returns>指定年月の納車リスト</returns>
        /// <history>
        /// 2023/09/18 yano #4181【車両伝票】オプション区分追加（サーチャージ）
        /// 2023/01/16 yano #4138【納車リスト】集計項目の追加（メンテナンスパッケージ、延長保証）
        /// 2017/10/14 arc yano #3790 納車リスト　店舗全体の表示の追加
        /// 2017/03/09 arc nakayama #3723_納車リスト　新規作成
        /// </history>
        public List<GetCarSalesHeaderListResult> GetCarSalesHeaderListByCondition(CarSalesHeaderListSearchCondition condition)
        {
            var Ret = db.GetCarSalesHeaderList(condition.SelectYear, condition.SelectMonth, condition.DepartmentCode, condition.NewUsedType, condition.AAType);

            List<GetCarSalesHeaderListResult> RetList = new List<GetCarSalesHeaderListResult>();

            foreach (var Data in Ret)
            {
                GetCarSalesHeaderListResult CarSalesData = new GetCarSalesHeaderListResult();

                CarSalesData.SalesDate = Data.SalesDate;
                CarSalesData.NewUsedTypeName = Data.NewUsedTypeName;
                CarSalesData.SlipNumber = Data.SlipNumber;
                CarSalesData.SalesCarNumber = Data.SalesCarNumber;
                CarSalesData.Vin = Data.Vin;
                CarSalesData.CustomerName = Data.CustomerName;
                CarSalesData.DepartmentCode = Data.DepartmentCode;
                CarSalesData.DepartmentName = Data.DepartmentName;
                CarSalesData.Employeename = Data.Employeename;
                CarSalesData.SalesPrice = Data.SalesPrice;
                CarSalesData.ShopOptionAmountWithTax = Data.ShopOptionAmountWithTax;
                CarSalesData.MaintenancePackageAmount = Data.MaintenancePackageAmount;    //Add 2023/01/16 yano #4138
                CarSalesData.ExtendedWarrantyAmount = Data.ExtendedWarrantyAmount;        //Add 2023/01/16 yano #4138
                CarSalesData.SurchargeAmount = Data.SurchargeAmount;                      //Add 2023/09/18 yano #4181
                CarSalesData.MakerOptionAmount = Data.MakerOptionAmount;
                CarSalesData.AAAmount = Data.AAAmount;                                    //Add 2017/10/14 arc yano #3790
                CarSalesData.SalesCostTotalAmount = Data.SalesCostTotalAmount;
                CarSalesData.DiscountAmount = Data.DiscountAmount;
                CarSalesData.OtherCostTotalAmount = Data.OtherCostTotalAmount;
                CarSalesData.TaxFreeTotalAmount = Data.TaxFreeTotalAmount;
                CarSalesData.CarLiabilityInsurance = Data.CarLiabilityInsurance;
                CarSalesData.RecycleDeposit = Data.RecycleDeposit;
                CarSalesData.GrandTotalAmount = Data.GrandTotalAmount;
                CarSalesData.TradeInTotalAmountNotTax = Data.TradeInTotalAmountNotTax;
                CarSalesData.TradeInVin1 = Data.TradeInVin1;                        //Add 2017/10/14 arc yano #3790
                CarSalesData.TradeInVin2 = Data.TradeInVin2;                        //Add 2017/10/14 arc yano #3790
                CarSalesData.TradeInVin3 = Data.TradeInVin3;                        //Add 2017/10/14 arc yano #3790
                CarSalesData.TradeInUnexpiredCarTaxTotalAmount = Data.TradeInUnexpiredCarTaxTotalAmount;
                CarSalesData.TradeInRemainDebtTotalAmount = Data.TradeInRemainDebtTotalAmount;
                CarSalesData.TradeInAppropriationTotalAmount = Data.TradeInAppropriationTotalAmount;
                CarSalesData.CarName = Data.CarName;
                CarSalesData.CarBrandName = Data.CarBrandName;

                

                RetList.Add(CarSalesData);
            }

            return RetList;
        }

        /// <summary>
        /// 車両伝票修正の進行中のリスト表示
        /// </summary>
        /// <param name="ChangeStatus">リストの種類</param>
        /// <param name="Authority">納車済伝票ステータス戻し権限</param>
        /// <param name="EmployeeCode">ユーザ</param>
        /// <returns>進行中のリスト</returns>
        /// <history>
        /// 2018/08/07 yano #3911 登録済車両の車両伝票ステータス修正について
        /// 2018/06/18 arc yano #3897 車両伝票ステータス修正　権限による機能の制限
        /// 2017/05/11 arc nakayama #3761_サブシステムの伝票戻しの移行　新規作成
        /// </history>
        public List<Get_CarSlipStatusChangeResult> Get_CarSlipStatusChange(string ChangeStatus, bool Authority, string EmployeeCode)
        {
            //納車済伝票修正権限がある場合は最終更新者による絞込みは行わず、ない場合は絞込みを行う
            var RetData = Authority ? db.Get_CarSlipStatusChange(ChangeStatus) : db.Get_CarSlipStatusChange(ChangeStatus).Where(x => x.LastUpdateEmployeeCode.Equals(EmployeeCode));

            List<c_SalesOrderStatus> slist = new CodeDao(db).GetSalesOrderStatusAll(false);

            List<Get_CarSlipStatusChangeResult> ChangeStatusList = new List<Get_CarSlipStatusChangeResult>();

            foreach (var Ret in RetData)
            {
                Get_CarSlipStatusChangeResult ChangeData = new Get_CarSlipStatusChangeResult();
                ChangeData.SlipNumber = Ret.SlipNumber;
                ChangeData.SalesOrderStatus = Ret.SalesOrderStatus;
                ChangeData.RequestUserName = Ret.RequestUserName;
                ChangeData.CreateEmployeeCode = Ret.CreateEmployeeCode;
                ChangeData.CreateDate = Ret.CreateDate;
                ChangeData.LastUpdateEmployeeCode = Ret.LastUpdateEmployeeCode;
                ChangeData.LastUpdateDate = Ret.LastUpdateDate;
                ChangeData.ChangeStatus = Ret.ChangeStatus;
                ChangeData.StatusChangeCode = Ret.StatusChangeCode;
                ChangeData.EmployeeName = Ret.EmployeeName;
                ChangeData.SalesOrderStatusName = slist.Where(x => x.Code.Equals(Ret.SalesOrderStatus)).FirstOrDefault().Name;  //Add 2018/08/07 yano #3911
                ChangeStatusList.Add(ChangeData);
            }

            return ChangeStatusList;
        }

        /// <summary>
        /// 修正中の伝票情報取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>複数件の伝票</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public List<ModificationControl> GetModificationStatusAll(string slipNumber)
        {
            var query =
                from a in db.ModificationControl
                where (string.IsNullOrEmpty(slipNumber) || slipNumber.Equals(a.SlipNumber))
                select a;

            return query.ToList<ModificationControl>();
        }

        /// <summary>
        /// 修正履歴１件を取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>修正履歴</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public ModifiedReason GetLatestModifiedReason(string slipNumber)
        {
            var query =
                (from a in db.ModifiedReason
                 where slipNumber.Equals(a.SlipNumber)
                 select a).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 修正履歴を取得する（該当伝票の全履歴）
        /// </summary>
        /// <param name="code">伝票番号</param>
        /// <returns>修正履歴</returns>
        /// <history>Add 2017/05/24 arc nakayama #3761_サブシステムの伝票戻しの移行</history>
        public List<ModifiedReason> GetModifiedReason(string slipNumber)
        {
            var query =
                from a in db.ModifiedReason
                where slipNumber.Equals(a.SlipNumber)
                orderby a.CreateDate descending
                select a;

            return query.ToList<ModifiedReason>();
        }

        /// <summary>
        /// 修正中の伝票情報取得
        /// </summary>
        /// <param name="slipNumber">伝票番号</param>
        /// <returns>１件の伝票</returns>
        public ModificationControl GetModificationStatus(string slipNumber, int RevisionNumber)
        {
            var query =
                (from a in db.ModificationControl
                 where slipNumber.Equals(a.SlipNumber)
                 && RevisionNumber.Equals(a.RevisionNumber)
                 select a).FirstOrDefault();

            return query;
        }
    }
}
