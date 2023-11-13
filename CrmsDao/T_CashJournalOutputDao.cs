using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao
{
    //Mod 2015/02/23 arc yano 現金出納帳出力対応 現金出納帳一覧の取得をビューテーブルから、ストアドプロシージャに変更
    public class T_CashJournalOutputDao
  {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public T_CashJournalOutputDao(CrmsLinqDataContext context)
        {
            db = context;
        }
    
        //現金出納帳検索
        public PaginatedList<T_CashJournalOutput> GetJournalOutputByCondition(T_CashJournalOutput T_CashJournalOutputcondition, int? pageIndex, int? pageSize)
        {
            DateTime? TargetDate = T_CashJournalOutputcondition.Lastdate;            
            string OfficeCode = T_CashJournalOutputcondition.OfficeCode;
            string CashAccountCode = T_CashJournalOutputcondition.CashAccountCode;
            string OfficeName = T_CashJournalOutputcondition.OfficeName;
            string CashAccountName = T_CashJournalOutputcondition.CashAccountName;
            
            //Mod 2015/02/23 arc yano 現金出納帳出力対応 リストの取得方法をビューテーブルから、ストアドに変更
            var result = db.GetCashJournalOutput(TargetDate).AsQueryable();

            //Mod 2015/02/20 arc yano 現金出納帳出力対応 事業所コード＞現金口座アカウントコードのソート順バグ
            IOrderedQueryable<T_CashJournalOutput>
                T_CashJournalOutputList =
                from a in result
                where (string.IsNullOrEmpty(OfficeCode) || a.OfficeCode.Equals(OfficeCode))
                &&(string.IsNullOrEmpty(CashAccountCode) || a.CashAccountCode.Equals(CashAccountCode))
                &&(string.IsNullOrEmpty(OfficeName) || a.OfficeName.Equals(OfficeName))
                &&(string.IsNullOrEmpty(CashAccountName) || a.CashAccountName.Equals(CashAccountName))
                //orderby OfficeCode, CashAccountCode
                orderby a.OfficeCode, a.CashAccountCode
                select a;

            List<T_CashJournalOutput> list = new List<T_CashJournalOutput>();

            foreach (var GetJournalData in T_CashJournalOutputList)
            {
                T_CashJournalOutput GetJournalOutputResultList = new T_CashJournalOutput(); 
                GetJournalOutputResultList.OfficeCode = GetJournalData.OfficeCode;
                GetJournalOutputResultList.CashAccountCode = GetJournalData.CashAccountCode;
                GetJournalOutputResultList.OfficeName = GetJournalData.OfficeName;
                GetJournalOutputResultList.CashAccountName =GetJournalData.CashAccountName;
                GetJournalOutputResultList.LastMonthBalance = GetJournalData.LastMonthBalance;
                GetJournalOutputResultList.ThisMonthJournal = GetJournalData.ThisMonthJournal;
                GetJournalOutputResultList.ThisMonthPayment = GetJournalData.ThisMonthPayment;
                GetJournalOutputResultList.ThisMonthBalance = GetJournalData.ThisMonthBalance;
               
                //リスト追加
                list.Add(GetJournalOutputResultList);
            }

            return new PaginatedList<T_CashJournalOutput>(list.AsQueryable<T_CashJournalOutput>(), pageIndex ?? 0, pageSize ?? 0);

        }

        //CSV出力
        public List<T_CashJournalOutput> GetCashJournalOutputByConditionForCSV(DocumentExportCondition T_CashJournalOutputcondition)
        {

            DateTime? TargetDate = T_CashJournalOutputcondition.Lastdate;
            string OfficeCode = T_CashJournalOutputcondition.OfficeCode;
            string CashAccountCode = T_CashJournalOutputcondition.CashAccountCode;
            string OfficeName = T_CashJournalOutputcondition.OfficeName;
            string CashAccountName = T_CashJournalOutputcondition.CashAccountName;

            //Mod 2015/02/23 arc yano 現金出納帳出力対応 リストの取得方法をビューテーブルから、ストアドに変更
            var result = db.GetCashJournalOutput(TargetDate).AsQueryable();
            
            IOrderedQueryable<T_CashJournalOutput>
                T_CashJournalOutput =
                from a in result
                where (string.IsNullOrEmpty(OfficeCode) || a.OfficeCode.Equals(OfficeCode))
                && (string.IsNullOrEmpty(CashAccountCode) || a.CashAccountCode.Equals(CashAccountCode))
                && (string.IsNullOrEmpty(OfficeName) || a.OfficeName.Equals(OfficeName))
                && (string.IsNullOrEmpty(CashAccountName) || a.CashAccountName.Equals(CashAccountName))
                orderby OfficeCode, CashAccountCode
                select a;

            List<T_CashJournalOutput> list = new List<T_CashJournalOutput>();

            foreach (var GetJournalData in T_CashJournalOutput)
            {
                T_CashJournalOutput GetJournalOutputResultList = new T_CashJournalOutput();
                GetJournalOutputResultList.OfficeCode = GetJournalData.OfficeCode;
                GetJournalOutputResultList.CashAccountCode = GetJournalData.CashAccountCode;
                GetJournalOutputResultList.OfficeName = GetJournalData.OfficeName;
                GetJournalOutputResultList.CashAccountName = GetJournalData.CashAccountName;
                GetJournalOutputResultList.StrLastMonthBalance = string.Format("{0:#,0}",GetJournalData.LastMonthBalance);
                GetJournalOutputResultList.StrThisMonthJournal = string.Format("{0:#,0}",GetJournalData.ThisMonthJournal);
                GetJournalOutputResultList.StrThisMonthPayment = string.Format("{0:#,0}",GetJournalData.ThisMonthPayment);
                GetJournalOutputResultList.StrThisMonthBalance = string.Format("{0:#,0}",GetJournalData.ThisMonthBalance);

                //リスト追加
                list.Add(GetJournalOutputResultList);
            }

            return list;

        }




    }
}
