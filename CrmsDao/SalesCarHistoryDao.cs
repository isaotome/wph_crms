using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class SalesCarHistoryDao {
        private CrmsLinqDataContext db;
        public SalesCarHistoryDao(CrmsLinqDataContext context) {
            db = context;
        }
        public List<SalesCarHistory> GetByVin(string vin) {
            var query = from a in db.SalesCarHistory
                        where a.Vin.Equals(vin)
                        && a.DelFlag.Equals("0")
                        select a;
            return query.ToList();
        }
        public List<SalesCarHistory> GetByKey(string salesCarNumber) {
            var query = from a in db.SalesCarHistory
                        where a.SalesCarNumber.Equals(salesCarNumber)
                        //&& a.DelFlag.Equals("0")
                        select a;
            return query.ToList();
        }
    }
}
