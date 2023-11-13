using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace CrmsDao {
    public class CustomerUpdateLogDao {

        private CrmsLinqDataContext db;

        public CustomerUpdateLogDao(CrmsLinqDataContext context) {
            db = context;
        }

        public List<CustomerUpdateLog> GetListByKey(string customerCode) {
            var query =
                from a in db.CustomerUpdateLog
                where a.CustomerCode.Equals(customerCode)
                select a;
            return query.ToList();
        }
    }
}
