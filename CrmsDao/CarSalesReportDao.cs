using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarSalesReportDao {
        private CrmsLinqDataContext db;
        public CarSalesReportDao(CrmsLinqDataContext context) {
            db = context;
        }
        public List<CarSalesReport> GetBySlipNumber(string slipNumber) {
            var query =
                from a in db.CarSalesReport
                where a.SlipNumber.Equals(slipNumber)
                select a;
            return query.ToList<CarSalesReport>();
        }
    }
}
