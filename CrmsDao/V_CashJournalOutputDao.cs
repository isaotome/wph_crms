using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;

namespace CrmsDao
{
    public class V_CashJournalOutputDao
  {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        public V_CashJournalOutputDao(CrmsLinqDataContext context)
        {
            db = context;
        }
    
        //現金出納帳検索
        public PaginatedList<V_CashJournalOutput> GetJournalOutputByCondition(V_CashJournalOutput V_CashJournalOutputcondition, int? pageIndex, int? pageSize)
        {
            DateTime? TargetDate = V_CashJournalOutputcondition.Lastdate;            
            string OfficeCode = V_CashJournalOutputcondition.OfficeCode;
            string CashAccountCode = V_CashJournalOutputcondition.CashAccountCode;
            string OfficeName = V_CashJournalOutputcondition.OfficeName;
            string CashAccountName = V_CashJournalOutputcondition.CashAccountName;
            
            IOrderedQueryable<V_CashJournalOutput>
                V_CashJournalOutput =
                from a in db.V_CashJournalOutput
                where (TargetDate == null || a.Lastdate.Equals(TargetDate))
                &&(string.IsNullOrEmpty(OfficeCode) || a.OfficeCode.Equals(OfficeCode))
                &&(string.IsNullOrEmpty(CashAccountCode) || a.CashAccountCode.Equals(CashAccountCode))
                &&(string.IsNullOrEmpty(OfficeName) || a.OfficeName.Equals(OfficeName))
                &&(string.IsNullOrEmpty(CashAccountName) || a.CashAccountName.Equals(CashAccountName))
                orderby OfficeCode, CashAccountCode
                select a;

            List<V_CashJournalOutput> list = new List<V_CashJournalOutput>();

            foreach (var GetJournalData in V_CashJournalOutput)
            {
                V_CashJournalOutput GetJournalOutputResultList = new V_CashJournalOutput(); 
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

            return new PaginatedList<V_CashJournalOutput>(list.AsQueryable<V_CashJournalOutput>(), pageIndex ?? 0, pageSize ?? 0);

        }

        //CSV出力
        public List<V_CashJournalOutput> GetCashJournalOutputByConditionForCSV(DocumentExportCondition V_CashJournalOutputcondition)
        {

            DateTime? TargetDate = V_CashJournalOutputcondition.Lastdate;
            string OfficeCode = V_CashJournalOutputcondition.OfficeCode;
            string CashAccountCode = V_CashJournalOutputcondition.CashAccountCode;
            string OfficeName = V_CashJournalOutputcondition.OfficeName;
            string CashAccountName = V_CashJournalOutputcondition.CashAccountName;

            IOrderedQueryable<V_CashJournalOutput>
                V_CashJournalOutput =
                from a in db.V_CashJournalOutput
                where (TargetDate == null || a.Lastdate.Equals(TargetDate))
                && (string.IsNullOrEmpty(OfficeCode) || a.OfficeCode.Equals(OfficeCode))
                && (string.IsNullOrEmpty(CashAccountCode) || a.CashAccountCode.Equals(CashAccountCode))
                && (string.IsNullOrEmpty(OfficeName) || a.OfficeName.Equals(OfficeName))
                && (string.IsNullOrEmpty(CashAccountName) || a.CashAccountName.Equals(CashAccountName))
                orderby OfficeCode, CashAccountCode
                select a;

            List<V_CashJournalOutput> list = new List<V_CashJournalOutput>();

            foreach (var GetJournalData in V_CashJournalOutput)
            {
                V_CashJournalOutput GetJournalOutputResultList = new V_CashJournalOutput();
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
