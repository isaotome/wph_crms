using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class V_ServiceReceiptionHistoryDao {
        private CrmsLinqDataContext db;

        public V_ServiceReceiptionHistoryDao(CrmsLinqDataContext context) {
            db = context;
        }

        public List<V_ServiceReceiptionHistory> GetListByCondition(string customerCode, string salesCarNumber) {
            var query =
                from a in db.V_ServiceReceiptionHistory
                where a.CustomerCode.Equals(customerCode)
                && a.SalesCarNumber.Equals(salesCarNumber)
                select a;
            return query.ToList();
        }
    }
}
