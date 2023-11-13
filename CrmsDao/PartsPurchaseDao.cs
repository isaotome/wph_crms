using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CrmsDao {
    public class PartsPurchaseDao {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context">データコンテキスト</param>
        public PartsPurchaseDao(CrmsLinqDataContext context) {
            db = context;
        }

        /// <summary>
        /// 部品仕入データを取得する（PK指定）
        /// </summary>
        /// <param name="purchaseNumber">部品仕入番号</param>
        /// <returns></returns>
        public PartsPurchase GetByKey(string purchaseNumber) {
            var query =
                (from a in db.PartsPurchase
                 where a.PurchaseNumber.Equals(purchaseNumber)
                 select a).FirstOrDefault();
            return query;
        }
        /// <summary>
        /// 部品仕入データを検索する
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        public PaginatedList<PartsPurchase> GetListByCondition(PartsPurchase condition, int pageIndex, int pageSize) {
            return new PaginatedList<PartsPurchase>(GetQueryable(condition), pageIndex, pageSize);
        }

        /// <summary>
        /// 部品仕入データを検索する
        /// （ページング非対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public List<PartsPurchase> GetListByCondition(DocumentExportCondition condition){
            PartsPurchase purchaseCondition = new PartsPurchase();
            purchaseCondition.SetAuthCondition(condition.AuthEmployee);
            
            purchaseCondition.PurchaseDateFrom = condition.TermFrom;
            purchaseCondition.PurchaseDateTo = condition.TermTo;
            
            purchaseCondition.DepartmentCode = condition.DepartmentCode;
            purchaseCondition.OfficeCode = condition.OfficeCode;
            purchaseCondition.SupplierCode = condition.SupplierCode;
            purchaseCondition.PartsPurchaseOrder = new PartsPurchaseOrder();
            purchaseCondition.PartsPurchaseOrder.ServiceSlipNumber = condition.SlipNumber;
            purchaseCondition.CustomerName = condition.CustomerName;
            return GetQueryable(purchaseCondition).ToList();
        }

        /// <summary>
        /// 部品仕入検索条件式を取得する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        private IQueryable<PartsPurchase> GetQueryable(PartsPurchase condition){
            string purchaseOrderNumber = condition.PartsPurchaseOrder!=null ? condition.PartsPurchaseOrder.PurchaseOrderNumber : "";
            string locationCode = condition.LocationCode;
            string employeeCode = condition.EmployeeCode;
            string webOrderNumber = condition.PartsPurchaseOrder != null ? condition.PartsPurchaseOrder.WebOrderNumber : "";
            //string makerOrderNumber = condition.PartsPurchaseOrder != null ? condition.PartsPurchaseOrder.MakerOrderNumber : "";
            //string invoiceNo = condition.PartsPurchaseOrder != null ? condition.PartsPurchaseOrder.InvoiceNo : "";
            DateTime? purchasePlanDateFrom = condition.PurchasePlanDateFrom;
            DateTime? purchasePlanDateTo = condition.PurchasePlanDateTo;
            DateTime? purchaseDateFrom = condition.PurchaseDateFrom;
            DateTime? purchaseDateTo = condition.PurchaseDateTo;
            string departmentCode = condition.DepartmentCode;
            string officeCode = condition.OfficeCode;
            string slipnumber = condition.PartsPurchaseOrder != null ? condition.PartsPurchaseOrder.ServiceSlipNumber : "";
            string supplierCode = condition.SupplierCode;
            string customerName = condition.CustomerName;
            string purchaseStatus = condition.PurchaseStatus;
            string partsNumber = condition.PartsNumber;
            // Add 2014/09/04 arc amii 検索条件対応 #3041 検索条件に入荷伝票番号を追加
            string purchaseNumber = condition.PurchaseNumber;

            var query =
                from a in db.PartsPurchase
                orderby a.PurchaseNumber descending
                where (string.IsNullOrEmpty(purchaseNumber) || a.PurchaseNumber.Equals(purchaseNumber))            // Add 2014/09/04 arc amii 検索条件対応 #3041 検索条件に入荷伝票番号を追加
                && (string.IsNullOrEmpty(purchaseOrderNumber) || a.PurchaseOrderNumber.Equals(purchaseOrderNumber))
                && (string.IsNullOrEmpty(partsNumber) || a.PartsNumber.Contains(partsNumber))
                && (string.IsNullOrEmpty(locationCode) || a.Location.Equals(locationCode))
                && (string.IsNullOrEmpty(employeeCode) || a.EmployeeCode.Equals(employeeCode))
                && (string.IsNullOrEmpty(webOrderNumber) || a.PartsPurchaseOrder.WebOrderNumber.Equals(webOrderNumber))
                //&& (string.IsNullOrEmpty(makerOrderNumber) || a.PartsPurchaseOrder.MakerOrderNumber.Equals(makerOrderNumber))
                //&& (string.IsNullOrEmpty(invoiceNo) || a.PartsPurchaseOrder.InvoiceNo.Equals(invoiceNo))
                && (purchasePlanDateFrom==null || (DateTime.Compare(a.PurchasePlanDate ?? DaoConst.SQL_DATETIME_MIN, purchasePlanDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0))
                && (purchasePlanDateTo==null || (DateTime.Compare(a.PurchasePlanDate ?? DaoConst.SQL_DATETIME_MAX,purchasePlanDateTo ?? DaoConst.SQL_DATETIME_MIN)<=0))
                && (purchaseDateFrom==null || (DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MIN,purchaseDateFrom ?? DaoConst.SQL_DATETIME_MAX)>=0))
                && (purchaseDateTo==null || (DateTime.Compare(a.PurchaseDate ?? DaoConst.SQL_DATETIME_MAX,purchaseDateTo ?? DaoConst.SQL_DATETIME_MIN)<=0))
                && (string.IsNullOrEmpty(officeCode) || a.Department.OfficeCode.Equals(officeCode))
                && (string.IsNullOrEmpty(slipnumber) || a.PartsPurchaseOrder.ServiceSlipNumber.Contains(slipnumber))
                && (string.IsNullOrEmpty(supplierCode) || a.SupplierCode.Equals(supplierCode))
                && (string.IsNullOrEmpty(departmentCode) || a.DepartmentCode.Equals(departmentCode))
                && (string.IsNullOrEmpty(purchaseStatus) || a.PurchaseStatus.Equals(purchaseStatus))
                && a.DelFlag.Equals("0")
                && (string.IsNullOrEmpty(customerName) || (
                    from b in db.ServiceSalesHeader
                    // Edit 2014/05/19 arc ookubo vs2012対応
                    //  where b.Customer.CustomerName.Contains(a.CustomerName)
                    where b.Customer.CustomerName.Contains(customerName)
                    select b.SlipNumber).Contains(a.PartsPurchaseOrder.ServiceSlipNumber))
                select a;

            ParameterExpression param = Expression.Parameter(typeof(PartsPurchase), "x");
            Expression depExpression = condition.CreateExpressionForDepartment(param, new string[] { "DepartmentCode" });
            if (depExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchase, bool>>(depExpression, param));
            }
            Expression offExpression = condition.CreateExpressionForOffice(param, new string[] { "Department", "OfficeCode" });
            if (offExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchase, bool>>(offExpression, param));
            }
            Expression comExpression = condition.CreateExpressionForCompany(param, new string[] { "Department", "Office", "CompanyCode" });
            if (comExpression != null) {
                query = query.Where(Expression.Lambda<Func<PartsPurchase, bool>>(comExpression, param));
            } 
            
            return query;
            
        }

        /// <summary>
        /// 発注番号から仕入データを取得する
        /// </summary>
        /// <param name="purchaseOrderNumber">発注番号</param>
        /// <returns>仕入データ</returns>
        public PartsPurchase GetByPurchaseOrderNumber(string purchaseOrderNumber) {
            var query =
                (from a in db.PartsPurchase
                 where a.DelFlag.Equals("0")
                 && a.PurchaseOrderNumber.Equals(purchaseOrderNumber)
                 select a).FirstOrDefault();
            return query;
        }

        /// <summary>
        /// 部品仕入データを検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        ///  2018/03/26 arc yano #3863 部品入荷　LinkEntry取込日の追加
        ///  2015/11/16 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善)　新規作成
        /// </history>
        public PaginatedList<GetPartsPurchase_Result> GetPurchaseListByCondition(PartsPurchaseSearchCondition condition, int? pageIndex, int? pageSize)
        {
            System.Data.Linq.ISingleResult<GetPartsPurchase_Result> Ret;

            if (condition.PurchaseStatus.ToString().Equals("001"))
            {
                //未入荷の場合の検索
                Ret = db.GetPartsPurchase_NotInStock(condition.PurchaseNumberFrom,
                                                         condition.PurchaseNumberTo,
                                                         condition.PurchaseOrderNumberFrom,
                                                         condition.PurchaseOrderNumberTo,
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseOrderDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseOrderDateTo),
                                                         condition.SlipNumberFrom,
                                                         condition.SlipNumberTo,
                                                         condition.OrderType,
                                                         condition.CustomerCode,
                                                         condition.PartsNumber,
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchasePlanDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchasePlanDateTo),
                                                         condition.DepartmentCode,
                                                         condition.EmployeeCode,
                                                         condition.SupplierCode,
                                                         condition.WebOrderNumber,
                                                         condition.MakerOrderNumber,
                                                         condition.InvoiceNo,
                                                         string.Format("{0:yyyy/MM/dd}", condition.LinkEntryCaptureDateFrom),       //Add 2018/03/26 arc yano #3863
                                                         string.Format("{0:yyyy/MM/dd}", condition.LinkEntryCaptureDateTo)          //Add 2018/03/26 arc yano #3863
                                                         );
            }
            else
            {
                //入荷済みの場合の検索
                Ret = db.GetPartsPurchase_InStock(condition.PurchaseNumberFrom,
                                                         condition.PurchaseNumberTo,
                                                         condition.PurchaseOrderNumberFrom,
                                                         condition.PurchaseOrderNumberTo,
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseOrderDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseOrderDateTo),
                                                         condition.SlipNumberFrom,
                                                         condition.SlipNumberTo,
                                                         condition.PurchaseType,
                                                         condition.OrderType,
                                                         condition.CustomerCode,
                                                         condition.PartsNumber,
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchasePlanDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchasePlanDateTo),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseDateFrom),
                                                         string.Format("{0:yyyy/MM/dd}", condition.PurchaseDateTo),
                                                         condition.DepartmentCode,
                                                         condition.EmployeeCode,
                                                         condition.SupplierCode,
                                                         condition.WebOrderNumber,
                                                         condition.MakerOrderNumber,
                                                         condition.InvoiceNo,
                                                         string.Format("{0:yyyy/MM/dd}", condition.LinkEntryCaptureDateFrom),       //Add 2018/03/26 arc yano #3863
                                                         string.Format("{0:yyyy/MM/dd}", condition.LinkEntryCaptureDateTo)          //Add 2018/03/26 arc yano #3863
                                                         );
            }
            List<GetPartsPurchase_Result> list = new List<GetPartsPurchase_Result>();

            foreach (var ret in Ret)
            {
                GetPartsPurchase_Result PartsPurchaseList = new GetPartsPurchase_Result();

                PartsPurchaseList.PurchaseNumber = ret.PurchaseNumber;
                PartsPurchaseList.PurchaseOrderNumber = ret.PurchaseOrderNumber;
                PartsPurchaseList.PurchaseOrderDate = ret.PurchaseOrderDate;
                PartsPurchaseList.SlipNumber = ret.SlipNumber;
                PartsPurchaseList.PurchaseTypeName = ret.PurchaseTypeName;
                PartsPurchaseList.CustomerName = ret.CustomerName;
                PartsPurchaseList.PartsNumber = ret.PartsNumber;
                PartsPurchaseList.PartsNameJp = ret.PartsNameJp;
                PartsPurchaseList.PurchaseOrderQuantity = ret.PurchaseOrderQuantity;
                PartsPurchaseList.RemainingQuantity = ret.RemainingQuantity;
                PartsPurchaseList.PurchaseQuantity = ret.PurchaseQuantity;
                PartsPurchaseList.PurchasePlanQuantity = ret.PurchasePlanQuantity;
                PartsPurchaseList.OrderTypeName = ret.OrderTypeName;
                PartsPurchaseList.PurchasePlanDate = ret.PurchasePlanDate;
                PartsPurchaseList.PurchaseDate = ret.PurchaseDate;
                PartsPurchaseList.WebOrderNumber = ret.WebOrderNumber;
                PartsPurchaseList.MakerOrderNumber = ret.MakerOrderNumber;
                PartsPurchaseList.InvoiceNo = ret.InvoiceNo;
                PartsPurchaseList.DepartmentName = ret.DepartmentName;
                PartsPurchaseList.EmployeeName = ret.EmployeeName;
                PartsPurchaseList.SupplierName = ret.SupplierName;
                PartsPurchaseList.LinkEntryCaptureDate = ret.LinkEntryCaptureDate;      //Add 2018/03/26 arc yano #3863

                //リスト追加
                list.Add(PartsPurchaseList);
            }

            return new PaginatedList<GetPartsPurchase_Result>(list.AsQueryable<GetPartsPurchase_Result>(), pageIndex ?? 0, pageSize ?? 0);
        }

        //Add 2015/11/16 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善)
        /// <summary>
        /// 部品仕入データを検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/06 arc yano #3808 部品入荷入力　Webオーダー番号入力欄の追加
        /// 2017/08/02 arc yano #3783 部品入荷入力 入荷取消・キャンセル機能の追加 入荷済入荷情報を表示する際は発注部品番号を取得する
        /// 2016/06/20 arc yano #3585 部品入荷一覧　引数追加(PurchaseStatus) 
        /// </history>
        public List<GetPartsPurchaseList_Result> GetPurchaseByCondition(string PurchaseNumber, string PurchaseOrderNumber, string PartsNumber, string DepartmentCode, string PurchaseStatus)
        {

            var Ret = db.GetPartsPurchaseList(PurchaseNumber, PurchaseOrderNumber, PartsNumber, DepartmentCode, PurchaseStatus);

            List<GetPartsPurchaseList_Result> list = new List<GetPartsPurchaseList_Result>();

            foreach (var ret in Ret)
            {
                GetPartsPurchaseList_Result PartsPurchaseList = new GetPartsPurchaseList_Result();

                PartsPurchaseList.PurchaseNumber = ret.PurchaseNumber;
                PartsPurchaseList.PurchaseOrderNumber = ret.PurchaseOrderNumber;
                PartsPurchaseList.InvoiceNo = ret.InvoiceNo;
                PartsPurchaseList.PartsNumber = ret.PartsNumber;
                PartsPurchaseList.PartsNameJp = ret.PartsNameJp;
                PartsPurchaseList.LocationCode = ret.LocationCode;
                PartsPurchaseList.LocationName = ret.LocationName;
                PartsPurchaseList.RemainingQuantity = ret.RemainingQuantity;
                PartsPurchaseList.PurchaseQuantity = ret.PurchaseQuantity;
                PartsPurchaseList.Price = ret.Price;
                PartsPurchaseList.Amount = ret.Amount;
                PartsPurchaseList.MakerOrderNumber = ret.MakerOrderNumber;
                PartsPurchaseList.WebOrderNumber = ret.WebOrderNumber;          //Add 2017/11/06 arc yano #3808
                PartsPurchaseList.ReceiptNumber = ret.ReceiptNumber;
                PartsPurchaseList.SupplierCode = ret.SupplierCode;
                PartsPurchaseList.SupplierName = ret.SupplierName;
                PartsPurchaseList.PurchaseType = ret.PurchaseType;
                PartsPurchaseList.SlipNumber = ret.SlipNumber;
                PartsPurchaseList.Memo = ret.Memo;
                if (!string.IsNullOrEmpty(ret.ChangePartsFlag) && ret.ChangePartsFlag.Equals("1"))
                {
                    PartsPurchaseList.ChangeParts = true;
                }
                else
                {
                    PartsPurchaseList.ChangeParts = false;
                }

                //Add 2017/08/02 arc yano #3783
                if (PurchaseStatus.Equals("002") && !string.IsNullOrWhiteSpace(ret.OrderPartsNumber))   //入荷ステータス＝「入荷済」
                {
                    PartsPurchaseList.ChangePartsNumber = ret.OrderPartsNumber;

                    Parts parts = new PartsDao(db).GetByKey(ret.OrderPartsNumber);

                    PartsPurchaseList.ChangePartsNameJp = (parts != null ? parts.PartsNameJp : "");
                }

                //リスト追加
                list.Add(PartsPurchaseList);
            }

            return new List<GetPartsPurchaseList_Result>(list);

        }

        /// <summary>
        /// 部品仕入データを検索する
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="pageIndex">ページ番号</param>
        /// <param name="pageSize">ページサイズ</param>
        /// <returns></returns>
        /// <history>
        /// 2017/11/06 arc yano #3808 部品入荷入力　Webオーダー番号入力欄の追加
        /// 2015/11/16 arc nakayama #3292_部品入荷一覧(#3234_【大項目】部品仕入れ機能の改善)
        /// </history>
        public List<GetPartsPurchaseList_Result> GetPurchaseByPurchaseOrderNumber(string PurchaseOrderNumber, string DepartmentCode)
        {

            var Ret = db.GetPartsPurchaseListByPurchaseOrderNumber(PurchaseOrderNumber, DepartmentCode);

            List<GetPartsPurchaseList_Result> list = new List<GetPartsPurchaseList_Result>();

            foreach (var ret in Ret)
            {
                GetPartsPurchaseList_Result PartsPurchaseList = new GetPartsPurchaseList_Result();

                PartsPurchaseList.PurchaseNumber = ret.PurchaseNumber;
                PartsPurchaseList.PurchaseOrderNumber = ret.PurchaseOrderNumber;
                PartsPurchaseList.InvoiceNo = ret.InvoiceNo;
                PartsPurchaseList.PartsNumber = ret.PartsNumber;
                PartsPurchaseList.PartsNameJp = ret.PartsNameJp;
                PartsPurchaseList.LocationCode = ret.LocationCode;
                PartsPurchaseList.LocationName = ret.LocationName;
                PartsPurchaseList.RemainingQuantity = ret.RemainingQuantity;
                PartsPurchaseList.PurchaseQuantity = ret.PurchaseQuantity;
                PartsPurchaseList.Price = ret.Price;
                PartsPurchaseList.Amount = ret.Amount;
                PartsPurchaseList.MakerOrderNumber = ret.MakerOrderNumber;
                PartsPurchaseList.WebOrderNumber = ret.WebOrderNumber;          //Add 2017/11/06 arc yano #3808
                PartsPurchaseList.ReceiptNumber = ret.ReceiptNumber;
                PartsPurchaseList.SupplierCode = ret.SupplierCode;
                PartsPurchaseList.SupplierName = ret.SupplierName;
                PartsPurchaseList.PurchaseType = ret.PurchaseType;
                PartsPurchaseList.SlipNumber = ret.SlipNumber;
                PartsPurchaseList.Memo = ret.Memo;
                if (!string.IsNullOrEmpty(ret.ChangePartsFlag) && ret.ChangePartsFlag.Equals("1"))
                {
                    PartsPurchaseList.ChangeParts = true;
                }
                else
                {
                    PartsPurchaseList.ChangeParts = false;
                }

                //リスト追加
                list.Add(PartsPurchaseList);
            }

            return new List<GetPartsPurchaseList_Result>(list);

        }




        //Add 2015/12/01 arc nakayama #3292_部品入荷一覧
        /// <summary>
        /// 部品入荷データを取得する（インボイス番号指定）
        /// </summary>
        /// <param name="purchaseOrderNumber">インボイス番号</param>
        /// <returns>部品入荷データ</returns>
        public PartsPurchase GetByInvoiceNo(string InvoiceNo)
        {
            var query =
                (from a in db.PartsPurchase
                 where a.InvoiceNo.Equals(InvoiceNo)
                 && a.DelFlag.Equals("0")
                 select a).FirstOrDefault();
            return query;
        }

        // Add 2015/12/15 arc nakayama #3294_部品入荷Excel取込確認(#3234_【大項目】部品仕入れ機能の改善)
        /// <summary>
        /// 存在するインボイス番号を重複なしで取得
        /// </summary>
        /// <returns>インボイス番号リスト</returns>
        public List<string> GetInvoiceNoList()
        {
            var query =
                (from a in db.PartsPurchase
                 where a.DelFlag.Equals("0")
                 select a.InvoiceNo).Distinct();

            return query.ToList();
        }
        
        /// <summary>
        /// 部門と部品の純正区分から入荷実績を取得する
        /// </summary>
        /// <param name="departmentCode">部門</param>
        /// <param name="genuineType">純正区分</param>
        /// <returns>入荷実績</returns>
        /// <history>Add 2015/11/09 arc yano #3291 部品仕入機能改善(部品発注入力) サービス伝票経由時の発注画面表示用のアクション追加</history>
        public List<PartsPurchase> GetPurchaseResult(string departmentCode, string genuineType)
        {
           List<PartsPurchase> ret = new List<PartsPurchase>();
            
            var query =
                (from a in db.PartsPurchase
                 where a.DelFlag.Equals("0")
                 && a.DepartmentCode.Equals(departmentCode)
                 && a.Parts.GenuineType.Equals(genuineType)
                 && a.PurchaseStatus.Equals("002")
                 select a);

            if (query != null)
            {
                ret = query.ToList();
            }

            return ret; 
        }
    }
}
