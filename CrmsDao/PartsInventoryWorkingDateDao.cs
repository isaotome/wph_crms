using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Data.SqlClient;
namespace CrmsDao
{
    public class PartsInventoryWorkingDateDao
    {
        /// <summary>
        /// データコンテキスト
        /// </summary>
        private CrmsLinqDataContext db;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dataContext">データコンテキスト</param>
        public PartsInventoryWorkingDateDao(CrmsLinqDataContext dataContext)
        {
            db = dataContext;
        }

        public List<PartsInventoryWorkingDate> GetListByCondition(PartsInventoryWorkingDate PartsInventoryWorkingDate)
        {
            var query =
                from a in db.PartsInventoryWorkingDate
                //where (string.IsNullOrEmpty(PartsWipStockCondition.DepartmentCode) || a.DepartmentCode.Equals(PartsWipStockCondition.DepartmentCode))
                select a;

            return query.ToList<PartsInventoryWorkingDate>();
        }

        //部品棚卸作業日全件取得()
        public List<PartsInventoryWorkingDate> GetAll()
        {
            var WorkingDate =
                from a in db.PartsInventoryWorkingDate
                where (a.InventoryWorkingDate != null)
                select a;

            return WorkingDate.ToList<PartsInventoryWorkingDate>();
        }

        //部品棚卸作業日取得()
        public PartsInventoryWorkingDate GetAllVal()
        {
            PartsInventoryWorkingDate WorkingDate =
                (from a in db.PartsInventoryWorkingDate
                where (a.InventoryWorkingDate != null)
                select a).FirstOrDefault();

            return WorkingDate;
        }

    }
}
