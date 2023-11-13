using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao
{
    public class AccountsReceivableCarDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public AccountsReceivableCarDao(CrmsLinqDataContext context)
        {
            db = context;
        }

        /// <summary>
        /// 車両売掛金検索
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="closeStatus">締ステータス</param>
        /// <returns></returns>
        public PaginatedList<AccountsReceivableCar> GetListByCondition(AccountsReceivableCar condition, string closeStatus, int pageIndex, int pageSize)
        {

            int actionFlag = 0; //動作指定は「表示」

            PaginatedList<AccountsReceivableCar> ret = new PaginatedList<AccountsReceivableCar>();

            if (!string.IsNullOrWhiteSpace(closeStatus) && closeStatus.Equals("003"))   //検索対象年月 = 「本締め」の場合はテーブルからそのまま取得
            {
                var stret = db.Get_AccountsReceivableCar(condition.CloseMonth,
                                                       string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                                       string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                                       condition.SlipNumber,
                                                       condition.DepartmentCode,
                                                       condition.CustomerCode,
                                                       condition.Zerovisible).ToList();

                ret = new PaginatedList<AccountsReceivableCar>(stret.AsQueryable(), pageIndex, pageSize);

            }
            else　//検索対象年月≠「本締め」の場合は当日時点までの売掛金検索
            {
                
                var stret = db.Insert_AccountsReceivableCar(condition.CloseMonth,
                                                            actionFlag,
                                                            string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                                            string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                                            condition.SlipNumber,
                                                            condition.DepartmentCode,
                                                            condition.CustomerCode,
                                                            condition.Zerovisible).ToList();

                ret = new PaginatedList<AccountsReceivableCar>(stret.AsQueryable(), pageIndex, pageSize);

            }

            return ret;
        }


        /// <summary>
        /// 車両売掛金検索(Excel出力用)
        /// </summary>
        /// <param name="condition">検索条件</param>
        /// <param name="closeStatus">締ステータス</param>
        /// <returns></returns>
        public List<AccountsReceivableCarExcelResult> GetExcelListByCondition(AccountsReceivableCar condition, string closeStatus)
        {

            int actionFlag = 0; //動作指定は「表示」
            List<AccountsReceivableCarExcelResult> ret = new List<AccountsReceivableCarExcelResult>();

            if (!string.IsNullOrWhiteSpace(closeStatus) && closeStatus.Equals("003"))   //検索対象年月 = 「本締め」の場合はテーブルからそのまま取得
            {
                var stret = db.Get_AccountsReceivableCar(condition.CloseMonth,
                                                       string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                                       string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                                       condition.SlipNumber,
                                                       condition.DepartmentCode,
                                                       condition.CustomerCode,
                                                       condition.Zerovisible);

                foreach (var r in stret)
                {
                    AccountsReceivableCarExcelResult rs = new AccountsReceivableCarExcelResult();
                    rs.SlipNumber = r.SlipNumber;
                    rs.SalesDate = string.Format("{0:yyyy/MM/dd}", r.SalesDate);
                    rs.DepartmentCode = r.DepartmentCode;
                    rs.DepartmentName = r.DepartmentName;
                    rs.CustomerCode = r.CustomerCode;
                    rs.CustomerName = r.CustomerName;
                    rs.CarriedBalance = string.Format("{0:N0}", r.CarriedBalance);
                    rs.PresentMonth = string.Format("{0:N0}", r.PresentMonth);
                    rs.PaymentAmount = string.Format("{0:N0}", r.PaymentAmount);
                    rs.BalanceAmount = string.Format("{0:N0}", r.BalanceAmount);

                    ret.Add(rs);
                }
            }
            else　//検索対象年月≠「本締め」の場合は当日時点までの売掛金検索
            {
                var stret = db.Insert_AccountsReceivableCar(condition.CloseMonth,
                                                            actionFlag,
                                                            string.Format("{0:yyyy/MM/dd}", condition.SalesDateFrom),
                                                            string.Format("{0:yyyy/MM/dd}", condition.SalesDateTo),
                                                            condition.SlipNumber,
                                                            condition.DepartmentCode,
                                                            condition.CustomerCode,
                                                            condition.Zerovisible);

                foreach (var r in stret)
                {
                    AccountsReceivableCarExcelResult rs = new AccountsReceivableCarExcelResult();
                    rs.SlipNumber = r.SlipNumber;
                    rs.SalesDate = string.Format("{0:yyyy/MM/dd}", r.SalesDate);
                    rs.DepartmentCode = r.DepartmentCode;
                    rs.DepartmentName = r.DepartmentName;
                    rs.CustomerCode = r.CustomerCode;
                    rs.CustomerName = r.CustomerName;
                    rs.CarriedBalance = string.Format("{0:N0}", r.CarriedBalance);
                    rs.PresentMonth = string.Format("{0:N0}", r.PresentMonth);
                    rs.PaymentAmount = string.Format("{0:N0}", r.PaymentAmount);
                    rs.BalanceAmount = string.Format("{0:N0}", r.BalanceAmount);

                    ret.Add(rs);
                }
            }

            return ret;
        }


    }
}