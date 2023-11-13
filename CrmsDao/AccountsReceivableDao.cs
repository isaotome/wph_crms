using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class AccountsReceivableDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public AccountsReceivableDao(CrmsLinqDataContext context)
        {
            db = context;
        }
        /// 売掛金検索検索　
        /// （ページング対応）
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <returns></returns>
        public PaginatedList<AccountsReceivable> GetListByCondition(AccountsReceivable condition, string closeStatus, int pageIndex, int pageSize)
        {
            return new PaginatedList<AccountsReceivable>(GetQueryable(condition, closeStatus), pageIndex, pageSize);
        }

        /// <summary>
        /// 売掛金検索(ページング非対応)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="closeStatus">締ステータス</param>
        /// <returns></returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加
        /// </history>
        public IQueryable<AccountsReceivable> GetQueryable(AccountsReceivable condition, string closeStatus)
        {

            DateTime? salesDateFrom = null;
            DateTime? salesDateTo = null;


            if (!string.IsNullOrWhiteSpace(condition.SalesDateFrom))
            {
                salesDateFrom =  DateTime.Parse(condition.SalesDateFrom);
            }
            if (!string.IsNullOrWhiteSpace(condition.SalesDateTo))
            {
                salesDateTo = DateTime.Parse(condition.SalesDateTo);
            }
        
            int actionFlag = 0;                                                     //動作指定は「表示」

            IQueryable<AccountsReceivable> ret;                                     //戻り値

            List<AccountsReceivable> rec = new List<AccountsReceivable>(); 

            if (!string.IsNullOrWhiteSpace(closeStatus) && closeStatus.Equals("003"))   //検索対象年月 = 「本締め」の場合はテーブルからそのまま取得
            {
                rec  = (from a in db.AccountsReceivable 
                        where a.CloseMonth.Equals(condition.CloseMonth)
                         && (salesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, salesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                         && (salesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, salesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                         //&& (string.IsNullOrWhiteSpace(condition.SlipType) || condition.SlipType.Equals(a.SlipType))
                         && (string.IsNullOrWhiteSpace(condition.SlipNumber) || condition.SlipNumber.Equals(a.SlipNumber))
                         && (string.IsNullOrWhiteSpace(condition.DepartmentCode) || condition.DepartmentCode.Equals(a.DepartmentCode))
                         && (string.IsNullOrWhiteSpace(condition.CustomerCode) || a.CustomerCode.Equals(condition.CustomerCode))
                         // Add 2016/09/26 arc nakayama #3630_【製造】車両売掛金対応 ゼロ表示の検索条件追加
                         && (string.IsNullOrWhiteSpace(condition.Zerovisible) || (condition.Zerovisible.Equals("1")) || !a.BalanceAmount.Equals(0))
                         && (from b in db.ServiceSalesHeader where b.DelFlag.Equals("0") select b.SlipNumber).Contains(a.SlipNumber) //サービス伝票のみ検索
                         //&& (TypeCodeList.AsQueryable().Contains(a.CustomerClaimType))
                         && (string.IsNullOrWhiteSpace(condition.Classification) || (from c in db.c_CustomerClaimType where c.DelFlag.Equals("0") && c.CustomerClaimClass.Equals(condition.Classification) select c.Code).Contains(a.CustomerClaimType))
                        select a).ToList();
            }
            else　//検索対象年月≠「本締め」の場合はストアド実行
            {
                rec = db.Insert_AccountsReceivable(condition.CloseMonth, actionFlag, condition.SalesDateFrom, condition.SalesDateTo, condition.SlipNumber, condition.DepartmentCode, condition.CustomerCode, condition.Zerovisible, condition.Classification).ToList();
            }

            if (rec != null)
            {
                ret = rec.AsQueryable();
            }
            else
            {
                ret = null;
            }

            //Add 2020/05/22 yano  #4032
            //---------------------------
            //集計方法によって条件分離
            //---------------------------
            IEnumerable<AccountsReceivable> alist = null;

            switch (condition.SummaryPattern)
            {
                //請求先毎
                case 0:

                    alist = SummaryCustomerClaim(ret);

                    break;
                //部門毎
                case 1:
                    alist = SummaryDepartment(ret);

                    break;
                //伝票番号＋請求先毎
                default:

                    alist = ret.AsEnumerable();
                    
                    break;
            }

            return alist.AsQueryable();
        }

        /// <summary>
        /// 売掛金の集計(請求先毎)
        /// </summary>
        /// <param name="ret">売掛金データ</param>
        /// <returns>請求先毎に集計された売掛金データ</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private IEnumerable<AccountsReceivable> SummaryCustomerClaim(IQueryable<AccountsReceivable> ret)
        {

            IEnumerable<AccountsReceivable> alist =
                                    
                ret.Select(x => new AccountsReceivable()
                {
                    CloseMonth = x.CloseMonth
                    ,
                    SlipNumber = x.SlipNumber
                    ,
                    CustomerClaimCode = x.CustomerClaimCode
                    ,
                    CustomerClaimName = x.CustomerClaimName
                    ,
                    CustomerClaimType = x.CustomerClaimType
                    ,
                    CustomerClaimTypeName = x.CustomerClaimTypeName
                    ,
                    DepartmentCode = x.DepartmentCode
                    ,
                    DepartmentName = x.DepartmentName
                    ,
                    CustomerCode = x.CustomerCode
                    ,
                    CustomerName = x.CustomerName
                    ,
                    SalesDate = x.SalesDate
                    ,
                    CarriedBalance = x.CarriedBalance
                    ,
                    PresentMonth = x.PresentMonth
                    ,
                    Expendes = x.Expendes
                    ,
                    TotalAmount = x.TotalAmount
                    ,
                    Payment = x.Payment
                    ,
                    BalanceAmount = x.BalanceAmount
                    ,
                    CreateEmployeeCode = x.CreateEmployeeCode
                    ,
                    CreateDate = x.CreateDate
                    ,
                    LastUpdateEmployeeCode = x.LastUpdateEmployeeCode
                    ,
                    LastUpdateDate = x.LastUpdateDate
                    ,
                    DelFlag = x.DelFlag
                    ,
                    ChargesPayment = x.ChargesPayment
                }
                ).Union(
                ret.GroupBy(
                        x => new
                        {
                            x.CloseMonth
                            ,
                            x.CustomerClaimCode
                            ,
                            x.CustomerClaimName
                            ,
                            x.CustomerClaimType
                            ,
                            x.CustomerClaimTypeName
                        }
                        ).Select(
                            x => new AccountsReceivable()
                            {
                                CloseMonth = x.Key.CloseMonth
                                ,
                                SlipNumber = "合計"
                                ,
                                CustomerClaimCode = x.Key.CustomerClaimCode
                                ,
                                CustomerClaimName = x.Key.CustomerClaimName
                                ,
                                CustomerClaimType = x.Key.CustomerClaimType
                                ,
                                CustomerClaimTypeName = x.Key.CustomerClaimTypeName
                                ,
                                DepartmentCode = ""
                                ,
                                DepartmentName = ""
                                ,
                                CustomerCode = ""
                                ,
                                CustomerName = ""
                                ,
                                SalesDate = (DateTime?)null
                                ,
                                CarriedBalance = x.Sum(y => y.CarriedBalance)
                                ,
                                PresentMonth = x.Sum(y => y.PresentMonth)
                                ,
                                Expendes = x.Sum(y => y.Expendes)
                                ,
                                TotalAmount = x.Sum(y => y.TotalAmount)
                                ,
                                Payment = x.Sum(y => y.Payment)
                                ,
                                BalanceAmount = x.Sum(y => y.BalanceAmount)
                                ,
                                CreateEmployeeCode = ""
                                ,
                                CreateDate = (DateTime?)null
                                ,
                                LastUpdateEmployeeCode = ""
                                ,
                                LastUpdateDate = (DateTime?)null
                                ,
                                DelFlag = "0"
                                ,
                                ChargesPayment = x.Sum(y => y.ChargesPayment)
                            }
                        )).OrderBy(x => x.CustomerClaimCode).ThenBy(x => x.SlipNumber);

            return alist;
        }

        // <summary>
        /// 売掛金の集計(部門毎)
        /// </summary>
        /// <param name="ret">売掛金データ</param>
        /// <returns>部門毎に集計された売掛金データ</returns>
        /// <history>
        /// 2020/05/22 yano #4032【サービス売掛金】請求先毎、部門毎の集計の追加 新規追加
        /// </history>
        private IEnumerable<AccountsReceivable> SummaryDepartment(IQueryable<AccountsReceivable> ret)
        {
            IEnumerable<AccountsReceivable> alist =

                ret.Select(x => new AccountsReceivable()
                {
                    CloseMonth = x.CloseMonth
                    ,
                    SlipNumber = x.SlipNumber
                    ,
                    CustomerClaimCode = x.CustomerClaimCode
                    ,
                    CustomerClaimName = x.CustomerClaimName
                    ,
                    CustomerClaimType = x.CustomerClaimType
                    ,
                    CustomerClaimTypeName = x.CustomerClaimTypeName
                    ,
                    DepartmentCode = x.DepartmentCode
                    ,
                    DepartmentName = x.DepartmentName
                    ,
                    CustomerCode = x.CustomerCode
                    ,
                    CustomerName = x.CustomerName
                    ,
                    SalesDate = x.SalesDate
                    ,
                    CarriedBalance = x.CarriedBalance
                    ,
                    PresentMonth = x.PresentMonth
                    ,
                    Expendes = x.Expendes
                    ,
                    TotalAmount = x.TotalAmount
                    ,
                    Payment = x.Payment
                    ,
                    BalanceAmount = x.BalanceAmount
                    ,
                    CreateEmployeeCode = x.CreateEmployeeCode
                    ,
                    CreateDate = x.CreateDate
                    ,
                    LastUpdateEmployeeCode = x.LastUpdateEmployeeCode
                    ,
                    LastUpdateDate = x.LastUpdateDate
                    ,
                    DelFlag = x.DelFlag
                    ,
                    ChargesPayment = x.ChargesPayment
                }
                ).Union(
                ret.GroupBy(
                        x => new
                        {
                            x.CloseMonth
                            ,
                            x.DepartmentCode
                            ,
                            x.DepartmentName
                        }
                        ).Select(
                            x => new AccountsReceivable()
                            {
                                CloseMonth = x.Key.CloseMonth
                                ,
                                SlipNumber = ""
                                ,
                                CustomerClaimCode = "合計"
                                ,
                                CustomerClaimName = ""
                                ,
                                CustomerClaimType = ""
                                ,
                                CustomerClaimTypeName = ""
                                ,
                                DepartmentCode = x.Key.DepartmentCode
                                ,
                                DepartmentName = x.Key.DepartmentName
                                ,
                                CustomerCode = ""
                                ,
                                CustomerName = ""
                                ,
                                SalesDate = (DateTime?)null
                                ,
                                CarriedBalance = x.Sum(y => y.CarriedBalance)
                                ,
                                PresentMonth = x.Sum(y => y.PresentMonth)
                                ,
                                Expendes = x.Sum(y => y.Expendes)
                                ,
                                TotalAmount = x.Sum(y => y.TotalAmount)
                                ,
                                Payment = x.Sum(y => y.Payment)
                                ,
                                BalanceAmount = x.Sum(y => y.BalanceAmount)
                                ,
                                CreateEmployeeCode = ""
                                ,
                                CreateDate = (DateTime?)null
                                ,
                                LastUpdateEmployeeCode = ""
                                ,
                                LastUpdateDate = (DateTime?)null
                                ,
                                DelFlag = "0"
                                ,
                                ChargesPayment = x.Sum(y => y.ChargesPayment)
                            }
                        )).OrderBy(x => x.DepartmentCode).ThenBy(x => x.CustomerClaimCode).ThenBy(x => x.SlipNumber);

            return alist;
        }
    }
}
