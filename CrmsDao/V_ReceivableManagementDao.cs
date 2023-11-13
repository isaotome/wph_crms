using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Data.SqlTypes;
namespace CrmsDao
{
    public class V_ReceivableManagementDao
    {

        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public V_ReceivableManagementDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        /*
        public PaginatedList<V_ReceivableManagement> GetListbyCondition(V_ReceivableManagement ReceivableMgmCondition, int? pageIndex, int? pageSize)
        {
            var ReceivableMgmList = 
                from a in db.V_ReceivableManagement
                where (a.InventoryMonth.Date.Equals(ReceivableMgmCondition.InventoryMonth.Date))
                && (ReceivableMgmCondition.SlipType == null || ReceivableMgmCondition.SlipType == "" || a.SlipType.Equals(ReceivableMgmCondition.SlipType))
                && (string.IsNullOrEmpty(ReceivableMgmCondition.SlipNumber) || a.SlipNumber.Contains(ReceivableMgmCondition.SlipNumber))
                && (string.IsNullOrEmpty(ReceivableMgmCondition.DepartmentCode) || a.DepartmentCode.Equals(ReceivableMgmCondition.DepartmentCode))
                && (ReceivableMgmCondition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, ReceivableMgmCondition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (ReceivableMgmCondition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, ReceivableMgmCondition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(ReceivableMgmCondition.CustomerCode) || a.CustomerCode.Equals(ReceivableMgmCondition.CustomerCode))
                orderby a.DepartmentCode, a.SlipNumber
                select a;
                

            return new PaginatedList<V_ReceivableManagement>(ReceivableMgmList, pageIndex ?? 0, pageSize ?? 0);
                
        }
        
        public List<ReceivableManagementExcelResult> GetListbyConditionForCSV(DocumentExportCondition ReceivableMgmCondition)
        {
            var ReceivableMgmList =
                from a in db.V_ReceivableManagement
                where (a.InventoryMonth.Equals(ReceivableMgmCondition.TargetDate))
                && (ReceivableMgmCondition.SlipType == null || ReceivableMgmCondition.SlipType == "" || a.SlipType.Equals(ReceivableMgmCondition.SlipType))
                && (string.IsNullOrEmpty(ReceivableMgmCondition.SlipNumber) || a.SlipNumber.Contains(ReceivableMgmCondition.SlipNumber))
                && (string.IsNullOrEmpty(ReceivableMgmCondition.DepartmentCode) || a.DepartmentCode.Equals(ReceivableMgmCondition.DepartmentCode))
                && (ReceivableMgmCondition.SalesDateFrom == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MIN, ReceivableMgmCondition.SalesDateFrom ?? DaoConst.SQL_DATETIME_MAX) >= 0)
                && (ReceivableMgmCondition.SalesDateTo == null || DateTime.Compare(a.SalesDate ?? DaoConst.SQL_DATETIME_MAX, ReceivableMgmCondition.SalesDateTo ?? DaoConst.SQL_DATETIME_MIN) <= 0)
                && (string.IsNullOrEmpty(ReceivableMgmCondition.CustomerCode) || a.CustomerCode.Equals(ReceivableMgmCondition.CustomerCode))
                orderby a.DepartmentCode, a.SlipNumber
                select a;

            List<ReceivableManagementExcelResult> list = new List<ReceivableManagementExcelResult>();

            foreach( var Ret in ReceivableMgmList){
                ReceivableManagementExcelResult Output = new ReceivableManagementExcelResult();

                //Mod 2015/06/23 arc nakayama 伝票種別の名称をView上でマスタから取得するように変更
                Output.SlipNumber = Ret.SlipNumber; //伝票番号
                Output.SlipTypeName = Ret.SlipTypeName; //伝票種別名
                Output.SalesDate = string.Format("{0:yyyy/MM/dd}",Ret.SalesDate);   //納車年月日
                Output.DepartmentCode = Ret.DepartmentCode; //部門コード
                Output.DepartmentName = Ret.DepartmentName; //部門名称
                Output.CustomerCode = Ret.CustomerCode; //顧客コード
                Output.CustomerName = Ret.CustomerName; //顧客名称
                Output.Code = Ret.Code;                 //請求先区分コード
                Output.Name = Ret.Name;                 //請求先区分
                Output.CustomerClaimCode = Ret.CustomerClaimCode; //請求先コード
                Output.CustomerClaimName = Ret.CustomerClaimName; //請求先名称
                Output.Amount = string.Format("{0:N0}",Ret.Amount);                       //入金予定金額
                Output.MaeAmount = string.Format("{0:N0}",Ret.MaeAmount);                 //前金
                Output.AtoAmount = string.Format("{0:N0}",Ret.AtoAmount);                 //後金
                Output.BalanceAmount = string.Format("{0:N0}",Ret.BalanceAmount);         //残高
                list.Add(Output);

            } 


            return list;

        }
         */
    }
}
