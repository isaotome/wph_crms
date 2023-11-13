using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmsDao {
    public class CarAvailableColorDao {
        private CrmsLinqDataContext db;
        public CarAvailableColorDao(CrmsLinqDataContext context) {
            db = context;
        }
        public CarAvailableColor GetByKey(string carGradeCode, string carColorCode) {
            var query = (from a in db.CarAvailableColor
                         where a.CarGradeCode.Equals(carGradeCode)
                         && a.CarColorCode.Equals(carColorCode)
                         && a.DelFlag.Equals("0")
                         select a).FirstOrDefault();
            return query;
        }
    }
}
