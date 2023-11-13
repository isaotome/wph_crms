using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Data.Linq;
using System.Reflection;

namespace CrmsDao {
    public class CarPurchaseOrderDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public CarPurchaseOrderDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 発注依頼データを取得する（PK指定）
        /// </summary>
        /// <param name="carPurchaseOrderId">車両発注依頼ID</param>
        /// <returns></returns>
        public CarPurchaseOrder GetByKey(string carPurchaseOrderNumber) {
            var query =
                (from a in db.CarPurchaseOrder
                where a.CarPurchaseOrderNumber.Equals(carPurchaseOrderNumber)
                select a).FirstOrDefault();
            return query;
        }
        
        /// <summary>
        /// 発注依頼番号リストに該当する車両発注依頼データリストを取得する
        /// </summary>
        /// <param name="keyList">発注依頼番号リスト</param>
        /// <returns>発注依頼データリスト</returns>
        public List<CarPurchaseOrder> GetListByKeyList(string[] keyList) {
            var query =
                from a in db.CarPurchaseOrder
                select a;

            ParameterExpression param = Expression.Parameter(typeof(CarPurchaseOrder), "x");
            MemberExpression left = Expression.Property(param, "CarPurchaseOrderNumber");
            BinaryExpression body = null;
            foreach (var b in keyList) {
                ConstantExpression right = Expression.Constant(b, typeof(string));
                if (body == null) {
                    body = Expression.Equal(left, right);
                } else {
                    body = Expression.OrElse(body,Expression.Equal(left, right));
                }
            }
            query = query.Where(Expression.Lambda<Func<CarPurchaseOrder, bool>>(body, param));
            query = query.OrderByDescending(x => x.CarPurchaseOrderNumber);
            return query.ToList<CarPurchaseOrder>();
        }

        /// <summary>
        /// 車両発注依頼検索
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<CarPurchaseOrder> GetListByCondition(CarPurchaseOrder condition, int pageIndex, int pageSize) {
            return new PaginatedList<CarPurchaseOrder>(GetQueryable(condition), pageIndex, pageSize);

        }

        /// <summary>
        /// 車両発注依頼検索
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<CarPurchaseOrder> GetListByCondition(DocumentExportCondition condition){
            CarPurchaseOrder orderCondition = new CarPurchaseOrder();
            orderCondition.StopFlag = condition.StopFlag;
            orderCondition.SetAuthCondition(condition.AuthEmployee);
            return GetQueryable(orderCondition).ToList();
        }

        /// <summary>
        /// 車両発注依頼検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        /// <history>
        /// 2023/01/19 yano #4161【車両発注依頼引当】検索実行時の条件設定の誤り対応
        /// </history>
        private IQueryable<CarPurchaseOrder> GetQueryable(CarPurchaseOrder condition){
            //承認
            bool approvalFlag = false;
            try { approvalFlag = string.IsNullOrEmpty(condition.CarSalesHeader.ApprovalFlag); } catch (NullReferenceException) { }

            //発注
            bool purchaseOrderStatus = string.IsNullOrEmpty(condition.PurchaseOrderStatus);

            //引当
            bool reservationStatus = string.IsNullOrEmpty(condition.ReservationStatus);

            //登録
            bool registrationStatus = string.IsNullOrEmpty(condition.RegistrationStatus);

            //預り
            bool stopFlag = string.IsNullOrEmpty(condition.StopFlag);

            //未引当
            bool noReservation = string.IsNullOrEmpty(condition.NoReservation);

            //未登録
            bool noRegistration = string.IsNullOrEmpty(condition.NoRegistration);

            //キャンセルを表示
            bool cancelFlag = string.IsNullOrEmpty(condition.CancelFlag);

            //部門
            string departmentCode = condition.DepartmentCode;

            //営業担当者
            string employeeCode = condition.EmployeeCode;

            // Mod 2014/08/18 arc amii 検索仕様変更対応 #3074 Fromのみ入力された場合、Fromで指定した管理番号のみ検索できるよう修正
            //管理番号
            string salesCarNumberFrom = "";
            string salesCarNumberTo = "";
            if (string.IsNullOrWhiteSpace(condition.SalesCarNumberFrom) == false && string.IsNullOrWhiteSpace(condition.SalesCarNumberTo) == true)
            {
                // Fromのみ入力されていた場合、Fromの値のデータのみ検索できること
                salesCarNumberFrom = condition.SalesCarNumberFrom;
                salesCarNumberTo = condition.SalesCarNumberFrom;
            } else {
                //上記以外の場合、通常の検索が行われること
                salesCarNumberFrom = condition.SalesCarNumberFrom;
                salesCarNumberTo = condition.SalesCarNumberTo;
            }

            // Mod 2014/08/18 arc amii 検索仕様変更対応 #3074 Fromのみ入力された場合、Fromで指定した伝票番号のみ検索できるよう修正
            //伝票番号
            string slipNumberFrom = "";
            string slipNumberTo = "";
            if (string.IsNullOrWhiteSpace(condition.SlipNumberFrom) == false && string.IsNullOrWhiteSpace(condition.SlipNumberTo) == true)
            {
                // Fromのみ入力されていた場合、ToにもFromと同じ値を設定する
                slipNumberFrom = condition.SlipNumberFrom;
                slipNumberTo = condition.SlipNumberFrom;
            }
            else
            {
                //上記以外の場合、通常の検索条件を設定する
                slipNumberFrom = condition.SlipNumberFrom;
                slipNumberTo = condition.SlipNumberTo;
            }

            //受注日
            DateTime? salesOrderDateFrom = condition.SalesOrderDateFrom;
            DateTime? salesOrderDateTo = condition.SalesOrderDateTo;

            // Mod 2014/08/18 arc amii 検索仕様変更対応 #3074 Fromのみ入力された場合、Fromで指定した伝票番号のみ検索できるよう修正
            //オーダー番号
            string makerOrderNumberFrom = "";
            string makerOrderNumberTo = "";
            if (string.IsNullOrWhiteSpace(condition.MakerOrderNumberFrom) == false && string.IsNullOrWhiteSpace(condition.MakerOrderNumberTo) == true)
            {
                // Fromのみ入力されていた場合、ToにもFromと同じ値を設定する
                makerOrderNumberFrom = condition.MakerOrderNumberFrom;
                makerOrderNumberTo = condition.MakerOrderNumberFrom;
            }
            else
            {
                //上記以外の場合、通常の検索条件を設定する
                makerOrderNumberFrom = condition.MakerOrderNumberFrom;
                makerOrderNumberTo = condition.MakerOrderNumberTo;
            }

            //登録予定日
            DateTime? registPlanDateFrom = condition.RegistrationPlanDateFrom;
            DateTime? registPlanDateTo = condition.RegistrationPlanDateTo;

            //メーカー名
            string makerName = condition.MakerName;

            //車種名
            string carName = condition.CarName;

            //モデルコード
            string modelCode = condition.ModelCode;

            //型式
            string modelName = condition.ModelName;

            //書類購入希望日
            DateTime? documentPurchaseRequestDateFrom = condition.DocumentPurchaseRequestDateFrom;
            DateTime? documentPurchaseRequestDateTo = condition.DocumentPurchaseRequestDateTo;

            //書類購入日
            DateTime? documentPurchaseDateFrom = condition.DocumentPurchaseDateFrom;
            DateTime? documentPurchaseDateTo = condition.DocumentPurchaseDateTo;

            //書類到着日
            DateTime? documentReceiptDateFrom = condition.DocumentReceiptDateFrom;
            DateTime? documentReceiptDateTo = condition.DocumentReceiptDateTo;

            //所属会社による閲覧権限
            string companyCode = condition.AuthCompanyCode;

            //車台番号
            string vin = condition.Vin;

            var query =
                from a in db.CarPurchaseOrder
                join b in db.CarSalesHeader on new { a.SlipNumber, DelFlag = "0" } equals new { b.SlipNumber, b.DelFlag } into sales
                from b in sales.DefaultIfEmpty()
                orderby a.CarPurchaseOrderNumber descending
                where (string.IsNullOrEmpty(departmentCode) || b.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(employeeCode) || b.EmployeeCode.Equals(employeeCode))
                && (string.IsNullOrEmpty(salesCarNumberFrom) || string.Compare(a.SalesCar.SalesCarNumber, salesCarNumberFrom) >= 0)
                && (string.IsNullOrEmpty(salesCarNumberTo) || string.Compare(a.SalesCar.SalesCarNumber, salesCarNumberTo) <= 0)
                && (string.IsNullOrEmpty(slipNumberFrom) || string.Compare(a.SlipNumber, slipNumberFrom) >= 0)
                && (string.IsNullOrEmpty(slipNumberTo) || string.Compare(a.SlipNumber, slipNumberTo) <= 0)
                && (salesOrderDateFrom == null || DateTime.Compare(salesOrderDateFrom ?? DaoConst.SQL_DATETIME_MIN, b.SalesOrderDate ?? DaoConst.SQL_DATETIME_MAX) <= 0)
                && (salesOrderDateTo == null || DateTime.Compare(salesOrderDateTo ?? DaoConst.SQL_DATETIME_MAX, b.SalesOrderDate ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                && (string.IsNullOrEmpty(makerOrderNumberFrom) || string.Compare(a.MakerOrderNumber, makerOrderNumberFrom) >= 0)
                    // Mod 2014/08/18 arc amii 検索仕様変更対応 #3074 Toの比較演算子が間違っているのを修正
                && (string.IsNullOrEmpty(makerOrderNumberTo) || string.Compare(a.MakerOrderNumber, makerOrderNumberTo) <= 0)
                && (registPlanDateFrom == null || DateTime.Compare(a.RegistrationPlanDate ?? DaoConst.SQL_DATETIME_MIN, registPlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (registPlanDateTo == null || DateTime.Compare(a.RegistrationPlanDate ?? DaoConst.SQL_DATETIME_MAX, registPlanDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(makerName) || b.MakerName.Contains(makerName))
                && (string.IsNullOrEmpty(carName) || b.CarName.Contains(carName))
                && (string.IsNullOrEmpty(modelCode) || b.CarGrade.ModelCode.Contains(modelCode))
                && (string.IsNullOrEmpty(modelName) || b.ModelName.Contains(modelName))
                && (string.IsNullOrEmpty(companyCode) || b.CarGrade.Car.Brand.CompanyCode.Equals(companyCode))
                && (approvalFlag || b.ApprovalFlag.Equals("1"))
                && (purchaseOrderStatus || a.PurchaseOrderStatus.Equals("1"))
                && (reservationStatus || a.ReservationStatus.Equals("1"))
                && (registrationStatus || a.RegistrationStatus.Equals("1"))
                && (stopFlag || a.StopFlag.Equals("1"))
                && (noRegistration || a.RegistrationStatus == null)
                && (cancelFlag || (!b.SalesOrderStatus.Equals("006") && !b.SalesOrderStatus.Equals("007")) || a.SlipNumber == null)  //Mod 2023/01/19 yano #4161
                //&& (cancelFlag || !b.SalesOrderStatus.Equals("006") || a.SlipNumber == null)
                && (noReservation || !a.ReservationStatus.Equals("1") || a.ReservationStatus == null)
                && (documentPurchaseRequestDateFrom == null || DateTime.Compare(documentPurchaseRequestDateFrom ?? DaoConst.SQL_DATETIME_MIN, a.DocumentPurchaseRequestDate ?? DaoConst.SQL_DATETIME_MAX) <= 0)
                && (documentPurchaseRequestDateTo == null || DateTime.Compare(documentPurchaseRequestDateTo ?? DaoConst.SQL_DATETIME_MAX, a.DocumentPurchaseRequestDate ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                    // Mod 2014/07/22 arc amii 既存バグ対応 書類購入日Fromの条件が含まれていないのを修正
                    //&& (documentPurchaseDateTo == null || DateTime.Compare(documentPurchaseDateTo ?? DaoConst.SQL_DATETIME_MAX, a.DocumentPurchaseDate ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                && (documentPurchaseDateFrom == null || DateTime.Compare(documentPurchaseDateFrom ?? DaoConst.SQL_DATETIME_MIN, a.DocumentPurchaseDate ?? DaoConst.SQL_DATETIME_MAX) <= 0)
                && (documentPurchaseDateTo == null || DateTime.Compare(documentPurchaseDateTo ?? DaoConst.SQL_DATETIME_MAX, a.DocumentPurchaseDate ?? DaoConst.SQL_DATETIME_MIN) >= 0)

                // Mod 2014/07/22 arc amii 既存バグ対応 書類到着日を検索条件に含めるとシステムエラーが発生するのを修正
                    //&& (documentReceiptDateFrom == null || DateTime.Compare(documentReceiptDateFrom ?? DaoConst.SQL_DATETIME_MAX, a.DocumentReceiptDateFrom ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                    //&& (documentReceiptDateTo == null || DateTime.Compare(documentReceiptDateTo ?? DaoConst.SQL_DATETIME_MAX, a.DocumentReceiptDateTo ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                && (documentReceiptDateFrom == null || DateTime.Compare(documentReceiptDateFrom ?? DaoConst.SQL_DATETIME_MIN, a.DocumentReceiptDate ?? DaoConst.SQL_DATETIME_MAX) <= 0)
                && (documentReceiptDateTo == null || DateTime.Compare(documentReceiptDateTo ?? DaoConst.SQL_DATETIME_MAX, a.DocumentReceiptDate ?? DaoConst.SQL_DATETIME_MIN) >= 0)
                && a.DelFlag.Equals("0")
                && (string.IsNullOrEmpty(vin) || a.Vin.Contains(vin) || b.Vin.Contains(vin))
                select a;

            return query;
            
        }

        /// <summary>
        /// 車両伝票番号に該当する車両発注依頼データを取得する
        /// </summary>
        /// <param name="slipNumber">車両伝票番号</param>
        /// <returns>車両発注依頼データ</returns>
        public CarPurchaseOrder GetBySlipNumber(string slipNumber) {
            var query =
                (from a in db.CarPurchaseOrder
                where a.SlipNumber.Equals(slipNumber)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();
            return query;
        }


        /// <summary>
        /// 車両発注依頼に関連付けされたオブジェクトに値を代入
        /// </summary>
        /// <param name="list">車両発注依頼リスト</param>
        /// <returns>車両発注依頼リスト更新版</returns>
        public List<CarPurchaseOrder> SetCarPurchaseOrderList(List<CarPurchaseOrder> orderList) {
            CarSalesOrderDao orderDao = new CarSalesOrderDao(db);
            SalesCarDao salesCarDao = new SalesCarDao(db);
            JournalDao journalDao = new JournalDao(db);
            EmployeeDao employeeDao = new EmployeeDao(db);
            SupplierDao supplierDao = new SupplierDao(db);
            CarPurchaseDao purchaseDao = new CarPurchaseDao(db);
            CarAppraisalDao appraisalDao = new CarAppraisalDao(db);
            SupplierPaymentDao supplierPaymentDao = new SupplierPaymentDao(db);
            List<IEnumerable<SelectListItem>> registMonthList = new List<IEnumerable<SelectListItem>>();
            List<IEnumerable<SelectListItem>> firmList = new List<IEnumerable<SelectListItem>>();
            CodeDao dao = new CodeDao(db);

            foreach (var a in orderList) {
                if (!string.IsNullOrEmpty(a.SlipNumber)) {
                    a.CarSalesHeader = orderDao.GetBySlipNumber(a.SlipNumber);
                    a.ReceiptAmount = journalDao.GetTotalBySlipNumber(a.SlipNumber);
                }
                a.Employee = employeeDao.GetByKey(a.EmployeeCode);
                a.Supplier = supplierDao.GetByKey(a.SupplierCode);
                a.SupplierPayment = supplierPaymentDao.GetByKey(a.SupplierPaymentCode);
                CarPurchase purchase = purchaseDao.GetBySalesCarNumber(a.SalesCarNumber);
                if (purchase != null) {
                //    a.FirmMargin = purchase.FirmPrice;
                //    a.DiscountAmount = purchase.DiscountPrice;
                //    a.VehiclePrice = purchase.VehiclePrice;
                //    a.MetallicPrice = purchase.MetallicPrice;
                //    a.OptionPrice = purchase.OptionPrice;
                //    a.Amount = purchase.Amount;
                    a.PurchaseMemo = purchase.Memo;
                }
                
                if (a.CarSalesHeader == null) a.CarSalesHeader = new CarSalesHeader();
                if (a.Employee == null) a.Employee = new Employee();
                if (a.Supplier == null) a.Supplier = new Supplier();
                if (a.SupplierPayment == null) {
                    a.SupplierPayment = new SupplierPayment();
                } else {

                    //**メーカー出荷予定日、書類購入日が入っていたら金利計算
                    //経過日数
                    int period = a.MakerShipmentPlanDate == null ? 0 : (a.DocumentPurchaseDate ?? DateTime.Today).Subtract(a.MakerShipmentPlanDate ?? DateTime.Today).Days;


                    //金利発生
                    if (a.MakerShipmentPlanDate != null) {
                        a.PaymentExpiredate = a.SupplierPayment.PaymentPeriod1 - period;
                    }

                    if (period < a.SupplierPayment.PaymentPeriod1) {
                        a.PaymentPeriod1 = 0;
                        a.PaymentPeriod2 = 0;
                        a.PaymentPeriod3 = 0;
                        a.PaymentPeriod4 = 0;
                        a.PaymentPeriod5 = 0;
                        a.PaymentPeriod6 = 0;
                    } else if (period >= a.SupplierPayment.PaymentPeriod6) {
                        a.PaymentPeriod1 = a.SupplierPayment.PaymentPeriod1 ?? 0;
                        a.PaymentPeriod2 = a.SupplierPayment.PaymentPeriod2 ?? 0;
                        a.PaymentPeriod3 = a.SupplierPayment.PaymentPeriod3 ?? 0;
                        a.PaymentPeriod4 = a.SupplierPayment.PaymentPeriod4 ?? 0;
                        a.PaymentPeriod5 = a.SupplierPayment.PaymentPeriod5 ?? 0;
                        a.PaymentPeriod6 = period - a.PaymentPeriod1 - a.PaymentPeriod2 - a.PaymentPeriod3 - a.PaymentPeriod4 - a.PaymentPeriod5;
                    } else if (period >= a.SupplierPayment.PaymentPeriod5) {
                        a.PaymentPeriod1 = a.SupplierPayment.PaymentPeriod1 ?? 0;
                        a.PaymentPeriod2 = a.SupplierPayment.PaymentPeriod2 ?? 0;
                        a.PaymentPeriod3 = a.SupplierPayment.PaymentPeriod3 ?? 0;
                        a.PaymentPeriod4 = a.SupplierPayment.PaymentPeriod4 ?? 0;
                        a.PaymentPeriod5 = period < a.SupplierPayment.PaymentPeriod6 ? period - a.PaymentPeriod1 - a.PaymentPeriod2 - a.PaymentPeriod3 - a.PaymentPeriod4 : a.SupplierPayment.PaymentPeriod5 ?? 0;
                    } else if (period >= a.SupplierPayment.PaymentPeriod4) {
                        a.PaymentPeriod1 = a.SupplierPayment.PaymentPeriod1 ?? 0;
                        a.PaymentPeriod2 = a.SupplierPayment.PaymentPeriod2 ?? 0;
                        a.PaymentPeriod3 = a.SupplierPayment.PaymentPeriod3 ?? 0;
                        a.PaymentPeriod4 = period < a.SupplierPayment.PaymentPeriod5 ? period - a.PaymentPeriod1 - a.PaymentPeriod2 - a.PaymentPeriod3 : a.SupplierPayment.PaymentPeriod4 ?? 0;
                    } else if (period >= a.SupplierPayment.PaymentPeriod3) {
                        a.PaymentPeriod1 = a.SupplierPayment.PaymentPeriod1 ?? 0;
                        a.PaymentPeriod2 = a.SupplierPayment.PaymentPeriod2 ?? 0;
                        a.PaymentPeriod3 = period < a.SupplierPayment.PaymentPeriod4 ? period - a.PaymentPeriod1 - a.PaymentPeriod2 : a.SupplierPayment.PaymentPeriod3 ?? 0;
                    } else if (period >= a.SupplierPayment.PaymentPeriod2) {
                        a.PaymentPeriod1 = a.SupplierPayment.PaymentPeriod1 ?? 0;
                        a.PaymentPeriod2 = period < a.SupplierPayment.PaymentPeriod3 ? period - a.PaymentPeriod1 : a.SupplierPayment.PaymentPeriod2 ?? 0;
                    } else if (period >= a.SupplierPayment.PaymentPeriod1) {
                        a.PaymentPeriod1 = period < a.SupplierPayment.PaymentPeriod2 ? period : a.SupplierPayment.PaymentPeriod1 ?? 0;
                    }

                    //金額の計算
                    a.PaymentAmount1 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod1 / 365 * a.SupplierPayment.PaymentRate1 / 100) ?? 0, 1);
                    a.PaymentAmount2 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod2 / 365 * a.SupplierPayment.PaymentRate2 / 100) ?? 0, 1);
                    a.PaymentAmount3 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod3 / 365 * a.SupplierPayment.PaymentRate3 / 100) ?? 0, 1);
                    a.PaymentAmount4 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod4 / 365 * a.SupplierPayment.PaymentRate4 / 100) ?? 0, 1);
                    a.PaymentAmount5 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod5 / 365 * a.SupplierPayment.PaymentRate5 / 100) ?? 0, 1);
                    a.PaymentAmount6 = CommonUtils.RoundDown((a.Amount * a.PaymentPeriod6 / 365 * a.SupplierPayment.PaymentRate6 / 100) ?? 0, 1);

                }

                a.FirmList = CodeUtils.GetSelectListByModel<c_Firm>(dao.GetFirmAll(false), a.Firm, true);
                a.RegistMonthList = CodeUtils.GetSelectListByModel<c_RegistMonth>(dao.GetRegistMonthAll(false), a.RegistMonth, true);

                if (a.CarSalesHeader!=null) {
                    CarSalesHeader header = a.CarSalesHeader;

                    if (!string.IsNullOrEmpty(header.TradeInVin1)) {
                        //下車情報
                        CarAppraisal appraisal = appraisalDao.GetBySlipNumberVin(a.SlipNumber, header.TradeInVin1);
                        if (appraisal != null) {
                            a.TradeInBrandName = appraisal.MakerName;
                            a.TradeInCarName = appraisal.CarName;
                            a.TradeInModelYear = appraisal.ModelYear;

                            CarPurchase tradeInPurchase = purchaseDao.GetByCarAppraisalId(appraisal.CarAppraisalId);
                            if (tradeInPurchase != null) {
                                a.PurchaseDate = tradeInPurchase.PurchaseDate;
                            }
                        }
                    }

                    //下取車が仕入完了しているか
                    a.TradeInPurchaseFlag = true;
                    for (int i = 1; i <= 3; i++) {
                        PropertyInfo prop = header.GetType().GetProperty("TradeInVin" + i);
                        if (prop != null && prop.GetValue(header, null) != null) {
                            List<CarPurchase> tradeInList = purchaseDao.GetBySlipNumber(a.SlipNumber, prop.GetValue(header, null).ToString());
                            if (tradeInList == null || tradeInList.Count == 0) {
                                a.TradeInPurchaseFlag = false;
                                continue;
                            }
                        }
                    }

                    //登録予定日(NULLの場合車両伝票の登録希望日をセット）
                    if (a.RegistrationPlanDate == null) {
                        a.RegistrationPlanDate = header.RequestRegistDate;
                    }
                    //月内(NULLの場合車両伝票の登録希望日の月をセット）
                    if (string.IsNullOrEmpty(a.RegistMonth) && header.RequestRegistDate != null) {
                        a.RegistMonth = header.RequestRegistDate.Value.Month.ToString();
                    }

                    if (header.CarSalesLine != null && header.CarSalesLine.Count>0) {
                        //オプション情報を上位5件まで
                        for (int i = 0; (i < header.CarSalesLine.Count && i < 5); i++) {
                            a.GetType().GetProperty("Parts" + (i + 1)).SetValue(a, header.CarSalesLine[i].CarOptionName, null);
                        }
                    }

                    if (header.CarSalesReport != null && header.CarSalesReport.Count>0) {
                        //販売報告書データを上位10個までセット
                        for (int i = 0; (i < header.CarSalesReport.Count && i < 10); i++) {
                            a.GetType().GetProperty("EventCode" + (i + 1)).SetValue(a, header.CarSalesReport[i].CampaignCode, null);
                            a.GetType().GetProperty("ReportName" + (i + 1)).SetValue(a, header.CarSalesReport[i].ReportName, null);
                            a.GetType().GetProperty("ReportAmount" + (i + 1)).SetValue(a, header.CarSalesReport[i].Amount, null);
                        }
                        //ローンキャンペーン合計金額
                        a.LoanCampaign = a.CarSalesHeader.CarSalesReport.Sum(x => x.Amount);

                    }

                    //ローン情報
                    switch (header.PaymentPlanType) {
                        case "A":
                            a.LoanCompanyName = header.LoanA.CustomerClaim.CustomerClaimName;
                            a.LoanFrequency = header.PaymentFrequencyA;
                            a.LoanRate = header.LoanRateA;
                            a.RemainAmount = header.RemainAmountA;
                            a.RemainFinalMonth = header.RemainFinalMonthA;
                            a.AuthenticationNumber = header.AuthorizationNumberA;
                            break;
                        case "B":
                            a.LoanCompanyName = header.LoanB.CustomerClaim.CustomerClaimName;
                            a.LoanFrequency = header.PaymentFrequencyB;
                            a.LoanRate = header.LoanRateB;
                            a.RemainAmount = header.RemainAmountB;
                            a.RemainFinalMonth = header.RemainFinalMonthB;
                            a.AuthenticationNumber = header.AuthorizationNumberB;
                            break;
                        case "C":
                            a.LoanCompanyName = header.LoanC.CustomerClaim.CustomerClaimName;
                            a.LoanFrequency = header.PaymentFrequencyC;
                            a.RemainAmount = header.RemainAmountC;
                            a.RemainFinalMonth = header.RemainFinalMonthC;
                            a.AuthenticationNumber = header.AuthorizationNumberC;
                            break;
                        default:
                            break;
                    }


                }
            }
            return orderList;
        }

        //Add 2014/10/23 arc yano 車両ステータス追加対応
        /// <summary>
        /// 最新の車両発注依頼番号(左１桁が'2'のもの)を取得する。
        /// </summary>
        /// <returns>車両発注依頼データ</returns>
        public String GetLatestPurchaseOrderNumber()
        {
            var query =
                (from a in db.CarPurchaseOrder
                 where a.CarPurchaseOrderNumber.Substring(0,1).Equals("2")
                 //&& a.DelFlag.Equals("0")
                 select a).Max(a => a.CarPurchaseOrderNumber);
            return query;
        }

        //Add 2014/10/23 arc amii 車両ステータス追加対応
        /// <summary>
        /// 指定した管理番号の車両発注依頼を取得する
        /// </summary>
        /// <param name="salesCarNumber">管理番号</param>
        /// <returns>車両発注依頼</returns>
        public CarPurchaseOrder GetBySalesCarNumber(string salesCarNumber)
        {
            var query =
                (from a in db.CarPurchaseOrder
                where a.SalesCarNumber.Equals(salesCarNumber)
                && a.DelFlag.Equals("0")
                select a).FirstOrDefault();
            return query;
        }
    }
}
